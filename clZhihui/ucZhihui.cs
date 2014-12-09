using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Styles;
using MapInfo.Windows.Dialogs;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Geometry ;
using System.Net.Sockets ;
using MapInfo.Tools;
using System.Collections;
using System.IO;

namespace clZhihui
{
    public partial class ucZhihui :UserControl
    {
        private MapInfo.Windows.Controls.MapControl mapControl1;
        private string[] StrCon;                  // 数据库连接字符组
        private string strConn;                   // 数据库连接字符串
        public string StrRegion;                  // 派出所权限 
        public string StrRegion1 = "";            // 中队权限
        public string User = "";                  // 用户登陆名称 
        public string GetFromNamePath = string.Empty; // GetNameFromPath 的文件位置

        public System.Data.DataTable dtExcel;     // 地图工具栏导出

        private static int _videoPort;            // 通讯端口
        private static string[] _videoString;     // 视频连接字符
        private static ToolStripLabel _st;        // 状态栏
        private static NetworkStream _ns;         //
        private static ToolStripDropDownButton _tdb; // 工具栏菜单
        private  Boolean _vf;                     // 通讯是否已经连接的标识 
        private static string _vEpath = string.Empty;// 视频监控客户端所在路径　

        public ToolStripProgressBar toolPro;      // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolProLbl;         // 用于显示进度文本　
        public ToolStripSeparator toolProSep;
        public Panel panError;                    // 用于弹出错误提示
        public string plicNo = string.Empty;      // 用于存储与110关联值  lili 2010-11-25

        /// <summary>
        /// 模块初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="m">地图控件</param>
        /// <param name="s">访问数据库参数</param>
        /// <param name="tools"></param>
        /// <param name="td"></param>
        /// <param name="port"></param>
        /// <param name="vs"></param>
        /// <param name="videopath"></param>
        public ucZhihui(MapInfo.Windows.Controls.MapControl m, string[] s, ToolStripLabel tools, ToolStripDropDownButton td, int port, string[] vs, string videopath,string getfromnamepath)
        {
            InitializeComponent();
            try
            {
                mapControl1 = m;
                StrCon = s;
                strConn = "data source =" + StrCon[0] + ";user id =" + StrCon[1] + ";password=" + StrCon[2];
                _tdb = td;
                _videoPort = port;
                _videoString = vs;
                _st = tools;
                _vEpath = videopath;
                GetFromNamePath = getfromnamepath;

                SetParmeterAnsEvents();

                comboBoxField.Text = comboBoxField.Items[0].ToString();
            
                this.comboBoxField.Items.Clear();
                this.comboBoxField.Items.Add("报警编号");
                this.comboBoxField.Items.Add("案件类型");
                this.comboBoxField.Items.Add("案别_案由");
                this.comboBoxField.Items.Add("发案地点详址");
                this.comboBoxField.Items.Add("所属派出所");
                this.comboBoxField.Items.Add("所属中队");
                this.comboBoxField.Items.Add("所属警务室");
                this.comboBoxField.Text = this.comboBoxField.Items[0].ToString();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "01-模块初始化");
            }
        }

