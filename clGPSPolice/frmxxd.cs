using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace clGPSPolice
{
    public partial class frmxxd : Form
    {
        public frmxxd()
        {
            InitializeComponent();
        }

        public string Xxname = string.Empty;  // 名称
        public int Xxlb;                      // 类别编号

        /// <summary>
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Button1Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text.Trim().Length > 0)
                {
                    if (IsInt(textBox2.Text.Trim()) || textBox2.Text.Trim().Length == 0)
                    {
                        Xxname = textBox1.Text.Trim();
                        Xxlb = textBox2.Text.Trim().Length == 0 ? 0 : Convert.ToInt32(textBox2.Text.Trim());
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show(@"类别只能为空值或者整数，请重新填写", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(@"名称不能为空值，请重新填写", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmxxd-Button1Click");
            }
        }

        /// <summary>
        /// 判断值是不是空值或者整数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="value">要判断的值</param>
        /// <returns>布尔值(true-是 false-否)</returns>
        private static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }

        /// <summary>
        /// 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Button2Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}