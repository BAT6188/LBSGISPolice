using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            //this.FormBorderStyle = FormBorderStyle.None;
            this.BackgroundImage = Image.FromFile(Application.StartupPath+"\\loading.bmp");
            //this.pictureBox1.Image = Image.FromFile("loginBG.bmp");
        }
    }
}