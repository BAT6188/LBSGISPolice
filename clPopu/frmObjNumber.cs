using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MapInfo.Windows.Controls;

namespace clPopu
{
    public partial class frmObjNumber : Form
    {
        public frmObjNumber()
        {
            InitializeComponent();
        }
        public MapControl mapControl = null;
        public string strConn;
        public string tabName;
        public string getFromNamePath;
        public string LayerName;

        // 给所有房屋编号设置链接
        public void setfrmInfo(DataTable table,string tableName)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                tabName = tableName;
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 访问数据库
            System.Data.OracleClient.OracleConnection objcon = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter objoda = null;
            DataSet objset = new DataSet();

            // 获取身份证号码
            CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
            string IdCard = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Substring(0, this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Length - 1);
            string sqlStr = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID+ "='" + IdCard + "'";
            try
            {
                objcon.Open();
                objoda = new System.Data.OracleClient.OracleDataAdapter(sqlStr, objcon);
                objoda.Fill(objset);

                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("请确认编号是否正确！", "系统提示", MessageBoxButtons.OK);
                    objoda.Dispose();
                    return;
                }

                DataRow row = objset.Tables[0].Rows[0];

                System.Drawing.Point pt = new System.Drawing.Point();
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }

                disPlayInfo(objset.Tables[0], pt);
                objoda.Dispose();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick");
            }
            finally
            { objcon.Close(); }
        }


        private frmObjInfo frmhouse = new frmObjInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (this.frmhouse.Visible == false)
                {
                    this.frmhouse = new frmObjInfo();
                    frmhouse.SetDesktopLocation(-30, -30);
                    frmhouse.Show();
                    frmhouse.Visible = false;
                }
                frmhouse.mapControl = mapControl;
                frmhouse.LayerName = LayerName;
                frmhouse.getFromNamePath = getFromNamePath;
                frmhouse.strConn = strConn;
                frmhouse.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clPopu-frmObjNumber-" + sFunc);
        }
    }
}