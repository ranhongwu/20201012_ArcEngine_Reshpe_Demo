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
    public class MoveVertexToolClass : ICommand, ITool
    {
        private IMap m_Map = null;
        private bool bEnable = true;
        private IHookHelper m_hookHelper = null;
        private IActiveView m_activeView = null;
        private IEngineEditor m_EngineEditor = null;
        private IEngineEditLayers m_EngineEditLayers = null;
        private IElement m_pHitElem;
        private IDisplayFeedback m_editDispFeed;

        //多边形反馈
        private IPolygonMovePointFeedback m_polyMvPtFeed;

        //线反馈
        private ILineMovePointFeedback m_polylineMvPtFeed;

        //考虑联动时移动的开始点
        private IPoint m_fromPoint;

        //鼠标指针
        private esriControlsMousePointer m_mousePointer;

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
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        public void OnKeyDown(int keyCode, int shift)
        {
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        public void OnKeyUp(int keyCode, int shift)
        {
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        public void OnMouseDown(int button, int shift, int x, int y)
        {
            try
            {
                IPoint m_startPt = m_activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                if (m_EngineEditor == null) return;
                if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
                if (m_EngineEditLayers == null) return;
                EditMouseDown(m_startPt);
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("移动节点错误", ex.Source, "数据编辑");
            }
        }

        public void OnMouseMove(int button, int shift, int x, int y)
        {
            try
            {
                IPoint pPnt = m_activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                EditMouseMove(pPnt);
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("移动节点错误", ex.Source, "数据编辑");
            }
        }

        public void OnMouseUp(int button, int shift, int x, int y)
        {
            try
            {
                IPoint pPnt = m_activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                EditMouseUp(pPnt);
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("移动节点错误", ex.Source, "数据编辑");
            }
        }

        public void Refresh(int hdc)
        {
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        #endregion ITool 成员

        #region ICommand 成员

        public int Bitmap
        {
            get { return -1; }
        }

        public string Caption
        {
            get { return "移动节点"; }
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
            get { return "移动节点"; }
        }

        public string Name
        {
            get { return "MoveVectexTool"; }
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
                MessageBox.Show("请选择要编辑节点要素！", "提示",
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
            get { return "移动节点"; }
        }

        #endregion ICommand 成员

        private void EditMouseDown(IPoint pPnt)
        {
            try
            {
                IPolygon pGeompoly;
                IPolyline pGeomPolyline;
                IHitTest pHtTest;
                IPoint pPtHit;
                double pDblHitDis = 0;
                int pLngPrtIdx = 0;
                int pLngSegIdx = 0;
                Boolean pBoolHitRt = false;
                Boolean pBoolHitTest = false;
                double pDblSrchDis = 0;
                pPnt.Z = 0;
                pPtHit = new PointClass();
                pDblSrchDis = m_activeView.Extent.Width / 200;
                //获取编辑目标图层
                IFeatureLayer pFeatLyr = m_EngineEditLayers.TargetLayer;
                if (pFeatLyr == null) return;
                IFeatureCursor pFeatCur = MapManager.GetSelectedFeatures(pFeatLyr);
                IFeature pTFeature = pFeatCur.NextFeature();
                switch (pTFeature.Shape.GeometryType)
                {
                    //当为单点、点集时直接返回
                    case esriGeometryType.esriGeometryPoint:
                    case esriGeometryType.esriGeometryMultipoint:
                        return;
                    //线要素
                    case esriGeometryType.esriGeometryLine:
                    case esriGeometryType.esriGeometryPolyline:
                        m_pHitElem = new LineElementClass();
                        break;
                    //面要素
                    case esriGeometryType.esriGeometryPolygon:
                    case esriGeometryType.esriGeometryEnvelope:
                        m_pHitElem = new PolygonElementClass();
                        break;
                }
                //获取选中要素的几何对象
                m_pHitElem.Geometry = pTFeature.Shape;
                if (m_pHitElem != null)
                {
                    switch (pTFeature.Shape.GeometryType)
                    {
                        case esriGeometryType.esriGeometryLine:
                        case esriGeometryType.esriGeometryPolyline:
                            pGeomPolyline = m_pHitElem.Geometry as IPolyline;
                            pHtTest = pGeomPolyline as IHitTest;
                            pBoolHitTest = pHtTest.HitTest(pPnt, pDblSrchDis,
                                esriGeometryHitPartType.esriGeometryPartVertex, pPtHit,
                                ref pDblHitDis, ref pLngPrtIdx, ref pLngSegIdx, ref pBoolHitRt);
                            if (pBoolHitTest)
                            {
                                EditVertexClass.pHitPnt = pPtHit;
                                m_editDispFeed = new LineMovePointFeedbackClass();
                                m_editDispFeed.Display = m_activeView.ScreenDisplay;
                                m_polylineMvPtFeed = m_editDispFeed as ILineMovePointFeedback;
                                m_polylineMvPtFeed.Start(pGeomPolyline, pLngSegIdx, pPnt);
                            }

                            break;

                        case esriGeometryType.esriGeometryPolygon:
                        case esriGeometryType.esriGeometryEnvelope:
                            pGeompoly = m_pHitElem.Geometry as IPolygon;
                            pHtTest = pGeompoly as IHitTest;
                            pBoolHitTest = pHtTest.HitTest(pPnt, pDblSrchDis,
                                esriGeometryHitPartType.esriGeometryPartVertex,
                                pPtHit, ref pDblHitDis, ref pLngPrtIdx, ref pLngSegIdx, ref pBoolHitRt);
                            EditVertexClass.pHitPnt = pPtHit;
                            if (pBoolHitTest)
                            {
                                //定义获取到的与传入点的最近所选地物
                                IFeature pFeature;
                                //定义测量距离用于捕获离点最近的地物
                                double pTestDist = 0;
                                //用于求点与地物的距离
                                IProximityOperator pProximity;
                                IGeometry pGeoM;
                                IFeature pTestFeature;
                                //用于最短距离的比较
                                double pTempDist = 0;
                                IFeatureCursor pSelected;
                                ITopologicalOperator pTopoOpt;
                                //捕捉到的要移动的节点
                                IPoint pSnapVertex = default(IPoint);
                                //从所选地物中获得离点最近的那个地物
                                pFeature = null;
                                //用鼠标点进行运算
                                pTopoOpt = pPnt as ITopologicalOperator;
                                pTopoOpt.Simplify();
                                pProximity = pPnt as IProximityOperator;
                                pSelected = MapManager.GetSelectedFeatures(pFeatLyr);
                                pTestFeature = pSelected.NextFeature();
                                pGeoM = pTestFeature.Shape;
                                pTestDist = pProximity.ReturnDistance(pGeoM);
                                pFeature = pTestFeature;
                                //从所选地物中获得离点最近的那个地物
                                while (pTestFeature != null)
                                {
                                    pTestFeature = pSelected.NextFeature();
                                    if (pTestFeature != null)
                                    {
                                        pGeoM = pTestFeature.Shape;
                                        pTempDist = pProximity.ReturnDistance(pGeoM);
                                        if (pTempDist < pTestDist)
                                        {
                                            pTestDist = pTempDist;
                                            pFeature = pTestFeature;
                                        }
                                    }
                                }

                                //检查pSnapPoint是否是所选地物中的某个图斑的一个节点
                                double pDblHDis = 0;
                                int pLngVertexIdx = 0;
                                int pLngSIdx = 0;
                                bool pBoolHRt = false;
                                bool pBlnGet = false;
                                double pDblDistMin = 0;
                                pDblDistMin = pTempDist;
                                if (pDblDistMin == 0)
                                    pDblDistMin = 3;
                                //两倍可以保证一般一次找的到
                                double pDblSearchRadius = pDblDistMin * 2;
                                IHitTest pHT = default(IHitTest);
                                pHT = pFeature.Shape as IHitTest;
                                //如果pSnapPoint不是pTempParcel的节点
                                if (EditVertexClass.GetVertexIndex(pPnt, pFeature.Shape) == -2)
                                {
                                    pSnapVertex = new Point();
                                    while (!pBlnGet)
                                    {
                                        pBlnGet = pHT.HitTest(pPnt, pDblSearchRadius,
                                            esriGeometryHitPartType.esriGeometryPartVertex,
                                            pSnapVertex, ref pDblHDis, ref pLngVertexIdx, ref pLngSIdx, ref pBoolHRt);
                                        pDblSearchRadius = pDblSearchRadius + pDblDistMin;
                                    }
                                    EditVertexClass.pHitPnt = pSnapVertex;
                                    //如果pSnapVertex仍然不是pTempParcel的节点
                                    if (EditVertexClass.GetVertexIndex(pSnapVertex, pFeature.Shape) == -2)
                                    {
                                        MessageBox.Show("所想移动的起点不是所选面的节点", "提示",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return;
                                    }
                                    m_fromPoint = pSnapVertex;
                                }
                                else
                                {
                                    pSnapVertex = pPnt;
                                    m_fromPoint = pSnapVertex;
                                }
                                //找到包含所要移动的节点的图形,在该过程里面对m_FeatArray进行了赋值
                                EditVertexClass.SelectByShapeTop(pFeatLyr, pSnapVertex, esriSpatialRelEnum.esriSpatialRelTouches,
                                    false, esriSelectionResultEnum.esriSelectionResultNew);
                                m_editDispFeed = new PolygonMovePointFeedbackClass();
                                m_editDispFeed.Display = m_activeView.ScreenDisplay;
                                m_polyMvPtFeed = m_editDispFeed as IPolygonMovePointFeedback;
                                m_polyMvPtFeed.Start(pGeompoly, pLngSegIdx, pPnt);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("节点移动错误", ex.Source, "数据编辑");
            }
        }

        private void EditMouseMove(IPoint pPnt)
        {
            if (m_editDispFeed != null)
            {
                m_mousePointer = esriControlsMousePointer.esriPointerSizeWE;
                IZAware pIZAware = null;
                pIZAware = pPnt as IZAware;
                pIZAware.ZAware = true;
                pPnt.Z = 0;
                m_editDispFeed.MoveTo(pPnt);
            }
        }

        private void EditMouseUp(IPoint pPnt)
        {
            IPolygon pPolyResult;
            IPolyline pPolylineResult;
            IFeatureCursor pFeatureCursor;
            IFeature pFeature;
            IFeatureLayer pFeatLyr = m_EngineEditLayers.TargetLayer;
            //检查编辑的地物
            if (m_editDispFeed != null)
            {
                switch (m_pHitElem.Geometry.GeometryType)
                {
                    case esriGeometryType.esriGeometryLine:
                    case esriGeometryType.esriGeometryPolyline:
                        pPolylineResult = m_polylineMvPtFeed.Stop();
                        //作有效性检查
                        if ((pPolylineResult != null))
                        {
                            //更新元素
                            m_pHitElem.Geometry = pPolylineResult;

                            //获取选中的地物
                            pFeatureCursor = MapManager.GetSelectedFeatures(pFeatLyr);
                            if (pFeatureCursor == null) return;
                            pFeature = pFeatureCursor.NextFeature();
                            m_EngineEditor.StartOperation();
                            //更新要素形状
                            pFeature.Shape = pPolylineResult;
                            pFeature.Store();
                            //停止编辑
                            m_EngineEditor.StopOperation("MoveVertex");
                            EditVertexClass.ShowAllVertex(pFeatLyr);
                        }
                        break;

                    case esriGeometryType.esriGeometryPolygon:
                    case esriGeometryType.esriGeometryEnvelope:
                        //得到反馈的结果
                        pPolyResult = m_polyMvPtFeed.Stop();
                        //作有效性检查
                        if (pPolyResult != null)
                        {
                            //更新元素
                            m_pHitElem.Geometry = pPolyResult;
                            //获取选中的地物
                            pFeatureCursor = MapManager.GetSelectedFeatures(pFeatLyr);
                            if (pFeatureCursor == null)
                                return;
                            pFeature = pFeatureCursor.NextFeature();
                            m_EngineEditor.StartOperation();
                            //更新要素形状
                            pFeature.Shape = pPolyResult;
                            pFeature.Store();
                            //停止编辑
                            m_EngineEditor.StopOperation("MoveVertex");
                            if (pFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon
                                || pFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                EditVertexClass.ShowAllVertex(pFeatLyr);
                            }
                        }
                        IFeature pTempParcel;
                        //定义存储多边形要素的局部变量
                        IPoint pDestPoint;
                        //定义节点移动的目标位置
                        pDestPoint = pPnt;
                        IZAware pZAware = pDestPoint as IZAware;
                        pZAware.ZAware = true;
                        pDestPoint.Z = 0;
                        //所有包含该节点的多边形都进行操作
                        for (int i = 0; i <= EditVertexClass.m_featArray.Count - 1; i++)
                        {
                            pTempParcel = EditVertexClass.m_featArray.get_Element(i) as IFeature;
                            //记录节点序号
                            int pIndex = 0;
                            pIndex = EditVertexClass.GetVertexIndex(m_fromPoint, pTempParcel.Shape);
                            if (!(pIndex == -2))
                            {
                                ITopologicalOperator pTopoOpt = default(ITopologicalOperator);
                                IPolygon pPolygon = default(IPolygon);
                                IPointCollection pPolygonPointCol = default(IPointCollection);
                                pPolygonPointCol = pTempParcel.ShapeCopy as IPointCollection;
                                pPolygonPointCol.UpdatePoint(pIndex, pDestPoint);
                                pPolygon = pPolygonPointCol as IPolygon;
                                pPolygon.Close();
                                pTopoOpt = pPolygon as ITopologicalOperator;
                                pTopoOpt.Simplify();
                                pTempParcel.Shape = pPolygon;
                                pTempParcel.Store();
                            }
                        }
                        EditVertexClass.m_featArray.RemoveAll();
                        break;
                }

                //释放内存
                m_polyMvPtFeed = null;
                m_polylineMvPtFeed = null;
                m_editDispFeed = null;
                m_pHitElem = null;
                //刷新地图
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