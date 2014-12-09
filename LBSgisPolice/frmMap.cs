using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Data.OracleClient;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;

using Crownwood.DotNetMagic.Common;
using Crownwood.DotNetMagic.Docking;
using clUser;

using LBSDataGuide;

namespace LBSgisPolice
{
    public partial class frmMap : Form
    {
        private frmLayer fLayer;

        private clCar.ucCar fCar = null;                           // GPS����
        private clVideo.ucVideo fVideo = null;                     // ��Ƶ��� jie.zhang 2008.9.19
        private clZonghe.ucZonghe fZonghe = null;                  // �ۺϲ�ѯ
        private clAnjian.ucAnjian fAnjian = null;                  // ��������
        private clPopu.ucPopu fPopu = null;                        // �˿ڹ���
        private clHouse.ucHouse fHouse = null;                     // ���ݹ���
        private clZhihui.ucZhihui fZhihui = null;                  // ֱ��ָ��
        private clKaKou.ucKakou fKakou = null;                     // �ΰ����� jie.zhang 20090709
        private cl110.uc110 f110 = null;                           // 110�Ӿ�  jie.zhnag 20091230
        private clGPSPolice.UcGpsPolice fGPSp = null;              // GPS��Ա  jie.zhang 20091230
        private clGISPoliceEdit.ucGISPoliceEdit fGISEdit = null;   // ���ݱ༭ lili 20110107

        private string exportSQL;         // ��ŵ�����SQL���
        private string exePath;           // ������·��
        private string GetFromNamePath;   // ���GetFromNameConfig.ini�����ļ���·��
        private string strConn;           // �����ַ���
        CompositeStyle comStyle = null;

        private int videop = 0;                            // ��Ƶ��ض�ͨѶ�˿�
        private string[] videoConnstring = new string[6];  // ��Ƶ��������ַ���
        private string videoexepath = string.Empty;        // ��Ƶ��ض�λ��
        //private string fileIP = "";

        private string[] ConStr=new string[3] ;  // ���ݿ�������Ϣ

        private Int32 _schnum;                   // 110��Ϣ����ʽ
        private frmPro frmpro = new frmPro();    // ��ʾ���ȴ���
        private System.Windows.Forms.Label[] _majorTask;  // ��ʾ�ش��������ϸ��Ϣ���ı��ؼ�

        private System.Windows.Forms.Label[] levelArr = new System.Windows.Forms.Label[7];

        /// <summary>
        /// �����ʼ��
        /// ���༭�� ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        public frmMap()
        {
            try
            {
                try
                {
                    InitializeComponent();

                    reg110();  //ע��110�ؼ�

                    frmpro.Show();
                    frmpro.progressBar1.Value = 0;
                    frmpro.progressBar1.Maximum = 12;
                    Application.DoEvents();
                }
                catch (System.ComponentModel.LicenseException ex)
                {
                    ExToLog(ex, "frmMap");
                    MessageBox.Show("��ͼ�ؼ�δ��Ȩ���߹���,ϵͳ��������!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    frmpro.Close();
                    this.Dispose();
                    return;
                }
                catch (Exception ex)
                {
                    writelog("�ؼ���֧�֣�����ϵ������" + ex.ToString());
                }

                exePath = Application.StartupPath;     //����·��
                try
                {
                    frmpro.lblMessage.Text = "���ڳ�ʼ���������壬���Ժ򣮣���";
                    frmpro.progressBar1.Value = 1;
                    Application.DoEvents();
                    InitDocument();     //��ʼ����������
                }
                catch (Exception ex) { ExToLog(ex, "01-��ʼ����������"); }

                try
                {
                    frmpro.lblMessage.Text = "���ڳ�ʼ����ͼ�����Ժ򣮣���";
                    frmpro.progressBar1.Value = 2;
                    Application.DoEvents();
                    InitialMap();      //��ʼ����ͼ
                }
                catch (Exception ex) { ExToLog(ex, "02-��ʼ����ͼ"); }

                try
                {
                    frmpro.lblMessage.Text = "���ڶ�ȡ�����ļ������Ժ򣮣���";
                    frmpro.progressBar1.Value = 3;
                    Application.DoEvents();
                    strConn = getStrConn();  //���ݿ������ַ���
                    if (strConn == "")
                    {
                        MessageBox.Show("��ȡ�����ļ�ʱ��������,���޸������ļ�������!");
                        return;
                    }
                }
                catch (Exception ex) { ExToLog(ex, "03-���ݿ������ַ���"); }

                try
                {
                    frmpro.lblMessage.Text = "���ڴ�����ʱ����ӥ�۵ľ��ο����Ժ򣮣���";
                    frmpro.progressBar1.Value = 4;
                    Application.DoEvents();
                    CreateEagleLayer();//������ʱ����ӥ�۵ľ��ο�
                }
                catch (Exception ex) { ExToLog(ex, "04-������ʱ����ӥ��"); }

                mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(mapControl1_ViewChanged);

                try
                {
                    frmpro.lblMessage.Text = "���ڽ�����Ƶ���ͨѶ�˿ڣ����Ժ򣮣���";
                    frmpro.progressBar1.Value = 5;
                    Application.DoEvents();
                    CreateVideoSocket();//zhangjie 2008.12.17 ������Ƶ���ͨѶ�˿�
                }
                catch (Exception ex) { ExToLog(ex, "05-������Ƶ���ͨѶ�˿�"); }

                //��ʼ����������
                levelArr[0] = level1;
                levelArr[1] = level2;
                levelArr[2] = level3;
                levelArr[3] = level4;
                levelArr[4] = level5;
                levelArr[5] = level6;
                levelArr[6] = level7;
                level1.Click += new System.EventHandler(this.level1_Click);
                level2.Click += new System.EventHandler(this.level2_Click);
                level3.Click += new System.EventHandler(this.level3_Click);
                level4.Click += new System.EventHandler(this.level4_Click);
                level5.Click += new System.EventHandler(this.level5_Click);
                level6.Click += new System.EventHandler(this.level6_Click);
                level7.Click += new System.EventHandler(this.level7_Click);

                frmpro.lblMessage.Text = "���ڳ�ʼ���û���Ϣ�����Ժ򣮣���";
                frmpro.progressBar1.Value = 7;
                Application.DoEvents();
                try
                {
                    setOnline(frmLogin.string�û�����, 1);//��¼�û�����
                }
                catch (Exception ex) { ExToLog(ex, "06-��¼�û�����"); }

                frmpro.lblMessage.Text = "���ڳ�ʼ��ģ����Ϣ�����Ժ򣮣���";
                frmpro.progressBar1.Value = 8;
                Application.DoEvents();
                try
                {
                    InitializeMenuItems();  //��ģ���ʼ��
                }
                catch (Exception ex) { ExToLog(ex, "07-��ģ���ʼ��"); }

                frmpro.lblMessage.Text = "���ڳ�ʼ���û�Ȩ�ޣ����Ժ򣮣���";
                frmpro.progressBar1.Value = 9;
                Application.DoEvents();
                try
                {
                    InitPrivilege();   //����Ȩ��
                }
                catch (Exception ex) { ExToLog(ex, "08-�û�Ȩ��"); }

                frmpro.lblMessage.Text = "���ڳ�ʼ��Ĭ��ģ�飬���Ժ򣮣���";
                frmpro.progressBar1.Value = 10;
                Application.DoEvents();
                try
                {
                    SetDefaultFuncItem();  //����Ĭ��ģ��
                    //frm.Close();
                }
                catch (Exception ex) { ExToLog(ex, "09-Ĭ��ģ��"); }

                try
                {
                    frmpro.lblMessage.Text = "���ڼ������������Ժ򣮣���";
                    frmpro.progressBar1.Value = 11;
                    Application.DoEvents();

                    //add by siumo 090121
                    checkServerComputer(); //siumo 090211 ��������

                    //timeIP.Interval = 1 * 60 * 1000;

                    frmpro.progressBar1.Value = 12;
                    frmpro.lblMessage.Text = "�����ʼ�����";
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(300);
                    
                    //timeIP.Start();
                }
                catch (Exception ex) { ExToLog(ex, "10-��������"); }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "11-�����ʼ��");
            }
            finally
            {
                frmpro.Close();
            }
        }

        /// <summary>
        /// ע��netcomm.ocx�ؼ�
        /// ���༭�� ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        private void reg110()
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "regsvr32";
                p.StartInfo.Arguments = "/s " + Application.StartupPath + "\\netcomm.ocx";
                p.Start();

                writelog("ע��netcomm.ocx�ؼ��ɹ�");

                p.WaitForExit();
                p.Close();
                p.Dispose();
            }
            catch (Exception ex)
            {
                writelog("ע��netcomm.ocx�ؼ�ʧ��" + ex.ToString());
            }


