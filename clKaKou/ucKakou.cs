////////////////////////////////////////////////////////////
///==============˳�¹����ΰ����ڳ���====================///
///////////////////////////////////////////////////////////
//�������ڣ�20090708
//������:jie.zhang
//www.lbschina.com.cn

//�޸ļ�¼
//20090709 jie.zhang ����ΰ����ڵĳ�ʼ�����򣬰��������Ĵ�����趨�� ��ʶ 20090709
//20090803 jie.zhang ��ӵ���excel��ģ����ѯ
//20090804 jie.zhang ���������ʱ��=˳��ţ� ��ӷ�ҳ���ܡ�
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Dialogs;
using MapInfo.Windows.Controls;

using EpoliceOracleDal;


namespace clKaKou
{
    public partial class ucKakou : UserControl
    {
        //Boolean UseDll = true; 

        EHL.ATMS.TGS.Interface.SD.TGSManage ehltgs = new EHL.ATMS.TGS.Interface.SD.TGSManage();  //�׻�¼�ӿ�  
        GDW_GIS_Interface.communication gdwcom = new GDW_GIS_Interface.communication(); //�ߵ����ӿ�
       
        //Ϊ�ۺϲ�ѯ�ṩͼƬ��������ַ
        public string ehlserver = string.Empty; 
        public string gdwserver = string.Empty;
        public string bkserver = string.Empty;
        public string photoserver = string.Empty;

        public MapControl mapControl1 = null;
        private string mysqlstr;                       //���ݿ������ַ���
        private string[] StrCon;                      //���ݿ������ַ�����
        public string strRegion = string.Empty;  //�ɳ���Ȩ��
        public string strRegion1 = "";              //�ж�Ȩ��
        public string user = "";
        private string getfrompath = string.Empty;//GetFromName�������ļ�λ��

        public System.Windows.Forms.Panel panError;       // ���ڵ���������ʾ
        public System.Data.DataTable dtExcel = null; //��ͼҳ�����ݵ�����ť

        frmtip ftip = new frmtip();

        private int VideoPort = 0;           //ͨѶ�˿�
        private string[] VideoString;     // ��Ƶ�����ַ�
        private ToolStripLabel st = null;
        private static NetworkStream ns = null;  //

        private Boolean vf = false; // ͨѶ�Ƿ��Ѿ����ӵı�ʶ
        private string VEpath = string.Empty;

        private string ALARMSYS = string.Empty;  // ������Ե�ģ��
        private string ALARMUSER = string.Empty; //���ص�λ
        double SCHDIS = 0;    //��ѯ�뾶

        public ToolStripProgressBar toolPro;  // ���ڲ�ѯ�Ľ�������lili 2010-8-10
        public ToolStripLabel toolProLbl;     // ������ʾ�����ı���
        public ToolStripSeparator toolProSep; 

        private ToolStripDropDownButton tddb;


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


        /// <summary>
        /// ��ȡ���ڲ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-27
        /// </summary>
        /// <param name="m">��ͼ�ؼ�</param>
        /// <param name="t"></param>
        /// <param name="s">���ݿ����Ӳ���</param>
        /// <param name="zh">ֱ��ָ�ӵı�ʾ</param>
        public ucKakou(MapControl mc, string[] sqlcon, ToolStripLabel tools, ToolStripDropDownButton dt, int port, string[] vs, string videopath, string Alarmsys, string AlarmUser, double SchDis, string streg, string usern,string getfnpath,bool isEvent)
        {
            InitializeComponent();
            try
            {

                Writelog("���ñ���,using EpoliceOracleDal;");

                mapControl1 = mc;

                StrCon = sqlcon;
                mysqlstr = "data source=" + StrCon[0] + ";user id=" + StrCon[1] + ";password=" + StrCon[2];
                this.tddb = dt;

                getfrompath = getfnpath;

                if (isEvent)
                {
                    mapControl1.Tools.Used -= new ToolUsedEventHandler(Tools_Used);
                    mapControl1.Tools.FeatureAdded -= new FeatureAddedEventHandler(Tools_FeatureAdded);
                    mapControl1.Tools.FeatureSelected -= new FeatureSelectedEventHandler(Tools_FeatureSelected);

                    mapControl1.Tools.Used += new ToolUsedEventHandler(Tools_Used);
                    mapControl1.Tools.FeatureAdded += new FeatureAddedEventHandler(Tools_FeatureAdded);
                    mapControl1.Tools.FeatureSelected += new FeatureSelectedEventHandler(Tools_FeatureSelected);
                }

                this.bindingNavigator1.Visible = true;
                this.bindingNavigator2.Visible = true;

                SetDrawStyle();

                //������Ϣ
                this.ALARMSYS = Alarmsys;
                this.ALARMUSER = AlarmUser;
                this.SCHDIS = SchDis;

                VideoPort = port;
                VideoString = vs;
                this.st = tools;
                this.VEpath = videopath;

                this.strRegion = streg;
                this.user = usern;

                ftip.GetPara(StrCon , mapControl1, ALARMSYS, ALARMUSER, strRegion, user);
                ftip.Visible = false;
                //ftip.timalarm.Enabled = true;

                InitAlarmSet();//��ʼ����������

                this.comboxTable.Items.Clear();
                this.comboxTable.Items.Add("����ͨ����Ϣ");
                this.comboxTable.Items.Add("����������Ϣ");
                this.comboxTable.Items.Add("�ΰ�������Ϣ");
                this.comboxTable.Text = this.comboxTable.Items[0].ToString();

                this.label8.Text = "1�����ԡ���ǡ��ַ������ѯ������\n\r"+
                                   "2��ģ����ѯʱ���빴ѡ��ģ����ѯ���\n\r" +
                                   "3��֧�ֹؼ���ģ����ѯ�����磺Ҫ��ѯ\n\r   ��X12345���ƣ������롰1234����\n\r   ����X123���Ƚ���ģ����ѯ��\n\r" +
                                   "4����ʹ��ͨ���ַ�����������ģ����ѯ��\n\r   ÿһ������������һ���ַ�����ͬʱʹ\n\r   " +
                                      "�ö�������������磺Ҫ��ѯ��X12345\n\r   ���ƣ������롰��X��2345��������X12\n\r   34�����򡰣���12345���Ƚ���ģ����ѯ��\n\r" +
                                   "5�������������Ӵ�ģ����ѯ�ٶ���Խ�\n\r   ���������ĵȺ�";

            }
            catch (Exception ex)
            {
                writeToLog(ex, "01-��ȡ���ڲ���");
            }
        }            
    

        /// <summary>
        /// ��ʼ����������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void InitAlarmSet()
        {
            try
            {
                if (this.ALARMSYS == "ϵͳ")
                {
                    this.rdboxsys.Checked = true;
                    this.rdboxmod.Checked = false;
                }
                else if (this.ALARMSYS == "ģ��")
                {
                    this.rdboxmod.Checked = true;
                    this.rdboxsys.Checked = false;
                }

                if (this.ALARMUSER == "����")
                {
                    this.rdboxall.Checked = true;
                    this.rdboxuser.Checked = false;
                }
                else if (this.ALARMUSER == "�û�")
                {
                    this.rdboxall.Checked = false;
                    this.rdboxuser.Checked = true;
                }

                this.txtdist.Text = this.SCHDIS.ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InitAlarmSet-02-��ʼ����������");
            }
        }

        /// <summary>
        /// ������Ƶ�鿴����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ns1"></param>
        /// <param name="vf1"></param>
        public void getNetParameter(NetworkStream ns1, Boolean vf1)
        {
            try
            {
                ns = ns1;
                this.vf = vf1;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getNetParameter-03-������Ƶ�鿴����");
            }
        }               
        
        /// <summary>
        /// ��ʼ���ΰ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void InitKK()
        {
            try
            {
                ClearKaKou();

                CreateKKLyr();//�����ΰ�����ͼ��
                
                if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                {
                    strRegion = strRegion.Replace("����", "����,��ʤ");
                }            
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InitKK-04-��ʼ���ΰ�����");
            }
            //AddGrid();  //������ݵ�Grid===�޸ĵ���δ����Ҫ��GRID����ʾ
        }

