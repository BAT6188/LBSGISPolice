using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LBSDataGuide;

namespace clZonghe
{
    public partial class frmTongji : Form
    {
        public frmTongji()
        {
            InitializeComponent();
        }
        public DataTable _exportDT;

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 导出数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void btnExcel_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.Rows.Count <= 0)
            {
                MessageBox.Show("没有可导出的数据,不能执行此操作!","提示");
                return;
            }
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "请选择将导出的EXCEL文件存放路径";
                sfd.Filter = "Excel文档(*.xls)|*.xls";
                sfd.FileName = "EXP" + string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
                if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                {
                    if (sfd.FileName.LastIndexOf(".xls") <= 0)
                    {
                        sfd.FileName = sfd.FileName + ".xls";
                    }
                    string fileName = sfd.FileName;
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;

                    if (dg.OutData(fileName, _exportDT))
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("导出Excel完成!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;

                        MessageBox.Show("导出Excel失败!", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "DataExport");
                this.Cursor = Cursors.Default;
            }
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
            CLC.BugRelated.ExceptionWrite(ex, "clZonghe-frmTongji-" + sFunc);
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void frmTongji_Load(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.DataSource != null)
                {
                    dataGridView1.DataSource = null;
                }

                dataGridView1.DataSource = _exportDT;
                dataGridView1.Columns[0].Width = 210;

                // 添加行颜色
                for (int i = 0; i < _exportDT.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmTongji_Load");
            }
        }
    }
}