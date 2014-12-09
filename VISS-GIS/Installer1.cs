using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;


namespace path
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }

       
        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {   
            try
            {
                string filepath = this.Context.Parameters["targetdir"] + @"\path.bat";
                string folder = filepath.Substring(0, filepath.Length - 10);
                string cmd = "reg add " + "\"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment\" /v \"path\" /d \"%path%;" + folder + "\" /t REG_EXPAND_SZ /f";

                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }

                System.IO.FileStream fs = System.IO.File.Create(filepath);
                byte[] con = System.Text.Encoding.GetEncoding("GB2312").GetBytes(cmd);
                fs.Write(con, 0, con.Length);
                fs.Close();

                Process pro = new Process();
                pro.StartInfo.UseShellExecute = true;
                pro.StartInfo.FileName = filepath;
                pro.StartInfo.CreateNoWindow = true;
                pro.Start();
            }
            catch { MessageBox.Show("环境变量没有注册成功！", "Viss_GIS", MessageBoxButtons.OK, MessageBoxIcon.Warning); }

            base.OnAfterInstall(savedState);
        }

       
    }
}