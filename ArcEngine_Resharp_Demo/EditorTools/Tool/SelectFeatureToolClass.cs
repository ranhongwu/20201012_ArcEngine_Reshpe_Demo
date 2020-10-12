using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using System;
using System.Windows.Forms;
using PS.Plot.Sys;

namespace PS.Plot.Editor
{
    public class SelectFeatureToolClass : ITool, ICommand
    {
        private IMap m_Map = null;
        private bool bEnable = true;
        private IHookHelper m_hookHelper = null;
        private IActiveView m_activeView = null;
        private IEngineEditor m_EngineEditor = null;
        private IEngineEditLayers m_EngineEditLayers = null;

        #region ITool 成员

        public int Cursor
        {
            get { return -1; }
        }

        public bool Deactivate()
        {
            return true;
        }

        public bool OnContextMenu(int x, int y)
        {
            return false;
        }

        public void OnDblClick()
        {
        }

        public void OnKeyDown(int keyCode, int shift)
        {
            if (keyCode == (int)Keys.Delete)
            {
                try
                {
                    ICommand m_delFeatCom = new DelFeatureCommandClass();
                    m_delFeatCom.OnCreate(m_hookHelper.Hook);
                    m_delFeatCom.OnClick();
                }
                catch (Exception ex)
                {
                }
            }
            if (keyCode == (int)Keys.Escape)
            {
                m_hookHelper.FocusMap.ClearSelection();
                m_hookHelper.ActiveView.Refresh();
            }
        }

        public void OnKeyUp(int keyCode, int shift)
        {
        }

        public void OnMouseDown(int button, int shift, int x, int y)
        {
            
        }

        public void OnMouseMove(int button, int shift, int x, int y)
        {
        }

        public void OnMouseUp(int button, int shift, int x, int y)
        {
            if (shift == 0)
            {
                //清除所有选择的内容
                m_Map.ClearSelection();
                m_activeView.Refresh();
            }
            if (button != 1)
            {
                return;
            }
            try
            {
                if (m_EngineEditor == null) return;
                if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
                if (m_EngineEditLayers == null) return;
                //获取目标图层
                IFeatureLayer pFeatLyr = m_EngineEditLayers.TargetLayer;
                IFeatureClass pFeatCls = pFeatLyr.FeatureClass;

                //MapControl axMapControl1 = Control.FromHandle(new IntPtr(this.m_hookHelper.ActiveView.ScreenDisplay.hWnd)) as MapControl;
                IMapControl4 axMapControl1 = GlobalVars.instance.MapControl;
                IEnvelope pEnvelope = axMapControl1.TrackRectangle();
                IGeometry pGeometry = null;
                //当点选的情况时，Envelope为空，此时建立缓冲区
                if (pEnvelope.IsEmpty == true)
                {
                    IPoint point = m_activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                    //定义缓冲区
                    double db = MapManager.ConvertPixelsToMapUnits(m_activeView, 4);
                    ITopologicalOperator pTop;
                    pTop = point as ITopologicalOperator;
                    pGeometry = pTop.Buffer(db);
                }
                else
                    pGeometry = pEnvelope as IGeometry;
                //设置选择过滤条件
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                //不同的图层类型设置不同的过滤条件
                switch (pFeatCls.ShapeType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        //将像素距离转换为地图单位距离
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                        break;

                    case esriGeometryType.esriGeometryPolygon:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        break;

                    case esriGeometryType.esriGeometryPolyline:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        break;
                }
                pSpatialFilter.Geometry = pGeometry;
                pSpatialFilter.GeometryField = pFeatCls.ShapeFieldName;
                IQueryFilter pQueryFilter = pSpatialFilter as IQueryFilter;
                //根据过滤条件进行查询
                IFeatureCursor pFeatCursor = pFeatCls.Search(pQueryFilter, false);
                IFeature pFeature = pFeatCursor.NextFeature();
                IFeatureSelection pFeatureSelection;
                pFeatureSelection = pFeatLyr as IFeatureSelection;

                IEnumFeature pEnumFeature2 = m_Map.FeatureSelection as IEnumFeature;//已经选择的选择集
                int a = pFeatureSelection.SelectionSet.Count;
                pEnumFeature2.Reset();
                IFeature pFeature2 = pEnumFeature2.Next();

                //遍历整个要素类中符合条件的要素
                while (pFeature != null)
                {
                    if (pFeature2 == null)
                    {
                        pFeatureSelection.SelectFeatures(pQueryFilter, esriSelectionResultEnum.esriSelectionResultAdd, false);
                        break;
                    }
                    //遍历当前选择的要素
                    while (pFeature2 != null)
                    {
                        IRelationalOperator re = (IRelationalOperator)pFeature.Shape;
                        if (re.Equals(pFeature2.Shape))
                        {
                            pFeatureSelection.SelectFeatures(pQueryFilter, esriSelectionResultEnum.esriSelectionResultSubtract, false);
                            break;
                        }
                        else
                        {
                            m_Map.SelectFeature(pFeatLyr as ILayer, pFeature);
                        }
                        pFeature2 = pEnumFeature2.Next();
                    }
                    pFeature = pFeatCursor.NextFeature();
                }
                m_activeView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatCursor);
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("选择要素错误", ex.Source, "数据编辑");
            }
        }

        public void Refresh(int hdc)
        {
        }

        #endregion ITool 成员

        #region ICommand 成员

        public int Bitmap
        {
            get { return -1; }
        }

        public string Caption
        {
            get { return "选择要素"; }
        }

        public string Category
        {
            get { return "编辑工具"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return bEnable; }
        }

        public int HelpContextID
        {
            get { return -1; }
        }

        public string HelpFile
        {
            get { return ""; }
        }

        public string Message
        {
            get { return "选择要素"; }
        }

        public string Name
        {
            get { return "SelectFeatureTool"; }
        }

        public void OnClick()
        {
            m_Map = m_hookHelper.FocusMap;
            m_activeView = m_Map as IActiveView;
            m_EngineEditor = MapManager.EngineEditor;
            m_EngineEditLayers = MapManager.EngineEditor as IEngineEditLayers;
            EditVertexClass.ClearResource();
        }

        public void OnCreate(object Hook)
        {
            if (Hook == null) return;
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = Hook;
                if (m_hookHelper.ActiveView == null)
                    m_hookHelper = null;
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                bEnable = false;
            else
                bEnable = true;
        }

        public string Tooltip
        {
            get { return "选择要素"; }
        }

        #endregion ICommand 成员
    }
}