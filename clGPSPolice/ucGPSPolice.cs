﻿//=========================================
//=======顺德公安GPS警员跟踪=============
//=======上海数字位图===============
//=======jie.zhang
//========创建：2010.1.20

using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;

using MapInfo.Data;
using MapInfo.Engine;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Styles;
using MapInfo.Text;
using MapInfo.Tools;
using MapInfo.Windows.Controls;
using System.Collections;


namespace clGPSPolice
{
    public partial class UcGpsPolice : UserControl 
    {

        public Boolean GetPeopleflag;      // 开始警员监控的标志
        public string JzPeopleName = "";   // 居中警员的名称 
        public string GzPeopleName = "";   // 跟踪警员的名称

        public double Xx;                  // 当前警员的上次经度
        public double Yy;                  // 当前警员的上次纬度 
        public FrmGuijiTime Fhistory; 

        public IResultSetFeatureCollection RsfcView;// 范围警员的图元集合 
        public string[] Peoplen;           // 各个警员的号牌
        public double[] Lastx;             // 各个警员上次的精度
        public double[] Lasty;             // 各个警员上次的纬度

        public Boolean SetViewFlag;        // 设置范围标识符

        public int Iflash; 

        private readonly ToolStripDropDownButton _toolDDbtn;  // 工具栏下拉菜单
        public MapControl MapControl1;     // 地图控件 
        private readonly string[] _conStr; // 数据库连接字符数组

        private string VEpath = string.Empty; // 监控客户端文件目录
        private int VideoPort;             // 通讯端口
        private string[] VideoString;      // 视频连接字符
        private ToolStripLabel st;         // 状态栏
        private double videodis;           // 周边查询视频半径   
    


        private string _strUseRegion = string.Empty;     // 最后的派出所权限
        private string _strZDRegion = string.Empty;      // 最后的中队权限
        public string StrRegion = string.Empty;          // 派出所权限
        public string StrRegion1 = "";                   // 中队权限
        public string User = "";                         // 登陆用户名
        public DataTable dtExcel;                        // 导出数据

        private string _canNm = "All"; 
        private int _sr = 30;

        public ToolStripProgressBar toolPro;  // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolProLbl;     // 用于显示进度文本　
        public ToolStripSeparator toolProSep; // 分隔符

        private string[] majorTask = null;     // 重大任务参数
        public System.Windows.Forms.Label[] maTask = new System.Windows.Forms.Label[4]; // 重大任务详细信息显示控件
        public System.Windows.Forms.Panel _panMajorTask;  // 显示重大任务的详细信息

        /// <summary>
        /// 构造函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        /// <param name="m">当前地图</param>
        /// <param name="s">数据库连接字符串</param>
        /// <param name="tools">状态栏状态</param>
        /// <param name="td">下拉菜单</param>
        /// <param name="port">通讯端口</param>
        /// <param name="vs">视频客户端登陆参数</param>
        /// <param name="videopath">客户端启动位置</param>
        /// <param name="dist">查询半径</param>
        public UcGpsPolice(MapControl m, string[] s, ToolStripLabel tools, ToolStripDropDownButton t, int port, string[] vs, string videopath, double dist)  
        {
            InitializeComponent();

            try
            {
                MapControl1 = m;    // 地图
                _conStr = s;        // 数据库连接字符串组
                _toolDDbtn = t;     // 操作菜单


                st = tools;         // 状态栏状态
                VideoPort = port;   // 通讯端口
                VideoString = vs;   // 视频客户端登陆参数
                VEpath = videopath; // 客户端启动位置
                videodis = dist;    // 查询半径

                //初始化自定义工具
                MapTool ptMapTool = new CustomPointMapTool(false, MapControl1.Tools.FeatureViewer,
                                                                  MapControl1.Handle.ToInt32(), MapControl1.Tools,
                                                                  MapControl1.Tools.MouseToolProperties,
                                                                  MapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                MapControl1.Tools.Add("MoveTool", ptMapTool);

                MapControl1.Tools.Used += Tools_Used;

                MapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(MapControl1_ViewChanged);
                MapControl1.MouseMove += new MouseEventHandler(MapControl1_MouseMove);
                
                Fhistory = new FrmGuijiTime(_conStr);
                Fhistory.MapControl1 = m;

                _toolDDbtn.DropDownItemClicked += ToolSelect;

                comboBox1.Items.Add("警力编号");
                comboBox1.Items.Add("派出所名");
                comboBox1.Items.Add("中队名");
                comboBox1.Items.Add("所属科室");
                comboBox1.Items.Add("设备编号");
                comboBox1.Text = comboBox1.Items[0].ToString();

                SetUserRegion();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "01-初始化变量");
            }
        }     

        /// <summary>
        /// 初始化GPS警员
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        public void InitGpsPolice()
        {
            _toolDDbtn.Visible = true;

            GetPeopleflag = true;

            ToolPeopleEnable();

            CreatePeopleLayer();
           
            CreateTrackLayer();

            CreateTemLayer("GPS警员");

            AddGz();

            _canNm = "All";

            AddGrid();

            SetLayerSelect("PeopleLayer");
            setTabInsertable("GPS警员", true);  // 设置图层可编辑

            //给关联窗体传递参数
            frmbuss.Visible = false;
            frmbuss.ConStr = _conStr;

            // 读取自动更新设置频率
            // 获取警员GPS信息 jie.zhang 20100311
            try
            {
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                _sr = Convert.ToInt32(CLC.INIClass.IniReadValue("GPS警员", "更新频率"));

                timePeople.Interval = _sr * 1000;
                timePeople.Enabled = true;
            }
            catch (Exception ex) { ExToLog(ex, "02-读取更新频率时发生错误"); }           
        }

        /// <summary>
        /// 创建临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-14
        /// </summary>
        /// <param name="tableAiles"></param>
        private void CreateTemLayer(string tableAiles)
        {
            try
            {
                //   create   a   temp   layer   as   the   rectangle   holder 
                TableInfoMemTable ti = new TableInfoMemTable(tableAiles);
                ti.Temporary = true;

                //   add   columns 
                Column column;
                column = new GeometryColumn(MapControl1.Map.GetDisplayCoordSys());
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

                MapControl1.Map.Layers.Insert(0, temLayer);
            }
            catch (Exception ex)
            { 
                ExToLog(ex, "CreateTemLayer"); 
            }
        }

