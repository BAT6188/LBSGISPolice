using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MapInfo.Windows.Controls;
using System.Collections;
using System.Data.OracleClient;
using MapInfo.Data;
using MapInfo.Mapping;
using MapInfo.Engine;
using MapInfo.Mapping.Thematics;
using MapInfo.Geometry;
using System.Globalization;

namespace clCXTJ
{
    public partial class ucCXTJ : UserControl
    {

        private string ConnStr = "";
        private DataTable dataTable = null;        
        private MapControl mapControl1;
        public bool doubleClick = false;
        public string user = "";
        public string strRegion = "";
        private void mapControl1_DoubleClick(object sender, EventArgs e)
        {
            if (doubleClick == false) { return; }
            
            try
            {
                IResultSetFeatureCollection fc = Session.Current.Selections.DefaultSelection[this.queryTable];//得到选取中对像的集合
                if (fc == null)
                {
                    return;
                }
                Feature f = fc[0];
                if (f == null) return;

                ArrayList arrayList = new ArrayList();
                for (int i = 0; i < this.count; i++)
                {
                    if (i == 0)
                    {
                        arrayList.Add(f["iCount"].ToString());
                    }
                    else
                    {
                        arrayList.Add(f["iCount" + i.ToString()].ToString());
                    }
                }
                double[] d = new double[this.count];
                for (int i = 0; i < this.count; i++)
                {
                    d[i] = Convert.ToDouble(arrayList[i].ToString());
                }

                string[] timeStr = new string[this.count];

                TimeSpan time;
                if (radioWeek.Checked)
                {
                    for (int i = 0; i < this.count; i++)
                    {
                        if (i == 0)
                        {
                            timeStr[i] = this.getSubString(this.dateFrom.Value.Date.ToString());
                        }
                        else
                        {
                            time = new TimeSpan(i * 7, 0, 0, 0);
                            timeStr[i] = this.getSubString((this.dateFrom.Value.Date + time).ToString());
                        }
                    }
                }
                else
                {
                    DateTime newDt = this.dateFrom.Value.Date;
                    for (int i = 0; i < this.count; i++)
                    {
                        if (i == 0)
                        {
                            timeStr[i] = this.getSubString(this.dateFrom.Value.Date.ToString());
                        }
                        else
                        {
                            //判断起始所在月份,有多少天就加多少天
                            GregorianCalendar gc = new GregorianCalendar();
                            int days = gc.GetDaysInMonth(newDt.Year, newDt.Month);
                            time = new TimeSpan(days, 0, 0, 0);
                            newDt += time;
                            timeStr[i] = this.getSubString((newDt).ToString());
                        }
                    }
                }  

                FrmStaticBar frmBar = new FrmStaticBar();

                frmBar.CreateBarChart(d, timeStr, f["name"].ToString() + "成效分析统计图", "时间", "案件/警员数");
                frmBar.setParameter(d, timeStr, f["name"].ToString() + "成效分析统计图", "时间", "案件/警员数");
                frmBar.ShowDialog(this);
            }
            catch(Exception ex)
            {
                writeToLog(ex,"clCXTJ-ucCXTJ-mapControl1_DoubleClick");
            }
        }

        private string getSubString(string str)
        {
            int i = str.IndexOf(" ");
            return str.Substring(0,i);            
        }

        public ucCXTJ(MapControl m,string s)
        {
            try
            {
                InitializeComponent();
                mapControl1 = m;
                ConnStr = s;
                this.mapControl1.DoubleClick += new System.EventHandler(this.mapControl1_DoubleClick);
                this.dateFrom.Text = System.DateTime.Now.ToString();
                this.dateTo.Text = System.DateTime.Now.ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clCXTJ-ucCXTJ-构造函数");
            }
        }

        public  ArrayList  arrayList =new ArrayList();

        private int count = 1;
        string strLayer = "派出所辖区视图";
        string field = "所属派出所";
        string field2 = "派出所名称";      //每日警员表中的字段
        string policeTableName = "派出所每日警员表";
        string fieldCode = "派出所编号";
        
