using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGPSPolice
{
    public partial class frmExeGroup : Form
    {
        private string[] _conStr;        // 存放数据库访问参数
        private bool flag;　　　　　　　 // 表示当前窗体是增加还是修改
        private string[] _exeGroup;      // 存储修改前数据

        public DataTable exeGroupTable;  // 重大任务组数据
        

        public frmExeGroup(string[] conStr,bool falg,string[] exeGroup)
        {
            InitializeComponent();

            this._conStr = conStr;
            this.flag = falg;
            this._exeGroup = exeGroup;
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        private void frmExeGroup_Load(object sender, EventArgs e)
        {
            try
            {
                if (flag)
                {
                    this.lblRenNum.Text = System.DateTime.Now.ToString("yyMMddhh24mmss") + System.DateTime.Now.Millisecond;

                    string strMajor = "select 任务名称 from 重大任务";
                    DataTable table = GetTable(strMajor);

                    if (table.Rows.Count <= 0) return;

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        this.comName.Items.Add(table.Rows[i][0].ToString());
                    }
                    this.comName.SelectedIndex = 0;
                }
                else
                {
                    this.lblRenNum.Text = _exeGroup[0];
                    this.txtName.Text = _exeGroup[1];
                    this.comName.Text = _exeGroup[2];
                    this.txtUntis.Text = _exeGroup[3];
                    this.txtID.Text = _exeGroup[4];
                    setStandbyID(this.listBox1, _exeGroup[5]);
                    this.txtNum.Text = _exeGroup[6];
                    this.btnOK.Text = "更新组";
                    this.Text = "修改任务组";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmMajorTask_Load");
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
                ExToLog(ex, "setStandbyID");
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
        /// 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        /// <summary>
        /// 创建组
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkInput())
                {
                    if (flag)  　// 增加
                    {
                        string strSql = "insert into 重大任务组 values('" + this.lblRenNum.Text + "','" + this.txtName.Text + "','"
                                                                          + this.comName.Text + "','" + this.txtUntis.Text + "','"
                                                                          + this.txtID.Text + "','" + this.getBeiID() + "',"
                                                                          + this.txtNum.Text + ")";

                        commandSql(strSql);
                    }
                    else　　　　// 修改
                    {
                        string updSql = "Update 重大任务组 set 任务组名称='" + this.txtName.Text + "',执行任务名称='" + this.comName.Text 
                                                          + "',执行任务单位='" + this.txtUntis.Text + "',指挥员肩咪ID='" + this.txtID.Text
                                                          + "',备用肩咪ID='" + this.getBeiID() + "',参加任务人数='" + this.txtNum.Text + "' where 任务组编号='" + _exeGroup[0]+ "'";
                        commandSql(updSql);
                    }

                    string strExeg = "select * from 重大任务组";
                    exeGroupTable = GetTable(strExeg);

                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
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
                                                      || this.comName.Text == string.Empty )
                {
                    MessageBox.Show("数据未完善，请完善数据后创建任务点", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (!checkNumber(this.txtID.Text.Trim()) || !checkNumber(this.txtNum.Text.Trim()))
                {
                    MessageBox.Show("肩咪ID和参加任务人数必须为数字！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnOK_Click");
                return false;
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
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
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
                CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
                CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "commandSql");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmExeGroup-" + sFunc);
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
                        this.listBox1.Items.Add(this.txtBeiID.Text);
                    }
                    else 
                    {
                        MessageBox.Show("备用ID中包含此肩咪，请重设！", "系统提示",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnBeiAdd_Click");
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
                if (listBox1.Items.Count <= 0) return true;

                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if (beiStr == listBox1.Items[i].ToString())
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "addNoDouble");
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
                this.listBox1.Items.Remove(this.listBox1.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnBeiDel_Click");
            }
        }
    }
}