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

        private MapControl mapcontrol1;    //地图控件
        private string[] ConStr;                 //数据库连接字符串组

        public string AlarmSys = string.Empty;
        public string AlarmUser = string.Empty;
        private string UserRegion = string.Empty;
        private string UserN = string.Empty;   //登陆用户名称
        private frmAlarm falarm = new frmAlarm();

        public Boolean isKakou = false;

        public frmtip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 系统初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
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
                writeToLog(ex, "clKaKou-frmtip-01-系统初始化");
            }
        }

        /// <summary>
        /// 窗体关闭
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void frmtip_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.Visible = false;
                e.Cancel = true;
                this.timalarm.Enabled = false;

                // 关闭一段时间后，自动弹出
                // 只有卡口窗体不可见并且对象为：模块 时才不会弹出。  判断 不是卡口模块，并且不是模块
                //if (isKakou == false && AlarmSys == "模块")
                if (AlarmSys == "模块" && isKakou ==false)
                {
                    this.timer1.Enabled = false;
                }
                else if ((AlarmSys == "模块" && isKakou == true) || (AlarmSys == "系统" && isKakou == false) || (AlarmSys == "系统" && isKakou == true))
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
                writeToLog(ex, "clKaKou-frmtip-02-窗体关闭");
            }
        }

        /// <summary>
        /// 传递参数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sqlstr">数据库连接字符组</param>
        /// <param name="mapc">地图控件</param>
        /// <param name="asys">报警系统设置</param>
        /// <param name="auer">报警对象设置</param>
        /// <param name="usrg">用户区域权限</param>
        /// <param name="usern">登陆用户名称</param>
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
                writeToLog(ex, "clKaKou-frmtip-03-传递参数");
            }
        }



        ///  <summary>  
        ///  用于播放音乐  
        ///  最后编辑人   李立
        ///  最后编辑时间  2011-1-28
        ///  </summary>    
        internal class Helpers
        {
            [Flags]
            public enum PlaySoundFlags : int
            {
                SND_SYNC = 0x0000,    /*  play  synchronously  (default)  */  //同步  
                SND_ASYNC = 0x0001,    /*  play  asynchronously  */  //异步  
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
        /// 播放音乐
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="strFileName">音乐文件地址</param>
        public static void PlayAlarmSound(string strFileName)
        {
            //调用PlaySound方法,播放音乐  
            try
            {
                Helpers.PlaySound(strFileName, IntPtr.Zero, Helpers.PlaySoundFlags.SND_ASYNC);
            }
            catch //(Exception ex)
            {
                //writeToLog(ex, "clKaKou-frmtip-PlayAlarmSound-播放音乐");
            }
        }


        private int alarmcount = 0;
        /// <summary>
        /// 查询报警信息并显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void timalarm_Tick(object sender, EventArgs e)
        {
            //if (this.Visible == false)
            //{
                try
                {
                    string sqlstring = string.Empty;
                    if (this.AlarmUser == "所有" || this.UserRegion == "顺德区")
                    {
                        sqlstring = "Select * from V_ALARM where 处理状态 is null  and  trunc(sysdate) - trunc(报警时间)<7 and (处理情况 not like '%"+ this.UserN+"%' or 处理情况 is null) order by 报警时间";
                    }
                    else if (this.AlarmUser == "用户")
                    {
                        sqlstring = "Select * from V_ALARM where 处理状态 is null and  trunc(sysdate) - trunc(报警时间)<7 and (处理情况 not like '%" + this.UserN + "%'  or 处理情况 is null) and  布控单位 in ('" + this.UserRegion.Replace(",", "','") + "') order by 报警时间";
                    }

                    DataTable dt = GetTable(sqlstring);

                    if (dt.Rows.Count == 0) return;

                    this.label2.Text = "共有 " + dt.Rows.Count.ToString() + " 条卡口报警记录";

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
                    writeToLog(ex, "clKaKou-frmtip-04-查询报警信息并显示");
                }
            //}
        }

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
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
        /// 双击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void frmtip_DoubleClick(object sender, EventArgs e)
        {
            if (this.mapcontrol1.Map.Layers["KKLayer"] == null)
            {
                MessageBox.Show("请切换至治安卡口功能模块下进行报警信息查看！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (falarm.Visible == true)
            {
                return;
            }

            try
            {
                string sqlstring = string.Empty;
                if (this.AlarmUser == "所有" || this.UserRegion == "顺德区")
                {
                    sqlstring = "Select 报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3 from V_ALARM where  (处理情况 not like '%" + this.UserN + "%' or 处理情况 is null)  and  trunc(sysdate) - trunc(报警时间)<7 and 处理状态 is null";
                }
                else if (this.AlarmUser == "用户" && this.UserRegion != "")
                {
                    sqlstring = "Select 报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3 from V_ALARM where (处理情况 not like '%" + this.UserN + "%' or 处理情况 is null)' and  trunc(sysdate) - trunc(报警时间)<7 and 处理状态 is null and  布控单位 in ('" + this.UserRegion.Replace(",", "','") + "')";
                }
                else
                {
                    return;
                }

                DataTable dt = GetTable(sqlstring);

                WriteEditLog(sqlstring, "查看报警记录", "V_ALARM");

                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows.Count > 1)
                    {
                        falarm.Visible = true;
                        falarm.dataGridView1.DataSource = dt;
                        falarm.dataGridView1.Refresh();
                        falarm.Text = "共有 " + dt.Rows.Count.ToString() + " 条卡口报警记录";
                    }
                    else if (dt.Rows.Count == 1)
                    {
                        falarm.Visible = false;
                        falarm.dataGridView1.DataSource = dt;
                        falarm.dataGridView1.Refresh();
                        falarm.Text = "共有 " + dt.Rows.Count.ToString() + " 条卡口报警记录";

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
                writeToLog(ex, "clKaKou-frmtip-06-双击事件");
            }
        }

        /// <summary>
        /// 播放声音
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void timsound_Tick(object sender, EventArgs e)
        {
            try
            {
                PlayAlarmSound(Application.StartupPath + @"\ALARM.WAV");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmtip-07-播放声音");
            }
        }

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
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
                writeToLog(ex, "clKaKou-frmtip-frmtip_VisibleChanged-08-窗体的显示");
            }
        }

        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 获取Scalar
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">操作sql语句</param>
        /// <param name="method">操作方式</param>
        /// <param name="tablename">表名</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + UserN + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'治安卡口','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucKakou-61-记录操作记录");
            }
        }
    }
}