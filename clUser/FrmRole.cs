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
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void FrmRole_Load(object sender, EventArgs e)
        {
            try
            {
                ListQuyu.Items.Clear();
                ListQuyu.Items.Add("顺德区公安");
                comboBox1.Text = comboBox1.Items[0].ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "FrmRole_Load");
            }
        }

        /// <summary>
        /// 从配置文件中获取连接字符串毁
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
                return "";
            }
        }

        /// <summary>
        /// 关闭窗口
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
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
        /// 添加按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from 角色 where 角色名 = '" + this.typeName.Text.Trim().ToLower() + "'");
                if (dr.HasRows)
                {
                    MessageBox.Show("该角色已存在，请重新选择!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.typeName.Text = "";
                    return;
                }
                if (this.typeName.Text == "")
                {
                    MessageBox.Show("角色名不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show("请选择至少一个区域权限!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show("请选择至少一个类型权限!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return; 
                }
                leixingQX = leixingQX.Remove(leixingQX.Length - 1);
                leixingQX = leixingQX.Replace("110", "llo");

                int maxId = CLC.DatabaseRelated.OracleDriver.OracleComScalar("select max(rownum) from 角色");

                string cmdstr = "";
                OracleDataReader dtreader = CLC.DatabaseRelated.OracleDriver.OracleComReader("select max(rownum)+1 from 角色");
                if (dtreader.HasRows)
                {
                    dtreader.Read();
                    if (dtreader.GetValue(0).ToString() == "")
                    {
                        cmdstr = "insert into 角色 (ID,角色名,区域权限,模块权限) values ('1','" + this.typeName.Text.Trim().ToLower() + "','" + quyuQX + "','" + leixingQX + "')";
                    }
                    else
                    {
                        cmdstr = "insert into 角色 (ID,角色名,区域权限,模块权限) values ('" + Convert.ToString(maxId + 1) + "','" + this.typeName.Text.Trim().ToLower() + "','" + quyuQX + "','" + leixingQX + "')";
                    }
                }
                CLC.DatabaseRelated.OracleDriver.OracleComRun(cmdstr);
                this.Rolelist.Items.Add(this.typeName.Text.Trim().ToLower());  // 更新listbox   add by fisher in 09-12-22
                MessageBox.Show("添加角色成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dr.Close();
                dtreader.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "btnOK_Click");
                MessageBox.Show("添加角色失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }            
        }

        /// <summary>
        /// 全选框
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
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
        /// 切换区域级别
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListQuyu.Items.Clear();
                if (comboBox1.Text == "顺德区公安")
                {
                    ListQuyu.Items.Add("顺德区公安");
                }
                else if (comboBox1.Text == "派出所")
                {
                    OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select 派出所名 from 基层派出所");
                    while (dr.Read())
                    {
                        ListQuyu.Items.Add(dr.GetValue(0).ToString().Trim());
                    }
                    dr.Close();
                }
                else
                {
                    OracleDataReader dr1 = CLC.DatabaseRelated.OracleDriver.OracleComReader("select 中队名 from 基层民警中队");
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
        /// 初始化listbox
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void setListbox()
        {
            OracleDataReader dr = null;
            try
            {
                Rolelist.Items.Clear();
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                string sql = "select 角色名 from 角色 order by 角色名";
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
        /// 切换角色显示该角色信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void Rolelist_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from 角色 where 角色名 = '" + this.Rolelist.Items[this.Rolelist.SelectedIndex].ToString() + "'";
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
        /// 初始化各列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
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

                if (dt.Rows[0]["区域权限"].ToString() == "顺德区公安")
                {
                    comboBox1.Text = "顺德区公安"; 
                }
                else if (dt.Rows[0]["区域权限"].ToString().IndexOf("中队") > -1)
                {
                    comboBox1.Text = "民警中队";
                }
                else
                {
                    comboBox1.Text = "派出所";
                }

                for (int i = 0; i < ListQuyu.Items.Count; i++)
                {
                    ListQuyu.SetItemCheckState(i, CheckState.Unchecked); 
                }

                //读取区域权限的信息，并在列表框中选中
                string quyuArr = dt.Rows[0]["区域权限"].ToString();
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

                //读取模块权限的信息，并在列表框中选中
                string mokArr = dt.Rows[0]["模块权限"].ToString();
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
        /// 根据角色名获取角色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnLook_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from 角色 where 角色名 = '" + this.typeName.Text.Trim() + "'";
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("没有该角色！");
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
        /// 输入角色名按回车执行查看角色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
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
        /// 保存角色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "select * from 角色 where 角色名='" + this.typeName.Text.Trim() + "'";
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql);
                if (!dr.HasRows)
                {
                    MessageBox.Show("没有此角色！");
                    dr.Close();
                    return;
                }
                else
                {
                    string quyuArr = "";   //获得区域权限字符串
                    for (int i = 0; i < ListQuyu.Items.Count; i++)
                    {
                        if (ListQuyu.GetItemChecked(i))
                        {
                            quyuArr += ListQuyu.Items[i].ToString() + ",";
                        }
                    }
                    if (quyuArr == "")
                    {
                        MessageBox.Show("请至少选择一个区域！");
                        return;
                    }
                    quyuArr = quyuArr.Remove(quyuArr.LastIndexOf(','));

                    string mokArr = "";    //获得模块权限字符串
                    for (int i = 0; i < cbListModule.Items.Count; i++)
                    {
                        if (cbListModule.GetItemChecked(i))
                        {
                            mokArr += cbListModule.Items[i].ToString() + ",";
                        }
                    }
                    if (mokArr == "")
                    {
                        MessageBox.Show("请至少选择一个模块权限!");
                        return;
                    }
                    mokArr = mokArr.Remove(mokArr.LastIndexOf(','));
                    mokArr = mokArr.Replace("110", "llo");

                    string upsql = "update 角色 set 区域权限 = '" + quyuArr + "',模块权限 = '" + mokArr + "' where 角色名 = '" + this.typeName.Text.Trim() + "'";
                    CLC.DatabaseRelated.OracleDriver.OracleComRun(upsql);
                    MessageBox.Show("更新成功!");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnSave_Click()");
            }
        }

        /// <summary>
        /// 删除角色名称
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnDel_Click(object sender, EventArgs e)
        {
            if (this.typeName.Text.Trim() == "")
            {
                MessageBox.Show("请选择您要删除的角色!");
                return;
            }
            if (MessageBox.Show("确认删除角色:"+this.typeName.Text.Trim()+"?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            try
            {
                string qsql = "select * from 角色 where 角色名='" + this.typeName.Text.Trim() + "'";
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader(qsql);
                if (!dr.HasRows)
                {
                    MessageBox.Show("没有此角色！");
                    dr.Close();
                    return;
                }

                string sql = "delete from 角色 t where t.角色名 = '" + this.typeName.Text.Trim() + "'";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
                this.InitialLists();
                MessageBox.Show("删除成功!");
                this.setListbox();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnDel_Click()");
            }
        }

        /// <summary>
        /// 初始化各列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
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
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clUser-FrmRole-" + sFunc);
        }
    }
}