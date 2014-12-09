using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice
{
    public partial class frmAbout : Form
    {
        /// <summary>
        /// ���캯�� ��ȡϵͳ�汾����ͼӰ��汾
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public frmAbout()
        {
            InitializeComponent();

            string sVerb ="";
            int iA = 2,sB;
            string  sC;

            try
            {
                sVerb = CLC.INIClass.IniReadValue("�汾", "1");
                iA += Convert.ToInt16(sVerb) / 100;
                sB = Convert.ToInt16(sVerb)/10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label1.Text = "˳�¾��õ�����Ϣϵͳ �汾 " + iA.ToString() + "." + sB + "." + sC;

                sVerb = CLC.INIClass.IniReadValue("�汾", "5");
                sB = Convert.ToInt16(sVerb) / 10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label2.Text = "˳�¾������ݱ༭ϵͳ �汾 " + iA.ToString() + "." + sB + "." + sC;

                sVerb = CLC.INIClass.IniReadValue("�汾", "2");
                sB = Convert.ToInt16(sVerb) / 10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label3.Text = "��ͼ���� �汾 1." + sB + "." + sC;

                sVerb = CLC.INIClass.IniReadValue("�汾", "3");
                sB = Convert.ToInt16(sVerb) / 10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label4.Text = "Ӱ������ �汾 1." + sB + "." + sC;
            }
            catch(Exception ex)
            {
                ExToLog(ex,"��ʼ��");
            }
        }

        /// <summary>
        /// ȷ����ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "Main-frmAbout-" + sFunc);
        }

    }
}