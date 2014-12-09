using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using System.Data.OracleClient;
using MapInfo.Windows.Controls;

using System.Collections;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using nsGetFromName;
using System.IO;
using DY.Utility;
using System.Runtime.InteropServices;
using System.Reflection;

namespace clZonghe
{
    public partial class ucZonghe : UserControl
    {
        private string[] tableNames = new string[] { "公共场所","安全防护单位", "网吧", "治安卡口", "特种行业", "视频", "消防栓", "消防重点单位", "GPS警车定位系统", "基层派出所", "警员定位系统", "基层民警中队", "社区警务室", "临时表" };
        private MapControl mapControl1 = null;
        private ToolStrip toolStrip1 = null;
        private string strConn;          // 连接字符串
        private string[] conStr = null;  // 连接参数
        private string getFromNamePath;  // GetFromNameConfig.ini的路径
        public string strRegion = "";    // 派出所权限
        public string strRegion1 = "";   // 中队权限
        public string user = "";         // 用户名

        public string strRegion2 = "";   // 可导出的派出所
        public string strRegion3 = "";   // 可导出的中队
        public string YTexcelSql = "";   // 音头查询导出sql
        public string ZBexcelSql = "";   // 周边查询导出sql
        public string ZDexcelSql = "";   // 重点单位查询导出sql
        public string GJexcelSql = "";   // 高级查询导出sql
        public string exportSql = "";    // 导出Excel完整SQL

        ////////--关于通过车辆查询所要传的参数--//////////////
        
        public ToolStripLabel toolStriplbl = null;
        public ToolStripDropDownButton toolSbutton = null;
        public int videop = 0;  //视频监控端通讯端口
        public string[] videoConnstring = new string[6];  // 视频监控连接字符串
        public string videoexepath = string.Empty;        // 视频监控端位置
        public string KKAlSys = string.Empty;
        public string KKALUser = string.Empty;
        public double KKSearchDist = 0;
        public string string辖区 = string.Empty;

        //////////////////////////////////////////////////

        private string photoserver = string.Empty;

        public System.Data.DataTable dtExcel = null; //导出表
        public System.Data.DataTable dtEdit = null;  // 重点单位文件操作权限表（lili）

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

