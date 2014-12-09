using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SDPoliceGISSetup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void link_MouseEnter(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.ForeColor = Color.Red;
        }

        private void link_MouseHover(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.ForeColor = Color.Red;
        }

        private void link_MouseLeave(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.ForeColor = Color.White;
        }

        private void lSetupOracleClient_Click(object sender, EventArgs e)
        {
            string strExePath = Application.StartupPath + "\\Oracle10Client\\client\\setup.exe";
            if (File.Exists(strExePath))
            {
                Cursor.Current = Cursors.WaitCursor;
                System.Diagnostics.Process.Start(strExePath);
                Cursor.Current = Cursors.Default;
            }
            else
            {
                MessageBox.Show("Oracle客户端安装文件不存在.");
            }
        }

        private void lSetupOracleService_Click(object sender, EventArgs e)
        {
            //先检查Oracle是否安装完好
            //如果安装好了,还找不到,估计安装目录不对,提示指示安装目录
            string oracleClientPath = "c:\\oracle";
            if (Directory.Exists(oracleClientPath+"\\product\\10.2.0\\client_1\\NETWORK\\ADMIN"))
            {
                File.Copy(Application.StartupPath + "\\tnsnames.ora", oracleClientPath + "\\product\\10.2.0\\client_1\\NETWORK\\ADMIN\\tnsnames.ora", true);
                MessageBox.Show("Oracle服务配置完毕,请进行下一步的安装.","提示");
            }
            else {
                DialogResult dResult =  MessageBox.Show("未找到Oracle客户端的安装目录,如果已安装,请指定.如'd:\\oracle'\r\t否则请先安装Oracle\r\t是否指定目录?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dResult == DialogResult.Yes)
                {
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) {
                        oracleClientPath = folderBrowserDialog1.SelectedPath;
                        if (Directory.Exists(oracleClientPath + "\\product\\10.2.0\\client_1\\NETWORK\\ADMIN"))
                        {
                            if (radioVideoNet.Checked)
                            {
                                File.Copy(Application.StartupPath + "\\SDPoliceSetup_VideoNet\\tnsnames.ora", oracleClientPath + "\\product\\10.2.0\\client_1\\NETWORK\\ADMIN\\tnsnames.ora", true);
                            }
                            else {
                                File.Copy(Application.StartupPath + "\\SDPoliceSetup_PoliceNet\\tnsnames.ora", oracleClientPath + "\\product\\10.2.0\\client_1\\NETWORK\\ADMIN\\tnsnames.ora", true);                           
                            }
                            MessageBox.Show("Oracle服务配置完毕,请进行下一步的安装.", "提示");
                        }
                    }
                }
            }
        }

        private void linkSetupDotnet_Click(object sender, EventArgs e)
        {
            string strExePath = Application.StartupPath + "\\Mapxtreme 2005 V6.7 SCP\\INSTALL\\MSDOTNETFRAMEWORK\\v2_0\\dotnetfx.exe";
            if (File.Exists(strExePath))
            {
                Cursor.Current = Cursors.WaitCursor;
                System.Diagnostics.Process.Start(strExePath);
                Cursor.Current = Cursors.Default;
            }
            else
            {
                MessageBox.Show("安装文件不存在.");
            }
        }

        private void linkSetupMapxtreme_Click(object sender, EventArgs e)
        {
            string strExePath = Application.StartupPath + "\\Mapxtreme 2005 V6.7 SCP\\INSTALL\\InstallRuntime\\MXTRunSCP.exe";
            if (File.Exists(strExePath))
            {
                Cursor.Current = Cursors.WaitCursor;
                System.Diagnostics.Process.Start(strExePath);
                Cursor.Current = Cursors.Default;
            }
            else
            {
                MessageBox.Show("安装文件不存在.");
            }
        }

        private void lSetupGIS_Click(object sender, EventArgs e)
        {
            string strExePath = Application.StartupPath;
            if (radioVideoNet.Checked)
            {
                strExePath += "\\SDPoliceSetup_VideoNet\\PoliceGISSetup.msi";
            }
            else
            {
                strExePath += "\\SDPoliceSetup_PoliceNet\\PoliceGISSetup.msi";
            }
            if (File.Exists(strExePath))
            {
                Cursor.Current = Cursors.WaitCursor;
                System.Diagnostics.Process.Start(strExePath);
                Cursor.Current = Cursors.Default;
            }
            else
            {
                MessageBox.Show("安装程序不存在,请联系供应商.");
            }
        }

        private void lExit_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
                this.Dispose();
            }
            catch { }
        }

    }
}