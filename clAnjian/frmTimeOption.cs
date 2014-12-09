using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clAnjian  
{
    public partial class frmTimeOption : Form
    {
        public frmTimeOption()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
                this.Dispose();
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clAnjian-frmTimeOption-buttonCancel_Click");
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (dateTimeBegin.Value >= dateTimeEnd.Value)
                {
                    MessageBox.Show("��ʼʱ��ӦС����ֹʱ��,������!", "ʱ�����ô���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Dispose();
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clAnjian-frmTimeOption-buttonOK_Click");
            }
        }
    }
}