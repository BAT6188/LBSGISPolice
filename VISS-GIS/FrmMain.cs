//================================
//-------------------
//***项目名称：alcert与Mapinfo接口程序
//***开发单位：上海数字位图
//***开始时间：2008.11.1
//***开发人员：jie.zhang
//***维护记录：
//-------------------
//================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Data.OracleClient;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;

using MapInfo.Data;
using MapInfo.Geometry;
using MapInfo.Engine;
using MapInfo.Mapping;
using MapInfo.Styles;
using MapInfo.Windows.Dialogs;
using MapInfo.Tools;

namespace LBSgisPolice110
{
    public partial class FrmMain : Form 
    {
        //oracle 数据库
        static int messageId = 0;
        private  OracleConnection con;
        
        
        private string userid = "";          // ORacle 数据库的用户名
        private string datasource = "";      // oracle数据库的数据源 
        private string password = "";        // oracle 数据库的用户名密码
                   
        public int Listenport = 0;              // 管理客户端监听端口
        
        private int ListenportVideo = 0;        //监听客户端监听端口

        public bool showMap=false;              // 是否显示地图
        
        public string MapName = "";             // 地图名称

        public string VideoExePath = "";        //  视频监控客户端启动位置

        private double distan = 500;              //周边查询半径

        private string strRegion = string.Empty;

        private string[] VideoConnect;

        private LayerControlDlg _lcd;
        private string _videoTableName = "VideoLayer";
        private string _areaTableName = "街镇面";

        //private string vf = "";
        public  const int PackageHeaderLength=8;

        public Boolean VideoFlag = false;

        public string Systemstatus = string.Empty;


        public FrmMain(string[] c)
        {
            InitializeComponent();            
            this.VideoConnect = c;

            writelog(c.Length.ToString());

            for (int i = 0; i < c.Length;i++ )
            {
                writelog(c[i].ToString());
            }           
        }

        public static int getMessageId()
        {
           if(messageId>=65000)
               messageId=0;
            return messageId++;
        }


        /// <summary>
        /// 十六进制转换为字符
        /// </summary>
        /// <param name="HexCode"></param>
        /// <returns></returns>
        private string HexToString(String HexCode)
        {
            string sResult = "";
            for (int i = 0; i < HexCode.Length / 4; i++)
            {
                sResult += (char)short.Parse(HexCode.Substring(i * 4, 4), global::System.Globalization.NumberStyles.HexNumber);
            }
            return sResult;
        }

       
        private void ReadXML()
        {
            try
            {
                string s = "";
                string path = "";
                path = Application.StartupPath + "\\Config.XML";
                XmlDocument doc = new XmlDocument();

                doc.Load(path );

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
                                password =HexToString(reader.Value);
                            }
                            if (s.Equals("Listenport"))
                            {
                                this.Listenport = Convert.ToInt16(reader.Value);
                            }
                            if (s.Equals("ListenportVideo"))
                            {
                                this.ListenportVideo  = Convert.ToInt16(reader.Value);
                            }
                            if (s.Equals("Map"))
                            {
                                this.MapName = reader.Value;
                            }
                            if (s.Equals("showMap"))
                            {
                                this.showMap = Convert.ToBoolean(reader.Value);
                            }
                            if (s.Equals("dist"))
                            {
                                this.distan  = Convert.ToDouble(reader.Value);
                            }
                            if (s.Equals("VideoExePath"))
                            {
                                this.VideoExePath = reader.Value;
                            }
                            if (s.Equals("zone"))
                            {
                                this.strRegion = reader.Value;
                            }
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                writelog(ex.Message); 
            }
        }

        
        //初始化程序
        private void FrmMain_Load(object sender, EventArgs e)
        {            
            LoadApp();       
        }


        private void LoadApp()
        {
            //StopThread();
            try
            {
                this.panTip.Visible = false;
                ReadXML();
                this.Visible = this.showMap;
            }
            catch(Exception ex)
            {
                
                writelog("配置文件读取错误:"+ex.Message);
                ShowDoInfo("配置文件读取错误");
            }
            try
            {
                ConOracle();         //建立数据库连接
            }
            catch(Exception ex)
            {
                writelog("数据库:未能成功连接到数据库服务器."+ex.Message);
                ShowDoInfo("未能成功连接到数据库服务器");
            }
            try
            {
                OpenWMS();           //建立地图连接
            }
            catch(Exception ex)
            {
                writelog("地图;未能成功初始化地图.."+ex.Message);
                ShowDoInfo("未能成功初始化地图");
            }

            try
            {
                CreateManageSocket();         //建立管理客户端链接
                CreateVideoSocket();          //建立监控客户端连接

                this.toolstatus.Text = "管理端口：" + this.Listenport.ToString() + " 监控端口：" + this.ListenportVideo.ToString();
            }
            catch(Exception ex)
            {
                writelog("网络:未能成功建立监听."+ex.Message);
                ShowDoInfo("网络:未能成功建立监听");
            }  
        }


        //----------======-----------
        //建立管理客户端网络监听
        //===========================

        private TcpListener tcpserver;       
        private System.Net.Sockets.TcpListener tcpListener;
        private System.Threading.Thread ServerThread;

        //停止服务标志    
        public bool Stop;
        private TcpClient tcpClient;
        private bool TcpClose;  
        public System.Threading.Thread tcpClientThread;
        //启动客户连接线程  

