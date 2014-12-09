using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Data.OracleClient;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;

using Crownwood.DotNetMagic.Common;
using Crownwood.DotNetMagic.Docking;
using clUser;

using LBSDataGuide;

namespace LBSgisPolice
{
    public partial class frmMap : Form
    {
        private frmLayer fLayer;

        private clCar.ucCar fCar = null;                           // GPS警车
        private clVideo.ucVideo fVideo = null;                     // 视频监控 jie.zhang 2008.9.19
        private clZonghe.ucZonghe fZonghe = null;                  // 综合查询
        private clAnjian.ucAnjian fAnjian = null;                  // 案件分析
        private clPopu.ucPopu fPopu = null;                        // 人口管理
        private clHouse.ucHouse fHouse = null;                     // 房屋管理
        private clZhihui.ucZhihui fZhihui = null;                  // 直观指挥
        private clKaKou.ucKakou fKakou = null;                     // 治安卡口 jie.zhang 20090709
        private cl110.uc110 f110 = null;                           // 110接警  jie.zhnag 20091230
        private clGPSPolice.UcGpsPolice fGPSp = null;              // GPS警员  jie.zhang 20091230
        private clGISPoliceEdit.ucGISPoliceEdit fGISEdit = null;   // 数据编辑 lili 20110107

        private string exportSQL;         // 存放导出的SQL语句
        private string exePath;           // 主程序路径
        private string GetFromNamePath;   // 存放GetFromNameConfig.ini配置文件的路径
        private string strConn;           // 连接字符串
        CompositeStyle comStyle = null;

        private int videop = 0;                            // 视频监控端通讯端口
        private string[] videoConnstring = new string[6];  // 视频监控连接字符串
        private string videoexepath = string.Empty;        // 视频监控端位置
        //private string fileIP = "";

        private string[] ConStr=new string[3] ;  // 数据库连接信息

        private Int32 _schnum;                   // 110信息处理方式
        private frmPro frmpro = new frmPro();    // 显示进度窗体
        private System.Windows.Forms.Label[] _majorTask;  // 显示重大任务的详细信息的文本控件

        private System.Windows.Forms.Label[] levelArr = new System.Windows.Forms.Label[7];

        /// <summary>
        /// 程序初始化
        /// 最后编辑人 李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        public frmMap()
        {
            try
            {
                try
                {
                    InitializeComponent();

                    reg110();  //注册110控件

                    frmpro.Show();
                    frmpro.progressBar1.Value = 0;
                    frmpro.progressBar1.Maximum = 12;
                    Application.DoEvents();
                }
                catch (System.ComponentModel.LicenseException ex)
                {
                    ExToLog(ex, "frmMap");
                    MessageBox.Show("地图控件未授权或者过期,系统不能启动!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    frmpro.Close();
                    this.Dispose();
                    return;
                }
                catch (Exception ex)
                {
                    writelog("控件不支持，请联系开发商" + ex.ToString());
                }

                exePath = Application.StartupPath;     //程序路径
                try
                {
                    frmpro.lblMessage.Text = "正在初始化测量窗体，请稍候．．．";
                    frmpro.progressBar1.Value = 1;
                    Application.DoEvents();
                    InitDocument();     //初始化测量窗体
                }
                catch (Exception ex) { ExToLog(ex, "01-初始化测量窗体"); }

                try
                {
                    frmpro.lblMessage.Text = "正在初始化地图，请稍候．．．";
                    frmpro.progressBar1.Value = 2;
                    Application.DoEvents();
                    InitialMap();      //初始化地图
                }
                catch (Exception ex) { ExToLog(ex, "02-初始化地图"); }

                try
                {
                    frmpro.lblMessage.Text = "正在读取配置文件，请稍候．．．";
                    frmpro.progressBar1.Value = 3;
                    Application.DoEvents();
                    strConn = getStrConn();  //数据库连接字符串
                    if (strConn == "")
                    {
                        MessageBox.Show("读取配置文件时发生错误,请修改配置文件后重试!");
                        return;
                    }
                }
                catch (Exception ex) { ExToLog(ex, "03-数据库连接字符串"); }

                try
                {
                    frmpro.lblMessage.Text = "正在创建临时层存放鹰眼的矩形框，请稍候．．．";
                    frmpro.progressBar1.Value = 4;
                    Application.DoEvents();
                    CreateEagleLayer();//创建临时层存放鹰眼的矩形框
                }
                catch (Exception ex) { ExToLog(ex, "04-创建临时层存放鹰眼"); }

                mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(mapControl1_ViewChanged);

                try
                {
                    frmpro.lblMessage.Text = "正在建立视频监控通讯端口，请稍候．．．";
                    frmpro.progressBar1.Value = 5;
                    Application.DoEvents();
                    CreateVideoSocket();//zhangjie 2008.12.17 建立视频监控通讯端口
                }
                catch (Exception ex) { ExToLog(ex, "05-建立视频监控通讯端口"); }

                //初始化级别数组
                levelArr[0] = level1;
                levelArr[1] = level2;
                levelArr[2] = level3;
                levelArr[3] = level4;
                levelArr[4] = level5;
                levelArr[5] = level6;
                levelArr[6] = level7;
                level1.Click += new System.EventHandler(this.level1_Click);
                level2.Click += new System.EventHandler(this.level2_Click);
                level3.Click += new System.EventHandler(this.level3_Click);
                level4.Click += new System.EventHandler(this.level4_Click);
                level5.Click += new System.EventHandler(this.level5_Click);
                level6.Click += new System.EventHandler(this.level6_Click);
                level7.Click += new System.EventHandler(this.level7_Click);

                frmpro.lblMessage.Text = "正在初始化用户信息，请稍候．．．";
                frmpro.progressBar1.Value = 7;
                Application.DoEvents();
                try
                {
                    setOnline(frmLogin.string用户名称, 1);//记录用户在线
                }
                catch (Exception ex) { ExToLog(ex, "06-记录用户在线"); }

                frmpro.lblMessage.Text = "正在初始化模块信息，请稍候．．．";
                frmpro.progressBar1.Value = 8;
                Application.DoEvents();
                try
                {
                    InitializeMenuItems();  //各模块初始化
                }
                catch (Exception ex) { ExToLog(ex, "07-各模块初始化"); }

                frmpro.lblMessage.Text = "正在初始化用户权限，请稍候．．．";
                frmpro.progressBar1.Value = 9;
                Application.DoEvents();
                try
                {
                    InitPrivilege();   //设置权限
                }
                catch (Exception ex) { ExToLog(ex, "08-用户权限"); }

                frmpro.lblMessage.Text = "正在初始化默认模块，请稍候．．．";
                frmpro.progressBar1.Value = 10;
                Application.DoEvents();
                try
                {
                    SetDefaultFuncItem();  //设置默认模块
                    //frm.Close();
                }
                catch (Exception ex) { ExToLog(ex, "09-默认模块"); }

                try
                {
                    frmpro.lblMessage.Text = "正在检测服务器，请稍候．．．";
                    frmpro.progressBar1.Value = 11;
                    Application.DoEvents();

                    //add by siumo 090121
                    checkServerComputer(); //siumo 090211 检测服务器

                    //timeIP.Interval = 1 * 60 * 1000;

                    frmpro.progressBar1.Value = 12;
                    frmpro.lblMessage.Text = "程序初始化完成";
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(300);
                    
                    //timeIP.Start();
                }
                catch (Exception ex) { ExToLog(ex, "10-检测服务器"); }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "11-程序初始化");
            }
            finally
            {
                frmpro.Close();
            }
        }

        /// <summary>
        /// 注册netcomm.ocx控件
        /// 最后编辑人 李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        private void reg110()
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "regsvr32";
                p.StartInfo.Arguments = "/s " + Application.StartupPath + "\\netcomm.ocx";
                p.Start();

                writelog("注册netcomm.ocx控件成功");

                p.WaitForExit();
                p.Close();
                p.Dispose();
            }
            catch (Exception ex)
            {
                writelog("注册netcomm.ocx控件失败" + ex.ToString());
            }


