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
        private string[] tableNames = new string[] { "��������","��ȫ������λ", "����", "�ΰ�����", "������ҵ", "��Ƶ", "����˨", "�����ص㵥λ", "GPS������λϵͳ", "�����ɳ���", "��Ա��λϵͳ", "�������ж�", "����������", "��ʱ��" };
        private MapControl mapControl1 = null;
        private ToolStrip toolStrip1 = null;
        private string strConn;          // �����ַ���
        private string[] conStr = null;  // ���Ӳ���
        private string getFromNamePath;  // GetFromNameConfig.ini��·��
        public string strRegion = "";    // �ɳ���Ȩ��
        public string strRegion1 = "";   // �ж�Ȩ��
        public string user = "";         // �û���

        public string strRegion2 = "";   // �ɵ������ɳ���
        public string strRegion3 = "";   // �ɵ������ж�
        public string YTexcelSql = "";   // ��ͷ��ѯ����sql
        public string ZBexcelSql = "";   // �ܱ߲�ѯ����sql
        public string ZDexcelSql = "";   // �ص㵥λ��ѯ����sql
        public string GJexcelSql = "";   // �߼���ѯ����sql
        public string exportSql = "";    // ����Excel����SQL

        ////////--����ͨ��������ѯ��Ҫ���Ĳ���--//////////////
        
        public ToolStripLabel toolStriplbl = null;
        public ToolStripDropDownButton toolSbutton = null;
        public int videop = 0;  //��Ƶ��ض�ͨѶ�˿�
        public string[] videoConnstring = new string[6];  // ��Ƶ��������ַ���
        public string videoexepath = string.Empty;        // ��Ƶ��ض�λ��
        public string KKAlSys = string.Empty;
        public string KKALUser = string.Empty;
        public double KKSearchDist = 0;
        public string stringϽ�� = string.Empty;

        //////////////////////////////////////////////////

        private string photoserver = string.Empty;

        public System.Data.DataTable dtExcel = null; //������
        public System.Data.DataTable dtEdit = null;  // �ص㵥λ�ļ�����Ȩ�ޱ�lili��

        public ToolStripProgressBar toolPro;  // ���ڲ�ѯ�Ľ�������lili 2010-8-10
        public ToolStripLabel toolProLbl;     // ������ʾ�����ı���
        public ToolStripSeparator toolProSep;

        #region ���뷨
        //����һЩAPI����
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

        private System.Data.DataTable dt = null;//DataTable��
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
                //��ӵ�ͼ�¼�
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
        /// ��ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25  
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
        /// ��ͷģ���ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25  
        /// </summary>
        private void InitialYintouTable()
        {
            try
            {
                comboBox1.Items.Clear();
                OracelData linkData = new OracelData(strConn);
                DataTable datatable = linkData.SelectDataBase("select ���� from ��ͷ��ѯ�� group by ����");
                comboBox1.Items.Add("ȫ��");
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
        /// ��ʼ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25  
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
        /// �����л�ʱҪ�رյı�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25 
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
        /// ��ʼ���߼���ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25 
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
        private string[] strName = new string[] { "�����ѯ", "�ܱ߲�ѯ", "��ͷ��ѯ", "�߼���ѯ" };  // ��ȡ�ۺϲ�ѯ���漰��ͼ���ģ��
        /// <summary>
        /// tabҳ�л�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                #region ����ǰ���룬����ѡ��л�ʱ�������ģ��Ĳ�ѯ���
                //��ͷ��ѯ���رյ��ӱ�
                //�����ѯ��������checkbox��check
                //����������رյ��ӱ�����ͼ��������Ϊpan
                //removeTemPoints();

                ////��ʼ��������
                //pageSize = 1000;     //ÿҳ��ʾ����
                //nMax = 0;         //�ܼ�¼��
                //pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
                //pageCurrent = 0;   //��ǰҳ��
                //nCurrent = 0;      //��ǰ��¼��

                ////�л�tab������ʱ,���б�,������0
                //dataGridView1.Rows.Clear();
                //PageNow1.Text = "0";
                //PageCount1.Text = "/ {0}";
                //RecordCount1.Text = "��0����¼";
                //TextNum1.Text = pageSize.ToString();

                //dataGridView2.Rows.Clear();
                //PageNow2.Text = "0";
                //PageCount2.Text = "/ {0}";
                //RecordCount2.Text = "��0����¼";
                //TextNum2.Text = pageSize.ToString();

                //dataGridView4.Rows.Clear();
                //PageNow3.Text = "0";
                //PageCount3.Text = "/ {0}";
                //RecordCount3.Text = "��0����¼";
                //TextNum3.Text = pageSize.ToString();

                //dataGridView5.DataSource = null;
                //PageNow4.Text = "0";
                //PageCount4.Text = "/ {0}";
                //RecordCount4.Text = "��0����¼";
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
                    isVisbleLayer("�����ѯ");
                    this.LinklblHides.Visible = false; 
                }
                if (this.tabControl1.SelectedTab == this.tabZhoubian)
                {
                    isVisbleLayer("�ܱ߲�ѯ");
                    if (this.groupBox3.Visible)
                        this.LinklblHides.Text = "����������";
                    else
                        this.LinklblHides.Text = "��ʾ������";

                    this.LinklblHides.Visible = true;
                }
                if (tabControl1.SelectedTab == this.tabAdvance)
                {
                    isVisbleLayer("�߼���ѯ");
                    if (this.groupBox2.Visible)
                        this.LinklblHides.Text = "����������";
                    else
                        this.LinklblHides.Text = "��ʾ������";

                    this.LinklblHides.Visible = true;
                    //ͨ�����ƻ�ȡ����
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text.Trim(), getFromNamePath);
                    setFields(CLC.ForSDGA.GetFromTable.TableName, comboField);
                }

                if (tabControl1.SelectedTab == this.tabYintou)
                {
                    isVisbleLayer("��ͷ��ѯ");

                    if (this.groupBox1.Visible)
                        this.LinklblHides.Text = "����������";
                    else
                        this.LinklblHides.Text = "��ʾ������";

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
        //            IMapLayer layer = mapControl1.Map.Layers[strName[i]];      // �������ͼ��
        //            layer.Enabled = flag;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        writeZongheLog(ex, "isKKSearchLayer");
        //    }
        //}

        /// <summary>
        /// ����ģ��������ʾ��ģ���ͼ�㣬���ز��Ǵ�ģ�������ͼ�� 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tabAtils">ģ����</param>
        private void isVisbleLayer(string tabAtils)
        {
            try
            {
                for (int i = 0; i < strName.Length; i++)
                {
                    FeatureLayer feat = (FeatureLayer)mapControl1.Map.Layers[strName[i]];
                    if (strName[i] != tabAtils && feat != null)
                    {
                        IMapLayer layer = mapControl1.Map.Layers[strName[i]];      // �������ͼ��
                        IMapLayer LblLayer = mapControl1.Map.Layers["��עͼ��"];   // ������б�עͼ��
                        if (layer.Type == LayerType.Normal)
                        {
                            layer.Enabled = false;
                            if (tabAtils != "�ܱ߲�ѯ")
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
                            if (lLayer.Sources["��ѯ��"] != null && tabAtils != "�ܱ߲�ѯ")
                               lLayer.Sources["��ѯ��"].Enabled = false;
                        }
                    }
                    else if (strName[i] == tabAtils && feat != null)
                    {
                        IMapLayer layer = mapControl1.Map.Layers[strName[i]];      // �������ͼ��
                        IMapLayer LblLayer = mapControl1.Map.Layers["��עͼ��"];   // ������б�עͼ��
                        if (layer.Type == LayerType.Normal)
                        {
                            layer.Enabled = true;
                            if (tabAtils == "�ܱ߲�ѯ")
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
                            if (tabAtils == "�ܱ߲�ѯ")
                            {
                                if (lLayer.Sources["��ѯ��"] != null)
                                    lLayer.Sources["��ѯ��"].Enabled = true;
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
        /// ����ͳ�ƹ��߳�ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void loadDefault()
        {
            try
            {
                cmbTabName.SelectedIndex = 0;
                cmbType.SelectedIndex = 0;
                dtpStartTime.Text = System.DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                dtpEndTime.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

                string str = "select �ɳ����� from �����ɳ���";
                cmbPic.Items.Clear();
                cmbPic.Items.Add("�����ɳ���");
                addCombox(str, cmbPic);

                str = "select �ж��� from �������ж�";
                cmbZhongdu.Items.Clear();
                cmbZhongdu.Items.Add("�����ж�");
                addCombox(str, cmbZhongdu);

                str = "select �������� from ����������";
                cmbJinWuShi.Items.Clear();
                cmbJinWuShi.Items.Add("���о�����");
                addCombox(str, cmbJinWuShi);

                string strsql = "select username||'------'||��ʵ���� from �û�";
                cmbName.Items.Clear();
                cmbName.Items.Add("--��ѡ���û�--");
                cmbName.Items.Add("�����û�");
                addCombox(strsql, cmbName);

                strsql = "select �û���λ from �û� group by �û���λ";
                cmbDan.Items.Clear();
                cmbDan.Items.Add("--��ѡ��λ--");
                cmbDan.Items.Add("���е�λ");
                addCombox(strsql, cmbDan);
                rdoDan.Checked = true;

                rdoGdw.Checked = true;
                panGdw.Visible = true;
                panUsd.Visible = false;
                panBren.Visible = false;
            }
            catch (Exception ex) { writeZongheLog(ex, "loadDefault-����ͳ�ƹ��߳�ʼ��"); }
        }

        /// <summary>
        /// ���comboBox�ĺ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sqlStr">sql���</param>
        /// <param name="combox">Ҫ����comboBox</param>
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
        /// ��ͷ��ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonQuery_Click(object sender, EventArgs e)
        {
            try
            {
                yinTouQuery();
            }
            catch (Exception ex) { writeZongheLog(ex, "buttonQuery_Click-��ͷ��ѯ"); }
        }

        string YTsql = "";
        /// <summary>
        /// ������ͷ������ѯ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void yinTouQuery()
        {
            try
            {
                dataGridView1.Rows.Clear();
                PageNow1.Text = "0";
                PageCount1.Text = "/ {0}";
                RecordCount1.Text = "0��";
                //removeTemPoints();
                if (this.textYintou.Text.Trim() == "")
                {
                    MessageBox.Show("��ͷ����Ϊ�գ�", "��ʾ");
                    return;
                }
                if (this.comboBox1.Text == "")
                {
                    MessageBox.Show("��ѯ���಻��Ϊ�գ�", "��ʾ");
                    return;
                }

                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;

                YTsql = "select ����,'����...',��_ID,���� from ��ͷ��ѯ�� where YINTOU like '%" + this.textYintou.Text.Trim().ToLower() + "%'";
                if (comboBox1.Text != "ȫ��")
                {
                    YTsql += " and ����='" + comboBox1.Text + "'";
                }
                //alter by siumo 2009-03-10   (edit by fisher in 09-11-26)
                string sRegion = strRegion;
                string sRegion1 = strRegion1;
                string paiNum = "";
                if (strRegion == "")               //add by fisher in 09-12-04
                {
                    isShowPro(false);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("��û�в�ѯȨ�ޣ�");
                    return;
                }
                if (strRegion != "˳����" && strRegion!="")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        sRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    paiNum = getPaiNumber(sRegion.Replace(",", "','"));
                    YTsql += " and �����ɳ������� in ('" + paiNum.Replace(",", "','") + "')";
                }
                
                //alter by siumo 090116
                this.getMaxCount(YTsql.Replace("����,'����...',��_ID,����", "count(*)"));
                InitDataSet(RecordCount1); //��ʼ�����ݼ�
                
                if (nMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

               
                DataTable datatable = LoadData(PageNow1,PageCount1, YTsql); //��ȡ��ǰҳ����
                this.toolPro.Value = 1;
                Application.DoEvents();

                #region �ۺϲ�ѯ����Excel
                YTexcelSql = YTsql.Replace("����,'����...',��_ID,����", "����");
                string sRegion2 = strRegion2;
                string sRegion3 = strRegion3;
                if (strRegion2 == "")
                {
                    YTexcelSql += " and 1=2 ";
                }
                else if (strRegion2 != "˳����")
                {
                    if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
                    {
                        sRegion2 = strRegion2.Replace("����", "����,��ʤ");
                        paiNum = getPaiNumber(sRegion2.Replace(",", "','"));
                    }
                    YTexcelSql += " and �����ɳ������� in ('" + paiNum.Replace(",", "','") + "')";
                }
                exportSql = YTexcelSql;
                //DataTable datatableExcel = LoadData(PageNow1, PageCount1, YTexcelSql);
                //if(dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion

                fillDataGridView(datatable, dataGridView1);  //���datagridview
                this.toolPro.Value = 2;
                //Application.DoEvents();

                drawPointsInMap(datatable,comboBox1.Text.Trim());   //�ڵ�ͼ�ϻ���

                WriteEditLog("��ͷ��ѯ","��ͷ��ѯ��",YTsql,"��ѯ");
                this.Cursor = Cursors.Default;
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (System.Data.OracleClient.OracleException ex)
            {
                isShowPro(false);
                this.Cursor = Cursors.Default;
                MessageBox.Show("��ͷ��ѯʧ�ܣ�\n" + ex.Message, "��ʾ");
                writeZongheLog(ex,"yinTouQuery");
            }
        }

        /// <summary>
        /// ���ɳ�����ת�����ɳ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="pai">Ҫת�����ɳ�����</param>
        /// <returns>�ɳ��������ַ�</returns>
        private string getPaiNumber(string pai)
        {
            string paiNum = "";
            try
            {
                string sql = "select �ɳ������� from �����ɳ��� where �ɳ����� in ('" + pai + "')";
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
                writeZongheLog(ex, "getPaiNumber-���ɳ�����ת�����ɳ�������");
                return paiNum;
            }
        }

        /// <summary>
        /// �����ͼ����ӵĶ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void removeTemPoints()
        {
            try
            {
                //�ҵ���ǰͼ��
                FeatureLayer fl = null;
                if (tabControl1.SelectedTab == this.tabYintou)
                    fl = (FeatureLayer)mapControl1.Map.Layers["��ͷ��ѯ"];
                if (tabControl1.SelectedTab == this.tabZhoubian)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�ܱ߲�ѯ"];
                if (tabControl1.SelectedTab == this.tabDianji)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�����ѯ"];
                if (tabControl1.SelectedTab == this.tabAdvance)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�߼���ѯ"];
                if (fl == null)
                {
                    return;
                }
                Table tableTem = fl.Table;

                //�����ͼ�϶���
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
        /// ���ͼ�в���ͼ�㣬���ܹ����в�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
                //����ط��������ɵ�ͼ��������Map������
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet("Data", this.dt);
                MapInfo.Data.SpatialSchemaXY xy = new MapInfo.Data.SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0, 0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;
                MapInfo.Data.Table tempTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable("�ڴ��");


                foreach (MapInfo.Data.Column col in tempTable.TableInfo.Columns) //���Ʊ�ṹ
                {
                    MapInfo.Data.Column col2 = col.Clone();
                    col2.ReadOnly = false;
                    mainMemTableInfo.Columns.Add(col2);
                }

                this.mainTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(mainMemTableInfo);
                string currentSql = "Insert into " + this.mainTable.Alias + "  Select * From " + tempTable.Alias;//����ͼԪ����
                mapConnection.Open();
                MapInfo.Data.MICommand mapCommand = mapConnection.CreateCommand();
                mapCommand.CommandText = currentSql;
                mapCommand.ExecuteNonQuery();
                currentFeatureLayer = new MapInfo.Mapping.FeatureLayer(this.mainTable, "��ʱ��");

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
                MessageBox.Show("���ͼ�������ʱͼ��ʧ�ܣ�\n" + ex.Message, "��ʾ");
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
        private void setLayerStyle(MapInfo.Mapping.FeatureLayer featureLayer, Color color, short code, int size)//���õ����ʽ
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
                MessageBox.Show("������ʽ����\n" + ex.Message, "��ʾ");
            }
        }
        private int setLayerStyle()//����ÿ��ͼ�����ʽ
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
                MessageBox.Show("������ʽ����\n" + ex.Message, "��ʾ");
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

                if (tableAlies == "��Ƶ_Zonghe")
                {
                    //tableAlies = "�����Ƶ";
                    tableAlies = "��Ƶλ��_Zonghe";
                }
               
                if (check.Checked)//�򿪱�
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (this.OpenTable(tableAlies) == false)
                    {
                        System.Windows.Forms.CheckBox cb = (System.Windows.Forms.CheckBox)sender;
                        cb.Checked = false;
                    }
                    WriteEditLog("�����ѯ", tableAlies,"","��");
                    this.Cursor = Cursors.Default;
                }
                if (!check.Checked)//�رձ�
                {
                    this.RemoveTemLayer(tableAlies);

                    WriteEditLog("�����ѯ", tableAlies, "", "�ر�");
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"checkBox_CheckedChanged");
            }
        }

        /// <summary>
        /// ������ʱͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableAiles">ͼ������</param>
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
                column.Alias = "��_ID";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "����";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "����";
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

                #region ��ӱ�ע
                //string activeMapLabel = "lbl_"+tableAiles;
                //MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableAiles);
                //MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                //lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                //lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                //lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //lbsource.DefaultLabelProperties.Caption = "����";
                //lblayer.Sources.Append(lbsource);
                //mapControl1.Map.Layers.Add(lblayer);
                #endregion
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableAiles);
                labelLayer(activeMapTable, "����");
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "������ʱͼ��");
            }
        }

        /// <summary>
        /// �Ƴ���ʱͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableAlies">ͼ������</param>
        private void RemoveTemLayer(string tableAlies)
        {
            try
            {
                if (this.mapControl1.Map.Layers[tableAlies] != null)
                {
                    this.mapControl1.Map.Layers.Remove(tableAlies);
                    MapInfo.Engine.Session.Current.Catalog.CloseTable(tableAlies);

                    if (tableAlies == "�����Ƶ_Zonghe")
                    {
                        this.RemoveTemLayer("�������Ƶ_Zonghe");
                        this.RemoveTemLayer("������Ƶ_Zonghe");
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"RemoveTemLayer");
            }
        }

        //�򿪱�
        MapInfo.Mapping.FeatureLayer currentFeatureLayer;
        /// <summary>
        /// �򿪱�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableAlies">����</param>
        /// <returns>��������</returns>
        private bool OpenTable(string tableAlies)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(tableAlies.Substring(0,tableAlies.LastIndexOf('_')), getFromNamePath);

                string strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName;
                if (tableAlies == "������Ƶ_Zonghe")
                {
                    strSQL = "select CAMID as �豸���, �ն˳������� as �豸����,������λ as �����ɳ���,null as �ճ�������,null as MAPID,null as �豸ID, X,Y from gps������λϵͳ where CAMID is not null and X>0 and Y >0 ";
                }
                if (tableAlies == "��ȫ������λ_Zonghe")
                {
                    strSQL = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ��λ���� ,'����鿴' as �ļ� ,X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ���� from " + CLC.ForSDGA.GetFromTable.TableName;
                }

                if (CLC.ForSDGA.GetFromTable.XiaQuField != "" &&  strRegion != "˳����" && strRegion != "")
                {//���ڿ�ʱ,˵��������ֶ�,��ѯȫ��
                    string sRegion = strRegion;
                    if (strRegion != "˳����")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "����") > -1) //�������滻Ϊ����,��ʤ;�������û��ɲ��ʤ
                        {
                            sRegion = strRegion.Replace("����", "����,��ʤ");
                        }
                        if (tableAlies == "������Ƶ_Zonghe")
                        {
                            strSQL = "select CAMID as �豸���, �ն˳������� as �豸����,������λ as �����ɳ���,null as �ճ�������,null as MAPID,null as �豸ID, X,Y from gps������λϵͳ where CAMID is not null and X>0 and Y >0 and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName + " where " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "')";
                        }
                    }
                }
                if (strRegion == "")
                {
                    if (tableAlies == "��������_Zonghe" && strRegion1 != "")
                    {
                        strSQL += " where " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
                    }
                    else
                    {
                        MessageBox.Show("�޲�ѯ��¼", "��ʾ");
                        return false; 
                    }
                }

                //if(tableAlies =="����"||tableAlies=="�˿�"||tableAlies =="�����ݷ���"){
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

                    // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                    if (tableAlies != "��Ա_Zonghe" && tableAlies != "��Ƶλ��_Zonghe")   // ��ԱΪ��ͼû�б����ֶ�һ
                    {
                        if (strSQL.IndexOf("where") >= 0)
                            strSQL += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                        else
                            strSQL += " where (�����ֶ�һ is null or �����ֶ�һ='')";
                    }
                    //-----------------------------------------------------------------------

                    DataTable datatable = linkData.SelectDataBase(strSQL);
                    if (datatable == null || datatable.Rows.Count < 1)
                    {
                        MessageBox.Show("�޲�ѯ��¼","��ʾ");
                        return false;
                    }
                    //����ط��������ɵ�ͼ��������Map������
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

                    //ͨ�������ƻ�ȡͼ��
                    string bmpName = "";
                    bmpName = CLC.ForSDGA.GetFromTable.BmpName;

                    if (tableAlies == "�������Ƶ_Zonghe")
                    {
                        this.setLayerStyle(currentFeatureLayer, "TARG1-32.BMP", Color.Red, 12);
                    }
                    else if (tableAlies == "������Ƶ_Zonghe")
                    {
                        this.setLayerStyle(currentFeatureLayer, "ydsp.BMP", Color.Red, 30);
                    }else
                    {
                        this.setLayerStyle(currentFeatureLayer, bmpName, Color.Red, 12);
                    }
                    labelLayer(currentFeatureLayer.Table, CLC.ForSDGA.GetFromTable.ObjName);

                    if (tableAlies == "�����Ƶ_Zonghe")
                    {
                        OpenTable("�������Ƶ_Zonghe");
                        OpenTable("������Ƶ_Zonghe");
                    }
                //}
                return true;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"OpenTable");
                MessageBox.Show("�����ڣ�","��ʾ");
                return false;
            }
        }

        /// <summary>
        /// ����ϴβ�ѯ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.textYintou.Text = "";  //�����ͷ
            this.dataGridView1.Rows.Clear();   //����б�
            PageNow1.Text = "0";
            PageCount1.Text = "/ {0}";
            RecordCount1.Text = "0��";
            //this.labelCount1.Visible = false;

            removeTemPoints();  //�����ʱ��ӵĵ�
            WriteEditLog("��ͷ��ѯ", "��ͷ��ѯ��", "", "�����ѯ��¼");
        }

        /// <summary>
        /// ��datatable���������꽨��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dt2">���ݱ�</param>
        private void addPoints(DataTable dt2)
        {
            try
            {
                FeatureLayer fl = null;
                if (this.tabControl1.SelectedTab == this.tabAdvance)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�߼���ѯ"];
                if (this.tabControl1.SelectedTab == this.tabZhoubian)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�ܱ߲�ѯ"];
                if(this.tabControl1.SelectedTab == this.tabDianji)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�����ѯ"];
                if (this.tabControl1.SelectedTab == this.tabYintou)
                    fl = (FeatureLayer)mapControl1.Map.Layers["��ͷ��ѯ"];

                Table tableTem = fl.Table;
                
                //��������ж��� 
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);

                double dx = 0, dy = 0;
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    //���������ֵ,�����˼�¼
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

                        CLC.ForSDGA.GetFromTable.GetFromName(dt2.Rows[i]["����"].ToString(), getFromNamePath);
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
                            if (dt2.Rows[i]["����"].ToString() == "��Ƶλ��VIEW")
                            {
                                string styid = dt2.Rows[i]["����"].ToString();
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
                        pFeat["��_ID"] = dt2.Rows[i]["��_ID"].ToString();
                        pFeat["����"] = dt2.Rows[i]["����"].ToString();
                        pFeat["����"] = dt2.Rows[i]["����"].ToString();
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
        /// ��DataGridView����ʾ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="��_ID">��_ID</param>
        /// <param name="����">����</param>
        public  void showDataGridViewLineOnlyOneTable(string ��_ID,string ����)
        {
            try
            {
                if (this.tabControl1.SelectedTab == this.tabYintou)
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        if ((this.dataGridView1.Rows[i].Cells["��_ID"].Value.ToString() == ��_ID) && (this.dataGridView1.Rows[i].Cells["tabName"].Value.ToString() == ����))
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
                //        if ((this.dataGridView4.Rows[i].Cells["colMapID4"].Value.ToString() == ��_ID)&&(this.dataGridView4.Rows[i].Cells["colTableName4"].Value.ToString()==����))
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
                        if ((this.dataGridView5.Rows[i].Cells["��_ID"].Value.ToString() == ��_ID) && (this.dataGridView5.Rows[i].Cells["����"].Value.ToString() == ����))
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
                        if ((this.dataGridView2.Rows[i].Cells["��_ID"].Value.ToString() == ��_ID) && (this.dataGridView2.Rows[i].Cells["����"].Value.ToString() == ����))
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
        /// ͼԪѡ���¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
                    if (f.Table.Alias == "�߼���ѯ_selection" || f.Table.Alias == "�����ѯ_selection" 
                                                              || f.Table.Alias == "�ܱ߲�ѯ_selection" 
                                                              || f.Table.Alias == "��ͷ��ѯ_selection")
                    {
                        this.showDataGridViewLineOnlyOneTable(f["��_ID"].ToString(), f["����"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    writeZongheLog(ex,"Feature_Selected");
                }
            }
        }

        string GJsql = "";
        //-----��ҳ��ȫ�ֱ���----//
        int _startNo = 1;    // ��ʼ����
        int _endNo = 0;      // ��������
        //------------------------
        private clKaKou.ucKakou uckakou = null;
        private DataTable tablePage = null;
        /// <summary>
        /// �߼���ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonMultiOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    MessageBox.Show("����Ӳ�ѯ���!", "��ʾ");
                    return;
                }

                if (getSqlString() == "")
                {
                    MessageBox.Show("��ѯ����д���,������!", "��ʾ");
                    return;
                }
                ClearEvent(comboTable.Text.Trim());������// ���ԭ���¼�������¼�
                // ��ʾ��ѯ������
                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;
                dataGridView5.DataSource = null;
                PageNow4.Text = "0";
                PageCount4.Text = "/ {0}";
                RecordCount4.Text = "0��";
                _endNo = Convert.ToInt32(this.TextNum4.Text);
                _startNo = 1;
                GJsql = "";
                GJexcelSql = "";
                //this.removeTemPoints();

                //ͨ�����ƻ�ȡ������������
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                string tabName = CLC.ForSDGA.GetFromTable.TableName;

                if (comboTable.Text.Trim() == "��Ϣ��")
                {
                    #region ��Ϣ���ѯ
                    // �����ڴ��洢��ѯ���
                    DataTable dt = new DataTable();
                    DataColumn dc = new DataColumn("���", System.Type.GetType("System.Int32"));
                    dt.Columns.Add(dc);
                    DataColumn dc1 = new DataColumn("����", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc1);
                    DataColumn dc2 = new DataColumn("X", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc2);
                    DataColumn dc3 = new DataColumn("Y", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc3);
                    DataColumn dc4 = new DataColumn("�����", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc4);
                    DataColumn dc5 = new DataColumn("����", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc5);

                    // �ҵ���Ϣ��ͼ��
                    FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["��Ϣ��"];
                    Table table = fl.Table;
                    IResultSetFeatureCollection mFeatCol = MapInfo.Data.FeatureCollectionFactory.CreateResultSetFeatureCollection(table, table.TableInfo.Columns);

                    string sExpress = this.getSqlString();

                    // ��ѯ����������������ڴ��
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
                            row[5] = "��Ϣ��";
                            dt.Rows.Add(row);
                        }
                        this.toolPro.Value = 2;
                        nMax = mFeatCol.Count;
                        InitDataSet(RecordCount4);
                        LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, dataGridView5, dt);
                        tablePage = dtExcel = dt;
                        this.dataGridView5.Columns["�����"].Visible = false;
                        this.dataGridView5.Columns["����"].Visible = false;
                    }
                    else
                    {
                        isShowPro(false);
                        MessageBox.Show("�޲�ѯ���", "���");
                    }
                    WriteEditLog("�ܱ߲�ѯ", "��Ϣ��", sExpress, "��λ���ĵ�");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                    this.Cursor = Cursors.Default;
                    return;
                    #endregion
                }
                else if (comboTable.Text.Trim() == "ͨ��������ѯ")
                {
                    #region ͨ��������ѯ
                    isShowPro(false);
                    string NewSql = this.getSqlString();    // �������������
                    uckakou = new clKaKou.ucKakou(mapControl1, conStr, toolStriplbl, toolSbutton, videop, videoConnstring, videoexepath, KKAlSys, KKALUser, KKSearchDist, stringϽ��, user, getFromNamePath,false);

                    DataTable table  = uckakou.GetPassCar(NewSql);        // ͨ�����ڻ�ȡ����
                    nMax = table.Rows.Count;
                    InitDataSet(RecordCount4);          // ��ҳ����
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, dataGridView5, table);

                    tablePage = table;

                    WriteEditLog("ͨ��������ѯ", "V_Cross", NewSql, "��λ���ĵ�");

                    this.Cursor = Cursors.Default;
                    return;
                    #endregion
                }
                //add by siumo 2008-12-30
                GJsql = this.getSqlString();
                string newSQl = "select count(*) from " + CLC.ForSDGA.GetFromTable.TableName + " where " + this.getSqlString();
                string sRegion = strRegion;
                if (strRegion != "˳����")   // edit by fisher in 09-12-08
                {
                    if (strRegion != "")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                        {
                            sRegion = strRegion.Replace("����", "����,��ʤ");
                        }
                        GJsql += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "'))";
                        newSQl += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "'))";

                        if (strRegion1 != "" && (tabName == "������Ϣ" || tabName == "��������"))
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
                        if (strRegion1 != "" && (tabName == "������Ϣ" || tabName == "��������"))
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
                            MessageBox.Show("��û�в�ѯȨ��!");
                            return;
                        }
                    }                    
                }
                // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                if (comboTable.Text.Trim() != "��Ա" && comboTable.Text.Trim() != "��Ƶ")
                {
                    newSQl += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                    GJsql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                }
                //-------------------------------------------------------
                this.getMaxCount(newSQl);
                InitDataSet(RecordCount4); //��ʼ�����ݼ�
                this.toolPro.Value = 1;
                Application.DoEvents();
                if (nMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

                DataTable datatable = LoadData(_startNo, _endNo, GJsql, tabName,false); //��ȡ��ǰҳ����
                this.dataGridView5.DataSource = datatable;
                this.dataGridView5.Columns[datatable.Columns.Count - 1].Visible = false;
                this.dataGridView5.Columns[datatable.Columns.Count - 2].Visible = false;

                for (int i = 0; i < dataGridView5.Rows.Count; i++)
                {
                    if (comboTable.Text.Trim() == "��ȫ������λ")
                    {
                        // ����ȫ������λ���ļ���������
                        DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                        dgvlc.Value = "����鿴";
                        dgvlc.ToolTipText = "�鿴��ȫ������λ�ļ�";
                        dataGridView5.Rows[i].Cells["�ļ�"] = dgvlc;
                    }
                    if (i % 2 == 1)
                    {
                        dataGridView5.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                this.toolPro.Value = 2;

                #region �߼���ѯ����Excel

                GJexcelSql += GJsql;
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
                // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ lili 2010-8-19
                if (comboTable.Text.Trim() != "��Ա" && comboTable.Text.Trim() != "��Ƶ")
                    GJexcelSql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                //-------------------------------------------------------
                LoadData(_startNo, _endNo, GJexcelSql, tabName, true);

                //DataTable datatableExcel = LoadData(_startNo, _endNo, GJexcelSql, tabName ,true); 
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion

                //ͨ�����ƻ�ȡ������������
                drawPointsInMap(datatable,CLC.ForSDGA.GetFromTable.TableName);   //�ڵ�ͼ�ϻ���
                WriteEditLog("�߼���ѯ", tabName, GJsql, "��ѯ");
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
        /// ���ԭ���¼�������¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="eventName">Ҫ�����¼�ģ��</param>
        public void ClearEvent(string eventName)
        {
            try
            {
                if (eventName == "��Ϣ��" || eventName == "ͨ��������ѯ")      // ���ڷ�ҳ�ķ�ʽ��ͬ���Բ�ͬ��ѡ��ͬ�¼����з�ҳ
                {
                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator4_ItemClicked);  // �����֮ǰ�󶨵��¼�
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(TextNum4_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNow4_KeyPress);

                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator_ItemClicked);   // �˴���Ϊ�˱����ظ���������һ��
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(PageNumber_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNowText_KeyPress);

                    this.bindingNavigator4.ItemClicked += new ToolStripItemClickedEventHandler(bindingNavigator_ItemClicked);   // ��������¼�
                    this.TextNum4.KeyPress += new KeyPressEventHandler(PageNumber_KeyPress);
                    this.PageNow4.KeyPress += new KeyPressEventHandler(PageNowText_KeyPress);
                }
                else
                {
                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator_ItemClicked);   // �����֮ǰ�󶨵��¼�
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(PageNumber_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNowText_KeyPress);

                    this.bindingNavigator4.ItemClicked -= new ToolStripItemClickedEventHandler(bindingNavigator4_ItemClicked);  // �˴���Ϊ�˱����ظ���������һ��
                    this.TextNum4.KeyPress -= new KeyPressEventHandler(TextNum4_KeyPress);
                    this.PageNow4.KeyPress -= new KeyPressEventHandler(PageNow4_KeyPress);
��
                    this.bindingNavigator4.ItemClicked += new ToolStripItemClickedEventHandler(bindingNavigator4_ItemClicked); // ��������¼�
                    this.TextNum4.KeyPress += new KeyPressEventHandler(TextNum4_KeyPress);
                    this.PageNow4.KeyPress += new KeyPressEventHandler(PageNow4_KeyPress);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("�������쳣��������������ٳ��Դ˲�����","��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                CLC.BugRelated.ExceptionWrite(ex, "clZonghe-ucZonghe-ClearEvent");
            }
        }

        #region �ѷ�������
        /// <summary>
        /// ����ѯ�����ʾ�ڵ�ͼ��
        /// </summary>
        /// <param name="datatable"></param>
        /// <param name="tableName"></param>
        //private void DrawPoints(DataTable dt)
        //{
        //    try
        //    {
        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            //�����ͼ����ӵĶ���
        //            FeatureLayer lyrcar = (FeatureLayer)mapControl1.Map.Layers["�߼���ѯ"];
        //            if (lyrcar == null)
        //            {
        //                return;
        //            }
        //            Table tblcar = lyrcar.Table;

        //            //��������ж���
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

        //                idname = dr["ͨ�г������"].ToString();
        //                kkname = dr["���ڱ��"].ToString();
        //                CarName = dr["��������"].ToString();
        //                //////��ȡ
        //                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
        //                DataTable dt1 = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select X,Y from �ΰ�����ϵͳ where ���ڱ��='" + kkname + "'");

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
        //                    ftr["����"] = CarName;
        //                    ftr["��_ID"] = idname;
        //                    ftr["����"] = "����ͨ����Ϣ";
        //                    tblcar.InsertFeature(ftr);

        //                    if (px != 0 && py != 0 && Cx != 0 && Cy != 0 && textValue.Text.Trim() != "�޺���" && this.comboYunsuanfu.Text == "����")
        //                        Trackline(px, py, Cx, Cy);

        //                    px = Cx;
        //                    py = Cy;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        writeZongheLog(ex, "DrawPoints-����ѯ�����ʾ�ڵ�ͼ��");
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

        private bool fuzzyFlag = false;         // �ڿ�������ʱ�����ж��Ƿ��ǳ���ģ����ѯ lili 2010-12-21

        /// <summary>
        /// �������ڲ�ѯͼ�㲢�����ڻ��Ƶ���ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dt">������Ϣ</param>
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
                //    //������ʱ��
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

                //    //��ӱ�ע
                //    string activeMapLabel = "KKSearchLabel";
                //    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                //    MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //    MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //    lbsource.DefaultLabelProperties.Style.Font.Name = "����";
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
                //MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["�߼���ѯ"] as FeatureLayer;
                //Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("�߼���ѯ");
                //this.SetLayerEdit("KKSearchLayer");   // ���ø�ͼ��ɱ༭
                #endregion

                //�����ͼ����ӵĶ���
                FeatureLayer lyrcar = (FeatureLayer)mapControl1.Map.Layers["�߼���ѯ"];
                if (lyrcar == null)
                {
                    return;
                }
                Table tblKKTRA = lyrcar.Table;

                //��������ж���
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
                    //ͨ�����ڱ�Ż�ȡ���ڵľ�γ��
                    string KaID = dr["���ڱ��"].ToString().Trim();    // ���ڱ��
                    string id = dr["ͨ�г������"].ToString();         // ͨ�г������
                    string tnum = dr["���к�"].ToString();             // ���к�

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

                    DataTable dt1 = GetTable("select X,Y from �ΰ�����ϵͳ where ���ڱ��='" + KaID + "'");
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
                        ftr["��_ID"] = tnum;
                        ftr["����"] = tnum;       // ���к�
                        ftr["����"] = "V_CROSS";
                        tblKKTRA.InsertFeature(ftr);

                        if (px != 0 && py != 0 && x != 0 && y != 0 && textValue.Text.Trim() != "�޺���" && fuzzyFlag)
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
                writeZongheLog(ex, "CreateKakouTrack-�������ڲ�ѯͼ��");
            }
        }

        /// <summary>
        /// ���������ΰ�����ʱ�Ĺ켣��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="x1">�ϴεľ���</param>  
        /// <param name="y1">�ϴε�γ��</param>  
        /// <param name="x2">���εľ���</param>  
        /// <param name="y2">���ε�γ��</param>  
        private void Trackline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = null;
                MapInfo.Data.Table tblTemp = null;

                if (this.mapControl1.Map.Layers["�߼���ѯ"] != null)
                {
                    workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["�߼���ѯ"];
                    tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("�߼���ѯ");
                }

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                // 2 û�м�ͷ
                //54 �����ͷ
                // 59 ������ͷ
                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(2, MapInfo.Styles.LineWidthUnit.Pixel), 59, System.Drawing.Color.Red);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "Trackline-���������ΰ�����ʱ�Ĺ켣��");
            }
        }

        string[] KKiD;
        Int32[] KKnum;
        /// <summary>
        /// ��ȡ���ڱ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void InitUsedFlag()
        {
            try
            {
                DataTable dt = GetTable("Select * from �ΰ�����ϵͳ");
                if (dt.Rows.Count > 0)
                {
                    KKiD = new string[dt.Rows.Count];
                    KKnum = new Int32[dt.Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string camid = dr["���ڱ��"].ToString();
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
        /// ����������ʽ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                this.dataGridView5.DataSource = null;
                PageNow4.Text = "0";
                PageCount4.Text = "/ {0}";
                RecordCount4.Text = "0��";
                this.textValue.Text = "";
                this.comboTable.Enabled = true;
                removeTemPoints();
                dataGridViewValue.Rows.Clear();
                clearData();
                WriteEditLog("�߼���ѯ", "", "", "��ղ�ѯ��¼,���ò�ѯ����");
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "buttonClear_Click");
            }
        }

        /// <summary>
        /// ҳ�浼��(�˺���Ϊ��Ϣ�㼰ͨ��������ѯ��ҳʹ��)
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void bindingNavigator_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent--;
                    if (pageCurrent < 1)
                    {
                        pageCurrent = 1;
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��");
                        return;
                    }
                    else
                    {
                        nCurrent = pageSize * (pageCurrent - 1);
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent++;
                    if (pageCurrent > pageCount)
                    {
                        pageCurrent = pageCount;
                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��");
                        return;
                    }
                    else
                    {
                       nCurrent = pageSize * (pageCurrent - 1);
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = pageSize * (pageCurrent - 1);
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = pageCount;
                        nCurrent = pageSize * (pageCurrent - 1);
                    }
                    LoadData2(PageNow4, PageCount4, bindingSource1, bindingNavigator4, this.dataGridView5, tablePage);
                }
                //else if (e.ClickedItem.Text == "���ݵ���")
                //{
                //    DataExport();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeZongheLog(ex, "bindingNavigator_ItemClicked");
            }
        }

        /// <summary>
        /// ����ÿҳ��ʾ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void PageNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && this.dataGridView5.Rows.Count > 0)
                {
                    pageSize = Convert.ToInt32(this.TextNum4.Text);
                    pageCurrent = 1;   //��ǰת����һҳ
                    nCurrent = pageSize * (pageCurrent - 1);
                    pageCount = (nMax / pageSize);//�������ҳ��
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
        /// ҳ��ת��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void PageNowText_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(this.PageNow4.Text) < 1 || Convert.ToInt32(this.PageNow4.Text) > pageCount)
                    {
                        System.Windows.Forms.MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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

        //+ѡ��Ԫ���¼�
        FrmZLMessage frmZL;
        //��˸����
        private Feature flashFt;
        private Style defaultStyle;
        private int k,rowIndex;
        /// <summary>
        /// ����б�λ����ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;   //�����ͷ,�˳�

                DataGridView dataGV = (DataGridView)sender;
                int tabNum = dataGV.Rows[0].Cells.Count - 1;
                int idNum = dataGV.Rows[0].Cells.Count - 2;
                DPoint dp = new DPoint();

                if (tabControl1.SelectedTab == tabAdvance)
                    dataGridView5.Rows[e.RowIndex].Selected = true;
                if (tabControl1.SelectedTab == tabZhoubian)
                    dataGridView2.Rows[e.RowIndex].Selected = true;

                //������࣬��ʾ��ϸ��Ϣ�������ͼ��λ
                if ((e.ColumnIndex == dataGV.Columns.Count - 5 && comboTable.Text.Trim() == "��ȫ������λ") || (e.ColumnIndex == dataGV.Columns.Count - 5 && comboClass.Text.Trim() == "��ȫ������λ"))
                {
                    #region ��ȫ������λ����
                    if (dataGV.Rows[e.RowIndex].Cells[dataGV.Columns.Count - 5].Value.ToString() != "����鿴") return;

                    if (dataGV == dataGridView5 || dataGV == dataGridView2)
                    {
                        if (this.frmZL != null)
                        {
                            if (this.frmZL.Visible == true)
                            {
                                this.frmZL.Close();
                            }
                        }

                        if (dataGV.Rows[dataGV.CurrentRow.Index].Cells["����"].Value.ToString() == "")
                        {
                            MessageBox.Show("���Ʋ���Ϊ�գ�", "��ʾ");
                            return;
                        }
                        this.frmZL = new FrmZLMessage(dataGV.Rows[dataGV.CurrentRow.Index].Cells["����"].Value.ToString(), strConn, dtEdit);

                        //this.frmZL.SetDesktopLocation(Control.MousePosition.X + 50, Control.MousePosition.Y - this.frmZL.Height / 2);
                        //������Ϣ�������½�
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
                    fl = (FeatureLayer)mapControl1.Map.Layers["�߼���ѯ"];
                if (this.tabControl1.SelectedTab == this.tabZhoubian)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�ܱ߲�ѯ"];
                if (this.tabControl1.SelectedTab == this.tabDianji)
                    fl = (FeatureLayer)mapControl1.Map.Layers["�����ѯ"];
                if (this.tabControl1.SelectedTab == this.tabYintou)
                    fl = (FeatureLayer)mapControl1.Map.Layers["��ͷ��ѯ"];
                Table table = fl.Table;

                Feature ft = null;
                if (dataGV.Rows[e.RowIndex].Cells[tabNum].Value.ToString() == "��Ϣ��")
                {
                    #region ��Ϣ������չʾ
                    rowIndex = e.RowIndex;
                    FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["��Ϣ��"];
                    Table tableInfo = fLayer.Table;

                    MapInfo.Data.Feature feat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableInfo.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("OBJECTID=" + Convert.ToInt32(dataGV.Rows[e.RowIndex].Cells[idNum].Value)));

                    //��������ж���
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
                else if (comboTable.Text.Trim() == "ͨ��������ѯ" && this.tabControl1.SelectedTab == tabAdvance)
                {
                    #region ͨ��������ѯ����չʾ
                    MapInfo.Data.MIConnection conn = new MIConnection();
                    try
                    {
                        int lastCell = dataGV.CurrentRow.Cells.Count - 1;          // ���һ�е�λ��
                        string serNum = dataGV.CurrentRow.Cells[lastCell].Value.ToString();  // ���к�
                        string tempname = dataGV.CurrentRow.Cells[0].Value.ToString();
                        string KaIDnu = dataGV.CurrentRow.Cells[1].Value.ToString();

                        string tblname = "�߼���ѯ";

                        MapInfo.Mapping.Map map = this.mapControl1.Map;

                        if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                        {
                            return;
                        }
                        rsfcflash = null;

                        DataRow[] row = dataZhonghe.Select("���к�='" + serNum + "'");
                        picturePoint(row[0]);                                              // �����µı��

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
                        writeZongheLog(ex, "dataGridView_CellClick-�����¼�");
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
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(table.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("��_ID='" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString() + "' and ����='" + dataGV.Rows[e.RowIndex].Cells[tabNum].Value.ToString() + "'"));
                }
                if (ft != null)
                {
                    dp = ft.Geometry.Centroid;
                    //��˸Ҫ��
                    flashFt = ft;
                    defaultStyle = ft.Style;
                    k = 0;
                    timer1.Start();

                    // ���´�����������ǰ��ͼ����Ұ�������ö������ڵ��ɳ���
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
        /// �ı䵱ǰ���ھ����ĳ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dr">��¼��</param>
        private void picturePoint(DataRow dr)
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["�߼���ѯ"] as FeatureLayer;
                Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("�߼���ѯ");
                //this.SetLayerEdit("KKSearchLayer");   // ���ø�ͼ��ɱ༭

                double x = 0;
                double y = 0;

                int ci = 0;

                //ͨ�����ڱ�Ż�ȡ���ڵľ�γ��
                string KaID = dr["���ڱ��"].ToString().Trim();    // ���ڱ��
                string id = dr["ͨ�г������"].ToString();         // ͨ�г������
                string tnum = dr["���к�"].ToString();             // ���к�
                string KaName = dr["��������"].ToString();         // ��������

                DataTable dt1 = GetTable("select X,Y from �ΰ�����ϵͳ where ���ڱ��='" + KaID + "'");
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
                    if (KaName == dataZhonghe.Rows[k]["��������"].ToString().Trim())
                    {
                        getFeatureCollection(dataZhonghe.Rows[k]["���к�"].ToString().Trim(), "�߼���ѯ");
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
                                f["��_ID"] = tnum;       // �����кŴ��桮ͨ�г�����š� ��ͨ�г�����š����ظ�
                                f["����"] = tnum;        // ���к�
                                f["����"] = "V_CROSS";   // ����
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
        /// ����ͼ���ҵ���ͼ���ϵĵ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="fearID">��ID</param>
        /// <param name="tblname">ͼ����</param>
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
                cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where ��_ID = @name ";
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

                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["�߼���ѯ"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("�߼���ѯ");


                double Cx = 0;
                double Cy = 0;

                //////��ȡ
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable dt1 = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select X,Y from �ΰ�����ϵͳ where ���ڱ��='" + kkname + "'");

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
                    ftr["����"] = CarName;
                    ftr["MapID"] = idname;
                    tblcar.InsertFeature(ftr);

                    string tempname = idname;

                    string tblname = "�߼���ѯ";


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
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where ��_ID = @name ";
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
        /// ��ȡ���ű���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <returns>���ű���ֵ</returns>
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
                writeZongheLog(ex, "getScale-��ȡ���ű���");
                return 0;
            }
        }
        private IResultSetFeatureCollection rsfcflash = null;
        /// <summary>
        /// ˫���鿴��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;   //�����ͷ,�˳�

                DataGridView dataGV = (DataGridView)sender;
                int tabNum = dataGV.Rows[0].Cells.Count - 1;
                int idNum = dataGV.Rows[0].Cells.Count - 2;
                DataTable objTable = null;
                DPoint dp = new DPoint();

                string tabName = dataGV.Rows[e.RowIndex].Cells[tabNum].Value.ToString().Trim();
                if (tabName == "��Ϣ��")
                {
                    #region ��Ϣ����ʾ��ϸ��Ϣ
                    try
                    {
                        DataTable dt = new DataTable();
                        DataColumn dc = new DataColumn("���", System.Type.GetType("System.String"));
                        dt.Columns.Add(dc);
                        DataColumn dc1 = new DataColumn("����", System.Type.GetType("System.String"));
                        dt.Columns.Add(dc1);
                        DataColumn dc2 = new DataColumn("X", System.Type.GetType("System.Double"));
                        dt.Columns.Add(dc2);
                        DataColumn dc3 = new DataColumn("Y", System.Type.GetType("System.Double"));
                        dt.Columns.Add(dc3);

                        DataRow row = dt.NewRow();
                        if (this.tabControl1.SelectedTab == this.tabYintou)
                        {
                            row[0] = dataGV.Rows[e.RowIndex].Cells["��_ID"].Value.ToString();
                            row[1] = dataGV.Rows[e.RowIndex].Cells[1].Value.ToString();
                            FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["��Ϣ��"];
                            Table tableInfo = fLayer.Table;
                            MapInfo.Data.Feature feat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableInfo.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("OBJECTID=" + dataGV.Rows[e.RowIndex].Cells["��_ID"].Value.ToString()));
                            row[2] = feat.Geometry.Centroid.x;
                            row[3] = feat.Geometry.Centroid.y;
                        }
                        else
                        {
                            row[0] = dataGV.Rows[e.RowIndex].Cells["�����"].Value.ToString();
                            row[1] = dataGV.Rows[e.RowIndex].Cells["����"].Value.ToString();

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

                            this.disPlayInfo(dt, ptZ, "�߼���ѯ");
                            return;
                        }
                        mapControl1.Map.DisplayTransform.ToDisplay(dp, out ptZ);
                        ptZ.X += this.Width + 10;
                        ptZ.Y += 80;

                        this.disPlayInfo(dt, ptZ, "�߼���ѯ");
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "��ʾ");
                        return;
                    }
                    #endregion
                }
                if (comboTable.Text == "ͨ��������ѯ" && tabControl1.SelectedTab == tabAdvance)
                {
                    #region ͨ��������ѯ��ʾ��ϸ��Ϣ
                    string[] sqlFields ={ "ͨ�г������", "���ڱ��", "��������", "ͨ��ʱ��", "��������", "��������", "������ɫ", "��ɫ��ǳ", "��ʻ����", "��Ƭ1", "��Ƭ2", "��Ƭ3" };

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

                    /////////���ݵ�ǰͨ�г�������ж�ͼƬ��������ַ
                    try
                    {
                        if (dataGV.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "01")   //�ߵ�����Ƶ��ͼƬ��������ַ
                        {
                            photoserver = uckakou.gdwserver;
                        }
                        else if (dataGV.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "02") //�׻�¼��Ƶ��ͼƬ��������ַ
                        {
                            photoserver = uckakou.ehlserver;
                        }
                        else if (dataGV.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "03")  //������Ƶ��ͼƬ��������ַ
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
                        writeZongheLog(ex, "����ͨ����Ϣ����ip��ַ��ȡͼƬ��������ַ");
                    }

                    MapInfo.Geometry.DPoint dpoint = new MapInfo.Geometry.DPoint();
                    if (objTable != null && objTable.Rows.Count > 0)
                    {
                        //////////////////////////////////////////
                        string tempname = dataGV.CurrentRow.Cells[0].Value.ToString();

                        int lastCell = dataGV.CurrentRow.Cells.Count - 1;          // ���һ�е�λ��
                        string serNum = dataGV.CurrentRow.Cells[lastCell].Value.ToString();  // ���к�
                        string tblname = "�߼���ѯ";

                        //��ȡ��ǰѡ�����Ϣ��ͨ�г��������Ϊ����ֵ

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
                            System.Windows.Forms.MessageBox.Show("�˶���δ��λ!");
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
                //ͨ�������ƻ�ȡ�����Ϣ
                //GetFromName getFromName = new GetFromName(tabName.ToUpper());
                CLC.ForSDGA.GetFromTable.GetFromName(tabName.ToUpper(), getFromNamePath);

                string objID = CLC.ForSDGA.GetFromTable.ObjID;

                string strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + tabName + " t where " + objID + "='" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString() + "'";
                if (tabName == "��ȫ������λ")
                {
                    strSQL = "select ���,��λ����,��λ����,��λ��ַ,���ܱ�������������,�ֻ�����,�����ɳ���,�����ж�,����������,'����鿴' as ��ȫ������λ�ļ�,��ע��,��עʱ��,X,Y from " + tabName + " t where " + objID + "='" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString() + "'";
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
                        this.disPlayInfo(datatable, pt, "�߼���ѯ");
                    else
                        MessageBox.Show("��ѯ�޴˼�¼!\n��ȷ�����ݿ���ͷ��ѯ���еı��!", "��ʾ");
                    return;
                }

                if (dp.x == 0 || dp.y == 0)
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2;
                    pt.Y = 10;

                    this.disPlayInfo(datatable, pt, "�߼���ѯ");
                    return;
                }
                mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                pt.X += this.Width + 10;
                pt.Y += 80;

                this.disPlayInfo(datatable, pt, "�߼���ѯ");

                string sModule = "";
                if (tabControl1.SelectedTab == tabYintou)
                {
                    sModule = "��ͷ��ѯ";
                }
                else if (tabControl1.SelectedTab == tabDianji)
                {
                    sModule = "�����ѯ";
                }
                else if (tabControl1.SelectedTab == tabZhoubian)
                {
                    sModule = "�ܱ߲�ѯ";
                }
                else if (tabControl1.SelectedTab == tabDianji)
                {
                    sModule = "��λ��ѯ";
                }
                else
                {
                    sModule = "�߼���ѯ";
                }
                WriteEditLog(sModule, CLC.ForSDGA.GetFromTable.TableName, objID + "=" + dataGV.Rows[e.RowIndex].Cells[idNum].Value.ToString(), "�鿴����");
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "dataGridView_CellDoubleClick");
                MessageBox.Show(ex.Message, "��ʾ");
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
        /// ��ʾͨ��������ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="pt">��ʾλ��</param>
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
                frminfo.layerName = "�߼���ѯ";
                frminfo.getFromNamePath = this.getFromNamePath;
                frminfo.setInfo(dt.Rows[0], pt, conStr, user);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "disPlayInfo-��ʾ������ϸ��Ϣ");
            }
        }

        private FrmInfo frmMessage = new FrmInfo();
        /// <summary>
        /// ��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="pt">��ʾλ��</param>
        /// <param name="LayerName">ͼ����</param>
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

        #region �����ص㵥λ
        //private void searchDanwei()
        //{
        //    try
        //    {
        //        dataGridView4.Rows.Clear();
        //        //removeTemPoints();

        //        ZDsql = "select X, Y , ��λ���� as ���� , ��š�as  ��_ID, '��ȫ������λ' as ����  from ��ȫ������λ where X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2;
        //        ZDsql += " and Y>" + mapControl1.Map.Bounds.y1 + " and Y<" + mapControl1.Map.Bounds.y2;

        //        //add by siumo 2008-12-30
        //        string sRegion = "";
        //        if (strRegion == "")     //add by fisher in 09-12-04
        //        {
        //            MessageBox.Show("��û�в�ѯȨ�ޣ�");
        //            return;
        //        }
        //        if (strRegion != "˳����" && strRegion != "")
        //        {
        //            if (Array.IndexOf(strRegion.Split(','), "����") > -1)
        //            {
        //                sRegion = strRegion.Replace("����", "����,��ʤ");
        //            }

        //            ZDsql += " and �����ɳ��� in ('" + sRegion.Replace(",", "','") + "')";
        //        }

        //        this.getMaxCount(ZDsql.Replace("X, Y , ��λ���� as ���� , ��š�as  ��_ID, '��ȫ������λ' as ����", "count(*)") + " and �����ֶ�һ is null or �����ֶ�һ=''");
        //        InitDataSet(RecordCount3); //��ʼ�����ݼ�

        //        if (nMax < 1)
        //        {
        //            MessageBox.Show("���������޼�¼.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            this.Cursor = Cursors.Default;
        //            return;
        //        }
        //        // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
        //        ZDsql += " and �����ֶ�һ is null or �����ֶ�һ=''";
        //        //-----------------------------------------------------------------------

        //        DataTable datatable = LoadData(PageNow3,PageCount3, ZDsql); //��ȡ��ǰҳ����
        //        fillDataGridView(datatable, dataGridView4);  //���datagridview

        //        ZDexcelSql = ZDsql.Replace("X, Y , ��λ���� as ���� , ��š�as  ��_ID, '��ȫ������λ' as ����", "��λ����") + " and �����ֶ�һ is null or �����ֶ�һ=''";
        //        DataTable datatableExcel = LoadData(PageNow3, PageCount3, ZDexcelSql);
        //        if (dtExcel != null) dtExcel.Clear();
        //        dtExcel = datatableExcel;

        //        drawPointsInMap(datatable,"��ȫ������λ");   //�ڵ�ͼ�ϻ���
        //        WriteEditLog("�ص㵥λ��ѯ", "��ȫ������λ", ZDsql, "��ѯ");
        //    }
        //    catch (Exception ex)
        //    {
        //        writeZongheLog(ex,"searchDanwei");
        //    }
        //}
        #endregion

        /// <summary>
        /// ��λ���ĵ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �ܱ�ģ���ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void zhouBianCenPointQuery()
        {
            try
            {
                if (textKeyWord.Text.Trim() == "" || textKeyWord.Text.Trim() == "����ؼ���")
                {
                    MessageBox.Show("������ؼ���", "��ʾ");
                    return;
                }
                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;

                dataGridView2.DataSource = null;
                PageNow2.Text = "0";
                PageCount2.Text = "/ {0}";
                RecordCount2.Text = "0��";
                _endNo = Convert.ToInt32(this.TextNum4.Text);
                _startNo = 1;
                ZBexcelSql = "";
                ZBsql = "";
                //removeTemPoints();

                CLC.ForSDGA.GetFromTable.GetFromName(comboClass.Text.Trim(), getFromNamePath);
                if (comboClass.Text.Trim() == "��Ϣ��")
                {
                    #region ��Ϣ���ѯ
                    DataTable dt = new DataTable();
                    DataColumn dc = new DataColumn("���", System.Type.GetType("System.Int32"));
                    dt.Columns.Add(dc);
                    DataColumn dc1 = new DataColumn("����", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc1);
                    DataColumn dc2 = new DataColumn("X", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc2);
                    DataColumn dc3 = new DataColumn("Y", System.Type.GetType("System.Double"));
                    dt.Columns.Add(dc3);
                    DataColumn dc4 = new DataColumn("�����", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc4);
                    DataColumn dc5 = new DataColumn("����", System.Type.GetType("System.String"));
                    dt.Columns.Add(dc5);
                    //���ң���
                    FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["��Ϣ��"];
                    Table table = fl.Table;
                    IResultSetFeatureCollection mFeatCol = MapInfo.Data.FeatureCollectionFactory.CreateResultSetFeatureCollection(table, table.TableInfo.Columns);

                    string sExpress = "NAME like'%" + textKeyWord.Text.Trim() + "%'";
                    if (comboType.Text.Trim() != "ȫ��")
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
                            row[5] = "��Ϣ��";
                            dt.Rows.Add(row);
                            //dataGridView2.Rows.Add(i + 1, ft["NAME"], "",ft.Geometry.Centroid.x.ToString(),ft.Geometry.Centroid.y.ToString(), ft["OBJECTID"], "��Ϣ��");
                        }
                        dataZhoubian = new DataTable();
                        dataZhoubian = dt;
                        this.dataGridView2.DataSource = dt;

                        this.toolPro.Value = 2;
                        //Application.DoEvents();
                        //���ÿ��
                        //setDataGridViewColumnWidth(dataGridView2);
                        for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                        {
                            if (i % 2 == 1)
                            {
                                dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                            }
                        }
                        //CountPage2.Text = "��1ҳ��1ҳ";
                        //BindLabelCount2.Text = "��" + mFeatCol.Count.ToString() + "����¼";
                        PageNow2.Text = "1";
                        PageCount2.Text = "/ 1";
                        RecordCount2.Text = mFeatCol.Count.ToString() + "��";
                        this.dataGridView2.Columns["�����"].Visible = false;
                        this.dataGridView2.Columns["����"].Visible = false;
                    }
                    else
                    {
                        isShowPro(false);
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("�޲�ѯ���", "���");
                    }
                    WriteEditLog("�ܱ߲�ѯ", "��Ϣ��", sExpress, "��λ���ĵ�");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                    this.Cursor = Cursors.Default;
                    return;
                    #endregion
                }
                #region ֮ǰ��SQL���
                //else{
                //    switch (comboClass.Text.Trim())
                //    {
                //        case "����":
                //        case "�˿�":
                //        case "�����ݷ���":
                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ����," + CLC.ForSDGA.GetFromTable.FrmFields + ", t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y ," + CLC.ForSDGA.GetFromTable.ObjID + " as  ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ����  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //        case "��Ƶ":
                //             // ZBsql = "select X,Y , " + CLC.ForSDGA.GetFromTable.ObjID + " as  ��_ID," + CLC.ForSDGA.GetFromTable.ObjName + " as ����,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ���� from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";

                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ����," + CLC.ForSDGA.GetFromTable.FrmFields + ",X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as  ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ����  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //        case "����":
                //        case "����":
                //        case "��Ա":
                //        case "����˨":
                //        case "��������":
                //        case "������ҵ":
                //        case "�ΰ�����":
                //        case "�����ص㵥λ":
                //           // ZBsql = "select X,Y , " + CLC.ForSDGA.GetFromTable.ObjID + " as  ��_ID," + CLC.ForSDGA.GetFromTable.ObjName + " as ����,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ����  from " + CLC.ForSDGA.GetFromTable.TableName+ " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";

                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ����," + CLC.ForSDGA.GetFromTable.FrmFields + ",X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as  ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ����  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //        case "��ȫ������λ":
                //            ZBsql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ����," + CLC.ForSDGA.GetFromTable.FrmFields + ",'����鿴' as �ļ�,X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as  ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ����  from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";
                //            break;
                //    }
                #endregion
                ZBsql = comboField2.Text.Trim() + " like '%" + textKeyWord.Text.Trim() + "%'";

                string sRegion = strRegion;
                // edit by fisher in 09-12-08
                if (strRegion != "˳����")
                {
                    if (strRegion != "")
                    {
                        if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                        {
                            sRegion = strRegion.Replace("����", "����,��ʤ");
                        }
                        ZBsql += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + sRegion.Replace(",", "','") + "'))";
                        if (strRegion1 != "" && (comboClass.Text.Trim() == "����" || comboClass.Text.Trim() == "��������"))
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
                        if (strRegion1 != "" && (comboClass.Text.Trim() == "����" || comboClass.Text.Trim() == "��������"))
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
                            MessageBox.Show("��ȷ�����в�ѯȨ��!");
                            return;
                        }
                    }
                }
                //this.getMaxCount(ZBsql.Replace("t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y , ������� as  ��_ID,�������� as ����,'������Ϣ' as ����", "count(*)"));

                // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                string maxSql = "";
                if (comboClass.Text.Trim() != "��Ա" && comboClass.Text.Trim() != "��Ƶ")   // ��ԱΪ��ͼû�б����ֶ�һ
                {
                    maxSql = "select count(*) from " + CLC.ForSDGA.GetFromTable.TableName + " where " + ZBsql + " and (�����ֶ�һ is null or �����ֶ�һ='')";
                    ZBsql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                }
                else
                    maxSql = "select count(*) from " + CLC.ForSDGA.GetFromTable.TableName + " where " + ZBsql;
                //-----------------------------------------------------------------------

                this.getMaxCount(maxSql);  // edit by fisher in 09-12-17
                InitDataSet(RecordCount2); //��ʼ�����ݼ�

                if (nMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

                //DataTable datatable = LoadData(PageNow2, PageCount2, ZBsql); //��ȡ��ǰҳ����
                DataTable datatable = LoadData(_startNo, _endNo, ZBsql, CLC.ForSDGA.GetFromTable.TableName, false);
                this.toolPro.Value = 1;
                Application.DoEvents();
                //fillDataGridView(datatable, dataGridView2);  //���datagridview
                //dataZhoubian = datatable;
                this.dataGridView2.DataSource = datatable;

                for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                {
                    if (comboClass.Text.Trim() == "��ȫ������λ")
                    {
                        // ����ȫ������λ���ļ���������
                        DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                        dgvlc.Value = "����鿴";
                        dgvlc.ToolTipText = "�鿴��ȫ������λ�ļ�";
                        dataGridView2.Rows[i].Cells["�ļ�"] = dgvlc;
                    }
                    if (i % 2 == 1)
                    {
                        dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                this.dataGridView2.Columns["��_ID"].Visible = false;
                this.dataGridView2.Columns["����"].Visible = false;

                #region �ܱ߲�ѯ����Excel
                ZBexcelSql = ZBsql;
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
                if (comboClass.Text.Trim() != "��Ա" && comboClass.Text.Trim() != "��Ƶ")   // ��ԱΪ��ͼû�б����ֶ�һ
                    ZBexcelSql += " and (�����ֶ�һ is null or �����ֶ�һ='')"; // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19

                LoadData(_startNo, _endNo, ZBexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);

                ////DataTable datatableExcel = LoadData(PageNow2, PageCount2, ZBexcelSql);
                //DataTable datatableExcel = LoadData(_startNo, _endNo, ZBexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                # endregion

                this.toolPro.Value = 2;
                //Application.DoEvents();
                drawPointsInMap(datatable, CLC.ForSDGA.GetFromTable.TableName);   //�ڵ�ͼ�ϻ���

                WriteEditLog("�ܱ߲�ѯ", CLC.ForSDGA.GetFromTable.TableName, ZBsql, "��λ���ĵ�");
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
        /// ���ݲ�ͬ���ͷ�����Ӧ�ĵ�ͼ�д���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="p">��������</param>
        /// <returns>���ʹ���</returns>
        private string getTypeColl(string p)
        {
            try
            {
                string types = "";
                switch (p)
                {
                    case "��������":
                        types = "0,1,2,3";
                        break;
                    case "����ס��":
                        types = "16,17";
                        break;
                    case "��������":
                        types = "13,18,19,21";
                        break;
                    case "����":
                        types = "32,33";
                        break;
                    case "��ͨ":
                        types = "22,23,24,25,26,27";
                        break;
                    case "���н���":
                        types = "8,9,10,11";
                        break;
                    case "ҽ������":
                        types = "14,15";
                        break;
                    case "��������":
                        types = "5,6,7";
                        break;
                    case "���ڱ���":
                        types = "4";
                        break;
                    case "������ʩ":
                        types = "12,31,34";
                        break;
                    case "��˾����":
                        types = "28,29";
                        break;
                    case "С��¥��":
                        types = "30";
                        break;
                    case "����":
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
        /// �����ܱ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
                    MessageBox.Show("���ȶ�λ��ѡ�����ĵ�!", "��ʾ");
                    this.Cursor = Cursors.Default;
                    return;
                }

                //���Ȼ�ȡѡ���¼��x��y
                int idNum = dataGridView2.Rows[0].Cells.Count - 2;
                double x = 0, y = 0, dBufferDis = 0;
                dBufferDis = Convert.ToDouble(comboDis.Text) / 111000;
                string sql = "";

                try
                {
                    if (comboClass.Text == "��Ϣ��")
                    {
                        FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["��Ϣ��"];
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
                    MessageBox.Show("�����ĵ�������!");
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

                //�����Ϊ�գ���ô����Ϣ��tab�в�ѯ
                bool isHave = false;
                if (types != "")
                {
                    FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["��Ϣ��"];
                    Table table = fl.Table;
                    //��ͨ��������ң��ٱȽϾ���ȥ�����Եģ��γ�Բ��ѡ��
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
                        MessageBox.Show("������Χ������ض���", "��ʾ");
                        return;
                    }
                    FeatureLayer fLayer = new MapInfo.Mapping.FeatureLayer(this.queryTable, "queryLayer");

                    mapControl1.Map.Layers.Insert(0, fLayer);
                    this.setLayerStyle(fLayer, Color.Blue, 45, 15);
                    labelLayer(fLayer.Table,"Name");
                    WriteEditLog("�ܱ߲�ѯ", "��Ϣ��", sExpress, "��ѯ");
                }
                else
                {
                    //ͨ�����ƻ�ȡ����
                    GetFromName getFromName = new GetFromName(objName);
                    CLC.ForSDGA.GetFromTable.GetFromName(objName, getFromNamePath);
                    string tableName = CLC.ForSDGA.GetFromTable.TableName;
                    //��ͨ��������ң��ٱȽϾ���ȥ�����Եģ��γ�Բ��ѡ��
                    sql = "select * from " + tableName + " where X>" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                    if (objName == "�˿�" || objName == "�����ݷ���") {
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
                            //datatable.Rows.Remove(dr);//�Ƴ���,��һ��ѭ�������
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
                        MessageBox.Show("������Χ������ض���", "��ʾ");
                        return;
                    }
                    this.insertQueryIntoMap(datatable);//���ͼ�в���ͼ��   
                    WriteEditLog("�ܱ߲�ѯ", tableName, sql, "��ѯ");
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
                //����γ��ת�ɻ���
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
                MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable("��ѯ��");

                foreach (MapInfo.Data.Column col in cols) //���Ʊ�ṹ
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
                //����ط��������ɵ�ͼ��������Map������
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

                string currentSql = "Insert into " + this.queryTable.Alias + "  Select * From " + tempTable.Alias;//����ͼԪ����
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
                MessageBox.Show("�����ʱ��������.\n" + ex.Message, "��ʾ");
            }
        }

        private void labelLayer(Table editTable,string labelField)
        {
            try
            {
                LabelLayer labelLayer = mapControl1.Map.Layers["��עͼ��"] as LabelLayer;

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
                if (str.IndexOf("ȫ��") >= 0)
                    _whereName = str.Replace("ȫ��", "NAME");
                if (str.IndexOf("��������") >= 0)
                {
                    _whereName = str.Replace("��������", "NAME");
                    _whereFldm = " and FLDM in(0,1,2,3)";
                }
                if (str.IndexOf("����ס��") >= 0)
                {
                    _whereName = str.Replace("����ס��", "NAME");
                    _whereFldm = " and FLDM in(16,17)";
                }
                if (str.IndexOf("��������") >= 0)
                {
                    _whereName = str.Replace("��������", "NAME");
                    _whereFldm = " and FLDM in(13,18,19,21)";
                }
                if (str.IndexOf("����") >= 0)
                {
                    _whereName = str.Replace("����", "NAME");
                    _whereFldm = " and FLDM in(32,33)";
                }
                if (str.IndexOf("��ͨ") >= 0)
                {
                    _whereName = str.Replace("��ͨ", "NAME");
                    _whereFldm = " and FLDM in(22,23,24,25,26,27)";
                }
                if (str.IndexOf("���н���") >= 0)
                {
                    _whereName = str.Replace("���н���", "NAME");
                    _whereFldm = " and FLDM in(8,9,10,11)";
                }
                if (str.IndexOf("ҽ������") >= 0)
                {
                    _whereName = str.Replace("ҽ������", "NAME");
                    _whereFldm = " and FLDM in(14,15)";
                }
                if (str.IndexOf("��������") >= 0)
                {
                    _whereName = str.Replace("��������", "NAME");
                    _whereFldm = " and FLDM in(5,6,7)";
                }
                if (str.IndexOf("���ڱ���") >= 0)
                {
                    _whereName = str.Replace("���ڱ���", "NAME");
                    _whereFldm = " and FLDM in(4)";
                }
                if (str.IndexOf("������ʩ") >= 0)
                {
                    _whereName = str.Replace("������ʩ", "NAME");
                    _whereFldm = " and FLDM in(12,31,34)";
                }
                if (str.IndexOf("��˾����") >= 0)
                {
                    _whereName = str.Replace("��˾����", "NAME");
                    _whereFldm = " and FLDM in(28,29)";
                }
                if (str.IndexOf("С��¥��") >= 0)
                {
                    _whereName = str.Replace("С��¥��", "NAME");
                    _whereFldm = " and FLDM in(30)";
                }
                if (str.IndexOf("����") >= 0)
                {
                    _whereName = str.Replace("����", "NAME");
                    _whereFldm = " and FLDM in(20,99)";
                }
                sit[0] = _whereName;
                sit[1] = _whereFldm;
                return sit;
            }
            catch (Exception ex) { writeZongheLog(ex, "getWhere"); return sit; }
        }

        /// <summary>
        /// ת���ַ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <returns>�����ַ���</returns>
        private string getSqlString()//ת���ַ���
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";

                for (int i = 0; i < this.dataGridViewValue.Rows.Count; i++)
                {
                    string type = this.dataGridViewValue.Rows[i].Cells["Type"].Value.ToString();
                    string str = this.dataGridViewValue.Rows[i].Cells["Value"].Value.ToString();

                    //if (comboTable.Text == "ͨ��������ѯ")    // ͨ��������ѯ�Ŀ�������Ҫת��Ϊ���ڱ�ź��ѯ  add by lili 2010-12-16
                    //    str = transSerial(str);

                    if (str.IndexOf("�������� ����") > -1)
                        fuzzyFlag = true;

                    if (type == "����")
                    {
                        if (comboTable.Text == "��Ϣ��")
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
                    else if (type == "ʱ��")
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
                        if (comboTable.Text == "��Ϣ��")
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

                getsql = getsql.Replace("����", "and");
                getsql = getsql.Replace("����", "or");
                getsql = getsql.Replace("����", "like");
                getsql = getsql.Replace("���ڵ���", ">=");
                getsql = getsql.Replace("С�ڵ���", "<=");
                getsql = getsql.Replace("����", ">");
                getsql = getsql.Replace("С��", "<");
                getsql = getsql.Replace("������", "!=");
                getsql = getsql.Replace("����", "=");

                return getsql;
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"getSqlString");
                return "";
            }          
        }

        /// <summary>
        /// �˷���ֻ��getSqlString����ʹ�ã�����ת���ΰ���������ת��Ϊ���ڱ�ţ��˹�����ȡ�����˷���δʹ�ã�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tansSer">Ҫת����sql</param>
        private string transSerial(string tansSer)
        {
            try
            {
                string newSql = "";

                if (tansSer.IndexOf("�������� ����") > -1)
                {
                    string serails = tansSer.Substring(tansSer.IndexOf("'"));

                    CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                    DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select ���ڱ�� from �ΰ�����ϵͳ where ��������=" + serails);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (newSql == string.Empty)
                            newSql = "���ڱ�� in ('" + dt.Rows[i][0].ToString() + "'";
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
        /// ���һ�����ʽ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboTable.Text.Trim() == "")
                {
                    MessageBox.Show("��ѡ���", "��ʾ");
                    return;
                }

                if (textValue.Visible && textValue.Text.Trim() == "")
                {
                    MessageBox.Show("��ѯֵ����Ϊ�գ�", "��ʾ");
                    return;
                }

                if (this.textValue.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("������ַ����в��ܰ���������!", "��ʾ");
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
                        this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                        break;
                    case "DATE":
                        string tValue = this.dateTimePicker1.Value.ToString();
                        if (tValue == "")
                        {
                            MessageBox.Show("��ѯֵ����Ϊ�գ�", "��ʾ");
                            return;
                        }

                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "ʱ��" });
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + " " + this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "ʱ��" });
                        }
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "����")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "�ַ���" });
                            }
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + " " + this.comboField.Text + " " + this.comboYunsuanfu.Text + " '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "����")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "�ַ���" });
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
        /// �Ƴ�һ�����ʽ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
                    string text= this.dataGridViewValue.Rows[0].Cells["Value"].Value.ToString().Replace("����", "");
                    
                    text = text.Replace("����", "").Trim();
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
        /// �л���ѯ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void comboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //ͨ�����ƻ�ȡ����
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
        /// ���ݱ���������ֶμ��ֶ�������ӵ�ComboBox�ؼ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableName">����</param>
        /// <param name="comboBoxField">ComboBox�ؼ�</param>
        private void setFields(string tableName, System.Windows.Forms.ComboBox comboBoxField)
        {
            try
            {
                OracelData linkData = new OracelData(strConn);
                string sExp="";
                if (tableName == "gps��Ա")
                {
                    comboBoxField.Items.Clear();
                    comboBoxField.Items.Add("�������");    comboBoxField.Items.Add("�ɳ�����");
                    comboBoxField.Items.Add("�ж���");   �� comboBoxField.Items.Add("��������");
                    comboBoxField.Items.Add("��ǰ����");    comboBoxField.Items.Add("�豸���");
                    comboBoxField.Items.Add("��λ����ʱ��");
                    arrType = "NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,DATE";
                }
                else if(tableName=="��Ϣ��")
                {
                    comboBoxField.Items.Clear();
                    comboBoxField.Items.Add("ȫ��");     comboBoxField.Items.Add("��������");
                    comboBoxField.Items.Add("����ס��"); comboBoxField.Items.Add("��������");
                    comboBoxField.Items.Add("����");     comboBoxField.Items.Add("��ͨ");
                    comboBoxField.Items.Add("���н���"); comboBoxField.Items.Add("ҽ������");
                    comboBoxField.Items.Add("��������"); comboBoxField.Items.Add("���ڱ���");
                    comboBoxField.Items.Add("������ʩ"); comboBoxField.Items.Add("��˾����");
                    comboBoxField.Items.Add("С��¥��"); comboBoxField.Items.Add("����");
                    arrType = "NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2,NVARCHAR2";
                }
                else if (this.comboTable.Text == "ͨ��������ѯ" && tabControl1.SelectedTab == tabAdvance)
                {
                    comboBoxField.Items.Clear();
                    comboBoxField.Items.Add("���ڱ��"); comboBoxField.Items.Add("��������");
                    comboBoxField.Items.Add("ͨ��ʱ��"); comboBoxField.Items.Add("��������");
                    comboBoxField.Items.Add("��������"); comboBoxField.Items.Add("������ɫ");
                    comboBoxField.Items.Add("��ɫ��ǳ"); comboBoxField.Items.Add("���ڷ���");
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

                        if (aCol != "" && aCol != "MAPID" && aCol.IndexOf("�����ֶ�") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("����") < 0)
                        {
                            //�ܱ߲�ѯ��,ֻ���ַ����ֶν��в�ѯ
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
        /// ѡ���ֶ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void comboField_SelectedIndexChanged(object sender, EventArgs e)
        {
            setYunsuanfuValue(comboField.SelectedIndex);
        }

        /// <summary>
        /// ���ݲ�ͬ�����������Ӧ�ȽϷ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="p">���������е�λ��</param>
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
                        comboYunsuanfu.Items.Add("����");
                        comboYunsuanfu.Items.Add("������");
                        comboYunsuanfu.Items.Add("����");
                        comboYunsuanfu.Items.Add("���ڵ���");
                        comboYunsuanfu.Items.Add("С��");
                        comboYunsuanfu.Items.Add("С�ڵ���");
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        comboYunsuanfu.Items.Add("����");
                        comboYunsuanfu.Items.Add("������");
                        comboYunsuanfu.Items.Add("����");
                        break;
                }
                comboYunsuanfu.Text = "����";
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"setYunsuanfuValue");
            }
        }

        //
        /// <summary>
        /// ��ͼ��Ұ�����仯ʱ��ʵʱ�����ص㵥λ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void mapControl1_ViewChanged(object sender, EventArgs e)
        {
            //if (this.Visible)
            //{
            //    if (checkBoxZhongdian.Checked) return;  //���ʹ�ùؼ��ʲ�ѯ���رղ�ѯ��ǰ��Ұ��
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
        /// �ܱ߶�λ���ĵ㣬�������Ϣ�㣬����ɼ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void comboClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboClass.Text == "��Ϣ��")
                {
                    comboType.Visible = true;
                    comboField2.Visible = false;
                    label12.Text = "ѡ������";
                }
                else
                {
                    comboField2.Visible = true;
                    comboType.Visible = false;
                    label12.Text = "ѡ���ֶ�";
                    //ͨ�����ƻ�ȡ����
                    CLC.ForSDGA.GetFromTable.GetFromName(comboClass.Text.Trim(), getFromNamePath);
                    setFields(CLC.ForSDGA.GetFromTable.TableName, comboField2);
                }
            }
            catch (Exception ex) {
                writeZongheLog(ex, "comboClass_SelectedIndexChanged");
            }
        }

        /// <summary>
        /// �л��ۺ�ģ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void ucZonghe_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    FeatureLayer fl = null;
                    if (tabControl1.SelectedTab == tabAdvance)
                        fl = (FeatureLayer)mapControl1.Map.Layers["�߼���ѯ"];
                    if (tabControl1.SelectedTab == tabDianji)
                        fl = (FeatureLayer)mapControl1.Map.Layers["�����ѯ"];
                    if (tabControl1.SelectedTab == tabZhoubian)
                        fl = (FeatureLayer)mapControl1.Map.Layers["�ܱ߲�ѯ"];
                    if (tabControl1.SelectedTab == tabYintou)
                        fl = (FeatureLayer)mapControl1.Map.Layers["��ͷ��ѯ"];

                    Table tableTem = fl.Table; 
                }
                else
                {
                    //�л�������ʱ,���б�,������0
                    dataGridView1.Rows.Clear();
                    PageNow1.Text = "0";
                    PageCount1.Text = "/ {0}";
                    RecordCount1.Text = "0��";
                    dataGridView2.DataSource = null;
                    PageNow2.Text = "0";
                    PageCount2.Text = "/ {0}";
                    RecordCount2.Text = "0��";

                    dataGridView5.DataSource = null;
                    PageNow4.Text = "0";
                    PageCount4.Text = "/ {0}";
                    RecordCount4.Text = "0��";

                    //pageSize = 0;     //ÿҳ��ʾ����
                    nMax = 0;         //�ܼ�¼��
                    pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
                    pageCurrent = 0;   //��ǰҳ��
                    nCurrent = 0;      //��ǰ��¼��

                    setCheckBoxFasle();

                    closeTables();
                    if (this.queryTable != null)
                    {
                        this.queryTable.Close();
                    }

                    RemoveTemLayer("�߼���ѯ");
                    RemoveTemLayer("�����ѯ");
                    RemoveTemLayer("�ܱ߲�ѯ"); 
                    RemoveTemLayer("��ͷ��ѯ");
                    RemoveTemLayer("lbl_�߼���ѯ");
                    RemoveTemLayer("lbl_�����ѯ");
                    RemoveTemLayer("lbl_�ܱ߲�ѯ");
                    RemoveTemLayer("lbl_��ͷ��ѯ");
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex,"ucZonghe_VisibleChanged");
            }
        }

        #region ɾ�����ص㵥λ����
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
        //            MessageBox.Show("������ؼ���!", "��ʾ");
        //            return;
        //        }
        //        isShowPro(true);
        //        this.Cursor = Cursors.WaitCursor;
        //        dataGridView4.Rows.Clear();
        //        PageNow3.Text = "0";
        //        PageCount3.Text = "/ {0}";
        //        RecordCount3.Text = "��0����¼";
        //        //removeTemPoints();

        //        ZDsql = "select X, Y , ��λ���� as ���� , ��š�as  ��_ID, '��ȫ������λ' as ����  from ��ȫ������λ where ";
        //        ZDsql += cmbDanWei.Text + " like '%" + textWord.Text + "%'";

        //        //add by siumo 2008-12-30
        //        string sRegion = strRegion;
        //        if (strRegion == "")
        //        {
        //            isShowPro(false);
        //            this.Cursor = Cursors.Default;
        //            MessageBox.Show("��û�в�ѯȨ�ޣ�");
        //            return;
        //        }
        //        if (strRegion != "˳����"&& strRegion!="")
        //        {
        //            if (Array.IndexOf(strRegion.Split(','), "����") > -1)
        //            {
        //                sRegion = strRegion.Replace("����", "����,��ʤ");
        //            }

        //            ZDsql += " and �����ɳ��� in ('" + sRegion.Replace(",", "','") + "')";
        //        }

        //        this.getMaxCount(ZDsql.Replace("X, Y , ��λ���� as ���� , ��š�as  ��_ID, '��ȫ������λ' as ����", "count(*)") + " and �����ֶ�һ is null or �����ֶ�һ=''");
        //        InitDataSet(RecordCount3); //��ʼ�����ݼ�

        //        if (nMax < 1)
        //        {
        //            isShowPro(false);
        //            MessageBox.Show("���������޼�¼.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            this.Cursor = Cursors.Default;
        //            return;
        //        }

        //        ZDsql += " and �����ֶ�һ is null or �����ֶ�һ=''";  // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
        //        DataTable datatable = LoadData(PageNow3,PageCount3, ZDsql); //��ȡ��ǰҳ����
        //        fillDataGridView(datatable, dataGridView4);  //���datagridview
        //        this.toolPro.Value = 1;
        //        Application.DoEvents();

        //        #region �ص㵥λ��ѯ����Excel
        //        string sRegion2 = strRegion2;
        //        string sRegion3 = strRegion3;
        //        if (strRegion2 == "")
        //        {
        //            ZDexcelSql += " and 1=2 ";
        //        }
        //        else if (strRegion2 != "˳����")
        //        {
        //            if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
        //            {
        //                sRegion2 = strRegion2.Replace("����", "����,��ʤ");
        //            }
        //            ZDexcelSql += " and �����ɳ��� in ('" + sRegion2.Replace(",", "','") + "')";
        //        }
        //        ZDexcelSql = ZDsql.Replace("X, Y , ��λ���� as ���� , ��š�as  ��_ID, '��ȫ������λ' as ����", "��λ����") + " and �����ֶ�һ is null or �����ֶ�һ=''";  // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19;
        //        DataTable datatableExcel = LoadData(PageNow3, PageCount3, ZDexcelSql);
        //        if (dtExcel != null) dtExcel.Clear();
        //        dtExcel = datatableExcel;
        //        #endregion
        //        this.toolPro.Value = 2;
        //        Application.DoEvents();

        //        drawPointsInMap(datatable,"��ȫ������λ");   //�ڵ�ͼ�ϻ���
        //        WriteEditLog("�ص㵥λ��ѯ", "��ȫ������λ", ZDsql, "��ѯ"); 
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
        //    cmbDanWei.Items.Add("���");       cmbDanWei.Items.Add("��λ����");
        //    cmbDanWei.Items.Add("��λ����");   cmbDanWei.Items.Add("��λ��ַ");
        //    cmbDanWei.Items.Add("�����ɳ���"); cmbDanWei.Items.Add("�����ж�");
        //    cmbDanWei.Items.Add("����������");
        //    cmbDanWei.SelectedIndex = 0;
        //    cmbDanWei.Enabled = checkBoxZhongdian.Checked;
        //    textWord.Enabled = checkBoxZhongdian.Checked;
        //    buttonSearch.Enabled = checkBoxZhongdian.Checked;
        //    pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
        //    pageCurrent = 0;   //��ǰҳ��
        //}
        #endregion

        /// <summary>
        /// ��������ִ��ʱ�������ͼ�������ϵ������ťʱ��ִ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public void clearTem() 
        {
            try
            {
                removeTemPoints();  //�Ƴ���ͼ�ϵ�Ҫ��
                //pageSize = 0;     //ÿҳ��ʾ����
                nMax = 0;         //�ܼ�¼��
                pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
                pageCurrent = 0;   //��ǰҳ��
                nCurrent = 0;      //��ǰ��¼��
                if (this.tabControl1.SelectedTab == this.tabYintou)
                {
                    this.dataGridView1.Rows.Clear();   //����б�
                    PageNow1.Text = "0";
                    PageCount1.Text = "/ {0}";
                    RecordCount1.Text = "0��";
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
                    this.dataGridView2.DataSource = null;   //����б�
                    PageNow2.Text = "0";
                    PageCount2.Text = "/ {0}";
                    RecordCount2.Text = "0��"; 
                    if (this.queryTable != null)
                    {
                        this.queryTable.Close();
                        this.queryTable = null;
                    }
                }
                else
                {
                    this.dataGridView5.DataSource = null;   //����б�
                    PageNow4.Text = "0";
                    PageCount4.Text = "/ {0}";
                    RecordCount4.Text = "0��";
                }
            }
            catch (Exception ex)
            { 
                writeZongheLog(ex,"clearTem");
            }
        }

        /// <summary>
        /// �����ؼ���Сʱ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �����¼�ܸ߶ȴ��������߶�,����ֹ�����,�����еĿ��Ҫ�Զ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �ı������ʱ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �ж�������ǲ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="str">Ҫ��֤���ַ���</param>
        /// <returns>����ֵ</returns>
        private bool checkNumber(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return true;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//�ж�������ǲ�������
                {
                    MessageBox.Show("�������֣�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        /// �ı���ֵ�ı��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void textYintou_TextChanged(object sender, EventArgs e)
        {
            checkLetter(textYintou.Text);
        }

        /// <summary>
        /// �ж����������ĸ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="str">Ҫ�жϵ���ĸ</param>
        private void checkLetter(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^[A-Za-z0-9]+$"))//�ж�������ǲ�����ĸ������
                {
                    MessageBox.Show("ֻ����������ĸ�����֣�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch(Exception ex)
            {
                writeZongheLog(ex,"checkLetter");
            }
        }

        /// <summary>
        /// ��ͷ�ı��򵱻س�ʱִ����ͷ��ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �ܱ��ı��򵱻س�ʱִ���ܱ߲�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �ڵ�ͼ�ϻ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="datatable">����Դ</param>
        /// <param name="tableName">����</param>
        private void drawPointsInMap(DataTable datatable,string tableName)
        {
            try
            {
                if (datatable == null || datatable.Rows.Count < 1) { return; }
                string sql = "";
                //dtTable = datatable;
                if (tabControl1.SelectedTab==tabYintou)
                {
                    if (comboBox1.Text == "ȫ��")
                    {
                        //oracleSpatial�Ĳ�ѯ�����ձ��Ĳ�ѯ��� union ʱ�����,��˷ֿ�
                          //oracleSpatial��
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
                                if (tabName == datatable.Rows[j]["����"].ToString())
                                {
                                    idArr += "'" + datatable.Rows[j]["��_ID"].ToString() + "',";
                                }
                            }
                            if (idArr == "")
                            {
                                continue;
                            }
                            idArr = idArr.Remove(idArr.Length - 1);
                            //ͨ�������ƻ�ȡ����ֶ�
                            CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                            bianhao = CLC.ForSDGA.GetFromTable.ObjID;

                            if (tabName == "������Ϣ" || tabName == "�˿�ϵͳ" || tabName == "�����ݷ���ϵͳ")
                            {
                                sql = "select " + CLC.ForSDGA.GetFromTable.ObjName+ " as ����," + tabName + ".geoloc.SDO_POINT.X as X, " + tabName + ".geoloc.SDO_POINT.Y as Y,to_char(" + bianhao + ") as  ��_ID,'" + tabName + "' as  ����   FROM " + tabName + " " + tabName + " where  " + bianhao + " in (" + idArr + ")";
                            }
                            else
                            {
                                sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ����, X,Y,to_char(" + bianhao + ") as  ��_ID,'" + tabName + "' as  ����   FROM " + tabName + " where  " + bianhao + " in (" + idArr + ")";
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
                            // MessageBox.Show("�޲�ѯ��¼","��ʾ");
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
                                idArr = "'" + datatable.Rows[i]["��_ID"].ToString() + "'";
                            }
                            else
                            {
                                idArr += ",'" + datatable.Rows[i]["��_ID"].ToString() + "'";
                            }
                        }
                        //ͨ�������ƻ�ȡͼ��
                        CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                        string bianhao = CLC.ForSDGA.GetFromTable.ObjID;
                        if (tableName == "������Ϣ" || tableName == "�˿�ϵͳ" || tableName == "�����ݷ���ϵͳ")
                        {
                            sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ����, t.geoloc.SDO_POINT.X as X, t.geoloc.SDO_POINT.Y as Y,to_char(" + bianhao + ") as  ��_ID,'" + tableName + "' as  ����   FROM " + tableName + " t where  " + bianhao + " in (" + idArr + ")";
                        }
                        else
                        {
                            sql = "select  " + CLC.ForSDGA.GetFromTable.ObjName + " as ����, X,Y," + bianhao + " as  ��_ID,'" + tableName + "' as  ����   FROM " + tableName + " where  " + bianhao + " in (" + idArr + ")";
                        }
                        OracelData linkData = new OracelData(strConn);
                        datatable = linkData.SelectDataBase(sql);
                        if (datatable == null)
                        {
                            // MessageBox.Show("�޲�ѯ��¼","��ʾ");
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
                            idArr = "'" + datatable.Rows[i]["��_ID"].ToString() + "'";
                        }
                        else
                        {
                            idArr += ",'" + datatable.Rows[i]["��_ID"].ToString() + "'";
                        }
                    }
                    //ͨ�������ƻ�ȡͼ��
                    CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                    string bianhao = CLC.ForSDGA.GetFromTable.ObjID;
                    if (tableName == "������Ϣ" || tableName == "�˿�ϵͳ" || tableName == "�����ݷ���ϵͳ")
                    {
                        sql = "select t.geoloc.SDO_POINT.X as X, t.geoloc.SDO_POINT.Y as Y,to_char(" + bianhao + ") as  ��_ID,'" + tableName + "' as  ����   FROM " + tableName + " t where  " + bianhao + " in (" + idArr + ")";
                    }
                    else if (tableName == "��Ƶλ��")
                    {
                        sql = "SELECT  X,Y," + bianhao + " as  ��_ID,'��Ƶλ��VIEW' as  ����   FROM ��Ƶλ��VIEW where  " + bianhao + " in (" + idArr + ")";
                    }
                    else
                    {
                        sql = "SELECT  X,Y," + bianhao + " as  ��_ID,'" + tableName + "' as  ����   FROM " + tableName + " where  " + bianhao + " in (" + idArr + ")";
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
        /// ����б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="datatable">����Դ</param>
        /// <param name="dataGridView">dataGridView�ؼ�</param>
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
                    //datagridview��ǰ������ʾ��
                    //��4���Ǳ�����¼��MapID����5���Ǹü�¼��ԭ������������ѯʱʹ��
                    if (dataGridView == dataGridView1)
                    {
                        dataGridView.Rows.Add(i + 1, datatable.Rows[i]["����"], "����...", datatable.Rows[i]["��_ID"], datatable.Rows[i]["����"]);
                    }
                    else
                    {
                        dataGridView.Rows.Add(i + 1, datatable.Rows[i]["����"], "����...", datatable.Rows[i]["X"], datatable.Rows[i]["Y"], datatable.Rows[i]["��_ID"], datatable.Rows[i]["����"]);
                    }
                    if (i % 2 == 1)
                    {
                        dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }

                //���ݹ������Ƿ�������ÿ��
                setDataGridViewColumnWidth(dataGridView);
            }
            catch(Exception ex) {
                writeZongheLog(ex, "fillDataGridView");
            }
        }

        /// <summary>
        /// ��ͷ��ҳ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //���ز���,���false,˵�����˵�һҳ�����һҳ,�в������ý���
                if (isOn)
                {
                    isShowPro(true);
                    DataTable dt = LoadData(PageNow1,PageCount1, YTsql);
                    fillDataGridView(dt, dataGridView1);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
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
        /// �ܱ߷�ҳ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void bindingNavigator2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //���ز���,���false,˵�����˵�һҳ�����һҳ,�в������ý���
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
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
        /// �ۺϷ�ҳ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void bindingNavigator4_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //���ز���,���false,˵�����˵�һҳ�����һҳ,�в������ý���
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);

                    //ͨ�����ƻ�ȡ������������
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

        int pageSize = 1000;     // ÿҳ��ʾ����
        int nMax = 0;            // �ܼ�¼��
        int pageCount = 0;       // ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageCurrent = 0;     // ��ǰҳ��
        int nCurrent = 0;        // ��ǰ��¼��
        DataSet ds = new DataSet();

        /// <summary>
        /// ��ȡ���β�ѯ���м�¼��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">���SQL</param>
        private void getMaxCount(string sql)//�õ�����ֵt
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
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nMax = 0;
            }
        }

        /// <summary>
        /// ��ʼ��ҳ�ؼ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tsLabel"></param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                pageSize = Convert.ToInt32(this.TextNum4.Text);      //����ҳ������                
                tsLabel.Text = nMax.ToString()+"��";//�ڵ���������ʾ�ܼ�¼��
                pageCount = (nMax / pageSize);//�������ҳ��
                if ((nMax % pageSize) > 0) pageCount++;
                if (nMax != 0)
                {
                    pageCurrent = 1;
                }
                nCurrent = 0;       //��ǰ��¼����0��ʼ
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// �˺�����ʵ����Ϣ���ͨ��������ѯ�ķ�ҳ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

                //��Ԫ����Դ���Ƽ�¼��
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(dtPage.Rows[i]);
                    nCurrent++;
                }
                dataZhonghe = new DataTable();           // �����ڴ��ֵ���ڷŴ�����չʾ
                bds.DataSource = dataZhonghe = dtTemp;

                if (comboTable.Text.Trim() == "ͨ��������ѯ")
                {
                    CreateKakouTrack(dtTemp);             // ��ͼ�ϻ��㲢����
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
        /// ��ѯ���ݲ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="textNowPage">��ҳ�ؼ����ӿؼ���������ʾ�����뵱ǰҳ��</param>
        /// <param name="lblPageCount">��ҳ�ؼ����ӿؼ���������ʾ��ǰ����������</param>
        /// <param name="sql">��ѯSQL���</param>
        /// <returns></returns>
        public DataTable LoadData(ToolStripTextBox textNowPage,ToolStripLabel lblPageCount, string sql)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                int nStartPos = 0;   //��ǰҳ�濪ʼ��¼��
                nStartPos = nCurrent;
                //lblPageCount.Text ="��"+Convert.ToString(pageCurrent)+ "ҳ��" + pageCount.ToString()+"ҳ";
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
                Adp.Fill(nStartPos, pageSize, dataTables);//����ط���֪���Ǵ����ݿ��в鵽ǰ100�з��أ��������е����ݾݶ���ѯ�����أ��ٴ��л�ȡǰ100�С�

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
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// ��ѯ���ݲ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="_startNo">��ʼ����</param>
        /// <param name="_endNo">��������</param>
        /// <param name="_whereSql">SQL����</param>
        /// <param name="tableName">����</param>
        /// <param name="isExcel">�����ɵ���SQL</param>
        /// <returns>��ѯ���</returns>
        public DataTable LoadData(int _startNo, int _endNo, string _whereSql,string tableName,bool isExcel)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                int nStartPos = 0;   //��ǰҳ�濪ʼ��¼��
                nStartPos = nCurrent;
                if (tabControl1.SelectedTab == this.tabAdvance)  // �߼���ѯ
                {
                    this.PageNow4.Text = Convert.ToString(pageCurrent);
                    this.PageCount4.Text = "/" + pageCount.ToString();
                }
                else if (tabControl1.SelectedTab == this.tabYintou)  // ��ͷ��ѯ
                {
                    this.PageNow1.Text = Convert.ToString(pageCurrent);
                    this.PageCount1.Text = "/" + pageCount.ToString();
                }
                else if (tabControl1.SelectedTab == this.tabZhoubian)��// �ܱ߲�ѯ
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
                    if (tableName == "������Ϣ" || tableName == "�˿�ϵͳ" || tableName == "�����ݷ���ϵͳ")
                    {
                        if (tableName == "�����ݷ���ϵͳ")
                        {
                            sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ����,���ݱ��,��������,������ϵ�绰,��ϵ��ַ,��Ȩ֤���,��Ȩ֤��,����Ƭ��,����վ,���,ȫ��ַ,"+
                                  "��ַ��·��,��ַ����,¥��,�����,��������,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y," + CLC.ForSDGA.GetFromTable.ObjID + " as ��_ID,'" + 
                                  CLC.ForSDGA.GetFromTable.TableName + "' as ����  from (select rownum as rn1,a.* from �����ݷ���ϵͳ a where rownum<=" + _endNo + " and " + _whereSql +
                                  ") t where rn1 >=" + _startNo;
                        }
                        else
                        {
                            sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ���� ," + CLC.ForSDGA.GetFromTable.FrmFields + ",t.geoloc.SDO_POINT.X as X, t.geoloc.SDO_POINT.Y as Y," + 
                                   CLC.ForSDGA.GetFromTable.ObjID + " as ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ���� from (select rownum as rn1,a.* from " + 
                                   tableName + " a where rownum<=" + _endNo + " and " + _whereSql + ") t where rn1 >=" + _startNo;
                        }
                    }
                    else if (tableName == "��ȫ������λ")
                    {
                        sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ���� ," + CLC.ForSDGA.GetFromTable.FrmFields + ",'����鿴' as �ļ� ,X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ���� from (select rownum as rn1,a.* from ��ȫ������λ a where rownum<=" + _endNo + " and " + _whereSql + ") t where  rn1 >=" + _startNo;
                    }
                    else
                    {
                        sql = "select " + CLC.ForSDGA.GetFromTable.ObjName + " as ���� ," + CLC.ForSDGA.GetFromTable.FrmFields + ",X,Y," + CLC.ForSDGA.GetFromTable.ObjID + " as ��_ID,'" + CLC.ForSDGA.GetFromTable.TableName + "' as ���� from (select rownum as rn1,a.* from " + tableName + " a where rownum <= " + _endNo + " and " + _whereSql + ") t where rn1 >= " + _startNo;
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

                // ���ڷŴ�鿴���ݵ�Table 
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
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// ��ҳ����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <returns>����ֵ</returns>
        private bool bdnInfo_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.TextNum4.Text);
                if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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
                else if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// �쳣��־���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void writeZongheLog(Exception ex,string sFunc) {
            CLC.BugRelated.ExceptionWrite(ex, "clZonghe-ucZonghe-" + sFunc);
        }


        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sModule">����ģ������</param>
        /// <param name="tName">����</param>
        /// <param name="sql">����SQL���</param>
        /// <param name="method">������</param>
        private void WriteEditLog(string sModule,string tName,string sql,string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�ۺϲ�ѯ:" + sModule + "','" + tName + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex) { writeZongheLog(ex, "WriteEditLog"); }
        }

        /// <summary>
        /// ��Ʒ�ҳת��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ltextNow">��ҳ�ؼ����ӿؼ���������ʾ�����뵱ǰҳ��</param>
        private void PageNow_KeyPress(ToolStripTextBox ltextNow)
        {
            try
            {
                if (Convert.ToInt32(ltextNow.Text) < 1 || Convert.ToInt32(ltextNow.Text) > pageCount)
                {
                    MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
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
        /// ͨ������ҳ��ʵ�ַ�ҳ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
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
        /// ͨ������ҳ��ʵ�ַ�ҳ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
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
        /// ͨ������ҳ��ʵ�ַ�ҳ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    //this.toolPro.Value = 2;
                    //Application.DoEvents();
                    LoadData(_startNo, _endNo, GJexcelSql, CLC.ForSDGA.GetFromTable.TableName, true);

                    //ͨ�����ƻ�ȡ������������
                    drawPointsInMap(dt, CLC.ForSDGA.GetFromTable.TableName);
                    this.toolPro.Value = 3;
                    Application.DoEvents();

                    isShowPro(false);
                }
            }
            catch (Exception ex) { writeZongheLog(ex, "PageNow4_KeyPress"); }
        }

        /// <summary>
        /// ʵ��ÿҳ��ʾ��������Ŀ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ltextNum">��ҳ�ؼ����ӿؼ���������ʾ�����뵱ǰҳ��</param>
        /// <param name="datagridview">�����б�</param>
        private void TextNum_KeyPress(ToolStripTextBox ltextNum,DataGridView datagridview)
        {
            try
            {
                if (datagridview.Rows.Count > 0)
                {
                    this.pageSize = Convert.ToInt32(ltextNum.Text);
                    this.pageCurrent = 1;
                    nCurrent = pageSize * (pageCurrent - 1);
                    pageCount = (nMax / pageSize);//�������ҳ��
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
        /// ͨ����������ʵ��ÿҳ��ʾ���ټ�¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
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
        /// ͨ����������ʵ��ÿҳ��ʾ���ټ�¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
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
        /// ͨ����������ʵ��ÿҳ��ʾ���ټ�¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void TextNum4_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView5.Rows.Count > 0)
                {
                    isShowPro(true);
                    //ͨ�����ƻ�ȡ������������
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

                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
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
        /// ����ͳ�ƹ�������ı��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                switch (cmbType.Text)
                {
                    case "���ɳ���":
                        cmbPic.Enabled = true;
                        cmbJinWuShi.Enabled = false;
                        cmbZhongdu.Enabled = false;
                        break;
                    case "���ж�":
                        cmbZhongdu.Enabled = true;
                        cmbJinWuShi.Enabled = false;
                        cmbPic.Enabled = true;
                        break;
                    case "��������":
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
        /// ����ͳ�ƹ����ɳ�����ı��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void cmbPic_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cmbZhongdu.Items.Clear();
                string sql = "select �жӴ���,�ж��� from �������ж�";
                if (cmbPic.Text != "�����ɳ���")
                {
                    sql += " where �����ɳ���='" + cmbPic.Text + "'";
                }

                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable tab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                cmbZhongdu.Items.Add("�����ж�");
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
        /// ����ͳ�ƹ������ð�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// ����ͳ�ƹ��߲�ѯ��ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

            //�жϱ�Ҫ�����Ƿ�ѡ��
            if (tableName == "-��ѡ�����-" || statTypeSelect == "-��ѡ�����-" || cmbTabName.Text == "" || cmbType.Text == "")
            {
                MessageBox.Show("��ѡ��ͳ�Ƶı�Ҫ����������������ѡ��", "����");
                return;
            }
            isShowPro(true);
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (statTypeSelect == "���ɳ���")
                {
                    statType = "�����ɳ�������";
                    pzjSQL = "select �ɳ�����,�ɳ������� from �����ɳ���";
                    wName = cmbPic.Text.ToString();
                }
                if (statTypeSelect == "���ж�")
                {
                    statType = "�����жӴ���";
                    pzjSQL = getPZJ(this.cmbPic, statType);
                    wName = cmbZhongdu.Text.ToString();
                }
                if (statTypeSelect == "��������")
                {
                    statType = "���������Ҵ���";
                    pzjSQL = getPZJ(this.cmbZhongdu, statType);
                    wName = cmbJinWuShi.Text.ToString();
                }

                string pcs = cmbPic.Text.ToString() != "�����ɳ���" ? cmbPic.Text.ToString() : "";
                string zd = cmbZhongdu.Text.ToString() != "�����ж�" ? cmbZhongdu.Text.ToString() : "";
                string jws = cmbJinWuShi.Text.ToString() != "���о�����" ? cmbJinWuShi.Text.ToString() : "";

                begin = dtpStartTime.Text.Replace("��", "-").Replace("��", "-").Substring(0, dtpStartTime.Text.Length - 1);
                end = dtpEndTime.Text.Replace("��", "-").Replace("��", "-").Substring(0, dtpEndTime.Text.Length - 1);

                dtble = new DataTable();
                // ������
                DataColumn col = new DataColumn("ͳ��ʱ���", System.Type.GetType("System.String"));
                DataColumn col1 = new DataColumn("������", System.Type.GetType("System.String"));
                DataColumn col2 = new DataColumn("��ʱ��θ�����Ҫ��ע����", System.Type.GetType("System.String"));
                DataColumn col3 = new DataColumn("�ѱ�ע������", System.Type.GetType("System.String"));
                DataColumn col4 = new DataColumn("δ��ע������", System.Type.GetType("System.String"));
                DataColumn col5 = new DataColumn("��ע��", System.Type.GetType("System.String"));
                DataColumn col6 = new DataColumn("��ע׼ȷ��", System.Type.GetType("System.String"));
                DataColumn col7 = new DataColumn("������λ", System.Type.GetType("System.String"));
                DataColumn col8 = new DataColumn("��ע", System.Type.GetType("System.String"));
                // �����������
                dtble.Columns.Add(col);  dtble.Columns.Add(col1); dtble.Columns.Add(col2);
                dtble.Columns.Add(col3); dtble.Columns.Add(col4); dtble.Columns.Add(col5);
                dtble.Columns.Add(col6); dtble.Columns.Add(col7); dtble.Columns.Add(col8);

                this.toolPro.Value = 1;
                Application.DoEvents();

                // �����������
                DataRow row = null;
                int couLen = 0;        // ��ʱ��θ�����Ҫ��ע��
                int aleLen = 0;        // �ѱ�ע��
                int notLen = 0;        // δ��ע������
                string pzjNo = "";
                if (wName == "�����ɳ���" || wName == "�����ж�" || wName == "���о�����")
                {
                    if (tableName == "���б�")
                    {
                        dtble = GetAllTable(dtpStartTime.Text, dtpEndTime.Text, statType, begin, end, wName, "�����û�");

                    }
                    else
                    {
                        getSQL(tableName, statType, begin, end, wName, "�����û�",out pzjNo);

                        DataTable pzjTab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(pzjSQL);
                        for (int j = 0; j < pzjTab.Rows.Count; j++)
                        {
                            couLen = PossTab.Select(statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;  // ��ʱ��θ�����Ҫ��ע��
                            aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0 and " + statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;   // �ѱ�ע��
                            notLen = couLen - aleLen;     // δ��ע������

                            row = dtble.NewRow();
                            row[0] = dtpStartTime.Text + " �� " + dtpEndTime.Text;     // ͳ��ʱ���
                            row[1] = tableName;       // ������
                            row[2] = couLen;          // ��ʱ��θ�����Ҫ��ע��
                            row[3] = aleLen;          // �ѱ�ע��
                            row[4] = notLen;          // δ��ע������
                            row[5] = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "������" ? "0%" : Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) + "%"; // ��ע��
                            row[6] = "";       // ��ע׼ȷ��
                            row[7] = pzjTab.Rows[j][0].ToString();    // ������λ
                            row[8] = "";       // ��ע

                            if (couLen > 0)
                                dtble.Rows.Add(row);
                        }
                    }

                    this.toolPro.Value = 2;
                    Application.DoEvents();
                }
                else
                {
                    if (tableName == "���б�")
                    {
                        dtble = GetAllTable(dtpStartTime.Text, dtpEndTime.Text, statType, begin, end, wName, "ĳһ���û�");
                    }
                    else
                    {
                        getSQL(tableName, statType, begin, end, wName, "ĳһ���û�",out pzjNo);

                        couLen = PossTab.Rows.Count;  // ��ʱ��θ�����Ҫ��ע��
                        aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0").Length;    // �ѱ�ע��
                        notLen = couLen - aleLen;     // δ��ע������

                        row = dtble.NewRow();
                        row[0] = dtpStartTime.Text + " �� " + dtpEndTime.Text;   // ͳ��ʱ���
                        row[1] = tableName;
                        row[2] = couLen;             // ��ʱ��θ�����Ҫ��ע��
                        row[3] = aleLen;             // �ѱ�ע��
                        row[4] = notLen;
                        row[5] = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "������" ? "0%" : Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) + "%";
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
                    MessageBox.Show("û��Ҫͳ�Ƶ����ݣ�����", "��ʾ");
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
        /// �����ϼ�ѡ�����¼���sql
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="box">�ϼ��ؼ�</param>
        /// <param name="strBox">����</param>
        /// <returns>sql</returns>
        private string getPZJ(System.Windows.Forms.ComboBox box,string strBox)
        {
            try
            {
                string pzjSql = "";                               // ����¼���sql
                string comSelect = box.SelectedItem.ToString();   // �ϼ�ѡ���ֵ

                switch (strBox)
                {
                    case "�����жӴ���":
                        if (comSelect.IndexOf("����") > -1)       // ��������е��ɳ�����������жӷ����ɳ������ж�
                        {
                            pzjSql = "select �ж���,�жӴ��� from �������ж�";
                        }
                        else
                        {
                            pzjSql = "select �ж���,�жӴ��� from �������ж� where �����ɳ���='" + comSelect + "'";
                        }
                        break;
                    case "���������Ҵ���":
                        if (comSelect.IndexOf("����") > -1)       // ��������е��ж�������о����ҷ����жӲ龯����
                        {
                            pzjSql = "select ��������,�����Ҵ��� from ����������";
                        }
                        else
                        {
                            pzjSql = "select ��������,�����Ҵ��� from ���������� where �����ж�='" + comSelect + "'";
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
        /// ƴ��SQL���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableName">����</param>
        /// <param name="statType">������λ</param>
        /// <param name="begin">��ʼʱ��</param>
        /// <param name="end">����ʱ��</param>
        /// <param name="wName">������λֵ</param>
        /// <param name="alloneName">�û����ͣ������û�/ĳһ���û���</param>
        /// <param name="pzjNo">����������</param>
        private void getSQL(string tableName, string statType, string begin, string end, string wName, string alloneName, out string pzjNo)
        {
            string  wTime = "";

            #region ƴ��SQL���
            try
            {
                string cretabSql = "";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                wTime = "��ȡ����ʱ�� between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')";
                if (alloneName == "�����û�")
                {
                    switch (tableName)
                    {
                        case "�˿�ϵͳ":
                        case "�����ݷ���ϵͳ":
                        case "������Ϣ":
                            cretabSql = "select t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ��� from " + tableName + " t where " + wTime;
                            break;
                        default:
                            cretabSql = "select X,Y,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ��� from " + tableName + " where " + wTime;
                            break;
                    }
                    pzjNo = "";
                }
                else
                {
                    string sqlStr1 = "";
                    switch (statType)
                    {
                        case "�����ɳ�������":
                            sqlStr1 = "select �ɳ������� from �����ɳ��� where �ɳ�����='" + wName + "'";
                            break;
                        case "�����жӴ���":
                            sqlStr1 = "select �жӴ��� from �������ж� where �ж���='" + wName + "'";
                            break;
                        case "���������Ҵ���":
                            sqlStr1 = "select �����Ҵ��� from ���������� where ��������='" + wName + "'";
                            break;
                    }
                    DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr1);
                    string wNameNo = pzjNo = dt.Rows[0][0].ToString();
                    switch (tableName)
                    {
                        case "�˿�ϵͳ":
                        case "�����ݷ���ϵͳ":
                        case "������Ϣ":
                            cretabSql = "select t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ��� from " + tableName + " t where " + wTime + "and " + statType + "='" + wNameNo + "'";
                            break;
                        default:
                            cretabSql = "select X,Y,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ��� from " + tableName + " where " + wTime + "and " + statType + "='" + wNameNo + "'";
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
        /// ͳ�����б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dtpStartTime">�������ʾ�Ŀ�ʼʱ��</param>
        /// <param name="dtpEndTime">�������ʾ�Ľ���ʱ��</param>
        /// <param name="statType">������λ</param>
        /// <param name="begin">��ʼʱ��</param>
        /// <param name="end">����ʱ��</param>
        /// <param name="wName">������λֵ</param>
        /// <param name="allOneName">�û����ͣ������û�/ĳһ���û���</param>
        /// <returns>ͳ�ƽ��</returns>
        private DataTable GetAllTable(string dtpStartTime,string dtpEndTime,string statType,string begin,string end ,string wName,string allOneName)
        {
            // ������
            DataTable dTable = new DataTable();
            CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
            try
            {
                // ������
                DataColumn col = new DataColumn("ͳ��ʱ���", System.Type.GetType("System.String"));
                DataColumn col1 = new DataColumn("������", System.Type.GetType("System.String"));
                DataColumn col2 = new DataColumn("��ʱ��θ�����Ҫ��ע����", System.Type.GetType("System.String"));
                DataColumn col3 = new DataColumn("�ѱ�ע������", System.Type.GetType("System.String"));
                DataColumn col4 = new DataColumn("δ��ע������", System.Type.GetType("System.String"));
                DataColumn col5 = new DataColumn("��ע��", System.Type.GetType("System.String"));
                DataColumn col6 = new DataColumn("��ע׼ȷ��", System.Type.GetType("System.String"));
                DataColumn col7 = new DataColumn("������λ", System.Type.GetType("System.String"));
                DataColumn col8 = new DataColumn("��ע", System.Type.GetType("System.String"));

                // �����������
                dTable.Columns.Add(col);  dTable.Columns.Add(col1); dTable.Columns.Add(col2);
                dTable.Columns.Add(col3); dTable.Columns.Add(col4); dTable.Columns.Add(col5);
                dTable.Columns.Add(col6); dTable.Columns.Add(col7); dTable.Columns.Add(col8);

                // ��ȡ���б���
                string[] tbName = new string[cmbTabName.Items.Count - 2];
                for (int i = 2; i < cmbTabName.Items.Count; i++)
                {
                    tbName[i - 2] = cmbTabName.Items[i].ToString();
                }

                // �����������
                DataRow row = null;
                isShowPro(true);
                string timLen = "";    // ͳ��ʱ���
                string tabLen = "";    // ������
                int couLen = 0;        // ��ʱ��θ�����Ҫ��ע��
                int aleLen = 0;        // �ѱ�ע��
                int notLen = 0;        // δ��ע������
                string ratLen = "";    // ��ע��
                string uniLen = "";    // ������λ
                string pzjNo="";
                for (int i = 0; i < tbName.Length; i++)
                {
                    getSQL(tbName[i], statType, begin, end, wName, allOneName, out pzjNo);

                    if (wName == "�����ɳ���" || wName == "�����ж�" || wName == "���о�����")
                    {
                        DataTable pzjTab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(pzjSQL);
                        for (int j = 0; j < pzjTab.Rows.Count; j++)
                        {
                            timLen = dtpStartTime + " �� " + dtpEndTime;
                            tabLen = tbName[i];
                            couLen = PossTab.Select(statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;
                            aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0 and " + statType + "='" + pzjTab.Rows[j][1].ToString() + "'").Length;
                            notLen = couLen - aleLen;
                            ratLen = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "������" 
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
                        timLen = dtpStartTime + " �� " + dtpEndTime;
                        tabLen = tbName[i];
                        couLen = PossTab.Rows.Count;
                        aleLen = PossTab.Select("X is not null and Y is not null and X<>0 and Y<>0").Length;
                        notLen = couLen - aleLen;
                        ratLen = Convert.ToString(Math.Round(Convert.ToDouble(aleLen) / Convert.ToDouble(couLen) * 100, 2, MidpointRounding.AwayFromZero)) == "������"
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
        /// �û���¼ͳ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �û���¼ͳ�ƵĲ�ѯ��ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbDan.Enabled == true)
                {
                    if (cmbDan.Text == "" || cmbDan.Text == "--��ѡ��λ--")
                    {
                        MessageBox.Show("��ѡ��λ��ͳ��!", "��ʾ");
                        return;
                    }

                }
                if (cmbName.Enabled == true)
                {
                    if (cmbName.Text == "" || cmbName.Text == "--��ѡ���û�--")
                    {
                        MessageBox.Show("��ѡ���û���ͳ��!", "��ʾ");
                        return;
                    }
                }

                isShowPro(true);
                ////////---����û� lili 2010-6-8---///////
                string nameStr = "";
                if (cmbName.Text.Trim() == "�����û�")
                    nameStr = "�����û�";
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
                    //��ʼʱ��
                    begin = dtpStart.Text.Replace("��", "-").Replace("��", "-").Substring(0, dtpStart.Text.Length - 1);
                    //����ʱ��
                    end = dtpEnd.Text.Replace("��", "-").Replace("��", "-").Substring(0, dtpEnd.Text.Length - 1);

                    if (cmbDan.Enabled == true)
                    {
                        if (cmbDan.Text.Trim() == "���е�λ")
                        {
                            dt = getAllDan(dtpStart.Text, dtpEnd.Text, begin, end, "����");
                        }
                        else
                        {
                            dt = getAllDan(dtpStart.Text, dtpEnd.Text, begin, end, "ĳһ��");
                        }

                        this.toolPro.Value = 2;
                        Application.DoEvents();
                    }

                    if (cmbName.Enabled == true)
                    {
                        if (nameStr == "�����û�")
                        {
                            sqlStr = GetDenLuSQL(dtpStart.Text, dtpEnd.Text, nameStr, begin, end, "����");
                            dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        }
                        else
                        {
                            sqlStr = GetDenLuSQL(dtpStart.Text, dtpEnd.Text, nameStr, begin, end, "ĳһ��");
                            dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        }

                        this.toolPro.Value = 2;
                        Application.DoEvents();
                    }

                    if (dt.Rows.Count <= 0)
                    {
                        isShowPro(false);
                        MessageBox.Show("��������ͳ�ƽ��!", "��ʾ");
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
        /// ���ɵ�¼ͳ�Ƶ�SQL���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dtpStart">�û���ʾ�Ŀ�ʼʱ��</param>
        /// <param name="dtpEnd">�û���ʾ�Ľ���ʱ��</param>
        /// <param name="username">�û���</param>
        /// <param name="begin">�û������Ŀ�ʼʱ</param>
        /// <param name="end">�û������Ľ���ʱ��</param>
        /// <param name="oneAlluser">�û����ͣ������û�/ĳһ���û���</param>
        /// <returns>ͳ�Ƶ�¼��SQL�ַ���</returns>
        private string GetDenLuSQL(string dtpStart, string dtpEnd, string username, string begin, string end,string oneAlluser)
        {
            try
            {
                string sqlSQL = "", wTime = "";
                wTime = "ʱ�� between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss') and ����ģ��='��¼ϵͳ'";
                int count = CLC.DatabaseRelated.OracleDriver.OracleComScalar("select count(*) from ������¼ where " + wTime);
                switch (oneAlluser)
                {
                    case "����":       // ͳ�������û���¼������SQL
                        sqlSQL = "select '" + dtpStart + " �� " + dtpEnd + "' as ͳ��ʱ���, �û���, count(*) as ��¼����,case substr(to_char(Round(count(*)/" + count.ToString() + "*100,2)),0,1) when '.' then '0'||to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' else to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' end as ʹ����,'' as ��ע from ������¼ where " + wTime + " group by �û���";
                        break;
                    case "ĳһ��":����// ͳ�Ƶ����û���¼������SQL
                        sqlSQL = "select '" + dtpStart + " �� " + dtpEnd + "' as ͳ��ʱ���, �û���, count(*) as ��¼����,case substr(to_char(Round(count(*)/" + count.ToString() + "*100,2)),0,1) when '.' then '0'||to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' else to_char(Round(count(*)/" + count.ToString() + "*100,2))||'%' end as ʹ����,'' as ��ע from ������¼ where �û���='" + username + "' and " + wTime + " group by �û���";
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
        /// ����ͳ�ƽ�����ڴ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dtpStart">�û���ʾ�Ŀ�ʼʱ��</param>
        /// <param name="dtpEnd">�û���ʾ�Ľ���ʱ��</param>
        /// <param name="begin">�û������Ŀ�ʼʱ</param>
        /// <param name="end">�û������Ľ���ʱ��</param>
        /// <param name="oneAllUser">�û����ͣ������û�/ĳһ���û���</param>
        /// <returns>ͳ�ƽ��DataTable</returns>
        private DataTable getAllDan(string dtpStart,string dtpEnd,string begin,string end,string oneAllUser)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
            DataTable dtable = new DataTable();

            // �����
            DataColumn col = new DataColumn("ͳ��ʱ���", System.Type.GetType("System.String"));
            DataColumn col1 = new DataColumn("�û���", System.Type.GetType("System.String"));
            DataColumn col2 = new DataColumn("��½����", System.Type.GetType("System.String"));
            DataColumn col3 = new DataColumn("ʹ����", System.Type.GetType("System.String"));
            DataColumn col4 = new DataColumn("��ע", System.Type.GetType("System.String"));
            dtable.Columns.Add(col);  dtable.Columns.Add(col1);
            dtable.Columns.Add(col2); dtable.Columns.Add(col3);
            dtable.Columns.Add(col4);

            int shu = 0;  // ���ĳ����λ���ܴ���
            int count = CLC.DatabaseRelated.OracleDriver.OracleComScalar("select count(*) from ������¼ where ����ģ��='��¼ϵͳ'and ʱ�� between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')");   // ������е�¼����
            try
            {
                string[] danWei = new string[cmbDan.Items.Count - 2];
                for (int i = 2; i < cmbDan.Items.Count; i++)
                {
                    danWei[i - 2] = cmbDan.Items[i].ToString();
                }

                DataRow row = null;
                if (oneAllUser == "����")
                {
                    for (int i = 0; i < danWei.Length; i++)
                    {
                        DataTable dt = new DataTable();
                        shu = 0;
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select USERNAME from �û� where �û���λ='" + danWei[i] + "'");
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            string sqlSQL = GetDenLuSQL(dtpStart, dtpEnd, dt.Rows[j][0].ToString(), begin, end, "ĳһ��");
                            DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlSQL);
                            for (int k = 0; k < table.Rows.Count; k++)
                            {
                                shu += Convert.ToInt32(table.Rows[k][2].ToString());
                            }
                        }
                        if (dt.Rows.Count > 0)
                        {
                            row = dtable.NewRow();
                            row[0] = dtpStart + " �� " + dtpEnd;
                            row[1] = danWei[i];
                            row[2] = shu;
                            row[3] = Convert.ToString(Math.Round(shu / Convert.ToDouble(count) * 100, 2, MidpointRounding.AwayFromZero)) == "������" ? "0%" : Convert.ToString(Math.Round(shu / Convert.ToDouble(count) * 100, 2, MidpointRounding.AwayFromZero)) + "%";
                            row[4] = "";
                            dtable.Rows.Add(row);
                        }
                    }
                    return dtable;
                }
                else
                {
                    DataTable dt = new DataTable();
                    dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select USERNAME from �û� where �û���λ='" + cmbDan.Text.Trim() + "'");
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
        /// ��ע��ͳ�����ð�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// ��ע��ͳ��ͳ�ư�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cmbTable.Text == "--��ѡ�����--" || cmbTable.Text == "")
            {
                MessageBox.Show("��ѡ����Ҫͳ�Ƶı�","��ʾ");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            try
            {
                isShowPro(true);
                DataTable dt = new DataTable();
                
                string sqlStr = "", end = "", begin = "", wTime = "";

                //��ʼʱ��
                begin =  dtpKai.Text.Replace("��", "-").Replace("��", "-").Substring(0, dtpKai.Text.Length - 1);
                //����ʱ��
                end = dtpjie.Text.Replace("��", "-").Replace("��", "-").Substring(0, dtpjie.Text.Length - 1);
                wTime = " ��עʱ�� between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')";

                if (string.IsNullOrEmpty(textName.Text))
                {
                    sqlStr = "select t.��ע�� as ��ע��,count(*) as ��ע����,t.�����ж� as ���ڵ�λ from " + cmbTable.Text + " t where ��ע�� is not null and " + wTime + "  Group by t.��ע��,t.�����ж�";
                }
                else
                {
                    sqlStr = "select t.��ע�� as ��ע��,count(*) as ��ע����,t.�����ж� as ���ڵ�λ from " + cmbTable.Text + " t where " + wTime + " and ��ע��='" + textName.Text + "' Group by t.��ע��,t.�����ж�";
                }

                this.toolPro.Value = 1;
                Application.DoEvents();
                dt = BiaoRens(sqlStr, begin, end);

                if (dt.Rows.Count <= 0)
                {
                    isShowPro(false);
                    MessageBox.Show("��������ͳ�ƽ��!", "��ʾ");
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
        /// ��ע��ͳ�Ʋ�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">ͳ�Ƶ�SQL</param>
        /// <param name="begin">��ʼʱ��</param>
        /// <param name="end">����ʱ��</param>
        /// <returns>�����</returns>
        private DataTable BiaoRens(string sql,string begin,string end)
        {
            try
            {
                dateTable = new DataTable();
                DataColumn col = new DataColumn("ͳ��ʱ���",System.Type.GetType("System.String"));
                DataColumn col1 = new DataColumn("��ע��", System.Type.GetType("System.String"));
                DataColumn col2 = new DataColumn("��ע����", System.Type.GetType("System.String"));
                DataColumn col3 = new DataColumn("���ڵ�λ", System.Type.GetType("System.String"));
                DataColumn col4 = new DataColumn("��ע", System.Type.GetType("System.String"));
                dateTable.Columns.Add(col); dateTable.Columns.Add(col1);
                dateTable.Columns.Add(col2); dateTable.Columns.Add(col3);
                dateTable.Columns.Add(col4); 

                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                this.toolPro.Value = 2;
                Application.DoEvents();

                string  whereTime = " ��עʱ�� between to_date('" + begin + " 00:00:01','yyyy-MM-dd HH24:mi:ss') and to_date('" + end + " 23:59:59','yyyy-MM-dd HH24:mi:ss')";

                string sql2 = "";
                if (string.IsNullOrEmpty(textName.Text))
                { 
                    sql2 = "select distinct t.��ע�� from " + cmbTable.Text + " t where ��ע�� is not null and " + whereTime + "  Group by t.��ע��,t.�����ж�";
                }
                else
                {
                    sql2 = "select distinct t.��ע�� from " + cmbTable.Text + " t where ��ע�� ='" + textName.Text + "' and " + whereTime + "  Group by t.��ע��,t.�����ж�";
                }
                DataTable dt2 = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql2);
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    dateTable.Rows.Add(getMessage(dt.Select("��ע��='" + dt2.Rows[i][0].ToString() + "'"), begin, end));
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
        /// �����ڳ���ͳ�ƽ�����ͳ���½��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="rows">Ҫͳ�Ƶ���</param>
        /// <param name="begin">��ʼʱ��</param>
        /// <param name="end">����ʱ��</param>
        /// <returns>�½����</returns>
        private DataRow getMessage(DataRow[] rows, string begin, string end)
        {
            try
            {
                string zdName = "";    // ���ڵ�λ
                int markCount = 0;     // ��ע����
                string sName = "";     // ��ע��
                foreach (DataRow row in rows)
                {
                    markCount += Convert.ToInt32(row["��ע����"].ToString());
                    if (row["���ڵ�λ"].ToString().Trim() != "")
                    {
                        zdName = row["���ڵ�λ"].ToString();
                    }
                    sName = row["��ע��"].ToString();
                }
                DataRow dr = dateTable.NewRow();
                dr[0] = begin + " �� " + end;
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
        /// ����λͳ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// ���û�ͳ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// ����λ��ע���ͳ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// ��ע��ͳ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �û���¼ͳ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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

        /* �������Զ���ȫ���� */

        /// <summary>
        /// �Զ���ȫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="keyword">�ı����������ֵ</param>
        /// <param name="colword">����</param>
        /// <param name="tableName">����</param>
        /// <param name="listBox1">��ʾ�Զ���ȫֵ�Ŀؼ�</param>
        /// <returns>������</returns>
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
                        #region ����SQL���
                        switch (colword)
                        {
                            case "��������":
                                strExp = "select �������� from �������� t where ��������  like '" + keyword + "%' union select �������� from �������� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from �������� t where ��������  like '%" + keyword + "'";
                                break;
                            case "����_����":
                                strExp = "select ����2 from ���۰��� t where ����2  like '" + keyword + "%' union select ����2 from ���۰��� t where ����2 like '%" + keyword + "%' and ����2 not like '" + keyword + "%' and ����2 not like '%" + keyword + "' union select ����2 from ���۰��� t where ����2  like '%" + keyword + "'";
                                break;
                            case "�˿�����":
                                strExp = "select �������� from �˿����� t where ��������  like '" + keyword + "%' union select �������� from �˿����� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from �˿����� t where ��������  like '%" + keyword + "'";
                                break;
                            case "�Ա�":
                                strExp = "select ���� from �Ա� t where ����  like '" + keyword + "%' union select ���� from �Ա� t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from �Ա� t where ����  like '%" + keyword + "'";
                                break;
                            case "����":
                                strExp = "select ���� from ���� t where ����  like '" + keyword + "%' union select ���� from ���� t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from ���� t where ����  like '%" + keyword + "'";
                                break;
                            case "����״̬":
                                strExp = "select ���� from ����״�� t where ����  like '" + keyword + "%' union select ���� from ����״�� t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from ����״�� t where ����  like '%" + keyword + "'";
                                break;
                            case "������ò":
                                strExp = "select ���� from ������ò t where ����  like '" + keyword + "%' union select ���� from ������ò t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from ������ò t where ����  like '%" + keyword + "'";
                                break;
                            case "��������":
                                strExp = "select �������� from �������� t where ��������  like '" + keyword + "%' union select �������� from �������� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from �������� t where ��������  like '%" + keyword + "'";
                                break;
                            case "�����ɳ���":
                                strExp = "select �ɳ����� from �����ɳ��� t where �ɳ�����  like '" + keyword + "%' union select �ɳ����� from �����ɳ��� t where �ɳ����� like '%" + keyword + "%' and �ɳ����� not like '" + keyword + "%' and �ɳ����� not like '%" + keyword + "' union select �ɳ����� from �����ɳ��� t where �ɳ�����  like '%" + keyword + "'";
                                break;
                            case "�����ж�":
                                strExp = "select �ж��� from �������ж� t where �ж���  like '" + keyword + "%' union select �ж��� from �������ж� t where �ж��� like '%" + keyword + "%' and �ж��� not like '" + keyword + "%' and �ж��� not like '%" + keyword + "' union select �ж��� from �������ж� t where �ж���  like '%" + keyword + "'";
                                break;
                            case "����������":
                                strExp = "select �������� from ���������� t where ��������  like '" + keyword + "%' union select �������� from ���������� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from ���������� t where ��������  like '%" + keyword + "'";
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
        /// ��Ϊ�̶�ֵʱ�Զ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="colName">����</param>
        /// <param name="tableName">����</param>
        /// <param name="listBox">��ʾ�Զ���ȫֵ�Ŀؼ�</param>
        /// <returns>ƥ����</returns>
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
                        #region ����SQL���
                        switch (colName)
                        {
                            case "��������":
                                strExp = "select �������� from �������� t Group by ��������";
                                break;
                            case "����_����":
                                strExp = "select ����2 from ���۰��� t Group by ����2";
                                break;
                            case "�˿�����":
                                strExp = "select �������� from �˿����� t Group by ��������";
                                break;
                            case "�Ա�":
                                strExp = "select ���� from �Ա� t Group by ����";
                                break;
                            case "����":
                                strExp = "select ���� from ���� t Group by ����";
                                break;
                            case "����״̬":
                                strExp = "select ���� from ����״�� t Group by ����";
                                break;
                            case "������ò":
                                strExp = "select ���� from ������ò t Group by ����";
                                break;
                            case "��������":
                                strExp = "select �������� from �������� t Group by ��������";
                                break;
                            case "�����ɳ���":
                                strExp = "select �ɳ����� from �����ɳ��� t Group by �ɳ�����";
                                break;
                            case "�����ж�":
                                strExp = "select �ж��� from �������ж� t Group by �ж���";
                                break;
                            case "����������":
                                strExp = "select �������� from ���������� t Group by ��������";
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
        /// �ܱ��ı���ƥ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �ܱ��ı���ƥ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �߼���ѯ�ı���ƥ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void textValue_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                if (comboField.Text == "��������" || comboField.Text == "���ڱ��" || comboField.Text == "������������" || comboField.Text == "�������ڱ��")
                {
                    string keyword = this.textValue.Text.Trim();
                    string colword = string.Empty;

                    if (comboField.Text.IndexOf("��������") > -1)
                    {
                        colword = "��������";
                    }
                    else if (comboField.Text.IndexOf("���ڱ��") > -1)
                    {
                        colword = "���ڱ��";
                    }

                    if (keyword != "" && colword != "")
                    {
                        string strExp = "select distinct(" + colword + ") from �ΰ�����ϵͳ t where " + colword + " like '%" + keyword + "%'  order by " + colword;
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
        /// �߼���ѯ�ı���ƥ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// ������־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="message">�쳣�ַ���</param>
        /// <param name="funName">������</param>
        private void WriteLog(string message, string funName)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\timeTestLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine("����" + funName + "  ��     " + message);
                strWri.Dispose();
                strWri.Close();
            }
            catch
            { }
        }

        /// <summary>
        /// ��ʾ�����ؽ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="falg">����ֵ(true-��ʾ false-����)</param>
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
        /// ��������������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void LinklblHides_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.tabControl1.SelectedTab == this.tabYintou)               //  ��ͷ��ѯ
                    HidesCondition(sender, e, this.groupBox1, null);
                if (this.tabControl1.SelectedTab == this.tabZhoubian)          //  �ܱ߲�ѯ
                    HidesCondition(sender, e, this.groupBox3, this.textKeyWord);
                if (this.tabControl1.SelectedTab == this.tabAdvance)           //  �߼���ѯ
                    HidesCondition(sender, e, this.groupBox2, this.textValue);
            }
            catch (Exception ex) { writeZongheLog(ex, "isShowPro"); }
        }

        /// <summary>
        /// ��ʾ������������ 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="groupBox">groupBox�ؼ�</param>
        /// <param name="text">�ı���</param>
        private void HidesCondition(object sender, LinkLabelLinkClickedEventArgs e,System.Windows.Forms.GroupBox groupBox,SplitWord.SpellSearchBoxEx text)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;
                link.Visible = true;

                if (link.Text == "����������")
                {
                    if (text != null)
                    {
                        text.Visible = false;
                    }
                    groupBox.Visible = false;
                    link.Text = "��ʾ������";
                }
                else
                {
                    if (text != null)
                    {
                        text.Visible = true;
                    }
                    groupBox.Visible = true;
                    link.Text = "����������";
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "textValue_Click");
            }
        }

        private int iflash = 0;
        /// <summary>
        /// ͼԪ��˸
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
                writeZongheLog(ex, "timerFlash_Tick-ͼԪ��˸");
            }
        }

        /// <summary>
        /// ȫ�ǰ�Ǵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void textValue_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                IntPtr HIme = ImmGetContext(this.Handle);
                if (ImmGetOpenStatus(HIme)) //������뷨���ڴ�״̬
                {
                    int iMode = 0;
                    int iSentence = 0;
                    bool bSuccess = ImmGetConversionStatus(HIme, ref iMode, ref iSentence); //�������뷨��Ϣ
                    if (bSuccess)
                    {
                        if ((iMode & IME_CMODE_FULLSHAPE) > 0) //�����ȫ��
                            ImmSimulateHotKey(this.Handle, IME_CHOTKEY_SHAPE_TOGGLE); //ת���ɰ��
                    }
                }
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "timerFlash_Tick-ͼԪ��˸");
            }
        }

        /// <summary>
        /// �л��ж��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void cmbZhongdu_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbZhongdu.Enabled)
                {
                    cmbJinWuShi.Items.Clear();
                    string sql = "select �����Ҵ���,�������� from ����������";
                    if (cmbZhongdu.Text != "�����ɳ���")
                    {
                        sql += " where ����������='" + cmbZhongdu.Text + "'";
                    }

                    CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                    DataTable tab = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                    cmbJinWuShi.Items.Add("���о�����");
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
        /// ��ѯSQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">��ѯ���</param>
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
        private DataTable dataZhoubian = null;    /// ����ȫ�ֱ������ڴ�Ŷ���ģ��Ĳ�ѯ���� 
        private frmDisplay fmDis = null;

        /// <summary>
        /// �ۺϲ�ѯ�Ŵ�鿴���ݰ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void btnEnlarge_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataZhonghe == null)
                {
                    MessageBox.Show("������չʾ����ѡ��ѯ�����ݺ�Ŵ�鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
        /// ����Ŵ��������ݲ��رոô���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
        /// �ܱ߲�ѯ�ķŴ�鿴���ݰ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void btnEnlaData_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataZhoubian == null)
                {
                    MessageBox.Show("������չʾ����ѡ��ѯ�����ݺ�Ŵ�鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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