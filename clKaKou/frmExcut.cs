using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clKaKou
{
    public partial class frmExcut : Form
    {
        public frmExcut()
        {
            InitializeComponent();
        }
        public string ExDetail = string.Empty;

        /// <summary>
        /// ȷ����ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ExDetail = this.textBox1.Text.Trim();

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { CLC.BugRelated.ExceptionWrite(ex, "clKaKou-frmExcut-button1_Click"); }
        }

        /// <summary>
        /// ȡ����ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ExDetail = " ";
                this.DialogResult = DialogResult.Cancel;
            }
            catch (Exception ex) { CLC.BugRelated.ExceptionWrite(ex, "clKaKou-frmExcut-button2_Click"); }
        }        
    }
}