        /// <summary>
        /// 添加圈选、框选、任选三个事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void SetParmeterAnsEvents()
        {
            try
            {
                mapControl1.Tools.Used += Tools_Used;
                mapControl1.Tools.FeatureAdded += Tools_FeatureAdded;
                mapControl1.Tools.FeatureSelected += Tools_FeatureSelected;

                this.mapControl1.Tools.Add("SelByRect", new CustomRectangleMapTool(true, true, true, mapControl1.Viewer,mapControl1.Handle.ToInt32(), 
                                                                                                     mapControl1.Tools, mapControl1.Tools.MouseToolProperties,
                                                                                                     mapControl1.Tools.MapToolProperties));
                this.mapControl1.Tools.Add("SelByCircle", new CustomCircleMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), 
                                                                                                    mapControl1.Tools, mapControl1.Tools.MouseToolProperties,
                                                                                                    mapControl1.Tools.MapToolProperties));
                this.mapControl1.Tools.Add("SelByPolygon", new CustomPolygonMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), 
                                                                                                      mapControl1.Tools, mapControl1.Tools.MouseToolProperties,
                                                                                                      mapControl1.Tools.MapToolProperties));
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetParmeterAnsEvents");
            }
        }

        /// <summary>
        /// 初始化直观指挥
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        public void InitZhihui()
        {
            try
            {
                CreateTemLayer("直观指挥");
                SetLayerEdit("直观指挥");
                CreateTemLayer("查看选择");
                SetDrawStyle();
                if (tabControl1.SelectedTab == tabYuan)
                {
                    RefreshYuan();
                }

                // 判断plicNo是否有值，从而来判断是否是从110关联过来
                if (this.plicNo != string.Empty)
                {
                    this.comboBoxField.Text = this.comboBoxField.Items[0].ToString();
                    this.CaseSearchBox.Text = this.plicNo;
                    caseSearch();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitZhihui-初始化直观指挥");
            }
         
        }


        /// <summary>
        /// 创建临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
            catch (Exception ex) { ExToLog(ex, "CreateTemLayer"); }
        }

        /// <summary>
        /// 设置当前图层可编辑
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="layername"></param>
        private void SetLayerEdit(string layername)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layername], true);
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layername], true);
            }
            catch (Exception ex) { ExToLog(ex, "SetLayerEdit"); }
        }

        /// <summary>
        /// 图元添加
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        void Tools_FeatureAdded(object sender, MapInfo.Tools.FeatureAddedEventArgs e)
        {
            if (!this.Visible)
            {
            }
            else
            {
                try
                {
                    Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("直观指挥",
                                                                                         MapInfo.Data.SearchInfoFactory.
                                                                                             SearchWhere(
                                                                                                 "MapID is null or MapID=0 "));
                    switch (e.Feature.Type)
                    {
                        case MapInfo.Geometry.GeometryType.Point:
                            if (tabControl1.SelectedTab == tabYuan)
                            {
                                planX = e.MapCoordinate.x;
                                planY = e.MapCoordinate.y;
                                planLayer();
                                clearFeatures("直观指挥");
                            }
                            else
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
                    ft["MapID"] = 1;
                    ft.Update();
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Tools_FeatureAdded-图元添加");
                }
            }
        }


        private Table planTable;
        /// <summary>
        /// 创建预演图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void planLayer()
        {
            try
            {
                if (planTable != null)
                {
                    planTable.Close();
                }

                TableInfoMemTable ti = new TableInfoMemTable("预演图层");
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
                column.Alias = "预演名称";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                planTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);

                double dx = 0, dy = 0;

                dx = planX;
                dy = planY;

                if (dx > 0 || dy > 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), dx, dy);

                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 9));
                    cs = new CompositeStyle(new SimpleVectorPointStyle(34, Color.Red, 9));

                    Feature pFeat = new Feature(planTable.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["预演名称"] = this.textPlan.Text;
                    planTable.InsertFeature(pFeat);
                }

                currentFeatureLayer = new FeatureLayer(planTable, "预演图层");

                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                labeLayer(planTable, "预演名称");
                try
                {
                    DPoint dP = new DPoint(dx, dy);
                    CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();

                    mapControl1.Map.SetView(dP, cSys, 6000);
                    mapControl1.Map.Center = dP;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "设定视图范围");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "planLayer");
            }
        }

        AreaStyle aStyle;
        BaseLineStyle lStyle;
        TextStyle tStyle;
        BasePointStyle pStyle;

        /// <summary>
        /// 设置绘制要素的默认样式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                pStyle = new BitmapPointStyle("jc.bmp", BitmapStyles.None, Color.Blue, 14);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetDrawStyle-设置绘制要素的默认样式");
            }
        }

        private bool isDel;
        /// <summary>
        /// 地图工具
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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

            }
            catch (Exception ex)
            {
                ExToLog(ex, "mapToolBar1_ButtonClick-地图工具");
            }
            finally { mapControl1.Map.Center = mapControl1.Map.Center; }
        }

        /// <summary>
        /// 点击工具栏上的工具时，对工具按钮进行设置，由于按钮分组，iFrom表示组的首Index，iEnd表示末Index
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void UncheckedTool()
        {
            try
            {
                for (int i = 0; i < 12; i++)
                {
                    if (mapToolBar1.Buttons[i].Pushed)
                    {
                        mapToolBar1.Buttons[i].Pushed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "UncheckedTool-05-工具按钮进行设置");
            }
        }

  
        /// <summary>
        /// 图元选择
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void Tools_FeatureSelected(object sender, MapInfo.Tools.FeatureSelectedEventArgs e)
        {
            if (isDel == false) return;
            if (!this.Visible)
            {
                return;
            }
            else
            {
                try
                {
                    FeatureLayer fLayer = mapControl1.Map.Layers["直观指挥"] as FeatureLayer;
                    Table table = fLayer.Table;

                    //IResultSetFeatureCollection fc= MapInfo.Engine.Session.Current.Catalog.Search(table.Alias, MapInfo.Data.SearchInfoFactory.(e.Selection.Envelope,mapControl1.Map.FeatureCoordSys, ContainsType.Geometry));
                    IResultSetFeatureCollection fc = e.Selection[table];
                    foreach (Feature f in fc)
                    {
                        table.DeleteFeature(f);
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Tools_FeatureSelected-06-图元选择");
                }
            }
        }

        /// <summary>
        /// 获取网络变量
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ns1">网络流</param>
        /// <param name="vf1">连接成功标识符</param>
        public void getNetParameter(NetworkStream ns1, Boolean vf1)
        {
            try
            {
                _ns = ns1;
                this._vf = vf1;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getNetParameter-获取网络变量");
            }
        }

        int pageSize = 0;      // 每页显示行数
        int nMax = 0;          // 总记录数
        int pageCount = 0;     // 页数＝总记录数/每页显示行数
        int pageCurrent = 0;   // 当前页号
        int nCurrent = 0;      // 当前记录行

        //---------分页用全局变量------
        int startNo = 1;   // 开始的行数
        int endNo = 0;　　 // 结束的行数
        //----------------------------
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            caseSearch();
        }

        private frmPro pro = null;
        /// <summary>
        /// 模糊查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void caseSearch()
        {
            if (this.CaseSearchBox.Text == "")
            {
                MessageBox.Show(@"请输入关键词");
                return;
            }
            try
            {
                //isShowPro(true);
                pro = new frmPro();
                pro.Show();
                dataGridView1.Rows.Clear();
                string countSql = "select count(*) from GPS110.报警信息110 where GPS110.报警信息110." + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%'";
                getMaxCount(countSql);
                InitDataSet(RecordCount);

                //string strExpress = "select * from GPS110.报警信息110 where GPS110.报警信息110." + comboBoxField.Text + " like '%" + ToDBC(CaseSearchBox.Text) + "%' order by GPS110.报警信息110.报警编号 desc";
                //DataTable dataTable = GetTable(strExpress); 

                endNo = Convert.ToInt32(this.TextNum.Text);
                startNo = 1;

                //LoadData(startNo, endNo)
                DataTable dataTable = LoadData(endNo, 1, true); 
                pro.progressBar1.Maximum = 3;
                pro.progressBar1.Value = 1;
                //this.toolPro.Value = 1;
                Application.DoEvents();

                if (dataTable == null || dataTable.Rows.Count < 1)
                {
                    pro.Close();
                    // isShowPro(false);
                    MessageBox.Show(@"无查询结果！");
                }
                else
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add(i + 1, dataTable.Rows[i]["发案地点详址"], "详情...", dataTable.Rows[i]["报警编号"], dataTable.Rows[i]["X"], dataTable.Rows[i]["Y"],dataTable.Rows[i]["案件类型"]);
                        if (i % 2 == 1)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    //设置宽度
                    setDataGridViewColumnWidth(dataGridView1);
                    insertQueryIntoMap(dataTable);
                    WriteEditLog("案件选择", "报警系统", countSql, "查询");
                }
                pro.progressBar1.Value = 2;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                }
                //this.toolPro.Value = 3;
                pro.progressBar1.Value = 3;
                Application.DoEvents();
                pro.Close();
                //isShowPro(false);
                //this.toolPro.Value = 2;
                //Application.DoEvents();
            }
            catch (Exception ex)
            {
                pro.Close();
                //isShowPro(false);
                ExToLog(ex, "caseSearch-08-模糊查询");
            }         
        }

        /// <summary>
        /// 查询结果
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="_startNo">每页显示数</param>
        /// <param name="_endNo">第几页</param>
        /// <param name="isFrist">当前查的是否是第一页</param>
        /// <returns>结果集</returns>
        private DataTable LoadData(int xiShu, int yeShu,bool isFrist)
        {
            try
            {
                DataTable dt = new DataTable();
      
                string sql = "";
                if (isFrist)
                    sql = "select * from (select * from gps110.报警信息110 where " + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%' order by 报警编号 desc) t where rownum <= " + xiShu;
                else
                    sql = "select * from (select w.*,rownum rn from gps110.报警信息110 w where rownum <" + yeShu + "*" + xiShu + " and " + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%' order by 报警编号 desc) t where rn <= " + yeShu + "*" + xiShu + " and rn > (" + yeShu + "-1)*" + xiShu;

                WriteLog(sql);

                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

                PageNow.Text = Convert.ToString(pageCurrent);
                PageCount.Text = "/" + pageCount.ToString();

                if (dtExcel != null) dtExcel.Clear();
                dtExcel = dt;

                return dt;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "LoadData-查询数据");
                return null;
            }
        }

        /// <summary>
        /// 测试日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        private void WriteLog(string sql)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\TestSqlLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine(sql);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "WriteLog");
            }
        }

        /// <summary>
        /// 查询结果
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="_startNo">开始行数</param>
        /// <param name="_endNo">结束行数</param>
        /// <returns>结果集</returns>
        private DataTable LoadData(int _startNo,int _endNo)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from (select rownum as rn1,a.* from GPS110.报警信息110 a where rownum<=" + _endNo + " and a." + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%') where rn1 >=" + _startNo + " order by 报警编号 desc";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                PageNow.Text = Convert.ToString(pageCurrent);
                PageCount.Text = "/" + pageCount.ToString();
                if (dtExcel != null) dtExcel.Clear();
                dtExcel = dt;
                return dt;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "LoadData-查询数据");
                return null;
            }
        }

        /// <summary>
        /// 得到最大的值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">sql语句</param>
        private void getMaxCount(string sql)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();

                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                nMax = Convert.ToInt32(Cmd.ExecuteScalar().ToString());
                Cmd.Dispose();
                Conn.Close();
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                System.Windows.Forms.MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nMax = 0;
            }
        }

        /// <summary>
        /// 初始化分页工具
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tsLabel">显示当前记录条数</param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                pageSize = Convert.ToInt32(this.TextNum.Text);      //设置页面行数
                TextNum.Text = pageSize.ToString();
                tsLabel.Text = nMax.ToString() + "条";              //在导航栏上显示总记录数
                pageCount = (nMax / pageSize);                      //计算出总页数
                if ((nMax % pageSize) > 0) pageCount++;
                if (nMax != 0)
                {
                    pageCurrent = 1;
                }
                nCurrent = 0;       //当前记录数从0开始
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Table queryTable;
        FeatureLayer currentFeatureLayer;
        /// <summary>
        /// 将查询结果表示在地图上
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="datatable">要处理的数据源</param>
        private void insertQueryIntoMap(DataTable datatable)
        {
            try
            {
                if (queryTable != null)
                {
                    queryTable.Close();

                }
                if (datatable == null)
                {
                    return;
                }
                TableInfoMemTable ti = new TableInfoMemTable("案件选择");
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
                column.Alias = "表名";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                queryTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);

                double dx = 0, dy = 0;
                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    try
                    {
                        dx = Convert.ToDouble(datatable.Rows[i]["X"]);
                        dy = Convert.ToDouble(datatable.Rows[i]["Y"]);
                    }
                    catch
                    {
                        continue;
                    }
                    if (dx <= 0 || dy <= 0) continue;
                    FeatureGeometry pt = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), dx, dy);
                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, Color.Red, 9));

                    Feature pFeat = new Feature(queryTable.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["表_ID"] = datatable.Rows[i]["报警编号"].ToString();
                    pFeat["表名"] = "报警系统";
                    queryTable.InsertFeature(pFeat);
                }

                currentFeatureLayer = new FeatureLayer(queryTable, "案件选择");
                
                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                labeLayer(queryTable,"表_ID");
                //this.setLayerStyle(currentFeatureLayer, Color.Red, 34, 10);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "insertQueryIntoMap-09-将查询结果显示在地图上");
                //writeToLog(ex,"insertQueryIntoMap");
            }
        }

        /// <summary>
        /// 修改要素的显示样式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="featureLayer">修改要素</param>
        /// <param name="color">颜色</param>
        /// <param name="code"></param>
        /// <param name="size">大小</param>
        private void setLayerStyle(MapInfo.Mapping.FeatureLayer featureLayer, Color color, short code, int size)//设置点的样式
        {
            try
            {
                MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(code, color, size);

                MapInfo.Styles.CompositeStyle comStyle = new MapInfo.Styles.CompositeStyle();
                comStyle.SymbolStyle = pStyle;
                MapInfo.Mapping.FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, comStyle);
                featureLayer.Modifiers.Append(fsm);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setLayerStyle-10-修改要素的显示样式");
            }
        }

        /// <summary>
        /// 创建图层标注
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="editTable">图层名称</param>
        /// <param name="field">标注字段</param>
        private void labeLayer(Table editTable,string field)//创建标注
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
                ExToLog(ex, "labeLayer-11-创建图层标注");
            }
        }

        //  项目---顺德公安警用GIS系统
        //  模块---直观指挥中的视频联动模块
        //  zhangjie 2008.12.3        //  

       /// <summary>
        /// 周边查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
       /// </summary>
       /// <param name="dp">定位点</param>
       /// <param name="distCar">车辆查询半径</param>
       /// <param name="carflag">车辆查询标识</param>
       /// <param name="distVideo">视频查询半径</param>
       /// <param name="videoflag">视频查询标识</param>
        private void SearchDistance(DPoint dp, double distCar, bool carflag,double distVideo, bool videoflag)
        {
            try
            {
                // 创建 视频图层             
                if (videoflag)
                {
                    clVideo.ucVideo fv = new clVideo.ucVideo(mapControl1, _st, _tdb, StrCon, _videoPort, _videoString, _vEpath, true, false);
                    fv.getNetParameter(_ns, _vf);
                    fv.strRegion = this.StrRegion;
                    fv.SearchVideoDistance(dp, distVideo,"直观指挥");
                }

                // 创建车辆图层
                if (carflag)
                {
                    clCar.ucCar fcar = new clCar.ucCar(mapControl1, null, StrCon, true);
                    fcar.strRegion = StrRegion;
                    fcar.SearchCarDistance(dp, distCar);
                    fcar.ZhiHui = false;
                }

                if (!videoflag && !carflag)
                {
                    //return;
                }

                Distance dt = new Distance();
                if (distCar >= distVideo)
                {
                    dt = new Distance(distCar, DistanceUnit.Meter);
                }
                else
                {
                    dt = new Distance(distVideo, DistanceUnit.Meter);
                }

                this.mapControl1.Map.SetView(dp, this.mapControl1.Map.GetDisplayCoordSys(), dt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SearchDistance-12-周边查询");
            }
        }

        /// <summary>
        /// 切换功能模块
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.panError.Visible = false;    // 隐藏错误提示

                if (tabControl1.SelectedTab == tabLiandong)
                {
                    this.panel3.Visible = false;
                }

                if (tabControl1.SelectedTab == tabYuan)
                {
                    if (dataGridView1.CurrentCell == null && this.panel3.Visible == false)
                    {
                        if (MessageBox.Show("您没有选择案件，您是否要自定义案件进行预演！", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.Cancel)
                        {
                            return;
                        }
                        else
                        {
                            this.panel3.Visible = true;
                        }
                    }
                    RefreshYuan();
                }
                if (tabControl1.SelectedTab != tabBaiban)
                {
                    isDel = false;
                    mapControl1.Tools.LeftButtonTool = "Select";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tabControl1_SelectedIndexChanged-13-切换功能模块");
            }
        }

        /// <summary>
        /// 更新预案列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void RefreshYuan()
        {
            this.dataGridView2.Rows.Clear();
            RunCommand("Update 预案 set ID=rownum");
           try
            {
               
                string strExpress = "select * from 预案";
                DataTable dataTable = GetTable(strExpress);
                if (dataTable.Rows.Count < 1 || dataTable == null)
                {
                    MessageBox.Show("无查询结果！");
                }
                else
                {
                    DataGridViewButtonCell btn = new DataGridViewButtonCell();
                    DataGridViewButtonCell btn2 = new DataGridViewButtonCell();
                    DataGridViewButtonCell btn3 = new DataGridViewButtonCell();
                    btn.ToolTipText = "启用对应预案";
                    btn2.ToolTipText = "修改预案内容...";
                    btn3.ToolTipText = "删除当前预案";
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataGridView2.Rows.Add(dataTable.Rows[i]["ID"], dataTable.Rows[i]["名称"], btn, btn2, btn3);
                        dataGridView2.Rows[i].Height = 30;
                    }

                    //设置宽度
                    setDataGridViewColumnWidth(dataGridView2);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "RefreshYuan-14-更新预案列表");
            }
       
            //设置宽度
            setDataGridViewColumnWidth(dataGridView2);
        }


        /// <summary>
        /// 添加预案
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            frmCustomYuAn fYuan = new frmCustomYuAn();
            fYuan.Text = @"自定义预案";
            fYuan.buttonAdd.Text = @"添加";

            if (fYuan.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            else
            {
                try
                {
                    int[] disArr = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0};

                    for (int i = 0; i < fYuan.texts.Length; i++)
                    {
                        if (fYuan.texts[i].Text != "")
                        {
                            disArr[i] = Convert.ToInt32(ToDBC(fYuan.texts[i].Text));
                        }
                    }

                    string id = GetID();

                    string strExpress = "insert into 预案(ID,名称,警车范围,摄像头范围,";
                    strExpress += "警员范围,治安卡口范围,";
                    strExpress += "基层派出所范围,社区警务室范围,公共场所范围,";
                    strExpress += "网吧范围,安全防护单位范围,特种行业范围,";
                    strExpress += "消防重点单位范围,基层民警中队范围,消防栓范围) values('" + id + "','" + ToDBC(fYuan.textYuanName.Text) + "'";
                    for (int i = 0; i < 13; i++)
                    {
                        strExpress += "," + disArr[i];
                    }
                    strExpress += ")";

                    RunCommand(strExpress);
                   
                    RefreshYuan();
                    WriteEditLog("预案管理", "预案", "名称=" + ToDBC(fYuan.textYuanName.Text), "添加预案");
                    MessageBox.Show(@"添加成功!");
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "buttonAdd_Click-15-添加预案");
                }
            }
        }


        /// <summary>
        /// 操作预案
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            #region 启用预案

            if (dataGridView2.Columns[e.ColumnIndex].Name == "colAct")
            {
                mapControl1.Tools.LeftButtonTool = "Select";
                if (dataGridView1.CurrentCell != null)
                {
                    try
                    {
                        planX = Convert.ToDouble(dataGridView1["ColX", dataGridView1.CurrentCell.RowIndex].Value);
                        planY = Convert.ToDouble(dataGridView1["ColY", dataGridView1.CurrentCell.RowIndex].Value);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("案件坐标不明确，不能实现预演！", "提示");
                        ExToLog(ex, "dataGridView2_CellContentClick");
                    }
                }
                int sIndex = e.RowIndex;
                startPlan(planX, planY, sIndex);
            }
            #endregion

            #region 查看和修改预案
            if (dataGridView2.Columns[e.ColumnIndex].Name == "colAlter")
            {
                mapControl1.Tools.LeftButtonTool = "Select";
                OracleConnection Conn = new OracleConnection(strConn);
                try
                {
                    Conn.Open();

                    //选择预案

                    //checks = new CheckBox[12] { checkCar, checkVideo, checkJY, checkZAKK, checkPCS, checkJWS, checkGGCS, checkWB, checkAF, checkTZHY, checkXF, checkMJZD };
                    //texts = new TextBox[12] { textCarDis, textVideoDis, textJY, textZAKK, textPCS, textJWS, textGGCS, textWB, textAF, textTZHY, textXF, textMJZD };

                    string id;
                    string strExpress = "select t.名称,t.警车范围,t.摄像头范围,";
                    strExpress += "t.警员范围,t.治安卡口范围,";
                    strExpress += "t.基层派出所范围,t.社区警务室范围,t.公共场所范围,";
                    strExpress += "t.网吧范围,t.安全防护单位范围,t.特种行业范围,";
                    strExpress += "t.消防重点单位范围,t.基层民警中队范围,t.消防栓范围,";
                    strExpress += "t.ID from 预案 t where t.ID='" + dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString() + "'";
                    OracleCommand cmd = new OracleCommand(strExpress, Conn);

                    OracleDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        id = dr["ID"].ToString();
                        frmCustomYuAn fYuan = new frmCustomYuAn();
                        fYuan.Text = "预案修改";
                        fYuan.buttonAdd.Text = "更改";
                        fYuan.textYuanName.Text = dr["名称"].ToString();

                        for (int i = 1; i < 14; i++)
                        {
                            if (Convert.ToDouble(dr[i]) != 0)
                            {
                                fYuan.checks[i-1].Checked = true;
                                fYuan.texts[i-1].Text = dr[i].ToString();
                            }
                            else
                            {
                                fYuan.checks[i-1].Checked = false;
                            }
                        }

                        dr.Close();

                        if (fYuan.ShowDialog() == DialogResult.OK)
                        {
                            strExpress = "update 预案 t set ";
                            strExpress += "名称='" + ToDBC(fYuan.textYuanName.Text) + "',";

                            if (fYuan.checks[0].Checked)
                            {
                                strExpress += "警车范围=" + Convert.ToInt32(fYuan.texts[0].Text) + ",";
                            }
                            else
                            {
                                strExpress += "警车范围=0,";
                            }

                            if (fYuan.checks[1].Checked)
                            {
                                strExpress += "摄像头范围=" + Convert.ToInt32(fYuan.texts[1].Text) + ",";
                            }
                            else
                            {
                                strExpress += "摄像头范围=0,";
                            }

                            if (fYuan.checks[2].Checked)
                            {
                                strExpress += "警员范围=" + Convert.ToInt32(fYuan.texts[2].Text) + ",";
                            }
                            else
                            {
                                strExpress += "警员范围=0,";
                            }

                            if (fYuan.checks[3].Checked)
                            {
                                strExpress += "治安卡口范围=" + Convert.ToInt32(fYuan.texts[3].Text) + ",";
                            }
                            else
                            {
                                strExpress += "治安卡口范围=0,";
                            }

                            if (fYuan.checks[4].Checked)
                            {
                                strExpress += "基层派出所范围=" + Convert.ToInt32(fYuan.texts[4].Text) + ",";
                            }
                            else
                            {
                                strExpress += "基层派出所范围=0,";
                            }

                            if (fYuan.checks[5].Checked)
                            {
                                strExpress += "社区警务室范围=" + Convert.ToInt32(fYuan.texts[5].Text) + ",";
                            }
                            else
                            {
                                strExpress += "社区警务室范围=0,";
                            }

                            if (fYuan.checks[6].Checked)
                            {
                                strExpress += "公共场所范围=" + Convert.ToInt32(fYuan.texts[6].Text) + ",";
                            }
                            else
                            {
                                strExpress += "公共场所范围=0,";
                            }

                            if (fYuan.checks[7].Checked)
                            {
                                strExpress += "网吧范围=" + Convert.ToInt32(fYuan.texts[7].Text) + ",";
                            }
                            else
                            {
                                strExpress += "网吧范围=0,";
                            }

                            if (fYuan.checks[8].Checked)
                            {
                                strExpress += "安全防护单位范围=" + Convert.ToInt32(fYuan.texts[8].Text) + ",";
                            }
                            else
                            {
                                strExpress += "安全防护单位范围=0,";
                            }

                            if (fYuan.checks[9].Checked)
                            {
                                strExpress += "特种行业范围=" + Convert.ToInt32(fYuan.texts[9].Text) + ",";
                            }
                            else
                            {
                                strExpress += "特种行业范围=0,";
                            }

                            if (fYuan.checks[10].Checked)
                            {
                                strExpress += "消防重点单位范围=" + Convert.ToInt32(fYuan.texts[10].Text) + ",";
                            }
                            else
                            {
                                strExpress += "消防重点单位范围=0,";
                            }

                            if (fYuan.checks[11].Checked)
                            {
                                strExpress += "基层民警中队范围=" + Convert.ToInt32(fYuan.texts[11].Text);
                            }
                            else
                            {
                                strExpress += "基层民警中队范围=0,";
                            }

                            if (fYuan.checks[12].Checked)
                            {
                                strExpress += "消防栓范围=" + Convert.ToInt32(fYuan.texts[12].Text);
                            }
                            else
                            {
                                strExpress += "消防栓范围=0";
                            }

                            strExpress += " where ID='" + id + "'";

                            try
                            {
                                RunCommand(strExpress);
                                RefreshYuan();
                                MessageBox.Show("预案更改成功!");
                                WriteEditLog("预案管理", "预案", "名称" + ToDBC(fYuan.textYuanName.Text), "查看和修改预案");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("更改失败,请查看log文件,修复错误");
                                ExToLog(ex, "16-修改预案");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "16-查看预案");
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                }
            }
#endregion

            #region 删除预案
            if (dataGridView2.Columns[e.ColumnIndex].Name == "ColDel")
            {
                mapControl1.Tools.LeftButtonTool = "Select";
                try
                {
                    string strExpress = "delete from 预案 where 名称='" + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() + "'";
                    RunCommand(strExpress);
                    RefreshYuan();
                    WriteEditLog("预案管理", "预案", "名称" + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString(), "删除预案");
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "16-删除预案");
                }
            }
            #endregion
        }

        /// <summary>
        /// 根据坐标点查找预案所设定范围内的要素
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="pX">X坐标</param>
        /// <param name="pY">Y坐标</param>
        /// <param name="sIndex">记录行行数</param>
        private void startPlan(double pX, double pY,int sIndex)
        {
            if (pX == 0 || pY == 0)
            {
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                double carDis = 0, videoDis = 0;
                bool checkCar = false, checkVideo = false;
                Conn.Open();

                //选择预案
                string strExpress = "select * from 预案 where 名称='" + dataGridView2.Rows[sIndex].Cells[1].Value.ToString() + "'";
                OracleCommand cmd = new OracleCommand(strExpress, Conn);

                OracleDataReader dr = cmd.ExecuteReader();
                clearFeatures("直观指挥");
                StopVideo();
                if (dr.HasRows)
                {
                    dr.Read();
                    carDis = Convert.ToDouble(dr["警车范围"]);
                    videoDis = Convert.ToDouble(dr["摄像头范围"]);

                    if (carDis != 0)
                    {
                        checkCar = true;
                    }
                    if (videoDis != 0)
                    {
                        checkVideo = true;
                    }

                    if (Convert.ToDouble(dr["警员范围"]) != 0)
                    {
                        insertPointsIntoMap("警员", Convert.ToDouble(dr["警员范围"]));
                    }

                    if (Convert.ToDouble(dr["治安卡口范围"]) != 0)
                    {
                        insertPointsIntoMap("治安卡口", Convert.ToDouble(dr["治安卡口范围"]));
                    }

                    if (Convert.ToDouble(dr["基层派出所范围"]) != 0)
                    {
                        insertPointsIntoMap("基层派出所", Convert.ToDouble(dr["基层派出所范围"]));
                    }

                    if (Convert.ToDouble(dr["社区警务室范围"]) != 0)
                    {
                        insertPointsIntoMap("社区警务室", Convert.ToDouble(dr["社区警务室范围"]));
                    }

                    if (Convert.ToDouble(dr["公共场所范围"]) != 0)
                    {
                        insertPointsIntoMap("公共场所", Convert.ToDouble(dr["公共场所范围"]));
                    }

                    if (Convert.ToDouble(dr["网吧范围"]) != 0)
                    {
                        insertPointsIntoMap("网吧", Convert.ToDouble(dr["网吧范围"]));
                    }

                    if (Convert.ToDouble(dr["安全防护单位范围"]) != 0)
                    {
                        insertPointsIntoMap("安全防护单位", Convert.ToDouble(dr["安全防护单位范围"]));
                    }
                    if (Convert.ToDouble(dr["特种行业范围"]) != 0)
                    {
                        insertPointsIntoMap("特种行业", Convert.ToDouble(dr["特种行业范围"]));
                    }
                    if (Convert.ToDouble(dr["消防重点单位范围"]) != 0)
                    {
                        insertPointsIntoMap("消防重点单位", Convert.ToDouble(dr["消防重点单位范围"]));
                    }
                    if (Convert.ToDouble(dr["基层民警中队范围"]) != 0)
                    {
                        insertPointsIntoMap("基层民警中队", Convert.ToDouble(dr["基层民警中队范围"]));
                    }
                    if (Convert.ToDouble(dr["消防栓范围"]) != 0)
                    {
                        insertPointsIntoMap("消防栓", Convert.ToDouble(dr["消防栓范围"]));
                    }
                    dr.Close();
                    cmd.Dispose();
                    WriteEditLog("预案管理", "预案", "名称=" + dataGridView2.Rows[sIndex].Cells[1].Value.ToString(), "启用预案");
                }
                else
                {
                    MessageBox.Show("预案错误,可能刚被他人删除!");
                    this.Cursor = Cursors.Default;
                    return;
                }

                //选择案件坐标
                DPoint dp = new DPoint();

                dp.x = pX;
                dp.y = pY;

                SearchDistance(dp, carDis, checkCar, videoDis, checkVideo); //查询周边视频和车辆
            }
            catch (Exception ex)
            {
                ExToLog(ex, "startPlan-16-启动预案");
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                //clearFeatures("直观指挥");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 移除视频图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void StopVideo()
        {
            try
            {
                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "StopVideo");
            }
        }

        /// <summary>
        /// 获取最大ID
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="Columname">列名</param>
        /// <returns>最大ID数</returns>
        private string GetID()
        {
            string id = string.Empty;
            try
            {
                string sql = "Select max(ID)+1 from 预案";
                id = GetScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetID-17-获取最大ID");
            }
            return id;
        }

        /// <summary>
        /// 显示图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sName">表名关键字</param>
        /// <param name="dis">显示范围</param>
        private void insertPointsIntoMap(string sName,double dis)
        {
            CLC.ForSDGA.GetFromTable.GetFromName(sName,GetFromNamePath);

            string tabName = CLC.ForSDGA.GetFromTable.TableName;

            double x = planX;
            double y = planY;  

            double x1, x2;
            double y1, y2;
            x1 = x - dis;
            x2 = x + dis;
            y1 = y - dis;
            y2 = y + dis;
            string sql = "select * from " + tabName + " where x>" + x1 + " and x<" + x2 + " and y>" + y1 + " and y<" + y2; ;

            try
            {              
                DataTable dt = GetTable(sql);
                if (dt == null || dt.Rows.Count < 1)
                {
                    return;
                }
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["直观指挥"];
                Table tableTem = fl.Table;

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

                //先清除已有对象
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    double dx = Convert.ToDouble(dt.Rows[i]["X"]);
                    double dy = Convert.ToDouble(dt.Rows[i]["Y"]);
                    if (calDisTwoPoints(x, y, dx, dy) <= dis)//查找出来的是方形, 距离可能不对,再次判断
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);

                        Feature pFeat = new Feature(tableTem.TableInfo.Columns);
                        pFeat.Geometry = pt;
                        pFeat.Style = cs;
                        pFeat["表_ID"] = dt.Rows[i][CLC.ForSDGA.GetFromTable.ObjID].ToString();
                        pFeat["表名"] = sName;

                        tableTem.InsertFeature(pFeat);
                    }
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "insertPointsIntoMap-18-显示图元");
            }
        }

        /// <summary>
        /// 将经纬度转成弧度
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <returns></returns>
        private double calDisTwoPoints(double X1, double Y1, double X2, double Y2)
        {
            double d=0;
            try
            {
                X1 = X1 / 180 * Math.PI;
                Y1 = Y1 / 180 * Math.PI;
                X2 = X2 / 180 * Math.PI;
                Y2 = Y2 / 180 * Math.PI;
                d = Math.Sin(Y1) * Math.Sin(Y2) + Math.Cos(Y1) * Math.Cos(Y2) * Math.Cos(X1 - X2);
                d = Math.Acos(d) * 6371004;
                d = Math.Round(d);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "calDisTwoPoints-19-将经纬度转成弧度");
            }
            return d;
        }

        /// <summary>
        /// 窗体大小改变时发生
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary
        private void dataGridView2_Resize(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView2);
                }
                else
                {
                    dataGridView1.Columns[1].Width = dataGridView2.Width - 115;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "20-dataGridView2_Resize");
            }
        }

        /// <summary>
        /// 窗体大小改变时发生
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary
        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView1);
                }
                else
                {
                    dataGridView1.Columns[1].Width = dataGridView1.Width - 145;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "21-dataGridView1_Resize");
            }
        }

        /// <summary>
        /// 设置列表宽度
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dataGridView">要设置的控件</param>
        private void setDataGridViewColumnWidth(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.Rows[0].Height * dataGridView.Rows.Count > dataGridView.Height)
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 160;
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 145;
                }
                if (dataGridView == dataGridView2)
                {
                    dataGridView.Columns[1].Width -= 15;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setDataGridViewColumnWidth-22-设置列表宽度");
            }
        }

        /// <summary>
        /// 窗体可见性切换
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void ucZhihui_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    if (this.queryTable != null)
                    {
                        try
                        {
                            this.queryTable.Close();
                            this.queryTable = null;
                        }
                        catch { }

                    }
                    mapControl1.Tools.LeftButtonTool = "Select";
                    dataGridView1.Rows.Clear();
                    dataGridView2.Rows.Clear();
                    RemoveTemLayer("直观指挥");
                    RemoveTemLayer("查看选择");
                    RemoveTemLayer("预演图层");
                    
                    RemoveTemLayer("VideoLayer");
                    RemoveTemLayer("VideoCarLayer");
                    RemoveTemLayer("CarLayer");
                    
                    closeAllTables();
                }
                else
                {
                    FeatureLayer fl = mapControl1.Map.Layers["查看选择"] as FeatureLayer;

                    labeLayer(fl.Table, "名称");
                }
                isDel = false;
                this.panError.Visible = false;    // 隐藏错误提示
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ucZhihui_VisibleChanged-23-窗体可见性切换");
            }
        }

        /// <summary>
        /// 创建新表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tabAlias">表名</param>
        private void openTable(string tabAlias)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,GetFromNamePath);
                string strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName;

                if (tabAlias == "车载视频")
                {
                    strSQL = "select CAMID as 设备编号, 终端车辆号牌 as 设备名称,所属单位 as 所属派出所,null as 日常管理人,null as MAPID,null as 设备ID, X,Y from gps警车定位系统 where CAMID is not null and X>0 and Y >0 ";
                }
                if (tabAlias == "社会视频")
                {
                    strSQL = "select * from 视频位置";
                }
                if (tabAlias == "非社会视频") 
                {
                    strSQL = "select * from 视频位置非社会";
                }

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
                ExToLog(ex, "openTable-24-创建新表");
            }
        }

        private string[] tableNames = new string[] { "公共场所", "安全防护单位", "网吧", "治安卡口", "特种行业", "社会视频", "非社会视频", "车载视频", "消防栓", "消防重点单位", "警车", "基层派出所", "警员", "基层民警中队", "社区警务室" };
        /// <summary>
        /// 关闭打开的点选层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void closeAllTables()
        {
            try
            {
                for (int i = 0; i < tableNames.Length; i++)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable(tableNames[i]) != null) {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(tableNames[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "closeAllTables-25-关闭打开的点选层");
            }
        }

        /// <summary>
        /// 移除临时图层,关闭表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableAlies">图层名称</param>
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
            catch(Exception ex)
            {
                ExToLog(ex, "RemoveTemLayer-26-移除临时图层");
            }
        }


        //点击单元格，查找对应的要素，变换要素的样式，实现闪烁。
        private Feature flashFt;
        private Style defaultStyle;
        int k = 0;
        /// <summary>
        /// 单击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dataGridView1.CurrentRow != null)
                if (dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "0" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "0")
                {
                    return;
                }

            //点击一个记录，进行地图定位
            try
            {
                //定位
                double x = Convert.ToDouble(dataGridView1["ColX", e.RowIndex].Value);
                double y = Convert.ToDouble(dataGridView1["ColY", e.RowIndex].Value);
               
                // 以下代码用来将当前地图的视野缩放至该对象所在的派出所   add by fisher in 09-12-24
                DPoint dP = new DPoint(x, y);
                CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                mapControl1.Map.Center = dP;
                mapControl1.Map.SetView(dP, cSys, getScale());
                    

                //闪烁要素
                timer1.Stop();
                if (flashFt != null)
                {
                    try
                    {
                        flashFt.Style = defaultStyle;
                        flashFt.Update();
                    }
                    catch { }
                }
                FeatureLayer tempLayer = mapControl1.Map.Layers["案件选择"] as MapInfo.Mapping.FeatureLayer;

                Table tableTem = tempLayer.Table;
                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + dataGridView1["colBianhao", e.RowIndex].Value.ToString() + "'"));

                //闪烁要素
                flashFt = ft;
                defaultStyle = ft.Style;
                k = 0;
                timer1.Start();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellClick-27-单击事件");
            }
        }

        /// <summary>
        /// 获取缩放比例
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                CLC.BugRelated.ExceptionWrite(ex, "ucZhihui-getScale");
                return 0;
            }
        }

        private Color col = Color.Blue;
        /// <summary>
        /// 实现图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (col == Color.Red)
                {
                    col = Color.Blue;
                }
                else
                {
                    col = Color.Red;
                }
                BasePointStyle pStyle = new SimpleVectorPointStyle(35, col, 26);
                flashFt.Style = pStyle;
                flashFt.Update();
                k++;
                if (k == 10)
                {
                    //flashFt.Style = defaultStyle;
                    //flashFt.Update();
                    timer1.Stop();
                }
            }
            catch(Exception ex)
            {
                timer1.Stop();
                ExToLog(ex, "timer1_Tick-28-图元闪烁");
            }
        }

        /// <summary>
        /// 显示预案内容.
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView2_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == 1) {
                OracleConnection Conn = new OracleConnection(strConn);
                try
                {
                    string aTip = "";
                    Conn.Open();

                    string strExpress = "select 名称,警车范围,摄像头范围,警员范围,治安卡口范围,"
                                             + "基层派出所范围,社区警务室范围,公共场所范围,"
                                             + "网吧范围,安全防护单位范围,特种行业范围,"
                                             + "消防重点单位范围,基层民警中队范围,消防栓范围"
                                             +" from 预案 where 名称='" + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() + "'";
                    OracleCommand cmd = new OracleCommand(strExpress, Conn);

                    OracleDataReader dr = cmd.ExecuteReader();
                    if(dr.HasRows){
                        dr.Read();
                        for (int i = 1; i < 13; i++) {
                            if (Convert.ToInt32(dr[i]) != 0) {
                                aTip += dr.GetName(i) + ":" + Convert.ToInt32(dr[i]) + "米\n";
                            }
                        }

                        if (aTip == "")
                        {
                            aTip = "本预案未设置.可进行修改";
                        }
                        else {
                            aTip = aTip.Remove(aTip.Length - 1);
                        }
                    }
                    dr.Close();

                    dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText =aTip;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "dataGridView2_CellMouseEnter-29-显示预案内容");
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                }
            }
        }
 
        private void textKeyword_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }


        private MapInfo.Geometry.DPoint dptStart;
        private MapInfo.Geometry.DPoint dptEnd;

        private System.Collections.ArrayList arrlstPoints = new ArrayList();
        /// <summary>
        /// 工具使用
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            if (this.Visible)
            {
                try
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
                                    WriteEditLog("指挥白板", "", "", "框选");
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
                                    WriteEditLog("指挥白板", "", "", "圈选");
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
                                    for (int j = 0; j < intCount; j++)
                                    {
                                        dptPoints[j] = (MapInfo.Geometry.DPoint)arrlstPoints[j];
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
                                    WriteEditLog("指挥白板", "", "", "多边形选择");
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Tools_Used-30-工具使用");
                }
            }
        }

        /// <summary>
        /// 清除图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        public void clearTem()
        {
            try
            {
                clearFeatures("直观指挥");
                clearFeatures("查看选择");
                clearFeatures("预演图层");
                StopVideo();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "clearTem-31-清除图元");
            }
        }


        /// <summary>
        /// 清除图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                ExToLog(ex, "clearFeatures-31-清除图元");
            }
        }


        private int i = 0; // 存储用户所选范围内的对象数
        /// <summary>
        /// 点击查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="geo"></param> 
        private void selectAndInsertByGeometry(FeatureGeometry geo)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
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
                    //if (MapInfo.Engine.Session.Current.Catalog.GetTable("视频") == null)
                    //{
                    //    openTable("视频");
                    //}
                    //SpatialSearchAndView(geo, "视频");
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("社会视频") == null)
                    {
                        openTable("社会视频");
                    }
                    SpatialSearchAndView(geo, "社会视频");
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("非社会视频") == null)
                    {
                        openTable("非社会视频");
                    }
                    SpatialSearchAndView(geo, "非社会视频");
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("车载视频") == null)
                    {
                        openTable("车载视频");
                    }
                    SpatialSearchAndView(geo, "车载视频");
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
            }
            catch (Exception ex)
            {
                ExToLog(ex, "selectAndInsertByGeometry-32-点击查询");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 查询图元并显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="tabAlias">图层名</param>
        private void SpatialSearchAndView(FeatureGeometry geo,string tabAlias)
        {
            try
            {
                FeatureLayer fl=mapControl1.Map.Layers["查看选择"] as FeatureLayer;
                Table ccTab = fl.Table;

                CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,GetFromNamePath);
                string objID = CLC.ForSDGA.GetFromTable.ObjID;
                string objName = CLC.ForSDGA.GetFromTable.ObjName;
                
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(geo, ContainsType.Geometry);
                si.QueryDefinition.Columns = null;
                IResultSetFeatureCollection fc= MapInfo.Engine.Session.Current.Catalog.Search(tabAlias,si);

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
                        if (tabAlias == "车载视频")
                        {
                            cs = new CompositeStyle(new BitmapPointStyle("ydsp.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));
                            tabAlias = "视频";
                        }
                        else if (tabAlias == "非社会视频")
                        {
                            cs = new CompositeStyle(new BitmapPointStyle("TARG1-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            objID = "设备编号";
                            objName = "设备名称";
                            tabAlias = "视频";
                        }
                        else if (tabAlias == "社会视频")
                        {
                            cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            objID = "设备编号";
                            objName = "设备名称";
                            tabAlias = "视频";
                        }
                        else
                        {
                            MapInfo.Styles.BitmapPointStyle bitmappointstyle = new MapInfo.Styles.BitmapPointStyle(bmpName);
                            cs = new CompositeStyle(bitmappointstyle);
                        }
                }

                Feature newFt;
                foreach (Feature ft in fc)
                {
                    i++;
                    newFt = new Feature(ccTab.TableInfo.Columns);
                    newFt["表_ID"] = ft[objID];
                    newFt["名称"] = ft[objName];
                    newFt["表名"] = tabAlias;
                    newFt.Geometry = ft.Geometry;
                    newFt.Style=cs;
                    ccTab.InsertFeature(newFt);
                }
            }
            catch(Exception ex) { 
                ExToLog(ex, "SpatialSearchAndView-33-查询图元并显示");
            }
        }

        /// <summary>
        /// 切换选中状态
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.CheckBox check = (System.Windows.Forms.CheckBox)sender;
                
                if (!check.Checked)//去掉选择,从查看选择中删除该类对象
                {
                    string tableAlies = check.Text;
                    CLC.ForSDGA.GetFromTable.GetFromName(tableAlies,GetFromNamePath);

                    if (tableAlies == "视频")
                    {
                        DeleteFeature("社会视频");
                        DeleteFeature("非社会视频");
                        DeleteFeature("车载视频");
                    }
                    else
                    {
                        DeleteFeature(tableAlies);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkBox_CheckedChanged-34-切换选中状态");
            }
        }

        /// <summary>
        /// 删除图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableAlies">表名</param>
        private void DeleteFeature(string tableAlies)
        {
            try
            {
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
            catch (Exception ex)
            {
                ExToLog(ex, "DeleteFeature-35-删除图元");
            }
        }


        
        /// <summary>
        /// 记录操作记录 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sModule">操作模块</param>
        /// <param name="tName">操作数据库表名</param>
        /// <param name="sql">操作sql语句</param>
        /// <param name="method">操作方法</param>
        private void WriteEditLog(string sModule, string tName, string sql, string method)
        {          
            try
            {     
                string strExe = "insert into 操作记录 values('" + User + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'直观指挥:" + sModule + "','" + tName + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "WriteEditLog-记录操作记录");
            }
        }

        /// <summary>
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">执行的sql语句</param>
        /// <returns>查询结果集</returns>
        private DataTable GetTable(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                return dt;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "WriteEditLog-获取查询结果表");
                return null;
            }
        }

        /// <summary>
        /// 处理SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">执行的sql语句</param>
        private void RunCommand(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "RunCommand-处理SQL");
            }
        }

        /// <summary>
        /// 获取Scalar
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">执行的sql语句</param>
        /// <returns>第一行第一列的结果</returns>
        private Int32 GetScalar(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetScalar-获取Scalar");
                return 0;
            }
        }


        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            try
            {
                CLC.BugRelated.ExceptionWrite(ex,"clZhihui-ucZhihui-"+sFunc);              
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmpname"></param>
        /// <param name="msg"></param>
        public void Deal110Msg(string bmpname, string[] msg)
        {
            try
            {
                InitZhihui();

                //comboBoxField.Text = @"报警编号";
                //textKeyword.Text = msg[0].Substring(1, msg[0].Length - 1);
                //caseSearch();
                //dataGridView1.Rows[0].Selected = true;

                Bind110Data(msg);

                Create110Ftr(bmpname, msg);

                tabControl1.SelectedTab = tabYuan;
            }
            catch (Exception ex) { ExToLog(ex, "Deal110Msg"); }
        }

        private void Bind110Data(string[] msg)
        {
            try
            {
                DataTable dt = new DataTable("socket");
                //列
                string[] func = { "报警编号", "发案地点详址", "简要案情", "案件名称", "所属派出所", "所属中队", "对讲机ID", "案件来源", "报警时间", "X", "Y" };
                for (int i = 0; i < func.Length; i++)
                {
                    DataColumn dc = new DataColumn(func[i]);
                    dc.DataType = Type.GetType("System.String");
                    dt.Columns.Add(dc);
                }

                //行
                string ajbh = msg[0].Substring(1, msg[0].Length - 1); // 去掉标志位的案件编号
                DataRow drow = dt.NewRow();
                drow[func[0]] = ajbh;
                for (int i = 1; i < func.Length; i++)
                {
                    drow[func[i]] = msg[i];
                }
                dt.Rows.Add(drow);

                dataGridView1.Rows.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dataGridView1.Rows.Add(i + 1, dt.Rows[i]["发案地点详址"], "详情...", dt.Rows[i]["报警编号"], dt.Rows[i]["X"], dt.Rows[i]["Y"]);
                }
                //设置宽度
                setDataGridViewColumnWidth(dataGridView1);
            }
            catch (Exception ex) { ExToLog(ex, "Bind110Data"); }
        }


        private void Create110Ftr(string bmpname, string[] message) 
        {
            try
            {
                if (queryTable != null)
                {
                    queryTable.Close();

                }
                           
                TableInfoMemTable ti = new TableInfoMemTable("案件选择");
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
                column.Alias = "表名";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                queryTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);


                //"1A000000@上海@有人抢接@软件园@普陀@金沙江@ID101@110报警@2010-2-3@113.239@22.8375@".Split('@');

                double dx = 0, dy = 0;
                
                        dx = Convert.ToDouble(message[9]);
                        dy = Convert.ToDouble(message[10]);
                
                    if (dx >0 || dy > 0)
                    {
                    FeatureGeometry pt = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), dx, dy);

                    CompositeStyle cs = new CompositeStyle();
                    cs.ApplyStyle(new BitmapPointStyle(bmpname, BitmapStyles.None, Color.Red, 20));

                    Feature pFeat = new Feature(queryTable.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["表_ID"] = message[0].Substring(1, message[0].Length - 1);
                    pFeat["表名"] = "报警系统";
                    queryTable.InsertFeature(pFeat);
                    }                

                currentFeatureLayer = new FeatureLayer(queryTable, "案件选择");

                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                labeLayer(queryTable, "表_ID");
                //this.setLayerStyle(currentFeatureLayer, Color.Red, 34, 10);
                // 以下代码用来将当前地图的视野缩放至1:6000   jie.zhang 20100311
                try
                {
                    DPoint dP = new DPoint(dx, dy);
                    CoordSys cSys = mapControl1 .Map.GetDisplayCoordSys();

                    mapControl1.Map.SetView(dP, cSys, 6000);
                    mapControl1.Map.Center = dP;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Create110Ftr-设定视图范围");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Create110Ftr-将查询结果显示在地图上");
                //writeToLog(ex,"insertQueryIntoMap");
            }
        }

        /// <summary>
        /// 双击进行定位并该记录显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //点击一个记录，进行地图定位
            try
            {
                if (e.RowIndex < 0) return;

                DataTable datatable = GetTable("Select * from GPS110.报警信息110 where GPS110.报警信息110.报警编号='" + dataGridView1["colBianhao", e.RowIndex].Value.ToString() + "'");
                if (dataGridView1.CurrentRow != null)
                    if (dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "0" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "0")
                    {
                        System.Drawing.Point ptZ = new System.Drawing.Point();
                        Screen scren = Screen.PrimaryScreen;
                        ptZ.X = scren.WorkingArea.Width / 2;
                        ptZ.Y = 10;

                        disPlayInfo(datatable, ptZ);
                        return;
                    }


                //定位
                double x = Convert.ToDouble(dataGridView1["ColX", e.RowIndex].Value);
                double y = Convert.ToDouble(dataGridView1["ColY", e.RowIndex].Value);

                // 以下代码用来将当前地图的视野缩放至该对象所在的派出所   add by fisher in 09-12-24
                DPoint dP = new DPoint(x, y);
                CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                mapControl1.Map.Center = dP;
                mapControl1.Map.SetView(dP, cSys, getScale());

                //闪烁要素
                timer1.Stop();
                if (flashFt != null)
                {
                    try
                    {
                        flashFt.Style = defaultStyle;
                        flashFt.Update();
                    }
                    catch { }
                }
                FeatureLayer tempLayer = mapControl1.Map.Layers["案件选择"] as MapInfo.Mapping.FeatureLayer;

                Table tableTem = tempLayer.Table;
                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + dataGridView1["colBianhao", e.RowIndex].Value.ToString() + "'"));

                //闪烁要素
                flashFt = ft;
                defaultStyle = ft.Style;
                k = 0;
                timer1.Start();

                if (dP.x == 0 || dP.y == 0)
                {
                    dP.x = mapControl1.Map.Center.x;
                    dP.y = mapControl1.Map.Center.y;
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                mapControl1.Map.DisplayTransform.ToDisplay(dP, out pt);
                pt.X += Width + 10;
                pt.Y += 80;

                disPlayInfo(datatable, pt); 

            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellDoubleClick-27-单击事件");
            }
        }

        private FrmInfo frminfo = new FrmInfo();

        /// <summary>
        /// 显示车辆详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">位置</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (frminfo.Visible == false)
                {
                    frminfo = new FrmInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.TopMost = true;
                    frminfo.Visible = false;
                }
                frminfo.setInfo(dt.Rows[0], pt, StrCon, User);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo-16-显示报警详细信息");
            }
        }

        /// <summary>
        /// 文本框输入回车后进行查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spellSearchBoxEx1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    caseSearch();

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                    //this.toolPro.Value = 3;
                    //Application.DoEvents();
                    //isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "spellSearchBoxEx1_KeyPress");
            }
        }

        /// <summary>
        /// 自动补全功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void spellSearchBoxEx1_TextChanged(object sender, EventArgs e)
        {
            try
            {

                string keyword = ToDBC(this.CaseSearchBox.Text.Trim());
                string colfield = this.comboBoxField.Text.Trim();

                if (keyword.Length < 1 || colfield.Length < 1) return;
                
                if (colfield == "案件类型")
                {

                    string strExp = "select distinct(类型名称) from 案件类型 where 类型名称 like '%" + ToDBC(this.CaseSearchBox.Text.Trim()) + "%' order by 类型名称";
                    DataTable dt = GetTable(strExp);

                    if (this.CaseSearchBox.Text.Trim().Length < 1)
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
                ExToLog(ex, "spellSearchBoxEx1_TextChanged-17-数据匹配");
            }
        }

        /// <summary>
        /// 自动补全功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                string colfield = this.comboBoxField.Text.Trim();

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

        /// <summary>
        /// 显示或隐藏进度条
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="falg">布尔值（true-显示 false-隐藏）</param>
        private void isShowPro(bool falg)
        {
            try
            {
                this.toolPro.Value = 0;
                this.toolPro.Maximum = 3;
                this.toolPro.Width = 100;
                this.toolProLbl.Visible = falg;
                this.toolProSep.Visible = falg;
                this.toolPro.Visible = falg;
                Application.DoEvents();
            }
            catch (Exception ex) { ExToLog(ex, "isShowPro"); }
        }

        /// <summary>
        /// 将半角改为全角
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="input">要改变的文本</param>
        /// <returns>改变后的文本</returns>
        private String ToDBC(String input)
        {
            try
            {
                char[] c = input.ToCharArray();

                for (int i = 0; i < c.Length; i++)
                {
                    if (c[i] == 12288)
                    {

                        c[i] = (char)32;

                        continue;

                    }
                    if (c[i] > 65280 && c[i] < 65375)

                        c[i] = (char)(c[i] - 65248);

                }
                return new String(c);
            }
            catch (Exception ex) { ExToLog(ex, "ToDBC"); return null; }
        }

        /// <summary>
        /// 分页控件操作
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void bindingNavigatorZhihui_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.TextNum.Text);    //每页显示多少数据
                bool isFirst = false;
                if (e.ClickedItem.Text == "上一页")
                {
                    if (pageCurrent <= 1)
                    {
                        isFirst = true;
                        System.Windows.Forms.MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        if (endNo == nMax)
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            startNo = nMax - (nMax % countShu) + 1 - countShu;
                            endNo = nMax - (nMax % countShu);
                        }
                        else
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            startNo -= countShu;
                            endNo -= countShu;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "下一页")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent++;
                        nCurrent = pageSize * (pageCurrent - 1);
                        startNo += countShu;
                        endNo += countShu;
                    }
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (pageCurrent <= 1)
                    {
                        isFirst = true;
                        System.Windows.Forms.MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = 0;
                        startNo = 1;
                        endNo = countShu;
                    }
                }
                else if (e.ClickedItem.Text == "转到尾页")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = pageCount;
                        nCurrent = pageSize * (pageCurrent - 1);
                        startNo = nMax - (nMax % countShu) + 1;
                        endNo = nMax;
                    }
                }
                else
                {
                    return;
                }
                // LoadData(startNo, endNo);
                DataTable dt = LoadData(countShu, pageCurrent, isFirst);
                this.dataGridView1.Rows.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dataGridView1.Rows.Add(i + 1, dt.Rows[i]["发案地点详址"], "详情...", dt.Rows[i]["报警编号"], dt.Rows[i]["X"], dt.Rows[i]["Y"], dt.Rows[i]["案件类型"]);
                    if (i % 2 == 1)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                //设置宽度
                setDataGridViewColumnWidth(dataGridView1);
                insertQueryIntoMap(dt);

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 通过改变当前页数实现分页
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void PageNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                bool isFrist = false;
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(PageNow.Text) < 1 || Convert.ToInt32(PageNow.Text) > pageCount)
                    {
                        System.Windows.Forms.MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        PageNow.Text = pageCurrent.ToString();
                        return;
                    }
                    else
                    {
                        this.pageCurrent = Convert.ToInt32(PageNow.Text);
                        nCurrent = pageSize * (pageCurrent - 1);
                        startNo = ((pageCurrent - 1) * pageSize) + 1;
                        endNo = startNo + pageSize - 1;
                    }

                    //LoadData(startNo, endNo);
                    DataTable dt = LoadData(pageSize, pageCurrent, isFrist);
                    this.dataGridView1.Rows.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add(i + 1, dt.Rows[i]["发案地点详址"], "详情...", dt.Rows[i]["报警编号"], dt.Rows[i]["X"], dt.Rows[i]["Y"], dt.Rows[i]["案件类型"]);
                        if (i % 2 == 1)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    //设置宽度
                    setDataGridViewColumnWidth(dataGridView1);
                    insertQueryIntoMap(dt);

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "PageNow_KeyPress"); }
        }

        /// <summary>
        /// 改变列表显示数量
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void TextNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView1.Rows.Count > 0)
                {
                    this.pageSize = Convert.ToInt32(TextNum.Text);
                    this.pageCurrent = 1;
                    nCurrent = pageSize * (pageCurrent - 1);
                    pageCount = (nMax / pageSize);//计算出总页数
                    if ((nMax % pageSize) > 0) pageCount++;
                    endNo = pageSize;
                    startNo = 1;

                    //LoadData(startNo, endNo);

                    DataTable dt = LoadData(pageSize, 1, true);

                    this.dataGridView1.Rows.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add(i + 1, dt.Rows[i]["发案地点详址"], "详情...", dt.Rows[i]["报警编号"], dt.Rows[i]["X"], dt.Rows[i]["Y"], dt.Rows[i]["案件类型"]);
                        if (i % 2 == 1)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    //设置宽度
                    setDataGridViewColumnWidth(dataGridView1);
                    insertQueryIntoMap(dt);

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "TextNum_KeyPress"); }
        }

        /// <summary>
        /// 取消自定义演练
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                this.panel3.Visible = false;
                mapControl1.Tools.LeftButtonTool = "Select";
                RemoveTemLayer("预演图层");
                clearFeatures("直观指挥");
                clearFeatures("查看选择");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }
        }

        private double planX = 0;  // 预案演练坐标
        private double planY = 0;  // 预案演练坐标

        /// <summary>
        /// 创建自定义演练地点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        private void btnAddress_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textPlan.Text != string.Empty)
                {
                    mapControl1.Tools.LeftButtonTool = "AddPoint";
                }
                else
                {
                    MessageBox.Show("请输入演练名称后创建地点！", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnAddress_Click");
            }
        }
    }
}