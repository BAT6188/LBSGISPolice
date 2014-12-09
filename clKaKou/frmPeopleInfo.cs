using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clKaKou
{
    public partial class frmPeopleInfo : Form
    {
        public frmPeopleInfo()
        {
            InitializeComponent();
        }

        public string UserName;      // ��½�û�����

        internal void setInfo(DataRow dRow, string un)
        {
            try
            {
                //StrCon = Constr;

                UserName = un;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("�����ֶ�") < 0)
                    {
                        if (col.Caption.IndexOf("��Ƭ") < 0 && col.Caption != "���֤������")
                        {
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                        }
                        else if (col.Caption == "���֤������" && dRow[col].ToString().Length > 0)
                        {
                            //|| (col.Caption == "��Ƭ1" && dRow[col].ToString().Length > 0) || (col.Caption == "��Ƭ2" && dRow[col].ToString().Length > 0) || (col.Caption == "��Ƭ3" && dRow[col].ToString().Length > 0))
                            //{
                            DataGridViewLinkCell dglc1 = new DataGridViewLinkCell();

                            //dglc1.Value = "�鿴 " + col.Caption + " ����Ϣ";

                            if (col.Caption == "���֤������")
                                dglc1.Value = dRow[col].ToString();

                            dglc1.ToolTipText = "�鿴�˿���ϸ��Ϣ";

                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);

                            this.dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[1] = dglc1;
                            //}
                        }
                    }
                }

                this.setSize();

                //this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //����λ��

                int ki = 0;
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].ToString().IndexOf("����״̬") > 0)
                    {
                        ki = i;
                    }
                }



                this.TopMost = true;

                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                //writeToLog(ex, "clKaKou-frmInfo-01-setInfo");
            }
        }

        private void setSize()
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].Width > 300)
                    {
                        this.dataGridView1.Columns[1].Width = 300;
                        this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    double width = this.dataGridView1.Columns[1].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView1.Rows[i].Cells[1].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView1.Rows[i].Height = (this.dataGridView1.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + 30;

                if (this.dataGridView1.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView1.Rows[i].Height;
                }
                cMessageHeight += 100;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //���ô�С
            }
            catch (Exception ex)
            {
                //writeToLog(ex, "clKaKou-frmInfo-02-setSize");
            }
        }
    }
}