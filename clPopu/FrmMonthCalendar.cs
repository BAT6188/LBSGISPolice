using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clPopu
{
    public partial class FrmMonthCalendar : Form
    {

        public string dateString = "";
        public FrmMonthCalendar()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.getDateString();
            this.DialogResult = DialogResult.OK;
        }

        public void getDateString()
        {
            this.dateString = this.monthCalendar1.SelectionStart.ToString();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void FrmMonthCalendar_Load(object sender, EventArgs e)
        {

        }


      
    }
}