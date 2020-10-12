using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using PS.Plot.Sys;
using System;

namespace PS.Plot.Editor
{
    public class RedoCommandClass : ICommand
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
            get { return "恢复操作"; }
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
            get { return "恢复撤销的操作"; }
        }

        public string Name
        {
            get { return "RedoCommand"; }
        }

        public void OnClick()
        {
            try
            {
                m_Map = m_hookHelper.FocusMap;
                m_activeView = m_Map as IActiveView;
                m_EngineEditor = MapManager.EngineEditor;
                if (m_EngineEditor == null) return;
                if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing) return;
                IWorkspaceEdit pWSEdit = m_EngineEditor.EditWorkspace as IWorkspaceEdit;
                if (pWSEdit == null) return;
                Boolean bHasRedo = true;
                pWSEdit.HasRedos(ref bHasRedo);
                if (bHasRedo)
                    pWSEdit.RedoEditOperation();
                m_activeView.Refresh();
            }
            catch (Exception ex)
            {
            }
        }

        public void OnCreate(object Hook)
        {
            try
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
            catch (Exception ex)
            {
                //SysLogHelper.WriteOperationLog("恢复撤销编辑错误", ex.Source, "数据编辑");
            }
        }

        public string Tooltip
        {
            get { return "恢复撤销的操作"; }
        }

        #endregion ICommand 成员
    }
}