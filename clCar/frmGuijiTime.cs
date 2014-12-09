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
    public partial class frmGuijiTime : Form
    {
        private MapControl mapControl1 = null;
        static string[] ConStr;
        static string mysqlstr;


        public double lx = 0; //上次的经度
        public double ly = 0; // 上次的纬度
        
        private string cnname = string.Empty ;

        public string userid = "";
        public string datasource = "";
        public string password = "";

        public string user = "";

        private Boolean jzflag = false ;


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="s">数据库连接字符组</param>
        public frmGuijiTime(MapControl mc,string[] s)
        {
            try
            {
                InitializeComponent();

                mapControl1 = mc;
                ConStr = s;
                mysqlstr = "data source=" + ConStr[0] + ";user id=" + ConStr[1] + ";password=" + ConStr[2];
                AddCarName();
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-01-初始化");
            }
        }

        public void getflag(Boolean jz)
        {
            this.jzflag = jz;
        }

        /// <summary>
        /// 添加车辆名称
        /// </summary>
        public void AddCarName()
        {
            OracleConnection con = new OracleConnection(mysqlstr);
            try
            {
                con.Open();
                OracleCommand cmdadd = new OracleCommand("select 终端车辆号牌 from GPS警车定位系统 order by 终端车辆号牌", con);
                OracleDataReader dr = cmdadd.ExecuteReader();             
                while (dr.Read())
                {
                    this.comboBox1.Items.Add(Convert.ToString(dr.GetString(0)));
                }
                dr.Close();
            }
            catch(Exception ex) 
            {
                writeToLog(ex, "frmGuijiTime-01-添加车辆名称");
            }
            finally {
                con.Close();
            }
        }

        /// <summary>
        /// 开始播放轨迹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.txtplyspe.Text == "")
                {
                    MessageBox.Show("播放速度不能为空");
                    return;
                }
                if (Convert.ToInt32(this.txtplyspe.Text) <= 0 )
                {
                    MessageBox.Show("播放速度不能小于零");
                    return;
                }

                Int32 psp = Convert.ToInt32(this.txtplyspe.Text);

                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                //string cn = this.comboBox1.Text;

                string cn = GetCarName();

                if (cn != "")
                {
                    this.comboBox1.Text = cn;
                }                

                if (dateTimePicker1.Text == "" || dateTimePicker2.Text == "")
                {
                    MessageBox.Show("起始时间不能为空！", "系统提示");
                    return ;
                }
                    
                if (dateTimePicker3.Text == "" || dateTimePicker4.Text == "")
                {
                    MessageBox.Show("结束时间不能为空！", "系统提示");
                    return ;
                }
         
                if (cn == "")
                {
                    MessageBox.Show("车辆名称为必选项，不能为空！", "系统提示");
                    return ;
                }
         
                
                if (dte < dts)
                {
                    MessageBox.Show("起始时间不能大于或等于终止时间！", "系统提示");
                    return;
                }

                cnname = cn;
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                bool df =  CreateHistoryDate();
                if (df == false)
                {
                    return;
                }
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                bool lf = CreateHistoryLayer();
                if (lf == false)
                {
                    return;
                }
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                bool ff = CreateHisFtr();
                if (ff == false)
                {
                    return;
                }
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                //this.Visible = false;
                if(this.btncar1.Enabled == true)
                this.btncar1.Enabled = false;
                if(this.btncar2.Enabled == false )
                this.btncar2.Enabled = true;
                if(this.btncar3.Enabled == true)
                this.btncar3.Enabled = false ;
                if(this.btncar4.Enabled == false )
                this.btncar4.Enabled = true;
                timer1.Interval = psp*1000;
                timer1.Enabled = true;

                WriteEditLog(cn, "轨迹回放");
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-03-开始播放轨迹");
            }
        }

        /// <summary>
        /// 获得车辆名称
        /// </summary>
        /// <returns></returns>
        private string GetCarName()
        {
            string carname = "";
            try
            {
                string sqlstring = "select * from GPS警车定位系统 where 终端车辆号牌 like '%" + this.comboBox1.Text.Trim() + "%' order by 终端车辆号牌";
                DataTable dt =GetTable (sqlstring);
                if (dt.Rows.Count == 1)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        carname = Convert.ToString(dr["终端车辆号牌"]);
                    }
                }
                else
                {
                    MessageBox.Show("没有找到与关键字匹配的车辆信息","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }             
                return carname;
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-04-获取车辆名称");
                return carname;
            }
        }

        double[] hisx;  //历史轨迹的x值
        double[] hisy;  //历史轨迹的Y值
        string[] hisdate;//历史轨迹的时间值
        string[] hisspe; //历史轨迹的速度值
        string[] hisver; //历史轨迹的方向值

        /// <summary>
        /// 创建历史数据
        /// </summary>
        /// <returns></returns>
        public Boolean  CreateHistoryDate()
        {
            //删除临时表的数据
            try
            {
                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                if (dte > dts)
                {

                    string sqlstring = "select * from GPSCAR where 时间> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 时间< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 车辆牌号 = '" + cnname + "' order by 时间";
                    DataTable dtsel = GetTable(sqlstring);

                    if (dtsel.Rows.Count > 0)
                    {
                        hisx = new double[dtsel.Rows.Count];
                        hisy = new double[dtsel.Rows.Count];
                        hisdate = new string[dtsel.Rows.Count];
                        hisspe = new string[dtsel.Rows.Count];
                        hisver = new string[dtsel.Rows.Count];

                        int i = 0;
                        foreach (DataRow dr in dtsel.Rows)
                        {
                            //获取经纬度                                                    
                            hisx[i] = Convert.ToDouble(dr["经度"]);
                            hisy[i] = Convert.ToDouble(dr["纬度"]);
                            hisdate[i] = Convert.ToString(dr["时间"]);
                            hisspe[i] = Convert.ToString(dr["速度"]);
                            hisver[i] = Convert.ToString(dr["方向"]);
                            i = i + 1;
                        }

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("当前时间段内无该车辆的历史轨迹数据,请重新选择", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;

                    }
                }
                else
                {
                    MessageBox.Show("选择的时间段结束时间不大于开始时间,将无法演示,请重新选择", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-05-创建历史数据");
                return false;
            }
        }


        /// <summary>
        /// 创建车辆图层
        /// </summary>
        /// <returns></returns>
 
        public Boolean  CreateHistoryLayer()
        {
            try
            {

                if (mapControl1.Map.Layers["CarGuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarGuijiLayer");
                }

                if (mapControl1.Map.Layers["CarGuijiLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarGuijiLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarGuijiLayer");
                Table tblTemp = Cat.GetTable("CarGuijiLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarGuijiLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);

          

                //添加标注
                string activeMapLabel = "CarGuijiLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarGuijiLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                //lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                return true;
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-06-创建车辆图层");
                return false;
            }
        }


        private int hi;
        private string Road = string.Empty;
        /// <summary>
        /// 创建历史轨迹的点
        /// </summary>
        /// <returns></returns>

        public Boolean  CreateHisFtr()
        {
            try
            {
                if (hisx.Length > 0 && hisy.Length > 0 && (hisx.Length == hisy.Length))
                {
                    double xx = 0;
                    double yy = 0;

                    hi = 0;

                    xx = hisx[hi];
                    yy = hisy[hi];

                    //Road = GetNearestRoad(xx, yy);

                    lx = xx;
                    ly = yy;

                    MapInfo.Mapping.Map map = mapControl1.Map;;

                    MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["CarGuijiLayer"] as FeatureLayer;

                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarGuijiLayer");

                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xx, yy)) as FeatureGeometry;
                    //CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(43,System.Drawing.Color.Red, 20));

                    //CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("POLI1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Blue , 16));
                    CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("POLI1-32.bmp", BitmapStyles.None, System.Drawing.Color.Blue, 20));
                    
                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                    ftr.Geometry = pt;
                    ftr.Style = cs;
                    ftr["Name"] = cnname;                    
                    tblcar.InsertFeature(ftr);



                    Feature ftr1 = new Feature(tblcar.TableInfo.Columns);
                    ftr1.Geometry = pt;
                    ftr1.Style = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Blue, 10));
                    string cardate = hisdate[hi];
                    int hii = hisdate[hi].IndexOf('-');
                    int hjj = hisdate[hi].LastIndexOf(':');
                    ftr1["Name"] = cardate.Substring(hii+1, hjj - hii - 1);     
                    tblcar.InsertFeature(ftr1);
                    //mapControl1.Map.Center = ftr.Geometry.Centroid;
                    //mapControl1.Map.Zoom = new Distance(2500, DistanceUnit.Meter);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex) 
            {
                writeToLog(ex, "frmGuijiTime-07-创建历史轨迹的点");
                return false;
            }            
        }        

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-09-窗体关闭");
            }
        }


        /// <summary>
        /// 创建历史轨迹线 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
     
        public void TrackHistoryline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["CarTrack"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("CarTrack");

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
                writeToLog(ex, "frmGuijiTime-10-创建历史轨迹线");
            }
        }

        /// <summary>
        /// timer1_trick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false) return;

                MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                FeatureLayer lyr = mapControl1.Map.Layers["CarGuijiLayer"] as FeatureLayer;

                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("CarGuijiLayer");

                MapInfo.Mapping.Map map = mapControl1.Map; 
                Feature fcar = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tbl, MapInfo.Data.SearchInfoFactory.SearchWhere("Name='" + this.cnname + "'"));

                if (fcar != null)
                {
                    try
                    {
                        if (hi < hisx.Length)
                        {
                            hi = hi + 1;
                        
                            double x = 0;
                            double y = 0;

                            x = hisx[hi];
                            y = hisy[hi];

                            string dt = hisdate[hi];

                            tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);


                            fcar.Geometry.GetGeometryEditor().OffsetByXY(x - lx, y - ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                            MapInfo.Geometry.DPoint dpoint;
                            MapInfo.Geometry.Point point;

                            dpoint = new DPoint(x, y);
                            point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                            fcar.Geometry = point;
                            this.txtdate.Text = hisdate[hi];
                            this.txtspe.Text = hisspe[hi];
                            this.txtver.Text = hisver[hi];

                            if (jzflag == true)
                            {
                                mapControl1.Map.Center = dpoint;
                            }

                            Boolean inf = this.IsInBounds(x, y);
                            if (inf == false)
                            {
                                mapControl1.Map.Center = dpoint;
                            }
                            TrackHistoryline(x, y, lx, ly);

                            fcar.Geometry.EditingComplete();
                            fcar.Update();



                            Feature ftr1 = new Feature(tbl.TableInfo.Columns);
                            ftr1.Geometry = point;
                            ftr1.Style = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Blue, 10));

                            string cardate = hisdate[hi];

                            int hii = hisdate[hi].IndexOf('-');
                            int hjj = hisdate[hi].LastIndexOf(':');

                            ftr1["Name"] = cardate.Substring(hii+1, hjj-hii-1);
                            tbl.InsertFeature(ftr1);

                            mapControl1.Refresh();

                            this.label8.Text = "当前数据量: " + hi.ToString() + "/" + hisx.Length.ToString();


                            tbl.EndAccess();

                            Console.WriteLine(hi.ToString() + "   " + this.mapControl1.Map.Layers.Count.ToString());

                            lx = x;
                            ly = y;
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
                    catch { }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-11-timer1_trick");
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
       /// </summary>
       /// <param name="x"></param>
       /// <param name="y"></param>
       /// <returns></returns>
        private Boolean IsInBounds(double x, double y)
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
            catch(Exception ex) 
            {
                writeToLog(ex, "frmGuijiTime-12-判断车辆是否在视图范围");
                return false; 
            }
        }


      
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-13-暂停");
            }
        }

        /// <summary>
        /// 继续
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-14-继续");
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-15-停止");
            }
        }
        

        /// <summary>
        /// 窗体LOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmGuijiTime_Load(object sender, EventArgs e)
        {
            try
            {
                this.Left = Screen.PrimaryScreen.WorkingArea.Left;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-16-窗体LOAD");
            }
        }

        /// <summary>
        /// 选择车辆
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-17-选择车辆");
            }
        }
       
        /// <summary>
        /// 查询SQL
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
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 获取Scalar
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        //记录操作记录
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'车辆监控','GPS警车定位系统:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch { }
        }
    }
}