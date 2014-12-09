//********˳�¹�����Ŀ-�������ģ��******
//********�����ˣ�jie.zhang
//********�������ڣ� 2008.9.10
//********��Ȩ���У��Ϻ�����λͼ��Ϣ�Ƽ����޹�˾

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Xml;

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;

namespace clCar
{
    public partial class frmCar : Form
    {
        public MapControl mapControl1 = null;

        //jie.zhang 2008.9.24
        public Boolean GetCarflag = false; //��ʼ������صı�־
        public string NowCarName; //��ǰ��Grid��ѡ�еĳ���
        public string GzCarName;  //�����ٵĳ���������
        public string JzCarName;  //ѡ�еĳ�������

        private OracleConnection con;
        public string CanNm;

        public frmGuijiTime fhistory = new frmGuijiTime();

        public Boolean GVFlag = false;//��ʼѡ��ص��صı�ʶ
        public IResultSetFeatureCollection rsfcView;//��Χ������ͼԪ����
        public string[] carn; //���������ĺ���
        public double[] lastx; //���������ϴεľ���
        public double[] lasty; //���������ϴε�γ��
        public Boolean SetViewFlag = false;//���÷�Χ��ʶ��
        public IResultSetFeatureCollection rsfcflash;//��˸��ͼԪ����
        public int iflash = 0;
        //��ȡ��ǰ��ͼ�Ķ�������ֵ
        //public double ldx = 0; //����x
        //public double ldy = 0; //����y
        //public double rux = 0; //����x
        //public double ruy = 0; //����y
        //end

        //XML�ļ���ȡ�����ݿ���û��������ݿ������û�����
        public string userid = "";
        public string datasource = "";
        public string password = "";

        public frmCar()
        {
            try
            {
                InitializeComponent();

                ReadXML();
                string mysqlstr = "user id =" + userid + " ;data source = " + datasource + ";password =" + password;

                con = new OracleConnection(mysqlstr);
            }
            catch { }
        }

