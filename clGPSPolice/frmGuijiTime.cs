using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace clGPSPolice   
{
    public partial class FrmGuijiTime : Form
    {
        public MapControl MapControl1;       // 地图控件
        private readonly string _mysqlstr;   // 数据库连接字符串
        private readonly string[] _conStr;   // 数据库连接参数
        public double Lx;                    // 上次的经度
        public double Ly;                    // 上次的纬度
        
        private string _cnname = string.Empty ;   // 车辆名称

        public string Userid = "";           // 数据连接用户名
        public string Datasource = "";       // 数据连接数据源
        public string Password = "";         // 数据连接密码

        public string User = "";             // 程序登录用户名

        private Boolean _jzflag;


        /// <summary>
        /// 初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="s">数据库连接参数</param>
        public FrmGuijiTime(string[] s)
        {
            try
            {
                InitializeComponent();
                _conStr = s;

                _mysqlstr = "data source=" + _conStr[0] + ";user id=" + _conStr[1] + ";password=" + _conStr[2];

                AddCarName();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "01-初始化");
            }
        }


        public void Getflag(Boolean jz)
        {
            _jzflag = jz;
        }

        /// <summary>
        /// 添加车辆名称
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void AddCarName()
        {
            OracleConnection con = new OracleConnection(_mysqlstr);
            try
            {
                con.Open();
                OracleCommand cmdadd = new OracleCommand("select 警力编号 from GPS警员 order by 警力编号", con);
                OracleDataReader dr = cmdadd.ExecuteReader();

                while (dr.Read())
                {
                    comboBox1.Items.Add(Convert.ToString(dr.GetString(0)));
                }
                dr.Close();
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "02-添加警力编号名称");
            }
            finally {
                con.Close();
            }
        }

        /// <summary>
        /// 开始播放轨迹
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Button1Click(object sender, EventArgs e)
        {
            try
            {
                if (txtplyspe.Text == "")
                {
                    MessageBox.Show(@"播放速度不能为空");
                    return;
                }
                if (Convert.ToInt32(txtplyspe.Text) <= 0)
                {
                    MessageBox.Show(@"播放速度不能小于零");
                    return;
                }

                Int32 psp = Convert.ToInt32(txtplyspe.Text);

                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                //string cn = this.comboBox1.Text;

                string cn = GetCarName();

                if (cn != "")
                {
                    comboBox1.Text = cn;
                }

                if (dateTimePicker1.Text == "" || dateTimePicker2.Text == "")
                {
                    MessageBox.Show(@"起始时间不能为空！", @"系统提示");
                    return;
                }

                if (dateTimePicker3.Text == "" || dateTimePicker4.Text == "")
                {
                    MessageBox.Show(@"结束时间不能为空！", @"系统提示");
                    return;
                }

                if (cn == "")
                {
                    MessageBox.Show(@"车辆名称为必选项，不能为空！", @"系统提示");
                    return;
                }


                if (dte < dts)
                {
                    MessageBox.Show(@"起始时间不能大于或等于终止时间！", @"系统提示");
                    return;
                }

                _cnname = cn;

                bool df = CreateHistoryDate();
                if (df == false)
                {
                    return;
                }

                bool lf = CreateHistoryLayer();
                if (lf == false)
                {
                    return;
                }

                bool ff = CreateHisFtr();
                if (ff == false)
                {
                    return;
                }

                if (_hisx.Length == 1|| _hisy.Length ==1) return;

                //this.Visible = false;
                if (btncar1.Enabled)
                    btncar1.Enabled = false;
                if (btncar2.Enabled == false)
                    btncar2.Enabled = true;
                if (btncar3.Enabled)
                    btncar3.Enabled = false;
                if (btncar4.Enabled == false)
                    btncar4.Enabled = true;


                timer1.Interval = psp*1000;
                timer1.Enabled = true;

                WriteEditLog(cn, "轨迹回放");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "03-开始播放轨迹");
            }
        }
        
        /// <summary>
        /// 获得车辆名称
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>车辆名称</returns>
        private string GetCarName()
        {            
            OracleConnection con = new OracleConnection(_mysqlstr);
            string carname = "";
            try
            {
                string sqlstring = "select * from gps警员 where 警力编号 like '%" + comboBox1.Text.Trim() + "%' order by 警力编号";
                con.Open();
                OracleCommand cmd = new OracleCommand(sqlstring, con);
                cmd.ExecuteNonQuery();
                OracleDataAdapter apt = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);
                if (dt.Rows.Count == 1)
                    foreach (DataRow dr in dt.Rows)
                        carname = Convert.ToString(dr["警力编号"]);
                else
                    MessageBox.Show(@"没有找到与关键字唯一匹配的警员信息", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "04-获取警员名称");              
            }
            return carname;
        }

        double[] _hisx;   // 历史轨迹的x值
        double[] _hisy;   // 历史轨迹的Y值
        string[] _hisdate;// 历史轨迹的时间值
        string[] _hisspe; // 历史轨迹的速度值
        string[] _hisver; // 历史轨迹的方向值 

        /// <summary>
        /// 创建历史数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>布尔值(true-创建成功 false-创建失败)</returns>
        public Boolean  CreateHistoryDate()
        {            
            OracleConnection con = new OracleConnection(_mysqlstr);
            try
            {
                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                if (dte > dts)
                {

                    string sqlstring = "select 警力情况表.警力编号,GPS110.警力GPS位置.X,GPS110.警力GPS位置.Y, GPS110.警力GPS位置.定位更新时间 from GPS110.警力GPS位置,警力情况表 where  GPS110.警力GPS位置.定位更新时间> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and  GPS110.警力GPS位置.定位更新时间< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and GPS110.警力GPS位置.对讲机ID=警力情况表.设备编号 and 警力情况表.警力编号 = '" + _cnname + "' order by  GPS110.警力GPS位置.定位更新时间";
                    con.Open();
                    OracleCommand cmdsel = new OracleCommand(sqlstring, con);
                    cmdsel.ExecuteNonQuery();

                    OracleDataAdapter aptsel = new OracleDataAdapter(cmdsel);
                    DataTable dtsel = new DataTable();
                    aptsel.Fill(dtsel);

                    if (dtsel.Rows.Count > 0)
                    {
                        _hisx = new double[dtsel.Rows.Count];
                        _hisy = new double[dtsel.Rows.Count];
                        _hisdate = new string[dtsel.Rows.Count];
                        _hisspe = new string[dtsel.Rows.Count];
                        _hisver = new string[dtsel.Rows.Count];

                        int i = 0;
                        foreach (DataRow dr in dtsel.Rows)
                        {
                            //获取经纬度                                                    
                            _hisx[i] = Convert.ToDouble(dr["X"]);
                            _hisy[i] = Convert.ToDouble(dr["Y"]);
                            _hisdate[i] = Convert.ToString(dr["定位更新时间"]);
                          
                            i = i + 1;
                        }

                        return true;
                    }
                    else
                    {
                        MessageBox.Show(@"当前时间段内无该警员的历史轨迹数据,请重新选择", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;

                    }
                }
                else
                {
                    MessageBox.Show(@"选择的时间段结束时间不大于开始时间,将无法演示,请重新选择", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            catch(Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                ExToLog(ex, "05-创建历史数据");
                return false;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();                
            }
        }


        /// <summary>
        /// 创建车辆图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>布尔值(true-创建成功 false-创建失败)</returns>
        public Boolean  CreateHistoryLayer()
        {
            try
            {               

                Catalog cat = Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("PoliceGuijiLayer");
                Table tblTemp = cat.GetTable("PoliceGuijiLayer");
                if (tblTemp != null) //Table exists close it
                {
                    cat.CloseTable("PoliceGuijiLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                MapControl1.Map.Layers.Add(lyr);

                return true;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "06-创建警员图层");
                return false;
            }
        }


        private int _hi; 

        /// <summary>
        /// 创建历史轨迹的点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>布尔值(true-创建成功 false-创建失败)</returns>
        public Boolean  CreateHisFtr()
        {
            try
            {
                if (_hisx.Length > 0 && _hisy.Length > 0 && (_hisx.Length == _hisy.Length))
                {
                    double xx = 0;
                    double yy = 0;

                    _hi = 0;

                    xx = _hisx[_hi];
                    yy = _hisy[_hi];

                    //Road = GetNearestRoad(xx, yy);

                    Lx = xx;
                    Ly = yy;

                    FeatureLayer lyrcar = MapControl1.Map.Layers["PoliceGuijiLayer"] as FeatureLayer;
                    Table tblcar = Session.Current.Catalog.GetTable("PoliceGuijiLayer");

                    if (lyrcar != null)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xx, yy));
                        //CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(43,System.Drawing.Color.Red, 20));

                        //CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("POLI1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Blue , 16));
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("jc.bmp", BitmapStyles.ApplyColor, Color.Red, 20));
                    
                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = _cnname;
                        tblcar.InsertFeature(ftr);
                        MapControl1.Map.Center = ftr.Geometry.Centroid;
                    }
                    //MapControl1.Map.Zoom = new Distance(2500, DistanceUnit.Meter);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "07-创建历史轨迹的点");
                return false;
            }            
        }

        /// <summary>
        /// 移动车辆
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="x">当前X坐标</param>
        /// <param name="y">当前Y坐标</param>
        /// <param name="lx">要移动的X坐标</param>
        /// <param name="ly">要移动的Y坐标</param>
        /// <param name="cnum">车辆编号</param>
        public void MoveHisFtr(double x, double y, double lx, double ly, string cnum)
        {
            try
            {
                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("PoliceGuijiLayer");

                MapInfo.Mapping.Map map = MapControl1.Map;

                tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);

                foreach (Feature fcar in tbl)
                {
                    if (fcar["Name"].ToString() == cnum)
                    {
                        fcar.Geometry.GetGeometryEditor().OffsetByXY(x - lx, y - ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                        MapInfo.Geometry.DPoint dpoint;
                        MapInfo.Geometry.Point point;

                        dpoint = new DPoint(x, y);
                        point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                        fcar.Geometry = point;
                        TrackHistoryline(x, y, lx, ly);
                        Boolean inf = this.IsInBounds(x, y);
                        MapControl1.Map.Center = dpoint;
                    }

                    fcar.Geometry.EditingComplete();
                    fcar.Update();

                    MapControl1.Refresh();

                    tbl.EndAccess();
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "08-移动车辆");
            }
        }

        /// <summary>
        /// 窗体关闭
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void frmGuijiTime_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.Visible = false;
                e.Cancel = true;
                this.Left = Screen.PrimaryScreen.WorkingArea.Left;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "09-窗体关闭");
            }
        }

        /// <summary>
        /// 创建历史轨迹线 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="x1">当前X坐标</param>
        /// <param name="y1">当前Y坐标</param>
        /// <param name="x2">要移动的X坐标</param>
        /// <param name="y2">要移动的Y坐标</param>
        public void TrackHistoryline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = MapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["Track"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("Track");

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(2, MapInfo.Styles.LineWidthUnit.Pixel), 2, System.Drawing.Color.Blue);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "10-创建历史轨迹线");
            }
        }

        /// <summary>
        /// 使警员移动
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                FeatureLayer lyr = MapControl1.Map.Layers["PoliceGuijiLayer"] as FeatureLayer;

                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("PoliceGuijiLayer");

                MapInfo.Mapping.Map map = MapControl1.Map;    

                MapInfo.Data.IResultSetFeatureCollection rsfc = session.Selections.DefaultSelection[lyr.Table];
              
                if (tbl != null)
                {
                    OracleConnection con = new OracleConnection(_mysqlstr);
                    try
                    {
                        if (_hi < _hisx.Length)
                        {
                            _hi = _hi + 1;

                            double x = 0;
                            double y = 0;

                            x = _hisx[_hi];
                            y = _hisy[_hi];
                            
                            string dt = _hisdate[_hi];

                            tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);

                            foreach (Feature fcar in tbl)
                            {
                                if (fcar["Name"].ToString() == _cnname)
                                {
                                    fcar.Geometry.GetGeometryEditor().OffsetByXY(x - Lx, y - Ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                                    MapInfo.Geometry.DPoint dpoint;
                                    MapInfo.Geometry.Point point;

                                    dpoint = new DPoint(x, y);
                                    point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                                    fcar.Geometry = point;
                                    this.txtdate.Text = _hisdate[_hi];
                                    this.txtspe.Text = _hisspe[_hi];
                                    this.txtver.Text = _hisver[_hi];
                               
                                    if (_jzflag == true)
                                    {
                                        this.MapControl1.Map.Center = dpoint;
                                    }

                                    Boolean inf = this.IsInBounds(x, y);
                                    if (inf == false)
                                    {
                                        MapControl1.Map.Center = dpoint;
                                    }
                                    TrackHistoryline(x, y, Lx, Ly);

                                    fcar.Geometry.EditingComplete();
                                    fcar.Update();

                                    MapControl1.Refresh();

                                    tbl.EndAccess();

                                    Lx = x;
                                    Ly = y;

                                    break;
                                }
                            }
                        }
                        else
                        {
                            this.btncar1.Enabled = true;
                            this.btncar2.Enabled = false;
                            this.btncar3.Enabled = false;
                            this.btncar4.Enabled = false;
                            timer1.Enabled = false;
                        }
                    }
                    catch { timer1.Enabled = false; }
                }
            }
            catch(Exception ex)
            {
                timer1.Enabled = false;
                ExToLog(ex, "11-timer1_trick");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                
               // this.Visible = false;
            }
            catch { }
        }

       /// <summary>
        /// 判断车辆是否在视图范围
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
       /// </summary>
       /// <param name="x">X坐标</param>
       /// <param name="y">Y坐标</param>
       /// <returns>布尔值(true-是 false-否)</returns>
        private Boolean IsInBounds(double x, double y)
        {
            try
            {
                if (MapControl1.Map.Bounds.x1 < x && x < MapControl1.Map.Bounds.x2 && MapControl1.Map.Bounds.y1 < y && y < MapControl1.Map.Bounds.y2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "12-判断警员是否在视图范围");
                return false; 
            }
        }

        /// <summary>
        /// 暂停
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btncar2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btncar1.Enabled == true)
                    this.btncar1.Enabled = false;

                if (this.btncar4.Enabled == false)
                    this.btncar4.Enabled = true;

                if (this.btncar2.Enabled == true)
                    this.btncar2.Enabled = false;

                if (this.btncar3.Enabled == false)
                    this.btncar3.Enabled = true;

                if (this.timer1.Enabled == true)
                    timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "13-暂停");
            }
        }

        /// <summary>
        /// 继续
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btncar3_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btncar1.Enabled == true)
                    this.btncar1.Enabled = false;
                if (this.btncar4.Enabled == false)
                    this.btncar4.Enabled = true;
                if (this.btncar3.Enabled == true)
                    this.btncar3.Enabled = false;
                if (this.btncar2.Enabled == false)
                    this.btncar2.Enabled = true;
                if (this.timer1.Enabled == false)
                    timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "14-继续");
            }
        }

        /// <summary>
        /// 停止
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btncar4_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btncar4.Enabled == true)
                    this.btncar4.Enabled = false;
                if (this.btncar2.Enabled == true)
                    this.btncar2.Enabled = false;
                if (this.btncar3.Enabled == true)
                    this.btncar3.Enabled = false;
                if (this.btncar1.Enabled == false)
                    this.btncar1.Enabled = true;
                if (this.timer1.Enabled == true)
                    timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "15-停止");
            }
        }
        

        /// <summary>
        /// 窗体LOAD
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void frmGuijiTime_Load(object sender, EventArgs e)
        {
            try
            {
                this.Left = Screen.PrimaryScreen.WorkingArea.Left;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "16-窗体LOAD");
            }
        }

        /// <summary>
        /// 选择车辆
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.comboBox1.Focus();
                this.comboBox1.Select(this.comboBox1.Text.Length + 1, 0);

                GetCarName();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "17-选择车辆");
            }
        }

        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmGuijiTime-"+sFunc);
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sql">当前操作SQL</param>
        /// <param name="method">操作方式</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + User + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'警员监控','GPS警员定位系统:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch { }
        }
    }
}