using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace LBSgisPolice
{
    public partial class frmLogin : Form
    {
        public static string string用户名称 = "";
        public static string string辖区 = "顺德区";
        
        //以下14组变量用来存储当前用户对哪些派出所有操作权限 (added by fisher in 10-03-04)
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

        //以下14组变量用来存储当前用户对哪些中队有操作权限
        private string zdZH = "";
        private string zdAJ = "";
        private string zdCL = "";
        private string zdSP = "";
        private string zdZA = "";
        private string zdRK = "";
        private string zdFW = "";
        private string zdZG = "";
        private string zdGPS = "";        //GPS警员
        private string zd110 = "";        //110接警
        private string zdJSJ = "";
        private string zdYSJ = "";
        private string zdSB = "";
        private string zdQX = "";
        private string zdDC = "";

        //public string userType="";
        public string userName = "";
        public static DataTable temDt = null;
        public static DataTable temRegionDt = null;  // 用来存储查询区域（fisher）
        public static DataTable temEditDt = null;    // 用来存储编辑模块的权限（Lili）

        public static string region1 = "";       //用来存储具有编辑模块权限的派出所（Lili）
        public static string region2 = "";       //用来存储具有编辑模块权限的中队（Lili）

        public frmLogin()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 读取配置文件，设置数据库连接字符串
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <returns>数据库连接字符串</returns>
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);

                string connString = "user id = " + userid + ";data source = " + datasource + ";password =" + password;
                return connString;
            }
            catch( Exception ex) {
                ExToLog(ex, "读取配置文件");
                return "";
            }
        }

        /// <summary>
        /// 登录点击事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string strConn=getStrConn();
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
                        frmLogin.string用户名称 = this.textUserName.Text.Trim();
                        setTemDt(dr["角色"].ToString());　　　　　　　　// 主程序的权限
                        setTemEditDt(userName, dr["角色"].ToString());  // 编辑模块的权限
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
            catch(Exception ex) {
                ExToLog(ex,"登陆系统");
                MessageBox.Show("请检查网络,数据库连接,配置文件中的数据库配置!" + ex.Message);
            }
        }

        /// <summary>
        /// 取消登录，关闭窗体
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 创建编辑模块权限内存表 及 区域权限字符串
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="uname">用户名</param>
        /// <param name="roles">角色名</param>
        private void setTemEditDt(string uname, string roles)
        {
            try
            {
                temEditDt = new DataTable("temQuanX");
                DataColumn dc = new DataColumn("userNow");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                //列         edit by fisher in 10-02-26
                dc = new DataColumn("基础数据可编辑");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("业务数据可编辑");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("视频可编辑");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("辖区");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("└基础数据可删、改");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("└业务数据可删、改");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("可导出");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("可导入");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                //行
                DataRow dRow;
                dRow = temEditDt.NewRow();
                dRow[0] = uname;
                dRow[1] = "0";
                dRow[2] = "0";
                dRow[5] = "0";
                dRow[6] = "0";
                dRow[7] = "0"; 
                dRow[8] = "0";
                temEditDt.Rows.Add(dRow);

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
            catch { }
        }

        /// <summary>
        /// 创建模块权限内存表
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="roles">角色名</param>
        private void setTemDt(string roles)
        {
            //读取角色的类型,创建dt的表结构
            try
            {
                temDt = new DataTable("role");
                DataColumn dc = new DataColumn("region");
                //指定字段的数据类型，这步没有也不会出错
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                //列
                string[] func ={"综合查询","案件分析","车辆监控","视频监控","治安卡口","人口管理","房屋管理","直观指挥","GPS警员","llo接警","基础数据编辑","业务数据编辑","视频编辑","权限管理","可导出" };
                for (int i = 0; i < func.Length; i++)
                {
                    //创建并添加虚拟列到虚拟表中
                    dc = new DataColumn(func[i]);
                    //指定字段的数据类型，这步没有也不会出错
                    dc.DataType = System.Type.GetType("System.Int16");
                    temDt.Columns.Add(dc);
                }

                //行
                DataRow dRow;
                dRow = temDt.NewRow();
                dRow[0] = "顺德区公安";
                for (int i = 1; i <= func.Length; i++) {
                    dRow[i] = 0;
                }
                temDt.Rows.Add(dRow);
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select 派出所名 from 基层派出所 group by 派出所名");
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        dRow = temDt.NewRow();
                        dRow["region"] = dr.GetValue(0).ToString().Trim();
                        for (int i = 1; i <= func.Length; i++)
                        {
                            dRow[i] = 0;
                        }
                        temDt.Rows.Add(dRow);
                    }
                    dr.Close();
                }

                OracleDataReader dr1 = CLC.DatabaseRelated.OracleDriver.OracleComReader("select 中队名 from 基层民警中队 group by 中队名");
                if (dr1.HasRows)
                {
                    while (dr1.Read())
                    {
                        DataRow dRow1 = temDt.NewRow();
                        dRow1["region"] = dr1.GetValue(0).ToString().Trim();
                        for (int i = 1; i <= func.Length; i++)
                        {
                            dRow1[i] = 0;
                        }
                        temDt.Rows.Add(dRow1);
                    }
                    dr1.Close();
                }

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
                                    if (b[k].IndexOf("可删、改") < 0 && b[k].IndexOf("可导入") < 0)      //由于编辑模块以后要合并到主系统中，这里的接口设计先预留，等具体方案订立后再做处理（fisher in 10-03-01）
                                    {
                                        if (Convert.ToInt16(temDt.Rows[rIndex][b[k]]) == 0)
                                        {
                                            temDt.Rows[rIndex][b[k]] = 1;
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
                    }
                }
                //读取临时表temDt的内容，构建temRegionDt
                setRegionDt();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "设置权限临时表");
            }
        }

        /// <summary>
        /// 读取临时表temDt的内容，构建temRegionDt
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
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

                temRegionDt = new DataTable("popedom");
                DataColumn dc = new DataColumn("zone");
                dc.DataType = System.Type.GetType("System.String");
                temRegionDt.Columns.Add(dc);

                //列
                string[] func ={ "综合查询", "案件分析", "车辆监控", "视频监控", "治安卡口", "人口管理", "房屋管理", "直观指挥", "GPS警员","llo接警", "基础数据编辑","业务数据编辑","视频编辑", "权限管理" ,"可导出"};
                for (int i = 0; i < func.Length; i++)
                {
                    //创建并添加虚拟列到虚拟表中
                    dc = new DataColumn(func[i]);
                    //指定字段的数据类型，这步没有也不会出错
                    dc.DataType = System.Type.GetType("System.String");
                    temRegionDt.Columns.Add(dc);
                }

                //行
                DataRow drow = temRegionDt.NewRow();
                drow["zone"] = "派出所";
                temRegionDt.Rows.Add(drow);
                drow = temRegionDt.NewRow();
                drow["zone"] = "中队";
                temRegionDt.Rows.Add(drow);

                //更新temRegionDt表的内容
                temRegionDt.Rows[0]["综合查询"] = pcsZH;         temRegionDt.Rows[1]["综合查询"] = zdZH;
                temRegionDt.Rows[0]["案件分析"] = pcsAJ;         temRegionDt.Rows[1]["案件分析"] = zdAJ;
                temRegionDt.Rows[0]["车辆监控"] = pcsCL;         temRegionDt.Rows[1]["车辆监控"] = zdCL;
                temRegionDt.Rows[0]["视频监控"] = pcsSP;         temRegionDt.Rows[1]["视频监控"] = zdSP;
                temRegionDt.Rows[0]["治安卡口"] = pcsZA;         temRegionDt.Rows[1]["治安卡口"] = zdZA;
                temRegionDt.Rows[0]["人口管理"] = pcsRK;         temRegionDt.Rows[1]["人口管理"] = zdRK;
                temRegionDt.Rows[0]["房屋管理"] = pcsFW;         temRegionDt.Rows[1]["房屋管理"] = zdFW;
                temRegionDt.Rows[0]["直观指挥"] = pcsZG;         temRegionDt.Rows[1]["直观指挥"] = zdZG;
                temRegionDt.Rows[0]["GPS警员"] = pcsGPS;         temRegionDt.Rows[1]["GPS警员"] = zdGPS;
                temRegionDt.Rows[0]["llo接警"] = pcs110;         temRegionDt.Rows[1]["llo接警"] = zd110;    
                temRegionDt.Rows[0]["基础数据编辑"] = pcsJSJ;    temRegionDt.Rows[1]["基础数据编辑"] = zdJSJ;
                temRegionDt.Rows[0]["业务数据编辑"] = pcsYSJ;    temRegionDt.Rows[1]["业务数据编辑"] = zdYSJ;
                temRegionDt.Rows[0]["视频编辑"] = pcsSB;         temRegionDt.Rows[1]["视频编辑"] = zdSB;
                temRegionDt.Rows[0]["权限管理"] = pcsQX;         temRegionDt.Rows[1]["权限管理"] = zdQX;
                temRegionDt.Rows[0]["可导出"] = pcsDC;           temRegionDt.Rows[1]["可导出"] = zdDC;
            }
            catch(Exception ex)
            {
                ExToLog(ex,"设置用户权限");
            }
        }

        //  (fisher)
        /// <summary>
        /// 以下函数旨在初始化用户能够查询的辖区
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="mkStr">模块名称</param>
        /// <param name="pcsStr">派出所权限</param>
        /// <param name="zdStr">中队权限</param>
        private void initialStr(string mkStr,ref string pcsStr,ref string zdStr)  //传址调用
        {
            try
            {
                if (temDt.Select(mkStr + " = 1").Length > 0)
                {
                    if (temDt.Rows[0][mkStr].ToString() == "1")
                    {
                        pcsStr = "顺德区";
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)    //确定10个派出所所拥有的不同模块的权限
                        {
                            if (temDt.Rows[i][mkStr].ToString() == "1")
                            {
                                pcsStr += temDt.Rows[i]["region"] + ",";
                            }
                        }

                        if (pcsStr != "") { pcsStr = pcsStr.Remove(pcsStr.LastIndexOf(",")); }
                        for (int j = 11; j < temDt.Rows.Count; j++)    //确定n个中队所拥有的不同模块的权限
                        {
                            if (temDt.Rows[j][mkStr].ToString() == "1")
                            {
                                zdStr += temDt.Rows[j]["region"] + ",";
                            }
                        }
                        if (zdStr != "") { zdStr = zdStr.Remove(zdStr.LastIndexOf(",")); }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "初始化用户查询的辖区");
            }
        }

        /// <summary>
        /// 获取字符串P在用户权限内存表中所在的行号
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="p">p</param>
        /// <returns>行号</returns>
        private int getIndex(string p)
        {
            for (int i = 0; i < temDt.Rows.Count; i++)
            {
                if (temDt.Rows[i]["region"].ToString() == p)
                {
                    return i;
                }
            }
            return -1;
        }        

        /// <summary>
        /// 异常日志
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "Main-frmLogin-" + sFunc);
        }
    }
}