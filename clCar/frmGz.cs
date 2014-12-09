using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace clCar
{
    public partial class frmGz : Form
    {
        private DataTable dt;
        public string[] ArrayName;
        public string[] _conStr;


        public frmGz(DataTable dadt,string[] AryName,string[] strCon) 
        {
            InitializeComponent();

            try
            {
                this.dt = dadt;
                this.ArrayName = AryName;
                this._conStr = strCon;
            }
            catch (Exception ex) { writeToLog(ex, "构造函数"); }
        }


        private void frmGz_Load(object sender, EventArgs e)
        {
            try
            {
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
            catch (Exception ex) { writeToLog(ex, "frmGz_Load"); }
        }

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
            catch (Exception ex) { writeToLog(ex, "btnOK_Click"); }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

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

                for (int i = 0; i < this.listBox1.Items.Count; i++)
                {
                    if (this.listBox1.Items[i].ToString() == this.listBox2.SelectedItem.ToString())
                    {
                        MessageBox.Show("该车辆已经被设置为监控目标", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }
                }

                this.listBox1.Items.Add(this.listBox2.SelectedItem.ToString());

            }
            catch (Exception ex) { writeToLog(ex, "button1_Click"); }
        }

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
            catch (Exception ex) { writeToLog(ex, "button2_Click"); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.listBox1.Items.Count > 0)               
                    this.listBox1.Items.Clear();
            }
            catch (Exception ex) { writeToLog(ex, "button3_Click"); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = string.Empty;

                if (this.textBox1.Text != "")
                    sql = "Select 终端车辆号牌 from GPS警车定位系统 where 终端车辆号牌 like '%" + this.textBox1.Text.Trim() + "%' order by 终端车辆号牌";
                else
                    sql = "Select 终端车辆号牌 from GPS警车定位系统 order by 终端车辆号牌";

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
            catch (Exception ex)
            {
                writeToLog(ex, "button4_Click");
            }
        }

        /// <summary>
        /// 查询SQL
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

        private void writeToLog(Exception ex, string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine(DateTime.Now.ToString() + ": " + "车辆Info窗体:在 " + sFunc + "方法中发生错误。");
            sw.WriteLine(ex.ToString());
            sw.Close();
        }
    }
}