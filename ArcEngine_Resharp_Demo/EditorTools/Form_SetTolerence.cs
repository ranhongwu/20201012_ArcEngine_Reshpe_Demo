using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PS.Plot.Editor
{
    public partial class Form_SetTolerence : DevExpress.XtraEditors.XtraForm
    {
        public Form_SetTolerence(ref double editTolerence)
        {
            InitializeComponent();
            this.tolerence = editTolerence;
        }

        #region 定义变量

        public double tolerence = 0;
        public string setUnit;

        #endregion 定义变量

        //加载窗体
        private void Form_SetTolerence_Load(object sender, EventArgs e)
        {
            lbUnit.Text = setUnit;
        }

        //确定
        private void btnOK_Click(object sender, EventArgs e)
        {
            string pattern = @"^[+]{0,1}(\d+)$|^[+]{0,1}(\d+\.\d+)$";
            if (!Regex.IsMatch(textEdit1.Text, pattern))
            {
                MessageBox.Show("输入的容差不合法！", "错误");
                return;
            }
            tolerence = Convert.ToDouble(textEdit1.Text);
        }
    }
}