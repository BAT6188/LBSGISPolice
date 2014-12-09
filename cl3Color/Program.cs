using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace cl3Color
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            
                Application.Run(new frmReport());
            
        }
    }
}