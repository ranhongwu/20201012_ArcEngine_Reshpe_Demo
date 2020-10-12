using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using PS.Plot.Sys;
using System;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public class DelFeatureCommandClass : ICommand
    {
        private IMap m_Map = null;
        private bool bEnable = true;
        private IHookHelper m_hookHelper = null;
        private IActiveView m_activeView = null;
        private IEngineEditor m_EngineEditor = null;
        private IEngineEditLayers m_EngineEditLayers = null;

        #region ICommand 成员

        public int Bitmap
        {
            get { return -1; }
        }

        public string Caption
        {
            get { return "删除要素"; }
        }

        public string Category
        {
            get { return "编辑按钮"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return bEnable; }
        }

        public int HelpContextID
        {
            get { return -1; }
        }

        public string HelpFile
        {
            get { return ""; }
        }

        public string Message
        {
            get { return "删除要素"; }
        }

        public string Name
        {
            get { return "DeleteCommand"; }
        }

        public void OnClick()
        {
            try
            {
                m_Map = m_hookHelper.FocusMap;
                m_activeView = m_Map as IActiveView;
                m_EngineEditor = MapManager.EngineEditor;
                m_EngineEditLayers = MapManager.EngineEditor as IEngineEditLayers;
                if (m_EngineEditor == null) return;
                if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
                if (m_EngineEditLayers == null) return;
                IFeatureLayer pFeatLyr = m_EngineEditLayers.TargetLayer;
                if (pFeatLyr == null) return;
                IFeatureClass pFeatCls = pFeatLyr.FeatureClass;
                if (pFeatCls == null) return;
                IFeatureCursor pFeatCur = MapManager.GetSelectedFeatures(pFeatLyr);
                if (pFeatCur == null)
                {
                    MessageBox.Show("请选择要删除的要素！", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                m_EngineEditor.StartOperation();
                IFeature pFeature = pFeatCur.NextFeature();
                if (MessageBox.Show("是否删除所选要素？", "提示", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    while (pFeature != null)
                    {
                        pFeature.Delete();
                        pFeature = pFeatCur.NextFeature();
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatCur);
                m_EngineEditor.StopOperation("DelFeatureCommand");
                m_activeView.Refresh();
            }
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("要素删除错误", ex.Source, "数据编辑");
            }
        }

        public void OnCreate(object Hook)
        {
            EditVertexClass.ClearResource();

            if (Hook == null) return;
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = Hook;
                if (m_hookHelper.ActiveView == null)
                    m_hookHelper = null;
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                bEnable = false;
            else
                bEnable = true;
        }

        public string Tooltip
        {
            get { return "删除选择要素"; }
        }

        #endregion ICommand 成员
    }
}