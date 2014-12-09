using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MapInfo.Windows.Controls;

namespace clInfo
{
    public partial class frmNumber : Form
    {
        public MapControl mapControl;      // 用于操作的地图
        public string getFromNamePath;     // 配置文件地址（GetFromNameConfig.ini）
        public string LayerName;           // 临时图层名称
        public string[] StrCon;            // 数据库连接参数
        public string tabName;             // 相关表名

        public frmNumber()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 给窗体添加内容
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="table">用于操作的内存表</param>
        /// <param name="tableName">表名</param>
        public void setInfo(DataTable table, string tableName)
        {
            try
            {
                tabName = tableName;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataRow row in table.Rows)
                {
                    this.dataGridView1.Rows.Add(new object[] { row[0] + ":", row[1] });
                }

                setSize();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setfrmInfo");
            }
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void setSize()
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].Width > 300)
                    {
                        this.dataGridView1.Columns[1].Width = 300;
                        this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    double width = this.dataGridView1.Columns[1].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView1.Rows[i].Cells[1].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView1.Rows[i].Height = (this.dataGridView1.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + 30;

                if (this.dataGridView1.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView1.Rows[i].Height;
                }
                cMessageHeight += 50;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clInfo-frmNumber-" + sFuns);
        }

        /// <summary>
        /// 点击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string numStr = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                string numNext = numStr.Substring(0, numStr.Length - 1);

                DataTable tbNum = new DataTable();
                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);

                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where "
                                                                 + CLC.ForSDGA.GetFromTable.ObjID + "='" + numNext + "'";
                tbNum = GetTable(strSQL);

                if (tbNum.Rows.Count == 0)
                {
                    MessageBox.Show("请确认编号是否正确！", "系统提示", MessageBoxButtons.OK);
                    return;
                }

                DataRow row = tbNum.Rows[0];

                System.Drawing.Point pt = new System.Drawing.Point();

                if (tbNum.Rows[0]["X"] != null && tbNum.Rows[0]["Y"] != null 
                                               && tbNum.Rows[0]["X"].ToString() != "" 
                                               && tbNum.Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(tbNum.Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(tbNum.Rows[0]["Y"]);
                }

                disPlayInfo(row, pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick");
            }
        }

        /// <summary>
        /// 显示详细信息窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="row">要显示的记录</param>
        /// <param name="pt">显示窗体的位置</param>
        private void disPlayInfo(DataRow row, System.Drawing.Point pt)
        {
            try
            {
                frmInfo fmInfo = new frmInfo();
                if (fmInfo.Visible == false)
                {
                    fmInfo = new frmInfo();
                    fmInfo.SetDesktopLocation(-30, -30);
                    fmInfo.Show();
                    fmInfo.Visible = false;
                }
                fmInfo.mapControl = mapControl;
                fmInfo.LayerName = LayerName;
                fmInfo.getFromNamePath = getFromNamePath;
                fmInfo.StrCon = StrCon;
                fmInfo.tableName = tabName;
                fmInfo.setInfo(row, pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <returns>结果集</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }
    }
}