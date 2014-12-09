using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace clGPSPolice   
{
    public partial class FrmGuijiTime : Form
    {
        public MapControl MapControl1;       // ��ͼ�ؼ�
        private readonly string _mysqlstr;   // ���ݿ������ַ���
        private readonly string[] _conStr;   // ���ݿ����Ӳ���
        public double Lx;                    // �ϴεľ���
        public double Ly;                    // �ϴε�γ��
        
        private string _cnname = string.Empty ;   // ��������

        public string Userid = "";           // ���������û���
        public string Datasource = "";       // ������������Դ
        public string Password = "";         // ������������

        public string User = "";             // �����¼�û���

        private Boolean _jzflag;


        /// <summary>
        /// ��ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <param name="s">���ݿ����Ӳ���</param>
        public FrmGuijiTime(string[] s)
        {
            try
            {
                InitializeComponent();
                _conStr = s;

                _mysqlstr = "data source=" + _conStr[0] + ";user id=" + _conStr[1] + ";password=" + _conStr[2];

                AddCarName();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "01-��ʼ��");
            }
        }


        public void Getflag(Boolean jz)
        {
            _jzflag = jz;
        }

        /// <summary>
        /// ��ӳ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        public void AddCarName()
        {
            OracleConnection con = new OracleConnection(_mysqlstr);
            try
            {
                con.Open();
                OracleCommand cmdadd = new OracleCommand("select ������� from GPS��Ա order by �������", con);
                OracleDataReader dr = cmdadd.ExecuteReader();

                while (dr.Read())
                {
                    comboBox1.Items.Add(Convert.ToString(dr.GetString(0)));
                }
                dr.Close();
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "02-��Ӿ����������");
            }
            finally {
                con.Close();
            }
        }

        /// <summary>
        /// ��ʼ���Ź켣
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void Button1Click(object sender, EventArgs e)
        {
            try
            {
                if (txtplyspe.Text == "")
                {
                    MessageBox.Show(@"�����ٶȲ���Ϊ��");
                    return;
                }
                if (Convert.ToInt32(txtplyspe.Text) <= 0)
                {
                    MessageBox.Show(@"�����ٶȲ���С����");
                    return;
                }

                Int32 psp = Convert.ToInt32(txtplyspe.Text);

                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                //string cn = this.comboBox1.Text;

                string cn = GetCarName();

                if (cn != "")
                {
                    comboBox1.Text = cn;
                }

                if (dateTimePicker1.Text == "" || dateTimePicker2.Text == "")
                {
                    MessageBox.Show(@"��ʼʱ�䲻��Ϊ�գ�", @"ϵͳ��ʾ");
                    return;
                }

                if (dateTimePicker3.Text == "" || dateTimePicker4.Text == "")
                {
                    MessageBox.Show(@"����ʱ�䲻��Ϊ�գ�", @"ϵͳ��ʾ");
                    return;
                }

                if (cn == "")
                {
                    MessageBox.Show(@"��������Ϊ��ѡ�����Ϊ�գ�", @"ϵͳ��ʾ");
                    return;
                }


                if (dte < dts)
                {
                    MessageBox.Show(@"��ʼʱ�䲻�ܴ��ڻ������ֹʱ�䣡", @"ϵͳ��ʾ");
                    return;
                }

                _cnname = cn;

                bool df = CreateHistoryDate();
                if (df == false)
                {
                    return;
                }

                bool lf = CreateHistoryLayer();
                if (lf == false)
                {
                    return;
                }

                bool ff = CreateHisFtr();
                if (ff == false)
                {
                    return;
                }

                if (_hisx.Length == 1|| _hisy.Length ==1) return;

                //this.Visible = false;
                if (btncar1.Enabled)
                    btncar1.Enabled = false;
                if (btncar2.Enabled == false)
                    btncar2.Enabled = true;
                if (btncar3.Enabled)
                    btncar3.Enabled = false;
                if (btncar4.Enabled == false)
                    btncar4.Enabled = true;


                timer1.Interval = psp*1000;
                timer1.Enabled = true;

                WriteEditLog(cn, "�켣�ط�");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "03-��ʼ���Ź켣");
            }
        }
        
        /// <summary>
        /// ��ó�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <returns>��������</returns>
        private string GetCarName()
        {            
            OracleConnection con = new OracleConnection(_mysqlstr);
            string carname = "";
            try
            {
                string sqlstring = "select * from gps��Ա where ������� like '%" + comboBox1.Text.Trim() + "%' order by �������";
                con.Open();
                OracleCommand cmd = new OracleCommand(sqlstring, con);
                cmd.ExecuteNonQuery();
                OracleDataAdapter apt = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);
                if (dt.Rows.Count == 1)
                    foreach (DataRow dr in dt.Rows)
                        carname = Convert.ToString(dr["�������"]);
                else
                    MessageBox.Show(@"û���ҵ���ؼ���Ψһƥ��ľ�Ա��Ϣ", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "04-��ȡ��Ա����");              
            }
            return carname;
        }

        double[] _hisx;   // ��ʷ�켣��xֵ
        double[] _hisy;   // ��ʷ�켣��Yֵ
        string[] _hisdate;// ��ʷ�켣��ʱ��ֵ
        string[] _hisspe; // ��ʷ�켣���ٶ�ֵ
        string[] _hisver; // ��ʷ�켣�ķ���ֵ 

        /// <summary>
        /// ������ʷ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <returns>����ֵ(true-�����ɹ� false-����ʧ��)</returns>
        public Boolean  CreateHistoryDate()
        {            
            OracleConnection con = new OracleConnection(_mysqlstr);
            try
            {
                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                if (dte > dts)
                {

                    string sqlstring = "select ���������.�������,GPS110.����GPSλ��.X,GPS110.����GPSλ��.Y, GPS110.����GPSλ��.��λ����ʱ�� from GPS110.����GPSλ��,��������� where  GPS110.����GPSλ��.��λ����ʱ��> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and  GPS110.����GPSλ��.��λ����ʱ��< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and GPS110.����GPSλ��.�Խ���ID=���������.�豸��� and ���������.������� = '" + _cnname + "' order by  GPS110.����GPSλ��.��λ����ʱ��";
                    con.Open();
                    OracleCommand cmdsel = new OracleCommand(sqlstring, con);
                    cmdsel.ExecuteNonQuery();

                    OracleDataAdapter aptsel = new OracleDataAdapter(cmdsel);
                    DataTable dtsel = new DataTable();
                    aptsel.Fill(dtsel);

                    if (dtsel.Rows.Count > 0)
                    {
                        _hisx = new double[dtsel.Rows.Count];
                        _hisy = new double[dtsel.Rows.Count];
                        _hisdate = new string[dtsel.Rows.Count];
                        _hisspe = new string[dtsel.Rows.Count];
                        _hisver = new string[dtsel.Rows.Count];

                        int i = 0;
                        foreach (DataRow dr in dtsel.Rows)
                        {
                            //��ȡ��γ��                                                    
                            _hisx[i] = Convert.ToDouble(dr["X"]);
                            _hisy[i] = Convert.ToDouble(dr["Y"]);
                            _hisdate[i] = Convert.ToString(dr["��λ����ʱ��"]);
                          
                            i = i + 1;
                        }

                        return true;
                    }
                    else
                    {
                        MessageBox.Show(@"��ǰʱ������޸þ�Ա����ʷ�켣����,������ѡ��", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;

                    }
                }
                else
                {
                    MessageBox.Show(@"ѡ���ʱ��ν���ʱ�䲻���ڿ�ʼʱ��,���޷���ʾ,������ѡ��", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            catch(Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                ExToLog(ex, "05-������ʷ����");
                return false;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();                
            }
        }


        /// <summary>
        /// ��������ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <returns>����ֵ(true-�����ɹ� false-����ʧ��)</returns>
        public Boolean  CreateHistoryLayer()
        {
            try
            {               

                Catalog cat = Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("PoliceGuijiLayer");
                Table tblTemp = cat.GetTable("PoliceGuijiLayer");
                if (tblTemp != null) //Table exists close it
                {
                    cat.CloseTable("PoliceGuijiLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(MapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                MapControl1.Map.Layers.Add(lyr);

                return true;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "06-������Աͼ��");
                return false;
            }
        }


        private int _hi; 

        /// <summary>
        /// ������ʷ�켣�ĵ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <returns>����ֵ(true-�����ɹ� false-����ʧ��)</returns>
        public Boolean  CreateHisFtr()
        {
            try
            {
                if (_hisx.Length > 0 && _hisy.Length > 0 && (_hisx.Length == _hisy.Length))
                {
                    double xx = 0;
                    double yy = 0;

                    _hi = 0;

                    xx = _hisx[_hi];
                    yy = _hisy[_hi];

                    //Road = GetNearestRoad(xx, yy);

                    Lx = xx;
                    Ly = yy;

                    FeatureLayer lyrcar = MapControl1.Map.Layers["PoliceGuijiLayer"] as FeatureLayer;
                    Table tblcar = Session.Current.Catalog.GetTable("PoliceGuijiLayer");

                    if (lyrcar != null)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xx, yy));
                        //CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(43,System.Drawing.Color.Red, 20));

                        //CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("POLI1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Blue , 16));
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("jc.bmp", BitmapStyles.ApplyColor, Color.Red, 20));
                    
                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = _cnname;
                        tblcar.InsertFeature(ftr);
                        MapControl1.Map.Center = ftr.Geometry.Centroid;
                    }
                    //MapControl1.Map.Zoom = new Distance(2500, DistanceUnit.Meter);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "07-������ʷ�켣�ĵ�");
                return false;
            }            
        }

        /// <summary>
        /// �ƶ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <param name="x">��ǰX����</param>
        /// <param name="y">��ǰY����</param>
        /// <param name="lx">Ҫ�ƶ���X����</param>
        /// <param name="ly">Ҫ�ƶ���Y����</param>
        /// <param name="cnum">�������</param>
        public void MoveHisFtr(double x, double y, double lx, double ly, string cnum)
        {
            try
            {
                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("PoliceGuijiLayer");

                MapInfo.Mapping.Map map = MapControl1.Map;

                tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);

                foreach (Feature fcar in tbl)
                {
                    if (fcar["Name"].ToString() == cnum)
                    {
                        fcar.Geometry.GetGeometryEditor().OffsetByXY(x - lx, y - ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                        MapInfo.Geometry.DPoint dpoint;
                        MapInfo.Geometry.Point point;

                        dpoint = new DPoint(x, y);
                        point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                        fcar.Geometry = point;
                        TrackHistoryline(x, y, lx, ly);
                        Boolean inf = this.IsInBounds(x, y);
                        MapControl1.Map.Center = dpoint;
                    }

                    fcar.Geometry.EditingComplete();
                    fcar.Update();

                    MapControl1.Refresh();

                    tbl.EndAccess();
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "08-�ƶ�����");
            }
        }

        /// <summary>
        /// ����ر�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void frmGuijiTime_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.Visible = false;
                e.Cancel = true;
                this.Left = Screen.PrimaryScreen.WorkingArea.Left;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "09-����ر�");
            }
        }

        /// <summary>
        /// ������ʷ�켣�� 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <param name="x1">��ǰX����</param>
        /// <param name="y1">��ǰY����</param>
        /// <param name="x2">Ҫ�ƶ���X����</param>
        /// <param name="y2">Ҫ�ƶ���Y����</param>
        public void TrackHistoryline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = MapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["Track"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("Track");

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(2, MapInfo.Styles.LineWidthUnit.Pixel), 2, System.Drawing.Color.Blue);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "10-������ʷ�켣��");
            }
        }

        /// <summary>
        /// ʹ��Ա�ƶ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                FeatureLayer lyr = MapControl1.Map.Layers["PoliceGuijiLayer"] as FeatureLayer;

                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("PoliceGuijiLayer");

                MapInfo.Mapping.Map map = MapControl1.Map;    

                MapInfo.Data.IResultSetFeatureCollection rsfc = session.Selections.DefaultSelection[lyr.Table];
              
                if (tbl != null)
                {
                    OracleConnection con = new OracleConnection(_mysqlstr);
                    try
                    {
                        if (_hi < _hisx.Length)
                        {
                            _hi = _hi + 1;

                            double x = 0;
                            double y = 0;

                            x = _hisx[_hi];
                            y = _hisy[_hi];
                            
                            string dt = _hisdate[_hi];

                            tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);

                            foreach (Feature fcar in tbl)
                            {
                                if (fcar["Name"].ToString() == _cnname)
                                {
                                    fcar.Geometry.GetGeometryEditor().OffsetByXY(x - Lx, y - Ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                                    MapInfo.Geometry.DPoint dpoint;
                                    MapInfo.Geometry.Point point;

                                    dpoint = new DPoint(x, y);
                                    point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                                    fcar.Geometry = point;
                                    this.txtdate.Text = _hisdate[_hi];
                                    this.txtspe.Text = _hisspe[_hi];
                                    this.txtver.Text = _hisver[_hi];
                               
                                    if (_jzflag == true)
                                    {
                                        this.MapControl1.Map.Center = dpoint;
                                    }

                                    Boolean inf = this.IsInBounds(x, y);
                                    if (inf == false)
                                    {
                                        MapControl1.Map.Center = dpoint;
                                    }
                                    TrackHistoryline(x, y, Lx, Ly);

                                    fcar.Geometry.EditingComplete();
                                    fcar.Update();

                                    MapControl1.Refresh();

                                    tbl.EndAccess();

                                    Lx = x;
                                    Ly = y;

                                    break;
                                }
                            }
                        }
                        else
                        {
                            this.btncar1.Enabled = true;
                            this.btncar2.Enabled = false;
                            this.btncar3.Enabled = false;
                            this.btncar4.Enabled = false;
                            timer1.Enabled = false;
                        }
                    }
                    catch { timer1.Enabled = false; }
                }
            }
            catch(Exception ex)
            {
                timer1.Enabled = false;
                ExToLog(ex, "11-timer1_trick");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                
               // this.Visible = false;
            }
            catch { }
        }

       /// <summary>
        /// �жϳ����Ƿ�����ͼ��Χ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
       /// </summary>
       /// <param name="x">X����</param>
       /// <param name="y">Y����</param>
       /// <returns>����ֵ(true-�� false-��)</returns>
        private Boolean IsInBounds(double x, double y)
        {
            try
            {
                if (MapControl1.Map.Bounds.x1 < x && x < MapControl1.Map.Bounds.x2 && MapControl1.Map.Bounds.y1 < y && y < MapControl1.Map.Bounds.y2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "12-�жϾ�Ա�Ƿ�����ͼ��Χ");
                return false; 
            }
        }

        /// <summary>
        /// ��ͣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void btncar2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btncar1.Enabled == true)
                    this.btncar1.Enabled = false;

                if (this.btncar4.Enabled == false)
                    this.btncar4.Enabled = true;

                if (this.btncar2.Enabled == true)
                    this.btncar2.Enabled = false;

                if (this.btncar3.Enabled == false)
                    this.btncar3.Enabled = true;

                if (this.timer1.Enabled == true)
                    timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "13-��ͣ");
            }
        }

        /// <summary>
        /// ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void btncar3_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btncar1.Enabled == true)
                    this.btncar1.Enabled = false;
                if (this.btncar4.Enabled == false)
                    this.btncar4.Enabled = true;
                if (this.btncar3.Enabled == true)
                    this.btncar3.Enabled = false;
                if (this.btncar2.Enabled == false)
                    this.btncar2.Enabled = true;
                if (this.timer1.Enabled == false)
                    timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "14-����");
            }
        }

        /// <summary>
        /// ֹͣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void btncar4_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btncar4.Enabled == true)
                    this.btncar4.Enabled = false;
                if (this.btncar2.Enabled == true)
                    this.btncar2.Enabled = false;
                if (this.btncar3.Enabled == true)
                    this.btncar3.Enabled = false;
                if (this.btncar1.Enabled == false)
                    this.btncar1.Enabled = true;
                if (this.timer1.Enabled == true)
                    timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "15-ֹͣ");
            }
        }
        

        /// <summary>
        /// ����LOAD
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void frmGuijiTime_Load(object sender, EventArgs e)
        {
            try
            {
                this.Left = Screen.PrimaryScreen.WorkingArea.Left;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "16-����LOAD");
            }
        }

        /// <summary>
        /// ѡ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.comboBox1.Focus();
                this.comboBox1.Select(this.comboBox1.Text.Length + 1, 0);

                GetCarName();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "17-ѡ����");
            }
        }

        /// <summary>
        /// ��ѯSQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ִ��SQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(_conStr[0], _conStr[1], _conStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clGPSPolice-frmGuijiTime-"+sFunc);
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-10
        /// </summary>
        /// <param name="sql">��ǰ����SQL</param>
        /// <param name="method">������ʽ</param>
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + User + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'��Ա���','GPS��Ա��λϵͳ:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch { }
        }
    }
}