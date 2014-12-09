using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGPSPolice
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int SecFresh = 0;

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = SecFresh.ToString();
        }

        /// <summary>
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text != "")
                {
                    SecFresh = Convert.ToInt32(this.textBox1.Text.Trim());
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex) { CLC.BugRelated.ExceptionWrite(ex, "button1_Click"); }
        }

        /// <summary>
        /// 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}