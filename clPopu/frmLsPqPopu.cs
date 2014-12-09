using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Data.OracleClient;
using System.Windows.Forms;

using MapInfo.Styles;
using MapInfo.Mapping;
using MapInfo.Engine;
using MapInfo.Geometry;
using MapInfo.Data;
using System.IO;

namespace clPopu
{
    public partial class frmLsPqPopu : Form
    {
        public MapInfo.Windows.Controls.MapControl mapControl = null;
        private Feature flashFt = null;
        public string strConn;
        public string getFromNamePath;
        public string LayerName;
        public string[] StrCon;

        public frmLsPqPopu()
        {
            InitializeComponent();
            getStrConn();
        }

        //读取配置文件，设置数据库连接字符串
        private void getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");

                StrCon = new string[] { datasource, userid, password };
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getStrConn");
            }
        }

        private DataRow row = null;
        private string[] dqPopu = null, lsPopu = null, wzPopu = null, yzPopu = null;  // 数组存储查询结果，方便后面查询
        public void setInfo(DataRow dRow, System.Drawing.Point pt)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                row = dRow;
                string houseNo = "";   // 获得房屋编号
                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码" && col.Caption != "所属中队代码" && col.Caption != "所属警务室代码" && col.Caption != "抽取ID" && col.Caption != "抽取更新时间" && col.Caption != "最后更新人" && col.Caption != "配偶公民身份号码" && col.Caption != "屋主身份证号码" && col.Caption != "住址门牌")
                    {
                        if (col.Caption == "涉案人员" || col.Caption == "相关案件" || (col.Caption == "房屋编号" && col.Table.Columns[0].Caption != "房屋编号") || col.Caption == "当前居住人数" || col.Caption == "历史居住人数" || col.Caption == "暂住证有效期内人数" || col.Caption == "未办暂住证人数")
                            this.dataGridView1.Rows.Add(col.Caption + ":", "");
                        else
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                    if (col.Table.Columns[0].Caption == "房屋编号")
                        houseNo = dRow.Table.Rows[0]["房屋编号"].ToString();
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
                this.Visible = true;

                string strPopu = "";   // 用于统计的sql;
                #region 添加链接 相互关联
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "涉案人员:" || this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "相关案件:" || this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "房屋编号:")
                    {
                        k = i;
                        continue;
                    }
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "当前居住人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        dqPopu = getPopuCount(strPopu, dqPopu);

                        DataGridViewLinkCell Datagvlc = new DataGridViewLinkCell();
                        if (dqPopu[0] == "" || dqPopu[0] == "0")
                            Datagvlc.Value = "0";
                        else
                            Datagvlc.Value = dqPopu.Length.ToString();
                        Datagvlc.ToolTipText = "查看当前居住人详细信息";
                        dataGridView1.Rows[i].Cells[1] = Datagvlc;
                        continue;
                    }
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "历史居住人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        lsPopu = getPopuCount(strPopu, lsPopu);

                        DataGridViewLinkCell Datagvlc1 = new DataGridViewLinkCell();
                        if (lsPopu[0] == "" || lsPopu[0] == "0")
                            Datagvlc1.Value = "0";
                        else
                            Datagvlc1.Value = lsPopu.Length.ToString();
                        Datagvlc1.ToolTipText = "查看历史居住人详细信息";
                        dataGridView1.Rows[i].Cells[1] = Datagvlc1;
                        continue;
                    }
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "暂住证有效期内人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and 人口性质='暂住人口' and 暂住证号 is not null and 暂住证有效期限>sysdate";
                        yzPopu = getPopuCount(strPopu, yzPopu);

                        DataGridViewLinkCell Datagvlc2 = new DataGridViewLinkCell();
                        if (yzPopu[0] == "" || yzPopu[0] == "0")
                            Datagvlc2.Value = "0";
                        else
                            Datagvlc2.Value = yzPopu.Length.ToString();
                        Datagvlc2.ToolTipText = "查看暂住证有效期内人详细信息";
                        dataGridView1.Rows[i].Cells[1] = Datagvlc2;
                        continue;
                    }
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "未办暂住证人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and 人口性质='暂住人口' and ((暂住证号 is null) or (暂住证号 is not null and 暂住证有效期限<sysdate))";
                        wzPopu = getPopuCount(strPopu, wzPopu);

                        DataGridViewLinkCell Datagvlc3 = new DataGridViewLinkCell();
                        if (wzPopu[0] == "" || wzPopu[0] == "0")
                            Datagvlc3.Value = "0";
                        else
                            Datagvlc3.Value = wzPopu.Length.ToString();
                        Datagvlc3.ToolTipText = "查看未办暂住证人详细信息";
                        dataGridView1.Rows[i].Cells[1] = Datagvlc3;
                        continue;
                    }
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "配偶姓名:")
                    {
                        DataGridViewLinkCell Datagvlc4 = new DataGridViewLinkCell();
                        Datagvlc4.Value = row["配偶姓名"].ToString();
                        Datagvlc4.ToolTipText = "查看配偶详细信息";
                        dataGridView1.Rows[i].Cells[1] = Datagvlc4;
                        continue;
                    }
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "屋主姓名:")
                    {
                        DataGridViewLinkCell Datagvlc5 = new DataGridViewLinkCell();
                        Datagvlc5.Value = row["屋主姓名"].ToString();
                        Datagvlc5.ToolTipText = "查看屋主详细信息";
                        dataGridView1.Rows[i].Cells[1] = Datagvlc5;
                        continue;
                    }
                }
                DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                if (this.dataGridView1.Rows[k].Cells[0].Value.ToString() == "涉案人员:")
                {
                    dgvlc.Value = "查看相关涉案人员信息";
                    dgvlc.ToolTipText = "查看相关涉案人员信息";
                    this.dataGridView1.Rows[k].Cells[1] = dgvlc;
                }
                if (this.dataGridView1.Rows[k].Cells[0].Value.ToString() == "相关案件:")
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "房屋编号:")
                        {
                            DataGridViewLinkCell dgvlc1 = new DataGridViewLinkCell();
                            dgvlc1.Value = "查看房屋详细信息";
                            dgvlc1.ToolTipText = "查看房屋详细信息";
                            this.dataGridView1.Rows[i].Cells[1] = dgvlc1;
                            break;
                        }
                        DataGridViewLinkCell dgvlc2 = new DataGridViewLinkCell();
                        dgvlc2.Value = dRow[dRow.Table.Columns[0]];
                        dgvlc2.ToolTipText = "查看照片";
                        this.dataGridView1.Rows[0].Cells[1] = dgvlc2;
                    }
                    dgvlc.Value = "查看案件详细信息";
                    dgvlc.ToolTipText = "查看案件详细信息";
                    this.dataGridView1.Rows[k].Cells[1] = dgvlc;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }
        }


        /// <summary>
        /// 根据sql查询编号并逐个保存到数组中   add lili 2010-12-23
        /// </summary>
        /// <param name="poStr">sql语句</param>
        /// <param name="conPo">用于存放编号的数组</param>
        /// <returns></returns>
        private string[] getPopuCount(string poStr, string[] conPo)
        {
            try
            {
                DataTable poDat = GetTable(poStr);

                if (poDat.Rows.Count > 0)
                    conPo = new string[poDat.Rows.Count];
                else   // 如果没有查询到编号则给数组一个“0”值，此做法为防止数组无值引发异常
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
                ExToLog(ex, "getPopuCount");
                return conPo;
            }
        }

        /// <summary>
        /// 用来转换房屋编号代码  add lili 2010-12-23
        /// </summary>
        /// <param name="str">要转换的编号</param>
        /// <returns>转换后字符串</returns>
        private string ConversionStr(string str)
        {
            try
            {
                string converStr = "";

                switch (str.Substring(0, 1))    // 获取编号的第一个英文字母从而判断是那种类型
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
                ExToLog(ex, "ConversionStr");
                return "";
            }
        }

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
                cMessageHeight += 60;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

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
                ExToLog(ex, "setLocation");
            }
        }

        // 单击单元格时触发
        string tabName = "",tableName="";
        string[] strAnjan;
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            OracleConnection myConnection = new OracleConnection(strConn);
            OracleCommand oracleCmd = null;
            OracleDataReader dataReader = null;
            OracleDataAdapter objoda = null;
            DataSet objset = new DataSet();

            if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "姓名:")
            {
                int k = 0, r = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "身份证号码:")
                    {
                        k = i;
                    }
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "人口性质:")
                    {
                        r = i;
                    }
                }
                System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=czrk_cx;Password=czrk_cx;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.47.227.15)(PORT=1521)))(CONNECT_DATA=(SID=ora81)))");
                try
                {
                    FrmImage fimage = new FrmImage();
                    oraconn.Open();
                    string sqlstr = "";
                    if (dataGridView1.Rows[r].Cells[1].Value.ToString() == "常住人口")
                        sqlstr = "select TX from czrk_cx.v_gis_czrk_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";
                    else
                        sqlstr = "select TX from czrk_cx.v_gis_ldry_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";

                    oracleCmd = new System.Data.OracleClient.OracleCommand(sqlstr, oraconn);
                    dataReader = oracleCmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        byte[] bytes = new byte[(dataReader.GetBytes(0, 0, null, 0, int.MaxValue))];
                        if (dataReader.IsDBNull(0))
                        {
                            System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, bytes.Length);
                            dataReader.Close();

                            Stream fs = new MemoryStream(bytes);
                            //创建一个bitmap类型的bmp变量来读取文件。
                            Bitmap bmp = new Bitmap(Image.FromStream(fs));
                            bmp.Save(Application.StartupPath + "\\lspqpopu.jpg");
                            fimage.pictureBox1.ImageLocation = Application.StartupPath + "\\lspqpopu.jpg";
                            bmp.Dispose();//释放bmp文件资源
                            fimage.pictureBox1.Invalidate();
                            fs.Close();
                            fimage.TopMost = true;
                            fimage.Visible = false;
                            fimage.ShowDialog();
                            fimage.Dispose();
                            File.Delete(Application.StartupPath + "\\lspqpopu.jpg");
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    oracleCmd.Dispose();
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "dataGridView1_CellContentClick.01");
                }
                finally
                {
                    if (oraconn.State == ConnectionState.Open)
                        oraconn.Close();
                }
            }

            tabName = ""; strAnjan = new string[] { };

            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "涉案人员:")
                tabName = "涉案人员:";
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "相关案件:")
                tabName = "相关案件:";
            if (row.Table.Columns[0].Caption == "姓名" && dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "房屋编号:")
                tabName = "房屋编号:";
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "当前居住人数:")
                tabName = "当前居住人数:";
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "历史居住人数:")
                tabName = "历史居住人数:";
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "暂住证有效期内人数:")
                tabName = "暂住证有效期内人数:";
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "未办暂住证人数:")
                tabName = "未办暂住证人数:";
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "配偶姓名:")
                tabName = "配偶公民身份号码:";
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "屋主姓名:")
                tabName = "屋主身份证号码:";

            string strSQL = "", strMessage = "";
            try
            {
                myConnection.Open();
                string colName = tabName.Substring(0, tabName.Length - 1);
                switch (colName)
                {
                    case "相关案件":
                        string AnName = row[colName].ToString();
                        strAnjan = AnName.Split(',');
                        if (AnName != "")
                        {
                            AnName = AnName.Replace(",", "','");
                            strSQL = "select 案件编号,案件名称 from 案件信息 t where 案件编号 in ('" + AnName + "')";
                        }
                        tableName = "案件信息";
                        strMessage = "无涉案记录！";
                        break;
                    case "房屋编号":
                        string houseNo = row[colName].ToString();
                        strAnjan = houseNo.Split(',');
                        if (houseNo != "")
                        {
                            houseNo = houseNo.Replace(",", "','");
                            strSQL = "select 房屋编号,屋主姓名 from 出租屋房屋系统 t where 房屋编号 in ('" + houseNo + "')";
                        }
                        tableName = "出租屋房屋系统";
                        strMessage = "房屋未存档！";
                        break;
                    case "涉案人员":
                        string anRen = row[colName].ToString();
                        strAnjan = anRen.Split(',');
                        if (anRen != "")
                        {
                            anRen = anRen.Replace(",", "','");
                            strSQL = "select 身份证号码,姓名 from 人口系统 t where 身份证号码 in ('" + anRen + "')";
                        }
                        tableName = "人口系统";
                        string renName = colName.Substring(0, colName.Length - 1);
                        strMessage = renName + "未存档！";
                        break;
                    case "当前居住人数":
                    case "暂住证有效期内人数":
                    case "历史居住人数":
                    case "未办暂住证人数":
                        string cardId = arrayConStr(colName);
                        strAnjan = cardId.Split(',');
                        if (cardId != "")
                        {
                            cardId = cardId.Replace(",", "','");
                            strSQL = "select 身份证号码,姓名 from 人口系统 t where 身份证号码 in ('" + cardId + "')";
                        }
                        tableName = "人口系统";
                        string meName = colName.Substring(0, colName.Length - 1);
                        strMessage = meName + "未存档！";
                        break;
                    case "配偶公民身份号码":
                    case "屋主身份证号码":
                        tableName = "人口系统";
                        string sfzID = row[colName].ToString();
                        onlyFirst(sfzID);
                        break;
                }
                if (strSQL != "")
                {
                    objset = new DataSet();
                    objoda = new OracleDataAdapter(strSQL, myConnection);
                    objoda.Fill(objset, "table");
                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show(strMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }
                else
                {
                    if (tabName != "" && tabName != "配偶公民身份号码:" && tabName != "屋主身份证号码:")
                        MessageBox.Show(strMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                if (strAnjan.Length > 1)
                    disPlayNumberInfo(objset.Tables[0], tableName);
            }
            catch (Exception ex)
            {
               ExToLog(ex, "dataGridView1_CellContentClick.02");
            }
            finally
            {
                if (myConnection.State == ConnectionState.Open)
                    myConnection.Close();
                try
                {
                    // 如果只有一条记录则直接显示信息
                    if (strAnjan != null && strAnjan.Length == 1 && strAnjan[0] != "")
                        onlyFirst(strAnjan[0].ToString());
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "dataGridView1_CellContentClick.03");
                }
            }

        }

        /// <summary>
        /// 根据列名找到字符串数组转换为可供SQL使用的字符串
        /// </summary>
        /// <param name="constrArray"></param>
        /// <returns></returns>
        private string arrayConStr(string colName)
        {
            try
            {
                string[] constrArray = null;
                switch (colName)
                {
                    case "当前居住人数":
                        constrArray = dqPopu;
                        break;
                    case "暂住证有效期内人数":
                        constrArray = yzPopu;
                        break;
                    case "历史居住人数":
                        constrArray = lsPopu;
                        break;
                    case "未办暂住证人数":
                        constrArray = wzPopu;
                        break;
                    default:
                        break;
                }

                string arrayStr = "";

                if (constrArray.Length > 0)     // 将数组中的编号转换成用逗号分隔的字符串
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

                return arrayStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "arrayConStr");
                return "";
            }
        }

        frmObjNumber frmNumber = new frmObjNumber();
        private void disPlayNumberInfo(DataTable dt, string tabName)
        {
            try
            {
                if (this.frmNumber.Visible == false)
                {
                    this.frmNumber = new frmObjNumber();
                    frmNumber.Show();
                    frmNumber.Visible = true;
                }
                frmNumber.mapControl = mapControl;
                frmNumber.LayerName = LayerName;
                frmNumber.getFromNamePath = getFromNamePath;
                frmNumber.strConn = strConn;
                frmNumber.setfrmInfo(dt,tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayNumberInfo");
            }
        }

        // 只有一条信息时调用该函数
        private void onlyFirst(string number)
        {
            OracleConnection myConnection = new OracleConnection(strConn);
            OracleDataAdapter objoda = null;
            DataSet objset = new DataSet();
            try
            {
                myConnection.Open();
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + number + "'";
                objoda = new OracleDataAdapter(strSQL, myConnection);
                objoda.Fill(objset, "table");

                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("未查询到此记录，请管理确认数据库中编号是否正确！", "系统提示", MessageBoxButtons.OK);
                    return;
                }

                DataRow row = objset.Tables[0].Rows[0];

                System.Drawing.Point pt = new System.Drawing.Point(); 
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }
                disPlayInfo(objset.Tables[0], pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "onlyFirst");
            }
            finally
            {
                objoda.Dispose();
                myConnection.Close();
            }
        }

        frmObjInfo frmhouse = new frmObjInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (this.frmhouse.Visible == false)
                {
                    this.frmhouse = new frmObjInfo();
                    frmhouse.SetDesktopLocation(-30, -30);
                    frmhouse.Show();
                    frmhouse.Visible = false;
                }
                frmhouse.mapControl = mapControl;
                frmhouse.LayerName = LayerName;
                frmhouse.getFromNamePath = getFromNamePath;
                frmhouse.strConn = strConn;
                frmhouse.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        // 对象定位按钮
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (flashFt != null)
                {
                    try
                    {
                        flashFt.Style = defaultStyle;
                        flashFt.Update();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                string[] winStr = new string[] { row["X"].ToString(), row["Y"].ToString(), CLC.ForSDGA.GetFromTable.TableName, row[CLC.ForSDGA.GetFromTable.ObjID].ToString(), row[CLC.ForSDGA.GetFromTable.ObjName].ToString() };
                DinPoint(winStr);
                k = 0;
                this.timer1.Start();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }
        }

        // 定位指的点
        private Style defaultStyle = null;
        private void DinPoint(string[] winStr)
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl.Map.Layers[LayerName];
                Table tableTem = ftla.Table;

                double dx = 0, dy = 0;

                try
                {
                    dx = Convert.ToDouble(winStr[0]);
                    dy = Convert.ToDouble(winStr[1]);
                }
                catch
                {
                    MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                if (dx > 0 && dy > 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);
                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 9));

                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                    cs = new CompositeStyle(pStyle);
                    Feature pFeat = new Feature(tableTem.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["表_ID"] = winStr[3];
                    pFeat["表名"] = winStr[2];
                    pFeat["名称"] = winStr[4];
                    tableTem.InsertFeature(pFeat);
                    flashFt = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + winStr[3] + "' and 表名='" + winStr[2] + "'"));
                    defaultStyle = flashFt.Style;
                    if (flashFt != null)
                    {
                        mapControl.Map.SetView(flashFt);
                    }
                    else
                    {
                        MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "DinPoint");
                Console.WriteLine(ex.Message);
            }
        }

        // 图片闪烁
        int k = 0;
        Color col = Color.Red;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (col == Color.Red)
                {
                    col = Color.Blue;
                }
                else
                {
                    col = Color.Red;
                }
                BasePointStyle pStyle = new SimpleVectorPointStyle(35, col, 26);
                try
                {
                    flashFt.Style = pStyle;
                    flashFt.Update();
                }
                catch { }
                k++;
                if (k == 10)
                {
                    timer1.Stop();
                    flashFt.Style = defaultStyle;
                    flashFt.Update();
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "timer1_Tick");
                timer1.Stop();
            }
        }

        /// <summary>
        /// 获取查询结果表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clPopu-frmLsPqPopu-" + sFuns);
        }
    }
}