            // add by jie.zhang 2010.3.1 
            //�ж�Ȩ��
            try
            {
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                string TcpConnName = "�Ӵ���";
                string RemoteHost = CLC.INIClass.IniReadValue("110", "������").Trim();
                short PortNumber = Convert.ToInt16(CLC.INIClass.IniReadValue("110", "�˿�").Trim());
                short ConType = Convert.ToInt16(CLC.INIClass.IniReadValue("110", "��������").Trim());
                writelog("TcpConnName=�Ӵ�����RemoteHost=" + RemoteHost + "��PortNumber=" + PortNumber.ToString() + "��ConType=" + ConType.ToString());
                axNetcomm1.AddConnection2(TcpConnName, RemoteHost, PortNumber, ConType);
                //axNetcomm1.AddConnection2("�Ӵ���", "sdjjt9", 20012, 2);
                writelog("�Ѿ����к��� axNetcomm1.AddConnection2(TcpConnName, RemoteHost, PortNumber, ConType);");
            }
            catch
            {
                writelog("110Socket����ʧ��");
            }
        }

        #region �������� add by siumo 090211 start
        bool DisConnection = false;
        /// <summary>
        /// ��������
        /// ���༭�� ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        private void checkServerComputer()
        {
            try
            {
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");

                if (DisConnection == false)
                {
                    bool b1 = IsWebResourceAvailable(CLC.INIClass.IniReadValue("�ļ�������", "IP1"));
                    bool b2 = IsWebResourceAvailable(CLC.INIClass.IniReadValue("�ļ�������", "IP2"));
                    if (b1 == false && b2 == false)
                    {
                        toolStripFile1.BackColor = Color.Red;
                        toolStripFile2.BackColor = Color.Red;
                        toolStripFile1.ToolTipText = "�ļ�������1��������";
                        toolStripFile2.ToolTipText = "�ļ�������2��������";
                        //DialogResult dr=  MessageBox.Show("Ŀǰ�����ļ����������������ӣ��������޷�����ϵͳ��"+
                        //    "�Ͼɵİ汾���ܵ���ϵͳ���в��������������ȷ����Ҫ����ϵͳ�������Խ��룬"+
                        //    "�����²��������������ԣ�����Ҫ�˳�������ֹ�������Ա��ϵ��",
                        //    "�û�ȷ��", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
                        //if (dr == DialogResult.Abort) {
                        //    Application.ExitThread();
                        //    this.Dispose();
                        //    Application.Exit();
                        //}
                        //else if (dr == DialogResult.Retry)
                        //{
                        //    return "Retry";
                        //}
                        //else { }
                    }
                    else
                    {
                        DisConnection = false;
                        if (b1 == true)
                        {
                            toolStripFile1.BackColor = Color.Lime;
                            toolStripFile1.ToolTipText = "�ļ�������1��ͨ";
                        }
                        else
                        {
                            toolStripFile1.BackColor = Color.Red;
                            toolStripFile1.ToolTipText = "�ļ�������1��������";
                        }

                        if (b2 == true)
                        {
                            toolStripFile2.BackColor = Color.Lime;
                            toolStripFile2.ToolTipText = "�ļ�������2��ͨ";
                        }
                        else
                        {
                            toolStripFile2.BackColor = Color.Red;
                            toolStripFile2.ToolTipText = "�ļ�������2��������";
                        }
                    }
                }

                string databaseIP1 = CLC.INIClass.IniReadValue("���ݷ�����", "IP1");
                string databaseIP2 = CLC.INIClass.IniReadValue("���ݷ�����", "IP2");
                string connStr1 = "data source = " + databaseIP1 + ";user id = system;password = system";
                string connStr2 = "data source = " + databaseIP2 + ";user id = system;password = system";
                bool d1 = false, d2 = false;
                OracleConnection conn = new OracleConnection(connStr1);
                try
                {
                    conn.Open();
                    d1 = true;
                    conn.Close();
                }
                catch (Exception ex)
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    ExToLog(ex, "12-�������ݿ������1");
                }

                conn = new OracleConnection(connStr2);
                try
                {
                    conn.Open();
                    //OracleCommand cmd = conn.CreateCommand();
                    //cmd.CommandText = "select * from LOGMNRC_DBNAME_UID_MAP";
                    //int i = cmd.ExecuteNonQuery();
                    d2 = true;
                    conn.Close();
                }
                catch (Exception ex)
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    ExToLog(ex, "13-�������ݿ������2");
                }

                if (d1 == false && d2 == false)
                {
                    DialogResult dResult = MessageBox.Show("��̨���ݿ����������������,���򽫹ر�,��ȷ�������������Ӻ���������!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    Application.Exit();
                }
                else
                {
                    if (d1 == true)
                    {
                        toolStripDB1.BackColor = Color.Lime;
                        toolStripDB1.ToolTipText = "���ݿ������1��ͨ";
                    }
                    else
                    {
                        toolStripDB1.BackColor = Color.Red;
                        toolStripDB1.ToolTipText = "���ݿ������1��������";
                    }

                    if (d2 == true)
                    {
                        toolStripDB2.BackColor = Color.Lime;
                        toolStripDB2.ToolTipText = "���ݿ������2��ͨ";
                    }
                    else
                    {
                        toolStripDB2.BackColor = Color.Red;
                        toolStripDB2.ToolTipText = "���ݿ������2��������";
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkServerComputer-��������");
            }
        }
        #endregion

        //��ʼ����������
        UserControl[] funcArr;
        /// <summary>
        /// ������ģ���ʼ��
        /// ���༭��  ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        private void InitializeMenuItems()
        {
            try
            {
                //WriteLog("�ۺ�ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fZonghe = new clZonghe.ucZonghe(mapControl1, toolStrip1, strConn, ConStr, GetFromNamePath,frmLogin.temEditDt);

                    ////////--����ͨ��������ѯ��Ҫ���Ĳ���--//////////////
                    fZonghe.toolStriplbl = this.toolStripInfo;
                    fZonghe.toolSbutton = this.toolvideo;
                    fZonghe.videop = this.videop;
                    fZonghe.videoConnstring = this.videoConnstring;
                    fZonghe.videoexepath = this.videoexepath;
                    fZonghe.KKAlSys = this.KKAlSys;
                    fZonghe.KKALUser = this.KKALUser;
                    fZonghe.KKSearchDist = this.KKSearchDist;
                    fZonghe.user = frmLogin.string�û�����;
                    fZonghe.stringϽ�� = frmLogin.stringϽ��;
                    //////////////////////////////////////////////////////

                    fZonghe.toolPro = this.toolPro;
                    fZonghe.toolProLbl = this.toolProLbl;
                    fZonghe.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {
                    ExToLog(ex,"14-��ʼ���ۺ�ģ��");
                }
                //WriteLog("�ۺ�ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("����ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fAnjian = new clAnjian.ucAnjian(mapControl1, strConn, ConStr,GetFromNamePath);
                    fAnjian.toolPro = this.toolPro;
                    fAnjian.toolProLbl = this.toolProLbl;
                    fAnjian.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "15-��ʼ������ģ��");
                }
                //WriteLog("����ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("�˿�ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fPopu = new clPopu.ucPopu(mapControl1, strConn, ConStr, GetFromNamePath);
                    fPopu.toolPro = this.toolPro;
                    fPopu.toolProLbl = this.toolProLbl;
                    fPopu.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "16-��ʼ���˿�ģ��");
                }
                //WriteLog("�˿�ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("����ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fHouse = new clHouse.ucHouse(mapControl1, strConn, ConStr,GetFromNamePath);
                    fHouse.toolPro = this.toolPro;
                    fHouse.toolProLbl = this.toolProLbl;
                    fHouse.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "17-��ʼ������ģ��");
                }
                //WriteLog("����ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("ֱ��ָ��ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fZhihui = new clZhihui.ucZhihui(mapControl1, ConStr, this.toolStripInfo, this.toolvideo, this.videop, this.videoConnstring, this.videoexepath,this.GetFromNamePath);
                    fZhihui.toolPro = this.toolPro;
                    fZhihui.toolProLbl = this.toolProLbl;
                    fZhihui.toolProSep = this.toolProSep;
                    fZhihui.panError = this.panErrorMessage;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "18-��ʼ��ֱ��ָ��ģ��");
                }
                //WriteLog("ֱ��ָ��ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("�������ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fCar = new clCar.ucCar(mapControl1, this.toolcar, ConStr, false);
                    fCar.toolPro = this.toolPro;
                    fCar.toolProLbl = this.toolProLbl;
                    fCar.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "19-��ʼ���������ģ��");
                }
                //WriteLog("�������ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("��Ƶ���ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fVideo = new clVideo.ucVideo(mapControl1, this.toolStripInfo, this.toolvideo, ConStr, this.videop, this.videoConnstring, this.videoexepath, false, true);
                    fVideo.toolPro = this.toolPro;
                    fVideo.toolProLbl = this.toolProLbl;
                    fVideo.toolProSep = this.toolProSep;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "20-��ʼ����Ƶ���ģ��");
                }
                //WriteLog("��Ƶ���ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("�ΰ�����ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fKakou = new clKaKou.ucKakou(mapControl1, ConStr, this.toolStripInfo, this.toolvideo, this.videop, this.videoConnstring, this.videoexepath, KKAlSys, KKALUser, KKSearchDist, frmLogin.stringϽ��, frmLogin.string�û�����, GetFromNamePath,true);//jie.zhang 20090709  ����ΰ�����
                    fKakou.toolPro = this.toolPro;
                    fKakou.toolProLbl = this.toolProLbl;
                    fKakou.toolProSep = this.toolProSep;
                    fKakou.panError = this.panErrorMessage;

                }
                catch (Exception ex)
                {

                    ExToLog(ex, "21-��ʼ���ΰ�����ģ��");
                }
                //WriteLog("�ΰ�����ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("110ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    f110 = new cl110.uc110(mapControl1, ConStr, this.toolStripInfo, this.toolvideo, this.videop, this.videoConnstring, this.videoexepath, this.dist,this.GetFromNamePath); //jie.zhang 20091230 ���110�Ӿ�
                    f110.toolPro = this.toolPro;
                    f110.toolProLbl = this.toolProLbl;
                    f110.toolProSep = this.toolProSep;
                    f110.split = this.splitContainer2;
                    f110.panError = this.panErrorMessage;
                }
                catch (Exception ex)
                {

                    ExToLog(ex, "22-��ʼ��110ģ��");
                }
                //WriteLog("110ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);


                //WriteLog("��Ա���ģ��", "��ʼʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
                try
                {
                    fGPSp = new clGPSPolice.UcGpsPolice(mapControl1, ConStr, this.toolStripInfo, this.toolPolice, this.videop, this.videoConnstring, this.videoexepath, this.dist);   //jie.zhang 20101027  ���GPS��Ա
                    fGPSp.toolPro = this.toolPro;             //
                    fGPSp.toolProLbl = this.toolProLbl;       //--��ʾ����������
                    fGPSp.toolProSep = this.toolProSep;       //
                    _majorTask = new System.Windows.Forms.Label[4] { this.lblName, this.lblUnits, this.lblID, this.lblNum };               // �����ش�������ϸ��Ϣ���ı��ؼ�
                    fGPSp.maTask = _majorTask;
                    fGPSp._panMajorTask = this.panMajorTask;  // ��ʾ�ش�������ϸ��Ϣ�����
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "23-��ʼ����Ա���ģ��");
                }
                //WriteLog("��Ա���ģ��", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);

                try
                {
                    string ZoomFiles = Application.StartupPath + "\\ConfigBJXX.ini";        // ���༭ģ�鴫���ַ���ڶ�ȡ��ͼ�����ű���ֵ
                    Cursor.Current = Cursors.WaitCursor;
                    CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                    DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select ��ʵ���� from �û� where USERnAME='" + frmLogin.string�û����� + "'");
                    fGISEdit = new clGISPoliceEdit.ucGISPoliceEdit(this.mapControl1,frmLogin.region1,frmLogin.region2,frmLogin.temEditDt,this.toolStrip1);
                    fGISEdit.toolEditPro = this.toolPro;
                    fGISEdit.toolEditProLbl = this.toolProLbl;
                    fGISEdit.toolEditProSep = this.toolProSep;
                    fGISEdit.ZoomFile = ZoomFiles;
                    fGISEdit.userName = table.Rows[0][0].ToString();
                    fGISEdit.panPc = this.panel7;
                    fGISEdit.label5 = this.label5;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "24-��ʼ�����ݱ༭ģ��");
                }

                funcArr = new UserControl[] { fZonghe, fAnjian, fCar, fVideo, fPopu, fHouse, fZhihui, fKakou, f110, fGPSp, fGISEdit};

                for (int i = 0; i < funcArr.Length; i++)
                {
                    splitContainer2.Panel1.Controls.Add(funcArr[i]);
                    funcArr[i].Dock = DockStyle.Fill;
                    funcArr[i].Visible = false;
                }
                //WriteLog("ģ���ʼ������", "����ʱ��" + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond);
            }
            catch(Exception ex) {
                MessageBox.Show("��ʼ�����ܲ˵�����,������!");
                ExToLog(ex, "24-��ʼ����������");
            }
        }

        /// <summary>
        /// �����û��Ĺ������ݲ�һ��,���ȡ�û��ĵ�һ��������ΪĬ�Ϲ�����
        /// ���༭�� ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        private void SetDefaultFuncItem()
        {
            try
            {                
                setUserSearchRegion(frmLogin.string�û�����);

                if (this.MenuZonghe.Enabled)
                {
                    fZonghe.Visible = true;
                    string[] strName = new string[] { "�����ѯ", "�ܱ߲�ѯ", "��ͷ��ѯ", "�߼���ѯ" };
                    FeatureLayer featureLay = null;
                    for (int i = 0; i < strName.Length; i++)
                    {
                        featureLay = (FeatureLayer)mapControl1.Map.Layers[strName[i]];
                        if (featureLay == null)
                            fZonghe.CreateTemLayer(strName[i]);
                    }

                    WriteEditLog("�ۺϲ�ѯ");
                }
                else if (this.MenuAnjian.Enabled)
                {
                    fAnjian.Visible = true;
                    WriteEditLog("��������");
                }
                else if (this.ToolGPSCar.Enabled)
                {
                    fCar.Visible = true;
                    fCar.StartTimeCar();
                    WriteEditLog("�������");
                }
                else if (this.MenuVideo.Enabled)
                {
                    fVideo.Visible = true;
                    fVideo.CreateVideoLayer();
                    WriteEditLog("��Ƶ���");
                }
                else if (this.MenuKakou.Enabled)      //jie.zhang 20090709
                {
                    this.fKakou.Visible = true;
                    fKakou.InitKK();
                    this.SetLayerEdit("KKLayer");
                    WriteEditLog("�ΰ�����");
                }
                else if (this.Menu110.Enabled)
                {
                    f110.Visible = true;
                    f110.Init110();
                    WriteEditLog("110�Ӵ���");
                }
                else if (this.ToolGPSPolice.Enabled)
                {
                    this.fGPSp.Visible = true;
                    fGPSp.InitGpsPolice();
                    WriteEditLog("GPS��Ա");
                }
                else if (this.MenuItemPop.Enabled)
                {
                    fPopu.Visible = true;
                    CreateTemLayer("layerPopu", "�˿�ϵͳ");
                    WriteEditLog("�˿�ϵͳ");
                }
                else if (this.MenuItemHouse.Enabled)
                {
                    fHouse.Visible = true;
                    CreateTemLayer("layerHouse", "����ϵͳ");
                    WriteEditLog("����ϵͳ");
                }
                else if (this.MenuCommand.Enabled)
                {
                    fZhihui.Visible = true;
                    CreateTemLayer("layerZhihui", "ֱ��ָ��");
                    this.SetLayerEdit("ֱ��ָ��");
                    fZhihui.SetDrawStyle();
                    WriteEditLog("ֱ��ָ��");
                }
                else if (this.menuDataEdit.Enabled)      // lili 20110107
                {
                    fGISEdit.Visible = true;
                    WriteEditLog("���ݱ༭");
                }
                else { }
            }
            catch (Exception ex)
            {
                ExToLog(ex,"25-����Ĭ��ģ��ʱ��������");
            }
        }

        /// <summary>
        /// ���ص�ͼ����ӵ�ͼ�¼�
        /// ���༭��  ����
        /// ���༭ʱ��  2011-1-24
        /// </summary>
        private void InitialMap()
        {
            MapInfo.Tools.MapTool ptMapTool;
            try
            {
                this.mapControl1.Map.Load(MapLoader.CreateFromFile(exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\Data\\Shunde1.mws"));
                this.mapOverview.Map.Load(MapLoader.CreateFromFile(exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\Data\\ShundeOverview.mws"));
                IMapLayer mapLayer = mapControl1.Map.Layers["�ε�·"];
                int ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                mapControl1.Map.Layers.Move(ilayer, 3);
                mapLayer = mapControl1.Map.Layers["����·"];
                ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                mapControl1.Map.Layers.Move(ilayer, 3);
                this.mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["������"]);

                //����������ͼ���¼�
                this.mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);

                //����Զ����������
                ptMapTool = new CustomPolylineMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), mapControl1.Tools, mapControl1.Tools.MouseToolProperties, mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("DistanceTool", ptMapTool);

                //����Զ���������
                ptMapTool = new CustomPolygonMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), mapControl1.Tools, mapControl1.Tools.MouseToolProperties, mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("AreaTool", ptMapTool);

                //����Զ�����ι켣����
                this.mapControl1.Tools.Add("drawRectTool", new CustomRectangleMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), mapControl1.Tools, mapControl1.Tools.MouseToolProperties, mapControl1.Tools.MapToolProperties));

                //����Զ�����Ϣ��ѯ����
                ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("toolInfo", ptMapTool);

                //����Զ�����Ƶ�鿴����
                ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("VideoTool", ptMapTool);

                //����Զ����ܱ߲鿴����
                ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("VCTool", ptMapTool);

                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(9, System.Drawing.Color.Gray, System.Drawing.Color.Green, true);
                comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "26-InitialMap");
            }
        }

        /// <summary>
        /// �����û�Ȩ����������ģ�������
        /// ��������  ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void InitPrivilege()
        {   //"�ۺϲ�ѯ","��������","�������","��Ƶ���","�ΰ�����","�˿ڹ���","���ݹ���","ֱ��ָ��","GPS��Ա","�������ݱ༭","ҵ�����ݱ༭","��Ƶ�༭","Ȩ�޹���","�ɵ���" 
            try
            {
                if (frmLogin.temDt.Select("�ۺϲ�ѯ=1").Length > 0)
                {
                    this.MenuZonghe.Enabled = true;
                }
                else
                {
                    this.MenuZonghe.Enabled = false;
                }
                if (frmLogin.temDt.Select("��������=1").Length > 0)
                {
                    this.MenuAnjian.Enabled = true;
                }
                else
                {
                    this.MenuAnjian.Enabled = false;
                }
                if (frmLogin.temDt.Select("�������=1").Length > 0)
                {
                    this.ToolGPSCar.Enabled = true;
                }
                else
                {
                    this.ToolGPSCar.Enabled = false;
                }
                if (frmLogin.temDt.Select("��Ƶ���=1").Length > 0)
                {
                    this.MenuVideo.Enabled = true;
                }
                else
                {
                    this.MenuVideo.Enabled = false;
                }
                if (frmLogin.temDt.Select("�˿ڹ���=1").Length > 0)
                {
                    this.MenuItemPop.Enabled = true;
                }
                else
                {
                    this.MenuItemPop.Enabled = false;
                }
                if (frmLogin.temDt.Select("���ݹ���=1").Length > 0)
                {
                    this.MenuItemHouse.Enabled = true;
                }
                else
                {
                    this.MenuItemHouse.Enabled = false;
                }
                if (frmLogin.temDt.Select("���ݹ���=1").Length > 0 || frmLogin.temDt.Select("�˿ڹ���=1").Length > 0)
                {
                    this.MenuPopulation.Enabled = true;
                }
                else 
                {
                    this.MenuPopulation.Enabled = false;
                }
                if (frmLogin.temDt.Select("�ΰ�����=1").Length > 0)
                {
                    this.MenuKakou.Enabled = true;
                }                    
                else
                {
                    this.MenuKakou.Enabled = false;
                }
                if (frmLogin.temDt.Select("ֱ��ָ��=1").Length > 0)
                {
                    this.MenuCommand.Enabled = true;
                }
                else
                {
                    this.MenuCommand.Enabled = false;
                }
                if (frmLogin.temDt.Select("GPS��Ա=1").Length > 0)
                {
                    this.ToolGPSPolice.Enabled = true;
                }
                else
                {
                    this.ToolGPSPolice.Enabled = false;
                }
                if (frmLogin.temDt.Select("GPS��Ա=1").Length > 0 || frmLogin.temDt.Select("�������=1").Length > 0)
                {
                    this.MenuCar1.Enabled = true;
                }
                else
                {
                    this.MenuCar1.Enabled = false;
                }
                if (frmLogin.temDt.Select("llo�Ӿ�=1").Length > 0)
                {
                    this.Menu110.Enabled = true;
                }
                else
                {
                    this.Menu110.Enabled = false;
                }
                //��ʼ�����ݱ༭ģ�飬���������ݿɱ༭������ҵ�����ݿɱ༭��������Ƶ�༭����ֻҪ������һ������Ϊ�棬�����ݱ༭�˵�����
                if (frmLogin.temDt.Select("�������ݱ༭=1").Length > 0||frmLogin.temDt.Select("ҵ�����ݱ༭=1").Length>0||frmLogin.temDt.Select("��Ƶ�༭=1").Length>0)
                {
                    this.menuDataEdit.Enabled = true;
                }
                else
                {
                    this.menuDataEdit.Enabled = false;
                }
                if (frmLogin.temDt.Select("Ȩ�޹���=1").Length > 0)
                {
                    this.MenuAuthorize.Enabled = true;
                }
                else
                {
                    this.MenuAuthorize.Enabled = false;
                }
                if (frmLogin.temDt.Select("�ɵ���=1").Length > 0)               //add by fisher in 10-01-04
                {
                    this.toolDateOut.Enabled = true;
                }
                else
                {
                    this.toolDateOut.Enabled = false;
                }

                toolStripUser.Text = @"�û���" + frmLogin.string�û�����;

                if (frmLogin.string�û����� == "sdga" || frmLogin.string�û����� == "admin")
                {
                    test110.Visible = true;
                }
                else
                {
                    test110.Visible = false;
                }
            }
            catch (Exception ex) {
                ExToLog(ex, "27-InitPrivilege");
            }
        }

        private Double dist = 0;

        //��ȡ�����ļ����������ݿ������ַ���
        string datasource;   // ����Դ
        string userid;       // �û���
        string password;     // ����

        string KKAlSys = string.Empty; 
        string KKALUser = string.Empty;
        double KKSearchDist = 0;
       
        /// <summary>
        /// �������ļ��ж�ȡ���ݿ����ӡ���Ƶ�����������Ϣ
        /// ��������  ����
        /// ������ʱ��  2011-1-24
        /// </summary>
        /// <returns>�����ַ���</returns>
        private string getStrConn()
        {
            try
            {
                GetFromNamePath = exePath + "\\GetFromNameConfig.ini";
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");
               
                videop = Convert.ToInt32(CLC.INIClass.IniReadValue("��Ƶ", "�˿�"));
                //this.videoexepath = CLC.INIClass.IniReadValue("��Ƶ", "�ͻ���");
                dist = Convert.ToDouble(CLC.INIClass.IniReadValue("��Ƶ", "����"));
                videoexepath = exePath.Remove(exePath.LastIndexOf("\\")) + "\\Carrier\\surveillance1.exe";

                // ��ȡ�ΰ�������Ϣ  jie.zhang 20090709
                KKAlSys = CLC.INIClass.IniReadValue("�ΰ�����", "ϵͳ����");
                KKALUser = CLC.INIClass.IniReadValue("�ΰ�����", "��������");
                KKSearchDist = Convert.ToDouble(CLC.INIClass.IniReadValue("�ΰ�����", "��ѯ�뾶"));
               
                ReadXML();    //��ȡ��Ƶ��ص�������Ϣ
                get110Con();  //��ȡ110��Ϣ����ģʽ

                ConStr[0] = datasource;
                ConStr[1] = userid;
                ConStr[2] = password;
               
 
                string connString = "data source = " + datasource + ";user id = " + userid + ";password = " + password;
                return connString;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "28-getStrConn");
                return "";
            }
        }

        /// <summary>
        /// ��ConfigBJXX.ini�ж�ȡ110ģʽ
        /// ���༭�� ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        private void get110Con()
        {
            try
            {
                string Config110Path = exePath + "\\ConfigBJXX.ini";
                CLC.INIClass.IniPathSet(Config110Path);
                _schnum = Convert.ToInt32(CLC.INIClass.IniReadValue("110", "ģʽ"));
            }
            catch (Exception ex)
            {
                ExToLog(ex, "29-get110Con");
            }
        }

        /// <summary>
        /// ��ȡEMS�����ļ��е�CMS����
        /// ���༭�� ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        private void ReadXML()
        {
            try
            {
                if (this.videoexepath != "")
                {
                    this.videoConnstring[0] = CLC.INIClass.IniReadValue("��Ƶ��", "�ļ���");
                    this.videoConnstring[1] = CLC.INIClass.IniReadValue("��Ƶ��", "ip");
                    this.videoConnstring[2] = CLC.INIClass.IniReadValue("��Ƶ��", "�˿�");
                    this.videoConnstring[3] = CLC.INIClass.IniReadValue("��Ƶ��", "�û���");
                    this.videoConnstring[4] = CLC.INIClass.IniReadValue("��Ƶ��", "����");
                    this.videoConnstring[5] = Convert.ToString(15);
                }
            }
            catch (Exception ex)
            {                
                ExToLog(ex, "30-ReadXML");
            }
        }

        /// <summary>
        /// ������ʱ����ӥ�۵ľ��ο�
        /// ���༭�� ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        private void CreateEagleLayer()
        {
            try
            {
                //   disable   the   mouse   wheel-zoom   funtionality   of   eagle   eye   map 
                mapOverview.MouseWheelSupport = new MouseWheelSupport(MouseWheelBehavior.None, 10, 5);

                //   create   a   temp   layer   as   the   rectangle   holder 
                TableInfoMemTable ti = new TableInfoMemTable("EagleEyeTemp");
                ti.Temporary = true;

                //   add   columns   
                Column column;
                column = new GeometryColumn(mapOverview.Map.GetDisplayCoordSys());
                column.Alias = "MI_Geometry";
                column.DataType = MIDbType.FeatureGeometry;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "MI_Style";
                column.DataType = MIDbType.Style;
                ti.Columns.Add(column);

                //   create   table   and   feature   layer 
                Table table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);

                FeatureLayer eagleEye = new FeatureLayer(table, "EagleEye", "MyEagleEye");
                //mapOverview.Map.Layers.Insert(0, eagleEye);
                mapOverview.Map.Layers.Insert(0, (IMapLayer)eagleEye);
            }
            catch(Exception ex) {

                ExToLog(ex, "31-������ʱ����ӥ�۵ľ��ο�");
            }
        }

        //+��ͼ��Ұ�����仯ʱ
        //-ӥ��ͼ�ϵ�ͼʾ��
        //-ͼ������е�ͼ��
        int iMapLevel = 1;
        /// <summary>
        /// ��ͼ��Ұ�����仯ʱ ӥ��ͼ�ϵ�ͼʾ�� ͼ������е�ͼ��
        /// ��������  ����
        /// ������ʱ��  2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapControl1_ViewChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false) return;

                //   create   temp   table   and   temp   feature 
                Table tabTemp = Session.Current.Catalog.GetTable("EagleEyeTemp");

                (tabTemp as ITableFeatureCollection).Clear();

                #region   Draw   the   rectangle
                //���þ��ε���ʽ
                DRect rect = mapControl1.Map.Bounds;
                FeatureGeometry feageo = new MapInfo.Geometry.Rectangle(mapControl1.Map.GetDisplayCoordSys(), rect);

                //�����β��뵽ͼ����
                Feature fea = new Feature(feageo, comStyle);

                tabTemp.InsertFeature(fea);
                #endregion

                iMapLevel = setToolslevel();   //��ʾ����

                //�����Ӱ����ʾӰ��
                if (toolImageOrMap.Text == "��ͼ")
                {
                    closeOtherLevelImg(iMapLevel);//�ȹر����������Ӱ��

                    CalRowColAndDisImg(iMapLevel);
                }

                panelMaptools.Refresh();

                labelName.Visible = false;
            }
            catch(Exception ex) {
                ExToLog(ex, "32-��ͼ��Ұ�����仯ʱ");
            }
        }

        #region �鿴Ӱ��
        /// <summary>
        /// �ر�ĳһ��Ӱ��
        /// ���༭��  ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        /// <param name="iLevel">�ȼ���</param>
        private void closeOtherLevelImg(int iLevel)
        {
            try
            {
                GroupLayer gLayer = mapControl1.Map.Layers["Ӱ��"] as GroupLayer;
                if (gLayer == null) return;
                int iCount = gLayer.Count;
                for (int i = 0; i < iCount; i++)
                {
                    IMapLayer layer = gLayer[0];   //�����ҵ�һ��ͼ��(�Ƴ�һ����,������油Ϊ��һ��);
                    string alies = layer.Alias;
                    if (Convert.ToInt16(alies.Substring(1, 1)) != iLevel)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(alies);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex,"33-closeOtherLevelImg");
            }
        }

        /// <summary>
        /// ���ݵȼ���ʾӰ��ͼ
        /// ��������  ����
        /// ���༭ʱ�� 2011-1-24
        /// </summary>
        /// <param name="iLevel">�ȼ���</param>
        private void CalRowColAndDisImg(int iLevel)
        {
            try
            {
                double dScale = 0;
                int minRow = 0, minCol = 0;
                int maxRow = 0, maxCol = 0;
                switch (iLevel)
                {
                    case 1:
                        dScale = 200000;
                        minRow = 1;
                        minCol = 1;
                        maxRow = 2;
                        maxCol = 2;
                        break;
                    case 2:
                        dScale = 100000;
                        minRow = 1;
                        minCol = 1;
                        maxRow = 4;
                        maxCol = 4;
                        break;
                    case 3:
                        dScale = 50000;
                        minRow = 2;
                        minCol = 2;
                        maxRow = 7;
                        maxCol = 8;
                        break;
                    case 4:
                        dScale = 20000;
                        minRow = 3;
                        minCol = 3;
                        maxRow = 17;
                        maxCol = 19;
                        break;
                    case 5:
                        dScale = 10000;
                        minRow = 6;
                        minCol = 6;
                        maxRow = 34;
                        maxCol = 37;
                        break;
                    case 6:
                        dScale = 5000;
                        minRow = 12;
                        minCol = 11;
                        maxRow = 67;
                        maxCol = 74;
                        break;
                    case 7:
                        dScale = 2000;
                        minRow = 29;
                        minCol = 27;
                        maxRow = 166;
                        maxCol = 184;
                        break;
                }
                double gridDis = 6.0914200613 * Math.Pow(10, -7) * dScale * 2;  //���������񳤶�
                int beginRow = 0, endRow = 0;
                int beginCol = 0, endCol = 0;
                //�������к�
                //��ʼ�к�
                int dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y2) / gridDis);
                if (dRow > maxRow) return;    //�������ʼ�кűȱ���ͼƬ����кŻ���˵���˷�Χ��ͼ

                if (dRow < minRow)
                {
                    beginRow = minRow;
                }
                else
                {
                    beginRow = dRow;
                }

                //��ֹ�к�
                dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y1) / gridDis) + 1;
                if (dRow < minRow) return; //�������ֹ�кűȱ���ͼƬ��С�кŻ�С��˵���˷�Χ��ͼ
                if (dRow > maxRow)
                {
                    endRow = maxRow;
                }
                else
                {
                    endRow = dRow;
                }

                int dCol = Convert.ToInt32((mapControl1.Map.Bounds.x1 - 112.94) / gridDis);
                if (dCol > maxCol) return; //�������ʼ�кűȱ���ͼƬ����кŻ���˵���˷�Χ��ͼ
                if (dCol < minCol)
                {
                    beginCol = minCol;
                }
                else
                {
                    beginCol = dCol;
                }
                //������ֹ�к�
                dCol = Convert.ToInt32((mapControl1.Map.Bounds.x2 - 112.94) / gridDis) + 1;
                if (dCol < minCol) return; //�������ֹ�кűȱ���ͼƬ��С�кŻ���˵���˷�Χ��ͼ
                if (dCol > maxCol)
                {
                    endCol = maxCol;
                }
                else
                {
                    endCol = dCol;
                }

                DisImg(iLevel, beginRow, endRow, beginCol, endCol);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "34-CalRowColAndDisImg");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iLevel"></param>
        /// <param name="beginRow"></param>
        /// <param name="endRow"></param>
        /// <param name="beginCol"></param>
        /// <param name="endCol"></param>
        private void DisImg(int iLevel, int beginRow, int endRow, int beginCol, int endCol)
        {
            try
            {
                string tabName = "";
                for (int i = beginRow; i <= endRow; i++)
                {
                    for (int j = beginCol; j <= endCol; j++)
                    {
                        tabName = iLevel.ToString() + "_" + i.ToString() + "_" + j.ToString();
                        openTable(iLevel, tabName);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "34-CalRowColAndDisImg");
            }
        }

        /// <summary>
        /// ���ݲ�ͬ�ȼ��ҵ����ͼ����ʾ
        /// �������� ����
        /// ������ʱ��  2011-1-24
        /// </summary>
        /// <param name="iLevel">�ȼ���</param>
        /// <param name="tableName">Ӱ��ͼ����</param>
        private void openTable(int iLevel, string tableName)
        {
            //���ж���û�м���
            try
            {
                GroupLayer groupLayer = mapControl1.Map.Layers["Ӱ��"] as GroupLayer;

                if (groupLayer["_" + tableName] == null)
                {
                    //���ж��ļ����д岻���ڣ����ھʹ�

                    string imgPath = exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\ImgData\\" + iLevel.ToString() + "\\" + tableName + ".tab";
                    if (File.Exists(imgPath))
                    {
                        Table tab = MapInfo.Engine.Session.Current.Catalog.OpenTable(imgPath);

                        MapInfo.Mapping.FeatureLayer fl = new MapInfo.Mapping.FeatureLayer(tab, "_" + tableName);

                        groupLayer.Add(fl);
                    }
                }
            }
            catch(Exception ex)
            {
               //MessageBox.Show("��ͼ�ļ�����,����ϵ���������Ա!");
                ExToLog(ex, "35-openTable");
                return;
            }
        }

        #endregion

        /// <summary>
        /// ��ӥ��ͼ�ϵ������λ��ͼ��Ұ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapOverview_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                //��������Ϊ����ͼ��ӥ��ͼ������
                MapInfo.Geometry.DPoint dP;
                mapOverview.Map.DisplayTransform.FromDisplay(new System.Drawing.Point(e.X, e.Y), out dP);
                mapControl1.Map.Center = dP;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "36-��λ��ͼ��Ұ");
            }
        }
        
        private Table tabCurve = null;
        /// <summary>
        /// tool���ߵ���¼�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Name)
                {
                    case "toolSel":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolZoomIn":
                        mapControl1.Tools.LeftButtonTool = "ZoomIn";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolZoomOut":
                        mapControl1.Tools.LeftButtonTool = "ZoomOut";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolPan":
                        mapControl1.Tools.LeftButtonTool = "Pan";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolFullExtent":
                        mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["������"]);
                        break;
                    case "toolDistance":
                        UncheckedTool();
                        mapControl1.Tools.LeftButtonTool = "DistanceTool";
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolArea":
                        mapControl1.Tools.LeftButtonTool = "AreaTool";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolInfo":
                        mapControl1.Tools.LeftButtonTool = "toolInfo";
                        UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolLocation":
                        winLoc.frmLocDia fLoc = new winLoc.frmLocDia(mapControl1);
                        fLoc.Show();
                        break;
                    case "toolClear":
                        clearAllTemp();
                        return;
                    //break;
                    case "toolvideoclient":
                        break;
                    case "toolLayers":
                        fLayer = new frmLayer();
                        fLayer.InitialLayers(mapControl1.Map);
                        fLayer.ShowDialog();
                        break;
                    case "toolImageOrMap":
                        try
                        {
                            if (e.ClickedItem.Text == "Ӱ��")
                            {

                                closeOtherLevelImg(iMapLevel);//�ȹر����������Ӱ��

                                CalRowColAndDisImg(iMapLevel);
                                mapControl1.Map.Layers["������"].Enabled = false;
                                mapControl1.Map.Layers["��԰"].Enabled = false;
                                mapControl1.Map.Layers["������"].Enabled = false;
                                mapControl1.Map.Layers["ɽ��"].Enabled = false;
                                mapControl1.Map.Layers["ˮϵ"].Enabled = false;
                                mapControl1.Map.Layers["����·"].Enabled = false;
                                mapControl1.Map.Layers["�ε�·"].Enabled = false;
                                mapControl1.Map.Layers["Ӱ��"].Enabled = true;
                                e.ClickedItem.Text = "��ͼ";

                            }
                            else
                            {
                                mapControl1.Map.Layers["������"].Enabled = true;
                                mapControl1.Map.Layers["��԰"].Enabled = true;
                                mapControl1.Map.Layers["������"].Enabled = true;
                                mapControl1.Map.Layers["ɽ��"].Enabled = true;
                                mapControl1.Map.Layers["ˮϵ"].Enabled = true;
                                mapControl1.Map.Layers["����·"].Enabled = true;
                                mapControl1.Map.Layers["�ε�·"].Enabled = true;
                                mapControl1.Map.Layers["Ӱ��"].Enabled = false;
                                e.ClickedItem.Text = "Ӱ��";
                                //mapControl1.Map.Center = mapControl1.Map.Center;
                            }
                            //mapControl1.Refresh();
                            panelMaptools.Refresh();
                        }
                        catch (Exception ex)
                        {
                            ExToLog(ex, "toolStrip1_ItemClicked");
                        }
                        break;
                    case "toolEditPolice":
                        this.UncheckedTool();
                        break;
                    case "toolRefresh":
                        this.UncheckedTool();
                        break;
                }

                //����ǲ���������ߣ������ʱͼ���Ź켣
                if (mapControl1.Tools.LeftButtonTool == "AreaTool" || mapControl1.Tools.LeftButtonTool == "DistanceTool")
                {
                    try
                    {
                        if (MapInfo.Engine.Session.Current.Catalog.GetTable("�������켣") == null)
                        {
                            //   create   a   temp   layer   as   the   rectangle   holder 
                            TableInfoMemTable ti = new TableInfoMemTable("�������켣");
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

                            Table table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                            FeatureLayer temLayer = new FeatureLayer(table);

                            mapControl1.Map.Layers.Insert(0, temLayer);
                            tabCurve = temLayer.Table;
                        }
                    }
                    catch (Exception ex) { ExToLog(ex, "����������߹켣ͼ��"); }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "��������ť");
            }
        }

        /// <summary>
        /// �����ͼҪ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void clearAllTemp()
        {
            try
            {
                //����ǵ�ǰ�����ֻ�������ʱͼ���е�Ҫ��
                //�����Ƴ����ݲ�
                if (fZonghe.Visible)
                {
                    fZonghe.clearTem();
                }

                if (fAnjian.Visible)
                {
                    fAnjian.clearTem();
                }

                if (fPopu.Visible)
                {
                    fPopu.clearTem();
                }

                if (fHouse.Visible)
                {
                    fHouse.clearTem();
                }

                if (fZhihui.Visible)
                {
                    fZhihui.clearTem();
                    fCar.ClearCarTemp();
                    fVideo.ClearVideoTemp();
                }

                if (fKakou.Visible)
                {
                    fKakou.ClearKaKouTemp();
                    fCar.ClearCarTemp();
                    fVideo.ClearVideoTemp();
                }

                if (fCar.Visible)
                {
                    fCar.ClearCarTemp();
                }

                if (fVideo.Visible)
                {
                    fVideo.ClearVideoTemp();
                }

                if (f110.Visible)
                {
                    f110.cleartemp();
                }

                if (fGPSp.Visible)
                {
                    fGPSp.Cleartemp();
                }

                if (fGISEdit.Visible)
                {
                    fGISEdit.ClearLayer();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "�����ʱͼ��");
            }

            //�������������
            try
            {
                if (tabCurve != null) {
                    (tabCurve as ITableFeatureCollection).Clear();
                    tabCurve.Pack(PackType.All);
                }
            }
            catch { }

            //�����λ��ӵ�ͼ��
            try {
                if (MapInfo.Engine.Session.Current.Catalog.GetTable("CodingPoint") != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CodingPoint");
                    mapControl1.Map.Layers.Remove("geoCodeLabel");
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("geoCodeLabel");
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("Location_temp");
                }
            }
            catch { }
        }

        /// <summary>
        /// ����������ϵĹ���ʱ���Թ��߰�ť��������
        /// ѡ�е�checked������unchecked���Ա���ȷ��ǰ��ѡ����
        /// ���ڰ�ť���飬iFrom��ʾ�����Index��iEnd��ʾĩIndex
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void UncheckedTool()
        {
            try
            {
                for (int i = 0; i < toolStrip1.Items.Count; i++)
                {
                    ToolStripButton tsButton = toolStrip1.Items[i] as ToolStripButton;
                    if (tsButton != null)
                    {
                        if (tsButton.Checked == true)
                        {
                            tsButton.Checked = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "��ť�任");
            }
        }

        #region �Զ��幤��ʵ�ִ���
        private DataTable dt = new DataTable("Data");
        private bool mouseMoveTag = true;
        private double dblDistance;
        private double currentdblDistance;
        private MapInfo.Geometry.DPoint dptStart;
        //private MapInfo.Geometry.DPoint dptEnd;
        private System.Collections.ArrayList arrlstPoints = new ArrayList();
        private MapInfo.Geometry.DPoint dptFirstPoint;

        private FrmInfo frmMessage = new FrmInfo();
        /// <summary>
        /// ��ͼ����¼�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                switch (e.ToolName)
                {
                    case "DistanceTool":
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                this.mouseMoveTag = true;
                                dblDistance = 0;
                                dptStart = e.MapCoordinate;
                                arrlstPoints.Clear();
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.InProgress:
                                dblDistance += MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, e.MapCoordinate);
                                dptStart = e.MapCoordinate;
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.End:
                                this.mouseMoveTag = false;
                                dblDistance += MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, e.MapCoordinate);

                                int intCount = arrlstPoints.Count;
                                MapInfo.Geometry.DPoint[] dptPoints = new DPoint[intCount];
                                for (int i = 0; i < intCount; i++)
                                {
                                    dptPoints[i] = (MapInfo.Geometry.DPoint)arrlstPoints[i];
                                }
                                MultiCurve mulCur = new MapInfo.Geometry.MultiCurve(mapControl1.Map.GetDisplayCoordSys(), CurveSegmentType.Linear, dptPoints);

                                Feature ft = new Feature(mulCur, new SimpleLineStyle(new LineWidth(2, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red));
                                tabCurve.InsertFeature(ft);
                                mapControl1.Map.Invalidate(true);
                                break;
                            default:
                                break;
                        }
                        this.dockingManager.ShowContent(cDistance);//�������ӵ�����Ϣ��          
                        break;

                    case "AreaTool":
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                arrlstPoints.Clear();
                                dptFirstPoint = e.MapCoordinate;
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.InProgress:
                                arrlstPoints.Add(e.MapCoordinate);
                                break;
                            case MapInfo.Tools.ToolStatus.End:
                                //����һ���պϻ�
                                //arrlstPoints.Add(e.MapCoordinate);
                                arrlstPoints.Add(dptFirstPoint);
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
                                MapInfo.Geometry.Polygon objPolygon = new Polygon(objCoordSys, MapInfo.Geometry.CurveSegmentType.Linear, dptPoints);
                                if (objPolygon == null)
                                {
                                    return;
                                }
                                this.lblArea.Text = "�����Ϊ:" + string.Format("{0:F3}", objPolygon.Area(costAreaUnit)) + " ƽ������";
                                this.dockingManager.ShowContent(this.cArea);

                                MultiPolygon mulPolygon = new MultiPolygon(objCoordSys, CurveSegmentType.Linear, dptPoints);
                                //MultiCurve mulCur = new MapInfo.Geometry.MultiCurve(mapControl1.Map.GetDisplayCoordSys(), CurveSegmentType.Linear, dptPoints);
                                Feature ft = ft = new Feature(mulPolygon, comStyle);
                                tabCurve.InsertFeature(ft);
                                this.mapControl1.Map.Invalidate(true);
                                break;
                            default:
                                break;
                        }
                        break;

                    case "toolInfo":
                        //setTableFalg();
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                Distance dictance = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(this.mapControl1.Map, 5);
                                IMapLayerFilter layerFilter = MapLayerFilterFactory.FilterByLayerType(LayerType.Normal);

                                ITableEnumerator tableEnum = mapControl1.Map.Layers.GetTableEnumerator(layerFilter);
                                MultiResultSetFeatureCollection mrfc;
                                try
                                {
                                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(e.MapCoordinate, mapControl1.Map.GetDisplayCoordSys(), dictance);
                                    si.QueryDefinition.Columns = null;
                                    mrfc = Session.Current.Catalog.Search(tableEnum, si);
                                }
                                catch
                                {
                                    MessageBox.Show("ERR");
                                    return;
                                }

                                if (mrfc.Count > 0)
                                {
                                    if (this.frmMessage.Visible == false)
                                    {
                                        this.frmMessage = new FrmInfo();
                                        this.frmMessage.SetDesktopLocation(-30, -30);
                                        this.frmMessage.Show();
                                        this.frmMessage.Visible = false;
                                    }
                                    MapInfo.Data.Feature ftr = mrfc[0][0];
                                    this.frmMessage.getFromNamePath = GetFromNamePath;
                                    this.frmMessage.mapControl = mapControl1;
                                    if (mapControl1.Map.Layers[0].ToString() != "��������")
                                        this.frmMessage.LayerName = mapControl1.Map.Layers[0].ToString();
                                    else
                                        this.frmMessage.LayerName = mapControl1.Map.Layers[1].ToString();
                                    frmMessage.setInfo(ftr, ConStr);  //������Ϣ
                                    try
                                    {
                                        GetFromName getName = new GetFromName(ftr["����"].ToString());
                                        writeEditLog(ftr["����"].ToString(), getName.ObjID +"="+ ftr["��_ID"].ToString(), "�鿴����");
                                    }
                                    catch {
                                        GetFromName getName = new GetFromName(ftr.Table.Alias.Remove(ftr.Table.Alias.IndexOf("_")));
                                        if (getName.ObjID != null)
                                        {
                                            writeEditLog(getName.TableName, getName.ObjID + "=" + ftr[getName.ObjID].ToString(), "�鿴����");
                                        }
                                    }
                                }
                                else
                                {
                                    this.Activate();
                                }                                
                                break;
                            default:
                                break;
                        }
                        break;
                    //��������������û��Զ���Tool
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                ExToLog(Ex, "�Զ��幤��");
            }
        }

        /// <summary>
        /// ����ģ������ͼ���Ƿ����ѡ��
        /// �������� ����
        /// ������ʱ�� 2011-2-28
        /// </summary>
        private void setTableFalg()
        {
            try 
            {
                if (this.fZonghe.Visible || this.fGISEdit.Visible)  // �ۺ�ģ�鼰�༭ģ���е�ͼ���ѡ
                {
                    setSelectLayer(true);
                }
                else
                {
                    setSelectLayer(false);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSelectLayer");
            }
        }

        /// <summary>
        /// ����ͼ���Ƿ����ѡ��
        /// �������� ����
        /// ������ʱ�� 2011-2-28
        /// </summary>
        /// <param name="falg">����ֵ��true-ͼ���ѡ�� false-ͼ�㲻��ѡ��</param>
        private void setSelectLayer(bool falg)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["��Ϣ��"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["��·"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["����·"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["�ε�·"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["��������"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["��԰"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["������"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["ˮϵ"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["������"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["ɽ��"], falg);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers["Ӱ��"], falg);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSelectLayer");
            }
 
        }

        #endregion
        /// <summary>
        /// ����ڵ�ͼ���ƶ�ʱ����״̬����ʾ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                double totalDistance = dblDistance;
                System.Drawing.Point sPoint = new System.Drawing.Point(e.X, e.Y);
                DPoint dPoint;
                mapControl1.Map.DisplayTransform.FromDisplay(sPoint, out dPoint);
                this.toolStripcbScale.Text =mapControl1.Map.Scale.ToString("#,###");
                this.toolStripCoord.Text = "X:" + dPoint.x.ToString("#.####") + ", Y:" + dPoint.y.ToString("#.####") ;
                currentdblDistance = MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, dPoint);
                totalDistance += MapInfo.Geometry.CoordSys.Distance(MapInfo.Geometry.DistanceType.Spherical, DistanceUnit.Meter, mapControl1.Map.GetDisplayCoordSys(), dptStart, dPoint);

                if (this.mouseMoveTag == true)
                {
                    if (dblDistance > 10000)
                    {
                        this.lblDistance.Text = "���ξ���: " + string.Format("{0:F3}", currentdblDistance / 1000) + "����" + "\n";
                        this.lblDistance.Text += "  ��   : " + string.Format("{0:F3}", totalDistance / 1000) + "����";
                    }
                    else
                    {
                        this.lblDistance.Text = "���ξ���: " + string.Format("{0:F2}", currentdblDistance) + " ��" + "\n";
                        this.lblDistance.Text += "  ��   : " + string.Format("{0:F2}", dblDistance) + " ��";
                    }
                }

                //GetDisFtr(dPoint,e);
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "״̬������ֵ");
            }
        }       
              
        /// <summary>
        /// ���õ�ǰͼ��ɱ༭
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="layername"></param>
        private void SetLayerEdit(string layername)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layername], true);
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layername], true);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "���õ�ǰͼ��ɱ༭"); 
            }
        }


        /// <summary>
        /// ����ͼ��Ұ�仯ʱ�����ݱ�����ָ����ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <returns></returns>
        private int setToolslevel()
        {
            int iLevel = 0;

            try
            {
                foreach (System.Windows.Forms.Label lev in levelArr)
                {
                    if (lev.BackColor == Color.Red)
                    {
                        lev.BackColor = Color.Transparent;
                    }
                }

                if (Math.Round(mapControl1.Map.Scale) >= 200000)
                {
                    level1.BackColor = Color.Red;
                    iLevel = 1;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 200000 && Math.Round(mapControl1.Map.Scale) >= 100000)
                {
                    level2.BackColor = Color.Red;
                    iLevel = 2;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 100000 && Math.Round(mapControl1.Map.Scale) >= 50000)
                {
                    level3.BackColor = Color.Red;
                    iLevel = 3;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 50000 && Math.Round(mapControl1.Map.Scale) >= 20000)
                {
                    level4.BackColor = Color.Red;
                    iLevel = 4;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 20000 && Math.Round(mapControl1.Map.Scale) >= 10000)
                {
                    level5.BackColor = Color.Red;
                    iLevel = 5;
                }
                else if (Math.Round(mapControl1.Map.Scale) < 10000 && Math.Round(mapControl1.Map.Scale) >= 5000)
                {
                    level6.BackColor = Color.Red;
                    iLevel = 6;
                }
                else
                {
                    level7.BackColor = Color.Red;
                    iLevel = 7;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "���ݱ�����ָ����ͼ����");
            }
            return iLevel;
        }

        #region ������ͼ�������¼�����
        /// <summary>
        /// �����������ʵ�ֵ�ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void level1_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 200000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level1_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void level2_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 100000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level2_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void level3_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 50000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level3_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void level4_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 20000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level4_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void level5_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 10000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level5_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void level6_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 5000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level6_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void level7_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.Scale = 2000;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "level7_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ�Ŵ�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void bZoomOut_Click(object sender, EventArgs e)
        {
            try
            {
                //mapControl1.Map.Scale = mapControl1.Map.Scale * 2;
                switch (iMapLevel)
                {
                    case 1:
                        mapControl1.Map.Scale = mapControl1.Map.Scale * 2;
                        break;
                    case 2:
                        mapControl1.Map.Scale = 200000;
                        break;
                    case 3:
                        mapControl1.Map.Scale = 100000;
                        break;
                    case 4:
                        mapControl1.Map.Scale = 50000;
                        break;
                    case 5:
                        mapControl1.Map.Scale = 20000;
                        break;
                    case 6:
                        mapControl1.Map.Scale = 10000;
                        break;
                    case 7:
                        mapControl1.Map.Scale = 5000;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "bZoomOut_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ��С
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void bZoomIn_Click(object sender, EventArgs e)
        {
            try
            {
                //mapControl1.Map.Scale = mapControl1.Map.Scale / 2;
                switch (iMapLevel)
                {
                    case 1:
                        mapControl1.Map.Scale = 100000;
                        break;
                    case 2:
                        mapControl1.Map.Scale = 50000;
                        break;
                    case 3:
                        mapControl1.Map.Scale = 20000;
                        break;
                    case 4:
                        mapControl1.Map.Scale = 10000;
                        break;
                    case 5:
                        mapControl1.Map.Scale = 5000;
                        break;
                    case 6:
                        mapControl1.Map.Scale = 2000;
                        break;
                    case 7:
                        mapControl1.Map.Scale = mapControl1.Map.Scale / 2;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "bZoomIn_Click");
            }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼȫͼ�鿴
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void bCenter_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["������"]);
            }
            catch (Exception ex) { ExToLog(ex, "bCenter_Click"); }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ�����ƶ�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void bLeft_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x - mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bLeft_Click"); }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ�����ƶ�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void bRight_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x + mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bRight_Click"); }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ�����ƶ�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void bDown_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y - mapControl1.Map.Bounds.Height() / 4);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bDown_Click"); }
        }

        /// <summary>
        /// �����������ʵ�ֵ�ͼ�����ƶ�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void bUp_Click(object sender, EventArgs e)
        {
            try
            {
                DPoint dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y + mapControl1.Map.Bounds.Height() / 4);
                mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
            }
            catch (Exception ex) { ExToLog(ex, "bUp_Click"); }
        }
        #endregion

        private DockingManager dockingManager;

        public VisualStyle frmVS = VisualStyle.Office2007Blue;

        private System.Windows.Forms.Label lblDistance = new System.Windows.Forms.Label();
        private System.Windows.Forms.Label lblArea = new System.Windows.Forms.Label();
        private Content cMessage=null;
        private Content cDistance;
        private Content cArea;
        /// <summary>
        /// ��ʼ��������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void InitDocument()
        {
            try
            {
                this.toolStripContainer1.Visible = false;
                this.dockingManager = new DockingManager(this.toolStripContainer1.ContentPanel, this.frmVS);
                this.dockingManager.AllowRedocking = false;

                this.lblDistance.BackColor = Color.White;
                this.lblDistance.Font = new System.Drawing.Font("����", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
                this.lblDistance.TextAlign = ContentAlignment.MiddleLeft;

                this.lblArea.BackColor = Color.White;
                this.lblArea.Font = new System.Drawing.Font("����", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
                this.lblArea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

                this.cDistance = this.dockingManager.Contents.Add(this.lblDistance, "����");
                this.cDistance.FloatingSize = new Size(250, 50);

                this.cArea = this.dockingManager.Contents.Add(this.lblArea, "���");
                this.cArea.FloatingSize = new Size(250, 50);

                this.dockingManager.AddContentWithState(cMessage, State.Floating);
                this.dockingManager.AddContentWithState(cDistance, State.Floating);
                this.dockingManager.AddContentWithState(cArea, State.Floating);

                this.dockingManager.HideAllContents(true);
            }
            catch (Exception ex)
            {
                ExToLog(ex,"InitDocument");
            }
        }

        /// <summary>
        /// �л�ģ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="userControl">Ҫ��ʾ��ģ��</param>
        private void ChangeFunctionItem(UserControl userControl)
        {
            this.mapControl1.Tools.LeftButtonTool = "Select";
            try
            {
                //�������Ϣ���ߣ�����ѡ�񹤾�    
                if (((ToolStripButton)this.toolStrip1.Items["toolInfo"]).Checked)
                {
                    ((ToolStripButton)this.toolStrip1.Items["toolInfo"]).Checked = false;
                    ((ToolStripButton)this.toolStrip1.Items["toolSel"]).Checked = true;
                    this.mapControl1.Tools.LeftButtonTool = "select";
                }
                
                //for (int i = 0; i < funcArr.Length; i++)
                //{
                //    funcArr[i].Visible = false;
                //}
                for (int i = 0; i < splitContainer2.Panel1.Controls.Count; i++)
                {
                    splitContainer2.Panel1.Controls[i].Visible = false;

                    this.toolvideo.Visible = false;
                }

                userControl.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ChangeFunctionItem-�л�ģ��");
            }
        }

        /// <summary>
        /// �ۺϲ�ѯģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuZonghe_Click(object sender, EventArgs e)
        {
            try
            {
                if (fZonghe.Visible) return;

                string[] strName = new string[] { "�����ѯ", "�ܱ߲�ѯ", "��ͷ��ѯ", "�߼���ѯ" };
                FeatureLayer featureLay = null;
                for (int i = 0; i < strName.Length; i++)
                {
                    featureLay = (FeatureLayer)mapControl1.Map.Layers[strName[i]];
                    if (featureLay == null)
                        fZonghe.CreateTemLayer(strName[i]);
                }

                ChangeFunctionItem(funcArr[0]);
                if (fZonghe.dtExcel != null) fZonghe.dtExcel.Clear();
                this.toolvideo.Visible = true;
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("�ۺϲ�ѯ");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuZonghe_Click-�ۺ�ģ��");
            }
        }

        /// <summary>
        /// ��������ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuAnjian_Click(object sender, EventArgs e)
        {
            try
            {
                if (fAnjian.Visible) return;
                ChangeFunctionItem(funcArr[1]);
                CreateTemLayer("layerLinShi", "��ʱͼ��");
                if (fAnjian.dtExcel != null) fAnjian.dtExcel.Clear();
                WriteEditLog("��������");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuAnjian_Click-����ģ��");
            }
        }

        /// <summary>
        /// �������ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuCar_Click(object sender, EventArgs e)
        {
            try
            {
                if (fCar.Visible) return;
                ChangeFunctionItem(funcArr[2]);
                UncheckedTool();
                toolSel.Checked = true;
                fCar.StartTimeCar();
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("�������");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuCar_Click-����ģ��");
            }
        }


        /// <summary>
        /// ��Ա���ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
         private void ToolGPSPolice_Click(object sender, EventArgs e)  //jie.zhang 20091230 GPS��Ա
        {
            try
            {
                if (fGPSp.Visible) return;
                ChangeFunctionItem(funcArr[9]);
                UncheckedTool();
                this.toolvideo.Visible = true;
                toolSel.Checked = true;
                fGPSp.InitGpsPolice();
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("GPS��Ա���");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ToolGPSPolice_Click-��Աģ��");
            }
        }

        /// <summary>
        /// 110�Ӵ���ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void Menu110_Click(object sender, EventArgs e)   //jie.zhang 20091230 110�Ӵ���
        {
            try
            {
                if (f110.Visible) return;
                ChangeFunctionItem(funcArr[8]);
                UncheckedTool();
                f110.Init110();
                this.toolvideo.Visible = true;
                toolSel.Checked = true;
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("110�Ӵ���");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Menu110_Click-110ģ��");
            }
        }

        /// <summary>
        /// ��Ƶ���ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuVideo_Click(object sender, EventArgs e)
        {
            try
            {
                if (fVideo.Visible) return;
                ChangeFunctionItem(funcArr[3]);
                UncheckedTool();
                fVideo.CreateVideoLayer();
                toolSel.Checked = true;
                this.toolvideo.Visible = true;
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("��Ƶ���");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuVideo_Click-��Ƶ���ģ��");
            }
        }

        /// <summary>
        /// �ΰ�����ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuKakou_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.fKakou.Visible) return;
                ChangeFunctionItem(funcArr[7]);
                fKakou.InitKK();
                CreateTemLayer("layerViewSel", "�鿴ѡ��");
                RemoveTemLayer("��ʱͼ��");
                this.toolvideo.Visible = true;

                WriteEditLog("�ΰ�����");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuKakou_Click-�ΰ�����ģ��");
            }
        }

        /// <summary>
        /// �˿ڹ���ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuPopulation_Click(object sender, EventArgs e)
        {
            try
            {
                if (fPopu.Visible) return;
                CreateTemLayer("layerPopu", "�˿�ϵͳ");
                ChangeFunctionItem(funcArr[4]);
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("�˿�ϵͳ");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuPopulation_Click-�˿�ģ��");
            }
        }

        /// <summary>
        /// ���ݹ���ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuHouse_Click(object sender, EventArgs e)
        {
            try
            {
                if (fHouse.Visible) return;
                CreateTemLayer("layerHouse", "����ϵͳ");
                ChangeFunctionItem(funcArr[5]);
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("����ϵͳ");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuHouse_Click-����ģ��");
            }
        }

        /// <summary>
        /// ֱ��ָ��ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuCommand_Click(object sender, EventArgs e)
        {
            try
            {
                if (fZhihui.Visible) return;
                fZhihui.InitZhihui();

                ChangeFunctionItem(funcArr[6]);
                this.toolvideo.Visible = true;
                RemoveTemLayer("��ʱͼ��");
                WriteEditLog("ֱ��ָ��");
            }
            catch (Exception ex) 
            {
                ExToLog(ex, "MenuCommand_Click-ֱ��ָ��ģ��");
            }
        }

        /// <summary>
        /// ������ʱͼ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="tableAiles">ͼ����</param>
        private void CreateTemLayer(string tablename, string tableAiles)
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
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "CreateTemLayer-������ʱͼ��");
            }
        }

        /// <summary>
        /// �Ƴ���ʱͼ��,�رձ�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="tableAlies">ͼ����</param>
        private void RemoveTemLayer(string tableAlies)
        {
            try
            {
                if (mapControl1.Map.Layers[tableAlies] != null)
                {
                    mapControl1.Map.Layers.Remove(tableAlies);
                    MapInfo.Engine.Session.Current.Catalog.CloseTable(tableAlies);
                }
            }
            catch(Exception ex) {
                ExToLog(ex, "RemoveTemLayer-�Ƴ���ʱͼ��");
            }
        }

        /// <summary>
        /// �����������Ҫ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="alies">ͼ������</param>
        private void clearFeatures(string alies)
        {
            try
            {
                //�����ͼ����ӵĶ���
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers[alies];
                if (fl == null) return;
                Table tableTem = fl.Table;

                //��������ж���
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch(Exception ex) {
                ExToLog(ex, "clearFeatures-�����������Ҫ��");
            }
        }

        /// <summary>
        /// �˳�ϵͳ 
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            try
            {
                fVideo.StopVideo();
                setOnline(frmLogin.string�û�����, 0);
                fAnjian.closeGLThread();
                Application.ExitThread();
                this.Dispose();
                Application.Exit();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemExit_Click-�˳�");
            }
        }

        /// <summary>
        /// �л��û�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuItemChangeUser_Click(object sender, EventArgs e)   //�л��û�
        {
            try
            {
                FrmChangeUser frmChangeUser = new FrmChangeUser(frmLogin.temDt, frmLogin.temRegionDt, ConStr,frmLogin.temEditDt);
                if (frmChangeUser.ShowDialog(this) == DialogResult.OK)
                {
                    frmLogin.temDt = FrmChangeUser.temDt1;
                    frmLogin.temRegionDt = FrmChangeUser.temRegionDt1;
                    frmLogin.temEditDt = FrmChangeUser.temEditDt;
                    frmLogin.region1 = FrmChangeUser.region1;
                    frmLogin.region2 = FrmChangeUser.region2;
                    InitPrivilege();   //����Ȩ��
                    this.toolStripUser.Text = "�û���" + FrmChangeUser.string�û�����;

                    setOnline(frmLogin.string�û�����, 0);  //��һ�û�����
                    frmLogin.string�û����� = FrmChangeUser.string�û�����;
                    setOnline(frmLogin.string�û�����, 1);  //���û�����

                    setUserSearchRegion(FrmChangeUser.string�û�����); //�����û����������ò�ѯ��Χ.
                    try
                    {
                        //��������������û��ĳ�ʼ������
                        if (this.MenuZonghe.Enabled)
                        {
                            if (fZonghe.Visible == false)
                            {
                                //CreateTemLayer("layerZonghe", "�ۺϲ�ѯ");
                                ChangeFunctionItem(funcArr[0]);
                            }
                        }

                        else if (this.MenuAnjian.Enabled)
                        {
                            if (fAnjian.Visible == false)
                            {
                                //CreateTemLayer("layerAnjian", "��������");
                                ChangeFunctionItem(funcArr[1]);
                            }
                        }

                        else if (this.ToolGPSCar.Enabled)
                        {
                            if (fCar.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[2]);
                                fCar.StartTimeCar();
                            }
                        }

                        else if (this.MenuVideo.Enabled)
                        {
                            if (fVideo.Visible == false)
                            {
                                fVideo.CreateVideoLayer();
                                ChangeFunctionItem(funcArr[3]);  //jie.zhang 2008.9.22 ע��--�ı䵯������
                            }
                        }

                        else if (this.MenuKakou.Enabled)
                        {
                            if (this.fKakou.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[7]);
                                fKakou.InitKK();
                                this.SetLayerEdit("KKLayer");
                                CreateTemLayer("layerViewSel", "�鿴ѡ��");
                                WriteEditLog("�ΰ�����");
                            }
                        }
                        else if (this.Menu110.Enabled)
                        {
                            if (this.f110.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[8]);
                                f110.Init110();
                                WriteEditLog("110�Ӵ���");
                            }
                        }
                        else if (this.ToolGPSPolice.Enabled)
                        {
                            if (this.fGPSp.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[9]);
                                fGPSp.InitGpsPolice();
                                WriteEditLog("GPS��Ա");
                            }
                        }
                        else if (this.MenuItemPop.Enabled)
                        {
                            if (fPopu.Visible == false)
                            {
                                CreateTemLayer("layerPopu", "�˿�ϵͳ");
                                ChangeFunctionItem(funcArr[4]);
                            }
                        }

                        else if (this.MenuItemHouse.Enabled)
                        {
                            if (fHouse.Visible == false)
                            {
                                CreateTemLayer("layerHouse", "����ϵͳ");
                                ChangeFunctionItem(funcArr[5]);
                            }
                        }

                        else if (this.MenuCommand.Enabled)
                        {
                            if (fZhihui.Visible == false)
                            {
                                fZhihui.InitZhihui();
                                ChangeFunctionItem(funcArr[6]);
                            }
                        }
                        else if (this.menuDataEdit.Enabled)
                        {
                            if (fGISEdit.Visible == false)
                            {
                                ChangeFunctionItem(funcArr[10]);
                            }
                        }
                    }
                    catch(Exception ex) 
                    {
                        ExToLog(ex, "MenuItemChangeUser_Click-�л��û�-��ʼ��");
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemChangeUser_Click-�л��û�");
            }
        }

        /// <summary>
        /// �����û���������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="user">�û���</param>
        private void setUserSearchRegion(string user)
        {
            try
            {
                fZonghe.strRegion = frmLogin.temRegionDt.Rows[0]["�ۺϲ�ѯ"].ToString().Trim();
                fAnjian.strRegion = frmLogin.temRegionDt.Rows[0]["��������"].ToString().Trim();
                fCar.strRegion = frmLogin.temRegionDt.Rows[0]["�������"].ToString().Trim();
                fZhihui.StrRegion = frmLogin.temRegionDt.Rows[0]["ֱ��ָ��"].ToString().Trim();
                fVideo.strRegion = frmLogin.temRegionDt.Rows[0]["��Ƶ���"].ToString().Trim();
                fHouse.strRegion = frmLogin.temRegionDt.Rows[0]["���ݹ���"].ToString().Trim();
                fPopu.strRegion = frmLogin.temRegionDt.Rows[0]["�˿ڹ���"].ToString().Trim();
                fKakou.strRegion = frmLogin.temRegionDt.Rows[0]["�ΰ�����"].ToString().Trim();  //jie.zhang 20090709 
                fGPSp.StrRegion = frmLogin.temRegionDt.Rows[0]["GPS��Ա"].ToString().Trim();
                f110.strRegion = frmLogin.temRegionDt.Rows[0]["llo�Ӿ�"].ToString().Trim();

                fZonghe.strRegion1 = frmLogin.temRegionDt.Rows[1]["�ۺϲ�ѯ"].ToString().Trim();
                fAnjian.strRegion1 = frmLogin.temRegionDt.Rows[1]["��������"].ToString().Trim();
                fCar.strRegion1 = frmLogin.temRegionDt.Rows[1]["�������"].ToString().Trim();
                fZhihui.StrRegion1 = frmLogin.temRegionDt.Rows[1]["ֱ��ָ��"].ToString().Trim();
                fVideo.strRegion1 = frmLogin.temRegionDt.Rows[1]["��Ƶ���"].ToString().Trim();
                fHouse.strRegion1 = frmLogin.temRegionDt.Rows[1]["���ݹ���"].ToString().Trim();
                fPopu.strRegion1 = frmLogin.temRegionDt.Rows[1]["�˿ڹ���"].ToString().Trim();
                fKakou.strRegion1 = frmLogin.temRegionDt.Rows[1]["�ΰ�����"].ToString().Trim();
                fGPSp.StrRegion1 = frmLogin.temRegionDt.Rows[1]["GPS��Ա"].ToString().Trim();
                f110.strRegion1 = frmLogin.temRegionDt.Rows[1]["llo�Ӿ�"].ToString().Trim();    

                #region ���뵼��
                fZonghe.strRegion2 = frmLogin.temRegionDt.Rows[0]["�ɵ���"].ToString().Trim(); //�ɳ����ɵ�������
                fZonghe.strRegion3 = frmLogin.temRegionDt.Rows[1]["�ɵ���"].ToString().Trim(); //�жӿɵ�������
                fAnjian.strRegion2 = frmLogin.temRegionDt.Rows[0]["�ɵ���"].ToString().Trim(); //�ɳ����ɵ�������
                fAnjian.strRegion3 = frmLogin.temRegionDt.Rows[1]["�ɵ���"].ToString().Trim(); //�жӿɵ�������
                fCar.strRegion2 = frmLogin.temRegionDt.Rows[0]["�ɵ���"].ToString().Trim(); //�ɳ����ɵ�������
                fCar.strRegion3 = frmLogin.temRegionDt.Rows[1]["�ɵ���"].ToString().Trim(); //�жӿɵ�������
                fVideo.strRegion2 = frmLogin.temRegionDt.Rows[0]["�ɵ���"].ToString().Trim(); //�ɳ����ɵ�������
                fVideo.strRegion3 = frmLogin.temRegionDt.Rows[1]["�ɵ���"].ToString().Trim(); //�жӿɵ�������
                fPopu.strRegion2 = frmLogin.temRegionDt.Rows[0]["�ɵ���"].ToString().Trim(); //�ɳ����ɵ�������
                fPopu.strRegion3 = frmLogin.temRegionDt.Rows[1]["�ɵ���"].ToString().Trim(); //�жӿɵ�������
                fHouse.strRegion2 = frmLogin.temRegionDt.Rows[0]["�ɵ���"].ToString().Trim(); //�ɳ����ɵ�������
                fHouse.strRegion3 = frmLogin.temRegionDt.Rows[1]["�ɵ���"].ToString().Trim(); //�жӿɵ�������
                f110.strRegion2 = frmLogin.temRegionDt.Rows[0]["�ɵ���"].ToString().Trim(); //�ɳ����ɵ�������   jie.zhang 20100621
                f110.strRegion3 = frmLogin.temRegionDt.Rows[1]["�ɵ���"].ToString().Trim(); //�жӿɵ�������
                #endregion

                fZonghe.user = user;
                fAnjian.user = user;
                fPopu.user = user;
                fHouse.user = user;
                fZhihui.User = user;
                fCar.user = user;
                fVideo.user = user;

                fKakou.user = user;   //jie.zhang 20090709 
                f110.user = user;     //jie.zhang 20091230
                fGPSp.User = user;    //jie.zhang 20091230

                fVideo.SetUserRegion();
                fCar.SetUserRegion();

                fAnjian.InitialCX3CComboBoxText(frmLogin.temRegionDt.Rows[0]["��������"].ToString().Trim());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setUserSearchRegion");
            }

            ////�ж��Ƿ���е���Ȩ��
            //OracleConnection conn = new OracleConnection(getStrConn());
            //try
            //{
                //conn.Open();
                //OracleCommand cmd = new OracleCommand("select ���� from �û� where username = '" + user + "'", conn);
                //OracleDataReader dr = cmd.ExecuteReader();
            //    if (dr.HasRows)
            //    {
            //        dr.Read();
            //        if (dr.GetValue(0).ToString() == "1")
            //        {
            //            toolDateOut.Enabled = true;
            //        }
            //        else
            //        {
            //            toolDateOut.Enabled = false;
            //        }
            //    }
            //    dr.Dispose();
            //    cmd.Dispose();
            //    conn.Close();
            //}
            //catch(Exception ex)
            //{ conn.Close();
            //ExToLog(ex, "�жϵ���Ȩ��");
            //}
        }
       
        /// <summary>
        /// �����û�Ȩ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuItemAlterType_Click(object sender, EventArgs e)
        {
            try
            {

                FrmManager frmManager = new FrmManager();
                //frmManager.Width = 280;
                frmManager.conStr = ConStr;
                frmManager.setDataGridView();
                frmManager.initRoleList();
                frmManager.user = frmLogin.string�û�����;
                frmManager.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemAlterType_Click-�����û�Ȩ��");
            }
        }       

        /// <summary>
        /// ��ӽ�ɫ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuItemAddRole_Click(object sender, EventArgs e)
        {
            try
            {
                FrmRole fRole = new FrmRole();
                fRole.conStr = ConStr;
                fRole.setListbox();
                fRole.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemAddRole_Click-��ӽ�ɫ");
            }
        }

        /// <summary>
        /// �����û�����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuItemAlterPW_Click(object sender, EventArgs e)
        {
            try
            {
                FrmChangePassword frmChangePassword = new FrmChangePassword();
                frmChangePassword.cbUser.Text = frmLogin.string�û�����;
                frmChangePassword.conStr = ConStr;
                frmChangePassword.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MenuItemAlterPW_Click-�����û�����");
            }
        }

        /// <summary>
        /// ����������Ϣ��־
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="msg">�쳣Դ</param>
        public void writelog(string msg)
        {
            StreamWriter sw = null;
            try
            {
                string filepath = Application.StartupPath + "\\rec.log";
                msg = DateTime.Now.ToString() + ":" + msg;

                sw = File.AppendText(filepath);
                sw.WriteLine(DateTime.Now.ToString() + ": " + msg);
                sw.Flush();
                sw.Dispose();
                sw.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "writelog");
            }
        }            

        /// <summary>
        /// ���ڰ�ť����¼�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                frmAbout fAbout = new frmAbout();
                fAbout.ShowDialog();
            }
            catch(Exception ex) { ExToLog(ex, "aboutToolStripMenuItem_Click"); }
        }

        /// <summary>
        /// �������Ƶ�ͼ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void mapControl1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DPoint dp;
                switch (e.KeyCode.ToString())
                {
                    case "Up":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y + mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Down":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y - mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Left":
                        dp = new DPoint(mapControl1.Map.Center.x - mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Right":
                        dp = new DPoint(mapControl1.Map.Center.x + mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Oemplus":
                    case "Add":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale / 2f;
                        break;
                    case "OemMinus":
                    case "Subtract":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale * 2f;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "mapControl1_KeyDown");
            }
        }

        /// <summary>
        /// MD5����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="s">����ܵ��ַ���</param>
        /// <returns>���ܺ���ַ���</returns>
        public string md5(string s)
        {
            try
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
                bytes = md5.ComputeHash(bytes);

                md5.Clear();

                string ret = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    ret += Convert.ToString(bytes[i], 16).PadLeft(2, '0');
                }
                return ret.PadLeft(32, '0');
            }
            catch (Exception ex) { ExToLog(ex, "md5"); return null; }
        }

        //----------======-----------
        //������ؿͻ����������
        //===========================
        private TcpListener tcpserver1 = null;
        private System.Net.Sockets.TcpListener tcpListener1 = null;
        private System.Threading.Thread ServerThread1 = null;

        //ֹͣ�����־    
        public bool Stop1 = false;
        private TcpClient tcpClient1 = null;
        private bool TcpClose1=false;
        public System.Threading.Thread tcpClientThread1 = null;
        //�����ͻ������߳�  

        private Boolean VideoFlag1 = false;

        public void CreateVideoSocket()
        {
            try
            {
                StopThread();
            }
            catch
            {
                writelog("��������ǰ�ͷ����м�������ʱ��������");
            }
            try
            {    //ȡ������    
                string host1 = Dns.GetHostName();
                //��������IP��ַ��    
                IPHostEntry hostIp1 = Dns.GetHostEntry(host1);
                writelog(string.Format("������: {0}", host1));
                writelog("��������ַ:" + hostIp1.AddressList[0].ToString());
            }
            catch (Exception x)
            {               //������ʱ�򶼳����ͱ������   �������˳�    
                writelog(x.Message);
                return;
            }

            try
            {
                IPAddress ip1 = IPAddress.Parse("127.0.0.1");                
                tcpserver1 = new TcpListener(ip1, this.videop);
                StartServer1();
            }
            catch (Exception x)
            {
                this.writelog(x.Message);
            }
        }

        // ThreadServer
        //===================    

        public void StartServer1()
        {
            try
            {
               if (this.ServerThread1 != null)
                {
                    return;
                }
                writelog(string.Format("New TcpListener....Port={0}", this.tcpserver1.LocalEndpoint.ToString()));
      
                tcpserver1.Start();
                //�����̣߳�start��������Ϊ�߳�����ʱ��ĳ����    
                this.ServerThread1 = new Thread(new ThreadStart(startsever1));
                ////�̵߳����ȼ���    
                //this.ServerThread.Priority = ThreadPriority.BelowNormal;
                this.ServerThread1.IsBackground = true;
                this.ServerThread1.Start();
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
        }

        private void startsever1()
        {
            try
            {
                while (!Stop1)   //��������ѭ��    
                {
                    writelog("���ڵȴ�����......");
                    ShowDoInfo(this.tcpserver1.LocalEndpoint.ToString());
                    tcpClient1 = tcpserver1.AcceptTcpClient();
                    writelog("�Ѿ���������......");
                    ShowDoInfo("�����ؿͻ��˽�������");
                    this.VideoFlag1 = true;
                    fVideo.getNetParameter(networkStream1, VideoFlag1);
                    tcpClientThread1 = new Thread(new ThreadStart(startclient1));
                    tcpClientThread1.IsBackground = true;
                    tcpClientThread1.Start();
                    if (Stop1) break;
                }
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
            finally
            {
                this.tcpserver1.Stop();
            }
        }

        private NetworkStream networkStream1 = null;
        private byte[] buf1 = new byte[1024 * 1024];   //Ԥ�ȶ���1MB�Ļ���  
        private int Len1 = 0;   //����ʵ�ʳ���  

        //��д���ӵĺ����������߳�    
        private void startclient1()
        {
            networkStream1 = tcpClient1.GetStream();   //������дTcp����    

            fVideo.getNetParameter(networkStream1, VideoFlag1);
            writelog(" Video �Ѿ�������Ƶ��");
            
            fZhihui.getNetParameter(networkStream1, VideoFlag1);
            writelog(" Zhihu �Ѿ�������Ƶ��");

            fKakou.getNetParameter(networkStream1, VideoFlag1);
            writelog(" KaKou �Ѿ�������Ƶ��");
            
            f110.getNetParameter(networkStream1, VideoFlag1);
            writelog(" 110 �Ѿ�������Ƶ��");
            
            fGPSp.getNetParameter(networkStream1, VideoFlag1);
            writelog(" GPSPolice �Ѿ�������Ƶ��");

            try
            {
                //��ʼѭ����дtcp��    
                while (!TcpClose1)
                {
                    //�����ǰ�߳���������״̬�����ȴ����𣬵ȴ���ֹ.....)�ͽ�����ѭ��    
                    //if (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.Running)
                    //    break;

                    //�ж�Tcp���Ƿ��пɶ��Ķ���    
                    if (networkStream1.DataAvailable)
                    {
                        //�����ж�ȡ�����ֽ�����    
                        Len1 = networkStream1.Read(buf1, 0, buf1.Length);
                        //ת����������Ϊ��    
                        byte[] temp1 = new byte[Len1];

                        for (int i = 0; i < Len1; i++)
                        {
                            temp1[i] = buf1[i];

                        }
                        ShowDoInfo("�������ݳɹ���");
                        // AnalyData(temp1);//�����յ�������                           
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(200d));
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                writelog(e.Message);
            }
            catch (ThreadAbortException y)
            {
                writelog(y.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                networkStream1.Close();
                tcpClient1.Close();
                writelog("�ر�����......");
            }
        }

        private void StopThread()
        {
            try
            {
                if (ServerThread1 != null)
                {
                    ServerThread1.Abort();
                }

                if (tcpClientThread1 != null)
                {
                    tcpClientThread1.Abort();
                }

                if (tcpClient1 != null)
                {
                    tcpClient1.Close();
                }

                if (tcpserver1 != null)
                {
                    tcpserver1.Stop();
                }

                if (tcpListener1 != null)
                {
                    tcpListener1.Stop();
                }

                //Application.ExitThread();
                //this.Dispose();
            }
            catch (Exception ex)
            { writelog(ex.Message); }
        }
       
        /// <summary>
        /// ��ʾ��Ϣ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="str">��ʾ����Ϣ</param>
        private void ShowDoInfo(string str)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    DecshowMessage dc = new DecshowMessage(ShowDoInfo);
                    this.BeginInvoke(dc, new object[] { str });

                }
                else
                    //this.toolStatusInfo.Text = "��Ϣ:" + str;
                    this.toolStripInfo.Text = "��Ϣ:" + str;
            }
            catch (Exception ex) { ExToLog(ex, "ShowDoInfo"); }
        }
        delegate void DecshowMessage(string str);

        /// <summary>
        /// ������ť����¼�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void MenuItemHelp_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(exePath + "\\˳�¹������õ�����Ϣϵͳ�����ֲ�.chm");
            }
            catch(Exception ex) {
                ExToLog(ex, "MenuItemHelp_Click");
            }
        }

        /// <summary>
        /// ���ݱ༭ģ�������ʾ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void menuDataEdit_Click(object sender, EventArgs e)
        {
            try
            {
                #region  δ�ϲ�֮ǰ�ı༭ģ��
                //string ZoomFiles = Application.StartupPath + "\\ConfigBJXX.ini";        // ���༭ģ�鴫���ַ���ڶ�ȡ��ͼ�����ű���ֵ
                //Cursor.Current = Cursors.WaitCursor;
                //CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                //DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected("select ��ʵ���� from �û� where USERnAME='" + frmLogin.string�û����� + "'");

                //LBSgisPoliceEdit.frmMap frm = new LBSgisPoliceEdit.frmMap(frmLogin.region1, frmLogin.region2, frmLogin.temEditDt);
                //frm.userName = table.Rows[0][0].ToString();
                //frm.ZoomFile = ZoomFiles;
                //frm.Show();
                //Cursor.Current = Cursors.Default;
                #endregion

                #region �ϲ���ı༭ģ��
                if (this.fGISEdit.Visible) return;
                ChangeFunctionItem(funcArr[10]);
                RemoveTemLayer("��ʱͼ��");
                #endregion

                WriteEditLog("���ݱ༭");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "menuDataEdit_Click-�༭ģ��");
            }
        }

        /// <summary>
        /// ����ر��¼�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void frmMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                fAnjian.closeGLThread();
                fCar.Dispose();// StopTimeCar();
                fVideo.Dispose();
                setOnline(frmLogin.string�û�����, 0);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmMap_FormClosing-�ر�������");
            }
        }

        /// <summary>
        /// �����û�����״̬
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="userName">�û���</param>
        /// <param name="i">�����Ƿ����ߣ�0Ϊ���ߣ�1Ϊ���ߣ�</param>
        private void setOnline(string userName,int i)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.OracleComRun("update �û� set ����=" + i + " where USERNAME='" + userName + "'");

                if (i == 1)
                {
                    WriteEditLog("��¼ϵͳ");
                }
                else {
                    WriteEditLog("�˳�ϵͳ");
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "setOnline");
            }
        }

        #region �������� add by siumo 090121
        private bool IsWebResourceAvailable(string webResourceAddress)
        {
            TcpClient tcpClient=null;
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(webResourceAddress), 80);
                tcpClient = new TcpClient();
                tcpClient.Connect(ipep);
                
                //tcpClient.GetStream();

                return true;
                //HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(webResourceAddress);
                //req.Method = "HEAD";
                //req.Timeout = 15000;
                //HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                //return (res.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "IsWebResourceAvailable");

                return false;
            }
            finally {
                tcpClient.Close();
            }
        }

        private void timeIP_Tick(object sender, EventArgs e)
        {
             //checkServerComputer();
        }
        #endregion

        /// <summary>
        /// ��������߰��س�������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolStripcbScale_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    this.mapControl1.Map.Scale = Convert.ToDouble(toolStripcbScale.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolStripcbScale_KeyPress-��������߰��س���");
            }
        }

        /// <summary>
        /// ѡ���ͼ������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolStripcbScale_SelectedIndexChanged(object sender, EventArgs e)
        { 
            try
            {
                this.mapControl1.Map.Scale = Convert.ToDouble(toolStripcbScale.Text.Trim());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolStripcbScale_SelectedIndexChanged-ѡ���ͼ������");
            }
        }

        frmOnlineUsers fOnlineUsers = new frmOnlineUsers();
        /// <summary>
        /// �鿴�����û�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolStripOnlineUsers_Click(object sender, EventArgs e)
        {
            try
            {

                if (fOnlineUsers != null)
                {
                    fOnlineUsers.Close();
                }
                fOnlineUsers = new frmOnlineUsers();
                fOnlineUsers.Left = Screen.PrimaryScreen.WorkingArea.Width;

                fOnlineUsers.strConn = strConn;
                fOnlineUsers.TopMost = true;
                fOnlineUsers.Show();
                //�ô����𽥻���
                for (int i = 0; i < 15; i++)
                {
                    fOnlineUsers.Left = fOnlineUsers.Left - 14;
                    Thread.Sleep(50);
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "toolStripOnlineUsers_Click-�鿴�����û�");
            }
        }

        string sMod;
        /// <summary>
        /// ������¼��־����
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        /// <param name="sModule">����ģ������</param>
        private void WriteEditLog(string sModule)
        {
            try
            {
                sMod=sModule;
                string strExe = "insert into ������¼ values('" + frmLogin.string�û����� + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'" + sModule + "','��¼������','')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "WriteEditLog-������¼");
            }
        }

        /// <summary>
        /// ������¼��־����
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        /// <param name="strTabName">��������</param>
        /// <param name="sql">������sql���</param>
        /// <param name="p_3">������ʽ</param>
        private void writeEditLog(string strTabName, string sql, string p_3)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + frmLogin.string�û����� + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'" + sMod + "','"+strTabName+":"+sql+"','"+p_3+"')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex) { ExToLog(ex, "WriteEditLog-������¼"); }
        }

        private void MenuItemViewLog_Click(object sender, EventArgs e)
        {
            try
            {
                frmEditLog fLog = new frmEditLog(ConStr);
                fLog.ShowDialog(this);
            }
            catch (Exception ex) { ExToLog(ex, "MenuItemViewLog_Click-������¼"); }
        }


        private MapInfo.Geometry.DPoint KKdp;
        private IResultSetFeatureCollection rfc = null;
        System.Drawing.Point point;
        /// <summary>
        /// ��ͼ�Ҽ��˵�
        /// �������� ����
        /// ������ʱ�� 2011-2-14 
        /// </summary>
        private void mapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    this.mapControl1.ContextMenuStrip = this.contextMenuStrip1;

                    string VideoTableName = string.Empty;
                    string VideoColumName = string.Empty;

                    point = e.Location;
                    MapInfo.Geometry.DPoint dp;
                    mapControl1.Map.DisplayTransform.FromDisplay(point, out dp);

                    Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 30);

                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dp, mapControl1.Map.GetDisplayCoordSys(), d);
                    si.QueryDefinition.Columns = null;

                    this.toolopenvideo.Visible = false;
                    this.menuSearchDist.Visible = false;

                    this.toolOpenClient.Visible = false;
                    this.toolVideoset.Visible = false;
                    this.toolDownVideo.Visible = false;
                    this.toolRect.Visible = false;             // ��ѡ
                    this.menuItemBayonetDire.Visible = false;  // lili 2010-12-20 �鿴���ڷ���
                    this.toolMessage.Visible = false;          // lili 2011-01-04 �鿴�ö������ϸ��Ϣ
                    selcamerid = "";

                    if (fVideo.Visible)  //jie.zhang 20101216 �Ҽ���ѡ
                    {
                        this.toolRect.Visible = true;
                    }

                    //����ѡ����Ƶʱ
                    if (this.mapControl1.Map.Layers["�鿴ѡ��"] != null)
                    {
                        try
                        {
                            Irfc = rfc = Session.Current.Catalog.Search("�鿴ѡ��", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    foreach (Feature f in rfc)
                                    {
                                        if (Convert.ToString(f["����"]) == "��Ƶ")
                                        {
                                            selcamerid = Convert.ToString(f["��_ID"]);
                                            this.toolopenvideo.Visible = true;
                                            this.toolOpenClient.Visible = true;
                                            this.toolVideoset.Visible = true;
                                            this.toolDownVideo.Visible = true;
                                            this.toolMessage.Visible = true;
                                            break;
                                        }
                                        else if (Convert.ToString(f["����"]) == "����")
                                        {
                                            string sId = Convert.ToString(f["��_ID"]);
                                            this.toolMessage.Visible = true;
                                            DataTable ttTab = GetTable("select * from GPS������λϵͳ where �ն�ID����='" + sId + "'");
                                            if (ttTab != null && ttTab.Rows.Count > 0)
                                            {
                                                selcamerid = ttTab.Rows[0]["CAMID"].ToString();
                                                break;
                                            }
                                        }
                                        else if (fKakou.Visible)
                                        {
                                            this.toolMessage.Visible = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    //�ΰ��������Ƿ��п���, ������ʾ�鿴�ܱ�
                    if (fKakou.Visible && this.mapControl1.Map.Layers["KKLayer"] != null) {
                        try
                        {
                            rfc = Session.Current.Catalog.Search("KKLayer", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    this.menuSearchDist.Visible = true;
                                    this.menuItemBayonetDire.Visible = true;  // 2010-12-20 lili �鿴���ڷ���
                                    this.toolMessage.Visible = true;
                                    this.KKdp = dp;
                                }
                            }
                        }
                        catch { }
                    }

                    //if (f110.Visible && this.mapControl1.Map.Layers["SocketLayer"] != null)
                    //{
                    //    try
                    //    {
                    //        rfc = Session.Current.Catalog.Search("SocketLayer", si);
                    //        if (rfc != null)
                    //        {
                    //            if (rfc.Count > 0)
                    //            {
                    //                this.menuSearchDist.Visible = true;
                    //                this.KKdp = dp;
                    //                return;
                    //            }
                    //        }
                    //    }
                    //    catch { }
                    //}

                    //���selcamerid��ֵ,˵���Ѿ��ҵ���Ƶ,�����ж�������Ƶ��
                    if (selcamerid != "") return;

                    if (this.mapControl1.Map.Layers["VideoLayer"] != null || this.mapControl1.Map.Layers["VideoCarLayer"] != null)
                    {
                        if(this.mapControl1.Map.Layers["VideoLayer"] != null)
                        {
                            //this.toolOpenClient.Visible = true;
                            //this.toolVideoset.Visible = true;
                            //this.toolopenvideo.Visible = true;
                            //this.toolDownVideo.Visible = true;
                            
                            rfc = Session.Current.Catalog.Search("VideoLayer", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    foreach (Feature f in rfc)
                                    {
                                        selcamerid = Convert.ToString(f["�豸���"]);
                                        if (selcamerid!="")
                                        {
                                           
                                            this.toolopenvideo.Visible = true;
                                            this.toolDownVideo.Visible = true;
                                            this.toolOpenClient.Visible = true;
                                            this.toolVideoset.Visible = true;
                                            return;  //���selcamerid��ֵ,����
                                        }
                                    }
                                }
                            }
                        }
                        if(this.mapControl1.Map.Layers["VideoCarLayer"] != null)
                        {
                            rfc = Session.Current.Catalog.Search("VideoCarLayer", si);
                            if (rfc != null)
                            {
                                if (rfc.Count > 0)
                                {
                                    foreach (Feature f in rfc)
                                    {
                                        selcamerid = Convert.ToString(f["�豸���"]);
                                        if (selcamerid!="")
                                        {
                                            this.toolOpenClient.Visible = true;
                                            this.toolVideoset.Visible = true;
                                            this.toolopenvideo.Visible = true;
                                            this.toolDownVideo.Visible = true;
                                            return;  //���selcamerid��ֵ,����
                                        }
                                    }
                                }
                            }
                        }
                    }

                    else if (this.mapControl1.Map.Layers["queryLayer"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("��ѯ��", si);
                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    if (Convert.ToString(f["�豸����"]) != "")
                                    {
                                        selcamerid = Convert.ToString(f["�豸���"]);
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (this.mapControl1.Map.Layers["��Ƶ"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("��Ƶ", si);
                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    selcamerid = Convert.ToString(f["�豸���"]);
                                    if (selcamerid != "")
                                    {
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        break;  //���selcamerid��ֵ,����
                                    }
                                }
                            }
                        }
                    }
                    else if (this.mapControl1.Map.Layers["CarLayer"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("CarLayer", si);
                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    selcamerid = Convert.ToString(f["CAMERID"]);
                                    if (selcamerid.Length > 5)
                                    {
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        fVideo.getVideoparam(VideoTableName, VideoColumName);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (this.mapControl1.Map.Layers["�ۺϲ�ѯ"] != null)
                    {
                        rfc = Session.Current.Catalog.Search("�ۺϲ�ѯ", si);

                        if (rfc != null)
                        {
                            if (rfc.Count > 0)
                            {
                                foreach (Feature f in rfc)
                                {
                                    if (Convert.ToString(f["����"]) == "��Ƶλ��")
                                    {
                                        selcamerid = Convert.ToString(f["��_ID"]);
                                        this.toolOpenClient.Visible = true;
                                        this.toolVideoset.Visible = true;
                                        this.toolopenvideo.Visible = true;
                                        this.toolDownVideo.Visible = true;
                                        fVideo.getVideoparam(VideoTableName, VideoColumName);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                    }

                }
                catch(Exception ex)
                {
                    this.contextMenuStrip1.Visible = false;
                    ExToLog(ex, "�Ҽ�����");
                }
            }
            else
            {
				this.contextMenuStrip1.Visible = false;
            }
        }

        string selcamerid = "";

        /// <summary>
        /// �鿴��Ƶ
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        private void toolopenvideo_Click(object sender, EventArgs e)
        {
            try
            {
                fVideo.getNetParameter(networkStream1, VideoFlag1);
                bool sendflag = fVideo.OpenVideo(selcamerid);
                if (sendflag)
                {
                    this.WindowState = FormWindowState.Minimized;
                }
                writeEditLog("��Ƶλ��", "��Ƶ���=" + selcamerid, "�鿴��Ƶ");
            }
            catch (Exception ex)
            {
                writelog("����Ƶʱ��������--" + ex.Message);
            }
        }

        /// <summary>
        /// �Ҽ��ܱ߲�ѯ
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        private void menuSearchDist_Click(object sender, EventArgs e)
        {
            try
            {
                fKakou.SearchDistance(this.KKdp, this.KKSearchDist);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "menuSearchDist_Click");
            }
        }

        /// <summary>
        /// �Ҽ����ΪͼƬ
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        private void conmenuPic_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog MyDlg = new SaveFileDialog();
                MyDlg.AddExtension = true;
                MyDlg.DefaultExt = "jpg";
                MyDlg.FileName = string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                MyDlg.Filter = "ͼ���ļ�(Jpg)|*.jpg";
                if (MyDlg.ShowDialog() == DialogResult.OK)
                {
                    string MyFileName = MyDlg.FileName;

                    Map map = (Map)this.mapControl1.Map.Clone();

                    MapExport printer = new MapExport(map);

                    printer.ExportSize = new ExportSize(1024, 768);

                    printer.Format = ExportFormat.Jpeg;

                    printer.Export(MyFileName);

                    MessageBox.Show("�������", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                MessageBox.Show("����ͼƬʱ���������޷���ɱ���", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// ������ť����¼�
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        private void toolDateOut_Click(object sender, EventArgs e)
        {
            try
            {
                string tableName = "";
                if (_exportDT != null)
                    _exportDT.Clear();

                if (fZonghe.Visible)
                {
                    //_exportDT = fZonghe.dtExcel;
                    exportSQL = fZonghe.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                    if (fZonghe.tabControl1.SelectedIndex == 0)
                    {
                        if (fZonghe.comboBox1.Text == "ȫ��")
                        {
                            MessageBox.Show("��ѡ��һ������е���(����ѡ��'ȫ��')", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        tableName = fZonghe.comboBox1.Text;

                    }
                    else if (fZonghe.tabControl1.SelectedIndex == 2)
                    {
                        tableName = fZonghe.comboClass.Text;
                    }
                    else if (fZonghe.tabControl1.SelectedIndex == 3)
                    {
                        tableName = fZonghe.comboTable.Text;
                        if (fZonghe.comboTable.Text == "��Ϣ��")
                            _exportDT = fZonghe.dtExcel;
                    }
                    else
                    {
                        MessageBox.Show("��ǰû�пɵ��������ݱ�!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (fZonghe.strRegion != fZonghe.strRegion2 || fZonghe.strRegion1 != fZonghe.strRegion3)
                    {
                        MessageBox.Show("���ĵ���Ȩ��С�ڲ�ѯȨ��,��������뵱ǰ�б���ܲ�һ��!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fAnjian.Visible)
                {
                    tableName = "������Ϣ";
                    //_exportDT = fAnjian.dtExcel;
                    exportSQL = fAnjian.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                    if (fAnjian.strRegion != fAnjian.strRegion2 || fAnjian.strRegion1 != fAnjian.strRegion3)
                    {
                        MessageBox.Show("���ĵ���Ȩ��С�ڲ�ѯȨ��,��������뵱ǰ�б���ܲ�һ��!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fCar.Visible)
                {
                    tableName = "GPS������λϵͳ";
                    //_exportDT = fCar.dtExcel;
                    _exportDT = GetExcelDataTable(fCar._startNo, fCar._endNo, fCar.excelSql);
                    if (fCar.strRegion != fCar.strRegion2 || fCar.strRegion1 != fCar.strRegion3)
                    {
                        MessageBox.Show("���ĵ���Ȩ��С�ڲ�ѯȨ��,��������뵱ǰ�б���ܲ�һ��!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fGPSp.Visible)
                {
                    tableName = "��Ա��λϵͳ";
                    _exportDT = fGPSp.dtExcel;
                    if (fCar.strRegion != fCar.strRegion2 || fCar.strRegion1 != fCar.strRegion3)
                    {
                        MessageBox.Show("���ĵ���Ȩ��С�ڲ�ѯȨ��,��������뵱ǰ�б���ܲ�һ��!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fVideo.Visible)
                {
                    tableName = "��Ƶλ��";
                    //_exportDT = fVideo.dtExcel;
                    _exportDT = GetExcelDataTable(fVideo._startNo, fVideo._endNo, fVideo.excelSql);

                }
                else if (fKakou.Visible)
                {
                    tableName = "�ΰ�����ϵͳ";
                    _exportDT = fKakou.dtExcel;
                }
                else if (fPopu.Visible)
                {
                    tableName = "�˿�ϵͳ";
                    //_exportDT = fPopu.dtExcel;
                    exportSQL = fPopu.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                }
                else if (fHouse.Visible)
                {
                    tableName = "�����ݷ���ϵͳ";
                    //_exportDT = fHouse.dtExcel;
                    exportSQL = fHouse.exportSql;
                    _exportDT = GetExcelDataTable(exportSQL);
                }
                else if (fZhihui.Visible)
                {
                    tableName = "GPS110.������Ϣ110";
                    _exportDT = fZhihui.dtExcel;
                }
                else if (f110.Visible)������������������������������//jie.zhang 20100621  ���ݵ���
                {
                    tableName = "GPS110.������Ϣ110";
                    _exportDT = f110.dtExcel;
                }

                if (_exportDT != null && _exportDT.Rows.Count > 0)
                {
                    DataExport(tableName);
                }
                else
                {
                    MessageBox.Show("��ǰ�б�Ϊ��", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "��������");
            }
        }

        /// <summary>
        /// ��ȡ��������
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        /// <param name="_startNo">��ʼ��</param>
        /// <param name="_endNo">������</param>
        /// <param name="excelSQL">��������SQL</param>
        /// <returns>���ݼ�</returns>
        private DataTable GetExcelDataTable(int _startNo, int _endNo, string excelSQL)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                if (excelSQL == string.Empty)
                {
                    System.Windows.Forms.MessageBox.Show("���Ȳ�ѯ�����ݣ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                    return null;
                }
                DataTable dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = excelSQL;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(_startNo, _endNo,dtInfo);

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
                ExToLog(ex, "GetExcelDataTable");
                return null;
            }
        }

        /// <summary>
        /// ��ȡ��������
        /// �������� ����
        /// ������ʱ�� 2011-1-24 
        /// </summary>
        /// <param name="excelSQL">��������SQL</param>
        /// <returns>���ݼ�</returns>
        private DataTable GetExcelDataTable(string excelSQL)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                if (excelSQL == string.Empty)
                {
                    System.Windows.Forms.MessageBox.Show("���Ȳ�ѯ�����ݣ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                DataTable dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = excelSQL;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(dtInfo);

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
                ExToLog(ex, "GetExcelDataTable");
                return null;
            }
        }

        private DataTable _exportDT = null;

        /// <summary>
        /// ��������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="tableName">����</param>
        private void DataExport(string tableName)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "��ѡ�񽫵�����EXCEL�ļ����·��";
                sfd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                sfd.FileName = "EXP" + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                {
                    if (sfd.FileName.LastIndexOf(".xls") <= 0)
                    {
                        sfd.FileName = sfd.FileName + ".xls";
                    }
                    string fileName = sfd.FileName;
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;

                    if (dg.OutData(fileName, _exportDT, tableName))
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("����Excel���!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("����Excelʧ��!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "DataExport");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ������� ���˹��������Σ�
        /// </summary>
        private void toolImport_Click(object sender, EventArgs e)
        {
            #region �������
            //try
            //{
            //    if (fZonghe.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
            //        ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
            //        string tableName = "";
            //        if (fZonghe.tabControl1.SelectedIndex == 0)
            //        {
            //            if (fZonghe.comboBox1.Text == "ȫ��")
            //            {
            //                MessageBox.Show("��ѡ��һ������е���(����ѡ��'ȫ��')", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                return;
            //            }
            //            ofd.FileName = fZonghe.comboBox1.Text;
            //            tableName = fZonghe.comboBox1.Text;

            //        }
            //        else if (fZonghe.tabControl1.SelectedIndex == 2)
            //        {
            //            GetFromName getFromName = new GetFromName(fZonghe.comboClass.Text);
            //            ofd.FileName = getFromName.TableName;
            //            tableName = getFromName.TableName;
            //        }
            //        //else if (fZonghe.tabControl1.SelectedIndex == 3)
            //        //{
            //        //    ofd.FileName = "��ȫ������λ";
            //        //    tableName = "��ȫ������λ";
            //        //}
            //        else if (fZonghe.tabControl1.SelectedIndex == 3)
            //        {
            //            GetFromName getFromName = new GetFromName(fZonghe.comboTable.Text);
            //            ofd.FileName = getFromName.TableName;
            //            tableName = getFromName.TableName;
            //        }
            //        else
            //        {
            //            MessageBox.Show("��ǰû����Ҫ��������ݱ�!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //            return;
            //        }

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("�ɹ���� " + dataCount.ToString() + " �����ݵ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("��������ʧ�ܣ������Excel�Ƿ������ݣ�\r \r���߸�Excel�Ƿ�����ʹ��,�Լ����ݸ�ʽ�Ƿ���ȷ", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }

            //    }
            //    else if (fAnjian.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
            //        ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
            //        ofd.FileName = "������Ϣ";
            //        string tableName = "������Ϣ";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("�ɹ���� " + dataCount.ToString() + " �����ݵ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("��������ʧ�ܣ������Excel�Ƿ������ݣ�\r \r���߸�Excel�Ƿ�����ʹ��,�Լ����ݸ�ʽ�Ƿ���ȷ", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }
            //    }
            //    else if (fCar.Visible)
            //    {
            //        MessageBox.Show("��ǰû����Ҫ��������ݱ�!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }
            //    else if (fVideo.Visible)
            //    {
            //        MessageBox.Show("��ǰû����Ҫ��������ݱ�!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //        /*
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
            //        ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
            //        ofd.FileName = "��Ƶλ��";
            //        string tableName = "��Ƶλ��";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("�ɹ���� " + dataCount.ToString() + " �����ݵ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("��������ʧ�ܣ������Excel�Ƿ������ݣ�\r \r���߸�Excel�Ƿ�����ʹ��,�Լ����ݸ�ʽ�Ƿ���ȷ", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }
            //        return;
            //        */
            //    }
            //    else if (fKakou.Visible)
            //    {
            //        MessageBox.Show("��ǰû����Ҫ��������ݱ�!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }
            //    else if (fPopu.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
            //        ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
            //        ofd.FileName = "�˿�ϵͳ";
            //        string tableName = "�˿�ϵͳ";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("�ɹ���� " + dataCount.ToString() + " �����ݵ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("��������ʧ�ܣ������Excel�Ƿ������ݣ�\r \r���߸�Excel�Ƿ�����ʹ��,�Լ����ݸ�ʽ�Ƿ���ȷ", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }
            //    }
            //    else if (fHouse.Visible)
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
            //        ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
            //        ofd.FileName = "�����ݷ���ϵͳ";
            //        string tableName = "�����ݷ���ϵͳ";

            //        try
            //        {
            //            if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
            //            {
            //                DataGuide dg = new DataGuide();
            //                this.Cursor = Cursors.WaitCursor;
            //                int dataCount = dg.InData(ofd.FileName, tableName);
            //                this.Cursor = Cursors.Default;
            //                if (dataCount != 0)
            //                {
            //                    MessageBox.Show("�ɹ���� " + dataCount.ToString() + " �����ݵ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //                else
            //                {
            //                    MessageBox.Show("��������ʧ�ܣ������Excel�Ƿ������ݣ�\r \r���߸�Excel�Ƿ�����ʹ��,�Լ����ݸ�ʽ�Ƿ���ȷ", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            this.Cursor = Cursors.Default;
            //        }

            //    }
            //    else
            //    {
            //        MessageBox.Show("��ǰû����Ҫ��������ݱ�!", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ExToLog(ex, "toolImport_Click");
            //}
            #endregion
        }
     
        /// <summary>
        /// ����110 ���˹������ڲ��ԣ�
        /// </summary>
        private void ����110ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = string.Empty;

                Test test = new Test();
                test.TopMost = true;
                if (test.ShowDialog(this) == DialogResult.OK)
                {
                    msg = test.msg;
                    if (msg.Length > 0)
                        Receiver110Data(msg);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "����110ToolStripMenuItem_Click");
            }
        }     

        private void axNetcomm1_DataArrived(object sender, AxNETCOMMLib._DNetcommEvents_DataArrivedEvent e)
        {
            writelog("110Socket���յ������ݣ�" + e.dataBuf);

            try
            {
                if (!string.IsNullOrEmpty(e.dataBuf))
                {
                    //�������@�����ص���ַ@��Ҫ����@��������@�����ɳ���@�����ж�@�Խ���ID@������Դ@����ʱ��@X@Y@
                    Receiver110Data(e.dataBuf);
                }
            }
            catch (Exception ex)
            {
                writelog("�ж�110���ݴ���ʽ"+ex.ToString());
            }
        }
      

        private void Receiver110Data(string socketData)
        {
            try
            {

                string msg = socketData;

                string[] message = msg.Split('@');

                int i = Convert.ToInt32(message[0].Substring(0, 1));

                if(i!=1 && i!=2)
                {
                    MessageBox.Show(@"���ݸ�ʽ�����޷�����");
                    return;
                }

                
                if (i == 2)
                    message = Get110Msg(message[0]);

                if (message == null) return;

                string bmpName = f110.GetBmpName(message[7]);

                //message[0]  �������
                //message[1]  �����ص���ַ  110
                //message[2]  ��Ҫ����
                //message[3]  ��������
                //message[4]  �����ɳ���
                //message[5]  �����ж�
                //message[6]  �Խ���ID
                //message[7]  ������Դ
                //message[8]  ����ʱ��
                //message[9]  X
                //message[10]  Y

                // 0-5       "��������,�������,����״̬,��������,����_����,ר����ʶ," +
                //6-11           "����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ص�_�ֵ�,��������,�����ص���ַ," +
                //12-17           "��������,��Ҫ����,�����ֶ��ص�,������������,��������,������Դ," +
                // 18-20          "������Դ,����ʱ��, �����ж�," +
                //     21      " �����ɳ���," +
                //     22-25      "����������, �Խ���ID,x,y ";


                switch (_schnum)
                {
                    case 1: //ֱ��ָ��

                        if (fZhihui.Visible == false)
                        {
                            fZhihui.InitZhihui();

                            ChangeFunctionItem(funcArr[6]);
                            RemoveTemLayer("��ʱͼ��");
                            WriteEditLog("ֱ��ָ��");
                        }

                        fZhihui.Deal110Msg(bmpName, message);

                        break;
                    case 2: //ƥ������
                        if (f110.Visible == false)
                        {
                            ChangeFunctionItem(funcArr[8]);
                            UncheckedTool();
                            f110.Init110();
                            toolSel.Checked = true;
                            RemoveTemLayer("��ʱͼ��");
                        }

                        f110.DealSocket(bmpName, message, "ƥ������");

                        break;
                    case 3: //��������

                        if (f110.Visible == false)
                        {
                            ChangeFunctionItem(funcArr[8]);
                            UncheckedTool();
                            f110.Init110();
                            toolSel.Checked = true;
                            RemoveTemLayer("��ʱͼ��");
                        }

                        f110.DealSocket(bmpName, message, "��������");
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex,"�ж����ݲ�����");
            }
        }

        private string[] Get110Msg(string msg)
        {
            //message[0]  �������  110
            //message[1]  �����ص���ַ  110
            //message[2]  ��Ҫ����
            //message[3]  ��������
            //message[4]  �����ɳ���
            //message[5]  �����ж�
            //message[6]  �Խ���ID
            //message[7]  ������Դ
            //message[8]  ����ʱ��
            //message[9]  X
            //message[10]  Y
            string[] mg = null;

            try
            {
                string ajbh = msg.Substring(1, msg.Length - 1);
                string sql = "Select t.�����ص���ַ,t.��Ҫ����,t.��������,t.�����ɳ���," +
                             "t.�����ж�,t.�Խ���ID,t.������Դ,t.����ʱ��," +
                             "t.X,t.Y from GPS110.������Ϣ110 t where t.�������='" + ajbh + "'";
                DataTable dt = GetTable(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string fadz = Convert.ToString(dr["�����ص���ַ"]);
                        string jyaq = Convert.ToString(dr["��Ҫ����"]);
                        string ajmc = Convert.ToString(dr["��������"]);
                        string sspcs = Convert.ToString("�����ɳ���");
                        string sszd = Convert.ToString("�����ж�");
                        string callid = Convert.ToString(dr["�Խ���ID"]);
                        string bjly = Convert.ToString(dr["������Դ"]);
                        string bjsj = Convert.ToString(dr["����ʱ��"]);
                        try
                        {
                            string sx = Convert.ToString(dr["X"]);
                            string sy = Convert.ToString(dr["Y"]);
                            mg = new string[] {ajbh, fadz, jyaq, ajmc, sspcs, sszd, callid, bjly, bjsj, sx, sy};
                            
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(@"�ñ�����Ŷ�Ӧ������������ֵ����ȷ�ϴ��ڴ�����", @"ϵͳ��ʾ", MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"û����˱�����Ŷ�Ӧ����Ϣ", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show(@"����Ϊ�Ƿ����ݣ��޷�����:"+msg, @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ExToLog(ex, @"���ݱ��������ȡ����ʱ��������:" + msg);
            }
            return mg;
        }


        private void axNetcomm1_NetMessage(object sender, AxNETCOMMLib._DNetcommEvents_NetMessageEvent e)
        {
            try
            {
                switch (e.msgCode)
                {
                    case 0: break;
                    case 1: break;
                    case 2:
                        RecToLog("110���ӳɹ���");
                        toolStripInfo.Text = @"��Ϣ��110���ӳɹ�";
                        break;//��Ӵ�������������
                    case 3: RecToLog("110���ӶϿ���"); toolStripInfo.Text = @"��Ϣ��110���ӶϿ�"; break;// ��Ӵ����������ӶϿ�
                    case 4: RecToLog("110����ʧ�ܣ�"); toolStripInfo.Text = @"��Ϣ��110����ʧ��"; break; // ��Ӵ�����������ӳ���
                    case 5: break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writelog("�ж�110�Ƿ���ͨ"+ex.ToString());
            }
        }

        private void frmMap_Load(object sender, EventArgs e)
        {
           
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    string s = "1A000000@�Ϻ�@��������@���԰@����@��ɳ��@ID101@100����@2010-2-3@113.239@22.8375";
        //    writelog("110Socket���յ������ݣ�" +s);

        //    if (f110.Visible) return;
        //    else
        //    {
        //        ChangeFunctionItem(funcArr[8]);
        //        UncheckedTool();
        //        f110.Init110();
        //        toolSel.Checked = true;
        //        RemoveTemLayer("��ʱͼ��");
        //    }

        //    if (s != null && s.Length > 0)
        //    {
        //        f110.DealSocket(s);
        //    }
        //}

        //private void button1_Click_1(object sender, EventArgs e)
        //{
        //    string s = "1A000000@�Ϻ�@��������@���԰@����@��ɳ��@ID101@100����@2010-2-3@113.239@22.8375";
        //    writelog("110Socket���յ������ݣ�" + s);

        //    if (f110.Visible == false)
        //    {
        //        ChangeFunctionItem(funcArr[8]);
        //        UncheckedTool();
        //        f110.Init110();
        //        toolSel.Checked = true;
        //        RemoveTemLayer("��ʱͼ��");
        //    }

        //    if (s != null && s.Length > 0)
        //    {
        //        f110.DealSocket(s);
        //    }
        //}



        /// <summary>
        /// ��ѯSQL
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ִ��SQL
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// �쳣��־
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPolice-frmMap-" + sFunc);
        }

        /// <summary>
        /// 110������־
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="s">��־����</param>
        private void RecToLog(string s)
        {
            CLC.BugRelated.LogWrite(s, Application.StartupPath + "\rec.log");
        }

        /// <summary>
        /// ��¼������¼
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="sql">����sql���</param>
        /// <param name="method">������</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + frmLogin.string�û����� + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'������',''" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch { }
        }

        /// <summary>
        /// ���ù���-110�������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void menu110set_Click(object sender, EventArgs e)
        {
            try
            {
                FrmScheme frmScheme = new FrmScheme();
                frmScheme.SchemeNmuber = _schnum;
                if (frmScheme.ShowDialog(this) != DialogResult.OK) return;
                _schnum = frmScheme.SchemeNmuber;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                CLC.INIClass.IniWriteValue("110", "ģʽ", _schnum.ToString());
            }
            catch (Exception ex)
            {
                writelog("����110����ģʽʱ��������"+ex.ToString());
            }
        }

        /// <summary>
        /// ���ù���-���ű���������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void ���ű�����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string scale = "";
                frmScale frmscale = new frmScale();
                frmscale.scale = scale;
                if (frmscale.ShowDialog(this) != DialogResult.OK) return;
                scale = frmscale.scale;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                CLC.INIClass.IniWriteValue("������", "���ű���", scale);
            }
            catch (Exception ex)
            {
                writelog("�������ű���ʱ��������" + ex.ToString());
            }
        }

        /// <summary>
        /// ��ʱ������־
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="message">�쳣��Ϣ</param>
        /// <param name="funName">��������</param>
        private void WriteLog(string message, string funName)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\timeTestLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine("����:" + funName + "  ��     " + message);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            { ExToLog(ex, "��ʱ������־"); }
        }

        /// <summary>
        /// ����Ƶ�ͻ���
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolOpenClient_Click(object sender, EventArgs e)
        {try
            {
                fVideo.OpenVideoClient();
            }
            catch (Exception ex)
            { ExToLog(ex, "toolOpenClient_Click"); }
        }

        /// <summary>
        /// ��Ƶ�������
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                fVideo.videoset();
            }
            catch (Exception ex)
            { ExToLog(ex, "toolStripMenuItem3_Click"); }
        }

        /// <summary>
        /// ��Ƶ����
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.selcamerid != "")
                {
                    fVideo.DownRecord(selcamerid);
                }
            }
            catch (Exception ex)
            { ExToLog(ex, "toolStripMenuItem2_Click"); }
        }

        /// <summary>
        /// �رմ�����ʾ���
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                this.panErrorMessage.Visible = false;
            }
            catch (Exception ex)
            { ExToLog(ex, "linkClose_LinkClicked"); }
        }

        /// <summary>
        /// �һ���ѡ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolRect_Click(object sender, EventArgs e)
        {
            try
            {
                this.mapControl1.Tools.LeftButtonTool = "SelectRect";
            }
            catch (Exception ex)
            { ExToLog(ex, "toolRect_Click"); }
        }

        /// <summary>
        /// �鿴���ڷ���
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void menuItemBayonetDire_Click(object sender, EventArgs e)
        {
            try
            {
                fKakou.AddBayoneDireCond(rfc);
            }
            catch (Exception ex)
            { ExToLog(ex, "menuItemBayonetDire_Click"); }
        }

        private IResultSetFeatureCollection Irfc = null;
        /// <summary>
        /// �鿴��ϸ��Ϣ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void toolMessage_Click(object sender, EventArgs e)
        {
            try
            {
                if (Irfc != null)
                    if (Irfc.Count > 0)
                        fKakou.getMessage(Irfc, point);

                if (rfc != null)
                    if (rfc.Count > 0)
                        fKakou.getMessage(rfc, point);
            }
            catch (Exception ex)
            { 
                ExToLog(ex, "toolMessage_Click");
            }
        }
    }
}