using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.IO;
using LBSDataGuide;

namespace clUser
{
    public partial class FrmManager : Form
    {
        public string user = "";
        public string[] conStr;
        public FrmManager()//构造函数
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void initRoleList()
        {
            cbRole.Items.Clear();
            try
            { 
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select 角色名 from 角色");
                while (dr.Read())
                {
                    cbRole.Items.Add(dr.GetValue(0).ToString().Trim());
                } 
                dr.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "initJiezhenList");
            }        
        }

        /// <summary>
        /// 得到数据库连接字符串
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
        /// 更改权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (this.cbUser.Text.Trim().ToLower() == "admin")
            {
                MessageBox.Show("不能更改此用户的权限.");
                return;
            }
            OracleDataReader dr = null;
            try
            {
                string sql2 = "select * from 用户 where USERNAME='" + this.cbUser.Text.Trim().ToLower() + "'";
                dr = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql2);
                if (!dr.HasRows)
                {
                    MessageBox.Show("用户不存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if(this.添加用户验证())    //edit by fisher in 09-12-22
                {                    
                    string roleArr = "";
                    for (int i = 0; i < cbRole.Items.Count; i++)
                    {
                        if (cbRole.GetItemChecked(i))
                        {
                            roleArr += cbRole.Items[i].ToString() + ",";
                        }
                    }
                    if (roleArr == "")
                    {
                        MessageBox.Show("请选择至少一个角色权限!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    roleArr = roleArr.Remove(roleArr.Length - 1);
                    string[] roleStr = roleArr.Split(',');

                    if (roleStr.Length > 10)
                    {
                        DialogResult result = MessageBox.Show("该用户拥有的角色超过10个，可能导致其使用系统时产生不必要的影响!\n点击“确定”可重新定义。", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                        if (result == DialogResult.OK)
                            return;
                    }
                   
                    //int isExport = 0;      //判断是否有导出权限
                    //if (cbISExport.Checked) isExport = 1;

                    string sql = "update 用户　set  password = '" + this.cbPassword1.Text.Trim() + "' ,真实姓名='" + this.cbName.Text.Trim() + "' ,用户单位='" + this.cbDanwei.Text.Trim() + "' ,联系电话 ='" + this.cbPhone.Text.Trim() + "' ,角色='" + roleArr + "',职务='" + this.tbZhiWu.Text.Trim() + "' where USERNAME='" + this.cbUser.Text.Trim().ToLower() + "'  ";
                    CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
                    WriteEditLog(sql,"更改用户权限");
                    MessageBox.Show("更改成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "buttonSave_Click");
            }
            finally
            {
                if (dr != null)
                {
                    dr.Close();
                }
            }          
        }

        /// <summary>
        /// 查看权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void buttonQXGL_Click(object sender, EventArgs e)
        {
            this.userSearch(cbUser);
        }

        /// <summary>
        /// 根据文本框值查出相关内容
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="useText">文本框</param>
        private void userSearch(TextBox useText)
        {
            string sql = "";
            try
            {
                DataTable dt = new DataTable();
                if (useText == cbUser)
                {
                    sql = "select  *  from  用户 where USERNAME='" + this.cbUser.Text.Trim().ToLower() + "' ";
                }
                if (useText == cbName)
                {
                    sql = "select * from 用户 where 真实姓名 ='" + this.cbName.Text.Trim() + "'";
                }
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                if (dt.Rows.Count==0)
                {
                    MessageBox.Show("用户不存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.cbUser.Text = "";
                    InitialfrmNow();
                    return;
                }

                //设置用户列表中相应的用户呈被选中状态
                for (int i = 0; i < dataGridView1.Rows.Count;i++ )
                {
                    if (dt.Rows[0]["username"].ToString() == dataGridView1.Rows[i].Cells[0].Value.ToString())
                    {
                        dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                        break;
                    }
                }

                setQuanxian(dt); //根据数据库中的配置设置权限
                WriteEditLog(sql, "查询用户");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "userSearch");
            }
        }

        /// <summary>
        /// 根据数据库中的配置设置权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="dt">用户</param>
        private void setQuanxian(DataTable dt)
        {
            //设置权限
            try
            {
                InitialfrmNow(); //初始化窗体  
                if (dt.Rows[0]["角色"]==null || dt.Rows[0]["角色"].ToString() == "") {
                    //MessageBox.Show();
                    return;
                }

                string JueseArr = dt.Rows[0]["角色"].ToString();
                string[] arrRole = JueseArr.Split(',');
                for (int i = 0; i < cbRole.Items.Count; i++)
                {
                    for (int j = 0; j < arrRole.Length; j++)
                    {
                        if (arrRole[j].ToString() == cbRole.Items[i].ToString())
                        {
                            this.cbRole.SetItemCheckState(i, CheckState.Checked);
                        }
                    }
                }

                //真实姓名,用户单位
                this.cbUser.Text = dt.Rows[0]["username"].ToString();
                this.cbName.Text = dt.Rows[0]["真实姓名"].ToString();
                this.cbDanwei.Text = dt.Rows[0]["用户单位"].ToString();
                //if (Convert.ToInt16(dt.Rows[0]["导出"]) == 1)
                //{
                //    cbISExport.Checked = true;
                //}
                //else {
                //    cbISExport.Checked = false;
                //}
                //密码，联系电话   fisher in 09-12-21
                this.cbPassword1.Text = dt.Rows[0]["password"].ToString();
                this.cbPassword2.Text = dt.Rows[0]["password"].ToString();
                this.cbPhone.Text = dt.Rows[0]["联系电话"].ToString();
                this.tbZhiWu.Text = dt.Rows[0]["职务"].ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setQuanxian");
            }
        }

        /// <summary>
        /// 填充DdataGridView
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        public void setDataGridView()
        {

            //查询数据据库,得到数据户
            try
            {
                DataTable dt = new DataTable();
                string sql = "select  USERNAME as 用户名,用户单位  from  用户  order by USERNAME";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                this.dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setDataGridView");
            }
        }

        /// <summary>
        /// 双击列表显示该条记录详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select  *  from  用户 where USERNAME='" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString().Trim() + "' ";
                this.cbUser.Text = this.dataGridView1.CurrentRow.Cells[0].Value.ToString().Trim().ToLower();
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                if (dt.Rows.Count==0)
                {
                    MessageBox.Show("用户不存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.cbUser.Text = "";
                    return;
                }
                setQuanxian(dt);  //根据数据库中的配置设置权限
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellMouseClick");
            }
        }

        /// <summary>
        /// 删除用户
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认删除?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            if (this.cbUser.Text.Trim().ToLower() == "admin")
            {
                MessageBox.Show("不能删除此用户.","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                return;
            }
            OracleDataReader dr = null;
            try
            {
                string sql = "select  *  from  用户 where USERNAME='" + this.cbUser.Text.Trim().ToLower()+ "' ";
                dr = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql);
                if (!dr.HasRows)
                {
                    MessageBox.Show("用户不存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.cbUser.Text = "";
                    return;
                }
                else
                {
                    sql = "delete from 用户  where USERNAME='" + this.cbUser.Text.Trim().ToLower() + "' ";
                    CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
                    MessageBox.Show("删除用户成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.setDataGridView();
                }

                //清空用户名和功能列表
                InitialfrmNow(); //初始化窗体             
                this.cbUser.Text = "";
                WriteEditLog(sql, "删除用户");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "buttonDelete_Click");
            }
            finally
            {
                if (dr != null)
                {
                    dr.Close();
                }
            }
        }

        /// <summary>
        /// 用户名文本框回车执行
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void cbUser_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    userSearch(cbUser);
                }
            }
            catch (Exception ex) { ExToLog(ex, "cbUser_KeyPress"); }
        }

        /// <summary>
        /// 全选按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < cbRole.Items.Count; i++)
                {
                    cbRole.SetItemChecked(i, checkBox1.Checked);
                }
            }
            catch (Exception ex) { ExToLog(ex, "checkBox1_CheckedChanged"); }
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="sql">操作SQL</param>
        /// <param name="method"></param>
        private void WriteEditLog(string sql,string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'权限管理','用户:"+sql.Replace('\'','"')+"','"+method+"')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "WriteEditLog");
            }
        }

        /// <summary>
        /// 根据真实姓名查找
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRealName_Click(object sender, EventArgs e)
        {
            this.userSearch(cbName);
        }

        /// <summary>
        /// 初始化整个窗体里的对象
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void InitialfrmNow()
        {
            //清空用户名和功能列表
            //this.cbUser.Text = "";
            try
            {
                this.checkBox1.CheckState = CheckState.Unchecked;
                this.checkBox2.CheckState = CheckState.Unchecked;
                this.cbPassword1.Text = "";
                this.cbPassword2.Text = "";
                this.cbDanwei.Text = "";
                this.cbName.Text = "";
                this.cbPhone.Text = "";
                this.tbZhiWu.Text = "";
                for (int i = 0; i < cbRole.Items.Count; i++)
                {
                    cbRole.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitialfrmNow");
            }
        }

        /// <summary>
        /// 添加用户信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            //执行填加用户操作
            if (this.添加用户验证())
            {
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from 用户 where USERNAME='" + this.cbUser.Text.Trim().ToLower() + "'");

                try
                {
                    if (dr.HasRows)
                    {
                        MessageBox.Show("用户已存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.cbUser.Text = "";
                        return;
                    }

                    string roleArr = "";
                    for (int i = 0; i < cbRole.Items.Count; i++)
                    {
                        if (cbRole.GetItemChecked(i))
                        {
                            roleArr += cbRole.Items[i].ToString() + ",";
                        }
                    }
                    if (roleArr == "")
                    {
                        MessageBox.Show("请选择至少一个角色权限!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    roleArr = roleArr.Remove(roleArr.Length - 1);
                    string[] roleStr = roleArr.Split(',');
                    if (roleStr.Length > 10)
                    {
                        DialogResult result = MessageBox.Show("该用户拥有的角色超过10个，可能导致其使用系统时产生不必要的影响!\n点击“确定”可重新定义。", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                        if (result == DialogResult.OK)
                            return;
                    }

                    //int isExport = 0;      //判断是否有导出权限
                    //if (cbISExport.Checked) isExport = 1;
                    CLC.DatabaseRelated.OracleDriver.OracleComRun("insert into 用户(ID,USERNAME,PASSWORD,真实姓名,用户单位,角色,在线,联系电话,职务) values((select count(*)+1 from 用户),'" + this.cbUser.Text.Trim().ToLower() + "','" + this.cbPassword1.Text.Trim() + "','" + this.cbName.Text.Trim() + "','" + this.cbDanwei.Text.Trim() + "','" + roleArr + "',0," + "'" + this.cbPhone.Text.Trim() + "','" + this.tbZhiWu.Text.Trim() + "')");
                    //WriteEditLog("USERNAME=\"" + this.cbUser.Text.Trim().ToLower() + "\"");
                    MessageBox.Show("添加用户成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.setDataGridView();
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "buttonOk_Click");
                    MessageBox.Show(ex.Message+"添加用户失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    if (dr != null)
                        dr.Close();
                }
            }
        }

        /// <summary>
        /// 选中控件则使用默认密码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBox2.Checked)
                {
                    cbPassword1.Text = "12345678";
                    cbPassword2.Text = "12345678";
                    cbPassword1.Enabled = false;
                    cbPassword2.Enabled = false;
                }
                else
                {
                    cbPassword1.Text = "";
                    cbPassword2.Text = "";
                    cbPassword1.Enabled = true;
                    cbPassword2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkBox2_CheckedChanged");
            }
        }

        /// <summary>
        /// 用户验证
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <returns>布尔值（true-验证成功 false-验证失败）</returns>
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
                ExToLog(ex, "添加用户验证");
                return false;
            }
        }

        /// <summary>
        /// 批量用户导入
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnInData_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "请选择将导入的EXCEL文件路径";
                ofd.Filter = "Excel文档(*.xls)|*.xls";

                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
                {
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;
                    int dataCount = dg.InData(ofd.FileName, "用户");
                    this.Cursor = Cursors.Default;
                    if (dataCount != 0)
                    {
                        MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnInData_Click");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 用户导出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnOutData_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "请选择将导出的EXCEL文件存放路径";
                sfd.Filter = "Excel文档(*.xls)|*.xls";
                sfd.FileName = "EXP" + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                {
                    if (sfd.FileName.LastIndexOf(".xls") <= 0)
                    {
                        sfd.FileName = sfd.FileName + ".xls";
                    }
                    string fileName = sfd.FileName;
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;

                    if (dg.OutData(fileName, "用户"))
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("导出Excel完成!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("导出Excel失败!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnOutData_Click");
                this.Cursor = Cursors.Default;
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
            CLC.BugRelated.ExceptionWrite(ex, "clUser-FrmManager-" + sFunc);
        }
    }
}