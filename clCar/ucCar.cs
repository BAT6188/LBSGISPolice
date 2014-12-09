//********顺德公安项目-车辆监控模块******
//********创建人：jie.zhang
//********创建日期： 2008.9.10
//********版本修改：
//********   1. 2009.4.15 修改移动车辆视频
//********   2. 2009.5.8   修改视频图标大小，并移动视频放在顶层
//********   3. 2009.5.13  移动视频的标注与普通不同
//                         修改跟踪视频时的初始值问题
//********版权所有：上海数字位图信息科技有限公司

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Xml;
using System.IO;


using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace clCar
{
    public partial class ucCar : UserControl
    {
        //亘古不变的变量
        static MapControl mapControl1 = null;
        static string mysqlstr;    //数据库连接字符串
        static string[] StrCon;   // 数据库连接字符串组
        static ToolStripDropDownButton toolDDbtn;  //工具栏菜单


        //jie.zhang 2008.9.24
        public Boolean GetCarflag = false; //开始车辆监控的标志
        public string JZCarName = "";  //居中车辆的名称
        public string GzCarName = "";  // 跟踪车辆的名称
        public string CanNm;
        public frmGuijiTime fhistory;  //历史轨迹窗体        
        public IResultSetFeatureCollection rsfcView;//范围车辆的图元集合
        public string[] carn; //各个车辆的号牌
        public double[] lastx; //各个车辆上次的精度
        public double[] lasty; //各个车辆上次的纬度
        public double xx;  //当前车辆的上次经度
        public double yy;  //当前车辆的上次纬度
        public Boolean SetViewFlag = false;//设置范围标识符
        public int iflash = 0;
        public Boolean ZhiHui = false;

        public string strRegion = string.Empty; //派出所用户区域
        public string strRegion1 = "";             //中队用户区域
        public string user = "";                      //登陆用户

        //导出变量
        public string strRegion2 = ""; //可导出的派出所
        public string strRegion3 = ""; //可导出的中队
        public string excelSql = "";   //查询导出sql
        public int _startNo, _endNo;   // 可导出分页数

        public System.Data.DataTable dtExcel = null; //地图页面数据导出表
        OracleDataAdapter apt1 = null;

        public ToolStripProgressBar toolPro;  // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolProLbl;     // 用于显示进度文本　
        public ToolStripSeparator toolProSep;



        /// <summary>
        ///  初始化车辆模块
        /// </summary>
        /// <param name="m">地图控件</param>
        /// <param name="t">工具栏菜单</param>
        /// <param name="s">数据库连接参数</param>
        /// <param name="zh">直观指挥的标示</param>
        public ucCar(MapControl m, ToolStripDropDownButton t, string[] s, Boolean zh)
        {
            InitializeComponent();

            try
            {
                mapControl1 = m;
                toolDDbtn = t;
                StrCon = s;
                this.ZhiHui = zh;
                mysqlstr = "data source=" + StrCon[0] + ";user id=" + StrCon[1] + ";password=" + StrCon[2];
                mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);
                //mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(mapControl1_ViewChanged);

                fhistory = new frmGuijiTime(m, s);

                toolDDbtn.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolSelect);
                this.fhistory.Visible = false;
                this.ToolCarDisable();

                this.comboBox1.Items.Clear();
                this.comboBox1.Items.Add("终端车辆号牌");
                this.comboBox1.Items.Add("所属单位");
                this.comboBox1.Text = this.comboBox1.Items[0].ToString();

            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-01-初始化车辆模块");
            }
        }

        /// <summary>
        /// 获取居中标识符
        /// </summary>
        /// <param name="jz"></param>
        public void getjzParameter(Boolean jz)
        {
            try
            {
                fhistory.getflag(jz);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-02-获取居中标识符");
            }
        }

        /// <summary>
        /// 绑定DataGrid
        /// </summary>
        public void AddGrid()
        {
            try
            {
                isShowPro(true);
                if (CanNm != "")
                {
                    string sql = string.Empty;
                    if (strRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show("没有设置区域权限", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (strRegion == "顺德区")
                    {
                        if (CanNm == "All")
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统";
                        }
                        else
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统 where " + CanNm;
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(strRegion.Split(','), "大良") > -1 && strRegion.IndexOf("德胜") < 0)
                        {
                            strRegion = strRegion.Replace("大良", "大良,德胜");
                        }
                        if (CanNm == "All")
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统 where 权限单位 in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统 where " + CanNm + " and 权限单位 in('" + strRegion.Replace(",", "','") + "')";
                        }
                    }

                    // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                    if (sql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                        sql += " and (备用字段一 is null or 备用字段一='')";
                    else
                        sql += " where (备用字段一 is null or 备用字段一='')";
                    //-------------------------------------------------------

                    string Gzstring1 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break; 

                        if (GzArrayName[i] != "")
                        {
                            Gzstring1 = Gzstring1 + " 终端车辆号牌='" + GzArrayName[i] + "' or ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring1 = Gzstring1.Substring(0, Gzstring1.LastIndexOf("or") - 1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring1 = Gzstring1 + " 终端车辆号牌='" + GzArrayName[i] + "'";
                        }
                    }


                    string Gzstring2 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring2 = Gzstring2 + " 终端车辆号牌<>'" + GzArrayName[i] + "' and ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring2 = Gzstring2.Substring(0, Gzstring2.LastIndexOf("and") - 1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring2 = Gzstring2 + " 终端车辆号牌<>'" + GzArrayName[i] + "'";
                        }
                    }

                    string tsql = string.Empty;

                    if (Gzstring1 != "" && Gzstring2 != "")
                        tsql = sql + " and " + Gzstring1 + " Union all " + sql + " and " + Gzstring2;
                    else
                        tsql = sql;

                    if (tsql == "") return;

                    DataTable dt = GetTable(tsql);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("查询结果为0！","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                        return;
                    }

                    #region 导出Excel
                    try
                    {
                        excelSql = sql;
                        excelSql = "select * " + excelSql.Substring(excelSql.IndexOf("from"));
                        string sRegion2 = strRegion2;
                        string sRegion3 = strRegion3;

                        if (strRegion2 != "顺德区")
                        {
                            if (strRegion2 != "")
                            {
                                if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                                {
                                    sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                                }
                                excelSql += " and (权限单位 in ('" + sRegion2.Replace(",", "','") + "'))";
                            }
                            else if (strRegion2 == "")
                            {
                                if (excelSql.IndexOf("where") < 0)
                                {
                                    excelSql += " where 1=2 ";
                                }
                                else
                                {
                                    excelSql += " and 1=2 ";
                                }
                            }
                        }

                        // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                        if (excelSql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                            excelSql += " and (备用字段一 is null or 备用字段一='')";
                        else
                            excelSql += " where (备用字段一 is null or 备用字段一='')";
                        //-------------------------------------------------------
                        _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                        _endNo = pageSize1;

                    //    OracleConnection orc = new OracleConnection(mysqlstr);
                    //    try
                    //    {
                    //        orc.Open();
                    //        OracleCommand cmd = new OracleCommand(excelSql, orc);
                    //        apt1 = new OracleDataAdapter(cmd);
                    //        DataTable datatableExcel = new DataTable();
                    //        apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //        if (dtExcel != null) dtExcel.Clear();
                    //        dtExcel = datatableExcel;
                    //        cmd.Dispose();
                    //    }
                    //    catch
                    //    {
                    //        isShowPro(false);
                    //    }
                    //    finally { orc.Close(); }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    # endregion
                    
                    this.toolPro.Value = 2;
                    Application.DoEvents();

                    Pagedt1 = dt;
                    InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, this.dataGridView1);

                    //this.label2.Text = "共有" + dt.Rows.Count.ToString() + "条记录";

                    //dataGridView1.DataSource = dt;                    
                    //dataGridView1.Refresh();
                    WriteEditLog(sql, "ucCar-04-查询");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "ucCar-03-绑定DataGrid");
            }

        }

        //==========
        //==========
        //翻页功能
        //==========
        //==========

        int pageSize1 = 100;     //每页显示行数
        int PagenMax1 = 0;         //总记录数
        int pageCount1 = 0;    //页数＝总记录数/每页显示行数
        int pageCurrent1 = 0;   //当前页号
        int PagenCurrent1 = 0;      //当前记录行 
        DataSet Pageds1 = new DataSet();
        DataTable Pagedt1 = new DataTable();

        /// <summary>
        /// 得到最大的查询记录数
        /// </summary>
        /// <param name="sql">查询语句</param>
        private void getMaxCount1(string sql)//
        {
            PagenMax1 = GetScalar(sql);
        }

        //edit by fisher in 09-11-23
        public void InitDataSet1(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize1 = 100;      //设置页面行数
                PagenMax1 = Pagedt1.Rows.Count;
                TextNum1.Text = pageSize1.ToString();
                lblcount.Text = PagenMax1.ToString() + "条";//在导航栏上显示总记录数

                pageCount1 = (PagenMax1 / pageSize1);//计算出总页数
                if ((PagenMax1 % pageSize1) > 0) pageCount1++;
                if (PagenMax1 != 0)
                {
                    pageCurrent1 = 1;
                }
                PagenCurrent1 = 0;       //当前记录数从0开始

                LoadData1(textNowPage, lblPageCount, bs, bn, dgv);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-04-InitDataSet1");
            }
        }

        //edit by fisher in 09-11-23
        public void LoadData1(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                isShowPro(true);
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = Pagedt1.Clone();

                if (pageCurrent1 == pageCount1)
                    nEndPos = PagenMax1;
                else
                    nEndPos = pageSize1 * pageCurrent1;
                nStartPos = PagenCurrent1;

                //tsl.Text = Convert.ToString(pageCurrent1) + "/" + pageCount1.ToString();
                textNowPage.Text = Convert.ToString(pageCurrent1);
                lblPageCount.Text = "/" + pageCount1.ToString();
                this.toolPro.Value = 1;
                Application.DoEvents();
                _startNo = nStartPos;
                _endNo = nEndPos;

                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt1.Rows[i]);
                    PagenCurrent1++;
                }

                bds.DataSource = dtTemp;
                bdn.BindingSource = bds;
                this.toolPro.Value = 2;
                Application.DoEvents();
                dgv.DataSource = bds;
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "ucCar-05-LoadData");
            }
        }

        /// <summary>
        /// 翻页功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "上一页")
                {
                    pageCurrent1--;
                    if (pageCurrent1 < 1)
                    {
                        pageCurrent1 = 1;
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！");
                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                if (e.ClickedItem.Text == "下一页")
                {
                    pageCurrent1++;
                    if (pageCurrent1 > pageCount1)
                    {
                        pageCurrent1 = pageCount1;

                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！");

                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (pageCurrent1 <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent1 = 1;
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                else if (e.ClickedItem.Text == "转到尾页")
                {
                    if (pageCurrent1 > pageCount1 - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent1 = pageCount1;
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                else if (e.ClickedItem.Text == "数据导出")
                {
                    //DataExport();
                }

                #region 数据导出
                //DataTable datatableExcel = new DataTable();
                //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion

            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-06-翻页功能");
            }
        }


        //以下代码由fisher添加，旨在设置每页显示的数据数量
        /// <summary>
        /// 设置每页显示的数据数量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && this.dataGridView1.Rows.Count > 0)
                {
                    pageSize1 = Convert.ToInt32(TextNum1.Text);
                    pageCurrent1 = 1;   //当前转到第一页
                    PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    pageCount1 = (PagenMax1 / pageSize1);//计算出总页数
                    if ((PagenMax1 % pageSize1) > 0) pageCount1++;

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);

                    #region 数据导出
                    //DataTable datatableExcel = new DataTable();
                    //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-07-设置每页显示的数据数量");
            }
        }

        //以下代码由fisher添加，旨在实现页面转向（09-11-23）
        /// <summary>
        /// 页面转向
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageNow1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(this.PageNow1.Text) < 1 || Convert.ToInt32(this.PageNow1.Text) > pageCount1)
                    {
                        System.Windows.Forms.MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow1.Text = pageCurrent1.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent1 = Convert.ToInt32(this.PageNow1.Text);
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);

                        #region 数据导出
                        //DataTable datatableExcel = new DataTable();
                        //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                        //if (dtExcel != null) dtExcel.Clear();
                        //dtExcel = datatableExcel;
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-08-页面转向");
            }
        }


        /// <summary>
        /// 设置DataGrid颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dataGridView1.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }

                    for (int k = 0; k < this.dataGridView1.Rows.Count; k++)
                        for (int j = 0; j < this.GzArrayName.Length; j++)
                            if (GzArrayName[j] != "")
                                if (GzArrayName[j] == this.dataGridView1.Rows[k].Cells["终端车辆号牌"].Value.ToString())                                
                                    this.dataGridView1.Rows[k].DefaultCellStyle.BackColor = this.GzArrayColor[j];
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-09-设置DataGrid颜色");
            }
        }

        private string SelectCarName = string.Empty;
        //当前选择的车辆牌号

        /// <summary>
        /// 创建车辆临时图层
        /// </summary>
        public void CreateCarLayer()
        {

           #region  数据绑定的方式读取图元

            try
            {

                if (mapControl1.Map.Layers["CarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                }

                if (mapControl1.Map.Layers["CarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarLabel");
                }

                string tableAlies = "CarLayer";


                //string strSQL = "Select 终端车辆号牌 as Name,终端ID号码 as 表_ID,'GPS警车定位系统' as 表名,X,Y from GPS警车定位系统 ";

                string strSQL = string.Empty;
                
                if (this.strRegion == string.Empty)
                {
                    MessageBox.Show(@"没有设置区域权限", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (strRegion == "顺德区")
                {
                    strSQL = "Select 终端车辆号牌 as Name,终端ID号码 as 表_ID,'GPS警车定位系统' as 表名,X,Y from GPS警车定位系统 ";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    strSQL = "Select 终端车辆号牌 as Name,终端ID号码 as 表_ID,'GPS警车定位系统' as 表名,X,Y from GPS警车定位系统 where 权限单位 in ('" + strRegion.Replace(",", "','") + "') ";
                }


                DataTable dt = this.GetTable(strSQL);

                if (dt == null || dt.Rows.Count < 1)
                {
                    return;
                }

                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tableAlies, dt);
                MapInfo.Data.SpatialSchemaXY xy = new SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0,0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;



                MapInfo.Data.Table temTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                FeatureLayer temlayer = new FeatureLayer(temTable, tableAlies);

                mapControl1.Map.Layers.Add(temlayer);


                //改变图层的图元样式 
                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("jCar.bmp", BitmapStyles.None, System.Drawing.Color.Red, 16));
                FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, cs);
                temlayer.Modifiers.Clear();

                //ProtectMap();

                temlayer.Modifiers.Append(fsm);


                //添加标注
                const string activeMapLabel = "CarLabel";
                Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");
                LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);

                LabelSource lbsource = new LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                //lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                //lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);               // ProtectMap(); 
                mapControl1.Map.Layers.Add(lblayer);


                //// 设置跟踪对象
                //SetGZPolice();


                //设置居中对象
                SetJZPolice();



                if (this.ZhiHui == true)
                {
                    SetTableDisable("CarLayer");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-10-创建车辆临时图层");
            }

            #endregion
        }



        // 设置跟踪对象
        private void SetGZPolice()
        {
            //ProtectMap();

            //string sql = "Select X,Y from GPS警车定位系统 where 终端车辆号牌 ='" + this.GzCarName + "'";
            //DataTable dt = GetTable(sql);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    double xv = Convert.ToDouble(dt.Rows[0]["X"]);
            //    double yv = Convert.ToDouble(dt.Rows[0]["Y"]);
            //    Trackline(xx, yy, xv, yv);

            //    xx = xv;
            //    yy = yv;

            //    DPoint dpoint = new DPoint(xv, yv);
            //    Boolean inflag = IsInBounds(xv, yv);
            //    if (inflag == false)
            //    {
            //        mapControl1.Map.Center = dpoint;
            //    }
            //}
        }


        private void SetJZPolice()
        {
            try
            {
                ProtectMap();

                string sql = "Select X,Y from  GPS警车定位系统 where 终端车辆号牌 ='" + this.JZCarName + "'";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    double xv = Convert.ToDouble(dt.Rows[0]["X"]);
                    double yv = Convert.ToDouble(dt.Rows[0]["Y"]);

                    DPoint dpt = new DPoint(xv, yv);
                    mapControl1.Map.Center = dpt;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetJZPolice");
            }
        }

        private void ProtectMap()
        {
            try
            {
                if (this.Visible == false)   //jie.zhang 2010-0826  如果GPS警员模块不可见，不进行图元的添加
                {
                    if (mapControl1.Map.Layers["CarLayer"] != null)
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                    if (mapControl1.Map.Layers["CarLabel"] != null)
                        mapControl1.Map.Layers.Remove("CarLabel");

                    return;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ProtectMap");
            }
        }


        /// <summary>
        /// 创建移动车辆临时图层
        /// </summary>
        public void CreateVideoCarLayer()
        {
            #region 数据绑定方式
            try
            {
                string tableAlies = "VideoCarLayer";

                if (mapControl1.Map.Layers[tableAlies] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable(tableAlies);
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }                

                string strSQL = string.Empty;// "Select 终端车辆号牌 as Name,CAMID as 设备编号,X,Y from  GPS警车定位系统 where CAMID is not null and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2;

                if (this.strRegion == string.Empty)
                {
                    MessageBox.Show(@"没有设置区域权限", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (this.strRegion == "顺德区")
                {
                    strSQL = "Select 终端车辆号牌 as Name,CAMID as 设备编号,X,Y from  GPS警车定位系统 where CAMID is not null ";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        this.strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    strSQL = "Select 终端车辆号牌 as Name,CAMID as 设备编号,X,Y from  GPS警车定位系统 where CAMID is not null and 派出所名 in ('" + this.strRegion.Replace(",", "','") + "')";
                }

                DataTable dt = this.GetTable(strSQL);
                if (dt == null || dt.Rows.Count < 1)
                {
                    return;
                }
               
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tableAlies, dt);
                MapInfo.Data.SpatialSchemaXY xy = new SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0,0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;

                MapInfo.Data.Table temTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                FeatureLayer temlayer = new FeatureLayer(temTable, tableAlies);

                mapControl1.Map.Layers.Add(temlayer);

                //改变图层的图元样式
                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("ydsp.bmp", BitmapStyles.None, System.Drawing.Color.Red, 20));
                FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, cs);
                temlayer.Modifiers.Clear();
                temlayer.Modifiers.Append(fsm);


                //添加标注
                string activeMapLabel = "VideoCarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableAlies);
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.YellowGreen;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                // 设置跟踪对象
                SetGZPolice();

                //设置居中对象
                SetJZPolice();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-11-创建移动车辆临时图层");
            }

            #endregion
        }

        /// <summary>
        /// 设置图层可见
        /// </summary>
        /// <param name="tablename"></param>
        public void SetTableVisable(string tablename)
        {
            try
            {
                for (int i = 0; i < mapControl1.Map.Layers.Count; i++)
                {
                    IMapLayer layer = mapControl1.Map.Layers[i];

                    if (layer is FeatureLayer)
                    {
                        if (layer.Name == tablename)
                        {
                            layer.Enabled = true;
                        }
                    }
                    else if (layer is LabelLayer)
                    {
                        LabelLayer lLayer = (LabelLayer)layer;
                        for (int m = 0; m < lLayer.Sources.Count; m++)
                        {

                            if (lLayer.Sources[m].Name == tablename)
                            {
                                lLayer.Sources[m].Enabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-12-设置图层可见");
            }
        }

        /// <summary>
        /// 设置图层不可见
        /// </summary>
        /// <param name="tablename"></param>
        public void SetTableDisable(string tablename)
        {
            try
            {
                this.GetCarflag = false;
                for (int i = 0; i < mapControl1.Map.Layers.Count; i++)
                {
                    IMapLayer layer = mapControl1.Map.Layers[i];

                    if (layer is FeatureLayer)
                    {
                        if (layer.Name == tablename)
                        {
                            layer.Enabled = false;
                        }
                    }
                    else if (layer is LabelLayer)
                    {
                        LabelLayer lLayer = (LabelLayer)layer;
                        for (int m = 0; m < lLayer.Sources.Count; m++)
                        {

                            if (lLayer.Sources[m].Name == tablename)
                            {
                                lLayer.Sources[m].Enabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-13-设置图层不可见");
            }
        }


        /// <summary>
        /// 创建轨迹层
        /// </summary>
        public void CreateTrackLayer()
        {
            try
            {
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;

                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarTrack");
                Table tblTemp = Cat.GetTable("CarTrack");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarTrack");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);


                //添加标注
                //string activeMapLabel = "CarTrackLabel";
                //MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarTrack");
                //MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                //lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                //lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                //lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                ////lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                ////lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                //lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                //lbsource.DefaultLabelProperties.Caption = "Name";
                //lblayer.Sources.Append(lbsource);
                //mapControl1.Map.Layers.Add(lblayer);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-14-创建轨迹层");
            }
        }

        /// <summary>
        /// 初始化添加车辆
        /// </summary>
        public void AddCarFtr()
        {
            try
            {
                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["CarLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");

                string sqlcmd = string.Empty;
                if (strRegion == string.Empty)
                {
                    MessageBox.Show("没有设置区域权限", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (strRegion == "顺德区")
                {
                    sqlcmd = "Select * from GPS警车定位系统 where CAMID is null";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    sqlcmd = "Select * from GPS警车定位系统 where 权限单位 in ('" + strRegion.Replace(",", "','") + "') and CAMID is null";
                }

                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["终端车辆号牌"].ToString();
                        string camid = dr["终端ID号码"].ToString();

                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;                       
                        ftr["Name"] = tempname;
                        //ftr["CAMERID"] = camid;
                        ftr["表_ID"] = camid;
                        ftr["表名"] = "GPS警车定位系统";
                       

                        if (tempname == GzCarName)
                        {
                            //Trackline(xx, yy, xv, yv);
                            xx = xv;
                            yy = yv;
                            MapInfo.Geometry.DPoint dpoint = new DPoint(xv, yv);
                            Boolean inflag = this.IsInBounds(xv, yv);
                            if (inflag == false)
                            {
                                mapControl1.Map.Center = dpoint;
                            }
                           
                            
                            cs.ApplyStyle(new BitmapPointStyle("jCar.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));

                            ////修改标注
                            //LabelProperties properties = new LabelProperties();
                            //properties.Attributes = LabelAttribute.VisibilityEnabled | LabelAttribute.Caption | LabelAttribute.PriorityMinor;
                            //properties.Visibility.Enabled = true;
                            //properties.Caption = "Name";

                            //SelectionLabelModifier modifer = new SelectionLabelModifier();
                            //modifer.Properties.Add(ftr.Key, properties);
                            //LabelLayer lblLayer = map.Layers[0] as LabelLayer;
                            //LabelSource source = lblLayer.Sources[0];
                            //source.Modifiers.Append(modifer);


                        }
                        else
                        {
                            cs.ApplyStyle(new BitmapPointStyle("jCar.BMP", BitmapStyles.None, System.Drawing.Color.Red, 16));
                           
                        }

                        ftr.Style = cs;
                        tblcar.InsertFeature(ftr);


                        if (tempname == JZCarName)
                        {
                            MapInfo.Geometry.DPoint dpoint = new DPoint(xv, yv);
                            mapControl1.Map.Center = dpoint;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-15-初始化添加车辆");
            }
        }

        /// <summary>
        /// 判读车辆是否在可视范围
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Boolean IsInBounds(double x, double y)
        {
            try
            {
                if (mapControl1.Map.Bounds.x1 < x && x < mapControl1.Map.Bounds.x2 && mapControl1.Map.Bounds.y1 < y && y < mapControl1.Map.Bounds.y2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-16-判断车辆是否在可视范围");
                return false;
            }
        }

        /// <summary>
        /// 车辆闪现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timLocation_Tick(object sender, EventArgs e)
        {
            try
            {
                iflash = iflash + 1;

                int i = iflash % 2;
                if (i == 0)
                {
                    GetFlash();
                }
                else
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }

                if (this.iflash % 10 == 0)
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                    timLocation.Enabled = false;

                    GetGzPoistion();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-17-车辆闪现");
            }
        }

        /// <summary>
        /// 返回跟踪车辆
        /// </summary>
        private void GetGzPoistion()
        {
            try
            {
                if (GzCarName != "")
                {
                    Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                    Table tbl = cat.GetTable("CarLayer");
                    MapInfo.Mapping.Map map = mapControl1.Map;
                    MapInfo.Data.MIConnection micon = new MIConnection();
                    micon.Open();

                    string tblname = "CarLayer";
                    string colname = "Name";

                    MapInfo.Data.MICommand micmd = micon.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    micmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + "= '" + GzCarName + "'";
                    IResultSetFeatureCollection rsfcflash3 = micmd.ExecuteFeatureCollection();

                    micon.Close();
                    micon.Dispose();
                    micmd.Cancel();
                    micmd.Dispose();

                    if (tbl != null)
                    {
                        if (rsfcflash3.Count == 1)
                        {
                            foreach (Feature fcar in rsfcflash3)
                            {
                                MapInfo.Geometry.DPoint dpoint = new DPoint(fcar.Geometry.Centroid.x, fcar.Geometry.Centroid.y);
                                mapControl1.Map.Center = dpoint;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-18-返回跟踪车辆");
            }
        }

        /// <summary>
        /// 设置车辆为闪现车辆时
        /// </summary>
        public void GetFlash()
        {
            try
            {
                string tblname = "CarLayer";
                string colname = "Name";
                string ftrname = SelectCarName;

                MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();

                if (ftrname != "")
                {
                    IResultSetFeatureCollection rsfcflash1 = null;
                    MapInfo.Mapping.Map map = mapControl1.Map;
                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }

                    MapInfo.Data.MIConnection conn = new MIConnection();
                    conn.Open();

                    MapInfo.Data.MICommand cmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + "= '" + ftrname + "'";
                    rsfcflash1 = cmd.ExecuteFeatureCollection();
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash1);
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();

                    if (rsfcflash1.Count > 0)
                    {
                        foreach (Feature f in rsfcflash1)
                        {
                            mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                            mapControl1.Map.Center = f.Geometry.Centroid;
                        }
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "select * from " + mapControl1.Map.Layers["VideoCarLayer"].ToString() + " where " + colname + "=@name";
                        cmd.Parameters.Add("@name", ftrname);
                        rsfcflash1 = cmd.ExecuteFeatureCollection();
                        if (rsfcflash1.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash1);
                            foreach (Feature f in rsfcflash1)
                            {
                                mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                break;
                            }
                        }
                    }
                    cmd.Cancel();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-19-设置车辆为闪现车辆");
            }
        }

        // 获取缩放比例
        private double getScale()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("比例尺", "缩放比例"));
                return dou;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getScale");
                return 0;
            }
        }

        /// <summary>
        /// 添加轨迹点
        /// </summary>
        public void setthefirstpoint()
        {
            try
            {
                IResultSetFeatureCollection rsfcflash = null;
                rsfcflash = GetFirstPoint("CarLayer");
                if (rsfcflash.Count > 0)
                {
                    foreach (Feature fcar in rsfcflash)
                    {
                        if (fcar["Name"].ToString() == this.GzCarName)
                        {
                            xx = fcar.Geometry.Centroid.x;
                            yy = fcar.Geometry.Centroid.y;
                        }
                    }
                }
                else
                {
                    rsfcflash = GetFirstPoint("VideoCarLayer");
                    if (rsfcflash.Count > 0)
                    {
                        foreach (Feature fcar in rsfcflash)
                        {
                            if (fcar["Name"].ToString() == this.GzCarName)
                            {
                                xx = fcar.Geometry.Centroid.x;
                                yy = fcar.Geometry.Centroid.y;

                                // writelog("firstpoint " + xx.ToString()+" " + yy.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-20-添加轨迹点");
            }
        }


        private IResultSetFeatureCollection GetFirstPoint(string LayerName)
        {
            IResultSetFeatureCollection rsfcflash = null;
            try
            {
                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable(LayerName);
                MapInfo.Mapping.Map map = mapControl1.Map;

                MapInfo.Data.MIConnection micon = new MIConnection();
                micon.Open();


                string tblname = LayerName;
                string colname = "Name";

                MapInfo.Data.MICommand micmd = micon.CreateCommand();
                Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                micmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + "= '" + this.GzCarName + "'";
                rsfcflash = micmd.ExecuteFeatureCollection();

                micon.Close();
                micon.Dispose();
                micmd.Cancel();
                micmd.Dispose();

                return rsfcflash;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-21-GetFirstPoint");
                return rsfcflash;
            }
        }

        public void StartFlash()
        {
            try
            {
                timLocation.Enabled = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "StartFlash");
            }
        }
 

        /// <summary>
        /// 更改模块区域权限
        /// </summary>
        public void SetUserRegion()
        {
            try
            {

                if (strRegion == "" && strRegion1 == "")// 既没有派出所权限，也没有中队权限
                {
                    return;
                }
                else if (strRegion == "" && strRegion1 != "") // 有中队权限，没有派出所权限
                {
                    strRegion = GetPolice(strRegion1);

                }
                else if (strRegion != "" && strRegion1 != "")  // 有中队权限，也有派出所权限
                {
                    strRegion = strRegion + "," + GetPolice(strRegion1);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-22-更改模块区域权限");
            }
        }

        /// <summary>
        /// 根据中队名称获取所在的派出所名称
        /// </summary>
        /// <param name="s1"></param>
        /// <returns></returns>
        private String GetPolice(string s1)
        {
            string reg = string.Empty;

            try
            {
                string[] ZdArr = s1.Split(',');
                for (int i = 0; i < ZdArr.Length; i++)
                {
                    string zdn = ZdArr[i];

                    DataTable dt = GetTable("Select 所属派出所 from 基层民警中队 where 中队名='" + zdn + "'");

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (i != ZdArr.Length - 1)
                            {
                                reg = reg + dr["所属派出所"].ToString() + ",";
                            }
                            else
                            {
                                reg = reg + dr["所属派出所"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-23-根据中队名称获取所在的派出所名称");
            }
            return reg;
        }

        /// <summary>
        /// 初始化实时监控
        /// </summary>        
        public void StartTimeCar()
        {
            try
            {
                StopTimeCar();

                this.GetCarflag = true;

                mapControl1.Tools.LeftButtonTool = "Select";

                ToolCarEnable();
                CreateCarLayer();
                CreateVideoCarLayer();

                CreateTrackLayer();

                AddGz();

                CanNm = "All";
                AddGrid();

                this.SetLayerSelect("CarLayer", "VideoCarLayer");

                timeCar.Interval = 30000;
                timeCar.Enabled = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-24-初始化实时监控");
            }
        }

        /// <summary>
        /// 删除轨迹
        /// </summary>
        public void ClearTrack()
        {
            try
            {
                if (mapControl1.Map.Layers["CarTrack"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarTrack");
                }

                CreateTrackLayer();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-25-删除轨迹");
            }
        }

        /// <summary>
        /// 停止车辆监控
        /// </summary>
        public void StopTimeCar()
        {
            try
            {
                ToolCarDisable();

                if (this.timeCar.Enabled == true)
                {
                    timeCar.Enabled = false;
                }             

                GetCarflag = false;

                //fhistory.Dispose(); //Close();

                if (fhistory.timer1.Enabled == true)
                {
                    fhistory.timer1.Enabled = false;
                }

                if (mapControl1.Map.Layers["CarTrack"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarTrack");
                }

                if (mapControl1.Map.Layers["CarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                }

                if (mapControl1.Map.Layers["CarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarLabel");
                }

                if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoCarLayer");
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }

                if (mapControl1.Map.Layers["CarGuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarGuijiLayer");
                }

                if (mapControl1.Map.Layers["CarGzLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarGzLayer");
                }

                this.GzCarName = "";
                this.JZCarName = "";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-26-停止车辆监控");
            }
        }


        /// <summary>
        /// 清楚车辆监控的临时图层
        /// </summary>
        public void ClearCarTemp()
        {
            try
            {
                clearFeatures("CarTrack");
                clearFeatures("CarGuijiLayer");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-27-停止车辆监控");
            }
        }

        private void clearFeatures(string tabAlias)
        {
            try
            {
                //清除地图上添加的对象
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers[tabAlias];
                if (fl == null)
                {
                    return;
                }
                Table tableTem = fl.Table;

                //先清除已有对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-28-clearFeatures");
            }
        }

        /// <summary>
        /// 创建轨迹线
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>      
        public void Trackline(double x1, double y1, double x2, double y2,Color color)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["CarTrack"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("CarTrack");

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(3, MapInfo.Styles.LineWidthUnit.Pixel), 2, color);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-29-创建轨迹线");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SearchCar();
        }

        /// <summary>
        /// 查询车辆
        /// </summary>
        private void SearchCar()
        {
            try
            {
                if (this.CaseSearchBox.Text == "")
                {                     //终端ID,车辆牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间
                    CanNm = "All";
                }
                else
                {
                    CanNm = this.comboBox1.Text + " like  '%" + this.CaseSearchBox.Text + "%' ";
                }
                AddGrid();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-30-查询车辆");
            }
        }

        //设置当前图层可选择
        private void SetLayerSelect(string layername1, string layername2)
        {
            try
            {
                MapInfo.Mapping.Map map = mapControl1.Map;

                for (int i = 0; i < map.Layers.Count; i++)
                {
                    IMapLayer layer = map.Layers[i];
                    //string lyrname = layer.Alias;

                    MapInfo.Mapping.LayerHelper.SetSelectable(layer, false);
                }

                if (mapControl1.Map.Layers[layername1] != null)
                    MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername1], true);
                if (mapControl1.Map.Layers[layername2] != null)

                    MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername2], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-31-设置图层可选");
            }
        }


        /// <summary>
        /// 车辆周边查询工具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    switch (e.ToolName)
                    {
                        case "SelectPolygon":

                            switch (e.ToolStatus)
                            {
                                case ToolStatus.End:
                                    if (this.GetCarflag)
                                    {
                                        MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                                        FeatureLayer lyr = mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                                        this.rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                        if (this.rsfcView != null)
                                        {
                                            string SearchName = "";
                                            int i = 1;
                                            foreach (Feature f in this.rsfcView)
                                            {
                                                foreach (MapInfo.Data.Column col in f.Columns)
                                                {
                                                    if (col.ToString() == "NAME")
                                                    {
                                                        string ftename = f["NAME"].ToString();
                                                        if (i == this.rsfcView.Count || this.rsfcView.Count == 1)
                                                        {
                                                            SearchName = SearchName + "终端车辆号牌 = '" + ftename + "'";
                                                        }
                                                        else
                                                        {
                                                            SearchName = SearchName + "终端车辆号牌 = '" + ftename + "' or ";
                                                        }
                                                        i = i + 1;
                                                    }
                                                }
                                            }


                                            this.CanNm = SearchName; //20081008
                                            this.AddGrid();

                                            this.SetViewFlag = true;
                                            if (this.rsfcView.Count > 0)
                                            {
                                                if (this.rsfcView.Count == 1)
                                                {
                                                    foreach (Feature f in this.rsfcView)
                                                    {
                                                        mapControl1.Map.Center = f.Geometry.Centroid;
                                                    }
                                                }
                                                else
                                                {
                                                    mapControl1.Map.SetView(this.rsfcView.Envelope);
                                                }
                                            }
                                            WriteEditLog("", "多边形选择");
                                        }

                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Select":
                            switch (e.ToolStatus)
                            {
                                case ToolStatus.End:
                                    if (GetCarflag == true)
                                    {
                                        try
                                        {
                                            MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                                            FeatureLayer lyr = mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                                            IResultSetFeatureCollection rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                            try
                                            {
                                                if (rsfcView != null)
                                                {
                                                    if (rsfcView.Count > 0)
                                                    {
                                                        foreach (Feature f in rsfcView)
                                                        {
                                                            string ftename = f["Name"].ToString();

                                                            if (this.dataGridView1.Rows.Count > 0)
                                                            {
                                                                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                                                {
                                                                    if (
                                                                        dataGridView1.Rows[i].Cells[0].Value.ToString() ==
                                                                        ftename)
                                                                    {
                                                                        dataGridView1.CurrentCell =
                                                                            dataGridView1.Rows[i].Cells[0];
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    lyr = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;

                                                    rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                                    if (rsfcView.Count > 0)
                                                    {
                                                        foreach (Feature f in rsfcView)
                                                        {
                                                            string ftename = f["Name"].ToString();

                                                            if (this.dataGridView1.Rows.Count > 0)
                                                            {
                                                                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                                                {
                                                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == ftename)
                                                                    {
                                                                        dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                lyr = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;

                                                rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                                if (rsfcView.Count > 0)
                                                {
                                                    foreach (Feature f in rsfcView)
                                                    {
                                                        string ftename = f["Name"].ToString();

                                                        if (this.dataGridView1.Rows.Count > 0)
                                                        {
                                                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                                            {
                                                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == ftename)
                                                                {
                                                                    dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            WriteEditLog("", "单击选择");
                                        }
                                        catch (Exception ex)
                                        {
                                            writeToLog(ex, "获取DG中的数据");
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-32-车辆周边查询工具");
            }
        }

        /// <summary>
        /// 周边查询车辆
        /// </summary>
        /// <param name="dpt"></param>
        /// <param name="distance"></param>
        public void SearchCarDistance(MapInfo.Geometry.DPoint dpt, Double distance)
        {
            //判断临时TempCar是否存在，存在则关闭并重新建立
            try
            {
                if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoCarLayer");
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoCarLayer");
                Table tblTemp = Cat.GetTable("VideoCarLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("VideoCarLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("设备编号", 100));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);

                //添加标注
                const string activeMapLabel = "VideoCarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);


                //在CarLayer中选择周边的车辆，并添加到tempcar
                double x1, x2;
                double y1, y2;
                double x, y;

                double dbufferdis = distance / 111000;

                x = dpt.x;
                y = dpt.y;
                x1 = x - dbufferdis;
                x2 = x + dbufferdis;
                y1 = y - dbufferdis;
                y2 = y + dbufferdis;


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");

                if (strRegion == string.Empty)
                {
                    MessageBox.Show("没有设置区域权限", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string sql = string.Empty;

                if (strRegion == "顺德区")
                {
                    sql = "Select * from GPS警车定位系统 where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " order by CAMID desc ";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    sql = "Select * from GPS警车定位系统 where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and 权限单位 in('" + strRegion.Replace(",", "','") + "') order by CAMID desc";
                }

                DataTable dt = GetTable(sql);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string CAMID = Convert.ToString(dr["CAMID"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();

                        if (CAMID == "")
                        {
                            cs.ApplyStyle(new BitmapPointStyle("jCar.BMP", BitmapStyles.None, System.Drawing.Color.Red, 16));
                        }
                        else
                        {
                            cs.ApplyStyle(new BitmapPointStyle("ydsp.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));
                        }

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = dr["终端车辆号牌"].ToString();
                        ftr["设备编号"] = CAMID;
                        tblcar.InsertFeature(ftr);
                    }
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-33-周边查询车辆");
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        SearchCar();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textBox1_KeyDown");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.RefreshGrid();
        }

        /// <summary>
        /// 数据表双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

            try
            {
                DPoint dp = new DPoint();

                string sqlFields = "终端ID号码,终端车辆号牌,所属单位,当前速度,当前方向,X,Y,导航状态,GPS时间 ";
                string strSQL = "select " + sqlFields + " from GPS警车定位系统 t where 终端车辆号牌='" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'";

                DataTable datatable = GetTable(strSQL);

                System.Drawing.Point pt = new System.Drawing.Point();
                if (datatable.Rows.Count > 0)
                {
                    try
                    {
                        dp.x = Convert.ToDouble(datatable.Rows[0]["X"]);
                        dp.y = Convert.ToDouble(datatable.Rows[0]["Y"]);
                    }
                    catch
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;
                        return;
                    }
                    if (dp.x == 0 || dp.y == 0)
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;
                        return;
                    }
                    mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                    WriteEditLog("终端车辆号牌='" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-34-数据表双击");
            }
        }

        /// <summary>
        /// 显示车辆信息
        /// </summary>
        private nsInfo.FrmInfo frmMessage = new nsInfo.FrmInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (this.frmMessage.Visible == false)
                {
                    this.frmMessage = new nsInfo.FrmInfo();
                    frmMessage.SetDesktopLocation(-30, -30);
                    frmMessage.Show();
                    frmMessage.Visible = false;
                }
                frmMessage.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " ucCar-35-显示车辆信息");
            }
        }

        /// <summary>
        /// 车辆工具Enable
        /// </summary>
        public void ToolCarEnable()
        {
            try
            {
                toolDDbtn.Visible = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-36-车辆工具Enable");
            }
        }


        /// <summary>
        /// 车辆菜单Disable
        /// </summary>
        public void ToolCarDisable()
        {
            try
            {
                toolDDbtn.Visible = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-37-车辆菜单Disable");
            }
        }

        private string[] GzArrayName = new string[10] { "","","","","","","","","",""};  //跟踪车辆的名称数组
        private Color[] GzArrayColor = new Color[10] { Color.SandyBrown, Color.Green, Color.Yellow, Color.DarkCyan, Color.Firebrick, Color.Fuchsia, Color.Gainsboro, Color.Gold, Color.Honeydew, Color.Khaki }; //跟踪车辆的颜色数组
        private double[] GzArrayLx = new double[10] {0, 0,0,0,0,0,0,0,0,0};    //跟踪车辆的上个点的经度数组 经度
        private double[] GzArrayLy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪车辆的上个点的纬度数组 纬度

        private double[] GzArrayNx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪车辆的上个点的经度数组 经度
        private double[] GzArrayNy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪车辆的上个点的纬度数组 纬度


        private void RefreshGzArray()
        {
            try
            {
                GzArrayLx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪车辆的上个点的经度数组 经度
                GzArrayLy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪车辆的上个点的纬度数组 纬度

                GzArrayNx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪车辆的上个点的经度数组 经度
                GzArrayNy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪车辆的上个点的纬度数组 纬度

                for (int i = 0; i < this.GzArrayName.Length; i++)
                {
                    if (GzArrayName[i] != "")
                    {
                        DataTable dt = GetTable("Select * from gps警车定位系统 where 终端车辆号牌 like '%" + this.GzArrayName[i] + "%'");
                        if (dt.Rows.Count > 0)
                        {
                            GzArrayLx[i] = Convert.ToDouble(dt.Rows[0]["X"]);
                            GzArrayLy[i] = Convert.ToDouble(dt.Rows[0]["Y"]);
                        }
                    }
                }

                AddGrid();

                AddGz();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "RefreshGzArray");
            }
        }


        private void AddGz()
        {
            try
            { 
                /////获取最新的数据
                if (GzArrayName.Length == 0) return;

                for (int i = 0; i < this.GzArrayName.Length; i++)
                {
                    if (GzArrayName[i] != "")
                    {
                        DataTable dt = GetTable("Select * from gps警车定位系统 where 终端车辆号牌 like '%" + this.GzArrayName[i] + "%'");
                        if (dt.Rows.Count > 0)
                        {
                            GzArrayNx[i] = Convert.ToDouble(dt.Rows[0]["X"]);
                            GzArrayNy[i] = Convert.ToDouble(dt.Rows[0]["Y"]);
                        }
                    }
                }

                //创建临时层
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;                
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarGZLayer");
                Table tblTemp = Cat.GetTable("CarGZLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarGZLayer");
                }
                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Insert(0,lyr);


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["CarGZLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarGzLayer");

                //加载数据
                for (int i = 0; i < GzArrayName.Length; i++)
                {
                    string tempname = GzArrayName[i];

                    if (tempname != "")
                    {

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(GzArrayNx[i], GzArrayNy[i])) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("jCar.bmp", BitmapStyles.None, System.Drawing.Color.Red, 26));

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr["Name"] = tempname;

                        MapInfo.Geometry.DPoint dpoint = new DPoint(GzArrayNx[i], GzArrayNy[i]);
                      
                        ftr.Style = cs;
                        tblcar.InsertFeature(ftr);

                        Trackline(GzArrayLx[i], GzArrayLy[i], GzArrayNx[i], GzArrayNy[i], GzArrayColor[i]);

                        GzArrayLx[i] = GzArrayNx[i];
                        GzArrayLy[i] = GzArrayNy[i];
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-15-初始化添加车辆");
            }
        }

        /// <summary>
        /// 选择工具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToolSelect(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                string Selectext = e.ClickedItem.Text;

                switch (Selectext)
                {
                    case "单击选择":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        break;
                    case "范围查询":
                        mapControl1.Tools.LeftButtonTool = "SelectPolygon";
                        break;
                    case "跟踪车辆":
                        toolDDbtn.DropDownItems[2].Text = "取消跟踪";
                        GzCarName = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        setthefirstpoint();
                        WriteEditLog(GzCarName, "跟踪车辆");
                        break;
                    case "取消跟踪":
                        toolDDbtn.DropDownItems[2].Text = "跟踪车辆";
                        GzCarName = "";
                        xx = 0;
                        yy = 0;
                        ClearTrack();
                        WriteEditLog(GzCarName, "取消跟踪");
                        break;
                    case "车辆居中":
                        JZCarName = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        toolDDbtn.DropDownItems[3].Text = "取消居中";
                        WriteEditLog(JZCarName, "车辆居中");
                        break;
                    case "取消居中":
                        JZCarName = "";
                        toolDDbtn.DropDownItems[3].Text = "车辆居中";
                        WriteEditLog(JZCarName, "取消居中");
                        break;
                    case "轨迹回放":
                        fhistory.Visible = true;
                        fhistory.user = user;
                        fhistory.comboBox1.Text = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        break;
                    case "跟踪目标":
                        DataTable dt = GetTable("Select 终端车辆号牌 from GPS警车定位系统 where 备用字段一 is null or 备用字段一='' order by 终端车辆号牌" );
                        if (dt.Rows.Count > 0)
                        {
                            frmGz frmgz = new frmGz(dt, GzArrayName, StrCon);
                            if (frmgz.ShowDialog(this) == DialogResult.OK)
                            {
                                this.GzArrayName = frmgz.ArrayName;

                                RefreshGzArray();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-38-选择工具");
            }
        }

       

        /// <summary>
        /// 更新数据表
        /// </summary>
        public void RefreshGrid()
        {
            try
            {
                isShowPro(true);
                if (CanNm != "")
                {
                    string sql = string.Empty;

                    if (strRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show("没有设置区域权限", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (strRegion == "顺德区")
                    {
                        if (CanNm == "All")
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统";
                        }
                        else
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统 where " + CanNm;
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(strRegion.Split(','), "大良") > -1 && strRegion.IndexOf("德胜") < 0)
                        {
                            strRegion = strRegion.Replace("大良", "大良,德胜");
                        }
                        if (CanNm == "All")
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统 where 权限单位 in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            sql = "select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from GPS警车定位系统 where " + CanNm + " and 权限单位 in('" + strRegion.Replace(",", "','") + "')";
                        }
                    }

                    // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                    if (sql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                        sql += " and (备用字段一 is null or 备用字段一='')";
                    else
                        sql += " where (备用字段一 is null or 备用字段一='')";
                    //-------------------------------------------------------

                    string Gzstring1 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring1 = Gzstring1 + " 终端车辆号牌='" + GzArrayName[i] + "' or ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring1 = Gzstring1.Substring(0,Gzstring1.LastIndexOf("or")-1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring1 = Gzstring1 + " 终端车辆号牌='" + GzArrayName[i] + "'";
                        }
                    }


                    string Gzstring2 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring2 = Gzstring2 + " 终端车辆号牌<>'" + GzArrayName[i] + "' and ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring2 = Gzstring2.Substring(0, Gzstring2.LastIndexOf("and") - 1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring2 = Gzstring2 + " 终端车辆号牌<>'" + GzArrayName[i] + "'";
                        }
                    }

                    string tsql = string.Empty ;

                    if (Gzstring1 != "" && Gzstring2 != "")
                        tsql = sql +" and " +Gzstring1 + " Union all " + sql +" and "+ Gzstring2;
                    else
                        tsql = sql;


                    if (Gzstring1 != "" && Gzstring2 != "" && ordername != "")
                        tsql = sql + " and " + Gzstring1 + " Union all select 终端车辆号牌,所属单位,当前速度,当前方向,GPS时间,导航状态,X,Y from (" + sql + " and " + Gzstring2 + " order by " + ordername + " desc)";
                    if (Gzstring1 == "" && Gzstring2 == "" && ordername != "")
                        tsql = sql + " order by " + ordername + " desc";

                    if (tsql == "") return;

                    DataTable dt = GetTable(tsql);
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    if (dt.Rows.Count > 0)
                    {
                        this.dataGridView1.DataSource = dt;

                        foreach (DataRow dr in dt.Rows)
                        {
                            //string carname = Convert.ToString(dr["终端车辆号牌"]);
                            if (dataGridView1.Rows.Count > 0)
                            {
                                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                {
                                    //if (dataGridView1.Rows[i].Cells[0].Value.ToString() == carname)
                                    //{
                                    //    dataGridView1.Rows[i].Cells[1].Value = dr["所属单位"].ToString();
                                    //    dataGridView1.Rows[i].Cells[2].Value = dr["当前速度"].ToString();
                                    //    dataGridView1.Rows[i].Cells[3].Value = dr["当前方向"].ToString();
                                    //    dataGridView1.Rows[i].Cells[4].Value = dr["GPS时间"].ToString();
                                    //    dataGridView1.Rows[i].Cells[5].Value = dr["导航状态"].ToString();
                                    //    dataGridView1.Rows[i].Cells[6].Value = dr["X"].ToString();
                                    //    dataGridView1.Rows[i].Cells[7].Value = dr["Y"].ToString();
                                    //}
                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == this.SelectCarName)
                                    {
                                        //dataGridView1.Rows[i].Selected = true;
                                        dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                                    }
                                }
                            }
                        }
                    }
                    this.toolPro.Value = 2;
                    Application.DoEvents();

                    #region 数据导出
                    excelSql = sql;
                    excelSql = "select * " + excelSql.Substring(excelSql.IndexOf("from"));
                    string sRegion2 = strRegion2;
                    string sRegion3 = strRegion3;

                    if (strRegion2 != "顺德区")
                    {
                        if (strRegion2 != "")
                        {
                            if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                            {
                                sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                            }
                            excelSql += " and (权限单位 in ('" + sRegion2.Replace(",", "','") + "'))";
                        }
                        else if (strRegion2 == "")
                        {
                            if (excelSql.IndexOf("where") < 0)
                            {
                                excelSql += " where 1=2 ";
                            }
                            else
                            {
                                excelSql += " and 1=2 ";
                            }
                        }
                    }

                    // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                    if (excelSql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                        excelSql += " and (备用字段一 is null or 备用字段一='')";
                    else
                        excelSql += " where (备用字段一 is null or 备用字段一='')";
                    //-------------------------------------------------------

                    _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                    _endNo = pageSize1;

                    //OracleConnection orc = new OracleConnection(sql);
                    //try
                    //{
                    //    orc.Open();
                    //    OracleCommand cmd = new OracleCommand(excelSql, orc);
                    //    apt1 = new OracleDataAdapter(cmd);
                    //    DataTable datatableExcel = new DataTable();
                    //    apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //    if (dtExcel != null) dtExcel.Clear();
                    //    dtExcel = datatableExcel;
                    //    cmd.Dispose();
                    //}
                    //catch
                    //{
                    //    isShowPro(false);
                    //}
                    //finally
                    //{
                    //    orc.Close();
                    //}
                    #endregion

                    WriteEditLog(sql, "查询");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "ucCar-39-更新数据表时发生错误");
            }
        }

        /// <summary>
        /// 车辆闪烁
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                SelectCarName = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();

                this.toolgz.Text = "跟踪";

                for (int i = 0; i < this.GzArrayName.Length; i++)
                {
                    if (this.GzArrayName[i] == this.SelectCarName)
                    {
                        this.toolgz.Text = "取消跟踪";
                        break;
                    }
                }


                this.timLocation.Enabled = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-40-车辆闪烁");
            }
        }


        private void ucCar_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    StopTimeCar();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar_VisibleChanged");
            }
        }

        /// <summary>
        /// 查询SQL
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 获取Scalar
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clCar-ucCar-" + sFunc);
        }

        //记录操作记录
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'车辆监控','GPS警车定位系统:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "WriteEditLog");
            }
        }

        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {

                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();

                if (keyword.Length < 1 || colfield.Length < 1) return;

                if (colfield == "终端车辆号牌")
                {
                    string strExp = "select distinct(" + colfield + ") from gps警车定位系统 t  where " + colfield + " like '%" + keyword + "%' order by " + colfield;
                    DataTable dt = GetTable(strExp);
                    CaseSearchBox.GetSpellBoxSource(dt);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-41-数据匹配");
            }
        }

        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {

                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();

                if (colfield.Length < 1) return;

                if (colfield == "所属单位")
                {
                    string strExp = "select distinct(派出所名) from 基层派出所 order by 派出所名";
                    DataTable dt = GetTable(strExp);
                    CaseSearchBox.GetSpellBoxSource(dt);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-42-下拉列表");
            }
        }

        private void CaseSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        SearchCar();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-42-下拉列表");
            }
        }

        // 显示或隐藏进度条
        private void isShowPro(bool falg)
        {
            try
            {
                this.toolPro.Value = 0;
                this.toolPro.Maximum = 3;
                this.toolProLbl.Visible = falg;
                this.toolProSep.Visible = falg;
                this.toolPro.Visible = falg;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "isShowPro");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.button2.Text == "开始")
                {
                    this.timer1.Interval = 3 * 1000;
                    this.timer1.Enabled = true;
                    this.button2.Text = "结束";
                }
                else if (this.button2.Text == "结束")
                {
                    this.timer1.Enabled = false;
                    this.button2.Text = "开始";
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button2_Click");
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            try
            {
                string sql = "select 终端车辆号牌,X,Y from gps警车定位系统 ";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string num = Convert.ToString(dr["终端车辆号牌"]);
                        double x = Convert.ToDouble(dr["X"]);
                        double y = Convert.ToDouble(dr["Y"]);

                        if (x > mapControl1.Map.Center.x)
                            x = x - 0.1;
                        else
                            x = x + 0.1;

                        if (y > mapControl1.Map.Center.y)
                            y = y - 0.1;
                        else
                            y = y + 0.1;

                        string sqltem = "update gps警车定位系统 set x = " + x.ToString() + ",y=" + y.ToString() + " where 终端车辆号牌='" + num + "'";

                        Console.WriteLine(num + ":经度 " + x.ToString() + " 纬度:" + y.ToString());
                        this.RunCommand(sqltem);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "timer1_Tick_1");
            }
        }


        //  定时更新
        private void timeCar_Tick(object sender, EventArgs e)
        {
            try
            {
                CreateCarLayer();

                CreateVideoCarLayer();

                AddGz();

                RefreshGrid();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "timeCar_Tick");
            }
        }

        private void toolgz_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.SelectCarName != "")
                {

                    System.Windows.Forms.ListBox lst = new System.Windows.Forms.ListBox();
                    lst.Items.Clear();
                    if (GzArrayName.Length > 0)
                    {
                        for (int i = 0; i < GzArrayName.Length; i++)
                        {
                            if (GzArrayName[i] != "")
                                lst.Items.Add(GzArrayName[i]);
                        }
                    }

                    if (this.toolgz.Text == "跟踪")
                    {
                        if (lst.Items.Count > 9)
                        {
                            MessageBox.Show("监控目标不能超过10个", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        for (int i = 0; i < lst.Items.Count; i++)
                        {
                            if (lst.Items[i].ToString() == SelectCarName)
                            {
                                MessageBox.Show("该警车已经被设置为监控目标", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }

                        lst.Items.Add(this.SelectCarName);
                    }
                    else if (this.toolgz.Text == "取消跟踪")
                    {
                        for (int i = 0; i < lst.Items.Count; i++)
                        {
                            if (lst.Items[i].ToString() == this.SelectCarName)
                            {
                                lst.Items.RemoveAt(i);
                            }
                        }
                    }

                    GzArrayName = new string[10] { "", "", "", "", "", "", "", "", "", "" };

                    for (int i = 0; i < lst.Items.Count; i++)
                    {
                        GzArrayName[i] = lst.Items[i].ToString();
                    }

                    RefreshGzArray();
                }
            }
            catch (Exception ex)
            { writeToLog(ex, "toolgz_Click"); }
        }

        string ordername = string.Empty;

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                DataGridViewColumn Column = dataGridView1.Columns[e.ColumnIndex];
                Console.WriteLine(Column.HeaderText);

                ordername = Column.HeaderText;

                RefreshGrid();

                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[ordername];
            }
            catch (Exception ex)
            { writeToLog(ex, "dataGridView1_ColumnHeaderMouseClick"); }
        }
    }
}