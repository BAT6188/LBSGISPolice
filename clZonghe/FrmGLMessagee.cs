using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OracleClient;

using MapInfo.Data;
using MapInfo.Engine;
using MapInfo.Mapping;
using MapInfo.Styles;
using MapInfo.Windows.Controls;
using nsGetFromName;
using MapInfo.Geometry;

namespace clZonghe
{
    public partial class FrmGLMessagee : Form
    {
        public string strConn;
        public string tabName;
        private DataRow row;
        public MapControl mapControl = null;
        public FrmGLMessagee()
        {
            InitializeComponent();
        }

        //传递dt获取信息
        private string[] dqPopu = null, lsPopu = null, wzPopu = null, yzPopu = null;
        public void setInfo(DataRow dRow, System.Drawing.Point pt)
        {
            this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.dataGridView1.Rows.Clear();

            row = dRow;
            foreach (DataColumn col in dRow.Table.Columns)
            {
                if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码" && col.Caption != "所属中队代码" && col.Caption != "所属警务室代码" && col.Caption != "抽取ID" && col.Caption != "抽取更新时间" && col.Caption != "最后更新人")
                {
                    if (col.Caption == "当前居住人数" || col.Caption == "历史居住人数" || col.Caption == "暂住证有效期内人数" || col.Caption == "未办暂住证人数")
                        this.dataGridView1.Rows.Add(col.Caption + ":", "");
                    else
                        this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                }
            }
            this.setSize();

            #region 加上链接相互关联
            //　姓名加上链接查看照片
            if (dRow.Table.Columns[0].Caption.ToString() == "姓名")
            {
                DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                dgvlc.Value = dRow[dRow.Table.Columns[0]];
                dgvlc.ToolTipText = "查看照片";
                this.dataGridView1.Rows[0].Cells[1] = dgvlc;
            }

            int l = 0, d = 0, w = 0, y = 0;
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "当前居住人数:")
                {
                    d = i;
                    dqPopu = new string[] { };
                    dqPopu = row["当前居住人数"].ToString().Split(',');
                    continue;
                }
                if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "历史居住人数:")
                {
                    l = i;
                    lsPopu = new string[] { };
                    lsPopu = row["历史居住人数"].ToString().Split(',');
                    continue;
                }
                if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "暂住证有效期内人数:")
                {
                    y = i;
                    yzPopu = new string[] { };
                    yzPopu = row["暂住证有效期内人数"].ToString().Split(',');
                    continue;
                }
                if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "未办暂住证人数:")
                {
                    w = i;
                    wzPopu = new string[] { };
                    wzPopu = row["未办暂住证人数"].ToString().Split(',');
                    continue;
                }

            }
            DataGridViewLinkCell Datagvlc = new DataGridViewLinkCell();
            if (dqPopu[0] == "" || dqPopu[0] == "0")
                Datagvlc.Value = "0";
            else
                Datagvlc.Value = dqPopu.Length.ToString();
            Datagvlc.ToolTipText = "查看当前居住人详细信息";
            dataGridView1.Rows[d].Cells[1] = Datagvlc;

            DataGridViewLinkCell Datagvlc1 = new DataGridViewLinkCell();
            if (lsPopu[0] == "" || lsPopu[0] == "0")
                Datagvlc1.Value = "0";
            else
                Datagvlc1.Value = lsPopu.Length.ToString();
            Datagvlc1.ToolTipText = "查看暂住证有效期内人详细信息";
            dataGridView1.Rows[l].Cells[1] = Datagvlc1;

            DataGridViewLinkCell Datagvlc2 = new DataGridViewLinkCell();
            if (yzPopu[0] == "" || yzPopu[0] == "0")
                Datagvlc2.Value = "0";
            else
                Datagvlc2.Value = yzPopu.Length.ToString();
            Datagvlc2.ToolTipText = "查看当前居住人详细信息";
            dataGridView1.Rows[y].Cells[1] = Datagvlc2;

            DataGridViewLinkCell Datagvlc3 = new DataGridViewLinkCell();
            if (wzPopu[0] == "" || wzPopu[0] == "0")
                Datagvlc3.Value = "0";
            else
                Datagvlc3.Value = wzPopu.Length.ToString();
            Datagvlc3.ToolTipText = "查看未办暂住证人详细信息";
            dataGridView1.Rows[w].Cells[1] = Datagvlc3;
            #endregion

            this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
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


        // 定位按钮单击事件
        private Feature flashFt = null;
        private Style defaultStyle;
        private void btnPoint_Click(object sender, EventArgs e)
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
                string tName = row.Table.Columns[0].Caption.ToString();
                string[] winStr = null;
                if (tName == "房屋编号")
                {
                    GetFromName getFromName = new GetFromName("出租屋房屋系统");
                    winStr = new string[] { row["X"].ToString(), row["Y"].ToString(), "出租屋房屋系统", row[getFromName.ObjID].ToString(), row[getFromName.ObjName].ToString() };
                }
                if (tName == "姓名")
                {
                    GetFromName getFromName = new GetFromName("人口系统");
                    winStr = new string[] { row["X"].ToString(), row["Y"].ToString(), "人口系统", row[getFromName.ObjID].ToString(), row[getFromName.ObjName].ToString() };
                }
                if (tName == "案件名称")
                {
                    GetFromName getFromName = new GetFromName("案件信息");
                    winStr = new string[] { row["X"].ToString(), row["Y"].ToString(), "案件信息", row[getFromName.ObjID].ToString(), row[getFromName.ObjName].ToString() };
                }
                DinPoint(winStr);
                this.timer1.Start();
                k = 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // 在图层中画点并定位
        private void DinPoint(string[] winStr)
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl.Map.Layers["综合查询"];

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
                   
                    GetFromName getFromName = new GetFromName(winStr[2]);
                    string bmpName = getFromName.BmpName;
                    if (bmpName == "anjian")
                    {
                        MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                        cs = new CompositeStyle(pStyle);
                    }
                    else
                    {
                        MapInfo.Styles.BitmapPointStyle bitmappointstyle = new MapInfo.Styles.BitmapPointStyle(bmpName);
                        cs = new CompositeStyle(bitmappointstyle);
                    }

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

        private Color col = Color.Blue;
        int k = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (col == Color.Red)
                {
                    col = Color.Blue;
                }
                else
                {
                    col = Color.Red;
                }
                BasePointStyle pStyle = new SimpleVectorPointStyle(35, col, 26);
                flashFt.Style = pStyle;
                flashFt.Update();
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
            OracleDataAdapter oracleDat = null;
            DataSet objset = new DataSet();

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
                    if (myConnection.State == ConnectionState.Open)
                    {
                        oracleCmd.Dispose();
                        myConnection.Close();
                    }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "当前居住人数:")
            {
                if (dqPopu[0] == "" || dqPopu[0] == "0")
                    return;
                string dqPopus = row["当前居住人数"].ToString();
                dqPopus = dqPopus.Replace(",", "','");

                try
                {
                    string strSQL = "select 姓名,身份证号码 from 人口系统 where 身份证号码 in ('" + dqPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        oracleDat.Dispose();
                        MessageBox.Show("居住人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (dqPopu.Length > 1)
                        disPlay(objset.Tables[0]);

                    oracleDat.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try
                    {
                        if (dqPopu.Length == 1 && dqPopu[0] != "" && dqPopu[0] != "0")
                            poupDisplayInfo(dqPopus);
                    }
                    catch { }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "历史居住人数:")
            {
                if (lsPopu[0] == "" || lsPopu[0] == "0")
                    return;
                string lsPopus = row["历史居住人数"].ToString();
                lsPopus = lsPopus.Replace(",", "','");

                try
                {
                    string strSQL = "select 姓名,身份证号码 from 人口系统 where 身份证号码 in ('" + lsPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        oracleDat.Dispose();
                        MessageBox.Show("居住人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (lsPopu.Length > 1)
                        disPlay(objset.Tables[0]);

                    oracleDat.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try
                    {
                        if (lsPopu.Length == 1 && lsPopu[0] != "" && lsPopu[0] != "0")
                            poupDisplayInfo(lsPopus);
                    }
                    catch { }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "未办暂住证人数:")
            {
                if (wzPopu[0] == "" || wzPopu[0] == "0")
                    return;
                string wzPopus = row["未办暂住证人数"].ToString();
                wzPopus = wzPopus.Replace(",", "','");

                try
                {
                    string strSQL = "select 姓名,身份证号码 from 人口系统 where 身份证号码 in ('" + wzPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        oracleDat.Dispose();
                        MessageBox.Show("未办暂住证人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (wzPopu.Length > 1)
                        disPlay(objset.Tables[0]);

                    oracleDat.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try
                    {
                        if (wzPopu.Length == 1 && wzPopu[0] != "" && wzPopu[0] != "0")
                            poupDisplayInfo(wzPopus);
                    }
                    catch { }
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "暂住证有效期内人数:")
            {
                if (yzPopu[0] == "" || yzPopu[0] == "0")
                    return;
                string yzPopus = row["暂住证有效期内人数"].ToString();
                yzPopus = yzPopus.Replace(",", "','");

                try
                {
                    string strSQL = "select 姓名,身份证号码 from 人口系统 where 身份证号码 in ('" + yzPopus + "')";
                    myConnection.Open();
                    oracleDat = new OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");

                    if (objset.Tables[0].Rows.Count == 0)
                    {
                        oracleDat.Dispose();
                        MessageBox.Show("暂住证有效期内人未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (yzPopu.Length > 1)
                        disPlay(objset.Tables[0]);

                    oracleDat.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (myConnection.State == ConnectionState.Open)
                    {
                        myConnection.Close();
                    }
                    try
                    {
                        if (yzPopu.Length == 1 && yzPopu[0] != "" && yzPopu[0] != "0")
                            poupDisplayInfo(yzPopus);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 只有一条记录时调用的函数
        /// </summary>
        /// <param name="popuNo">身份证号码</param>
        private void poupDisplayInfo(string popuNo)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter oracleDat = null;
            DataSet objset = new DataSet();
            try
            {
                myConnection.Open();
                string sqlFields = "姓名,身份证号码,民族,性别,人口性质,出生日期,全住址,住址街路巷,住址门牌,楼层,房间号,户籍地址,婚姻状态,配偶姓名,结婚时间,交通工具,车牌号码,服务处所,政治面貌, 暂住证号,暂住证有效期限,入住日期,注销日期,联系电话,重点人口,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                string strSQL = "select " + sqlFields + " from 人口系统 t where 身份证号码='" + popuNo + "'";
                oracleDat = new OracleDataAdapter(strSQL, myConnection);
                oracleDat.Fill(objset, "table");

                // 判断是否有记录
                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("身份证号码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Console.WriteLine(ex.Message);
            }
            finally
            {
                oracleDat.Dispose();
                myConnection.Close();
            }
        }


        private frmLsPqNumber frmPopu = new frmLsPqNumber();
        private void disPlay(DataTable dt)
        {
            try
            {
                if (this.frmPopu.Visible == false)
                {
                    this.frmPopu = new frmLsPqNumber();
                    frmPopu.mapControl = mapControl;
                    frmPopu.Show();
                    frmPopu.Visible = true;
                }
                
                frmPopu.strConn = strConn;
                frmPopu.setfrmInfo(dt);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private frmLsPqInfo frmMessage = new frmLsPqInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                frmMessage.mapControl = mapControl;
                if (this.frmMessage.Visible == false)
                {
                    this.frmMessage = new frmLsPqInfo();
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