        private System.Data.DataTable dt = null;//DataTable表
        public ucZonghe(MapControl m, ToolStrip t, string s,string[] canStr,string FromNamePath,System.Data.DataTable temEditDt)
        {
            InitializeComponent();
            mapControl1 = m;
            toolStrip1 = t;
            strConn = s;
            conStr = canStr;
            getFromNamePath = FromNamePath;
            dtEdit = temEditDt;
            this.comboTable.SelectedIndex = 0;
            CLC.DatabaseRelated.OracleDriver.CreateConstring(canStr[0], canStr[1], canStr[2]);
            try
            {
                //添加地图事件
                mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(mapControl1_ViewChanged);
                this.mapControl1.Tools.FeatureSelected += new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);

                InitialYintouTable();

                InitialComboxText();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"ucZonghe");
            }
        }

        /// <summary>
        /// 初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        private void InitialComboxText()
        {
            try
            {
                comboClass.Text = comboClass.Items[0].ToString();
                comboType.Text = comboType.Items[0].ToString();
                comboOrAnd.SelectedIndex = 0;
            }
            catch (Exception ex) { writeZongheLog(ex, "InitialComboxText"); }
        }

        /// <summary>
        /// 音头模块初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        private void InitialYintouTable()
        {
            try
            {
                comboBox1.Items.Clear();
                OracelData linkData = new OracelData(strConn);
                DataTable datatable = linkData.SelectDataBase("select 表名 from 音头查询表 group by 表名");
                comboBox1.Items.Add("全部");
                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    comboBox1.Items.Add(datatable.Rows[i][0].ToString());
                }
                comboBox1.Text = comboBox1.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"InitialYintouTable");
            }
        }

        /// <summary>
        /// 初始化点击
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        private void setCheckBoxFasle()
        {
            try
            {
                this.checkBoxJingwushi.Checked = false;
                this.checkBoxZhongdui.Checked = false;
                this.checkBoxPaichusuo.Checked = false;
                this.checkBoxXiaofangdangwei.Checked = false;
                this.checkBoxXiaofangshuan.Checked = false;
                this.checkBoxTezhong.Checked = false;
                this.checkBoxWangba.Checked = false;
                this.checkBoxAnfang.Checked = false;
                this.checkBoxChangsuo.Checked = false;
                this.checkBoxJingyuan.Checked = false;
                this.checkBoxJingche.Checked = false;
                this.checkBoxShiping.Checked = false;
                this.checkBoxZhikou.Checked = false;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"setCheckBoxFasle");
            }
        }

        /// <summary>
        /// 功能切换时要关闭的表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25 
        /// </summary>
        private void closeTables()
        {
            try
            {
                for (int i = 0; i < tableNames.Length; i++)
                {
                    this.RemoveTemLayer(tableNames[i]);
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"closeTables");
            }
        }

        /// <summary>
        /// 初始化高级查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25 
        /// </summary>
        private void settabAdvance()
        {
            try
            {
                this.comboTable.SelectedIndex = 0;
                this.comboOrAnd.SelectedIndex = 0;
                this.comboField.SelectedIndex = 0;
                this.comboYunsuanfu.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"settabAdvance");
            }
        }

        public MapInfo.Data.Table queryTable = null;
        private string[] strName = new string[] { "点击查询", "周边查询", "音头查询", "高级查询" };  // 存取综合查询中涉及到图层的模块
        /// <summary>
        /// tab页切换
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                #region 较早前代码，用于选项卡切换时清楚各个模块的查询结果
                //音头查询，关闭叠加表
                //点击查询，将所有checkbox不check
                //其他清况，关闭叠加表，将地图工具设置为pan
                //removeTemPoints();

                ////初始化各变量
                //pageSize = 1000;     //每页显示行数
                //nMax = 0;         //总记录数
                //pageCount = 0;    //页数＝总记录数/每页显示行数
                //pageCurrent = 0;   //当前页号
                //nCurrent = 0;      //当前记录行

                ////切换tab功能项时,各列表,变量归0
                //dataGridView1.Rows.Clear();
                //PageNow1.Text = "0";
                //PageCount1.Text = "/ {0}";
                //RecordCount1.Text = "共0条记录";
                //TextNum1.Text = pageSize.ToString();

                //dataGridView2.Rows.Clear();
                //PageNow2.Text = "0";
                //PageCount2.Text = "/ {0}";
                //RecordCount2.Text = "共0条记录";
                //TextNum2.Text = pageSize.ToString();

                //dataGridView4.Rows.Clear();
                //PageNow3.Text = "0";
                //PageCount3.Text = "/ {0}";
                //RecordCount3.Text = "共0条记录";
                //TextNum3.Text = pageSize.ToString();

                //dataGridView5.DataSource = null;
                //PageNow4.Text = "0";
                //PageCount4.Text = "/ {0}";
                //RecordCount4.Text = "共0条记录";
                //TextNum4.Text = pageSize.ToString();
                //dataGridViewValue.Rows.Clear();
                //this.textValue.Text = "";
                //this.comboTable.Enabled = true;
                #endregion

                if (this.tabControl1.SelectedTab != this.tabDianji)
                {
                    this.setCheckBoxFasle();
                }
                if (this.tabControl1.SelectedTab == this.tabDianji)
                {
                    isVisbleLayer("点击查询");
                    this.LinklblHides.Visible = false; 
                }
                if (this.tabControl1.SelectedTab == this.tabZhoubian)
                {
                    isVisbleLayer("周边查询");
                    if (this.groupBox3.Visible)
                        this.LinklblHides.Text = "隐藏条件栏";
                    else
                        this.LinklblHides.Text = "显示条件栏";

                    this.LinklblHides.Visible = true;
                }
                if (tabControl1.SelectedTab == this.tabAdvance)
                {
                    isVisbleLayer("高级查询");
                    if (this.groupBox2.Visible)
                        this.LinklblHides.Text = "隐藏条件栏";
                    else
                        this.LinklblHides.Text = "显示条件栏";

                    this.LinklblHides.Visible = true;
                    //通过名称获取表名
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text.Trim(), getFromNamePath);
                    setFields(CLC.ForSDGA.GetFromTable.TableName, comboField);
                }

                if (tabControl1.SelectedTab == this.tabYintou)
                {
                    isVisbleLayer("音头查询");

                    if (this.groupBox1.Visible)
                        this.LinklblHides.Text = "隐藏条件栏";
                    else
                        this.LinklblHides.Text = "显示条件栏";

                    this.LinklblHides.Visible = true;
                }

                if (tabControl1.SelectedTab == this.tabDataTongji)
                {
                    this.LinklblHides.Visible = false; 
                    this.loadDefault();
                }

            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"tabControl1_SelectedIndexChanged");
            }
        }

        //private void isKKSearchLayer(bool flag)
        //{
        //    try
        //    {
        //        string[] searchLayer = new string[] { "KKSearchLayer", "KKSearchLabel" };

        //        for (int i = 0; i < searchLayer.Length; i++)
        //        {
        //            IMapLayer layer = mapControl1.Map.Layers[strName[i]];      // 获得所有图层
        //            layer.Enabled = flag;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        writeZongheLog(ex, "isKKSearchLayer");
        //    }
        //}

        /// <summary>
        /// 根据模块名来显示此模块的图层，隐藏不是此模块的所有图层 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tabAtils">模块名</param>
        private void isVisbleLayer(string tabAtils)
        {
            try
            {
                for (int i = 0; i < strName.Length; i++)
                {
                    FeatureLayer feat = (FeatureLayer)mapControl1.Map.Layers[strName[i]];
                    if (strName[i] != tabAtils && feat != null)
                    {
                        IMapLayer layer = mapControl1.Map.Layers[strName[i]];      // 获得所有图层
                        IMapLayer LblLayer = mapControl1.Map.Layers["标注图层"];   // 获得所有标注图层
                        if (layer.Type == LayerType.Normal)
                        {
                            layer.Enabled = false;
                            if (tabAtils != "周边查询")
                            {
                                IMapLayer layers = mapControl1.Map.Layers[0];
                                if (layers != null)
                                    layers.Enabled = false;
                            }
                        }
                        if (LblLayer.Type == LayerType.Label)
                        {
                            LabelLayer lLayer = (LabelLayer)LblLayer;
                            lLayer.Sources[strName[i]].Enabled = false;
                            if (lLayer.Sources["查询表"] != null && tabAtils != "周边查询")
                               lLayer.Sources["查询表"].Enabled = false;
                        }
                    }
                    else if (strName[i] == tabAtils && feat != null)
                    {
                        IMapLayer layer = mapControl1.Map.Layers[strName[i]];      // 获得所有图层
                        IMapLayer LblLayer = mapControl1.Map.Layers["标注图层"];   // 获得所有标注图层
                        if (layer.Type == LayerType.Normal)
                        {
                            layer.Enabled = true;
                            if (tabAtils == "周边查询")
                            {
                                IMapLayer layers = mapControl1.Map.Layers[0];
                                if (layers != null)
                                    layers.Enabled = true;
                            }
                        }
                        if (LblLayer.Type == LayerType.Label)
                        {
                            LabelLayer lLayer = (LabelLayer)LblLayer;
                            lLayer.Sources[strName[i]].Enabled = true;
                            if (tabAtils == "周边查询")
                            {
                                if (lLayer.Sources["查询表"] != null)
                                    lLayer.Sources["查询表"].Enabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "tabControl1_SelectedIndexChanged");
            }
        }

        /// <summary>
        /// 数据统计工具初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void loadDefault()
        {
            try
            {
                cmbTabName.SelectedIndex = 0;
                cmbType.SelectedIndex = 0;
                dtpStartTime.Text = System.DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                dtpEndTime.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

                string str = "select 派出所名 from 基层派出所";
                cmbPic.Items.Clear();
                cmbPic.Items.Add("所有派出所");
                addCombox(str, cmbPic);

                str = "select 中队名 from 基层民警中队";
                cmbZhongdu.Items.Clear();
                cmbZhongdu.Items.Add("所有中队");
                addCombox(str, cmbZhongdu);

                str = "select 警务室名 from 社区警务室";
                cmbJinWuShi.Items.Clear();
                cmbJinWuShi.Items.Add("所有警务室");
                addCombox(str, cmbJinWuShi);

                string strsql = "select username||'------'||真实姓名 from 用户";
                cmbName.Items.Clear();
                cmbName.Items.Add("--请选择用户--");
                cmbName.Items.Add("所有用户");
                addCombox(strsql, cmbName);

                strsql = "select 用户单位 from 用户 group by 用户单位";
                cmbDan.Items.Clear();
                cmbDan.Items.Add("--请选择单位--");
                cmbDan.Items.Add("所有单位");
                addCombox(strsql, cmbDan);
                rdoDan.Checked = true;

                rdoGdw.Checked = true;
                panGdw.Visible = true;
                panUsd.Visible = false;
                panBren.Visible = false;
            }
            catch (Exception ex) { writeZongheLog(ex, "loadDefault-数据统计工具初始化"); }
        }

        /// <summary>
        /// 填充comboBox的函数　
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sqlStr">sql语句</param>
        /// <param name="combox">要填充的comboBox</param>
        private void addCombox(string sqlStr, System.Windows.Forms.ComboBox combox)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                OracleDataReader reader = CLC.DatabaseRelated.OracleDriver.OracleComReader(sqlStr);
                while (reader.Read())
                {
                    combox.Items.Add(reader[0].ToString());
                    
                }
                combox.SelectedIndex = 0;
            }
            catch { }
        }

        /// <summary>
        /// 音头查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonQuery_Click(object sender, EventArgs e)
        {
            try
            {
                yinTouQuery();
            }
            catch (Exception ex) { writeZongheLog(ex, "buttonQuery_Click-音头查询"); }
        }

        string YTsql = "";
        /// <summary>
        /// 根据音头条件查询出数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void yinTouQuery()
        {
            try
            {
                dataGridView1.Rows.Clear();
                PageNow1.Text = "0";
                PageCount1.Text = "/ {0}";
                RecordCount1.Text = "0条";
                //removeTemPoints();
                if (this.textYintou.Text.Trim() == "")
                {
                    MessageBox.Show("音头不能为空！", "提示");
                    return;
                }
                if (this.comboBox1.Text == "")
                {
                    MessageBox.Show("查询分类不能为空！", "提示");
                    return;
                }

                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;

                YTsql = "select 名称,'更多...',表_ID,表名 from 音头查询表 where YINTOU like '%" + this.textYintou.Text.Trim().ToLower() + "%'";
                if (comboBox1.Text != "全部")
                {
                    YTsql += " and 表名='" + comboBox1.Text + "'";
                }
                //alter by siumo 2009-03-10   (edit by fisher in 09-11-26)
                string sRegion = strRegion;
                string sRegion1 = strRegion1;
                string paiNum = "";
                if (strRegion == "")               //add by fisher in 09-12-04
                {
                    isShowPro(false);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("您没有查询权限！");
                    return;
                }
                if (strRegion != "顺德区" && strRegion!="")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        sRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    paiNum = getPaiNumber(sRegion.Replace(",", "','"));
                    YTsql += " and 所属派出所代码 in ('" + paiNum.Replace(",", "','") + "')";
                }
                
                //alter by siumo 090116
                this.getMaxCount(YTsql.Replace("名称,'更多...',表_ID,表名", "count(*)"));
                InitDataSet(RecordCount1); //初始化数据集
                
                if (nMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

               
                DataTable datatable = LoadData(PageNow1,PageCount1, YTsql); //获取当前页数据
                this.toolPro.Value = 1;
                Application.DoEvents();

                #region 综合查询导出Excel
                YTexcelSql = YTsql.Replace("名称,'更多...',表_ID,表名", "名称");
                string sRegion2 = strRegion2;
                string sRegion3 = strRegion3;
                if (strRegion2 == "")
                {
                    YTexcelSql += " and 1=2 ";
                }
                else if (strRegion2 != "顺德区")
                {
                    if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                    {
                        sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                        paiNum = getPaiNumber(sRegion2.Replace(",", "','"));
                    }
                    YTexcelSql += " and 所属派出所代码 in ('" + paiNum.Replace(",", "','") + "')";
                }
                exportSql = YTexcelSql;
                //DataTable datatableExcel = LoadData(PageNow1, PageCount1, YTexcelSql);
                //if(dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion

                fillDataGridView(datatable, dataGridView1);  //填充datagridview
                this.toolPro.Value = 2;
                //Application.DoEvents();

                drawPointsInMap(datatable,comboBox1.Text.Trim());   //在地图上画点

                WriteEditLog("音头查询","音头查询表",YTsql,"查询");
                this.Cursor = Cursors.Default;
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (System.Data.OracleClient.OracleException ex)
            {
                isShowPro(false);
                this.Cursor = Cursors.Default;
                MessageBox.Show("音头查询失败：\n" + ex.Message, "提示");
                writeZongheLog(ex,"yinTouQuery");
            }
        }

        /// <summary>
        /// 将派出所名转换成派出所代码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="pai">要转换的派出所名</param>
        /// <returns>派出所代码字符</returns>
        private string getPaiNumber(string pai)
        {
            string paiNum = "";
            try
            {
                string sql = "select 派出所代码 from 基层派出所 where 派出所名 in ('" + pai + "')";
                OracleDataReader reader = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql);
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        paiNum += reader[0].ToString() + ",";
                    }
                    paiNum = paiNum.Remove(paiNum.LastIndexOf(','));
                }
                reader.Close();
                return paiNum;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "getPaiNumber-将派出所名转换成派出所代码");
                return paiNum;
            }
        }

        /// <summary>
        /// 清除地图上添加的对象
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void removeTemPoints()
        {
            try
            {
                //找到当前图层
                FeatureLayer fl = null;
                if (tabControl1.SelectedTab == this.tabYintou)
                    fl = (FeatureLayer)mapControl1.Map.Layers["音头查询"];
                if (tabControl1.SelectedTab == this.tabZhoubian)
                    fl = (FeatureLayer)mapControl1.Map.Layers["周边查询"];
                if (tabControl1.SelectedTab == this.tabDianji)
                    fl = (FeatureLayer)mapControl1.Map.Layers["点击查询"];
                if (tabControl1.SelectedTab == this.tabAdvance)
                    fl = (FeatureLayer)mapControl1.Map.Layers["高级查询"];
                if (fl == null)
                {
                    return;
                }
                Table tableTem = fl.Table;

                //清除地图上对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch(Exception ex)
            {
                writeZongheLog(ex,"removeTemPoints");
            }
        }

        MapInfo.Data.Table mainTable = null;
        /// <summary>
        /// 向地图中插入图层，并能够进行查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void insertLayerIntoMap()
        {
            MapInfo.Data.MIConnection mapConnection = new MapInfo.Data.MIConnection();
            try
            {
                if (this.dt == null)
                {
                    return;
                }
                if (this.mainTable != null)
                {
                    this.mainTable.Close();
                }
                //这个地方用来生成地图，并放在Map中显视
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet("Data", this.dt);
                MapInfo.Data.SpatialSchemaXY xy = new MapInfo.Data.SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0, 0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;
                MapInfo.Data.Table tempTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable("内存表");


                foreach (MapInfo.Data.Column col in tempTable.TableInfo.Columns) //复制表结构
                {
                    MapInfo.Data.Column col2 = col.Clone();
                    col2.ReadOnly = false;
                    mainMemTableInfo.Columns.Add(col2);
                }

                this.mainTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(mainMemTableInfo);
                string currentSql = "Insert into " + this.mainTable.Alias + "  Select * From " + tempTable.Alias;//复制图元数据
                mapConnection.Open();
                MapInfo.Data.MICommand mapCommand = mapConnection.CreateCommand();
                mapCommand.CommandText = currentSql;
                mapCommand.ExecuteNonQuery();
                currentFeatureLayer = new MapInfo.Mapping.FeatureLayer(this.mainTable, "临时表");

                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);
                tempTable.Close();
                mapCommand.Dispose();
                mapConnection.Close();
                this.setLayerStyle();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"insertLayerIntoMap");
                if (mapConnection.State == ConnectionState.Open)
                    mapConnection.Close();
                MessageBox.Show("向地图中添加临时图层失败．\n" + ex.Message, "提示");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureLayer"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        private void setLayerStyle(MapInfo.Mapping.FeatureLayer featureLayer, string name, Color color, int size)
        {
            try
            {
                MapInfo.Mapping.FeatureOverrideStyleModifier fsm = null;
                if (name == "anjian") {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, color, 9);
                    fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, new MapInfo.Styles.CompositeStyle(pStyle));
                }
                else if (name == "gonggong") {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(33, Color.Yellow, 9);
                    fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, new MapInfo.Styles.CompositeStyle(pStyle));
                }
                else if (name == "tezhong")
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(36, Color.Cyan, 10);
                    fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, new MapInfo.Styles.CompositeStyle(pStyle));
                }
                else if (name == "") {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(45, Color.Blue, 15);
                    fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, new MapInfo.Styles.CompositeStyle(pStyle));
                }
                else
                {
                    MapInfo.Styles.BitmapPointStyle bitmappointstyle = new BitmapPointStyle(name, BitmapStyles.None, color, size);
                    fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, new MapInfo.Styles.CompositeStyle(bitmappointstyle));
                }
                featureLayer.Modifiers.Clear();
                featureLayer.Modifiers.Append(fsm);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"setLayerStyle");
            }
        }
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
                writeZongheLog(ex,"setLayerStyle");
                MessageBox.Show("设置样式错误．\n" + ex.Message, "提示");
            }
        }
        private int setLayerStyle()//设置每个图层的样式
        {
            int mess = 0;
            MapInfo.Data.MIConnection mapConnection = new MapInfo.Data.MIConnection();
            try
            {
                mapConnection.Open();
                MapInfo.Styles.SimpleVectorPointStyle svs = new MapInfo.Styles.SimpleVectorPointStyle();
                svs.Color = Color.Red;
                svs.Code = 34;
                svs.PointSize = 12;
                string currentSql = "update " + this.mainTable.Alias + "  set MI_Style=@style,Obj=Obj ";
                MapInfo.Data.MICommand mapCommand = mapConnection.CreateCommand();
                mapCommand.CommandText = currentSql;
                mapCommand.Parameters.Add("@style", svs);
                mess = mapCommand.ExecuteNonQuery();

                mapCommand.Dispose();
                mapConnection.Close();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"setLayerStyle");
                if (mapConnection.State == ConnectionState.Open)
                    mapConnection.Close();
                MessageBox.Show("设置样式错误．\n" + ex.Message, "提示");
            }
            return mess;
        }

        private Color col = Color.Blue;
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
            catch 
            {
                timer1.Stop();
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.CheckBox check = (System.Windows.Forms.CheckBox)sender;
                string tableAlies = check.Text + "_Zonghe";

                if (tableAlies == "视频_Zonghe")
                {
                    //tableAlies = "社会视频";
                    tableAlies = "视频位置_Zonghe";
                }
               
                if (check.Checked)//打开表
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (this.OpenTable(tableAlies) == false)
                    {
                        System.Windows.Forms.CheckBox cb = (System.Windows.Forms.CheckBox)sender;
                        cb.Checked = false;
                    }
                    WriteEditLog("点击查询", tableAlies,"","打开");
                    this.Cursor = Cursors.Default;
                }
                if (!check.Checked)//关闭表
                {
                    this.RemoveTemLayer(tableAlies);

                    WriteEditLog("点击查询", tableAlies, "", "关闭");
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"checkBox_CheckedChanged");
            }
        }

        /// <summary>
        /// 创建临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableAiles">图层名称</param>
        public void CreateTemLayer(string tableAiles)
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

                #region 添加标注
                //string activeMapLabel = "lbl_"+tableAiles;
                //MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableAiles);
                //MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                //lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                //lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                //lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //lbsource.DefaultLabelProperties.Caption = "名称";
                //lblayer.Sources.Append(lbsource);
                //mapControl1.Map.Layers.Add(lblayer);
                #endregion
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableAiles);
                labelLayer(activeMapTable, "名称");
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "创建临时图层");
            }
        }

        /// <summary>
        /// 移除临时图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableAlies">图层名称</param>
        private void RemoveTemLayer(string tableAlies)
        {
            try
            {
                if (this.mapControl1.Map.Layers[tableAlies] != null)
                {
                    this.mapControl1.Map.Layers.Remove(tableAlies);
                    MapInfo.Engine.Session.Current.Catalog.CloseTable(tableAlies);

                    if (tableAlies == "社会视频_Zonghe")
                    {
                        this.RemoveTemLayer("非社会视频_Zonghe");
                        this.RemoveTemLayer("车载视频_Zonghe");
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"RemoveTemLayer");
            }
        }

        //打开表
        MapInfo.Mapping.FeatureLayer currentFeatureLayer;
        /// <summary>
        /// 打开表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableAlies">表名</param>
        /// <returns>布尔类型</returns>
        private bool OpenTable(string tableAlies)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(tableAlies.Substring(0,tableAlies.LastIndexOf('_')), getFromNamePath);

                string strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName;
                if (tableAlies == "车载视频_Zonghe")
                {
                    strSQL = "select CAMID as 设备编号, 终端车辆号牌 as 设备名称,所属单位 as 所属派出所,null as 日常管理人,null as MAPID,null as 设备ID, X,Y from gps警车定位系统 where CAMID is not null and X>0 and Y >0 ";
                }
                if (tableAlies == "安全防护单位_Zonghe")
                {
                    strSQL = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 单位名称 ,'点击查看' as 文件 ,X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as 表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名 from " + CLC.ForSDGA.GetFromTable.TableName;
                }

                if (CLC.ForSDGA.GetFromTable.XiaQuField != "" &&  strRegion != "顺德区" && strRegion != "")
                {//等于空时,说明无镇街字段,查询全部
                    string sRegion = strRegion;
                    if (strRegion != "顺德区")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "大良") > -1) //将大良替换为大良,德胜;即大良用户可查德胜
                        {
                            sRegion = strRegion.Replace("大良", "大良,德胜");
                        }
                        if (tableAlies == "车载视频_Zonghe")
                        {
                            strSQL = "select CAMID as 设备编号, 终端车辆号牌 as 设备名称,所属单位 as 所属派出所,null as 日常管理人,null as MAPID,null as 设备ID, X,Y from gps警车定位系统 where CAMID is not null and X>0 and Y >0 and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName + " where " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "')";
                        }
                    }
                }
                if (strRegion == "")
                {
                    if (tableAlies == "公共场所_Zonghe" && strRegion1 != "")
                    {
                        strSQL += " where " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
                    }
                    else
                    {
                        MessageBox.Show("无查询记录", "提示");
                        return false; 
                    }
                }

                //if(tableAlies =="案件"||tableAlies=="人口"||tableAlies =="出租屋房屋"){
                //    MIConnection miConnection = new MIConnection();
                //    try
                //    {
                //        Table oracleTab;
                //        miConnection.Open();

                //        TableInfoServer ti = new TableInfoServer(tableAlies, strConn.Replace("data source = ", "SRVR=").Replace("user id = ", "UID=").Replace("password = ", "PWD="), strSQL, ServerToolkit.Oci);
                //        ti.CacheSettings.CacheType = CacheOption.Off;
                //        oracleTab = miConnection.Catalog.OpenTable(ti);
  
                //        miConnection.Close();
                //        FeatureLayer fl = new FeatureLayer(oracleTab);
                //        mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);
                //     }
                //    catch (Exception ex)
                //    {
                //        if (miConnection.State == ConnectionState.Open)
                //        {
                //            miConnection.Close();
                //        }
                //        writeZongheLog(ex,"OpenTable");
                //    }
                //}
                //else{
                    OracelData linkData = new OracelData(strConn);

                    // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                    if (tableAlies != "警员_Zonghe" && tableAlies != "视频位置_Zonghe")   // 警员为视图没有备用字段一
                    {
                        if (strSQL.IndexOf("where") >= 0)
                            strSQL += " and (备用字段一 is null or 备用字段一='')";
                        else
                            strSQL += " where (备用字段一 is null or 备用字段一='')";
                    }
                    //-----------------------------------------------------------------------

                    DataTable datatable = linkData.SelectDataBase(strSQL);
                    if (datatable == null || datatable.Rows.Count < 1)
                    {
                        MessageBox.Show("无查询记录","提示");
                        return false;
                    }
                    //这个地方用来生成地图，并放在Map中显视
                    MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tableAlies, datatable);
                    MapInfo.Data.SpatialSchemaXY xy = new MapInfo.Data.SpatialSchemaXY();
                    xy.XColumn = "X";
                    xy.YColumn = "Y";
                    xy.NullPoint = "0.0, 0.0";
                    xy.StyleType = MapInfo.Data.StyleType.None;
                    xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                    ti.SpatialSchema = xy;
                    MapInfo.Data.Table  temTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                    currentFeatureLayer = new MapInfo.Mapping.FeatureLayer(temTable, tableAlies);

                    mapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                    //通过表名称获取图标
                    string bmpName = "";
                    bmpName = CLC.ForSDGA.GetFromTable.BmpName;

                    if (tableAlies == "非社会视频_Zonghe")
                    {
                        this.setLayerStyle(currentFeatureLayer, "TARG1-32.BMP", Color.Red, 12);
                    }
                    else if (tableAlies == "车载视频_Zonghe")
                    {
                        this.setLayerStyle(currentFeatureLayer, "ydsp.BMP", Color.Red, 30);
                    }else
                    {
                        this.setLayerStyle(currentFeatureLayer, bmpName, Color.Red, 12);
                    }
                    labelLayer(currentFeatureLayer.Table, CLC.ForSDGA.GetFromTable.ObjName);

                    if (tableAlies == "社会视频_Zonghe")
                    {
                        OpenTable("非社会视频_Zonghe");
                        OpenTable("车载视频_Zonghe");
                    }
                //}
                return true;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"OpenTable");
                MessageBox.Show("表不存在！","提示");
                return false;
            }
        }

        /// <summary>
        /// 清除上次查询结果
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.textYintou.Text = "";  //清空音头
            this.dataGridView1.Rows.Clear();   //清空列表
            PageNow1.Text = "0";
            PageCount1.Text = "/ {0}";
            RecordCount1.Text = "0条";
            //this.labelCount1.Visible = false;

            removeTemPoints();  //清除临时添加的点
            WriteEditLog("音头查询", "音头查询表", "", "清除查询记录");
        }

        /// <summary>
        /// 传datatable，根据坐标建点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dt2">数据表</param>
        private void addPoints(DataTable dt2)
        {
            try
            {
                FeatureLayer fl = null;
                if (this.tabControl1.SelectedTab == this.tabAdvance)
                    fl = (FeatureLayer)mapControl1.Map.Layers["高级查询"];
                if (this.tabControl1.SelectedTab == this.tabZhoubian)
                    fl = (FeatureLayer)mapControl1.Map.Layers["周边查询"];
                if(this.tabControl1.SelectedTab == this.tabDianji)
                    fl = (FeatureLayer)mapControl1.Map.Layers["点击查询"];
                if (this.tabControl1.SelectedTab == this.tabYintou)
                    fl = (FeatureLayer)mapControl1.Map.Layers["音头查询"];

                Table tableTem = fl.Table;
                
                //先清除已有对象 
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);

                double dx = 0, dy = 0;
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    //如果不是数值,跳过此记录
                    try
                    {
                        dx = Convert.ToDouble(dt2.Rows[i]["X"]);
                        dy = Convert.ToDouble(dt2.Rows[i]["Y"]);
                    }
                    catch {
                        continue;
                    }

                    if (dx > 0 && dy > 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);
                        CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 9));

                        CLC.ForSDGA.GetFromTable.GetFromName(dt2.Rows[i]["表名"].ToString(), getFromNamePath);
                        string bmpName = CLC.ForSDGA.GetFromTable.BmpName;
                        if (bmpName == "anjian")
                        {
                            MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                            cs = new CompositeStyle(pStyle);
                        }
                        else if (bmpName == "gonggong")
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
                            if (dt2.Rows[i]["表名"].ToString() == "视频位置VIEW")
                            {
                                string styid = dt2.Rows[i]["类型"].ToString();
                                if (styid == "2")
                                {
                                    cs = new CompositeStyle(new BitmapPointStyle("ydsp.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));
                                }
                                else if (styid == "1")
                                {
                                    cs = new CompositeStyle(new BitmapPointStyle("TARG1-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                                }
                                else
                                {
                                    cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                                }
                            }
                            else
                            {
                                MapInfo.Styles.BitmapPointStyle bitmappointstyle = new MapInfo.Styles.BitmapPointStyle(bmpName);
                                cs = new CompositeStyle(bitmappointstyle);
                            }
                        }

                        Feature pFeat = new Feature(tableTem.TableInfo.Columns);

                        pFeat.Geometry = pt;
                        pFeat.Style = cs;
                        pFeat["表_ID"] = dt2.Rows[i]["表_ID"].ToString();
                        pFeat["表名"] = dt2.Rows[i]["表名"].ToString();
                        pFeat["名称"] = dt2.Rows[i]["名称"].ToString();
                        tableTem.InsertFeature(pFeat);
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"addPoints");
            }
        }

        /// <summary>
        /// 在DataGridView中显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="表_ID">表_ID</param>
        /// <param name="表名">表名</param>
        public  void showDataGridViewLineOnlyOneTable(string 表_ID,string 表名)
        {
            try
            {
                if (this.tabControl1.SelectedTab == this.tabYintou)
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        if ((this.dataGridView1.Rows[i].Cells["表_ID"].Value.ToString() == 表_ID) && (this.dataGridView1.Rows[i].Cells["tabName"].Value.ToString() == 表名))
                        {
                            this.dataGridView1.CurrentCell = this.dataGridView1.Rows[i].Cells[0];
                            break;
                        }
                    }
                }
                //else if (this.tabControl1.SelectedTab == this.tabDanwei)
                //{
                //    for (int i = 0; i < this.dataGridView4.Rows.Count; i++)
                //    {
                //        if ((this.dataGridView4.Rows[i].Cells["colMapID4"].Value.ToString() == 表_ID)&&(this.dataGridView4.Rows[i].Cells["colTableName4"].Value.ToString()==表名))
                //        {
                //            this.dataGridView4.CurrentCell = this.dataGridView4.Rows[i].Cells[0];
                //            break;
                //        }
                //    }
                //}
                else if (this.tabControl1.SelectedTab == this.tabAdvance)
                {
                    for (int i = 0; i < this.dataGridView5.Rows.Count; i++)
                    {
                        if ((this.dataGridView5.Rows[i].Cells["表_ID"].Value.ToString() == 表_ID) && (this.dataGridView5.Rows[i].Cells["表名"].Value.ToString() == 表名))
                        {
                            this.dataGridView5.CurrentCell = this.dataGridView5.Rows[i].Cells[0];
                            break;
                        }
                    }
                }
                else if (this.tabControl1.SelectedTab == this.tabZhoubian)
                {
                    for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                    {
                        if ((this.dataGridView2.Rows[i].Cells["表_ID"].Value.ToString() == 表_ID) && (this.dataGridView2.Rows[i].Cells["表名"].Value.ToString() == 表名))
                        {
                            this.dataGridView2.CurrentCell = this.dataGridView2.Rows[i].Cells[0];
                            break;
                        }
                    }
                }
                else { }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"showDataGridViewLineOnlyOneTable");
            }
        }

        private MapInfo.Data.MultiResultSetFeatureCollection mirfc = null;
        private MapInfo.Data.IResultSetFeatureCollection mirfc1 = null;
        /// <summary>
        /// 图元选择事件
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
                    if (f.Table.Alias == "高级查询_selection" || f.Table.Alias == "点击查询_selection" 
                                                              || f.Table.Alias == "周边查询_selection" 
                                                              || f.Table.Alias == "音头查询_selection")
                    {
                        this.showDataGridViewLineOnlyOneTable(f["表_ID"].ToString(), f["表名"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    writeZongheLog(ex,"Feature_Selected");
                }
            }
        }

        string GJsql = "";
        //-----分页用全局变量----//
        int _startNo = 1;    // 开始行数
        int _endNo = 0;      // 结束行数
        //------------------------
        private clKaKou.ucKakou uckakou = null;
        private DataTable tablePage = null;
        /// <summary>
        /// 高级查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonMultiOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    MessageBox.Show("请添加查询语句!", "提示");
                    return;
                }

                if (getSqlString() == "")
                {
                    MessageBox.Show("查询语句有错误,请重设!", "提示");
                    return;
                }
                ClearEvent(comboTable.Text.Trim());　　　// 清除原有事件添加新事件
                // 显示查询进度条
                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;
                dataGridView5.DataSource = null;
                PageNow4.Text = "0";
                PageCount4.Text = "/ {0}";
                RecordCount4.Text = "0条";
                _endNo = Convert.ToInt32(this.TextNum4.Text);
                _startNo = 1;
                GJsql = "";
                GJexcelSql = "";
                //this.removeTemPoints();

                //通过名称获取表名，对象名
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                string tabName = CLC.ForSDGA.GetFromTable.TableName;

                if (comboTable.Text.Trim() == "信息点")
                {
                    #region 信息点查询
                    // 创建内存表存储查询结果
                    DataTable dt = new DataTable();
                    DataColumn dc = new DataColumn("序号", System.Type.GetType("System.Int32"));
                    dt.Columns.Add(dc);
                    DataColumn dc1 = new DataColumn("名称", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc1);
                    DataColumn dc2 = new DataColumn("X", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc2);
                    DataColumn dc3 = new DataColumn("Y", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc3);
                    DataColumn dc4 = new DataColumn("表序号", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc4);
                    DataColumn dc5 = new DataColumn("表名", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc5);

                    // 找到信息点图层
                    FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["信息点"];
                    Table table = fl.Table;
                    IResultSetFeatureCollection mFeatCol = MapInfo.Data.FeatureCollectionFactory.CreateResultSetFeatureCollection(table, table.TableInfo.Columns);

                    string sExpress = this.getSqlString();

                    // 查询出结果并将它存入内存表
                    MapInfo.Engine.Session.Current.Catalog.Search(table.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere(sExpress), mFeatCol, ResultSetCombineMode.Replace);
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    if (mFeatCol.Count > 0)
                    {
                        for (int i = 0; i < mFeatCol.Count; i++)
                        {
                            Feature ft = mFeatCol[i];
                            DataRow row = dt.NewRow();
                            row[0] = i + 1; row[1] = ft["NAME"].ToString();
                            row[2] = ft.Geometry.Centroid.x;
                            row[3] = ft.Geometry.Centroid.y;
                            row[4] = ft["OBJECTID"].ToString();
                            row[5] = "信息点";
                            dt.Rows.Add(row);
                        }
                        this.toolPro.Value = 2;
                        nMax = mFeatCol.Count;
                        InitDataSet(RecordCount4);
                        LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, dataGridView5, dt);
                        tablePage = dtExcel = dt;
                        this.dataGridView5.Columns["表序号"].Visible = false;
                        this.dataGridView5.Columns["表名"].Visible = false;
                    }
                    else
                    {
                        isShowPro(false);
                        MessageBox.Show("无查询结果", "结果");
                    }
                    WriteEditLog("周边查询", "信息点", sExpress, "定位中心点");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                    this.Cursor = Cursors.Default;
                    return;
                    #endregion
                }
                else if (comboTable.Text.Trim() == "通过车辆查询")
                {
                    #region 通过车辆查询
                    isShowPro(false);
                    string NewSql = this.getSqlString();    // 获得所设置条件
                    uckakou = new clKaKou.ucKakou(mapControl1, conStr, toolStriplbl, toolSbutton, videop, videoConnstring, videoexepath, KKAlSys, KKALUser, KKSearchDist, string辖区, user, getFromNamePath,false);

                    DataTable table  = uckakou.GetPassCar(NewSql);        // 通过卡口获取数据
                    nMax = table.Rows.Count;
                    InitDataSet(RecordCount4);          // 分页设置
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, dataGridView5, table);

                    tablePage = table;

                    WriteEditLog("通过车辆查询", "V_Cross", NewSql, "定位中心点");

                    this.Cursor = Cursors.Default;
                    return;
                    #endregion
                }
                //add by siumo 2008-12-30
                GJsql = this.getSqlString();
                string newSQl = "select count(*) from " + CLC.ForSDGA.GetFromTable.TableName + " where " + this.getSqlString();
                string sRegion = strRegion;
                if (strRegion != "顺德区")   // edit by fisher in 09-12-08
                {
                    if (strRegion != "")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                        {
                            sRegion = strRegion.Replace("大良", "大良,德胜");
                        }
                        GJsql += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "'))";
                        newSQl += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "'))";

                        if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                        {
                            if (GJsql.IndexOf("and") > -1)
                            {
                                GJsql = GJsql.Remove(GJsql.LastIndexOf(")"));
                                GJsql += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                                newSQl = newSQl.Remove(newSQl.LastIndexOf(")"));
                                newSQl += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                            else
                            {
                                GJsql += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                                newSQl += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                        }
                    }
                    else if (strRegion == "")
                    {
                        if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                        {
                            if (GJsql.IndexOf("and") > -1)
                            {
                                GJsql = GJsql.Remove(GJsql.LastIndexOf(")"));
                                GJsql += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                                newSQl = newSQl.Remove(newSQl.LastIndexOf(")"));
                                newSQl += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                            else
                            {
                                GJsql += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                                newSQl += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                        }
                        else
                        {
                            isShowPro(false);
                            this.Cursor = Cursors.Default;
                            MessageBox.Show("您没有查询权限!");
                            return;
                        }
                    }                    
                }
                // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                if (comboTable.Text.Trim() != "警员" && comboTable.Text.Trim() != "视频")
                {
                    newSQl += " and (备用字段一 is null or 备用字段一='')";
                    GJsql += " and (备用字段一 is null or 备用字段一='')";
                }
                //-------------------------------------------------------
                this.getMaxCount(newSQl);
                InitDataSet(RecordCount4); //初始化数据集
                this.toolPro.Value = 1;
                Application.DoEvents();
                if (nMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

                DataTable datatable = LoadData(_startNo, _endNo, GJsql, tabName,false); //获取当前页数据
                this.dataGridView5.DataSource = datatable;
                this.dataGridView5.Columns[datatable.Columns.Count - 1].Visible = false;
                this.dataGridView5.Columns[datatable.Columns.Count - 2].Visible = false;

                for (int i = 0; i < dataGridView5.Rows.Count; i++)
                {
                    if (comboTable.Text.Trim() == "安全防护单位")
                    {
                        // 给安全防护单位的文件加上链接
                        DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                        dgvlc.Value = "点击查看";
                        dgvlc.ToolTipText = "查看安全防护单位文件";
                        dataGridView5.Rows[i].Cells["文件"] = dgvlc;
                    }
                    if (i % 2 == 1)
                    {
                        dataGridView5.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                this.toolPro.Value = 2;

                #region 高级查询导出Excel

                GJexcelSql += GJsql;
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
                        GJexcelSql += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion2.Replace(",", "','") + "'))";
                        if (strRegion3 != "" && CLC.ForSDGA.GetFromTable.ZhongDuiField != "")
                        {
                            GJexcelSql = GJexcelSql.Remove(GJexcelSql.LastIndexOf(")"));
                            GJexcelSql += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion3.Replace(",", "','") + "'))";
                        }
                    }
                    else if (strRegion2 == "")
                    {
                        if (strRegion3 != "" && CLC.ForSDGA.GetFromTable.ZhongDuiField != "")
                        {
                            GJexcelSql += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion3.Replace(",", "','") + "'))";
                        }
                        else
                        {
                            GJexcelSql += " and 1=2 ";
                        }
                    }
                }
                // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示 lili 2010-8-19
                if (comboTable.Text.Trim() != "警员" && comboTable.Text.Trim() != "视频")
                    GJexcelSql += " and (备用字段一 is null or 备用字段一='')";
                //-------------------------------------------------------
                LoadData(_startNo, _endNo, GJexcelSql, tabName, true);

                //DataTable datatableExcel = LoadData(_startNo, _endNo, GJexcelSql, tabName ,true); 
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion

                //通过名称获取表名，对象名
                drawPointsInMap(datatable,CLC.ForSDGA.GetFromTable.TableName);   //在地图上画点
                WriteEditLog("高级查询", tabName, GJsql, "查询");
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex,"buttonMultiOk_Click");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 清除原有事件添加新事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="eventName">要生成事件模块</param>
        public void ClearEvent(string eventName)
        {
            try
            {
                if (eventName == "信息点" || eventName == "通过车辆查询")      // 由于分页的方式不同所以不同表选择不同事件进行分页
                {
                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator4_ItemClicked);  // 先清除之前绑定的事件
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(TextNum4_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNow4_KeyPress);

                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator_ItemClicked);   // 此处是为了避免重复添加再清除一遍
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(PageNumber_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNowText_KeyPress);

                    this.bindingNavigator4.ItemClicked += new ToolStripItemClickedEventHandler(bindingNavigator_ItemClicked);   // 再添加新事件
                    this.TextNum4.KeyPress += new KeyPressEventHandler(PageNumber_KeyPress);
                    this.PageNow4.KeyPress += new KeyPressEventHandler(PageNowText_KeyPress);
                }
                else
                {
                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator_ItemClicked);   // 先清除之前绑定的事件
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(PageNumber_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNowText_KeyPress);

                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator4_ItemClicked);  // 此处是为了避免重复添加再清除一遍
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(TextNum4_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNow4_KeyPress);
　
                    this.bindingNavigator4.ItemClicked += new ToolStripItemClickedEventHandler(bindingNavigator4_ItemClicked); // 再添加新事件
                    this.TextNum4.KeyPress += new KeyPressEventHandler(TextNum4_KeyPress);
                    this.PageNow4.KeyPress += new KeyPressEventHandler(PageNow4_KeyPress);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("程序发生异常，请重启程序后再尝试此操作！","提示",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                CLC.BugRelated.ExceptionWrite(ex, "clZonghe-ucZonghe-ClearEvent");
            }
        }

        #region 已废弃方法
        /// <summary>
        /// 将查询结果显示在地图上
        /// </summary>
        /// <param name="datatable"></param>
        /// <param name="tableName"></param>
        //private void DrawPoints(DataTable dt)
        //{
        //    try
        //    {
        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            //清除地图上添加的对象
        //            FeatureLayer lyrcar = (FeatureLayer)mapControl1.Map.Layers["高级查询"];
        //            if (lyrcar == null)
        //            {
        //                return;
        //            }
        //            Table tblcar = lyrcar.Table;

        //            //先清除已有对象
        //            (tblcar as IFeatureCollection).Clear();
        //            tblcar.Pack(PackType.All);

        //            double Cx = 0;
        //            double Cy = 0;

        //            double px = 0;
        //            double py = 0;

        //            int i = 0;

        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                if (i > 50) return;

        //                string kkname = string.Empty;
        //                string CarName = string.Empty;
        //                string idname = string.Empty;

        //                idname = dr["通行车辆编号"].ToString();
        //                kkname = dr["卡口编号"].ToString();
        //                CarName = dr["车辆号牌"].ToString();
        //                //////获取
        //                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
        //                DataTable dt1 = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select X,Y from 治安卡口系统 where 卡口编号='" + kkname + "'");

        //                if (dt1.Rows.Count > 0)
        //                {
        //                    foreach (DataRow dr1 in dt1.Rows)
        //                    {
        //                        Cx = Convert.ToDouble(dr1["X"]);
        //                        Cy = Convert.ToDouble(dr1["Y"]);
        //                    }
        //                }

        //                i = i + 1;

        //                if (Cx != 0 && Cy != 0)
        //                {
        //                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Cx, Cy)) as FeatureGeometry;
        //                    CompositeStyle cs = new CompositeStyle();
        //                    cs.ApplyStyle(new BitmapPointStyle("PIN1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 24));

        //                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
        //                    ftr.Geometry = pt;
        //                    ftr.Style = cs;
        //                    ftr["名称"] = CarName;
        //                    ftr["表_ID"] = idname;
        //                    ftr["表名"] = "车辆通过信息";
        //                    tblcar.InsertFeature(ftr);

        //                    if (px != 0 && py != 0 && Cx != 0 && Cy != 0 && textValue.Text.Trim() != "无号牌" && this.comboYunsuanfu.Text == "等于")
        //                        Trackline(px, py, Cx, Cy);

        //                    px = Cx;
        //                    py = Cy;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        writeZongheLog(ex, "DrawPoints-将查询结果显示在地图上");
        //    }
        //}
        #endregion

        Color[] colors = new Color[141] {   Color.AliceBlue,Color.AntiqueWhite,Color.Aqua,Color.Aquamarine,Color.Azure,Color.Beige,                               
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
                                            Color.Turquoise,Color.Violet,Color.Wheat,Color.White,Color.WhiteSmoke,Color.Yellow,Color.YellowGreen };

        private bool fuzzyFlag = false;         // 在卡口连线时用于判断是否是车辆模糊查询 lili 2010-12-21

        /// <summary>
        /// 创建卡口查询图层并将卡口绘制到地图上
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dt">卡口信息</param>
        private void CreateKakouTrack(DataTable dt)
        {
            try
            {
                #region 
                //try
                //{
                //    if (mapControl1.Map.Layers["KKSearchLayer"] != null)
                //    {
                //        MapInfo.Engine.Session.Current.Catalog.CloseTable("KKSearchLayer");
                //    }

                //    if (mapControl1.Map.Layers["KKSearchLabel"] != null)
                //    {
                //        mapControl1.Map.Layers.Remove("KKSearchLabel");
                //    }


                //    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //    //创建临时层
                //    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("KKSearchLayer");
                //    Table tblTemp = Cat.GetTable("KKSearchLayer");
                //    if (tblTemp != null) //Table exists close it
                //    {
                //        Cat.CloseTable("KKSearchLayer");
                //    }

                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 50));

                //    tblTemp = Cat.CreateTable(tblInfoTemp);
                //    FeatureLayer lyr = new FeatureLayer(tblTemp);
                //    //mapControl1.Map.Layers.Add(lyr);
                //    mapControl1.Map.Layers.Insert(0, lyr);

                //    //添加标注
                //    string activeMapLabel = "KKSearchLabel";
                //    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                //    MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //    MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //    lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                //    lbsource.DefaultLabelProperties.Style.Font.Size = 20;
                //    lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                //    lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //    lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                //    lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //    lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                //    lbsource.DefaultLabelProperties.Caption = "Name";
                //    lblayer.Sources.Append(lbsource);
                //    mapControl1.Map.Layers.Add(lblayer);

                //}
                //catch { }

                //MapInfo.Mapping.Map map = this.mapControl1.Map;
                //MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["高级查询"] as FeatureLayer;
                //Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("高级查询");
                //this.SetLayerEdit("KKSearchLayer");   // 设置该图层可编辑
                #endregion

                //清除地图上添加的对象
                FeatureLayer lyrcar = (FeatureLayer)mapControl1.Map.Layers["高级查询"];
                if (lyrcar == null)
                {
                    return;
                }
                Table tblKKTRA = lyrcar.Table;

                //先清除已有对象
                (tblKKTRA as IFeatureCollection).Clear();
                tblKKTRA.Pack(PackType.All);

                double x = 0;
                double y = 0;
                double px = 0;
                double py = 0;

                int i = 0;
                int ci = 0;

                InitUsedFlag();

                foreach (DataRow dr in dt.Rows)
                {
                    //通过卡口编号获取所在的经纬度
                    string KaID = dr["卡口编号"].ToString().Trim();    // 卡口编号
                    string id = dr["通行车辆编号"].ToString();         // 通行车辆编号
                    string tnum = dr["序列号"].ToString();             // 序列号

                    for (int k = 0; k < KKiD.Length; k++)
                    {
                        if (KaID == KKiD[k])
                        {
                            if (KKnum[k] != 0)
                            {
                                tnum = KKnum[k].ToString();
                            }
                            else
                            {
                                KKnum[k] = Convert.ToInt32(tnum.Trim());
                            }
                            break;
                        }
                    }

                    DataTable dt1 = GetTable("select X,Y from 治安卡口系统 where 卡口编号='" + KaID + "'");
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow dr1 in dt1.Rows)
                        {
                            if (dr1["X"].ToString() != "" && dr1["Y"].ToString() != "")
                            {
                                x = Convert.ToDouble(dr1["X"]);
                                y = Convert.ToDouble(dr1["Y"]);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    if (x != 0 && y != 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("STOP1-32.BMP", BitmapStyles.ApplyColor, colors[ci], 10));
                        Feature ftr = new Feature(tblKKTRA.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["表_ID"] = tnum;
                        ftr["名称"] = tnum;       // 序列号
                        ftr["表名"] = "V_CROSS";
                        tblKKTRA.InsertFeature(ftr);

                        if (px != 0 && py != 0 && x != 0 && y != 0 && textValue.Text.Trim() != "无号牌" && fuzzyFlag)
                            Trackline(px, py, x, y);

                        px = x;
                        py = y;
                        i = i + 1;
                        if (ci > 139)
                        {
                            ci = ci - 1;
                        }
                        else
                        {
                            ci = ci + 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "CreateKakouTrack-创建卡口查询图层");
            }
        }

        /// <summary>
        /// 车辆经过治安卡口时的轨迹线
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="x1">上次的经度</param>  
        /// <param name="y1">上次的纬度</param>  
        /// <param name="x2">本次的经度</param>  
        /// <param name="y2">本次的纬度</param>  
        private void Trackline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = null;
                MapInfo.Data.Table tblTemp = null;

                if (this.mapControl1.Map.Layers["高级查询"] != null)
                {
                    workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["高级查询"];
                    tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("高级查询");
                }

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                // 2 没有箭头
                //54 多个箭头
                // 59 单个箭头
                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(2, MapInfo.Styles.LineWidthUnit.Pixel), 59, System.Drawing.Color.Red);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "Trackline-车辆经过治安卡口时的轨迹线");
            }
        }

        string[] KKiD;
        Int32[] KKnum;
        /// <summary>
        /// 获取卡口编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void InitUsedFlag()
        {
            try
            {
                DataTable dt = GetTable("Select * from 治安卡口系统");
                if (dt.Rows.Count > 0)
                {
                    KKiD = new string[dt.Rows.Count];
                    KKnum = new Int32[dt.Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string camid = dr["卡口编号"].ToString();
                        KKiD[i] = camid;
                        KKnum[i] = 0;
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "InitUsedFlag");
            }
        }

        /// <summary>
        /// 清空条件表达式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                this.dataGridView5.DataSource = null;
                PageNow4.Text = "0";
                PageCount4.Text = "/ {0}";
                RecordCount4.Text = "0条";
                this.textValue.Text = "";
                this.comboTable.Enabled = true;
                removeTemPoints();
                dataGridViewValue.Rows.Clear();
                clearData();
                WriteEditLog("高级查询", "", "", "清空查询记录,重置查询条件");
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "buttonClear_Click");
            }
        }

        /// <summary>
        /// 页面导航(此函数为信息点及通过车辆查询分页使用)
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void bindingNavigator_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "上一页")
                {
                    pageCurrent--;
                    if (pageCurrent < 1)
                    {
                        pageCurrent = 1;
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！");
                        return;
                    }
                    else
                    {
                        nCurrent = pageSize * (pageCurrent - 1);
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
                if (e.ClickedItem.Text == "下一页")
                {
                    pageCurrent++;
                    if (pageCurrent > pageCount)
                    {
                        pageCurrent = pageCount;
                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！");
                        return;
                    }
                    else
                    {
                       nCurrent = pageSize * (pageCurrent - 1);
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (pageCurrent <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = pageSize * (pageCurrent - 1);
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
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
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
                //else if (e.ClickedItem.Text == "数据导出")
                //{
                //    DataExport();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeZongheLog(ex, "bindingNavigator_ItemClicked");
            }
        }

        /// <summary>
        /// 设置每页显示的数据量
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void PageNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && this.dataGridView5.Rows.Count > 0)
                {
                    pageSize = Convert.ToInt32(this.TextNum4.Text);
                    pageCurrent = 1;   //当前转到第一页
                    nCurrent = pageSize * (pageCurrent - 1);
                    pageCount = (nMax / pageSize);//计算出总页数
                    if ((nMax % pageSize) > 0) pageCount++;

                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "PageNumber_KeyPress");
            }
        }

        /// <summary>
        /// 页面转向
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void PageNowText_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(this.PageNow4.Text) < 1 || Convert.ToInt32(this.PageNow4.Text) > pageCount)
                    {
                        System.Windows.Forms.MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow4.Text = pageCurrent.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent = Convert.ToInt32(this.PageNow4.Text);
                        nCurrent = pageSize * (pageCurrent - 1);
                        LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "PageNow_KeyPress");
            }
        }

        //+选择单元格事件
        FrmZLMessage frmZL;
        //闪烁变量
        private Feature flashFt;
        private Style defaultStyle;
        private int k,rowIndex;
        /// <summary>
        /// 点击列表定位并显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;   //点击表头,退出

                DataGridView dataGV = (DataGridView)sender;
                int tabNum = dataGV.Rows[0].Cells.Count - 1;
                int idNum = dataGV.Rows[0].Cells.Count - 2;
                DPoint dp = new DPoint();

                if (tabControl1.SelectedTab == tabAdvance)
                    dataGridView5.Rows[e.RowIndex].Selected = true;
                if (tabControl1.SelectedTab == tabZhoubian)
                    dataGridView2.Rows[e.RowIndex].Selected = true;

                //点击更多，显示详细信息，否则地图定位
                if ((e.ColumnIndex == dataGV.Columns.Count - 5 && comboTable.Text.Trim() == "安全防护单位") || (e.ColumnIndex == dataGV.Columns.Count - 5 && comboClass.Text.Trim() == "安全防护单位"))
                {
                    #region 安全防护单位处理
                    if (dataGV.Rows[e.RowIndex].Cells[dataGV.Columns.Count - 5].Value.ToString() != "点击查看") return;

                    if (dataGV == dataGridView5 || dataGV == dataGridView2)
                    {
                        if (this.frmZL != null)
                        {
                            if (this.frmZL.Visible == true)
                            {
                                this.frmZL.Close();
                            }
                        }

                        if (dataGV.Rows[dataGV.CurrentRow.Index].Cells["名称"].Value.ToString() == "")
                        {
                            MessageBox.Show("名称不能为空！", "提示");
                            return;
                        }
                        this.frmZL = new FrmZLMessage(dataGV.Rows[dataGV.CurrentRow.Index].Cells["名称"].Value.ToString(), strConn, dtEdit);

                        //this.frmZL.SetDesktopLocation(Control.MousePosition.X + 50, Control.MousePosition.Y - this.frmZL.Height / 2);
                        //设置信息框在右下角
                        System.Drawing.Point p = this.PointToScreen(mapControl1.Parent.Location);
                        this.frmZL.SetDesktopLocation(mapControl1.Width - frmZL.Width + p.X, mapControl1.Height - frmZL.Height + p.Y + 25);
                        this.frmZL.Show();
                    }
                    #endregion
                }

                if (flashFt != null)
                {
                    try
                    {
                        flashFt.Style = defaultStyle;
                        flashFt.Update();
                    }
                    catch { }
                }

                FeatureLayer fl = null;
                if (this.tabControl1.SelectedTab == this.tabAdvance)
                    fl = (FeatureLayer)mapControl1.Map.Layers["高级查询"];
                if (this.tabControl1.SelectedTab == this.tabZhoubian)
                    fl = (FeatureLayer)mapControl1.Map.Layers["周边查询"];
                if (this.tabControl1.SelectedTab == this.tabDianji)
                    fl = (FeatureLayer)mapControl1.Map.Layers["点击查询"];
                if (this.tabControl1.SelectedTab == this.tabYintou)
                    fl = (FeatureLayer)mapControl1.Map.Layers["音头查询"];
                Table table = fl.Table;

                Feature ft = null;
                if (dataGV.Rows[e.RowIndex].Cells[tabNum].Value.ToString() == "信息点")
                {
                    #region 信息点数据展示
                    rowIndex = e.RowIndex;
                    FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["信息点"];
                    Table tableInfo = fLayer.Table;

                    MapInfo.Data.Feature feat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableInfo.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("OBJECTID=" + Convert.ToInt32(dataGV.Rows[e.RowIndex].Cells[idNum].Value)));

                    //先清除已有对象
                    (table as IFeatureCollection).Clear();
                    table.Pack(PackType.All);
                    dp.x = feat.Geometry.Centroid.x;
                    dp.y = feat.Geometry.Centroid.y;
                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 15));
                    MapInfo.Geometry.Point p = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), dp);
                    ft = new MapInfo.Data.Feature(p,cs);
                    table.InsertFeature(ft);
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(table.Alias, MapInfo.Data.SearchInfoFactory.SearchAll());
                    #endregion
                }
                else if (comboTable.Text.Trim() == "通过车辆查询" && this.tabControl1.SelectedTab == tabAdvance)
                {
                    #region 通过车辆查询数据展示
                    MapInfo.Data.MIConnection conn = new MIConnection();
                    try
                    {
                        int lastCell = dataGV.CurrentRow.Cells.Count - 1;          // 最后一列的位置
                        string serNum = dataGV.CurrentRow.Cells[lastCell].Value.ToString();  // 序列号
                        string tempname = dataGV.CurrentRow.Cells[0].Value.ToString();
                        string KaIDnu = dataGV.CurrentRow.Cells[1].Value.ToString();

                        string tblname = "高级查询";

                        MapInfo.Mapping.Map map = this.mapControl1.Map;

                        if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                        {
                            return;
                        }
                        rsfcflash = null;

                        DataRow[] row = dataZhonghe.Select("序列号='" + serNum + "'");
                        picturePoint(row[0]);                                              // 创建新的编号

                        getFeatureCollection(serNum, tblname);
                        MapInfo.Geometry.CoordSys cSys = this.mapControl1.Map.GetDisplayCoordSys();
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                break;
                            }
                            this.timerFlash.Enabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        writeZongheLog(ex, "dataGridView_CellClick-单击事件");
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                    return;
                    #endregion
                }
                else
                {
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(table.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString() + "' and 表名='" + dataGV.Rows[e.RowIndex].Cells[tabNum].Value.ToString() + "'"));
                }
                if (ft != null)
                {
                    dp = ft.Geometry.Centroid;
                    //闪烁要素
                    flashFt = ft;
                    defaultStyle = ft.Style;
                    k = 0;
                    timer1.Start();

                    // 以下代码用来将当前地图的视野缩放至该对象所在的派出所
                    //add by fisher in 09-12-24
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                    mapControl1.Map.SetView(dp, cSys, getScale());
                    this.mapControl1.Map.Center = dp;  
                }
              
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"dataGridView_CellClick");
            }
        }

        /// <summary>
        /// 改变当前卡口经过的车次
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dr">记录行</param>
        private void picturePoint(DataRow dr)
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["高级查询"] as FeatureLayer;
                Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("高级查询");
                //this.SetLayerEdit("KKSearchLayer");   // 设置该图层可编辑

                double x = 0;
                double y = 0;

                int ci = 0;

                //通过卡口编号获取所在的经纬度
                string KaID = dr["卡口编号"].ToString().Trim();    // 卡口编号
                string id = dr["通行车辆编号"].ToString();         // 通行车辆编号
                string tnum = dr["序列号"].ToString();             // 序列号
                string KaName = dr["卡口名称"].ToString();         // 卡口名称

                DataTable dt1 = GetTable("select X,Y from 治安卡口系统 where 卡口编号='" + KaID + "'");
                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow dr1 in dt1.Rows)
                    {
                        if (dr1["X"].ToString() != "" && dr1["Y"].ToString() != "")
                        {
                            x = Convert.ToDouble(dr1["X"]);
                            y = Convert.ToDouble(dr1["Y"]);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                for (int k = 0; k < dataZhonghe.Rows.Count; k++)
                {
                    if (KaName == dataZhonghe.Rows[k]["卡口名称"].ToString().Trim())
                    {
                        getFeatureCollection(dataZhonghe.Rows[k]["序列号"].ToString().Trim(), "高级查询");
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                FeatureGeometry pt = new MapInfo.Geometry.Point(lyrKKTrack.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("STOP1-32.BMP", BitmapStyles.ApplyColor, colors[ci], 10));
                                f.Geometry = pt;
                                f.Style = cs;
                                f["表_ID"] = tnum;       // 用序列号代替‘通行车辆编号’ 因‘通行车辆编号’有重复
                                f["名称"] = tnum;        // 序列号
                                f["表名"] = "V_CROSS";   // 表名
                                tblKKTRA.UpdateFeature(f);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "picturePoint");
            }
        }

        /// <summary>
        /// 根据图层找到该图层上的点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="fearID">点ID</param>
        /// <param name="tblname">图层名</param>
        private void getFeatureCollection(string fearID, string tblname)
        {
            MapInfo.Data.MIConnection conn = new MIConnection();
            rsfcflash = null;
            try
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();

                MapInfo.Data.MICommand cmd = conn.CreateCommand();
                Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where 表_ID = @name ";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@name", fearID);

                this.rsfcflash = cmd.ExecuteFeatureCollection();

                cmd.Dispose();
            }
            catch (Exception ex) { writeZongheLog(ex, "getFeatureCollection"); }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="idname"></param>
        /// <param name="kkname"></param>
        /// <param name="CarName"></param>
        private void InsertSearchPoint(string idname, string kkname, string CarName)
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;

                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["高级查询"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("高级查询");


                double Cx = 0;
                double Cy = 0;

                //////获取
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable dt1 = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select X,Y from 治安卡口系统 where 卡口编号='" + kkname + "'");

                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow dr1 in dt1.Rows)
                    {
                        Cx = Convert.ToDouble(dr1["X"]);
                        Cy = Convert.ToDouble(dr1["Y"]);
                    }
                }


                if (Cx != 0 && Cy != 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Cx, Cy)) as FeatureGeometry;
                    CompositeStyle cs = new CompositeStyle();
                    cs.ApplyStyle(new BitmapPointStyle("PIN1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 24));

                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                    ftr.Geometry = pt;
                    ftr.Style = cs;
                    ftr["名称"] = CarName;
                    ftr["MapID"] = idname;
                    tblcar.InsertFeature(ftr);

                    string tempname = idname;

                    string tblname = "高级查询";


                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    rsfcflash = null;

                    MapInfo.Data.MIConnection conn = new MIConnection();

                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    conn.Open();

                    MapInfo.Data.MICommand cmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where 表_ID = @name ";
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
                        cmd.Clone();
                        this.timerFlash.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "InsertSearchPoint");
            }
        }

        /// <summary>
        /// 获取缩放比例
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <returns>缩放比例值</returns>
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
                writeZongheLog(ex, "getScale-获取缩放比例");
                return 0;
            }
        }
        private IResultSetFeatureCollection rsfcflash = null;
        /// <summary>
        /// 双击查看详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;   //点击表头,退出

                DataGridView dataGV = (DataGridView)sender;
                int tabNum = dataGV.Rows[0].Cells.Count - 1;
                int idNum = dataGV.Rows[0].Cells.Count - 2;
                DataTable objTable = null;
                DPoint dp = new DPoint();

                string tabName = dataGV.Rows[e.RowIndex].Cells[tabNum].Value.ToString().Trim();
                if (tabName == "信息点")
                {
                    #region 信息点显示详细信息
                    try
                    {
                        DataTable dt = new DataTable();
                        DataColumn dc = new DataColumn("序号", System.Type.GetType("System.String"));
                        dt.Columns.Add(dc);
                        DataColumn dc1 = new DataColumn("名称", System.Type.GetType("System.String"));
                        dt.Columns.Add(dc1);
                        DataColumn dc2 = new DataColumn("X", System.Type.GetType("System.Double"));
                        dt.Columns.Add(dc2);
                        DataColumn dc3 = new DataColumn("Y", System.Type.GetType("System.Double"));
                        dt.Columns.Add(dc3);

                        DataRow row = dt.NewRow();
                        if (this.tabControl1.SelectedTab == this.tabYintou)
                        {
                            row[0] = dataGV.Rows[e.RowIndex].Cells["表_ID"].Value.ToString();
                            row[1] = dataGV.Rows[e.RowIndex].Cells[1].Value.ToString();
                            FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["信息点"];
                            Table tableInfo = fLayer.Table;
                            MapInfo.Data.Feature feat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableInfo.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("OBJECTID=" + dataGV.Rows[e.RowIndex].Cells["表_ID"].Value.ToString()));
                            row[2] = feat.Geometry.Centroid.x;
                            row[3] = feat.Geometry.Centroid.y;
                        }
                        else
                        {
                            row[0] = dataGV.Rows[e.RowIndex].Cells["表序号"].Value.ToString();
                            row[1] = dataGV.Rows[e.RowIndex].Cells["名称"].Value.ToString();

                            row[2] = dataGV.Rows[e.RowIndex].Cells["X"].Value.ToString();
                            row[3] = dataGV.Rows[e.RowIndex].Cells["Y"].Value.ToString();
                        }
                        dt.Rows.Add(row);
                        dp.x = Convert.ToDouble(row[2]);
                        dp.y = Convert.ToDouble(row[3]);
                        System.Drawing.Point ptZ = new System.Drawing.Point();

                        if (Convert.ToDouble(row[2]) == 0 || row[2].ToString() == "" || Convert.ToDouble(row[3]) == 0 || row[3].ToString() == "")
                        {
                            Screen screen = Screen.PrimaryScreen;
                            ptZ.X = screen.WorkingArea.Width / 2;
                            ptZ.Y = 10;

                            this.disPlayInfo(dt, ptZ, "高级查询");
                            return;
                        }
                        mapControl1.Map.DisplayTransform.ToDisplay(dp, out ptZ);
                        ptZ.X += this.Width + 10;
                        ptZ.Y += 80;

                        this.disPlayInfo(dt, ptZ, "高级查询");
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "提示");
                        return;
                    }
                    #endregion
                }
                if (comboTable.Text == "通过车辆查询" && tabControl1.SelectedTab == tabAdvance)
                {
                    #region 通过车辆查询显示详细信息
                    string[] sqlFields ={ "通行车辆编号", "卡口编号", "卡口名称", "通过时间", "车辆号牌", "号牌种类", "车身颜色", "颜色深浅", "行驶方向", "照片1", "照片2", "照片3" };

                    objTable = new DataTable("TemData");
                    for (int i = 0; i < sqlFields.Length; i++)
                    {
                        DataColumn dc = new DataColumn(sqlFields[i]);
                        if (i == 3)
                        {
                            dc.DataType = System.Type.GetType("System.DateTime");
                        }
                        else
                        {
                            dc.DataType = System.Type.GetType("System.String");
                        }
                        objTable.Columns.Add(dc);
                    }

                    DataRow dr = objTable.NewRow();
                    for (int i = 0; i < sqlFields.Length; i++)
                    {
                        dr[i] = dataGV.CurrentRow.Cells[sqlFields[i]].Value.ToString();
                    }
                    objTable.Rows.Add(dr);

                    /////////根据当前通行车辆编号判断图片服务器地址
                    try
                    {
                        if (dataGV.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "01")   //高德威视频网图片服务器地址
                        {
                            photoserver = uckakou.gdwserver;
                        }
                        else if (dataGV.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "02") //易华录视频网图片服务器地址
                        {
                            photoserver = uckakou.ehlserver;
                        }
                        else if (dataGV.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "03")  //宝康视频网图片服务器地址
                        {
                            photoserver = uckakou.bkserver;
                        }
                        else if (dataGV.Rows[e.RowIndex].Cells[0].Value.ToString().IndexOf("lbschina") > -1)
                        {
                            photoserver = "http://192.168.0.50/";
                        }
                    }
                    catch (Exception ex)
                    {
                        writeZongheLog(ex, "车辆通过信息根据ip地址获取图片服务器地址");
                    }

                    MapInfo.Geometry.DPoint dpoint = new MapInfo.Geometry.DPoint();
                    if (objTable != null && objTable.Rows.Count > 0)
                    {
                        //////////////////////////////////////////
                        string tempname = dataGV.CurrentRow.Cells[0].Value.ToString();

                        int lastCell = dataGV.CurrentRow.Cells.Count - 1;          // 最后一列的位置
                        string serNum = dataGV.CurrentRow.Cells[lastCell].Value.ToString();  // 序列号
                        string tblname = "高级查询";

                        //提取当前选择的信息的通行车辆编号作为主键值

                        MapInfo.Mapping.Map map = this.mapControl1.Map;

                        if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                        {
                            return;
                        }

                        getFeatureCollection(serNum, tblname);
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                dpoint.x = f.Geometry.Centroid.x;
                                dpoint.y = f.Geometry.Centroid.y;
                                break;
                            }
                            //this.timerFlash.Enabled = true;
                        }

                        /////////////////////////////////////////

                        if (dpoint.x == 0 || dpoint.y == 0)
                        {
                            System.Windows.Forms.MessageBox.Show("此对象未定位!");
                            return;
                        }

                        System.Drawing.Point point = new System.Drawing.Point();
                        mapControl1.Map.DisplayTransform.ToDisplay(dpoint, out point);
                        point.X += this.Width + 10;
                        point.Y += 80;
                        this.disPlayInfo(objTable, point);
                    }
                    return;
                    #endregion
                }
                //通过表名称获取相关信息
                //GetFromName getFromName = new GetFromName(tabName.ToUpper());
                CLC.ForSDGA.GetFromTable.GetFromName(tabName.ToUpper(), getFromNamePath);

                string objID = CLC.ForSDGA.GetFromTable.ObjID;

                string strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + tabName + " t where " + objID + "='" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString() + "'";
                if (tabName == "安全防护单位")
                {
                    strSQL = "select 编号,单位名称,单位性质,单位地址,主管保卫工作负责人,手机号码,所属派出所,所属中队,所属警务室,'点击查看' as 安全防护单位文件,标注人,标注时间,X,Y from " + tabName + " t where " + objID + "='" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString() + "'";
                }
                OracelData linkData = new OracelData(strConn);
                DataTable datatable = linkData.SelectDataBase(strSQL);
                System.Drawing.Point pt = new System.Drawing.Point();
                try
                {
                    dp.x = Convert.ToDouble(datatable.Rows[0]["X"]);
                    dp.y = Convert.ToDouble(datatable.Rows[0]["Y"]);
                }
                catch
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2;
                    pt.Y = 10;

                    if (datatable.Rows.Count != 0)
                        this.disPlayInfo(datatable, pt, "高级查询");
                    else
                        MessageBox.Show("查询无此记录!\n请确认数据库音头查询表中的编号!", "提示");
                    return;
                }

                if (dp.x == 0 || dp.y == 0)
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2;
                    pt.Y = 10;

                    this.disPlayInfo(datatable, pt, "高级查询");
                    return;
                }
                mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                pt.X += this.Width + 10;
                pt.Y += 80;

                this.disPlayInfo(datatable, pt, "高级查询");

                string sModule = "";
                if (tabControl1.SelectedTab == tabYintou)
                {
                    sModule = "音头查询";
                }
                else if (tabControl1.SelectedTab == tabDianji)
                {
                    sModule = "点击查询";
                }
                else if (tabControl1.SelectedTab == tabZhoubian)
                {
                    sModule = "周边查询";
                }
                else if (tabControl1.SelectedTab == tabDianji)
                {
                    sModule = "单位查询";
                }
                else
                {
                    sModule = "高级查询";
                }
                WriteEditLog(sModule, CLC.ForSDGA.GetFromTable.TableName, objID + "=" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString(), "查看详情");
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "dataGridView_CellDoubleClick");
                MessageBox.Show(ex.Message, "提示");
            }
            finally
            {
                try{
                    fmDis.Visible = false;
                }catch { }
            }
        }

        private clKaKou.FrmInfo frminfo = new clKaKou.FrmInfo();

        /// <summary>
        /// 显示通过车辆详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">显示位置</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (this.frminfo.Visible == false)
                {
                    this.frminfo = new clKaKou.FrmInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.Visible = false;
                }
                frminfo.photoserver = photoserver;
                frminfo.mapControl = this.mapControl1;
                frminfo.layerName = "高级查询";
                frminfo.getFromNamePath = this.getFromNamePath;
                frminfo.setInfo(dt.Rows[0], pt, conStr, user);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "disPlayInfo-显示车辆详细信息");
            }
        }

        private FrmInfo frmMessage = new FrmInfo();
        /// <summary>
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">显示位置</param>
        /// <param name="LayerName">图层名</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt, string LayerName)
        {
            try
            {
                if (frmMessage.Visible == false)
                {
                    frmMessage = new FrmInfo();
                    frmMessage.SetDesktopLocation(-30, -30);
                    frmMessage.Show();
                    frmMessage.Visible = false;
                }
                frmMessage.mapControl = mapControl1;
                frmMessage.getFromNamePath = getFromNamePath;
                frmMessage.setInfo(dt.Rows[0], pt, LayerName);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "disPlayInfo");
            }
        }

        #region 查找重点单位
        //private void searchDanwei()
        //{
        //    try
        //    {
        //        dataGridView4.Rows.Clear();
        //        //removeTemPoints();

        //        ZDsql = "select X, Y , 单位名称 as 名称 , 编号　as  表_ID, '安全防护单位' as 表名  from 安全防护单位 where X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2;
        //        ZDsql += " and Y>" + mapControl1.Map.Bounds.y1 + " and Y<" + mapControl1.Map.Bounds.y2;

        //        //add by siumo 2008-12-30
        //        string sRegion = "";
        //        if (strRegion == "")     //add by fisher in 09-12-04
        //        {
        //            MessageBox.Show("您没有查询权限！");
        //            return;
        //        }
        //        if (strRegion != "顺德区" && strRegion != "")
        //        {
        //            if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
        //            {
        //                sRegion = strRegion.Replace("大良", "大良,德胜");
        //            }

        //            ZDsql += " and 所属派出所 in ('" + sRegion.Replace(",", "','") + "')";
        //        }

        //        this.getMaxCount(ZDsql.Replace("X, Y , 单位名称 as 名称 , 编号　as  表_ID, '安全防护单位' as 表名", "count(*)") + " and 备用字段一 is null or 备用字段一=''");
        //        InitDataSet(RecordCount3); //初始化数据集

        //        if (nMax < 1)
        //        {
        //            MessageBox.Show("所设条件无记录.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            this.Cursor = Cursors.Default;
        //            return;
        //        }
        //        // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
        //        ZDsql += " and 备用字段一 is null or 备用字段一=''";
        //        //-----------------------------------------------------------------------

        //        DataTable datatable = LoadData(PageNow3,PageCount3, ZDsql); //获取当前页数据
        //        fillDataGridView(datatable, dataGridView4);  //填充datagridview

        //        ZDexcelSql = ZDsql.Replace("X, Y , 单位名称 as 名称 , 编号　as  表_ID, '安全防护单位' as 表名", "单位名称") + " and 备用字段一 is null or 备用字段一=''";
        //        DataTable datatableExcel = LoadData(PageNow3, PageCount3, ZDexcelSql);
        //        if (dtExcel != null) dtExcel.Clear();
        //        dtExcel = datatableExcel;

        //        drawPointsInMap(datatable,"安全防护单位");   //在地图上画点
        //        WriteEditLog("重点单位查询", "安全防护单位", ZDsql, "查询");
        //    }
        //    catch (Exception ex)
        //    {
        //        writeZongheLog(ex,"searchDanwei");
        //    }
        //}
        #endregion

        /// <summary>
        /// 定位中心点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnLocationCenter_Click(object sender, EventArgs e)
        {
            try
            {
                zhouBianCenPointQuery();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "btnLocationCenter_Click");
            }
        }

        string ZBsql = "";
        /// <summary>
        /// 周边模块查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void zhouBianCenPointQuery()
        {
            try
            {
                if (textKeyWord.Text.Trim() == "" || textKeyWord.Text.Trim() == "输入关键词")
                {
                    MessageBox.Show("请输入关键词", "提示");
                    return;
                }
                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;

                dataGridView2.DataSource = null;
                PageNow2.Text = "0";
                PageCount2.Text = "/ {0}";
                RecordCount2.Text = "0条";
                _endNo = Convert.ToInt32(this.TextNum4.Text);
                _startNo = 1;
                ZBexcelSql = "";
                ZBsql = "";
                //removeTemPoints();

                CLC.ForSDGA.GetFromTable.GetFromName(comboClass.Text.Trim(), getFromNamePath);
                if (comboClass.Text.Trim() == "信息点")
                {
                    #region 信息点查询
                    DataTable dt = new DataTable();
                    DataColumn dc = new DataColumn("序号", System.Type.GetType("System.Int32"));
                    dt.Columns.Add(dc);
                    DataColumn dc1 = new DataColumn("名称", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc1);
                    DataColumn dc2 = new DataColumn("X", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc2);
                    DataColumn dc3 = new DataColumn("Y", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc3);
                    DataColumn dc4 = new DataColumn("表序号", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc4);
                    DataColumn dc5 = new DataColumn("表名", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc5);
                    //查找ｐｏｉ
                    FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["信息点"];
                    Table table = fl.Table;
                    IResultSetFeatureCollection mFeatCol = MapInfo.Data.FeatureCollectionFactory.CreateResultSetFeatureCollection(table, table.TableInfo.Columns);

                    string sExpress = "NAME like'%" + textKeyWord.Text.Trim() + "%'";
                    if (comboType.Text.Trim() != "全部")
                    {
                        string types = getTypeColl(comboType.Text.Trim());
                        sExpress += " and FLDM in(" + types + ")";
                    }

                    MapInfo.Engine.Session.Current.Catalog.Search(table.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere(sExpress), mFeatCol, ResultSetCombineMode.Replace);
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    if (mFeatCol.Count > 0)
                    {
                        for (int i = 0; i < mFeatCol.Count; i++)
                        {
                            Feature ft = mFeatCol[i];
                            DataRow row = dt.NewRow();
                            row[0] = i + 1; row[1] = ft["NAME"].ToString();
                            row[2] = ft.Geometry.Centroid.x;
                            row[3] = ft.Geometry.Centroid.y;
                            row[4] = ft["OBJECTID"].ToString();
                            row[5] = "信息点";
                            dt.Rows.Add(row);
                            //dataGridView2.Rows.Add(i + 1, ft["NAME"], "",ft.Geometry.Centroid.x.ToString(),ft.Geometry.Centroid.y.ToString(), ft["OBJECTID"], "信息点");
                        }
                        dataZhoubian = new DataTable();
                        dataZhoubian = dt;
                        this.dataGridView2.DataSource = dt;

                        this.toolPro.Value = 2;
                        //Application.DoEvents();
                        //设置宽度
                        //setDataGridViewColumnWidth(dataGridView2);
                        for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                        {
                            if (i % 2 == 1)
                            {
                                dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                            }
                        }
                        //CountPage2.Text = "第1页共1页";
                        //BindLabelCount2.Text = "共" + mFeatCol.Count.ToString() + "条记录";
                        PageNow2.Text = "1";
                        PageCount2.Text = "/ 1";
                        RecordCount2.Text = mFeatCol.Count.ToString() + "条";
                        this.dataGridView2.Columns["表序号"].Visible = false;
                        this.dataGridView2.Columns["表名"].Visible = false;
                    }
                    else
                    {
                        isShowPro(false);
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("无查询结果", "结果");
                    }
                    WriteEditLog("周边查询", "信息点", sExpress, "定位中心点");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                    this.Cursor = Cursors.Default;
                    return;
                    #endregion
                }
                #region 之前的SQL设计
                //else{
                //    switch (comboClass.Text.Trim())
                //    {
                //        case "案件":
                //        case "人口":
                //        case "出租屋房屋":
                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称," + CLC.ForSDGA.GetFromTable.FrmFields + ", t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y ," + CLC.ForSDGA.GetFromTable.ObjID + " as  表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //        case "视频":
                //             // ZBsql = "select X,Y , " + CLC.ForSDGA.GetFromTable.ObjID + " as  表_ID," + CLC.ForSDGA.GetFromTable.ObjName + " as 名称,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名 from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";

                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称," + CLC.ForSDGA.GetFromTable.FrmFields + ",X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as  表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //        case "网吧":
                //        case "警车":
                //        case "警员":
                //        case "消防栓":
                //        case "公共场所":
                //        case "特种行业":
                //        case "治安卡口":
                //        case "消防重点单位":
                //           // ZBsql = "select X,Y , " + CLC.ForSDGA.GetFromTable.ObjID + " as  表_ID," + CLC.ForSDGA.GetFromTable.ObjName + " as 名称,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名  from " + CLC.ForSDGA.GetFromTable.TableName+ " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";

                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称," + CLC.ForSDGA.GetFromTable.FrmFields + ",X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as  表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //        case "安全防护单位":
                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称," + CLC.ForSDGA.GetFromTable.FrmFields + ",'点击查看' as 文件,X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as  表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //    }
                #endregion
                ZBsql = comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";

                string sRegion = strRegion;
                // edit by fisher in 09-12-08
                if (strRegion != "顺德区")
                {
                    if (strRegion != "")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                        {
                            sRegion = strRegion.Replace("大良", "大良,德胜");
                        }
                        ZBsql += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "'))";
                        if (strRegion1 != "" && (comboClass.Text.Trim() == "案件" || comboClass.Text.Trim() == "公共场所"))
                        {
                            if (ZBsql.IndexOf("and") > -1)
                            {
                                ZBsql = ZBsql.Remove(ZBsql.LastIndexOf(")"));
                                ZBsql += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                            else
                            {
                                ZBsql += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                        }
                    }
                    else if (strRegion == "")
                    {
                        if (strRegion1 != "" && (comboClass.Text.Trim() == "案件" || comboClass.Text.Trim() == "公共场所"))
                        {
                            if (ZBsql.IndexOf("and") > -1)
                            {
                                ZBsql = ZBsql.Remove(ZBsql.LastIndexOf(")"));
                                ZBsql += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                            else
                            {
                                ZBsql += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                            }
                        }
                        else
                        {
                            isShowPro(false);
                            this.Cursor = Cursors.Default;
                            MessageBox.Show("请确保您有查询权限!");
                            return;
                        }
                    }
                }
                //this.getMaxCount(ZBsql.Replace("t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y , 案件编号 as  表_ID,案件名称 as 名称,'案件信息' as 表名", "count(*)"));

                // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                string maxSql = "";
                if (comboClass.Text.Trim() != "警员" && comboClass.Text.Trim() != "视频")   // 警员为视图没有备用字段一
                {
                    maxSql = "select count(*) from " + CLC.ForSDGA.GetFromTable.TableName + " where " + ZBsql + " and (备用字段一 is null or 备用字段一='')";
                    ZBsql += " and (备用字段一 is null or 备用字段一='')";
                }
                else
                    maxSql = "select count(*) from " + CLC.ForSDGA.GetFromTable.TableName + " where " + ZBsql;
                //-----------------------------------------------------------------------

                this.getMaxCount(maxSql);  // edit by fisher in 09-12-17
                InitDataSet(RecordCount2); //初始化数据集

                if (nMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

                //DataTable datatable = LoadData(PageNow2, PageCount2, ZBsql); //获取当前页数据
                DataTable datatable = LoadData(_startNo, _endNo, ZBsql, CLC.ForSDGA.GetFromTable.TableName, false);
                this.toolPro.Value = 1;
                Application.DoEvents();
                //fillDataGridView(datatable, dataGridView2);  //填充datagridview
                //dataZhoubian = datatable;
                this.dataGridView2.DataSource = datatable;

                for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                {
                    if (comboClass.Text.Trim() == "安全防护单位")
                    {
                        // 给安全防护单位的文件加上链接
                        DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                        dgvlc.Value = "点击查看";
                        dgvlc.ToolTipText = "查看安全防护单位文件";
                        dataGridView2.Rows[i].Cells["文件"] = dgvlc;
                    }
                    if (i % 2 == 1)
                    {
                        dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                this.dataGridView2.Columns["表_ID"].Visible = false;
                this.dataGridView2.Columns["表名"].Visible = false;

                #region 周边查询导出Excel
                ZBexcelSql = ZBsql;
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
                        ZBexcelSql += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion2.Replace(",", "','") + "'))";
                        if (strRegion3 != "" && CLC.ForSDGA.GetFromTable.ZhongDuiField != "")
                        {
                            ZBexcelSql = ZBexcelSql.Remove(ZBexcelSql.LastIndexOf(")"));
                            ZBexcelSql += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion3.Replace(",", "','") + "'))";
                        }
                    }
                    else if (strRegion2 == "")
                    {
                        if (strRegion3 != "" && CLC.ForSDGA.GetFromTable.ZhongDuiField != "")
                        {
                            ZBexcelSql += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion3.Replace(",", "','") + "'))";
                        }
                        else
                        {
                            ZBexcelSql += " and 1=2 ";
                        }
                    }
                }
                if (comboClass.Text.Trim() != "警员" && comboClass.Text.Trim() != "视频")   // 警员为视图没有备用字段一
                    ZBexcelSql += " and (备用字段一 is null or 备用字段一='')"; // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19

                LoadData(_startNo, _endNo, ZBexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);

                ////DataTable datatableExcel = LoadData(PageNow2, PageCount2, ZBexcelSql);
                //DataTable datatableExcel = LoadData(_startNo, _endNo, ZBexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                # endregion

                this.toolPro.Value = 2;
                //Application.DoEvents();
                drawPointsInMap(datatable, CLC.ForSDGA.GetFromTable.TableName);   //在地图上画点

                WriteEditLog("周边查询", CLC.ForSDGA.GetFromTable.TableName, ZBsql, "定位中心点");
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "zhouBianCenPointQuery");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 根据不同类型返回相应的地图中代码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="p">类型名称</param>
        /// <returns>类型代码</returns>
        private string getTypeColl(string p)
        {
            try
            {
                string types = "";
                switch (p)
                {
                    case "政府机构":
                        types = "0,1,2,3";
                        break;
                    case "餐饮住宿":
                        types = "16,17";
                        break;
                    case "休闲娱乐":
                        types = "13,18,19,21";
                        break;
                    case "购物":
                        types = "32,33";
                        break;
                    case "交通":
                        types = "22,23,24,25,26,27";
                        break;
                    case "科研教育":
                        types = "8,9,10,11";
                        break;
                    case "医疗卫生":
                        types = "14,15";
                        break;
                    case "邮政电信":
                        types = "5,6,7";
                        break;
                    case "金融保险":
                        types = "4";
                        break;
                    case "公共设施":
                        types = "12,31,34";
                        break;
                    case "公司工厂":
                        types = "28,29";
                        break;
                    case "小区楼盘":
                        types = "30";
                        break;
                    case "其他":
                        types = "20,99";
                        break;
                    default:
                        types = "";
                        break;
                }
                return types;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"getTypeColl");
                return null;
            }
        }

        /// <summary>
        /// 查找周边
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnSearchAround_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.SelectedCells == null) { return; }
                this.Cursor = Cursors.WaitCursor;
                if (this.queryTable != null)
                {
                    this.queryTable.Close();
                }
                OracelData linkData = new OracelData(strConn);
                DataTable datatable = null;

                if (dataGridView2.RowCount < 1 || dataGridView2.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先定位并选择中心点!", "提示");
                    this.Cursor = Cursors.Default;
                    return;
                }

                //首先获取选择记录的x，y
                int idNum = dataGridView2.Rows[0].Cells.Count - 2;
                double x = 0, y = 0, dBufferDis = 0;
                dBufferDis = Convert.ToDouble(comboDis.Text) / 111000;
                string sql = "";

                try
                {
                    if (comboClass.Text == "信息点")
                    {
                        FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["信息点"];
                        Table tableInfo = fLayer.Table;
                        MapInfo.Data.Feature feat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableInfo.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("OBJECTID=" + Convert.ToInt32(dataGridView2.Rows[rowIndex].Cells[idNum].Value)));

                        x = feat.Geometry.Centroid.x;
                        y = feat.Geometry.Centroid.y;
                    }
                    else
                    {
                        x = Convert.ToDouble(dataGridView2.Rows[dataGridView2.SelectedCells[0].RowIndex].Cells["X"].Value);
                        y = Convert.ToDouble(dataGridView2.Rows[dataGridView2.SelectedCells[0].RowIndex].Cells["Y"].Value);
                    }
                }
                catch {
                    MessageBox.Show("该中心点无坐标!");
                    this.Cursor = Cursors.Default;
                    return;
                }

                double x1, x2;
                double y1, y2;
                x1 = x - dBufferDis;
                x2 = x + dBufferDis;
                y1 = y - dBufferDis;
                y2 = y + dBufferDis;

                double aX = 0, aY = 0;
                double dis = 0;

                string objName = comboObj.Text.Trim();
                string types = getTypeColl(objName);

                //如果不为空，那么从信息点tab中查询
                bool isHave = false;
                if (types != "")
                {
                    FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["信息点"];
                    Table table = fl.Table;
                    //先通过方框查找，再比较距离去掉不对的，形成圆形选择
                    string sExpress = "FLDM in(" + types + ") and MI_CentroidX(obj)>" + x1 + "  and  MI_CentroidX(obj)<" + x2 + "  and MI_CentroidY(obj)>" + y1 + "  and  MI_CentroidY(obj)<" + y2; ;

                    IResultSetFeatureCollection mFeatCol = MapInfo.Data.FeatureCollectionFactory.CreateResultSetFeatureCollection(table, table.TableInfo.Columns);
                    MapInfo.Engine.Session.Current.Catalog.Search(table.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere(sExpress), mFeatCol, ResultSetCombineMode.Replace);

                    createInfoPointLayer(table.TableInfo.Columns);
                    isHave = false;
                    if (mFeatCol.Count > 0)
                    {
                        for (int i = 0; i < mFeatCol.Count; i++)
                        {
                            Feature ft = mFeatCol[i];
                            aX = ft.Geometry.Centroid.x;
                            aY = ft.Geometry.Centroid.y;
                            dis = calDisTwoPoints(x, y, aX, aY);
                            if (dis / 111000 <= dBufferDis)
                            {
                                queryTable.InsertFeature(ft);
                                isHave = true;
                            }
                        }
                    }
                    if (isHave == false)
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("给定范围内无相关对象。", "提示");
                        return;
                    }
                    FeatureLayer fLayer = new MapInfo.Mapping.FeatureLayer(this.queryTable, "queryLayer");

                    mapControl1.Map.Layers.Insert(0, fLayer);
                    this.setLayerStyle(fLayer, Color.Blue, 45, 15);
                    labelLayer(fLayer.Table,"Name");
                    WriteEditLog("周边查询", "信息点", sExpress, "查询");
                }
                else
                {
                    //通过名称获取表名
                    GetFromName getFromName = new GetFromName(objName);
                    CLC.ForSDGA.GetFromTable.GetFromName(objName, getFromNamePath);
                    string tableName = CLC.ForSDGA.GetFromTable.TableName;
                    //先通过方框查找，再比较距离去掉不对的，形成圆形选择
                    sql = "select * from " + tableName + " where X>" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                    if (objName == "人口" || objName == "出租屋房屋") {
                        sql = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + tableName + " t where  t.geoloc.SDO_POINT.X>" + x1 + "  and  t.geoloc.SDO_POINT.X<" + x2 + "  and t.geoloc.SDO_POINT.Y>" + y1 + "  and  t.geoloc.SDO_POINT.Y<" + y2;
                    }
                    DataSet dataset = new System.Data.DataSet();
                    dataset = linkData.SelectDataBase(sql, "QueryDataTable");
                    datatable = dataset.Tables[0];
                    // DataTable dtNew = new DataTable();
                    DataRow[] drArr = new DataRow[datatable.Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in datatable.Rows)
                    {
                        aX = Convert.ToDouble(dr["X"]);
                        aY = Convert.ToDouble(dr["Y"]);
                        dis = calDisTwoPoints(x, y, aX, aY);
                        if (dis / 111000 > dBufferDis)
                        {
                            //datatable.Rows.Remove(dr);//移除后,下一个循环会出错
                            //DataRow drNew = dr;
                            //dtNew.Rows.Add(drNew);
                            drArr[i] = dr;
                            i++;
                        }
                    }
                    for (int j = 0; j < i; j++)
                    {
                        datatable.Rows.Remove(drArr[j]);
                    }
                    drArr = null;

                    if (datatable.Rows.Count < 1)
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("给定范围内无相关对象。", "提示");
                        return;
                    }
                    this.insertQueryIntoMap(datatable);//向地图中插入图层   
                    WriteEditLog("周边查询", tableName, sql, "查询");
                }

            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"btnSearchAround_Click");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <returns></returns>
        private double calDisTwoPoints(double X1, double Y1, double X2, double Y2)
        {
            try
            {
                double d;
                //将经纬度转成弧度
                X1 = X1 / 180 * Math.PI;
                Y1 = Y1 / 180 * Math.PI;
                X2 = X2 / 180 * Math.PI;
                Y2 = Y2 / 180 * Math.PI;
                d = Math.Sin(Y1) * Math.Sin(Y2) + Math.Cos(Y1) * Math.Cos(Y2) * Math.Cos(X1 - X2);
                d = Math.Acos(d) * 6371004;
                d = Math.Round(d);
                return d;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "calDisTwoPoints");
                return 0;
            }
        }

        private void createInfoPointLayer(Columns cols)
        {
            try
            {
                if (queryTable != null)
                {
                    this.queryTable.Close();
                }
                MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable("查询表");

                foreach (MapInfo.Data.Column col in cols) //复制表结构
                {
                    MapInfo.Data.Column col2 = col.Clone();
                    
                    col2.ReadOnly = false;
                    mainMemTableInfo.Columns.Add(col2);
                }

                this.queryTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(mainMemTableInfo);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"createInfoPointLayer");
            }
        }

        private void insertQueryIntoMap(DataTable datatable)
        {
            MapInfo.Data.MIConnection mapConnection = new MapInfo.Data.MIConnection();
            try
            {
                if (datatable == null)
                {
                    return;
                }
                //这个地方用来生成地图，并放在Map中显视
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet("QueryData", datatable);
                MapInfo.Data.SpatialSchemaXY xy = new MapInfo.Data.SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0, 0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;
                MapInfo.Data.Table tempTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                createInfoPointLayer(tempTable.TableInfo.Columns);

                string currentSql = "Insert into " + this.queryTable.Alias + "  Select * From " + tempTable.Alias;//复制图元数据
                mapConnection.Open();
                MapInfo.Data.MICommand mapCommand = mapConnection.CreateCommand();
                mapCommand.CommandText = currentSql;
                mapCommand.ExecuteNonQuery();
                currentFeatureLayer = new MapInfo.Mapping.FeatureLayer(this.queryTable, "queryLayer");
                //currentFeatureLayer = new MapInfo.Mapping.FeatureLayer(tempTable, "queryLayer");

                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);
                tempTable.Close();
                mapCommand.Dispose();
                mapConnection.Close();
                CLC.ForSDGA.GetFromTable.GetFromName(comboObj.Text.Trim(), getFromNamePath);
                string bmpName = CLC.ForSDGA.GetFromTable.BmpName;
                this.setLayerStyle(currentFeatureLayer, bmpName, Color.Blue, 15);

                labelLayer(currentFeatureLayer.Table,CLC.ForSDGA.GetFromTable.ObjName);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"insertQueryIntoMap");
                if (mapConnection.State == ConnectionState.Open)
                    mapConnection.Close();
                MessageBox.Show("添加临时对象层出错.\n" + ex.Message, "提示");
            }
        }

        private void labelLayer(Table editTable,string labelField)
        {
            try
            {
                LabelLayer labelLayer = mapControl1.Map.Layers["标注图层"] as LabelLayer;

                LabelSource source = new LabelSource(editTable);

                source.DefaultLabelProperties.Caption = labelField;
                source.DefaultLabelProperties.Layout.Offset = 4;
                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;
                //source.DefaultLabelProperties.Visibility.VisibleRangeEnabled = true;
                //source.DefaultLabelProperties.Visibility.VisibleRange = new VisibleRange(0.0, 10, DistanceUnit.Kilometer);

                labelLayer.Sources.Insert(0, source);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"labelLayer");
            }
        }

        private string[] getWhere(string str)
        {
            string[] sit = new string[2];
            string _whereName = "", _whereFldm = "";
            try
            {
                if (str.IndexOf("全部") >= 0)
                    _whereName = str.Replace("全部", "NAME");
                if (str.IndexOf("政府机构") >= 0)
                {
                    _whereName = str.Replace("政府机构", "NAME");
                    _whereFldm = " and FLDM in(0,1,2,3)";
                }
                if (str.IndexOf("餐饮住宿") >= 0)
                {
                    _whereName = str.Replace("餐饮住宿", "NAME");
                    _whereFldm = " and FLDM in(16,17)";
                }
                if (str.IndexOf("休闲娱乐") >= 0)
                {
                    _whereName = str.Replace("休闲娱乐", "NAME");
                    _whereFldm = " and FLDM in(13,18,19,21)";
                }
                if (str.IndexOf("购物") >= 0)
                {
                    _whereName = str.Replace("购物", "NAME");
                    _whereFldm = " and FLDM in(32,33)";
                }
                if (str.IndexOf("交通") >= 0)
                {
                    _whereName = str.Replace("交通", "NAME");
                    _whereFldm = " and FLDM in(22,23,24,25,26,27)";
                }
                if (str.IndexOf("科研教育") >= 0)
                {
                    _whereName = str.Replace("科研教育", "NAME");
                    _whereFldm = " and FLDM in(8,9,10,11)";
                }
                if (str.IndexOf("医疗卫生") >= 0)
                {
                    _whereName = str.Replace("医疗卫生", "NAME");
                    _whereFldm = " and FLDM in(14,15)";
                }
                if (str.IndexOf("邮政电信") >= 0)
                {
                    _whereName = str.Replace("邮政电信", "NAME");
                    _whereFldm = " and FLDM in(5,6,7)";
                }
                if (str.IndexOf("金融保险") >= 0)
                {
                    _whereName = str.Replace("金融保险", "NAME");
                    _whereFldm = " and FLDM in(4)";
                }
                if (str.IndexOf("公共设施") >= 0)
                {
                    _whereName = str.Replace("公共设施", "NAME");
                    _whereFldm = " and FLDM in(12,31,34)";
                }
                if (str.IndexOf("公司工厂") >= 0)
                {
                    _whereName = str.Replace("公司工厂", "NAME");
                    _whereFldm = " and FLDM in(28,29)";
                }
                if (str.IndexOf("小区楼盘") >= 0)
                {
                    _whereName = str.Replace("小区楼盘", "NAME");
                    _whereFldm = " and FLDM in(30)";
                }
                if (str.IndexOf("其他") >= 0)
                {
                    _whereName = str.Replace("其他", "NAME");
                    _whereFldm = " and FLDM in(20,99)";
                }
                sit[0] = _whereName;
                sit[1] = _whereFldm;
                return sit;
            }
            catch (Exception ex) { writeZongheLog(ex, "getWhere"); return sit; }
        }

        /// <summary>
        /// 转换字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <returns>条件字符串</returns>
        private string getSqlString()//转换字符串
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";

                for (int i = 0; i < this.dataGridViewValue.Rows.Count; i++)
                {
                    string type = this.dataGridViewValue.Rows[i].Cells["Type"].Value.ToString();
                    string str = this.dataGridViewValue.Rows[i].Cells["Value"].Value.ToString();

                    //if (comboTable.Text == "通过车辆查询")    // 通过车辆查询的卡口名称要转换为卡口编号后查询  add by lili 2010-12-16
                    //    str = transSerial(str);

                    if (str.IndexOf("车辆号牌 等于") > -1)
                        fuzzyFlag = true;

                    if (type == "包含")
                    {
                        if (comboTable.Text == "信息点")
                        {
                            string[] strArray = new string[3];
                            strArray = str.Split('\'');
                            str = "";
                            for (int j = 0; j < strArray.Length; j++)
                            {
                                if (j == 0)
                                {
                                    str = getWhere(strArray[0])[0];
                                }
                                if (j == 1)
                                {
                                    str += " '%" + strArray[1] + "%' " + getWhere(strArray[0])[1];
                                }
                            }
                        }
                        else
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
                                str += "to_date('" + strArray[1] + "', 'YYYY-MM-DD HH24:MI:SS')";
                            }
                        }
                        array.Add(str);
                    }
                    else
                    {
                        if (comboTable.Text == "信息点")
                        {
                            str = getWhere(str)[0] + " " + getWhere(str)[1];
                        }
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
                        getsql += "   " + array[j].ToString();
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
                writeZongheLog(ex,"getSqlString");
                return "";
            }          
        }

        /// <summary>
        /// 此方法只供getSqlString方法使用，用于转换治安卡口名称转换为卡口编号（此功能已取消，此方法未使用）
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tansSer">要转换的sql</param>
        private string transSerial(string tansSer)
        {
            try
            {
                string newSql = "";

                if (tansSer.IndexOf("卡口名称 等于") > -1)
                {
                    string serails = tansSer.Substring(tansSer.IndexOf("'"));

                    CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                    DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select 卡口编号 from 治安卡口系统 where 卡口名称=" + serails);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (newSql == string.Empty)
                            newSql = "卡口编号 in ('" + dt.Rows[i][0].ToString() + "'";
                        else
                            newSql += ",'" + dt.Rows[i][0].ToString() + "'";
                    }

                    newSql += ")";
                    return newSql;
                }
                else
                {
                    return tansSer;
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "transSerial");
                return tansSer;
            }
        }

        /// <summary>
        /// 添加一个表达式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboTable.Text.Trim() == "")
                {
                    MessageBox.Show("请选择表", "提示");
                    return;
                }

                if (textValue.Visible && textValue.Text.Trim() == "")
                {
                    MessageBox.Show("查询值不能为空！", "提示");
                    return;
                }

                if (this.textValue.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("输入的字符串中不能包含单引号!", "提示");
                    return;
                }
  
                string strExp = "";
                int p = comboField.SelectedIndex;
                string[] arr = arrType.Split(',');
                string type = arr[p].ToUpper();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + " " + this.comboYunsuanfu.Text + " " + textValue.Text.Trim();
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + " " + this.comboField.Text + " " + this.comboYunsuanfu.Text + " " + textValue.Text.Trim();
                        }
                        this.dataGridViewValue.Rows.Add(new object[] { strExp, "数字" });
                        break;
                    case "DATE":
                        string tValue = this.dateTimePicker1.Value.ToString();
                        if (tValue == "")
                        {
                            MessageBox.Show("查询值不能为空！", "提示");
                            return;
                        }

                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "时间" });
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + " " + this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "时间" });
                        }
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "包含")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + " " + this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "包含")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        break;
                }
                this.comboTable.Enabled = false;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"buttonAdd_Click");
            }
        }

        /// <summary>
        /// 移除一个表达式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonRemove_Click(object sender, EventArgs e)
        {
           try
            {
                if (this.dataGridViewValue.CurrentRow.Index != 0)
                {
                    this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                }
                else
                {
                    this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                    string text= this.dataGridViewValue.Rows[0].Cells["Value"].Value.ToString().Replace("并且", "");
                    
                    text = text.Replace("或者", "").Trim();
                    this.dataGridViewValue.Rows[0].Cells["Value"].Value=text;
                }

                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    this.comboTable.Enabled = true;
                }
            }
            catch(Exception ex)
            {
                writeZongheLog(ex,"buttonRemove_Click");
            }
        }

        /// <summary>
        /// 切换查询表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void comboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //通过名称获取表名
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text.Trim(), getFromNamePath);
                setFields(CLC.ForSDGA.GetFromTable.TableName,comboField);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"comboTable_SelectedIndexChanged");
            }
        }

        string arrType = "";
        /// <summary>
        /// 根据表名将相关字段及字段类型添加到ComboBox控件中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="comboBoxField">ComboBox控件</param>
        private void setFields(string tableName, System.Windows.Forms.ComboBox comboBoxField)
        {
            try
            {
                OracelData linkData = new OracelData(strConn);
                string sExp="";
                if (tableName == "gps警员")
                {
                    comboBoxField.Items.Clear();
                    comboBoxField.Items.Add("警力编号");    comboBoxField.Items.Add("派出所名");
                    comboBoxField.Items.Add("中队名");   　 comboBoxField.Items.Add("所属科室");
                    comboBoxField.Items.Add("当前任务");    comboBoxField.Items.Add("设备编号");
                    comboBoxField.Items.Add("定位更新时间");
                    arrType = "NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,DATE";
                }
                else if(tableName=="信息点")
                {
                    comboBoxField.Items.Clear();
                    comboBoxField.Items.Add("全部");     comboBoxField.Items.Add("政府机构");
                    comboBoxField.Items.Add("餐饮住宿"); comboBoxField.Items.Add("休闲娱乐");
                    comboBoxField.Items.Add("购物");     comboBoxField.Items.Add("交通");
                    comboBoxField.Items.Add("科研教育"); comboBoxField.Items.Add("医疗卫生");
                    comboBoxField.Items.Add("邮政电信"); comboBoxField.Items.Add("金融保险");
                    comboBoxField.Items.Add("公共设施"); comboBoxField.Items.Add("公司工厂");
                    comboBoxField.Items.Add("小区楼盘"); comboBoxField.Items.Add("其他");
                    arrType = "NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2";
                }
                else if (this.comboTable.Text == "通过车辆查询" && tabControl1.SelectedTab == tabAdvance)
                {
                    comboBoxField.Items.Clear();
                    comboBoxField.Items.Add("卡口编号"); comboBoxField.Items.Add("卡口名称");
                    comboBoxField.Items.Add("通过时间"); comboBoxField.Items.Add("车辆号牌");
                    comboBoxField.Items.Add("号牌种类"); comboBoxField.Items.Add("车身颜色");
                    comboBoxField.Items.Add("颜色深浅"); comboBoxField.Items.Add("卡口方向");
                    arrType = "NVARCHAR2,NVARCHAR2,DATE,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2";
                }
                else
                {
                    sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME= '" + tableName + "'";
                    DataTable dt = linkData.SelectDataBase(sExp);
                    comboBoxField.Items.Clear();
                    arrType = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string aCol = dt.Rows[i][0].ToString();
                        string atype = dt.Rows[i][1].ToString().ToUpper();

                        if (aCol != "" && aCol != "MAPID" && aCol.IndexOf("备用字段") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("代码") < 0)
                        {
                            //周边查询中,只对字符型字段进行查询
                            if (comboBoxField == comboField2 && (atype == "DATE" || atype == "NUMBER" || atype == "INTEGER"))
                            {
                                continue;
                            }
                            comboBoxField.Items.Add(aCol);
                            arrType += atype + ",";
                        }
                    }
                }
                comboBoxField.Text = comboBoxField.Items[0].ToString();
                setYunsuanfuValue(0);
            }
            catch(Exception ex)
            {
                writeZongheLog(ex,"setFields");
            }
        }

        /// <summary>
        /// 选择字段
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void comboField_SelectedIndexChanged(object sender, EventArgs e)
        {
            setYunsuanfuValue(comboField.SelectedIndex);
        }

        /// <summary>
        /// 根据不同的类型添加相应比较符
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="p">类型数组中的位置</param>
        private void setYunsuanfuValue(int p)
        {
            try
            {
                this.textValue.Text = "";
                string[] arr = arrType.Split(',');
                string type = arr[p].ToUpper();
                if (type == "DATE")
                {
                    dateTimePicker1.Visible = true;
                    this.dateTimePicker1.Text = System.DateTime.Now.ToString();
                    textValue.Visible = false;
                }
                else
                {
                    dateTimePicker1.Visible = false;
                    textValue.Visible = true;
                }
                //  if(type=="VARCHAR2"||type=="NVARCHAR2")
                comboYunsuanfu.Items.Clear();

                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        comboYunsuanfu.Items.Add("等于");
                        comboYunsuanfu.Items.Add("不等于");
                        comboYunsuanfu.Items.Add("大于");
                        comboYunsuanfu.Items.Add("大于等于");
                        comboYunsuanfu.Items.Add("小于");
                        comboYunsuanfu.Items.Add("小于等于");
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        comboYunsuanfu.Items.Add("等于");
                        comboYunsuanfu.Items.Add("不等于");
                        comboYunsuanfu.Items.Add("包含");
                        break;
                }
                comboYunsuanfu.Text = "等于";
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"setYunsuanfuValue");
            }
        }

        //
        /// <summary>
        /// 地图视野发生变化时，实时搜索重点单位
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void mapControl1_ViewChanged(object sender, EventArgs e)
        {
            //if (this.Visible)
            //{
            //    if (checkBoxZhongdian.Checked) return;  //如果使用关键词查询，关闭查询当前视野。
            //    try
            //    {
            //        if (tabControl1.SelectedTab == tabDanwei)
            //        {
            //            searchDanwei();
            //        }
            //    }
            //    catch { }
            //}
        }

        /// <summary>
        /// 周边定位中心点，如果是信息点，分类可见
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void comboClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboClass.Text == "信息点")
                {
                    comboType.Visible = true;
                    comboField2.Visible = false;
                    label12.Text = "选择类型";
                }
                else
                {
                    comboField2.Visible = true;
                    comboType.Visible = false;
                    label12.Text = "选择字段";
                    //通过名称获取表名
                    CLC.ForSDGA.GetFromTable.GetFromName(comboClass.Text.Trim(), getFromNamePath);
                    setFields(CLC.ForSDGA.GetFromTable.TableName, comboField2);
                }
            }
            catch (Exception ex) {
                writeZongheLog(ex, "comboClass_SelectedIndexChanged");
            }
        }

        /// <summary>
        /// 切换综合模块
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void ucZonghe_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    FeatureLayer fl = null;
                    if (tabControl1.SelectedTab == tabAdvance)
                        fl = (FeatureLayer)mapControl1.Map.Layers["高级查询"];
                    if (tabControl1.SelectedTab == tabDianji)
                        fl = (FeatureLayer)mapControl1.Map.Layers["点击查询"];
                    if (tabControl1.SelectedTab == tabZhoubian)
                        fl = (FeatureLayer)mapControl1.Map.Layers["周边查询"];
                    if (tabControl1.SelectedTab == tabYintou)
                        fl = (FeatureLayer)mapControl1.Map.Layers["音头查询"];

                    Table tableTem = fl.Table; 
                }
                else
                {
                    //切换功能项时,各列表,变量归0
                    dataGridView1.Rows.Clear();
                    PageNow1.Text = "0";
                    PageCount1.Text = "/ {0}";
                    RecordCount1.Text = "0条";
                    dataGridView2.DataSource = null;
                    PageNow2.Text = "0";
                    PageCount2.Text = "/ {0}";
                    RecordCount2.Text = "0条";

                    dataGridView5.DataSource = null;
                    PageNow4.Text = "0";
                    PageCount4.Text = "/ {0}";
                    RecordCount4.Text = "0条";

                    //pageSize = 0;     //每页显示行数
                    nMax = 0;         //总记录数
                    pageCount = 0;    //页数＝总记录数/每页显示行数
                    pageCurrent = 0;   //当前页号
                    nCurrent = 0;      //当前记录行

                    setCheckBoxFasle();

                    closeTables();
                    if (this.queryTable != null)
                    {
                        this.queryTable.Close();
                    }

                    RemoveTemLayer("高级查询");
                    RemoveTemLayer("点击查询");
                    RemoveTemLayer("周边查询"); 
                    RemoveTemLayer("音头查询");
                    RemoveTemLayer("lbl_高级查询");
                    RemoveTemLayer("lbl_点击查询");
                    RemoveTemLayer("lbl_周边查询");
                    RemoveTemLayer("lbl_音头查询");
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"ucZonghe_VisibleChanged");
            }
        }

        #region 删除的重点单位代码
        //private void buttonSearch_Click(object sender, EventArgs e)
        //{
        //    zhongDianQuery();
        //}

        //string ZDsql = "";
        //private void zhongDianQuery()
        //{
        //    try
        //    {
        //        if (textWord.Text == "")
        //        {
        //            MessageBox.Show("请输入关键词!", "提示");
        //            return;
        //        }
        //        isShowPro(true);
        //        this.Cursor = Cursors.WaitCursor;
        //        dataGridView4.Rows.Clear();
        //        PageNow3.Text = "0";
        //        PageCount3.Text = "/ {0}";
        //        RecordCount3.Text = "共0条记录";
        //        //removeTemPoints();

        //        ZDsql = "select X, Y , 单位名称 as 名称 , 编号　as  表_ID, '安全防护单位' as 表名  from 安全防护单位 where ";
        //        ZDsql += cmbDanWei.Text + " like '%" + textWord.Text + "%'";

        //        //add by siumo 2008-12-30
        //        string sRegion = strRegion;
        //        if (strRegion == "")
        //        {
        //            isShowPro(false);
        //            this.Cursor = Cursors.Default;
        //            MessageBox.Show("您没有查询权限！");
        //            return;
        //        }
        //        if (strRegion != "顺德区"&& strRegion!="")
        //        {
        //            if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
        //            {
        //                sRegion = strRegion.Replace("大良", "大良,德胜");
        //            }

        //            ZDsql += " and 所属派出所 in ('" + sRegion.Replace(",", "','") + "')";
        //        }

        //        this.getMaxCount(ZDsql.Replace("X, Y , 单位名称 as 名称 , 编号　as  表_ID, '安全防护单位' as 表名", "count(*)") + " and 备用字段一 is null or 备用字段一=''");
        //        InitDataSet(RecordCount3); //初始化数据集

        //        if (nMax < 1)
        //        {
        //            isShowPro(false);
        //            MessageBox.Show("所设条件无记录.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            this.Cursor = Cursors.Default;
        //            return;
        //        }

        //        ZDsql += " and 备用字段一 is null or 备用字段一=''";  // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
        //        DataTable datatable = LoadData(PageNow3,PageCount3, ZDsql); //获取当前页数据
        //        fillDataGridView(datatable, dataGridView4);  //填充datagridview
        //        this.toolPro.Value = 1;
        //        Application.DoEvents();

        //        #region 重点单位查询导出Excel
        //        string sRegion2 = strRegion2;
        //        string sRegion3 = strRegion3;
        //        if (strRegion2 == "")
        //        {
        //            ZDexcelSql += " and 1=2 ";
        //        }
        //        else if (strRegion2 != "顺德区")
        //        {
        //            if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
        //            {
        //                sRegion2 = strRegion2.Replace("大良", "大良,德胜");
        //            }
        //            ZDexcelSql += " and 所属派出所 in ('" + sRegion2.Replace(",", "','") + "')";
        //        }
        //        ZDexcelSql = ZDsql.Replace("X, Y , 单位名称 as 名称 , 编号　as  表_ID, '安全防护单位' as 表名", "单位名称") + " and 备用字段一 is null or 备用字段一=''";  // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19;
        //        DataTable datatableExcel = LoadData(PageNow3, PageCount3, ZDexcelSql);
        //        if (dtExcel != null) dtExcel.Clear();
        //        dtExcel = datatableExcel;
        //        #endregion
        //        this.toolPro.Value = 2;
        //        Application.DoEvents();

        //        drawPointsInMap(datatable,"安全防护单位");   //在地图上画点
        //        WriteEditLog("重点单位查询", "安全防护单位", ZDsql, "查询"); 
        //        this.toolPro.Value = 3;
        //        Application.DoEvents();
        //        isShowPro(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        isShowPro(false);
        //        writeZongheLog(ex,"zhongDianQuery");
        //    }
        //    this.Cursor = Cursors.Default;
        //}

        //private void checkBoxZhongdian_CheckedChanged(object sender, EventArgs e)
        //{
        //    cmbDanWei.Items.Clear();
        //    cmbDanWei.Items.Add("编号");       cmbDanWei.Items.Add("单位名称");
        //    cmbDanWei.Items.Add("单位性质");   cmbDanWei.Items.Add("单位地址");
        //    cmbDanWei.Items.Add("所属派出所"); cmbDanWei.Items.Add("所属中队");
        //    cmbDanWei.Items.Add("所属警务室");
        //    cmbDanWei.SelectedIndex = 0;
        //    cmbDanWei.Enabled = checkBoxZhongdian.Checked;
        //    textWord.Enabled = checkBoxZhongdian.Checked;
        //    buttonSearch.Enabled = checkBoxZhongdian.Checked;
        //    pageCount = 0;    //页数＝总记录数/每页显示行数
        //    pageCurrent = 0;   //当前页号
        //}
        #endregion

        /// <summary>
        /// 当本功能执行时，点击地图工具栏上的清除按钮时，执行
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        public void clearTem() 
        {
            try
            {
                removeTemPoints();  //移除地图上的要素
                //pageSize = 0;     //每页显示行数
                nMax = 0;         //总记录数
                pageCount = 0;    //页数＝总记录数/每页显示行数
                pageCurrent = 0;   //当前页号
                nCurrent = 0;      //当前记录行
                if (this.tabControl1.SelectedTab == this.tabYintou)
                {
                    this.dataGridView1.Rows.Clear();   //清空列表
                    PageNow1.Text = "0";
                    PageCount1.Text = "/ {0}";
                    RecordCount1.Text = "0条";
                }
                else if (this.tabControl1.SelectedTab == this.tabDianji)
                {
                    this.setCheckBoxFasle();
                    if (this.queryTable != null)
                    {
                        this.queryTable.Close();
                        this.queryTable = null;
                    }
                    this.closeTables();
                }
                else if (this.tabControl1.SelectedTab == this.tabZhoubian)
                {
                    this.dataGridView2.DataSource = null;   //清空列表
                    PageNow2.Text = "0";
                    PageCount2.Text = "/ {0}";
                    RecordCount2.Text = "0条"; 
                    if (this.queryTable != null)
                    {
                        this.queryTable.Close();
                        this.queryTable = null;
                    }
                }
                else
                {
                    this.dataGridView5.DataSource = null;   //清空列表
                    PageNow4.Text = "0";
                    PageCount4.Text = "/ {0}";
                    RecordCount4.Text = "0条";
                }
            }
            catch (Exception ex)
            { 
                writeZongheLog(ex,"clearTem");
            }
        }

        /// <summary>
        /// 调整控件大小时发生
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void dataGridView_Resize(object sender, System.EventArgs e)
        {
            try
            {
                DataGridView dataGridView = (DataGridView)sender;
                if (dataGridView.Name == "dataGridView1")
                {
                    if (dataGridView.Rows.Count > 0)
                    {
                        setDataGridViewColumnWidth(dataGridView);
                    }
                    else
                    {
                        dataGridView.Columns[1].Width = dataGridView.Width - 105;
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"dataGridView_Resize");
            }
        }

        /// <summary>
        /// 如果记录总高度大于容器高度,会出现滚动条,名称列的宽度要自定
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dataGridView"></param>
        private void setDataGridViewColumnWidth(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.Rows[0].Height * dataGridView.Rows.Count + 40 > dataGridView.Height)
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 60;
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 45;
                }
            }
            catch (Exception ex) { writeZongheLog(ex, "setDataGridViewColumnWidth"); }
        }

        /// <summary>
        /// 文本框更改时发生
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void comboDis_TextUpdate(object sender, EventArgs e)
        {
            try
            {
                if (checkNumber(comboDis.Text) == false)
                {
                    comboDis.Text = comboDis.Text.Remove(comboDis.Text.Length - 1);
                }
            }
            catch (Exception ex) { writeZongheLog(ex, "comboDis_TextUpdate"); }
        }

        /// <summary>
        /// 判断输入的是不是数字
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="str">要验证的字符串</param>
        /// <returns>布尔值</returns>
        private bool checkNumber(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return true;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//判断输入的是不是数字
                {
                    MessageBox.Show("输入数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                writeZongheLog(ex,"checkNumber");
                return false;
            }
        }

        /// <summary>
        /// 文本框值改变事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textYintou_TextChanged(object sender, EventArgs e)
        {
            checkLetter(textYintou.Text);
        }

        /// <summary>
        /// 判断输入的是字母还是数字
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="str">要判断的字母</param>
        private void checkLetter(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^[A-Za-z0-9]+$"))//判断输入的是不是字母或数字
                {
                    MessageBox.Show("只能输入半角字母或数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch(Exception ex)
            {
                writeZongheLog(ex,"checkLetter");
            }
        }

        /// <summary>
        /// 音头文本框当回车时执行音头查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textYintou_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    yinTouQuery();
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textYintou_KeyPress");
            }
        }

        /// <summary>
        /// 周边文本框当回车时执行周边查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textKeyWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    zhouBianCenPointQuery();
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textKeyWord_KeyPress");
            }
        }

        /// <summary>
        /// 在地图上画点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="datatable">数据源</param>
        /// <param name="tableName">表名</param>
        private void drawPointsInMap(DataTable datatable,string tableName)
        {
            try
            {
                if (datatable == null || datatable.Rows.Count < 1) { return; }
                string sql = "";
                //dtTable = datatable;
                if (tabControl1.SelectedTab==tabYintou)
                {
                    if (comboBox1.Text == "全部")
                    {
                        //oracleSpatial的查询语句和普遍表的查询语句 union 时会出错,因此分开
                          //oracleSpatial表
                        OracelData linkData = new OracelData(strConn);
                        DataTable temDt1 = null, temDt2 = null;
                        for (int i = 1; i < this.comboBox1.Items.Count; i++)
                        {
                            string bianhao = "";
                            string idArr = "";
                            sql = "";
                            string tabName = this.comboBox1.Items[i].ToString().Trim();
                            for (int j = 0; j < datatable.Rows.Count; j++)
                            {
                                if (tabName == datatable.Rows[j]["表名"].ToString())
                                {
                                    idArr += "'" + datatable.Rows[j]["表_ID"].ToString() + "',";
                                }
                            }
                            if (idArr == "")
                            {
                                continue;
                            }
                            idArr = idArr.Remove(idArr.Length - 1);
                            //通过表名称获取编号字段
                            CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                            bianhao = CLC.ForSDGA.GetFromTable.ObjID;

                            if (tabName == "案件信息" || tabName == "人口系统" || tabName == "出租屋房屋系统")
                            {
                                sql = "select " + CLC.ForSDGA.GetFromTable.ObjName+ " as 名称," + tabName + ".geoloc.SDO_POINT.X as X, " + tabName + ".geoloc.SDO_POINT.Y as Y,to_char(" + bianhao + ") as  表_ID,'" + tabName + "' as  表名   FROM " + tabName + " " + tabName + " where  " + bianhao + " in (" + idArr + ")";
                            }
                            else
                            {
                                sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称, X,Y,to_char(" + bianhao + ") as  表_ID,'" + tabName + "' as  表名   FROM " + tabName + " where  " + bianhao + " in (" + idArr + ")";
                            }
                            temDt2 = linkData.SelectDataBase(sql);
                            if (temDt1 == null || temDt1.Rows.Count < 1)
                            {
                                if (temDt2 != null && temDt2.Rows.Count > 0)
                                {
                                    temDt1 = temDt2;
                                }
                            }
                            else {
                                if (temDt2 != null && temDt2.Rows.Count > 0)
                                {
                                    //for (int k = 0; k < temDt2.Rows.Count; k++)
                                    //{
                                    //    temDt1.ImportRow(temDt2.Rows[k]);
                                    //}
                                    temDt1.Merge(temDt2);
                                }
                            }
                        }
                        
                        datatable = temDt1;
                        if (datatable == null ||datatable.Rows.Count<1)
                        {
                            // MessageBox.Show("无查询记录","提示");
                            this.Cursor = Cursors.Default;
                            return;
                        }
                    }
                    else
                    {
                        string idArr = "";
                        for (int i = 0; i < datatable.Rows.Count; i++)
                        {
                            if (i == 0)
                            {
                                idArr = "'" + datatable.Rows[i]["表_ID"].ToString() + "'";
                            }
                            else
                            {
                                idArr += ",'" + datatable.Rows[i]["表_ID"].ToString() + "'";
                            }
                        }
                        //通过表名称获取图标
                        CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                        string bianhao = CLC.ForSDGA.GetFromTable.ObjID;
                        if (tableName == "案件信息" || tableName == "人口系统" || tableName == "出租屋房屋系统")
                        {
                            sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称, t.geoloc.SDO_POINT.X as X, t.geoloc.SDO_POINT.Y as Y,to_char(" + bianhao + ") as  表_ID,'" + tableName + "' as  表名   FROM " + tableName + " t where  " + bianhao + " in (" + idArr + ")";
                        }
                        else
                        {
                            sql = "select  " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称, X,Y," + bianhao + " as  表_ID,'" + tableName + "' as  表名   FROM " + tableName + " where  " + bianhao + " in (" + idArr + ")";
                        }
                        OracelData linkData = new OracelData(strConn);
                        datatable = linkData.SelectDataBase(sql);
                        if (datatable == null)
                        {
                            // MessageBox.Show("无查询记录","提示");
                            this.Cursor = Cursors.Default;
                            return;
                        }
                    }
                }
                else
                {
                    string idArr = "";
                    for (int i = 0; i < datatable.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            idArr = "'" + datatable.Rows[i]["表_ID"].ToString() + "'";
                        }
                        else
                        {
                            idArr += ",'" + datatable.Rows[i]["表_ID"].ToString() + "'";
                        }
                    }
                    //通过表名称获取图标
                    CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                    string bianhao = CLC.ForSDGA.GetFromTable.ObjID;
                    if (tableName == "案件信息" || tableName == "人口系统" || tableName == "出租屋房屋系统")
                    {
                        sql = "select t.geoloc.SDO_POINT.X as X, t.geoloc.SDO_POINT.Y as Y,to_char(" + bianhao + ") as  表_ID,'" + tableName + "' as  表名   FROM " + tableName + " t where  " + bianhao + " in (" + idArr + ")";
                    }
                    else if (tableName == "视频位置")
                    {
                        sql = "SELECT  X,Y," + bianhao + " as  表_ID,'视频位置VIEW' as  表名   FROM 视频位置VIEW where  " + bianhao + " in (" + idArr + ")";
                    }
                    else
                    {
                        sql = "SELECT  X,Y," + bianhao + " as  表_ID,'" + tableName + "' as  表名   FROM " + tableName + " where  " + bianhao + " in (" + idArr + ")";
                    }
                }

                addPoints(datatable);
                this.Cursor = Cursors.Default;
            }
            catch(Exception ex)
            {
                writeZongheLog(ex,"drawPointsInMap");
            }
        }

        /// <summary>
        /// 填充列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="datatable">数据源</param>
        /// <param name="dataGridView">dataGridView控件</param>
        private void fillDataGridView(DataTable datatable, DataGridView dataGridView)
        {
            try
            {
                if (pageCurrent >= 1 && pageCurrent <= pageCount)
                {
                    dataGridView.Rows.Clear();
                }
                if (datatable == null || datatable.Rows.Count < 1)
                {
                    this.Cursor = Cursors.Default;
                    return;
                }

                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    //datagridview的前三列显示，
                    //第4列是本条记录的MapID，第5列是该记录的原表名，用来查询时使用
                    if (dataGridView == dataGridView1)
                    {
                        dataGridView.Rows.Add(i + 1, datatable.Rows[i]["名称"], "更多...", datatable.Rows[i]["表_ID"], datatable.Rows[i]["表名"]);
                    }
                    else
                    {
                        dataGridView.Rows.Add(i + 1, datatable.Rows[i]["名称"], "更多...", datatable.Rows[i]["X"], datatable.Rows[i]["Y"], datatable.Rows[i]["表_ID"], datatable.Rows[i]["表名"]);
                    }
                    if (i % 2 == 1)
                    {
                        dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }

                //根据滚动条是否出现设置宽度
                setDataGridViewColumnWidth(dataGridView);
            }
            catch(Exception ex) {
                writeZongheLog(ex, "fillDataGridView");
            }
        }

        /// <summary>
        /// 音头分页功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //返回参数,如果false,说明到了第一页或最好一页,有操作不用进行
                if (isOn)
                {
                    isShowPro(true);
                    DataTable dt = LoadData(PageNow1,PageCount1, YTsql);
                    fillDataGridView(dt, dataGridView1);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(PageNow1, PageCount1, YTexcelSql);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    exportSql = YTexcelSql;

                    drawPointsInMap(dt, comboBox1.Text.Trim());
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch(Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "bindingNavigator1_ItemClicked");
            }
        }

        /// <summary>
        /// 周边分页功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void bindingNavigator2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //返回参数,如果false,说明到了第一页或最好一页,有操作不用进行
                if (isOn)
                {
                    isShowPro(true);
                    DataTable dt = LoadData(_startNo, _endNo, ZBsql, comboClass.Text.Trim(),false);
                    this.dataGridView2.DataSource = dt;
                    //fillDataGridView(dt, dataGridView2);
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, ZBexcelSql, comboClass.Text.Trim(), true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, ZBexcelSql, comboClass.Text.Trim(), true);

                    drawPointsInMap(dt, comboClass.Text.Trim());
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "bindingNavigator2_ItemClicked");
            }
        }

        /// <summary>
        /// 综合分页功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void bindingNavigator4_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //返回参数,如果false,说明到了第一页或最好一页,有操作不用进行
                if (isOn)
                {
                    isShowPro(true);
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    //DataTable dt = LoadData(PageNow4,PageCount4, GJsql);
                    DataTable dt = LoadData(_startNo, _endNo, GJsql, CLC.ForSDGA.GetFromTable.TableName,false);
                    this.dataGridView5.DataSource = dt;
                    for (int i = 0; i < dataGridView5.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridView5.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);

                    //通过名称获取表名，对象名
                    drawPointsInMap(dt, CLC.ForSDGA.GetFromTable.TableName);
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "bindingNavigator4_ItemClicked");
            }
        }

        int pageSize = 1000;     // 每页显示行数
        int nMax = 0;            // 总记录数
        int pageCount = 0;       // 页数＝总记录数/每页显示行数
        int pageCurrent = 0;     // 当前页号
        int nCurrent = 0;        // 当前记录行
        DataSet ds = new DataSet();

        /// <summary>
        /// 获取本次查询所有记录数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">相关SQL</param>
        private void getMaxCount(string sql)//得到最大的值t
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
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nMax = 0;
            }
        }

        /// <summary>
        /// 初始化页控件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tsLabel"></param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                pageSize = Convert.ToInt32(this.TextNum4.Text);      //设置页面行数                
                tsLabel.Text = nMax.ToString()+"条";//在导航栏上显示总记录数
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
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 此函数用实现信息点和通过车辆查询的分页功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="textNowPage"></param>
        /// <param name="lblPageCount"></param>
        /// <param name="bds"></param>
        /// <param name="bdn"></param>
        /// <param name="dgv"></param>
        public void LoadData2(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv,DataTable dtPage)
        {
            try
            {
                isShowPro(true);
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = dtPage.Clone();

                if (pageCurrent == pageCount)
                    nEndPos = nMax;
                else
                    nEndPos = pageSize * pageCurrent;
                nStartPos = nCurrent;

                textNowPage.Text = Convert.ToString(pageCurrent);
                lblPageCount.Text = "/" + pageCount.ToString();
                this.toolPro.Value = 1;
                Application.DoEvents();

                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(dtPage.Rows[i]);
                    nCurrent++;
                }
                dataZhonghe = new DataTable();           // 给此内存表赋值用于放大数据展示
                bds.DataSource = dataZhonghe = dtTemp;

                if (comboTable.Text.Trim() == "通过车辆查询")
                {
                    CreateKakouTrack(dtTemp);             // 地图上画点并连线
                }

                bdn.BindingSource = bds;
                this.toolPro.Value = 2;
                Application.DoEvents();
                dgv.DataSource = bds;
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dgv.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                this.toolPro.Value = 1;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "LoadData2");
            }
        }

        /// <summary>
        /// 查询数据并返回
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="textNowPage">分页控件上子控件（用于显示或输入当前页）</param>
        /// <param name="lblPageCount">分页控件上子控件（用于显示当前数据行数）</param>
        /// <param name="sql">查询SQL语句</param>
        /// <returns></returns>
        public DataTable LoadData(ToolStripTextBox textNowPage,ToolStripLabel lblPageCount, string sql)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                int nStartPos = 0;   //当前页面开始记录行
                nStartPos = nCurrent;
                //lblPageCount.Text ="第"+Convert.ToString(pageCurrent)+ "页共" + pageCount.ToString()+"页";
                textNowPage.Text = Convert.ToString(pageCurrent);
                lblPageCount.Text = "/" + pageCount.ToString();

                DataTable dtInfo;

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
                Adp.Fill(nStartPos, pageSize, dataTables);//这个地方不知道是从数据库中查到前100行返回，还是所有的数据据都查询到返回，再从中获取前100行。

                dtInfo = dataTables[0];
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                return dtInfo;
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeZongheLog(ex,"InitDataSet");
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 查询数据并返回
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="_startNo">开始行数</param>
        /// <param name="_endNo">结束行数</param>
        /// <param name="_whereSql">SQL条件</param>
        /// <param name="tableName">表名</param>
        /// <param name="isExcel">是生成导出SQL</param>
        /// <returns>查询结果</returns>
        public DataTable LoadData(int _startNo, int _endNo, string _whereSql,string tableName,bool isExcel)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                int nStartPos = 0;   //当前页面开始记录行
                nStartPos = nCurrent;
                if (tabControl1.SelectedTab == this.tabAdvance)  // 高级查询
                {
                    this.PageNow4.Text = Convert.ToString(pageCurrent);
                    this.PageCount4.Text = "/" + pageCount.ToString();
                }
                else if (tabControl1.SelectedTab == this.tabYintou)  // 音头查询
                {
                    this.PageNow1.Text = Convert.ToString(pageCurrent);
                    this.PageCount1.Text = "/" + pageCount.ToString();
                }
                else if (tabControl1.SelectedTab == this.tabZhoubian)　// 周边查询
                {
                    this.PageNow2.Text = Convert.ToString(pageCurrent);
                    this.PageCount2.Text = "/" + pageCount.ToString();
                }

                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                tableName = CLC.ForSDGA.GetFromTable.TableName;
                DataTable dtInfo;
                string sql ="";
                if (isExcel)
                {
                    sql = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from " + tableName + " a where rownum<=" + _endNo + " and " + _whereSql + ") t where rn1 >=" + _startNo;
                    exportSql = sql;
                    return null;
                }
                else
                {
                    if (tableName == "案件信息" || tableName == "人口系统" || tableName == "出租屋房屋系统")
                    {
                        if (tableName == "出租屋房屋系统")
                        {
                            sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称,房屋编号,屋主姓名,屋主联系电话,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,全地址,"+
                                  "地址街路巷,地址门牌,楼层,房间号,房屋类型,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y," + CLC.ForSDGA.GetFromTable.ObjID + " as 表_ID,'" + 
                                  CLC.ForSDGA.GetFromTable.TableName + "' as 表名  from (select rownum as rn1,a.* from 出租屋房屋系统 a where rownum<=" + _endNo + " and " + _whereSql +
                                  ") t where rn1 >=" + _startNo;
                        }
                        else
                        {
                            sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称 ," + CLC.ForSDGA.GetFromTable.FrmFields + ",t.geoloc.SDO_POINT.X as X, t.geoloc.SDO_POINT.Y as Y," + 
                                   CLC.ForSDGA.GetFromTable.ObjID + " as 表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名 from (select rownum as rn1,a.* from " + 
                                   tableName + " a where rownum<=" + _endNo + " and " + _whereSql + ") t where rn1 >=" + _startNo;
                        }
                    }
                    else if (tableName == "安全防护单位")
                    {
                        sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称 ," + CLC.ForSDGA.GetFromTable.FrmFields + ",'点击查看' as 文件 ,X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as 表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名 from (select rownum as rn1,a.* from 安全防护单位 a where rownum<=" + _endNo + " and " + _whereSql + ") t where  rn1 >=" + _startNo;
                    }
                    else
                    {
                        sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as 名称 ," + CLC.ForSDGA.GetFromTable.FrmFields + ",X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as 表_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as 表名 from (select rownum as rn1,a.* from " + tableName + " a where rownum <= " + _endNo + " and " + _whereSql + ") t where rn1 >= " + _startNo;
                    }
                }

                dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(dtInfo);

                Cmd.Dispose();
                Conn.Close();

                // 用于放大查看数据的Table 
                if (tabControl1.SelectedTab == tabAdvance)
                {
                    dataZhonghe = new DataTable();
                    dataZhonghe = dtInfo;
                }
                if (tabControl1.SelectedTab == tabZhoubian)
                {
                    dataZhoubian = new DataTable();
                    dataZhoubian = dtInfo;
                }
                return dtInfo;
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeZongheLog(ex, "LoadData");
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 分页点击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <returns>布尔值</returns>
        private bool bdnInfo_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.TextNum4.Text);
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
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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
                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 异常日志输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeZongheLog(Exception ex,string sFunc) {
            CLC.BugRelated.ExceptionWrite(ex, "clZonghe-ucZonghe-" + sFunc);
        }


        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sModule">操作模块名称</param>
        /// <param name="tName">表名</param>
        /// <param name="sql">操作SQL语句</param>
        /// <param name="method">函数名</param>
        private void WriteEditLog(string sModule,string tName,string sql,string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'综合查询:" + sModule + "','" + tName + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex) { writeZongheLog(ex, "WriteEditLog"); }
        }

        /// <summary>
        /// 设计翻页转跳
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ltextNow">分页控件上子控件（用于显示或输入当前页）</param>
        private void PageNow_KeyPress(ToolStripTextBox ltextNow)
        {
            try
            {
                if (Convert.ToInt32(ltextNow.Text) < 1 || Convert.ToInt32(ltextNow.Text) > pageCount)
                {
                    MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                    ltextNow.Text = pageCurrent.ToString();
                    return;
                }
                else
                {
                    this.pageCurrent = Convert.ToInt32(ltextNow.Text);
                    nCurrent = pageSize * (pageCurrent - 1);
                }
            }
            catch (Exception ex) { writeZongheLog(ex, "PageNow_KeyPress"); }
        }

        /// <summary>
        /// 通过输入页数实现分页
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void PageNow1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    isShowPro(true);
                    PageNow_KeyPress(PageNow1);
                    DataTable dt = LoadData(PageNow1, PageCount1, YTsql);
                    this.dataGridView2.DataSource = dt;
                    //fillDataGridView(dt, dataGridView1);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(PageNow1, PageCount1, YTexcelSql);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    exportSql = YTexcelSql;

                    drawPointsInMap(dt, comboBox1.Text.Trim());
                    this.toolPro.Value = 3;
                    Application.DoEvents();

                    isShowPro(false);
                }
            }
            catch (Exception ex) { writeZongheLog(ex, "PageNow1_KeyPress"); }
        }

        /// <summary>
        /// 通过输入页数实现分页
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void PageNow2_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    isShowPro(true);
                    PageNow_KeyPress(PageNow2);
                    _startNo = ((pageCurrent - 1) * pageSize) + 1;
                    _endNo = _startNo + pageSize - 1;

                    DataTable dt = LoadData(_startNo, _endNo, ZBsql, comboClass.Text.Trim(), false);
                    this.dataGridView2.DataSource = dt;
                    //fillDataGridView(dt, dataGridView2);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, ZBexcelSql,comboClass.Text.Trim(),true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, ZBexcelSql, comboClass.Text.Trim(), true);

                    drawPointsInMap(dt, comboClass.Text.Trim());
                    this.toolPro.Value = 3;
                    Application.DoEvents();

                    isShowPro(false);
                }
            }
            catch (Exception ex) { writeZongheLog(ex, "PageNow2_KeyPress"); }
        }

        /// <summary>
        /// 通过输入页数实现分页
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void PageNow4_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    isShowPro(true);
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    PageNow_KeyPress(PageNow4);
                    _startNo = ((pageCurrent - 1) * pageSize) + 1;
                    _endNo = _startNo + pageSize - 1;

                    DataTable dt = LoadData(_startNo, _endNo, GJsql, CLC.ForSDGA.GetFromTable.TableName, false);
                    this.dataGridView5.DataSource = dt;
                    for (int i = 0; i < dataGridView5.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridView5.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);

                    //通过名称获取表名，对象名
                    drawPointsInMap(dt, CLC.ForSDGA.GetFromTable.TableName);
                    this.toolPro.Value = 3;
                    Application.DoEvents();

                    isShowPro(false);
                }
            }
            catch (Exception ex) { writeZongheLog(ex, "PageNow4_KeyPress"); }
        }

        /// <summary>
        /// 实现每页显示的数据数目
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ltextNum">分页控件上子控件（用于显示或输入当前页）</param>
        /// <param name="datagridview">数据列表</param>
        private void TextNum_KeyPress(ToolStripTextBox ltextNum,DataGridView datagridview)
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
                }
            }
            catch (Exception ex)
            {
                ltextNum.Text = pageSize.ToString();
                writeZongheLog(ex, "TextNum_KeyPress");
            }
        }

        /// <summary>
        /// 通过输入数字实现每页显示多少记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView1.Rows.Count > 0)
                {
                    isShowPro(true);
                    TextNum_KeyPress(TextNum1, dataGridView1);
                    DataTable dt = LoadData(PageNow1, PageCount1, YTsql);
                    fillDataGridView(dt, dataGridView1);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(PageNow1, PageCount1, YTexcelSql);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    exportSql = YTexcelSql;

                    drawPointsInMap(dt, comboBox1.Text.Trim());
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "TextNum1_KeyPress()");
            }
        }

        /// <summary>
        /// 通过输入数字实现每页显示多少记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void TextNum2_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView2.Rows.Count > 0)
                {
                    isShowPro(true);
                    TextNum_KeyPress(TextNum2, dataGridView2);
                    _endNo = pageSize;
                    _startNo = 1;

                    DataTable dt = LoadData(_startNo,_endNo, ZBsql,comboClass.Text.Trim(),false);
                    this.dataGridView2.DataSource = dt;
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    //fillDataGridView(dt, dataGridView2);
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, ZBexcelSql,comboClass.Text.Trim(), true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel; 
                    //this.toolPro.Value =2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, ZBexcelSql, comboClass.Text.Trim(), true);

                    drawPointsInMap(dt, comboClass.Text.Trim());
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "TextNum2_KeyPress()"); 
            }
        }

        /// <summary>
        /// 通过输入数字实现每页显示多少记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void TextNum4_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView5.Rows.Count > 0)
                {
                    isShowPro(true);
                    //通过名称获取表名，对象名
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    _endNo = Convert.ToInt32(this.TextNum4.Text);
                    TextNum_KeyPress(TextNum4, dataGridView5);
                    _endNo = pageSize;
                    _startNo = 1;

                    DataTable dt = LoadData(_startNo, _endNo, GJsql, CLC.ForSDGA.GetFromTable.TableName, false);
                    this.dataGridView5.DataSource = dt;
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    for (int i = 0; i < dataGridView5.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridView5.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    //fillDataGridView(dt, dataGridView5);

                    //Excel先屏弊，因占用资源过大，待更好的解决方案实现导出
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName,true);

                    drawPointsInMap(dt, CLC.ForSDGA.GetFromTable.TableName);
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "TextNum4_KeyPress()");
            }
        }

        /// <summary>
        /// 数据统计工具类别框改变事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                switch (cmbType.Text)
                {
                    case "按派出所":
                        cmbPic.Enabled = true;
                        cmbJinWuShi.Enabled = false;
                        cmbZhongdu.Enabled = false;
                        break;
                    case "按中队":
                        cmbZhongdu.Enabled = true;
                        cmbJinWuShi.Enabled = false;
                        cmbPic.Enabled = true;
                        break;
                    case "按警务室":
                        cmbJinWuShi.Enabled = true;
                        cmbZhongdu.Enabled = true;
                        cmbPic.Enabled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "cmbType_SelectedIndexChanged()");
            }
        }

        /// <summary>
        /// 数据统计工具派出所框改变事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void cmbPic_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cmbZhongdu.Items.Clear();
                string sql = "select 中队代码,中队名 from 基层民警中队";
                if (cmbPic.Text != "所有派出所")
                {
                    sql += " where 所属派出所='" + cmbPic.Text + "'";
                }

                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable tab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                cmbZhongdu.Items.Add("所有中队");
                foreach (DataRow row in tab.Rows)
                {
                    cmbZhongdu.Items.Add(row[1].ToString());
                }
                cmbZhongdu.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "cmbPic_SelectedIndexChanged");
            }
        }

        /// <summary>
        /// 数据统计工具重置按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                cmbTabName.SelectedIndex = 0;
                cmbType.SelectedIndex = 0;
                dtpStartTime.Text = System.DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                dtpEndTime.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
                dtpStartTime.Enabled = true;
                dtpEndTime.Enabled = true;
            }
            catch (Exception ex){ writeZongheLog(ex, "btnClose_Click");}
        }

        public string statType = "", begin = "", end = "", wName = "", tableName = "", statTypeSelect = "";
        private string pzjSQL = "";
        public DataTable dtble = null;

        /// <summary>
        /// 数据统计工具查询按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                tableName = cmbTabName.SelectedItem.ToString();
                statTypeSelect = cmbType.SelectedItem.ToString();
            }
            catch (Exception ex) { writeZongheLog(ex, "btnSearch_Click"); }

            //判断必要条件是否选中
            if (tableName == "-请选择表名-" || statTypeSelect == "-请选择类别-" || cmbTabName.Text == "" || cmbType.Text == "")
            {
                MessageBox.Show("请选择统计的必要条件（表名和类别必选）", "警告");
                return;
            }
            isShowPro(true);
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (statTypeSelect == "按派出所")
                {
                    statType = "所属派出所代码";
                    pzjSQL = "select 派出所名,派出所代码 from 基层派出所";
                    wName = cmbPic.Text.ToString();
                }
                if (statTypeSelect == "按中队")
                {
                    statType = "所属中队代码";
                    pzjSQL = getPZJ(this.cmbPic, statType);
                    wName = cmbZhongdu.Text.ToString();
                }
                if (statTypeSelect == "按警务室")
                {
                    statType = "所属警务室代码";
                    pzjSQL = getPZJ(this.cmbZhongdu, statType);
                    wName = cmbJinWuShi.Text.ToString();
                }

                string pcs = cmbPic.Text.ToString() != "所有派出所" ? cmbPic.Text.ToString() : "";
                string zd = cmbZhongdu.Text.ToString() != "所有中队" ? cmbZhongdu.Text.ToString() : "";
                string jws = cmbJinWuShi.Text.ToString() != "所有警务室" ? cmbJinWuShi.Text.ToString() : "";

                begin = dtpStartTime.Text.Replace("年", "-").Replace("月", "-").Substring(0, dtpStartTime.Text.Length - 1);
                end = dtpEndTime.Text.Replace("年", "-").Replace("月", "-").Substring(0, dtpEndTime.Text.Length - 1);

                dtble = new DataTable();
                // 创建列
                DataColumn col = new DataColumn("统计时间段", System.Type.GetType("System.String"));
                DataColumn col1 = new DataColumn("所属表", System.Type.GetType("System.String"));
                DataColumn col2 = new DataColumn("该时间段更新需要标注数据", System.Type.GetType("System.String"));
                DataColumn col3 = new DataColumn("已标注的数据", System.Type.GetType("System.String"));
                DataColumn col4 = new DataColumn("未标注的数据", System.Type.GetType("System.String"));
                DataColumn col5 = new DataColumn("标注率", System.Type.GetType("System.String"));
                DataColumn col6 = new DataColumn("标注准确率", System.Type.GetType("System.String"));
                DataColumn col7 = new DataColumn("所属单位", System.Type.GetType("System.String"));
                DataColumn col8 = new DataColumn("备注", System.Type.GetType("System.String"));
                // 往表中添加列
                dtble.Columns.Add(col);  dtble.Columns.Add(col1); dtble.Columns.Add(col2);
                dtble.Columns.Add(col3); dtble.Columns.Add(col4); dtble.Columns.Add(col5);
                dtble.Columns.Add(col6); dtble.Columns.Add(col7); dtble.Columns.Add(col8);

                this.toolPro.Value = 1;
                Application.DoEvents();

                // 创建并添加行
                DataRow row = null;
                int couLen = 0;        // 该时间段更新需要标注数
                int aleLen = 0;        // 已标注数
                int notLen = 0;        // 未标注的数据
                string pzjNo = "";
                if (wName == "所有派出所" || wName == "所有中队" || wName == "所有警务室")
                {
                    if (tableName == "所有表")
                    {
                        dtble = GetAllTable(dtpStartTime.Text, dtpEndTime.Text, statType, begin, end, wName, "所有用户");

                    }
                    else
                    {
                        getSQL(tableName, statType, begin, end, wName, "所有用户",out pzjNo);

                        DataTable pzjTab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(pzjSQL);
                        for (int j = 0; j < pzjTab.Rows.Count; j++)
                        {
                            couLen = PossTab.Select(statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;  // 该时间段更新需要标注数
                            aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0 and " + statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;   // 已标注数
                            notLen = couLen - aleLen;     // 未标注的数据

                            row = dtble.NewRow();
                            row[0] = dtpStartTime.Text + " 至 " + dtpEndTime.Text;     // 统计时间段
                            row[1] = tableName;       // 所属表
                            row[2] = couLen;          // 该时间段更新需要标注数
                            row[3] = aleLen;          // 已标注数
                            row[4] = notLen;          // 未标注的数据
                            row[5] = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "非数字" ? "0%" : Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) + "%"; // 标注率
                            row[6] = "";       // 标注准确率
                            row[7] = pzjTab.Rows[j][0].ToString();    // 所属单位
                            row[8] = "";       // 备注

                            if (couLen > 0)
                                dtble.Rows.Add(row);
                        }
                    }

                    this.toolPro.Value = 2;
                    Application.DoEvents();
                }
                else
                {
                    if (tableName == "所有表")
                    {
                        dtble = GetAllTable(dtpStartTime.Text, dtpEndTime.Text, statType, begin, end, wName, "某一个用户");
                    }
                    else
                    {
                        getSQL(tableName, statType, begin, end, wName, "某一个用户",out pzjNo);

                        couLen = PossTab.Rows.Count;  // 该时间段更新需要标注数
                        aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0").Length;    // 已标注数
                        notLen = couLen - aleLen;     // 未标注的数据

                        row = dtble.NewRow();
                        row[0] = dtpStartTime.Text + " 至 " + dtpEndTime.Text;   // 统计时间段
                        row[1] = tableName;
                        row[2] = couLen;             // 该时间段更新需要标注数
                        row[3] = aleLen;             // 已标注数
                        row[4] = notLen;
                        row[5] = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "非数字" ? "0%" : Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) + "%";
                        row[6] = "";
                        row[7] = wName;
                        row[8] = ""; 
                        if (couLen > 0)
                            dtble.Rows.Add(row);
                    }

                    this.toolPro.Value = 2;
                    Application.DoEvents();
                }
                if (dtble.Rows.Count > 0)
                {
                    frmTongji tong = new frmTongji();
                    tong._exportDT = dtble;
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    tong.Show();
                    this.Cursor = Cursors.Default;
                    isShowPro(false);
                }
                else
                {
                    isShowPro(false);
                    MessageBox.Show("没有要统计的数据！请检查", "提示");
                    this.Cursor = Cursors.Default;
                }
            }
            catch(Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "btnSearch_Click");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 根据上级选择获得下级的sql
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="box">上级控件</param>
        /// <param name="strBox">级别</param>
        /// <returns>sql</returns>
        private string getPZJ(System.Windows.Forms.ComboBox box,string strBox)
        {
            try
            {
                string pzjSql = "";                               // 获得下级的sql
                string comSelect = box.SelectedItem.ToString();   // 上级选择的值

                switch (strBox)
                {
                    case "所属中队代码":
                        if (comSelect.IndexOf("所有") > -1)       // 如果是所有的派出所则查所有中队否则按派出所查中队
                        {
                            pzjSql = "select 中队名,中队代码 from 基层民警中队";
                        }
                        else
                        {
                            pzjSql = "select 中队名,中队代码 from 基层民警中队 where 所属派出所='" + comSelect + "'";
                        }
                        break;
                    case "所属警务室代码":
                        if (comSelect.IndexOf("所有") > -1)       // 如果是所有的中队则查所有警务室否则按中队查警务室
                        {
                            pzjSql = "select 警务室名,警务室代码 from 社区警务室";
                        }
                        else
                        {
                            pzjSql = "select 警务室名,警务室代码 from 社区警务室 where 所属中队='" + comSelect + "'";
                        }
                        break;
                    default:
                        break;
                }
                return pzjSql;
            }
            catch (Exception ex)
            { writeZongheLog(ex, "getPZJ"); return null; }
        }

        private DataTable PossTab = new DataTable();
        /// <summary>
        /// 拼接SQL语句
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="statType">所属单位</param>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="wName">所属单位值</param>
        /// <param name="alloneName">用户类型（所有用户/某一个用户）</param>
        /// <param name="pzjNo">所属区域编号</param>
        private void getSQL(string tableName, string statType, string begin, string end, string wName, string alloneName, out string pzjNo)
        {
            string  wTime = "";

            #region 拼接SQL语句
            try
            {
                string cretabSql = "";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                wTime = "抽取更新时间 between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')";
                if (alloneName == "所有用户")
                {
                    switch (tableName)
                    {
                        case "人口系统":
                        case "出租屋房屋系统":
                        case "案件信息":
                            cretabSql = "select t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码 from " + tableName + " t where " + wTime;
                            break;
                        default:
                            cretabSql = "select X,Y,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码 from " + tableName + " where " + wTime;
                            break;
                    }
                    pzjNo = "";
                }
                else
                {
                    string sqlStr1 = "";
                    switch (statType)
                    {
                        case "所属派出所代码":
                            sqlStr1 = "select 派出所代码 from 基层派出所 where 派出所名='" + wName + "'";
                            break;
                        case "所属中队代码":
                            sqlStr1 = "select 中队代码 from 基层民警中队 where 中队名='" + wName + "'";
                            break;
                        case "所属警务室代码":
                            sqlStr1 = "select 警务室代码 from 社区警务室 where 警务室名='" + wName + "'";
                            break;
                    }
                    DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr1);
                    string wNameNo = pzjNo = dt.Rows[0][0].ToString();
                    switch (tableName)
                    {
                        case "人口系统":
                        case "出租屋房屋系统":
                        case "案件信息":
                            cretabSql = "select t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码 from " + tableName + " t where " + wTime + "and " + statType + "='" + wNameNo + "'";
                            break;
                        default:
                            cretabSql = "select X,Y,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码 from " + tableName + " where " + wTime + "and " + statType + "='" + wNameNo + "'";
                            break;
                    }
                }

                PossTab.Clear();
                PossTab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(cretabSql);
            }
            catch (Exception ex)
            {
                pzjNo = "";
                writeZongheLog(ex, "getSQL");
            }
           #endregion
        }

        /// <summary>
        /// 统计所有表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dtpStartTime">结果中显示的开始时间</param>
        /// <param name="dtpEndTime">结果中显示的结束时间</param>
        /// <param name="statType">所属单位</param>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="wName">所属单位值</param>
        /// <param name="allOneName">用户类型（所有用户/某一个用户）</param>
        /// <returns>统计结果</returns>
        private DataTable GetAllTable(string dtpStartTime,string dtpEndTime,string statType,string begin,string end ,string wName,string allOneName)
        {
            // 创建表
            DataTable dTable = new DataTable();
            CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
            try
            {
                // 创建列
                DataColumn col = new DataColumn("统计时间段", System.Type.GetType("System.String"));
                DataColumn col1 = new DataColumn("所属表", System.Type.GetType("System.String"));
                DataColumn col2 = new DataColumn("该时间段更新需要标注数据", System.Type.GetType("System.String"));
                DataColumn col3 = new DataColumn("已标注的数据", System.Type.GetType("System.String"));
                DataColumn col4 = new DataColumn("未标注的数据", System.Type.GetType("System.String"));
                DataColumn col5 = new DataColumn("标注率", System.Type.GetType("System.String"));
                DataColumn col6 = new DataColumn("标注准确率", System.Type.GetType("System.String"));
                DataColumn col7 = new DataColumn("所属单位", System.Type.GetType("System.String"));
                DataColumn col8 = new DataColumn("备注", System.Type.GetType("System.String"));

                // 往表中添加列
                dTable.Columns.Add(col);  dTable.Columns.Add(col1); dTable.Columns.Add(col2);
                dTable.Columns.Add(col3); dTable.Columns.Add(col4); dTable.Columns.Add(col5);
                dTable.Columns.Add(col6); dTable.Columns.Add(col7); dTable.Columns.Add(col8);

                // 获取所有表名
                string[] tbName = new string[cmbTabName.Items.Count - 2];
                for (int i = 2; i < cmbTabName.Items.Count; i++)
                {
                    tbName[i - 2] = cmbTabName.Items[i].ToString();
                }

                // 创建并添加行
                DataRow row = null;
                isShowPro(true);
                string timLen = "";    // 统计时间段
                string tabLen = "";    // 所属表
                int couLen = 0;        // 该时间段更新需要标注数
                int aleLen = 0;        // 已标注数
                int notLen = 0;        // 未标注的数据
                string ratLen = "";    // 标注率
                string uniLen = "";    // 所属单位
                string pzjNo="";
                for (int i = 0; i < tbName.Length; i++)
                {
                    getSQL(tbName[i], statType, begin, end, wName, allOneName, out pzjNo);

                    if (wName == "所有派出所" || wName == "所有中队" || wName == "所有警务室")
                    {
                        DataTable pzjTab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(pzjSQL);
                        for (int j = 0; j < pzjTab.Rows.Count; j++)
                        {
                            timLen = dtpStartTime + " 至 " + dtpEndTime;
                            tabLen = tbName[i];
                            couLen = PossTab.Select(statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;
                            aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0 and " + statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;
                            notLen = couLen - aleLen;
                            ratLen = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "非数字" 
                                     ? "0%" : Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) + "%";
                            uniLen = pzjTab.Rows[j][0].ToString();

                            row = dTable.NewRow();
                            row[0] = timLen;
                            row[1] = tabLen;
                            row[2] = couLen;
                            row[3] = aleLen;
                            row[4] = notLen;
                            row[5] = ratLen;
                            row[6] = "";
                            row[7] = uniLen;
                            row[8] = "";

                            if (couLen > 0)
                                dTable.Rows.Add(row);
                        }
                    }
                    else
                    {
                        timLen = dtpStartTime + " 至 " + dtpEndTime;
                        tabLen = tbName[i];
                        couLen = PossTab.Rows.Count;
                        aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0").Length;
                        notLen = couLen - aleLen;
                        ratLen = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "非数字"
                                 ? "0%" : Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) + "%";
                        uniLen = wName;

                        row = dTable.NewRow();
                        row[0] = timLen;
                        row[1] = tabLen;
                        row[2] = couLen;
                        row[3] = aleLen;
                        row[4] = notLen;
                        row[5] = ratLen;
                        row[6] = "";
                        row[7] = uniLen;
                        row[8] = ""; 

                        if (couLen > 0)
                            dTable.Rows.Add(row);
                    }
                }
                return dTable;
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "GetAllTable");
                return null;
            }
        }

        /// <summary>
        /// 用户登录统计重置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                rdoDan.Checked = true;
                cmbDan.Enabled = true;
                cmbDan.SelectedIndex = 0;
                cmbName.Enabled = false;
                cmbName.SelectedIndex = 0;
            }
            catch (Exception ex) { writeZongheLog(ex, "button2_Click"); }
        }

        /// <summary>
        /// 用户登录统计的查询按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbDan.Enabled == true)
                {
                    if (cmbDan.Text == "" || cmbDan.Text == "--请选择单位--")
                    {
                        MessageBox.Show("请选择单位后统计!", "提示");
                        return;
                    }

                }
                if (cmbName.Enabled == true)
                {
                    if (cmbName.Text == "" || cmbName.Text == "--请选择用户--")
                    {
                        MessageBox.Show("请选择用户后统计!", "提示");
                        return;
                    }
                }

                isShowPro(true);
                ////////---获得用户 lili 2010-6-8---///////
                string nameStr = "";
                if (cmbName.Text.Trim() == "所有用户")
                    nameStr = "所有用户";
                else
                {
                    if (cmbName.Text.Trim().IndexOf('-') < 0)
                        nameStr = cmbName.Text.Trim();
                    else
                        nameStr = cmbName.Text.Trim().Substring(0, cmbName.Text.Trim().IndexOf('-'));
                }
                //////////////////////////////////////////

                this.Cursor = Cursors.WaitCursor;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable dt = null;
                try
                {
                    string sqlStr = "", end = "", begin = "";
                    //开始时间
                    begin = dtpStart.Text.Replace("年", "-").Replace("月", "-").Substring(0, dtpStart.Text.Length - 1);
                    //结束时间
                    end = dtpEnd.Text.Replace("年", "-").Replace("月", "-").Substring(0, dtpEnd.Text.Length - 1);

                    if (cmbDan.Enabled == true)
                    {
                        if (cmbDan.Text.Trim() == "所有单位")
                        {
                            dt = getAllDan(dtpStart.Text, dtpEnd.Text, begin, end, "所有");
                        }
                        else
                        {
                            dt = getAllDan(dtpStart.Text, dtpEnd.Text, begin, end, "某一个");
                        }

                        this.toolPro.Value = 2;
                        Application.DoEvents();
                    }

                    if (cmbName.Enabled == true)
                    {
                        if (nameStr == "所有用户")
                        {
                            sqlStr = GetDenLuSQL(dtpStart.Text, dtpEnd.Text, nameStr, begin, end, "所有");
                            dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        }
                        else
                        {
                            sqlStr = GetDenLuSQL(dtpStart.Text, dtpEnd.Text, nameStr, begin, end, "某一个");
                            dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        }

                        this.toolPro.Value = 2;
                        Application.DoEvents();
                    }

                    if (dt.Rows.Count <= 0)
                    {
                        isShowPro(false);
                        MessageBox.Show("该条件无统计结果!", "提示");
                        this.Cursor = Cursors.Default;
                        return;
                    }

                    frmTongji tongji = new frmTongji();
                    tongji._exportDT = dt;
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    tongji.Show();
                    this.Cursor = Cursors.Default;
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeZongheLog(ex, "button1_Click");
                    this.Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "button1_Click");
            }
        }

        /// <summary>
        /// 生成登录统计的SQL语句
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dtpStart">用户显示的开始时间</param>
        /// <param name="dtpEnd">用户显示的结束时间</param>
        /// <param name="username">用户名</param>
        /// <param name="begin">用户条件的开始时</param>
        /// <param name="end">用户条件的结束时间</param>
        /// <param name="oneAlluser">用户类型（所有用户/某一个用户）</param>
        /// <returns>统计登录的SQL字符串</returns>
        private string GetDenLuSQL(string dtpStart, string dtpEnd, string username, string begin, string end,string oneAlluser)
        {
            try
            {
                string sqlSQL = "", wTime = "";
                wTime = "时间 between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss') and 功能模块='登录系统'";
                int count = CLC.DatabaseRelated.OracleDriver.OracleComScalar("select count(*) from 操作记录 where " + wTime);
                switch (oneAlluser)
                {
                    case "所有":       // 统计所有用户登录次数的SQL
                        sqlSQL = "select '" + dtpStart + " 至 " + dtpEnd + "' as 统计时间段, 用户名, count(*) as 登录次数,case substr(to_char(Round(count(*)/" + count.ToString() + "*100,2)),0,1) when '.' then '0'||to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' else to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' end as 使用率,'' as 备注 from 操作记录 where " + wTime + " group by 用户名";
                        break;
                    case "某一个":　　// 统计单个用户登录次数的SQL
                        sqlSQL = "select '" + dtpStart + " 至 " + dtpEnd + "' as 统计时间段, 用户名, count(*) as 登录次数,case substr(to_char(Round(count(*)/" + count.ToString() + "*100,2)),0,1) when '.' then '0'||to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' else to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' end as 使用率,'' as 备注 from 操作记录 where 用户名='" + username + "' and " + wTime + " group by 用户名";
                        break;
                }
                return sqlSQL;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "GetDenLuSQL");
                return null;
            }
        }

        /// <summary>
        /// 生成统计结果的内存表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dtpStart">用户显示的开始时间</param>
        /// <param name="dtpEnd">用户显示的结束时间</param>
        /// <param name="begin">用户条件的开始时</param>
        /// <param name="end">用户条件的结束时间</param>
        /// <param name="oneAllUser">用户类型（所有用户/某一个用户）</param>
        /// <returns>统计结果DataTable</returns>
        private DataTable getAllDan(string dtpStart,string dtpEnd,string begin,string end,string oneAllUser)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
            DataTable dtable = new DataTable();

            // 添加列
            DataColumn col = new DataColumn("统计时间段", System.Type.GetType("System.String"));
            DataColumn col1 = new DataColumn("用户名", System.Type.GetType("System.String"));
            DataColumn col2 = new DataColumn("登陆次数", System.Type.GetType("System.String"));
            DataColumn col3 = new DataColumn("使用率", System.Type.GetType("System.String"));
            DataColumn col4 = new DataColumn("备注", System.Type.GetType("System.String"));
            dtable.Columns.Add(col);  dtable.Columns.Add(col1);
            dtable.Columns.Add(col2); dtable.Columns.Add(col3);
            dtable.Columns.Add(col4);

            int shu = 0;  // 存放某个单位的总次数
            int count = CLC.DatabaseRelated.OracleDriver.OracleComScalar("select count(*) from 操作记录 where 功能模块='登录系统'and 时间 between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')");   // 存放所有登录次数
            try
            {
                string[] danWei = new string[cmbDan.Items.Count - 2];
                for (int i = 2; i < cmbDan.Items.Count; i++)
                {
                    danWei[i - 2] = cmbDan.Items[i].ToString();
                }

                DataRow row = null;
                if (oneAllUser == "所有")
                {
                    for (int i = 0; i < danWei.Length; i++)
                    {
                        DataTable dt = new DataTable();
                        shu = 0;
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select USERNAME from 用户 where 用户单位='" + danWei[i] + "'");
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            string sqlSQL = GetDenLuSQL(dtpStart, dtpEnd, dt.Rows[j][0].ToString(), begin, end, "某一个");
                            DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlSQL);
                            for (int k = 0; k < table.Rows.Count; k++)
                            {
                                shu += Convert.ToInt32(table.Rows[k][2].ToString());
                            }
                        }
                        if (dt.Rows.Count > 0)
                        {
                            row = dtable.NewRow();
                            row[0] = dtpStart + " 至 " + dtpEnd;
                            row[1] = danWei[i];
                            row[2] = shu;
                            row[3] = Convert.ToString(Math.Round(shu / Convert.ToDouble(count) * 100, 2, MidpointRounding.AwayFromZero)) == "非数字" ? "0%" : Convert.ToString(Math.Round(shu / Convert.ToDouble(count) * 100, 2, MidpointRounding.AwayFromZero)) + "%";
                            row[4] = "";
                            dtable.Rows.Add(row);
                        }
                    }
                    return dtable;
                }
                else
                {
                    DataTable dt = new DataTable();
                    dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select USERNAME from 用户 where 用户单位='" + cmbDan.Text.Trim() + "'");
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        string sqlSQL = GetDenLuSQL(dtpStart, dtpEnd, dt.Rows[j][0].ToString(), begin, end, oneAllUser);
                        DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlSQL);
                        for (int k = 0; k < table.Rows.Count; k++)
                        {
                            row = dtable.NewRow();
                            row[0] = table.Rows[k][0].ToString();
                            row[1] = table.Rows[k][1].ToString();
                            row[2] = table.Rows[k][2].ToString();
                            row[3] = table.Rows[k][3].ToString();
                            row[4] = table.Rows[k][4].ToString();
                            dtable.Rows.Add(row);
                        }
                    }
                    return dtable;
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "ucZonghe-getAllDan");
                return null;
            }
        }

        /// <summary>
        /// 标注人统计重置按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnChong_Click(object sender, EventArgs e)
        {
            try
            {
                cmbTable.SelectedIndex = 0;
            }
            catch (Exception ex) { writeZongheLog(ex, "btnChong_Click"); }
        }

        /// <summary>
        /// 标注人统计统计按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cmbTable.Text == "--请选择表名--" || cmbTable.Text == "")
            {
                MessageBox.Show("请选择您要统计的表！","提示");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            try
            {
                isShowPro(true);
                DataTable dt = new DataTable();
                
                string sqlStr = "", end = "", begin = "", wTime = "";

                //开始时间
                begin =  dtpKai.Text.Replace("年", "-").Replace("月", "-").Substring(0, dtpKai.Text.Length - 1);
                //结束时间
                end = dtpjie.Text.Replace("年", "-").Replace("月", "-").Substring(0, dtpjie.Text.Length - 1);
                wTime = " 标注时间 between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')";

                if (string.IsNullOrEmpty(textName.Text))
                {
                    sqlStr = "select t.标注人 as 标注人,count(*) as 标注数量,t.所属中队 as 所在单位 from " + cmbTable.Text + " t where 标注人 is not null and " + wTime + "  Group by t.标注人,t.所属中队";
                }
                else
                {
                    sqlStr = "select t.标注人 as 标注人,count(*) as 标注数量,t.所属中队 as 所在单位 from " + cmbTable.Text + " t where " + wTime + " and 标注人='" + textName.Text + "' Group by t.标注人,t.所属中队";
                }

                this.toolPro.Value = 1;
                Application.DoEvents();
                dt = BiaoRens(sqlStr, begin, end);

                if (dt.Rows.Count <= 0)
                {
                    isShowPro(false);
                    MessageBox.Show("该条件无统计结果!", "提示");
                    this.Cursor = Cursors.Default;
                    return;
                }

                frmTongji tongji = new frmTongji();
                tongji._exportDT = dt;
                this.toolPro.Value = 3;
                Application.DoEvents();

                tongji.Show();
                this.Cursor = Cursors.Default;
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "btnOK_Click");
                this.Cursor = Cursors.Default;
            }
        }

        private DataTable dateTable = null;
        /// <summary>
        /// 标注人统计查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">统计的SQL</param>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>结果集</returns>
        private DataTable BiaoRens(string sql,string begin,string end)
        {
            try
            {
                dateTable = new DataTable();
                DataColumn col = new DataColumn("统计时间段",System.Type.GetType("System.String"));
                DataColumn col1 = new DataColumn("标注人", System.Type.GetType("System.String"));
                DataColumn col2 = new DataColumn("标注数量", System.Type.GetType("System.String"));
                DataColumn col3 = new DataColumn("所在单位", System.Type.GetType("System.String"));
                DataColumn col4 = new DataColumn("备注", System.Type.GetType("System.String"));
                dateTable.Columns.Add(col); dateTable.Columns.Add(col1);
                dateTable.Columns.Add(col2); dateTable.Columns.Add(col3);
                dateTable.Columns.Add(col4); 

                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                this.toolPro.Value = 2;
                Application.DoEvents();

                string  whereTime = " 标注时间 between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')";

                string sql2 = "";
                if (string.IsNullOrEmpty(textName.Text))
                { 
                    sql2 = "select distinct t.标注人 from " + cmbTable.Text + " t where 标注人 is not null and " + whereTime + "  Group by t.标注人,t.所属中队";
                }
                else
                {
                    sql2 = "select distinct t.标注人 from " + cmbTable.Text + " t where 标注人 ='" + textName.Text + "' and " + whereTime + "  Group by t.标注人,t.所属中队";
                }
                DataTable dt2 = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql2);
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    dateTable.Rows.Add(getMessage(dt.Select("标注人='" + dt2.Rows[i][0].ToString() + "'"), begin, end));
                }
                return dateTable;
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeZongheLog(ex, "BiaoRens");
                return null;
            }
        }

        /// <summary>
        /// 用于在初次统计结里中统计新结果
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="rows">要统计的行</param>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>新结果行</returns>
        private DataRow getMessage(DataRow[] rows, string begin, string end)
        {
            try
            {
                string zdName = "";    // 所在单位
                int markCount = 0;     // 标注数量
                string sName = "";     // 标注人
                foreach (DataRow row in rows)
                {
                    markCount += Convert.ToInt32(row["标注数量"].ToString());
                    if (row["所在单位"].ToString().Trim() != "")
                    {
                        zdName = row["所在单位"].ToString();
                    }
                    sName = row["标注人"].ToString();
                }
                DataRow dr = dateTable.NewRow();
                dr[0] = begin + " 至 " + end;
                dr[1] = sName;
                dr[2] = markCount.ToString();
                dr[3] = zdName;
                dr[4] = "";

                return dr;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "getMessage");
                return null;
            }
        }

        /// <summary>
        /// 按单位统计
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void rdoDan_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                cmbDan.Enabled = true;
                cmbDan.SelectedIndex = 0;
                cmbName.Enabled = false;
                cmbName.SelectedIndex = 0;
            }
            catch { }
        }

        /// <summary>
        /// 按用户统计
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void rdoUser_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                cmbName.Enabled = true;
                cmbName.SelectedIndex = 0;
                cmbDan.Enabled = false;
                cmbDan.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "getMessage");
            }
        }

        /// <summary>
        /// 各单位标注情况统计
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void rdoGdw_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                panGdw.Visible = true;
                panUsd.Visible = false;
                panBren.Visible = false;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "getMessage");
            }
        }

        /// <summary>
        /// 标注人统计
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void rdoBren_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                panGdw.Visible = false;
                panUsd.Visible = false;
                panBren.Visible = true;
                cmbTable.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "getMessage");
            }
        }

        /// <summary>
        /// 用户登录统计
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void rdoUsd_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                panGdw.Visible = false;
                panUsd.Visible = true;
                panBren.Visible = false;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "getMessage");
            }
        }

        /* 以下是自动补全代码 */

        /// <summary>
        /// 自动补全方法
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                writeZongheLog(ex, "getListBox");
                return null;
            }
        }

        /// <summary>
        /// 列为固定值时自动添加
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="colName">列名</param>
        /// <param name="tableName">表名</param>
        /// <param name="listBox">显示自动补全值的控件</param>
        /// <returns>匹配结果</returns>
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
                writeZongheLog(ex, "MatchShu");
                return null;
            }
        }

        /// <summary>
        /// 周边文本框匹配
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textKeyValue_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.textKeyWord.Text.Trim(), this.comboField2.Text, this.comboClass.Text);

                if (dt != null)
                    textKeyWord.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textKeyValue_TextChanged");
            }
        }

        /// <summary>
        /// 周边文本框匹配
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textKeyValue_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.comboField2.Text, this.comboClass.Text);

                if (dt != null)
                    textKeyWord.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textKeyValue_Click");
            }
        }

        /// <summary>
        /// 高级查询文本框匹配
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textValue_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                if (comboField.Text == "卡口名称" || comboField.Text == "卡口编号" || comboField.Text == "报警卡口名称" || comboField.Text == "报警卡口编号")
                {
                    string keyword = this.textValue.Text.Trim();
                    string colword = string.Empty;

                    if (comboField.Text.IndexOf("卡口名称") > -1)
                    {
                        colword = "卡口名称";
                    }
                    else if (comboField.Text.IndexOf("卡口编号") > -1)
                    {
                        colword = "卡口编号";
                    }

                    if (keyword != "" && colword != "")
                    {
                        string strExp = "select distinct(" + colword + ") from 治安卡口系统 t where " + colword + " like '%" + keyword + "%'  order by " + colword;
                        CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                        DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                        this.textValue.GetSpellBoxSource(dt);
                    }
                }
                else
                {
                    DataTable dt = getListBox(this.textValue.Text.Trim(), this.comboField.Text, this.comboTable.Text);

                    if (dt != null)
                        textValue.GetSpellBoxSource(dt);
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textValue_TextChanged_1");
            }
        }

        /// <summary>
        /// 高级查询文本框匹配
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textValue_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.comboField.Text, this.comboTable.Text);

                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textValue_Click");
            }
        }

        /// <summary>
        /// 错误日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="message">异常字符串</param>
        /// <param name="funName">函数名</param>
        private void WriteLog(string message, string funName)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\timeTestLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine("任务：" + funName + "  　     " + message);
                strWri.Dispose();
                strWri.Close();
            }
            catch
            { }
        }

        /// <summary>
        /// 显示或隐藏进度条
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
            catch (Exception ex) { writeZongheLog(ex, "isShowPro"); }
        }

        /// <summary>
        /// 隐藏条件栏链接
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void LinklblHides_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.tabControl1.SelectedTab == this.tabYintou)               //  音头查询
                    HidesCondition(sender, e, this.groupBox1, null);
                if (this.tabControl1.SelectedTab == this.tabZhoubian)          //  周边查询
                    HidesCondition(sender, e, this.groupBox3, this.textKeyWord);
                if (this.tabControl1.SelectedTab == this.tabAdvance)           //  高级查询
                    HidesCondition(sender, e, this.groupBox2, this.textValue);
            }
            catch (Exception ex) { writeZongheLog(ex, "isShowPro"); }
        }

        /// <summary>
        /// 显示或隐藏条件栏 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="groupBox">groupBox控件</param>
        /// <param name="text">文本框</param>
        private void HidesCondition(object sender, LinkLabelLinkClickedEventArgs e,System.Windows.Forms.GroupBox groupBox,SplitWord.SpellSearchBoxEx text)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;
                link.Visible = true;

                if (link.Text == "隐藏条件栏")
                {
                    if (text != null)
                    {
                        text.Visible = false;
                    }
                    groupBox.Visible = false;
                    link.Text = "显示条件栏";
                }
                else
                {
                    if (text != null)
                    {
                        text.Visible = true;
                    }
                    groupBox.Visible = true;
                    link.Text = "隐藏条件栏";
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textValue_Click");
            }
        }

        private int iflash = 0;
        /// <summary>
        /// 图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void timerFlash_Tick(object sender, EventArgs e)
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
                    this.timerFlash.Enabled = false;
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "timerFlash_Tick-图元闪烁");
            }
        }

        /// <summary>
        /// 全角半角处理
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void textValue_MouseDown(object sender, MouseEventArgs e)
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
                writeZongheLog(ex, "timerFlash_Tick-图元闪烁");
            }
        }

        /// <summary>
        /// 切换中队事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void cmbZhongdu_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbZhongdu.Enabled)
                {
                    cmbJinWuShi.Items.Clear();
                    string sql = "select 警务室代码,警务室名 from 社区警务室";
                    if (cmbZhongdu.Text != "所有派出所")
                    {
                        sql += " where 所属警务室='" + cmbZhongdu.Text + "'";
                    }

                    CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                    DataTable tab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                    cmbJinWuShi.Items.Add("所有警务室");
                    foreach (DataRow row in tab.Rows)
                    {
                        cmbJinWuShi.Items.Add(row[1].ToString());
                    }
                    cmbJinWuShi.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "cmbPic_SelectedIndexChanged");
            }
        }

        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                return dt;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "GetTable");
                return null;
            }
        }

        private DataTable dataZhonghe = null;     /// 
        private DataTable dataZhoubian = null;    /// 二个全局变量用于存放二个模块的查询数据 
        private frmDisplay fmDis = null;

        /// <summary>
        /// 综合查询放大查看数据按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnEnlarge_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataZhonghe == null)
                {
                    MessageBox.Show("无数据展示，请选查询出数据后放大查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new frmDisplay(dataZhonghe);

                fmDis.dataGridDisplay.CellClick += this.dataGridView_CellClick;
                fmDis.dataGridDisplay.CellDoubleClick += this.dataGridView_CellDoubleClick;

                fmDis.Show();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "btnEnlarge_Click");
            }
        }

        /// <summary>
        /// 清除放大数据内容并关闭该窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void clearData()
        {
            try
            {
                dataZhonghe = null;
                dataZhoubian = null;
                fmDis.Close();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "clearData");
            }
        }

        /// <summary>
        /// 周边查询的放大查看数据按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnEnlaData_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataZhoubian == null)
                {
                    MessageBox.Show("无数据展示，请选查询出数据后放大查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new frmDisplay(dataZhoubian);

                fmDis.dataGridDisplay.CellClick += this.dataGridView_CellClick;
                fmDis.dataGridDisplay.CellDoubleClick += this.dataGridView_CellDoubleClick;

                fmDis.Show();
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "btnEnlarge_Click");
            }
        }
    }
}