using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSDataGuide
{
    public partial class frmSchedule : Form
    {

        private string proName;
        public frmSchedule(string pro)
        {
            InitializeComponent();
            this.proName = pro;
        }

        /// <summary>
        /// 默认显示的信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void frmSchedule_Load(object sender, EventArgs e)
        {
            this.lblMessage.Text = "正在" + proName + "数据，请稍候．．．";
        }
    }
}