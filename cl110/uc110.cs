﻿using System;
using System.Data;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;

using CLC;
using clVideo;
using MapInfo.Data;
using MapInfo.Engine;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Styles;
using MapInfo.Text;
using MapInfo.Tools;
using MapInfo.Windows.Controls;
using Point = System.Drawing.Point;
using System.Data.OracleClient;
using clZhihui;



namespace cl110
{
    public partial class uc110 : UserControl
    {
        private string mysqlstr;                      // 数据库连接字符串
        private  string[] ConStr; 　　　　　　　　　　// 数据库连接字符组 
        private  string VEpath = string.Empty; 　　　 // 监控客户端文件目录


        private  int VideoPort;                       // 通讯端口
        private  string[] VideoString;                // 视频连接字符
        private  frmalarm frmarm = new frmalarm();
        private  frmBus frmbus = new frmBus();
        private  ToolStripLabel st;                   // 状态栏
        private  ToolStripDropDownButton tdb;         // 工具栏下拉菜单
        private double videodis;                      // 周边查询视频半径 
        private string getFromNamePath = string.Empty; // 配置文件getFromNameConfig.ini的地址
        public MapControl MapControl1;                // 地图控件
        public SplitContainer split;                  // 主程序面板 
        public System.Windows.Forms.Panel panError;   // 用于关联时传入直观指挥的错误提示面板

        /// <summary>
        /// 显示车辆详细信息
        /// </summary>
        private FrmInfo frminfo = new FrmInfo();

        private int iflash;
        private static NetworkStream ns; //网络数据流 

        private IResultSetFeatureCollection rsfcflash;
        public string strRegion = string.Empty; // 派出所权限
        public string strRegion1 = "";   // 中队权限
        public string user = ""; // 用户姓名 

        private Boolean vf;      // 通讯是否已经连接的标识


        //导出变量
        public string strRegion2 = ""; // 可导出的派出所
        public string strRegion3 = ""; // 可导出的中队
        public string excelSql = "";   // 查询导出sql
        public System.Data.DataTable dtExcel = null; // 地图页面数据导出表
        OracleDataAdapter apt1 = null;

        public ToolStripProgressBar toolPro;  // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolProLbl;     // 用于显示进度文本　
        public ToolStripSeparator toolProSep;
         

        /// <summary>
        ///   传递参数
        /// </summary>
        /// <param name="m">当前地图</param>
        /// <param name="s">数据库连接字符串</param>
        /// <param name="tools">状态栏状态</param>
        /// <param name="td">下拉菜单</param>
        /// <param name="port">通讯端口</param>
        /// <param name="vs">视频客户端登陆参数</param>
        /// <param name="videopath">客户端启动位置</param>
        /// <param name="dist">查询半径</param>
        public uc110(MapControl m, string[] s, ToolStripLabel tools, ToolStripDropDownButton td, int port, string[] vs,
                     string videopath, double dist,string getFromPath)
        {
            InitializeComponent();
            try
            {
                //赋值传参
                MapControl1 = m;
                ConStr = s;
                mysqlstr = "data source=" + ConStr[0] + ";user id=" + ConStr[1] + ";password=" + ConStr[2];
                st = tools;
                tdb = td;
                VideoPort = port;
                VideoString = vs;
                VEpath = videopath;
                videodis = dist;
                getFromNamePath = getFromPath;

                //初始化自定义工具
                MapTool ptMapTool = new CustomPointMapTool(false, MapControl1.Tools.FeatureViewer,
                                                           MapControl1.Handle.ToInt32(), MapControl1.Tools,
                                                           MapControl1.Tools.MouseToolProperties,
                                                           MapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                MapControl1.Tools.Add("MoveTool1", ptMapTool);

                //初始化自定义工具
                MapTool ptMapTool2 = new CustomPointMapTool(false, MapControl1.Tools.FeatureViewer,
                                                            MapControl1.Handle.ToInt32(), MapControl1.Tools,
                                                            MapControl1.Tools.MouseToolProperties,
                                                            MapControl1.Tools.MapToolProperties);
                ptMapTool2.UseDefaultCursor = false;
                ptMapTool2.Cursor = Cursors.Cross;
                MapControl1.Tools.Add("MoveTool2", ptMapTool2);


                //初始化自定义工具
                MapTool ptMapTool3 = new CustomPointMapTool(false, MapControl1.Tools.FeatureViewer,
                                                            MapControl1.Handle.ToInt32(), MapControl1.Tools,
                                                            MapControl1.Tools.MouseToolProperties,
                                                            MapControl1.Tools.MapToolProperties);
                ptMapTool3.UseDefaultCursor = false;
                ptMapTool3.Cursor = Cursors.Cross;
                MapControl1.Tools.Add("MoveTool3", ptMapTool3);

                //地图工具使用事件
                MapControl1.Tools.Used += Tools_Used;
                //this.mapControl1.MouseMove += new MouseEventHandler(Tool_MouseMove);

                this.cmbField.Items.Clear();

                this.cmbField.Items.Add("报警编号");
                this.cmbField.Items.Add("案件名称");
                this.cmbField.Items.Add("案件类型");
                this.cmbField.Items.Add("案别_案由");
                this.cmbField.Items.Add("专案标识");
                this.cmbField.Items.Add("发案时间初值");
                this.cmbField.Items.Add("发案时间终值");
                this.cmbField.Items.Add("发案地点详址");
                this.cmbField.Items.Add("简要案情");
                this.cmbField.Items.Add("报警时间");
                this.cmbField.Items.Add("对讲机ID");
                this.cmbField.Items.Add("处理警力编号");
                this.cmbField.Items.Add("所属派出所");
                this.cmbField.Items.Add("所属中队");
                this.cmbField.Items.Add("所属警务室");
                this.cmbField.Text = this.cmbField.Items[0].ToString();
                this.bindingNavigator2.Visible = true;


                this.comboBox1.Items.Clear();

                this.comboBox1.Items.Add("报警编号");
                this.comboBox1.Items.Add("案件名称");
                this.comboBox1.Items.Add("案件类型");
                this.comboBox1.Items.Add("案别_案由");
                this.comboBox1.Items.Add("专案标识");
                this.comboBox1.Items.Add("发案时间初值");
                this.comboBox1.Items.Add("发案时间终值");
                this.comboBox1.Items.Add("发案地点详址");
                this.comboBox1.Items.Add("简要案情");
                this.comboBox1.Items.Add("报警时间");
                this.comboBox1.Items.Add("对讲机ID");
                this.comboBox1.Items.Add("处理警力编号");
                this.comboBox1.Items.Add("所属派出所");
                this.comboBox1.Items.Add("所属中队");
                this.comboBox1.Items.Add("所属警务室");
                this.comboBox1.Text = this.comboBox1.Items[0].ToString();
                this.bindingNavigator3.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "01-传递参数");
            }
        }

        /// <summary>
        /// 传递网络参数
        /// </summary>
        /// <param name="ns1"></param>
        /// <param name="vf1"></param>
        /// 因为这里的参数会根据通讯的不同操作而改变，所以会在参数的其它时候进行传递
        public void getNetParameter(NetworkStream ns1, Boolean vf1)
        {
            try
            {
                ns = ns1;
                vf = vf1;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "02-传递网络参数");
            }
        }


        /// <summary>
        /// 初始化110模块
        /// </summary>
        public void Init110()
        {
            try
            {
               
                    if (MapControl1.Map.Layers["JCLayer"] != null)
                    {
                        Session.Current.Catalog.CloseTable("JCLayer");
                    }

                    if (MapControl1.Map.Layers["JCLabel"] != null)
                    {
                        MapControl1.Map.Layers.Remove("JCLabel");
                    }

                    if (MapControl1.Map.Layers["SocketLayer"] != null)
                    {
                        Session.Current.Catalog.CloseTable("SocketLayer");
                    }

                    if (MapControl1.Map.Layers["SocketLabel"] != null)
                    {
                        MapControl1.Map.Layers.Remove("SocketLabel");
                    }
                
               
                //给关联窗体传递参数
                frmbus.Visible = false;
                frmbus.ConStr = ConStr;
                frmbus.UserName = user;
                frmarm.Visible = false;

                //timer1.Interval = 10*1000;
                //timer1.Enabled = true;

                //timer2.Interval = 500;
                //timer2.Enabled = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "03-初始化110模块");
            }
        }


