using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Mapping.Thematics;
using System.IO;
using MapInfo.Windows.Dialogs;
using System.Threading;

using LBSDataGuide;


namespace LBSgisPoliceEdit
{
    public partial class frmMap : Form
    {
        private string mysqlstr = "";
        string exePath;
        private bool isOracleSpatialTab = false;  //ʵΪ�Ƿ���OracleSpatial��
        private string datasource, userid, password;
        private AreaStyle aStyle; //���������Ҫ�ص�Ĭ����ʽ
        private Style featStyle;
        private string fileName = "";
        private string getFromNamePath = ""; // ��ȡ����GetFromNameConfig.ini�ļ���·����lili
        public string userName = "";        // ��ȡ��ǰ�û����û���  lili
        public string ZoomFile = "";        // ��ȡ��ͼ���ű�����ֵ lili
        //private string GafileName = "";//���ڹ������ݱ༭��fileName����  feng

        private DataTable _exportDT = null;//����ѯ�������ݵ���
        private DataTable Ga_exportDT = null;//�������༭��ѯ�������ݵ���
        private DataTable temEditDt = null; // �洢�༭ģ��Ȩ�ޡ�lili
        private Table zhongduiST = null;//��������Ӷ���ʱ�ж���������Ƿ���ȷ

        private string[] listPaichusuo=null;
        string strRegion = "";  //�����洢Ȩ�����ڵ��ɳ���
        string strRegion1 = ""; //�����洢Ȩ�����ڵ��ж�

        public frmMap(string region1,string region2,DataTable dt)   //���캯��
        {
            try
            {
                InitializeComponent();

                //Ƥ��
                //this.skinEngine1.SkinStream = new MemoryStream(Properties.Resources.DiamondBlue1);
                //this.skinEngine1.SkinAllForm = false;
                strRegion = region1;
                strRegion1 = region2;
                temEditDt = dt;

                //�������ݿ�
                exePath = Application.StartupPath;
                getFromNamePath = exePath + "\\GetFromNameConfig.ini";
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");
                mysqlstr = "data source=" + datasource + ";user id=" + userid + ";password=" + password;
                listPaichusuo = getPaichusuo();
                InitialMap();
                comboTables.Text = comboTables.Items[0].ToString();
                setPrivilege();//����Ȩ��
                CreateEagleLayer();//������ʱ����ӥ�۵ľ��ο�
                //settabGa();//���ù���ҵ��༭�ĳ�ֵ  feng

                //��ӻ������ж�ͼ�㣬�����ж���ӵ����������  ��fisher in 09-11-30��
                addMJZD();

                this.mapControl1.Map.ViewChangedEvent += new ViewChangedEventHandler(mapControl1_ViewChanged);
                this.mapControl1.Tools.FeatureChanging += new FeatureChangingEventHandler(Tools_FeatureChanging);
                this.mapControl1.Tools.FeatureChanged += new MapInfo.Tools.FeatureChangedEventHandler(Feature_Changed);//��ͼ�༭ʱ���õĺ���
                this.mapControl1.Tools.FeatureSelected += new FeatureSelectedEventHandler(Feature_Selected);//��ͼѡ��ʱ���õĺ���
                
                this.mapControl1.Tools.FeatureAdded += new FeatureAddedEventHandler(Tools_FeatureAdded);
                this.mapControl1.Tools.Used += new ToolUsedEventHandler(Tools_Used);
                this.mapControl1.Tools.NodeChanged += new NodeChangedEventHandler(Tools_NodeChanged);
                this.dataGridView1.DataError+=new DataGridViewDataErrorEventHandler(dataGridView1_DataError);

                //����Զ��嶨λ����
                MapInfo.Tools.MapTool ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, this.mapControl1.Handle.ToInt32(), this.mapControl1.Tools, this.mapControl1.Tools.MouseToolProperties, this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("Location", ptMapTool);

                //��ʼ��Ĭ�ϱ�ı༭�ֶ�
                //this.InitialEditFields(comboTables.Text);//�����������ʹ��Ҫ������ֶ��õ�
                this.mapToolBar1.Buttons["mapToolAddpolygon"].Visible = false;

                //��ʼ������ʽ������ʽ
                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(1);
                this.aStyle = new AreaStyle(simLineStyle, simInterior);
                this.featStyle = setFeatStyle(comboTables.Text);

                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                
                string info = "Ȩ�޷�ΧΪ: ";
                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1"&&temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString()=="1"&&temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString()=="1")                
                {
                    info += "ȫ���༭Ȩ��"; 
                }
                else
                {
                    info += "ֻ�в������ݱ༭Ȩ��";
                }
                toolStatusInfo.Text = info;
                toolStatusUser.Text = "�û�: " + temEditDt.Rows[0]["userNow"].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex,"���캯��");
            }
        }

        //������ж�Ͻ����ͼ (add by fisher in 09-12-01)
        private void addMJZD()
        {
            MIConnection miConnection = new MIConnection();
            try
            {
                miConnection.Open();
                if (miConnection.Catalog.GetTable("�ж�Ͻ��") == null)
                {
                    TableInfoServer ti = new TableInfoServer("�ж�Ͻ��");
                    string str1 = mysqlstr.Replace("data source", "SRVR").Replace("user id", "UID").Replace("password", "PWD");
                    ti.ConnectString = str1;
                    string strSQL = "Select * From ���ж�Ͻ��";
                    ti.Query = strSQL;
                    ti.Toolkit = ServerToolkit.Oci;
                    ti.CacheSettings.CacheType = CacheOption.Off;
                    zhongduiST = miConnection.Catalog.OpenTable(ti);
                    //���ͼ�㵽layers������ͼ�㲻�ɼ�
                    int t = mapControl1.Map.Layers.Add(new FeatureLayer(zhongduiST));
                    mapControl1.Map.Layers[t].Enabled = false;
                    //MapInfo.Mapping.LayerHelper.SetSelectable(
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "addMJZD");
                MessageBox.Show("�����ж�Ͻ������:" + ex.Message);
            }
            finally
            {
                if (miConnection.State == ConnectionState.Open)
                    miConnection.Close();
            }
        }

        void Tools_FeatureChanging(object sender, FeatureChangingEventArgs e)
        {
            try
            {
                if (e.FeatureChangeMode == FeatureChangeMode.Delete)
                {
                    //if (comboTables.Text == "��Ƶλ��") {  //��Ƶλ�ò�����ɾ��
                    if (tabControl2.SelectedTab == tabPage2)
                    {  //��Ƶλ�ò�����ɾ��   edit by fisher in 09-12-10
                        e.Cancel = true;
                        return;
                    }
                    if (MessageBox.Show("ȷ��ɾ��?", "ȷ��", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_FeatureChanging");
            }
        }

        //������нڵ�༭ʱ
        private void Tools_NodeChanged(object sender, NodeChangedEventArgs e)
        {
            try
            {
                //�ҵ��ƶ����Ҫ��
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + selPrinx);
                si.QueryDefinition.Columns = null;
                Feature f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);

                //�ҵ�oracleSpatial���ж�Ӧ��Ҫ��
                si = MapInfo.Data.SearchInfoFactory.SearchWhere("MI_PRINX=" + selPrinx);
                si.QueryDefinition.Columns = null;
                Feature newFeat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(lTable.Alias, si);

                //�½�����,ΪԭҪ�ص�����+�ƶ����geometry
                Feature addFeat = newFeat;
                addFeat.Geometry = f.Geometry;

                //��ɾ��ԭ����Ҫ��,�������λ�õ�Ҫ��
                lTable.DeleteFeature(newFeat);
                lTable.InsertFeature(addFeat);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_NodeChanged");
            }
        }

        private void setPrivilege()    //������������Ȩ��
        {
            try
            {
                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() != "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text], false);
                    //this.dataGridViewVideo.ReadOnly = true;   //������Ƶ�����ϢΪֻ��
                }                
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setPrivilege");
            }
        }

        private bool isUse = false;
        void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            if (e.ToolStatus == ToolStatus.Start)
            {
                isUse = true;
            }
            if (e.ToolStatus == ToolStatus.End)
            {
                isUse = false;
            }
            try
            {
                if (e.ToolName == "Location")
                {
                    //added by fisher(09-09-03)
                    if (this.tabControl2.SelectedTab == tabPage1)  //����ҵ������
                    {
                        if (dataGridViewList.CurrentCell == null)
                        {
                            MessageBox.Show("��ѡ����Ҫ��λ�ĵ�!", "��ʾ");
                            btnLoc3.Enabled = false;
                            return;
                        }

                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                this.tabControl1.SelectedTab = tabPageInfo;
                                Feature f;   //��ǰ��������ӵĶ���
                                Feature Jc_pref = null;   //������ɾ���Ķ���
                                if (this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value.ToString() != "" && this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value.ToString() != "")
                                {
                                    if (Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value) != 0)
                                    {
                                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value));
                                        si.QueryDefinition.Columns = null;
                                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                                        if (f != null)
                                        {
                                            Jc_pref = f;
                                            this.editTable.DeleteFeature(f);
                                        }
                                        //��ɾ��
                                    }
                                }

                                //��ӵ�
                                CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                                string sName = CLC.ForSDGA.GetFromTable.ObjName;
                                string sobjID = CLC.ForSDGA.GetFromTable.ObjID;   //Ĭ�ϵ�����

