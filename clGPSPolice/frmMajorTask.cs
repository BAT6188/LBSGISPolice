using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MapInfo.Windows.Controls;

namespace clGPSPolice
{
    public partial class frmMajorTask : Form
    {
        private string[] conStr = null;   // 数据库访问参数
        private bool upIns;               // 布尔值用来确定窗体功能是增加还是修改
        private string[] upStr;           // 用于修改时传递原始数据

        public string strNumber;          // 重大任务编号
        public string strName;            // 重大任务名称
        public string strCommander;       // 指挥员
        public string strUnits;           // 重大任务详情
        public DataTable majorTable;      // 重大任务查询结果（关闭窗体时将值传出）
         

        /// <summary>
        /// 构造函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        /// <param name="strCon">数据库访问参数</param>
        /// <param name="upIn">布尔值用来确定窗体功能是增加还是修改</param>
        /// <param name="_upStr">用于修改时传递原始数据</param>
        public frmMajorTask(string[] strCon,bool upIn,string[] _upStr)
        {
            InitializeComponent();

            this.conStr = strCon;
            this.upIns = upIn;
            this.upStr = _upStr;
        }

        /// <summary>
        /// 保存配置并创建任务地点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkInput())
                {
                    if (upIns)  // 增加
                    {
                        string strSql = "insert into 重大任务 values('" + this.lblRenNum.Text + "','"
                                                                        + this.txtName.Text + "','"
                                                                        + this.txtCommander.Text + "','"
                                                                        + this.txtMessage.Text + "')";
                        commandSql(strSql);
                    }
                    else　　　　// 修改
                    {
                        string updSql = "update 重大任务 set 任务名称='" + this.txtName.Text + "',指挥员='" + this.txtCommander.Text +
                                                          "',任务描述='" + this.txtMessage.Text + "' where 任务编号='" + upStr[0] + "'";
                        commandSql(updSql);
                    }

                    string selSql = "select * from 重大任务";
                    majorTable = GetTable(selSql);

                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnOK_Click");
            }
        }

        /// <summary>
        /// 检查用户输入是否符合要求
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        /// <returns>布尔值(true-符合 false-不符合)</returns>
        private bool checkInput()
        {
            try
            {
                if (this.txtCommander.Text == string.Empty || this.txtMessage.Text == string.Empty
                                                           || this.txtName.Text == string.Empty)
                {
                    MessageBox.Show("数据未完善，请完善数据后创建任务点", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmMajorTask-" + sFunc);
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
        /// 最后编辑时间  2011-2-11
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

        /// <summary>
        /// 判断输入的是不是数字(不能带小数)
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
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
                    MessageBox.Show("请在指挥人数文本框中输入数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        /// 取消重大任务创建
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-11
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            try {

                this.DialogResult = DialogResult.No;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnClose_Click");
            }
        }

        /// <summary>
        /// 关闭窗体时将值传出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-12
        /// </summary>
        private void frmMajorTask_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            { 
                ExToLog(ex, "frmMajorTask_FormClosing"); 
            }
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-14
        /// </summary>
        private void frmMajorTask_Load(object sender, EventArgs e)
        {
            try
            {
                if (upIns) 　// 增加
                {　
                    this.lblRenNum.Text = System.DateTime.Now.ToString("yyMMddhh24mmss") + System.DateTime.Now.Millisecond;
                }　
                else　　　　// 修改
                {
                    this.lblRenNum.Text = upStr[0];
                    this.txtName.Text = upStr[1];
                    this.txtCommander.Text = upStr[2];
                    this.txtMessage.Text = upStr[3];
                    this.btnOK.Text = "更新";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmMajorTask_Load");
            }
        }
    }
}