using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace LBSgisPolice
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            frmLogin fLogin = new frmLogin();
            string root = Application.StartupPath;
            CLC.INIClass.IniPathSet(root.Remove(root.LastIndexOf("\\")) + "\\config.ini");
            try
            {
                string sVerb = CLC.INIClass.IniReadValue("版本", "1");
                int iA = 2;
                string sB,sC ;
                iA += Convert.ToInt16(sVerb) / 100;
                sB = sVerb.Substring(sVerb.Length - 2, 1);
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                fLogin.labelVerb.Text = "V "+ iA.ToString()+"."+sB+"." + sC;
            }
            catch { }
            if (fLogin.ShowDialog() == DialogResult.OK)
            {
                string fileIP = testFileServer();
                Application.DoEvents();
                if (isContinue)
                {
                    try
                    {
                        ClsFileManagement.ClsFileManagement.SetConstring(root.Remove(root.LastIndexOf("\\")) + @"\config.ini", "http://" + fileIP + "/LBSCHINA");
                        ClsFileManagement.ClsFileManagement.FileUpdate();

                        frmMap fMain = new frmMap();
                        if (fMain.IsDisposed == false)
                        {
                            Application.Run(fMain);
                        }
                    }
                    catch
                    { }
                }
            }
        }
        
        private static bool isContinue = false; 
        private static string testFileServer() {
            Application.DoEvents();
            string root = Application.StartupPath;
            CLC.INIClass.IniPathSet(root.Remove(root.LastIndexOf("\\")) + "\\config.ini");
            string fileIP = "";
            while (isContinue == false)
            {
                fileIP = "http://" + CLC.INIClass.IniReadValue("文件服务器", "IP1") + "/LBSCHINA";
                bool b = IsWebResourceAvailable(CLC.INIClass.IniReadValue("文件服务器", "IP1"));
                if (b == false)
                {
                    fileIP = "http://" + CLC.INIClass.IniReadValue("文件服务器", "IP2") + "/LBSCHINA";
                    b = IsWebResourceAvailable(CLC.INIClass.IniReadValue("文件服务器", "IP2"));
                    if (b == false)
                    {
                        try
                        {
                            DialogResult dr = MessageBox.Show("目前所有文件服务器均不能连接，将可能无法更新系统，" +
                                "较旧的版本可能导致系统运行不正常或错误，如您确定需要进入系统请点击忽略进入，" +
                                "如重新测试连接请点击重试，如需要退出请点击中止并与管理员联系。",
                                "用户确认", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
                            if (dr == DialogResult.Abort)
                            {
                                break;
                            }
                            else if (dr == DialogResult.Retry)
                            {
                                isContinue = false;
                            }
                            else
                            {
                                isContinue = true;
                                break;
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        isContinue = true;
                    }

                }
                else
                {
                    isContinue = true;
                }
            }
            return fileIP;
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
    }
}