using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OracleClient;

namespace clKaKou
{
    public partial class FrmImage : Form
    {

        public string username = string.Empty;
        public string[] SQLCON;
        public FrmImage()
        {
            
            InitializeComponent();
           
        }

        /// <summary>
        /// 关闭窗体 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 保存图片到本地
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnsave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog MyDlg = new SaveFileDialog();
                MyDlg.AddExtension = true;
                MyDlg.DefaultExt = "jpg";
                MyDlg.Filter = "图像文件(Jpeg,Jpg)|*.jpg;*.jpeg;|所有文件(*.*)|*.*";
                if (MyDlg.ShowDialog() == DialogResult.OK)
                {
                    string MyFileName = MyDlg.FileName;

                    this.pictureBox1.Image.Save(MyFileName);

                    WriteEditLog("照片另存为: " + MyFileName, "保存照片", "详细信息");

                    MessageBox.Show("保存完毕", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex,"btnsave_Click");
                MessageBox.Show("保存图片时发生错误，无法完成保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="method">操作方式</param>
        /// <param name="tablename">操作表</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {           
            string strExe = "insert into 操作记录 values('" + username + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'治安卡口','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
            RunCommand(strExe);     
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(SQLCON[0], SQLCON[1], SQLCON[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clKaKou-FrmImage-" + sFunc);
        }
    }
}