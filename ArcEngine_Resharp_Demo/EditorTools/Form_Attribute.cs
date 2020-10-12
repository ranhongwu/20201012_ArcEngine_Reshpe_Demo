using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using PS.Plot.Common;
using PS.Plot.Sys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public partial class Form_Attribute : DevExpress.XtraEditors.XtraForm
    {
        public Form_Attribute(IFeatureLayer _featureLayer, IMap map, bool isEditing)
        {
            startEdit = isEditing;
            InitializeComponent();
            featureLayer = _featureLayer;
            this.Text = featureLayer.Name;
            pMap = map;

            ctrlAttrubuteGrid1.InitAttributeGrid(featureLayer, null);
        }

        #region 定义变量

        public DataTable attributeTable;
        private IFeatureLayer featureLayer = null;
        private IMap pMap = null;
        private bool hasEdited = false;
        private bool startEdit;
        private IWorkspace pWorkspace = null;
        private IWorkspaceEdit pWorkspaceEdit = null;
        public string pAddFieldName = "";//要添加的属性字段
        public esriFieldType pAddFieldEsriFieldType;//要添加的属性类型
        public List<string> pDelFieldsList;
        private List<AttributeEditObject> attributeEditObjectsList = new List<AttributeEditObject>();

        #endregion 定义变量

        //窗体加载
        private void FormAttribute_Load(object sender, EventArgs e)
        {
            if (startEdit == true)//已开始编辑
            {
                ctrlAttrubuteGrid1.SetGridEditable(true);
                toolStripStatusLabel1.Text = "属性表编辑中...";
                pWorkspace = (featureLayer.FeatureClass as IDataset).Workspace;
                pWorkspaceEdit = pWorkspace as IWorkspaceEdit;
                setButtonEnable(true);
            }
            else
            {
                ctrlAttrubuteGrid1.SetGridEditable(false);
                toolStripStatusLabel1.Text = "";
                setButtonEnable(false);
            }
        }

        //开始编辑
        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            pWorkspace = (featureLayer.FeatureClass as IDataset).Workspace;
            pWorkspaceEdit = pWorkspace as IWorkspaceEdit;
            startEdit = pWorkspaceEdit.IsBeingEdited();
            if (!startEdit)
            {
                pWorkspaceEdit.StartEditing(true);
            }
            setButtonEnable(true);
            toolStripStatusLabel1.Text = "属性表编辑中...";
            ctrlAttrubuteGrid1.SetGridEditable(true);
        }

        //保存编辑
        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (pWorkspaceEdit == null) return;
            try
            {
                if (attributeEditObjectsList.Count != 0)
                    WriteAttributeToFeatureClass(featureLayer.FeatureClass);
                startEdit = pWorkspaceEdit.IsBeingEdited();
                if (startEdit)
                {
                    pWorkspaceEdit.StopEditing(true);
                }
                setButtonEnable(false);
                ctrlAttrubuteGrid1.SetGridEditable(false);
                toolStripStatusLabel1.Text = "";
                hasEdited = false;
                attributeEditObjectsList = new List<AttributeEditObject>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Attribute), ex, "属性表编辑");
            }
        }

        //添加字段
        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Form_AddField form_AddField = new Form_AddField();
            form_AddField.Owner = this;
            if (form_AddField.ShowDialog() == DialogResult.OK)
            {
                if (AddField(featureLayer.FeatureClass, pAddFieldName, pAddFieldName, pAddFieldEsriFieldType) == 1)
                {
                    MessageBox.Show("字段创建成功!", "提示");
                }
                else
                {
                    MessageBox.Show("字段创建失败!", "提示");
                    return;
                }
                ctrlAttrubuteGrid1.InitAttributeGrid(featureLayer, null);
            }
        }

        //删除字段
        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            pDelFieldsList = new List<string>();
            Form_DelFields form_DelFields = new Form_DelFields(featureLayer.FeatureClass);
            form_DelFields.Owner = this;
            if (form_DelFields.ShowDialog() == DialogResult.OK)
            {
                foreach (string s in pDelFieldsList)
                {
                    if (DeleteField(featureLayer, s) == 1)
                    {
                        continue;
                    }
                    else
                    {
                        MessageBox.Show("删除 " + s + " 属性失败!", "提示");
                        continue;
                    }
                }
                ctrlAttrubuteGrid1.InitAttributeGrid(featureLayer, null);
            }
        }

        //导出表
        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "导出Excel";
            saveFileDialog.Filter = "Excel文件(*.xls)|*.xls";
            DialogResult dialogResult = saveFileDialog.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                DevExpress.XtraPrinting.XlsExportOptions options = new DevExpress.XtraPrinting.XlsExportOptions();
                //MainGridControl.ExportToXls(saveFileDialog.FileName);
                DevExpress.XtraEditors.XtraMessageBox.Show("保存成功！", "提示");
            }
        }

        //关闭窗体
        private void Form_Attribute_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (hasEdited)
            {
                DialogResult dialogResult = MessageBox.Show("是否保存编辑的内容?", "提示", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Yes)
                {
                    barButtonItem3_ItemClick(sender, null);
                }
            }
            Form_Editor.formAttribute = null;
        }

        //导出当前页
        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            saveFileDialog1.Filter = "Excel 2007文档(*.xlsx)|*.xlsx|Excel 2003文档(*.xls)|*.xls" +
                "|csv文件(*.csv)|*.csv|文本文件(*.txt)|*.txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                DataTable exportTable = null;
                string exportPath = saveFileDialog1.FileName;
                string exportFormat = (new System.IO.FileInfo(exportPath)).Extension;
                if (ctrlAttrubuteGrid1.PageTag == 0)
                    exportTable = (DataTable)ctrlAttrubuteGrid1.AllDataGrid.DataSource;
                else
                    exportTable = (DataTable)ctrlAttrubuteGrid1.SelectionDataGrid.DataSource;
                exportTable = exportTable.Copy();
                if(exportTable.Columns.Contains("选中要素"))
                    exportTable.Columns.Remove("选中要素");
                switch (exportFormat)
                {
                    case ".xlsx":
                        CommonAPI.ExportDataTableToXLSX(exportTable, exportPath);
                        break;
                    case ".xls":
                        CommonAPI.ExportDataTableToXLS(exportTable, exportPath);
                        break;
                    case ".csv":
                        CommonAPI.ExportDataTableToCSV(exportTable, exportPath);
                        break;
                    case ".txt":
                        CommonAPI.DataTableExportToTxt(exportPath, exportTable);
                        break;
                }
                XtraMessageBox.Show("导出成功！");
            }
        }

        //导出所有记录
        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            saveFileDialog1.Filter = "Excel 2007文档(*.xlsx)|*.xlsx|Excel 2003文档(*.xls)|*.xls" +
                "|csv文件(*.csv)|*.csv|文本文件(*.txt)|*.txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                DataTable exportTable = null;
                string exportPath = saveFileDialog1.FileName;
                string exportFormat = (new System.IO.FileInfo(exportPath)).Extension;
                if (ctrlAttrubuteGrid1.PageTag == 0)
                    exportTable = ctrlAttrubuteGrid1.AllAttTable.Copy();
                else
                    exportTable = ctrlAttrubuteGrid1.SelectionAttTable.Copy();
                if (exportTable.Columns.Contains("选中要素"))
                    exportTable.Columns.Remove("选中要素");
                switch (exportFormat)
                {
                    case ".xlsx":
                        CommonAPI.ExportDataTableToXLSX(exportTable, exportPath);
                        break;
                    case ".xls":
                        CommonAPI.ExportDataTableToXLS(exportTable, exportPath);
                        break;
                    case ".csv":
                        CommonAPI.ExportDataTableToCSV(exportTable, exportPath);
                        break;
                    case ".txt":
                        CommonAPI.DataTableExportToTxt(exportPath, exportTable);
                        break;
                }
                XtraMessageBox.Show("导出成功！");
            }
        }

        #region 数据表事件
        //改变全部属性表中的值
        private void ctrlAttrubuteGrid1_ViewAllAttributeCellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            try
            {
                AttributeEditObject attributeEditObject = new AttributeEditObject();
                string pFieldName = e.Column.FieldName;
                if (pFieldName == ctrlAttrubuteGrid1.OIDFieldName || pFieldName.ToUpper() == featureLayer.FeatureClass.ShapeFieldName||
                    pFieldName==featureLayer.FeatureClass.AreaField.Name||pFieldName==featureLayer.FeatureClass.LengthField.Name)
                {
                    MessageBox.Show("此属性不允许更改!", "提示");
                    return;
                }
                if (pFieldName != "选择要素")
                {
                    hasEdited = true;
                    object pFieldValue = e.Value;
                    DataRowView dataRow = (DataRowView)ctrlAttrubuteGrid1.AllDataGrid.MainView.GetRow(e.RowHandle) ;
                    object pFID = dataRow[ctrlAttrubuteGrid1.OIDFieldName];
                    attributeEditObject.FID = pFID;
                    attributeEditObject.FieldName = pFieldName;
                    attributeEditObject.Value = pFieldValue;
                    attributeEditObjectsList.Add(attributeEditObject);
                    ctrlAttrubuteGrid1.SyncAllAttTableAndSelectionAttTable(SystemUI.CtrlAttrubuteGrid.TableType.AllAttributeTable
                        , e.RowHandle, pFieldName);
                }
            }
            catch (Exception ex)
            {

            }
        }

        //改变选择属性表中的值
        private void ctrlAttrubuteGrid1_ViewSelectionAttributeCellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            try
            {
                AttributeEditObject attributeEditObject = new AttributeEditObject();
                string pFieldName = e.Column.FieldName;
                if (pFieldName == ctrlAttrubuteGrid1.OIDFieldName || pFieldName.ToUpper() == featureLayer.FeatureClass.ShapeFieldName ||
                    pFieldName == featureLayer.FeatureClass.AreaField.Name || pFieldName == featureLayer.FeatureClass.LengthField.Name)
                {
                    MessageBox.Show("此属性不允许更改!", "提示");
                    return;
                }
                if (pFieldName != "选择要素")
                {
                    hasEdited = true;
                    object pFieldValue = e.Value;
                    DataRowView dataRow = (DataRowView)ctrlAttrubuteGrid1.SelectionDataGrid.MainView.GetRow(e.RowHandle);
                    object pFID = dataRow[pFieldName];
                    attributeEditObject.FID = pFID;
                    attributeEditObject.FieldName = pFieldName;
                    attributeEditObject.Value = pFieldValue;
                    attributeEditObjectsList.Add(attributeEditObject);
                    ctrlAttrubuteGrid1.SyncAllAttTableAndSelectionAttTable(SystemUI.CtrlAttrubuteGrid.TableType.SelectionTable
                        , e.RowHandle, pFieldName);
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion 数据表事件

        #region 封装方法

        /// <summary>
        /// 设置窗体按钮的Enable属性
        /// </summary>
        /// <param name="enable"></param>
        private void setButtonEnable(bool enable)
        {
            barButtonItem2.Enabled = !enable;
            barButtonItem3.Enabled = enable;
            barButtonItem4.Enabled = enable;
            barButtonItem5.Enabled = enable;
            barSubItem1.Enabled = !enable;
        }

        /// <summary>
        /// 将GeoDatabase字段类型转换成.Net相应的数据类型
        /// </summary>
        /// <param name="fieldType">GeoDatabase字段类型</param>
        /// <returns>返回.net的字段类型</returns>
        public static string ParseFieldType(esriFieldType fieldType)
        {
            switch (fieldType)
            {
                case esriFieldType.esriFieldTypeBlob:
                    return "System.String";

                case esriFieldType.esriFieldTypeDate:
                    return "System.DateTime";

                case esriFieldType.esriFieldTypeDouble:
                    return "System.Double";

                case esriFieldType.esriFieldTypeGeometry:
                    return "System.String";

                case esriFieldType.esriFieldTypeGlobalID:
                    return "System.String";

                case esriFieldType.esriFieldTypeGUID:
                    return "System.String";

                case esriFieldType.esriFieldTypeInteger:
                    return "System.Int32";

                case esriFieldType.esriFieldTypeOID:
                    return "System.String";

                case esriFieldType.esriFieldTypeRaster:
                    return "System.String";

                case esriFieldType.esriFieldTypeSingle:
                    return "System.Single";

                case esriFieldType.esriFieldTypeSmallInteger:
                    return "System.Int32";

                case esriFieldType.esriFieldTypeString:
                    return "System.String";

                default:
                    return "System.String";
            }
        }

        /// <summary>
        /// 要素添加属性字段
        /// </summary>
        /// <param name="pFeatureClass">待添加字段的要素类</param>
        /// <param name="name">添加字段的名称</param>
        /// <param name="aliasName">添加字段的别名</param>
        /// <param name="FieldType">添加字段的类型</param>
        /// <returns>添加成功返回1，否则返回0</returns>
        public static int AddField(IFeatureClass pFeatureClass, string name, string aliasName, esriFieldType FieldType)
        {
            //若存在，则不需添加
            if (pFeatureClass.Fields.FindField(name) > -1) return 0;
            try
            {
                IField pField = new FieldClass();
                IFieldEdit pFieldEdit = pField as IFieldEdit;
                pFieldEdit.AliasName_2 = aliasName;
                pFieldEdit.Name_2 = name;
                pFieldEdit.Type_2 = FieldType;

                IClass pClass = pFeatureClass as IClass;
                pClass.AddField(pField);
                return 1;
            }
            catch (COMException ex)
            {
                MessageBox.Show(ex.Message);
                //SysLogHelper.WriteOperationLog("属性表添加字段错误", ex.Source, "数据编辑");
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Attribute), ex, "0304");
                return 0;
            }
        }

        /// <summary>
        /// 删除某个属性字段
        /// </summary>
        /// <param name="layer">需要操作的要素图层</param>
        /// <param name="fieldName">需要删除字段的名称</param>
        /// <returns>删除成功返回1，否则返回0</returns>
        public static int DeleteField(IFeatureLayer layer, string fieldName)
        {
            try
            {
                ITable pTable = (ITable)layer;
                IFields pfields;
                IField pfield;
                pfields = pTable.Fields;
                int fieldIndex = pfields.FindField(fieldName);
                pfield = pfields.get_Field(fieldIndex);
                pTable.DeleteField(pfield);
                return 1;
            }
            catch (COMException ex)
            {
                //SysLogHelper.WriteOperationLog("属性字段删除错误", ex.Source, "数据编辑");
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Attribute), ex, "属性表编辑");
                return 0;
            }
        }

        /// <summary>
        /// 更新FeatureClass的属性表
        /// </summary>
        /// <param name="dataTable">更新的属性表</param>
        public void WriteAttributeToFeatureClass(IFeatureClass pFeatureClass)
        {
            try
            {
                IQueryFilter pQueryFilter = new QueryFilterClass();
                IFeatureCursor pFeatureCursour = null;
                IFeature pFeature = null;
                foreach (AttributeEditObject attributeEditObject in attributeEditObjectsList)
                {
                    pQueryFilter.WhereClause = ctrlAttrubuteGrid1.OIDFieldName+"=" + attributeEditObject.FID;
                    pFeatureCursour = pFeatureClass.Search(pQueryFilter, false);
                    pFeature = pFeatureCursour.NextFeature();
                    pFeature.set_Value(pFeature.Fields.FindField(attributeEditObject.FieldName), attributeEditObject.Value);
                    pFeature.Store();
                }

                Marshal.ReleaseComObject(pQueryFilter);
                Marshal.ReleaseComObject(pFeatureCursour);
                Marshal.ReleaseComObject(pFeature);
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog("属性编辑失败", ex, ex.Message);
            }
        }

        #endregion 封装方法
        
    }
}