using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using PS.Plot.Sys;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public class DelVertexToolClass : ICommand, ITool
    {
        private IMap m_Map = null;
        private bool bEnable = true;
        private IHookHelper m_hookHelper = null;
        private IActiveView m_activeView = null;
        private IEngineEditor m_EngineEditor = null;
        private IEngineEditLayers m_EngineEditLayers = null;

        #region ICommand 成员

        public int Bitmap
        {
            get { return -1; }
        }

        public string Caption
        {
            get { return "删除节点"; }
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
            get { return "删除节点"; }
        }

        public string Name
        {
            get { return "DelVertexTool"; }
        }

        public void OnClick()
        {
            m_Map = m_hookHelper.FocusMap;
            m_activeView = m_Map as IActiveView;
            m_EngineEditor = MapManager.EngineEditor;
            m_EngineEditLayers = MapManager.EngineEditor as IEngineEditLayers;

            EditVertexClass.m_activeView = m_activeView;
            EditVertexClass.m_Map = m_Map;

            EditVertexClass.ClearResource();

            if (m_EngineEditor == null) return;
            if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
            if (m_EngineEditLayers == null) return;

            IFeatureLayer pFeatLyr = m_EngineEditLayers.TargetLayer;
            if (pFeatLyr == null) return;
            IFeatureCursor pFeatCur = MapManager.GetSelectedFeatures(pFeatLyr);
            if (pFeatCur == null)
            {
                MessageBox.Show("请选择要删除节点的要素！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            EditVertexClass.ShowAllVertex(pFeatLyr);

            ((IActiveViewEvents_Event)m_Map).AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(map_AfterDraw);
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
            get { return "删除节点"; }
        }

        #endregion ICommand 成员

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
                ICommand m_SelTool = new SelectFeatureToolClass();
                m_SelTool.OnCreate(m_hookHelper.Hook);
                m_SelTool.OnClick();
            }
        }

        public void OnKeyUp(int keyCode, int shift)
        {
        }

        public void OnMouseDown(int button, int shift, int x, int y)
        {
            if (m_EngineEditor == null) return;
            if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
            if (m_EngineEditLayers == null) return;

            try
            {
                IPoint m_startPt;
                IMapControl4 mapControl = GlobalVars.instance.MapControl;
                IEnvelope pEnvelope = mapControl.TrackRectangle();
                if (pEnvelope.IsEmpty)
                {
                    m_startPt = m_activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                    DelVertexNode(m_startPt);
                }
                else
                {
                    IFeature pSelFeature = EditVertexClass.GetSelectedFeature(m_EngineEditLayers.TargetLayer);
                    List<IPoint> PointList = new List<IPoint>();
                    PointList = getIntersectPoint(pEnvelope, pSelFeature);
                    foreach (IPoint pPoint in PointList)
                    {
                        DelVertexNode(pPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void OnMouseMove(int button, int shift, int x, int y)
        {
        }

        public void OnMouseUp(int button, int shift, int x, int y)
        {
        }

        public void Refresh(int hdc)
        {
        }

        #endregion ITool 成员

        private List<IPoint> getIntersectPoint(IEnvelope pEnvelope, IFeature pFeature)
        {
            if (pFeature == null) return null;
            IPolygon pPolygon = pFeature.ShapeCopy as IPolygon;
            IPointCollection pPointCollection = pFeature.ShapeCopy as IPointCollection;
            List<IPoint> pPointList = new List<IPoint>();
            for (int i = 0; i < pPointCollection.PointCount; i++)
            {
                if (pPointCollection.Point[i].X > pEnvelope.XMin && pPointCollection.Point[i].X < pEnvelope.XMax &&
                    pPointCollection.Point[i].Y > pEnvelope.YMin && pPointCollection.Point[i].Y < pEnvelope.YMax)
                {
                    pPointList.Add(pPointCollection.Point[i]);
                }
            }
            return pPointList;
        }

        private void DelVertexNode(IPoint pPnt)
        {
            try
            {
                IFeatureLayer pFeaturelayer = m_EngineEditLayers.TargetLayer;
                IActiveView pActiveView = m_activeView;
                IPoint pPoint = pPnt;
                if (pFeaturelayer.FeatureClass == null) return;
                //如果不是面状地物则退出
                if (pFeaturelayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryEnvelope
                    && pFeaturelayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon
                    && pFeaturelayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryLine
                    && pFeaturelayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
                    return;
                IGeometry pGeo = null;
                IFeature pSelFeature = null;
                pSelFeature = EditVertexClass.GetSelectedFeature(pFeaturelayer);
                //是否有选中的几何体
                if (pSelFeature == null)
                {
                    return;
                }
                pGeo = pSelFeature.ShapeCopy;
                double pSrchDis = 0;
                double pHitDis = 0;
                pSrchDis = pActiveView.Extent.Width / 200;
                pPoint.Z = 0;
                int pIndex = 0;
                IElement pElement = null;
                IHitTest pHtTest = null;
                bool pBoolHitTest = false;
                IPoint pPtHit = null;
                IPointCollection pPointCol = null;
                IPolygon pPolygon = null;
                IPolyline pPyline = null;
                bool bRightZSide = true;
                int pInt = 0;
                m_EngineEditor.StartOperation();
                //删除面状要素的节点
                if (pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    || pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryEnvelope)
                {
                    pElement = new PolygonElement();
                    pElement.Geometry = pSelFeature.Shape;
                    IPolygon pPoly = null;
                    pPoly = pElement.Geometry as IPolygon;
                    pHtTest = pPoly as IHitTest;
                    pBoolHitTest = pHtTest.HitTest(pPoint, pSrchDis, esriGeometryHitPartType.esriGeometryPartVertex,
                        pPtHit, ref pHitDis, ref pInt, ref pIndex, ref bRightZSide);
                    if (pBoolHitTest == false)
                        return;
                    EditVertexClass.pHitPnt = pPtHit;
                    pPointCol = pSelFeature.ShapeCopy as IPointCollection;
                    //如果多边形的节点只有3个则不能再删除了
                    if (pPointCol.PointCount <= 4)
                    {
                        MessageBox.Show("多边形的节点至少需要3个!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //顶点删除
                    pPointCol.RemovePoints(pIndex, 1);
                    pPolygon = pPointCol as IPolygon;
                    pPolygon.Close();
                    pSelFeature.Shape = pPolygon;
                    pSelFeature.Store();
                }
                //删除线状要素的节点
                else if (pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    || pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryLine)
                {
                    pElement = new LineElement();
                    pElement.Geometry = pSelFeature.Shape;
                    IPolyline pPolyLine = default(IPolyline);
                    pPolyLine = pElement.Geometry as IPolyline;
                    pHtTest = pPolyLine as IHitTest;
                    pBoolHitTest = pHtTest.HitTest(pPoint, pSrchDis, esriGeometryHitPartType.esriGeometryPartVertex,
                        pPtHit, ref pHitDis, ref pInt, ref pIndex, ref bRightZSide);
                    if (pBoolHitTest == false)
                        return;
                    EditVertexClass.pHitPnt = pPtHit;
                    pPointCol = pSelFeature.ShapeCopy as IPointCollection;
                    //如果Polyline节点只有2个则不能再删除了
                    if (pPointCol.PointCount <= 2)
                    {
                        MessageBox.Show("线的节点至少需要2个!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //顶点删除
                    pPointCol.RemovePoints(pIndex, 1);
                    pPyline = pPointCol as IPolyline;
                    pSelFeature.Shape = pPyline;
                    pSelFeature.Store();
                }
                //与选中点坐标相同的节点都删除
                for (int i = 0; i <= pPointCol.PointCount - 1; i++)
                {
                    if (i > pPointCol.PointCount - 1)
                        break;
                    if (pPointCol.get_Point(i).X == pPoint.X & pPointCol.get_Point(i).Y == pPoint.Y)
                    {
                        pPointCol.RemovePoints(i, 1);
                        i = i - 1;
                    }
                }
                //停止编辑
                m_EngineEditor.StopOperation("DelVertexTool");
                //显示顶点
                EditVertexClass.ShowAllVertex(pFeaturelayer);
                m_activeView.Refresh();
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("删除节点错误", ex.Source, "数据编辑");
                m_activeView.Refresh();
            }
        }

        private void map_AfterDraw(IDisplay display, esriViewDrawPhase drawphase)
        {
            IColor pColor = null;
            IPoint pPoint = null;
            if (EditVertexClass.m_vertexGeoBag != null)
            {
                for (int i = 0; i <= EditVertexClass.m_vertexGeoBag.GeometryCount - 1; i++)
                {
                    pPoint = EditVertexClass.m_vertexGeoBag.get_Geometry(i) as IPoint;
                    if (pPoint == EditVertexClass.pHitPnt)
                    {
                        EditVertexClass.DisplayGraphic(pPoint, pColor, EditVertexClass.m_selPointSym as ISymbol);
                    }
                    if (pPoint.ID == 10)
                    {
                        EditVertexClass.DisplayGraphic(pPoint, pColor, EditVertexClass.m_endPointSym as ISymbol);
                    }
                    else
                    {
                        EditVertexClass.DisplayGraphic(pPoint, pColor, EditVertexClass.m_vertexSym as ISymbol);
                    }
                }
            }
        }
    }
}