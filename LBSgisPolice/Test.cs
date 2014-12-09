using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        public string msg = string.Empty;

        private void button1_Click(object sender, EventArgs e)
        {
            msg = textBox1.Text.Trim();
            DialogResult= DialogResult.OK;
        }
    }
}