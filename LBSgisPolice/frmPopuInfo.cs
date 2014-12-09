using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Data.OracleClient;
using MapInfo.Data;
using MapInfo.Windows.Controls;

namespace LBSgisPolice
{
    public partial class frmPopuInfo : Form
    {
        public MapInfo.Windows.Controls.MapControl mapControl = null;
        private DataRow row = null;
        public frmPopuInfo()
        {
            InitializeComponent();
        }

        internal void setInfo(DataRow dRow, System.Drawing.Point pt)
        {
            this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.dataGridView1.Rows.Clear();

            row = dRow;
            foreach (DataColumn col in dRow.Table.Columns)
            {
                if (col.Caption.IndexOf("备用字段") < 0)
                {
                    this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                }
            }
            this.setSize();
            this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
            this.Visible = true;
        }

        private void setSize()
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

        private void setLocation(int iWidth, int iHeight, int x, int y)
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


        // 列表单击事件
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }
    }
}