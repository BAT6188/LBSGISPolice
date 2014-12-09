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
                //��ʼ��ʱ��ѡ��
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
                //�����������򣬴�1991������
                for (int i = year; i > 1990; i--)
                {
                    comboYear.Items.Add(i);
                    comboYear2.Items.Add(i);
                }
                comboYear.Text = year.ToString();    //��ǰ��
                comboYear2.Text = year.ToString();

                //��ʼ���·�
                comboMonth.Items.Clear();
                for (int i = 1; i < month; i++)
                {
                    comboMonth.Items.Add(i);
                }
                comboMonth.Text = Convert.ToString(month - 1);   //ǰһ��

                if (dt.Month == 1)
                {
                    comboMonth.Items.Add(12);
                    comboMonth.Text = Convert.ToString(12);
                }

                //���ڴӱ���1������
                dateTimeBegin.Value = Convert.ToDateTime(dt.Year.ToString() + "-" + dt.Month.ToString() + "-01");
                dateTimeEnd.Value = dt;

                radioNumCase.Click += new EventHandler(radioStaMothod_Click);
                radioTongbi.Click += new EventHandler(radioStaMothod_Click);
                comboYear2.SelectedValueChanged += new EventHandler(comboYear2_SelectedValueChanged);
                if (comboRegion.Text == "�ɳ���")
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

        //ѡ�����ʱ����������꣬����������12���£�����ǽ��꣬��1�µ����¡�
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

        //����ѡ��õ�ͳ��ֵ
        private void setStaValuesAndCreateBar()
        {

            //string staRegion = "�����ص�_�ֵ�";
            string staRegion = "�����ɳ���";
            //string staZhibiao = "";
            string staTime = "";
            string staPreTime = "";
            string staPreTimeHuanbi = "";

            string strExp = "";
            string strExp1 = "";

            string staTitleTime = "";
            string staTitle = "����ͳ��";
            string staYTitle = "";

            if (comboRegion.Text == "�ɳ���")
            {
                //staRegion = "�����ص�_�ֵ�";
                staRegion = "�����ɳ���";
            }
            else if (comboRegion.Text == "�ж�")
            {
                staRegion = "�����ж�";
            }
            else
            {//������
                staRegion = "����������";
            }
            try
            {
                //����Ϊͳ�ƻ�׼
                if (radioYear.Checked)
                {
                    int year = Convert.ToInt16(comboYear.Text);
                    //ѡ��ʱ���
                    staTime = "����ʱ���ֵ>=to_date('" + year.ToString() + "-01-01','yyyy-mm-dd hh24:mi:ss') ";
                    staTime += " and ����ʱ���ֵ<=to_date('" + year.ToString() + "-12-31 23:59:59','yyyy-mm-dd hh24:mi:ss')";

                    //ȥ���ʱ��Σ�ͬ�ȣ�
                    staPreTime = "����ʱ���ֵ>=to_date('" + Convert.ToString(year - 1) + "-01-01','yyyy-mm-dd hh24:mi:ss') ";
                    staPreTime += " and ����ʱ���ֵ<=to_date('" + Convert.ToString(year - 1) + "-12-31 23:59:59','yyyy-mm-dd hh24:mi:ss')";

                    //��һ����ʱ��Σ����ȣ�
                    staPreTimeHuanbi = "����ʱ���ֵ>=to_date('" + Convert.ToString(year - 1) + "-01-01','yyyy-mm-dd hh24:mi:ss') ";
                    staPreTimeHuanbi += " and ����ʱ���ֵ<=to_date('" + Convert.ToString(year - 1) + "-12-31 23:59:59','yyyy-mm-dd hh24:mi:ss')";

                    staTitleTime = comboYear.Text + "��";
                }
                //����Ϊͳ�ƻ�׼
                else if (radioMonth.Checked)
                {
                    int year = Convert.ToInt16(comboYear2.Text);
                    int month = Convert.ToInt16(comboMonth.Text);
                    staTime = "����ʱ���ֵ>=to_date('" + year.ToString() + "-" + month.ToString() + "-01','yyyy-mm-dd hh24:mi:ss') ";
                    if (month == 12)
                    {
                        staTime += " and ����ʱ���ֵ<to_date('" + Convert.ToString(year + 1) + "-01-01','yyyy-mm-dd hh24:mi:ss')";
                    }
                    else
                    {
                        staTime += " and ����ʱ���ֵ<to_date('" + year.ToString() + "-" + Convert.ToString(month + 1) + "-01','yyyy-mm-dd hh24:mi:ss')";
                    }

                    //ȥ���ʱ��Σ�ͬ�ȣ�
                    staPreTime = "����ʱ���ֵ>=to_date('" + Convert.ToString(year - 1) + "-" + month.ToString() + "-01','yyyy-mm-dd hh24:mi:ss') ";
                    if (month == 12)
                    {
                        staPreTime += " and ����ʱ���ֵ<to_date('" + Convert.ToString(year + 1) + "-01-01','yyyy-mm-dd hh24:mi:ss')";
                    }
                    else
                    {
                        staPreTime += " and ����ʱ���ֵ<to_date('" + Convert.ToString(year - 1) + "-" + Convert.ToString(month + 1) + "-01','yyyy-mm-dd hh24:mi:ss')";
                    }

                    //��һ����ʱ��Σ����ȣ�
                    //�����һ�£����ȶ�ȥ��12��
                    if (month == 1)
                    {
                        staPreTimeHuanbi = "����ʱ���ֵ>=to_date('" + Convert.ToString(year - 1) + "-12-01','yyyy-mm-dd hh24:mi:ss') ";
                    }
                    else
                    {
                        staPreTimeHuanbi = "����ʱ���ֵ>=to_date('" + year.ToString() + "-" + Convert.ToString(month - 1) + "-01','yyyy-mm-dd hh24:mi:ss') ";
                    }
                    staPreTimeHuanbi += " and ����ʱ���ֵ<to_date('" + year.ToString() + "-" + month.ToString() + "-01','yyyy-mm-dd hh24:mi:ss')";

                    staTitleTime = comboYear2.Text + "�� " + comboMonth.Text + "��";
                }
                //�Զ���ʱ���
                else
                {
                    staTime = "����ʱ���ֵ>=to_date('" + dateTimeBegin.Value.ToString() + "','yyyy-mm-dd hh24:mi:ss') ";
                    staTime += " and ����ʱ���ֵ<=to_date('" + dateTimeEnd.Value.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                }
                strExp = "select " + staRegion + ",count(*) from ������Ϣ where " + staTime;

                if (comboRegion.Text == "�ɳ���")
                {
                    if (strRegion != "˳����")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                        {
                            strRegion = strRegion.Replace("����", "����,��ʤ");
                        }

                        strExp += " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else if (comboRegion.Text == "�ж�")
                {
                    if (strRegion != "˳����")
                    {
                        if (comboBoxHighLevel.Text == "ȫ��")
                        {
                            strExp += " and �����ж� in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strExp += " and �����ж� like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                    else
                    {
                        if (comboBoxHighLevel.Text != "ȫ��")
                        {
                            strExp += " and �����жӡ�like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                }
                else
                {
                    strExp += " and �����ɳ���='" + comboBoxHighLevel.Text + "'";
                }

                //��Ӱ�����Դ����
                if (radioJingzong.Checked)
                {
                    strExp += " and ������Դ='����'";
                }
                else
                {
                    strExp += " and ������Դ='110'";
                }

                //��Ӱ�����������
                if (radioXS.Checked) {
                    strExp += " and ��������='����'";
                }
                else if (radioZA.Checked)
                {
                    strExp += " and ��������='����'";
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
                    staYTitle = "������";
                    CreateBarChart(dCounts, sLabels, staTitle, comboRegion.Text, staYTitle);  //��ͳ��ͼ
                }

                //�����ͬ�Ȼ��ȣ�������һ�ڣ����㻷��ֵ
                else
                {
                    double[] dCounts1 = new double[dt.Rows.Count];
                    string[] sLabels1 = new string[dt.Rows.Count];
                    strExp1 = "select " + staRegion + ",count(*) from ������Ϣ where " + staPreTime;

                    if (comboRegion.Text == "�ɳ���")
                    {
                        if (strRegion != "˳����")
                        {
                            if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                            {
                                strRegion = strRegion.Replace("����", "����,��ʤ");
                            }

                            strExp1 += " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        }
                    }
                    else if (comboRegion.Text == "�ж�")
                    {
                        if (comboBoxHighLevel.Text == "ȫ��")
                        {
                            strExp1 += " and �����ж� in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strExp1 += " and �����ж� like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                    else
                    {
                        strExp1 += " and �����ɳ���='" + comboBoxHighLevel.Text + "'";
                    }

                    //������Դ
                    if (radioJingzong.Checked)
                    {
                        strExp1 += " and ������Դ='����'";
                    }
                    else
                    {
                        strExp1 += " and ������Դ='110'";
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
                    strExp1 = "select " + staRegion + ",count(*) from ������Ϣ where " + staPreTimeHuanbi;

                    if (comboRegion.Text == "�ɳ���")
                    {
                        if (strRegion != "˳����")
                        {
                            if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                            {
                                strRegion = strRegion.Replace("����", "����,��ʤ");
                            }

                            strExp1 += " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        }
                    }
                    else if (comboRegion.Text == "�ж�")
                    {
                        if (comboBoxHighLevel.Text == "ȫ��")
                        {
                            strExp1 += " and �����ж� in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strExp1 += " and �����ж� like '%" + comboBoxHighLevel.Text + "%'";
                        }
                    }
                    else
                    {
                        strExp1 += " and �����ɳ���='" + comboBoxHighLevel.Text + "'";
                    }

                    if (radioJingzong.Checked)
                    {
                        strExp1 += " and ������Դ='����'";
                    }
                    else
                    {
                        strExp1 += " and ������Դ='110'";
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
                    staTitle = "����ͳ��(" + staTitleTime + "��" + comboRegion.Text + "����)";
                    staYTitle = "������";
                    CreateTwoBarChart(dCounts1, sLabels1, dCounts2, staTitle, comboRegion.Text, staYTitle);   //��ͳ��ͼ
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
                    slabel[i] = slabel[i].Replace("�ɳ���", "").Replace("������", "");  // ��������̫����ȥ��Щ���ص��ַ�
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

        private void CreateTwoBarChart(double[] dCounts1, string[] sLabels1, double[] dCounts2, string staTitleTime, string strRegion, string staYTitle)
        {
            try
            {
                sLabels1 = strSub(sLabels1);   // �����ַ�����������������ʾ
                panel3.Controls.Clear();
                ZedGraphControl zedGraphControl1 = new ZedGraphControl();
                panel3.Controls.Add(zedGraphControl1);
                zedGraphControl1.Dock = DockStyle.Fill;
                GraphPane myPane = zedGraphControl1.GraphPane;

                // ����ͼ���˵������
                myPane.Title.Text = staTitleTime;

                // ���ú������˵������
                myPane.XAxis.Title.Text = strRegion;

                // �����������˵������
                myPane.YAxis.Title.Text = staYTitle;

                // ����ÿ��bar
                BarItem myCurve = myPane.AddBar("ͬ������", null, dCounts1, Color.Orange);
                BarItem myCurve2 = myPane.AddBar("��������", null, dCounts2, Color.Green);

                //����bar����ɫ
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);
                myCurve2.Bar.Fill = new Fill(Color.Green, Color.White, Color.Green);

                // ���ñ�������ɫ�ͽ���ɫ
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels1;
                myPane.XAxis.Scale.FontSpec.IsItalic = false;
                myPane.XAxis.Scale.FontSpec.Size = 8f;
                //myPane.XAxis.Scale.FontSpec.Angle = 90;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // ����ÿ��bar��label,���е�2��������ʾ�Ƿ���ʾ��bar������λ�ã���3��������ʾlabel�����з���
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

        //������״ͼ
        public void CreateBarChart(double[] dCounts, string[] sLabels, string staTitleTime,string strRegion,string staYTitle)
        {
            try
            {
                sLabels = strSub(sLabels);   // �����ַ�����������������ʾ
                panel3.Controls.Clear();
                ZedGraphControl zedGraphControl1 = new ZedGraphControl();
                panel3.Controls.Add(zedGraphControl1);
                zedGraphControl1.Dock = DockStyle.Fill;
                GraphPane myPane = zedGraphControl1.GraphPane;

                // ����ͼ���˵������
                myPane.Title.Text = staTitleTime;

                // ���ú������˵������
                myPane.XAxis.Title.Text = strRegion;

                // �����������˵������
                myPane.YAxis.Title.Text = staYTitle;

                // ����ÿ��bar
                BarItem myCurve = myPane.AddBar(staYTitle, null, dCounts, Color.Orange);

                //����bar����ɫ
                myCurve.Bar.Fill = new Fill(Color.Orange, Color.White, Color.Orange);

                // ���ñ�������ɫ�ͽ���ɫ
                myPane.Chart.Fill = new Fill(Color.White, Color.Blue, 45.0F);

                myPane.XAxis.MajorTic.IsBetweenLabels = true;
                myPane.XAxis.Scale.TextLabels = sLabels;
                myPane.XAxis.Scale.FontSpec.IsItalic = false;
                myPane.XAxis.Scale.FontSpec.Size = 8f;
                //myPane.XAxis.Scale.FontSpec.Angle = 90;
                myPane.XAxis.Type = AxisType.Text;

                zedGraphControl1.AxisChange();

                // ����ÿ��bar��label,���е�2��������ʾ�Ƿ���ʾ��bar������λ�ã���3��������ʾlabel�����з���
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
                if (comboRegion.Text == "�ж�")
                {
                    //if (strRegion != "" && strRegion != "˳����")
                    //{
                    //    comboBoxHighLevel.Enabled = false;
                    //}
                    //else {
                    comboBoxHighLevel.Enabled = true;
                    getPaichusuo();
                    //}
                }
                else if (comboRegion.Text == "������")
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
                cmd.CommandText = "select �ɳ����� from �����ɳ���";
                if (strRegion != "˳����")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    cmd.CommandText += " where �ɳ����� in ('" + strRegion.Replace(",", "','") + "')";
                }
                comboBoxHighLevel.Items.Add("ȫ��");
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

                if (strRegion != "˳����")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    cmd.CommandText = "select �ж��� from �������ж� where �����ɳ��� in ('" + strRegion.Replace(",", "','") + "') order by �ж���";
                }
                else
                {
                    cmd.CommandText = "select �ж��� from �������ж� order by �ж���";
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
            sw.WriteLine("��������:�� frmStatic." + sFunc + "������," + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

    }
}