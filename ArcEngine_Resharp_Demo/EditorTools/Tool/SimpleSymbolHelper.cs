using ESRI.ArcGIS.Display;
using System.Drawing;

namespace PS.Plot.Editor
{
    /// <summary>
    /// 图形样式
    /// </summary>
    public class SimpleSymbolHelper
    {
        /// <summary>
        /// 创建点要素样式
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="size">大小</param>
        /// <param name="style">样式</param>
        /// <returns>ISymbol</returns>
        public static ISymbol CreateSimpleMarkSymbol(Color color, double size = 2, esriSimpleMarkerStyle style = esriSimpleMarkerStyle.esriSMSCross)
        {
            ISimpleMarkerSymbol pSimpleLineSymbol;
            pSimpleLineSymbol = new SimpleMarkerSymbol();
            pSimpleLineSymbol.Color = new RgbColor() { Red = color.R, Green = color.G, Blue = color.B, Transparency = color.A };
            pSimpleLineSymbol.Style = style;//样式
            pSimpleLineSymbol.Size = size;//大小
            return (ISymbol)pSimpleLineSymbol;
        }

        /// <summary>
        /// 创建线要素样式
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="width">宽度</param>
        /// <param name="style">样式</param>
        /// <returns>ISymbol</returns>
        public static ISymbol CreateSimpleLineSymbol(Color color, double width = 2, esriSimpleLineStyle style = esriSimpleLineStyle.esriSLSNull)
        {
            ISimpleLineSymbol pSimpleLineSymbol;
            pSimpleLineSymbol = new SimpleLineSymbol();
            pSimpleLineSymbol.Width = width;
            pSimpleLineSymbol.Color = new RgbColor() { Red = color.R, Green = color.G, Blue = color.B, Transparency = color.A };
            pSimpleLineSymbol.Style = style;
            return (ISymbol)pSimpleLineSymbol;
        }

        /// <summary>
        /// 创建面要素样式
        /// </summary>
        /// <param name="fillColor">填充颜色</param>
        /// <param name="fillStyle">填充样式</param>
        /// <param name="lineSymbol">线样式</param>
        /// <returns>ISymbol</returns>
        public static ISymbol CreateSimpleFillSymbol(Color fillColor, ILineSymbol lineSymbol, esriSimpleFillStyle fillStyle = esriSimpleFillStyle.esriSFSBackwardDiagonal)
        {
            ISimpleFillSymbol pSimpleFillSymbol;
            pSimpleFillSymbol = new SimpleFillSymbol();
            pSimpleFillSymbol.Style = fillStyle;
            pSimpleFillSymbol.Color = new RgbColor() { Red = fillColor.R, Green = fillColor.G, Blue = fillColor.B, Transparency = fillColor.A };
            pSimpleFillSymbol.Outline = lineSymbol;
            return (ISymbol)pSimpleFillSymbol;
        }

        /// <summary>
        /// 创建面要素样式
        /// </summary>
        /// <param name="fillColor">填充颜色</param>
        /// <param name="fillStyle">填充样式</param>
        /// <param name="lineSymbol">线样式</param>
        /// <returns>ISymbol</returns>
        public static ISymbol CreateSimpleFillSymbol(Color fillColor, esriSimpleFillStyle fillStyle = esriSimpleFillStyle.esriSFSBackwardDiagonal)
        {
            ISimpleFillSymbol pSimpleFillSymbol;
            pSimpleFillSymbol = new SimpleFillSymbol();
            pSimpleFillSymbol.Style = fillStyle;
            pSimpleFillSymbol.Color = new RgbColor() { Red = fillColor.R, Green = fillColor.G, Blue = fillColor.B, Transparency = fillColor.A };
            pSimpleFillSymbol.Outline = (ILineSymbol)CreateSimpleLineSymbol(Color.Red, 1.5, esriSimpleLineStyle.esriSLSSolid);
            return (ISymbol)pSimpleFillSymbol;
        }
    }
}