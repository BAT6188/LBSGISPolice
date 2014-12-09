using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGPSPolice
{
    public partial class frmGz : Form
    {
        private DataTable dt;
        public string[] ArrayName;
        public string[] _conStr;

        /// <summary>
        /// 构造函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dadt">数据源</param>
        /// <param name="AryName"></param>
        /// <param name="strCon">数据库连接参数</param>
        public frmGz(DataTable dadt, string[] AryName,string[] strCon)
        {
            InitializeComponent();

            try
            {
                this.dt = dadt;
                this.ArrayName = AryName;
                this._conStr = strCon;
            }
            catch (Exception ex) { ExToLog(ex, "构造函数"); }
        }


        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void frmGz_Load(object sender, EventArgs e)
        {
            try
            {
                this.comboBox1.Items.Clear();
                this.comboBox1.Items.Add("警力编号");
                this.comboBox1.Items.Add("设备编号");
                this.comboBox1.Text = this.comboBox1.Items[0].ToString();

                this.listBox1.Items.Clear();
                this.listBox2.Items.Clear();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {

                        string cn = dr[0].ToString();
                        Boolean isin = false;
                        for (int j = 0; j < ArrayName.Length; j++)
                        {
                            if (ArrayName[j] == cn)
                                isin = true;
                        }

                        if (isin == false)
                            this.listBox2.Items.Add(cn);
                    }
                }

                if (ArrayName.Length > 0)
                {
                    for (int i = 0; i < ArrayName.Length; i++)
                    {
                        if (ArrayName[i] != "")
                            this.listBox1.Items.Add(ArrayName[i]);
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "frmGz_Load"); }
        }

        /// <summary>
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayName = new string[10] { "", "", "", "", "", "", "", "", "", "" };

                for (int i = 0; i < this.listBox1.Items.Count; i++)
                {
                    ArrayName[i] = this.listBox1.Items[i].ToString();
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { ExToLog(ex, "btnOK_Click"); }
        }

        /// <summary>
        /// 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 添加跟踪警员项
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.listBox2.SelectedIndex < 0)
                {
                    MessageBox.Show("没有选择任何车辆信息", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (this.listBox1.Items.Count > 9)
                {
                    MessageBox.Show("监控目标不能超过10个", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string name = this.listBox2.SelectedItem.ToString();
                if (this.comboBox1.Text == "设备编号")
                {
                    name = GetIDFromDev(name);
                    MessageBox.Show("该设备编号对应的警员编号为:" + name, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

                for (int i = 0; i < this.listBox1.Items.Count; i++)
                {
                    if (this.listBox1.Items[i].ToString() == name)
                    {
                        MessageBox.Show("该警员已经被设置为监控目标", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }
                }

                this.listBox1.Items.Add(name);

            }
            catch (Exception ex) { ExToLog(ex, "button1_Click"); }
        }

        /// <summary>
        /// 移除跟踪警员项
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.listBox1.SelectedIndex < 0)
                {
                    MessageBox.Show("没有选择任何车辆信息", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.listBox1.Items.RemoveAt(this.listBox1.SelectedIndex);
            }
            catch (Exception ex) { ExToLog(ex, "button2_Click"); }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }


        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        private void 选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        /// <summary>
        /// 根据设备编号获取警力编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="id">设备编号</param>
        /// <returns>警力编号</returns>
        private string GetIDFromDev(string id)
        {
            try
            {
                string sql = "Select 警力编号 from gps警员 where 设备编号='" + id + "'";
                DataTable dt = GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex) { ExToLog(ex, "GetIDFromDev"); return ""; }
        }

        /// <summary>
        /// 查询按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = string.Empty;
                if (this.comboBox1.Text != "")
                {
                    if (this.textBox1.Text != "")
                        sql = "Select " + this.comboBox1.Text.Trim() + " from gps警员 where " + this.comboBox1.Text.Trim() + " like '%" + this.textBox1.Text.Trim() + "%' order by " + this.comboBox1.Text.Trim();
                    else
                        sql = "Select " + this.comboBox1.Text.Trim() + " from gps警员 order by " + this.comboBox1.Text.Trim(); ;

                    DataTable dt = GetTable(sql);

                    this.listBox2.Items.Clear();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            this.listBox2.Items.Add(dr[0].ToString());
                        }
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "GetIDFromDev"); }
        }

        private void listBox2_Click(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 选择要监控的警员
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    if (this.listBox2.SelectedIndex < 0)
                    {
                        MessageBox.Show("没有选择任何警员信息", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (this.listBox1.Items.Count > 9)
                    {
                        MessageBox.Show("监控目标不能超过10个", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string name = this.listBox2.SelectedItem.ToString();
                    if (this.comboBox1.Text == "设备编号")
                    {
                        name = GetIDFromDev(name);
                        MessageBox.Show("该设备编号对应的警员编号为:" + name, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }

                    for (int i = 0; i < this.listBox1.Items.Count; i++)
                    {
                        if (this.listBox1.Items[i].ToString() == name)
                        {
                            MessageBox.Show("该警员已经被设置为监控目标", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            return;
                        }
                    }

                    this.listBox1.Items.Add(name);

                }
                catch (Exception ex) { ExToLog(ex, "listBox2_MouseDown"); }
            }
        }

        /// <summary>
        /// 清除已跟踪警员
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.listBox1.Items.Count > 0)
                    this.listBox1.Items.Clear();
            }
            catch (Exception ex) { ExToLog(ex, "button4_Click"); }
        }

        /// <summary>
        /// 异常日志输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmGz-" + sFunc);
        }
    }
}