        private void ReadXML()
        {
            try
            {
                string s = "";
                string path = "";
                path = Application.StartupPath;
                XmlDocument doc = new XmlDocument();

                doc.Load("Config.XML");

                XmlNodeReader reader = new XmlNodeReader(doc);

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            s = reader.Name;
                            break;
                        case XmlNodeType.Text:
                            if (s.Equals("user"))
                            {
                                userid = reader.Value;
                            }
                            if (s.Equals("ds"))
                            {
                                datasource = reader.Value;
                            }
                            if (s.Equals("password"))
                            {
                                password = reader.Value;
                            }
                            break;
                    }
                }
            }
            catch { }
        }

        //�õ���ͼ���ؼ��͹�����
        public void getParameter(MapControl m, ToolStrip t)
        {
            mapControl1 = m;
            toolStrip1 = t;
            mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);
            fhistory.mapControl1 = m;
        }

        //��ʼ��datagrid
        private void InitCarGrid()
        {
            try
            {
                string[] HeadArr = new string[] { "���ƺ���", "�ն�����", "������λ", "��ǰ����", "����", "γ��", "�ٶ�", "����", "����״̬", "ʱ��" };

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
                con.Open();
                OracleCommand cmd = new OracleCommand("select * from GPS������λϵͳ", con);
                OracleDataReader dataReader = cmd.ExecuteReader();

                dataGridView1.Rows.Clear();

                for (int i = 1; i < 11; i++)
                {
                    string fieldName = dataReader.GetName(i);
                    dataGridView1.Columns.Add(fieldName, HeadArr[i - 1]);
                    //dataGridView1.Rows.Add(1);

                    //dataGridView1.Rows[i].SetValues(fieldName, "", "����");                   
                }
            }
            catch { }
            finally
            {
                con.Close();
            }
        }

        ////�켣��ʾ
        //private void toolGuiji_Click(object sender, EventArgs e)
        //{
        //    frmGuijiTime fGuijiTime = new frmGuijiTime();
        //    fGuijiTime.ShowDialog();
        //}

        //ѡ�񹤾ߵ��¼�
        private void toolSelect_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                UncheckedTool(e, 0, 3);

                //GetCarflag = true;

                switch (e.ClickedItem.Name)
                {
                    case "starttimer":
                        StartTimeCar();
                        break;
                    case "stoptimer":
                        StopTimeCar();
                        break;
                    case "toolSelByPoint":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        break;
                    case "toolSelByCircle":
                        mapControl1.Tools.LeftButtonTool = "SelectRadius";
                        break;
                    case "toolSelByRect":
                        mapControl1.Tools.LeftButtonTool = "SelectRect";
                        break;
                    case "toolSelByPolygon":
                        mapControl1.Tools.LeftButtonTool = "SelectPolygon";
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        //����������ϵĹ���ʱ���Թ��߰�ť�������ã�
        //ѡ�еı�����ɫ��Ϊ��ɫ������͸�����Ա���ȷ��ǰ��ѡ����
        //���ڰ�ť���飬iFrom��ʾ�����Index��iEnd��ʾĩIndex
        private void UncheckedTool(ToolStripItemClickedEventArgs e, int iFrom, int iEnd)
        {
            try
            {
                for (int i = iFrom; i <= iEnd; i++)
                {
                    if (e.ClickedItem.Owner.Items[i].BackColor == Color.White)
                    {
                        e.ClickedItem.Owner.Items[i].BackColor = Color.Transparent;
                    }
                }
                e.ClickedItem.BackColor = Color.White;
            }
            catch { }
        }


        //��ѡ��ĳ�����Ϊ����Դ����grid��
        public void AddGrid()
        {
            try
            {
                if (CanNm != "")
                {
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }
                    con.Open();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = con;
                    if (CanNm == "All")
                    {                     //�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��
                        cmd.CommandText = "select �ն˳�������,������λ,��ǰ����,X,Y,��ǰ�ٶ�,��ǰ����,����״̬,GPSʱ�� from GPS������λϵͳ";
                    }
                    else
                    {
                        cmd.CommandText = "select �ն˳�������,������λ,��ǰ����,X,Y,��ǰ�ٶ�,��ǰ����,����״̬,GPSʱ�� from GPS������λϵͳ where (" + CanNm + ")";
                    }

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    this.label2.Text = "����" + dt.Rows.Count.ToString() + "����¼";

                    dataGridView1.DataSource = dt;
                    dataGridView1.Refresh();
                    con.Close();
                }
            }
            catch { }
        }

        //ѡ�񹤾�ʱ�ĸ���״̬
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //try
            //{
            //    if (NowCarName != "")
            //    {


            //        switch (e.ClickedItem.Name)
            //        {
            //            case "toolLocation"://��Χ
            //                mapControl1.Tools.LeftButtonTool = "SelectRadius";
            //                GetCarflag = true;
            //                break;
            //            case "toolGenzong": //����1

            //                GzCarName = NowCarName;

            //                break;
            //            case "toolJuzhong": //����2

            //                break;

            //            case "toolGuiji": //�켣

            //                this.Visible = true;
            //                break;
            //            default:
            //                break;
            //        }                    
            //    }
            //    else
            //    { MessageBox.Show("û��ѡ���κγ�����", "ϵͳ��ʾ"); };
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message.ToString(),"ϵͳ��ʾ");
            //}
        }

        //private void AddGrid()
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        //���ñ�����ɫ
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
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch { }
        }

        //��ǰѡ��ĳ����ƺ�
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                NowCarName = this.dataGridView1.CurrentRow.Cells[0].Value.ToString();
            }
            catch
            { }
        }

        public void CopyCarData()
        {
            //try
            //{
            //    con.Open();

            //    OracleCommand cmddel = new OracleCommand();
            //    cmddel.Connection = con;
            //    cmddel.CommandText = "delete from sysman.GPSCAR";
            //    cmddel.ExecuteNonQuery();
            //    //�ж�GPSCar���Ƿ������ݣ������ɾ������
            //    //con.Close();


            //        //����GPS������λϵͳ�в�ͬ�����ݸ��Ƶ�����
            //        //con.Open();
            //        OracleCommand cmdcopy = new OracleCommand();
            //        cmdcopy.Connection = con;

            //        //cmdcopy.CommandText = "insert into sysman.GPSCAR(�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��,�ϴξ���,�ϴ�γ��) select distinct �ն�ID����,�ն˳�������,������λ,��ǰ����,X,Y,��ǰ�ٶ�,��ǰ����,����״̬,GPSʱ��,X,Y from sysman.GPS������λϵͳ";
            //        cmdcopy.CommandText = "insert into sysman.GPSCAR(�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��,�ϴξ���,�ϴ�γ��) select  �ն�ID����,�ն˳�������,������λ,��ǰ����,X,Y,��ǰ�ٶ�,��ǰ����,����״̬,GPSʱ��,X,Y from sysman.GPS������λϵͳ where �ն�ID���� in(select distinct �ն�ID���� from sysman.GPS������λϵͳ)";
            //        cmdcopy.ExecuteNonQuery();

            //    con.Close();
            //}
            //catch (Exception ex)
            //{ 
            ////    MessageBox.Show(ex.Message.ToString());               
            //}
            //finally { con.Close(); };
        }


        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                GetFlash();
            }
            catch
            { }
        }

        //����������ʾ����ʱͼ��
        public void CreateCarLayer()
        {
            try
            {
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("CarLayer");
                Table tblTemp = Cat.GetTable("CarLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("CarLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);

                //��ӱ�ע
                string activeMapLabel = "CarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");
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
            }
            catch { }
        }

        //����������ʾ�Ĺ켣ͼ��
        public void CreateTrackLayer()
        {
            try
            {
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;


                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("Track");
                Table tblTemp = Cat.GetTable("Track");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("Track");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                tblTemp = Cat.CreateTable(tblInfoTemp);

                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Add(lyr);
            }
            catch { }
        }

        public void AddCarFtr()
        {
            string tempstrCon = "user id =" + userid + " ;data source = " + datasource + ";password =" + password;
            OracleConnection conf = new OracleConnection(tempstrCon);

            try
            {
                MapInfo.Mapping.Map map = MapInfo.Engine.Session.Current.MapFactory[1];
                MapInfo.Mapping.FeatureLayer lyrcar =mapControl1.Map.Layers["CarLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("CarLayer");


                conf.Open();
                //����dr = new  OracleDataReader();
                //OracleDataReader dr = GetDataReader("Select * from GPS������λϵͳ", con);

                OracleCommand cmd = new OracleCommand("Select * from GPS������λϵͳ", conf);
                cmd.ExecuteNonQuery();
                OracleDataAdapter apt = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);


                if (dt.Rows.Count > 0)
                {
                    this.lastx = new double[dt.Rows.Count];
                    this.lasty = new double[dt.Rows.Count];
                    this.carn = new string[dt.Rows.Count];
                    int i = 0;

                    foreach (DataRow dr in dt.Rows)
                    {
                        lastx[i] = Convert.ToDouble(dr["X"]);
                        lasty[i] = Convert.ToDouble(dr["Y"]);
                        carn[i] = Convert.ToString(dr["�ն˳�������"]);

                        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                        CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("RED-CAR.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 20));
                        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                        ftr.Geometry = pt;
                        ftr.Style = cs;
                        ftr["Name"] = dr["�ն˳�������"].ToString();
                        tblcar.InsertFeature(ftr);

                        i = i + 1;
                    }
                }
            }
            catch { }
            finally
            {
                conf.Close();
            };
        }

        //�ж�����ֵ�Ƿ��ڶ��㷶Χ��
        public Boolean IsInBounds(double x, double y)
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
            catch { return false; }
        }


        private void timLocation_Tick(object sender, EventArgs e)
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
                    timLocation.Enabled = false;
                }
            }
            catch { }

        }

        public void GetFlash()
        {
            try
            {
                string tblname = "CarLayer";
                string colname = "Name";
                string ftrname = NowCarName;

                if (ftrname != "")
                {
                    //Find find = null;
                    MapInfo.Mapping.Map map = null;

                    map = MapInfo.Engine.Session.Current.MapFactory[1];

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }

                    //MapInfo.Mapping.FeatureLayer findlayer = (MapInfo.Mapping.FeatureLayer)map.Layers[tblname];
                    //find = new Find(findlayer.Table, findlayer.Table.TableInfo.Columns[colname]);
                    //FindResult result = find.Search(ftrname);
                    //if(result.ExactMatch)
                    //{
                    MapInfo.Data.MIConnection conn = new MIConnection();
                    conn.Open();

                    MapInfo.Data.MICommand cmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(tblname);
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[tblname].ToString() + " where " + colname + " like '%' +@name +'%'";
                    cmd.Parameters.Add("@name", ftrname);

                    rsfcflash = cmd.ExecuteFeatureCollection();

                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash);

                    if (rsfcflash.Count > 0)
                    {
                        foreach (Feature f in rsfcflash)
                        {
                            mapControl1.Map.Center = f.Geometry.Centroid;
                        }
                    }

                    cmd.Clone();
                    conn.Close();

                    StartFlash();
                }
                else
                {
                    MessageBox.Show("û��ѡ���κγ�����", "ϵͳ��ʾ");
                }
            }
            catch { }
        }

        public void StartFlash()
        {
            timLocation.Enabled = true;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //��������ͼ������ͼԪ 

                //MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                //FeatureLayer lyr = this.mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                Catalog cat = MapInfo.Engine.Session.Current.Catalog;
                Table tbl = cat.GetTable("CarLayer");

                MapInfo.Mapping.Map map = null;

                map = MapInfo.Engine.Session.Current.MapFactory[1];

                //MapInfo.Data.IResultSetFeatureCollection rsfc = session.Selections.DefaultSelection[lyr.Table]; 



                //=========================================
                //DEBUG--------�������� jie.zhang 20081008
                //=========================================
                //========start========================

                //if (tbl != null)
                //{
                //    //��ȡ���ݿ����ݽ��и���
                //    if (con.State == System.Data.ConnectionState.Open)
                //    {
                //        con.Close();
                //    }
                //    con.Open();

                //    OracleCommand cmd = new OracleCommand("Select * from sysman.GPSTEMP", con);

                //    cmd.ExecuteNonQuery();
                //    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                //    DataTable dt = new DataTable();
                //    apt.Fill(dt);

                //    int carid = 0;
                //    string cnum = "";
                //    double x = 0;
                //    double y = 0;

                //    if (dt.Rows.Count > 0)
                //    {
                //        foreach (DataRow dr in dt.Rows)
                //        {
                //            //��ȡ�����ƺ� �ͻ�ȡ��γ��
                //             carid =  Convert.ToInt32(dr["ID"]);;
                //             cnum = Convert.ToString( dr["CN"]);
                //             x = Convert.ToDouble( dr["X"]);
                //             y = Convert.ToDouble(dr["Y"]);
                //             break;
                //        }

                //        double lx = 0;
                //        double ly = 0;
                //        int j = 0;

                //        //��ȡ�������ϴξ�γ��
                //        for (j = 0; j < this.carn.Length; j++)
                //        {
                //            if (this.carn[j] == cnum)
                //            {
                //                lx = this.lastx[j];
                //                ly = this.lasty[j];
                //                break;
                //            }
                //        }
                //        con.Close();

                //        con.Open();
                //        OracleCommand cmdd1 = new OracleCommand("delete from sysman.GPSTEMP where ID = " + carid.ToString(), con);
                //        cmdd1.ExecuteNonQuery();
                //        con.Close();



                //        //���ݳ������ƺ��ƶ�����
                //        tbl.BeginAccess(MapInfo.Data.TableAccessMode.Write);

                //        foreach (Feature fcar in tbl)
                //        {
                //            if (fcar["Name"].ToString() == cnum)
                //            {
                //                fcar.Geometry.GetGeometryEditor().OffsetByXY(x - lx, y - ly, MapInfo.Geometry.DistanceUnit.Degree, MapInfo.Geometry.DistanceType.Spherical);
                //                MapInfo.Geometry.DPoint dpoint;
                //                MapInfo.Geometry.Point point;

                //                dpoint = new DPoint(x, y);
                //                point = new MapInfo.Geometry.Point(map.GetDisplayCoordSys(), dpoint);
                //                fcar.Geometry = point;



                //                if (cnum == this.GzCarName)
                //                {                                        //����
                //                    Boolean inf =this.IsInBounds(x, y);
                //                    if (inf == false)
                //                    {
                //                        mapControl1.Map.Center = dpoint;
                //                    }
                //                    Trackline(x, y, lx, ly);
                //                }

                //                if (cnum == this.JzCarName)
                //                {                                     //����                                    
                //                    mapControl1.Map.Center = dpoint;
                //                }

                //                fcar.Geometry.EditingComplete();
                //                fcar.Update();

                //                mapControl1.Refresh();

                //                tbl.EndAccess();

                //                this.lastx[j] = x;
                //                this.lasty[j] = y;

                //                break;
                //            }
                //        }
                //     }
                //    }                  

                //=========================================
                //DEBUG--------�������� jie.zhang 20081008
                //=========================================
                //========end========================

                if (tbl != null)
                {

                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }

                    con.Open();
                    OracleCommand cmd = new OracleCommand("Select * from sysman.GPS������λϵͳ", con);

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    double lx = 0;
                    double ly = 0;
                    int j = 0;

                    string cnum = "";
                    double x = 0;
                    double y = 0;

                    if (dt.Rows.Count > 0)
                    {
                        //��ȡ�����ƺ� �ͻ�ȡ��γ��

                        foreach (DataRow dr in dt.Rows)
                        {
                            cnum = Convert.ToString(dr["�ն˳�������"]);
                            x = Convert.ToDouble(dr["X"]);
                            y = Convert.ToDouble(dr["Y"]);

                            //��ȡ�������ϴξ�γ��
                            for (j = 0; j < this.carn.Length; j++)
                            {
                                if (carn[j] == cnum)
                                {
                                    lx = lastx[j];
                                    ly = lasty[j];
                                    break;
                                }
                            }

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

                                    if (cnum == this.GzCarName)
                                    {
                                        //����
                                        Boolean inflag = this.IsInBounds(x, y);
                                        if (inflag == false)
                                        {
                                            mapControl1.Map.Center = dpoint;
                                        }
                                        Trackline(x, y, lx, ly);
                                        //this.mapControl1.Map.Center = fcar.Geometry.Centroid;
                                    }
                                    if (cnum == this.JzCarName)
                                    {
                                        //����
                                        mapControl1.Map.Center = dpoint;
                                    }

                                    //if (rsfcView != null)
                                    //{    //��Χ

                                    //    Boolean inflag = IsInBounds(x, y);
                                    //    if (inflag == false)
                                    //    {
                                    //        mapControl1.Map.SetView(rsfcView.Envelope);
                                    //    }
                                    //}

                                    fcar.Geometry.EditingComplete();
                                    fcar.Update();

                                    mapControl1.Refresh();

                                    tbl.EndAccess();

                                    lastx[j] = x;
                                    lasty[j] = y;

                                    break;
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch { }
        }

        public void StartTimeCar()
        {
            try
            {
                StopTimeCar();

                CreateCarLayer();
                CreateTrackLayer();
                AddCarFtr();

                CanNm = "All";
                AddGrid();

                mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["CarLayer"]);

                ToolCarEnable();

                CreateDate();
                this.SetLayerSelect("CarLayer");

                timeCar.Interval = 1000;
                //timeCar.Enabled = true;
            }
            catch { }
        }

        public void CreateDate()
        {
            //ɾ����ʱ�������
            try
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
                con.Open();
                OracleCommand cmdelAll = new OracleCommand("Delete from sysman.GPSTEMP", con);
                cmdelAll.ExecuteNonQuery();
                con.Close();

                con.Open();
                OracleCommand cmdsel = new OracleCommand("select �����ƺ�,����,γ�� from sysman.GPSCAR", con);
                OracleDataReader drsel = cmdsel.ExecuteReader();

                int i = 0;

                while (drsel.Read())
                {
                    i = i + 1;
                    string mysqlstr = "user id =" + userid + " ;data source = " + datasource + ";password =" + password; ;
                    OracleConnection conn = new OracleConnection(mysqlstr);
                    conn.Open();
                    OracleCommand cmdins = new OracleCommand("insert into sysman.GPSTEMP(ID,CN,X,Y) values(" + i + ",'" + drsel.GetString(0) + "'," + drsel.GetFloat(1) + "," + drsel.GetFloat(2) + ")", conn);
                    cmdins.ExecuteNonQuery();
                    conn.Close();
                }
                con.Close();
            }
            catch { }
        }


        public void StopTimeCar()
        {
            try
            {
                timeCar.Enabled = false;
                GetCarflag = false;

                ToolCarDisable();

                if (mapControl1.Map.Layers["Track"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("Track");
                    //mapControl1.Map.Layers.Remove("Track");
                }

                if (mapControl1.Map.Layers["CarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("CarLayer");
                    //mapControl1.Map.Layers.Remove("CarLayer");
                }


                if (mapControl1.Map.Layers["GuijiLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("GuijiLayer");
                    //mapControl1.Map.Layers.Remove("GuijiLayer");
                }

                if (mapControl1.Map.Layers["CarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("CarLabel");
                }
            }
            catch { }
        }


        //������      
        public void Trackline(double x1, double y1, double x2, double y2)
        {
            try
            {
                DPoint pts = new DPoint(x1, y1);
                DPoint pte = new DPoint(x2, y2);

                MapInfo.Mapping.Map map = MapInfo.Engine.Session.Current.MapFactory[1];
                MapInfo.Mapping.FeatureLayer workLayer = (MapInfo.Mapping.FeatureLayer)map.Layers["Track"];
                MapInfo.Data.Table tblTemp = MapInfo.Engine.Session.Current.Catalog.GetTable("Track");

                FeatureGeometry lfg = MultiCurve.CreateLine(workLayer.CoordSys, pts, pte);

                MapInfo.Styles.SimpleLineStyle lsty = new MapInfo.Styles.SimpleLineStyle(new MapInfo.Styles.LineWidth(3, MapInfo.Styles.LineWidthUnit.Pixel), 2, System.Drawing.Color.OrangeRed);
                MapInfo.Styles.CompositeStyle cstyle = new MapInfo.Styles.CompositeStyle(lsty);

                MapInfo.Data.Feature lft = new MapInfo.Data.Feature(tblTemp.TableInfo.Columns);
                lft.Geometry = lfg;
                lft.Style = cstyle;
                workLayer.Table.InsertFeature(lft);
            }
            catch { }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboBox1.Text != "")
                {
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }

                    con.Open();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = con;
                    if (this.textBox1.Text == "")
                    {                     //�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��
                        cmd.CommandText = "select �ն˳�������,�ն�����,������λ,��ǰ����,X,Y,��ǰ�ٶ�,��ǰ����,GPSʱ�� from sysman.GPS������λϵͳ order by �ն˳�������";
                    }
                    else
                    {
                        cmd.CommandText = "select �ն˳�������,�ն�����,������λ,��ǰ����,X,Y,��ǰ�ٶ�,��ǰ����,GPSʱ�� from sysman.GPS������λϵͳ where ( " + this.comboBox1.Text + " like  '%" + this.textBox1.Text + "%') order by �ն˳�������";
                    }

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    this.label2.Text = "����" + dt.Rows.Count.ToString() + "����¼";

                    this.dataGridView1.DataSource = dt;
                    this.dataGridView1.Refresh();
                    con.Close();
                }
            }
            catch { }
        }


        public void ToolCarEnable()
        {
            try
            {
                for (int i = 10; i < 14; i++)
                {
                    string tooltext = toolStrip1.Items[i].Text;

                    toolStrip1.Items[i].Visible = true;
                }
            }
            catch { }
        }


        public void ToolCarDisable()
        {
            try
            {
                for (int i = 10; i < 14; i++)
                {
                    string tooltext = toolStrip1.Items[i].Text;

                    toolStrip1.Items[i].Visible = false;
                }
            }
            catch { }
        }

        //���õ�ǰͼ���ѡ��
        private void SetLayerSelect(string layername)
        {
            MapInfo.Mapping.Map map = null;

            map = MapInfo.Engine.Session.Current.MapFactory[1];

            for (int i = 0; i < map.Layers.Count; i++)
            {
                IMapLayer layer = map.Layers[i];
                string lyrname = layer.Alias;

                MapInfo.Mapping.LayerHelper.SetSelectable(layer, false);
            }

            MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername], true);

        }

        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                switch (e.ToolName)
                {
                    case "SelectRadius":
                        if (this.GetCarflag)
                        {
                            MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;
                            FeatureLayer lyr = this.mapControl1.Map.Layers["CarLayer"] as FeatureLayer;

                            this.rsfcView = session.Selections.DefaultSelection[lyr.Table];
                            if (this.rsfcView != null)
                            {
                                string SearchName = "";
                                int i = 1;
                                foreach (Feature f in this.rsfcView)
                                {
                                    foreach (MapInfo.Data.Column col in f.Columns)
                                    {
                                        if (col.ToString() == "Name")
                                        {
                                            string ftename = f["Name"].ToString();
                                            if (i == this.rsfcView.Count || this.rsfcView.Count == 1)
                                            {
                                                SearchName = SearchName + "�ն˳������� = '" + ftename + "'";
                                            }
                                            else
                                            {
                                                SearchName = SearchName + "�ն˳������� = '" + ftename + "' or ";
                                            }
                                            i = i + 1;
                                        }
                                    }
                                }

                                this.CanNm = SearchName; //20081008
                                this.AddGrid();

                                this.SetViewFlag = true;
                                if (this.rsfcView.Count > 0)
                                {
                                    if (this.rsfcView.Count == 1)
                                    {
                                        foreach (Feature f in this.rsfcView)
                                        {
                                            this.mapControl1.Map.Center = f.Geometry.Centroid;
                                        }
                                    }
                                    else
                                    {
                                        mapControl1.Map.SetView(this.rsfcView.Envelope);
                                    }
                                }
                            }

                        }
                        break;
                }
            }
            catch { }
        }
    }
}