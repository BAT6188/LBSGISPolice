//********顺德公安项目-车辆监控模块******
//********创建人：jie.zhang
//********创建日期： 2008.9.10
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

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace clCar
{
    public partial class frmCar : Form
    {
        public MapControl mapControl1 = null;

        //jie.zhang 2008.9.24
        public Boolean GetCarflag = false; //开始车辆监控的标志
        public string NowCarName; //当前在Grid中选中的车辆
        public string GzCarName;  //被跟踪的车辆的名称
        public string JzCarName;  //选中的车辆局中

        private OracleConnection con;
        public string CanNm;

        public frmGuijiTime fhistory = new frmGuijiTime();

        public Boolean GVFlag = false;//开始选择地点监控的标识
        public IResultSetFeatureCollection rsfcView;//范围车辆的图元集合
        public string[] carn; //各个车辆的号牌
        public double[] lastx; //各个车辆上次的精度
        public double[] lasty; //各个车辆上次的纬度
        public Boolean SetViewFlag = false;//设置范围标识符
        public IResultSetFeatureCollection rsfcflash;//闪烁的图元集合
        public int iflash = 0;
        //获取当前地图的定点坐标值
        //public double ldx = 0; //左下x
        //public double ldy = 0; //左下y
        //public double rux = 0; //右上x
        //public double ruy = 0; //右上y
        //end

        //XML文件读取的数据库的用户名、数据库名、用户密码
        public string userid = "";
        public string datasource = "";
        public string password = "";

        public frmCar()
        {
            try
            {
                InitializeComponent();

                ReadXML();
                string mysqlstr = "user id =" + userid + " ;data source = " + datasource + ";password =" + password;

                con = new OracleConnection(mysqlstr);
            }
            catch { }
        }

        private void ReadXML()
        {
            try
            {
                string s = "";
                string path = "";
                path = Application.StartupPath;
                XmlDocument doc = new XmlDocument();

                doc.Load("Config.XML");

                XmlNodeReader reader = new XmlNodeReader(doc);

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            s = reader.Name;
                            break;
                        case XmlNodeType.Text:
                            if (s.Equals("user"))
                            {
                                userid = reader.Value;
                            }
                            if (s.Equals("ds"))
                            {
                                datasource = reader.Value;
                            }
                            if (s.Equals("password"))
                            {
                                password = reader.Value;
                            }
                            break;
                    }
                }
            }
            catch { }
        }

        //得到地图主控件和工具栏
        public void getParameter(MapControl m, ToolStrip t)
        {
            mapControl1 = m;
            toolStrip1 = t;
            mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);
            fhistory.mapControl1 = m;
        }

        //初始化datagrid
        private void InitCarGrid()
        {
            try
            {
                string[] HeadArr = new string[] { "车牌号码", "终端类型", "所属单位", "当前任务", "经度", "纬度", "速度", "方向", "导航状态", "时间" };

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
                con.Open();
                OracleCommand cmd = new OracleCommand("select * from GPS警车定位系统", con);
                OracleDataReader dataReader = cmd.ExecuteReader();

                dataGridView1.Rows.Clear();

                for (int i = 1; i < 11; i++)
                {
                    string fieldName = dataReader.GetName(i);
                    dataGridView1.Columns.Add(fieldName, HeadArr[i - 1]);
                    //dataGridView1.Rows.Add(1);

                    //dataGridView1.Rows[i].SetValues(fieldName, "", "必填");                   
                }
            }
            catch { }
            finally
            {
                con.Close();
            }
        }

        ////轨迹演示
        //private void toolGuiji_Click(object sender, EventArgs e)
        //{
        //    frmGuijiTime fGuijiTime = new frmGuijiTime();
        //    fGuijiTime.ShowDialog();
        //}

        //选择工具的事件
        private void toolSelect_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                UncheckedTool(e, 0, 3);

                //GetCarflag = true;

                switch (e.ClickedItem.Name)
                {
                    case "starttimer":
                        StartTimeCar();
                        break;
                    case "stoptimer":
                        StopTimeCar();
                        break;
                    case "toolSelByPoint":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        break;
                    case "toolSelByCircle":
                        mapControl1.Tools.LeftButtonTool = "SelectRadius";
                        break;
                    case "toolSelByRect":
                        mapControl1.Tools.LeftButtonTool = "SelectRect";
                        break;
                    case "toolSelByPolygon":
                        mapControl1.Tools.LeftButtonTool = "SelectPolygon";
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        //点击工具栏上的工具时，对工具按钮进行设置，
        //选中的背景颜色设为白色，其他透明，以便明确当前的选择项
        //由于按钮分组，iFrom表示组的首Index，iEnd表示末Index
        private void UncheckedTool(ToolStripItemClickedEventArgs e, int iFrom, int iEnd)
        {
            try
            {
                for (int i = iFrom; i <= iEnd; i++)
                {
                    if (e.ClickedItem.Owner.Items[i].BackColor == Color.White)
                    {
                        e.ClickedItem.Owner.Items[i].BackColor = Color.Transparent;
                    }
                }
                e.ClickedItem.BackColor = Color.White;
            }
            catch { }
        }


        //将选择的车辆作为数据源绑定在grid上
        public void AddGrid()
        {
            try
            {
                if (CanNm != "")
                {
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }
                    con.Open();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = con;
                    if (CanNm == "All")
                    {                     //终端ID,车辆牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间
                        cmd.CommandText = "select 终端车辆号牌,所属单位,当前任务,X,Y,当前速度,当前方向,导航状态,GPS时间 from GPS警车定位系统";
                    }
                    else
                    {
                        cmd.CommandText = "select 终端车辆号牌,所属单位,当前任务,X,Y,当前速度,当前方向,导航状态,GPS时间 from GPS警车定位系统 where (" + CanNm + ")";
                    }

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    this.label2.Text = "共有" + dt.Rows.Count.ToString() + "条记录";

                    dataGridView1.DataSource = dt;
                    dataGridView1.Refresh();
                    con.Close();
                }
            }
            catch { }
        }

        //选择工具时的各种状态
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //try
            //{
            //    if (NowCarName != "")
            //    {


            //        switch (e.ClickedItem.Name)
            //        {
            //            case "toolLocation"://范围
            //                mapControl1.Tools.LeftButtonTool = "SelectRadius";
            //                GetCarflag = true;
            //                break;
            //            case "toolGenzong": //跟踪1

            //                GzCarName = NowCarName;

            //                break;
            //            case "toolJuzhong": //居中2

            //                break;

            //            case "toolGuiji": //轨迹

            //                this.Visible = true;
            //                break;
            //            default:
            //                break;
            //        }                    
            //    }
            //    else
            //    { MessageBox.Show("没有选择任何车辆！", "系统提示"); };
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message.ToString(),"系统提示");
            //}
        }

        //private void AddGrid()
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        //设置背景颜色
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
                }
            }
            catch { }
        }

        //当前选择的车辆牌号
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                NowCarName = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();
            }
            catch
            { }
        }

        public void CopyCarData()
        {
            //try
            //{
            //    con.Open();

            //    OracleCommand cmddel = new OracleCommand();
            //    cmddel.Connection = con;
            //    cmddel.CommandText = "delete from sysman.GPSCAR";
            //    cmddel.ExecuteNonQuery();
            //    //判断GPSCar中是否含有数据，如果有删除数据
            //    //con.Close();


            //        //搜索GPS警车定位系统中不同的数据复制到表中
            //        //con.Open();
            //        OracleCommand cmdcopy = new OracleCommand();
            //        cmdcopy.Connection = con;

            //        //cmdcopy.CommandText = "insert into sysman.GPSCAR(终端ID,车辆牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间,上次经度,上次纬度) select distinct 终端ID号码,终端车辆号牌,所属单位,当前任务,X,Y,当前速度,当前方向,导航状态,GPS时间,X,Y from sysman.GPS警车定位系统";
            //        cmdcopy.CommandText = "insert into sysman.GPSCAR(终端ID,车辆牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间,上次经度,上次纬度) select  终端ID号码,终端车辆号牌,所属单位,当前任务,X,Y,当前速度,当前方向,导航状态,GPS时间,X,Y from sysman.GPS警车定位系统 where 终端ID号码 in(select distinct 终端ID号码 from sysman.GPS警车定位系统)";
            //        cmdcopy.ExecuteNonQuery();

            //    con.Close();
            //}
            //catch (Exception ex)
            //{ 
            ////    MessageBox.Show(ex.Message.ToString());               
            //}
            //finally { con.Close(); };
        }


        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                GetFlash();
            }
            catch
            { }
        }

        //创建车辆显示的临时图层
        public void CreateCarLayer()
        {
            try
            {
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarLayer");
                Table tblTemp = Cat.GetTable("CarLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);

                //添加标注
                string activeMapLabel = "CarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");
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
            }
            catch { }
        }

        //创建跟踪显示的轨迹图层
        public void CreateTrackLayer()
        {
            try
            {
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;


                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("Track");
                Table tblTemp = Cat.GetTable("Track");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("Track");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);
            }
            catch { }
        }

        public void AddCarFtr()
        {
            string tempstrCon = "user id =" + userid + " ;data source = " + datasource + ";password =" + password;
            OracleConnection conf = new OracleConnection(tempstrCon);

            try
            {
                MapInfo.Mapping.Map map = MapInfo.Engine.Session.Current.MapFactory[1];
                MapInfo.Mapping.FeatureLayer lyrcar =mapControl1.Map.Layers["CarLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");


                conf.Open();
                //、、dr = new  OracleDataReader();
                //OracleDataReader dr = GetDataReader("Select * from GPS警车定位系统", con);

                OracleCommand cmd = new OracleCommand("Select * from GPS警车定位系统", conf);
                cmd.ExecuteNonQuery();
                OracleDataAdapter apt = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);


                if (dt.Rows.Count > 0)
                {
                    this.lastx = new double[dt.Rows.Count];
                    this.lasty = new double[dt.Rows.Count];
                    this.carn = new string[dt.Rows.Count];
                    int i = 0;

                    foreach (DataRow dr in dt.Rows)
                    {
                        lastx[i] = Convert.ToDouble(dr["X"]);
                        lasty[i] = Convert.ToDouble(dr["Y"]);
                        carn[i] = Convert.ToString(dr["终端车辆号牌"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("RED-CAR.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 20));
                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = dr["终端车辆号牌"].ToString();
                        tblcar.InsertFeature(ftr);

                        i = i + 1;
                    }
                }
            }
            catch { }
            finally
            {
                conf.Close();
            };
        }

        //判断坐标值是否在定点范围内
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
            catch { return false; }
        }


        private void timLocation_Tick(object sender, EventArgs e)
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
                    timLocation.Enabled = false;
                }
            }
            catch { }

        }

        public void GetFlash()
        {
            try
            {
                string tblname = "CarLayer";
                string colname = "Name";
                string ftrname = NowCarName;

                if (ftrname != "")
                {
                    //Find find = null;
                    MapInfo.Mapping.Map map = null;

                    map = MapInfo.Engine.Session.Current.MapFactory[1];

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }

                    //MapInfo.Mapping.FeatureLayer findlayer = (MapInfo.Mapping.FeatureLayer)map.Layers[tblname];
                    //find = new Find(findlayer.Table, findlayer.Table.TableInfo.Columns[colname]);
                    //FindResult result = find.Search(ftrname);
                    //if(result.ExactMatch)
                    //{
                    MapInfo.Data.MIConnection conn = new MIConnection();
                    conn.Open();

                    MapInfo.Data.MICommand cmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + " like '%' +@name +'%'";
                    cmd.Parameters.Add("@name", ftrname);

                    rsfcflash = cmd.ExecuteFeatureCollection();

                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash);

                    if (rsfcflash.Count > 0)
                    {
                        foreach (Feature f in rsfcflash)
                        {
                            mapControl1.Map.Center = f.Geometry.Centroid;
                        }
                    }

                    cmd.Clone();
                    conn.Close();

                    StartFlash();
                }
                else
                {
                    MessageBox.Show("没有选择任何车辆！", "系统提示");
                }
            }
            catch { }
        }

        public void StartFlash()
        {
            timLocation.Enabled = true;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //搜索车辆图层上有图元 

                //MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                //FeatureLayer lyr = this.mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("CarLayer");

                MapInfo.Mapping.Map map = null;

                map = MapInfo.Engine.Session.Current.MapFactory[1];

                //MapInfo.Data.IResultSetFeatureCollection rsfc = session.Selections.DefaultSelection[lyr.Table]; 



                //=========================================
                //DEBUG--------测试数据 jie.zhang 20081008
                //=========================================
                //========start========================

                //if (tbl != null)
                //{
                //    //读取数据库数据进行更新
                //    if (con.State == System.Data.ConnectionState.Open)
                //    {
                //        con.Close();
                //    }
                //    con.Open();

                //    OracleCommand cmd = new OracleCommand("Select * from sysman.GPSTEMP", con);

                //    cmd.ExecuteNonQuery();
                //    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                //    DataTable dt = new DataTable();
                //    apt.Fill(dt);

                //    int carid = 0;
                //    string cnum = "";
                //    double x = 0;
                //    double y = 0;

                //    if (dt.Rows.Count > 0)
                //    {
                //        foreach (DataRow dr in dt.Rows)
                //        {
                //            //获取车辆牌号 和获取经纬度
                //             carid =  Convert.ToInt32(dr["ID"]);;
                //             cnum = Convert.ToString( dr["CN"]);
                //             x = Convert.ToDouble( dr["X"]);
                //             y = Convert.ToDouble(dr["Y"]);
                //             break;
                //        }

                //        double lx = 0;
                //        double ly = 0;
                //        int j = 0;

                //        //获取车辆的上次经纬度
                //        for (j = 0; j < this.carn.Length; j++)
                //        {
                //            if (this.carn[j] == cnum)
                //            {
                //                lx = this.lastx[j];
                //                ly = this.lasty[j];
                //                break;
                //            }
                //        }
                //        con.Close();

                //        con.Open();
                //        OracleCommand cmdd1 = new OracleCommand("delete from sysman.GPSTEMP where ID = " + carid.ToString(), con);
                //        cmdd1.ExecuteNonQuery();
                //        con.Close();



                //        //根据车辆的牌号移动车辆
                //        tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);

                //        foreach (Feature fcar in tbl)
                //        {
                //            if (fcar["Name"].ToString() == cnum)
                //            {
                //                fcar.Geometry.GetGeometryEditor().OffsetByXY(x - lx, y - ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                //                MapInfo.Geometry.DPoint dpoint;
                //                MapInfo.Geometry.Point point;

                //                dpoint = new DPoint(x, y);
                //                point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                //                fcar.Geometry = point;



                //                if (cnum == this.GzCarName)
                //                {                                        //跟踪
                //                    Boolean inf =this.IsInBounds(x, y);
                //                    if (inf == false)
                //                    {
                //                        mapControl1.Map.Center = dpoint;
                //                    }
                //                    Trackline(x, y, lx, ly);
                //                }

                //                if (cnum == this.JzCarName)
                //                {                                     //跟踪                                    
                //                    mapControl1.Map.Center = dpoint;
                //                }

                //                fcar.Geometry.EditingComplete();
                //                fcar.Update();

                //                mapControl1.Refresh();

                //                tbl.EndAccess();

                //                this.lastx[j] = x;
                //                this.lasty[j] = y;

                //                break;
                //            }
                //        }
                //     }
                //    }                  

                //=========================================
                //DEBUG--------测试数据 jie.zhang 20081008
                //=========================================
                //========end========================

                if (tbl != null)
                {

                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }

                    con.Open();
                    OracleCommand cmd = new OracleCommand("Select * from sysman.GPS警车定位系统", con);

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    double lx = 0;
                    double ly = 0;
                    int j = 0;

                    string cnum = "";
                    double x = 0;
                    double y = 0;

                    if (dt.Rows.Count > 0)
                    {
                        //获取车辆牌号 和获取经纬度

                        foreach (DataRow dr in dt.Rows)
                        {
                            cnum = Convert.ToString(dr["终端车辆号牌"]);
                            x = Convert.ToDouble(dr["X"]);
                            y = Convert.ToDouble(dr["Y"]);

                            //获取车辆的上次经纬度
                            for (j = 0; j < this.carn.Length; j++)
                            {
                                if (carn[j] == cnum)
                                {
                                    lx = lastx[j];
                                    ly = lasty[j];
                                    break;
                                }
                            }

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

                                    if (cnum == this.GzCarName)
                                    {
                                        //跟踪
                                        Boolean inflag = this.IsInBounds(x, y);
                                        if (inflag == false)
                                        {
                                            mapControl1.Map.Center = dpoint;
                                        }
                                        Trackline(x, y, lx, ly);
                                        //this.mapControl1.Map.Center = fcar.Geometry.Centroid;
                                    }
                                    if (cnum == this.JzCarName)
                                    {
                                        //居中
                                        mapControl1.Map.Center = dpoint;
                                    }

                                    //if (rsfcView != null)
                                    //{    //范围

                                    //    Boolean inflag = IsInBounds(x, y);
                                    //    if (inflag == false)
                                    //    {
                                    //        mapControl1.Map.SetView(rsfcView.Envelope);
                                    //    }
                                    //}

                                    fcar.Geometry.EditingComplete();
                                    fcar.Update();

                                    mapControl1.Refresh();

                                    tbl.EndAccess();

                                    lastx[j] = x;
                                    lasty[j] = y;

                                    break;
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch { }
        }

        public void StartTimeCar()
        {
            try
            {
                StopTimeCar();

                CreateCarLayer();
                CreateTrackLayer();
                AddCarFtr();

                CanNm = "All";
                AddGrid();

                mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["CarLayer"]);

                ToolCarEnable();

                CreateDate();
                this.SetLayerSelect("CarLayer");

                timeCar.Interval = 1000;
                //timeCar.Enabled = true;
            }
            catch { }
        }

        public void CreateDate()
        {
            //删除临时表的数据
            try
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
                con.Open();
                OracleCommand cmdelAll = new OracleCommand("Delete from sysman.GPSTEMP", con);
                cmdelAll.ExecuteNonQuery();
                con.Close();

                con.Open();
                OracleCommand cmdsel = new OracleCommand("select 车辆牌号,经度,纬度 from sysman.GPSCAR", con);
                OracleDataReader drsel = cmdsel.ExecuteReader();

                int i = 0;

                while (drsel.Read())
                {
                    i = i + 1;
                    string mysqlstr = "user id =" + userid + " ;data source = " + datasource + ";password =" + password; ;
                    OracleConnection conn = new OracleConnection(mysqlstr);
                    conn.Open();
                    OracleCommand cmdins = new OracleCommand("insert into sysman.GPSTEMP(ID,CN,X,Y) values(" + i + ",'" + drsel.GetString(0) + "'," + drsel.GetFloat(1) + "," + drsel.GetFloat(2) + ")", conn);
                    cmdins.ExecuteNonQuery();
                    conn.Close();
                }
                con.Close();
            }
            catch { }
        }


        public void StopTimeCar()
        {
            try
            {
                timeCar.Enabled = false;
                GetCarflag = false;

                ToolCarDisable();

                if (mapControl1.Map.Layers["Track"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("Track");
                    //mapControl1.Map.Layers.Remove("Track");
                }

                if (mapControl1.Map.Layers["CarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                    //mapControl1.Map.Layers.Remove("CarLayer");
                }


                if (mapControl1.Map.Layers["GuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("GuijiLayer");
                    //mapControl1.Map.Layers.Remove("GuijiLayer");
                }

                if (mapControl1.Map.Layers["CarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarLabel");
                }
            }
            catch { }
        }


        //创建线      
        public void Trackline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = MapInfo.Engine.Session.Current.MapFactory[1];
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["Track"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("Track");

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(3, MapInfo.Styles.LineWidthUnit.Pixel), 2, System.Drawing.Color.OrangeRed);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch { }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboBox1.Text != "")
                {
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }

                    con.Open();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = con;
                    if (this.textBox1.Text == "")
                    {                     //终端ID,车辆牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间
                        cmd.CommandText = "select 终端车辆号牌,终端类型,所属单位,当前任务,X,Y,当前速度,当前方向,GPS时间 from sysman.GPS警车定位系统 order by 终端车辆号牌";
                    }
                    else
                    {
                        cmd.CommandText = "select 终端车辆号牌,终端类型,所属单位,当前任务,X,Y,当前速度,当前方向,GPS时间 from sysman.GPS警车定位系统 where ( " + this.comboBox1.Text + " like  '%" + this.textBox1.Text + "%') order by 终端车辆号牌";
                    }

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    this.label2.Text = "共有" + dt.Rows.Count.ToString() + "条记录";

                    this.dataGridView1.DataSource = dt;
                    this.dataGridView1.Refresh();
                    con.Close();
                }
            }
            catch { }
        }


        public void ToolCarEnable()
        {
            try
            {
                for (int i = 10; i < 14; i++)
                {
                    string tooltext = toolStrip1.Items[i].Text;

                    toolStrip1.Items[i].Visible = true;
                }
            }
            catch { }
        }


        public void ToolCarDisable()
        {
            try
            {
                for (int i = 10; i < 14; i++)
                {
                    string tooltext = toolStrip1.Items[i].Text;

                    toolStrip1.Items[i].Visible = false;
                }
            }
            catch { }
        }

        //设置当前图层可选择
        private void SetLayerSelect(string layername)
        {
            MapInfo.Mapping.Map map = null;

            map = MapInfo.Engine.Session.Current.MapFactory[1];

            for (int i = 0; i < map.Layers.Count; i++)
            {
                IMapLayer layer = map.Layers[i];
                string lyrname = layer.Alias;

                MapInfo.Mapping.LayerHelper.SetSelectable(layer, false);
            }

            MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername], true);

        }

        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                switch (e.ToolName)
                {
                    case "SelectRadius":
                        if (this.GetCarflag)
                        {
                            MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                            FeatureLayer lyr = this.mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                            this.rsfcView = session.Selections.DefaultSelection[lyr.Table];
                            if (this.rsfcView != null)
                            {
                                string SearchName = "";
                                int i = 1;
                                foreach (Feature f in this.rsfcView)
                                {
                                    foreach (MapInfo.Data.Column col in f.Columns)
                                    {
                                        if (col.ToString() == "Name")
                                        {
                                            string ftename = f["Name"].ToString();
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
                                            this.mapControl1.Map.Center = f.Geometry.Centroid;
                                        }
                                    }
                                    else
                                    {
                                        mapControl1.Map.SetView(this.rsfcView.Envelope);
                                    }
                                }
                            }

                        }
                        break;
                }
            }
            catch { }
        }
    }
}