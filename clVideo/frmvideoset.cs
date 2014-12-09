using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace clVideo
{
    public partial class frmvideoset : Form
    {
        public frmvideoset()
        {
            InitializeComponent();
        }

        string inipath = string.Empty;

        public string Sip = string.Empty;
        public string Sport = string.Empty;
        public string Sfolder = string.Empty;
        public string Susername = string.Empty;
        public string Spassword = string.Empty;

        /// <summary>
        /// 保存按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Sip = this.txtip.Text.Trim();
                Sport = this.txtport.Text.Trim();
                Sfolder = this.txtfolder.Text.Trim();
                Susername = this.txtusername.Text.Trim();
                Spassword = this.txtpswd.Text.Trim();

                if (Sip == "" || Sport == "" || Sfolder == "" || Susername == "" || Spassword == "")
                {
                    MessageBox.Show("所填项不能为空，请确认！","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                }

                //WriteEditLog(ALARMSYS + ":" + ALARMUSER + ":" + this.txtdist.Text.Trim(), "更改卡口设置", "配置文件");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }     
        }

        /// <summary>
        /// 关闭窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 设置视频客户端
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnClient_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofg = new OpenFileDialog();
                ofg.Filter = "应用程序(*.exe)|*.exe";
                ofg.Title = "选择视频监控客户端";
                ofg.AddExtension = true;
                ofg.InitialDirectory = Application.StartupPath.Remove(Application.StartupPath.LastIndexOf("\\")) + "\\Carrier\\";

                if (ofg.ShowDialog() == DialogResult.OK)
                {
                    string FileName = ofg.FileName;

                    if (FileName.IndexOf(".exe") > -1)
                    {
                        FileName = FileName.Remove(FileName.LastIndexOf("\\")) + "\\surveillance1.exe";

                        this.txtClient.Text = FileName;
                    }
                    else
                    {
                        MessageBox.Show("选择错误，请重新选择", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "btnClient_Click"); }                
        }

        /// <summary>
        /// 异常日志输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clVideo-frmvideoset-" + sFunc);
        }
    }
}