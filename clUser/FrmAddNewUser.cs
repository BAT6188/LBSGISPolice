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
                cmd.CommandText = "select ��ɫ�� from ��ɫ";
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
            //�������ļ��л�ȡ�����ַ���
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                string userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                string password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");
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

        private bool ����û���֤()
        {
            //��֤�û������Ϣ�Ƿ���ȷ
            try
            {
                if (this.cbUser.Text.Trim() == "")
                {
                    MessageBox.Show("�û�������Ϊ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                if (this.cbPassword1.Text.Trim() == "")
                {
                    MessageBox.Show("���벻��Ϊ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                if (this.cbPassword2.Text.Trim() == "")
                {
                    MessageBox.Show("���벻��Ϊ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                if (this.cbPassword1.Text.Trim() != this.cbPassword2.Text.Trim())
                {
                    MessageBox.Show("�����������벻һ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            //ִ������û�����
            if (this.����û���֤())
            {
                OracleConnection conn = new OracleConnection(getStrConn());
                conn.Open();
                OracleCommand cmd = new OracleCommand("select * from �û� where USERNAME='" + this.cbUser.Text.Trim().ToLower() + "'", conn);
                OracleDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.HasRows)
                    {
                        MessageBox.Show("�û��Ѵ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        MessageBox.Show("��ѡ������һ����ɫȨ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    roleArr = roleArr.Remove(roleArr.Length - 1);

                    OracleString rowId;
                    cmd = new OracleCommand("insert into �û�(ID,USERNAME,PASSWORD,��ʵ����,�û���λ,��ɫ,����) values((select max(id)+1 from �û�),'" + this.cbUser.Text.Trim().ToLower() + "','" + this.cbPassword1.Text.Trim() + "','" + this.cbName.Text.Trim() + "','" + this.cbDanwei.Text.Trim() + "','" + roleArr + "',0)", conn);
                    cmd.ExecuteOracleNonQuery(out rowId);
                    WriteEditLog("USERNAME=\"" + this.cbUser.Text.Trim().ToLower() + "\"");
                    MessageBox.Show("����û��ɹ�!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(ex.StackTrace);
                    MessageBox.Show("����û�ʧ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            //�رմ���
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

        //��¼������¼
        private void WriteEditLog(string uName)
        {
            OracleConnection Conn = new OracleConnection(getStrConn());
            try
            {
                Conn.Open();
                OracleCommand cmd;
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'Ȩ�޹���','"+uName+"','����û�')";
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