using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using nsGetFromName;
using nsInfo;
using MapInfo.Windows.Controls;

namespace clZonghe
{
    public partial class FrmNumber : Form
    {
        public MapControl mapControl = null;
        public FrmNumber()
        {
            InitializeComponent();
        }
        public string strConn;
        public string tabName;
        public string getFromNamePath;

        // 给所有房屋编号设置链接
        public void setfrmInfo(DataTable table)
        {
            this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.dataGridView1.Rows.Clear();

            foreach (DataRow row in table.Rows)
            {
                this.dataGridView1.Rows.Add(new object[] { row[0] + ":", row[1] });
            }

            setSize();
        }

        private void setSize()
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


        // GridView单击事件
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 访问数据库
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter oracleDat = null;
            DataSet objset = new DataSet();

            string sqlStr = "";      // SQL语句
            string colName = tabName.Substring(0, tabName.Length - 1);
            switch (colName)
            {
                case "相关案件":
                    GetFromName getName = new GetFromName("案件信息");
                    string AnNumber = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    AnNumber = AnNumber.Substring(0, AnNumber.Length - 1);
                    sqlStr = "select " + getName.FrmFilelds + " from " + getName.TableName + " t where " + getName.ObjID + "='" + AnNumber + "'";
                    break;
                case "房屋编号":
                    GetFromName getName1 = new GetFromName("出租屋房屋系统");
                    string houseNo = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    houseNo = houseNo.Substring(0, houseNo.Length - 1);
                    sqlStr = "select " + getName1.FrmFilelds + " from " + getName1.TableName + " t where " + getName1.ObjID + "='" + houseNo + "'"; 
                    break;
                case "涉案人员":
                case "当前居住人数":
                case "历史居住人数":
                case "暂住证有效期内人数":
                case "未办暂住证人数":
                    GetFromName getName2 = new GetFromName("人口系统");
                    string cardId = this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                    sqlStr = "select " + getName2.FrmFilelds + " from " + getName2.TableName + " t where " + getName2.ObjID + "='" + cardId + "'";
                    break;
            }

            try
            {
                myConnection.Open();
                oracleDat = new System.Data.OracleClient.OracleDataAdapter(sqlStr, myConnection);
                oracleDat.Fill(objset, "table");

                if (objset.Tables[0].Rows.Count == 0)
                {
                    switch (tabName)
                    {
                        case "相关案件:":
                            MessageBox.Show("无涉案记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            break;
                        case "房屋编号:":
                            MessageBox.Show("房屋未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            break;
                        case "当前居住人数:":
                        case "历史居住人数:":
                        case "暂住证有效期内人数:":
                        case "未办暂住证人数:":
                        case "涉案人员:":
                            string meName = tabName.Substring(0, tabName.Length - 2);
                            MessageBox.Show(meName + "未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            break;

                    }
                }
                else
                {
                    Point pt = new Point();
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);

                    disPlayInfo(objset.Tables[0], pt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                oracleDat.Dispose();
                myConnection.Close();
            }
        }

        private FrmGLMessagee frmglMessage = new FrmGLMessagee();
        private void disPlayInfo(DataTable dt, Point pt)
        {
            try
            {
                frmglMessage.mapControl = mapControl;
                if (this.frmglMessage.Visible == false)
                {
                    this.frmglMessage = new FrmGLMessagee();
                    frmglMessage.mapControl = mapControl;
                    frmglMessage.Show();
                    frmglMessage.Visible = true;
                }
                frmglMessage.strConn = strConn;
                frmglMessage.tabName = tabName;
                frmglMessage.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}