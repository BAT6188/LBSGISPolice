using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace LBSgisPoliceEdit
{
    public partial class FrmLogin : Form
    {
        public string userName = "";
        public static string region1 = "";    //用来存储具有权限的派出所
        public static string region2 = "";    //用来存储具有权限的中队
        public FrmLogin()
        {
            InitializeComponent();
            textUserName.Focus();
        }

        //读取配置文件，设置数据库连接字符串
        private string getStrConn()
        {
            string exePath = Application.StartupPath;
            CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
            string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
            string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
            string password = CLC.INIClass.IniReadValuePW("数据库", "密码");

            string connString = "user id = " + userid + ";data source = " + datasource + ";password =" + password;
            CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
            return connString;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string strConn = getStrConn();
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
                    if (this.textPW.Text == dr["PASSWORD"].ToString())
                    {
                        userName = this.textUserName.Text.Trim().ToLower();
                        setTemDt(userName,dr["角色"].ToString());
                        this.DialogResult = DialogResult.OK;
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
            }
            catch
            {               
                MessageBox.Show("请检查网络,配置文件,数据库连接!");
                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //创建编辑模块的内存表
        public static DataTable temDt = null;
        private void setTemDt(string uname,string roles)
        {
            try
            {
                temDt = new DataTable("temQuanX");
                DataColumn dc = new DataColumn("userNow");
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                //列         edit by fisher in 10-02-26
                dc = new DataColumn("基础数据可编辑");
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                dc = new DataColumn("业务数据可编辑");
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                dc = new DataColumn("视频可编辑");
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                dc = new DataColumn("辖区");
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                dc = new DataColumn("└基础数据可删、改");
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                dc = new DataColumn("└业务数据可删、改");
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);


                //行
                DataRow dRow;
                dRow = temDt.NewRow();
                dRow[0] = uname;
                dRow[1] = "0";
                dRow[2] = "0";
                dRow[5] = "0";
                dRow[6] = "0";
                temDt.Rows.Add(dRow);

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
                                temDt.Rows[0]["└基础数据可删、改"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("└业务数据可删、改") > -1)
                            {
                                temDt.Rows[0]["└业务数据可删、改"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("基础数据编辑") > -1)
                            {
                                temDt.Rows[0]["基础数据可编辑"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("业务数据编辑") > -1)
                            {
                                temDt.Rows[0]["业务数据可编辑"] = "1";
                            }
                            if (dr["模块权限"].ToString().IndexOf("视频编辑") > -1)
                            {
                                temDt.Rows[0]["视频可编辑"] = "1";
                            }
                            temDt.Rows[0]["辖区"] += dr["区域权限"].ToString() + ",";     //这里允许重复
                        }
                        dr.Close();
                    }
                }
                temDt.Rows[0]["辖区"] = temDt.Rows[0]["辖区"].ToString().Remove(temDt.Rows[0]["辖区"].ToString().LastIndexOf(','));
                string[] quyu = temDt.Rows[0]["辖区"].ToString().Split(',');
                string xiaqu = "";
                for (int i = 0; i < quyu.Length; i++)
                {
                    if (xiaqu.IndexOf(quyu[i]) < 0)   //不包含重复的字符串
                    {
                        xiaqu += quyu[i] + ",";
                    }
                }
                xiaqu = xiaqu.Remove(xiaqu.LastIndexOf(','));   //这里获得的辖区是没有重复区域的字符串
                temDt.Rows[0]["辖区"] = xiaqu;
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
            catch {}
        }
    }
}