                                f = new Feature(this.editTable.TableInfo.Columns);
                                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(e.MapCoordinate.x, e.MapCoordinate.y));

                                if (dataGridViewList.Rows[browIndex].Cells["mapid"].Value == null || dataGridViewList.Rows[browIndex].Cells["mapid"].Value.ToString() == "")
                                {
                                    OracleConnection conn = new OracleConnection(mysqlstr);
                                    try
                                    {
                                        conn.Open();
                                        //���´�����fisher��09-09-22�ո��£������¶�λһ��û��XY������Ϣ�ļ�¼ʱ������ü�¼��mapidΪ�գ�
                                        //�����ȸ�����mapid����������ĸ��³�������
                                        if (isOracleSpatialTab == false)
                                        {
                                            OracleCommand cmd = new OracleCommand("select max(mapid) from " + comboTables.Text.ToString(), conn);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            int objId3 = 0;
                                            if (dr.HasRows)
                                            {
                                                dr.Read();
                                                objId3 = Convert.ToInt32(dr.GetValue(0)) + 1;
                                                selMapID = Convert.ToString(objId3);   //��ȫ�ֱ�����ֵ���������ݵĸ���
                                            }
                                            f["mapid"] = objId3.ToString();
                                            dataGridViewList.Rows[browIndex].Cells["mapid"].Value = objId3;
                                            dr.Close();
                                            cmd.Dispose();
                                            cmd = new OracleCommand("update " + comboTables.Text + " t set t.mapid = " + objId3 + " where " + sobjID + " = " + dataGridViewList.Rows[browIndex].Cells[sobjID].Value.ToString(), conn);
                                            cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        writeToLog(ex, "Tools_Used-��ӵ�");
                                    }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open)
                                            conn.Close();
                                    }
                                }
                                else
                                {
                                    f["mapid"] = this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value.ToString();
                                }
                                f["name"] = this.dataGridViewList.Rows[browIndex].Cells[sName].Value.ToString();
                                f.Style = featStyle;

                                //���ҵ�����ӵ�ķ�Χ���ж�Ȩ��   fisher in 09-12-31
                                SearchInfo Jc_si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                                Jc_si.QueryDefinition.Columns = null;
                                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", Jc_si);

                                //Ϊ��ʱ,����������������,�����
                                if (ft == null)
                                {
                                    this.editTable.InsertFeature(Jc_pref);
                                    this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = Jc_pref.Geometry.Centroid.x;
                                    this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = Jc_pref.Geometry.Centroid.y;
                                    MessageBox.Show("���ܽ������ƶ�����ͼ��!", "������ʾ");
                                }
                                else
                                {
                                    bool quyuCZ = false;  //�����ж���ӵĵ��ǲ��������������Ȩ�޷�Χ��
                                    if (strRegion != "˳����")
                                    {
                                        //���������ཻ,�������ڽ������ǲ����û���Χ
                                        if (Array.IndexOf(strRegion.Split(','), ft["name"].ToString()) > -1)
                                        {
                                            quyuCZ = true;
                                        }

                                        if (quyuCZ == false)   //������Ȩ������ķ�Χ���������
                                        {
                                            this.editTable.InsertFeature(Jc_pref);
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = Jc_pref.Geometry.Centroid.x;
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = Jc_pref.Geometry.Centroid.y;
                                            MessageBox.Show("���ܽ������ƶ�����Ȩ�޷�Χ���������!", "������ʾ");
                                        }
                                        else
                                        {
                                            this.editTable.InsertFeature(f);
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                            Jc_pref = f;
                                        }
                                    }
                                    else  //˳����Ȩ��
                                    {
                                        this.editTable.InsertFeature(f);
                                        this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                        this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                        Jc_pref = f;
                                    }
                                } 
                                break;
                            default:
                                break;
                        }
                    }
                    else if (this.tabControl2.SelectedTab == tabPage2)  //��Ƶ��λ
                    {
                        if (dataGridViewVideo.CurrentCell == null)
                        {
                            MessageBox.Show("��ѡ��λ����Ƶ!", "��ʾ");
                            return;
                        }
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                Feature f;   //��ǰ��������ӵĶ���
                                Feature f_pre=null;   //ǰһ������,��������ɾ���Ķ��� fisher 09-12-31
                                int iRow = dataGridViewVideo.CurrentCell.RowIndex;
                                //updated by fisher in 09-09-21
                                if (dataGridViewVideo.Rows[iRow].Cells["x"].Value.ToString() != "" && dataGridViewVideo.Rows[iRow].Cells["y"].Value.ToString() != "")
                                {
                                    if (Convert.ToDouble(dataGridViewVideo.Rows[iRow].Cells["x"].Value) != 0 && Convert.ToDouble(dataGridViewVideo.Rows[iRow].Cells["y"].Value) != 0)
                                    {
                                        //��ɾ��
                                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(dataGridViewVideo.Rows[iRow].Cells["mapid"].Value));
                                        si.QueryDefinition.Columns = null;
                                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                                        if (f != null)
                                        {
                                            f_pre = f;
                                            this.editTable.DeleteFeature(f);
                                        }
                                    }
                                }
                               
                                //��ӵ�
                                f = new Feature(this.editTable.TableInfo.Columns);
                                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(e.MapCoordinate.x, e.MapCoordinate.y));
                                f["name"] = dataGridViewVideo.Rows[iRow].Cells["�豸����"].Value.ToString();

                                if (dataGridViewVideo.Rows[iRow].Cells["mapid"].Value == null || dataGridViewVideo.Rows[iRow].Cells["mapid"].Value.ToString() == "")
                                {
                                    OracleConnection conn = new OracleConnection(mysqlstr);
                                    try
                                    {
                                        conn.Open();
                                        OracleCommand cmd = new OracleCommand("select max(mapid) from ��Ƶλ��", conn);
                                        OracleDataReader dr = cmd.ExecuteReader();
                                        int videoId = 0;
                                        if (dr.HasRows)
                                        {
                                            dr.Read();
                                            videoId = Convert.ToInt32(dr.GetValue(0)) + 1;
                                        }
                                        f["mapid"] = videoId.ToString();
                                        dataGridViewVideo.Rows[iRow].Cells["mapid"].Value = videoId;
                                        dr.Close();
                                        cmd.Dispose();
                                    }
                                    catch (Exception ex)
                                    { writeToLog(ex, "Tools_Used-��Ƶ��ӵ�"); }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open)
                                            conn.Close();
                                    }
                                }
                                else
                                {
                                    f["mapid"] = dataGridViewVideo.Rows[iRow].Cells["mapid"].Value.ToString();
                                }
                                f.Style = new MapInfo.Styles.BitmapPointStyle("sxt.bmp");

                                //���ҵ�����ӵ�ķ�Χ���ж�Ȩ��   fisher in 09-12-31
                                SearchInfo newsi = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                                newsi.QueryDefinition.Columns = null;
                                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", newsi);

                                //Ϊ��ʱ,����������������,�����
                                if (ft == null)
                                {
                                    this.editTable.InsertFeature(f_pre);
                                    dataGridViewVideo.Rows[iRow].Cells["x"].Value = f_pre.Geometry.Centroid.x;
                                    dataGridViewVideo.Rows[iRow].Cells["y"].Value = f_pre.Geometry.Centroid.y;
                                    MessageBox.Show("���ܽ������ƶ�����ͼ��!", "������ʾ");
                                }
                                else
                                {
                                    bool quyuCZ = false;  //�����ж���ӵĵ��ǲ��������������Ȩ�޷�Χ��
                                    if (strRegion != "˳����")
                                    {
                                        //���������ཻ,�������ڽ������ǲ����û���Χ
                                        if (Array.IndexOf(strRegion.Split(','), ft["name"].ToString()) > -1)
                                        {
                                            quyuCZ = true;
                                        }

                                        if (quyuCZ == false)   //������Ȩ������ķ�Χ���������
                                        {
                                            this.editTable.InsertFeature(f_pre);
                                            dataGridViewVideo.Rows[iRow].Cells["x"].Value = f_pre.Geometry.Centroid.x;
                                            dataGridViewVideo.Rows[iRow].Cells["y"].Value = f_pre.Geometry.Centroid.y;
                                            MessageBox.Show("���ܽ������ƶ�����Ȩ�޷�Χ���������!", "������ʾ");
                                        }
                                        else
                                        {
                                            this.editTable.InsertFeature(f);
                                            dataGridViewVideo.Rows[iRow].Cells["x"].Value = e.MapCoordinate.x;
                                            dataGridViewVideo.Rows[iRow].Cells["y"].Value = e.MapCoordinate.y;
                                            f_pre = f;
                                        }
                                    }
                                    else  //˳����Ȩ��
                                    {
                                        this.editTable.InsertFeature(f);
                                        dataGridViewVideo.Rows[iRow].Cells["x"].Value = e.MapCoordinate.x;
                                        dataGridViewVideo.Rows[iRow].Cells["y"].Value = e.MapCoordinate.y;
                                        f_pre = f;
                                    }
                                }                                
                                break;
                            default:
                                break;
                        }
                    }

                    //added by fisher (09-09-03)
                    else if (this.tabControl2.SelectedTab == tabPage3)   //����ҵ������
                    {
                        if (dataGridViewGaList.CurrentCell == null)
                        {
                            MessageBox.Show("��ѡ����Ҫ��λ�ĵ�!", "��ʾ");
                            return;
                        }
                        
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                this.tabControl3.SelectedTab = tabGaInfo;

                                // by fisher on 09-09-23
                                //�����MapInfo�ı��ڱ���û��XY��Ϣʱ����ɾ��������¼��Ȼ����ӣ�������Ӧ��XY��ֵΪ0

                                if (this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() == "" && this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() == "")
                                {
                                    if (isOracleSpatialTab)
                                    {
 
                                    }
                                }

                                Feature f;     //��ǰ��������ӵĶ���
                                Feature J_pref=null;  //��ǰ������ɾ���Ķ���      add by fisher in 09-12-31

                                //if (dataGridViewGaInfo.Rows[rowIndex].Cells["x"].Value.ToString() != "" && dataGridViewGaInfo.Rows[rowIndex].Cells["y"].Value.ToString() != "")
                                if (this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() != "" && this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() != "")
                                {
                                    if (Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value) != 0)
                                    {
                                        //if (isOracleSpatialTab)
                                        //{
                                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value));
                                        si.QueryDefinition.Columns = null;
                                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                                        if (f != null)
                                        {
                                            J_pref = f;
                                            this.editTable.DeleteFeature(f);
                                        }
                                            //��ɾ��
                                        //}
                                        //else
                                        //{
                                        //    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("���='" + this.dataGridViewGaList.Rows[rowIndex].Cells["���"].Value.ToString() + "'");
                                        //    si.QueryDefinition.Columns = null;
                                        //    f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                                        //    if (f != null)
                                        //    {
                                        //        J_pref = f;
                                        //        this.editTable.DeleteFeature(f);
                                        //    }
                                        //}
                                    }
                                }

                                //��ӵ�
                                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                                string sName = CLC.ForSDGA.GetFromTable.ObjName;
                                string sobjID = CLC.ForSDGA.GetFromTable.ObjID;

                                f = new Feature(this.editTable.TableInfo.Columns);
                                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(e.MapCoordinate.x, e.MapCoordinate.y));

                                if (dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value == null || dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value.ToString() == "")
                                {
                                    OracleConnection conn = new OracleConnection(mysqlstr);
                                    try
                                    {
                                        conn.Open();
                                        //���´�����fisher��09-09-22�ո��£������¶�λһ��û��XY������Ϣ�ļ�¼ʱ������ü�¼��mapidΪ�գ�
                                        //�����ȸ�����mapid����������ĸ��³�������
                                        if (isOracleSpatialTab)
                                        {
                                            OracleCommand cmd = new OracleCommand("select max(MI_PRINX) from " + comboTable.Text.ToString(), conn);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            int objId1 = 0;
                                            if (dr.HasRows)
                                            {
                                                dr.Read();
                                                objId1 = Convert.ToInt32(dr.GetValue(0)) + 1;
                                                selPrinx = objId1;   //��ȫ�ֱ�����ֵ���������ݵĸ���
                                            }
                                            f["mapid"] = objId1.ToString();
                                            dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value = objId1;
                                            dr.Close();
                                            cmd.Dispose();
                                            cmd = new OracleCommand("update " + comboTable.Text + " t set t.MI_PRINX = " + objId1 + " where " + sobjID + " = " + dataGridViewGaList.Rows[rowIndex].Cells[sobjID].Value.ToString(), conn);
                                            cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                        }
                                        else
                                        {
                                            OracleCommand cmd = new OracleCommand("select max(mapid) from " + comboTable.Text.ToString(), conn);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            int objId2 = 0;
                                            if (dr.HasRows)
                                            {
                                                dr.Read();
                                                objId2 = Convert.ToInt32(dr.GetValue(0)) + 1;
                                                selMapID = Convert.ToString(objId2);   //��ȫ�ֱ�����ֵ���������ݵĸ���
                                            }
                                            f["mapid"] = objId2.ToString();
                                            dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value = objId2;
                                            dr.Close();
                                            cmd.Dispose();
                                            cmd = new OracleCommand("update " + comboTable.Text + " t set t.mapid = " + objId2 + " where " + sobjID + " = " + dataGridViewGaList.Rows[rowIndex].Cells[sobjID].Value.ToString(), conn);
                                            cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    { writeToLog(ex, "Tools_Used-ҵ��������ӵ�"); }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open)
                                            conn.Close();
                                    }
                                }
                                else
                                {
                                    f["mapid"] = dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value.ToString();
                                    selPrinx = Convert.ToInt32(f["mapid"]);
                                }

                                f["name"] = this.dataGridViewGaList.Rows[rowIndex].Cells[sName].Value.ToString();
                                f.Style = featStyle;

                                //���ҵ�����ӵ�ķ�Χ���ж�Ȩ��   fisher in 09-12-31
                                SearchInfo J_si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                                J_si.QueryDefinition.Columns = null;
                                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������",J_si);

                                //Ϊ��ʱ,����������������,�����
                                if (ft == null)
                                {
                                    this.editTable.InsertFeature(J_pref);
                                    this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = J_pref.Geometry.Centroid.x;
                                    this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = J_pref.Geometry.Centroid.y;
                                    MessageBox.Show("���ܽ������ƶ�����ͼ��!", "������ʾ");
                                }
                                else
                                {
                                    bool quyuCZ = false;  //�����ж���ӵĵ��ǲ��������������Ȩ�޷�Χ��
                                    if (strRegion != "˳����")
                                    {
                                        //���������ཻ,�������ڽ������ǲ����û���Χ
                                        if (Array.IndexOf(strRegion.Split(','), ft["name"].ToString()) > -1)
                                        {
                                            quyuCZ = true;
                                        }

                                        if (quyuCZ == false)   //������Ȩ������ķ�Χ���������
                                        {
                                            this.editTable.InsertFeature(J_pref);
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = J_pref.Geometry.Centroid.x;
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = J_pref.Geometry.Centroid.y;
                                            MessageBox.Show("���ܽ������ƶ�����Ȩ�޷�Χ���������!", "������ʾ");
                                        }
                                        else
                                        {
                                            this.editTable.InsertFeature(f);
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                            J_pref = f;
                                        }
                                    }
                                    else  //˳����Ȩ��
                                    {
                                        this.editTable.InsertFeature(f);
                                        this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                        this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                        J_pref = f;
                                    }
                                }                                 
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            catch(Exception ex) {
                writeToLog(ex, "Tools_Used");
            }
        }

        private double dx = 0, dy = 0;
        private Feature ft = null;   //����ӵĵ�ͼ������߱�ѡ�еĶ���
        private Feature feature = null;  //oracleSpatial���ж�Ӧ�ڵ�ͼ���ϵĶ���

        //2009-08-20 update by feng
        private void Tools_FeatureAdded(object sender, FeatureAddedEventArgs e)
        {
            try
            {
                if (tabControl2.SelectedTab == tabPage3)
                {
                    string tabName = comboTable.Text.Trim();
                    if (isOracleSpatialTab)
                    {
                        tabName += "_tem";
                    }
                    setTabInsertable(tabName, false); //�ҵ�Ҫ�����ͼ��

                    double gx = 0, gy = 0;  //added by fisher(09-09-02)
                    int w = 0;     // ��ȡ������˵���λ�� lili
                    if (e.Feature.Type == GeometryType.Point)
                    {
                        try
                        {
                            //�ҵ�����ӵĶ���
                            SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(e.Feature, ContainsType.Geometry);
                            si.QueryDefinition.Columns = null;
                            ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tabName, si);

                            //�ҵ���Ӷ�����������
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft1 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", si);

                            //�ҵ���Ӷ��������ж�Ͻ��  (���´�����fisher���  09-12-1)
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("�ж�Ͻ��", si);
                            string zdmc = "";  //�ж�����

                            //���´��뷵������ӵ����ڵ��ж�����
                            if (ft2 != null)
                            {
                                string zddm = ft2["�жӴ���"].ToString(); //�жӴ���
                                OracleConnection conn = new OracleConnection(mysqlstr);
                                try
                                {
                                    conn.Open();
                                    OracleCommand cmd = new OracleCommand("select �ж��� from �������ж� where �жӴ��� = " + zddm, conn);
                                    OracleDataReader dr = cmd.ExecuteReader();
                                    if (dr.HasRows)
                                    {
                                        dr.Read();
                                        zdmc = dr.GetValue(0).ToString();
                                    }
                                    dr.Close();
                                    cmd.Dispose();
                                    conn.Close();
                                }
                                catch (Exception ex)
                                {
                                    writeToLog(ex, "Tools_FeatureAdded");
                                    conn.Close();
                                }
                            }

                            //Ϊ��ʱ,����������������,�����
                            if (ft1 == null)
                            {
                                editTable.DeleteFeature(ft);
                                setTabInsertable(tabName, true);
                                return;
                            }
                            else
                            {
                                bool quyuCZ = false;  //�����ж���ӵĵ��ǲ������û������Ȩ�޷�Χ��
                                if (strRegion != "" || strRegion1 != "")
                                {
                                    if (strRegion != "˳����")
                                    {
                                        if (strRegion != "")
                                        {
                                            //���������ཻ,�������ڽ������ǲ����û���Χ
                                            if (Array.IndexOf(strRegion.Split(','), ft1["name"].ToString()) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (strRegion1 != "")
                                        {
                                            if (Array.IndexOf(strRegion1.Split(','), zdmc) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (quyuCZ == false)   //������Ȩ������ķ�Χ���������
                                        {
                                            editTable.DeleteFeature(ft);
                                            setTabInsertable(tabName, true);
                                            return;
                                        }
                                    }
                                }
                            }

                            if (isOracleSpatialTab)
                            {
                                ft["MapID"] = -1;
                                gx = e.Feature.Centroid.x;
                                gy = e.Feature.Centroid.y;
                            }
                            else
                            {
                                dx = e.Feature.Centroid.x;
                                dy = e.Feature.Centroid.y;

                                gx = e.Feature.Centroid.x;
                                gy = e.Feature.Centroid.y;
                            }
                            ft.Style = featStyle;
                            ft.Update();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Tools_FeatureAdded");
                            MessageBox.Show(ex.Message, "GaTools_FeatureAdded000");
                        }
                    }
                    try
                    {
                        for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                        {
                            switch (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString())
                            {
                                case "X":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = gx;
                                    break;
                                case "Y":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = gy;
                                    break;
                                case "��������":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = userName;
                                    w = i;
                                    break;
                                case "��ע��":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = userName;
                                    break;
                                default:
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                                    fileName = "";
                                    break;
                            }
                        }
                        if (comboTable.Text == "��ȫ������λ")
                        {
                            dataGridViewGaInfo.Rows.Remove(dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1]);
                        }
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "Tools_FeatureAdded");
                        MessageBox.Show(ex.Message, "Tools_FeatureAdded(������겻�ɹ�)");
                    }
                    tabControl3.SelectedTab = tabGaInfo;
                    btnGaSave.Text = "����";
                    btnGaCancel.Text = "ȡ�����";
                    dataGridViewGaInfo.Visible = true;
                    btnGaSave.Enabled = false;
                    btnGaCancel.Enabled = true;
                    dataGridViewGaInfo.Columns[1].ReadOnly = false;
                    dataGridViewGaInfo.Rows[w].Cells[1].ReadOnly = true;   // �������˲������޸�  lili 2010-9-27
                }
                else
                {
                    string tabName = comboTables.Text.Trim();
                    if (isOracleSpatialTab)
                    {
                        tabName += "_tem";
                    }
                    setTabInsertable(tabName, false);

                    if (e.Feature.Type == GeometryType.Point)
                    {
                        try
                        {
                            //�ҵ�����ӵĶ���
                            SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(e.Feature, ContainsType.Geometry);
                            si.QueryDefinition.Columns = null;
                            ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tabName, si);

                            //�ҵ���Ӷ�����������
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft1 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", si);

                            //�ҵ���Ӷ��������ж�Ͻ��  (���´�����fisher���  09-12-1)
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("�ж�Ͻ��", si);
                            string zdmc = "";  //�ж�����
                            if (ft2 != null)
                            {
                                string zddm = ft2["�жӴ���"].ToString();
                                OracleConnection conn = new OracleConnection(mysqlstr);
                                try
                                {
                                    conn.Open();
                                    OracleCommand cmd = new OracleCommand("select �ж��� from �������ж� where �жӴ��� = '" + zddm + "'", conn);
                                    OracleDataReader dr = cmd.ExecuteReader();
                                    if (dr.HasRows)
                                    {
                                        dr.Read();
                                        zdmc = dr.GetValue(0).ToString();
                                    }
                                    dr.Close();
                                    cmd.Dispose();
                                    conn.Close();
                                }
                                catch (Exception ex)
                                {
                                    writeToLog(ex, "Tools_FeatureAdded");
                                    conn.Close();
                                }
                            }

                            //Ϊ��ʱ,����������������,�����
                            if (ft1 == null)
                            {
                                editTable.DeleteFeature(ft);
                                setTabInsertable(tabName, true);
                                return;
                            }
                            else
                            {
                                bool quyuCZ = false;  //�����ж���ӵĵ��ǲ��������������Ȩ�޷�Χ��
                                if (strRegion != "" || strRegion1 != "")
                                {
                                    if (strRegion != "˳����")
                                    {
                                        if (strRegion != "")
                                        {
                                            //���������ཻ,�������ڽ������ǲ����û���Χ
                                            if (Array.IndexOf(strRegion.Split(','), ft1["name"].ToString()) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (strRegion1 != "")
                                        {
                                            if (Array.IndexOf(strRegion1.Split(','), zdmc) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (quyuCZ == false)
                                        {
                                            editTable.DeleteFeature(ft);
                                            setTabInsertable(tabName, true);
                                            return;
                                        }
                                    }
                                }
                            }

                            if (isOracleSpatialTab)
                            {
                                ft["MapID"] = -1;
                            }
                            else
                            {
                                dx = e.Feature.Centroid.x;
                                dy = e.Feature.Centroid.y;
                            }
                            ft.Style = featStyle;
                            ft.Update();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Tools_FeatureAdded");
                            MessageBox.Show(ex.Message, "Tools_FeatureAdded feng");
                        }
                    }

                    if (e.Feature.Type == GeometryType.MultiPolygon)
                    {
                        try
                        {
                            SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(e.Feature, ContainsType.Geometry);
                            si.QueryDefinition.Columns = null;
                            ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tabName, si);

                            ft["MapID"] = -1;
                            ft.Style = aStyle;
                            ft.Update();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Tools_FeatureAdded");
                        }
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        //updated by fisher(09-09-02)
                        if (comboTables.Text == "�ΰ�����ϵͳ" || comboTables.Text == "�����ɳ���" || comboTables.Text == "�������ж�" || comboTables.Text == "����������")
                        {
                            if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                            {
                                dataGridView1.Rows[i].Cells[1].Value = dx;
                            }
                            else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                            {
                                dataGridView1.Rows[i].Cells[1].Value = dy;
                            }
                            else
                            {
                                dataGridView1.Rows[i].Cells[1].Value = "";
                                fileName = "";
                            }
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[1].Value = "";
                            fileName = "";
                        }
                    }

                    tabControl1.SelectedTab = tabPageInfo;
                    buttonSave.Text = "����";
                    buttonCancel.Text = "ȡ�����";
                    dataGridView1.Visible = true;
                    buttonSave.Enabled = false;
                    buttonCancel.Enabled = true;
                    dataGridView1.Columns[1].ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_FeatureAdded");
            }
        }

        private void labeLayer(Table table)//������ע,�ڴ���֮ǰҪɾ����ǰ�ı�עͼ��
        {
            try
            {
                LabelLayer labelLayer=null ;
                for(int i=0;i<mapControl1.Map.Layers.Count;i++){
                    if (mapControl1.Map.Layers[i] is LabelLayer) {
                        labelLayer = mapControl1.Map.Layers[i] as LabelLayer;
                        break;
                    }
                }
                LabelSource source = new LabelSource(table);
                            
                source.DefaultLabelProperties.Caption = "Name";
                if (table.Alias == "��Ϣ��") {
                    source.DefaultLabelProperties.Caption = "����";
                    source.VisibleRange = new VisibleRange(0, 5, DistanceUnit.Kilometer);
                }
                source.DefaultLabelProperties.Layout.Offset = 4;
                
                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;

                labelLayer.Sources.Append(source);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "labeLayer");
            }
        }

        private void InitialEditFields(string tabName)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();
                OracleCommand cmd = new OracleCommand("SELECT COLUMN_NAME, NULLABLE,DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME= '" + tabName + "'", Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                cmd.Dispose();
                Conn.Close();

                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                string BobjID = CLC.ForSDGA.GetFromTable.ObjID;    //  by fisher on 09-09-03
                dataGridView1.Rows.Clear();
                int k = 0;
                int j = 0;
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    string fieldName = dt.Rows[i][0].ToString();
                    if (fieldName.ToUpper() == "MAPID" || fieldName.ToUpper() == "X" || fieldName.ToUpper() == "Y" || fieldName.IndexOf("�����ֶ�") > -1 || fieldName.ToUpper() == "MI_STYLE" || fieldName.ToUpper() == "MI_PRINX" || fieldName.ToUpper() == "GEOLOC")
                    {
                        k++;
                        continue;
                    }
                    dataGridView1.Rows.Add(1);//��dataGridView1�����һ��

                    dataGridView1.Rows[i-k].Cells[0].Value = fieldName;
                    if (fieldName == CLC.ForSDGA.GetFromTable.XiaQuField && tabName != "�����ɳ���")
                    {
                        dataGridView1.Rows[i-k].Cells[1] = dgvComboBoxJieZhen();
                    }
                    else if (fieldName == "�����ж�")
                    {
                        dataGridView1.Rows[i - k].Cells[1] = dgvComboBoxZhongdu();
                    }
                    else if (fieldName == "����������")
                    {
                        dataGridView1.Rows[i - k].Cells[1] = dgvComboBoxJinWuShi();
                    }
                    else if (fieldName == "�ص��˿�")
                    {
                        dataGridView1.Rows[i - k].Cells[1] = dgvComboBoxShiFou();
                    }
                    else
                    {
                        dataGridView1.Rows[i - k].Cells[1].Value = "";
                    }

                    if (dt.Rows[i][1].ToString().ToUpper() == "N"||fieldName==BobjID) 
                    {
                        dataGridView1.Rows[i-k].Cells[2].Value = "����";
                    }

                    dataGridView1.Rows[i-k].Cells[3].Value = dt.Rows[i][2].ToString().ToUpper();
                    j++;
                }

                if (comboTables.Text == "�˿�ϵͳ")
                {
                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[j].Cells[0].Value = "��Ƭ";
                    dataGridView1.Rows[j].Cells[1].Value = "";
                    dataGridView1.Rows[j].Cells[2].Value = "";
                    dataGridView1.Rows[j].Cells[3].Value = "VARCHAR2";
                }
                // added by fisher (09-09-02)
                if (comboTables.Text == "�ΰ�����ϵͳ" || comboTables.Text == "�����ɳ���" || comboTables.Text == "�������ж�"||comboTables.Text=="����������")
                {
                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[j].Cells[0].Value = "X";
                    dataGridView1.Rows[j].Cells[1].Value = "";
                    dataGridView1.Rows[j].Cells[2].Value = "";
                    dataGridView1.Rows[j].Cells[3].Value = "FLOAT";

                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[j+1].Cells[0].Value = "Y";
                    dataGridView1.Rows[j+1].Cells[1].Value = "";
                    dataGridView1.Rows[j + 1].Cells[2].Value = "";
                    dataGridView1.Rows[j+1].Cells[3].Value = "FLOAT";
                }
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                writeToLog(ex, "InitialEditFields");
            }

            setDataGridViewColumnWidth(dataGridView1);
        }

        private string[] getPaichusuo()    //edit by fisher in 09-12-01
        {
            try
            {
                string a = "";
                string pcsArr = strRegion;
                string pcsArr1 = strRegion1;
                if (pcsArr != "˳����" && pcsArr != "")
                {
                    if (Array.IndexOf(pcsArr.Split(','), "����") > -1)
                    {
                        pcsArr = pcsArr.Replace("����", "����,��ʤ");
                    }
                    if (pcsArr1 != "")
                    {
                        pcsArr += ",�����ɳ���";
                    }
                    return pcsArr.Split(',');
                }
                else if (pcsArr == "" && pcsArr1 != "")
                {
                    pcsArr = "�����ɳ���";
                    return pcsArr.Split(',');
                }
                else
                {
                    OracleConnection Conn = new OracleConnection(mysqlstr);
                    try
                    {
                        Conn.Open();
                        OracleCommand cmd = Conn.CreateCommand();

                        cmd.CommandText = "select �ɳ����� from �����ɳ���";

                        OracleDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            a += dr.GetValue(0).ToString().Trim() + ",";
                        }
                        dr.Close();
                        cmd.Dispose();
                        Conn.Close();
                        a = a.Remove(a.LastIndexOf(','));
                        return a.Split(',');
                    }
                    catch(Exception ex)
                    {
                        writeToLog(ex, "getPaichusuo");
                        if (Conn.State == ConnectionState.Open)
                        {
                            Conn.Close();
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getPaichusuo");
                return null;
            }
            //add by fisher in 09-12-01
        }

        private DataGridViewComboBoxCell dgvComboBoxJieZhen() 
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                for (int i = 0; i < listPaichusuo.Length; i++)
                {
                    dgvComBox.Items.Add(listPaichusuo[i]);
                }
                //�����ĳ�������û�,ֱ�Ӹ���Ͻ����ʼֵ
                //if (FrmLogin.stringϽ�� != "˳����")
                //{
                //    dgvComBox.Value = FrmLogin.stringϽ��;
                //}
                //else {
                dgvComBox.Value = dgvComBox.Items[0].ToString();
                //}
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxJieZhen");
                return null;
            }
        }

        private DataGridViewComboBoxCell dgvComboBoxJinWuShi()
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                string sqlstr = "select �������� from ����������";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                OracleDataReader orader = CLC.DatabaseRelated.OracleDriver.OracleComReader(sqlstr);
                while (orader.Read())
                {
                    dgvComBox.Items.Add(orader[0]);
                }
                dgvComBox.Value = dgvComBox.Items[0].ToString();
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxJinWuShi");
                return null;
            }
        }

        private DataGridViewComboBoxCell dgvComboBoxZhongdu()
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                string sqlstr = "select �ж��� from �������ж�";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                OracleDataReader orader = CLC.DatabaseRelated.OracleDriver.OracleComReader(sqlstr);

                while (orader.Read())
                {
                    dgvComBox.Items.Add(orader[0]);
                }
                dgvComBox.Value = dgvComBox.Items[0].ToString();
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxZhongdu");
                return null;
            }
        }
        private DataGridViewComboBoxCell dgvComboBoxShiFou()
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                dgvComBox.Items.Add("��");
                dgvComBox.Items.Add("��");
                dgvComBox.Value = dgvComBox.Items[0];
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxJieZhen");
                return null;
            }

        }

        private void InitialMap()//��ʹ����ͼ
        {
            try
            {
                string exePath = Application.StartupPath;
                mapControl1.Map.Load(MapLoader.CreateFromFile(exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\Data\\Shunde.mws"));
                mapOverview.Map.Load(MapLoader.CreateFromFile(exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\Data\\ShundeOverview.mws"));
                IMapLayer mapLayer = mapControl1.Map.Layers["��·��"];
                int ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                mapControl1.Map.Layers.Move(ilayer, 3);

                //MIConnection miConnection = new MIConnection();
                //miConnection.Open();
                //TableInfoServer ti = new TableInfoServer("��Ϣ��", "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select  *  From ��Ϣ��", ServerToolkit.Oci);
                //ti.CacheSettings.CacheType = CacheOption.Off;
                //Table t = miConnection.Catalog.OpenTable(ti);
                //FeatureLayer fl = new FeatureLayer(t);
                //fl.VisibleRange = new VisibleRange(0, 5, DistanceUnit.Kilometer);
                //mapControl1.Map.Layers.Insert(1, (IMapLayer)fl);
                //labeLayer(t);

                this.mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["������"]);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InitialMap");
            }
        }

        //������ʱ����ӥ�۵ľ��ο�
        private void CreateEagleLayer()
        {
            try
            {
                mapOverview.MouseWheelSupport = new MouseWheelSupport(MouseWheelBehavior.None, 10, 5); //��껬��Ĭ���ǷŴ���С�Ĺ��ܣ����ｫ�ù���ȡ��
                TableInfoMemTable ti = new TableInfoMemTable("EagleEyeTemp");
                ti.Temporary = true;
                Column column;
                column = new GeometryColumn(mapOverview.Map.GetDisplayCoordSys());
                column.Alias = "MI_Geometry";
                column.DataType = MIDbType.FeatureGeometry;
                ti.Columns.Add(column);
                column = new Column();
                column.Alias = "MI_Style";
                column.DataType = MIDbType.Style;
                ti.Columns.Add(column);
                Table table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                FeatureLayer eagleEye = new FeatureLayer(table, "EagleEye", "MyEagleEye");
                mapOverview.Map.Layers.Insert(0, (IMapLayer)eagleEye);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateEagleLayer");
            }
        }

        //+��ͼ��Ұ�����仯ʱ
        //-ӥ��ͼ�ϵ�ͼʾ��
        //-ͼ������е�ͼ��
        int iLevel = 1;
        private void mapControl1_ViewChanged(object sender, EventArgs e)
        {
            try
            {
                Table tabTemp = Session.Current.Catalog.GetTable("EagleEyeTemp");
                (tabTemp as ITableFeatureCollection).Clear();
                #region   Draw   the   rectangle
                //���þ��ε���ʽ
                DRect rect = mapControl1.Map.Bounds;
                FeatureGeometry feageo = new MapInfo.Geometry.Rectangle(mapControl1.Map.GetDisplayCoordSys(), rect);
                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(9, System.Drawing.Color.Gray, System.Drawing.Color.Green, true);
                CompositeStyle comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);

                //�����β��뵽ͼ����
                Feature fea = new Feature(feageo, comStyle);

                tabTemp.InsertFeature(fea);
                #endregion

                iLevel = setToolslevel();   //��ʾ����
                //�����Ӱ����ʾӰ��
                if (toolImgOrMap.Text == "��ͼ")
                {
                    closeOtherLevelImg(iLevel);//�ȹر����������Ӱ��

                    CalRowColAndDisImg(iLevel);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapControl1_ViewChanged");
            }
        }

        #region �鿴Ӱ��
        //����ͼ��Ұ�仯ʱ�����ݱ�����ָ����ͼ����
        private int setToolslevel()
        {
            int iLevel = 0;
            try
            {
                if (mapControl1.Map.Scale >= 200000)
                {
                    iLevel = 1;
                }
                else if (mapControl1.Map.Scale < 200000 && mapControl1.Map.Scale >= 100000)
                {
                    iLevel = 2;
                }
                else if (mapControl1.Map.Scale < 100000 && mapControl1.Map.Scale >= 50000)
                {
                    iLevel = 3;
                }
                else if (mapControl1.Map.Scale < 50000 && mapControl1.Map.Scale >= 20000)
                {
                    iLevel = 4;
                }
                else if (mapControl1.Map.Scale < 20000 && mapControl1.Map.Scale >= 10000)
                {
                    iLevel = 5;
                }
                else if (mapControl1.Map.Scale < 10000 && mapControl1.Map.Scale >= 5000)
                {
                    iLevel = 6;
                }
                else
                {
                    iLevel = 7;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setToolslevel");
            }
            return iLevel;
        }

        private void closeOtherLevelImg(int iLevel)
        {
            try
            {            
                GroupLayer gLayer = mapControl1.Map.Layers["Ӱ��"] as GroupLayer;
                if (gLayer == null) return;

                int iCount = gLayer.Count;
                for (int i = 0; i < iCount; i++)
                {
                    IMapLayer layer = gLayer[0];
                    string alies = layer.Alias;
                    if (Convert.ToInt16(alies.Substring(1, 1)) != iLevel)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(alies);
                    }
                }
                mapControl1.Refresh();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "closeOtherLevelImg");
            }
        }

        private void CalRowColAndDisImg(int iLevel)
        {
                double dScale = 0;
                int minRow = 0, minCol = 0;
                int maxRow = 0, maxCol = 0;
                switch (iLevel)
                {
                    case 1:
                        dScale = 200000;
                        minRow = 1;
                        minCol = 1;
                        maxRow = 2;
                        maxCol = 2;
                        break;
                    case 2:
                        dScale = 100000;
                        minRow = 1;
                        minCol = 1;
                        maxRow = 4;
                        maxCol = 4;
                        break;
                    case 3:
                        dScale = 50000;
                        minRow = 2;
                        minCol = 2;
                        maxRow = 7;
                        maxCol = 8;
                        break;
                    case 4:
                        dScale = 20000;
                        minRow = 3;
                        minCol = 3;
                        maxRow = 17;
                        maxCol = 19;
                        break;
                    case 5:
                        dScale = 10000;
                        minRow = 6;
                        minCol = 6;
                        maxRow = 34;
                        maxCol = 37;
                        break;
                    case 6:
                        dScale = 5000;
                        minRow = 12;
                        minCol = 11;
                        maxRow = 67;
                        maxCol = 74;
                        break;
                    case 7:
                        dScale = 2000;
                        minRow = 29;
                        minCol = 27;
                        maxRow = 166;
                        maxCol = 184;
                        break;
                }
                double gridDis = 6.0914200613 * Math.Pow(10, -7) * dScale * 2;  //���������񳤶�
                int beginRow = 0, endRow = 0;
                int beginCol = 0, endCol = 0;
                //�������к�
                //��ʼ�к�
                int dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y2) / gridDis);
                if (dRow > maxRow) return;    //�������ʼ�кűȱ���ͼƬ����кŻ���˵���˷�Χ��ͼ

                if (dRow < minRow)
                {
                    beginRow = minRow;
                }
                else
                {
                    beginRow = dRow;
                }

                //��ֹ�к�
                dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y1) / gridDis) + 1;
                if (dRow < minRow) return; //�������ֹ�кűȱ���ͼƬ��С�кŻ�С��˵���˷�Χ��ͼ
                if (dRow > maxRow)
                {
                    endRow = maxRow;
                }
                else
                {
                    endRow = dRow;
                }

                int dCol = Convert.ToInt32((mapControl1.Map.Bounds.x1 - 112.94) / gridDis);
                if (dCol > maxCol) return; //�������ʼ�кűȱ���ͼƬ����кŻ���˵���˷�Χ��ͼ
                if (dCol < minCol)
                {
                    beginCol = minCol;
                }
                else
                {
                    beginCol = dCol;
                }

                dCol = Convert.ToInt32((mapControl1.Map.Bounds.x2 - 112.94) / gridDis) + 1;  //������ֹ�к�
                if (dCol < minCol) return; //�������ֹ�кűȱ���ͼƬ��С�кŻ���˵���˷�Χ��ͼ
                if (dCol > maxCol)
                {
                    endCol = maxCol;
                }
                else
                {
                    endCol = dCol;
                }

                DisImg(iLevel, beginRow, endRow, beginCol, endCol);
        }

        private void DisImg(int iLevel, int beginRow, int endRow, int beginCol, int endCol)
        {
            string tabName = "";
            for (int i = beginRow; i <= endRow; i++)
            {
                for (int j = beginCol; j <= endCol; j++)
                {
                    tabName = iLevel.ToString() + "_" + i.ToString() + "_" + j.ToString();
                    openTable(iLevel, tabName);
                }
            }
               // mapControl1.Refresh();
        }

        private void openTable(int iLevel, string tableName)
        {
                //���ж��ļ����д岻���ڣ����ھʹ�
            try
            {       
                GroupLayer groupLayer = mapControl1.Map.Layers["Ӱ��"] as GroupLayer;//���ж���û�м���
                if (groupLayer["_" + tableName] == null)
                {
                    string imgPath = exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\ImgData\\" + iLevel.ToString() + "\\" + tableName + ".tab";
                    if (File.Exists(imgPath))
                    {
                        Table tab = MapInfo.Engine.Session.Current.Catalog.OpenTable(imgPath);

                        MapInfo.Mapping.FeatureLayer fl = new MapInfo.Mapping.FeatureLayer(tab, "_" + tableName);

                        groupLayer.Add(fl);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "openTable");
            }
        }

        #endregion

        private void mapOverview_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                MapInfo.Geometry.DPoint dP;//��������Ϊ����ͼ��ӥ��ͼ������
                mapOverview.Map.DisplayTransform.FromDisplay(new System.Drawing.Point(e.X, e.Y), out dP);
                mapControl1.Map.Center = dP;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapOverview_MouseDown");
            }
        }

        private string policeNo = "";
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Name)
                {
                    case "toolSel":
                        this.mapControl1.Tools.LeftButtonTool = "Select";
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        mapToolBar1.Buttons[0].Pushed = false;
                        mapToolBar1.Buttons[1].Pushed = false;
                        break;
                    case "toolZoomIn":
                        this.mapControl1.Tools.LeftButtonTool = "ZoomIn";
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        mapToolBar1.Buttons[0].Pushed = false;
                        mapToolBar1.Buttons[1].Pushed = false;
                        break;
                    case "toolZoomOut":
                        this.mapControl1.Tools.LeftButtonTool = "ZoomOut";
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        mapToolBar1.Buttons[0].Pushed = false;
                        mapToolBar1.Buttons[1].Pushed = false;
                        break;
                    case "toolPan":
                        this.mapControl1.Tools.LeftButtonTool = "Pan";
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        mapToolBar1.Buttons[0].Pushed = false;
                        mapToolBar1.Buttons[1].Pushed = false;
                        break;
                    case "toolFullExtent":
                        this.mapControl1.Map.SetView((FeatureLayer)mapControl1.Map.Layers["������"]);
                        //this.UncheckedTool();
                        break;
                    case "toolImgOrMap":
                        try
                        {
                            if (e.ClickedItem.Text == "Ӱ��")
                            {
                                mapControl1.Map.Layers["������"].Enabled = false;
                                mapControl1.Map.Layers["������"].Enabled = false;
                                mapControl1.Map.Layers["��·��"].Enabled = false;
                                mapControl1.Map.Layers["Ӱ��"].Enabled = true;
                                e.ClickedItem.Text = "��ͼ";

                                closeOtherLevelImg(iLevel);//�ȹر����������Ӱ��

                                CalRowColAndDisImg(iLevel);
                            }
                            else
                            {
                                mapControl1.Map.Layers["������"].Enabled = true;
                                mapControl1.Map.Layers["��԰"].Enabled = true;
                                mapControl1.Map.Layers["������"].Enabled = true;
                                mapControl1.Map.Layers["��·��"].Enabled = true;
                                mapControl1.Map.Layers["Ӱ��"].Enabled = false;
                                e.ClickedItem.Text = "Ӱ��";
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "toolStrip1_ItemClicked");
                        }
                        break;
                    case "toolLoc":
                        this.mapControl1.Tools.LeftButtonTool = "Location";
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        break;
                    case "toolMapLayer":
                        LayerControlDlg lcDlg = new LayerControlDlg();
                        lcDlg.Map = mapControl1.Map;
                        lcDlg.LayerControl.Tools = mapControl1.Tools;
                        lcDlg.ShowDialog(this);
                        break;
                    case "toolDingwei":
                        winLoc.frmLocDia floc = new winLoc.frmLocDia(mapControl1);
                        floc.Show();
                        break;
                    case "toolClear":              //add by fisher on Christmas Day
                        //�����λ��ӵ�ͼ��
                        try
                        {
                            //string tabName = ""; 
                            //if (tabControl2.SelectedTab == tabPage1)
                            //{ tabName=comboTables.Text.Trim();}
                            //if (tabControl2.SelectedTab == tabPage2)
                            //{ tabName = "��Ƶλ��"; } 
                            //if (tabControl2.SelectedTab == tabPage3)
                            //{ tabName=comboTable.Text.Trim();}
                            //if (MapInfo.Engine.Session.Current.Catalog.GetTable(tabName+"_tem") != null)
                            if (MapInfo.Engine.Session.Current.Catalog.GetTable("CodingPoint") != null)
                            {
                                MapInfo.Engine.Session.Current.Catalog.CloseTable("CodingPoint");
                                mapControl1.Map.Layers.Remove("geoCodeLabel");
                                MapInfo.Engine.Session.Current.Catalog.CloseTable("geoCodeLabel");
                                MapInfo.Engine.Session.Current.Catalog.CloseTable("Location_temp");
                            }
                        }
                        catch { MessageBox.Show("���ͼ�����", "��ʾ"); }
                        break;
                    case "toolGPSPolice":
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        switch (this.toolGPSPolice.Text)
                        {
                            case "��Ա�����༭":
                                frmGpsPolice frmgps = new frmGpsPolice();
                                if (frmgps.ShowDialog() == DialogResult.OK)
                                {
                                    policeNo = frmgps.textBox1.Text;
                                    GetPolice(policeNo);
                                }
                                else
                                {
                                    ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = false;
                                    this.toolGPSPolice.Text = "��Ա�����༭";
                                    this.toolRefresh.Visible = false;
                                    this.timerGPS.Stop();
                                    DelLayer();
                                    this.dataGridViewGaList.ContextMenuStrip = null;
                                }
                                break;
                            case "�ر������༭":
                                ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = false;
                                this.toolGPSPolice.Text = "��Ա�����༭";
                                this.toolRefresh.Visible = false;
                                this.timerGPS.Stop();
                                DelLayer();
                                this.dataGridViewGaList.ContextMenuStrip = null;
                                break;
                        }
                        break;
                    case "toolRefresh":
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        DelLayer();
                        GetPolice(policeNo);
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "toolStrip1_ItemClicked");
            }
        }

        private void GetPolice(string policeNo)
        {
            try
            {
                string sql = string.Empty;
                string _strUseRegion = this.strRegion;
                string _strZDRegion = this.strRegion1;
                string regionStr = "";   // ���Ȩ������
                if (_strUseRegion == string.Empty && _strZDRegion == string.Empty)
                {
                    isShowPro(false);
                    MessageBox.Show(@"û����������Ȩ��", @"ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    // Ȩ�����ã��ɳ������ж�Ȩ�ޣ�
                    if (_strUseRegion != "˳����")   // edit by fisher in 09-12-08
                    {
                        if (_strUseRegion != "")
                        {
                            if (Array.IndexOf(_strUseRegion.Split(','), "����") > -1)
                            {
                                _strUseRegion = _strUseRegion.Replace("����", "����,��ʤ");
                            }
                            regionStr += " �ɳ����� in ('" + _strUseRegion.Replace(",", "','") + "')";

                            if (_strZDRegion != "")
                            {
                                if (regionStr.IndexOf("and") > -1)
                                {
                                    regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                    regionStr += " or �ж��� in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                }
                                else
                                {
                                    regionStr += " �ж��� in ('" + _strZDRegion.Replace(",", "','") + "')";
                                }
                            }
                        }
                        else if (_strUseRegion == "")
                        {
                            if (_strZDRegion != "")
                            {
                                if (regionStr.IndexOf("and") > -1)
                                {
                                    regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                    regionStr += " or �ж��� in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                }
                                else
                                {
                                    regionStr += " �ж��� in ('" + _strZDRegion.Replace(",", "','") + "')";
                                }
                            }
                            else
                            {
                                MessageBox.Show("��û�в�ѯȨ��!");
                                return;
                            }
                        }
                    }
                }
                if (regionStr == "")
                    sql = "Select ������� as Name,������� as ��_ID,'GPS��Ա' as ����,X,Y from GPS��Ա where �������='" + policeNo + "'";
                else
                    sql = "Select ������� as Name,������� as ��_ID,'GPS��Ա' as ����,X,Y from GPS��Ա where �������='" + policeNo + "' and " + regionStr;

                // ��ѯ������ڵ�ͼ��ʾ��2010-10-24��
                DataTable table = new DataTable();
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

                if (table.Rows.Count <= 0)
                {
                    MessageBox.Show("����ѯ�ľ�Ա�����ڣ�", "��ʾ");
                    frmGpsPolice frmgps = new frmGpsPolice();
                    if (frmgps.ShowDialog() == DialogResult.OK)
                    {
                        policeNo = frmgps.textBox1.Text;
                        GetPolice(policeNo);
                    }
                    else
                    {
                        this.toolGPSPolice.Checked = false;
                        this.toolGPSPolice.Text = "��Ա�����༭";
                        this.toolRefresh.Visible = false;
                        this.timerGPS.Stop();
                        DelLayer();
                    }
                    return;
                }
                this.toolGPSPolice.Text = "�ر������༭";
                this.toolRefresh.Visible = true;
                this.timerGPS.Interval = Convert.ToInt32(getGPSTime());
                this.timerGPS.Start();
                CreateGPSLayer(table);
                this.dataGridViewGaList.ContextMenuStrip = this.contextMenuStrip1;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GetPolice");
            }
        }

        private void CreateGPSLayer(DataTable dt)
        {
            try
            {
                DelLayer();

                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet("GPSLayer", dt);
                MapInfo.Data.SpatialSchemaXY xy = new SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0,0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;

                MapInfo.Data.Table temTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                FeatureLayer temlayer = new FeatureLayer(temTable, "GPSLayer");

                this.mapControl1.Map.Layers.Add(temlayer);

                //�ı�ͼ���ͼԪ��ʽ
                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("Police.bmp", BitmapStyles.None, System.Drawing.Color.Red, 20));
                FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, cs);
                temlayer.Modifiers.Clear();

                // ProtectMap();

                temlayer.Modifiers.Append(fsm);

                //��ӱ�ע
                const string activeMapLabel = "GPSLabel";
                Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("GPSLayer");
                LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);

                LabelSource lbsource = new LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "����";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);

                //ProtectMap();

                mapControl1.Map.Layers.Add(lblayer);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateGPSLayer");
            }
        }

        private void DelLayer()
        {
            try
            {
                if (mapControl1.Map.Layers["GPSLayer"] != null)
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("GPSLayer");
                if (mapControl1.Map.Layers["GPSLabel"] != null)
                    mapControl1.Map.Layers.Remove("GPSLabel");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "DelLayer");
            }
        }

        //����������ϵĹ���ʱ���Թ��߰�ť�������ã�
        //ѡ�еı�����ɫ��Ϊ��ɫ������͸�����Ա���ȷ��ǰ��ѡ����
        //���ڰ�ť���飬iFrom��ʾ�����Index��iEnd��ʾĩIndex
        private void UncheckedTool()//��ToolStripButton����Ϊδѡ�е�״̬
        {
            try
            {
                for (int i = 0; i < this.toolStrip1.Items.Count; i++)
                {
                    System.Windows.Forms.ToolStripButton tempToolStripButton = (System.Windows.Forms.ToolStripButton)this.toolStrip1.Items[i];
                    if (tempToolStripButton != null)
                    {
                        if (tempToolStripButton.Checked == true)
                        {
                            tempToolStripButton.Checked = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "UncheckedTool");
            }
        }
        
        //����ڵ�ͼ���ƶ�ʱ����״̬����ʾ����
        private void mapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                System.Drawing.Point sPoint = new System.Drawing.Point(e.X, e.Y);
                DPoint dPoint;
                mapControl1.Map.DisplayTransform.FromDisplay(sPoint, out dPoint);
                toolStatusCoord.Text = "X:" + dPoint.x.ToString("#.#####") + ", Y:" + dPoint.y.ToString("#.#####");

                if (mapControl1.Tools.LeftButtonTool == "AddPolygon" && MapInfo.Mapping.LayerHelper.GetEditable(mapControl1.Map.Layers[comboTables.Text]) == true && isUse)
                {
                    if (dPoint.x <= mapControl1.Map.Bounds.x1)
                    {
                        DPoint dp = new DPoint(mapControl1.Map.Center.x - mapControl1.Map.Bounds.Width() / 20, mapControl1.Map.Center.y);
                        mapControl1.Map.Center = dp;
                    }
                    if (dPoint.x >= mapControl1.Map.Bounds.x2)
                    {
                        DPoint dp = new DPoint(mapControl1.Map.Center.x + mapControl1.Map.Bounds.Width() / 20, mapControl1.Map.Center.y);
                        mapControl1.Map.Center = dp;
                    }
                    if (dPoint.y <= mapControl1.Map.Bounds.y1)
                    {
                        DPoint dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y - mapControl1.Map.Bounds.Height() / 20);
                        mapControl1.Map.Center = dp;
                    }
                    if (dPoint.y >= mapControl1.Map.Bounds.y2)
                    {
                        DPoint dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y + mapControl1.Map.Bounds.Height() / 20);
                        mapControl1.Map.Center = dp;
                    }
                }

                if (tabControl2.SelectedTab == tabPage1 && this.comboTables.Text=="���ж�Ͻ��")
                {
                    // ������Ƶ�ĳһ��������ʱ��ʾ�þ����ҵ�����
                    System.Drawing.Point point = e.Location;
                    MapInfo.Geometry.DPoint dpt;
                    mapControl1.Map.DisplayTransform.FromDisplay(point, out dpt);

                    Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 30);
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dpt, mapControl1.Map.GetDisplayCoordSys(), d);
                    si.SearchResultProcessor = null;
                    si.QueryDefinition.Columns = null;

                    Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("�ж�Ͻ��", si);

                    if (ft2 != null)
                    {
                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                        string sqlStr = "select �ж��� from �������ж� where �жӴ��� = '" + ft2["�жӴ���"].ToString() + "'";
                        DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        this.label5.Text = table.Rows[0][0].ToString();
                        point.X = point.X + 20;
                        this.panel7.Location = point;
                        this.panel7.Visible = true;
                    }
                    else
                    {
                        this.panel7.Visible = false;
                    }
                }
                if (tabControl2.SelectedTab == tabPage1 && this.comboTables.Text == "������Ͻ��")
                {
                    // ������Ƶ�ĳһ��������ʱ��ʾ�þ����ҵ�����
                    System.Drawing.Point point = e.Location;
                    MapInfo.Geometry.DPoint dpt;
                    mapControl1.Map.DisplayTransform.FromDisplay(point, out dpt);

                    Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 30);
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dpt, mapControl1.Map.GetDisplayCoordSys(), d);
                    si.SearchResultProcessor = null;
                    si.QueryDefinition.Columns = null;

                    Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������Ͻ��", si);

                    if (ft2 != null)
                    {
                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                        string sqlStr = "select �������� from ���������� where �����Ҵ��� = '" + ft2["�����Ҵ���"].ToString() + "'";
                        DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        this.label5.Text = table.Rows[0][0].ToString();
                        point.X = point.X + 20;
                        this.panel7.Location = point;
                        this.panel7.Visible = true;
                    }
                    else
                    {
                        this.panel7.Visible = false;
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "mapControl1_MouseMove");
            }
        }

        private string valueText = "";
        MapInfo.Data.Table editTable = null;
        MapInfo.Data.Table lTable = null;
        private void comboTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (this.valueText == this.comboTables.Text)
                {
                    this.mapControl1.Focus();
                    return;
                }

                dataGridViewList.DataSource = null;
                //dataGridViewList.Visible = false;
                dataGridView1.Rows.Clear();
                dataGridView1.Visible = false;

                this.bpageCount = 0;
                this.bpageCurrent = 0;
                this.toolStripRecord.Text = "0��";
                this.tsTextBoxPageNow.Text = "0";//���õ�ǰҳ
                this.tStripLabelPageCount.Text = "/ {0}";//������ҳ��
                this.toolStripPageSize.Text = bpageSize.ToString();

                this.valueText = this.comboTables.Text;

                CLC.ForSDGA.GetFromTable.GetFromName(valueText, getFromNamePath);
                label3.Text = CLC.ForSDGA.GetFromTable.ObjName;
                try  //�ȹر�֮ǰ�ı�ͱ�ע��
                {
                    if (this.editTable != null)
                    {
                        string sAlias = this.editTable.Alias;
                        if (mapControl1.Map.Layers[sAlias] != null)
                        {
                            MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                        }
                    }
                }
                catch (Exception ex) { writeToLog(ex, "comboTables_SelectedIndexChanged-�ر�֮ǰ�ı�ͱ�ע��"); }

                this.btnLoc1.Enabled = false;
                this.mapControl1.Tools.LeftButtonTool = "Pan";

                textKeyWord.Text = "";
                switch (comboTables.Text)
                {
                    case "��Ƶλ��":
                    case "��������":
                    case "��ȫ������λ":
                    case "����":
                    case "������ҵ":
                    case "����˨":
                    case "�����ص㵥λ":
                    case "�ΰ�����ϵͳ":
                    case "�����ɳ���":
                    case "�������ж�":
                    case "����������":
                        mapToolBar1.Buttons[0].Visible = true;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = true;
                        mapToolBar1.Buttons[6].Visible = false;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPolygon")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = false;
                        this.buttonSearch.Enabled = true;
                        this.textKeyWord.Enabled = true;
                        this.cbMohu.Enabled = true;
                        this.textKeyWord.Focus();
                        featStyle = setFeatStyle(comboTables.Text);
                        this.GetTable(this.comboTables.Text);
                        break;
                    case "�ɳ���Ͻ��":
                    case "������Ͻ��":
                    case "���ж�Ͻ��":
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = true;
                        mapToolBar1.Buttons[3].Visible = true;
                        mapToolBar1.Buttons[3].Pushed = false;
                        mapToolBar1.Buttons[4].Visible = true;
                        mapToolBar1.Buttons[4].Enabled = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = true;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPoint")
                        {
                            this.mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.buttonSearch.Enabled = false;
                        this.textKeyWord.Enabled = false;
                        this.cbMohu.Enabled = false;
                        this.mapControl1.Focus();
                        this.isOracleSpatialTab = true;
                        this.GetTable(this.comboTables.Text);
                        break;
                    case "�˿�ϵͳ":
                    case "�����ݷ���ϵͳ":
                    case "������Ϣ":
                        mapToolBar1.Buttons[0].Visible = true;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = true;
                        mapToolBar1.Buttons[6].Visible = false;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPolygon")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = true;
                        this.buttonSearch.Enabled = true;
                        this.textKeyWord.Enabled = true;
                        this.cbMohu.Enabled = true;
                        this.textKeyWord.Focus();
                        featStyle = setFeatStyle(comboTables.Text);
                        this.GetTable(this.comboTables.Text);
                        break;
                    case "�ɳ���ÿ�վ�Ա��":
                        if (listPaichusuo[0] == "" || (listPaichusuo.Length == 1 && listPaichusuo[0] == "�����ɳ���"))
                        {
                            MessageBox.Show("��û�иò���Ȩ��");
                            break;
                        }
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = false;

                        frmPoliceCount fPoliceCount = new frmPoliceCount(listPaichusuo, comboTables.Text, this.temEditDt);
                        fPoliceCount.ShowDialog();
                        break;
                    case "�ж�ÿ�վ�Ա��":
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = false;

                        frmPoliceCount fPoliceCount1 = new frmPoliceCount(listPaichusuo, comboTables.Text, this.temEditDt);
                        fPoliceCount1.ShowDialog();
                        break;
                    case "�������ά��":
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = false;
                        frmCaseType fCaseType = new frmCaseType(mysqlstr);
                        fCaseType.ShowDialog();
                        break;
                }
                //fisher(09-09-01)
                switch (comboTables.Text)
                {
                    case "��Ƶλ��":
                    case "�ΰ�����ϵͳ":
                    case "�����ɳ���":
                    case "�������ж�":
                    case "����������":
                        this.btnLocatedYes.Enabled = true;
                        this.btnLocatedNo.Enabled = true;
                        break;
                    default:
                        this.btnLocatedYes.Enabled = false;
                        this.btnLocatedNo.Enabled = false;
                        break;
                }
                if (comboTables.Text == "��Ƶλ��")
                {
                    mapToolBar1.Buttons[0].Visible = false;
                    mapToolBar1.Buttons[5].Visible = false;
                }

                _exportDT = null;

                buttonSave.Text = "����";
                buttonCancel.Text = "ȡ��";
                buttonSave.Enabled = false;
                buttonCancel.Enabled = false;

                SetButtonStyle(this.valueText);//���ñ༭��ť
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex) { writeToLog(ex, "comboTables_SelectedIndexChanged"); }
        }
        
        private Style setFeatStyle(string p)
        {
            try
            {
                GetFromName getName = new GetFromName(p);
                BasePointStyle ppStyle;
                string bmpName = getName.BmpName;

                if (bmpName == "anjian")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                    return ppStyle;
                }
                else if (bmpName == "gonggong")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(33, Color.Yellow, 9);
                    return ppStyle;
                }
                else if (bmpName == "tezhong")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(36, Color.Cyan, 10);
                    return ppStyle;
                }
                else if (bmpName == "")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(45, Color.Blue, 15);
                    return ppStyle;
                }
                else
                {
                    MapInfo.Styles.BitmapPointStyle bitmappointstyle = new MapInfo.Styles.BitmapPointStyle(bmpName);
                    bitmappointstyle.NativeSize = true;
                    return bitmappointstyle;
                }
            }
            catch (Exception ex) { writeToLog(ex, "setFeatStyle"); return null; }
        }

        private void GetTable(string tableName)//�����ݿ��еõ���
        {
            if (isOracleSpatialTab)
            {
                MIConnection miConnection = new MIConnection();
                try
                {
                    if (this.lTable != null)
                    {
                        this.lTable.Close();
                    }
                    miConnection.Open();

                    //֮ǰ�пռ��򲻿�, �������˶�δ�.
                    try
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select  *  From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;
                        //try
                        //{
                            this.lTable = miConnection.Catalog.OpenTable(ti);
                        //}
                        //catch (System.AccessViolationException ex)
                        //{
                        //    writeToLog(ex);
                        //    this.lTable = miConnection.Catalog.OpenTable(ti);
                        //}
                    }
                    catch
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select * From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;
                        this.lTable = miConnection.Catalog.OpenTable(ti);
                    }

                    MapInfo.Data.TableInfoNative ListTableInfo = new MapInfo.Data.TableInfoNative(tableName + "_tem");  
                    ListTableInfo.Temporary = true;
                    MapInfo.Geometry.CoordSys LCoordsys;
                    MapInfo.Data.GeometryColumn GC = (MapInfo.Data.GeometryColumn)(this.lTable.TableInfo.Columns["obj"]);
                    LCoordsys = GC.CoordSys;
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateFeatureGeometryColumn(LCoordsys));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateIntColumn("MapID"));
                    //ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("MapID",100));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("keyID", 100));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("Name", 200));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStyleColumn());
                    // ListTableInfo.WriteTabFile();
                    this.editTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ListTableInfo);

                    if (tableName == "�ɳ���Ͻ��" || tableName == "���ж�Ͻ��" || tableName == "������Ͻ��")
                    {
                        CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                        MICommand command = miConnection.CreateCommand();
                        command.CommandText = "insert into " + tableName + "_tem Select obj,MI_PRINX as MapID," +CLC.ForSDGA.GetFromTable.ObjID+ " as keyID," + CLC.ForSDGA.GetFromTable.ObjName + " as Name,MI_STYLE From " + lTable.Alias + " t";
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    miConnection.Close();

                    //��ͼ��ʾ
                    FeatureLayer fl = new FeatureLayer(editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(editTable);//��עͼ��

                    if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                    {
                        MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    }
                    MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    mapControl1.Map.Center = mapControl1.Map.Center;
                }
                catch (Exception ex)
                {
                    if (miConnection.State == ConnectionState.Open)
                    {
                        miConnection.Close();
                    }
                    writeToLog(ex, "GetTable-OracleSpatialTab");
                }
            }
            else
            {
                try
                {
                    if (this.editTable != null)
                    {
                        this.editTable.Close();
                    }
                    this.editTable = createTable(tableName);
                    FeatureLayer fl = new FeatureLayer(this.editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(this.editTable);//��עͼ��

                    if (this.editTable != null)
                    {
                        if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                        }
                        MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                        //setPrivilege();
                        mapControl1.Map.Center = mapControl1.Map.Center;
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "GetTable");
                }
            }

            this.InitialEditFields(this.comboTables.Text);
        }

        private MapInfo.Data.Table createTable(string tableName)
        {
            try
            {
                Table table;
                if (MapInfo.Engine.Session.Current.Catalog.GetTable(tableName) == null)
                {
                    TableInfoMemTable ti = new TableInfoMemTable(tableName);
                    ti.Temporary = true;
                    Column column;
                    column = new GeometryColumn(mapControl1.Map.GetDisplayCoordSys());
                    column.Alias = "MI_Geometry";
                    column.DataType = MIDbType.FeatureGeometry;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "MI_Style";
                    column.DataType = MIDbType.Style;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "MapID";
                    column.DataType = MIDbType.Int;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "keyID";
                    column.DataType = MIDbType.String;
                    ti.Columns.Add(column);

                    column = new Column();
                    column.Alias = "Name";
                    column.DataType = MIDbType.String;
                    ti.Columns.Add(column);

                    try
                    {
                        table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                    }
                    catch
                    {
                        table = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);
                    }
                }
                else
                {
                    table = MapInfo.Engine.Session.Current.Catalog.GetTable(tableName);
                }
                return table;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "createTable");
                return null;
            }
        }

        //�ж������Ƿ��ظ���������fisher��ӣ�09-08-28
        private bool isZhujian()
        {
            OracleConnection Conn = new OracleConnection(mysqlstr); //�������ݿ�
            Conn.Open();
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
            string Pdstr = "select " + CLC.ForSDGA.GetFromTable.ObjID+ " from " + comboTables.Text;
            OracleCommand orcmd = new OracleCommand(Pdstr, Conn);
            OracleDataAdapter PdAdapter = new OracleDataAdapter(orcmd);
            DataSet Pdds = new DataSet();
            PdAdapter.Fill(Pdds);
            DataTable Pddt = Pdds.Tables[0];
            try
            {
                int ix = 0;
                for (int j = 0; j < dataGridView1.Rows.Count; j++)
                {
                    if (dataGridView1.Rows[j].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = j;
                    }
                }
                if (Pddt.Rows.Count < 1)
                {
                    orcmd.Dispose();
                    Conn.Close();
                    return false;
                }
                else
                {
                    for (int i = 0; i < Pddt.Rows.Count; i++)
                    {
                        if (Pddt.Rows[i][0].ToString() == dataGridView1.Rows[ix].Cells[1].Value.ToString())
                        {
                            orcmd.Dispose();
                            Conn.Close();
                            return true;
                        }
                    }
                }
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
            catch(Exception ex)
            {
                writeToLog(ex, "isZhujian()");
                MessageBox.Show(ex.Message, "isZhujian()");
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
        }

        int mapId = 0;
        private void buttonSave_Click(object sender, EventArgs e)
        {
            //���ж������ֶ�,��������ֵ�Ѵ���,������ʾ
            OracleConnection Conn = new OracleConnection(mysqlstr); //�������ݿ�

            this.Cursor = Cursors.WaitCursor;

            this.dataGridView1.CurrentCell = null;//�ڽ��б����ʱ����DataGridView1ʧȥ����
            this.mapControl1.Focus();

            bool IsorKey = false;

            if (isOracleSpatialTab)
            {
                OracleDataReader dr = null;
                try
                {
                    Conn.Open();
                    OracleCommand cmd;
                    if (buttonSave.Text == "����")
                    {
                        IsorKey = isZhujian();
                        if (IsorKey)
                        {
                            MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            IsorKey = false;
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        else
                        {
                            cmd = new OracleCommand("select max(MI_PRINX) from " + comboTables.Text, Conn);
                            dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                dr.Read();
                                if (dr.GetValue(0) != null)
                                {
                                    selPrinx = Convert.ToInt32(dr.GetValue(0)) + 1;
                                }
                            }
                            dr.Close();
                            cmd.Dispose();

                            feature = new Feature(lTable.TableInfo.Columns);
                            feature.Geometry = ft.Geometry;
                            feature.Style = ft.Style;

                            string strValue = "";
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                strValue = "";
                                if (dataGridView1.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                }
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "��Ƭ")
                                {
                                    continue;
                                }
                                if (dataGridView1.Rows[i].Cells[2].Value != null && dataGridView1.Rows[i].Cells[2].Value.ToString() == "����")
                                {
                                    if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridView1.Rows[i].Cells[0].Value.ToString() + " ����Ϊ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        return;
                                    }
                                }
                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    feature[dataGridView1.Rows[i].Cells[0].Value.ToString()] = Convert.ToDateTime(strValue);
                                }
                                else
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    feature[dataGridView1.Rows[i].Cells[0].Value.ToString()] = strValue;
                                }
                            }
                            feature["MI_PRINX"] = selPrinx;

                            for (int i = 0; i < feature.Columns.Count; i++)
                            {
                                if (feature.Columns[i].Alias.ToUpper() == "MAPID")
                                {
                                    feature["MapID"] = selPrinx;
                                    break;
                                }
                            }
                            lTable.InsertFeature(feature);

                            //����featureֻ��¼ʱ��ֵ�����ڲ���,�����ٴθ���date��ֵ
                            string command = "update " + comboTables.Text + " set ";
                            strValue = "";
                            bool isUpdate = false;
                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridView1.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                }

                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE" && strValue != "")
                                {
                                    DateTime dTime = Convert.ToDateTime(strValue);
                                    if (dTime.TimeOfDay.ToString() != "00:00:00")
                                    {
                                        command += dataGridView1.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                        isUpdate = true;
                                    }
                                }

                            }
                            if (isUpdate)
                            {
                                command = command.Remove(command.Length - 1);
                                command += " where MI_PRINX=" + selPrinx;
                                cmd = new OracleCommand(command, Conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                Conn.Close();
                            }

                            // ����ӵ����ݲ��������Ͼ�ִ�и��£���Ϊ�����ֻ���л���dataGridViewGaListѡ��ĳ����¼����ܸ���
                            // updated by fisher in 09-10-23
                            this.dataGridView1.Columns[1].ReadOnly = true;
                            addToList(feature.Geometry.Centroid.x, feature.Geometry.Centroid.y);
                        }
                    }
                    else {//����
                        string command = "update " + comboTables.Text + " set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                        {
                            strValue = "";
                            if (dataGridView1.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            }
                            if (dataGridView1.Rows[i].Cells[0].Value.ToString() != "��Ƭ")
                            {
                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    command += dataGridView1.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X = '0',";
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X='" + strValue + "',"; 
                                    }
                                }
                                else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y = '0',"; 
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y='" + strValue + "',";
                                    }
                                }
                                else
                                {
                                    command += dataGridView1.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                                }
                            }
                        }
                        command = command.Remove(command.Length - 1);
                        //command += " where MI_PRINX=" + selPrinx;
                        //update by fisher in 1019
                        GetFromName OrgetName = new GetFromName(comboTables.Text);
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                        command += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewList.Rows[browIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                        cmd = new OracleCommand(command, Conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        Conn.Close();
                                                
                        updateListValue();
                    }
                    //���µ�ͼҪ�ص�����
                    updateMapValue();
                    
                }
                catch(OracleException ex) {
                    if (ex.Code == 1)//�����ֶ������˷�Ψһֵ
                    {
                        MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "buttonSave_Click-OracleSpatialTab");
                    }
                    this.Cursor = Cursors.Default;
                    if (Conn.State == ConnectionState.Open)
                        Conn.Close();
                    return;
                }

                if (comboTables.Text.Trim() == "�˿�ϵͳ")
                {
                    if (hasUpdate)
                    {
                        Conn = new OracleConnection(mysqlstr); //�������ݿ�
                        OracleCommand cmd;
                        Conn.Open();
                        string strValue = "";

                        try
                        {
                            string strExe = "";
                            strValue = "";
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "���֤����")
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                    break;
                                }
                            }
                            if (strValue != "")
                            {
                                if (buttonSave.Text == "����")
                                {
                                    strExe = "insert into �˿���Ƭ(���֤����,��Ƭ) values('" + strValue + "' ,:imgData)";
                                }
                                else
                                {
                                    //�Ȳ�ѯ,�����˿���Ƭ����û�ж�Ӧ�Ķ���
                                    cmd = new OracleCommand("select * from �˿���Ƭ where ���֤����='" + strValue + "'", Conn);
                                    OracleDataReader oDr = cmd.ExecuteReader();
                                    if (oDr.HasRows)
                                    {
                                        strExe = "update �˿���Ƭ set ��Ƭ=:imgData where ���֤����='" + strValue + "'";
                                    }
                                    else {
                                        strExe = "insert into �˿���Ƭ(���֤����,��Ƭ) values('" + strValue + "' ,:imgData)";
                                    }
                                    oDr.Close();
                                }
                                cmd = new OracleCommand(strExe, Conn);

                                if (fileName == "")
                                {
                                    fileName = Application.StartupPath + "\\Ĭ��.bmp";
                                }

                                FileStream fs = File.OpenRead(fileName);
                                byte[] imgData = new byte[fs.Length];
                                long n = fs.Read(imgData, 0, imgData.Length);
                                cmd.Parameters.Add(":ImgData", OracleType.LongRaw).Value = imgData;
                                fs.Close();

                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "buttonSave_Click-OracleSpatialTab");
                            if (Conn.State == ConnectionState.Open)
                                Conn.Close();
                        }
                    }
                    Conn.Close();
                }               
            }
          
            else  //��oracleSpatial��
            {
                try
                {
                    Conn.Open();
                    OracleCommand cmd;
                    string strExe = "";
                    if (buttonSave.Text == "����")
                    {
                        IsorKey = isZhujian();
                        if (IsorKey)
                        {
                            MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            IsorKey = false;
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        else
                        {
                            //cmd = new OracleCommand("select max(mapid) from " + comboTables.Text, Conn);
                            //dr = cmd.ExecuteReader();
                            //if (dr.HasRows)
                            //{
                            //    dr.Read();
                            //    if (dr.GetValue(0) != null)
                            //    {
                            //        mapId = Convert.ToInt32(dr.GetValue(0)) + 1;
                            //    }
                            //}
                            //dr.Close();
                            strExe = "insert into " + comboTables.Text + "(";

                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                if (dataGridView1.Rows[i].Cells[2].Value != null && dataGridView1.Rows[i].Cells[2].Value.ToString() == "����")
                                {
                                    if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridView1.Rows[i].Cells[0].Value.ToString() + " ����Ϊ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        return;
                                    }
                                }
                                if (i == 0)
                                    strExe += dataGridView1.Rows[i].Cells[0].Value.ToString();
                                else
                                    strExe += "," + dataGridView1.Rows[i].Cells[0].Value.ToString();
                            }

                            strExe += ") values(";

                            string strValue = "";
                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridView1.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                }
                                else
                                {
                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        strValue = "0";
                                    } 
                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        strValue = "0";
                                    }
                                }
                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    strExe += "to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else
                                {
                                    strExe += "'" + strValue + "',";
                                }
                            }

                            strExe = strExe.Remove(strExe.Length - 1);
                            strExe += ")";

                            selMapID = mapId.ToString();

                            // ����ӵ����ݲ��������Ͼ�ִ�и��£���Ϊ�����ֻ���л���dataGridViewGaListѡ��ĳ����¼����ܸ���
                            // updated by fisher in 09-10-23
                            this.dataGridView1.Columns[1].ReadOnly = true;
                            addToList(dx, dy);
                        }
                    }
                    else
                    {   //����
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                        strExe = "update " + comboTables.Text + " set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                        {
                            //update by siumo 09-01-08, ��Ϊnullʱ,toString����
                            strValue = "";
                            if (dataGridView1.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            }
                            else
                            {
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                    strValue = "0";
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                    strValue = "0";
                            }
                            if (dataGridView1.Rows[i].Cells[2].Value != null && dataGridView1.Rows[i].Cells[2].Value.ToString() == "����")
                            {
                                if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                                {
                                    MessageBox.Show(dataGridView1.Rows[i].Cells[0].Value.ToString() + " ����Ϊ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Cursor = Cursors.Default;
                                    return;
                                }
                            }
                            if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                            {
                                strExe += dataGridView1.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                            }
                            else
                            {
                                strExe += dataGridView1.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                            }
                        }
                        strExe = strExe.Remove(strExe.Length - 1);
                        //strExe += " where MapID='" + this.selMapID + "'";
                        strExe += "where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewList.Rows[browIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";

                        updateListValue();
                    }

                    cmd = new OracleCommand(strExe, Conn);
                    cmd.ExecuteNonQuery();

                    Conn.Close();
                }
                catch (OracleException ex)
                {
                    if (Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                    if (ex.Code == 1)//�����ֶ������˷�Ψһֵ
                    {
                        MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "buttonSave_Click");
                    }
                    this.Cursor = Cursors.Default;
                    return;
                }

                //���µ�ͼҪ�ص�����
                updateMapValue();

            }
            string editMothed="���";
            if (buttonSave.Text == "����")
            {
                MessageBox.Show("���³ɹ�!");
                editMothed = "�޸�����";
            }
            else {
                MessageBox.Show("������ݳɹ�!");
                buttonSave.Text = "����";
                buttonCancel.Text = "ȡ��";
            }

            //��¼�༭log
            WriteEditLog(comboTables.Text.Trim(), selMapID, editMothed);
            this.mapControl1.Tools.LeftButtonTool = "Pan";
            this.UncheckedTool();
            this.dataGridView1.CurrentCell = null;
            buttonSave.Enabled = false;
            buttonCancel.Enabled = false;
            try
            {
                string tabName = comboTables.Text.Trim();
                if (isOracleSpatialTab) {
                    tabName += "_tem";
                }
                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex) { writeToLog(ex, "buttonSave_Click"); }
            this.Cursor = Cursors.Default;
        }

        private void updateListValue()
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);

                //�õ�keyID��ֵ
                string sId = "";
                for (int i = 0; i < dataGridView1.Rows.Count; i++) {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        sId = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        break;
                    }
                }

                //����keyID���б��е���
                int ix = 0;
                for (int i = 0; i < dataGridViewList.Columns.Count; i++) {
                    if (dataGridViewList.Columns[i].Name == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = i;
                        break;
                    }
                }

                //���Ҹ���Ҫ�����б��е���,��������һ�е�ֵ.
                for (int i = 0; i < dataGridViewList.Rows.Count; i++) {
                    if (dataGridViewList.Rows[i].Cells[ix].Value.ToString() == sId) {
                        for (int j = 0; j < dataGridView1.Rows.Count; j++)
                        {
                            if (dataGridView1.Rows[j].Cells[0].Value.ToString() != "��Ƭ")
                            {
                              dataGridViewList.Rows[i].Cells[dataGridView1.Rows[j].Cells[0].Value.ToString()].Value = dataGridView1.Rows[j].Cells[1].Value;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex) 
            {
                writeToLog(ex, "updateListValue");
            }
        }

        private void GaupdateListValue()
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);


                //�õ�keyID��ֵ
                string sId = "";
                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                {
                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        sId = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                        break;
                    }
                }

                //����keyID���б��е���
                int ix = 0;
                for (int i = 0; i < dataGridViewGaList.Columns.Count; i++)
                {
                    if (dataGridViewGaList.Columns[i].Name == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = i;
                        break;
                    }
                }

                //���Ҹ���Ҫ�����б��е���,��������һ�е�ֵ.
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (dataGridViewGaList.Rows[i].Cells[ix].Value.ToString() == sId)
                    {
                        for (int j = 0; j < dataGridViewGaInfo.Rows.Count; j++)
                        {
                            if (dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString() != "��Ƭ")
                            {
                                dataGridViewGaList.Rows[i].Cells[dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString()].Value = dataGridViewGaInfo.Rows[j].Cells[1].Value;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaupdateListValue");
                //MessageBox.Show(ex.Message, "GaupdateListValue()");
            }
        }

        //
        private void addToList(double x,double y)
        {
            try
            {
                if (dataGridViewList.Rows.Count == 0)
                {
                    dataGridViewList.Columns.Clear();
                    string stag = "";
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        stag = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        if (stag != "��Ƭ")
                        {
                            dataGridViewList.Columns.Add(stag, stag);
                        }
                    }
                    //edit by fisher in 09-10-23
                    //dataGridViewList.Columns.Add("x", "x");
                    //dataGridViewList.Columns.Add("y", "y");
                }

                DataTable dt=new DataTable();
                if(dataGridViewList.DataSource==null){
                    for(int i=0;i<dataGridViewList.Columns.Count;i++){
                        dt.Columns.Add(dataGridViewList.Columns[i].Name);
                    }
                }
                else{
                    dt = (DataTable)dataGridViewList.DataSource;
                }
                DataRow dr = dt.NewRow();

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() != "��Ƭ")
                    {
                        if (dt.Columns[dataGridView1.Rows[i].Cells[0].Value.ToString()].DataType.ToString() == "System.Decimal" && dataGridView1.Rows[i].Cells[1].Value.ToString().Trim() == "")
                        { continue; }
                        else
                        {
                            dr[dataGridView1.Rows[i].Cells[0].Value.ToString()] = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        }                        
                    }
                }
                dr["x"] = x.ToString("#.######");
                dr["y"] = y.ToString("#.######");
                dt.Rows.Add(dr);
                dataGridViewList.DataSource = null;
                dataGridViewList.Columns.Clear();
                dataGridViewList.DataSource = dt;
                //dataGridViewList.Visible = true;

                setDataGridBG();
            }
            catch (Exception ex) {
                writeToLog(ex, "addToList");
            }
        }
        //feng
        private void GaaddToList(double x, double y)
        {
            try
            {
                if (dataGridViewGaList.Rows.Count == 0)
                {
                    dataGridViewGaList.Columns.Clear();
                    string stag = "";
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                    {
                        stag = dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                        if (stag != "��Ƭ")
                        {
                            dataGridViewGaList.Columns.Add(stag, stag);
                        }
                    }
                    //edit by fisher in 09-10-23
                    //dataGridViewGaList.Columns.Add("x", "x");
                    //dataGridViewGaList.Columns.Add("y", "y");
                }

                DataTable dt = new DataTable();
                if (dataGridViewGaList.DataSource == null)
                {
                    for (int i = 0; i < dataGridViewGaList.Columns.Count; i++)
                    {
                        dt.Columns.Add(dataGridViewGaList.Columns[i].Name);
                    }
                }
                else
                {
                    dt = (DataTable)dataGridViewGaList.DataSource;
                }
                DataRow dr = dt.NewRow();
                
                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                {
                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() != "��Ƭ")
                    {
                        if (dt.Columns[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()].DataType.ToString() == "System.Decimal" && dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString().Trim() == "")
                        { continue; }
                        else
                        {
                            dr[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                        }
                    }
                }
                dr["x"] = x.ToString("#.######");
                dr["y"] = y.ToString("#.######");
                dt.Rows.Add(dr);
                dataGridViewGaList.DataSource = null;
                dataGridViewGaList.Columns.Clear();
                dataGridViewGaList.DataSource = dt;
                //dataGridViewList.Visible = true;

                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaaddToList");
                //MessageBox.Show(ex.Message, "GaaddToList");
            }
        }

        private void updateMapValue()
        {
            try
            {
                if (ft != null)
                {
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text.Trim(), getFromNamePath);
                    string sname = "";
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjName)
                        {
                            sname = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    string sKey = "";
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                        {
                            sKey = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    ft["MapID"] = selPrinx;
                    ft["keyID"] = sKey;
                    ft["Name"] = sname;
                    ft.Update();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "updateMapValue");
            }
        }

        // feng
        private void GaupdateMapValue()
        {
            try
            {
                if (ft != null)
                {
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text.Trim(), getFromNamePath);
                    string sname = "";
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                    {
                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjName)
                        {
                            sname = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    string sKey = "";
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                    {
                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                        {
                            sKey = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    ft["MapID"] = selPrinx;
                    ft["keyID"] = sKey;
                    ft["Name"] = sname;
                    ft.Update();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaupdateMapValue");
                //MessageBox.Show(ex.Message, "GaupdateMapValue()");
            }
        }

        private void WriteEditLog(string sTabName, string editID, string editMothed)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();
                OracleCommand cmd;
                string strExe = "insert into ������¼ values('" + temEditDt.Rows[0]["userNow"].ToString() + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�༭ģ��','��" + sTabName + " ��¼MapID/��ţ�" + editID + "','" + editMothed + "')";
                cmd = new OracleCommand(strExe, Conn);
                cmd.ExecuteNonQuery();

                Conn.Close();
            }
            catch(Exception ex) {
                writeToLog(ex, "WriteEditLog");
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonCancel.Text == "ȡ�����")
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++) //���datagridview
                    {
                        dataGridView1.Rows[i].Cells[1].Value = "";
                    }
                    dataGridView1.Visible = false;
                    buttonSave.Enabled = false;
                    buttonCancel.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();
                    deleteFeature();
                }
                else if (buttonCancel.Text == "ȡ��")
                { 
                    try
                    {
                        for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                        {
                            if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "��Ƭ")
                                continue;
                            this.dataGridView1.Rows[i].Cells[1].Value = this.dataGridViewList.Rows[browIndex].Cells[dataGridView1.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        MessageBox.Show(ex.Message, "buttonCancel_Click(ȡ��)");
                    }

                    //��ͼ�ϵĵ�λ�ø�ԭ
                    Feature f;

                    //added by fisher in 09-09-23

                    if (this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value.ToString() == "" || this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value.ToString() == "" 
                     || this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value.ToString() == "0" || this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value.ToString() == "0")
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("MapID=" + Convert.ToInt32(this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        this.buttonSave.Enabled = false;
                        this.buttonCancel.Enabled = false;
                        this.mapControl1.Tools.LeftButtonTool = "Pan";
                        this.UncheckedTool();
                        btnLoc1.Enabled = false;
                        return;
                        //ɾ���������� 
                    }

                    if (Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value) != 0)
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("MapID=" + Convert.ToInt32(this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        //��ɾ��
                    }

                    //��ӵ�
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                    string sName = CLC.ForSDGA.GetFromTable.ObjName;

                    f = new Feature(this.editTable.TableInfo.Columns);
                    f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(Convert.ToDouble(this.dataGridViewList.Rows[browIndex].Cells["X"].Value), Convert.ToDouble(this.dataGridViewList.Rows[browIndex].Cells["Y"].Value)));
                    f["mapid"] = this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value.ToString();
                    f["name"] = this.dataGridViewList.Rows[browIndex].Cells[sName].Value.ToString();
                    f.Style = featStyle;
                    this.editTable.InsertFeature(f);
                    this.dataGridView1.CurrentCell = null;
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonCancel_Click");
            }
        }

        private void deleteFeature()
        {
            //ɾ��ͼԪ
            try
            {
                editTable.DeleteFeature(ft);
                ft = null;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "deleteFeature-ɾ��ͼԪ");
            }

            //���ÿ����
            string tabName = comboTables.Text.Trim();
            if (isOracleSpatialTab)
            {
                tabName += "_tem";
            }
            try
            {
                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "deleteFeature-���ÿ����");
            }
        }
        //feng
        private void GadeleteFeature()
        {
            //ɾ��ͼԪ
            try
            {
                editTable.DeleteFeature(ft);
                ft = null;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GadeleteFeature-ɾ��ͼԪ");
            }

            //���ÿ����
            string tabName = comboTable.Text.Trim();
            if (isOracleSpatialTab)
            {
                tabName += "_tem";
            }
            try
            {
                if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GadeleteFeature-���ÿ����");
            }
        }

        private MIConn dbMI = new MIConn();
        private void Feature_Changed(object sender, MapInfo.Tools.FeatureChangedEventArgs e)
        {
            if ((this.buttonCancel.Enabled == true && this.buttonCancel.Text=="ȡ�����")||(this.btnGaCancel.Enabled==true && this.btnGaCancel.Text=="ȡ�����"))
            {
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                string sql = "";
                if (e.FeatureChangeMode == FeatureChangeMode.Delete)  //ɾ��Ҫ��
                {
                    if (this.tabControl2.SelectedTab == tabPage1 && temEditDt.Rows[0]["���������ݿ�ɾ����"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("��û��ɾ��Ȩ�ޣ�");
                        return;
                    }
                    if (this.tabControl2.SelectedTab == tabPage3 && temEditDt.Rows[0]["��ҵ�����ݿ�ɾ����"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("��û��ɾ��Ȩ�ޣ�");
                        return;
                    }

                    //�ҵ�ѡ�ж�����������
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(ft.Geometry, IntersectType.Geometry);
                    si.QueryDefinition.Columns = null;
                    Feature ft1 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", si);

                    //Ϊ��ʱ,����������������,��ɾ��
                    if (ft1 != null)
                    {
                        if (strRegion != "˳����")
                        {
                            //���������ཻ,�������ڽ������ǲ����û���Χ,������ǲ���ɾ��.
                            if (Array.IndexOf(strRegion.Split(','), ft1["name"].ToString()) == -1)
                            {
                                editTable.InsertFeature(ft);
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("����ɾ��Ȩ�޷�Χ��Ķ���.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                    }

                    if (isOracleSpatialTab == false)
                    {
                        sql = "delete from " + this.editTable.Alias + " where   to_char(MapID) ='" + selMapID + "'";

                        Conn.Open();
                        OracleCommand cmd = new OracleCommand(sql, Conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        Conn.Close();
                        if (tabControl2.SelectedTab == tabPage1)
                        {
                            if (temEditDt.Rows[0]["���������ݿ�ɾ����"].ToString() == "1")
                            {
                                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                            }
                            MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                        }
                        if (tabControl2.SelectedTab == tabPage3)
                        {
                            if (temEditDt.Rows[0]["��ҵ�����ݿ�ɾ����"].ToString() == "1")
                            {
                                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);
                            }
                            MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);
                        }
                    }
                    else
                    {
                        if (tabControl2.SelectedTab == tabPage3 && comboTable.Text.Trim() == "�˿�ϵͳ")
                        {
                            try
                            {
                                int iID = 0;
                                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                                {
                                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString().Trim() == "���֤����")
                                    {
                                        iID = i;
                                        break;
                                    }
                                }
                                sql = "delete from �˿���Ƭ where ���֤����='" + dataGridViewGaInfo.Rows[iID].Cells[1].Value.ToString() + "'";
                                if (Conn.State == ConnectionState.Closed)
                                {
                                    Conn.Open();
                                }
                                OracleCommand cmd = new OracleCommand(sql, Conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                Conn.Close();

                                this.dataGridViewGaInfo.Visible = false;
                                this.btnGaSave.Enabled = false;
                                this.btnGaSave.Text = "����";
                                this.btnGaCancel.Enabled = false;
                            }
                            catch(Exception ex)
                            {
                                writeToLog(ex, "Feature_Changed");
                                if (Conn.State == ConnectionState.Open)
                                {
                                    Conn.Close();
                                }
                            }
                        }

                        MIConnection miConn = new MIConnection();
                        try
                        {
                            miConn.Open();
                            MICommand miCmd = miConn.CreateCommand();
                            miCmd.CommandText = "delete from " + lTable.Alias + " where MI_PRINX=" + selPrinx;
                            miCmd.ExecuteNonQuery();
                            miCmd.Dispose();
                            miConn.Close();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Changed");
                            if (miConn.State == ConnectionState.Open)
                                miConn.Close();
                        }
                    }
                    if (tabControl2.SelectedTab == tabPage1)
                    {
                        dataGridView1.Visible = false;
                        buttonSave.Enabled = false;
                        buttonSave.Text = "����";
                        buttonCancel.Enabled = false;

                        reMoveListValue(ft["keyID"].ToString());
                        setDataGridBG();

                        WriteEditLog(comboTables.Text.Trim(), selMapID, "ɾ��");
                    }
                    else if (tabControl2.SelectedTab == tabPage3)
                    {
                        this.dataGridViewGaInfo.Visible = false;
                        this.btnGaSave.Enabled = false;
                        this.btnGaSave.Text = "����";
                        this.btnGaCancel.Enabled = false;

                        GareMoveListValue(ft["keyID"].ToString());
                        GasetDataGridBG();

                        WriteEditLog(comboTable.Text.Trim(), selMapID, "ɾ��");
                    }
                }
                else if (e.FeatureChangeMode == FeatureChangeMode.Move)  // �ƶ�Ҫ��
                {
                    if (this.tabControl2.SelectedTab == tabPage1 && temEditDt.Rows[0]["�������ݿɱ༭"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("��û���ƶ�Ȩ�ޣ�");
                        return;
                    }
                    if (this.tabControl2.SelectedTab == tabPage3 && temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("��û���ƶ�Ȩ�ޣ�");
                        return;
                    }
                    //add by fisher in 09-12-10
                    if (tabControl2.SelectedTab == tabPage2)
                    {
                        OracleConnection conn = new OracleConnection(mysqlstr);
                        try
                        {
                            conn.Open();
                            //�ҵ��ƶ����Ҫ��
                            SearchInfo sio = MapInfo.Data.SearchInfoFactory.SearchWhere("keyID = '" + selMapID + "'");
                            sio.QueryDefinition.Columns = null;
                            Feature ff = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, sio);

                            //�����ƶ����Ҫ�����ڵ�����
                            sio = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(ff.Geometry, IntersectType.Geometry);
                            sio.QueryDefinition.Columns = null;
                            Feature vft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", sio);

                            if (vft == null)  //���ܽ������ƶ�������Χ��
                            {
                                editTable.DeleteFeature(ff);
                                editTable.InsertFeature(ft);
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("���ܽ������ƶ���Ȩ�޷�Χ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            else
                            {
                                bool quyuCZ = false;  // �����ж��ƶ��ĵ��ǲ��������������Ȩ�޷�Χ��
                                if (strRegion != "˳����")
                                {
                                    //���������ཻ,�������ڽ������ǲ����û���Χ
                                    if (Array.IndexOf(this.strRegion.Split(','), vft["name"].ToString()) > -1)
                                    {
                                        quyuCZ = true;
                                    }
                                    if (quyuCZ == false)
                                    {
                                        editTable.DeleteFeature(ff);
                                        editTable.InsertFeature(ft);
                                        this.Cursor = Cursors.Default;
                                        MessageBox.Show("���ܽ������ƶ���Ȩ�޷�Χ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return;
                                    }
                                }
                            }

                            string vmoveStr = "update ��Ƶλ�� set X = " + ff.Geometry.Centroid.x + ",Y = " + ff.Geometry.Centroid.y + " where �豸��� = '" + selMapID + "'";
                            int s = 0;
                            // �ҵ���ѡ��ĵ�����dataGridViewVideo�е���һ��
                            for (int i = 0; i < this.dataGridViewVideo.Rows.Count; i++)
                            {
                                if (this.dataGridViewVideo.Rows[i].Cells["�豸���"].Value.ToString() == selMapID)
                                {
                                    s = i;
                                    break;
                                }
                            }
                            // ���δ�϶�֮ǰ������
                            double xSize = Convert.ToDouble(this.dataGridViewVideo.Rows[s].Cells["X"].Value);
                            double ySize = Convert.ToDouble(this.dataGridViewVideo.Rows[s].Cells["Y"].Value);
                            // �Ƚ��Ƿ����϶�
                            if (xSize == ff.Geometry.Centroid.x && ySize == ff.Geometry.Centroid.y)
                            {
                                return;
                            }
                            // �������ı�dataGridViewVideo�е�ֵ
                            this.dataGridViewVideo.Rows[s].Cells["X"].Value = ff.Geometry.Centroid.x;
                            this.dataGridViewVideo.Rows[s].Cells["Y"].Value = ff.Geometry.Centroid.y;

                            // Ȼ��������ݿ�����ֵ
                            OracleCommand cmd = conn.CreateCommand();
                            cmd.CommandText = vmoveStr;
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            conn.Close();
                            this.Cursor = Cursors.Default;
                            MessageBox.Show("�����ƶ��ɹ�,�ѱ���!", "��ʾ");
                            return;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "�ƶ�Ҫ��");
                            writeToLog(ex, "Feature_Changed");
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }

                    //�ҵ��ƶ����Ҫ��
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("MapID=" + selPrinx);
                    si.QueryDefinition.Columns = null;
                    Feature f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                    Feature fuwei = ft;   //����һ��������λ��feature,���������ƶ���λ��  add by fisher in 09-12-30
                    ft = f;

                    //�ҵ���Ӷ�����������
                    si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                    si.QueryDefinition.Columns = null;
                    Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", si);

                    //�ҵ��ƶ����������ж�Ͻ��  (���´�����fisher���  09-12-1)
                    si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                    si.QueryDefinition.Columns = null;
                    Feature ft3 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("�ж�Ͻ��", si);
                    string zdmc = "";  //�ж�����

                    if (ft3 != null)
                    {
                        string zddm = ft3["�жӴ���"].ToString();
                        OracleConnection conn = new OracleConnection(mysqlstr);
                        try
                        {
                            conn.Open();
                            OracleCommand cmd = new OracleCommand("select �ж��� from �������ж� where �жӴ��� = " + zddm, conn);
                            OracleDataReader dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                dr.Read();
                                zdmc = dr.GetValue(0).ToString();
                            }
                            dr.Close();
                            cmd.Dispose();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Changed");
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }

                    //Ϊ��ʱ,����������������,�����ƶ�
                    if (ft2 == null)
                    {
                        editTable.DeleteFeature(f);
                        editTable.InsertFeature(fuwei);
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("���ܽ������ƶ���Ȩ�޷�Χ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        bool quyuCZ = false;  //�����ж��ƶ��ĵ��ǲ��������������Ȩ�޷�Χ��
                        if (strRegion != "" || strRegion1 != "")
                        {
                            if (strRegion != "˳����")
                            {
                                if (strRegion != "")
                                {
                                    //���������ཻ,�������ڽ������ǲ����û���Χ
                                    if (Array.IndexOf(strRegion.Split(','), ft2["name"].ToString()) > -1)
                                    {
                                        quyuCZ = true;
                                    }
                                }
                                if (strRegion1 != "")
                                {
                                    if (Array.IndexOf(strRegion1.Split(','), zdmc) > -1)
                                    {
                                        quyuCZ = true;
                                    }
                                }
                                if (quyuCZ == false)
                                {
                                    editTable.DeleteFeature(f);
                                    editTable.InsertFeature(fuwei);
                                    this.Cursor = Cursors.Default;
                                    MessageBox.Show("���ܽ������ƶ���Ȩ�޷�Χ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                        }
                    }

                    if (isOracleSpatialTab)
                    {
                        //if (f.Geometry.Type == GeometryType.MultiPolygon)
                        //{
                        //�ҵ�oracleSpatial���ж�Ӧ��Ҫ��
                        si = MapInfo.Data.SearchInfoFactory.SearchWhere("MI_PRINX=" + selPrinx);
                        si.QueryDefinition.Columns = null;
                        Feature newFeat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(lTable.Alias, si);

                        //�½�����,ΪԭҪ�ص�����+�ƶ����geometry
                        Feature addFeat = newFeat;
                        addFeat.Geometry = f.Geometry;
                        //newFeat.Geometry = f.Geometry;
                        //newFeat.Update();

                        //���´�����fisher��� ��09-09-04��
                        if (this.tabControl2.SelectedTab == tabPage1)
                        {
                            double xSize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value);
                            if (xSize == addFeat.Geometry.GeometricCentroid.x && ySize == addFeat.Geometry.GeometricCentroid.y)
                            {
                                return;
                            }
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = addFeat.Geometry.GeometricCentroid.x;
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = addFeat.Geometry.GeometricCentroid.y;
                        }
                        if (this.tabControl2.SelectedTab == tabPage3)
                        {
                            double xSize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value);
                            if (xSize == addFeat.Geometry.GeometricCentroid.x && ySize == addFeat.Geometry.GeometricCentroid.y)
                            {
                                return;
                            }
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = addFeat.Geometry.GeometricCentroid.x;
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = addFeat.Geometry.GeometricCentroid.y;
                        }

                        //��ɾ��ԭ����Ҫ��,�������λ�õ�Ҫ��
                        lTable.DeleteFeature(newFeat);
                        lTable.InsertFeature(addFeat);

                    }
                    else
                    {
                        ////�ҵ��ƶ����Ҫ��
                        //si = MapInfo.Data.SearchInfoFactory.SearchWhere("MapID=" + selMapID);
                        //si.QueryDefinition.Columns = null;
                        //ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);

                        //�������ݿ��ж�ӦҪ�ص�����  
                        //���´�����fisher��� ��09-09-04��
                        if (this.tabControl2.SelectedTab == tabPage1)
                        {
                            double xSize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value);
                            if (xSize == f.Geometry.Centroid.x && ySize == f.Geometry.Centroid.y)
                            {
                                return;
                            }
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = f.Geometry.Centroid.x;
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = f.Geometry.Centroid.y;
                        }
                        if (this.tabControl2.SelectedTab == tabPage3)
                        {
                            double xSize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value);
                            if (xSize == f.Geometry.Centroid.x && ySize == f.Geometry.Centroid.y)
                            {
                                return;
                            }
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = f.Geometry.Centroid.x;
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = f.Geometry.Centroid.y;
                        }
                    }

                    //���´�����fisher��ӣ��ƶ�ʱֱ�Ӹ��£�09-12-31��
                    if (tabControl2.SelectedTab == tabPage3)   //����ҵ������
                    {
                        MoveUpData(dataGridViewGaList, dataGridViewGaInfo, comboTable.Text, rowIndex);
                    }
                    else if (tabControl2.SelectedTab == tabPage1) //�������ݱ༭
                    {
                        MoveUpData(dataGridViewList, dataGridView1, comboTables.Text, browIndex);
                    }
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;
                    WriteEditLog(comboTables.Text.Trim(), selMapID, "�ƶ�");
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                writeToLog(ex, "Feature_Changed");
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            this.Cursor = Cursors.Default;
        }

        //����һ���������������ƶ���ֱ�Ӹ����ƶ�Ҫ�� fisher in 09-12-31
        private void MoveUpData(DataGridView dGVList, DataGridView dGVInfo, string tableName,int Rowid)
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            OracleCommand cmd = null;
            try
            {
                conn.Open();
                string command = "update " + tableName + " t set ";
                string strValue = "";
                for (int i = 0; i < dGVInfo.RowCount; i++)
                {
                    strValue = "";
                    if (dGVInfo.Rows[i].Cells[0].Value.ToString() == "X")
                    {
                        strValue = dGVInfo.Rows[i].Cells[1].Value.ToString();
                        if (isOracleSpatialTab)
                        {
                            command += "t.GEOLOC.SDO_POINT.X = '" + strValue + "',";
                        }
                        else
                        {
                            command += "t." + dGVInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                        }
                    }
                    else if (dGVInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                    {
                        strValue = dGVInfo.Rows[i].Cells[1].Value.ToString();
                        if (isOracleSpatialTab)
                        {
                            command += "t.GEOLOC.SDO_POINT.Y = '" + strValue + "',";
                        }
                        else
                        {
                            command += "t." + dGVInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                        }
                    }
                }
                command = command.Remove(command.Length - 1);
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                command += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + dGVList.Rows[Rowid].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                cmd = new OracleCommand(command, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
                MessageBox.Show("�����ƶ��ɹ�,�ѱ���!", "��ʾ");
            }
            catch (Exception ex)
            {
                WriteEditLog(comboTables.Text.Trim(), selMapID, "�ƶ�Ҫ�ظ���");
                MessageBox.Show(ex.Message, "�ƶ�Ҫ�ظ���");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void reMoveListValue(string keyID)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                for (int i = 0; i < dataGridViewList.Rows.Count; i++)
                {
                    if (dataGridViewList.Rows[i].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() == keyID)
                    {
                        dataGridViewList.Rows.RemoveAt(i);
                        return;
                    }
                }
                setDataGridBG();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "reMoveListValue");
            }
        }

        private void GareMoveListValue(string keyID)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (dataGridViewGaList.Rows[i].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() == keyID)
                    {
                        dataGridViewGaList.Rows.RemoveAt(i);
                        return;
                    }
                }
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GareMoveListValue");
            }
        }

        string selMapID = "";
        int selPrinx = -1;
        private void Feature_Selected(object sender, FeatureSelectedEventArgs e)
        {
            if (tabControl2.SelectedTab == tabPage2)  // add by fisher in 09-12-10
            {
                try
                {
                    IResultSetFeatureCollection fc = e.Selection[this.editTable];
                    if (fc == null || fc.Count == 0)
                    {
                        selMapID = "";
                        return;
                    }
                    ft = fc[0];
                    selMapID = ft["keyID"].ToString();
                }
                catch(Exception ex) 
                {
                    writeToLog(ex, "Feature_Selected");
                }
            }

            if (tabControl2.SelectedTab == tabPage3)   //����ҵ��༭ģ��  by feng 09-08-21
            {
                if (this.btnGaCancel.Enabled == true && this.btnGaCancel.Text=="ȡ�����")
                {
                    MessageBox.Show("�������һ����¼��������Ӻͱ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Session.Current.Selections.DefaultSelection.Clear();
                    return;
                }

                try
                {
                    IResultSetFeatureCollection fc = e.Selection[this.editTable];
                    if (fc == null || fc.Count == 0)
                    {
                        GaNodesEdit.Enabled = false;
                        GaAddNode.Enabled = false;
                        dataGridViewGaInfo.Visible = false;
                        btnGaSave.Enabled = false;
                        selPrinx = -1;
                        selMapID = "";
                        return;
                    }
                    //toolBarNodesEdit.Enabled = true;
                    ft = feature = fc[0];
                    selPrinx = Convert.ToInt32(feature["MapID"]);
                    selMapID = feature["keyID"].ToString();
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                    MessageBox.Show(ex.Message, "Feature_Selected000");
                }

                try
                {
                    this.dataGridViewGaInfo.Visible = true;
                    this.btnGaSave.Text = "����";
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Text = "ȡ��";
                    this.btnGaCancel.Enabled = false;
                    
                    if (isOracleSpatialTab)
                    {
                        MIConnection miConnection = new MIConnection();
                        miConnection.Open();
                        Table changeTable = null;
                        CLC.ForSDGA.GetFromTable.GetFromName(this.comboTable.Text.Trim(), getFromNamePath);
                        string tableName = CLC.ForSDGA.GetFromTable.TableName;
                        if (tableName == null || tableName == "")
                        {
                            tableName = this.comboTable.Text.Trim();
                        }
                        TableInfoServer ti = new TableInfoServer("change" + tableName);  //��������ti�ı���
                        ti.ConnectString = "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password;
                        ti.Query = "Select * From  " + tableName + "  where MI_PRINX = " + selPrinx + " ";
                        ti.Toolkit = ServerToolkit.Oci;
                        ti.CacheSettings.CacheType = CacheOption.Off;

                        changeTable = miConnection.Catalog.OpenTable(ti);
                        miConnection.Close();

                        MICommand miCommand = miConnection.CreateCommand();
                        MIDataReader miReader = null;

                        try
                        {
                            miConnection.Open();
                            miCommand.CommandText = "select * from  " + changeTable.Alias + " ";
                            miReader = miCommand.ExecuteReader();
                            int i = 0;
                            int iFieldsCount = this.dataGridViewGaInfo.Rows.Count;
                            
                            if (miReader.Read())
                            {
                                for (i = 0; i < iFieldsCount; i++)
                                {
                                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "��Ƭ")
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                                        fileName = "";  // ��Ƭ·�� 
                                    }
                                    else if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = e.MapCoordinate.x;
                                    }
                                    else if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = e.MapCoordinate.y;
                                    }
                                    else
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = miReader.GetValue(this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()).ToString();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                        finally
                        {
                            if (miReader != null)
                            {
                                miReader.Dispose();
                            }
                            if (miCommand != null)
                            {
                                miCommand.Dispose();
                            }
                            if (miConnection != null)
                            {
                                miConnection.Dispose();
                            }
                        }
                        changeTable.Close();     
                    }
                    else
                    {
                        try
                        {
                            CLC.ForSDGA.GetFromTable.GetFromName(this.comboTable.Text,getFromNamePath);
                            string tableName = CLC.ForSDGA.GetFromTable.TableName;
                            OracelData linkData = new OracelData(mysqlstr);
                            DataTable dt = linkData.SelectDataBase("select * from  " + tableName + "  where ��� = '" + selMapID + "'");
                            for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                            {
                                this.dataGridViewGaInfo.Rows[i].Cells[1].Value = dt.Rows[0][this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()];
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                    }

                    //add by siumo 208-12-31:�����ֻ����ӹ��ܵ��û�,ѡ��Ҫ�غ�ֻ�ܲ鿴��Ϣ,�����޸�,����datagridview���ɱ༭
                    if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() != "1")
                    {
                        dataGridViewGaInfo.Columns[1].ReadOnly = true;
                        //buttonSave.Enabled = false;
                    }
                    else
                    {
                        dataGridViewGaInfo.Columns[1].ReadOnly = false;
                        //buttonSave.Enabled = true;
                    }

                    tabControl3.SelectedTab = tabGaInfo;

                    //���´�����fisher��ӣ�ּ��ͨ����ǰѡ�е�featureȥѰ��dataGridViewGaList����֮���Ӧ����  (09-09-04)
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    string fobjId = CLC.ForSDGA.GetFromTable.ObjID;
                    if (isOracleSpatialTab)
                    {
                        for (int i = 0; i < this.dataGridViewGaList.Rows.Count; i++)
                        {
                            if (this.dataGridViewGaList.Rows[i].Cells["MapID"].Value.ToString() == selMapID)
                            {
                                rowIndex = i;
                                this.dataGridViewGaList.CurrentCell = this.dataGridViewGaList.Rows[rowIndex].Cells[fobjId];
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.dataGridViewGaList.Rows.Count; i++)
                        {
                            if (this.dataGridViewGaList.Rows[i].Cells["���"].Value.ToString() == selMapID)
                            {
                                rowIndex = i;
                                this.dataGridViewGaList.CurrentCell = this.dataGridViewGaList.Rows[rowIndex].Cells[fobjId];
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                }

                this.btnGaCancel.Enabled = false;
                this.btnGaSave.Enabled = false; 
            }
            //................................................

            if (tabControl2.SelectedTab == tabPage1)
            {
                if (this.buttonCancel.Enabled == true && this.buttonCancel.Text == "ȡ�����")
                {
                    MessageBox.Show("�������һ����¼��������Ӻͱ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Session.Current.Selections.DefaultSelection.Clear();
                    return;
                }

                try
                {
                    IResultSetFeatureCollection fc = e.Selection[this.editTable];
                    if (fc == null || fc.Count == 0)
                    {
                        toolBarNodesEdit.Enabled = false;
                        toolBarAddNode.Enabled = false;
                        dataGridView1.Visible = false;
                        buttonSave.Enabled = false;
                        selPrinx = -1;
                        selMapID = "";
                        return;
                    }
                    toolBarNodesEdit.Enabled = true;
                    ft = feature = fc[0];

                    selPrinx = Convert.ToInt32(feature["MapID"]);
                    selMapID = feature["keyID"].ToString();
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                }

                try
                {
                    this.dataGridView1.Visible = true;
                    this.buttonSave.Text = "����";
                    this.buttonCancel.Text = "ȡ��";
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;
                    //this.buttonSave.Enabled = true;  // by feng 09-08-21

                    if (isOracleSpatialTab)
                    {
                        MIConnection miConnection = new MIConnection();
                        miConnection.Open();
                        Table changeTable = null;
                        CLC.ForSDGA.GetFromTable.GetFromName(this.comboTables.Text.Trim(), getFromNamePath);
                        string tableName = CLC.ForSDGA.GetFromTable.TableName;
                        if (tableName == null || tableName == "")
                        {
                            tableName = this.comboTables.Text.Trim();
                        }
                        TableInfoServer ti = new TableInfoServer("change" + tableName);
                        ti.ConnectString = "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password;
                        ti.Query = "Select * From  " + tableName + "  where MI_PRINX = " + selPrinx + " ";
                        ti.Toolkit = ServerToolkit.Oci;
                        ti.CacheSettings.CacheType = CacheOption.Off;

                        changeTable = miConnection.Catalog.OpenTable(ti);
                        miConnection.Close();

                        MICommand miCommand = miConnection.CreateCommand();
                        MIDataReader miReader = null;

                        try
                        {
                            miConnection.Open();
                            miCommand.CommandText = "select * from  " + changeTable.Alias + " ";
                            miReader = miCommand.ExecuteReader();
                            int i = 0;
                            int iFieldsCount = this.dataGridView1.Rows.Count;
                            
                            if (miReader.Read())
                            {
                                for (i = 0; i < iFieldsCount; i++)
                                {
                                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        this.dataGridView1.Rows[i].Cells[1].Value = e.MapCoordinate.x;
                                    }
                                    else if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        this.dataGridView1.Rows[i].Cells[1].Value = e.MapCoordinate.y;
                                    }
                                    else
                                    {
                                        this.dataGridView1.Rows[i].Cells[1].Value = miReader.GetValue(this.dataGridView1.Rows[i].Cells[0].Value.ToString()).ToString();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                        finally
                        {
                            if (miReader != null)
                            {
                                miReader.Dispose();
                            }
                            if (miCommand != null)
                            {
                                miCommand.Dispose();
                            }
                            if (miConnection != null)
                            {
                                miConnection.Dispose();
                            }
                        }
                        changeTable.Close();
                    }
                    else
                    {
                        try
                        {
                            CLC.ForSDGA.GetFromTable.GetFromName(this.comboTables.Text, getFromNamePath);
                            string tableName = CLC.ForSDGA.GetFromTable.TableName;
                            OracelData linkData = new OracelData(mysqlstr);
                            DataTable dt = linkData.SelectDataBase("select * from  " + tableName + "  where ��� = '" + selMapID + "'");
                            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                            {
                                this.dataGridView1.Rows[i].Cells[1].Value = dt.Rows[0][this.dataGridView1.Rows[i].Cells[0].Value.ToString()];
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                    }

                    //add by siumo 208-12-31:�����ֻ����ӹ��ܵ��û�,ѡ��Ҫ�غ�ֻ�ܲ鿴��Ϣ,�����޸�,����datagridview���ɱ༭
                    if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() != "1")
                    {
                        dataGridView1.Columns[1].ReadOnly = true;
                        //buttonSave.Enabled = false;
                    }
                    else
                    {
                        dataGridView1.Columns[1].ReadOnly = false;
                        //buttonSave.Enabled = true;
                    }

                    tabControl1.SelectedTab = tabPageInfo;
                    //���´�����fisher��ӣ�ּ��ͨ����ǰѡ�е�featureȥѰ��dataGridViewList����֮���Ӧ����  (09-09-04)
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                    string bobjID = CLC.ForSDGA.GetFromTable.ObjID;
                    if (isOracleSpatialTab)
                    {
                        for (int i = 0; i < this.dataGridViewList.Rows.Count; i++)
                        {
                            if (this.dataGridViewList.Rows[i].Cells["MapID"].Value.ToString() == selMapID)
                            {
                                browIndex = i;
                                this.dataGridViewList.CurrentCell = this.dataGridViewList.Rows[browIndex].Cells[bobjID];
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.dataGridViewList.Rows.Count; i++)
                        {
                            if (this.dataGridViewList.Rows[i].Cells["���"].Value.ToString() == selMapID)
                            {
                                browIndex = i;
                                this.dataGridViewList.CurrentCell = this.dataGridViewList.Rows[browIndex].Cells[bobjID];
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                }
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)//�����䣬�����ж�����ĸ�ʽ������ȷ
        {
            //OracleConnection Conn = new OracleConnection(mysqlstr);
            string type = "";//�õ�����
            try
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "���֤����")
                {
                    if (dataGridView1.Rows[e.RowIndex].Cells[1].Value != null)
                    {
                        CheckIdentity(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                    }
                }
                else
                {
                    if (this.dataGridView1.CurrentCell == null || this.dataGridView1.CurrentCell.Value == null)
                    {
                        return;
                    }

                    string value = this.dataGridView1.CurrentCell.Value.ToString().Trim();
                    type = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value.ToString();
                    switch (type)
                    {
                        case "NUMBER":
                        case "INTEGER":
                        case "LONG":
                            this.checkNumber(value);
                            break;
                        case "FLOAT":
                        case "DOUBLE":
                            this.checkFloat(value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridView1_CellEndEdit");
            }
        }

        private void CheckIdentity(string strID)
        {
            try
            {
                if (strID.Length != 15 && strID.Length != 18)
                {
                    System.Windows.Forms.MessageBox.Show("��ȷ������������֤����Ϊ15λ��18λ!");
                    this.dataGridView1.CurrentCell.Value = string.Empty;
                    return;
                }
                string str = strID.Substring(0, strID.Length - 1);
                if (!System.Text.RegularExpressions.Regex.IsMatch(strID.Trim(), @"^\d+(\d*)?$"))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))
                    {
                        System.Windows.Forms.MessageBox.Show("��ȷ������������֤�����ʽ��ȷ!");
                        this.dataGridView1.CurrentCell.Value = string.Empty;
                    }
                    if (strID.Substring(strID.Length - 1, 1).ToUpper() != "X")
                    {
                        System.Windows.Forms.MessageBox.Show("��ȷ������������֤�����ʽ��ȷ!");
                        this.dataGridView1.CurrentCell.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CheckIdentity");
            }
        }

        private void GaCheckIdentity(string strID)
        {
            try
            {
                if (strID.Length != 15 && strID.Length != 18)
                {
                    System.Windows.Forms.MessageBox.Show("��ȷ������������֤����Ϊ15λ��18λ!");
                    this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                    return;
                }
                string str = strID.Substring(0, strID.Length - 1);
                if (!System.Text.RegularExpressions.Regex.IsMatch(strID.Trim(), @"^\d+(\d*)?$"))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))
                    {
                        System.Windows.Forms.MessageBox.Show("��ȷ������������֤�����ʽ��ȷ!");
                        this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                    }
                    if (strID.Substring(strID.Length - 1, 1).ToUpper() != "X")
                    {
                        System.Windows.Forms.MessageBox.Show("��ȷ������������֤�����ʽ��ȷ!");
                        this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaCheckIdentity");
            }
        }

        private bool hasUpdate = false;
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // add by fisher in 09-12-29
            this.buttonSave.Enabled = true;
            this.buttonCancel.Enabled = true;

            if(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString()=="��Ƭ"){
                System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(mysqlstr);
                System.Data.OracleClient.OracleCommand oracleCmd = null;
                System.Data.OracleClient.OracleDataReader dataReader = null;

                FrmImage fimage = new FrmImage();
                if (buttonCancel.Enabled)
                {
                    if (fimage.ShowDialog(this) == DialogResult.OK)
                    {
                        fileName = fimage.openFileDialog.FileName;
                        dataGridView1.Rows[e.RowIndex].Cells[1].Value = fileName;
                        hasUpdate = fimage.hasUpdate;
                    }
                }
                else
                {
                    try
                    {
                        myConnection.Open();
                        int iID = 0;
                        for (int i = 0; i < dataGridView1.Rows.Count;i++ )
                        {
                            if (this.dataGridView1.Rows[i].Cells[0].Value.ToString().Trim() == "���֤����")
                            {
                                iID = i;
                                break;
                            }
                        }
                        string sqlstr = "select ��Ƭ from  �˿���Ƭ where ���֤����='" + this.dataGridView1.Rows[iID].Cells[1].Value.ToString() + "'  ";
                        oracleCmd = new OracleCommand(sqlstr, myConnection);
                        dataReader = oracleCmd.ExecuteReader();

                        try
                        {
                            if (dataReader.Read())
                            {
                                byte[] bytes = new byte[2000000];
                                long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, 2000000);
                                Stream fs = new MemoryStream(bytes);
                                fimage.pictureBox1.Image = Image.FromStream(fs);
                                fimage.pictureBox1.Invalidate();
                                fs.Close();
                            }
                            dataReader.Close();
                        }
                        catch (Exception ex) { writeToLog(ex, "dataGridView1_CellBeginEdit"); }
                        if (fimage.ShowDialog(this) == DialogResult.OK)
                        {
                            fileName = fimage.openFileDialog.FileName;
                            dataGridView1.Rows[e.RowIndex].Cells[1].Value = fileName;
                            hasUpdate = fimage.hasUpdate;
                        }
                    }

                    catch (Exception ex)
                    {
                        writeToLog(ex, "dataGridView1_CellBeginEdit");
                    }
                    finally
                    {
                        if (oracleCmd != null)
                            oracleCmd.Dispose();
                        if (myConnection.State == ConnectionState.Open)
                            myConnection.Close();
                    }
                }
                try
                {
                    dataGridView1.CurrentCell = null;
                }
                catch(Exception ex) {
                    writeToLog(ex, "dataGridView1_CellBeginEdit");
                }
            }
            else{
                string type = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value.ToString();

                try
                {
                    if (type == "DATE") {
                        FrmMonthCalendar fCalendar = new FrmMonthCalendar();
                        if (fCalendar.ShowDialog() == DialogResult.OK) {
                            dataGridView1.CurrentCell.Value = fCalendar.dateString;
                        }
                        dataGridView1.CurrentCell = null;
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "dataGridView1_CellBeginEdit");
                }
            }
        }

        private void checkNumber(string str)//�ж�������ǲ�������
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//�ж�������ǲ�������
                {

                    MessageBox.Show("�������֣�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridView1.CurrentCell.Value = string.Empty;                   
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "checkNumber");
            }
        }

        private void GacheckNumber(string str)//�ж�������ǲ�������,�����༭ģ��  feng
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//�ж�������ǲ�������
                {

                    MessageBox.Show("���������֣�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex,"GacheckNumber");
            }
        }

        private void checkFloat(string str)//�ж�������ǲ�������
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\.\d*)?$"))//�ж�������ǲ���flaot
                {
                    MessageBox.Show("�������֣����Դ�С����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridView1.CurrentCell.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "checkFloat");
            }
        }

        private void GacheckFloat(string str)//�ж�������ǲ�������,�����༭ģ��  feng
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\.\d*)?$"))//�ж�������ǲ���flaot
                {
                    MessageBox.Show("�������֣����Դ�С����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GacheckFloat");
            }
        }

        private void comboTables_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.Visible && buttonCancel.Enabled)
            {
                MessageBox.Show("�������һ����¼��������Ӻͱ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            searchByKey();
        }

        //���±������ڱ༭�ķ�ҳ��ʾ
        int bpageSize = 100;     //ÿҳ��ʾ����
        int bnMax = 0;         //�ܼ�¼��
        int bpageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int bpageCurrent = 0;   //��ǰҳ��
        int bnCurrent = 0;      //��ǰ��¼��

        string bPageSQL = "";   //���ڻ�÷�ҳ����
        string blctStr="";        //�༭ģ���� ��λ���ַ���

        private void searchByKey()
        {
            isShowPro(true);
            dataGridViewList.Columns.Clear();
            if (textKeyWord.Text.Trim() == "")
            {
                isShowPro(false);
                MessageBox.Show("������ؼ��ʣ�","��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);

            string strSQL = "";
            if (isOracleSpatialTab)
            {
                strSQL = "select MI_PRINX as MapID," +CLC.ForSDGA.GetFromTable.SQLFields +" from " + comboTables.Text + " t";
                bPageSQL = "select MI_PRINX as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text + " t";
                blctStr = "select MI_PRINX as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text + " t";
            }
            else
            {
                strSQL = "select rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text;
                bPageSQL = "select  rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text;
                blctStr = "select  rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text;
            }

            if (cbMohu.Checked)
            {
                strSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " like '%" + textKeyWord.Text.Trim() + "%'";
                bPageSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " like '%" + textKeyWord.Text.Trim() + "%'";
                blctStr += " where " + CLC.ForSDGA.GetFromTable.ObjName + " like '%" + textKeyWord.Text.Trim() + "%'";
            }
            else
            {
                strSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " ='" + textKeyWord.Text.Trim() + "'";
                bPageSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " ='" + textKeyWord.Text.Trim() + "'";
                blctStr += " where " + CLC.ForSDGA.GetFromTable.ObjName + " ='" + textKeyWord.Text.Trim() + "'";
            }

            //alter by siumo 2009-03-10
            //string strRegion = strRegion;

            if (strRegion != "˳����" && strRegion != "")
            {
                if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                {
                    strRegion = strRegion.Replace("����", "����,��ʤ");
                }

                strSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                bPageSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                blctStr += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
            }
            if (strRegion1 != "" && comboTables.Text == "����������")
            {
                strSQL += " and " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
                bPageSQL += " and " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
                blctStr += " and " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
            }
            this.toolEditPro.Value = 1;
            Application.DoEvents();
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(strSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                dAdapter.Dispose();
                conn.Close();
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.toolStripRecord.Text = "0��";
                    this.tsTextBoxPageNow.Text = "0";//���õ�ǰҳ
                    this.tStripLabelPageCount.Text = "/ {0}";//������ҳ��
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    return;
                }

                //���´����������÷�ҳ
                try
                {
                    bnMax = dt.Rows.Count;
                    this.toolStripRecord.Text = bnMax.ToString() + "��";
                    bpageSize = 100;      //����ҳ������
                    bpageCount = (bnMax / bpageSize);//�������ҳ��
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//������ҳ��
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    if (bnMax != 0)
                    {
                        bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//���õ�ǰҳ
                    }
                    bnCurrent = 0;       //��ǰ��¼����0��ʼ
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "searchByKey()��ȡ��ҳ����");
                    MessageBox.Show(ex.Message, "searchByKey() ��ȡ��ҳ����");
                }


                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //��ȡ��ǰҳ����
                dataGridViewList.DataSource = datatable;

                if (dataGridViewList.Columns["MAPID"] != null)
                {
                    dataGridViewList.Columns["MAPID"].Visible = false;
                }
                //dataGridViewList.Visible = true;

                setDataGridBG();
                this.toolEditPro.Value = 2;
                //Application.DoEvents();

                this.insertQueryIntoTable(dt);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//���datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "searchByKey");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            this.toolEditPro.Value = 3;
            Application.DoEvents();
            isShowPro(false);
            Cursor.Current = Cursors.Default;
        }

        //�����б��еı���,�����ɫ.
        private void setDataGridBG()
        {
            try
            {
                for (int i = 0; i < dataGridViewList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setDataGridBG");
            }
        }

        private void GasetDataGridBG()
        {
            try
            {
                if (dataGridViewGaList.Rows.Count == 0) return;
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (comboTable.Text.Trim() == "��ȫ������λ")
                    {
                        // ����ȫ������λ���ļ���������
                        DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                        dgvlc.Value = "����༭";
                        dgvlc.ToolTipText = "�鿴��ȫ������λ�ļ�";
                        dataGridViewGaList.Rows[i].Cells["��ȫ������λ�ļ�"] = dgvlc;
                    }
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GasetDataGridBG");
            }
        }

        private void insertQueryIntoTable(DataTable dataTable)//����ѯ���Ľ����ӵ�����
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("û����ض����������ѯ�ؼ��ʣ�");
                    (this.editTable as IFeatureCollection).Clear();
                    this.editTable.Pack(PackType.All);
                    return;
                }
                (this.editTable as IFeatureCollection).Clear();
                this.editTable.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                writeToLog(ex,"insertQueryIntoTable");
                (this.editTable as IFeatureCollection).Clear();
                this.editTable.Pack(PackType.All);
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                double dX = 0, dY = 0;
                try
                {
                    dX = Convert.ToDouble(dataTable.Rows[i]["X"]);
                    dY = Convert.ToDouble(dataTable.Rows[i]["Y"]);

                    if (dX > 0 && dY > 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(this.editTable)).CoordSys, dX, dY);
                        string tabName = comboTables.Text;
                        if (tabControl2.SelectedTab == tabPage2) 
                        {
                            tabName = "��Ƶ";
                        }
                        CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                        string sName =CLC.ForSDGA.GetFromTable.ObjName;
                        string strID = CLC.ForSDGA.GetFromTable.ObjID;

                        Feature pFeat = new Feature(editTable.TableInfo.Columns);

                        pFeat.Geometry = pt;
                        pFeat.Style = featStyle;
                        if (dataTable.Columns.Contains("MapID"))
                        {
                            pFeat["MapID"] = dataTable.Rows[i]["MapID"].ToString();
                        }
                        //else {
                        //    if (tabName == "�ΰ�����ϵͳ")
                        //    {
                        //        pFeat["MapID"] = dataTable.Rows[i]["���"].ToString();
                        //    }
                        //}
                        pFeat["keyID"] = dataTable.Rows[i][strID].ToString();
                        pFeat["Name"] = dataTable.Rows[i][sName].ToString();
                        editTable.InsertFeature(pFeat);
                    }
                }
                catch
                {//�������������,�����˼�¼
                    continue;
                }
            }
        }

        private void mapToolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button == mapToolAddPoint || e.Button == mapToolAddpolygon)
                {
                    this.UncheckedTool();
                }
                else if (e.Button == toolBarNodesEdit)
                {
                    if (e.Button.Pushed)
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.Nodes;
                        mapControl1.Tools.LeftButtonTool = "Select";
                        this.UncheckedTool();
                        toolSel.Checked = true;
                        mapToolBar1.Buttons[4].Enabled = true;
                    }
                    else
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.Objects;
                        mapToolBar1.Buttons[4].Pushed = false;
                        mapToolBar1.Buttons[4].Enabled = false;
                    }
                }
                if (e.Button == toolBarAddNode)
                {
                    if (e.Button.Pushed)
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.AddNode;
                    }
                    else
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.Nodes;
                    }
                }
                if (e.Button == toolBarPointStyle)
                {
                    SymbolStyleDlg pStyleDlg = new SymbolStyleDlg();
                    if (pStyleDlg.ShowDialog() == DialogResult.OK)
                    {
                        featStyle = pStyleDlg.SymbolStyle;
                    }
                }
                if (e.Button == toolBarPolygonStyle)
                {
                    AreaStyleDlg aStyleDlg = new AreaStyleDlg();
                    if (aStyleDlg.ShowDialog() == DialogResult.OK)
                    {
                        aStyle = aStyleDlg.AreaStyle;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar1_ButtonClick");
            }
        }

        private void frmMap_FormClosing(object sender, FormClosingEventArgs e) //�ر�ʱ������ж���δ���棬��ʾ
        {
            try
            {
                if (buttonCancel.Visible && buttonCancel.Enabled)
                {
                    if (MessageBox.Show("�ж���δ���棬�Ƿ���Ҫ�ر�!", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        this.deleteFeature();
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar1_ButtonClick");
            }
        }

        private void mapControl1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DPoint dp;
                switch (e.KeyCode.ToString())
                {
                    case "Up":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y + mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Down":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y - mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Left":
                        dp = new DPoint(mapControl1.Map.Center.x - mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Right":
                        dp = new DPoint(mapControl1.Map.Center.x + mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Oemplus":
                    case "Add":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale / 2f;
                        break;
                    case "OemMinus":
                    case "Subtract":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale * 2f;
                        break;
                    case "Delete":
                        if (buttonCancel.Enabled)
                        {
                            e.Handled = false;
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], false);
                            MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], false);
                        }
                        break;
                    default:
                        System.Console.WriteLine(e.KeyCode.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapControl1_KeyDown");
            }
        }

        private void setTabInsertable(string tableName, bool flag) 
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tableName], flag);
            }
            catch (Exception ex)
            {
                writeToLog(ex,"setTabInsertable");
            }
        }

        /// <summary>
        /// �쳣��־
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFuns">������</param>
        private void writeToLog(Exception ex,string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-frmMap-" + sFuns);
        }

        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            try
            {
                DataGridView dataGridView = (DataGridView)sender;
                if (dataGridView.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView);
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 135;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridView1_Resize");
            }
        }

        //�����¼�ܸ߶ȴ��������߶�,����ֹ�����,�����еĿ��Ҫ�Զ�
        private void setDataGridViewColumnWidth(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.Rows[0].Height * dataGridView.Rows.Count + 40 > dataGridView.Height)
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 150;
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 135;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setDataGridViewColumnWidth");
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            return;
        }


        //�����Ԫ�񣬲��Ҷ�Ӧ��Ҫ�أ��任Ҫ�ص���ʽ��ʵ����˸��
        private Feature flashFt;
        private Style defaultStyle;
        int k = 0;
        int browIndex = 0;  //���ڻ�ȡ��ǰѡ�е��к�
        private void dataGridViewList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //if (flashFt != null && defaultStyle != null)
                //{
                //    flashFt.Style = defaultStyle;
                //}
                //���һ����¼�����е�ͼ��λ
                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                {
                    btnLoc1.Enabled = true;  //׼���༭�������
                }   
                if (e.RowIndex > -1)
                {
                    this.toolLoc.Enabled = true;
                    if (dataGridViewList["X", e.RowIndex].Value == null || dataGridViewList["Y", e.RowIndex].Value == null || dataGridViewList["X", e.RowIndex].Value.ToString() == "" || dataGridViewList["Y", e.RowIndex].Value.ToString() == "")
                    {
                        System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
                        buttonSave.Enabled = false;
                        buttonSave.Text = "����";
                        buttonCancel.Enabled = false;
                        buttonCancel.Text = "ȡ��";

                        //���datagridview1
                        browIndex = e.RowIndex;
                        fillInfoDatagridView(this.dataGridViewList.Rows[e.RowIndex]);
                        return;
                    }
                    double x = Convert.ToDouble(dataGridViewList["X", e.RowIndex].Value);
                    double y = Convert.ToDouble(dataGridViewList["Y", e.RowIndex].Value);
                    if (x == 0 || y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
                        buttonSave.Enabled = false;
                        buttonSave.Text = "����";
                        buttonCancel.Enabled = false;
                        buttonCancel.Text = "ȡ��";

                        //���datagridview1
                        browIndex = e.RowIndex;
                        fillInfoDatagridView(this.dataGridViewList.Rows[e.RowIndex]);
                        return;
                    }

                    // edit by fisher in 09-12-24   ���õ�ͼ��Ұ
                    MapInfo.Geometry.DPoint dP = new MapInfo.Geometry.DPoint(x, y);
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                    MapInfo.Geometry.FeatureGeometry fg = new MapInfo.Geometry.Point(cSys, dP);
                    //SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(fg, IntersectType.Geometry);
                    //si.QueryDefinition.Columns = null;
                    //Feature ftjz = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("������", si);
                    //if (ftjz != null)
                    //{
                    //    //mapControl1.Map.SetView(ftjz);
                    //    mapControl1.Map.SetView(dP, cSys, getScale()); 
                    //    this.mapControl1.Map.Center = dP;
                    //    mapControl1.Refresh();
                    //}
                    //else
                    //{
                    mapControl1.Map.SetView(dP, cSys, getScale());
                    this.mapControl1.Map.Center = dP;
                    mapControl1.Refresh();
                    //} 

                    //������,�ӵ�ͼ�в���Ҫ��,��˸

                    FeatureLayer tempLayer = mapControl1.Map.Layers[editTable.Alias] as MapInfo.Mapping.FeatureLayer;
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                    string sID = dataGridViewList[CLC.ForSDGA.GetFromTable.ObjID, e.RowIndex].Value.ToString();
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tempLayer.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("keyID='" + sID + "'"));
                    if (ft != null)
                    {
                        if (ft["MapID"] != null && ft["MapID"].ToString() != "")
                        {
                            selPrinx = Convert.ToInt32(ft["MapID"]);
                        }
                        selMapID = sID;
                        ////��˸Ҫ��
                        flashFt = ft;
                        defaultStyle = ft.Style;
                        k = 0;
                        timer1.Start();
                    }
                    fillInfoDatagridView(this.dataGridViewList.Rows[e.RowIndex]);
                    buttonSave.Enabled = false;
                    buttonSave.Text = "����";
                    buttonCancel.Enabled = false;
                    buttonCancel.Text = "ȡ��";
                }
                this.dataGridViewList.Invalidate();
                browIndex = e.RowIndex;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
            }
            setDataGridBG();
        }

        private void fillInfoDatagridView(DataGridViewRow dataGridViewRow)
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "��Ƭ")
                        continue;
                    this.dataGridView1.Rows[i].Cells[1].Value = dataGridViewRow.Cells[this.dataGridView1.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                }
            }
            catch(Exception ex) {
                writeToLog(ex, "fillInfoDatagridView");
            }
            if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() != "1")
            {
                this.dataGridView1.Visible = true;
                this.dataGridView1.ReadOnly = true;
                this.btnLoc1.Enabled = false;
                buttonSave.Enabled = false;
            }
            else
            {
                this.dataGridView1.Visible = true;
                this.dataGridView1.Columns[1].ReadOnly=false;
                this.buttonSave.Text = "����";
                //this.buttonSave.Enabled = true;
            }
            // �����ɳ������뼰�����жӴ����ֵ���Զ����ɣ����Բ�����
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                switch (comboTables.Text)
                {
                    case "�������ж�":
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "�����ɳ�������")
                        {
                            dataGridView1.Rows[i].Cells[1].ReadOnly = true;
                        }
                        break;
                    case "����������":
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "�����ɳ�������" || dataGridView1.Rows[i].Cells[0].Value.ToString() == "�����жӴ���")
                        {
                            dataGridView1.Rows[i].Cells[1].ReadOnly = true;
                        }
                        break;
                }
            }
            
        }

        private Color col = Color.Blue;
        private void timer1_Tick(object sender, EventArgs e)
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
                BasePointStyle pStyle = new SimpleVectorPointStyle(70, col, 12);
                flashFt.Style = pStyle;
                flashFt.Update();
                k++;
                if (k == 7)
                {
                    flashFt.Style = defaultStyle;
                    flashFt.Update();
                    timer1.Stop();
                }
            }
            catch
            {
                timer1.Stop();
            }
        }

        #region ���ݵ��뵼��(Creat by Rainny)

        /// <summary>
        /// ���ð�ť
        /// </summary>
        /// <param name="tableName">����</param>
        private void SetButtonStyle(string tableName)
        {
            try
            {
                if (tableName == "�ɳ���Ͻ��" || tableName == "������Ͻ��" || tableName == "���ж�Ͻ��" || tableName == "�ɳ���ÿ�վ�Ա��" || tableName == "�ж�ÿ�վ�Ա��" || tableName == "�������ά��")
                {
                    btnDataIn.Enabled = false;
                    btnDataOut.Enabled = false;

                    buttonSearch.Enabled = false;
                    textKeyWord.Enabled = false;
                    cbMohu.Enabled = false;
                }
                else if (tableName == "��Ƶλ��")
                {
                    btnDataIn.Enabled = false;
                    btnDataOut.Enabled = false;

                    buttonSearch.Enabled = true;
                    textKeyWord.Enabled = true;
                    cbMohu.Enabled = true;
                }
                else
                {
                    btnDataIn.Enabled = true;
                    btnDataOut.Enabled = true;

                    buttonSearch.Enabled = true;
                    textKeyWord.Enabled = true;
                    cbMohu.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetButtonStyle");
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDataIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["�ɵ���"].ToString() != "1")
                {
                    MessageBox.Show("��û�е���Ȩ�ޣ�","��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
                ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                ofd.FileName = this.comboTables.Text;

                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
                {
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;
                    int dataCount = dg.InData(ofd.FileName, this.comboTables.Text);
                    this.Cursor = Cursors.Default;
                    if (dataCount != 0)
                    {
                        if (MessageBox.Show("�ɹ���� " + dataCount.ToString() + " �����ݵ�������£�\n\t���Ƿ�Ҫ�鿴�������棡", "��ʾ", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            string FilePath = Application.StartupPath + @"\Export.txt";

                            if (File.Exists(FilePath))
                            {
                                System.Diagnostics.Process.Start(FilePath);         // Ȼ��ɾ�����ļ�
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("��������ʧ�ܣ������Excel�Ƿ������ݣ�\r \r���߸�Excel�Ƿ�����ʹ��,�Լ����ݸ�ʽ�Ƿ���ȷ", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "btnDataIn_Click");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDataOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["�ɵ���"].ToString() != "1")
                {
                    MessageBox.Show("��û�е���Ȩ�ޣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_exportDT == null)
                {
                    if (MessageBox.Show("����δ��ѯ�κ�����,���������ݿ����иñ�����;\r��ķѽϳ�ʱ��,��������ȷ��,���²�ѯ���ȡ��", "����ȷ��", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Title = "��ѡ�񽫵�����EXCEL�ļ����·��";
                        sfd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                        sfd.FileName = this.comboTables.Text;
                        if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                        {
                            if (sfd.FileName.LastIndexOf(".xls") <= 0)
                            {
                                sfd.FileName = sfd.FileName + ".xls";
                            }
                            string fileName = sfd.FileName;
                            if (System.IO.File.Exists(fileName))
                            {
                                System.IO.File.Delete(fileName);
                            }
                            DataGuide dg = new DataGuide();
                            this.Cursor = Cursors.WaitCursor;
                            if (dg.OutData(fileName, this.comboTables.Text))
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("����Excel���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("����Excelʧ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                        }
                    }
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "��ѡ�񽫵�����EXCEL�ļ����·��";
                    sfd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                    sfd.FileName = this.comboTables.Text;
                    if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                    {
                        if (sfd.FileName.LastIndexOf(".xls") <= 0)
                        {
                            sfd.FileName = sfd.FileName + ".xls";
                        }
                        string fileName = sfd.FileName;
                        if (System.IO.File.Exists(fileName))
                        {
                            System.IO.File.Delete(fileName);
                        }
                        DataGuide dg = new DataGuide();
                        this.Cursor = Cursors.WaitCursor;

                        if (dg.OutData(fileName, _exportDT, this.comboTables.Text))
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("����Excel���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("����Excelʧ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        _exportDT = null;
                    }
                }
            }
            catch(Exception ex) {
                writeToLog(ex, "btnDataOut_Click");
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.SelectedTab == tabPageList)
                {
                    if (this.buttonCancel.Enabled == true && this.buttonCancel.Text == "ȡ�����")
                    {
                        MessageBox.Show("�������һ����¼��������Ӻͱ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabControl1.SelectedTab = tabPageInfo;
                    }
                }
                else if (comboTables.Text == "��Ƶλ��")
                {
                    tabControl1.SelectedTab = tabPageList;
                }
                else
                {
                    if (this.dataGridView1.Visible == false)
                    {
                        this.buttonSave.Enabled = false;
                        this.buttonCancel.Enabled = false;
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "tabControl1_SelectedIndexChanged");
                this.Cursor = Cursors.Default;
            }
        }

        DataSet ds;
        private void checkListAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkListAll.Checked)
            {
                textBox1.Enabled = false;
                checkISMohu.Enabled = false;
                try
                {
                    string sql = "";
                    if (dataGridViewVideo.DataSource == null || dataGridViewVideo.Visible == false || videolocSql == ""||this.dataGridExp.Rows.Count==0)
                    {
                        sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where x is null or x=0 or y is null or y=0";
                    }
                    else
                    {
                        if (videolocSql.IndexOf("where") > -1)
                        {
                            sql = videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                        }
                        else
                        {
                            sql = videolocSql + " where (X is null or Y is null or X = 0 or Y = 0)";
                        }
                    }
                    OracelData linkData = new OracelData(mysqlstr);
                    ds = linkData.SelectDataBase(sql, "temVideo");
                    DataTable dt = ds.Tables[0];
                    dataGridViewVideo.DataSource = dt;
                    dataGridViewVideo.Columns[5].Visible = false;      // �豸��Ų���ʾ
                    dataGridViewVideo.Columns[6].Visible = false;      // mapid����ʾ
                    //dataGridViewVideo.Columns[4].ReadOnly = true;
                    //dataGridViewVideo.Columns[3].ReadOnly = true;
                    //insertQueryIntoTable(dt);
                }
                catch (Exception ex) { writeToLog(ex, "checkListAll_CheckedChanged"); }
            }
            else {
                textBox1.Enabled = true;
                checkISMohu.Enabled = true;
            }
        }

        private void buttonSaveWZ_Click(object sender, EventArgs e)   //����λ��
        {
            if (dataGridViewVideo.Rows.Count < 1) return;
            OracelData linkData = new OracelData(mysqlstr);
            string sql = "";
            try
            {
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    //updated by fisher in 09-09-21
                    if (dataGridViewVideo.Rows[i].Cells["x"].Value == null || dataGridViewVideo.Rows[i].Cells["y"].Value == null || dataGridViewVideo.Rows[i].Cells["x"].Value.ToString() == "" || dataGridViewVideo.Rows[i].Cells["y"].Value.ToString() == "")
                    {
                        continue;
                    }
                    else
                    {
                        if (dataGridViewVideo.Rows[i].Cells["mapid"].Value != null && dataGridViewVideo.Rows[i].Cells["mapid"].Value.ToString() != "")
                        {
                            sql = "update ��Ƶλ�� set x=" + Convert.ToDouble(dataGridViewVideo.Rows[i].Cells["x"].Value) +
                            ",y=" + Convert.ToDouble(dataGridViewVideo.Rows[i].Cells["y"].Value) +
                            ",�豸���� = '"+Convert.ToString(dataGridViewVideo.Rows[i].Cells["�豸����"].Value)+
                            "',�����ɳ���='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["�����ɳ���"].Value) +
                            "',�ճ�������='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["�ճ�������"].Value) +
                            "',mapid='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["mapid"].Value) +
                            "' where �豸���='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["�豸���"].Value) + "'";
                            linkData.UpdateDataBase(sql);                            
                        }
                        continue;
                    }
                }
                insertQueryIntoTable((DataTable)dataGridViewVideo.DataSource);
                MessageBox.Show("����ɹ�!","��ʾ");
                buttonSaveWZ.Enabled = false;
                butLoc2.Enabled = false;
                btnCancel2.Enabled = false;
                this.mapControl1.Tools.LeftButtonTool = "Pan";
            }
            catch(Exception ex) {
                writeToLog(ex, "buttonSaveWZ_Click");
                MessageBox.Show(ex.Message, "buttonSaveWZ_Click()");
            }
        }

        private void dataGridViewVideo_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
            {
                MessageBox.Show("�������Ϊ������!", "��ʾ");
            }
        }

        //���±������ڹ�����Ƶ�༭�ķ�ҳ��ʾ lili 2010-11-11 ���������
        int perPageSize = 100;       // ÿҳ��ʾ����
        int vMax = 0;                // �ܼ�¼��
        int pageVideoCount = 0;      // ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageVideoCurrent = 0;    // ��ǰҳ��
        int vCurrent = 0;            // ��ǰ��¼��

        int _startNo = 0, _endNo = 0; // �洢��ѯ����Ŀ�ʼ����β��
        string videolocSql = ""; //�����г�δ��λ��Ƶ��Ĳ�ѯ��
        string videoSQL = "";
        private void buttonOK_Click(object sender, EventArgs e)       //update by fisher in 09-12-22
        {
            try
            {
                if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() == "0")
                {
                    MessageBox.Show("��û�в�ѯȨ��!", "��ʾ");
                    return;
                }
                isShowPro(true);
                string sql = "", countSql = "";
                if (this.dataGridExp.Rows.Count == 0)
                {
                    if (strRegion == "˳����")
                    {
                        //sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ��";
                        //videolocSql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ��";

                        videoSQL = "";
                        videolocSql = "";
                        countSql = "select count(*) from ��Ƶλ��";
                    }
                    else 
                    {
                        //sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        //videolocSql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";

                        videoSQL = " �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        videolocSql = " �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        countSql = "select count(*) from ��Ƶλ�� where �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else 
                {
                    if (Video_getSqlString() == "")
                    {
                        isShowPro(false);
                        MessageBox.Show("��ѯ����д���,������!");
                        return;
                    }
                    if (strRegion == "˳����")
                    {
                        //sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where " + Video_getSqlString();
                        //videolocSql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where " + Video_getSqlString();

                        videoSQL = " " + Video_getSqlString();
                        videolocSql = " " + Video_getSqlString();
                        countSql = "select count(*) from ��Ƶλ�� where " + Video_getSqlString();
                    }
                    else
                    {
                        //sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where " + Video_getSqlString() + " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        //videolocSql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where " + Video_getSqlString() + " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";

                        videoSQL = " " + Video_getSqlString() + " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        videolocSql = " " + Video_getSqlString() + " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "') ";
                        countSql = "select count(*) from ��Ƶλ�� where " + Video_getSqlString() + " and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                this.getMaxCount(countSql);
                InitDataSet(this.tsCount);
                _startNo = 0;
                _endNo = perPageSize;
                DataTable dt = getLoadData(_startNo, _endNo, videoSQL);
                dataGridViewVideo.DataSource = dt;
                this.toolEditPro.Value = 1;
                Application.DoEvents();
                
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                //OracelData linkData = new OracelData(mysqlstr);
                //ds = linkData.SelectDataBase(sql, "temVideo");
               
                //DataTable dt = ds.Tables[0];
                this.toolEditPro.Value = 2;
                //Application.DoEvents();
                dataGridViewVideo.Columns[5].Visible = false;      // �豸��Ų���ʾ
                dataGridViewVideo.Columns[6].Visible = false;      // mapid����ʾ
                //dataGridViewVideo.Columns[4].ReadOnly = true;   //�豸��Ų������޸�
                //dataGridViewVideo.Columns[3].ReadOnly = true;   //mapid�������޸�
                insertQueryIntoTable(dt);
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch(Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "buttonOK_Click");
            }
        }

        /// <summary>
        /// ��ѯ���  
        /// </summary>
        /// <param name="starNo">��ʼ��¼��</param>
        /// <param name="endNo">������¼��</param>
        /// <param name="sql">��������</param>
        /// <returns>��ѯ���</returns>
        private DataTable getLoadData(int starNo, int endNo, string sql)
        {
            try {
                string completeSQL = "";
                DataTable objset = new DataTable();
                OracelData linkData = new OracelData(mysqlstr);
                if (sql == "")
                {
                    completeSQL = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from (select rownum as rn1,a.* from ��Ƶλ�� a where rownum<=" + endNo + ") t where rn1 >=" + _startNo;
                }
                else 
                {
                    completeSQL = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from (select rownum as rn1,a.* from ��Ƶλ�� a where rownum<=" + endNo + " and " + sql + ") t where rn1 >=" + _startNo;
                }
                objset = linkData.SelectDataBase(completeSQL);
                return objset;

            }
            catch(Exception ex) 
            {
                writeToLog(ex, "getLoadData");
                return null;
            }
        }

        /// <summary>
        /// ��ȡ����������¼����
        /// </summary>
        /// <param name="sql"></param>
        private void getMaxCount(string sql)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();

                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                vMax = Convert.ToInt32(Cmd.ExecuteScalar().ToString());
                Cmd.Dispose();
                Conn.Close();
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                writeToLog(ex, "getMaxCount");
                vMax = 0;
            }
        }

        /// <summary>
        /// ��ʼ����ҳ�ؼ�
        /// </summary>
        /// <param name="tsLabel"></param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                perPageSize = Convert.ToInt32(this.tstbPre.Text);      //����ҳ������                
                tsLabel.Text = vMax.ToString() + "��";//�ڵ���������ʾ�ܼ�¼��
                pageVideoCount = (vMax / perPageSize);//�������ҳ��
                if ((vMax % perPageSize) > 0) pageVideoCount++;
                if (vMax != 0)
                {
                    pageVideoCurrent = 1;
                }
                else { pageVideoCurrent = 0; }
                this.bnCount.Text = "/" + pageVideoCount.ToString();
                this.tstNow.Text = pageVideoCurrent.ToString();
                vCurrent = 0;       //��ǰ��¼����0��ʼ
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InitDataSet");
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                LayerControlDlg lcDlg = new LayerControlDlg();
                lcDlg.Map = mapControl1.Map;
                lcDlg.LayerControl.Tools = mapControl1.Tools;
                lcDlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "toolStripButton1_Click");
            }
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string layName = "";
            if (tabControl2.SelectedTab == tabPage1)
            {
                //feng  09-08-20
                toolLoc.Enabled = false;
                comboTables.Text = "�����ɳ���";
                comboTables_SelectedIndexChanged(null, null);                
            }
            if(tabControl2.SelectedTab == tabPage2)
            {
                Initialvideo();
                V_setfield();  //��ʼ����ѯ�ֶ�
                toolLoc.Enabled = true;
                checkListAll.Checked = false;
                layName = "��Ƶλ��";
                Cursor.Current = Cursors.Default;
                mapControl1.Tools.LeftButtonTool = "Pan";
                this.valueText = "��Ƶλ��";
                //feng
                try  //�ȹر�֮ǰ�ı�ͱ�ע��
                {
                    string sAlias = this.editTable.Alias;
                    if (mapControl1.Map.Layers[sAlias] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    }
                    //�����´�����feng  09-08-20 �޸���ɣ�
                    this.editTable = createTable(layName);
                    FeatureLayer fl = new FeatureLayer(this.editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(this.editTable);//��עͼ��
                    this.featStyle = setFeatStyle(layName);

                    if (this.editTable != null)
                    {
                        if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() == "1")   //������Ƶλ�õ�ͼ���Ƿ���Ա༭
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layName], true);
                            //MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layName + "_tem"], true);
                        }
                        else
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layName], false);
                        }
                        MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layName], true);
                        //MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layName + "_tem"], true);
                        //setPrivilege();
                        mapControl1.Map.Center = mapControl1.Map.Center;
                    }
                    //this.featStyle = setFeatStyle(layName);
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "tabControl2_SelectedIndexChanged");
                    MessageBox.Show(ex.Message, "tabControl2_SelectedIndexChanged");
                }
                finally
                {
                    //add by fisher in 09-12-15
                    if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() != "1")  //û�б༭Ȩ��
                    {
                        this.dataGridViewVideo.ReadOnly = true;
                        this.butLoc2.Enabled = false;
                        this.buttonSaveWZ.Enabled = false;
                        this.btnCancel2.Enabled = false;
                    }
                }
            }
            if (tabControl2.SelectedTab == tabPage3) //����ҵ��༭
            {
                 settabGa();//���ù���ҵ��༭�ĳ�ֵ  feng
                this.dataGridViewValue.Rows.Clear();
                toolLoc.Enabled = false;
                comboTable.Text = "������Ϣ";
                comboTable_SelectedIndexChanged(null, null);                
            }

        }

        //����ҵ��༭��ʼ��
        private void settabGa()
        {
            try
            {
                this.comboTable.SelectedIndex = 0;
                this.comboOrAnd.SelectedIndex = 0;
                this.comboField.SelectedIndex = 0;
                this.comboYunsuanfu.SelectedIndex = 0;
                //��ʼʱ��������Ϣ����Ӧ�ý�isOracleSpatialTab����������Ϊtrue
                isOracleSpatialTab = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "settabGa");
            }
        }

        private void Initialvideo()  //��ʼ����Ƶ��
        {
            try
            {
                this.FieldStr.Text = "�豸���";
                this.ValueStr.Text = "";
                this.dataGridExp.Rows.Clear();
                this.dataGridViewVideo.DataSource = null;
                //this.dataGridViewVideo.Visible = false;
                this.butLoc2.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Initialvideo");
            }
        }

        private void dataGridViewList_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                buttonSave.Enabled = true;
                buttonCancel.Enabled = true;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                if (dataGridView1.Rows.Count > 2)
                {
                    string cellName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string celStr = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string sqlStr = "", telNo = "";
                    DataTable table = null;

                    switch (cellName)
                    {
                        //case "�����ɳ���":
                        //    sqlStr = "select �ɳ������� from �����ɳ��� where �ɳ�����='" + celStr + "'";
                        //    table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        //    telNo = table.Rows[0][0].ToString();
                        //    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        //    {
                        //        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "�����ɳ�������")
                        //        {
                        //            dataGridView1.Rows[i].Cells[1].Value = telNo;
                        //        }
                        //    }
                        //    break;
                        //case "�����ж�":
                        //    sqlStr = "select �жӴ��� from �������ж� where �ж���='" + celStr + "'";
                        //    table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        //    telNo = table.Rows[0][0].ToString();
                        //    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        //    {
                        //        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "�����жӴ���")
                        //        {
                        //            dataGridView1.Rows[i].Cells[1].Value = telNo;
                        //        }
                        //    }
                        //    break;
                        //case "����������":
                        //    sqlStr = "select �����Ҵ��� from ���������� where ��������='" + celStr + "'";
                        //    table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        //    telNo = table.Rows[0][0].ToString();
                        //    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        //    {
                        //        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "���������Ҵ���")
                        //        {
                        //            dataGridView1.Rows[i].Cells[1].Value = telNo;
                        //        }
                        //    }
                        //    break;
                        //default:
                        //    break;

                        case "�����ɳ���":
                            sqlStr = "select �ɳ������� from �����ɳ��� where �ɳ�����='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "�����ɳ�������", dataGridView1);
                            break;
                        case "�����ж�":
                            sqlStr = "select �жӴ��� from �������ж� where �ж���='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "�����жӴ���", dataGridView1);
                            break;
                        case "����������":
                            sqlStr = "select �����Ҵ��� from ���������� where ��������='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "���������Ҵ���", dataGridView1);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridView1_CellValueChanged");
            }
        }

        private void dataGridViewVideo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            buttonSaveWZ.Enabled = true;
            btnCancel2.Enabled = true;
        }

        //���´�����Fisher.feng���  20090814
        private void comboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.lctSQL = "";
            try
            {
                if (this.valueText == this.comboTable.Text)
                {
                    this.mapControl1.Focus();
                    return;
                }
                try  //�ȹر�֮ǰ�ı�ͱ�ע��
                {
                    string sAlias = this.editTable.Alias;
                    if (mapControl1.Map.Layers[sAlias] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    }
                }
                catch(Exception ex)
                {
                    writeToLog(ex, "comboTable_SelectedIndexChanged");
                    MessageBox.Show(ex.Message, "comboTable_SelectedIndexChanged�ر�֮ǰ�ı�");
                }

                this.pageCount = 0;
                this.pageCurrent = 0;
                this.RecordCount.Text = "0��";
                this.PageNow.Text = "0";//���õ�ǰҳ
                this.lblPageCount.Text = "/ {0}";//������ҳ��
                this.toolStripTextBox1.Text = pageSize.ToString();

                dataGridViewGaList.DataSource = null;
                this.dataGridViewGaInfo.Rows.Clear();
                this.dataGridViewGaInfo.Visible = false;
                btnGaSave.Text = "����";
                btnGaCancel.Text = "ȡ��";
                //btnGaSave.Enabled = false;
                this.valueText = this.comboTable.Text;
                this.btnLoc3.Enabled = false;
             
                //ͨ�����ƻ�ȡ����
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text.Trim(), getFromNamePath);
                setFields(CLC.ForSDGA.GetFromTable.TableName);//.���ø���ѯ�ֶ�
                switch (comboTable.Text)
                {
                    case "��������":
                    case "��ȫ������λ":
                    case "����":
                    case "������ҵ":
                    case "����˨":
                    case "�����ص㵥λ":
                    case "�ΰ�����ϵͳ":
                        mapToolBar2.Buttons[0].Visible = true;
                        mapToolBar2.Buttons[0].Enabled = true;
                        mapToolBar2.Buttons[2].Visible = true;
                        mapToolBar2.Buttons[2].Enabled = true;
                        mapToolBar2.Buttons[5].Visible = true;
                        mapToolBar2.Buttons[5].Enabled = true;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPoint")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = false;
                        //this.buttonSearch.Enabled = true;
                        //this.textKeyWord.Enabled = true;
                        //this.cbMohu.Enabled = true;
                        //this.textKeyWord.Focus();
                        featStyle = setFeatStyle(comboTable.Text);
                        this.GaGetTable(this.comboTable.Text);
                        break;
                    
                    case "�˿�ϵͳ":
                    case "�����ݷ���ϵͳ":
                    case "������Ϣ":
                        mapToolBar2.Buttons[0].Visible = true;
                        mapToolBar2.Buttons[0].Enabled = true;
                        mapToolBar2.Buttons[2].Visible = true;
                        mapToolBar2.Buttons[2].Enabled = true;
                        mapToolBar2.Buttons[5].Visible = true;
                        mapToolBar2.Buttons[5].Enabled = true;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPoint")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = true;
                        //this.buttonSearch.Enabled = true;
                        //this.textKeyWord.Enabled = true;
                        //this.cbMohu.Enabled = true;
                        //this.textKeyWord.Focus();
                        featStyle = setFeatStyle(comboTable.Text);
                        this.GaGetTable(this.comboTable.Text);
                        break;
                    }
                    Ga_exportDT = null;
                    
            }
            catch (Exception ex)
            {
                writeToLog(ex, "comboTable_SelectedIndexChanged");
            }
            Cursor.Current = Cursors.Default;
            btnGaSave.Enabled = false;
            btnGaCancel.Enabled = false;
            btnLoc3.Enabled = false;
        }

        //����comboField��ߵ��ֶ�ֵ
        string arrType = "";
        private void setFields(string tableName)
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                //OracelData linkData = new OracelData(strConn);
                conn.Open();
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME= '" + tableName + "'";
                OracleCommand cmd = new OracleCommand(sExp, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                dAdapter.Dispose();
                cmd.Dispose();
                DataTable dt = ds.Tables[0];
                conn.Close();
                //DataTable dt = linkData.SelectDataBase(sExp);


                //CLC.DatabaseRelated.OracleDriver.OracleComRun();
                //CLC.DatabaseRelated.OracleDriver.OracleComScalar();                
                //CLC.DatabaseRelated.OracleDriver.OracleComSelected();

                //CLC.INIClass.IniPathSet;
                //CLC.INIClass.IniReadValue;
                //CLC.INIClass.IniReadValuePW;
                //CLC.INIClass.IniWriteValue;

                comboField.Items.Clear();
                arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string aCol = dt.Rows[i][0].ToString();
                    string atype = dt.Rows[i][1].ToString();

                    if (aCol != "" && aCol != "mapid" && aCol.IndexOf("�����ֶ�") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("����") < 0)
                    {
                        comboField.Items.Add(aCol);
                        arrType += atype + ",";
                    }
                }

                comboField.Text = comboField.Items[0].ToString();
                //setYunsuanfuValue(0);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setFields");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        //������Ƶ��λ�е�FieldStr�ֶ�
        string V_arrType = "";
        private void V_setfield()
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                conn.Open();
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '��Ƶλ��'";
                OracleCommand cmd = new OracleCommand(sExp, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();

                FieldStr.Items.Clear();
                V_arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string vCol = dt.Rows[i][0].ToString();
                    string vType = dt.Rows[i][1].ToString();
                    if (vCol != "" && vCol != "mapID" && vCol != "X" && vCol != "Y")
                    {
                        FieldStr.Items.Add(vCol);
                        V_arrType += vType + ",";
                    }
                }
                FieldStr.Text = "�豸���";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "V_setfield");
            }
            finally {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        //��comboField��ѡ����任ʱ��Ҫ��������������߼���
        private void comboField_SelectedIndexChanged(object sender, EventArgs e)
        {
            setYunsuanfuValue(comboField.SelectedIndex);
        }

        private void setYunsuanfuValue(int p)
        {
            try
            {
                string[] arr = arrType.Split(',');
                string type = arr[p].ToUpper();
                if (type == "DATE")
                {
                    dateTimePicker1.Visible = true;
                    dateTimePicker1.Text = DateTime.Now.ToString();
                    textValue.Visible = false;
                }
                else
                {
                    dateTimePicker1.Visible = false;
                    textValue.Visible = true;
                }
                //  if(type=="VARCHAR2"||type=="NVARCHAR2")
                comboYunsuanfu.Items.Clear();

                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        comboYunsuanfu.Items.Add("����");
                        comboYunsuanfu.Items.Add("������");
                        comboYunsuanfu.Items.Add("����");
                        comboYunsuanfu.Items.Add("���ڵ���");
                        comboYunsuanfu.Items.Add("С��");
                        comboYunsuanfu.Items.Add("С�ڵ���");
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        comboYunsuanfu.Items.Add("����");
                        comboYunsuanfu.Items.Add("������");
                        comboYunsuanfu.Items.Add("����");
                        break;
                }
                comboYunsuanfu.Text = "����";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setYunsuanfuValue");
            }
        }

        private void V_setYSF(int V_index)  //��Ƶ��λ���������
        {
            try
            {
                string[] arr = V_arrType.Split(',');
                string type = arr[V_index].ToUpper();
                MathStr.Items.Clear();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        MathStr.Items.Add("����");
                        MathStr.Items.Add("������");
                        MathStr.Items.Add("����");
                        MathStr.Items.Add("���ڵ���");
                        MathStr.Items.Add("С��");
                        MathStr.Items.Add("С�ڵ���");
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        MathStr.Items.Add("����");
                        MathStr.Items.Add("������");
                        MathStr.Items.Add("����");
                        break; 
                }
                MathStr.Text = "����";
                connStr.Text = connStr.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "V_setYSF");
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboTable.Text.Trim() == "")
                {
                    MessageBox.Show("��ѡ���");
                    return;
                }

                if (textValue.Visible && textValue.Text.Trim() == "")
                {
                    MessageBox.Show("��ѯֵ����Ϊ�գ�");
                    return;
                }

                if (this.textValue.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("������ַ����в��ܰ���������!");
                    return;
                }

                string strExp = "";
                int p = comboField.SelectedIndex;
                string[] arr = arrType.Split(',');
                string type = arr[p].ToUpper();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   " + textValue.Text.Trim();
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   " + textValue.Text.Trim();
                        }
                        this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                        break;
                    case "DATE":
                        string tValue = this.dateTimePicker1.Value.ToString();
                        if (tValue == "")
                        {
                            MessageBox.Show("��ѯֵ����Ϊ�գ�");
                            return;
                        }

                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "ʱ��" });
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "ʱ��" });
                        }
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "����")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "�ַ���" });
                            }
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "����")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "����" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "�ַ���" });
                            }
                        }
                        break;
                }
                this.comboTable.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonAdd_Click");
            }
        }

        // �Ƴ�һ�����ʽ
        private void buttonRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if(this.dataGridViewValue.Rows.Count!=0)
                {
                    if (this.dataGridViewValue.CurrentRow.Index != 0)
                    {
                        this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                    }
                    else
                    {
                        if (this.dataGridViewValue.Rows.Count > 1)
                        {
                            this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                            string text = this.dataGridViewValue.Rows[0].Cells["Value"].Value.ToString().Replace("����", "");

                            text = text.Replace("����", "").Trim();
                            this.dataGridViewValue.Rows[0].Cells["Value"].Value = text;
                        }
                        else
                        {
                            this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                            this.comboTable.Enabled = true; 
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
                writeToLog(ex, "buttonRemove_Click");
            }
        }

        //����������ʽ
        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.lctSQL = "";
            _mySQL = "";
            _first = Convert.ToInt32(this.toolStripTextBox1.Text);
            _end = 1;
            try
            {
                this.dataGridViewGaList.Columns.Clear();
                //this.dataGridViewGaInfo.Rows.Clear();
                this.dataGridViewGaInfo.Visible = false;
                this.textValue.Text = "";
                this.comboTable.Enabled = true;
                dataGridViewValue.Rows.Clear();
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;
                this.toolLoc.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonClear_Click");
            }
        }

        //��dataGridViewValue��ߵ���Ŀת��Ϊ�ַ���
        private string getSqlString()//ת���ַ���
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";

                for (int i = 0; i < this.dataGridViewValue.Rows.Count; i++)
                {
                    string type = this.dataGridViewValue.Rows[i].Cells["Type"].Value.ToString();
                    string str = this.dataGridViewValue.Rows[i].Cells["Value"].Value.ToString();
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
                        getsql += "   " + array[j].ToString();
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
                writeToLog(ex, "getSqlString");
                return "";
            }
        }

        //��dataGridExp��ߵ���Ŀת��Ϊ�ַ���
        private string Video_getSqlString()//ת���ַ���
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";
                for (int i = 0; i < this.dataGridExp.Rows.Count; i++)
                {
                    string type = this.dataGridExp.Rows[i].Cells["video_Type"].Value.ToString();
                    string str = this.dataGridExp.Rows[i].Cells["video_Value"].Value.ToString();
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
                        getsql += "   " + array[j].ToString();
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
                writeToLog(ex, "Video_getSqlString");
                return "";
            }
        }

        //string GJsql = "";
        //���±������ڹ���ҵ��༭�ķ�ҳ��ʾ
        int pageSize = 100;     //ÿҳ��ʾ����
        int nMax = 0;         //�ܼ�¼��
        int pageCount = 0;    //ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageCurrent = 0;   //��ǰҳ��
        int nCurrent = 0;      //��ǰ��¼��
        //DataSet Gads = new DataSet();

        //----------- ���±������ڷ�ҳ add by lili 2010-8-26 ----------------         
        int _first = 0;         // ��������
        int _end = 1;           // ��ʼ����
        string tabName = "";    // ����
        string _mySQL = "", _PageSQL = "", _lctSQL = "";  //�û���ѯȨ��
        //-------------------------------------------------------------------

        string PageSQL = "";   //���ڻ�÷�ҳ����
        string lctSQL = "";    //�����Ѷ�λ��δ��λ��SQL���Ĵ��ݣ��深����fisher��
        string mySQL = "";

        private void buttonMultiOk_Click(object sender, EventArgs e)
        {
            try
            {
                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;
                dataGridViewGaList.Columns.Clear();
                //this.removeTemPoints();

                //ͨ�����ƻ�ȡ������������
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                tabName = CLC.ForSDGA.GetFromTable.TableName;
                string sql = "";
                _mySQL = "";
                _first = Convert.ToInt32(this.toolStripTextBox1.Text);
                _end = 1;
                // ���û���������Ͳ�ѯȫ�����
                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    mySQL = "select count(*) from " + tabName;
                }
                else
                {
                    if (isOracleSpatialTab)
                    {
                        mySQL = "select count(*) from " + tabName + " t" + " where " + this.getSqlString();
                    }
                    else
                    {
                        mySQL = "select count(*) from " + tabName + " where " + this.getSqlString();
                    }
                }

                if (strRegion != "˳����" && strRegion != "")   //edit by fisher in 09-12-01
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }

                    if (this.dataGridViewValue.Rows.Count == 0)
                    {
                        _mySQL += " " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                    }
                    else
                    {
                        _mySQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                    }

                }
                if (strRegion1 != "" && (tabName == "������Ϣ" || tabName == "��������"))
                {
                    if (this.dataGridViewValue.Rows.Count == 0)
                    {
                        _mySQL += " (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        if (_mySQL.IndexOf("and") > -1)
                        {
                            _mySQL = _mySQL.Remove(_mySQL.LastIndexOf(")"));
                            _mySQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                        }
                        else
                        {
                            _mySQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                        }
                    }
                }
                OracleConnection conn = new OracleConnection(mysqlstr);
                try
                {
                    // ���û���������Ͳ�ѯȫ�����
                    if (this.dataGridViewValue.Rows.Count == 0)
                    {
                        mySQL = mySQL.IndexOf("where") > -1 ? mySQL : _mySQL == string.Empty ? mySQL : mySQL + " where " + _mySQL;
                    }
                    else
                    {
                        mySQL += _mySQL;
                    }
                    conn.Open();
                    OracleCommand cmd = new OracleCommand(mySQL, conn);
                    OracleDataAdapter orada = new OracleDataAdapter(cmd);
                    DataTable _ds = new DataTable();
                    orada.Fill(_ds);
                    cmd.Dispose();
                    orada.Dispose();
                    conn.Close();
                    if (Convert.ToInt32(_ds.Rows[0][0]) < 1)
                    {
                        isShowPro(false);
                        MessageBox.Show("���������޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Cursor = Cursors.Default;
                        this.RecordCount.Text = "0��";
                        this.PageNow.Text = "0";//���õ�ǰҳ
                        this.lblPageCount.Text = "/ {0}";//������ҳ��
                        this.toolStripTextBox1.Text = pageSize.ToString();
                        return;
                    }
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();
                    //���´����������÷�ҳ
                    try
                    {
                        nMax = Convert.ToInt32(_ds.Rows[0][0]);
                        this.RecordCount.Text = nMax.ToString() + "��";
                        pageSize = Convert.ToInt32(this.toolStripTextBox1.Text);      //����ҳ������
                        pageCount = (nMax / pageSize);//�������ҳ��
                        if ((nMax % pageSize) > 0) pageCount++;
                        this.lblPageCount.Text = "/" + pageCount.ToString();//������ҳ��
                        this.toolStripTextBox1.Text = pageSize.ToString();
                        if (nMax != 0)
                        {
                            pageCurrent = 1;
                            this.PageNow.Text = pageCurrent.ToString();//���õ�ǰҳ
                        }
                        nCurrent = 0;       //��ǰ��¼����0��ʼ
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "buttonMultiOk_Click ��ȡ��ҳ����");
                        MessageBox.Show(ex.Message, "buttonMultiOk_Click ��ȡ��ҳ����");
                    }

                    //DataTable datatable = Ga_exportDT = LoadData(PageSQL); //��ȡ��ǰҳ����
                    if (this.dataGridViewValue.Rows.Count == 0)          // �ж��Ƿ��û���������
                        _mySQL = _mySQL == string.Empty ? "" : " and " + _mySQL;

                    DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //��ȡ��ǰҳ����
                    dataGridViewGaList.DataSource = datatable;
                    this.toolEditPro.Value = 2;
                    //Application.DoEvents();
                    if (dataGridViewGaList.Columns["mapid"] != null)
                    {
                        dataGridViewGaList.Columns["mapid"].Visible = false;
                    }
                    //dataGridViewList.Visible = true;
                    //����gridview�ļ����ɫ
                    GasetDataGridBG();

                    this.insertGaQueryIntoTable(datatable);
                    this.dataGridViewGaInfo.Visible = false;

                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//���datagridview
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                    }

                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;

                    //if (FrmLogin.string�༭Ȩ�� == "true")
                    //{
                    //    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);
                    //}
                    //MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);

                    this.toolEditPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "buttonMultiOk_Click����");
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "buttonMultiOk_Click");
            }            
        }

        /// <summary>
        /// ��ѯ��ҳ���� lili 2010-8-26
        /// </summary>
        /// <param name="wheresql">Ȩ������</param>
        /// <param name="_first">��ʼ����</param>
        /// <param name="_end">��������</param>
        /// <param name="tabName">����</param>
        /// <returns>�����</returns>
        public DataTable LoadData(string wheresql, int _first, int _end,string tabName)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                string sql = "";
                string getSqlStr = getSqlString() == "" ? "" : "and " + getSqlString();    // �ж��Ƿ�Ϊ��
                string _wheresql = wheresql == "" ? "" : wheresql;                         // �ж��Ƿ�Ϊ��
                if (isOracleSpatialTab)
                {
                    sql = "select MI_PRINX as mapid," + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from " + tabName + " a where rownum<=" + _first + " " + getSqlStr + _wheresql + ") t where rn1 >=" + _end;
                }
                else
                {
                    if (tabName == "��ȫ������λ")
                        sql = "select rownum as mapid,���,��λ����,��λ����,��λ��ַ,���ܱ�������������,�ֻ�����,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,'����༭' as ��ȫ������λ�ļ�,��ȡID,��ȡ����ʱ��,��ע��,��עʱ��,��������,X,Y,�����ֶ�һ,�����ֶζ�,�����ֶ��� from (select rownum as rn1,a.* from " + tabName + " a where rownum<=" + _first + getSqlStr + _wheresql + ") t where rn1 >=" + _end;
                    else
                        sql = "select rownum as mapid," + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from " + tabName + " a where rownum<=" + _first + getSqlStr + _wheresql + ") t where rn1 >=" + _end;
                }

                int nStartPos = 0;   //��ǰҳ�濪ʼ��¼��
                nStartPos = nCurrent;

                DataTable dtInfo;

                dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(dtInfo);
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                return dtInfo;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LoadData()");
                MessageBox.Show(ex.Message, "����LoadData()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
        }

        public DataTable LoadData(string sql)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                int nStartPos = 0;   //��ǰҳ�濪ʼ��¼��
                nStartPos = nCurrent;

                DataTable dtInfo;

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
                Adp.Fill(nStartPos, pageSize, dataTables);//����ط���֪���Ǵ����ݿ��в鵽ǰ100�з��أ��������е����ݾݶ���ѯ�����أ��ٴ��л�ȡǰ100�С�

                dtInfo = dataTables[0];
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                return dtInfo;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LoadData()");
                MessageBox.Show(ex.Message, "����LoadData()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
        }

        public DataTable BJLoadData(string sql)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                int nStartPos = 0;   //��ǰҳ�濪ʼ��¼��
                nStartPos = bnCurrent;

                DataTable bjdtInfo;

                bjdtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                DataTable[] dataTables = new DataTable[1];
                for (int i = 0; i < dataTables.Length; i++)
                {
                    dataTables[i] = new DataTable();
                }
                Adp.Fill(nStartPos, bpageSize, dataTables);//����ط���֪���Ǵ����ݿ��в鵽ǰ100�з��أ��������е����ݾݶ���ѯ�����أ��ٴ��л�ȡǰ100�С�
                //Adp.Fill(0, 2, dataTables);

                bjdtInfo = dataTables[0];
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                return bjdtInfo;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "BJLoadData()");
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
        }

        private void insertGaQueryIntoTable(DataTable dataTable)//����ѯ���Ľ����ӵ�����
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("û����ض����������ѯ�ؼ��ʣ�");
                    return;
                }

                (this.editTable as IFeatureCollection).Clear();
                this.editTable.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "insertGaQueryIntoTable");
                MessageBox.Show(ex.Message, "insertGaQueryIntoTable111");
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                double dX = 0, dY = 0;
                try
                {
                    dX = Convert.ToDouble(dataTable.Rows[i]["X"]);
                    dY = Convert.ToDouble(dataTable.Rows[i]["Y"]);

                    if (dX > 0 && dY > 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(this.editTable)).CoordSys, dX, dY);
                        string tabName = comboTable.Text;
                        if (tabControl2.SelectedTab == tabPage2)
                        {
                            tabName = "��Ƶ";
                        }
                        CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                        string sName = CLC.ForSDGA.GetFromTable.ObjName;
                        string strID = CLC.ForSDGA.GetFromTable.ObjID;

                        Feature pFeat = new Feature(editTable.TableInfo.Columns);

                        pFeat.Geometry = pt;
                        pFeat.Style = setFeatStyle(comboTable.Text.Trim());
                        if (dataTable.Columns.Contains("mapid"))
                        {
                            pFeat["mapid"] = dataTable.Rows[i]["mapid"].ToString();
                        }
                        //else {
                        //    if (tabName == "�ΰ�����ϵͳ")
                        //    {
                        //        pFeat["MapID"] = dataTable.Rows[i]["���"].ToString();
                        //    }
                        //}
                        pFeat["keyID"] = dataTable.Rows[i][strID].ToString();
                        pFeat["Name"] = dataTable.Rows[i][sName].ToString();
                        editTable.InsertFeature(pFeat);
                    }
                }
                catch(Exception ex)
                {//�������������,�����˼�¼
                    writeToLog(ex, "insertGaQueryIntoTable");
                    continue;
                }
            }
        }


        /// <summary>
        /// ����ҵ��༭��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttDataOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["�ɵ���"].ToString() != "1")
                {
                    MessageBox.Show("��û�е���Ȩ�ޣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Ga_exportDT == null)
                {
                    if (MessageBox.Show("����δ��ѯ�κ�����,���������ݿ����иñ�����;\r��ķѽϳ�ʱ��,��������ȷ��,���²�ѯ���ȡ��", "����ȷ��", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Title = "��ѡ�񽫵�����EXCEL�ļ����·��";
                        sfd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                        sfd.FileName = this.comboTable.Text;
                        if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                        {
                            if (sfd.FileName.LastIndexOf(".xls") <= 0)
                            {
                                sfd.FileName = sfd.FileName + ".xls";
                            }
                            string fileName = sfd.FileName;
                            if (System.IO.File.Exists(fileName))
                            {
                                System.IO.File.Delete(fileName);
                            }
                            DataGuide dg = new DataGuide();
                            this.Cursor = Cursors.WaitCursor;
                            if (dg.OutData(fileName, this.comboTable.Text.Trim()))
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("����Excel���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("����Excelʧ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "��ѡ�񽫵�����EXCEL�ļ����·��";
                    sfd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                    sfd.FileName = this.comboTable.Text;
                    if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                    {
                        if (sfd.FileName.LastIndexOf(".xls") <= 0)
                        {
                            sfd.FileName = sfd.FileName + ".xls";
                        }
                        string fileName = sfd.FileName;
                        if (System.IO.File.Exists(fileName))
                        {
                            System.IO.File.Delete(fileName);
                        }
                        DataGuide dg = new DataGuide();
                        this.Cursor = Cursors.WaitCursor;
                        if (dg.OutData(fileName, Ga_exportDT,this.comboTable.Text))
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("����Excel���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("����Excelʧ��!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        Ga_exportDT = null;
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "buttDataOut_Click");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ����ҵ��༭��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttDataIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["�ɵ���"].ToString() != "1")
                {
                    MessageBox.Show("��û�е���Ȩ�ޣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "��ѡ�񽫵����EXCEL�ļ�·��";
                ofd.Filter = "Excel�ĵ�(*.xls)|*.xls";
                ofd.FileName = this.comboTable.Text;

                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
                {
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;
                    int dataCount = dg.InData(ofd.FileName, this.comboTable.Text);
                    this.Cursor = Cursors.Default;
                    if (dataCount != 0)
                    {
                        if (MessageBox.Show("�ɹ���� " + dataCount.ToString() + " �����ݵ�������£�\n���Ƿ�Ҫ�鿴�������棡", "��ʾ", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            string FilePath = Application.StartupPath + @"\Export.txt";

                            if (File.Exists(FilePath))
                            {
                                System.Diagnostics.Process.Start(FilePath);             // Ȼ��ɾ�����ļ�
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("��������ʧ�ܣ������Excel�Ƿ������ݣ�\r \r���߸�Excel�Ƿ�����ʹ��,�Լ����ݸ�ʽ�Ƿ���ȷ", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttDataIn_Click");
                this.Cursor = Cursors.Default;
            }
        }

        //����ҵ��༭�У������Ԫ�񣬲��Ҷ�Ӧ��Ҫ�أ��任Ҫ�ص���ʽ��ʵ����˸��
        //private Feature flashFt1;
        //private Style defaultStyle1;
        //int k1 = 0;

        FrmZLMessage frmZL;
        int rowIndex = 0;   //���������ȡ��ǰ���������������dataGridViewGaInfo��ֵ  fisher��09-09-02��
        private void dataGridViewGaList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //���һ����¼�����е�ͼ��λ
                if (e.RowIndex > -1)
                {
                    //������࣬��ʾ��ϸ��Ϣ�������ͼ��λ
                    if (e.ColumnIndex == dataGridViewGaList.Columns.Count - 11 && comboTable.Text.Trim() == "��ȫ������λ")
                    {
                        if (dataGridViewGaList.Rows[e.RowIndex].Cells[dataGridViewGaList.Columns.Count - 11].Value.ToString() != "����༭") return;

                        if (this.frmZL != null)
                        {
                            if (this.frmZL.Visible == true)
                            {
                                this.frmZL.Close();
                            }
                        }

                        if (dataGridViewGaList.Rows[dataGridViewGaList.CurrentRow.Index].Cells["��λ����"].Value.ToString() == "")
                        {
                            MessageBox.Show("��λ���Ʋ���Ϊ�գ�", "��ʾ");
                            return;
                        }
                        this.frmZL = new FrmZLMessage(dataGridViewGaList.Rows[dataGridViewGaList.CurrentRow.Index].Cells["��λ����"].Value.ToString(), mysqlstr,this.temEditDt);

                        //this.frmZL.SetDesktopLocation(Control.MousePosition.X + 50, Control.MousePosition.Y - this.frmZL.Height / 2);
                        //������Ϣ�������½�
                        System.Drawing.Point p = this.PointToScreen(mapControl1.Parent.Location);
                        this.frmZL.SetDesktopLocation(mapControl1.Width - frmZL.Width + p.X, mapControl1.Height - frmZL.Height + p.Y + 25);
                        this.frmZL.Show();
                    }

                    if (dataGridViewGaList["X", e.RowIndex].Value == null || dataGridViewGaList["Y", e.RowIndex].Value == null || dataGridViewGaList["X", e.RowIndex].Value.ToString() == "" || dataGridViewGaList["Y", e.RowIndex].Value.ToString() == "")
                    {
                        System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
                        btnGaSave.Enabled = false;
                        btnGaSave.Text = "����";
                        btnGaCancel.Enabled = false;
                        btnGaCancel.Text = "ȡ��";
                        if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() == "1")
                        {
                            btnLoc3.Enabled = true;  //׼���༭�������
                        }                       

                        //�����ϢDatagridView
                        rowIndex = e.RowIndex;
                        GafillInfoDatagridView(this.dataGridViewGaList.Rows[e.RowIndex]);  
                      
                        //add by fisher on 09-09-24
                        //���´��������û�¼�������ǣ����û�и�MapInfo�ı����XY��Ϣ�����丳ֵΪ0�����������ݿ⣬Ϊ�������ݸ��´��»���
                        OracleConnection conn = new OracleConnection(mysqlstr);
                        try
                        {
                            if (isOracleSpatialTab)
                            {
                                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                                string GAobjId = CLC.ForSDGA.GetFromTable.ObjID;
                                string KeyID = this.dataGridViewGaList[GAobjId, e.RowIndex].Value.ToString();                               
                                conn.Open();
                                string updtstr="update " +comboTable.Text + " t set t.GEOLOC = MDSYS.SDO_GEOMETRY(2001, 8307, MDSYS.SDO_POINT_TYPE('0','0', NULL),NULL, NULL) where " + GAobjId + "= '" + KeyID + "'";
                                OracleCommand cmd= new OracleCommand(updtstr, conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                conn.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "dataGridViewGaList_CellClick(�������ݿ�)");
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                        return;
                    }

                    //toolLoc.Enabled = true;  //׼���༭�������
                    btnLoc3.Enabled = true;  //׼���༭�������

                    double x = Convert.ToDouble(dataGridViewGaList["X", e.RowIndex].Value);
                    double y = Convert.ToDouble(dataGridViewGaList["Y", e.RowIndex].Value);
                    if (x == 0 || y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
                        btnGaSave.Enabled = false;
                        btnGaSave.Text = "����";
                        btnGaCancel.Enabled = false;
                        btnGaCancel.Text = "ȡ��";

                        if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() == "1")
                        {
                            btnLoc3.Enabled = true;  //׼���༭�������
                        }   

                        //�����ϢDatagridView
                        rowIndex = e.RowIndex;
                        GafillInfoDatagridView(this.dataGridViewGaList.Rows[e.RowIndex]);
                        return;
                    }

                    // ���´�����������ǰ��ͼ����Ұ�������ö������ڵ��ɳ���
                    //add by fisher in 09-12-24
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

                    //������,�ӵ�ͼ�в���Ҫ��,��˸

                    FeatureLayer tempLayer = mapControl1.Map.Layers[editTable.Alias] as MapInfo.Mapping.FeatureLayer;
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    string sID = dataGridViewGaList[CLC.ForSDGA.GetFromTable.ObjID, e.RowIndex].Value.ToString();
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tempLayer.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("keyID='" + sID + "'"));
                    if (ft != null)
                    {
                        if (ft["mapid"] != null && ft["mapid"].ToString() != "")
                        {
                            selPrinx = Convert.ToInt32(ft["mapid"]);
                        }
                        selMapID = sID;  //??????????????????????
                        ////��˸Ҫ��
                        flashFt = ft;
                        defaultStyle = ft.Style;
                        k = 0;
                        timer1.Start();
                    }
                    //�����ϢDatagridView
                    rowIndex = e.RowIndex;
                    GafillInfoDatagridView(this.dataGridViewGaList.Rows[e.RowIndex]);
                    btnGaSave.Enabled = false;
                    btnGaSave.Text = "����";
                    btnGaCancel.Enabled = false;
                    btnGaCancel.Text = "ȡ��";
                }
                //if (e.RowIndex >= 0)
                //{
                //    this.dataGridViewGaList.ContextMenuStrip = this.contextMenuStrip1;
                //}
                //else
                //{
                //    this.dataGridViewGaList.ContextMenuStrip = null;
                //}
            }
            catch(Exception ex)
            {
                writeToLog(ex, "dataGridViewGaList_CellClick");
                System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
            }
            //����gridview�ļ����ɫ
            for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
            {
                if (i % 2 == 1)
                {
                    dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                }
                else
                {
                    dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                }
            }
        }

        private void GafillInfoDatagridView(DataGridViewRow dataGridViewRow)
        {
            try
            {
                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count ; i++)
                {
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "��Ƭ")
                        continue;
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "��������")
                        this.dataGridViewGaInfo.Rows[i].Cells[1].ReadOnly = true;
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = dataGridViewRow.Cells[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GafillInfoDatagridView");
                MessageBox.Show(ex.Message, "GafillInfoDatagridView");
            }
            if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() != "1")  //���ɱ༭
            {
                this.dataGridViewGaInfo.Visible = true;
                this.dataGridViewGaInfo.ReadOnly = true;
                btnGaSave.Enabled = false;
                btnLoc3.Enabled = false;
            }
            else
            {
                string colName = "";
                this.dataGridViewGaInfo.Visible = true;
                this.dataGridViewGaInfo.Columns[1].ReadOnly = false;
                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                {
                    colName = this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                    if (colName == "��������" || colName == "�����ɳ�������" || colName == "�����жӴ���" || colName=="���������Ҵ���")
                        this.dataGridViewGaInfo.Rows[i].Cells[1].ReadOnly = true;
                }
                this.btnGaSave.Text = "����";
                this.btnGaCancel.Text = "ȡ��";
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;
                //this.buttonSave.Enabled = true;
            }
        }

        private void GaGetTable(string tableName)//�����ݿ��еõ���
        {
            if (isOracleSpatialTab)
            {
                MIConnection miConnection = new MIConnection();
                try
                {
                    if (this.lTable != null)
                    {
                        this.lTable.Close();
                    }
                    miConnection.Open();

                    //֮ǰ�пռ��򲻿�, �������˶�δ�.
                    try
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select  *  From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;

                        if (miConnection.Catalog.GetTable(tableName) == null)
                            this.lTable = miConnection.Catalog.OpenTable(ti);
                        else
                            this.lTable = miConnection.Catalog.GetTable(tableName);
                      
                    }
                    catch
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select * From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;
                        if (miConnection.Catalog.GetTable(tableName) == null)
                            this.lTable = miConnection.Catalog.OpenTable(ti);
                        else
                            this.lTable = miConnection.Catalog.GetTable(tableName);
                    }

                    if (MapInfo.Engine.Session.Current.Catalog.GetTable(tableName + "_tem") == null)
                    {
                        MapInfo.Data.TableInfoNative ListTableInfo = new MapInfo.Data.TableInfoNative(tableName + "_tem");
                        ListTableInfo.Temporary = true;
                        MapInfo.Geometry.CoordSys LCoordsys;
                        MapInfo.Data.GeometryColumn GC = (MapInfo.Data.GeometryColumn)(this.lTable.TableInfo.Columns["obj"]);
                        LCoordsys = GC.CoordSys;
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateFeatureGeometryColumn(LCoordsys));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateIntColumn("mapid"));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("keyID", 100));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("Name", 200));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStyleColumn());
                        // ListTableInfo.WriteTabFile();
                        this.editTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ListTableInfo);
                    }
                    else
                        this.editTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableName + "_tem");
                    
                    miConnection.Close();

                    //��ͼ��ʾ
                    FeatureLayer fl = new FeatureLayer(editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(editTable);//��עͼ��

                    if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() == "1")
                    {
                        MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    }
                    MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    mapControl1.Map.Center = mapControl1.Map.Center;
                }
                catch (Exception ex)
                {
                    if (miConnection.State == ConnectionState.Open)
                    {
                        miConnection.Close();
                    }
                    MessageBox.Show(ex.Message, "GaGetTable()");
                    writeToLog(ex, "GaGetTable");
                }
            }
            else
            {
                try
                {
                    if (this.editTable != null)
                    {
                        this.editTable.Close();
                    }
                    this.editTable = createTable(tableName);
                    FeatureLayer fl = new FeatureLayer(this.editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(this.editTable);//��עͼ��

                    if (this.editTable != null)
                    {
                        if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() == "1")
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);
                        }
                        MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTable.Text], true);
                        //setPrivilege();
                        mapControl1.Map.Center = mapControl1.Map.Center;
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "GaGetTable");
                    MessageBox.Show(ex.Message, "GaGetTable111");
                }
            }

            this.GaInitialEditFields(this.comboTable.Text);
        }

        private void GaInitialEditFields(string tabName)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();
                OracleCommand cmd = new OracleCommand("SELECT COLUMN_NAME, NULLABLE,DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME= '" + tabName + "'", Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                cmd.Dispose();
                Conn.Close();

                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                string GaobjID = CLC.ForSDGA.GetFromTable.ObjID;   //  by fisher on 09-09-23
                dataGridViewGaInfo.Rows.Clear();
                int k = 0;
                int j = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string fieldName = dt.Rows[i][0].ToString();
                    //edited by fisher in 09-09-01
                    //if (fieldName.ToUpper() == "MAPID" || fieldName.ToUpper() == "X" || fieldName.ToUpper() == "Y" || fieldName.IndexOf("�����ֶ�") > -1 || fieldName.ToUpper() == "MI_STYLE" || fieldName.ToUpper() == "MI_PRINX" || fieldName.ToUpper() == "GEOLOC")
                    if (fieldName.ToUpper() == "mapid" || fieldName.IndexOf("�����ֶ�") > -1 || fieldName.ToUpper() == "MI_STYLE" || fieldName.ToUpper() == "MI_PRINX" || fieldName.ToUpper() == "GEOLOC" || fieldName.ToUpper() == "X" || fieldName.ToUpper() == "Y")
                    {
                        k++;
                        continue;
                    }
                    dataGridViewGaInfo.Rows.Add(1);//���һ��

                    dataGridViewGaInfo.Rows[i - k].Cells[0].Value = fieldName;
                    if (fieldName == CLC.ForSDGA.GetFromTable.XiaQuField)
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxJieZhen();
                    }
                    else if (fieldName == "�����ж�")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxZhongdu();
                    }
                    else if (fieldName == "����������")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxJinWuShi();
                    }
                    else if (fieldName == "�ص��˿�")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxShiFou();
                    }
                    else
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1].Value = "";
                    }

                    if (dt.Rows[i][1].ToString().ToUpper() == "N" || fieldName == GaobjID || fieldName == "��ע��")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[2].Value = "����";
                    }

                    dataGridViewGaInfo.Rows[i - k].Cells[3].Value = dt.Rows[i][2].ToString().ToUpper();
                    j++;
                }
                //������fisher �޸ģ�09-09-01��
                if (comboTable.Text == "�˿�ϵͳ")
                {
                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j].Cells[0].Value = "��Ƭ";
                    dataGridViewGaInfo.Rows[j].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[3].Value = "VARCHAR2";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 1].Cells[0].Value = "X";
                    dataGridViewGaInfo.Rows[j + 1].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[3].Value = "FLOAT";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 2].Cells[0].Value = "Y";
                    dataGridViewGaInfo.Rows[j + 2].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 2].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 2].Cells[3].Value = "FLOAT";
                }
                else if (comboTable.Text == "��ȫ������λ")
                {
                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j].Cells[0].Value = "X";
                    dataGridViewGaInfo.Rows[j].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[3].Value = "FLOAT";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 1].Cells[0].Value = "Y";
                    dataGridViewGaInfo.Rows[j + 1].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[3].Value = "FLOAT";
                    dataGridViewGaInfo.Rows.Add(1);

                    dataGridViewGaInfo.Rows[j + 2].Cells[0].Value = "��ȫ������λ�ļ�";
                    DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                    dgvlc.Value = "����༭";
                    dgvlc.ToolTipText = "����༭��ȫ������λ�ļ�";
                    dataGridViewGaInfo.Rows[j + 2].Cells[1] = dgvlc;
                    dataGridViewGaInfo.Rows[j + 2].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 2].Cells[3].Value = "VARCHAR2";

                    
                }
                else
                {
                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j].Cells[0].Value = "X";
                    dataGridViewGaInfo.Rows[j].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[3].Value = "FLOAT";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 1].Cells[0].Value = "Y";
                    dataGridViewGaInfo.Rows[j + 1].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[3].Value = "FLOAT";
                }
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                writeToLog(ex, "GaInitialEditFields");
                MessageBox.Show(ex.Message, "GaInitialEditFields");
            }

            setDataGridViewColumnWidth(dataGridViewGaInfo);
        }

        private void dataGridViewGaList_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //if (dataGridViewGaInfo.Rows.Count > 2)
            //{
            //    string cellName = dataGridViewGaInfo.Rows[e.RowIndex].Cells[0].Value.ToString();
            //    string celStr = dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value.ToString();
            //    string sqlStr = "", telNo = "";

            //    switch (cellName)
            //    {
            //        case "�����ɳ���":
            //            sqlStr = "select �ɳ������� from �����ɳ��� where �ɳ�����='" + celStr + "'";
            //            setSystemValue(sqlStr, telNo, "�����ɳ�������", dataGridViewGaInfo);
            //            break;
            //        case "�����ж�":
            //            sqlStr = "select �жӴ��� from �������ж� where �ж���='" + celStr + "'";
            //            setSystemValue(sqlStr, telNo, "�����жӴ���", dataGridViewGaInfo);
            //            break;
            //        case "����������":
            //            sqlStr = "select �����Ҵ��� from ���������� where ��������='" + celStr + "'";
            //            setSystemValue(sqlStr, telNo, "���������Ҵ���", dataGridViewGaInfo);
            //            break;
            //        default:
            //            setSystemValue(null, userName, "��������", dataGridViewGaInfo);
            //            break;
            //    }
            //}
        }

        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl3.SelectedTab == tabGaList)
                {
                    if (this.btnGaSave.Text == "����" && this.btnGaCancel.Enabled == true)
                    {
                        MessageBox.Show("�������һ����¼��������Ӻͱ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabControl3.SelectedTab = tabGaInfo;
                    }
                }
                else if (tabControl3.SelectedTab == tabGaInfo)
                {
                    if (this.dataGridViewGaInfo.Visible == false)
                    {
                        this.btnGaSave.Enabled = false;
                        this.btnGaCancel.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaInitialEditFields");
            }
        }

        private void dataGridViewGaInfo_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            return;
        }

        //dataGridViewGaInfo��ʼ�༭
        private bool GahasUpdate = false; //�ж���Ƭ�����Ƿ����

        private void dataGridViewGaInfo_CellEndEdit(object sender, DataGridViewCellEventArgs e) //�����䣬�����ж�����ĸ�ʽ������ȷ
        {
            string type = "";//�õ�����
            try
            {
                if (dataGridViewGaInfo.Rows[e.RowIndex].Cells[0].Value.ToString() == "���֤����")
                {
                    if (dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value != null)
                    {
                        GaCheckIdentity(dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value.ToString());
                    }
                }
                else
                {
                    if (this.dataGridViewGaInfo.CurrentCell == null || this.dataGridViewGaInfo.CurrentCell.Value == null)
                    {
                        return;
                    }

                    string value = this.dataGridViewGaInfo.CurrentCell.Value.ToString().Trim();
                    type = dataGridViewGaInfo.Rows[dataGridViewGaInfo.CurrentCell.RowIndex].Cells[3].Value.ToString();
                    switch (type)
                    {                       
                        case "INTEGER":
                        case "LONG":
                            this.GacheckNumber(value);
                            break;
                        case "NUMBER":
                        case "FLOAT":
                        case "DOUBLE":
                            this.checkFloat(value);
                            break;
                    }
                }
                setSystemValue(null, userName, "��������", dataGridViewGaInfo);
            }
            catch (Exception ex)
            {
                writeToLog(ex,"dataGridViewGaInfo_CellEndEdit");
            }
        }

        /// <summary>
        /// ����ĳ�ֶ��Զ�����ֵ   lili 2010-9-26
        /// </summary>
        /// <param name="sql">���ҵ�ֵ��sql</param>
        /// <param name="telNo">Ҫ���ɵ�ֵ</param>
        /// <param name="valueName">������������</param>
        private void setSystemValue(string sql, string telNo, string valueName,System.Windows.Forms.DataGridView data)
        {
            try
            {
                DataTable table = null;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                if (sql != null && sql != "")
                {
                    table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                    telNo = table.Rows[0][0].ToString();
                }
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    if (data.Rows[i].Cells[0].Value.ToString() == valueName)
                    {
                        data.Rows[i].Cells[1].Value = telNo;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setSystemValue");
            }
        }

        private void dataGridViewGaInfo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                btnGaSave.Enabled = true;
                btnGaCancel.Enabled = true; 
                
                if (dataGridViewGaInfo.Rows.Count > 2)
                {
                    string cellName = dataGridViewGaInfo.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string celStr = dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string sqlStr = "", telNo = "";

                    switch (cellName)
                    {
                        case "�����ɳ���":
                            sqlStr = "select �ɳ������� from �����ɳ��� where �ɳ�����='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "�����ɳ�������", dataGridViewGaInfo);
                            break;
                        case "�����ж�":
                            sqlStr = "select �жӴ��� from �������ж� where �ж���='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "�����жӴ���", dataGridViewGaInfo);
                            break;
                        case "����������":
                            sqlStr = "select �����Ҵ��� from ���������� where ��������='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "���������Ҵ���", dataGridViewGaInfo);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaInfo_CellValueChanged");
            }
        }

        private void dataGridViewGaInfo_Resize(object sender, EventArgs e)
        {
            try
            {
                DataGridView dataGridView = (DataGridView)sender;
                if (dataGridView.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView);
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 135;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaInfo_Resize");
            }
        }

        //�ж������Ƿ��ظ���������fisher��ӣ�09-08-28
        private bool GaisZhujian()
        {
            OracleConnection Conn = new OracleConnection(mysqlstr); //�������ݿ�
            Conn.Open();
            CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
            string Pdstr = "select " + CLC.ForSDGA.GetFromTable.ObjID+ " from " + comboTable.Text;
            OracleCommand orcmd = new OracleCommand(Pdstr, Conn);
            OracleDataAdapter PdAdapter = new OracleDataAdapter(orcmd);
            DataSet Pdds = new DataSet();
            PdAdapter.Fill(Pdds);
            DataTable Pddt = Pdds.Tables[0];
            try
            {
                int ix = 0;
                for (int j = 0; j < dataGridViewGaInfo.Rows.Count; j++)
                {
                    if (dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = j;
                        break;
                    }
                }
                if (Pddt.Rows.Count < 1)
                {
                    orcmd.Dispose();
                    Conn.Close();
                    return false;
                }
                else
                {
                    for (int i = 0; i < Pddt.Rows.Count; i++)
                    {
                        //string abc = Pddt.Rows[i][0].ToString();
                        if (Pddt.Rows[i][0].ToString() == dataGridViewGaInfo.Rows[ix].Cells[1].Value.ToString())
                        {
                            orcmd.Dispose();
                            Conn.Close();
                            return true;
                        }
                    }
                }
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaisZhujian()");
                MessageBox.Show(ex.Message,"GaisZhujian()");
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
        }

        //�����޸ļ�¼  ��feng 2009-08-19��
        private void btnGaSave_Click(object sender, EventArgs e)
        {
            //���ж������ֶ�,��������ֵ�Ѵ���,������ʾ
            OracleConnection Conn = new OracleConnection(mysqlstr); //�������ݿ�
            this.Cursor = Cursors.WaitCursor;
            this.dataGridViewGaInfo.CurrentCell = null;//�ڽ��б����ʱ����dataGridViewGaInfoʧȥ����
            this.mapControl1.Focus();

            bool GaisorKey = false; //�ж��Ƿ������ظ�

            if (isOracleSpatialTab)
            {
                OracleDataReader dr = null;
                try
                {

                    Conn.Open();
                    OracleCommand cmd;
                    if (btnGaSave.Text == "����")
                    {
                        GaisorKey = GaisZhujian();
                        if (GaisorKey)
                        {
                            MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            GaisorKey = false;
                            this.Cursor = Cursors.Default;
                            btnLoc3.Enabled = false;
                            return;
                        }
                        else
                        {
                            cmd = new OracleCommand("select max(MI_PRINX) from " + comboTable.Text, Conn);
                            dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                dr.Read();
                                if (dr.GetValue(0) != null)
                                {
                                    selPrinx = Convert.ToInt32(dr.GetValue(0)) + 1;
                                }
                            }
                            dr.Close();
                            cmd.Dispose();

                            feature = new Feature(lTable.TableInfo.Columns);
                            feature.Geometry = ft.Geometry;
                            feature.Style = ft.Style;

                            string strValue = "";
                            for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                            {
                                strValue = "";
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                }
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "��Ƭ")
                                {
                                    continue;
                                }
                                if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "����")
                                {
                                    if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " ����Ϊ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        //this.dataGridViewGaInfo.CurrentCell = null;
                                        return;
                                    }
                                }
                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    feature[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = Convert.ToDateTime(strValue);
                                }
                                else
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    //���´�����fisher���   ��09-09-01��
                                    if (isOracleSpatialTab)
                                    {
                                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                        {
                                            //feature["GEOLOC.SD0_POINT.X"] = strValue;
                                            continue;
                                        }
                                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                        {
                                            //feature["GEOLOC.SD0_POINT.Y"] = strValue;
                                            continue;
                                        }
                                        else
                                        {
                                            feature[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = strValue;
                                        }
                                    }
                                    else
                                    {
                                        feature[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = strValue;
                                    }
                                }
                            }
                            feature["MI_PRINX"] = selPrinx;

                            for (int i = 0; i < feature.Columns.Count; i++)
                            {
                                if (feature.Columns[i].Alias.ToUpper() == "mapid")
                                {
                                    feature["mapid"] = selPrinx;
                                    break;
                                }
                            }
                            lTable.InsertFeature(feature);


                            //����featureֻ��¼ʱ��ֵ�����ڲ���,�����ٴθ���date��ֵ
                            string command = "update " + comboTable.Text + " set ";
                            strValue = "";
                            bool isUpdate = false;
                            for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                }

                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE" && strValue != "")
                                {
                                    DateTime dTime = Convert.ToDateTime(strValue);
                                    if (dTime.TimeOfDay.ToString() != "00:00:00")
                                    {
                                        command += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                        isUpdate = true;
                                    }
                                }
                            }
                            if (isUpdate)
                            {
                                command = command.Remove(command.Length - 1);
                                command += " where MI_PRINX=" + selPrinx;
                                cmd = new OracleCommand(command, Conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                Conn.Close();
                            }

                            GaaddToList(feature.Geometry.Centroid.x, feature.Geometry.Centroid.y);

                            // ����ӵ����ݲ��������Ͼ�ִ�и��£���Ϊ�����ֻ���л���dataGridViewGaListѡ��ĳ����¼����ܸ���
                            // updated by fisher in 09-10-23
                            this.dataGridViewGaInfo.Columns[1].ReadOnly = true;

                            btnLoc3.Enabled = false;
                        }
                    }
                    else
                    {//����
                        string command = "update " + comboTable.Text + " t set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                        {
                            strValue = "";
                            if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            }

                            if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "����")
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                {
                                    MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " ����Ϊ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Cursor = Cursors.Default;
                                    //this.dataGridViewGaInfo.CurrentCell = null;
                                    return;
                                }
                            }

                            if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() != "��Ƭ")
                            {
                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    if (strValue == "")     //update by fisher in 09-12-24
                                    { continue; }
                                    command += "t." + dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X = '0',";
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X='" + strValue + "',";
                                    }
                                }
                                else if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y = '0',";
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y ='" + strValue + "',";
                                    }
                                }
                                else
                                {
                                    command += "t." + dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                                }
                            }
                        }
                        command = command.Remove(command.Length - 1);
                        //command += " where MI_PRINX=" + selPrinx;
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                        command += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                        cmd = new OracleCommand(command, Conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        Conn.Close();

                        //�������״̬ fisher
                        //this.toolLoc.Enabled = false;
                        this.mapControl1.Tools.LeftButtonTool = "Pan";
                        this.UncheckedTool();
                        btnLoc3.Enabled = false;
                        GaupdateListValue();
                    }
                    //���µ�ͼҪ�ص�����
                    GaupdateMapValue();

                }
                catch (OracleException ex)
                {
                    if (ex.Code == 1)//�����ֶ������˷�Ψһֵ
                    {
                        MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "btnGaSave_Click");
                    }
                    this.Cursor = Cursors.Default;
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "��ʾ");
                    writeToLog(ex, "btnGaSave_Click");
                    this.Cursor = Cursors.Default;
                    return;
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                        Conn.Close();
                }
                if (comboTable.Text.Trim() == "�˿�ϵͳ")
                {
                    if (GahasUpdate)//�ж��Ƿ��Ѿ�����������
                    {
                        Conn = new OracleConnection(mysqlstr); //�������ݿ�
                        OracleCommand cmd;
                        Conn.Open();
                        string strValue = "";

                        try
                        {
                            string strExe = "";
                            strValue = "";
                            for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "���֤����")
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                    break;
                                }
                            }
                            if (strValue != "")
                            {
                                if (btnGaSave.Text == "����")
                                {
                                    strExe = "insert into �˿���Ƭ(���֤����,��Ƭ) values('" + strValue + "' ,:imgData)";
                                }
                                else
                                {
                                    //�Ȳ�ѯ,�����˿���Ƭ����û�ж�Ӧ�Ķ���
                                    cmd = new OracleCommand("select * from �˿���Ƭ where ���֤����='" + strValue + "'", Conn);
                                    OracleDataReader oDr = cmd.ExecuteReader();
                                    if (oDr.HasRows)
                                    {
                                        strExe = "update �˿���Ƭ set ��Ƭ=:imgData where ���֤����='" + strValue + "'";
                                    }
                                    else
                                    {
                                        strExe = "insert into �˿���Ƭ(���֤����,��Ƭ) values('" + strValue + "' ,:imgData)";
                                    }
                                    oDr.Close();
                                }
                                cmd = new OracleCommand(strExe, Conn);

                                if (fileName == "")
                                {
                                    fileName = Application.StartupPath + "\\Ĭ��.bmp";
                                }

                                FileStream fs = File.OpenRead(fileName);
                                byte[] imgData = new byte[fs.Length];
                                long n = fs.Read(imgData, 0, imgData.Length);
                                cmd.Parameters.Add(":ImgData", OracleType.LongRaw).Value = imgData;
                                fs.Close();

                                cmd.ExecuteNonQuery();
                                btnLoc3.Enabled = false;
                                cmd.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "btnGaSave_Click");
                            MessageBox.Show(ex.Message, "btnGaSave_Click,110");
                        }
                        finally
                        {
                            if (Conn.State == ConnectionState.Open)
                                Conn.Close();
                        }
                    }
                }
            }

            else  //��oracleSpatial��
            {
                try
                {
                    Conn.Open();
                    OracleCommand cmd;
                    string strExe = "";
                    if (btnGaSave.Text == "����")
                    {
                        GaisorKey = GaisZhujian();
                        if (GaisorKey)
                        {
                            MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            GaisorKey = false;
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        else
                        {
                            //cmd = new OracleCommand("select max(mapid) from " + comboTable.Text, Conn);
                            //dr = cmd.ExecuteReader();
                            //if (dr.HasRows)
                            //{
                            //    dr.Read();
                            //    if (dr.GetValue(0) != null)
                            //    {
                            //        mapId = Convert.ToInt32(dr.GetValue(0)) + 1;
                            //    }
                            //}
                            //dr.Close();
                            strExe = "insert into " + comboTable.Text + "(";

                            for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "��ȫ������λ�ļ�")
                                    continue;
                                if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "����")
                                {
                                    if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " ����Ϊ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        return;
                                    }
                                }
                                if (i == 0)
                                    strExe += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                                else
                                    strExe += "," + dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                            }

                            strExe += ") values(";

                            string strValue = "";
                            for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                }
                                else
                                {
                                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        strValue = "0";
                                    }
                                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        strValue = "0";
                                    }
                                }
                                if (strValue == "����༭")
                                    continue;
                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    strExe += "to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else
                                {
                                    strExe += "'" + strValue + "',";
                                }
                            }

                            //strExe += dx + "," + dy + ")";
                            strExe = strExe.Remove(strExe.Length - 1);
                            strExe += ")";

                            selMapID = mapId.ToString();

                            // ����ӵ����ݲ��������Ͼ�ִ�и��£���Ϊ�����ֻ���л���dataGridViewGaListѡ��ĳ����¼����ܸ���
                            // updated by fisher in 09-10-23
                            this.dataGridViewGaInfo.Columns[1].ReadOnly = true;

                            GaaddToList(dx, dy);
                        }
                    }
                    else
                    {   //����
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                        strExe = "update " + comboTable.Text + " set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                        {
                            //update by siumo 09-01-08, ��Ϊnullʱ,toString����
                            strValue = "";
                            if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            }
                            else
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                    strValue = "0";
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                    strValue = "0";
                            }
                            if (strValue == "����༭")
                                continue;
                            if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "����")
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                {
                                    MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " ����Ϊ��.", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Cursor = Cursors.Default;
                                    return;
                                }
                            }
                            if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE" && strValue != "")
                            {
                                strExe += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                            }
                            else
                            {
                                strExe += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                            }
                        }
                        strExe = strExe.Remove(strExe.Length - 1);
                        //getName.ObjID
                        //strExe += " where MapID='" + this.selMapID + "'";
                        strExe += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                        btnLoc3.Enabled = false;

                    }
                    cmd = new OracleCommand(strExe, Conn);
                    cmd.ExecuteNonQuery();
                    GaupdateListValue();
                    cmd.Dispose();
                    Conn.Close();
                }
                catch (OracleException ex)
                {
                    if (ex.Code == 1)//�����ֶ������˷�Ψһֵ
                    {
                        MessageBox.Show("���ݿ����Ѵ��ڴ˱�ŵ�����,���޸�;\r\r���Ҫ���´˱������,���Ƚ��в�ѯ!!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "btnGaSave_Click");
                    }
                    this.Cursor = Cursors.Default;
                    btnLoc3.Enabled = false;
                    return;
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                        Conn.Close();
                }

                //���µ�ͼҪ�ص�����
                GaupdateMapValue();

            }
            string editMothed = "���";
            if (btnGaSave.Text == "����")
            {
                editMothed = "�޸�����";
                MessageBox.Show("���³ɹ�!");
            }
            else
            {
                MessageBox.Show("������ݳɹ�!");
                btnGaSave.Text = "����";
                btnGaCancel.Text = "ȡ��";
            }

            //��¼�༭log
            WriteEditLog(comboTable.Text.Trim(), selMapID, editMothed);
            this.dataGridViewGaInfo.CurrentCell = null;
            btnGaSave.Enabled = false;
            btnGaCancel.Enabled = false;
            this.mapControl1.Tools.LeftButtonTool = "Pan";
            this.UncheckedTool();
            try
            {
                string tabName = comboTable.Text.Trim();
                if (isOracleSpatialTab)
                {
                    tabName += "_tem";
                }
                if (temEditDt.Rows[0]["ҵ�����ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex) { writeToLog(ex, "btnGaSave_Click"); }
            this.Cursor = Cursors.Default;
            this.dataGridViewGaList.ContextMenuStrip = null;
            btnLoc3.Enabled = false;
        }

        private void btnGaCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnGaCancel.Text == "ȡ�����")
                {
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++) //���datagridview
                    {
                        dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                    }
                    dataGridViewGaInfo.Visible = false;
                    btnGaSave.Enabled = false;
                    btnGaCancel.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();

                    GadeleteFeature();
                    this.mapControl1.Focus();
                }
                else if(btnGaCancel.Text == "ȡ��")
                {
                    try
                    {
                        for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                        {
                            if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "��Ƭ")
                                continue;
                            this.dataGridViewGaInfo.Rows[i].Cells[1].Value = this.dataGridViewGaList.Rows[rowIndex].Cells[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        MessageBox.Show(ex.Message, "btnGaCancel_Click(ȡ��)");
                        btnLoc3.Enabled = false;
                    }

                    //��ͼ�ϵĵ�λ�ø�ԭ
                    Feature f;

                    //added by fisher in 09-09-22

                    if (this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() == "" || this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() == "" || this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() == "0" || this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() == "0")
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        btnGaSave.Enabled = false;
                        btnGaCancel.Enabled = false;
                        this.mapControl1.Tools.LeftButtonTool = "Pan";
                        this.UncheckedTool();
                        btnLoc3.Enabled = false;
                        return;
                        //ɾ���������� 
                    }

                    if (Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value) != 0)
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        //��ɾ��
                    }

                    //��ӵ�
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    string sName =CLC.ForSDGA.GetFromTable.ObjName;

                    f = new Feature(this.editTable.TableInfo.Columns);
                    f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(Convert.ToDouble(this.dataGridViewGaList.Rows[rowIndex].Cells["X"].Value), Convert.ToDouble(this.dataGridViewGaList.Rows[rowIndex].Cells["Y"].Value)));
                    f["mapid"] = this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value.ToString();
                    f["name"] = this.dataGridViewGaList.Rows[rowIndex].Cells[sName].Value.ToString();
                    f.Style = featStyle;
                    this.editTable.InsertFeature(f);

                    this.dataGridViewGaInfo.CurrentCell = null;
                    this.btnLoc3.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnGaCancel_Click");
                MessageBox.Show(ex.Message, "btnGaCancel_Click");
            }
        }

        private void mapToolBar2_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button == GaAddPoint)
                {
                    this.UncheckedTool();
                }              
                if (e.Button == GaPointStyle)
                {
                    SymbolStyleDlg pStyleDlg = new SymbolStyleDlg();
                    if (pStyleDlg.ShowDialog() == DialogResult.OK)
                    {
                        featStyle = pStyleDlg.SymbolStyle;
                    }
                }              
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar2_ButtonClick");
                MessageBox.Show(ex.Message, "mapToolBar2_ButtonClick");
            }
        }

        private void comboTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridViewGaInfo.Visible && btnGaCancel.Enabled)
            {
                MessageBox.Show("�������һ����¼��������Ӻͱ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dataGridViewGaList_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
           
        }

        private void bindingNavigatorGa_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                tabName = CLC.ForSDGA.GetFromTable.TableName;
                int countShu = Convert.ToInt32(this.toolStripTextBox1.Text);
                if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        if (_first == nMax)
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _first = nMax - (nMax % countShu);
                            _end = nMax - (nMax % countShu) + 1 - countShu;
                        }
                        else
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _first -= countShu;
                            _end -= countShu;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent++;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _first += countShu;
                        _end += countShu;
                    }
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ�����ҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = 0;
                        _first = countShu;
                        _end = 1;
                    }
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        MessageBox.Show("�Ѿ���βҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = pageCount;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _first = nMax;
                        _end = nMax - (nMax % countShu) + 1;
                    }
                }
                else
                {
                    return;
                }

                isShowPro(true);
                this.PageNow.Text = pageCurrent.ToString();//���õ�ǰҳ

                //���´����������datagridview��feng��
                this.Cursor = Cursors.WaitCursor;
                dataGridViewGaList.Columns.Clear();
                //DataTable datatable = Ga_exportDT = LoadData(PageSQL); //��ȡ��ǰҳ����
                DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //��ȡ��ǰҳ����; lili 2010-8-26
                dataGridViewGaList.DataSource = datatable;
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                //dataGridViewList.Visible = true;
                //����gridview�ļ����ɫ
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;
                //Application.DoEvents();

                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//���datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                }
                this.Cursor = Cursors.Default;
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "bindingNavigatorGa_ItemClicked()");
                MessageBox.Show(ex.Message, "bindingNavigatorGa_ItemClicked()");
                this.Cursor = Cursors.Default;
            }
        }

        private void PageNow_TextChanged(object sender, EventArgs e)
        {
            
            //}
            //catch (Exception ex)
            //{
            //    CLC.BugRelated.ExceptionWrite(ex, "bindingNavigatorGa_ItemClicked()");
            //    MessageBox.Show(ex.Message, "bindingNavigatorGa_ItemClicked()");
            //    this.Cursor = Cursors.Default;
            //}
        }

        private void PageNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                try
                {
                    if (Convert.ToInt32(this.PageNow.Text) < 1 || Convert.ToInt32(this.PageNow.Text) > pageCount)
                    {
                        MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow.Text = pageCurrent.ToString();
                        return;
                    }
                    //else if (Char.IsNumber(Convert.ToChar(this.PageNow.Text))==false) 
                    //{
                    //    MessageBox.Show("���������֣�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                    //    this.PageNow.Text = pageCurrent.ToString();
                    //    return;
                    //}
                    else
                    {
                        isShowPro(true);
                        this.pageCurrent = Convert.ToInt32(this.PageNow.Text);
                        nCurrent = pageSize * (pageCurrent - 1);
                        _end = ((pageCurrent - 1) * pageSize) + 1;
                        _first = _end + pageSize - 1;

                        //���´����������datagridview��feng��
                        this.Cursor = Cursors.WaitCursor;
                        dataGridViewGaList.Columns.Clear();
                        DataTable datatable = Ga_exportDT = LoadData(_mySQL,_first,_end,tabName); //��ȡ��ǰҳ����
                        dataGridViewGaList.DataSource = datatable;
                        this.toolEditPro.Value = 1;
                        Application.DoEvents();

                        if (dataGridViewGaList.Columns["mapid"] != null)
                        {
                            dataGridViewGaList.Columns["mapid"].Visible = false;
                        }
                        //dataGridViewList.Visible = true;
                        //����gridview�ļ����ɫ
                        for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                        {
                            if (i % 2 == 1)
                            {
                                dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                            }
                            else
                            {
                                dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                            }
                        }
                        this.toolEditPro.Value = 2;
                        //Application.DoEvents();

                        this.insertGaQueryIntoTable(datatable);
                        this.dataGridViewGaInfo.Visible = false;
                        this.btnGaSave.Enabled = false;
                        this.btnGaCancel.Enabled = false;

                        for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//���datagridview
                        {
                            this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                        }
                        this.Cursor = Cursors.Default;
                        this.toolEditPro.Value = 3;
                        Application.DoEvents();
                        isShowPro(false);
                    }
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "PageNow_KeyPress()");
                }
            }            
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r" && this.dataGridViewGaList.DataSource!=null)
            {
                try
                {
                    isShowPro(true);
                    this.Cursor = Cursors.WaitCursor;
                    this.pageSize = Convert.ToInt32(this.toolStripTextBox1.Text);
                    this.pageCount = (nMax / pageSize);//�������ҳ��
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//������ҳ��
                    if (nMax != 0)
                    {
                        this.pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//���õ�ǰҳ
                    }
                    this.nCurrent = 0;       //��ǰ��¼����0��ʼ
                    _first = pageSize;
                    _end = 1;

                    //���´����������datagridview��feng��
                    this.Cursor = Cursors.WaitCursor;
                    dataGridViewGaList.Columns.Clear();
                    DataTable datatable = Ga_exportDT = LoadData(_mySQL,_first,_end,tabName); //��ȡ��ǰҳ����
                    dataGridViewGaList.DataSource = datatable;
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();

                    if (dataGridViewGaList.Columns["mapid"] != null)
                    {
                        dataGridViewGaList.Columns["mapid"].Visible = false;
                    }
                    //dataGridViewList.Visible = true;
                    //����gridview�ļ����ɫ
                    for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                        else
                        {
                            dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                        }
                    }
                    this.toolEditPro.Value = 2;
                    //Application.DoEvents();

                    this.insertGaQueryIntoTable(datatable);
                    this.dataGridViewGaInfo.Visible = false;
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;

                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//���datagridview
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                    }
                    this.Cursor = Cursors.Default;
                    this.toolEditPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        private void tsTextBoxPageNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                try
                {
                    if (Convert.ToInt32(this.tsTextBoxPageNow.Text) < 1 || Convert.ToInt32(this.tsTextBoxPageNow.Text) > bpageCount)
                    {
                        MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();
                        return;
                    }
                    else
                    {
                        isShowPro(true);
                        this.Cursor = Cursors.WaitCursor;
                        this.bpageCurrent = Convert.ToInt32(this.tsTextBoxPageNow.Text);
                        this.bnCurrent = bpageSize * (bpageCurrent - 1);       //��ǰ��¼����0��ʼ

                        //���´����������datagridview��feng��
                        this.Cursor = Cursors.WaitCursor;
                        dataGridViewList.Columns.Clear();

                        DataTable datatable = _exportDT = BJLoadData(bPageSQL); //��ȡ��ǰҳ����
                        dataGridViewList.DataSource = datatable;
                        this.toolEditPro.Value = 1;
                        Application.DoEvents();

                        if (dataGridViewList.Columns["mapid"] != null)
                        {
                            dataGridViewList.Columns["mapid"].Visible = false;
                        }
                        //dataGridViewList.Visible = true;
                        //����gridview�ļ����ɫ
                        setDataGridBG();
                        this.toolEditPro.Value = 2;
                        //Application.DoEvents();

                        this.insertQueryIntoTable(datatable);
                        this.dataGridView1.Visible = false;
                        this.buttonSave.Enabled = false;
                        this.buttonCancel.Enabled = false;

                        for (int i = 0; i < dataGridView1.Rows.Count; i++)//���datagridview
                        {
                            this.dataGridView1.Rows[i].Cells[1].Value = "";
                        }
                        this.Cursor = Cursors.Default;
                        this.toolEditPro.Value = 3;
                        Application.DoEvents();
                    }
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        private void bindingNavigatorEdit_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Text == "��һҳ")
                {
                    if (bpageCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent--;
                        bnCurrent = bpageSize * (bpageCurrent - 1);
                    }
                }
                else if (e.ClickedItem.Text == "��һҳ")
                {
                    if (bpageCurrent > bpageCount - 1)
                    {
                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent++;
                        bnCurrent = bpageSize * (bpageCurrent - 1);
                    }
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (bpageCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ�����ҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent = 1;
                        bnCurrent = 0;
                    }
                }
                else if (e.ClickedItem.Text == "ת��ĩҳ")
                {
                    if (bpageCurrent > bpageCount - 1)
                    {
                        MessageBox.Show("�Ѿ���βҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent = bpageCount;
                        bnCurrent = bpageSize * (bpageCurrent - 1);
                    }
                }
                else
                {
                    return;
                }

                isShowPro(true);
                this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//���õ�ǰҳ

                //���´����������datagridview��feng��
                this.Cursor = Cursors.WaitCursor;
                dataGridViewList.Columns.Clear();

                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //��ȡ��ǰҳ����
                dataGridViewList.DataSource = datatable;
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                if (dataGridViewList.Columns["mapid"] != null)
                {
                    dataGridViewList.Columns["mapid"].Visible = false;
                }
                //dataGridViewList.Visible = true;
                //����gridview�ļ����ɫ
                setDataGridBG();
                this.toolEditPro.Value = 2;
                //Application.DoEvents();

                this.insertQueryIntoTable(datatable);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//���datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                this.Cursor = Cursors.Default;
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "bindingNavigatorGa_ItemClicked()");
                MessageBox.Show(ex.Message, "bindingNavigatorGa_ItemClicked()");
                this.Cursor = Cursors.Default;
            }
        }

        private void toolStripPageSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r" && this.dataGridViewList.DataSource!=null)
            {
                try
                {
                    isShowPro(true);
                    this.Cursor = Cursors.WaitCursor;
                    this.bpageSize = Convert.ToInt32(this.toolStripPageSize.Text);
                    this.bpageCount = (bnMax / bpageSize);//�������ҳ��
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//������ҳ��
                    if (bnMax != 0)
                    {
                        this.bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//���õ�ǰҳ
                    }
                    this.bnCurrent = 0;       //��ǰ��¼����0��ʼ


                    //���´����������datagridview��feng��
                    this.Cursor = Cursors.WaitCursor;
                    dataGridViewList.Columns.Clear();

                    DataTable datatable =_exportDT = BJLoadData(bPageSQL); //��ȡ��ǰҳ����
                    dataGridViewList.DataSource = datatable;
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();

                    if (dataGridViewList.Columns["mapid"] != null)
                    {
                        dataGridViewList.Columns["mapid"].Visible = false;
                    }
                    //dataGridViewList.Visible = true;
                    //����gridview�ļ����ɫ
                    setDataGridBG();
                    this.toolEditPro.Value = 2;
                    //Application.DoEvents();

                    this.insertQueryIntoTable(datatable);
                    this.dataGridView1.Visible = false;
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)//���datagridview
                    {
                        this.dataGridView1.Rows[i].Cells[1].Value = "";
                    }
                    this.Cursor = Cursors.Default;
                    this.toolEditPro.Value =3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        private void toolStripPageSize_Click(object sender, EventArgs e)
        {

        }

        //�Ѷ�λ����(fisher09-08-31)
        private void btnGaLocatedYes_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
            tabName = CLC.ForSDGA.GetFromTable.TableName;
            _mySQL = "";
            _PageSQL = "";
            _first = Convert.ToInt32(this.toolStripTextBox1.Text);
            _end = 1;
            string getSqlStr = getSqlString();
            if (dataGridViewGaList.DataSource == null || dataGridViewGaList.Visible == false || getSqlStr == "")
            //if(PageSQL=="")
            {
                if (isOracleSpatialTab)
                {
                   // PageSQL = "select MI_PRINX as mapid," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTable.Text + " t" + " where (t.geoloc.SDO_POINT.X is not null and t.geoloc.SDO_POINT.Y is not null and t.geoloc.SDO_POINT.X != '0' and t.geoloc.SDO_POINT.Y != '0')";
                    PageSQL = "select count(*) from " + tabName + " t" + " where (t.geoloc.SDO_POINT.X is not null and t.geoloc.SDO_POINT.Y is not null and t.geoloc.SDO_POINT.X != '0' and t.geoloc.SDO_POINT.Y != '0') or �����ֶ�һ is not null ";
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is not null and a.geoloc.SDO_POINT.Y is not null and a.geoloc.SDO_POINT.X != '0' and a.geoloc.SDO_POINT.Y != '0')  or �����ֶ�һ is not null) ";

                }
                else
                {
                    //PageSQL = "select rownum as mapid," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " where (X is not null and Y is not null and X != '0' and Y != '0')";
                    PageSQL = "select count(*) from " + tabName + " where (X is not null and Y is not null and X != '0' and Y != '0') or �����ֶ�һ is not null";
                    _mySQL = " and ((a.X is not null and a.Y is not null and a.X != '0' and a.Y != '0')  or �����ֶ�һ is not null) ";
                }

                //string strRegion = strRegion;
                //string strRegion1 = strRegion1;
                if (strRegion != "˳����" && strRegion != "")  //edit by fisher in 09-12-01
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "������Ϣ" || tabName == "��������"))
                {
                    if (_PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";                        
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }
            else
            {
                if (isOracleSpatialTab)
                {
                    PageSQL = "select count(*) from " + tabName + " t" + " where ((t.geoloc.SDO_POINT.X is not null and t.geoloc.SDO_POINT.Y is not null and t.geoloc.SDO_POINT.X != '0' and t.geoloc.SDO_POINT.Y != '0') or �����ֶ�һ is not null) and " + getSqlStr;
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is not null and a.geoloc.SDO_POINT.Y is not null and a.geoloc.SDO_POINT.X != '0' and a.geoloc.SDO_POINT.Y != '0') or �����ֶ�һ is not null)";
                }
                else
                {
                    PageSQL = "select count(*) from " + tabName + " where ((X is not null and Y is not null and X != '0' and Y != '0') or �����ֶ�һ is not null) and " + getSqlStr;
                    _mySQL = " and ((a.X is not null and a.Y is not null and a.X != '0' and a.Y != '0') or �����ֶ�һ is not null) ";
                }

                if (strRegion != "˳����" && strRegion != "")  //edit by fisher in 09-12-01
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "������Ϣ" || tabName == "��������"))
                {
                    if (_PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }

            this.Cursor = Cursors.WaitCursor;
            try
            {
                conn.Open();
                PageSQL += _PageSQL;
                OracleCommand cmd = new OracleCommand(PageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                dAdapter.Fill(dt);
                cmd.Dispose();
                conn.Close();
                if (Convert.ToInt32(dt.Rows[0][0]) < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.RecordCount.Text = "0��";
                    this.PageNow.Text = "0";//���õ�ǰҳ
                    this.lblPageCount.Text = "/ {0}";//������ҳ��
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    return;
                }

                //���´����������÷�ҳ
                try
                {
                    nMax = Convert.ToInt32(dt.Rows[0][0]);
                    this.RecordCount.Text = nMax.ToString() + "��";
                    pageSize = 100;      //����ҳ������
                    pageCount = (nMax / pageSize);//�������ҳ��
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//������ҳ��
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    if (nMax != 0)
                    {
                        pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//���õ�ǰҳ
                    }
                    nCurrent = 0;       //��ǰ��¼����0��ʼ
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnGaLocatedYes_Click ��ȡ��ҳ����");
                    MessageBox.Show(ex.Message, "btnGaLocatedYes_Click ��ȡ��ҳ����");
                }
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //��ȡ��ǰҳ����
                dataGridViewGaList.DataSource = datatable;
                
                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                //dataGridViewList.Visible = true;
                //����gridview�ļ����ɫ
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;
                //Application.DoEvents();
                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//���datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                } 
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex, "btnGaLocatedYes_Click");
            }

            this.Cursor = Cursors.Default;
        }

        //δ��λ����(fisher09-08-31)
        private void btnGaLocatedNo_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
            string tabName = CLC.ForSDGA.GetFromTable.TableName;
            _mySQL = "";
            _PageSQL = "";
            _first = Convert.ToInt32(this.toolStripTextBox1.Text);
            _end = 1;
            string getSqlStr = getSqlString();

            if (dataGridViewGaList.DataSource == null || dataGridViewGaList.Visible == false || getSqlStr == "")
            {
                if (isOracleSpatialTab)
                {
                    PageSQL = "select count(*) from " + tabName + " t" + " where (t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') and �����ֶ�һ is null";
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is null or a.geoloc.SDO_POINT.Y is null or a.geoloc.SDO_POINT.X = '0' or a.geoloc.SDO_POINT.Y = '0') and �����ֶ�һ is null)";
                }
                else
                {
                    PageSQL = "select count(*)  from " + tabName + " where (X is null or Y is null or X = '0' or Y = '0') and �����ֶ�һ is null";
                    _mySQL = " and ((a.X is null or a.Y is null or a.X = '0' or a.Y = '0') and �����ֶ�һ is null)";
                }

                //string strRegion = strRegion;
                //string strRegion1 = strRegion1;
                if (strRegion == "" && strRegion1 == "")
                {
                    isShowPro(false);
                    MessageBox.Show("��û�в�ѯȨ�ޣ�");
                    return;
                }
                if (strRegion != "˳����" && strRegion != "") 
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "������Ϣ" || tabName == "��������"))
                {
                    if (PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }
            else
            {
                if (isOracleSpatialTab)
                {
                    PageSQL = "select count(*) from " + tabName + " t" + " where ((t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') and �����ֶ�һ is null) and " + getSqlStr;
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is null or a.geoloc.SDO_POINT.Y is null or a.geoloc.SDO_POINT.X = '0' or a.geoloc.SDO_POINT.Y = '0')  and �����ֶ�һ is null) ";
                }
                else
                {
                    PageSQL = "select count(*)  from " + tabName + " where ((X is null or Y is null or X = '0' or Y = '0') and �����ֶ�һ is null) and " + getSqlStr;
                    _mySQL = " and ((a.X is null or a.Y is null or a.X = '0' or a.Y = '0') and �����ֶ�һ is null )";
                } 
                
                if (strRegion != "˳����" && strRegion != "")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "������Ϣ" || tabName == "��������"))
                {
                    if (PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }

            this.Cursor = Cursors.WaitCursor;
            try
            {
                conn.Open();
                PageSQL += _PageSQL;
                OracleCommand cmd = new OracleCommand(PageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                dAdapter.Fill(dt);
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();
                if (Convert.ToInt32(dt.Rows[0][0]) < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.pageCount = 0;
                    this.pageCurrent = 0;
                    this.RecordCount.Text = "0��";
                    this.PageNow.Text = "0";//���õ�ǰҳ
                    this.lblPageCount.Text = "/ {0}";//������ҳ��
                    this.toolStripTextBox1.Text = pageSize.ToString();

                    this.dataGridViewGaList.Columns.Clear();
                    this.dataGridViewGaInfo.Visible = false;

                    //try  //�ȹر�֮ǰ�ı�ͱ�ע��
                    //{
                    //    string sAlias = this.editTable.Alias;
                    //    if (mapControl1.Map.Layers[sAlias] != null)
                    //    {
                    //        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.Message, "btnGaLocatedNo_Click�ر�֮ǰ�ı�");
                    //}

                    return;
                }

                //���´����������÷�ҳ
                try
                {
                    nMax = Convert.ToInt32(dt.Rows[0][0]);
                    this.RecordCount.Text = nMax.ToString() + "��";
                    pageSize = 100;      //����ҳ������
                    pageCount = (nMax / pageSize);//�������ҳ��
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//������ҳ��
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    if (nMax != 0)
                    {
                        pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//���õ�ǰҳ
                    }
                    nCurrent = 0;       //��ǰ��¼����0��ʼ
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnGaLocatedNo_Click ��ȡ��ҳ����");
                    MessageBox.Show(ex.Message, "btnGaLocatedNo_Click ��ȡ��ҳ����");
                }
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = Ga_exportDT = LoadData(_mySQL,_first,_end,tabName); //��ȡ��ǰҳ����
                dataGridViewGaList.DataSource = datatable;

                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                //dataGridViewList.Visible = true;
                //����gridview�ļ����ɫ
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;
                //Application.DoEvents();
                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//���datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                }
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex, "btnGaLocatedNo_Click");
                MessageBox.Show(ex.Message, "btnGaLocatedNo_Click ����");
            }

            this.Cursor = Cursors.Default;
        }

        private void btnLocatedYes_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
            Cursor.Current = Cursors.WaitCursor;

            if (dataGridViewList.DataSource == null || dataGridViewList.Visible == false || blctStr == ""||textKeyWord.Text=="")  //updated by fisher in 09-10-10
            //if(PageSQL=="")
            {
                bPageSQL = "select rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " where (X is not null and Y is not null and X != '0' and Y != '0')";

                //string strRegion = strRegion;
                if (strRegion != "˳����")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    bPageSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                }
            }
            else
            {
                bPageSQL = blctStr + " and (X is not null and Y is not null and X != '0' and Y != '0')";
            }
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(bPageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt =ds.Tables[0];
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.toolStripRecord.Text = "0��";
                    this.tsTextBoxPageNow.Text = "0";//���õ�ǰҳ
                    this.tStripLabelPageCount.Text = "/ {0}";//������ҳ��
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    return;
                }

                //���´����������÷�ҳ
                try
                {
                    bnMax = dt.Rows.Count;
                    this.toolStripRecord.Text = bnMax.ToString() + "��";
                    bpageSize = 100;      //����ҳ������
                    bpageCount = (bnMax / bpageSize);//�������ҳ��
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//������ҳ��
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    if (bnMax != 0)
                    {
                        bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//���õ�ǰҳ
                    }
                    bnCurrent = 0;       //��ǰ��¼����0��ʼ
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnLocatedYes_Click()��ȡ��ҳ����");
                    MessageBox.Show(ex.Message, "btnLocatedYes_Click() ��ȡ��ҳ����");
                }

                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //��ȡ��ǰҳ����
                dataGridViewList.DataSource = datatable;

                if (dataGridViewList.Columns["mapid"] != null)
                {
                    dataGridViewList.Columns["mapid"].Visible = false;
                }
                //dataGridViewList.Visible = true;

                setDataGridBG();

                this.toolEditPro.Value = 2;
                //Application.DoEvents();

                this.insertQueryIntoTable(dt);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//���datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);

                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex,"btnLocatedYes_Click");
            }
            
            Cursor.Current = Cursors.Default;
        }

        private void btnLocatedNo_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
            Cursor.Current = Cursors.WaitCursor;

            if (dataGridViewList.DataSource == null || dataGridViewList.Visible == false || blctStr == ""||textKeyWord.Text=="") //updated by fisher in 09-10-10
            //if(PageSQL=="")
            {
                bPageSQL = "select rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " where (X is null or Y is null or X = '0' or Y = '0')";

                //string strRegion = strRegion;
                if (strRegion != "˳����")
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    bPageSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                }
            }
            else
            {
                bPageSQL = blctStr + " and (X is null or Y is null or X = '0' or Y = '0')";
            }
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(bPageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.toolStripRecord.Text = "0��";
                    this.tsTextBoxPageNow.Text = "0";//���õ�ǰҳ
                    this.tStripLabelPageCount.Text = "/ {0}";//������ҳ��
                    this.toolStripPageSize.Text = bpageSize.ToString();

                    this.bpageCount = 0;
                    this.bpageCurrent = 0;
                    this.dataGridViewList.Columns.Clear();
                    this.dataGridView1.Visible = false;

                    //�رձ��Ҫ���µĿɱ༭�ı���Ȼ�������!
                    //try  //�ȹر�֮ǰ�ı�ͱ�ע��
                    //{
                    //    string sAlias = this.editTable.Alias;
                    //    if (mapControl1.Map.Layers[sAlias] != null)
                    //    {
                    //        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.Message, "btnLocatedNo_Click()�ر�֮ǰ�ı�");
                    //}

                    return;
                }

                //���´����������÷�ҳ
                try
                {
                    bnMax = dt.Rows.Count;
                    this.toolStripRecord.Text = bnMax.ToString() + "��";
                    bpageSize = 100;      //����ҳ������
                    bpageCount = (bnMax / bpageSize);//�������ҳ��
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//������ҳ��
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    if (bnMax != 0)
                    {
                        bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//���õ�ǰҳ
                    }
                    bnCurrent = 0;       //��ǰ��¼����0��ʼ
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnLocatedNo_Click()��ȡ��ҳ����");
                    MessageBox.Show(ex.Message, "btnLocatedNo_Click() ��ȡ��ҳ����");
                }

                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //��ȡ��ǰҳ����
                dataGridViewList.DataSource = datatable;

                if (dataGridViewList.Columns["mapid"] != null)
                {
                    dataGridViewList.Columns["mapid"].Visible = false;
                }
                //dataGridViewList.Visible = true;

                setDataGridBG();
                this.toolEditPro.Value = 2;
                //Application.DoEvents();

                this.insertQueryIntoTable(dt);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//���datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["�������ݿɱ༭"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);

                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex, "btnLocatedNo_Click");
            }
            Cursor.Current = Cursors.Default;
        }

        private void butLoc2_Click(object sender, EventArgs e)
        {
            //Tools_Used(null, null);
            this.mapControl1.Tools.LeftButtonTool = "Location";
        }

        private void btnLoc1_Click(object sender, EventArgs e)
        {
            this.mapControl1.Tools.LeftButtonTool = "Location";
        }

        private void btnLoc3_Click(object sender, EventArgs e)
        {
            this.mapControl1.Tools.LeftButtonTool = "Location";
        }

        string videoX = "", videoY = "";  //��������������ȡ��ǰ����е�XYֵ���Ա����ȡ���ɹ��� fisher(09-09-27)
        int videoIndex = 0;
        private void dataGridViewVideo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex > -1)
                {
                    videoIndex = e.RowIndex;
                    videoX = dataGridViewVideo["X", e.RowIndex].Value.ToString();
                    videoY = dataGridViewVideo["Y", e.RowIndex].Value.ToString();

                    if (dataGridViewVideo["X", e.RowIndex].Value == null || dataGridViewVideo["Y", e.RowIndex].Value == null
                        || dataGridViewVideo["X", e.RowIndex].Value.ToString() == "" || dataGridViewVideo["Y", e.RowIndex].Value.ToString() == ""
                        || Convert.ToInt32(dataGridViewVideo["X", e.RowIndex].Value) == 0 || Convert.ToInt32(dataGridViewVideo["Y", e.RowIndex].Value) == 0)
                    {
                        //System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
                        if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() == "1")  // �б༭Ȩ�ޣ�09-12-15��
                        {
                            this.butLoc2.Enabled = true;
                        }
                        return;
                    }
                    double x = Convert.ToDouble(dataGridViewVideo["X", e.RowIndex].Value);
                    double y = Convert.ToDouble(dataGridViewVideo["Y", e.RowIndex].Value);
                    if (x == 0 || y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("�˶���δ��λ.");
                        if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() == "1")
                        {
                            this.butLoc2.Enabled = true;
                        }
                        return;
                    }

                    //edit by fisher in 09-12-24  ��λ��ͼ��Ұ
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

                    //������,�ӵ�ͼ�в���Ҫ��,��˸
                    FeatureLayer tempLayer = mapControl1.Map.Layers[editTable.Alias] as MapInfo.Mapping.FeatureLayer;
                    CLC.ForSDGA.GetFromTable.GetFromName("��Ƶλ�� ", getFromNamePath);
                    string sID = dataGridViewVideo[CLC.ForSDGA.GetFromTable.ObjID, e.RowIndex].Value.ToString();
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tempLayer.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("keyID='" + sID + "'"));
                    if (ft != null)
                    {
                        if (ft["mapid"] != null && ft["mapid"].ToString() != "")
                        {
                            selPrinx = Convert.ToInt32(ft["mapid"]);
                        }
                        selMapID = sID;  //??????????????????????
                        ////��˸Ҫ��
                        flashFt = ft;
                        defaultStyle = ft.Style;
                        k = 0;
                        timer1.Start();
                    }

                    if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() == "1")
                    {
                        this.butLoc2.Enabled = true;
                    }
                }
            }
            catch(Exception ex)
            {
                writeToLog(ex, "dataGridViewVideo_CellClick");
            }
        }

        private void frmMap_Load(object sender, EventArgs e)
        {
            
        }

        private void btnCancel2_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoX == "" || videoY == "")
                {
                    this.dataGridViewVideo["X", videoIndex].Value = 0.0;
                    this.dataGridViewVideo["Y", videoIndex].Value = 0.0;
                }
                else
                {
                    this.dataGridViewVideo["X", videoIndex].Value = Convert.ToDouble(videoX);
                    this.dataGridViewVideo["Y", videoIndex].Value = Convert.ToDouble(videoY);
                }
                this.btnCancel2.Enabled = false;
                this.buttonSaveWZ.Enabled = false;
                this.butLoc2.Enabled = false;

                Feature f;
                if (videoX == "" || videoY == "" || videoX == "0" || videoY == "0")
                {
                    //ɾ�����˳�
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(dataGridViewVideo.Rows[videoIndex].Cells["mapid"].Value));
                    si.QueryDefinition.Columns = null;
                    f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                    if (f != null)
                    {
                        this.editTable.DeleteFeature(f);
                    }
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    return;
                }
                //updated by fisher in 09-09-21
                else if (dataGridViewVideo.Rows[videoIndex].Cells["x"].Value.ToString() != "" && dataGridViewVideo.Rows[videoIndex].Cells["y"].Value.ToString() != "")
                {
                    if (Convert.ToDouble(dataGridViewVideo.Rows[videoIndex].Cells["x"].Value) != 0 && Convert.ToDouble(dataGridViewVideo.Rows[videoIndex].Cells["y"].Value) != 0)
                    {
                        //��ɾ��
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(dataGridViewVideo.Rows[videoIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                    }
                }

                //��ӵ�
                f = new Feature(this.editTable.TableInfo.Columns);
                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(Convert.ToDouble(videoX), Convert.ToDouble(videoY)));
                f["name"] = dataGridViewVideo.Rows[videoIndex].Cells["�豸����"].Value.ToString();
                f["mapid"] = dataGridViewVideo.Rows[videoIndex].Cells["mapid"].Value.ToString();
                f.Style = new MapInfo.Styles.BitmapPointStyle("sxt.bmp");
                this.editTable.InsertFeature(f);
                this.mapControl1.Tools.LeftButtonTool = "Pan";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnCancel2_Click");
            }
        }

        private void FieldStr_SelectedIndexChanged(object sender, EventArgs e)
        {
            V_setYSF(FieldStr.SelectedIndex);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValueStr.Text == "")
                {
                    MessageBox.Show("��ѯֵ����Ϊ�գ�");
                    return;
                }
                if (ValueStr.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("������ַ����в��ܰ���������!");
                    return;
                }
                string strExp = "";
                int p = FieldStr.SelectedIndex;
                string[] arr = V_arrType.Split(',');
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
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   " + ValueStr.Text.Trim();
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   " + ValueStr.Text.Trim();
                        }
                        this.dataGridExp.Rows.Add(new object[] { strExp, "����" });
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
            catch(Exception ex)
            {
                writeToLog(ex, "btnAdd_Click");
            }
        }

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
                            string text = this.dataGridExp.Rows[0].Cells["video_Value"].Value.ToString().Replace("����", "");

                            text = text.Replace("����", "").Trim();
                            this.dataGridExp.Rows[0].Cells["video_Value"].Value = text;
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
                writeToLog(ex, "btnDelete_Click");
            }
        }

        private void mapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                System.Drawing.Point sPoint = new System.Drawing.Point(e.X, e.Y);
                DPoint dPoint;
                mapControl1.Map.DisplayTransform.FromDisplay(sPoint, out dPoint);
                string nPoint = "X = " + dPoint.x.ToString("#.#####") + ", Y = " + dPoint.y.ToString("#.#####");
                switch (e.Button.ToString())
                {
                    case "Right":
                        //MessageBox.Show(nPoint, "����");
                        //Graphics objGraphics = this.CreateGraphics();
                        //Pen redPen = new Pen(Color.Red, 3);
                        //SolidBrush blueBrush = new SolidBrush(Color.Black);
                        //float X = 0.0F;
                        //float Y = 0.0F;
                        //float width = 30.0F;
                        //float heith = 30.0F;
                        //objGraphics.DrawRectangle(redPen, X, Y, width, heith);
                        //objGraphics.FillRectangle(blueBrush, X, Y, width, heith);

                        labelXY.Visible = true;
                        labelXY.Text = nPoint;
                        labelXY.Location = new System.Drawing.Point(e.X, e.Y);
                        break;
                    case "Left":
                        labelXY.Visible = false;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapControl1_MouseClick");
            }
        }

        //��ѯ�Ѷ�λ��Ƶ
        private void btnVlocYes_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() == "0")
                {
                    MessageBox.Show("��û�в�ѯȨ��!", "��ʾ");
                    return;
                }
                isShowPro(true);
                string sql = "", countSQL = "";
                if (dataGridViewVideo.DataSource == null || dataGridViewVideo.Visible == false || videolocSql == "" || this.dataGridExp.Rows.Count == 0)
                {
                    if (strRegion == "˳����")
                    {
                        sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where (x is not null and x!=0 and y is not null and y!=0)";
                        countSQL = "select count(*) from ��Ƶλ�� where (x is not null and x!=0 and y is not null and y!=0)";
                        sql = "(x is not null and x!=0 and y is not null and y!=0)";
                    }
                    else
                    {
                        sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where (x is not null and x!=0 and y is not null and y!=0) and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        countSQL = "select count(*) from ��Ƶλ�� where (x is not null and x!=0 and y is not null and y!=0) and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        sql = "(x is not null and x!=0 and y is not null and y!=0) and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else
                {
                    if (videolocSql.IndexOf("where") > -1)
                    {
                        sql = videolocSql + " and (X is not null and Y is not null and X != 0 and Y != 0)";
                        countSQL = "select count(*) from ��Ƶλ�� where " + videolocSql + " and (X is not null and Y is not null and X != 0 and Y != 0)";
                        sql = videolocSql + " and (X is not null and Y is not null and X != 0 and Y != 0)";
                    }
                    else
                    {
                        sql = videolocSql + " where (X is not null and Y is not null and X! = 0 and Y != 0)";
                        countSQL = "select count(*) from ��Ƶλ��  where " + videolocSql + " and (X is not null and Y is not null and X! = 0 and Y != 0)";
                        sql = videolocSql + " and (X is not null and Y is not null and X! = 0 and Y != 0)";
                    }                    
                }
                //OracelData linkData = new OracelData(mysqlstr);
                //ds = linkData.SelectDataBase(sql, "temVideo");
                this.getMaxCount(countSQL);
                InitDataSet(this.tsCount);
                _startNo = 0;
                _endNo = perPageSize;

                this.toolEditPro.Value = 1;
                Application.DoEvents();
                //DataTable dt = ds.Tables[0];
                DataTable dt = getLoadData(_startNo, _endNo, sql);
                dataGridViewVideo.DataSource = dt;
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                dataGridViewVideo.Columns[5].Visible = false;
                dataGridViewVideo.Columns[6].Visible = false;      // mapid����ʾ
                //dataGridViewVideo.Columns[4].ReadOnly = true;
                //dataGridViewVideo.Columns[3].ReadOnly = true;
                this.toolEditPro.Value = 2;
                //Application.DoEvents();
                insertQueryIntoTable(dt);
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch(Exception ex)
            {
                writeToLog(ex, "btnVlocYes_Click");
                isShowPro(false);
            }
        }

        //��ѯδ��λ��Ƶ
        private void btnVlocNo_Click(object sender, EventArgs e)
        {
            try
            {
                isShowPro(true);
                if (temEditDt.Rows[0]["��Ƶ�ɱ༭"].ToString() == "0")
                {
                    isShowPro(false);
                    MessageBox.Show("��û�в�ѯȨ��!", "��ʾ");
                    return;
                }

                (this.editTable as IFeatureCollection).Clear();  //�����ͼ�ϵ����е�
                this.editTable.Pack(PackType.All);
                string sql = "", countSQL = "";
                
                if (dataGridViewVideo.DataSource == null || dataGridViewVideo.Visible == false || videolocSql == "" || this.dataGridExp.Rows.Count == 0)
                {
                    if (strRegion == "˳����")
                    {
                        sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where (x is null or x=0 or y is null or y=0)";
                        countSQL = "select count(*) from ��Ƶλ�� where (x is null or x=0 or y is null or y=0)";
                        sql = " (x is null or x=0 or y is null or y=0)";
                    }
                    else
                    {
                        sql = "select �豸����,X,Y,�����ɳ���,�ճ�������,�豸���,mapid from ��Ƶλ�� where (x is null or x=0 or y is null or y=0) and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        countSQL = "select count(*) from ��Ƶλ�� where (x is null or x=0 or y is null or y=0) and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                        sql = " (x is null or x=0 or y is null or y=0) and �����ɳ��� in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else
                {
                    if (videolocSql.IndexOf("where") > -1)
                    {
                        sql = videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                        countSQL = "select count(*) from ��Ƶλ�� where " + videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                    }
                    else
                    {
                        sql = videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                        countSQL = "select count(*) from ��Ƶλ�� where " + videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                    }
                }

                this.getMaxCount(countSQL);
                if (vMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���������޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.pageVideoCount = 0;
                    this.pageVideoCurrent = 0;
                    this.RecordCount.Text = "0��";
                    this.tstNow.Text = "0"; //���õ�ǰҳ
                    this.bnCount.Text = "/ {0}";//������ҳ��
                    this.tstbPre.Text = this.perPageSize.ToString();

                    this.dataGridViewVideo.Columns.Clear();
                    return;
                }
                InitDataSet(this.tsCount);
                _startNo = 0;
                _endNo = perPageSize;

                this.toolEditPro.Value = 1;
                Application.DoEvents();
                //OracelData linkData = new OracelData(mysqlstr);
                //ds = linkData.SelectDataBase(sql, "temVideo");
                //Application.DoEvents();
                //DataTable dt = ds.Tables[0];
                DataTable dt = this.getLoadData(_startNo, _endNo, sql);
                this.toolEditPro.Value = 2;
                dataGridViewVideo.DataSource = dt;
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                dataGridViewVideo.Columns[5].Visible = false;
                dataGridViewVideo.Columns[6].Visible = false;      // mapid����ʾ
                //dataGridViewVideo.Columns[4].ReadOnly = true;
                //dataGridViewVideo.Columns[3].ReadOnly = true;
                //insertQueryIntoTable(dt);
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnVlocNo_Click");
                isShowPro(false);
            }
        }

        //////////////////------�������Զ�ƥ�����(add by lili in 2010-5-28)-----////////////////

        /// <summary>
        /// �Զ���ȫ����(add by LiLi in 2010-5-21)
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

                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
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
                writeToLog(ex, "getListBox");
                return null;
            }
        }

        /// <summary>
        /// ��Ϊ�̶�ֵʱ�Զ����(add by LiLi in 2010-5-21)
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

                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                    }
                    else
                        dt = null;
                }
                return dt;
            }
            catch (Exception ex)
            {
               writeToLog(ex, "MatchShu");
                return null;
            }
        }

        // �������ݱ༭ƥ��
        private void textKeyWord_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = null;
                switch (comboTables.Text)
                {
                    case "�����ɳ���":
                        dt = getListBox(this.textKeyWord.Text.Trim(), "�����ɳ���", this.comboTables.Text);
                        break;
                    case "�������ж�":
                        dt = getListBox(this.textKeyWord.Text.Trim(), "�����ж�", this.comboTables.Text);
                        break;
                    case "����������":
                        dt = getListBox(this.textKeyWord.Text.Trim(), "����������", this.comboTables.Text);
                        break;
                }
                if (dt != null)
                    textKeyWord.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textKeyWord_TextChanged_1");
            }
        }

        // �������ݱ༭ƥ��
        private void textKeyWord_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = null;
                switch (comboTables.Text)
                {
                    case "�����ɳ���":
                        dt = MatchShu("�����ɳ���", this.comboTables.Text);
                        break;
                    case "�������ж�":
                        dt = MatchShu("�����ж�", this.comboTables.Text);
                        break;
                    case "����������":
                        dt = MatchShu("����������", this.comboTables.Text);
                        break;
                } 
                if (dt != null)
                    textKeyWord.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textKeyWord_Click_1");
            } 
        }

        private void textKeyWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                searchByKey();
            }
        }

        // ��Ƶ�༭ƥ��
        private void ValueStr_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.ValueStr.Text.Trim(), "��Ƶλ��", this.FieldStr.Text);
                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ValueStr_TextChanged");
            }
        }

        // ��Ƶ�༭ƥ��
        private void ValueStr_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.FieldStr.Text,"��Ƶλ��");
                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ValueStr_Click");
            }
        }

        // ҵ�����ݱ�ƥ��
        private void textValue_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.textValue.Text.Trim(), this.comboField.Text, this.comboTable.Text);
                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textValue_TextChanged_1");
            }
        }

        // ҵ�����ݱ�ƥ��
        private void textValue_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.comboField.Text, this.comboTable.Text);
                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textValue_Click_1");
            }
        }

        /// <summary>
        /// ���ػ���ʾ������
        /// </summary>
        /// <param name="falg">����ֵ</param>
        private void isShowPro(bool falg)
        {
            try {
                this.toolEditPro.Value = 0;
                this.toolEditPro.Maximum = 3;
                this.toolEditPro.Visible = falg;
                this.toolEditProLbl.Visible = falg;
                this.toolEditProSep.Visible = falg;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "isShowPro");
            }
        }

        // ������Ԫ��
        private void dataGridViewGaInfo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int rowIn = dataGridViewGaInfo.Rows.Count - 1;
                if (e.RowIndex == rowIn && comboTable.Text == "��ȫ������λ")
                {
                    if (dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value.ToString() != "����༭") return;

                    if (this.frmZL != null)
                    {
                        if (this.frmZL.Visible == true)
                        {
                            this.frmZL.Close();
                        }
                    }
                    string DWMC = "";
                    for (int j = 0; j < this.dataGridViewGaInfo.Rows.Count; j++)
                    {
                        if (dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString() == "��λ����")
                        {
                            DWMC = dataGridViewGaInfo.Rows[j].Cells[1].Value.ToString();
                        }
                    }
                    if (DWMC == "")
                    {
                        MessageBox.Show("���Ʋ���Ϊ�գ�", "��ʾ");
                        return;
                    }
                    this.frmZL = new FrmZLMessage(DWMC, mysqlstr, this.temEditDt);

                    //������Ϣ�������½�
                    System.Drawing.Point p = this.PointToScreen(mapControl1.Parent.Location);
                    this.frmZL.SetDesktopLocation(mapControl1.Width - frmZL.Width + p.X, mapControl1.Height - frmZL.Height + p.Y + 25);
                    this.frmZL.Show();
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaInfo_CellClick");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;

                switch (link.Text)
                {
                    case "����������":
                        if (tabControl2.SelectedTab == tabPage1)
                        {
                            this.textKeyWord.Visible = false;
                            groupBox3.Visible = false;
                        } 
                        if (tabControl2.SelectedTab == tabPage2)
                        {
                            this.ValueStr.Visible = false;
                            this.groupBox1.Visible = false;
                        }
                        if (tabControl2.SelectedTab == tabPage3)
                        {
                            this.textValue.Visible = false;
                            groupBox2.Visible = false;
                        }
                        link.Text = "��ʾ������";
                        break;
                    case "��ʾ������":
                        if (tabControl2.SelectedTab == tabPage1)
                        {
                            this.textKeyWord.Visible = true;
                            groupBox3.Visible = true;
                        }
                        if (tabControl2.SelectedTab == tabPage2)
                        {
                            this.ValueStr.Visible = true;
                            this.groupBox1.Visible = true;
                        }
                        if (tabControl2.SelectedTab == tabPage3)
                        {
                            this.textValue.Visible = true;
                            groupBox2.Visible = true;
                        }
                        link.Text = "����������";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LinklblHides_LinkClicked");
            }
        }

        // ��ȡ���ű���
        private double getScale()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(ZoomFile);
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("������", "���ű���"));
                return dou;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LBSgisPoliceEdit-getScale");
                return 0;
            }
        }

        private void timerGPS_Tick(object sender, EventArgs e)
        {
            try
            {
                DelLayer();
                GetPolice(policeNo);
            }
            catch (Exception ex)
            {
                this.timerGPS.Stop();
                writeToLog(ex, "timerGPS_Tick");
            }
        }

        // GPS��Ա������ѡ�����겢�л����༭��ʽ
        private void udToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                setEditXY(policeNo);
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;
                this.tabControl3.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "timerGPS_Tick");
            }
        }

        /// <summary>
        /// ���ݾ�Ա�ı�Ž���Ա�����긳ֵ����ѡ��ҵ������
        /// </summary>
        /// <param name="policeNo">��Ա�ı��</param>
        private void setEditXY(string policeNo)
        {
            try
            {
                if (dataGridViewGaList.CurrentRow == null)
                {
                    MessageBox.Show("�б��������ݣ��޷�ʵ�ֹ���������", "����", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                string sql = "Select X,Y from GPS��Ա where �������='" + policeNo + "'";
                DataTable table = new DataTable();
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

                // ������Ϣ�б������
                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                {
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = table.Rows[0]["X"].ToString();
                        continue;
                    }
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = table.Rows[0]["Y"].ToString();
                        continue;
                    }
                }

                double dX = 0, dY = 0;
                try
                {
                    dX = Convert.ToDouble(table.Rows[0]["X"]);
                    dY = Convert.ToDouble(table.Rows[0]["Y"]);

                    string tabName = comboTable.Text;
                    if (tabControl2.SelectedTab == tabPage2)
                    {
                        tabName = "��Ƶ";
                    }
                    string upstr = "";   // ���µ�SQL
                    CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                    string sName = CLC.ForSDGA.GetFromTable.ObjName;
                    string strID = CLC.ForSDGA.GetFromTable.ObjID;
                    string tableName = CLC.ForSDGA.GetFromTable.TableName;
                    if (tableName == "�˿�ϵͳ" || tableName == "������Ϣ" || tableName == "�����ݷ���ϵͳ")
                    {
                        upstr = "update " + tableName + " t set t.geoloc.SDO_POINT.X=" + dX + ",t.geoloc.SDO_POINT.Y=" + dY + " where t." + strID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[strID].Value.ToString() + "'";
                    }
                    else 
                    {
                        upstr = "update " + CLC.ForSDGA.GetFromTable.TableName + " t set t.X=" + dX + ",t.Y=" + dY + " where t." + strID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[strID].Value.ToString() + "'";
                    }
                    CLC.DatabaseRelated.OracleDriver.OracleComRun(upstr);

                    // ��ɾ��֮ǰ�ĵ�
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["MAPID"].Value));
                    si.QueryDefinition.Columns = null;
                    Feature f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                    if (f != null)
                    {
                        this.editTable.DeleteFeature(f);
                    }

                    // ����Ӹ��µ�
                    if (dX > 0 && dY > 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(this.editTable)).CoordSys, dX, dY);

                        Feature pFeat = new Feature(editTable.TableInfo.Columns);

                        pFeat.Geometry = pt;
                        pFeat.Style = featStyle;
                        pFeat["MapID"] = this.dataGridViewGaList.Rows[rowIndex].Cells["MAPID"].Value.ToString();
                        pFeat["keyID"] = this.dataGridViewGaList.Rows[rowIndex].Cells[strID].Value.ToString();
                        pFeat["Name"] = this.dataGridViewGaList.Rows[rowIndex].Cells[sName].Value.ToString();
                        editTable.InsertFeature(pFeat);
                    }

                    // ���²�ѯ�б����������
                    this.dataGridViewGaList.Rows[rowIndex].Cells["X"].Value = dX;
                    this.dataGridViewGaList.Rows[rowIndex].Cells["Y"].Value = dY;
                    MessageBox.Show("��������ɹ���","��ʾ");
                }
                catch
                {
                    MessageBox.Show("�������ʧ�ܣ�", "��ʾ");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setEditXY");
            }
        }

        // ��ȡGPS��Աˢ��ʱ��
        private double getGPSTime()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("GPS��Ա", "����Ƶ��"));
                return dou * 1000;      // ������ת������
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getGPSTime");
                return 0;
            }
        }

        private void bindingNavigatorVideo_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.tstbPre.Text);
                if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageVideoCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        if (_endNo == vMax)
                        {
                            pageVideoCurrent--;
                            vCurrent = perPageSize * (pageVideoCurrent - 1);
                            _startNo = vMax - (vMax % countShu) + 1 - countShu;
                            _endNo = vMax - (vMax % countShu);
                        }
                        else
                        {
                            pageVideoCurrent--;
                            vCurrent = pageSize * (pageVideoCurrent - 1);
                            _startNo -= countShu;
                            _endNo -= countShu;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageVideoCurrent > pageVideoCount - 1)
                    {
                        MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageVideoCurrent++;
                        vCurrent = perPageSize * (pageVideoCurrent - 1);
                        _startNo += countShu;
                        _endNo += countShu;
                    }
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageVideoCurrent <= 1)
                    {
                        MessageBox.Show("�Ѿ�����ҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageVideoCurrent = 1;
                        vCurrent = 0;
                        _endNo = countShu;
                        _startNo = 1;
                    }
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageVideoCurrent > pageVideoCount - 1)
                    {
                        MessageBox.Show("�Ѿ���βҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageVideoCurrent = pageVideoCount;
                        vCurrent = perPageSize * (pageVideoCurrent - 1);
                        _endNo = vMax;
                        _startNo = vMax - (vMax % countShu) + 1;
                    }
                }
                else
                {
                    return;
                }

                isShowPro(true);
                this.tstNow.Text = pageVideoCurrent.ToString();//���õ�ǰҳ

                //���´����������datagridview��feng��
                this.Cursor = Cursors.WaitCursor;
                dataGridViewVideo.Columns.Clear();
                //DataTable datatable = Ga_exportDT = LoadData(PageSQL); //��ȡ��ǰҳ����
                DataTable datatable = getLoadData(_startNo, _endNo, videoSQL); //��ȡ��ǰҳ����; lili 2010-8-26
                dataGridViewVideo.DataSource = datatable;
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                //if (dataGridViewVideo.Columns["mapid"] != null)
                //{
                //    dataGridViewVideo.Columns["mapid"].Visible = false;
                //}
                //dataGridViewList.Visible = true;
                //����gridview�ļ����ɫ
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;
                //Application.DoEvents();

                this.insertQueryIntoTable(datatable);
                this.Cursor = Cursors.Default;
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "bindingNavigatorVideo_ItemClicked()");
                MessageBox.Show(ex.Message, "bindingNavigatorVideo_ItemClicked()");
                this.Cursor = Cursors.Default;
            }
        }

        private void tstNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                try
                {
                    if (Convert.ToInt32(this.tstNow.Text) < 1 || Convert.ToInt32(this.tstNow.Text) > pageVideoCount)
                    {
                        MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.tstNow.Text = pageVideoCurrent.ToString();
                        return;
                    }
                    else
                    {
                        isShowPro(true);
                        this.pageVideoCurrent = Convert.ToInt32(this.tstNow.Text);
                        vCurrent = perPageSize * (pageVideoCurrent - 1);
                        _startNo = ((pageVideoCurrent - 1) * perPageSize) + 1;
                        _endNo = _startNo + perPageSize - 1;

                        //���´����������datagridview��feng��
                        this.Cursor = Cursors.WaitCursor;
                        dataGridViewVideo.Columns.Clear();
                        DataTable datatable = getLoadData(_startNo, _endNo,videoSQL); //��ȡ��ǰҳ����
                        dataGridViewVideo.DataSource = datatable;
                        this.toolEditPro.Value = 1;
                        Application.DoEvents();

                        if (dataGridViewVideo.Columns["mapid"] != null)
                        {
                            dataGridViewVideo.Columns["mapid"].Visible = false;
                        }
                        //dataGridViewList.Visible = true;
                        //����gridview�ļ����ɫ
                        for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                        {
                            if (i % 2 == 1)
                            {
                                dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                            }
                            else
                            {
                                dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                            }
                        }
                        this.toolEditPro.Value = 2;
                        //Application.DoEvents();

                        this.insertQueryIntoTable(datatable);
                        this.Cursor = Cursors.Default;
                        this.toolEditPro.Value = 3;
                        Application.DoEvents();
                        isShowPro(false);
                    }
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "tstNow_KeyPress()");
                }
            }            
        }

        private void tstbPre_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r" && this.dataGridViewVideo.DataSource != null)
            {
                try
                {
                    isShowPro(true);
                    this.Cursor = Cursors.WaitCursor;
                    this.perPageSize = Convert.ToInt32(this.tstbPre.Text);
                    this.pageVideoCount = (vMax / perPageSize);//�������ҳ��
                    if ((vMax % perPageSize) > 0) pageVideoCount++;
                    this.bnCount.Text = "/" + pageVideoCount.ToString();//������ҳ��
                    if (vMax != 0)
                    {
                        this.pageVideoCurrent = 1;
                        this.tstNow.Text = pageVideoCurrent.ToString();//���õ�ǰҳ
                    }
                    this.vCurrent = 0;       //��ǰ��¼����0��ʼ
                    _endNo = perPageSize;
                    _startNo = 1;

                    //���´����������datagridview��feng��
                    this.Cursor = Cursors.WaitCursor;
                    dataGridViewVideo.Columns.Clear();
                    DataTable datatable = getLoadData(_startNo, _endNo,videoSQL); //��ȡ��ǰҳ����
                    dataGridViewVideo.DataSource = datatable;
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();

                    if (dataGridViewVideo.Columns["mapid"] != null)
                    {
                        dataGridViewVideo.Columns["mapid"].Visible = false;
                    }
                    //dataGridViewList.Visible = true;
                    //����gridview�ļ����ɫ
                    for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                        else
                        {
                            dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                        }
                    }
                    this.toolEditPro.Value = 2;
                    //Application.DoEvents();

                    this.insertQueryIntoTable(datatable);
                    this.Cursor = Cursors.Default;
                    this.toolEditPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        // �˷�������ҵ�����ݲ鿴���һ���µ�����     edit by lili in 2010-12-17
        private void btnMonth_Click(object sender, EventArgs e)
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                this.Cursor = Cursors.WaitCursor;
                isShowPro(true);
                string powerLimit = "";
                DateTime frontTime = DateTime.Now;
                DateTime time = frontTime.AddDays(-30);
                _first = Convert.ToInt32(this.toolStripTextBox1.Text);
                _end = 1;

                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                string biaoName = CLC.ForSDGA.GetFromTable.TableName;

                string sqlStr = "";
                if (biaoName == "������Ϣ")
                {
                    sqlStr = "select count(*) from " + biaoName + " where ����ʱ���ֵ >= to_date('" + time + "', 'YYYY-MM-DD HH24:MI:SS') " +
                             "and ����ʱ���ֵ <= to_date('" + frontTime + "', 'YYYY-MM-DD HH24:MI:SS')";
                }
                else
                {
                    sqlStr = "select count(*) from " + biaoName + " where ��ȡ����ʱ�� >= to_date('" + time + "', 'YYYY-MM-DD HH24:MI:SS') " +
                             "and ��ȡ����ʱ�� <= to_date('" + frontTime + "', 'YYYY-MM-DD HH24:MI:SS')";
                }


                if (strRegion != "˳����" && strRegion != "")     // Ȩ�޴���
                {
                    if (Array.IndexOf(strRegion.Split(','), "����") > -1)
                    {
                        strRegion = strRegion.Replace("����", "����,��ʤ");
                    }
                    powerLimit += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";

                }
                if (strRegion1 != "" && (biaoName == "������Ϣ" || biaoName == "��������"))
                {
                    if (powerLimit.IndexOf("and") > -1)
                    {
                        powerLimit = powerLimit.Remove(powerLimit.LastIndexOf(")"));
                        powerLimit += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        powerLimit += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                
                this.toolEditPro.Value = 1;
                Application.DoEvents();
                sqlStr = powerLimit == string.Empty ? sqlStr : sqlStr + " and " + powerLimit;

                conn.Open();
                OracleCommand cmd = new OracleCommand(sqlStr, conn);
                OracleDataAdapter orada = new OracleDataAdapter(cmd);
                DataTable _ds = new DataTable();
                orada.Fill(_ds);
                cmd.Dispose();
                orada.Dispose();
                conn.Close();
                if (Convert.ToInt32(_ds.Rows[0][0]) < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("���һ�����޼�¼��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.RecordCount.Text = "0��";
                    this.PageNow.Text = "0";//���õ�ǰҳ
                    this.lblPageCount.Text = "/ {0}";//������ҳ��
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    return;
                }

                //���´����������÷�ҳ
                try
                {
                    nMax = Convert.ToInt32(_ds.Rows[0][0]);
                    this.RecordCount.Text = nMax.ToString() + "��";
                    pageSize = Convert.ToInt32(this.toolStripTextBox1.Text);      //����ҳ������
                    pageCount = (nMax / pageSize);//�������ҳ��
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//������ҳ��
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    if (nMax != 0)
                    {
                        pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//���õ�ǰҳ
                    }
                    nCurrent = 0;       //��ǰ��¼����0��ʼ
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "btnMonth_Click ��ȡ��ҳ����");
                    MessageBox.Show(ex.Message, "btnMonth_Click ��ȡ��ҳ����");
                }

                string _whereSql = "";
                if (biaoName == "������Ϣ")
                    _whereSql = " and " + sqlStr.Substring(sqlStr.IndexOf("����ʱ���ֵ"));
                else
                    _whereSql = " and " + sqlStr.Substring(sqlStr.IndexOf("��ȡ����ʱ��"));

                _mySQL = _whereSql;
                this.toolEditPro.Value = 2;
                Application.DoEvents();

                DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, biaoName); //��ȡ��ǰҳ����
                this.dataGridViewGaList.DataSource = datatable;

                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                GasetDataGridBG();

                //����gridview�ļ����ɫ
                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//���datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                }

                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "LBSgisPoliceEdit--btnMonth_Click");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            this.Cursor = Cursors.Default;
        }
    }
}