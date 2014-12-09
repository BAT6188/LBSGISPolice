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

        public string strRegion2 = ""; // �ɵ������ɳ���
        public string strRegion3 = ""; // �ɵ������ж�
        public string excelSql = "";   // ��ѯ����sql
        public string exportSql = "";  // ��ŵ���������SQL 
        public System.Data.DataTable dtExcel = null;//������

        public int anjianDouble = 0; //����״��״ͼ�ǲ���˫���¼�
        public ToolStripProgressBar toolPro;  // ���ڲ�ѯ�Ľ�������lili 2010-8-11
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

        private cl3Color.uc3Color f3Color;
        private clCXTJ.ucCXTJ fCXTJ;
        public ucAnjian(MapInfo.Windows.Controls.MapControl m, string s,string[] canStr,string getFromPath)
        {
            try
            {
                InitializeComponent();
                exePath = Application.StartupPath;    //����·��
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
                writeAnjianLog(ex, "���캯��");
            }
        }

        /// <summary>
        /// ��ʼ���ֶκ�����������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
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
                if (region != "˳����")
                {
                    string[] a = region.Split(',');
                    fCXTJ.comboBoxZD.Items.Clear();
                    fCXTJ.comboBoxZD.Items.Add("ȫ��");
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
                        //�����ݿ�
                        string aStr = "select �ɳ����� from �����ɳ��� where �ɳ����� not like '%���%'";
                        DataTable dt = new DataTable();
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(aStr);
                        fCXTJ.comboBoxZD.Items.Clear();
                        fCXTJ.comboBoxZD.Items.Add("ȫ��");
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
                    IResultSetFeatureCollection fc = Session.Current.Selections.DefaultSelection[this.queryTable];//�õ�ѡȡ�ж���ļ���
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
                        if (sLevelRange == "�ɳ���")
                        {
                            si = MapInfo.Data.SearchInfoFactory.SearchAll();
                            themeTitle = "���ɳ�������ͳ��";
                            xTitle = "�ɳ�����";
                            yTitle = "������";
                        }
                        else if (sLevelRange == "�ж�")
                        {
                            pcs = f["�����ɳ���"].ToString();
                            si = MapInfo.Data.SearchInfoFactory.SearchWhere("�����ɳ���='" + pcs + "'");
                            themeTitle = pcs + "���жӰ���ͳ��";
                            xTitle = "�ж���";
                            yTitle = "������";
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
                        fChart.CreateBarChart(dCounts, sLabels, "ͳ��ͼ", themeTitle, xTitle, yTitle);
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
                        fChart.CreatePieChart(dCounts, sLabels, f["name"].ToString() + "�������ͳ��ͼ");
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

        //��ʱ��ѯ
        private string strExpress = "select * from ������Ϣ t";
        private DataTable dataTable = null;
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkOption.Checked && dataGridExp.Rows.Count == 0)
                {
                    MessageBox.Show("����Ӳ�ѯ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (checkOption.Checked && getSqlString() == "")
                {
                    System.Windows.Forms.MessageBox.Show("��ѯ����д���,������!", "��ʾ");
                    return;
                }

                if (checkTime.Checked && Convert.ToDateTime(dateFrom.Value.Date + timeFrom.Value.TimeOfDay) >= Convert.ToDateTime(dateTo.Value.Date + timeTo.Value.TimeOfDay))
                {
                    MessageBox.Show("��ʼʱ��ӦС����ֹʱ��,������!", "ʱ�����ô���", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                if (strRegion != "˳����" && strRegion != "")
                {
                    string sRegion = strRegion;
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        sRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    addExp = " (�����ɳ��� in ('" + sRegion.Replace(",", "','") + "'))";
                }
                // add by fisher in 09-11-26
                if (strRegion1 != "")
                {
                    if (addExp.IndexOf("�����ɳ���") > -1)
                    {
                        addExp = addExp.Remove(addExp.LastIndexOf(")"));
                        addExp += " or �����ж� in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        addExp += " (�����ж� in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }


                if (checkOption.Checked == false && checkTime.Checked == false)
                {
                    strExpress = "select * from ������Ϣ t";
                    if (addExp != "")
                    {
                        strExpress += " where " + addExp;
                    }
                    isShowPro(false);
                    MessageBox.Show("���趨�ؼ��ʲ�ѯ��������ʱ������!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                this.Cursor = Cursors.WaitCursor;

                //�����ݿ�
                strExpress = "select * from ������Ϣ t where ";
                string generalOption = "";
                if (checkOption.Checked)
                {
                    generalOption = getSqlString();
                    strExpress += generalOption;
                }
                string timeOption = "";
                if (checkTime.Checked)
                {
                    timeOption = "����ʱ���ֵ>=to_date('" + (dateFrom.Value.Date + timeFrom.Value.TimeOfDay) + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + (dateTo.Value.Date + timeTo.Value.TimeOfDay) + "','yyyy-mm-dd hh24:mi:ss')";
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
                    strExpress += " and ������Դ='����'";
                }
                else
                {
                    strExpress += " and ������Դ='110'";
                }

                // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                if (strExpress.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                    strExpress += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                else
                    strExpress += " where (�����ֶ�һ is null or �����ֶ�һ='')";
                //-------------------------------------------------------
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeAnjianLog(ex, "anJianSearch");
                MessageBox.Show("��ѯ�������ô���,������!");
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
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("��������") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("��������");
                }
                TableInfoServer ti = new TableInfoServer("temAnjian", strConn.Replace("data source = ", "SRVR=").Replace("user id = ", "UID=").Replace("password = ", "PWD="), strExpress, ServerToolkit.Oci);
                ti.CacheSettings.CacheType = CacheOption.Off;

                queryTable = miConnection.Catalog.OpenTable(ti);
                this.toolPro.Value = 2;
                Application.DoEvents();

                MapInfo.Data.Table LTable;
                MICommand command = miConnection.CreateCommand();
                command.CommandText = "insert into �������� Select obj,'������Ϣ' as ����,��������,������� as ��_ID,��������,����_����,����ʱ���ֵ,�����ɳ���,�����ж�,���������� From " + queryTable.Alias + " t";
                MapInfo.Data.TableInfoNative ListTableInfo = new MapInfo.Data.TableInfoNative("��������");
                ListTableInfo.Temporary = false;
                ListTableInfo.TablePath = exePath + "\\��������.tab";
                MapInfo.Geometry.CoordSys LCoordsys;
                MapInfo.Data.GeometryColumn GC = (MapInfo.Data.GeometryColumn)(queryTable.TableInfo.Columns["obj"]);
                LCoordsys = GC.CoordSys;
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateFeatureGeometryColumn(LCoordsys));
                //ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStyleColumn());      //add by fisher in 10-01-11 ,�������style���ԣ���Ϊ�ֳ����ݿ��кö�����û�м�����Ϣ
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("����", 50));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("��������", 200));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("��_ID", 80));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("��������", 80));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("����_����", 150));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateDateColumn("����ʱ���ֵ"));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("�����ɳ���", 50));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("�����ж�", 100));
                ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("����������", 100));

                ListTableInfo.WriteTabFile();
                LTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ListTableInfo);
                command.ExecuteNonQuery();

                //��ͼ��ʾ
                FeatureLayer fl = new FeatureLayer(LTable);

                mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                //������fisherע����10-01-11����ּ��ȥ��ԭ��׷�ӵ���ʽ���ǣ���ʵ��ѡ��feature����˸
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
                    MessageBox.Show("������ʽ����\n" + ex.Message, "��ʾ");
                }
                queryTable.Close();

                this.toolPro.Value = 3;
                Application.DoEvents();

                command.CommandText = "select ��������,��_ID as �������,�������� from " + LTable.Alias;
                MIDataReader miDr = command.ExecuteReader();

                DataTable dt = new DataTable();
                DataColumn dCol = new DataColumn("��������", System.Type.GetType("System.String"));
                dt.Columns.Add(dCol);
                dCol = new DataColumn("�������", System.Type.GetType("System.String"));
                dt.Columns.Add(dCol);
                dCol = new DataColumn("��������", System.Type.GetType("System.String"));
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
                    MessageBox.Show("�޲�ѯ�����");
                }
                else
                {
                    dataGridViewJixi.DataSource = dt;
                    this.toolPro.Value = 4;
                    Application.DoEvents();
                    #region ��������
                    //OracleConnection Conn = new OracleConnection(strConn);
                    try
                    {
                        //Conn.Open();
                        //OracleCommand Cmd = Conn.CreateCommand();
                        CLC.ForSDGA.GetFromTable.GetFromName("������Ϣ", getFromNamePath);
                        string sRegion2 = strRegion2;
                        string sRegion3 = strRegion3;
                        //excelSql = "select ��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ɳ���,��������,�����ص���ַ,��������,������Դ,�����ֶ��ص�,�����ɳ�������,�����жӴ���,���������Ҵ���,�永��Ա,��ȡID,��ȡ����ʱ��,��������,�����ֶ�һ,�����ֶζ�,�����ֶ���,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y  " + strExpress.Substring(strExpress.IndexOf("from"));
                        excelSql = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " " + strExpress.Substring(strExpress.IndexOf("from"));
                        if (strRegion2 != "˳����")
                        {
                            if (strRegion2 != "")
                            {
                                if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
                                {
                                    sRegion2 = strRegion2.Replace("����", "����,��ʤ");
                                }
                                excelSql += " and (�����ɳ��� in ('" + sRegion2.Replace(",", "','") + "'))";
                                if (strRegion3 != "")
                                {
                                    excelSql = excelSql.Remove(excelSql.LastIndexOf(")"));
                                    excelSql += " or �����ж� in ('" + strRegion3.Replace(",", "','") + "'))";
                                }
                            }
                            else if (strRegion2 == "")
                            {
                                if (strRegion3 != "")
                                {
                                    excelSql += " and (�����ж� in ('" + strRegion3.Replace(",", "','") + "'))";
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

                        // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                        if (excelSql.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                            excelSql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                        else
                            excelSql += " where (�����ֶ�һ is null or �����ֶ�һ='')";
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
                    

                    //���������еı���ɫ
                    for (int i = 1; i < dataGridViewJixi.Rows.Count; i += 2)
                    {
                        dataGridViewJixi.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    labelCount1.Text = "�� " + dataGridViewJixi.Rows.Count.ToString() + " ����¼��";
                    labelCount1.Visible = true;
                }
                WriteEditLog(":" + strExpress, "��ѯ");
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

        //����ѯ�����ʾ�ڵ�ͼ��
        public MapInfo.Data.Table queryTable = null;
        MapInfo.Mapping.FeatureLayer currentFeatureLayer;

        private void tool4D_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewJixi.RowCount < 1)
                {
                    MessageBox.Show("���Ƚ��а�����ѯ");
                    return;
                }

                //ɾ��ר��ͼ��ͼ��
                closeLayerZTTB();
                this.mapControl1.Map.Adornments.Clear();
                mapControl1.Map.Legends.Clear();

                is4D = true;
                is3D = false;
                mapControl1.Tools.LeftButtonTool = "drawRectTool";
                //���ò�ѯ4Dʱ�Ĳ�ѯ����
                string temExp = strExpress.Replace("*", "�����ɳ��� as name,count(*) as shuliang");

                exp4DHead = "select a.*,b.shuliang from �ɳ��� a,(" + temExp + " ";

                exp4DEnd = " group by �����ɳ���) b where a.�ɳ�����=b.name(+)";
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
                    MessageBox.Show("���Ƚ��а�����ѯ");
                    return;
                }

                //ɾ��ר��ͼ��ͼ��
                closeLayerZTTB();
                this.mapControl1.Map.Adornments.Clear();
                mapControl1.Map.Legends.Clear();

                is3D = true;
                is4D = false;
                mapControl1.Tools.LeftButtonTool = "drawRectTool";
                //string caozuofu = comboTiaojian.Text;
                string temExp = strExpress.Replace("*", "�����ɳ��� as name,count(*) as shuliang");
                temExp += " group by �����ɳ���";
                exp3D = "select a.*,b.shuliang from �ɳ��� a,(" + temExp + ") b where a.�ɳ�����=b.name(+)";
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "tool3D_Click");
            }
        }

        //�������ܶ�ͼ
        private string strLayer = "";     //ר��ͼ�����ݲ�
        private string strAlies = "";   //ר��ͼ��ʶ��
        private void toolDotDensity_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewJixi.RowCount < 1)
                {
                    MessageBox.Show("���Ƚ��а�����ѯ");
                    return;
                }
                strAlies = "�����ܶ�";
                string field = "";
                frmDotDensity fDotDensity = new frmDotDensity();
                fDotDensity.strConn = strConn;
                fDotDensity.strRegion = strRegion;
                //add by siumo 2008-12-30:���Ȩ����ĳ������,û���ɳ���ѡ��
                fDotDensity.comboRegionLevel.Items.Clear();
                if (strRegion != "" && strRegion != "˳����")
                {
                    fDotDensity.comboRegionLevel.Items.Add("�ж�");
                    fDotDensity.comboRegionLevel.Items.Add("������");
                }
                else
                {
                    fDotDensity.comboRegionLevel.Items.Add("�ɳ���");
                    fDotDensity.comboRegionLevel.Items.Add("�ж�");
                    fDotDensity.comboRegionLevel.Items.Add("������");
                }
                fDotDensity.comboRegionLevel.Text = fDotDensity.comboRegionLevel.Items[0].ToString();

                if (fDotDensity.ShowDialog() == DialogResult.OK)
                {
                    if (this.queryTable != null)
                    {
                        this.queryTable.Close();
                    }
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��������") != null && mapControl1.Map.Layers["��������"].Enabled)
                    {
                        //MapInfo.Engine.Session.Current.Catalog.CloseTable("��������");
                        mapControl1.Map.Layers["��������"].Enabled = false;
                    }
                    switch (fDotDensity.comboRegionLevel.Text)
                    {
                        case "�ɳ���":
                            strLayer = "�ɳ���Ͻ����ͼ";
                            field = "�����ɳ���";
                            break;
                        case "�ж�":
                            strLayer = "���ж�Ͻ����ͼ";
                            field = "�����ж�";
                            break;
                        case "������":
                            strLayer = "������Ͻ����ͼ";
                            field = "����������";
                            break;
                    }
                    ClearDotDensityThemeAndLegend("ר��ͼ��", strAlies);  //������ϴ����ɵ��ܶ�ͼ
                    ClearRangeThemeAndLegend(strRangeLayer, "������Χ");   //�����
                    CloseOracleSpatialTable();  //�ȹر���ʱ��
                    OpenOracleSpatialTable(strLayer, fDotDensity.comboRegionLevel.Text, fDotDensity.comboBoxHighLevel.Text);  //��������ר��ͼ�ı�
                    FeatureLayer lyr = mapControl1.Map.Layers[0] as FeatureLayer;
                    bool bindSuccess = bindTableStatic(lyr, field, "������");
                    if (bindSuccess == false)
                    {
                        MessageBox.Show("���ݴ����������ѯ������");
                        return;
                    }
                    DotDensityTheme thm = new DotDensityTheme(lyr, "iCount",
                    strAlies, fDotDensity.dotColor, DotDensitySize.Large);
                    thm.ValuePerDot = fDotDensity.numPerDot;
                    lyr.Modifiers.Append(thm);
                    // ����ͼ��
                    Legend legend = mapControl1.Map.Legends.CreateLegend("ͼ��", "�����ܶ�", new Size(5, 5));
                    legend.Border = true;
                    ThemeLegendFrame frame = LegendFrameFactory.CreateThemeLegendFrame("ͼ��", "�����ܶ�", thm);
                    legend.Frames.Append(frame);
                    frame.Title = "������/ÿ��";
                    this.mapControl1.Map.Adornments.Append(legend);
                    // Set the initial legend location to be the lower right corner of the map control.
                    System.Drawing.Point pt = new System.Drawing.Point(0, 0);
                    pt.X = mapControl1.Size.Width - legend.Size.Width;
                    pt.Y = mapControl1.Size.Height - legend.Size.Height;
                    legend.Location = pt;

                    //���lyr���޶���,setview����,ʹ��try����
                    mapControl1.Map.SetView(lyr);
                }
                WriteEditLog(":" + strExpress, "�ܶ�ͼ");
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
                Session.Current.Catalog.CloseTable("ר��ͼ��");
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

                string currentSql = "Insert into " + this.queryTable.Alias + "  Select * From " + editTable.Alias;//����ͼԪ����
                if (level == "�ж�" && strRg != "ȫ��")
                {
                    currentSql += " where �����ɳ���='" + strRg + "'";
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
                MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable("ר��ͼ��");

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
                writeAnjianLog(ex, "createInfoPointLayer");
            }
        }

        //��ͳ��ֵ����
        private bool bindTableStatic(FeatureLayer lyr, string field, string content)
        {
            MIConnection miConn = new MIConnection();
            try
            {
                //�����ݿ�
                dataTable = new DataTable();
                DataColumn dCol = new DataColumn("name", System.Type.GetType("System.String"));
                dataTable.Columns.Add(dCol);
                dCol = new DataColumn("iCount", System.Type.GetType("System.Int32"));
                dataTable.Columns.Add(dCol);

                miConn.Open();
                MICommand miCmd = miConn.CreateCommand();
                miCmd.CommandText = "select " + field + " as name, count(*) as iCount from �������� group by " + field;
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
                    //���֮ǰ�󶨹�iCount�����Ƴ�
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

        //������ܶ�ר��ͼ��ͼ��
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

        //������Χ����ר��ͼ
        string strRangeLayer = "";
        string sLevelRange = "";
        string field = "";
        private void toolRegion_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewJixi.RowCount < 1)
                {
                    MessageBox.Show("���Ƚ��а�����ѯ");
                    return;
                }
                frmRegionOption fRegionOption = new frmRegionOption();
                fRegionOption.strConn = strConn;
                fRegionOption.strRegion = strRegion;
                //add by siumo 2008-12-30:���Ȩ����ĳ������,û���ɳ���ѡ��
                fRegionOption.comboRegionLevel.Items.Clear();
                //if (strRegion != "" && strRegion != "˳����")
                //{
                //    fRegionOption.comboRegionLevel.Items.Add("�ж�");
                //    fRegionOption.comboRegionLevel.Items.Add("������");
                //}
                //else
                //{
                fRegionOption.comboRegionLevel.Items.Add("�ɳ���");
                fRegionOption.comboRegionLevel.Items.Add("�ж�");
                fRegionOption.comboRegionLevel.Items.Add("������");
                //}
                fRegionOption.comboRegionLevel.Text = fRegionOption.comboRegionLevel.Items[0].ToString();
                if (fRegionOption.ShowDialog() == DialogResult.OK)
                {
                    if (this.queryTable != null)
                        this.queryTable.Close();
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��������") != null && mapControl1.Map.Layers["��������"].Enabled)
                    {
                        //MapInfo.Engine.Session.Current.Catalog.CloseTable("��������");
                        mapControl1.Map.Layers["��������"].Enabled = false;
                    }
                    strRangeLayer = "������";
                    switch (fRegionOption.comboRegionLevel.Text)
                    {
                        case "�ɳ���":
                            strRangeLayer = "�ɳ���Ͻ����ͼ";
                            field = "�����ɳ���";
                            break;
                        case "�ж�":
                            strRangeLayer = "���ж�Ͻ����ͼ";
                            field = "�����ж�";
                            break;
                        case "������":
                            strRangeLayer = "������Ͻ����ͼ";
                            field = "����������";
                            break;
                    }

                    sLevelRange = fRegionOption.comboRegionLevel.Text;

                    ClearDotDensityThemeAndLegend("ר��ͼ��", "�����ܶ�");
                    ClearRangeThemeAndLegend("ר��ͼ��", "������Χ");   //�����
                    CloseOracleSpatialTable();  //�ȹر���ʱ��
                    OpenOracleSpatialTable(strRangeLayer, fRegionOption.comboRegionLevel.Text, fRegionOption.comboBoxHighLevel.Text);  //��������ר��ͼ�ı�
                    if (fRegionOption.radioClass.Checked)
                    {
                        //�ּ�ͳ��ͼ
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
                    MessageBox.Show("���ݴ����������ѯ������");
                    return;
                }
                //mapControl1.Map.Zoom = new Distance(25, mapControl1.Map.Zoom.Unit);
                if (fRegionOption.comboRegionLevel.Text == "�ɳ���")
                {
                    mapControl1.Map.Scale = 100000;
                }
                else if (fRegionOption.comboRegionLevel.Text == "�ж�")
                {
                    mapControl1.Map.Scale = 60000;
                }
                else
                {
                    mapControl1.Map.Scale = 30000;
                }
                PieTheme pieTheme = new PieTheme(mapControl1.Map, lyr.Table, aStr);
                ObjectThemeLayer thmLayer = new ObjectThemeLayer("��������", null, pieTheme);
                this.mapControl1.Map.Layers.Add(thmLayer);
                pieTheme.DataValueAtSize /= 4;
                pieTheme.GraduateSizeBy = GraduateSizeBy.Constant;
                pieTheme.Clockwise = false;
                pieTheme.Graduated = false;
                pieTheme.StartAngle = 90;
                thmLayer.RebuildTheme();
                // ����ͼ��
                Legend legend = mapControl1.Map.Legends.CreateLegend("ͼ��", "������", new Size(5, 5));
                legend.Border = true;
                ThemeLegendFrame frame = LegendFrameFactory.CreateThemeLegendFrame("ͼ��", "��������", pieTheme);
                legend.Frames.Append(frame);
                frame.Title = "��������";
                this.mapControl1.Map.Adornments.Append(legend);
                // Set the initial legend location to be the lower right corner of the map control.
                System.Drawing.Point pt = new System.Drawing.Point(0, 0);
                pt.X = 2;
                pt.Y = mapControl1.Size.Height - legend.Size.Height;
                legend.Location = pt;

                //���lyr���޶���,setview����,ʹ��try����
                mapControl1.Map.SetView(lyr);
                WriteEditLog(":" + strExpress, "��ͼ");
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
                bool bindSuccess = bindTableStatic(lyr, field, "������");
                if (bindSuccess == false)
                {
                    MessageBox.Show("���ݴ����������ѯ������");
                    return;
                }
                //mapControl1.Map.Zoom = new Distance(10, mapControl1.Map.Zoom.Unit);
                if (fRegionOption.comboRegionLevel.Text == "�ɳ���")
                {
                    mapControl1.Map.Scale = 80000;
                }
                else if (fRegionOption.comboRegionLevel.Text == "�ж�")
                {
                    mapControl1.Map.Scale = 40000;
                }
                else
                {
                    mapControl1.Map.Scale = 20000;
                }
                BarTheme barTheme = new BarTheme(mapControl1.Map, lyr.Table, "iCount");
                ObjectThemeLayer thmLayer = new ObjectThemeLayer("������", null, barTheme);
                this.mapControl1.Map.Layers.Add(thmLayer);
                barTheme.Stacked = false;
                barTheme.GraduateSizeBy = GraduateSizeBy.Constant;
                // Allow the bars to be different heights. Setting to true would set each bar
                barTheme.GraduatedStacked = false;

                //���lyr���޶���,setview����,ʹ��try����
                mapControl1.Map.SetView(lyr);
                WriteEditLog(":" + strExpress, "��״ͼ");
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
                    case "�������":
                        disMethod = DistributionMethod.EqualCountPerRange;
                        break;
                    case "��Χ���":
                        disMethod = DistributionMethod.EqualRangeSize;
                        break;
                    case "��Ȼ���":
                        disMethod = DistributionMethod.NaturalBreak;
                        break;
                    case "��׼ƫ��":
                        disMethod = DistributionMethod.StandardDeviation;
                        break;
                    case "��λ��":
                        //disMethod= DistributionMethod.BIQuantile;
                        break;
                }
                FeatureLayer lyr = mapControl1.Map.Layers[0] as FeatureLayer;
                bool bindSuccess = bindTableStatic(lyr, field, "������");
                if (bindSuccess == false)
                {
                    MessageBox.Show("���ݴ����������ѯ������");
                    return;
                }
                RangedTheme theme = new RangedTheme(lyr, "iCount", "������Χ", Convert.ToInt16(fRegionOption.numericUpDownClass.Value), disMethod);
                int nBins = theme.Bins.Count;
                theme.Bins[0].Style.AreaStyle.Interior = new SimpleInterior((int)PatternStyle.Solid, fRegionOption.BtnColL.BackColor);
                theme.Bins[nBins - 1].Style.AreaStyle.Interior = new SimpleInterior((int)PatternStyle.Solid, fRegionOption.BtnColH.BackColor);
                theme.SpreadBy = SpreadByPart.Color;
                theme.ColorSpreadBy = ColorSpreadMethod.Rgb;
                theme.RecomputeStyles();
                lyr.Modifiers.Append(theme);
                // ����ͼ��
                Legend legend = mapControl1.Map.Legends.CreateLegend("ͼ��", "������Χ", new Size(5, 5));
                legend.Border = true;
                ThemeLegendFrame frame = LegendFrameFactory.CreateThemeLegendFrame("ͼ��", "������Χ", theme);
                legend.Frames.Append(frame);
                frame.Title = "������ (��)";
                this.mapControl1.Map.Adornments.Append(legend);
                // Set the initial legend location to be the lower right corner of the map control.
                System.Drawing.Point pt = new System.Drawing.Point(0, 0);
                pt.X = 2;
                pt.Y = mapControl1.Size.Height - legend.Size.Height;
                legend.Location = pt;

                //���lyr���޶���,setview����,ʹ��try����
                mapControl1.Map.SetView(lyr);
                WriteEditLog(":" + strExpress, "�ּ�ͼ");
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "createFenjiTheme");
            }
        }

        //���ר��ͼ��ͼ��
        private void ClearRangeThemeAndLegend(string tabName, string themeAlies)
        {
            try
            {
                //ɾ���ּ�ר��ͼ
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

                //ɾ��֮ǰ���õñ�״��״ר��ͼ
                MapLayerEnumerator mapLayerEnum = this.mapControl1.Map.Layers.GetMapLayerEnumerator(MapLayerFilterFactory.FilterByType(typeof(ObjectThemeLayer)));
                while (mapLayerEnum.MoveNext())
                {
                    this.mapControl1.Map.Layers.Remove(mapLayerEnum.Current);
                }

                anjianDouble = 0;

                //ɾ��ͼ��
                this.mapControl1.Map.Adornments.Clear();
                mapControl1.Map.Legends.Clear();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "ClearRangeThemeAndLegend");
            }
        }

        //���������ͷ��࣬���Ҹ��స�������������󶨵�ͼ����
        private string[] bindTableType(FeatureLayer lyr, string field)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                //�����ݿ�
                Conn.Open();
                string aStr = strExpress.Replace("*", "distinct ��������");
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
                        temStr += "sum(case �������� when '" + arrStr[i] + "' then 1 else 0 end) " + arrStr[i];
                    }
                    else
                    {
                        temStr += "sum(case �������� when '" + arrStr[i] + "' then 1 else 0 end) " + arrStr[i] + ",";
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

        //��˸����
        private Feature flashFt;
        private Style defaultStyle;
        private int k;
        private void dataGridViewJixi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                //���һ����¼�����е�ͼ��λ
                if (flashFt != null)
                {
                    try
                    {
                        flashFt.Style = defaultStyle;
                        flashFt.Update();
                    }
                    catch { }
                }

                FeatureLayer fLayer = (FeatureLayer)mapControl1.Map.Layers["��������"];
                Table tableInfo = fLayer.Table;


                MapInfo.Data.Feature feat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableInfo.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("��_ID='" + dataGridViewJixi.Rows[e.RowIndex].Cells["�������"].Value.ToString() + "'"));

                if (feat == null) return;

                OracleConnection oraConn = new OracleConnection(strConn);
                OracleDataAdapter oraOda = null;
                DataSet objset = new DataSet();
                try
                {
                    string sqlStr = "select t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y from ������Ϣ t where t.�������='" + dataGridViewJixi.Rows[e.RowIndex].Cells["�������"].Value.ToString() + "'";
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
                // ���´�����������ǰ��ͼ����Ұ�������ö������ڵ��ɳ���   add by fisher in 09-12-24
                MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                //MapInfo.Geometry.FeatureGeometry fg = new MapInfo.Geometry.Point(cSys, dp);
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchIntersectsFeature(feat, IntersectType.Geometry);
                si.QueryDefinition.Columns = null;
                Feature ftjz = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", si);
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
                label.Text = dataGridViewJixi["��������", e.RowIndex].Value.ToString();
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
                string[] winStr ={ objset.Tables[0].Rows[0][0].ToString(), objset.Tables[0].Rows[0][1].ToString(), "������Ϣ", dataGridViewJixi.Rows[e.RowIndex].Cells["�������"].Value.ToString(), dataGridViewJixi.Rows[e.RowIndex].Cells["��������"].Value.ToString() };

                //��˸Ҫ��
                DinPoint(winStr);
                k = 0;
                timer1.Start();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "dataGridViewJixi_CellClick");
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

        // ͼ���ϻ��㲢��λ
        private void DinPoint(string[] winStr)
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl1.Map.Layers["��ʱͼ��"];
                Table tableTem = ftla.Table;
                // ��������ж���
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
                    //MessageBox.Show("�ö���δ��λ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
                    pFeat["��_ID"] = winStr[3];
                    pFeat["����"] = winStr[2];
                    pFeat["����"] = winStr[4];
                    tableTem.InsertFeature(pFeat);
                    flashFt = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("��_ID='" + winStr[3] + "' and ����='" + winStr[2] + "'"));
                    defaultStyle = flashFt.Style;
                    if (flashFt != null)
                    {
                        mapControl1.Map.SetView(flashFt);
                    }
                    else
                    {
                        //MessageBox.Show("�ö���δ��λ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }
                else
                {
                    //MessageBox.Show("�ö���δ��λ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
                                        MessageBox.Show("��ѡ���������ݡ�");
                                    }

                                    is3D = false;
                                    mapControl1.Tools.LeftButtonTool = "Pan";
                                    WriteEditLog(":" + strExpress, "��ά��ʾ");
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

                                    //��ͬһ���£�ʹ������Ϊ�ָλ
                                    if ((beginYear == engYear && beginMonth == engMonth) || (tSpan.Days <= 10))
                                    {
                                        for (int i = 0; i <= tSpan.Days; i++)
                                        {
                                            D4policeData.Add(new List<GeoPoint>(5));

                                            DateTime aDate = dateTimeBegin.Date.AddDays(i);
                                            listTime.Add(aDate.ToShortDateString().Replace('-', '/'));
                                            string exp = exp4DHead + "and ����ʱ���ֵ>to_date('" + aDate.ToShortDateString() + "','yyyy-mm-dd') and ����ʱ���ֵ<=to_date('" + aDate.ToShortDateString() + " 23:59:59','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
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
                                                exp = exp4DHead + "and ����ʱ���ֵ>=to_date('" + dateTimeBegin + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
                                            }
                                            else if (beginYear == engYear && beginMonth == engMonth)
                                            {
                                                exp = exp4DHead + "and ����ʱ���ֵ>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + dateTimeEnd + "','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
                                            }
                                            else
                                            {
                                                exp = exp4DHead + "and ����ʱ���ֵ>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')" + exp4DEnd;
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
                                        MessageBox.Show("��ѡʱ�������ݡ�");
                                    }
                                    WriteEditLog(":" + strExpress, "��ά��ʾ");
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
           IntPtr hdcDest, //Ŀ���豸�ľ�� 
           int nXDest, // Ŀ���������Ͻǵ�X����
           int nYDest, // Ŀ���������Ͻǵ�Y����
           int nWidth, // Ŀ�����ľ��εĿ��
           int nHeight, // Ŀ�����ľ��εĳ���
           IntPtr hdcSrc, // Դ�豸�ľ��
           int nXSrc, // Դ��������Ͻǵ�X����
           int nYSrc, // Դ��������Ͻǵ�Y����
           System.Int32 dwRop // ��դ�Ĳ���ֵ
         );

        public static Bitmap capture()
        {
            try
            {
                //������ĻGraphics
                Graphics grpScreen = Graphics.FromHwnd(IntPtr.Zero);
                //������Ļ��С����λͼ
                Bitmap bitmap = new Bitmap(ScreenMapRect.Width, ScreenMapRect.Height, grpScreen);
                //����λͼ���Graphics
                Graphics grpBitmap = Graphics.FromImage(bitmap);
                //������Ļ������
                IntPtr hdcScreen = grpScreen.GetHdc();
                //����λͼ������
                IntPtr hdcBitmap = grpBitmap.GetHdc();
                //����Ļ���񱣴���ͼλ��
                BitBlt(hdcBitmap, 0, 0, ScreenMapRect.Width, ScreenMapRect.Height, hdcScreen, ScreenMapRect.Left, ScreenMapRect.Top, 0x00CC0020);
                //�ر�λͼ���
                grpBitmap.ReleaseHdc(hdcBitmap);
                //�ر���Ļ���
                grpScreen.ReleaseHdc(hdcScreen);
                //�ͷ�λͼ����
                grpBitmap.Dispose();
                //�ͷ���Ļ����
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Bitmap Bigbitmap = new Bitmap(1024, 1024, grpScreen);
                Graphics biggrpBitmap = Graphics.FromImage(Bigbitmap);
                biggrpBitmap.DrawImageUnscaled(bitmap, 0, 0);

                grpBitmap.Dispose();
                grpScreen.Dispose();

                //���ز���λͼ
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
                    //add by siumo 2008-12-30:���Ȩ����ĳ������,û���ɳ���ѡ��
                    fStatic.comboRegion.Items.Clear();
                    //if (strRegion != "" && strRegion != "˳����")
                    //{
                    //    fStatic.comboRegion.Items.Add("�ж�");
                    //    fStatic.comboRegion.Items.Add("������");
                    //}
                    //else
                    //{
                    fStatic.comboRegion.Items.Add("�ɳ���");
                    fStatic.comboRegion.Items.Add("�ж�");
                    fStatic.comboRegion.Items.Add("������");
                    //}
                    fStatic.comboRegion.Text = fStatic.comboRegion.Items[0].ToString();

                    fStatic.strConn = strConn;
                    fStatic.strRegion = strRegion;
                    fStatic.Show();
                    WriteEditLog("", "ͬ����");
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
                            if (strExpress == "select * from ������Ϣ t")
                            {
                                DialogResult dResult = MessageBox.Show("δ���ò�ѯ����,����ȫ����������ͳ��,�Ƿ����?", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dResult == DialogResult.No)
                                {
                                    return;  //δ���ò�ѯ�������в�ѯ������
                                }
                                // sExp = "select count(*),����1 from ������Ϣ��� group by ����1";
                            }
                            //else {
                            sExp = strExpress.Replace("*", "count(*),����1").Replace("from ������Ϣ", "from ������Ϣ���").Replace(" and (�����ֶ�һ is null or �����ֶ�һ='')"," ");
                            sExp += " group by ����1";
                            //}
                            fSta = new frmChart(conStr);
                            fSta.PieDataStatic(strConn, sExp);
                            fSta.connString = strConn;
                            fSta.strExe = sExp;
                            fSta.TopMost = true;
                            fSta.groupBox1.Visible = true;
                            fSta.Show();
                            WriteEditLog("", "�����ͳ��");
                            break;
                        case "staByDate":
                            //�Ȼ�ȡ��С�����ʱ��
                            if (strExpress == "select * from ������Ϣ t")
                            {
                                DialogResult dResult = MessageBox.Show("δ���ò�ѯ����,����ȫ����������ͳ��,�Ƿ����?", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dResult == DialogResult.No)
                                {
                                    return;  //δ���ò�ѯ�������в�ѯ������
                                }
                            }
                            sExp = strExpress.Replace("*", "min(����ʱ���ֵ),max(����ʱ���ֵ)").Replace("from ������Ϣ", "from ������Ϣ���").Replace(" and (�����ֶ�һ is null or �����ֶ�һ='')", " ");

                            fSta = new frmChart(conStr);
                            fSta.BarDataStatic(strConn, sExp);
                            fSta.TopMost = true;
                            fSta.groupBox1.Visible = false;
                            fSta.Show();
                            WriteEditLog("", "������ͳ��");
                            break;
                        case "staByTime":
                            if (strExpress == "select * from ������Ϣ t")
                            {
                                DialogResult dResult = MessageBox.Show("δ���ò�ѯ����,����ȫ����������ͳ��,�Ƿ����?", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dResult == DialogResult.No)
                                {
                                    return;  //δ���ò�ѯ�������в�ѯ������
                                }
                            }
                            sExp = strExpress.Replace("*", "����ʱ���ֵ");

                            fSta = new frmChart(conStr);
                            fSta.LineDataStatic(strConn, sExp);
                            fSta.TopMost = true;
                            fSta.groupBox1.Visible = false;
                            fSta.Show();
                            WriteEditLog("", "��ʱ��ͳ��");
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
                    if (f.Table.Alias.ToUpper() == "��������_SELECTION")
                    {
                        this.showDataGridViewLineOnlyOneTable(f["��_ID"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    writeAnjianLog(ex, "Feature_Selected");
                }
            }
        }

        public void showDataGridViewLineOnlyOneTable(string ��_ID)//��DataGridView������
        {
            try
            {
                for (int i = 0; i < this.dataGridViewJixi.Rows.Count; i++)
                {
                    if (this.dataGridViewJixi.Rows[i].Cells["�������"].Value.ToString() == ��_ID)
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
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("��������") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("��������");
                }
                this.dataGridViewJixi.DataSource = null;
                labelCount1.Text = "";
                strExpress = "select * from ������Ϣ t";
                WriteEditLog("", "���ò�ѯ����");
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

        // �����ʱͼ���ϵ����е�
        private void clearLinShi()
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl1.Map.Layers["��ʱͼ��"];
                Table tableTem = ftla.Table;
                // ��������ж���
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch (Exception ex) { writeAnjianLog(ex, "clearLinShi"); }
        }

        //��������ɼ�ʱ,����ر���Ӧ����ʱ��
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

        //�����ѯ������ʱ����
        private void clearCXFX()
        {
            try
            {
                dataGridViewJixi.DataSource = null;
                strExpress = "select * from ������Ϣ t";
                labelCount1.Visible = false;
                System.Windows.Forms.Label label = (System.Windows.Forms.Label)mapControl1.Parent.Controls["labelName"];
                label.Visible = false;
                if (this.queryTable != null)
                {
                    this.queryTable.Close();
                }
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("��������") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("��������");
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "clearCXFX"); }

            ClearDotDensityThemeAndLegend(strLayer, strAlies);  //����ϴ����ɵ��ܶ�ͼ
            ClearRangeThemeAndLegend(strRangeLayer, "������Χ");   //�����
        }

        //�����ɫԤ����ʱ����
        private void clearSSYJ()
        {
            string alias = "�ж�";
            if (f3Color.radioPCS.Checked)
            {
                alias = "�ɳ���";
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

        //�ر�ר��ͼ��
        private void closeLayerZTTB()
        {
            try
            {
                if (Session.Current.Catalog.GetTable("ר��ͼ��") != null)
                {
                    Session.Current.Catalog.CloseTable("ר��ͼ��");
                }
            }
            catch (Exception ex) { writeAnjianLog(ex, "closeLayerZTTB"); }
        }

        //����ر�ʱ,�ر�֮ǰδ�رյ�3D,4D����.
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
            if (e.RowIndex == -1) return;   //�����ͷ,�˳�

            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                DPoint dp = new DPoint();
                string sqlFields = "��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ɳ���,��������,�����ص���ַ,��������,������Դ,�����ֶ��ص�,�永��Ա,�����ɳ�������,�����жӴ���,���������Ҵ���,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                string strSQL = "select " + sqlFields + " from ������Ϣ t where �������='" + dataGridViewJixi.Rows[e.RowIndex].Cells["�������"].Value.ToString() + "'";

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

                    this.disPlayInfo(datatable, pt, "��ʱͼ��");
                    WriteEditLog(":�������='" + dataGridViewJixi.Rows[e.RowIndex].Cells["�������"].Value.ToString() + "'", "�鿴����");
                    return;
                }
                string[] winStr ={ datatable.Rows[0]["X"].ToString(), datatable.Rows[0]["Y"].ToString(), "������Ϣ", datatable.Rows[0]["�������"].ToString(), datatable.Rows[0]["��������"].ToString() };
                if (dp.x == 0 || dp.y == 0)
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2;
                    pt.Y = 10;

                    this.disPlayInfo(datatable, pt, "��ʱͼ��");
                    WriteEditLog(":�������='" + dataGridViewJixi.Rows[e.RowIndex].Cells["�������"].Value.ToString() + "'", "�鿴����");
                    return;
                }
                mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                pt.X += this.Width + 10;
                pt.Y += 80;
                this.disPlayInfo(datatable, pt,"��ʱͼ��");
                WriteEditLog(":�������='" + dataGridViewJixi.Rows[e.RowIndex].Cells["�������"].Value.ToString() + "'", "�鿴����");
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
            sw.WriteLine("��������:�� " + sFunc + "������," + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

        //��¼������¼
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'��������:��ѯ����','������Ϣ" + sql.Replace('\'', '"') + "','" + method + "')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "WriteEditLog");
            }
        }

        //���ð����е�comboField�ֶ�   add by fisher in 09-12-28
        string P_arrType = "";
        private void P_setfield()
        {
            try
            {
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '������Ϣ���'";
                DataTable dt = new DataTable();
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);

                comboField.Items.Clear();
                P_arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string aCol = dt.Rows[i][0].ToString();
                    string aType = dt.Rows[i][1].ToString();
                    if (aCol != "" && aCol != "MAPID" && aCol.IndexOf("�����ֶ�") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("����ʱ��") < 0 && aCol != "������Դ" && aCol != "��ȡ����ʱ��" && aCol.IndexOf("����") < 0 && aCol.Substring(0, 2) != "����")
                    {
                        comboField.Items.Add(aCol);
                        P_arrType += aType + ",";
                    }
                }
                comboField.Text = "��������";
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
                    System.Windows.Forms.MessageBox.Show("��ѯֵ����Ϊ�գ�", "��ʾ");
                    return;
                }

                if (this.textValue.Text.IndexOf("\'") > -1)
                {
                    System.Windows.Forms.MessageBox.Show("������ַ����в��ܰ���������!", "��ʾ");
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
                        this.dataGridExp.Rows.Add(new object[] { strExp, "����" });
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboTiaojian.Text + "   '" + this.textValue.Text.Trim() + "'";
                            if (this.comboTiaojian.Text.Trim() == "����")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "�ַ���" });
                            }
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.comboField.Text + "   " + this.comboTiaojian.Text + "   '" + this.textValue.Text.Trim() + "'";
                            if (this.comboTiaojian.Text.Trim() == "����")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "�ַ���" });
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
                            string text = this.dataGridExp.Rows[0].Cells["Value"].Value.ToString().Replace("����", "");

                            text = text.Replace("����", "").Trim();
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
                        this.comboTiaojian.Items.Add("����");
                        this.comboTiaojian.Items.Add("������");
                        this.comboTiaojian.Items.Add("����");
                        this.comboTiaojian.Items.Add("���ڵ���");
                        this.comboTiaojian.Items.Add("С��");
                        this.comboTiaojian.Items.Add("С�ڵ���");
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        this.comboTiaojian.Items.Add("����");
                        this.comboTiaojian.Items.Add("������");
                        this.comboTiaojian.Items.Add("����");
                        break;
                }
                this.comboTiaojian.Text = "����";
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "setYunsuanfuValue()");
            }
        }

        private string getSqlString()//ת���ַ���
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";

                for (int i = 0; i < this.dataGridExp.Rows.Count; i++)
                {
                    string type = this.dataGridExp.Rows[i].Cells["Type"].Value.ToString();
                    string str = this.dataGridExp.Rows[i].Cells["Value"].Value.ToString();
                    if (type == "����")
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
                writeAnjianLog(ex, "getSqlString()");

                return "";
            }
        }

        /* �������Զ���ȫ���� */

        /// <summary>
        /// �Զ���ȫ����(add by LiLi in 2010-5-21)
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
                writeAnjianLog(ex, "getListBox");
                return null;
            }
        }

        /// <summary>
        /// ��Ϊ�̶�ֵʱ�Զ����(add by LiLi in 2010-5-21)
        /// </summary>
        /// <param name="colName">����</param>
        /// <param name="tableName">����</param>
        /// <param name="listBox">��ʾ�Զ���ȫֵ�Ŀؼ�</param>
        /// <returns>������</returns>
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
                writeAnjianLog(ex, "MatchShu");
                return null;
            }
        }

        private void textValue_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.textValue.Text.Trim(), this.comboField.Text, "������Ϣ");

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
                DataTable dt = MatchShu(this.comboField.Text, "������Ϣ");

                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "textValue_Click_1");
            }
        }

        // ���س�ʱ��ѯ
        private void textValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                anJianSearch();
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
                writeAnjianLog(ex, "isShowPro");
            }
        }

        private void LinklblHides_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;

                if (link.Text == "����������")
                {
                    this.textValue.Visible = false;
                    groupBox1.Visible = false;
                    link.Text = "��ʾ������";
                }
                else
                {
                    this.textValue.Visible = true;
                    groupBox1.Visible = true;
                    link.Text = "����������";
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
                writeAnjianLog(ex, "textValue_MouseDown");
            }
        }
    }
}