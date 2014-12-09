using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.IO;
using clInfo;
using System.Text.RegularExpressions;


namespace clPopu
{
    public partial class FrmInfo : Form
    {
        public string strConn;
        public MapInfo.Windows.Controls.MapControl mapControl = null;
        private DataRow row = null;
        public string getFromNamePath;
        private string LayerName;
        private string[] StrCon;

        public FrmInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 传递dt获取信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="dRow">记录行</param>
        /// <param name="pt">窗体显示位置</param>
        /// <param name="LayName">图层名称</param>
        public void setInfo(DataRow dRow,Point pt,string LayName)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                row = dRow;
                LayerName = LayName;
                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码" 
                                                            && col.Caption != "所属中队代码" 
                                                            && col.Caption != "所属警务室代码"
                                                            && col.Caption != "抽取ID" 
                                                            && col.Caption != "抽取更新时间" 
                                                            && col.Caption != "最后更新人" 
                                                            && col.Caption != "配偶公民身份号码" 
                                                            && col.Caption != "住址门牌")
                    {
                        if (col.Caption == "涉案人员" || col.Caption == "相关案件" 
                                                      || (col.Caption == "房屋编号" && col.Table.Columns[0].Caption != "房屋编号") 
                                                      || col.Caption == "当前居住人数" 
                                                      || col.Caption == "历史居住人数" 
                                                      || col.Caption == "暂住证有效期内人数" 
                                                      || col.Caption == "未办暂住证人数")
                            this.dataGridView1.Rows.Add(col.Caption + ":", "");
                        else
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置

                #region 添加链接，相互关联
                string[] hoNum = null, anNum = null;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    #region 较早前代码
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "姓名:")
                    //{
                    //    //k = i;
                    //    //在姓名上加链接,可查看照片
                    //    DataGridViewLinkCell dglc = new DataGridViewLinkCell();
                    //    dglc.Value = dRow[dRow.Table.Columns[i]];
                    //    dglc.ToolTipText = "查看照片";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc;
                    //    continue;
                    //}
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "房屋编号:")
                    //{
                    //    hoNum = new string[] { };
                    //    hoNum = row["房屋编号"].ToString().Split(',');
                    //    //查看房屋链接
                    //    DataGridViewLinkCell dglc1 = new DataGridViewLinkCell();
                    //    dglc1.Value = "查看房屋详细信息";
                    //    dglc1.ToolTipText = "查看房屋详细信息";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc1;
                    //    continue;
                    //}
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "相关案件:")
                    //{
                    //    anNum = new string[] { };
                    //    anNum = row["相关案件"].ToString().Split(',');
                    //    DataGridViewLinkCell dglc2 = new DataGridViewLinkCell();
                    //    dglc2.Value = "查看相关案件信息";
                    //    dglc2.ToolTipText = "查看相关案件信息";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc2;
                    //    continue;
                    //}
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "配偶姓名:")
                    //{
                    //    //在配偶姓名上加链接,可查看配偶信息
                    //    DataGridViewLinkCell dglc = new DataGridViewLinkCell();
                    //    dglc.Value = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                    //    dglc.ToolTipText = "查看配偶信息";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc;
                    //    continue;
                    //}
                    #endregion

                    string valueRow=this.dataGridView1.Rows[i].Cells[0].Value.ToString();
                    switch (valueRow)
                    {
                        case "姓名:":
                            setDataGridLinkCell(i, dRow[dRow.Table.Columns[i]].ToString(), "查看照片");
                            break;
                        case "房屋编号:":
                            hoNum = new string[] { };
                            hoNum = row["房屋编号"].ToString().Split(',');
                            setDataGridLinkCell(i, "查看房屋详细信息", "查看房屋详细信息");
                            break;
                        case "相关案件:":
                            anNum = new string[] { };
                            anNum = row["相关案件"].ToString().Split(',');
                            setDataGridLinkCell(i, "查看相关案件信息", "查看相关案件信息");
                            break;
                        case "配偶姓名:":
                            setDataGridLinkCell(i, this.dataGridView1.Rows[i].Cells[1].Value.ToString(), "查看配偶信息");
                            break;
                    }
                }
                #endregion

                this.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }
        }

        /// <summary>
        /// 添加连接的方法
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="s">要添加的记录行</param>
        /// <param name="valueName">链接显示值</param>
        /// <param name="toolText">链接显示ToolText</param>
        private void setDataGridLinkCell(int s, string valueName, string toolText)
        {
            try {
                DataGridViewLinkCell dglc = new DataGridViewLinkCell();
                dglc.Value = valueName;
                dglc.ToolTipText = toolText;
                this.dataGridView1.Rows[s].Cells[1] = dglc;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setDataGridLinkCell");
            }
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
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

        /// <summary>
        /// 设置窗体位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="iWidth">窗体宽度</param>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        private void setLocation(int iWidth,int iHeight,int x,int y) {
            try
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
            catch (Exception ex)
            {
                ExToLog(ex, "setLocation");
            }
        }

        private FrmHouseNumber frmhn = new FrmHouseNumber();

        /// <summary>
        /// 点击列表链接实现关连
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            try
            {
                System.Data.OracleClient.OracleCommand oracleCmd = null;
                System.Data.OracleClient.OracleDataReader dataReader = null;
                System.Data.OracleClient.OracleDataAdapter objoda = null;
                System.Data.DataSet objset = new System.Data.DataSet();
                string[] strHouse = null, Anjan = null;

                if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "姓名:")
                {
                    int k = 0, r = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "身份证号码:")
                        {
                            k = i;
                        }
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "人口性质:")
                        {
                            r = i;
                        }
                    } 
                    System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=czrk_cx;" +
                                                                                                                      "Password=czrk_cx;" + 
                                                                                                                      "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)" + 
                                                                                                                                               "(HOST=10.47.227.15)(PORT=1521)))" +
                                                                                                                                               "(CONNECT_DATA=(SID=ora81)))");
                    try
                    {
                        FrmImage fimage = new FrmImage();
                        string sqlstr = ""; 
                        if(dataGridView1.Rows[r].Cells[1].Value.ToString()=="常住人口")
                            sqlstr = "select TX from czrk_cx.v_gis_czrk_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";
                        else
                            sqlstr = "select TX from czrk_cx.v_gis_ldry_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";

                        oraconn.Open();
                        oracleCmd = new OracleCommand(sqlstr, oraconn);
                        dataReader = oracleCmd.ExecuteReader();

                        if (dataReader.Read())
                        {
                            byte[] bytes = new byte[(dataReader.GetBytes(0, 0, null, 0, int.MaxValue))];
                            if (dataReader.IsDBNull(0))
                            {
                                System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, bytes.Length);
                                dataReader.Close();
                                Stream fs = new MemoryStream(bytes);

                                //创建一个bitmap类型的bmp变量来读取文件。
                                Bitmap bmp = new Bitmap(Image.FromStream(fs));
                                bmp.Save(Application.StartupPath + "\\popu.jpg");
                                fimage.pictureBox1.ImageLocation = Application.StartupPath + "\\popu.jpg";
                                bmp.Dispose();//释放bmp文件资源
                                fimage.pictureBox1.Invalidate();
                                fs.Close();
                                fimage.TopMost = true;
                                fimage.Visible = false;
                                fimage.ShowDialog();
                                fimage.Dispose();
                                File.Delete(Application.StartupPath + "\\popu.jpg");
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExToLog(ex, "dataGridView1_CellContentClick-显示照片");
                    }
                    finally
                    {
                        if (oraconn.State == ConnectionState.Open)
                        {
                            oracleCmd.Dispose();
                            oraconn.Close();
                        }
                    }
                }
                else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "房屋编号:")
                {
                    string RenHouse = "查看房屋详细信息";

                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() == RenHouse)
                    {
                        try
                        {
                            if (frmhn.dataGridView1.Rows.Count != 0)
                            {
                                frmhn.dataGridView1.Rows.Clear();
                            }
                            myConnection.Open();
                            strHouse = row["房屋编号"].ToString().Split(',');
                            string strHouseNumber = row["房屋编号"].ToString();
                            strHouseNumber = strHouseNumber.Replace(",", "','");
                            string sql = "select 房屋编号,屋主姓名 from 出租屋房屋系统 where 房屋编号 in('" + strHouseNumber + "')";
                            objoda = new OracleDataAdapter(sql, myConnection);
                            objoda.Fill(objset, "table");
                            if (objset.Tables[0].Rows.Count == 0)
                            {
                                MessageBox.Show("房屋未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }

                            if (strHouse.Length > 1)
                                disPlayNumberInfo(objset.Tables[0], "出租屋房屋");

                        }
                        catch (Exception ex)
                        {
                            ExToLog(ex, "dataGridView1_CellContentClick--显示多条记录");
                        }
                        finally
                        {
                            objoda.Dispose();
                            myConnection.Close();
                            try
                            {
                                // 判断是否只有一间房屋
                                if (strHouse.Length == 1 && strHouse[0] != "")
                                    onlyFirst(strHouse[0], "出租屋房屋");
                            }
                            catch (Exception ex) { ExToLog(ex, "dataGridView1_CellContentClick--显示多条记录"); }
                        }
                    }
                }
                else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "相关案件:")
                {
                    string RenHouse = "查看相关案件信息";

                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() == RenHouse)
                    {
                        try
                        {
                            if (frmhn.dataGridView1.Rows.Count != 0)
                            {
                                frmhn.dataGridView1.Rows.Clear();
                            }
                            myConnection.Open();
                            Anjan = row["相关案件"].ToString().Split(',');
                            string AnjanNumber = row["相关案件"].ToString();
                            AnjanNumber = AnjanNumber.Replace(",", "','");
                            string sql = "select 案件编号,案件名称 from 案件信息 where 案件编号 in('" + AnjanNumber + "')";
                            objoda = new OracleDataAdapter(sql, myConnection);
                            objoda.Fill(objset, "table");
                            if (objset.Tables[0].Rows.Count == 0)
                            {
                                MessageBox.Show("案件未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }

                            if (Anjan.Length > 1)
                                disPlayNumberInfo(objset.Tables[0], "案件信息");

                        }
                        catch (Exception ex)
                        {
                            ExToLog(ex, "dataGridView1_CellContentClick--显示多条记录");
                        }
                        finally
                        {
                            objoda.Dispose();
                            myConnection.Close();
                            try
                            {
                                // 判断是否只有一间房屋
                                if (Anjan.Length == 1 && Anjan[0] != "")
                                    onlyFirst(Anjan[0], "案件信息");
                            }
                            catch (Exception ex) { ExToLog(ex, "dataGridView1_CellContentClick--显示一条记录"); }
                        }
                    }
                }
                else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "配偶姓名:")
                {
                    onlyFirst(row["配偶公民身份号码"].ToString(), "人口系统");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick");
            }
        }


        /// <summary>
        /// 当只有一间房屋时直接显示房屋信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="strHouse">房屋编号</param>
        /// <param name="tabName">表名</param>
        private void onlyFirst(string strHouse,string tabName)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter objoda = null;
            System.Data.DataSet objset = new System.Data.DataSet();
            try
            {
                myConnection.Open();
                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + strHouse + "'";
                objoda = new OracleDataAdapter(strSQL, myConnection);
                objoda.Fill(objset, "table");

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

                disPlayInfo(objset.Tables[0], pt,tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "onlyFirst");
            }
            finally
            {
                objoda.Dispose();
                myConnection.Close();
            }
        }

        //private FrmHouseNumber frmNumeber = new FrmHouseNumber();
        //private void disPlayNumberInfo(DataTable dt, string tabName)
        //{
        //    try
        //    {
        //        if (this.frmNumeber.Visible == false)
        //        {
        //            this.frmNumeber = new FrmHouseNumber();
        //            frmNumeber.Show();
        //            frmNumeber.Visible = true;
        //        }
        //        frmNumeber.mapControl = mapControl;
        //        frmNumeber.LayerName = LayerName;
        //        frmNumeber.getFromNamePath = getFromNamePath;
        //        frmNumeber.strConn = strConn;
        //        frmNumeber.setfrmInfo(dt, tabName);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExToLog(ex, "disPlayNumberInfo");
        //    }
        //}

        /// <summary>
        /// 显示编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="tabName">表名</param>
        private void disPlayNumberInfo(DataTable dt, string tabName)
        {
            try
            {
                frmNumber fmNum = new frmNumber();
                if (fmNum.Visible == false)
                {
                    fmNum = new frmNumber();
                    fmNum.Show();
                    fmNum.Visible = true;
                }
                fmNum.mapControl = mapControl;
                fmNum.LayerName = LayerName;
                fmNum.getFromNamePath = getFromNamePath;
                fmNum.StrCon = StrCon;
                fmNum.setInfo(dt, tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayNumberInfo");
            }
        }

        //private FrmHouseInfo frmhouse = new FrmHouseInfo();
        //private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string tabName)
        //{
        //    try
        //    {
        //        if (this.frmhouse.Visible == false)
        //        {
        //            this.frmhouse = new FrmHouseInfo();
        //            frmhouse.SetDesktopLocation(-30, -30);
        //            frmhouse.Show();
        //            frmhouse.Visible = false;
        //        }
        //        frmhouse.mapControl = mapControl;
        //        frmhouse.LayerName = LayerName;
        //        frmhouse.getFromNamePath = getFromNamePath;
        //        frmhouse.strConn = strConn;
        //        frmhouse.setInfo(dt.Rows[0], pt, tabName);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExToLog(ex, "disPlayInfo");
        //    }
        //}

        /// <summary>
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">显示位置</param>
        /// <param name="tabName">表名</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt, string tabName)
        {
            try
            {
                frmInfo fmIn = new frmInfo();
                if (fmIn.Visible == false)
                {
                    fmIn = new frmInfo();
                    fmIn.SetDesktopLocation(-30, -30);
                    fmIn.Show();
                    fmIn.Visible = false;
                }
                fmIn.mapControl = mapControl;
                fmIn.LayerName = LayerName;
                fmIn.getFromNamePath = getFromNamePath;
                fmIn.StrCon = StrCon;
                fmIn.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clPopu-FrmInfo-" + sFuns);
        }
    }
}