        /// <summary>
        /// ���õ�ǰͼ��ɱ༭
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="layername">ͼ������</param>
        private void SetLayerEdit(string layername)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layername], true);
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layername], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetLayerEdit-05-��ʼ���ΰ�����");
            }
        }

        /// <summary>
        /// �����ΰ�����ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void CreateKKLyr()
        {
            //writelog("��������ͼ�㿪ʼ" + System.DateTime.Now.ToString());
            try
            {

                if (mapControl1.Map.Layers["KKLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("KKLayer");
                }

                if (mapControl1.Map.Layers["KKLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("KKLayer");
                Table tblTemp = Cat.GetTable("KKLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("KKLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 50));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("��_ID",50));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("����", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);

                //��ӱ�ע
                string activeMapLabel = "KKLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                AddKKFtr();

                this.SetLayerEdit("KKLayer");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateKKLyr-06-�����ΰ�����ͼ��");
            }
        }

        /// <summary>
        /// ����ΰ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void AddKKFtr() 
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;

                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["KKLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("KKLayer");

                string sqlcmd = "Select * from �ΰ�����ϵͳ";

                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tempname = dr["��������"].ToString();
                        string camid = dr["���ڱ��"].ToString();

                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle();
                        cs.ApplyStyle(new BitmapPointStyle("zakk.bmp", BitmapStyles.None, System.Drawing.Color.Red, 24));

                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = tempname;
                        ftr["ID"] = camid;
                        ftr["����"] = "�ΰ�����";
                        ftr["��_ID"] = camid;
                        tblcar.InsertFeature(ftr);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "AddKKFtr-07-����ΰ�����");
            }        
        }

        /// <summary>
        /// �Ҽ�����ͼ�꽫�ÿ��ڷ�����ҵ������ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="rfc">��ͼҪ��</param>
        public void AddBayoneDireCond(IResultSetFeatureCollection rfc)
        {
            try
            {
                if (rfc == null) {
                    MessageBox.Show("����ѡ�񿨿�ͼ������ԣ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                string carName = "";   // ��������
                string carBayo = "";   // ���ڷ���
                frmBayonetDire frmBay = new frmBayonetDire();
                frmBay.comBanyoneDire.Items.Clear();

                if (rfc != null)
                {
                    if (rfc.Count > 0)
                    {
                        foreach (Feature f in rfc)
                        {
                            frmBay.lblBayonet.Text = carName = f["Name"].ToString();
                            break;
                        }
                    }
                }

                DataTable table = GetTable("select distinct ���ڷ��� from �ΰ�����ϵͳ where ��������='" + frmBay.lblBayonet.Text + "'");  
                for (int i = 0; i < table.Rows.Count; i++)     // ��ӿ��ڷ���
                {
                    frmBay.comBanyoneDire.Items.Add(table.Rows[i][0].ToString());
                }
                frmBay.comBanyoneDire.SelectedIndex = 0;

                if (frmBay.ShowDialog() == DialogResult.OK)
                {
                    carBayo = frmBay.comBanyoneDire.Text;

                    if (carName == string.Empty || carBayo == string.Empty){
                        MessageBox.Show("���ݲ�ȫ�����ܴ���������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    this.tabControl1.SelectedTab = tabPage3;
                    this.dataGridViewValue.Rows.Clear();
                    // �������
                    this.dataGridViewValue.Rows.Add(new object[] { "�������� ���� '" + carName + "'", "�ַ���" });
                    this.dataGridViewValue.Rows.Add(new object[] { "���� ���ڷ��� ���� '" + carBayo + "'", "�ַ���" });
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "AddBayoneDireCond");
            }
        }

        /// <summary>
        /// ��DataGrid
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void AddGrid()
        {
            try
            {
                DataTable dt = GetTable("select ��������,���ڱ��,X,Y from �ΰ�����ϵͳ");
                this.lblCount.Text = "����" + dt.Rows.Count.ToString() + "����¼";
                this.dataGridViewKakou.DataSource = dt;
                this.dataGridViewKakou.Refresh();                
            }
            catch (Exception ex)
            {
                writeToLog(ex, "AddGrid08-��DataGrid");
            }           
        }

        /// <summary>
        /// �������ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void ClearKaKou()
        {
            try
            {
                if (mapControl1.Map.Layers["KKLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("KKLayer");
                }

                if (mapControl1.Map.Layers["KKLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKLabel");
                }               

                if (mapControl1.Map.Layers["KKSearchLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("KKSearchLayer");
                }

                if (mapControl1.Map.Layers["KKSearchLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKSearchLabel");
                }

                if (mapControl1.Map.Layers["KKDrawLayer"] != null)
                {
                    mapControl1.Map.Layers.Remove("KKDrawLayer");
                }

                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
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

                clearFeatures("�鿴ѡ��");
 
                this.dataGridViewKakou.DataSource =null ;
                this.dgvres.DataSource = null;
                this.txtBoxCar.Text = "";
                this.chkmh.Checked = false;
                this.dataGridViewValue.DataSource = null;
            }
            catch(Exception ex)
            {
                writeToLog(ex, "ClearKaKou-09-�������ͼ��");
            }
        }

        /// <summary>
        /// ���������ʱͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void ClearKaKouTemp() 
        {
            try
            {                
                clearFeatures("KaKouTrackLayer");
                clearFeatures("KKSearchLayer");
                clearFeatures("�鿴ѡ��");
                clearFeatures("KKLayer");
                CreateKKLyr();

                this.dataGridViewKakou.DataSource = null;
                this.dgvres.DataSource = null;
                //this.txtBoxCar.Text = "";
                //this.chkmh.Checked = false;
                this.dataGridViewValue.DataSource = null;

                PageNow1.Text = "0";
                PageNow2.Text = "0";

                PageNum1.Text = "/ {0}";
                PageNum2.Text = "/ {0}";

                RecordCount1.Text = "0 ��";
                this.RCount2.Text = "0 ��";

                PageNumber.Text = "100";
                TextNum1.Text = "100";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ClearKaKouTemp-10-���������ʱͼ��");
            }
        }

        /// <summary>
        /// ���ͼ��ͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="tabAlias">ͼ������</param>
        private void clearFeatures(string tabAlias)
        {
            try
            {
                //�����ͼ����ӵĶ���
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
                writeToLog(ex, "clearFeatures-11-���ͼ��ͼԪ");
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            Writelog("����������Ϣ��ѯ����");
            //this.groupBox3.Visible = false;
            SearchCarPass();
        }


        /// <summary>
        /// ��־���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="msg">�������</param>
        private void Writelog(string msg)
        {
            try
            {
                string filepath = Application.StartupPath + "\\KKrec.log";
                msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + ":" + msg;

                StreamWriter sw = File.AppendText(filepath);
                sw.WriteLine(msg);
                sw.Flush();
                sw.Close();
            }
            catch(Exception ex)
            {
                writeToLog(ex, "Writelog");
            }
        }

        string SQLSearch = string.Empty;
        /// <summary>
        /// ��ѯͨ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void SearchCarPass()
        {
            try
            {
                if (this.txtBoxCar.Text != "")
                {

                    string sqlend = string.Empty;
                    fuzzyFlag = false;


                    if (this.chkmh.Checked == true)
                    {
                        sqlend = "like  '%" + this.txtBoxCar.Text.Trim() + "%'";
                    }
                    else
                    {
                        sqlend = "='" + this.txtBoxCar.Text.Trim() + "'";
                        fuzzyFlag = true;
                    }

                    if (sqlend.IndexOf('?') > -1)
                    {
                        sqlend = "like  '" + this.txtBoxCar.Text.Trim().Replace('?', '%') + "'";
                    }

                    DateTime dts = Convert.ToDateTime(this.dateFrom.Text);
                    DateTime dte = Convert.ToDateTime(this.dateTo.Text);
                    DataTable dt = new DataTable();

                    if (dts >= dte)
                    {
                        MessageBox.Show("��ʼʱ��ӦС����ֹʱ��,������!", "ʱ�����ô���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    //////////////////////////////////  xxxxxx    1
                    Application.DoEvents();
                    this.Cursor = Cursors.WaitCursor;

                    ClearKaKouTemp();

                    string sqlstr = string.Empty;

                    if (this.txtBoxCar.Text.Trim().IndexOf("�޺���") > -1)
                        sqlstr = "select * from V_Cross where ͨ��ʱ��> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ͨ��ʱ��< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and (�������� ='�޺���' or �������� is null or ��������='') order by ͨ��ʱ��";
                    else
                        sqlstr = "select * from V_Cross where ͨ��ʱ��> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ͨ��ʱ��< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and �������� " + sqlend + " order by ͨ��ʱ��";

                    Writelog("��ѯִ��sql��" + sqlstr);

                    DataTable dt2 = null;
                    DataTable dt3 = null;

                    FrmProgress frmpro = new FrmProgress();
                    frmpro.progressBar1.Maximum = 4;
                    frmpro.Show();

                    frmpro.progressBar1.Value = 0;

                    messageStr = "";  // ������ʾ�ⲿ�����ṩ�̵Ĳ�ѯ������쳣��Ϣ

                    Application.DoEvents();
                    frmpro.label1.Text = "���ڲ�ѯ�׻�¼����.....";
                    try
                    {
                        Writelog("�����׻�¼���ݲ�ѯ����");
                        this.st.Text = "���ڲ�ѯ�׻�¼��Ϣ......";
                        dt = ehltgs.GetPassCarInfo(sqlstr, ref ehlserver);                  // �׻�¼��ѯ�ӿ�
                        Writelog("����׻�¼���ݽ��,������:" + dt.Rows.Count.ToString());
                        messageStr += "�׻�¼��������ѯ��" + dt.Rows.Count + "������\t\n";
                    }
                    catch
                    {
                        messageStr += "�׻�¼������ͨѶ��������,�޷���ѯ������\t\n";
                    }
                    Application.DoEvents();

                    this.toolPro.Value = 1;
                    frmpro.progressBar1.Value = 1;


                    frmpro.label1.Text = "���ڲ�ѯ�ߵ�������.....";
                    try
                    {
                        Writelog("���͸ߵ������ݲ�ѯ����");
                        this.st.Text = "���ڲ�ѯ�ߵ�����Ϣ......";
                        dt2 = gdwcom.QueryData(sqlstr, ref gdwserver);  //�ߵ�����ѯ�ӿ�
                        Writelog("��øߵ������ݽ��,������:" + dt2.Rows.Count.ToString());
                        messageStr += "�ߵ�����������ѯ��" + dt2.Rows.Count + "������\t\n";
                    }
                    catch 
                    {
                        messageStr += "�ߵ���������ͨѶ��������,�޷���ѯ������\t\n"; 
                    }
                    Application.DoEvents();

                    this.toolPro.Value = 2;
                    frmpro.progressBar1.Value = 2;

                    frmpro.label1.Text = "���ڲ�ѯ��������.....";

                    try
                    {
                        Writelog("���ͱ������ݲ�ѯ����");
                        this.st.Text = "���ڲ�ѯ������Ϣ......";
                        PassDataQuery psqy = new PassDataQuery();
                        Writelog("���ñ���������PassDataQuery psqy = new PassDataQuery();");

                        bkserver = "";
                        Writelog("���ñ���������bkserver = " + bkserver);
                        dt3 = psqy.QueryPassData(sqlstr, ref bkserver);    //������ѯ�ӿ�
                        Writelog("���ñ���������psqy.QueryPassData(sqlstr, ref bkserver);");
                        Writelog("�����������,sqlstr:" + sqlstr + ",bkserver:" + bkserver);

                        Writelog("��ñ������ݷ�������ַ:" + bkserver);
                        Writelog("��ñ������ݽ��,������:" + dt3.Rows.Count.ToString());
                        messageStr += "������������ѯ��" + dt3.Rows.Count + "������\t\n";

                    }
                    catch 
                    {
                        messageStr += "����������ͨѶ��������,�޷���ѯ������\t\n"; 
                    }

                    Application.DoEvents();

                    this.toolPro.Value = 3;
                    frmpro.progressBar1.Value = 3;
                    this.st.Text = "�������ݲ�ѯ���......";

                    #region ����������ѯ
                    try
                    {
                        if (dt != null && dt2 != null && dt3 != null)   //�׻�¼+�ߵ��� + ����
                        {
                            dt2.Merge(dt3, false);
                            dt.Merge(dt2, false);
                        }
                        else if (dt == null && dt2 != null && dt3 != null)   //�ߵ��� + ����
                        {
                            dt2.Merge(dt3, false);
                            dt = dt2;
                        }
                        else if (dt != null && dt2 == null && dt3 != null)   //�׻�¼ + ����
                        {
                            dt.Merge(dt3, false);
                        }
                        else if (dt != null && dt2 != null && dt3 == null)    // �ߵ���+�׻�¼
                        {
                            dt.Merge(dt2, false);
                        }
                        else if (dt != null && dt2 == null && dt3 == null)  // �׻�¼
                        {
                        }
                        else if (dt == null && dt2 != null && dt3 == null)  // �ߵ���
                        {
                            dt = dt2;
                        }
                        else if (dt == null && dt2 == null && dt3 != null)  // ����
                        {
                            dt = dt3;
                        }
                        else if (dt == null && dt2 == null && dt3 == null)  // ȫ��
                        {
                            MessageBox.Show("���ΰ����ڽӿڵķ�����ͨѶ��������,�޷���ѯ�����ݡ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("�ӶԷ���������ȡ����ʱ��������");
                        writeToLog(ex, "clKaKou-ucKakou-12-�ӶԷ���������ȡ����ʱ��������");
                        return;
                    }
                    #endregion


                    Application.DoEvents();

                    // ������������
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (chkmh.Checked == false)
                            dt = RefreshData(dt, "1");

                        dt = InsertColumns(dt); //������к�
                        
                        Pagedt1 = dt;
                        InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                    }
                    frmpro.progressBar1.Value = 4;

                    frmpro.Close();

                    //clickTable = new DataTable();
                    //////////////////////////////////
                    if (dt != null && dt.Rows.Count > 0)
                        this._exportDT  = dt;

                    if (dtExcel != null) dtExcel.Clear();
                    dtExcel = dt;

                    Application.DoEvents();

                    //if (dt.Rows.Count > 0)
                    //    CreateKakouTrack(dt); //��ʾ���������Ŀ��ڲ����߽������ӡ�   //  kakouTrack

                    WriteEditLog(SQLSearch, "����������ѯ", "V_CROSS");

                    Writelog("�ڵ�ͼ����ʾ��Ϣ�����");

                    isShowPro(false);

                    MessageBox.Show(messageStr, "��ѯ�����ʾ", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    isShowPro(false);
                    MessageBox.Show("��ѯ�ؼ��ֲ���Ϊ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "SearchCarPass-12-��ȡ�ߵ����׻�¼����ʱ��������");
                //writeToLog(ex, "clKaKou-ucKakou-12-xxxxx 3");
            }
            finally
            {
                Application.DoEvents();
                this.Cursor = Cursors.Default;
            }           
        }

        /// <summary>
        /// �����ݰ���ͨ��ʱ�䡱����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt1">����Դ</param>
        /// <param name="num">��������</param>
        /// <returns>����������</returns>
        private DataTable RefreshData(DataTable dt1,string num)
        {
            try
            {
                DataTable dt2;
                dt2 = dt1.Copy();
                string strExpr = "ͨ��ʱ��";
                string strSort = string.Empty;

                if (num == "1")
                    strSort = "ͨ��ʱ�� desc";
                else if (num == "2")
                    strSort = "ͨ��ʱ�� desc";   // �ǲ���asc

                DataRow[] foundRows = dt1.Select("", strSort);
                dt2.Rows.Clear();
                foreach (DataRow dr in foundRows)
                {
                    dt2.ImportRow(dr);
                }

                return dt2;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "RefreshData");
                return null;
            }
        }
        /// <summary>
        /// �����ݱ���ӡ����кš���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt">���ݱ�</param>
        /// <returns>�����ݱ��������ˡ����кš���</returns>
        private DataTable InsertColumns(DataTable dt)
        {
            try
            {
                dt.Columns.Add("���к�", System.Type.GetType("System.String"));
                for (int i = dt.Rows.Count ; i >0; i--)
                {
                    dt.Rows[dt.Rows.Count - i]["���к�"] = (i).ToString();
                }
            }
            catch (Exception ex) { writeToLog(ex, "InsertColumns-13-�����"); }
            return dt;

            //DataTable dtnew = new DataTable();
            //try
            //{
            //    dtnew.Columns.Add("���к�", System.Type.GetType("System.String"));
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        dtnew.Columns.Add(dt.Columns[i].ToString(), System.Type.GetType("System.String"));
            //    }
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        DataRow dr = dtnew.NewRow();
                    
            //        dr["���к�"]= (i + 1).ToString();
                    
            //        for(int j=0;j<dt.Columns.Count ;j++)
            //        {
            //          dr[dt.Columns[j].ToString()] = dt.Rows[i][j].ToString();
            //        }
            //        dt.Rows.Add(dr);
            //    }
            //}
            //catch { }
            //return dt;
        }




        Color[] colors = new Color[141] { Color.AliceBlue,Color.AntiqueWhite,Color.Aqua,Color.Aquamarine,Color.Azure,Color.Beige,                               
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
                                            Color.Turquoise,Color.Violet,Color.Wheat,Color.White,Color.WhiteSmoke,Color.Yellow,Color.YellowGreen};

        private bool fuzzyFlag = false;         // �ڿ�������ʱ�����ж��Ƿ��ǳ���ģ����ѯ lili 2010-12-21

        /// <summary>
        /// �������ڲ�ѯͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt">���ݱ�</param>
        private void CreateKakouTrack(DataTable dt)
        {
            try
            {
                try
                {
                    if (mapControl1.Map.Layers["KKSearchLayer"] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("KKSearchLayer");
                    }

                    if (mapControl1.Map.Layers["KKSearchLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("KKSearchLabel");
                    }


                    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                    //������ʱ��
                    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("KKSearchLayer");
                    Table tblTemp = Cat.GetTable("KKSearchLayer");
                    if (tblTemp != null) //Table exists close it
                    {
                        Cat.CloseTable("KKSearchLayer");
                    }

                    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 50));

                    tblTemp = Cat.CreateTable(tblInfoTemp);
                    FeatureLayer lyr = new FeatureLayer(tblTemp);
                    //mapControl1.Map.Layers.Add(lyr);
                    mapControl1.Map.Layers.Insert(0, lyr);

                    //��ӱ�ע
                    if (this.chkmh.Checked == false && fuzzyFlag && this.txtBoxCar.Text.Trim() != "�޺���")
                    {
                        string activeMapLabel = "KKSearchLabel";
                        MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                        MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                        MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                        lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                        lbsource.DefaultLabelProperties.Style.Font.Size = 20;
                        lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                        lbsource.DefaultLabelProperties.Layout.Offset = 2;
                        lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                        lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                        lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                        lbsource.DefaultLabelProperties.Caption = "Name";
                        lblayer.Sources.Append(lbsource);
                        mapControl1.Map.Layers.Add(lblayer);
                    }
                    
                }
                catch (Exception ex) { writeToLog(ex, "CreateKakouTrack-����ͼ�����"); }

                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;
                Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                this.SetLayerEdit("KKSearchLayer");   // ���ø�ͼ��ɱ༭

                double x = 0;
                double y = 0;
                double px = 0;
                double py = 0;

                //int i = 0;
                int ci = 0;

                InitUsedFlag();

                foreach (DataRow dr in dt.Rows)
                {
                    //if (i > 100)   // Ϊ���Ч�ʻ���100���󲻻���
                    //    return;

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
                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrKKTrack.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("STOP1-32.BMP", BitmapStyles.ApplyColor, colors[ci], 10));
                        Feature ftr = new Feature(tblKKTRA.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = tnum;         // ���к�
                        //ftr["ID"] = id;           // ͨ�г������
                        ftr["ID"] = tnum;           // �����к���ΪID  ��Ϊͨ�г�����Ż����ظ�
                        tblKKTRA.InsertFeature(ftr);

                        if (px != 0 && py != 0 && x != 0 && y != 0 && this.chkmh.Checked == false && fuzzyFlag && this.txtBoxCar.Text.Trim() != "�޺���")
                            Trackline(px, py, x, y);

                        px = x;
                        py = y;
                        //i = i + 1;
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
                writeToLog(ex, "CreateKakouTrack-14-�������ڲ�ѯͼ��"); 
            }
        }

        /// <summary>
        /// �޸�ͨ�����ڴ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dr">������</param>
        private void picturePoint(DataRow dr)
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrKKTrack = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;
                Table tblKKTRA = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                this.SetLayerEdit("KKSearchLayer");   // ���ø�ͼ��ɱ༭

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

                for (int k = 0; k < dataKaKou.Rows.Count; k++)
                {
                    if (KaName == dataKaKou.Rows[k]["��������"].ToString().Trim())
                    {
                        string tID = dataKaKou.Rows[k]["���к�"].ToString().Trim();
                        getFeatureCollection(tID, "KKSearchLayer");
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
                                f["Name"] = tnum;         // ���к�
                                // f["ID"] = id;          // ͨ�г������
                                f["ID"] = tnum;           // �����к���ΪID  ��Ϊͨ�г�����Ż����ظ�
                                tblKKTRA.UpdateFeature(f);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "picturePoint");
            }
        }

        string[] KKiD;
        Int32[] KKnum;
        string[] KKname;
        /// <summary>
        /// ��ȡ���п��ڱ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
                    KKname = new string[dt.Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string camid = dr["���ڱ��"].ToString();
                        string caName = dr["��������"].ToString();
                        KKiD[i] = camid ;
                        KKname[i] = caName;
                        KKnum[i] = 0;
                        i++;
                    }
                }            
            }
            catch (Exception ex)
            {
                writeToLog(ex, "15-InitUsedFlag");
            }
        }

        /// <summary>
        /// ���������ΰ�����ʱ�Ĺ켣��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
                MapInfo.Mapping.FeatureLayer workLayer =null;
                MapInfo.Data.Table tblTemp = null; 

                if (this.mapControl1.Map.Layers["KKSearchLayer"] != null)
                {
                    workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["KKSearchLayer"];
                    tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
                }
                else if (this.mapControl1.Map.Layers["KakouTrackLayer"] != null)
                {
                    workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["KakouTrackLayer"];
                    tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("KakouTrackLayer");
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
                writeToLog(ex, "Trackline-16-���������ΰ�����ʱ�Ĺ켣��");
            }
        }

        /// <summary>
        /// ����dataGridViewKakou��ɫ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dataGridViewKakou_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dataGridViewKakou.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dataGridViewKakou.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dataGridViewKakou.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dataGridViewKakou.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewKakou_DataBindingComplete-17-����dataGridViewKakou��ɫ");
            }
        }

        private IResultSetFeatureCollection rsfcflash = null;
        /// <summary>
        /// �����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dataGridViewKakou_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView portQueryView = (DataGridView)sender;

                this.dataGridViewKakou.Rows[e.RowIndex].Selected = true;

                int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // ���һ�е�λ��
                string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();
                string KaIDnu = portQueryView.CurrentRow.Cells[1].Value.ToString();
                string serNum = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // ���к�

                string tblname = "KKSearchLayer";

                //��ȡ��ǰѡ�����Ϣ��ͨ�г��������Ϊ����ֵ

                MapInfo.Mapping.Map map = this.mapControl1.Map;

                if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }

                DataRow[] row = dataKaKou.Select("���к�='" + serNum + "'");
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
                writeToLog(ex, "dataGridViewKakou_CellClick-18-��dataGridViewKakou�б��ϵ���");
            }
        }

        /// <summary>
        /// ɾ����ͼ�ϵĵ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        private void delPoint(double x,double y)
        {
            try
            {
                FeatureLayer fLayer = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;
                Table table = fLayer.Table;

                FeatureGeometry pt = new MapInfo.Geometry.Point(fLayer.CoordSys, new DPoint(x, y)) as FeatureGeometry;
                Feature fct = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("KKSearchLayer", MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(pt, ContainsType.Centroid));
         
                table.DeleteFeature(fct);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "delPoint");
            }
        }

        /// <summary>
        /// ��ȡ���ű���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <returns>������</returns>
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
                CLC.BugRelated.ExceptionWrite(ex, "getScale");
                return 0;
            }
        }

        private int iflash = 0;
        /// <summary>
        /// ͼԪ��˸
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
                this.timerFlash.Enabled = false;
                writeToLog(ex, "timerFlash_Tick-19-ͼԪ��˸");
            }
        }

        /// <summary>
        /// ˫���¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dataGridViewKakou_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //�����ͷ,�˳�
            try
            {
                DataGridView portQueryView = (DataGridView)sender;

                string[] sqlFields = {"ͨ�г������","���ڱ��","��������","ͨ��ʱ��","��������","��������","������ɫ","��ɫ��ǳ","��ʻ����","��Ƭ1","��Ƭ2","��Ƭ3"};
                DataTable datatable = new DataTable("TemData");
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
                    datatable.Columns.Add(dc);
                }

                DataRow dr = datatable.NewRow();
                for (int i = 0; i < sqlFields.Length; i++)
                {
                    dr[i] = portQueryView.CurrentRow.Cells[sqlFields[i]].Value.ToString();
                }
                datatable.Rows.Add(dr);

                /////////���ݵ�ǰͨ�г�������ж�ͼƬ��������ַ
                try
                {
                    if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "01")   //�ߵ�����Ƶ��ͼƬ��������ַ
                    {
                        photoserver = gdwserver;
                    }
                    else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "02") //�׻�¼��Ƶ��ͼƬ��������ַ
                    {
                        photoserver = ehlserver;
                    }
                    else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "03")  //������Ƶ��ͼƬ��������ַ
                    {
                        photoserver = bkserver;
                    }
                    else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().IndexOf("lbschina") > -1)
                    {
                        photoserver = "http://192.168.0.50/";
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "clKaKou-ucKakou-20-����ͨ����Ϣ����ip��ַ��ȡͼƬ��������ַ");
                }

                DPoint dp = new DPoint();

                if (datatable.Rows.Count > 0)
                {                    
                    //////////////////////////////////////////
                    string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                    int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // ���һ�е�λ��
                    string serNum = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // ���к�

                    string tblname = "KKSearchLayer";

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        MessageBox.Show("�˶���δ��λ,�޷����в���!", "��ʾ");
                        return;
                    }                   
                 
                    try
                    {
                        getFeatureCollection(serNum, tblname);
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }       
                            this.timerFlash.Enabled = true;
                        }
                    }
                    catch (Exception ex) { writeToLog(ex, "clKaKou-ucKakou-20-�ڵ�ͼ��������Ӧ��Ϣʱ��������"); }

                    /////////////////////////////////////////                    

                    System.Drawing.Point pt = new System.Drawing.Point();
                    if (dp.x == 0 || dp.y == 0)
                    {
                        MessageBox.Show("�˶���δ��λ,�޷����в���!", "��ʾ");
                        return;
                    }
                    mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                    WriteEditLog("�ն˳�������='" + portQueryView.CurrentRow.Cells[0].Value.ToString() + "'", "�鿴����", "V_CROSS");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewKakou_CellDoubleClick-20-���ݱ�dataGridViewKakou˫��");
            }
            finally
            {
                try
                {
                    fmDis.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// ��ȡ��ͼ��Ҫ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="fearID">ͼԪ��Ϣ</param>
        /// <param name="tblname">����</param>
        private void getFeatureCollection(string fearID,string tblname)
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
                cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where ID = @name ";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@name", fearID);

                this.rsfcflash = cmd.ExecuteFeatureCollection();

                cmd.Dispose();
            }
            catch (Exception ex) { writeToLog(ex, "getFeatureCollection"); }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

            }
        }

        /// <summary>
        /// ��ʾ������ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private FrmInfo frminfo = new FrmInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (this.frminfo.Visible == false)
                {
                    this.frminfo = new FrmInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.Visible = false;
                }
                frminfo.photoserver = photoserver;
                frminfo.mapControl = this.mapControl1;
                frminfo.layerName = "KKSearchLayer";
                frminfo.getFromNamePath = this.getfrompath;
                frminfo.setInfo(dt.Rows[0], pt, StrCon,user);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " disPlayInfo-21-��ʾ������ϸ��Ϣ");
            }
        }

        /// <summary>
        /// �л�Ҫ���в�ѯ�ı�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void comboxTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //ͨ�����ƻ�ȡ��ĸ����ֶ����ƣ�����ӵ��ֶ�dropdownlist
                if (this.comboxTable.Text == "����ͨ����Ϣ")
                {
                    this.comboxField.Items.Clear();
                    this.comboxField.Items.Add("���ڱ��");
                    this.comboxField.Items.Add("��������");
                    this.comboxField.Items.Add("ͨ��ʱ��");
                    this.comboxField.Items.Add("��������");
                    this.comboxField.Items.Add("��������");
                    this.comboxField.Items.Add("������ɫ");
                    this.comboxField.Items.Add("��ɫ��ǳ");
                    this.comboxField.Items.Add("���ڷ���");
                }
                else if (this.comboxTable.Text == "����������Ϣ")
                {
                    this.comboxField.Items.Clear();
                    this.comboxField.Items.Add("��������");
                    this.comboxField.Items.Add("�������ڱ��");
                    this.comboxField.Items.Add("������������");
                    this.comboxField.Items.Add("����ʱ��");
                    this.comboxField.Items.Add("��������");
                    this.comboxField.Items.Add("��������");
                    this.comboxField.Items.Add("�����������");
                    this.comboxField.Items.Add("���ص�λ");
                    this.comboxField.Items.Add("������");
                }
                else if (this.comboxTable.Text == "�ΰ�������Ϣ")
                {
                    this.comboxField.Items.Clear();
                    this.comboxField.Items.Add("��������");
                    this.comboxField.Items.Add("���ڱ��");
                    this.comboxField.Items.Add("��װ�ص�");
                    this.comboxField.Items.Add("��ص��������");
                    this.comboxField.Items.Add("�����ɳ���");
                    this.comboxField.Items.Add("��ط���");
                    this.comboxField.Items.Add("��ϵ��");
                    this.comboxField.Items.Add("��Դ");
                }


                this.comboxField.Text = this.comboxField.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "comboxTable_SelectedIndexChanged-22-�л�Ҫ���в�ѯ�ı�");                
            }
        }

        /// <summary>
        /// �����ֶ����͸ı������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void comboxField_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.comboxField.Text == "ͨ��ʱ��" || this.comboxField.Text == "����ʱ��")
                {
                    this.comboxCon.Items.Clear();
                    this.comboxCon.Items.Add("����");
                    this.comboxCon.Items.Add("����");
                    this.comboxCon.Items.Add("С��");

                    dateTimePicker2.Visible = true;
                    this.dateTimePicker2.Text = System.DateTime.Now.ToString();
                    this.CaseSearchBox.Visible = false;
                }
                else
                {
                    this.comboxCon.Items.Clear();
                    this.comboxCon.Items.Add("����");
                    this.comboxCon.Items.Add("������");
                    this.comboxCon.Items.Add("����");

                    dateTimePicker2.Visible = false;
                    this.CaseSearchBox.Visible = true;
                }
                this.comboxCon.Text = this.comboxCon.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "comboxField_SelectedIndexChanged-23-�����ֶ����͸ı������");
            }
        }


        /// <summary>
        /// ��Ӳ�ѯ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboxTable.Text.Trim() == "")
                {
                    MessageBox.Show("��ѡ���");
                    return;
                }

                if (this.CaseSearchBox.Visible && CaseSearchBox.Text.Trim() == "")
                {
                    MessageBox.Show("��ѯֵ����Ϊ�գ�");
                    return;
                }

                if (this.CaseSearchBox.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("������ַ����в��ܰ��������ַ�!");
                    return;
                }

                string strExp = "";
               
                 if(this.comboxField.Text =="double")
                 {
                     if (this.dataGridViewValue.Rows.Count == 0)
                        {

                            strExp = this.comboxField.Text + " " + this.comboxCon.Text + " " + CaseSearchBox.Text.Trim();
                        }
                        else
                        {
                            if (this.comboxORA.Text == "")
                            {
                                MessageBox.Show("�������ӷ�ʽ����,������������룡", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            strExp = this.comboxORA.Text + " " + this.comboxField.Text + " " + this.comboxCon.Text + " " + CaseSearchBox.Text.Trim();
                        }
                        this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                 }

                 else if(this.comboxField.Text == "ͨ��ʱ��"||this.comboxField.Text =="����ʱ��")
                 {
                        string tValue = this.dateTimePicker2.Value.ToString();
                        if (tValue == "")
                        {
                            MessageBox.Show("��ѯֵ����Ϊ�գ�");
                            return;
                        }

                        if (this.dataGridViewValue.Rows.Count == 0)
                        {                           
                            strExp = this.comboxField.Text + " " + this.comboxCon.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "ʱ��" });
                        }
                        else
                        {

                            if (this.comboxORA.Text == "")
                            {
                                MessageBox.Show("�������ӷ�ʽ����,������������룡", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            strExp = this.comboxORA.Text + " " + this.comboxField.Text + " " + this.comboxCon.Text + " '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "ʱ��" });
                        }                        
                 }
                 else 
                 {
                     if (this.dataGridViewValue.Rows.Count == 0)
                        {

                            strExp = this.comboxField.Text + " " + this.comboxCon.Text + " '" + this.CaseSearchBox.Text.Trim() + "'";
                            if (this.comboxCon.Text.Trim() == "����")
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
                            if (this.comboxORA.Text == "")
                            {
                                MessageBox.Show("�������ӷ�ʽ����,������������룡","����",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                                return;
                            }

                            strExp = this.comboxORA.Text + " " + this.comboxField.Text + " " + this.comboxCon.Text + " '" + this.CaseSearchBox.Text.Trim() + "'";
                            if (this.comboxCon.Text.Trim() == "����")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "�ַ���" });
                            }
                        }
                }
                this.comboxTable.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button5_Click-24-��Ӳ�ѯ����");
            }
        }

        /// <summary>
        /// �Ƴ�һ�����ʽ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
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
                    string text = this.dataGridViewValue.Rows[0].Cells["Value"].Value.ToString().Replace("����", "");

                    text = text.Replace("����", "").Trim();
                    this.dataGridViewValue.Rows[0].Cells["Value"].Value = text;
                }

                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    this.comboxTable.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button4_Click-25-�Ƴ�һ�����ʽ");
            }
        }


       

        /// <summary>
        /// ��ϲ�ѯ 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    MessageBox.Show("����Ӳ�ѯ����!");
                    return;
                }

                if (getSqlString() == "")
                {
                    MessageBox.Show("��ѯ�����д���,������!");
                    return;
                }
                dataGridView5.Rows.Clear();

                this.Cursor = Cursors.WaitCursor;



                this.st.Text = "���ڲ�ѯ......";
                Application.DoEvents();

                ClearKaKouTemp();

                string fieldtext = string.Empty;
                string NewSql = string.Empty;
                DataTable dt = new DataTable();

                Application.DoEvents();

                if (this.comboxTable.Text == "����ͨ����Ϣ")  //V_Cross
                {
                    dt = this.GetPassCar(this.getSqlString());

                    Application.DoEvents();

                    Pagedt2 = dt;

                    InitDataSet2(this.RCount2, this.PageNow2, this.PageNum2, bindingSource2, bindingNavigator2, dgvres);
                    WriteEditLog(NewSql, "��ϲ�ѯ", "V_CROSS");

                    MessageBox.Show(messageStr, "��ѯ�����ʾ", MessageBoxButtons.OK, MessageBoxIcon.None);

                }
                else if (this.comboxTable.Text == "����������Ϣ")// V_Alarm
                {

                    try
                    {
                        fieldtext = " select �������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3 from V_ALARM where ";
                        NewSql = fieldtext + this.getSqlString() + " order by ����ʱ�� desc";
                        SQLSearch = NewSql;


                        WriteEditLog(NewSql, "��ϲ�ѯ", "V_ALARM");
                        dt = GetTable(NewSql);
                        Application.DoEvents();
                        this.st.Text = "���ڲ�ѯ����������Ϣ......";
                        if (dt.Rows.Count > 0)
                        {
                            Pagedt2 = dt;
                            InitDataSet2(this.RCount2, this.PageNow2, this.PageNum2, bindingSource2, bindingNavigator2, dgvres);
                        }

                        Application.DoEvents();
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            this._exportDT = dt;
                            if (dt.Rows.Count > 0)
                            {
                                //this.dgvres.DataSource 
                                DrawPoints(dt);   //�ڵ�ͼ�ϻ���
                            }
                        }
                        else
                        {
                            MessageBox.Show("��ѯ���Ϊ0", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                    }
                    catch(Exception ex)
                    {
                        writeToLog(ex, "button2_Click-26-����������Ϣ");
                    }
                }
                else if (this.comboxTable.Text == "�ΰ�������Ϣ")// V_Alarm
                {
                    fieldtext = " select ���ڱ��,��������,��װ�ص�,��ص��������,���ڶ�Ӧ������,��س�����,�����ɳ���,��ط���,��ϵ��,��ϵ��ʽ,��Դ as ������Դ from �ΰ�����ϵͳ where ";
                    NewSql = fieldtext + this.getSqlString() + " order by ���ڱ�� ";
                    SQLSearch = NewSql;
                    dt = GetTable(NewSql);
                    Application.DoEvents();

                    this.st.Text = "���ڲ�ѯ�ΰ�������Ϣ......";

                    if (dt.Rows.Count > 0)
                    {
                        Pagedt2 = dt;
                        InitDataSet2(this.RCount2, this.PageNow2, this.PageNum2, bindingSource2, bindingNavigator2, dgvres);
                    }
                    WriteEditLog(NewSql, "��ϲ�ѯ", "�ΰ�����ϵͳ");

                    Application.DoEvents();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        this._exportDT = dt;
                        if (dt.Rows.Count > 0)
                        {
                            //this.dgvres.DataSource 
                            DrawPoints(dt);   //�ڵ�ͼ�ϻ���
                        }
                    }
                    else
                    {
                        MessageBox.Show("��ѯ���Ϊ0", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                this.st.Text = "��ѯ���......";
                

                Application.DoEvents();

                if (dtExcel != null) dtExcel.Clear();
                this.dtExcel = dt;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button2_Click-26-��ϲ�ѯ");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }



        private string messageStr = "";  // ������ʾ�ⲿ�����ṩ�̵Ĳ�ѯ������쳣��Ϣ

        /// <summary>
        /// ��ѯͨ��������Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">sql���</param>
        /// <returns>��ѯ���</returns>
        public DataTable GetPassCar(string sql)
        {

            FrmProgress frmpro = new FrmProgress();
            frmpro.progressBar1.Maximum = 6;
            frmpro.Show();
            frmpro.progressBar1.Value = 0;
            //frmpro.TopMost = true;

           string  fieldtext = " select ͨ�г������,���ڱ��,��������,ͨ��ʱ��,��������,��������,������ɫ,��ɫ��ǳ,��ʻ����,��Ƭ1,��Ƭ2,��Ƭ3 from V_Cross where ";
           string NewSql = fieldtext + sql;
           string   SQLSearch = NewSql;

            Writelog(NewSql);

            DataTable dt = new DataTable();

            frmpro.progressBar1.Value = 1;

            Application.DoEvents();

            messageStr = "";

            try
            {
                string sqlstr = NewSql;

                dt = ehltgs.GetPassCarInfo(sqlstr, ref ehlserver);

                //this.st.Text = "���ڲ�ѯ�׻�¼����......";
                frmpro.label1.Text = "���ڲ�ѯ�׻�¼����......";
                if (dt == null)
                {
                    Writelog("���׻�¼������ͨѶ��������,�޷���ѯ�����ݡ�");
                    messageStr += "�׻�¼������ͨѶ��������,�޷���ѯ������\t\n";
                }
                else
                {
                    messageStr += "�׻�¼��������ѯ��" + dt.Rows.Count + "������\t\n";
                }
                frmpro.progressBar1.Value = 2;

                Application.DoEvents();

                frmpro.label1.Text = "���ڲ�ѯ�ߵ�������......";
                DataTable dt2 = gdwcom.QueryData(sqlstr, ref gdwserver);

                if (dt2 == null)
                {
                    Writelog("��ߵ���������ͨѶ��������,�޷���ѯ�����ݡ�");
                    messageStr += "�ߵ���������ͨѶ��������,�޷���ѯ������\t\n";
                }
                else
                {
                    messageStr += "�ߵ�����������ѯ��" + dt2.Rows.Count + "������\t\n";
                }
                frmpro.progressBar1.Value = 3;

                Application.DoEvents();
                frmpro.label1.Text = "���ڲ�ѯ��������......";
                PassDataQuery psqy = new PassDataQuery();
                DataTable dt3 = psqy.QueryPassData(sqlstr, ref bkserver);    //������ѯ�ӿ�   

                if (dt3 == null)
                {
                    Writelog("�뱦��������ͨѶ��������,�޷���ѯ�����ݡ�");
                    messageStr += "����������ͨѶ��������,�޷���ѯ������\t\n";
                }
                else
                {
                    messageStr += "������������ѯ��" + dt3.Rows.Count + "������\t\n";
                }
                frmpro.progressBar1.Value = 4;
                Application.DoEvents();

                #region ����������ѯ
                try
                {
                    if (dt != null && dt2 != null && dt3 != null)   //�׻�¼+�ߵ��� + ����
                    {
                        dt2.Merge(dt3, false);
                        dt.Merge(dt2, false);
                    }
                    else if (dt == null && dt2 != null && dt3 != null)   //�ߵ��� + ����
                    {
                        dt2.Merge(dt3, false);
                        dt = dt2;
                    }
                    else if (dt != null && dt2 == null && dt3 != null)   //�׻�¼ + ����
                    {
                        dt.Merge(dt3, false);
                    }
                    else if (dt != null && dt2 != null && dt3 == null)    // �ߵ���+�׻�¼
                    {
                        dt.Merge(dt2, false);
                    }
                    else if (dt != null && dt2 == null && dt3 == null)  // �׻�¼
                    {
                    }
                    else if (dt == null && dt2 != null && dt3 == null)  // �ߵ���
                    {
                        dt = dt2;
                    }
                    else if (dt == null && dt2 == null && dt3 != null)  // ����
                    {
                        dt = dt3;
                    }
                    else if (dt == null && dt2 == null && dt3 == null)  // ȫ��
                    {
                        isShowPro(false);
                        MessageBox.Show("���ΰ����ڽӿڵķ�����ͨѶ��������,�޷���ѯ�����ݡ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    MessageBox.Show("�ӶԷ���������ȡ����ʱ��������");
                    writeToLog(ex, "clKaKou-ucKakou-26-�ӶԷ���������ȡ����ʱ��������");
                }
                #endregion
                frmpro.progressBar1.Value = 5;
                Application.DoEvents();
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (NewSql.IndexOf("�޺���") < 0)
                        dt = RefreshData(dt, "2");

                    dt = InsertColumns(dt); //������к�

                    this._exportDT = dt;
                }

                frmpro.progressBar1.Value = 6;
                frmpro.Close();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GetPassCar");
            }

            return dt;
        }

        /// <summary>
        /// ����ѯ�����ʾ�ڵ�ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="datatable">����Դ</param>
        /// <param name="tableName">����</param>
        private void DrawPoints(DataTable dt)
        {
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (mapControl1.Map.Layers["KKSearchLayer"] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("KKSearchLayer");
                    }

                    if (mapControl1.Map.Layers["KKSearchLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("KKSearchLabel");
                    }                  

                    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                    //������ʱ��
                    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("KKSearchLayer");
                    Table tblTemp = Cat.GetTable("KKSearchLayer");
                    if (tblTemp != null) //Table exists close it
                    {
                        Cat.CloseTable("KKSearchLayer");
                    }

                    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("ID", 50));

                    tblTemp = Cat.CreateTable(tblInfoTemp);
                    FeatureLayer lyr = new FeatureLayer(tblTemp);
                    mapControl1.Map.Layers.Insert(0, lyr);

                    //��ӱ�ע
                    string activeMapLabel = "KKSearchLabel";
                    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");
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

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");


                    double Cx = 0;
                    double Cy = 0;

                    double px = 0;
                    double py = 0;

                    int i = 0;

                    foreach (DataRow dr in dt.Rows)
                    {

                        i++;
                        if (i > 50) return;

                        string kkname = string.Empty;
                        string CarName = string.Empty;
                        string idname = string.Empty;

                        if (this.comboxTable.Text == "����ͨ����Ϣ")
                        {
                            idname = dr["ͨ�г������"].ToString();
                            kkname = dr["���ڱ��"].ToString();
                            CarName = dr["��������"].ToString();
                        }
                        else if (this.comboxTable.Text == "����������Ϣ")
                        {
                            idname = dr["�������"].ToString();
                            kkname = dr["�������ڱ��"].ToString();
                            CarName = dr["��������"].ToString();
                        }
                        else if (this.comboxTable.Text == "�ΰ�������Ϣ")
                        {
                            idname = dr["���ڱ��"].ToString();
                            CarName = dr["��������"].ToString();
                            kkname = dr["���ڱ��"].ToString();
                        }


                        //////��ȡ

                       DataTable dt1 = GetTable("select X,Y from �ΰ�����ϵͳ where ���ڱ��='" + kkname + "'");

                        if (dt1.Rows.Count > 0)
                        {
                            foreach (DataRow dr1 in dt1.Rows)
                            {
                                Cx = Convert.ToDouble(dr1["X"]);
                                Cy = Convert.ToDouble(dr1["Y"]);
                            }
                        }

                        i = i + 1;

                        if (Cx != 0 && Cy != 0)
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Cx, Cy)) as FeatureGeometry;
                            CompositeStyle cs = new CompositeStyle();
                            cs.ApplyStyle(new BitmapPointStyle("PIN1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 24));

                            Feature ftr = new Feature(tblcar.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = CarName;
                            ftr["ID"] = idname;
                            tblcar.InsertFeature(ftr);

                            if (this.comboxTable.Text == "����ͨ����Ϣ")
                            {
                                if (px != 0 && py != 0 && Cx != 0 && Cy != 0 && CaseSearchBox.Text.Trim() != "�޺���" && comboxField.Text == "��������" && comboxCon.Text == "����")
                                    Trackline(px, py, Cx, Cy);

                                px = Cx;
                                py = Cy;
                            }
                        }
                    }                   
                }              
            }
            catch (Exception ex)
            {
                writeToLog(ex, "DrawPoints-27-����ѯ�����ʾ�ڵ�ͼ��");
            }
        }        

        /// <summary>
        /// ת��SQL��ѯ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <returns>�ɹ����ݿ��ѯ��SQL</returns>
        private string getSqlString()//ת���ַ���
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = string.Empty;
                fuzzyFlag = false;

                for (int i = 0; i < this.dataGridViewValue.Rows.Count; i++)
                {
                    string type = this.dataGridViewValue.Rows[i].Cells[1].Value.ToString();
                    string str = this.dataGridViewValue.Rows[i].Cells[0].Value.ToString();

                    //if (comboxTable.Text == "����ͨ����Ϣ")    // ͨ��������Ϣ�Ŀ�������Ҫת��Ϊ���ڱ�ź��ѯ  add by lili 2010-12-16
                    //    str = transSerial(str); 

                    if (str.IndexOf("�������� ����") > -1)
                        fuzzyFlag = true;

                    if (str.IndexOf("��������") > -1 && str.IndexOf("�޺���") > -1)
                    {
                        if (i > 0)
                        {
                            str = str.Substring(0, str.IndexOf("��������")) + " (��������='�޺���' or �������� is null)";
                        }
                    }


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
                                //strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   to_date('" + tValue + "', 'YYYY-MM-DD HH24:MI:SS')";
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
                writeToLog(ex, "getSqlString-28-ת��SQL��ѯ���");
                return "";
            }
        }


        /// <summary>
        /// �˷���ֻ��getSqlString����ʹ�ã�����ת���ΰ���������ת��Ϊ���ڱ��    
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="tansSer">Ҫת����sql</param>
        /// <returns>���ڱ����Ϊ����</returns>
        private string transSerial(string tansSer)
        {
            try
            {
                string newSql = "";

                if (tansSer.IndexOf("�������� ����") > -1)
                {
                    string serails = tansSer.Substring(tansSer.IndexOf("'"));

                    DataTable dt = GetTable("select ���ڱ�� from �ΰ�����ϵͳ where ��������=" + serails);

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
                writeToLog(ex, "transSerial");
                return tansSer;
            }
        }

        /// <summary>
        /// ���ò�ѯ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.dataGridViewValue.Rows.Clear();
                //this.textValue.Text = "";
                this.CaseSearchBox.Text = "";
                this.comboxTable.Enabled = true;
                this.dataKaKou = null;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "button3_Click-29-���ò�ѯ����");
            }
        }


        //private int alarmcount = 0;
        ///// <summary>
        ///// ��ѯ������Ϣ����ʾ
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        ///// 
        //private void timalarm_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //            OracleConnection con = new OracleConnection(mysqlstr);
        //            if (con.State == ConnectionState.Open) { con.Close(); }
        //            con.Open();
                    
        //            string sqlstring = string.Empty ;
        //            if(this.ALARMUSER =="����" || this.strRegion =="˳����")
        //            {
        //                sqlstring = "Select �������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3,����״̬,������,����ʱ��,������� from V_ALARM where ����״̬ is null order by ����ʱ��";
        //            }
        //            else if(this.ALARMUSER =="�û�")
        //            {
        //                sqlstring = "Select �������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3,����״̬,������,����ʱ��,������� from V_ALARM where ����״̬ is null' and  ���ص�λ in ('" + this.strRegion.Replace(",", "','") + "') order by ����ʱ��";
        //            }
                    
        //            OracleCommand cmd = new OracleCommand(sqlstring,con);
        //            cmd.ExecuteNonQuery();
        //            OracleDataAdapter apt = new OracleDataAdapter(cmd);
        //            DataTable dt = new DataTable();
        //            apt.Fill(dt);
        //            if (dt.Rows.Count > 0)
        //            {
        //                ftip.renum = dt.Rows.Count;
        //                ftip.label2.Text = "���� " + dt.Rows.Count.ToString() + " �����ڱ�����¼";
        //                if (ftip.Visible == false)
        //                {
        //                    ftip.Visible = true;
        //                }
        //                ftip.TopMost = true;
        //            }
        //            alarmcount = dt.Rows.Count;
        //    }
        //    catch (Exception ex)
        //    {
        //        writeToLog(ex, "��ѯ������Ϣ����ʾ");
        //    }
        //    finally { }
        //}

        /// <summary>
        /// �ı��б���ɫ 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dgvres_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dgvres.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dgvres.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dgvres.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dgvres.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvres_DataBindingComplete-30-����dgvresDataGrid��ɫ");
            }
        }

        AreaStyle aStyle;
        BaseLineStyle lStyle;
        TextStyle tStyle;
        BasePointStyle pStyle;

        /// <summary>
        /// ���û���Ҫ�ص�Ĭ����ʽ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        public void SetDrawStyle()
        {
            try
            {
                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2.5, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(1);
                aStyle = new AreaStyle(simLineStyle, simInterior);
                lStyle = new SimpleLineStyle(new LineWidth(2.5, MapInfo.Styles.LineWidthUnit.Point), 59, System.Drawing.Color.Red);
                tStyle = new TextStyle(new MapInfo.Styles.Font("����", 16.0, Color.Red, Color.Transparent, FontFaceStyle.Normal, FontWeight.Bold, TextEffect.None, TextDecoration.None, TextCase.Default, false, false));
                //pStyle = new SimpleVectorPointStyle(69, Color.Blue, 12);
                pStyle = new BitmapPointStyle("ren2.bmp", BitmapStyles.None, Color.Blue, 14);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetDrawStyle-31-���û���Ҫ�ص�Ĭ����ʽ");
            }
        }

        private bool isDel = false;
        /// <summary>
        /// ѡ�񹤾�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void mapToolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                UncheckedTool();
                isDel = false;
                switch (e.Button.Name)
                {
                    case "toolAddPoint":
                        mapControl1.Tools.LeftButtonTool = "AddPoint";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddPolyline":
                        mapControl1.Tools.LeftButtonTool = "AddPolyline";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddPolygon":
                        mapControl1.Tools.LeftButtonTool = "AddPolygon";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddText":
                        mapControl1.Tools.LeftButtonTool = "AddText";
                        e.Button.Pushed = true;
                        break;
                    case "toolPointStyle":
                        SymbolStyleDlg pStyleDlg = new SymbolStyleDlg();
                        pStyleDlg.SymbolStyle = pStyle;
                        if (pStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            pStyle = pStyleDlg.SymbolStyle;
                        }
                        break;
                    case "toolLineStyle":
                        LineStyleDlg lStyleDlg = new LineStyleDlg();
                        lStyleDlg.LineStyle = lStyle;
                        if (lStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            lStyle = lStyleDlg.LineStyle;
                        }
                        break;
                    case "toolAreaStyle":
                        AreaStyleDlg aStyleDlg = new AreaStyleDlg();
                        aStyleDlg.AreaStyle = aStyle;
                        if (aStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            aStyle = aStyleDlg.AreaStyle;
                        }
                        break;
                    case "toolTextStyle":
                        TextStyleDlg tStylrDlg = new TextStyleDlg();
                        tStylrDlg.FontStyle = tStyle.Font;
                        if (tStylrDlg.ShowDialog() == DialogResult.OK)
                        {
                            tStyle = new TextStyle(tStylrDlg.FontStyle);
                        }
                        break;
                    case "toolButtonDel":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        e.Button.Pushed = true;
                        isDel = true;
                        break;
                    case "toolBarRect":
                        mapControl1.Tools.LeftButtonTool = "SelByRect";
                        e.Button.Pushed = true;
                        break;
                    case "toolBarCircle":
                        mapControl1.Tools.LeftButtonTool = "SelByCircle";
                        e.Button.Pushed = true;
                        break;
                    case "toolBarPolygon":
                        mapControl1.Tools.LeftButtonTool = "SelByPolygon";
                        e.Button.Pushed = true;
                        break;
                }
                mapControl1.Map.Center = mapControl1.Map.Center;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar1_ButtonClick-32-ѡ�񹤾�");
            }
        }

        /// <summary>
        /// ���������ʾ״̬
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void setSelectableLayers()
        {
            this.Cursor = Cursors.WaitCursor;

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// ����������ϵĹ���ʱ���Թ��߰�ť�������ã�
        /// ���ڰ�ť���飬iFrom��ʾ�����Index��iEnd��ʾĩIndex
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void UncheckedTool()
        {
            try
            {

                for (int i = 0; i < this.mapToolBar1.Buttons.Count; i++)
                {
                    if (mapToolBar1.Buttons[i].Pushed)
                    {
                        mapToolBar1.Buttons[i].Pushed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "33-UncheckedTool");
            }
        }

        /// <summary>
        /// ���ͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        void Tools_FeatureAdded(object sender, MapInfo.Tools.FeatureAddedEventArgs e)
        {

            if (this.Visible)
            {
                try
                {
                    Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("KKLayer", MapInfo.Data.SearchInfoFactory.SearchWhere("ID is null or ID=''"));
                    switch (e.Feature.Type)
                    {
                        case MapInfo.Geometry.GeometryType.Point:
                            ft.Style = pStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.MultiCurve:
                            ft.Style = lStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.MultiPolygon:
                            ft.Style = aStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.LegacyText:
                            ft.Style = tStyle;
                            break;
                    }
                    ft["ID"] = "t1";
                    ft.Update();
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "34-Tools_FeatureAdded");
                }
            }
        }

        /// <summary>
        /// ѡ��ͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void Tools_FeatureSelected(object sender, MapInfo.Tools.FeatureSelectedEventArgs e)
        {
            if (isDel == false) return;
            if (this.Visible)
            {
                try
                {
                    FeatureLayer fLayer = mapControl1.Map.Layers["KKLayer"] as FeatureLayer;
                    Table table = fLayer.Table;

                    //IResultSetFeatureCollection fc= MapInfo.Engine.Session.Current.Catalog.Search(table.Alias, MapInfo.Data.SearchInfoFactory.(e.Selection.Envelope,mapControl1.Map.FeatureCoordSys, ContainsType.Geometry));
                    IResultSetFeatureCollection fc = e.Selection[table];
                    foreach (Feature f in fc)
                    {
                        if (f["ID"].ToString() == "t1")
                        { table.DeleteFeature(f); }
                    }
                }
                catch(Exception ex)
                {
                    writeToLog(ex, "35-Tools_FeatureSelected");
                }
            }
        }


        private MapInfo.Geometry.DPoint dptStart;
        private MapInfo.Geometry.DPoint dptEnd;

        private System.Collections.ArrayList arrlstPoints = new ArrayList();

        /// <summary>
        /// ʹ�ù���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            try
            {
                if (this.Visible == true)
                {
                    switch (e.ToolName)
                    {
                        case "SelByRect":
                            //add by siumo 20080923
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    dptStart = e.MapCoordinate;
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    dptEnd = e.MapCoordinate;
                                    if (dptStart == dptEnd) return;
                                    MapInfo.Geometry.DRect MapRect = new MapInfo.Geometry.DRect();

                                    MapRect.x1 = dptStart.x;
                                    MapRect.y2 = dptStart.y;
                                    MapRect.x2 = dptEnd.x;
                                    MapRect.y1 = dptEnd.y;
                                    clearFeatures("�鿴ѡ��");
                                    MapInfo.Geometry.Rectangle rect = new MapInfo.Geometry.Rectangle(mapControl1.Map.GetDisplayCoordSys(), MapRect);
                                    selectAndInsertByGeometry((FeatureGeometry)rect);
                                    WriteEditLog("��ѡ��ʾ��ӦͼԪ", "��ѡ", "ָ�Ӱװ�");
                                    break;
                            }
                            break;
                        case "SelByCircle":
                            //add by siumo 20080923
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    dptStart = e.MapCoordinate;
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    dptEnd = e.MapCoordinate;
                                    double radius = Math.Sqrt((dptEnd.y - dptStart.y) * (dptEnd.y - dptStart.y) + (dptEnd.x - dptStart.x) * (dptEnd.x - dptStart.x));

                                    Ellipse circle = new Ellipse(mapControl1.Map.GetDisplayCoordSys(), dptStart, radius, radius, DistanceUnit.Degree, DistanceType.Spherical);
                                    clearFeatures("�鿴ѡ��");
                                    selectAndInsertByGeometry((FeatureGeometry)circle);
                                    WriteEditLog("Ȧѡ��ʾ��ӦͼԪ", "Ȧѡ", "ָ�Ӱװ�");
                                    break;
                            }
                            break;
                        case "SelByPolygon":
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    arrlstPoints.Clear();
                                    dptStart = e.MapCoordinate;
                                    arrlstPoints.Add(e.MapCoordinate);
                                    break;
                                case MapInfo.Tools.ToolStatus.InProgress:
                                    arrlstPoints.Add(e.MapCoordinate);
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    //����һ���պϻ�
                                    //arrlstPoints.Add(e.MapCoordinate);
                                    arrlstPoints.Add(dptStart);
                                    int intCount = arrlstPoints.Count;
                                    if (intCount <= 3)
                                    {
                                        MessageBox.Show("�뻭3�����ϵĵ��γ�������������Ҫ�����");
                                        return;
                                    }
                                    MapInfo.Geometry.DPoint[] dptPoints = new DPoint[intCount];
                                    for (int i = 0; i < intCount; i++)
                                    {
                                        dptPoints[i] = (MapInfo.Geometry.DPoint)arrlstPoints[i];
                                    }
                                    //dptPoints[intCount] = dptFirstPoint;

                                    //�ñպϵĻ�����һ����		
                                    MapInfo.Geometry.AreaUnit costAreaUnit;
                                    costAreaUnit = MapInfo.Geometry.CoordSys.GetAreaUnitCounterpart(DistanceUnit.Kilometer);
                                    MapInfo.Geometry.CoordSys objCoordSys = this.mapControl1.Map.GetDisplayCoordSys();
                                    MultiPolygon objPolygon = new MultiPolygon(objCoordSys, CurveSegmentType.Linear, dptPoints);
                                    if (objPolygon == null)
                                    {
                                        return;
                                    }
                                    clearFeatures("�鿴ѡ��");
                                    selectAndInsertByGeometry((FeatureGeometry)objPolygon);
                                    WriteEditLog("�����ѡ����ʾ��ӦͼԪ", "�����", "ָ�Ӱװ�");
                                    break;
                                default:
                                    dptStart = e.MapCoordinate;
                                    break;
                            }
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_Used-36-ʹ�ù���");
            }
        }    

        private int i = 0; // �洢�û���ѡ��Χ�ڵĶ����� 
        /// <summary>
        /// �жϲ鿴ѡ���ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="geo">ͼԪ</param>
        private void selectAndInsertByGeometry(FeatureGeometry geo)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                i = 0;
                if (checkBoxChangsuo.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��������") == null)
                    {
                        openTable("��������");
                    }
                    SpatialSearchAndView(geo, "��������");
                }
                if (checkBoxAnfang.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��ȫ������λ") == null)
                    {
                        openTable("��ȫ������λ");
                    }
                    SpatialSearchAndView(geo, "��ȫ������λ");
                }
                if (checkBoxWangba.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����") == null)
                    {
                        openTable("����");
                    }
                    SpatialSearchAndView(geo, "����");
                }
                if (checkBoxZhikou.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�ΰ�����") == null)
                    {
                        openTable("�ΰ�����");
                    }
                    SpatialSearchAndView(geo, "�ΰ�����");
                }
                if (checkBoxTezhong.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("������ҵ") == null)
                    {
                        openTable("������ҵ");
                    }
                    SpatialSearchAndView(geo, "������ҵ");
                }
                if (checkBoxShiping.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��Ƶ") == null)
                    {
                        openTable("��Ƶ");
                    }
                    SpatialSearchAndView(geo, "��Ƶ");
                }
                if (checkBoxXiaofangshuan.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����˨") == null)
                    {
                        openTable("����˨");
                    }
                    SpatialSearchAndView(geo, "����˨");
                }
                if (checkBoxXiaofangdangwei.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�����ص㵥λ") == null)
                    {
                        openTable("�����ص㵥λ");
                    }
                    SpatialSearchAndView(geo, "�����ص㵥λ");
                }
                if (checkBoxJingche.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����") == null)
                    {
                        openTable("����");
                    }
                    SpatialSearchAndView(geo, "����");
                }
                if (checkBoxJingyuan.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��Ա") == null)
                    {
                        openTable("��Ա");
                    }
                    SpatialSearchAndView(geo, "��Ա");
                }
                if (checkBoxPaichusuo.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�����ɳ���") == null)
                    {
                        openTable("�����ɳ���");
                    }
                    SpatialSearchAndView(geo, "�����ɳ���");
                }
                if (checkBoxZhongdui.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�������ж�") == null)
                    {
                        openTable("�������ж�");
                    }
                    SpatialSearchAndView(geo, "�������ж�");
                }
                if (checkBoxJingwushi.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����������") == null)
                    {
                        openTable("����������");
                    }
                    SpatialSearchAndView(geo, "����������");
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                this.panError.Visible = false;
                if (i == 0)  // ����ж��Ƿ���ѡ��Χ���ж���
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2 - 250;
                    pt.Y = screen.WorkingArea.Height / 2 - 100;
                    this.panError.Location = pt;
                    this.panError.Visible = true;
                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "selectAndInsertByGeometry-37-�жϲ鿴ѡ���ͼ��");
            }
        }

        /// <summary>
        /// ��ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="tabAlias">����</param>
        private void openTable(string tabAlias)
        {
            try
            {
                 CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,getfrompath);
                string strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName;              

                DataTable datatable = GetTable(strSQL);
                if (datatable == null || datatable.Rows.Count < 1)
                {
                    return;
                }
                i = i + datatable.Rows.Count;
                //����ط��������ɵ�ͼ��������Map������
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tabAlias, datatable);
                MapInfo.Data.SpatialSchemaXY xy = new MapInfo.Data.SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0, 0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;

                MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

            }
            catch (Exception ex)
            {
                writeToLog(ex, "openTable-38-��ͼ��");
            }
        }


        /// <summary>
        /// �鿴ѡ����ʾͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="geo">ͼԪ</param>
        /// <param name="tabAlias">����</param>
        private void SpatialSearchAndView(FeatureGeometry geo, string tabAlias)
        {
            try
            {
                FeatureLayer fl = mapControl1.Map.Layers["�鿴ѡ��"] as FeatureLayer;
                Table ccTab = fl.Table;

                CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,getfrompath);
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(geo, ContainsType.Geometry);
                si.QueryDefinition.Columns = null;
                IResultSetFeatureCollection fc = MapInfo.Engine.Session.Current.Catalog.Search(tabAlias, si);
                //ͨ�������ƻ�ȡͼ��
                string bmpName = CLC.ForSDGA.GetFromTable.BmpName;
                CompositeStyle cs;
                if (bmpName == "gonggong")
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
                    cs = new CompositeStyle(new BitmapPointStyle(bmpName));
                }

                Feature newFt;
                foreach (Feature ft in fc)
                {
                    i++;
                    newFt = new Feature(ccTab.TableInfo.Columns);
                    newFt["��_ID"] = ft[CLC.ForSDGA.GetFromTable.ObjID];
                    newFt["����"] = ft[CLC.ForSDGA.GetFromTable.ObjName];
                    newFt["����"] = tabAlias;
                    newFt.Geometry = ft.Geometry;
                    newFt.Style = cs;
                    ccTab.InsertFeature(newFt);
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "SpatialSearchAndView-39-�鿴ѡ����ʾͼԪ");
            }
        }

        /// <summary>
        /// �鿴ѡ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.CheckBox check = (System.Windows.Forms.CheckBox)sender;

                if (!check.Checked)//ȥ��ѡ��,�Ӳ鿴ѡ����ɾ���������
                {
                    string tableAlies = check.Text;
                    CLC.ForSDGA.GetFromTable.GetFromName(tableAlies,getfrompath);

                    FeatureLayer fl = mapControl1.Map.Layers["�鿴ѡ��"] as FeatureLayer;
                    Table viewFtTable = fl.Table;
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("����='" + tableAlies + "'");
                    si.QueryDefinition.Columns = null;
                    IResultSetFeatureCollection fc = MapInfo.Engine.Session.Current.Catalog.Search(viewFtTable, si);
                    foreach (Feature ft in fc)
                    {
                        viewFtTable.DeleteFeature(ft);
                    }

                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "checkBox_CheckedChanged-39-�鿴ѡ��");
            }
        }

        //һ����Χ�ڵ���Ƶ�ͳ�����������Ƶͼ��ͳ���ͼ�㣬Ȼ��ѡ��һ����Χ�ڵ�
        /// <summary>
        /// ������Ƶͼ��ͳ���ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dp">����</param>
        /// <param name="dis"></param>
        //public void SearchDistance(DPoint dp, double dis)
        //{
        //    // ���� ��Ƶͼ�� 
        //    try
        //    {
        //        clVideo.ucVideo fv = new clVideo.ucVideo(mapControl1, st, this.tddb, StrCon, this.VideoPort, this.VideoString, VEpath, true,false);
        //        fv.getNetParameter(ns, vf);
        //        fv.strRegion = this.strRegion;
        //        fv.SearchVideoDistance(dp, dis);
                
        //    }
        //    catch(Exception ex)
        //    {
        //        writeToLog(ex, "clKaKou-ucKakou-40-������Ƶͼ��");    
        //    } 

        //    try
        //    {
        //        // ��������ͼ��
        //        clCar.ucCar fcar = new clCar.ucCar(mapControl1, null, StrCon, true);
        //        fcar.strRegion = strRegion;
        //        fcar.SearchCarDistance(dp, dis);
        //        fcar.ZhiHui = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        writeToLog(ex, "clKaKou-ucKakou-40-��������ͼ��");
        //    }
        //}

        /// <summary>
        /// ���ݷ�Χ������ΧҪ��
        /// </summary>
        /// <param name="dp">����</param>
        /// <param name="dis">��Χ</param>
        public void SearchDistance(DPoint dp, double dis)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                clearFeatures("�鿴ѡ��");   // �����֮ǰ�����ĵ�
                i = 0;
                if (checkBoxChangsuo.Checked)
                {
                    SearchSomeDistance(dp, dis, "��������");
                    i++;
                }
                if (checkBoxTezhong.Checked)
                {
                    SearchSomeDistance(dp, dis, "������ҵ");
                    i++;
                }
                if (checkBoxAnfang.Checked)
                {
                    SearchSomeDistance(dp, dis, "��ȫ������λ");
                    i++;
                }
                if (checkBoxXiaofangshuan.Checked)
                {
                    SearchSomeDistance(dp, dis, "����˨");
                    i++;
                }
                if (checkBoxWangba.Checked)
                {
                    SearchSomeDistance(dp, dis, "����");
                    i++;
                }
                if (checkBoxXiaofangdangwei.Checked)
                {
                    SearchSomeDistance(dp, dis, "�����ص㵥λ");
                    i++;
                }
                if (checkBoxZhikou.Checked)
                {
                    SearchSomeDistance(dp, dis, "�ΰ�����");
                    i++;
                }
                if (checkBoxPaichusuo.Checked)
                {
                    SearchSomeDistance(dp, dis, "�����ɳ���");
                    i++;
                }
                if (checkBoxShiping.Checked)
                {
                    SearchSomeDistance(dp, dis, "��Ƶλ��");
                    i++;
                }
                if (checkBoxZhongdui.Checked)
                {
                    SearchSomeDistance(dp, dis, "�������ж�");
                    i++;
                }
                if (checkBoxJingche.Checked)
                {
                    SearchSomeDistance(dp, dis, "����");
                    i++;
                }
                if (checkBoxJingwushi.Checked)
                {
                    SearchSomeDistance(dp, dis, "����������");
                    i++;
                }
                if (checkBoxJingyuan.Checked)
                {
                    SearchSomeDistance(dp, dis, "��Ա");
                    i++;
                }

                if (i == 0)
                {
                    MessageBox.Show("������ѡ��������ͺ��ѯ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SearchDistance");
            }
        }

        /// <summary>
        /// �ܱ߲�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dpt">����</param>
        /// <param name="dis">��Χ</param>
        /// <param name="tabAils">����</param>
        public void SearchSomeDistance(MapInfo.Geometry.DPoint dpt, Double dis,string tabAils)
        {
            try
            {
                //FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["�鿴ѡ��"];
                //if (fl == null)   // Ϊ���ȴ���
                //{
                //    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //    //������ʱ��
                //    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("�鿴ѡ��");
                //    Table tblTemp = Cat.GetTable("�鿴ѡ��");
                //    if (tblTemp != null)
                //    {
                //        Cat.CloseTable("�鿴ѡ��");
                //    }

                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("��_ID", 40));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("����", 50));
                //    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("����", 50));

                //    tblTemp = Cat.CreateTable(tblInfoTemp);
                //    FeatureLayer lyr = new FeatureLayer(tblTemp);
                //    mapControl1.Map.Layers.Insert(0, lyr);

                //    //��ӱ�ע
                //    string activeMapLabel = "DistanceLabel";
                //    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("�鿴ѡ��");
                //    MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //    MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //    lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                //    lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                //    lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                //    lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //    lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                //    lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //    lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                //    lbsource.DefaultLabelProperties.Caption = "����";
                //    lblayer.Sources.Append(lbsource);
                //    mapControl1.Map.Layers.Add(lblayer);
                //}
                //else����// ��Ϊ�������֮ǰ�ĵ�
                //{
                //    clearFeatures("�鿴ѡ��");
                //}

                //��VideoLayer��ѡ���ܱߵ���Ƶ������ӵ�tempvideo

                double x1, x2;
                double y1, y2;
                double x, y;

                double dBufferDis = dis / 111000;
                x = dpt.x;
                y = dpt.y;
                x1 = x - dBufferDis;
                x2 = x + dBufferDis;
                y1 = y - dBufferDis;
                y2 = y + dBufferDis;


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrvideo = mapControl1.Map.Layers["�鿴ѡ��"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblvideo = MapInfo.Engine.Session.Current.Catalog.GetTable("�鿴ѡ��");

                //string[] camidarr = null;
                string sql = string.Empty;       // sql���
                string objId = string.Empty;���� // ���IDֵ
                string objName = string.Empty;�� // �������ֵ��
                string tableName = string.Empty; // ����

                CLC.ForSDGA.GetFromTable.GetFromName(tabAils, getfrompath);
                tableName = CLC.ForSDGA.GetFromTable.TableName;
                objId = CLC.ForSDGA.GetFromTable.ObjID;
                objName = CLC.ForSDGA.GetFromTable.ObjName;

                if (strRegion == string.Empty)   //add by fisher in 09-12-08
                {
                    MessageBox.Show(@"��û������Ȩ�ޣ�", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (strRegion == "˳����")
                {
                    if (tableName == "��Ƶλ��")
                    {
                        sql = "Select * from ��Ƶλ��VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                        tableName = "��Ƶ";
                    }
                    else
                    {
                        sql = "Select * from " + tableName + " where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                    }
                }
                else
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1 && strRegion.IndexOf("��ʤ") < 0)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    if (tableName == "��Ƶλ��")
                    {
                        sql = "Select * from ��Ƶλ��VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        tableName = "��Ƶ";
                    }
                    else
                    {
                        sql = "Select * from " + tableName + " where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }

                DataTable dt = GetTable(sql);
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        foreach (DataRow dr in dt.Rows)
                        {

                            string camid = dr[objId].ToString();
                            if (camid != "" && dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr[objId].ToString() != "")
                            {
                                FeatureGeometry pt = new MapInfo.Geometry.Point(lyrvideo.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                                CompositeStyle cs = new CompositeStyle();

                                cs = new CompositeStyle(new BitmapPointStyle(CLC.ForSDGA.GetFromTable.BmpName, BitmapStyles.ApplyColor, System.Drawing.Color.Red, 12));
                                Feature ftr = new Feature(tblvideo.TableInfo.Columns);
                                ftr.Geometry = pt;
                                ftr.Style = cs;
                                ftr["��_ID"] = dr[objId].ToString();
                                ftr["����"] = dr[objName].ToString();
                                ftr["����"] = tableName;
                                tblvideo.InsertFeature(ftr);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "SearchSomeDistance");
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SearchSomeDistance");
            }
        }

        /// <summary>
        /// ��ͼ�ϻ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="idname">�������</param>
        /// <param name="kkname">���ڱ��</param>
        /// <param name="CarName">�������</param>
        private void InsertSearchPoint(string idname, string kkname, string CarName)
        {
            try
            {
                MapInfo.Mapping.Map map = this.mapControl1.Map;

                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["KKSearchLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("KKSearchLayer");


                double Cx = 0;
                double Cy = 0;

                //////��ȡ

                DataTable dt1 = GetTable("select X,Y from �ΰ�����ϵͳ where ���ڱ��='" + kkname + "'");

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
                    ftr["Name"] = CarName;
                    ftr["ID"] = idname;
                    tblcar.InsertFeature(ftr);

                    string tempname = idname;

                    string tblname = "KKSearchLayer";


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
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where ID = @name ";
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
                writeToLog(ex, "InsertSearchPoint");
            }
        }


        /// <summary>
        /// �����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dgvres_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            MapInfo.Data.MIConnection conn = new MIConnection();
            try
            {
                DataGridView portQueryView = (DataGridView)sender;

                this.dgvres.Rows[e.RowIndex].Selected = true;
                string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                string tblname = "KKSearchLayer";

                MapInfo.Mapping.Map map = this.mapControl1.Map;

                if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                MapInfo.Geometry.CoordSys cSys = this.mapControl1.Map.GetDisplayCoordSys();

                getFeatureCollection(tempname, tblname);

                    string idname = string.Empty;
                    string kkname = String.Empty;
                    string CarName = string.Empty;
                    string serNum = string.Empty;

                    if (this.comboxTable.Text == "����ͨ����Ϣ")
                    {
                        //ͨ�г������,���ڱ��,��������,ͨ��ʱ��,��������,��������,������ɫ,��ɫ��ǳ,��ʻ����,��Ƭ1,��Ƭ2,��Ƭ3
                        int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // ���һ�е�λ��
                        idname = portQueryView.CurrentRow.Cells[0].Value.ToString();
                        kkname = portQueryView.CurrentRow.Cells[1].Value.ToString();
                        CarName = portQueryView.CurrentRow.Cells[4].Value.ToString();
                        serNum = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // ���к�


                        DataRow[] row = dataKaKou.Select("���к�='" + serNum + "'");
                        picturePoint(row[0]);                                              // �����µı��

                        getFeatureCollection(serNum, tblname);
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
                    else if (this.comboxTable.Text == "����������Ϣ")
                    {
                        //�������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3
                        idname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                        kkname = portQueryView.CurrentRow.Cells[2].Value.ToString();
                        CarName = portQueryView.CurrentRow.Cells[5].Value.ToString();

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

                        this.InsertSearchPoint(idname, kkname, CarName);
                    }
                    else if (this.comboxTable.Text == "�ΰ�������Ϣ")
                    {
                        //���ڱ��,��������,��װ�ص�,��ص��������,���ڶ�Ӧ������,��س�����,�����ɳ���,��ط���,��ϵ��,��ϵ��ʽ,��Դ as ������Դ

                        idname = portQueryView.CurrentRow.Cells[0].Value.ToString();
                        CarName = portQueryView.CurrentRow.Cells[1].Value.ToString();
                        kkname = portQueryView.CurrentRow.Cells[0].Value.ToString();

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

                        this.InsertSearchPoint(idname, kkname, CarName);
                    }

                //}
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvres_CellClick-41-�����¼�");
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
        /// ˫���¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dgvres_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //�����ͷ,�˳�

            try
            {
                DataGridView portQueryView = (DataGridView)sender;
                string strSQL = string.Empty;
                DataTable datatable = new DataTable();

                if (this.comboxTable.Text == "����ͨ����Ϣ")
                {
                    string[] sqlFields ={ "ͨ�г������", "���ڱ��", "��������", "ͨ��ʱ��", "��������", "��������", "������ɫ", "��ɫ��ǳ", "��ʻ����", "��Ƭ1", "��Ƭ2", "��Ƭ3" };

                    datatable = new DataTable("TemData");
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
                        datatable.Columns.Add(dc);
                    }

                    DataRow dr = datatable.NewRow();
                    for (int i = 0; i < sqlFields.Length; i++)
                    {
                        dr[i] = portQueryView.CurrentRow.Cells[sqlFields[i]].Value.ToString();
                    }
                    datatable.Rows.Add(dr);

                    /////////���ݵ�ǰͨ�г�������ж�ͼƬ��������ַ
                    try
                    {
                        if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "01")   //�ߵ�����Ƶ��ͼƬ��������ַ
                        {
                            photoserver = gdwserver;
                        }
                        else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "02") //�׻�¼��Ƶ��ͼƬ��������ַ
                        {
                            photoserver = ehlserver;
                        }
                        else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(2, 2) == "03")  //������Ƶ��ͼƬ��������ַ
                        {
                            photoserver = bkserver;
                        }
                        else if (portQueryView.Rows[e.RowIndex].Cells[0].Value.ToString().IndexOf("lbschina") > -1)
                        {
                            photoserver = "http://192.168.0.50/";
                        }
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "����ͨ����Ϣ����ip��ַ��ȡͼƬ��������ַ");
                    }

                    WriteEditLog(strSQL, "�鿴����������Ϣ", "V_CROSS");
                }
                else if (this.comboxTable.Text == "����������Ϣ")
                {

                    string sqlFields = "�������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3,����״̬,������,����ʱ��,�������";
                    strSQL = "select " + sqlFields + " from V_ALARM t where �������='" + portQueryView.CurrentRow.Cells[0].Value.ToString() + "'";
                    datatable = GetTable(strSQL);
                    WriteEditLog(strSQL, "�鿴����������Ϣ", "V_ALARM");

                }
                else if (this.comboxTable.Text == "�ΰ�������Ϣ")
                {
                    strSQL = "select ���ڱ��,��������,��װ�ص�,��ص��������,���ڶ�Ӧ������,��س�����,�����ɳ���,��ط���,��ϵ��,��ϵ��ʽ from �ΰ�����ϵͳ where ���ڱ��='" + portQueryView.CurrentRow.Cells[0].Value.ToString() + "'";
                    datatable = GetTable(strSQL);
                    WriteEditLog(strSQL, "�鿴�ΰ�������Ϣ", "�ΰ�����ϵͳ");
                }

                DPoint dp = new DPoint();
                if (datatable != null && datatable.Rows.Count > 0)
                {
                    //////////////////////////////////////////
                    string tempname = portQueryView.CurrentRow.Cells[0].Value.ToString();

                    if (this.comboxTable.Text == "����ͨ����Ϣ")
                    {
                        int lastCell = portQueryView.CurrentRow.Cells.Count - 1;          // ���һ�е�λ��
                        tempname = portQueryView.CurrentRow.Cells[lastCell].Value.ToString();  // ���к�
                    }

                    string tblname = "KKSearchLayer";

                    //��ȡ��ǰѡ�����Ϣ��ͨ�г��������Ϊ����ֵ

                    MapInfo.Mapping.Map map = this.mapControl1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }
                    try
                    {
                        getFeatureCollection(tempname, tblname);
                        if (this.rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            foreach (Feature f in this.rsfcflash)
                            {
                                mapControl1.Map.Center = f.Geometry.Centroid;
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }
                            //cmd.Clone();
                            this.timerFlash.Enabled = true;
                        }
                    }
                    catch (Exception ex) { writeToLog(ex, "�ڵ�ͼ��������Ӧ��Ϣʱ��������"); }
                    /////////////////////////////////////////

                    if (dp.x == 0 || dp.y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("�˶���δ��λ!");
                        return;
                    }

                    System.Drawing.Point pt = new System.Drawing.Point();
                    mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvres_CellDoubleClick-42-˫���¼�");
            }
            finally
            {
                try { fmDis.Close(); }
                catch { }
            }
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (this.ALARMSYS == "ϵͳ")
        //        {
        //            this.timalarm.Enabled = true;
        //        }
        //        else if (this.ALARMSYS == "ģ��")
        //        {
        //            if (this.Visible == true)
        //            {
        //                this.timalarm.Enabled = true;
        //            }
        //            else
        //            {
        //                this.timalarm.Enabled = false;
        //                this.ftip.Visible = false;
        //            }
        //        }
        //        else
        //        {
        //            this.timalarm.Enabled = false;
        //            this.ftip.Visible = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        writeToLog(ex, "timer1_Tick");
        //    }
        //}

        /// <summary>
        /// ��ʼ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            InitAlarmSet();
        }
        
        /// <summary>
        /// ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.rdboxsys.Checked == true)
                {
                    this.ALARMSYS = "ϵͳ";
                }
                else if (this.rdboxmod.Checked == true)
                {
                    this.ALARMSYS = "ģ��";
                }

                if (this.rdboxall.Checked == true)
                {
                    this.ALARMUSER = "����";
                }
                else if (this.rdboxuser.Checked == true)
                {
                    this.ALARMUSER = "�û�";
                }

                ftip.AlarmSys = this.ALARMSYS;
                ftip.AlarmUser = this.ALARMUSER;

                this.SCHDIS = Convert.ToDouble(this.txtdist.Text.Trim());

                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
             
                CLC.INIClass.IniWriteValue("�ΰ�����", "ϵͳ����", ALARMSYS);
                CLC.INIClass.IniWriteValue("�ΰ�����", "��������", ALARMUSER);
                CLC.INIClass.IniWriteValue("�ΰ�����", "��ѯ�뾶", this.txtdist.Text.Trim());

                MessageBox.Show(@"�������óɹ��������������޸Ĳ�����Ч",@"ϵͳ��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Information);

                WriteEditLog(ALARMSYS + ":" + ALARMUSER + ":" + this.txtdist.Text.Trim(), "���Ŀ�������", "�����ļ�");
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"��������ʧ��", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                writeToLog(ex, "btnOK_Click-43-��������");
            }                 
        }


       /// <summary>
        /// ��������غ���ʾ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
       /// </summary>
        private void ucKakou_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.ALARMSYS == "ϵͳ")
                {
                    ftip.timalarm.Enabled = true;
                }
                else if (this.ALARMSYS == "ģ��")
                {
                    if (this.Visible == true)
                    {
                        ftip.timalarm.Enabled = true;
                    }
                    else
                    {
                        ftip.timalarm.Enabled = false;
                        ftip.Visible = false;
                    }
                }
                else
                {
                    ftip.timalarm.Enabled = false;
                    ftip.Visible = false;
                }


                if (this.Visible == false)
                {
                    this.ClearKaKou();
                    ftip.isKakou = false;
                }
                else
                {
                    FeatureLayer fl = mapControl1.Map.Layers["�鿴ѡ��"] as FeatureLayer;
                    if (fl != null)
                    {
                        labeLayer(fl.Table, "����");
                    }

                    ftip.isKakou = true;
                }
                isDel = false;
                this.dateFrom.Value = DateTime.Now.Date;
                this.dateTo.Value = DateTime.Now.Date;
                this.panError.Visible = false;    // ���ش�����ʾ
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucKakou_VisibleChanged-44-��������غ���ʾ");
            }
        }

        /// <summary>
        /// ������ע
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="editTable">ͼ������</param>
        /// <param name="field">��ע�ֶ�</param>
        private void labeLayer(Table editTable, string field)
        {
            try
            {
                LabelLayer labelLayer = mapControl1.Map.Layers["��עͼ��"] as LabelLayer;

                LabelSource source = new LabelSource(editTable);

                source.DefaultLabelProperties.Caption = field;
                source.DefaultLabelProperties.Layout.Offset = 4;
                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;
                //source.DefaultLabelProperties.Visibility.VisibleRangeEnabled = true;
                //source.DefaultLabelProperties.Visibility.VisibleRange = new VisibleRange(0.0, 10, DistanceUnit.Kilometer);

                labelLayer.Sources.Insert(0, source);
            }

            catch (Exception ex)
            {
                writeToLog(ex, "labeLayer-45-������ע");
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
        /// ��ҳ��ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="lblcount">��ʾ�ܼ�¼�ؼ�</param>
        /// <param name="textNowPage">��ʾ��ǰҳ���ؼ�</param>
        /// <param name="lblPageCount">��ʾ��ҳ���ؼ�</param>
        /// <param name="bs">����Դ�ؼ�</param>
        /// <param name="bn">��ҳ�ؼ�</param>
        /// <param name="dgv">��ʾ�б�ؼ�</param>
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

                LoadData1(textNowPage,lblPageCount,bs,bn,dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "47-InitDataSet1");
            }
        }

        /// <summary>
        /// ��ѯ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="textNowPage">��ʾ��ǰҳ���ؼ�</param>
        /// <param name="lblPageCount">��ʾ��ҳ���ؼ�</param>
        /// <param name="bds">����Դ�ؼ�</param>
        /// <param name="bdn">��ҳ�ؼ�</param>
        /// <param name="dgv">��ʾ�б�ؼ�</param>
        public void LoadData1(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
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
                //��Ԫ����Դ���Ƽ�¼��
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt1.Rows[i]);
                    PagenCurrent1++;
                }
                dataKaKou = new DataTable();
                bds.DataSource = dataKaKou = dtTemp;
                
                CreateKakouTrack(dtTemp);

                bdn.BindingSource = bds;
                dgv.DataSource = bds;
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "48-LoadData1");
            }
         }


        /// <summary>
        /// ҳ�浼��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent1--;
                    if (pageCurrent1 <1)
                    {
                        pageCurrent1 = 1;
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��");
                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }

                    LoadData1(PageNow1,PageNum1,bindingSource1 ,bindingNavigator1,dataGridViewKakou);
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
                    LoadData1(PageNow1,PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
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
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
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
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                }
                else if (e.ClickedItem.Text == "���ݵ���")
                {
                    DataExport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "bindingNavigator1_ItemClicked-49-ҳ�浼��");
            }
        }

       
        public DataTable _exportDT = null;
        /// <summary>
        /// �������ݵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void DataExport()
        {
            try
            {
                if (_exportDT != null)
                {

                    FolderBrowserDialog sfd = new FolderBrowserDialog();
                    sfd.ShowNewFolderButton =true;
                    //sfd.FileName = "EXP" + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        DataView dataview = _exportDT.DefaultView;
                        DataTable dt = dataview.ToTable(true, "��������");
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {

                                string KKnamet = dr["��������"].ToString();

                                DataRow[] drp = _exportDT.Select("��������='" + KKnamet + "'");
                                DataTable dtt = _exportDT.Clone();
                                for (int i = 0; i < drp.Length; i++)
                                {
                                    dtt.Rows.Add(drp[i].ItemArray);
                                }
                                string StoreName = sfd.SelectedPath + @"\" + KKnamet + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);

                                if (Directory.Exists(StoreName)==false)
                                {
                                    Directory.CreateDirectory(StoreName);                                    
                                } 

                                ExportExcel(dtt,StoreName + @"\" + KKnamet + ".xls");
                                ExportPic(dtt, StoreName);
                            }

                            MessageBox.Show("�������");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "50-DataExport");

            }

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// ����ͼƬ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="StoreName">����ͼƬ��ַ</param>
        private void ExportPic(DataTable dt, string StoreName)
        {
            try
            {
                if (dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                    {
                        /////////���ݵ�ǰͨ�г�������ж�ͼƬ��������ַ
                        try
                        {
                            if (dr[0].ToString().Substring(2, 2) == "01")   //�ߵ�����Ƶ��ͼƬ��������ַ
                            {
                                photoserver = gdwserver;
                            }
                            else if (dr[0].ToString().Substring(2, 2) == "02") //�׻�¼��Ƶ��ͼƬ��������ַ
                            {
                                photoserver = ehlserver;
                            }
                            else if (dr[0].ToString().Substring(2, 2) == "03")  //������Ƶ��ͼƬ��������ַ
                            {
                                photoserver = bkserver;
                            }
                            else if (dr[0].ToString().IndexOf("lbschina") > -1)
                            {
                                photoserver = "http://192.168.0.50/";
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "clKaKou-ucKakou-20-����ͨ����Ϣ����ip��ַ��ȡͼƬ��������ַ");
                        }

                        if (dr["��Ƭ1"].ToString() != "")
                            SavePic(photoserver + dr["��Ƭ1"].ToString(), StoreName);

                        if (dr["��Ƭ2"].ToString() != "")
                            SavePic(photoserver + dr["��Ƭ2"].ToString(), StoreName);

                        if (dr["��Ƭ3"].ToString() != "")
                            SavePic(photoserver + dr["��Ƭ3"].ToString(), StoreName);
                    }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "50-ExportPic");
            }
        }

        /// <summary>
        /// ����ͼƬ
        /// </summary>
        /// <param name="filename">ͼƬ��ַ</param>
        /// <param name="storename">�����ַ</param>
        private void SavePic(string filename, string storename)
        {
            try
            {
                if (filename != "")
                {
                    //Ϊ�˷�ֹ��ǰ�������д��ڲ������ַ�����ftp��������ַ����http��������ַ���ִ����ڴ˽��д����ַ�Replace��
                    if (filename.IndexOf("\\") > 0)
                    {
                        filename = filename.Replace("\\", "/");
                    }

                    string filen = filename.Substring(filename.LastIndexOf('/') + 1, filename.Length - filename.LastIndexOf('/') - 1); //ȡ���ļ���

                    System.Net.WebClient client = new WebClient();
                   
                    client.DownloadFile(filename, storename + "\\" + filen);
                    client = null;                   
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "50-SavePic:"+filename);
            }

        }

        /// <summary>
        /// ���ڵ���Excel
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="fileName">��ַ</param>
        private void ExportExcel(DataTable dt, string fileName) 
        {
            try
            {      
                    LBSDataGuide.DataGuide dg = new LBSDataGuide.DataGuide();
             
                    this.Cursor = Cursors.WaitCursor;

                    dg.OutData(fileName, dt, "�ΰ�����ϵͳ");        
            }
            catch(Exception ex)
            {
                this.writeToLog(ex,"ExportExcel");
            }
            
        }


        //==========
        //==========
        //��ҳ����2
        //==========
        //==========

        int pageSize2 = 100;     //ÿҳ��ʾ����
        int PagenMax2 = 0;         //�ܼ�¼��
        int pageCount2 = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageCurrent2 = 0;   //��ǰҳ��
        int PagenCurrent2 = 0;      //��ǰ��¼�� 
        DataSet Pageds2 = new DataSet();
        DataTable Pagedt2 = new DataTable();

        /// <summary>
        /// ��ҳ��ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="lblcount">��ʾ�ܼ�¼�ؼ�</param>
        /// <param name="textNowPage">��ʾ��ǰҳ���ؼ�</param>
        /// <param name="lblPageCount">��ʾ��ҳ���ؼ�</param>
        /// <param name="bs">����Դ�ؼ�</param>
        /// <param name="bn">��ҳ�ؼ�</param>
        /// <param name="dgv">��ʾ�б�ؼ�</param>
        public void InitDataSet2(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize2 = 100;      //����ҳ������
                PagenMax2 = Pagedt2.Rows.Count;

                lblcount.Text = "��" + PagenMax2.ToString() + "����¼";//�ڵ���������ʾ�ܼ�¼��

                pageCount2 = (PagenMax2 / pageSize2);//�������ҳ��
                if ((PagenMax2 % pageSize2) > 0) pageCount2++;
                if (PagenMax2 != 0)
                {
                    pageCurrent2 = 1;
                }
                PagenCurrent2 = 0;       //��ǰ��¼����0��ʼ

                LoadData2(textNowPage,lblPageCount,bs, bn, dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "52-InitDataSet2");
            }
        }

        /// <summary>
        /// ��ѯ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="textNowPage">��ʾ��ǰҳ���ؼ�</param>
        /// <param name="lblPageCount">��ʾ��ҳ���ؼ�</param>
        /// <param name="bds">����Դ�ؼ�</param>
        /// <param name="bdn">��ҳ�ؼ�</param>
        /// <param name="dgv">��ʾ�б�ؼ�</param>
        public void LoadData2(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = Pagedt2.Clone();

                if (pageCurrent2 == pageCount2)
                    nEndPos = PagenMax2;
                else
                    nEndPos = pageSize2 * pageCurrent2;
                nStartPos = PagenCurrent2;

                //tsl.Text = Convert.ToString(pageCurrent2) + "/" + pageCount2.ToString();
                textNowPage.Text = Convert.ToString(pageCurrent2);
                lblPageCount.Text = "/" + pageCount2.ToString();

                //��Ԫ����Դ���Ƽ�¼��
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt2.Rows[i]);
                    PagenCurrent2++;
                }
                dataKaKou = new DataTable();
                bds.DataSource = dataKaKou = dtTemp;

                if (this.comboxTable.Text == "����ͨ����Ϣ")  // ����ͨ����Ϣ�Ļ�������
                {
                    CreateKakouTrack(dtTemp);
                }

                bdn.BindingSource = bds;
                dgv.DataSource = bds;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "53-LoadData2");
            }
        }

        /// <summary>
        /// ҳ�浼��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void bindingNavigator2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent2--;
                    if (pageCurrent2 <1)
                    {
                        pageCurrent2 = 1;
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��");
                        return;
                    }
                    else
                    {
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    }

                    LoadData2(PageNow2,PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent2++;
                    if (pageCurrent2 > pageCount2)
                    {
                        pageCurrent2 = pageCount2;
                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��");
                        return;
                    }
                    else
                    {
                        PagenCurrent2 = pageSize2 * (pageCurrent2- 1);
                    }
                    LoadData2(PageNow2,PageNum2,bindingSource2, bindingNavigator2, dgvres);
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent2 <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent2 = 1;
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    }
                    LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent2 > pageCount2 - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent2 = pageCount2;
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    }
                    LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
                else if (e.ClickedItem.Text == "���ݵ���")
                {
                    DataExport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                writeToLog(ex, "54-bindingNavigator2_ItemClicked");
            }
        }

        /// <summary>
        /// ȡ����������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void button8_Click(object sender, EventArgs e)
        {
            InitAlarmSet();
        }

        /// <summary>
        /// ���ݵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void btnDateOut_Click(object sender, EventArgs e)
        {
            DataExport();
        }

        /// <summary>
        /// ����ÿҳ��ʾ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridViewKakou.Rows.Count > 0)
                {
                    pageSize1 = Convert.ToInt32(TextNum1.Text);
                    pageCurrent1 = 1;   //��ǰת����һҳ
                    PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    pageCount1 = (PagenMax1 / pageSize1);//�������ҳ��
                    if ((PagenMax1 % pageSize1) > 0) pageCount1++;

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                }
            }
            catch(Exception ex)            
            {
                writeToLog(ex, "55-TextNum1_KeyPress");
            }
        }

        /// <summary>
        /// ҳ��ת��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
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
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, dataGridViewKakou);
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "56-PageNow1_KeyPress");
            }
        }

        /// <summary>
        /// ��������ͨ����Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void btnDataOut1_Click(object sender, EventArgs e)
        {
            if (this.comboxTable.Text == "����ͨ����Ϣ")  //V_Cross
            {
                DataExport();
            }
        }

        /// <summary>
        /// ����ÿҳ��ʾ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void PageNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && this.dgvres.Rows.Count > 0)
                {
                    pageSize2 = Convert.ToInt32(this.PageNumber.Text);
                    pageCurrent2 = 1;   //��ǰת����һҳ
                    PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                    pageCount2 = (PagenMax2 / pageSize2);//�������ҳ��
                    if ((PagenMax2 % pageSize2) > 0) pageCount2++;

                    LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "57-PageNumber_KeyPress");
            }
        }

        /// <summary>
        /// ҳ��ת��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void PageNow2_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(this.PageNow2.Text) < 1 || Convert.ToInt32(this.PageNow2.Text) > pageCount2)
                    {
                        System.Windows.Forms.MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow2.Text = pageCurrent2.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent2 = Convert.ToInt32(this.PageNow2.Text);
                        PagenCurrent2 = pageSize2 * (pageCurrent2 - 1);
                        LoadData2(PageNow2, PageNum2, bindingSource2, bindingNavigator2, dgvres);
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "58-PageNow2_KeyPress");
            }
        }

        /// <summary>
        /// �л�Tab
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.dataGridViewKakou.DataSource = null;
                this.dgvres.DataSource = null;
                this.chkmh.Checked = false;       // ȥ��ģ����ѯ
                this.panError.Visible = false;    // ���ش�����ʾ
                this.comboxCon.SelectedIndex = 0; // �л�ģ��ʱ��ԭ����Ϊ�����ڡ�
                this.fuzzyFlag = false;           // �����ж��Ƿ���ģ����ѯ��boolֵ��Ϊfalse
                dataKaKou = null;                

                this.dateFrom.Value = DateTime.Now.Date;
                this.dateTo.Value = DateTime.Now.Date;

                if (tabControl1.SelectedTab == tabPage1)   //  ������ѯ
                {
                    if (this.groupBox1.Visible)
                        this.linkLabel1.Text = "����������";
                    else
                        this.linkLabel1.Text = "��ʾ������";

                    this.linkLabel1.Visible = true;
                }
                if (tabControl1.SelectedTab == tabPage2)   //  ָ�Ӽ����� 
                {
                    this.linkLabel1.Visible = false;
                }
                if (tabControl1.SelectedTab == tabPage3)   //  ��ϲ�ѯ
                {
                    if (this.groupBox4.Visible)
                        this.linkLabel1.Text = "����������";
                    else
                        this.linkLabel1.Text = "��ʾ������";

                    this.linkLabel1.Visible = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "tabControl1_SelectedIndexChanged-61-�л�Tab");
            }
        }

        /// <summary>
        /// ��ѯSQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }
       
        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clKaKou-ucKakou-" + sFunc);
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">��ǰ������SQL</param>
        /// <param name="method">������ʽ</param>
        /// <param name="tablename">��������</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�ΰ�����','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch(Exception ex)
            {
                writeToLog(ex, "WriteEditLog-61-��¼������¼");
            }           
        }

        /// <summary>
        /// �Զ���ȫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboxField.Text == "��������" || comboxField.Text == "���ڱ��" || comboxField.Text == "������������" || comboxField.Text == "�������ڱ��")
                {
                    string keyword = this.CaseSearchBox.Text.Trim();
                    string colword = string.Empty;

                    if (comboxField.Text.IndexOf("��������") > -1)
                    {
                        colword = "��������";
                    }
                    else if (comboxField.Text.IndexOf("���ڱ��") > -1)
                    {
                        colword = "���ڱ��";
                    }

                    if (keyword != "" && colword != "")
                    {
                        string strExp = "select distinct(" + colword + ") from �ΰ�����ϵͳ t where " + colword + " like '%" + keyword + "%'  order by "+ colword;
                        DataTable dt = GetTable(strExp);
                        this.CaseSearchBox.GetSpellBoxSource(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CaseSearchBox_TextChanged-59");
            }
        }

        /// <summary>
        /// �Զ���ȫ��ȫ�ǰ�Ǵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
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
            
            
            try
            {
                string colfield = this.comboxField.Text.Trim();
                string strExp = string.Empty;

                if(colfield == "�����ɳ���")
                {
                    strExp = "select distinct(�ɳ�����) from �����ɳ��� order by �ɳ�����";
                  
                }
                else if (colfield == "��Դ")
                {
                    strExp = "select distinct(��Դ) from �ΰ�����ϵͳ order by ��Դ";
                }

                DataTable dt = GetTable(strExp);
                if (dt.Rows.Count > 0)
                    this.CaseSearchBox.GetSpellBoxSource(dt);
                else
                    this.CaseSearchBox.GetSpellBoxSource(null);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CaseSearchBox_MouseDown-35-�����ʾ����");
            }
        }

        /// <summary>
        /// ��ʾ�����ؽ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="falg">����ֵ��true-��ʾ false-���أ�</param>
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
                writeToLog(ex, "isShowPro-35-�����ʾ����");
            }
        }

        /// <summary>
        /// ���ػ���ʾ����������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.tabControl1.SelectedTab == this.tabPage1)    //  ������ѯ
                    HidesCondition(sender, e, this.groupBox1, null);
                if (this.tabControl1.SelectedTab == this.tabPage3)    //  ��ϲ�ѯ
                    HidesCondition(sender, e, this.groupBox4, this.CaseSearchBox);
                if (this.tabControl1.SelectedTab == this.tabPage2)    // ָ�Ӽ����� 
                    return;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "linkLabel1_LinkClicked-35-�����ʾ����");
            }
        }

        /// <summary>
        /// ���ػ���ʾ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="groupBox">Ҫ���ػ���ʾ�Ŀؼ�</param>
        /// <param name="text">Ҫ���ػ���ʾ�Ŀؼ�</param>
        private void HidesCondition(object sender, LinkLabelLinkClickedEventArgs e, System.Windows.Forms.GroupBox groupBox, SplitWord.SpellSearchBoxEx text)
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
                writeToLog(ex, "HidesCondition");
            }
        }

        /// <summary>
        /// ȫ�ǰ�Ǵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void txtBoxCar_MouseDown(object sender, MouseEventArgs e)
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

        /// <summary>
        /// ����쵽��ʾ������ʾ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void txtBoxCar_MouseEnter(object sender, EventArgs e)
        {
            this.groupBox3.Visible = true;
        }

        /// <summary>
        /// ����ƿ�����������ʾ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void txtBoxCar_MouseLeave(object sender, EventArgs e)
        {
            this.groupBox3.Visible = false;
        }

        /// <summary>
        /// ������־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="funName">�޸ı��</param>
        /// <param name="id">������к�</param>
        private void WriteLog(string funName,string id)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\TestLog.txt";
                strWri = new StreamWriter(exePath, true);
                if (funName != null)
                    strWri.WriteLine("�޸ı�ţ�  " + funName);
                if (id != null)
                    strWri.WriteLine("������кţ� " + id);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "WriteLog");
            }
        }

        private DataTable dataKaKou;             // ������ʾ�Ŵ����ݵ��ڴ��
        private clPopu.frmDisplay fmDis;

        /// <summary>
        /// ������ѯ �Ŵ����ݰ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void btnEnal_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataKaKou == null)
                {
                    System.Windows.Forms.MessageBox.Show("������չʾ�����ѯ�����ݺ�Ŵ�鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new clPopu.frmDisplay(dataKaKou);

                fmDis.dataGridDisplay.CellClick += this.dataGridViewKakou_CellClick;               // �󶨵����¼�
                fmDis.dataGridDisplay.CellDoubleClick += this.dataGridViewKakou_CellDoubleClick;   // ��˫���¼�

                fmDis.Show();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnEnal_Click");
            }
        }

        /// <summary>
        /// ��ϲ�ѯ �Ŵ����ݰ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void btnEnalData_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataKaKou == null)
                {
                    System.Windows.Forms.MessageBox.Show("������չʾ�����ѯ�����ݺ�Ŵ�鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new clPopu.frmDisplay(dataKaKou);

                fmDis.dataGridDisplay.CellClick += this.dgvres_CellClick;               // �󶨵����¼�
                fmDis.dataGridDisplay.CellDoubleClick += this.dgvres_CellDoubleClick;   // ��˫���¼�

                fmDis.Show();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnEnalData_Click");
            }     
        }

        //private clPopu.FrmHouseInfo frmMessage = new clPopu.FrmHouseInfo();
        private frmMessage frmMessage = new frmMessage();
        /// <summary>
        /// ��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="rfc">��ͼҪ��</param>
        /// <param name="po">��ʾλ��</param>
        public void getMessage(IResultSetFeatureCollection rfc,System.Drawing.Point po)
        {
            try
            {

                if (rfc.Count > 0)
                {
                    if (this.frmMessage.Visible == false)
                    {
                        this.frmMessage = new frmMessage();
                        this.frmMessage.SetDesktopLocation(-30, -30);
                        this.frmMessage.Show();
                        this.frmMessage.Visible = false;
                    }

                    this.frmMessage.getFromNamePath = this.getfrompath;
                    this.frmMessage.mapControl = mapControl1;
                    this.frmMessage.LayerName = "�鿴ѡ��";
                    this.frmMessage.setInfo(rfc[0], StrCon);
                    this.frmMessage.Show();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnEnalData_Click");
            } 
        }

        /// <summary>
        /// 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="ftr">Feature����</param>
        /// <param name="myconStr">���ݿ������ַ���</param>
        /// <returns></returns>
        private DataRow featureStyle(IResultSetFeatureCollection ftr, string myconStr)
        {
            #region
            //try
            //{
            //    //ͨ���鿴�ֶ�,�����"��_ID",˵������ʱ��
            //    bool isTemTab = false;
            //    foreach (MapInfo.Data.Column col in ftr.Columns)
            //    {
            //        String upAlias = col.Alias.ToUpper();
            //        if (upAlias.IndexOf("��_ID") > -1)
            //        {
            //            isTemTab = true;
            //            break;
            //        }
            //    }
            //    DataTable dt = null;
            //    DataRow row = null;
            //    if (isTemTab)
            //    {
            //        string strTabName = ftr["����"].ToString();
            //        if (strTabName.IndexOf("��Ƶ") >= 0)
            //        {
            //            strTabName = "��Ƶ";
            //        }
            //        CLC.ForSDGA.GetFromTable.GetFromName(strTabName, getfrompath);

            //        string strSQL1 = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr["��_ID"].ToString() + "'";
            //        if (CLC.ForSDGA.GetFromTable.TableName == "��ȫ������λ")
            //            strSQL1 = "select ���,��λ����,��λ����,��λ��ַ,���ܱ�������������,�ֻ�����,�����ɳ���,�����ж�,����������,'����鿴' as ��ȫ������λ�ļ�,��ע��,��עʱ��,X,Y from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr["��_ID"].ToString() + "'";

            //        OracleConnection Conn = new OracleConnection(myconStr);
            //        try
            //        {
            //            Conn.Open();
            //            OracleCommand cmd = new OracleCommand(strSQL1, Conn);
            //            cmd.ExecuteNonQuery();
            //            OracleDataAdapter apt = new OracleDataAdapter(cmd);
            //            dt = new DataTable();
            //            apt.Fill(dt);
            //            row = dt.Rows[0];
            //            cmd.Dispose();
            //            Conn.Close();
            //        }
            //        catch
            //        {
            //            if (Conn.State == ConnectionState.Open)
            //                Conn.Close();
            //            return null;
            //        }
            //    }
            //    return row;
            //}
            //catch (Exception ex)
            //{
            //    writeToLog(ex, "featureStyle");
            //    return null;
            //}
            #endregion

            try
            {
                if (ftr == null)
                {
                    MessageBox.Show("����ѡ��ͼ������ԣ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return null;
                }
                bool isTemTab = false;  // ͨ���鿴�ֶ�,�����"��_ID",˵������ʱ��

                if (ftr != null)
                {
                    if (ftr.Count > 0)
                    {
                        foreach (MapInfo.Data.Column col in ftr[0].Columns)
                        {
                            String upAlias = col.Alias.ToUpper();
                            if (upAlias.IndexOf("��_ID") > -1)
                            {
                                isTemTab = true;
                                break;
                            }
                        }
                    }
                }
                DataTable dt = null;
                DataRow row = null;

                if (isTemTab)
                {
                    string strTabName = ftr[0]["����"].ToString();
                    if (strTabName.IndexOf("��Ƶ") >= 0)
                    {
                        strTabName = "��Ƶ";
                    }
                    CLC.ForSDGA.GetFromTable.GetFromName(strTabName, getfrompath);

                    string strSQL1 = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr[0]["��_ID"].ToString() + "'";
                    if (CLC.ForSDGA.GetFromTable.TableName == "��ȫ������λ")
                        strSQL1 = "select ���,��λ����,��λ����,��λ��ַ,���ܱ�������������,�ֻ�����,�����ɳ���,�����ж�,����������,'����鿴' as ��ȫ������λ�ļ�,��ע��,��עʱ��,X,Y from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + ftr[0]["��_ID"].ToString() + "'";

                    OracleConnection Conn = new OracleConnection(myconStr);
                    try
                    {
                        Conn.Open();
                        OracleCommand cmd = new OracleCommand(strSQL1, Conn);
                        cmd.ExecuteNonQuery();
                        OracleDataAdapter apt = new OracleDataAdapter(cmd);
                        dt = new DataTable();
                        apt.Fill(dt);
                        row = dt.Rows[0];
                        cmd.Dispose();
                        Conn.Close();
                    }
                    catch
                    {
                        if (Conn.State == ConnectionState.Open)
                            Conn.Close();
                        return null;
                    }
                }
                return row;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "featureStyle");
                return null;
            }
        }
    }
}
