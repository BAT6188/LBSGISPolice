
//********˳�¹�����Ŀ-��Ƶ���ģ��******
//********�����ˣ�jie.zhang
//********�������ڣ� 2008.9.10
//********�汾�޸ģ�
//********1. 2009.4.15  �޸��ƶ���Ƶ�ɲ鿴������ʵʱ�ƶ�
//********2. 2009.5.8   �޸���Ƶͼ���С�����ƶ���Ƶ���ڶ���
//********3. 2009.5.13  �޸Ĳ�ѯ�ƶ���Ƶ�޽��
//                      �ƶ���Ƶ�ı�ע����ͨ��ͬ
//                      ȷ������������ͼ��Χ��
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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;


using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Windows.Controls;
using clPopu;


namespace clVideo
{
    public partial class ucVideo : UserControl
    {

        //jie.zhang 20090119  �޸Ĺ̶�����Ϊ��̬����
        #region
        static MapControl mapControl1 = null;
        static string[] StrCon;
        static string mysqlstr;
        public ToolStripDropDownButton tlvideo;
        static int videoport = 0;
        static string[] VideoConnectString; // CMS�������ַ���
        static ToolStripLabel StatusLabel = null;
        static int messageId = 0;
        static string VideoClient = string.Empty;
        static string UserRegion;   //jie.zhang 20090119
        #endregion

        public string VideoNm;// ѡ��ʱ����Ƶ������
        public string userid = "";
        public string datasource = "";
        public string password = "";
        public Boolean VideoFlag = false; // ͨѶ�Ƿ��Ѿ���ͨ�ı�ʶ��
        public string Videotblname = "VideoLayer";
        public string Videocolname = "Name";
        public String NowVideoName;//���gridʱ��ѡ�е���Ƶ����  
        private IResultSetFeatureCollection rsfcflash;//��˸��ͼԪ����
        public Boolean GVFlag = false;//��ʼѡ��ص��صı�ʶ
        private int iflash = 0;
        //public string getFromNamePath;// ��ȡ���ֶ������ļ���ַ

        public Boolean[] Camlist;  //�Ѵ򿪵�����ͷ��mapid������

        public string strRegion = string.Empty;
        public string strRegion1 = "";
        public string user = "";
        
        public string strRegion2 = ""; // �ɵ������ɳ���
        public string strRegion3 = ""; // �ɵ������ж�
        public string excelSql = "";   // ��Ƶ��ѯ����sql
        public int _startNo, _endNo;   // �ɵ�����ҳ��

        public System.Data.DataTable dtExcel = null; //����Excel��
        OracleDataAdapter apt1 = null;

        private static NetworkStream networkStream1 = null;
        public Boolean ZhiHui = false;

        public ToolStripProgressBar toolPro;  // ���ڲ�ѯ�Ľ�������lili 2010-8-10
        public ToolStripLabel toolProLbl;     // ������ʾ�����ı���
        public ToolStripSeparator toolProSep;


        /// <summary>
        /// ����ȫ�ֲ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="m">��ͼ�ؼ�</param>
        /// <param name="st">״̬��</param>
        /// <param name="tl">�����������˵�</param>
        /// <param name="s">SQL�����ַ���</param>
        /// <param name="vport">���Ӷ˿�</param>
        /// <param name="vs">���Ӳ�����</param>
        /// <param name="exepath">��س�������Ŀ¼</param>
        /// <param name="zh">ֱ��ָ�ӱ�ʶ��</param>
        public ucVideo(MapInfo.Windows.Controls.MapControl m, ToolStripLabel st, System.Windows.Forms.ToolStripDropDownButton tl, string[] s, int vport, string[] vs, string exepath, Boolean zh,Boolean isEvent)
        {
          
                InitializeComponent();
                try
                {
                    mapControl1 = m;

                    StrCon = s;

                    mysqlstr = "data source =" + StrCon[0] + ";user id =" + StrCon[1] + ";password=" + StrCon[2];

                    videoport = vport;

                    VideoConnectString = vs;

                    StatusLabel = st;

                    VideoClient = exepath;

                    ZhiHui = zh;

                    //mapControl1.Tools.FeatureSelected -= new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);

                    //mapControl1.Map.ViewChangedEvent -= new ViewChangedEventHandler(MapControl1_ViewChanged);
                    //mapControl1.Tools.Used -= new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);

                    mapControl1.Tools.FeatureSelected += new MapInfo.Tools.FeatureSelectedEventHandler(Feature_Selected);

                    mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(MapControl1_ViewChanged);
                    mapControl1.Tools.Used += new MapInfo.Tools.ToolUsedEventHandler(Tools_Used);

                    tlvideo = tl;
                    if (isEvent)
                    {

                        tlvideo.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolvideo_DropDownItemClicked);
                    }

                    this.comboBox1.Items.Clear();
                    this.comboBox1.Items.Add("�豸����");
                    this.comboBox1.Items.Add("�����ɳ���");
                    this.comboBox1.Text = this.comboBox1.Items[0].ToString();

                }
                catch (Exception ex)
                {
                    ExToLog(ex, "01-���캯������ȫ�ֲ���");
                }           
        }


        double xx1 = 0;
        double yy1 = 0;
        double xx2 = 0;
        double yy2 = 0; 

        /// <summary>
        /// ��ͼ����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void Tools_Used(object sender, MapInfo.Tools.ToolUsedEventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    switch (e.ToolName)
                    {
                        case "SelectRect":
                            if (mapControl1.Map.Scale < 5000) return;

                           

                            switch (e.ToolStatus)
                            {

                                case MapInfo.Tools.ToolStatus.Start:
                                    xx1 = e.MapCoordinate.x;
                                    yy1 = e.MapCoordinate.y;
                                    break;
                                case ToolStatus.End:
                                    xx2 = e.MapCoordinate.x;
                                    yy2 = e.MapCoordinate.y;

                                    if (xx1 != 0 && yy1 != 0 && xx2 != 0 && yy2 != 0)
                                    {
                                        if (xx1 > xx2)
                                        {
                                            double xx = xx1;
                                            xx1 = xx2;
                                            xx2 = xx;
                                        }

                                        if (yy1 > yy2)
                                        {
                                            double yy = yy1;
                                            yy1 = yy2;
                                            yy2 = yy;
                                        }

                                        string sqlcmd = string.Empty;

                                        if (UserRegion == string.Empty)
                                        {
                                            MessageBox.Show("��û������Ȩ�ޣ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            return;
                                        }
                                        else if (UserRegion == "˳����")
                                        {
                                            sqlcmd = "Select �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID,X,Y,STYID from ��Ƶλ��VIEW where X>" + xx1 + " and X<" + xx2 + " and Y>" + yy1 + " and Y < " + yy2 + " order by �豸��� desc";
                                        }
                                        else
                                        {
                                            if (Array.IndexOf(UserRegion.Split(','), "����") > -1)
                                            {
                                                UserRegion = UserRegion.Replace("����", "����,��ʤ");
                                            }
                                            sqlcmd = "Select �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID,X,Y,STYID from ��Ƶλ��VIEW  where �����ɳ��� in ('" + UserRegion.Replace(",", "','") + "') and X>" + xx1 + " and X<" + xx2 + " and Y>" + yy1 + " and Y < " + yy2 + " order by �豸���  desc";
                                        }

                                        DataTable dt = GetTable(sqlcmd);
                                        if (this.SpeVideoArray == null && dt != null)
                                            this.SpeVideoArray = dt.Clone();

                                        this.SpeVideoArray.Merge(dt,false);// jie.zhang 20101215 �޸�Ϊ������ݳ��Ǵ������ɾ�� GetTable(sqlcmd);

                                        this.AddSpecVideoFtr();
                                    }

                                    break;
                            }

                           
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                ExToLog(ex, "Tools_Used");
            }       
        }

