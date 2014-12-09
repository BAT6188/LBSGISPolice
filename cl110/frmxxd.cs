using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace cl110
{
    public partial class Frmxxd : Form 
    {
        public Frmxxd()
        {
            InitializeComponent();
        }

        public string Xxname = string.Empty;
        public int Xxlb ;

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
                        MessageBox.Show(@"���ֻ��Ϊ��ֵ������������������д", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(@"���Ʋ���Ϊ��ֵ����������д", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex,"cl110-Frmxxd-Button1Click");
            }
        }

        private static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }

        private void Button2Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}