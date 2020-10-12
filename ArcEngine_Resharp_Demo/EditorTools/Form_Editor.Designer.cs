namespace PS.Plot.Editor
{
    partial class Form_Editor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Editor));
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.btnAttributeEdit = new DevExpress.XtraEditors.SimpleButton();
            this.btnRedo = new DevExpress.XtraEditors.SimpleButton();
            this.btnBack = new DevExpress.XtraEditors.SimpleButton();
            this.btnMerge = new DevExpress.XtraEditors.SimpleButton();
            this.btnDivision = new DevExpress.XtraEditors.SimpleButton();
            this.btn_DelNode = new DevExpress.XtraEditors.SimpleButton();
            this.btnAddNode = new DevExpress.XtraEditors.SimpleButton();
            this.btnMoveNode = new DevExpress.XtraEditors.SimpleButton();
            this.btnClearSelection = new DevExpress.XtraEditors.SimpleButton();
            this.btnAddFeature = new DevExpress.XtraEditors.SimpleButton();
            this.btnDelete = new DevExpress.XtraEditors.SimpleButton();
            this.btnMove = new DevExpress.XtraEditors.SimpleButton();
            this.btnSelect = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnStop = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnStart = new DevExpress.XtraEditors.SimpleButton();
            this.cbxEditorFeature = new DevExpress.XtraEditors.ComboBoxEdit();
            this.toolTip1 = new System.Windows.Forms.ToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbxEditorFeature.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // simpleButton1
            // 
            this.simpleButton1.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.Image")));
            this.simpleButton1.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButton1.Location = new System.Drawing.Point(1060, 5);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(4);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(48, 32);
            this.simpleButton1.TabIndex = 0;
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.simpleButton2);
            this.panelControl1.Controls.Add(this.btnAttributeEdit);
            this.panelControl1.Controls.Add(this.btnRedo);
            this.panelControl1.Controls.Add(this.btnBack);
            this.panelControl1.Controls.Add(this.btnMerge);
            this.panelControl1.Controls.Add(this.btnDivision);
            this.panelControl1.Controls.Add(this.btn_DelNode);
            this.panelControl1.Controls.Add(this.btnAddNode);
            this.panelControl1.Controls.Add(this.btnMoveNode);
            this.panelControl1.Controls.Add(this.btnClearSelection);
            this.panelControl1.Controls.Add(this.btnAddFeature);
            this.panelControl1.Controls.Add(this.btnDelete);
            this.panelControl1.Controls.Add(this.btnMove);
            this.panelControl1.Controls.Add(this.btnSelect);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Controls.Add(this.btnStop);
            this.panelControl1.Controls.Add(this.btnSave);
            this.panelControl1.Controls.Add(this.btnStart);
            this.panelControl1.Controls.Add(this.cbxEditorFeature);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelControl1.Location = new System.Drawing.Point(0, 36);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(4, 1, 4, 4);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1122, 45);
            this.panelControl1.TabIndex = 1;
            // 
            // simpleButton2
            // 
            this.simpleButton2.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton2.Image")));
            this.simpleButton2.Location = new System.Drawing.Point(895, 9);
            this.simpleButton2.Margin = new System.Windows.Forms.Padding(4);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(34, 33);
            this.simpleButton2.TabIndex = 18;
            this.simpleButton2.ToolTip = "拆分多部件";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // btnAttributeEdit
            // 
            this.btnAttributeEdit.Image = ((System.Drawing.Image)(resources.GetObject("btnAttributeEdit.Image")));
            this.btnAttributeEdit.Location = new System.Drawing.Point(1060, 9);
            this.btnAttributeEdit.Margin = new System.Windows.Forms.Padding(4);
            this.btnAttributeEdit.Name = "btnAttributeEdit";
            this.btnAttributeEdit.Size = new System.Drawing.Size(34, 33);
            this.btnAttributeEdit.TabIndex = 17;
            this.btnAttributeEdit.Click += new System.EventHandler(this.btnAttributeEdit_Click);
            this.btnAttributeEdit.MouseHover += new System.EventHandler(this.btnAttributeEdit_MouseHover);
            // 
            // btnRedo
            // 
            this.btnRedo.Image = ((System.Drawing.Image)(resources.GetObject("btnRedo.Image")));
            this.btnRedo.Location = new System.Drawing.Point(985, 9);
            this.btnRedo.Margin = new System.Windows.Forms.Padding(4);
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.Size = new System.Drawing.Size(34, 33);
            this.btnRedo.TabIndex = 16;
            this.btnRedo.Click += new System.EventHandler(this.btnRedo_Click);
            this.btnRedo.MouseHover += new System.EventHandler(this.btnRedo_MouseHover);
            // 
            // btnBack
            // 
            this.btnBack.Image = ((System.Drawing.Image)(resources.GetObject("btnBack.Image")));
            this.btnBack.Location = new System.Drawing.Point(944, 9);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(34, 33);
            this.btnBack.TabIndex = 15;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            this.btnBack.MouseHover += new System.EventHandler(this.btnBack_MouseHover);
            // 
            // btnMerge
            // 
            this.btnMerge.Image = ((System.Drawing.Image)(resources.GetObject("btnMerge.Image")));
            this.btnMerge.Location = new System.Drawing.Point(853, 9);
            this.btnMerge.Margin = new System.Windows.Forms.Padding(4);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(34, 33);
            this.btnMerge.TabIndex = 14;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            this.btnMerge.MouseHover += new System.EventHandler(this.btnMerge_MouseHover);
            // 
            // btnDivision
            // 
            this.btnDivision.Image = ((System.Drawing.Image)(resources.GetObject("btnDivision.Image")));
            this.btnDivision.Location = new System.Drawing.Point(812, 9);
            this.btnDivision.Margin = new System.Windows.Forms.Padding(4);
            this.btnDivision.Name = "btnDivision";
            this.btnDivision.Size = new System.Drawing.Size(34, 33);
            this.btnDivision.TabIndex = 13;
            this.btnDivision.Click += new System.EventHandler(this.btnDivision_Click);
            this.btnDivision.MouseHover += new System.EventHandler(this.btnDivision_MouseHover);
            // 
            // btn_DelNode
            // 
            this.btn_DelNode.Image = ((System.Drawing.Image)(resources.GetObject("btn_DelNode.Image")));
            this.btn_DelNode.Location = new System.Drawing.Point(759, 9);
            this.btn_DelNode.Margin = new System.Windows.Forms.Padding(4);
            this.btn_DelNode.Name = "btn_DelNode";
            this.btn_DelNode.Size = new System.Drawing.Size(34, 33);
            this.btn_DelNode.TabIndex = 12;
            this.btn_DelNode.Click += new System.EventHandler(this.btn_DelNode_Click);
            this.btn_DelNode.MouseHover += new System.EventHandler(this.btn_DelNode_MouseHover);
            // 
            // btnAddNode
            // 
            this.btnAddNode.Image = ((System.Drawing.Image)(resources.GetObject("btnAddNode.Image")));
            this.btnAddNode.Location = new System.Drawing.Point(718, 9);
            this.btnAddNode.Margin = new System.Windows.Forms.Padding(4);
            this.btnAddNode.Name = "btnAddNode";
            this.btnAddNode.Size = new System.Drawing.Size(34, 33);
            this.btnAddNode.TabIndex = 11;
            this.btnAddNode.Click += new System.EventHandler(this.btnAddNode_Click);
            this.btnAddNode.MouseHover += new System.EventHandler(this.btnAddNode_MouseHover);
            // 
            // btnMoveNode
            // 
            this.btnMoveNode.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveNode.Image")));
            this.btnMoveNode.Location = new System.Drawing.Point(676, 9);
            this.btnMoveNode.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveNode.Name = "btnMoveNode";
            this.btnMoveNode.Size = new System.Drawing.Size(34, 33);
            this.btnMoveNode.TabIndex = 10;
            this.btnMoveNode.Click += new System.EventHandler(this.btnMoveNode_Click);
            this.btnMoveNode.MouseHover += new System.EventHandler(this.btnMoveNode_MouseHover);
            // 
            // btnClearSelection
            // 
            this.btnClearSelection.Image = ((System.Drawing.Image)(resources.GetObject("btnClearSelection.Image")));
            this.btnClearSelection.Location = new System.Drawing.Point(611, 9);
            this.btnClearSelection.Margin = new System.Windows.Forms.Padding(4);
            this.btnClearSelection.Name = "btnClearSelection";
            this.btnClearSelection.Size = new System.Drawing.Size(34, 33);
            this.btnClearSelection.TabIndex = 9;
            this.btnClearSelection.Click += new System.EventHandler(this.btnClearSelection_Click);
            this.btnClearSelection.MouseHover += new System.EventHandler(this.btnClearSelection_MouseHover);
            // 
            // btnAddFeature
            // 
            this.btnAddFeature.Image = ((System.Drawing.Image)(resources.GetObject("btnAddFeature.Image")));
            this.btnAddFeature.Location = new System.Drawing.Point(570, 9);
            this.btnAddFeature.Margin = new System.Windows.Forms.Padding(4);
            this.btnAddFeature.Name = "btnAddFeature";
            this.btnAddFeature.Size = new System.Drawing.Size(34, 33);
            this.btnAddFeature.TabIndex = 8;
            this.btnAddFeature.Click += new System.EventHandler(this.btnAddFeature_Click);
            this.btnAddFeature.MouseHover += new System.EventHandler(this.btnAddFeature_MouseHover);
            // 
            // btnDelete
            // 
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.Location = new System.Drawing.Point(529, 9);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(34, 33);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            this.btnDelete.MouseHover += new System.EventHandler(this.btnDelete_MouseHover);
            // 
            // btnMove
            // 
            this.btnMove.Image = ((System.Drawing.Image)(resources.GetObject("btnMove.Image")));
            this.btnMove.Location = new System.Drawing.Point(488, 9);
            this.btnMove.Margin = new System.Windows.Forms.Padding(4);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(34, 33);
            this.btnMove.TabIndex = 6;
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            this.btnMove.MouseHover += new System.EventHandler(this.btnMove_MouseHover);
            // 
            // btnSelect
            // 
            this.btnSelect.Image = ((System.Drawing.Image)(resources.GetObject("btnSelect.Image")));
            this.btnSelect.Location = new System.Drawing.Point(446, 9);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(4);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(34, 33);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            this.btnSelect.MouseHover += new System.EventHandler(this.btnSelect_MouseHover);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(28, 12);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(4);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(90, 22);
            this.labelControl1.TabIndex = 4;
            this.labelControl1.Text = "编辑要素：";
            // 
            // btnStop
            // 
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.Location = new System.Drawing.Point(382, 9);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(34, 33);
            this.btnStop.TabIndex = 3;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            this.btnStop.MouseHover += new System.EventHandler(this.btnStop_MouseHover);
            // 
            // btnSave
            // 
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.Location = new System.Drawing.Point(345, 9);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(34, 33);
            this.btnSave.TabIndex = 2;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSave.MouseHover += new System.EventHandler(this.btnSave_MouseHover);
            // 
            // btnStart
            // 
            this.btnStart.Image = ((System.Drawing.Image)(resources.GetObject("btnStart.Image")));
            this.btnStart.Location = new System.Drawing.Point(308, 9);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(34, 33);
            this.btnStart.TabIndex = 1;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            this.btnStart.MouseHover += new System.EventHandler(this.btnStart_MouseHover);
            // 
            // cbxEditorFeature
            // 
            this.cbxEditorFeature.Location = new System.Drawing.Point(124, 9);
            this.cbxEditorFeature.Margin = new System.Windows.Forms.Padding(4);
            this.cbxEditorFeature.Name = "cbxEditorFeature";
            this.cbxEditorFeature.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbxEditorFeature.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbxEditorFeature.Size = new System.Drawing.Size(176, 28);
            this.cbxEditorFeature.TabIndex = 0;
            this.cbxEditorFeature.EditValueChanged += new System.EventHandler(this.cbxEditorFeature_EditValueChanged);
            // 
            // Form_Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1122, 81);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.panelControl1);
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Shadow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Form_Editor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form_Editor_Load);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbxEditorFeature.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.ComboBoxEdit cbxEditorFeature;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnStart;
        private DevExpress.XtraEditors.SimpleButton btnStop;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnMove;
        private DevExpress.XtraEditors.SimpleButton btnSelect;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnAddFeature;
        private System.Windows.Forms.ToolTip toolTip1;
        private DevExpress.XtraEditors.SimpleButton btnClearSelection;
        private DevExpress.XtraEditors.SimpleButton btn_DelNode;
        private DevExpress.XtraEditors.SimpleButton btnAddNode;
        private DevExpress.XtraEditors.SimpleButton btnMoveNode;
        private DevExpress.XtraEditors.SimpleButton btnRedo;
        private DevExpress.XtraEditors.SimpleButton btnBack;
        private DevExpress.XtraEditors.SimpleButton btnMerge;
        private DevExpress.XtraEditors.SimpleButton btnDivision;
        private DevExpress.XtraEditors.SimpleButton btnAttributeEdit;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
    }
}