        /// <summary>
        /// 校正坐标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            try
            {
                if (Visible)
                    switch (e.ToolName)
                    {
                        case "MoveTool1":
                            switch (e.ToolStatus)
                            {
                                case ToolStatus.Start:

                                    try
                                    {
                                        Feature f;

                                        Table editTable = Session.Current.Catalog.GetTable("SocketLayer");

                                        #region   删除选择的点

                                        string tempname = "";
                                        tempname = dgvip.Rows[0].Cells[0].Value.ToString();
                                        if (tempname != "")
                                        {
                                            const string tblname = "SocketLayer";

                                            //提取当前选择的信息的通行车辆编号作为主键值

                                            Map map = MapControl1.Map;

                                            if ((Session.Current.MapFactory.Count == 0) || (map == null))
                                            {
                                                return;
                                            }
                                            rsfcflash = null;

                                            MIConnection miconn = new MIConnection();
                                            miconn.Open();

                                            MICommand micmd = miconn.CreateCommand();
                                            micmd.CommandText = "select * from " +
                                                                MapControl1.Map.Layers[tblname].ToString() +
                                                                " where AID = @name ";
                                            micmd.Parameters.Clear();
                                            micmd.Parameters.Add("@name", tempname);

                                            try
                                            {
                                                rsfcflash = micmd.ExecuteFeatureCollection();


                                                if (rsfcflash.Count > 0)
                                                {
                                                    Style s = null;

                                                    Session.Current.Selections.DefaultSelection.Clear();
                                                    Session.Current.Selections.DefaultSelection.Add(
                                                        rsfcflash);
                                                    foreach (Feature ft in rsfcflash)
                                                    {
                                                        s = ft.Style;
                                                        editTable.DeleteFeature(ft);
                                                        //mapControl1.Map.Center = ft.Geometry.Centroid;
                                                        break;
                                                    }

                                                    #region 添加点

                                                    f = new Feature(editTable.TableInfo.Columns);
                                                    f.Geometry =
                                                        new MapInfo.Geometry.Point(MapControl1.Map.GetDisplayCoordSys(),
                                                                                   new DPoint(e.MapCoordinate.x,
                                                                                              e.MapCoordinate.y));
                                                    f["Name"] = dgvip.Rows[0].Cells[3].Value.ToString();
                                                    f["AID"] = dgvip.Rows[0].Cells[0].Value.ToString();
                                                    f["CallID"] = dgvip.Rows[0].Cells["对讲机ID"].Value.ToString();
                                                    f.Style = s;
                                                    editTable.InsertFeature(f);

                                                    #endregion

                                                    
                                                        #region 更新列表中的XY值

                                                        dgvip.Rows[0].Cells["X"].Value = e.MapCoordinate.x.ToString();
                                                        dgvip.Rows[0].Cells["Y"].Value = e.MapCoordinate.y.ToString();

                                                        frmbus.x = e.MapCoordinate.x;
                                                        frmbus.y = e.MapCoordinate.y;

                                                        //if (MessageBox.Show(@"是否保存当前坐标修改？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                                                        //{
                                                            if (dgvip.CurrentRow != null)
                                                                Update110XY(dgvip.CurrentRow.Cells["报警编号"].Value.ToString(),
                                                                            dgvip.CurrentRow.Cells["X"].Value.ToString(),
                                                                            dgvip.CurrentRow.Cells["Y"].Value.ToString());
                                                        //    MessageBox.Show("保存成功");
                                                        //}

                                                        #endregion                                                     
                                                    
                                                }
                                            }
                                            catch
                                            {
                                            }
                                            finally
                                            {
                                                micmd.Dispose();
                                                miconn.Close();
                                            }

                                            #endregion
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ExToLog(ex, "移动图元时发生错误");
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "MoveTool2":
                            switch (e.ToolStatus)
                            {
                                case ToolStatus.Start:

                                    try
                                    {
                                        Feature f;
                                        Table editTable = Session.Current.Catalog.GetTable("JCLayer");

                                        string tempname = "";
                                        if (this.dgvNoip.CurrentRow != null)
                                        {
                                            tempname = dgvNoip.CurrentRow.Cells[0].Value.ToString();
                                            if (tempname != "")
                                            {
                                                const string tblname = "JCLayer";
                                                Map map = MapControl1.Map;
                                                if ((Session.Current.MapFactory.Count == 0) || (map == null))
                                                {
                                                    return;
                                                }
                                                rsfcflash = null;
                                                MIConnection miconn = new MIConnection();
                                                miconn.Open();
                                                MICommand micmd = miconn.CreateCommand();
                                                micmd.CommandText = "select * from " +
                                                                    MapControl1.Map.Layers[tblname].ToString() +
                                                                    " where AID = @name ";
                                                micmd.Parameters.Clear();
                                                micmd.Parameters.Add("@name", tempname);

                                                try
                                                {
                                                    Style s = null;
                                                    rsfcflash = micmd.ExecuteFeatureCollection();
                                                    if (rsfcflash.Count < 0) return;

                                                    if (rsfcflash.Count > 0)     //更新点的位置
                                                    {
                                                        //删除选中的点
                                                        foreach (Feature ft in rsfcflash)
                                                        {
                                                            s = ft.Style;
                                                            editTable.DeleteFeature(ft);
                                                            break;
                                                        }

                                                        #region 添加点

                                                        f = new Feature(editTable.TableInfo.Columns);
                                                        f.Geometry =
                                                            new MapInfo.Geometry.Point(MapControl1.Map.GetDisplayCoordSys(),
                                                                                       new DPoint(e.MapCoordinate.x,
                                                                                                  e.MapCoordinate.y));
                                                        f["Name"] = dgvNoip.CurrentRow.Cells[1].Value.ToString();
                                                        f["AID"] = dgvNoip.CurrentRow.Cells[0].Value.ToString();
                                                        f.Style = s;
                                                        editTable.InsertFeature(f);

                                                        #endregion


                                                        #region 更新列表中的XY值


                                                        this.dgvNoip.CurrentRow.Cells["X"].Value = e.MapCoordinate.x.ToString();
                                                        this.dgvNoip.CurrentRow.Cells["Y"].Value = e.MapCoordinate.y.ToString();

                                                        frmbus.x = e.MapCoordinate.x;
                                                        frmbus.y = e.MapCoordinate.y;

                                                        //if (MessageBox.Show(@"是否保存当前坐标修改？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                                                        //{
                                                            Update110XY(dgvNoip.CurrentRow.Cells["报警编号"].Value.ToString(),
                                                                        dgvNoip.CurrentRow.Cells["X"].Value.ToString(),
                                                                        dgvNoip.CurrentRow.Cells["Y"].Value.ToString());
                                                            //MessageBox.Show("保存成功");
                                                        //}

                                                        #endregion
                                                    }                                                   
                                                }
                                                catch
                                                {
                                                }
                                                finally
                                                {
                                                    micmd.Dispose();
                                                    miconn.Close();
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case "MoveTool3":

                            switch (e.ToolStatus)
                            {
                                case ToolStatus.Start:

                                    try
                                    {
                                        Feature f;
                                        Table editTable = Session.Current.Catalog.GetTable("JCLayer");

                                        string tempname = "";
                                        if (this.dataGridView1.CurrentRow != null)
                                        {
                                            tempname = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                                            if (tempname != "")
                                            {
                                                const string tblname = "JCLayer";
                                                Map map = MapControl1.Map;
                                                if ((Session.Current.MapFactory.Count == 0) || (map == null))
                                                {
                                                    return;
                                                }
                                                rsfcflash = null;
                                                MIConnection miconn = new MIConnection();
                                                miconn.Open();
                                                MICommand micmd = miconn.CreateCommand();
                                                micmd.CommandText = "select * from " +
                                                                    MapControl1.Map.Layers[tblname].ToString() +
                                                                    " where AID = @name ";
                                                micmd.Parameters.Clear();
                                                micmd.Parameters.Add("@name", tempname);

                                                try
                                                {
                                                    Style s = null;
                                                    rsfcflash = micmd.ExecuteFeatureCollection();

                                                    if (rsfcflash.Count > 0)     //更新点的位置
                                                    {
                                                        //删除选中的点
                                                        foreach (Feature ft in rsfcflash)
                                                        {
                                                            s = ft.Style;
                                                            editTable.DeleteFeature(ft);
                                                            break;
                                                        }

                                                        #region 添加点

                                                        f = new Feature(editTable.TableInfo.Columns);
                                                        f.Geometry =
                                                            new MapInfo.Geometry.Point(MapControl1.Map.GetDisplayCoordSys(),
                                                                                       new DPoint(e.MapCoordinate.x,
                                                                                                  e.MapCoordinate.y));
                                                        f["Name"] = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                                                        f["AID"] = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                                                        f.Style = s;
                                                        editTable.InsertFeature(f);

                                                        #endregion


                                                        #region 更新列表中的XY值


                                                        this.dataGridView1.CurrentRow.Cells["X"].Value = e.MapCoordinate.x.ToString();
                                                        this.dataGridView1.CurrentRow.Cells["Y"].Value = e.MapCoordinate.y.ToString();

                                                        frmbus.x = e.MapCoordinate.x;
                                                        frmbus.y = e.MapCoordinate.y;

                                                        //if (MessageBox.Show(@"是否保存当前坐标修改？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                                                        //{
                                                        //    Update110XY(dgvNoip.CurrentRow.Cells["报警编号"].Value.ToString(),
                                                        //                dgvNoip.CurrentRow.Cells["X"].Value.ToString(),
                                                        //                dgvNoip.CurrentRow.Cells["Y"].Value.ToString());
                                                        //    MessageBox.Show("保存成功");
                                                        //}

                                                        #endregion
                                                    }
                                                    else     // 如果不存在点，则要添加点
                                                    {
                                                        CompositeStyle cs = new CompositeStyle();
                                                        cs.ApplyStyle(new BitmapPointStyle(GetBmpName(this.dataGridView1.CurrentRow.Cells["案件来源"].ToString().Trim()), BitmapStyles.None, Color.Red, 20));


                                                        f = new Feature(editTable.TableInfo.Columns);
                                                        f.Geometry =
                                                            new MapInfo.Geometry.Point(MapControl1.Map.GetDisplayCoordSys(),
                                                                                       new DPoint(e.MapCoordinate.x,
                                                                                                  e.MapCoordinate.y));
                                                        f["Name"] = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                                                        f["AID"] = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                                                        f.Style = cs;
                                                        editTable.InsertFeature(f);

                                                        this.dataGridView1.CurrentRow.Cells["X"].Value = e.MapCoordinate.x.ToString();
                                                        this.dataGridView1.CurrentRow.Cells["Y"].Value = e.MapCoordinate.y.ToString();

                                                        frmbus.x = e.MapCoordinate.x;
                                                        frmbus.y = e.MapCoordinate.y;

                                                    }
                                                }
                                                catch
                                                {
                                                }
                                                finally
                                                {
                                                    micmd.Dispose();
                                                    miconn.Close();
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
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
            catch (Exception ex)
            {
                ExToLog(ex, "04-校正坐标");
            }
        }


        public void cleartemp()
        {
            try
            {
                if (MapControl1.Map.Layers["VideoLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (MapControl1.Map.Layers["VideoLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("VideoLabel");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "05-清除临时图层");
            }
        }

        /// <summary>
        /// 切换到其它模块时，清除本模块临时图层
        /// </summary>
        public void Dipose110()
        {
            try
            {
                //timer1.Enabled = false;

                //timer2.Enabled = false;

                //if (MapControl1.Map.Layers["Track"] != null)
                //{
                //    Session.Current.Catalog.CloseTable("Track");
                //}

                if (MapControl1.Map.Layers["JCLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("JCLayer");
                }

                if (MapControl1.Map.Layers["JCLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("JCLabel");
                }

                if (MapControl1.Map.Layers["SocketLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("SocketLayer");
                }

                if (MapControl1.Map.Layers["SocketLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("SocketLabel");
                }

                if (MapControl1.Map.Layers["VideoLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (MapControl1.Map.Layers["VideoLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("VideoLabel");
                }

                if (MapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("VideoCarLayer");
                }

                if (MapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("VideoCarLabel");
                }

                //if (MapControl1.Map.Layers["GuijiLayer"] != null)
                //{
                //    Session.Current.Catalog.CloseTable("GuijiLayer");
                //}

                dgvip.DataSource = null;
                dgvNoip.DataSource = null;
                frmarm.Visible = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "06-切换到其它模块时，清除本模块临时图层");
            }
        }

        /// <summary>
        /// 页面切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {    
                    if (MapControl1.Map.Layers["JCLayer"] != null)
                    {
                        Session.Current.Catalog.CloseTable("JCLayer");
                    }

                    if (MapControl1.Map.Layers["JCLabel"] != null)
                    {
                        MapControl1.Map.Layers.Remove("JCLabel");
                    }


                    if (tabControl1.SelectedTab == tabPage2)
                    {
                        //this.cmbField.Text = "报警时间";
                        stime = System.DateTime.Now.ToShortDateString() + " 00:00:01";
                        etime = System.DateTime.Now.ToShortDateString() + " 23:59:59";
                        FillNoIpData("报警时间");
                    }

                    if (tabControl1.SelectedTab == tabPage3)
                    {
                        //this.comboBox1.Text = "报警时间";
                        stime = System.DateTime.Now.ToShortDateString() + " 00:00:01";
                        etime = System.DateTime.Now.ToShortDateString() + " 23:59:59";
                        FillNoXYData("报警时间");
                    }

                    if (tabControl1.SelectedTab != tabPage1)
                    {
                        this.dgvip.DataSource = null;
                    }
                    
                    if (tabControl1.SelectedTab != tabPage2)
                    {
                        this.dgvNoip.DataSource = null;
                        this.CaseSearchBox.Text = "";
                        this.cmbField.Text = this.cmbField.Items[0].ToString();
                        this.PageNow2.Text = "0";
                        this.PageNum2.Text = "/0";
                        this.RCount2.Text = "0条";
                    }
                    
                    if (tabControl1.SelectedTab != tabPage3)
                    {
                        this.dataGridView1.DataSource = null;
                        this.spellSearchBoxEx1.Text = "";
                        this.comboBox1.Text = this.comboBox1.Items[0].ToString();
                        this.PageNow3.Text = "0";
                        this.PageNum3.Text = "/0";
                        this.RCount3.Text = "0条";
                    }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "07-本机案件和非本机案件进行切换");
            }
        }

        /// <summary>
        /// 创建已办案件图层
        /// </summary>
        private void Create110NovipLayer()
        {
            try
            {
                if (MapControl1.Map.Layers["JCLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("JCLayer");
                }

                if (MapControl1.Map.Layers["JCLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("JCLabel");
                }

                //if (MapControl1.Map.Layers["SocketLayer"] != null)
                //{
                //    Session.Current.Catalog.CloseTable("SocketLayer");
                //}
                //if (MapControl1.Map.Layers["SocketLabel"] != null)
                //{
                //    MapControl1.Map.Layers.Remove("SocketLabel");
                //}

                Catalog Cat = Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("JCLayer");
                Table tblTemp = Cat.GetTable("JCLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("JCLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("AID", 50));


                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                MapControl1.Map.Layers.Insert(0, lyr);

                //添加标注
                const string activeMapLabel = "JCLabel";
                Table activeMapTable = Session.Current.Catalog.GetTable("JCLayer");
                LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);

                LabelSource lbsource = new LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "AID";
                lblayer.Sources.Append(lbsource);
                MapControl1.Map.Layers.Add(lblayer);                
            }
            catch (Exception ex)
            {
                ExToLog(ex, "08-创建非本机案件图层");
            }
        }       

        public String GetBmpName(String ajly)
        {
            try
            {
                string bmpname = "jc.bmp";
                if (ajly.IndexOf("110") > -1)
                    bmpname = "jCar.bmp";
                if (ajly.IndexOf("120") > -1)
                    bmpname = "AMBU-64.BMP";
                if (ajly.IndexOf("119") > -1)
                    bmpname = "FIRE-64.BMP";
                if (ajly.IndexOf("交通事故") > -1)
                    return bmpname = "TRAF1-32.BMP";

                return bmpname;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetBmpName");
                return null;
            }
        }




        //获取查询时间的起始时间和终止时间 

        string stime = string.Empty;  //时间段的开始时间
        string etime = string.Empty;  //时间段的终止时间

        private void cmbField_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbField.Text == "报警时间" || this.cmbField.Text == "发案时间初值" || this.cmbField.Text == "发案时间终值")
                {
                    frmalarm fa = new frmalarm();
                    if (fa.ShowDialog(this) == DialogResult.OK)
                    {
                        stime = fa.st;
                        etime = fa.et;

                        this.CaseSearchBox.Text = stime + etime;
                    }
                    fa.Close();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetBmpName");
            }
        }
        /// <summary>
        /// 查询已办案情
        /// </summary>
        private void FillNoIpData(string KeyField)
        {
            try
            {
                isShowPro(true);

                string sRegion2 = strRegion2;
                string sRegion3 = strRegion3;
                string strWhere = getPaiNo(sRegion2); // 根据区域派出所权限生成条件

                //(select 派出所名 from 基层派出所 where t.所属派出所 = 基层派出所.派出所代码)
                // 此常量用于将派出所代码转换成派出所名（由于GPS110.报警信息110的派出所代码与主程序派出所代码不同 所以用此转换）
                const string paiName = "case  when t.所属派出所 like '4406006050%' then '大良' when t.所属派出所 like '4406006051%' then '容桂' when t.所属派出所 like '4406006052%' then '伦教' when t.所属派出所 like '4406006053%' then '北滘' when t.所属派出所 like '4406006054%' then '陈村' when t.所属派出所 like '4406006055%' then '乐从' when t.所属派出所 like '4406006056%' then '龙江' when t.所属派出所 like '4406006057%' then '勒流' when t.所属派出所 like '4406006058%' then '杏坛'  when t.所属派出所 like '4406006059%' then '均安' end ";

                const string fields =
                   "t.报警编号,t.案件名称,t.案件类型,t.案别_案由,t.专案标识,t.发案时间初值,t.发案时间终值," +
                   "t.发案地点详址,t.简要案情,t.报警时间," + paiName + "  as 所属派出所,t.案件来源," +
                   "t.对讲机ID,处理警力编号 as 处理警员,t.X,t.Y ";
                string sql = string.Empty;
                
                if (KeyField == "报警时间" || KeyField == "发案时间初值" || KeyField == "发案时间终值")
                {
                    if (strWhere != "")
                        sql = "Select " + fields + " from GPS110.报警信息110 t where t.处理状态 ='已办' and " + KeyField + ">to_date('" + stime + "','YYYY-MM-DD HH24:MI:SS') and " + KeyField + " <to_date('" + etime + "','YYYY-MM-DD HH24:MI:SS') and " + getAnjianType() + " and " + strWhere;
                    else
                        sql = "Select " + fields + " from GPS110.报警信息110 t where t.处理状态 ='已办' and " + KeyField + ">to_date('" + stime + "','YYYY-MM-DD HH24:MI:SS') and " + KeyField + " <to_date('" + etime + "','YYYY-MM-DD HH24:MI:SS') and " + getAnjianType();
                }
                else
                {
                    if (strWhere != "")
                        sql = "Select " + fields + " from GPS110.报警信息110 t where t.处理状态 ='已办' and " + KeyField + " like '%" + this.CaseSearchBox.Text.Trim() + "%' and " + getAnjianType() + " and " + strWhere;
                    else
                        sql = "Select " + fields + " from GPS110.报警信息110 t where t.处理状态 ='已办' and " + KeyField + " like '%" + this.CaseSearchBox.Text.Trim() + "%' and " + getAnjianType();
                }

                // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                if (sql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                    sql += " and (t.备用字段一 is null or t.备用字段一='') order by t.x";
                else
                    sql += " where (t.备用字段一 is null or t.备用字段一='') order by t.x";
                //-------------------------------------------------------
                      
                DataTable dt = GetTable(sql);
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("没有查询到对应数据，请重新查询","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    return;
                }
                this.toolPro.Value = 1;
                Application.DoEvents();

                #region 导出Excel
                //try
                //{
                //    //excelSql = sql.Substring(0, sql.LastIndexOf("order")); // 去掉排序,为防止下面连接条件时出错


                //    //if (strRegion2 != "顺德区")
                //    //{
                //    //    if (strRegion2 != "")
                //    //    {
                //    //        if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                //    //        {
                //    //            sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                //    //        }
                //    //        excelSql += " and (权限单位 in ('" + sRegion2.Replace(",", "','") + "'))";
                //    //    }
                //    //    else if (strRegion2 == "")
                //    //    {
                //    //        if (excelSql.IndexOf("where") < 0)
                //    //        {
                //    //            excelSql += " where 1=2 ";
                //    //        }
                //    //        else
                //    //        {
                //    //            excelSql += " and 1=2 ";
                //    //        }
                //    //    }
                //    //}
                //    //excelSql += " order by t.x";
                //    excelSql = sql;
                //    OracleConnection orc = new OracleConnection(mysqlstr);
                //    try
                //    {
                //        orc.Open();
                //        OracleCommand cmd = new OracleCommand(excelSql, orc);
                //        apt1 = new OracleDataAdapter(cmd);
                //        DataTable datatableExcel = new DataTable();
                //        apt1.Fill(PagenCurrent2 == 0 ? 0 : PagenCurrent2 - pageSize2 < 0 ? 0 : PagenCurrent2 - pageSize2, pageSize2, datatableExcel);
                //        if (dtExcel != null) dtExcel.Clear();
                //        dtExcel = datatableExcel;
                //        cmd.Dispose();
                //    }
                //    catch (Exception ex)
                //    {
                //        isShowPro(false);
                //        ExToLog(ex, "008-判断导出列表数量");
                //    }
                //    finally { orc.Close(); }   
                //}
                //catch (Exception ex)
                //{
                //    isShowPro(false);
                //    Console.WriteLine(ex.Message);
                //}
                # endregion
                this.toolPro.Value = 2;
                Application.DoEvents();    

                Add110NovipFtr(dt);  //添加图元
                if (dtExcel != null) dtExcel.Clear();
                Pagedt2 = dtExcel = dt;
                InitDataSet2(RCount2, PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvNoip);  //分页 

                //SetDataGridViewYellow(dgvNoip);  //显示没定位的坐标点
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);  
            }
            catch (Exception ex)
            {
                isShowPro(false);
                ExToLog(ex, "10-填充非本机案件DataGrid");
            }
        }

        /// <summary>
        /// 用于区域派出所权限生成条件
        /// </summary>
        /// <param name="strPai">派出所权限</param>
        /// <returns>条件表达式</returns>
        private string getPaiNo(string strPai)
        {
            try
            {
                string strWhere = "";
                string[] pai = strPai.Split(',');
                for (int i = 0; i < pai.Length; i++)
                {
                    switch (pai[i])
                    {
                        case "大良":
                            strWhere += " or 所属派出所 like '4406006050%'";
                            break;
                        case "容桂":
                            strWhere += " or 所属派出所 like '4406006051%'";
                            break;
                        case "伦教":
                            strWhere += " or 所属派出所 like '4406006052%'";
                            break;
                        case "北滘":
                            strWhere += " or 所属派出所 like '4406006053%'";
                            break;
                        case "陈村":
                            strWhere += " or 所属派出所 like '4406006054%'";
                            break;
                        case "乐从":
                            strWhere += " or 所属派出所 like '4406006055%'";
                            break;
                        case "龙江":
                            strWhere += " or 所属派出所 like '4406006056%'";
                            break;
                        case "勒流":
                            strWhere += " or 所属派出所 like '4406006057%'";
                            break;
                        case "杏坛":
                            strWhere += " or 所属派出所 like '4406006058%'";
                            break;
                        case "均安":
                            strWhere += " or 所属派出所 like '4406006059%'";
                            break;
                        default:
                            strWhere = "";
                            break;
                    }
                }
                if (strWhere != "")
                    strWhere = strWhere.Remove(0, 3);  // 去掉第一个" or"
                return strWhere;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getPaiNo");
                return "";
            }
        }

        /// <summary>
        /// 添加已办案件图元
        /// </summary>
        public void Add110NovipFtr(DataTable dt)
        {
            try
            {
                Create110NovipLayer();

                FeatureLayer lyrcar = MapControl1.Map.Layers["JCLayer"] as FeatureLayer;
                Table tblcar = Session.Current.Catalog.GetTable("JCLayer");

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["案件名称"].ToString();
                        string tempid = dr["报警编号"].ToString();

                        string bmpName = GetBmpName(dr["案件来源"].ToString());

                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        if (lyrcar != null)
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv));

                            CompositeStyle cs = new CompositeStyle();
                            cs.ApplyStyle(new BitmapPointStyle(bmpName, BitmapStyles.None, Color.Red, 20));

                            Feature ftr = new Feature(tblcar.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = tempname;
                            ftr["AID"] = tempid;
                            tblcar.InsertFeature(ftr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "09-添加非本机案件图元");
            }
        }

        /// <summary>
        /// 周边查询
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="distCar"></param>
        /// <param name="carflag"></param>
        /// <param name="distVideo"></param>
        /// <param name="videoflag"></param>
        private void SearchDistance(DPoint dp, double distCar, bool carflag, double distVideo, bool videoflag)
        {
            try
            {
                // 创建 视频图层             
                if (videoflag)
                {
                    ucVideo fv = new ucVideo(MapControl1, st, tdb, ConStr, VideoPort, VideoString, VEpath, true,false);
                    fv.getNetParameter(ns, vf);
                    //fv.strRegion = this.strRegion;
                    //fv.strRegion1 = this.strRegion1;
                    //fv.SetUserRegion(this.strRegion, this.strRegion1);  //jie.zhang 20100118

                    fv.SearchVideoDistance(dp, distVideo, "110");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "11-周边查询");
            }

            //// 创建车辆图层
            //if (carflag == true)
            //{
            //    clCar.ucCar fcar = new clCar.ucCar();
            //    fcar.getParameter(mapControl1, null, strConn, true);
            //    fcar.strRegion = strRegion;
            //    //fcar.CreateCarLayer();

            //    fcar.SearchCarDistance(dp, distCar);
            //    fcar.ZhiHui = false;
            //}

            //Distance dt = new Distance();
            //if (distCar >= distVideo)
            //{
            //    dt = new Distance(distCar, DistanceUnit.Meter);
            //}
            //else
            //{
            //    dt = new Distance(distVideo, DistanceUnit.Meter);
            //}

            //this.mapControl1.Map.SetView(dp, this.mapControl1.Map.GetDisplayCoordSys(), dt);
        }

        private void dgvip_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvip.CurrentRow != null)
                {
                    string tempname = dgvip.CurrentRow.Cells[0].Value.ToString();

                    string tblname = "SocketLayer";

                    //提取当前选择的信息的通行车辆编号作为主键值

                    Map map = MapControl1.Map;

                    if ((Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    rsfcflash = null;

                    MIConnection conn = new MIConnection();
                    conn.Open();

                    MICommand cmd = conn.CreateCommand();
                    cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() + " where AID = @name ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@name", tempname);
                    MapInfo.Geometry.CoordSys cSys = MapControl1.Map.GetDisplayCoordSys();

                    rsfcflash = cmd.ExecuteFeatureCollection();
                    if (rsfcflash.Count > 0)
                    {
                        Session.Current.Selections.DefaultSelection.Clear();
                        Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                        foreach (Feature f in rsfcflash)
                        {
                            MapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                            MapControl1.Map.Center = f.Geometry.Centroid;
                            break;
                        }
                        cmd.Dispose();
                        conn.Close();

                        iflash = 0;

                        timeflash.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "12-在dgvip列表上双击");
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
                CLC.BugRelated.ExceptionWrite(ex, "uc110-getScale");
                return 0;
            }
        }

        private void timeflash_Tick(object sender, EventArgs e)
        {
            try
            {
                iflash = iflash + 1;

                int i = iflash%2;
                if (i == 0)
                {
                    Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                }
                else
                {
                    Session.Current.Selections.DefaultSelection.Clear();
                }

                if (iflash%10 == 0)
                {
                    timeflash.Enabled = false;

                    //GetGzPoistion();
                }
            }
            catch (Exception ex)
            {
                timeflash.Enabled = false;
                ExToLog(ex, "13-案件图元闪现");
            }
        }

        private void dgvNoip_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //点击表头,退出

            if (dgvNoip.CurrentRow == null) return;
            
            try
            {
                string tempname = dgvNoip.CurrentRow.Cells[0].Value.ToString();

                string tblname = "JCLayer";

                //提取当前选择的信息的通行车辆编号作为主键值

                Map map = MapControl1.Map;

                if ((Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                rsfcflash = null;

                MIConnection conn = new MIConnection();
                conn.Open();

                MICommand cmd = conn.CreateCommand();
                cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() + " where AID = @name ";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@name", tempname);

                rsfcflash = cmd.ExecuteFeatureCollection();
                if (rsfcflash.Count > 0)
                {
                    Session.Current.Selections.DefaultSelection.Clear();
                    Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                    foreach (Feature f in rsfcflash)
                    {
                        MapControl1.Map.Center = f.Geometry.Centroid;

                        CoordSys cSys = MapControl1.Map.GetDisplayCoordSys();

                        MapControl1.Map.SetView(MapControl1.Map.Center, cSys, getScale());

                        break;
                    }
                    cmd.Dispose();
                    conn.Close();

                    iflash = 0;

                    timeflash.Enabled = true;
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "14-在dgvip列表上双击");
            }
        }

        private void dgvip_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //点击表头,退出

            try
            {
                DPoint dp = new DPoint();

                DataTable datatable = new DataTable("socket");

                //列
                string[] func = {"案件编号", "发案地点详址", "简要案情", "案件名称", "所属派出所", "所属中队", "对讲机ID", "案件来源", "报警时间", "X", "Y"};
                for (int i = 0; i < func.Length; i++)
                {
                    //创建并添加虚拟列到虚拟表中
                    DataColumn dc = new DataColumn(func[i]);
                    //指定字段的数据类型，这步没有也不会出错
                    dc.DataType = Type.GetType("System.String");
                    datatable.Columns.Add(dc);
                }

                //行
                DataRow drow = datatable.NewRow();
                for (int i = 0; i < func.Length; i++)
                {
                    drow[func[i]] = dgvip.Rows[0].Cells[i].Value;
                }
                datatable.Rows.Add(drow);


                if (datatable.Rows.Count > 0)
                {
                    //闪现图元
                    //////////////////////////////////////////
                    string tempname = dgvip.CurrentRow.Cells[0].Value.ToString();

                    string tblname = "SocketLayer";

                    Map map = MapControl1.Map;

                    if ((Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    rsfcflash = null;


                    MIConnection conn = new MIConnection();

                    try
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                        conn.Open();

                        MICommand cmd = conn.CreateCommand();
                        cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() +
                                          " where AID = @name ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@name", tempname);

                        rsfcflash = cmd.ExecuteFeatureCollection();
                        if (rsfcflash.Count > 0)
                        {
                            Session.Current.Selections.DefaultSelection.Clear();
                            Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                            foreach (Feature f in rsfcflash)
                            {
                                MapControl1.Map.Center = f.Geometry.Centroid;
                                CoordSys cSys = MapControl1.Map.GetDisplayCoordSys();

                                MapControl1.Map.SetView(MapControl1.Map.Center, cSys, getScale());
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }
                            cmd.Dispose();

                            timeflash.Enabled = true;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                    }

                    /////////////////////////////////////////                    

                    Point pt = new Point();
                    if (dp.x == 0 || dp.y == 0)
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;
                        disPlayInfo(datatable, pt);
                        return;
                    }

                    MapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += Width + 10;
                    pt.Y += 80;
                    disPlayInfo(datatable, pt);

                    //WriteEditLog("终端车辆号牌='" + this.dataGridViewKakou.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情", "V_CROSS");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "15-数据表dataGridViewKakou双击");
            }
        }

        private void disPlayInfo(DataTable dt, Point pt)
        {
            try
            {
                if (frminfo.Visible == false)
                {
                    frminfo = new FrmInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.Visible = false;
                }
                frminfo.setInfo(dt.Rows[0], pt, ConStr, user);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "16-显示报警详细信息");
            }
        }

        private void dgvNoip_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //点击表头,退出

            if (dgvNoip.CurrentRow == null) return;

            try
            {
                DPoint dp = new DPoint();

                // 此常量用于将派出所代码转换成派出所名（由于GPS110.报警信息110的派出所代码与主程序派出所代码不同 所以用此转换）
                const string paiName = "case  when t.所属派出所 like '4406006050%' then '大良' when t.所属派出所 like '4406006051%' then '容桂' when t.所属派出所 like '4406006052%' then '伦教' when t.所属派出所 like '4406006053%' then '北滘' when t.所属派出所 like '4406006054%' then '陈村' when t.所属派出所 like '4406006055%' then '乐从' when t.所属派出所 like '4406006056%' then '龙江' when t.所属派出所 like '4406006057%' then '勒流' when t.所属派出所 like '4406006058%' then '杏坛'  when t.所属派出所 like '4406006059%' then '均安' end ";
                const string sqlFields =
                     "t.报警编号,t.案件名称,t.案件类型,t.案别_案由,t.专案标识,t.发案时间初值,t.发案时间终值," +
                     "t.发案地点详址,t.简要案情,t.报警时间," + paiName + " as 所属派出所," +
                     "t.对讲机ID,处理警力编号 as 处理警员,t.X,t.Y "; 
              
                string strSQL = "Select " + sqlFields + " from GPS110.报警信息110 t where t.报警编号 = '" +
                                dgvNoip.CurrentRow.Cells[0].Value.ToString() +"'";

                DataTable datatable = GetTable(strSQL);

                if (datatable.Rows.Count > 0)
                {
                    //闪现图元
                    //////////////////////////////////////////
                    string tempname = dgvNoip.CurrentRow.Cells[0].Value.ToString();

                    string tblname = "JCLayer";

                    Map map = MapControl1.Map;

                    if ((Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    rsfcflash = null;


                    MIConnection conn = new MIConnection();

                    try
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                        conn.Open();

                        MICommand cmd = conn.CreateCommand();
                        cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() +
                                          " where AID = @name ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@name", tempname);

                        rsfcflash = cmd.ExecuteFeatureCollection();
                        if (rsfcflash.Count > 0)
                        {
                            Session.Current.Selections.DefaultSelection.Clear();
                            Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                            foreach (Feature f in rsfcflash)
                            {
                                MapControl1.Map.Center = f.Geometry.Centroid;
                                CoordSys cSys = MapControl1.Map.GetDisplayCoordSys();
                                MapControl1.Map.SetView(MapControl1.Map.Center, cSys, 6000);
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }
                            cmd.Dispose();

                            timeflash.Enabled = true;
                        }
                        //else
                        //{
                        //    MessageBox.Show(@"当前数据无坐标值！", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //}
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                    }

                    /////////////////////////////////////////                    
                    Point pt = new Point();
                    if (dp.x == 0 || dp.y == 0)
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;
                        disPlayInfo(datatable, pt);
                        return;
                    }

                    MapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += Width + 10;
                    pt.Y += 80;
                    disPlayInfo(datatable, pt);
                    //WriteEditLog("终端车辆号牌='" + this.dataGridViewKakou.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情", "V_CROSS");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "17-数据表dataGridViewKakou双击");
            }
        }

        private void ToolVip_Click(object sender, EventArgs e)
        {
            try
            {

                if (dgvip.CurrentRow == null)
                {
                    MessageBox.Show(@"没有选择任何数据，无法进行当前操作！", @"系统提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information);
                    return;
                }

                switch (ToolVip.Text)
                {
                    case "结束校正":
                        if (MessageBox.Show(@"是否结束坐标修改？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            //Update110XY(dgvip.CurrentRow.Cells["报警编号"].Value.ToString(),
                            //            dgvip.CurrentRow.Cells["X"].Value.ToString(),
                            //            dgvip.CurrentRow.Cells["Y"].Value.ToString());

                            MapControl1.Tools.LeftButtonTool = "Select";
                            ToolVip.Text = @"坐标校正";
                        }
                        break;
                    case "坐标校正":
                       
                            if (
                                MessageBox.Show(@"确定要进行数据校正吗？", @"系统提示", MessageBoxButtons.OKCancel,
                                                MessageBoxIcon.Information) ==
                                DialogResult.OK)
                            {

                                MapControl1.Tools.LeftButtonTool = "MoveTool1";
                                ToolVip.Text = @"结束校正";
                            }
                         
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "18-ToolVip_Click");
            }
        }

        private void toolNoVip_Click(object sender, EventArgs e)
        {
            try
            {
                if ( dgvNoip.CurrentRow == null)
                {
                    MessageBox.Show(@"没有选择任何数据，无法进行当前操作！", @"系统提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information);
                    return;
                }

                switch (toolNoVip.Text)
                {
                    case @"坐标校正":
                        if (MessageBox.Show(@"确定要进行数据校正吗？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) ==
                            DialogResult.OK)
                        {
                                MapControl1.Tools.LeftButtonTool = "MoveTool2";
                                toolNoVip.Text = @"结束校正";
                        }
                        break;
                    case @"结束校正":
                        if (MessageBox.Show(@"是否结束坐标修改？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) ==
                            DialogResult.OK)
                        {
                            //Update110XY(dgvNoip.CurrentRow.Cells["报警编号"].Value.ToString(),
                            //            dgvNoip.CurrentRow.Cells["X"].Value.ToString(),
                            //            dgvNoip.CurrentRow.Cells["Y"].Value.ToString()); 

                            MapControl1.Tools.LeftButtonTool = "Select";
                            toolNoVip.Text = @"坐标校正";
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "19-toolNoVip_Click");
            }
        }


       private void  Update110XY(string id,string x,string y)
       {
           try
           {
               string sql = "select count(*) from GPS110.报警信息110 where 报警编号='" + id + "'";
               int i = GetScalar(sql);
               if (i > 0)
               {
                   sql = "Update GPS110.报警信息110 t set t.X=" + x + ",t.Y=" + y + " where t.报警编号='" + id + "'";
                   RunCommand(sql);
               }
               else
               {
                   MessageBox.Show(@"数据库中不存在当前编号的报警信息？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                   return;
               }
           }
           catch (Exception ex)
           {
               ExToLog(ex, "20-更新坐标");
           }
       }

        private void uc110_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (Visible == false)
                {
                    Dipose110();
                }
                else
                {
                    Init110();

                    //timer1.Interval = 10*1000;
                    //timer1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "20-窗口显示切换");
            }
        }

        private void toolBusVip_Click(object sender, EventArgs e)
        {
            try
            {
                //frmBus frmbus = new frmBus();
                frmbus.x = Convert.ToDouble(dgvip.CurrentRow.Cells["X"].Value);
                frmbus.y = Convert.ToDouble(dgvip.CurrentRow.Cells["Y"].Value);

                frmbus.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "21-窗口显示切换");
            }
        }

        private void toolBusNoVip_Click(object sender, EventArgs e)
        {
            try
            {
                frmbus.x = Convert.ToDouble(dgvNoip.CurrentRow.Cells["X"].Value);
                frmbus.y = Convert.ToDouble(dgvNoip.CurrentRow.Cells["Y"].Value);

                frmbus.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "22-toolBusNoVip_Click");
            }
        }


        private void tooldisNovip_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint();
                string tempname = dgvNoip.CurrentRow.Cells[0].Value.ToString(); //获取当前选择的行中 案件编号

                string tblname = "JCLayer";

                Map map = MapControl1.Map;

                if ((Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                rsfcflash = null;

                MIConnection conn = new MIConnection();

                try
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    conn.Open();

                    MICommand cmd = conn.CreateCommand();
                    cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() +
                                      " where AID = @name ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@name", tempname);

                    rsfcflash = cmd.ExecuteFeatureCollection();
                    if (rsfcflash.Count > 0)
                    {
                        Session.Current.Selections.DefaultSelection.Clear();
                        Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                        foreach (Feature f in rsfcflash)
                        {
                            MapControl1.Map.Center = f.Geometry.Centroid;
                            dp.x = f.Geometry.Centroid.x;
                            dp.y = f.Geometry.Centroid.y;
                            break;
                        }
                        conn.Close();
                        cmd.Dispose();
                    }
                    SearchDistance(dp, 0, true, videodis, true);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "23-tooldisNovip_Click");
            }
        }

        private void tooldisVip_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint();
                string tempname = dgvip.CurrentRow.Cells[0].Value.ToString(); //获取当前选择的行中 案件编号

                string tblname = "SocketLayer";

                Map map = MapControl1.Map;

                if ((Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                rsfcflash = null;


                MIConnection conn = new MIConnection();

                try
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    conn.Open();

                    MICommand cmd = conn.CreateCommand();
                    cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() +
                                      " where AID = @name ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@name", tempname);

                    rsfcflash = cmd.ExecuteFeatureCollection();
                    if (rsfcflash.Count > 0)
                    {
                        Session.Current.Selections.DefaultSelection.Clear();
                        Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                        foreach (Feature f in rsfcflash)
                        {
                            MapControl1.Map.Center = f.Geometry.Centroid;
                            dp.x = f.Geometry.Centroid.x;
                            dp.y = f.Geometry.Centroid.y;
                            break;
                        }
                        conn.Close();
                        cmd.Dispose();
                    }
                    SearchDistance(dp, 0, true, videodis, true);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "24-tooldisVip_Click");
            }
        }
          


        //已办警情信息分页

        private void bindingNavigator2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Text)
                {
                    case "上一页":
                        pageCurrent2--;
                        if (pageCurrent2 < 1)
                        {
                            pageCurrent2 = 1;
                            MessageBox.Show("已经是第一页，请点击“下一页”查看！");
                            return;
                        }
                        else
                        {
                            PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                        }
                        LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvNoip);
                        break;
                    case "下一页":
                        pageCurrent2++;
                        if (pageCurrent2 > pageCount2)
                        {
                            pageCurrent2 = pageCount2;
                            MessageBox.Show("已经是最后一页，请点击“上一页”查看！");
                            return;
                        }
                        else
                        {
                            PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                        }
                        LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvNoip);
                        break;
                    case "转到首页":
                        if (pageCurrent2 <= 1)
                        {
                            MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                            MessageBoxDefaultButton.Button2);
                            return;
                        }
                        else
                        {
                            pageCurrent2 = 1;
                            PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                        }
                        LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvNoip);
                        break;
                    case "转到尾页":
                        if (pageCurrent2 > pageCount2 - 1)
                        {
                            MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK,
                                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                            return;
                        }
                        else
                        {
                            pageCurrent2 = pageCount2;
                            PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                        }
                        LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvNoip);
                        break;
                    case "数据导出":
                        break;                   
                }
                //SetDataGridViewYellow(dgvNoip);

                #region 数据导出
                //DataTable datatableExcel = new DataTable();
                //apt1.Fill(PagenCurrent2 == 0 ? 0 : PagenCurrent2 - pageSize2 < 0 ? 0 : PagenCurrent2 - pageSize2, pageSize2, datatableExcel);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExToLog(ex, "32-bindingNavigator2_ItemClicked");
            }
        }




        //==========
        //==========
        //翻页功能
        //==========
        //==========

        int pageSize2 = 100;     //每页显示行数
        int PagenMax2 = 0;         //总记录数
        int pageCount2 = 0;    //页数＝总记录数/每页显示行数
        int pageCurrent2 = 0;   //当前页号
        int PagenCurrent2 = 0;      //当前记录行 
        DataSet Pageds2 = new DataSet();　
        DataTable Pagedt2 = new DataTable();

        //edit by fisher in 09-11-23
        public void InitDataSet2(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount,
                                 BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize2 = 100;      //设置页面行数
                PagenMax2 = Pagedt2.Rows.Count;

                lblcount.Text = PagenMax2.ToString() + "条"; //在导航栏上显示总记录数

                pageCount2 = (PagenMax2/pageSize2); //计算出总页数
                if ((PagenMax2%pageSize2) > 0) pageCount2++;
                if (PagenMax2 != 0)
                {
                    pageCurrent2 = 1;
                }
                PagenCurrent2 = 0; //当前记录数从0开始

                LoadData2(textNowPage, lblPageCount, bs, bn, dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExToLog(ex, "33-InitDataSet2");
            }
        }


        public void LoadData2(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds,
                              BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                isShowPro(true);
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = Pagedt2.Clone();

                if (pageCurrent2 == pageCount2)
                    nEndPos = PagenMax2;
                else
                    nEndPos = pageSize2*pageCurrent2;
                nStartPos = PagenCurrent2;

                //tsl.Text = Convert.ToString(pageCurrent2) + "/" + pageCount2.ToString();
                textNowPage.Text = Convert.ToString(pageCurrent2);
                lblPageCount.Text = "/" + pageCount2.ToString();
                this.toolPro.Value = 1;
                Application.DoEvents();
                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt2.Rows[i]);
                    PagenCurrent2++;
                }

                if (dtExcel != null) dtExcel.Clear();
                bds.DataSource = dtExcel = dtTemp;
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
                ExToLog(ex, "34-LoadData2");
            }
        }

