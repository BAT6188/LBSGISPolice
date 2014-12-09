using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LBSgisPolice
{
    public partial class frmAbout : Form
    {
        /// <summary>
        /// 构造函数 获取系统版本及地图影像版本
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        public frmAbout()
        {
            InitializeComponent();

            string sVerb ="";
            int iA = 2,sB;
            string  sC;

            try
            {
                sVerb = CLC.INIClass.IniReadValue("版本", "1");
                iA += Convert.ToInt16(sVerb) / 100;
                sB = Convert.ToInt16(sVerb)/10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label1.Text = "顺德警用地理信息系统 版本 " + iA.ToString() + "." + sB + "." + sC;

                sVerb = CLC.INIClass.IniReadValue("版本", "5");
                sB = Convert.ToInt16(sVerb) / 10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label2.Text = "顺德警用数据编辑系统 版本 " + iA.ToString() + "." + sB + "." + sC;

                sVerb = CLC.INIClass.IniReadValue("版本", "2");
                sB = Convert.ToInt16(sVerb) / 10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label3.Text = "地图数据 版本 1." + sB + "." + sC;

                sVerb = CLC.INIClass.IniReadValue("版本", "3");
                sB = Convert.ToInt16(sVerb) / 10;
                sC = sVerb.Substring(sVerb.Length - 1, 1);
                label4.Text = "影像数据 版本 1." + sB + "." + sC;
            }
            catch(Exception ex)
            {
                ExToLog(ex,"初始化");
            }
        }

        /// <summary>
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "Main-frmAbout-" + sFunc);
        }

    }
}