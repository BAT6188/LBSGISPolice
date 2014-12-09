using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice
{
    public partial class frmScale : Form
    {
        public frmScale()
        {
            InitializeComponent();
            try
            {
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                txtScale.Text = CLC.INIClass.IniReadValue("比例尺", "缩放比例");
            }
            catch { }
        }
        public string scale = "";

        /// <summary>
        /// 确定按钮 点击时获取文本框中的值 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtScale.Text == "")
            {
                MessageBox.Show("请输入缩放比例！","系统提示");
                return;
            }
            scale = txtScale.Text;
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 关闭窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 当输入的不是数字时无效
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtScale_KeyPress(object sender, KeyPressEventArgs e)
        {
            string s = "1234567890." + (char)8;
            if (s.IndexOf(e.KeyChar.ToString()) < 0)
            {
                e.Handled = true;
            }
        }
    }
}