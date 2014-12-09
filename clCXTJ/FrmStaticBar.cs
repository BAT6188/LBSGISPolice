using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.IO;

namespace clCXTJ
{
    public partial class FrmStaticBar : Form
    {
        public FrmStaticBar()
        {
            InitializeComponent();
            this.radioButton1.Checked = true;
        }


        private  double[] d;
        private string[] str;
        private string staTitleTime;
        private string strRegion;
        private string staYTitle;

        public void setParameter(double[] d, string[] str, string staTitleTime, string strRegion, string staYTitle)
        {
            this.d = d;
            this.str = str;
            this.staTitleTime = staTitleTime;
            this.strRegion = strRegion;
            this.staYTitle = staYTitle;
        }

        //创建柱状图
        public void CreateBarChart(double[] dCounts, string[] sLabels, string staTitleTime, string strRegion, string staYTitle)
        {
            try
            {
                this.panel1.Controls.Clear();
                ZedGraphControl zedGraphControl1 = new ZedGraphControl();
                this.panel1.Controls.Add(zedGraphControl1);
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
                myPane.XAxis.Type = AxisType.Text;
                zedGraphControl1.AxisChange();

                // 创建每条bar的label,其中第2个参数表示是否显示在bar的中心位置，第3个参数表示label的排列方向
                BarItem.CreateBarLabels(myPane, false, "0.00");
                zedGraphControl1.IsShowPointValues = true;
                zedGraphControl1.AxisChange();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateBarChart");
            }
        }


        public void CreateLineChart(double[] dCounts, string[] sLabels, string staTitleTime, string strRegion, string staYTitle)
        {
            try
            {
                this.panel1.Controls.Clear();
                ZedGraphControl zedGraphControl1 = new ZedGraphControl();
                this.panel1.Controls.Add(zedGraphControl1);
                zedGraphControl1.Dock = DockStyle.Fill;
                GraphPane myPane = zedGraphControl1.GraphPane;

                // 设置图表的说明文字
                myPane.Title.Text = staTitleTime;

                // 设置横坐标的说明文字
                myPane.XAxis.Title.Text = strRegion;

                // 设置纵坐标的说明文字
                myPane.YAxis.Title.Text = staYTitle;
             
                LineItem myCurve = myPane.AddCurve(staYTitle, null, dCounts, Color.Black, SymbolType.Circle);
                myCurve.Line.Fill = new Fill(Color.White, Color.LightSkyBlue, -45F);

                // 设置背景的颜色和渐变色
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);
                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels;
                myPane.XAxis.Type = AxisType.Text;
               
                // 创建每条bar的label,其中第2个参数表示是否显示在bar的中心位置，第3个参数表示label的排列方向
                BarItem.CreateBarLabels(myPane, false, "0.00");
                zedGraphControl1.IsShowPointValues = true;
                zedGraphControl1.AxisChange();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateLineChart");
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                this.CreateBarChart(d, str, staTitleTime, strRegion, staYTitle);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                this.CreateLineChart(d, str, staTitleTime, strRegion, staYTitle);
            }
        }

        private void writeToLog(Exception ex, string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine("成效统计:在 FrmStaticBar." + sFunc + " 方法中" + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}