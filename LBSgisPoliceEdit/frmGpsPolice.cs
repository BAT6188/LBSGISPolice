using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPoliceEdit
{
    public partial class frmGpsPolice : Form
    {
        public frmGpsPolice()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text == null || this.textBox1.Text == "")
                {
                    MessageBox.Show("请输入警力编号！", "提示");
                    return;
                }
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-frmGpsPolice-btnOK_Click");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}