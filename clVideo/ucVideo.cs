
//********顺德公安项目-视频监控模块******
//********创建人：jie.zhang
//********创建日期： 2008.9.10
//********版本修改：
//********1. 2009.4.15  修改移动视频可查看，并能实时移动
//********2. 2009.5.8   修改视频图标大小，并移动视频放在顶层
//********3. 2009.5.13  修改查询移动视频无结果
//                      移动视频的标注与普通不同
//                      确定车辆都在视图范围内
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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;


using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;
using clPopu;


namespace clVideo
{
    public partial class ucVideo : UserControl
    {

        //jie.zhang 20090119  修改固定参数为静态变量
        #region
        static MapControl mapControl1 = null;
        static string[] StrCon;
        static string mysqlstr;
        public ToolStripDropDownButton tlvideo;
        static int videoport = 0;
        static string[] VideoConnectString; // CMS的连接字符串
        static ToolStripLabel StatusLabel = null;
        static int messageId = 0;
        static string VideoClient = string.Empty;
        static string UserRegion;   //jie.zhang 20090119
        #endregion

        public string VideoNm;// 选择时的视频的名称
        public string userid = "";
        public string datasource = "";
        public string password = "";
        public Boolean VideoFlag = false; // 通讯是否已经连通的标识符
        public string Videotblname = "VideoLayer";
        public string Videocolname = "Name";
        public String NowVideoName;//点击grid时所选中的视频名称  
        private IResultSetFeatureCollection rsfcflash;//闪烁的图元集合
        public Boolean GVFlag = false;//开始选择地点监控的标识
        private int iflash = 0;
        //public string getFromNamePath;// 存取表字段配置文件地址

        public Boolean[] Camlist;  //已打开的摄像头的mapid的数组

        public string strRegion = string.Empty;
        public string strRegion1 = "";
        public string user = "";
        
        public string strRegion2 = ""; // 可导出的派出所
        public string strRegion3 = ""; // 可导出的中队
        public string excelSql = "";   // 视频查询导出sql
        public int _startNo, _endNo;   // 可导出分页数

        public System.Data.DataTable dtExcel = null; //导出Excel表
        OracleDataAdapter apt1 = null;

        private static NetworkStream networkStream1 = null;
        public Boolean ZhiHui = false;

        public ToolStripProgressBar toolPro;  // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolProLbl;     // 用于显示进度文本　
        public ToolStripSeparator toolProSep;


        /// <summary>
        /// 传递全局参数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="m">地图控件</param>
        /// <param name="st">状态栏</param>
        /// <param name="tl">工具栏下拉菜单</param>
        /// <param name="s">SQL连接字符串</param>
        /// <param name="vport">连接端口</param>
        /// <param name="vs">连接参数组</param>
        /// <param name="exepath">监控程序所在目录</param>
        /// <param name="zh">直观指挥标识符</param>
        public ucVideo(MapInfo.Windows.Controls.MapControl m, ToolStripLabel st, System.Windows.Forms.ToolStripDropDownButton tl, string[] s, int vport, string[] vs, string exepath, Boolean zh,Boolean isEvent)
        {
          
                InitializeComponent();
                try
                {
                    mapControl1 = m;

                    StrCon = s;

                    mysqlstr = "data source =" + StrCon[0] + ";user id =" + StrCon[1] + ";password=" + StrCon[2];

                    videoport = vport;

                    VideoConnectString = vs;

                    StatusLabel = st;

                    VideoClient = exepath;

                    ZhiHui = zh;

                    //mapControl1.Tools.FeatureSelected -= new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);

                    //mapControl1.Map.ViewChangedEvent -= new ViewChangedEventHandler(MapControl1_ViewChanged);
                    //mapControl1.Tools.Used -= new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);

                    mapControl1.Tools.FeatureSelected += new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);

                    mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(MapControl1_ViewChanged);
                    mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);

                    tlvideo = tl;
                    if (isEvent)
                    {

                        tlvideo.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolvideo_DropDownItemClicked);
                    }

