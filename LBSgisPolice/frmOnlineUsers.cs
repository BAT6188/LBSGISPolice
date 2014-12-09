using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Data.OracleClient;
using CLC;

namespace LBSgisPolice
{
    public partial class frmOnlineUsers : Form
    {
        public string strConn = "";

        /// <summary>
        /// ���캯��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public frmOnlineUsers()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �����¼� ��ѯ�������û�����ʾ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void frmOnlineUsers_Load(object sender, EventArgs e)
        {
            try
            {
                OracleConnection Conn = new OracleConnection(strConn);
                Conn.Open();

                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = "select count(*) from �û� where ����=1";
                int iCount = Convert.ToInt32(Cmd.ExecuteScalar().ToString());
                //int iCount = CLC.DatabaseRelated.OracleDriver.OracleComScalar("select count(*) from �û� where ����=1");
                label1.Text = "�� " + iCount.ToString() + " ���û�����";

                this.Height =52 + 23 * (iCount+1);
                if (this.Height > Screen.PrimaryScreen.WorkingArea.Height) {
                    this.Height = Screen.PrimaryScreen.WorkingArea.Height;
                }
                Cmd.CommandText = "select username as �û���,�û���λ from �û� where ����=1 order by username";
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                DataSet ds = new DataSet();
                Adp.Fill(ds);
                DataTable dt = ds.Tables[0];
                //DataTable dt = new DataTable();
                //dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select username as �û���,�û���λ from �û� where ����=1 order by username");
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                dataGridView1.Rows.Clear();
                dataGridView1.DataSource = dt;
                dataGridView1.Columns[0].Width = 100;
                dataGridView1.Columns[1].Width = 104;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("�鿴�����û�ʱ��������", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CLC.BugRelated.ExceptionWrite(ex, "Main-frmOnlineUser-�����û�");
            }
        }
    }
}