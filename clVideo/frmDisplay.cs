using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clVideo
{
    public partial class frmDisplay : Form
    {
        private DataTable dts;

        public frmDisplay(DataTable dt)
        {
            InitializeComponent();

            dts = dt;
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void frmDisplay_Load(object sender, EventArgs e)
        {
            try
            {
                this.dataGridDisplay.DataSource = dts;

                for (int i = 0; i < dataGridDisplay.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridDisplay.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clVideo-frmDisplay-frmDisplay_Load");
            }
        }
    }
}