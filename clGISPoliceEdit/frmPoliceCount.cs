using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using Excel;
using CommonLib;
using System.IO;

namespace clGISPoliceEdit
{
    public partial class frmPoliceCount : Form
    {
        private string strConn = "";
        private string tableName = "";
        private string[] strArr = null;
        private System.Data.DataTable DataInOut = null;  // ���뵼����Ȩ�� lili 2010-9-26
        string region1, region2;  // ��ȡ�����ɳ������ж�Ȩ��

        public frmPoliceCount(string[] listPaichusuo, string tabName, System.Data.DataTable temData)
        {
            try
            {
                InitializeComponent();
                strConn = getStrConn();
                strArr = listPaichusuo;
                tableName = tabName;
                InitialCombo();
                this.DataInOut = temData;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "���캯��");
            }
        }

        private void InitialCombo()
        {
            try
            {
                if (tableName == "�ɳ���ÿ�վ�Ա��")
                {
                    comboBox1.Enabled = false;
                    //�����ĳ�������û�,ֱ�Ӹ���Ͻ����ʼֵ
                    if (region1 != "" && region1 != "˳����")   //edit by fisher in 09-11-30
                    {
                        dataGridView1.Rows.Clear();
                        string[] pcsStr = region1.Split(',');
                        for (int i = 0; i < pcsStr.Length; i++)
                        {
                            dataGridView1.Rows.Add(1);
                            dataGridView1.Rows[i].Cells[0].Value = pcsStr[i];
                            dataGridView1.Rows[i].Cells[1].Value = 0;
                        }
                    }
                    else
                    {
                        InitialDatagridView();
                    }
                }
                else  //�ж�ÿ�վ�Ա��
                {
                    for (int i = 0; i < strArr.Length; i++)
                    {
                        comboBox1.Items.Add(strArr[i]);
                    }
                    ////�����ĳ�������û�,ֱ�Ӹ���Ͻ����ʼֵ
                    comboBox1.Text = comboBox1.Items[0].ToString();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitialCombo");
            }
        }

        private void InitialDatagridView()
        {
            try
            {
                dataGridView1.Rows.Clear();
                //dataGridView1.Rows.Add(strArr.Length);
                OracelData linkData = new OracelData(strConn);
                if (tableName == "�ж�ÿ�վ�Ա��")
                {
                    string sql = "select (select a.�ж��� from �������ж� a where a.�жӴ���=t.�жӴ���) as �ж���,��Ա���� from �ж�ÿ�վ�Ա�� t where t.����=to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd')";
                    System.Data.DataTable dt = linkData.SelectDataBase(sql);
                    dataGridView1.Rows.Add(strArr.Length);
                    if (dt != null)
                    {
                        for (int i = 0; i < strArr.Length; i++)
                        {
                            dataGridView1.Rows[i].Cells[0].Value = strArr[i];
                            DataRow[] drArr = dt.Select(dt.Columns[0].Caption + "='" + strArr[i] + "'");
                            if (drArr.Length > 0)
                            {
                                DataRow dr = drArr[0];
                                dataGridView1.Rows[i].Cells[1].Value = dr[1];
                            }
                            else
                            {
                                dataGridView1.Rows[i].Cells[1].Value = 0;
                            }
                        }
                    }
                }
                if (tableName == "�ɳ���ÿ�վ�Ա��")
                {
                    string sql = "select (select a.�ɳ����� from �����ɳ��� a where a.�ɳ�������=t.�ɳ�������) as �ɳ�����,��Ա���� from �ɳ���ÿ�վ�Ա�� t where t.����=to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd')";
                    System.Data.DataTable dt = linkData.SelectDataBase(sql);
                    if (dt != null)
                    {
                        string[] strArr1 = null;
                        string arrstr = "";
                        for (int i = 0; i < strArr.Length; i++)
                        {
                            if (strArr[i] != "�����ɳ���")
                            {
                                arrstr += strArr[i] + ",";
                            }
                        }
                        arrstr = arrstr.Remove(arrstr.LastIndexOf(','));
                        strArr1 = arrstr.Split(',');
                        dataGridView1.Rows.Add(strArr1.Length);  //�������ε��������ɳ�����
                        for (int i = 0; i < strArr1.Length; i++)
                        {
                            dataGridView1.Rows[i].Cells[0].Value = strArr1[i];
                            DataRow[] drArr = dt.Select(dt.Columns[0].Caption + "='" + strArr1[i] + "'");
                            if (drArr.Length > 0)
                            {
                                DataRow dr = drArr[0];
                                dataGridView1.Rows[i].Cells[1].Value = dr[1];
                            }
                            else
                            {
                                dataGridView1.Rows[i].Cells[1].Value = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitialDatagridView");
            }            
        }

        //��ȡ�����ļ����������ݿ������ַ���
        private string getStrConn()
        {
            try
            {
                string exePath = System.Windows.Forms.Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                string userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                string password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");

                string connString = "user id = " + userid + ";data source = " + datasource + ";password =" + password;
                return connString;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getStrConn");
                return null;
            }
        }

        private string[] getPaichusuo(string tabName)
        {
            string a = "";
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "select �ж��� from �������ж�";
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    a += dr.GetValue(0).ToString().Trim() + ",";
                }
                Conn.Close();
                a = a.Remove(a.LastIndexOf(','));
                return a.Split(',');
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getPaichusuo");
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                return null;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string[] listZhongdui = getZhongduiList(comboBox1.Text);
                if (listZhongdui == null) return;
                strArr = listZhongdui;
                InitialDatagridView();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getPaichusuo");
            }         
        }

        private string[] getZhongduiList(string p)
        {
            string a = "";
            if (p == "�����ɳ���")
            {
                a = region2;
                return a.Split(',');
            }                
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "select �ɳ������� from �����ɳ��� where �ɳ�����='" + p + "'";
                OracleDataReader dr = cmd.ExecuteReader();
                cmd.Dispose();
                if(dr.HasRows){
                    dr.Read();
                    a = dr.GetValue(0).ToString().Trim().Substring(0,8);
                }
                else{
                    dr.Close();
                    Conn.Close();
                    return null;
                }
                dr.Close();
                cmd.CommandText = "select �ж��� from �������ж� where �жӴ��� like '"+a+"%'";
                dr = cmd.ExecuteReader();
                a = "";
                while (dr.Read())
                {
                    a += dr.GetValue(0).ToString().Trim() + ",";
                }
                dr.Close();
                Conn.Close();
                a = a.Remove(a.LastIndexOf(','));
                return a.Split(',');
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                ExToLog(ex, "getZhongduiList");
                return null;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dResult = MessageBox.Show("����ӻ���� " + dateTimePicker1.Value.ToLongDateString() + " �ľ�Ա�����Ƿ������", "ȷ��", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dResult == DialogResult.OK)
                {
                    string strField = "�ɳ�������";
                    if (tableName == "�ж�ÿ�վ�Ա��")
                    {
                        strField = "�жӴ���";
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        int maxID = getMaxID(tableName) + 1;
                        string code = getCodeFromName(dataGridView1.Rows[i].Cells[0].Value.ToString());
                        bool isExist = checkIsExist(dateTimePicker1.Value.ToShortDateString(), code);

                        string sql = "";
                        if (isExist)//����
                        {
                            sql = "update " + tableName + " set ��Ա����=" + Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value) + " where ����=to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd') and " + strField + "='" + code + "'";
                        }
                        else
                        { //���
                            sql = "insert into " + tableName + "(id,����," + strField + ",��Ա����) values(" + maxID + ",to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd')," + code + "," + Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value) + ")";
                        }
                        saveOneRow(sql);
                    }
                    MessageBox.Show("�ѱ���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "buttonOK_Click");
            }
        }

        private void saveOneRow(string sql)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                Conn.Close();
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                ExToLog(ex, "saveOneRow");
            }
        }

