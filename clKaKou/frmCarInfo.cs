using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MapInfo.Windows.Controls;


namespace clKaKou
{
    public partial class frmCarInfo : Form
    {
        public frmCarInfo()
        {
            InitializeComponent();
        }

        public string[] StrCon;         // 数据库连接字符串数组
        public string UserName;         // 登陆用户姓名
        public MapControl mapControl;   // 地图控件
        public string getFromNamePath;  // GetFromNameConfig.ini的地址
        public string layerName;        // 图层名称
        public string mysql;            // 连接字符串 

        /// <summary>
        /// 初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dRow">数据行</param>
        /// <param name="Constr">连接参数</param>
        /// <param name="un">用户名</param>
        internal void setInfo(DataRow dRow, string[] Constr, string un)
        {
            try
            {
                StrCon = Constr;

                UserName = un;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0)
                    {
                        if (col.Caption.IndexOf("照片") < 0 && col.Caption != "身份证明号码")
                        {
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                        }
                        else if (col.Caption == "身份证明号码" && dRow[col].ToString().Length > 0)
                        {
                             //|| (col.Caption == "照片1" && dRow[col].ToString().Length > 0) || (col.Caption == "照片2" && dRow[col].ToString().Length > 0) || (col.Caption == "照片3" && dRow[col].ToString().Length > 0))
                            //{
                                DataGridViewLinkCell dglc1 = new DataGridViewLinkCell();

                                //dglc1.Value = "查看 " + col.Caption + " 的信息";

                                if (col.Caption == "身份证明号码")
                                    dglc1.Value = dRow[col].ToString();

                                dglc1.ToolTipText = "查看人口详细信息";

                                this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);

                                this.dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[1] = dglc1;
                            //}
                        }
                    }
                }
                //this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置

                int ki = 0;
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].ToString().IndexOf("处理状态") > 0)
                    {
                        ki = i;
                    }
                }

                this.TopMost = true;

                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }

                this.setSize();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-01-setInfo");
            }
        }

        /// <summary>
        /// 设置窗体大小
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
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
                writeToLog(ex, "clKaKou-frmCarInfo-02-setSize");
            }
        }

        /// <summary>
        /// 设置窗体位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="iWidth">窗体宽度</param>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        private void setLocation(int iWidth, int iHeight, int x, int y)
        {
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
                writeToLog(ex, "clKaKou-frmCarInfo-setLocation");
            }
        }

        /// <summary>
        /// 列表单击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "身份证明号码:")
            {

                try
                {
                    MapInfo.Geometry.DPoint dp = new MapInfo.Geometry.DPoint();
                   
                    CLC.ForSDGA.GetFromTable.GetFromName("人口系统", Application.StartupPath + "\\GetFromNameConfig.ini");
                    string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                    string sFno = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();

                    if (sFno.Length == 15)
                    {
                        sFno = oldToNew(sFno);
                    }

                    string strSQL = "select " + sqlFields + " from 人口系统 t where 身份证号码='" + sFno + "'";

                    DataTable datatable = this.GetTable(strSQL);

                    if (datatable.Rows.Count > 0)
                    {
                        System.Drawing.Point pt = new System.Drawing.Point();
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width / 2;
                        pt.Y = 10;

                        this.disPlayInfo(datatable, pt);
                    }
                    else
                    {
                        MessageBox.Show("没有查询到相对应的人口信息","系统提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    }
                    //WriteEditLog("身份证号码='" + dataGridView1.Rows[e.RowIndex].Cells["身份证号码"].Value.ToString() + "'", "查看详情");
                }
                catch (Exception ex)
                {
                    writeToLog(ex, " ucKakou-dataGridView1_CellContentClick-显示车辆详细信息");
                }
            }
        }

        /// <summary>
        /// 将身份证号码由15位转为18位
        /// </summary>
        /// <param name="id">15位身份证号码</param>
        /// <returns>18位身份证号码</returns>
        private string oldToNew(string id)
        {
            try
            {
                if (id == null || id == "")
                {

                    return "";

                }
                else
                {

                    if (id.Length == 18) { return id; }
                    else
                    {

                        int[] W ={ 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2, 1 };

                        string[] A ={ "1", "0", "X", "9", "8", "7", "6", "5", "4", "3", "2" };

                        int i, j, s = 0;

                        string newid;

                        newid = id;

                        newid = newid.Substring(0, 6) + "19" + newid.Substring(6, id.Length-6);

                        for (i = 0; i < newid.Length; i++)
                        {

                            j = Int32.Parse(newid.Substring(i,1)) * W[i];
                            //Integer.parseInt(

                            s = s + j;

                        }

                        s = s % 11;

                        newid = newid + A[s];

                        return newid;

                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, " ucKakou-frmCarInfo-oldToNew");
                return "";
            }

        }

        private clPopu.FrmHouseInfo frminfo = new clPopu.FrmHouseInfo();
        /// <summary>
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="pt">坐标</param>
        private void disPlayInfo(DataTable dt,Point pt)
        {
            try
            {
                if (this.frminfo.Visible == false)
                {
                    this.frminfo = new clPopu.FrmHouseInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.Visible = false;
                }

                frminfo.mapControl = mapControl;
                frminfo.LayerName = this.layerName;
                frminfo.getFromNamePath = getFromNamePath;
                frminfo.strConn = this.mysql;
                frminfo.setInfo(dt.Rows[0],pt,this.UserName);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " ucKakou-21-显示车辆详细信息");
            }
        }


        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>结果集</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        /// <summary>
        /// 记录操作记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="method">操作方式</param>
        /// <param name="tablename">表名</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + UserName + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'治安卡口','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-06-记录操作记录");
            }
        }

    }
}