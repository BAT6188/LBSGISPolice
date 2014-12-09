using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace clUser
{
    public partial class FrmChangeUser : Form
    {
        public static string string用户名称 = "";
        public static string string辖区 = "顺德区";

        //以下14组变量用来存储当前用户对派出所操作的权限 (added by fisher in 09-11-27)
        private string pcsZH = "";     //综合查询    
        private string pcsAJ = "";     //案件分析   
        private string pcsCL = "";     //车辆监控
        private string pcsSP = "";     //视频监控
        private string pcsZA = "";     //治安卡口
        private string pcsRK = "";     //人口管理
        private string pcsFW = "";     //房屋管理
        private string pcsZG = "";     //直观指挥
        private string pcsGPS = "";    //GPS警员
        private string pcs110 = "";    //110接警
        private string pcsJSJ = "";    //基础数据编辑
        private string pcsYSJ = "";    //业务数据编辑
        private string pcsSB = "";     //视频编辑
        private string pcsQX = "";     //权限管理
        private string pcsDC = "";     //可导出

        //以下14组变量用来存储当前用户对中队操作的权限
        private string zdZH = "";
        private string zdAJ = "";
        private string zdCL = "";
        private string zdSP = "";
        private string zdZA = "";
        private string zdRK = "";
        private string zdFW = "";
        private string zdZG = "";
        private string zdGPS = "";
        private string zd110 = "";    //110接警
        private string zdJSJ = "";
        private string zdYSJ = "";
        private string zdSB = "";
        private string zdQX = "";
        private string zdDC = "";

        public string[] conStr;
        public static string region1 = "";
        public static string region2 = "";
        public static DataTable temEditDt = null;
        public static DataTable temDt1 = null;
        public static DataTable temRegionDt1 = null;

        public string userName = "";
        /// <summary>
        /// 构造函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="temTB">派出所权限</param>
        /// <param name="temRgTB">中队权限</param>
        /// <param name="canStr">数据库连接参数</param>
        /// <param name="temEdDt">编辑模块权限</param>
        public FrmChangeUser(DataTable temTB, DataTable temRgTB, string[] canStr, DataTable temEdDt)
        {
            try
            {
                InitializeComponent();
                textUserName.Focus();
                temDt1 = temTB;
                temRegionDt1 = temRgTB;
                temEditDt = temEdDt;
                conStr = canStr;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "FrmChangeUser");
            }
        }

        /// <summary>
        /// 读取配置文件，设置数据库连接字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <returns>连接字符串</returns>
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");

                string connString = "user id = " + userid + ";data source = " + datasource + ";password =" + password;
                return connString;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getStrConn"); 
                return null;
            }
        }

        /// <summary>
        /// 登录按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string strConn = CLC.DatabaseRelated.OracleDriver.GetConString;
            if (strConn == "")
            {
                MessageBox.Show("读取配置文件时发生错误,请修改配置文件后重试!");
                return;
            }
            try
            {
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from 用户 where USERNAME='" + textUserName.Text.Trim().ToLower() + "'");
                if (dr.HasRows)
                {
                    dr.Read();
                    if (textPW.Text == dr["PASSWORD"].ToString())
                    {
                        this.DialogResult = DialogResult.OK;
                        userName = textUserName.Text.Trim();
                        string用户名称 = this.textUserName.Text.Trim();
                        setTemDt(dr["角色"].ToString());
                        setTemEditDt(userName, dr["角色"].ToString());
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("密码错误！");
                    }
                }
                else
                {
                    MessageBox.Show("此用户不存在！");
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnLogin_Click"); 
                MessageBox.Show("请检查网络,数据库连接,配置文件中的数据库配置!" + ex.Message);
            }
        }

        /// <summary>
        /// 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex) { ExToLog(ex, "btnCancel_Click"); }
        }

        /// <summary>
        /// 构建编辑模块权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="uname">用户名</param>
        /// <param name="roles">角色</param>
        private void setTemEditDt(string uname, string roles)
        {
            try
            {
                //重新初始化内存表
                temEditDt.Rows[0][0] = uname;
                temEditDt.Rows[0][1] = "0";
                temEditDt.Rows[0][2] = "0";
                temEditDt.Rows[0][3] = "0";
                temEditDt.Rows[0][4] = "";
                temEditDt.Rows[0][5] = "0";
                temEditDt.Rows[0][6] = "0";
                temEditDt.Rows[0][7] = "0";
                temEditDt.Rows[0][8] = "0";

                //读取角色，更新dt的内容
                OracleDataReader dr = null;
                string[] aRole = roles.Split(',');
                for (int i = 0; i < aRole.Length; i++)  //判断是否可编辑和可操作的辖区
                {
                    dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from 角色 where 角色名 = '" + aRole[i] + "'");
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            if (dr["模块权限"].ToString().IndexOf("└基础数据可删、改") > -1)
                            {
                                temEditDt.Rows[0]["└基础数据可删、改"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("└业务数据可删、改") > -1)
                            {
                                temEditDt.Rows[0]["└业务数据可删、改"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("基础数据编辑") > -1)
                            {
                                temEditDt.Rows[0]["基础数据可编辑"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("业务数据编辑") > -1)
                            {
                                temEditDt.Rows[0]["业务数据可编辑"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("视频编辑") > -1)
                            {
                                temEditDt.Rows[0]["视频可编辑"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("可导出") > -1)
                            {
                                temEditDt.Rows[0]["可导出"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("可导入") > -1)
                            {
                                temEditDt.Rows[0]["可导入"] = "1";
                            }
                            temEditDt.Rows[0]["辖区"] += dr["区域权限"].ToString() + ",";     //这里允许重复
                        }
                        dr.Close();
                    }
                }
                temEditDt.Rows[0]["辖区"] = temEditDt.Rows[0]["辖区"].ToString().Remove(temEditDt.Rows[0]["辖区"].ToString().LastIndexOf(','));
                string[] quyu = temEditDt.Rows[0]["辖区"].ToString().Split(',');
                string xiaqu = "";
                for (int i = 0; i < quyu.Length; i++)
                {
                    if (xiaqu.IndexOf(quyu[i]) < 0)   //不包含重复的字符串
                    {
                        xiaqu += quyu[i] + ",";
                    }
                }
                xiaqu = xiaqu.Remove(xiaqu.LastIndexOf(','));   //这里获得的辖区是没有重复区域的字符串
                temEditDt.Rows[0]["辖区"] = xiaqu;
                string[] quyu1 = xiaqu.Split(',');
                region1 = "";
                region2 = "";
                if (xiaqu.IndexOf("顺德区公安") > -1)
                {
                    region1 = "顺德区";
                }
                else
                {
                    for (int i = 0; i < quyu1.Length; i++)
                    {
                        if (quyu1[i].Length == 2)     // 派出所（只有两个字为派出所）
                        {
                            region1 += quyu1[i] + ",";
                        }
                        else　　　　　　　　　　　　  // 中队（中队不可能只有两个字）
                        {
                            region2 += quyu1[i] + ",";
                        }
                    }
                    if (region1 != "")
                    {
                        region1 = region1.Remove(region1.LastIndexOf(','));
                    }
                    if (region2 != "")
                    {
                        region2 = region2.Remove(region2.LastIndexOf(','));
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "setTemEditDt"); }
        }

        /// <summary>
        /// 构建用户权限内存表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="roles">角色</param>
        private void setTemDt(string roles)
        {
            //读取角色的类型,创建dt的表结构
            try
            {
                //重新初始化内存表
                for (int i = 0; i < temDt1.Rows.Count; i++)
                {
                    for (int j = 1; j < temDt1.Columns.Count; j++)
                    {
                        temDt1.Rows[i][j] = 0;
                    }
                }
                OracleDataReader dr = null;
                //读取角色的区域,更新dt的内容
                string[] aRole = roles.Split(',');
                for (int i = 0; i < aRole.Length; i++)
                {
                    dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from 角色 where 角色名 = '" + aRole[i] + "'");
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            string[] a = dr["区域权限"].ToString().Split(',');
                            for (int j = 0; j < a.Length; j++)
                            {
                                int rIndex = getIndex(a[j]);
                                string[] b = dr["模块权限"].ToString().Split(',');
                                for (int k = 0; k < b.Length; k++)
                                {
                                    //b[k] = b[k].Replace("110", "llo");
                                    if (b[k].IndexOf("可删、改") < 0 && b[k].IndexOf("可导入") < 0)     //由于编辑模块以后要合并到主系统中，这里的接口设计先预留，等具体方案订立后再做处理（fisher in 10-03-01）
                                    {
                                        if (Convert.ToInt16(temDt1.Rows[rIndex][b[k]]) == 0)
                                        {
                                            temDt1.Rows[rIndex][b[k]] = 1;
                                        }
                                    }
                                }
                            }
                        }
                        dr.Close();
                    }
                    else
                    {
                        dr.Close();
                        return; 
                    }
                }
                //读取临时表temDt的内容，构建temRegionDt
                setRegionDt();
            }
            catch (Exception ex) { ExToLog(ex, "setTemDt"); }
        }

        /// <summary>
        /// 读取临时表temDt的内容，构建temRegionDt
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void setRegionDt()
        {
            try
            {
                initialStr("综合查询", ref pcsZH, ref zdZH);
                initialStr("案件分析", ref pcsAJ, ref zdAJ);
                initialStr("车辆监控", ref pcsCL, ref zdCL);
                initialStr("视频监控", ref pcsSP, ref zdSP);
                initialStr("治安卡口", ref pcsZA, ref zdZA);
                initialStr("人口管理", ref pcsRK, ref zdRK);
                initialStr("房屋管理", ref pcsFW, ref zdFW);
                initialStr("直观指挥", ref pcsZG, ref zdZG);
                initialStr("GPS警员", ref pcsGPS, ref zdGPS);
                initialStr("llo接警", ref pcs110, ref zd110);
                initialStr("基础数据编辑", ref pcsJSJ, ref zdJSJ);
                initialStr("业务数据编辑", ref pcsYSJ, ref zdYSJ);
                initialStr("视频编辑", ref pcsSB, ref zdSB);
                initialStr("权限管理", ref pcsQX, ref zdQX);
                initialStr("可导出", ref pcsDC, ref zdDC);

                //更新temRegionDt表的内容
                temRegionDt1.Rows[0]["综合查询"] = pcsZH;         temRegionDt1.Rows[1]["综合查询"] = zdZH;
                temRegionDt1.Rows[0]["案件分析"] = pcsAJ;         temRegionDt1.Rows[1]["案件分析"] = zdAJ;
                temRegionDt1.Rows[0]["车辆监控"] = pcsCL;         temRegionDt1.Rows[1]["车辆监控"] = zdCL;
                temRegionDt1.Rows[0]["视频监控"] = pcsSP;         temRegionDt1.Rows[1]["视频监控"] = zdSP;
                temRegionDt1.Rows[0]["治安卡口"] = pcsZA;         temRegionDt1.Rows[1]["治安卡口"] = zdZA;
                temRegionDt1.Rows[0]["人口管理"] = pcsRK;         temRegionDt1.Rows[1]["人口管理"] = zdRK;
                temRegionDt1.Rows[0]["房屋管理"] = pcsFW;         temRegionDt1.Rows[1]["房屋管理"] = zdFW;
                temRegionDt1.Rows[0]["直观指挥"] = pcsZG;         temRegionDt1.Rows[1]["直观指挥"] = zdZG;
                temRegionDt1.Rows[0]["GPS警员"] = pcsGPS;         temRegionDt1.Rows[1]["GPS警员"] = zdGPS;
                temRegionDt1.Rows[0]["llo接警"] = pcs110;         temRegionDt1.Rows[1]["llo接警"] = zd110;
                temRegionDt1.Rows[0]["基础数据编辑"] = pcsJSJ;    temRegionDt1.Rows[1]["基础数据编辑"] = zdJSJ;
                temRegionDt1.Rows[0]["业务数据编辑"] = pcsYSJ;    temRegionDt1.Rows[1]["业务数据编辑"] = zdYSJ;
                temRegionDt1.Rows[0]["视频编辑"] = pcsSB;         temRegionDt1.Rows[1]["视频编辑"] = zdSB;
                temRegionDt1.Rows[0]["权限管理"] = pcsQX;         temRegionDt1.Rows[1]["权限管理"] = zdQX;
                temRegionDt1.Rows[0]["可导出"] = pcsDC;           temRegionDt1.Rows[1]["可导出"] = zdDC;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setRegionDt");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 初始化用户能够查询的辖区
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="mkStr">模块名称</param>
        /// <param name="pcsStr">派出所权限</param>
        /// <param name="zdStr">中队权限</param>
        private void initialStr(string mkStr, ref string pcsStr, ref string zdStr)  //传址调用
        {
            try
            {
                if (temDt1.Select(mkStr + " = 1").Length > 0)
                {
                    if (temDt1.Rows[0][mkStr].ToString() == "1")
                    {
                        pcsStr = "顺德区";
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            if (temDt1.Rows[i][mkStr].ToString() == "1")
                            {
                                pcsStr += temDt1.Rows[i]["region"] + ",";
                            }
                        }

                        if (pcsStr != "") { pcsStr = pcsStr.Remove(pcsStr.LastIndexOf(",")); }
                        for (int j = 11; j < temDt1.Rows.Count; j++)
                        {
                            if (temDt1.Rows[j][mkStr].ToString() == "1")
                            {
                                zdStr += temDt1.Rows[j]["region"] + ",";
                            }
                        }
                        if (zdStr != "") { zdStr = zdStr.Remove(zdStr.LastIndexOf(",")); }
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "initialStr"); }
        }

        /// <summary>
        /// 找到权限在temDt1中的位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="p">权限名称</param>
        /// <returns>位置</returns>
        private int getIndex(string p)
        {
            try
            {
                for (int i = 0; i < temDt1.Rows.Count; i++)
                {
                    if (temDt1.Rows[i]["region"].ToString() == p)
                    {
                        return i;
                    }
                }
                return -1;
            }
            catch (Exception ex) { ExToLog(ex, "getIndex"); return -1; }
        }

        /// <summary>
        /// 异常日志输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clUser-FrmChangeUser-" + sFunc);
        }
    }
}