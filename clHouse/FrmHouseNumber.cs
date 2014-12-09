using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using clHouse;


namespace clHouse
{
    public partial class FrmPopuNumber : Form
    {
        public string strConn;
        public MapInfo.Windows.Controls.MapControl mapControl;
        public FrmPopuNumber()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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
            cMessageHeight += 40;
            this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
        }


        // 调用显示房屋详细信息的窗口
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 获取身份证号码
            string popuNo = this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            // 访问数据库
            OracleConnection objcon = new OracleConnection(strConn);
            OracleDataAdapter objoda = null;
            DataSet objset = new DataSet();

            try
            {
                objcon.Open();
                string sqlFields = "姓名,身份证号码,民族,性别,人口性质,出生日期,全住址,住址街路巷,住址门牌,楼层,房间号,户籍地址,婚姻状态,配偶姓名,结婚时间,交通工具,车牌号码,服务处所,政治面貌, 暂住证号,暂住证有效期限,入住日期,注销日期,联系电话,重点人口,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                string strSQL = "select " + sqlFields + " from 人口系统 t where 身份证号码='" + popuNo + "'";
                objoda = new OracleDataAdapter(strSQL, objcon);
                objoda.Fill(objset, "table");

                // 判断是否有记录
                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("身份证号码不正确！", "提示", MessageBoxButtons.OK,MessageBoxIcon.Information);
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }

                disPlayInfo(objset.Tables[0], pt);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            finally
            {
                objoda.Dispose();
                objcon.Close();
            }
        }

        private FrmPopuInfo frmMessage = new FrmPopuInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                frmMessage.mapControl = mapControl;
                if (this.frmMessage.Visible == false)
                {
                    this.frmMessage = new FrmPopuInfo();
                    frmMessage.mapControl = mapControl;
                    frmMessage.SetDesktopLocation(-30, -30);
                    frmMessage.Show();
                    frmMessage.Visible = false;
                }
                frmMessage.strConn = strConn;
                frmMessage.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}