using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace cl110
{
    public partial class FrmScheme : Form
    {
        public FrmScheme()
        {
            InitializeComponent();
        }

        public int SchemeNmuber;

        private void Button1Click(object sender, EventArgs e) 
        {
            if (this.radioButton1.Checked)
                this.SchemeNmuber = 1;
            if (this.radioButton2.Checked)
                this.SchemeNmuber = 2;
            if (this.radioButton3.Checked)
                this.SchemeNmuber = 3;

            this.DialogResult = DialogResult.OK;
        }

        private void Button2Click(object sender, EventArgs e) 
        {
            this.DialogResult = DialogResult.Cancel;
        }

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