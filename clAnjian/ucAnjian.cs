using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using MapInfo.Mapping.Thematics;
using MapInfo.Windows.Dialogs;
using MapInfo.Mapping;
using MapInfo.Styles;
using MapInfo.Data;
using MapInfo.Mapping.Legends;
using MapInfo.Geometry;

using MapXtremeMapForm;
using System.Threading;
using System.Xml;
using MapInfo.Engine;
using System.Runtime.InteropServices;

namespace clAnjian
{
    public partial class ucAnjian : UserControl
    {
        private MapInfo.Windows.Controls.MapControl mapControl1;
        private string getFromNamePath;
        string exePath = "";

        private bool is3D = false;
        private string exp3D = "";
        private bool is4D = false;
        private string exp4DHead = "";
        private string exp4DEnd = "";

        private string[] conStr = null;
        private string strConn = "";
        public string strRegion = "";
        public string strRegion1 = "";
        public string user = "";

        public string strRegion2 = ""; // 可导出的派出所
        public string strRegion3 = ""; // 可导出的中队
        public string excelSql = "";   // 查询导出sql
        public string exportSql = "";  // 存放导出的完整SQL 
        public System.Data.DataTable dtExcel = null;//导出表

        public int anjianDouble = 0; //在柱状饼状图是才有双击事件
        public ToolStripProgressBar toolPro;  // 用于查询的进度条　lili 2010-8-11
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

        private cl3Color.uc3Color f3Color;
        private clCXTJ.ucCXTJ fCXTJ;
        public ucAnjian(MapInfo.Windows.Controls.MapControl m, string s,string[] canStr,string getFromPath)
        {
            try
            {
                InitializeComponent();
                exePath = Application.StartupPath;    //程序路径
                mapControl1 = m;
                getFromNamePath = getFromPath;
                strConn = s;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(canStr[0], canStr[1], canStr[2]);
                InitialSet();
                this.P_setfield();          // fisher in 09-12-28
                conStr = canStr;
                InitialComboBoxText();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "构造函数");
            }
        }

