using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MapInfo.Windows.Controls;
using MapInfo.Data;
using MapInfo.Engine;
using MapInfo.Mapping;
using MapInfo.Styles;
using System.Reflection;
using System.Data.OracleClient;
using System.IO;

namespace cl3Color
{
    public partial class uc3Color : UserControl
    {
        private MapControl mapControl1;
        private string strConn = "";
        public string user = "";
        public string strRegion = "";
        public uc3Color(MapControl m,string s)
        {
            InitializeComponent();
            mapControl1 = m;
            strConn = s;
            setDefaultDate();
        }

        /// <summary>
        /// 设置默认值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void setDefaultDate()
        {
            try
            {
                DateTime dtNow = DateTime.Now;
                int iDays = Convert.ToInt16(dtNow.DayOfWeek);
                TimeSpan ts1 = new TimeSpan((iDays + 7), 0, 0, 0);
                dateFrom.Value = dtNow.Subtract(ts1);
            }
            catch(Exception ex) {
                write3ColorLog(ex, "setDefaultDate");
            }
        }

        /// <summary>
        /// 警情严重颜色设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void btnColor1_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    btnColor1.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "btnColor1_Click");
            }
        }

        /// <summary>
        /// 警情小幅上升颜色设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void btnColor2_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    btnColor2.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "btnColor2_Click");
            }
        }

        /// <summary>
        /// 警情下降颜色设置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void btnColor3_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    btnColor3.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "btnColor3_Click");
            }
        }

        /// <summary>
        /// 生成预警信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioZD.Checked && comboBoxZD.Text.Trim() == "")
            {
                MessageBox.Show("请选择中队所属派出所!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime fromTime = dateFrom.Value.Date;
            DateTime endTime;
            if (radioButton1W.Checked)        //一周
            {
                endTime = fromTime.AddDays(7);
            }
            else if (radioButton2W.Checked)     //两周
            {
                endTime = fromTime.AddDays(14);
            }
            else {    //一个月
                //endTime = new DateTime(fromTime.Year, (fromTime.Month + 1), fromTime.Day);
                endTime = fromTime.AddMonths(1);
            }

            if (endTime>DateTime.Now)
            {
                MessageBox.Show("终止日期超过现在日期,请重设!", "时间设置错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            try
            {
                int kk = 0;
                if (radioButton1W.Checked) {
                    kk = 0;
                }
                else if (radioButton2W.Checked) {
                    kk = 1;
                }
                else {
                    kk = 2;
                }
                clsCal3Color cl3 = new clsCal3Color(fromTime, endTime,radioJingzong.Checked,kk,radioPCS.Checked);
                //string[] strArr ={ "大良", "容桂", "伦教", "北滘", "陈村", "乐从", "龙江", "勒流", "杏坛", "均安" };

                string[] strArr = getPaichusuo();
                string tAlias = "";
                try {
                    if (mapControl1.Map.Layers["派出所"] != null)
                    {
                        this.mapControl1.Map.Layers.Remove("派出所");
                        Session.Current.Catalog.CloseTable("派出所");
                    }
                    if (mapControl1.Map.Layers["中队"] != null)
                    {
                        this.mapControl1.Map.Layers.Remove("中队");
                        Session.Current.Catalog.CloseTable("中队");
                    }
                    // add by fisher in 10-01-07   内存会莫名其妙地出错，所以再次关闭表！
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("民警中队辖区视图") != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("民警中队辖区视图");
                    }
                }
                catch { }
                if (radioPCS.Checked) {
                    tAlias = "派出所";

                    createMemTable("派出所辖区视图",tAlias);
                }
                else{
                    tAlias = "中队";
                    createMemTable("民警中队辖区视图",tAlias);
                    strArr = getZhongduiByName(comboBoxZD.Text.Trim());
                }

                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(1, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Gray);

                SimpleInterior simInterior;
                CompositeStyle comStyle;
                for (int i = 0; i < strArr.Length; i++)
                {
                    string name = strArr[i];
                    string whereClause = "";
                    if (radioPCS.Checked)
                    {
                        whereClause = tAlias + "代码='" + name + "'";
                    }
                    else {
                        whereClause = tAlias + "代码='" + name + "'";
                    }
                    Feature ft = Session.Current.Catalog.SearchForFeature(tAlias, MapInfo.Data.SearchInfoFactory.SearchWhere(whereClause));
                    if (ft == null)  // 如果为空，则忽略
                    {
                        continue;
                    }

                    cl3.isFudong = checkBoxFudong.Checked;
                    cl3.setParameter(name);

                    double iFudong = 0.1;
                    if (checkBoxFudong.Checked)
                    {
                       iFudong = getFudongValue(cl3.A1,cl3.A2,cl3.A3); //获取浮动值
                    }

                     if (cl3.A0 >= iFudong)
                    {
                        simInterior = new SimpleInterior(9, btnColor1.BackColor, System.Drawing.Color.Gray, false);
                        comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
                    }
                    else if (cl3.A0 <= 0.0)
                    {
                        simInterior = new SimpleInterior(9, btnColor3.BackColor, System.Drawing.Color.Gray, false);
                        comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
                    }
                    else
                    {
                        simInterior = new SimpleInterior(9, btnColor2.BackColor, System.Drawing.Color.Gray, false);
                        comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
                    }
                    ft.Style = comStyle;
                    if (string.Format("{0:0.0}", cl3.A0) == "非数字")
                    {
                        ft["预警值"] = "该周期内无案件";
                    }
                    else
                    {
                        ft["预警值"] = string.Format("{0:0.00}", cl3.A0*100) +"%";
                    }
                    ft.Update();
                    Application.DoEvents();
                }

                string sql="";
                if (radioJingzong.Checked)
                {
                    sql = "select * from 案件信息 where 发案时间初值>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and 案件来源='警综'";
                }
                else
                {
                    sql = "select * from gps110.报警信息110 where 发案时间初值>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and 案件来源='110'";
                }
                WriteEditLog(":"+sql, "生成预警图");
            }
            catch(Exception ex) {
                write3ColorLog(ex,"buttonOK_Click");
            }
            this.Cursor = Cursors.Default;
        }        

        /// <summary>
        /// 获得某个派出所下的所有中队名称及中队代码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="pcsName">派出所名</param>
        /// <returns>存有中队名称及中队代码的数组</returns>
        private string[] getZhongduiByName(string pcsName)
        {
            string a = "", b = "";
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                //基层派出所表中包含大队,所以设置查询条件
                if (radioJingzong.Checked)
                {
                    cmd.CommandText = "select 中队代码,中队名 from 基层民警中队 where 所属派出所='" + pcsName + "'";
                }
                else
                {
                    cmd.CommandText = "select 中队代码,中队名 from 中队110 where 所属派出所='" + pcsName + "'";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ////基层派出所中添加了三个大队,三色预警不需要
                    //if (dr.GetValue(0).ToString().Trim().IndexOf("大队") == -1)
                    //{
                    a += dr.GetValue(0).ToString().Trim() + ",";
                    b += dr.GetValue(1).ToString().Trim() + ",";
                    //}
                }
                Conn.Close(); 
                b = b.Remove(b.LastIndexOf(','));
                a = a.Remove(a.LastIndexOf(','));
                paiZhong = b.Split(',');
                return a.Split(',');
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                write3ColorLog(ex, "getZhongduiByName");
                return null;
            }
        }

        /// <summary>
        /// 在地图上生成预警值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="strLayer">表名</param>
        /// <param name="layer">图层名</param>
        private void createMemTable(string strLayer,string layer)
        {
            try
            {
                if (mapControl1.Map.Layers[layer] == null)
                {
                    MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable(layer);

                    string strSQL = "";
                    //string _conStr = "";

                    if (radioJingzong.Checked)
                    {
                        strSQL = "select  *  from " + strLayer;
                        //_conStr = strConn;
                    }
                    else
                    {
                        strSQL = "select  *  from " + strLayer + "110";
                        //_conStr = "data source =gis;user id =gps110;password =gps11012345;";
                    }

                    MIConnection miConnection = new MIConnection();

                    TableInfoServer ti = new TableInfoServer(strLayer, strConn.Replace("data source = ", "SRVR=").Replace("user id = ", "UID=").Replace("password = ", "PWD="), strSQL, ServerToolkit.Oci);
                    ti.CacheSettings.CacheType = CacheOption.Off;
                    miConnection.Open();
                    Table tempTable = miConnection.Catalog.OpenTable(ti);
                    miConnection.Close();  

                    foreach (MapInfo.Data.Column col in tempTable.TableInfo.Columns) //复制表结构
                    {
                        MapInfo.Data.Column col2 = col.Clone();

                        col2.ReadOnly = false;

                        mainMemTableInfo.Columns.Add(col2);
                    }
                    Column labelCol = new Column("预警值", MIDbType.String);
                    if (mainMemTableInfo.Columns.Contains(labelCol)==false)             // add by fisher in 10-01-07
                    {
                        mainMemTableInfo.Columns.Add(labelCol);
                    }

                    // add by fisher in 10-01-07
                    // 再次强行关闭“中队”，因为这里运行多次后会莫名其妙地内存出错
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("中队") != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("中队");
                    }

                    Table table = MapInfo.Engine.Session.Current.Catalog.CreateTable(mainMemTableInfo);

                    string currentSql = "Insert into " + table.Alias + "  Select *,'0' From " + tempTable.Alias;//复制图元数据
                    if (radioZD.Checked) {
                        currentSql += " where 所属派出所='"+comboBoxZD.Text.Trim()+"'";
                    }

                    miConnection.Open();
                    MICommand miCmd = miConnection.CreateCommand();
                    miCmd.CommandText = currentSql;
                    miCmd.ExecuteNonQuery();
                    miCmd.Dispose();
                    miConnection.Catalog.CloseTable(tempTable.Alias);
                    miConnection.Close();

                    IMapLayer mapLayer = mapControl1.Map.Layers["标注图层"];
                    int ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                    FeatureLayer fl = new FeatureLayer(table);
                    mapControl1.Map.Layers.Insert(ilayer + 1, fl);
                    try
                    {
                        mapControl1.Map.SetView(fl);
                    }
                    catch { }
                    labeLayer(fl.Table);
                }
            }
            catch(Exception ex) {
                write3ColorLog(ex,"createMemTable");
            }
        }     

        /// <summary>
        /// 添加标注图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="table">Table</param>
        private void labeLayer(Table table)
        {
            try
            {
                LabelLayer labelLayer = this.mapControl1.Map.Layers["标注图层"] as LabelLayer;

                LabelSource source = new LabelSource(table);

                source.DefaultLabelProperties.Caption = "预警值";
                source.DefaultLabelProperties.Layout.Offset = 4;

                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;

                labelLayer.Sources.Insert(0, source);
            }
            catch (Exception ex)
            {
                write3ColorLog(ex,"labeLayer");
            }
        }

        private string[] paiZhong;
        /// <summary>
        /// 生成预警统计表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (radioZD.Checked && comboBoxZD.Text.Trim() == "")
            {
                MessageBox.Show("请选择中队所属派出所!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime fromTime = dateFrom.Value.Date;
            DateTime endTime;
            if (radioButton1W.Checked)
            {
                endTime = fromTime.AddDays(7);
            }
            else if (radioButton2W.Checked)
            {
                endTime = fromTime.AddDays(14);
            }
            else
            {
                endTime = new DateTime(fromTime.Year, (fromTime.Month + 1), fromTime.Day);
            }

            if (endTime > DateTime.Now)
            {
                MessageBox.Show("终止日期超过现在日期,请重设!", "时间设置错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

           // Cursor.Current = Cursors.WaitCursor;


            try{
                Excel.Application xls_exp = new Excel.ApplicationClass();
                Excel._Workbook xls_book;
                if(radioPCS.Checked){
                    xls_book = xls_exp.Workbooks._Open(Application.StartupPath + @"\pcsModel.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                    , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
                else{
                    xls_book = xls_exp.Workbooks._Open(Application.StartupPath + @"\zdModel.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                     , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }

                //xls_book.Title =dateFrom.Value.ToLongDateString()+"至"+dateTo.Value.ToLongDateString() +" 案件统计分析表";
                Excel._Worksheet xls_sheet = (Excel.Worksheet)xls_book.ActiveSheet;
                Object oMissing = System.Reflection.Missing.Value;  //实例化对象时缺省参数 

                int kk = 0;
                if (radioButton1W.Checked)
                {
                    kk = 0;
                }
                else if (radioButton2W.Checked)
                {
                    kk = 1;
                }
                else
                {
                    kk = 2;
                }

                clsCal3Color cl3 = new clsCal3Color(fromTime, endTime,radioJingzong.Checked,kk,radioPCS.Checked);
     
                cl3.isFudong = checkBoxFudong.Checked;
                Excel.Range rng3;
                Application.DoEvents();
                //string[] strArrtem;
                string[] strArr;// { "全区", "大良", "容桂", "伦教", "北滘", "陈村", "乐从", "龙江", "勒流", "杏坛", "均安" };
                //if (radioPCS.Checked) {
                //    strArrtem = getPaichusuo();
                //    strArr=new string[strArrtem.Length+1];
                //    strArr[0] = "全区";
                //}

                //else {
                //    strArrtem = getZhongduiByName(comboBoxZD.Text.Trim());
                //    strArr=new string[strArrtem.Length+1];
                //    strArr[0] = "全镇(街)";
                //}
                if (radioPCS.Checked)
                {
                    strArr = getPaichusuo();
                }
                else {
                    strArr = getZhongduiByName(comboBoxZD.Text.Trim());
                }

                //for(int i=0;i<strArrtem.Length;i++){
                //    strArr[i+1]=strArrtem[i];
                //}
                string title = "";
                if (radioButton1M.Checked)
                {
                    title = "各镇(街)月警情三色预警分布表";
                    if (radioZD.Checked)
                    {
                       title= comboBoxZD.Text.Trim() + "各中队月警情三色预警分布表";
                    }
                }
                else {
                    title = "各镇(街)周警情三色预警分布表";
                    if (radioZD.Checked)
                    {
                        title = comboBoxZD.Text.Trim()+ "各中队周警情三色预警分布表";
                    }
                }

                if (radioJingzong.Checked)
                {
                    title=title.Replace("警情", "案件");
                    xls_sheet.Cells[3, 5] = "刑事案件";
                    xls_sheet.Cells[3, 14] = "行政案件";
                }
                else
                {
                    xls_sheet.Cells[3, 5] = "刑事警情";
                    xls_sheet.Cells[3, 14] = "行政警情";
                }
                
                xls_sheet.Cells[1, 1] = title;
                xls_sheet.Cells[2, 1] = "查询周期范围:" + fromTime.ToShortDateString() + "～" + endTime.ToShortDateString();
                xls_sheet.Cells[2, 8] = "出表时间：" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

                frmPro fPro = new frmPro();
                fPro.progressBar1.Value = 0;
                fPro.progressBar1.Maximum = strArr.Length;
                fPro.Show();
                Application.DoEvents();

                for (int i = 4; i < strArr.Length + 4; i++)
                {
                    string name = paiZhong[i - 4];
                    int iRow = i * 3 - 4;
                    xls_sheet.Cells[iRow, 1] = name;
                    //if (name == "全区" || name == "全镇(街)")
                    //{
                    //    continue;
                    //    //cl3.getData("",getCycleValue());
                    //}
                    //else
                    //{
                    cl3.getData(strArr[i-4]);
                    //}


                    xls_sheet.Cells[iRow, 5] = cl3.XAZS;
                    xls_sheet.Cells[iRow, 6] = cl3.DQQC;
                    xls_sheet.Cells[iRow, 7] = cl3.DMTC;
                    xls_sheet.Cells[iRow, 8] = cl3.LRDQ;
                    xls_sheet.Cells[iRow, 9] = cl3.FCQD;
                    xls_sheet.Cells[iRow, 10] = cl3.YBQD;
                    xls_sheet.Cells[iRow, 11] = cl3.QJ;
                    xls_sheet.Cells[iRow, 12] = cl3.SH;
                    xls_sheet.Cells[iRow, 13] = cl3.ZP;
                    xls_sheet.Cells[iRow, 14] = cl3.ZAZS;
                    xls_sheet.Cells[iRow, 15] = cl3.DB;
                    xls_sheet.Cells[iRow, 16] = cl3.DJDO;
                    xls_sheet.Cells[iRow, 17] = cl3.TQ;
                    xls_sheet.Cells[iRow, 18] = cl3.XD;
                    xls_sheet.Cells[iRow, 19] = cl3.ZS;

                    xls_sheet.Cells[iRow + 1, 5] = cl3.XAZS2;
                    xls_sheet.Cells[iRow + 1, 6] = cl3.DQQC2;
                    xls_sheet.Cells[iRow + 1, 7] = cl3.DMTC2;
                    xls_sheet.Cells[iRow + 1, 8] = cl3.LRDQ2;
                    xls_sheet.Cells[iRow + 1, 9] = cl3.FCQD2;
                    xls_sheet.Cells[iRow + 1, 10] = cl3.YBQD2;
                    xls_sheet.Cells[iRow + 1, 11] = cl3.QJ2;
                    xls_sheet.Cells[iRow + 1, 12] = cl3.SH2;
                    xls_sheet.Cells[iRow + 1, 13] = cl3.ZP2;
                    xls_sheet.Cells[iRow + 1, 14] = cl3.ZAZS2;
                    xls_sheet.Cells[iRow + 1, 15] = cl3.DB2;
                    xls_sheet.Cells[iRow + 1, 16] = cl3.DJDO2;
                    xls_sheet.Cells[iRow + 1, 17] = cl3.TQ2;
                    xls_sheet.Cells[iRow + 1, 18] = cl3.XD2;
                    xls_sheet.Cells[iRow + 1, 19] = cl3.ZS2;

                    rng3 = xls_sheet.get_Range("B" + iRow.ToString(), Missing.Value);

                    double iFudong = 0.1;
                    if (checkBoxFudong.Checked)
                    {
                        iFudong = getFudongValue(cl3.A1, cl3.A2, cl3.A3); //获取浮动值
                    }

                    if (cl3.A0 <= 0.0)
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(btnColor3.BackColor);
                    }
                    else if (cl3.A0 >= iFudong)
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(btnColor1.BackColor);
                    }
                    else
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(btnColor2.BackColor);
                    }
                    

                    if (fPro.Visible == false) {
                        xls_exp = null;
                        xls_book = null;
                        xls_sheet = null;
                        return;
                    }
                    fPro.progressBar1.Value += 1;
                    Application.DoEvents();
                }

                fPro.Close();
                xls_exp.Visible = true;
                int generation = 0;
                xls_exp.UserControl = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xls_exp);
                generation = System.GC.GetGeneration(xls_exp);

                xls_exp = null;
                xls_book = null;
                xls_sheet = null;
                string sql="";
                if (radioJingzong.Checked)
                {
                    sql = "select * from 案件信息 where 发案时间初值>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and 案件来源='警综'";
                }
                else
                {
                    sql = "select * from gps110.报警信息110 where 发案时间初值>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and 案件来源='110'";
                }
                WriteEditLog(":"+sql, "生成三色预警统计表");
            }
            catch
            {
                MessageBox.Show("未安装微软的Excel软件，本功能将不能执行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 得到派出所名称
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <returns>存储派出所名称集合</returns>
        private string[] getPaichusuo()
        {
            string a = "", b = "";
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                //基层派出所表中包含大队,所以设置查询条件
                if (radioJingzong.Checked)
                {
                    cmd.CommandText = "select 派出所代码,派出所名 from 基层派出所 where 派出所名 not like '%大队%'";
                }
                else
                {
                    cmd.CommandText = "select 派出所代码,派出所名 from 派出所110";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ////基层派出所中添加了三个大队,三色预警不需要
                    //if (dr.GetValue(0).ToString().Trim().IndexOf("大队") == -1)
                    //{
                    a += dr.GetValue(0).ToString().Trim() + ",";
                    b += dr.GetValue(1).ToString().Trim() + ",";
                    //}
                }
                Conn.Close();
                b = b.Remove(b.LastIndexOf(','));
                a = a.Remove(a.LastIndexOf(','));
                paiZhong = b.Split(',');
                return a.Split(',');
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                write3ColorLog(ex, "getPaichusuo");
                return null;
            }
        }

        /// <summary>
        /// 根据前三周期的预警值,调整本周期的预警信号起点值 
        /// 如果预警值小于0，增加浮动值1，如果比-10还小，再增加一个浮动值1
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="p_2"></param>
        /// <param name="p_3"></param>
        /// <param name="p_4"></param>
        /// <returns></returns>
        private double getFudongValue(double p_2, double p_3, double p_4)
        {
            try
            {
                double i = 10;
                if (p_2 <= 0)
                {
                    i++;
                }
                if (p_2 < -0.1)
                {
                    i++;
                }
                if (p_3 <= 0)
                {
                    i++;
                }
                if (p_3 < -0.1)
                {
                    i++;
                }
                if (p_4 <= 0)
                {
                    i++;
                }
                if (p_4 < -0.1)
                {
                    i++;
                }
                return i / 100;
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "getFudongValue");
                return 0;
            }
        }

        /// <summary>
        /// 异常日志记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void write3ColorLog(Exception ex,string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "cl3Color-uc3Color-" + sFunc);
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        /// <param name="sql">操作的sql语句</param>
        /// <param name="method">操作方式</param>
        private void WriteEditLog(string sql, string method)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd;
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'案件分析:三色预警','案件信息" + sql.Replace('\'', '"') + "','" + method + "')";
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
        /// 是否按中队统计
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-23
        /// </summary>
        private void radioZD_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxZD.Enabled = radioZD.Checked;
        }
    }
}