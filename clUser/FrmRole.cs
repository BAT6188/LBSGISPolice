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
    public partial class FrmRole : Form
    {
        public string[] conStr;
        public FrmRole()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void FrmRole_Load(object sender, EventArgs e)
        {
            try
            {
                ListQuyu.Items.Clear();
                ListQuyu.Items.Add("˳��������");
                comboBox1.Text = comboBox1.Items[0].ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "FrmRole_Load");
            }
        }

        /// <summary>
        /// �������ļ��л�ȡ�����ַ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <returns>�����ַ���</returns>
        private string getStrConn()
        {
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
                ExToLog(ex, "getStrConn");
                return "";
            }
        }

        /// <summary>
        /// �رմ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnCancel_Click");
            }
        }

        /// <summary>
        /// ��Ӱ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from ��ɫ where ��ɫ�� = '" + this.typeName.Text.Trim().ToLower() + "'");
                if (dr.HasRows)
                {
                    MessageBox.Show("�ý�ɫ�Ѵ��ڣ�������ѡ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.typeName.Text = "";
                    return;
                }
                if (this.typeName.Text == "")
                {
                    MessageBox.Show("��ɫ������Ϊ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;  
                }

                string quyuQX = "";
                for (int i = 0; i < this.ListQuyu.Items.Count; i++)
                {
                    if (this.ListQuyu.GetItemChecked(i))
                    {
                        quyuQX += ListQuyu.Items[i].ToString() + ",";
                    }
                }
                if (quyuQX == "")
                {
                    MessageBox.Show("��ѡ������һ������Ȩ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                quyuQX = quyuQX.Remove(quyuQX.Length - 1);

                string leixingQX = "";
                for (int i = 0; i < cbListModule.Items.Count; i++)
                {
                    if (cbListModule.GetItemChecked(i))
                    {
                       leixingQX += cbListModule.Items[i].ToString() + ",";
                    }
                }
                if (leixingQX == "")
                {
                    MessageBox.Show("��ѡ������һ������Ȩ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return; 
                }
                leixingQX = leixingQX.Remove(leixingQX.Length - 1);
                leixingQX = leixingQX.Replace("110", "llo");

                int maxId = CLC.DatabaseRelated.OracleDriver.OracleComScalar("select max(rownum) from ��ɫ");

                string cmdstr = "";
                OracleDataReader dtreader = CLC.DatabaseRelated.OracleDriver.OracleComReader("select max(rownum)+1 from ��ɫ");
                if (dtreader.HasRows)
                {
                    dtreader.Read();
                    if (dtreader.GetValue(0).ToString() == "")
                    {
                        cmdstr = "insert into ��ɫ (ID,��ɫ��,����Ȩ��,ģ��Ȩ��) values ('1','" + this.typeName.Text.Trim().ToLower() + "','" + quyuQX + "','" + leixingQX + "')";
                    }
                    else
                    {
                        cmdstr = "insert into ��ɫ (ID,��ɫ��,����Ȩ��,ģ��Ȩ��) values ('" + Convert.ToString(maxId + 1) + "','" + this.typeName.Text.Trim().ToLower() + "','" + quyuQX + "','" + leixingQX + "')";
                    }
                }
                CLC.DatabaseRelated.OracleDriver.OracleComRun(cmdstr);
                this.Rolelist.Items.Add(this.typeName.Text.Trim().ToLower());  // ����listbox   add by fisher in 09-12-22
                MessageBox.Show("��ӽ�ɫ�ɹ�!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dr.Close();
                dtreader.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "btnOK_Click");
                MessageBox.Show("��ӽ�ɫʧ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }            
        }

        /// <summary>
        /// ȫѡ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < cbListModule.Items.Count; i++)
                {
                    cbListModule.SetItemChecked(i, checkBox1.Checked);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkBox1_CheckedChanged");
            }
        }

        /// <summary>
        /// �л����򼶱�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListQuyu.Items.Clear();
                if (comboBox1.Text == "˳��������")
                {
                    ListQuyu.Items.Add("˳��������");
                }
                else if (comboBox1.Text == "�ɳ���")
                {
                    OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select �ɳ����� from �����ɳ���");
                    while (dr.Read())
                    {
                        ListQuyu.Items.Add(dr.GetValue(0).ToString().Trim());
                    }
                    dr.Close();
                }
                else
                {
                    OracleDataReader dr1 = CLC.DatabaseRelated.OracleDriver.OracleComReader("select �ж��� from �������ж�");
                    while (dr1.Read())
                    {
                        ListQuyu.Items.Add(dr1.GetValue(0).ToString().Trim());
                    }
                    dr1.Close();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "comboBox1_SelectedIndexChanged");
            }
        }

        /// <summary>
        /// ��ʼ��listbox
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void setListbox()
        {
            OracleDataReader dr = null;
            try
            {
                Rolelist.Items.Clear();
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                string sql = "select ��ɫ�� from ��ɫ order by ��ɫ��";
                dr = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        Rolelist.Items.Add(dr.GetValue(0).ToString().Trim());
                    }
                }
                dr.Dispose();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setListbox()");
            }
            finally
            {
                if (dr != null)
                {
                    dr.Dispose();
                }
            }
        }

        /// <summary>
        /// �л���ɫ��ʾ�ý�ɫ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void Rolelist_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from ��ɫ where ��ɫ�� = '" + this.Rolelist.Items[this.Rolelist.SelectedIndex].ToString() + "'";
                typeName.Text = this.Rolelist.Items[this.Rolelist.SelectedIndex].ToString();
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                setJuese(dt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Rolelist_SelectedIndexChanged()");
            }
        }

        /// <summary>
        /// ��ʼ�����б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="dt"></param>
        private void setJuese(DataTable dt)
        {
            try
            {
                checkBox1.Checked = false;
                for (int i = 0; i < cbListModule.Items.Count; i++)
                {
                    cbListModule.SetItemCheckState(i, CheckState.Unchecked);
                }

                if (dt.Rows[0]["����Ȩ��"].ToString() == "˳��������")
                {
                    comboBox1.Text = "˳��������"; 
                }
                else if (dt.Rows[0]["����Ȩ��"].ToString().IndexOf("�ж�") > -1)
                {
                    comboBox1.Text = "���ж�";
                }
                else
                {
                    comboBox1.Text = "�ɳ���";
                }

                for (int i = 0; i < ListQuyu.Items.Count; i++)
                {
                    ListQuyu.SetItemCheckState(i, CheckState.Unchecked); 
                }

                //��ȡ����Ȩ�޵���Ϣ�������б����ѡ��
                string quyuArr = dt.Rows[0]["����Ȩ��"].ToString();
                string[] quyuArr1 = quyuArr.Split(',');
                for (int i = 0; i < quyuArr1.Length; i++)
                {
                    for (int j = 0; j < ListQuyu.Items.Count; j++)
                    {
                        if (quyuArr1[i] == ListQuyu.Items[j].ToString())
                        {
                            ListQuyu.SetItemCheckState(j, CheckState.Checked);
                        }
                    }
                }

                //��ȡģ��Ȩ�޵���Ϣ�������б����ѡ��
                string mokArr = dt.Rows[0]["ģ��Ȩ��"].ToString();
                mokArr = mokArr.Replace("llo", "110");
                string[] mokArr1 = mokArr.Split(',');
                for (int i = 0; i < mokArr1.Length; i++)
                {
                    for (int j = 0; j < cbListModule.Items.Count; j++)
                    {
                        if (mokArr1[i] == cbListModule.Items[j].ToString())
                        {
                            cbListModule.SetItemCheckState(j, CheckState.Checked);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setJuese()");
            }
        }


        /// <summary>
        /// ���ݽ�ɫ����ȡ��ɫ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnLook_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from ��ɫ where ��ɫ�� = '" + this.typeName.Text.Trim() + "'";
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("û�иý�ɫ��");
                    this.InitialLists();
                    return;
                }
                setJuese(dt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnLook_Click()");
            }
        }

        /// <summary>
        /// �����ɫ�����س�ִ�в鿴��ɫ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void typeName_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    btnLook_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "typeName_KeyPress()");
            }
        }

        /// <summary>
        /// �����ɫ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "select * from ��ɫ where ��ɫ��='" + this.typeName.Text.Trim() + "'";
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql);
                if (!dr.HasRows)
                {
                    MessageBox.Show("û�д˽�ɫ��");
                    dr.Close();
                    return;
                }
                else
                {
                    string quyuArr = "";   //�������Ȩ���ַ���
                    for (int i = 0; i < ListQuyu.Items.Count; i++)
                    {
                        if (ListQuyu.GetItemChecked(i))
                        {
                            quyuArr += ListQuyu.Items[i].ToString() + ",";
                        }
                    }
                    if (quyuArr == "")
                    {
                        MessageBox.Show("������ѡ��һ������");
                        return;
                    }
                    quyuArr = quyuArr.Remove(quyuArr.LastIndexOf(','));

                    string mokArr = "";    //���ģ��Ȩ���ַ���
                    for (int i = 0; i < cbListModule.Items.Count; i++)
                    {
                        if (cbListModule.GetItemChecked(i))
                        {
                            mokArr += cbListModule.Items[i].ToString() + ",";
                        }
                    }
                    if (mokArr == "")
                    {
                        MessageBox.Show("������ѡ��һ��ģ��Ȩ��!");
                        return;
                    }
                    mokArr = mokArr.Remove(mokArr.LastIndexOf(','));
                    mokArr = mokArr.Replace("110", "llo");

                    string upsql = "update ��ɫ set ����Ȩ�� = '" + quyuArr + "',ģ��Ȩ�� = '" + mokArr + "' where ��ɫ�� = '" + this.typeName.Text.Trim() + "'";
                    CLC.DatabaseRelated.OracleDriver.OracleComRun(upsql);
                    MessageBox.Show("���³ɹ�!");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnSave_Click()");
            }
        }

        /// <summary>
        /// ɾ����ɫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnDel_Click(object sender, EventArgs e)
        {
            if (this.typeName.Text.Trim() == "")
            {
                MessageBox.Show("��ѡ����Ҫɾ���Ľ�ɫ!");
                return;
            }
            if (MessageBox.Show("ȷ��ɾ����ɫ:"+this.typeName.Text.Trim()+"?", "ȷ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            try
            {
                string qsql = "select * from ��ɫ where ��ɫ��='" + this.typeName.Text.Trim() + "'";
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader(qsql);
                if (!dr.HasRows)
                {
                    MessageBox.Show("û�д˽�ɫ��");
                    dr.Close();
                    return;
                }

                string sql = "delete from ��ɫ t where t.��ɫ�� = '" + this.typeName.Text.Trim() + "'";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
                this.InitialLists();
                MessageBox.Show("ɾ���ɹ�!");
                this.setListbox();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnDel_Click()");
            }
        }

        /// <summary>
        /// ��ʼ�����б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void InitialLists()      
        {
            try
            {
                for (int i = 0; i < ListQuyu.Items.Count; i++)
                {
                    ListQuyu.SetItemCheckState(i, CheckState.Unchecked);
                }
                for (int i = 0; i < cbListModule.Items.Count; i++)
                {
                    cbListModule.SetItemCheckState(i, CheckState.Unchecked);
                }
                this.typeName.Text = "";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitialLists");
            }
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clUser-FrmRole-" + sFunc);
        }
    }
}