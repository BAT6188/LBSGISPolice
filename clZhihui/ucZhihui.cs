using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Styles;
using MapInfo.Windows.Dialogs;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Geometry ;
using System.Net.Sockets ;
using MapInfo.Tools;
using System.Collections;
using System.IO;

namespace clZhihui
{
    public partial class ucZhihui :UserControl
    {
        private MapInfo.Windows.Controls.MapControl mapControl1;
        private string[] StrCon;                  // ���ݿ������ַ���
        private string strConn;                   // ���ݿ������ַ���
        public string StrRegion;                  // �ɳ���Ȩ�� 
        public string StrRegion1 = "";            // �ж�Ȩ��
        public string User = "";                  // �û���½���� 
        public string GetFromNamePath = string.Empty; // GetNameFromPath ���ļ�λ��

        public System.Data.DataTable dtExcel;     // ��ͼ����������

        private static int _videoPort;            // ͨѶ�˿�
        private static string[] _videoString;     // ��Ƶ�����ַ�
        private static ToolStripLabel _st;        // ״̬��
        private static NetworkStream _ns;         //
        private static ToolStripDropDownButton _tdb; // �������˵�
        private  Boolean _vf;                     // ͨѶ�Ƿ��Ѿ����ӵı�ʶ 
        private static string _vEpath = string.Empty;// ��Ƶ��ؿͻ�������·����

        public ToolStripProgressBar toolPro;      // ���ڲ�ѯ�Ľ�������lili 2010-8-10
        public ToolStripLabel toolProLbl;         // ������ʾ�����ı���
        public ToolStripSeparator toolProSep;
        public Panel panError;                    // ���ڵ���������ʾ
        public string plicNo = string.Empty;      // ���ڴ洢��110����ֵ  lili 2010-11-25

        /// <summary>
        /// ģ���ʼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="m">��ͼ�ؼ�</param>
        /// <param name="s">�������ݿ����</param>
        /// <param name="tools"></param>
        /// <param name="td"></param>
        /// <param name="port"></param>
        /// <param name="vs"></param>
        /// <param name="videopath"></param>
        public ucZhihui(MapInfo.Windows.Controls.MapControl m, string[] s, ToolStripLabel tools, ToolStripDropDownButton td, int port, string[] vs, string videopath,string getfromnamepath)
        {
            InitializeComponent();
            try
            {
                mapControl1 = m;
                StrCon = s;
                strConn = "data source =" + StrCon[0] + ";user id =" + StrCon[1] + ";password=" + StrCon[2];
                _tdb = td;
                _videoPort = port;
                _videoString = vs;
                _st = tools;
                _vEpath = videopath;
                GetFromNamePath = getfromnamepath;

                SetParmeterAnsEvents();

                comboBoxField.Text = comboBoxField.Items[0].ToString();
            
                this.comboBoxField.Items.Clear();
                this.comboBoxField.Items.Add("�������");
                this.comboBoxField.Items.Add("��������");
                this.comboBoxField.Items.Add("����_����");
                this.comboBoxField.Items.Add("�����ص���ַ");
                this.comboBoxField.Items.Add("�����ɳ���");
                this.comboBoxField.Items.Add("�����ж�");
                this.comboBoxField.Items.Add("����������");
                this.comboBoxField.Text = this.comboBoxField.Items[0].ToString();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "01-ģ���ʼ��");
            }
        }

