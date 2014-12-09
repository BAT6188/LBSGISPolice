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

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace clCar
{
    public partial class frmGuijiTime : Form
    {
        private MapControl mapControl1 = null;
        static string[] ConStr;
        static string mysqlstr;


        public double lx = 0; //�ϴεľ���
        public double ly = 0; // �ϴε�γ��
        
        private string cnname = string.Empty ;

        public string userid = "";
        public string datasource = "";
        public string password = "";

        public string user = "";

        private Boolean jzflag = false ;


        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="s">���ݿ������ַ���</param>
        public frmGuijiTime(MapControl mc,string[] s)
        {
            try
            {
                InitializeComponent();

                mapControl1 = mc;
                ConStr = s;
                mysqlstr = "data source=" + ConStr[0] + ";user id=" + ConStr[1] + ";password=" + ConStr[2];
                AddCarName();
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-01-��ʼ��");
            }
        }

        public void getflag(Boolean jz)
        {
            this.jzflag = jz;
        }

        /// <summary>
        /// ��ӳ�������
        /// </summary>
        public void AddCarName()
        {
            OracleConnection con = new OracleConnection(mysqlstr);
            try
            {
                con.Open();
                OracleCommand cmdadd = new OracleCommand("select �ն˳������� from GPS������λϵͳ order by �ն˳�������", con);
                OracleDataReader dr = cmdadd.ExecuteReader();             
                while (dr.Read())
                {
                    this.comboBox1.Items.Add(Convert.ToString(dr.GetString(0)));
                }
                dr.Close();
            }
            catch(Exception ex) 
            {
                writeToLog(ex, "frmGuijiTime-01-��ӳ�������");
            }
            finally {
                con.Close();
            }
        }

        /// <summary>
        /// ��ʼ���Ź켣
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.txtplyspe.Text == "")
                {
                    MessageBox.Show("�����ٶȲ���Ϊ��");
                    return;
                }
                if (Convert.ToInt32(this.txtplyspe.Text) <= 0 )
                {
                    MessageBox.Show("�����ٶȲ���С����");
                    return;
                }

                Int32 psp = Convert.ToInt32(this.txtplyspe.Text);

                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                //string cn = this.comboBox1.Text;

                string cn = GetCarName();

                if (cn != "")
                {
                    this.comboBox1.Text = cn;
                }                

                if (dateTimePicker1.Text == "" || dateTimePicker2.Text == "")
                {
                    MessageBox.Show("��ʼʱ�䲻��Ϊ�գ�", "ϵͳ��ʾ");
                    return ;
                }
                    
                if (dateTimePicker3.Text == "" || dateTimePicker4.Text == "")
                {
                    MessageBox.Show("����ʱ�䲻��Ϊ�գ�", "ϵͳ��ʾ");
                    return ;
                }
         
                if (cn == "")
                {
                    MessageBox.Show("��������Ϊ��ѡ�����Ϊ�գ�", "ϵͳ��ʾ");
                    return ;
                }
         
                
                if (dte < dts)
                {
                    MessageBox.Show("��ʼʱ�䲻�ܴ��ڻ������ֹʱ�䣡", "ϵͳ��ʾ");
                    return;
                }

                cnname = cn;
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                bool df =  CreateHistoryDate();
                if (df == false)
                {
                    return;
                }
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                bool lf = CreateHistoryLayer();
                if (lf == false)
                {
                    return;
                }
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                bool ff = CreateHisFtr();
                if (ff == false)
                {
                    return;
                }
                Console.WriteLine(this.mapControl1.Map.Layers.Count.ToString());
                //this.Visible = false;
                if(this.btncar1.Enabled == true)
                this.btncar1.Enabled = false;
                if(this.btncar2.Enabled == false )
                this.btncar2.Enabled = true;
                if(this.btncar3.Enabled == true)
                this.btncar3.Enabled = false ;
                if(this.btncar4.Enabled == false )
                this.btncar4.Enabled = true;
                timer1.Interval = psp*1000;
                timer1.Enabled = true;

                WriteEditLog(cn, "�켣�ط�");
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-03-��ʼ���Ź켣");
            }
        }

        /// <summary>
        /// ��ó�������
        /// </summary>
        /// <returns></returns>
        private string GetCarName()
        {
            string carname = "";
            try
            {
                string sqlstring = "select * from GPS������λϵͳ where �ն˳������� like '%" + this.comboBox1.Text.Trim() + "%' order by �ն˳�������";
                DataTable dt =GetTable (sqlstring);
                if (dt.Rows.Count == 1)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        carname = Convert.ToString(dr["�ն˳�������"]);
                    }
                }
                else
                {
                    MessageBox.Show("û���ҵ���ؼ���ƥ��ĳ�����Ϣ","ϵͳ��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }             
                return carname;
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-04-��ȡ��������");
                return carname;
            }
        }

        double[] hisx;  //��ʷ�켣��xֵ
        double[] hisy;  //��ʷ�켣��Yֵ
        string[] hisdate;//��ʷ�켣��ʱ��ֵ
        string[] hisspe; //��ʷ�켣���ٶ�ֵ
        string[] hisver; //��ʷ�켣�ķ���ֵ

        /// <summary>
        /// ������ʷ����
        /// </summary>
        /// <returns></returns>
        public Boolean  CreateHistoryDate()
        {
            //ɾ����ʱ�������
            try
            {
                DateTime dts = Convert.ToDateTime(dateTimePicker1.Text + " " + dateTimePicker3.Text);
                DateTime dte = Convert.ToDateTime(dateTimePicker2.Text + " " + dateTimePicker4.Text);

                if (dte > dts)
                {

                    string sqlstring = "select * from GPSCAR where ʱ��> to_date('" + dts.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ʱ��< to_date('" + dte.ToString() + "','yyyy-mm-dd hh24:mi:ss') and �����ƺ� = '" + cnname + "' order by ʱ��";
                    DataTable dtsel = GetTable(sqlstring);

                    if (dtsel.Rows.Count > 0)
                    {
                        hisx = new double[dtsel.Rows.Count];
                        hisy = new double[dtsel.Rows.Count];
                        hisdate = new string[dtsel.Rows.Count];
                        hisspe = new string[dtsel.Rows.Count];
                        hisver = new string[dtsel.Rows.Count];

                        int i = 0;
                        foreach (DataRow dr in dtsel.Rows)
                        {
                            //��ȡ��γ��                                                    
                            hisx[i] = Convert.ToDouble(dr["����"]);
                            hisy[i] = Convert.ToDouble(dr["γ��"]);
                            hisdate[i] = Convert.ToString(dr["ʱ��"]);
                            hisspe[i] = Convert.ToString(dr["�ٶ�"]);
                            hisver[i] = Convert.ToString(dr["����"]);
                            i = i + 1;
                        }

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("��ǰʱ������޸ó�������ʷ�켣����,������ѡ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;

                    }
                }
                else
                {
                    MessageBox.Show("ѡ���ʱ��ν���ʱ�䲻���ڿ�ʼʱ��,���޷���ʾ,������ѡ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-05-������ʷ����");
                return false;
            }
        }


        /// <summary>
        /// ��������ͼ��
        /// </summary>
        /// <returns></returns>
 
        public Boolean  CreateHistoryLayer()
        {
            try
            {

                if (mapControl1.Map.Layers["CarGuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarGuijiLayer");
                }

                if (mapControl1.Map.Layers["CarGuijiLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarGuijiLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarGuijiLayer");
                Table tblTemp = Cat.GetTable("CarGuijiLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarGuijiLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);

          

                //��ӱ�ע
                string activeMapLabel = "CarGuijiLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarGuijiLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                //lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                return true;
            }
            catch(Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-06-��������ͼ��");
                return false;
            }
        }


        private int hi;
        private string Road = string.Empty;
        /// <summary>
        /// ������ʷ�켣�ĵ�
        /// </summary>
        /// <returns></returns>

        public Boolean  CreateHisFtr()
        {
            try
            {
                if (hisx.Length > 0 && hisy.Length > 0 && (hisx.Length == hisy.Length))
                {
                    double xx = 0;
                    double yy = 0;

                    hi = 0;

                    xx = hisx[hi];
                    yy = hisy[hi];

                    //Road = GetNearestRoad(xx, yy);

                    lx = xx;
                    ly = yy;

                    MapInfo.Mapping.Map map = mapControl1.Map;;

                    MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["CarGuijiLayer"] as FeatureLayer;

                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarGuijiLayer");

                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xx, yy)) as FeatureGeometry;
                    //CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(43,System.Drawing.Color.Red, 20));

                    //CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("POLI1-32.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Blue , 16));
                    CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("POLI1-32.bmp", BitmapStyles.None, System.Drawing.Color.Blue, 20));
                    
                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                    ftr.Geometry = pt;
                    ftr.Style = cs;
                    ftr["Name"] = cnname;                    
                    tblcar.InsertFeature(ftr);



                    Feature ftr1 = new Feature(tblcar.TableInfo.Columns);
                    ftr1.Geometry = pt;
                    ftr1.Style = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Blue, 10));
                    string cardate = hisdate[hi];
                    int hii = hisdate[hi].IndexOf('-');
                    int hjj = hisdate[hi].LastIndexOf(':');
                    ftr1["Name"] = cardate.Substring(hii+1, hjj - hii - 1);     
                    tblcar.InsertFeature(ftr1);
                    //mapControl1.Map.Center = ftr.Geometry.Centroid;
                    //mapControl1.Map.Zoom = new Distance(2500, DistanceUnit.Meter);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex) 
            {
                writeToLog(ex, "frmGuijiTime-07-������ʷ�켣�ĵ�");
                return false;
            }            
        }        

        /// <summary>
        /// ����ر�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-09-����ر�");
            }
        }


        /// <summary>
        /// ������ʷ�켣�� 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
     
        public void TrackHistoryline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["CarTrack"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("CarTrack");

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
                writeToLog(ex, "frmGuijiTime-10-������ʷ�켣��");
            }
        }

        /// <summary>
        /// timer1_trick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false) return;

                MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                FeatureLayer lyr = mapControl1.Map.Layers["CarGuijiLayer"] as FeatureLayer;

                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("CarGuijiLayer");

                MapInfo.Mapping.Map map = mapControl1.Map; 
                Feature fcar = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tbl, MapInfo.Data.SearchInfoFactory.SearchWhere("Name='" + this.cnname + "'"));

                if (fcar != null)
                {
                    try
                    {
                        if (hi < hisx.Length)
                        {
                            hi = hi + 1;
                        
                            double x = 0;
                            double y = 0;

                            x = hisx[hi];
                            y = hisy[hi];

                            string dt = hisdate[hi];

                            tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);


                            fcar.Geometry.GetGeometryEditor().OffsetByXY(x - lx, y - ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                            MapInfo.Geometry.DPoint dpoint;
                            MapInfo.Geometry.Point point;

                            dpoint = new DPoint(x, y);
                            point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                            fcar.Geometry = point;
                            this.txtdate.Text = hisdate[hi];
                            this.txtspe.Text = hisspe[hi];
                            this.txtver.Text = hisver[hi];

                            if (jzflag == true)
                            {
                                mapControl1.Map.Center = dpoint;
                            }

                            Boolean inf = this.IsInBounds(x, y);
                            if (inf == false)
                            {
                                mapControl1.Map.Center = dpoint;
                            }
                            TrackHistoryline(x, y, lx, ly);

                            fcar.Geometry.EditingComplete();
                            fcar.Update();



                            Feature ftr1 = new Feature(tbl.TableInfo.Columns);
                            ftr1.Geometry = point;
                            ftr1.Style = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Blue, 10));

                            string cardate = hisdate[hi];

                            int hii = hisdate[hi].IndexOf('-');
                            int hjj = hisdate[hi].LastIndexOf(':');

                            ftr1["Name"] = cardate.Substring(hii+1, hjj-hii-1);
                            tbl.InsertFeature(ftr1);

                            mapControl1.Refresh();

                            this.label8.Text = "��ǰ������: " + hi.ToString() + "/" + hisx.Length.ToString();


                            tbl.EndAccess();

                            Console.WriteLine(hi.ToString() + "   " + this.mapControl1.Map.Layers.Count.ToString());

                            lx = x;
                            ly = y;
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
                    catch { }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-11-timer1_trick");
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
       /// </summary>
       /// <param name="x"></param>
       /// <param name="y"></param>
       /// <returns></returns>
        private Boolean IsInBounds(double x, double y)
        {
            try
            {
                if (mapControl1.Map.Bounds.x1 < x && x < mapControl1.Map.Bounds.x2 && mapControl1.Map.Bounds.y1 < y && y < mapControl1.Map.Bounds.y2)
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
                writeToLog(ex, "frmGuijiTime-12-�жϳ����Ƿ�����ͼ��Χ");
                return false; 
            }
        }


      
        /// <summary>
        /// ��ͣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-13-��ͣ");
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-14-����");
            }
        }

        /// <summary>
        /// ֹͣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-15-ֹͣ");
            }
        }
        

        /// <summary>
        /// ����LOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmGuijiTime_Load(object sender, EventArgs e)
        {
            try
            {
                this.Left = Screen.PrimaryScreen.WorkingArea.Left;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "frmGuijiTime-16-����LOAD");
            }
        }

        /// <summary>
        /// ѡ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                writeToLog(ex, "frmGuijiTime-17-ѡ����");
            }
        }
       
        /// <summary>
        /// ��ѯSQL
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ִ��SQL
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// ��ȡScalar
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Int32 GetScalar(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
        }

        /// <summary>
        /// �쳣��־
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        //��¼������¼
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�������','GPS������λϵͳ:" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch { }
        }
    }
}