                    this.comboBox1.Items.Clear();
                    this.comboBox1.Items.Add("设备名称");
                    this.comboBox1.Items.Add("所属派出所");
                    this.comboBox1.Text = this.comboBox1.Items[0].ToString();

                }
                catch (Exception ex)
                {
                    ExToLog(ex, "01-构造函数传递全局参数");
                }           
        }


        double xx1 = 0;
        double yy1 = 0;
        double xx2 = 0;
        double yy2 = 0; 

        /// <summary>
        /// 地图点击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    switch (e.ToolName)
                    {
                        case "SelectRect":
                            if (mapControl1.Map.Scale < 5000) return;

                           

                            switch (e.ToolStatus)
                            {

                                case MapInfo.Tools.ToolStatus.Start:
                                    xx1 = e.MapCoordinate.x;
                                    yy1 = e.MapCoordinate.y;
                                    break;
                                case ToolStatus.End:
                                    xx2 = e.MapCoordinate.x;
                                    yy2 = e.MapCoordinate.y;

                                    if (xx1 != 0 && yy1 != 0 && xx2 != 0 && yy2 != 0)
                                    {
                                        if (xx1 > xx2)
                                        {
                                            double xx = xx1;
                                            xx1 = xx2;
                                            xx2 = xx;
                                        }

                                        if (yy1 > yy2)
                                        {
                                            double yy = yy1;
                                            yy1 = yy2;
                                            yy2 = yy;
                                        }

                                        string sqlcmd = string.Empty;

                                        if (UserRegion == string.Empty)
                                        {
                                            MessageBox.Show("您没有设置权限！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            return;
                                        }
                                        else if (UserRegion == "顺德区")
                                        {
                                            sqlcmd = "Select 设备编号,设备名称,所属派出所,日常管理人,设备ID,X,Y,STYID from 视频位置VIEW where X>" + xx1 + " and X<" + xx2 + " and Y>" + yy1 + " and Y < " + yy2 + " order by 设备编号 desc";
                                        }
                                        else
                                        {
                                            if (Array.IndexOf(UserRegion.Split(','), "大良") > -1)
                                            {
                                                UserRegion = UserRegion.Replace("大良", "大良,德胜");
                                            }
                                            sqlcmd = "Select 设备编号,设备名称,所属派出所,日常管理人,设备ID,X,Y,STYID from 视频位置VIEW  where 所属派出所 in ('" + UserRegion.Replace(",", "','") + "') and X>" + xx1 + " and X<" + xx2 + " and Y>" + yy1 + " and Y < " + yy2 + " order by 设备编号  desc";
                                        }

                                        DataTable dt = GetTable(sqlcmd);
                                        if (this.SpeVideoArray == null && dt != null)
                                            this.SpeVideoArray = dt.Clone();

                                        this.SpeVideoArray.Merge(dt,false);// jie.zhang 20101215 修改为添加数据除非从清除处删除 GetTable(sqlcmd);

                                        this.AddSpecVideoFtr();
                                    }

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
                ExToLog(ex, "Tools_Used");
            }       
        }

        /// <summary>
        /// 地图视图改变事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void MapControl1_ViewChanged(object sender, EventArgs e)
        {
            try
            {
                if (mapControl1.Map.Layers.Count == 0) return;

                if (this.Visible && mapControl1.Map.Scale < 5000)
                {
                    CreateLayer();                    
                }
                else if (this.Visible)
                {
                    if (mapControl1.Map.Layers[Videotblname] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoLayer");
                    }

                    if (mapControl1.Map.Layers["VideoLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoLabel");
                    }

                    if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoCarLayer");
                    }

                    if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoCarLabel");
                    }

                    AddSpecVideoFtr();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MapControl1_ViewChanged-32-地图视野发生变化时");
            }
        }



        /// <summary>
        /// 获取网络参数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ns">数据流</param>
        /// <param name="vf">连接成功标识</param>
        public void getNetParameter(NetworkStream ns,Boolean vf)
        {
            try
            {
                networkStream1 = ns;
                VideoFlag = vf;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getNetParameter-02-获取网络参数");
            }
        }

        public string videotablename = string.Empty;
        public string videocolumname = string.Empty;
        /// <summary>
        /// 获取表名和列名
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="vtn">表名</param>
        /// <param name="vcn">列名</param>
        public void getVideoparam(string vtn, string vcn)
        {
            try
            {
                videotablename = vtn;
                videocolumname = vcn;
            }
            catch (Exception ex) { ExToLog(ex, "getVideoparam-03-获取表名和列名"); }
        }

        private MapInfo.Data.MultiResultSetFeatureCollection mirfc = null;
        private MapInfo.Data.IResultSetFeatureCollection mirfc1 = null;
        /// <summary>
        /// 图元选择
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void Feature_Selected(object sender, MapInfo.Tools.FeatureSelectedEventArgs e)
        {
            if (this.Visible)
            {
                try
                {
                    this.mirfc = e.Selection;
                    if (this.mirfc.Count == 0)
                    {
                        return;
                    }
                    this.mirfc1 = this.mirfc[0];
                    if (this.mirfc1.Count == 0)
                    {
                        return;
                    }
                    Feature f = this.mirfc1[0];
                    if (f == null)
                    {
                        return;
                    }
                    if (f.Table.Alias == "VideoLayer")
                    {

                        MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;

                        FeatureLayer lyr;

                        lyr = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;

                        IResultSetFeatureCollection rsfcView = session.Selections.DefaultSelection[lyr.Table];

                        if (rsfcView != null)
                        {
                            if (rsfcView.Count > 0)
                            {
                                foreach (Feature ft in rsfcView)
                                {
                                    string ftename = ft["Name"].ToString();

                                    if (this.gvVideo.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < gvVideo.Rows.Count; i++)
                                        {
                                            if (gvVideo.Rows[i].Cells[0].Value.ToString() == ftename)
                                            {
                                                gvVideo.CurrentCell = gvVideo.Rows[i].Cells[0];
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }                              
                    }
                    else if (f.Table.Alias == "VideoCarLayer")
                    {
                        MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;

                        FeatureLayer lyr;

                        lyr = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;

                        IResultSetFeatureCollection rsfcView = session.Selections.DefaultSelection[lyr.Table];

                        if (rsfcView != null)
                        {
                            if (rsfcView.Count > 0)
                            {
                                foreach (Feature ft in rsfcView)
                                {
                                    string ftename = ft["Name"].ToString();

                                    if (this.gvVideo.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < gvVideo.Rows.Count; i++)
                                        {
                                            if (gvVideo.Rows[i].Cells[0].Value.ToString() == ftename)
                                            {
                                                gvVideo.CurrentCell = gvVideo.Rows[i].Cells[0];
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }    
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Feature_Selected-04-图元选择");
                }
            }
        }

        /// <summary>
        /// 更改模块区域权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                    UserRegion = GetPolice(strRegion1);

                }
                else if (strRegion != "" && strRegion1 != "")  // 有中队权限，也有派出所权限
                {
                    UserRegion = strRegion + "," + GetPolice(strRegion1);
                }
                else if (strRegion != "" && strRegion1 == "")
                {
                    UserRegion = strRegion;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetUserRegion-05-更改模块区域权限");
            }
        }


        /// <summary>
        /// 获取中队所在的派出所
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="s1">中队名称字符串</param>
        /// <returns>派出所名称</returns>
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
                ExToLog(ex, "GetPolice-06-获取中队所在的派出所");
            }
            return reg;
        }

        /// <summary>
        /// 创建视频图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        public void CreateVideoLayer()
        {
            try
            {
                VideoNm = "All";

                OpenVideoClient();

                CreateLayer();

                VideoAddGrid();


                //this.toolPro.Value = 5;
                //Application.DoEvents();

            }
            catch (Exception ex)
            {
                //isShowPro(false);
                ExToLog(ex, "CreateVideoLayer-07-创建视频图层");
            }
        }

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void CreateLayer()
        {
            try
            {
                if (tlvideo.Visible == false)
                    tlvideo.Visible = true;

                if (mapControl1.Map.Scale > 5000) return;

                if (mapControl1.Map.Layers.Count == 0) return;

                isShowPro(true);
                this.toolPro.Maximum = 6;
                StopVideo();

                string s = CLC.DatabaseRelated.OracleDriver.GetConString;

                AddVideoFtr();
                this.toolPro.Value = 3;
                Application.DoEvents();

                AddCarViedoFtr();
                this.toolPro.Value = 4;
                Application.DoEvents();

                if (mapControl1.Map.Layers["VideoCarLayer"] != null && mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    this.SetLayerSelect("VideoCarLayer", Videotblname);
                    this.GVFlag = true;
                }
                else
                {
                    isShowPro(false);
                    return;
                }

                if (this.GVFlag == false)
                    this.GVFlag = true;

                if (this.timer1.Enabled == false)
                {
                    this.timer1.Interval = 30 * 1000;
                    this.timer1.Enabled = true;
                    this.toolPro.Value = 6;
                }
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                //isShowPro(false);
                ExToLog(ex, "CreateLayer");
            }
        }


        private DataTable SpeVideoArray;
        private string SpeType;  //当前是选择了点还是框选

        /// <summary>
        /// 添加被选择或框选的视频点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void AddSpecVideoFtr()
        {
            try
            {
                if (this.SpeType == "") return;

                if (mapControl1.Map.Layers.Count == 0) return;

                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }
                if (SpeVideoArray.Rows.Count > 0)
                {

                    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                    //创建临时层
                    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoLayer");
                    Table tblTemp = Cat.GetTable("VideoLayer");
                    if (tblTemp != null) //Table exists close it
                    {
                        Cat.CloseTable("VideoLayer");
                    }

                    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("设备编号", 50));

                    tblTemp = Cat.CreateTable(tblInfoTemp);
                    FeatureLayer lyr = new FeatureLayer(tblTemp);
                    //mapControl1.Map.Layers.Add(lyr);
                    mapControl1.Map.Layers.Insert(0, lyr);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //添加标注
                    string activeMapLabel = "VideoLabel";
                    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");
                    MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                    MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                    lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                    lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                    lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                    lbsource.DefaultLabelProperties.Layout.Offset = 2;
                    lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                    lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                    lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                    lbsource.DefaultLabelProperties.Caption = "Name";
                    lblayer.Sources.Append(lbsource);
                    mapControl1.Map.Layers.Add(lblayer);

                    this.toolPro.Value = 2;
                    Application.DoEvents();


                    MapInfo.Mapping.Map map = mapControl1.Map;
                    MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;
                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");

                    //if(SpeType=="Square")
                    //{

                    foreach (DataRow dr in SpeVideoArray.Rows)
                    {
                        if (dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr["设备名称"].ToString() != "" && dr["设备编号"].ToString() != "")
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;

                            CompositeStyle cs = new CompositeStyle();

                            if (dr["STYID"].ToString() == "0")       //社会面视频
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }
                            else if (dr["STYID"].ToString() == "1")  //非社会面视频
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("TARG1-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }

                            Feature ftr = new Feature(tblcar.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = dr["设备名称"].ToString();
                            ftr["设备编号"] = dr["设备编号"].ToString();
                            tblcar.InsertFeature(ftr);
                        }
                    }
                }
                else if (SpeType == "Single")
                {

                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AddSpecVideoFtr-08-添加图元");
            }
        }

        /// <summary>
        /// 添加图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void AddVideoFtr()
        {
            try
            {
                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoLayer");
                Table tblTemp = Cat.GetTable("VideoLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("VideoLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("设备编号", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);
                this.toolPro.Value = 1;
                Application.DoEvents();

                //添加标注
                string activeMapLabel = "VideoLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                this.toolPro.Value = 2;
                Application.DoEvents();


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");

                string sqlcmd = string.Empty;

                if (UserRegion == string.Empty)
                {
                    MessageBox.Show("您没有设置权限！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (UserRegion == "顺德区")
                {
                    sqlcmd = "Select 设备编号,设备名称,所属派出所,日常管理人,设备ID,X,Y,STYID from 视频位置VIEW where X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2 + " order by 设备编号 desc";
                }
                else
                {
                    if (Array.IndexOf(UserRegion.Split(','), "大良") > -1)
                    {
                        UserRegion = UserRegion.Replace("大良", "大良,德胜");
                    }
                    sqlcmd = "Select 设备编号,设备名称,所属派出所,日常管理人,设备ID,X,Y,STYID from 视频位置VIEW  where 所属派出所 in ('" + UserRegion.Replace(",", "','") + "') and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2 + " order by 设备编号  desc";
                }

                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr["设备名称"].ToString() != "" && dr["设备编号"].ToString() != "")
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;

                            CompositeStyle cs = new CompositeStyle();

                            if (dr["STYID"].ToString() == "0")       //社会面视频
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }
                            else if (dr["STYID"].ToString() == "1")  //非社会面视频
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("TARG1-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }

                            Feature ftr = new Feature(tblcar.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = dr["设备名称"].ToString();
                            ftr["设备编号"] = dr["设备编号"].ToString();
                            tblcar.InsertFeature(ftr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AddVideoFtr-08-添加图元");
            }
        }

        /// <summary>
        /// 更新视频中车辆的经纬度
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="CamName">车辆编号</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        private void UpdateVideoCarGrid(string CamName, double x, double y)
        {
            try
            {
                if (CamName != "")
                {                    
                    if (this.gvVideo.Rows.Count > 0)
                    {
                        for (int i = 0; i < gvVideo.Rows.Count; i++)
                        {
                            if (gvVideo.Rows[i].Cells[0].Value.ToString() == CamName)
                            {
                                gvVideo.Rows[i].Cells[3].Value = x.ToString();
                                gvVideo.Rows[i].Cells[4].Value = y.ToString();
                                break;            
                            }                         
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "UpdateVideoCarGrid-09-更新视频中的车辆的经纬度");
            }
        }


        /// <summary>
        /// 添加移动视频图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void AddCarViedoFtr()
        {
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
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("设备编号", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);

                //添加标注
                string activeMapLabel = "VideoCarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");
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

                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");

                string sqlcmd = string.Empty;
                if (UserRegion == string.Empty)
                {
                    MessageBox.Show("没有设置区域权限", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (UserRegion == "顺德区")
                {
                    sqlcmd = "Select  设备编号,设备名称,所属派出所,日常管理人,设备ID,X,Y  from 视频位置VIEW where STYID='2'and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2;
                }
                else
                {
                    if (Array.IndexOf(UserRegion.Split(','), "大良") > -1)
                    {
                        UserRegion = UserRegion.Replace("大良", "大良,德胜");
                    }
                    sqlcmd = "Select 设备编号,设备名称,所属派出所,日常管理人,设备ID,X,Y  from 视频位置VIEW where STYID='2' and 权限单位 in ('" + UserRegion.Replace(",", "','") + "') and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2;
                }
                                
                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string camid = dr["设备编号"].ToString();
                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);
                        if (xv != 0 && yv != 0 && dr["X"].ToString() != "" && dr["Y"].ToString() != "")
                        {
                            if (camid.Length > 5)
                            {
                                try
                                {
                                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;

                                    CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("ydsp.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));
                                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                                    ftr.Geometry = pt;
                                    ftr.Style = cs;
                                    ftr["Name"] = dr["设备名称"].ToString();
                                    ftr["设备编号"] = dr["设备编号"].ToString();
                                    tblcar.InsertFeature(ftr);
                                }
                                catch (Exception ex) { ExToLog(ex, "10-插入图元"); }
                                UpdateVideoCarGrid(dr["设备名称"].ToString(), Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AddCarViedoFtr-10-添加移动视频图元");
            }
        }

        /// <summary>
        /// 判读车辆是否在可视范围
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns>布尔值（true-在可视范围 false-不在可视范围）</returns>
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
            catch (Exception ex)
            {
                ExToLog(ex, "IsInBounds-11-判断车辆是否在可视范围");
                return false;
            }
        }

        /// <summary>
        /// 设置图层可见--针对于已存在的图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="tablename">图层名称</param>
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
            catch(Exception ex)
            {
                ExToLog(ex, "SetTableVisable-12-设置图层可见");
            }
        }


       /// <summary>
        /// 设置图层不可见--针对于已存在的图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
       /// </summary>
       /// <param name="tablename">图层名称</param>
        public void SetTableDisable(string tablename)
        {
            try
            {
                this.GVFlag = false;
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
            catch( Exception ex )
            {
                ExToLog(ex, "SetTableDisable-13-设置图层不可见");
            }
        }

        /// <summary>
        /// 停止视频监控模块
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void StopVideo()
        {
            try
            {
                this.GVFlag = false;

                //tlvideo.Visible = false;

                if (mapControl1.Map.Layers[Videotblname] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }

                if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLayer");
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }

                if (this.timer1.Enabled == true)
                    this.timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "StopVideo-14-停止视频监控模块");
            }
        }

        /// <summary>
        /// 切换列表选择
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void gvVideo_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView grid = (DataGridView)sender;
                NowVideoName = grid.CurrentRow.Cells[0].Value.ToString();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "gvVideo_SelectionChanged-15-切换列表选择");
            }
        }

        /// <summary>
        /// 设置列表背景颜色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void gvVideo_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.gvVideo.Rows.Count != 0)
                {
                    for (int i = 0; i < this.gvVideo.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.gvVideo.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.gvVideo.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "gvVideo_DataBindingComplete-16-设置列表背景颜色");
            }
        }

        /// <summary>
        /// 添加数据到列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void VideoAddGrid()
        {
            if (VideoNm != "")
            {
                try
                {
                    string sql = string.Empty;
                    if (UserRegion == string.Empty)
                    {
                        MessageBox.Show("您没有设置权限！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (UserRegion == "顺德区")
                    {
                        if (VideoNm == "All")
                        {
                            sql = "select 设备名称,所属派出所,日常管理人,X,Y from 视频位置VIEW";
                        }
                        else
                        {
                            sql = "select  设备名称,所属派出所,日常管理人,X,Y  from 视频位置VIEW where ( 设备名称 = '" + VideoNm + "')";
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(UserRegion.Split(','), "大良") > -1 && UserRegion.IndexOf("德胜") < 0)
                        {
                            UserRegion = UserRegion.Replace("大良", "大良,德胜");
                        }

                        if (VideoNm == "All")
                        {
                            sql = "select 设备名称,所属派出所,日常管理人,X,Y from 视频位置VIEW where 所属派出所 in ('" + UserRegion.Replace(",", "','") + "') ";
                        }
                        else
                        {
                            sql = "select  设备名称,所属派出所,日常管理人,X,Y  from 视频位置VIEW where ( 设备名称 = '" + VideoNm + "' and 所属派出所 in  ('" + UserRegion.Replace(",", "','") + "')";
                        }
                    }

                    DataTable dt = new DataTable();

                    if (sql != "")
                    {
                        dt = GetTable(sql);
                    }
                    else
                    {
                        return;
                    }
                    #region 导出Excel
                    try
                    {
                        excelSql = sql;
                        excelSql = "select 设备编号,设备名称,所属派出所,日常管理人,设备ID, X,Y " + excelSql.Substring(excelSql.IndexOf("from"));
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
                        _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                        _endNo = pageSize1;

                        //OracleConnection orc = new OracleConnection(mysqlstr);
                        //orc.Open();
                        //OracleCommand cmd = new OracleCommand(excelSql, orc);
                        //apt1 = new OracleDataAdapter(cmd);
                        //DataTable datatableExcel = new DataTable();
                        //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                        //if (dtExcel != null) dtExcel.Clear();
                        //dtExcel = datatableExcel;
                        //cmd.Dispose();
                        //orc.Close();
                    }
                    catch { }
                    # endregion

                    lblcount.Text = "共有" + dt.Rows.Count.ToString() + "条记录";

                    if (dt != null)
                    {
                        Pagedt1 = dt;
                        InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "VideoAddGrid-17-将视频添加到列表");
                }
            }
        }

        /// <summary>
        /// 查询按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchVideo();           
        }

        /// <summary>
        /// 视频查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void SearchVideo()
        {
            if (this.comboBox1.Text != "")
            {
                 try
                {
                    string sql = string.Empty;
                    isShowPro(true);
                    if (UserRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show("您没有设置权限！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (UserRegion == "顺德区")
                    {
                        if (this.CaseSearchBox.Text == "")
                        {                     //终端ID,车辆牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间

                            sql = "select 设备名称,所属派出所,日常管理人,X,Y from 视频位置VIEW";
                        }
                        else
                        {
                            sql = "select  设备名称,所属派出所,日常管理人,X,Y from 视频位置VIEW where " + this.comboBox1.Text + " like  '%" + this.CaseSearchBox.Text + "%'"; 
                        }                       
                    }
                    else
                    {
                        if (Array.IndexOf(UserRegion.Split(','), "大良") > -1 && UserRegion.IndexOf("德胜") < 0)
                        {
                            UserRegion = UserRegion.Replace("大良", "大良,德胜");
                        }

                        if (this.CaseSearchBox.Text == "")
                        {                     //终端ID,车辆牌号,所属单位,当前任务,经度,纬度,速度,方向,导航状态,时间

                            sql = "select 设备名称,所属派出所,日常管理人,X,Y from 视频位置VIEW where 所属派出所 in ('" + UserRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            sql = "select 设备名称,所属派出所,日常管理人,X,Y from 视频位置VIEW where " + this.comboBox1.Text + " like  '%" + this.CaseSearchBox.Text + "%' and 所属派出所 in ('" + UserRegion.Replace(",", "','") + "')";
                        }                      
                    }

                    // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                    //if (sql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                    //    sql += " and (备用字段一 is null or 备用字段一='')";
                    //else
                    //    sql += " where (备用字段一 is null or 备用字段一='')";
                    //-------------------------------------------------------

                    DataTable dt  = GetTable(sql);

                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    #region 导出Excel
                    try
                    {
                        excelSql = sql;
                        excelSql = "select 设备编号,设备名称,所属派出所,日常管理人,设备ID, X,Y " + excelSql.Substring(excelSql.IndexOf("from"));
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
                        _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                        _endNo = pageSize1;
                        
                    //    // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                    //    //if (excelSql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                    //    //    excelSql += " and 备用字段一 is null or 备用字段一=''";
                    //    //else
                    //    //    excelSql += " where 备用字段一 is null or 备用字段一=''";
                    //    //-------------------------------------------------------

                    //    OracleConnection orc = new OracleConnection(mysqlstr);
                    //    orc.Open();
                    //    OracleCommand cmd = new OracleCommand(excelSql, orc);
                    //    apt1 = new OracleDataAdapter(cmd);
                    //    DataTable datatableExcel = new DataTable();
                    //    apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //    if (dtExcel != null) dtExcel.Clear();
                    //    dtExcel = datatableExcel;                  
                    }
                    catch { isShowPro(false); }
                    this.toolPro.Value = 2;
                    Application.DoEvents();
                    # endregion

                    lblcount.Text = "共有" + dt.Rows.Count.ToString() + "条记录";
                    Pagedt1 = dt;
                    InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                    WriteEditLog("视频查询", "视频位置View", sql, "查询视频");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    ExToLog(ex, "SearchVideo-18-视频查询");
                }
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
        /// 初始化分页控件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="lblcount">用于显示总记录的子控件</param>
        /// <param name="textNowPage">用于显示当前页的子控件</param>
        /// <param name="lblPageCount">用于显示总页数的子控件</param>
        /// <param name="bs">分页控件的数据源</param>
        /// <param name="bn">分页控件</param>
        /// <param name="dgv">显示的列表</param>
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
                ExToLog(ex, "20-InitDataSet1");
            }
        }

        /// <summary>
        /// 根据分页查询出数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="textNowPage">用于显示当前页的子控件</param>
        /// <param name="lblPageCount">用于显示总页数的子控件</param>
        /// <param name="bds">分页控件的数据源</param>
        /// <param name="bdn">分页控件</param>
        /// <param name="dgv">显示的列表</param>
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
                dataVideo = new DataTable();  // 放大数据用的DataTable

                bds.DataSource = dataVideo = dtTemp;
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
                ExToLog(ex, "20-LoadData1");
            }
        }

        /// <summary>
        /// 翻页功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
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

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
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
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
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
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
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
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
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
                ExToLog(ex, "bindingNavigator1_ItemClicked-21-翻页功能");
            }
        }

        /// <summary>
        /// 旨在设置每页显示的数据数量
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
               if (e.KeyChar.ToString() == "\r" && gvVideo.Rows.Count > 0)
                {
                    pageSize1 = Convert.ToInt32(TextNum1.Text);
                    pageCurrent1 = 1;   //当前转到第一页
                    PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    pageCount1 = (PagenMax1 / pageSize1);//计算出总页数
                    if ((PagenMax1 % pageSize1) > 0) pageCount1++;

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "22-TextNum1_KeyPress");
            }
        }

        /// <summary>
        /// 旨在实现页面转向
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
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
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);

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
                ExToLog(ex, "23-PageNow1_KeyPress");
            }
        }
       
        /// <summary>
        /// 列表双击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void gvVideo_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string ftrname = NowVideoName;

                OracleConnection orc11 = new OracleConnection(mysqlstr);
                orc11.Open();
                OracleCommand cmd11 = new OracleCommand("Select * from 视频位置VIEW where 设备名称='" + NowVideoName + "'", orc11);
                OracleDataAdapter apt11 = new OracleDataAdapter(cmd11);
                DataTable dt11 = new DataTable();
                apt11.Fill(dt11);
                if (dt11.Rows.Count > 0)
                {
                    this.SpeVideoArray = dt11;

                    double x = Convert.ToDouble(dt11.Rows[0]["X"]);
                    double y = Convert.ToDouble(dt11.Rows[0]["Y"]);
                    mapControl1.Map.SetView(new DPoint(x, y), mapControl1.Map.GetDisplayCoordSys(), getScale());

                    if (ftrname != "")
                    {
                        MapInfo.Mapping.Map map = mapControl1.Map;

                        if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                        {
                            return;
                        }

                        rsfcflash = null;

                        MapInfo.Data.MIConnection conn = new MIConnection();
                        conn.Open();

                        MapInfo.Data.MICommand cmd = conn.CreateCommand();
                        Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(Videotblname);
                        cmd.CommandText = "select * from " + mapControl1.Map.Layers[Videotblname].ToString() + " where " + Videocolname + " = @name ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@name", ftrname);

                        this.rsfcflash = cmd.ExecuteFeatureCollection();
                        MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();

                        if (this.rsfcflash.Count > 0)
                        {
                            //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            //foreach (Feature f in this.rsfcflash)
                            //{
                            //    mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                            //    mapControl1.Map.Center = f.Geometry.Centroid;
                            //    break;
                            //}
                        }
                        else
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "select * from " + mapControl1.Map.Layers["VideoCarLayer"].ToString() + " where " + Videocolname + "=@name";
                            cmd.Parameters.Add("@name", ftrname);
                            this.rsfcflash = cmd.ExecuteFeatureCollection();

                            if (this.rsfcflash.Count > 0)
                            {
                                //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                                //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                                //foreach (Feature f in this.rsfcflash)
                                //{
                                //    mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                //    mapControl1.Map.Center = f.Geometry.Centroid;
                                //    break;
                                //}
                            }
                        }

                        cmd.Clone();
                        conn.Close();

                        if (this.rsfcflash.Count < 1) return;
                        this.StartFlash();
                        WriteEditLog("视频查询", "视频位置View", "设备名称=" + NowVideoName, "双击视频");
                    }
                    else
                    {
                        MessageBox.Show("没有选择任何车辆！", "系统提示");
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "gvVideo_DoubleClick-23-在列表上双击");
            }
            finally
            {
                try { fmDis.Close(); }
                catch { }
            }
        }

        /// <summary>
        /// 获取缩放比例
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
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
                CLC.BugRelated.ExceptionWrite(ex, "getScale-获取缩放比例");
                return 0;
            }
        }

        /// <summary>
        /// 设置当前图层可选择
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="layername1">图层1的名称</param>
        /// <param name="layername2">图层2的名称</param>
        private void SetLayerSelect(string layername1, string layername2)
        {
            try
            {
                MapInfo.Mapping.Map map = mapControl1.Map;

                for (int i = 0; i < map.Layers.Count; i++)
                {
                    IMapLayer layer = map.Layers[i];
                    string lyrname = layer.Alias;

                    MapInfo.Mapping.LayerHelper.SetSelectable(layer, false);
                }
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername1], true);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername2], true);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetLayerSelect-24-设置图层可选择");
            }
        }

        /// <summary>
        /// 开启图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void StartFlash()
        {
            try
            {
                timLocation.Enabled = true;
            }
            catch (Exception ex) { ExToLog(ex, "StartFlash"); }
        }

        /// <summary>
        /// 图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void timLocation_Tick(object sender, EventArgs e)
        {
            try
            {
                iflash = iflash + 1;

                int i = iflash % 2;
                if (i == 0)
                {
                    try
                    {
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {                  

                    try
                    {
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                if (this.iflash % 10 == 0)
                {
                    timLocation.Enabled = false;
                }
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "timLocation_Tick-25-图元闪烁");
            }
        }

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void SetFullView()
        {
            try
            {
                Map map = mapControl1.Map;
                IMapLayerFilter lyrFilter = MapLayerFilterFactory.FilterByType(typeof(FeatureLayer));
                MapLayerEnumerator lyrEnum = map.Layers.GetMapLayerEnumerator(lyrFilter);
                map.SetView(lyrEnum);
                //mapControl1.Tools.LeftButtonTool = "Select";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetFullView-25-图元闪烁");
            }
        }

        /// <summary>
        /// 周边查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="dpt"></param>
        /// <param name="dis"></param>
        public void SearchVideoDistance(MapInfo.Geometry.DPoint dpt, Double dis,string moName)
        {
            try
            {
                if (moName == "直观指挥")  // 如果是直观指挥则去掉的地图缩放改变事件
                {
                    mapControl1.Map.ViewChangedEvent -= new ViewChangedEventHandler(MapControl1_ViewChanged);
                }

                StopVideo();
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //创建临时层
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoLayer");
                Table tblTemp = Cat.GetTable("VideoLayer");
                if (tblTemp != null) 
                {
                    Cat.CloseTable("VideoLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("设备编号", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Insert(0, lyr);

                //添加标注
                string activeMapLabel = "VideoLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

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
                MapInfo.Mapping.FeatureLayer lyrvideo = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblvideo = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");

                string[] camidarr = null;
                string sql = string.Empty;

                if (UserRegion == string.Empty)   //add by fisher in 09-12-08
                {
                    MessageBox.Show(@"您没有设置权限！", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (UserRegion == "顺德区")
                {
                   sql = "Select * from 视频位置VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                }
                else
                {
                    if (Array.IndexOf(UserRegion.Split(','), "大良") > -1 && UserRegion.IndexOf("德胜") < 0)
                    {
                        UserRegion = UserRegion.Replace("大良", "大良,德胜");
                    }
                    sql = "Select * from 视频位置VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and 所属派出所 in ('" + UserRegion.Replace(",", "','") + "')";
                }

                DataTable dt = GetTable(sql);
                if (dt.Rows.Count > 0)
                {

                    if (dt.Rows.Count > 32)
                    {
                        camidarr = new string[32];
                    }
                    else
                    {
                        camidarr = new string[dt.Rows.Count];
                    }

                    int i = 0;
                    try
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                          
                            string camid = dr["设备编号"].ToString();
                            if (camid != "" && dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr["设备名称"].ToString() != "")
                            {
                                FeatureGeometry pt = new MapInfo.Geometry.Point(lyrvideo.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                                CompositeStyle cs = new CompositeStyle();

                                if (this.ZhiHui == true)
                                {
                                    cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 12));
                                }
                                else
                                {
                                    cs = new CompositeStyle(new SimpleVectorPointStyle(46, System.Drawing.Color.Red, 20));
                                }
                                Feature ftr = new Feature(tblvideo.TableInfo.Columns);
                                ftr.Geometry = pt;
                                ftr.Style = cs;
                                ftr["Name"] = dr["设备名称"].ToString();
                                ftr["设备编号"] = dr["设备编号"].ToString();
                                tblvideo.InsertFeature(ftr);

                                if (i <= 31)
                                {
                                    camidarr[i] = camid;
                                    i = i + 1;
                                }                             
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExToLog(ex, "26-插入图元");                       
                    }
                }
                OpenVideo(camidarr);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SearchVideoDistance-26-周边查询");
            }
        }
     
        /// <summary>
        /// 获取messageid
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <returns></returns>
        public static int getMessageId()
        {
            if (messageId >= 65000)
                messageId = 0;
            return messageId++;
        }

        /// <summary>
        /// Socket发送数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="msg1"></param>
        /// <returns></returns>
        private bool SendMsg1(byte[] msg1)
        {
            foreach (byte i in msg1)
            {
                Console.Write(i);
            }

            try
            {
                networkStream1.Write(msg1, 0, msg1.Length);
                networkStream1.Flush();
                ShowDoInfo("发送成功");
                //writelog("数据发送成功" + Encoding.UTF8.GetString(msg1, 0, msg1.Length) + "  长度：" + msg1.Length.ToString());
            }
            catch (Exception ex)
            {
                ShowDoInfo("发送失败");
                ExToLog(ex, "SendMsg1-27-Socket发送数据");
                return false;               
            }
            return true;
        }

        /// <summary>
        /// 查看监控图像
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="camid"></param>
        /// <returns>布尔值（true-查看成功 false-查看失败）</returns>
        public bool OpenVideo(string camid)
        {
            try
            {
                // 组合camera参数的TLV
                byte[] ct = new byte[2];   //camera参数的Type   
                ct[1] = 9;
                ct[0] = 0;

                byte[] c_value = Encoding.UTF8.GetBytes(camid);  //camera参数的Value
                if (c_value.Length % 4 != 0)                     //判断是否为是4的倍数
                {
                    byte[] temp1 = new byte[c_value.Length];
                    for (int t1 = 0; t1 < c_value.Length; t1++)
                    {
                        temp1[t1] = c_value[t1];
                    }

                    c_value = new byte[temp1.Length + (4 - temp1.Length % 4)];
                    for (int ii = 0; ii < temp1.Length; ii++)
                    {
                        c_value[ii] = temp1[ii];
                    }
                }

                byte[] cl = new byte[2];       //camera参数的Length
                cl[1] = (byte)(4 + c_value.Length);
                cl[0] = Convert.ToByte((4 + c_value.Length) / 256);

                //包头
                byte[] v = new byte[1] { 1 };    //版本
                byte[] tp = new byte[1] { 2 };   //request
                byte[] cmd = new byte[1] { 9 };  //命令
                byte[] rs = new byte[1] { 0 };   // 结果
                byte[] tl = new byte[2];        //总长度
                tl[1] = (byte)(12 + c_value.Length);
                tl[0] = Convert.ToByte((12 + c_value.Length) / 256);

                byte[] s = new byte[2];       //sqence number 的第一个字节

                int tempMessageId = getMessageId();
                s[0] = Convert.ToByte(tempMessageId / 256);
                s[1] = Convert.ToByte(tempMessageId % 256);

                buffer = new byte[12 + c_value.Length];

                int i = 0;
                Array.Copy(v, 0, buffer, i, v.Length);  //版本号复制

                i += v.Length;
                Array.Copy(tp, 0, buffer, i, tp.Length);   //type复制

                i += tp.Length;
                Array.Copy(cmd, 0, buffer, i, cmd.Length); // 命令复制

                i += cmd.Length;
                Array.Copy(rs, 0, buffer, i, rs.Length); //结果复制

                i += rs.Length;
                Array.Copy(tl, 0, buffer, i, tl.Length); // 长度复制

                i += tl.Length;
                Array.Copy(s, 0, buffer, i, s.Length);  //squence number 复制

                i += s.Length;
                Array.Copy(ct, 0, buffer, i, ct.Length);  // camera type复制

                i += ct.Length;
                Array.Copy(cl, 0, buffer, i, cl.Length);  //camera  length复制

                i += cl.Length;
                Array.Copy(c_value, 0, buffer, i, c_value.Length); //camera value的复制       

                //this.timer1.Interval = 4000;
                //this.timer1.Enabled = true;
                //this.sendtime = 0;
                //Thread.Sleep(10000);

                Boolean b = SendMsg1(buffer);

                Thread.Sleep(1000);

                if (SendMsg1(buffer) == false)
                {
                    MessageBox.Show("查看图像失败，请确认监控客户端已经打开并且端口配置正确！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "OpenVideo-28-查看监控图像");
                return false;
            }
        }


        public byte[] buffer;
        /// <summary>
        /// 发送打开cameraid
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void OpenVideo(string[] camerarray)
        {
            try
            {
                // cameralistTLV

                byte[] ct = new byte[2];   //cameralist参数的Type   
                ct[1] = 9;
                ct[0] = 0;


                // cameralist 参数的value 

                byte[] c1_value = Encoding.UTF8.GetBytes(camerarray[0]);  //cameralist第一个Value

                byte[] c_value = new byte[c1_value.Length + (camerarray.Length - 1) * (c1_value.Length + 1)];

                int k = 0;

                Array.Copy(c1_value, 0, c_value, k, c1_value.Length);  // 将第一个cameraid复制到value中

                for (int j = 1; j < camerarray.Length; j++)   //从cameralist 的第二个值开始循环赋值
                {
                    byte[] temp_value = Encoding.UTF8.GetBytes("," + camerarray[j]);  // 

                    Array.Copy(temp_value, 0, c_value, c1_value.Length + temp_value.Length * (j - 1), temp_value.Length);  // 将第一个cameraid复制到value中
                }

                if (c_value.Length % 4 != 0)
                {
                    byte[] temp1 = new byte[c_value.Length];
                    for (int t1 = 0; t1 < c_value.Length; t1++)
                    {
                        temp1[t1] = c_value[t1];
                    }

                    c_value = new byte[temp1.Length + (4 - temp1.Length % 4)];

                    for (int ii = 0; ii < temp1.Length; ii++)
                    {
                        c_value[ii] = temp1[ii];
                    }
                }


                byte[] cl = new byte[2];       //cameralist参数的Length
                cl[1] = (byte)(4 + c_value.Length);
                cl[0] = Convert.ToByte((4 + c_value.Length)/256);

                //包头
                byte[] v = new byte[1] { 1 };    //版本
                byte[] tp = new byte[1] { 2 };   //request
                byte[] cmd = new byte[1] { 9 };  //命令
                byte[] rs = new byte[1] { 0 };   // 结果
                byte[] tl = new byte[2];        //总长度
                tl[1] = (byte)(12 + c_value.Length);
                tl[0] = Convert.ToByte((12 + c_value.Length)/256);


                byte[] s = new byte[2];       //sqence number 的第一个字节

                int tempMessageId = getMessageId();
                s[0] = Convert.ToByte(tempMessageId / 256);
                s[1] = Convert.ToByte(tempMessageId % 256);

                buffer = new byte[12 + c_value.Length];

                int i = 0;
                Array.Copy(v, 0, buffer, i, v.Length);  //版本号复制

                i += v.Length;
                Array.Copy(tp, 0, buffer, i, tp.Length);   //type复制

                i += tp.Length;
                Array.Copy(cmd, 0, buffer, i, cmd.Length); // 命令复制

                i += cmd.Length;
                Array.Copy(rs, 0, buffer, i, rs.Length); //结果复制

                i += rs.Length;
                Array.Copy(tl, 0, buffer, i, tl.Length); // 长度复制

                i += tl.Length;
                Array.Copy(s, 0, buffer, i, s.Length);  //squence number 复制

                i += s.Length;
                Array.Copy(ct, 0, buffer, i, ct.Length);  // cameralist type复制

                i += ct.Length;
                Array.Copy(cl, 0, buffer, i, cl.Length);  //cameralist  length复制

                i += cl.Length;
                Array.Copy(c_value, 0, buffer, i, c_value.Length); //cameralist value的复制

                Boolean b = SendMsg1(buffer);

                Thread.Sleep(1000);

                if (SendMsg1(buffer)==false)
                {
                    MessageBox.Show("查看视频图像失败，请检测监控客户端是否打开！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }               
            }
            catch(Exception ex)
            {
                ExToLog(ex, "OpenVideo-29-发送cameraid");
            }
        }

        /// <summary>
        /// 判读视频监控窗口进程是否已经打开
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <returns>布尔值(true-打开 false-关闭)</returns>
        public Boolean IsOpenVideoClient()
        {
            try
            {
                string MyModuleName = "surveillance1.exe";
                string MyProcessName = System.IO.Path.GetFileNameWithoutExtension(MyModuleName);
                Process[] MyProcesses = Process.GetProcessesByName(MyProcessName);
                if (MyProcesses.Length >= 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "IsOpenVideoClient-29-发送cameraid");
                return false;
            }
        }

        /// <summary>
        /// 启动监控端程序
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void OpenVideoClient()
        {
            try
            {             
                Boolean vf = IsOpenVideoClient();
                if (vf == true)
                {
                    OpenAgentClient();

                    System.Diagnostics.Process process = new System.Diagnostics.Process();

                    int index = VideoClient.LastIndexOf('\\');
                    if (index != -1)
                    {
                        string dir = VideoClient.Substring(0, index);
                        process.StartInfo.WorkingDirectory = dir;

                        string VideoArg = string.Empty;
                        for (int i = 0; i < VideoConnectString.Length; i++)
                        {
                            VideoArg += VideoConnectString[i];
                            VideoArg += " ";
                        }

                        process.StartInfo.Arguments = VideoArg;
                        process.StartInfo.FileName = VideoClient;
                        process.Start();
                    }
                    WriteEditLog("视频监控", "", VideoClient, "打开视频监控客户端");
                }
                else
                {
                    MessageBox.Show("视频监控端已启动！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动监控端程序失败，请检测配置管理中监控端设置！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ExToLog(ex, "OpenVideoClient-30-启动监控端程序");
            }
        }

        /// <summary>
        /// 打开CAgent.exe 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void OpenAgentClient()
        {
            try
            {
                KillProcess("CAgent.exe");

                System.Diagnostics.Process process = new System.Diagnostics.Process();

                string dir = "";
                dir = VideoClient.Remove(VideoClient.LastIndexOf("\\"));
                                
                process.StartInfo.WorkingDirectory = dir;
                process.StartInfo.FileName = dir + "\\CAgent.exe";
                process.Start();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "OpenAgentClient-31-启动Agent程序失败");
            }
        }

        /// <summary>
        /// 关闭进程
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ProcName">进程名称</param>
        private void KillProcess(string ProcName)
        {
            try
            {
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();

                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if (ProcName == myProcess.ProcessName)
                        myProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "KillProcess");
            }
        }


        /// <summary>
        /// 显示信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
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
                    StatusLabel.Text = "信息:" + str;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ShowDoInfo");
            }
        }
        delegate void DecshowMessage(string str);
       
        /// <summary>
        /// 
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {            
            AddCarViedoFtr(); 
        }

        private void ucVideo_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    this.StopVideo();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ucVideo_VisibleChanged");
            }
        }

        /// <summary>
        /// 清除视频监控的临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void ClearVideoTemp()
        {
            try
            {
                clearFeatures("VideoCarLayer");
                clearFeatures("VideoLayer");
                this.SpeVideoArray.Clear();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ClearVideoTemp-34-清除视频监控的临时图层");
            }
        }

        /// <summary>
        /// 清除图层图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="tabAlias">图层名</param>
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
                ExToLog(ex, "clearFeatures-35-清除图层图元");
            }
        }


        /// <summary>
        /// 视频监控参数设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void videoset()
        {
            try
            {
                frmvideoset fs = new frmvideoset();
                fs.txtfolder.Text = VideoConnectString[0];
                fs.txtip.Text = VideoConnectString[1];
                fs.txtport.Text = VideoConnectString[2];
                fs.txtusername.Text = VideoConnectString[3];
                fs.txtpswd.Text =VideoConnectString[4];
                fs.txtClient.Text = VideoClient.Trim();    

                fs.ShowDialog(this);

                if (fs.DialogResult == DialogResult.OK)
                {
                    VideoConnectString[0] = fs.txtfolder.Text.Trim();
                    VideoConnectString[1] = fs.txtip.Text.Trim();
                    VideoConnectString[2] = fs.txtport.Text.Trim();
                    VideoConnectString[3] = fs.txtusername.Text.Trim();
                    VideoConnectString[4] =  md5(fs.txtpswd.Text.Trim());
                    VideoClient = fs.txtClient.Text.Trim();

                    CLC.INIClass.IniPathSet(Application.StartupPath.Remove(Application.StartupPath.LastIndexOf("\\")) + "\\config.ini");
                    CLC.INIClass.IniWriteValue("视频网", "文件夹", VideoConnectString[0]);
                    CLC.INIClass.IniWriteValue("视频网", "ip", VideoConnectString[1]);
                    CLC.INIClass.IniWriteValue("视频网", "端口", VideoConnectString[2]);
                    CLC.INIClass.IniWriteValue("视频网", "用户名", VideoConnectString[3]);
                    CLC.INIClass.IniWriteValue("视频网", "密码", VideoConnectString[4]);

                    CLC.INIClass.IniWriteValue("视频", "客户端", VideoClient);  //jie.zhang 2010.1.4


                    CLC.INIClass.IniPathSet(Application.StartupPath + @"\ConfigBJXX.ini");
                    CLC.INIClass.IniWriteValue("视频监控", "密码", fs.txtpswd.Text.Trim());

                    MessageBox.Show("设置保存成功","系统提示",MessageBoxButtons.OK,MessageBoxIcon.None);
                }
                else if (fs.DialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "videoset-35-视频监控参数设置");

                MessageBox.Show("设置保存失败", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// MD5加密算法
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>加密后字符串</returns>
        public string md5(string str)
        {
            string cl = str;
            string pwd = string.Empty;
            try
            {
                MD5 md5 = MD5.Create();

                byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));

                for (int i = 0; i < s.Length; i++)
                {
                    pwd = pwd + s[i].ToString("x2");
                }
            }
            catch (Exception ex) { ExToLog(ex, "md5"); }
            return pwd;
        }

        /// <summary>
        /// 视频监控按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void toolvideo_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                string Selectext = e.ClickedItem.Text;

                switch (Selectext)
                {
                    case "设置":
                        videoset();
                        break;
                    case "客户端":
                        OpenVideoClient();
                        break;
                    case "统计报表":
                        FrmSheZhi frmsz = new FrmSheZhi();
                        Size size = new Size(337, 102);
                        frmsz.Size = size;
                        frmsz.strConn = mysqlstr;
                        frmsz.Show();
                        break;
                    case "框选":
                        mapControl1.Tools.LeftButtonTool = "SelectRect";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex) { ExToLog(ex, "toolvideo_DropDownItemClicked"); }
        }

        /// <summary>
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="sql">要执行的SQL</param>
        /// <returns>结果集</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 处理SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="sql"></param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }       

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clVideo-ucVideo-" + sFunc);
        }

        /// <summary>
        /// 记录操作记录 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="sModule">操作模块</param>
        /// <param name="tName">操作数据库表名</param>
        /// <param name="sql">操作sql语句</param>
        /// <param name="method">操作方法</param>
        private void WriteEditLog(string sModule, string tName, string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'视频监控:" + sModule + "','" + tName + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch
            {
            }
        }

        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            //try
            //{

            //    string keyword = this.CaseSearchBox.Text.Trim();
            //    string colfield = this.comboBox1.Text.Trim();
            //    string Tablename = string.Empty;

            //    if (keyword.Length < 1 || colfield.Length < 1) return;

            //    string strExp = string.Empty;

            //    switch (colfield)
            //    {
            //        case "所属派出所":
            //            strExp = "select " + colfield + " from " + Tablename;
            //            break;
            //        default:
            //            strExp = "select " + colfield + " from 视频位置VIEW t  where " + colfield + " like '" + keyword + "%' union all select " + colfield + " from 视频位置VIEW t  where " + colfield + " like '%" + keyword + "%' and " + colfield + " not like '" + keyword + "%' and " + colfield + " not like '%" + keyword + "' union all select " + colfield + " from 视频位置VIEW t where " + colfield + " like '%" + keyword + "'";
            //            break;
            //    }

            //    DataTable dt = GetTable(strExp);
            //    this.CaseSearchBox.GetSpellBoxSource(dt);

            //}
            //catch (Exception ex)
            //{
            //    ExToLog(ex, "35-点击显示下拉");
            //}
        }

        /// <summary>
        /// 自动匹配功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();

                if (colfield == "所属派出所")
                {
                    string strExp = "select distinct(派出所名) from 基层派出所 order by 派出所名" ;
                    DataTable dt = GetTable(strExp);
                    this.CaseSearchBox.GetSpellBoxSource(dt);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "CaseSearchBox_MouseDown-35-点击显示下拉");
            }
        }

        /// <summary>
        /// 文本框按键事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void CaseSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        SearchVideo();
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
        /// 最后编辑时间  2011-1-26
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
            catch(Exception ex)
            {
                ExToLog(ex, "isShowPro");
            }
        }


        frmrecord frecord = new frmrecord();

        /// <summary>
        /// 右击视频下载按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void menurecord_Click(object sender, EventArgs e)
        {
            try
            {
                string camid = GetIdFromName();

                this.DownRecord(camid);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "menurecord_Click");
            }
        }

        /// <summary>
        /// 视频下载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="Camid">视频ID</param>
        public void DownRecord(string Camid)
        {
            try
            {
                string rfolder = VideoConnectString[0];
                string rip = VideoConnectString[1];
                string rport = VideoConnectString[2];
                string rname = VideoConnectString[3];

                CLC.INIClass.IniPathSet(Application.StartupPath + @"\ConfigBJXX.ini");
                string rpsw = CLC.INIClass.IniReadValue("视频监控", "密码");
                if (rpsw == "")
                {
                    MessageBox.Show("请确认视频监控登录密码", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    frmvideoset fs = new frmvideoset();
                    fs.txtfolder.Text = VideoConnectString[0];
                    fs.txtip.Text = VideoConnectString[1];
                    fs.txtport.Text = VideoConnectString[2];
                    fs.txtusername.Text = VideoConnectString[3];
                    fs.txtpswd.BackColor = System.Drawing.Color.Red;
                    fs.txtClient.Text = VideoClient.Trim();

                    fs.ShowDialog(this);

                    if (fs.DialogResult == DialogResult.OK)
                    {
                        VideoConnectString[0] = fs.txtfolder.Text.Trim();
                        VideoConnectString[1] = fs.txtip.Text.Trim();
                        VideoConnectString[2] = fs.txtport.Text.Trim();
                        VideoConnectString[3] = fs.txtusername.Text.Trim();
                        VideoConnectString[4] = md5(fs.txtpswd.Text.Trim());
                        VideoClient = fs.txtClient.Text.Trim();

                        rpsw = fs.txtpswd.Text.Trim();

                        CLC.INIClass.IniWriteValue("视频监控", "密码", fs.txtpswd.Text.Trim());



                        CLC.INIClass.IniPathSet(Application.StartupPath.Remove(Application.StartupPath.LastIndexOf("\\")) + "\\config.ini");
                        CLC.INIClass.IniWriteValue("视频网", "文件夹", VideoConnectString[0]);
                        CLC.INIClass.IniWriteValue("视频网", "ip", VideoConnectString[1]);
                        CLC.INIClass.IniWriteValue("视频网", "端口", VideoConnectString[2]);
                        CLC.INIClass.IniWriteValue("视频网", "用户名", VideoConnectString[3]);
                        CLC.INIClass.IniWriteValue("视频网", "密码", VideoConnectString[4]);

                        CLC.INIClass.IniWriteValue("视频", "客户端", VideoClient);
                    }
                    else
                    {
                        return;
                    }
                }

                if (rfolder != "" && rip != "" && rport != "" && rname != "" && rpsw != "" && Camid != "")
                {
                    frecord.Visible = true;
                    frecord.TopMost = true;


                    frecord.Initial(rip, rport, rfolder, rname, rpsw, Camid, StrCon, user);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "DownRecord");
            }
        }

        /// <summary>
        /// 获取视频编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <returns>视频编号</returns>
        private string GetIdFromName()
        {
            try
            {
                DataTable dt = this.GetTable("Select 设备编号 from 视频位置view t where t.设备名称='" + this.NowVideoName + "'");
                if (dt.Rows.Count > 0)
                {
                    string camid = dt.Rows[0]["设备编号"].ToString();
                    return camid;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetIdFromName");
                return "";
            }
        }

        private DataTable dataVideo;
        private frmDisplay fmDis;

        /// <summary>
        /// 放大查看数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnEnal_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataVideo == null)
                {
                    System.Windows.Forms.MessageBox.Show("无数据展示，请选查询出数据后放大查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new frmDisplay(dataVideo);

                fmDis.dataGridDisplay.CellDoubleClick += this.gvVideo_DoubleClick;
                fmDis.dataGridDisplay.SelectionChanged += this.gvVideo_SelectionChanged;

                fmDis.Show();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnEnal_Click");
            }
        }

        /// <summary>
        /// 测试日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="funName">函数名</param>
        /// <param name="message">执行代码</param>
        private void WriteLog(string funName,string message)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\videoTestLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine("函数名：" + funName + "  　   执行代码：" + message);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            { ExToLog(ex, "WriteLog"); }
        }


        #region 双击事件，用来显示详细信息　已屏蔽
        //private void gvVideo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex == -1)
        //        return;
        //    try
        //    {
        //        DPoint dp = new DPoint();
        //        CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
        //        string sql = "select * from 视频位置 where 设备名称='" + gvVideo.Rows[e.RowIndex].Cells[0].Value.ToString() + "'";
        //        DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

        //        dp.x = Convert.ToDouble(dt.Rows[0]["X"]);
        //        dp.y = Convert.ToDouble(dt.Rows[0]["Y"]);
        //        if (dp.x == 0 || dp.y == 0)
        //        {
        //            System.Windows.Forms.MessageBox.Show("此对象未定位!");
        //            return;
        //        }

        //        System.Drawing.Point pt = new System.Drawing.Point();
        //        pt.X = Convert.ToInt32(dp.x);
        //        pt.Y = Convert.ToInt32(dp.y);

        //        disPlayInfo(dt, pt);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLC.BugRelated.ExceptionWrite(ex, "gvVideo_CellContentDoubleClick");
        //    }
        //}

        //private FrmInfo frmMessage = new FrmInfo();
        //private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        //{
        //    try
        //    {
        //        if (this.frmMessage.Visible == false)
        //        {
        //            this.frmMessage = new FrmInfo();
        //            frmMessage.SetDesktopLocation(-30, -30);
        //            frmMessage.Show();
        //            frmMessage.Visible = false;
        //        }
        //        frmMessage.mapControl = mapControl1;
        //        frmMessage.getFromNamePath = getFromNamePath;
        //        frmMessage.strConn = mysqlstr;
        //        frmMessage.setInfo(dt.Rows[0], pt);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Console.WriteLine(ex.Message);
        //    }
        //}
        #endregion
    }
}