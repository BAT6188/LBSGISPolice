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
        /// 读取配置文件，设置数据库连接字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <returns>连接字符串</returns>
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");

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
        private string[] StrCon;              // 连接参数
        public string strConn;                // 连接字符串
        public MapInfo.Windows.Controls.MapControl mapControl = null; // 地图控件
        private DataRow row = null;           // 数据行
        public string getFromNamePath;        // 配置文件GetFromNameConfig.ini的地址
        private string LayerName;             // 图层名
        public FrmInfo()
        {
            InitializeComponent();
            strConn = getStrConn();
        }

        private string[] lsPopu = null, dqPopu = null, wzPopu = null, yzPopu = null;     // 存放历史居住人数、当前居住人数,暂住有效期内人数,已办暂住人数的身份证号码

        /// <summary>
        /// 初始化窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dRow">数据行</param>
        /// <param name="pt">显示位置</param>
        /// <param name="LayName">图层名</param>
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
                    if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码"
                                                            && col.Caption != "所属中队代码"
                                                            && col.Caption != "所属警务室代码"
                                                            && col.Caption != "抽取ID"
                                                            && col.Caption != "抽取更新时间"
                                                            && col.Caption != "最后更新人"
                                                            && col.Caption != "屋主身份证号码")
                    {
                        if (col.Caption == "涉案人员" || col.Caption == "相关案件"  
                                                      || (col.Caption == "房屋编号" && col.Table.Columns[0].Caption != "房屋编号") 
                                                      || col.Caption == "当前居住人数" 
                                                      || col.Caption == "历史居住人数" 
                                                      || col.Caption == "暂住证有效期内人数" 
                                                      || col.Caption == "未办暂住证人数")
                            this.dataGridView1.Rows.Add(col.Caption + ":", "");
                        else
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
                this.Visible = true;

                #region 添加链接，相互关联3
                DataGridViewLinkCell dgvlc = null, dgvlc3 = null, dgvlc1 = null, dgvlc2 = null, dgvlc4 = null;
                string strPopu = "";   // 用于统计的sql;
                string houseNo = dRow.Table.Rows[0][0].ToString();   // 获得房屋编号

                for (int i = 0; i < dRow.Table.Columns.Count; i++)
                {
                    if (dRow.Table.Columns[i].Caption == "暂住证有效期内人数")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and 人口性质='暂住人口' and 暂住证号 is not null and 暂住证有效期限>sysdate";
                        wzPopu = getPopuCount(strPopu, wzPopu);

                        //wzPopu = new string[] { };
                        //wzPopu = dRow.Table.Rows[0][i].ToString().Split(',');

                        // 加上链接，可查看暂住有效期内人详细信息
                        dgvlc = new DataGridViewLinkCell();
                        if (wzPopu[0] == "" || wzPopu[0] == "0")
                            dgvlc.Value = "0";
                        else
                            dgvlc.Value = wzPopu.Length.ToString();
                        dgvlc.ToolTipText = "查看暂住证有效期内人详细信息";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "未办暂住证人数")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and 人口性质='暂住人口' and ((暂住证号 is null) or (暂住证号 is not null and 暂住证有效期限<sysdate))";
                        yzPopu = getPopuCount(strPopu, yzPopu);

                        //yzPopu = new string[] { };
                        //yzPopu = dRow.Table.Rows[0][i].ToString().Split(',');

                        // 加上链接，可查看未办暂住证人详细信息
                        dgvlc3 = new DataGridViewLinkCell();
                        if (yzPopu[0] == "" || yzPopu[0] == "0")
                            dgvlc3.Value = "0";
                        else
                            dgvlc3.Value = yzPopu.Length.ToString();
                        dgvlc3.ToolTipText = "查看未办暂住证人详细信息";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc3;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "历史居住人数")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        lsPopu = getPopuCount(strPopu, lsPopu);

                        //lsPopu = new string[] { };
                        //lsPopu = dRow.Table.Rows[0][i].ToString().Split(',');
                        // 加上链接，可查看历史居住人详细信息
                        dgvlc1 = new DataGridViewLinkCell();
                        if (lsPopu[0] == "" || lsPopu[0] == "0")
                            dgvlc1.Value = "0";
                        else
                            dgvlc1.Value = lsPopu.Length.ToString();
                        dgvlc1.ToolTipText = "查看历史居住人详细信息";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc1;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "当前居住人数")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        dqPopu = getPopuCount(strPopu, dqPopu);

                        //dqPopu = new string[] { };
                        //dqPopu = dRow.Table.Rows[0][i].ToString().Split(',');

                        // 加上链接，可查看当前居住人详细信息
                        dgvlc2 = new DataGridViewLinkCell();
                        if (dqPopu[0] == "" || dqPopu[0] == "0")
                            dgvlc2.Value = "0";
                        else
                            dgvlc2.Value = dqPopu.Length.ToString();
                        dgvlc2.ToolTipText = "查看当前居住人详细信息";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc2;
                        continue;
                    }
                    if (dRow.Table.Columns[i].Caption == "屋主姓名")
                    {
                        dgvlc4 = new DataGridViewLinkCell();
                        dgvlc4.Value = row["屋主姓名"].ToString();
                        dgvlc4.ToolTipText = "查看屋主详细信息";
                        //this.dataGridView1.Rows[i].Cells[1] = dgvlc4;
                    }
                }
                for (int j = 0; j < this.dataGridView1.Rows.Count; j++)
                {
                    switch (this.dataGridView1.Rows[j].Cells[0].Value.ToString())
                    {
                        case "暂住证有效期内人数:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc;
                            break;
                        case "未办暂住证人数:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc3;
                            break;
                        case "历史居住人数:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc1;
                            break;
                        case "当前居住人数:":
                            this.dataGridView1.Rows[j].Cells[1] = dgvlc2;
                            break;
                        case "屋主姓名:":
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
        /// 根据sql查询人数并逐个保存到数组中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="poStr">sql语句</param>
        /// <param name="conPo">用于存放人数的数组</param>
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
        /// 用来转换房屋编号代码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="str">要转换的编号</param>
        /// <returns>转换后字符串</returns>
        private string ConversionStr(string str)
        {
            try
            {
                string converStr = "";

                switch (str.Substring(0, 1))
                {
                    case "a":
                        converStr = "户号";
                        break;
                    case "b":
                        converStr = "出租屋编号";
                        break;
                    case "c":
                        converStr = "住址门牌代码";
                        break;
                    case "d":
                        converStr = "企业编号";
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
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
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
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.frmInfo.setSize");
            }
        }

        /// <summary>
        /// 设置窗体位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="iWidth">窗体宽度</param>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
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
        /// 单击列表事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (strConn == "")
            {
                MessageBox.Show("读取配置文件时发生错误,请修改配置文件后重试!");
                return;
            }

            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter oracleDat = null;
            DataSet objset = new DataSet();

            if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "暂住证有效期内人数:")
            {
                if (wzPopu[0] == "" || wzPopu[0] == "0")
                    return;
                string wzPopus = arrayConStr(wzPopu);

                try
                {
                    string strSQL = "select 身份证号码,姓名 from 人口系统 where 身份证号码 in ('" + wzPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL,myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("居住人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (wzPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "人口系统");
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
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "未办暂住证人数:")
            {
                if (yzPopu[0] == "" || yzPopu[0] == "0")
                    return;
                string yzPopus = arrayConStr(yzPopu);

                try
                {
                    string strSQL = "select 身份证号码,姓名 from 人口系统 where 身份证号码 in ('" + yzPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("居住人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (yzPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "人口系统");
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
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "历史居住人数:")
            {
                if (lsPopu[0] == "" || lsPopu[0] == "0")
                    return;
                string lsPopus = arrayConStr(lsPopu); 
               
                try
                {
                    string strSQL = "select 身份证号码,姓名 from 人口系统 where 身份证号码 in ('" + lsPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("居住人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (lsPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "人口系统");
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
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "当前居住人数:")
            {
                if (dqPopu[0] == "" || dqPopu[0] == "0")
                    return;
                string dqPopus = arrayConStr(dqPopu); 
                
                try
                {
                    string strSQL = "select 身份证号码,姓名 from 人口系统 where 身份证号码 in ('" + dqPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("居住人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (dqPopu.Length > 1)
                        disPlayNumberInfo(objset.Tables[0], "人口系统");
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
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "屋主姓名:")
            {
                poupDisplayInfo(row["屋主身份证号码"].ToString());
            }
        }

        /// <summary>
        /// 字符串数组转换为可供SQL使用的字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="constrArray">字符串数组</param>
        /// <returns>可供SQL使用的字符串</returns>
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
        /// 当为一个记录显示时用此方法
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="popuNo">人口编号</param>
        private void poupDisplayInfo(string popuNo)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter oracleDat = null;
            DataSet objset = new DataSet();
            try
            {
                myConnection.Open();
                CLC.ForSDGA.GetFromTable.GetFromName("人口系统", getFromNamePath);
                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from 人口系统 t where 身份证号码='" + popuNo + "'";
                oracleDat = new OracleDataAdapter(strSQL, myConnection);
                oracleDat.Fill(objset, "table");

                // 判断是否有记录
                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("身份证号码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }

                disPlayInfo(objset.Tables[0], pt, "人口系统");
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
        /// 显示所有编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="tabName">表名</param>
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
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">位置</param>
        /// <param name="tableName">表名</param>
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
        /// 获取查询结果表显示所有编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>结果集</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }
    }
}

