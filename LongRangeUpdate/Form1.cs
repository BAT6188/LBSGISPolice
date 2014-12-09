using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace LongRangeUpdate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();           
        }

        private bool isContinue = false;

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isContinue)
            {
                System.Diagnostics.Process.Start("SDpolicegis\\LBSgisPolice.exe");
            }
        }

        static bool IsWebResourceAvailable(string webResourceAddress)
        {
            TcpClient tcpClient = null;
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(webResourceAddress), 80);
                tcpClient = new TcpClient();
                tcpClient.Connect(ipep);

                //tcpClient.GetStream();

                return true;
                //HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(webResourceAddress);
                //req.Method = "HEAD";
                //req.Timeout = 15000;
                //HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                //return (res.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Write(ex.Message);

                return false;
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            File.Delete(Application.StartupPath + "\\Config.ini");
            File.Delete(Application.StartupPath + "\\clc.dll");
            Application.DoEvents();
            CLC.ClsFileManagement.SetConstring(Application.StartupPath + "\\Config.ini");
            string fileIP = "http://" + CLC.INIClass.IniReadValue("�ļ�������", "IP1") + "/LBSCHINA";
            bool b = IsWebResourceAvailable(CLC.INIClass.IniReadValue("�ļ�������", "IP1"));
            if (b == false)
            {
                fileIP = "http://" + CLC.INIClass.IniReadValue("�ļ�������", "IP2") + "/LBSCHINA";
                b = IsWebResourceAvailable(CLC.INIClass.IniReadValue("�ļ�������", "IP2"));
                if (b == false)
                {
                    //MessageBox.Show("��̨�ļ�����������������,���ܽ��г�����£����ܻᵼ�³������в�����,�����������ϵ����Ա!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    Application.DoEvents();
                    CLC.ClsFileManagement.FileUpdate(fileIP);
                }
            }
            else
            {
                Application.DoEvents();
                CLC.ClsFileManagement.FileUpdate(fileIP);
            }
            isContinue = true;
            this.Close();
        }
    }
}