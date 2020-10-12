using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using PS.Plot.Sys;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    /// <summary>
    /// Summary description for CutPolygonTool.
    /// </summary>
    [Guid("52ab1179-57dd-412d-8be6-11a19a9eb353")]
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class CutPolygonTool : BaseTool
    {
        #region COM Registration Function(s)

        [ComRegisterFunction()]
        [ComVisible(false)]
        private static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        private static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code

        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }

        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion ArcGIS Component Category Registrar generated code

        #endregion COM Registration Function(s)

        private IHookHelper m_hookHelper = null;

        private IActiveView m_activeView = null;
        private IMap m_map = null;

        private IEngineEditProperties m_engineEditor = null;

        private IPoint m_activePoint = null;
        private IFeature m_selectedFeature = null;

        private enum ToolPhase { SelectFeature, Cut }

        private ToolPhase m_toolPhase;

        public CutPolygonTool()
        {
            base.m_category = "GISEditor";
            base.m_caption = "裁剪面";
            base.m_message = "裁剪面";
            base.m_toolTip = "裁剪面";
            base.m_name = "CutPolygonTool";
            try
            {
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                {
                    m_hookHelper = null;
                }
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            m_activePoint = new PointClass();
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            m_toolPhase = ToolPhase.SelectFeature;
            m_engineEditor = new EngineEditorClass();
            ILayer layer = m_engineEditor.TargetLayer;
            if (layer == null)
            {
                MessageBox.Show("请先启动编辑！！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        public override bool Deactivate()
        {
            //Release object references.
            m_selectedFeature = null;
            m_activePoint = null;

            return true;
        }

        public override void OnKeyDown(int keyCode, int Shift)
        {
            // If the Escape key is used, throw away the calculated point.
            if (keyCode == (int)Keys.Escape)
            {
                m_toolPhase = ToolPhase.SelectFeature;
                m_map.ClearSelection();
                m_activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_activeView.Extent);
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != (int)Keys.LButton) return;
            m_activeView = m_hookHelper.ActiveView;
            m_map = m_hookHelper.FocusMap;
            if (m_map == null || m_activeView == null) return;
            m_engineEditor = new EngineEditorClass();
            ILayer layer = m_engineEditor.TargetLayer;
            if (layer == null)
            {
                MessageBox.Show("请先启动编辑！！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            m_activePoint = m_activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            switch (m_toolPhase)
            {
                case (ToolPhase.SelectFeature):
                    //base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "arrow_l.cur");
                    GetSelectedFeature();
                    // base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "pen.cur");
                    break;

                case (ToolPhase.Cut):
                    // base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "pen.cur");
                    CutSelectedPolygon();
                    // base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "arrow_l.cur");
                    break;
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add CutPolygonTool.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add CutPolygonTool.OnMouseUp implementation
        }

        #endregion Overridden Class Methods

        private void GetSelectedFeature()
        {
            if (m_activePoint == null) return;
            IPoint mousePoint = m_activePoint;
            ISelectionEnvironment pSelectionEnvironment = new SelectionEnvironmentClass();
            pSelectionEnvironment.PointSelectionMethod = esriSpatialRelEnum.esriSpatialRelWithin;
            m_map.SelectByShape(mousePoint as IGeometry, pSelectionEnvironment, true);
            IEnumFeature SelectedFeatures = m_map.FeatureSelection as IEnumFeature;
            if (SelectedFeatures == null) return;
            SelectedFeatures.Reset();
            m_selectedFeature = SelectedFeatures.Next();
            if (m_selectedFeature != null)
            {
                m_toolPhase = ToolPhase.Cut;
                m_activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_activeView.Extent);
            }
        }

        private void CutSelectedPolygon()
        {
            if (m_selectedFeature == null)
            {
                MessageBox.Show("请先选择要分割的面要素！！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                m_toolPhase = ToolPhase.SelectFeature;
                return;
            }

            //在屏幕上绘制用于分割的线要素
            IScreenDisplay screenDisplay = m_activeView.ScreenDisplay;
            ISimpleLineSymbol sym = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();
            color.Red = 255;
            color.Green = 128;
            color.Blue = 128;
            sym.Color = color;
            sym.Style = esriSimpleLineStyle.esriSLSSolid;
            sym.Width = 2;
            IRubberBand cutBand = new RubberLineClass();
            IGeometry cutGeometry = cutBand.TrackNew(screenDisplay, sym as ISymbol);
            screenDisplay.StartDrawing(screenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
            screenDisplay.SetSymbol(sym as ISymbol);
            screenDisplay.DrawPolyline(cutGeometry);
            screenDisplay.FinishDrawing();

            IFeatureClass featureClass = m_selectedFeature.Class as IFeatureClass;
            IDataset dataset = featureClass as IDataset;
            IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
            if (!(workspaceEdit.IsBeingEdited())) return;

            //分割选择的面要素
            if (cutGeometry.IsEmpty == true) return;
            try
            {
                workspaceEdit.StartEditOperation();
                IFeatureEdit featureEdit = m_selectedFeature as IFeatureEdit;
                //分割产生新的面要素
                ISet newFeaturesSet = featureEdit.Split(cutGeometry);
                //亮闪新要素
                if (newFeaturesSet != null)
                {
                    newFeaturesSet.Reset();
                    for (int featureCount = 0; featureCount < newFeaturesSet.Count; featureCount++)
                    {
                        IFeature newFeature = newFeaturesSet.Next() as IFeature;
                        FlashGeometry(newFeature.Shape, 3, 20);
                    }
                }
                workspaceEdit.StopEditOperation();
            }
            catch (Exception ex)
            {
                workspaceEdit.AbortEditOperation();
                //SysLogHelper.WriteOperationLog("要素分割错误", ex.Source, "数据编辑");
                MessageBox.Show("分割面要素失败！！" + ex.Message, "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            m_activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_activeView.Extent);
            m_toolPhase = ToolPhase.SelectFeature;
        }

        private void FlashGeometry(IGeometry geometry, int flashCount, int interval)
        {
            IScreenDisplay display = m_activeView.ScreenDisplay;
            ISymbol symbol = CreateSimpleSymbol(geometry.GeometryType);
            display.StartDrawing(0, (short)esriScreenCache.esriNoScreenCache);
            display.SetSymbol(symbol);

            for (int i = 0; i < flashCount; i++)
            {
                switch (geometry.GeometryType)
                {
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                        display.DrawPoint(geometry);
                        break;

                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultipoint:
                        display.DrawMultipoint(geometry);
                        break;

                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                        display.DrawPolyline(geometry);
                        break;

                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                        display.DrawPolygon(geometry);
                        break;

                    default:
                        break;
                }
                System.Threading.Thread.Sleep(interval);
            }
            display.FinishDrawing();
        }

        private ISymbol CreateSimpleSymbol(esriGeometryType geometryType)
        {
            ISymbol symbol = null;
            switch (geometryType)
            {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultipoint:
                    ISimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbolClass();
                    markerSymbol.Color = getRGB(255, 128, 128);
                    markerSymbol.Size = 2;
                    symbol = markerSymbol as ISymbol;
                    break;

                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPath:
                    ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                    lineSymbol.Color = getRGB(255, 128, 128);
                    lineSymbol.Width = 4;
                    symbol = lineSymbol as ISymbol;
                    break;

                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryRing:
                    ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = getRGB(255, 128, 128);
                    symbol = fillSymbol as ISymbol;
                    break;

                default:
                    break;
            }
            symbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;

            return symbol;
        }

        public IColor getRGB(int yourRed, int yourGreen, int yourBlue)
        {
            IRgbColor pRGB;
            pRGB = new RgbColorClass();
            pRGB.Red = yourRed;
            pRGB.Green = yourGreen;
            pRGB.Blue = yourBlue;
            pRGB.UseWindowsDithering = true;
            return pRGB;
        }
    }
}