using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Xml;
using System.IO;

using MapInfo.Styles;
using MapInfo.Mapping;
using MapInfo.Engine;
using MapInfo.Geometry;
using MapInfo.Data;
using MapInfo.Windows.Controls;

namespace clInfo
{
    public partial class frmInfo : Form
    {
        public MapControl mapControl;      // 用于操作的地图
        public string getFromNamePath;     // 配置文件地址（GetFromNameConfig.ini）
        public string LayerName;           // 临时图层名称
        public string[] StrCon;            // 数据库连接参数
        public string tableName;           // 存储要操作的表名

        private Feature flashFt;           // 当前对象地图点
        private DataRow row;               // 当前显示记录

        private string[] dqPopu = null, lsPopu = null, wzPopu = null, yzPopu = null;   // 用于存放关链字段值

        public frmInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dRow">数据行</param>
        /// <param name="pt">坐标</param>
        public void setInfo(DataRow dRow, System.Drawing.Point pt)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                row = dRow;
                string houseNo = "";   // 获得房屋编号

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    switch (col.Caption)
                    {
                        case "所属派出所代码":
                        case "所属中队代码":
                        case "所属警务室代码":
                        case "抽取ID":
                        case "抽取更新时间":
                        case "最后更新人":
                        case "配偶公民身份号码":
                        case "屋主身份证号码":
                        case "住址门牌":
                            break;
                        default:
                            if (col.Caption.IndexOf("备用字段") < 0)
                            {
                                switch (col.Caption)
                                {
                                    case "房屋编号":
                                        if (col.Table.Columns[0].Caption != "房屋编号")
                                            this.dataGridView1.Rows.Add(col.Caption + ":", "");
                                        break;
                                    case "涉案人员":
                                    case "相关案件":
                                    case "当前居住人数":
                                    case "历史居住人数":
                                    case "暂住证有效期内人数":
                                    case "未办暂住证人数":
                                        this.dataGridView1.Rows.Add(col.Caption + ":", "");
                                        break;
                                    default:
                                        this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                                        break;
                                }
                            }
                            break;
                    }

                    if (col.Table.Columns[0].Caption == "房屋编号")
                        houseNo = dRow.Table.Rows[0]["房屋编号"].ToString();
                }

                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置
                this.Visible = true;

                #region 添加链接 相互关联

