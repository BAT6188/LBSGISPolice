using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clGISPoliceEdit
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
            this.dateString = this.monthCalendar1.SelectionStart.Date.ToShortDateString()+" "+dateTimePicker1.Value.TimeOfDay.ToString();
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
      
    }
}