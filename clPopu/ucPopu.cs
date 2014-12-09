using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using MapInfo.Windows;
using MapInfo.Windows.Dialogs;
using MapInfo.Styles;
using MapInfo.Mapping;
using MapInfo.Geometry;
using MapInfo.Data;
using MapInfo.Tools;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CLC;
using System.Runtime.InteropServices;

namespace clPopu
{
    public partial class ucPopu : UserControl
    {
        private MapInfo.Windows.Controls.MapControl mapControl1;
        private string strConn;
        private string[] conStr = null;
        private string getFromNamePath = "";

        public string strRegion = "";
        public string strRegion1 = "";
        public string user = "";

        public string strRegion2 = "";      // �ɵ������ɳ���
        public string strRegion3 = "";      // �ɵ������ж�
        public string excelSql = "";        // ��ѯ����sql
        public string exportSql = "";       // ��������SQL
        public System.Data.DataTable dtExcel = null; //��ͼҳ�����ݵ�����ť

        public ToolStripProgressBar toolPro;  // ���ڲ�ѯ�Ľ�������lili 2010-8-10
        public ToolStripLabel toolProLbl;     // ������ʾ�����ı���
        public ToolStripSeparator toolProSep;

        #region ���뷨
        //����һЩAPI����
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetOpenStatus(IntPtr himc);
        [DllImport("imm32.dll")]
        public static extern bool ImmSetOpenStatus(IntPtr himc, bool b);
        [DllImport("imm32.dll")]
        public static extern bool ImmGetConversionStatus(IntPtr himc, ref int lpdw, ref int lpdw2);
        [DllImport("imm32.dll")]
        public static extern int ImmSimulateHotKey(IntPtr hwnd, int lngHotkey);
        private const int IME_CMODE_FULLSHAPE = 0x8;
        private const int IME_CHOTKEY_SHAPE_TOGGLE = 0x11;
        #endregion

        private MIConnection _miConnection = new MIConnection();

        /// <summary>
        /// ���캯��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="m">��ͼ�ؼ�</param>
        /// <param name="s">���������ַ���</param>
        /// <param name="canStr">���ݿ����Ӳ���</param>
        /// <param name="getFnPath">�����ļ�GetFromNameConfig.ini·��</param>
        public ucPopu(MapInfo.Windows.Controls.MapControl m, string s,string[] canStr,string getFnPath)
        {
            try
            {
                InitializeComponent();
                mapControl1 = m;
                strConn = s;
                getFromNamePath = getFnPath;
                conStr = canStr;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(canStr[0], canStr[1], canStr[2]);
                setEvents();
                this.P_setfield();
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "ucPopu���캯��");
            }
        }

