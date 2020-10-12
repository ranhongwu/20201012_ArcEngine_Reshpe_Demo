using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    internal class EditVertexClass
    {
        public static IActiveView m_activeView = null;
        public static IMap m_Map = null;

        //定义绘图状态
        public static bool m_inUse;

        //显示节点时所用的Symbol
        public static ISimpleMarkerSymbol m_vertexSym;

        //显示终点时所用的Symbol
        public static ISimpleMarkerSymbol m_endPointSym;

        //显示选中节点时所用的Symbol
        public static ISimpleMarkerSymbol m_selPointSym;

        //显示点对象所用的Symbol
        public static ISimpleMarkerSymbol m_markerSym;

        //显示线对象所用的Symbol
        public static ISimpleLineSymbol m_lineSym;

        //显示面对象所用的Symbol
        public static ISimpleFillSymbol m_fillSym;

        //动态画线时所用的Symbol
        public static ISimpleLineSymbol m_tracklineSym;

        //保存选中地物的节点
        public static IGeometryCollection m_vertexGeoBag;

        //记录所有包含该节点的图形
        public static IArray m_featArray;

        public static IPoint pHitPnt = null;

        public static int GetVertexIndex(IPoint pPoint, IGeometry pGeo)
        {
            int functionReturnValue = -2;

            //图形为空,则退出
            IPointCollection pPointCollection = pGeo as IPointCollection;
            if (pPointCollection == null) return functionReturnValue;

            ITopologicalOperator pTopoOpt = pPointCollection as ITopologicalOperator;
            IRelationalOperator pRelationalOperator = pPoint as IRelationalOperator;
            IProximityOperator pProx = pPoint as IProximityOperator;

            bool pIsEqual = false;
            for (int i = 0; i < pPointCollection.PointCount; i++)
            {
                pIsEqual = pRelationalOperator.Equals(pPointCollection.get_Point(i));
                if (pIsEqual)
                {
                    functionReturnValue = i;
                    break;
                }
            }
            return functionReturnValue;
        }

        //找到包含所要移动的节点的图形,在该过程里面对m_FeatArray进行了赋值
        public static void SelectByShapeTop(IFeatureLayer pFeaturelayer, IGeometry pGeo, esriSpatialRelEnum SpatialRel, bool blnShow, esriSelectionResultEnum Method)
        {
            ITopologicalOperator pTopo = null;
            //对于非要素层不能进行选择
            m_inUse = blnShow;
            //用来查找的图形的复制品，避免对图形的修改（SimplyFy会更改结点顺序)
            IGeometry pGeometry = default(IGeometry);
            IClone pClone = pGeo as IClone;
            pGeometry = pClone.Clone() as IGeometry;
            IFeatureSelection pFeatureSelection = pFeaturelayer as IFeatureSelection;
            //判断是否跳出过程
            bool pBlnExit = false;
            //输入对象为空时清空选择集
            if (pGeometry == null) pBlnExit = true;
            //输入图形为空时清空选择集
            if (pGeometry.IsEmpty) pBlnExit = true;
            switch (pGeometry.GeometryType)
            {
                case esriGeometryType.esriGeometryEnvelope:
                    IEnvelope pEnve = null;
                    pEnve = pGeometry as IEnvelope;
                    if (pEnve.Height == 0 | pEnve.Width == 0)
                        pBlnExit = true;
                    break;

                case esriGeometryType.esriGeometryPolygon:
                    IPolygon pPolygon = null;
                    pPolygon = pGeometry as IPolygon;
                    if (pPolygon.Length == 0)
                        pBlnExit = true;
                    break;

                case esriGeometryType.esriGeometryPolyline:
                    IPolyline pPolyLine = null;
                    pPolyLine = pGeometry as IPolyline;
                    if (pPolyLine.Length == 0)
                        pBlnExit = true;
                    break;
            }
            if (pBlnExit == true)
            {
                if (blnShow == true)
                {
                    if (Method == esriSelectionResultEnum.esriSelectionResultNew)
                    {
                        //在每次选择前清空上次的选择集
                        pFeatureSelection.Clear();
                    }
                }
                else
                {
                    if (Method == esriSelectionResultEnum.esriSelectionResultNew)
                    {
                        m_featArray = new ESRI.ArcGIS.esriSystem.Array();
                    }
                }
                return;
            }
            if (pGeometry is ITopologicalOperator)
            {
                pTopo = pGeometry as ITopologicalOperator;
                pTopo.Simplify();
            }
            //构造空间查询条件
            ISpatialFilter pSpatialfilter = null;
            pSpatialfilter = new SpatialFilter();
            pSpatialfilter.Geometry = pGeometry;
            pSpatialfilter.GeometryField = pFeaturelayer.FeatureClass.ShapeFieldName;
            pSpatialfilter.SpatialRel = SpatialRel;
            IFeatureCursor pFeatCursor = null;
            if (blnShow)
            {
                //高亮显示
                pFeatureSelection.SelectFeatures(pSpatialfilter, Method, false);
            }
            else
            {
                //将结果加到FeaureCursor中
                pFeatCursor = pFeaturelayer.Search(pSpatialfilter, false);
                IFeature pFeature = null;
                pFeature = pFeatCursor.NextFeature();
                int i = 0;
                IArray pTempArray = null;
                switch (Method)
                {
                    case esriSelectionResultEnum.esriSelectionResultNew:
                        m_featArray = new ESRI.ArcGIS.esriSystem.Array();
                        while (pFeature != null)
                        {
                            m_featArray.Add(pFeature);
                            pFeature = pFeatCursor.NextFeature();
                        }
                        break;

                    case esriSelectionResultEnum.esriSelectionResultAdd:
                        while (pFeature != null)
                        {
                            for (i = 0; i <= m_featArray.Count - 1; i++)
                            {
                                if (object.ReferenceEquals(m_featArray.get_Element(i), pFeature))
                                    break;
                            }
                            if (i == m_featArray.Count)
                            {
                                m_featArray.Add(pFeature);
                            }
                            pFeature = pFeatCursor.NextFeature();
                        }
                        break;

                    case esriSelectionResultEnum.esriSelectionResultSubtract:
                        while ((pFeature != null))
                        {
                            for (i = 0; i <= m_featArray.Count - 1; i++)
                            {
                                if (object.ReferenceEquals(m_featArray.get_Element(i), pFeature))
                                    break;
                            }
                            if (i < m_featArray.Count)
                            {
                                m_featArray.Remove(i);
                            }
                            pFeature = pFeatCursor.NextFeature();
                        }
                        break;

                    case esriSelectionResultEnum.esriSelectionResultAnd:
                        pTempArray = new ESRI.ArcGIS.esriSystem.Array();
                        while ((pFeature != null))
                        {
                            for (i = 0; i <= m_featArray.Count - 1; i++)
                            {
                                if (object.ReferenceEquals(m_featArray.get_Element(i), pFeature))
                                {
                                    pTempArray.Add(pFeature);
                                    break;
                                }
                            }
                            pFeature = pFeatCursor.NextFeature();
                        }
                        m_featArray = pTempArray;
                        break;

                    case esriSelectionResultEnum.esriSelectionResultXOR:
                        while ((pFeature != null))
                        {
                            for (i = 0; i <= m_featArray.Count - 1; i++)
                            {
                                if (object.ReferenceEquals(m_featArray.get_Element(i), pFeature))
                                    break;
                            }
                            if (i == m_featArray.Count)
                            {
                                m_featArray.Add(pFeature);
                            }
                            else
                            {
                                m_featArray.Remove(i);
                            }
                            pFeature = pFeatCursor.NextFeature();
                        }
                        break;

                    default:
                        m_featArray = new ESRI.ArcGIS.esriSystem.Array();
                        break;
                }
            }
            Marshal.ReleaseComObject(pFeatCursor);
        }

        private static void SymbolInit()
        {
            //设置各种地图符号的颜色
            IRgbColor pFcolor;
            IRgbColor pOcolor;
            IRgbColor pTrackcolor;
            IRgbColor pVcolor;
            IRgbColor pSColor;
            pFcolor = new RgbColor();
            pOcolor = new RgbColor();
            pTrackcolor = new RgbColor();
            pVcolor = new RgbColor();
            pSColor = new RgbColor();
            pFcolor = MapManager.GetRgbColor(255, 0, 0);
            pOcolor = MapManager.GetRgbColor(0, 0, 255);
            pTrackcolor = MapManager.GetRgbColor(0, 255, 255);
            pVcolor = MapManager.GetRgbColor(0, 255, 0);
            pSColor = MapManager.GetRgbColor(0, 0, 0);
            //设置各种地图符号的属性
            m_markerSym = new SimpleMarkerSymbol();
            m_markerSym.Style = esriSimpleMarkerStyle.esriSMSCircle;
            m_markerSym.Color = pFcolor;
            m_markerSym.Outline = true;
            m_markerSym.OutlineColor = pOcolor;
            m_markerSym.OutlineSize = 1;
            m_vertexSym = new SimpleMarkerSymbol();
            m_vertexSym.Style = esriSimpleMarkerStyle.esriSMSSquare;
            m_vertexSym.Color = pVcolor;
            m_vertexSym.Size = 4;
            m_selPointSym = new SimpleMarkerSymbol();
            m_selPointSym.Style = esriSimpleMarkerStyle.esriSMSSquare;
            m_selPointSym.Color = pSColor;
            m_selPointSym.Size = 4;
            m_endPointSym = new SimpleMarkerSymbol();
            m_endPointSym.Style = esriSimpleMarkerStyle.esriSMSSquare;
            m_endPointSym.Color = pFcolor;
            m_endPointSym.Size = 4;
            m_lineSym = new SimpleLineSymbol();
            m_lineSym.Color = pFcolor;
            m_lineSym.Width = 1;
            m_tracklineSym = new SimpleLineSymbol();
            m_tracklineSym.Color = pTrackcolor;
            m_tracklineSym.Width = 1;
            ISimpleLineSymbol pOsym = default(ISimpleLineSymbol);
            pOsym = new SimpleLineSymbol();
            pOsym.Color = pOcolor;
            pOsym.Width = 1;
            m_fillSym = new SimpleFillSymbol();
            m_fillSym.Color = pFcolor;
            m_fillSym.Style = esriSimpleFillStyle.esriSFSVertical;
            m_fillSym.Outline = pOsym;
        }

        public static void DisplayGraphic(IGeometry pGeometry, IColor pColor, ISymbol pSymbol)
        {
            ISimpleMarkerSymbol pMSym = new SimpleMarkerSymbol();
            ISimpleLineSymbol pLSym = new SimpleLineSymbol();
            ISimpleFillSymbol pSFSym = new SimpleFillSymbol();
            if (pColor != null)
            {
                pMSym.Style = esriSimpleMarkerStyle.esriSMSCircle;
                pMSym.Color = pColor;

                pLSym.Color = pColor;
                pSFSym.Color = pColor;
                pSFSym.Style = esriSimpleFillStyle.esriSFSSolid;
            }
            //开始绘制图形
            m_activeView.ScreenDisplay.StartDrawing(m_activeView.ScreenDisplay.hDC, -1);
            switch (pGeometry.GeometryType)
            {
                case esriGeometryType.esriGeometryPoint:
                case esriGeometryType.esriGeometryMultipoint:
                    //绘制点
                    if ((pColor != null))
                    {
                        m_activeView.ScreenDisplay.SetSymbol(pMSym as ISymbol);
                    }
                    else if ((pSymbol != null))
                    {
                        m_activeView.ScreenDisplay.SetSymbol(pSymbol);
                    }
                    else
                    {
                        m_activeView.ScreenDisplay.SetSymbol(m_markerSym as ISymbol);
                    }
                    m_activeView.ScreenDisplay.DrawPoint(pGeometry);
                    break;

                case esriGeometryType.esriGeometryLine:
                case esriGeometryType.esriGeometryPolyline:
                    //绘制线
                    if (pColor != null)
                    {
                        m_activeView.ScreenDisplay.SetSymbol(pLSym as ISymbol);
                    }
                    else if (pSymbol != null)
                    {
                        m_activeView.ScreenDisplay.SetSymbol(pSymbol);
                    }
                    else
                    {
                        m_activeView.ScreenDisplay.SetSymbol(m_lineSym as ISymbol);
                    }
                    m_activeView.ScreenDisplay.DrawPolyline(pGeometry);
                    break;

                case esriGeometryType.esriGeometryPolygon:
                case esriGeometryType.esriGeometryEnvelope:
                    //绘制面
                    if (pColor != null)
                    {
                        m_activeView.ScreenDisplay.SetSymbol(pSFSym as ISymbol);
                    }
                    else if (pSymbol != null)
                    {
                        m_activeView.ScreenDisplay.SetSymbol(pSymbol as ISymbol);
                    }
                    else
                    {
                        m_activeView.ScreenDisplay.SetSymbol(m_fillSym as ISymbol);
                    }
                    m_activeView.ScreenDisplay.DrawPolygon(pGeometry);

                    break;
            }
            //结束绘制
            m_activeView.ScreenDisplay.FinishDrawing();
        }

        public static void ShowAllVertex(IFeatureLayer pFeatLyr)
        {
            m_vertexGeoBag = null;
            if (pFeatLyr == null) return;
            IFeatureCursor pFeatureCursor = MapManager.GetSelectedFeatures(pFeatLyr);
            if (pFeatureCursor == null) return;
            IFeature pTFeature = null;
            //得到要显示节点的地物
            pTFeature = pFeatureCursor.NextFeature();
            if (pTFeature == null) return;
            //只选中一个地物进行节点移动
            m_Map.ClearSelection();
            m_Map.SelectFeature(pFeatLyr as ILayer, pTFeature);
            m_activeView.Refresh();
            //如果为点状地物，不显示节点
            if (pTFeature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                return;
            IArray pFeatureArray = null;
            pFeatureArray = new ESRI.ArcGIS.esriSystem.Array();
            pFeatureArray.Add(pTFeature);
            //绘图符号初始化
            SymbolInit();
            IFeature pFeature = default(IFeature);
            IPointCollection pPointCol = default(IPointCollection);
            IPoint pPoint = default(IPoint);
            int i = 0; int j = 0;
            m_vertexGeoBag = new GeometryBagClass();
            for (i = 0; i <= pFeatureArray.Count - 1; i++)
            {
                pFeature = pFeatureArray.get_Element(i) as IFeature;
                //获取图形边界的点集
                pPointCol = pFeature.ShapeCopy as IPointCollection;
                for (j = 0; j <= pPointCol.PointCount - 1; j++)
                {
                    pPoint = pPointCol.get_Point(j);
                    if (j == 0 | j == pPointCol.PointCount - 1)
                    {
                        //两个端点的ID设为10
                        pPoint.ID = 10;
                    }
                    else
                    {
                        //中间点的ID设为100
                        pPoint.ID = 100;
                    }
                    IColor pColor = null;
                    object obj = Type.Missing;
                    //显示节点
                    if (pPoint == pHitPnt)
                    {
                        DisplayGraphic(pPoint, pColor, m_selPointSym as ISymbol);
                    }
                    if (j == 0 || j == pPointCol.PointCount - 1)
                    {
                        DisplayGraphic(pPoint, pColor, m_endPointSym as ISymbol);
                    }
                    else
                    {
                        DisplayGraphic(pPoint, pColor, m_vertexSym as ISymbol);
                    }

                    m_vertexGeoBag.AddGeometry(pPoint, ref obj, ref obj);
                }
            }
            Marshal.ReleaseComObject(pFeatureCursor);
        }

        public static IFeature GetSelectedFeature(IFeatureLayer pFeatLyr)
        {
            IFeature functionReturnValue = null;
            IFeatureCursor pFeatureCursor = null;
            IFeature pFeature = null;
            //获取选中的地物
            pFeatureCursor = MapManager.GetSelectedFeatures(pFeatLyr);
            if (pFeatureCursor == null)
            {
                functionReturnValue = null;
                return functionReturnValue;
            }
            pFeature = pFeatureCursor.NextFeature();
            functionReturnValue = pFeature;
            //释放游标
            Marshal.ReleaseComObject(pFeatureCursor);

            return functionReturnValue;
        }

        public static IPolyline GetBoundary(IFeatureLayer pFeatLyr)
        {
            IPolyline functionReturnValue = null;
            ITopologicalOperator pTopologicOpr = null;
            IGeometry pGeometry = null;
            IFeature pFeature = null;
            pFeature = GetSelectedFeature(pFeatLyr);
            if (pFeature == null)
            {
                functionReturnValue = null;
                MessageBox.Show("请选择要编辑的多边形!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return functionReturnValue;
            }
            else
            {
                pGeometry = pFeature.Shape;
                pTopologicOpr = pGeometry as ITopologicalOperator;
                //得到边界
                functionReturnValue = pTopologicOpr.Boundary as IPolyline;
            }
            return functionReturnValue;
        }

        //清除节点改变后原位置上的节点痕迹
        public static void ClearResource()
        {
            m_vertexGeoBag = null;
            if (m_activeView != null) m_activeView.Refresh();
        }
    }
}