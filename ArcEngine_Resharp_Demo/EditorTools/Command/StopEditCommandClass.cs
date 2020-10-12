using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using PS.Plot.Sys;
using System;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public class StopEditCommandClass : ICommand
    {
        private IMap m_Map = null;
        private bool bEnable = true;
        private IActiveView m_activeView = null;
        private IHookHelper m_hookHelper = null;
        private IEngineEditor m_EngineEditor = null;

        #region ICommand 成员

        public int Bitmap
        {
            get { return -1; }
        }

        public string Caption
        {
            get { return "停止编辑"; }
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
            get { return "停止编辑"; }
        }

        public string Name
        {
            get { return "StopEditCommand"; }
        }

        public void OnClick()
        {
            m_Map = m_hookHelper.FocusMap;
            m_activeView = m_Map as IActiveView;
            m_EngineEditor = MapManager.EngineEditor;
            Boolean bSave = true;
            if (m_EngineEditor == null) return;
            if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
            IWorkspaceEdit pWsEdit = m_EngineEditor.EditWorkspace as IWorkspaceEdit;
            if (pWsEdit.IsBeingEdited())
            {
                Boolean bHasEdit = m_EngineEditor.HasEdits();
                if (bHasEdit)
                {
                    if (MessageBox.Show("是否保存所做的编辑？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        bSave = true;
                        SysLogHelper.WriteOperationLog("数据管理-数据编辑", "保存并停止编辑", "数据管理");
                    }
                    else
                    {
                        bSave = false;
                        SysLogHelper.WriteOperationLog("数据管理-数据编辑", "停止编辑未保存", "数据管理");
                    }
                }
                m_EngineEditor.StopEditing(bSave);
            }
            m_Map.ClearSelection();
            m_activeView.Refresh();
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
            get { return "停止编辑"; }
        }

        #endregion ICommand 成员
    }
}