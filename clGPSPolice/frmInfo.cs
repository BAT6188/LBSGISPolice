using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.IO;
using MapInfo.Data;


namespace clGPSPolice 
{ 
    public partial class FrmInfo : Form
    {
        public FrmInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 传递dt获取信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dRow">数据行</param>
        /// <param name="pt">坐标点</param>
        internal void setInfo(DataRow dRow,Point pt)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0)
                    {
                        this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
                this.TopMost = true;
                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "01-setInfo");
            }
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
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
                ExToLog(ex, "02-setSize");
            }
        }

        /// <summary>
        /// 设置窗体位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="iWidth">窗体宽度</param>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        private void setLocation(int iWidth,int iHeight,int x,int y)
        {
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
            catch (Exception ex)
            {
                ExToLog(ex, "03-setLocation");
            }
        }

        /// <summary>
        /// 窗体加载时设置窗体位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void FrmInfo_Load(object sender, EventArgs e)
        {
            try
            {
                this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "04-Load");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmInfo-"+sFunc);          
        }
    }
}

