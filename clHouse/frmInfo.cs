using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.Xml;
using clHouse;
using clPopu;
using clInfo;


namespace nsInfo
{
    public partial class FrmInfo : Form
    {
        /// <summary>
        /// ��ȡ�����ļ����������ݿ������ַ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <returns>�����ַ���</returns>
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                string userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                string password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");

                string connString = "user id = " + userid + ";data source = " + datasource + ";password =" + password;
                StrCon = new string[] { datasource, userid, password };
                return connString;
            }
            catch(Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.getStrConn");
                return "";
            }
        }
        private string[] StrCon;              // ���Ӳ���
        public string strConn;                // �����ַ���
        public MapInfo.Windows.Controls.MapControl mapControl = null; // ��ͼ�ؼ�
        private DataRow row = null;           // ������
        public string getFromNamePath;        // �����ļ�GetFromNameConfig.ini�ĵ�ַ
        private string LayerName;             // ͼ����
        public FrmInfo()
        {
            InitializeComponent();
            strConn = getStrConn();
        }

        private string[] lsPopu = null, dqPopu = null, wzPopu = null, yzPopu = null;     // �����ʷ��ס��������ǰ��ס����,��ס��Ч��������,�Ѱ���ס���������֤����

        /// <summary>
        /// ��ʼ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dRow">������</param>
        /// <param name="pt">��ʾλ��</param>
        /// <param name="LayName">ͼ����</param>
        public void setInfo(DataRow dRow, Point pt,string LayName)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                row = dRow;
                LayerName = LayName;
                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("�����ֶ�") < 0 && col.Caption != "�����ɳ�������"
                                                            && col.Caption != "�����жӴ���"
                                                            && col.Caption != "���������Ҵ���"
                                                            && col.Caption != "��ȡID"
                                                            && col.Caption != "��ȡ����ʱ��"
                                                            && col.Caption != "��������"
                                                            && col.Caption != "�������֤����")
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
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //����λ��
                this.Visible = true;

                #region ������ӣ��໥����3
                DataGridViewLinkCell dgvlc = null, dgvlc3 = null, dgvlc1 = null, dgvlc2 = null, dgvlc4 = null;
                string strPopu = "";   // ����ͳ�Ƶ�sql;
                string houseNo = dRow.Table.Rows[0][0].ToString();   // ��÷��ݱ��

                for (int i = 0; i < dRow.Table.Columns.Count; i++)
                {
                    if (dRow.Table.Columns[i].Caption == "��ס֤��Ч��������")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and �˿�����='��ס�˿�' and ��ס֤�� is not null and ��ס֤��Ч����>sysdate";
                        wzPopu = getPopuCount(strPopu, wzPopu);

                        //wzPopu = new string[] { };
                        //wzPopu = dRow.Table.Rows[0][i].ToString().Split(',');

                        // �������ӣ��ɲ鿴��ס��Ч��������ϸ��Ϣ
                        dgvlc = new DataGridViewLinkCell();
                        if (wzPopu[0] == "" || wzPopu[0] == "0")
                            dgvlc.Value = "0";
                        else
                            dgvlc.Value = wzPopu.Length.ToString();
                        dgvlc.ToolTipText = "�鿴��ס֤��Ч��������ϸ��Ϣ";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "δ����ס֤����")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and �˿�����='��ס�˿�' and ((��ס֤�� is null) or (��ס֤�� is not null and ��ס֤��Ч����<sysdate))";
                        yzPopu = getPopuCount(strPopu, yzPopu);

                        //yzPopu = new string[] { };
                        //yzPopu = dRow.Table.Rows[0][i].ToString().Split(',');

                        // �������ӣ��ɲ鿴δ����ס֤����ϸ��Ϣ
                        dgvlc3 = new DataGridViewLinkCell();
                        if (yzPopu[0] == "" || yzPopu[0] == "0")
                            dgvlc3.Value = "0";
                        else
                            dgvlc3.Value = yzPopu.Length.ToString();
                        dgvlc3.ToolTipText = "�鿴δ����ס֤����ϸ��Ϣ";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc3;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "��ʷ��ס����")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        lsPopu = getPopuCount(strPopu, lsPopu);

                        //lsPopu = new string[] { };
                        //lsPopu = dRow.Table.Rows[0][i].ToString().Split(',');
                        // �������ӣ��ɲ鿴��ʷ��ס����ϸ��Ϣ
                        dgvlc1 = new DataGridViewLinkCell();
                        if (lsPopu[0] == "" || lsPopu[0] == "0")
                            dgvlc1.Value = "0";
                        else
                            dgvlc1.Value = lsPopu.Length.ToString();
                        dgvlc1.ToolTipText = "�鿴��ʷ��ס����ϸ��Ϣ";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc1;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "��ǰ��ס����")
                    {
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        dqPopu = getPopuCount(strPopu, dqPopu);

                        //dqPopu = new string[] { };
                        //dqPopu = dRow.Table.Rows[0][i].ToString().Split(',');

                        // �������ӣ��ɲ鿴��ǰ��ס����ϸ��Ϣ
                        dgvlc2 = new DataGridViewLinkCell();
                        if (dqPopu[0] == "" || dqPopu[0] == "0")
                            dgvlc2.Value = "0";
                        else
                            dgvlc2.Value = dqPopu.Length.ToString();
                        dgvlc2.ToolTipText = "�鿴��ǰ��ס����ϸ��Ϣ";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc2;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "��������")
                    {
                        dgvlc4 = new DataGridViewLinkCell();
                        dgvlc4.Value = row["��������"].ToString();
                        dgvlc4.ToolTipText = "�鿴������ϸ��Ϣ";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc4;
                    }
                }
                for (int j = 0; j < this.dataGridView1.Rows.Count; j++)
                {
                    switch (this.dataGridView1.Rows[j].Cells[0].Value.ToString())
                    {
                        case "��ס֤��Ч��������:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc;
                            break;
                        case "δ����ס֤����:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc3;
                            break;
                        case "��ʷ��ס����:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc1;
                            break;
                        case "��ǰ��ס����:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc2;
                            break;
                        case "��������:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc4;
                            break;
                    }
                }
                #endregion

                this.Visible = true;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.setInfo");
            }
        }

        /// <summary>
        /// ����sql��ѯ������������浽������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="poStr">sql���</param>
        /// <param name="conPo">���ڴ������������</param>
        /// <returns></returns>
        private string[] getPopuCount(string poStr, string[] conPo)
        {
            try {
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
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.getPopuCount");
                return conPo;
            }
        }

        /// <summary>
        /// ����ת�����ݱ�Ŵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.ConversionStr");
                return "";
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
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.setSize");
            }
        }

        /// <summary>
        /// ���ô���λ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="iWidth">������</param>
        /// <param name="iHeight">����߶�</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
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
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.setLocation");
            }
        }

        /// <summary>
        /// �����б��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (strConn == "")
            {
                MessageBox.Show("��ȡ�����ļ�ʱ��������,���޸������ļ�������!");
                return;
            }

            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter oracleDat = null;
            DataSet objset = new DataSet();

            if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ס֤��Ч��������:")
            {
                if (wzPopu[0] == "" || wzPopu[0] == "0")
                    return;
                string wzPopus = arrayConStr(wzPopu);

                try
                {
                    string strSQL = "select ���֤����,���� from �˿�ϵͳ where ���֤���� in ('" + wzPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL,myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("��ס��δ�浵��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (wzPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "�˿�ϵͳ");
                }
                catch (Exception ex)
                {
                    CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.dataGridView1_CellContentClick");
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    oracleDat.Dispose();
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try 
                    {
                        if (wzPopu.Length == 1 && wzPopu[0] != "")
                            poupDisplayInfo(wzPopus);
                    }
                    catch { }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "δ����ס֤����:")
            {
                if (yzPopu[0] == "" || yzPopu[0] == "0")
                    return;
                string yzPopus = arrayConStr(yzPopu);

                try
                {
                    string strSQL = "select ���֤����,���� from �˿�ϵͳ where ���֤���� in ('" + yzPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("��ס��δ�浵��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (yzPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "�˿�ϵͳ");
                }
                catch (Exception ex)
                {
                    CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.dataGridView1_CellContentClick");
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    oracleDat.Dispose();
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try
                    {
                        if (yzPopu.Length == 1 && yzPopu[0] != "" && yzPopu[0] != "0")
                            poupDisplayInfo(yzPopus);
                    }
                    catch (Exception ex)
                    {
                        CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.dataGridView1_CellContentClick");
                    }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ʷ��ס����:")
            {
                if (lsPopu[0] == "" || lsPopu[0] == "0")
                    return;
                string lsPopus = arrayConStr(lsPopu); 
               
                try
                {
                    string strSQL = "select ���֤����,���� from �˿�ϵͳ where ���֤���� in ('" + lsPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("��ס��δ�浵��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (lsPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "�˿�ϵͳ");
                }
                catch (Exception ex)
                {
                    CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.dataGridView1_CellContentClick");
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    oracleDat.Dispose();
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try
                    {
                        if (lsPopu.Length == 1 && lsPopu[0] != "" && lsPopu[0] != "0")
                            poupDisplayInfo(lsPopus);
                    }
                    catch(Exception ex)
                    {
                        CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.dataGridView1_CellContentClick");
                    }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ǰ��ס����:")
            {
                if (dqPopu[0] == "" || dqPopu[0] == "0")
                    return;
                string dqPopus = arrayConStr(dqPopu); 
                
                try
                {
                    string strSQL = "select ���֤����,���� from �˿�ϵͳ where ���֤���� in ('" + dqPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("��ס��δ�浵��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (dqPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "�˿�ϵͳ");
                }
                catch (Exception ex)
                {
                    CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.dataGridView1_CellContentClick");
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    oracleDat.Dispose();
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try
                    {
                        if (dqPopu.Length == 1 && dqPopu[0] != "" && dqPopu[0] != "0")
                            poupDisplayInfo(dqPopus);
                    }
                    catch(Exception ex)
                    {
                        CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.dataGridView1_CellContentClick");
                    }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��������:")
            {
                poupDisplayInfo(row["�������֤����"].ToString());
            }
        }

        /// <summary>
        /// �ַ�������ת��Ϊ�ɹ�SQLʹ�õ��ַ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="constrArray">�ַ�������</param>
        /// <returns>�ɹ�SQLʹ�õ��ַ���</returns>
        private string arrayConStr(string[] constrArray)
        {
            try
            {
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
                arrayStr = arrayStr.Replace(",", "','");

                return arrayStr;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.arrayConStr");
                return "";
            }
        }

        /// <summary>
        /// ��Ϊһ����¼��ʾʱ�ô˷���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="popuNo">�˿ڱ��</param>
        private void poupDisplayInfo(string popuNo)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter oracleDat = null;
            DataSet objset = new DataSet();
            try
            {
                myConnection.Open();
                CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", getFromNamePath);
                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from �˿�ϵͳ t where ���֤����='" + popuNo + "'";
                oracleDat = new OracleDataAdapter(strSQL, myConnection);
                oracleDat.Fill(objset, "table");

                // �ж��Ƿ��м�¼
                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("���֤���벻��ȷ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }

                disPlayInfo(objset.Tables[0], pt, "�˿�ϵͳ");
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.poupDisplayInfo");
            }
            finally
            {
                oracleDat.Dispose();
                myConnection.Close();
            }
        }

        //private FrmHouseNumber frmNumeber = new FrmHouseNumber();
        //private void disPlayNumberInfo(DataTable dt, string tabName)
        //{
        //    try
        //    {
        //        if (this.frmNumeber.Visible == false)
        //        {
        //            this.frmNumeber = new FrmHouseNumber();
        //            frmNumeber.Show();
        //            frmNumeber.Visible = true;
        //        }
        //        frmNumeber.mapControl = mapControl;
        //        frmNumeber.LayerName = LayerName;
        //        frmNumeber.getFromNamePath = getFromNamePath;
        //        frmNumeber.strConn = strConn;
        //        frmNumeber.setfrmInfo(dt, tabName);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.disPlayNumberInfo");
        //    }
        //}

        private frmNumber frmNumeber = new frmNumber();
        /// <summary>
        /// ��ʾ���б��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="tabName">����</param>
        private void disPlayNumberInfo(DataTable dt, string tabName)
        {
            try
            {
                if (frmNumeber.Visible == false)
                {
                    frmNumeber = new frmNumber();
                    frmNumeber.Show();
                    frmNumeber.Visible = true;
                }
                frmNumeber.mapControl = mapControl;
                frmNumeber.LayerName = LayerName;
                frmNumeber.getFromNamePath = getFromNamePath;
                frmNumeber.StrCon = StrCon;
                frmNumeber.setInfo(dt, tabName);
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.disPlayNumberInfo");
            }
        }

        //private FrmHouseInfo frmhouse = new FrmHouseInfo();
        //private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string tableName)
        //{
        //    try
        //    {
        //        if (this.frmhouse.Visible == false)
        //        {
        //            this.frmhouse = new FrmHouseInfo();
        //            frmhouse.SetDesktopLocation(-30, -30);
        //            frmhouse.Show();
        //            frmhouse.Visible = false;
        //        }
        //        frmhouse.mapControl = mapControl;
        //        frmhouse.LayerName = LayerName;
        //        frmhouse.getFromNamePath = getFromNamePath;
        //        frmhouse.strConn = strConn;
        //        frmhouse.setInfo(dt.Rows[0], pt, tableName);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.disPlayInfo");
        //    }
        //}

        private frmInfo frmhouse = new frmInfo();
        /// <summary>
        /// ��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="pt">λ��</param>
        /// <param name="tableName">����</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt, string tableName)
        {
            try
            {
                if (frmhouse.Visible == false)
                {
                    frmhouse = new frmInfo();
                    frmhouse.SetDesktopLocation(-30, -30);
                    frmhouse.Show();
                    frmhouse.Visible = false;
                }
                frmhouse.mapControl = mapControl;
                frmhouse.LayerName = LayerName;
                frmhouse.getFromNamePath = getFromNamePath;
                frmhouse.StrCon = StrCon;
                frmhouse.tableName = tableName;
                frmhouse.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.disPlayInfo");
            }
        }

        /// <summary>
        /// ��ȡ��ѯ�������ʾ���б��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">sql���</param>
        /// <returns>�����</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }
    }
}

