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
        //读取配置文件，设置数据库连接字符串
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");

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

        // 数据库访问
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
            catch (Exception ex) { ExToLog(ex, "FrmInfo-构造函数"); }
        }

        private string[] dqPopu = null, lsPopu = null, wzPopu = null, yzPopu = null;  // 数组存储查询结果，方便后面查询
        /// <summary>
        /// 筛选显示字段并添加连接
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dRow">记录行</param>
        /// <param name="pt">显示位置</param>
        /// <param name="LayName">图层名称</param>
        internal void setInfo(DataRow dRow,Point pt,string LayName)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                row = dRow;
                LayerName = LayName;
                string houseNo = "";   // 获得房屋编号

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码"     
                                                            && col.Caption != "所属中队代码"  
                                                            && col.Caption != "所属警务室代码" 
                                                            && col.Caption != "抽取ID" 
                                                            && col.Caption != "抽取更新时间" 
                                                            && col.Caption != "最后更新人" 
                                                            && col.Caption != "屋主身份证号码" 
                                                            && col.Caption != "配偶公民身份号码" 
                                                            && col.Caption != "住址门牌")
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
                    if (col.Table.Columns[0].Caption == "房屋编号")
                        houseNo = dRow.Table.Rows[0]["房屋编号"].ToString();
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
                this.Visible = true;
                string strPopu = "";   // 用于统计的sql;

                //　存储某列的位置
                #region 加上链接，相互关连
                ////////////////------加上链接，相互关连-------/////////////////////
                int k = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "涉案人员:" || dataGridView1.Rows[i].Cells[0].Value.ToString() == "相关案件:" 
                                                                                       || dataGridView1.Rows[i].Cells[0].Value.ToString() == "房屋编号:" 
                                                                                       || dataGridView1.Rows[i].Cells[0].Value.ToString() == "安全防护单位文件:")
                    {
                        k = i;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "当前居住人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        dqPopu = getPopuCount(strPopu, dqPopu);

                        //dqPopu = new string[] { };
                        //dqPopu = row["当前居住人数"].ToString().Split(',');

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (dqPopu[0] == "" || dqPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = dqPopu.Length.ToString();
                        Dgvlink.ToolTipText = "查看当前居住人详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "历史居住人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        lsPopu = getPopuCount(strPopu, lsPopu);

                        //lsPopu = new string[] { };
                        //lsPopu = row["历史居住人数"].ToString().Split(','); 

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (lsPopu[0] == "" || lsPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = lsPopu.Length.ToString();
                        Dgvlink.ToolTipText = "查看历史居住人详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "暂住证有效期内人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and 人口性质='暂住人口' and 暂住证号 is not null and 暂住证有效期限>sysdate";
                        yzPopu = getPopuCount(strPopu, yzPopu);

                        //yzPopu = new string[] { };
                        //yzPopu = row["暂住证有效期内人数"].ToString().Split(',');

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (yzPopu[0] == "" || yzPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = yzPopu.Length.ToString();
                        Dgvlink.ToolTipText = "查看暂住证有效期内人详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "未办暂住证人数:")
                    {
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and 人口性质='暂住人口' and ((暂住证号 is null) or (暂住证号 is not null and 暂住证有效期限<sysdate))";
                        wzPopu = getPopuCount(strPopu, wzPopu);

                        //wzPopu = new string[] { };
                        //wzPopu = row["未办暂住证人数"].ToString().Split(',');

                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        if (wzPopu[0] == "" || wzPopu[0] == "0")
                            Dgvlink.Value = "0";
                        else
                            Dgvlink.Value = wzPopu.Length.ToString();
                        Dgvlink.ToolTipText = "查看未办暂住证人详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "屋主姓名:")
                    {
                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        Dgvlink.Value = row["屋主姓名"].ToString();
                        Dgvlink.ToolTipText = "查看屋主人详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "配偶姓名:")
                    {
                        DataGridViewLinkCell Dgvlink = new DataGridViewLinkCell();
                        Dgvlink.Value = row["配偶姓名"].ToString();
                        Dgvlink.ToolTipText = "查看配偶详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Dgvlink;
                        continue;
                    }
                }

                DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                if (dataGridView1.Rows[k].Cells[0].Value.ToString() == "涉案人员:")
                {
                    dgvlc.Value = "查看相关涉案人员信息";
                    dgvlc.ToolTipText = "查看相关涉案人员信息";
                    this.dataGridView1.Rows[k].Cells[1] = dgvlc;
                }
                if (dataGridView1.Rows[k].Cells[0].Value.ToString() == "相关案件:")
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "房屋编号:")
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
                if (dataGridView1.Rows[k].Cells[0].Value.ToString() == "安全防护单位文件:")
                {
                    dgvlc.Value = "查看文件";
                    dgvlc.ToolTipText = "查看安全防护单位文件";
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
        /// 根据sql查询人数并逐个保存到数组中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="poStr">sql语句</param>
        /// <param name="conPo">用于存放人数的数组</param>
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
                ExToLog(ex, "getPopuCount-根据sql查询人数并逐个保存到数组中");
                return conPo;
            }
        }

        /// <summary>
        /// 用来转换房屋编号代码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                ExToLog(ex, "ConversionStr-转换房屋编号代码");
                return "";
            }
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

        /// <summary>
        /// 设置窗体位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="iWidth">窗体宽度</param>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
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


        // 单击事件
        private FrmZLMessage frmZL;
        private FrmHouseNumber frmNumber;
        private string[] strAnjan;
        private string tableName = "";
        /// <summary>
        /// 单击列表事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (strConn == "")
                {
                    MessageBox.Show("读取配置文件时发生错误,请修改配置文件后重试!");
                    return;
                }
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
                        clZonghe.FrmImage fimage = new clZonghe.FrmImage();
                        oraconn.Open();
                        string sqlstr = "";
                        if (dataGridView1.Rows[r].Cells[1].Value.ToString() == "常住人口")
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
                                System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, bytes.Length);
                                dataReader.Close();

                                Stream fs = new MemoryStream(bytes);
                                //创建一个bitmap类型的bmp变量来读取文件。
                                Bitmap bmp = new Bitmap(Image.FromStream(fs));

                                bmp.Save(Application.StartupPath + "\\Zhonghe.jpg");
                                fimage.pictureBox1.ImageLocation = Application.StartupPath + "\\Zhonghe.jpg";
                                bmp.Dispose();//释放bmp文件资源
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
                            System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "涉案人员:")
                    tabName = "涉案人员:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "相关案件:")
                    tabName = "相关案件:";
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "安全防护单位文件:")
                    tabName = "安全防护单位文件:";
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
                DataTable table = new DataTable();
                try
                {
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
                        case "安全防护单位文件":
                            if (this.frmZL != null)
                            {
                                if (this.frmZL.Visible == true)
                                {
                                    this.frmZL.Close();
                                }
                            }

                            if (dataGridView1.Rows[1].Cells[1].Value.ToString() == "")
                            {
                                MessageBox.Show("名称不能为空！", "提示");
                                return;
                            }
                            this.frmZL = new FrmZLMessage(dataGridView1.Rows[1].Cells[1].Value.ToString(), strConn);
                            //设置信息框在右下角  
                            System.Drawing.Point po = new Point();
                            po.X = Screen.PrimaryScreen.WorkingArea.Width;
                            po.Y = Screen.PrimaryScreen.WorkingArea.Height;
                            this.frmZL.SetDesktopLocation(po.X - frmZL.Width, po.Y - frmZL.Height);
                            this.frmZL.Show();
                            break;
                        case "配偶公民身份号码":
                        case "屋主身份证号码":
                            tableName = "人口系统";
                            GetAnjanInfo();
                            strMessage = "未查询到此记录，请管理确认数据库中身份证号码是否正确!";
                            break;
                    }
                    if (strSQL != "")
                    {
                        myConnection.Open();
                        oracleDat = new OracleDataAdapter(strSQL, myConnection);
                        oracleDat.Fill(table);

                        if (table.Rows.Count == 0)
                        {
                            MessageBox.Show(strMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            return;
                        }
                    }
                    else
                    {
                        if (tabName != "" && tabName != "安全防护单位文件:" && tabName != "配偶公民身份号码:" && tabName != "屋主身份证号码:")
                            MessageBox.Show(strMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
                        // 如果只有一条记录则直接显示信息
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
        /// 根据列名找到字符串数组转换为可供SQL使用的字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>sql</returns>
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
        /// 显示列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="tableName">表名</param>
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
        /// 单条记录的查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                    case "相关案件":
                        CLC.ForSDGA.GetFromTable.GetFromName("案件信息", getFromNamePath);
                        string AnNumber = row[colName].ToString();
                        if (AnNumber != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID+ "='" + AnNumber + "'";
                        break;
                    case "房屋编号":
                        CLC.ForSDGA.GetFromTable.GetFromName("出租屋房屋系统", getFromNamePath);
                        string houseNo = row[colName].ToString();
                        if (houseNo != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" + houseNo + "'";
                        break;
                    case "涉案人员":
                        CLC.ForSDGA.GetFromTable.GetFromName("人口系统", getFromNamePath);
                        string anRen = row[colName].ToString();
                        if (anRen != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" + anRen + "'";
                        break;
                    case "当前居住人数":
                    case "历史居住人数":
                    case "暂住证有效期内人数":
                    case "未办暂住证人数":
                        CLC.ForSDGA.GetFromTable.GetFromName("人口系统", getFromNamePath);
                        string cardId = arrayConStr(colName);
                        if (cardId != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where "
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" + cardId + "'";
                        break;
                    case "配偶公民身份号码":
                    case "屋主身份证号码":
                        CLC.ForSDGA.GetFromTable.GetFromName("人口系统", getFromNamePath);
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
                    MessageBox.Show("所查记录未存档！", "提示");
                    return;
                }
                Point pt = new Point();
                pt.X = Convert.ToInt32(table.Rows[0]["X"]);
                pt.Y = Convert.ToInt32(table.Rows[0]["Y"]);

                disPlayInfo(table, pt, "人口系统");
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
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>查询结果集</returns>
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
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clZonghe-FrmInfo-" + sFunc);
        }
    }
}

