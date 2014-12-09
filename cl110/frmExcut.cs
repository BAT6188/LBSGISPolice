using System;
using System.Windows.Forms;

namespace cl110
{
    public partial class frmExcut : Form
    {
        public double ds;

        public frmExcut()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ds = Convert.ToDouble(textBox2.Text.Trim());
                DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "cl110-frmExcut-button1_Click");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "cl110-frmExcut-button2_Click");
            }
        }

        private void frmExcut_Load(object sender, EventArgs e)
        {
            textBox2.Text = ds.ToString();
        }
    }
}