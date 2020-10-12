using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using GISEditor.EditTool.Tool;
using KYKJ.EditTool.BasicClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcEngine_Resharp_Demo
{
    public partial class Form1 : Form
    {
        IDataset pDataSet = null;
        IFeatureLayer pCurrentLyr = null;
        IWorkspace pWs = null;
        IEngineEditTask pEngineEditTask = null;
        IEngineEditor pEngineEditor = null;
        IMap pMap = null;
        IEngineEditLayers pEngineEditLayers = null;

        IMapControl2 mapControl4 ;

        public Form1()
        {
            InitializeComponent();
            MapManager.EngineEditor=new EngineEditorClass();
            
            
            mapControl4 = axMapControl1.Object as IMapControl2;
            //pMap = this.mapControl4.Map;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pCurrentLyr = this.mapControl4.get_Layer(0) as IFeatureLayer;

            pDataSet = pCurrentLyr.FeatureClass as IDataset;
            pWs = pDataSet.Workspace;
            //设置编辑模式，如果是ArcSDE采用版本模式
            if (pWs.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
            {
                IVersionedObject3 versionedObject = pDataSet as IVersionedObject3;
                //注册版本
                if (versionedObject != null && !versionedObject.IsRegisteredAsVersioned)
                {
                    versionedObject.RegisterAsVersioned(true);
                }
                MapManager.EngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;
            }
            else
            {
                MapManager.EngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeNonVersioned;
            }
            //设置编辑任务
            ((IEngineEditLayers)MapManager.EngineEditor).SetTargetLayer(pCurrentLyr, 0);
            MapManager.EngineEditor.StartEditing(pWs, this.mapControl4.ActiveView.FocusMap);
            MapManager.EngineEditor.EnableUndoRedo(true);
            
            pEngineEditTask = MapManager.EngineEditor as IEngineEditTask;
            pEngineEditTask = MapManager.EngineEditor.GetTaskByUniqueName("ControlToolsEditing_CreateNewFeatureTask");
            MapManager.EngineEditor.CurrentTask = pEngineEditTask;// 设置编辑任务
        }

        //重塑
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {

                ICommand m_CreateFeatTool = new ReshaprTool();
                m_CreateFeatTool.OnCreate(axMapControl1.Object);
                m_CreateFeatTool.OnClick();
                this.axMapControl1.CurrentTool = m_CreateFeatTool as ITool;
                mapControl4.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {

            }
        }

    }
}
