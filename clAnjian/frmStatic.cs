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
    public partial class frmStatic : Form
    {
        public string strConn = "";
        public string strRegion = "";
        public frmStatic()
        {
            InitializeComponent();
        }

        private void frmStatic_Load(object sender, EventArgs e)
        {
            try
            {
                //初始化时间选项
                DateTime dt = DateTime.Now;
                int year = dt.Year;
                int month = dt.Month;
                comboYear.Items.Clear();
                comboYear2.Items.Clear();

                if (month == 1)
                {
                    year--;
                    month = 12;
                }
                //填充年份下拉框，从1991到今年
                for (int i = year; i > 1990; i--)
                {
                    comboYear.Items.Add(i);
                    comboYear2.Items.Add(i);
                }
                comboYear.Text = year.ToString();    //当前年
                comboYear2.Text = year.ToString();

                //初始化月份
                comboMonth.Items.Clear();
                for (int i = 1; i < month; i++)
                {
                    comboMonth.Items.Add(i);
                }
                comboMonth.Text = Convert.ToString(month - 1);   //前一月

                if (dt.Month == 1)
                {
                    comboMonth.Items.Add(12);
                    comboMonth.Text = Convert.ToString(12);
                }

                //日期从本月1号至今
                dateTimeBegin.Value = Convert.ToDateTime(dt.Year.ToString() + "-" + dt.Month.ToString() + "-01");
                dateTimeEnd.Value = dt;

                radioNumCase.Click += new EventHandler(radioStaMothod_Click);
                radioTongbi.Click += new EventHandler(radioStaMothod_Click);
                comboYear2.SelectedValueChanged += new EventHandler(comboYear2_SelectedValueChanged);
                if (comboRegion.Text == "派出所")
                {
                    setStaValuesAndCreateBar();
                }
                else {
                    //getPaichusuo();
                    //comboBoxHighLevel.Text = strRegion;
                    //comboBoxHighLevel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"frmStatic_Load");
            }
        }

        //选择年份时，如果是往年，月下拉框中12个月，如果是今年，从1月到上月。
        void comboYear2_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboYear2.Text == DateTime.Now.Year.ToString())
                {
                    comboMonth.Items.Clear();
                    for (int i = 1; i < DateTime.Now.Month; i++)
                    {
                        comboMonth.Items.Add(i);
                    }
                    comboMonth.Text = "1";
                }
                else
                {
                    comboMonth.Items.Clear();
                    for (int i = 1; i < 13; i++)
                    {
                        comboMonth.Items.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"comboYear2_SelectedValueChanged");
            }
        }

        void radioStaMothod_Click(object sender, EventArgs e)
        {
            setStaValuesAndCreateBar();
        }

        private void buttonReCreate_Click(object sender, EventArgs e)
        {
            setStaValuesAndCreateBar();
        }

        //根据选项得到统计值
        private void setStaValuesAndCreateBar()
        {

            //string staRegion = "发案地点_街道";
            string staRegion = "所属派出所";
            //string staZhibiao = "";
            string staTime = "";
            string staPreTime = "";
            string staPreTimeHuanbi = "";

            string strExp = "";
            string strExp1 = "";

            string staTitleTime = "";
            string staTitle = "案件统计";
            string staYTitle = "";

            if (comboRegion.Text == "派出所")
            {
                //staRegion = "发案地点_街道";
                staRegion = "所属派出所";
            }
            else if (comboRegion.Text == "中队")
            {
                staRegion = "所属中队";
            }
            else
            {//警务室
                staRegion = "所属警务室";
            }
            try
            {
                //以年为统计基准
                if (radioYear.Checked)
                {
                    int year = Convert.ToInt16(comboYear.Text);
                    //选定时间段
                    staTime = "发案时间初值>=to_date('" + year.ToString() + "-01-01','yyyy-mm-dd hh24:mi:ss') ";
                    staTime += " and 发案时间初值<=to_date('" + year.ToString() + "-12-31 23:59:59','yyyy-mm-dd hh24:mi:ss')";

                    //去年此时间段（同比）
                    staPreTime = "发案时间初值>=to_date('" + Convert.ToString(year - 1) + "-01-01','yyyy-mm-dd hh24:mi:ss') ";
                    staPreTime += " and 发案时间初值<=to_date('" + Convert.ToString(year - 1) + "-12-31 23:59:59','yyyy-mm-dd hh24:mi:ss')";

                    //上一个此时间段（环比）
                    staPreTimeHuanbi = "发案时间初值>=to_date('" + Convert.ToString(year - 1) + "-01-01','yyyy-mm-dd hh24:mi:ss') ";
                    staPreTimeHuanbi += " and 发案时间初值<=to_date('" + Convert.ToString(year - 1) + "-12-31 23:59:59','yyyy-mm-dd hh24:mi:ss')";

                    staTitleTime = comboYear.Text + "年";
                }
                //以月为统计基准
                else if (radioMonth.Checked)
                {
                    int year = Convert.ToInt16(comboYear2.Text);
                    int month = Convert.ToInt16(comboMonth.Text);
                    staTime = "发案时间初值>=to_date('" + year.ToString() + "-" + month.ToString() + "-01','yyyy-mm-dd hh24:mi:ss') ";
                    if (month == 12)
                    {
                        staTime += " and 发案时间初值<to_date('" + Convert.ToString(year + 1) + "-01-01','yyyy-mm-dd hh24:mi:ss')";
                    }
                    else
                    {
                        staTime += " and 发案时间初值<to_date('" + year.ToString() + "-" + Convert.ToString(month + 1) + "-01','yyyy-mm-dd hh24:mi:ss')";
                    }

                    //去年此时间段（同比）
                    staPreTime = "发案时间初值>=to_date('" + Convert.ToString(year - 1) + "-" + month.ToString() + "-01','yyyy-mm-dd hh24:mi:ss') ";
                    if (month == 12)
                    {
                        staPreTime += " and 发案时间初值<to_date('" + Convert.ToString(year + 1) + "-01-01','yyyy-mm-dd hh24:mi:ss')";
                    }
                    else
                    {
                        staPreTime += " and 发案时间初值<to_date('" + Convert.ToString(year - 1) + "-" + Convert.ToString(month + 1) + "-01','yyyy-mm-dd hh24:mi:ss')";
                    }

                    //上一个此时间段（环比）
                    //如果是一月，环比对去年12月
                    if (month == 1)
                    {
                        staPreTimeHuanbi = "发案时间初值>=to_date('" + Convert.ToString(year - 1) + "-12-01','yyyy-mm-dd hh24:mi:ss') ";
                    }
                    else
                    {
                        staPreTimeHuanbi = "发案时间初值>=to_date('" + year.ToString() + "-" + Convert.ToString(month - 1) + "-01','yyyy-mm-dd hh24:mi:ss') ";
                    }
                    staPreTimeHuanbi += " and 发案时间初值<to_date('" + year.ToString() + "-" + month.ToString() + "-01','yyyy-mm-dd hh24:mi:ss')";

                    staTitleTime = comboYear2.Text + "年 " + comboMonth.Text + "月";
                }
                //自定义时间段
                else
                {
                    staTime = "发案时间初值>=to_date('" + dateTimeBegin.Value.ToString() + "','yyyy-mm-dd hh24:mi:ss') ";
                    staTime += " and 发案时间初值<=to_date('" + dateTimeEnd.Value.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                }
                strExp = "select " + staRegion + ",count(*) from 案件信息 where " + staTime;

                if (comboRegion.Text == "派出所")
                {
                    if (strRegion != "顺德区")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                        {
                            strRegion = strRegion.Replace("大良", "大良,德胜");
                        }

                        strExp += " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else if (comboRegion.Text == "中队")
                {
                    if (strRegion != "顺德区")
                    {
                        if (comboBoxHighLevel.Text == "全部")
                        {
                            strExp += " and 所属中队 in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strExp += " and 所属中队 like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                    else
                    {
                        if (comboBoxHighLevel.Text != "全部")
                        {
                            strExp += " and 所属中队　like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                }
                else
                {
                    strExp += " and 所属派出所='" + comboBoxHighLevel.Text + "'";
                }

                //添加案件来源条件
                if (radioJingzong.Checked)
                {
                    strExp += " and 案件来源='警综'";
                }
                else
                {
                    strExp += " and 案件来源='110'";
                }

                //添加案件类型条件
                if (radioXS.Checked) {
                    strExp += " and 案件类型='刑事'";
                }
                else if (radioZA.Checked)
                {
                    strExp += " and 案件类型='行政'";
                }
                else { }
            }

            catch (Exception ex)
            {
                writeAnjianLog(ex,"setStaValuesAndCreateBar");
            }

            strExp += " group by " + staRegion + " order by " + staRegion;
            OracleConnection conn = new OracleConnection(strConn);
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(strExp, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);

                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];

                double[] dCounts = new double[dt.Rows.Count];
                string[] sLabels = new string[dt.Rows.Count];
                if (radioNumCase.Checked)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dCounts[i] = Convert.ToInt32(dt.Rows[i][1]);
                        sLabels[i] = dt.Rows[i][0].ToString();
                    }
                    staYTitle = "案件量";
                    CreateBarChart(dCounts, sLabels, staTitle, comboRegion.Text, staYTitle);  //画统计图
                }

                //如果是同比环比，计算上一期，计算环比值
                else
                {
                    double[] dCounts1 = new double[dt.Rows.Count];
                    string[] sLabels1 = new string[dt.Rows.Count];
                    strExp1 = "select " + staRegion + ",count(*) from 案件信息 where " + staPreTime;

                    if (comboRegion.Text == "派出所")
                    {
                        if (strRegion != "顺德区")
                        {
                            if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                            {
                                strRegion = strRegion.Replace("大良", "大良,德胜");
                            }

                            strExp1 += " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        }
                    }
                    else if (comboRegion.Text == "中队")
                    {
                        if (comboBoxHighLevel.Text == "全部")
                        {
                            strExp1 += " and 所属中队 in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strExp1 += " and 所属中队 like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                    else
                    {
                        strExp1 += " and 所属派出所='" + comboBoxHighLevel.Text + "'";
                    }

                    //案件来源
                    if (radioJingzong.Checked)
                    {
                        strExp1 += " and 案件来源='警综'";
                    }
                    else
                    {
                        strExp1 += " and 案件来源='110'";
                    }

                    strExp1 += " group by " + staRegion + " order by " + staRegion;

                    cmd = new OracleCommand(strExp1, conn);
                    dAdapter = new OracleDataAdapter(cmd);
                    ds = new DataSet();
                    dAdapter.Fill(ds);
                    DataTable dt1 = ds.Tables[0];
                    string rName = "";
                    int preValue = 0;
                    bool isExist = false;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        rName = dt.Rows[i][0].ToString();
                        isExist = false;
                        for (int j = 0; j < dt1.Rows.Count; j++)
                        {
                            if (dt1.Rows[j][0].ToString() == rName)
                            {
                                preValue = Convert.ToInt32(dt1.Rows[j][1]);
                                isExist = true;
                                break;
                            }
                        }
                        if (isExist == false)
                        {
                            preValue = 0;
                        }
                        dCounts1[i] = (Convert.ToInt32(dt.Rows[i][1]) - preValue) / Convert.ToDouble(dt.Rows[i][1]);
                        dCounts1[i] = Convert.ToDouble(dCounts1[i].ToString("0.00"));
                        sLabels1[i] = dt.Rows[i][0].ToString();
                    }

                    double[] dCounts2 = new double[dt.Rows.Count];
                    string[] sLabels2 = new string[dt.Rows.Count];
                    strExp1 = "select " + staRegion + ",count(*) from 案件信息 where " + staPreTimeHuanbi;

                    if (comboRegion.Text == "派出所")
                    {
                        if (strRegion != "顺德区")
                        {
                            if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                            {
                                strRegion = strRegion.Replace("大良", "大良,德胜");
                            }

                            strExp1 += " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        }
                    }
                    else if (comboRegion.Text == "中队")
                    {
                        if (comboBoxHighLevel.Text == "全部")
                        {
                            strExp1 += " and 所属中队 in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strExp1 += " and 所属中队 like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                    else
                    {
                        strExp1 += " and 所属派出所='" + comboBoxHighLevel.Text + "'";
                    }

                    if (radioJingzong.Checked)
                    {
                        strExp1 += " and 案件来源='警综'";
                    }
                    else
                    {
                        strExp1 += " and 案件来源='110'";
                    }

                    strExp1 += " group by " + staRegion + " order by " + staRegion;
                    cmd = new OracleCommand(strExp1, conn);
                    dAdapter = new OracleDataAdapter(cmd);
                    ds = new DataSet();
                    dAdapter.Fill(ds);
                    dt1 = ds.Tables[0];

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        rName = dt.Rows[i][0].ToString();
                        isExist = false;
                        for (int j = 0; j < dt1.Rows.Count; j++)
                        {
                            if (dt1.Rows[j][0].ToString() == rName)
                            {
                                preValue = Convert.ToInt32(dt1.Rows[j][1]);
                                isExist = true;
                                break;
                            }
                        }
                        if (isExist == false)
                        {
                            preValue = 0;
                        }
                        dCounts2[i] = (Convert.ToInt32(dt.Rows[i][1]) - preValue) / Convert.ToDouble(dt.Rows[i][1]);
                        dCounts2[i] = Convert.ToDouble(dCounts2[i].ToString("0.00"));
                    }
                    staTitle = "案件统计(" + staTitleTime + "各" + comboRegion.Text + "增长)";
                    staYTitle = "增长量";
                    CreateTwoBarChart(dCounts1, sLabels1, dCounts2, staTitle, comboRegion.Text, staYTitle);   //画统计图
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
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
                    slabel[i] = slabel[i].Replace("派出所", "").Replace("社区民警", "");  // 由于名称太长，去掉些不必的字符
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

        private void CreateTwoBarChart(double[] dCounts1, string[] sLabels1, double[] dCounts2, string staTitleTime, string strRegion, string staYTitle)
        {
            try
            {
                sLabels1 = strSub(sLabels1);   // 处理字符串数组让其竖着显示
                panel3.Controls.Clear();
                ZedGraphControl zedGraphControl1 = new ZedGraphControl();
                panel3.Controls.Add(zedGraphControl1);
                zedGraphControl1.Dock = DockStyle.Fill;
                GraphPane myPane = zedGraphControl1.GraphPane;

                // 设置图表的说明文字
                myPane.Title.Text = staTitleTime;

                // 设置横坐标的说明文字
                myPane.XAxis.Title.Text = strRegion;

                // 设置纵坐标的说明文字
                myPane.YAxis.Title.Text = staYTitle;

                // 创建每个bar
                BarItem myCurve = myPane.AddBar("同比增长", null, dCounts1, Color.Orange);
                BarItem myCurve2 = myPane.AddBar("环比增长", null, dCounts2, Color.Green);

                //设置bar的颜色
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);
                myCurve2.Bar.Fill = new Fill(Color.Green, Color.White, Color.Green);

                // 设置背景的颜色和渐变色
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels1;
                myPane.XAxis.Scale.FontSpec.IsItalic = false;
                myPane.XAxis.Scale.FontSpec.Size = 8f;
                //myPane.XAxis.Scale.FontSpec.Angle = 90;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // 创建每条bar的label,其中第2个参数表示是否显示在bar的中心位置，第3个参数表示label的排列方向
                // BarItem.CreateBarLabels(myPane, false, "f1");
                string sFormat = "00%";
                if (radioNumCase.Checked)
                {
                    sFormat = "f0";
                }
                BarItem.CreateBarLabels(myPane, false, sFormat);
                zedGraphControl1.IsShowPointValues = true;
                zedGraphControl1.Refresh();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"CreateTwoBarChart");
            }
        }

        //创建柱状图
        public void CreateBarChart(double[] dCounts, string[] sLabels, string staTitleTime,string strRegion,string staYTitle)
        {
            try
            {
                sLabels = strSub(sLabels);   // 处理字符串数组让其竖着显示
                panel3.Controls.Clear();
                ZedGraphControl zedGraphControl1 = new ZedGraphControl();
                panel3.Controls.Add(zedGraphControl1);
                zedGraphControl1.Dock = DockStyle.Fill;
                GraphPane myPane = zedGraphControl1.GraphPane;

                // 设置图表的说明文字
                myPane.Title.Text = staTitleTime;

                // 设置横坐标的说明文字
                myPane.XAxis.Title.Text = strRegion;

                // 设置纵坐标的说明文字
                myPane.YAxis.Title.Text = staYTitle;

                // 创建每个bar
                BarItem myCurve = myPane.AddBar(staYTitle, null, dCounts, Color.Orange);

                //设置bar的颜色
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);

                // 设置背景的颜色和渐变色
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels;
                myPane.XAxis.Scale.FontSpec.IsItalic = false;
                myPane.XAxis.Scale.FontSpec.Size = 8f;
                //myPane.XAxis.Scale.FontSpec.Angle = 90;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // 创建每条bar的label,其中第2个参数表示是否显示在bar的中心位置，第3个参数表示label的排列方向
                // BarItem.CreateBarLabels(myPane, false, "f1");
                string sFormat = "00%";
                if (radioNumCase.Checked)
                {
                    sFormat = "f0";
                }
                BarItem.CreateBarLabels(myPane, false, sFormat);

                zedGraphControl1.IsShowPointValues = true;

                zedGraphControl1.Refresh();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"CreateBarChart");
            }
        }

        private void comboRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboBoxHighLevel.Items.Clear();
                if (comboRegion.Text == "中队")
                {
                    //if (strRegion != "" && strRegion != "顺德区")
                    //{
                    //    comboBoxHighLevel.Enabled = false;
                    //}
                    //else {
                    comboBoxHighLevel.Enabled = true;
                    getPaichusuo();
                    //}
                }
                else if (comboRegion.Text == "警务室")
                {
                    comboBoxHighLevel.Enabled = true;
                    getZhongdui();
                }
                else
                {
                    comboBoxHighLevel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "comboRegion_SelectedIndexChanged");
            }
        }

        private void getPaichusuo()
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "select 派出所名 from 基层派出所";
                if (strRegion != "顺德区")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    cmd.CommandText += " where 派出所名 in ('" + strRegion.Replace(",", "','") + "')";
                }
                comboBoxHighLevel.Items.Add("全部");
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        comboBoxHighLevel.Items.Add(dr.GetValue(0).ToString().Trim());
                    }
                    comboBoxHighLevel.Text = comboBoxHighLevel.Items[0].ToString();
                }
                Conn.Close();
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex,"getPaichusuo");
            }
        }

        private void getZhongdui()
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();

                if (strRegion != "顺德区")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    cmd.CommandText = "select 中队名 from 基层民警中队 where 所属派出所 in ('" + strRegion.Replace(",", "','") + "') order by 中队名";
                }
                else
                {
                    cmd.CommandText = "select 中队名 from 基层民警中队 order by 中队名";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        comboBoxHighLevel.Items.Add(dr.GetValue(0).ToString().Trim());
                    }
                    comboBoxHighLevel.Text = comboBoxHighLevel.Items[0].ToString();
                }
                
                Conn.Close();
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex,"getZhongdui");
            }
        }

        private void writeAnjianLog(Exception ex,string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine("案件分析:在 frmStatic." + sFunc + "方法中," + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

    }
}