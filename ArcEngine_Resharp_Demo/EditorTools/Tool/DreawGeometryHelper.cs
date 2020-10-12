using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace PS.Plot.Editor
{
    /// <summary>
    /// 绘制点线面的帮助类
    /// </summary>
    public class DreawGeometryHelper
    {
        /// <summary>
        /// 绘制面要素
        /// </summary>
        /// <param name="view">活动窗体</param>
        /// <param name="symbol">面样式SimpleSymbolHelper</param>
        /// <returns>要素</returns>
        public static IGeometry DreawPolygon(IActiveView view, ISymbol symbol)
        {
            //初始rubberband
            IRubberBand band = new RubberPolygonClass();
            IGeometry newGeo = band.TrackNew(view.ScreenDisplay, symbol);
            if (newGeo == null) return null;
            return newGeo;
        }

        /// <summary>
        /// 绘制线要素
        /// </summary>
        /// <param name="view">活动视图</param>
        /// <param name="symbol">线样式SimpleSymbolHelper</param>
        /// <returns></returns>
        public static IGeometry DreawPolyLine(IActiveView view, ISymbol symbol)
        {
            //初始rubberband
            IRubberBand band = new RubberLineClass();
            IGeometry newGeo = band.TrackNew(view.ScreenDisplay, symbol);
            if (newGeo == null) return null;
            return newGeo;
        }

        /// <summary>
        /// 绘制点要素
        /// </summary>
        /// <param name="view">活动视图</param>
        /// <param name="symbol">点样式SimpleSymbolHelper</param>
        /// <returns></returns>
        public static IGeometry DreawPoint(IActiveView view, ISymbol symbol)
        {
            //初始rubberband
            IRubberBand band = new RubberPointClass();
            IGeometry newGeo = band.TrackNew(view.ScreenDisplay, symbol);
            if (newGeo == null) return null;
            return newGeo;
        }
    }
}