        /// <summary>
        /// 初始化字段和条件下拉框
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void InitialComboBoxText()
        {
            try
            {
                comboField.Text = comboField.Items[0].ToString();
                comboTiaojian.Text = comboTiaojian.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "InitialComboBoxText");
            }
        }

        public void InitialCX3CComboBoxText(string region)
        {
            try
            {
                if (region != "顺德区")
                {
                    string[] a = region.Split(',');
                    fCXTJ.comboBoxZD.Items.Clear();
                    fCXTJ.comboBoxZD.Items.Add("全部");
                    f3Color.comboBoxZD.Items.Clear();
                    f3Color.comboBoxZD.Text = "";
                    for (int i = 0; i < a.Length; i++)
                    {
                        fCXTJ.comboBoxZD.Items.Add(a[i]);
                        f3Color.comboBoxZD.Items.Add(a[i]);
                    }
                }
                else
                {
                    try
                    {
                        //打开数据库
                        string aStr = "select 派出所名 from 基层派出所 where 派出所名 not like '%大队%'";
                        DataTable dt = new DataTable();
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(aStr);
                        fCXTJ.comboBoxZD.Items.Clear();
                        fCXTJ.comboBoxZD.Items.Add("全部");
                        f3Color.comboBoxZD.Items.Clear();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            fCXTJ.comboBoxZD.Items.Add(dt.Rows[i][0].ToString());
                            f3Color.comboBoxZD.Items.Add(dt.Rows[i][0].ToString());
                        }
                        dt = null;
                    }
                    catch (Exception ex)
                    {
                        writeAnjianLog(ex, "InitialCX3CComboBoxText");
                    }
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "InitialCX3CComboBoxText");
            }
        }

        private void InitialSet()
        {
            try
            {
                this.mapControl1.MouseDoubleClick += new MouseEventHandler(mapControl1_MouseDoubleClick);
                mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);
                this.mapControl1.Tools.FeatureSelected += new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);

                f3Color = new cl3Color.uc3Color(mapControl1, strConn);
                f3Color.user = user;

                tabPage2.Controls.Add(f3Color);
                f3Color.Dock = DockStyle.Fill;
                f3Color.strRegion = strRegion;

                fCXTJ = new clCXTJ.ucCXTJ(mapControl1, strConn);
                fCXTJ.user = user;
                fCXTJ.strRegion = strRegion;
                tabPage3.Controls.Add(fCXTJ);
                fCXTJ.Dock = DockStyle.Fill;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "InitialSet");
            }
        }

        private void mapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                if (anjianDouble == 0) { return; }
                double[] dCounts = null;
                string[] sLabels = null;
                frmChart fChart = null;
                //MIConnection miConn = new MIConnection();
                try
                {
                    IResultSetFeatureCollection fc = Session.Current.Selections.DefaultSelection[this.queryTable];//得到选取中对像的集合
                    if (fc == null)
                    {
                        return;
                    }
                    Feature f = fc[0];
                    if (f == null) return;

                    string pcs = "";
                    string themeTitle = "";
                    string xTitle = "";
                    string yTitle = "";
                    if (anjianDouble == 1)
                    {
                        SearchInfo si = null;
                        if (sLevelRange == "派出所")
                        {
                            si = MapInfo.Data.SearchInfoFactory.SearchAll();
                            themeTitle = "各派出所案件统计";
                            xTitle = "派出所名";
                            yTitle = "案件量";
                        }
                        else if (sLevelRange == "中队")
                        {
                            pcs = f["所属派出所"].ToString();
                            si = MapInfo.Data.SearchInfoFactory.SearchWhere("所属派出所='" + pcs + "'");
                            themeTitle = pcs + "各中队案件统计";
                            xTitle = "中队名";
                            yTitle = "案件量";
                        }
                        else { return; }
                        si.QueryDefinition.Columns = null;
                        fc = MapInfo.Engine.Session.Current.Catalog.Search(this.queryTable.Alias, si);
                        if (fc == null)
                        {
                            return;
                        }
                        dCounts = new double[fc.Count];
                        sLabels = new string[fc.Count];
                        for (int i = 0; i < fc.Count; i++)
                        {
                            f = fc[i];
                            dCounts[i] = Convert.ToDouble(f["iCount"]);
                            sLabels[i] = f["name"].ToString();
                        }
                        fChart = new frmChart(conStr);
                        fChart.CreateBarChart(dCounts, sLabels, "统计图", themeTitle, xTitle, yTitle);
                    }
                    else
                    {
                        sLabels = aStr;
                        dCounts = new double[aStr.Length];
                        for (int i = 0; i < aStr.Length; i++)
                        {
                            dCounts[i] = Convert.ToDouble(f[aStr[i]]);
                        }
                        fChart = new frmChart(conStr);
                        fChart.CreatePieChart(dCounts, sLabels, f["name"].ToString() + "案件类别统计图");
                    }
                    f = null;
                    fc = null;
                    fChart.groupBox1.Visible = false;
                    fChart.ShowDialog();

                }
                catch (Exception ex)
                {
                    writeAnjianLog(ex, "mapControl1_MouseDoubleClick");
                }
            }
        }

        //即时查询
        private string strExpress = "select * from 案件信息 t";
        private DataTable dataTable = null;
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkOption.Checked && dataGridExp.Rows.Count == 0)
                {
                    MessageBox.Show("请添加查询语句!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (checkOption.Checked && getSqlString() == "")
                {
                    System.Windows.Forms.MessageBox.Show("查询语句有错误,请重设!", "提示");
                    return;
                }

                if (checkTime.Checked && Convert.ToDateTime(dateFrom.Value.Date + timeFrom.Value.TimeOfDay) >= Convert.ToDateTime(dateTo.Value.Date + timeTo.Value.TimeOfDay))
                {
                    MessageBox.Show("起始时间应小于终止时间,请重设!", "时间设置错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                anJianSearch();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "buttonSearch_Click");
            }
        }

        // Table oracleTab = null;
        private void anJianSearch()
        {
            try
            {
                isShowPro(true);
                this.toolPro.Maximum = 6;
                string addExp = "";
                if (strRegion != "顺德区" && strRegion != "")
                {
                    string sRegion = strRegion;
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        sRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    addExp = " (所属派出所 in ('" + sRegion.Replace(",", "','") + "'))";
                }
                // add by fisher in 09-11-26
                if (strRegion1 != "")
                {
                    if (addExp.IndexOf("所属派出所") > -1)
                    {
                        addExp = addExp.Remove(addExp.LastIndexOf(")"));
                        addExp += " or 所属中队 in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        addExp += " (所属中队 in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }


                if (checkOption.Checked == false && checkTime.Checked == false)
                {
                    strExpress = "select * from 案件信息 t";
                    if (addExp != "")
                    {
                        strExpress += " where " + addExp;
                    }
                    isShowPro(false);
                    MessageBox.Show("请设定关键词查询条件或者时间条件!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                this.Cursor = Cursors.WaitCursor;

                //打开数据库
                strExpress = "select * from 案件信息 t where ";
                string generalOption = "";
                if (checkOption.Checked)
                {
                    generalOption = getSqlString();
                    strExpress += generalOption;
                }
                string timeOption = "";
                if (checkTime.Checked)
                {
                    timeOption = "发案时间初值>=to_date('" + (dateFrom.Value.Date + timeFrom.Value.TimeOfDay) + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<=to_date('" + (dateTo.Value.Date + timeTo.Value.TimeOfDay) + "','yyyy-mm-dd hh24:mi:ss')";
                    if (checkOption.Checked)
                    {
                        strExpress += " and " + timeOption;
                    }
                    else
                    {
                        strExpress += timeOption;
                    }
                }
                if (addExp != "")
                {
                    strExpress += " and " + addExp;
                }

                if (radioJingzong.Checked)
                {
                    strExpress += " and 案件来源='警综'";
                }
                else
                {
                    strExpress += " and 案件来源='110'";
                }

                // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                if (strExpress.IndexOf("where") >= 0)    // 判断字符串中是否有where
                    strExpress += " and (备用字段一 is null or 备用字段一='')";
                else
                    strExpress += " where (备用字段一 is null or 备用字段一='')";
                //-------------------------------------------------------
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeAnjianLog(ex, "anJianSearch");
                MessageBox.Show("查询条件设置错误,请重设!");
            }

            this.toolPro.Value = 1;
            Application.DoEvents();
            MIConnection miConnection = new MIConnection();
            try
            {
                miConnection.Open();
                if (this.queryTable != null)
                {
                    queryTable.Close();
                }
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("案件分析") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("案件分析");
                }
                TableInfoServer ti = new TableInfoServer("temAnjian", strConn.Replace("data source = ", "SRVR=").Replace("user id = ", "UID=").Replace("password = ", "PWD="), strExpress, ServerToolkit.Oci);
                ti.CacheSettings.CacheType = CacheOption.Off;

                queryTable = miConnection.Catalog.OpenTable(ti);
                this.toolPro.Value = 2;
                Application.DoEvents();

                MapInfo.Data.Table LTable;
                MICommand command = miConnection.CreateCommand();
                command.CommandText = "insert into 案件分析 Select obj,'案件信息' as 表名,案件名称,案件编号 as 表_ID,案件类型,案别_案由,发案时间初值,所属派出所,所属中队,所属警务室 From " + queryTable.Alias + " t";
                MapInfo.Data.TableInfoNative ListTableInfo = new MapInfo.Data.TableInfoNative("案件分析");
                ListTableInfo.Temporary = false;
                ListTableInfo.TablePath = exePath + "\\案件分析.tab";
                MapInfo.Geometry.CoordSys LCoordsys;
                MapInfo.Data.GeometryColumn GC = (MapInfo.Data.GeometryColumn)(queryTable.TableInfo.Columns["obj"]);
                LCoordsys = GC.CoordSys;
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateFeatureGeometryColumn(LCoordsys));
                //ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStyleColumn());      //add by fisher in 10-01-11 ,不能添加style属性，因为现场数据库中好多数据没有几何信息
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("表名", 50));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("案件名称", 200));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("表_ID", 80));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("案件类型", 80));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("案别_案由", 150));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateDateColumn("发案时间初值"));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("所属派出所", 50));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("所属中队", 100));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("所属警务室", 100));

                ListTableInfo.WriteTabFile();
                LTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ListTableInfo);
                command.ExecuteNonQuery();

                //地图显示
                FeatureLayer fl = new FeatureLayer(LTable);

                mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                //以下由fisher注销（10-01-11），旨在去掉原来追加的样式覆盖，而实现选中feature的闪烁
                try
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 12);

                    MapInfo.Styles.CompositeStyle comStyle = new MapInfo.Styles.CompositeStyle();
                    comStyle.SymbolStyle = pStyle;
                    MapInfo.Mapping.FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier("fsmstyle", comStyle);
                    fl.Modifiers.Append(fsm);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeAnjianLog(ex, "setLayerStyle");
                    MessageBox.Show("设置样式错误．\n" + ex.Message, "提示");
                }
                queryTable.Close();

                this.toolPro.Value = 3;
                Application.DoEvents();

                command.CommandText = "select 案件名称,表_ID as 案件编号,案件类型 from " + LTable.Alias;
                MIDataReader miDr = command.ExecuteReader();

                DataTable dt = new DataTable();
                DataColumn dCol = new DataColumn("案件名称", System.Type.GetType("System.String"));
                dt.Columns.Add(dCol);
                dCol = new DataColumn("案件编号", System.Type.GetType("System.String"));
                dt.Columns.Add(dCol);
                dCol = new DataColumn("案件类型", System.Type.GetType("System.String"));
                dt.Columns.Add(dCol);

                while (miDr.Read())
                {
                    DataRow myDataRow = dt.NewRow();

                    for (int i = 0; i < 3; i++)
                    {
                        myDataRow[i] = miDr[i];
                    }
                    dt.Rows.Add(myDataRow);
                }
                miDr.Close();
                miConnection.Close();

                if (dt == null || dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("无查询结果！");
                }
                else
                {
                    dataGridViewJixi.DataSource = dt;
                    this.toolPro.Value = 4;
                    Application.DoEvents();
                    #region 导出数据
                    //OracleConnection Conn = new OracleConnection(strConn);
                    try
                    {
                        //Conn.Open();
                        //OracleCommand Cmd = Conn.CreateCommand();
                        CLC.ForSDGA.GetFromTable.GetFromName("案件信息", getFromNamePath);
                        string sRegion2 = strRegion2;
                        string sRegion3 = strRegion3;
                        //excelSql = "select 案件名称,案件编号,案发状态,案件类型,案别_案由,简要案情,专案标识,发案时间初值,发案时间终值,发案地点_区县,所属派出所,所属社区,发案地点详址,发案场所,案件来源,作案手段特点,所属派出所代码,所属中队代码,所属警务室代码,涉案人员,抽取ID,抽取更新时间,最后更新人,备用字段一,备用字段二,备用字段三,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y  " + strExpress.Substring(strExpress.IndexOf("from"));
                        excelSql = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " " + strExpress.Substring(strExpress.IndexOf("from"));
                        if (strRegion2 != "顺德区")
                        {
                            if (strRegion2 != "")
                            {
                                if (Array.IndexOf(strRegion2.Split(','), "大良") > -1)
                                {
                                    sRegion2 = strRegion2.Replace("大良", "大良,德胜");
                                }
                                excelSql += " and (所属派出所 in ('" + sRegion2.Replace(",", "','") + "'))";
                                if (strRegion3 != "")
                                {
                                    excelSql = excelSql.Remove(excelSql.LastIndexOf(")"));
                                    excelSql += " or 所属中队 in ('" + strRegion3.Replace(",", "','") + "'))";
                                }
                            }
                            else if (strRegion2 == "")
                            {
                                if (strRegion3 != "")
                                {
                                    excelSql += " and (所属中队 in ('" + strRegion3.Replace(",", "','") + "'))";
                                }
                                else
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
                        }

                        // 启用备用字段一(如果该字段有值则此记录不显示否则记录显示) lili 2010-8-19
                        if (excelSql.IndexOf("where") >= 0)    // 判断字符串中是否有where
                            excelSql += " and (备用字段一 is null or 备用字段一='')";
                        else
                            excelSql += " where (备用字段一 is null or 备用字段一='')";
                        //-------------------------------------------------------
                        exportSql = excelSql;
                        //Cmd.CommandText = excelSql;
                        //OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                        //DataTable datatableExcel = new DataTable();
                        //Adp.Fill(datatableExcel);
                        //if (dtExcel != null) dtExcel.Clear();
                        //dtExcel = datatableExcel;
                        //Cmd.Dispose();
                        //Conn.Close();
                    }
                    catch 
                    {
                        //if (Conn.State == ConnectionState.Open)
                        //    Conn.Close();
                    }
                    #endregion
                    this.toolPro.Value = 5;
                    //Application.DoEvents();
                    

                    //分行设置行的背景色
                    for (int i = 1; i < dataGridViewJixi.Rows.Count; i += 2)
                    {
                        dataGridViewJixi.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    labelCount1.Text = "共 " + dataGridViewJixi.Rows.Count.ToString() + " 条记录。";
                    labelCount1.Visible = true;
                }
                WriteEditLog(":" + strExpress, "查询");
                this.toolPro.Value = 6;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (miConnection.State == ConnectionState.Open)
                {
                    miConnection.Close();
                }
                writeAnjianLog(ex, "anJianSearch");
            }
            this.Cursor = Cursors.Default;
        }

        //将查询结果表示在地图上
        public MapInfo.Data.Table queryTable = null;
        MapInfo.Mapping.FeatureLayer currentFeatureLayer;

        private void tool4D_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewJixi.RowCount < 1)
                {
                    MessageBox.Show("请先进行案件查询");
                    return;
                }

                //删除专题图和图例
                closeLayerZTTB();
                this.mapControl1.Map.Adornments.Clear();
                mapControl1.Map.Legends.Clear();

                is4D = true;
                is3D = false;
                mapControl1.Tools.LeftButtonTool = "drawRectTool";
                //设置查询4D时的查询条件
                string temExp = strExpress.Replace("*", "所属派出所 as name,count(*) as shuliang");

                exp4DHead = "select a.*,b.shuliang from 派出所 a,(" + temExp + " ";

                exp4DEnd = " group by 所属派出所) b where a.派出所名=b.name(+)";
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "tool4D_Click");
            }
        }

        private void tool3D_Click(object sender, EventArgs e)
        {

            try
            {
                if (dataGridViewJixi.RowCount < 1)
                {
                    MessageBox.Show("请先进行案件查询");
                    return;
                }

                //删除专题图和图例
                closeLayerZTTB();
                this.mapControl1.Map.Adornments.Clear();
                mapControl1.Map.Legends.Clear();

                is3D = true;
                is4D = false;
                mapControl1.Tools.LeftButtonTool = "drawRectTool";
                //string caozuofu = comboTiaojian.Text;
                string temExp = strExpress.Replace("*", "所属派出所 as name,count(*) as shuliang");
                temExp += " group by 所属派出所";
                exp3D = "select a.*,b.shuliang from 派出所 a,(" + temExp + ") b where a.派出所名=b.name(+)";
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "tool3D_Click");
            }
        }

        //创建点密度图
        private string strLayer = "";     //专题图的数据层
        private string strAlies = "";   //专题图标识名
        private void toolDotDensity_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewJixi.RowCount < 1)
                {
                    MessageBox.Show("请先进行案件查询");
                    return;
                }
                strAlies = "案件密度";
                string field = "";
                frmDotDensity fDotDensity = new frmDotDensity();
                fDotDensity.strConn = strConn;
                fDotDensity.strRegion = strRegion;
                //add by siumo 2008-12-30:如果权限是某个乡镇,没有派出所选项
                fDotDensity.comboRegionLevel.Items.Clear();
                if (strRegion != "" && strRegion != "顺德区")
                {
                    fDotDensity.comboRegionLevel.Items.Add("中队");
                    fDotDensity.comboRegionLevel.Items.Add("警务室");
                }
                else
                {
                    fDotDensity.comboRegionLevel.Items.Add("派出所");
                    fDotDensity.comboRegionLevel.Items.Add("中队");
                    fDotDensity.comboRegionLevel.Items.Add("警务室");
                }
                fDotDensity.comboRegionLevel.Text = fDotDensity.comboRegionLevel.Items[0].ToString();

                if (fDotDensity.ShowDialog() == DialogResult.OK)
                {
                    if (this.queryTable != null)
                    {
                        this.queryTable.Close();
                    }
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("案件分析") != null && mapControl1.Map.Layers["案件分析"].Enabled)
                    {
                        //MapInfo.Engine.Session.Current.Catalog.CloseTable("案件分析");
                        mapControl1.Map.Layers["案件分析"].Enabled = false;
                    }
                    switch (fDotDensity.comboRegionLevel.Text)
                    {
                        case "派出所":
                            strLayer = "派出所辖区视图";
                            field = "所属派出所";
                            break;
                        case "中队":
                            strLayer = "民警中队辖区视图";
                            field = "所属中队";
                            break;
                        case "警务室":
                            strLayer = "警务室辖区视图";
                            field = "所属警务室";
                            break;
                    }
                    ClearDotDensityThemeAndLegend("专题图表", strAlies);  //先清除上次生成的密度图
                    ClearRangeThemeAndLegend(strRangeLayer, "案件范围");   //先清除
                    CloseOracleSpatialTable();  //先关闭临时表
                    OpenOracleSpatialTable(strLayer, fDotDensity.comboRegionLevel.Text, fDotDensity.comboBoxHighLevel.Text);  //打开用来做专题图的表
                    FeatureLayer lyr = mapControl1.Map.Layers[0] as FeatureLayer;
                    bool bindSuccess = bindTableStatic(lyr, field, "案件量");
                    if (bindSuccess == false)
                    {
                        MessageBox.Show("数据错误，请重设查询条件！");
                        return;
                    }
                    DotDensityTheme thm = new DotDensityTheme(lyr, "iCount",
                    strAlies, fDotDensity.dotColor, DotDensitySize.Large);
                    thm.ValuePerDot = fDotDensity.numPerDot;
                    lyr.Modifiers.Append(thm);
                    // 创建图例
                    Legend legend = mapControl1.Map.Legends.CreateLegend("图例", "案件密度", new Size(5, 5));
                    legend.Border = true;
                    ThemeLegendFrame frame = LegendFrameFactory.CreateThemeLegendFrame("图例", "案件密度", thm);
                    legend.Frames.Append(frame);
                    frame.Title = "案件量/每点";
                    this.mapControl1.Map.Adornments.Append(legend);
                    // Set the initial legend location to be the lower right corner of the map control.
                    System.Drawing.Point pt = new System.Drawing.Point(0, 0);
                    pt.X = mapControl1.Size.Width - legend.Size.Width;
                    pt.Y = mapControl1.Size.Height - legend.Size.Height;
                    legend.Location = pt;

                    //如果lyr中无对象,setview出错,使用try跳过
                    mapControl1.Map.SetView(lyr);
                }
                WriteEditLog(":" + strExpress, "密度图");
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "toolDotDensity_Click");
            }
        }

        private void CloseOracleSpatialTable()
        {
            try
            {
                Session.Current.Catalog.CloseTable("专题图表");
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "CloseOracleSpatialTable");
            }
        }

        private void OpenOracleSpatialTable(string strLayer, string level, string strRg)
        {
            MIConnection miConnection = new MIConnection();
            try
            {
                string strSQL = "Select  *  From " + strLayer;
                TableInfoServer ti = new TableInfoServer(strLayer, strConn.Replace("data source = ", "SRVR=").Replace("user id = ", "UID=").Replace("password = ", "PWD="), strSQL, ServerToolkit.Oci);
                //TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + uid + ";PWD=" + pwd, "Select  *  From " + tableName, ServerToolkit.Oci);
                ti.CacheSettings.CacheType = CacheOption.Off;


                miConnection.Open();
                Table editTable = miConnection.Catalog.OpenTable(ti);

                createInfoPointLayer(editTable.TableInfo.Columns);

                string currentSql = "Insert into " + this.queryTable.Alias + "  Select * From " + editTable.Alias;//复制图元数据
                if (level == "中队" && strRg != "全部")
                {
                    currentSql += " where 所属派出所='" + strRg + "'";
                }

                if (miConnection.State != ConnectionState.Open)
                    miConnection.Open();
                MapInfo.Data.MICommand mapCommand = miConnection.CreateCommand();
                mapCommand.CommandText = currentSql;
                mapCommand.ExecuteNonQuery();
                currentFeatureLayer = new MapInfo.Mapping.FeatureLayer(this.queryTable, "queryLayer");

                //FeatureLayer fl = new FeatureLayer(editTable);
                mapControl1.Map.Layers.Insert(0, (IMapLayer)currentFeatureLayer);
                editTable.Close();
                miConnection.Close();
            }
            catch (System.AccessViolationException ex)
            {
                if (miConnection.State == ConnectionState.Open)
                {
                    miConnection.Close();
                }
                writeAnjianLog(ex, "OpenOracleSpatialTable");
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
                MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable("专题图表");

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
                writeAnjianLog(ex, "createInfoPointLayer");
            }
        }

        //绑定统计值到表
        private bool bindTableStatic(FeatureLayer lyr, string field, string content)
        {
            MIConnection miConn = new MIConnection();
            try
            {
                //打开数据库
                dataTable = new DataTable();
                DataColumn dCol = new DataColumn("name", System.Type.GetType("System.String"));
                dataTable.Columns.Add(dCol);
                dCol = new DataColumn("iCount", System.Type.GetType("System.Int32"));
                dataTable.Columns.Add(dCol);

                miConn.Open();
                MICommand miCmd = miConn.CreateCommand();
                miCmd.CommandText = "select " + field + " as name, count(*) as iCount from 案件分析 group by " + field;
                MIDataReader miDr = miCmd.ExecuteReader();

                DataRow dRow;
                while (miDr.Read())
                {
                    dRow = dataTable.NewRow();
                    dRow["name"] = miDr["name"].ToString();
                    dRow["iCount"] = Convert.ToInt32(miDr["iCount"]);
                    dataTable.Rows.Add(dRow);
                }

                miDr.Close();
                miConn.Close();

                if (dataTable == null || dataTable.Rows.Count < 1)
                {
                    return false;
                }
                else
                {
                    // Now open a MapInfo Table which accesses this DataTable
                    if (Session.Current.Catalog.GetTable("tempBindTable") != null)
                    {
                        Session.Current.Catalog.CloseTable("tempBindTable");
                    }
                    TableInfoAdoNet ti = new TableInfoAdoNet("tempBindTable");
                    ti.ReadOnly = false;

                    ti.DataTable = dataTable;
                    Table table = Session.Current.Catalog.OpenTable(ti);
                    Table targetTab = lyr.Table;
                    //如果之前绑定过iCount，先移除
                    //try
                    //{
                    //    System.Collections.Specialized.StringCollection strCol = new System.Collections.Specialized.StringCollection();
                    //    strCol.Add("iCount");
                    //    targetTab.DropColumns(strCol);
                    //}

                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine(ex.Message);
                    //}
                    Columns col = new Columns();
                    col.Add(new Column("iCount", MIDbType.Int, "iCount"));
                    targetTab.AddColumns(col, BindType.DynamicCopy, table, "name", Operator.Equal, "NAME");
                    Session.Current.Catalog.CloseTable("tempBindTable");
                    return true;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "bindTableStatic");
                if (miConn.State == ConnectionState.Open)
                {
                    miConn.Close();
                }
                return false;
            }
        }

        //清楚点密度专题图和图例
        private void ClearDotDensityThemeAndLegend(string tabName, string themeAlies)
        {
            try
            {
                FeatureLayer lyr = mapControl1.Map.Layers[tabName] as FeatureLayer;
                if (lyr != null)
                {
                    if (lyr.Modifiers[themeAlies] != null)
                        lyr.Modifiers.Remove(themeAlies);
                    if (mapControl1.Map.Adornments[themeAlies] != null)
                    {
                        mapControl1.Map.Adornments.Remove(themeAlies);
                        mapControl1.Map.Legends.Remove(themeAlies);
                    }
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "ClearDotDensityThemeAndLegend");
            }
        }

        //创建范围分析专题图
        string strRangeLayer = "";
        string sLevelRange = "";
        string field = "";
        private void toolRegion_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewJixi.RowCount < 1)
                {
                    MessageBox.Show("请先进行案件查询");
                    return;
                }
                frmRegionOption fRegionOption = new frmRegionOption();
                fRegionOption.strConn = strConn;
                fRegionOption.strRegion = strRegion;
                //add by siumo 2008-12-30:如果权限是某个乡镇,没有派出所选项
                fRegionOption.comboRegionLevel.Items.Clear();
                //if (strRegion != "" && strRegion != "顺德区")
                //{
                //    fRegionOption.comboRegionLevel.Items.Add("中队");
                //    fRegionOption.comboRegionLevel.Items.Add("警务室");
                //}
                //else
                //{
                fRegionOption.comboRegionLevel.Items.Add("派出所");
                fRegionOption.comboRegionLevel.Items.Add("中队");
                fRegionOption.comboRegionLevel.Items.Add("警务室");
                //}
                fRegionOption.comboRegionLevel.Text = fRegionOption.comboRegionLevel.Items[0].ToString();
                if (fRegionOption.ShowDialog() == DialogResult.OK)
                {
                    if (this.queryTable != null)
                        this.queryTable.Close();
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("案件分析") != null && mapControl1.Map.Layers["案件分析"].Enabled)
                    {
                        //MapInfo.Engine.Session.Current.Catalog.CloseTable("案件分析");
                        mapControl1.Map.Layers["案件分析"].Enabled = false;
                    }
                    strRangeLayer = "街镇面";
                    switch (fRegionOption.comboRegionLevel.Text)
                    {
                        case "派出所":
                            strRangeLayer = "派出所辖区视图";
                            field = "所属派出所";
                            break;
                        case "中队":
                            strRangeLayer = "民警中队辖区视图";
                            field = "所属中队";
                            break;
                        case "警务室":
                            strRangeLayer = "警务室辖区视图";
                            field = "所属警务室";
                            break;
                    }

                    sLevelRange = fRegionOption.comboRegionLevel.Text;

                    ClearDotDensityThemeAndLegend("专题图表", "案件密度");
                    ClearRangeThemeAndLegend("专题图表", "案件范围");   //先清除
                    CloseOracleSpatialTable();  //先关闭临时表
                    OpenOracleSpatialTable(strRangeLayer, fRegionOption.comboRegionLevel.Text, fRegionOption.comboBoxHighLevel.Text);  //打开用来做专题图的表
                    if (fRegionOption.radioClass.Checked)
                    {
                        //分级统计图
                        createFenjiTheme(fRegionOption);
                        anjianDouble = 0;
                    }
                    else if (fRegionOption.radioBar.Checked)
                    {
                        createBarTheme(fRegionOption);
                        anjianDouble = 1;
                    }
                    else
                    {
                        createPieTheme(fRegionOption);
                        anjianDouble = 2;
                    }
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "toolRegion_Click");
            }
        }

        string[] aStr = null;
        private void createPieTheme(frmRegionOption fRegionOption)
        {
            try
            {
                FeatureLayer lyr = mapControl1.Map.Layers[0] as FeatureLayer;
                aStr = bindTableType(lyr, field);
                if (aStr == null)
                {
                    MessageBox.Show("数据错误，请重设查询条件！");
                    return;
                }
                //mapControl1.Map.Zoom = new Distance(25, mapControl1.Map.Zoom.Unit);
                if (fRegionOption.comboRegionLevel.Text == "派出所")
                {
                    mapControl1.Map.Scale = 100000;
                }
                else if (fRegionOption.comboRegionLevel.Text == "中队")
                {
                    mapControl1.Map.Scale = 60000;
                }
                else
                {
                    mapControl1.Map.Scale = 30000;
                }
                PieTheme pieTheme = new PieTheme(mapControl1.Map, lyr.Table, aStr);
                ObjectThemeLayer thmLayer = new ObjectThemeLayer("案件类型", null, pieTheme);
                this.mapControl1.Map.Layers.Add(thmLayer);
                pieTheme.DataValueAtSize /= 4;
                pieTheme.GraduateSizeBy = GraduateSizeBy.Constant;
                pieTheme.Clockwise = false;
                pieTheme.Graduated = false;
                pieTheme.StartAngle = 90;
                thmLayer.RebuildTheme();
                // 创建图例
                Legend legend = mapControl1.Map.Legends.CreateLegend("图例", "案件量", new Size(5, 5));
                legend.Border = true;
                ThemeLegendFrame frame = LegendFrameFactory.CreateThemeLegendFrame("图例", "案件类型", pieTheme);
                legend.Frames.Append(frame);
                frame.Title = "案件类型";
                this.mapControl1.Map.Adornments.Append(legend);
                // Set the initial legend location to be the lower right corner of the map control.
                System.Drawing.Point pt = new System.Drawing.Point(0, 0);
                pt.X = 2;
                pt.Y = mapControl1.Size.Height - legend.Size.Height;
                legend.Location = pt;

                //如果lyr中无对象,setview出错,使用try跳过
                mapControl1.Map.SetView(lyr);
                WriteEditLog(":" + strExpress, "饼图");
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "createPieTheme");
            }
        }

        private void createBarTheme(frmRegionOption fRegionOption)
        {
            try
            {
                FeatureLayer lyr = mapControl1.Map.Layers[0] as FeatureLayer;
                bool bindSuccess = bindTableStatic(lyr, field, "案件量");
                if (bindSuccess == false)
                {
                    MessageBox.Show("数据错误，请重设查询条件！");
                    return;
                }
                //mapControl1.Map.Zoom = new Distance(10, mapControl1.Map.Zoom.Unit);
                if (fRegionOption.comboRegionLevel.Text == "派出所")
                {
                    mapControl1.Map.Scale = 80000;
                }
                else if (fRegionOption.comboRegionLevel.Text == "中队")
                {
                    mapControl1.Map.Scale = 40000;
                }
                else
                {
                    mapControl1.Map.Scale = 20000;
                }
                BarTheme barTheme = new BarTheme(mapControl1.Map, lyr.Table, "iCount");
                ObjectThemeLayer thmLayer = new ObjectThemeLayer("案件量", null, barTheme);
                this.mapControl1.Map.Layers.Add(thmLayer);
                barTheme.Stacked = false;
                barTheme.GraduateSizeBy = GraduateSizeBy.Constant;
                // Allow the bars to be different heights. Setting to true would set each bar
                barTheme.GraduatedStacked = false;

                //如果lyr中无对象,setview出错,使用try跳过
                mapControl1.Map.SetView(lyr);
                WriteEditLog(":" + strExpress, "柱状图");
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "createBarTheme");
            }
        }

        private void createFenjiTheme(frmRegionOption fRegionOption)
        {
            try
            {
                DistributionMethod disMethod = DistributionMethod.EqualCountPerRange;
                switch (fRegionOption.CmbMethod.Text)
                {
                    case "数量相等":
                        disMethod = DistributionMethod.EqualCountPerRange;
                        break;
                    case "范围相等":
                        disMethod = DistributionMethod.EqualRangeSize;
                        break;
                    case "自然间隔":
                        disMethod = DistributionMethod.NaturalBreak;
                        break;
                    case "标准偏差":
                        disMethod = DistributionMethod.StandardDeviation;
                        break;
                    case "分位数":
                        //disMethod= DistributionMethod.BIQuantile;
                        break;
                }
                FeatureLayer lyr = mapControl1.Map.Layers[0] as FeatureLayer;
                bool bindSuccess = bindTableStatic(lyr, field, "案件量");
                if (bindSuccess == false)
                {
                    MessageBox.Show("数据错误，请重设查询条件！");
                    return;
                }
                RangedTheme theme = new RangedTheme(lyr, "iCount", "案件范围", Convert.ToInt16(fRegionOption.numericUpDownClass.Value), disMethod);
                int nBins = theme.Bins.Count;
                theme.Bins[0].Style.AreaStyle.Interior = new SimpleInterior((int)PatternStyle.Solid, fRegionOption.BtnColL.BackColor);
                theme.Bins[nBins - 1].Style.AreaStyle.Interior = new SimpleInterior((int)PatternStyle.Solid, fRegionOption.BtnColH.BackColor);
                theme.SpreadBy = SpreadByPart.Color;
                theme.ColorSpreadBy = ColorSpreadMethod.Rgb;
                theme.RecomputeStyles();
                lyr.Modifiers.Append(theme);
                // 创建图例
                Legend legend = mapControl1.Map.Legends.CreateLegend("图例", "案件范围", new Size(5, 5));
                legend.Border = true;
                ThemeLegendFrame frame = LegendFrameFactory.CreateThemeLegendFrame("图例", "案件范围", theme);
                legend.Frames.Append(frame);
                frame.Title = "案件量 (个)";
                this.mapControl1.Map.Adornments.Append(legend);
                // Set the initial legend location to be the lower right corner of the map control.
                System.Drawing.Point pt = new System.Drawing.Point(0, 0);
                pt.X = 2;
                pt.Y = mapControl1.Size.Height - legend.Size.Height;
                legend.Location = pt;

                //如果lyr中无对象,setview出错,使用try跳过
                mapControl1.Map.SetView(lyr);
                WriteEditLog(":" + strExpress, "分级图");
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "createFenjiTheme");
            }
        }

        //清楚专题图和图例
        private void ClearRangeThemeAndLegend(string tabName, string themeAlies)
        {
            try
            {
                //删除分级专题图
                FeatureLayer lyr = mapControl1.Map.Layers[tabName] as FeatureLayer;
                if (lyr != null)
                {
                    if (lyr.Modifiers[themeAlies] != null)
                        lyr.Modifiers.Remove(themeAlies);
                    if (mapControl1.Map.Adornments[themeAlies] != null)
                    {
                        mapControl1.Map.Adornments.Remove(themeAlies);
                        mapControl1.Map.Legends.Remove(themeAlies);
                    }
                }

                //删除之前做好得饼状柱状专题图
                MapLayerEnumerator mapLayerEnum = this.mapControl1.Map.Layers.GetMapLayerEnumerator(MapLayerFilterFactory.FilterByType(typeof(ObjectThemeLayer)));
                while (mapLayerEnum.MoveNext())
                {
                    this.mapControl1.Map.Layers.Remove(mapLayerEnum.Current);
                }

                anjianDouble = 0;

                //删除图例
                this.mapControl1.Map.Adornments.Clear();
                mapControl1.Map.Legends.Clear();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "ClearRangeThemeAndLegend");
            }
        }

        //按案件类型分类，查找各类案件的数量，并绑定到图层中
        private string[] bindTableType(FeatureLayer lyr, string field)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                //打开数据库
                Conn.Open();
                string aStr = strExpress.Replace("*", "distinct 案件类型");
                OracleCommand cmd = new OracleCommand(aStr, Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                dataTable = ds.Tables[0];

                string[] arrStr = new string[dataTable.Rows.Count];
                string temStr = "";
                Columns col = new Columns();
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    arrStr[i] = dataTable.Rows[i][0].ToString();

                    if (i == dataTable.Rows.Count - 1)
                    {
                        temStr += "sum(case 案件类型 when '" + arrStr[i] + "' then 1 else 0 end) " + arrStr[i];
                    }
                    else
                    {
                        temStr += "sum(case 案件类型 when '" + arrStr[i] + "' then 1 else 0 end) " + arrStr[i] + ",";
                    }
                    col.Add(new Column(arrStr[i], MIDbType.Int, arrStr[i]));
                }
                string str = strExpress.Replace("*", field + " as name," + temStr);
                str += " group by " + field;
                cmd = new OracleCommand(str, Conn);
                dAdapter = new OracleDataAdapter(cmd);
                ds = new DataSet();
                dAdapter.Fill(ds);
                dataTable = ds.Tables[0];

                Session.Current.Catalog.CloseTable("tempBindTable");

                TableInfoAdoNet ti = new TableInfoAdoNet("tempBindTable");
                ti.ReadOnly = false;
                ti.DataTable = dataTable;
                Table table = Session.Current.Catalog.OpenTable(ti);
                Table targetTab = lyr.Table;
                targetTab.AddColumns(col, BindType.Static, table, "name", Operator.Equal, "NAME");
                table.Close();
                //Session.Current.Catalog.CloseTable("tempBindTable");
                dAdapter.Dispose();
                cmd.Dispose();
                Conn.Close();
                return arrStr;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "bindTableType");
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                return null;
            }
        }

        //闪烁变量
        private Feature flashFt;
        private Style defaultStyle;
        private int k;
        private void dataGridViewJixi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                //点击一个记录，进行地图定位
                if (flashFt != null)
                {
                    try
                    {
                        flashFt.Style = defaultStyle;
                        flashFt.Update();
                    }
                    catch { }
                }

                FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["案件分析"];
                Table tableInfo = fLayer.Table;


                MapInfo.Data.Feature feat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableInfo.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + dataGridViewJixi.Rows[e.RowIndex].Cells["案件编号"].Value.ToString() + "'"));

                if (feat == null) return;

                OracleConnection oraConn = new OracleConnection(strConn);
                OracleDataAdapter oraOda = null;
                DataSet objset = new DataSet();
                try
                {
                    string sqlStr = "select t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y from 案件信息 t where t.案件编号='" + dataGridViewJixi.Rows[e.RowIndex].Cells["案件编号"].Value.ToString() + "'";
                    oraOda = new OracleDataAdapter(sqlStr, oraConn);
                    oraOda.Fill(objset);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    oraOda.Dispose();
                    oraConn.Close();
                }
                //mapControl1.Map.Scale = 1800;
                //mapControl1.Map.Center = feat.Geometry.Centroid;
                // 以下代码用来将当前地图的视野缩放至该对象所在的派出所   add by fisher in 09-12-24
                MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                //MapInfo.Geometry.FeatureGeometry fg = new MapInfo.Geometry.Point(cSys, dp);
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchIntersectsFeature(feat, IntersectType.Geometry);
                si.QueryDefinition.Columns = null;
                Feature ftjz = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", si);
                //if (ftjz != null)
                //{
                //    mapControl1.Map.SetView(feat.Geometry.Centroid, cSys, getScale());
                //    mapControl1.Map.Center = feat.Geometry.Centroid;
                //    //mapControl1.Map.SetView(ftjz);
                //}
                //else
                //{
                //    //mapControl1.Map.Scale = 1800;
                mapControl1.Map.SetView(feat.Geometry.Centroid, cSys, getScale());
                mapControl1.Map.Center = feat.Geometry.Centroid;
                //}

                //label
                System.Windows.Forms.Label label = (System.Windows.Forms.Label)mapControl1.Parent.Controls["labelName"];
                label.Text = dataGridViewJixi["案件名称", e.RowIndex].Value.ToString();
                System.Drawing.Point sP = new System.Drawing.Point();
                mapControl1.Map.DisplayTransform.ToDisplay(feat.Geometry.Centroid, out sP);
                label.Top = sP.Y + 15;
                label.Left = sP.X + 8;
                if (label.Visible == false)
                {
                    label.Visible = true;
                }
                label.BringToFront();
                mapControl1.Refresh();
                string[] winStr ={ objset.Tables[0].Rows[0][0].ToString(), objset.Tables[0].Rows[0][1].ToString(), "案件信息", dataGridViewJixi.Rows[e.RowIndex].Cells["案件编号"].Value.ToString(), dataGridViewJixi.Rows[e.RowIndex].Cells["案件名称"].Value.ToString() };

                //闪烁要素
                DinPoint(winStr);
                k = 0;
                timer1.Start();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "dataGridViewJixi_CellClick");
            }
        }

        // 获取缩放比例
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
                CLC.BugRelated.ExceptionWrite(ex, "ucAnjian-getScale");
                return 0;
            }
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
                BasePointStyle pStyle = new SimpleVectorPointStyle(35, col, 30);
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

        // 图层上画点并定位
        private void DinPoint(string[] winStr)
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl1.Map.Layers["临时图层"];
                Table tableTem = ftla.Table;
                // 先清除已有对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);

                double dx = 0, dy = 0;

                try
                {
                    dx = Convert.ToDouble(winStr[0]);
                    dy = Convert.ToDouble(winStr[1]);
                }
                catch
                {
                    //MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                if (dx > 0 && dy > 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);
                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 9));

                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                    cs = new CompositeStyle(pStyle);
                    Feature pFeat = new Feature(tableTem.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["表_ID"] = winStr[3];
                    pFeat["表名"] = winStr[2];
                    pFeat["名称"] = winStr[4];
                    tableTem.InsertFeature(pFeat);
                    flashFt = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + winStr[3] + "' and 表名='" + winStr[2] + "'"));
                    defaultStyle = flashFt.Style;
                    if (flashFt != null)
                    {
                        mapControl1.Map.SetView(flashFt);
                    }
                    else
                    {
                        //MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }
                else
                {
                    //MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "DinPoint");
            }
        }

        private MapInfo.Geometry.DPoint dptStart;
        private MapInfo.Geometry.DPoint dptEnd;
        static System.Drawing.Rectangle ScreenMapRect = new System.Drawing.Rectangle();
        MapInfo.Geometry.DRect MapRect = new MapInfo.Geometry.DRect();
        private List<GeoPoint> policeData = new List<GeoPoint>(10);
        private List<List<GeoPoint>> D4policeData = null;
        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            if (this.Visible == false) return;
            switch (e.ToolName)
            {
                case "drawRectTool":
                    //add by siumo 20080923
                    switch (e.ToolStatus)
                    {
                        case MapInfo.Tools.ToolStatus.Start:
                            dptStart = e.MapCoordinate;
                            break;
                        case MapInfo.Tools.ToolStatus.End:
                            dptEnd = e.MapCoordinate;

                            MapRect.x1 = dptStart.x;
                            MapRect.y2 = dptStart.y;
                            MapRect.x2 = dptEnd.x;
                            MapRect.y1 = dptEnd.y;

                            try
                            {
                                mapControl1.Map.DisplayTransform.ToDisplay(MapRect, out ScreenMapRect);
                                ScreenMapRect.X = ScreenMapRect.X + mapControl1.Parent.Location.X + this.Location.X + mapControl1.Parent.Parent.Parent.Location.X + 4;
                                ScreenMapRect.Y = ScreenMapRect.Y + mapControl1.Location.Y + this.Location.Y + 50 + mapControl1.Parent.Parent.Parent.Location.Y + 4;
                            }
                            catch { }
                            System.Drawing.RectangleF GISRect = new RectangleF((float)MapRect.x1, (float)MapRect.y1, (float)(MapRect.Width()), (float)(MapRect.Height()));

                            Bitmap bMap = capture();

                            if (is3D)
                            {
                                OracleConnection con = new OracleConnection(strConn);
                                try
                                {
                                    List<GeoPoint> policeData = new List<GeoPoint>(10);
                                    con.Open();
                                    OracleCommand cmd = new OracleCommand(exp3D, con);
                                    OracleDataReader dr = cmd.ExecuteReader();
                                    while (dr.Read())
                                    {
                                        GeoPoint gp;
                                        if (dr["shuliang"].ToString() == "")
                                        {
                                            gp = new GeoPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]), 0);
                                        }
                                        else
                                        {
                                            gp = new GeoPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]), Convert.ToInt32(dr["shuliang"]));
                                        }
                                        policeData.Add(gp);
                                    }
                                    dr.Close();
                                    con.Close();
                                    if (policeData.Count > 0)
                                    {
                                        GLThread mythread = new GLThread(bMap, ScreenMapRect, GISRect, policeData);
                                        Thread thread = new Thread(new ThreadStart(mythread.startThread));
                                        thread.IsBackground = true;
                                        thread.Start();
                                    }
                                    else
                                    {
                                        MessageBox.Show("所选区域无数据。");
                                    }

                                    is3D = false;
                                    mapControl1.Tools.LeftButtonTool = "Pan";
                                    WriteEditLog(":" + strExpress, "三维显示");
                                }
                                catch (Exception ex)
                                {
                                    writeAnjianLog(ex, "Tools_Used");
                                    if (con.State == ConnectionState.Open)
                                        con.Close();
                                }
                            }
                            if (is4D)
                            {
                                int beginYear = 0;
                                int engYear = 0;
                                int beginMonth = 0;
                                int engMonth = 0;

                                TimeSpan tSpan;
                                DateTime dateTimeBegin, dateTimeEnd;
                                if (checkTime.Checked)
                                {
                                    beginYear = dateFrom.Value.Year;
                                    engYear = dateTo.Value.Year;
                                    beginMonth = dateFrom.Value.Month;
                                    engMonth = dateTo.Value.Month;
                                    tSpan = dateTo.Value.Date - dateFrom.Value.Date;
                                    dateTimeBegin = dateFrom.Value;
                                    dateTimeEnd = dateTo.Value;
                                }
                                else
                                {
                                    frmTimeOption fTO = new frmTimeOption();
                                    if (fTO.ShowDialog() == DialogResult.OK)
                                    {
                                        beginYear = fTO.dateTimeBegin.Value.Year;
                                        engYear = fTO.dateTimeEnd.Value.Year;
                                        beginMonth = fTO.dateTimeBegin.Value.Month;
                                        engMonth = fTO.dateTimeEnd.Value.Month;
                                        tSpan = fTO.dateTimeEnd.Value.Date - fTO.dateTimeBegin.Value.Date;
                                        dateTimeBegin = fTO.dateTimeBegin.Value;
                                        dateTimeEnd = fTO.dateTimeEnd.Value;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                List<string> listTime = new List<string>();
                                D4policeData = new List<List<GeoPoint>>(4);
                                OracleConnection con = new OracleConnection(strConn);
                                try
                                {
                                    con.Open();

                                    //在同一个月，使用天作为分割单位
                                    if ((beginYear == engYear && beginMonth == engMonth) || (tSpan.Days <= 10))
                                    {
                                        for (int i = 0; i <= tSpan.Days; i++)
                                        {
                                            D4policeData.Add(new List<GeoPoint>(5));

                                            DateTime aDate = dateTimeBegin.Date.AddDays(i);
                                            listTime.Add(aDate.ToShortDateString().Replace('-', '/'));
                                            string exp = exp4DHead + "and 发案时间初值>to_date('" + aDate.ToShortDateString() + "','yyyy-mm-dd') and 发案时间初值<=to_date('" + aDate.ToShortDateString() + " 23:59:59','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
                                            OracleCommand cmd = new OracleCommand(exp, con);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            while (dr.Read())
                                            {
                                                GeoPoint gp;
                                                if (dr["shuliang"].ToString() == "")
                                                {
                                                    gp = new GeoPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]), 0);
                                                }
                                                else
                                                {
                                                    gp = new GeoPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]), Convert.ToInt32(dr["shuliang"]));
                                                }
                                                D4policeData[i].Add(gp);
                                            }
                                            dr.Close();
                                        }
                                    }
                                    else
                                    {
                                        int i = 0;
                                        while (true)
                                        {
                                            DateTime d1 = Convert.ToDateTime(beginYear.ToString() + "-" + beginMonth.ToString());
                                            int temMonth = beginMonth + 1;
                                            int temYear = beginYear;
                                            if (temMonth > 12)
                                            {
                                                temMonth = temMonth - 12;
                                                temYear++;
                                            }
                                            DateTime d2 = Convert.ToDateTime(temYear.ToString() + "-" + temMonth.ToString());
                                            string exp = "";
                                            if (beginYear == dateTimeBegin.Year && beginMonth == dateTimeBegin.Month)
                                            {
                                                exp = exp4DHead + "and 发案时间初值>=to_date('" + dateTimeBegin + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
                                            }
                                            else if (beginYear == engYear && beginMonth == engMonth)
                                            {
                                                exp = exp4DHead + "and 发案时间初值>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + dateTimeEnd + "','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
                                            }
                                            else
                                            {
                                                exp = exp4DHead + "and 发案时间初值>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
                                            }
                                            OracleCommand cmd = new OracleCommand(exp, con);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            if (dr.HasRows)
                                            {
                                                D4policeData.Add(new List<GeoPoint>(5));
                                                listTime.Add(beginYear.ToString() + "/" + beginMonth.ToString());
                                                while (dr.Read())
                                                {
                                                    GeoPoint gp;
                                                    if (dr["shuliang"].ToString() == "")
                                                    {
                                                        gp = new GeoPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]), 0);
                                                    }
                                                    else
                                                    {
                                                        gp = new GeoPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]), Convert.ToInt32(dr["shuliang"]));
                                                    }
                                                    D4policeData[i].Add(gp);
                                                }
                                                i++;
                                            }
                                            dr.Close();
                                            beginMonth++;
                                            if (beginMonth > 12)
                                            {
                                                beginYear++;
                                                beginMonth = beginMonth - 12;
                                            }
                                            if (beginYear == engYear && beginMonth > engMonth)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    con.Close();
                                    if (D4policeData.Count > 0)
                                    {
                                        GLThread mythread = new GLThread(bMap, ScreenMapRect, GISRect, D4policeData, listTime);
                                        Thread thread = new Thread(new ThreadStart(mythread.startThread));
                                        thread.IsBackground = true;
                                        thread.Start();
                                    }
                                    else
                                    {
                                        MessageBox.Show("所选时间无数据。");
                                    }
                                    WriteEditLog(":" + strExpress, "四维演示");
                                }
                                catch (Exception ex)
                                {
                                    writeAnjianLog(ex, "Tools_Used");
                                    if (con.State == ConnectionState.Open)
                                        con.Close();
                                }
                            }
                            is4D = false;
                            mapControl1.Tools.LeftButtonTool = "Pan";

                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern bool BitBlt(
           IntPtr hdcDest, //目标设备的句柄 
           int nXDest, // 目标对象的左上角的X坐标
           int nYDest, // 目标对象的左上角的Y坐标
           int nWidth, // 目标对象的矩形的宽度
           int nHeight, // 目标对象的矩形的长度
           IntPtr hdcSrc, // 源设备的句柄
           int nXSrc, // 源对象的左上角的X坐标
           int nYSrc, // 源对象的左上角的Y坐标
           System.Int32 dwRop // 光栅的操作值
         );

        public static Bitmap capture()
        {
            try
            {
                //建立屏幕Graphics
                Graphics grpScreen = Graphics.FromHwnd(IntPtr.Zero);
                //根据屏幕大小建立位图
                Bitmap bitmap = new Bitmap(ScreenMapRect.Width, ScreenMapRect.Height, grpScreen);
                //建立位图相关Graphics
                Graphics grpBitmap = Graphics.FromImage(bitmap);
                //建立屏幕上下文
                IntPtr hdcScreen = grpScreen.GetHdc();
                //建立位图上下文
                IntPtr hdcBitmap = grpBitmap.GetHdc();
                //将屏幕捕获保存在图位中
                BitBlt(hdcBitmap, 0, 0, ScreenMapRect.Width, ScreenMapRect.Height, hdcScreen, ScreenMapRect.Left, ScreenMapRect.Top, 0x00CC0020);
                //关闭位图句柄
                grpBitmap.ReleaseHdc(hdcBitmap);
                //关闭屏幕句柄
                grpScreen.ReleaseHdc(hdcScreen);
                //释放位图对像
                grpBitmap.Dispose();
                //释放屏幕对像
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Bitmap Bigbitmap = new Bitmap(1024, 1024, grpScreen);
                Graphics biggrpBitmap = Graphics.FromImage(Bigbitmap);
                biggrpBitmap.DrawImageUnscaled(bitmap, 0, 0);

                grpBitmap.Dispose();
                grpScreen.Dispose();

                //返回捕获位图
                return Bigbitmap;
            }
            catch
            {
                return null;
            }
        }

        private void toolStatics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "staTongHuanbi")
            {
                try
                {
                    frmStatic fStatic = new frmStatic();
                    //add by siumo 2008-12-30:如果权限是某个乡镇,没有派出所选项
                    fStatic.comboRegion.Items.Clear();
                    //if (strRegion != "" && strRegion != "顺德区")
                    //{
                    //    fStatic.comboRegion.Items.Add("中队");
                    //    fStatic.comboRegion.Items.Add("警务室");
                    //}
                    //else
                    //{
                    fStatic.comboRegion.Items.Add("派出所");
                    fStatic.comboRegion.Items.Add("中队");
                    fStatic.comboRegion.Items.Add("警务室");
                    //}
                    fStatic.comboRegion.Text = fStatic.comboRegion.Items[0].ToString();

                    fStatic.strConn = strConn;
                    fStatic.strRegion = strRegion;
                    fStatic.Show();
                    WriteEditLog("", "同环比");
                }
                catch (Exception ex) { writeAnjianLog(ex, "toolStatics_DropDownItemClicked"); }
            }
            else
            {
                frmChart fSta = null;

                string sExp = "";
                try
                {
                    switch (e.ClickedItem.Name)
                    {
                        case "staByType":
                            if (strExpress == "select * from 案件信息 t")
                            {
                                DialogResult dResult = MessageBox.Show("未设置查询条件,将对全部案件进行统计,是否继续?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dResult == DialogResult.No)
                                {
                                    return;  //未设置查询条件进行查询，跳出
                                }
                                // sExp = "select count(*),名称1 from 案件信息类别 group by 名称1";
                            }
                            //else {
                            sExp = strExpress.Replace("*", "count(*),名称1").Replace("from 案件信息", "from 案件信息类别").Replace(" and (备用字段一 is null or 备用字段一='')"," ");
                            sExp += " group by 名称1";
                            //}
                            fSta = new frmChart(conStr);
                            fSta.PieDataStatic(strConn, sExp);
                            fSta.connString = strConn;
                            fSta.strExe = sExp;
                            fSta.TopMost = true;
                            fSta.groupBox1.Visible = true;
                            fSta.Show();
                            WriteEditLog("", "按类别统计");
                            break;
                        case "staByDate":
                            //先获取最小和最大时间
                            if (strExpress == "select * from 案件信息 t")
                            {
                                DialogResult dResult = MessageBox.Show("未设置查询条件,将对全部案件进行统计,是否继续?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dResult == DialogResult.No)
                                {
                                    return;  //未设置查询条件进行查询，跳出
                                }
                            }
                            sExp = strExpress.Replace("*", "min(发案时间初值),max(发案时间初值)").Replace("from 案件信息", "from 案件信息类别").Replace(" and (备用字段一 is null or 备用字段一='')", " ");

                            fSta = new frmChart(conStr);
                            fSta.BarDataStatic(strConn, sExp);
                            fSta.TopMost = true;
                            fSta.groupBox1.Visible = false;
                            fSta.Show();
                            WriteEditLog("", "按日期统计");
                            break;
                        case "staByTime":
                            if (strExpress == "select * from 案件信息 t")
                            {
                                DialogResult dResult = MessageBox.Show("未设置查询条件,将对全部案件进行统计,是否继续?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dResult == DialogResult.No)
                                {
                                    return;  //未设置查询条件进行查询，跳出
                                }
                            }
                            sExp = strExpress.Replace("*", "发案时间初值");

                            fSta = new frmChart(conStr);
                            fSta.LineDataStatic(strConn, sExp);
                            fSta.TopMost = true;
                            fSta.groupBox1.Visible = false;
                            fSta.Show();
                            WriteEditLog("", "按时间统计");
                            break;
                    }

                }
                catch (Exception ex)
                {
                    writeAnjianLog(ex, "toolStatics_DropDownItemClicked");
                }
            }
        }

        private MapInfo.Data.MultiResultSetFeatureCollection mirfc = null;
        private MapInfo.Data.IResultSetFeatureCollection mirfc1 = null;
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
                    if (f.Table.Alias.ToUpper() == "案件分析_SELECTION")
                    {
                        this.showDataGridViewLineOnlyOneTable(f["表_ID"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    writeAnjianLog(ex, "Feature_Selected");
                }
            }
        }

        public void showDataGridViewLineOnlyOneTable(string 表_ID)//在DataGridView中显视
        {
            try
            {
                for (int i = 0; i < this.dataGridViewJixi.Rows.Count; i++)
                {
                    if (this.dataGridViewJixi.Rows[i].Cells["案件编号"].Value.ToString() == 表_ID)
                    {
                        this.dataGridViewJixi.CurrentCell = this.dataGridViewJixi.Rows[i].Cells[0];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "showDataGridViewLineOnlyOneTable");
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                this.textValue.Text = "";
                this.dataGridExp.Rows.Clear();
                if (this.queryTable != null)
                {
                    queryTable.Close();
                }
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("案件分析") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("案件分析");
                }
                this.dataGridViewJixi.DataSource = null;
                labelCount1.Text = "";
                strExpress = "select * from 案件信息 t";
                WriteEditLog("", "重置查询条件");
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "buttonClear_Click");
            }
        }

        public void clearTem()
        {
            try
            {
                if (tabControl1.SelectedTab == tabPage1)
                {
                    clearCXFX();
                }
                else if (tabControl1.SelectedTab == tabPage2)
                {
                    clearSSYJ();
                }
                else
                {
                    closeLayerZTTB();
                    fCXTJ.doubleClick = false;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "buttonClear_Click");
            }
        }

        // 清除临时图层上的所有点
        private void clearLinShi()
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl1.Map.Layers["临时图层"];
                Table tableTem = ftla.Table;
                // 先清除已有对象
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch (Exception ex) { writeAnjianLog(ex, "clearLinShi"); }
        }

        //本功能项不可见时,清楚关闭相应的临时项
        private void ucAnjian_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    clearCXFX();

                    clearSSYJ();

                    closeLayerZTTB();

                    fCXTJ.doubleClick = false;
                }
                else
                {
                    this.dateFrom.Value = DateTime.Now.Date;
                    this.dateTo.Value = DateTime.Now.Date;
                    this.checkTime.Checked = true;
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "ucAnjian_VisibleChanged"); }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.SelectedTab != tabPage2)
                {
                    clearSSYJ();
                    clearLinShi();
                }
                if (tabControl1.SelectedTab != tabPage3)
                {
                    closeLayerZTTB();
                    clearLinShi();
                    fCXTJ.doubleClick = false;
                }
                if (tabControl1.SelectedTab != tabPage1)
                {
                    clearCXFX();

                    closeLayerZTTB();
                    this.dateFrom.Value = DateTime.Now.Date;
                    this.dateTo.Value = DateTime.Now.Date;
                    this.checkTime.Checked = true;
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "ucAnjian_VisibleChanged"); }
        }

        //清除查询分析临时内容
        private void clearCXFX()
        {
            try
            {
                dataGridViewJixi.DataSource = null;
                strExpress = "select * from 案件信息 t";
                labelCount1.Visible = false;
                System.Windows.Forms.Label label = (System.Windows.Forms.Label)mapControl1.Parent.Controls["labelName"];
                label.Visible = false;
                if (this.queryTable != null)
                {
                    this.queryTable.Close();
                }
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("案件分析") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("案件分析");
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "clearCXFX"); }

            ClearDotDensityThemeAndLegend(strLayer, strAlies);  //清除上次生成的密度图
            ClearRangeThemeAndLegend(strRangeLayer, "案件范围");   //先清除
        }

        //清除三色预警临时内容
        private void clearSSYJ()
        {
            string alias = "中队";
            if (f3Color.radioPCS.Checked)
            {
                alias = "派出所";
            }
            try
            {
                if (Session.Current.Catalog.GetTable(alias) != null)
                {
                    Session.Current.Catalog.CloseTable(alias);
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "clearSSYJ"); }
            try
            {
                IMapLayer layer = this.mapControl1.Map.Layers[alias + "label"] as IMapLayer;
                if (layer != null)
                {
                    this.mapControl1.Map.Layers.Remove(layer);
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "clearSSYJ"); }
        }

        //关闭专题图表
        private void closeLayerZTTB()
        {
            try
            {
                if (Session.Current.Catalog.GetTable("专题图表") != null)
                {
                    Session.Current.Catalog.CloseTable("专题图表");
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "closeLayerZTTB"); }
        }

        //程序关闭时,关闭之前未关闭的3D,4D窗体.
        public void closeGLThread()
        {
            try
            {
                GLThread.Close();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "closeGLThread");
            }
        }

        private void dataGridViewJixi_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                DPoint dp = new DPoint();
                string sqlFields = "案件名称,案件编号,案发状态,案件类型,案别_案由,简要案情,专案标识,发案时间初值,发案时间终值,发案地点_区县,所属派出所,所属社区,发案地点详址,发案场所,案件来源,作案手段特点,涉案人员,所属派出所代码,所属中队代码,所属警务室代码,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                string strSQL = "select " + sqlFields + " from 案件信息 t where 案件编号='" + dataGridViewJixi.Rows[e.RowIndex].Cells["案件编号"].Value.ToString() + "'";

                DataSet ds = new DataSet();
                OracleDataAdapter da = new System.Data.OracleClient.OracleDataAdapter(strSQL, Conn);
                da.Fill(ds);
                Conn.Close();

                DataTable datatable = ds.Tables[0];
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

                    this.disPlayInfo(datatable, pt, "临时图层");
                    WriteEditLog(":案件编号='" + dataGridViewJixi.Rows[e.RowIndex].Cells["案件编号"].Value.ToString() + "'", "查看详情");
                    return;
                }
                string[] winStr ={ datatable.Rows[0]["X"].ToString(), datatable.Rows[0]["Y"].ToString(), "案件信息", datatable.Rows[0]["案件编号"].ToString(), datatable.Rows[0]["案件名称"].ToString() };
                if (dp.x == 0 || dp.y == 0)
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2;
                    pt.Y = 10;

                    this.disPlayInfo(datatable, pt, "临时图层");
                    WriteEditLog(":案件编号='" + dataGridViewJixi.Rows[e.RowIndex].Cells["案件编号"].Value.ToString() + "'", "查看详情");
                    return;
                }
                mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                pt.X += this.Width + 10;
                pt.Y += 80;
                this.disPlayInfo(datatable, pt,"临时图层");
                WriteEditLog(":案件编号='" + dataGridViewJixi.Rows[e.RowIndex].Cells["案件编号"].Value.ToString() + "'", "查看详情");
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex, "dataGridViewJixi_CellDoubleClick");
            }
        }

        private nsInfo.FrmInfo frmMessage = new nsInfo.FrmInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string LayerName)
        {
            try
            {
                if (this.frmMessage.Visible == false)
                {
                    this.frmMessage = new nsInfo.FrmInfo();
                    frmMessage.SetDesktopLocation(-30, -30);
                    frmMessage.Show();
                    frmMessage.Visible = false;
                }
                frmMessage.strConn = strConn;
                frmMessage.mapControl = mapControl1;
                frmMessage.getFromNamePath = getFromNamePath;
                frmMessage.setInfo(dt.Rows[0], pt, LayerName);
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "disPlayInfo");
            }
        }

        private void writeAnjianLog(Exception ex, string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine("案件分析:在 " + sFunc + "方法中," + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

        //记录操作记录
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'案件分析:查询分析','案件信息" + sql.Replace('\'', '"') + "','" + method + "')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "WriteEditLog");
            }
        }

        //设置案件中的comboField字段   add by fisher in 09-12-28
        string P_arrType = "";
        private void P_setfield()
        {
            try
            {
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '案件信息类别'";
                DataTable dt = new DataTable();
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);

                comboField.Items.Clear();
                P_arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string aCol = dt.Rows[i][0].ToString();
                    string aType = dt.Rows[i][1].ToString();
                    if (aCol != "" && aCol != "MAPID" && aCol.IndexOf("备用字段") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("发案时间") < 0 && aCol != "案件来源" && aCol != "抽取更新时间" && aCol.IndexOf("代码") < 0 && aCol.Substring(0, 2) != "名称")
                    {
                        comboField.Items.Add(aCol);
                        P_arrType += aType + ",";
                    }
                }
                comboField.Text = "案件名称";
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "P_setfield");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (textValue.Text.Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show("查询值不能为空！", "提示");
                    return;
                }

                if (this.textValue.Text.IndexOf("\'") > -1)
                {
                    System.Windows.Forms.MessageBox.Show("输入的字符串中不能包含单引号!", "提示");
                    return;
                }

                string strExp = "";
                int p = comboField.SelectedIndex;
                string[] arr = P_arrType.Split(',');
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
                            strExp = this.comboField.Text + "   " + this.comboTiaojian.Text + "   " + this.textValue.Text.Trim();
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.comboField.Text + "   " + this.comboTiaojian.Text + "   " + this.textValue.Text.Trim();
                        }
                        this.dataGridExp.Rows.Add(new object[] { strExp, "数字" });
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboTiaojian.Text + "   '" + this.textValue.Text.Trim() + "'";
                            if (this.comboTiaojian.Text.Trim() == "包含")
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
                            strExp = this.connStr.Text + "  " + this.comboField.Text + "   " + this.comboTiaojian.Text + "   '" + this.textValue.Text.Trim() + "'";
                            if (this.comboTiaojian.Text.Trim() == "包含")
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
                writeAnjianLog(ex, "btnAdd_Click");
            }
        }

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
                writeAnjianLog(ex, "btnDelete_Click");
            }
        }

        private void comboField_SelectedIndexChanged(object sender, EventArgs e)
        {
            setYunsuanfuValue(this.comboField.SelectedIndex);
        }

        private void setYunsuanfuValue(int p)
        {
            try
            {
                string[] arr = P_arrType.Split(',');
                string type = arr[p].ToUpper();
                this.comboTiaojian.Items.Clear();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        this.comboTiaojian.Items.Add("等于");
                        this.comboTiaojian.Items.Add("不等于");
                        this.comboTiaojian.Items.Add("大于");
                        this.comboTiaojian.Items.Add("大于等于");
                        this.comboTiaojian.Items.Add("小于");
                        this.comboTiaojian.Items.Add("小于等于");
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        this.comboTiaojian.Items.Add("等于");
                        this.comboTiaojian.Items.Add("不等于");
                        this.comboTiaojian.Items.Add("包含");
                        break;
                }
                this.comboTiaojian.Text = "等于";
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "setYunsuanfuValue()");
            }
        }

        private string getSqlString()//转换字符串
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
                writeAnjianLog(ex, "getSqlString()");

                return "";
            }
        }

        /* 以下是自动补全代码 */

        /// <summary>
        /// 自动补全方法(add by LiLi in 2010-5-21)
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
                writeAnjianLog(ex, "getListBox");
                return null;
            }
        }

        /// <summary>
        /// 列为固定值时自动添加(add by LiLi in 2010-5-21)
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
                writeAnjianLog(ex, "MatchShu");
                return null;
            }
        }

        private void textValue_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.textValue.Text.Trim(), this.comboField.Text, "案件信息");

                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "textValue_TextChanged_1");
            }
        }

        private void textValue_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.comboField.Text, "案件信息");

                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "textValue_Click_1");
            }
        }

        // 按回车时查询
        private void textValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                anJianSearch();
            }
        }

        // 显示或隐藏进度条
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
            catch (Exception ex)
            {
                writeAnjianLog(ex, "isShowPro");
            }
        }

        private void LinklblHides_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;

                if (link.Text == "隐藏条件栏")
                {
                    this.textValue.Visible = false;
                    groupBox1.Visible = false;
                    link.Text = "显示条件栏";
                }
                else
                {
                    this.textValue.Visible = true;
                    groupBox1.Visible = true;
                    link.Text = "隐藏条件栏";
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "LinklblHides_LinkClicked");
            }
        }

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
                writeAnjianLog(ex, "textValue_MouseDown");
            }
        }
    }
}