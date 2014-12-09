using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace clZhihui  
{
    public partial class FrmInfo : Form
    {
        private string[] StrCon;
        public string UserName;

        public FrmInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ɸѡ�ֶ��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dRow">������</param>
        /// <param name="pt">λ������</param>
        /// <param name="Constr">�������ݿ����</param>
        /// <param name="un">�û���</param>
        internal void setInfo(DataRow dRow, Point pt, string[] Constr, string un)
        {
            try
            {
                StrCon = Constr;

                UserName = un;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("�����ֶ�") < 0 && col.Caption != "�����ɳ�������" 
                                                            && col.Caption != "�����жӴ���"
                                                            && col.Caption != "���������Ҵ���"
                                                            && col.Caption != "��ȡID"
                                                            && col.Caption != "��ȡ����ʱ��"
                                                            && col.Caption != "��������")
                    {
                        this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                }

                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //����λ��

                //�������ϼ�����,�ɲ鿴��Ƭ
                int k = 0;
                for (int i = 0; i < dRow.Table.Columns.Count; i++)
                {
                    if (dRow.Table.Columns[i].Caption == "��Ƭ1:" || dRow.Table.Columns[i].Caption == "��Ƭ2:" || dRow.Table.Columns[i].Caption == "��Ƭ3:")
                    {
                        k = i;
                        break;
                    }
                }

                DataGridViewTextBoxCell dglc = new DataGridViewTextBoxCell();

                dglc.Value = dRow[dRow.Table.Columns[k]];

                this.dataGridView1.Rows[k].Cells[1] = dglc;

               //if (dRow.Table.Columns.Count != 11)
               //     this.panel3.Visible = true;
               // else
               //     this.panel3.Visible = false;

                this.TopMost = true;

                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "01-����dt��ȡ��Ϣ");
            }
        }

        /// <summary>
        /// ���ô����С
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
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
            catch(Exception ex)
            {
                ExToLog(ex, "02-setSize");
            }
        }

        /// <summary>
        /// ���ô���λ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="iWidth">������</param>
        /// <param name="iHeight">����߶�</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        private void setLocation(int iWidth,int iHeight,int x,int y) {
            try
            {
                if (x + iWidth > Screen.PrimaryScreen.WorkingArea.Width)
                {
                    x = x - iWidth - 10;
                }
                if (y + iHeight > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    y = y - iHeight - 10;
                    if (y < 0) y = 0;
                }
                this.SetDesktopLocation(x, y);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "03-setLocation");
            }
        }       

        /// <summary>
        /// �رմ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           try
            {
                //string exedtl = string.Empty;   //�������
                //string exenum = string.Empty;   //����Ա���        

                //frmExcut frmexec = new frmExcut();

                //frmexec.ShowDialog(this);
                //if (frmexec.DialogResult == DialogResult.OK)
                //{
                //    if (frmexec.ExDetail.Length > 50)
                //    {
                //        MessageBox.Show("���������������50�ַ�,ϵͳ�Զ�ȡǰ50���ַ���Ϊ������Ϣ���б���", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //        exedtl = frmexec.ExDetail.Substring(0, 50);
                //    }
                //    else
                //    {
                //        exedtl = frmexec.ExDetail;
                //    }
                //    exenum = frmexec.ExNum;

                //    if (exenum != "" && exedtl != "")
                //    {
                      
                //        string sqlstr = "update GPS110.������Ϣ110 set GPS110.������Ϣ110.����״̬ = '�Ѵ���',GPS110.������Ϣ110.���������='" + exenum + "',GPS110.������Ϣ110.�������='" + exedtl + "' where GPS110.������Ϣ110.������� ='" + this.dataGridView1.Rows[1].Cells[1].Value.ToString() + "'";
                //        RunCommand(sqlstr);

                //        MessageBox.Show("�ɹ����洦�����", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //        WriteEditLog(sqlstr,"������Ϣ");
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("û��д�봦���������", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);                  
                //}
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click-04-�������");
            }          
        }

        /// <summary>
        /// ִ��SQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "cl110-frmInfo-" + sFunc);
        }

        private void RecToLog(string s)
        {
            CLC.BugRelated.LogWrite(s, Application.StartupPath + "\rec.log");
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">SQL���</param>
        /// <param name="method">������</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + UserName + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'110�Ӵ���','GPS110.������Ϣ110:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex) { ExToLog(ex, "WriteEditLog"); }
        }
    }
}

