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
                writeAnjianLog(ex, "frmRegionOption.构造函数");
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

        //设置高值颜色
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

        //设置低值颜色
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
                if (comboRegionLevel.Text == "中队")
                {
                    comboBoxHighLevel.Enabled = true;
                    getPaichusuo();
                }
                else if (comboRegionLevel.Text == "警务室")
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
                cmd.CommandText = "select 派出所名 from 基层派出所";
                if (strRegion != "顺德区")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    cmd.CommandText += " where 派出所名 in ('" + strRegion.Replace(",", "','") + "')";
                }
                comboBoxHighLevel.Items.Add("全部");
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

                if (strRegion != "顺德区")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    cmd.CommandText = "select 中队名 from 基层民警中队 where 所属派出所 in ('" + strRegion.Replace(",", "','") + "') order by 中队名";
                }
                else
                {
                    cmd.CommandText = "select 中队名 from 基层民警中队 order by 中队名";
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
            sw.WriteLine("案件分析:在 " + sFunc + "方法中," + DateTime.Now.ToString() + ": ");
            sw.WriteLine(ex.ToString());
            sw.WriteLine();
            sw.Close();
        }

    }
}