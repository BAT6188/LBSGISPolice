using System;
using System.Windows.Forms;

namespace cl110
{
    public partial class frmalarm : Form
    {
        public frmalarm()
        {
            InitializeComponent();
        }

        public string st = string.Empty;
        public string et = string.Empty;

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.radioButton1.Checked == true)
            {
                st = System.DateTime.Now.ToShortDateString() + " 00:00:01";
                et = System.DateTime.Now.ToShortDateString() + " 23:59:59";
            }
            else 
            {
                if (this.dateTimePicker2.Value > this.dateTimePicker1.Value)
                {
                    st = this.dateTimePicker1.Value.ToString();
                    et = this.dateTimePicker2.Value.ToString();
                }
                else
                {
                    MessageBox.Show("终止时间一定要大于起始时间！","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    return;
                }
            }
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void frmalarm_Load(object sender, EventArgs e)
        {
            this.radioButton1.Checked = true;

            this.radioButton2.Checked = false;
            this.dateTimePicker1.Enabled = false;
            this.dateTimePicker2.Enabled = false;

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton2.Checked == false)
            {
                this.dateTimePicker1.Enabled = false;
                this.dateTimePicker2.Enabled = false;
                this.radioButton1.Checked = true;
            }
            else
            {
                this.dateTimePicker1.Enabled = true;
                this.dateTimePicker2.Enabled = true;
                this.radioButton1.Checked = false;
                this.dateTimePicker1.Text = System.DateTime.Now.ToShortDateString()+" 00:00:01";

                this.dateTimePicker2.Text = System.DateTime.Now.ToShortDateString()+" 23:59:59";
            }
        }


      
    }
}