using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;

namespace clUser
{
    public partial class FrmChangePassword : Form
    {
        public string[] conStr;
        public FrmChangePassword()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取连接字符串信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <returns>连接字符串</returns>
        public string getStrConn()
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
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            //执生行更改密码的操作
            CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);

            OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from 用户 where USERNAME='" + cbUser.Text.Trim().ToLower() + "'");
            try
            {
                if (dr.HasRows)
                {
                    dr.Read();
                    if (dr["PASSWORD"].ToString() == textPW.Text)
                    {
                        if (textPWNew.Text != textPWNewAgain.Text)
                        {
                            MessageBox.Show("两次密码输入不一样!");
                            textPWNew.Text = "";
                            textPWNewAgain.Text = "";
                            return;
                        }

                        CLC.DatabaseRelated.OracleDriver.OracleComRun("update 用户 set PASSWORD='" + textPWNew.Text.Trim() + "' where USERNAME='" + cbUser.Text.Trim().ToLower() + "'");
                        MessageBox.Show("密码更改成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("原密码错误!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        textPW.Text = "";
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("此用户不存在，不能更改!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                WriteEditLog();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "buttonOk_Click");
                MessageBox.Show("错误，角色未更改！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                if (dr != null)
                    dr.Close();
            }
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void WriteEditLog()
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + cbUser.Text + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'权限管理','用户','更改密码')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex) { ExToLog(ex, "WriteEditLog"); }
        }

        /// <summary>
        /// 异常日志
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