        //跳转指定页
        private void PageNow2_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(PageNow2.Text) < 1 || Convert.ToInt32(PageNow2.Text) > pageCount2)
                    {
                        MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                        MessageBoxDefaultButton.Button2);
                        PageNow2.Text = pageCurrent2.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent2 = Convert.ToInt32(PageNow2.Text);
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                        LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvNoip);
                        //SetDataGridViewYellow(dgvNoip);
                    }

                    #region 数据导出
                    //DataTable datatableExcel = new DataTable();
                    //apt1.Fill(PagenCurrent2 == 0 ? 0 : PagenCurrent2 - pageSize2 < 0 ? 0 : PagenCurrent2 - pageSize2, pageSize2, datatableExcel);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion

                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "35-toolStripTextBox1_KeyPress");
            }
        }

        //设定每页数量
        private void PageNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dgvNoip.Rows.Count > 0)
                {
                    pageSize2 = Convert.ToInt32(PageNumber.Text);
                    pageCurrent2 = 1; //当前转到第一页
                    PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    pageCount2 = (PagenMax2 / pageSize2); //计算出总页数
                    if ((PagenMax2 % pageSize2) > 0) pageCount2++;

                    LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvNoip);

                    //SetDataGridViewYellow(dgvNoip);


                    #region 数据导出
                    //DataTable datatableExcel = new DataTable();
                    //apt1.Fill(PagenCurrent2 == 0 ? 0 : PagenCurrent2 - pageSize2 < 0 ? 0 : PagenCurrent2 - pageSize2, pageSize2, datatableExcel);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "36-toolStripTextBox2_KeyPress");
            }
        }


        private void dgvip_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (dgvip.Rows.Count != 0)
                {
                    for (int i = 0; i < dgvip.Rows.Count; i++)
                    {
                        if ((i%2) == 1)
                        {
                            dgvip.Rows[i].DefaultCellStyle.BackColor = Color.WhiteSmoke;
                        }
                        else
                        {
                            dgvip.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "37-设置DataGrid颜色");
            }
        }

        private void SetDataGridViewYellow(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.Rows.Count != 0)
                {
                    for (int i = 0; i < dataGridView.Rows.Count; i++)
                    {
                        if ((i%2) == 1)
                        {
                            dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.WhiteSmoke;
                        }
                        else
                        {
                            dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }

                        if (dataGridView.Rows[i].Cells["X"].Value.ToString() == "" || dataGridView.Rows[i].Cells["Y"].Value.ToString() == "")
                        {
                            dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "38-设置DataGrid颜色");
            }
        }


        public void DealSocket(string bmpName, string[] msg, string DealType)
        {
            try
            {
                RecToLog(msg[0] + msg[1] + msg[2] + msg[3] + msg[4] + msg[5] + msg[6] + msg[7] + msg[8] + msg[9] +
                         msg[10]);

                string ajbh = msg[0].Substring(1, msg[0].Length - 1); // 去掉标志位的案件编号
                string callId = msg[6]; // 对讲机ID编号
                string ajmc = msg[3]; //案件名称
                double x = Convert.ToDouble(msg[9]); //经度               
                double y = Convert.ToDouble(msg[10]); //纬度


                DataTable dt = new DataTable("socket");
                //列
                string[] func = {"报警编号", "发案地点详址", "简要案情", "案件名称", "所属派出所", "所属中队", "对讲机ID", "案件来源", "报警时间", "X", "Y"};
                for (int i = 0; i < func.Length; i++)
                {
                    DataColumn dc = new DataColumn(func[i]);
                    dc.DataType = Type.GetType("System.String");
                    dt.Columns.Add(dc);
                }

                //行
                DataRow drow = dt.NewRow();
                drow[func[0]] = ajbh;
                for (int i = 1; i < func.Length; i++)
                {
                    drow[func[i]] = msg[i];
                }
                dt.Rows.Add(drow);

                dgvip.DataSource = dt;

                tabControl1.SelectedTab = tabPage1;

                if (MapControl1.Map.Layers["SocketLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("SocketLayer");
                }
                if (MapControl1.Map.Layers["SocketLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("SocketLabel");
                }

                if (MapControl1.Map.Layers["SocketLabe2"] != null)
                {
                    MapControl1.Map.Layers.Remove("SocketLabe2");
                }


                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("SocketLayer");
                Table tblTemp = Session.Current.Catalog.GetTable("SocketLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Session.Current.Catalog.CloseTable("SocketLayer");
                }
                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("AID", 50));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("CallID",40));

                tblTemp = Session.Current.Catalog.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                MapControl1.Map.Layers.Insert(0, lyr);

                //添加标注 案件编号
                string activeMapLabel = "SocketLabel";
                Table activeMapTable = Session.Current.Catalog.GetTable("SocketLayer");

                LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);
                LabelSource lbsource = new LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = Alignment.BottomCenter;
                lbsource.DefaultLabelProperties.Layout.Offset = 5;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "AID";               
                lblayer.Sources.Append(lbsource);
                MapControl1.Map.Layers.Add(lblayer);


                //添加标注 对讲机ID
                string activeMapLabel2 = "SocketLabe2";
                Table activeMapTable2 = Session.Current.Catalog.GetTable("SocketLayer");

                LabelLayer lblayer2 = new LabelLayer(activeMapLabel2, activeMapLabel2);
                LabelSource lbsource2 = new LabelSource(activeMapTable2);
                lbsource2.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource2.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource2.DefaultLabelProperties.Layout.Alignment = Alignment.TopCenter;
                lbsource2.DefaultLabelProperties.Layout.Offset = 5;
                lbsource2.DefaultLabelProperties.Style.Font.ForeColor = Color.Red;
                lbsource2.DefaultLabelProperties.Style.Font.BackColor = Color.Transparent;
                lbsource2.DefaultLabelProperties.Caption = "CallID";
                lblayer2.Sources.Append(lbsource2);
                MapControl1.Map.Layers.Add(lblayer2);
                                                
                
                RecToLog("案件编号:" + ajbh + " 对讲机ID:" + callId + " 案件名称:" + ajmc + " X:" + x.ToString() + " Y:" +
                         y.ToString());

                FeatureLayer lyrcar = MapControl1.Map.Layers["SocketLayer"] as FeatureLayer;
                    //(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = Session.Current.Catalog.GetTable("SocketLayer");

                DPoint dp = new DPoint();
                if (x != 0 && y != 0)
                {
                    dp.x = x;
                    dp.y = y;
                    MapControl1.Map.Center = dp;
                    if (lyrcar != null)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(x, y));

                        CompositeStyle cs = new CompositeStyle();
                        cs.ApplyStyle(new BitmapPointStyle(bmpName, BitmapStyles.None, Color.Red,20));

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = ajmc; //案件名称
                        ftr["AID"] = ajbh; // 案件编号
                        ftr["CallID"] = callId;//对讲机ID
                        tblcar.InsertFeature(ftr);
                    }


                    // 以下代码用来将当前地图的视野缩放至系统设置视图范围  jie.zhang 20100311
                    try
                    {
                        DPoint dP = new DPoint(x, y);
                        CoordSys cSys = MapControl1.Map.GetDisplayCoordSys();

                        MapControl1.Map.SetView(dP, cSys, getScale());
                        MapControl1.Map.Center = dP;
                    }
                    catch (Exception ex)
                    {
                        ExToLog(ex, "设定视图范围");
                    }

                    switch (DealType)
                    {
                        case "匹配数据":
                            try
                            {
                                if (dgvip.CurrentRow != null)
                                {
                                    frmbus.x = Convert.ToDouble(dgvip.Rows[0].Cells["X"].Value);
                                    frmbus.y = Convert.ToDouble(dgvip.Rows[0].Cells["Y"].Value);
                                    frmbus.Visible = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                ExToLog(ex, "21-窗口显示切换");
                            }
                            break;
                        case "坐标修正":

                            try
                            {
                                //if (
                                //    MessageBox.Show(@"确定要进行坐标校正吗？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                                //{
                                    if (dgvip.CurrentRow != null)
                                    {
                                        Update110XY(ajbh,x.ToString(),y.ToString());  //jie.zhang 20100624  修改为接收数据后直接保存坐标，不进行工具判断操作 
                                    }
                                //}
                            }
                            catch (Exception ex)
                            {
                                ExToLog(ex, "18-ToolVip_Click");
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "39-DealSocket");
            }
        }

        /// <summary>
        /// 查询SQL
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            return DatabaseRelated.OracleDriver.OracleComSelected(sql);
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        private int GetScalar(string sql)
        {
            DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            return DatabaseRelated.OracleDriver.OracleComScalar(sql);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void ExToLog(Exception ex, string sFunc)
        {
            BugRelated.ExceptionWrite(ex, "cl110-uc110-" + sFunc);
        }

        private void RecToLog(string s)
        {
            BugRelated.LogWrite(s, Application.StartupPath + "\rec.log");
        }

        //记录操作记录
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() +
                                "','yyyy-mm-dd hh24:mi:ss'),'110接处警','GPS110.案件信息110:" + sql.Replace('\'', '"') + "','" +
                                method + "')";
                RunCommand(strExe);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "WriteEditLog");
            }
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            FillNoIpData(this.cmbField.Text);
        }

        private void CaseSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":

                        FillNoIpData(this.cmbField.Text);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "WriteEditLog");
            }
        }        


        private void toolSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvip.CurrentRow == null) return;
                if (Convert.ToDouble(dgvip.Rows[0].Cells["X"].Value) > 0 && Convert.ToDouble(dgvip.Rows[0].Cells["Y"].Value) > 0)
                {
                    Frmxxd fxxd = new Frmxxd();
                    if (fxxd.ShowDialog(this) == DialogResult.OK)
                    {
                        string sql = "insert into 信息点(ID,名称,类别,MI_STYLE,MI_PRINX,GEOLOC) values(" +
                                    "(select max(ID)+1 from 信息点),'" + fxxd.Xxname + "'," + fxxd.Xxlb + ",'Symbol (34,16711680,9)',(select max(MI_PRINX)+1 from 信息点)," +
                                    "MDSYS.SDO_GEOMETRY(2001,8307,MDSYS.SDO_POINT_TYPE(" + Convert.ToDouble(dgvip.Rows[0].Cells["X"].Value) + "," + Convert.ToDouble(dgvip.Rows[0].Cells["Y"].Value) + ",NULL),NULL,NULL))";
                        RunCommand(sql);
                    }
                    fxxd.Close();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolSave");
            }
        }

        private void dgvNoip_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (dgvNoip.CurrentRow != null)
                        if (dgvNoip.CurrentRow.Cells["x"].Value.ToString() == "" || dgvNoip.CurrentRow.Cells["Y"].Value.ToString() == "")
                        {
                            MessageBox.Show(@"当前数据无坐标值,无法对其进行操作！", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                }
            }
            catch (Exception ex)
            {

                ExToLog(ex, "dgvNoip_MouseDown");
            }
        }

        private void toolNoSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvNoip.CurrentRow == null) return;
                if (Convert.ToDouble(dgvNoip.Rows[0].Cells["X"].Value) > 0 &&
                    Convert.ToDouble(dgvNoip.Rows[0].Cells["Y"].Value) > 0)
                {
                    Frmxxd fxxd = new Frmxxd();
                    if (fxxd.ShowDialog(this) == DialogResult.OK)
                    {
                        string sql = "insert into 信息点(ID,名称,类别,MI_STYLE,MI_PRINX,GEOLOC) values(" +
                                     "(select max(ID)+1 from 信息点),'" + fxxd.Xxname + "'," + fxxd.Xxlb +
                                     ",'Symbol (34,16711680,9)',(select max(MI_PRINX)+1 from 信息点)," +
                                     "MDSYS.SDO_GEOMETRY(2001,8307,MDSYS.SDO_POINT_TYPE(" +
                                     Convert.ToDouble(dgvNoip.Rows[0].Cells["X"].Value) + "," +
                                     Convert.ToDouble(dgvNoip.Rows[0].Cells["Y"].Value) + ",NULL),NULL,NULL))";
                        RunCommand(sql);
                    }
                    fxxd.Close();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolNoSave");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //frmExcut frmset = new frmExcut();
            //frmset.ds = videodis;
            //if(frmset.ShowDialog(this)==DialogResult.OK)
            //{
            //    videodis = frmset.ds;
            //    string exePath = Application.StartupPath;
            //    INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
            //    INIClass.IniWriteValue("视频", "距离", videodis.ToString());

            //    MessageBox.Show(@"修改成功",@"系统提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
            //}
            try
            {
                frmAnjianType frmAnType = new frmAnjianType();
                frmAnType.ds = videodis;
                frmAnType.Show();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }
        }

        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.cmbField.Text.Trim();
                string strExp = string.Empty;

                if (keyword.Length < 1 || colfield.Length < 1) return;

                if (colfield == "案件类型")
                {
                    strExp = "select distinct(类型名称) from 案件类型 where 类型名称 like '%"+ this.CaseSearchBox.Text.Trim()+"%' order by 类型名称";

                    DataTable dt = GetTable(strExp);

                    if (dt.Rows.Count < 1)
                        CaseSearchBox.GetSpellBoxSource(null);
                    else
                        CaseSearchBox.GetSpellBoxSource(dt);
                }
                else
                {
                    CaseSearchBox.GetSpellBoxSource(null);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "CaseSearchBox_TextChanged");
            }
        }

        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                string colfield = this.cmbField.Text.Trim();

                if (colfield == "所属派出所" || colfield == "所属中队" || colfield == "所属警务室")
                {
                    string strExp = string.Empty;

                    if (colfield == "所属派出所")
                        strExp = "select distinct(派出所名) from 基层派出所 order by 派出所名";
                    else if (colfield == "所属中队")
                        strExp = "select distinct(中队名) from 基层民警中队 order by 中队名";
                    else if (colfield == "所属警务室")
                        strExp = "select distinct(警务室名) from 社区警务室 order by 警务室名";

                    DataTable dt = GetTable(strExp);

                    if (dt.Rows.Count <1)
                        CaseSearchBox.GetSpellBoxSource(null);
                    else
                        CaseSearchBox.GetSpellBoxSource(dt);
                }
                else
                {
                    CaseSearchBox.GetSpellBoxSource(null);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "CaseSearchBox_MouseDown");
            }
        }


        public void Writelog(string msg)
        {
            try
            {
                string filepath = Application.StartupPath + "\\timerec.log";
                msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + ":" + msg;

                StreamWriter sw = File.AppendText(filepath);
                sw.WriteLine(msg);
                sw.Flush();
                sw.Close();
            }
            catch
            {
                //Console.WriteLine("写入日志出错");
            }
        }

        private void dgvNoip_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (dgvNoip.Rows.Count != 0)
                {
                    for (int i = 0; i < dgvNoip.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            dgvNoip.Rows[i].DefaultCellStyle.BackColor = Color.WhiteSmoke;
                        }
                        else
                        {
                            dgvNoip.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                    }

                    SetDataGridViewYellow(dgvNoip);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "37-设置DataGrid颜色");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.comboBox1.Text == "报警时间" || this.comboBox1.Text == "发案时间初值" || this.comboBox1.Text == "发案时间终值")
                {
                    frmalarm fa = new frmalarm();
                    if (fa.ShowDialog(this) == DialogResult.OK)
                    {
                        stime = fa.st;
                        etime = fa.et;

                        this.spellSearchBoxEx1.Text = stime + etime;
                    }
                    fa.Close();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "comboBox1_SelectedIndexChanged");
            }
        }

        private void spellSearchBoxEx1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                string colfield = this.comboBox1.Text.Trim();

                if (colfield == "所属派出所" || colfield == "所属中队" || colfield == "所属警务室")
                {
                    string strExp = string.Empty;

                    if (colfield == "所属派出所")
                        strExp = "select distinct(派出所名) from 基层派出所 order by 派出所名";
                    else if (colfield == "所属中队")
                        strExp = "select distinct(中队名) from 基层民警中队 order by 中队名";
                    else if (colfield == "所属警务室")
                        strExp = "select distinct(警务室名) from 社区警务室 order by 警务室名";

                    DataTable dt = GetTable(strExp);

                    if (dt.Rows.Count < 1)
                        this.spellSearchBoxEx1.GetSpellBoxSource(null);
                    else
                        spellSearchBoxEx1.GetSpellBoxSource(dt);
                }
                else
                {
                    spellSearchBoxEx1.GetSpellBoxSource(null);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "spellSearchBoxEx1_MouseDown");
            }
        }

        private void spellSearchBoxEx1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string keyword = this.spellSearchBoxEx1.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();
                string strExp = string.Empty;

                if (keyword.Length < 1 || colfield.Length < 1) return;

                if (colfield == "案件类型")
                {
                    strExp = "select distinct(类型名称) from 案件类型 where 类型名称 like '%" + this.CaseSearchBox.Text.Trim() + "%' order by 类型名称";

                    DataTable dt = GetTable(strExp);

                    if (dt.Rows.Count < 1)
                        spellSearchBoxEx1.GetSpellBoxSource(null);
                    else
                        spellSearchBoxEx1.GetSpellBoxSource(dt);
                }
                else
                {
                    spellSearchBoxEx1.GetSpellBoxSource(null);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "spellSearchBoxEx1_TextChanged");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FillNoXYData(this.comboBox1.Text);

        }

        private void spellSearchBoxEx1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        FillNoXYData(this.comboBox1.Text);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "spellSearchBoxEx1_KeyDown");
            }
        }

        
        private void FillNoXYData(string KeyField)
        {
            try
            {
                isShowPro(true);
                string sRegion2 = strRegion2;
                string sRegion3 = strRegion3;
                string strWhere = getPaiNo(sRegion2);

                //(select 派出所名 from 基层派出所 where t.所属派出所 = 基层派出所.派出所代码)
                // 此常量用于将派出所代码转换成派出所名（由于GPS110.报警信息110的派出所代码与主程序派出所代码不同 所以用此转换）
                const string paiName = "case  when t.所属派出所 like '4406006050%' then '大良' when t.所属派出所 like '4406006051%' then '容桂' when t.所属派出所 like '4406006052%' then '伦教' when t.所属派出所 like '4406006053%' then '北滘' when t.所属派出所 like '4406006054%' then '陈村' when t.所属派出所 like '4406006055%' then '乐从' when t.所属派出所 like '4406006056%' then '龙江' when t.所属派出所 like '4406006057%' then '勒流' when t.所属派出所 like '4406006058%' then '杏坛'  when t.所属派出所 like '4406006059%' then '均安' end ";

                const string fields =
                   "t.报警编号,t.案件名称,t.案件类型,t.案别_案由,t.专案标识,t.发案时间初值,t.发案时间终值," +
                   "t.发案地点详址,t.简要案情,t.报警时间, " + paiName + " as 所属派出所,t.案件来源," +
                   "t.对讲机ID,处理警力编号 as 处理警员,t.x,t.y";
                string sql = string.Empty;
                if (KeyField == "报警时间" || KeyField == "发案时间初值" || KeyField == "发案时间终值")
                {
                    if (strWhere != "")
                        sql = "Select " + fields + " from GPS110.报警信息110 t where (t.x ='' or t.x is null or t.y ='' or t.y is null) and " + KeyField + ">to_date('" + stime + "','YYYY-MM-DD HH24:MI:SS') and " + KeyField + " <to_date('" + etime + "','YYYY-MM-DD HH24:MI:SS') and " + getAnjianType() + " and " + strWhere + " and 备用字段一 is null or 备用字段一='' order by t.报警编号";
                    else
                        sql = "Select " + fields + " from GPS110.报警信息110 t where (t.x ='' or t.x is null or t.y ='' or t.y is null) and " + KeyField + ">to_date('" + stime + "','YYYY-MM-DD HH24:MI:SS') and " + KeyField + " <to_date('" + etime + "','YYYY-MM-DD HH24:MI:SS') and " + getAnjianType() + " and 备用字段一 is null or 备用字段一='' order by t.报警编号";

                }
                else
                {
                    if (strWhere != "")
                        sql = "Select " + fields + " from GPS110.报警信息110 t where (t.x ='' or t.x is null or t.y ='' or t.y is null) and " + KeyField + " like '%" + spellSearchBoxEx1.Text.Trim() + "%' and " + getAnjianType() + " and " + strWhere + " and 备用字段一 is null or 备用字段一='' order by t.报警编号";
                    else
                        sql = "Select " + fields + " from GPS110.报警信息110 t where (t.x ='' or t.x is null or t.y ='' or t.y is null) and " + KeyField + " like '%" + spellSearchBoxEx1.Text.Trim() + "%' and " + getAnjianType() + "  and 备用字段一 is null or 备用字段一='' order by t.报警编号";
                }
                DataTable dt = GetTable(sql);
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("没有查询到对应数据，请重新查询", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                this.toolPro.Value = 1;
                Application.DoEvents();
                #region 导出Excel
                //try
                //{
                //    //excelSql = sql.Substring(0, sql.LastIndexOf("order"));  // 去掉排序,为防止下面连接条件时出错
                //    ////excelSql = "select * " + excelSql.Substring(excelSql.IndexOf("from"));
                    

                //    //if (strRegion2 != "顺德区")
                //    //{
                //    //    if (strRegion2 != "")
                //    //    {
                //    //        if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                //    //        {
                //    //            sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                //    //        }
                //    //        excelSql += " and (权限单位 in ('" + sRegion2.Replace(",", "','") + "'))";
                //    //    }
                //    //    else if (strRegion2 == "")
                //    //    {
                //    //        if (excelSql.IndexOf("where") < 0)
                //    //        {
                //    //            excelSql += " where 1=2 ";
                //    //        }
                //    //        else
                //    //        {
                //    //            excelSql += " and 1=2 ";
                //    //        }
                //    //    }
                //    //}
                //    //excelSql += " order by t.报警编号";
                //    excelSql = sql;
                //    OracleConnection orc = new OracleConnection(mysqlstr);
                //    try
                //    {
                //        orc.Open();
                //        OracleCommand cmd = new OracleCommand(excelSql, orc);
                //        apt1 = new OracleDataAdapter(cmd);
                //        DataTable datatableExcel = new DataTable();
                //        apt1.Fill(PagenCurrent3 == 0 ? 0 : PagenCurrent3 - pageSize3 < 0 ? 0 : PagenCurrent3 - pageSize3, pageSize3, datatableExcel);
                //        if (dtExcel != null) dtExcel.Clear();
                //        dtExcel = datatableExcel;
                //        cmd.Dispose();
                //    }

                //    catch (Exception ex)
                //    {
                //        isShowPro(false);
                //        ExToLog(ex, "008-判断导出列表数量");
                //    }
                //    finally { orc.Close(); }
                //}
                //catch (Exception ex)
                //{
                //    isShowPro(false);
                //    Console.WriteLine(ex.Message);
                //}
                //this.toolPro.Value = 2;
                //Application.DoEvents();
                # endregion

                //Add110NovipFtr(dt);  //添加图元
                Pagedt3 = dt;
                InitDataSet3(RCount3, PageNow3, PageNum3, bindingSource3, bindingNavigator3, dataGridView1);  //分页 

                Create110NovipLayer();//创建临时图层

                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                ExToLog(ex, "10-填充非本机案件DataGrid");
            }
        }

        #region  未定位数据翻页功能



        private DataTable Pagedt3 = new DataTable();
        private int PagenCurrent3; //当前记录行 
        private int PagenMax3; //总记录数
        private int pageCount3; //页数＝总记录数/每页显示行数
        private int pageCurrent3; //当前页号
        private int pageSize3 = 100; //每页显示行数

        public void InitDataSet3(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount,
                                 BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize1 = 100;      //设置页面行数
                PagenMax3 = Pagedt3.Rows.Count;
                PageNumber3.Text = pageSize3.ToString();
                RCount3.Text = @"共" + PagenMax3.ToString() + @"条记录"; //在导航栏上显示总记录数

                pageCount3 = (PagenMax3 / pageSize3); //计算出总页数
                if ((PagenMax3 % pageSize3) > 0) pageCount3++;
                if (PagenMax3 != 0)
                {
                    pageCurrent3 = 1;
                }
                PagenCurrent3 = 0; //当前记录数从0开始

                LoadData3(textNowPage, lblPageCount, bs, bn, dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExToLog(ex, "26-InitDataSet1");
            }
        }


        public void LoadData3(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds,
                              BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                isShowPro(true);
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = Pagedt3.Clone();

                if (pageCurrent3 == pageCount3)
                    nEndPos = PagenMax3;
                else
                    nEndPos = pageSize3 * pageCurrent3;
                nStartPos = PagenCurrent3;

                //tsl.Text = Convert.ToString(pageCurrent1) + "/" + pageCount1.ToString();
                PageNow3.Text = Convert.ToString(pageCurrent3);
                PageNum3.Text = "/" + pageCount3.ToString();
                this.toolPro.Value = 1;
                Application.DoEvents();
                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt3.Rows[i]);
                    PagenCurrent3++;
                }
                if (dtExcel != null) dtExcel.Clear();

                bds.DataSource = dtExcel = dtTemp;
                bdn.BindingSource = bds;
                this.toolPro.Value = 1;
                Application.DoEvents();
                dgv.DataSource = bds;
                this.toolPro.Value = 1;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                ExToLog(ex, "27-LoadData");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void bindingNavigator3_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Text)
                {
                    case "上一页":
                        pageCurrent3--;
                        if (pageCurrent3 < 1)
                        {
                            pageCurrent3= 1;
                            MessageBox.Show("已经是第一页，请点击“下一页”查看！");
                            return;
                        }
                        else
                        {
                            PagenCurrent3 = pageSize3 * (pageCurrent3 - 1);
                        }
                        LoadData3(PageNow3, PageNum3, bindingSource3, bindingNavigator3, dataGridView1);
                        break;
                    case "下一页":
                        pageCurrent3++;
                        if (pageCurrent3 > pageCount3)
                        {
                            pageCurrent3 = pageCount3;
                            MessageBox.Show("已经是最后一页，请点击“上一页”查看！");
                            return;
                        }
                        else
                        {
                            PagenCurrent3 = pageSize3 * (pageCurrent3 - 1);
                        }
                        LoadData3(PageNow3, PageNum3, bindingSource3, bindingNavigator3, dataGridView1);
                        break;
                    case "转到首页":
                        if (pageCurrent3 <= 1)
                        {
                            MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                            MessageBoxDefaultButton.Button2);
                            return;
                        }
                        else
                        {
                            pageCurrent3 = 1;
                            PagenCurrent3 = pageSize3 * (pageCurrent3 - 1);
                        }
                        LoadData3(PageNow3, PageNum3, bindingSource3, bindingNavigator3, dataGridView1);
                        break;
                    case "转到尾页":
                        if (pageCurrent3 > pageCount3 - 1)
                        {
                            MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK,
                                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                            return;
                        }
                        else
                        {
                            pageCurrent3 = pageCount3;
                            PagenCurrent3 = pageSize3 * (pageCurrent3 - 1);
                        }
                        LoadData3(PageNow3, PageNum3, bindingSource3, bindingNavigator3, dataGridView1);
                        break;
                    case "数据导出":
                        break;
                }
                //SetDataGridViewYellow(dataGridView1);

                #region 数据导出
                //DataTable datatableExcel = new DataTable();
                //apt1.Fill(PagenCurrent3 == 0 ? 0 : PagenCurrent3 - pageSize3 < 0 ? 0 : PagenCurrent3 - pageSize3, pageSize3, datatableExcel);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExToLog(ex, "32-bindingNavigator2_ItemClicked");
            }
        }


        //页面跳转
        private void PageNow3_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(PageNow3.Text) < 1 || Convert.ToInt32(PageNow3.Text) > pageCount3)
                    {
                        MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                        MessageBoxDefaultButton.Button2);
                        PageNow3.Text = pageCurrent3.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent3 = Convert.ToInt32(PageNow3.Text);
                        PagenCurrent3 = pageSize3 * (pageCurrent3 - 1);
                        LoadData3(PageNow3, PageNum3, bindingSource3, bindingNavigator3, dataGridView1);
                        //SetDataGridViewYellow(dataGridView1);
                    }
                    #region 数据导出
                    //DataTable datatableExcel = new DataTable();
                    //apt1.Fill(PagenCurrent3 == 0 ? 0 : PagenCurrent3 - pageSize3 < 0 ? 0 : PagenCurrent3 - pageSize3, pageSize3, datatableExcel);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "35-toolStripTextBox1_KeyPress");
            }
        }


       
        #endregion

        private void PageNumber3_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView1.Rows.Count > 0)
                {
                    pageSize3 = Convert.ToInt32(PageNumber3.Text);
                    pageCurrent3 = 1; //当前转到第一页
                    PagenCurrent3 = pageSize3 * (pageCurrent3 - 1);
                    pageCount3 = (PagenMax3 / pageSize3); //计算出总页数
                    if ((PagenMax3 % pageSize3) > 0) pageCount3++;

                    LoadData3(PageNow3, PageNum3, bindingSource3, bindingNavigator3, dataGridView1);

                    //SetDataGridViewYellow(dgvNoip);

                    #region 数据导出
                    //DataTable datatableExcel = new DataTable();
                    //apt1.Fill(PagenCurrent3 == 0 ? 0 : PagenCurrent3 - pageSize3 < 0 ? 0 : PagenCurrent3 - pageSize3, pageSize3, datatableExcel);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "36-toolStripTextBox2_KeyPress");
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count != 0)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.WhiteSmoke;
                        }
                        else
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                    }

                    SetDataGridViewYellow(dataGridView1);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "37-设置DataGrid颜色");
            }
        }

        private void toolXY_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show(@"没有选择任何数据，无法进行当前操作！", @"系统提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information);
                    return;
                }

                switch (this.toolXY.Text)
                {
                    case "结束标注":
                        //if (MessageBox.Show(@"是否结束坐标标注？", @"系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                        //{
                            Update110XY(this.dataGridView1.CurrentRow.Cells["报警编号"].Value.ToString(),
                                        this.dataGridView1.CurrentRow.Cells["X"].Value.ToString(),
                                        this.dataGridView1.CurrentRow.Cells["Y"].Value.ToString());

                            MapControl1.Tools.LeftButtonTool = "Select";
                            toolXY.Text = @"坐标标注";
                        //}
                        break;
                    case "坐标标注":

                        //if (
                        //    MessageBox.Show(@"确定要进行坐标标注吗？", @"系统提示", MessageBoxButtons.OKCancel,
                        //                    MessageBoxIcon.Information) ==
                        //    DialogResult.OK)
                        //{

                            MapControl1.Tools.LeftButtonTool = "MoveTool3";
                            toolXY.Text = @"结束标注";
                        //}

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "18-ToolVip_Click");
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
                ExToLog(ex, "isShowPro");
            }
        }

        // 获取查询的案件类型
        private string getAnjianType()
        {
            // 此两项为默认类型
            string strType = "'刑事案件','治安案件'";
            try
            {
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                string strSit = CLC.INIClass.IniReadValue("110", "案件类型");

                if (strSit != null && strSit != "")
                    strType += ",'" + strSit.Replace(",", "','") + "'";

                return " 案件类型 in(" + strType + ")";
            }
            catch(Exception ex) { ExToLog(ex, "getAnjianType"); return " 案件类型 in(" + strType + ")"; }
        }

        // 未标警情双击显示详细信息 add by lili 
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //点击表头,退出

            if (dataGridView1.CurrentRow == null) return;
            try
            {
                // 此常量用于将派出所代码转换成派出所名（由于GPS110.报警信息110的派出所代码与主程序派出所代码不同 所以用此转换）
                const string paiName = "case  when t.所属派出所 like '4406006050%' then '大良'" +
                                            " when t.所属派出所 like '4406006051%' then '容桂'" +
                                            " when t.所属派出所 like '4406006052%' then '伦教'" +
                                            " when t.所属派出所 like '4406006053%' then '北滘'" +
                                            " when t.所属派出所 like '4406006054%' then '陈村'" +
                                            " when t.所属派出所 like '4406006055%' then '乐从'" +
                                            " when t.所属派出所 like '4406006056%' then '龙江'" +
                                            " when t.所属派出所 like '4406006057%' then '勒流'" +
                                            " when t.所属派出所 like '4406006058%' then '杏坛'" +
                                            " when t.所属派出所 like '4406006059%' then '均安' end ";

                const string sqlFields =
                     "t.报警编号,t.案件名称,t.案件类型,t.案别_案由,t.专案标识,t.发案时间初值,t.发案时间终值," +
                     "t.发案地点详址,t.简要案情,t.报警时间, " + paiName + " as 所属派出所," +
                     "t.对讲机ID,处理警力编号 as 处理警员,t.X,t.Y ";

                string strSQL = "Select " + sqlFields + " from GPS110.报警信息110 t where t.报警编号 = '" +
                                dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'";

                DataTable datatable = GetTable(strSQL);
                Point pt = new Point();

                Screen scren = Screen.PrimaryScreen;
                pt.X = scren.WorkingArea.Width / 2;
                pt.Y = 10;
                disPlayInfo(datatable, pt);
            }
            catch (Exception exce)
            {
                ExToLog(exce, "17-数据表dataGridView1双击");
            }
        }

        // 从已办警情关联到直观指挥  add by lili
        private void toolToZhihui_Click(object sender, EventArgs e)
        {
            oneToZhihui(this.dgvNoip);
        }

        // 从未标警情关联到直观指挥  add by lili
        private void toolNoZhihui_Click(object sender, EventArgs e)
        {
            oneToZhihui(this.dataGridView1);
        }

        /// <summary>
        /// 从110关联到直观指挥   add by lili
        /// </summary>
        private void oneToZhihui(DataGridView dataGrid)
        {
            try
            {
                string pliceNo = dataGrid.CurrentRow.Cells[0].Value.ToString();   // 获取报警编号用于直观指挥查询

                if (pliceNo != string.Empty || pliceNo != null)
                {
                    ucZhihui zhiHui = new ucZhihui(this.MapControl1, ConStr, st, tdb, VideoPort, VideoString, VEpath, getFromNamePath);
                    zhiHui.panError = this.panError;
                    zhiHui.plicNo = pliceNo;
                    zhiHui.User = this.user;
                    zhiHui.getNetParameter(ns, vf);
                    zhiHui.InitZhihui();

                    // 先关闭110窗体
                    this.Visible = false;

                    /// 如果之前有过关联操作，先移除
                    /// 上次操作生成的控件，防止生成
                    /// 太多控件导致程序运行过慢。
                    if (split.Panel1.Controls.Count >11)     
                        split.Panel1.Controls.Remove(split.Panel1.Controls[11]);

                    // 切换到直观指挥
                    split.Panel1.Controls.Add(zhiHui);
                    zhiHui.Dock = DockStyle.Fill;
                    zhiHui.Visible = true;

                }
                else
                {
                    MessageBox.Show("本条数据由于确少报警编号因此无法关联！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "18-从110关联到直观指挥-oneToZhihui");
            }
        }
    }
}