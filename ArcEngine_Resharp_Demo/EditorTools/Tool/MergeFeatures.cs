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
    internal class MergeFeatures : BaseTool
    {
        private IHookHelper m_hookHelper = null;
        private IActiveView m_activeView = null;
        private IMap m_map = null;
        private IFeatureLayer currentLayer = null;
        private IEngineEditProperties m_engineEditor = null;

        public MergeFeatures()
        {
            base.m_caption = "Union要素";
            base.m_message = "Union要素";
            base.m_toolTip = "Union要素";
            base.m_name = "toolUnionFeature";
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
            if (selectedFeatures == null) return;
            SelectSourceFeature(selectedFeatures);
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
            if (m_map.SelectionCount < 2)
                return null;
            ILayer layer = m_engineEditor.TargetLayer;
            if (layer == null)
                return null;
            if (!(layer is IFeatureLayer))
                return null;
            currentLayer = layer as IFeatureLayer;
            if (currentLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                return null;
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
            IFeature feature = null;
            IGeometry geometry = null;
            object missing = Type.Missing;
            selectedFeatures.Reset();
            feature = selectedFeatures.Next();
            if (feature == null) return;
            IFeatureClass featureClass = feature.Class as IFeatureClass;
            IGeometryCollection geometries = new GeometryBagClass();
            IDataset dataset = null;
            while (feature != null)
            {
                geometry = feature.ShapeCopy;
                geometries.AddGeometry(geometry, ref missing, ref missing);
                feature = selectedFeatures.Next();
            }
            ITopologicalOperator2 unionedGeometry = null;
            switch (featureClass.ShapeType)
            {
                case esriGeometryType.esriGeometryMultipoint:
                    unionedGeometry = new MultipointClass(); break;
                case esriGeometryType.esriGeometryPolyline:
                    unionedGeometry = new PolylineClass(); break;
                case esriGeometryType.esriGeometryPolygon:
                    unionedGeometry = new PolygonClass(); break;
                default: break;
            }
            IEnumGeometry enuGeo = geometries as IEnumGeometry;
            unionedGeometry.ConstructUnion(enuGeo);
            ITopologicalOperator2 topo = unionedGeometry as ITopologicalOperator2;
            topo.IsKnownSimple_2 = false; topo.Simplify();
            IFeatureClass targetFeatureClass = currentLayer.FeatureClass;
            dataset = featureClass as IDataset;
            selectedFeatures.Reset();
            //如果没有IWorkspaceEdit则无法进行撤销重做操作
            IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;

            try
            {
                //workspaceEdit.StartEditOperation();
                IFeature unionedFeature = targetFeatureClass.CreateFeature();
                IFields pFields = unionedFeature.Fields;
                IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
                IField pField = null;
                for (int i = 0; i < pFields.FieldCount; i++)
                {
                    pField = pFields.Field[i];
                    if (targetFeatureClass.AreaField != null && targetFeatureClass.LengthField != null)
                    {
                        if (pField.Name != targetFeatureClass.OIDFieldName && pField.Name != targetFeatureClass.ShapeFieldName
                            && pField.Name != targetFeatureClass.AreaField.Name && pField.Name != targetFeatureClass.LengthField.Name)
                        {
                            unionedFeature.set_Value(i, pMergeFeature.Value[i]);
                        }
                    }
                    else
                    {
                        if (pField.Name != targetFeatureClass.OIDFieldName && pField.Name != targetFeatureClass.ShapeFieldName)
                        {
                            unionedFeature.set_Value(i, pMergeFeature.Value[i]);
                        }
                    }
                }

                unionedFeature.Shape = unionedGeometry as IGeometry;
                unionedFeature.Store();
                while ((feature = selectedFeatures.Next()) != null)
                {
                    feature.Delete();
                }
                workspaceEdit.StopEditOperation();
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("要素合并错误", ex.Source, "数据编辑");
                MessageBox.Show(ex.Message);
            }
        }
    }
}