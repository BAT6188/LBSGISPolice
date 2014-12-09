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
        private System.Data.DataTable DataInOut = null;  // 导入导出的权限 lili 2010-9-26
        string region1, region2;  // 存取所属派出所，中队权限

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
                ExToLog(ex, "构造函数");
            }
        }

        private void InitialCombo()
        {
            try
            {
                if (tableName == "派出所每日警员表")
                {
                    comboBox1.Enabled = false;
                    //如果是某个街镇用户,直接赋予辖区初始值
                    if (region1 != "" && region1 != "顺德区")   //edit by fisher in 09-11-30
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
                else  //中队每日警员表
                {
                    for (int i = 0; i < strArr.Length; i++)
                    {
                        comboBox1.Items.Add(strArr[i]);
                    }
                    ////如果是某个街镇用户,直接赋予辖区初始值
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
                if (tableName == "中队每日警员表")
                {
                    string sql = "select (select a.中队名 from 基层民警中队 a where a.中队代码=t.中队代码) as 中队名,警员数量 from 中队每日警员表 t where t.日期=to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd')";
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
                if (tableName == "派出所每日警员表")
                {
                    string sql = "select (select a.派出所名 from 基层派出所 a where a.派出所代码=t.派出所代码) as 派出所名,警员数量 from 派出所每日警员表 t where t.日期=to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd')";
                    System.Data.DataTable dt = linkData.SelectDataBase(sql);
                    if (dt != null)
                    {
                        string[] strArr1 = null;
                        string arrstr = "";
                        for (int i = 0; i < strArr.Length; i++)
                        {
                            if (strArr[i] != "其他派出所")
                            {
                                arrstr += strArr[i] + ",";
                            }
                        }
                        arrstr = arrstr.Remove(arrstr.LastIndexOf(','));
                        strArr1 = arrstr.Split(',');
                        dataGridView1.Rows.Add(strArr1.Length);  //这里屏蔽掉“其他派出所”
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

        //读取配置文件，设置数据库连接字符串
        private string getStrConn()
        {
            try
            {
                string exePath = System.Windows.Forms.Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");

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
                cmd.CommandText = "select 中队名 from 基层民警中队";
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
            if (p == "其他派出所")
            {
                a = region2;
                return a.Split(',');
            }                
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "select 派出所代码 from 基层派出所 where 派出所名='" + p + "'";
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
                cmd.CommandText = "select 中队名 from 基层民警中队 where 中队代码 like '"+a+"%'";
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
                DialogResult dResult = MessageBox.Show("将添加或更新 " + dateTimePicker1.Value.ToLongDateString() + " 的警员数，是否继续？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dResult == DialogResult.OK)
                {
                    string strField = "派出所代码";
                    if (tableName == "中队每日警员表")
                    {
                        strField = "中队代码";
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        int maxID = getMaxID(tableName) + 1;
                        string code = getCodeFromName(dataGridView1.Rows[i].Cells[0].Value.ToString());
                        bool isExist = checkIsExist(dateTimePicker1.Value.ToShortDateString(), code);

                        string sql = "";
                        if (isExist)//更新
                        {
                            sql = "update " + tableName + " set 警员数量=" + Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value) + " where 日期=to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd') and " + strField + "='" + code + "'";
                        }
                        else
                        { //添加
                            sql = "insert into " + tableName + "(id,日期," + strField + ",警员数量) values(" + maxID + ",to_date('" + dateTimePicker1.Value.ToShortDateString() + "','yyyy-mm-dd')," + code + "," + Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value) + ")";
                        }
                        saveOneRow(sql);
                    }
                    MessageBox.Show("已保存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                if (tableName == "派出所每日警员表")
                {
                    cmd.CommandText = "select * from 派出所每日警员表 where 派出所代码='" + code + "' and 日期=to_date('" + p + "','yyyy-mm-dd hh24:mi:ss')";
                }
                else
                {
                    cmd.CommandText = "select * from 中队每日警员表 where 中队代码='" + code + "' and 日期=to_date('" + p + "','yyyy-mm-dd hh24:mi:ss')";
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

        private string getCodeFromName(string p)  //获取派出所或中队所对应的代码（fisher）
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                string code = "";
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                if (tableName == "派出所每日警员表")
                {
                    cmd.CommandText = "select 派出所代码 from 基层派出所 where 派出所名='" + p + "'";
                }
                else
                {
                    cmd.CommandText = "select 中队代码 from 基层民警中队 where 中队名='" + p + "'";
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

        private void checkNumber(string str)//判断输入的是不是数字
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//判断输入的是不是数字
                {

                    MessageBox.Show("输入数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridView1.CurrentCell.Value = string.Empty;

                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkNumber");
            }
        }

        //派出所每日警员表和中队每日警员表的数据导出
        private void btnDataOut_Click(object sender, EventArgs e)
        {
            string fileName = "中队每日警员表";
            try
            {
                if (DataInOut.Rows[0]["可导出"].ToString() != "1")
                {
                    MessageBox.Show("您没有导出权限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (tableName == "派出所每日警员表")
                {
                    fileName = "派出所每日警员表";
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
                saveDialog.Filter = "Excel文件 (*.xls)|*.xls";
                saveDialog.FileName = saveFileName;
                saveDialog.Title = "导出到...";
                saveDialog.ShowDialog();
                saveFileName = saveDialog.FileName;//完整路径名
                if (saveFileName.IndexOf(":") < 0) return; //被点了取消 

                Excel.Application xlApp = new Excel.Application();
                if (xlApp == null)
                {
                    MessageBox.Show("无法创建Excel对象，可能您的机子未安装Excel");
                    return;
                }

                Excel.Workbooks workbooks = xlApp.Workbooks;
                Excel.Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];  //取得sheet1 

                //写入字段 
                int t = 0;
                for (int i = 0; i < dgv.ColumnCount; i++)
                {
                    worksheet.Cells[1, i + 1] = dgv.Columns[i].HeaderText;
                    t = i + 2;
                }

                //添加一列，时间
                worksheet.Cells[1, t] = "日期";

                //写入数值 
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
                    worksheet.Cells[r + 2, t] = dateTimePicker1.Value.ToShortDateString();  //添加日期
                    System.Windows.Forms.Application.DoEvents();
                }

                worksheet.Columns.EntireColumn.AutoFit();//列宽自适应

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
                        MessageBox.Show("导出文件时出错,文件可能正被打开！\n" + ex.Message);
                    }
                }

                else
                {
                    fileSaved = false;
                }
                xlApp.Quit();
                GC.Collect();//强行销毁 
                if (fileSaved == true)
                {
                    MessageBox.Show("导出数据完毕！", "tyj提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                if (DataInOut.Rows[0]["可导入"].ToString() != "1")
                {
                    MessageBox.Show("您没有导入权限！","提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "请选择将导入的EXCEL文件路径";
                ofd.Filter = "Excel文档(*.xls)|*.xls";
                ofd.FileName = "每日警员表";

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
                        MessageBox.Show("excel中无数据表");
                        return;
                    }

                    string strField = "派出所代码";
                    if (tableName == "中队每日警员表")
                    {
                        strField = "中队代码";
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int maxID = getMaxID(tableName) + 1;
                        string code = getCodeFromName(dt.Rows[i]["名称"].ToString());
                        if (code == "")
                        {
                            MessageBox.Show("失败！请确保您要导入或更新的表的格式正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        bool isExist = checkIsExist(dt.Rows[i]["日期"].ToString(), code);

                        string sql = "";
                        if (isExist)//更新
                        {
                            sql = "update " + tableName + " set 警员数量=" + Convert.ToInt32(dt.Rows[i]["数量"].ToString()) + " where 日期 = to_date('" + dt.Rows[i]["日期"] + "','yyyy-mm-dd hh24:mi:ss') and " + strField + "='" + code + "'";
                        }
                        else
                        { //添加
                            sql = "insert into " + tableName + "(id,日期," + strField + ",警员数量) values(" + maxID + ",to_date('" + dt.Rows[i]["日期"] + "','yyyy-mm-dd hh24:mi:ss')," + code + "," + Convert.ToInt32(dt.Rows[i]["数量"]) + ")";
                        }
                        saveOneRow(sql);
                    }
                    MessageBox.Show("数据导入或更新成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show("请确保需要导入的表的格式正确！", "提示!", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        /// 异常日志
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-frmPoliceCount-" + sFuns);
        }
    }
}