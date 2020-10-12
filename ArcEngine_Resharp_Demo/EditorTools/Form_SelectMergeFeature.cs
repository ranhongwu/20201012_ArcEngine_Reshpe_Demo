using ESRI.ArcGIS.Geodatabase;
using System;

namespace PS.Plot.Editor
{
    public partial class Form_SelectMergeFeature : DevExpress.XtraEditors.XtraForm
    {
        public Form_SelectMergeFeature(IEnumFeature enumFeature)
        {
            InitializeComponent();
            pEnumFeature = enumFeature;
        }

        #region 定义变量

        private IEnumFeature pEnumFeature = null;
        private IFeature pMergeFeature = null;

        #endregion 定义变量

        //加载窗体
        private void Form_SelectMergeFeature_Load(object sender, EventArgs e)
        {
            IFeature pFeature = null;
            pEnumFeature.Reset();
            while ((pFeature = pEnumFeature.Next()) != null)
            {
                listBoxControl1.Items.Add(pFeature.OID);
            }
        }

        //确定
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBoxControl1.SelectedItem == null)
            {
                return;
            }

            pEnumFeature.Reset();
            while ((pMergeFeature = pEnumFeature.Next()) != null)
            {
                if (pMergeFeature.OID.ToString() == listBoxControl1.SelectedItem.ToString())
                {
                    break;
                }
            }
            MergeFeatures.pMergeFeature = pMergeFeature;
            this.Close();
        }
    }
}