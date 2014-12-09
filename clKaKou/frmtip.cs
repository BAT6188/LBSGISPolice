using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.IO;
using System.Runtime.InteropServices;

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Dialogs;
using MapInfo.Windows.Controls;

namespace clKaKou
{
    public partial class frmtip : Form
    {
        public int renum = 0;

        private MapControl mapcontrol1;    //��ͼ�ؼ�
        private string[] ConStr;                 //���ݿ������ַ�����

        public string AlarmSys = string.Empty;
        public string AlarmUser = string.Empty;
        private string UserRegion = string.Empty;
        private string UserN = string.Empty;   //��½�û�����
        private frmAlarm falarm = new frmAlarm();

        public Boolean isKakou = false;

        public frmtip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ϵͳ��ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void frmtip_Load(object sender, EventArgs e)
        {
            try
            {

                this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
                falarm.Visible = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-01-ϵͳ��ʼ��");
            }
        }

        /// <summary>
        /// ����ر�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void frmtip_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.Visible = false;
                e.Cancel = true;
                this.timalarm.Enabled = false;

                // �ر�һ��ʱ����Զ�����
                // ֻ�п��ڴ��岻�ɼ����Ҷ���Ϊ��ģ�� ʱ�Ų��ᵯ����  �ж� ���ǿ���ģ�飬���Ҳ���ģ��
                //if (isKakou == false && AlarmSys == "ģ��")
                if (AlarmSys == "ģ��" && isKakou ==false)
                {
                    this.timer1.Enabled = false;
                }
                else if ((AlarmSys == "ģ��" && isKakou == true) || (AlarmSys == "ϵͳ" && isKakou == false) || (AlarmSys == "ϵͳ" && isKakou == true))
                {
                    this.timer1.Interval = 1 * 1000;
                    this.timer1.Enabled = true;
                }
                else
                {
                    this.timer1.Enabled = false;
                }

