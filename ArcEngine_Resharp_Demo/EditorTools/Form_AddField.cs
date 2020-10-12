using ESRI.ArcGIS.Geodatabase;
using System;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public partial class Form_AddField : DevExpress.XtraEditors.XtraForm
    {
        public Form_AddField()
        {
            InitializeComponent();
        }

        //取消
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //确定
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tbxFieldName.Text.Trim() == "")
            {
                MessageBox.Show("请输入字段名称!", "提示");
                return;
            }
            (this.Owner as Form_Attribute).pAddFieldName = tbxFieldName.Text.Trim();
            (this.Owner as Form_Attribute).pAddFieldEsriFieldType = StringToESRIFieldType(cbxType.Text.Trim());
        }

        #region 封装方法

        /// <summary>
        /// 根据文本返回相应的字段类型
        /// </summary>
        /// <param name="fieldType">字段类型的文本</param>
        /// <returns>返回esriFieldType类型的字段类型</returns>
        private esriFieldType StringToESRIFieldType(string fieldType)
        {
            switch (fieldType)
            {
                case "短整型":
                    return esriFieldType.esriFieldTypeSmallInteger;

                case "长整型":
                    return esriFieldType.esriFieldTypeInteger;

                case "浮点型":
                    return esriFieldType.esriFieldTypeSingle;

                case "双精度型":
                    return esriFieldType.esriFieldTypeDouble;

                case "文本型":
                    return esriFieldType.esriFieldTypeString;

                case "日期型":
                    return esriFieldType.esriFieldTypeDate;

                default:
                    return esriFieldType.esriFieldTypeSmallInteger;
            }
        }

        #endregion 封装方法
    }
}