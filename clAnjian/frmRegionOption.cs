using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.IO;

namespace clAnjian
{
    public partial class frmRegionOption : Form
    {
        public string strConn = "";
        public string strRegion = "";
        public frmRegionOption()
        {
            try
            {
                InitializeComponent();
                this.DialogResult = DialogResult.Cancel;
                radioClass.Click += new EventHandler(radio_Click);
                radioBar.Click += new EventHandler(radio_Click);
                radioPie.Click += new EventHandler(radio_Click);

                CmbMethod.Text = CmbMethod.Items[0].ToString();
                comboRegionLevel.Text = comboRegionLevel.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmRegionOption.���캯��");
            }
        }

        void radio_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioClass.Checked)
                {
                    BtnColH.Enabled = true;
                    BtnColL.Enabled = true;
                    CmbMethod.Enabled = true;
                    numericUpDownClass.Enabled = true;
                }
                else if (radioBar.Checked)
                {
                    BtnColH.Enabled = false;
                    BtnColL.Enabled = false;
                    CmbMethod.Enabled = false;
                    numericUpDownClass.Enabled = false;
                }
                else
                {
                    BtnColH.Enabled = false;
                    BtnColL.Enabled = false;
                    CmbMethod.Enabled = false;
                    numericUpDownClass.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmRegionOption.getZhongdui");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        //���ø�ֵ��ɫ
        private void BtnColH_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    BtnColH.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmRegionOption.getZhongdui");
            }
        }

        //���õ�ֵ��ɫ
        private void BtnColL_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    BtnColL.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmRegionOption.getZhongdui");
            }
        }

        private void comboRegionLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboBoxHighLevel.Items.Clear();
                if (comboRegionLevel.Text == "�ж�")
                {
                    comboBoxHighLevel.Enabled = true;
                    getPaichusuo();
                }
                else if (comboRegionLevel.Text == "������")
                {
                    comboBoxHighLevel.Enabled = true;
                    getZhongdui();
                }
                else
                {
                    comboBoxHighLevel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmRegionOption.comboRegionLevel_SelectedIndexChanged");
            }
        }

        private void getPaichusuo()
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "select �ɳ����� from �����ɳ���";
                if (strRegion != "˳����")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    cmd.CommandText += " where �ɳ����� in ('" + strRegion.Replace(",", "','") + "')";
                }
                comboBoxHighLevel.Items.Add("ȫ��");
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        comboBoxHighLevel.Items.Add(dr.GetValue(0).ToString().Trim());
                    }
                    comboBoxHighLevel.Text = comboBoxHighLevel.Items[0].ToString();
                }
                Conn.Close();
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex, "frmRegionOption.getPaichusuo");
            }
        }

        private void getZhongdui()
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();

                if (strRegion != "˳����")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    cmd.CommandText = "select �ж��� from �������ж� where �����ɳ��� in ('" + strRegion.Replace(",", "','") + "') order by �ж���";
                }
                else
                {
                    cmd.CommandText = "select �ж��� from �������ж� order by �ж���";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        comboBoxHighLevel.Items.Add(dr.GetValue(0).ToString().Trim());
                    }
                    comboBoxHighLevel.Text = comboBoxHighLevel.Items[0].ToString();
                }

                Conn.Close();
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeAnjianLog(ex, "frmRegionOption.getZhongdui");
            }
        }

        private void writeAnjianLog(Exception ex, string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine("��������:�� " + sFunc + "������," + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

    }
}