using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MapInfo.Windows.Controls;


namespace clKaKou
{
    public partial class frmCarInfo : Form
    {
        public frmCarInfo()
        {
            InitializeComponent();
        }

        public string[] StrCon;         // ���ݿ������ַ�������
        public string UserName;         // ��½�û�����
        public MapControl mapControl;   // ��ͼ�ؼ�
        public string getFromNamePath;  // GetFromNameConfig.ini�ĵ�ַ
        public string layerName;        // ͼ������
        public string mysql;            // �����ַ��� 

        /// <summary>
        /// ��ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dRow">������</param>
        /// <param name="Constr">���Ӳ���</param>
        /// <param name="un">�û���</param>
        internal void setInfo(DataRow dRow, string[] Constr, string un)
        {
            try
            {
                StrCon = Constr;

                UserName = un;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("�����ֶ�") < 0)
                    {
                        if (col.Caption.IndexOf("��Ƭ") < 0 && col.Caption != "���֤������")
                        {
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                        }
                        else if (col.Caption == "���֤������" && dRow[col].ToString().Length > 0)
                        {
                             //|| (col.Caption == "��Ƭ1" && dRow[col].ToString().Length > 0) || (col.Caption == "��Ƭ2" && dRow[col].ToString().Length > 0) || (col.Caption == "��Ƭ3" && dRow[col].ToString().Length > 0))
                            //{
                                DataGridViewLinkCell dglc1 = new DataGridViewLinkCell();

                                //dglc1.Value = "�鿴 " + col.Caption + " ����Ϣ";

                                if (col.Caption == "���֤������")
                                    dglc1.Value = dRow[col].ToString();

                                dglc1.ToolTipText = "�鿴�˿���ϸ��Ϣ";

                                this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);

                                this.dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[1] = dglc1;
                            //}
                        }
                    }
                }
                //this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //����λ��

                int ki = 0;
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].ToString().IndexOf("����״̬") > 0)
                    {
                        ki = i;
                    }
                }

                this.TopMost = true;

                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }

                this.setSize();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-01-setInfo");
            }
        }

        /// <summary>
        /// ���ô����С
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void setSize()
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].Width > 300)
                    {
                        this.dataGridView1.Columns[1].Width = 300;
                        this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    double width = this.dataGridView1.Columns[1].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView1.Rows[i].Cells[1].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView1.Rows[i].Height = (this.dataGridView1.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + 30;

                if (this.dataGridView1.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView1.Rows[i].Height;
                }
                cMessageHeight += 50;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //���ô�С
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmCarInfo-02-setSize");
            }
        }

        /// <summary>
        /// ���ô���λ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="iWidth">������</param>
        /// <param name="iHeight">����߶�</param>
        /// <param name="x">x����</param>
        /// <param name="y">y����</param>
        private void setLocation(int iWidth, int iHeight, int x, int y)
        {
            try
            {
                if (x + iWidth > Screen.PrimaryScreen.WorkingArea.Width)
                {
                    x = x - iWidth - 10;
                }
                if (y + iHeight > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    y = y - iHeight - 10;
                    if (y < 0) y = 0;
                }
                this.SetDesktopLocation(x, y);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmCarInfo-setLocation");
            }
        }

        /// <summary>
        /// �б����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "���֤������:")
            {

                try
                {
                    MapInfo.Geometry.DPoint dp = new MapInfo.Geometry.DPoint();
                   
                    CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", Application.StartupPath + "\\GetFromNameConfig.ini");
                    string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                    string sFno = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();

                    if (sFno.Length == 15)
                    {
                        sFno = oldToNew(sFno);
                    }

                    string strSQL = "select " + sqlFields + " from �˿�ϵͳ t where ���֤����='" + sFno + "'";

                    DataTable datatable = this.GetTable(strSQL);

                    if (datatable.Rows.Count > 0)
                    {
                        System.Drawing.Point pt = new System.Drawing.Point();
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;

                        this.disPlayInfo(datatable, pt);
                    }
                    else
                    {
                        MessageBox.Show("û�в�ѯ�����Ӧ���˿���Ϣ","ϵͳ��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    }
                    //WriteEditLog("���֤����='" + dataGridView1.Rows[e.RowIndex].Cells["���֤����"].Value.ToString() + "'", "�鿴����");
                }
                catch (Exception ex)
                {
                    writeToLog(ex, " ucKakou-dataGridView1_CellContentClick-��ʾ������ϸ��Ϣ");
                }
            }
        }

        /// <summary>
        /// �����֤������15λתΪ18λ
        /// </summary>
        /// <param name="id">15λ���֤����</param>
        /// <returns>18λ���֤����</returns>
        private string oldToNew(string id)
        {
            try
            {
                if (id == null || id == "")
                {

                    return "";

                }
                else
                {

                    if (id.Length == 18) { return id; }
                    else
                    {

                        int[] W ={ 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2, 1 };

                        string[] A ={ "1", "0", "X", "9", "8", "7", "6", "5", "4", "3", "2" };

                        int i, j, s = 0;

                        string newid;

                        newid = id;

                        newid = newid.Substring(0, 6) + "19" + newid.Substring(6, id.Length-6);

                        for (i = 0; i < newid.Length; i++)
                        {

                            j = Int32.Parse(newid.Substring(i,1)) * W[i];
                            //Integer.parseInt(

                            s = s + j;

                        }

                        s = s % 11;

                        newid = newid + A[s];

                        return newid;

                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, " ucKakou-frmCarInfo-oldToNew");
                return "";
            }

        }

        private clPopu.FrmHouseInfo frminfo = new clPopu.FrmHouseInfo();
        /// <summary>
        /// ��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="pt">����</param>
        private void disPlayInfo(DataTable dt,Point pt)
        {
            try
            {
                if (this.frminfo.Visible == false)
                {
                    this.frminfo = new clPopu.FrmHouseInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.Visible = false;
                }

                frminfo.mapControl = mapControl;
                frminfo.LayerName = this.layerName;
                frminfo.getFromNamePath = getFromNamePath;
                frminfo.strConn = this.mysql;
                frminfo.setInfo(dt.Rows[0],pt,this.UserName);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " ucKakou-21-��ʾ������ϸ��Ϣ");
            }
        }


        /// <summary>
        /// ��ѯSQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>�����</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ִ��SQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">sql���</param>
        /// <param name="method">������ʽ</param>
        /// <param name="tablename">����</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + UserName + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�ΰ�����','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-06-��¼������¼");
            }
        }

    }
}