            // add by jie.zhang 2010.3.1 
            //判断权限
            try
            {
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                string TcpConnName = "接处警";
                string RemoteHost = CLC.INIClass.IniReadValue("110", "机器名").Trim();
                short PortNumber = Convert.ToInt16(CLC.INIClass.IniReadValue("110", "端口").Trim());
                short ConType = Convert.ToInt16(CLC.INIClass.IniReadValue("110", "连接类型").Trim());
                writelog("TcpConnName=接处警，RemoteHost=" + RemoteHost + "，PortNumber=" + PortNumber.ToString() + "，ConType=" + ConType.ToString());
                axNetcomm1.AddConnection2(TcpConnName, RemoteHost, PortNumber, ConType);
                //axNetcomm1.AddConnection2("接处警", "sdjjt9", 20012, 2);
                writelog("已经运行函数 axNetcomm1.AddConnection2(TcpConnName, RemoteHost, PortNumber, ConType);");
            }
            catch
            {
                writelog("110Socket创建失败");
            }
        }

        #region 检测服务器 add by siumo 090211 start
        bool DisConnection = false;
        /// <summary>
        /// 检测服务器
        /// 最后编辑人 李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        private void checkServerComputer()
        {
            try
            {
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");

                if (DisConnection == false)
                {
                    bool b1 = IsWebResourceAvailable(CLC.INIClass.IniReadValue("文件服务器", "IP1"));
                    bool b2 = IsWebResourceAvailable(CLC.INIClass.IniReadValue("文件服务器", "IP2"));
                    if (b1 == false && b2 == false)
                    {
                        toolStripFile1.BackColor = Color.Red;
                        toolStripFile2.BackColor = Color.Red;
                        toolStripFile1.ToolTipText = "文件服务器1不能连接";
                        toolStripFile2.ToolTipText = "文件服务器2不能连接";
                        //DialogResult dr=  MessageBox.Show("目前所有文件服务器均不能连接，将可能无法更新系统，"+
                        //    "较旧的版本可能导致系统运行不正常或错误，如您确定需要进入系统请点击忽略进入，"+
                        //    "如重新测试连接请点击重试，如需要退出请点击中止并与管理员联系。",
                        //    "用户确认", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
                        //if (dr == DialogResult.Abort) {
                        //    Application.ExitThread();
                        //    this.Dispose();
                        //    Application.Exit();
                        //}
                        //else if (dr == DialogResult.Retry)
                        //{
                        //    return "Retry";
                        //}
                        //else { }
                    }
                    else
                    {
                        DisConnection = false;
                        if (b1 == true)
                        {
                            toolStripFile1.BackColor = Color.Lime;
                            toolStripFile1.ToolTipText = "文件服务器1连通";
                        }
                        else
                        {
                            toolStripFile1.BackColor = Color.Red;
                            toolStripFile1.ToolTipText = "文件服务器1不能连接";
                        }

                        if (b2 == true)
                        {
                            toolStripFile2.BackColor = Color.Lime;
                            toolStripFile2.ToolTipText = "文件服务器2连通";
                        }
                        else
                        {
                            toolStripFile2.BackColor = Color.Red;
                            toolStripFile2.ToolTipText = "文件服务器2不能连接";
                        }
                    }
                }

                string databaseIP1 = CLC.INIClass.IniReadValue("数据服务器", "IP1");
                string databaseIP2 = CLC.INIClass.IniReadValue("数据服务器", "IP2");
                string connStr1 = "data source = " + databaseIP1 + ";user id = system;password = system";
                string connStr2 = "data source = " + databaseIP2 + ";user id = system;password = system";
                bool d1 = false, d2 = false;
                OracleConnection conn = new OracleConnection(connStr1);
                try
                {
                    conn.Open();
                    d1 = true;
                    conn.Close();
                }
                catch (Exception ex)
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    ExToLog(ex, "12-测试数据库服务器1");
                }

                conn = new OracleConnection(connStr2);
                try
                {
                    conn.Open();
                    //OracleCommand cmd = conn.CreateCommand();
                    //cmd.CommandText = "select * from LOGMNRC_DBNAME_UID_MAP";
                    //int i = cmd.ExecuteNonQuery();
                    d2 = true;
                    conn.Close();
                }
                catch (Exception ex)
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    ExToLog(ex, "13-测试数据库服务器2");
                }

                if (d1 == false && d2 == false)
                {
                    DialogResult dResult = MessageBox.Show("两台数据库服务器均不能连接,程序将关闭,请确保服务器能连接后重启程序!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    Application.Exit();
                }
                else
                {
                    if (d1 == true)
                    {
                        toolStripDB1.BackColor = Color.Lime;
                        toolStripDB1.ToolTipText = "数据库服务器1连通";
                    }
                    else
                    {
                        toolStripDB1.BackColor = Color.Red;
                        toolStripDB1.ToolTipText = "数据库服务器1不能连接";
                    }

                    if (d2 == true)
                    {
                        toolStripDB2.BackColor = Color.Lime;
                        toolStripDB2.ToolTipText = "数据库服务器2连通";
                    }
                    else
                    {
                        toolStripDB2.BackColor = Color.Red;
                        toolStripDB2.ToolTipText = "数据库服务器2不能连接";
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkServerComputer-检测服务器");
            }
        }
        #endregion

        //初始化各功能项
        UserControl[] funcArr;
        /// <summary>
        /// 各功能模块初始化
        /// 最后编辑人  李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        private void InitializeMenuItems()
        {
            try
            {
                //WriteLog("综合模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fZonghe = new clZonghe.ucZonghe(mapControl1, toolStrip1, strConn, ConStr, GetFromNamePath,frmLogin.temEditDt);

                    ////////--关于通过车辆查询所要传的参数--//////////////
                    fZonghe.toolStriplbl = this.toolStripInfo;
                    fZonghe.toolSbutton = this.toolvideo;
                    fZonghe.videop = this.videop;
                    fZonghe.videoConnstring = this.videoConnstring;
                    fZonghe.videoexepath = this.videoexepath;
                    fZonghe.KKAlSys = this.KKAlSys;
                    fZonghe.KKALUser = this.KKALUser;
                    fZonghe.KKSearchDist = this.KKSearchDist;
                    fZonghe.user = frmLogin.string用户名称;
                    fZonghe.string辖区 = frmLogin.string辖区;
                    //////////////////////////////////////////////////////

                    fZonghe.toolPro = this.toolPro;
                    fZonghe.toolProLbl = this.toolProLbl;
                    fZonghe.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {
                    ExToLog(ex,"14-初始化综合模块");
                }
                //WriteLog("综合模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("案件模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fAnjian = new clAnjian.ucAnjian(mapControl1, strConn, ConStr,GetFromNamePath);
                    fAnjian.toolPro = this.toolPro;
                    fAnjian.toolProLbl = this.toolProLbl;
                    fAnjian.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "15-初始化案件模块");
                }
                //WriteLog("案件模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("人口模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fPopu = new clPopu.ucPopu(mapControl1, strConn, ConStr, GetFromNamePath);
                    fPopu.toolPro = this.toolPro;
                    fPopu.toolProLbl = this.toolProLbl;
                    fPopu.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "16-初始化人口模块");
                }
                //WriteLog("人口模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("房屋模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fHouse = new clHouse.ucHouse(mapControl1, strConn, ConStr,GetFromNamePath);
                    fHouse.toolPro = this.toolPro;
                    fHouse.toolProLbl = this.toolProLbl;
                    fHouse.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "17-初始化房屋模块");
                }
                //WriteLog("房屋模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("直观指挥模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fZhihui = new clZhihui.ucZhihui(mapControl1, ConStr, this.toolStripInfo, this.toolvideo, this.videop, this.videoConnstring, this.videoexepath,this.GetFromNamePath);
                    fZhihui.toolPro = this.toolPro;
                    fZhihui.toolProLbl = this.toolProLbl;
                    fZhihui.toolProSep = this.toolProSep;
                    fZhihui.panError = this.panErrorMessage;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "18-初始化直观指挥模块");
                }
                //WriteLog("直观指挥模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("车辆监控模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fCar = new clCar.ucCar(mapControl1, this.toolcar, ConStr, false);
                    fCar.toolPro = this.toolPro;
                    fCar.toolProLbl = this.toolProLbl;
                    fCar.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "19-初始化车辆监控模块");
                }
                //WriteLog("车辆监控模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("视频监控模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fVideo = new clVideo.ucVideo(mapControl1, this.toolStripInfo, this.toolvideo, ConStr, this.videop, this.videoConnstring, this.videoexepath, false, true);
                    fVideo.toolPro = this.toolPro;
                    fVideo.toolProLbl = this.toolProLbl;
                    fVideo.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "20-初始化视频监控模块");
                }
                //WriteLog("视频监控模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("治安卡口模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fKakou = new clKaKou.ucKakou(mapControl1, ConStr, this.toolStripInfo, this.toolvideo, this.videop, this.videoConnstring, this.videoexepath, KKAlSys, KKALUser, KKSearchDist, frmLogin.string辖区, frmLogin.string用户名称, GetFromNamePath,true);//jie.zhang 20090709  添加治安卡口
                    fKakou.toolPro = this.toolPro;
                    fKakou.toolProLbl = this.toolProLbl;
                    fKakou.toolProSep = this.toolProSep;
                    fKakou.panError = this.panErrorMessage;

                }
                catch (Exception ex)
                {

                    ExToLog(ex, "21-初始化治安卡口模块");
                }
                //WriteLog("治安卡口模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("110模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    f110 = new cl110.uc110(mapControl1, ConStr, this.toolStripInfo, this.toolvideo, this.videop, this.videoConnstring, this.videoexepath, this.dist,this.GetFromNamePath); //jie.zhang 20091230 添加110接警
                    f110.toolPro = this.toolPro;
                    f110.toolProLbl = this.toolProLbl;
                    f110.toolProSep = this.toolProSep;
                    f110.split = this.splitContainer2;
                    f110.panError = this.panErrorMessage;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "22-初始化110模块");
                }
                //WriteLog("110模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("警员监控模块", "开始时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fGPSp = new clGPSPolice.UcGpsPolice(mapControl1, ConStr, this.toolStripInfo, this.toolPolice, this.videop, this.videoConnstring, this.videoexepath, this.dist);   //jie.zhang 20101027  添加GPS警员
                    fGPSp.toolPro = this.toolPro;             //
                    fGPSp.toolProLbl = this.toolProLbl;       //--显示进度条参数
                    fGPSp.toolProSep = this.toolProSep;       //
                    _majorTask = new System.Windows.Forms.Label[4] { this.lblName, this.lblUnits, this.lblID, this.lblNum };               // 用于重大任务详细信息的文本控件
                    fGPSp.maTask = _majorTask;
                    fGPSp._panMajorTask = this.panMajorTask;  // 显示重大任务详细信息的面板
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "23-初始化警员监控模块");
                }
                //WriteLog("警员监控模块", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);

                try
                {
                    string ZoomFiles = Application.StartupPath + "\\ConfigBJXX.ini";        // 给编辑模块传入地址用于读取地图的缩放比例值
                    Cursor.Current = Cursors.WaitCursor;
                    CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                    DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select 真实姓名 from 用户 where USERnAME='" + frmLogin.string用户名称 + "'");
                    fGISEdit = new clGISPoliceEdit.ucGISPoliceEdit(this.mapControl1,frmLogin.region1,frmLogin.region2,frmLogin.temEditDt,this.toolStrip1);
                    fGISEdit.toolEditPro = this.toolPro;
                    fGISEdit.toolEditProLbl = this.toolProLbl;
                    fGISEdit.toolEditProSep = this.toolProSep;
                    fGISEdit.ZoomFile = ZoomFiles;
                    fGISEdit.userName = table.Rows[0][0].ToString();
                    fGISEdit.panPc = this.panel7;
                    fGISEdit.label5 = this.label5;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "24-初始化数据编辑模块");
                }

                funcArr = new UserControl[] { fZonghe, fAnjian, fCar, fVideo, fPopu, fHouse, fZhihui, fKakou, f110, fGPSp, fGISEdit};

                for (int i = 0; i < funcArr.Length; i++)
                {
                    splitContainer2.Panel1.Controls.Add(funcArr[i]);
                    funcArr[i].Dock = DockStyle.Fill;
                    funcArr[i].Visible = false;
                }
                //WriteLog("模块初始化结束", "结束时间" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
            }
            catch(Exception ex) {
                MessageBox.Show("初始化功能菜单错误,请重试!");
                ExToLog(ex, "24-初始化各功能项");
            }
        }

        /// <summary>
        /// 由于用户的功能内容不一样,因此取用户的第一个功能作为默认功能项
        /// 最后编辑人 李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        private void SetDefaultFuncItem()
        {
            try
            {                
                setUserSearchRegion(frmLogin.string用户名称);

                if (this.MenuZonghe.Enabled)
                {
                    fZonghe.Visible = true;
                    string[] strName = new string[] { "点击查询", "周边查询", "音头查询", "高级查询" };
                    FeatureLayer featureLay = null;
                    for (int i = 0; i < strName.Length; i++)
                    {
                        featureLay = (FeatureLayer)mapControl1.Map.Layers[strName[i]];
                        if (featureLay == null)
                            fZonghe.CreateTemLayer(strName[i]);
                    }

                    WriteEditLog("综合查询");
                }
                else if (this.MenuAnjian.Enabled)
                {
                    fAnjian.Visible = true;
                    WriteEditLog("案件分析");
                }
                else if (this.ToolGPSCar.Enabled)
                {
                    fCar.Visible = true;
                    fCar.StartTimeCar();
                    WriteEditLog("车辆监控");
                }
                else if (this.MenuVideo.Enabled)
                {
                    fVideo.Visible = true;
                    fVideo.CreateVideoLayer();
                    WriteEditLog("视频监控");
                }
                else if (this.MenuKakou.Enabled)      //jie.zhang 20090709
                {
                    this.fKakou.Visible = true;
                    fKakou.InitKK();
                    this.SetLayerEdit("KKLayer");
                    WriteEditLog("治安卡口");
                }
                else if (this.Menu110.Enabled)
                {
                    f110.Visible = true;
                    f110.Init110();
                    WriteEditLog("110接处警");
                }
                else if (this.ToolGPSPolice.Enabled)
                {
                    this.fGPSp.Visible = true;
                    fGPSp.InitGpsPolice();
                    WriteEditLog("GPS警员");
                }
                else if (this.MenuItemPop.Enabled)
                {
                    fPopu.Visible = true;
                    CreateTemLayer("layerPopu", "人口系统");
                    WriteEditLog("人口系统");
                }
                else if (this.MenuItemHouse.Enabled)
                {
                    fHouse.Visible = true;
                    CreateTemLayer("layerHouse", "房屋系统");
                    WriteEditLog("房屋系统");
                }
                else if (this.MenuCommand.Enabled)
                {
                    fZhihui.Visible = true;
                    CreateTemLayer("layerZhihui", "直观指挥");
                    this.SetLayerEdit("直观指挥");
                    fZhihui.SetDrawStyle();
                    WriteEditLog("直观指挥");
                }
                else if (this.menuDataEdit.Enabled)      // lili 20110107
                {
                    fGISEdit.Visible = true;
                    WriteEditLog("数据编辑");
                }
                else { }
            }
            catch (Exception ex)
            {
                ExToLog(ex,"25-设置默认模块时发生错误");
            }
        }

        /// <summary>
        /// 加载地图并添加地图事件
        /// 最后编辑人  李立
        /// 最后编辑时间  2011-1-24
        /// </summary>
        private void InitialMap()
        {
            MapInfo.Tools.MapTool ptMapTool;
            try
            {
                this.mapControl1.Map.Load(MapLoader.CreateFromFile(exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\Data\\Shunde1.mws"));
                this.mapOverview.Map.Load(MapLoader.CreateFromFile(exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\Data\\ShundeOverview.mws"));
                IMapLayer mapLayer = mapControl1.Map.Layers["次道路"];
                int ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                mapControl1.Map.Layers.Move(ilayer, 3);
                mapLayer = mapControl1.Map.Layers["主道路"];
                ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                mapControl1.Map.Layers.Move(ilayer, 3);
                this.mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["街镇面"]);

                //添加鼠标点击地图后事件
                this.mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);

                //添加自定义测量距离
                ptMapTool = new CustomPolylineMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), mapControl1.Tools, mapControl1.Tools.MouseToolProperties, mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("DistanceTool", ptMapTool);

                //添加自定义测量面积
                ptMapTool = new CustomPolygonMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), mapControl1.Tools, mapControl1.Tools.MouseToolProperties, mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("AreaTool", ptMapTool);

                //添加自定义矩形轨迹工具
                this.mapControl1.Tools.Add("drawRectTool", new CustomRectangleMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), mapControl1.Tools, mapControl1.Tools.MouseToolProperties, mapControl1.Tools.MapToolProperties));

                //添加自定义信息查询工具
                ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("toolInfo", ptMapTool);

                //添加自定义视频查看工具
                ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("VideoTool", ptMapTool);

                //添加自定义周边查看工具
                ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("VCTool", ptMapTool);

                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(9, System.Drawing.Color.Gray, System.Drawing.Color.Green, true);
                comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "26-InitialMap");
            }
        }

        /// <summary>
        /// 根据用户权限设置所有模块可用性
        /// 最后更新人  李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void InitPrivilege()
        {   //"综合查询","案件分析","车辆监控","视频监控","治安卡口","人口管理","房屋管理","直观指挥","GPS警员","基础数据编辑","业务数据编辑","视频编辑","权限管理","可导出" 
            try
            {
                if (frmLogin.temDt.Select("综合查询=1").Length > 0)
                {
                    this.MenuZonghe.Enabled = true;
                }
                else
                {
                    this.MenuZonghe.Enabled = false;
                }
                if (frmLogin.temDt.Select("案件分析=1").Length > 0)
                {
                    this.MenuAnjian.Enabled = true;
                }
                else
                {
                    this.MenuAnjian.Enabled = false;
                }
                if (frmLogin.temDt.Select("车辆监控=1").Length > 0)
                {
                    this.ToolGPSCar.Enabled = true;
                }
                else
                {
                    this.ToolGPSCar.Enabled = false;
                }
                if (frmLogin.temDt.Select("视频监控=1").Length > 0)
                {
                    this.MenuVideo.Enabled = true;
                }
                else
                {
                    this.MenuVideo.Enabled = false;
                }
                if (frmLogin.temDt.Select("人口管理=1").Length > 0)
                {
                    this.MenuItemPop.Enabled = true;
                }
                else
                {
                    this.MenuItemPop.Enabled = false;
                }
                if (frmLogin.temDt.Select("房屋管理=1").Length > 0)
                {
                    this.MenuItemHouse.Enabled = true;
                }
                else
                {
                    this.MenuItemHouse.Enabled = false;
                }
                if (frmLogin.temDt.Select("房屋管理=1").Length > 0 || frmLogin.temDt.Select("人口管理=1").Length > 0)
                {
                    this.MenuPopulation.Enabled = true;
                }
                else 
                {
                    this.MenuPopulation.Enabled = false;
                }
                if (frmLogin.temDt.Select("治安卡口=1").Length > 0)
                {
                    this.MenuKakou.Enabled = true;
                }                    
                else
                {
                    this.MenuKakou.Enabled = false;
                }
                if (frmLogin.temDt.Select("直观指挥=1").Length > 0)
                {
                    this.MenuCommand.Enabled = true;
                }
                else
                {
                    this.MenuCommand.Enabled = false;
                }
                if (frmLogin.temDt.Select("GPS警员=1").Length > 0)
                {
                    this.ToolGPSPolice.Enabled = true;
                }
                else
                {
                    this.ToolGPSPolice.Enabled = false;
                }
                if (frmLogin.temDt.Select("GPS警员=1").Length > 0 || frmLogin.temDt.Select("车辆监控=1").Length > 0)
                {
                    this.MenuCar1.Enabled = true;
                }
                else
                {
                    this.MenuCar1.Enabled = false;
                }
                if (frmLogin.temDt.Select("llo接警=1").Length > 0)
                {
                    this.Menu110.Enabled = true;
                }
                else
                {
                    this.Menu110.Enabled = false;
                }
                //初始化数据编辑模块，“基础数据可编辑”、“业务数据可编辑”、“视频编辑”，只要其中有一个条件为真，则数据编辑菜单可用
                if (frmLogin.temDt.Select("基础数据编辑=1").Length > 0||frmLogin.temDt.Select("业务数据编辑=1").Length>0||frmLogin.temDt.Select("视频编辑=1").Length>0)
                {
                    this.menuDataEdit.Enabled = true;
                }
                else
                {
                    this.menuDataEdit.Enabled = false;
                }
                if (frmLogin.temDt.Select("权限管理=1").Length > 0)
                {
                    this.MenuAuthorize.Enabled = true;
                }
                else
                {
                    this.MenuAuthorize.Enabled = false;
                }
                if (frmLogin.temDt.Select("可导出=1").Length > 0)               //add by fisher in 10-01-04
                {
                    this.toolDateOut.Enabled = true;
                }
                else
                {
                    this.toolDateOut.Enabled = false;
                }

                toolStripUser.Text = @"用户：" + frmLogin.string用户名称;

                if (frmLogin.string用户名称 == "sdga" || frmLogin.string用户名称 == "admin")
                {
                    test110.Visible = true;
                }
                else
                {
                    test110.Visible = false;
                }
            }
            catch (Exception ex) {
                ExToLog(ex, "27-InitPrivilege");
            }
        }

        private Double dist = 0;

        //读取配置文件，设置数据库连接字符串
        string datasource;   // 数据源
        string userid;       // 用户名
        string password;     // 密码

        string KKAlSys = string.Empty; 
        string KKALUser = string.Empty;
        double KKSearchDist = 0;
       
        /// <summary>
        /// 从配置文件中读取数据库连接、视频、卡口相关信息
        /// 最后更新人  李立
        /// 最后更新时间  2011-1-24
        /// </summary>
        /// <returns>连接字符串</returns>
        private string getStrConn()
        {
            try
            {
                GetFromNamePath = exePath + "\\GetFromNameConfig.ini";
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                password = CLC.INIClass.IniReadValuePW("数据库", "密码");
               
                videop = Convert.ToInt32(CLC.INIClass.IniReadValue("视频", "端口"));
                //this.videoexepath = CLC.INIClass.IniReadValue("视频", "客户端");
                dist = Convert.ToDouble(CLC.INIClass.IniReadValue("视频", "距离"));
                videoexepath = exePath.Remove(exePath.LastIndexOf("\\")) + "\\Carrier\\surveillance1.exe";

                // 获取治安卡口信息  jie.zhang 20090709
                KKAlSys = CLC.INIClass.IniReadValue("治安卡口", "系统报警");
                KKALUser = CLC.INIClass.IniReadValue("治安卡口", "报警对象");
                KKSearchDist = Convert.ToDouble(CLC.INIClass.IniReadValue("治安卡口", "查询半径"));
               
                ReadXML();    //获取视频监控的配置信息
                get110Con();  //获取110信息处理模式

                ConStr[0] = datasource;
                ConStr[1] = userid;
                ConStr[2] = password;
               
 
                string connString = "data source = " + datasource + ";user id = " + userid + ";password = " + password;
                return connString;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "28-getStrConn");
                return "";
            }
        }

        /// <summary>
        /// 从ConfigBJXX.ini中读取110模式
        /// 最后编辑人 李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        private void get110Con()
        {
            try
            {
                string Config110Path = exePath + "\\ConfigBJXX.ini";
                CLC.INIClass.IniPathSet(Config110Path);
                _schnum = Convert.ToInt32(CLC.INIClass.IniReadValue("110", "模式"));
            }
            catch (Exception ex)
            {
                ExToLog(ex, "29-get110Con");
            }
        }

        /// <summary>
        /// 读取EMS配置文件中的CMS设置
        /// 最后编辑人 李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        private void ReadXML()
        {
            try
            {
                if (this.videoexepath != "")
                {
                    this.videoConnstring[0] = CLC.INIClass.IniReadValue("视频网", "文件夹");
                    this.videoConnstring[1] = CLC.INIClass.IniReadValue("视频网", "ip");
                    this.videoConnstring[2] = CLC.INIClass.IniReadValue("视频网", "端口");
                    this.videoConnstring[3] = CLC.INIClass.IniReadValue("视频网", "用户名");
                    this.videoConnstring[4] = CLC.INIClass.IniReadValue("视频网", "密码");
                    this.videoConnstring[5] = Convert.ToString(15);
                }
            }
            catch (Exception ex)
            {                
                ExToLog(ex, "30-ReadXML");
            }
        }

        /// <summary>
        /// 创建临时层存放鹰眼的矩形框
        /// 最后编辑人 李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        private void CreateEagleLayer()
        {
            try
            {
                //   disable   the   mouse   wheel-zoom   funtionality   of   eagle   eye   map 
                mapOverview.MouseWheelSupport = new MouseWheelSupport(MouseWheelBehavior.None, 10, 5);

                //   create   a   temp   layer   as   the   rectangle   holder 
                TableInfoMemTable ti = new TableInfoMemTable("EagleEyeTemp");
                ti.Temporary = true;

                //   add   columns   
                Column column;
                column = new GeometryColumn(mapOverview.Map.GetDisplayCoordSys());
                column.Alias = "MI_Geometry";
                column.DataType = MIDbType.FeatureGeometry;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "MI_Style";
                column.DataType = MIDbType.Style;
                ti.Columns.Add(column);

                //   create   table   and   feature   layer 
                Table table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);

                FeatureLayer eagleEye = new FeatureLayer(table, "EagleEye", "MyEagleEye");
                //mapOverview.Map.Layers.Insert(0, eagleEye);
                mapOverview.Map.Layers.Insert(0, (IMapLayer)eagleEye);
            }
            catch(Exception ex) {

                ExToLog(ex, "31-创建临时层存放鹰眼的矩形框");
            }
        }

        //+地图视野发生变化时
        //-鹰眼图上的图示框
        //-图层控制中的图层
        int iMapLevel = 1;
        /// <summary>
        /// 地图视野发生变化时 鹰眼图上的图示框 图层控制中的图层
        /// 最后更新人  李立
        /// 最后更新时间  2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapControl1_ViewChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false) return;

                //   create   temp   table   and   temp   feature 
                Table tabTemp = Session.Current.Catalog.GetTable("EagleEyeTemp");

                (tabTemp as ITableFeatureCollection).Clear();

                #region   Draw   the   rectangle
                //设置矩形的样式
                DRect rect = mapControl1.Map.Bounds;
                FeatureGeometry feageo = new MapInfo.Geometry.Rectangle(mapControl1.Map.GetDisplayCoordSys(), rect);

                //将矩形插入到图层中
                Feature fea = new Feature(feageo, comStyle);

                tabTemp.InsertFeature(fea);
                #endregion

                iMapLevel = setToolslevel();   //显示级别

                //如果看影像，显示影像
                if (toolImageOrMap.Text == "地图")
                {
                    closeOtherLevelImg(iMapLevel);//先关闭其他级别的影像

                    CalRowColAndDisImg(iMapLevel);
                }

                panelMaptools.Refresh();

                labelName.Visible = false;
            }
            catch(Exception ex) {
                ExToLog(ex, "32-地图视野发生变化时");
            }
        }

        #region 查看影像
        /// <summary>
        /// 关闭某一级影像
        /// 最后编辑人  李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        /// <param name="iLevel">等级数</param>
        private void closeOtherLevelImg(int iLevel)
        {
            try
            {
                GroupLayer gLayer = mapControl1.Map.Layers["影像"] as GroupLayer;
                if (gLayer == null) return;
                int iCount = gLayer.Count;
                for (int i = 0; i < iCount; i++)
                {
                    IMapLayer layer = gLayer[0];   //总是找第一个图层(移除一个后,后面的替补为第一层);
                    string alies = layer.Alias;
                    if (Convert.ToInt16(alies.Substring(1, 1)) != iLevel)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(alies);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex,"33-closeOtherLevelImg");
            }
        }

        /// <summary>
        /// 根据等级显示影像图
        /// 最后更新人  李立
        /// 最后编辑时间 2011-1-24
        /// </summary>
        /// <param name="iLevel">等级数</param>
        private void CalRowColAndDisImg(int iLevel)
        {
            try
            {
                double dScale = 0;
                int minRow = 0, minCol = 0;
                int maxRow = 0, maxCol = 0;
                switch (iLevel)
                {
                    case 1:
                        dScale = 200000;
                        minRow = 1;
                        minCol = 1;
                        maxRow = 2;
                        maxCol = 2;
                        break;
                    case 2:
                        dScale = 100000;
                        minRow = 1;
                        minCol = 1;
                        maxRow = 4;
                        maxCol = 4;
                        break;
                    case 3:
                        dScale = 50000;
                        minRow = 2;
                        minCol = 2;
                        maxRow = 7;
                        maxCol = 8;
                        break;
                    case 4:
                        dScale = 20000;
                        minRow = 3;
                        minCol = 3;
                        maxRow = 17;
                        maxCol = 19;
                        break;
                    case 5:
                        dScale = 10000;
                        minRow = 6;
                        minCol = 6;
                        maxRow = 34;
                        maxCol = 37;
                        break;
                    case 6:
                        dScale = 5000;
                        minRow = 12;
                        minCol = 11;
                        maxRow = 67;
                        maxCol = 74;
                        break;
                    case 7:
                        dScale = 2000;
                        minRow = 29;
                        minCol = 27;
                        maxRow = 166;
                        maxCol = 184;
                        break;
                }
                double gridDis = 6.0914200613 * Math.Pow(10, -7) * dScale * 2;  //各级的网格长度
                int beginRow = 0, endRow = 0;
                int beginCol = 0, endCol = 0;
                //计算行列号
                //起始行号
                int dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y2) / gridDis);
                if (dRow > maxRow) return;    //如果此起始行号比本级图片最大行号还大，说明此范围无图

                if (dRow < minRow)
                {
                    beginRow = minRow;
                }
                else
                {
                    beginRow = dRow;
                }

                //终止行号
                dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y1) / gridDis) + 1;
                if (dRow < minRow) return; //如果此终止行号比本级图片最小行号还小，说明此范围无图
                if (dRow > maxRow)
                {
                    endRow = maxRow;
                }
                else
                {
                    endRow = dRow;
                }

                int dCol = Convert.ToInt32((mapControl1.Map.Bounds.x1 - 112.94) / gridDis);
                if (dCol > maxCol) return; //如果此起始列号比本级图片最大列号还大，说明此范围无图
                if (dCol < minCol)
                {
                    beginCol = minCol;
                }
                else
                {
                    beginCol = dCol;
                }
                //计算终止列号
                dCol = Convert.ToInt32((mapControl1.Map.Bounds.x2 - 112.94) / gridDis) + 1;
                if (dCol < minCol) return; //如果此终止列号比本级图片最小列号还大，说明此范围无图
                if (dCol > maxCol)
                {
                    endCol = maxCol;
                }
                else
                {
                    endCol = dCol;
                }

                DisImg(iLevel, beginRow, endRow, beginCol, endCol);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "34-CalRowColAndDisImg");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iLevel"></param>
        /// <param name="beginRow"></param>
        /// <param name="endRow"></param>
        /// <param name="beginCol"></param>
        /// <param name="endCol"></param>
        private void DisImg(int iLevel, int beginRow, int endRow, int beginCol, int endCol)
        {
            try
            {
                string tabName = "";
                for (int i = beginRow; i <= endRow; i++)
                {
                    for (int j = beginCol; j <= endCol; j++)
                    {
                        tabName = iLevel.ToString() + "_" + i.ToString() + "_" + j.ToString();
                        openTable(iLevel, tabName);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "34-CalRowColAndDisImg");
            }
        }

        /// <summary>
        /// 根据不同等级找到相关图层显示
        /// 最后更新人 李立
        /// 最后更新时间  2011-1-24
        /// </summary>
        /// <param name="iLevel">等级数</param>
        /// <param name="tableName">影像图表名</param>
        private void openTable(int iLevel, string tableName)
        {
            //先判断有没有加载
            try
            {
                GroupLayer groupLayer = mapControl1.Map.Layers["影像"] as GroupLayer;

                if (groupLayer["_" + tableName] == null)
                {
                    //再判断文件夹中村不存在，存在就打开

                    string imgPath = exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\ImgData\\" + iLevel.ToString() + "\\" + tableName + ".tab";
                    if (File.Exists(imgPath))
                    {
                        Table tab = MapInfo.Engine.Session.Current.Catalog.OpenTable(imgPath);

                        MapInfo.Mapping.FeatureLayer fl = new MapInfo.Mapping.FeatureLayer(tab, "_" + tableName);

                        groupLayer.Add(fl);
                    }
                }
            }
            catch(Exception ex)
            {
               //MessageBox.Show("地图文件已损坏,请联系软件开发人员!");
                ExToLog(ex, "35-openTable");
                return;
            }
        }

        #endregion

        /// <summary>
        /// 在鹰眼图上点击，定位地图视野
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapOverview_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                //将鼠标点设为主地图和鹰眼图的中心
                MapInfo.Geometry.DPoint dP;
                mapOverview.Map.DisplayTransform.FromDisplay(new System.Drawing.Point(e.X, e.Y), out dP);
                mapControl1.Map.Center = dP;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "36-定位地图视野");
            }
        }
        
        private Table tabCurve = null;
        /// <summary>
        /// tool工具点击事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Name)
                {
                    case "toolSel":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolZoomIn":
                        mapControl1.Tools.LeftButtonTool = "ZoomIn";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolZoomOut":
                        mapControl1.Tools.LeftButtonTool = "ZoomOut";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolPan":
                        mapControl1.Tools.LeftButtonTool = "Pan";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolFullExtent":
                        mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["街镇面"]);
                        break;
                    case "toolDistance":
                        UncheckedTool();
                        mapControl1.Tools.LeftButtonTool = "DistanceTool";
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolArea":
                        mapControl1.Tools.LeftButtonTool = "AreaTool";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolInfo":
                        mapControl1.Tools.LeftButtonTool = "toolInfo";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolLocation":
                        winLoc.frmLocDia fLoc = new winLoc.frmLocDia(mapControl1);
                        fLoc.Show();
                        break;
                    case "toolClear":
                        clearAllTemp();
                        return;
                    //break;
                    case "toolvideoclient":
                        break;
                    case "toolLayers":
                        fLayer = new frmLayer();
                        fLayer.InitialLayers(mapControl1.Map);
                        fLayer.ShowDialog();
                        break;
                    case "toolImageOrMap":
                        try
                        {
                            if (e.ClickedItem.Text == "影像")
                            {

                                closeOtherLevelImg(iMapLevel);//先关闭其他级别的影像

                                CalRowColAndDisImg(iMapLevel);
                                mapControl1.Map.Layers["建筑物"].Enabled = false;
                                mapControl1.Map.Layers["公园"].Enabled = false;
                                mapControl1.Map.Layers["街镇面"].Enabled = false;
                                mapControl1.Map.Layers["山脉"].Enabled = false;
                                mapControl1.Map.Layers["水系"].Enabled = false;
                                mapControl1.Map.Layers["主道路"].Enabled = false;
                                mapControl1.Map.Layers["次道路"].Enabled = false;
                                mapControl1.Map.Layers["影像"].Enabled = true;
                                e.ClickedItem.Text = "地图";

                            }
                            else
                            {
                                mapControl1.Map.Layers["建筑物"].Enabled = true;
                                mapControl1.Map.Layers["公园"].Enabled = true;
                                mapControl1.Map.Layers["街镇面"].Enabled = true;
                                mapControl1.Map.Layers["山脉"].Enabled = true;
                                mapControl1.Map.Layers["水系"].Enabled = true;
                                mapControl1.Map.Layers["主道路"].Enabled = true;
                                mapControl1.Map.Layers["次道路"].Enabled = true;
                                mapControl1.Map.Layers["影像"].Enabled = false;
                                e.ClickedItem.Text = "影像";
                                //mapControl1.Map.Center = mapControl1.Map.Center;
                            }
                            //mapControl1.Refresh();
                            panelMaptools.Refresh();
                        }
                        catch (Exception ex)
                        {
                            ExToLog(ex, "toolStrip1_ItemClicked");
                        }
                        break;
                    case "toolEditPolice":
                        this.UncheckedTool();
                        break;
                    case "toolRefresh":
                        this.UncheckedTool();
                        break;
                }

                //如果是测距或面积工具，添加临时图层存放轨迹
                if (mapControl1.Tools.LeftButtonTool == "AreaTool" || mapControl1.Tools.LeftButtonTool == "DistanceTool")
                {
                    try
                    {
                        if (MapInfo.Engine.Session.Current.Catalog.GetTable("测距面积轨迹") == null)
                        {
                            //   create   a   temp   layer   as   the   rectangle   holder 
                            TableInfoMemTable ti = new TableInfoMemTable("测距面积轨迹");
                            ti.Temporary = true;

                            //   add   columns   
                            Column column;
                            column = new GeometryColumn(mapControl1.Map.GetDisplayCoordSys());
                            column.Alias = "MI_Geometry";
                            column.DataType = MIDbType.FeatureGeometry;
                            ti.Columns.Add(column);

                            column = new Column();
                            column.Alias = "MI_Style";
                            column.DataType = MIDbType.Style;
                            ti.Columns.Add(column);

                            column = new Column();
                            column.Alias = "MapID";
                            column.DataType = MIDbType.Int;
                            ti.Columns.Add(column);

                            Table table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                            FeatureLayer temLayer = new FeatureLayer(table);

                            mapControl1.Map.Layers.Insert(0, temLayer);
                            tabCurve = temLayer.Table;
                        }
                    }
                    catch (Exception ex) { ExToLog(ex, "测距或面积工具轨迹图层"); }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "工具栏按钮");
            }
        }

        /// <summary>
        /// 清除地图要素
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void clearAllTemp()
        {
            try
            {
                //如果是当前功能项，只清除其临时图层中的要素
                //否则移除数据层
                if (fZonghe.Visible)
                {
                    fZonghe.clearTem();
                }

                if (fAnjian.Visible)
                {
                    fAnjian.clearTem();
                }

                if (fPopu.Visible)
                {
                    fPopu.clearTem();
                }

                if (fHouse.Visible)
                {
                    fHouse.clearTem();
                }

                if (fZhihui.Visible)
                {
                    fZhihui.clearTem();
                    fCar.ClearCarTemp();
                    fVideo.ClearVideoTemp();
                }

                if (fKakou.Visible)
                {
                    fKakou.ClearKaKouTemp();
                    fCar.ClearCarTemp();
                    fVideo.ClearVideoTemp();
                }

                if (fCar.Visible)
                {
                    fCar.ClearCarTemp();
                }

                if (fVideo.Visible)
                {
                    fVideo.ClearVideoTemp();
                }

                if (f110.Visible)
                {
                    f110.cleartemp();
                }

                if (fGPSp.Visible)
                {
                    fGPSp.Cleartemp();
                }

                if (fGISEdit.Visible)
                {
                    fGISEdit.ClearLayer();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "清除临时图层");
            }

            //清除测距面积画线
            try
            {
                if (tabCurve != null) {
                    (tabCurve as ITableFeatureCollection).Clear();
                    tabCurve.Pack(PackType.All);
                }
            }
            catch { }

            //清除定位添加的图层
            try {
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("CodingPoint") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CodingPoint");
                    mapControl1.Map.Layers.Remove("geoCodeLabel");
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("geoCodeLabel");
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("Location_temp");
                }
            }
            catch { }
        }

        /// <summary>
        /// 点击工具栏上的工具时，对工具按钮进行设置
        /// 选中的checked，其他unchecked，以便明确当前的选择项
        /// 由于按钮分组，iFrom表示组的首Index，iEnd表示末Index
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void UncheckedTool()
        {
            try
            {
                for (int i = 0; i < toolStrip1.Items.Count; i++)
                {
                    ToolStripButton tsButton = toolStrip1.Items[i] as ToolStripButton;
                    if (tsButton != null)
                    {
                        if (tsButton.Checked == true)
                        {
                            tsButton.Checked = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "按钮变换");
            }
        }

        #region 自定义工具实现代码
        private DataTable dt = new DataTable("Data");
        private bool mouseMoveTag = true;
        private double dblDistance;
        private double currentdblDistance;
        private MapInfo.Geometry.DPoint dptStart;
        //private MapInfo.Geometry.DPoint dptEnd;
        private System.Collections.ArrayList arrlstPoints = new ArrayList();
        private MapInfo.Geometry.DPoint dptFirstPoint;

        private FrmInfo frmMessage = new FrmInfo();
        /// <summary>
        /// 地图点击事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                switch (e.ToolName)
                {
                    case "DistanceTool":
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                this.mouseMoveTag = true;
                                dblDistance = 0;
                                dptStart = e.MapCoordinate;
                                arrlstPoints.Clear();
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.InProgress:
                                dblDistance += MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, e.MapCoordinate);
                                dptStart = e.MapCoordinate;
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.End:
                                this.mouseMoveTag = false;
                                dblDistance += MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, e.MapCoordinate);

                                int intCount = arrlstPoints.Count;
                                MapInfo.Geometry.DPoint[] dptPoints = new DPoint[intCount];
                                for (int i = 0; i < intCount; i++)
                                {
                                    dptPoints[i] = (MapInfo.Geometry.DPoint)arrlstPoints[i];
                                }
                                MultiCurve mulCur = new MapInfo.Geometry.MultiCurve(mapControl1.Map.GetDisplayCoordSys(), CurveSegmentType.Linear, dptPoints);

                                Feature ft = new Feature(mulCur, new SimpleLineStyle(new LineWidth(2, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red));
                                tabCurve.InsertFeature(ft);
                                mapControl1.Map.Invalidate(true);
                                break;
                            default:
                                break;
                        }
                        this.dockingManager.ShowContent(cDistance);//用来显视弹出信息的          
                        break;

                    case "AreaTool":
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                arrlstPoints.Clear();
                                dptFirstPoint = e.MapCoordinate;
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.InProgress:
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.End:
                                //构造一个闭合环
                                //arrlstPoints.Add(e.MapCoordinate);
                                arrlstPoints.Add(dptFirstPoint);
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
                                MapInfo.Geometry.Polygon objPolygon = new Polygon(objCoordSys, MapInfo.Geometry.CurveSegmentType.Linear, dptPoints);
                                if (objPolygon == null)
                                {
                                    return;
                                }
                                this.lblArea.Text = "总面积为:" + string.Format("{0:F3}", objPolygon.Area(costAreaUnit)) + " 平方公里";
                                this.dockingManager.ShowContent(this.cArea);

                                MultiPolygon mulPolygon = new MultiPolygon(objCoordSys, CurveSegmentType.Linear, dptPoints);
                                //MultiCurve mulCur = new MapInfo.Geometry.MultiCurve(mapControl1.Map.GetDisplayCoordSys(), CurveSegmentType.Linear, dptPoints);
                                Feature ft = ft = new Feature(mulPolygon, comStyle);
                                tabCurve.InsertFeature(ft);
                                this.mapControl1.Map.Invalidate(true);
                                break;
                            default:
                                break;
                        }
                        break;

                    case "toolInfo":
                        //setTableFalg();
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                Distance dictance = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(this.mapControl1.Map, 5);
                                IMapLayerFilter layerFilter = MapLayerFilterFactory.FilterByLayerType(LayerType.Normal);

                                ITableEnumerator tableEnum = mapControl1.Map.Layers.GetTableEnumerator(layerFilter);
                                MultiResultSetFeatureCollection mrfc;
                                try
                                {
                                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(e.MapCoordinate, mapControl1.Map.GetDisplayCoordSys(), dictance);
                                    si.QueryDefinition.Columns = null;
                                    mrfc = Session.Current.Catalog.Search(tableEnum, si);
                                }
                                catch
                                {
                                    MessageBox.Show("ERR");
                                    return;
                                }

                                if (mrfc.Count > 0)
                                {
                                    if (this.frmMessage.Visible == false)
                                    {
                                        this.frmMessage = new FrmInfo();
                                        this.frmMessage.SetDesktopLocation(-30, -30);
                                        this.frmMessage.Show();
                                        this.frmMessage.Visible = false;
                                    }
                                    MapInfo.Data.Feature ftr = mrfc[0][0];
                                    this.frmMessage.getFromNamePath = GetFromNamePath;
                                    this.frmMessage.mapControl = mapControl1;
                                    if (mapControl1.Map.Layers[0].ToString() != "案件分析")
                                        this.frmMessage.LayerName = mapControl1.Map.Layers[0].ToString();
                                    else
                                        this.frmMessage.LayerName = mapControl1.Map.Layers[1].ToString();
                                    frmMessage.setInfo(ftr, ConStr);  //设置信息
                                    try
                                    {
                                        GetFromName getName = new GetFromName(ftr["表名"].ToString());
                                        writeEditLog(ftr["表名"].ToString(), getName.ObjID +"="+ ftr["表_ID"].ToString(), "查看详情");
                                    }
                                    catch {
                                        GetFromName getName = new GetFromName(ftr.Table.Alias.Remove(ftr.Table.Alias.IndexOf("_")));
                                        if (getName.ObjID != null)
                                        {
                                            writeEditLog(getName.TableName, getName.ObjID + "=" + ftr[getName.ObjID].ToString(), "查看详情");
                                        }
                                    }
                                }
                                else
                                {
                                    this.Activate();
                                }                                
                                break;
                            default:
                                break;
                        }
                        break;
                    //可以添加其他的用户自定义Tool
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                ExToLog(Ex, "自定义工具");
            }
        }

        /// <summary>
        /// 根据模块设置图层是否可以选择
        /// 最后更新人 李立
        /// 最后更新时间 2011-2-28
        /// </summary>
        private void setTableFalg()
        {
            try 
            {
                if (this.fZonghe.Visible || this.fGISEdit.Visible)  // 综合模块及编辑模块中的图层可选
                {
                    setSelectLayer(true);
                }
                else
                {
                    setSelectLayer(false);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSelectLayer");
            }
        }

        /// <summary>
        /// 设置图层是否可以选择
        /// 最后更新人 李立
        /// 最后更新时间 2011-2-28
        /// </summary>
        /// <param name="falg">布尔值（true-图层可选择 false-图层不可选择）</param>
        private void setSelectLayer(bool falg)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["信息点"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["道路"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["主道路"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["次道路"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["行政界线"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["公园"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["体育场"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["水系"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["建筑物"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["山脉"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["影像"], falg);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSelectLayer");
            }
 
        }

        #endregion
        /// <summary>
        /// 鼠标在地图上移动时，在状态栏显示坐标
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                double totalDistance = dblDistance;
                System.Drawing.Point sPoint = new System.Drawing.Point(e.X, e.Y);
                DPoint dPoint;
                mapControl1.Map.DisplayTransform.FromDisplay(sPoint, out dPoint);
                this.toolStripcbScale.Text =mapControl1.Map.Scale.ToString("#,###");
                this.toolStripCoord.Text = "X:" + dPoint.x.ToString("#.####") + ", Y:" + dPoint.y.ToString("#.####") ;
                currentdblDistance = MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, dPoint);
                totalDistance += MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, dPoint);

                if (this.mouseMoveTag == true)
                {
                    if (dblDistance > 10000)
                    {
                        this.lblDistance.Text = "本次距离: " + string.Format("{0:F3}", currentdblDistance / 1000) + "公里" + "\n";
                        this.lblDistance.Text += "  共   : " + string.Format("{0:F3}", totalDistance / 1000) + "公里";
                    }
                    else
                    {
                        this.lblDistance.Text = "本次距离: " + string.Format("{0:F2}", currentdblDistance) + " 米" + "\n";
                        this.lblDistance.Text += "  共   : " + string.Format("{0:F2}", dblDistance) + " 米";
                    }
                }

                //GetDisFtr(dPoint,e);
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "状态栏坐标值");
            }
        }       
              
        /// <summary>
        /// 设置当前图层可编辑
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="layername"></param>
        private void SetLayerEdit(string layername)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layername], true);
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layername], true);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "设置当前图层可编辑"); 
            }
        }


        /// <summary>
        /// 当地图视野变化时，根据比例尺指定地图级别
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <returns></returns>
        private int setToolslevel()
        {
            int iLevel = 0;

            try
            {
                foreach (System.Windows.Forms.Label lev in levelArr)
                {
                    if (lev.BackColor == Color.Red)
                    {
                        lev.BackColor = Color.Transparent;
                    }
                }

                if (Math.Round(mapControl1.Map.Scale) >= 200000)
                {
                    level1.BackColor = Color.Red;
                    iLevel = 1;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 200000 && Math.Round(mapControl1.Map.Scale) >= 100000)
                {
                    level2.BackColor = Color.Red;
                    iLevel = 2;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 100000 && Math.Round(mapControl1.Map.Scale) >= 50000)
                {
                    level3.BackColor = Color.Red;
                    iLevel = 3;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 50000 && Math.Round(mapControl1.Map.Scale) >= 20000)
                {
                    level4.BackColor = Color.Red;
                    iLevel = 4;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 20000 && Math.Round(mapControl1.Map.Scale) >= 10000)
                {
                    level5.BackColor = Color.Red;
                    iLevel = 5;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 10000 && Math.Round(mapControl1.Map.Scale) >= 5000)
                {
                    level6.BackColor = Color.Red;
                    iLevel = 6;
                }
                else
                {
                    level7.BackColor = Color.Red;
                    iLevel = 7;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "根据比例尺指定地图级别");
            }
            return iLevel;
        }

        #region 浮动地图工具栏事件方法
        /// <summary>
        /// 点击浮动工具实现地图缩放
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void level1_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 200000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level1_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图缩放
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void level2_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 100000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level2_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图缩放
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void level3_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 50000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level3_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图缩放
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void level4_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 20000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level4_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图缩放
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void level5_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 10000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level5_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图缩放
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void level6_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 5000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level6_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图缩放
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void level7_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 2000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level7_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图放大
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void bZoomOut_Click(object sender, EventArgs e)
        {
            try
            {
                //mapControl1.Map.Scale = mapControl1.Map.Scale * 2;
                switch (iMapLevel)
                {
                    case 1:
                        mapControl1.Map.Scale = mapControl1.Map.Scale * 2;
                        break;
                    case 2:
                        mapControl1.Map.Scale = 200000;
                        break;
                    case 3:
                        mapControl1.Map.Scale = 100000;
                        break;
                    case 4:
                        mapControl1.Map.Scale = 50000;
                        break;
                    case 5:
                        mapControl1.Map.Scale = 20000;
                        break;
                    case 6:
                        mapControl1.Map.Scale = 10000;
                        break;
                    case 7:
                        mapControl1.Map.Scale = 5000;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "bZoomOut_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图缩小
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void bZoomIn_Click(object sender, EventArgs e)
        {
            try
            {
                //mapControl1.Map.Scale = mapControl1.Map.Scale / 2;
                switch (iMapLevel)
                {
                    case 1:
                        mapControl1.Map.Scale = 100000;
                        break;
                    case 2:
                        mapControl1.Map.Scale = 50000;
                        break;
                    case 3:
                        mapControl1.Map.Scale = 20000;
                        break;
                    case 4:
                        mapControl1.Map.Scale = 10000;
                        break;
                    case 5:
                        mapControl1.Map.Scale = 5000;
                        break;
                    case 6:
                        mapControl1.Map.Scale = 2000;
                        break;
                    case 7:
                        mapControl1.Map.Scale = mapControl1.Map.Scale / 2;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "bZoomIn_Click");
            }
        }

        /// <summary>
        /// 点击浮动工具实现地图全图查看
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void bCenter_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["街镇面"]);
            }
            catch (Exception ex) { ExToLog(ex, "bCenter_Click"); }
        }

        /// <summary>
        /// 点击浮动工具实现地图先左移动
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void bLeft_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x - mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bLeft_Click"); }
        }

        /// <summary>
        /// 点击浮动工具实现地图先右移动
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void bRight_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x + mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bRight_Click"); }
        }

        /// <summary>
        /// 点击浮动工具实现地图先下移动
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void bDown_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y - mapControl1.Map.Bounds.Height() / 4);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bDown_Click"); }
        }

        /// <summary>
        /// 点击浮动工具实现地图先上移动
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void bUp_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y + mapControl1.Map.Bounds.Height() / 4);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bUp_Click"); }
        }
        #endregion

        private DockingManager dockingManager;

        public VisualStyle frmVS = VisualStyle.Office2007Blue;

        private System.Windows.Forms.Label lblDistance = new System.Windows.Forms.Label();
        private System.Windows.Forms.Label lblArea = new System.Windows.Forms.Label();
        private Content cMessage=null;
        private Content cDistance;
        private Content cArea;
        /// <summary>
        /// 初始测量窗体
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void InitDocument()
        {
            try
            {
                this.toolStripContainer1.Visible = false;
                this.dockingManager = new DockingManager(this.toolStripContainer1.ContentPanel, this.frmVS);
                this.dockingManager.AllowRedocking = false;

                this.lblDistance.BackColor = Color.White;
                this.lblDistance.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
                this.lblDistance.TextAlign = ContentAlignment.MiddleLeft;

                this.lblArea.BackColor = Color.White;
                this.lblArea.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
                this.lblArea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

                this.cDistance = this.dockingManager.Contents.Add(this.lblDistance, "距离");
                this.cDistance.FloatingSize = new Size(250, 50);

                this.cArea = this.dockingManager.Contents.Add(this.lblArea, "面积");
                this.cArea.FloatingSize = new Size(250, 50);

                this.dockingManager.AddContentWithState(cMessage, State.Floating);
                this.dockingManager.AddContentWithState(cDistance, State.Floating);
                this.dockingManager.AddContentWithState(cArea, State.Floating);

                this.dockingManager.HideAllContents(true);
            }
            catch (Exception ex)
            {
                ExToLog(ex,"InitDocument");
            }
        }

        /// <summary>
        /// 切换模块
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="userControl">要显示的模块</param>
        private void ChangeFunctionItem(UserControl userControl)
        {
            this.mapControl1.Tools.LeftButtonTool = "Select";
            try
            {
                //如果是信息工具，换成选择工具    
                if (((ToolStripButton)this.toolStrip1.Items["toolInfo"]).Checked)
                {
                    ((ToolStripButton)this.toolStrip1.Items["toolInfo"]).Checked = false;
                    ((ToolStripButton)this.toolStrip1.Items["toolSel"]).Checked = true;
                    this.mapControl1.Tools.LeftButtonTool = "select";
                }
                
                //for (int i = 0; i < funcArr.Length; i++)
                //{
                //    funcArr[i].Visible = false;
                //}
                for (int i = 0; i < splitContainer2.Panel1.Controls.Count; i++)
                {
                    splitContainer2.Panel1.Controls[i].Visible = false;

                    this.toolvideo.Visible = false;
                }

                userControl.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ChangeFunctionItem-切换模块");
            }
        }

        /// <summary>
        /// 综合查询模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuZonghe_Click(object sender, EventArgs e)
        {
            try
            {
                if (fZonghe.Visible) return;

                string[] strName = new string[] { "点击查询", "周边查询", "音头查询", "高级查询" };
                FeatureLayer featureLay = null;
                for (int i = 0; i < strName.Length; i++)
                {
                    featureLay = (FeatureLayer)mapControl1.Map.Layers[strName[i]];
                    if (featureLay == null)
                        fZonghe.CreateTemLayer(strName[i]);
                }

                ChangeFunctionItem(funcArr[0]);
                if (fZonghe.dtExcel != null) fZonghe.dtExcel.Clear();
                this.toolvideo.Visible = true;
                RemoveTemLayer("临时图层");
                WriteEditLog("综合查询");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuZonghe_Click-综合模块");
            }
        }

        /// <summary>
        /// 案件分析模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuAnjian_Click(object sender, EventArgs e)
        {
            try
            {
                if (fAnjian.Visible) return;
                ChangeFunctionItem(funcArr[1]);
                CreateTemLayer("layerLinShi", "临时图层");
                if (fAnjian.dtExcel != null) fAnjian.dtExcel.Clear();
                WriteEditLog("案件分析");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuAnjian_Click-案件模块");
            }
        }

        /// <summary>
        /// 车辆监控模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuCar_Click(object sender, EventArgs e)
        {
            try
            {
                if (fCar.Visible) return;
                ChangeFunctionItem(funcArr[2]);
                UncheckedTool();
                toolSel.Checked = true;
                fCar.StartTimeCar();
                RemoveTemLayer("临时图层");
                WriteEditLog("车辆监控");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuCar_Click-车辆模块");
            }
        }


        /// <summary>
        /// 警员监控模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
         private void ToolGPSPolice_Click(object sender, EventArgs e)  //jie.zhang 20091230 GPS警员
        {
            try
            {
                if (fGPSp.Visible) return;
                ChangeFunctionItem(funcArr[9]);
                UncheckedTool();
                this.toolvideo.Visible = true;
                toolSel.Checked = true;
                fGPSp.InitGpsPolice();
                RemoveTemLayer("临时图层");
                WriteEditLog("GPS警员监控");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ToolGPSPolice_Click-警员模块");
            }
        }

        /// <summary>
        /// 110接处警模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void Menu110_Click(object sender, EventArgs e)   //jie.zhang 20091230 110接处警
        {
            try
            {
                if (f110.Visible) return;
                ChangeFunctionItem(funcArr[8]);
                UncheckedTool();
                f110.Init110();
                this.toolvideo.Visible = true;
                toolSel.Checked = true;
                RemoveTemLayer("临时图层");
                WriteEditLog("110接处警");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Menu110_Click-110模块");
            }
        }

        /// <summary>
        /// 视频监控模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuVideo_Click(object sender, EventArgs e)
        {
            try
            {
                if (fVideo.Visible) return;
                ChangeFunctionItem(funcArr[3]);
                UncheckedTool();
                fVideo.CreateVideoLayer();
                toolSel.Checked = true;
                this.toolvideo.Visible = true;
                RemoveTemLayer("临时图层");
                WriteEditLog("视频监控");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuVideo_Click-视频监控模块");
            }
        }

        /// <summary>
        /// 治安卡口模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuKakou_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.fKakou.Visible) return;
                ChangeFunctionItem(funcArr[7]);
                fKakou.InitKK();
                CreateTemLayer("layerViewSel", "查看选择");
                RemoveTemLayer("临时图层");
                this.toolvideo.Visible = true;

                WriteEditLog("治安卡口");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuKakou_Click-治安卡口模块");
            }
        }

        /// <summary>
        /// 人口管理模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuPopulation_Click(object sender, EventArgs e)
        {
            try
            {
                if (fPopu.Visible) return;
                CreateTemLayer("layerPopu", "人口系统");
                ChangeFunctionItem(funcArr[4]);
                RemoveTemLayer("临时图层");
                WriteEditLog("人口系统");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuPopulation_Click-人口模块");
            }
        }

        /// <summary>
        /// 房屋管理模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuHouse_Click(object sender, EventArgs e)
        {
            try
            {
                if (fHouse.Visible) return;
                CreateTemLayer("layerHouse", "房屋系统");
                ChangeFunctionItem(funcArr[5]);
                RemoveTemLayer("临时图层");
                WriteEditLog("房屋系统");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuHouse_Click-房屋模块");
            }
        }

        /// <summary>
        /// 直观指挥模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuCommand_Click(object sender, EventArgs e)
        {
            try
            {
                if (fZhihui.Visible) return;
                fZhihui.InitZhihui();

                ChangeFunctionItem(funcArr[6]);
                this.toolvideo.Visible = true;
                RemoveTemLayer("临时图层");
                WriteEditLog("直观指挥");
            }
            catch (Exception ex) 
            {
                ExToLog(ex, "MenuCommand_Click-直观指挥模块");
            }
        }

        /// <summary>
        /// 创建临时图层
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="tableAiles">图层名</param>
        private void CreateTemLayer(string tablename, string tableAiles)
        {
            try
            {
                //   create   a   temp   layer   as   the   rectangle   holder 
                TableInfoMemTable ti = new TableInfoMemTable(tableAiles);
                ti.Temporary = true;

                //   add   columns   
                Column column;
                column = new GeometryColumn(mapControl1.Map.GetDisplayCoordSys());
                column.Alias = "MI_Geometry";
                column.DataType = MIDbType.FeatureGeometry;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "MI_Style";
                column.DataType = MIDbType.Style;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "MapID";
                column.DataType = MIDbType.Int;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "表_ID";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "名称";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "表名";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                Table table;
                try
                {
                    //   create   table   and   feature   layer 
                    table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                }
                catch
                {
                    table = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);
                }
                FeatureLayer temLayer = new FeatureLayer(table);

                mapControl1.Map.Layers.Insert(0, temLayer);
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "CreateTemLayer-创建临时图层");
            }
        }

        /// <summary>
        /// 移除临时图层,关闭表
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="tableAlies">图层名</param>
        private void RemoveTemLayer(string tableAlies)
        {
            try
            {
                if (mapControl1.Map.Layers[tableAlies] != null)
                {
                    mapControl1.Map.Layers.Remove(tableAlies);
                    MapInfo.Engine.Session.Current.Catalog.CloseTable(tableAlies);
                }
            }
            catch(Exception ex) {
                ExToLog(ex, "RemoveTemLayer-移除临时图层");
            }
        }

        /// <summary>
        /// 清除层中所有要素
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="alies">图层名称</param>
        private void clearFeatures(string alies)
        {
            try
            {
                //清除地图上添加的对象
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers[alies];
                if (fl == null) return;
                Table tableTem = fl.Table;

                //先清除已有对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch(Exception ex) {
                ExToLog(ex, "clearFeatures-清除层中所有要素");
            }
        }

        /// <summary>
        /// 退出系统 
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            try
            {
                fVideo.StopVideo();
                setOnline(frmLogin.string用户名称, 0);
                fAnjian.closeGLThread();
                Application.ExitThread();
                this.Dispose();
                Application.Exit();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemExit_Click-退出");
            }
        }

        /// <summary>
        /// 切换用户
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuItemChangeUser_Click(object sender, EventArgs e)   //切换用户
        {
            try
            {
                FrmChangeUser frmChangeUser = new FrmChangeUser(frmLogin.temDt, frmLogin.temRegionDt, ConStr,frmLogin.temEditDt);
                if (frmChangeUser.ShowDialog(this) == DialogResult.OK)
                {
                    frmLogin.temDt = FrmChangeUser.temDt1;
                    frmLogin.temRegionDt = FrmChangeUser.temRegionDt1;
                    frmLogin.temEditDt = FrmChangeUser.temEditDt;
                    frmLogin.region1 = FrmChangeUser.region1;
                    frmLogin.region2 = FrmChangeUser.region2;
                    InitPrivilege();   //设置权限
                    this.toolStripUser.Text = "用户：" + FrmChangeUser.string用户名称;

                    setOnline(frmLogin.string用户名称, 0);  //上一用户下线
                    frmLogin.string用户名称 = FrmChangeUser.string用户名称;
                    setOnline(frmLogin.string用户名称, 1);  //新用户上线

                    setUserSearchRegion(FrmChangeUser.string用户名称); //根据用户所属区设置查询范围.
                    try
                    {
                        //下面代码设置新用户的初始功能项
                        if (this.MenuZonghe.Enabled)
                        {
                            if (fZonghe.Visible == false)
                            {
                                //CreateTemLayer("layerZonghe", "综合查询");
                                ChangeFunctionItem(funcArr[0]);
                            }
                        }

                        else if (this.MenuAnjian.Enabled)
                        {
                            if (fAnjian.Visible == false)
                            {
                                //CreateTemLayer("layerAnjian", "案件分析");
                                ChangeFunctionItem(funcArr[1]);
                            }
                        }

                        else if (this.ToolGPSCar.Enabled)
                        {
                            if (fCar.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[2]);
                                fCar.StartTimeCar();
                            }
                        }

                        else if (this.MenuVideo.Enabled)
                        {
                            if (fVideo.Visible == false)
                            {
                                fVideo.CreateVideoLayer();
                                ChangeFunctionItem(funcArr[3]);  //jie.zhang 2008.9.22 注释--改变弹出窗口
                            }
                        }

                        else if (this.MenuKakou.Enabled)
                        {
                            if (this.fKakou.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[7]);
                                fKakou.InitKK();
                                this.SetLayerEdit("KKLayer");
                                CreateTemLayer("layerViewSel", "查看选择");
                                WriteEditLog("治安卡口");
                            }
                        }
                        else if (this.Menu110.Enabled)
                        {
                            if (this.f110.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[8]);
                                f110.Init110();
                                WriteEditLog("110接处警");
                            }
                        }
                        else if (this.ToolGPSPolice.Enabled)
                        {
                            if (this.fGPSp.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[9]);
                                fGPSp.InitGpsPolice();
                                WriteEditLog("GPS警员");
                            }
                        }
                        else if (this.MenuItemPop.Enabled)
                        {
                            if (fPopu.Visible == false)
                            {
                                CreateTemLayer("layerPopu", "人口系统");
                                ChangeFunctionItem(funcArr[4]);
                            }
                        }

                        else if (this.MenuItemHouse.Enabled)
                        {
                            if (fHouse.Visible == false)
                            {
                                CreateTemLayer("layerHouse", "房屋系统");
                                ChangeFunctionItem(funcArr[5]);
                            }
                        }

                        else if (this.MenuCommand.Enabled)
                        {
                            if (fZhihui.Visible == false)
                            {
                                fZhihui.InitZhihui();
                                ChangeFunctionItem(funcArr[6]);
                            }
                        }
                        else if (this.menuDataEdit.Enabled)
                        {
                            if (fGISEdit.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[10]);
                            }
                        }
                    }
                    catch(Exception ex) 
                    {
                        ExToLog(ex, "MenuItemChangeUser_Click-切换用户-初始化");
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemChangeUser_Click-切换用户");
            }
        }

        /// <summary>
        /// 设置用户所属区域
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="user">用户名</param>
        private void setUserSearchRegion(string user)
        {
            try
            {
                fZonghe.strRegion = frmLogin.temRegionDt.Rows[0]["综合查询"].ToString().Trim();
                fAnjian.strRegion = frmLogin.temRegionDt.Rows[0]["案件分析"].ToString().Trim();
                fCar.strRegion = frmLogin.temRegionDt.Rows[0]["车辆监控"].ToString().Trim();
                fZhihui.StrRegion = frmLogin.temRegionDt.Rows[0]["直观指挥"].ToString().Trim();
                fVideo.strRegion = frmLogin.temRegionDt.Rows[0]["视频监控"].ToString().Trim();
                fHouse.strRegion = frmLogin.temRegionDt.Rows[0]["房屋管理"].ToString().Trim();
                fPopu.strRegion = frmLogin.temRegionDt.Rows[0]["人口管理"].ToString().Trim();
                fKakou.strRegion = frmLogin.temRegionDt.Rows[0]["治安卡口"].ToString().Trim();  //jie.zhang 20090709 
                fGPSp.StrRegion = frmLogin.temRegionDt.Rows[0]["GPS警员"].ToString().Trim();
                f110.strRegion = frmLogin.temRegionDt.Rows[0]["llo接警"].ToString().Trim();

                fZonghe.strRegion1 = frmLogin.temRegionDt.Rows[1]["综合查询"].ToString().Trim();
                fAnjian.strRegion1 = frmLogin.temRegionDt.Rows[1]["案件分析"].ToString().Trim();
                fCar.strRegion1 = frmLogin.temRegionDt.Rows[1]["车辆监控"].ToString().Trim();
                fZhihui.StrRegion1 = frmLogin.temRegionDt.Rows[1]["直观指挥"].ToString().Trim();
                fVideo.strRegion1 = frmLogin.temRegionDt.Rows[1]["视频监控"].ToString().Trim();
                fHouse.strRegion1 = frmLogin.temRegionDt.Rows[1]["房屋管理"].ToString().Trim();
                fPopu.strRegion1 = frmLogin.temRegionDt.Rows[1]["人口管理"].ToString().Trim();
                fKakou.strRegion1 = frmLogin.temRegionDt.Rows[1]["治安卡口"].ToString().Trim();
                fGPSp.StrRegion1 = frmLogin.temRegionDt.Rows[1]["GPS警员"].ToString().Trim();
                f110.strRegion1 = frmLogin.temRegionDt.Rows[1]["llo接警"].ToString().Trim();    

                #region 导入导出
                fZonghe.strRegion2 = frmLogin.temRegionDt.Rows[0]["可导出"].ToString().Trim(); //派出所可导出区域
                fZonghe.strRegion3 = frmLogin.temRegionDt.Rows[1]["可导出"].ToString().Trim(); //中队可导出区域
                fAnjian.strRegion2 = frmLogin.temRegionDt.Rows[0]["可导出"].ToString().Trim(); //派出所可导出区域
                fAnjian.strRegion3 = frmLogin.temRegionDt.Rows[1]["可导出"].ToString().Trim(); //中队可导出区域
                fCar.strRegion2 = frmLogin.temRegionDt.Rows[0]["可导出"].ToString().Trim(); //派出所可导出区域
                fCar.strRegion3 = frmLogin.temRegionDt.Rows[1]["可导出"].ToString().Trim(); //中队可导出区域
                fVideo.strRegion2 = frmLogin.temRegionDt.Rows[0]["可导出"].ToString().Trim(); //派出所可导出区域
                fVideo.strRegion3 = frmLogin.temRegionDt.Rows[1]["可导出"].ToString().Trim(); //中队可导出区域
                fPopu.strRegion2 = frmLogin.temRegionDt.Rows[0]["可导出"].ToString().Trim(); //派出所可导出区域
                fPopu.strRegion3 = frmLogin.temRegionDt.Rows[1]["可导出"].ToString().Trim(); //中队可导出区域
                fHouse.strRegion2 = frmLogin.temRegionDt.Rows[0]["可导出"].ToString().Trim(); //派出所可导出区域
                fHouse.strRegion3 = frmLogin.temRegionDt.Rows[1]["可导出"].ToString().Trim(); //中队可导出区域
                f110.strRegion2 = frmLogin.temRegionDt.Rows[0]["可导出"].ToString().Trim(); //派出所可导出区域   jie.zhang 20100621
                f110.strRegion3 = frmLogin.temRegionDt.Rows[1]["可导出"].ToString().Trim(); //中队可导出区域
                #endregion

                fZonghe.user = user;
                fAnjian.user = user;
                fPopu.user = user;
                fHouse.user = user;
                fZhihui.User = user;
                fCar.user = user;
                fVideo.user = user;

                fKakou.user = user;   //jie.zhang 20090709 
                f110.user = user;     //jie.zhang 20091230
                fGPSp.User = user;    //jie.zhang 20091230

                fVideo.SetUserRegion();
                fCar.SetUserRegion();

                fAnjian.InitialCX3CComboBoxText(frmLogin.temRegionDt.Rows[0]["案件分析"].ToString().Trim());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setUserSearchRegion");
            }

            ////判断是否具有导出权限
            //OracleConnection conn = new OracleConnection(getStrConn());
            //try
            //{
                //conn.Open();
                //OracleCommand cmd = new OracleCommand("select 导出 from 用户 where username = '" + user + "'", conn);
                //OracleDataReader dr = cmd.ExecuteReader();
            //    if (dr.HasRows)
            //    {
            //        dr.Read();
            //        if (dr.GetValue(0).ToString() == "1")
            //        {
            //            toolDateOut.Enabled = true;
            //        }
            //        else
            //        {
            //            toolDateOut.Enabled = false;
            //        }
            //    }
            //    dr.Dispose();
            //    cmd.Dispose();
            //    conn.Close();
            //}
            //catch(Exception ex)
            //{ conn.Close();
            //ExToLog(ex, "判断导出权限");
            //}
        }
       
        /// <summary>
        /// 更改用户权限
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuItemAlterType_Click(object sender, EventArgs e)
        {
            try
            {

                FrmManager frmManager = new FrmManager();
                //frmManager.Width = 280;
                frmManager.conStr = ConStr;
                frmManager.setDataGridView();
                frmManager.initRoleList();
                frmManager.user = frmLogin.string用户名称;
                frmManager.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemAlterType_Click-更改用户权限");
            }
        }       

        /// <summary>
        /// 添加角色
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuItemAddRole_Click(object sender, EventArgs e)
        {
            try
            {
                FrmRole fRole = new FrmRole();
                fRole.conStr = ConStr;
                fRole.setListbox();
                fRole.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemAddRole_Click-添加角色");
            }
        }

        /// <summary>
        /// 更改用户密码
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuItemAlterPW_Click(object sender, EventArgs e)
        {
            try
            {
                FrmChangePassword frmChangePassword = new FrmChangePassword();
                frmChangePassword.cbUser.Text = frmLogin.string用户名称;
                frmChangePassword.conStr = ConStr;
                frmChangePassword.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemAlterPW_Click-更改用户密码");
            }
        }

        /// <summary>
        /// 创建连接信息日志
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="msg">异常源</param>
        public void writelog(string msg)
        {
            StreamWriter sw = null;
            try
            {
                string filepath = Application.StartupPath + "\\rec.log";
                msg = DateTime.Now.ToString() + ":" + msg;

                sw = File.AppendText(filepath);
                sw.WriteLine(DateTime.Now.ToString() + ": " + msg);
                sw.Flush();
                sw.Dispose();
                sw.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "writelog");
            }
        }            

        /// <summary>
        /// 关于按钮点击事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                frmAbout fAbout = new frmAbout();
                fAbout.ShowDialog();
            }
            catch(Exception ex) { ExToLog(ex, "aboutToolStripMenuItem_Click"); }
        }

        /// <summary>
        /// 按键控制地图
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void mapControl1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DPoint dp;
                switch (e.KeyCode.ToString())
                {
                    case "Up":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y + mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Down":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y - mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Left":
                        dp = new DPoint(mapControl1.Map.Center.x - mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Right":
                        dp = new DPoint(mapControl1.Map.Center.x + mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Oemplus":
                    case "Add":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale / 2f;
                        break;
                    case "OemMinus":
                    case "Subtract":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale * 2f;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "mapControl1_KeyDown");
            }
        }

        /// <summary>
        /// MD5加密
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="s">需加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public string md5(string s)
        {
            try
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
                bytes = md5.ComputeHash(bytes);

                md5.Clear();

                string ret = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    ret += Convert.ToString(bytes[i], 16).PadLeft(2, '0');
                }
                return ret.PadLeft(32, '0');
            }
            catch (Exception ex) { ExToLog(ex, "md5"); return null; }
        }

        //----------======-----------
        //建立监控客户端网络监听
        //===========================
        private TcpListener tcpserver1 = null;
        private System.Net.Sockets.TcpListener tcpListener1 = null;
        private System.Threading.Thread ServerThread1 = null;

        //停止服务标志    
        public bool Stop1 = false;
        private TcpClient tcpClient1 = null;
        private bool TcpClose1=false;
        public System.Threading.Thread tcpClientThread1 = null;
        //启动客户连接线程  

        private Boolean VideoFlag1 = false;

        public void CreateVideoSocket()
        {
            try
            {
                StopThread();
            }
            catch
            {
                writelog("创建监听前释放所有监听数据时发生错误");
            }
            try
            {    //取主机名    
                string host1 = Dns.GetHostName();
                //解析本地IP地址，    
                IPHostEntry hostIp1 = Dns.GetHostEntry(host1);
                writelog(string.Format("服务器: {0}", host1));
                writelog("服务器地址:" + hostIp1.AddressList[0].ToString());
            }
            catch (Exception x)
            {               //如果这个时候都出错，就别想活了   ，还是退出    
                writelog(x.Message);
                return;
            }

            try
            {
                IPAddress ip1 = IPAddress.Parse("127.0.0.1");                
                tcpserver1 = new TcpListener(ip1, this.videop);
                StartServer1();
            }
            catch (Exception x)
            {
                this.writelog(x.Message);
            }
        }

        // ThreadServer
        //===================    

        public void StartServer1()
        {
            try
            {
               if (this.ServerThread1 != null)
                {
                    return;
                }
                writelog(string.Format("New TcpListener....Port={0}", this.tcpserver1.LocalEndpoint.ToString()));
      
                tcpserver1.Start();
                //生成线程，start（）函数为线程运行时候的程序块    
                this.ServerThread1 = new Thread(new ThreadStart(startsever1));
                ////线程的优先级别    
                //this.ServerThread.Priority = ThreadPriority.BelowNormal;
                this.ServerThread1.IsBackground = true;
                this.ServerThread1.Start();
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
        }

        private void startsever1()
        {
            try
            {
                while (!Stop1)   //几乎是死循环    
                {
                    writelog("正在等待连接......");
                    ShowDoInfo(this.tcpserver1.LocalEndpoint.ToString());
                    tcpClient1 = tcpserver1.AcceptTcpClient();
                    writelog("已经建立连接......");
                    ShowDoInfo("已与监控客户端建立连接");
                    this.VideoFlag1 = true;
                    fVideo.getNetParameter(networkStream1, VideoFlag1);
                    tcpClientThread1 = new Thread(new ThreadStart(startclient1));
                    tcpClientThread1.IsBackground = true;
                    tcpClientThread1.Start();
                    if (Stop1) break;
                }
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
            finally
            {
                this.tcpserver1.Stop();
            }
        }

        private NetworkStream networkStream1 = null;
        private byte[] buf1 = new byte[1024 * 1024];   //预先定义1MB的缓冲  
        private int Len1 = 0;   //流的实际长度  

        //读写连接的函数，用于线程    
        private void startclient1()
        {
            networkStream1 = tcpClient1.GetStream();   //建立读写Tcp的流    

            fVideo.getNetParameter(networkStream1, VideoFlag1);
            writelog(" Video 已经传输视频流");
            
            fZhihui.getNetParameter(networkStream1, VideoFlag1);
            writelog(" Zhihu 已经传输视频流");

            fKakou.getNetParameter(networkStream1, VideoFlag1);
            writelog(" KaKou 已经传输视频流");
            
            f110.getNetParameter(networkStream1, VideoFlag1);
            writelog(" 110 已经传输视频流");
            
            fGPSp.getNetParameter(networkStream1, VideoFlag1);
            writelog(" GPSPolice 已经传输视频流");

            try
            {
                //开始循环读写tcp流    
                while (!TcpClose1)
                {
                    //如果当前线程是在其它状态，（等待挂起，等待终止.....)就结束该循环    
                    //if (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.Running)
                    //    break;

                    //判断Tcp流是否有可读的东西    
                    if (networkStream1.DataAvailable)
                    {
                        //从流中读取缓冲字节数组    
                        Len1 = networkStream1.Read(buf1, 0, buf1.Length);
                        //转化缓冲数组为串    
                        byte[] temp1 = new byte[Len1];

                        for (int i = 0; i < Len1; i++)
                        {
                            temp1[i] = buf1[i];

                        }
                        ShowDoInfo("接收数据成功！");
                        // AnalyData(temp1);//解析收到的命令                           
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(200d));
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                writelog(e.Message);
            }
            catch (ThreadAbortException y)
            {
                writelog(y.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                networkStream1.Close();
                tcpClient1.Close();
                writelog("关闭连接......");
            }
        }

        private void StopThread()
        {
            try
            {
                if (ServerThread1 != null)
                {
                    ServerThread1.Abort();
                }

                if (tcpClientThread1 != null)
                {
                    tcpClientThread1.Abort();
                }

                if (tcpClient1 != null)
                {
                    tcpClient1.Close();
                }

                if (tcpserver1 != null)
                {
                    tcpserver1.Stop();
                }

                if (tcpListener1 != null)
                {
                    tcpListener1.Stop();
                }

                //Application.ExitThread();
                //this.Dispose();
            }
            catch (Exception ex)
            { writelog(ex.Message); }
        }
       
        /// <summary>
        /// 显示信息
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="str">显示的信息</param>
        private void ShowDoInfo(string str)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    DecshowMessage dc = new DecshowMessage(ShowDoInfo);
                    this.BeginInvoke(dc, new object[] { str });

                }
                else
                    //this.toolStatusInfo.Text = "信息:" + str;
                    this.toolStripInfo.Text = "信息:" + str;
            }
            catch (Exception ex) { ExToLog(ex, "ShowDoInfo"); }
        }
        delegate void DecshowMessage(string str);

        /// <summary>
        /// 帮助按钮点击事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void MenuItemHelp_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(exePath + "\\顺德公安警用地理信息系统操作手册.chm");
            }
            catch(Exception ex) {
                ExToLog(ex, "MenuItemHelp_Click");
            }
        }

        /// <summary>
        /// 数据编辑模块加载显示
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void menuDataEdit_Click(object sender, EventArgs e)
        {
            try
            {
                #region  未合并之前的编辑模块
                //string ZoomFiles = Application.StartupPath + "\\ConfigBJXX.ini";        // 给编辑模块传入地址用于读取地图的缩放比例值
                //Cursor.Current = Cursors.WaitCursor;
                //CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                //DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select 真实姓名 from 用户 where USERnAME='" + frmLogin.string用户名称 + "'");

                //LBSgisPoliceEdit.frmMap frm = new LBSgisPoliceEdit.frmMap(frmLogin.region1, frmLogin.region2, frmLogin.temEditDt);
                //frm.userName = table.Rows[0][0].ToString();
                //frm.ZoomFile = ZoomFiles;
                //frm.Show();
                //Cursor.Current = Cursors.Default;
                #endregion

                #region 合并后的编辑模块
                if (this.fGISEdit.Visible) return;
                ChangeFunctionItem(funcArr[10]);
                RemoveTemLayer("临时图层");
                #endregion

                WriteEditLog("数据编辑");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "menuDataEdit_Click-编辑模块");
            }
        }

        /// <summary>
        /// 窗体关闭事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void frmMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                fAnjian.closeGLThread();
                fCar.Dispose();// StopTimeCar();
                fVideo.Dispose();
                setOnline(frmLogin.string用户名称, 0);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmMap_FormClosing-关闭主窗体");
            }
        }

        /// <summary>
        /// 设置用户在线状态
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="i">表明是否在线（0为离线，1为在线）</param>
        private void setOnline(string userName,int i)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.OracleComRun("update 用户 set 在线=" + i + " where USERNAME='" + userName + "'");

                if (i == 1)
                {
                    WriteEditLog("登录系统");
                }
                else {
                    WriteEditLog("退出系统");
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "setOnline");
            }
        }

        #region 检测服务器 add by siumo 090121
        private bool IsWebResourceAvailable(string webResourceAddress)
        {
            TcpClient tcpClient=null;
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(webResourceAddress), 80);
                tcpClient = new TcpClient();
                tcpClient.Connect(ipep);
                
                //tcpClient.GetStream();

                return true;
                //HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(webResourceAddress);
                //req.Method = "HEAD";
                //req.Timeout = 15000;
                //HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                //return (res.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "IsWebResourceAvailable");

                return false;
            }
            finally {
                tcpClient.Close();
            }
        }

        private void timeIP_Tick(object sender, EventArgs e)
        {
             //checkServerComputer();
        }
        #endregion

        /// <summary>
        /// 输入比例尺按回车键发生
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolStripcbScale_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    this.mapControl1.Map.Scale = Convert.ToDouble(toolStripcbScale.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolStripcbScale_KeyPress-输入比例尺按回车键");
            }
        }

        /// <summary>
        /// 选择地图比例尺
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolStripcbScale_SelectedIndexChanged(object sender, EventArgs e)
        { 
            try
            {
                this.mapControl1.Map.Scale = Convert.ToDouble(toolStripcbScale.Text.Trim());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolStripcbScale_SelectedIndexChanged-选择地图比例尺");
            }
        }

        frmOnlineUsers fOnlineUsers = new frmOnlineUsers();
        /// <summary>
        /// 查看在线用户
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolStripOnlineUsers_Click(object sender, EventArgs e)
        {
            try
            {

                if (fOnlineUsers != null)
                {
                    fOnlineUsers.Close();
                }
                fOnlineUsers = new frmOnlineUsers();
                fOnlineUsers.Left = Screen.PrimaryScreen.WorkingArea.Width;

                fOnlineUsers.strConn = strConn;
                fOnlineUsers.TopMost = true;
                fOnlineUsers.Show();
                //让窗体逐渐滑出
                for (int i = 0; i < 15; i++)
                {
                    fOnlineUsers.Left = fOnlineUsers.Left - 14;
                    Thread.Sleep(50);
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolStripOnlineUsers_Click-查看在线用户");
            }
        }

        string sMod;
        /// <summary>
        /// 操作记录日志输入
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        /// <param name="sModule">功能模块名称</param>
        private void WriteEditLog(string sModule)
        {
            try
            {
                sMod=sModule;
                string strExe = "insert into 操作记录 values('" + frmLogin.string用户名称 + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'" + sModule + "','登录功能项','')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "WriteEditLog-操作记录");
            }
        }

        /// <summary>
        /// 操作记录日志输入
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        /// <param name="strTabName">操作表名</param>
        /// <param name="sql">操作的sql语句</param>
        /// <param name="p_3">操作方式</param>
        private void writeEditLog(string strTabName, string sql, string p_3)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + frmLogin.string用户名称 + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'" + sMod + "','"+strTabName+":"+sql+"','"+p_3+"')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex) { ExToLog(ex, "WriteEditLog-操作记录"); }
        }

        private void MenuItemViewLog_Click(object sender, EventArgs e)
        {
            try
            {
                frmEditLog fLog = new frmEditLog(ConStr);
                fLog.ShowDialog(this);
            }
            catch (Exception ex) { ExToLog(ex, "MenuItemViewLog_Click-操作记录"); }
        }


        private MapInfo.Geometry.DPoint KKdp;
        private IResultSetFeatureCollection rfc = null;
        System.Drawing.Point point;
        /// <summary>
        /// 地图右键菜单
        /// 最后更新人 李立
        /// 最后更新时间 2011-2-14 
        /// </summary>
        private void mapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    this.mapControl1.ContextMenuStrip = this.contextMenuStrip1;

                    string VideoTableName = string.Empty;
                    string VideoColumName = string.Empty;

                    point = e.Location;
                    MapInfo.Geometry.DPoint dp;
                    mapControl1.Map.DisplayTransform.FromDisplay(point, out dp);

                    Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 30);

                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dp, mapControl1.Map.GetDisplayCoordSys(), d);
                    si.QueryDefinition.Columns = null;

                    this.toolopenvideo.Visible = false;
                    this.menuSearchDist.Visible = false;

                    this.toolOpenClient.Visible = false;
                    this.toolVideoset.Visible = false;
                    this.toolDownVideo.Visible = false;
                    this.toolRect.Visible = false;             // 框选
                    this.menuItemBayonetDire.Visible = false;  // lili 2010-12-20 查看卡口方向
                    this.toolMessage.Visible = false;          // lili 2011-01-04 查看该对象的详细信息
                    selcamerid = "";

                    if (fVideo.Visible)  //jie.zhang 20101216 右键框选
                    {
                        this.toolRect.Visible = true;
                    }

                    //拉框选出视频时
                    if (this.mapControl1.Map.Layers["查看选择"] != null)
                    {
                        try
                        {
                            Irfc = rfc = Session.Current.Catalog.Search("查看选择", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    foreach (Feature f in rfc)
                                    {
                                        if (Convert.ToString(f["表名"]) == "视频")
                                        {
                                            selcamerid = Convert.ToString(f["表_ID"]);
                                            this.toolopenvideo.Visible = true;
                                            this.toolOpenClient.Visible = true;
                                            this.toolVideoset.Visible = true;
                                            this.toolDownVideo.Visible = true;
                                            this.toolMessage.Visible = true;
                                            break;
                                        }
                                        else if (Convert.ToString(f["表名"]) == "警车")
                                        {
                                            string sId = Convert.ToString(f["表_ID"]);
                                            this.toolMessage.Visible = true;
                                            DataTable ttTab = GetTable("select * from GPS警车定位系统 where 终端ID号码='" + sId + "'");
                                            if (ttTab != null && ttTab.Rows.Count > 0)
                                            {
                                                selcamerid = ttTab.Rows[0]["CAMID"].ToString();
                                                break;
                                            }
                                        }
                                        else if (fKakou.Visible)
                                        {
                                            this.toolMessage.Visible = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    //治安卡口中是否有卡口, 有则显示查看周边
                    if (fKakou.Visible && this.mapControl1.Map.Layers["KKLayer"] != null) {
                        try
                        {
                            rfc = Session.Current.Catalog.Search("KKLayer", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    this.menuSearchDist.Visible = true;
                                    this.menuItemBayonetDire.Visible = true;  // 2010-12-20 lili 查看卡口方向
                                    this.toolMessage.Visible = true;
                                    this.KKdp = dp;
                                }
                            }
                        }
                        catch { }
                    }

                    //if (f110.Visible && this.mapControl1.Map.Layers["SocketLayer"] != null)
                    //{
                    //    try
                    //    {
                    //        rfc = Session.Current.Catalog.Search("SocketLayer", si);
                    //        if (rfc != null)
                    //        {
                    //            if (rfc.Count > 0)
                    //            {
                    //                this.menuSearchDist.Visible = true;
                    //                this.KKdp = dp;
                    //                return;
                    //            }
                    //        }
                    //    }
                    //    catch { }
                    //}

                    //如果selcamerid有值,说明已经找到视频,不再判断其他视频点
                    if (selcamerid != "") return;

                    if (this.mapControl1.Map.Layers["VideoLayer"] != null || this.mapControl1.Map.Layers["VideoCarLayer"] != null)
                    {
                        if(this.mapControl1.Map.Layers["VideoLayer"] != null)
                        {
                            //this.toolOpenClient.Visible = true;
                            //this.toolVideoset.Visible = true;
                            //this.toolopenvideo.Visible = true;
                            //this.toolDownVideo.Visible = true;
                            
                            rfc = Session.Current.Catalog.Search("VideoLayer", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    foreach (Feature f in rfc)
                                    {
                                        selcamerid = Convert.ToString(f["设备编号"]);
                                        if (selcamerid!="")
                                        {
                                           
                                            this.toolopenvideo.Visible = true;
                                            this.toolDownVideo.Visible = true;
                                            this.toolOpenClient.Visible = true;
                                            this.toolVideoset.Visible = true;
                                            return;  //如果selcamerid有值,跳出
                                        }
                                    }
                                }
                            }
                        }
                        if(this.mapControl1.Map.Layers["VideoCarLayer"] != null)
                        {
                            rfc = Session.Current.Catalog.Search("VideoCarLayer", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    foreach (Feature f in rfc)
                                    {
                                        selcamerid = Convert.ToString(f["设备编号"]);
                                        if (selcamerid!="")
                                        {
                                            this.toolOpenClient.Visible = true;
                                            this.toolVideoset.Visible = true;
                                            this.toolopenvideo.Visible = true;
                                            this.toolDownVideo.Visible = true;
                                            return;  //如果selcamerid有值,跳出
                                        }
                                    }
                                }
                            }
                        }
                    }

                    else if (this.mapControl1.Map.Layers["queryLayer"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("查询表", si);
                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    if (Convert.ToString(f["设备名称"]) != "")
                                    {
                                        selcamerid = Convert.ToString(f["设备编号"]);
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (this.mapControl1.Map.Layers["视频"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("视频", si);
                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    selcamerid = Convert.ToString(f["设备编号"]);
                                    if (selcamerid != "")
                                    {
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        break;  //如果selcamerid有值,跳出
                                    }
                                }
                            }
                        }
                    }
                    else if (this.mapControl1.Map.Layers["CarLayer"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("CarLayer", si);
                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    selcamerid = Convert.ToString(f["CAMERID"]);
                                    if (selcamerid.Length > 5)
                                    {
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        fVideo.getVideoparam(VideoTableName, VideoColumName);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (this.mapControl1.Map.Layers["综合查询"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("综合查询", si);

                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    if (Convert.ToString(f["表名"]) == "视频位置")
                                    {
                                        selcamerid = Convert.ToString(f["表_ID"]);
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        fVideo.getVideoparam(VideoTableName, VideoColumName);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                    }

                }
                catch(Exception ex)
                {
                    this.contextMenuStrip1.Visible = false;
                    ExToLog(ex, "右键操作");
                }
            }
            else
            {
				this.contextMenuStrip1.Visible = false;
            }
        }

        string selcamerid = "";

        /// <summary>
        /// 查看视频
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        private void toolopenvideo_Click(object sender, EventArgs e)
        {
            try
            {
                fVideo.getNetParameter(networkStream1, VideoFlag1);
                bool sendflag = fVideo.OpenVideo(selcamerid);
                if (sendflag)
                {
                    this.WindowState = FormWindowState.Minimized;
                }
                writeEditLog("视频位置", "视频编号=" + selcamerid, "查看视频");
            }
            catch (Exception ex)
            {
                writelog("打开视频时发生错误--" + ex.Message);
            }
        }

        /// <summary>
        /// 右键周边查询
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        private void menuSearchDist_Click(object sender, EventArgs e)
        {
            try
            {
                fKakou.SearchDistance(this.KKdp, this.KKSearchDist);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "menuSearchDist_Click");
            }
        }

        /// <summary>
        /// 右键另存为图片
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        private void conmenuPic_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog MyDlg = new SaveFileDialog();
                MyDlg.AddExtension = true;
                MyDlg.DefaultExt = "jpg";
                MyDlg.FileName = string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                MyDlg.Filter = "图像文件(Jpg)|*.jpg";
                if (MyDlg.ShowDialog() == DialogResult.OK)
                {
                    string MyFileName = MyDlg.FileName;

                    Map map = (Map)this.mapControl1.Map.Clone();

                    MapExport printer = new MapExport(map);

                    printer.ExportSize = new ExportSize(1024, 768);

                    printer.Format = ExportFormat.Jpeg;

                    printer.Export(MyFileName);

                    MessageBox.Show("保存完毕", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                MessageBox.Show("保存图片时发生错误，无法完成保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 导出按钮点击事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        private void toolDateOut_Click(object sender, EventArgs e)
        {
            try
            {
                string tableName = "";
                if (_exportDT != null)
                    _exportDT.Clear();

                if (fZonghe.Visible)
                {
                    //_exportDT = fZonghe.dtExcel;
                    exportSQL = fZonghe.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                    if (fZonghe.tabControl1.SelectedIndex == 0)
                    {
                        if (fZonghe.comboBox1.Text == "全部")
                        {
                            MessageBox.Show("请选择一个表进行导出(不能选择'全部')", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        tableName = fZonghe.comboBox1.Text;

                    }
                    else if (fZonghe.tabControl1.SelectedIndex == 2)
                    {
                        tableName = fZonghe.comboClass.Text;
                    }
                    else if (fZonghe.tabControl1.SelectedIndex == 3)
                    {
                        tableName = fZonghe.comboTable.Text;
                        if (fZonghe.comboTable.Text == "信息点")
                            _exportDT = fZonghe.dtExcel;
                    }
                    else
                    {
                        MessageBox.Show("当前没有可导出的数据表!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (fZonghe.strRegion != fZonghe.strRegion2 || fZonghe.strRegion1 != fZonghe.strRegion3)
                    {
                        MessageBox.Show("您的导出权限小于查询权限,导出结果与当前列表可能不一致!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fAnjian.Visible)
                {
                    tableName = "案件信息";
                    //_exportDT = fAnjian.dtExcel;
                    exportSQL = fAnjian.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                    if (fAnjian.strRegion != fAnjian.strRegion2 || fAnjian.strRegion1 != fAnjian.strRegion3)
                    {
                        MessageBox.Show("您的导出权限小于查询权限,导出结果与当前列表可能不一致!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fCar.Visible)
                {
                    tableName = "GPS警车定位系统";
                    //_exportDT = fCar.dtExcel;
                    _exportDT = GetExcelDataTable(fCar._startNo, fCar._endNo, fCar.excelSql);
                    if (fCar.strRegion != fCar.strRegion2 || fCar.strRegion1 != fCar.strRegion3)
                    {
                        MessageBox.Show("您的导出权限小于查询权限,导出结果与当前列表可能不一致!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fGPSp.Visible)
                {
                    tableName = "警员定位系统";
                    _exportDT = fGPSp.dtExcel;
                    if (fCar.strRegion != fCar.strRegion2 || fCar.strRegion1 != fCar.strRegion3)
                    {
                        MessageBox.Show("您的导出权限小于查询权限,导出结果与当前列表可能不一致!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fVideo.Visible)
                {
                    tableName = "视频位置";
                    //_exportDT = fVideo.dtExcel;
                    _exportDT = GetExcelDataTable(fVideo._startNo, fVideo._endNo, fVideo.excelSql);

                }
                else if (fKakou.Visible)
                {
                    tableName = "治安卡口系统";
                    _exportDT = fKakou.dtExcel;
                }
                else if (fPopu.Visible)
                {
                    tableName = "人口系统";
                    //_exportDT = fPopu.dtExcel;
                    exportSQL = fPopu.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                }
                else if (fHouse.Visible)
                {
                    tableName = "出租屋房屋系统";
                    //_exportDT = fHouse.dtExcel;
                    exportSQL = fHouse.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                }
                else if (fZhihui.Visible)
                {
                    tableName = "GPS110.报警信息110";
                    _exportDT = fZhihui.dtExcel;
                }
                else if (f110.Visible)　　　　　　　　　　　　　　　//jie.zhang 20100621  数据导出
                {
                    tableName = "GPS110.报警信息110";
                    _exportDT = f110.dtExcel;
                }

                if (_exportDT != null && _exportDT.Rows.Count > 0)
                {
                    DataExport(tableName);
                }
                else
                {
                    MessageBox.Show("当前列表为空", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "导出数据");
            }
        }

        /// <summary>
        /// 获取导出数据
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        /// <param name="_startNo">开始数</param>
        /// <param name="_endNo">结束数</param>
        /// <param name="excelSQL">导出完整SQL</param>
        /// <returns>数据集</returns>
        private DataTable GetExcelDataTable(int _startNo, int _endNo, string excelSQL)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                if (excelSQL == string.Empty)
                {
                    System.Windows.Forms.MessageBox.Show("请先查询出数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                    return null;
                }
                DataTable dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = excelSQL;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(_startNo, _endNo,dtInfo);

                Cmd.Dispose();
                Conn.Close();

                return dtInfo;
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                ExToLog(ex, "GetExcelDataTable");
                return null;
            }
        }

        /// <summary>
        /// 获取导出数据
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24 
        /// </summary>
        /// <param name="excelSQL">导出完整SQL</param>
        /// <returns>数据集</returns>
        private DataTable GetExcelDataTable(string excelSQL)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                if (excelSQL == string.Empty)
                {
                    System.Windows.Forms.MessageBox.Show("请先查询出数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                DataTable dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = excelSQL;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(dtInfo);

                Cmd.Dispose();
                Conn.Close();

                return dtInfo;
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                ExToLog(ex, "GetExcelDataTable");
                return null;
            }
        }

        private DataTable _exportDT = null;

        /// <summary>
        /// 导出操作
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="tableName">表名</param>
        private void DataExport(string tableName)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "请选择将导出的EXCEL文件存放路径";
                sfd.Filter = "Excel文档(*.xls)|*.xls";
                sfd.FileName = "EXP" + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                {
                    if (sfd.FileName.LastIndexOf(".xls") <= 0)
                    {
                        sfd.FileName = sfd.FileName + ".xls";
                    }
                    string fileName = sfd.FileName;
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;

                    if (dg.OutData(fileName, _exportDT, tableName))
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("导出Excel完成!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("导出Excel失败!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "DataExport");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 导入代码 （此功能已屏蔽）
        /// </summary>
        private void toolImport_Click(object sender, EventArgs e)
        {
            #region 导入代码
            //try
            //{
            //    if (fZonghe.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "请选择将导入的EXCEL文件路径";
            //        ofd.Filter = "Excel文档(*.xls)|*.xls";
            //        string tableName = "";
            //        if (fZonghe.tabControl1.SelectedIndex == 0)
            //        {
            //            if (fZonghe.comboBox1.Text == "全部")
            //            {
            //                MessageBox.Show("请选择一个表进行导入(不能选择'全部')", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                return;
            //            }
            //            ofd.FileName = fZonghe.comboBox1.Text;
            //            tableName = fZonghe.comboBox1.Text;

            //        }
            //        else if (fZonghe.tabControl1.SelectedIndex == 2)
            //        {
            //            GetFromName getFromName = new GetFromName(fZonghe.comboClass.Text);
            //            ofd.FileName = getFromName.TableName;
            //            tableName = getFromName.TableName;
            //        }
            //        //else if (fZonghe.tabControl1.SelectedIndex == 3)
            //        //{
            //        //    ofd.FileName = "安全防护单位";
            //        //    tableName = "安全防护单位";
            //        //}
            //        else if (fZonghe.tabControl1.SelectedIndex == 3)
            //        {
            //            GetFromName getFromName = new GetFromName(fZonghe.comboTable.Text);
            //            ofd.FileName = getFromName.TableName;
            //            tableName = getFromName.TableName;
            //        }
            //        else
            //        {
            //            MessageBox.Show("当前没有需要导入的数据表!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //            return;
            //        }

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }

            //    }
            //    else if (fAnjian.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "请选择将导入的EXCEL文件路径";
            //        ofd.Filter = "Excel文档(*.xls)|*.xls";
            //        ofd.FileName = "案件信息";
            //        string tableName = "案件信息";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }
            //    }
            //    else if (fCar.Visible)
            //    {
            //        MessageBox.Show("当前没有需要导入的数据表!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }
            //    else if (fVideo.Visible)
            //    {
            //        MessageBox.Show("当前没有需要导入的数据表!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //        /*
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "请选择将导入的EXCEL文件路径";
            //        ofd.Filter = "Excel文档(*.xls)|*.xls";
            //        ofd.FileName = "视频位置";
            //        string tableName = "视频位置";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }
            //        return;
            //        */
            //    }
            //    else if (fKakou.Visible)
            //    {
            //        MessageBox.Show("当前没有需要导入的数据表!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }
            //    else if (fPopu.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "请选择将导入的EXCEL文件路径";
            //        ofd.Filter = "Excel文档(*.xls)|*.xls";
            //        ofd.FileName = "人口系统";
            //        string tableName = "人口系统";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }
            //    }
            //    else if (fHouse.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "请选择将导入的EXCEL文件路径";
            //        ofd.Filter = "Excel文档(*.xls)|*.xls";
            //        ofd.FileName = "出租屋房屋系统";
            //        string tableName = "出租屋房屋系统";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }

            //    }
            //    else
            //    {
            //        MessageBox.Show("当前没有需要导入的数据表!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ExToLog(ex, "toolImport_Click");
            //}
            #endregion
        }
     
        /// <summary>
        /// 连接110 （此功能用于测试）
        /// </summary>
        private void 连接110ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = string.Empty;

                Test test = new Test();
                test.TopMost = true;
                if (test.ShowDialog(this) == DialogResult.OK)
                {
                    msg = test.msg;
                    if (msg.Length > 0)
                        Receiver110Data(msg);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "连接110ToolStripMenuItem_Click");
            }
        }     

        private void axNetcomm1_DataArrived(object sender, AxNETCOMMLib._DNetcommEvents_DataArrivedEvent e)
        {
            writelog("110Socket接收到的数据：" + e.dataBuf);

            try
            {
                if (!string.IsNullOrEmpty(e.dataBuf))
                {
                    //案件编号@发案地点详址@简要案情@案件名称@所属派出所@所属中队@对讲机ID@案件来源@报警时间@X@Y@
                    Receiver110Data(e.dataBuf);
                }
            }
            catch (Exception ex)
            {
                writelog("判断110数据处理方式"+ex.ToString());
            }
        }
      

        private void Receiver110Data(string socketData)
        {
            try
            {

                string msg = socketData;

                string[] message = msg.Split('@');

                int i = Convert.ToInt32(message[0].Substring(0, 1));

                if(i!=1 && i!=2)
                {
                    MessageBox.Show(@"数据格式错误，无法处理");
                    return;
                }

                
                if (i == 2)
                    message = Get110Msg(message[0]);

                if (message == null) return;

                string bmpName = f110.GetBmpName(message[7]);

                //message[0]  案件编号
                //message[1]  发案地点详址  110
                //message[2]  简要案情
                //message[3]  案件名称
                //message[4]  所属派出所
                //message[5]  所属中队
                //message[6]  对讲机ID
                //message[7]  案件来源
                //message[8]  报警时间
                //message[9]  X
                //message[10]  Y

                // 0-5       "案件名称,报警编号,案件状态,案件类型,案别_案由,专案标识," +
                //6-11           "发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地点详址," +
                //12-17           "所属社区,简要案情,作案手段特点,发案地政区划,发案场所,案件来源," +
                // 18-20          "报警来源,报警时间, 所属中队," +
                //     21      " 所属派出所," +
                //     22-25      "所属警务室, 对讲机ID,x,y ";


                switch (_schnum)
                {
                    case 1: //直观指挥

                        if (fZhihui.Visible == false)
                        {
                            fZhihui.InitZhihui();

                            ChangeFunctionItem(funcArr[6]);
                            RemoveTemLayer("临时图层");
                            WriteEditLog("直观指挥");
                        }

                        fZhihui.Deal110Msg(bmpName, message);

                        break;
                    case 2: //匹配数据
                        if (f110.Visible == false)
                        {
                            ChangeFunctionItem(funcArr[8]);
                            UncheckedTool();
                            f110.Init110();
                            toolSel.Checked = true;
                            RemoveTemLayer("临时图层");
                        }

                        f110.DealSocket(bmpName, message, "匹配数据");

                        break;
                    case 3: //坐标修正

                        if (f110.Visible == false)
                        {
                            ChangeFunctionItem(funcArr[8]);
                            UncheckedTool();
                            f110.Init110();
                            toolSel.Checked = true;
                            RemoveTemLayer("临时图层");
                        }

                        f110.DealSocket(bmpName, message, "坐标修正");
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex,"判断数据并处理");
            }
        }

        private string[] Get110Msg(string msg)
        {
            //message[0]  报警编号  110
            //message[1]  发案地点详址  110
            //message[2]  简要案情
            //message[3]  案件名称
            //message[4]  所属派出所
            //message[5]  所属中队
            //message[6]  对讲机ID
            //message[7]  案件来源
            //message[8]  报警时间
            //message[9]  X
            //message[10]  Y
            string[] mg = null;

            try
            {
                string ajbh = msg.Substring(1, msg.Length - 1);
                string sql = "Select t.发案地点详址,t.简要案情,t.案件名称,t.所属派出所," +
                             "t.所属中队,t.对讲机ID,t.报警来源,t.报警时间," +
                             "t.X,t.Y from GPS110.报警信息110 t where t.报警编号='" + ajbh + "'";
                DataTable dt = GetTable(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string fadz = Convert.ToString(dr["发案地点详址"]);
                        string jyaq = Convert.ToString(dr["简要案情"]);
                        string ajmc = Convert.ToString(dr["案件名称"]);
                        string sspcs = Convert.ToString("所属派出所");
                        string sszd = Convert.ToString("所属中队");
                        string callid = Convert.ToString(dr["对讲机ID"]);
                        string bjly = Convert.ToString(dr["报警来源"]);
                        string bjsj = Convert.ToString(dr["报警时间"]);
                        try
                        {
                            string sx = Convert.ToString(dr["X"]);
                            string sy = Convert.ToString(dr["Y"]);
                            mg = new string[] {ajbh, fadz, jyaq, ajmc, sspcs, sszd, callid, bjly, bjsj, sx, sy};
                            
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(@"该报警编号对应的数据无坐标值，请确认存在此数据", @"系统提示", MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"没有与此报警编号对应的信息", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show(@"数据为非法数据，无法处理:"+msg, @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ExToLog(ex, @"根据报警编号提取数据时发出错误:" + msg);
            }
            return mg;
        }


        private void axNetcomm1_NetMessage(object sender, AxNETCOMMLib._DNetcommEvents_NetMessageEvent e)
        {
            try
            {
                switch (e.msgCode)
                {
                    case 0: break;
                    case 1: break;
                    case 2:
                        RecToLog("110连接成功！");
                        toolStripInfo.Text = @"信息：110连接成功";
                        break;//与接处警程序连接上
                    case 3: RecToLog("110连接断开！"); toolStripInfo.Text = @"信息：110连接断开"; break;// 与接处警程序连接断开
                    case 4: RecToLog("110连接失败！"); toolStripInfo.Text = @"信息：110连接失败"; break; // 与接处警程序的连接出错
                    case 5: break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writelog("判断110是否连通"+ex.ToString());
            }
        }

        private void frmMap_Load(object sender, EventArgs e)
        {
           
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    string s = "1A000000@上海@有人抢接@软件园@普陀@金沙江@ID101@100报警@2010-2-3@113.239@22.8375";
        //    writelog("110Socket接收到的数据：" +s);

        //    if (f110.Visible) return;
        //    else
        //    {
        //        ChangeFunctionItem(funcArr[8]);
        //        UncheckedTool();
        //        f110.Init110();
        //        toolSel.Checked = true;
        //        RemoveTemLayer("临时图层");
        //    }

        //    if (s != null && s.Length > 0)
        //    {
        //        f110.DealSocket(s);
        //    }
        //}

        //private void button1_Click_1(object sender, EventArgs e)
        //{
        //    string s = "1A000000@上海@有人抢接@软件园@普陀@金沙江@ID101@100报警@2010-2-3@113.239@22.8375";
        //    writelog("110Socket接收到的数据：" + s);

        //    if (f110.Visible == false)
        //    {
        //        ChangeFunctionItem(funcArr[8]);
        //        UncheckedTool();
        //        f110.Init110();
        //        toolSel.Checked = true;
        //        RemoveTemLayer("临时图层");
        //    }

        //    if (s != null && s.Length > 0)
        //    {
        //        f110.DealSocket(s);
        //    }
        //}



        /// <summary>
        /// 查询SQL
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 异常日志
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPolice-frmMap-" + sFunc);
        }

        /// <summary>
        /// 110连接日志
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="s">日志内容</param>
        private void RecToLog(string s)
        {
            CLC.BugRelated.LogWrite(s, Application.StartupPath + "\rec.log");
        }

        /// <summary>
        /// 记录操作记录
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="sql">操作sql语句</param>
        /// <param name="method">方法名</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + frmLogin.string用户名称 + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'主程序',''" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch { }
        }

        /// <summary>
        /// 配置管理-110监控设置
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void menu110set_Click(object sender, EventArgs e)
        {
            try
            {
                FrmScheme frmScheme = new FrmScheme();
                frmScheme.SchemeNmuber = _schnum;
                if (frmScheme.ShowDialog(this) != DialogResult.OK) return;
                _schnum = frmScheme.SchemeNmuber;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                CLC.INIClass.IniWriteValue("110", "模式", _schnum.ToString());
            }
            catch (Exception ex)
            {
                writelog("保存110处理模式时发生错误"+ex.ToString());
            }
        }

        /// <summary>
        /// 配置管理-缩放比例尺设置
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void 缩放比例尺ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string scale = "";
                frmScale frmscale = new frmScale();
                frmscale.scale = scale;
                if (frmscale.ShowDialog(this) != DialogResult.OK) return;
                scale = frmscale.scale;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                CLC.INIClass.IniWriteValue("比例尺", "缩放比例", scale);
            }
            catch (Exception ex)
            {
                writelog("保存缩放比例时发生错误" + ex.ToString());
            }
        }

        /// <summary>
        /// 用时测试日志
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="message">异常信息</param>
        /// <param name="funName">任务名称</param>
        private void WriteLog(string message, string funName)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\timeTestLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine("任务:" + funName + "  　     " + message);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            { ExToLog(ex, "用时测试日志"); }
        }

        /// <summary>
        /// 打开视频客户端
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolOpenClient_Click(object sender, EventArgs e)
        {try
            {
                fVideo.OpenVideoClient();
            }
            catch (Exception ex)
            { ExToLog(ex, "toolOpenClient_Click"); }
        }

        /// <summary>
        /// 视频监控设置
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                fVideo.videoset();
            }
            catch (Exception ex)
            { ExToLog(ex, "toolStripMenuItem3_Click"); }
        }

        /// <summary>
        /// 视频下载
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.selcamerid != "")
                {
                    fVideo.DownRecord(selcamerid);
                }
            }
            catch (Exception ex)
            { ExToLog(ex, "toolStripMenuItem2_Click"); }
        }

        /// <summary>
        /// 关闭错误提示面板
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                this.panErrorMessage.Visible = false;
            }
            catch (Exception ex)
            { ExToLog(ex, "linkClose_LinkClicked"); }
        }

        /// <summary>
        /// 右击框选
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolRect_Click(object sender, EventArgs e)
        {
            try
            {
                this.mapControl1.Tools.LeftButtonTool = "SelectRect";
            }
            catch (Exception ex)
            { ExToLog(ex, "toolRect_Click"); }
        }

        /// <summary>
        /// 查看卡口方向
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void menuItemBayonetDire_Click(object sender, EventArgs e)
        {
            try
            {
                fKakou.AddBayoneDireCond(rfc);
            }
            catch (Exception ex)
            { ExToLog(ex, "menuItemBayonetDire_Click"); }
        }

        private IResultSetFeatureCollection Irfc = null;
        /// <summary>
        /// 查看详细信息
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void toolMessage_Click(object sender, EventArgs e)
        {
            try
            {
                if (Irfc != null)
                    if (Irfc.Count > 0)
                        fKakou.getMessage(Irfc, point);

                if (rfc != null)
                    if (rfc.Count > 0)
                        fKakou.getMessage(rfc, point);
            }
            catch (Exception ex)
            { 
                ExToLog(ex, "toolMessage_Click");
            }
        }
    }
}