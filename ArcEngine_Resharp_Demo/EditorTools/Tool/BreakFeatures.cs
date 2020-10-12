using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using PS.Plot.Sys;
using System;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    /// <summary>
    /// 合并要素
    /// </summary>
    internal class BreakFeatures : BaseTool
    {
        private IHookHelper m_hookHelper = null;
        private IActiveView m_activeView = null;
        private IMap m_map = null;
        private IFeatureLayer currentLayer = null;
        private IEngineEditProperties m_engineEditor = null;

        public BreakFeatures()
        {
            base.m_caption = "打散要素";
            base.m_message = "打散要素";
            base.m_toolTip = "打散要素";
            base.m_name = "toolBreakFeature";
            //m_engineEditor = new EngineEditorClass();
        }

        public override bool Enabled
        {
            get
            {
                if (m_engineEditor.TargetLayer == null)
                    return false;
                else return true;
            }
        }

        public override void OnCreate(object hook)
        {
            if (hook == null) return;
            m_hookHelper = new HookHelperClass();
            m_hookHelper.Hook = hook;
        }

        public override void OnClick()
        {
            m_engineEditor = new EngineEditorClass();
            ILayer layer = m_engineEditor.TargetLayer;
            m_activeView = m_hookHelper.ActiveView;
            m_map = m_hookHelper.FocusMap;
            IEnumFeature selectedFeatures = GetSelectedFeatures();
            if (selectedFeatures == null)
            {
                MessageBox.Show("请选择需要打散的要素", "提示");
                return;
            }
            //SelectSourceFeature(selectedFeatures);
            UnionFeatures(selectedFeatures, pMergeFeature);

            m_activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography | esriViewDrawPhase.esriViewGeoSelection, null, m_activeView.Extent);
        }

        public static IFeature pMergeFeature = null;

        public IFeature SelectSourceFeature(IEnumFeature enumFeature)
        {
            Form_SelectMergeFeature frmSelectMergeFeature = new Form_SelectMergeFeature(enumFeature);
            frmSelectMergeFeature.ShowDialog();
            if (pMergeFeature == null) return null;
            else
            {
                return pMergeFeature;
            }
        }

        private IEnumFeature GetSelectedFeatures()
        {
            if (m_map.SelectionCount < 1)
                return null;
            ILayer layer = m_engineEditor.TargetLayer;
            if (layer == null)
                return null;
            if (!(layer is IFeatureLayer))
                return null;
            currentLayer = layer as IFeatureLayer;
            //if (currentLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            //    return null;
            IEnumFeature SelectedFeatures = m_map.FeatureSelection as IEnumFeature;
            if (SelectedFeatures == null)
                return null;
            //判断SelectedFeatures是否为相同的几何类型，且是否与m_engineEditor.TargetLayer几何类型相同
            bool sameGeometryType = JudgeGeometryType(SelectedFeatures);
            if (!sameGeometryType)
                return null;
            return SelectedFeatures;
        }

        private bool JudgeGeometryType(IEnumFeature SelectedFeatures)
        {
            SelectedFeatures.Reset();
            IFeature feature = SelectedFeatures.Next();
            if (feature == null) return false;
            esriGeometryType geometryType = feature.ShapeCopy.GeometryType;
            while ((feature = SelectedFeatures.Next()) != null)
            {
                if (geometryType != feature.ShapeCopy.GeometryType)
                { return false; }
            }
            if (geometryType == currentLayer.FeatureClass.ShapeType)
                return true;
            return false;
        }

        //合并要素
        private void UnionFeatures(IEnumFeature selectedFeatures, IFeature pMergeFeature)
        {
            try
            {
                IFeature feature = null;
                object missing = Type.Missing;
                selectedFeatures.Reset();
                feature = selectedFeatures.Next();
                if (feature == null) return;
                IFeatureClass featureClass = feature.Class as IFeatureClass;
                switch (featureClass.ShapeType)
                {
                    case esriGeometryType.esriGeometryPolyline:
                    case esriGeometryType.esriGeometryPolygon:
                        break;
                    default:
                        MessageBox.Show("请选择线或者面要素","提示");
                        return;
                }
                IGeometryCollection geometries = new GeometryBagClass();
                IDataset dataset = featureClass as IDataset;
                IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
                while (feature != null)
                {
                    if(!feature.ShapeCopy.IsEmpty)
                    {
                        IGeometry pGeometry = feature.ShapeCopy;
                        IGeometryCollection pGeometryCollection = pGeometry as IGeometryCollection;
                        int geomCount = pGeometryCollection.GeometryCount;
                        if (geomCount > 1)
                        {
                            for (int k = 0; k < geomCount; k++)
                            {

                                IFeature newFeature = (feature.Class as IFeatureClass).CreateFeature();
                                IFeatureEdit featureEdit = feature as IFeatureEdit;
                                featureEdit.SplitAttributes(newFeature);
                                IGeometry newGeom = pGeometryCollection.Geometry[k];
                                if(feature.ShapeCopy.GeometryType==esriGeometryType.esriGeometryPolygon)
                                {
                                   IGeometryCollection polyGonC=new PolygonClass();
                                    polyGonC.AddGeometry(newGeom as IGeometry);
                                    IGeometry pGeoNew2 = polyGonC as IGeometry;
                                    pGeoNew2.SpatialReference = feature.ShapeCopy.SpatialReference;
                                    int index = feature.Fields.FindField("Shape");
                                    IGeometryDef pGeometryDef = pGeometryDef = feature.Fields.get_Field(index).GeometryDef as IGeometryDef;
                                    if (pGeometryDef.HasZ)
                                    {
                                        IZAware pZAware = (IZAware)pGeoNew2;
                                        pZAware.ZAware = true;
                                        IZ iz1 = (IZ)pGeoNew2;
                                        iz1.SetConstantZ(0);   //将Z值设置为0 
                                    }
                                    else
                                    {
                                        IZAware pZAware = (IZAware)pGeoNew2;
                                        pZAware.ZAware = false;
                                    }
                                    if (pGeometryDef.HasM)
                                    {
                                        IMAware pMAware = (IMAware)pGeoNew2;
                                        pMAware.MAware = true;
                                    }
                                    else
                                    {
                                        IMAware pMAware = (IMAware)pGeoNew2;
                                        pMAware.MAware = false;
                                    }
                                    newFeature.Shape = pGeoNew2;
                                }
                                if(feature.ShapeCopy.GeometryType == esriGeometryType.esriGeometryPolyline)
                                {
                                    IGeometryCollection polyGonC = new PolylineClass();
                                    polyGonC.AddGeometry(newGeom as IGeometry);
                                    IGeometry pGeoNew2 = polyGonC as IGeometry;
                                    pGeoNew2.SpatialReference = feature.ShapeCopy.SpatialReference;
                                    int index = feature.Fields.FindField("Shape");
                                    IGeometryDef pGeometryDef = pGeometryDef = feature.Fields.get_Field(index).GeometryDef as IGeometryDef;
                                    if (pGeometryDef.HasZ)
                                    {
                                        IZAware pZAware = (IZAware)pGeoNew2;
                                        pZAware.ZAware = true;
                                        IZ iz1 = (IZ)pGeoNew2;
                                        iz1.SetConstantZ(0);   //将Z值设置为0 
                                    }
                                    else
                                    {
                                        IZAware pZAware = (IZAware)pGeoNew2;
                                        pZAware.ZAware = false;
                                    }
                                    if (pGeometryDef.HasM)
                                    {
                                        IMAware pMAware = (IMAware)pGeoNew2;
                                        pMAware.MAware = true;
                                    }
                                    else
                                    {
                                        IMAware pMAware = (IMAware)pGeoNew2;
                                        pMAware.MAware = false;
                                    }
                                    newFeature.Shape = pGeoNew2;
                                }
                                newFeature.Store();
                            }
                            feature.Delete();
                        }
                    }
                    feature = selectedFeatures.Next();
                }
                workspaceEdit.StopEditOperation();
                selectedFeatures.Reset();
                //如果没有IWorkspaceEdit则无法进行撤销重做操作



            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("要素合并错误", ex.Source, "数据编辑");
                MessageBox.Show(ex.Message);
            }
        }
    }
}