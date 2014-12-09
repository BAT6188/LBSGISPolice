using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MapInfo.Windows.Controls;
using MapInfo.Data;
using MapInfo.Engine;
using MapInfo.Mapping;
using MapInfo.Styles;
using System.Reflection;
using System.Data.OracleClient;
using System.IO;

namespace cl3Color
{
    public partial class uc3Color : UserControl
    {
        private MapControl mapControl1;
        private string strConn = "";
        public string user = "";
        public string strRegion = "";
        public uc3Color(MapControl m,string s)
        {
            InitializeComponent();
            mapControl1 = m;
            strConn = s;
            setDefaultDate();
        }

        /// <summary>
        /// ����Ĭ��ֵ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        private void setDefaultDate()
        {
            try
            {
                DateTime dtNow = DateTime.Now;
                int iDays = Convert.ToInt16(dtNow.DayOfWeek);
                TimeSpan ts1 = new TimeSpan((iDays + 7), 0, 0, 0);
                dateFrom.Value = dtNow.Subtract(ts1);
            }
            catch(Exception ex) {
                write3ColorLog(ex, "setDefaultDate");
            }
        }

        /// <summary>
        /// ����������ɫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        private void btnColor1_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    btnColor1.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "btnColor1_Click");
            }
        }

        /// <summary>
        /// ����С��������ɫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        private void btnColor2_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    btnColor2.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "btnColor2_Click");
            }
        }

        /// <summary>
        /// �����½���ɫ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        private void btnColor3_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    btnColor3.BackColor = colorDialog1.Color;
                }
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "btnColor3_Click");
            }
        }

        /// <summary>
        /// ����Ԥ����Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioZD.Checked && comboBoxZD.Text.Trim() == "")
            {
                MessageBox.Show("��ѡ���ж������ɳ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime fromTime = dateFrom.Value.Date;
            DateTime endTime;
            if (radioButton1W.Checked)        //һ��
            {
                endTime = fromTime.AddDays(7);
            }
            else if (radioButton2W.Checked)     //����
            {
                endTime = fromTime.AddDays(14);
            }
            else {    //һ����
                //endTime = new DateTime(fromTime.Year, (fromTime.Month + 1), fromTime.Day);
                endTime = fromTime.AddMonths(1);
            }

            if (endTime>DateTime.Now)
            {
                MessageBox.Show("��ֹ���ڳ�����������,������!", "ʱ�����ô���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            try
            {
                int kk = 0;
                if (radioButton1W.Checked) {
                    kk = 0;
                }
                else if (radioButton2W.Checked) {
                    kk = 1;
                }
                else {
                    kk = 2;
                }
                clsCal3Color cl3 = new clsCal3Color(fromTime, endTime,radioJingzong.Checked,kk,radioPCS.Checked);
                //string[] strArr ={ "����", "�ݹ�", "�׽�", "����", "�´�", "�ִ�", "����", "����", "��̳", "����" };

                string[] strArr = getPaichusuo();
                string tAlias = "";
                try {
                    if (mapControl1.Map.Layers["�ɳ���"] != null)
                    {
                        this.mapControl1.Map.Layers.Remove("�ɳ���");
                        Session.Current.Catalog.CloseTable("�ɳ���");
                    }
                    if (mapControl1.Map.Layers["�ж�"] != null)
                    {
                        this.mapControl1.Map.Layers.Remove("�ж�");
                        Session.Current.Catalog.CloseTable("�ж�");
                    }
                    // add by fisher in 10-01-07   �ڴ��Ī������س��������ٴιرձ�
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("���ж�Ͻ����ͼ") != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("���ж�Ͻ����ͼ");
                    }
                }
                catch { }
                if (radioPCS.Checked) {
                    tAlias = "�ɳ���";

                    createMemTable("�ɳ���Ͻ����ͼ",tAlias);
                }
                else{
                    tAlias = "�ж�";
                    createMemTable("���ж�Ͻ����ͼ",tAlias);
                    strArr = getZhongduiByName(comboBoxZD.Text.Trim());
                }

                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(1, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Gray);

                SimpleInterior simInterior;
                CompositeStyle comStyle;
                for (int i = 0; i < strArr.Length; i++)
                {
                    string name = strArr[i];
                    string whereClause = "";
                    if (radioPCS.Checked)
                    {
                        whereClause = tAlias + "����='" + name + "'";
                    }
                    else {
                        whereClause = tAlias + "����='" + name + "'";
                    }
                    Feature ft = Session.Current.Catalog.SearchForFeature(tAlias, MapInfo.Data.SearchInfoFactory.SearchWhere(whereClause));
                    if (ft == null)  // ���Ϊ�գ������
                    {
                        continue;
                    }

                    cl3.isFudong = checkBoxFudong.Checked;
                    cl3.setParameter(name);

                    double iFudong = 0.1;
                    if (checkBoxFudong.Checked)
                    {
                       iFudong = getFudongValue(cl3.A1,cl3.A2,cl3.A3); //��ȡ����ֵ
                    }

                     if (cl3.A0 >= iFudong)
                    {
                        simInterior = new SimpleInterior(9, btnColor1.BackColor, System.Drawing.Color.Gray, false);
                        comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
                    }
                    else if (cl3.A0 <= 0.0)
                    {
                        simInterior = new SimpleInterior(9, btnColor3.BackColor, System.Drawing.Color.Gray, false);
                        comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
                    }
                    else
                    {
                        simInterior = new SimpleInterior(9, btnColor2.BackColor, System.Drawing.Color.Gray, false);
                        comStyle = new CompositeStyle(new AreaStyle(simLineStyle, simInterior), null, null, null);
                    }
                    ft.Style = comStyle;
                    if (string.Format("{0:0.0}", cl3.A0) == "������")
                    {
                        ft["Ԥ��ֵ"] = "���������ް���";
                    }
                    else
                    {
                        ft["Ԥ��ֵ"] = string.Format("{0:0.00}", cl3.A0*100) +"%";
                    }
                    ft.Update();
                    Application.DoEvents();
                }

                string sql="";
                if (radioJingzong.Checked)
                {
                    sql = "select * from ������Ϣ where ����ʱ���ֵ>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and ������Դ='����'";
                }
                else
                {
                    sql = "select * from gps110.������Ϣ110 where ����ʱ���ֵ>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and ������Դ='110'";
                }
                WriteEditLog(":"+sql, "����Ԥ��ͼ");
            }
            catch(Exception ex) {
                write3ColorLog(ex,"buttonOK_Click");
            }
            this.Cursor = Cursors.Default;
        }        

        /// <summary>
        /// ���ĳ���ɳ����µ������ж����Ƽ��жӴ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="pcsName">�ɳ�����</param>
        /// <returns>�����ж����Ƽ��жӴ��������</returns>
        private string[] getZhongduiByName(string pcsName)
        {
            string a = "", b = "";
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                //�����ɳ������а������,�������ò�ѯ����
                if (radioJingzong.Checked)
                {
                    cmd.CommandText = "select �жӴ���,�ж��� from �������ж� where �����ɳ���='" + pcsName + "'";
                }
                else
                {
                    cmd.CommandText = "select �жӴ���,�ж��� from �ж�110 where �����ɳ���='" + pcsName + "'";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ////�����ɳ�����������������,��ɫԤ������Ҫ
                    //if (dr.GetValue(0).ToString().Trim().IndexOf("���") == -1)
                    //{
                    a += dr.GetValue(0).ToString().Trim() + ",";
                    b += dr.GetValue(1).ToString().Trim() + ",";
                    //}
                }
                Conn.Close(); 
                b = b.Remove(b.LastIndexOf(','));
                a = a.Remove(a.LastIndexOf(','));
                paiZhong = b.Split(',');
                return a.Split(',');
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                write3ColorLog(ex, "getZhongduiByName");
                return null;
            }
        }

        /// <summary>
        /// �ڵ�ͼ������Ԥ��ֵ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="strLayer">����</param>
        /// <param name="layer">ͼ����</param>
        private void createMemTable(string strLayer,string layer)
        {
            try
            {
                if (mapControl1.Map.Layers[layer] == null)
                {
                    MapInfo.Data.TableInfoMemTable mainMemTableInfo = new MapInfo.Data.TableInfoMemTable(layer);

                    string strSQL = "";
                    //string _conStr = "";

                    if (radioJingzong.Checked)
                    {
                        strSQL = "select  *  from " + strLayer;
                        //_conStr = strConn;
                    }
                    else
                    {
                        strSQL = "select  *  from " + strLayer + "110";
                        //_conStr = "data source =gis;user id =gps110;password =gps11012345;";
                    }

                    MIConnection miConnection = new MIConnection();

                    TableInfoServer ti = new TableInfoServer(strLayer, strConn.Replace("data source = ", "SRVR=").Replace("user id = ", "UID=").Replace("password = ", "PWD="), strSQL, ServerToolkit.Oci);
                    ti.CacheSettings.CacheType = CacheOption.Off;
                    miConnection.Open();
                    Table tempTable = miConnection.Catalog.OpenTable(ti);
                    miConnection.Close();  

                    foreach (MapInfo.Data.Column col in tempTable.TableInfo.Columns) //���Ʊ�ṹ
                    {
                        MapInfo.Data.Column col2 = col.Clone();

                        col2.ReadOnly = false;

                        mainMemTableInfo.Columns.Add(col2);
                    }
                    Column labelCol = new Column("Ԥ��ֵ", MIDbType.String);
                    if (mainMemTableInfo.Columns.Contains(labelCol)==false)             // add by fisher in 10-01-07
                    {
                        mainMemTableInfo.Columns.Add(labelCol);
                    }

                    // add by fisher in 10-01-07
                    // �ٴ�ǿ�йرա��жӡ�����Ϊ�������ж�κ��Ī��������ڴ����
                    if (MapInfo.Engine.Session.Current.Catalog.GetTable("�ж�") != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable("�ж�");
                    }

                    Table table = MapInfo.Engine.Session.Current.Catalog.CreateTable(mainMemTableInfo);

                    string currentSql = "Insert into " + table.Alias + "  Select *,'0' From " + tempTable.Alias;//����ͼԪ����
                    if (radioZD.Checked) {
                        currentSql += " where �����ɳ���='"+comboBoxZD.Text.Trim()+"'";
                    }

                    miConnection.Open();
                    MICommand miCmd = miConnection.CreateCommand();
                    miCmd.CommandText = currentSql;
                    miCmd.ExecuteNonQuery();
                    miCmd.Dispose();
                    miConnection.Catalog.CloseTable(tempTable.Alias);
                    miConnection.Close();

                    IMapLayer mapLayer = mapControl1.Map.Layers["��עͼ��"];
                    int ilayer = mapControl1.Map.Layers.IndexOf(mapLayer);
                    FeatureLayer fl = new FeatureLayer(table);
                    mapControl1.Map.Layers.Insert(ilayer + 1, fl);
                    try
                    {
                        mapControl1.Map.SetView(fl);
                    }
                    catch { }
                    labeLayer(fl.Table);
                }
            }
            catch(Exception ex) {
                write3ColorLog(ex,"createMemTable");
            }
        }     

        /// <summary>
        /// ��ӱ�עͼ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="table">Table</param>
        private void labeLayer(Table table)
        {
            try
            {
                LabelLayer labelLayer = this.mapControl1.Map.Layers["��עͼ��"] as LabelLayer;

                LabelSource source = new LabelSource(table);

                source.DefaultLabelProperties.Caption = "Ԥ��ֵ";
                source.DefaultLabelProperties.Layout.Offset = 4;

                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;

                labelLayer.Sources.Insert(0, source);
            }
            catch (Exception ex)
            {
                write3ColorLog(ex,"labeLayer");
            }
        }

        private string[] paiZhong;
        /// <summary>
        /// ����Ԥ��ͳ�Ʊ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (radioZD.Checked && comboBoxZD.Text.Trim() == "")
            {
                MessageBox.Show("��ѡ���ж������ɳ���!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime fromTime = dateFrom.Value.Date;
            DateTime endTime;
            if (radioButton1W.Checked)
            {
                endTime = fromTime.AddDays(7);
            }
            else if (radioButton2W.Checked)
            {
                endTime = fromTime.AddDays(14);
            }
            else
            {
                endTime = new DateTime(fromTime.Year, (fromTime.Month + 1), fromTime.Day);
            }

            if (endTime > DateTime.Now)
            {
                MessageBox.Show("��ֹ���ڳ�����������,������!", "ʱ�����ô���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

           // Cursor.Current = Cursors.WaitCursor;


            try{
                Excel.Application xls_exp = new Excel.ApplicationClass();
                Excel._Workbook xls_book;
                if(radioPCS.Checked){
                    xls_book = xls_exp.Workbooks._Open(Application.StartupPath + @"\pcsModel.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                    , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
                else{
                    xls_book = xls_exp.Workbooks._Open(Application.StartupPath + @"\zdModel.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value
                     , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                 , Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }

                //xls_book.Title =dateFrom.Value.ToLongDateString()+"��"+dateTo.Value.ToLongDateString() +" ����ͳ�Ʒ�����";
                Excel._Worksheet xls_sheet = (Excel.Worksheet)xls_book.ActiveSheet;
                Object oMissing = System.Reflection.Missing.Value;  //ʵ��������ʱȱʡ���� 

                int kk = 0;
                if (radioButton1W.Checked)
                {
                    kk = 0;
                }
                else if (radioButton2W.Checked)
                {
                    kk = 1;
                }
                else
                {
                    kk = 2;
                }

                clsCal3Color cl3 = new clsCal3Color(fromTime, endTime,radioJingzong.Checked,kk,radioPCS.Checked);
     
                cl3.isFudong = checkBoxFudong.Checked;
                Excel.Range rng3;
                Application.DoEvents();
                //string[] strArrtem;
                string[] strArr;// { "ȫ��", "����", "�ݹ�", "�׽�", "����", "�´�", "�ִ�", "����", "����", "��̳", "����" };
                //if (radioPCS.Checked) {
                //    strArrtem = getPaichusuo();
                //    strArr=new string[strArrtem.Length+1];
                //    strArr[0] = "ȫ��";
                //}

                //else {
                //    strArrtem = getZhongduiByName(comboBoxZD.Text.Trim());
                //    strArr=new string[strArrtem.Length+1];
                //    strArr[0] = "ȫ��(��)";
                //}
                if (radioPCS.Checked)
                {
                    strArr = getPaichusuo();
                }
                else {
                    strArr = getZhongduiByName(comboBoxZD.Text.Trim());
                }

                //for(int i=0;i<strArrtem.Length;i++){
                //    strArr[i+1]=strArrtem[i];
                //}
                string title = "";
                if (radioButton1M.Checked)
                {
                    title = "����(��)�¾�����ɫԤ���ֲ���";
                    if (radioZD.Checked)
                    {
                       title= comboBoxZD.Text.Trim() + "���ж��¾�����ɫԤ���ֲ���";
                    }
                }
                else {
                    title = "����(��)�ܾ�����ɫԤ���ֲ���";
                    if (radioZD.Checked)
                    {
                        title = comboBoxZD.Text.Trim()+ "���ж��ܾ�����ɫԤ���ֲ���";
                    }
                }

                if (radioJingzong.Checked)
                {
                    title=title.Replace("����", "����");
                    xls_sheet.Cells[3, 5] = "���°���";
                    xls_sheet.Cells[3, 14] = "��������";
                }
                else
                {
                    xls_sheet.Cells[3, 5] = "���¾���";
                    xls_sheet.Cells[3, 14] = "��������";
                }
                
                xls_sheet.Cells[1, 1] = title;
                xls_sheet.Cells[2, 1] = "��ѯ���ڷ�Χ:" + fromTime.ToShortDateString() + "��" + endTime.ToShortDateString();
                xls_sheet.Cells[2, 8] = "����ʱ�䣺" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

                frmPro fPro = new frmPro();
                fPro.progressBar1.Value = 0;
                fPro.progressBar1.Maximum = strArr.Length;
                fPro.Show();
                Application.DoEvents();

                for (int i = 4; i < strArr.Length + 4; i++)
                {
                    string name = paiZhong[i - 4];
                    int iRow = i * 3 - 4;
                    xls_sheet.Cells[iRow, 1] = name;
                    //if (name == "ȫ��" || name == "ȫ��(��)")
                    //{
                    //    continue;
                    //    //cl3.getData("",getCycleValue());
                    //}
                    //else
                    //{
                    cl3.getData(strArr[i-4]);
                    //}


                    xls_sheet.Cells[iRow, 5] = cl3.XAZS;
                    xls_sheet.Cells[iRow, 6] = cl3.DQQC;
                    xls_sheet.Cells[iRow, 7] = cl3.DMTC;
                    xls_sheet.Cells[iRow, 8] = cl3.LRDQ;
                    xls_sheet.Cells[iRow, 9] = cl3.FCQD;
                    xls_sheet.Cells[iRow, 10] = cl3.YBQD;
                    xls_sheet.Cells[iRow, 11] = cl3.QJ;
                    xls_sheet.Cells[iRow, 12] = cl3.SH;
                    xls_sheet.Cells[iRow, 13] = cl3.ZP;
                    xls_sheet.Cells[iRow, 14] = cl3.ZAZS;
                    xls_sheet.Cells[iRow, 15] = cl3.DB;
                    xls_sheet.Cells[iRow, 16] = cl3.DJDO;
                    xls_sheet.Cells[iRow, 17] = cl3.TQ;
                    xls_sheet.Cells[iRow, 18] = cl3.XD;
                    xls_sheet.Cells[iRow, 19] = cl3.ZS;

                    xls_sheet.Cells[iRow + 1, 5] = cl3.XAZS2;
                    xls_sheet.Cells[iRow + 1, 6] = cl3.DQQC2;
                    xls_sheet.Cells[iRow + 1, 7] = cl3.DMTC2;
                    xls_sheet.Cells[iRow + 1, 8] = cl3.LRDQ2;
                    xls_sheet.Cells[iRow + 1, 9] = cl3.FCQD2;
                    xls_sheet.Cells[iRow + 1, 10] = cl3.YBQD2;
                    xls_sheet.Cells[iRow + 1, 11] = cl3.QJ2;
                    xls_sheet.Cells[iRow + 1, 12] = cl3.SH2;
                    xls_sheet.Cells[iRow + 1, 13] = cl3.ZP2;
                    xls_sheet.Cells[iRow + 1, 14] = cl3.ZAZS2;
                    xls_sheet.Cells[iRow + 1, 15] = cl3.DB2;
                    xls_sheet.Cells[iRow + 1, 16] = cl3.DJDO2;
                    xls_sheet.Cells[iRow + 1, 17] = cl3.TQ2;
                    xls_sheet.Cells[iRow + 1, 18] = cl3.XD2;
                    xls_sheet.Cells[iRow + 1, 19] = cl3.ZS2;

                    rng3 = xls_sheet.get_Range("B" + iRow.ToString(), Missing.Value);

                    double iFudong = 0.1;
                    if (checkBoxFudong.Checked)
                    {
                        iFudong = getFudongValue(cl3.A1, cl3.A2, cl3.A3); //��ȡ����ֵ
                    }

                    if (cl3.A0 <= 0.0)
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(btnColor3.BackColor);
                    }
                    else if (cl3.A0 >= iFudong)
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(btnColor1.BackColor);
                    }
                    else
                    {
                        rng3.Interior.Color = ColorTranslator.ToWin32(btnColor2.BackColor);
                    }
                    

                    if (fPro.Visible == false) {
                        xls_exp = null;
                        xls_book = null;
                        xls_sheet = null;
                        return;
                    }
                    fPro.progressBar1.Value += 1;
                    Application.DoEvents();
                }

                fPro.Close();
                xls_exp.Visible = true;
                int generation = 0;
                xls_exp.UserControl = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xls_exp);
                generation = System.GC.GetGeneration(xls_exp);

                xls_exp = null;
                xls_book = null;
                xls_sheet = null;
                string sql="";
                if (radioJingzong.Checked)
                {
                    sql = "select * from ������Ϣ where ����ʱ���ֵ>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and ������Դ='����'";
                }
                else
                {
                    sql = "select * from gps110.������Ϣ110 where ����ʱ���ֵ>=to_date('" + fromTime.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + endTime.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    //sql += " and ������Դ='110'";
                }
                WriteEditLog(":"+sql, "������ɫԤ��ͳ�Ʊ�");
            }
            catch
            {
                MessageBox.Show("δ��װ΢���Excel����������ܽ�����ִ�У�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// �õ��ɳ�������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <returns>�洢�ɳ������Ƽ���</returns>
        private string[] getPaichusuo()
        {
            string a = "", b = "";
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                //�����ɳ������а������,�������ò�ѯ����
                if (radioJingzong.Checked)
                {
                    cmd.CommandText = "select �ɳ�������,�ɳ����� from �����ɳ��� where �ɳ����� not like '%���%'";
                }
                else
                {
                    cmd.CommandText = "select �ɳ�������,�ɳ����� from �ɳ���110";
                }
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ////�����ɳ�����������������,��ɫԤ������Ҫ
                    //if (dr.GetValue(0).ToString().Trim().IndexOf("���") == -1)
                    //{
                    a += dr.GetValue(0).ToString().Trim() + ",";
                    b += dr.GetValue(1).ToString().Trim() + ",";
                    //}
                }
                Conn.Close();
                b = b.Remove(b.LastIndexOf(','));
                a = a.Remove(a.LastIndexOf(','));
                paiZhong = b.Split(',');
                return a.Split(',');
            }
            catch(Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                write3ColorLog(ex, "getPaichusuo");
                return null;
            }
        }

        /// <summary>
        /// ����ǰ�����ڵ�Ԥ��ֵ,���������ڵ�Ԥ���ź����ֵ 
        /// ���Ԥ��ֵС��0�����Ӹ���ֵ1�������-10��С��������һ������ֵ1
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="p_2"></param>
        /// <param name="p_3"></param>
        /// <param name="p_4"></param>
        /// <returns></returns>
        private double getFudongValue(double p_2, double p_3, double p_4)
        {
            try
            {
                double i = 10;
                if (p_2 <= 0)
                {
                    i++;
                }
                if (p_2 < -0.1)
                {
                    i++;
                }
                if (p_3 <= 0)
                {
                    i++;
                }
                if (p_3 < -0.1)
                {
                    i++;
                }
                if (p_4 <= 0)
                {
                    i++;
                }
                if (p_4 < -0.1)
                {
                    i++;
                }
                return i / 100;
            }
            catch (Exception ex)
            {
                write3ColorLog(ex, "getFudongValue");
                return 0;
            }
        }

        /// <summary>
        /// �쳣��־��¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void write3ColorLog(Exception ex,string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "cl3Color-uc3Color-" + sFunc);
        }

        /// <summary>
        /// ��¼������¼
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="sql">������sql���</param>
        /// <param name="method">������ʽ</param>
        private void WriteEditLog(string sql, string method)
        {
            OracleConnection Conn = new OracleConnection(strConn);
            try
            {
                Conn.Open();
                OracleCommand cmd;
                string strExe = "insert into ������¼ values('" + user + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'��������:��ɫԤ��','������Ϣ" + sql.Replace('\'', '"') + "','" + method + "')";
                cmd = new OracleCommand(strExe, Conn);
                cmd.ExecuteNonQuery();

                Conn.Close();
            }
            catch
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }

        /// <summary>
        /// �Ƿ��ж�ͳ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        private void radioZD_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxZD.Enabled = radioZD.Checked;
        }
    }
}