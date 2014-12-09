using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace clGISPoliceEdit
{
    public partial class FrmImage : Form
    {
        public FrmImage()
        {
            
            InitializeComponent();
        }


        public OpenFileDialog openFileDialog = new OpenFileDialog();
        public bool hasUpdate=false;
        private void ChangeImageButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.openFileDialog.Multiselect = false;
                this.openFileDialog.Filter = "BPM图片 (*.BMP)|*.BMP|JPG图片 (*.JPG)|*.JPG|GIF图片 (*.GIF)|*.GIF|All files (*.*)|*.*";

                if (this.openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs;
                    string fileName = openFileDialog.FileName;
                    fs = File.OpenRead(fileName);

                    this.pictureBox1.Image = Image.FromStream(fs);
                    this.pictureBox1.Invalidate();
                    //this.DialogResult = DialogResult.OK;
                    hasUpdate = true;
                }
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-FrmImage-ChangeImageButton_Click");
            }  
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

     
    }
}