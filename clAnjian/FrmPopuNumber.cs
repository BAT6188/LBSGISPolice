using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;

namespace clAnjian
{
    public partial class FrmPopuNumber : Form
    {
        public FrmPopuNumber()
        {
            InitializeComponent();
        }
        public string strConn;
        public MapInfo.Windows.Controls.MapControl mapControl;

        //传递dt获取信息
        internal void setInfo(DataTable table)
        {
            this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.dataGridView1.Rows.Clear();

            foreach (DataRow row in table.Rows)
            {
                this.dataGridView1.Rows.Add(row[0].ToString() + ":", row[1].ToString());
            }
            this.setSize();

            //this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
            this.Visible = true;
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

        private void setLocation(int iWidth, int iHeight, int x, int y)
        {
            if (x + iWidth > Screen.PrimaryScreen.WorkingArea.Width)
            {
                x = x - iWidth - 10;
            }
            if (y + iHeight > Screen.PrimaryScreen.WorkingArea.Height)
            {
                y = y - iHeight - 10;
                if (y < 0) y = 0;
            }
            this.SetDesktopLocation(x, y);
        }

        // 单击网格事件
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出

            OracleConnection oracleConn = new OracleConnection(strConn);
            OracleDataAdapter oracleOda = null;
            DataSet objset = null;

            try
            {
                string cardId = this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                string sqlStr = "select 姓名,身份证号码,民族,性别,人口性质,出生日期,全住址,住址街路巷,住址门牌,楼层,房间号,户籍地址,婚姻状态,配偶姓名,结婚时间,交通工具,车牌号码,服务处所,政治面貌, 暂住证号,暂住证有效期限,入住日期,注销日期,联系电话,重点人口,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y from 人口系统 t where 身份证号码='" + cardId + "'";
                oracleConn.Open();
                oracleOda = new OracleDataAdapter(sqlStr, oracleConn);
                objset = new DataSet();
                oracleOda.Fill(objset);

                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("身份证号码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }
                displayInfo(objset.Tables[0],pt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                oracleOda.Dispose();
                oracleConn.Close();
            }
        }

        private FrmPopuInfo frmMessage = new FrmPopuInfo();
        private void displayInfo(DataTable table,Point pt)
        {
            try
            {
                if (frmMessage.Visible == false)
                {
                    frmMessage = new FrmPopuInfo();
                    frmMessage.strConn = strConn;
                    frmMessage.Show();
                    frmMessage.Visible = false;
                }
                frmMessage.mapControl = mapControl;
                frmMessage.setInfo(table.Rows[0], pt);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
