using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;

namespace cl110
{
    public partial class frmAnjianType : Form
    {
        public frmAnjianType()
        {
            InitializeComponent();
        }

        public double ds;

        private void frmAnjianType_Load(object sender, EventArgs e)
        {
            try {
                // 半径值
                this.txtRadius.Text = ds.ToString();

                // 往listBoxStart中添加项
                string sql = "select distinct 案件类型 from GPS110.报警信息110 where 案件类型 not in ('治安案件','刑事案件')";
                OracleDataReader order = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql);
                this.listBoxStart.Items.Clear();
                while (order.Read())
                {
                    this.listBoxStart.Items.Add(order[0].ToString());
                }


                // 往listBoxEnd中添加项
                this.listBoxEnd.Items.Clear();
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                string strSit = CLC.INIClass.IniReadValue("110", "案件类型");

                string[] strType = null;
                if (strSit != null && strSit != "")
                    strType = strSit.Split(',');

                for (int i = 0; i < strType.Length; i++)
                {
                    this.listBoxEnd.Items.Add(strType[i].ToString());
                    this.listBoxStart.Items.Remove(strType[i].ToString());
                }

            }
            catch (Exception ex)
            {
                ExToLog(ex, "frmAnjianType_Load");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            try
            {
                this.listBoxEnd.Items.Add(this.listBoxStart.SelectedItem.ToString());
                this.listBoxStart.Items.Remove(this.listBoxStart.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnRight_Click");
            }
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            try
            {
                this.listBoxStart.Items.Add(this.listBoxEnd.SelectedItem.ToString());
                this.listBoxEnd.Items.Remove(this.listBoxEnd.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnLeft_Click");
            }
        }

        // 写入配置文件
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // 保存查询半径的配置
                double videodis = Convert.ToDouble(this.txtRadius.Text.ToString());
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                CLC.INIClass.IniWriteValue("视频", "距离", videodis.ToString());

                // 保存案件类型的配置
                exePath = Application.StartupPath + "\\ConfigBJXX.ini";
                CLC.INIClass.IniPathSet(exePath);
                string strValue = "";
               
                for (int i = 0; i < this.listBoxEnd.Items.Count; i++)
                {
                    strValue += this.listBoxEnd.Items[i].ToString() + ",";
                }
                if (strValue != "")
                    strValue = strValue.Remove(strValue.LastIndexOf(','));

                CLC.INIClass.IniWriteValue("110", "案件类型", strValue);
                MessageBox.Show("保存成功!","提示");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败! \n" + ex.Message, "提示");
                ExToLog(ex, "btnOK_Click");
            }
        }

        private void txtRadius_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                string s = "1234567890." + (char)8;
                if (s.IndexOf(e.KeyChar.ToString()) < 0)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "txtRadius_KeyPress");
            }
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "cl110-frmAnjianType-" + sFunc);
        }
    }
}