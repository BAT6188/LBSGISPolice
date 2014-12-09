using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using ZedGraph;
using System.IO;


namespace clAnjian
{
    public partial class frmChart : Form
    {
        public string chartType;
        public string[] conStr;

        public frmChart(string[] canStr)
        {
            try
            {
                InitializeComponent();
                conStr = canStr;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(canStr[0], canStr[1], canStr[2]);
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "构造函数");
            }
        }

        public void LineDataStatic(string strConn, string sExp)
        {
            double[] dCounts = new double[24];
            double[] dLabels = new double[24];
            OracleConnection Conn = new OracleConnection(strConn);
            //打开数据库
            try
            {
                //打开数据库
                Conn.Open();
                OracleCommand cmd = new OracleCommand(sExp, Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                Conn.Close();

                for (int i = 0; i < 24; i++)
                {
                    dLabels[i] = Convert.ToDouble(i);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        DateTime aDT = Convert.ToDateTime(dt.Rows[i][0]);
                        dCounts[aDT.Hour]++;
                    }
                    catch
                    {
                        //如果有值不能转换成时间,跳过 
                    }
                }

                CreateLineChart(dLabels, dCounts);
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex, "LineDataStatic");
            }
        }

        //线头图
        private void CreateLineChart(double[] dDate,double[] dCount)
        {
            try
            {
                this.Text = "按发案时间统计";
                GraphPane myPane = zedGraphControl1.GraphPane;

                PointPairList list = new PointPairList();
                LineItem myCurve;
                myPane.Title.Text = "案件统计（发案时间）";
                myPane.XAxis.Title.Text = "时间(时)";
                myPane.YAxis.Title.Text = "数量";
                myPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;

                list.Add(dDate, dCount);

                myCurve = myPane.AddCurve("案件数",
                        list, Color.Red, SymbolType.Diamond);
                myCurve.Line.IsSmooth = true;
                myCurve.Line.Fill = new Fill(Color.Green, Color.White, 90.0f);
                myCurve.Line.IsAntiAlias = true;
                myCurve.Symbol.Fill = new Fill(Color.White);
                // Make the Y axis scale red
                myPane.YAxis.Scale.FontSpec.FontColor = Color.Red;
                myPane.YAxis.Title.FontSpec.FontColor = Color.Red;

                myPane.YAxis.MajorGrid.IsVisible = true;
                myPane.XAxis.MajorGrid.IsVisible = true;
                // Fill the axis background with a gradient
                myPane.Chart.Fill = new Fill(Color.White, Color.Orange, 45.0f);

                zedGraphControl1.IsShowPointValues = true;
                zedGraphControl1.AxisChange();

            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"CreateLineChart");
            }
        }

        //柱状图
        public void BarDataStatic(string strConn, string sExp)
        {
            double[] dCounts = null;
            string[] sLabels = null;
            OracleConnection Conn = new OracleConnection(strConn);

            //打开数据库
            try
            {
                //打开数据库
                Conn.Open();
                OracleCommand cmd = new OracleCommand(sExp, Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];

                DateTime dtMin = Convert.ToDateTime(dt.Rows[0][0]);
                DateTime dtMax = Convert.ToDateTime(dt.Rows[0][1]);
                int beginYear = dtMin.Year;
                int endYear = dtMax.Year;
                int beginMonth = dtMin.Month;
                int endMonth = dtMax.Month;
                int iMonth = (endYear - beginYear) * 12 + endMonth - beginMonth + 1;
                dCounts = new double[iMonth];
                sLabels = new string[iMonth];

                //查询案件，取发案时间初值
                sExp = sExp.Replace("min(发案时间初值),max(发案时间初值)", "发案时间初值");
                cmd = new OracleCommand(sExp, Conn);
                dAdapter = new OracleDataAdapter(cmd);
                ds = new DataSet();
                dAdapter.Fill(ds);
                dt = ds.Tables[0];
                Conn.Close();

                int j = 0;
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
                    DataRow[] dRows = null;
                    if (dtMin.Year == dtMax.Year && dtMin.Month == dtMax.Month)
                    {
                        dRows = dt.Select("发案时间初值>='" + dtMin + "' and 发案时间初值<='" + dtMax + "'");
                    }
                    else
                    {
                        if (beginYear == dtMin.Year && beginMonth == dtMin.Month)
                        {
                            dRows = dt.Select("发案时间初值>='" + dtMin + "' and 发案时间初值<'" + d2 + "'");
                        }
                        else if (beginYear == endYear && beginMonth == endMonth)
                        {
                            dRows = dt.Select("发案时间初值>='" + d1 + "' and 发案时间初值<='" + dtMax + "'");
                        }
                        else
                        {
                            dRows = dt.Select("发案时间初值>='" + d1 + "' and 发案时间初值<'" + d2 + "'");
                        }
                    }
                    dCounts[j] = dRows.Length;
                    sLabels[j] = beginYear.ToString() + "年" + beginMonth.ToString() + "月";
                    j++;
                    beginMonth++;
                    if (beginMonth > 12)
                    {
                        beginYear++;
                        beginMonth = beginMonth - 12;
                    }
                    if (beginYear == endYear && beginMonth >= endMonth)
                    {
                        break;
                    }
                }

                CreateBarChart(dCounts, sLabels);
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex,"BarDataStatic");
            }
        }

        private void CreateBarChart(double[] dCounts,string[] sLabels)
        {
            try
            {
                this.Text = "按发案月份统计";
                GraphPane myPane = zedGraphControl1.GraphPane;

                // 设置图表的说明文字
                myPane.Title.Text = "案件统计（发案月份）";

                // 设置横坐标的说明文字
                myPane.XAxis.Title.Text = "日期";

                // 设置纵坐标的说明文字
                myPane.YAxis.Title.Text = "案件数量";

                // 创建每个bar
                BarItem myCurve = myPane.AddBar("案件数", null, dCounts, Color.Orange);

                //设置bar的颜色
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);

                // 设置背景的颜色和渐变色
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // 创建每条bar的label,其中第2个参数表示是否显示在bar的中心位置，第3个参数表示label的排列方向
                BarItem.CreateBarLabels(myPane, false, "f0");
                zedGraphControl1.IsShowPointValues = true;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"CreateBarChart");
            }
        }

        //饼图
        public string connString="";
        public string strExe="";
        public void PieDataStatic(string strConn, string sExp)
        {
            double[] dCounts = null;
            string[] sLabels = null;
            OracleConnection Conn = new OracleConnection(strConn);
            //打开数据库
            try
            {
                Conn.Open();
                OracleCommand cmd = new OracleCommand(sExp, Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                Conn.Close();
                if (dt.Rows.Count < 1) {
                    MessageBox.Show("所设条件无相应统计记录,请重设!","提示",MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                dCounts = new double[dt.Rows.Count];
                sLabels = new string[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dCounts[i] = Convert.ToDouble(dt.Rows[i][0]);
                    sLabels[i] = dt.Rows[i][1].ToString();
                }
                CreatePieChart(dCounts, sLabels, "案件类别统计图");

                listLabel(cCol, sLs);
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex,"PieDataStatic");
            }
        }

        double[] dCs;
        string[] sLs;
        Color[] cCol;
        public void CreatePieChart(double[] dCounts,string[] sLabels,string themeTitle)
        {
            try
            {
                dCs = dCounts;
                sLs = sLabels;
                cCol = new Color[dCounts.Length];
                Random ran = new Random();
                int iRed = 0, iGreen = 0, iBlue = 0;
                for (int i = 0; i < dCounts.Length; i++)
                {
                    iRed = ran.Next(1, 255);
                    iGreen = ran.Next(1, 255);
                    iBlue = ran.Next(1, 255);
                    cCol[i] = Color.FromArgb(iRed, iGreen, iBlue);
                }
                zedGraphControl1.MouseClick += new MouseEventHandler(zedGraphControl1_MouseClick);
                createPie("default", themeTitle);
            }
            catch (Exception ex) { writeAnjianLog(ex, "listLabel"); }
        }

        //饼图图例列表
        private void listLabel(Color[] cCol, string[] sLs)
        {
            try
            {
                dataGridView1.Rows.Clear();
                for (int i = 0; i < cCol.Length; i++)
                {
                    dataGridView1.Rows.Add(1);
                    //dataGridView1.Rows[i].DefaultCellStyle.BackColor = cCol[i];
                    dataGridView1.Rows[i].Cells[0].Value = "████";
                    dataGridView1.Rows[i].Cells[0].Style.ForeColor = cCol[i];
                    //dataGridView1.Rows[i].Cells[0].Style.Padding.Bottom = 3;
                    dataGridView1.Rows[i].Cells[1].Value = sLs[i] + "：" + bfb[i];
                }
                //dataGridView1.Columns[1].DefaultCellStyle.BackColor = Color.White;
                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[1];
            }
            catch (Exception ex) { writeAnjianLog(ex, "listLabel"); }
        }

        string[] bfb = null;
        private void createPie(string label, string themeTitle)
        {
            try
            {
                //计算总量
                double dSum = 0;
                for (int i = 0; i < dCs.Length; i++) {
                    dSum += dCs[i];
                }

                //计算各个百分比
                bfb = new string[dCs.Length];
                double a=0d;
                for (int i = 0; i < dCs.Length; i++) { 
                    a=dCs[i]/dSum;
                    bfb[i] = a.ToString("0.0%");
                }

                this.Text = "按案件类别统计";
                GraphPane myPane = zedGraphControl1.GraphPane;
                myPane.CurveList.Clear();
                // 设置图表的标题和标题的样式
                myPane.Title.Text = themeTitle;
                myPane.Title.FontSpec.Size = 20f;
                //myPane.Title.FontSpec.Family = "Times New Roman";
                // 设置背景色
                myPane.Fill = new Fill(Color.White, Color.Goldenrod, 45.0f);
                // 设置图表的颜色填充，如果设置为FillType.None,则填充色和背景色相同
                myPane.Chart.Fill.Type = FillType.None;
                // 设置图例的大小和位置
                myPane.Legend.Position = LegendPos.InsideTopRight;
                myPane.Legend.Location = new Location(0.95f, 0.15f, CoordType.PaneFraction,
                AlignH.Right, AlignV.Top);
                myPane.Legend.FontSpec.Size = 9.5f;
                myPane.Legend.IsHStack = false;
                myPane.Legend.IsVisible = false;

                /*
                * 设置饼图的各个部分
                * AddPieSlice方法的参数是 value值, 颜色,渐变色,渐变大小,离开中心点的距离,名称
                */
                double d = 0;
                for (int i = 0; i < dCs.Length; i++)
                {
                    d = 0;
                    if (sLs[i] == label.Split('：')[0])
                    {
                        d = 0.1;
                    }
                    myPane.AddPieSlice(dCs[i],cCol[i], cCol[i], 45f, d, sLs[i]+"："+bfb[i]);
                }
                
                zedGraphControl1.AxisChange();
                zedGraphControl1.IsShowPointValues = true;
                
                zedGraphControl1.Refresh();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"createPie");
            }
        }

        System.Windows.Forms.Label label = null;
        void zedGraphControl1_MouseClick(object sender, MouseEventArgs e)
        {
            int iPt;
            object nearestObj;
            ZedGraphControl zdGrap = (ZedGraphControl)sender;

            try
            {
                zdGrap.GraphPane.FindNearestObject(new Point(e.X, e.Y), this.CreateGraphics(), out nearestObj, out iPt);

                if (nearestObj is CurveItem && iPt >= 0)
                {
                    CurveItem curve = (CurveItem)nearestObj;
                    createPie(curve.Label.Text, "案件类别统计图");

                    if (label == null)
                    {
                        label = new System.Windows.Forms.Label();
                    }
                    label.Text = curve.Label.Text;
                    label.ForeColor = Color.Red;
                    label.Location = new Point(e.X, e.Y);
                    label.AutoSize = true;
                    label.Font = new Font("宋体", 14, FontStyle.Bold);
                    label.BorderStyle = BorderStyle.FixedSingle;
                    label.BackColor = Color.LightSteelBlue;
                    zdGrap.Controls.Add(label);

                    findCurrentCell(label.Text);
                }
            }
            catch (Exception ex) {
                writeAnjianLog(ex,"zedGraphControl1_MouseClick");
            }

        }

        //根据专题图上的点击,找到datagridview中相应记录
        private void findCurrentCell(string p)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[1].Value.ToString() == p) {
                        dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[1];
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "findCurrentCell");
            }
        }

        private void frmChart_Load(object sender, EventArgs e)
        {
            try
            {
                comboBoxClass.Items.Clear();
                comboBoxClass.Items.Add("第一类");
                comboBoxClass.Items.Add("第二类");
                comboBoxClass.Items.Add("第三类");

                comboBoxClass.Text = "第一类";
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmChart_Load");
            }
        }

        private void comboBoxClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxClass.Text == "第一类")
                {
                    comboBoxType.Enabled = false;
                }
                else if (comboBoxClass.Text == "第二类")
                {
                    comboBoxType.Items.Clear();
                    string sExp = "select 名称1 from 案件信息类别 group by 名称1";
                    //打开数据库
                    try
                    {
                        DataTable dt = new DataTable();
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);
                        comboBoxType.Items.Add("全部");
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            comboBoxType.Items.Add(dt.Rows[i][0].ToString());
                        }
                        comboBoxType.Text = "全部";
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }
                    comboBoxType.Enabled = true;
                }
                else
                {
                    comboBoxType.Items.Clear();
                    string sExp = "select 名称2 from 案件信息类别 group by 名称2";
                    //打开数据库
                    try
                    {
                        DataTable dt = new DataTable();
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);
                        comboBoxType.Items.Add("全部");
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            comboBoxType.Items.Add(dt.Rows[i][0].ToString());
                        }
                        comboBoxType.Text = "全部";
                    }
                    catch (Exception ex)
                    {
                        writeAnjianLog(ex, "comboBoxClass_SelectedIndexChanged");
                    }
                    comboBoxType.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "comboBoxClass_SelectedIndexChanged");
            }
        }

        //private bool isNewPie = true;
        private void buttonReCreate_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                zedGraphControl1.Controls.Remove(label);
                if (comboBoxClass.Text == "第一类")
                {
                    PieDataStatic(connString, strExe);
                }
                else if (comboBoxClass.Text == "第二类")
                {
                    string strExpress = "";

                    strExpress = strExe.Replace("名称1", "名称2");
                    if (comboBoxType.Text != "全部")
                    {
                        if (strExpress.IndexOf("where") > -1)//如果原表达式有where条件
                        {
                            strExpress = strExpress.Replace("where", "where 名称1='" + comboBoxType.Text + "' and");
                        }
                        else
                        {
                            strExpress = strExpress.Replace("from 案件信息类别 t", " from 案件信息类别 where 名称1='" + comboBoxType.Text + "'");
                        }
                    }
                    PieDataStatic(connString, strExpress);
                }
                else
                {
                    string strExpress = "";
                    strExpress = strExe.Replace("名称1", "名称3");
                    if (comboBoxType.Text != "全部")
                    {
                        if (strExpress.IndexOf("where") > -1)//如果原表达式有where条件
                        {
                            strExpress = strExpress.Replace("where", "where 名称2='" + comboBoxType.Text + "' and");
                        }
                        else
                        {
                            strExpress = strExpress.Replace("from 案件信息类别 t", " from 案件信息类别 where 名称2='" + comboBoxType.Text + "'");
                        }
                    }
                    PieDataStatic(connString, strExpress);
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "buttonReCreate_Click");
            }
        }

        //创建柱状图
        public void CreateBarChart(double[] dCounts, string[] sLabels,string frmTitle, string themeTitle, string xTitle, string yTitle)
        {
            try
            {
                string[] labelStr = strSub(sLabels);

                this.Text = frmTitle;
                GraphPane myPane = zedGraphControl1.GraphPane;

                // 设置图表的说明文字
                myPane.Title.Text = themeTitle;

                // 设置横坐标的说明文字
                myPane.XAxis.Title.Text = xTitle;

                // 设置纵坐标的说明文字
                myPane.YAxis.Title.Text = yTitle;

                // 创建每个bar
                BarItem myCurve = myPane.AddBar(yTitle, null, dCounts, Color.Orange);

                //设置bar的颜色
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);

                // 设置背景的颜色和渐变色
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels;
                myPane.XAxis.Scale.FontSpec.IsItalic = false;
                myPane.XAxis.Scale.FontSpec.Size = 8f;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // 创建每条bar的label,其中第2个参数表示是否显示在bar的中心位置，第3个参数表示label的排列方向
                BarItem.CreateBarLabels(myPane, false, "f0");
                zedGraphControl1.IsShowPointValues = true;
            }
            catch(Exception ex)
            {
                writeAnjianLog(ex,"CreateBarChart");
            }
        }

        /// <summary>
        /// 为了实现生成柱状图时字体竖着排列定义此方法
        /// </summary>
        /// <param name="slabel">要处理的字符</param>
        /// <returns>处理后的字符</returns>
        private string[] strSub(string[] slabel)
        {
            try
            {
                List<string> list = new List<string>();
                string lableStr = "";

                for (int i = 0; i < slabel.Length; i++)
                {
                    slabel[i] = slabel[i].Replace("派出所", "").Replace("社区民警", "");  // 由于名称太长，去掉些不必要的字符
                    lableStr = "";

                    for (int j = 0; j < slabel[i].Length; j++)
                    {
                        lableStr += slabel[i].Substring(j, 1) + "\n\r";   // 每个字符后面加上换行符
                    }
                    list.Add(lableStr);
                }

                string[] strlabel = new string[list.Count];

                for (int s = 0; s < list.Count; s++)
                {
                    strlabel[s] = list[s];
                }
                return strlabel;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "CreateTwoBarChart");
                return null;
            }
        }

        private void writeAnjianLog(object ex,string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine("案件分析:在 frmChart." + sFunc + "方法中," + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0) {
                    dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[1];
                }
                createPie(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(), "案件类别统计图");
                zedGraphControl1.Controls.Remove(label);
                label = null;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "dataGridView1_CellClick");
            }
            //if (label == null)
            //{
            //    label = new System.Windows.Forms.Label();
            //}
            //label.Text = curve.Label.Text;
            //label.ForeColor = Color.Red;
            //label.Location = new Point(e.X, e.Y);
            //label.AutoSize = true;
            //label.Font = new Font("宋体", 14, FontStyle.Bold);
            //label.BorderStyle = BorderStyle.FixedSingle;
            //label.BackColor = Color.LightSteelBlue;
            //zdGrap.Controls.Add(label);
        }
    }
}