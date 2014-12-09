////////////////////////////////////////////////////////////
///==============顺德公安治安卡口程序====================///
///////////////////////////////////////////////////////////
//创建日期：20090708
//创建人:jie.zhang
//www.lbschina.com.cn

//修改记录
//20090709 jie.zhang 添加治安卡口的初始化程序，包括参数的传输和设定。 标识 20090709
//20090803 jie.zhang 添加导出excel，模糊查询
//20090804 jie.zhang 添加增加临时列=顺序号， 添加分页功能。
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Dialogs;
using MapInfo.Windows.Controls;

using EpoliceOracleDal;


namespace clKaKou
{
    public partial class ucKakou : UserControl
    {
        //Boolean UseDll = true; 

        EHL.ATMS.TGS.Interface.SD.TGSManage ehltgs = new EHL.ATMS.TGS.Interface.SD.TGSManage();  //易华录接口  
        GDW_GIS_Interface.communication gdwcom = new GDW_GIS_Interface.communication(); //高德威接口
       
        //为综合查询提供图片服务器地址
        public string ehlserver = string.Empty; 
        public string gdwserver = string.Empty;
        public string bkserver = string.Empty;
        public string photoserver = string.Empty;

        public MapControl mapControl1 = null;
        private string mysqlstr;                       //数据库连接字符串
        private string[] StrCon;                      //数据库连接字符串组
        public string strRegion = string.Empty;  //派出所权限
        public string strRegion1 = "";              //中队权限
        public string user = "";
        private string getfrompath = string.Empty;//GetFromName的配置文件位置

        public System.Windows.Forms.Panel panError;       // 用于弹出错误提示
        public System.Data.DataTable dtExcel = null; //地图页面数据导出按钮

        frmtip ftip = new frmtip();

        private int VideoPort = 0;           //通讯端口
        private string[] VideoString;     // 视频连接字符
        private ToolStripLabel st = null;
        private static NetworkStream ns = null;  //

        private Boolean vf = false; // 通讯是否已经连接的标识
        private string VEpath = string.Empty;

        private string ALARMSYS = string.Empty;  // 报警面对的模块
        private string ALARMUSER = string.Empty; //布控单位
        double SCHDIS = 0;    //查询半径

        public ToolStripProgressBar toolPro;  // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolProLbl;     // 用于显示进度文本　
        public ToolStripSeparator toolProSep; 

        private ToolStripDropDownButton tddb;


        #region 输入法
        //声明一些API函数
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetOpenStatus(IntPtr himc);
        [DllImport("imm32.dll")]
        public static extern bool ImmSetOpenStatus(IntPtr himc, bool b);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetConversionStatus(IntPtr himc, ref int lpdw, ref int lpdw2);
        [DllImport("imm32.dll")]
        public static extern int ImmSimulateHotKey(IntPtr hwnd, int lngHotkey);
        private const int IME_CMODE_FULLSHAPE = 0x8;
        private const int IME_CHOTKEY_SHAPE_TOGGLE = 0x11;

        #endregion


        /// <summary>
        /// 获取卡口参数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-27
        /// </summary>
        /// <param name="m">地图控件</param>
        /// <param name="t"></param>
        /// <param name="s">数据库连接参数</param>
        /// <param name="zh">直观指挥的标示</param>
        public ucKakou(MapControl mc, string[] sqlcon, ToolStripLabel tools, ToolStripDropDownButton dt, int port, string[] vs, string videopath, string Alarmsys, string AlarmUser, double SchDis, string streg, string usern,string getfnpath,bool isEvent)
        {
            InitializeComponent();
            try
            {

                Writelog("引用宝康,using EpoliceOracleDal;");

                mapControl1 = mc;

                StrCon = sqlcon;
                mysqlstr = "data source=" + StrCon[0] + ";user id=" + StrCon[1] + ";password=" + StrCon[2];
                this.tddb = dt;

                getfrompath = getfnpath;

                if (isEvent)
                {
                    mapControl1.Tools.Used -= new ToolUsedEventHandler(Tools_Used);
                    mapControl1.Tools.FeatureAdded -= new FeatureAddedEventHandler(Tools_FeatureAdded);
                    mapControl1.Tools.FeatureSelected -= new FeatureSelectedEventHandler(Tools_FeatureSelected);

                    mapControl1.Tools.Used += new ToolUsedEventHandler(Tools_Used);
                    mapControl1.Tools.FeatureAdded += new FeatureAddedEventHandler(Tools_FeatureAdded);
                    mapControl1.Tools.FeatureSelected += new FeatureSelectedEventHandler(Tools_FeatureSelected);
                }

                this.bindingNavigator1.Visible = true;
                this.bindingNavigator2.Visible = true;

                SetDrawStyle();

                //报警信息
                this.ALARMSYS = Alarmsys;
                this.ALARMUSER = AlarmUser;
                this.SCHDIS = SchDis;

                VideoPort = port;
                VideoString = vs;
                this.st = tools;
                this.VEpath = videopath;

                this.strRegion = streg;
                this.user = usern;

                ftip.GetPara(StrCon , mapControl1, ALARMSYS, ALARMUSER, strRegion, user);
                ftip.Visible = false;
                //ftip.timalarm.Enabled = true;

                InitAlarmSet();//初始化报警设置

                this.comboxTable.Items.Clear();
                this.comboxTable.Items.Add("车辆通过信息");
                this.comboxTable.Items.Add("车辆报警信息");
                this.comboxTable.Items.Add("治安卡口信息");
                this.comboxTable.Text = this.comboxTable.Items[0].ToString();

                this.label8.Text = "1、请以“半角”字符输入查询条件；\n\r"+
                                   "2、模糊查询时必须勾选“模糊查询”项；\n\r" +
                                   "3、支持关键字模糊查询，例如：要查询\n\r   粤X12345车牌，可输入“1234”或\n\r   “粤X123”等进行模糊查询；\n\r" +
                                   "4、可使用通配字符“？”进行模糊查询，\n\r   每一个“？”代表一个字符，可同时使\n\r   " +
                                      "用多个“？”，例如：要查询粤X12345\n\r   车牌，可输入“粤X？2345”、“粤X12\n\r   34？”或“？？12345”等进行模糊查询。\n\r" +
                                   "5、由于数据量庞大，模糊查询速度相对较\n\r   慢，请耐心等候。";

            }
            catch (Exception ex)
            {
                writeToLog(ex, "01-获取卡口参数");
            }
        }            
    

        /// <summary>
        /// 初始化报警设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void InitAlarmSet()
        {
            try
            {
                if (this.ALARMSYS == "系统")
                {
                    this.rdboxsys.Checked = true;
                    this.rdboxmod.Checked = false;
                }
                else if (this.ALARMSYS == "模块")
                {
                    this.rdboxmod.Checked = true;
                    this.rdboxsys.Checked = false;
                }

                if (this.ALARMUSER == "所有")
                {
                    this.rdboxall.Checked = true;
                    this.rdboxuser.Checked = false;
                }
                else if (this.ALARMUSER == "用户")
                {
                    this.rdboxall.Checked = false;
                    this.rdboxuser.Checked = true;
                }

                this.txtdist.Text = this.SCHDIS.ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InitAlarmSet-02-初始化报警设置");
            }
        }

        /// <summary>
        /// 传递视频查看参数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ns1"></param>
        /// <param name="vf1"></param>
        public void getNetParameter(NetworkStream ns1, Boolean vf1)
        {
            try
            {
                ns = ns1;
                this.vf = vf1;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getNetParameter-03-传递视频查看参数");
            }
        }               
        
        /// <summary>
        /// 初始化治安卡口
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void InitKK()
        {
            try
            {
                ClearKaKou();

                CreateKKLyr();//创建治安卡口图层
                
                if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                {
                    strRegion = strRegion.Replace("大良", "大良,德胜");
                }            
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InitKK-04-初始化治安卡口");
            }
            //AddGrid();  //添加数据到Grid===修改单中未标明要在GRID中显示
        }