                this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-02-����ر�");
            }
        }

        /// <summary>
        /// ���ݲ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sqlstr">���ݿ������ַ���</param>
        /// <param name="mapc">��ͼ�ؼ�</param>
        /// <param name="asys">����ϵͳ����</param>
        /// <param name="auer">������������</param>
        /// <param name="usrg">�û�����Ȩ��</param>
        /// <param name="usern">��½�û�����</param>
        public void GetPara(string[] sqlstr, MapControl mapc, string asys, string auer, string usrg, string usern)
        {
            try
            {
                mapcontrol1 = mapc;

                ConStr = sqlstr;

                AlarmSys = asys;
                AlarmUser = auer;
                UserRegion = usrg;
                UserN = usern;

                falarm.StrCon = ConStr;
                falarm.Mapcontrol1 = mapc;
                falarm.KKuser = auer;
                falarm.strRegion = usrg;
                falarm.UserName = usern;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-03-���ݲ���");
            }
        }



        ///  <summary>  
        ///  ���ڲ�������  
        ///  ���༭��   ����
        ///  ���༭ʱ��  2011-1-28
        ///  </summary>    
        internal class Helpers
        {
            [Flags]
            public enum PlaySoundFlags : int
            {
                SND_SYNC = 0x0000,    /*  play  synchronously  (default)  */  //ͬ��  
                SND_ASYNC = 0x0001,    /*  play  asynchronously  */  //�첽  
                SND_NODEFAULT = 0x0002,    /*  silence  (!default)  if  sound  not  found  */
                SND_MEMORY = 0x0004,    /*  pszSound  points  to  a  memory  file  */
                SND_LOOP = 0x0008,    /*  loop  the  sound  until  next  sndPlaySound  */
                SND_NOSTOP = 0x0010,    /*  don't  stop  any  currently  playing  sound  */
                SND_NOWAIT = 0x00002000,  /*  don't  wait  if  the  driver  is  busy  */
                SND_ALIAS = 0x00010000,  /*  name  is  a  registry  alias  */
                SND_ALIAS_ID = 0x00110000,  /*  alias  is  a  predefined  ID  */
                SND_FILENAME = 0x00020000,  /*  name  is  file  name  */
                SND_RESOURCE = 0x00040004    /*  name  is  resource  name  or  atom  */
            }

            [DllImport("winmm")]
            public static extern bool PlaySound(string szSound, IntPtr hMod, PlaySoundFlags flags);
        }

        /// <summary>
        /// ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="strFileName">�����ļ���ַ</param>
        public static void PlayAlarmSound(string strFileName)
        {
            //����PlaySound����,��������  
            try
            {
                Helpers.PlaySound(strFileName, IntPtr.Zero, Helpers.PlaySoundFlags.SND_ASYNC);
            }
            catch //(Exception ex)
            {
                //writeToLog(ex, "clKaKou-frmtip-PlayAlarmSound-��������");
            }
        }


        private int alarmcount = 0;
        /// <summary>
        /// ��ѯ������Ϣ����ʾ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void timalarm_Tick(object sender, EventArgs e)
        {
            //if (this.Visible == false)
            //{
                try
                {
                    string sqlstring = string.Empty;
                    if (this.AlarmUser == "����" || this.UserRegion == "˳����")
                    {
                        sqlstring = "Select * from V_ALARM where ����״̬ is null  and  trunc(sysdate) - trunc(����ʱ��)<7 and (������� not like '%"+ this.UserN+"%' or ������� is null) order by ����ʱ��";
                    }
                    else if (this.AlarmUser == "�û�")
                    {
                        sqlstring = "Select * from V_ALARM where ����״̬ is null and  trunc(sysdate) - trunc(����ʱ��)<7 and (������� not like '%" + this.UserN + "%'  or ������� is null) and  ���ص�λ in ('" + this.UserRegion.Replace(",", "','") + "') order by ����ʱ��";
                    }

                    DataTable dt = GetTable(sqlstring);

                    if (dt.Rows.Count == 0) return;

                    this.label2.Text = "���� " + dt.Rows.Count.ToString() + " �����ڱ�����¼";

                    if (this.Visible == false)
                    {
                        this.Visible = true;
                        this.TopMost = true;
                    }

                    //if (this.Visible == true)
                    //    this.TopMost = true;
                    alarmcount = renum;
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "clKaKou-frmtip-04-��ѯ������Ϣ����ʾ");
                }
            //}
        }

        /// <summary>
        /// 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                this.timalarm.Interval = 1000;
                this.timalarm.Enabled = true;
                this.timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-05-timer1_Tick");
            }
        }


        /// <summary>
        /// ˫���¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void frmtip_DoubleClick(object sender, EventArgs e)
        {
            if (this.mapcontrol1.Map.Layers["KKLayer"] == null)
            {
                MessageBox.Show("���л����ΰ����ڹ���ģ���½��б�����Ϣ�鿴��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (falarm.Visible == true)
            {
                return;
            }

            try
            {
                string sqlstring = string.Empty;
                if (this.AlarmUser == "����" || this.UserRegion == "˳����")
                {
                    sqlstring = "Select �������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3 from V_ALARM where  (������� not like '%" + this.UserN + "%' or ������� is null)  and  trunc(sysdate) - trunc(����ʱ��)<7 and ����״̬ is null";
                }
                else if (this.AlarmUser == "�û�" && this.UserRegion != "")
                {
                    sqlstring = "Select �������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3 from V_ALARM where (������� not like '%" + this.UserN + "%' or ������� is null)' and  trunc(sysdate) - trunc(����ʱ��)<7 and ����״̬ is null and  ���ص�λ in ('" + this.UserRegion.Replace(",", "','") + "')";
                }
                else
                {
                    return;
                }

                DataTable dt = GetTable(sqlstring);

                WriteEditLog(sqlstring, "�鿴������¼", "V_ALARM");

                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows.Count > 1)
                    {
                        falarm.Visible = true;
                        falarm.dataGridView1.DataSource = dt;
                        falarm.dataGridView1.Refresh();
                        falarm.Text = "���� " + dt.Rows.Count.ToString() + " �����ڱ�����¼";
                    }
                    else if (dt.Rows.Count == 1)
                    {
                        falarm.Visible = false;
                        falarm.dataGridView1.DataSource = dt;
                        falarm.dataGridView1.Refresh();
                        falarm.Text = "���� " + dt.Rows.Count.ToString() + " �����ڱ�����¼";

                        falarm.OpenInfo(falarm.dataGridView1.Rows[0].Cells[0].Value.ToString());
                    }
                }
                else
                {
                    falarm.Visible = false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-06-˫���¼�");
            }
        }

        /// <summary>
        /// ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void timsound_Tick(object sender, EventArgs e)
        {
            try
            {
                PlayAlarmSound(Application.StartupPath + @"\ALARM.WAV");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-07-��������");
            }
        }

        /// <summary>
        /// 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void frmtip_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == true)
                {
                    PlayAlarmSound(Application.StartupPath + "\\ALARM.WAV");
                    this.timsound.Interval = 30 * 1000;
                    this.timsound.Enabled = true;
                }
                else
                {
                    this.timsound.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-frmtip_VisibleChanged-08-�������ʾ");
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
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
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
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// ��ȡScalar
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
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
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">����sql���</param>
        /// <param name="method">������ʽ</param>
        /// <param name="tablename">����</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + UserN + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�ΰ�����','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucKakou-61-��¼������¼");
            }
        }
    }
}