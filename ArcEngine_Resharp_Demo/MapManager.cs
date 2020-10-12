using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;

namespace KYKJ.EditTool.BasicClass
{
    public class MapManager
    {
        #region 初始化

        public MapManager()
        {

        }

        #endregion

        #region 变量定义

        public static Form ToolPlatForm = null;
        private static IEngineEditor _engineEditor;
        public static IEngineEditor EngineEditor
        {
            get { return MapManager._engineEditor; }
            set { MapManager._engineEditor = value; }
        }

        #endregion

        #region 操作函数


        /// <summary>
        /// 获取颜色
        /// </summary>
        /// <param name="intR"></param>
        /// <param name="intG"></param>
        /// <param name="intB"></param>
        /// <returns></returns>
        public static IRgbColor GetRgbColor(int intR, int intG, int intB)
        {
            IRgbColor pRgbColor = null;
            pRgbColor = new RgbColorClass();
            if (intR < 0) pRgbColor.Red = 0;
            else pRgbColor.Red = intR;
            if (intG < 0) pRgbColor.Green = 0;
            else pRgbColor.Green = intG;
            if (intB < 0) pRgbColor.Blue = 0;
            else pRgbColor.Blue = intB;
            return pRgbColor;
        }

        /// <summary>
        /// 计算两点之间X轴方向和Y轴方向上的距离
        /// </summary>
        /// <param name="lastpoint"></param>
        /// <param name="firstpoint"></param>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <returns></returns>
        public static bool CalDistance(IPoint lastpoint, IPoint firstpoint, out double deltaX, out double deltaY)
        {
            deltaX = 0; deltaY = 0;
            if (lastpoint == null || firstpoint == null)
                return false;
            deltaX = lastpoint.X - firstpoint.X;
            deltaY = lastpoint.Y - firstpoint.Y;
            return true;
        }
        /// <summary>
        /// 单位转换
        /// </summary>
        /// <param name="pixelUnits"></param>
        /// <returns></returns>
        public static double ConvertPixelsToMapUnits(IActiveView activeView, double pixelUnits)
        {
            int pixelExtent = activeView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right
                - activeView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;

            double realWorldDisplayExtent = activeView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;

            return pixelUnits * sizeOfOnePixel;
        }

        /// <summary>
        /// 获取选择要素
        /// </summary>
        /// <param name="pFeatLyr"></param>
        /// <returns></returns>
        public static IFeatureCursor GetSelectedFeatures(IFeatureLayer pFeatLyr)
        {
            ICursor pCursor = null;
            IFeatureCursor pFeatCur = null;
            if (pFeatLyr == null) return null;

            IFeatureSelection pFeatSel = pFeatLyr as IFeatureSelection;
            ISelectionSet pSelSet = pFeatSel.SelectionSet;
            if (pSelSet.Count == 0) return null;
            pSelSet.Search(null, false, out pCursor);
            pFeatCur = pCursor as IFeatureCursor;
            return pFeatCur;
        }

        /// <summary>
        /// 获取当前地图文档所有图层集合
        /// </summary>
        /// <param name="pMap"></param>
        /// <returns></returns>
        public static List<ILayer> GetLayers(IMap pMap)
        {
            ILayer plyr = null;
            List<ILayer> pLstLayers = null;
            try
            {
                pLstLayers = new List<ILayer>();
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    if ((pMap.get_Layer(i) is IFeatureLayer)&& !(pMap.get_Layer(i) is GroupLayer))
                    {
                        plyr = pMap.get_Layer(i);
                        pLstLayers.Add(plyr);
                    }
                        if (pMap.get_Layer(i) is GroupLayer)
                        {
                            ICompositeLayer pComLayer = pMap.get_Layer(i) as ICompositeLayer;
                            for (int j = 0; j < pComLayer.Count; j++)
                            {
                                plyr = pComLayer.get_Layer(j);
                            pLstLayers.Add(plyr);
                            }
                        }
                }
            }
            catch (Exception ex)
            { }
            return pLstLayers;
        }

        //获取地图中的所有shp图层
        public static List<IFeatureLayer> getShpLayer(IMap pMap)
        {
            ILayer plyr = null;
            List<IFeatureLayer> pLstLayers = new List<IFeatureLayer>();
            IFeatureLayer pFeatureLayer;
            try
            {
                for(int i = 0; i < pMap.LayerCount; i++)
                {
                    if (!(pMap.Layer[i] is IFeatureLayer)) continue;
                    if(pMap.Layer[i] is GroupLayer)
                    {
                        ICompositeLayer pComLayer = pMap.get_Layer(i) as ICompositeLayer;
                        for(int j = 0; j < pComLayer.Count; j++)
                        {
                            pFeatureLayer = pComLayer.Layer[j] as IFeatureLayer;
                            if (pFeatureLayer.DataSourceType.Contains("Shapefile"))
                                pLstLayers.Add(pFeatureLayer);
                        }
                        continue;
                    }
                    pFeatureLayer = pMap.Layer[i] as IFeatureLayer;
                    if (pFeatureLayer.DataSourceType.Contains("Shapefile"))
                        pLstLayers.Add(pFeatureLayer);
                }
            }
            catch(Exception ex) { }
            return pLstLayers;
        }

        /// <summary>
        /// 根据图层名获取图层
        /// </summary>
        /// <param name="pMap">地图文档</param>
        /// <param name="sLyrName">图层名</param>
        /// <returns></returns>
        public static ILayer GetLayerByName(IMap pMap, string sLyrName)
        {
            ILayer pLyr = null;
            ILayer pLayer = null;
            try
            {
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    pLyr = pMap.get_Layer(i);
                    if (pLyr.Name.ToUpper() == sLyrName.ToUpper())
                    {
                        pLayer = pLyr;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return pLayer;
        }

        #endregion
    }
}
