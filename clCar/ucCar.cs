//********˳�¹�����Ŀ-�������ģ��******
//********�����ˣ�jie.zhang
//********�������ڣ� 2008.9.10
//********�汾�޸ģ�
//********   1. 2009.4.15 �޸��ƶ�������Ƶ
//********   2. 2009.5.8   �޸���Ƶͼ���С�����ƶ���Ƶ���ڶ���
//********   3. 2009.5.13  �ƶ���Ƶ�ı�ע����ͨ��ͬ
//                         �޸ĸ�����Ƶʱ�ĳ�ʼֵ����
//********��Ȩ���У��Ϻ�����λͼ��Ϣ�Ƽ����޹�˾

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Xml;
using System.IO;


using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace clCar
{
    public partial class ucCar : UserControl
    {
        //ب�Ų���ı���
        static MapControl mapControl1 = null;
        static string mysqlstr;    //���ݿ������ַ���
        static string[] StrCon;   // ���ݿ������ַ�����
        static ToolStripDropDownButton toolDDbtn;  //�������˵�


        //jie.zhang 2008.9.24
        public Boolean GetCarflag = false; //��ʼ������صı�־
        public string JZCarName = "";  //���г���������
        public string GzCarName = "";  // ���ٳ���������
        public string CanNm;
        public frmGuijiTime fhistory;  //��ʷ�켣����        
        public IResultSetFeatureCollection rsfcView;//��Χ������ͼԪ����
        public string[] carn; //���������ĺ���
        public double[] lastx; //���������ϴεľ���
        public double[] lasty; //���������ϴε�γ��
        public double xx;  //��ǰ�������ϴξ���
        public double yy;  //��ǰ�������ϴ�γ��
        public Boolean SetViewFlag = false;//���÷�Χ��ʶ��
        public int iflash = 0;
        public Boolean ZhiHui = false;

        public string strRegion = string.Empty; //�ɳ����û�����
        public string strRegion1 = "";             //�ж��û�����
        public string user = "";                      //��½�û�

        //��������
        public string strRegion2 = ""; //�ɵ������ɳ���
        public string strRegion3 = ""; //�ɵ������ж�
        public string excelSql = "";   //��ѯ����sql
        public int _startNo, _endNo;   // �ɵ�����ҳ��

        public System.Data.DataTable dtExcel = null; //��ͼҳ�����ݵ�����
        OracleDataAdapter apt1 = null;

        public ToolStripProgressBar toolPro;  // ���ڲ�ѯ�Ľ�������lili 2010-8-10
        public ToolStripLabel toolProLbl;     // ������ʾ�����ı���
        public ToolStripSeparator toolProSep;



        /// <summary>
        ///  ��ʼ������ģ��
        /// </summary>
        /// <param name="m">��ͼ�ؼ�</param>
        /// <param name="t">�������˵�</param>
        /// <param name="s">���ݿ����Ӳ���</param>
        /// <param name="zh">ֱ��ָ�ӵı�ʾ</param>
        public ucCar(MapControl m, ToolStripDropDownButton t, string[] s, Boolean zh)
        {
            InitializeComponent();

            try
            {
                mapControl1 = m;
                toolDDbtn = t;
                StrCon = s;
                this.ZhiHui = zh;
                mysqlstr = "data source=" + StrCon[0] + ";user id=" + StrCon[1] + ";password=" + StrCon[2];
                mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);
                //mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(mapControl1_ViewChanged);

                fhistory = new frmGuijiTime(m, s);

                toolDDbtn.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolSelect);
                this.fhistory.Visible = false;
                this.ToolCarDisable();

                this.comboBox1.Items.Clear();
                this.comboBox1.Items.Add("�ն˳�������");
                this.comboBox1.Items.Add("������λ");
                this.comboBox1.Text = this.comboBox1.Items[0].ToString();

            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-01-��ʼ������ģ��");
            }
        }

        /// <summary>
        /// ��ȡ���б�ʶ��
        /// </summary>
        /// <param name="jz"></param>
        public void getjzParameter(Boolean jz)
        {
            try
            {
                fhistory.getflag(jz);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-02-��ȡ���б�ʶ��");
            }
        }

        /// <summary>
        /// ��DataGrid
        /// </summary>
        public void AddGrid()
        {
            try
            {
                isShowPro(true);
                if (CanNm != "")
                {
                    string sql = string.Empty;
                    if (strRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show("û����������Ȩ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (strRegion == "˳����")
                    {
                        if (CanNm == "All")
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ";
                        }
                        else
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ where " + CanNm;
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(strRegion.Split(','), "����") > -1 && strRegion.IndexOf("��ʤ") < 0)
                        {
                            strRegion = strRegion.Replace("����", "����,��ʤ");
                        }
                        if (CanNm == "All")
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ where Ȩ�޵�λ in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ where " + CanNm + " and Ȩ�޵�λ in('" + strRegion.Replace(",", "','") + "')";
                        }
                    }

                    // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                    if (sql.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                        sql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                    else
                        sql += " where (�����ֶ�һ is null or �����ֶ�һ='')";
                    //-------------------------------------------------------

                    string Gzstring1 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break; 

                        if (GzArrayName[i] != "")
                        {
                            Gzstring1 = Gzstring1 + " �ն˳�������='" + GzArrayName[i] + "' or ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring1 = Gzstring1.Substring(0, Gzstring1.LastIndexOf("or") - 1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring1 = Gzstring1 + " �ն˳�������='" + GzArrayName[i] + "'";
                        }
                    }


                    string Gzstring2 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring2 = Gzstring2 + " �ն˳�������<>'" + GzArrayName[i] + "' and ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring2 = Gzstring2.Substring(0, Gzstring2.LastIndexOf("and") - 1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring2 = Gzstring2 + " �ն˳�������<>'" + GzArrayName[i] + "'";
                        }
                    }

                    string tsql = string.Empty;

                    if (Gzstring1 != "" && Gzstring2 != "")
                        tsql = sql + " and " + Gzstring1 + " Union all " + sql + " and " + Gzstring2;
                    else
                        tsql = sql;

                    if (tsql == "") return;

                    DataTable dt = GetTable(tsql);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("��ѯ���Ϊ0��","ϵͳ��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Information);
                        return;
                    }

                    #region ����Excel
                    try
                    {
                        excelSql = sql;
                        excelSql = "select * " + excelSql.Substring(excelSql.IndexOf("from"));
                        string sRegion2 = strRegion2;
                        string sRegion3 = strRegion3;

                        if (strRegion2 != "˳����")
                        {
                            if (strRegion2 != "")
                            {
                                if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
                                {
                                    sRegion2 = strRegion2.Replace("����", "����,��ʤ");
                                }
                                excelSql += " and (Ȩ�޵�λ in ('" + sRegion2.Replace(",", "','") + "'))";
                            }
                            else if (strRegion2 == "")
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

                        // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                        if (excelSql.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                            excelSql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                        else
                            excelSql += " where (�����ֶ�һ is null or �����ֶ�һ='')";
                        //-------------------------------------------------------
                        _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                        _endNo = pageSize1;

                    //    OracleConnection orc = new OracleConnection(mysqlstr);
                    //    try
                    //    {
                    //        orc.Open();
                    //        OracleCommand cmd = new OracleCommand(excelSql, orc);
                    //        apt1 = new OracleDataAdapter(cmd);
                    //        DataTable datatableExcel = new DataTable();
                    //        apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //        if (dtExcel != null) dtExcel.Clear();
                    //        dtExcel = datatableExcel;
                    //        cmd.Dispose();
                    //    }
                    //    catch
                    //    {
                    //        isShowPro(false);
                    //    }
                    //    finally { orc.Close(); }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    # endregion
                    
                    this.toolPro.Value = 2;
                    Application.DoEvents();

                    Pagedt1 = dt;
                    InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, this.dataGridView1);

                    //this.label2.Text = "����" + dt.Rows.Count.ToString() + "����¼";

                    //dataGridView1.DataSource = dt;                    
                    //dataGridView1.Refresh();
                    WriteEditLog(sql, "ucCar-04-��ѯ");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "ucCar-03-��DataGrid");
            }

        }

        //==========
        //==========
        //��ҳ����
        //==========
        //==========

        int pageSize1 = 100;     //ÿҳ��ʾ����
        int PagenMax1 = 0;         //�ܼ�¼��
        int pageCount1 = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageCurrent1 = 0;   //��ǰҳ��
        int PagenCurrent1 = 0;      //��ǰ��¼�� 
        DataSet Pageds1 = new DataSet();
        DataTable Pagedt1 = new DataTable();

        /// <summary>
        /// �õ����Ĳ�ѯ��¼��
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        private void getMaxCount1(string sql)//
        {
            PagenMax1 = GetScalar(sql);
        }

        //edit by fisher in 09-11-23
        public void InitDataSet1(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize1 = 100;      //����ҳ������
                PagenMax1 = Pagedt1.Rows.Count;
                TextNum1.Text = pageSize1.ToString();
                lblcount.Text = PagenMax1.ToString() + "��";//�ڵ���������ʾ�ܼ�¼��

                pageCount1 = (PagenMax1 / pageSize1);//�������ҳ��
                if ((PagenMax1 % pageSize1) > 0) pageCount1++;
                if (PagenMax1 != 0)
                {
                    pageCurrent1 = 1;
                }
                PagenCurrent1 = 0;       //��ǰ��¼����0��ʼ

                LoadData1(textNowPage, lblPageCount, bs, bn, dgv);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-04-InitDataSet1");
            }
        }

        //edit by fisher in 09-11-23
        public void LoadData1(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                isShowPro(true);
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = Pagedt1.Clone();

                if (pageCurrent1 == pageCount1)
                    nEndPos = PagenMax1;
                else
                    nEndPos = pageSize1 * pageCurrent1;
                nStartPos = PagenCurrent1;

                //tsl.Text = Convert.ToString(pageCurrent1) + "/" + pageCount1.ToString();
                textNowPage.Text = Convert.ToString(pageCurrent1);
                lblPageCount.Text = "/" + pageCount1.ToString();
                this.toolPro.Value = 1;
                Application.DoEvents();
                _startNo = nStartPos;
                _endNo = nEndPos;

                //��Ԫ����Դ���Ƽ�¼��
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt1.Rows[i]);
                    PagenCurrent1++;
                }

                bds.DataSource = dtTemp;
                bdn.BindingSource = bds;
                this.toolPro.Value = 2;
                Application.DoEvents();
                dgv.DataSource = bds;
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "ucCar-05-LoadData");
            }
        }

        /// <summary>
        /// ��ҳ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent1--;
                    if (pageCurrent1 < 1)
                    {
                        pageCurrent1 = 1;
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��");
                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent1++;
                    if (pageCurrent1 > pageCount1)
                    {
                        pageCurrent1 = pageCount1;

                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��");

                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent1 <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent1 = 1;
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent1 > pageCount1 - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent1 = pageCount1;
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);
                }
                else if (e.ClickedItem.Text == "���ݵ���")
                {
                    //DataExport();
                }

                #region ���ݵ���
                //DataTable datatableExcel = new DataTable();
                //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion

            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-06-��ҳ����");
            }
        }


        //���´�����fisher���ӣ�ּ������ÿҳ��ʾ����������
        /// <summary>
        /// ����ÿҳ��ʾ����������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && this.dataGridView1.Rows.Count > 0)
                {
                    pageSize1 = Convert.ToInt32(TextNum1.Text);
                    pageCurrent1 = 1;   //��ǰת����һҳ
                    PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    pageCount1 = (PagenMax1 / pageSize1);//�������ҳ��
                    if ((PagenMax1 % pageSize1) > 0) pageCount1++;

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);

                    #region ���ݵ���
                    //DataTable datatableExcel = new DataTable();
                    //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-07-����ÿҳ��ʾ����������");
            }
        }

        //���´�����fisher���ӣ�ּ��ʵ��ҳ��ת��09-11-23��
        /// <summary>
        /// ҳ��ת��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageNow1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(this.PageNow1.Text) < 1 || Convert.ToInt32(this.PageNow1.Text) > pageCount1)
                    {
                        System.Windows.Forms.MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow1.Text = pageCurrent1.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent1 = Convert.ToInt32(this.PageNow1.Text);
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridView1);

                        #region ���ݵ���
                        //DataTable datatableExcel = new DataTable();
                        //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                        //if (dtExcel != null) dtExcel.Clear();
                        //dtExcel = datatableExcel;
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-08-ҳ��ת��");
            }
        }


        /// <summary>
        /// ����DataGrid��ɫ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dataGridView1.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }

                    for (int k = 0; k < this.dataGridView1.Rows.Count; k++)
                        for (int j = 0; j < this.GzArrayName.Length; j++)
                            if (GzArrayName[j] != "")
                                if (GzArrayName[j] == this.dataGridView1.Rows[k].Cells["�ն˳�������"].Value.ToString())                                
                                    this.dataGridView1.Rows[k].DefaultCellStyle.BackColor = this.GzArrayColor[j];
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-09-����DataGrid��ɫ");
            }
        }

        private string SelectCarName = string.Empty;
        //��ǰѡ��ĳ����ƺ�

        /// <summary>
        /// ����������ʱͼ��
        /// </summary>
        public void CreateCarLayer()
        {

           #region  ���ݰ󶨵ķ�ʽ��ȡͼԪ

            try
            {

                if (mapControl1.Map.Layers["CarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                }

                if (mapControl1.Map.Layers["CarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarLabel");
                }

                string tableAlies = "CarLayer";


                //string strSQL = "Select �ն˳������� as Name,�ն�ID���� as ��_ID,'GPS������λϵͳ' as ����,X,Y from GPS������λϵͳ ";

                string strSQL = string.Empty;
                
                if (this.strRegion == string.Empty)
                {
                    MessageBox.Show(@"û����������Ȩ��", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (strRegion == "˳����")
                {
                    strSQL = "Select �ն˳������� as Name,�ն�ID���� as ��_ID,'GPS������λϵͳ' as ����,X,Y from GPS������λϵͳ ";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    strSQL = "Select �ն˳������� as Name,�ն�ID���� as ��_ID,'GPS������λϵͳ' as ����,X,Y from GPS������λϵͳ where Ȩ�޵�λ in ('" + strRegion.Replace(",", "','") + "') ";
                }


                DataTable dt = this.GetTable(strSQL);

                if (dt == null || dt.Rows.Count < 1)
                {
                    return;
                }

                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tableAlies, dt);
                MapInfo.Data.SpatialSchemaXY xy = new SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0,0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;



                MapInfo.Data.Table temTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                FeatureLayer temlayer = new FeatureLayer(temTable, tableAlies);

                mapControl1.Map.Layers.Add(temlayer);


                //�ı�ͼ���ͼԪ��ʽ 
                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("jCar.bmp", BitmapStyles.None, System.Drawing.Color.Red, 16));
                FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, cs);
                temlayer.Modifiers.Clear();

                //ProtectMap();

                temlayer.Modifiers.Append(fsm);


                //���ӱ�ע
                const string activeMapLabel = "CarLabel";
                Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");
                LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);

                LabelSource lbsource = new LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                //lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                //lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);               // ProtectMap(); 
                mapControl1.Map.Layers.Add(lblayer);


                //// ���ø��ٶ���
                //SetGZPolice();


                //���þ��ж���
                SetJZPolice();



                if (this.ZhiHui == true)
                {
                    SetTableDisable("CarLayer");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-10-����������ʱͼ��");
            }

            #endregion
        }



        // ���ø��ٶ���
        private void SetGZPolice()
        {
            //ProtectMap();

            //string sql = "Select X,Y from GPS������λϵͳ where �ն˳������� ='" + this.GzCarName + "'";
            //DataTable dt = GetTable(sql);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    double xv = Convert.ToDouble(dt.Rows[0]["X"]);
            //    double yv = Convert.ToDouble(dt.Rows[0]["Y"]);
            //    Trackline(xx, yy, xv, yv);

            //    xx = xv;
            //    yy = yv;

            //    DPoint dpoint = new DPoint(xv, yv);
            //    Boolean inflag = IsInBounds(xv, yv);
            //    if (inflag == false)
            //    {
            //        mapControl1.Map.Center = dpoint;
            //    }
            //}
        }


        private void SetJZPolice()
        {
            try
            {
                ProtectMap();

                string sql = "Select X,Y from  GPS������λϵͳ where �ն˳������� ='" + this.JZCarName + "'";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    double xv = Convert.ToDouble(dt.Rows[0]["X"]);
                    double yv = Convert.ToDouble(dt.Rows[0]["Y"]);

                    DPoint dpt = new DPoint(xv, yv);
                    mapControl1.Map.Center = dpt;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetJZPolice");
            }
        }

        private void ProtectMap()
        {
            try
            {
                if (this.Visible == false)   //jie.zhang 2010-0826  ���GPS��Աģ�鲻�ɼ���������ͼԪ������
                {
                    if (mapControl1.Map.Layers["CarLayer"] != null)
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                    if (mapControl1.Map.Layers["CarLabel"] != null)
                        mapControl1.Map.Layers.Remove("CarLabel");

                    return;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ProtectMap");
            }
        }


        /// <summary>
        /// �����ƶ�������ʱͼ��
        /// </summary>
        public void CreateVideoCarLayer()
        {
            #region ���ݰ󶨷�ʽ
            try
            {
                string tableAlies = "VideoCarLayer";

                if (mapControl1.Map.Layers[tableAlies] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable(tableAlies);
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }                

                string strSQL = string.Empty;// "Select �ն˳������� as Name,CAMID as �豸���,X,Y from  GPS������λϵͳ where CAMID is not null and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2;

                if (this.strRegion == string.Empty)
                {
                    MessageBox.Show(@"û����������Ȩ��", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (this.strRegion == "˳����")
                {
                    strSQL = "Select �ն˳������� as Name,CAMID as �豸���,X,Y from  GPS������λϵͳ where CAMID is not null ";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        this.strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    strSQL = "Select �ն˳������� as Name,CAMID as �豸���,X,Y from  GPS������λϵͳ where CAMID is not null and �ɳ����� in ('" + this.strRegion.Replace(",", "','") + "')";
                }

                DataTable dt = this.GetTable(strSQL);
                if (dt == null || dt.Rows.Count < 1)
                {
                    return;
                }
               
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tableAlies, dt);
                MapInfo.Data.SpatialSchemaXY xy = new SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0,0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;

                MapInfo.Data.Table temTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                FeatureLayer temlayer = new FeatureLayer(temTable, tableAlies);

                mapControl1.Map.Layers.Add(temlayer);

                //�ı�ͼ���ͼԪ��ʽ
                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("ydsp.bmp", BitmapStyles.None, System.Drawing.Color.Red, 20));
                FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, cs);
                temlayer.Modifiers.Clear();
                temlayer.Modifiers.Append(fsm);


                //���ӱ�ע
                string activeMapLabel = "VideoCarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableAlies);
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.YellowGreen;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                // ���ø��ٶ���
                SetGZPolice();

                //���þ��ж���
                SetJZPolice();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-11-�����ƶ�������ʱͼ��");
            }

            #endregion
        }

        /// <summary>
        /// ����ͼ��ɼ�
        /// </summary>
        /// <param name="tablename"></param>
        public void SetTableVisable(string tablename)
        {
            try
            {
                for (int i = 0; i < mapControl1.Map.Layers.Count; i++)
                {
                    IMapLayer layer = mapControl1.Map.Layers[i];

                    if (layer is FeatureLayer)
                    {
                        if (layer.Name == tablename)
                        {
                            layer.Enabled = true;
                        }
                    }
                    else if (layer is LabelLayer)
                    {
                        LabelLayer lLayer = (LabelLayer)layer;
                        for (int m = 0; m < lLayer.Sources.Count; m++)
                        {

                            if (lLayer.Sources[m].Name == tablename)
                            {
                                lLayer.Sources[m].Enabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-12-����ͼ��ɼ�");
            }
        }

        /// <summary>
        /// ����ͼ�㲻�ɼ�
        /// </summary>
        /// <param name="tablename"></param>
        public void SetTableDisable(string tablename)
        {
            try
            {
                this.GetCarflag = false;
                for (int i = 0; i < mapControl1.Map.Layers.Count; i++)
                {
                    IMapLayer layer = mapControl1.Map.Layers[i];

                    if (layer is FeatureLayer)
                    {
                        if (layer.Name == tablename)
                        {
                            layer.Enabled = false;
                        }
                    }
                    else if (layer is LabelLayer)
                    {
                        LabelLayer lLayer = (LabelLayer)layer;
                        for (int m = 0; m < lLayer.Sources.Count; m++)
                        {

                            if (lLayer.Sources[m].Name == tablename)
                            {
                                lLayer.Sources[m].Enabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-13-����ͼ�㲻�ɼ�");
            }
        }


        /// <summary>
        /// �����켣��
        /// </summary>
        public void CreateTrackLayer()
        {
            try
            {
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;

                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarTrack");
                Table tblTemp = Cat.GetTable("CarTrack");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarTrack");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);


                //���ӱ�ע
                //string activeMapLabel = "CarTrackLabel";
                //MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarTrack");
                //MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                //lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                //lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                //lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                ////lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                ////lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                //lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                //lbsource.DefaultLabelProperties.Caption = "Name";
                //lblayer.Sources.Append(lbsource);
                //mapControl1.Map.Layers.Add(lblayer);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-14-�����켣��");
            }
        }

        /// <summary>
        /// ��ʼ�����ӳ���
        /// </summary>
        public void AddCarFtr()
        {
            try
            {
                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["CarLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");

                string sqlcmd = string.Empty;
                if (strRegion == string.Empty)
                {
                    MessageBox.Show("û����������Ȩ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (strRegion == "˳����")
                {
                    sqlcmd = "Select * from GPS������λϵͳ where CAMID is null";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    sqlcmd = "Select * from GPS������λϵͳ where Ȩ�޵�λ in ('" + strRegion.Replace(",", "','") + "') and CAMID is null";
                }

                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["�ն˳�������"].ToString();
                        string camid = dr["�ն�ID����"].ToString();

                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;                       
                        ftr["Name"] = tempname;
                        //ftr["CAMERID"] = camid;
                        ftr["��_ID"] = camid;
                        ftr["����"] = "GPS������λϵͳ";
                       

                        if (tempname == GzCarName)
                        {
                            //Trackline(xx, yy, xv, yv);
                            xx = xv;
                            yy = yv;
                            MapInfo.Geometry.DPoint dpoint = new DPoint(xv, yv);
                            Boolean inflag = this.IsInBounds(xv, yv);
                            if (inflag == false)
                            {
                                mapControl1.Map.Center = dpoint;
                            }
                           
                            
                            cs.ApplyStyle(new BitmapPointStyle("jCar.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));

                            ////�޸ı�ע
                            //LabelProperties properties = new LabelProperties();
                            //properties.Attributes = LabelAttribute.VisibilityEnabled | LabelAttribute.Caption | LabelAttribute.PriorityMinor;
                            //properties.Visibility.Enabled = true;
                            //properties.Caption = "Name";

                            //SelectionLabelModifier modifer = new SelectionLabelModifier();
                            //modifer.Properties.Add(ftr.Key, properties);
                            //LabelLayer lblLayer = map.Layers[0] as LabelLayer;
                            //LabelSource source = lblLayer.Sources[0];
                            //source.Modifiers.Append(modifer);


                        }
                        else
                        {
                            cs.ApplyStyle(new BitmapPointStyle("jCar.BMP", BitmapStyles.None, System.Drawing.Color.Red, 16));
                           
                        }

                        ftr.Style = cs;
                        tblcar.InsertFeature(ftr);


                        if (tempname == JZCarName)
                        {
                            MapInfo.Geometry.DPoint dpoint = new DPoint(xv, yv);
                            mapControl1.Map.Center = dpoint;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-15-��ʼ�����ӳ���");
            }
        }

        /// <summary>
        /// �ж������Ƿ��ڿ��ӷ�Χ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Boolean IsInBounds(double x, double y)
        {
            try
            {
                if (mapControl1.Map.Bounds.x1 < x && x < mapControl1.Map.Bounds.x2 && mapControl1.Map.Bounds.y1 < y && y < mapControl1.Map.Bounds.y2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-16-�жϳ����Ƿ��ڿ��ӷ�Χ");
                return false;
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timLocation_Tick(object sender, EventArgs e)
        {
            try
            {
                iflash = iflash + 1;

                int i = iflash % 2;
                if (i == 0)
                {
                    GetFlash();
                }
                else
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }

                if (this.iflash % 10 == 0)
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                    timLocation.Enabled = false;

                    GetGzPoistion();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-17-��������");
            }
        }

        /// <summary>
        /// ���ظ��ٳ���
        /// </summary>
        private void GetGzPoistion()
        {
            try
            {
                if (GzCarName != "")
                {
                    Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                    Table tbl = cat.GetTable("CarLayer");
                    MapInfo.Mapping.Map map = mapControl1.Map;
                    MapInfo.Data.MIConnection micon = new MIConnection();
                    micon.Open();

                    string tblname = "CarLayer";
                    string colname = "Name";

                    MapInfo.Data.MICommand micmd = micon.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    micmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + "= '" + GzCarName + "'";
                    IResultSetFeatureCollection rsfcflash3 = micmd.ExecuteFeatureCollection();

                    micon.Close();
                    micon.Dispose();
                    micmd.Cancel();
                    micmd.Dispose();

                    if (tbl != null)
                    {
                        if (rsfcflash3.Count == 1)
                        {
                            foreach (Feature fcar in rsfcflash3)
                            {
                                MapInfo.Geometry.DPoint dpoint = new DPoint(fcar.Geometry.Centroid.x, fcar.Geometry.Centroid.y);
                                mapControl1.Map.Center = dpoint;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-18-���ظ��ٳ���");
            }
        }

        /// <summary>
        /// ���ó���Ϊ���ֳ���ʱ
        /// </summary>
        public void GetFlash()
        {
            try
            {
                string tblname = "CarLayer";
                string colname = "Name";
                string ftrname = SelectCarName;

                MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();

                if (ftrname != "")
                {
                    IResultSetFeatureCollection rsfcflash1 = null;
                    MapInfo.Mapping.Map map = mapControl1.Map;
                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }

                    MapInfo.Data.MIConnection conn = new MIConnection();
                    conn.Open();

                    MapInfo.Data.MICommand cmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + "= '" + ftrname + "'";
                    rsfcflash1 = cmd.ExecuteFeatureCollection();
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash1);
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();

                    if (rsfcflash1.Count > 0)
                    {
                        foreach (Feature f in rsfcflash1)
                        {
                            mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                            mapControl1.Map.Center = f.Geometry.Centroid;
                        }
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "select * from " + mapControl1.Map.Layers["VideoCarLayer"].ToString() + " where " + colname + "=@name";
                        cmd.Parameters.Add("@name", ftrname);
                        rsfcflash1 = cmd.ExecuteFeatureCollection();
                        if (rsfcflash1.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash1);
                            foreach (Feature f in rsfcflash1)
                            {
                                mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                break;
                            }
                        }
                    }
                    cmd.Cancel();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-19-���ó���Ϊ���ֳ���");
            }
        }

        // ��ȡ���ű���
        private double getScale()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("������", "���ű���"));
                return dou;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getScale");
                return 0;
            }
        }

        /// <summary>
        /// ���ӹ켣��
        /// </summary>
        public void setthefirstpoint()
        {
            try
            {
                IResultSetFeatureCollection rsfcflash = null;
                rsfcflash = GetFirstPoint("CarLayer");
                if (rsfcflash.Count > 0)
                {
                    foreach (Feature fcar in rsfcflash)
                    {
                        if (fcar["Name"].ToString() == this.GzCarName)
                        {
                            xx = fcar.Geometry.Centroid.x;
                            yy = fcar.Geometry.Centroid.y;
                        }
                    }
                }
                else
                {
                    rsfcflash = GetFirstPoint("VideoCarLayer");
                    if (rsfcflash.Count > 0)
                    {
                        foreach (Feature fcar in rsfcflash)
                        {
                            if (fcar["Name"].ToString() == this.GzCarName)
                            {
                                xx = fcar.Geometry.Centroid.x;
                                yy = fcar.Geometry.Centroid.y;

                                // writelog("firstpoint " + xx.ToString()+" " + yy.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-20-���ӹ켣��");
            }
        }


        private IResultSetFeatureCollection GetFirstPoint(string LayerName)
        {
            IResultSetFeatureCollection rsfcflash = null;
            try
            {
                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable(LayerName);
                MapInfo.Mapping.Map map = mapControl1.Map;

                MapInfo.Data.MIConnection micon = new MIConnection();
                micon.Open();


                string tblname = LayerName;
                string colname = "Name";

                MapInfo.Data.MICommand micmd = micon.CreateCommand();
                Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                micmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + "= '" + this.GzCarName + "'";
                rsfcflash = micmd.ExecuteFeatureCollection();

                micon.Close();
                micon.Dispose();
                micmd.Cancel();
                micmd.Dispose();

                return rsfcflash;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-21-GetFirstPoint");
                return rsfcflash;
            }
        }

        public void StartFlash()
        {
            try
            {
                timLocation.Enabled = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "StartFlash");
            }
        }
 

        /// <summary>
        /// ����ģ������Ȩ��
        /// </summary>
        public void SetUserRegion()
        {
            try
            {

                if (strRegion == "" && strRegion1 == "")// ��û���ɳ���Ȩ�ޣ�Ҳû���ж�Ȩ��
                {
                    return;
                }
                else if (strRegion == "" && strRegion1 != "") // ���ж�Ȩ�ޣ�û���ɳ���Ȩ��
                {
                    strRegion = GetPolice(strRegion1);

                }
                else if (strRegion != "" && strRegion1 != "")  // ���ж�Ȩ�ޣ�Ҳ���ɳ���Ȩ��
                {
                    strRegion = strRegion + "," + GetPolice(strRegion1);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-22-����ģ������Ȩ��");
            }
        }

        /// <summary>
        /// �����ж����ƻ�ȡ���ڵ��ɳ�������
        /// </summary>
        /// <param name="s1"></param>
        /// <returns></returns>
        private String GetPolice(string s1)
        {
            string reg = string.Empty;

            try
            {
                string[] ZdArr = s1.Split(',');
                for (int i = 0; i < ZdArr.Length; i++)
                {
                    string zdn = ZdArr[i];

                    DataTable dt = GetTable("Select �����ɳ��� from �������ж� where �ж���='" + zdn + "'");

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (i != ZdArr.Length - 1)
                            {
                                reg = reg + dr["�����ɳ���"].ToString() + ",";
                            }
                            else
                            {
                                reg = reg + dr["�����ɳ���"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-23-�����ж����ƻ�ȡ���ڵ��ɳ�������");
            }
            return reg;
        }

        /// <summary>
        /// ��ʼ��ʵʱ���
        /// </summary>        
        public void StartTimeCar()
        {
            try
            {
                StopTimeCar();

                this.GetCarflag = true;

                mapControl1.Tools.LeftButtonTool = "Select";

                ToolCarEnable();
                CreateCarLayer();
                CreateVideoCarLayer();

                CreateTrackLayer();

                AddGz();

                CanNm = "All";
                AddGrid();

                this.SetLayerSelect("CarLayer", "VideoCarLayer");

                timeCar.Interval = 30000;
                timeCar.Enabled = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-24-��ʼ��ʵʱ���");
            }
        }

        /// <summary>
        /// ɾ���켣
        /// </summary>
        public void ClearTrack()
        {
            try
            {
                if (mapControl1.Map.Layers["CarTrack"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarTrack");
                }

                CreateTrackLayer();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-25-ɾ���켣");
            }
        }

        /// <summary>
        /// ֹͣ�������
        /// </summary>
        public void StopTimeCar()
        {
            try
            {
                ToolCarDisable();

                if (this.timeCar.Enabled == true)
                {
                    timeCar.Enabled = false;
                }             

                GetCarflag = false;

                //fhistory.Dispose(); //Close();

                if (fhistory.timer1.Enabled == true)
                {
                    fhistory.timer1.Enabled = false;
                }

                if (mapControl1.Map.Layers["CarTrack"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarTrack");
                }

                if (mapControl1.Map.Layers["CarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                }

                if (mapControl1.Map.Layers["CarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarLabel");
                }

                if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoCarLayer");
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }

                if (mapControl1.Map.Layers["CarGuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarGuijiLayer");
                }

                if (mapControl1.Map.Layers["CarGzLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarGzLayer");
                }

                this.GzCarName = "";
                this.JZCarName = "";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-26-ֹͣ�������");
            }
        }


        /// <summary>
        /// ���������ص���ʱͼ��
        /// </summary>
        public void ClearCarTemp()
        {
            try
            {
                clearFeatures("CarTrack");
                clearFeatures("CarGuijiLayer");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-27-ֹͣ�������");
            }
        }

        private void clearFeatures(string tabAlias)
        {
            try
            {
                //�����ͼ�����ӵĶ���
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers[tabAlias];
                if (fl == null)
                {
                    return;
                }
                Table tableTem = fl.Table;

                //��������ж���
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-28-clearFeatures");
            }
        }

        /// <summary>
        /// �����켣��
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>      
        public void Trackline(double x1, double y1, double x2, double y2,Color color)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["CarTrack"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("CarTrack");

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(3, MapInfo.Styles.LineWidthUnit.Pixel), 2, color);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-29-�����켣��");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SearchCar();
        }

        /// <summary>
        /// ��ѯ����
        /// </summary>
        private void SearchCar()
        {
            try
            {
                if (this.CaseSearchBox.Text == "")
                {                     //�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��
                    CanNm = "All";
                }
                else
                {
                    CanNm = this.comboBox1.Text + " like  '%" + this.CaseSearchBox.Text + "%' ";
                }
                AddGrid();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-30-��ѯ����");
            }
        }

        //���õ�ǰͼ���ѡ��
        private void SetLayerSelect(string layername1, string layername2)
        {
            try
            {
                MapInfo.Mapping.Map map = mapControl1.Map;

                for (int i = 0; i < map.Layers.Count; i++)
                {
                    IMapLayer layer = map.Layers[i];
                    //string lyrname = layer.Alias;

                    MapInfo.Mapping.LayerHelper.SetSelectable(layer, false);
                }

                if (mapControl1.Map.Layers[layername1] != null)
                    MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername1], true);
                if (mapControl1.Map.Layers[layername2] != null)

                    MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername2], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-31-����ͼ���ѡ");
            }
        }


        /// <summary>
        /// �����ܱ߲�ѯ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    switch (e.ToolName)
                    {
                        case "SelectPolygon":

                            switch (e.ToolStatus)
                            {
                                case ToolStatus.End:
                                    if (this.GetCarflag)
                                    {
                                        MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                                        FeatureLayer lyr = mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                                        this.rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                        if (this.rsfcView != null)
                                        {
                                            string SearchName = "";
                                            int i = 1;
                                            foreach (Feature f in this.rsfcView)
                                            {
                                                foreach (MapInfo.Data.Column col in f.Columns)
                                                {
                                                    if (col.ToString() == "NAME")
                                                    {
                                                        string ftename = f["NAME"].ToString();
                                                        if (i == this.rsfcView.Count || this.rsfcView.Count == 1)
                                                        {
                                                            SearchName = SearchName + "�ն˳������� = '" + ftename + "'";
                                                        }
                                                        else
                                                        {
                                                            SearchName = SearchName + "�ն˳������� = '" + ftename + "' or ";
                                                        }
                                                        i = i + 1;
                                                    }
                                                }
                                            }


                                            this.CanNm = SearchName; //20081008
                                            this.AddGrid();

                                            this.SetViewFlag = true;
                                            if (this.rsfcView.Count > 0)
                                            {
                                                if (this.rsfcView.Count == 1)
                                                {
                                                    foreach (Feature f in this.rsfcView)
                                                    {
                                                        mapControl1.Map.Center = f.Geometry.Centroid;
                                                    }
                                                }
                                                else
                                                {
                                                    mapControl1.Map.SetView(this.rsfcView.Envelope);
                                                }
                                            }
                                            WriteEditLog("", "�����ѡ��");
                                        }

                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Select":
                            switch (e.ToolStatus)
                            {
                                case ToolStatus.End:
                                    if (GetCarflag == true)
                                    {
                                        try
                                        {
                                            MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                                            FeatureLayer lyr = mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                                            IResultSetFeatureCollection rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                            try
                                            {
                                                if (rsfcView != null)
                                                {
                                                    if (rsfcView.Count > 0)
                                                    {
                                                        foreach (Feature f in rsfcView)
                                                        {
                                                            string ftename = f["Name"].ToString();

                                                            if (this.dataGridView1.Rows.Count > 0)
                                                            {
                                                                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                                                {
                                                                    if (
                                                                        dataGridView1.Rows[i].Cells[0].Value.ToString() ==
                                                                        ftename)
                                                                    {
                                                                        dataGridView1.CurrentCell =
                                                                            dataGridView1.Rows[i].Cells[0];
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    lyr = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;

                                                    rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                                    if (rsfcView.Count > 0)
                                                    {
                                                        foreach (Feature f in rsfcView)
                                                        {
                                                            string ftename = f["Name"].ToString();

                                                            if (this.dataGridView1.Rows.Count > 0)
                                                            {
                                                                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                                                {
                                                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == ftename)
                                                                    {
                                                                        dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                lyr = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;

                                                rsfcView = session.Selections.DefaultSelection[lyr.Table];
                                                if (rsfcView.Count > 0)
                                                {
                                                    foreach (Feature f in rsfcView)
                                                    {
                                                        string ftename = f["Name"].ToString();

                                                        if (this.dataGridView1.Rows.Count > 0)
                                                        {
                                                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                                            {
                                                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == ftename)
                                                                {
                                                                    dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            WriteEditLog("", "����ѡ��");
                                        }
                                        catch (Exception ex)
                                        {
                                            writeToLog(ex, "��ȡDG�е�����");
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-32-�����ܱ߲�ѯ����");
            }
        }

        /// <summary>
        /// �ܱ߲�ѯ����
        /// </summary>
        /// <param name="dpt"></param>
        /// <param name="distance"></param>
        public void SearchCarDistance(MapInfo.Geometry.DPoint dpt, Double distance)
        {
            //�ж���ʱTempCar�Ƿ���ڣ�������رղ����½���
            try
            {
                if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoCarLayer");
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoCarLayer");
                Table tblTemp = Cat.GetTable("VideoCarLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("VideoCarLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("�豸���", 100));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);

                //���ӱ�ע
                const string activeMapLabel = "VideoCarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);


                //��CarLayer��ѡ���ܱߵĳ����������ӵ�tempcar
                double x1, x2;
                double y1, y2;
                double x, y;

                double dbufferdis = distance / 111000;

                x = dpt.x;
                y = dpt.y;
                x1 = x - dbufferdis;
                x2 = x + dbufferdis;
                y1 = y - dbufferdis;
                y2 = y + dbufferdis;


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");

                if (strRegion == string.Empty)
                {
                    MessageBox.Show("û����������Ȩ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string sql = string.Empty;

                if (strRegion == "˳����")
                {
                    sql = "Select * from GPS������λϵͳ where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " order by CAMID desc ";
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    sql = "Select * from GPS������λϵͳ where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and Ȩ�޵�λ in('" + strRegion.Replace(",", "','") + "') order by CAMID desc";
                }

                DataTable dt = GetTable(sql);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string CAMID = Convert.ToString(dr["CAMID"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();

                        if (CAMID == "")
                        {
                            cs.ApplyStyle(new BitmapPointStyle("jCar.BMP", BitmapStyles.None, System.Drawing.Color.Red, 16));
                        }
                        else
                        {
                            cs.ApplyStyle(new BitmapPointStyle("ydsp.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));
                        }

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = dr["�ն˳�������"].ToString();
                        ftr["�豸���"] = CAMID;
                        tblcar.InsertFeature(ftr);
                    }
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-33-�ܱ߲�ѯ����");
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        SearchCar();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textBox1_KeyDown");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.RefreshGrid();
        }

        /// <summary>
        /// ���ݱ�˫��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //�����ͷ,�˳�

            try
            {
                DPoint dp = new DPoint();

                string sqlFields = "�ն�ID����,�ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,X,Y,����״̬,GPSʱ�� ";
                string strSQL = "select " + sqlFields + " from GPS������λϵͳ t where �ն˳�������='" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'";

                DataTable datatable = GetTable(strSQL);

                System.Drawing.Point pt = new System.Drawing.Point();
                if (datatable.Rows.Count > 0)
                {
                    try
                    {
                        dp.x = Convert.ToDouble(datatable.Rows[0]["X"]);
                        dp.y = Convert.ToDouble(datatable.Rows[0]["Y"]);
                    }
                    catch
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;
                        return;
                    }
                    if (dp.x == 0 || dp.y == 0)
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;
                        return;
                    }
                    mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                    WriteEditLog("�ն˳�������='" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'", "�鿴����");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-34-���ݱ�˫��");
            }
        }

        /// <summary>
        /// ��ʾ������Ϣ
        /// </summary>
        private nsInfo.FrmInfo frmMessage = new nsInfo.FrmInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
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
                frmMessage.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " ucCar-35-��ʾ������Ϣ");
            }
        }

        /// <summary>
        /// ��������Enable
        /// </summary>
        public void ToolCarEnable()
        {
            try
            {
                toolDDbtn.Visible = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-36-��������Enable");
            }
        }


        /// <summary>
        /// �����˵�Disable
        /// </summary>
        public void ToolCarDisable()
        {
            try
            {
                toolDDbtn.Visible = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-37-�����˵�Disable");
            }
        }

        private string[] GzArrayName = new string[10] { "","","","","","","","","",""};  //���ٳ�������������
        private Color[] GzArrayColor = new Color[10] { Color.SandyBrown, Color.Green, Color.Yellow, Color.DarkCyan, Color.Firebrick, Color.Fuchsia, Color.Gainsboro, Color.Gold, Color.Honeydew, Color.Khaki }; //���ٳ�������ɫ����
        private double[] GzArrayLx = new double[10] {0, 0,0,0,0,0,0,0,0,0};    //���ٳ������ϸ���ľ������� ����
        private double[] GzArrayLy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //���ٳ������ϸ����γ������ γ��

        private double[] GzArrayNx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //���ٳ������ϸ���ľ������� ����
        private double[] GzArrayNy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //���ٳ������ϸ����γ������ γ��


        private void RefreshGzArray()
        {
            try
            {
                GzArrayLx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //���ٳ������ϸ���ľ������� ����
                GzArrayLy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //���ٳ������ϸ����γ������ γ��

                GzArrayNx = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //���ٳ������ϸ���ľ������� ����
                GzArrayNy = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //���ٳ������ϸ����γ������ γ��

                for (int i = 0; i < this.GzArrayName.Length; i++)
                {
                    if (GzArrayName[i] != "")
                    {
                        DataTable dt = GetTable("Select * from gps������λϵͳ where �ն˳������� like '%" + this.GzArrayName[i] + "%'");
                        if (dt.Rows.Count > 0)
                        {
                            GzArrayLx[i] = Convert.ToDouble(dt.Rows[0]["X"]);
                            GzArrayLy[i] = Convert.ToDouble(dt.Rows[0]["Y"]);
                        }
                    }
                }

                AddGrid();

                AddGz();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "RefreshGzArray");
            }
        }


        private void AddGz()
        {
            try
            { 
                /////��ȡ���µ�����
                if (GzArrayName.Length == 0) return;

                for (int i = 0; i < this.GzArrayName.Length; i++)
                {
                    if (GzArrayName[i] != "")
                    {
                        DataTable dt = GetTable("Select * from gps������λϵͳ where �ն˳������� like '%" + this.GzArrayName[i] + "%'");
                        if (dt.Rows.Count > 0)
                        {
                            GzArrayNx[i] = Convert.ToDouble(dt.Rows[0]["X"]);
                            GzArrayNy[i] = Convert.ToDouble(dt.Rows[0]["Y"]);
                        }
                    }
                }

                //������ʱ��
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;                
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarGZLayer");
                Table tblTemp = Cat.GetTable("CarGZLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarGZLayer");
                }
                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Insert(0,lyr);


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["CarGZLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarGzLayer");

                //��������
                for (int i = 0; i < GzArrayName.Length; i++)
                {
                    string tempname = GzArrayName[i];

                    if (tempname != "")
                    {

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(GzArrayNx[i], GzArrayNy[i])) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("jCar.bmp", BitmapStyles.None, System.Drawing.Color.Red, 26));

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr["Name"] = tempname;

                        MapInfo.Geometry.DPoint dpoint = new DPoint(GzArrayNx[i], GzArrayNy[i]);
                      
                        ftr.Style = cs;
                        tblcar.InsertFeature(ftr);

                        Trackline(GzArrayLx[i], GzArrayLy[i], GzArrayNx[i], GzArrayNy[i], GzArrayColor[i]);

                        GzArrayLx[i] = GzArrayNx[i];
                        GzArrayLy[i] = GzArrayNy[i];
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-15-��ʼ�����ӳ���");
            }
        }

        /// <summary>
        /// ѡ�񹤾�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToolSelect(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                string Selectext = e.ClickedItem.Text;

                switch (Selectext)
                {
                    case "����ѡ��":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        break;
                    case "��Χ��ѯ":
                        mapControl1.Tools.LeftButtonTool = "SelectPolygon";
                        break;
                    case "���ٳ���":
                        toolDDbtn.DropDownItems[2].Text = "ȡ������";
                        GzCarName = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        setthefirstpoint();
                        WriteEditLog(GzCarName, "���ٳ���");
                        break;
                    case "ȡ������":
                        toolDDbtn.DropDownItems[2].Text = "���ٳ���";
                        GzCarName = "";
                        xx = 0;
                        yy = 0;
                        ClearTrack();
                        WriteEditLog(GzCarName, "ȡ������");
                        break;
                    case "��������":
                        JZCarName = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        toolDDbtn.DropDownItems[3].Text = "ȡ������";
                        WriteEditLog(JZCarName, "��������");
                        break;
                    case "ȡ������":
                        JZCarName = "";
                        toolDDbtn.DropDownItems[3].Text = "��������";
                        WriteEditLog(JZCarName, "ȡ������");
                        break;
                    case "�켣�ط�":
                        fhistory.Visible = true;
                        fhistory.user = user;
                        fhistory.comboBox1.Text = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();
                        break;
                    case "����Ŀ��":
                        DataTable dt = GetTable("Select �ն˳������� from GPS������λϵͳ where �����ֶ�һ is null or �����ֶ�һ='' order by �ն˳�������" );
                        if (dt.Rows.Count > 0)
                        {
                            frmGz frmgz = new frmGz(dt, GzArrayName, StrCon);
                            if (frmgz.ShowDialog(this) == DialogResult.OK)
                            {
                                this.GzArrayName = frmgz.ArrayName;

                                RefreshGzArray();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-38-ѡ�񹤾�");
            }
        }

       

        /// <summary>
        /// �������ݱ�
        /// </summary>
        public void RefreshGrid()
        {
            try
            {
                isShowPro(true);
                if (CanNm != "")
                {
                    string sql = string.Empty;

                    if (strRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show("û����������Ȩ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (strRegion == "˳����")
                    {
                        if (CanNm == "All")
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ";
                        }
                        else
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ where " + CanNm;
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(strRegion.Split(','), "����") > -1 && strRegion.IndexOf("��ʤ") < 0)
                        {
                            strRegion = strRegion.Replace("����", "����,��ʤ");
                        }
                        if (CanNm == "All")
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ where Ȩ�޵�λ in ('" + strRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            sql = "select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from GPS������λϵͳ where " + CanNm + " and Ȩ�޵�λ in('" + strRegion.Replace(",", "','") + "')";
                        }
                    }

                    // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                    if (sql.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                        sql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                    else
                        sql += " where (�����ֶ�һ is null or �����ֶ�һ='')";
                    //-------------------------------------------------------

                    string Gzstring1 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring1 = Gzstring1 + " �ն˳�������='" + GzArrayName[i] + "' or ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring1 = Gzstring1.Substring(0,Gzstring1.LastIndexOf("or")-1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring1 = Gzstring1 + " �ն˳�������='" + GzArrayName[i] + "'";
                        }
                    }


                    string Gzstring2 = string.Empty;
                    for (int i = 0; i < this.GzArrayName.Length; i++)
                    {
                        if (GzArrayName[0] == "") break;

                        if (GzArrayName[i] != "")
                        {
                            Gzstring2 = Gzstring2 + " �ն˳�������<>'" + GzArrayName[i] + "' and ";
                        }
                        else if(GzArrayName[i]=="")
                        {
                            Gzstring2 = Gzstring2.Substring(0, Gzstring2.LastIndexOf("and") - 1);

                            break;
                        }
                        else if (i == GzArrayName.Length - 1)
                        {
                            Gzstring2 = Gzstring2 + " �ն˳�������<>'" + GzArrayName[i] + "'";
                        }
                    }

                    string tsql = string.Empty ;

                    if (Gzstring1 != "" && Gzstring2 != "")
                        tsql = sql +" and " +Gzstring1 + " Union all " + sql +" and "+ Gzstring2;
                    else
                        tsql = sql;


                    if (Gzstring1 != "" && Gzstring2 != "" && ordername != "")
                        tsql = sql + " and " + Gzstring1 + " Union all select �ն˳�������,������λ,��ǰ�ٶ�,��ǰ����,GPSʱ��,����״̬,X,Y from (" + sql + " and " + Gzstring2 + " order by " + ordername + " desc)";
                    if (Gzstring1 == "" && Gzstring2 == "" && ordername != "")
                        tsql = sql + " order by " + ordername + " desc";

                    if (tsql == "") return;

                    DataTable dt = GetTable(tsql);
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    if (dt.Rows.Count > 0)
                    {
                        this.dataGridView1.DataSource = dt;

                        foreach (DataRow dr in dt.Rows)
                        {
                            //string carname = Convert.ToString(dr["�ն˳�������"]);
                            if (dataGridView1.Rows.Count > 0)
                            {
                                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                {
                                    //if (dataGridView1.Rows[i].Cells[0].Value.ToString() == carname)
                                    //{
                                    //    dataGridView1.Rows[i].Cells[1].Value = dr["������λ"].ToString();
                                    //    dataGridView1.Rows[i].Cells[2].Value = dr["��ǰ�ٶ�"].ToString();
                                    //    dataGridView1.Rows[i].Cells[3].Value = dr["��ǰ����"].ToString();
                                    //    dataGridView1.Rows[i].Cells[4].Value = dr["GPSʱ��"].ToString();
                                    //    dataGridView1.Rows[i].Cells[5].Value = dr["����״̬"].ToString();
                                    //    dataGridView1.Rows[i].Cells[6].Value = dr["X"].ToString();
                                    //    dataGridView1.Rows[i].Cells[7].Value = dr["Y"].ToString();
                                    //}
                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == this.SelectCarName)
                                    {
                                        //dataGridView1.Rows[i].Selected = true;
                                        dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                                    }
                                }
                            }
                        }
                    }
                    this.toolPro.Value = 2;
                    Application.DoEvents();

                    #region ���ݵ���
                    excelSql = sql;
                    excelSql = "select * " + excelSql.Substring(excelSql.IndexOf("from"));
                    string sRegion2 = strRegion2;
                    string sRegion3 = strRegion3;

                    if (strRegion2 != "˳����")
                    {
                        if (strRegion2 != "")
                        {
                            if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
                            {
                                sRegion2 = strRegion2.Replace("����", "����,��ʤ");
                            }
                            excelSql += " and (Ȩ�޵�λ in ('" + sRegion2.Replace(",", "','") + "'))";
                        }
                        else if (strRegion2 == "")
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

                    // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                    if (excelSql.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                        excelSql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                    else
                        excelSql += " where (�����ֶ�һ is null or �����ֶ�һ='')";
                    //-------------------------------------------------------

                    _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                    _endNo = pageSize1;

                    //OracleConnection orc = new OracleConnection(sql);
                    //try
                    //{
                    //    orc.Open();
                    //    OracleCommand cmd = new OracleCommand(excelSql, orc);
                    //    apt1 = new OracleDataAdapter(cmd);
                    //    DataTable datatableExcel = new DataTable();
                    //    apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //    if (dtExcel != null) dtExcel.Clear();
                    //    dtExcel = datatableExcel;
                    //    cmd.Dispose();
                    //}
                    //catch
                    //{
                    //    isShowPro(false);
                    //}
                    //finally
                    //{
                    //    orc.Close();
                    //}
                    #endregion

                    WriteEditLog(sql, "��ѯ");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "ucCar-39-�������ݱ�ʱ��������");
            }
        }

        /// <summary>
        /// ������˸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                SelectCarName = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();

                this.toolgz.Text = "����";

                for (int i = 0; i < this.GzArrayName.Length; i++)
                {
                    if (this.GzArrayName[i] == this.SelectCarName)
                    {
                        this.toolgz.Text = "ȡ������";
                        break;
                    }
                }


                this.timLocation.Enabled = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-40-������˸");
            }
        }


        private void ucCar_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    StopTimeCar();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar_VisibleChanged");
            }
        }

        /// <summary>
        /// ��ѯSQL
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ִ��SQL
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// ��ȡScalar
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
        }

        /// <summary>
        /// �쳣��־
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clCar-ucCar-" + sFunc);
        }

        //��¼������¼
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�������','GPS������λϵͳ:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "WriteEditLog");
            }
        }

        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {

                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();

                if (keyword.Length < 1 || colfield.Length < 1) return;

                if (colfield == "�ն˳�������")
                {
                    string strExp = "select distinct(" + colfield + ") from gps������λϵͳ t  where " + colfield + " like '%" + keyword + "%' order by " + colfield;
                    DataTable dt = GetTable(strExp);
                    CaseSearchBox.GetSpellBoxSource(dt);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-41-����ƥ��");
            }
        }

        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {

                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();

                if (colfield.Length < 1) return;

                if (colfield == "������λ")
                {
                    string strExp = "select distinct(�ɳ�����) from �����ɳ��� order by �ɳ�����";
                    DataTable dt = GetTable(strExp);
                    CaseSearchBox.GetSpellBoxSource(dt);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-42-�����б�");
            }
        }

        private void CaseSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        SearchCar();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucCar-42-�����б�");
            }
        }

        // ��ʾ�����ؽ�����
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
                writeToLog(ex, "isShowPro");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.button2.Text == "��ʼ")
                {
                    this.timer1.Interval = 3 * 1000;
                    this.timer1.Enabled = true;
                    this.button2.Text = "����";
                }
                else if (this.button2.Text == "����")
                {
                    this.timer1.Enabled = false;
                    this.button2.Text = "��ʼ";
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button2_Click");
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            try
            {
                string sql = "select �ն˳�������,X,Y from gps������λϵͳ ";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string num = Convert.ToString(dr["�ն˳�������"]);
                        double x = Convert.ToDouble(dr["X"]);
                        double y = Convert.ToDouble(dr["Y"]);

                        if (x > mapControl1.Map.Center.x)
                            x = x - 0.1;
                        else
                            x = x + 0.1;

                        if (y > mapControl1.Map.Center.y)
                            y = y - 0.1;
                        else
                            y = y + 0.1;

                        string sqltem = "update gps������λϵͳ set x = " + x.ToString() + ",y=" + y.ToString() + " where �ն˳�������='" + num + "'";

                        Console.WriteLine(num + ":���� " + x.ToString() + " γ��:" + y.ToString());
                        this.RunCommand(sqltem);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "timer1_Tick_1");
            }
        }


        //  ��ʱ����
        private void timeCar_Tick(object sender, EventArgs e)
        {
            try
            {
                CreateCarLayer();

                CreateVideoCarLayer();

                AddGz();

                RefreshGrid();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "timeCar_Tick");
            }
        }

        private void toolgz_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.SelectCarName != "")
                {

                    System.Windows.Forms.ListBox lst = new System.Windows.Forms.ListBox();
                    lst.Items.Clear();
                    if (GzArrayName.Length > 0)
                    {
                        for (int i = 0; i < GzArrayName.Length; i++)
                        {
                            if (GzArrayName[i] != "")
                                lst.Items.Add(GzArrayName[i]);
                        }
                    }

                    if (this.toolgz.Text == "����")
                    {
                        if (lst.Items.Count > 9)
                        {
                            MessageBox.Show("���Ŀ�겻�ܳ���10��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        for (int i = 0; i < lst.Items.Count; i++)
                        {
                            if (lst.Items[i].ToString() == SelectCarName)
                            {
                                MessageBox.Show("�þ����Ѿ�������Ϊ���Ŀ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }

                        lst.Items.Add(this.SelectCarName);
                    }
                    else if (this.toolgz.Text == "ȡ������")
                    {
                        for (int i = 0; i < lst.Items.Count; i++)
                        {
                            if (lst.Items[i].ToString() == this.SelectCarName)
                            {
                                lst.Items.RemoveAt(i);
                            }
                        }
                    }

                    GzArrayName = new string[10] { "", "", "", "", "", "", "", "", "", "" };

                    for (int i = 0; i < lst.Items.Count; i++)
                    {
                        GzArrayName[i] = lst.Items[i].ToString();
                    }

                    RefreshGzArray();
                }
            }
            catch (Exception ex)
            { writeToLog(ex, "toolgz_Click"); }
        }

        string ordername = string.Empty;

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                DataGridViewColumn Column = dataGridView1.Columns[e.ColumnIndex];
                Console.WriteLine(Column.HeaderText);

                ordername = Column.HeaderText;

                RefreshGrid();

                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[ordername];
            }
            catch (Exception ex)
            { writeToLog(ex, "dataGridView1_ColumnHeaderMouseClick"); }
        }
    }
}