                int j = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    switch (this.dataGridView1.Rows[i].Cells[0].Value.ToString())
                    {
                        case "涉案人员:":
                        case "相关案件:":
                        case "房屋编号:":
                            j = i;
                            break;
                        case "当前居住人数:":
                            dqPopu = setDataGridLinkCell(i, "当前居住人数", "查看当前居住人详细信息", houseNo);
                            break;
                        case "历史居住人数:":
                            lsPopu = setDataGridLinkCell(i, "历史居住人数", "查看历史居住人详细信息", houseNo);
                            break;
                        case "暂住证有效期内人数:":
                            yzPopu = setDataGridLinkCell(i, "暂住证有效期内人数", "查看暂住证有效期内人详细信息", houseNo);
                            break;
                        case "未办暂住证人数:":
                            wzPopu = setDataGridLinkCell(i, "未办暂住证人数", "查看未办暂住证人详细信息", houseNo);
                            break;
                        case "配偶姓名:":
                            setDataGridViewLinkCell(i, "配偶姓名", "查看配偶详细信息");
                            break;
                        case "屋主姓名:":
                            setDataGridViewLinkCell(i, "屋主姓名", "查看屋主详细信息");
                            break;
                    } 
                }

                switch (this.dataGridView1.Rows[j].Cells[0].Value.ToString())
                {
                    case "安全防护单位文件:":
                        setDataGridViewLinkCell(j, "查看文件", "查看安全防护单位文件");
                        break;
                    case "涉案人员:":
                        setDataGridViewLinkCell(j, "涉案人员", "查看相关涉案人员信息");
                        break;
                    case "相关案件:":
                        for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                        {
                            if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "房屋编号:")
                            {
                                setDataGridViewLinkCell(i, "房屋编号", "查看房屋详细信息");
                            }
                            setDataGridViewLinkCell(0, dRow.Table.Columns[0].ToString(), "查看照片");
                        }
                        setDataGridViewLinkCell(j, "相关案件", "查看案件详细信息");
                        break;
                }

                #endregion
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }
        }

        /// <summary>
        /// 给关联相关的字段加上链接
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="f">在列表的第几条记录</param>
        /// <param name="cellName">列名</param>
        /// <param name="toolText">鼠标伸上显示的文本</param>
        private void setDataGridViewLinkCell(int f,string cellName,string toolText)
        {
            try
            {
                DataGridViewLinkCell Datagvlc = new DataGridViewLinkCell();
                switch (cellName)
                {
                    case "查看文件":
                    case "涉案人员":
                    case "相关案件":
                    case "房屋编号":
                        Datagvlc.Value = toolText;
                        break;
                    default:
                        Datagvlc.Value = row[cellName].ToString();
                        break;
                }
                Datagvlc.ToolTipText = toolText;
                dataGridView1.Rows[f].Cells[1] = Datagvlc;
            }
            catch (Exception ex) 
            {
                ExToLog(ex, "setDataGridViewLinkCell--1"); 
            }
        }

        /// <summary>
        /// 给关联相关的字段加上链接
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="f">在列表的第几条记录</param>
        /// <param name="cellName">列名</param>
        /// <param name="toolText">鼠标伸上显示的文本</param>
        /// <param name="huNum">房屋编号</param>
        /// <returns>存储人数组数</returns>
        private string[] setDataGridLinkCell(int f, string cellName, string toolText,string huNum)
        {
            try
            {
                string sqlStr;
                string[] witStr = null;
                switch (cellName)
                {
                    case "当前居住人数":
                        sqlStr = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(huNum) + "='" + huNum + "'";
                        witStr = getPopuCount(sqlStr, witStr);
                        break;
                    case "历史居住人数":
                        sqlStr = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(huNum) + "='" + huNum + "'";
                        witStr = getPopuCount(sqlStr, witStr);
                        break;
                    case "暂住证有效期内人数":
                        sqlStr = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(huNum) + "='" + huNum + "'" +
                                  " and 人口性质='暂住人口' and 暂住证号 is not null and 暂住证有效期限>sysdate";
                        witStr = getPopuCount(sqlStr, witStr);
                        break;
                    case "未办暂住证人数":
                        sqlStr = "select distinct 身份证号码 from gis_fwrk s where " + ConversionStr(huNum) + "='" + huNum + "'" +
                                  " and 人口性质='暂住人口' and ((暂住证号 is null) or (暂住证号 is not null and 暂住证有效期限<sysdate))";
                        witStr = getPopuCount(sqlStr, witStr);
                        break;
                }

                DataGridViewLinkCell Datagvlc = new DataGridViewLinkCell();

                if (witStr[0] == "" || witStr[0] == "0")     // 先判断是否有记录 如果没有则为0 如果有则为记录数
                    Datagvlc.Value = "0";
                else
                    Datagvlc.Value = witStr.Length.ToString();

                Datagvlc.ToolTipText = toolText;
                dataGridView1.Rows[f].Cells[1] = Datagvlc;

                return witStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setDataGridLinkCell");
                return null;
            }
        }

        /// <summary>
        /// 根据sql查询人数并逐个保存到数组中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="poStr">sql语句</param>
        /// <param name="conPo">用于存放人数的数组</param>
        /// <returns></returns>
        private string[] getPopuCount(string poStr, string[] conPo)
        {
            try
            {
                DataTable poDat = GetTable(poStr);

                if (poDat.Rows.Count > 0)
                    conPo = new string[poDat.Rows.Count];
                else
                {
                    conPo = new string[1];
                    conPo[0] = "0";
                    return conPo;
                }

                for (int i = 0; i < poDat.Rows.Count; i++)
                {
                    conPo[i] = poDat.Rows[i][0].ToString();
                }

                return conPo;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getPopuCount");
                return null;
            }
        }

        /// <summary>
        /// 用来转换房屋编号代码
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="str">要转换的编号</param>
        /// <returns>转换后字符串</returns>
        private string ConversionStr(string str)
        {
            try
            {
                string converStr = "";

                switch (str.Substring(0, 1))
                {
                    case "a":
                        converStr = "户号";
                        break;
                    case "b":
                        converStr = "出租屋编号";
                        break;
                    case "c":
                        converStr = "住址门牌代码";
                        break;
                    case "d":
                        converStr = "企业编号";
                        break;
                    default:
                        break;
                }
                return converStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ConversionStr");
                return "";
            }
        }

        /// <summary>
        /// 设置窗体的大小
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
                if (this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width < 300)
                {
                    cMessageWidth = 331;
                }
                else
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + 30;
                }

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
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

        /// <summary>
        /// 设置窗体的位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="iWidth">窗体宽度</param>
        /// <param name="iHeight">窗体高度</param>
        /// <param name="x">窗体x坐标</param>
        /// <param name="y">窗体y坐标</param>
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
                ExToLog(ex, "setLocation");
            }
        }

        // 对象闪烁
        int k = 0;               // 用于控制闪烁次数
        Color col = Color.Red;   // 用于闪烁时变化颜色
        /// <summary>
        /// 图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void timerFilcker_Tick(object sender, EventArgs e)
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
                try
                {
                    flashFt.Style = pStyle;
                    flashFt.Update();
                }
                catch { timerFilcker.Stop(); }

                k++;
                if (k == 10)
                {
                    timerFilcker.Stop();
                    flashFt.Style = defaultStyle;
                    flashFt.Update();
                }
            }
            catch
            {
                timerFilcker.Stop();
            }
        }

        /// <summary>
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>结果集</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 查询获取OracleDataReader对象
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>OracleDataReader对象</returns>
        private OracleDataReader GetDataReader(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            OracleDataReader reader = CLC.DatabaseRelated.OracleDriver.OracleComReader(sql);
            return reader;
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clInfo-frmInfo-" + sFuns);
        }

        /// <summary>
        /// 单击单元格内容时触发
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataTable numTable = new DataTable();  // 存储查询结果
                string naTure = "", carID = "";        // 存储人口性质及身份证号码
                string cellName = "";                  // 存储当前点击行的列名
                string[] conStrNum = null;             // 存储相关记录行需关联的值

                cellName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                if (e.ColumnIndex == 1 && cellName == "姓名:")
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "身份证号码:")
                        {
                            carID = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                        }
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "人口性质:")
                        {
                            naTure = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                        }
                    }
                    Image image = naTureNoToImage(naTure, carID);
                    if (image != null)
                    {
                        frmImage fimage = new frmImage();
                        fimage.pictureBox1.Image = image;
                        fimage.pictureBox1.Invalidate();
                        fimage.TopMost = true;
                        fimage.Visible = false;
                        fimage.ShowDialog();
                        fimage.Dispose();
                    }
                    else
                    {
                        return;
                    }
                }

                if (row.Table.Columns[0].Caption == "姓名" && cellName == "房屋编号:")
                    cellName = "房屋编号:";
                if (cellName == "配偶姓名:")
                    cellName = "配偶公民身份号码:";
                if (cellName == "屋主姓名:")
                    cellName = "屋主身份证号码:";

                string sqlStatement = "", DisInformation = "", dataNum = "";         // 生成的sql语句、提示信息、查询数据编号
                 try
                {
                    string colName = cellName.Substring(0, cellName.Length - 1);
                    switch (colName)
                    {
                        case "相关案件":
                            dataNum = row[colName].ToString();
                            conStrNum = dataNum.Split(',');
                            if (dataNum != "")
                            {
                                dataNum = dataNum.Replace(",", "','");
                                sqlStatement = "select 案件编号,案件名称 from 案件信息 t where 案件编号 in ('" + dataNum + "')";
                            }
                            DisInformation = "无涉案记录！";
                            tableName = "案件信息";
                            break;
                        case "房屋编号":
                            dataNum = row[colName].ToString();
                            conStrNum = dataNum.Split(',');
                            if (dataNum != "")
                            {
                                dataNum = dataNum.Replace(",", "','");
                                sqlStatement = "select 房屋编号,屋主姓名 from 出租屋房屋系统 t where 房屋编号 in ('" + dataNum + "')";
                            }
                            DisInformation = "房屋未存档！";
                            tableName = "出租屋房屋系统";
                            break;
                        case "涉案人员":
                            dataNum = row[colName].ToString();
                            conStrNum = dataNum.Split(',');
                            if (dataNum != "")
                            {
                                dataNum = dataNum.Replace(",", "','");
                                sqlStatement = "select 身份证号码,姓名 from 人口系统 t where 身份证号码 in ('" + dataNum + "')";
                            }
                            tableName = "人口系统";
                            string renName = colName.Substring(0, colName.Length - 1);
                            DisInformation = renName + "未存档！";
                            break;
                        case "当前居住人数":
                        case "暂住证有效期内人数":
                        case "历史居住人数":
                        case "未办暂住证人数":
                            dataNum = arrayConStr(colName);
                            conStrNum = dataNum.Split(',');
                            if (dataNum != "")
                            {
                                dataNum = dataNum.Replace(",", "','");
                                sqlStatement = "select 身份证号码,姓名 from 人口系统 t where 身份证号码 in ('" + dataNum + "')";
                            }
                            tableName = "人口系统";
                            string meName = colName.Substring(0, colName.Length - 1);
                            DisInformation = meName + "未存档！";
                            break;
                        case "配偶公民身份号码":
                        case "屋主身份证号码":
                            dataNum = row[colName].ToString();
                            tableName = "人口系统";
                            onlyFirstDisplay(tableName, dataNum);
                            DisInformation = "未查询到此记录，请管理确认数据库中身份证号码是否正确!";
                            break;
                    }

                    if (sqlStatement != "")
                    {
                        numTable = GetTable(sqlStatement);

                        if (numTable.Rows.Count == 0)
                        {
                            MessageBox.Show(DisInformation, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            return;
                        }
                    }
                    else
                    {
                        if (cellName != "" && cellName != "配偶公民身份号码:" 
                                           && cellName != "屋主身份证号码:")
                            MessageBox.Show(DisInformation, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (conStrNum.Length > 1)
                        disNumInfo(numTable, tableName);
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "dataGridView1_CellContentClick-显示多条记录");
                }
                finally
                {
                    try
                    {
                        // 如果只有一条记录则直接显示信息
                        if (conStrNum != null && conStrNum.Length == 1 
                                              && conStrNum[0] != "")
                            onlyFirstDisplay(tableName, conStrNum[0].ToString());
                    }
                    catch (Exception ex)
                    { 
                        ExToLog(ex, "dataGridView1_CellContentClick-显示一条记录");
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick");
            }
        }

        private void clickRelated(string column)
        {
            try
            {
            }
            catch (Exception ex)
            {
                ExToLog(ex, "clickRelated");
            }
        }

        /// <summary>
        /// 根据列名找到字符串数组转换为可供SQL使用的字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>可供SQL使用的字符串</returns>
        private string arrayConStr(string colName)
        {
            try {
                string[] constrArray = null;
                switch (colName)
                {
                    case "当前居住人数":
                        constrArray = dqPopu;
                        break;
                    case "暂住证有效期内人数":
                        constrArray = yzPopu;
                        break;
                    case "历史居住人数":
                        constrArray = lsPopu;
                        break;
                    case "未办暂住证人数":
                        constrArray = wzPopu;
                        break;
                    default:
                        break;
                }

                string arrayStr = "";

                if (constrArray.Length > 0)
                {
                    for (int j = 0; j < constrArray.Length; j++)
                    {
                        if (arrayStr == "")
                            arrayStr = constrArray[j];
                        else
                            arrayStr += "," + constrArray[j];
                    }
                }
                else
                {
                    arrayStr = "";
                }

                return arrayStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "arrayConStr");
                return "";
            }
        }

        /// <summary>
        /// 查询人口图像
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="naTure">人口性质</param>
        /// <param name="carID">身份证号码</param>
        /// <returns>图像</returns>
        private Image naTureNoToImage(string naTure,string carID)
        {
            OracleConnection oraconn = new OracleConnection("User ID=czrk_cx;"
                                                           + "Password=czrk_cx;"
                                                           + "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)"
                                                                       + "(HOST=10.47.227.15)(PORT=1521)))"
                                                                       + "(CONNECT_DATA=(SID=ora81)))");   // 连接公安照片服务器
            OracleCommand oracmd = null;
            OracleDataReader oraReader = null;

            try {
                oraconn.Open();
                string sqlstr = "";

                if (naTure == "常住人口")       // 根据人口性质查找照片 （分为‘常住人口’和‘暂住人口’）
                    sqlstr = "select TX from czrk_cx.v_gis_czrk_tx where ZJHM='" + carID + "'";
                else
                    sqlstr = "select TX from czrk_cx.v_gis_ldry_tx where ZJHM='" + carID + "'";

                oracmd = new OracleCommand(sqlstr, oraconn);
                oraReader = oracmd.ExecuteReader();

                Bitmap returnBmp = null;
                if (oraReader.Read()) {

                    byte[] byteArrayIn = new byte[(oraReader.GetBytes(0, 0, null, 0, int.MaxValue))];

                    if (oraReader.IsDBNull(0))
                    {
                        System.Windows.Forms.MessageBox.Show("照片未存档!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return null;
                    }
                    else
                    {
                        using (MemoryStream ms = new MemoryStream(byteArrayIn))
                        {
                            Bitmap Origninal = new Bitmap(ms);
                            returnBmp = new Bitmap(Origninal.Width, Origninal.Height);
                            Graphics g = Graphics.FromImage(returnBmp);
                            g.DrawImage(Origninal, 0, 0);
                            ms.Close();
                        }
                    }
                }
                else {
                    MessageBox.Show("照片未存档！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return null;
                }

                return (Image)returnBmp;
            }
            catch (Exception ex){
                ExToLog(ex, "naTureNoToImage");
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }
            finally {
                if (oraconn.State == ConnectionState.Open) {
                    oracmd.Dispose();      //
                    oraReader.Close();     // 释放资源
                    oraconn.Close();       //
                }
            }
        }

        /// <summary>
        /// 只有一条信息时调用该函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="tabName">查询记录的表名</param>
        /// <param name="number">查询记录的ID</param>
        private void onlyFirstDisplay(string tabName,string number)
        {
            try
            {
                DataTable tabFirst = new DataTable();

                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);

                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from " + CLC.ForSDGA.GetFromTable.TableName +
                                " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + number + "'";

                tabFirst = GetTable(strSQL);

                if (tabFirst.Rows.Count == 0)
                {
                    MessageBox.Show("未查询到此记录，请数据库管理员确认数据库中编号是否正确！", "系统提示", MessageBoxButtons.OK);
                    return;
                }

                DataRow row = tabFirst.Rows[0];

                System.Drawing.Point pt = new System.Drawing.Point();

                if (tabFirst.Rows[0]["X"] != null && tabFirst.Rows[0]["Y"] != null 
                                                  && tabFirst.Rows[0]["X"].ToString() != ""
                                                  && tabFirst.Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(tabFirst.Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(tabFirst.Rows[0]["Y"]);
                }
                disPlayInfo(tabFirst.Rows[0], pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "onlyFirstDisplay");
            }
        }

        /// <summary>
        /// 显示所有编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="tabName">表名</param>
        private void disNumInfo(DataTable dt, string tabName)
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
                fmNum.LayerName = LayerName;
                fmNum.mapControl = mapControl;
                fmNum.getFromNamePath = getFromNamePath;
                fmNum.StrCon = StrCon;
                fmNum.setInfo(dt, tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayNumberInfo");
            }
        }

        /// <summary>
        /// 显示详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="row">要显示的记录行</param>
        /// <param name="pt">显示的位置</param>
        private void disPlayInfo(DataRow row, System.Drawing.Point pt)
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
                fmIn.setInfo(row, pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// 对象定位按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
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
                string[] winStr = new string[] { row["X"].ToString(), row["Y"].ToString(),
                                                 CLC.ForSDGA.GetFromTable.TableName,
                                                 row[CLC.ForSDGA.GetFromTable.ObjID].ToString(),
                                                 row[CLC.ForSDGA.GetFromTable.ObjName].ToString() };
                clickAnchor(winStr);
                k = 0;

                this.timerFilcker.Start();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnPoint_Click");
            }
        }

        /// <summary>
        /// 创建并定位点
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="winStr">数组参数（X、Y、表名、表ID、记录名称）</param>
        private Style defaultStyle;
        private void clickAnchor(string[] winStr)
        {
            try
            {
                FeatureLayer ftla = (FeatureLayer)mapControl.Map.Layers[LayerName];
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

                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                    cs = new CompositeStyle(pStyle);
                    Feature pFeat = new Feature(tableTem.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["表_ID"] = winStr[3];
                    pFeat["表名"] = winStr[2];
                    pFeat["名称"] = winStr[4];
                    tableTem.InsertFeature(pFeat);              // 先在地图上画点

                    flashFt = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem.Alias, 
                                                                                      MapInfo.Data.SearchInfoFactory.SearchWhere("表_ID='" + winStr[3] +
                                                                                                                                 "' and 表名='" + winStr[2] + "'"));
                    defaultStyle = flashFt.Style;               // 再找到此点

                    if (flashFt != null)
                    {
                        mapControl.Map.SetView(flashFt);        // 定位此点
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
                ExToLog(ex, "clickAnchor");
            }
        }
    }
}