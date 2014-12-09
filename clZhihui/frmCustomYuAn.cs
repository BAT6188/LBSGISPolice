using System;
using System.Windows.Forms;

namespace clZhihui
{
    public partial class frmCustomYuAn : Form
    {
        public CheckBox[] checks;
        public TextBox[] texts;
        public frmCustomYuAn()
        {
            InitializeComponent();
            try
            {
                checks = new CheckBox[13] { checkCar, checkVideo, checkJY, checkZAKK, checkPCS, checkJWS, checkGGCS, checkWB, checkAF, checkTZHY, checkXF, checkMJZD,checkFire };
                texts = new TextBox[13] { textCarDis, textVideoDis, textJY, textZAKK, textPCS, textJWS, textGGCS, textWB, textAF, textTZHY, textXF, textMJZD,textFire };
            }
            catch (Exception ex) { ExToLog(ex, "���캯��"); }
        }

        /// <summary>
        /// ��Ӱ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (textYuanName.Text == "" || (checkCar.Checked && textCarDis.Text == "") || (checkVideo.Checked && textVideoDis.Text == "")
                     || (checkJY.Checked && textJY.Text == "") || (checkZAKK.Checked && textZAKK.Text == "")
                     || (checkGGCS.Checked && textGGCS.Text == "")
                     || (checkWB.Checked && textWB.Text == "") || (checkAF.Checked && textAF.Text == "")
                     || (checkTZHY.Checked && textTZHY.Text == "") || (checkXF.Checked && textXF.Text == "")
                     || (checkPCS.Checked && textPCS.Text == "") || (checkMJZD.Checked && textMJZD.Text == "")
                     || (checkJWS.Checked && textJWS.Text == "") || (checkFire.Checked && textFire.Text == ""))
                {
                    MessageBox.Show("������Ԥ������.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            catch (Exception ex) { ExToLog(ex, "buttonAdd_Click"); }
        }

        /// <summary>
        /// ȡ����ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex) { ExToLog(ex, "buttonCancel_Click"); }
        }

        private void textDis_Leave(object sender, EventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                bool isNum = checkNumber(textBox.Text);
                if (isNum == false)
                {
                    textBox.Text = "";
                    textBox.Focus();
                }
            }
            catch (Exception ex) { ExToLog(ex, "textDis_Leave"); }
        }

        /// <summary>
        /// �ж�������ǲ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="str"></param>
        /// <returns>����ֵ��true-�� false-���ǣ�</returns>
        private bool checkNumber(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return true;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//�ж�������ǲ�������
                {
                    System.Windows.Forms.MessageBox.Show("�������֣�");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex) { ExToLog(ex, "checkNumber"); return false; }
        }

        /// <summary>
        /// ��ѡ��ı��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void check_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox cb = (CheckBox)sender;
                texts[Convert.ToInt16(cb.Tag)].Enabled = cb.Checked;
                if (cb.Checked == false)
                {
                    texts[Convert.ToInt16(cb.Tag)].Text = "";
                }
            }
            catch (Exception ex) { ExToLog(ex, "check_CheckedChanged"); }
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
            CLC.BugRelated.ExceptionWrite(ex, "clZhihui-frmCustomYuAn-" + sFunc);
        }
    }
}