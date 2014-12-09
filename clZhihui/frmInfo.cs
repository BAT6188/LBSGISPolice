using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace clZhihui  
{
    public partial class FrmInfo : Form
    {
        private string[] StrCon;
        public string UserName;

        public FrmInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 筛选字段添加链接
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dRow">数据行</param>
        /// <param name="pt">位置坐标</param>
        /// <param name="Constr">访问数据库参数</param>
        /// <param name="un">用户名</param>
        internal void setInfo(DataRow dRow, Point pt, string[] Constr, string un)
        {
            try
            {
                StrCon = Constr;

                UserName = un;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码" 
                                                            && col.Caption != "所属中队代码"
                                                            && col.Caption != "所属警务室代码"
                                                            && col.Caption != "抽取ID"
                                                            && col.Caption != "抽取更新时间"
                                                            && col.Caption != "最后更新人")
                    {
                        this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                }

                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置

                //在姓名上加链接,可查看照片
                int k = 0;
                for (int i = 0; i < dRow.Table.Columns.Count; i++)
                {
                    if (dRow.Table.Columns[i].Caption == "照片1:" || dRow.Table.Columns[i].Caption == "照片2:" || dRow.Table.Columns[i].Caption == "照片3:")
                    {
                        k = i;
                        break;
                    }
                }

                DataGridViewTextBoxCell dglc = new DataGridViewTextBoxCell();

                dglc.Value = dRow[dRow.Table.Columns[k]];

                this.dataGridView1.Rows[k].Cells[1] = dglc;

               //if (dRow.Table.Columns.Count != 11)
               //     this.panel3.Visible = true;
               // else
               //     this.panel3.Visible = false;

                this.TopMost = true;

                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "01-传递dt获取信息");
            }
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
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
                cMessageHeight += 100;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch(Exception ex)
            {
                ExToLog(ex, "02-setSize");
            }
        }

        /// <summary>
        /// 设置窗体位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="iWidth">窗体宽度</param>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        private void setLocation(int iWidth,int iHeight,int x,int y) {
            try
            {
                if (x + iWidth > Screen.PrimaryScreen.WorkingArea.Width)
                {
                    x = x - iWidth - 10;
                }
                if (y + iHeight > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    y = y - iHeight - 10;
                    if (y < 0) y = 0;
                }
                this.SetDesktopLocation(x, y);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "03-setLocation");
            }
        }       

        /// <summary>
        /// 关闭窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           try
            {
                //string exedtl = string.Empty;   //处理情况
                //string exenum = string.Empty;   //处理警员编号        

                //frmExcut frmexec = new frmExcut();

                //frmexec.ShowDialog(this);
                //if (frmexec.DialogResult == DialogResult.OK)
                //{
                //    if (frmexec.ExDetail.Length > 50)
                //    {
                //        MessageBox.Show("处理情况概述超过50字符,系统自动取前50个字符作为概述信息进行保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //        exedtl = frmexec.ExDetail.Substring(0, 50);
                //    }
                //    else
                //    {
                //        exedtl = frmexec.ExDetail;
                //    }
                //    exenum = frmexec.ExNum;

                //    if (exenum != "" && exedtl != "")
                //    {
                      
                //        string sqlstr = "update GPS110.报警信息110 set GPS110.报警信息110.处理状态 = '已处理',GPS110.报警信息110.处理警力编号='" + exenum + "',GPS110.报警信息110.处理情况='" + exedtl + "' where GPS110.报警信息110.报警编号 ='" + this.dataGridView1.Rows[1].Cells[1].Value.ToString() + "'";
                //        RunCommand(sqlstr);

                //        MessageBox.Show("成功保存处理情况", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //        WriteEditLog(sqlstr,"报警信息");
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("没有写入处理情况概述", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);                  
                //}
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click-04-处理情况");
            }          
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
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
            CLC.BugRelated.ExceptionWrite(ex, "cl110-frmInfo-" + sFunc);
        }

        private void RecToLog(string s)
        {
            CLC.BugRelated.LogWrite(s, Application.StartupPath + "\rec.log");
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="method">方法名</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + UserName + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'110接处警','GPS110.案件信息110:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex) { ExToLog(ex, "WriteEditLog"); }
        }
    }
}

