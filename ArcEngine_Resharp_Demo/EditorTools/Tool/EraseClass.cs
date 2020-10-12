using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Runtime.InteropServices;

namespace PS.Plot.Editor
{
    /// <summary>
    /// </summary>
    [Guid("c7566697-a658-4b15-ac84-feee1900dac4")]
    public class EraseClass
    {
        public IGeometry pEnvelope { get; set; }

        public IFeatureClass pFeatureClass { get; set; }

        public EraseClass()
        { }

        public EraseClass(IGeometry pEnvelope, IFeatureClass pFeatureClass)
        {
            this.pFeatureClass = pFeatureClass;
            this.pEnvelope = pEnvelope;
        }

        public void EraseOper()
        {
            //空间查询
            ISpatialFilter tSF = new SpatialFilterClass();
            tSF.Geometry = pEnvelope;
            tSF.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor tFeatureCursor = pFeatureClass.Search(tSF, false);
            IFeature tFeature = tFeatureCursor.NextFeature();
            while (tFeature != null)
            {
                if (tFeature.Shape.SpatialReference != pEnvelope.SpatialReference)  //sourceGeometry为被裁剪的图形
                    pEnvelope.Project(tFeature.Shape.SpatialReference);
                //此处应保持裁剪与被裁剪图层的空间参考一致，否则容易发生异常
                switch (tFeature.Shape.GeometryType)
                {
                    case esriGeometryType.esriGeometryPolygon:
                        ITopologicalOperator tTope1 = pEnvelope as ITopologicalOperator;
                        tTope1.Simplify();
                        ITopologicalOperator tTope2 = tFeature.ShapeCopy as ITopologicalOperator;
                        tTope2.Simplify();
                        IGeometry tGeoDe = tTope2.Difference((IGeometry)tTope1);

                        if (tFeature.Fields.get_Field(tFeature.Fields.FindField("Shape")).GeometryDef.HasZ)
                        {
                            IZAware zAware = (IZAware)tGeoDe;
                            zAware.ZAware = true;
                            IZ iz = (IZ)tGeoDe;
                            iz.SetConstantZ(0);
                        }

                        tFeature.Shape = tGeoDe;
                        tFeature.Store();
                        tFeature = tFeatureCursor.NextFeature();
                        break;

                    default:
                        break;
                }
                tFeature = tFeatureCursor.NextFeature();
            }
            ReleaseCom(tFeatureCursor);
        }

        private void ReleaseCom(object o)
        {
            if (o != null)
            {
                while (System.Runtime.InteropServices.Marshal.ReleaseComObject(o) > 0)
                { }
            }
        }
    }
}