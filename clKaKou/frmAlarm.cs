using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.IO;

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Dialogs;
using MapInfo.Windows.Controls;

namespace clKaKou
{
    public partial class frmAlarm : Form
    {
        public string[] StrCon;

        public string myconstr = string.Empty;

        public string KKuser = string.Empty;  // 报警设置对象

        public string strRegion = string.Empty;  //用户权限，用来与布控单位相比较

        public MapControl Mapcontrol1;

        public string UserName = string.Empty;//系统登录用户名称


        public frmAlarm()
        {
            InitializeComponent();
        }

        private void frmAlarm_Load(object sender, EventArgs e)
        {
           
        }


        /// <summary>
        /// 窗体关闭
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void frmAlarm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {              
                this.Visible = false;
                e.Cancel = true;
            }
            catch (Exception ex)
            {
               writeToLog(ex, "clKaKou-frmAlarm-01-窗体关闭");
            }
        }  

        /// <summary>
        /// 设置背景色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.dataGridView1.Rows.Count != 0)
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightPink ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmAlarm-02-设置DataGrid颜色");
            }
        }

        /// <summary>
        /// 双击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;   //点击表头,退出

                if (this.Mapcontrol1.Map.Layers["KKLayer"] == null)
                {
                    MessageBox.Show("请切换至治安卡口功能模块下进行报警信息查看！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                OpenInfo(this.dataGridView1.CurrentRow.Cells[0].Value.ToString());

                WriteEditLog("查看卡口报警编号为:" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + " 的详细信息", "查看报警信息", "V_ALARM");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmAlarm-03-双击事件");
            }            
        }

        /// <summary>
        /// 显示车辆信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private FrmInfo frminfo = new FrmInfo();
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (this.frminfo.Visible == false)
                {
                    this.frminfo = new FrmInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.Visible = false;
                }
                frminfo.setInfo(dt.Rows[0], pt,StrCon,UserName);
            }
            catch (Exception ex)
            {
                writeToLog(ex, " frmAlarm-04-显示车辆信息");
            }
        }


        private IResultSetFeatureCollection rsfcflash = null;

        /// <summary>
        /// 查看详细信息
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="KeyID">报警编号</param>
        public void OpenInfo(string KeyID)
        {
           try
            {
                DPoint dp = new DPoint();
                string sqlFields = "报警编号,报警类型,报警卡口编号,报警卡口名称,报警时间,车辆号牌,号牌种类,行驶方向,行驶速度,布控名单编号,布控单位,布控人,联系电话,照片1,照片2,照片3,处理状态,处理人,处理时间,处理情况";
                string strSQL = "select " + sqlFields + " from V_ALARM t where 报警编号 ='" + KeyID + "'";
                DataTable datatable = GetTable(strSQL);

                if (datatable.Rows.Count > 0)
                {
                    //////////////////////////////////////////
                    string tempname = string.Empty;
                    string clqk = string.Empty;

                    foreach (DataRow dr in datatable.Rows)
                    {
                        tempname = dr["报警卡口编号"].ToString();
                        clqk = dr["处理情况"].ToString();
                    }
                    
                    if (clqk.IndexOf(this.UserName) < 0)
                    {
                        clqk = clqk + "," + this.UserName;

                        string sql = "Update V_ALARM set 处理情况='" + clqk + "' where 报警编号 ='" + KeyID + "'";
                        this.RunCommand(sql);
                    }



                    string tblname = "KKLayer";

                    //提取当前选择的信息的通行车辆编号作为主键值

                    MapInfo.Mapping.Map map = this.Mapcontrol1.Map;

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }

                    MapInfo.Data.MIConnection conn = new MIConnection();
                    try
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                        conn.Open();

                        MapInfo.Data.MICommand cmd = conn.CreateCommand();
                        Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                        cmd.CommandText = "select * from " + Mapcontrol1.Map.Layers[tblname].ToString() + " where ID = @name ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@name", tempname);
                        MapInfo.Geometry.CoordSys cSys = Mapcontrol1.Map.GetDisplayCoordSys();

                        rsfcflash = cmd.ExecuteFeatureCollection();
                        if (rsfcflash.Count > 0)
                        {
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                            foreach (Feature f in rsfcflash)
                            {
                                Mapcontrol1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                Mapcontrol1.Map.Center = f.Geometry.Centroid;
                                dp.x = f.Geometry.Centroid.x;
                                dp.y = f.Geometry.Centroid.y;
                                break;
                            }
                            cmd.Dispose();
                            conn.Close();

                            this.timer1.Enabled = true;
                        }

                    }
                    catch { }
                    finally { if (conn.State == ConnectionState.Open) conn.Close(); }
                    /////////////////////////////////////////

                    if (dp.x == 0 || dp.y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("此对象未定位!");
                        return;
                    }
                    System.Drawing.Point pt = new System.Drawing.Point();
                    Mapcontrol1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                    
                    WriteEditLog("报警编号='" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'", "查看详情","V_ALARM");
                }
            }
            catch (Exception ex)
            {

                writeToLog(ex, "clKaKou-frmAlarm-05-查看详细信息");
            }        
        }

 
        private int iflash = 0;
        /// <summary>
        /// 图元闪现
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                iflash = iflash + 1;

                int i = iflash % 2;
                if (i == 0)
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                }
                else
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }

                if (this.iflash % 10 == 0)
                {
                    this.timer1.Enabled = false;
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmAlarm-06-图元闪现");
            }
        }

        /// <summary>
        /// 单击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string tempname = this.dataGridView1.CurrentRow.Cells[2].Value.ToString();

                string tblname = "KKLayer";

                //提取当前选择的信息的通行车辆编号作为主键值

                MapInfo.Mapping.Map map = this.Mapcontrol1.Map;

                if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                {
                    return;
                }
                rsfcflash = null;

                MapInfo.Data.MIConnection conn = new MIConnection();
                conn.Open();

                MapInfo.Data.MICommand cmd = conn.CreateCommand();
                Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                cmd.CommandText = "select * from " + Mapcontrol1.Map.Layers[tblname].ToString() + " where ID = @name ";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@name", tempname);
                MapInfo.Geometry.CoordSys cSys = Mapcontrol1.Map.GetDisplayCoordSys();

                this.rsfcflash = cmd.ExecuteFeatureCollection();
                if (this.rsfcflash.Count > 0)
                {
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                    foreach (Feature f in this.rsfcflash)
                    {
                        Mapcontrol1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                        Mapcontrol1.Map.Center = f.Geometry.Centroid;
                        break;
                    }
                    cmd.Clone();
                    conn.Close();

                    this.timer1.Enabled = true;
                }                
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmAlarm-07-单击事件");
            }
        }

        /// <summary>
        /// 获取缩放比例
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <returns>缩放比例</returns>
        private double getScale()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("比例尺", "缩放比例"));
                return dou;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clKaKou.frmAlarm-getScale");
                return 0;
            }
        }
        /// <summary>
        /// 查询SQL
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
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
        /// 获取Scalar
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>唯一值</returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
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
                writeToLog(ex, "ucKakou-61-记录操作记录");
            }
        }
    }
}