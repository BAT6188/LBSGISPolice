using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace clPopu
{
    public partial class FrmImage : Form
    {
        public FrmImage()
        {
            
            InitializeComponent();
           
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}