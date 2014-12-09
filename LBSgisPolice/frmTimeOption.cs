using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice  
{
    public partial class frmTimeOption : Form
    {
        public frmTimeOption()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// �رմ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Dispose();
        }

        /// <summary>
        /// ȷ����ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
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
                CLC.BugRelated.ExceptionWrite(ex, "LBSgisPolice-frmTimeOption-buttonOk_Click");
            }
        }
    }
}