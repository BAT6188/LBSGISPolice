using System;
using System.Windows.Forms;

namespace LBSgisPolice 
{
    public partial class FrmScheme : Form
    {
        public FrmScheme()
        {
            InitializeComponent();
        }

        public int SchemeNmuber;

        /// <summary>
        /// 保存按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void Button1Click(object sender, EventArgs e) 
        {
            if (radioButton1.Checked)
                this.SchemeNmuber = 1;
            if (this.radioButton2.Checked)
                this.SchemeNmuber = 2;
            if (this.radioButton3.Checked)
                this.SchemeNmuber = 3;

            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 关闭窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void Button2Click(object sender, EventArgs e) 
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 加载事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void FrmScheme_Load(object sender, EventArgs e)
        {
            switch (this.SchemeNmuber)
            {
                case 1:
                    this.radioButton1.Checked = true;
                    break;
                case 2:
                    this.radioButton2.Checked = true;
                    break;
                case 3:
                    this.radioButton3.Checked = true;
                    break;
                default:
                    break;
            }
        }


    }
}