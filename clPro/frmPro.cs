using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clPro
{
    public partial class frmPro : Form
    {
        public frmPro()
        {
            InitializeComponent();
            NewMethod();
        }

        /// <summary>
        /// 控制窗体大小及图片位置
        /// </summary>
        private void NewMethod()
        {
            try
            {
                Screen screen = Screen.PrimaryScreen;
                int width = screen.WorkingArea.Width;
                int height = screen.WorkingArea.Height;

                Size _size = new Size();
                _size.Width = width;
                _size.Height = height;
                this.Size = _size;

                Point point = new Point();
                point.X = this.Width / 2;
                point.Y = this.Height / 2;
                this.pictureBox1.Location = point;
            }
            catch { }
        }
    }
}