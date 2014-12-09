using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Data.OracleClient;
using MapInfo.Styles;
using MapInfo.Data;
using System.Xml;
using MapInfo.Mapping;
using MapInfo.Geometry;
using MapInfo.Tools;
using System.IO;
using System.Runtime.InteropServices;

namespace clHouse
{
    public partial class ucHouse : UserControl
    {
        private MapInfo.Windows.Controls.MapControl mapControl1;
        private string strConn;
        private string[] conStr = null;
        private string getFromNamePath;
        public string strRegion = "";
        public string strRegion1 = "";
        public string user = "";
        
        public string strRegion2 = ""; // 可导出的派出所
        public string strRegion3 = ""; // 可导出的中队
        public string excelSql = "";   // 查询导出sql
        public string exportSql = "";  // 导出完整SQL  lili 2010-11-8
        public System.Data.DataTable dtExcel = null; //地图页面数据导出按钮

        public ToolStripProgressBar toolPro;  // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolProLbl;     // 用于显示进度文本　
        public ToolStripSeparator toolProSep;

        #region 输入法
        //声明一些API函数
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetOpenStatus(IntPtr himc);
        [DllImport("imm32.dll")]
        public static extern bool ImmSetOpenStatus(IntPtr himc, bool b);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetConversionStatus(IntPtr himc, ref int lpdw, ref int lpdw2);
        [DllImport("imm32.dll")]
        public static extern int ImmSimulateHotKey(IntPtr hwnd, int lngHotkey);
        private const int IME_CMODE_FULLSHAPE = 0x8;
        private const int IME_CHOTKEY_SHAPE_TOGGLE = 0x11;
        #endregion

        private MIConnection _miConnection = new MIConnection();
        public ucHouse(MapInfo.Windows.Controls.MapControl m, string s,string[] canStr,string getFromPath)
        {
            InitializeComponent();

            try
            {
                mapControl1 = m;
                getFromNamePath = getFromPath;
                strConn = s;
                conStr = canStr;
                setEvents();
                CLC.DatabaseRelated.OracleDriver.CreateConstring(canStr[0], canStr[1], canStr[2]);
                this.H_setfield();
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "构造函数");
            }
        }