        /// <summary>
        /// 设置当前图层可编辑
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="layername">图层名称</param>
        private void SetLayerEdit(string layername)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layername], true);
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layername], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetLayerEdit-05-初始化治安卡口");
            }
        }

        /// <summary>
        /// 创建治安卡口图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void CreateKKLyr()
        {
            //writelog("创建车辆图层开始" + System.DateTime.Now.ToString());
            try
            {

                if (mapControl1.Map.Layers["KKLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("KKLayer");
                }

                if (mapControl1.Map.Layers["KKLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("KKLayer");
                Table tblTemp = Cat.GetTable("KKLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("KKLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 50));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表_ID",50));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表名", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);

                //添加标注
                string activeMapLabel = "KKLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                AddKKFtr();

                this.SetLayerEdit("KKLayer");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateKKLyr-06-创建治安卡口图层");
            }
        }

        /// <summary>
        /// 添加治安卡口
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void AddKKFtr() 
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;

                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["KKLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("KKLayer");

                string sqlcmd = "Select * from 治安卡口系统";

                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["卡口名称"].ToString();
                        string camid = dr["卡口编号"].ToString();

                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();
                        cs.ApplyStyle(new BitmapPointStyle("zakk.bmp", BitmapStyles.None, System.Drawing.Color.Red, 24));

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = tempname;
                        ftr["ID"] = camid;
                        ftr["表名"] = "治安卡口";
                        ftr["表_ID"] = camid;
                        tblcar.InsertFeature(ftr);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "AddKKFtr-07-添加治安卡口");
            }        
        }

        /// <summary>
        /// 右键卡口图标将该卡口方向作业条件查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="rfc">地图要素</param>
        public void AddBayoneDireCond(IResultSetFeatureCollection rfc)
        {
            try
            {
                if (rfc == null) {
                    MessageBox.Show("请您选择卡口图标后重试！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                string carName = "";   // 卡口名称
                string carBayo = "";   // 卡口方向
                frmBayonetDire frmBay = new frmBayonetDire();
                frmBay.comBanyoneDire.Items.Clear();

                if (rfc != null)
                {
                    if (rfc.Count > 0)
                    {
                        foreach (Feature f in rfc)
                        {
                            frmBay.lblBayonet.Text = carName = f["Name"].ToString();
                            break;
                        }
                    }
                }

                DataTable table = GetTable("select distinct 卡口方向 from 治安卡口系统 where 卡口名称='" + frmBay.lblBayonet.Text + "'");  
                for (int i = 0; i < table.Rows.Count; i++)     // 添加卡口方向
                {
                    frmBay.comBanyoneDire.Items.Add(table.Rows[i][0].ToString());
                }
                frmBay.comBanyoneDire.SelectedIndex = 0;

                if (frmBay.ShowDialog() == DialogResult.OK)
                {
                    carBayo = frmBay.comBanyoneDire.Text;

                    if (carName == string.Empty || carBayo == string.Empty){
                        MessageBox.Show("数据不全，不能创建条件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    this.tabControl1.SelectedTab = tabPage3;
                    this.dataGridViewValue.Rows.Clear();
                    // 添加条件
                    this.dataGridViewValue.Rows.Add(new object[] { "卡口名称 等于 '" + carName + "'", "字符串" });
                    this.dataGridViewValue.Rows.Add(new object[] { "并且 卡口方向 等于 '" + carBayo + "'", "字符串" });
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "AddBayoneDireCond");
            }
        }

        /// <summary>
        /// 绑定DataGrid
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void AddGrid()
        {
            try
            {
                DataTable dt = GetTable("select 卡口名称,卡口编号,X,Y from 治安卡口系统");
                this.lblCount.Text = "共有" + dt.Rows.Count.ToString() + "条记录";
                this.dataGridViewKakou.DataSource = dt;
                this.dataGridViewKakou.Refresh();                
            }
            catch (Exception ex)
            {
                writeToLog(ex, "AddGrid08-绑定DataGrid");
            }           
        }

        /// <summary>
        /// 清除卡口图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void ClearKaKou()
        {
            try
            {
                if (mapControl1.Map.Layers["KKLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("KKLayer");
                }

                if (mapControl1.Map.Layers["KKLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKLabel");
                }               

                if (mapControl1.Map.Layers["KKSearchLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("KKSearchLayer");
                }

                if (mapControl1.Map.Layers["KKSearchLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKSearchLabel");
                }

                if (mapControl1.Map.Layers["KKDrawLayer"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKDrawLayer");
                }

                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
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

                clearFeatures("查看选择");
 
                this.dataGridViewKakou.DataSource =null ;
                this.dgvres.DataSource = null;
                this.txtBoxCar.Text = "";
                this.chkmh.Checked = false;
                this.dataGridViewValue.DataSource = null;
            }
            catch(Exception ex)
            {
                writeToLog(ex, "ClearKaKou-09-清除卡口图层");
            }
        }

        /// <summary>
        /// 清除卡口临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void ClearKaKouTemp() 
        {
            try
            {                
                clearFeatures("KaKouTrackLayer");
                clearFeatures("KKSearchLayer");
                clearFeatures("查看选择");
                clearFeatures("KKLayer");
                CreateKKLyr();

                this.dataGridViewKakou.DataSource = null;
                this.dgvres.DataSource = null;
                //this.txtBoxCar.Text = "";
                //this.chkmh.Checked = false;
                this.dataGridViewValue.DataSource = null;

                PageNow1.Text = "0";
                PageNow2.Text = "0";

                PageNum1.Text = "/ {0}";
                PageNum2.Text = "/ {0}";

                RecordCount1.Text = "0 条";
                this.RCount2.Text = "0 条";

                PageNumber.Text = "100";
                TextNum1.Text = "100";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ClearKaKouTemp-10-清除卡口临时图层");
            }
        }

        /// <summary>
        /// 清除图层图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="tabAlias">图层名称</param>
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
                writeToLog(ex, "clearFeatures-11-清除图层图元");
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            Writelog("启动卡口信息查询功能");
            //this.groupBox3.Visible = false;
            SearchCarPass();
        }


        /// <summary>
        /// 日志输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="msg">输出内容</param>
        private void Writelog(string msg)
        {
            try
            {
                string filepath = Application.StartupPath + "\\KKrec.log";
                msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + ":" + msg;

                StreamWriter sw = File.AppendText(filepath);
                sw.WriteLine(msg);
                sw.Flush();
                sw.Close();
            }
            catch(Exception ex)
            {
                writeToLog(ex, "Writelog");
            }
        }

        string SQLSearch = string.Empty;
        /// <summary>
        /// 查询通过车辆
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void SearchCarPass()
        {
            try
            {
                if (this.txtBoxCar.Text != "")
                {

                    string sqlend = string.Empty;
                    fuzzyFlag = false;


                    if (this.chkmh.Checked == true)
                    {
                        sqlend = "like  '%" + this.txtBoxCar.Text.Trim() + "%'";
                    }
                    else
                    {
                        sqlend = "='" + this.txtBoxCar.Text.Trim() + "'";
                        fuzzyFlag = true;
                    }

                    if (sqlend.IndexOf('?') > -1)
                    {
                        sqlend = "like  '" + this.txtBoxCar.Text.Trim().Replace('?', '%') + "'";
                    }

                    DateTime dts = Convert.ToDateTime(this.dateFrom.Text);
                    DateTime dte = Convert.ToDateTime(this.dateTo.Text);
                    DataTable dt = new DataTable();

                    if (dts >= dte)
                    {
                        MessageBox.Show("起始时间应小于终止时间,请重设!", "时间设置错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    //////////////////////////////////  xxxxxx    1
                    Application.DoEvents();
                    this.Cursor = Cursors.WaitCursor;

                    ClearKaKouTemp();

                    string sqlstr = string.Empty;

                    if (this.txtBoxCar.Text.Trim().IndexOf("无号牌") > -1)
                        sqlstr = "select * from V_Cross where 通过时间> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 通过时间< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and (车辆号牌 ='无号牌' or 车辆号牌 is null or 车辆号牌='') order by 通过时间";
                    else
                        sqlstr = "select * from V_Cross where 通过时间> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 通过时间< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 车辆号牌 " + sqlend + " order by 通过时间";

                    Writelog("查询执行sql：" + sqlstr);

                    DataTable dt2 = null;
                    DataTable dt3 = null;

                    FrmProgress frmpro = new FrmProgress();
                    frmpro.progressBar1.Maximum = 4;
                    frmpro.Show();

                    frmpro.progressBar1.Value = 0;

                    messageStr = "";  // 用于显示外部数据提供商的查询结果及异常信息

                    Application.DoEvents();
                    frmpro.label1.Text = "正在查询易华录数据.....";
                    try
                    {
                        Writelog("发送易华录数据查询请求");
                        this.st.Text = "正在查询易华录信息......";
                        dt = ehltgs.GetPassCarInfo(sqlstr, ref ehlserver);                  // 易华录查询接口
                        Writelog("获得易华录数据结果,数据量:" + dt.Rows.Count.ToString());
                        messageStr += "易华录服务器查询到" + dt.Rows.Count + "条数据\t\n";
                    }
                    catch
                    {
                        messageStr += "易华录服务器通讯出现问题,无法查询到数据\t\n";
                    }
                    Application.DoEvents();

                    this.toolPro.Value = 1;
                    frmpro.progressBar1.Value = 1;


                    frmpro.label1.Text = "正在查询高德威数据.....";
                    try
                    {
                        Writelog("发送高德威数据查询请求");
                        this.st.Text = "正在查询高德威信息......";
                        dt2 = gdwcom.QueryData(sqlstr, ref gdwserver);  //高德威查询接口
                        Writelog("获得高德威数据结果,数据量:" + dt2.Rows.Count.ToString());
                        messageStr += "高德威服务器查询到" + dt2.Rows.Count + "条数据\t\n";
                    }
                    catch 
                    {
                        messageStr += "高德威服务器通讯出现问题,无法查询到数据\t\n"; 
                    }
                    Application.DoEvents();

                    this.toolPro.Value = 2;
                    frmpro.progressBar1.Value = 2;

                    frmpro.label1.Text = "正在查询宝康数据.....";

                    try
                    {
                        Writelog("发送宝康数据查询请求");
                        this.st.Text = "正在查询宝康信息......";
                        PassDataQuery psqy = new PassDataQuery();
                        Writelog("调用宝康函数：PassDataQuery psqy = new PassDataQuery();");

                        bkserver = "";
                        Writelog("设置宝康函数：bkserver = " + bkserver);
                        dt3 = psqy.QueryPassData(sqlstr, ref bkserver);    //宝康查询接口
                        Writelog("调用宝康函数：psqy.QueryPassData(sqlstr, ref bkserver);");
                        Writelog("宝康传入参数,sqlstr:" + sqlstr + ",bkserver:" + bkserver);

                        Writelog("获得宝康数据服务器地址:" + bkserver);
                        Writelog("获得宝康数据结果,数据量:" + dt3.Rows.Count.ToString());
                        messageStr += "宝康服务器查询到" + dt3.Rows.Count + "条数据\t\n";

                    }
                    catch 
                    {
                        messageStr += "宝康服务器通讯出现问题,无法查询到数据\t\n"; 
                    }

                    Application.DoEvents();

                    this.toolPro.Value = 3;
                    frmpro.progressBar1.Value = 3;
                    this.st.Text = "卡口数据查询完成......";

                    #region 包含宝康查询
                    try
                    {
                        if (dt != null && dt2 != null && dt3 != null)   //易华录+高德威 + 宝康
                        {
                            dt2.Merge(dt3, false);
                            dt.Merge(dt2, false);
                        }
                        else if (dt == null && dt2 != null && dt3 != null)   //高德威 + 宝康
                        {
                            dt2.Merge(dt3, false);
                            dt = dt2;
                        }
                        else if (dt != null && dt2 == null && dt3 != null)   //易华录 + 宝康
                        {
                            dt.Merge(dt3, false);
                        }
                        else if (dt != null && dt2 != null && dt3 == null)    // 高德威+易华录
                        {
                            dt.Merge(dt2, false);
                        }
                        else if (dt != null && dt2 == null && dt3 == null)  // 易华录
                        {
                        }
                        else if (dt == null && dt2 != null && dt3 == null)  // 高德威
                        {
                            dt = dt2;
                        }
                        else if (dt == null && dt2 == null && dt3 != null)  // 宝康
                        {
                            dt = dt3;
                        }
                        else if (dt == null && dt2 == null && dt3 == null)  // 全空
                        {
                            MessageBox.Show("与治安卡口接口的服务器通讯出现问题,无法查询到数据。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("从对方服务器读取数据时发生错误");
                        writeToLog(ex, "clKaKou-ucKakou-12-从对方服务器读取数据时发生错误");
                        return;
                    }
                    #endregion


                    Application.DoEvents();

                    // 处理最后的数据
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (chkmh.Checked == false)
                            dt = RefreshData(dt, "1");

                        dt = InsertColumns(dt); //添加序列号
                        
                        Pagedt1 = dt;
                        InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                    }
                    frmpro.progressBar1.Value = 4;

                    frmpro.Close();

                    //clickTable = new DataTable();
                    //////////////////////////////////
                    if (dt != null && dt.Rows.Count > 0)
                        this._exportDT  = dt;

                    if (dtExcel != null) dtExcel.Clear();
                    dtExcel = dt;

                    Application.DoEvents();

                    //if (dt.Rows.Count > 0)
                    //    CreateKakouTrack(dt); //显示车辆经过的卡口并用线进行连接。   //  kakouTrack

                    WriteEditLog(SQLSearch, "经过车辆查询", "V_CROSS");

                    Writelog("在地图上显示信息点结束");

                    isShowPro(false);

                    MessageBox.Show(messageStr, "查询结果提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    isShowPro(false);
                    MessageBox.Show("查询关键字不能为空", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "SearchCarPass-12-读取高德威易华录数据时发生错误");
                //writeToLog(ex, "clKaKou-ucKakou-12-xxxxx 3");
            }
            finally
            {
                Application.DoEvents();
                this.Cursor = Cursors.Default;
            }           
        }

        /// <summary>
        /// 对数据按“通过时间”排序
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt1">数据源</param>
        /// <param name="num">排序类型</param>
        /// <returns>排序后的数据</returns>
        private DataTable RefreshData(DataTable dt1,string num)
        {
            try
            {
                DataTable dt2;
                dt2 = dt1.Copy();
                string strExpr = "通过时间";
                string strSort = string.Empty;

                if (num == "1")
                    strSort = "通过时间 desc";
                else if (num == "2")
                    strSort = "通过时间 desc";   // 是不是asc

                DataRow[] foundRows = dt1.Select("", strSort);
                dt2.Rows.Clear();
                foreach (DataRow dr in foundRows)
                {
                    dt2.ImportRow(dr);
                }

                return dt2;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "RefreshData");
                return null;
            }
        }
        /// <summary>
        /// 给数据表添加“序列号”列
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <returns>此数据表中增加了“序列号”列</returns>
        private DataTable InsertColumns(DataTable dt)
        {
            try
            {
                dt.Columns.Add("序列号", System.Type.GetType("System.String"));
                for (int i = dt.Rows.Count ; i >0; i--)
                {
                    dt.Rows[dt.Rows.Count - i]["序列号"] = (i).ToString();
                }
            }
            catch (Exception ex) { writeToLog(ex, "InsertColumns-13-添加列"); }
            return dt;

            //DataTable dtnew = new DataTable();
            //try
            //{
            //    dtnew.Columns.Add("序列号", System.Type.GetType("System.String"));
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        dtnew.Columns.Add(dt.Columns[i].ToString(), System.Type.GetType("System.String"));
            //    }
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        DataRow dr = dtnew.NewRow();
                    
            //        dr["序列号"]= (i + 1).ToString();
                    
            //        for(int j=0;j<dt.Columns.Count ;j++)
            //        {
            //          dr[dt.Columns[j].ToString()] = dt.Rows[i][j].ToString();
            //        }
            //        dt.Rows.Add(dr);
            //    }
            //}
            //catch { }
            //return dt;
        }




        Color[] colors = new Color[141] { Color.AliceBlue,Color.AntiqueWhite,Color.Aqua,Color.Aquamarine,Color.Azure,Color.Beige,                               
                                            Color.Bisque,Color.Black,Color.BlanchedAlmond,Color.Blue,Color.BlueViolet,Color.Brown,                                
                                            Color.BurlyWood,Color.CadetBlue,Color.Chartreuse,Color.Chocolate,Color.Coral,Color.CornflowerBlue,                    
                                            Color.Cornsilk,Color.Crimson,Color.Cyan,Color.DarkBlue,Color.DarkCyan,Color.DarkGoldenrod,                            
                                            Color.DarkGray,Color.DarkGreen,Color.DarkKhaki,Color.DarkMagenta,Color.DarkOliveGreen,Color.DarkOrange,               
                                            Color.DarkOrchid,Color.DarkRed,Color.DarkSalmon,Color.DarkSeaGreen,Color.DarkSlateBlue,Color.DarkSlateGray,           
                                            Color.DarkTurquoise,Color.DarkViolet,Color.DeepPink,Color.DeepSkyBlue,Color.DimGray, Color.DodgerBlue,                
                                            Color.Firebrick,Color.FloralWhite,Color.ForestGreen,Color.Fuchsia,Color.Gainsboro,Color.GhostWhite,                   
                                            Color.Gold,Color.Goldenrod,Color.Gray,Color.Green,Color.GreenYellow,Color.Honeydew,Color.HotPink,                     
                                            Color.IndianRed,Color.Indigo,Color.Ivory,Color.Khaki,Color.Lavender,Color.LavenderBlush,Color.LawnGreen,              
                                            Color.LemonChiffon,Color.LightBlue,Color.LightCoral,Color.LightCyan,Color.LightGoldenrodYellow,Color.LightGray,       
                                            Color.LightGreen,Color.LightPink,Color.LightSalmon,Color.LightSeaGreen,Color.LightSkyBlue,Color.LightSlateGray,       
                                            Color.LightSteelBlue,Color.LightYellow,Color.Lime,Color.LimeGreen,Color.Linen,Color.Magenta,                          
                                            Color.Maroon,Color.MediumAquamarine,Color.MediumBlue,Color.MediumOrchid,Color.MediumPurple,Color.MediumSeaGreen,      
                                            Color.MediumSlateBlue,Color.MediumSpringGreen,Color.MediumTurquoise,Color.MediumVioletRed,Color.MidnightBlue,Color.MintCream,                                             
                                            Color.MistyRose,Color.Moccasin,Color.NavajoWhite,Color.Navy,Color.OldLace,Color.Olive,                                    
                                            Color.OliveDrab,Color.Orange,Color.OrangeRed,Color.Orchid,Color.PaleGoldenrod,Color.PaleGreen,                            
                                            Color.PaleTurquoise,Color.PaleVioletRed,Color.PapayaWhip,Color.PeachPuff,Color.Peru,Color.Pink,                           
                                            Color.Plum,Color.PowderBlue,Color.Purple,Color.Red,Color.RosyBrown,Color.RoyalBlue,                                       
                                            Color.SaddleBrown,Color.Salmon,Color.SandyBrown,Color.SeaGreen,Color.SeaShell,Color.Sienna,                               
                                            Color.Silver,Color.SkyBlue,Color.SlateBlue,Color.SlateGray,Color.Snow,Color.SpringGreen,                                  
                                            Color.SteelBlue,Color.Tan,Color.Teal,Color.Thistle,Color.Tomato,Color.Transparent,                                        
                                            Color.Turquoise,Color.Violet,Color.Wheat,Color.White,Color.WhiteSmoke,Color.Yellow,Color.YellowGreen};

        private bool fuzzyFlag = false;         // 在卡口连线时用于判断是否是车辆模糊查询 lili 2010-12-21

        /// <summary>
        /// 创建卡口查询图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据表</param>
        private void CreateKakouTrack(DataTable dt)
        {
            try
            {
                try
                {
                    if (mapControl1.Map.Layers["KKSearchLayer"] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("KKSearchLayer");
                    }

                    if (mapControl1.Map.Layers["KKSearchLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("KKSearchLabel");
                    }


                    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                    //创建临时层
                    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("KKSearchLayer");
                    Table tblTemp = Cat.GetTable("KKSearchLayer");
                    if (tblTemp != null) //Table exists close it
                    {
                        Cat.CloseTable("KKSearchLayer");
                    }

                    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 50));

                    tblTemp = Cat.CreateTable(tblInfoTemp);
                    FeatureLayer lyr = new FeatureLayer(tblTemp);
                    //mapControl1.Map.Layers.Add(lyr);
                    mapControl1.Map.Layers.Insert(0, lyr);

                    //添加标注
                    if (this.chkmh.Checked == false && fuzzyFlag && this.txtBoxCar.Text.Trim() != "无号牌")
                    {
                        string activeMapLabel = "KKSearchLabel";
                        MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                        MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                        MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                        lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                        lbsource.DefaultLabelProperties.Style.Font.Size = 20;
                        lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                        lbsource.DefaultLabelProperties.Layout.Offset = 2;
                        lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                        lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                        lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                        lbsource.DefaultLabelProperties.Caption = "Name";
                        lblayer.Sources.Append(lbsource);
                        mapControl1.Map.Layers.Add(lblayer);
                    }
                    
                }
                catch (Exception ex) { writeToLog(ex, "CreateKakouTrack-创建图层完毕"); }

                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;
                Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                this.SetLayerEdit("KKSearchLayer");   // 设置该图层可编辑

                double x = 0;
                double y = 0;
                double px = 0;
                double py = 0;

                //int i = 0;
                int ci = 0;

                InitUsedFlag();

                foreach (DataRow dr in dt.Rows)
                {
                    //if (i > 100)   // 为提高效率画满100个后不画了
                    //    return;

                    //通过卡口编号获取所在的经纬度
                    string KaID = dr["卡口编号"].ToString().Trim();    // 卡口编号
                    string id = dr["通行车辆编号"].ToString();         // 通行车辆编号
                    string tnum = dr["序列号"].ToString();             // 序列号

                    for (int k = 0; k < KKiD.Length; k++)
                    {
                        if (KaID == KKiD[k])
                        {
                            if (KKnum[k] != 0)
                            {
                                tnum = KKnum[k].ToString();
                            }
                            else
                            {
                                KKnum[k] = Convert.ToInt32(tnum.Trim());
                            }
                            break;
                        }
                    }

                    DataTable dt1 = GetTable("select X,Y from 治安卡口系统 where 卡口编号='" + KaID + "'");
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow dr1 in dt1.Rows)
                        {
                            if (dr1["X"].ToString() != "" && dr1["Y"].ToString() != "")
                            {
                                x = Convert.ToDouble(dr1["X"]);
                                y = Convert.ToDouble(dr1["Y"]);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    if (x != 0 && y != 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrKKTrack.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("STOP1-32.BMP", BitmapStyles.ApplyColor, colors[ci], 10));
                        Feature ftr = new Feature(tblKKTRA.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = tnum;         // 序列号
                        //ftr["ID"] = id;           // 通行车辆编号
                        ftr["ID"] = tnum;           // 用序列号作为ID  因为通行车辆编号会有重复
                        tblKKTRA.InsertFeature(ftr);

                        if (px != 0 && py != 0 && x != 0 && y != 0 && this.chkmh.Checked == false && fuzzyFlag && this.txtBoxCar.Text.Trim() != "无号牌")
                            Trackline(px, py, x, y);

                        px = x;
                        py = y;
                        //i = i + 1;
                        if (ci > 139)
                        {
                            ci = ci - 1;
                        }
                        else
                        {
                            ci = ci + 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateKakouTrack-14-创建卡口查询图层"); 
            }
        }

        /// <summary>
        /// 修改通过卡口次数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dr">数据行</param>
        private void picturePoint(DataRow dr)
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;
                Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                this.SetLayerEdit("KKSearchLayer");   // 设置该图层可编辑

                double x = 0;
                double y = 0;

                int ci = 0;

                //通过卡口编号获取所在的经纬度
                string KaID = dr["卡口编号"].ToString().Trim();    // 卡口编号
                string id = dr["通行车辆编号"].ToString();         // 通行车辆编号
                string tnum = dr["序列号"].ToString();             // 序列号
                string KaName = dr["卡口名称"].ToString();         // 卡口名称

                DataTable dt1 = GetTable("select X,Y from 治安卡口系统 where 卡口编号='" + KaID + "'");
                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow dr1 in dt1.Rows)
                    {
                        if (dr1["X"].ToString() != "" && dr1["Y"].ToString() != "")
                        {
                            x = Convert.ToDouble(dr1["X"]);
                            y = Convert.ToDouble(dr1["Y"]);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                for (int k = 0; k < dataKaKou.Rows.Count; k++)
                {
                    if (KaName == dataKaKou.Rows[k]["卡口名称"].ToString().Trim())
                    {
                        string tID = dataKaKou.Rows[k]["序列号"].ToString().Trim();
                        getFeatureCollection(tID, "KKSearchLayer");
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                FeatureGeometry pt = new MapInfo.Geometry.Point(lyrKKTrack.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("STOP1-32.BMP", BitmapStyles.ApplyColor, colors[ci], 10));
                                f.Geometry = pt;
                                f.Style = cs;
                                f["Name"] = tnum;         // 序列号
                                // f["ID"] = id;          // 通行车辆编号
                                f["ID"] = tnum;           // 用序列号作为ID  因为通行车辆编号会有重复
                                tblKKTRA.UpdateFeature(f);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "picturePoint");
            }
        }

        string[] KKiD;
        Int32[] KKnum;
        string[] KKname;
        /// <summary>
        /// 获取所有卡口编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void InitUsedFlag()
        {
            try
            {
                DataTable dt = GetTable("Select * from 治安卡口系统");
                if (dt.Rows.Count > 0)
                {
                    KKiD = new string[dt.Rows.Count];
                    KKnum = new Int32[dt.Rows.Count];
                    KKname = new string[dt.Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string camid = dr["卡口编号"].ToString();
                        string caName = dr["卡口名称"].ToString();
                        KKiD[i] = camid ;
                        KKname[i] = caName;
                        KKnum[i] = 0;
                        i++;
                    }
                }            
            }
            catch (Exception ex)
            {
                writeToLog(ex, "15-InitUsedFlag");
            }
        }

        /// <summary>
        /// 车辆经过治安卡口时的轨迹线
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="x1">上次的经度</param>  
        /// <param name="y1">上次的纬度</param>  
        /// <param name="x2">本次的经度</param>  
        /// <param name="y2">本次的纬度</param>  
        private void Trackline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer =null;
                MapInfo.Data.Table tblTemp = null; 

                if (this.mapControl1.Map.Layers["KKSearchLayer"] != null)
                {
                    workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["KKSearchLayer"];
                    tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                }
                else if (this.mapControl1.Map.Layers["KakouTrackLayer"] != null)
                {
                    workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["KakouTrackLayer"];
                    tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("KakouTrackLayer");
                }

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                // 2 没有箭头
                //54 多个箭头
                // 59 单个箭头
                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(2, MapInfo.Styles.LineWidthUnit.Pixel), 59, System.Drawing.Color.Red);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Trackline-16-车辆经过治安卡口时的轨迹线");
            }
        }

        /// <summary>
        /// 设置dataGridViewKakou颜色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridViewKakou_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dataGridViewKakou.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dataGridViewKakou.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dataGridViewKakou.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dataGridViewKakou.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewKakou_DataBindingComplete-17-设置dataGridViewKakou颜色");
            }
        }

        private IResultSetFeatureCollection rsfcflash = null;
        /// <summary>
        /// 单击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridViewKakou_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView portQueryView = (DataGridView)sender;

                this.dataGridViewKakou.Rows[e.RowIndex].Selected = true;

                int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // 最后一列的位置
                string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();
                string KaIDnu = portQueryView.CurrentRow.Cells[1].Value.ToString();
                string serNum = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // 序列号

                string tblname = "KKSearchLayer";

                //提取当前选择的信息的通行车辆编号作为主键值

                MapInfo.Mapping.Map map = this.mapControl1.Map;

                if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }

                DataRow[] row = dataKaKou.Select("序列号='" + serNum + "'");
                picturePoint(row[0]);                                              // 创建新的编号

                getFeatureCollection(serNum, tblname);
                MapInfo.Geometry.CoordSys cSys = this.mapControl1.Map.GetDisplayCoordSys();
                if (this.rsfcflash.Count > 0)
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                    foreach (Feature f in this.rsfcflash)
                    {
                        mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                        mapControl1.Map.Center = f.Geometry.Centroid;
                        break;
                    }

                    this.timerFlash.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewKakou_CellClick-18-在dataGridViewKakou列表上单击");
            }
        }

        /// <summary>
        /// 删除地图上的点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        private void delPoint(double x,double y)
        {
            try
            {
                FeatureLayer fLayer = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;
                Table table = fLayer.Table;

                FeatureGeometry pt = new MapInfo.Geometry.Point(fLayer.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                Feature fct = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("KKSearchLayer", MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(pt, ContainsType.Centroid));
         
                table.DeleteFeature(fct);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "delPoint");
            }
        }

        /// <summary>
        /// 获取缩放比例
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <returns>比例尺</returns>
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
                CLC.BugRelated.ExceptionWrite(ex, "getScale");
                return 0;
            }
        }

        private int iflash = 0;
        /// <summary>
        /// 图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void timerFlash_Tick(object sender, EventArgs e)
        {
            try
            {
                iflash = iflash + 1;

                int i = iflash % 2;
                if (i == 0)
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                }
                else
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }

                if (this.iflash % 10 == 0)
                {
                    this.timerFlash.Enabled = false;
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }
            }
            catch (Exception ex)
            {
                this.timerFlash.Enabled = false;
                writeToLog(ex, "timerFlash_Tick-19-图元闪烁");
            }
        }

        /// <summary>
        /// 双击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridViewKakou_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出
            try
            {
                DataGridView portQueryView = (DataGridView)sender;

                string[] sqlFields = {"通行车辆编号","卡口编号","卡口名称","通过时间","车辆号牌","号牌种类","车身颜色","颜色深浅","行驶方向","照片1","照片2","照片3"};
                DataTable datatable = new DataTable("TemData");
                for (int i = 0; i < sqlFields.Length; i++)
                {
                    DataColumn dc = new DataColumn(sqlFields[i]);
                    if (i == 3)
                    {
                        dc.DataType = System.Type.GetType("System.DateTime");
                    }
                    else
                    {
                        dc.DataType = System.Type.GetType("System.String");
                    }
                    datatable.Columns.Add(dc);
                }

                DataRow dr = datatable.NewRow();
                for (int i = 0; i < sqlFields.Length; i++)
                {
                    dr[i] = portQueryView.CurrentRow.Cells[sqlFields[i]].Value.ToString();
                }
                datatable.Rows.Add(dr);

                /////////根据当前通行车辆编号判断图片服务器地址
                try
                {
                    if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "01")   //高德威视频网图片服务器地址
                    {
                        photoserver = gdwserver;
                    }
                    else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "02") //易华录视频网图片服务器地址
                    {
                        photoserver = ehlserver;
                    }
                    else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "03")  //宝康视频网图片服务器地址
                    {
                        photoserver = bkserver;
                    }
                    else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().IndexOf("lbschina") > -1)
                    {
                        photoserver = "http://192.168.0.50/";
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "clKaKou-ucKakou-20-车辆通过信息根据ip地址获取图片服务器地址");
                }

                DPoint dp = new DPoint();

                if (datatable.Rows.Count > 0)
                {                    
                    //////////////////////////////////////////
                    string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                    int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // 最后一列的位置
                    string serNum = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // 序列号

                    string tblname = "KKSearchLayer";

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        MessageBox.Show("此对象未定位,无法进行操作!", "提示");
                        return;
                    }                   
                 
                    try
                    {
                        getFeatureCollection(serNum, tblname);
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }       
                            this.timerFlash.Enabled = true;
                        }
                    }
                    catch (Exception ex) { writeToLog(ex, "clKaKou-ucKakou-20-在地图上搜索对应信息时发生错误"); }

                    /////////////////////////////////////////                    

                    System.Drawing.Point pt = new System.Drawing.Point();
                    if (dp.x == 0 || dp.y == 0)
                    {
                        MessageBox.Show("此对象未定位,无法进行操作!", "提示");
                        return;
                    }
                    mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                    WriteEditLog("终端车辆号牌='" + portQueryView.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情", "V_CROSS");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewKakou_CellDoubleClick-20-数据表dataGridViewKakou双击");
            }
            finally
            {
                try
                {
                    fmDis.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取地图上要素
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="fearID">图元信息</param>
        /// <param name="tblname">表名</param>
        private void getFeatureCollection(string fearID,string tblname)
        {
            MapInfo.Data.MIConnection conn = new MIConnection();
            rsfcflash = null;  
            try
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();

                MapInfo.Data.MICommand cmd = conn.CreateCommand();
                Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where ID = @name ";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@name", fearID);

                this.rsfcflash = cmd.ExecuteFeatureCollection();

                cmd.Dispose();
            }
            catch (Exception ex) { writeToLog(ex, "getFeatureCollection"); }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }

        /// <summary>
        /// 显示车辆详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private FrmInfo frminfo = new FrmInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (this.frminfo.Visible == false)
                {
                    this.frminfo = new FrmInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.Visible = false;
                }
                frminfo.photoserver = photoserver;
                frminfo.mapControl = this.mapControl1;
                frminfo.layerName = "KKSearchLayer";
                frminfo.getFromNamePath = this.getfrompath;
                frminfo.setInfo(dt.Rows[0], pt, StrCon,user);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " disPlayInfo-21-显示车辆详细信息");
            }
        }

        /// <summary>
        /// 切换要进行查询的表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void comboxTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //通过名称获取表的各个字段名称，并添加到字段dropdownlist
                if (this.comboxTable.Text == "车辆通过信息")
                {
                    this.comboxField.Items.Clear();
                    this.comboxField.Items.Add("卡口编号");
                    this.comboxField.Items.Add("卡口名称");
                    this.comboxField.Items.Add("通过时间");
                    this.comboxField.Items.Add("车辆号牌");
                    this.comboxField.Items.Add("号牌种类");
                    this.comboxField.Items.Add("车身颜色");
                    this.comboxField.Items.Add("颜色深浅");
                    this.comboxField.Items.Add("卡口方向");
                }
                else if (this.comboxTable.Text == "车辆报警信息")
                {
                    this.comboxField.Items.Clear();
                    this.comboxField.Items.Add("报警类型");
                    this.comboxField.Items.Add("报警卡口编号");
                    this.comboxField.Items.Add("报警卡口名称");
                    this.comboxField.Items.Add("报警时间");
                    this.comboxField.Items.Add("车辆号牌");
                    this.comboxField.Items.Add("号牌种类");
                    this.comboxField.Items.Add("布控名单编号");
                    this.comboxField.Items.Add("布控单位");
                    this.comboxField.Items.Add("布控人");
                }
                else if (this.comboxTable.Text == "治安卡口信息")
                {
                    this.comboxField.Items.Clear();
                    this.comboxField.Items.Add("卡口名称");
                    this.comboxField.Items.Add("卡口编号");
                    this.comboxField.Items.Add("安装地点");
                    this.comboxField.Items.Add("监控点接壤地区");
                    this.comboxField.Items.Add("所属派出所");
                    this.comboxField.Items.Add("监控方向");
                    this.comboxField.Items.Add("联系人");
                    this.comboxField.Items.Add("来源");
                }


                this.comboxField.Text = this.comboxField.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "comboxTable_SelectedIndexChanged-22-切换要进行查询的表");                
            }
        }

        /// <summary>
        /// 根据字段类型改变输入框
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void comboxField_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.comboxField.Text == "通过时间" || this.comboxField.Text == "报警时间")
                {
                    this.comboxCon.Items.Clear();
                    this.comboxCon.Items.Add("等于");
                    this.comboxCon.Items.Add("大于");
                    this.comboxCon.Items.Add("小于");

                    dateTimePicker2.Visible = true;
                    this.dateTimePicker2.Text = System.DateTime.Now.ToString();
                    this.CaseSearchBox.Visible = false;
                }
                else
                {
                    this.comboxCon.Items.Clear();
                    this.comboxCon.Items.Add("等于");
                    this.comboxCon.Items.Add("不等于");
                    this.comboxCon.Items.Add("包含");

                    dateTimePicker2.Visible = false;
                    this.CaseSearchBox.Visible = true;
                }
                this.comboxCon.Text = this.comboxCon.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "comboxField_SelectedIndexChanged-23-根据字段类型改变输入框");
            }
        }


        /// <summary>
        /// 添加查询条件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboxTable.Text.Trim() == "")
                {
                    MessageBox.Show("请选择表");
                    return;
                }

                if (this.CaseSearchBox.Visible && CaseSearchBox.Text.Trim() == "")
                {
                    MessageBox.Show("查询值不能为空！");
                    return;
                }

                if (this.CaseSearchBox.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("输入的字符串中不能包含特殊字符!");
                    return;
                }

                string strExp = "";
               
                 if(this.comboxField.Text =="double")
                 {
                     if (this.dataGridViewValue.Rows.Count == 0)
                        {

                            strExp = this.comboxField.Text + " " + this.comboxCon.Text + " " + CaseSearchBox.Text.Trim();
                        }
                        else
                        {
                            if (this.comboxORA.Text == "")
                            {
                                MessageBox.Show("条件连接方式出错,请检查后重新输入！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            strExp = this.comboxORA.Text + " " + this.comboxField.Text + " " + this.comboxCon.Text + " " + CaseSearchBox.Text.Trim();
                        }
                        this.dataGridViewValue.Rows.Add(new object[] { strExp, "数字" });
                 }

                 else if(this.comboxField.Text == "通过时间"||this.comboxField.Text =="报警时间")
                 {
                        string tValue = this.dateTimePicker2.Value.ToString();
                        if (tValue == "")
                        {
                            MessageBox.Show("查询值不能为空！");
                            return;
                        }

                        if (this.dataGridViewValue.Rows.Count == 0)
                        {                           
                            strExp = this.comboxField.Text + " " + this.comboxCon.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "时间" });
                        }
                        else
                        {

                            if (this.comboxORA.Text == "")
                            {
                                MessageBox.Show("条件连接方式出错,请检查后重新输入！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            strExp = this.comboxORA.Text + " " + this.comboxField.Text + " " + this.comboxCon.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "时间" });
                        }                        
                 }
                 else 
                 {
                     if (this.dataGridViewValue.Rows.Count == 0)
                        {

                            strExp = this.comboxField.Text + " " + this.comboxCon.Text + " '" + this.CaseSearchBox.Text.Trim() + "'";
                            if (this.comboxCon.Text.Trim() == "包含")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        else
                        {
                            if (this.comboxORA.Text == "")
                            {
                                MessageBox.Show("条件连接方式出错,请检查后重新输入！","错误",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                                return;
                            }

                            strExp = this.comboxORA.Text + " " + this.comboxField.Text + " " + this.comboxCon.Text + " '" + this.CaseSearchBox.Text.Trim() + "'";
                            if (this.comboxCon.Text.Trim() == "包含")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                }
                this.comboxTable.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button5_Click-24-添加查询条件");
            }
        }

        /// <summary>
        /// 移除一个表达式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridViewValue.CurrentRow.Index != 0)
                {
                    this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                }
                else
                {
                    this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                    string text = this.dataGridViewValue.Rows[0].Cells["Value"].Value.ToString().Replace("并且", "");

                    text = text.Replace("或者", "").Trim();
                    this.dataGridViewValue.Rows[0].Cells["Value"].Value = text;
                }

                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    this.comboxTable.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button4_Click-25-移除一个表达式");
            }
        }


       

        /// <summary>
        /// 组合查询 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    MessageBox.Show("请添加查询条件!");
                    return;
                }

                if (getSqlString() == "")
                {
                    MessageBox.Show("查询条件有错误,请重设!");
                    return;
                }
                dataGridView5.Rows.Clear();

                this.Cursor = Cursors.WaitCursor;



                this.st.Text = "正在查询......";
                Application.DoEvents();

                ClearKaKouTemp();

                string fieldtext = string.Empty;
                string NewSql = string.Empty;
                DataTable dt = new DataTable();

                Application.DoEvents();

                if (this.comboxTable.Text == "车辆通过信息")  //V_Cross
                {
                    dt = this.GetPassCar(this.getSqlString());

                    Application.DoEvents();

                    Pagedt2 = dt;

                    InitDataSet2(this.RCount2, this.PageNow2, this.PageNum2, bindingSource2, bindingNavigator2, dgvres);
                    WriteEditLog(NewSql, "组合查询", "V_CROSS");

                    MessageBox.Show(messageStr, "查询结果提示", MessageBoxButtons.OK, MessageBoxIcon.None);

                }
                else if (this.comboxTable.Text == "车辆报警信息")// V_Alarm
                {

                    try
                    {
                        fieldtext = " select 报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3 from V_ALARM where ";
                        NewSql = fieldtext + this.getSqlString() + " order by 报警时间 desc";
                        SQLSearch = NewSql;


                        WriteEditLog(NewSql, "组合查询", "V_ALARM");
                        dt = GetTable(NewSql);
                        Application.DoEvents();
                        this.st.Text = "正在查询车辆报警信息......";
                        if (dt.Rows.Count > 0)
                        {
                            Pagedt2 = dt;
                            InitDataSet2(this.RCount2, this.PageNow2, this.PageNum2, bindingSource2, bindingNavigator2, dgvres);
                        }

                        Application.DoEvents();
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            this._exportDT = dt;
                            if (dt.Rows.Count > 0)
                            {
                                //this.dgvres.DataSource 
                                DrawPoints(dt);   //在地图上画点
                            }
                        }
                        else
                        {
                            MessageBox.Show("查询结果为0", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                    }
                    catch(Exception ex)
                    {
                        writeToLog(ex, "button2_Click-26-车辆报警信息");
                    }
                }
                else if (this.comboxTable.Text == "治安卡口信息")// V_Alarm
                {
                    fieldtext = " select 卡口编号,卡口名称,安装地点,监控点接壤地区,卡口对应车道数,监控车道数,所属派出所,监控方向,联系人,联系方式,来源 as 数据来源 from 治安卡口系统 where ";
                    NewSql = fieldtext + this.getSqlString() + " order by 卡口编号 ";
                    SQLSearch = NewSql;
                    dt = GetTable(NewSql);
                    Application.DoEvents();

                    this.st.Text = "正在查询治安卡口信息......";

                    if (dt.Rows.Count > 0)
                    {
                        Pagedt2 = dt;
                        InitDataSet2(this.RCount2, this.PageNow2, this.PageNum2, bindingSource2, bindingNavigator2, dgvres);
                    }
                    WriteEditLog(NewSql, "组合查询", "治安卡口系统");

                    Application.DoEvents();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        this._exportDT = dt;
                        if (dt.Rows.Count > 0)
                        {
                            //this.dgvres.DataSource 
                            DrawPoints(dt);   //在地图上画点
                        }
                    }
                    else
                    {
                        MessageBox.Show("查询结果为0", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                this.st.Text = "查询完成......";
                

                Application.DoEvents();

                if (dtExcel != null) dtExcel.Clear();
                this.dtExcel = dt;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button2_Click-26-组合查询");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }



        private string messageStr = "";  // 用于显示外部数据提供商的查询结果及异常信息

        /// <summary>
        /// 查询通过车辆信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>查询结果</returns>
        public DataTable GetPassCar(string sql)
        {

            FrmProgress frmpro = new FrmProgress();
            frmpro.progressBar1.Maximum = 6;
            frmpro.Show();
            frmpro.progressBar1.Value = 0;
            //frmpro.TopMost = true;

           string  fieldtext = " select 通行车辆编号,卡口编号,卡口名称,通过时间,车辆号牌,号牌种类,车身颜色,颜色深浅,行驶方向,照片1,照片2,照片3 from V_Cross where ";
           string NewSql = fieldtext + sql;
           string   SQLSearch = NewSql;

            Writelog(NewSql);

            DataTable dt = new DataTable();

            frmpro.progressBar1.Value = 1;

            Application.DoEvents();

            messageStr = "";

            try
            {
                string sqlstr = NewSql;

                dt = ehltgs.GetPassCarInfo(sqlstr, ref ehlserver);

                //this.st.Text = "正在查询易华录数据......";
                frmpro.label1.Text = "正在查询易华录数据......";
                if (dt == null)
                {
                    Writelog("与易华录服务器通讯出现问题,无法查询到数据。");
                    messageStr += "易华录服务器通讯出现问题,无法查询到数据\t\n";
                }
                else
                {
                    messageStr += "易华录服务器查询到" + dt.Rows.Count + "条数据\t\n";
                }
                frmpro.progressBar1.Value = 2;

                Application.DoEvents();

                frmpro.label1.Text = "正在查询高德威数据......";
                DataTable dt2 = gdwcom.QueryData(sqlstr, ref gdwserver);

                if (dt2 == null)
                {
                    Writelog("与高德威服务器通讯出现问题,无法查询到数据。");
                    messageStr += "高德威服务器通讯出现问题,无法查询到数据\t\n";
                }
                else
                {
                    messageStr += "高德威服务器查询到" + dt2.Rows.Count + "条数据\t\n";
                }
                frmpro.progressBar1.Value = 3;

                Application.DoEvents();
                frmpro.label1.Text = "正在查询宝康数据......";
                PassDataQuery psqy = new PassDataQuery();
                DataTable dt3 = psqy.QueryPassData(sqlstr, ref bkserver);    //宝康查询接口   

                if (dt3 == null)
                {
                    Writelog("与宝康服务器通讯出现问题,无法查询到数据。");
                    messageStr += "宝康服务器通讯出现问题,无法查询到数据\t\n";
                }
                else
                {
                    messageStr += "宝康服务器查询到" + dt3.Rows.Count + "条数据\t\n";
                }
                frmpro.progressBar1.Value = 4;
                Application.DoEvents();

                #region 包含宝康查询
                try
                {
                    if (dt != null && dt2 != null && dt3 != null)   //易华录+高德威 + 宝康
                    {
                        dt2.Merge(dt3, false);
                        dt.Merge(dt2, false);
                    }
                    else if (dt == null && dt2 != null && dt3 != null)   //高德威 + 宝康
                    {
                        dt2.Merge(dt3, false);
                        dt = dt2;
                    }
                    else if (dt != null && dt2 == null && dt3 != null)   //易华录 + 宝康
                    {
                        dt.Merge(dt3, false);
                    }
                    else if (dt != null && dt2 != null && dt3 == null)    // 高德威+易华录
                    {
                        dt.Merge(dt2, false);
                    }
                    else if (dt != null && dt2 == null && dt3 == null)  // 易华录
                    {
                    }
                    else if (dt == null && dt2 != null && dt3 == null)  // 高德威
                    {
                        dt = dt2;
                    }
                    else if (dt == null && dt2 == null && dt3 != null)  // 宝康
                    {
                        dt = dt3;
                    }
                    else if (dt == null && dt2 == null && dt3 == null)  // 全空
                    {
                        isShowPro(false);
                        MessageBox.Show("与治安卡口接口的服务器通讯出现问题,无法查询到数据。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    MessageBox.Show("从对方服务器读取数据时发生错误");
                    writeToLog(ex, "clKaKou-ucKakou-26-从对方服务器读取数据时发生错误");
                }
                #endregion
                frmpro.progressBar1.Value = 5;
                Application.DoEvents();
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (NewSql.IndexOf("无号牌") < 0)
                        dt = RefreshData(dt, "2");

                    dt = InsertColumns(dt); //添加序列号

                    this._exportDT = dt;
                }

                frmpro.progressBar1.Value = 6;
                frmpro.Close();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GetPassCar");
            }

            return dt;
        }

        /// <summary>
        /// 将查询结果显示在地图上
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="datatable">数据源</param>
        /// <param name="tableName">表名</param>
        private void DrawPoints(DataTable dt)
        {
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (mapControl1.Map.Layers["KKSearchLayer"] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("KKSearchLayer");
                    }

                    if (mapControl1.Map.Layers["KKSearchLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("KKSearchLabel");
                    }                  

                    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                    //创建临时层
                    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("KKSearchLayer");
                    Table tblTemp = Cat.GetTable("KKSearchLayer");
                    if (tblTemp != null) //Table exists close it
                    {
                        Cat.CloseTable("KKSearchLayer");
                    }

                    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 50));

                    tblTemp = Cat.CreateTable(tblInfoTemp);
                    FeatureLayer lyr = new FeatureLayer(tblTemp);
                    mapControl1.Map.Layers.Insert(0, lyr);

                    //添加标注
                    string activeMapLabel = "KKSearchLabel";
                    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
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

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");


                    double Cx = 0;
                    double Cy = 0;

                    double px = 0;
                    double py = 0;

                    int i = 0;

                    foreach (DataRow dr in dt.Rows)
                    {

                        i++;
                        if (i > 50) return;

                        string kkname = string.Empty;
                        string CarName = string.Empty;
                        string idname = string.Empty;

                        if (this.comboxTable.Text == "车辆通过信息")
                        {
                            idname = dr["通行车辆编号"].ToString();
                            kkname = dr["卡口编号"].ToString();
                            CarName = dr["车辆号牌"].ToString();
                        }
                        else if (this.comboxTable.Text == "车辆报警信息")
                        {
                            idname = dr["报警编号"].ToString();
                            kkname = dr["报警卡口编号"].ToString();
                            CarName = dr["车辆号牌"].ToString();
                        }
                        else if (this.comboxTable.Text == "治安卡口信息")
                        {
                            idname = dr["卡口编号"].ToString();
                            CarName = dr["卡口名称"].ToString();
                            kkname = dr["卡口编号"].ToString();
                        }


                        //////获取

                       DataTable dt1 = GetTable("select X,Y from 治安卡口系统 where 卡口编号='" + kkname + "'");

                        if (dt1.Rows.Count > 0)
                        {
                            foreach (DataRow dr1 in dt1.Rows)
                            {
                                Cx = Convert.ToDouble(dr1["X"]);
                                Cy = Convert.ToDouble(dr1["Y"]);
                            }
                        }

                        i = i + 1;

                        if (Cx != 0 && Cy != 0)
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Cx, Cy)) as FeatureGeometry;
                            CompositeStyle cs = new CompositeStyle();
                            cs.ApplyStyle(new BitmapPointStyle("PIN1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 24));

                            Feature ftr = new Feature(tblcar.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = CarName;
                            ftr["ID"] = idname;
                            tblcar.InsertFeature(ftr);

                            if (this.comboxTable.Text == "车辆通过信息")
                            {
                                if (px != 0 && py != 0 && Cx != 0 && Cy != 0 && CaseSearchBox.Text.Trim() != "无号牌" && comboxField.Text == "车辆号牌" && comboxCon.Text == "等于")
                                    Trackline(px, py, Cx, Cy);

                                px = Cx;
                                py = Cy;
                            }
                        }
                    }                   
                }              
            }
            catch (Exception ex)
            {
                writeToLog(ex, "DrawPoints-27-将查询结果显示在地图上");
            }
        }        

        /// <summary>
        /// 转换SQL查询语句
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <returns>可供数据库查询的SQL</returns>
        private string getSqlString()//转换字符串
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = string.Empty;
                fuzzyFlag = false;

                for (int i = 0; i < this.dataGridViewValue.Rows.Count; i++)
                {
                    string type = this.dataGridViewValue.Rows[i].Cells[1].Value.ToString();
                    string str = this.dataGridViewValue.Rows[i].Cells[0].Value.ToString();

                    //if (comboxTable.Text == "车辆通过信息")    // 通过车辆信息的卡口名称要转换为卡口编号后查询  add by lili 2010-12-16
                    //    str = transSerial(str); 

                    if (str.IndexOf("车辆号牌 等于") > -1)
                        fuzzyFlag = true;

                    if (str.IndexOf("车辆号牌") > -1 && str.IndexOf("无号牌") > -1)
                    {
                        if (i > 0)
                        {
                            str = str.Substring(0, str.IndexOf("车辆号牌")) + " (车辆号牌='无号牌' or 车辆号牌 is null)";
                        }
                    }


                    if (type == "包含")
                    {
                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {
                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                str += " '%" + strArray[1] + "%'";
                            }
                        }
                        array.Add(str);
                    }
                    else if (type == "时间")
                    {

                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {

                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                //strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   to_date('" + tValue + "', 'YYYY-MM-DD HH24:MI:SS')";
                                str += "to_date('" + strArray[1] + "', 'YYYY-MM-DD HH24:MI:SS')";
                            }
                        }
                        array.Add(str);
                    }
                    else
                    {
                        array.Add(str);
                    }
                }

                for (int j = 0; j < array.Count; j++)
                {
                    if (j == 0)
                    {
                        getsql = array[j].ToString();
                    }
                    else
                    {
                        getsql += "   " + array[j].ToString();
                    }
                }

                getsql = getsql.Replace("并且", "and");
                getsql = getsql.Replace("或者", "or");
                getsql = getsql.Replace("包含", "like");
                getsql = getsql.Replace("大于等于", ">=");
                getsql = getsql.Replace("小于等于", "<=");
                getsql = getsql.Replace("大于", ">");
                getsql = getsql.Replace("小于", "<");
                getsql = getsql.Replace("不等于", "!=");
                getsql = getsql.Replace("等于", "=");

                
                return getsql;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getSqlString-28-转换SQL查询语句");
                return "";
            }
        }


        /// <summary>
        /// 此方法只供getSqlString方法使用，用于转换治安卡口名称转换为卡口编号    
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="tansSer">要转换的sql</param>
        /// <returns>卡口编号作为条件</returns>
        private string transSerial(string tansSer)
        {
            try
            {
                string newSql = "";

                if (tansSer.IndexOf("卡口名称 等于") > -1)
                {
                    string serails = tansSer.Substring(tansSer.IndexOf("'"));

                    DataTable dt = GetTable("select 卡口编号 from 治安卡口系统 where 卡口名称=" + serails);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (newSql == string.Empty)
                            newSql = "卡口编号 in ('" + dt.Rows[i][0].ToString() + "'";
                        else
                            newSql += ",'" + dt.Rows[i][0].ToString() + "'";
                    }

                    newSql += ")";

                    return newSql;
                }
                else
                {
                    return tansSer;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "transSerial");
                return tansSer;
            }
        }

        /// <summary>
        /// 重置查询条件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.dataGridViewValue.Rows.Clear();
                //this.textValue.Text = "";
                this.CaseSearchBox.Text = "";
                this.comboxTable.Enabled = true;
                this.dataKaKou = null;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button3_Click-29-重置查询条件");
            }
        }


        //private int alarmcount = 0;
        ///// <summary>
        ///// 查询报警信息并显示
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        ///// 
        //private void timalarm_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //            OracleConnection con = new OracleConnection(mysqlstr);
        //            if (con.State == ConnectionState.Open) { con.Close(); }
        //            con.Open();
                    
        //            string sqlstring = string.Empty ;
        //            if(this.ALARMUSER =="所有" || this.strRegion =="顺德区")
        //            {
        //                sqlstring = "Select 报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3,处理状态,处理人,处理时间,处理情况 from V_ALARM where 处理状态 is null order by 报警时间";
        //            }
        //            else if(this.ALARMUSER =="用户")
        //            {
        //                sqlstring = "Select 报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3,处理状态,处理人,处理时间,处理情况 from V_ALARM where 处理状态 is null' and  布控单位 in ('" + this.strRegion.Replace(",", "','") + "') order by 报警时间";
        //            }
                    
        //            OracleCommand cmd = new OracleCommand(sqlstring,con);
        //            cmd.ExecuteNonQuery();
        //            OracleDataAdapter apt = new OracleDataAdapter(cmd);
        //            DataTable dt = new DataTable();
        //            apt.Fill(dt);
        //            if (dt.Rows.Count > 0)
        //            {
        //                ftip.renum = dt.Rows.Count;
        //                ftip.label2.Text = "共有 " + dt.Rows.Count.ToString() + " 条卡口报警记录";
        //                if (ftip.Visible == false)
        //                {
        //                    ftip.Visible = true;
        //                }
        //                ftip.TopMost = true;
        //            }
        //            alarmcount = dt.Rows.Count;
        //    }
        //    catch (Exception ex)
        //    {
        //        writeToLog(ex, "查询报警信息并显示");
        //    }
        //    finally { }
        //}

        /// <summary>
        /// 改变列表颜色 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dgvres_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dgvres.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dgvres.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dgvres.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dgvres.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvres_DataBindingComplete-30-设置dgvresDataGrid颜色");
            }
        }

        AreaStyle aStyle;
        BaseLineStyle lStyle;
        TextStyle tStyle;
        BasePointStyle pStyle;

        /// <summary>
        /// 设置绘制要素的默认样式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        public void SetDrawStyle()
        {
            try
            {
                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2.5, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(1);
                aStyle = new AreaStyle(simLineStyle, simInterior);
                lStyle = new SimpleLineStyle(new LineWidth(2.5, MapInfo.Styles.LineWidthUnit.Point), 59, System.Drawing.Color.Red);
                tStyle = new TextStyle(new MapInfo.Styles.Font("黑体", 16.0, Color.Red, Color.Transparent, FontFaceStyle.Normal, FontWeight.Bold, TextEffect.None, TextDecoration.None, TextCase.Default, false, false));
                //pStyle = new SimpleVectorPointStyle(69, Color.Blue, 12);
                pStyle = new BitmapPointStyle("ren2.bmp", BitmapStyles.None, Color.Blue, 14);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetDrawStyle-31-设置绘制要素的默认样式");
            }
        }

        private bool isDel = false;
        /// <summary>
        /// 选择工具
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void mapToolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                UncheckedTool();
                isDel = false;
                switch (e.Button.Name)
                {
                    case "toolAddPoint":
                        mapControl1.Tools.LeftButtonTool = "AddPoint";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddPolyline":
                        mapControl1.Tools.LeftButtonTool = "AddPolyline";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddPolygon":
                        mapControl1.Tools.LeftButtonTool = "AddPolygon";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddText":
                        mapControl1.Tools.LeftButtonTool = "AddText";
                        e.Button.Pushed = true;
                        break;
                    case "toolPointStyle":
                        SymbolStyleDlg pStyleDlg = new SymbolStyleDlg();
                        pStyleDlg.SymbolStyle = pStyle;
                        if (pStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            pStyle = pStyleDlg.SymbolStyle;
                        }
                        break;
                    case "toolLineStyle":
                        LineStyleDlg lStyleDlg = new LineStyleDlg();
                        lStyleDlg.LineStyle = lStyle;
                        if (lStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            lStyle = lStyleDlg.LineStyle;
                        }
                        break;
                    case "toolAreaStyle":
                        AreaStyleDlg aStyleDlg = new AreaStyleDlg();
                        aStyleDlg.AreaStyle = aStyle;
                        if (aStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            aStyle = aStyleDlg.AreaStyle;
                        }
                        break;
                    case "toolTextStyle":
                        TextStyleDlg tStylrDlg = new TextStyleDlg();
                        tStylrDlg.FontStyle = tStyle.Font;
                        if (tStylrDlg.ShowDialog() == DialogResult.OK)
                        {
                            tStyle = new TextStyle(tStylrDlg.FontStyle);
                        }
                        break;
                    case "toolButtonDel":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        e.Button.Pushed = true;
                        isDel = true;
                        break;
                    case "toolBarRect":
                        mapControl1.Tools.LeftButtonTool = "SelByRect";
                        e.Button.Pushed = true;
                        break;
                    case "toolBarCircle":
                        mapControl1.Tools.LeftButtonTool = "SelByCircle";
                        e.Button.Pushed = true;
                        break;
                    case "toolBarPolygon":
                        mapControl1.Tools.LeftButtonTool = "SelByPolygon";
                        e.Button.Pushed = true;
                        break;
                }
                mapControl1.Map.Center = mapControl1.Map.Center;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar1_ButtonClick-32-选择工具");
            }
        }

        /// <summary>
        /// 设置鼠标显示状态
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void setSelectableLayers()
        {
            this.Cursor = Cursors.WaitCursor;

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 点击工具栏上的工具时，对工具按钮进行设置，
        /// 由于按钮分组，iFrom表示组的首Index，iEnd表示末Index
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void UncheckedTool()
        {
            try
            {

                for (int i = 0; i < this.mapToolBar1.Buttons.Count; i++)
                {
                    if (mapToolBar1.Buttons[i].Pushed)
                    {
                        mapToolBar1.Buttons[i].Pushed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "33-UncheckedTool");
            }
        }

        /// <summary>
        /// 添加图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        void Tools_FeatureAdded(object sender, MapInfo.Tools.FeatureAddedEventArgs e)
        {

            if (this.Visible)
            {
                try
                {
                    Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("KKLayer", MapInfo.Data.SearchInfoFactory.SearchWhere("ID is null or ID=''"));
                    switch (e.Feature.Type)
                    {
                        case MapInfo.Geometry.GeometryType.Point:
                            ft.Style = pStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.MultiCurve:
                            ft.Style = lStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.MultiPolygon:
                            ft.Style = aStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.LegacyText:
                            ft.Style = tStyle;
                            break;
                    }
                    ft["ID"] = "t1";
                    ft.Update();
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "34-Tools_FeatureAdded");
                }
            }
        }

        /// <summary>
        /// 选择图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void Tools_FeatureSelected(object sender, MapInfo.Tools.FeatureSelectedEventArgs e)
        {
            if (isDel == false) return;
            if (this.Visible)
            {
                try
                {
                    FeatureLayer fLayer = mapControl1.Map.Layers["KKLayer"] as FeatureLayer;
                    Table table = fLayer.Table;

                    //IResultSetFeatureCollection fc= MapInfo.Engine.Session.Current.Catalog.Search(table.Alias, MapInfo.Data.SearchInfoFactory.(e.Selection.Envelope,mapControl1.Map.FeatureCoordSys, ContainsType.Geometry));
                    IResultSetFeatureCollection fc = e.Selection[table];
                    foreach (Feature f in fc)
                    {
                        if (f["ID"].ToString() == "t1")
                        { table.DeleteFeature(f); }
                    }
                }
                catch(Exception ex)
                {
                    writeToLog(ex, "35-Tools_FeatureSelected");
                }
            }
        }


        private MapInfo.Geometry.DPoint dptStart;
        private MapInfo.Geometry.DPoint dptEnd;

        private System.Collections.ArrayList arrlstPoints = new ArrayList();

        /// <summary>
        /// 使用工具
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            try
            {
                if (this.Visible == true)
                {
                    switch (e.ToolName)
                    {
                        case "SelByRect":
                            //add by siumo 20080923
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    dptStart = e.MapCoordinate;
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    dptEnd = e.MapCoordinate;
                                    if (dptStart == dptEnd) return;
                                    MapInfo.Geometry.DRect MapRect = new MapInfo.Geometry.DRect();

                                    MapRect.x1 = dptStart.x;
                                    MapRect.y2 = dptStart.y;
                                    MapRect.x2 = dptEnd.x;
                                    MapRect.y1 = dptEnd.y;
                                    clearFeatures("查看选择");
                                    MapInfo.Geometry.Rectangle rect = new MapInfo.Geometry.Rectangle(mapControl1.Map.GetDisplayCoordSys(), MapRect);
                                    selectAndInsertByGeometry((FeatureGeometry)rect);
                                    WriteEditLog("框选显示对应图元", "框选", "指挥白板");
                                    break;
                            }
                            break;
                        case "SelByCircle":
                            //add by siumo 20080923
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    dptStart = e.MapCoordinate;
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    dptEnd = e.MapCoordinate;
                                    double radius = Math.Sqrt((dptEnd.y - dptStart.y) * (dptEnd.y - dptStart.y) + (dptEnd.x - dptStart.x) * (dptEnd.x - dptStart.x));

                                    Ellipse circle = new Ellipse(mapControl1.Map.GetDisplayCoordSys(), dptStart, radius, radius, DistanceUnit.Degree, DistanceType.Spherical);
                                    clearFeatures("查看选择");
                                    selectAndInsertByGeometry((FeatureGeometry)circle);
                                    WriteEditLog("圈选显示对应图元", "圈选", "指挥白板");
                                    break;
                            }
                            break;
                        case "SelByPolygon":
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    arrlstPoints.Clear();
                                    dptStart = e.MapCoordinate;
                                    arrlstPoints.Add(e.MapCoordinate);
                                    break;
                                case MapInfo.Tools.ToolStatus.InProgress:
                                    arrlstPoints.Add(e.MapCoordinate);
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    //构造一个闭合环
                                    //arrlstPoints.Add(e.MapCoordinate);
                                    arrlstPoints.Add(dptStart);
                                    int intCount = arrlstPoints.Count;
                                    if (intCount <= 3)
                                    {
                                        MessageBox.Show("请画3个以上的点形成面来测量你所要的面积");
                                        return;
                                    }
                                    MapInfo.Geometry.DPoint[] dptPoints = new DPoint[intCount];
                                    for (int i = 0; i < intCount; i++)
                                    {
                                        dptPoints[i] = (MapInfo.Geometry.DPoint)arrlstPoints[i];
                                    }
                                    //dptPoints[intCount] = dptFirstPoint;

                                    //用闭合的环构造一个面		
                                    MapInfo.Geometry.AreaUnit costAreaUnit;
                                    costAreaUnit = MapInfo.Geometry.CoordSys.GetAreaUnitCounterpart(DistanceUnit.Kilometer);
                                    MapInfo.Geometry.CoordSys objCoordSys = this.mapControl1.Map.GetDisplayCoordSys();
                                    MultiPolygon objPolygon = new MultiPolygon(objCoordSys, CurveSegmentType.Linear, dptPoints);
                                    if (objPolygon == null)
                                    {
                                        return;
                                    }
                                    clearFeatures("查看选择");
                                    selectAndInsertByGeometry((FeatureGeometry)objPolygon);
                                    WriteEditLog("多边形选择显示对应图元", "多边形", "指挥白板");
                                    break;
                                default:
                                    dptStart = e.MapCoordinate;
                                    break;
                            }
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_Used-36-使用工具");
            }
        }    

        private int i = 0; // 存储用户所选范围内的对象数 
        /// <summary>
        /// 判断查看选择的图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="geo">图元</param>
        private void selectAndInsertByGeometry(FeatureGeometry geo)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                i = 0;
                if (checkBoxChangsuo.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("公共场所") == null)
                    {
                        openTable("公共场所");
                    }
                    SpatialSearchAndView(geo, "公共场所");
                }
                if (checkBoxAnfang.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("安全防护单位") == null)
                    {
                        openTable("安全防护单位");
                    }
                    SpatialSearchAndView(geo, "安全防护单位");
                }
                if (checkBoxWangba.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("网吧") == null)
                    {
                        openTable("网吧");
                    }
                    SpatialSearchAndView(geo, "网吧");
                }
                if (checkBoxZhikou.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("治安卡口") == null)
                    {
                        openTable("治安卡口");
                    }
                    SpatialSearchAndView(geo, "治安卡口");
                }
                if (checkBoxTezhong.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("特种行业") == null)
                    {
                        openTable("特种行业");
                    }
                    SpatialSearchAndView(geo, "特种行业");
                }
                if (checkBoxShiping.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("视频") == null)
                    {
                        openTable("视频");
                    }
                    SpatialSearchAndView(geo, "视频");
                }
                if (checkBoxXiaofangshuan.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("消防栓") == null)
                    {
                        openTable("消防栓");
                    }
                    SpatialSearchAndView(geo, "消防栓");
                }
                if (checkBoxXiaofangdangwei.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("消防重点单位") == null)
                    {
                        openTable("消防重点单位");
                    }
                    SpatialSearchAndView(geo, "消防重点单位");
                }
                if (checkBoxJingche.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("警车") == null)
                    {
                        openTable("警车");
                    }
                    SpatialSearchAndView(geo, "警车");
                }
                if (checkBoxJingyuan.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("警员") == null)
                    {
                        openTable("警员");
                    }
                    SpatialSearchAndView(geo, "警员");
                }
                if (checkBoxPaichusuo.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("基层派出所") == null)
                    {
                        openTable("基层派出所");
                    }
                    SpatialSearchAndView(geo, "基层派出所");
                }
                if (checkBoxZhongdui.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("基层民警中队") == null)
                    {
                        openTable("基层民警中队");
                    }
                    SpatialSearchAndView(geo, "基层民警中队");
                }
                if (checkBoxJingwushi.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("社区警务室") == null)
                    {
                        openTable("社区警务室");
                    }
                    SpatialSearchAndView(geo, "社区警务室");
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                this.panError.Visible = false;
                if (i == 0)  // 最后判断是否所选范围内有对象
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2 - 250;
                    pt.Y = screen.WorkingArea.Height / 2 - 100;
                    this.panError.Location = pt;
                    this.panError.Visible = true;
                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "selectAndInsertByGeometry-37-判断查看选择的图层");
            }
        }

        /// <summary>
        /// 打开图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="tabAlias">表名</param>
        private void openTable(string tabAlias)
        {
            try
            {
                 CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,getfrompath);
                string strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName;              

                DataTable datatable = GetTable(strSQL);
                if (datatable == null || datatable.Rows.Count < 1)
                {
                    return;
                }
                i = i + datatable.Rows.Count;
                //这个地方用来生成地图，并放在Map中显视
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tabAlias, datatable);
                MapInfo.Data.SpatialSchemaXY xy = new MapInfo.Data.SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0, 0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;

                MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

            }
            catch (Exception ex)
            {
                writeToLog(ex, "openTable-38-打开图层");
            }
        }


        /// <summary>
        /// 查看选择显示图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="geo">图元</param>
        /// <param name="tabAlias">表名</param>
        private void SpatialSearchAndView(FeatureGeometry geo, string tabAlias)
        {
            try
            {
                FeatureLayer fl = mapControl1.Map.Layers["查看选择"] as FeatureLayer;
                Table ccTab = fl.Table;

                CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,getfrompath);
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(geo, ContainsType.Geometry);
                si.QueryDefinition.Columns = null;
                IResultSetFeatureCollection fc = MapInfo.Engine.Session.Current.Catalog.Search(tabAlias, si);
                //通过表名称获取图标
                string bmpName = CLC.ForSDGA.GetFromTable.BmpName;
                CompositeStyle cs;
                if (bmpName == "gonggong")
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(33, Color.Yellow, 9);
                    cs = new CompositeStyle(pStyle);
                }
                else if (bmpName == "tezhong")
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(36, Color.Cyan, 10);
                    cs = new CompositeStyle(pStyle);
                }
                else
                {
                    cs = new CompositeStyle(new BitmapPointStyle(bmpName));
                }

                Feature newFt;
                foreach (Feature ft in fc)
                {
                    i++;
                    newFt = new Feature(ccTab.TableInfo.Columns);
                    newFt["表_ID"] = ft[CLC.ForSDGA.GetFromTable.ObjID];
                    newFt["名称"] = ft[CLC.ForSDGA.GetFromTable.ObjName];
                    newFt["表名"] = tabAlias;
                    newFt.Geometry = ft.Geometry;
                    newFt.Style = cs;
                    ccTab.InsertFeature(newFt);
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "SpatialSearchAndView-39-查看选择显示图元");
            }
        }

        /// <summary>
        /// 查看选择
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.CheckBox check = (System.Windows.Forms.CheckBox)sender;

                if (!check.Checked)//去掉选择,从查看选择中删除该类对象
                {
                    string tableAlies = check.Text;
                    CLC.ForSDGA.GetFromTable.GetFromName(tableAlies,getfrompath);

                    FeatureLayer fl = mapControl1.Map.Layers["查看选择"] as FeatureLayer;
                    Table viewFtTable = fl.Table;
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("表名='" + tableAlies + "'");
                    si.QueryDefinition.Columns = null;
                    IResultSetFeatureCollection fc = MapInfo.Engine.Session.Current.Catalog.Search(viewFtTable, si);
                    foreach (Feature ft in fc)
                    {
                        viewFtTable.DeleteFeature(ft);
                    }

                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "checkBox_CheckedChanged-39-查看选择");
            }
        }

        //一定范围内的视频和车辆，创建视频图层和车辆图层，然后选择一定范围内的
        /// <summary>
        /// 创建视频图层和车辆图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dp">坐标</param>
        /// <param name="dis"></param>
        //public void SearchDistance(DPoint dp, double dis)
        //{
        //    // 创建 视频图层 
        //    try
        //    {
        //        clVideo.ucVideo fv = new clVideo.ucVideo(mapControl1, st, this.tddb, StrCon, this.VideoPort, this.VideoString, VEpath, true,false);
        //        fv.getNetParameter(ns, vf);
        //        fv.strRegion = this.strRegion;
        //        fv.SearchVideoDistance(dp, dis);
                
        //    }
        //    catch(Exception ex)
        //    {
        //        writeToLog(ex, "clKaKou-ucKakou-40-创建视频图层");    
        //    } 

        //    try
        //    {
        //        // 创建车辆图层
        //        clCar.ucCar fcar = new clCar.ucCar(mapControl1, null, StrCon, true);
        //        fcar.strRegion = strRegion;
        //        fcar.SearchCarDistance(dp, dis);
        //        fcar.ZhiHui = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        writeToLog(ex, "clKaKou-ucKakou-40-创建车辆图层");
        //    }
        //}

        /// <summary>
        /// 根据范围查找周围要素
        /// </summary>
        /// <param name="dp">坐标</param>
        /// <param name="dis">范围</param>
        public void SearchDistance(DPoint dp, double dis)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                clearFeatures("查看选择");   // 先清除之前操作的点
                i = 0;
                if (checkBoxChangsuo.Checked)
                {
                    SearchSomeDistance(dp, dis, "公共场所");
                    i++;
                }
                if (checkBoxTezhong.Checked)
                {
                    SearchSomeDistance(dp, dis, "特种行业");
                    i++;
                }
                if (checkBoxAnfang.Checked)
                {
                    SearchSomeDistance(dp, dis, "安全防护单位");
                    i++;
                }
                if (checkBoxXiaofangshuan.Checked)
                {
                    SearchSomeDistance(dp, dis, "消防栓");
                    i++;
                }
                if (checkBoxWangba.Checked)
                {
                    SearchSomeDistance(dp, dis, "网吧");
                    i++;
                }
                if (checkBoxXiaofangdangwei.Checked)
                {
                    SearchSomeDistance(dp, dis, "消防重点单位");
                    i++;
                }
                if (checkBoxZhikou.Checked)
                {
                    SearchSomeDistance(dp, dis, "治安卡口");
                    i++;
                }
                if (checkBoxPaichusuo.Checked)
                {
                    SearchSomeDistance(dp, dis, "基层派出所");
                    i++;
                }
                if (checkBoxShiping.Checked)
                {
                    SearchSomeDistance(dp, dis, "视频位置");
                    i++;
                }
                if (checkBoxZhongdui.Checked)
                {
                    SearchSomeDistance(dp, dis, "基层民警中队");
                    i++;
                }
                if (checkBoxJingche.Checked)
                {
                    SearchSomeDistance(dp, dis, "警车");
                    i++;
                }
                if (checkBoxJingwushi.Checked)
                {
                    SearchSomeDistance(dp, dis, "社区警务室");
                    i++;
                }
                if (checkBoxJingyuan.Checked)
                {
                    SearchSomeDistance(dp, dis, "警员");
                    i++;
                }

                if (i == 0)
                {
                    MessageBox.Show("请您先选择左边类型后查询！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SearchDistance");
            }
        }

        /// <summary>
        /// 周边查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dpt">坐标</param>
        /// <param name="dis">范围</param>
        /// <param name="tabAils">表名</param>
        public void SearchSomeDistance(MapInfo.Geometry.DPoint dpt, Double dis,string tabAils)
        {
            try
            {
                //FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["查看选择"];
                //if (fl == null)   // 为空先创建
                //{
                //    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //    //创建临时层
                //    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("查看选择");
                //    Table tblTemp = Cat.GetTable("查看选择");
                //    if (tblTemp != null)
                //    {
                //        Cat.CloseTable("查看选择");
                //    }

                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表_ID", 40));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("名称", 50));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表名", 50));

                //    tblTemp = Cat.CreateTable(tblInfoTemp);
                //    FeatureLayer lyr = new FeatureLayer(tblTemp);
                //    mapControl1.Map.Layers.Insert(0, lyr);

                //    //添加标注
                //    string activeMapLabel = "DistanceLabel";
                //    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("查看选择");
                //    MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //    MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //    lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                //    lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                //    lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                //    lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //    lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                //    lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //    lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                //    lbsource.DefaultLabelProperties.Caption = "名称";
                //    lblayer.Sources.Append(lbsource);
                //    mapControl1.Map.Layers.Add(lblayer);
                //}
                //else　　// 不为空则清楚之前的点
                //{
                //    clearFeatures("查看选择");
                //}

                //在VideoLayer中选择周边的视频，并添加到tempvideo

                double x1, x2;
                double y1, y2;
                double x, y;

                double dBufferDis = dis / 111000;
                x = dpt.x;
                y = dpt.y;
                x1 = x - dBufferDis;
                x2 = x + dBufferDis;
                y1 = y - dBufferDis;
                y2 = y + dBufferDis;


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrvideo = mapControl1.Map.Layers["查看选择"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblvideo = MapInfo.Engine.Session.Current.Catalog.GetTable("查看选择");

                //string[] camidarr = null;
                string sql = string.Empty;       // sql语句
                string objId = string.Empty;　　 // 表的ID值
                string objName = string.Empty;　 // 表的名称值　
                string tableName = string.Empty; // 表名

                CLC.ForSDGA.GetFromTable.GetFromName(tabAils, getfrompath);
                tableName = CLC.ForSDGA.GetFromTable.TableName;
                objId = CLC.ForSDGA.GetFromTable.ObjID;
                objName = CLC.ForSDGA.GetFromTable.ObjName;

                if (strRegion == string.Empty)   //add by fisher in 09-12-08
                {
                    MessageBox.Show(@"您没有设置权限！", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (strRegion == "顺德区")
                {
                    if (tableName == "视频位置")
                    {
                        sql = "Select * from 视频位置VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                        tableName = "视频";
                    }
                    else
                    {
                        sql = "Select * from " + tableName + " where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                    }
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1 && strRegion.IndexOf("德胜") < 0)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    if (tableName == "视频位置")
                    {
                        sql = "Select * from 视频位置VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        tableName = "视频";
                    }
                    else
                    {
                        sql = "Select * from " + tableName + " where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }

                DataTable dt = GetTable(sql);
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        foreach (DataRow dr in dt.Rows)
                        {

                            string camid = dr[objId].ToString();
                            if (camid != "" && dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr[objId].ToString() != "")
                            {
                                FeatureGeometry pt = new MapInfo.Geometry.Point(lyrvideo.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                                CompositeStyle cs = new CompositeStyle();

                                cs = new CompositeStyle(new BitmapPointStyle(CLC.ForSDGA.GetFromTable.BmpName, BitmapStyles.ApplyColor, System.Drawing.Color.Red, 12));
                                Feature ftr = new Feature(tblvideo.TableInfo.Columns);
                                ftr.Geometry = pt;
                                ftr.Style = cs;
                                ftr["表_ID"] = dr[objId].ToString();
                                ftr["名称"] = dr[objName].ToString();
                                ftr["表名"] = tableName;
                                tblvideo.InsertFeature(ftr);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "SearchSomeDistance");
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SearchSomeDistance");
            }
        }

        /// <summary>
        /// 地图上画点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="idname">报警编号</param>
        /// <param name="kkname">卡口编号</param>
        /// <param name="CarName">车辆编号</param>
        private void InsertSearchPoint(string idname, string kkname, string CarName)
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;

                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");


                double Cx = 0;
                double Cy = 0;

                //////获取

                DataTable dt1 = GetTable("select X,Y from 治安卡口系统 where 卡口编号='" + kkname + "'");

                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow dr1 in dt1.Rows)
                    {
                        Cx = Convert.ToDouble(dr1["X"]);
                        Cy = Convert.ToDouble(dr1["Y"]);
                    }
                }


                if (Cx != 0 && Cy != 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Cx, Cy)) as FeatureGeometry;
                    CompositeStyle cs = new CompositeStyle();
                    cs.ApplyStyle(new BitmapPointStyle("PIN1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 24));

                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                    ftr.Geometry = pt;
                    ftr.Style = cs;
                    ftr["Name"] = CarName;
                    ftr["ID"] = idname;
                    tblcar.InsertFeature(ftr);

                    string tempname = idname;

                    string tblname = "KKSearchLayer";


                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    rsfcflash = null;

                    MapInfo.Data.MIConnection conn = new MIConnection();

                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    conn.Open();

                    MapInfo.Data.MICommand cmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where ID = @name ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@name", tempname);

                    this.rsfcflash = cmd.ExecuteFeatureCollection();
                    if (this.rsfcflash.Count > 0)
                    {
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                        foreach (Feature f in this.rsfcflash)
                        {
                            mapControl1.Map.Center = f.Geometry.Centroid;
                            break;
                        }
                        cmd.Clone();
                        this.timerFlash.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InsertSearchPoint");
            }
        }


        /// <summary>
        /// 单击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dgvres_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            MapInfo.Data.MIConnection conn = new MIConnection();
            try
            {
                DataGridView portQueryView = (DataGridView)sender;

                this.dgvres.Rows[e.RowIndex].Selected = true;
                string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                string tblname = "KKSearchLayer";

                MapInfo.Mapping.Map map = this.mapControl1.Map;

                if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                MapInfo.Geometry.CoordSys cSys = this.mapControl1.Map.GetDisplayCoordSys();

                getFeatureCollection(tempname, tblname);

                    string idname = string.Empty;
                    string kkname = String.Empty;
                    string CarName = string.Empty;
                    string serNum = string.Empty;

                    if (this.comboxTable.Text == "车辆通过信息")
                    {
                        //通行车辆编号,卡口编号,卡口名称,通过时间,车辆号牌,号牌种类,车身颜色,颜色深浅,行驶方向,照片1,照片2,照片3
                        int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // 最后一列的位置
                        idname = portQueryView.CurrentRow.Cells[0].Value.ToString();
                        kkname = portQueryView.CurrentRow.Cells[1].Value.ToString();
                        CarName = portQueryView.CurrentRow.Cells[4].Value.ToString();
                        serNum = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // 序列号


                        DataRow[] row = dataKaKou.Select("序列号='" + serNum + "'");
                        picturePoint(row[0]);                                              // 创建新的编号

                        getFeatureCollection(serNum, tblname);
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                break;
                            }
                            this.timerFlash.Enabled = true;
                        }

                    }
                    else if (this.comboxTable.Text == "车辆报警信息")
                    {
                        //报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3
                        idname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                        kkname = portQueryView.CurrentRow.Cells[2].Value.ToString();
                        CarName = portQueryView.CurrentRow.Cells[5].Value.ToString();

                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                break;
                            }
                            this.timerFlash.Enabled = true;
                        }

                        this.InsertSearchPoint(idname, kkname, CarName);
                    }
                    else if (this.comboxTable.Text == "治安卡口信息")
                    {
                        //卡口编号,卡口名称,安装地点,监控点接壤地区,卡口对应车道数,监控车道数,所属派出所,监控方向,联系人,联系方式,来源 as 数据来源

                        idname = portQueryView.CurrentRow.Cells[0].Value.ToString();
                        CarName = portQueryView.CurrentRow.Cells[1].Value.ToString();
                        kkname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                break;
                            }
                            this.timerFlash.Enabled = true;
                        }

                        this.InsertSearchPoint(idname, kkname, CarName);
                    }

                //}
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvres_CellClick-41-单击事件");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 双击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dgvres_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

            try
            {
                DataGridView portQueryView = (DataGridView)sender;
                string strSQL = string.Empty;
                DataTable datatable = new DataTable();

                if (this.comboxTable.Text == "车辆通过信息")
                {
                    string[] sqlFields ={ "通行车辆编号", "卡口编号", "卡口名称", "通过时间", "车辆号牌", "号牌种类", "车身颜色", "颜色深浅", "行驶方向", "照片1", "照片2", "照片3" };

                    datatable = new DataTable("TemData");
                    for (int i = 0; i < sqlFields.Length; i++)
                    {
                        DataColumn dc = new DataColumn(sqlFields[i]);
                        if (i == 3)
                        {
                            dc.DataType = System.Type.GetType("System.DateTime");
                        }
                        else
                        {
                            dc.DataType = System.Type.GetType("System.String");
                        }
                        datatable.Columns.Add(dc);
                    }

                    DataRow dr = datatable.NewRow();
                    for (int i = 0; i < sqlFields.Length; i++)
                    {
                        dr[i] = portQueryView.CurrentRow.Cells[sqlFields[i]].Value.ToString();
                    }
                    datatable.Rows.Add(dr);

                    /////////根据当前通行车辆编号判断图片服务器地址
                    try
                    {
                        if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "01")   //高德威视频网图片服务器地址
                        {
                            photoserver = gdwserver;
                        }
                        else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "02") //易华录视频网图片服务器地址
                        {
                            photoserver = ehlserver;
                        }
                        else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "03")  //宝康视频网图片服务器地址
                        {
                            photoserver = bkserver;
                        }
                        else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().IndexOf("lbschina") > -1)
                        {
                            photoserver = "http://192.168.0.50/";
                        }
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "车辆通过信息根据ip地址获取图片服务器地址");
                    }

                    WriteEditLog(strSQL, "查看车辆经过信息", "V_CROSS");
                }
                else if (this.comboxTable.Text == "车辆报警信息")
                {

                    string sqlFields = "报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3,处理状态,处理人,处理时间,处理情况";
                    strSQL = "select " + sqlFields + " from V_ALARM t where 报警编号='" + portQueryView.CurrentRow.Cells[0].Value.ToString() + "'";
                    datatable = GetTable(strSQL);
                    WriteEditLog(strSQL, "查看车辆报警信息", "V_ALARM");

                }
                else if (this.comboxTable.Text == "治安卡口信息")
                {
                    strSQL = "select 卡口编号,卡口名称,安装地点,监控点接壤地区,卡口对应车道数,监控车道数,所属派出所,监控方向,联系人,联系方式 from 治安卡口系统 where 卡口编号='" + portQueryView.CurrentRow.Cells[0].Value.ToString() + "'";
                    datatable = GetTable(strSQL);
                    WriteEditLog(strSQL, "查看治安卡口信息", "治安卡口系统");
                }

                DPoint dp = new DPoint();
                if (datatable != null && datatable.Rows.Count > 0)
                {
                    //////////////////////////////////////////
                    string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                    if (this.comboxTable.Text == "车辆通过信息")
                    {
                        int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // 最后一列的位置
                        tempname = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // 序列号
                    }

                    string tblname = "KKSearchLayer";

                    //提取当前选择的信息的通行车辆编号作为主键值

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    try
                    {
                        getFeatureCollection(tempname, tblname);
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }
                            //cmd.Clone();
                            this.timerFlash.Enabled = true;
                        }
                    }
                    catch (Exception ex) { writeToLog(ex, "在地图上搜索对应信息时发生错误"); }
                    /////////////////////////////////////////

                    if (dp.x == 0 || dp.y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("此对象未定位!");
                        return;
                    }

                    System.Drawing.Point pt = new System.Drawing.Point();
                    mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvres_CellDoubleClick-42-双击事件");
            }
            finally
            {
                try { fmDis.Close(); }
                catch { }
            }
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (this.ALARMSYS == "系统")
        //        {
        //            this.timalarm.Enabled = true;
        //        }
        //        else if (this.ALARMSYS == "模块")
        //        {
        //            if (this.Visible == true)
        //            {
        //                this.timalarm.Enabled = true;
        //            }
        //            else
        //            {
        //                this.timalarm.Enabled = false;
        //                this.ftip.Visible = false;
        //            }
        //        }
        //        else
        //        {
        //            this.timalarm.Enabled = false;
        //            this.ftip.Visible = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        writeToLog(ex, "timer1_Tick");
        //    }
        //}

        /// <summary>
        /// 初始化设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            InitAlarmSet();
        }
        
        /// <summary>
        /// 保存设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.rdboxsys.Checked == true)
                {
                    this.ALARMSYS = "系统";
                }
                else if (this.rdboxmod.Checked == true)
                {
                    this.ALARMSYS = "模块";
                }

                if (this.rdboxall.Checked == true)
                {
                    this.ALARMUSER = "所有";
                }
                else if (this.rdboxuser.Checked == true)
                {
                    this.ALARMUSER = "用户";
                }

                ftip.AlarmSys = this.ALARMSYS;
                ftip.AlarmUser = this.ALARMUSER;

                this.SCHDIS = Convert.ToDouble(this.txtdist.Text.Trim());

                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
             
                CLC.INIClass.IniWriteValue("治安卡口", "系统报警", ALARMSYS);
                CLC.INIClass.IniWriteValue("治安卡口", "报警对象", ALARMUSER);
                CLC.INIClass.IniWriteValue("治安卡口", "查询半径", this.txtdist.Text.Trim());

                MessageBox.Show(@"保存设置成功，重启程序后该修改才能生效",@"系统提示",MessageBoxButtons.OK,MessageBoxIcon.Information);

                WriteEditLog(ALARMSYS + ":" + ALARMUSER + ":" + this.txtdist.Text.Trim(), "更改卡口设置", "配置文件");
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"保存设置失败", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                writeToLog(ex, "btnOK_Click-43-保存设置");
            }                 
        }


       /// <summary>
        /// 窗体的隐藏和显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
       /// </summary>
        private void ucKakou_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.ALARMSYS == "系统")
                {
                    ftip.timalarm.Enabled = true;
                }
                else if (this.ALARMSYS == "模块")
                {
                    if (this.Visible == true)
                    {
                        ftip.timalarm.Enabled = true;
                    }
                    else
                    {
                        ftip.timalarm.Enabled = false;
                        ftip.Visible = false;
                    }
                }
                else
                {
                    ftip.timalarm.Enabled = false;
                    ftip.Visible = false;
                }


                if (this.Visible == false)
                {
                    this.ClearKaKou();
                    ftip.isKakou = false;
                }
                else
                {
                    FeatureLayer fl = mapControl1.Map.Layers["查看选择"] as FeatureLayer;
                    if (fl != null)
                    {
                        labeLayer(fl.Table, "名称");
                    }

                    ftip.isKakou = true;
                }
                isDel = false;
                this.dateFrom.Value = DateTime.Now.Date;
                this.dateTo.Value = DateTime.Now.Date;
                this.panError.Visible = false;    // 隐藏错误提示
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucKakou_VisibleChanged-44-窗体的隐藏和显示");
            }
        }

        /// <summary>
        /// 创建标注
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="editTable">图层名称</param>
        /// <param name="field">标注字段</param>
        private void labeLayer(Table editTable, string field)
        {
            try
            {
                LabelLayer labelLayer = mapControl1.Map.Layers["标注图层"] as LabelLayer;

                LabelSource source = new LabelSource(editTable);

                source.DefaultLabelProperties.Caption = field;
                source.DefaultLabelProperties.Layout.Offset = 4;
                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;
                //source.DefaultLabelProperties.Visibility.VisibleRangeEnabled = true;
                //source.DefaultLabelProperties.Visibility.VisibleRange = new VisibleRange(0.0, 10, DistanceUnit.Kilometer);

                labelLayer.Sources.Insert(0, source);
            }

            catch (Exception ex)
            {
                writeToLog(ex, "labeLayer-45-创建标注");
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
        /// 分页初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="lblcount">显示总记录控件</param>
        /// <param name="textNowPage">显示当前页数控件</param>
        /// <param name="lblPageCount">显示总页数控件</param>
        /// <param name="bs">数据源控件</param>
        /// <param name="bn">分页控件</param>
        /// <param name="dgv">显示列表控件</param>
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

                LoadData1(textNowPage,lblPageCount,bs,bn,dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "47-InitDataSet1");
            }
        }

        /// <summary>
        /// 查询数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="textNowPage">显示当前页数控件</param>
        /// <param name="lblPageCount">显示总页数控件</param>
        /// <param name="bds">数据源控件</param>
        /// <param name="bdn">分页控件</param>
        /// <param name="dgv">显示列表控件</param>
        public void LoadData1(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
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
                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt1.Rows[i]);
                    PagenCurrent1++;
                }
                dataKaKou = new DataTable();
                bds.DataSource = dataKaKou = dtTemp;
                
                CreateKakouTrack(dtTemp);

                bdn.BindingSource = bds;
                dgv.DataSource = bds;
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "48-LoadData1");
            }
         }


        /// <summary>
        /// 页面导航
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "上一页")
                {
                    pageCurrent1--;
                    if (pageCurrent1 <1)
                    {
                        pageCurrent1 = 1;
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！");
                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }

                    LoadData1(PageNow1,PageNum1,bindingSource1 ,bindingNavigator1,dataGridViewKakou);
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
                    LoadData1(PageNow1,PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
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
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
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
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                }
                else if (e.ClickedItem.Text == "数据导出")
                {
                    DataExport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "bindingNavigator1_ItemClicked-49-页面导航");
            }
        }

       
        public DataTable _exportDT = null;
        /// <summary>
        /// 卡口数据导出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void DataExport()
        {
            try
            {
                if (_exportDT != null)
                {

                    FolderBrowserDialog sfd = new FolderBrowserDialog();
                    sfd.ShowNewFolderButton =true;
                    //sfd.FileName = "EXP" + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        DataView dataview = _exportDT.DefaultView;
                        DataTable dt = dataview.ToTable(true, "卡口名称");
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {

                                string KKnamet = dr["卡口名称"].ToString();

                                DataRow[] drp = _exportDT.Select("卡口名称='" + KKnamet + "'");
                                DataTable dtt = _exportDT.Clone();
                                for (int i = 0; i < drp.Length; i++)
                                {
                                    dtt.Rows.Add(drp[i].ItemArray);
                                }
                                string StoreName = sfd.SelectedPath + @"\" + KKnamet + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);

                                if (Directory.Exists(StoreName)==false)
                                {
                                    Directory.CreateDirectory(StoreName);                                    
                                } 

                                ExportExcel(dtt,StoreName + @"\" + KKnamet + ".xls");
                                ExportPic(dtt, StoreName);
                            }

                            MessageBox.Show("导出完成");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "50-DataExport");

            }

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 卡口图片导出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="StoreName">导出图片地址</param>
        private void ExportPic(DataTable dt, string StoreName)
        {
            try
            {
                if (dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                    {
                        /////////根据当前通行车辆编号判断图片服务器地址
                        try
                        {
                            if (dr[0].ToString().Substring(2, 2) == "01")   //高德威视频网图片服务器地址
                            {
                                photoserver = gdwserver;
                            }
                            else if (dr[0].ToString().Substring(2, 2) == "02") //易华录视频网图片服务器地址
                            {
                                photoserver = ehlserver;
                            }
                            else if (dr[0].ToString().Substring(2, 2) == "03")  //宝康视频网图片服务器地址
                            {
                                photoserver = bkserver;
                            }
                            else if (dr[0].ToString().IndexOf("lbschina") > -1)
                            {
                                photoserver = "http://192.168.0.50/";
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "clKaKou-ucKakou-20-车辆通过信息根据ip地址获取图片服务器地址");
                        }

                        if (dr["照片1"].ToString() != "")
                            SavePic(photoserver + dr["照片1"].ToString(), StoreName);

                        if (dr["照片2"].ToString() != "")
                            SavePic(photoserver + dr["照片2"].ToString(), StoreName);

                        if (dr["照片3"].ToString() != "")
                            SavePic(photoserver + dr["照片3"].ToString(), StoreName);
                    }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "50-ExportPic");
            }
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="filename">图片地址</param>
        /// <param name="storename">保存地址</param>
        private void SavePic(string filename, string storename)
        {
            try
            {
                if (filename != "")
                {
                    //为了防止以前的数据中存在不规则字符或者ftp服务器地址或者http服务器地址出现错误，在此进行错误字符Replace。
                    if (filename.IndexOf("\\") > 0)
                    {
                        filename = filename.Replace("\\", "/");
                    }

                    string filen = filename.Substring(filename.LastIndexOf('/') + 1, filename.Length - filename.LastIndexOf('/') - 1); //取得文件名

                    System.Net.WebClient client = new WebClient();
                   
                    client.DownloadFile(filename, storename + "\\" + filen);
                    client = null;                   
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "50-SavePic:"+filename);
            }

        }

        /// <summary>
        /// 卡口导出Excel
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="fileName">地址</param>
        private void ExportExcel(DataTable dt, string fileName) 
        {
            try
            {      
                    LBSDataGuide.DataGuide dg = new LBSDataGuide.DataGuide();
             
                    this.Cursor = Cursors.WaitCursor;

                    dg.OutData(fileName, dt, "治安卡口系统");        
            }
            catch(Exception ex)
            {
                this.writeToLog(ex,"ExportExcel");
            }
            
        }


        //==========
        //==========
        //翻页功能2
        //==========
        //==========

        int pageSize2 = 100;     //每页显示行数
        int PagenMax2 = 0;         //总记录数
        int pageCount2 = 0;    //页数＝总记录数/每页显示行数
        int pageCurrent2 = 0;   //当前页号
        int PagenCurrent2 = 0;      //当前记录行 
        DataSet Pageds2 = new DataSet();
        DataTable Pagedt2 = new DataTable();

        /// <summary>
        /// 分页初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="lblcount">显示总记录控件</param>
        /// <param name="textNowPage">显示当前页数控件</param>
        /// <param name="lblPageCount">显示总页数控件</param>
        /// <param name="bs">数据源控件</param>
        /// <param name="bn">分页控件</param>
        /// <param name="dgv">显示列表控件</param>
        public void InitDataSet2(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize2 = 100;      //设置页面行数
                PagenMax2 = Pagedt2.Rows.Count;

                lblcount.Text = "共" + PagenMax2.ToString() + "条记录";//在导航栏上显示总记录数

                pageCount2 = (PagenMax2 / pageSize2);//计算出总页数
                if ((PagenMax2 % pageSize2) > 0) pageCount2++;
                if (PagenMax2 != 0)
                {
                    pageCurrent2 = 1;
                }
                PagenCurrent2 = 0;       //当前记录数从0开始

                LoadData2(textNowPage,lblPageCount,bs, bn, dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "52-InitDataSet2");
            }
        }

        /// <summary>
        /// 查询数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="textNowPage">显示当前页数控件</param>
        /// <param name="lblPageCount">显示总页数控件</param>
        /// <param name="bds">数据源控件</param>
        /// <param name="bdn">分页控件</param>
        /// <param name="dgv">显示列表控件</param>
        public void LoadData2(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = Pagedt2.Clone();

                if (pageCurrent2 == pageCount2)
                    nEndPos = PagenMax2;
                else
                    nEndPos = pageSize2 * pageCurrent2;
                nStartPos = PagenCurrent2;

                //tsl.Text = Convert.ToString(pageCurrent2) + "/" + pageCount2.ToString();
                textNowPage.Text = Convert.ToString(pageCurrent2);
                lblPageCount.Text = "/" + pageCount2.ToString();

                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt2.Rows[i]);
                    PagenCurrent2++;
                }
                dataKaKou = new DataTable();
                bds.DataSource = dataKaKou = dtTemp;

                if (this.comboxTable.Text == "车辆通过信息")  // 车辆通过信息的画点连线
                {
                    CreateKakouTrack(dtTemp);
                }

                bdn.BindingSource = bds;
                dgv.DataSource = bds;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "53-LoadData2");
            }
        }

        /// <summary>
        /// 页面导航
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void bindingNavigator2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "上一页")
                {
                    pageCurrent2--;
                    if (pageCurrent2 <1)
                    {
                        pageCurrent2 = 1;
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！");
                        return;
                    }
                    else
                    {
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    }

                    LoadData2(PageNow2,PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
                if (e.ClickedItem.Text == "下一页")
                {
                    pageCurrent2++;
                    if (pageCurrent2 > pageCount2)
                    {
                        pageCurrent2 = pageCount2;
                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！");
                        return;
                    }
                    else
                    {
                        PagenCurrent2 = pageSize2 * (pageCurrent2- 1);
                    }
                    LoadData2(PageNow2,PageNum2,bindingSource2, bindingNavigator2, dgvres);
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (pageCurrent2 <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent2 = 1;
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    }
                    LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
                else if (e.ClickedItem.Text == "转到尾页")
                {
                    if (pageCurrent2 > pageCount2 - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent2 = pageCount2;
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    }
                    LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
                else if (e.ClickedItem.Text == "数据导出")
                {
                    DataExport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "54-bindingNavigator2_ItemClicked");
            }
        }

        /// <summary>
        /// 取消卡口设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void button8_Click(object sender, EventArgs e)
        {
            InitAlarmSet();
        }

        /// <summary>
        /// 数据导出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnDateOut_Click(object sender, EventArgs e)
        {
            DataExport();
        }

        /// <summary>
        /// 设置每页显示的数据量
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridViewKakou.Rows.Count > 0)
                {
                    pageSize1 = Convert.ToInt32(TextNum1.Text);
                    pageCurrent1 = 1;   //当前转到第一页
                    PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    pageCount1 = (PagenMax1 / pageSize1);//计算出总页数
                    if ((PagenMax1 % pageSize1) > 0) pageCount1++;

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                }
            }
            catch(Exception ex)            
            {
                writeToLog(ex, "55-TextNum1_KeyPress");
            }
        }

        /// <summary>
        /// 页面转向
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
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
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "56-PageNow1_KeyPress");
            }
        }

        /// <summary>
        /// 导出车辆通过信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnDataOut1_Click(object sender, EventArgs e)
        {
            if (this.comboxTable.Text == "车辆通过信息")  //V_Cross
            {
                DataExport();
            }
        }

        /// <summary>
        /// 设置每页显示的数据量
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void PageNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && this.dgvres.Rows.Count > 0)
                {
                    pageSize2 = Convert.ToInt32(this.PageNumber.Text);
                    pageCurrent2 = 1;   //当前转到第一页
                    PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    pageCount2 = (PagenMax2 / pageSize2);//计算出总页数
                    if ((PagenMax2 % pageSize2) > 0) pageCount2++;

                    LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "57-PageNumber_KeyPress");
            }
        }

        /// <summary>
        /// 页面转向
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void PageNow2_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(this.PageNow2.Text) < 1 || Convert.ToInt32(this.PageNow2.Text) > pageCount2)
                    {
                        System.Windows.Forms.MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow2.Text = pageCurrent2.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent2 = Convert.ToInt32(this.PageNow2.Text);
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                        LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "58-PageNow2_KeyPress");
            }
        }

        /// <summary>
        /// 切换Tab
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.dataGridViewKakou.DataSource = null;
                this.dgvres.DataSource = null;
                this.chkmh.Checked = false;       // 去掉模糊查询
                this.panError.Visible = false;    // 隐藏错误提示
                this.comboxCon.SelectedIndex = 0; // 切换模块时还原条件为‘等于’
                this.fuzzyFlag = false;           // 用于判断是否是模糊查询的bool值改为false
                dataKaKou = null;                

                this.dateFrom.Value = DateTime.Now.Date;
                this.dateTo.Value = DateTime.Now.Date;

                if (tabControl1.SelectedTab == tabPage1)   //  车辆查询
                {
                    if (this.groupBox1.Visible)
                        this.linkLabel1.Text = "隐藏条件栏";
                    else
                        this.linkLabel1.Text = "显示条件栏";

                    this.linkLabel1.Visible = true;
                }
                if (tabControl1.SelectedTab == tabPage2)   //  指挥及设置 
                {
                    this.linkLabel1.Visible = false;
                }
                if (tabControl1.SelectedTab == tabPage3)   //  组合查询
                {
                    if (this.groupBox4.Visible)
                        this.linkLabel1.Text = "隐藏条件栏";
                    else
                        this.linkLabel1.Text = "显示条件栏";

                    this.linkLabel1.Visible = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "tabControl1_SelectedIndexChanged-61-切换Tab");
            }
        }

        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
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
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }
       
        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clKaKou-ucKakou-" + sFunc);
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">当前操作的SQL</param>
        /// <param name="method">操作方式</param>
        /// <param name="tablename">操作表名</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'治安卡口','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch(Exception ex)
            {
                writeToLog(ex, "WriteEditLog-61-记录操作记录");
            }           
        }

        /// <summary>
        /// 自动补全功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboxField.Text == "卡口名称" || comboxField.Text == "卡口编号" || comboxField.Text == "报警卡口名称" || comboxField.Text == "报警卡口编号")
                {
                    string keyword = this.CaseSearchBox.Text.Trim();
                    string colword = string.Empty;

                    if (comboxField.Text.IndexOf("卡口名称") > -1)
                    {
                        colword = "卡口名称";
                    }
                    else if (comboxField.Text.IndexOf("卡口编号") > -1)
                    {
                        colword = "卡口编号";
                    }

                    if (keyword != "" && colword != "")
                    {
                        string strExp = "select distinct(" + colword + ") from 治安卡口系统 t where " + colword + " like '%" + keyword + "%'  order by "+ colword;
                        DataTable dt = GetTable(strExp);
                        this.CaseSearchBox.GetSpellBoxSource(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CaseSearchBox_TextChanged-59");
            }
        }

        /// <summary>
        /// 自动补全及全角半角处理
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            IntPtr HIme = ImmGetContext(this.Handle);
            if (ImmGetOpenStatus(HIme)) //如果输入法处于打开状态
            {
                int iMode = 0;
                int iSentence = 0;
                bool bSuccess = ImmGetConversionStatus(HIme, ref iMode, ref iSentence); //检索输入法信息
                if (bSuccess)
                {
                    if ((iMode & IME_CMODE_FULLSHAPE) > 0) //如果是全角
                        ImmSimulateHotKey(this.Handle, IME_CHOTKEY_SHAPE_TOGGLE); //转换成半角
                }

            }
            
            
            try
            {
                string colfield = this.comboxField.Text.Trim();
                string strExp = string.Empty;

                if(colfield == "所属派出所")
                {
                    strExp = "select distinct(派出所名) from 基层派出所 order by 派出所名";
                  
                }
                else if (colfield == "来源")
                {
                    strExp = "select distinct(来源) from 治安卡口系统 order by 来源";
                }

                DataTable dt = GetTable(strExp);
                if (dt.Rows.Count > 0)
                    this.CaseSearchBox.GetSpellBoxSource(dt);
                else
                    this.CaseSearchBox.GetSpellBoxSource(null);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CaseSearchBox_MouseDown-35-点击显示下拉");
            }
        }

        /// <summary>
        /// 显示或隐藏进度条
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="falg">布尔值（true-显示 false-隐藏）</param>
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
                writeToLog(ex, "isShowPro-35-点击显示下拉");
            }
        }

        /// <summary>
        /// 隐藏或显示条件栏链接
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.tabControl1.SelectedTab == this.tabPage1)    //  车辆查询
                    HidesCondition(sender, e, this.groupBox1, null);
                if (this.tabControl1.SelectedTab == this.tabPage3)    //  组合查询
                    HidesCondition(sender, e, this.groupBox4, this.CaseSearchBox);
                if (this.tabControl1.SelectedTab == this.tabPage2)    // 指挥及设置 
                    return;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "linkLabel1_LinkClicked-35-点击显示下拉");
            }
        }

        /// <summary>
        /// 隐藏或显示条件栏
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="groupBox">要隐藏或显示的控件</param>
        /// <param name="text">要隐藏或显示的控件</param>
        private void HidesCondition(object sender, LinkLabelLinkClickedEventArgs e, System.Windows.Forms.GroupBox groupBox, SplitWord.SpellSearchBoxEx text)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;
                link.Visible = true;

                if (link.Text == "隐藏条件栏")
                {
                    if (text != null)
                    {
                        text.Visible = false;
                    }
                    groupBox.Visible = false;
                    link.Text = "显示条件栏";
                }
                else
                {
                    if (text != null)
                    {
                        text.Visible = true;
                    }
                    groupBox.Visible = true;
                    link.Text = "隐藏条件栏";
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "HidesCondition");
            }
        }

        /// <summary>
        /// 全角半角处理
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void txtBoxCar_MouseDown(object sender, MouseEventArgs e)
        {
            IntPtr HIme = ImmGetContext(this.Handle);
            if (ImmGetOpenStatus(HIme)) //如果输入法处于打开状态
            {
                int iMode = 0;
                int iSentence = 0;
                bool bSuccess = ImmGetConversionStatus(HIme, ref iMode, ref iSentence); //检索输入法信息
                if (bSuccess)
                {
                    if ((iMode & IME_CMODE_FULLSHAPE) > 0) //如果是全角
                        ImmSimulateHotKey(this.Handle, IME_CHOTKEY_SHAPE_TOGGLE); //转换成半角
                }

            }

        }

        /// <summary>
        /// 鼠标伸到显示条件提示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void txtBoxCar_MouseEnter(object sender, EventArgs e)
        {
            this.groupBox3.Visible = true;
        }

        /// <summary>
        /// 鼠标移开隐藏条件提示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void txtBoxCar_MouseLeave(object sender, EventArgs e)
        {
            this.groupBox3.Visible = false;
        }

        /// <summary>
        /// 测试日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="funName">修改编号</param>
        /// <param name="id">添加序列号</param>
        private void WriteLog(string funName,string id)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\TestLog.txt";
                strWri = new StreamWriter(exePath, true);
                if (funName != null)
                    strWri.WriteLine("修改编号：  " + funName);
                if (id != null)
                    strWri.WriteLine("添加序列号： " + id);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "WriteLog");
            }
        }

        private DataTable dataKaKou;             // 用于显示放大数据的内存表
        private clPopu.frmDisplay fmDis;

        /// <summary>
        /// 车辆查询 放大数据按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnEnal_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataKaKou == null)
                {
                    System.Windows.Forms.MessageBox.Show("无数据展示，请查询出数据后放大查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new clPopu.frmDisplay(dataKaKou);

                fmDis.dataGridDisplay.CellClick += this.dataGridViewKakou_CellClick;               // 绑定单击事件
                fmDis.dataGridDisplay.CellDoubleClick += this.dataGridViewKakou_CellDoubleClick;   // 绑定双击事件

                fmDis.Show();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnEnal_Click");
            }
        }

        /// <summary>
        /// 组合查询 放大数据按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnEnalData_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataKaKou == null)
                {
                    System.Windows.Forms.MessageBox.Show("无数据展示，请查询出数据后放大查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new clPopu.frmDisplay(dataKaKou);

                fmDis.dataGridDisplay.CellClick += this.dgvres_CellClick;               // 绑定单击事件
                fmDis.dataGridDisplay.CellDoubleClick += this.dgvres_CellDoubleClick;   // 绑定双击事件

                fmDis.Show();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnEnalData_Click");
            }     
        }

        //private clPopu.FrmHouseInfo frmMessage = new clPopu.FrmHouseInfo();
        private frmMessage frmMessage = new frmMessage();
        /// <summary>
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="rfc">地图要素</param>
        /// <param name="po">显示位置</param>
        public void getMessage(IResultSetFeatureCollection rfc,System.Drawing.Point po)
        {
            try
            {

                if (rfc.Count > 0)
                {
                    if (this.frmMessage.Visible == false)
                    {
                        this.frmMessage = new frmMessage();
                        this.frmMessage.SetDesktopLocation(-30, -30);
                        this.frmMessage.Show();
                        this.frmMessage.Visible = false;
                    }

                    this.frmMessage.getFromNamePath = this.getfrompath;
                    this.frmMessage.mapControl = mapControl1;
                    this.frmMessage.LayerName = "查看选择";
                    this.frmMessage.setInfo(rfc[0], StrCon);
                    this.frmMessage.Show();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnEnalData_Click");
            } 
        }

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ftr">Feature集合</param>
        /// <param name="myconStr">数据库连接字符串</param>
        /// <returns></returns>
        private DataRow featureStyle(IResultSetFeatureCollection ftr, string myconStr)
        {
            #region
            //try
            //{
            //    //通过查看字段,如果含"表_ID",说明是临时表
            //    bool isTemTab = false;
            //    foreach (MapInfo.Data.Column col in ftr.Columns)
            //    {
            //        String upAlias = col.Alias.ToUpper();
            //        if (upAlias.IndexOf("表_ID") > -1)
            //        {
            //            isTemTab = true;
            //            break;
            //        }
            //    }
            //    DataTable dt = null;
            //    DataRow row = null;
            //    if (isTemTab)
            //    {
            //        string strTabName = ftr["表名"].ToString();
            //        if (strTabName.IndexOf("视频") >= 0)
            //        {
            //            strTabName = "视频";
            //        }
            //        CLC.ForSDGA.GetFromTable.GetFromName(strTabName, getfrompath);

            //        string strSQL1 = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr["表_ID"].ToString() + "'";
            //        if (CLC.ForSDGA.GetFromTable.TableName == "安全防护单位")
            //            strSQL1 = "select 编号,单位名称,单位性质,单位地址,主管保卫工作负责人,手机号码,所属派出所,所属中队,所属警务室,'点击查看' as 安全防护单位文件,标注人,标注时间,X,Y from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr["表_ID"].ToString() + "'";

            //        OracleConnection Conn = new OracleConnection(myconStr);
            //        try
            //        {
            //            Conn.Open();
            //            OracleCommand cmd = new OracleCommand(strSQL1, Conn);
            //            cmd.ExecuteNonQuery();
            //            OracleDataAdapter apt = new OracleDataAdapter(cmd);
            //            dt = new DataTable();
            //            apt.Fill(dt);
            //            row = dt.Rows[0];
            //            cmd.Dispose();
            //            Conn.Close();
            //        }
            //        catch
            //        {
            //            if (Conn.State == ConnectionState.Open)
            //                Conn.Close();
            //            return null;
            //        }
            //    }
            //    return row;
            //}
            //catch (Exception ex)
            //{
            //    writeToLog(ex, "featureStyle");
            //    return null;
            //}
            #endregion

            try
            {
                if (ftr == null)
                {
                    MessageBox.Show("请您选择图标后重试！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return null;
                }
                bool isTemTab = false;  // 通过查看字段,如果含"表_ID",说明是临时表

                if (ftr != null)
                {
                    if (ftr.Count > 0)
                    {
                        foreach (MapInfo.Data.Column col in ftr[0].Columns)
                        {
                            String upAlias = col.Alias.ToUpper();
                            if (upAlias.IndexOf("表_ID") > -1)
                            {
                                isTemTab = true;
                                break;
                            }
                        }
                    }
                }
                DataTable dt = null;
                DataRow row = null;

                if (isTemTab)
                {
                    string strTabName = ftr[0]["表名"].ToString();
                    if (strTabName.IndexOf("视频") >= 0)
                    {
                        strTabName = "视频";
                    }
                    CLC.ForSDGA.GetFromTable.GetFromName(strTabName, getfrompath);

                    string strSQL1 = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr[0]["表_ID"].ToString() + "'";
                    if (CLC.ForSDGA.GetFromTable.TableName == "安全防护单位")
                        strSQL1 = "select 编号,单位名称,单位性质,单位地址,主管保卫工作负责人,手机号码,所属派出所,所属中队,所属警务室,'点击查看' as 安全防护单位文件,标注人,标注时间,X,Y from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr[0]["表_ID"].ToString() + "'";

                    OracleConnection Conn = new OracleConnection(myconStr);
                    try
                    {
                        Conn.Open();
                        OracleCommand cmd = new OracleCommand(strSQL1, Conn);
                        cmd.ExecuteNonQuery();
                        OracleDataAdapter apt = new OracleDataAdapter(cmd);
                        dt = new DataTable();
                        apt.Fill(dt);
                        row = dt.Rows[0];
                        cmd.Dispose();
                        Conn.Close();
                    }
                    catch
                    {
                        if (Conn.State == ConnectionState.Open)
                            Conn.Close();
                        return null;
                    }
                }
                return row;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "featureStyle");
                return null;
            }
        }
    }
}