        private int getMaxID(string tableName)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                int code = -1;
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "select max(id) from "+tableName;
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    code =Convert.ToInt32( dr.GetValue(0));
                }
                dr.Close();
                Conn.Close();
                return code;
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                ExToLog(ex, "getMaxID");
                return -1;                
            }
        }

        private bool checkIsExist(string p, string code)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                if (tableName == "�ɳ���ÿ�վ�Ա��")
                {
                    cmd.CommandText = "select * from �ɳ���ÿ�վ�Ա�� where �ɳ�������='" + code + "' and ����=to_date('" + p + "','yyyy-mm-dd hh24:mi:ss')";
                }
                else
                {
                    cmd.CommandText = "select * from �ж�ÿ�վ�Ա�� where �жӴ���='" + code + "' and ����=to_date('" + p + "','yyyy-mm-dd hh24:mi:ss')";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    Conn.Close();
                    return true;
                }
                else
                {
                    Conn.Close();
                    return false;
                }

            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                ExToLog(ex, "checkIsExist");
                return false;
            }
        }

        private string getCodeFromName(string p)  //��ȡ�ɳ������ж�����Ӧ�Ĵ��루fisher��
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                string code = "";
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                if (tableName == "�ɳ���ÿ�վ�Ա��")
                {
                    cmd.CommandText = "select �ɳ������� from �����ɳ��� where �ɳ�����='" + p + "'";
                }
                else
                {
                    cmd.CommandText = "select �жӴ��� from �������ж� where �ж���='" + p + "'";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    code= dr.GetValue(0).ToString().Trim();
                }
                dr.Close();
                Conn.Close();
                return code;
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                ExToLog(ex, "getCodeFromName");
                return "";                
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            InitialDatagridView();
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (this.dataGridView1.CurrentCell == null || this.dataGridView1.CurrentCell.Value == null)
                {
                    return;
                }
                string value = this.dataGridView1.CurrentCell.Value.ToString().Trim();
                this.checkNumber(value);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellEndEdit");
            }
        }

        private void checkNumber(string str)//�ж�������ǲ�������
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//�ж�������ǲ�������
                {

                    MessageBox.Show("�������֣�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridView1.CurrentCell.Value = string.Empty;

                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkNumber");
            }
        }

        //�ɳ���ÿ�վ�Ա����ж�ÿ�վ�Ա������ݵ���
        private void btnDataOut_Click(object sender, EventArgs e)
        {
            string fileName = "�ж�ÿ�վ�Ա��";
            try
            {
                if (DataInOut.Rows[0]["�ɵ���"].ToString() != "1")
                {
                    MessageBox.Show("��û�е���Ȩ�ޣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (tableName == "�ɳ���ÿ�վ�Ա��")
                {
                    fileName = "�ɳ���ÿ�վ�Ա��";
                }
                ExportToExcel(dataGridView1, fileName);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "btnDataOut_Click");
            }            
        }

        // added by fisher in 09-10-13
        public void ExportToExcel(DataGridView dgv, string saveFileName)
        {
            try
            {
                bool fileSaved = false;
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.DefaultExt = "xls";
                saveDialog.Filter = "Excel�ļ� (*.xls)|*.xls";
                saveDialog.FileName = saveFileName;
                saveDialog.Title = "������...";
                saveDialog.ShowDialog();
                saveFileName = saveDialog.FileName;//����·����
                if (saveFileName.IndexOf(":") < 0) return; //������ȡ�� 

                Excel.Application xlApp = new Excel.Application();
                if (xlApp == null)
                {
                    MessageBox.Show("�޷�����Excel���󣬿������Ļ���δ��װExcel");
                    return;
                }

                Excel.Workbooks workbooks = xlApp.Workbooks;
                Excel.Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];  //ȡ��sheet1 

                //д���ֶ� 
                int t = 0;
                for (int i = 0; i < dgv.ColumnCount; i++)
                {
                    worksheet.Cells[1, i + 1] = dgv.Columns[i].HeaderText;
                    t = i + 2;
                }

                //���һ�У�ʱ��
                worksheet.Cells[1, t] = "����";

                //д����ֵ 
                for (int r = 0; r < dgv.RowCount; r++)
                {
                    for (int i = 0; i < dgv.ColumnCount; i++)
                    {
                        if (dgv.Rows[r].Cells[i].Value == null)
                        {
                            worksheet.Cells[r + 2, i + 1] = null;
                        }
                        else
                        {
                            worksheet.Cells[r + 2, i + 1] = dgv.Rows[r].Cells[i].Value.ToString();
                        }
                    }
                    worksheet.Cells[r + 2, t] = dateTimePicker1.Value.ToShortDateString();  //�������
                    System.Windows.Forms.Application.DoEvents();
                }

                worksheet.Columns.EntireColumn.AutoFit();//�п�����Ӧ

                if (saveFileName != "")
                {
                    try
                    {
                        workbook.Saved = true;
                        workbook.SaveCopyAs(saveFileName);
                        fileSaved = true;

                    }
                    catch (Exception ex)
                    {
                        fileSaved = false;
                        MessageBox.Show("�����ļ�ʱ����,�ļ����������򿪣�\n" + ex.Message);
                    }
                }

                else
                {
                    fileSaved = false;
                }
                xlApp.Quit();
                GC.Collect();//ǿ������ 
                if (fileSaved == true)
                {
                    MessageBox.Show("����������ϣ�", "tyj��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "ExportToExcel");
            }    
        }

        private void btnDataIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (DataInOut.Rows[0]["�ɵ���"].ToString() != "1")
                {
                    MessageBox.Show("��û�е���Ȩ�ޣ�","��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
                ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                ofd.FileName = "ÿ�վ�Ա��";

                this.Cursor = Cursors.WaitCursor;
                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
                {
                    XlsConn xlsConn = GetXlsConn(ofd.FileName);
                    string sheetName = GetSheetName(ofd.FileName);

                    if (sheetName == "")
                    {
                        sheetName = "Sheet1";
                    }
                    string selectSql = "select * from [" + sheetName + "$]";

                    System.Data.DataTable dt = xlsConn.DataTable(selectSql);
                    if (dt == null)
                    {
                        MessageBox.Show("excel�������ݱ�");
                        return;
                    }

                    string strField = "�ɳ�������";
                    if (tableName == "�ж�ÿ�վ�Ա��")
                    {
                        strField = "�жӴ���";
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int maxID = getMaxID(tableName) + 1;
                        string code = getCodeFromName(dt.Rows[i]["����"].ToString());
                        if (code == "")
                        {
                            MessageBox.Show("ʧ�ܣ���ȷ����Ҫ�������µı�ĸ�ʽ��ȷ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        bool isExist = checkIsExist(dt.Rows[i]["����"].ToString(), code);

                        string sql = "";
                        if (isExist)//����
                        {
                            sql = "update " + tableName + " set ��Ա����=" + Convert.ToInt32(dt.Rows[i]["����"].ToString()) + " where ���� = to_date('" + dt.Rows[i]["����"] + "','yyyy-mm-dd hh24:mi:ss') and " + strField + "='" + code + "'";
                        }
                        else
                        { //���
                            sql = "insert into " + tableName + "(id,����," + strField + ",��Ա����) values(" + maxID + ",to_date('" + dt.Rows[i]["����"] + "','yyyy-mm-dd hh24:mi:ss')," + code + "," + Convert.ToInt32(dt.Rows[i]["����"]) + ")";
                        }
                        saveOneRow(sql);
                    }
                    MessageBox.Show("���ݵ������³ɹ���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show("��ȷ����Ҫ����ı�ĸ�ʽ��ȷ��", "��ʾ!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ExToLog(ex, "btnDataIn_Click");
                this.Cursor = Cursors.Default;
                return;
            }            
        }

        private string GetSheetName(string filePath)
        {
            try
            {
                string sheetName = "";

                System.IO.FileStream tmpStream = File.OpenRead(filePath);
                byte[] fileByte = new byte[tmpStream.Length];
                tmpStream.Read(fileByte, 0, fileByte.Length);
                tmpStream.Close();

                byte[] tmpByte = new byte[]{Convert.ToByte(11),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),   
            Convert.ToByte(11),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),    
            Convert.ToByte(30),Convert.ToByte(16),Convert.ToByte(0),Convert.ToByte(0)};

                int index = GetSheetIndex(fileByte, tmpByte);
                if (index > -1)
                {
                    //index+=32+12;   
                    index += 16 + 12;
                    System.Collections.ArrayList sheetNameList = new System.Collections.ArrayList();

                    for (int i = index; i < fileByte.Length - 1; i++)
                    {
                        byte temp = fileByte[i];
                        if (temp != Convert.ToByte(0))
                            sheetNameList.Add(temp);
                        else
                            break;
                    }
                    byte[] sheetNameByte = new byte[sheetNameList.Count];
                    for (int i = 0; i < sheetNameList.Count; i++)
                        sheetNameByte[i] = Convert.ToByte(sheetNameList[i]);

                    sheetName = System.Text.Encoding.Default.GetString(sheetNameByte);
                }
                return sheetName;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetSheetName");
                return null;
            }   
        }

        private XlsConn GetXlsConn(string xlsPath)
        {
            try
            {
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + xlsPath + ";" + "Extended Properties='Excel 8.0;HDR=YES;IMEX=1';";
                return new XlsConn(connStr);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetXlsConn");
                return null;
            } 
        }

        private int GetSheetIndex(byte[] FindTarget, byte[] FindItem)
        {
            try
            {
                int index = -1;

                int FindItemLength = FindItem.Length;
                if (FindItemLength < 1) return -1;
                int FindTargetLength = FindTarget.Length;
                if ((FindTargetLength - 1) < FindItemLength) return -1;

                for (int i = FindTargetLength - FindItemLength - 1; i > -1; i--)
                {
                    System.Collections.ArrayList tmpList = new System.Collections.ArrayList();
                    int find = 0;
                    for (int j = 0; j < FindItemLength; j++)
                    {
                        if (FindTarget[i + j] == FindItem[j]) find += 1;
                    }
                    if (find == FindItemLength)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetSheetIndex");
                return 0;
            }           
        }

        /// <summary>
        /// �쳣��־
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFuns">������</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-frmPoliceCount-" + sFuns);
        }
    }
}