using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using clPopu;

using System.Data.OracleClient;
using MapInfo.Data;
using nsGetFromName;
using MapInfo.Windows.Controls;

namespace clZonghe
{
    public partial class FrmInfo : Form
    {
        //��ȡ�����ļ����������ݿ������ַ���
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                string userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                string password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");

                string connString = "user id = " + userid + ";data source = " + datasource + ";password = " + password;
                StrCon = new string[] { datasource, userid, password };
                return connString;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "getStrConn");
                return "";
            }
        }
        public string strConn;

        // ���ݿ����
        System.Data.OracleClient.OracleConnection myConnection = null;
        System.Data.OracleClient.OracleDataAdapter oracleDat = null;
        System.Data.OracleClient.OracleCommand oracleCmd = null;
        System.Data.OracleClient.OracleDataReader dataReader = null;

        public MapControl mapControl;
        private string tabName;
        public DataRow row = null;
        public string getFromNamePath;
        private string LayerName;
        private string[] StrCon;

        public FrmInfo()
        {
            try
            {
                InitializeComponent();
                frmNumber = new FrmHouseNumber();
                myConnection = new System.Data.OracleClient.OracleConnection(getStrConn());
                strConn = getStrConn();
            }
            catch (Exception ex) { ExToLog(ex, "FrmInfo-���캯��"); }
        }

        private string[] dqPopu = null, lsPopu = null, wzPopu = null, yzPopu = null;  // ����洢��ѯ�������������ѯ
        /// <summary>
        /// ɸѡ��ʾ�ֶβ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dRow">��¼��</param>
        /// <param name="pt">��ʾλ��</param>
        /// <param name="LayName">ͼ������</param>
        internal void setInfo(DataRow dRow,Point pt,string LayName)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                row = dRow;
                LayerName = LayName;
                string houseNo = "";   // ��÷��ݱ��

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("�����ֶ�") < 0 && col.Caption != "�����ɳ�������"     
                                                            && col.Caption != "�����жӴ���"  
                                                            && col.Caption != "���������Ҵ���" 
                                                            && col.Caption != "��ȡID" 
                                                            && col.Caption != "��ȡ����ʱ��" 
                                                            && col.Caption != "��������" 
                                                            && col.Caption != "�������֤����" 
                                                            && col.Caption != "��ż������ݺ���" 
                                                            && col.Caption != "סַ����")
                    {
                        if (col.Caption == "�永��Ա" || col.Caption == "��ذ���" 
                                                      || (col.Caption == "���ݱ��" && col.Table.Columns[0].Caption != "���ݱ��") 
                                                      || col.Caption == "��ǰ��ס����" 
                                                      || col.Caption == "��ʷ��ס����" 
                                                      || col.Caption == "��ס֤��Ч��������"
                                                      || col.Caption == "δ����ס֤����")
                            this.dataGridView1.Rows.Add(col.Caption + ":", "");
                        else
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                    if (col.Table.Columns[0].Caption == "���ݱ��")
                        houseNo = dRow.Table.Rows[0]["���ݱ��"].ToString();
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //����λ��
                this.Visible = true;
                string strPopu = "";   // ����ͳ�Ƶ�sql;

                //���洢ĳ�е�λ��
                #region �������ӣ��໥����
                ////////////////------�������ӣ��໥����-------/////////////////////
                int k = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "�永��Ա:" || dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ذ���:" 
                                                                                       || dataGridView1.Rows[i].Cells[0].Value.ToString() == "���ݱ��:" 
                                                                                       || dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ȫ������λ�ļ�:")
                    {
                        k = i;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ǰ��ס����:")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        dqPopu = getPopuCount(strPopu, dqPopu);

                        //dqPopu = new string[] { };
                        //dqPopu = row["��ǰ��ס����"].ToString().Split(',');

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (dqPopu[0] == "" || dqPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = dqPopu.Length.ToString();
                        Dgvlink.ToolTipText = "�鿴��ǰ��ס����ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ʷ��ס����:")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        lsPopu = getPopuCount(strPopu, lsPopu);

                        //lsPopu = new string[] { };
                        //lsPopu = row["��ʷ��ס����"].ToString().Split(','); 

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (lsPopu[0] == "" || lsPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = lsPopu.Length.ToString();
                        Dgvlink.ToolTipText = "�鿴��ʷ��ס����ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ס֤��Ч��������:")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and �˿�����='��ס�˿�' and ��ס֤�� is not null and ��ס֤��Ч����>sysdate";
                        yzPopu = getPopuCount(strPopu, yzPopu);

                        //yzPopu = new string[] { };
                        //yzPopu = row["��ס֤��Ч��������"].ToString().Split(',');

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (yzPopu[0] == "" || yzPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = yzPopu.Length.ToString();
                        Dgvlink.ToolTipText = "�鿴��ס֤��Ч��������ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "δ����ס֤����:")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and �˿�����='��ס�˿�' and ((��ס֤�� is null) or (��ס֤�� is not null and ��ס֤��Ч����<sysdate))";
                        wzPopu = getPopuCount(strPopu, wzPopu);

                        //wzPopu = new string[] { };
                        //wzPopu = row["δ����ס֤����"].ToString().Split(',');

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (wzPopu[0] == "" || wzPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = wzPopu.Length.ToString();
                        Dgvlink.ToolTipText = "�鿴δ����ס֤����ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "��������:")
                    {
                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        Dgvlink.Value = row["��������"].ToString();
                        Dgvlink.ToolTipText = "�鿴��������ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ż����:")
                    {
                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        Dgvlink.Value = row["��ż����"].ToString();
                        Dgvlink.ToolTipText = "�鿴��ż��ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                }

                DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                if (dataGridView1.Rows[k].Cells[0].Value.ToString() == "�永��Ա:")
                {
                    dgvlc.Value = "�鿴����永��Ա��Ϣ";
                    dgvlc.ToolTipText = "�鿴����永��Ա��Ϣ";
                    this.dataGridView1.Rows[k].Cells[1] = dgvlc;
                }
                if (dataGridView1.Rows[k].Cells[0].Value.ToString() == "��ذ���:")
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "���ݱ��:")
                        {
                            DataGridViewLinkCell dgvlc1 = new DataGridViewLinkCell();
                            dgvlc1.Value = "�鿴������ϸ��Ϣ";
                            dgvlc1.ToolTipText = "�鿴������ϸ��Ϣ";
                            this.dataGridView1.Rows[i].Cells[1] = dgvlc1;
                            break;
                        }
                        DataGridViewLinkCell dgvlc2 = new DataGridViewLinkCell();
                        dgvlc2.Value = dRow[dRow.Table.Columns[0]];
                        dgvlc2.ToolTipText = "�鿴��Ƭ";
                        this.dataGridView1.Rows[0].Cells[1] = dgvlc2;
                    }
                    dgvlc.Value = "�鿴������ϸ��Ϣ";
                    dgvlc.ToolTipText = "�鿴������ϸ��Ϣ";
                    this.dataGridView1.Rows[k].Cells[1] = dgvlc;
                }
                if (dataGridView1.Rows[k].Cells[0].Value.ToString() == "��ȫ������λ�ļ�:")
                {
                    dgvlc.Value = "�鿴�ļ�";
                    dgvlc.ToolTipText = "�鿴��ȫ������λ�ļ�";
                    this.dataGridView1.Rows[k].Cells[1] = dgvlc;
                }
                ////////////////////////////////////////////////////////////////////////
                #endregion
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }
        }

        /// <summary>
        /// ����sql��ѯ������������浽������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="poStr">sql���</param>
        /// <param name="conPo">���ڴ������������</param>
        /// <returns></returns>
        private string[] getPopuCount(string poStr, string[] conPo)
        {
            try
            {
                DataTable poDat = GetTable(poStr);

                if (poDat.Rows.Count > 0)
                    conPo = new string[poDat.Rows.Count];
                else
                {
                    conPo = new string[1];
                    conPo[0] = "0";
                    return conPo;
                }

                for (int i = 0; i < poDat.Rows.Count; i++)
                {
                    conPo[i] = poDat.Rows[i][0].ToString();
                }

                return conPo;

            }
            catch (Exception ex)
            {
                ExToLog(ex, "getPopuCount-����sql��ѯ������������浽������");
                return conPo;
            }
        }

        /// <summary>
        /// ����ת�����ݱ�Ŵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="str">Ҫת���ı��</param>
        /// <returns>ת�����ַ���</returns>
        private string ConversionStr(string str)
        {
            try
            {
                string converStr = "";

                switch (str.Substring(0, 1))
                {
                    case "a":
                        converStr = "����";
                        break;
                    case "b":
                        converStr = "�����ݱ��";
                        break;
                    case "c":
                        converStr = "סַ���ƴ���";
                        break;
                    case "d":
                        converStr = "��ҵ���";
                        break;
                    default:
                        break;
                }
                return converStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ConversionStr-ת�����ݱ�Ŵ���");
                return "";
            }
        }

        /// <summary>
        /// ���ô����С
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void setSize() {
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
                ExToLog(ex, "setSize");
            }
        }

        /// <summary>
        /// ���ô���λ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="iWidth">������</param>
        /// <param name="iHeight">����߶�</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        private void setLocation(int iWidth,int iHeight,int x,int y) {
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
                ExToLog(ex, "setLocation");
            }
        }


        // �����¼�
        private FrmZLMessage frmZL;
        private FrmHouseNumber frmNumber;
        private string[] strAnjan;
        private string tableName = "";
        /// <summary>
        /// �����б��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (strConn == "")
                {
                    MessageBox.Show("��ȡ�����ļ�ʱ��������,���޸������ļ�������!");
                    return;
                }
                if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "����:")
                {
                    int k = 0, r = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "���֤����:")
                        {
                            k = i;
                        }
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "�˿�����:")
                        {
                            r = i;
                        }
                    }
                    System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=czrk_cx;Password=czrk_cx;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.47.227.15)(PORT=1521)))(CONNECT_DATA=(SID=ora81)))");
                    try
                    {
                        clZonghe.FrmImage fimage = new clZonghe.FrmImage();
                        oraconn.Open();
                        string sqlstr = "";
                        if (dataGridView1.Rows[r].Cells[1].Value.ToString() == "��ס�˿�")
                            sqlstr = "select TX from czrk_cx.v_gis_czrk_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";
                        else
                            sqlstr = "select TX from czrk_cx.v_gis_ldry_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";

                        oracleCmd = new OracleCommand(sqlstr, oraconn);
                        dataReader = oracleCmd.ExecuteReader();
                        if (dataReader.Read())
                        {
                            byte[] bytes = new byte[(dataReader.GetBytes(0, 0, null, 0, int.MaxValue))];
                            if (dataReader.IsDBNull(0))
                            {
                                System.Windows.Forms.MessageBox.Show("��Ƭδ�浵!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, bytes.Length);
                                dataReader.Close();

                                Stream fs = new MemoryStream(bytes);
                                //����һ��bitmap���͵�bmp��������ȡ�ļ���
                                Bitmap bmp = new Bitmap(Image.FromStream(fs));

                                bmp.Save(Application.StartupPath + "\\Zhonghe.jpg");
                                fimage.pictureBox1.ImageLocation = Application.StartupPath + "\\Zhonghe.jpg";
                                bmp.Dispose();//�ͷ�bmp�ļ���Դ
                                fimage.pictureBox1.Invalidate();
                                fs.Close();
                                fimage.TopMost = true;
                                fimage.ShowDialog();
                                fimage.Dispose();
                                File.Delete(Application.StartupPath + "\\Zhonghe.jpg");
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("��Ƭδ�浵!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        if (oraconn.State == ConnectionState.Open)
                        {
                            oracleCmd.Dispose();
                            oraconn.Close();
                        }
                    }
                }

                tabName = ""; strAnjan = null;

                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "�永��Ա:")
                    tabName = "�永��Ա:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ذ���:")
                    tabName = "��ذ���:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ȫ������λ�ļ�:")
                    tabName = "��ȫ������λ�ļ�:";
                if (row.Table.Columns[0].Caption == "����" && dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "���ݱ��:")
                    tabName = "���ݱ��:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ǰ��ס����:")
                    tabName = "��ǰ��ס����:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ʷ��ס����:")
                    tabName = "��ʷ��ס����:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ס֤��Ч��������:")
                    tabName = "��ס֤��Ч��������:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "δ����ס֤����:")
                    tabName = "δ����ס֤����:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ż����:")
                    tabName = "��ż������ݺ���:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��������:")
                    tabName = "�������֤����:";

                string strSQL = "", strMessage = "";
                DataTable table = new DataTable();
                try
                {
                    string colName = tabName.Substring(0, tabName.Length - 1);
                    switch (colName)
                    {
                        case "��ذ���":
                            string AnName = row[colName].ToString();
                            strAnjan = AnName.Split(',');
                            if (AnName != "")
                            {
                                AnName = AnName.Replace(",", "','");
                                strSQL = "select �������,�������� from ������Ϣ t where ������� in ('" + AnName + "')";
                            }
                            tableName = "������Ϣ";
                            strMessage = "���永��¼��";
                            break;
                        case "���ݱ��":
                            string houseNo = row[colName].ToString();
                            strAnjan = houseNo.Split(',');
                            if (houseNo != "")
                            {
                                houseNo = houseNo.Replace(",", "','");
                                strSQL = "select ���ݱ��,�������� from �����ݷ���ϵͳ t where ���ݱ�� in ('" + houseNo + "')";
                            }
                            tableName = "�����ݷ���ϵͳ";
                            strMessage = "����δ�浵��";
                            break;
                        case "�永��Ա":
                            string anRen = row[colName].ToString();
                            strAnjan = anRen.Split(',');
                            if (anRen != "")
                            {
                                anRen = anRen.Replace(",", "','");
                                strSQL = "select ���֤����,���� from �˿�ϵͳ t where ���֤���� in ('" + anRen + "')";
                            }
                            tableName = "�˿�ϵͳ";
                            string renName = colName.Substring(0, colName.Length - 1);
                            strMessage = renName + "δ�浵��";
                            break;
                        case "��ǰ��ס����":
                        case "��ס֤��Ч��������":
                        case "��ʷ��ס����":
                        case "δ����ס֤����":
                            string cardId = arrayConStr(colName);
                            strAnjan = cardId.Split(',');
                            if (cardId != "")
                            {
                                cardId = cardId.Replace(",", "','");
                                strSQL = "select ���֤����,���� from �˿�ϵͳ t where ���֤���� in ('" + cardId + "')";
                            }
                            tableName = "�˿�ϵͳ";
                            string meName = colName.Substring(0, colName.Length - 1);
                            strMessage = meName + "δ�浵��";
                            break;
                        case "��ȫ������λ�ļ�":
                            if (this.frmZL != null)
                            {
                                if (this.frmZL.Visible == true)
                                {
                                    this.frmZL.Close();
                                }
                            }

                            if (dataGridView1.Rows[1].Cells[1].Value.ToString() == "")
                            {
                                MessageBox.Show("���Ʋ���Ϊ�գ�", "��ʾ");
                                return;
                            }
                            this.frmZL = new FrmZLMessage(dataGridView1.Rows[1].Cells[1].Value.ToString(), strConn);
                            //������Ϣ�������½�  
                            System.Drawing.Point po = new Point();
                            po.X = Screen.PrimaryScreen.WorkingArea.Width;
                            po.Y = Screen.PrimaryScreen.WorkingArea.Height;
                            this.frmZL.SetDesktopLocation(po.X - frmZL.Width, po.Y - frmZL.Height);
                            this.frmZL.Show();
                            break;
                        case "��ż������ݺ���":
                        case "�������֤����":
                            tableName = "�˿�ϵͳ";
                            GetAnjanInfo();
                            strMessage = "δ��ѯ���˼�¼�������ȷ�����ݿ������֤�����Ƿ���ȷ!";
                            break;
                    }
                    if (strSQL != "")
                    {
                        myConnection.Open();
                        oracleDat = new OracleDataAdapter(strSQL, myConnection);
                        oracleDat.Fill(table);

                        if (table.Rows.Count == 0)
                        {
                            MessageBox.Show(strMessage, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            return;
                        }
                    }
                    else
                    {
                        if (tabName != "" && tabName != "��ȫ������λ�ļ�:" && tabName != "��ż������ݺ���:" && tabName != "�������֤����:")
                            MessageBox.Show(strMessage, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (strAnjan.Length > 1)
                        disPlayInfo(table, tableName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (strSQL != "")
                        oracleDat.Dispose();
                    myConnection.Close();
                    try
                    {
                        // ���ֻ��һ����¼��ֱ����ʾ��Ϣ
                        if (strAnjan != null && strAnjan.Length == 1 && strAnjan[0] != "")
                            GetAnjanInfo();
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick");
            }
        }

        /// <summary>
        /// ���������ҵ��ַ�������ת��Ϊ�ɹ�SQLʹ�õ��ַ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="colName">����</param>
        /// <returns>sql</returns>
        private string arrayConStr(string colName)
        {
            try
            {
                string[] constrArray = null;
                switch (colName)
                {
                    case "��ǰ��ס����":
                        constrArray = dqPopu;
                        break;
                    case "��ס֤��Ч��������":
                        constrArray = yzPopu;
                        break;
                    case "��ʷ��ס����":
                        constrArray = lsPopu;
                        break;
                    case "δ����ס֤����":
                        constrArray = wzPopu;
                        break;
                    default:
                        break;
                }

                string arrayStr = "";

                if (constrArray.Length > 0)
                {
                    for (int j = 0; j < constrArray.Length; j++)
                    {
                        if (arrayStr == "")
                            arrayStr = constrArray[j];
                        else
                            arrayStr += "," + constrArray[j];
                    }
                }
                else
                {
                    arrayStr = "";
                }
                //arrayStr = arrayStr.Replace(",", "','");

                return arrayStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "arrayConStr");
                return "";
            }
        }

        /// <summary>
        /// ��ʾ�б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="tableName">����</param>
        private void disPlayInfo(DataTable dt,string tableName)
        {
            try
            {
                this.frmNumber = new FrmHouseNumber();
                if (this.frmNumber.Visible == false)
                {
                    frmNumber.Show();
                    frmNumber.Visible = true;
                }
                frmNumber.mapControl = mapControl;
                frmNumber.LayerName = LayerName;
                frmNumber.getFromNamePath = getFromNamePath;
                frmNumber.strConn = strConn;
                frmNumber.setfrmInfo(dt,tableName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// ������¼�Ĳ�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void GetAnjanInfo()
        {
            string strSQL = "";
            try
            {
                DataTable table = new DataTable();
                string colName = tabName.Substring(0,tabName.Length - 1);
                switch (colName) 
                {
                    case "��ذ���":
                        CLC.ForSDGA.GetFromTable.GetFromName("������Ϣ", getFromNamePath);
                        string AnNumber = row[colName].ToString();
                        if (AnNumber != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID+ "='" + AnNumber + "'";
                        break;
                    case "���ݱ��":
                        CLC.ForSDGA.GetFromTable.GetFromName("�����ݷ���ϵͳ", getFromNamePath);
                        string houseNo = row[colName].ToString();
                        if (houseNo != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" + houseNo + "'";
                        break;
                    case "�永��Ա":
                        CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", getFromNamePath);
                        string anRen = row[colName].ToString();
                        if (anRen != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" + anRen + "'";
                        break;
                    case "��ǰ��ס����":
                    case "��ʷ��ס����":
                    case "��ס֤��Ч��������":
                    case "δ����ס֤����":
                        CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", getFromNamePath);
                        string cardId = arrayConStr(colName);
                        if (cardId != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where "
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" + cardId + "'";
                        break;
                    case "��ż������ݺ���":
                    case "�������֤����":
                        CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", getFromNamePath);
                        string sfzID = row[colName].ToString();
                        if (sfzID != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" + sfzID + "'";
                        break;
                }

                if (strSQL != "")
                {
                    myConnection.Open();
                    oracleDat = new System.Data.OracleClient.OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(table);
                }
                else if (table.Rows.Count <= 0)
                {
                    MessageBox.Show("�����¼δ�浵��", "��ʾ");
                    return;
                }
                Point pt = new Point();
                pt.X = Convert.ToInt32(table.Rows[0]["X"]);
                pt.Y = Convert.ToInt32(table.Rows[0]["Y"]);

                disPlayInfo(table, pt, "�˿�ϵͳ");
            }
            catch(Exception ex)
            {
                ExToLog(ex, "GetAnjanInfo");
            }
            finally
            {
                oracleDat.Dispose();
                myConnection.Close();
            }
        }
         
        /// <summary>
        /// ��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private FrmHouseInfo frmglMessage = new FrmHouseInfo();
        private void disPlayInfo(DataTable dt, Point pt,string tableName)
        {
            try
            {
                if (this.frmglMessage.Visible == false)
                {
                    this.frmglMessage = new FrmHouseInfo();
                    frmglMessage.Show();
                    frmglMessage.Visible = true;
                }
                frmglMessage.mapControl = mapControl;
                frmglMessage.LayerName = LayerName;
                frmglMessage.getFromNamePath = getFromNamePath;
                frmglMessage.strConn = strConn;
                frmglMessage.setInfo(dt.Rows[0], pt,tableName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// ��ȡ��ѯ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">sql���</param>
        /// <returns>��ѯ�����</returns>
        private DataTable GetTable(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                return dt;
            }
            catch (Exception ex)
            { ExToLog(ex, "GetTable"); return null; }
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
            CLC.BugRelated.ExceptionWrite(ex, "clZonghe-FrmInfo-" + sFunc);
        }
    }
}

