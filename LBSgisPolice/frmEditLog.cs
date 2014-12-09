using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice
{
    public partial class frmEditLog : Form
    {
        private string[] strConn;

        /// <summary>
        /// ���캯��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="s">�������Ӳ���</param>
        public frmEditLog(string[] s)
        {
            InitializeComponent();
            strConn = s;
            InitializeUser();
            comboBoxModule.Text = comboBoxModule.Items[0].ToString();
        }

        /// <summary>
        /// ��ʼ��������¼��ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void InitializeUser()
        {
            try
            {
                string strExp = "select username from �û�";
                DataTable dt = GetTable(strExp);
                textBoxUser.Items.Clear();
                if (dt.Rows.Count>0)
                {
                    textBoxUser.Items.Add("ȫ��");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        textBoxUser.Items.Add(dt.Rows[i][0].ToString());
                    }
                    textBoxUser.Text = textBoxUser.Items[0].ToString();
                }

                dateFrom.Text = DateTime.Now.ToString();
                dateTo.Text = DateTime.Now.ToString();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "InitializeUser");
            }
        }

        /// <summary>
        /// ��ѡ��ѡ��������ʱ������ ���� ����ʱ����Ϊ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dateFrom.Enabled = checkBox1.Checked;
            timeFrom.Enabled = checkBox1.Checked;
            dateTo.Enabled = checkBox1.Checked;
            timeTo.Enabled = checkBox1.Checked;
        }

        /// <summary>
        /// ���ȷ������sql��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxUser.Text.Trim() == "") {
                MessageBox.Show("�������û���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                this.Cursor = Cursors.WaitCursor;
                string strExp = "select * from ������¼";
                if (textBoxUser.Text != "ȫ��") {
                    strExp += " where �û���='" + textBoxUser.Text.Trim() + "'";
                }
                if (comboBoxModule.Text != "ȫ��") {
                    if (strExp.IndexOf("where") > 0)
                        strExp += " and ����ģ�� like '" + comboBoxModule.Text + "%'";
                    else
                        strExp += " where ����ģ�� like '" + comboBoxModule.Text + "%'";
                }

                if (checkBox1.Checked)
                {
                    string d1 = dateFrom.Value.ToShortDateString() + " " + timeFrom.Value.ToShortTimeString();
                    string d2 = dateTo.Value.ToShortDateString() + " " + timeTo.Value.ToShortTimeString();
                    if (strExp.IndexOf("where") > 0)
                        strExp += " and ʱ��>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and ʱ��<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')";
                    else
                        strExp += " where ʱ��>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and ʱ��<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')";
                }
                strExp += " order by ʱ�� desc";
                DataTable dt = GetTable(strExp);
                if (dt == null || dt.Rows.Count < 1)
                {
                    MessageBox.Show("�ޱ༭��¼!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridView1.DataSource = null;
                    this.Cursor = Cursors.Default;
                    return;
                }

                dataGridView1.DataSource = dt;

                for (int i = 0; i < dataGridView1.Rows.Count; i++) {
                    if (i % 2 == 1)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                this.Cursor = Cursors.Default;
            }
            catch( Exception ex) {
                this.Cursor = Cursors.Default;
                ExToLog(ex,"��ѯ");
            }
        }

        /// <summary>
        /// ��ѯSQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(strConn[0], strConn[1], strConn[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
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
            CLC.BugRelated.ExceptionWrite(ex, "Main-frmEditLog-" + sFunc);
        }
    }
}