        private void getValueCycle(DateTime  startTime ,DateTime  endTime)
        {
            try
            {
                this.count = 0;

                if (radioPCS.Checked)
                {
                    strLayer = "派出所辖区视图";
                    policeTableName = "派出所每日警员表";
                    field = "案件信息.所属派出所";
                    fieldCode = "案件信息.所属派出所代码";
                    field2 = "派出所每日警员表.派出所代码";
                }
                else
                {
                    strLayer = "民警中队辖区视图";
                    field = "案件信息.所属中队";
                    policeTableName = "中队每日警员表";
                    fieldCode = "案件信息.所属中队代码";
                    field2 = "中队每日警员表.中队代码";
                }

                if (radioWeek.Checked)
                {
                    TimeSpan ts = new TimeSpan(7, 0, 0, 0);
                    DateTime tempTime = startTime + ts;
                    if (tempTime > endTime)
                    {
                        this.count++;
                        this.getWeekOneValue(startTime, endTime);
                    }
                    else
                    {
                        //执行有几个周期有函数
                        this.getWeekOneValue(startTime, tempTime);
                        DateTime time = tempTime;

                        for (; time < endTime; time = time + ts)
                        {
                            if (time + ts < endTime)
                            {
                                this.count++;
                                this.getAnotherValue(time, time + ts, "iCount" + count.ToString());
                            }
                        }
                        this.count++;
                        this.getAnotherValue(time - ts, endTime, "iCount" + count.ToString());
                        this.count++;
                    }
                }
                else
                {
                    //判断起始所在月份,有多少天就加多少天
                    GregorianCalendar gc = new GregorianCalendar();
                    int days = gc.GetDaysInMonth(startTime.Year, startTime.Month);
                    TimeSpan ts = new TimeSpan(days, 0, 0, 0);
                    DateTime tempTime = startTime + ts;
                    if (tempTime > endTime)
                    {
                        this.count++;
                        this.getWeekOneValue(startTime, endTime);
                    }
                    else
                    {
                        //执行有几个周期有函数
                        this.getWeekOneValue(startTime, tempTime);
                        DateTime time = tempTime;

                        while (time < endTime)
                        {
                            days = gc.GetDaysInMonth(time.Year, time.Month);
                            ts = new TimeSpan(days, 0, 0, 0);
                            this.count++;
                            this.getAnotherValue(time, time + ts, "iCount" + count.ToString());
                            time += ts;
                        }
                        this.count++;
                        this.getAnotherValue(time - ts, endTime, "iCount" + count.ToString());
                    }
                }

                this.OpenOracleSpatialTable(strLayer);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clCXTJ-ucCXTJ-getValueCycle");
            }
        }

