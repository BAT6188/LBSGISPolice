using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
namespace LBSgisPolice110
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string MyModuleName = Process.GetCurrentProcess().MainModule.ModuleName;
                string MyProcessName = System.IO.Path.GetFileNameWithoutExtension(MyModuleName);
                Process[] MyProcesses = Process.GetProcessesByName(MyProcessName);
                if (MyProcesses.Length > 1)
                {
                    //MessageBox.Show("程序已经运行！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //FrmMain.ActiveForm
                    return;
                }
                else
                {
                    //测试数据

                    //int i = 0;
                    //Array.Copy(v, 0, buffer, i, v.Length);  //版本号复制

                     if (args.Length == 0)
                     {
                         args = new string[6] {"viss","172.13.2.10","8081","gis" ,"12345678","15"};
                     }

                     int length = args.Length;
                     string[] VideoArr = new string[length];
                     
                     //MessageBox.Show(length.ToString());
                     //MessageBox.Show(args[0].ToString() + args[1].ToString() + args[2].ToString() + args[3].ToString() + args[4].ToString());

                     int i = 0;

                     if (args != null && args.Length > 0)
                     {
                         Array.Copy(args,0, VideoArr,i, length);       
                     }                   
                    
                    if (args.Length > 0)
                    {
                        //Boolean isnum = System.Text.RegularExpressions.Regex.IsMatch(args[0], @"^\d+(\d*)?$");
                        //string v = args[1];                        
                        //if (isnum)
                        //{
                            //int p = Convert.ToInt32(args[0]);
                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Application.Run(new FrmMain(VideoArr));                           
                        //}
                        //else
                        //{
                        //    MessageBox.Show("输入的参数非整型，无法启动程序", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        //}
                    }                   
                }
            }
            catch
            { }
        }

        
    }
}