        /// <summary>
        /// 设置事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void setEvents(){
            dataGV.CellClick += new DataGridViewCellEventHandler(dataGV_CellClick);
            dataGV.CellDoubleClick += new DataGridViewCellEventHandler(dataGV_CellDoubleClick);
            this.mapControl1.Tools.FeatureSelected += new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);
        }

        private MapInfo.Data.MultiResultSetFeatureCollection mirfc = null;
        private MapInfo.Data.IResultSetFeatureCollection mirfc1 = null;

        /// <summary>
        /// 图元选择事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
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

                    this.showDataGridViewLineOnlyOneTable(f["表_ID"].ToString(), "出租屋房屋系统");
                }
                catch (Exception ex)
                {
                    writeToHouseLog(ex,"Feature_Selected");
                }
            }
        }

        /// <summary>
        /// 在DataGridView中显视
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="表_ID">表_ID</param>
        /// <param name="表名">表名</param>
        public void showDataGridViewLineOnlyOneTable(string 表_ID, string 表名)
        {
            try
            {
                for (int i = 0; i < this.dataGV.Rows.Count; i++)
                {
                    if (this.dataGV.Rows[i].Cells["房屋编号"].Value.ToString() == 表_ID)
                    { 
                       this.dataGV.CurrentCell = this.dataGV.Rows[i].Cells[0];
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex,"showDataGridViewLineOnlyOneTable");
            }
        }

        /// <summary>
        /// 查询按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridExp.Rows.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("请添加查询语句!", "提示");
                    return;
                }

                if (getSqlString() == "")
                {
                    System.Windows.Forms.MessageBox.Show("查询语句有错误,请重设!", "提示");
                    return;
                }
                houseSearch();
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "buttonSearch_Click");
            }
        }

        string strWhere = "";
        //---------分页用全局变量------
        int _startNo = 1;   // 开始的行数
        int _endNo = 0;　　 // 结束的行数
        //----------------------------
        /// <summary>
        /// 查询房屋数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void houseSearch()
        {
            isShowPro(true);
            strWhere = getSqlString();
            
            if (strRegion == "")
            {
                isShowPro(false);
                MessageBox.Show("您没有管理权限！");
                return;
            }
            if (strRegion != "顺德区" && strRegion != "")
            {
                string sRegion = strRegion;
                if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                {
                    sRegion = strRegion.Replace("大良", "大良,德胜");
                }
                strWhere += " and 镇街 in ('" + sRegion.Replace(",", "','") + "')";
            }
            _endNo = Convert.ToInt32(this.TextNum1.Text);
            _startNo = 1;

            //strSQL = "select  房屋编号,屋主姓名,屋主联系电话,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,当前居住人数,暂住证有效期内人数,未办暂住证人数,历史居住人数,全地址,地址街路巷,地址门牌,楼层,房间号,房屋类型,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y   from 出租屋房屋系统 t where " + strWhere + " and 备用字段一 is null or 备用字段一=''"; ;

            this.Cursor = Cursors.WaitCursor;
            try
            {
                //alter by siumo 090116
                this.getMaxCount("select count(*) from 出租屋房屋系统 t where " + strWhere + " and (备用字段一 is null or 备用字段一='')");
                InitDataSet(RecordCount1); //初始化数据集

                if (nMax < 1)
                {
                    isShowPro(false);
                    clearTem();
                    MessageBox.Show("所设条件无记录.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

                DataTable datatable = LoadData(_startNo,_endNo, strWhere,false); //获取当前页数据

                //_RenNoMoth(datatable);       // 由于在列表中不显示与放屋居住人数的4列，所以此功能屏弊

                dataGV.DataSource = datatable;
                this.toolPro.Value = 1;
                Application.DoEvents();

                #region 数据导出Excel
                //excelSql = "select 房屋编号,屋主姓名,屋主联系电话,房屋类型,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,当前居住人数,暂住证有效期内人数,未办暂住证人数,历史居住人数,全地址,地址街路巷,地址门牌,楼层,房间号,抽取ID,抽取更新时间,最后更新人,备用字段一,备用字段二,备用字段三,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y ";
                //excelSql += strSQL.Substring(strSQL.IndexOf("from"));
                excelSql = strWhere;
                string sRegion2 = strRegion2;
                string sRegion3 = strRegion3;
                if (strRegion2 == "")
                {
                    excelSql += " and 1=2 ";
                }
                else if (strRegion2 != "顺德区")
                {
                    if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                    {
                        sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                    }
                    excelSql += " and 镇街 in ('" + sRegion2.Replace(",", "','") + "')";
                }
                LoadData(_startNo, _endNo, excelSql, true);
                //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql, true);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion

                this.toolPro.Value = 2;
                //Application.DoEvents();

                //分行设置行的背景色
                for (int i = 1; i < dataGV.Rows.Count; i += 2)
                {
                    dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                }
                createPoint(datatable);
                WriteEditLog(strWhere, "查询");
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch(Exception ex)
            {
                isShowPro(false);
                writeToHouseLog(ex,"houseSearch");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 将房屋中当前居住人数、历史居住人数、未办暂住证人数、暂住证有效期内人数四个字段的身份证号码转换成数字显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="datatable">要转换的结果集</param>
        private void _RenNoMoth(DataTable datatable)
        {
            try
            {
                for (int s = 0; s < datatable.Rows.Count; s++)
                {
                    switch (datatable.Rows[s]["当前居住人数"].ToString())
                    {
                        case "0":
                        case "":
                            datatable.Rows[s]["当前居住人数"] = 0;
                            break;
                        default:
                            datatable.Rows[s]["当前居住人数"] = datatable.Rows[s]["当前居住人数"].ToString().Split(',').Length;
                            break;
                    }
                    switch (datatable.Rows[s]["历史居住人数"].ToString())
                    {
                        case "0":
                        case "":
                            datatable.Rows[s]["历史居住人数"] = 0;
                            break;
                        default:
                            datatable.Rows[s]["历史居住人数"] = datatable.Rows[s]["历史居住人数"].ToString().Split(',').Length;
                            break;
                    }
                    switch (datatable.Rows[s]["未办暂住证人数"].ToString())
                    {
                        case "0":
                        case "":
                            datatable.Rows[s]["未办暂住证人数"] = 0;
                            break;
                        default:
                            datatable.Rows[s]["未办暂住证人数"] = datatable.Rows[s]["未办暂住证人数"].ToString().Split(',').Length;
                            break;
                    }
                    switch (datatable.Rows[s]["暂住证有效期内人数"].ToString())
                    {
                        case "0":
                        case "":
                            datatable.Rows[s]["暂住证有效期内人数"] = 0;
                            break;
                        default:
                            datatable.Rows[s]["暂住证有效期内人数"] = datatable.Rows[s]["暂住证有效期内人数"].ToString().Split(',').Length;
                            break;
                    }
                }
            }
            catch (Exception ex) 
            { 
                writeToHouseLog(ex, "_RenNoMoth"); 
            }
        }

        /// <summary>
        /// 地图上画点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        private void createPoint(DataTable dt)
        {
            try
            {
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["房屋系统"];
                Table tableTem = fl.Table;

                //先清除已有对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        double dx = Convert.ToDouble(dt.Rows[i]["X"]);
                        double dy = Convert.ToDouble(dt.Rows[i]["Y"]);
                        if (dx > 0 && dy > 0)
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);
                            CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("fw.bmp"));

                            Feature pFeat = new Feature(tableTem.TableInfo.Columns);
                            pFeat.Geometry = pt;
                            pFeat.Style = cs;
                            pFeat["表_ID"] = dt.Rows[i]["房屋编号"].ToString();
                            pFeat["表名"] = "出租屋房屋系统";
                            tableTem.InsertFeature(pFeat);
                        }
                    }
                    catch (Exception ex)
                    {
                        writeToHouseLog(ex, "createPoint");
                    }
                }
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "createPoint");
            }
        }

        private Feature flashFt;
        private Style defaultStyle;
        private int k;
        string StrID = "";
        /// <summary>
        /// 点击单元格，查找对应的要素，变换要素的样式，实现闪烁。
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        void dataGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            try
            {
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
                //点击一个记录，进行地图定位
                if (dataGV["X", e.RowIndex].Value == null || dataGV["Y", e.RowIndex].Value == null || dataGV["X", e.RowIndex].Value.ToString() == "" || dataGV["Y", e.RowIndex].Value.ToString() == "")
                {
                    return;
                }
                double x = 0, y = 0;

                try
                {
                    x = Convert.ToDouble(dataGV["X", e.RowIndex].Value.ToString());
                    y = Convert.ToDouble(dataGV["Y", e.RowIndex].Value.ToString());
                }
                catch(Exception ex) { CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.dataGV_CellClick"); return; }

                if (x == 0 || y == 0)
                {
                    return;
                }
                StrID = dataGV["房屋编号", e.RowIndex].Value.ToString();
                dataGV.Rows[e.RowIndex].Selected = true;
                
                // 以下代码用来将当前地图的视野缩放至该对象所在的派出所   add by fisher in 09-12-24
                MapInfo.Geometry.DPoint dP = new MapInfo.Geometry.DPoint(x, y);
                MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();

                mapControl1.Map.SetView(dP, cSys, getScale());
                this.mapControl1.Map.Center = dP;

                FeatureLayer tempLayer = mapControl1.Map.Layers["房屋系统"] as MapInfo.Mapping.FeatureLayer;

                Table tableTem = tempLayer.Table;
                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + dataGV["房屋编号", e.RowIndex].Value.ToString() + "'"));

                //闪烁要素
                flashFt = ft;
                defaultStyle = ft.Style;
                k = 0;
                timer1.Start();
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "dataGV_CellClick");
            }
        }

        /// <summary>
        /// 获取缩放比例
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <returns></returns>
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
                writeToHouseLog(ex, "getScale");
                return 0;
            }
        }

        /// <summary>
        /// 显示信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        void dataGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                DPoint dp = new DPoint();
                string sqlFields = "房屋编号,屋主姓名,屋主联系电话,屋主身份证号码,房屋类型,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,当前居住人数,暂住证有效期内人数,未办暂住证人数,历史居住人数,全地址,地址街路巷,地址门牌,楼层,房间号,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                string strSQL = "select " + sqlFields + " from 出租屋房屋系统 t where 房屋编号='" + dataGV.Rows[e.RowIndex].Cells["房屋编号"].Value.ToString() + "'";

                DataSet ds = new DataSet();
                OracleDataAdapter da = new System.Data.OracleClient.OracleDataAdapter(strSQL, Conn);
                da.Fill(ds);
                Conn.Close();
                DataTable datatable = ds.Tables[0];

                System.Drawing.Point pt = new System.Drawing.Point();
                if (dataGV["X", e.RowIndex].Value != null || dataGV["Y", e.RowIndex].Value != null || dataGV["X", e.RowIndex].Value.ToString() != "" || dataGV["Y", e.RowIndex].Value.ToString() != "")
                {
                    try
                    {
                        dp.x = Convert.ToDouble(datatable.Rows[0]["X"].ToString());
                        dp.y = Convert.ToDouble(datatable.Rows[0]["Y"].ToString());
                    }
                    catch (Exception ex)
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;
                        this.disPlayInfo(datatable, pt, "房屋系统");
                        WriteEditLog("房屋编号='" + dataGV.Rows[e.RowIndex].Cells["房屋编号"].Value.ToString() + "'", "查看详情");
                        CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.dataGV_CellDoubleClick");
                        return;
                    }
                }
                if (dp.x == 0 || dp.y == 0)
                {
                    Screen scren = Screen.PrimaryScreen;
                    pt.X = scren.WorkingArea.Width / 2;
                    pt.Y = 10;
                    this.disPlayInfo(datatable, pt, "房屋系统");
                    WriteEditLog("房屋编号='" + dataGV.Rows[e.RowIndex].Cells["房屋编号"].Value.ToString() + "'", "查看详情");
                    return;
                }
                mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                pt.X += this.Width + 10;
                pt.Y += 80;
                this.disPlayInfo(datatable, pt, "房屋系统");

                WriteEditLog("房屋编号='" + dataGV.Rows[e.RowIndex].Cells["房屋编号"].Value.ToString() + "'", "查看详情");

            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeToHouseLog(ex, "dataGV_CellDoubleClick");
            }
            finally
            {
                try
                {
                    fmDis.Visible = false;
                }
                catch { }
            }
        }

        private nsInfo.FrmInfo frmMessage = new nsInfo.FrmInfo();
        /// <summary>
        /// 详细信息显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">坐标</param>
        /// <param name="LayerName">图层名</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string LayerName)
        {
            try
            {  
                if (this.frmMessage.Visible == false)
                {
                    this.frmMessage = new nsInfo.FrmInfo();
                    frmMessage.Show();
                    frmMessage.Visible = false;
                }
                frmMessage.mapControl = mapControl1;
                frmMessage.getFromNamePath = getFromNamePath;
                frmMessage.setInfo(dt.Rows[0], pt, LayerName);
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex,"disPlayInfo");
            }
        }

        // private Color col = Color.Blue;
        /// <summary>
        /// 图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //CompositeStyle cs = null; ;
                MapInfo.Styles.BitmapPointStyle bitmappointstyle = null;
                if (k % 2 == 0)
                {
                    bitmappointstyle = new MapInfo.Styles.BitmapPointStyle("fw.bmp", BitmapStyles.None, Color.Red, 18);
                }
                else
                {
                    bitmappointstyle = new MapInfo.Styles.BitmapPointStyle("fw2.bmp", BitmapStyles.None, Color.Red, 18);
                }
                try
                {
                    flashFt.Style = bitmappointstyle;
                    flashFt.Update();
                }
                catch { }
                k++;
                if (k == 10)
                {
                    timer1.Stop();
                }
            }
            catch(Exception ex)
            {
                writeToHouseLog(ex, "timer1_Tick");
                timer1.Stop();
            }
        }

        /// <summary>
        /// 模块初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void ucHouse_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    //SetDrawStyle();
                    this.FieldStr.Text = this.FieldStr.Items[0].ToString();
                    this.ValueStr.Text = "";
                    this.dataGridExp.Rows.Clear();
                }
                else
                {
                    RemoveTemLayer("房屋系统");
                    dataGV.DataSource = null;
                    PageNow1.Text = "0";
                    PageCount1.Text = "/ {0}";
                    RecordCount1.Text = "0条";
                    pageSize = 0;     //每页显示行数
                    nMax = 0;         //总记录数
                    pageCount = 0;    //页数＝总记录数/每页显示行数
                    pageCurrent = 0;   //当前页号
                    nCurrent = 0;      //当前记录行
                }
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "timer1_Tick");
            }
        }

        /// <summary>
        /// 移除临时图层,关闭表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="tableAlies">表名</param>
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
                writeToHouseLog(ex, "RemoveTemLayer");
            }
        }

        /// <summary>
        /// 清除按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        public void clearTem()
        {
            dataGV.DataSource = null;
            PageNow1.Text = "0";
            PageCount1.Text = "/ {0}";
            RecordCount1.Text = "0条";
            pageSize = 0;     //每页显示行数
            nMax = 0;         //总记录数
            pageCount = 0;    //页数＝总记录数/每页显示行数
            pageCurrent = 0;   //当前页号
            nCurrent = 0;      //当前记录行

            try
            {
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["房屋系统"];
                Table tableTem = fl.Table;

                //清除已有对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch(Exception ex)
            {
                writeToHouseLog(ex, "clearTem");
            }
        }

        /// <summary>
        /// 文本框回车执行查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textKeyWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    houseSearch();
                }
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "textKeyWord_KeyPress");
            }
        }

        int pageSize = 0;     //每页显示行数
        int nMax = 0;         //总记录数
        int pageCount = 0;    //页数＝总记录数/每页显示行数
        int pageCurrent = 0;   //当前页号
        int nCurrent = 0;      //当前记录行
        DataSet ds = new DataSet();

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        private void getMaxCount(string sql)//得到最大的值
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
                writeToHouseLog(ex, "getMaxCount");
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nMax = 0;
            }
        }

        /// <summary>
        /// 初始化控件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="tsLabel">用于显示总记录数</param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                pageSize = Convert.ToInt32(this.TextNum1.Text);      //设置页面行数
                TextNum1.Text = pageSize.ToString();
                tsLabel.Text = nMax.ToString() + "条";//在导航栏上显示总记录数
                pageCount = (nMax / pageSize);//计算出总页数
                if ((nMax % pageSize) > 0) pageCount++;
                if (nMax != 0)
                {
                    pageCurrent = 1;
                }
                nCurrent = 0;       //当前记录数从0开始
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "InitDataSet");
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 查询数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="_startNo">开始数</param>
        /// <param name="_endNo">结束数</param>
        /// <param name="_whereSql">sql条件</param>
        /// <param name="isExcel">是否生成导出SQL</param>
        /// <returns>结果集</returns>
        public DataTable LoadData(int _startNo, int _endNo, string _whereSql, bool isExcel)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                int nStartPos = 0;   //当前页面开始记录行
                nStartPos = nCurrent;

                this.PageNow1.Text = Convert.ToString(pageCurrent);
                this.PageCount1.Text = "/" + pageCount.ToString();

                CLC.ForSDGA.GetFromTable.GetFromName("出租屋房屋系统", getFromNamePath);
                DataTable dtInfo;
                string sql = "";
                if (isExcel)   // 为了提高查询效率 如果是导出只要生成SQL语句，不用查询
                {
                    sql = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from 出租屋房屋系统 a where rownum<=" + _endNo + " and " + _whereSql + " and (备用字段一 is null or 备用字段一='')) t where rn1 >=" + _startNo;
                    exportSql = sql;  // 将导出SQL保存，点击导出按钮时调用
                    return null;
                }
                else
                {
                    //sql = "select 房屋编号,屋主姓名,屋主联系电话,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,当前居住人数,暂住证有效期内人数,未办暂住证人数,历史居住人数,全地址,地址街路巷,地址门牌,楼层,房间号,房屋类型,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y from (select rownum as rn1,a.* from 出租屋房屋系统 a where rownum<=" + _endNo + " and " + _whereSql + " and (备用字段一 is null or 备用字段一='')) t where rn1 >=" + _startNo;
                    sql = "select 房屋编号,屋主姓名,屋主联系电话,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,全地址,地址街路巷,地址门牌,楼层,房间号,房屋类型,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y from (select rownum as rn1,a.* from 出租屋房屋系统 a where rownum<=" + _endNo + " and " + _whereSql + " and (备用字段一 is null or 备用字段一='')) t where rn1 >=" + _startNo;
                }

                dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(dtInfo);

                Cmd.Dispose();
                Conn.Close();

                dataPopu = new DataTable();
                dataPopu = dtInfo;        // 放大数据用Table

                return dtInfo;
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeToHouseLog(ex, "LoadData");
                System.Windows.Forms.MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 查询数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="textNowPage">显示当前页控件</param>
        /// <param name="lblPageCount">每页显示数据控件</param>
        /// <param name="sql">sql语句</param>
        /// <returns>数据集</returns>
        public DataTable LoadData(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, string sql)
        {
            try
            {
                int nStartPos = 0;   //当前页面开始记录行
                nStartPos = nCurrent;
                //lblPageCount.Text = "第" + Convert.ToString(pageCurrent) + "页共" + pageCount.ToString() + "页";
                textNowPage.Text = Convert.ToString(pageCurrent);
                lblPageCount.Text = "/" + pageCount.ToString();

                OracleConnection Conn = new OracleConnection(strConn);
                DataTable dtInfo;

                try
                {
                    dtInfo = new DataTable();
                    Conn.Open();
                    OracleCommand Cmd = Conn.CreateCommand();
                    Cmd.CommandText = sql;
                    OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                    DataTable[] dataTables = new DataTable[1];
                    for (int i = 0; i < dataTables.Length; i++)
                    {
                        dataTables[i] = new DataTable();
                    }
                    Adp.Fill(nStartPos, pageSize, dataTables);//这个地方不知道是从数据库中查到前1000行返回，还是所有的数据据都查询到返回，再从中获取前100行。

                    dtInfo = dataTables[0];
                    Cmd.Dispose();
                    Conn.Close();

                    return dtInfo;
                }
                catch(Exception ex)
                {
                    writeToHouseLog(ex, "LoadData");
                    if (Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "LoadData");
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 分页导航
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <returns>布尔值（true-通过 false-不通过）</returns>
        private bool bdnInfo_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.TextNum1.Text);
                if (e.ClickedItem.Text == "上一页")
                {
                    if (pageCurrent <= 1)
                    {
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        if (_endNo == nMax)
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _startNo = nMax - (nMax % countShu) + 1 - countShu;
                            _endNo = nMax - (nMax % countShu);
                            return true;
                        }
                        else
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _startNo -= countShu;
                            _endNo -= countShu;
                            return true;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "下一页")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        pageCurrent++;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _startNo += countShu;
                        _endNo += countShu;
                        return true;
                    }
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (pageCurrent <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = 0;
                        _startNo = 1;
                        _endNo = countShu;
                        return true;
                    }
                }
                else if (e.ClickedItem.Text == "转到尾页")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        pageCurrent = pageCount;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _startNo = nMax - (nMax % countShu) + 1;
                        _endNo = nMax;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.bdnInfo_ItemClicked");
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 点击分页控件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //返回参数,如果false,说明到了第一页或最好一页,有操作不用进行
                if (isOn)
                {
                    isShowPro(true);
                    DataTable dt = LoadData(_startNo, _endNo, strWhere, false);
                    //_RenNoMoth(dt);        // 由于在列表中不显示与放屋居住人数的4列，所以此功能屏弊
                    dataGV.DataSource = dt;
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    for (int i = 1; i < dataGV.Rows.Count; i += 2)
                    {
                        dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    createPoint(dt);
                    this.toolPro.Value = 2;
                    //Application.DoEvents();

                    #region 数据导出
                    LoadData(_startNo, _endNo, excelSql, true);
                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion

                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.bindingNavigator1_ItemClicked");
            }
        }       


        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeToHouseLog(Exception ex,string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clHouse-ucHouse-" + sFunc);
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="method">操作方式</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'房屋管理','出租屋房屋系统:" + sql.Replace('\'', '"') + "','" + method + "')";

                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch(Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.WriteEditLog");
            }
        }

        /// <summary>
        /// 设计翻页转跳
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ltextNow">显示当前页的控件</param>
        private void PageNow_KeyPress(ToolStripTextBox ltextNow)
        {
            try
            {
                if (Convert.ToInt32(ltextNow.Text) < 1 || Convert.ToInt32(ltextNow.Text) > pageCount)
                {
                    System.Windows.Forms.MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                    ltextNow.Text = pageCurrent.ToString();
                    return;
                }
                else
                {
                    this.pageCurrent = Convert.ToInt32(ltextNow.Text);
                    nCurrent = pageSize * (pageCurrent - 1);
                    _startNo = ((pageCurrent - 1) * pageSize) + 1;
                    _endNo = _startNo + pageSize - 1;
                }
            }
            catch(Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.PageNow_KeyPress");
            }
        }

        /// <summary>
        /// 实现输入页数跳转至该页
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void PageNow1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    isShowPro(true);
                    PageNow_KeyPress(PageNow1);
                    DataTable dt = LoadData(_startNo, _endNo, strWhere,false);
                    // _RenNoMoth(dt);    // 由于在列表中不显示与放屋居住人数的4列，所以此功能屏弊
                    dataGV.DataSource = dt;
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    for (int i = 1; i < dataGV.Rows.Count; i += 2)
                    {
                        dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    createPoint(dt);
                    this.toolPro.Value = 2;
                    //Application.DoEvents();

                    #region 数据导出
                    LoadData(_startNo, _endNo, excelSql,true);
                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch(Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.PageNow1_KeyPress");
            }
        }

        /// <summary>
        /// 实现每页显示的数据数目
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ltextNum">每页显示数据数控件</param>
        /// <param name="datagridview">显示数据控件</param>
        private void TextNum_KeyPress(ToolStripTextBox ltextNum, DataGridView datagridview)
        {
            try
            {
                if (datagridview.Rows.Count > 0)
                {
                    this.pageSize = Convert.ToInt32(ltextNum.Text);
                    this.pageCurrent = 1;
                    nCurrent = pageSize * (pageCurrent - 1);
                    pageCount = (nMax / pageSize);//计算出总页数
                    if ((nMax % pageSize) > 0) pageCount++;
                    _endNo = pageSize;
                    _startNo = 1;

                    #region 数据导出
                    LoadData(_startNo, _endNo, excelSql, true);
                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch(Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.TextNum_KeyPress");
                ltextNum.Text = pageSize.ToString();
            }
        }

        /// <summary>
        /// 实现输入每页显示总数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGV.Rows.Count > 0)
                {
                    isShowPro(true);
                    TextNum_KeyPress(TextNum1, dataGV);
                    DataTable dt = LoadData(_startNo, _endNo, strWhere,false);
                    //_RenNoMoth(dt);    // 由于在列表中不显示与放屋居住人数的4列，所以此功能屏弊
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    dataGV.DataSource = dt;
                    this.toolPro.Value = 2;
                    //Application.DoEvents();
                    for (int i = 1; i < dataGV.Rows.Count; i += 2)
                    {
                        dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    createPoint(dt);
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch(Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.TextNum1_KeyPress");
                isShowPro(false);
            }
        }

        string H_arrType = "";
        /// <summary>
        /// 设置房屋中的FieldStr字段
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void H_setfield()
        {
            try
            {
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '出租屋房屋系统'";
                DataTable dt = new DataTable();
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);

                FieldStr.Items.Clear();
                H_arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string aCol = dt.Rows[i][0].ToString();
                    string aType = dt.Rows[i][1].ToString();
                    if (aCol != "" && aCol != "MAPID" && aCol.IndexOf("备用字段") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("代码") < 0)
                    {
                        FieldStr.Items.Add(aCol);
                        H_arrType += aType + ",";
                    }
                }
                FieldStr.Text = "房屋编号";
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.H_setfield");
            }
        }

        /// <summary>
        /// 切换字段生成运算符
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void FieldStr_SelectedIndexChanged(object sender, EventArgs e)
        {
            setYunsuanfuValue(FieldStr.SelectedIndex);
        }

        /// <summary>
        /// 根据字段名称及类型生成运算符
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="p">字段名称所在位置</param>
        private void setYunsuanfuValue(int p)
        {
            try
            {
                string[] arr = H_arrType.Split(',');
                string type = arr[p].ToUpper();
                if (type == "DATE")
                {
                    dateTimePicker1.Visible = true;
                    this.ValueStr.Visible = false;
                }
                else
                {
                    dateTimePicker1.Visible = false;
                    this.ValueStr.Visible = true;
                }
                //  if(type=="VARCHAR2"||type=="NVARCHAR2")
                this.MathStr.Items.Clear();

                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        this.MathStr.Items.Add("等于");
                        this.MathStr.Items.Add("不等于");
                        this.MathStr.Items.Add("大于");
                        this.MathStr.Items.Add("大于等于");
                        this.MathStr.Items.Add("小于");
                        this.MathStr.Items.Add("小于等于");
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        this.MathStr.Items.Add("等于");
                        this.MathStr.Items.Add("不等于");
                        this.MathStr.Items.Add("包含");
                        break;
                }
                this.MathStr.Text = "等于";
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.setYunsuanfuValue");
            }
        }

        /// <summary>
        /// 添加查询条件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValueStr.Visible && ValueStr.Text.Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show("查询值不能为空！", "提示");
                    return;
                }

                if (this.ValueStr.Text.IndexOf("\'") > -1)
                {
                    System.Windows.Forms.MessageBox.Show("输入的字符串中不能包含单引号!", "提示");
                    return;
                }

                string strExp = "";
                int p = FieldStr.SelectedIndex;
                string[] arr = H_arrType.Split(',');
                string type = arr[p].ToUpper();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   " + this.ValueStr.Text.Trim();
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   " + ValueStr.Text.Trim();
                        }
                        this.dataGridExp.Rows.Add(new object[] { strExp, "数字" });
                        break;
                    case "DATE":
                        string tValue = this.dateTimePicker1.Value.ToString();
                        if (tValue == "")
                        {
                            System.Windows.Forms.MessageBox.Show("查询值不能为空！", "提示");
                            return;
                        }

                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + tValue + "'";
                            this.dataGridExp.Rows.Add(new object[] { strExp, "时间" });
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + tValue + "'";
                            this.dataGridExp.Rows.Add(new object[] { strExp, "时间" });
                        }
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + ValueStr.Text.Trim() + "'";
                            if (this.MathStr.Text.Trim() == "包含")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + ValueStr.Text.Trim() + "'";
                            if (this.MathStr.Text.Trim() == "包含")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.btnAdd_Click");
            }
        }

        /// <summary>
        /// 删除查询条件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridExp.Rows.Count != 0)
                {
                    if (this.dataGridExp.CurrentRow.Index != 0)
                    {
                        this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                    }
                    else
                    {
                        if (this.dataGridExp.Rows.Count > 1)
                        {
                            this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                            string text = this.dataGridExp.Rows[0].Cells["Value"].Value.ToString().Replace("并且", "");

                            text = text.Replace("或者", "").Trim();
                            this.dataGridExp.Rows[0].Cells["Value"].Value = text;
                        }
                        else
                        {
                            this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.btnDelete_Click");
            }
        }

        /// <summary>
        /// 将查询条件转换成SQL字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <returns>SQL字符串</returns>
        private string getSqlString()
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";

                for (int i = 0; i < this.dataGridExp.Rows.Count; i++)
                {
                    string type = this.dataGridExp.Rows[i].Cells["Type"].Value.ToString();
                    string str = this.dataGridExp.Rows[i].Cells["Value"].Value.ToString();
                    if (type == "包含")
                    {
                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {
                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                str += " '%" + strArray[1] + "%'";
                            }
                        }
                        array.Add(str);
                    }
                    else if (type == "时间")
                    {

                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {

                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                //strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   to_date('" + tValue + "', 'YYYY-MM-DD HH24:MI:SS')";
                                str += "to_date('" + strArray[1] + "', 'YYYY-MM-DD HH24:MI:SS')";
                            }
                        }
                        array.Add(str);
                    }
                    else
                    {
                        array.Add(str);
                    }
                }

                for (int j = 0; j < array.Count; j++)
                {
                    if (j == 0)
                    {
                        getsql = array[j].ToString();
                    }
                    else
                    {
                        getsql += " " + array[j].ToString();
                    }
                }

                getsql = getsql.Replace("并且", "and");
                getsql = getsql.Replace("或者", "or");
                getsql = getsql.Replace("包含", "like");
                getsql = getsql.Replace("大于等于", ">=");
                getsql = getsql.Replace("小于等于", "<=");
                getsql = getsql.Replace("大于", ">");
                getsql = getsql.Replace("小于", "<");
                getsql = getsql.Replace("不等于", "!=");
                getsql = getsql.Replace("等于", "=");

                return getsql;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.getSqlString");
                return "";
            }
        }

        /// <summary>
        /// 重置按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                this.FieldStr.Text = this.FieldStr.Items[0].ToString();
                this.dataGridExp.Rows.Clear();
                this.ValueStr.Text = "";
                this.clearTem();
                this.dataPopu = null;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.getSqlString");
                this.writeToHouseLog(ex, "重置查询条件!");
            }
        }


        /* 以下是自动补全代码 */

        /// <summary>
        /// 自动补全方法
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        /// <param name="keyword">文本框中输入的值</param>
        /// <param name="colword">列名</param>
        /// <param name="tableName">表名</param>
        /// <param name="listBox1">显示自动补全值的控件</param>
        /// <returns>配配结果</returns>
        private DataTable getListBox(string keyword, string colword, string tableName)
        {
            try
            {
                DataTable dt = null;
                string strExp = "";
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\GetFromNameConfig.ini");
                string tabName = CLC.ForSDGA.GetFromTable.TableName;
                string match = CLC.INIClass.IniReadValue(tableName, "MatchField");

                if (keyword != "" && colword != "")
                {
                    if (match.IndexOf(colword) >= 0)
                    {
                        #region 生成SQL语句
                        switch (colword)
                        {
                            case "案件类型":
                                strExp = "select 类型名称 from 案件类型 t where 类型名称  like '" + keyword + "%' union select 类型名称 from 案件类型 t where 类型名称 like '%" + keyword + "%' and 类型名称 not like '" + keyword + "%' and 类型名称 not like '%" + keyword + "' union select 类型名称 from 案件类型 t where 类型名称  like '%" + keyword + "'";
                                break;
                            case "案别_案由":
                                strExp = "select 名称2 from 警综案别 t where 名称2  like '" + keyword + "%' union select 名称2 from 警综案别 t where 名称2 like '%" + keyword + "%' and 名称2 not like '" + keyword + "%' and 名称2 not like '%" + keyword + "' union select 名称2 from 警综案别 t where 名称2  like '%" + keyword + "'";
                                break;
                            case "人口性质":
                                strExp = "select 类型名称 from 人口性质 t where 类型名称  like '" + keyword + "%' union select 类型名称 from 人口性质 t where 类型名称 like '%" + keyword + "%' and 类型名称 not like '" + keyword + "%' and 类型名称 not like '%" + keyword + "' union select 类型名称 from 人口性质 t where 类型名称  like '%" + keyword + "'";
                                break;
                            case "性别":
                                strExp = "select 名称 from 性别 t where 名称  like '" + keyword + "%' union select 名称 from 性别 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 性别 t where 名称  like '%" + keyword + "'";
                                break;
                            case "民族":
                                strExp = "select 名称 from 民族 t where 名称  like '" + keyword + "%' union select 名称 from 民族 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 民族 t where 名称  like '%" + keyword + "'";
                                break;
                            case "婚姻状态":
                                strExp = "select 名称 from 婚姻状况 t where 名称  like '" + keyword + "%' union select 名称 from 婚姻状况 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 婚姻状况 t where 名称  like '%" + keyword + "'";
                                break;
                            case "政治面貌":
                                strExp = "select 名称 from 政治面貌 t where 名称  like '" + keyword + "%' union select 名称 from 政治面貌 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 政治面貌 t where 名称  like '%" + keyword + "'";
                                break;
                            case "房屋类型":
                                strExp = "select 类型名称 from 房屋类型 t where 类型名称  like '" + keyword + "%' union select 类型名称 from 房屋类型 t where 类型名称 like '%" + keyword + "%' and 类型名称 not like '" + keyword + "%' and 类型名称 not like '%" + keyword + "' union select 类型名称 from 房屋类型 t where 类型名称  like '%" + keyword + "'";
                                break;
                            case "所属派出所":
                                strExp = "select 派出所名 from 基层派出所 t where 派出所名  like '" + keyword + "%' union select 派出所名 from 基层派出所 t where 派出所名 like '%" + keyword + "%' and 派出所名 not like '" + keyword + "%' and 派出所名 not like '%" + keyword + "' union select 派出所名 from 基层派出所 t where 派出所名  like '%" + keyword + "'";
                                break;
                            case "所属中队":
                                strExp = "select 中队名 from 基层民警中队 t where 中队名  like '" + keyword + "%' union select 中队名 from 基层民警中队 t where 中队名 like '%" + keyword + "%' and 中队名 not like '" + keyword + "%' and 中队名 not like '%" + keyword + "' union select 中队名 from 基层民警中队 t where 中队名  like '%" + keyword + "'";
                                break;
                            case "所属警务室":
                                strExp = "select 警务室名 from 社区警务室 t where 警务室名  like '" + keyword + "%' union select 警务室名 from 社区警务室 t where 警务室名 like '%" + keyword + "%' and 警务室名 not like '" + keyword + "%' and 警务室名 not like '%" + keyword + "' union select 警务室名 from 社区警务室 t where 警务室名  like '%" + keyword + "'";
                                break;
                        }
                        #endregion

                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                    }
                    else
                    {
                        dt = null;
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "getListBox");
                return null;
            }
        }

        /// <summary>
        /// 列为固定值时自动添加
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        /// <param name="colName">列名</param>
        /// <param name="tableName">表名</param>
        /// <param name="listBox">显示自动补全值的控件</param>
        /// <returns>配配结果</returns>
        private DataTable MatchShu(string colName, string tableName)
        {
            try
            {
                DataTable dt = null;
                string strExp = "";
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                string tabName = CLC.ForSDGA.GetFromTable.TableName;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\GetFromNameConfig.ini");
                string match = CLC.INIClass.IniReadValue(tableName, "MatchField");
                if (colName != "" && tabName != "")
                {
                    if (match.IndexOf(colName) >= 0)
                    {
                        #region 生成SQL语句
                        switch (colName)
                        {
                            case "案件类型":
                                strExp = "select 类型名称 from 案件类型 t Group by 类型名称";
                                break;
                            case "案别_案由":
                                strExp = "select 名称2 from 警综案别 t Group by 名称2";
                                break;
                            case "人口性质":
                                strExp = "select 性质名称 from 人口性质 t Group by 性质名称";
                                break;
                            case "性别":
                                strExp = "select 名称 from 性别 t Group by 名称";
                                break;
                            case "民族":
                                strExp = "select 名称 from 民族 t Group by 名称";
                                break;
                            case "婚姻状态":
                                strExp = "select 名称 from 婚姻状况 t Group by 名称";
                                break;
                            case "政治面貌":
                                strExp = "select 名称 from 政治面貌 t Group by 名称";
                                break;
                            case "房屋类型":
                                strExp = "select 类型名称 from 房屋类型 t Group by 类型名称";
                                break;
                            case "所属派出所":
                                strExp = "select 派出所名 from 基层派出所 t Group by 派出所名";
                                break;
                            case "所属中队":
                                strExp = "select 中队名 from 基层民警中队 t Group by 中队名";
                                break;
                            case "所属警务室":
                                strExp = "select 警务室名 from 社区警务室 t Group by 警务室名";
                                break;
                        }
                        #endregion

                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                    }
                    else
                        dt = null;
                }
                return dt;
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "MatchShu");
                return null;
            }
        }

        /// <summary>
        /// 自动补全
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void ValueStr_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.ValueStr.Text.Trim(), this.FieldStr.Text, "出租屋房屋系统");

                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "ValueStr_TextChanged_1");
            }
        }

        /// <summary>
        /// 自动补全
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void ValueStr_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.FieldStr.Text, "出租屋房屋系统");

                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "ValueStr_Click_1");
            }
        }  

        /// <summary>
        /// 显示或隐藏进度条
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
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
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.isShowPro");
            }
        }

        /// <summary>
        /// 隐藏或显示条件栏
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;

                if (link.Text == "隐藏条件栏")
                {
                    this.ValueStr.Visible = false;
                    groupBox1.Visible = false;
                    link.Text = "显示条件栏";
                }
                else
                {
                    this.ValueStr.Visible = true;
                    groupBox1.Visible = true;
                    link.Text = "隐藏条件栏";
                }
            }
            catch (Exception ex)
            {
                writeToHouseLog(ex, "LinklblHides_LinkClicked");
            }
        }

        /// <summary>
        /// 处理全角半角问题
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void ValueStr_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                IntPtr HIme = ImmGetContext(this.Handle);
                if (ImmGetOpenStatus(HIme)) //如果输入法处于打开状态
                {
                    int iMode = 0;
                    int iSentence = 0;
                    bool bSuccess = ImmGetConversionStatus(HIme, ref iMode, ref iSentence); //检索输入法信息
                    if (bSuccess)
                    {
                        if ((iMode & IME_CMODE_FULLSHAPE) > 0) //如果是全角
                            ImmSimulateHotKey(this.Handle, IME_CHOTKEY_SHAPE_TOGGLE); //转换成半角
                    }
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clHouse.ucHouse.ValueStr_MouseDown");
            }
        }

        private DataTable dataPopu;
        private frmDisplay fmDis;
        /// <summary>
        /// 放大查看数据按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void btnEnalData_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataPopu == null)
                {
                    MessageBox.Show("无数据展示，请选查询出数据后放大查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new frmDisplay(dataPopu);

                fmDis.dataGridDisplay.CellClick += this.dataGV_CellClick;
                fmDis.dataGridDisplay.CellDoubleClick += this.dataGV_CellDoubleClick;

                fmDis.Show();
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "btnEnlarge_Click");
            }
        }
    }
}