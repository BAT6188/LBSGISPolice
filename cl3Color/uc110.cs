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

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace cl110
{
    public partial class uc110 : UserControl
    {
        public uc110()
        {
            InitializeComponent();
        }

       
        public MapControl mapControl1 = null;
        private string mysqlstr;
        public string strRegion = string.Empty;
        public string strRegion1 = "";
        public string user = "";

        private string ip;


        private int VideoPort = 0;           //通讯端口
        private string[] VideoString;     // 视频连接字符
        private ToolStripLabel st = null;
        private NetworkStream ns = null;  //
        private ToolStripDropDownButton tdb;
        private double videodis; 

        private Boolean vf = false; // 通讯是否已经连接的标识
        private string VEpath = string.Empty;

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
        public void getParameter(MapControl m, string s, ToolStripLabel tools, ToolStripDropDownButton td, int port, string[] vs, string videopath,double dist)
        {
            mapControl1 = m;
            mysqlstr = s;
            st = tools;
            this.tdb = td;
            VideoPort = port;
            VideoString = vs;
            this.VEpath = videopath;
            videodis = dist;
        }

        public void getNetParameter(NetworkStream ns1, Boolean vf1)
        {
            this.ns = ns1;
            this.vf = vf1;
        }



        /// <summary>
        /// 初始化110模块
        /// </summary>
        public void Init110()
        {
            string host1 = Dns.GetHostName();
            IPHostEntry hostIp1 = Dns.GetHostByName(host1);
            ip = hostIp1.AddressList[0].ToString();

            Create110VIPLayer();

            FillIpData();
        }


        public void cleartemp()
        {
        }

        public void Dipose110()
        {
        }

        private void FillIpData()
        {
            OracleConnection orc = new OracleConnection(mysqlstr);

            string fields = "案件名称,报警编号,案发状态,案件类型,案别_案由,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地点详址,所属社区,简要案情,作案手段特点,发案地政区划,发案场所";

            OracleCommand cmd = new OracleCommand("Select " + fields + " from 报警信息 where 接警IP like '%" + ip + "%' and 处理状态 is null and x is not null and y is not null", orc);
            orc.Open();
            cmd.ExecuteNonQuery();
            OracleDataAdapter apt = new OracleDataAdapter(cmd);
            DataTable dt = new DataTable();
            apt.Fill(dt);

            //if (dtExcel != null) dtExcel.Clear();
            //dtExcel = dt;

            //Pagedt1 = dt;
            //InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, this.dataGridView1);


            ////this.label2.Text = "共有" + dt.Rows.Count.ToString() + "条记录";

            this.dgvip.DataSource = dt;
            dgvip.Refresh();
            //WriteEditLog(cmd.CommandText, "查询");
            orc.Close();
            cmd.Dispose();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                Create110VIPLayer();
                FillIpData();               
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                Create110NOVIPLayer();
                FillNoIpData();
                
            }
        }

        private void Create110VIPLayer() 
        {
            //writelog("创建车辆图层开始" + System.DateTime.Now.ToString());
            try
            {

                if (mapControl1.Map.Layers["JCLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("JCLayer");
                }

                if (mapControl1.Map.Layers["JCLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("JCLabel");
                }


                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("JCLayer");
                Table tblTemp = Cat.GetTable("JCLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("JCLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("AID", 50));


                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);

                //添加标注
                string activeMapLabel = "JCLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("JCLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
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
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                Add110VIPFtr();  
            }
            catch (Exception ex)
            {
                writeToLog(ex, "创建车辆临时图层");
            } 
        }

        public void Add110VIPFtr()
        {
            OracleConnection conf = new OracleConnection(mysqlstr);

            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["JCLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("JCLayer");

                string fields = "案件名称,报警编号,案发状态,案件类型,案别_案由,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地点详址,所属社区,简要案情,作案手段特点,发案地政区划,发案场所,x,y";

                OracleCommand cmd = new OracleCommand("Select " + fields +" from 报警信息 where 接警IP like '%" + ip + "%' and 处理状态 is null and x is not null and y is not null", conf);
                conf.Open();
                cmd.ExecuteNonQuery();
                OracleDataAdapter apt = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["案件名称"].ToString();
                        string tempid = dr["报警编号"].ToString();
                      
                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();
                        cs.ApplyStyle(new BitmapPointStyle("PIN2-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 16));

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = tempname;
                        ftr["AID"] = tempid;
                        tblcar.InsertFeature(ftr);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "初始化添加车辆");
            }
            finally
            {
                conf.Close();
            }
        }


        private void Create110NOVIPLayer() 
        {            
            try
            {

                if (mapControl1.Map.Layers["JCLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("JCLayer");
                }

                if (mapControl1.Map.Layers["JCLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("JCLabel");
                }


                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("JCLayer");
                Table tblTemp = Cat.GetTable("JCLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("JCLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("AID", 50));


                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Insert(0, lyr);

                //添加标注
                string activeMapLabel = "JCLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("JCLayer");
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

                Add110NOVIPFtr();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "创建车辆临时图层");
            }
        }

        public void Add110NOVIPFtr() 
        {
            OracleConnection conf = new OracleConnection(mysqlstr);

            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["JCLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("JCLayer");

                string fields = "案件名称,报警编号,案发状态,案件类型,案别_案由,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地点详址,所属社区,简要案情,作案手段特点,发案地政区划,发案场所,X,Y";

                OracleCommand cmd = new OracleCommand("Select " + fields +" from 报警信息 where 接警IP <> '" + ip + "' and 处理状态 is null and x is not null and y is not null", conf);
                conf.Open();
                cmd.ExecuteNonQuery();
                OracleDataAdapter apt = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["案件名称"].ToString();
                        string tempid = dr["报警编号"].ToString();

                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();
                        cs.ApplyStyle(new BitmapPointStyle("PIN3-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 16));
                                                
                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = tempname;
                        ftr["AID"] = tempid;
                        tblcar.InsertFeature(ftr);
                     }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "初始化添加车辆");
            }
            finally
            {
                conf.Close();
            }
        }
               

        private void FillNoIpData()
        {
            OracleConnection orc = new OracleConnection(mysqlstr);

            string fields = "案件名称,报警编号,案发状态,案件类型,案别_案由,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地点详址,所属社区,简要案情,作案手段特点,发案地政区划,发案场所";

            OracleCommand cmd = new OracleCommand("Select " + fields + " from 报警信息 where 接警IP  <>'" + ip + "' and 处理状态 is null and x is not null and y is not null", orc);
            orc.Open();
            cmd.ExecuteNonQuery();
            OracleDataAdapter apt = new OracleDataAdapter(cmd);
            DataTable dt = new DataTable();
            apt.Fill(dt);

            //if (dtExcel != null) dtExcel.Clear();
            //dtExcel = dt;

            //Pagedt1 = dt;
            //InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, this.dataGridView1);


            ////this.label2.Text = "共有" + dt.Rows.Count.ToString() + "条记录";

            this.dgvNoip.DataSource = dt;
            dgvNoip.Refresh();
            //WriteEditLog(cmd.CommandText, "查询");
            orc.Close();
            cmd.Dispose();
        }





        private double CurrentX = 0;
        private double CurrentY = 0; 

        private void btnDeal1_Click(object sender, EventArgs e)
        {
            //2. 校正x.y


            //1. 显示周边
            DPoint dp = new DPoint(CurrentX ,CurrentY);

            SearchDistance(dp, 0, true, videodis, true);
            
            //3. 处理信息 
        }
                
        
        
        
        //一定范围内的视频和车辆，创建视频图层和车辆图层，然后选择一定范围内的

        
        private void SearchDistance(DPoint dp, double distCar, bool carflag, double distVideo, bool videoflag)
        {
            // 创建 视频图层             
            if (videoflag == true)
            {
                clVideo.ucVideo fv = new clVideo.ucVideo();
                fv.getParameter(mapControl1, st, tdb, mysqlstr, this.VideoPort, this.VideoString, VEpath, true);
                fv.getNetParameter(ns, vf);
                fv.strRegion = this.strRegion;
                fv.SearchVideoDistance(dp, distVideo);
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

        private void writeToLog(Exception ex, string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine(DateTime.Now.ToString() + ": " + "110接处警:在 " + sFunc + "方法中发生错误。");
            sw.WriteLine(ex.ToString());
            sw.Close();
        }


        private string SelectftrName = string.Empty;
        private int iflash = 0;
        private IResultSetFeatureCollection rsfcflash = null;

        private void dgvip_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string tempname = this.dgvip.CurrentRow.Cells[1].Value.ToString();

                string tblname = "JCLayer";

                //提取当前选择的信息的通行车辆编号作为主键值

                MapInfo.Mapping.Map map = this.mapControl1.Map;

                if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                rsfcflash = null;

                MapInfo.Data.MIConnection conn = new MIConnection();
                conn.Open();

                MapInfo.Data.MICommand cmd = conn.CreateCommand();
                Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where AID = @name ";
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
                    cmd.Dispose();
                    conn.Close();

                    iflash = 0;

                    this.timeflash.Enabled = true;
                }             
            }
            catch (Exception ex)
            {
                writeToLog(ex, "在dgvip列表上双击");
            }
        }


        private void timeflash_Tick(object sender, EventArgs e)
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
                    
                    timeflash.Enabled = false;

                    //GetGzPoistion();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "案件图元闪现");
            }
        }

        private void dgvNoip_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string tempname = this.dgvNoip.CurrentRow.Cells[1].Value.ToString();

                string tblname = "JCLayer";

                //提取当前选择的信息的通行车辆编号作为主键值

                MapInfo.Mapping.Map map = this.mapControl1.Map;

                if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                rsfcflash = null;

                MapInfo.Data.MIConnection conn = new MIConnection();
                conn.Open();

                MapInfo.Data.MICommand cmd = conn.CreateCommand();
                Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where AID = @name ";
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
                    cmd.Dispose();
                    conn.Close();

                    iflash = 0;

                    this.timeflash.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "在dgvip列表上双击");
            }
        }

        private void dgvip_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                DPoint dp = new DPoint();
                string sqlFields = "案件名称,报警编号,案发状态,案件类型,案别_案由,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地点详址,所属社区,简要案情,作案手段特点,发案地政区划,发案场所,现场警力编号,x,y";
                string strSQL = "Select " + sqlFields + " from 报警信息 where 报警编号 = '" + this.dgvip.CurrentRow.Cells[1].Value.ToString() + "'and x is not null and y is not null";
                              
                DataTable datatable = new DataTable();
                DataSet ds = new DataSet();
                OracleDataAdapter da = new System.Data.OracleClient.OracleDataAdapter(strSQL, Conn);
                da.Fill(ds);
                Conn.Close();
                datatable = ds.Tables[0];
                
                if (datatable.Rows.Count > 0)
                {
                    //闪现图元
                    //////////////////////////////////////////
                    string tempname = this.dgvip.CurrentRow.Cells[1].Value.ToString();

                    string tblname = "JCLayer";

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    rsfcflash = null;


                    MapInfo.Data.MIConnection conn = new MIConnection();

                    try
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                        conn.Open();

                        MapInfo.Data.MICommand cmd = conn.CreateCommand();
                        Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                        cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where AID = @name ";
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
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }
                            cmd.Dispose();

                            this.timeflash.Enabled = true;
                        }
                    }
                    catch { }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                    }

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
                    //WriteEditLog("终端车辆号牌='" + this.dataGridViewKakou.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情", "V_CROSS");
                }
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeToLog(ex, "数据表dataGridViewKakou双击");
                writeToLog(ex, "xxxxxx 7");
            }
        }

        /// <summary>
        /// 显示车辆详细信息
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
                frminfo.setInfo(dt.Rows[0], pt, mysqlstr, user);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " 显示报警详细信息");
            }
        }

        private void dgvNoip_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                DPoint dp = new DPoint();
                string sqlFields = "案件名称,报警编号,案发状态,案件类型,案别_案由,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地点详址,所属社区,简要案情,作案手段特点,发案地政区划,发案场所,现场警力编号,x,y";
                string strSQL = "Select " + sqlFields + " from 报警信息 where 报警编号 = '" + this.dgvNoip.CurrentRow.Cells[1].Value.ToString() + "'and x is not null and y is not null";

                DataTable datatable = new DataTable();
                DataSet ds = new DataSet();
                OracleDataAdapter da = new System.Data.OracleClient.OracleDataAdapter(strSQL, Conn);
                da.Fill(ds);
                Conn.Close();
                datatable = ds.Tables[0];

                if (datatable.Rows.Count > 0)
                {
                    //闪现图元
                    //////////////////////////////////////////
                    string tempname = this.dgvNoip.CurrentRow.Cells[1].Value.ToString();

                    string tblname = "JCLayer";

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    rsfcflash = null;


                    MapInfo.Data.MIConnection conn = new MIConnection();

                    try
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                        conn.Open();

                        MapInfo.Data.MICommand cmd = conn.CreateCommand();
                        Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                        cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where AID = @name ";
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
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }
                            cmd.Dispose();

                            this.timeflash.Enabled = true;
                        }
                    }
                    catch { }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                    }

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
                    //WriteEditLog("终端车辆号牌='" + this.dataGridViewKakou.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情", "V_CROSS");
                }
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeToLog(ex, "数据表dataGridViewKakou双击");
                writeToLog(ex, "xxxxxx 7");
            }
        }
    }
}
