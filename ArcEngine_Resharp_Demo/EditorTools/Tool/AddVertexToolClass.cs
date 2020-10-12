using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using PS.Plot.Sys;
using System;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public class AddVertexToolClass : ICommand, ITool
    {
        private IMap m_Map = null;
        private bool bEnable = true;
        private IHookHelper m_hookHelper = null;
        private IActiveView m_activeView = null;
        private IEngineEditor m_EngineEditor = null;
        private IEngineEditLayers m_EngineEditLayers = null;

        //两点之间的容忍距离
        public const double THE_POINT_TO_POINT_TOLERANCE = 200;

        #region ICommand 成员

        public int Bitmap
        {
            get { return -1; }
        }

        public string Caption
        {
            get { return "添加节点"; }
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
            get { return "添加节点"; }
        }

        public string Name
        {
            get { return "AddVertexTool"; }
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
                MessageBox.Show("请选择要添加节点的要素！", "提示",
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
            get { return "添加节点"; }
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
        }

        public void OnKeyUp(int keyCode, int shift)
        {
        }

        public void OnMouseDown(int button, int shift, int x, int y)
        {
            IPoint m_startPt = m_activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            if (m_EngineEditor == null) return;
            if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
            if (m_EngineEditLayers == null) return;

            AddVertexNode(m_startPt);
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

        private void AddVertexNode(IPoint pPnt)
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
                //解决不带Z值的要素的编辑和Z值为空的要素的编辑问题
                IZAware pZAware = pPoint as IZAware;
                pZAware.ZAware = true;
                pPoint.Z = 0;
                bool pInLine = false;
                ITopologicalOperator pTopoOpt = default(ITopologicalOperator);
                IPolyline pBoundaryLine = default(IPolyline);
                //最小的距离
                double pMindis = 0;
                IProximityOperator pProxOpt = default(IProximityOperator);
                //得到多边形的边界
                if (pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                || pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryEnvelope)
                {
                    //获取边界线
                    pBoundaryLine = EditVertexClass.GetBoundary(pFeaturelayer);
                }
                else if (pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                || pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryLine)
                {
                    pBoundaryLine = pSelFeature.ShapeCopy as IPolyline;
                }
                pTopoOpt = pPoint as ITopologicalOperator;
                IRelationalOperator pRelationalOperator = default(IRelationalOperator);
                pRelationalOperator = pPoint as IRelationalOperator;
                //判断点是否在边界上
                pInLine = pRelationalOperator.Within(pBoundaryLine);
                //如果不在边界上，判断是否小于容忍距离,如果大于容忍距离则退出程序
                if (pInLine == false)
                {
                    pProxOpt = pPoint as IProximityOperator;
                    pMindis = pProxOpt.ReturnDistance(pBoundaryLine);
                    //if (pMindis > THE_POINT_TO_POINT_TOLERANCE)
                    //    return;
                }
                //判断是否增加的点刚好为节点
                IPointCollection pPolylinePointCol = pBoundaryLine as IPointCollection;
                IHitTest pHitTest = default(IHitTest);
                double pHitDis = 0;
                int pSegIndex = 0;
                int pVerIndex = 0;
                IPoint pHitPoint = null;
                bool bRightSide = true;
                pHitTest = pBoundaryLine as IHitTest;
                //增加的点为已有的节点则退出程序
                //if (pHitTest.HitTest(pPoint, THE_POINT_TO_POINT_TOLERANCE * 10,
                //esriGeometryHitPartType.esriGeometryPartVertex, pHitPoint,
                //ref pHitDis, ref pSegIndex, ref pVerIndex, ref bRightSide) == true)
                //{
                //if (pHitDis > THE_POINT_TO_POINT_TOLERANCE) return;
                //}
                EditVertexClass.pHitPnt = pHitPoint;
                //为多边形增加节点
                ISegmentCollection pSegmentCollection = pBoundaryLine as ISegmentCollection;
                IPolyline pSegPolyline = null;
                int pPointIndex = 0;
                ILine pLine = default(ILine);
                double pDis1 = 0;
                double pDis2 = 0;
                IPoint pVerTex = default(IPoint);
                pMindis = 100;
                pProxOpt = pPoint as IProximityOperator;
                for (int i = 0; i <= pSegmentCollection.SegmentCount - 1; i++)
                {
                    //判断选中点是否在这个Segment上
                    pLine = pSegmentCollection.get_Segment(i) as ILine;
                    pDis1 = pProxOpt.ReturnDistance(pLine.FromPoint);
                    pDis2 = pProxOpt.ReturnDistance(pLine.ToPoint);
                    if (Math.Abs(pDis1 + pDis2 - pLine.Length) <= pMindis)
                    {
                        pMindis = Math.Abs(pDis1 + pDis2 - pLine.Length);
                        pVerTex = pLine.ToPoint;
                    }
                }
                //获取选中的几何特征
                pGeo = pSelFeature.Shape;
                //得到索引
                pPointIndex = EditVertexClass.GetVertexIndex(pVerTex, pGeo);
                //如果是首点，则设置为最后一个点
                if (pPointIndex == 0) pPointIndex = pSegmentCollection.SegmentCount;
                IPointCollection pPolygonPointCol = null;
                pPolygonPointCol = pSelFeature.ShapeCopy as IPointCollection;
                pPolygonPointCol.InsertPoints(pPointIndex, 1, ref pPoint);
                m_EngineEditor.StartOperation();
                //拓扑操作
                IPolygon pPolygon = null;
                IPolyline pPlyline = null;
                if (pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                || pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryEnvelope)
                {
                    pPolygon = pPolygonPointCol as IPolygon;
                    pPolygon.Close();
                    pTopoOpt = pPolygon as ITopologicalOperator;
                    pTopoOpt.Simplify();
                    pSelFeature.Shape = pPolygon;
                    pSelFeature.Store();
                }
                else if (pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                || pFeaturelayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryLine)
                {
                    pPlyline = pPolygonPointCol as IPolyline;
                    pTopoOpt = pPlyline as ITopologicalOperator;
                    pTopoOpt.Simplify();
                    pSelFeature.Shape = pPlyline;
                    pSelFeature.Store();
                }
                //停止编辑
                m_EngineEditor.StopOperation("AddVertexTool");
                //显示顶点
                EditVertexClass.ShowAllVertex(pFeaturelayer);
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("添加节点错误", ex.Source, "数据编辑");
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