        /// <summary>
        /// ��ӵ�ͼԪ��ѡ���¼� 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void setEvents()
        {
            this.mapControl1.Tools.FeatureSelected += new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);
        }

        private MapInfo.Data.MultiResultSetFeatureCollection mirfc = null;
        private MapInfo.Data.IResultSetFeatureCollection mirfc1 = null;

        /// <summary>
        /// ��ͼԪ��ѡ���¼� 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void Feature_Selected(object sender, MapInfo.Tools.FeatureSelectedEventArgs e)
        {
            if (this.Visible)
            {
                string ��_ID = string.Empty;
                try
                {
                    this.mirfc = e.Selection;
                    if (this.mirfc.Count == 0)
                    {
                        return;
                    }
                    this.mirfc1 = this.mirfc[0];
                    if (this.mirfc1.Count == 0)
                    {
                        return;
                    }
                    Feature f = this.mirfc1[0];
                    if (f == null)
                    {
                        return;
                    }

                    this.showDataGridViewLineOnlyOneTable(f["��_ID"].ToString(), "�˿�ϵͳ");
                }
                catch (Exception ex)
                {
                    writeToPopuLog(ex, "Feature_Selected");
                }
            }
        }

        /// <summary>
        /// ��DataGridView������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="��_ID">��_ID</param>
        /// <param name="����">����</param>
        public void showDataGridViewLineOnlyOneTable(string ��_ID, string ����)//
        {
            try
            {
                for (int i = 0; i < this.dataGV.Rows.Count; i++)
                {
                    if (this.dataGV.Rows[i].Cells["���֤����"].Value.ToString() == ��_ID)
                    {
                        this.dataGV.CurrentCell = this.dataGV.Rows[i].Cells[0];
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "showDataGridViewLineOnlyOneTable");
            }
        }

        /// <summary>
        /// ��ѯ��ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridExp.Rows.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("����Ӳ�ѯ���!", "��ʾ");
                    return;
                }

                if (getSqlString() == "")
                {
                    System.Windows.Forms.MessageBox.Show("��ѯ����д���,������!", "��ʾ");
                    return;
                }
                popuSearch();
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "buttonSearch_Click");
            }
        }

        string strWhere = "";
        //---------��ҳ��ȫ�ֱ���------
        int _startNo = 1;   // ��ʼ������
        int _endNo = 0;���� // ����������
        //----------------------------
        /// <summary>
        /// ����������ѯ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void popuSearch()
        {
            isShowPro(true);
            strWhere = getSqlString();

            if (strRegion == "")     //add by fisher in 09-12-04
            {
                isShowPro(false);
                System.Windows.Forms.MessageBox.Show("��û�й���Ȩ�ޣ�");
                return;
            }
            if (strRegion != "˳����" && strRegion != "")
            {
                string sRegion = strRegion;
                if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                {
                    sRegion = strRegion.Replace("����", "����,��ʤ");
                }
                strWhere += " and �����ɳ��� in ('" + sRegion.Replace(",", "','") + "')";
            }
            _endNo = Convert.ToInt32(this.TextNum1.Text);
            _startNo = 1;

            //strSQL = "select  ����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,�����ɳ���,���ݱ��,��ȡ����ʱ��,'�鿴...' as ��Ƭ,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y  from �˿�ϵͳ t where " + strWhere + " and �����ֶ�һ is null or �����ֶ�һ=''";

            this.Cursor = Cursors.WaitCursor;
            try
            {
                //alter by siumo 090116    + " and �����ֶ�һ is null or �����ֶ�һ=''"
                this.getMaxCount("select count(*) from �˿�ϵͳ t where " + strWhere + " and (�����ֶ�һ is null or �����ֶ�һ='')");
                InitDataSet(RecordCount1); //��ʼ�����ݼ�

                if (nMax < 1)
                {
                    isShowPro(false);
                    clearTem();
                    System.Windows.Forms.MessageBox.Show("���������޼�¼.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    return;
                }

                DataTable datatable = LoadData(_startNo, _endNo, strWhere,false); //��ȡ��ǰҳ����

                dataGV.DataSource = datatable;
                this.toolPro.Value = 1;
                Application.DoEvents();

                #region ���ݵ���Excel
                //excelSql = "select ����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,�����ɳ���,���ݱ��,��ȡID,��ȡ����ʱ��,��������,�����ֶ�һ,�����ֶζ�,�����ֶ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y ";
                //excelSql += strSQL.Substring(strSQL.IndexOf("from"));
                excelSql = strWhere;
                string sRegion2 = strRegion2;
                string sRegion3 = strRegion3;
                if (strRegion2 == "")
                {
                    excelSql += " and 1=2 ";
                }
                else if (strRegion2 != "˳����")
                {
                    if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
                    {
                        sRegion2 = strRegion2.Replace("����", "����,��ʤ");
                    }
                    excelSql += " and �����ɳ��� in ('" + sRegion2.Replace(",", "','") + "')";
                }
                LoadData(_startNo, _endNo, excelSql, true);
                //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql, true);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                ////string sql = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from �˿�ϵͳ a where rownum<=" + _endNo + " and " + excelSql + " and �����ֶ�һ is null or �����ֶ�һ='') t where rn1 >=" + _startNo;
                #endregion

                this.toolPro.Value = 2;
                //Application.DoEvents();
                //���������еı���ɫ
                for (int i = 1; i < dataGV.Rows.Count; i += 2)
                {
                    dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233); 
                }
                createPoint(datatable);//����
                WriteEditLog(strWhere, "��ѯ");
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToPopuLog(ex, "popuSearch");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// ��ͼ�ϴ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="dt">����Դ</param>
        private void createPoint(DataTable dt)
        {
            try
            {
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["�˿�ϵͳ"];
                Table tableTem = fl.Table;

                //��������ж���
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        double dx = Convert.ToDouble(dt.Rows[i]["X"]);
                        double dy = Convert.ToDouble(dt.Rows[i]["Y"]);
                        if (dx > 0 && dy > 0)
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);
                            CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("ren.bmp"));

                            Feature pFeat = new Feature(tableTem.TableInfo.Columns);
                            pFeat.Geometry = pt;
                            pFeat.Style = cs;
                            pFeat["��_ID"] = dt.Rows[i]["���֤����"].ToString();
                            pFeat["����"] = "�˿�ϵͳ";

                            tableTem.InsertFeature(pFeat);
                        }
                    }
                    catch (Exception ex) { writeToPopuLog(ex, "createPoint"); }
                }
            }
            catch (Exception ex) { writeToPopuLog(ex, "createPoint"); }
        }

        private Feature flashFt;
        private Style defaultStyle;
        private int k;
        private string StrID = "";

        /// <summary>
        /// �����Ԫ�񣬲��Ҷ�Ӧ��Ҫ�أ��任Ҫ�ص���ʽ��ʵ����˸
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void dataGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            dataGV.Rows[e.RowIndex].Selected = true;
            if (this.dataGV.CurrentCell.OwningColumn.Name == "��Ƭ")
            {
                System.Data.OracleClient.OracleCommand oracleCmd = null;
                System.Data.OracleClient.OracleDataReader dataReader = null;
                System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=czrk_cx;Password=czrk_cx;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.47.227.15)(PORT=1521)))(CONNECT_DATA=(SID=ora81)))");;
                try
                {
                    FrmImage fimage = new FrmImage();
                    oraconn.Open();
                    string sqlstr = "";
                    if (this.dataGV.CurrentRow.Cells["�˿�����"].Value.ToString() == "��ס�˿�")
                        sqlstr = "select TX from czrk_cx.v_gis_czrk_tx where ZJHM='" + this.dataGV.CurrentRow.Cells["���֤����"].Value.ToString() + "'";
                    else
                        sqlstr = "select TX from czrk_cx.v_gis_ldry_tx where ZJHM='" + this.dataGV.CurrentRow.Cells["���֤����"].Value.ToString() + "'";

                    oracleCmd = new OracleCommand(sqlstr, oraconn);
                    dataReader = oracleCmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        byte[] bytes = new byte[(dataReader.GetBytes(0, 0, null, 0, int.MaxValue))];
                        if (dataReader.IsDBNull(0))
                        {
                            System.Windows.Forms.MessageBox.Show("��Ƭδ�浵!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, bytes.Length);
                            dataReader.Close();
                            Stream fs = new MemoryStream(bytes);

                            //����һ��bitmap���͵�bmp��������ȡ�ļ���
                            Bitmap bmp = new Bitmap(Image.FromStream(fs));
                            bmp.Save(Application.StartupPath + "\\lbs.jpg");
                            fimage.pictureBox1.ImageLocation = Application.StartupPath + "\\lbs.jpg";

                            bmp.Dispose();//�ͷ�bmp�ļ���Դ
                            fimage.pictureBox1.Invalidate();
                            fs.Close();
                            fimage.TopMost = true;
                            fimage.ShowDialog();
                            fimage.Dispose();
                            File.Delete(Application.StartupPath + "\\lbs.jpg");
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("��Ƭδ�浵!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    writeToPopuLog(ex, "dataGV_CellClick");
                }
                finally
                {
                    oracleCmd.Dispose();
                    oraconn.Close();
                }
            }
            else
            {
                try
                {
                    timer1.Stop();
                    if (flashFt != null)
                    {
                        try
                        {
                            flashFt.Style = defaultStyle;
                            flashFt.Update();
                        }
                        catch { }
                    }
                    //���һ����¼�����е�ͼ��λ
                    if (dataGV["X", e.RowIndex].Value == null || dataGV["Y", e.RowIndex].Value == null || dataGV["X", e.RowIndex].Value.ToString() == "" || dataGV["Y", e.RowIndex].Value.ToString() == "")
                    {
                        return;
                    }
                    double x = 0, y = 0;
                    try
                    {
                        x = Convert.ToDouble(dataGV["X", e.RowIndex].Value.ToString());
                        y = Convert.ToDouble(dataGV["Y", e.RowIndex].Value.ToString());
                    }
                    catch { return; }

                    if (x == 0 || y == 0)
                    {
                        return;
                    }
                    StrID = dataGV["���֤����", e.RowIndex].Value.ToString();

                    // ���´�����������ǰ��ͼ����Ұ�������ö������ڵ��ɳ���   add by fisher in 09-12-24
                    MapInfo.Geometry.DPoint dP = new MapInfo.Geometry.DPoint(x, y);
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                    //MapInfo.Geometry.FeatureGeometry fg = new MapInfo.Geometry.Point(cSys, dP);
                    //SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(fg, IntersectType.Geometry);
                    //si.QueryDefinition.Columns = null;
                    //Feature ftjz = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", si);
                    //if (ftjz != null)
                    //{
                    //    mapControl1.Map.SetView(ftjz);
                    //}
                    //else
                    //{
                    mapControl1.Map.SetView(dP, cSys, getScale());
                    this.mapControl1.Map.Center = dP;
                    //}

                    FeatureLayer tempLayer = mapControl1.Map.Layers["�˿�ϵͳ"] as MapInfo.Mapping.FeatureLayer;

                    Table tableTem = tempLayer.Table;
                    Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem, MapInfo.Data.SearchInfoFactory.SearchWhere("��_ID='" + dataGV["���֤����", e.RowIndex].Value.ToString() + "'"));

                    //��˸Ҫ��
                    flashFt = ft;
                    defaultStyle = ft.Style;
                    k = 0;
                    timer1.Start();
                }
                catch (Exception ex)
                {
                    writeToPopuLog(ex, "dataGV_CellClick");
                }
            }
        }

        /// <summary>
        /// ��ȡ���ű���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
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
                writeToPopuLog(ex, "getScale");
                return 0;
            }
        }

        /// <summary>
        /// ˫���б���ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        void dataGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;   //�����ͷ,�˳�

            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                DPoint dp = new DPoint();
                CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", getFromNamePath);
                string sqlFields =CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from �˿�ϵͳ t where ���֤����='" + dataGV.Rows[e.RowIndex].Cells["���֤����"].Value.ToString() + "'";

                DataSet ds = new DataSet();
                OracleDataAdapter da = new System.Data.OracleClient.OracleDataAdapter(strSQL, Conn);
                da.Fill(ds);
                da.Dispose();
                Conn.Close();
                DataTable datatable = ds.Tables[0];

                System.Drawing.Point pt = new System.Drawing.Point();
                if (dataGV["X", e.RowIndex].Value != null || dataGV["Y", e.RowIndex].Value != null || dataGV["X", e.RowIndex].Value.ToString() != "" || dataGV["Y", e.RowIndex].Value.ToString() != "")
                {
                    try
                    {
                        dp.x = Convert.ToDouble(datatable.Rows[0]["X"].ToString());
                        dp.y = Convert.ToDouble(datatable.Rows[0]["Y"].ToString());
                    }
                    catch
                    {
                        Screen scren = Screen.PrimaryScreen;
                        pt.X = scren.WorkingArea.Width/2;
                        pt.Y = 10;
                        this.disPlayInfo(datatable, pt, "�˿�ϵͳ");
                        WriteEditLog("���֤����='" + dataGV.Rows[e.RowIndex].Cells["���֤����"].Value.ToString() + "'", "�鿴����");
                        return;
                    }
                }
                if (dp.x == 0 || dp.y == 0)
                {
                    Screen scren = Screen.PrimaryScreen;
                    pt.X = scren.WorkingArea.Width / 2;
                    pt.Y = 10;
                    this.disPlayInfo(datatable, pt, "�˿�ϵͳ");
                    WriteEditLog("���֤����='" + dataGV.Rows[e.RowIndex].Cells["���֤����"].Value.ToString() + "'", "�鿴����");
                    return;
                }
                mapControl1.Map.DisplayTransform.ToDisplay(dp, out pt);
                pt.X += this.Width + 10;
                pt.Y += 80;
                this.disPlayInfo(datatable, pt,"�˿�ϵͳ");
                WriteEditLog("���֤����='" + dataGV.Rows[e.RowIndex].Cells["���֤����"].Value.ToString() + "'", "�鿴����");
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeToPopuLog(ex, "dataGV_CellDoubleClick");
            }
            finally
            {
                try
                {
                    fmDis.Visible = false;
                }
                catch { }
            }
        }

        private FrmInfo frmMessage = new FrmInfo();
        /// <summary>
        /// ��ʾ��¼��ϸ��Ϣ����
        /// </summary>
        /// <param name="dt">��¼����</param>
        /// <param name="pt">��ʾλ��</param>
        /// <param name="LayerName">ͼ����</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string LayerName)
        {
            try
            {
                if (this.frmMessage.Visible == false)
                {
                    this.frmMessage = new FrmInfo();
                    frmMessage.SetDesktopLocation(-30, -30);
                    frmMessage.Show();
                    frmMessage.Visible = false;
                }
                frmMessage.mapControl = mapControl1;
                frmMessage.getFromNamePath = getFromNamePath;
                frmMessage.strConn = strConn;
                frmMessage.setInfo(dt.Rows[0], pt,LayerName);
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "disPlayInfo");
            }
        }

        //private Color col = Color.Blue;
        /// <summary>
        /// ͼԪ��˸
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
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

        /// <summary>
        /// ��ʾ������ģ��ʱ��ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void ucPopu_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    this.FieldStr.Text = this.FieldStr.Items[0].ToString();
                    this.ValueStr.Text = "";
                    this.dataGridExp.Rows.Clear();
                }
                else
                {
                    RemoveTemLayer("�˿�ϵͳ");
                    dataGV.DataSource = null;

                    PageNow1.Text = "0";
                    PageCount1.Text = "/ {0}";
                    RecordCount1.Text = "0��";

                    pageSize = 0;     //ÿҳ��ʾ����
                    nMax = 0;         //�ܼ�¼��
                    pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
                    pageCurrent = 0;   //��ǰҳ��
                    nCurrent = 0;      //��ǰ��¼��
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "ucPopu_VisibleChanged");
            }
        }

        /// <summary>
        /// �Ƴ���ʱͼ��,�رձ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="tableAlies">����</param>
        private void RemoveTemLayer(string tableAlies)
        {
            try
            {
                if (mapControl1.Map.Layers[tableAlies] != null)
                {
                    mapControl1.Map.Layers.Remove(tableAlies);
                    MapInfo.Engine.Session.Current.Catalog.CloseTable(tableAlies);
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "RemoveTemLayer");
            }
        }

        /// <summary>
        /// �����ͼҪ�ؼ������б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void clearTem()
        {
            dataGV.DataSource = null;
            PageNow1.Text = "0";
            PageCount1.Text = "/ {0}";
            RecordCount1.Text = "0��";
            pageSize = 0;     //ÿҳ��ʾ����
            nMax = 0;         //�ܼ�¼��
            pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
            pageCurrent = 0;   //��ǰҳ��
            nCurrent = 0;      //��ǰ��¼��

            try
            {
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["�˿�ϵͳ"];
                Table tableTem = fl.Table;

                //��������ж���
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "clearTem");
            }
        }

        /// <summary>
        /// �����ı��򰴻س�ʱ��ѯ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void textKeyWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    popuSearch();
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "textKeyWord_KeyPress");
            }
        }

        int pageSize = 0;     //ÿҳ��ʾ����
        int nMax = 0;         //�ܼ�¼��
        int pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageCurrent = 0;   //��ǰҳ��
        int nCurrent = 0;      //��ǰ��¼��
        DataSet ds = new DataSet();
        /// <summary>
        /// �õ���¼����
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void getMaxCount(string sql)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();

                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                nMax = Convert.ToInt32(Cmd.ExecuteScalar().ToString());
                Cmd.Dispose();
                Conn.Close();
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "getMaxCount");
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nMax = 0;
            }
        }

        /// <summary>
        /// ��ʼ����ҳ�ؼ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="tsLabel">��ҳ�ӿؼ�������ʾ�ܼ�¼��</param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                pageSize = Convert.ToInt32(this.TextNum1.Text);      //����ҳ������
                TextNum1.Text = pageSize.ToString();
                tsLabel.Text =  nMax.ToString() + "��";//�ڵ���������ʾ�ܼ�¼��
                pageCount = (nMax / pageSize);//�������ҳ��
                if ((nMax % pageSize) > 0) pageCount++;
                if (nMax != 0)
                {
                    pageCurrent = 1;
                }
                nCurrent = 0;       //��ǰ��¼����0��ʼ
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "InitDataSet");
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ��ѯ����
        /// </summary>
        /// <param name="_startNo">��ʼ����</param>
        /// <param name="_endNo">��������</param>
        /// <param name="_whereSql">��ѯ����SQL</param>
        /// <param name="isExcel">�Ƿ����ɵ�����SQL</param>
        /// <returns>�����</returns>
        public DataTable LoadData(int _startNo, int _endNo, string _whereSql, bool isExcel)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                int nStartPos = 0;   //��ǰҳ�濪ʼ��¼��
                nStartPos = nCurrent;

                this.PageNow1.Text = Convert.ToString(pageCurrent);
                this.PageCount1.Text = "/" + pageCount.ToString();

                CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", getFromNamePath);
                DataTable dtInfo;
                string sql = "";
                if (isExcel)   // Ϊ����߲�ѯЧ�� ����ǵ���ֻҪ����SQL��䣬���ò�ѯ
                {
                    sql = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from �˿�ϵͳ a where rownum<=" + _endNo + " and " + _whereSql + " and (�����ֶ�һ is null or �����ֶ�һ='')) t where rn1 >=" + _startNo;
                    exportSql = sql;   // ������SQL���棬���������ťʱ����
                    return null;
                }
                else
                {
                    //sql = "select ����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,�����ɳ���,���ݱ��,��ȡ����ʱ��,'�鿴...' as ��Ƭ,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y from (select rownum as rn1,a.* from �˿�ϵͳ a where rownum<=" + _endNo + " and " + _whereSql + "and (�����ֶ�һ is null or �����ֶ�һ='')) t where rn1 >=" + _startNo;
                    sql = "select ����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,�����ɳ���,��ȡ����ʱ��,'�鿴...' as ��Ƭ,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y from (select rownum as rn1,a.* from �˿�ϵͳ a where rownum<=" + _endNo + " and " + _whereSql + "and (�����ֶ�һ is null or �����ֶ�һ='')) t where rn1 >=" + _startNo;
                }
                // and �����ֶ�һ is null or �����ֶ�һ=''

                dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(dtInfo);

                Cmd.Dispose();
                Conn.Close();

                dataHouse = new DataTable();
                dataHouse = dtInfo;        // �Ŵ�������Table

                return dtInfo;
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                writeToPopuLog(ex, "LoadData");
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// ��ѯ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="textNowPage">��ҳ�ӿؼ�������ʾ��ǰҳ��</param>
        /// <param name="lblPageCount">��ҳ�ӿؼ�������ʾ�ܼ�¼��</param>
        /// <param name="sql">sql���</param>
        /// <returns>�����</returns>
        public DataTable LoadData(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, string sql)
        {
            try
            {
                int nStartPos = 0;   //��ǰҳ�濪ʼ��¼��
                nStartPos = nCurrent;
                //lblPageCount.Text = "��" + Convert.ToString(pageCurrent) + "ҳ��" + pageCount.ToString() + "ҳ";
                textNowPage.Text = Convert.ToString(pageCurrent);
                lblPageCount.Text = "/" + pageCount.ToString();

                OracleConnection Conn = new OracleConnection(strConn);
                DataTable dtInfo;

                try
                {
                    dtInfo = new DataTable();
                    Conn.Open();
                    OracleCommand Cmd = Conn.CreateCommand();
                    Cmd.CommandText = sql;
                    OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                    DataTable[] dataTables = new DataTable[1];
                    for (int i = 0; i < dataTables.Length; i++)
                    {
                        dataTables[i] = new DataTable();
                    }
                    Adp.Fill(nStartPos, pageSize, dataTables); // ����ط���֪���Ǵ����ݿ��в鵽ǰ1000�з��أ��������е����ݾݶ���ѯ�����أ��ٴ��л�ȡǰ100�С�

                    dtInfo = dataTables[0];
                    Cmd.Dispose();
                    Conn.Close();

                    return dtInfo;
                }
                catch (Exception ex) {
                    if (Conn.State == ConnectionState.Open) {
                        Conn.Close();
                    }
                    writeToPopuLog(ex, "LoadData");
                    return null;
                    //MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "LoadData");
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// �������ڷ�ҳ�ϵĲ������ж��Ƿ񳬳�ҳ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <returns>����ֵ(true-��ͨ�� false-ͨ��)</returns>
        private bool bdnInfo_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.TextNum1.Text);
                if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        if (_endNo == nMax)
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _startNo = nMax - (nMax % countShu) + 1 - countShu;
                            _endNo = nMax - (nMax % countShu);
                            return true;
                        }
                        else
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _startNo -= countShu;
                            _endNo -= countShu;
                            return true;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        pageCurrent++;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _startNo += countShu;
                        _endNo += countShu;
                        return true;
                    }
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = 0;
                        _startNo = 1;
                        _endNo = countShu;
                        return true;
                    }
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return false;
                    }
                    else
                    {
                        pageCurrent = pageCount;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _startNo = nMax - (nMax % countShu) + 1;
                        _endNo = nMax;
                        return true;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "bdnInfo_ItemClicked");
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// �����ҳ�ؼ��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                bool isOn = bdnInfo_ItemClicked(sender, e); //���ز���,���false,˵�����˵�һҳ�����һҳ,�в������ý���
                if (isOn)
                {
                    isShowPro(true);
                    DataTable dt = LoadData(_startNo, _endNo, strWhere, false);
                    dataGV.DataSource = dt;
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    for (int i = 1; i < dataGV.Rows.Count; i += 2)
                    {
                        dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    createPoint(dt);
                    this.toolPro.Value = 2;
                    //Application.DoEvents();

                    #region ���ݵ���
                    LoadData(_startNo, _endNo, excelSql, true);
                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "bindingNavigator1_ItemClicked");
            }
        }

        /// <summary>
        /// ������־���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">����</param>
        private void writeToPopuLog(Exception ex, string sFunc)
        {
            //StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            //sw.WriteLine("�˿ڹ���:�� " + sFunc + " ������," + DateTime.Now.ToString() + ": ");
            //sw.WriteLine(ex.ToString());
            //sw.WriteLine();
            //sw.Close();
            CLC.BugRelated.ExceptionWrite(ex, "clPopu-ucPopu-" + sFunc);
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="sql">����SQL</param>
        /// <param name="method">����ģ��</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�˿ڹ���','�˿�ϵͳ:" + sql.Replace('\'', '"') + "','" + method + "')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(strExe);
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "WriteEditLog");
            }
        }

        /// <summary>
        /// ּ����Ʒ�ҳת��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ltextNow">��ҳ�ӿؼ���ʾ��ǰҳ��</param>
        private void PageNow_KeyPress(ToolStripTextBox ltextNow)
        {
            try
            {
                if (Convert.ToInt32(ltextNow.Text) < 1 || Convert.ToInt32(ltextNow.Text) > pageCount)
                {
                    System.Windows.Forms.MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                    ltextNow.Text = pageCurrent.ToString();
                    return;
                }
                else
                {
                    this.pageCurrent = Convert.ToInt32(ltextNow.Text);
                    nCurrent = pageSize * (pageCurrent - 1);
                    _startNo = ((pageCurrent - 1) * pageSize) + 1;
                    _endNo = _startNo + pageSize - 1;
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "PageNow_KeyPress");
            }
        }

        /// <summary>
        /// ּ����Ʒ�ҳת��(����ҳ��ֱ����ת����ҳ)
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void PageNow1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    isShowPro(true);
                    PageNow_KeyPress(PageNow1);
                    DataTable dt = LoadData(_startNo, _endNo, strWhere,false);
                    dataGV.DataSource = dt;
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    for (int i = 1; i < dataGV.Rows.Count; i += 2)
                    {
                        dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    createPoint(dt);
                    this.toolPro.Value = 2;
                    //Application.DoEvents();

                    #region ���ݵ���
                    LoadData(_startNo, _endNo, excelSql, true);
                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "PageNow1_KeyPress");
                isShowPro(false);
            }
        }

        /// <summary>
        /// ּ��ʵ��ÿҳ��ʾ��������Ŀ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ltextNum">��ҳ�ӿؼ���ʾ��ǰÿҳ��ʾ��Ŀ</param>
        /// <param name="datagridview">������ʾ�ؼ�</param>
        private void TextNum_KeyPress(ToolStripTextBox ltextNum, DataGridView datagridview)
        {
            try
            {
                if (datagridview.Rows.Count > 0)
                {
                    this.pageSize = Convert.ToInt32(ltextNum.Text);
                    this.pageCurrent = 1;
                    nCurrent = pageSize * (pageCurrent - 1);
                    pageCount = (nMax / pageSize);//�������ҳ��
                    if ((nMax % pageSize) > 0) pageCount++;
                    _endNo = pageSize;
                    _startNo = 1;

                    #region ���ݵ���
                    LoadData(_startNo, _endNo, excelSql,true);
                    //Excel�����ף���ռ����Դ���󣬴����õĽ������ʵ�ֵ���
                    //DataTable datatableExcel = LoadData(_startNo, _endNo, excelSql,true);
                    //if (dtExcel != null) dtExcel.Clear();
                    //dtExcel = datatableExcel;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "TextNum_KeyPress");
                ltextNum.Text = pageSize.ToString();
            }
        }

        /// <summary>
        /// ּ��ʵ��ÿҳ��ʾ��������Ŀ(����ÿҳ��ʾ��ֱ�Ӹ�����ʾ��Ŀ)
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGV.Rows.Count > 0)
                {
                    isShowPro(true);
                    TextNum_KeyPress(TextNum1, dataGV);
                    DataTable dt = LoadData(_startNo, _endNo, strWhere,false);
                    this.toolPro.Value = 1;
                    Application.DoEvents();
                    dataGV.DataSource = dt;

                    this.toolPro.Value = 2;
                    //Application.DoEvents();
                    for (int i = 1; i < dataGV.Rows.Count; i += 2)
                    {
                        dataGV.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    createPoint(dt);
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "TextNum_KeyPress");
                isShowPro(false);
            }
        }

        string P_arrType = "";
        /// <summary>
        /// �����˿��е�FieldStr�ֶ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void P_setfield()
        {
            try
            {
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '�˿�ϵͳ'";
                DataTable dt = new DataTable();
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sExp);

                FieldStr.Items.Clear();
                P_arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string aCol = dt.Rows[i][0].ToString();
                    string aType = dt.Rows[i][1].ToString();
                    if (aCol != "" && aCol != "MAPID" && aCol.IndexOf("�����ֶ�") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("����") < 0)
                    {
                        FieldStr.Items.Add(aCol);
                        P_arrType += aType + ",";
                    }
                }
                FieldStr.Text = "���֤����";
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "P_setfield");
                System.Windows.Forms.MessageBox.Show(ex.Message, "P_setfield()");
            }
        }

        /// <summary>
        /// �л��ֶ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void FieldStr_SelectedIndexChanged(object sender, EventArgs e)
        {
            setYunsuanfuValue(FieldStr.SelectedIndex);
        }

        /// <summary>
        /// �����ֶ����������Ӧ�õıȽϷ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="p">�ֶ���</param>
        private void setYunsuanfuValue(int p)
        {
            try
            {
                string[] arr = P_arrType.Split(',');
                string type = arr[p].ToUpper();
                if (type == "DATE")
                {
                    dateTimePicker1.Visible = true;
                    this.ValueStr.Visible = false;
                }
                else
                {
                    dateTimePicker1.Visible = false;
                    this.ValueStr.Visible = true;
                }
                //  if(type=="VARCHAR2"||type=="NVARCHAR2")
                this.MathStr.Items.Clear();

                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        this.MathStr.Items.Add("����");
                        this.MathStr.Items.Add("������");
                        this.MathStr.Items.Add("����");
                        this.MathStr.Items.Add("���ڵ���");
                        this.MathStr.Items.Add("С��");
                        this.MathStr.Items.Add("С�ڵ���");
                        break;
                    case "CHAR":
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        this.MathStr.Items.Add("����");
                        this.MathStr.Items.Add("������");
                        this.MathStr.Items.Add("����");
                        break;
                }
                this.MathStr.Text = "����";
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "setYunsuanfuValue");
                System.Windows.Forms.MessageBox.Show(ex.Message, "setYunsuanfuValue()");
            }
        }

        /// <summary>
        /// �������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValueStr.Visible && ValueStr.Text.Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show("��ѯֵ����Ϊ�գ�", "��ʾ");
                    return;
                }

                if (this.ValueStr.Text.IndexOf("\'") > -1)
                {
                    System.Windows.Forms.MessageBox.Show("������ַ����в��ܰ���������!", "��ʾ");
                    return;
                }

                string strExp = "";
                int p = FieldStr.SelectedIndex;
                string[] arr = P_arrType.Split(',');
                string type = arr[p].ToUpper();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   " + this.ValueStr.Text.Trim();
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   " + ValueStr.Text.Trim();
                        }
                        this.dataGridExp.Rows.Add(new object[] { strExp, "����" });
                        break;
                    case "DATE":
                        string tValue = this.dateTimePicker1.Value.ToString();
                        if (tValue == "")
                        {
                            System.Windows.Forms.MessageBox.Show("��ѯֵ����Ϊ�գ�", "��ʾ");
                            return;
                        }

                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + tValue + "'";
                            this.dataGridExp.Rows.Add(new object[] { strExp, "ʱ��" });
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + tValue + "'";
                            this.dataGridExp.Rows.Add(new object[] { strExp, "ʱ��" });
                        }
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + ValueStr.Text.Trim() + "'";
                            if (this.MathStr.Text.Trim() == "����")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "�ַ���" });
                            }
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + ValueStr.Text.Trim() + "'";
                            if (this.MathStr.Text.Trim() == "����")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "�ַ���" });
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "btnAdd_Click");
                System.Windows.Forms.MessageBox.Show(ex.Message, "btnAdd_Click()");
            }
        }

        /// <summary>
        /// ɾ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridExp.Rows.Count != 0)
                {
                    if (this.dataGridExp.CurrentRow.Index != 0)
                    {
                        this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                    }
                    else
                    {
                        if (this.dataGridExp.Rows.Count > 1)
                        {
                            this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                            string text = this.dataGridExp.Rows[0].Cells["Value"].Value.ToString().Replace("����", "");

                            text = text.Replace("����", "").Trim();
                            this.dataGridExp.Rows[0].Cells["Value"].Value = text;
                        }
                        else
                        {
                            this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "btnDelete_Click");
                System.Windows.Forms.MessageBox.Show(ex.Message, "btnDelete_Click");
            }
        }

        /// <summary>
        /// �����ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                this.FieldStr.Text = this.FieldStr.Items[0].ToString();
                this.dataGridExp.Rows.Clear();
                this.ValueStr.Text = "";
                this.clearTem();
                this.dataHouse = null;
            }
            catch (Exception ex)
            {
                this.writeToPopuLog(ex, "buttonClear_Click-���ò�ѯ����!");
            }
        }

        /// <summary>
        /// ת���ַ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <returns>�ɹ���ѯ��sql</returns>
        private string getSqlString()
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";

                for (int i = 0; i < this.dataGridExp.Rows.Count; i++)
                {
                    string type = this.dataGridExp.Rows[i].Cells["Type"].Value.ToString();
                    string str = this.dataGridExp.Rows[i].Cells["Value"].Value.ToString();
                    if (type == "����")
                    {
                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {
                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                str += " '%" + strArray[1] + "%'";
                            }
                        }
                        array.Add(str);
                    }
                    else if (type == "ʱ��")
                    {

                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {

                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                //strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   to_date('" + tValue + "', 'YYYY-MM-DD HH24:MI:SS')";
                                str += "to_date('" + strArray[1] + "', 'YYYY-MM-DD HH24:MI:SS')";
                            }
                        }
                        array.Add(str);
                    }
                    else
                    {
                        array.Add(str);
                    }
                }

                for (int j = 0; j < array.Count; j++)
                {
                    if (j == 0)
                    {
                        getsql = array[j].ToString();
                    }
                    else
                    {
                        getsql += " " + array[j].ToString();
                    }
                }

                getsql = getsql.Replace("����", "and");
                getsql = getsql.Replace("����", "or");
                getsql = getsql.Replace("����", "like");
                getsql = getsql.Replace("���ڵ���", ">=");
                getsql = getsql.Replace("С�ڵ���", "<=");
                getsql = getsql.Replace("����", ">");
                getsql = getsql.Replace("С��", "<");
                getsql = getsql.Replace("������", "!=");
                getsql = getsql.Replace("����", "=");

                return getsql;
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "getSqlString");
                return "";
            }
        }

        /* �������Զ���ȫ���� */

        /// <summary>
        /// �Զ���ȫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="keyword">�ı����������ֵ</param>
        /// <param name="colword">����</param>
        /// <param name="tableName">����</param>
        /// <param name="listBox1">��ʾ�Զ���ȫֵ�Ŀؼ�</param>
        /// <returns>������</returns>
        private DataTable getListBox(string keyword, string colword, string tableName)
        {
            try
            {
                DataTable dt = null;
                string strExp = "";
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\GetFromNameConfig.ini");
                string tabName = CLC.ForSDGA.GetFromTable.TableName;
                string match = CLC.INIClass.IniReadValue(tableName, "MatchField");

                if (keyword != "" && colword != "")
                {
                    if (match.IndexOf(colword) >= 0)
                    {
                        #region ����SQL���
                        switch (colword)
                        {
                            case "��������":
                                strExp = "select �������� from �������� t where ��������  like '" + keyword + "%' union select �������� from �������� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from �������� t where ��������  like '%" + keyword + "'";
                                break;
                            case "����_����":
                                strExp = "select ����2 from ���۰��� t where ����2  like '" + keyword + "%' union select ����2 from ���۰��� t where ����2 like '%" + keyword + "%' and ����2 not like '" + keyword + "%' and ����2 not like '%" + keyword + "' union select ����2 from ���۰��� t where ����2  like '%" + keyword + "'";
                                break;
                            case "�˿�����":
                                strExp = "select �������� from �˿����� t where ��������  like '" + keyword + "%' union select �������� from �˿����� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from �˿����� t where ��������  like '%" + keyword + "'";
                                break;
                            case "�Ա�":
                                strExp = "select ���� from �Ա� t where ����  like '" + keyword + "%' union select ���� from �Ա� t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from �Ա� t where ����  like '%" + keyword + "'";
                                break;
                            case "����":
                                strExp = "select ���� from ���� t where ����  like '" + keyword + "%' union select ���� from ���� t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from ���� t where ����  like '%" + keyword + "'";
                                break;
                            case "����״̬":
                                strExp = "select ���� from ����״�� t where ����  like '" + keyword + "%' union select ���� from ����״�� t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from ����״�� t where ����  like '%" + keyword + "'";
                                break;
                            case "������ò":
                                strExp = "select ���� from ������ò t where ����  like '" + keyword + "%' union select ���� from ������ò t where ���� like '%" + keyword + "%' and ���� not like '" + keyword + "%' and ���� not like '%" + keyword + "' union select ���� from ������ò t where ����  like '%" + keyword + "'";
                                break;
                            case "��������":
                                strExp = "select �������� from �������� t where ��������  like '" + keyword + "%' union select �������� from �������� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from �������� t where ��������  like '%" + keyword + "'";
                                break;
                            case "�����ɳ���":
                                strExp = "select �ɳ����� from �����ɳ��� t where �ɳ�����  like '" + keyword + "%' union select �ɳ����� from �����ɳ��� t where �ɳ����� like '%" + keyword + "%' and �ɳ����� not like '" + keyword + "%' and �ɳ����� not like '%" + keyword + "' union select �ɳ����� from �����ɳ��� t where �ɳ�����  like '%" + keyword + "'";
                                break;
                            case "�����ж�":
                                strExp = "select �ж��� from �������ж� t where �ж���  like '" + keyword + "%' union select �ж��� from �������ж� t where �ж��� like '%" + keyword + "%' and �ж��� not like '" + keyword + "%' and �ж��� not like '%" + keyword + "' union select �ж��� from �������ж� t where �ж���  like '%" + keyword + "'";
                                break;
                            case "����������":
                                strExp = "select �������� from ���������� t where ��������  like '" + keyword + "%' union select �������� from ���������� t where �������� like '%" + keyword + "%' and �������� not like '" + keyword + "%' and �������� not like '%" + keyword + "' union select �������� from ���������� t where ��������  like '%" + keyword + "'";
                                break;
                        }
                        #endregion

                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                    }
                    else
                    {
                        dt = null;
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "getListBox");
                return null;
            }
        }

        /// <summary>
        /// ��Ϊ�̶�ֵʱ�Զ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="colName">����</param>
        /// <param name="tableName">����</param>
        /// <param name="listBox">��ʾ�Զ���ȫֵ�Ŀؼ�</param>
        /// <returns>������</returns>
        private DataTable MatchShu(string colName, string tableName)
        {
            try
            {
                DataTable dt = null;
                string strExp = "";
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                string tabName = CLC.ForSDGA.GetFromTable.TableName;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\GetFromNameConfig.ini");
                string match = CLC.INIClass.IniReadValue(tableName, "MatchField");
                if (colName != "" && tabName != "")
                {
                    if (match.IndexOf(colName) >= 0)
                    {
                        #region ����SQL���
                        switch (colName)
                        {
                            case "��������":
                                strExp = "select �������� from �������� t Group by ��������";
                                break;
                            case "����_����":
                                strExp = "select ����2 from ���۰��� t Group by ����2";
                                break;
                            case "�˿�����":
                                strExp = "select �������� from �˿����� t Group by ��������";
                                break;
                            case "�Ա�":
                                strExp = "select ���� from �Ա� t Group by ����";
                                break;
                            case "����":
                                strExp = "select ���� from ���� t Group by ����";
                                break;
                            case "����״̬":
                                strExp = "select ���� from ����״�� t Group by ����";
                                break;
                            case "������ò":
                                strExp = "select ���� from ������ò t Group by ����";
                                break;
                            case "��������":
                                strExp = "select �������� from �������� t Group by ��������";
                                break;
                            case "�����ɳ���":
                                strExp = "select �ɳ����� from �����ɳ��� t Group by �ɳ�����";
                                break;
                            case "�����ж�":
                                strExp = "select �ж��� from �������ж� t Group by �ж���";
                                break;
                            case "����������":
                                strExp = "select �������� from ���������� t Group by ��������";
                                break;
                        }
                        #endregion

                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                    }
                    else
                        dt = null;
                }
                return dt;
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "MatchShu");
                return null;
            }
        }

        /// <summary>
        /// �Զ���ȫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void ValueStr_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.ValueStr.Text.Trim(), this.FieldStr.Text, "�˿�ϵͳ");

                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "ValueStr_TextChanged_1");
            }
        }

        /// <summary>
        /// �Զ���ȫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void ValueStr_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.FieldStr.Text, "�˿�ϵͳ");

                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "ValueStr_Click_1");
            }
        }

        // 
        /// <summary>
        /// ��ʾ�����ؽ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="falg">����ֵ��true-��ʾ false-���أ�</param>
        private void isShowPro(bool falg)
        {
            try
            {
                this.toolPro.Value = 0;
                this.toolPro.Maximum = 3;
                this.toolProLbl.Visible = falg;
                this.toolProSep.Visible = falg;
                this.toolPro.Visible = falg;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "isShowPro");
            }
        }

        /// <summary>
        /// �������ػ���ʾ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;

                if (link.Text == "����������")
                {
                    this.ValueStr.Visible = false;
                    groupBox1.Visible = false;
                    link.Text = "��ʾ������";
                }
                else
                {
                    this.ValueStr.Visible = true;
                    groupBox1.Visible = true;
                    link.Text = "����������";
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "linkLabel1_LinkClicked");
            }
        }

        /// <summary>
        /// ���뷨ȫ�ǰ�Ǵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void ValueStr_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                IntPtr HIme = ImmGetContext(this.Handle);
                if (ImmGetOpenStatus(HIme)) //������뷨���ڴ�״̬
                {
                    int iMode = 0;
                    int iSentence = 0;
                    bool bSuccess = ImmGetConversionStatus(HIme, ref iMode, ref iSentence); //�������뷨��Ϣ
                    if (bSuccess)
                    {
                        if ((iMode & IME_CMODE_FULLSHAPE) > 0) //�����ȫ��
                            ImmSimulateHotKey(this.Handle, IME_CHOTKEY_SHAPE_TOGGLE); //ת���ɰ��
                    }
                }
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "ValueStr_MouseDown");
            }
        }

        private DataTable dataHouse;
        private frmDisplay fmDis;

        /// <summary>
        /// �Ŵ�鿴���ݰ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnEnalData_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataHouse == null)
                {
                    System.Windows.Forms.MessageBox.Show("������չʾ����ѡ��ѯ�����ݺ�Ŵ�鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new frmDisplay(dataHouse);

                fmDis.dataGridDisplay.CellClick += this.dataGV_CellClick;
                fmDis.dataGridDisplay.CellDoubleClick += this.dataGV_CellDoubleClick;

                fmDis.Show();
            }
            catch (Exception ex)
            {
                writeToPopuLog(ex, "btnEnlarge_Click");
            }
        }
    }
}