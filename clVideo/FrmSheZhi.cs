using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Data.OracleClient;
using System.Drawing.Printing;
using System.Reflection;

namespace clVideo
{
    public partial class FrmSheZhi : Form
    {
        public FrmSheZhi()
        {
            InitializeComponent();
        }
        public string strConn;

        /// <summary>
        /// 消取按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 确定按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (comboBox3.Text == "" || comboBox3.Text == "---请选择报表类型---")
            {
                MessageBox.Show("请选择报表类型！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (comboBox3.Text == "" || comboBox3.Text == "---请选择报表类型---")
                {
                    MessageBox.Show("请选择报表类型！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                else
                {
                    this.Cursor = Cursors.WaitCursor;
                    switch (comboBox3.Text)
                    {
                        case "治安卡口视频监控系统明细表":
                            SKXTbiao();
                            break;
                        case "公共安全视频监控建设总体情况表":
                            GAQSbiao();
                            break;
                        case "重点区域、场所系统联网建设情况表":
                            ZDCSbiao();
                            break;
                        default:
                            MessageBox.Show("无法生成此报表！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnOK_Click");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 窗体加载
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void FrmSheZhi_Load(object sender, EventArgs e)
        {
            try
            {
                this.comboBox3.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "FrmSheZhi_Load");
            }
        }

        /// <summary>
        /// 公共安全视频监控建设总体情况表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void GAQSbiao()
        {
            try
            {
                this.isShowPro(true);
                Excel.Application xls_exp = new Excel.ApplicationClass();
                Excel._Workbook xls_book;
                this.proSheZhi.Value = 1;
                Application.DoEvents();

                xls_book = xls_exp.Workbooks._Open(Application.StartupPath + @"\GAQSbiao.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                    , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                Excel._Worksheet xls_sheet = (Excel.Worksheet)xls_book.ActiveSheet;
                Object oMissing = System.Reflection.Missing.Value;  //实例化对象时缺省参数 
                this.proSheZhi.Value = 2;
                Application.DoEvents();

                xls_sheet.Cells[2, 1] = "填表单位：顺德区公安局视频办                                                        统计月份：" + System.DateTime.Now.GetDateTimeFormats('y')[0].ToString();
                Application.DoEvents();

                xls_exp.Visible = true;
                int generation = 0;
                xls_exp.UserControl = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xls_exp);
                generation = System.GC.GetGeneration(xls_exp);
                this.proSheZhi.Value = 4;
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);

                xls_exp = null;
                xls_book = null;
                xls_sheet = null;
                this.isShowPro(false);
            }
            catch (Exception ex)
            {
                this.isShowPro(false);
                MessageBox.Show("请确定本机已安装Excel软件！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                ExToLog(ex, "GAQSbiao");
            }
        }

        /// <summary>
        /// 视频卡口视频监控系统明细表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void SKXTbiao()
        {
            try
            {
                this.isShowPro(true);
                Excel.Application xls_exp = new Excel.ApplicationClass();
                Excel._Workbook xls_book;

                xls_book = xls_exp.Workbooks._Open(Application.StartupPath + @"\SKXTbiao.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                    , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                Excel._Worksheet xls_sheet = (Excel.Worksheet)xls_book.ActiveSheet;
                Object oMissing = System.Reflection.Missing.Value;  //实例化对象时缺省参数 

                Excel.Range rng3;
                Application.DoEvents();
                OracleDataAdapter oraOda = null;
                OracleConnection oraConn = null;
                try
                {
                    oraConn = new OracleConnection(strConn);
                    oraConn.Open();
                    oraOda = new OracleDataAdapter("select 编号 as 编号,所属派出所 as 辖区,卡口名称 as 名称,安装地点 as 安装位置,监控方向 as 方向,卡口对应车道数 as 车道数,监控车道数 as 监控车道,to_jinweistyle(X,'东经') as X,to_jinweistyle(Y,'北纬') as Y,监控点接壤地区 as 监控点接壤地区 from 治安卡口系统", oraConn);
                    DataSet objset = new DataSet();
                    oraOda.Fill(objset);
                    this.proSheZhi.Maximum = objset.Tables[0].Rows.Count + 4;
                    this.proSheZhi.Value = 1;
                    Application.DoEvents();
                    // 先清除值
                    for (int i = 4; i < 5000; i++)
                    {
                        rng3 = xls_sheet.get_Range("A" + i.ToString(), Missing.Value);
                        if ( rng3.Text.ToString() != "")　// 先判断ID列是否有值，如果有值先删除
                        {
                            xls_sheet.Cells[i, 1] = "";
                            xls_sheet.Cells[i, 2] = "";
                            xls_sheet.Cells[i, 3] = "";
                            xls_sheet.Cells[i, 13] = "";
                            xls_sheet.Cells[i, 14] = "";
                            xls_sheet.Cells[i, 4] = "";
                            xls_sheet.Cells[i, 5] = "";
                            xls_sheet.Cells[i, 9] = "";
                            xls_sheet.Cells[i, 10] = "";
                            xls_sheet.Cells[i, 15] = "";
                        }
                        else　// 否则跳出循环
                        {
                            break;
                        }
                    }
                    this.proSheZhi.Value = 4;
                    Application.DoEvents();
                    //　再添加值
                    for (int i = 4; i < objset.Tables[0].Rows.Count + 4; i++)
                    {
                        xls_sheet.Cells[i, 1] = objset.Tables[0].Rows[i - 4]["编号"].ToString();
                        xls_sheet.Cells[i, 2] = objset.Tables[0].Rows[i - 4]["辖区"].ToString();
                        xls_sheet.Cells[i, 3] = objset.Tables[0].Rows[i - 4]["名称"].ToString();
                        xls_sheet.Cells[i, 13] = objset.Tables[0].Rows[i - 4]["X"].ToString();
                        xls_sheet.Cells[i, 14] = objset.Tables[0].Rows[i - 4]["Y"].ToString();
                        xls_sheet.Cells[i, 4] = objset.Tables[0].Rows[i - 4]["安装位置"].ToString();
                        xls_sheet.Cells[i, 5] = objset.Tables[0].Rows[i - 4]["方向"].ToString();
                        xls_sheet.Cells[i, 9] = objset.Tables[0].Rows[i - 4]["车道数"].ToString();
                        xls_sheet.Cells[i, 10] = objset.Tables[0].Rows[i - 4]["监控车道"].ToString();
                        xls_sheet.Cells[i, 15] = objset.Tables[0].Rows[i - 4]["监控点接壤地区"].ToString();

                        this.proSheZhi.Value = i;
                        Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    this.isShowPro(false);
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    this.isShowPro(false);
                    oraOda.Dispose();
                    oraConn.Close();
                }
                xls_exp.Visible = true;
                int generation = 0;
                xls_exp.UserControl = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xls_exp);
                generation = System.GC.GetGeneration(xls_exp);
                xls_exp = null;
                xls_book = null;
                xls_sheet = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("请确定本机已安装Excel软件！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                ExToLog(ex, "SKXTbiao");
            }
        }

        /// <summary>
        /// 重点区域、场所系统联网建设情况表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        private void ZDCSbiao()
        {
            try
            {
                isShowPro(true);
                Excel.Application xls_exp = new Excel.ApplicationClass();
                Excel._Workbook xls_book; 
                this.proSheZhi.Value = 1;
                Application.DoEvents();

                xls_book = xls_exp.Workbooks._Open(Application.StartupPath + @"\ZDCSbiao.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                    , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                Excel._Worksheet xls_sheet = (Excel.Worksheet)xls_book.ActiveSheet;
                Object oMissing = System.Reflection.Missing.Value;  //实例化对象时缺省参数 
                this.proSheZhi.Value = 2;
                Application.DoEvents();

                xls_exp.Visible = true;
                int generation = 0;
                xls_exp.UserControl = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xls_exp);
                generation = System.GC.GetGeneration(xls_exp);
                this.proSheZhi.Value = 4;
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);

                xls_exp = null;
                xls_book = null;
                xls_sheet = null;
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                MessageBox.Show("请确定本机已安装Excel软件！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                ExToLog(ex, "ZDCSbiao");
            }
        }

        /// <summary>
        /// 隐藏或显示进度条
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="falg">布尔值</param>
        private void isShowPro(bool falg)
        {
            try
            {
                if (!falg)
                {
                    Size size = new Size(337,102);
                    this.Size = size;
                }
                else
                {
                    Size size = new Size(337,131);
                    this.Size = size;
                }
                this.proSheZhi.Maximum = 4;
                this.proSheZhi.Value = 0;
                this.sheZhilbl.Visible = falg;
                this.proSheZhi.Visible = falg;
                Application.DoEvents();
            }
            catch (Exception ex) { ExToLog(ex, "isShowPro"); }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-26
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clVideo-FrmSheZhi-" + sFunc);
        }
    }
}