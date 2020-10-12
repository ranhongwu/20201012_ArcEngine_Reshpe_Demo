using DevExpress.Utils;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using PS.Plot.Common;
using PS.Plot.Sys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public partial class Form_Editor : DevExpress.XtraEditors.XtraForm
    {
        public Form_Editor()
        {
            InitializeComponent();
            layerCount = GlobalVars.instance.MapControl.LayerCount;
            pActiveViewEvents = GlobalVars.instance.MapControl.Map as IActiveViewEvents_Event;
            pActiveViewEvents.ViewRefreshed += new IActiveViewEvents_ViewRefreshedEventHandler(MapEvent_LayerChanged);
        }

        #region 定义变量

        private bool isEditing = false;
        private IMap pMap = null;
        private int layerCount;
        private IMapControl4 pMapControl = null;
        private IActiveView pActiveView = null;
        private IEngineEditor pEngineEditor = null;
        private IEngineEditTask pEngineEditTask = null;
        private IEngineEditLayers pEngineEditLayers = null;
        private IFeatureLayer pCurrentLyr = null;
        private IList<ILayer> featureLayerList = new List<ILayer>();
        private ESRI.ArcGIS.Geodatabase.IWorkspace pWs = null;
        private IDataset pDataSet = null;
        private IActiveViewEvents_Event pActiveViewEvents;

        #endregion 定义变量

        #region 静态窗体

        public static Form_Attribute formAttribute = null;

        #endregion 静态窗体

        //窗体加载
        private void Form_Editor_Load(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(400, 190);
            setEditToolEnable(false);

            //遍历所有的图层，将矢量图层加入到下拉框
            cbxEditorFeature.Properties.Items.Clear();
            pMapControl = GlobalVars.instance.MapControl;
            pMap = GlobalVars.instance.MapControl.Map;
            featureLayerList = EngineAPI.GetMapControlFeatureLayer(pMap);
            foreach (IFeatureLayer pFeatureLayer in featureLayerList)
            {
                cbxEditorFeature.Properties.Items.Add(pFeatureLayer.FeatureClass.AliasName);
            }
        }

        //地图刷新
        private void MapEvent_LayerChanged(IActiveView View, esriViewDrawPhase phase, object Data, IEnvelope envelope)
        {
            List<string> featureLayerNameList = new List<string>();
            if (layerCount != GlobalVars.instance.MapControl.LayerCount)
            {
                layerCount = GlobalVars.instance.MapControl.LayerCount;
                cbxEditorFeature.Properties.Items.Clear();
                pMapControl = GlobalVars.instance.MapControl;
                pMap = GlobalVars.instance.MapControl.Map;
                featureLayerList = EngineAPI.GetMapControlFeatureLayer(pMap);
                foreach (IFeatureLayer pFeatureLayer in featureLayerList)
                {
                    cbxEditorFeature.Properties.Items.Add(pFeatureLayer.FeatureClass.AliasName);
                    featureLayerNameList.Add(pFeatureLayer.Name);
                }
                if (!featureLayerNameList.Contains(cbxEditorFeature.Text))
                {
                    cbxEditorFeature.Text = "";
                }
            }
            if (layerCount == 0)
            {
                cbxEditorFeature.Text = "";
            }
        }

        //选择要素（设置编辑目标图层）
        private void cbxEditorFeature_EditValueChanged(object sender, EventArgs e)
        {
            EditorInit();
            try
            {
                string sLyrName = cbxEditorFeature.EditValue.ToString();
                pCurrentLyr = MapManager.GetLayerByName(pMap, sLyrName) as IFeatureLayer;
                //设置编辑目标图层
                pEngineEditLayers.SetTargetLayer(pCurrentLyr, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "编辑工具选择要素");
            }
        }

        
        //开始编辑
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (pCurrentLyr == null)
                {
                    MessageBox.Show("请选择编辑图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                setEditToolEnable(true);
                //如果编辑已经开始，则直接退出
                if (pEngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing)
                    return;
                if (pCurrentLyr == null) return;
                //获取当前编辑图层工作空间
                pDataSet = pCurrentLyr.FeatureClass as IDataset;
                pWs = pDataSet.Workspace;
                pEngineEditTask = pEngineEditor.GetTaskByUniqueName("ControlToolsEditing_CreateNewFeatureTask");
                pEngineEditor.CurrentTask = pEngineEditTask;// 设置编辑任务
                pEngineEditor.EnableUndoRedo(true); //是否可以进行撤销、恢复操作
                //设置编辑模式，如果是ArcSDE采用版本模式
                if (pWs.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                {
                    IVersionedObject3 versionedObject = pDataSet as IVersionedObject3;
                    //注册版本
                    if (versionedObject != null && !versionedObject.IsRegisteredAsVersioned)
                    {
                        versionedObject.RegisterAsVersioned(true);
                    }
                    pEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;
                }
                //else
                //{
                //    pEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeNonVersioned;
                //}
                //设置编辑任务
                pEngineEditor.StartEditing(pWs, pMap);
                isEditing = true;
                //SysLogHelper.WriteOperationLog("数据编辑", "开始编辑", "数据管理");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "开始编辑");
            }
        }

        //保存编辑
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_saveEditCom = new SaveEditCommandClass();
                m_saveEditCom.OnCreate(pMapControl.Object);
                m_saveEditCom.OnClick();
                SysLogHelper.WriteOperationLog("数据管理-数据编辑", "保存编辑", "数据管理");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "保存编辑");
            }
        }

        //停止编辑
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_stopEditCom = new StopEditCommandClass();
                m_stopEditCom.OnCreate(pMapControl.Object);
                m_stopEditCom.OnClick();
                setEditToolEnable(false);
                pMapControl.CurrentTool = null;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
                isEditing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "停止编辑");
            }
        }

        //选择要素
        private void btnSelect_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_SelTool = new ControlsSelectFeaturesToolClass();
                m_SelTool.OnCreate(pMapControl.Object);
                m_SelTool.OnClick();
                pMapControl.CurrentTool = m_SelTool as ITool;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "选择要素");
            }
        }

        //移动要素
        private void btnMove_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_moveTool = new MoveFeatureToolClass();
                m_moveTool.OnCreate(pMapControl.Object);
                m_moveTool.OnClick();
                pMapControl.CurrentTool = m_moveTool as ITool;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog(typeof(Form_Editor), ex, "移动要素");
            }
        }

        //删除要素
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
                ICommand m_delFeatCom = new ControlsEditingClearCommandClass();
                m_delFeatCom.OnCreate(pMapControl.Object);
                m_delFeatCom.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog(typeof(Form_Editor), ex, "删除要素");
            }
        }

        //添加要素
        private void btnAddFeature_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_CreateFeatTool = new CreateFeatureToolClass();
                m_CreateFeatTool.OnCreate(pMapControl.Object);
                m_CreateFeatTool.OnClick();
                pMapControl.CurrentTool = m_CreateFeatTool as ITool;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogHelper.WriteLog(typeof(Form_Editor), ex, "添加要素");
            }
        }

        //清除选择
        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            pMapControl.Map.ClearSelection();
            EditVertexClass.ShowAllVertex(pCurrentLyr);
            pMapControl.Refresh();
        }

        //移动节点
        private void btnMoveNode_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_MoveVertexTool = new MoveVertexToolClass();
                m_MoveVertexTool.OnCreate(pMapControl.Object);
                pMapControl.CurrentTool = m_MoveVertexTool as ITool;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "移动节点");
            }
        }

        //添加节点
        private void btnAddNode_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_AddVertexTool = new AddVertexToolClass();
                m_AddVertexTool.OnCreate(pMapControl.Object);
                pMapControl.CurrentTool = m_AddVertexTool as ITool;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "添加节点");
            }
        }

        //删除节点
        private void btn_DelNode_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand m_DelVertexTool = new DelVertexToolClass();
                m_DelVertexTool.OnCreate(pMapControl.Object);
                pMapControl.CurrentTool = m_DelVertexTool as ITool;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "删除节点");
            }
        }

        //分割要素
        private void btnDivision_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand command = new CutPolygonTool();
                command.OnCreate(pMapControl.Object);
                pMapControl.CurrentTool = command as ITool;
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                command.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "分割要素");
            }
        }

        //合并要素
        private void btnMerge_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand pUnionFeature = new MergeFeatures();
                pUnionFeature.OnCreate(pMapControl.Object);
                pMapControl.CurrentTool = pUnionFeature as ITool;
                pMapControl.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "合并要素");
            }
        }

        //撤销操作
        private void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
                ICommand m_undoCommand = new UndoCommandClass();// ControlsUndoCommandClass();// 
                m_undoCommand.OnCreate(pMapControl.Object);
                m_undoCommand.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "编辑撤销");
            }
        }

        //重做操作
        private void btnRedo_Click(object sender, EventArgs e)
        {
            try
            {
                pMapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
                ICommand m_redoCommand = new RedoCommandClass();// ControlsRedoCommandClass();
                m_redoCommand.OnCreate(pMapControl.Object);
                m_redoCommand.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "编辑重做");
            }
        }

        //属性表编辑
        private void btnAttributeEdit_Click(object sender, EventArgs e)
        {
            WaitDialogForm dlg = new WaitDialogForm("正在查询，请稍候......", "提示");
            try
            {
                dlg.Show();
                if (pCurrentLyr == null)
                {
                    MessageBox.Show("请选择编辑图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (formAttribute == null)
                {
                    formAttribute = new Form_Attribute(pCurrentLyr, pMap, isEditing);
                    formAttribute.Owner = this;
                    formAttribute.Show();
                }
                else
                {
                    formAttribute.Activate();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("打开属性表失败", ex, ex.Message);
            }
            finally
            {
                dlg.Close();
            }
        }
        
        //关闭窗体
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (btnStop.Enabled == true)
            {
                DialogResult dialogResult = MessageBox.Show("是否保存并停止编辑?", "提示", MessageBoxButtons.YesNoCancel);
                switch (dialogResult)
                {
                    case DialogResult.Yes:
                        btnSave_Click(sender, e);
                        break;

                    case DialogResult.No:
                        IEngineEditor m_EngineEditor = MapManager.EngineEditor;
                        m_EngineEditor.StopEditing(false);
                        break;

                    case DialogResult.Cancel:
                        return;

                    default:
                        break;
                }
            }
            this.Close();
        }

        #region 控制窗体移动

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private const int VM_NCLBUTTONDOWN = 0XA1;//定义鼠标左键按下
        private const int HTCAPTION = 2;

        //窗体移动
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //为当前应用程序释放鼠标捕获
            ReleaseCapture();
            //发送消息 让系统误以为在标题栏上按下鼠标
            SendMessage((IntPtr)this.Handle, VM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        #endregion 控制窗体移动

        #region ToolTip

        //开始编辑ToolTip
        private void btnStart_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("开始编辑", this.btnStart, 10, 20, 3000);
        }

        //保存编辑ToolTip
        private void btnSave_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("保存编辑", this.btnSave, 10, 20, 3000);
        }

        //停止编辑ToolTip
        private void btnStop_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("停止编辑", this.btnStop, 10, 20, 3000);
        }

        //选择要素ToolTip
        private void btnSelect_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("选择要素", this.btnSelect, 10, 20, 3000);
        }

        //移动要素ToolTip
        private void btnMove_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("移动要素", this.btnMove, 10, 20, 3000);
        }

        //删除要素ToolTip
        private void btnDelete_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("删除要素", this.btnDelete, 10, 20, 3000);
        }

        //添加要素ToolTip
        private void btnAddFeature_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("添加要素", this.btnAddFeature, 10, 20, 3000);
        }

        //清除选择要素ToolTip
        private void btnClearSelection_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("清除选择", this.btnClearSelection, 10, 20, 3000);
        }

        //移动节点ToolTip
        private void btnMoveNode_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("移动节点", this.btnMoveNode, 10, 20, 3000);
        }

        //添加节点ToolTip
        private void btnAddNode_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("添加节点", this.btnAddNode, 10, 20, 3000);
        }

        //删除节点ToolTip
        private void btn_DelNode_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("删除节点", this.btn_DelNode, 10, 20, 3000);
        }

        //分割要素ToolTip
        private void btnDivision_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("分割要素", this.btnDivision, 10, 20, 3000);
        }

        //合并要素ToolTip
        private void btnMerge_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("合并要素", this.btnMerge, 10, 20, 3000);
        }

        //撤销操作ToolTip
        private void btnBack_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("撤销操作", this.btnBack, 10, 20, 3000);
        }

        //重做操作ToolTip
        private void btnRedo_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("重做操作", this.btnRedo, 10, 20, 3000);
        }

        //属性表编辑ToolTip
        private void btnAttributeEdit_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show("属性表编辑", this.btnRedo, 10, 20, 3000);
        }

        #endregion ToolTip

        #region 封装方法

        /// <summary>
        /// 编辑工具初始化
        /// </summary>
        private void EditorInit()
        {
            pEngineEditor = new EngineEditorClass();
            MapManager.EngineEditor = pEngineEditor;
            pEngineEditTask = pEngineEditor as IEngineEditTask;
            pEngineEditLayers = pEngineEditor as IEngineEditLayers;
            pActiveView = pMap as IActiveView;
        }

        /// <summary>
        /// 设置编辑按钮的Enable属性
        /// </summary>
        /// <param name="enable"></param>
        private void setEditToolEnable(bool enable)
        {
            btnSave.Enabled = enable;
            btnStop.Enabled = enable;
            btnSelect.Enabled = enable;
            btnMove.Enabled = enable;
            btnDelete.Enabled = enable;
            btnAddFeature.Enabled = enable;
            btnClearSelection.Enabled = enable;
            btnMoveNode.Enabled = enable;
            btnAddNode.Enabled = enable;
            btn_DelNode.Enabled = enable;
            btnDivision.Enabled = enable;
            btnMerge.Enabled = enable;
            btnBack.Enabled = enable;
            btnRedo.Enabled = enable;
        }

        //获取当前地图的单位
        public static string GetMapUnit(esriUnits _esriMapUnit)
        {
            string sMapUnits = string.Empty;
            switch (_esriMapUnit)
            {
                case esriUnits.esriCentimeters:
                    sMapUnits = "厘米";
                    break;

                case esriUnits.esriDecimalDegrees:
                    sMapUnits = "十进制度";
                    break;

                case esriUnits.esriDecimeters:
                    sMapUnits = "分米";
                    break;

                case esriUnits.esriFeet:
                    sMapUnits = "尺";
                    break;

                case esriUnits.esriInches:
                    sMapUnits = "英寸";
                    break;

                case esriUnits.esriKilometers:
                    sMapUnits = "千米";
                    break;

                case esriUnits.esriMeters:
                    sMapUnits = "米";
                    break;

                case esriUnits.esriMiles:
                    sMapUnits = "英里";
                    break;

                case esriUnits.esriMillimeters:
                    sMapUnits = "毫米";
                    break;

                case esriUnits.esriNauticalMiles:
                    sMapUnits = "海里";
                    break;

                case esriUnits.esriPoints:
                    sMapUnits = "点";
                    break;

                case esriUnits.esriUnitsLast:
                    sMapUnits = "UnitsLast";
                    break;

                case esriUnits.esriUnknownUnits:
                    sMapUnits = "未知单位";
                    break;

                case esriUnits.esriYards:
                    sMapUnits = "码";
                    break;

                default:
                    break;
            }
            return sMapUnits;
        }

        #endregion 封装方法

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            try
            {
                ICommand pUnionFeature = new BreakFeatures();
                pUnionFeature.OnCreate(pMapControl.Object);
                pMapControl.CurrentTool = pUnionFeature as ITool;
                pMapControl.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PS.Plot.Common.LogHelper.WriteLog(typeof(Form_Editor), ex, "打散要素");
            }
        }
    }
}