        public void CreateManageSocket()
        {
            try
            {    //取主机名    
                string host = Dns.GetHostName();
                //解析本地IP地址，    
                IPHostEntry hostIp = Dns.GetHostByName(host);               
                writelog(string.Format("服务器: {0}", host));
                writelog("服务器地址:" + hostIp.AddressList[0].ToString());
            }
            catch (Exception x)
            {               //如果这个时候都出错，就别想活了   ，还是退出    
                writelog(x.Message);
                    return ;
            }

            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                tcpserver = new TcpListener(ip, this.Listenport);
                StartServer();
            }
            catch (Exception x)
            {
                this.writelog(x.Message);
            }    
        }


        // ThreadServer
        //===================    

        public void StartServer()
        {
            try
            {
                //Control.CheckForIllegalCrossThreadCalls = false;

                if (this.ServerThread != null)
                {
                    //("线程已经运行......");
                    return;
                }
                writelog(string.Format("New TcpListener....Port={0}", this.tcpserver.LocalEndpoint.ToString()));


                tcpserver.Start();
                //生成线程，start（）函数为线程运行时候的程序块    
                this.ServerThread = new Thread(new ThreadStart(startsever));
                ////线程的优先级别    
                //this.ServerThread.Priority = ThreadPriority.BelowNormal;
                this.ServerThread.Start();
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
        }


        //结束该服务线程，同时要结束由该服务线程所生成的客户连接    
        public void Abort()
        {
            writelog("正在关闭所有客户连接......");
            //调用客户线程的静态方法，结束所以生成的客户线程    
            //ThreadClientProcessor.AbortAllClient();
            writelog(string.Format("关闭服务线程: {0}", this.tcpserver.LocalEndpoint.ToString()));
            this.Stop = true;
            //结束本服务启动的线程    
            if (this.ServerThread != null) ServerThread.Abort();
            tcpListener.Stop();
            //程序等500毫秒    
            Thread.Sleep(TimeSpan.FromMilliseconds(500d));
        }

        private void startsever()
        {
            try
            {
                while (!Stop)   //几乎是死循环    
                {
                    writelog("正在等待连接......");
                    tcpClient = tcpserver.AcceptTcpClient();
                    writelog("已经建立连接......");
                    ShowDoInfo("已与客户端建立连接");
                    tcpClientThread = new Thread(new ThreadStart(startclient));
                    tcpClientThread.Start();
                    if (Stop) break;
                }
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
            finally
            {
                this.tcpserver.Stop();
            }
        }


        // ThreadClient  
        //===================
    
        public void clientAbort()
        {
            if (this.tcpClientThread != null)
            {
                //tcpClientThread.Interrupt();    
                tcpClientThread.Abort();
                //一定要等一会儿，以为后边tcpClient.Close（）时候，会影响NetWorkStream的操作    
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                tcpClient.Close();
            }
        }


        private NetworkStream networkStream;
        private  byte[] buf = new byte[1024 * 1024];   //预先定义1MB的缓冲  
        private  int Len = 0;   //流的实际长度  

        //读写连接的函数，用于线程    
        private void startclient()
        {
            networkStream = tcpClient.GetStream();   //建立读写Tcp的流    
            try
            {
                //开始循环读写tcp流    
                while (!TcpClose)
                {
                    //如果当前线程是在其它状态，（等待挂起，等待终止.....)就结束该循环    
                    if (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.Running)
                        break;

                    //判断Tcp流是否有可读的东西    
                    if (networkStream.DataAvailable)
                    {
                        //从流中读取缓冲字节数组    
                        Len = networkStream.Read(buf, 0, buf.Length);
                        //转化缓冲数组为串    
                        byte[] temp = new byte[Len];

                        for (int i = 0; i < Len; i++)
                        {
                            temp[i] = buf[i];

                        }
                        // buf = temp;
                        // string cmd = Encoding.UTF8.GetString(buf, 0, Len);
                        //  writelog("接收：" + cmd);
                        //  toolstausContent.Text = cmd;//是 字节
                        ShowDoInfo("接收数据成功！");
                        AnalyData(temp);//解析收到的命令                           
                    }
                    else
                    {
                        //Thread.Sleep(TimeSpan.FromMilliseconds(200d));    
                        //this.MessageList.Items.Add("客户机无命令");    
                        //如果当前Tcp连接空闲，客户端没有写入，则当前线程停止200毫秒
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
                networkStream.Close();
                tcpClient.Close();
                writelog("关闭连接......");
            }
        }

        private bool SendMsg(byte[] msg)
        {
          foreach (byte i in msg)
          {
              Console.Write(i);
          }
            try
            {
                networkStream.Write(msg, 0, msg.Length);
            
                networkStream.Flush();

                ShowDoInfo("数据发送成功");

                writelog(Systemstatus + "成功");

                SetToolEnable();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                writelog(Systemstatus+"失败"+e.Message);
                ShowDoInfo("数据发送失败");
                return false;
                //writelog("发送:" + Encoding.UTF8.GetString(msg,0,msg.Length));
            }
            return true;
        }

        //建立数据库连接
        private void ConOracle()
        {
            try
            {
                string mysqlstr = "user id =" + userid + " ;data source = " + datasource + ";password =" + password;
                con = new OracleConnection(mysqlstr);
                con.Open();
            }
            catch(Exception ex)
            {
                writelog("数据库：没有连接到数据库服务器"+ex.Message);
                con.Close();
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        //初始化地图
        private void OpenWMS()
        {

            MapToolInit();
            try
            {
                string mapLoadPath = this.MapName;
                mapControl1.Map.Load(new MapWorkSpaceLoader(mapLoadPath));


                IMapLayer mapLayer0 = mapControl1.Map.Layers["主道路"];
                int ilayer0 = mapControl1.Map.Layers.IndexOf(mapLayer0);
                mapControl1.Map.Layers.Move(ilayer0, 3);

                IMapLayer mapLayer = mapControl1.Map.Layers["次道路"];
                int ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                mapControl1.Map.Layers.Move(ilayer, 3);

                this.mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["街镇面"]);

            }
            catch
            {
                MessageBox.Show("加载地图时发生错误,请在配置中选择地图后重新启动软件。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //return;
            }

            CreateVideoLayer();//创建视频监控图层
        }


        int iMapLevel = 1;

        //设置缩放图标比例
        private void Map_ViewChangedEvent(object sender, MapInfo.Mapping.ViewChangedEventArgs e)
        {

            //========================读取比例，设置影像图
            try
            {
                iMapLevel = setToolslevel();   //显示级别

                //如果看影像，显示影像
                if (toolimage.Text == "地图")
                {
                    closeOtherLevelImg(iMapLevel);//先关闭其他级别的影像

                    CalRowColAndDisImg(iMapLevel);
                }
            }
            catch
            {
            }

            try
            {
                Double dblZoom = System.Convert.ToDouble(String.Format("{0:E2}", mapControl1.Map.Zoom.Value));
                if (this.mapControl1.Map.Layers[_videoTableName] != null)
                {
                    if (dblZoom < 5.76)
                    {
                        //mapControl1.Map.Zoom = new Distance(5.76, this.mapControl1.Map.Zoom.Unit);
                    }
                    if (dblZoom > 5.76 && dblZoom <= 11.5)
                    {
                        SetFtrSize(20);
                    }
                    if (dblZoom > 11.5 && dblZoom <= 23)
                    {
                        SetFtrSize(16);
                    }
                    if (dblZoom > 23 && dblZoom <= 46)
                    {
                        SetFtrSize(12);
                    }
                    if (dblZoom > 46 && dblZoom <= 92)
                    {
                        SetFtrSize(8);
                    }
                    if (dblZoom > 92 && dblZoom <= 184)
                    {
                        SetFtrSize(4);
                    }
                    if (dblZoom > 184)
                    {
                       // mapControl1.Map.Zoom = new Distance(184, this.mapControl1.Map.Zoom.Unit);
                    }
                }
                if (statusStrip.Items.Count > 0)
                {
                    toolStripStatusLabelView.Text = "缩放: " + dblZoom.ToString() + " " + MapInfo.Geometry.CoordSys.DistanceUnitAbbreviation(mapControl1.Map.Zoom.Unit);
                    this.toolStripStatusMapCenter.Text = "地图中心:" + GetCenter(mapControl1.Map.Center);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace);}
        }

        //改变图标的大小
        public void SetFtrSize(double i)
        {
            try
            {
                //ISession session = MapInfo.Engine.Session.Current;
                //MapInfo.Data.Table tbl = session.Catalog.GetTable(_videoTableName);

                //MapInfo.Data.SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchAll();
                //MapInfo.Data.IResultSetFeatureCollection fc = session.Catalog.Search(tbl.Alias, si);               

                //SimpleVectorPointStyle ps = new SimpleVectorPointStyle(59, System.Drawing.Color.Green, i);
                //if (fc.Count != 0)
                //{
                //    foreach (Feature f in fc)
                //    {
                //        f.Style = ps;
                //        f.Update();
                //    }
                //}

                FeatureLayer lyr = this.mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;

                lyr.Modifiers.Clear();

                MapInfo.Styles.BasePointStyle pstyle = new MapInfo.Styles.SimpleVectorPointStyle(59, System.Drawing.Color.Green, i);

                MapInfo.Styles.CompositeStyle comstyle = new MapInfo.Styles.CompositeStyle();

                comstyle.SymbolStyle = pstyle;

                MapInfo.Mapping.FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null,comstyle);

                lyr.Modifiers.Append(fsm);              
            }
            catch
            { }
        }


        private string GetCenter(MapInfo.Geometry.DPoint dp)
        {
            try
            {
                foreach (Feature f in Session.Current.Catalog[_areaTableName])
                {
                    if (f.Geometry.ContainsPoint(dp))
                    {
                        return f["Name"].ToString();
                    }
                }
            }catch(Exception ee)
            {

                Console.WriteLine(ee.Message);
            }
            return "";
        }

        private void MapToolInit()
        {

            MapInfo.Tools.MapTool ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
            ptMapTool.UseDefaultCursor = false;
            ptMapTool.Cursor = Cursors.Cross;
            this.mapControl1.Tools.Add("VideoDistance", ptMapTool);

            ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
            ptMapTool.UseDefaultCursor = false;
            ptMapTool.Cursor = Cursors.Arrow;
            this.mapControl1.Tools.Add("None", ptMapTool);



            ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
            ptMapTool.UseDefaultCursor = false;
            ptMapTool.Cursor = Cursors.Cross;
            this.mapControl1.Tools.Add("Video", ptMapTool);


            ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
            ptMapTool.UseDefaultCursor = false;
            ptMapTool.Cursor = Cursors.Arrow;
            this.mapControl1.Tools.Add("Add", ptMapTool);


            ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
            ptMapTool.UseDefaultCursor = false;
            ptMapTool.Cursor = Cursors.Arrow;
            this.mapControl1.Tools.Add("Edit", ptMapTool);

            ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
            ptMapTool.UseDefaultCursor = false;
            ptMapTool.Cursor = Cursors.Arrow;
            this.mapControl1.Tools.Add("Del", ptMapTool);

            this.mapControl1.Map.ViewChangedEvent += new MapInfo.Mapping.ViewChangedEventHandler(Map_ViewChangedEvent);
            this.mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);
        }

        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {

                switch (e.ToolName)
                {

                    case "Add":

                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start: 
                                 AddCamera(e.MapCoordinate);
                                 break;
                            default:
                                break;
                        }
                        break;
                       
                    case "Edit":
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start: 
                                 EditCamera(e.MapCoordinate);                                  
                                 break;
                            default:
                                break;
                        }
                        break;
 
                    case "Del":
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start: 
                                    DelCamera(e.MapCoordinate);
                                    break;
                            default:
                                break;
                        }
                        break;
                        
                    case "Video":

                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start: 
                                     ShowVideo(e.MapCoordinate);
                                     break;
                            default:
                                break;
                        }

                        break;
                    case "VideoDistance":
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start: 
                                     ShowVideo(e.MapCoordinate);
                                     break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            
        }
        
        /// <summary>
        /// <函数：Decode>
        ///作用：将16进制数据编码转化为字符串，是Encode的逆过程
        /// </summary>
        /// <param name="strDecode"></param>
        /// <returns></returns>
        public string Decode(string strDecode)
        {
            string sResult = "";
            for (int i = 0; i < strDecode.Length / 4; i++)
            {
                sResult += (char)short.Parse(strDecode.Substring(i * 4, 4), global::System.Globalization.NumberStyles.HexNumber);
            }
            return sResult;
        }

        /// <summary>
        // 显示信息
        /// </summary>
        private void ShowDoInfo(string str)
        {
            if (this.statusStrip.InvokeRequired)
            {
                DecshowMessage dc = new DecshowMessage(ShowDoInfo);
                this.BeginInvoke(dc, new object[] { str });

            }
            else
                this.toolStripStatusLabelDo.Text = str;
        }
        delegate void DecshowMessage(string str);
        
        #region TOOLBAR
        /// <summary>
        // 工具条切换
        /// </summary>
        private void toolStripToolBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            object objTag = e.ClickedItem.Tag;
            string strTag = "";
            if (objTag == null)
                return;
            else
                strTag = objTag.ToString();
            switch (strTag)
            {
                case "Open":
                    OpenTable();
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "Layer":
                    if (_lcd == null)
                        _lcd = new LayerControlDlg();
                    _lcd.Map = mapControl1.Map;
                    if (this.TopMost == true)
                        _lcd.TopMost = true;
                    _lcd.ShowDialog();
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "ZoomIn":
                    mapControl1.Tools.LeftButtonTool = "ZoomIn";
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "ZoomOut":
                    mapControl1.Tools.LeftButtonTool = "ZoomOut";
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "Pan":
                    mapControl1.Tools.LeftButtonTool = "Pan";
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "GeoCoding":
                    ShowGeoCodingDialog();
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "tsbVideo":
                    StopThread();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    this.Close();
                    break; 
                case "FullMap":
                    mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["街镇面"]);
                    ToolBarCheck(e.ClickedItem);
                    break;
                case　"toolradius":
                    OpenRadius();
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "select":
                    mapControl1.Tools.LeftButtonTool = "Arrow";
                    ToolBarCheck(e.ClickedItem);
                    break;
                case "toolimage":
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
                    }
                    ToolBarCheck(e.ClickedItem);
                    break;
                default:
                    break;
            }
        }

        private void OpenRadius()
        {
            FrmGeoCoding  frmRad = new FrmGeoCoding();
            frmRad.disRadius = this.distan;
            if (frmRad.ShowDialog(this) == DialogResult.OK)
            {
                this.distan = frmRad.disRadius;
            }
            else
            {               
                return;
            }
        }


        private void StopThread()
        {
            try
            {
                CreatXML();

                if (ServerThread != null)
                {
                    ServerThread.Abort();
                }

                if (tcpClientThread != null)
                {
                    tcpClientThread.Abort();
                }

                if (tcpClient != null)
                {
                    tcpClient.Close();
                }

                if (tcpserver != null)
                {
                    tcpserver.Stop();
                }

                if (tcpListener != null)
                {
                    tcpListener.Stop();
                }




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



                Application.ExitThread();
                this.Dispose();
            }
            catch (Exception ex)
            { writelog(ex.Message); }
        }


        private void CreatXML()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(Application.StartupPath + @"\Config.XML");

                XmlNodeList nodeList = xmlDoc.SelectSingleNode("Con").ChildNodes;

                foreach (XmlNode mynode in nodeList)
                {
                    if (mynode.Name == "dist")
                    {
                        mynode.InnerText = this.distan.ToString();
                    }
                 }
                xmlDoc.Save(Application.StartupPath + @"\Config.XML");//保存。
            }
            catch (System.Exception e)
            {                
                writelog("创建文件失败" + e.Message);
                return;
            }
        }




        private void AddCamera(MapInfo.Geometry.DPoint dpt)
        {
            try
            {

                Systemstatus = "添加摄像机";
                // 发送 socket

                // 组合X参数的TLV
                byte[] xt = new byte[2];   //坐标x的type    
                xt[0] = 0;
                xt[1] = 3;
                byte[] x_value = BitConverter.GetBytes(dpt.x);  //x的坐标值
                if (x_value.Length % 4 != 0)                    //判断是不是4的倍数，不是补0 
                {
                    byte[] temp1 = new byte[x_value.Length];
                    for (int t1 = 0; t1 < x_value.Length; t1++)
                    {
                        temp1[t1] = x_value[t1];
                    }
                    x_value = new byte[temp1.Length + (4 - temp1.Length % 4)];
                    for (int t2 = 0; t2 < x_value.Length; t2++)
                    {
                        x_value[t2] = 0;
                    }

                    for (int ii = 0; ii < temp1.Length; ii++)
                    {
                        x_value[ii] = temp1[ii];
                    }
                }

                byte[] xl = new byte[2];
                xl[1] = (byte)(4 + x_value.Length);
                xl[0] = Convert.ToByte((4 + x_value.Length) / 256);

                // 组合Y参数的TLV
                byte[] yt = new byte[2];   //Y参数的Type   
                yt[0] = 0;
                yt[1] = 4;

                byte[] y_value = BitConverter.GetBytes(dpt.y);  //Y参数的Value
                if (y_value.Length % 4 != 0)                     //判断是否为是4的倍数
                {
                    byte[] temp1 = new byte[y_value.Length];
                    for (int t1 = 0; t1 < y_value.Length; t1++)
                    {
                        temp1[t1] = y_value[t1];
                    }
                    y_value = new byte[temp1.Length + (4 - temp1.Length % 4)];
                    for (int t2 = 0; t2 < y_value.Length; t2++)
                    {
                        y_value[t2] = 0;
                    }

                    for (int ii = 0; ii < temp1.Length; ii++)
                    {
                        y_value[ii] = temp1[ii];
                    }
                }
                byte[] yl = new byte[2];       //Y参数的Length
                yl[1] = (byte)(4 + y_value.Length);
                yl[0] = Convert.ToByte((4 + y_value.Length) / 256);


                //包头
                byte[] v = new byte[1] { 1 };    //版本
                byte[] tp = new byte[1] { 2 };   //request
                byte[] cmd = new byte[1] { 3 };  //命令
                byte[] rs = new byte[1] { 0 };   // 结果
                byte[] tl = new byte[2];        //总长度
                tl[1] = (byte)(16 + x_value.Length + y_value.Length);
                tl[0] = Convert.ToByte((16 + x_value.Length + y_value.Length) / 256);


                byte[] s = new byte[2];       //sqence number 的第一个字节
                int tempMessageId = FrmMain.getMessageId();
                s[0] = Convert.ToByte(tempMessageId / 256);
                s[1] = Convert.ToByte(tempMessageId % 256);


                byte[] buffer = new byte[16 + x_value.Length + y_value.Length];

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
                Array.Copy(xt, 0, buffer, i, xt.Length);  // X type复制

                i += xt.Length;
                Array.Copy(xl, 0, buffer, i, xl.Length);  //X  length复制

                i += xl.Length;
                Array.Copy(x_value, 0, buffer, i, x_value.Length); // X value的复制

                i += x_value.Length;
                Array.Copy(yt, 0, buffer, i, yt.Length);   //Y type 复制

                i += yt.Length;
                Array.Copy(yl, 0, buffer, i, yl.Length);   //Y length复制

                i += yl.Length;
                Array.Copy(y_value, 0, buffer, i, y_value.Length); // Y value复制

                if (!SendMsg(buffer))
                {

                    MessageBox.Show("数据传输发生错误，请确认通讯正常！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    this.SetToolEnable();
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void EditCamera(MapInfo.Geometry.DPoint dpt)
        {

            Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 5);
            IResultSetFeatureCollection rfc = null;

            try
            {
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dpt, mapControl1.Map.GetDisplayCoordSys(), d);
                si.QueryDefinition.Columns = null;
                rfc = Session.Current.Catalog.Search(_videoTableName, si);

                string camid = string.Empty;

                if (rfc.Count > 0)
                {
                    Systemstatus = "修改摄像机";

                    foreach (Feature f in rfc)
                    {
                        String id = f["ID"].ToString();

                        //查询数据库
                        if (this.con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }

                        con.Open();
                        OracleCommand sqlcmd = new OracleCommand("Select * from 视频位置 where MAPID=" + id, con);
                        Console.WriteLine(sqlcmd.CommandText);
                        sqlcmd.ExecuteNonQuery();
                        OracleDataAdapter apt = new OracleDataAdapter(sqlcmd);
                        DataTable dt = new DataTable();
                        apt.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                camid = Convert.ToString((dr["设备ID"]));
                                break;
                            }
                            if (camid != string.Empty)
                                break;
                        }
                        con.Close();
                    }
                }
                else
                {
                    MessageBox.Show("没有选择任何视频点");
                    return;
                }



                // 组合camera参数的TLV
                byte[] ct = new byte[2];   //camera参数的Type   
                ct[1] = 2;
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
                byte[] cmd = new byte[1] { 4 };  //命令
                byte[] rs = new byte[1] { 0 };   // 结果
                byte[] tl = new byte[2];        //总长度
                tl[1] = (byte)(12 + c_value.Length);
                tl[0] = Convert.ToByte((12 + c_value.Length) / 256);
                byte[] s = new byte[2];       //sqence number 的第一个字节

                int tempMessageId = FrmMain.getMessageId();
                s[0] = Convert.ToByte(tempMessageId / 256);
                s[1] = Convert.ToByte(tempMessageId % 256);

                byte[] buffer = new byte[12 + c_value.Length];

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

                if (!SendMsg(buffer))
                {
                    MessageBox.Show("数据传输发生错误，请确认通讯正常！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    this.contextMenuStrip1.Enabled = false;
                    this.toolcamera.Enabled = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                writelog("修改摄像机时发生错误"+e.Message);
                return;
            }
        }

        private void DelCamera(MapInfo.Geometry.DPoint dpt)
        {
            Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 5);
            IResultSetFeatureCollection rfc = null;

            try
            {

                int id = 100000;
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dpt, mapControl1.Map.GetDisplayCoordSys(), d);
                si.QueryDefinition.Columns = null;
                rfc = Session.Current.Catalog.Search(_videoTableName, si);

                string camid = "";

                if (rfc.Count > 0)
                {
                    foreach (Feature f in rfc)
                    {
                        id = Convert.ToInt32(f["ID"]);

                        //查询数据库
                        if (this.con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }

                        con.Open();
                        OracleCommand sqlcmd = new OracleCommand("Select * from 视频位置 where MAPID=" + id.ToString(), con);
                        sqlcmd.ExecuteNonQuery();
                        OracleDataAdapter apt = new OracleDataAdapter(sqlcmd);
                        DataTable dt = new DataTable();
                        apt.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                camid = Convert.ToString((dr["设备ID"]));
                                break;
                            }
                        }
                        con.Close();                      
                    }
                }
                else
                {
                    MessageBox.Show("没有选择任何视频点");
                    return;
                }

                // 发送 socket

                // 组合camera参数的TLV
                byte[] ct = new byte[2];   //camera参数的Type   
                ct[1] = 2;
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
                cl[0] = Convert.ToByte((4 + c_value.Length)/256);

                //包头
                byte[] v = new byte[1] { 1 };    //版本
                byte[] tp = new byte[1] { 2 };   //request
                byte[] cmd = new byte[1] { 5 };  //命令
                byte[] rs = new byte[1] { 0 };   // 结果
                byte[] tl = new byte[2];        //总长度
                tl[1] = (byte)(12 + c_value.Length);
                tl[0] = Convert.ToByte((12 + c_value.Length)/256);
                byte[] s = new byte[2];       //sqence number 的第一个字节
                int tempMessageId = FrmMain.getMessageId();
                s[0] = Convert.ToByte(tempMessageId / 256);
                s[1] = Convert.ToByte(tempMessageId % 256);

                byte[] buffer = new byte[12 + c_value.Length];

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
                Array.Copy(c_value, 0, buffer, i, c_value.Length); // camera value的复制

                Systemstatus = "删除摄像机";

                if (!SendMsg(buffer))
                {
                    MessageBox.Show("数据传输发生错误，请确认通讯正常！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    this.contextMenuStrip1.Enabled = false;
                    this.toolcamera.Enabled = false;
                    
                    //查询数据库
                    if (this.con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    con.Open();
                    OracleCommand sqldel = new OracleCommand("delete  视频位置 where MAPID=" + id.ToString(), con);
                    sqldel.ExecuteNonQuery();
                    con.Close();

                    RefreshMap();
                }
            }
            catch (DataException ex)
            {
                ShowDoInfo("处理数据库发生错误 ！");
                writelog(ex.Message);                
                return;
            }
            catch (Exception e)
            {
                writelog("删除摄像机时发生错误" + e.Message);
               // MessageBox.Show();
                return;
            }
           
        }


        private delegate void RefreshMapHandler(); 

        private void RefreshMap()
        {
            if (this.mapControl1.InvokeRequired)
            {
                this.BeginInvoke(new RefreshMapHandler(RefreshMap));
            }
            else
            {
                Distance dd = this.mapControl1.Map.Zoom;
                MapInfo.Geometry.DPoint dptt = new MapInfo.Geometry.DPoint(this.mapControl1.Map.Center.x, this.mapControl1.Map.Center.y);
                CreateVideoLayer();
                this.mapControl1.Map.Zoom = dd;
                this.mapControl1.Map.Center = dptt; 
            }
            //刷新地图             
        }

        private delegate void OnlyRefreshMapHandler(); 

        private void OnlyRefreshMap()
        {
            if (this.mapControl1.InvokeRequired)
            {
                this.BeginInvoke(new OnlyRefreshMapHandler(OnlyRefreshMap));
            }
            else
            {
                this.mapControl1.Refresh();
            } 
        }
        
        private delegate void SetToolEnableHandler();

        private void SetToolEnable()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new SetToolEnableHandler(SetToolEnable));
            }
            else
            {
                if (this.toolcamera.Enabled == false)
                {
                    this.toolcamera.Enabled = true;
                }
                if (this.contextMenuStrip1.Enabled == false)
                {
                    this.contextMenuStrip1.Enabled = true;
                }
            }
        }


        private delegate void SetMapViewHandler(string ftname);

        private void SetMapView(string ftrname)
        {
            if(this.mapControl1.InvokeRequired )
            {
                this.BeginInvoke(new SetMapViewHandler(SetMapView), ftrname);
            }
            else
            {
                string tblname = "街镇面";
                string colname = "NAME";

                if (ftrname != "")
                {
                    MapInfo.Mapping.Map map = null;


                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0))
                    {
                        return;
                    }

                    map = MapInfo.Engine.Session.Current.MapFactory[0];


                    MapInfo.Data.MIConnection conn = new MIConnection();
                    conn.Open();

                    MapInfo.Data.MICommand ftrcmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    ftrcmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + " like '%' +@name +'%'";
                    ftrcmd.Parameters.Add("@name", ftrname);

                    IResultSetFeatureCollection rsfcflash = ftrcmd.ExecuteFeatureCollection();

                    if (rsfcflash.Count > 0)
                    {
                        foreach (Feature f in rsfcflash)
                        {
                            mapControl1.Map.Center = f.Geometry.Centroid;
                        }
                    }
                    conn.Close();
                }
            }            
        }


        //十六进制转换为字符串
        public string StringToHex(string str)
        {
            //str = str.Trim();
            byte[] ByteFoo = System.Text.Encoding.Default.GetBytes(str);
            string TempStr = "";
            foreach (byte b in ByteFoo)
            {
                TempStr +=  b.ToString("X"); 
            }
            return TempStr;
        }


        private string[] camerarray;
        /// <summary>
        //打开视频窗口
        /// </summary>
        private void ShowVideo(MapInfo.Geometry.DPoint dPoint)
        {
            try
            {

                Distance d = new Distance() ;
                
                IResultSetFeatureCollection rfc = null;
                SearchInfo si = null;

                if ((this.mapControl1.Tools.LeftButtonTool == "Video"&&this.VideoLook !="周边")|| this.VideoLook == "单个")
                {
                     d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 8);
                     si = MapInfo.Data.SearchInfoFactory.SearchNearest(dPoint, mapControl1.Map.GetDisplayCoordSys(),d);
                }
                else if ((this.mapControl1.Tools.LeftButtonTool == "VideoDistance"&& this.VideoLook !="单个")|| this.VideoLook == "周边")
                {
                     d = new Distance(this.distan, DistanceUnit.Meter);
                     si = MapInfo.Data.SearchInfoFactory.SearchWithinDistance(dPoint, mapControl1.Map.GetDisplayCoordSys(), d, ContainsType.Geometry);
                }                

                try
                {                   
                    si.QueryDefinition.Columns = null;
                    rfc = Session.Current.Catalog.Search(_videoTableName, si);
                }
                catch(Exception ex)
                {                    
                    writelog("查询点位置可能存在问题"+ ex.Message );
                    return;
                }

                if (rfc.Count > 0)
                {
                    
                    if (rfc.Count > 32)
                    {
                        camerarray = new string[32];
                    }
                    else
                    {
                        camerarray = new string[rfc.Count];
                    }

                    int i = 0;
                    foreach (Feature f in rfc)
                    {
                        if (i > 31)
                        {
                           // MessageBox.Show("选择的视频点大于32个,系统默认取32个视频点！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        }

                        int ftename = Convert.ToInt32(f["ID"]);

                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }

                        con.Open();

                        OracleCommand cmd = new OracleCommand("Select * from 视频位置 where MAPID=" + ftename.ToString(), con);
                        cmd.ExecuteNonQuery();
                        OracleDataAdapter apt = new OracleDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        apt.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                string camid = Convert.ToString(dr["设备编号"]);
                                if (camid != "")
                                {
                                    camerarray[i] = Convert.ToString(dr["设备编号"]);
                                    i = i + 1;
                                    break;
                                }
                            }
                        }
                        con.Close();
                       
                    }
                }
                else
                {
                    MessageBox.Show("没有选择任何视频点");
                    return; 
                }

                if (camerarray.Length > 0 && camerarray[0] != "")
                {                    
                    SendCamerID();// 产生协议并发送      
                }
            }
            catch
            { MessageBox.Show("无法打开摄像机", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }



        private byte[] buffer;
        /// <summary>
        /// 发送打开设备编号
        /// </summary>
        private void SendCamerID()
        {
            try
            {        
                if (camerarray.Length == 1)
                {
                    // 组合camera参数的TLV
                    byte[] ct = new byte[2];   //camera参数的Type   
                    ct[1] = 9;
                    ct[0] = 0;

                    byte[] c_value = Encoding.UTF8.GetBytes(camerarray[0]);  //camera参数的Value
                    if (c_value.Length % 4 != 0)                     //判断是否为是4的倍数
                    {
                        byte[] temp1 = new byte[c_value.Length];
                        for (int t1 = 0; t1 < c_value.Length; t1++)
                        {
                            temp1[t1] = c_value[t1];
                        }

                        c_value = new byte[temp1.Length +(4- temp1.Length % 4)];
                        for (int ii = 0; ii < temp1.Length; ii++)
                        {
                            c_value[ii] = temp1[ii];
                        }
                    }

                    byte[] cl = new byte[2];       //camera参数的Length
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

                    int tempMessageId = FrmMain.getMessageId();
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

                }
                else if (camerarray.Length > 1)
                {
                    // cameralistTLV

                    byte[] ct = new byte[2];   //cameralist参数的Type   
                    ct[1] = 9;
                    ct[0] = 0;


                    // cameralist 参数的value 

                    byte[] c1_value = Encoding.UTF8.GetBytes(camerarray[0]);  //cameralist第一个Value

                    byte[] c_value = new byte[c1_value.Length  + (camerarray.Length - 1) * (c1_value.Length +1)];

                    int k = 0;

                    Array.Copy(c1_value, 0, c_value, k, c1_value.Length);  // 将第一个设备编号复制到value中
                    
                    for (int j = 1; j < camerarray.Length; j++)   //从cameralist 的第二个值开始循环赋值
                    {                        
                        byte[] temp_value = Encoding.UTF8.GetBytes(","+camerarray[j]);  // 

                        Array.Copy(temp_value, 0, c_value, c1_value.Length + temp_value.Length*(j-1), temp_value.Length);  // 将第一个设备编号复制到value中
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
                    tl[0] = Convert.ToByte((12 + c_value.Length) / 256);                                           
                    
                    byte[] s = new byte[2];       //sqence number 的第一个字节

                    int tempMessageId = FrmMain.getMessageId();
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
                }

                if (this.VideoFlag == true)
                {
                    if (!SendMsg1(buffer))
                    {
                        MessageBox.Show("发送数据失败，请检测通讯是否正常！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }
                }
                else
                {
                    MessageBox.Show("与监控客户端尚未建立连接，请检查！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                }
            }
            catch(Exception ex)
            {
                writelog("发送设备编号时发生错误！"+ex.Message);                
            }
        }

        private void OpenVideoClient()
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                int index = this.VideoExePath.LastIndexOf('\\');
                if (index != -1)
                {
                    string VideoArg = string .Empty;
                    for(int i =0; i< this.VideoConnect.Length; i++)
                    {
                      VideoArg += VideoConnect[i];
                      VideoArg += " ";
                    }
                    string dir = this.VideoExePath.Substring(0, index);
                    process.StartInfo.WorkingDirectory = dir;
                    process.StartInfo.Arguments = VideoArg;

                    writelog(VideoArg);
                }

                process.StartInfo.FileName = this.VideoExePath;
                process.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show("启动监控端程序失败，请检测配置管理中监控端设置！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                writelog(ex.Message);
            }
        }
        

        private const int GW_HWNDFIRST = 0;
        private const int GW_HWNDNEXT = 2;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 268435456;
        private const int WS_BORDER = 8388608;

        #region AIP声明
        [DllImport("IpHlpApi.dll")]
        extern static public uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        private extern static int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        private extern static int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
        #endregion

        /// <summary>
        /// 判断视频窗口是否已经打开
        /// </summary>
        /// <param name="ftrid"></param>
        /// <returns></returns>
        public Boolean IsOpenCam()
        {

            try
            {
                Boolean isop = false;

                int t = this.VideoExePath.LastIndexOf("\\");
                string CamName = this.VideoExePath.Substring(t + 1, this.VideoExePath.Length - t-1);// "surveillance.exe";

                int hwCurr;
                hwCurr = GetWindow(this.Handle.ToInt32(), GW_HWNDFIRST);

                while (hwCurr > 0)
                {
                    int IsTask = (WS_VISIBLE | WS_BORDER);
                    int lngStyle = GetWindowLongA(hwCurr, GWL_STYLE);
                    bool TaskWindow = ((lngStyle & IsTask) == IsTask);
                    if (TaskWindow)
                    {
                        int length = GetWindowTextLength(new IntPtr(hwCurr));
                        StringBuilder sb = new StringBuilder(2 * length + 1);
                        GetWindowText(hwCurr, sb, sb.Capacity);
                        string strTitle = sb.ToString();
                                                
                        if (!string.IsNullOrEmpty(strTitle))
                        {
                            int i = strTitle.IndexOf(CamName);
                            if (i != -1)
                            {
                                isop = true;
                                return isop;
                            }
                        }
                    }
                    hwCurr = GetWindow(hwCurr, GW_HWNDNEXT);
                }
                return isop;
            }
            catch(Exception ex)
            {
                writelog(ex.Message);
                return false;
            }
        }



     
        //public int Listenport = 0;              // 管理客户端监听端口

        //private int ListenportVideo = 0;        //监听客户端监听端口

        //private string userid = "";
        //private string datasource = "";
        //private string password = "";

        //public string MapName = "";             // 地图名称

        //public string VideoExePath = "";        //  视频监控客户端启动位置

        //private double distan = 0;              //周边查询半径



        /// <summary>
        // 测试通讯
        /// </summary>
        private void ShowGeoCodingDialog()
        {
            frmInput frmset = new frmInput();
            frmset.setparaments(this.datasource,this.userid,this.password,this.Listenport.ToString(),this.ListenportVideo.ToString(),this.MapName,this.VideoExePath,this.strRegion,this.distan);
            if (frmset.ShowDialog(this) == DialogResult.OK)
            {
                MessageBox.Show("配置文件设置成功,程序将关闭完成配置！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StopThread();
            }
            else
            {
                //MessageBox.Show("没有完成系统配置！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        /// <summary>
        // 工具条状态
        /// </summary>
        private void ToolBarCheck(ToolStripItem tsi)
        {
            //for (int i = 0; i < toolStripToolBar.Items.Count;i++)
            //{
            //    if(toolStripToolBar.Items[i].c
            //}
            //tsbGeoCoding.Checked = false;
            //tsbOpenTable.Checked = false;
            //toolStripButton2.Checked = false;

            //tsbZoomIn.Checked = false;
            //tsbZoomOut.Checked = false;
            //tsbPan.Checked = false;
            //toolStripButton1.Checked = false;

            //tsbMapLayer.Checked = false;
            //toolradius.Checked = false;
            //VideoClient.Checked = false;

            UncheckedTool();
            //tsbVideo.Checked = false;
            if (tsi != null)
                ((ToolStripButton)tsi).Checked = true;
        }

        //选中的checked，其他unchecked，以便明确当前的选择项
        //由于按钮分组，iFrom表示组的首Index，iEnd表示末Index
        private void UncheckedTool()
        {
            for (int i = 0; i < toolStripToolBar.Items.Count; i++)
            {
                ToolStripButton tsButton = toolStripToolBar.Items[i] as ToolStripButton;
                if (tsButton != null)
                {
                    if (tsButton.Checked == true)
                    {
                        tsButton.Checked = false;
                    }
                }
            }
        }

        /// <summary>
        //  加载地图
        /// </summary>
        private void OpenTable()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Multiselect = false;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.DefaultExt = "WMS";
            openFileDialog1.InitialDirectory = Application.StartupPath + @"\Map";
            openFileDialog1.Title = "选择地图文件--";
            openFileDialog1.Filter = "MapXtreme Space(*.mws)|*.mws|GST文件(*.gst)|*.gst|所有文件(*.*)|*.*";
            if (openFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                int k = openFileDialog1.FileName.LastIndexOf("\\");

                string filename = openFileDialog1.FileName.Substring(k+1,openFileDialog1.FileName.Length - k-1); ; //地图文件名称                

                int i = openFileDialog1.FileName.IndexOf(openFileDialog1.InitialDirectory);

                if (i != -1)
                {
                    //string mnn = filename.Substring(openFileDialog1.InitialDirectory.Length + 1, filename.Length - openFileDialog1.InitialDirectory.Length - 1);

                    string mnn = filename;

                    if (mnn != this.MapName)
                    {
                        mapControl1.Map.Clear();
                        mapControl1.Map.Load(new MapWorkSpaceLoader(openFileDialog1.FileName));
                        SaveMapName(mnn);                                
                    }
                    else
                    {
                        MessageBox.Show("打开的地图与当前地图相同.", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("请将地图拷贝到安装路径的Map文件夹下.", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }                
            }
        }

        #endregion

               
        /// <summary>
        /// 保存XML文件
        /// </summary>
        /// <param name="mapname"></param>
        private void SaveMapName(string mapname)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Application.StartupPath + @"\Config.XML");
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("Con").ChildNodes;
            foreach (XmlNode mynode in nodeList)
            {
                if (mynode.Name == "Map")
                {
                    mynode.InnerText = mapname;
                    break;                        
                }                
            }
            xmlDoc.Save(Application.StartupPath + @"\Config.XML");//保存。
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            System.Drawing.Point point = e.Location;
            MapInfo.Geometry.DPoint dp;
            mapControl1.Map.DisplayTransform.FromDisplay(point, out dp);
            this.toolStripStatusLabelCoord.Text = "坐标:" + dp.x.ToString("F4") + "," + dp.y.ToString("F4");

            if (mapControl1.Map.Layers[_videoTableName] != null)
            {
                GetDisFtr(dp, e);
            }
        }

                
        /// <summary>
        /// Tip显示
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="e"></param>
        public void GetDisFtr(MapInfo.Geometry.DPoint dp, MouseEventArgs e)
        {
            Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 5);
            IResultSetFeatureCollection rfc = null;
            try
            {
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dp, mapControl1.Map.GetDisplayCoordSys(), d);
                si.QueryDefinition.Columns = null;
                rfc = Session.Current.Catalog.Search(_videoTableName, si);           

            if (rfc != null)
            {
                if (rfc.Count > 0)
                {
                    PopumenuEnable();

                    //foreach (Feature f in rfc)
                    //{
                    //    int  id = Convert.ToInt32(f["ID"]);

                    //    string mysqlstr = "user id =" + this.userid + " ;data source = " + this.datasource + ";password =" + this.password;

                    //    OracleConnection con = new OracleConnection(mysqlstr);
                    //    con.Open();
                    //    OracleCommand cmd = new OracleCommand("Select * from 视频位置 where MAPID=" + id.ToString(), con);
                    //    cmd.ExecuteNonQuery();
                    //    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    //    DataTable dt = new DataTable();
                    //    apt.Fill(dt);
                    //    if (dt.Rows.Count > 0)
                    //    {
                    //        foreach (DataRow dr in dt.Rows)
                    //        {
                    //            //this.lblVideo.Text = Convert.ToString((dr["设备ID"]));
                    //            //if (lblVideo.Text == "")
                    //            //{
                    //            //    lblVideo.Text = "无";
                    //            //}
                    //            this.lblName.Text = Convert.ToString((dr["设备名称"]));
                    //            if (lblName.Text == "")
                    //            {
                    //                lblName.Text = "无";
                    //            }
                    //            this.lblPol.Text = Convert.ToString(dr["所属派出所"]);
                    //            if (lblPol.Text == "")
                    //            {
                    //                lblPol.Text = "无";
                    //            }
                    //            this.lblMan.Text = Convert.ToString(dr["日常管理人"]);
                    //            if (lblMan.Text == "")
                    //            {
                    //                lblMan.Text = "无";
                    //            }

                    //            this.panTip.Location = new System.Drawing.Point(e.X, e.Y + 10);
                    //            this.panTip.Visible = true;
                    //            break;
                    //        }
                    //    }
                    //    con.Close();
                    //}
                }
                else
                {
                    this.panTip.Visible = false;

                    PopumenuDisEnable();
                }
            }
        }
        catch { }
        }

        private void PopumenuEnable()
        {
            this.rtooldel.Enabled = true ;
            this.rtooledit.Enabled = true;

            this.rtoolopen.Enabled = true;
        }


        private void PopumenuDisEnable()
        {
            this.rtooldel.Enabled = false;
            this.rtooledit.Enabled = false;

            this.rtoolopen.Enabled = false;
        }


        //创建视频图层
        public void CreateVideoLayer()
        {
            try
            {
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable(_videoTableName);
                Table tblTemp = Cat.GetTable(_videoTableName);
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable(_videoTableName);
                }

                Table tblTempLabel = Cat.GetTable("VidoeLabel");
                if (tblTempLabel != null)
                {
                    Cat.CloseTable("VideoLabel");
                }

                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }


                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 20));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Insert(0,lyr);

                //添加标注
                string activeMapLabel = "VideoLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable(_videoTableName);
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
                mapControl1.Map.Layers.Insert(1, lblayer);//有错误

                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers[_videoTableName] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable(_videoTableName);
                if (this.con.State == ConnectionState.Open)
                {
                    con.Close();
                }

                con.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = con;
                if (this.strRegion == "")
                {
                    cmd.CommandText = "Select * from 视频位置 order by MAPID　desc";
                }
                else
                {
                    if (this.strRegion == null)
                    {
                        MessageBox.Show("区域权限没有正确配置", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {

                        cmd.CommandText = "Select * from 视频位置 where 所属派出所 ='" + this.strRegion + "' order by MAPID desc";
                    }
                }
                
                cmd.ExecuteNonQuery();
                OracleDataAdapter apt = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new MapInfo.Geometry.DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                        
                        CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(59, System.Drawing.Color.Green, 20));
                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = dr["设备名称"].ToString();
                        ftr["ID"] = dr["MAPID"].ToString();
                        tblcar.InsertFeature(ftr);
                    }
                }
                con.Close();

                if (mapControl1.Map.Layers[_videoTableName] == null)
                {
                   writelog("视频监控点的图层不存在，无法进行选择！");
                }              
            }
            catch(Exception e) 
            {

                Console.WriteLine(e.StackTrace);
            }
        }


        MapInfo.Geometry.DPoint dpr;
        private void mapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point point = e.Location;
                mapControl1.Map.DisplayTransform.FromDisplay(point, out dpr);
            }
        }

        private void AnalyData(byte[] msg) 
        {
            try
            {
                //解析 包头
                short type = msg[1]; //获取类型 --1 notic  2 request  3 response
                short cmd = msg[2];  //1 dis  2 close
                short res = msg[3]; //返回的结果 只有response有结果
                byte[] tl_array = new byte[2];
                tl_array[0] = msg[5];
                tl_array[1] = msg[4];
                short tl = BitConverter.ToInt16(tl_array, 0);//总长度

                tl_array[0] = msg[7];
                tl_array[1] = msg[6];
                short ss = BitConverter.ToInt16(tl_array, 0); ;  //sequence number
                string ftrname = "";
                short tt=0, ttl=0;


                string MapReset = "";
                //判断参数
                if (type == 1)  //请求
                {
                    if (cmd == 1)  //打开地图
                    {
                        tl_array[0] = msg[9];
                        tl_array[1] = msg[8];
                        tt = BitConverter.ToInt16(tl_array, 0); //参数名称  1 mapname 2 camnum 

                        tl_array[0] = msg[11];
                        tl_array[1] = msg[10];
                        ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度

                        tl_array = new byte[ttl - 4];
                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[12 + i];
                        }

                        String properties_value = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);
                        Console.WriteLine(properties_value);
                        ftrname = properties_value;
                        if (tt == 1)  //参数是地图
                        {
                            if (this.Visible == false)
                            {
                                this.Visible = true;
                            }

                            this.SetMapView(ftrname);
                        }
                        else if (tt == 2) //参数是摄像机
                        {
                                camerarray = new string[0];
                                camerarray[0] = ftrname;       // 设备编号 赋值给cameralist
                                Boolean isexist = IsOpenCam(); // 判读监控客户端是否已经打开
                                if (isexist == false)
                                {
                                    MessageBox.Show("监控客户端尚未打开，请打开后再进行此类操作！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                   // OpenVideoClient();         // 打开监控客户端                                    
                                }

                                Thread.Sleep(TimeSpan.FromMilliseconds(500d)); //延缓时间进行通讯链接
                                SendCamerID();// 产生协议并发送     
                        }
                    }
                    else if (cmd == 2)  //关闭地图
                    {
                        this.Visible = false;
                    }
                    else if (cmd == 6)    // Add Camera Request
                    {
                        tl_array[0] = msg[9];                         
                        tl_array[1] = msg[8];
                        tt = BitConverter.ToInt16(tl_array, 0); //设备编号
                        tl_array[0] = msg[11];
                        tl_array[1] = msg[10];
                        ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[12 + i];
                        }
                        String camid = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                        int nextPropertiesIndex = PackageHeaderLength;

                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引

                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //cameraNumber
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String cameraNumber = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);


                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //devName
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String devName = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);


                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); // X
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); // 属性长度
                        double x = BitConverter.ToDouble(msg, nextPropertiesIndex + 4);

                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); // Y
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); // 属性长度
                        double y = BitConverter.ToDouble(msg, nextPropertiesIndex + 4);

                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //GongAn
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String GongAn = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //Owner
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String Owner = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);
                        //取得最大ID
                        int MAPID = 0;
                        if (this.con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                        con.Open();
                        string sql1 = "select * from 视频位置 order by MAPID desc";
                        OracleCommand sqlcmd1 = new OracleCommand(sql1, con);
                        sqlcmd1.ExecuteNonQuery();
                        OracleDataAdapter apt1 = new OracleDataAdapter(sqlcmd1);
                        DataTable dt1 = new DataTable();
                        apt1.Fill(dt1);
                        if (dt1.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt1.Rows)
                            {
                                MAPID = Convert.ToInt32(dr["MAPID"]);
                                break;
                            }
                        }

                        string sql = "insert into 视频位置(设备编号,设备ID,设备名称,所属派出所,日常管理人,MAPID,X,Y) values('" + camid + "','" + cameraNumber + "','" + devName + "','" + GongAn + "','" + Owner + "'," + (MAPID + 1).ToString() + "," + x + "," + y + ")";
                        OracleCommand sqlcmd = new OracleCommand(sql, con);
                        sqlcmd.ExecuteNonQuery();
                        con.Close();

                        MapReset = "重载";
                    }
                    else if (cmd == 7)     //edit camera  request
                    {
                        tl_array[0] = msg[9];
                        tl_array[1] = msg[8];
                        tt = BitConverter.ToInt16(tl_array, 0); //设备编号
                        tl_array[0] = msg[11];
                        tl_array[1] = msg[10];
                        ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[12 + i];
                        }
                        String camid = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                        int nextPropertiesIndex = PackageHeaderLength;
                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引

                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //cameraNumber
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String cameraNumber = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);


                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //devName
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String devName = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);



                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //GongAn
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String GongAn = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                        nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                        tl_array[0] = msg[nextPropertiesIndex + 1];
                        tl_array[1] = msg[nextPropertiesIndex];
                        tt = BitConverter.ToInt16(tl_array, 0); //Owner
                        tl_array[0] = msg[nextPropertiesIndex + 3];
                        tl_array[1] = msg[nextPropertiesIndex + 2];
                        ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                        tl_array = new byte[ttl - 4];

                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                        }
                        String Owner = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);
                        if (this.con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }

                        con.Open();
                        string sql = "update 视频位置 set 设备ID='" + cameraNumber + "',设备名称='" + devName + "',所属派出所='" + GongAn + "',日常管理人='" + Owner + "' where 设备编号 ='" + camid + "'";
                        OracleCommand sqlcmd = new OracleCommand(sql, con);
                        sqlcmd.ExecuteNonQuery();
                        con.Close();

                        MapReset = "重载";
                    }
                    else if(cmd ==8)  // Delete Camera request
                    {

                        tl_array[0] = msg[9];
                        tl_array[1] = msg[8];
                        tt = BitConverter.ToInt16(tl_array, 0); //参数名称  cameranumber
                        tl_array[0] = msg[11];
                        tl_array[1] = msg[10];
                        ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度

                        tl_array = new byte[ttl - 4];
                        for (int i = 0; i < tl_array.Length; i++)
                        {
                            tl_array[i] = msg[12 + i];
                        }

                        String properties_value = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);
                        String cameranum = properties_value;

                        if (this.con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                        con.Open();
                        OracleCommand sqldel = new OracleCommand("delete  视频位置 where  设备ID = '" + cameranum +"'", con);
                        sqldel.ExecuteNonQuery();
                        con.Close();

                        MapReset = "重载";
                    } 
                }
                else if (type == 4)   //response  
                {
                    //有返回则设置工具栏可用

                    SetToolEnable();                   

                    if (res == 0)  //返回结果正确
                    {
                        if (cmd == 3)//add camear Response    添加摄像头
                        {
                            tl_array[0] = msg[9];
                            tl_array[1] = msg[8];
                            tt = BitConverter.ToInt16(tl_array, 0); //设备编号
                            tl_array[0] = msg[11];
                            tl_array[1] = msg[10];
                            ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[12 + i];
                            }
                            String 设备编号 = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                            int nextPropertiesIndex = PackageHeaderLength;

                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引

                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //cameraNumber
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String cameraNumber = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);


                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //devName
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String devName = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);


                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); // X
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); // 属性长度
                            double x = BitConverter.ToDouble(msg, nextPropertiesIndex + 4);

                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); // Y
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); // 属性长度
                            double y = BitConverter.ToDouble(msg, nextPropertiesIndex + 4);

                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //GongAn
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String GongAn = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //Owner
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String Owner = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);
                            //取得最大ID
                            int MAPID = 0;
                            if (this.con.State == ConnectionState.Open)
                            {
                                con.Close();
                            }
                            con.Open();
                            string sql1 = "select * from 视频位置 order by MAPID desc";
                            OracleCommand sqlcmd1 = new OracleCommand(sql1, con);
                            sqlcmd1.ExecuteNonQuery();
                            OracleDataAdapter apt1 = new OracleDataAdapter(sqlcmd1);
                            DataTable dt1 = new DataTable();
                            apt1.Fill(dt1);
                            if (dt1.Rows.Count > 0)
                            {
                                foreach (DataRow dr in dt1.Rows)
                                {
                                    MAPID = Convert.ToInt32(dr["MAPID"]);
                                    break;
                                }
                            }
 
                            string sql = "insert into 视频位置(设备编号,设备ID,设备名称,所属派出所,日常管理人,MAPID,X,Y) values('" + 设备编号 + "','" + cameraNumber + "','" + devName + "','" + GongAn + "','" + Owner + "'," + (MAPID + 1).ToString() + "," + x + "," + y + ")";
                            OracleCommand sqlcmd = new OracleCommand(sql, con);
                            sqlcmd.ExecuteNonQuery();
                            con.Close();
                            ////刷新地图  

                            MapReset = "重载";

                            //CreateVideoLayer();
                            //mapControl1.Refresh();

                        }
                        else if (cmd == 4)//edit camear    修改摄像头
                        {
                            tl_array[0] = msg[9];
                            tl_array[1] = msg[8];
                            tt = BitConverter.ToInt16(tl_array, 0); //设备编号
                            tl_array[0] = msg[11];
                            tl_array[1] = msg[10];
                            ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[12 + i];
                            }
                            String camid = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                            int nextPropertiesIndex = PackageHeaderLength;
                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引

                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //cameraNumber
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //参数的长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String cameraNumber = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);


                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //devName
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String devName = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);



                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //GongAn
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String GongAn = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                            nextPropertiesIndex = nextPropertiesIndex + getMessagePropertyLength(ttl);//下一个属性的索引
                            tl_array[0] = msg[nextPropertiesIndex + 1];
                            tl_array[1] = msg[nextPropertiesIndex];
                            tt = BitConverter.ToInt16(tl_array, 0); //Owner
                            tl_array[0] = msg[nextPropertiesIndex + 3];
                            tl_array[1] = msg[nextPropertiesIndex + 2];
                            ttl = BitConverter.ToInt16(tl_array, 0); //属性长度
                            tl_array = new byte[ttl - 4];

                            for (int i = 0; i < tl_array.Length; i++)
                            {
                                tl_array[i] = msg[nextPropertiesIndex + 4 + i];
                            }
                            String Owner = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);

                            try
                            {
                                if (this.con.State == ConnectionState.Open)
                                {
                                    con.Close();
                                }

                                con.Open();
                                //string sql = "update 视频位置 set 设备编号='" + 设备编号 + "',设备名称='" + devName + "',所属派出所='" + GongAn + "',日常管理人='" + Owner + "' where 设备ID='" + cameraNumber + "'";
                                string sql = "update 视频位置 set 设备ID='" + cameraNumber + "',设备名称='" + devName + "',所属派出所='" + GongAn + "',日常管理人='" + Owner + "' where 设备编号 ='" + camid + "'";
                                OracleCommand sqlcmd = new OracleCommand(sql, con);
                                sqlcmd.ExecuteNonQuery();
                                con.Close();
                                MapReset = "重载";
                            }
                            catch
                            {
                                if (this.con.State == ConnectionState.Open)
                                {
                                    con.Close();
                                }
                            }
                        }
                    }
                }
                ShowDoInfo("接收数据完成");
                writelog("数据接收完成");
                if (MapReset  == "重载")
                {
                    RefreshMap();
                    MapReset = "";
                }
                OnlyRefreshMap();               
                writelog("地图更新完成");               
            }
            catch (DataException ex)
            {
                Console.WriteLine(ex.StackTrace + ex.Message);
                writelog(ex.Message);
                ShowDoInfo("处理数据库发生错误.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace + ex.Message);
                writelog(ex.Message);
                ShowDoInfo("数据分析发生错误.");
            }       
        }

        private void toolOpen_Click(object sender, EventArgs e)
        {
          //  mapControl1.Tools.LeftButtonTool = "Video";   
        }

        private void toolInsert_Click(object sender, EventArgs e)
        {
            mapControl1.Tools.LeftButtonTool = "Add";           
        }

        private void tooledit_Click(object sender, EventArgs e)
        {
            mapControl1.Tools.LeftButtonTool = "Edit";          
        }

        private void tooldel_Click(object sender, EventArgs e)
        {
            mapControl1.Tools.LeftButtonTool = "Del";           
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;                    
        }

        private void FrmMain_Resize(object sender, EventArgs e)
        {
            //if (this.WindowState == FormWindowState.Minimized)
            //{
            //    this.Visible = false;
            //}
            //else
            //{
            //    this.Visible = true;
            //}
        }


        public void writelog(string msg)
        {
            try
            {
                string filepath = Application.StartupPath + "\\rec.log";
                msg = DateTime.Now.ToString() + ":" + msg;

                StreamWriter sw = File.AppendText(filepath);
                sw.WriteLine(msg);
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        private void FrmMain_MinimumSizeChanged(object sender, EventArgs e)
        {
            this.Visible = true;
        }
        private int getMessagePropertyLength(int length)
        {if(length%4!=0)
        {
            return 4 - length % 4 + length;
        }
        return length;

        }

         private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //mapControl1.Refresh();
            //CreateVideoLayer();

            LoadApp();
        }



        private void rtoolopen_Click(object sender, EventArgs e)
        {
            if (dpr != new MapInfo.Geometry.DPoint(0, 0))
            {
                ShowVideo(dpr);
                dpr = new MapInfo.Geometry.DPoint(0, 0);
            }
        }

        private void rtoolAdd_Click(object sender, EventArgs e)
        {
            if (dpr != new MapInfo.Geometry.DPoint(0, 0))
            {
                AddCamera(dpr);
                dpr = new MapInfo.Geometry.DPoint(0, 0);                
            }
        }

        private void rtooledit_Click(object sender, EventArgs e)
        {
            if (dpr != new MapInfo.Geometry.DPoint(0, 0))
            {
                EditCamera(dpr);
                dpr = new MapInfo.Geometry.DPoint(0, 0);               
            }
        }

        private void rtooldel_Click(object sender, EventArgs e)
        {
            if (dpr != new MapInfo.Geometry.DPoint(0, 0))
            {
                DelCamera(dpr);
                dpr = new MapInfo.Geometry.DPoint(0, 0);              
            }
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }


        //////

        //----------======-----------
        //建立监控客户端网络监听
        //===========================
        private TcpListener tcpserver1;
        private System.Net.Sockets.TcpListener tcpListener1;
        private System.Threading.Thread ServerThread1;

        //停止服务标志    
        public bool Stop1;
        private TcpClient tcpClient1;
        private bool TcpClose1;
        public System.Threading.Thread tcpClientThread1;
        //启动客户连接线程  

        public void CreateVideoSocket()
        {
            try
            {    //取主机名    
                string host1 = Dns.GetHostName();
                //解析本地IP地址，    
                IPHostEntry hostIp1 = Dns.GetHostByName(host1);
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
                //IPAddress ip1 = IPAddress.Parse("135.251.26.222");
                tcpserver1 = new TcpListener(ip1, this.ListenportVideo);
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
                //Control.CheckForIllegalCrossThreadCalls = false;

                if (this.ServerThread1 != null)
                {
                    //("线程已经运行......");
                    return;
                }
                writelog(string.Format("New TcpListener....Port={0}", this.tcpserver1.LocalEndpoint.ToString()));
                this.toolstatus.Text = this.tcpserver1.LocalEndpoint.ToString();

                tcpserver1.Start();
                //生成线程，start（）函数为线程运行时候的程序块    
                this.ServerThread1 = new Thread(new ThreadStart(startsever1));
                ////线程的优先级别    
                //this.ServerThread.Priority = ThreadPriority.BelowNormal;
                this.ServerThread1.Start();
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
        }


        ////结束该服务线程，同时要结束由该服务线程所生成的客户连接    
        //public void Abort()
        //{
        //    writelog("正在关闭所有客户连接......");
        //    //调用客户线程的静态方法，结束所以生成的客户线程    
        //    ThreadClientProcessor.AbortAllClient();
        //    writelog(string.Format("关闭服务线程: {0}", this.tcpserver.LocalEndpoint.ToString()));
        //    this.Stop = true;
        //    //结束本服务启动的线程    
        //    if (this.ServerThread != null) ServerThread.Abort();
        //    tcpListener.Stop();
        //    //程序等500毫秒    
        //    Thread.Sleep(TimeSpan.FromMilliseconds(500d));
        //}

        private void startsever1()
        {
            try
            {
                while (!Stop1)   //几乎是死循环    
                {
                    writelog("正在等待连接......");
                    tcpClient1 = tcpserver1.AcceptTcpClient();
                    writelog("已经建立连接......");
                    ShowDoInfo("已与客户端建立连接");
                    this.VideoFlag = true;
                    tcpClientThread1 = new Thread(new ThreadStart(startclient1));
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


        // ThreadClient
        ////===================

        //public void clientAbort()
        //{
        //    if (this.tcpClientThread != null)
        //    {
        //        //tcpClientThread.Interrupt();    
        //        tcpClientThread.Abort();
        //        //一定要等一会儿，以为后边tcpClient.Close（）时候，会影响NetWorkStream的操作    
        //        Thread.Sleep(TimeSpan.FromMilliseconds(100));
        //        tcpClient.Close();
        //    }
        //}


        private NetworkStream networkStream1;
        private byte[] buf1 = new byte[1024 * 1024];   //预先定义1MB的缓冲  
        private int Len1 = 0;   //流的实际长度  

        //读写连接的函数，用于线程    
        private void startclient1()
        {
            networkStream1 = tcpClient1.GetStream();   //建立读写Tcp的流    
            try
            {
                //开始循环读写tcp流    
                while (!TcpClose1)
                {
                    //如果当前线程是在其它状态，（等待挂起，等待终止.....)就结束该循环    
                    if (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.Running)
                        break;

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
                        AnalyData(temp1);//解析收到的命令                           
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
                SetToolEnable();
                //String properties_value = Encoding.UTF8.GetString(tl_array, 0, tl_array.Length);
                //writelog(Encoding.UTF8.GetString(msg1, 0, msg1.Length) + "  长度：" + msg1.Length.ToString());
                ShowDoInfo("数据发送成功");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                writelog(e.Message);
                ShowDoInfo("数据发送失败");
                return false;
                //writelog("发送:" + Encoding.UTF8.GetString(msg,0,msg.Length));
            }
            return true;
        }

        private void 选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl1.Tools.LeftButtonTool = "Video";   
        }

        private void 周边ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl1.Tools.LeftButtonTool = "VideoDistance";   
        }

        private string VideoLook = string.Empty;


        private void 查看单个ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dpr != new MapInfo.Geometry.DPoint(0, 0))
            {
                this.VideoLook = "单个";
                ShowVideo(dpr);
                dpr = new MapInfo.Geometry.DPoint(0, 0);
                this.VideoLook = "";
            }
        }

        private void 查看周边ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dpr != new MapInfo.Geometry.DPoint(0, 0))
            {
                this.VideoLook = "周边";
                ShowVideo(dpr);
                dpr = new MapInfo.Geometry.DPoint(0, 0);
                this.VideoLook = "";
            }

        }

        private void VideoClient_Click(object sender, EventArgs e)
        {
            try
            {
                Boolean isexist = IsOpenCam();
                if (isexist == false)
                {
                    OpenVideoClient();
                    // 打开监控客户端
                }
            }
            catch
            {
                MessageBox.Show("启动客户端错误，确认系统配置正确！","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }

            //Thread.Sleep(TimeSpan.FromMilliseconds(500d));
        }


        //====================================================================================
        //======================查看影像图
        //====================================================================================

       


        private System.Windows.Forms.Label[] levelArr = new System.Windows.Forms.Label[7];
        #region 查看影像
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
                writelog("closeOtherLevelImg" + ex.ToString());
            }
        }

        private void CalRowColAndDisImg(int iLevel)
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

        private void DisImg(int iLevel, int beginRow, int endRow, int beginCol, int endCol)
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
            //mapControl1.Refresh();
        }

        private void openTable(int iLevel, string tableName)
        {
            //先判断有没有加载
            try
            {
                GroupLayer groupLayer = mapControl1.Map.Layers["影像"] as GroupLayer;

                if (groupLayer["_" + tableName] == null)
                {
                    //再判断文件夹中村不存在，存在就打开

                    string imgPath = Application.StartupPath.Remove(Application.StartupPath.LastIndexOf(@"\")) + "\\Data\\ImgData\\" + iLevel.ToString() + "\\" + tableName + ".tab";
                    if (File.Exists(imgPath))
                    {
                        Table tab = MapInfo.Engine.Session.Current.Catalog.OpenTable(imgPath);

                        MapInfo.Mapping.FeatureLayer fl = new MapInfo.Mapping.FeatureLayer(tab, "_" + tableName);

                        groupLayer.Add(fl);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("地图文件已损坏,请联系软件开发人员!");
                writelog("openTable" + ex.ToString());
                return;
            }
        }

        #endregion

        //当地图视野变化时，根据比例尺指定地图级别
        private int setToolslevel()
        {
            //foreach (System.Windows.Forms.Label lev in levelArr)
            //{
            //    if (lev.BackColor == Color.Red)
            //    {
            //        lev.BackColor = Color.Transparent;
            //    }
            //}
            int iLevel = 0;
            if (Math.Round(mapControl1.Map.Scale) >= 200000)
            {
                //level1.BackColor = Color.Red;
                iLevel = 1;
            }
            else if (Math.Round(mapControl1.Map.Scale) < 200000 && Math.Round(mapControl1.Map.Scale) >= 100000)
            {
                //level2.BackColor = Color.Red;
                iLevel = 2;
            }
            else if (Math.Round(mapControl1.Map.Scale) < 100000 && Math.Round(mapControl1.Map.Scale) >= 50000)
            {
                //level3.BackColor = Color.Red;
                iLevel = 3;
            }
            else if (Math.Round(mapControl1.Map.Scale) < 50000 && Math.Round(mapControl1.Map.Scale) >= 20000)
            {
                //level4.BackColor = Color.Red;
                iLevel = 4;
            }
            else if (Math.Round(mapControl1.Map.Scale) < 20000 && Math.Round(mapControl1.Map.Scale) >= 10000)
            {
                //level5.BackColor = Color.Red;
                iLevel = 5;
            }
            else if (Math.Round(mapControl1.Map.Scale) < 10000 && Math.Round(mapControl1.Map.Scale) >= 5000)
            {
                //level6.BackColor = Color.Red;
                iLevel = 6;
            }
            else
            {
                //level7.BackColor = Color.Red;
                iLevel = 7;
            }
            return iLevel;
        }
    }
}