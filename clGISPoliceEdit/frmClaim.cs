using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGISPoliceEdit
{
    public partial class frmClaim : Form
    {
        private string claSql;    // 查询要认领数据的SQL
        private string pcsRegion; // 派出所权限
        private string zdRegion;  // 中队权限
        private string[] StrCon;  // 数据库访问参数

        public frmClaim(string sqlCla, string strRegion, string strRegion1, string[] conStr)
        {
            InitializeComponent();

            this.claSql = sqlCla;
            this.pcsRegion = strRegion;
            this.zdRegion = strRegion1;
            this.StrCon = conStr;
        }

        /// <summary>
        /// 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.No;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button2_Click");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGISPoliceEdit-frmClaim-" + sFuns);
        }


        /// <summary>
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string idStr = "";
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if ((bool)dataGridView1.Rows[i].Cells[0].Value)
                    {
                        if (i <= 100)
                        {
                            if (idStr == string.Empty)
                                idStr = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            else
                                idStr += "," + dataGridView1.Rows[i].Cells[1].Value.ToString();
                        }
                        else
                        {
                            string upSql = "select * from 基础数据";
                            for (int j = 0; j < dataGridView1.Rows.Count; j++)
                            {
                                if ((bool)dataGridView1.Rows[j].Cells[0].Value)
                                {
                                    dataGridView1.Rows.Remove(dataGridView1.Rows[j]);
                                }
                            }
                            i = 0;
                        }
                    }
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }
        }

        /// <summary>
        /// 窗体加载事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private void frmClaim_Load(object sender, EventArgs e)
        {
            try
            {
                GetPerUntis();
                DataTable claimTable = GetTable(claSql);
                DataGridViewCheckBoxCell dgvcb;

                for (int i = 0; i < claimTable.Rows.Count; i++)
                {
                    dgvcb = new DataGridViewCheckBoxCell();
                    this.dataGridView1.Rows.Add(new object[] {false, claimTable.Rows[i][0], claimTable.Rows[i][1]});

                    if (i % 2 == 1)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmClaim_Load");
            }
        }

        /// <summary>
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>结果集</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 获取认领单位
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private string[] GetPerUntis()
        {
            try
            {
                string[] perUntis = null;
                //if (pcsRegion == "顺德区")
                //{
                //    string pcsStr = "select 派出所名,派出所代码 from 基层派出所";
                //    DataTable dt = GetTable(pcsStr);

                //    perUntis = new string[dt.Rows.Count];
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        comCliam.Items.Add(dt.Rows[i][0]);
                //        perUntis[i] = dt.Rows[i][1].ToString();
                //    }
                //}

                if (pcsRegion != "顺德区" && pcsRegion != "")
                {
                    string pcsStr = "select 派出所名,派出所代码 from 基层派出所";
                    DataTable dt = GetTable(pcsStr);

                    perUntis = new string[dt.Rows.Count];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        comCliam.Items.Add(dt.Rows[i][0]);
                        perUntis[i] = dt.Rows[i][1].ToString();
                    }
                }

                if (zdRegion != "")
                {
                    string zdStr = "select distinct(所属派出所) from 基层民警中队 where 中队名 in ('" + zdRegion.Replace(",", "','") + "')";
                    DataTable db = GetTable(zdStr);
                    string pcStr = "";
                    for (int j = 0; j < db.Rows.Count; j++)
                    {
                        if (pcStr == "")
                            pcStr = db.Rows[j][0].ToString();
                        else
                            pcStr += "," + db.Rows[j][0].ToString();
                    }

                    string pcSql = "select 中队,中队名 from 基层民警中队 where 所属派出所 in ('" + pcStr.Replace(",", "','") + "')";
                    DataTable zdTab = GetTable(zdStr);

                    perUntis = new string[zdTab.Rows.Count];
                    for (int k = 0; k < zdTab.Rows.Count; k++)
                    {
                        comCliam.Items.Add(zdTab.Rows[k][0]);
                        perUntis[k] = zdTab.Rows[k][1].ToString();
                    }
                }
                this.comCliam.SelectedIndex = 0;
                return perUntis;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetPerUntis");
                return null;
            }   
        }

        /// <summary>p
        /// 全选或全不选
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private void checkAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = checkAll.Checked;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkAll_CheckedChanged");
            }
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        /// <param name="sql">执行的SQL语句</param>
        private void commandSql(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "commandSql");
            }
        }
    }
}