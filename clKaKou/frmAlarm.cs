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

        public string KKuser = string.Empty;  // �������ö���

        public string strRegion = string.Empty;  //�û�Ȩ�ޣ������벼�ص�λ��Ƚ�

        public MapControl Mapcontrol1;

        public string UserName = string.Empty;//ϵͳ��¼�û�����


        public frmAlarm()
        {
            InitializeComponent();
        }

        private void frmAlarm_Load(object sender, EventArgs e)
        {
           
        }


        /// <summary>
        /// ����ر�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
               writeToLog(ex, "clKaKou-frmAlarm-01-����ر�");
            }
        }  

        /// <summary>
        /// ���ñ���ɫ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
                writeToLog(ex, "clKaKou-frmAlarm-02-����DataGrid��ɫ");
            }
        }

        /// <summary>
        /// ˫���¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;   //�����ͷ,�˳�

                if (this.Mapcontrol1.Map.Layers["KKLayer"] == null)
                {
                    MessageBox.Show("���л����ΰ����ڹ���ģ���½��б�����Ϣ�鿴��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                OpenInfo(this.dataGridView1.CurrentRow.Cells[0].Value.ToString());

                WriteEditLog("�鿴���ڱ������Ϊ:" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + " ����ϸ��Ϣ", "�鿴������Ϣ", "V_ALARM");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmAlarm-03-˫���¼�");
            }            
        }

        /// <summary>
        /// ��ʾ������Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
                writeToLog(ex, " frmAlarm-04-��ʾ������Ϣ");
            }
        }


        private IResultSetFeatureCollection rsfcflash = null;

        /// <summary>
        /// �鿴��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="KeyID">�������</param>
        public void OpenInfo(string KeyID)
        {
           try
            {
                DPoint dp = new DPoint();
                string sqlFields = "�������,��������,�������ڱ��,������������,����ʱ��,��������,��������,��ʻ����,��ʻ�ٶ�,�����������,���ص�λ,������,��ϵ�绰,��Ƭ1,��Ƭ2,��Ƭ3,����״̬,������,����ʱ��,�������";
                string strSQL = "select " + sqlFields + " from V_ALARM t where ������� ='" + KeyID + "'";
                DataTable datatable = GetTable(strSQL);

                if (datatable.Rows.Count > 0)
                {
                    //////////////////////////////////////////
                    string tempname = string.Empty;
                    string clqk = string.Empty;

                    foreach (DataRow dr in datatable.Rows)
                    {
                        tempname = dr["�������ڱ��"].ToString();
                        clqk = dr["�������"].ToString();
                    }
                    
                    if (clqk.IndexOf(this.UserName) < 0)
                    {
                        clqk = clqk + "," + this.UserName;

                        string sql = "Update V_ALARM set �������='" + clqk + "' where ������� ='" + KeyID + "'";
                        this.RunCommand(sql);
                    }



                    string tblname = "KKLayer";

                    //��ȡ��ǰѡ�����Ϣ��ͨ�г��������Ϊ����ֵ

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
                        System.Windows.Forms.MessageBox.Show("�˶���δ��λ!");
                        return;
                    }
                    System.Drawing.Point pt = new System.Drawing.Point();
                    Mapcontrol1.Map.DisplayTransform.ToDisplay(dp, out pt);
                    pt.X += this.Width + 10;
                    pt.Y += 80;
                    this.disPlayInfo(datatable, pt);
                    
                    WriteEditLog("�������='" + this.dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'", "�鿴����","V_ALARM");
                }
            }
            catch (Exception ex)
            {

                writeToLog(ex, "clKaKou-frmAlarm-05-�鿴��ϸ��Ϣ");
            }        
        }

 
        private int iflash = 0;
        /// <summary>
        /// ͼԪ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
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
                writeToLog(ex, "clKaKou-frmAlarm-06-ͼԪ����");
            }
        }

        /// <summary>
        /// �����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string tempname = this.dataGridView1.CurrentRow.Cells[2].Value.ToString();

                string tblname = "KKLayer";

                //��ȡ��ǰѡ�����Ϣ��ͨ�г��������Ϊ����ֵ

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
                writeToLog(ex, "clKaKou-frmAlarm-07-�����¼�");
            }
        }

        /// <summary>
        /// ��ȡ���ű���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <returns>���ű���</returns>
        private double getScale()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("������", "���ű���"));
                return dou;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "clKaKou.frmAlarm-getScale");
                return 0;
            }
        }
        /// <summary>
        /// ��ѯSQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ִ��SQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// ��ȡScalar
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">sql���</param>
        /// <returns>Ψһֵ</returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">sql���</param>
        /// <param name="method">������ʽ</param>
        /// <param name="tablename">����</param>
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + UserName + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�ΰ�����','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucKakou-61-��¼������¼");
            }
        }
    }
}