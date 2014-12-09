using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGISPoliceEdit
{
    public partial class frmNoLocation : Form
    {
        private string frmSql;                 // 查询sql
        private string pcsRegion;              // 派出所权限
        private string zdRegion;               // 民警中队权限
        private DataGridViewCheckBoxCell g;    // 复选框

        public frmNoLocation(string sql,string strRegion,string strRegion1)
        {
            InitializeComponent();

            this.frmSql = sql;
            this.pcsRegion = strRegion;
            this.zdRegion = strRegion1;
        }

        private void getPcsZd(pcsZd pz, string s)
        {
            try
            {
                if (pz.ToString() == "pcs")  // 派出所
                {
                    
                }
                else   // 民警中队
                {

                }

            }
            catch (Exception ex)
            {
                ExToLog(ex, "");
            }
        }

        /// <summary>
        /// 枚举值 区分是派出所或中队
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-1
        /// </summary>
        private enum pcsZd  
        {
             pcs,
             zd
        }

        /// <summary>
        /// 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-25
        /// </summary>
        private void btnColse_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnColse_Click");
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
            CLC.BugRelated.ExceptionWrite(ex, "clGISPoliceEdit-frmNoLocation-" + sFunc);
        }

        /// <summary>
        /// 处理指派数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btnAssigned_Click(object sender, EventArgs e)
        {
            try
            {


            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnAssigned_Click");
            }
        }

        /// <summary>
        /// 窗体加载事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-1
        /// </summary>
        private void frmNoLocation_Load(object sender, EventArgs e)
        {
            try
            {
                this.lalCla.Text = "认领数据：指此条数据知道其派出所但不知道\t\n          " 
                                           + "民警中队的数据，让此派出所辖区\t\n          " 
                                           + "的民警中队认领过去。";

                this.lalAss.Text = "指派数据：指此条数据不知道其派出所及民警\t\n          " 
                                           + "中队的数据，让具有全区权限的用\t\n          " 
                                           + "户将数据指派到派出所或民警中队。";

                if (pcsRegion != "顺德区" && pcsRegion != "")
                {
                    this.groupBox1.Visible = false;
                    this.groupBox2.Visible = true;
                }
                else
                {
                    getPcsZd(pcsZd.zd, pcsRegion);
                }
                if (pcsRegion == "顺德区")
                {
                    this.groupBox2.Visible = false;
                    this.groupBox1.Visible = true;
                }
                else
                {
                    getPcsZd(pcsZd.pcs, pcsRegion);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmNoLocation_Load");
            }
        }

        /// <summary>
        /// 处理认领数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-1
        /// </summary>
        private void btnClaim_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnClaim_Click");
            }
        }
    }
}