        /// <summary>
        /// ��ͼ��ͼ�ı��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void MapControl1_ViewChanged(object sender, EventArgs e)
        {
            try
            {
                if (mapControl1.Map.Layers.Count == 0) return;

                if (this.Visible && mapControl1.Map.Scale < 5000)
                {
                    CreateLayer();                    
                }
                else if (this.Visible)
                {
                    if (mapControl1.Map.Layers[Videotblname] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoLayer");
                    }

                    if (mapControl1.Map.Layers["VideoLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoLabel");
                    }

                    if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoCarLayer");
                    }

                    if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                    {
                        mapControl1.Map.Layers.Remove("VideoCarLabel");
                    }

                    AddSpecVideoFtr();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "MapControl1_ViewChanged-32-��ͼ��Ұ�����仯ʱ");
            }
        }



        /// <summary>
        /// ��ȡ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ns">������</param>
        /// <param name="vf">���ӳɹ���ʶ</param>
        public void getNetParameter(NetworkStream ns,Boolean vf)
        {
            try
            {
                networkStream1 = ns;
                VideoFlag = vf;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getNetParameter-02-��ȡ�������");
            }
        }

        public string videotablename = string.Empty;
        public string videocolumname = string.Empty;
        /// <summary>
        /// ��ȡ����������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="vtn">����</param>
        /// <param name="vcn">����</param>
        public void getVideoparam(string vtn, string vcn)
        {
            try
            {
                videotablename = vtn;
                videocolumname = vcn;
            }
            catch (Exception ex) { ExToLog(ex, "getVideoparam-03-��ȡ����������"); }
        }

        private MapInfo.Data.MultiResultSetFeatureCollection mirfc = null;
        private MapInfo.Data.IResultSetFeatureCollection mirfc1 = null;
        /// <summary>
        /// ͼԪѡ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void Feature_Selected(object sender, MapInfo.Tools.FeatureSelectedEventArgs e)
        {
            if (this.Visible)
            {
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
                    if (f.Table.Alias == "VideoLayer")
                    {

                        MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;

                        FeatureLayer lyr;

                        lyr = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;

                        IResultSetFeatureCollection rsfcView = session.Selections.DefaultSelection[lyr.Table];

                        if (rsfcView != null)
                        {
                            if (rsfcView.Count > 0)
                            {
                                foreach (Feature ft in rsfcView)
                                {
                                    string ftename = ft["Name"].ToString();

                                    if (this.gvVideo.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < gvVideo.Rows.Count; i++)
                                        {
                                            if (gvVideo.Rows[i].Cells[0].Value.ToString() == ftename)
                                            {
                                                gvVideo.CurrentCell = gvVideo.Rows[i].Cells[0];
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }                              
                    }
                    else if (f.Table.Alias == "VideoCarLayer")
                    {
                        MapInfo.Engine.ISession session = MapInfo.Engine.Session.Current;

                        FeatureLayer lyr;

                        lyr = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;

                        IResultSetFeatureCollection rsfcView = session.Selections.DefaultSelection[lyr.Table];

                        if (rsfcView != null)
                        {
                            if (rsfcView.Count > 0)
                            {
                                foreach (Feature ft in rsfcView)
                                {
                                    string ftename = ft["Name"].ToString();

                                    if (this.gvVideo.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < gvVideo.Rows.Count; i++)
                                        {
                                            if (gvVideo.Rows[i].Cells[0].Value.ToString() == ftename)
                                            {
                                                gvVideo.CurrentCell = gvVideo.Rows[i].Cells[0];
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }    
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Feature_Selected-04-ͼԪѡ��");
                }
            }
        }

        /// <summary>
        /// ����ģ������Ȩ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public void SetUserRegion()
        {
            try
            {
                if (strRegion == "" && strRegion1 == "")// ��û���ɳ���Ȩ�ޣ�Ҳû���ж�Ȩ��
                {
                    return;
                }
                else if (strRegion == "" && strRegion1 != "") // ���ж�Ȩ�ޣ�û���ɳ���Ȩ��
                {
                    UserRegion = GetPolice(strRegion1);

                }
                else if (strRegion != "" && strRegion1 != "")  // ���ж�Ȩ�ޣ�Ҳ���ɳ���Ȩ��
                {
                    UserRegion = strRegion + "," + GetPolice(strRegion1);
                }
                else if (strRegion != "" && strRegion1 == "")
                {
                    UserRegion = strRegion;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetUserRegion-05-����ģ������Ȩ��");
            }
        }


        /// <summary>
        /// ��ȡ�ж����ڵ��ɳ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="s1">�ж������ַ���</param>
        /// <returns>�ɳ�������</returns>
        private String GetPolice(string s1)
        {           
            string reg = string.Empty;

            try
            {
                string[] ZdArr = s1.Split(',');
                for (int i = 0; i < ZdArr.Length; i++)
                {
                    string zdn = ZdArr[i];

                    DataTable dt = GetTable("Select �����ɳ��� from �������ж� where �ж���='" + zdn + "'");
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (i != ZdArr.Length - 1)
                            {
                                reg = reg + dr["�����ɳ���"].ToString() + ",";
                            }
                            else
                            {
                                reg = reg + dr["�����ɳ���"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetPolice-06-��ȡ�ж����ڵ��ɳ���");
            }
            return reg;
        }

        /// <summary>
        /// ������Ƶͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public void CreateVideoLayer()
        {
            try
            {
                VideoNm = "All";

                OpenVideoClient();

                CreateLayer();

                VideoAddGrid();


                //this.toolPro.Value = 5;
                //Application.DoEvents();

            }
            catch (Exception ex)
            {
                //isShowPro(false);
                ExToLog(ex, "CreateVideoLayer-07-������Ƶͼ��");
            }
        }

        /// <summary>
        /// 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void CreateLayer()
        {
            try
            {
                if (tlvideo.Visible == false)
                    tlvideo.Visible = true;

                if (mapControl1.Map.Scale > 5000) return;

                if (mapControl1.Map.Layers.Count == 0) return;

                isShowPro(true);
                this.toolPro.Maximum = 6;
                StopVideo();

                string s = CLC.DatabaseRelated.OracleDriver.GetConString;

                AddVideoFtr();
                this.toolPro.Value = 3;
                Application.DoEvents();

                AddCarViedoFtr();
                this.toolPro.Value = 4;
                Application.DoEvents();

                if (mapControl1.Map.Layers["VideoCarLayer"] != null && mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    this.SetLayerSelect("VideoCarLayer", Videotblname);
                    this.GVFlag = true;
                }
                else
                {
                    isShowPro(false);
                    return;
                }

                if (this.GVFlag == false)
                    this.GVFlag = true;

                if (this.timer1.Enabled == false)
                {
                    this.timer1.Interval = 30 * 1000;
                    this.timer1.Enabled = true;
                    this.toolPro.Value = 6;
                }
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                //isShowPro(false);
                ExToLog(ex, "CreateLayer");
            }
        }


        private DataTable SpeVideoArray;
        private string SpeType;  //��ǰ��ѡ���˵㻹�ǿ�ѡ

        /// <summary>
        /// ��ӱ�ѡ����ѡ����Ƶ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void AddSpecVideoFtr()
        {
            try
            {
                if (this.SpeType == "") return;

                if (mapControl1.Map.Layers.Count == 0) return;

                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }
                if (SpeVideoArray.Rows.Count > 0)
                {

                    Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                    //������ʱ��
                    TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoLayer");
                    Table tblTemp = Cat.GetTable("VideoLayer");
                    if (tblTemp != null) //Table exists close it
                    {
                        Cat.CloseTable("VideoLayer");
                    }

                    tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                    tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("�豸���", 50));

                    tblTemp = Cat.CreateTable(tblInfoTemp);
                    FeatureLayer lyr = new FeatureLayer(tblTemp);
                    //mapControl1.Map.Layers.Add(lyr);
                    mapControl1.Map.Layers.Insert(0, lyr);
                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    //��ӱ�ע
                    string activeMapLabel = "VideoLabel";
                    MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");
                    MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                    MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                    lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                    lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                    lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                    lbsource.DefaultLabelProperties.Layout.Offset = 2;
                    lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                    lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                    lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                    lbsource.DefaultLabelProperties.Caption = "Name";
                    lblayer.Sources.Append(lbsource);
                    mapControl1.Map.Layers.Add(lblayer);

                    this.toolPro.Value = 2;
                    Application.DoEvents();


                    MapInfo.Mapping.Map map = mapControl1.Map;
                    MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;
                    Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");

                    //if(SpeType=="Square")
                    //{

                    foreach (DataRow dr in SpeVideoArray.Rows)
                    {
                        if (dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr["�豸����"].ToString() != "" && dr["�豸���"].ToString() != "")
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;

                            CompositeStyle cs = new CompositeStyle();

                            if (dr["STYID"].ToString() == "0")       //�������Ƶ
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }
                            else if (dr["STYID"].ToString() == "1")  //���������Ƶ
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("TARG1-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }

                            Feature ftr = new Feature(tblcar.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = dr["�豸����"].ToString();
                            ftr["�豸���"] = dr["�豸���"].ToString();
                            tblcar.InsertFeature(ftr);
                        }
                    }
                }
                else if (SpeType == "Single")
                {

                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AddSpecVideoFtr-08-���ͼԪ");
            }
        }

        /// <summary>
        /// ���ͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void AddVideoFtr()
        {
            try
            {
                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }

                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoLayer");
                Table tblTemp = Cat.GetTable("VideoLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("VideoLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("�豸���", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);
                this.toolPro.Value = 1;
                Application.DoEvents();

                //��ӱ�ע
                string activeMapLabel = "VideoLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                this.toolPro.Value = 2;
                Application.DoEvents();


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");

                string sqlcmd = string.Empty;

                if (UserRegion == string.Empty)
                {
                    MessageBox.Show("��û������Ȩ�ޣ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (UserRegion == "˳����")
                {
                    sqlcmd = "Select �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID,X,Y,STYID from ��Ƶλ��VIEW where X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2 + " order by �豸��� desc";
                }
                else
                {
                    if (Array.IndexOf(UserRegion.Split(','), "����") > -1)
                    {
                        UserRegion = UserRegion.Replace("����", "����,��ʤ");
                    }
                    sqlcmd = "Select �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID,X,Y,STYID from ��Ƶλ��VIEW  where �����ɳ��� in ('" + UserRegion.Replace(",", "','") + "') and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2 + " order by �豸���  desc";
                }

                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr["�豸����"].ToString() != "" && dr["�豸���"].ToString() != "")
                        {
                            FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;

                            CompositeStyle cs = new CompositeStyle();

                            if (dr["STYID"].ToString() == "0")       //�������Ƶ
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }
                            else if (dr["STYID"].ToString() == "1")  //���������Ƶ
                            {
                                cs = new CompositeStyle(new BitmapPointStyle("TARG1-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            }

                            Feature ftr = new Feature(tblcar.TableInfo.Columns);
                            ftr.Geometry = pt;
                            ftr.Style = cs;
                            ftr["Name"] = dr["�豸����"].ToString();
                            ftr["�豸���"] = dr["�豸���"].ToString();
                            tblcar.InsertFeature(ftr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AddVideoFtr-08-���ͼԪ");
            }
        }

        /// <summary>
        /// ������Ƶ�г����ľ�γ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="CamName">�������</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        private void UpdateVideoCarGrid(string CamName, double x, double y)
        {
            try
            {
                if (CamName != "")
                {                    
                    if (this.gvVideo.Rows.Count > 0)
                    {
                        for (int i = 0; i < gvVideo.Rows.Count; i++)
                        {
                            if (gvVideo.Rows[i].Cells[0].Value.ToString() == CamName)
                            {
                                gvVideo.Rows[i].Cells[3].Value = x.ToString();
                                gvVideo.Rows[i].Cells[4].Value = y.ToString();
                                break;            
                            }                         
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "UpdateVideoCarGrid-09-������Ƶ�еĳ����ľ�γ��");
            }
        }


        /// <summary>
        /// ����ƶ���ƵͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void AddCarViedoFtr()
        {
            try
            {
                if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("VideoCarLayer");
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }


                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoCarLayer");
                Table tblTemp = Cat.GetTable("VideoCarLayer");
                if (tblTemp != null) //Table exists close it
                {
                    Cat.CloseTable("VideoCarLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("�豸���", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                //mapControl1.Map.Layers.Add(lyr);
                mapControl1.Map.Layers.Insert(0, lyr);

                //��ӱ�ע
                string activeMapLabel = "VideoCarLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.YellowGreen;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrcar = mapControl1.Map.Layers["VideoCarLayer"] as FeatureLayer;
                Table tblcar = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoCarLayer");

                string sqlcmd = string.Empty;
                if (UserRegion == string.Empty)
                {
                    MessageBox.Show("û����������Ȩ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (UserRegion == "˳����")
                {
                    sqlcmd = "Select  �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID,X,Y  from ��Ƶλ��VIEW where STYID='2'and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2;
                }
                else
                {
                    if (Array.IndexOf(UserRegion.Split(','), "����") > -1)
                    {
                        UserRegion = UserRegion.Replace("����", "����,��ʤ");
                    }
                    sqlcmd = "Select �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID,X,Y  from ��Ƶλ��VIEW where STYID='2' and Ȩ�޵�λ in ('" + UserRegion.Replace(",", "','") + "') and X>" + mapControl1.Map.Bounds.x1 + " and X<" + mapControl1.Map.Bounds.x2 + " and Y>" + mapControl1.Map.Bounds.y1 + " and Y < " + mapControl1.Map.Bounds.y2;
                }
                                
                DataTable dt = GetTable(sqlcmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string camid = dr["�豸���"].ToString();
                        double xv = Convert.ToDouble(dr["X"]);
                        double yv = Convert.ToDouble(dr["Y"]);
                        if (xv != 0 && yv != 0 && dr["X"].ToString() != "" && dr["Y"].ToString() != "")
                        {
                            if (camid.Length > 5)
                            {
                                try
                                {
                                    FeatureGeometry pt = new MapInfo.Geometry.Point(lyrcar.CoordSys, new DPoint(xv, yv)) as FeatureGeometry;

                                    CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("ydsp.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));
                                    Feature ftr = new Feature(tblcar.TableInfo.Columns);
                                    ftr.Geometry = pt;
                                    ftr.Style = cs;
                                    ftr["Name"] = dr["�豸����"].ToString();
                                    ftr["�豸���"] = dr["�豸���"].ToString();
                                    tblcar.InsertFeature(ftr);
                                }
                                catch (Exception ex) { ExToLog(ex, "10-����ͼԪ"); }
                                UpdateVideoCarGrid(dr["�豸����"].ToString(), Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "AddCarViedoFtr-10-����ƶ���ƵͼԪ");
            }
        }

        /// <summary>
        /// �ж������Ƿ��ڿ��ӷ�Χ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="x">x����</param>
        /// <param name="y">y����</param>
        /// <returns>����ֵ��true-�ڿ��ӷ�Χ false-���ڿ��ӷ�Χ��</returns>
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
            catch (Exception ex)
            {
                ExToLog(ex, "IsInBounds-11-�жϳ����Ƿ��ڿ��ӷ�Χ");
                return false;
            }
        }

        /// <summary>
        /// ����ͼ��ɼ�--������Ѵ��ڵ�ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="tablename">ͼ������</param>
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
                                lLayer.Sources[m].Enabled = false;                                 
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "SetTableVisable-12-����ͼ��ɼ�");
            }
        }


       /// <summary>
        /// ����ͼ�㲻�ɼ�--������Ѵ��ڵ�ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
       /// </summary>
       /// <param name="tablename">ͼ������</param>
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
            catch( Exception ex )
            {
                ExToLog(ex, "SetTableDisable-13-����ͼ�㲻�ɼ�");
            }
        }

        /// <summary>
        /// ֹͣ��Ƶ���ģ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void StopVideo()
        {
            try
            {
                this.GVFlag = false;

                //tlvideo.Visible = false;

                if (mapControl1.Map.Layers[Videotblname] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }

                if (mapControl1.Map.Layers["VideoCarLayer"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLayer");
                }

                if (mapControl1.Map.Layers["VideoCarLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoCarLabel");
                }

                if (this.timer1.Enabled == true)
                    this.timer1.Enabled = false;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "StopVideo-14-ֹͣ��Ƶ���ģ��");
            }
        }

        /// <summary>
        /// �л��б�ѡ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void gvVideo_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView grid = (DataGridView)sender;
                NowVideoName = grid.CurrentRow.Cells[0].Value.ToString();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "gvVideo_SelectionChanged-15-�л��б�ѡ��");
            }
        }

        /// <summary>
        /// �����б�����ɫ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
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
            catch(Exception ex) 
            {
                ExToLog(ex, "gvVideo_DataBindingComplete-16-�����б�����ɫ");
            }
        }

        /// <summary>
        /// ������ݵ��б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void VideoAddGrid()
        {
            if (VideoNm != "")
            {
                try
                {
                    string sql = string.Empty;
                    if (UserRegion == string.Empty)
                    {
                        MessageBox.Show("��û������Ȩ�ޣ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (UserRegion == "˳����")
                    {
                        if (VideoNm == "All")
                        {
                            sql = "select �豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ��VIEW";
                        }
                        else
                        {
                            sql = "select  �豸����,�����ɳ���,�ճ�������,X,Y  from ��Ƶλ��VIEW where ( �豸���� = '" + VideoNm + "')";
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(UserRegion.Split(','), "����") > -1 && UserRegion.IndexOf("��ʤ") < 0)
                        {
                            UserRegion = UserRegion.Replace("����", "����,��ʤ");
                        }

                        if (VideoNm == "All")
                        {
                            sql = "select �豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ��VIEW where �����ɳ��� in ('" + UserRegion.Replace(",", "','") + "') ";
                        }
                        else
                        {
                            sql = "select  �豸����,�����ɳ���,�ճ�������,X,Y  from ��Ƶλ��VIEW where ( �豸���� = '" + VideoNm + "' and �����ɳ��� in  ('" + UserRegion.Replace(",", "','") + "')";
                        }
                    }

                    DataTable dt = new DataTable();

                    if (sql != "")
                    {
                        dt = GetTable(sql);
                    }
                    else
                    {
                        return;
                    }
                    #region ����Excel
                    try
                    {
                        excelSql = sql;
                        excelSql = "select �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID, X,Y " + excelSql.Substring(excelSql.IndexOf("from"));
                        string sRegion2 = strRegion2;
                        string sRegion3 = strRegion3;

                        if (strRegion2 != "˳����")
                        {
                            if (strRegion2 != "")
                            {
                                if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
                                {
                                    sRegion2 = strRegion2.Replace("����", "����,��ʤ");
                                }
                                excelSql += " and (Ȩ�޵�λ in ('" + sRegion2.Replace(",", "','") + "'))";
                            }
                            else if (strRegion2 == "")
                            {
                                if (excelSql.IndexOf("where") < 0)
                                {
                                    excelSql += " where 1=2 ";
                                }
                                else
                                {
                                    excelSql += " and 1=2 ";
                                }
                            }
                        }
                        _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                        _endNo = pageSize1;

                        //OracleConnection orc = new OracleConnection(mysqlstr);
                        //orc.Open();
                        //OracleCommand cmd = new OracleCommand(excelSql, orc);
                        //apt1 = new OracleDataAdapter(cmd);
                        //DataTable datatableExcel = new DataTable();
                        //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                        //if (dtExcel != null) dtExcel.Clear();
                        //dtExcel = datatableExcel;
                        //cmd.Dispose();
                        //orc.Close();
                    }
                    catch { }
                    # endregion

                    lblcount.Text = "����" + dt.Rows.Count.ToString() + "����¼";

                    if (dt != null)
                    {
                        Pagedt1 = dt;
                        InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "VideoAddGrid-17-����Ƶ��ӵ��б�");
                }
            }
        }

        /// <summary>
        /// ��ѯ��ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchVideo();           
        }

        /// <summary>
        /// ��Ƶ��ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void SearchVideo()
        {
            if (this.comboBox1.Text != "")
            {
                 try
                {
                    string sql = string.Empty;
                    isShowPro(true);
                    if (UserRegion == string.Empty)
                    {
                        isShowPro(false);
                        MessageBox.Show("��û������Ȩ�ޣ�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (UserRegion == "˳����")
                    {
                        if (this.CaseSearchBox.Text == "")
                        {                     //�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��

                            sql = "select �豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ��VIEW";
                        }
                        else
                        {
                            sql = "select  �豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ��VIEW where " + this.comboBox1.Text + " like  '%" + this.CaseSearchBox.Text + "%'"; 
                        }                       
                    }
                    else
                    {
                        if (Array.IndexOf(UserRegion.Split(','), "����") > -1 && UserRegion.IndexOf("��ʤ") < 0)
                        {
                            UserRegion = UserRegion.Replace("����", "����,��ʤ");
                        }

                        if (this.CaseSearchBox.Text == "")
                        {                     //�ն�ID,�����ƺ�,������λ,��ǰ����,����,γ��,�ٶ�,����,����״̬,ʱ��

                            sql = "select �豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ��VIEW where �����ɳ��� in ('" + UserRegion.Replace(",", "','") + "')";
                        }
                        else
                        {
                            sql = "select �豸����,�����ɳ���,�ճ�������,X,Y from ��Ƶλ��VIEW where " + this.comboBox1.Text + " like  '%" + this.CaseSearchBox.Text + "%' and �����ɳ��� in ('" + UserRegion.Replace(",", "','") + "')";
                        }                      
                    }

                    // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                    //if (sql.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                    //    sql += " and (�����ֶ�һ is null or �����ֶ�һ='')";
                    //else
                    //    sql += " where (�����ֶ�һ is null or �����ֶ�һ='')";
                    //-------------------------------------------------------

                    DataTable dt  = GetTable(sql);

                    this.toolPro.Value = 1;
                    Application.DoEvents();

                    #region ����Excel
                    try
                    {
                        excelSql = sql;
                        excelSql = "select �豸���,�豸����,�����ɳ���,�ճ�������,�豸ID, X,Y " + excelSql.Substring(excelSql.IndexOf("from"));
                        string sRegion2 = strRegion2;
                        string sRegion3 = strRegion3;

                        if (strRegion2 != "˳����")
                        {
                            if (strRegion2 != "")
                            {
                                if (Array.IndexOf(strRegion2.Split(','), "����") > -1)
                                {
                                    sRegion2 = strRegion2.Replace("����", "����,��ʤ");
                                }
                                excelSql += " and (Ȩ�޵�λ in ('" + sRegion2.Replace(",", "','") + "'))";
                            }
                            else if (strRegion2 == "")
                            {
                                if (excelSql.IndexOf("where") < 0)
                                {
                                    excelSql += " where 1=2 ";
                                }
                                else
                                {
                                    excelSql += " and 1=2 ";
                                }
                            }
                        }
                        _startNo = PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1;
                        _endNo = pageSize1;
                        
                    //    // ���ñ����ֶ�һ(������ֶ���ֵ��˼�¼����ʾ�����¼��ʾ) lili 2010-8-19
                    //    //if (excelSql.IndexOf("where") >= 0)    // �ж��ַ������Ƿ���where
                    //    //    excelSql += " and �����ֶ�һ is null or �����ֶ�һ=''";
                    //    //else
                    //    //    excelSql += " where �����ֶ�һ is null or �����ֶ�һ=''";
                    //    //-------------------------------------------------------

                    //    OracleConnection orc = new OracleConnection(mysqlstr);
                    //    orc.Open();
                    //    OracleCommand cmd = new OracleCommand(excelSql, orc);
                    //    apt1 = new OracleDataAdapter(cmd);
                    //    DataTable datatableExcel = new DataTable();
                    //    apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                    //    if (dtExcel != null) dtExcel.Clear();
                    //    dtExcel = datatableExcel;                  
                    }
                    catch { isShowPro(false); }
                    this.toolPro.Value = 2;
                    Application.DoEvents();
                    # endregion

                    lblcount.Text = "����" + dt.Rows.Count.ToString() + "����¼";
                    Pagedt1 = dt;
                    InitDataSet1(RecordCount1, PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                    WriteEditLog("��Ƶ��ѯ", "��Ƶλ��View", sql, "��ѯ��Ƶ");
                    this.toolPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    ExToLog(ex, "SearchVideo-18-��Ƶ��ѯ");
                }
            }
        }



        //==========
        //==========
        //��ҳ����
        //==========
        //==========

        int pageSize1 = 100;     //ÿҳ��ʾ����
        int PagenMax1 = 0;         //�ܼ�¼��
        int pageCount1 = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageCurrent1 = 0;   //��ǰҳ��
        int PagenCurrent1 = 0;      //��ǰ��¼�� 
        DataSet Pageds1 = new DataSet();
        DataTable Pagedt1 = new DataTable();

        /// <summary>
        /// ��ʼ����ҳ�ؼ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="lblcount">������ʾ�ܼ�¼���ӿؼ�</param>
        /// <param name="textNowPage">������ʾ��ǰҳ���ӿؼ�</param>
        /// <param name="lblPageCount">������ʾ��ҳ�����ӿؼ�</param>
        /// <param name="bs">��ҳ�ؼ�������Դ</param>
        /// <param name="bn">��ҳ�ؼ�</param>
        /// <param name="dgv">��ʾ���б�</param>
        public void InitDataSet1(ToolStripLabel lblcount, ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bs, BindingNavigator bn, DataGridView dgv)
        {
            try
            {
                //pageSize1 = 100;      //����ҳ������
                PagenMax1 = Pagedt1.Rows.Count;
                TextNum1.Text = pageSize1.ToString();
                lblcount.Text = PagenMax1.ToString() + "��";//�ڵ���������ʾ�ܼ�¼��

                pageCount1 = (PagenMax1 / pageSize1);//�������ҳ��
                if ((PagenMax1 % pageSize1) > 0) pageCount1++;
                if (PagenMax1 != 0)
                {
                    pageCurrent1 = 1;
                }
                PagenCurrent1 = 0;       //��ǰ��¼����0��ʼ

                LoadData1(textNowPage, lblPageCount, bs, bn, dgv);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "20-InitDataSet1");
            }
        }

        /// <summary>
        /// ���ݷ�ҳ��ѯ������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="textNowPage">������ʾ��ǰҳ���ӿؼ�</param>
        /// <param name="lblPageCount">������ʾ��ҳ�����ӿؼ�</param>
        /// <param name="bds">��ҳ�ؼ�������Դ</param>
        /// <param name="bdn">��ҳ�ؼ�</param>
        /// <param name="dgv">��ʾ���б�</param>
        public void LoadData1(ToolStripTextBox textNowPage, ToolStripLabel lblPageCount, BindingSource bds, BindingNavigator bdn, DataGridView dgv)
        {
            try
            {
                isShowPro(true);
                int nStartPos = 0;
                int nEndPos = 0;

                DataTable dtTemp = Pagedt1.Clone();

                if (pageCurrent1 == pageCount1)
                    nEndPos = PagenMax1;
                else
                    nEndPos = pageSize1 * pageCurrent1;
                nStartPos = PagenCurrent1;

                //tsl.Text = Convert.ToString(pageCurrent1) + "/" + pageCount1.ToString();
                textNowPage.Text = Convert.ToString(pageCurrent1);
                lblPageCount.Text = "/" + pageCount1.ToString();
                this.toolPro.Value = 1;
                Application.DoEvents();

                _startNo = nStartPos;
                _endNo = nEndPos;

                //��Ԫ����Դ���Ƽ�¼��
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    dtTemp.ImportRow(Pagedt1.Rows[i]);
                    PagenCurrent1++;
                }
                dataVideo = new DataTable();  // �Ŵ������õ�DataTable

                bds.DataSource = dataVideo = dtTemp;
                bdn.BindingSource = bds;
                this.toolPro.Value = 2;
                Application.DoEvents();
                dgv.DataSource = bds;
                this.toolPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                ExToLog(ex, "20-LoadData1");
            }
        }

        /// <summary>
        /// ��ҳ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void bindingNavigator1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent1--;
                    if (pageCurrent1 < 1)
                    {
                        pageCurrent1 = 1;
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��");
                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                }
                if (e.ClickedItem.Text == "��һҳ")
                {
                    pageCurrent1++;
                    if (pageCurrent1 > pageCount1)
                    {
                        pageCurrent1 = pageCount1;

                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��");

                        return;
                    }
                    else
                    {
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent1 <= 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent1 = 1;
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent1 > pageCount1 - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent1 = pageCount1;
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    }
                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                }
                else if (e.ClickedItem.Text == "���ݵ���")
                {
                    //DataExport();
                }

                #region ���ݵ���
                //DataTable datatableExcel = new DataTable();
                //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                //if (dtExcel != null) dtExcel.Clear();
                //dtExcel = datatableExcel;
                #endregion
            }
            catch (Exception ex)
            {
                ExToLog(ex, "bindingNavigator1_ItemClicked-21-��ҳ����");
            }
        }

        /// <summary>
        /// ּ������ÿҳ��ʾ����������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void TextNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
               if (e.KeyChar.ToString() == "\r" && gvVideo.Rows.Count > 0)
                {
                    pageSize1 = Convert.ToInt32(TextNum1.Text);
                    pageCurrent1 = 1;   //��ǰת����һҳ
                    PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                    pageCount1 = (PagenMax1 / pageSize1);//�������ҳ��
                    if ((PagenMax1 % pageSize1) > 0) pageCount1++;

                    LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "22-TextNum1_KeyPress");
            }
        }

        /// <summary>
        /// ּ��ʵ��ҳ��ת��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void PageNow1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(this.PageNow1.Text) < 1 || Convert.ToInt32(this.PageNow1.Text) > pageCount1)
                    {
                        System.Windows.Forms.MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow1.Text = pageCurrent1.ToString();
                        return;
                    }
                    else
                    {
                        pageCurrent1 = Convert.ToInt32(this.PageNow1.Text);
                        PagenCurrent1 = pageSize1 * (pageCurrent1 - 1);
                        LoadData1(PageNow1, PageNum1, bindingSource1, bindingNavigator1, gvVideo);

                        #region ���ݵ���
                        //DataTable datatableExcel = new DataTable();
                        //apt1.Fill(PagenCurrent1 == 0 ? 0 : PagenCurrent1 - pageSize1 < 0 ? 0 : PagenCurrent1 - pageSize1, pageSize1, datatableExcel);
                        //if (dtExcel != null) dtExcel.Clear();
                        //dtExcel = datatableExcel;
                        #endregion
                    }
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "23-PageNow1_KeyPress");
            }
        }
       
        /// <summary>
        /// �б�˫���¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void gvVideo_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string ftrname = NowVideoName;

                OracleConnection orc11 = new OracleConnection(mysqlstr);
                orc11.Open();
                OracleCommand cmd11 = new OracleCommand("Select * from ��Ƶλ��VIEW where �豸����='" + NowVideoName + "'", orc11);
                OracleDataAdapter apt11 = new OracleDataAdapter(cmd11);
                DataTable dt11 = new DataTable();
                apt11.Fill(dt11);
                if (dt11.Rows.Count > 0)
                {
                    this.SpeVideoArray = dt11;

                    double x = Convert.ToDouble(dt11.Rows[0]["X"]);
                    double y = Convert.ToDouble(dt11.Rows[0]["Y"]);
                    mapControl1.Map.SetView(new DPoint(x, y), mapControl1.Map.GetDisplayCoordSys(), getScale());

                    if (ftrname != "")
                    {
                        MapInfo.Mapping.Map map = mapControl1.Map;

                        if ((MapInfo.Engine.Session.Current.MapFactory.Count == 0) || (map == null))
                        {
                            return;
                        }

                        rsfcflash = null;

                        MapInfo.Data.MIConnection conn = new MIConnection();
                        conn.Open();

                        MapInfo.Data.MICommand cmd = conn.CreateCommand();
                        Table selecttable = MapInfo.Engine.Session.Current.Catalog.GetTable(Videotblname);
                        cmd.CommandText = "select * from " + mapControl1.Map.Layers[Videotblname].ToString() + " where " + Videocolname + " = @name ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@name", ftrname);

                        this.rsfcflash = cmd.ExecuteFeatureCollection();
                        MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();

                        if (this.rsfcflash.Count > 0)
                        {
                            //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                            //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                            //foreach (Feature f in this.rsfcflash)
                            //{
                            //    mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                            //    mapControl1.Map.Center = f.Geometry.Centroid;
                            //    break;
                            //}
                        }
                        else
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "select * from " + mapControl1.Map.Layers["VideoCarLayer"].ToString() + " where " + Videocolname + "=@name";
                            cmd.Parameters.Add("@name", ftrname);
                            this.rsfcflash = cmd.ExecuteFeatureCollection();

                            if (this.rsfcflash.Count > 0)
                            {
                                //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                                //MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(this.rsfcflash);
                                //foreach (Feature f in this.rsfcflash)
                                //{
                                //    mapControl1.Map.SetView(f.Geometry.Centroid, cSys, getScale());
                                //    mapControl1.Map.Center = f.Geometry.Centroid;
                                //    break;
                                //}
                            }
                        }

                        cmd.Clone();
                        conn.Close();

                        if (this.rsfcflash.Count < 1) return;
                        this.StartFlash();
                        WriteEditLog("��Ƶ��ѯ", "��Ƶλ��View", "�豸����=" + NowVideoName, "˫����Ƶ");
                    }
                    else
                    {
                        MessageBox.Show("û��ѡ���κγ�����", "ϵͳ��ʾ");
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "gvVideo_DoubleClick-23-���б���˫��");
            }
            finally
            {
                try { fmDis.Close(); }
                catch { }
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
                CLC.BugRelated.ExceptionWrite(ex, "getScale-��ȡ���ű���");
                return 0;
            }
        }

        /// <summary>
        /// ���õ�ǰͼ���ѡ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="layername1">ͼ��1������</param>
        /// <param name="layername2">ͼ��2������</param>
        private void SetLayerSelect(string layername1, string layername2)
        {
            try
            {
                MapInfo.Mapping.Map map = mapControl1.Map;

                for (int i = 0; i < map.Layers.Count; i++)
                {
                    IMapLayer layer = map.Layers[i];
                    string lyrname = layer.Alias;

                    MapInfo.Mapping.LayerHelper.SetSelectable(layer, false);
                }
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername1], true);
                MapInfo.Mapping.LayerHelper.SetSelectable(mapControl1.Map.Layers[layername2], true);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetLayerSelect-24-����ͼ���ѡ��");
            }
        }

        /// <summary>
        /// ����ͼԪ��˸
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void StartFlash()
        {
            try
            {
                timLocation.Enabled = true;
            }
            catch (Exception ex) { ExToLog(ex, "StartFlash"); }
        }

        /// <summary>
        /// ͼԪ��˸
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void timLocation_Tick(object sender, EventArgs e)
        {
            try
            {
                iflash = iflash + 1;

                int i = iflash % 2;
                if (i == 0)
                {
                    try
                    {
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Add(rsfcflash);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {                  

                    try
                    {
                        MapInfo.Engine.Session.Current.Selections.DefaultSelection.Clear();
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                if (this.iflash % 10 == 0)
                {
                    timLocation.Enabled = false;
                }
            }
            catch(Exception ex) 
            {
                ExToLog(ex, "timLocation_Tick-25-ͼԪ��˸");
            }
        }

        /// <summary>
        /// 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void SetFullView()
        {
            try
            {
                Map map = mapControl1.Map;
                IMapLayerFilter lyrFilter = MapLayerFilterFactory.FilterByType(typeof(FeatureLayer));
                MapLayerEnumerator lyrEnum = map.Layers.GetMapLayerEnumerator(lyrFilter);
                map.SetView(lyrEnum);
                //mapControl1.Tools.LeftButtonTool = "Select";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetFullView-25-ͼԪ��˸");
            }
        }

        /// <summary>
        /// �ܱ߲�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="dpt"></param>
        /// <param name="dis"></param>
        public void SearchVideoDistance(MapInfo.Geometry.DPoint dpt, Double dis,string moName)
        {
            try
            {
                if (moName == "ֱ��ָ��")  // �����ֱ��ָ����ȥ���ĵ�ͼ���Ÿı��¼�
                {
                    mapControl1.Map.ViewChangedEvent -= new ViewChangedEventHandler(MapControl1_ViewChanged);
                }

                StopVideo();
                Catalog Cat = MapInfo.Engine.Session.Current.Catalog;
                //������ʱ��
                TableInfoMemTable tblInfoTemp = new TableInfoMemTable("VideoLayer");
                Table tblTemp = Cat.GetTable("VideoLayer");
                if (tblTemp != null) 
                {
                    Cat.CloseTable("VideoLayer");
                }

                tblInfoTemp.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(mapControl1.Map.GetDisplayCoordSys()));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStyleColumn());
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("Name", 40));
                tblInfoTemp.Columns.Add(ColumnFactory.CreateStringColumn("�豸���", 50));

                tblTemp = Cat.CreateTable(tblInfoTemp);
                FeatureLayer lyr = new FeatureLayer(tblTemp);
                mapControl1.Map.Layers.Insert(0, lyr);

                //��ӱ�ע
                string activeMapLabel = "VideoLabel";
                MapInfo.Data.Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");
                MapInfo.Mapping.LabelLayer lblayer = new MapInfo.Mapping.LabelLayer(activeMapLabel, activeMapLabel);

                MapInfo.Mapping.LabelSource lbsource = new MapInfo.Mapping.LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 12;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.TopCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.Red;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Style.Font.Shadow = true;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);
                mapControl1.Map.Layers.Add(lblayer);

                //��VideoLayer��ѡ���ܱߵ���Ƶ������ӵ�tempvideo

                double x1, x2;
                double y1, y2;
                double x, y;

                double dBufferDis = dis / 111000;
                x = dpt.x;
                y = dpt.y;
                x1 = x - dBufferDis;
                x2 = x + dBufferDis;
                y1 = y - dBufferDis;
                y2 = y + dBufferDis;


                MapInfo.Mapping.Map map = mapControl1.Map;
                MapInfo.Mapping.FeatureLayer lyrvideo = mapControl1.Map.Layers["VideoLayer"] as FeatureLayer;//(MapInfo.Mapping.FeatureLayer)map.Layers["CarLayer"];
                Table tblvideo = MapInfo.Engine.Session.Current.Catalog.GetTable("VideoLayer");

                string[] camidarr = null;
                string sql = string.Empty;

                if (UserRegion == string.Empty)   //add by fisher in 09-12-08
                {
                    MessageBox.Show(@"��û������Ȩ�ޣ�", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (UserRegion == "˳����")
                {
                   sql = "Select * from ��Ƶλ��VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2;
                }
                else
                {
                    if (Array.IndexOf(UserRegion.Split(','), "����") > -1 && UserRegion.IndexOf("��ʤ") < 0)
                    {
                        UserRegion = UserRegion.Replace("����", "����,��ʤ");
                    }
                    sql = "Select * from ��Ƶλ��VIEW where X >" + x1 + "  and  X<" + x2 + "  and Y>" + y1 + "  and  Y<" + y2 + " and �����ɳ��� in ('" + UserRegion.Replace(",", "','") + "')";
                }

                DataTable dt = GetTable(sql);
                if (dt.Rows.Count > 0)
                {

                    if (dt.Rows.Count > 32)
                    {
                        camidarr = new string[32];
                    }
                    else
                    {
                        camidarr = new string[dt.Rows.Count];
                    }

                    int i = 0;
                    try
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                          
                            string camid = dr["�豸���"].ToString();
                            if (camid != "" && dr["X"].ToString() != "" && dr["Y"].ToString() != "" && Convert.ToDouble(dr["X"]) > 0 && Convert.ToDouble(dr["Y"]) > 0 && dr["�豸����"].ToString() != "")
                            {
                                FeatureGeometry pt = new MapInfo.Geometry.Point(lyrvideo.CoordSys, new DPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"]))) as FeatureGeometry;
                                CompositeStyle cs = new CompositeStyle();

                                if (this.ZhiHui == true)
                                {
                                    cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.ApplyColor, System.Drawing.Color.Red, 12));
                                }
                                else
                                {
                                    cs = new CompositeStyle(new SimpleVectorPointStyle(46, System.Drawing.Color.Red, 20));
                                }
                                Feature ftr = new Feature(tblvideo.TableInfo.Columns);
                                ftr.Geometry = pt;
                                ftr.Style = cs;
                                ftr["Name"] = dr["�豸����"].ToString();
                                ftr["�豸���"] = dr["�豸���"].ToString();
                                tblvideo.InsertFeature(ftr);

                                if (i <= 31)
                                {
                                    camidarr[i] = camid;
                                    i = i + 1;
                                }                             
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExToLog(ex, "26-����ͼԪ");                       
                    }
                }
                OpenVideo(camidarr);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SearchVideoDistance-26-�ܱ߲�ѯ");
            }
        }
     
        /// <summary>
        /// ��ȡmessageid
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <returns></returns>
        public static int getMessageId()
        {
            if (messageId >= 65000)
                messageId = 0;
            return messageId++;
        }

        /// <summary>
        /// Socket��������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="msg1"></param>
        /// <returns></returns>
        private bool SendMsg1(byte[] msg1)
        {
            foreach (byte i in msg1)
            {
                Console.Write(i);
            }

            try
            {
                networkStream1.Write(msg1, 0, msg1.Length);
                networkStream1.Flush();
                ShowDoInfo("���ͳɹ�");
                //writelog("���ݷ��ͳɹ�" + Encoding.UTF8.GetString(msg1, 0, msg1.Length) + "  ���ȣ�" + msg1.Length.ToString());
            }
            catch (Exception ex)
            {
                ShowDoInfo("����ʧ��");
                ExToLog(ex, "SendMsg1-27-Socket��������");
                return false;               
            }
            return true;
        }

        /// <summary>
        /// �鿴���ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="camid"></param>
        /// <returns>����ֵ��true-�鿴�ɹ� false-�鿴ʧ�ܣ�</returns>
        public bool OpenVideo(string camid)
        {
            try
            {
                // ���camera������TLV
                byte[] ct = new byte[2];   //camera������Type   
                ct[1] = 9;
                ct[0] = 0;

                byte[] c_value = Encoding.UTF8.GetBytes(camid);  //camera������Value
                if (c_value.Length % 4 != 0)                     //�ж��Ƿ�Ϊ��4�ı���
                {
                    byte[] temp1 = new byte[c_value.Length];
                    for (int t1 = 0; t1 < c_value.Length; t1++)
                    {
                        temp1[t1] = c_value[t1];
                    }

                    c_value = new byte[temp1.Length + (4 - temp1.Length % 4)];
                    for (int ii = 0; ii < temp1.Length; ii++)
                    {
                        c_value[ii] = temp1[ii];
                    }
                }

                byte[] cl = new byte[2];       //camera������Length
                cl[1] = (byte)(4 + c_value.Length);
                cl[0] = Convert.ToByte((4 + c_value.Length) / 256);

                //��ͷ
                byte[] v = new byte[1] { 1 };    //�汾
                byte[] tp = new byte[1] { 2 };   //request
                byte[] cmd = new byte[1] { 9 };  //����
                byte[] rs = new byte[1] { 0 };   // ���
                byte[] tl = new byte[2];        //�ܳ���
                tl[1] = (byte)(12 + c_value.Length);
                tl[0] = Convert.ToByte((12 + c_value.Length) / 256);

                byte[] s = new byte[2];       //sqence number �ĵ�һ���ֽ�

                int tempMessageId = getMessageId();
                s[0] = Convert.ToByte(tempMessageId / 256);
                s[1] = Convert.ToByte(tempMessageId % 256);

                buffer = new byte[12 + c_value.Length];

                int i = 0;
                Array.Copy(v, 0, buffer, i, v.Length);  //�汾�Ÿ���

                i += v.Length;
                Array.Copy(tp, 0, buffer, i, tp.Length);   //type����

                i += tp.Length;
                Array.Copy(cmd, 0, buffer, i, cmd.Length); // �����

                i += cmd.Length;
                Array.Copy(rs, 0, buffer, i, rs.Length); //�������

                i += rs.Length;
                Array.Copy(tl, 0, buffer, i, tl.Length); // ���ȸ���

                i += tl.Length;
                Array.Copy(s, 0, buffer, i, s.Length);  //squence number ����

                i += s.Length;
                Array.Copy(ct, 0, buffer, i, ct.Length);  // camera type����

                i += ct.Length;
                Array.Copy(cl, 0, buffer, i, cl.Length);  //camera  length����

                i += cl.Length;
                Array.Copy(c_value, 0, buffer, i, c_value.Length); //camera value�ĸ���       

                //this.timer1.Interval = 4000;
                //this.timer1.Enabled = true;
                //this.sendtime = 0;
                //Thread.Sleep(10000);

                Boolean b = SendMsg1(buffer);

                Thread.Sleep(1000);

                if (SendMsg1(buffer) == false)
                {
                    MessageBox.Show("�鿴ͼ��ʧ�ܣ���ȷ�ϼ�ؿͻ����Ѿ��򿪲��Ҷ˿�������ȷ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "OpenVideo-28-�鿴���ͼ��");
                return false;
            }
        }


        public byte[] buffer;
        /// <summary>
        /// ���ʹ�cameraid
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void OpenVideo(string[] camerarray)
        {
            try
            {
                // cameralistTLV

                byte[] ct = new byte[2];   //cameralist������Type   
                ct[1] = 9;
                ct[0] = 0;


                // cameralist ������value 

                byte[] c1_value = Encoding.UTF8.GetBytes(camerarray[0]);  //cameralist��һ��Value

                byte[] c_value = new byte[c1_value.Length + (camerarray.Length - 1) * (c1_value.Length + 1)];

                int k = 0;

                Array.Copy(c1_value, 0, c_value, k, c1_value.Length);  // ����һ��cameraid���Ƶ�value��

                for (int j = 1; j < camerarray.Length; j++)   //��cameralist �ĵڶ���ֵ��ʼѭ����ֵ
                {
                    byte[] temp_value = Encoding.UTF8.GetBytes("," + camerarray[j]);  // 

                    Array.Copy(temp_value, 0, c_value, c1_value.Length + temp_value.Length * (j - 1), temp_value.Length);  // ����һ��cameraid���Ƶ�value��
                }

                if (c_value.Length % 4 != 0)
                {
                    byte[] temp1 = new byte[c_value.Length];
                    for (int t1 = 0; t1 < c_value.Length; t1++)
                    {
                        temp1[t1] = c_value[t1];
                    }

                    c_value = new byte[temp1.Length + (4 - temp1.Length % 4)];

                    for (int ii = 0; ii < temp1.Length; ii++)
                    {
                        c_value[ii] = temp1[ii];
                    }
                }


                byte[] cl = new byte[2];       //cameralist������Length
                cl[1] = (byte)(4 + c_value.Length);
                cl[0] = Convert.ToByte((4 + c_value.Length)/256);

                //��ͷ
                byte[] v = new byte[1] { 1 };    //�汾
                byte[] tp = new byte[1] { 2 };   //request
                byte[] cmd = new byte[1] { 9 };  //����
                byte[] rs = new byte[1] { 0 };   // ���
                byte[] tl = new byte[2];        //�ܳ���
                tl[1] = (byte)(12 + c_value.Length);
                tl[0] = Convert.ToByte((12 + c_value.Length)/256);


                byte[] s = new byte[2];       //sqence number �ĵ�һ���ֽ�

                int tempMessageId = getMessageId();
                s[0] = Convert.ToByte(tempMessageId / 256);
                s[1] = Convert.ToByte(tempMessageId % 256);

                buffer = new byte[12 + c_value.Length];

                int i = 0;
                Array.Copy(v, 0, buffer, i, v.Length);  //�汾�Ÿ���

                i += v.Length;
                Array.Copy(tp, 0, buffer, i, tp.Length);   //type����

                i += tp.Length;
                Array.Copy(cmd, 0, buffer, i, cmd.Length); // �����

                i += cmd.Length;
                Array.Copy(rs, 0, buffer, i, rs.Length); //�������

                i += rs.Length;
                Array.Copy(tl, 0, buffer, i, tl.Length); // ���ȸ���

                i += tl.Length;
                Array.Copy(s, 0, buffer, i, s.Length);  //squence number ����

                i += s.Length;
                Array.Copy(ct, 0, buffer, i, ct.Length);  // cameralist type����

                i += ct.Length;
                Array.Copy(cl, 0, buffer, i, cl.Length);  //cameralist  length����

                i += cl.Length;
                Array.Copy(c_value, 0, buffer, i, c_value.Length); //cameralist value�ĸ���

                Boolean b = SendMsg1(buffer);

                Thread.Sleep(1000);

                if (SendMsg1(buffer)==false)
                {
                    MessageBox.Show("�鿴��Ƶͼ��ʧ�ܣ������ؿͻ����Ƿ�򿪣�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }               
            }
            catch(Exception ex)
            {
                ExToLog(ex, "OpenVideo-29-����cameraid");
            }
        }

        /// <summary>
        /// �ж���Ƶ��ش��ڽ����Ƿ��Ѿ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <returns>����ֵ(true-�� false-�ر�)</returns>
        public Boolean IsOpenVideoClient()
        {
            try
            {
                string MyModuleName = "surveillance1.exe";
                string MyProcessName = System.IO.Path.GetFileNameWithoutExtension(MyModuleName);
                Process[] MyProcesses = Process.GetProcessesByName(MyProcessName);
                if (MyProcesses.Length >= 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "IsOpenVideoClient-29-����cameraid");
                return false;
            }
        }

        /// <summary>
        /// ������ض˳���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void OpenVideoClient()
        {
            try
            {             
                Boolean vf = IsOpenVideoClient();
                if (vf == true)
                {
                    OpenAgentClient();

                    System.Diagnostics.Process process = new System.Diagnostics.Process();

                    int index = VideoClient.LastIndexOf('\\');
                    if (index != -1)
                    {
                        string dir = VideoClient.Substring(0, index);
                        process.StartInfo.WorkingDirectory = dir;

                        string VideoArg = string.Empty;
                        for (int i = 0; i < VideoConnectString.Length; i++)
                        {
                            VideoArg += VideoConnectString[i];
                            VideoArg += " ";
                        }

                        process.StartInfo.Arguments = VideoArg;
                        process.StartInfo.FileName = VideoClient;
                        process.Start();
                    }
                    WriteEditLog("��Ƶ���", "", VideoClient, "����Ƶ��ؿͻ���");
                }
                else
                {
                    MessageBox.Show("��Ƶ��ض���������", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("������ض˳���ʧ�ܣ��������ù����м�ض����ã�", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ExToLog(ex, "OpenVideoClient-30-������ض˳���");
            }
        }

        /// <summary>
        /// ��CAgent.exe 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void OpenAgentClient()
        {
            try
            {
                KillProcess("CAgent.exe");

                System.Diagnostics.Process process = new System.Diagnostics.Process();

                string dir = "";
                dir = VideoClient.Remove(VideoClient.LastIndexOf("\\"));
                                
                process.StartInfo.WorkingDirectory = dir;
                process.StartInfo.FileName = dir + "\\CAgent.exe";
                process.Start();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "OpenAgentClient-31-����Agent����ʧ��");
            }
        }

        /// <summary>
        /// �رս���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ProcName">��������</param>
        private void KillProcess(string ProcName)
        {
            try
            {
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();

                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if (ProcName == myProcess.ProcessName)
                        myProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "KillProcess");
            }
        }


        /// <summary>
        /// ��ʾ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void ShowDoInfo(string str)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    DecshowMessage dc = new DecshowMessage(ShowDoInfo);
                    this.BeginInvoke(dc, new object[] { str });

                }
                else
                    StatusLabel.Text = "��Ϣ:" + str;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ShowDoInfo");
            }
        }
        delegate void DecshowMessage(string str);
       
        /// <summary>
        /// 
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {            
            AddCarViedoFtr(); 
        }

        private void ucVideo_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    this.StopVideo();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ucVideo_VisibleChanged");
            }
        }

        /// <summary>
        /// �����Ƶ��ص���ʱͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void ClearVideoTemp()
        {
            try
            {
                clearFeatures("VideoCarLayer");
                clearFeatures("VideoLayer");
                this.SpeVideoArray.Clear();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ClearVideoTemp-34-�����Ƶ��ص���ʱͼ��");
            }
        }

        /// <summary>
        /// ���ͼ��ͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="tabAlias">ͼ����</param>
        private void clearFeatures(string tabAlias)
        {
            try
            {
                //�����ͼ����ӵĶ���
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers[tabAlias];
                if (fl == null)
                {
                    return;
                }
                Table tableTem = fl.Table;

                //��������ж���
                (tableTem as IFeatureCollection).Clear();
                tableTem.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "clearFeatures-35-���ͼ��ͼԪ");
            }
        }


        /// <summary>
        /// ��Ƶ��ز�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        public void videoset()
        {
            try
            {
                frmvideoset fs = new frmvideoset();
                fs.txtfolder.Text = VideoConnectString[0];
                fs.txtip.Text = VideoConnectString[1];
                fs.txtport.Text = VideoConnectString[2];
                fs.txtusername.Text = VideoConnectString[3];
                fs.txtpswd.Text =VideoConnectString[4];
                fs.txtClient.Text = VideoClient.Trim();    

                fs.ShowDialog(this);

                if (fs.DialogResult == DialogResult.OK)
                {
                    VideoConnectString[0] = fs.txtfolder.Text.Trim();
                    VideoConnectString[1] = fs.txtip.Text.Trim();
                    VideoConnectString[2] = fs.txtport.Text.Trim();
                    VideoConnectString[3] = fs.txtusername.Text.Trim();
                    VideoConnectString[4] =  md5(fs.txtpswd.Text.Trim());
                    VideoClient = fs.txtClient.Text.Trim();

                    CLC.INIClass.IniPathSet(Application.StartupPath.Remove(Application.StartupPath.LastIndexOf("\\")) + "\\config.ini");
                    CLC.INIClass.IniWriteValue("��Ƶ��", "�ļ���", VideoConnectString[0]);
                    CLC.INIClass.IniWriteValue("��Ƶ��", "ip", VideoConnectString[1]);
                    CLC.INIClass.IniWriteValue("��Ƶ��", "�˿�", VideoConnectString[2]);
                    CLC.INIClass.IniWriteValue("��Ƶ��", "�û���", VideoConnectString[3]);
                    CLC.INIClass.IniWriteValue("��Ƶ��", "����", VideoConnectString[4]);

                    CLC.INIClass.IniWriteValue("��Ƶ", "�ͻ���", VideoClient);  //jie.zhang 2010.1.4


                    CLC.INIClass.IniPathSet(Application.StartupPath + @"\ConfigBJXX.ini");
                    CLC.INIClass.IniWriteValue("��Ƶ���", "����", fs.txtpswd.Text.Trim());

                    MessageBox.Show("���ñ���ɹ�","ϵͳ��ʾ",MessageBoxButtons.OK,MessageBoxIcon.None);
                }
                else if (fs.DialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "videoset-35-��Ƶ��ز�������");

                MessageBox.Show("���ñ���ʧ��", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// MD5�����㷨
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="str">�����ַ���</param>
        /// <returns>���ܺ��ַ���</returns>
        public string md5(string str)
        {
            string cl = str;
            string pwd = string.Empty;
            try
            {
                MD5 md5 = MD5.Create();

                byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));

                for (int i = 0; i < s.Length; i++)
                {
                    pwd = pwd + s[i].ToString("x2");
                }
            }
            catch (Exception ex) { ExToLog(ex, "md5"); }
            return pwd;
        }

        /// <summary>
        /// ��Ƶ��ذ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void toolvideo_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                string Selectext = e.ClickedItem.Text;

                switch (Selectext)
                {
                    case "����":
                        videoset();
                        break;
                    case "�ͻ���":
                        OpenVideoClient();
                        break;
                    case "ͳ�Ʊ���":
                        FrmSheZhi frmsz = new FrmSheZhi();
                        Size size = new Size(337, 102);
                        frmsz.Size = size;
                        frmsz.strConn = mysqlstr;
                        frmsz.Show();
                        break;
                    case "��ѡ":
                        mapControl1.Tools.LeftButtonTool = "SelectRect";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex) { ExToLog(ex, "toolvideo_DropDownItemClicked"); }
        }

        /// <summary>
        /// ��ȡ��ѯ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="sql">Ҫִ�е�SQL</param>
        /// <returns>�����</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ����SQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="sql"></param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }       

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clVideo-ucVideo-" + sFunc);
        }

        /// <summary>
        /// ��¼������¼ 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="sModule">����ģ��</param>
        /// <param name="tName">�������ݿ����</param>
        /// <param name="sql">����sql���</param>
        /// <param name="method">��������</param>
        private void WriteEditLog(string sModule, string tName, string sql, string method)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'��Ƶ���:" + sModule + "','" + tName + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch
            {
            }
        }

        private void CaseSearchBox_TextChanged(object sender, EventArgs e)
        {
            //try
            //{

            //    string keyword = this.CaseSearchBox.Text.Trim();
            //    string colfield = this.comboBox1.Text.Trim();
            //    string Tablename = string.Empty;

            //    if (keyword.Length < 1 || colfield.Length < 1) return;

            //    string strExp = string.Empty;

            //    switch (colfield)
            //    {
            //        case "�����ɳ���":
            //            strExp = "select " + colfield + " from " + Tablename;
            //            break;
            //        default:
            //            strExp = "select " + colfield + " from ��Ƶλ��VIEW t  where " + colfield + " like '" + keyword + "%' union all select " + colfield + " from ��Ƶλ��VIEW t  where " + colfield + " like '%" + keyword + "%' and " + colfield + " not like '" + keyword + "%' and " + colfield + " not like '%" + keyword + "' union all select " + colfield + " from ��Ƶλ��VIEW t where " + colfield + " like '%" + keyword + "'";
            //            break;
            //    }

            //    DataTable dt = GetTable(strExp);
            //    this.CaseSearchBox.GetSpellBoxSource(dt);

            //}
            //catch (Exception ex)
            //{
            //    ExToLog(ex, "35-�����ʾ����");
            //}
        }

        /// <summary>
        /// �Զ�ƥ�书��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                string keyword = this.CaseSearchBox.Text.Trim();
                string colfield = this.comboBox1.Text.Trim();

                if (colfield == "�����ɳ���")
                {
                    string strExp = "select distinct(�ɳ�����) from �����ɳ��� order by �ɳ�����" ;
                    DataTable dt = GetTable(strExp);
                    this.CaseSearchBox.GetSpellBoxSource(dt);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "CaseSearchBox_MouseDown-35-�����ʾ����");
            }
        }

        /// <summary>
        /// �ı��򰴼��¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void CaseSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        SearchVideo();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "CaseSearchBox_KeyDown");
            }
        }

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
            catch(Exception ex)
            {
                ExToLog(ex, "isShowPro");
            }
        }


        frmrecord frecord = new frmrecord();

        /// <summary>
        /// �һ���Ƶ���ذ�ť
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void menurecord_Click(object sender, EventArgs e)
        {
            try
            {
                string camid = GetIdFromName();

                this.DownRecord(camid);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "menurecord_Click");
            }
        }

        /// <summary>
        /// ��Ƶ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="Camid">��ƵID</param>
        public void DownRecord(string Camid)
        {
            try
            {
                string rfolder = VideoConnectString[0];
                string rip = VideoConnectString[1];
                string rport = VideoConnectString[2];
                string rname = VideoConnectString[3];

                CLC.INIClass.IniPathSet(Application.StartupPath + @"\ConfigBJXX.ini");
                string rpsw = CLC.INIClass.IniReadValue("��Ƶ���", "����");
                if (rpsw == "")
                {
                    MessageBox.Show("��ȷ����Ƶ��ص�¼����", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    frmvideoset fs = new frmvideoset();
                    fs.txtfolder.Text = VideoConnectString[0];
                    fs.txtip.Text = VideoConnectString[1];
                    fs.txtport.Text = VideoConnectString[2];
                    fs.txtusername.Text = VideoConnectString[3];
                    fs.txtpswd.BackColor = System.Drawing.Color.Red;
                    fs.txtClient.Text = VideoClient.Trim();

                    fs.ShowDialog(this);

                    if (fs.DialogResult == DialogResult.OK)
                    {
                        VideoConnectString[0] = fs.txtfolder.Text.Trim();
                        VideoConnectString[1] = fs.txtip.Text.Trim();
                        VideoConnectString[2] = fs.txtport.Text.Trim();
                        VideoConnectString[3] = fs.txtusername.Text.Trim();
                        VideoConnectString[4] = md5(fs.txtpswd.Text.Trim());
                        VideoClient = fs.txtClient.Text.Trim();

                        rpsw = fs.txtpswd.Text.Trim();

                        CLC.INIClass.IniWriteValue("��Ƶ���", "����", fs.txtpswd.Text.Trim());



                        CLC.INIClass.IniPathSet(Application.StartupPath.Remove(Application.StartupPath.LastIndexOf("\\")) + "\\config.ini");
                        CLC.INIClass.IniWriteValue("��Ƶ��", "�ļ���", VideoConnectString[0]);
                        CLC.INIClass.IniWriteValue("��Ƶ��", "ip", VideoConnectString[1]);
                        CLC.INIClass.IniWriteValue("��Ƶ��", "�˿�", VideoConnectString[2]);
                        CLC.INIClass.IniWriteValue("��Ƶ��", "�û���", VideoConnectString[3]);
                        CLC.INIClass.IniWriteValue("��Ƶ��", "����", VideoConnectString[4]);

                        CLC.INIClass.IniWriteValue("��Ƶ", "�ͻ���", VideoClient);
                    }
                    else
                    {
                        return;
                    }
                }

                if (rfolder != "" && rip != "" && rport != "" && rname != "" && rpsw != "" && Camid != "")
                {
                    frecord.Visible = true;
                    frecord.TopMost = true;


                    frecord.Initial(rip, rport, rfolder, rname, rpsw, Camid, StrCon, user);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "DownRecord");
            }
        }

        /// <summary>
        /// ��ȡ��Ƶ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <returns>��Ƶ���</returns>
        private string GetIdFromName()
        {
            try
            {
                DataTable dt = this.GetTable("Select �豸��� from ��Ƶλ��view t where t.�豸����='" + this.NowVideoName + "'");
                if (dt.Rows.Count > 0)
                {
                    string camid = dt.Rows[0]["�豸���"].ToString();
                    return camid;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetIdFromName");
                return "";
            }
        }

        private DataTable dataVideo;
        private frmDisplay fmDis;

        /// <summary>
        /// �Ŵ�鿴����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void btnEnal_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataVideo == null)
                {
                    System.Windows.Forms.MessageBox.Show("������չʾ����ѡ��ѯ�����ݺ�Ŵ�鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                fmDis = new frmDisplay(dataVideo);

                fmDis.dataGridDisplay.CellDoubleClick += this.gvVideo_DoubleClick;
                fmDis.dataGridDisplay.SelectionChanged += this.gvVideo_SelectionChanged;

                fmDis.Show();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnEnal_Click");
            }
        }

        /// <summary>
        /// ������־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="funName">������</param>
        /// <param name="message">ִ�д���</param>
        private void WriteLog(string funName,string message)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\videoTestLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine("��������" + funName + "  ��   ִ�д��룺" + message);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            { ExToLog(ex, "WriteLog"); }
        }


        #region ˫���¼���������ʾ��ϸ��Ϣ��������
        //private void gvVideo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex == -1)
        //        return;
        //    try
        //    {
        //        DPoint dp = new DPoint();
        //        CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
        //        string sql = "select * from ��Ƶλ�� where �豸����='" + gvVideo.Rows[e.RowIndex].Cells[0].Value.ToString() + "'";
        //        DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

        //        dp.x = Convert.ToDouble(dt.Rows[0]["X"]);
        //        dp.y = Convert.ToDouble(dt.Rows[0]["Y"]);
        //        if (dp.x == 0 || dp.y == 0)
        //        {
        //            System.Windows.Forms.MessageBox.Show("�˶���δ��λ!");
        //            return;
        //        }

        //        System.Drawing.Point pt = new System.Drawing.Point();
        //        pt.X = Convert.ToInt32(dp.x);
        //        pt.Y = Convert.ToInt32(dp.y);

        //        disPlayInfo(dt, pt);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLC.BugRelated.ExceptionWrite(ex, "gvVideo_CellContentDoubleClick");
        //    }
        //}

        //private FrmInfo frmMessage = new FrmInfo();
        //private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        //{
        //    try
        //    {
        //        if (this.frmMessage.Visible == false)
        //        {
        //            this.frmMessage = new FrmInfo();
        //            frmMessage.SetDesktopLocation(-30, -30);
        //            frmMessage.Show();
        //            frmMessage.Visible = false;
        //        }
        //        frmMessage.mapControl = mapControl1;
        //        frmMessage.getFromNamePath = getFromNamePath;
        //        frmMessage.strConn = mysqlstr;
        //        frmMessage.setInfo(dt.Rows[0], pt);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Console.WriteLine(ex.Message);
        //    }
        //}
        #endregion
    }
}