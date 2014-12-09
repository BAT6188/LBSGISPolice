using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clVideo
{
    public partial class frmvideoset : Form
    {
        public frmvideoset()
        {
            InitializeComponent();
        }

        string inipath = string.Empty;

        public string Sip = string.Empty;
        public string Sport = string.Empty;
        public string Sfolder = string.Empty;
        public string Susername = string.Empty;
        public string Spassword = string.Empty;

        /// <summary>
        /// ���水ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Sip = this.txtip.Text.Trim();
                Sport = this.txtport.Text.Trim();
                Sfolder = this.txtfolder.Text.Trim();
                Susername = this.txtusername.Text.Trim();
                Spassword = this.txtpswd.Text.Trim();

                if (Sip == "" || Sport == "" || Sfolder == "" || Susername == "" || Spassword == "")
                {
                    MessageBox.Show("�������Ϊ�գ���ȷ�ϣ�","ϵͳ��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                }

                //WriteEditLog(ALARMSYS + ":" + ALARMUSER + ":" + this.txtdist.Text.Trim(), "���Ŀ�������", "�����ļ�");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }     
        }

        /// <summary>
        /// �رմ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// ������Ƶ�ͻ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnClient_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofg = new OpenFileDialog();
                ofg.Filter = "Ӧ�ó���(*.exe)|*.exe";
                ofg.Title = "ѡ����Ƶ��ؿͻ���";
                ofg.AddExtension = true;
                ofg.InitialDirectory = Application.StartupPath.Remove(Application.StartupPath.LastIndexOf("\\")) + "\\Carrier\\";

                if (ofg.ShowDialog() == DialogResult.OK)
                {
                    string FileName = ofg.FileName;

                    if (FileName.IndexOf(".exe") > -1)
                    {
                        FileName = FileName.Remove(FileName.LastIndexOf("\\")) + "\\surveillance1.exe";

                        this.txtClient.Text = FileName;
                    }
                    else
                    {
                        MessageBox.Show("ѡ�����������ѡ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "btnClient_Click"); }                
        }

        /// <summary>
        /// �쳣��־���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clVideo-frmvideoset-" + sFunc);
        }
    }
}