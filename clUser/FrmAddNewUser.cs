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
    public partial class FrmAddNewUser : Form
    {
        public string user = "";
        public FrmAddNewUser()
        {
            try
            {
                InitializeComponent();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
            }

            initRoleList();
        }

        private void initRoleList()
        {
            cbRole.Items.Clear();
            OracleConnection Conn = new OracleConnection(getStrConn());
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "select 角色名 from 角色";
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    cbRole.Items.Add(dr.GetValue(0).ToString().Trim());
                }
                Conn.Close();
            }
            catch
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
        }

        public string getStrConn()
        {
            //从配置文件中获取连接字符串
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
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        private bool 添加用户验证()
        {
            //验证用户填的信息是否正确
            try
            {
                if (this.cbUser.Text.Trim() == "")
                {
                    MessageBox.Show("用户名不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                if (this.cbPassword1.Text.Trim() == "")
                {
                    MessageBox.Show("密码不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                if (this.cbPassword2.Text.Trim() == "")
                {
                    MessageBox.Show("密码不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                if (this.cbPassword1.Text.Trim() != this.cbPassword2.Text.Trim())
                {
                    MessageBox.Show("两次密码输入不一样!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            //执行填加用户操作
            if (this.添加用户验证())
            {
                OracleConnection conn = new OracleConnection(getStrConn());
                conn.Open();
                OracleCommand cmd = new OracleCommand("select * from 用户 where USERNAME='" + this.cbUser.Text.Trim().ToLower() + "'", conn);
                OracleDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.HasRows)
                    {
                        MessageBox.Show("用户已存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.cbUser.Text = "";
                        return;
                    }

                    string roleArr = "";
                    for (int i = 0; i < cbRole.Items.Count; i++) {
                        if (cbRole.GetItemChecked(i)) {
                            roleArr += cbRole.Items[i].ToString() + ",";
                        }
                    }
                    if (roleArr == "")
                    {
                        MessageBox.Show("请选择至少一个角色权限!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    roleArr = roleArr.Remove(roleArr.Length - 1);

                    OracleString rowId;
                    cmd = new OracleCommand("insert into 用户(ID,USERNAME,PASSWORD,真实姓名,用户单位,角色,在线) values((select max(id)+1 from 用户),'" + this.cbUser.Text.Trim().ToLower() + "','" + this.cbPassword1.Text.Trim() + "','" + this.cbName.Text.Trim() + "','" + this.cbDanwei.Text.Trim() + "','" + roleArr + "',0)", conn);
                    cmd.ExecuteOracleNonQuery(out rowId);
                    WriteEditLog("USERNAME=\"" + this.cbUser.Text.Trim().ToLower() + "\"");
                    MessageBox.Show("添加用户成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(ex.StackTrace);
                    MessageBox.Show("添加用户失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    dr.Close();
                    cmd.Dispose();
                    conn.Close();
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //关闭窗口
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < cbRole.Items.Count; i++) {
                cbRole.SetItemChecked(i, checkBox1.Checked);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                cbPassword1.Text = "12345678";
                cbPassword2.Text = "12345678";
                cbPassword1.Enabled = false;
                cbPassword2.Enabled = false;
            }
            else {
                cbPassword1.Text = "";
                cbPassword2.Text = "";
                cbPassword1.Enabled = true;
                cbPassword2.Enabled = true;
            }
        }

        //记录操作记录
        private void WriteEditLog(string uName)
        {
            OracleConnection Conn = new OracleConnection(getStrConn());
            try
            {
                Conn.Open();
                OracleCommand cmd;
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'权限管理','"+uName+"','添加用户')";
                cmd = new OracleCommand(strExe, Conn);
                cmd.ExecuteNonQuery();

                Conn.Close();
            }
            catch
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }
    }
}