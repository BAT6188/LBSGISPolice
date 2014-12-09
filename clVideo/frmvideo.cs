
//********˳�¹�����Ŀ-��Ƶ���ģ��******
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


namespace clVideo
{
    public partial class frmVideo : Form
    {
        public OracleConnection con;
        private MapControl mapControl1 = null;
        public String NowVideoName;//���gridʱ��ѡ�е���Ƶ����         
        public string VideoNm;// ѡ��ʱ����Ƶ������
        public string userid = "";
        public string datasource = "";
        public string password = "";

        public string Videotblname = "";
        public string Videocolname = "";

        private IResultSetFeatureCollection rsfcflash;//��˸��ͼԪ����
        private Boolean GVFlag = false;//��ʼѡ��ص��صı�ʶ
        private int iflash = 0;

        public frmVideo()
        {
            try
            {
                InitializeComponent();

                ReadXML();
                string mysqlstr = "user id =" + userid + " ;data source = " + datasource + ";password =" + password;
                con = new OracleConnection(mysqlstr);
            }
            catch
            { }
        }

        public void getParameter(MapInfo.Windows.Controls.MapControl m)
        {
            mapControl1 = m;
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
                            if (s.Equals("VideoTabName"))
                            {
                                Videotblname = reader.Value;
                            }
                            if (s.Equals("VideoColName"))
                            {
                                Videocolname = reader.Value;
                            }
                            break;
                    }
                }
            }
            catch { }
        }


        public void RefreshTxt(string VideoName)
        {
            //if (VideoName != "")
            //{

            //    con.Open();

            //    string sqlstring = "select * from sysman.��Ƶλ�� where �豸���� = '" + VideoName + "'";
            //    OracleCommand cmd = new OracleCommand(sqlstring, con);

            //    OracleDataReader dr = cmd.ExecuteReader();

            //    while (dr.Read())
            //    {
            //        this.txtID.Text = Convert.ToString( dr.GetInt32(0));
            //        this.txtName.Text = dr.GetString(1);
            //        this.txtPol.Text = dr.GetString(2);
            //        this.txtMan.Text = dr.GetString(3);
            //        this.txtX.Text = Convert.ToString(dr.GetFloat(8));
            //        this.txtY.Text = Convert.ToString(dr.GetFloat(9)); 
            //    }

            //    con.Close();
            //} 
        }




        public void CreateVideoLayer()
        {
            try
            {
                //StopVideo();

                //Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                ////������ʱ��
                //TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoLayer");
                //Table tblTemp = Cat.GetTable("VideoLayer");
                //if (tblTemp != null) //Table exists close it
                //{
                //    Cat.CloseTable("VideoLayer");
                //}

                //tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(frmMap.pMainWin.mapControl1.Map.GetDisplayCoordSys()));
                //tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                //tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));

                //tblTemp = Cat.CreateTable(tblInfoTemp);
                //FeatureLayer lyr = new FeatureLayer(tblTemp);
                //frmMap.pMainWin.mapControl1.Map.Layers.Add(lyr);

                ////��ӱ�ע
                //string activeMapLabel = "VideoLabel";
                //MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");
                //MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                //MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                //lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                //lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                //lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                //lbsource.DefaultLabelProperties.Layout.Offset = 2;
                //lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                ////lbsource.DefaultLabelProperties.Style.Font.TextEffect = MapInfo.Styles.TextEffect.Box;
                //lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                ////lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.ForestGreen;
                //lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                //lbsource.DefaultLabelProperties.Caption = "Name";
                //lblayer.Sources.Append(lbsource);
                //frmMap.pMainWin.mapControl1.Map.Layers.Add(lblayer);

                ////string tempstrCon = "user id = sysman;data source = LBSCHINA_192.168.0.50;password =lbschina";
                ////OracleConnection conv = new OracleConnection(tempstrCon);

                //MapInfo.Mapping.Map map = MapInfo.Engine.Session.Current.MapFactory[1];
                //MapInfo.Mapping.FeatureLayer lyrcar = frmMap.pMainWin.mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                //Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");

                //con.Open();
                //OracleCommand cmd = new OracleCommand("Select * from sysman.��Ƶλ�� order by �豸��� ", con);
                //cmd.ExecuteNonQuery();
                //OracleDataAdapter apt = new OracleDataAdapter(cmd);
                //DataTable dt = new DataTable();
                //apt.Fill(dt);

                //if (dt.Rows.Count > 0)
                //{
                //    foreach (DataRow dr in dt.Rows)
                //    {
                //        FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                //        CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(59,  System.Drawing.Color.Green, 20));
                //        Feature ftr = new Feature(tblcar.TableInfo.Columns);
                //        ftr.Geometry = pt;
                //        ftr.Style = cs;
                //        ftr["Name"] = dr["�豸����"].ToString();
                //        tblcar.InsertFeature(ftr);
                //    }
                //}

                //con.Close();



                //frmMap.pMainWin.OpenTable(Videotblname);

                if (mapControl1.Map.Layers[Videotblname] != null)
                {
                    SetTableVisable(Videotblname);
                    this.SetLayerSelect(Videotblname);
                    mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers[Videotblname]);
                }
                else
                {
                    MessageBox.Show("��Ƶ��ص��ͼ�㲻���ڣ��޷�����ѡ��", "ϵͳ��ʾ", System.Windows.Forms.MessageBoxButtons.OK);
                }

                VideoNm = "All";
                VideoAddGrid();


            }
            catch { }

        }


        public void SetTableVisable(string tablename)
        {
            try
            {
                for (int i = 0; i < mapControl1.Map.Layers.Count; i++)
                {
                    IMapLayer layer = mapControl1.Map.Layers[i];

                    if (layer is FeatureLayer)
                    {
                        if (layer.Name == tablename)
                        {
                            layer.Enabled = true;
                        }
                    }
                    else if (layer is LabelLayer)
                    {
                        LabelLayer lLayer = (LabelLayer)layer;
                        for (int m = 0; m < lLayer.Sources.Count; m++)
                        {

                            if (lLayer.Sources[m].Name == tablename)
                            {
                                lLayer.Sources[m].Enabled = true;
                            }
                        }
                    }
                }

                this.SetLayerSelect(Videotblname);
            }
            catch { }
        }

        public void SetTableDisable(string tablename)
        {
            try
            {
                this.GVFlag = false;
                for (int i = 0; i < mapControl1.Map.Layers.Count; i++)
                {
                    IMapLayer layer = mapControl1.Map.Layers[i];

                    if (layer is FeatureLayer)
                    {
                        if (layer.Name == tablename)
                        {
                            layer.Enabled = false;
                        }
                    }
                    else if (layer is LabelLayer)
                    {
                        LabelLayer lLayer = (LabelLayer)layer;
                        for (int m = 0; m < lLayer.Sources.Count; m++)
                        {

                            if (lLayer.Sources[m].Name == tablename)
                            {
                                lLayer.Sources[m].Enabled = false;
                            }
                        }
                    }
                }
            }
            catch { }
        }


        public void StopVideo()
        {
            try
            {
                //if(mapControl1.Map.Layers[Videotblname] != null ) 
                //{
                //    mapControl1.Map.Layers[Videotblname].IsVisible = false;
                //}

                //if (mapControl1.Map.Layers["VideoLabel"] != null)
                //{
                //    mapControl1.Map.Layers.Remove("VideoLabel");
                //}

                SetTableDisable(Videotblname);
                //frmMap.pMainWin.CloseTable("��Ƶλ��");
            }
            catch { }
        }

        public void InitialVideo()
        {
            //try
            //{
            //    axarray = new AxBABYONLINELib.AxBabyOnline[] { vd1, vd2, vd3, vd4 };
            //    for (int i = 0; i < axarray.Length; i++)
            //    {
            //        int ci = axarray[i].CreateInstance();                                  
            //    }

            //    InitVideoNet(); 
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }

        //�ж���½�����ʼ���ɹ�
        public void InitVideoNet()
        {
            //logVideo = User_Login(); //��Ƶ�����¼
            //if (logVideo == true)
            //{
            //    SetStatus("�ѵ�¼��Ƶ�������");
            //    GetCamList();
            //}
            //else
            //{ SetStatus("��Ƶ��������޷���¼��"); }   
        }

        //�û���¼��Ƶ����
        public void User_Login()
        {
            //try
            //{
            //    int lgflag = 1;
            //    Boolean dlg;

            //    lgflag = this.vd2.Login("222.66.88.27", "8081", "viss", "test123", "12345678");
            //    if (lgflag == 0)
            //    {
            //        dlg = true;
            //        return dlg;
            //    }
            //    else
            //    {
            //        dlg = false;
            //        return dlg;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    return false;
            //}
        }


        //��ȡ��ǰ������������������ַ���
        public void GetCamList()
        {
            //CamString = this.vd2.GetCamList();
            //if (CamString != "")
            //{ }
            //else
            //{ SetStatus("��ǰ�������κ��������Ϣ��"); }
        }


        //�򿪶�Ӧ�������
        //CamName ���������
        public void OpenVideo(string CamName)
        {
            //CamName = "Bell_test";
            //try
            //{                
            //    string temstring = "";
            //    string idstring = "";

            //    //��ȡ�������ϢID
            //    if (CamString != "")
            //    {
            //        for (int i = 0; i < CamString.Length; i++)
            //        {
            //            temstring = CamString.Substring(i, CamName.Length);
            //            if (temstring == CamName)
            //            {
            //                idstring = CamString.Substring(i - 33, 32);
            //                break;
            //            }
            //        }
            //    }

            //    //����Ƶ���м��
            //    if (idstring == "")
            //    {
            //        SetStatus("û�ж�Ӧ���������");
            //        return false;        
            //    }
            //    int sv = 1;


            //    Camint = Camint % 4;
            //    Camint = Camint + 1;

            //    AxBABYONLINELib.AxBabyOnline vd = GetVideoControl(CamName);

            //   // gb.Text = CamName;

            //    vd.StopLiveVideo(); 
            //    sv = vd.StartLiveVideo(idstring, 0);
            //    if (sv != 0)
            //    {
            //        SetStatus("��ǰ������޷��򿪣�");
            //        return false;
            //    }
            //    else
            //    { return true; } 
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    return false;
            //}        
        }

        public void SetStatus(string sstring)
        {
            //this.label2.Text = sstring;
        }



        //��ȡ��ʼ��Ƶ�Ŀؼ����
        //public AxBABYONLINELib.AxBabyOnline  GetVideoControl(string cm)
        //{
        //    if (Camint == 1)
        //    {
        //        this.groupBox1.Text = cm;
        //        return   vd1;
        //    }
        //    else if (Camint == 2)
        //    {
        //        this.groupBox2.Text = cm;
        //        return vd2;
        //    }
        //    else if (Camint == 3)
        //    {
        //        this.groupBox3.Text = cm;
        //        return vd3;
        //    }
        //    else if (Camint == 4)
        //    {
        //        this.groupBox4.Text = cm;
        //        return vd4;
        //    }
        //    else
        //    {
        //        return null;
        //        //gb = null;
        //    }
        //}


        //�رմ���ʱ�ر���Ƶ
       // private void frmVideo_FormClosing(object sender, FormClosingEventArgs e)
        //{
            //this.vd1.StopLiveVideo();
            //this.vd2.StopLiveVideo();
            //this.vd3.StopLiveVideo();
            //this.vd4.StopLiveVideo();

            //frmMap.pMainWin.fCar.GVFlag = false;  //jie.zhang 2008.9.19  ȡ����Ƶ��ص�ѡ�񹤾�

            //Camint = 0;

            //this.groupBox1.Text = "��Ƶ1";
            //this.groupBox2.Text = "��Ƶ2";
            //this.groupBox3.Text = "��Ƶ3";
            //this.groupBox4.Text = "��Ƶ4";

            //this.Visible = false;

            //e.Cancel = true; 
        //}

        private void gvVideo_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                NowVideoName = this.gvVideo.CurrentRow.Cells[1].Value.ToString();
            }
            catch
            {
            }
        }

        private void gvVideo_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (this.gvVideo.Rows.Count != 0)
                {
                    for (int i = 0; i < this.gvVideo.Rows.Count; i++)
                    {
                        if ((i % 2) == 1)
                        {
                            this.gvVideo.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                        else
                        {
                            this.gvVideo.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        }
                    }
                }
            }
            catch { }
        }


        public void VideoAddGrid()
        {
            try
            {
                if (VideoNm != "")
                {
                    con.Open();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = con;
                    if (VideoNm == "All")
                    {                     //�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��
                        cmd.CommandText = "select �豸���,�豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ�� order by �豸���";
                    }
                    else
                    {
                        cmd.CommandText = "select  �豸���,�豸����,�����ɳ���,�ճ�������,X,Y  from ��Ƶλ�� where ( ��Ƶ���� = '" + VideoNm + "')";
                    }

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    lblcount.Text = "����" + dt.Rows.Count.ToString() + "����¼";

                    gvVideo.DataSource = dt;
                    gvVideo.Refresh();
                    con.Close();
                }
            }
            catch { }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboBox1.Text != "")
                {
                    con.Open();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = con;
                    if (this.textBox1.Text == "")
                    {                     //�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��
                        cmd.CommandText = "select �豸���,�豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ�� order by �豸���";
                    }
                    else
                    {
                        cmd.CommandText = "select  �豸���,�豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ�� where ( " + this.comboBox1.Text + " like  '%" + this.textBox1.Text + "%') order by �豸���";
                    }

                    cmd.ExecuteNonQuery();
                    OracleDataAdapter apt = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    apt.Fill(dt);

                    lblcount.Text = "����" + dt.Rows.Count.ToString() + "����¼";

                    gvVideo.DataSource = dt;
                    gvVideo.Refresh();
                    con.Close();
                }
            }
            catch { }
        }

        private void gvVideo_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string ftrname = NowVideoName;

                if (ftrname != "")
                {
                    //Find find = null;
                    MapInfo.Mapping.Map map = null;

                    map = MapInfo.Engine.Session.Current.MapFactory[1];

                    if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                    {
                        return;
                    }

                    MapInfo.Data.MIConnection conn = new MIConnection();
                    conn.Open();

                    MapInfo.Data.MICommand cmd = conn.CreateCommand();
                    Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(Videotblname);
                    cmd.CommandText = "select * from " + mapControl1.Map.Layers[Videotblname].ToString() + " where " + Videocolname + " like '%' +@name +'%'";
                    cmd.Parameters.Add("@name", ftrname);

                    this.rsfcflash = cmd.ExecuteFeatureCollection();

                    MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);

                    if (this.rsfcflash.Count > 0)
                    {
                        foreach (Feature f in this.rsfcflash)
                        {
                            mapControl1.Map.Center = f.Geometry.Centroid;
                        }
                    }

                    cmd.Clone();
                    conn.Close();

                    this.StartFlash();
                }
                else
                {
                    MessageBox.Show("û��ѡ���κγ�����", "ϵͳ��ʾ");
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

        private void StartFlash()
        {
            timLocation.Enabled = true;
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


    }
}