using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LBSgisPoliceEdit
{
    static class Program
    {

        [STAThread]

       
        static void Main()
        {
            try
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //FrmLogin fLogin = new FrmLogin();
                //if (fLogin.ShowDialog() == DialogResult.OK)
                //{
                if ((FrmLogin.region1 != "" || FrmLogin.region2 != "") && FrmLogin.temDt != null)
                {
                    Application.Run(new frmMap(FrmLogin.region1, FrmLogin.region2, FrmLogin.temDt));
                }
                //}
               
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
          
        }

    }
}