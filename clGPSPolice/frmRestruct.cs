using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace clGPSPolice
{
    public partial class frmRestruct : Form
    {
        private string[] _numStr;　  // 存放要重组的任务组编号
        private string[] conStr;     // 存放数据库访问参数
        private ArrayList taskForceList = new ArrayList();  // 存放任务组对象

        private string numberStr;  // 要重组任务组编号

        public frmRestruct(string[] numStr,string[] _conStr)
        {
            InitializeComponent();

            this._numStr = numStr;
            this.conStr = _conStr;
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        private void frmRestruct_Load(object sender, EventArgs e)
        {
            try
            {
                InitializationRestruct();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmRestruct_Load");
            }
        }

        /// <summary>
        /// 初始化窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-21
        /// </summary>
        private void InitializationRestruct()
        {
            try
            {

                for (int i = 0; i < _numStr.Length; i++)
                {
                    numberStr += _numStr[i] + ",";
                }
                numberStr = numberStr.Substring(0, numberStr.Length - 1).Replace(",", "','");

                string resSql = "select * from 重大任务组 where 任务组编号 in ('" + numberStr + "')";
                string numRen = "";
                int sitRen = 0;

                DataTable table = GetTable(resSql);


                for (int j = 0; j < table.Rows.Count; j++)
                {
                    TaskForce task = new TaskForce();
                    task.TaskForceNum = table.Rows[j][0].ToString();
                    task.TaskForceName = table.Rows[j][1].ToString();
                    task.MajorName = table.Rows[j][2].ToString();
                    task.MajorUntis = table.Rows[j][3].ToString();
                    task.CommanderID = table.Rows[j][4].ToString();
                    task.StandCommID = table.Rows[j][5].ToString();
                    task.ExecutiveRen = table.Rows[j][6].ToString();
                    taskForceList.Add(task);
                    //task.TaskForceNum + "  " +
                    this.listBoxRestruct.Items.Add(task.TaskForceName + "  " + task.CommanderID + "  " + task.ExecutiveRen);

                    if (table.Rows[j][5].ToString() != "")
                    {
                        numRen += table.Rows[j][5].ToString() + ",";
                    }
                    sitRen += Convert.ToInt32(table.Rows[j][6].ToString());
                }

                string strMajor = "select 任务名称 from 重大任务";
                DataTable tableMajor = GetTable(strMajor);

                if (tableMajor.Rows.Count <= 0) return;

                for (int i = 0; i < tableMajor.Rows.Count; i++)
                {
                    this.comName.Items.Add(tableMajor.Rows[i][0].ToString());
                }
                this.comName.SelectedIndex = 0;
                numRen = numRen.Substring(0, numRen.Length - 1);

                setStandbyID(this.listBoxID, numRen);
                this.lblRen.Text = sitRen.ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitializationRestruct");
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
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-15
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private static void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmRestruct-" + sFunc);
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
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-21
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string numStr = System.DateTime.Now.ToString("yyMMddhh24mmss") + System.DateTime.Now.Millisecond;  // 生成任务组编号

                string strSql = "insert into 重大任务组 values('" + numStr + "','" + this.txtName.Text + "','"
                                                                          + this.comName.Text + "','" + this.txtUntis.Text + "','"
                                                                          + this.txtID.Text + "','" + this.getBeiID() + "',"
                                                                          + this.lblRen.Text + ")";

                commandSql(strSql);

                string delSql = "delete from 重大任务组 where 任务组编号 in ('" + numberStr + "')";
                commandSql(delSql);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnOK_Click");
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

                if (listBoxID.Items.Count <= 0) return beiStr;

                for (int i = 0; i < listBoxID.Items.Count; i++)
                {
                    if (beiStr == string.Empty)
                        beiStr = listBoxID.Items[i].ToString();
                    else
                        beiStr += "," + listBoxID.Items[i].ToString();
                }

                return beiStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getBeiID");
                return null;
            }
        }
    }
}