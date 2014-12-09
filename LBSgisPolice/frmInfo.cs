using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.IO;
using MapInfo.Windows.Controls;
using clPopu;
using clZonghe;


namespace LBSgisPolice
{
    public partial class FrmInfo : Form
    {
        public FrmInfo()
        {
            InitializeComponent();
        }
        private string strConn;
        System.Data.OracleClient.OracleConnection myConnection = null;
        System.Data.OracleClient.OracleDataAdapter oracleDat = null;
        System.Data.OracleClient.OracleCommand oracleCmd = null;
        System.Data.OracleClient.OracleDataReader dataReader = null;
        DataSet objset = null;


        private string[] conStr;
        public MapControl mapControl = null;
        private string tabName;
        public DataRow row =null;
        public string getFromNamePath;
        public string LayerName;
        private string houseNo = "";   // 获得房屋编号
        private string[] dqPopu = null, lsPopu = null, yzPopu = null, wzPopu = null; // 用数组存储查询结果，方便后面查询 (当前居住人数，历史居住人数，暂住证有效期内人数，未办暂住证人数)

        /// <summary>
        /// 传递features和数据库连接字符串获取信息
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="ftr">地图要素</param>
        /// <param name="Con">数据库连接参数</param>
        public void setInfo(Feature ftr,string[] Con) {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;

                this.dataGridView1.Rows.Clear();

                strConn = "user id=" + Con[1] + ";password=" + Con[2] + ";data source=" + Con[0];
                conStr = Con;

                //通过查看字段,如果含"表_ID",说明是临时表
                bool isTemTab = false;
                foreach (MapInfo.Data.Column col in ftr.Columns)
                {
                    String upAlias = col.Alias.ToUpper();
                    if (upAlias.IndexOf("表_ID") > -1)
                    {
                        isTemTab = true;
                        break;
                    }
                }
                DataTable dt = null;
                if (isTemTab)
                {
                    string strTabName = ftr["表名"].ToString();
                    if (strTabName.IndexOf("视频") >= 0)
                    {
                        strTabName = "视频";
                    }
                    //GetFromName getFromName = new GetFromName(strTabName);
                    CLC.ForSDGA.GetFromTable.GetFromName(strTabName, getFromNamePath);

                    string strSQL1 = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" 
                                               + ftr["表_ID"].ToString() + "'";

                    if (CLC.ForSDGA.GetFromTable.TableName == "安全防护单位")
                        strSQL1 = "select 编号,单位名称,单位性质,单位地址,主管保卫工作负责人,手机号码,所属派出所,所属中队,所属警务室," + 
                                  "'点击查看' as 安全防护单位文件,标注人,标注时间,X,Y from " 
                                  + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                  + CLC.ForSDGA.GetFromTable.ObjID + "='" 
                                  + ftr["表_ID"].ToString() + "'";

                    OracleConnection Conn = new OracleConnection(strConn);
                    try
                    {
                        Conn.Open();
                        OracleCommand cmd = new OracleCommand(strSQL1, Conn);
                        cmd.ExecuteNonQuery();
                        OracleDataAdapter apt = new OracleDataAdapter(cmd);
                        dt = new DataTable();
                        //dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strSQL1);
                        apt.Fill(dt);
                        row = dt.Rows[0];
                        cmd.Dispose();
                        Conn.Close();

                        if (dt.Rows.Count > 0)
                        {
                            foreach (System.Data.DataColumn dataCol in dt.Columns)
                            {
                                if (dataCol.Caption.IndexOf("备用字段") < 0 && dataCol.Caption != "所属派出所代码" 
                                                                            && dataCol.Caption != "所属中队代码" 
                                                                            && dataCol.Caption != "所属警务室代码"
                                                                            && dataCol.Caption != "抽取ID" 
                                                                            && dataCol.Caption != "抽取更新时间"
                                                                            && dataCol.Caption != "最后更新人"
                                                                            && dataCol.Caption != "配偶公民身份号码"
                                                                            && dataCol.Caption != "屋主身份证号码" 
                                                                            && dataCol.Caption != "住址门牌")
                                {
                                    if (dataCol.Caption == "相关案件" || dataCol.Caption == "当前居住人数" 
                                                                      || dataCol.Caption == "历史居住人数" 
                                                                      || dataCol.Caption == "暂住证有效期内人数" 
                                                                      || dataCol.Caption == "未办暂住证人数" 
                                                                      || dataCol.Caption == "涉案人员" 
                                                                      || (dt.Columns[0].Caption != "房屋编号" && dataCol.Caption == "房屋编号"))
                                        this.dataGridView1.Rows.Add(dataCol.Caption + ":", "");
                                    else
                                        this.dataGridView1.Rows.Add(dataCol.Caption + ":", dt.Rows[0][dataCol]);
                                }
                                if (dataCol.Table.Columns[0].Caption == "房屋编号")
                                    houseNo = dt.Rows[0]["房屋编号"].ToString();
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch
                    {
                        if (Conn.State == ConnectionState.Open)
                            Conn.Close();
                        return;
                    }
                }
                else
                {
                    foreach (MapInfo.Data.Column col in ftr.Columns)
                    {
                        String upAlias = col.Alias.ToUpper();
                        if (upAlias != "OBJ" && upAlias != "MI_STYLE" 
                                             && upAlias != "MI_KEY" 
                                             && upAlias.IndexOf("备用字段") < 0 
                                             && upAlias != "所属派出所代码" 
                                             && upAlias != "所属中队代码" 
                                             && upAlias != "所属警务室代码"
                                             && upAlias != "抽取ID" 
                                             && upAlias != "抽取更新时间" 
                                             && upAlias != "最后更新人")
                        {
                            this.dataGridView1.Rows.Add(new object[] { string.Format("{0}:", col.ToString()), string.Format("{0}", ftr[col.ToString()].ToString()) });
                        }
                    }
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, Control.MousePosition.X + 5, Control.MousePosition.Y + 5);           //设置位置
                this.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }

            #region 添加链接，相互关联
            string strPopu = "";   // 用于统计的sql;
            try
            {
                DataGridViewLinkCell Datagvl = null;
                // 找到要链接字段的位置
                int name = 0, dq = 0, ls = 0, fwNo = 0, xanj = 0, yb = 0, wb = 0, anRen = 0, file = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    string objName = this.dataGridView1.Rows[i].Cells[0].Value.ToString();
                    if (objName == "姓名:")
                        name = i;
                    if (objName == "安全防护单位文件:")
                        file = i;
                    if (objName == "相关案件:")
                        xanj = i;
                    if (objName == "当前居住人数:")
                    {
                        dq = i;
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        dqPopu = getPopuCount(strPopu, dqPopu);
                    }
                    if (objName == "历史居住人数:")
                    {
                        ls = i;
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        lsPopu = getPopuCount(strPopu, lsPopu);
                    }
                    if (objName == "房屋编号:")
                        fwNo = i;
                    if (objName == "暂住证有效期内人数:")
                    {
                        yb = i;
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                   " and 人口性质='暂住人口' and 暂住证号 is not null and 暂住证有效期限>sysdate";
                        yzPopu = getPopuCount(strPopu, yzPopu);
                    }
                    if (objName == "未办暂住证人数:")
                    {
                        wb = i;
                        strPopu = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and 人口性质='暂住人口' and ((暂住证号 is null) or (暂住证号 is not null and 暂住证有效期限<sysdate))";
                        wzPopu = getPopuCount(strPopu, wzPopu);
                    }
                    if (objName == "涉案人员:")
                        anRen = i;
                    if (objName == "屋主姓名:")
                    {
                        // 姓名加链接
                        Datagvl = new DataGridViewLinkCell();
                        Datagvl.Value = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                        Datagvl.ToolTipText = "查看屋主详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Datagvl;
                    }
                    if (objName == "配偶姓名:")
                    {
                        // 姓名加链接
                        Datagvl = new DataGridViewLinkCell();
                        Datagvl.Value = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                        Datagvl.ToolTipText = "查看配偶详细信息";
                        this.dataGridView1.Rows[i].Cells[1] = Datagvl;
                    }
                }
                string TabName = ftr["表名"].ToString();
                if (TabName == "人口系统")
                {
                    // 姓名加链接
                    Datagvl = new DataGridViewLinkCell();
                    Datagvl.Value = this.dataGridView1.Rows[0].Cells[1].Value.ToString();
                    Datagvl.ToolTipText = "查看照片";
                    this.dataGridView1.Rows[0].Cells[1] = Datagvl;

                    // 房屋编号加链接
                    DataGridViewLinkCell Datagvl2 = new DataGridViewLinkCell();
                    Datagvl2.Value = "点击查看相关房屋";
                    Datagvl2.ToolTipText = "查看相关房屋信息";
                    this.dataGridView1.Rows[fwNo].Cells[1] = Datagvl2;

                    // 相关案件加链接
                    DataGridViewLinkCell Datagvl3 = new DataGridViewLinkCell();
                    Datagvl3.Value = "点击查看相关案件";
                    Datagvl3.ToolTipText = "查看相关案件信息";
                    this.dataGridView1.Rows[xanj].Cells[1] = Datagvl3;
                }
                if (TabName == "出租屋房屋系统")
                {
                    // 当前居住人数加链接
                    Datagvl = new DataGridViewLinkCell();
                    if (dqPopu[0] == "" || dqPopu[0] == "0")
                        Datagvl.Value = "0";
                    else
                        Datagvl.Value = dqPopu.Length.ToString();
                    Datagvl.ToolTipText = "查看当前居住人信息";
                    this.dataGridView1.Rows[dq].Cells[1] = Datagvl;

                    // 历史居住人数加链接
                    DataGridViewLinkCell Datagvl2 = new DataGridViewLinkCell();
                    if (lsPopu[0] == "" || lsPopu[0] == "0")
                        Datagvl2.Value = "0";
                    else
                        Datagvl2.Value = lsPopu.Length.ToString();
                    Datagvl2.ToolTipText = "查看历史居住人信息";
                    this.dataGridView1.Rows[ls].Cells[1] = Datagvl2;

                    // 暂住证有效期内人数加链接
                    DataGridViewLinkCell Datagvl3 = new DataGridViewLinkCell();
                    if (yzPopu[0] == "" || yzPopu[0] == "0")
                        Datagvl3.Value = "0";
                    else
                        Datagvl3.Value = yzPopu.Length.ToString();
                    Datagvl3.ToolTipText = "查看暂住证有效期内人详细信息";
                    this.dataGridView1.Rows[yb].Cells[1] = Datagvl3;

                    // 身份证号码加链接
                    DataGridViewLinkCell Datagvl4 = new DataGridViewLinkCell();
                    if (wzPopu[0] == "" || wzPopu[0] == "0")
                        Datagvl4.Value = "0";
                    else
                        Datagvl4.Value = wzPopu.Length.ToString();
                    Datagvl4.ToolTipText = "查看未办暂住证人详细信息";
                    this.dataGridView1.Rows[wb].Cells[1] = Datagvl4;
                }
                if (TabName == "案件信息")
                {
                    // 涉案人员加链接
                    Datagvl = new DataGridViewLinkCell();
                    Datagvl.Value = "点击查看涉案人员信息";
                    Datagvl.ToolTipText = "查看涉案人员信息";
                    this.dataGridView1.Rows[anRen].Cells[1] = Datagvl;
                }
                if (TabName == "安全防护单位")
                {
                    // 涉案人员加链接
                    Datagvl = new DataGridViewLinkCell();
                    Datagvl.Value = "点击查看";
                    Datagvl.ToolTipText = "查看安全防护单位文件";
                    this.dataGridView1.Rows[file].Cells[1] = Datagvl;
                }
            }
            catch( Exception ex)
            {
                ExToLog(ex, "添加链接，相互关联");
            }
            #endregion
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
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
        /// 设置窗体显示位置
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="iWidth">窗体宽度</param>
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

        /// <summary>
        /// 根据sql查询编号并逐个保存到数组中   
        /// 最后编辑人 李立
        /// 最后编辑时间 2010-12-23
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
        /// 函数用途 用来转换房屋编号代码
        /// 最后编辑人   李立
        /// 最后编辑时间  2010-12-23
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

        private FrmZLMessage frmZL;
        private string[] strAnjan;
        private string tableName = "";
        /// <summary>
        /// 列表单击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2010-12-23
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                myConnection = new OracleConnection(strConn);
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

                    string connectionStr = "User ID=czrk_cx;" + 
                                           "Password=czrk_cx;" + 
                                           "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)" + 
                                                                    "(HOST=10.47.227.15)(PORT=1521)))" +
                                                                    "(CONNECT_DATA=(SID=ora81)))";

                    System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection(connectionStr);
                    try
                    {
                        clPopu.FrmImage fimage = new clPopu.FrmImage();
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
                                bmp.Save(Application.StartupPath + "\\lbs.jpg");
                                fimage.pictureBox1.ImageLocation = Application.StartupPath + "\\lbs.jpg";

                                bmp.Dispose();//释放bmp文件资源
                                fimage.pictureBox1.Invalidate();
                                fs.Close();
                                fimage.TopMost = true;
                                fimage.ShowDialog();
                                fimage.Dispose();
                                File.Delete(Application.StartupPath + "\\lbs.jpg");
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExToLog(ex, "dataGridView1_CellContentClick-查看照片");
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

                try
                {
                    tabName = ""; strAnjan = null;

                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "涉案人员:")
                        tabName = "涉案人员:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "安全防护单位文件:")
                        tabName = "安全防护单位文件:";
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

                }
                catch { }

                string strSQL = "", strMessage = "";
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
                            string renName = colName = colName.Substring(0, colName.Length - 1);
                            strMessage = renName + "未存档！";
                            break;
                        case "当前居住人数":
                        case "历史居住人数":
                        case "暂住证有效期内人数":
                        case "未办暂住证人数":
                            string cardId = arrayConStr(colName);
                            strAnjan = cardId.Split(',');
                            if (cardId != "")
                            {
                                cardId = cardId.Replace(",", "','");
                                strSQL = "select 身份证号码,姓名 from 人口系统 t where 身份证号码 in ('" + cardId + "')";
                            }
                            tableName = "人口系统";
                            string meName = colName = colName.Substring(0, colName.Length - 1);
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
                            strMessage = "未查询到此记录，请确认数据库中身份证号码是否正确!";
                            break;
                    }

                    if (strSQL != "")
                    {
                        myConnection.Open();
                        objset = new DataSet();
                        oracleDat = new OracleDataAdapter(strSQL, myConnection);
                        oracleDat.Fill(objset, "table");

                        if (objset.Tables.Count != 0)
                        {
                            if (objset.Tables[0].Rows.Count == 0)
                            {
                                MessageBox.Show(strMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (tabName != "" && tabName != "安全防护单位文件:" && tabName != "配偶公民身份号码:" && tabName != "屋主身份证号码:")
                            MessageBox.Show(strMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (strAnjan.Length > 1)
                        disPlayInfo(objset.Tables[0], tableName);
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "dataGridView1_CellContentClick");
                }
                finally
                {
                    if (strSQL != "")
                        oracleDat.Dispose();
                    if (myConnection.State == ConnectionState.Open)
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
                ExToLog(ex, "dataGridView1_CellContentClick-列表单击事件");
            }
        }

        /// <summary>
        /// 根据列名找到字符串数组转换为可供SQL使用的字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2010-12-23
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>条件字符串</returns>
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

        FrmHouseNumber frmNumber = new FrmHouseNumber();
        /// <summary>
        /// 显示列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2010-12-23
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="tableName">表名</param>
        private void disPlayInfo(DataTable dt,string tableName)
        {
            try
            {
                if (dt == null)
                {
                    frmNumber.Close();
                    return;
                }

                if (this.frmNumber.Visible == false)
                {
                    frmNumber = new FrmHouseNumber();
                    frmNumber.Show();
                    frmNumber.Visible = true;
                }
                frmNumber.mapControl = mapControl;
                frmNumber.getFromNamePath = getFromNamePath;
                frmNumber.LayerName = LayerName;
                frmNumber.strConn = strConn;
                frmNumber.setfrmInfo(dt, tableName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo-显示列表");
            }
        }

        /// <summary>
        /// 单条记录的查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void GetAnjanInfo()
        {
            string strSQL = "", tableName = "";
            try
            {
                string colName = tabName.Substring(0, tabName.Length - 1);
                switch (colName)
                {
                    case "相关案件": 
                        CLC.ForSDGA.GetFromTable.GetFromName("案件信息",getFromNamePath);
                        string AnNumber = row[colName].ToString();
                        if (AnNumber != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.FrmFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + AnNumber + "'";
                        tableName = "案件信息";
                        break;
                    case "房屋编号":
                        CLC.ForSDGA.GetFromTable.GetFromName("出租屋房屋系统",getFromNamePath);
                        string houseNo = row[colName].ToString();
                        if (houseNo != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.FrmFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + houseNo + "'";
                        tableName = "出租屋房屋系统";
                        break;
                    case "涉案人员":
                    case "历史居住人数":
                    case "当前居住人数":
                    case "暂住证有效期内人数":
                    case "未办暂住证人数": 
                        CLC.ForSDGA.GetFromTable.GetFromName("人口系统",getFromNamePath);
                        string cardId = row[colName].ToString();
                        if (cardId != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.FrmFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + cardId + "'";
                        tableName = "人口系统";
                        break;
                    case "配偶公民身份号码":
                    case "屋主身份证号码":
                        CLC.ForSDGA.GetFromTable.GetFromName("人口系统", getFromNamePath);
                        string sfzID = row[colName].ToString();
                        if (sfzID != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + sfzID + "'";
                        break;
                }

                if (strSQL != "")
                {
                    myConnection.Open();
                    objset = new DataSet();
                    oracleDat = new System.Data.OracleClient.OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");
                }
                else
                {
                    return;
                }

                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("所查记录未存档！", "提示");
                    return;
                }
                else
                {
                    Point pt = new Point();
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);

                    disPlayInfo(objset.Tables[0], pt,tableName);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetAnjanInfo-单条记录的查询");
            }
            finally
            {
                oracleDat.Dispose();
                myConnection.Close();
            }
        }
        private FrmHouseInfo frmglMessage = new FrmHouseInfo();
        /// <summary>
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">位置</param>
        /// <param name="tableName">表名</param>
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
                frmglMessage.getFromNamePath = getFromNamePath;
                frmglMessage.LayerName = LayerName;
                frmglMessage.strConn = strConn;
                frmglMessage.setInfo(dt.Rows[0], pt, tableName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo-显示详细信息");
            }
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
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPolice-FrmInfo-" + sFunc);
        }

        /// <summary>
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">要执行的SQL</param>
        /// <returns>结果内存表</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }
    }
}

