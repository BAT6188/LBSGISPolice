using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using clPopu;

namespace nsInfo
{
    public partial class FrmInfo : Form
    {
        public FrmInfo()
        {
            InitializeComponent();
        }
        public string strConn;
        public MapInfo.Windows.Controls.MapControl mapControl;
        public string getFromNamePath;
        private string LayerName;

        private DataRow row = null;
        //传递dt获取信息
        internal void setInfo(DataRow dRow,Point pt,string LayName)
        {
            try
            {
                this.dataGridView2.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView2.Rows.Clear();

                row = dRow;
                LayerName = LayName;
                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0 && col.Caption != "所属派出所代码" && col.Caption != "所属中队代码" && col.Caption != "所属警务室代码" && col.Caption != "抽取ID" && col.Caption != "抽取更新人" && col.Caption != "抽取更新时间")
                    {
                        if (col.Caption == "涉案人员" || col.Caption == "相关案件" || (col.Caption == "房屋编号" && col.Table.Columns[0].Caption != "房屋编号") || col.Caption == "当前居住人数" || col.Caption == "历史居住人数" || col.Caption == "暂住证有效期内人数" || col.Caption == "未办暂住证人数")
                            this.dataGridView2.Rows.Add(col.Caption + ":", "");
                        else
                            this.dataGridView2.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                }
                this.setSize();
                int k = 0;
                for (int i = 0; i < dRow.Table.Columns.Count; i++)
                {
                    if (dRow.Table.Columns[i].Caption == "涉案人员")
                        k = i;
                }

                // 设置涉案人员链接
                DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                dgvlc.Value = "查看相关涉案人员信息";
                dgvlc.ToolTipText = "查看相关涉案人员信息";
                dataGridView2.Rows[k].Cells[1] = dgvlc;


                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
                this.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }
        }

        private void setSize() {
            try
            {
                for (int i = 0; i < this.dataGridView2.Columns.Count; i++)
                {
                    if (this.dataGridView2.Columns[i].Width > 300)
                    {
                        this.dataGridView2.Columns[1].Width = 300;
                        this.dataGridView2.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                {
                    double width = this.dataGridView2.Columns[1].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView2.Rows[i].Cells[1].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView2.Rows[i].Height = (this.dataGridView2.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView2.Columns[0].Width + this.dataGridView2.Columns[1].Width + 30;

                if (this.dataGridView2.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView2.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView2.Rows[i].Height;
                }
                cMessageHeight += 50;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

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

        // 单击网格事件
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //点击表头,退出
            string popuNo = "";
            string[] peplNo = null;
            try
            {
                OracleConnection oracleConn = new OracleConnection(strConn);
                OracleDataAdapter oracleOda = null;
                DataSet objset = null;
                if (e.ColumnIndex == 1 && dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString() == "涉案人员:")
                {
                    popuNo = row["涉案人员"].ToString();
                    peplNo = popuNo.Split(',');

                    if (popuNo != "")
                    {
                        try
                        {
                            popuNo = popuNo.Replace(",", "','");
                            string sqlStr = "select 身份证号码,姓名 from 人口系统 t where 身份证号码 in ('" + popuNo + "')";
                            oracleConn.Open();
                            oracleOda = new OracleDataAdapter(sqlStr, oracleConn);
                            objset = new DataSet();
                            oracleOda.Fill(objset);
                            if (objset.Tables[0].Rows.Count < 1)
                            {
                                MessageBox.Show("涉案人员未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }
                            if (peplNo.Length > 1)
                                disPlayNumberInfo(objset.Tables[0],"人口系统");
                        }
                        catch (Exception ex)
                        {
                            ExToLog(ex, "dataGridView2_CellContentClick");
                        }
                        finally
                        {
                            oracleOda.Dispose();
                            oracleConn.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("涉案人员未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView2_CellContentClick");
            }
            finally
            {
                try
                {
                    if (peplNo.Length == 1 && peplNo[0] != "" && peplNo[0] != null)
                        OnePopuInfo();
                }
                catch { }
            }
        }

        private void OnePopuInfo()
        {
            OracleConnection oracleConn = new OracleConnection(strConn);
            OracleDataAdapter oracleOda = null;
            DataSet objset = null;
            try
            {
                string popuNo = row["涉案人员"].ToString();

                CLC.ForSDGA.GetFromTable.GetFromName("人口系统", getFromNamePath);
                string sqlStr = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from 人口系统 t where 身份证号码='" + popuNo + "'";
                oracleConn.Open();
                oracleOda = new OracleDataAdapter(sqlStr, oracleConn);
                objset = new DataSet();
                oracleOda.Fill(objset);
                if (objset.Tables[0].Rows.Count > 1)
                {
                    MessageBox.Show("身份证号码有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                } 
                System.Drawing.Point pt = new System.Drawing.Point();
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }

                disPlayInfo(objset.Tables[0], pt, "人口系统");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "OnePopuInfo");
            }
            finally
            {
                oracleOda.Dispose();
                oracleConn.Close();
            }
        }

        private FrmHouseNumber frmNumeber = new FrmHouseNumber();
        private void disPlayNumberInfo(DataTable dt, string tabName)
        {
            try
            {
                if (this.frmNumeber.Visible == false)
                {
                    this.frmNumeber = new FrmHouseNumber();
                    frmNumeber.Show();
                    frmNumeber.Visible = true;
                }
                frmNumeber.mapControl = mapControl;
                frmNumeber.LayerName = LayerName;
                frmNumeber.getFromNamePath = getFromNamePath;
                frmNumeber.strConn = strConn;
                frmNumeber.setfrmInfo(dt, tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayNumberInfo");
            }
        }

        private FrmHouseInfo frmhouse = new FrmHouseInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string tableName)
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
                frmhouse.setInfo(dt.Rows[0], pt, tableName);
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
            CLC.BugRelated.ExceptionWrite(ex, "clAnjian-frmInfo-" + sFunc);
        }
    }
}

