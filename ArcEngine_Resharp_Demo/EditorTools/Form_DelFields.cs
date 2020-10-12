using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;

namespace PS.Plot.Editor
{
    public partial class Form_DelFields : DevExpress.XtraEditors.XtraForm
    {
        public Form_DelFields(IFeatureClass featureClass)
        {
            pFeatureClass = featureClass;
            InitializeComponent();
        }

        #region 定义变量

        private List<string> pAllFieldsList = new List<string>();
        private List<string> pDelFieldsList = new List<string>();
        private IFeatureClass pFeatureClass = null;

        #endregion 定义变量

        //加载窗体
        private void Form_DelFields_Load(object sender, EventArgs e)
        {
            clbxDelFields.Items.Clear();
            pAllFieldsList = get_FieldsString(pFeatureClass);
            foreach (string s in pAllFieldsList)
            {
                if (s.ToUpper() == "FID" || s.ToUpper() == "SHAPE") continue;
                clbxDelFields.Items.Add(s);
            }
        }

        //确定
        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (object s in clbxDelFields.CheckedItems)
            {
                pDelFieldsList.Add(s.ToString());
            }
            (this.Owner as Form_Attribute).pDelFieldsList = pDelFieldsList;
        }

        #region 封装方法

        /// <summary>
        /// 获取待要素类的所有属性字段名
        /// </summary>
        /// <param name="pFeatureClass">待复制要素类</param>
        /// <returns>返回待复制要素类的所有属性字段名</returns>
        public static List<string> get_FieldsString(IFeatureClass pFeatureClass)
        {
            IFields pFields = pFeatureClass.Fields;
            IField pField;
            List<string> s = new List<string>();
            for (int i = 0; i < pFields.FieldCount; i++)
            {
                pField = pFields.Field[i];
                if (pField.Type != esriFieldType.esriFieldTypeGeometry)
                    s.Add(pField.Name);
            }
            return s;
        }

        #endregion 封装方法
    }
}