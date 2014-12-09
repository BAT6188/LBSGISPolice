using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice
{
    public partial class frmEditLog : Form
    {
        private string[] strConn;

        /// <summary>
        /// 构造函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="s">数据连接参数</param>
        public frmEditLog(string[] s)
        {
            InitializeComponent();
            strConn = s;
            InitializeUser();
            comboBoxModule.Text = comboBoxModule.Items[0].ToString();
        }

        /// <summary>
        /// 初始化操作记录查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void InitializeUser()
        {
            try
            {
                string strExp = "select username from 用户";
                DataTable dt = GetTable(strExp);
                textBoxUser.Items.Clear();
                if (dt.Rows.Count>0)
                {
                    textBoxUser.Items.Add("全部");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        textBoxUser.Items.Add(dt.Rows[i][0].ToString());
                    }
                    textBoxUser.Text = textBoxUser.Items[0].ToString();
                }

                dateFrom.Text = DateTime.Now.ToString();
                dateTo.Text = DateTime.Now.ToString();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "InitializeUser");
            }
        }

        /// <summary>
        /// 复选框选中则起用时间条件 否则 不用时间做为条件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dateFrom.Enabled = checkBox1.Checked;
            timeFrom.Enabled = checkBox1.Checked;
            dateTo.Enabled = checkBox1.Checked;
            timeTo.Enabled = checkBox1.Checked;
        }

        /// <summary>
        /// 点击确定生成sql语句查出结果
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxUser.Text.Trim() == "") {
                MessageBox.Show("请输入用户名!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                this.Cursor = Cursors.WaitCursor;
                string strExp = "select * from 操作记录";
                if (textBoxUser.Text != "全部") {
                    strExp += " where 用户名='" + textBoxUser.Text.Trim() + "'";
                }
                if (comboBoxModule.Text != "全部") {
                    if (strExp.IndexOf("where") > 0)
                        strExp += " and 功能模块 like '" + comboBoxModule.Text + "%'";
                    else
                        strExp += " where 功能模块 like '" + comboBoxModule.Text + "%'";
                }

                if (checkBox1.Checked)
                {
                    string d1 = dateFrom.Value.ToShortDateString() + " " + timeFrom.Value.ToShortTimeString();
                    string d2 = dateTo.Value.ToShortDateString() + " " + timeTo.Value.ToShortTimeString();
                    if (strExp.IndexOf("where") > 0)
                        strExp += " and 时间>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and 时间<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')";
                    else
                        strExp += " where 时间>=to_date('" + d1 + "','yyyy-mm-dd hh24:mi:ss') and 时间<to_date('" + d2 + "','yyyy-mm-dd hh24:mi:ss')";
                }
                strExp += " order by 时间 desc";
                DataTable dt = GetTable(strExp);
                if (dt == null || dt.Rows.Count < 1)
                {
                    MessageBox.Show("无编辑记录!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridView1.DataSource = null;
                    this.Cursor = Cursors.Default;
                    return;
                }

                dataGridView1.DataSource = dt;

                for (int i = 0; i < dataGridView1.Rows.Count; i++) {
                    if (i % 2 == 1)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                this.Cursor = Cursors.Default;
            }
            catch( Exception ex) {
                this.Cursor = Cursors.Default;
                ExToLog(ex,"查询");
            }
        }

        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(strConn[0], strConn[1], strConn[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "Main-frmEditLog-" + sFunc);
        }
    }
}