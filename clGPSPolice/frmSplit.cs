using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGPSPolice
{
    public partial class frmSplit : Form
    {
        private string[] _strMess = new string[7];  // 任务组详细信息
        private string[] conStr;                    // 数据库访问参数
        public DataTable majorTable;                // 重大任务查询结果（关闭窗体时将值传出）

        public frmSplit(string[] strMess,string[] _conStr)
        {
            InitializeComponent();

            this._strMess = strMess;
            this.conStr = _conStr;
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        private void frmSplit_Load(object sender, EventArgs e)
        {
            try 
            {
                this.lblName.Text = _strMess[0];
                this.lblMajorTask.Text = _strMess[1];
                this.lblUnits.Text = _strMess[2];
                this.lblID.Text = _strMess[3];
                this.lblNum.Text = _strMess[4];
                setStandbyID(this.listBox1, _strMess[5]);

                string strMajor = "select 任务名称 from 重大任务";
                DataTable table = GetTable(strMajor);

                if (table.Rows.Count <= 0) return;

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    this.comName.Items.Add(table.Rows[i][0].ToString());
                }
                this.comName.SelectedIndex = 0;
            }
            catch (Exception ex)
            { 
                ExToLog(ex, "frmSplit_Load"); 
            }
        }

        /// <summary>
        /// 将备用肩咪ID添加到listBox中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <param name="list">ListBox控件</param>
        /// <param name="strID">备用肩咪ID</param>
        private void setStandbyID(ListBox list, string strID)
        {
            try
            {
                string[] tolSet = strID.Split(',');

                if (tolSet.Length == 0) return;

                for (int i = 0; i < tolSet.Length; i++)
                {
                    list.Items.Add(tolSet[i]);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmSplit_Load");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private static void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmSplit-" + sFunc);
        }

        /// <summary>
        /// 关闭按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        /// <summary>
        /// 实现拆分功能
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkInput())
                {
                    string renNum = System.DateTime.Now.ToString("yyMMddhh24mmss") + System.DateTime.Now.Millisecond;

                    string strSql = "insert into 重大任务组 values('" + renNum + "','" + this.txtName.Text + "','"
                                                                      + this.comName.Text + "','" + this.txtUntis.Text + "','"
                                                                      + this.txtID.Text + "','" + this.getBeiID() + "',"
                                                                      + this.txtNum.Text + ")";

                    commandSql(strSql);   // 重新组建一个任务组

                    string updSql = "update 重大任务组 set 参加任务人数=" + this.lblNum.Text.Trim() + " where 任务组编号='" + _strMess[6] + "'";

                    commandSql(updSql);   // 更改拆分后的任务人数

                    string strExeg = "select * from 重大任务组";
                    majorTable = GetTable(strExeg);

                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnOK_Click");
            }
        }

        /// <summary>
        /// 获取备用肩咪ID
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <returns></returns>
        private string getBeiID()
        {
            try
            {
                string beiStr = string.Empty;

                if (listBox1.Items.Count <= 0) return beiStr;

                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if (beiStr == string.Empty)
                        beiStr = listBox1.Items[i].ToString();
                    else
                        beiStr += "," + listBox1.Items[i].ToString();
                }

                return beiStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getBeiID");
                return null;
            }
        }

        /// <summary>
        /// 检查用户输入是否符合要求
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <returns>布尔值(true-符合 false-不符合)</returns>
        private bool checkInput()
        {
            try
            {
                if (this.txtName.Text == string.Empty || this.txtUntis.Text == string.Empty
                                                      || this.comName.Text == string.Empty)
                {
                    MessageBox.Show("数据未完善，请完善数据后创建任务点", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (!checkNumber(this.txtID.Text.Trim()) || !checkNumber(this.txtNum.Text.Trim()))
                {
                    MessageBox.Show("肩咪ID和参加任务人数必须为数字！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (Convert.ToInt32(this.txtNum.Text.Trim()) >= Convert.ToInt32(this.lblNum.Text.Trim()))
                {
                    MessageBox.Show("拆分参加任务人数过多不够此次拆分，请重新设置！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.txtNum.Text = "";
                    this.txtNum.Focus();
                    return false;
                }
                else
                {
                    // 对本次任务拆分人数进行更改
                    int renNum = Convert.ToInt32(this.lblNum.Text.Trim());
                    this.lblNum.Text = Convert.ToString(renNum - Convert.ToInt32(this.txtNum.Text.Trim()));
                    return true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkInput");
                return false;
            }
        }

        /// <summary>
        /// 判断输入的是不是数字(不能带小数)
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        private bool checkNumber(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return false;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$")) // 正则表达式判断输入的是不是数字
                {
                    MessageBox.Show("请在文本框中输入数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkNumber");
                return false;
            }
        }

        /// <summary>
        /// 肩咪ID自动补全
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        private void spellSearchBoxEx1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SplitWord.SpellSearchBoxEx txtID = (SplitWord.SpellSearchBoxEx)sender;

                string keyword = txtID.Text.Trim();

                string strExp = "select distinct(设备编号) from gps警员 where 设备编号 like '" + keyword + "%'";

                DataTable dt = GetTable(strExp);

                if (dt.Rows.Count < 1)
                    txtID.GetSpellBoxSource(null);
                else
                    txtID.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "spellSearchBoxEx1_TextChanged");
            }
        }

        /// <summary>
        /// 添加备用肩咪ID
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        private void btnBeiAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkNumber(this.txtBeiID.Text))
                {
                    if (addNoDouble(this.txtBeiID.Text))
                    {
                        this.listBox2.Items.Add(this.txtBeiID.Text);
                    }
                    else
                    {
                        MessageBox.Show("备用ID中包含此肩咪，请重设！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "commandSql");
            }
        }

        /// <summary>
        /// 检查添加的ID是否有重复
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <returns>布尔值(true-无重复 false-有重复)</returns>
        private bool addNoDouble(string beiStr)
        {
            try
            {
                if (listBox2.Items.Count <= 0) return true;

                for (int i = 0; i < listBox2.Items.Count; i++)
                {
                    if (beiStr == listBox2.Items[i].ToString())
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "commandSql");
                return false;
            }
        }

        /// <summary>
        /// 删除备用肩咪ID
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        private void btnBeiDel_Click(object sender, EventArgs e)
        {
            try
            {
                this.listBox2.Items.Remove(this.listBox2.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "commandSql");
            }
        }

        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                return dt;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetTable");
                return null;
            }
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-16
        /// </summary>
        /// <param name="sql">执行的SQL语句</param>
        private void commandSql(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
                CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "commandSql");
            }
        }
    }
}