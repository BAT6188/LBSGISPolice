using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace clAnjian
{
    public partial class frmDotDensity : Form
    {
        public int numPerDot=10;
        public Color dotColor = Color.Red;
        public string strConn = "";
        public string strRegion = "";

        #region 输入法
        //声明一些API函数
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetOpenStatus(IntPtr himc);
        [DllImport("imm32.dll")]
        public static extern bool ImmSetOpenStatus(IntPtr himc, bool b);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetConversionStatus(IntPtr himc, ref int lpdw, ref int lpdw2);
        [DllImport("imm32.dll")]
        public static extern int ImmSimulateHotKey(IntPtr hwnd, int lngHotkey);
        private const int IME_CMODE_FULLSHAPE = 0x8;
        private const int IME_CHOTKEY_SHAPE_TOGGLE = 0x11;
        #endregion


        public frmDotDensity()
        {
            try
            {
                InitializeComponent();
                this.DialogResult = DialogResult.Cancel;
                comboRegionLevel.Text = comboRegionLevel.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmDotDensity.构造函数");
            }
        }

        private void buttonThemeColor_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    buttonThemeColor.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmDotDensity.buttonThemeColor_Click");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
                this.Dispose();
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmDotDensity.buttonCancel_Click");
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    numPerDot = Convert.ToInt32(textNumPerDot.Text);

                }
                catch
                {
                    MessageBox.Show("请输入数字!");
                    textNumPerDot.Text = "";
                    return;
                }
                dotColor = buttonThemeColor.BackColor;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmDotDensity.buttonOK_Click");
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
                writeAnjianLog(ex, "frmDotDensity.comboRegionLevel_SelectedIndexChanged");
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
                writeAnjianLog(ex, "frmDotDensity.getPaichusuo");
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
                writeAnjianLog(ex, "frmDotDensity.getZhongdui");
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

        private void textNumPerDot_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                IntPtr HIme = ImmGetContext(this.Handle);
                if (ImmGetOpenStatus(HIme)) //如果输入法处于打开状态
                {
                    int iMode = 0;
                    int iSentence = 0;
                    bool bSuccess = ImmGetConversionStatus(HIme, ref iMode, ref iSentence); //检索输入法信息
                    if (bSuccess)
                    {
                        if ((iMode & IME_CMODE_FULLSHAPE) > 0) //如果是全角
                            ImmSimulateHotKey(this.Handle, IME_CHOTKEY_SHAPE_TOGGLE); //转换成半角
                    }
                }
            }
            catch (Exception ex)
            {
                writeAnjianLog(ex, "frmDotDensity.getZhongdui");
            }
        }
    }
}