        /// <summary>
        /// ���Ȧѡ����ѡ����ѡ�����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void SetParmeterAnsEvents()
        {
            try
            {
                mapControl1.Tools.Used += Tools_Used;
                mapControl1.Tools.FeatureAdded += Tools_FeatureAdded;
                mapControl1.Tools.FeatureSelected += Tools_FeatureSelected;

                this.mapControl1.Tools.Add("SelByRect", new CustomRectangleMapTool(true, true, true, mapControl1.Viewer,mapControl1.Handle.ToInt32(), 
                                                                                                     mapControl1.Tools, mapControl1.Tools.MouseToolProperties,
                                                                                                     mapControl1.Tools.MapToolProperties));
                this.mapControl1.Tools.Add("SelByCircle", new CustomCircleMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), 
                                                                                                    mapControl1.Tools, mapControl1.Tools.MouseToolProperties,
                                                                                                    mapControl1.Tools.MapToolProperties));
                this.mapControl1.Tools.Add("SelByPolygon", new CustomPolygonMapTool(true, true, true, mapControl1.Viewer, mapControl1.Handle.ToInt32(), 
                                                                                                      mapControl1.Tools, mapControl1.Tools.MouseToolProperties,
                                                                                                      mapControl1.Tools.MapToolProperties));
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetParmeterAnsEvents");
            }
        }

        /// <summary>
        /// ��ʼ��ֱ��ָ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public void InitZhihui()
        {
            try
            {
                CreateTemLayer("ֱ��ָ��");
                SetLayerEdit("ֱ��ָ��");
                CreateTemLayer("�鿴ѡ��");
                SetDrawStyle();
                if (tabControl1.SelectedTab == tabYuan)
                {
                    RefreshYuan();
                }

                // �ж�plicNo�Ƿ���ֵ���Ӷ����ж��Ƿ��Ǵ�110��������
                if (this.plicNo != string.Empty)
                {
                    this.comboBoxField.Text = this.comboBoxField.Items[0].ToString();
                    this.CaseSearchBox.Text = this.plicNo;
                    caseSearch();
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "InitZhihui-��ʼ��ֱ��ָ��");
            }
         
        }


        /// <summary>
        /// ������ʱͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableAiles"></param>
        private void CreateTemLayer(string tableAiles)
        {
            try
            {
                //   create   a   temp   layer   as   the   rectangle   holder 
                TableInfoMemTable ti = new TableInfoMemTable(tableAiles);
                ti.Temporary = true;

                //   add   columns 
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
                column.Alias = "��_ID";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "����";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "����";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                Table table;
                try
                {
                    //   create   table   and   feature   layer 
                    table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                }
                catch
                {
                    table = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);
                }
                FeatureLayer temLayer = new FeatureLayer(table);

                mapControl1.Map.Layers.Insert(0, temLayer);
            }
            catch (Exception ex) { ExToLog(ex, "CreateTemLayer"); }
        }

        /// <summary>
        /// ���õ�ǰͼ��ɱ༭
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="layername"></param>
        private void SetLayerEdit(string layername)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layername], true);
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layername], true);
            }
            catch (Exception ex) { ExToLog(ex, "SetLayerEdit"); }
        }

        /// <summary>
        /// ͼԪ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        void Tools_FeatureAdded(object sender, MapInfo.Tools.FeatureAddedEventArgs e)
        {
            if (!this.Visible)
            {
            }
            else
            {
                try
                {
                    Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("ֱ��ָ��",
                                                                                         MapInfo.Data.SearchInfoFactory.
                                                                                             SearchWhere(
                                                                                                 "MapID is null or MapID=0 "));
                    switch (e.Feature.Type)
                    {
                        case MapInfo.Geometry.GeometryType.Point:
                            if (tabControl1.SelectedTab == tabYuan)
                            {
                                planX = e.MapCoordinate.x;
                                planY = e.MapCoordinate.y;
                                planLayer();
                                clearFeatures("ֱ��ָ��");
                            }
                            else
                                ft.Style = pStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.MultiCurve:
                            ft.Style = lStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.MultiPolygon:
                            ft.Style = aStyle;
                            break;
                        case MapInfo.Geometry.GeometryType.LegacyText:
                            ft.Style = tStyle;
                            break;
                    }
                    ft["MapID"] = 1;
                    ft.Update();
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Tools_FeatureAdded-ͼԪ���");
                }
            }
        }


        private Table planTable;
        /// <summary>
        /// ����Ԥ��ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void planLayer()
        {
            try
            {
                if (planTable != null)
                {
                    planTable.Close();
                }

                TableInfoMemTable ti = new TableInfoMemTable("Ԥ��ͼ��");
                ti.Temporary = true;

                //   add   columns   
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
                column.Alias = "Ԥ������";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                planTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);

                double dx = 0, dy = 0;

                dx = planX;
                dy = planY;

                if (dx > 0 || dy > 0)
                {
                    FeatureGeometry pt = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), dx, dy);

                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, System.Drawing.Color.Red, 9));
                    cs = new CompositeStyle(new SimpleVectorPointStyle(34, Color.Red, 9));

                    Feature pFeat = new Feature(planTable.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["Ԥ������"] = this.textPlan.Text;
                    planTable.InsertFeature(pFeat);
                }

                currentFeatureLayer = new FeatureLayer(planTable, "Ԥ��ͼ��");

                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                labeLayer(planTable, "Ԥ������");
                try
                {
                    DPoint dP = new DPoint(dx, dy);
                    CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();

                    mapControl1.Map.SetView(dP, cSys, 6000);
                    mapControl1.Map.Center = dP;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "�趨��ͼ��Χ");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "planLayer");
            }
        }

        AreaStyle aStyle;
        BaseLineStyle lStyle;
        TextStyle tStyle;
        BasePointStyle pStyle;

        /// <summary>
        /// ���û���Ҫ�ص�Ĭ����ʽ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public void SetDrawStyle()
        {
            try
            {
                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2.5, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(1);
                aStyle = new AreaStyle(simLineStyle, simInterior);
                lStyle = new SimpleLineStyle(new LineWidth(2.5, MapInfo.Styles.LineWidthUnit.Point), 59, System.Drawing.Color.Red);
                tStyle = new TextStyle(new MapInfo.Styles.Font("����", 16.0, Color.Red, Color.Transparent, FontFaceStyle.Normal, FontWeight.Bold, TextEffect.None, TextDecoration.None, TextCase.Default, false, false));
                //pStyle = new SimpleVectorPointStyle(69, Color.Blue, 12);
                pStyle = new BitmapPointStyle("jc.bmp", BitmapStyles.None, Color.Blue, 14);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SetDrawStyle-���û���Ҫ�ص�Ĭ����ʽ");
            }
        }

        private bool isDel;
        /// <summary>
        /// ��ͼ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void mapToolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                UncheckedTool();
                isDel = false;
                switch (e.Button.Name)
                {
                    case "toolAddPoint":
                        mapControl1.Tools.LeftButtonTool = "AddPoint";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddPolyline":
                        mapControl1.Tools.LeftButtonTool = "AddPolyline";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddPolygon":
                        mapControl1.Tools.LeftButtonTool = "AddPolygon";
                        e.Button.Pushed = true;
                        break;
                    case "toolAddText":
                        mapControl1.Tools.LeftButtonTool = "AddText";
                        e.Button.Pushed = true;
                        break;
                    case "toolPointStyle":
                        SymbolStyleDlg pStyleDlg = new SymbolStyleDlg();
                        pStyleDlg.SymbolStyle = pStyle;
                        if (pStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            pStyle = pStyleDlg.SymbolStyle;
                        }
                        break;
                    case "toolLineStyle":
                        LineStyleDlg lStyleDlg = new LineStyleDlg();
                        lStyleDlg.LineStyle = lStyle;
                        if (lStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            lStyle = lStyleDlg.LineStyle;
                        }
                        break;
                    case "toolAreaStyle":
                        AreaStyleDlg aStyleDlg = new AreaStyleDlg();
                        aStyleDlg.AreaStyle = aStyle;
                        if (aStyleDlg.ShowDialog() == DialogResult.OK)
                        {
                            aStyle = aStyleDlg.AreaStyle;
                        }
                        break;
                    case "toolTextStyle":
                        TextStyleDlg tStylrDlg = new TextStyleDlg();
                        tStylrDlg.FontStyle = tStyle.Font;
                        if (tStylrDlg.ShowDialog() == DialogResult.OK)
                        {
                            tStyle = new TextStyle(tStylrDlg.FontStyle);
                        }
                        break;
                    case "toolButtonDel":
                        mapControl1.Tools.LeftButtonTool = "Select";
                        e.Button.Pushed = true;
                        isDel = true;
                        break;
                    case "toolBarRect":
                        mapControl1.Tools.LeftButtonTool = "SelByRect";
                        e.Button.Pushed = true;
                        break;
                    case "toolBarCircle":
                        mapControl1.Tools.LeftButtonTool = "SelByCircle";
                        e.Button.Pushed = true;
                        break;
                    case "toolBarPolygon":
                        mapControl1.Tools.LeftButtonTool = "SelByPolygon";
                        e.Button.Pushed = true;
                        break;
                }

            }
            catch (Exception ex)
            {
                ExToLog(ex, "mapToolBar1_ButtonClick-��ͼ����");
            }
            finally { mapControl1.Map.Center = mapControl1.Map.Center; }
        }

        /// <summary>
        /// ����������ϵĹ���ʱ���Թ��߰�ť�������ã����ڰ�ť���飬iFrom��ʾ�����Index��iEnd��ʾĩIndex
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void UncheckedTool()
        {
            try
            {
                for (int i = 0; i < 12; i++)
                {
                    if (mapToolBar1.Buttons[i].Pushed)
                    {
                        mapToolBar1.Buttons[i].Pushed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "UncheckedTool-05-���߰�ť��������");
            }
        }

  
        /// <summary>
        /// ͼԪѡ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void Tools_FeatureSelected(object sender, MapInfo.Tools.FeatureSelectedEventArgs e)
        {
            if (isDel == false) return;
            if (!this.Visible)
            {
                return;
            }
            else
            {
                try
                {
                    FeatureLayer fLayer = mapControl1.Map.Layers["ֱ��ָ��"] as FeatureLayer;
                    Table table = fLayer.Table;

                    //IResultSetFeatureCollection fc= MapInfo.Engine.Session.Current.Catalog.Search(table.Alias, MapInfo.Data.SearchInfoFactory.(e.Selection.Envelope,mapControl1.Map.FeatureCoordSys, ContainsType.Geometry));
                    IResultSetFeatureCollection fc = e.Selection[table];
                    foreach (Feature f in fc)
                    {
                        table.DeleteFeature(f);
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Tools_FeatureSelected-06-ͼԪѡ��");
                }
            }
        }

        /// <summary>
        /// ��ȡ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ns1">������</param>
        /// <param name="vf1">���ӳɹ���ʶ��</param>
        public void getNetParameter(NetworkStream ns1, Boolean vf1)
        {
            try
            {
                _ns = ns1;
                this._vf = vf1;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getNetParameter-��ȡ�������");
            }
        }

        int pageSize = 0;      // ÿҳ��ʾ����
        int nMax = 0;          // �ܼ�¼��
        int pageCount = 0;     // ҳ�����ܼ�¼��/ÿҳ��ʾ����
        int pageCurrent = 0;   // ��ǰҳ��
        int nCurrent = 0;      // ��ǰ��¼��

        //---------��ҳ��ȫ�ֱ���------
        int startNo = 1;   // ��ʼ������
        int endNo = 0;���� // ����������
        //----------------------------
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            caseSearch();
        }

        private frmPro pro = null;
        /// <summary>
        /// ģ����ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void caseSearch()
        {
            if (this.CaseSearchBox.Text == "")
            {
                MessageBox.Show(@"������ؼ���");
                return;
            }
            try
            {
                //isShowPro(true);
                pro = new frmPro();
                pro.Show();
                dataGridView1.Rows.Clear();
                string countSql = "select count(*) from GPS110.������Ϣ110 where GPS110.������Ϣ110." + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%'";
                getMaxCount(countSql);
                InitDataSet(RecordCount);

                //string strExpress = "select * from GPS110.������Ϣ110 where GPS110.������Ϣ110." + comboBoxField.Text + " like '%" + ToDBC(CaseSearchBox.Text) + "%' order by GPS110.������Ϣ110.������� desc";
                //DataTable dataTable = GetTable(strExpress); 

                endNo = Convert.ToInt32(this.TextNum.Text);
                startNo = 1;

                //LoadData(startNo, endNo)
                DataTable dataTable = LoadData(endNo, 1, true); 
                pro.progressBar1.Maximum = 3;
                pro.progressBar1.Value = 1;
                //this.toolPro.Value = 1;
                Application.DoEvents();

                if (dataTable == null || dataTable.Rows.Count < 1)
                {
                    pro.Close();
                    // isShowPro(false);
                    MessageBox.Show(@"�޲�ѯ�����");
                }
                else
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add(i + 1, dataTable.Rows[i]["�����ص���ַ"], "����...", dataTable.Rows[i]["�������"], dataTable.Rows[i]["X"], dataTable.Rows[i]["Y"],dataTable.Rows[i]["��������"]);
                        if (i % 2 == 1)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    //���ÿ��
                    setDataGridViewColumnWidth(dataGridView1);
                    insertQueryIntoMap(dataTable);
                    WriteEditLog("����ѡ��", "����ϵͳ", countSql, "��ѯ");
                }
                pro.progressBar1.Value = 2;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                }
                //this.toolPro.Value = 3;
                pro.progressBar1.Value = 3;
                Application.DoEvents();
                pro.Close();
                //isShowPro(false);
                //this.toolPro.Value = 2;
                //Application.DoEvents();
            }
            catch (Exception ex)
            {
                pro.Close();
                //isShowPro(false);
                ExToLog(ex, "caseSearch-08-ģ����ѯ");
            }         
        }

        /// <summary>
        /// ��ѯ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="_startNo">ÿҳ��ʾ��</param>
        /// <param name="_endNo">�ڼ�ҳ</param>
        /// <param name="isFrist">��ǰ����Ƿ��ǵ�һҳ</param>
        /// <returns>�����</returns>
        private DataTable LoadData(int xiShu, int yeShu,bool isFrist)
        {
            try
            {
                DataTable dt = new DataTable();
      
                string sql = "";
                if (isFrist)
                    sql = "select * from (select * from gps110.������Ϣ110 where " + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%' order by ������� desc) t where rownum <= " + xiShu;
                else
                    sql = "select * from (select w.*,rownum rn from gps110.������Ϣ110 w where rownum <" + yeShu + "*" + xiShu + " and " + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%' order by ������� desc) t where rn <= " + yeShu + "*" + xiShu + " and rn > (" + yeShu + "-1)*" + xiShu;

                WriteLog(sql);

                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

                PageNow.Text = Convert.ToString(pageCurrent);
                PageCount.Text = "/" + pageCount.ToString();

                if (dtExcel != null) dtExcel.Clear();
                dtExcel = dt;

                return dt;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "LoadData-��ѯ����");
                return null;
            }
        }

        /// <summary>
        /// ������־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-28
        /// </summary>
        /// <param name="sql">sql���</param>
        private void WriteLog(string sql)
        {
            StreamWriter strWri = null;
            try
            {
                string exePath = Application.StartupPath + "\\TestSqlLog.txt";
                strWri = new StreamWriter(exePath, true);
                strWri.WriteLine(sql);
                strWri.Dispose();
                strWri.Close();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "WriteLog");
            }
        }

        /// <summary>
        /// ��ѯ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="_startNo">��ʼ����</param>
        /// <param name="_endNo">��������</param>
        /// <returns>�����</returns>
        private DataTable LoadData(int _startNo,int _endNo)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from (select rownum as rn1,a.* from GPS110.������Ϣ110 a where rownum<=" + _endNo + " and a." + comboBoxField.Text + " like '%" + CaseSearchBox.Text + "%') where rn1 >=" + _startNo + " order by ������� desc";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                PageNow.Text = Convert.ToString(pageCurrent);
                PageCount.Text = "/" + pageCount.ToString();
                if (dtExcel != null) dtExcel.Clear();
                dtExcel = dt;
                return dt;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "LoadData-��ѯ����");
                return null;
            }
        }

        /// <summary>
        /// �õ�����ֵ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">sql���</param>
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
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nMax = 0;
            }
        }

        /// <summary>
        /// ��ʼ����ҳ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tsLabel">��ʾ��ǰ��¼����</param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                pageSize = Convert.ToInt32(this.TextNum.Text);      //����ҳ������
                TextNum.Text = pageSize.ToString();
                tsLabel.Text = nMax.ToString() + "��";              //�ڵ���������ʾ�ܼ�¼��
                pageCount = (nMax / pageSize);                      //�������ҳ��
                if ((nMax % pageSize) > 0) pageCount++;
                if (nMax != 0)
                {
                    pageCurrent = 1;
                }
                nCurrent = 0;       //��ǰ��¼����0��ʼ
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Table queryTable;
        FeatureLayer currentFeatureLayer;
        /// <summary>
        /// ����ѯ�����ʾ�ڵ�ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="datatable">Ҫ���������Դ</param>
        private void insertQueryIntoMap(DataTable datatable)
        {
            try
            {
                if (queryTable != null)
                {
                    queryTable.Close();

                }
                if (datatable == null)
                {
                    return;
                }
                TableInfoMemTable ti = new TableInfoMemTable("����ѡ��");
                ti.Temporary = true;

                //   add   columns   
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
                column.Alias = "��_ID";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "����";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                queryTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);

                double dx = 0, dy = 0;
                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    try
                    {
                        dx = Convert.ToDouble(datatable.Rows[i]["X"]);
                        dy = Convert.ToDouble(datatable.Rows[i]["Y"]);
                    }
                    catch
                    {
                        continue;
                    }
                    if (dx <= 0 || dy <= 0) continue;
                    FeatureGeometry pt = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), dx, dy);
                    CompositeStyle cs = new CompositeStyle(new SimpleVectorPointStyle(34, Color.Red, 9));

                    Feature pFeat = new Feature(queryTable.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["��_ID"] = datatable.Rows[i]["�������"].ToString();
                    pFeat["����"] = "����ϵͳ";
                    queryTable.InsertFeature(pFeat);
                }

                currentFeatureLayer = new FeatureLayer(queryTable, "����ѡ��");
                
                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                labeLayer(queryTable,"��_ID");
                //this.setLayerStyle(currentFeatureLayer, Color.Red, 34, 10);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "insertQueryIntoMap-09-����ѯ�����ʾ�ڵ�ͼ��");
                //writeToLog(ex,"insertQueryIntoMap");
            }
        }

        /// <summary>
        /// �޸�Ҫ�ص���ʾ��ʽ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="featureLayer">�޸�Ҫ��</param>
        /// <param name="color">��ɫ</param>
        /// <param name="code"></param>
        /// <param name="size">��С</param>
        private void setLayerStyle(MapInfo.Mapping.FeatureLayer featureLayer, Color color, short code, int size)//���õ����ʽ
        {
            try
            {
                MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(code, color, size);

                MapInfo.Styles.CompositeStyle comStyle = new MapInfo.Styles.CompositeStyle();
                comStyle.SymbolStyle = pStyle;
                MapInfo.Mapping.FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, comStyle);
                featureLayer.Modifiers.Append(fsm);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setLayerStyle-10-�޸�Ҫ�ص���ʾ��ʽ");
            }
        }

        /// <summary>
        /// ����ͼ���ע
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="editTable">ͼ������</param>
        /// <param name="field">��ע�ֶ�</param>
        private void labeLayer(Table editTable,string field)//������ע
        {
            try
            {
                LabelLayer labelLayer = mapControl1.Map.Layers["��עͼ��"] as LabelLayer;

                LabelSource source = new LabelSource(editTable);

                source.DefaultLabelProperties.Caption = field;
                source.DefaultLabelProperties.Layout.Offset = 4;
                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;
                //source.DefaultLabelProperties.Visibility.VisibleRangeEnabled = true;
                //source.DefaultLabelProperties.Visibility.VisibleRange = new VisibleRange(0.0, 10, DistanceUnit.Kilometer);
                labelLayer.Sources.Insert(0, source);
            }

            catch (Exception ex)
            {
                ExToLog(ex, "labeLayer-11-����ͼ���ע");
            }
        }

        //  ��Ŀ---˳�¹�������GISϵͳ
        //  ģ��---ֱ��ָ���е���Ƶ����ģ��
        //  zhangjie 2008.12.3        //  

       /// <summary>
        /// �ܱ߲�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
       /// </summary>
       /// <param name="dp">��λ��</param>
       /// <param name="distCar">������ѯ�뾶</param>
       /// <param name="carflag">������ѯ��ʶ</param>
       /// <param name="distVideo">��Ƶ��ѯ�뾶</param>
       /// <param name="videoflag">��Ƶ��ѯ��ʶ</param>
        private void SearchDistance(DPoint dp, double distCar, bool carflag,double distVideo, bool videoflag)
        {
            try
            {
                // ���� ��Ƶͼ��             
                if (videoflag)
                {
                    clVideo.ucVideo fv = new clVideo.ucVideo(mapControl1, _st, _tdb, StrCon, _videoPort, _videoString, _vEpath, true, false);
                    fv.getNetParameter(_ns, _vf);
                    fv.strRegion = this.StrRegion;
                    fv.SearchVideoDistance(dp, distVideo,"ֱ��ָ��");
                }

                // ��������ͼ��
                if (carflag)
                {
                    clCar.ucCar fcar = new clCar.ucCar(mapControl1, null, StrCon, true);
                    fcar.strRegion = StrRegion;
                    fcar.SearchCarDistance(dp, distCar);
                    fcar.ZhiHui = false;
                }

                if (!videoflag && !carflag)
                {
                    //return;
                }

                Distance dt = new Distance();
                if (distCar >= distVideo)
                {
                    dt = new Distance(distCar, DistanceUnit.Meter);
                }
                else
                {
                    dt = new Distance(distVideo, DistanceUnit.Meter);
                }

                this.mapControl1.Map.SetView(dp, this.mapControl1.Map.GetDisplayCoordSys(), dt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "SearchDistance-12-�ܱ߲�ѯ");
            }
        }

        /// <summary>
        /// �л�����ģ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.panError.Visible = false;    // ���ش�����ʾ

                if (tabControl1.SelectedTab == tabLiandong)
                {
                    this.panel3.Visible = false;
                }

                if (tabControl1.SelectedTab == tabYuan)
                {
                    if (dataGridView1.CurrentCell == null && this.panel3.Visible == false)
                    {
                        if (MessageBox.Show("��û��ѡ�񰸼������Ƿ�Ҫ�Զ��尸������Ԥ�ݣ�", "ϵͳ��ʾ", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.Cancel)
                        {
                            return;
                        }
                        else
                        {
                            this.panel3.Visible = true;
                        }
                    }
                    RefreshYuan();
                }
                if (tabControl1.SelectedTab != tabBaiban)
                {
                    isDel = false;
                    mapControl1.Tools.LeftButtonTool = "Select";
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "tabControl1_SelectedIndexChanged-13-�л�����ģ��");
            }
        }

        /// <summary>
        /// ����Ԥ���б�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void RefreshYuan()
        {
            this.dataGridView2.Rows.Clear();
            RunCommand("Update Ԥ�� set ID=rownum");
           try
            {
               
                string strExpress = "select * from Ԥ��";
                DataTable dataTable = GetTable(strExpress);
                if (dataTable.Rows.Count < 1 || dataTable == null)
                {
                    MessageBox.Show("�޲�ѯ�����");
                }
                else
                {
                    DataGridViewButtonCell btn = new DataGridViewButtonCell();
                    DataGridViewButtonCell btn2 = new DataGridViewButtonCell();
                    DataGridViewButtonCell btn3 = new DataGridViewButtonCell();
                    btn.ToolTipText = "���ö�ӦԤ��";
                    btn2.ToolTipText = "�޸�Ԥ������...";
                    btn3.ToolTipText = "ɾ����ǰԤ��";
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataGridView2.Rows.Add(dataTable.Rows[i]["ID"], dataTable.Rows[i]["����"], btn, btn2, btn3);
                        dataGridView2.Rows[i].Height = 30;
                    }

                    //���ÿ��
                    setDataGridViewColumnWidth(dataGridView2);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "RefreshYuan-14-����Ԥ���б�");
            }
       
            //���ÿ��
            setDataGridViewColumnWidth(dataGridView2);
        }


        /// <summary>
        /// ���Ԥ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            frmCustomYuAn fYuan = new frmCustomYuAn();
            fYuan.Text = @"�Զ���Ԥ��";
            fYuan.buttonAdd.Text = @"���";

            if (fYuan.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            else
            {
                try
                {
                    int[] disArr = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0};

                    for (int i = 0; i < fYuan.texts.Length; i++)
                    {
                        if (fYuan.texts[i].Text != "")
                        {
                            disArr[i] = Convert.ToInt32(ToDBC(fYuan.texts[i].Text));
                        }
                    }

                    string id = GetID();

                    string strExpress = "insert into Ԥ��(ID,����,������Χ,����ͷ��Χ,";
                    strExpress += "��Ա��Χ,�ΰ����ڷ�Χ,";
                    strExpress += "�����ɳ�����Χ,���������ҷ�Χ,����������Χ,";
                    strExpress += "���ɷ�Χ,��ȫ������λ��Χ,������ҵ��Χ,";
                    strExpress += "�����ص㵥λ��Χ,�������жӷ�Χ,����˨��Χ) values('" + id + "','" + ToDBC(fYuan.textYuanName.Text) + "'";
                    for (int i = 0; i < 13; i++)
                    {
                        strExpress += "," + disArr[i];
                    }
                    strExpress += ")";

                    RunCommand(strExpress);
                   
                    RefreshYuan();
                    WriteEditLog("Ԥ������", "Ԥ��", "����=" + ToDBC(fYuan.textYuanName.Text), "���Ԥ��");
                    MessageBox.Show(@"��ӳɹ�!");
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "buttonAdd_Click-15-���Ԥ��");
                }
            }
        }


        /// <summary>
        /// ����Ԥ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            #region ����Ԥ��

            if (dataGridView2.Columns[e.ColumnIndex].Name == "colAct")
            {
                mapControl1.Tools.LeftButtonTool = "Select";
                if (dataGridView1.CurrentCell != null)
                {
                    try
                    {
                        planX = Convert.ToDouble(dataGridView1["ColX", dataGridView1.CurrentCell.RowIndex].Value);
                        planY = Convert.ToDouble(dataGridView1["ColY", dataGridView1.CurrentCell.RowIndex].Value);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("�������겻��ȷ������ʵ��Ԥ�ݣ�", "��ʾ");
                        ExToLog(ex, "dataGridView2_CellContentClick");
                    }
                }
                int sIndex = e.RowIndex;
                startPlan(planX, planY, sIndex);
            }
            #endregion

            #region �鿴���޸�Ԥ��
            if (dataGridView2.Columns[e.ColumnIndex].Name == "colAlter")
            {
                mapControl1.Tools.LeftButtonTool = "Select";
                OracleConnection Conn = new OracleConnection(strConn);
                try
                {
                    Conn.Open();

                    //ѡ��Ԥ��

                    //checks = new CheckBox[12] { checkCar, checkVideo, checkJY, checkZAKK, checkPCS, checkJWS, checkGGCS, checkWB, checkAF, checkTZHY, checkXF, checkMJZD };
                    //texts = new TextBox[12] { textCarDis, textVideoDis, textJY, textZAKK, textPCS, textJWS, textGGCS, textWB, textAF, textTZHY, textXF, textMJZD };

                    string id;
                    string strExpress = "select t.����,t.������Χ,t.����ͷ��Χ,";
                    strExpress += "t.��Ա��Χ,t.�ΰ����ڷ�Χ,";
                    strExpress += "t.�����ɳ�����Χ,t.���������ҷ�Χ,t.����������Χ,";
                    strExpress += "t.���ɷ�Χ,t.��ȫ������λ��Χ,t.������ҵ��Χ,";
                    strExpress += "t.�����ص㵥λ��Χ,t.�������жӷ�Χ,t.����˨��Χ,";
                    strExpress += "t.ID from Ԥ�� t where t.ID='" + dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString() + "'";
                    OracleCommand cmd = new OracleCommand(strExpress, Conn);

                    OracleDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        id = dr["ID"].ToString();
                        frmCustomYuAn fYuan = new frmCustomYuAn();
                        fYuan.Text = "Ԥ���޸�";
                        fYuan.buttonAdd.Text = "����";
                        fYuan.textYuanName.Text = dr["����"].ToString();

                        for (int i = 1; i < 14; i++)
                        {
                            if (Convert.ToDouble(dr[i]) != 0)
                            {
                                fYuan.checks[i-1].Checked = true;
                                fYuan.texts[i-1].Text = dr[i].ToString();
                            }
                            else
                            {
                                fYuan.checks[i-1].Checked = false;
                            }
                        }

                        dr.Close();

                        if (fYuan.ShowDialog() == DialogResult.OK)
                        {
                            strExpress = "update Ԥ�� t set ";
                            strExpress += "����='" + ToDBC(fYuan.textYuanName.Text) + "',";

                            if (fYuan.checks[0].Checked)
                            {
                                strExpress += "������Χ=" + Convert.ToInt32(fYuan.texts[0].Text) + ",";
                            }
                            else
                            {
                                strExpress += "������Χ=0,";
                            }

                            if (fYuan.checks[1].Checked)
                            {
                                strExpress += "����ͷ��Χ=" + Convert.ToInt32(fYuan.texts[1].Text) + ",";
                            }
                            else
                            {
                                strExpress += "����ͷ��Χ=0,";
                            }

                            if (fYuan.checks[2].Checked)
                            {
                                strExpress += "��Ա��Χ=" + Convert.ToInt32(fYuan.texts[2].Text) + ",";
                            }
                            else
                            {
                                strExpress += "��Ա��Χ=0,";
                            }

                            if (fYuan.checks[3].Checked)
                            {
                                strExpress += "�ΰ����ڷ�Χ=" + Convert.ToInt32(fYuan.texts[3].Text) + ",";
                            }
                            else
                            {
                                strExpress += "�ΰ����ڷ�Χ=0,";
                            }

                            if (fYuan.checks[4].Checked)
                            {
                                strExpress += "�����ɳ�����Χ=" + Convert.ToInt32(fYuan.texts[4].Text) + ",";
                            }
                            else
                            {
                                strExpress += "�����ɳ�����Χ=0,";
                            }

                            if (fYuan.checks[5].Checked)
                            {
                                strExpress += "���������ҷ�Χ=" + Convert.ToInt32(fYuan.texts[5].Text) + ",";
                            }
                            else
                            {
                                strExpress += "���������ҷ�Χ=0,";
                            }

                            if (fYuan.checks[6].Checked)
                            {
                                strExpress += "����������Χ=" + Convert.ToInt32(fYuan.texts[6].Text) + ",";
                            }
                            else
                            {
                                strExpress += "����������Χ=0,";
                            }

                            if (fYuan.checks[7].Checked)
                            {
                                strExpress += "���ɷ�Χ=" + Convert.ToInt32(fYuan.texts[7].Text) + ",";
                            }
                            else
                            {
                                strExpress += "���ɷ�Χ=0,";
                            }

                            if (fYuan.checks[8].Checked)
                            {
                                strExpress += "��ȫ������λ��Χ=" + Convert.ToInt32(fYuan.texts[8].Text) + ",";
                            }
                            else
                            {
                                strExpress += "��ȫ������λ��Χ=0,";
                            }

                            if (fYuan.checks[9].Checked)
                            {
                                strExpress += "������ҵ��Χ=" + Convert.ToInt32(fYuan.texts[9].Text) + ",";
                            }
                            else
                            {
                                strExpress += "������ҵ��Χ=0,";
                            }

                            if (fYuan.checks[10].Checked)
                            {
                                strExpress += "�����ص㵥λ��Χ=" + Convert.ToInt32(fYuan.texts[10].Text) + ",";
                            }
                            else
                            {
                                strExpress += "�����ص㵥λ��Χ=0,";
                            }

                            if (fYuan.checks[11].Checked)
                            {
                                strExpress += "�������жӷ�Χ=" + Convert.ToInt32(fYuan.texts[11].Text);
                            }
                            else
                            {
                                strExpress += "�������жӷ�Χ=0,";
                            }

                            if (fYuan.checks[12].Checked)
                            {
                                strExpress += "����˨��Χ=" + Convert.ToInt32(fYuan.texts[12].Text);
                            }
                            else
                            {
                                strExpress += "����˨��Χ=0";
                            }

                            strExpress += " where ID='" + id + "'";

                            try
                            {
                                RunCommand(strExpress);
                                RefreshYuan();
                                MessageBox.Show("Ԥ�����ĳɹ�!");
                                WriteEditLog("Ԥ������", "Ԥ��", "����" + ToDBC(fYuan.textYuanName.Text), "�鿴���޸�Ԥ��");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("����ʧ��,��鿴log�ļ�,�޸�����");
                                ExToLog(ex, "16-�޸�Ԥ��");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "16-�鿴Ԥ��");
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                }
            }
#endregion

            #region ɾ��Ԥ��
            if (dataGridView2.Columns[e.ColumnIndex].Name == "ColDel")
            {
                mapControl1.Tools.LeftButtonTool = "Select";
                try
                {
                    string strExpress = "delete from Ԥ�� where ����='" + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() + "'";
                    RunCommand(strExpress);
                    RefreshYuan();
                    WriteEditLog("Ԥ������", "Ԥ��", "����" + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString(), "ɾ��Ԥ��");
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "16-ɾ��Ԥ��");
                }
            }
            #endregion
        }

        /// <summary>
        /// ������������Ԥ�����趨��Χ�ڵ�Ҫ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="pX">X����</param>
        /// <param name="pY">Y����</param>
        /// <param name="sIndex">��¼������</param>
        private void startPlan(double pX, double pY,int sIndex)
        {
            if (pX == 0 || pY == 0)
            {
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                double carDis = 0, videoDis = 0;
                bool checkCar = false, checkVideo = false;
                Conn.Open();

                //ѡ��Ԥ��
                string strExpress = "select * from Ԥ�� where ����='" + dataGridView2.Rows[sIndex].Cells[1].Value.ToString() + "'";
                OracleCommand cmd = new OracleCommand(strExpress, Conn);

                OracleDataReader dr = cmd.ExecuteReader();
                clearFeatures("ֱ��ָ��");
                StopVideo();
                if (dr.HasRows)
                {
                    dr.Read();
                    carDis = Convert.ToDouble(dr["������Χ"]);
                    videoDis = Convert.ToDouble(dr["����ͷ��Χ"]);

                    if (carDis != 0)
                    {
                        checkCar = true;
                    }
                    if (videoDis != 0)
                    {
                        checkVideo = true;
                    }

                    if (Convert.ToDouble(dr["��Ա��Χ"]) != 0)
                    {
                        insertPointsIntoMap("��Ա", Convert.ToDouble(dr["��Ա��Χ"]));
                    }

                    if (Convert.ToDouble(dr["�ΰ����ڷ�Χ"]) != 0)
                    {
                        insertPointsIntoMap("�ΰ�����", Convert.ToDouble(dr["�ΰ����ڷ�Χ"]));
                    }

                    if (Convert.ToDouble(dr["�����ɳ�����Χ"]) != 0)
                    {
                        insertPointsIntoMap("�����ɳ���", Convert.ToDouble(dr["�����ɳ�����Χ"]));
                    }

                    if (Convert.ToDouble(dr["���������ҷ�Χ"]) != 0)
                    {
                        insertPointsIntoMap("����������", Convert.ToDouble(dr["���������ҷ�Χ"]));
                    }

                    if (Convert.ToDouble(dr["����������Χ"]) != 0)
                    {
                        insertPointsIntoMap("��������", Convert.ToDouble(dr["����������Χ"]));
                    }

                    if (Convert.ToDouble(dr["���ɷ�Χ"]) != 0)
                    {
                        insertPointsIntoMap("����", Convert.ToDouble(dr["���ɷ�Χ"]));
                    }

                    if (Convert.ToDouble(dr["��ȫ������λ��Χ"]) != 0)
                    {
                        insertPointsIntoMap("��ȫ������λ", Convert.ToDouble(dr["��ȫ������λ��Χ"]));
                    }
                    if (Convert.ToDouble(dr["������ҵ��Χ"]) != 0)
                    {
                        insertPointsIntoMap("������ҵ", Convert.ToDouble(dr["������ҵ��Χ"]));
                    }
                    if (Convert.ToDouble(dr["�����ص㵥λ��Χ"]) != 0)
                    {
                        insertPointsIntoMap("�����ص㵥λ", Convert.ToDouble(dr["�����ص㵥λ��Χ"]));
                    }
                    if (Convert.ToDouble(dr["�������жӷ�Χ"]) != 0)
                    {
                        insertPointsIntoMap("�������ж�", Convert.ToDouble(dr["�������жӷ�Χ"]));
                    }
                    if (Convert.ToDouble(dr["����˨��Χ"]) != 0)
                    {
                        insertPointsIntoMap("����˨", Convert.ToDouble(dr["����˨��Χ"]));
                    }
                    dr.Close();
                    cmd.Dispose();
                    WriteEditLog("Ԥ������", "Ԥ��", "����=" + dataGridView2.Rows[sIndex].Cells[1].Value.ToString(), "����Ԥ��");
                }
                else
                {
                    MessageBox.Show("Ԥ������,���ܸձ�����ɾ��!");
                    this.Cursor = Cursors.Default;
                    return;
                }

                //ѡ�񰸼�����
                DPoint dp = new DPoint();

                dp.x = pX;
                dp.y = pY;

                SearchDistance(dp, carDis, checkCar, videoDis, checkVideo); //��ѯ�ܱ���Ƶ�ͳ���
            }
            catch (Exception ex)
            {
                ExToLog(ex, "startPlan-16-����Ԥ��");
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                //clearFeatures("ֱ��ָ��");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// �Ƴ���Ƶͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void StopVideo()
        {
            try
            {
                if (mapControl1.Map.Layers["VideoLayer"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLayer");
                }

                if (mapControl1.Map.Layers["VideoLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("VideoLabel");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "StopVideo");
            }
        }

        /// <summary>
        /// ��ȡ���ID
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="Columname">����</param>
        /// <returns>���ID��</returns>
        private string GetID()
        {
            string id = string.Empty;
            try
            {
                string sql = "Select max(ID)+1 from Ԥ��";
                id = GetScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetID-17-��ȡ���ID");
            }
            return id;
        }

        /// <summary>
        /// ��ʾͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sName">�����ؼ���</param>
        /// <param name="dis">��ʾ��Χ</param>
        private void insertPointsIntoMap(string sName,double dis)
        {
            CLC.ForSDGA.GetFromTable.GetFromName(sName,GetFromNamePath);

            string tabName = CLC.ForSDGA.GetFromTable.TableName;

            double x = planX;
            double y = planY;  

            double x1, x2;
            double y1, y2;
            x1 = x - dis;
            x2 = x + dis;
            y1 = y - dis;
            y2 = y + dis;
            string sql = "select * from " + tabName + " where x>" + x1 + " and x<" + x2 + " and y>" + y1 + " and y<" + y2; ;

            try
            {              
                DataTable dt = GetTable(sql);
                if (dt == null || dt.Rows.Count < 1)
                {
                    return;
                }
                FeatureLayer fl = (FeatureLayer)mapControl1.Map.Layers["ֱ��ָ��"];
                Table tableTem = fl.Table;

                //ͨ�������ƻ�ȡͼ��
                string bmpName = CLC.ForSDGA.GetFromTable.BmpName;
                CompositeStyle cs;
                if (bmpName == "gonggong")
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(33, Color.Yellow, 9);
                    cs = new CompositeStyle(pStyle);
                }
                else if (bmpName == "tezhong")
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(36, Color.Cyan, 10);
                    cs = new CompositeStyle(pStyle);
                }
                else
                {
                    cs = new CompositeStyle(new BitmapPointStyle(bmpName));
                }

                //��������ж���
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    double dx = Convert.ToDouble(dt.Rows[i]["X"]);
                    double dy = Convert.ToDouble(dt.Rows[i]["Y"]);
                    if (calDisTwoPoints(x, y, dx, dy) <= dis)//���ҳ������Ƿ���, ������ܲ���,�ٴ��ж�
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(tableTem)).CoordSys, dx, dy);

                        Feature pFeat = new Feature(tableTem.TableInfo.Columns);
                        pFeat.Geometry = pt;
                        pFeat.Style = cs;
                        pFeat["��_ID"] = dt.Rows[i][CLC.ForSDGA.GetFromTable.ObjID].ToString();
                        pFeat["����"] = sName;

                        tableTem.InsertFeature(pFeat);
                    }
                }
            }
            catch(Exception ex)
            {
                ExToLog(ex, "insertPointsIntoMap-18-��ʾͼԪ");
            }
        }

        /// <summary>
        /// ����γ��ת�ɻ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <returns></returns>
        private double calDisTwoPoints(double X1, double Y1, double X2, double Y2)
        {
            double d=0;
            try
            {
                X1 = X1 / 180 * Math.PI;
                Y1 = Y1 / 180 * Math.PI;
                X2 = X2 / 180 * Math.PI;
                Y2 = Y2 / 180 * Math.PI;
                d = Math.Sin(Y1) * Math.Sin(Y2) + Math.Cos(Y1) * Math.Cos(Y2) * Math.Cos(X1 - X2);
                d = Math.Acos(d) * 6371004;
                d = Math.Round(d);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "calDisTwoPoints-19-����γ��ת�ɻ���");
            }
            return d;
        }

        /// <summary>
        /// �����С�ı�ʱ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary
        private void dataGridView2_Resize(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView2);
                }
                else
                {
                    dataGridView1.Columns[1].Width = dataGridView2.Width - 115;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "20-dataGridView2_Resize");
            }
        }

        /// <summary>
        /// �����С�ı�ʱ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary
        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView1);
                }
                else
                {
                    dataGridView1.Columns[1].Width = dataGridView1.Width - 145;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "21-dataGridView1_Resize");
            }
        }

        /// <summary>
        /// �����б���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dataGridView">Ҫ���õĿؼ�</param>
        private void setDataGridViewColumnWidth(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.Rows[0].Height * dataGridView.Rows.Count > dataGridView.Height)
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 160;
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 145;
                }
                if (dataGridView == dataGridView2)
                {
                    dataGridView.Columns[1].Width -= 15;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setDataGridViewColumnWidth-22-�����б���");
            }
        }

        /// <summary>
        /// ����ɼ����л�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void ucZhihui_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    if (this.queryTable != null)
                    {
                        try
                        {
                            this.queryTable.Close();
                            this.queryTable = null;
                        }
                        catch { }

                    }
                    mapControl1.Tools.LeftButtonTool = "Select";
                    dataGridView1.Rows.Clear();
                    dataGridView2.Rows.Clear();
                    RemoveTemLayer("ֱ��ָ��");
                    RemoveTemLayer("�鿴ѡ��");
                    RemoveTemLayer("Ԥ��ͼ��");
                    
                    RemoveTemLayer("VideoLayer");
                    RemoveTemLayer("VideoCarLayer");
                    RemoveTemLayer("CarLayer");
                    
                    closeAllTables();
                }
                else
                {
                    FeatureLayer fl = mapControl1.Map.Layers["�鿴ѡ��"] as FeatureLayer;

                    labeLayer(fl.Table, "����");
                }
                isDel = false;
                this.panError.Visible = false;    // ���ش�����ʾ
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ucZhihui_VisibleChanged-23-����ɼ����л�");
            }
        }

        /// <summary>
        /// �����±�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tabAlias">����</param>
        private void openTable(string tabAlias)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,GetFromNamePath);
                string strSQL = "select * from " + CLC.ForSDGA.GetFromTable.TableName;

                if (tabAlias == "������Ƶ")
                {
                    strSQL = "select CAMID as �豸���, �ն˳������� as �豸����,������λ as �����ɳ���,null as �ճ�������,null as MAPID,null as �豸ID, X,Y from gps������λϵͳ where CAMID is not null and X>0 and Y >0 ";
                }
                if (tabAlias == "�����Ƶ")
                {
                    strSQL = "select * from ��Ƶλ��";
                }
                if (tabAlias == "�������Ƶ") 
                {
                    strSQL = "select * from ��Ƶλ�÷����";
                }

                DataTable datatable = GetTable(strSQL);
                if (datatable == null || datatable.Rows.Count < 1)
                {
                    return;
                }
                i = i + datatable.Rows.Count;
                //����ط��������ɵ�ͼ��������Map������
                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet(tabAlias, datatable);
                MapInfo.Data.SpatialSchemaXY xy = new MapInfo.Data.SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0, 0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;
                
                MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "openTable-24-�����±�");
            }
        }

        private string[] tableNames = new string[] { "��������", "��ȫ������λ", "����", "�ΰ�����", "������ҵ", "�����Ƶ", "�������Ƶ", "������Ƶ", "����˨", "�����ص㵥λ", "����", "�����ɳ���", "��Ա", "�������ж�", "����������" };
        /// <summary>
        /// �رմ򿪵ĵ�ѡ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void closeAllTables()
        {
            try
            {
                for (int i = 0; i < tableNames.Length; i++)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable(tableNames[i]) != null) {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(tableNames[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "closeAllTables-25-�رմ򿪵ĵ�ѡ��");
            }
        }

        /// <summary>
        /// �Ƴ���ʱͼ��,�رձ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableAlies">ͼ������</param>
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
            catch(Exception ex)
            {
                ExToLog(ex, "RemoveTemLayer-26-�Ƴ���ʱͼ��");
            }
        }


        //�����Ԫ�񣬲��Ҷ�Ӧ��Ҫ�أ��任Ҫ�ص���ʽ��ʵ����˸��
        private Feature flashFt;
        private Style defaultStyle;
        int k = 0;
        /// <summary>
        /// �����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dataGridView1.CurrentRow != null)
                if (dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "0" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "0")
                {
                    return;
                }

            //���һ����¼�����е�ͼ��λ
            try
            {
                //��λ
                double x = Convert.ToDouble(dataGridView1["ColX", e.RowIndex].Value);
                double y = Convert.ToDouble(dataGridView1["ColY", e.RowIndex].Value);
               
                // ���´�����������ǰ��ͼ����Ұ�������ö������ڵ��ɳ���   add by fisher in 09-12-24
                DPoint dP = new DPoint(x, y);
                CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                mapControl1.Map.Center = dP;
                mapControl1.Map.SetView(dP, cSys, getScale());
                    

                //��˸Ҫ��
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
                FeatureLayer tempLayer = mapControl1.Map.Layers["����ѡ��"] as MapInfo.Mapping.FeatureLayer;

                Table tableTem = tempLayer.Table;
                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem, MapInfo.Data.SearchInfoFactory.SearchWhere("��_ID='" + dataGridView1["colBianhao", e.RowIndex].Value.ToString() + "'"));

                //��˸Ҫ��
                flashFt = ft;
                defaultStyle = ft.Style;
                k = 0;
                timer1.Start();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellClick-27-�����¼�");
            }
        }

        /// <summary>
        /// ��ȡ���ű���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
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
                CLC.BugRelated.ExceptionWrite(ex, "ucZhihui-getScale");
                return 0;
            }
        }

        private Color col = Color.Blue;
        /// <summary>
        /// ʵ��ͼԪ��˸
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
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
                BasePointStyle pStyle = new SimpleVectorPointStyle(35, col, 26);
                flashFt.Style = pStyle;
                flashFt.Update();
                k++;
                if (k == 10)
                {
                    //flashFt.Style = defaultStyle;
                    //flashFt.Update();
                    timer1.Stop();
                }
            }
            catch(Exception ex)
            {
                timer1.Stop();
                ExToLog(ex, "timer1_Tick-28-ͼԪ��˸");
            }
        }

        /// <summary>
        /// ��ʾԤ������.
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void dataGridView2_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == 1) {
                OracleConnection Conn = new OracleConnection(strConn);
                try
                {
                    string aTip = "";
                    Conn.Open();

                    string strExpress = "select ����,������Χ,����ͷ��Χ,��Ա��Χ,�ΰ����ڷ�Χ,"
                                             + "�����ɳ�����Χ,���������ҷ�Χ,����������Χ,"
                                             + "���ɷ�Χ,��ȫ������λ��Χ,������ҵ��Χ,"
                                             + "�����ص㵥λ��Χ,�������жӷ�Χ,����˨��Χ"
                                             +" from Ԥ�� where ����='" + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() + "'";
                    OracleCommand cmd = new OracleCommand(strExpress, Conn);

                    OracleDataReader dr = cmd.ExecuteReader();
                    if(dr.HasRows){
                        dr.Read();
                        for (int i = 1; i < 13; i++) {
                            if (Convert.ToInt32(dr[i]) != 0) {
                                aTip += dr.GetName(i) + ":" + Convert.ToInt32(dr[i]) + "��\n";
                            }
                        }

                        if (aTip == "")
                        {
                            aTip = "��Ԥ��δ����.�ɽ����޸�";
                        }
                        else {
                            aTip = aTip.Remove(aTip.Length - 1);
                        }
                    }
                    dr.Close();

                    dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText =aTip;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "dataGridView2_CellMouseEnter-29-��ʾԤ������");
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                }
            }
        }
 
        private void textKeyword_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }


        private MapInfo.Geometry.DPoint dptStart;
        private MapInfo.Geometry.DPoint dptEnd;

        private System.Collections.ArrayList arrlstPoints = new ArrayList();
        /// <summary>
        /// ����ʹ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            if (this.Visible)
            {
                try
                {
                    switch (e.ToolName)
                    {
                        case "SelByRect":
                            //add by siumo 20080923
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    dptStart = e.MapCoordinate;
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    dptEnd = e.MapCoordinate;
                                    if (dptStart == dptEnd) return;
                                    MapInfo.Geometry.DRect MapRect = new MapInfo.Geometry.DRect();

                                    MapRect.x1 = dptStart.x;
                                    MapRect.y2 = dptStart.y;
                                    MapRect.x2 = dptEnd.x;
                                    MapRect.y1 = dptEnd.y;
                                    clearFeatures("�鿴ѡ��");
                                    MapInfo.Geometry.Rectangle rect = new MapInfo.Geometry.Rectangle(mapControl1.Map.GetDisplayCoordSys(), MapRect);
                                    selectAndInsertByGeometry((FeatureGeometry)rect);
                                    WriteEditLog("ָ�Ӱװ�", "", "", "��ѡ");
                                    break;
                            }
                            break;
                        case "SelByCircle":
                            //add by siumo 20080923
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    dptStart = e.MapCoordinate;
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    dptEnd = e.MapCoordinate;
                                    double radius = Math.Sqrt((dptEnd.y - dptStart.y) * (dptEnd.y - dptStart.y) + (dptEnd.x - dptStart.x) * (dptEnd.x - dptStart.x));

                                    Ellipse circle = new Ellipse(mapControl1.Map.GetDisplayCoordSys(), dptStart, radius, radius, DistanceUnit.Degree, DistanceType.Spherical);
                                    clearFeatures("�鿴ѡ��");
                                    selectAndInsertByGeometry((FeatureGeometry)circle);
                                    WriteEditLog("ָ�Ӱװ�", "", "", "Ȧѡ");
                                    break;
                            }
                            break;
                        case "SelByPolygon":
                            switch (e.ToolStatus)
                            {
                                case MapInfo.Tools.ToolStatus.Start:
                                    arrlstPoints.Clear();
                                    dptStart = e.MapCoordinate;
                                    arrlstPoints.Add(e.MapCoordinate);
                                    break;
                                case MapInfo.Tools.ToolStatus.InProgress:
                                    arrlstPoints.Add(e.MapCoordinate);
                                    break;
                                case MapInfo.Tools.ToolStatus.End:
                                    //����һ���պϻ�
                                    //arrlstPoints.Add(e.MapCoordinate);
                                    arrlstPoints.Add(dptStart);
                                    int intCount = arrlstPoints.Count;
                                    if (intCount <= 3)
                                    {
                                        MessageBox.Show("�뻭3�����ϵĵ��γ�������������Ҫ�����");
                                        return;
                                    }
                                    MapInfo.Geometry.DPoint[] dptPoints = new DPoint[intCount];
                                    for (int j = 0; j < intCount; j++)
                                    {
                                        dptPoints[j] = (MapInfo.Geometry.DPoint)arrlstPoints[j];
                                    }
                                    //dptPoints[intCount] = dptFirstPoint;

                                    //�ñպϵĻ�����һ����		
                                    MapInfo.Geometry.AreaUnit costAreaUnit;
                                    costAreaUnit = MapInfo.Geometry.CoordSys.GetAreaUnitCounterpart(DistanceUnit.Kilometer);
                                    MapInfo.Geometry.CoordSys objCoordSys = this.mapControl1.Map.GetDisplayCoordSys();
                                    MultiPolygon objPolygon = new MultiPolygon(objCoordSys, CurveSegmentType.Linear, dptPoints);
                                    if (objPolygon == null)
                                    {
                                        return;
                                    }
                                    clearFeatures("�鿴ѡ��");
                                    selectAndInsertByGeometry((FeatureGeometry)objPolygon);
                                    WriteEditLog("ָ�Ӱװ�", "", "", "�����ѡ��");
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Tools_Used-30-����ʹ��");
                }
            }
        }

        /// <summary>
        /// ���ͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        public void clearTem()
        {
            try
            {
                clearFeatures("ֱ��ָ��");
                clearFeatures("�鿴ѡ��");
                clearFeatures("Ԥ��ͼ��");
                StopVideo();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "clearTem-31-���ͼԪ");
            }
        }


        /// <summary>
        /// ���ͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tabAlias">ͼ������</param>
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
                ExToLog(ex, "clearFeatures-31-���ͼԪ");
            }
        }


        private int i = 0; // �洢�û���ѡ��Χ�ڵĶ�����
        /// <summary>
        /// �����ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="geo"></param> 
        private void selectAndInsertByGeometry(FeatureGeometry geo)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                i = 0;
                if (checkBoxChangsuo.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��������") == null)
                    {
                        openTable("��������");
                    }
                    SpatialSearchAndView(geo, "��������");
                }
                if (checkBoxAnfang.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��ȫ������λ") == null)
                    {
                        openTable("��ȫ������λ");
                    }
                    SpatialSearchAndView(geo, "��ȫ������λ");
                }
                if (checkBoxWangba.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����") == null)
                    {
                        openTable("����");
                    }
                    SpatialSearchAndView(geo, "����");
                }
                if (checkBoxZhikou.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�ΰ�����") == null)
                    {
                        openTable("�ΰ�����");
                    }
                    SpatialSearchAndView(geo, "�ΰ�����");
                }
                if (checkBoxTezhong.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("������ҵ") == null)
                    {
                        openTable("������ҵ");
                    }
                    SpatialSearchAndView(geo, "������ҵ");
                }
                if (checkBoxShiping.Checked)
                {
                    //if (MapInfo.Engine.Session.Current.Catalog.GetTable("��Ƶ") == null)
                    //{
                    //    openTable("��Ƶ");
                    //}
                    //SpatialSearchAndView(geo, "��Ƶ");
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�����Ƶ") == null)
                    {
                        openTable("�����Ƶ");
                    }
                    SpatialSearchAndView(geo, "�����Ƶ");
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�������Ƶ") == null)
                    {
                        openTable("�������Ƶ");
                    }
                    SpatialSearchAndView(geo, "�������Ƶ");
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("������Ƶ") == null)
                    {
                        openTable("������Ƶ");
                    }
                    SpatialSearchAndView(geo, "������Ƶ");
                }
                if (checkBoxXiaofangshuan.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����˨") == null)
                    {
                        openTable("����˨");
                    }
                    SpatialSearchAndView(geo, "����˨");
                }
                if (checkBoxXiaofangdangwei.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�����ص㵥λ") == null)
                    {
                        openTable("�����ص㵥λ");
                    }
                    SpatialSearchAndView(geo, "�����ص㵥λ");
                }
                if (checkBoxJingche.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����") == null)
                    {
                        openTable("����");
                    }
                    SpatialSearchAndView(geo, "����");
                }
                if (checkBoxJingyuan.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("��Ա") == null)
                    {
                        openTable("��Ա");
                    }
                    SpatialSearchAndView(geo, "��Ա");
                }
                if (checkBoxPaichusuo.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�����ɳ���") == null)
                    {
                        openTable("�����ɳ���");
                    }
                    SpatialSearchAndView(geo, "�����ɳ���");
                }
                if (checkBoxZhongdui.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�������ж�") == null)
                    {
                        openTable("�������ж�");
                    }
                    SpatialSearchAndView(geo, "�������ж�");
                }
                if (checkBoxJingwushi.Checked)
                {
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("����������") == null)
                    {
                        openTable("����������");
                    }
                    SpatialSearchAndView(geo, "����������");
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                this.panError.Visible = false;
                if (i == 0)  // ����ж��Ƿ���ѡ��Χ���ж���
                {
                    Screen screen = Screen.PrimaryScreen;
                    pt.X = screen.WorkingArea.Width / 2 - 250;
                    pt.Y = screen.WorkingArea.Height / 2 - 100;
                    this.panError.Location = pt;
                    this.panError.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "selectAndInsertByGeometry-32-�����ѯ");
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// ��ѯͼԪ����ʾ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="tabAlias">ͼ����</param>
        private void SpatialSearchAndView(FeatureGeometry geo,string tabAlias)
        {
            try
            {
                FeatureLayer fl=mapControl1.Map.Layers["�鿴ѡ��"] as FeatureLayer;
                Table ccTab = fl.Table;

                CLC.ForSDGA.GetFromTable.GetFromName(tabAlias,GetFromNamePath);
                string objID = CLC.ForSDGA.GetFromTable.ObjID;
                string objName = CLC.ForSDGA.GetFromTable.ObjName;
                
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(geo, ContainsType.Geometry);
                si.QueryDefinition.Columns = null;
                IResultSetFeatureCollection fc= MapInfo.Engine.Session.Current.Catalog.Search(tabAlias,si);

                 //ͨ�������ƻ�ȡͼ��
                string bmpName = CLC.ForSDGA.GetFromTable.BmpName;
                CompositeStyle cs;
                if (bmpName == "gonggong")
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(33, Color.Yellow, 9);
                    cs = new CompositeStyle(pStyle);
                }
                else if (bmpName == "tezhong")
                {
                    MapInfo.Styles.BasePointStyle pStyle = new MapInfo.Styles.SimpleVectorPointStyle(36, Color.Cyan, 10);
                    cs = new CompositeStyle(pStyle);
                }
                else
                {
                        if (tabAlias == "������Ƶ")
                        {
                            cs = new CompositeStyle(new BitmapPointStyle("ydsp.BMP", BitmapStyles.None, System.Drawing.Color.Red, 30));
                            tabAlias = "��Ƶ";
                        }
                        else if (tabAlias == "�������Ƶ")
                        {
                            cs = new CompositeStyle(new BitmapPointStyle("TARG1-32.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            objID = "�豸���";
                            objName = "�豸����";
                            tabAlias = "��Ƶ";
                        }
                        else if (tabAlias == "�����Ƶ")
                        {
                            cs = new CompositeStyle(new BitmapPointStyle("sxt.BMP", BitmapStyles.None, System.Drawing.Color.Red, 12));
                            objID = "�豸���";
                            objName = "�豸����";
                            tabAlias = "��Ƶ";
                        }
                        else
                        {
                            MapInfo.Styles.BitmapPointStyle bitmappointstyle = new MapInfo.Styles.BitmapPointStyle(bmpName);
                            cs = new CompositeStyle(bitmappointstyle);
                        }
                }

                Feature newFt;
                foreach (Feature ft in fc)
                {
                    i++;
                    newFt = new Feature(ccTab.TableInfo.Columns);
                    newFt["��_ID"] = ft[objID];
                    newFt["����"] = ft[objName];
                    newFt["����"] = tabAlias;
                    newFt.Geometry = ft.Geometry;
                    newFt.Style=cs;
                    ccTab.InsertFeature(newFt);
                }
            }
            catch(Exception ex) { 
                ExToLog(ex, "SpatialSearchAndView-33-��ѯͼԪ����ʾ");
            }
        }

        /// <summary>
        /// �л�ѡ��״̬
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.CheckBox check = (System.Windows.Forms.CheckBox)sender;
                
                if (!check.Checked)//ȥ��ѡ��,�Ӳ鿴ѡ����ɾ���������
                {
                    string tableAlies = check.Text;
                    CLC.ForSDGA.GetFromTable.GetFromName(tableAlies,GetFromNamePath);

                    if (tableAlies == "��Ƶ")
                    {
                        DeleteFeature("�����Ƶ");
                        DeleteFeature("�������Ƶ");
                        DeleteFeature("������Ƶ");
                    }
                    else
                    {
                        DeleteFeature(tableAlies);
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "checkBox_CheckedChanged-34-�л�ѡ��״̬");
            }
        }

        /// <summary>
        /// ɾ��ͼԪ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="tableAlies">����</param>
        private void DeleteFeature(string tableAlies)
        {
            try
            {
                FeatureLayer fl = mapControl1.Map.Layers["�鿴ѡ��"] as FeatureLayer;
                Table viewFtTable = fl.Table;
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("����='" + tableAlies + "'");
                si.QueryDefinition.Columns = null;
                IResultSetFeatureCollection fc = MapInfo.Engine.Session.Current.Catalog.Search(viewFtTable, si);
                foreach (Feature ft in fc)
                {
                    viewFtTable.DeleteFeature(ft);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "DeleteFeature-35-ɾ��ͼԪ");
            }
        }


        
        /// <summary>
        /// ��¼������¼ 
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sModule">����ģ��</param>
        /// <param name="tName">�������ݿ����</param>
        /// <param name="sql">����sql���</param>
        /// <param name="method">��������</param>
        private void WriteEditLog(string sModule, string tName, string sql, string method)
        {          
            try
            {     
                string strExe = "insert into ������¼ values('" + User + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'ֱ��ָ��:" + sModule + "','" + tName + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "WriteEditLog-��¼������¼");
            }
        }

        /// <summary>
        /// ��ȡ��ѯ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">ִ�е�sql���</param>
        /// <returns>��ѯ�����</returns>
        private DataTable GetTable(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                return dt;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "WriteEditLog-��ȡ��ѯ�����");
                return null;
            }
        }

        /// <summary>
        /// ����SQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">ִ�е�sql���</param>
        private void RunCommand(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "RunCommand-����SQL");
            }
        }

        /// <summary>
        /// ��ȡScalar
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">ִ�е�sql���</param>
        /// <returns>��һ�е�һ�еĽ��</returns>
        private Int32 GetScalar(string sql)
        {
            try
            {
                CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
                return CLC.DatabaseRelated.OracleDriver.OracleComScalar(sql);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetScalar-��ȡScalar");
                return 0;
            }
        }


        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            try
            {
                CLC.BugRelated.ExceptionWrite(ex,"clZhihui-ucZhihui-"+sFunc);              
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmpname"></param>
        /// <param name="msg"></param>
        public void Deal110Msg(string bmpname, string[] msg)
        {
            try
            {
                InitZhihui();

                //comboBoxField.Text = @"�������";
                //textKeyword.Text = msg[0].Substring(1, msg[0].Length - 1);
                //caseSearch();
                //dataGridView1.Rows[0].Selected = true;

                Bind110Data(msg);

                Create110Ftr(bmpname, msg);

                tabControl1.SelectedTab = tabYuan;
            }
            catch (Exception ex) { ExToLog(ex, "Deal110Msg"); }
        }

        private void Bind110Data(string[] msg)
        {
            try
            {
                DataTable dt = new DataTable("socket");
                //��
                string[] func = { "�������", "�����ص���ַ", "��Ҫ����", "��������", "�����ɳ���", "�����ж�", "�Խ���ID", "������Դ", "����ʱ��", "X", "Y" };
                for (int i = 0; i < func.Length; i++)
                {
                    DataColumn dc = new DataColumn(func[i]);
                    dc.DataType = Type.GetType("System.String");
                    dt.Columns.Add(dc);
                }

                //��
                string ajbh = msg[0].Substring(1, msg[0].Length - 1); // ȥ����־λ�İ������
                DataRow drow = dt.NewRow();
                drow[func[0]] = ajbh;
                for (int i = 1; i < func.Length; i++)
                {
                    drow[func[i]] = msg[i];
                }
                dt.Rows.Add(drow);

                dataGridView1.Rows.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dataGridView1.Rows.Add(i + 1, dt.Rows[i]["�����ص���ַ"], "����...", dt.Rows[i]["�������"], dt.Rows[i]["X"], dt.Rows[i]["Y"]);
                }
                //���ÿ��
                setDataGridViewColumnWidth(dataGridView1);
            }
            catch (Exception ex) { ExToLog(ex, "Bind110Data"); }
        }


        private void Create110Ftr(string bmpname, string[] message) 
        {
            try
            {
                if (queryTable != null)
                {
                    queryTable.Close();

                }
                           
                TableInfoMemTable ti = new TableInfoMemTable("����ѡ��");
                ti.Temporary = true;

                //   add   columns   
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
                column.Alias = "��_ID";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                column = new Column();
                column.Alias = "����";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);

                queryTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);


                //"1A000000@�Ϻ�@��������@���԰@����@��ɳ��@ID101@110����@2010-2-3@113.239@22.8375@".Split('@');

                double dx = 0, dy = 0;
                
                        dx = Convert.ToDouble(message[9]);
                        dy = Convert.ToDouble(message[10]);
                
                    if (dx >0 || dy > 0)
                    {
                    FeatureGeometry pt = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), dx, dy);

                    CompositeStyle cs = new CompositeStyle();
                    cs.ApplyStyle(new BitmapPointStyle(bmpname, BitmapStyles.None, Color.Red, 20));

                    Feature pFeat = new Feature(queryTable.TableInfo.Columns);

                    pFeat.Geometry = pt;
                    pFeat.Style = cs;
                    pFeat["��_ID"] = message[0].Substring(1, message[0].Length - 1);
                    pFeat["����"] = "����ϵͳ";
                    queryTable.InsertFeature(pFeat);
                    }                

                currentFeatureLayer = new FeatureLayer(queryTable, "����ѡ��");

                mapControl1.Map.Layers.Insert(0, currentFeatureLayer);

                labeLayer(queryTable, "��_ID");
                //this.setLayerStyle(currentFeatureLayer, Color.Red, 34, 10);
                // ���´�����������ǰ��ͼ����Ұ������1:6000   jie.zhang 20100311
                try
                {
                    DPoint dP = new DPoint(dx, dy);
                    CoordSys cSys = mapControl1 .Map.GetDisplayCoordSys();

                    mapControl1.Map.SetView(dP, cSys, 6000);
                    mapControl1.Map.Center = dP;
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "Create110Ftr-�趨��ͼ��Χ");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Create110Ftr-����ѯ�����ʾ�ڵ�ͼ��");
                //writeToLog(ex,"insertQueryIntoMap");
            }
        }

        /// <summary>
        /// ˫�����ж�λ���ü�¼��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //���һ����¼�����е�ͼ��λ
            try
            {
                if (e.RowIndex < 0) return;

                DataTable datatable = GetTable("Select * from GPS110.������Ϣ110 where GPS110.������Ϣ110.�������='" + dataGridView1["colBianhao", e.RowIndex].Value.ToString() + "'");
                if (dataGridView1.CurrentRow != null)
                    if (dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "" || dataGridView1.CurrentRow.Cells["ColX"].Value.ToString() == "0" || dataGridView1.CurrentRow.Cells["ColY"].Value.ToString() == "0")
                    {
                        System.Drawing.Point ptZ = new System.Drawing.Point();
                        Screen scren = Screen.PrimaryScreen;
                        ptZ.X = scren.WorkingArea.Width / 2;
                        ptZ.Y = 10;

                        disPlayInfo(datatable, ptZ);
                        return;
                    }


                //��λ
                double x = Convert.ToDouble(dataGridView1["ColX", e.RowIndex].Value);
                double y = Convert.ToDouble(dataGridView1["ColY", e.RowIndex].Value);

                // ���´�����������ǰ��ͼ����Ұ�������ö������ڵ��ɳ���   add by fisher in 09-12-24
                DPoint dP = new DPoint(x, y);
                CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                mapControl1.Map.Center = dP;
                mapControl1.Map.SetView(dP, cSys, getScale());

                //��˸Ҫ��
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
                FeatureLayer tempLayer = mapControl1.Map.Layers["����ѡ��"] as MapInfo.Mapping.FeatureLayer;

                Table tableTem = tempLayer.Table;
                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tableTem, MapInfo.Data.SearchInfoFactory.SearchWhere("��_ID='" + dataGridView1["colBianhao", e.RowIndex].Value.ToString() + "'"));

                //��˸Ҫ��
                flashFt = ft;
                defaultStyle = ft.Style;
                k = 0;
                timer1.Start();

                if (dP.x == 0 || dP.y == 0)
                {
                    dP.x = mapControl1.Map.Center.x;
                    dP.y = mapControl1.Map.Center.y;
                }

                System.Drawing.Point pt = new System.Drawing.Point();
                mapControl1.Map.DisplayTransform.ToDisplay(dP, out pt);
                pt.X += Width + 10;
                pt.Y += 80;

                disPlayInfo(datatable, pt); 

            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellDoubleClick-27-�����¼�");
            }
        }

        private FrmInfo frminfo = new FrmInfo();

        /// <summary>
        /// ��ʾ������ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="pt">λ��</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt)
        {
            try
            {
                if (frminfo.Visible == false)
                {
                    frminfo = new FrmInfo();
                    frminfo.SetDesktopLocation(-30, -30);
                    frminfo.Show();
                    frminfo.TopMost = true;
                    frminfo.Visible = false;
                }
                frminfo.setInfo(dt.Rows[0], pt, StrCon, User);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo-16-��ʾ������ϸ��Ϣ");
            }
        }

        /// <summary>
        /// �ı�������س�����в�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spellSearchBoxEx1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r")
                {
                    caseSearch();

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                    //this.toolPro.Value = 3;
                    //Application.DoEvents();
                    //isShowPro(false);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "spellSearchBoxEx1_KeyPress");
            }
        }

        /// <summary>
        /// �Զ���ȫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void spellSearchBoxEx1_TextChanged(object sender, EventArgs e)
        {
            try
            {

                string keyword = ToDBC(this.CaseSearchBox.Text.Trim());
                string colfield = this.comboBoxField.Text.Trim();

                if (keyword.Length < 1 || colfield.Length < 1) return;
                
                if (colfield == "��������")
                {

                    string strExp = "select distinct(��������) from �������� where �������� like '%" + ToDBC(this.CaseSearchBox.Text.Trim()) + "%' order by ��������";
                    DataTable dt = GetTable(strExp);

                    if (this.CaseSearchBox.Text.Trim().Length < 1)
                        CaseSearchBox.GetSpellBoxSource(null);
                    else
                        CaseSearchBox.GetSpellBoxSource(dt);
                }
                else
                {
                    CaseSearchBox.GetSpellBoxSource(null);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "spellSearchBoxEx1_TextChanged-17-����ƥ��");
            }
        }

        /// <summary>
        /// �Զ���ȫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void CaseSearchBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                string colfield = this.comboBoxField.Text.Trim();

                if (colfield == "�����ɳ���" || colfield == "�����ж�" || colfield == "����������")
                {
                    string strExp = string.Empty;

                    if (colfield == "�����ɳ���")
                        strExp = "select distinct(�ɳ�����) from �����ɳ��� order by �ɳ�����";
                    else if (colfield == "�����ж�")
                        strExp = "select distinct(�ж���) from �������ж� order by �ж���";
                    else if (colfield == "����������")
                        strExp = "select distinct(��������) from ���������� order by ��������";

                    DataTable dt = GetTable(strExp);

                    if (dt.Rows.Count < 1)
                        CaseSearchBox.GetSpellBoxSource(null);
                    else
                        CaseSearchBox.GetSpellBoxSource(dt);
                }
                else
                {
                    CaseSearchBox.GetSpellBoxSource(null);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "CaseSearchBox_MouseDown");
            }
        }

        /// <summary>
        /// ��ʾ�����ؽ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="falg">����ֵ��true-��ʾ false-���أ�</param>
        private void isShowPro(bool falg)
        {
            try
            {
                this.toolPro.Value = 0;
                this.toolPro.Maximum = 3;
                this.toolPro.Width = 100;
                this.toolProLbl.Visible = falg;
                this.toolProSep.Visible = falg;
                this.toolPro.Visible = falg;
                Application.DoEvents();
            }
            catch (Exception ex) { ExToLog(ex, "isShowPro"); }
        }

        /// <summary>
        /// ����Ǹ�Ϊȫ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="input">Ҫ�ı���ı�</param>
        /// <returns>�ı����ı�</returns>
        private String ToDBC(String input)
        {
            try
            {
                char[] c = input.ToCharArray();

                for (int i = 0; i < c.Length; i++)
                {
                    if (c[i] == 12288)
                    {

                        c[i] = (char)32;

                        continue;

                    }
                    if (c[i] > 65280 && c[i] < 65375)

                        c[i] = (char)(c[i] - 65248);

                }
                return new String(c);
            }
            catch (Exception ex) { ExToLog(ex, "ToDBC"); return null; }
        }

        /// <summary>
        /// ��ҳ�ؼ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void bindingNavigatorZhihui_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.TextNum.Text);    //ÿҳ��ʾ��������
                bool isFirst = false;
                if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        isFirst = true;
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        if (endNo == nMax)
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            startNo = nMax - (nMax % countShu) + 1 - countShu;
                            endNo = nMax - (nMax % countShu);
                        }
                        else
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            startNo -= countShu;
                            endNo -= countShu;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "��һҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent++;
                        nCurrent = pageSize * (pageCurrent - 1);
                        startNo += countShu;
                        endNo += countShu;
                    }
                }
                else if (e.ClickedItem.Text == "ת����ҳ")
                {
                    if (pageCurrent <= 1)
                    {
                        isFirst = true;
                        System.Windows.Forms.MessageBox.Show("�Ѿ��ǵ�һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = 0;
                        startNo = 1;
                        endNo = countShu;
                    }
                }
                else if (e.ClickedItem.Text == "ת��βҳ")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        System.Windows.Forms.MessageBox.Show("�Ѿ������һҳ����������һҳ���鿴��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = pageCount;
                        nCurrent = pageSize * (pageCurrent - 1);
                        startNo = nMax - (nMax % countShu) + 1;
                        endNo = nMax;
                    }
                }
                else
                {
                    return;
                }
                // LoadData(startNo, endNo);
                DataTable dt = LoadData(countShu, pageCurrent, isFirst);
                this.dataGridView1.Rows.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dataGridView1.Rows.Add(i + 1, dt.Rows[i]["�����ص���ַ"], "����...", dt.Rows[i]["�������"], dt.Rows[i]["X"], dt.Rows[i]["Y"], dt.Rows[i]["��������"]);
                    if (i % 2 == 1)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                }
                //���ÿ��
                setDataGridViewColumnWidth(dataGridView1);
                insertQueryIntoMap(dt);

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ͨ���ı䵱ǰҳ��ʵ�ַ�ҳ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void PageNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                bool isFrist = false;
                if (e.KeyChar.ToString() == "\r")
                {
                    if (Convert.ToInt32(PageNow.Text) < 1 || Convert.ToInt32(PageNow.Text) > pageCount)
                    {
                        System.Windows.Forms.MessageBox.Show("ҳ�볬����Χ�����������룡", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        PageNow.Text = pageCurrent.ToString();
                        return;
                    }
                    else
                    {
                        this.pageCurrent = Convert.ToInt32(PageNow.Text);
                        nCurrent = pageSize * (pageCurrent - 1);
                        startNo = ((pageCurrent - 1) * pageSize) + 1;
                        endNo = startNo + pageSize - 1;
                    }

                    //LoadData(startNo, endNo);
                    DataTable dt = LoadData(pageSize, pageCurrent, isFrist);
                    this.dataGridView1.Rows.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add(i + 1, dt.Rows[i]["�����ص���ַ"], "����...", dt.Rows[i]["�������"], dt.Rows[i]["X"], dt.Rows[i]["Y"], dt.Rows[i]["��������"]);
                        if (i % 2 == 1)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    //���ÿ��
                    setDataGridViewColumnWidth(dataGridView1);
                    insertQueryIntoMap(dt);

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "PageNow_KeyPress"); }
        }

        /// <summary>
        /// �ı��б���ʾ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void TextNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString() == "\r" && dataGridView1.Rows.Count > 0)
                {
                    this.pageSize = Convert.ToInt32(TextNum.Text);
                    this.pageCurrent = 1;
                    nCurrent = pageSize * (pageCurrent - 1);
                    pageCount = (nMax / pageSize);//�������ҳ��
                    if ((nMax % pageSize) > 0) pageCount++;
                    endNo = pageSize;
                    startNo = 1;

                    //LoadData(startNo, endNo);

                    DataTable dt = LoadData(pageSize, 1, true);

                    this.dataGridView1.Rows.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add(i + 1, dt.Rows[i]["�����ص���ַ"], "����...", dt.Rows[i]["�������"], dt.Rows[i]["X"], dt.Rows[i]["Y"], dt.Rows[i]["��������"]);
                        if (i % 2 == 1)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                    }
                    //���ÿ��
                    setDataGridViewColumnWidth(dataGridView1);
                    insertQueryIntoMap(dt);

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells["ColX"].Value.ToString() == "" || dataGridView1.Rows[i].Cells["ColY"].Value.ToString() == "")
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                }
            }
            catch (Exception ex) { ExToLog(ex, "TextNum_KeyPress"); }
        }

        /// <summary>
        /// ȡ���Զ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                this.panel3.Visible = false;
                mapControl1.Tools.LeftButtonTool = "Select";
                RemoveTemLayer("Ԥ��ͼ��");
                clearFeatures("ֱ��ָ��");
                clearFeatures("�鿴ѡ��");
            }
            catch (Exception ex)
            {
                ExToLog(ex, "button1_Click");
            }
        }

        private double planX = 0;  // Ԥ����������
        private double planY = 0;  // Ԥ����������

        /// <summary>
        /// �����Զ��������ص�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-11
        /// </summary>
        private void btnAddress_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textPlan.Text != string.Empty)
                {
                    mapControl1.Tools.LeftButtonTool = "AddPoint";
                }
                else
                {
                    MessageBox.Show("�������������ƺ󴴽��ص㣡", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "btnAddress_Click");
            }
        }
    }
}