        private void getWeekOneValue(DateTime startTime, DateTime endTime)
        {
            OracleConnection Conn = new OracleConnection(ConnStr);
            try
            {
                //string strExp = "select   " + field + " as  name  , substr(to_char(count(*) / avg(" + policeTableName + ".警员数量)),1,5)   as  iCount  from  案件信息," + policeTableName + "  where  发案时间初值>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and  " + fieldCode + "　= " + field2;
                string strExp = "";
                if (radioJingzong.Checked)
                {
                    //strExp += " and 案件来源='警综'";
                    strExp = "select   " + field + " as  name  , substr(to_char(count(*) / avg(" + policeTableName + ".警员数量)),1,5)   as  iCount  from  案件信息," + policeTableName + "  where  发案时间初值>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and  " + fieldCode + "　= " + field2;
                }
                else
                {
                    //strExp += " and 案件来源='110'";
                    strExp = "select   " + field + " as  name  , substr(to_char(count(*) / avg(" + policeTableName + ".警员数量)),1,5)   as  iCount  from  gps110.报警信息110," + policeTableName + "  where  发案时间初值>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and  " + fieldCode + "　= " + field2;
                
                }
                if (radioZD.Checked && comboBoxZD.Text != "全部") {
                    if (strRegion == "大良")
                    {
                        strExp += " and 所属派出所 in('大良','德胜')";
                    }
                    else
                    {
                        strExp += " and 所属派出所='" + comboBoxZD.Text.Trim() + "'";
                    }
                }
                strExp += "   group   by  " + field;

                Conn.Open();
                OracleCommand cmd = new OracleCommand(strExp, Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dataTempTable = ds.Tables[0];
                DataTable tempTable = new DataTable();
                this.dataTable = dataTempTable;
                cmd.Dispose();
                Conn.Close();
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                writeToLog(ex, "clCXTJ-ucCXTJ-getWeekOneValue");
            }
        }

        private void getAnotherValue( DateTime startTime, DateTime endTime,string colName )
        {
            OracleConnection Conn = new OracleConnection(ConnStr);
            try
            {
                //string strExp = "select   " + field + " as  name  , substr(to_char(count(*) / avg(" + policeTableName + ".警员数量)),1,5)   as  " + colName + "  from  案件信息," + policeTableName + "  where  发案时间初值>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and  " + fieldCode + "　= " + field2;
                string strExp = "";

                if (radioJingzong.Checked)
                {
                    //strExp += " and 案件来源='警综'";
                    strExp = "select   " + field + " as  name  , substr(to_char(count(*) / avg(" + policeTableName + ".警员数量)),1,5)   as  " + colName + "  from  案件信息," + policeTableName + "  where  发案时间初值>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and  " + fieldCode + "　= " + field2;
                }
                else
                {
                    //strExp += " and 案件来源='110'";
                    strExp = "select   " + field + " as  name  , substr(to_char(count(*) / avg(" + policeTableName + ".警员数量)),1,5)   as  " + colName + "  from  gps110.报警信息110," + policeTableName + "  where  发案时间初值>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期>=to_date('" + startTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 日期<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and  " + fieldCode + "　= " + field2;
                }
                if (radioZD.Checked && comboBoxZD.Text != "全部")
                {
                    if (strRegion == "大良")
                    {
                        strExp += " and 所属派出所 in('大良','德胜')";
                    }
                    else
                    {
                        strExp += " and 所属派出所='" + comboBoxZD.Text.Trim() + "'";
                    }
                }
                strExp += "   group   by  " + field;

                Conn.Open();
                OracleCommand cmd = new OracleCommand(strExp, Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dataTempTable = ds.Tables[0];
               
                dataTable.PrimaryKey = new DataColumn[] { this.dataTable.Columns["NAME"] };
                this.dataTable.Merge(dataTempTable);
                cmd.Dispose();
                Conn.Close();
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeToLog(ex, "clCXTJ-ucCXTJ-getAnotherValue");
            }
        }

        private Table queryTable = null;//定义一个全局表
        private FeatureLayer currentFeatureLayer = null;

        private void OpenOracleSpatialTable(string strLayer)
        {
            string strSQL = "Select  *  From " + strLayer;

            MIConnection miConnection = new MIConnection();
            try
            {
                TableInfoServer ti = new TableInfoServer(strLayer, ConnStr.Replace("data source = ", "SRVR=").Replace("user id = ", "UID=").Replace("password = ", "PWD="), strSQL, ServerToolkit.Oci);
              
                ti.CacheSettings.CacheType = CacheOption.Off;

                miConnection.Open();
                Table editTable = miConnection.Catalog.OpenTable(ti);

                this.createInfoMemLayer(editTable.TableInfo.Columns);

                string currentSql = "Insert into " + this.queryTable.Alias + "  Select * From " + editTable.Alias;//复制图元数据
                if (radioZD.Checked && comboBoxZD.Text != "全部") {
                    currentSql += " where 所属派出所='"+comboBoxZD.Text.Trim()+"'";
                }
                MapInfo.Data.MIConnection mapConnection = new MapInfo.Data.MIConnection();
                mapConnection.Open();
                MapInfo.Data.MICommand mapCommand = mapConnection.CreateCommand();
                mapCommand.CommandText = currentSql;
                mapCommand.ExecuteNonQuery();
                this.currentFeatureLayer = new MapInfo.Mapping.FeatureLayer(this.queryTable, "queryLayer");

                this.mapControl1.Map.Layers.Insert(0, (IMapLayer)currentFeatureLayer);
                editTable.Close();
                miConnection.Close();
            }
            catch (System.AccessViolationException ex)
            {
                if (miConnection.State == ConnectionState.Open)
                    miConnection.Close();
                writeToLog(ex, "clCXTJ-ucCXTJ-OpenOracleSpatialTable");
            }
        }

        /// <summary>
        /// 创建专题图表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="cols">列集合</param>
        private void createInfoMemLayer(Columns cols)
        {
            try
            {
                if (queryTable != null)
                {
                    this.queryTable.Close();
                }
                try
                {
                    Session.Current.Catalog.CloseTable("专题图表");
                }
                catch { }

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
                writeToLog(ex, "clCXTJ-ucCXTJ-createInfoMemLayer");
            }
        }

        /// <summary>
        /// 生成统计结果
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioZD.Checked&&comboBoxZD.Text.Trim()=="")
                {
                    MessageBox.Show("请选择中队所属派出所!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (dateFrom.Value >= dateTo.Value)
                {
                    MessageBox.Show("起始时间应小于终止时间,请重设!", "时间设置错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DateTime fromTime = dateFrom.Value.Date + timeFrom.Value.TimeOfDay;
                DateTime endTime = dateTo.Value.Date + timeTo.Value.TimeOfDay;

                if (fromTime > endTime)
                {
                    MessageBox.Show("时间选择错误！");
                    return;
                }

                TimeSpan tSpan = endTime - fromTime;

                if (radioWeek.Checked)
                {
                    if (tSpan.Days > 84)
                    {
                        MessageBox.Show("所选时间周期太长，请重设", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                else {
                    if (tSpan.Days > 366)
                    {
                        MessageBox.Show("所选时间周期太长，请重设", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                this.Cursor = Cursors.WaitCursor;
                this.getValueCycle(fromTime, endTime);
                this.createBarTheme();
                this.Cursor = Cursors.Default;
                this.doubleClick = true;

                //string sql = "select * from 案件信息 where 发案时间初值>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                string sql = "";

                if (radioJingzong.Checked)
                {
                    //sql += " and 案件来源='警综'";
                    sql = "select * from 案件信息 where 发案时间初值>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                }
                else
                {
                    //sql += " and 案件来源='110'";
                    sql = "select * from gps110.报警信息110 where 发案时间初值>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                }
                WriteEditLog(":"+sql, "成效统计");
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                writeToLog(ex, "clCXTJ-ucCXTJ-button1_Click");
            }    
        }

        private bool bindTableStatic(FeatureLayer lyr, string field, string content,DataTable dataTable)
        {
            try
            {
                    if (Session.Current.Catalog.GetTable("tempBindTable") != null)
                    {
                        Session.Current.Catalog.CloseTable("tempBindTable");
                    }
                    TableInfoAdoNet ti = new TableInfoAdoNet("tempBindTable");
                    ti.ReadOnly = false;
                    ti.DataTable = dataTable;
                    Table table = Session.Current.Catalog.OpenTable(ti);
                    Table targetTab = lyr.Table;

                    Columns col = new Columns();
                    col.Add(new Column("iCount", MIDbType.Int, "iCount"));
                    for (int i = 1; i < this.count; i++)
                    {
                        col.Add(new Column("iCount"+i.ToString(), MIDbType.Double, "iCount"+i.ToString()));
                    }
                    targetTab.AddColumns(col, BindType.DynamicCopy, table, "name", Operator.Equal, "NAME");
                    Session.Current.Catalog.CloseTable("tempBindTable");
                    return true;
               
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clCXTJ-ucCXTJ-bindTableStatic");
                return false;
            }
        }

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void createBarTheme()
        {
            try
            {
                FeatureLayer lyr = mapControl1.Map.Layers[0] as FeatureLayer;
                bool bindSuccess = bindTableStatic(lyr, field, "案件量", this.dataTable);
        
                if (bindSuccess == false)
                {
                    MessageBox.Show("绑定查询值到地图失败，不能创建专题图！");
                    return;
                }
                      
                MapControl mapMain = this.mapControl1;
                Table themeTable = lyr.Table;
                if (themeTable == null)
                {
                    MessageBox.Show("the table data is nonentity", "Theme", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);//制作专题图的表数据不存在
                    return;
                }
                //获取制作专题图的列
                ArrayList arrThemeCols = new ArrayList();
                TableInfo themeTableInfo = themeTable.TableInfo;
                if (themeTableInfo == null)
                {
                    MessageBox.Show("the table data is nonentity", "Theme", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);//制作专题图的表数据不存在
                    return;
                }

                foreach (Column col in themeTableInfo.Columns)
                {
                    if (col.ToString().IndexOf("iCount") > -1)
                    {
                        arrThemeCols.Add(col.Alias);
                    }
                }

                if (arrThemeCols.Count <= 0)
                {
                    MessageBox.Show("无关联列.", "专题图", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);//制作专题图的列数据不存在
                    return;
                }

                mapMain.Map.Scale = 80000;
                bool oo=false;
                int i = 0;
                while (oo == false)
                {
                    try
                    {
                        BarTheme theme = new BarTheme(mapMain.Map, themeTable, (string[])arrThemeCols.ToArray(typeof(string)));
                        oo = true;
                        ObjectThemeLayer themeLayer = new ObjectThemeLayer("Theme2", null, theme);
                        mapMain.Map.Layers.Add(themeLayer);
                        theme.DataValueAtSize /= 1;
                        theme.GraduateSizeBy = GraduateSizeBy.Constant;
                        themeLayer.RebuildTheme();
                    }
                    catch(Exception ex) {
                        i++;
                        if (i == 5) {
                            writeToLog(ex, "clCXTJ-ucCXTJ-createBarTheme");
                            break;//超过5次不出来,不做
                        } //
                    }
                }

                try
                {
                    mapControl1.Map.SetView(lyr);
                }
                catch { }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "clCXTJ-ucCXTJ-createBarTheme");
            }
        }


        /// <summary>
        /// 异常输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeToLog(Exception ex,string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine("成效统计:在 "+sFunc+" 方法中" + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="method">操作方式</param>
        private void WriteEditLog(string sql, string method)
        {
            OracleConnection Conn = new OracleConnection(ConnStr);
            try
            {
                Conn.Open();
                OracleCommand cmd;
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'案件分析:成效统计','案件信息" + sql.Replace('\'', '"') + "','" + method + "')";
                cmd = new OracleCommand(strExe, Conn);
                cmd.ExecuteNonQuery();

                Conn.Close();
            }
            catch
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }

        /// <summary>
        /// 是否是按中队统计
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void radioZD_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxZD.Enabled = radioZD.Checked;
        }
    }
}