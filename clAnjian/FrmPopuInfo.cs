using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OracleClient;

using MapInfo.Mapping;
using MapInfo.Geometry;
using MapInfo.Styles;
using MapInfo.Data;

namespace clAnjian
{
    public partial class FrmPopuInfo : Form
    {
        public FrmPopuInfo()
        {
            InitializeComponent();
        }
        public MapInfo.Windows.Controls.MapControl mapControl;
        public string strConn;
        private DataRow row = null;

        //传递dt获取信息
        internal void setInfo(DataRow dRow, System.Drawing.Point pt)
        {
            this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.dataGridView1.Rows.Clear();

            row = dRow;
            foreach (DataColumn col in dRow.Table.Columns)
            {
                if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码" && col.Caption != "所属中队代码" && col.Caption != "所属警务室代码" && col.Caption != "抽取ID" && col.Caption != "抽取更新人" && col.Caption != "抽取更新时间")
                {
                    this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                }
            }
            this.setSize();
            this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置

            //在姓名上加链接,可查看照片
            DataGridViewLinkCell dglc = new DataGridViewLinkCell();
            dglc.Value = dRow[dRow.Table.Columns[0]];
            dglc.ToolTipText = "查看照片";
            this.dataGridView1.Rows[0].Cells[1] = dglc;

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
            cMessageHeight += 60;
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


        // 定位按钮
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (flashFt != null)
                {
                    try
                    {
                        flashFt.Style = defaultStyle;
                        flashFt.Update();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                string[] winStr = new string[] { row["X"].ToString(), row["Y"].ToString(), "人口系统", row["身份证号码"].ToString(), row["姓名"].ToString() };
                DinPoint(winStr);
                k = 0;
                this.timer1.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private Style defaultStyle = null;
        private Feature flashFt = null;
        // 图层上画点并定位
        private void DinPoint(string[] winStr)
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl.Map.Layers["临时图层"];
                Table tableTem = ftla.Table;

                double dx = 0, dy = 0;

                try
                {
                    dx = Convert.ToDouble(winStr[0]);
                    dy = Convert.ToDouble(winStr[1]);
                }
                catch
                {
                    MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                if (dx > 0 && dy > 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);
                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 9));

                    MapInfo.Styles.BitmapPointStyle pStyle = new MapInfo.Styles.BitmapPointStyle("ren.bmp");
                    cs = new CompositeStyle(pStyle);
                    Feature pFeat = new Feature(tableTem.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["表_ID"] = winStr[3];
                    pFeat["表名"] = winStr[2];
                    pFeat["名称"] = winStr[4];
                    tableTem.InsertFeature(pFeat);
                    flashFt = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + winStr[3] + "' and 表名='" + winStr[2] + "'"));
                    defaultStyle = flashFt.Style;
                    if (flashFt != null)
                    {
                        mapControl.Map.SetView(flashFt);
                    }
                    else
                    {
                        MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("该对象未定位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        int k = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                MapInfo.Styles.BitmapPointStyle bitmappointstyle = null;
                if (k % 2 == 0)
                {
                    bitmappointstyle = new MapInfo.Styles.BitmapPointStyle("ren.bmp", BitmapStyles.None, Color.Red, 18);
                }
                else
                {
                    bitmappointstyle = new MapInfo.Styles.BitmapPointStyle("ren2.bmp", BitmapStyles.None, Color.Red, 18);
                }
                try
                {
                    flashFt.Style = bitmappointstyle;
                    flashFt.Update();
                }
                catch { }
                k++;
                if (k == 10)
                {
                    timer1.Stop();
                }
            }
            catch
            {
                timer1.Stop();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleCommand oracleCmd = null;
            System.Data.OracleClient.OracleDataReader dataReader = null;

            if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "姓名:")
            {
                int k = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "身份证号码:")
                    {
                        k = i;
                        break;
                    }
                }

                try
                {
                    FrmImage fimage = new FrmImage();
                    myConnection.Open();
                    string sqlstr = "select 照片 from  人口照片 where 身份证号码='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";
                    oracleCmd = new OracleCommand(sqlstr, myConnection);
                    dataReader = oracleCmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        byte[] bytes = new byte[2000000];
                        if (dataReader.IsDBNull(0))
                        {
                            System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, 2000000);

                            Stream fs = new MemoryStream(bytes);
                            fimage.pictureBox1.Image = Image.FromStream(fs);
                            fimage.pictureBox1.Invalidate();
                            fs.Close();
                            fimage.TopMost = true;
                            fimage.ShowDialog();
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    oracleCmd.Dispose();
                    myConnection.Close();
                }
            }
        }
    }
}