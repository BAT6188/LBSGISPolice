using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;

using MapInfo.Windows.Controls;

namespace clPopu
{
    public partial class FrmHouseNumber : Form
    {
        public string strConn;
        public MapControl mapControl = null;
        private string tabName = "";
        public string getFromNamePath;
        public string LayerName;
        public string[] StrCon;

        public FrmHouseNumber()
        {
            InitializeComponent();
            getStrConn();
        }

        //读取配置文件，设置数据库连接字符串
        private void getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");

                StrCon = new string[] { datasource, userid, password };
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getStrConn");
            }
        }

        // 给所有房屋编号设置链接
        public void setfrmInfo(DataTable table,string tableName)
        {
            try
            {
                tabName = tableName;
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataRow row in table.Rows)
                {
                    this.dataGridView1.Rows.Add(new object[] { row[0] + ":", row[1] });
                }

                setSize();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setfrmInfo");
            }
        }

        private void setSize()
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].Width > 300)
                    {
                        this.dataGridView1.Columns[1].Width = 300;
                        this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    double width = this.dataGridView1.Columns[1].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView1.Rows[i].Cells[1].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView1.Rows[i].Height = (this.dataGridView1.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + 30;

                if (this.dataGridView1.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView1.Rows[i].Height;
                }
                cMessageHeight += 50;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

        // 调用显示房屋详细信息的窗口
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 获取房屋编号
            string houseN = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(0, this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Length - 1);
            // 访问数据库
            OracleConnection objcon = new OracleConnection(strConn);
            OracleDataAdapter objoda = null;
            DataSet objset = new DataSet();

            try
            {
                objcon.Open();
                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + houseN + "'";
                objoda = new OracleDataAdapter(strSQL, objcon);
                objoda.Fill(objset, "table");
                objoda.Dispose();

                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("请确认编号是否正确！", "系统提示", MessageBoxButtons.OK);
                    return;
                }

                DataRow row = objset.Tables[0].Rows[0];

                System.Drawing.Point pt = new System.Drawing.Point();

                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }

                disPlayInfo(objset.Tables[0],pt,tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick"); 
            }
            finally
            {
                objcon.Close();
            }
        }

        private FrmHouseInfo frmhouse = new FrmHouseInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string tabName)
        {
            try
            {
                if (this.frmhouse.Visible == false)
                {
                    this.frmhouse = new FrmHouseInfo();
                    frmhouse.SetDesktopLocation(-30, -30);
                    frmhouse.Show();
                    frmhouse.Visible = false;
                }
                frmhouse.mapControl = mapControl;
                frmhouse.LayerName = LayerName;
                frmhouse.getFromNamePath = getFromNamePath;
                frmhouse.strConn = strConn;
                frmhouse.setInfo(dt.Rows[0], pt, tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clPopu-FrmHouseNumber-" + sFuns);
        }
    }
}