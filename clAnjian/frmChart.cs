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
                writeAnjianLog(ex, "���캯��");
            }
        }

        public void LineDataStatic(string strConn, string sExp)
        {
            double[] dCounts = new double[24];
            double[] dLabels = new double[24];
            OracleConnection Conn = new OracleConnection(strConn);
            //�����ݿ�
            try
            {
                //�����ݿ�
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
                        //�����ֵ����ת����ʱ��,���� 
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

        //��ͷͼ
        private void CreateLineChart(double[] dDate,double[] dCount)
        {
            try
            {
                this.Text = "������ʱ��ͳ��";
                GraphPane myPane = zedGraphControl1.GraphPane;

                PointPairList list = new PointPairList();
                LineItem myCurve;
                myPane.Title.Text = "����ͳ�ƣ�����ʱ�䣩";
                myPane.XAxis.Title.Text = "ʱ��(ʱ)";
                myPane.YAxis.Title.Text = "����";
                myPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;

                list.Add(dDate, dCount);

                myCurve = myPane.AddCurve("������",
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

        //��״ͼ
        public void BarDataStatic(string strConn, string sExp)
        {
            double[] dCounts = null;
            string[] sLabels = null;
            OracleConnection Conn = new OracleConnection(strConn);

            //�����ݿ�
            try
            {
                //�����ݿ�
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

                //��ѯ������ȡ����ʱ���ֵ
                sExp = sExp.Replace("min(����ʱ���ֵ),max(����ʱ���ֵ)", "����ʱ���ֵ");
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
                        dRows = dt.Select("����ʱ���ֵ>='" + dtMin + "' and ����ʱ���ֵ<='" + dtMax + "'");
                    }
                    else
                    {
                        if (beginYear == dtMin.Year && beginMonth == dtMin.Month)
                        {
                            dRows = dt.Select("����ʱ���ֵ>='" + dtMin + "' and ����ʱ���ֵ<'" + d2 + "'");
                        }
                        else if (beginYear == endYear && beginMonth == endMonth)
                        {
                            dRows = dt.Select("����ʱ���ֵ>='" + d1 + "' and ����ʱ���ֵ<='" + dtMax + "'");
                        }
                        else
                        {
                            dRows = dt.Select("����ʱ���ֵ>='" + d1 + "' and ����ʱ���ֵ<'" + d2 + "'");
                        }
                    }
                    dCounts[j] = dRows.Length;
                    sLabels[j] = beginYear.ToString() + "��" + beginMonth.ToString() + "��";
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
                this.Text = "�������·�ͳ��";
                GraphPane myPane = zedGraphControl1.GraphPane;

                // ����ͼ���˵������
                myPane.Title.Text = "����ͳ�ƣ������·ݣ�";

                // ���ú������˵������
                myPane.XAxis.Title.Text = "����";

                // �����������˵������
                myPane.YAxis.Title.Text = "��������";

                // ����ÿ��bar
                BarItem myCurve = myPane.AddBar("������", null, dCounts, Color.Orange);

                //����bar����ɫ
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);

                // ���ñ�������ɫ�ͽ���ɫ
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // ����ÿ��bar��label,���е�2��������ʾ�Ƿ���ʾ��bar������λ�ã���3��������ʾlabel�����з���
                BarItem.CreateBarLabels(myPane, false, "f0");
                zedGraphControl1.IsShowPointValues = true;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex,"CreateBarChart");
            }
        }

        //��ͼ
        public string connString="";
        public string strExe="";
        public void PieDataStatic(string strConn, string sExp)
        {
            double[] dCounts = null;
            string[] sLabels = null;
            OracleConnection Conn = new OracleConnection(strConn);
            //�����ݿ�
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
                    MessageBox.Show("������������Ӧͳ�Ƽ�¼,������!","��ʾ",MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                dCounts = new double[dt.Rows.Count];
                sLabels = new string[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dCounts[i] = Convert.ToDouble(dt.Rows[i][0]);
                    sLabels[i] = dt.Rows[i][1].ToString();
                }
                CreatePieChart(dCounts, sLabels, "�������ͳ��ͼ");

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

        //��ͼͼ���б�
        private void listLabel(Color[] cCol, string[] sLs)
        {
            try
            {
                dataGridView1.Rows.Clear();
                for (int i = 0; i < cCol.Length; i++)
                {
                    dataGridView1.Rows.Add(1);
                    //dataGridView1.Rows[i].DefaultCellStyle.BackColor = cCol[i];
                    dataGridView1.Rows[i].Cells[0].Value = "��������";
                    dataGridView1.Rows[i].Cells[0].Style.ForeColor = cCol[i];
                    //dataGridView1.Rows[i].Cells[0].Style.Padding.Bottom = 3;
                    dataGridView1.Rows[i].Cells[1].Value = sLs[i] + "��" + bfb[i];
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
                //��������
                double dSum = 0;
                for (int i = 0; i < dCs.Length; i++) {
                    dSum += dCs[i];
                }

                //��������ٷֱ�
                bfb = new string[dCs.Length];
                double a=0d;
                for (int i = 0; i < dCs.Length; i++) { 
                    a=dCs[i]/dSum;
                    bfb[i] = a.ToString("0.0%");
                }

                this.Text = "���������ͳ��";
                GraphPane myPane = zedGraphControl1.GraphPane;
                myPane.CurveList.Clear();
                // ����ͼ��ı���ͱ������ʽ
                myPane.Title.Text = themeTitle;
                myPane.Title.FontSpec.Size = 20f;
                //myPane.Title.FontSpec.Family = "Times New Roman";
                // ���ñ���ɫ
                myPane.Fill = new Fill(Color.White, Color.Goldenrod, 45.0f);
                // ����ͼ�����ɫ��䣬�������ΪFillType.None,�����ɫ�ͱ���ɫ��ͬ
                myPane.Chart.Fill.Type = FillType.None;
                // ����ͼ���Ĵ�С��λ��
                myPane.Legend.Position = LegendPos.InsideTopRight;
                myPane.Legend.Location = new Location(0.95f, 0.15f, CoordType.PaneFraction,
                AlignH.Right, AlignV.Top);
                myPane.Legend.FontSpec.Size = 9.5f;
                myPane.Legend.IsHStack = false;
                myPane.Legend.IsVisible = false;

                /*
                * ���ñ�ͼ�ĸ�������
                * AddPieSlice�����Ĳ����� valueֵ, ��ɫ,����ɫ,�����С,�뿪���ĵ�ľ���,����
                */
                double d = 0;
                for (int i = 0; i < dCs.Length; i++)
                {
                    d = 0;
                    if (sLs[i] == label.Split('��')[0])
                    {
                        d = 0.1;
                    }
                    myPane.AddPieSlice(dCs[i],cCol[i], cCol[i], 45f, d, sLs[i]+"��"+bfb[i]);
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
                    createPie(curve.Label.Text, "�������ͳ��ͼ");

                    if (label == null)
                    {
                        label = new System.Windows.Forms.Label();
                    }
                    label.Text = curve.Label.Text;
                    label.ForeColor = Color.Red;
                    label.Location = new Point(e.X, e.Y);
                    label.AutoSize = true;
                    label.Font = new Font("����", 14, FontStyle.Bold);
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

        //����ר��ͼ�ϵĵ��,�ҵ�datagridview����Ӧ��¼
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
                comboBoxClass.Items.Add("��һ��");
                comboBoxClass.Items.Add("�ڶ���");
                comboBoxClass.Items.Add("������");

                comboBoxClass.Text = "��һ��";
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
                if (comboBoxClass.Text == "��һ��")
                {
                    comboBoxType.Enabled = false;
                }
                else if (comboBoxClass.Text == "�ڶ���")
                {
                    comboBoxType.Items.Clear();
                    string sExp = "select ����1 from ������Ϣ��� group by ����1";
                    //�����ݿ�
                    try
                    {
                        DataTable dt = new DataTable();
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);
                        comboBoxType.Items.Add("ȫ��");
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            comboBoxType.Items.Add(dt.Rows[i][0].ToString());
                        }
                        comboBoxType.Text = "ȫ��";
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
                    string sExp = "select ����2 from ������Ϣ��� group by ����2";
                    //�����ݿ�
                    try
                    {
                        DataTable dt = new DataTable();
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);
                        comboBoxType.Items.Add("ȫ��");
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            comboBoxType.Items.Add(dt.Rows[i][0].ToString());
                        }
                        comboBoxType.Text = "ȫ��";
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
                if (comboBoxClass.Text == "��һ��")
                {
                    PieDataStatic(connString, strExe);
                }
                else if (comboBoxClass.Text == "�ڶ���")
                {
                    string strExpress = "";

                    strExpress = strExe.Replace("����1", "����2");
                    if (comboBoxType.Text != "ȫ��")
                    {
                        if (strExpress.IndexOf("where") > -1)//���ԭ���ʽ��where����
                        {
                            strExpress = strExpress.Replace("where", "where ����1='" + comboBoxType.Text + "' and");
                        }
                        else
                        {
                            strExpress = strExpress.Replace("from ������Ϣ��� t", " from ������Ϣ��� where ����1='" + comboBoxType.Text + "'");
                        }
                    }
                    PieDataStatic(connString, strExpress);
                }
                else
                {
                    string strExpress = "";
                    strExpress = strExe.Replace("����1", "����3");
                    if (comboBoxType.Text != "ȫ��")
                    {
                        if (strExpress.IndexOf("where") > -1)//���ԭ���ʽ��where����
                        {
                            strExpress = strExpress.Replace("where", "where ����2='" + comboBoxType.Text + "' and");
                        }
                        else
                        {
                            strExpress = strExpress.Replace("from ������Ϣ��� t", " from ������Ϣ��� where ����2='" + comboBoxType.Text + "'");
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

        //������״ͼ
        public void CreateBarChart(double[] dCounts, string[] sLabels,string frmTitle, string themeTitle, string xTitle, string yTitle)
        {
            try
            {
                string[] labelStr = strSub(sLabels);

                this.Text = frmTitle;
                GraphPane myPane = zedGraphControl1.GraphPane;

                // ����ͼ���˵������
                myPane.Title.Text = themeTitle;

                // ���ú������˵������
                myPane.XAxis.Title.Text = xTitle;

                // �����������˵������
                myPane.YAxis.Title.Text = yTitle;

                // ����ÿ��bar
                BarItem myCurve = myPane.AddBar(yTitle, null, dCounts, Color.Orange);

                //����bar����ɫ
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);

                // ���ñ�������ɫ�ͽ���ɫ
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels;
                myPane.XAxis.Scale.FontSpec.IsItalic = false;
                myPane.XAxis.Scale.FontSpec.Size = 8f;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // ����ÿ��bar��label,���е�2��������ʾ�Ƿ���ʾ��bar������λ�ã���3��������ʾlabel�����з���
                BarItem.CreateBarLabels(myPane, false, "f0");
                zedGraphControl1.IsShowPointValues = true;
            }
            catch(Exception ex)
            {
                writeAnjianLog(ex,"CreateBarChart");
            }
        }

        /// <summary>
        /// Ϊ��ʵ��������״ͼʱ�����������ж���˷���
        /// </summary>
        /// <param name="slabel">Ҫ������ַ�</param>
        /// <returns>�������ַ�</returns>
        private string[] strSub(string[] slabel)
        {
            try
            {
                List<string> list = new List<string>();
                string lableStr = "";

                for (int i = 0; i < slabel.Length; i++)
                {
                    slabel[i] = slabel[i].Replace("�ɳ���", "").Replace("������", "");  // ��������̫����ȥ��Щ����Ҫ���ַ�
                    lableStr = "";

                    for (int j = 0; j < slabel[i].Length; j++)
                    {
                        lableStr += slabel[i].Substring(j, 1) + "\n\r";   // ÿ���ַ�������ϻ��з�
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
            sw.WriteLine("��������:�� frmChart." + sFunc + "������," + DateTime.Now.ToString() + ": ");
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
                createPie(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(), "�������ͳ��ͼ");
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
            //label.Font = new Font("����", 14, FontStyle.Bold);
            //label.BorderStyle = BorderStyle.FixedSingle;
            //label.BackColor = Color.LightSteelBlue;
            //zdGrap.Controls.Add(label);
        }
    }
}