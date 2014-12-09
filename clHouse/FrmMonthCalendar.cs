using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clHouse
{
    public partial class FrmMonthCalendar : Form
    {

        public string dateString = "";
        public FrmMonthCalendar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.getDateString();
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 获取日期
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        public void getDateString()
        {
            this.dateString = this.monthCalendar1.SelectionStart.ToString();
        }

        /// <summary>
        /// 取消窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-30
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }


      
    }
}