        /// <summary>
        /// 鼠标移到地图时触发
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void MapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    System.Drawing.Point point = e.Location;
                    MapInfo.Geometry.DPoint dpt;
                    MapControl1.Map.DisplayTransform.FromDisplay(point, out dpt);

                    Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(MapControl1.Map, 30);
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dpt, MapControl1.Map.GetDisplayCoordSys(), d);
                    si.SearchResultProcessor = null;
                    si.QueryDefinition.Columns = null;

                    Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("MajorTaskLayer", si);
                    if (ft2 != null)
                    {
                        maTask[0].Text = ft2["任务组编号"].ToString();
                        maTask[1].Text = ft2["任务组名称"].ToString();
                        maTask[2].Text = ft2["指挥肩咪ID"].ToString();
                        maTask[3].Text = ft2["参加人数"].ToString();
                        point.X = point.X + 20;
                        this._panMajorTask.Location = point;
                        this._panMajorTask.Visible = true;
                    }
                    else
                    {
                        this._panMajorTask.Visible = false;
                    }
                }
            }
            catch (Exception ex) 
            { 
                ExToLog(ex, "MapControl1_MouseMove");
            }
        }

        /// <summary>
        /// 创建轨迹层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        public void CreateTrackLayer()
        {
            try
            {
                Catalog cat = MapInfo.Engine.Session.Current.Catalog;

                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("Track");
                Table tblTemp = cat.GetTable("Track");
                if (tblTemp != null) //Table exists close it
                {
                    cat.CloseTable("Track");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                MapControl1.Map.Layers.Add(lyr);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "03-创建轨迹层");
            }
        }

        /// <summary>
        /// 地图缩放查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void MapControl1_ViewChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible && this.MapControl1.Map.Scale <5000 )
                {
                    CreatePeopleLayer();
                }
                else if(this.Visible)
                {
                    StopPolice();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "32-地图视野发生变化时");
            }
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

        private Table MajorTaskTable;   // 重大任务临时表
        private double MajorX, MajorY;  // 重大任务X，Y坐标
        /// <summary>
        /// 为重大任务创建临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        private void majorTaskLayer(string[] _MajorTask,double MajorX,double MajorY,int cl)
        {
            try
            {
                if (MajorTaskTable == null)
                {
                    TableInfoMemTable ti = new TableInfoMemTable("MajorTaskLayer");
                    ti.Temporary = true;

                    //   add   columns   
                    Column column;
                    column = new GeometryColumn(MapControl1.Map.GetDisplayCoordSys());
                    column.Alias = "MI_Geometry";
                    column.DataType = MIDbType.FeatureGeometry;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "MI_Style";
                    column.DataType = MIDbType.Style;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "任务组编号";
                    column.DataType = MIDbType.String;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "任务组名称";
                    column.DataType = MIDbType.String;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "指挥肩咪ID";
                    column.DataType = MIDbType.String;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "参加人数";
                    column.DataType = MIDbType.Int;
                    ti.Columns.Add(column);

                    MajorTaskTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);

                    FeatureLayer currentFeatureLayer = new FeatureLayer(MajorTaskTable, "MajorTaskLayer");

                    MapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                    labeLayer(MajorTaskTable, "任务组名称");
                }

                double dx = 0, dy = 0;

                dx = MajorX;
                dy = MajorY;

                if (dx > 0 || dy > 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point(MapControl1.Map.GetDisplayCoordSys(), dx, dy);

                    //CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 9));
                    //cs = new CompositeStyle(new SimpleVectorPointStyle(34, Color.Red, 9));

                    MapInfo.Styles.BitmapPointStyle bitmappointstyle = new MapInfo.Styles.BitmapPointStyle("YIEL2-32.BMP", BitmapStyles.None, colors[cl], 18);

                    Feature pFeat = new Feature(MajorTaskTable.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = bitmappointstyle;
                    pFeat["任务组编号"] = _MajorTask[0];
                    pFeat["任务组名称"] = _MajorTask[1];
                    pFeat["指挥肩咪ID"] = _MajorTask[2];
                    pFeat["参加人数"] = Convert.ToInt32(_MajorTask[3]);

                    MajorTaskTable.InsertFeature(pFeat);
                }

                //try
                //{
                //    DPoint dP = new DPoint(dx, dy);
                //    CoordSys cSys = MapControl1.Map.GetDisplayCoordSys();

                //    MapControl1.Map.SetView(dP, cSys, 6000);
                //    MapControl1.Map.Center = dP;
                //}
                //catch (Exception ex)
                //{
                //    ExToLog(ex, "设定视图范围");
                //}
            }
            catch (Exception ex)
            {
                ExToLog(ex, "majorTaskLayer");
            }
        }

        /// <summary>
        /// 创建图层标注
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="editTable">图层名称</param>
        /// <param name="field">标注字段</param>
        private void labeLayer(Table editTable, string field)//创建标注
        {
            try
            {
                LabelLayer labelLayer = MapControl1.Map.Layers["标注图层"] as LabelLayer;

                LabelSource source = new LabelSource(editTable);

                source.DefaultLabelProperties.Caption = field;
                source.DefaultLabelProperties.Layout.Offset = 4;
                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;
                labelLayer.Sources.Insert(0, source);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "labeLayer-11-创建图层标注");
            }
        }

        /// <summary>
        /// 绑定DataGrid
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        public void AddGrid()
        {
            try
            {
                isShowPro(true);
                if (_canNm != "")
                {
                    string sql = string.Empty;
                    _strUseRegion = this.StrRegion;
                    _strZDRegion = this.StrRegion1;
                    if (_strUseRegion == string.Empty && _strZDRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show(@"没有设置区域权限", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        // 权限设置（派出所及中队权限）
                        string regionStr = "";   // 存放权限条件
                        if (_strUseRegion != "顺德区")   // edit by fisher in 09-12-08
                        {
                            if (_strUseRegion != "")
                            {
                                if (Array.IndexOf(_strUseRegion.Split(','), "大良") > -1)
                                {
                                    _strUseRegion = _strUseRegion.Replace("大良", "大良,德胜");
                                }
                                regionStr += " 派出所名 in ('" + _strUseRegion.Replace(",", "','") + "')";

                                if (_strZDRegion != "")
                                {
                                    if (regionStr.IndexOf("and") > -1)
                                    {
                                        regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                        regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                    }
                                    else
                                    {
                                        regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                    }
                                }
                            }
                            else if (_strUseRegion == "")
                            {
                                if (_strZDRegion != "")
                                {
                                    if (regionStr.IndexOf("and") > -1)
                                    {
                                        regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                        regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                    }
                                    else
                                    {
                                        regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("您没有查询权限!");
                                    return;
                                }
                            }
                        }

                        if (_strUseRegion.IndexOf("顺德区") > -1)
                        {
                            sql = _canNm == "All"
                                         ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y, 定位更新时间 from GPS警员 where 警力编号 is not null"
                                         : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm;
                        }
                        else
                        {
                            sql = _canNm == "All"
                                      ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + regionStr
                                      : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm +
                                        " and " + regionStr;
                        }

                        //switch (_strUseRegion)
                        //{
                        //    case "顺德区":
                        //        sql = _canNm == "All"
                        //                  ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y, 定位更新时间 from GPS警员"
                        //                  : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm;
                        //        break;
                        //    default:
                        //        if (Array.IndexOf(StrRegion.Split(','), "大良") > -1 && StrRegion.IndexOf("德胜") < 0)
                        //            StrRegion = StrRegion.Replace("大良", "大良,德胜");
                        //        sql = _canNm == "All"
                        //                  ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where 派出所名 in ('" +
                        //                    _strUseRegion.Replace(",", "','") + "')"
                        //                  : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm +
                        //                    " and 派出所名 in('" + _strUseRegion.Replace(",", "','") + "')";
                        //        break;
                        //}
                    }



                    string Gzstring1 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring1 = Gzstring1 + " 警力编号='" + GzArrayName[i] + "' or ";
                        }
                        else if (GzArrayName[i] == "") 
                        {
                            Gzstring1 = Gzstring1.Substring(0, Gzstring1.LastIndexOf("or") - 1);

                            break;
                        }
                        else if (i == GzArrayName.Length-1)
                        {
                            Gzstring1 = Gzstring1 + " 警力编号='" + GzArrayName[i] + "'";
                        }
                    }


                    string Gzstring2 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring2 = Gzstring2 + " 警力编号<>'" + GzArrayName[i] + "' and ";
                        }
                        else if (GzArrayName[i] == "" ) 
                        {
                            Gzstring2 = Gzstring2.Substring(0, Gzstring2.LastIndexOf("and") - 1);

                            break;
                        }
                        else if( i == this.GzArrayName.Length-1)
                        {
                            Gzstring2 = Gzstring2 + " 警力编号<>'" + GzArrayName[i] + "'";
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
                    #region 导出Excel
                    //try
                    //{
                    //    //excelSql = cmd.CommandText;
                    //    //excelSql = "select * " + excelSql.Substring(excelSql.IndexOf("from"));
                    //    //string sRegion2 = strRegion2;
                    //    //string sRegion3 = strRegion3;

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

                    //    //cmd.CommandText = excelSql;
                    //    //apt1 = new OracleDataAdapter(cmd);
                    //    //DataTable datatableExcel = new DataTable();
                    //    //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //    //if (dtExcel != null) dtExcel.Clear();
                    //    //dtExcel = datatableExcel;
                    //}
                    //catch (Exception ex)
                    //{
                    //    isShowPro(false);
                    //    ExToLog(ex, "04-导出Excel");
                    //}
                    # endregion

                    this.toolPro.Value = 2;
                    Application.DoEvents();

                    _pagedt1 = dt;
                    dtExcel = dt;   // 导出
                    InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);


                    WriteEditLog(sql, "查询");

                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                ExToLog(ex, "05-绑定DataGrid");
            }          
        }

        int _pageSize1 = 100;     // 每页显示行数 
        int _pagenMax1;           // 总记录数
        int _pageCount1;          // 页数＝总记录数/每页显示行数
        int _pageCurrent1;        // 当前页号
        int _pagenCurrent1;       // 当前记录行 
        DataTable _pagedt1 = new DataTable();

        /// <summary>
        /// 翻页功能初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        /// <param name="lblcount">显示总记录数控件</param>
        /// <param name="textNowPage">显示当前页数控件</param>
        /// <param name="lblPageCount">显示每页数据数量控件</param>
        /// <param name="bs">分页数据源</param>
        /// <param name="bn">分页控件</param>
        /// <param name="dgv">数据控件</param>
        public void InitDataSet1(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize1 = 100;      //设置页面行数
                _pagenMax1 = _pagedt1.Rows.Count;
                TextNum1.Text = _pageSize1.ToString();
                lblcount.Text = _pagenMax1.ToString() + @"条";//在导航栏上显示总记录数

                _pageCount1 = (_pagenMax1 / _pageSize1);//计算出总页数
                if ((_pagenMax1 % _pageSize1) > 0) _pageCount1++;
                if (_pagenMax1 != 0)
                {
                    _pageCurrent1 = 1;
                }
                _pagenCurrent1 = 0;       //当前记录数从0开始

                LoadData1(textNowPage, lblPageCount, bs, bn, dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExToLog(ex, "06-InitDataSet1");
            }
        }

        /// <summary>
        /// 翻页功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        /// <param name="textNowPage">显示当前页数控件</param>
        /// <param name="lblPageCount">显示每页数据数量控件</param>
        /// <param name="bds">分页数据源</param>
        /// <param name="bdn">分页控件</param>
        /// <param name="dgv">数据控件</param>
        public void LoadData1(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                isShowPro(true);
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = _pagedt1.Clone();

                if (_pageCurrent1 == _pageCount1)
                    nEndPos = _pagenMax1;
                else
                    nEndPos = _pageSize1 * _pageCurrent1;
                nStartPos = _pagenCurrent1;

                //tsl.Text = Convert.ToString(pageCurrent1) + "/" + pageCount1.ToString();
                textNowPage.Text = Convert.ToString(_pageCurrent1);
                lblPageCount.Text = @"/" + _pageCount1.ToString();
                this.toolPro.Value = 1;
                Application.DoEvents();
                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(_pagedt1.Rows[i]);
                    _pagenCurrent1++;
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
                ExToLog(ex, "07-LoadData");
            }
        }

        /// <summary>
        /// 警员工具Enable
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        public void ToolPeopleEnable()
        {
            try
            {
                _toolDDbtn.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "08-警员工具Enable");
            }
        }

        /// <summary>
        /// 警员工具Disable
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        public void ToolPeopleDisable() 
        {
            try
            {
                _toolDDbtn.Visible = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "09-警员工具Disable");
            }
        }

        /// <summary>
        /// 创建警员临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        public void CreatePeopleLayer()
        {
            #region 注释
            ////writelog("创建警员图层开始" + System.DateTime.Now.ToString());
            //try
            //{

            //    if (MapControl1.Map.Layers["PeopleLayer"] != null)
            //    {
            //        //MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");


            //        //              //清除层中所有要素
            //        //private void clearFeatures(string alies)
            //        //{
            //        try
            //        {
            //            if (this.Visible == false)   //jie.zhang 2010-0826
            //            {
            //                if (MapControl1.Map.Layers["PeopleLayer"] != null)
            //                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
            //                if (MapControl1.Map.Layers["PeopleLabel"] != null)
            //                    MapControl1.Map.Layers.Remove("PeopleLabel");

            //                return;
            //            }

            //            //清除地图上添加的对象
            //            FeatureLayer fl = (FeatureLayer)MapControl1.Map.Layers["PeopleLayer"];
            //            if (fl == null) return;
            //            Table tableTem = fl.Table;

            //            //先清除已有对象
            //            (tableTem as IFeatureCollection).Clear();
            //            tableTem.Pack(PackType.All);
            //        }
            //        catch (Exception ex)
            //        {
            //            ExToLog(ex, "清除层中所有要素");
            //        }
            //        //}
            //    }
            //    else
            //    {
            //        Catalog cat = MapInfo.Engine.Session.Current.Catalog;

            //        //创建临时层
            //        TableInfoMemTable tblInfoTemp = new TableInfoMemTable("PeopleLayer");
            //        Table tblTemp = cat.GetTable("PeopleLayer");
            //        if (tblTemp != null) //Table exists close it
            //        {
            //            cat.CloseTable("PeopleLayer");
            //        }

            //        tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
            //        tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
            //        tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
            //        tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表_ID", 50));
            //        tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表名", 50));

            //        tblTemp = cat.CreateTable(tblInfoTemp);
            //        FeatureLayer lyr = new FeatureLayer(tblTemp);

            //        if (this.Visible == false)   //jie.zhang 2010-0826
            //        {
            //            MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
            //            return;
            //        }

            //        MapControl1.Map.Layers.Insert(0, lyr);
            //    }

            //    if (MapControl1.Map.Layers["PeopleLabel"] != null)
            //    {
            //        MapControl1.Map.Layers.Remove("PeopleLabel");
            //    }



            //    if (this.Visible == false)   //jie.zhang 2010-0826
            //    {
            //        if (MapControl1.Map.Layers["PeopleLayer"] != null)
            //            MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
            //        if (MapControl1.Map.Layers["PeopleLabel"] != null)
            //            MapControl1.Map.Layers.Remove("PeopleLabel");

            //        return;
            //    }

            //    //添加标注
            //    const string activeMapLabel = "PeopleLabel";
            //    Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("PeopleLayer");
            //    LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);

            //    LabelSource lbsource = new LabelSource(activeMapTable);
            //    lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
            //    lbsource.DefaultLabelProperties.Style.Font.Size = 10;
            //    lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

            //    lbsource.DefaultLabelProperties.Layout.Offset = 2;
            //    lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
            //    //lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
            //    lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
            //    //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
            //    //lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
            //    lbsource.DefaultLabelProperties.Caption = "Name";
            //    lblayer.Sources.Append(lbsource);

            //    if (this.Visible == false)   //jie.zhang 2010-0826
            //    {
            //        if (MapControl1.Map.Layers["PeopleLayer"] != null)
            //            MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
            //        if (MapControl1.Map.Layers["PeopleLabel"] != null)
            //            MapControl1.Map.Layers.Remove("PeopleLabel");

            //        return;
            //    }

            //    MapControl1.Map.Layers.Add(lblayer);

            //    //if (this.ZhiHui == true)
            //    //{
            //    //    SetTableDisable("PeopleLayer");
            //    //}

            //    if (this.Visible == false)   //jie.zhang 2010-0826
            //    {
            //        if (MapControl1.Map.Layers["PeopleLayer"] != null)
            //            MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
            //        if (MapControl1.Map.Layers["PeopleLabel"] != null)
            //            MapControl1.Map.Layers.Remove("PeopleLabel");

            //        return;
            //    }


            //    AddPeopleFtr();
            //}
            //catch (Exception ex)
            //{
            //    ExToLog(ex, "10-创建警员临时图层");
            //}
            #endregion

            #region 绑定方式读取

            //writelog("创建警员图层开始" + System.DateTime.Now.ToString());


            try
            {
                if (this.MapControl1.Map.Scale > 5000) return;

                if (MapControl1.Map.Layers["PeopleLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
                }

                if (MapControl1.Map.Layers["PeopleLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("PeopleLabel");
                }

                string tableAlies = "PeopleLayer";

                _strUseRegion = this.StrRegion;
                _strZDRegion = this.StrRegion1;

                // 权限设置（派出所及中队权限）
                string regionStr = "";   // 存放权限条件
                if (_strUseRegion != "顺德区")   // edit by fisher in 09-12-08
                {
                    if (_strUseRegion != "")
                    {
                        if (Array.IndexOf(_strUseRegion.Split(','), "大良") > -1)
                        {
                            _strUseRegion = _strUseRegion.Replace("大良", "大良,德胜");
                        }
                        regionStr += " 派出所名 in ('" + _strUseRegion.Replace(",", "','") + "')";

                        if (_strZDRegion != "")
                        {
                            if (regionStr.IndexOf("and") > -1)
                            {
                                regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                            }
                            else
                            {
                                regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                            }
                        }
                    }
                    else if (_strUseRegion == "")
                    {
                        if (_strZDRegion != "")
                        {
                            if (regionStr.IndexOf("and") > -1)
                            {
                                regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                            }
                            else
                            {
                                regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                            }
                        }
                        else
                        {
                            MessageBox.Show("您没有查询权限!");
                            return;
                        }
                    }
                }

                // 通过SQL读取所需要的信息
                string strSQL = "Select 警力编号 as Name,警力编号 as 表_ID,'GPS警员' as 表名,X,Y from GPS警员 where X>" + MapControl1.Map.Bounds.x1 + " and X<" + MapControl1.Map.Bounds.x2 + " and Y>" + this.MapControl1.Map.Bounds.y1 + " and Y < " + this.MapControl1.Map.Bounds.y2;

                if (_strUseRegion == string.Empty && _strZDRegion == string.Empty)
                {
                    MessageBox.Show(@"没有设置区域权限", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (_strUseRegion == "顺德区")
                {
                    strSQL = "Select 警力编号 as Name,警力编号 as 表_ID,'GPS警员' as 表名,X,Y from GPS警员 where X>" + MapControl1.Map.Bounds.x1 + " and X<" + MapControl1.Map.Bounds.x2 + " and Y>" + this.MapControl1.Map.Bounds.y1 + " and Y < " + this.MapControl1.Map.Bounds.y2;
                }
                else
                {
                    //if (Array.IndexOf(_strUseRegion.Split(','), "大良") > -1)
                    //{
                    //    _strUseRegion = _strUseRegion.Replace("大良", "大良,德胜");
                    //}
                    strSQL = "Select 警力编号 as Name,警力编号 as 表_ID,'GPS警员' as 表名,X,Y from GPS警员 where " + regionStr + " and X>" + MapControl1.Map.Bounds.x1 + " and X<" + MapControl1.Map.Bounds.x2 + " and Y>" + this.MapControl1.Map.Bounds.y1 + " and Y < " + this.MapControl1.Map.Bounds.y2;
                }

                DataTable dt = this.GetTable(strSQL);
                if (dt == null || dt.Rows.Count < 1)
                {
                    return;
                }

                ProtectMap();

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

                this.MapControl1.Map.Layers.Insert(1,temlayer);



                //改变图层的图元样式
                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("Police.bmp", BitmapStyles.None, System.Drawing.Color.Red, 20));
                FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, cs);
                temlayer.Modifiers.Clear();

                ProtectMap();

                temlayer.Modifiers.Append(fsm);

                //// 设置图层可见级别
                //temlayer.VisibleRangeEnabled = true;
                //VisibleRange zoom = new VisibleRange(0, 5000, MapInfo.Geometry.DistanceUnit.Kilometer);
                //temlayer.VisibleRange = zoom;
                //temlayer.Enabled = true;
                ////

                //添加标注
                const string activeMapLabel = "PeopleLabel";
                Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("PeopleLayer");
                LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);

                LabelSource lbsource = new LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 10;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.OrangeRed;
                //lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                //lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);

                ProtectMap();

                MapControl1.Map.Layers.Add(lblayer);

                // 读取自动更新设置频率
                // 获取警员GPS信息 jie.zhang 20100311
                try
                {
                    CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                    _sr = Convert.ToInt32(CLC.INIClass.IniReadValue("GPS警员", "更新频率"));

                    timePeople.Interval = _sr * 1000;
                    timePeople.Enabled = true;

                    //MessageBox.Show("定时更新开始");
                    //ExToLog("GPS警员定时更新开始");
                }
                catch (Exception ex) { ExToLog(ex, "02-读取更新频率时发生错误"); }           
            }
            catch (Exception ex)
            {
                ExToLog(ex, "10-创建警员临时图层");
            }
            #endregion
        }

        /// <summary>
        /// 设置图层是否可编辑
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="flag">布尔值(true-可编辑 false-不可编辑)</param>
        private void setTabInsertable(string tableName, bool flag)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetInsertable(MapControl1.Map.Layers[tableName], flag);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setTabInsertable");
            }
        }

        /// <summary>
        /// 设置跟踪对象
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void SetGZPolice()
        {
            try
            {

                if (MapControl1.Map.Layers["PeopleLayer"] == null) return;

                ProtectMap();

                string sql = "Select X,Y from GPS警员 where 警力编号 ='" + GzPeopleName + "'";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    double xv = Convert.ToDouble(dt.Rows[0]["X"]);
                    double yv = Convert.ToDouble(dt.Rows[0]["Y"]);
                    //Trackline(Xx, Yy, xv, yv);

                    Xx = xv;
                    Yy = yv;

                    // 应用跟踪样式
                    MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                    FeatureLayer lyr = this.MapControl1.Map.Layers["PeopleLayer"] as FeatureLayer;

                    lyr.Enabled = true;

                    Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                    Table tbl = cat.GetTable("PeopleLayer");

                    Feature fp = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tbl, MapInfo.Data.SearchInfoFactory.SearchWhere("Name='" + GzPeopleName + "'"));
                    CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("jCar.BMP", BitmapStyles.None, System.Drawing.Color.Red, 20));
                    fp.Style = cs;
                    fp.Update();
                    this.MapControl1.Refresh();

                    DPoint dpoint = new DPoint(xv, yv);
                    Boolean inflag = IsInBounds(xv, yv);
                    if (inflag == false)
                    {
                        MapControl1.Map.Center = dpoint;
                    }
                }

            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetGZPolice");
            }
        }

        /// <summary>
        /// 创建跟踪图层并画点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void SetJZPolice()
        {
            try
            {
                ProtectMap();

                string sql = "Select X,Y from GPS警员 where 警力编号 ='" + this.JzPeopleName + "'";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    double x = Convert.ToDouble(dt.Rows[0]["X"]);
                    double y = Convert.ToDouble(dt.Rows[0]["Y"]);


                    Catalog cat = MapInfo.Engine.Session.Current.Catalog;

                    //创建临时层
                    if (MapControl1.Map.Layers["PolGzLayer"] == null)
                    {
                        TableInfoMemTable tblInfoTemp = new TableInfoMemTable("PolGzLayer");
                        Table tblTemp = cat.GetTable("PolGzLayer");
                        if (tblTemp != null) //Table exists close it
                        {
                            cat.CloseTable("PolGzLayer");
                        }

                        tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                        tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                        tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                        tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表_ID", 50));
                        tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表名", 50));

                        tblTemp = cat.CreateTable(tblInfoTemp);
                        FeatureLayer lyr = new FeatureLayer(tblTemp);

                        if (this.Visible == false)   //jie.zhang 2010-0826
                        {
                            MapInfo.Engine.Session.Current.Catalog.CloseTable("PolGzLayer");
                            return;
                        }

                        MapControl1.Map.Layers.Insert(0, lyr);
                    }

                    MapInfo.Mapping.Map map = MapControl1.Map;
                    MapInfo.Mapping.FeatureLayer lyrcar = MapControl1.Map.Layers["PolGzLayer"] as FeatureLayer;
                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("PolGzLayer");

                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                    CompositeStyle cs = new CompositeStyle();
                    cs.ApplyStyle(new SimpleVectorPointStyle(35, Color.Red, 44));

                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                    ftr.Geometry = pt;
                    ftr["Name"] = JzPeopleName;                    

                    ftr.Style = cs;
                    tblcar.InsertFeature(ftr);

                    DPoint dpt = new DPoint(x, y);
                    MapControl1.Map.Center = dpt;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetJZPolice");
            }
        }

        /// <summary>
        /// 移除警员图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void ProtectMap()
        {
            try
            {
                if (this.Visible == false)   //jie.zhang 2010-0826  如果GPS警员模块不可见，不进行图元的添加
                {
                    if (MapControl1.Map.Layers["PeopleLayer"] != null)
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
                    if (MapControl1.Map.Layers["PeopleLabel"] != null)
                        MapControl1.Map.Layers.Remove("PeopleLabel");

                    return;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ProtectMap");
            }
        }

        /// <summary>
        /// 根据警力编号在地图上画点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="name">警力编号</param>
        private void AppSelect(string name)
        {
            try
            {
                ProtectMap();

                string sql = "Select X,Y from GPS警员 where 警力编号 ='" + name + "'";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    double x = Convert.ToDouble(dt.Rows[0]["X"]);
                    double y = Convert.ToDouble(dt.Rows[0]["Y"]);


                    Catalog cat = MapInfo.Engine.Session.Current.Catalog;


                    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("SelcLayer");
                    Table tblTemp = cat.GetTable("SelcLayer");
                    if (tblTemp != null) //Table exists close it
                    {
                        cat.CloseTable("SelcLayer");
                    }

                    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表_ID", 50));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("表名", 50));

                    tblTemp = cat.CreateTable(tblInfoTemp);
                    FeatureLayer lyr = new FeatureLayer(tblTemp);

                    if (this.Visible == false)   //jie.zhang 2010-0826
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("SelcLayer");
                        return;
                    }

                    MapControl1.Map.Layers.Insert(0, lyr);

                    MapInfo.Mapping.Map map = MapControl1.Map;
                    MapInfo.Mapping.FeatureLayer lyrcar = MapControl1.Map.Layers["SelcLayer"] as FeatureLayer;
                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("SelcLayer");

                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                    CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("Police.bmp", BitmapStyles.None, System.Drawing.Color.Red, 30));

                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                    ftr.Geometry = pt;
                    ftr["Name"] = JzPeopleName;

                    ftr.Style = cs;
                    tblcar.InsertFeature(ftr);

                    DPoint dpt = new DPoint(x, y);
                    MapControl1.Map.Center = dpt;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AppSelect");
            }
        }

        /// <summary>
        /// 初始化添加警员
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void AddPeopleFtr()
        {
            try
            {
                FeatureLayer lyrPeople = MapControl1.Map.Layers["PeopleLayer"] as FeatureLayer;   //(MapInfo.Mapping.FeatureLayer)map.Layers["PeopleLayer"];
                Table tblPeople = MapInfo.Engine.Session.Current.Catalog.GetTable("PeopleLayer");

                string sqlcmd = string.Empty;
                _strUseRegion = this.StrRegion;
                _strZDRegion = this.StrRegion1;
                if (_strUseRegion == string.Empty && _strZDRegion == string.Empty)
                {
                    MessageBox.Show(@"没有设置区域权限", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    // 权限设置（派出所及中队权限）
                    string regionStr = "";   // 存放权限条件
                    if (_strUseRegion != "顺德区")   // edit by fisher in 09-12-08
                    {
                        if (_strUseRegion != "")
                        {
                            if (Array.IndexOf(_strUseRegion.Split(','), "大良") > -1)
                            {
                                _strUseRegion = _strUseRegion.Replace("大良", "大良,德胜");
                            }
                            regionStr += " 派出所名 in ('" + _strUseRegion.Replace(",", "','") + "')";

                            if (_strZDRegion != "")
                            {
                                if (regionStr.IndexOf("and") > -1)
                                {
                                    regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                    regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                }
                                else
                                {
                                    regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                }
                            }
                        }
                        else if (_strUseRegion == "")
                        {
                            if (_strZDRegion != "")
                            {
                                if (regionStr.IndexOf("and") > -1)
                                {
                                    regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                    regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                }
                                else
                                {
                                    regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                }
                            }
                            else
                            {
                                MessageBox.Show("您没有查询权限!");
                                return;
                            }
                        }
                    }

                    if (_strZDRegion.IndexOf("顺德区") < -1)
                    {
                        sqlcmd = "Select * from GPS警员";
                    }
                    else
                    {
                        sqlcmd = "Select * from GPS警员 where " + regionStr;
                    }
                }
          
                DataTable dt = GetTable(sqlcmd);
                
                if (this.Visible == false)   //jie.zhang 2010-0826
                {
                    if (MapControl1.Map.Layers["PeopleLayer"] != null)
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
                    if (MapControl1.Map.Layers["PeopleLabel"] != null)
                        MapControl1.Map.Layers.Remove("PeopleLabel");

                    return;
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["警力编号"].ToString();
                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        if (lyrPeople != null)
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrPeople.CoordSys, new DPoint(xv, yv));
                            CompositeStyle cs = new CompositeStyle();
                            cs.ApplyStyle(new BitmapPointStyle("Police.bmp", BitmapStyles.None, System.Drawing.Color.Red, 20));
                            Feature ftr = new Feature(tblPeople.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = tempname;
                            ftr["表_ID"] = tempname;
                            ftr["表名"] = "GPS警员";

                            if (this.Visible == false)   //jie.zhang 2010-0826
                            {
                                if (MapControl1.Map.Layers["PeopleLayer"] != null)
                                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
                                if (MapControl1.Map.Layers["PeopleLabel"] != null)
                                    MapControl1.Map.Layers.Remove("PeopleLabel");

                                return;
                            }

                            tblPeople.InsertFeature(ftr);
                        }

                        if (tempname == GzPeopleName)
                        {
                            //Trackline(Xx, Yy, xv, yv);
                            Xx = xv;
                            Yy = yv;
                            DPoint dpoint = new DPoint(xv, yv);
                            Boolean inflag = IsInBounds(xv, yv);
                            if (inflag == false)
                            {
                                MapControl1.Map.Center = dpoint;
                            }
                        }
                        if (tempname != JzPeopleName) continue;
                        DPoint dpt = new DPoint(xv, yv);
                        MapControl1.Map.Center = dpt;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "11-初始化添加警员");
            }           
        }

        /// <summary>
        /// 设置当前图层可选择
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="layername1">图层名称</param>
        private void SetLayerSelect(string layername1)
        {
            try
            {
                Map map = MapControl1.Map;
                for (int i = 0; i < map.Layers.Count; i++)
                {
                    IMapLayer layer = map.Layers[i];
                    LayerHelper.SetSelectable(layer, false);
                }
                LayerHelper.SetSelectable(MapControl1.Map.Layers[layername1], true);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "12-设置图层可选");
            }
        }


        /// <summary>
        /// 更改模块区域权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void SetUserRegion()
        {
            //if (strRegion == "" && strRegion1 == "")// 既没有派出所权限，也没有中队权限
            //{
            //    return;
            //}
            //else if (strRegion == "" && strRegion1 != "") // 有中队权限，没有派出所权限
            //{
            //    StrUseRegion = GetPolice(strRegion1);

            //}
            //else if (strRegion != "" && strRegion1 != "")  // 有中队权限，也有派出所权限
            //{
            //    StrUseRegion = strRegion + "," + GetPolice(strRegion1);
            //}

            _strUseRegion = "顺德区";
        }


        ///// <summary>
        ///// 根据中队名称获取所在派出所名称
        ///// </summary>
        ///// <param name="s1">中队名称</param>
        ///// <returns>派出所名称</returns>
        //private String GetPolice(string s1)
        //{
        //    string reg = string.Empty;

        //    try
        //    {
        //        string[] ZdArr = s1.Split(',');
        //        for (int i = 0; i < ZdArr.Length; i++)
        //        {
        //            string zdn = ZdArr[i];

        //            DataTable dt = GetTable("Select 所属派出所 from 基层民警中队 where 中队名='" + zdn + "'");

        //            if (dt.Rows.Count > 0)
        //            {
        //                foreach (DataRow dr in dt.Rows)
        //                {
        //                    if (i != ZdArr.Length - 1)
        //                    {
        //                        reg = reg + dr["所属派出所"].ToString() + ",";
        //                    }
        //                    else
        //                    {
        //                        reg = reg + dr["所属派出所"].ToString();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ExToLog(ex, "13-根据中队名称获取所在派出所名称");
        //    }
        //    return reg;
        //}


        /// <summary>
        /// 清除临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void Cleartemp()
        {
            try
            {
                if (MapControl1.Map.Layers["Track"] != null)
                {
                    //MapInfo.Engine.Session.Current.Catalog.CloseTable("Track");
                    ClearTrack();
                }

                if (MapControl1.Map.Layers["VideoPeopleLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoPeopleLayer");
                }

                if (MapControl1.Map.Layers["VideoPeopleLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("VideoPeopleLabel");
                }

                if (MapControl1.Map.Layers["PoliceGuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PoliceGuijiLayer");
                }

                if (MapControl1.Map.Layers["MajorTaskLayer"] != null)  // 清除重大任务临时图层
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("MajorTaskLayer");
                    MajorTaskTable = null;
                }

                if (MapControl1.Map.Layers["GPS警员"] != null)  // 清除重大任务临时图层
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("GPS警员");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Cleartemp");
            }
        }

        /// <summary>
        /// 清除图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-14
        /// </summary>
        /// <param name="tabAlias">图层名称</param>
        private void clearFeatures(string tabAlias)
        {
            try
            {
                //清除地图上添加的对象
                FeatureLayer fl = (FeatureLayer)MapControl1.Map.Layers[tabAlias];
                if (fl == null)
                {
                    return;
                }
                Table tableTem = fl.Table;

                //先清除已有对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);

                setTabInsertable(tabAlias, true);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "clearFeatures-31-清除图元");
            }
        }

        /// <summary>
        /// 停止警员监控
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void StopPolice()
        {
            try
            {
                 if (timePeople.Enabled)
                {
                    timePeople.Enabled = false;
                }

                if (this.timflash.Enabled)
                {
                    this.timflash.Enabled = false;                    
                }

                //GetPeopleflag = false;

                if (MapControl1.Map.Layers["PeopleLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
                }

                if (MapControl1.Map.Layers["PeopleLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("PeopleLabel");
                }

                if (MapControl1.Map.Layers["VideoPeopleLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoPeopleLayer");
                }

                if (MapControl1.Map.Layers["VideoPeopleLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("VideoPeopleLabel");
                }

                if (MapControl1.Map.Layers["SelcLayer"] != null)
                {
                    MapControl1.Map.Layers.Remove("SelcLayer");
                }
                
            }
            catch (Exception ex)
            {
                ExToLog(ex, "14-停止警员监控");
            }
        }

        /// <summary>
        /// 停止警员监控
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void DisposePolice() 
        {
            try
            {
                _toolDDbtn.Visible = false;

                if (timePeople.Enabled)
                {
                    timePeople.Enabled = false;
                    //ExToLog("GPS警员定时更新已关闭");

                    //MessageBox.Show("定时更新已关闭");
                }

                if (this.timflash.Enabled)
                {
                    this.timflash.Enabled = false;

                    //ExToLog("GPS警员定时更新已关闭");

                    //MessageBox.Show("定时闪现已关闭");
                }

                GetPeopleflag = false;

                if (MapControl1.Map.Layers["Track"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("Track");
                }

                if (MapControl1.Map.Layers["PeopleLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PeopleLayer");
                }

                if (MapControl1.Map.Layers["PeopleLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("PeopleLabel");
                }

                if (MapControl1.Map.Layers["VideoPeopleLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoPeopleLayer");
                }

                if (MapControl1.Map.Layers["VideoPeopleLabel"] != null)
                {
                    MapControl1.Map.Layers.Remove("VideoPeopleLabel");
                }

                if (MapControl1.Map.Layers["PoliceGuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PoliceGuijiLayer");
                }

                if (MapControl1.Map.Layers["PolGzLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("PolGzLayer");
                }

                if (MapControl1.Map.Layers["SelcLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("SelcLayer");
                }

                if (MapControl1.Map.Layers["GPS警员"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("GPS警员");
                }

                if (MapControl1.Map.Layers["MajorTaskLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("MajorTaskLayer");
                    MajorTaskTable = null;
                }

                GzPeopleName = "";
                JzPeopleName = "";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "14-停止警员监控");
            }
        }

        /// <summary>
        /// 切换模块时停止警员监控
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void UcGpsPoliceVisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                    DisposePolice();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "UcGpsPoliceVisibleChanged-切换模块时停止警员监控");
            }
        }
            
        /// <summary>
        /// 换页功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void BindingNavigator1ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Text)
                {
                    case "上一页":
                        _pageCurrent1--;
                        if (_pageCurrent1 < 1)
                        {
                            _pageCurrent1 = 1;
                            MessageBox.Show(@"已经是第一页，请点击“下一页”查看！");
                            return;
                        }
                        else
                        {
                            _pagenCurrent1 = _pageSize1 * (_pageCurrent1 - 1);
                        }
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                        break;
                    case "下一页":
                        _pageCurrent1++;
                        if (_pageCurrent1 > _pageCount1)
                        {
                            _pageCurrent1 = _pageCount1;

                            MessageBox.Show(@"已经是最后一页，请点击“上一页”查看！");

                            return;
                        }
                        else
                        {
                            _pagenCurrent1 = _pageSize1 * (_pageCurrent1 - 1);
                        }
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                        break;
                    case "转到首页":
                        if (_pageCurrent1 <= 1)
                        {
                            MessageBox.Show(@"已经是第一页，请点击“下一页”查看！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                            return;
                        }
                        else
                        {
                            _pageCurrent1 = 1;
                            _pagenCurrent1 = _pageSize1 * (_pageCurrent1 - 1);
                        }
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                        break;
                    case "转到尾页":
                        if (_pageCurrent1 > _pageCount1 - 1)
                        {
                            MessageBox.Show(@"已经是最后一页，请点击“上一页”查看！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                            return;
                        }
                        else
                        {
                            _pageCurrent1 = _pageCount1;
                            _pagenCurrent1 = _pageSize1 * (_pageCurrent1 - 1);
                        }
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                        break;
                    case "数据导出":
                        break;
                }

                //#region 数据导出
                //DataTable datatableExcel = new DataTable();
                //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                //#endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExToLog(ex, "15-换页功能");
            }
        }


        /// <summary>
        /// 设置每页显示的记录数目
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView1.Rows.Count > 0)
                {
                    _pageSize1 = Convert.ToInt32(TextNum1.Text);
                    _pageCurrent1 = 1;   //当前转到第一页
                    _pagenCurrent1 = _pageSize1 * (_pageCurrent1 - 1);
                    _pageCount1 = (_pagenMax1 / _pageSize1);//计算出总页数
                    if ((_pagenMax1 % _pageSize1) > 0) _pageCount1++;

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);

                    #region 数据导出
                    //DataTable datatableExcel = new DataTable();
                    //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "16-设置每页显示的记录数目");
            }
        }

        /// <summary>
        /// 翻转页码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void PageNow1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(PageNow1.Text) < 1 || Convert.ToInt32(PageNow1.Text) > _pageCount1)
                    {
                        MessageBox.Show(@"页码超出范围，请重新输入！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        PageNow1.Text = _pageCurrent1.ToString();
                        return;
                    }
                    else
                    {
                        _pageCurrent1 = Convert.ToInt32(PageNow1.Text);
                        _pagenCurrent1 = _pageSize1 * (_pageCurrent1 - 1);
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
            catch(Exception ex)
            {
                ExToLog(ex, "17-设置每页显示的记录数目");
            }
        }

        /// <summary>
        /// 更新警员坐标
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-22
        /// </summary>
        private void Timer1Tick(object sender, EventArgs e)
        {

            CreatePeopleLayer();
            
            AddGz();

            RefreshGrid();

            insertExeGroupPoint(); 　// 刷新重大任务表
        }

        /// <summary>
        /// 创建轨迹线
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="x1">点1的X坐标</param>
        /// <param name="y1">点1的Y坐标</param>
        /// <param name="x2">点2的X坐标</param>
        /// <param name="y2">点2的Y坐标</param>  
        /// <param name="color">轨迹线颜色</param>
        public void Trackline(double x1, double y1, double x2, double y2, Color color)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                Map map = MapControl1.Map;
                FeatureLayer workLayer = (FeatureLayer)map.Layers["Track"];
                Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("Track");

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                SimpleLineStyle lsty = new SimpleLineStyle(new LineWidth(3, LineWidthUnit.Pixel), 2, color);
                CompositeStyle cstyle = new CompositeStyle(lsty);

                Feature lft = new Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "18-创建轨迹线");
            }
        }

        /// <summary>
        /// 判读警员是否在可视范围
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="x">警员X坐标</param>
        /// <param name="y">警员Y坐标</param>
        /// <returns>布尔值(true-在可视范围 false-不在可视范围)</returns>
        public Boolean IsInBounds(double x, double y)
        {
            try
            {
                return MapControl1.Map.Bounds.x1 < x && x < MapControl1.Map.Bounds.x2 && MapControl1.Map.Bounds.y1 < y &&
                       y < MapControl1.Map.Bounds.y2;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "19-判断警员是否在可视范围");
                return false;
            }
        }

        private MapInfo.Geometry.DPoint dptStart;

        private System.Collections.ArrayList arrlstPoints = new ArrayList();
        /// <summary>
        /// 地图点击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            try
            {
                if (Visible)
                    switch (e.ToolName)
                    {
                        case "SelectPolygon":

                            if (tabControl1.SelectedTab == tabPage2)  // 重大任务
                            {
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
                                        arrlstPoints.Add(dptStart);
                                        int intCount = arrlstPoints.Count;
                                        if (intCount <= 3)
                                        {
                                            MessageBox.Show("请画3个以上的点形成面来查找面内对象!", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                            return;
                                        }
                                        MapInfo.Geometry.DPoint[] dptPoints = new DPoint[intCount];
                                        for (int j = 0; j < intCount; j++)
                                        {
                                            dptPoints[j] = (MapInfo.Geometry.DPoint)arrlstPoints[j];
                                        }

                                        //用闭合的环构造一个面		
                                        MapInfo.Geometry.AreaUnit costAreaUnit;
                                        costAreaUnit = MapInfo.Geometry.CoordSys.GetAreaUnitCounterpart(DistanceUnit.Kilometer);
                                        MapInfo.Geometry.CoordSys objCoordSys = this.MapControl1.Map.GetDisplayCoordSys();
                                        MultiPolygon objPolygon = new MultiPolygon(objCoordSys, CurveSegmentType.Linear, dptPoints);
                                        if (objPolygon == null)
                                        {
                                            return;
                                        }
                                        GetRestructMajorTask((FeatureGeometry)objPolygon);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                switch (e.ToolStatus)
                                {
                                    case ToolStatus.End:
                                        if (GetPeopleflag)
                                        {
                                            MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                                            FeatureLayer lyr = MapControl1.Map.Layers["PeopleLayer"] as FeatureLayer;

                                            if (lyr != null) RsfcView = session.Selections.DefaultSelection[lyr.Table];
                                            if (RsfcView != null)
                                            {
                                                string searchName = "";
                                                int i = 1;
                                                foreach (Feature f in RsfcView)
                                                {
                                                    foreach (Column col in f.Columns)
                                                    {
                                                        if (col.ToString() != "NAME") continue;
                                                        string ftename = f["NAME"].ToString();
                                                        if (i == RsfcView.Count || RsfcView.Count == 1)
                                                        {
                                                            searchName = searchName + "警力编号 = '" + ftename + "'";
                                                        }
                                                        else
                                                        {
                                                            searchName = searchName + "警力编号 = '" + ftename + "' or ";
                                                        }
                                                        i = i + 1;
                                                    }
                                                }

                                                //多边形查询并显示在DataGridView中
                                                _canNm = searchName;
                                                AddGrid();

                                                SetViewFlag = true;
                                                if (RsfcView.Count > 0)
                                                {
                                                    if (RsfcView.Count == 1)
                                                    {
                                                        foreach (Feature f in RsfcView)
                                                        {
                                                            MapControl1.Map.Center = f.Geometry.Centroid;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        MapControl1.Map.SetView(RsfcView.Envelope);
                                                    }
                                                }
                                                WriteEditLog("", "多边形选择");
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "Select":
                            switch (e.ToolStatus)
                            {
                                case ToolStatus.End:
                                    if (GetPeopleflag)
                                        try
                                        {
                                            MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                                            FeatureLayer lyr = MapControl1.Map.Layers["PeopleLayer"] as FeatureLayer;

                                            if (lyr != null)
                                            {
                                                IResultSetFeatureCollection resultSetFeatureCollection =
                                                    session.Selections.DefaultSelection[lyr.Table];
                                                try
                                                {
                                                    if (resultSetFeatureCollection != null)
                                                        if (resultSetFeatureCollection.Count > 0)
                                                            foreach (Feature f in resultSetFeatureCollection)
                                                            {
                                                                string ftename = f["Name"].ToString();

                                                                if (dataGridView1.Rows.Count > 0)
                                                                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                                                    {
                                                                        if (
                                                                            dataGridView1.Rows[i].Cells[0].Value.ToString() !=
                                                                            ftename) continue;
                                                                        dataGridView1.CurrentCell =
                                                                            dataGridView1.Rows[i].Cells[0];
                                                                        break;
                                                                    }
                                                            }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ExToLog(ex, "21-获取DG中的数据");
                                                }
                                            }
                                            WriteEditLog("", "单击选择");
                                        }
                                        catch (Exception ex)
                                        {
                                            ExToLog(ex, "22-获取DG中的数据");
                                        }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "MoveTool":
                            switch (e.ToolStatus)
                            {
                                case ToolStatus.Start:

                                    try
                                    {

                                        if (CaoType == "") return;

                                        double x = 0;
                                        double y = 0;


                                        this.AddJZSJ(this.dataGridView1.CurrentRow.Cells["警力编号"].ToString(), e.MapCoordinate.x, e.MapCoordinate.y);

                                        if (MessageBox.Show("是否采用当前校正坐标值?","系统提示",MessageBoxButtons.YesNo,MessageBoxIcon.Information) == DialogResult.Yes)
                                        {
                                            x = e.MapCoordinate.x;
                                            y = e.MapCoordinate.y;

                                            this.MapControl1.Tools.LeftButtonTool = "Select";
                                        }
                                        else if (MessageBox.Show("是否采用当前校正坐标值?", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                                        {
                                            x = Convert.ToDouble(this.dataGridView1.CurrentRow.Cells["X"]);
                                            y = Convert.ToDouble(this.dataGridView1.CurrentRow.Cells["Y"]);
                                        }

                                        if (CaoType == "关联业务")
                                        {
                                            this.frmbuss.x = x;
                                            this.frmbuss.y = y;
                                            this.frmbuss.Visible = true;

                                            if (MapControl1.Map.Layers["JZLayer"] != null)
                                            {
                                                Session.Current.Catalog.CloseTable("JZLayer");
                                            }      
                                        }
                                        else if (CaoType == "信息点")
                                        {
                                            frmxxd fxxd = new frmxxd();
                                            if (fxxd.ShowDialog(this) == DialogResult.OK)
                                            {
                                                string sql = "insert into 信息点(ID,名称,类别,MI_STYLE,MI_PRINX,GEOLOC) values(" +
                                                            "(select max(ID)+1 from 信息点),'" + fxxd.Xxname + "'," + fxxd.Xxlb + ",'Symbol (34,16711680,9)',(select max(MI_PRINX)+1 from 信息点)," +
                                                            "MDSYS.SDO_GEOMETRY(2001,8307,MDSYS.SDO_POINT_TYPE(" + Convert.ToDouble(this.dataGridView1.Rows[0].Cells["X"].Value) + "," + Convert.ToDouble(dataGridView1.Rows[0].Cells["Y"].Value) + ",NULL),NULL,NULL))";
                                                RunCommand(sql);
                                            }
                                            fxxd.Close();

                                            if (MapControl1.Map.Layers["JZLayer"] != null)
                                            {
                                                Session.Current.Catalog.CloseTable("JZLayer");
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
                        case "SelByPolygon":
                           
                            break;
                        default:
                            break;
                    }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "20-警员周边查询工具");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void AddJZSJ(string id,double x,double y)
        {
            try
            {
                if (MapControl1.Map.Layers["JZLayer"] != null)
                {
                    Session.Current.Catalog.CloseTable("JZLayer");
                }

                Catalog Cat = Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("JZLayer");
                Table tblTemp = Cat.GetTable("JZLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("JZLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                MapControl1.Map.Layers.Insert(0, lyr);


                FeatureLayer lyrcar = MapControl1.Map.Layers["JZLayer"] as FeatureLayer;
                Table tblcar = Session.Current.Catalog.GetTable("JZLayer");

                FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(x, y));

                CompositeStyle cs = new CompositeStyle();
                cs.ApplyStyle(new SimpleVectorPointStyle(35, Color.Red, 20));

                Feature ftr = new Feature(tblcar.TableInfo.Columns);
                ftr.Geometry = pt;
                ftr.Style = cs;
                ftr["Name"] = id;
                tblcar.InsertFeature(ftr);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AddJZSJ");
            }
        }

        /// <summary>
        /// 查询按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Button1Click(object sender, EventArgs e)
        {
            SearchPeople();        
        }


        /// <summary>
        /// 查询关键字
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void SearchPeople()
        {
            try
            {
                if (this.CaseSearchBox.Text == "")
                {                     //终端ID,警员牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间
                    _canNm = "All";
                }
                else
                {
                    _canNm = comboBox1.Text + " like  '%" + CaseSearchBox.Text + "%' ";
                }
                //AddGrid();
                this.RefreshGrid();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "23-选择时间段");
            }
        }

        /// <summary>
        /// 设置DataGrid颜色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void DataGridView1DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e) 
        {
            try
            {
                if (dataGridView1.Rows.Count != 0)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = (i % 2) == 1 ? System.Drawing.Color.WhiteSmoke : System.Drawing.Color.LightBlue;
                    }


                    for (int k = 0; k < this.dataGridView1.Rows.Count; k++)
                        for (int j = 0; j < this.GzArrayName.Length; j++)
                            if (GzArrayName[j] != "")
                                if (GzArrayName[j] == this.dataGridView1.Rows[k].Cells["警力编号"].Value.ToString())
                                    this.dataGridView1.Rows[k].DefaultCellStyle.BackColor = this.GzArrayColor[j];
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "24-设置DataGrid颜色");
            }
        }

        /// <summary>
        /// 案件图元闪现
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void TimflashTick(object sender, EventArgs e)
        {
            try
            {
                Iflash = Iflash + 1;

                int i = Iflash % 2;
                if (i == 0)
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(_rsfcflash);
                }
                else
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }

                if (Iflash % 10 == 0)
                {

                    timflash.Enabled = false;

                    //GetGzPoistion();
                }
            }
            catch (Exception ex)
            {
                timflash.Enabled = false;
                ExToLog(ex, "25-案件图元闪现");
            }
        }


        private IResultSetFeatureCollection _rsfcflash;
        private string _selectPeopleName = string.Empty;
        
        /// <summary>
        /// 双击列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void DataGridView1CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow != null)
                {
                    string tempname = dataGridView1.CurrentRow.Cells[0].Value.ToString();

                    _selectPeopleName = dataGridView1.CurrentRow.Cells[0].Value.ToString();

                    this.toolgz.Text = "跟踪";

                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (this.GzArrayName[i] == this._selectPeopleName)
                        {
                            this.toolgz.Text = "取消跟踪";
                            break;
                        }
                    }


                    //// Jay 20100827  更改了数据绑定方式。只加载了视图内的图元
                    double x = Convert.ToDouble(dataGridView1.CurrentRow.Cells["X"].Value);
                    double y = Convert.ToDouble(dataGridView1.CurrentRow.Cells["Y"].Value);
                    DPoint dpt = new DPoint(x, y);
                    MapControl1.Map.SetView(new DPoint(x, y), this.MapControl1.Map.GetDisplayCoordSys(), this.getScale());

                    const string tblname = "PeopleLayer";

                    //提取当前选择的信息的通行警员编号作为主键值

                    Map map = MapControl1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    _rsfcflash = null;

                    MIConnection conn = new MIConnection();
                    conn.Open();

                    MICommand cmd = conn.CreateCommand();
                    cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() + " where Name = @name ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@name", tempname);
                    MapInfo.Geometry.CoordSys cSys = MapControl1.Map.GetDisplayCoordSys();

                    _rsfcflash = cmd.ExecuteFeatureCollection();
                    if (_rsfcflash.Count > 0)
                    {
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(_rsfcflash);
                        foreach (Feature f in _rsfcflash)
                        {
                            MapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                            MapControl1.Map.Center = f.Geometry.Centroid;
                            break;
                        }
                        cmd.Dispose();
                        conn.Close();

                        AppSelect(tempname);

                        Iflash = 0;

                        timflash.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "26-在dgvip列表上双击");
            }
        }

        /// <summary>
        /// 获取缩放比例
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>缩放比例</returns>
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
                ExToLog(ex, "26-2-getScale");
                return 0;
            }
        } 

        /// <summary>
        /// 数据表dataGridViewKakou双击
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void DataGridView1CellDoubleClick(object sender, DataGridViewCellEventArgs e) 
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

           try
            {
                DPoint dp = new DPoint();

                const string sqlFields = "警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间";
                if (dataGridView1.CurrentRow != null)
                {
                    string strSql = "Select " + sqlFields + " from gps警员 where 警力编号 = '" + dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'and x is not null and y is not null";

                    DataTable datatable = GetTable(strSql);

                    if (datatable.Rows.Count > 0)
                    {
                        //闪现图元
                        //////////////////////////////////////////
                        string tempname = dataGridView1.CurrentRow.Cells[0].Value.ToString();

                        const string tblname = "PeopleLayer";

                        Map map = MapControl1.Map;

                        if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                        {
                            return;
                        }
                        _rsfcflash = null;


                        MIConnection conn = new MIConnection();

                        try
                        {
                            if (conn.State == ConnectionState.Open)
                                conn.Close();
                            conn.Open();

                            MICommand cmd = conn.CreateCommand();
                            cmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() + " where Name = @name ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@name", tempname);

                            _rsfcflash = cmd.ExecuteFeatureCollection();
                            if (_rsfcflash.Count > 0)
                            {
                                MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                                MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(_rsfcflash);
                                foreach (Feature f in _rsfcflash)
                                {
                                    MapControl1.Map.Center = f.Geometry.Centroid;
                                    dp.x = f.Geometry.Centroid.x;
                                    dp.y = f.Geometry.Centroid.y;
                                    break;
                                }
                                cmd.Dispose();

                                Iflash = 0;

                                timflash.Enabled = true;
                            }
                        }
                        catch (Exception ex)
                        {
                          Console.WriteLine(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                                conn.Close();
                        }

                        /////////////////////////////////////////                    

                        System.Drawing.Point pt = new System.Drawing.Point();
                        if (dp.x == 0 || dp.y == 0)
                        {
                            Screen scren = Screen.PrimaryScreen;
                            pt.X = scren.WorkingArea.Width / 2;
                            pt.Y = 10;
                            DisPlayInfo(datatable, pt);
                            return;
                        }
                        MapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                        pt.X += Width + 10;
                        pt.Y += 80;

                        DisPlayInfo(datatable, pt);

                        //WriteEditLog("终端警员号牌='" + this.dataGridViewKakou.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情", "V_CROSS");
                    }
                }
            }
            catch (Exception ex)
            {               
                ExToLog(ex, "27-数据表dataGridViewKakou双击");
             }
        }


        private FrmInfo _frminfo = new FrmInfo();
        /// <summary>
        /// 显示警员详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void DisPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (_frminfo.Visible == false)
                {
                    _frminfo = new FrmInfo();
                    _frminfo.SetDesktopLocation(-30, -30);
                    _frminfo.Show();
                    _frminfo.Visible = false;
                }
                _frminfo.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, " 28-显示报警详细信息");
            }
        }


        /// <summary>
        /// 更新数据表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void RefreshGrid()
        {          
            try
            {
                isShowPro(true);
                if (_canNm != "")
                {
                    string sql = string.Empty;

                    _strUseRegion = this.StrRegion;
                    _strZDRegion = this.StrRegion1;
                    if (_strUseRegion == string.Empty && _strZDRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show(@"没有设置区域权限", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        // 权限设置（派出所及中队权限）
                        string regionStr = "";   // 存放权限条件


                        if (_strUseRegion != "顺德区")   // edit by fisher in 09-12-08
                        {
                            if (_strUseRegion != "")
                            {
                                if (Array.IndexOf(_strUseRegion.Split(','), "大良") > -1)
                                {
                                    _strUseRegion = _strUseRegion.Replace("大良", "大良,德胜");
                                }
                                regionStr += " 派出所名 in ('" + _strUseRegion.Replace(",", "','") + "')";

                                if (_strZDRegion != "")
                                {
                                    if (regionStr.IndexOf("and") > -1)
                                    {
                                        regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                        regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                    }
                                    else
                                    {
                                        regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                    }
                                }
                            }
                            else if (_strUseRegion == "")
                            {
                                if (_strZDRegion != "")
                                {
                                    if (regionStr.IndexOf("and") > -1)
                                    {
                                        regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                        regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                    }
                                    else
                                    {
                                        regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("您没有查询权限!");
                                    return;
                                }
                            }
                        }

                        if (_strUseRegion.IndexOf("顺德区") > -1)
                        {
                            sql = _canNm == "All"
                                         ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y, 定位更新时间 from GPS警员 where 警力编号 is not null"
                                         : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm;
                        }
                        else
                        {
                            sql = _canNm == "All"
                                      ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + regionStr
                                      : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm +
                                        " and " + regionStr;
                        }

                        //switch (_strUseRegion)
                        //{
                        //    case "顺德区":
                        //        sql = _canNm == "All"
                        //                  ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员"
                        //                  : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm;
                        //        break;
                        //    default:
                        //        if (Array.IndexOf(StrRegion.Split(','), "大良") > -1 && StrRegion.IndexOf("德胜") < 0)
                        //            StrRegion = StrRegion.Replace("大良", "大良,德胜");
                        //        sql = _canNm == "All"
                        //                  ? "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where 派出所名 in ('" +
                        //                    _strUseRegion.Replace(",", "','") + "')"
                        //                  : "select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y,定位更新时间 from GPS警员 where " + _canNm +
                        //                    " and 派出所名 in('" + _strUseRegion.Replace(",", "','") + "')";
                        //        break;
                        //}
                    }


                    string Gzstring1 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring1 = Gzstring1 + " 警力编号='" + GzArrayName[i] + "' or ";
                        }
                        else if(GzArrayName[i]=="") 
                        {
                            Gzstring1 = Gzstring1.Substring(0, Gzstring1.LastIndexOf("or") - 1);

                            break;
                        }
                        else if (i == this.GzArrayName.Length-1)
                        {
                            Gzstring1 = Gzstring1 + " 警力编号='" + GzArrayName[i] + "'";
                        }
                    }


                    string Gzstring2 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring2 = Gzstring2 + " 警力编号<>'" + GzArrayName[i] + "' and ";
                        }
                        else if (GzArrayName[i] == "" ) 
                        {
                            Gzstring2 = Gzstring2.Substring(0, Gzstring2.LastIndexOf("and") - 1);

                            break;
                        }
                        else if (i == this.GzArrayName.Length-1)
                        {
                            Gzstring2 = Gzstring2 + " 警力编号<>'" + GzArrayName[i] + "'";
                        }
                    }

                    

                    string tsql = string.Empty;

                    if (Gzstring1 != "" && Gzstring2 != "")
                        tsql = sql + " and " + Gzstring1 + " Union all " + sql + " and " + Gzstring2;
                    else
                        tsql = sql;

                    if (Gzstring1 != "" && Gzstring2 != "" && ordername != "")
                        tsql = sql + " and " + Gzstring1 + " Union all select 警力编号,派出所名,中队名,所属科室,当前任务,设备编号,X,Y, 定位更新时间 from (" + sql +" and "+ Gzstring2+" order by "+ ordername+" desc)";
                    if (Gzstring1 == "" && Gzstring2 == "" && ordername != "")
                        tsql = sql + " order by " + ordername +" desc";

                    if (tsql == "") return;


                    DataTable dt = GetTable(tsql);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("查询结果为0！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }


                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    _pagedt1 = dt;
                    InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);


                    //if (dt.Rows.Count > 0)
                    //    foreach (DataRow dr in dt.Rows)
                    //    {
                    //        string peoplenum = Convert.ToString(dr["警力编号"]);
                    //        if (dataGridView1.Rows.Count > 0)
                    //            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    //            {
                    //                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == peoplenum)
                    //                {
                    //                    dataGridView1.Rows[i].Cells[1].Value = dr["派出所名"].ToString();
                    //                    dataGridView1.Rows[i].Cells[2].Value = dr["中队名"].ToString();
                    //                    dataGridView1.Rows[i].Cells[3].Value = dr["所属科室"].ToString();
                    //                    dataGridView1.Rows[i].Cells[4].Value = dr["当前任务"].ToString();
                    //                    dataGridView1.Rows[i].Cells[5].Value = dr["设备编号"].ToString();
                    //                    dataGridView1.Rows[i].Cells[6].Value = dr["X"].ToString();
                    //                    dataGridView1.Rows[i].Cells[7].Value = dr["Y"].ToString();
                    //                    dataGridView1.Rows[i].Cells[8].Value = dr["定位更新时间"].ToString();
                    //                }

                    //                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == _selectPeopleName)
                    //                    //dataGridView1.Rows[i].Selected = true;
                    //                    dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                    //            }
                    //    }

                    #region 数据导出

                    try
                    {
                        //excelSql = cmd.CommandText;
                        //excelSql = "select * " + excelSql.Substring(excelSql.IndexOf("from"));
                        //string sRegion2 = strRegion2;
                        //string sRegion3 = strRegion3;

                        //if (strRegion2 != "顺德区")
                        //{
                        //    if (strRegion2 != "")
                        //    {
                        //        if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                        //        {
                        //            sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                        //        }
                        //        excelSql += " and (权限单位 in ('" + sRegion2.Replace(",", "','") + "'))";
                        //    }
                        //    else if (strRegion2 == "")
                        //    {
                        //        if (excelSql.IndexOf("where") < 0)
                        //        {
                        //            excelSql += " where 1=2 ";
                        //        }
                        //        else
                        //        {
                        //            excelSql += " and 1=2 ";
                        //        }
                        //    }
                        //}

                        //cmd.CommandText = excelSql;
                        //apt1 = new OracleDataAdapter(cmd);
                        //DataTable datatableExcel = new DataTable();
                        //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                        //if (dtExcel != null) dtExcel.Clear();
                        //dtExcel = datatableExcel;
                    }
                    catch (Exception ex)
                    {
                        isShowPro(false);
                        Console.WriteLine(ex.ToString());
                    }
                    #endregion

                    WriteEditLog(sql, "查询");
                    this.toolPro.Value = 2;
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                ExToLog(ex, "29-更新数据表时发生错误");
            }
            finally
            {
                CreatePeopleLayer();
                this.toolPro.Value = 1;
                Application.DoEvents();
                isShowPro(false);
            }
        }

        /// <summary>
        /// 刷新按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void BtnRefreshClick(object sender, EventArgs e) 
        {
            RefreshGrid();
        }

        private string[] GzArrayName = new string[10] { "", "", "", "", "", "", "", "", "", "" };  //跟踪警员的名称数组
        private Color[] GzArrayColor = new Color[10] { Color.SandyBrown, Color.Green, Color.Yellow, Color.DarkCyan, Color.Firebrick, Color.Fuchsia,  
                                                       Color.Gainsboro, Color.Gold, Color.PaleGreen, Color.Khaki }; //跟踪车辆的颜色数组
        private double[] GzArrayLx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的经度数组 经度
        private double[] GzArrayLy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的纬度数组 纬度

        private double[] GzArrayNx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的经度数组 经度
        private double[] GzArrayNy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的纬度数组 纬度

        /// <summary>
        /// 刷新跟踪警员
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void RefreshGzArray()
        {
            try
            {
                GzArrayLx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的经度数组 经度
                GzArrayLy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的纬度数组 纬度

                GzArrayNx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的经度数组 经度
                GzArrayNy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //跟踪警员的上个点的纬度数组 纬度

                for (int i = 0; i < this.GzArrayName.Length; i++)
                {
                    if (GzArrayName[i] != "")
                    {
                        DataTable dt = GetTable("Select * from gps警员 where 警力编号 like '%" + this.GzArrayName[i] + "%'");
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
                ExToLog(ex, "RefreshGzArray");
            }
        }

        /// <summary>
        /// 初始化跟踪目标
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
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
                        DataTable dt = GetTable("Select * from gps警员 where 警力编号 like '%" + this.GzArrayName[i] + "%'");
                        if (dt.Rows.Count > 0)
                        {
                            GzArrayNx[i] = Convert.ToDouble(dt.Rows[0]["X"]);
                            GzArrayNy[i] = Convert.ToDouble(dt.Rows[0]["Y"]);
                        }
                    }
                }

                //创建临时层
    
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("PolGZLayer");
                Table tblTemp = Cat.GetTable("PolGZLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("PolGZLayer");
                }
                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(this.MapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                this.MapControl1.Map.Layers.Insert(0, lyr);


                MapInfo.Mapping.Map map = MapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = MapControl1.Map.Layers["PolGZLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("PolGZLayer");

                //加载数据
                for (int i = 0; i < GzArrayName.Length; i++)
                {
                    string tempname = GzArrayName[i];

                    if (tempname != "")
                    {

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(GzArrayNx[i], GzArrayNy[i])) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("Police.bmp", BitmapStyles.None, System.Drawing.Color.Red, 30));

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr["Name"] = tempname;                        

                        ftr.Style = cs;
                        tblcar.InsertFeature(ftr);

                        Trackline(GzArrayLx[i], GzArrayLy[i], GzArrayNx[i], GzArrayNy[i], GzArrayColor[i]);

                        GzArrayLx[i] = GzArrayNx[i];
                        GzArrayLy[i] = GzArrayNy[i];
                    }
                }


                //设置居中对象
                if (this.JzPeopleName != "")
                    SetJZPolice();

            }
            catch (Exception ex)
            {
                ExToLog(ex, "ucPolice-15-初始化跟踪目标");
            }
        }

        /// <summary>
        /// 选择工具
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void ToolSelect(object sender, ToolStripItemClickedEventArgs e)
        {
            string selectext = e.ClickedItem.Text;

            try
            {
                switch (selectext)
                {
                    case "单击选择":                        
                        MapControl1.Tools.LeftButtonTool = "Select";
                        break;
                    case "范围查询":
                        MapControl1.Tools.LeftButtonTool = "SelectPolygon";
                        break;
                    case "跟踪警员":
                        _toolDDbtn.DropDownItems[2].Text = @"取消跟踪";
                        if (dataGridView1.CurrentRow != null)
                            GzPeopleName = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        Setthefirstpoint();
                        WriteEditLog(GzPeopleName, "跟踪警员");
                        break;
                    case "取消跟踪":
                        _toolDDbtn.DropDownItems[2].Text = @"跟踪警员";
                        GzPeopleName = "";
                        Xx = 0;
                        Yy = 0;
                        //ClearTrack();
                        WriteEditLog(GzPeopleName, "取消跟踪");
                        break;
                    case "警员居中":
                        if (dataGridView1.CurrentRow != null)
                            JzPeopleName = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        _toolDDbtn.DropDownItems[3].Text = @"取消居中";
                        WriteEditLog(JzPeopleName, "警员居中");
                        break;
                    case "取消居中":
                        JzPeopleName = "";
                        _toolDDbtn.DropDownItems[3].Text = @"警员居中";
                        WriteEditLog(JzPeopleName, "取消居中");
                        break;
                    case "轨迹回放":
                        Fhistory.Visible = true;
                        Fhistory.User = User;
                        if (dataGridView1.CurrentRow != null)
                            Fhistory.comboBox1.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        break;
                    case "跟踪目标":
                        DataTable dt = GetTable("Select 警力编号 from GPS警员 order by 警力编号");
                        if (dt.Rows.Count > 0)
                        {
                            frmGz frmgz = new frmGz(dt, GzArrayName, _conStr);
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
                ExToLog(ex, "30-选择工具时发生错误");
            }
        }

        /// <summary>
        /// 添加轨迹点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void Setthefirstpoint()
        {
            try
            {
                IResultSetFeatureCollection resultSetFeatureCollection = GetFirstPoint("PeopleLayer");
                if (resultSetFeatureCollection.Count > 0)
                {
                    foreach (Feature fpeople in resultSetFeatureCollection)
                    {
                        if (fpeople["Name"].ToString() != GzPeopleName) continue;
                        Xx = fpeople.Geometry.Centroid.x;
                        Yy = fpeople.Geometry.Centroid.y;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "31-添加轨迹点");
            }
        }

        /// <summary>
        /// 删除轨迹
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void ClearTrack()
        {
            try
            {
                if (MapControl1.Map.Layers["Track"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("Track");
                }

                CreateTrackLayer();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "32-删除轨迹");
            }
        }

        /// <summary>
        /// 获得跟踪警员的所有点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="layerName">图层名称</param>
        /// <returns>警员的所有点</returns>
        private IResultSetFeatureCollection GetFirstPoint(string layerName)
        {
            IResultSetFeatureCollection resultSetFeatureCollection = null;
            try
            {
               
                MIConnection micon = new MIConnection();
                micon.Open();

                string tblname = layerName;
                const string colname = "Name";

                MICommand micmd = micon.CreateCommand();
                if (MapControl1.Map.Layers != null)
                    micmd.CommandText = "select * from " + MapControl1.Map.Layers[tblname].ToString() + " where " + colname + "= '" + GzPeopleName + "'";
                resultSetFeatureCollection = micmd.ExecuteFeatureCollection();

                micon.Close();
                micon.Dispose();
                micmd.Cancel();
                micmd.Dispose();                
            }
            catch (Exception ex)
            {
                ExToLog(ex, "33-GetFirstPoint");
            }
            return resultSetFeatureCollection;
        }

        /// <summary>
        /// 设置自动刷新
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void 设置自动刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 frmset = new Form1();
            frmset.SecFresh = _sr;
            if (frmset.ShowDialog(this) == DialogResult.OK)
                try
                {
                    _sr = frmset.SecFresh;
                    CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                    CLC.INIClass.IniWriteValue("GPS警员", "更新频率", _sr.ToString()); 
                    timePeople.Interval = _sr*1000;
                    MessageBox.Show(@"更改成功", @"系统提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"更改失败", @"系统提示");
                    ExToLog(ex, "34-设置自动刷新");
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
        private static void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex,"clGPSPolice-ucGPSPolice"+ sFunc);
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
            catch (Exception ex)
            {
                ExToLog(ex, "WriteEditLog");
            }
        }

        /// <summary>
        /// 文本框自动补全功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();
                string Tablename = string.Empty;
                string strExp = string.Empty;

                if (keyword.Length < 1 || colfield.Length < 1) return;

                Tablename = "警力情况表";

                if (colfield == "警力编号" || colfield == "所属科室" || colfield == "设备编号")

                    //strExp = "select distinct(" + colfield + ") from " + Tablename + " t  where " + colfield + " like '" + keyword + "%' union all select distinct(" + colfield + ") from " + Tablename + " t  where " +
                    //                colfield + " like '%" + keyword + "%' and " + colfield + " not like '" + keyword + "%' and " + colfield + " not like '%" + keyword + "' union all select distinct(" + colfield +
                    //                ") from " + Tablename + " t where " + colfield + " like '%" + keyword + "' and " +colfield +" not like '%"+keyword+"%' and "+colfield +" not like '"+keyword +"%'";

                    strExp = "select distinct(" + colfield + ") from " + Tablename + " t  where " + colfield + " like '%" + keyword + "%' order by "+colfield;

                DataTable dt = GetTable(strExp);
                if (dt.Rows.Count < 1)
                    this.CaseSearchBox.GetSpellBoxSource(null);
                else
                    this.CaseSearchBox.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "35-点击显示下拉");
            }
        }

        /// <summary>
        /// 文本框自动补全功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {

                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();
                string Tablename = string.Empty;

                if (colfield != "派出所名" && colfield != "中队名") return;

                switch (colfield)
                {
                    case "派出所名":
                        Tablename = "基层派出所";
                        break;
                    case "中队名":
                        Tablename = "基层民警中队";
                        break;
                    case "":
                        Tablename = "社区警务室";
                        break;
                    default:
                        return;
                }

                string strExp = "select distinct(" + colfield + ") from " + Tablename +" order by "+ colfield;
                DataTable dt = GetTable(strExp);
                this.CaseSearchBox.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "35-点击显示下拉");
            }
        }

        /// <summary>
        /// 文本框
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void CaseSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        SearchPeople();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "CaseSearchBox_KeyDown");
            }
        }

        /// <summary>
        /// 显示或隐藏进度条
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="falg">布尔值(true-显示 false-隐藏)</param>
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
            catch(Exception ex)
            {
                ExToLog(ex, "isShowPro");
            }
        }

        /// <summary>
        /// 开始按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
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
                ExToLog(ex, "button2_Click");
            }
        }

        /// <summary>
        /// 实时更新警力位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                string sql = "select t.对讲机ID,t.X,t.Y from GPS110.警力gps位置 t where t.对讲机ID='" + this.GzPeopleName + "'";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string num = Convert.ToString(dr["对讲机ID"]);
                        double x = Convert.ToDouble(dr["X"]);
                        double y = Convert.ToDouble(dr["Y"]);

                        if (x > this.MapControl1.Map.Center.x)
                            x = x - 0.1;
                        else
                            x = x + 0.1;

                        if (y > this.MapControl1.Map.Center.y)
                            y = y - 0.1;
                        else
                            y = y - 0.1;

                        string sqltem = "update GPS110.警力gps位置 set GPS110.警力gps位置.x = " + x.ToString() + ",GPS110.警力gps位置.y=" + y.ToString() + " where GPS110.警力gps位置.对讲机ID='" + this.GzPeopleName + "'";
                        this.RunCommand(sqltem);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "timer1_Tick");
            }
        }


        private frmbus frmbuss = new frmbus();

        /// <summary>
        /// 坐标校正
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void 坐标校正ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.dataGridView1.CurrentRow == null)
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

                            MapControl1.Tools.LeftButtonTool = "MoveTool";
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


        private string CaoType = "";

        /// <summary>
        /// 关联业务
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 关联业务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //frmBus frmbus = new frmBus();
                if (MessageBox.Show("是否进行坐标校正？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    CaoType = "关联业务";
                    this.MapControl1.Tools.LeftButtonTool = "MoveTool";
                }
                else
                {
                    frmbuss.x = Convert.ToDouble(this.dataGridView1.CurrentRow.Cells["X"].Value);
                    frmbuss.y = Convert.ToDouble(this.dataGridView1.CurrentRow.Cells["Y"].Value);

                    frmbuss.Visible = true;

                    CaoType = "";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "21-窗口显示切换");
            }
        }

        private IResultSetFeatureCollection rsfcflash;

        /// <summary>
        /// 周边视频
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void 周边视频ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint();
                string tempname = dataGridView1.CurrentRow.Cells[0].Value.ToString(); //获取当前选择的行中 案件编号

                string tblname = "PeopleLayer";

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
                                      " where Name = @name ";
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

        private static NetworkStream ns; //网络数据流 

        private Boolean vf; // 通讯是否已经连接的标识

        /// <summary>
        /// 周边查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dp">坐标点</param>
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
                    clVideo.ucVideo fv = new clVideo.ucVideo(MapControl1, st, _toolDDbtn, _conStr, VideoPort, VideoString, VEpath, true,false);
                    fv.getNetParameter(ns, vf);
                    //fv.strRegion = this.strRegion;
                    //fv.strRegion1 = this.strRegion1;
                    //fv.SetUserRegion(this.strRegion, this.strRegion1);  //jie.zhang 20100118

                    fv.SearchVideoDistance(dp, distVideo,"GPS警员");
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

        /// <summary>
        /// 另存信息点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void 另存信息点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridView1.CurrentRow == null) return;
                //frmBus frmbus = new frmBus();
                if (MessageBox.Show("是否进行坐标校正？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    CaoType = "信息点";
                    this.MapControl1.Tools.LeftButtonTool = "MoveTool";
                }
                else
                {
                    if (Convert.ToDouble(dataGridView1.Rows[0].Cells["X"].Value) > 0 && Convert.ToDouble(dataGridView1.Rows[0].Cells["Y"].Value) > 0)
                    {
                        frmxxd fxxd = new frmxxd();
                        if (fxxd.ShowDialog(this) == DialogResult.OK)
                        {
                            string sql = "insert into 信息点(ID,名称,类别,MI_STYLE,MI_PRINX,GEOLOC) values(" +
                                        "(select max(ID)+1 from 信息点),'" + fxxd.Xxname + "'," + fxxd.Xxlb + ",'Symbol (34,16711680,9)',(select max(MI_PRINX)+1 from 信息点)," +
                                        "MDSYS.SDO_GEOMETRY(2001,8307,MDSYS.SDO_POINT_TYPE(" + Convert.ToDouble(this.dataGridView1.Rows[0].Cells["X"].Value) + "," + Convert.ToDouble(dataGridView1.Rows[0].Cells["Y"].Value) + ",NULL),NULL,NULL))";
                            RunCommand(sql);
                        }
                        fxxd.Close();
                    }

                    CaoType = "";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolSave");
            }
        }


        /// <summary>
        /// 传递网络参数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="ns1">网络数据流</param>
        /// <param name="vf1">通讯是否已经连接的标识</param>
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
        /// 右击菜单的跟踪按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void toolgz_Click(object sender, EventArgs e)
        {
            try
            {
                if (this._selectPeopleName != "")
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
                            if (lst.Items[i].ToString() == _selectPeopleName)
                            {
                                MessageBox.Show("该警员已经被设置为监控目标", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                        lst.Items.Add(_selectPeopleName);
                    }
                    else if (this.toolgz.Text == "取消跟踪")
                    {
                        for (int i = 0; i < lst.Items.Count; i++)
                        {
                            if (lst.Items[i].ToString() == _selectPeopleName)
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
            catch(Exception ex)
            { ExToLog(ex, "toolgz_Click"); }
        }

        string ordername = string.Empty;

        /// <summary>
        /// 点击列表头
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
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
            {
                ExToLog(ex, "dataGridView1_ColumnHeaderMouseClick");
            }
        }

        /// <summary>
        /// 创建重大任务功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        private void btnMajorTask_Click(object sender, EventArgs e)
        {
            try
            {
                frmMajorTask frmMaTa = new frmMajorTask(_conStr,true,majorTask);


                if (frmMaTa.ShowDialog() == DialogResult.OK)
                {
                    this.dataMajorTask.DataSource = frmMaTa.majorTable;
                    changRowColor(this.dataMajorTask);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnMajorTask_Click");
            }
        }

        /// <summary>
        /// 重组重大任务
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        /// <param name="rfc">地图图元</param>
        public void restructuringTask(IResultSetFeatureCollection rfc)
        {
            try
            {
             

            }
            catch (Exception ex)
            {
                ExToLog(ex, "restructuringTask");
            }
        }

        /// <summary>
        /// 拆分重大任务
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        /// <param name="rfc">地图图元</param>
        public void splitTask(IResultSetFeatureCollection rfc)
        {
            try
            {

            }
            catch (Exception ex)
            {
                ExToLog(ex, "splitTask");
            }
        }

        /// <summary>
        /// 创建执行组按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                frmExeGroup fmExe = new frmExeGroup(_conStr,true,null);

                if (fmExe.ShowDialog() == DialogResult.OK)
                {
                    this.dataExegroup.DataSource = fmExe.exeGroupTable;
                    this.changRowColor(this.dataExegroup);
                    insertExeGroupPoint();
                }

            }
            catch (Exception ex)
            {
                ExToLog(ex, "button3_Click");
            }
        }

        /// <summary>
        /// 切换功能模块
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.SelectedTab == this.tabPage2)
                {
                    DataSet setTable = new DataSet();
                    setTable = getMajorTasks();

                    this.dataMajorTask.DataSource = setTable.Tables[0];
                    this.changRowColor(this.dataMajorTask);
                    this.dataExegroup.DataSource = setTable.Tables[1];
                    this.changRowColor(this.dataExegroup);
                    insertExeGroupPoint();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tabControl1_SelectedIndexChanged");
            }
        }

        /// <summary>
        /// 将任务组信息在地图上显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        private void insertExeGroupPoint()
        {
            try
            {
                clearFeatures("MajorTaskLayer");  // 先清除地图上的点

                for (int j = 0; j < this.dataExegroup.Rows.Count; j++)
                {
                    try
                    {
                        string sql = "select X,Y from GPS警员 where 设备编号='" + this.dataExegroup.Rows[j].Cells["指挥员肩咪ID"].Value.ToString() + "'";
                        DataTable table = GetTable(sql);

                        majorTask = new string[] { dataExegroup.Rows[j].Cells["任务组编号"].Value.ToString(), 
                                                   dataExegroup.Rows[j].Cells["任务组名称"].Value.ToString(),
                                                   dataExegroup.Rows[j].Cells["指挥员肩咪ID"].Value.ToString(),
                                                   dataExegroup.Rows[j].Cells["参加任务人数"].Value.ToString()};

                        majorTaskLayer(majorTask, Convert.ToDouble(table.Rows[0][0]), Convert.ToDouble(table.Rows[0][1]),j);
                    }
                    catch   // 如果有异常忽略此条记录
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "insertExeGroupPoint");
            }
        }

        /// <summary>
        /// 改变行列表行的颜色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <param name="dgv">要改变行颜色的列表控件</param>
        private void changRowColor(DataGridView dgv)
        {
            try
            {
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dgv.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "changRowColor");
            }
        }

        private enum upInsert
        {
            update,
            insert
        }

        /// <summary>
        /// 查询任务及执行组数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <returns>包含多个Table的结果</returns>
        private DataSet getMajorTasks()
        {
            try
            {
                DataSet objSet = new DataSet();

                string strMajor = "select * from 重大任务";
                string strExeg = "select * from 重大任务组";

                objSet.Tables.Add(GetTable(strMajor));
                objSet.Tables.Add(GetTable(strExeg));

                return objSet;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getMajorTasks");
                return null;
            }
        }

        /// <summary>
        /// 拆分任务组
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        private void tsmSplit_Click(object sender, EventArgs e)
        {
            try
            {
                string[] _splitTask = new string[7];

                _splitTask[0] = this.dataExegroup.SelectedRows[0].Cells["任务组名称"].Value.ToString();
                _splitTask[1] = this.dataExegroup.SelectedRows[0].Cells["执行任务名称"].Value.ToString();
                _splitTask[2] = this.dataExegroup.SelectedRows[0].Cells["执行任务单位"].Value.ToString();
                _splitTask[3] = this.dataExegroup.SelectedRows[0].Cells["指挥员肩咪ID"].Value.ToString();
                _splitTask[4] = this.dataExegroup.SelectedRows[0].Cells["参加任务人数"].Value.ToString();
                _splitTask[5] = this.dataExegroup.SelectedRows[0].Cells["备用肩咪ID"].Value.ToString();
                _splitTask[6] = this.dataExegroup.SelectedRows[0].Cells["任务组编号"].Value.ToString();

                frmSplit fmSplit = new frmSplit(_splitTask,_conStr);

                if (fmSplit.ShowDialog() == DialogResult.OK)
                {
                    this.dataExegroup.DataSource = fmSplit.majorTable;
                    this.changRowColor(this.dataExegroup);
                    insertExeGroupPoint();  // 重新在地图上画点
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tsmSplit_Click");
            }
        }

        /// <summary>
        /// 结束重大任务
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-18
        /// </summary>
        private void tsmEndMajor_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("删除任务时正在执行当前任务的任务组会一并删除，确认删除？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string majorName = this.dataMajorTask.SelectedRows[0].Cells["任务名称"].Value.ToString(); ;

                    string delSql = "Delete from 重大任务 where 任务名称='" + majorName + "'";

                    RunCommand(delSql);   　// 先删除重大任务

                    string delGroupSql = "delete from 重大任务组 where 执行任务名称='" + majorName + "'";

                    RunCommand(delGroupSql);// 再删除在执此任务的所有组

                    // 重新初始化数据
                    DataSet objset = getMajorTasks();

                    this.dataMajorTask.DataSource = objset.Tables[0];
                    this.changRowColor(this.dataMajorTask);
                    this.dataExegroup.DataSource = objset.Tables[1];
                    this.changRowColor(this.dataExegroup);
                    insertExeGroupPoint();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tsmEndMajor_Click");
            }
        }

        /// <summary>
        /// 结束任务组
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-18
        /// </summary>
        private void tsmEnd_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("确认要删除该任务组？","提示",MessageBoxButtons.YesNo,MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string majorName = this.dataExegroup.SelectedRows[0].Cells["任务组编号"].Value.ToString();

                    string delGroupSql = "delete from 重大任务组 where 任务组编号='" + majorName + "'";

                    RunCommand(delGroupSql);// 删除任务组

                    // 重新初始化任务组数据
                    DataSet objset = getMajorTasks();

                    this.dataExegroup.DataSource = objset.Tables[1];
                    this.changRowColor(this.dataExegroup);
                    insertExeGroupPoint();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tsmEnd_Click");
            }
        }

        /// <summary>
        /// 修改任务组
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-18
        /// </summary>
        private void tsmModify_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewCellCollection dgc = this.dataExegroup.SelectedRows[0].Cells;
                string[] _exeGroup = new string[] { dgc["任务组编号"].Value.ToString(), dgc["任务组名称"].Value.ToString(), 
                    　　　　　　　　　　　　　　　　dgc["执行任务名称"].Value.ToString(), dgc["执行任务单位"].Value.ToString(), 
                    　　　　　　　　　　　　　　　　dgc["指挥员肩咪ID"].Value.ToString(), dgc["备用肩咪ID"].Value.ToString(), 
                    　　　　　　　　　　　　　　　　dgc["参加任务人数"].Value.ToString() };

                frmExeGroup fmExe = new frmExeGroup(_conStr, false, _exeGroup);

                if (fmExe.ShowDialog() == DialogResult.OK)
                {
                    // 重新初始化任务组数据
                    DataSet objset = getMajorTasks();

                    this.dataExegroup.DataSource = objset.Tables[1];
                    this.changRowColor(this.dataExegroup);
                    insertExeGroupPoint();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tsmModify_Click");
            }
        }

        /// <summary>
        /// 修改重大任务
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-18
        /// </summary>
        private void tsmModifyMajor_Click(object sender, EventArgs e)
        {
            try
            {
                majorTask = new string[] { dataMajorTask.SelectedRows[0].Cells["任务编号"].Value.ToString(), 
                                           dataMajorTask.SelectedRows[0].Cells["任务名称"].Value.ToString(),
                                           dataMajorTask.SelectedRows[0].Cells["指挥员"].Value.ToString(),
                                           dataMajorTask.SelectedRows[0].Cells["任务描述"].Value.ToString()};

                frmMajorTask frmMaTa = new frmMajorTask(_conStr,false,majorTask);

                if (frmMaTa.ShowDialog() == DialogResult.OK)
                {
                    this.dataMajorTask.DataSource = frmMaTa.majorTable;
                    changRowColor(this.dataMajorTask);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tsmModifyMajor_Click");
            }
        }

        private string[] _restructMajor;

        /// <summary>
        /// 从用户选择中获得所有的任务组
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-21
        /// </summary>
        /// <param name="fgo">用户选择范围</param>
        private void GetRestructMajorTask(FeatureGeometry fgo)
        {
            try
            {
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("MajorTaskLayer") != null)
                {
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(fgo, ContainsType.Geometry);
                    si.QueryDefinition.Columns = null;

                    IResultSetFeatureCollection fc = MapInfo.Engine.Session.Current.Catalog.Search("MajorTaskLayer", si);

                    if (fc.Count <= 1)
                    {
                        MessageBox.Show("请选择二个或二个以上的任务组进行重组！", "系统提示",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
                        return;
                    }

                    _restructMajor = new string[fc.Count];
                    int i = 0;
                    foreach (Feature ft in fc)
                    {
                        _restructMajor[i] = ft["任务组编号"].ToString();
                        i++;
                    }

                    frmRestruct frmRest = new frmRestruct(_restructMajor, _conStr);
                    if (frmRest.ShowDialog() == DialogResult.OK)
                    {
                        // 重新初始化任务组数据
                        DataSet objset = getMajorTasks();

                        this.dataExegroup.DataSource = objset.Tables[1];
                        this.changRowColor(this.dataExegroup);
                        insertExeGroupPoint();
                    }
                    this.MapControl1.Tools.LeftButtonTool = "Select";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetRestructMajorTask");
            }
        }

        /// <summary>
        /// 重组重大任务
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-21
        /// </summary>
        private void tsmExeg_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                ExToLog(ex, "tsmExeg_Click");
            }
        }

        /// <summary>
        /// 重组任务组按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-21
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                MapControl1.Tools.LeftButtonTool = "SelectPolygon";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button4_Click");
            }
        }
    }
}