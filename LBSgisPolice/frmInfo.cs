using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.IO;
using MapInfo.Windows.Controls;
using clPopu;
using clZonghe;


namespace LBSgisPolice
{
    public partial class FrmInfo : Form
    {
        public FrmInfo()
        {
            InitializeComponent();
        }
        private string strConn;
        System.Data.OracleClient.OracleConnection myConnection = null;
        System.Data.OracleClient.OracleDataAdapter oracleDat = null;
        System.Data.OracleClient.OracleCommand oracleCmd = null;
        System.Data.OracleClient.OracleDataReader dataReader = null;
        DataSet objset = null;


        private string[] conStr;
        public MapControl mapControl = null;
        private string tabName;
        public DataRow row =null;
        public string getFromNamePath;
        public string LayerName;
        private string houseNo = "";   // ��÷��ݱ��
        private string[] dqPopu = null, lsPopu = null, yzPopu = null, wzPopu = null; // ������洢��ѯ�������������ѯ (��ǰ��ס��������ʷ��ס��������ס֤��Ч����������δ����ס֤����)

        /// <summary>
        /// ����features�����ݿ������ַ�����ȡ��Ϣ
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="ftr">��ͼҪ��</param>
        /// <param name="Con">���ݿ����Ӳ���</param>
        public void setInfo(Feature ftr,string[] Con) {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;

                this.dataGridView1.Rows.Clear();

                strConn = "user id=" + Con[1] + ";password=" + Con[2] + ";data source=" + Con[0];
                conStr = Con;

                //ͨ���鿴�ֶ�,�����"��_ID",˵������ʱ��
                bool isTemTab = false;
                foreach (MapInfo.Data.Column col in ftr.Columns)
                {
                    String upAlias = col.Alias.ToUpper();
                    if (upAlias.IndexOf("��_ID") > -1)
                    {
                        isTemTab = true;
                        break;
                    }
                }
                DataTable dt = null;
                if (isTemTab)
                {
                    string strTabName = ftr["����"].ToString();
                    if (strTabName.IndexOf("��Ƶ") >= 0)
                    {
                        strTabName = "��Ƶ";
                    }
                    //GetFromName getFromName = new GetFromName(strTabName);
                    CLC.ForSDGA.GetFromTable.GetFromName(strTabName, getFromNamePath);

                    string strSQL1 = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " 
                                               + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                               + CLC.ForSDGA.GetFromTable.ObjID + "='" 
                                               + ftr["��_ID"].ToString() + "'";

                    if (CLC.ForSDGA.GetFromTable.TableName == "��ȫ������λ")
                        strSQL1 = "select ���,��λ����,��λ����,��λ��ַ,���ܱ�������������,�ֻ�����,�����ɳ���,�����ж�,����������," + 
                                  "'����鿴' as ��ȫ������λ�ļ�,��ע��,��עʱ��,X,Y from " 
                                  + CLC.ForSDGA.GetFromTable.TableName + " t where " 
                                  + CLC.ForSDGA.GetFromTable.ObjID + "='" 
                                  + ftr["��_ID"].ToString() + "'";

                    OracleConnection Conn = new OracleConnection(strConn);
                    try
                    {
                        Conn.Open();
                        OracleCommand cmd = new OracleCommand(strSQL1, Conn);
                        cmd.ExecuteNonQuery();
                        OracleDataAdapter apt = new OracleDataAdapter(cmd);
                        dt = new DataTable();
                        //dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strSQL1);
                        apt.Fill(dt);
                        row = dt.Rows[0];
                        cmd.Dispose();
                        Conn.Close();

                        if (dt.Rows.Count > 0)
                        {
                            foreach (System.Data.DataColumn dataCol in dt.Columns)
                            {
                                if (dataCol.Caption.IndexOf("�����ֶ�") < 0 && dataCol.Caption != "�����ɳ�������" 
                                                                            && dataCol.Caption != "�����жӴ���" 
                                                                            && dataCol.Caption != "���������Ҵ���"
                                                                            && dataCol.Caption != "��ȡID" 
                                                                            && dataCol.Caption != "��ȡ����ʱ��"
                                                                            && dataCol.Caption != "��������"
                                                                            && dataCol.Caption != "��ż������ݺ���"
                                                                            && dataCol.Caption != "�������֤����" 
                                                                            && dataCol.Caption != "סַ����")
                                {
                                    if (dataCol.Caption == "��ذ���" || dataCol.Caption == "��ǰ��ס����" 
                                                                      || dataCol.Caption == "��ʷ��ס����" 
                                                                      || dataCol.Caption == "��ס֤��Ч��������" 
                                                                      || dataCol.Caption == "δ����ס֤����" 
                                                                      || dataCol.Caption == "�永��Ա" 
                                                                      || (dt.Columns[0].Caption != "���ݱ��" && dataCol.Caption == "���ݱ��"))
                                        this.dataGridView1.Rows.Add(dataCol.Caption + ":", "");
                                    else
                                        this.dataGridView1.Rows.Add(dataCol.Caption + ":", dt.Rows[0][dataCol]);
                                }
                                if (dataCol.Table.Columns[0].Caption == "���ݱ��")
                                    houseNo = dt.Rows[0]["���ݱ��"].ToString();
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch
                    {
                        if (Conn.State == ConnectionState.Open)
                            Conn.Close();
                        return;
                    }
                }
                else
                {
                    foreach (MapInfo.Data.Column col in ftr.Columns)
                    {
                        String upAlias = col.Alias.ToUpper();
                        if (upAlias != "OBJ" && upAlias != "MI_STYLE" 
                                             && upAlias != "MI_KEY" 
                                             && upAlias.IndexOf("�����ֶ�") < 0 
                                             && upAlias != "�����ɳ�������" 
                                             && upAlias != "�����жӴ���" 
                                             && upAlias != "���������Ҵ���"
                                             && upAlias != "��ȡID" 
                                             && upAlias != "��ȡ����ʱ��" 
                                             && upAlias != "��������")
                        {
                            this.dataGridView1.Rows.Add(new object[] { string.Format("{0}:", col.ToString()), string.Format("{0}", ftr[col.ToString()].ToString()) });
                        }
                    }
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, Control.MousePosition.X + 5, Control.MousePosition.Y + 5);           //����λ��
                this.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }

            #region ������ӣ��໥����
            string strPopu = "";   // ����ͳ�Ƶ�sql;
            try
            {
                DataGridViewLinkCell Datagvl = null;
                // �ҵ�Ҫ�����ֶε�λ��
                int name = 0, dq = 0, ls = 0, fwNo = 0, xanj = 0, yb = 0, wb = 0, anRen = 0, file = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    string objName = this.dataGridView1.Rows[i].Cells[0].Value.ToString();
                    if (objName == "����:")
                        name = i;
                    if (objName == "��ȫ������λ�ļ�:")
                        file = i;
                    if (objName == "��ذ���:")
                        xanj = i;
                    if (objName == "��ǰ��ס����:")
                    {
                        dq = i;
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        dqPopu = getPopuCount(strPopu, dqPopu);
                    }
                    if (objName == "��ʷ��ס����:")
                    {
                        ls = i;
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'";
                        lsPopu = getPopuCount(strPopu, lsPopu);
                    }
                    if (objName == "���ݱ��:")
                        fwNo = i;
                    if (objName == "��ס֤��Ч��������:")
                    {
                        yb = i;
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                   " and �˿�����='��ס�˿�' and ��ס֤�� is not null and ��ס֤��Ч����>sysdate";
                        yzPopu = getPopuCount(strPopu, yzPopu);
                    }
                    if (objName == "δ����ס֤����:")
                    {
                        wb = i;
                        strPopu = "select distinct ���֤���� from gis_fwrk s where " + ConversionStr(houseNo) + "='" + houseNo + "'" +
                                  " and �˿�����='��ס�˿�' and ((��ס֤�� is null) or (��ס֤�� is not null and ��ס֤��Ч����<sysdate))";
                        wzPopu = getPopuCount(strPopu, wzPopu);
                    }
                    if (objName == "�永��Ա:")
                        anRen = i;
                    if (objName == "��������:")
                    {
                        // ����������
                        Datagvl = new DataGridViewLinkCell();
                        Datagvl.Value = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                        Datagvl.ToolTipText = "�鿴������ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Datagvl;
                    }
                    if (objName == "��ż����:")
                    {
                        // ����������
                        Datagvl = new DataGridViewLinkCell();
                        Datagvl.Value = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                        Datagvl.ToolTipText = "�鿴��ż��ϸ��Ϣ";
                        this.dataGridView1.Rows[i].Cells[1] = Datagvl;
                    }
                }
                string TabName = ftr["����"].ToString();
                if (TabName == "�˿�ϵͳ")
                {
                    // ����������
                    Datagvl = new DataGridViewLinkCell();
                    Datagvl.Value = this.dataGridView1.Rows[0].Cells[1].Value.ToString();
                    Datagvl.ToolTipText = "�鿴��Ƭ";
                    this.dataGridView1.Rows[0].Cells[1] = Datagvl;

                    // ���ݱ�ż�����
                    DataGridViewLinkCell Datagvl2 = new DataGridViewLinkCell();
                    Datagvl2.Value = "����鿴��ط���";
                    Datagvl2.ToolTipText = "�鿴��ط�����Ϣ";
                    this.dataGridView1.Rows[fwNo].Cells[1] = Datagvl2;

                    // ��ذ���������
                    DataGridViewLinkCell Datagvl3 = new DataGridViewLinkCell();
                    Datagvl3.Value = "����鿴��ذ���";
                    Datagvl3.ToolTipText = "�鿴��ذ�����Ϣ";
                    this.dataGridView1.Rows[xanj].Cells[1] = Datagvl3;
                }
                if (TabName == "�����ݷ���ϵͳ")
                {
                    // ��ǰ��ס����������
                    Datagvl = new DataGridViewLinkCell();
                    if (dqPopu[0] == "" || dqPopu[0] == "0")
                        Datagvl.Value = "0";
                    else
                        Datagvl.Value = dqPopu.Length.ToString();
                    Datagvl.ToolTipText = "�鿴��ǰ��ס����Ϣ";
                    this.dataGridView1.Rows[dq].Cells[1] = Datagvl;

                    // ��ʷ��ס����������
                    DataGridViewLinkCell Datagvl2 = new DataGridViewLinkCell();
                    if (lsPopu[0] == "" || lsPopu[0] == "0")
                        Datagvl2.Value = "0";
                    else
                        Datagvl2.Value = lsPopu.Length.ToString();
                    Datagvl2.ToolTipText = "�鿴��ʷ��ס����Ϣ";
                    this.dataGridView1.Rows[ls].Cells[1] = Datagvl2;

                    // ��ס֤��Ч��������������
                    DataGridViewLinkCell Datagvl3 = new DataGridViewLinkCell();
                    if (yzPopu[0] == "" || yzPopu[0] == "0")
                        Datagvl3.Value = "0";
                    else
                        Datagvl3.Value = yzPopu.Length.ToString();
                    Datagvl3.ToolTipText = "�鿴��ס֤��Ч��������ϸ��Ϣ";
                    this.dataGridView1.Rows[yb].Cells[1] = Datagvl3;

                    // ���֤���������
                    DataGridViewLinkCell Datagvl4 = new DataGridViewLinkCell();
                    if (wzPopu[0] == "" || wzPopu[0] == "0")
                        Datagvl4.Value = "0";
                    else
                        Datagvl4.Value = wzPopu.Length.ToString();
                    Datagvl4.ToolTipText = "�鿴δ����ס֤����ϸ��Ϣ";
                    this.dataGridView1.Rows[wb].Cells[1] = Datagvl4;
                }
                if (TabName == "������Ϣ")
                {
                    // �永��Ա������
                    Datagvl = new DataGridViewLinkCell();
                    Datagvl.Value = "����鿴�永��Ա��Ϣ";
                    Datagvl.ToolTipText = "�鿴�永��Ա��Ϣ";
                    this.dataGridView1.Rows[anRen].Cells[1] = Datagvl;
                }
                if (TabName == "��ȫ������λ")
                {
                    // �永��Ա������
                    Datagvl = new DataGridViewLinkCell();
                    Datagvl.Value = "����鿴";
                    Datagvl.ToolTipText = "�鿴��ȫ������λ�ļ�";
                    this.dataGridView1.Rows[file].Cells[1] = Datagvl;
                }
            }
            catch( Exception ex)
            {
                ExToLog(ex, "������ӣ��໥����");
            }
            #endregion
        }

        /// <summary>
        /// ���ô����С
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void setSize() {
            try
            {
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].Width > 300)
                    {
                        this.dataGridView1.Columns[1].Width = 300;
                        this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    double width = this.dataGridView1.Columns[1].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView1.Rows[i].Cells[1].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView1.Rows[i].Height = (this.dataGridView1.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + 30;

                if (this.dataGridView1.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView1.Rows[i].Height;
                }
                cMessageHeight += 50;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //���ô�С
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setSize");
            }
        }

        /// <summary>
        /// ���ô�����ʾλ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="iHeight">����߶�</param>
        /// <param name="iWidth">������</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        private void setLocation(int iWidth,int iHeight,int x,int y) {
            try
            {
                if (x + iWidth > Screen.PrimaryScreen.WorkingArea.Width)
                {
                    x = x - iWidth - 10;
                }
                if (y + iHeight > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    y = y - iHeight - 10;
                    if (y < 0) y = 0;
                }
                this.SetDesktopLocation(x, y);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setLocation");
            }
        }

        /// <summary>
        /// ����sql��ѯ��Ų�������浽������   
        /// ���༭�� ����
        /// ���༭ʱ�� 2010-12-23
        /// </summary>
        /// <param name="poStr">sql���</param>
        /// <param name="conPo">���ڴ�ű�ŵ�����</param>
        /// <returns></returns>
        private string[] getPopuCount(string poStr, string[] conPo)
        {
            try
            {
                DataTable poDat = GetTable(poStr);

                if (poDat.Rows.Count > 0)
                    conPo = new string[poDat.Rows.Count];
                else   // ���û�в�ѯ������������һ����0��ֵ��������Ϊ��ֹ������ֵ�����쳣
                {
                    conPo = new string[1];
                    conPo[0] = "0";
                    return conPo;
                }

                for (int i = 0; i < poDat.Rows.Count; i++)
                {
                    conPo[i] = poDat.Rows[i][0].ToString();
                }

                return conPo;

            }
            catch (Exception ex)
            {
                ExToLog(ex, "getPopuCount");
                return conPo;
            }
        }

        /// <summary> 
        /// ������; ����ת�����ݱ�Ŵ���
        /// ���༭��   ����
        /// ���༭ʱ��  2010-12-23
        /// </summary>
        /// <param name="str">Ҫת���ı��</param>
        /// <returns>ת�����ַ���</returns>
        private string ConversionStr(string str)
        {
            try
            {
                string converStr = "";

                switch (str.Substring(0, 1))    // ��ȡ��ŵĵ�һ��Ӣ����ĸ�Ӷ��ж�����������
                {
                    case "a":
                        converStr = "����";
                        break;
                    case "b":
                        converStr = "�����ݱ��";
                        break;
                    case "c":
                        converStr = "סַ���ƴ���";
                        break;
                    case "d":
                        converStr = "��ҵ���";
                        break;
                    default:
                        break;
                }
                return converStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "ConversionStr");
                return "";
            }
        }

        private FrmZLMessage frmZL;
        private string[] strAnjan;
        private string tableName = "";
        /// <summary>
        /// �б����¼�
        /// ���༭��   ����
        /// ���༭ʱ��  2010-12-23
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                myConnection = new OracleConnection(strConn);
                if (strConn == "")
                {
                    MessageBox.Show("��ȡ�����ļ�ʱ��������,���޸������ļ�������!");
                    return;
                }
                if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "����:")
                {
                    int k = 0, r = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "���֤����:")
                        {
                            k = i;
                        }
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "�˿�����:")
                        {
                            r = i;
                        }
                    }

                    string connectionStr = "User ID=czrk_cx;" + 
                                           "Password=czrk_cx;" + 
                                           "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)" + 
                                                                    "(HOST=10.47.227.15)(PORT=1521)))" +
                                                                    "(CONNECT_DATA=(SID=ora81)))";

                    System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection(connectionStr);
                    try
                    {
                        clPopu.FrmImage fimage = new clPopu.FrmImage();
                        oraconn.Open();
                        string sqlstr = "";
                        if (dataGridView1.Rows[r].Cells[1].Value.ToString() == "��ס�˿�")
                            sqlstr = "select TX from czrk_cx.v_gis_czrk_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";
                        else
                            sqlstr = "select TX from czrk_cx.v_gis_ldry_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";

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
                        ExToLog(ex, "dataGridView1_CellContentClick-�鿴��Ƭ");
                    }
                    finally
                    {
                        if (oraconn.State == ConnectionState.Open)
                        {
                            oracleCmd.Dispose();
                            oraconn.Close();
                        }
                    }
                }

                try
                {
                    tabName = ""; strAnjan = null;

                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "�永��Ա:")
                        tabName = "�永��Ա:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ȫ������λ�ļ�:")
                        tabName = "��ȫ������λ�ļ�:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ذ���:")
                        tabName = "��ذ���:";
                    if (row.Table.Columns[0].Caption == "����" && dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "���ݱ��:")
                        tabName = "���ݱ��:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ǰ��ס����:")
                        tabName = "��ǰ��ס����:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ʷ��ס����:")
                        tabName = "��ʷ��ס����:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ס֤��Ч��������:")
                        tabName = "��ס֤��Ч��������:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "δ����ס֤����:")
                        tabName = "δ����ס֤����:"; 
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ż����:")
                        tabName = "��ż������ݺ���:";
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��������:")
                        tabName = "�������֤����:";

                }
                catch { }

                string strSQL = "", strMessage = "";
                try
                {
                    string colName = tabName.Substring(0, tabName.Length - 1);
                    switch (colName)
                    {
                        case "��ذ���":
                            string AnName = row[colName].ToString();
                            strAnjan = AnName.Split(',');
                            if (AnName != "")
                            {
                                AnName = AnName.Replace(",", "','");
                                strSQL = "select �������,�������� from ������Ϣ t where ������� in ('" + AnName + "')";
                            }
                            tableName = "������Ϣ";
                            strMessage = "���永��¼��";
                            break;
                        case "���ݱ��":
                            string houseNo = row[colName].ToString();
                            strAnjan = houseNo.Split(',');
                            if (houseNo != "")
                            {
                                houseNo = houseNo.Replace(",", "','");
                                strSQL = "select ���ݱ��,�������� from �����ݷ���ϵͳ t where ���ݱ�� in ('" + houseNo + "')";
                            }
                            tableName = "�����ݷ���ϵͳ";
                            strMessage = "����δ�浵��";
                            break;
                        case "�永��Ա":
                            string anRen = row[colName].ToString();
                            strAnjan = anRen.Split(',');
                            if (anRen != "")
                            {
                                anRen = anRen.Replace(",", "','");
                                strSQL = "select ���֤����,���� from �˿�ϵͳ t where ���֤���� in ('" + anRen + "')";
                            }
                            tableName = "�˿�ϵͳ";
                            string renName = colName = colName.Substring(0, colName.Length - 1);
                            strMessage = renName + "δ�浵��";
                            break;
                        case "��ǰ��ס����":
                        case "��ʷ��ס����":
                        case "��ס֤��Ч��������":
                        case "δ����ס֤����":
                            string cardId = arrayConStr(colName);
                            strAnjan = cardId.Split(',');
                            if (cardId != "")
                            {
                                cardId = cardId.Replace(",", "','");
                                strSQL = "select ���֤����,���� from �˿�ϵͳ t where ���֤���� in ('" + cardId + "')";
                            }
                            tableName = "�˿�ϵͳ";
                            string meName = colName = colName.Substring(0, colName.Length - 1);
                            strMessage = meName + "δ�浵��";
                            break;
                        case "��ȫ������λ�ļ�":
                            if (this.frmZL != null)
                            {
                                if (this.frmZL.Visible == true)
                                {
                                    this.frmZL.Close();
                                }
                            }

                            if (dataGridView1.Rows[1].Cells[1].Value.ToString() == "")
                            {
                                MessageBox.Show("���Ʋ���Ϊ�գ�", "��ʾ");
                                return;
                            }
                            this.frmZL = new FrmZLMessage(dataGridView1.Rows[1].Cells[1].Value.ToString(), strConn);
                            //������Ϣ�������½�  
                            System.Drawing.Point po = new Point();
                            po.X = Screen.PrimaryScreen.WorkingArea.Width;
                            po.Y = Screen.PrimaryScreen.WorkingArea.Height;
                            this.frmZL.SetDesktopLocation(po.X - frmZL.Width, po.Y - frmZL.Height);
                            this.frmZL.Show();
                            break;
                        case "��ż������ݺ���":
                        case "�������֤����":
                            tableName = "�˿�ϵͳ";
                            GetAnjanInfo();
                            strMessage = "δ��ѯ���˼�¼����ȷ�����ݿ������֤�����Ƿ���ȷ!";
                            break;
                    }

                    if (strSQL != "")
                    {
                        myConnection.Open();
                        objset = new DataSet();
                        oracleDat = new OracleDataAdapter(strSQL, myConnection);
                        oracleDat.Fill(objset, "table");

                        if (objset.Tables.Count != 0)
                        {
                            if (objset.Tables[0].Rows.Count == 0)
                            {
                                MessageBox.Show(strMessage, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (tabName != "" && tabName != "��ȫ������λ�ļ�:" && tabName != "��ż������ݺ���:" && tabName != "�������֤����:")
                            MessageBox.Show(strMessage, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    if (strAnjan.Length > 1)
                        disPlayInfo(objset.Tables[0], tableName);
                }
                catch (Exception ex)
                {
                    ExToLog(ex, "dataGridView1_CellContentClick");
                }
                finally
                {
                    if (strSQL != "")
                        oracleDat.Dispose();
                    if (myConnection.State == ConnectionState.Open)
                        myConnection.Close();
                    try
                    {
                        // ���ֻ��һ����¼��ֱ����ʾ��Ϣ
                        if (strAnjan != null && strAnjan.Length == 1 && strAnjan[0] != "")
                            GetAnjanInfo();
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick-�б����¼�");
            }
        }

        /// <summary>
        /// ���������ҵ��ַ�������ת��Ϊ�ɹ�SQLʹ�õ��ַ���
        /// ���༭��   ����
        /// ���༭ʱ��  2010-12-23
        /// </summary>
        /// <param name="colName">����</param>
        /// <returns>�����ַ���</returns>
        private string arrayConStr(string colName)
        {
            try
            {
                string[] constrArray = null;
                switch (colName)
                {
                    case "��ǰ��ס����":
                        constrArray = dqPopu;
                        break;
                    case "��ס֤��Ч��������":
                        constrArray = yzPopu;
                        break;
                    case "��ʷ��ס����":
                        constrArray = lsPopu;
                        break;
                    case "δ����ס֤����":
                        constrArray = wzPopu;
                        break;
                    default:
                        break;
                }

                string arrayStr = "";

                if (constrArray.Length > 0)     // �������еı��ת�����ö��ŷָ����ַ���
                {
                    for (int j = 0; j < constrArray.Length; j++)
                    {
                        if (arrayStr == "")
                            arrayStr = constrArray[j];
                        else
                            arrayStr += "," + constrArray[j];
                    }
                }
                else
                {
                    arrayStr = "";
                }

                return arrayStr;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "arrayConStr");
                return "";
            }
        }

        FrmHouseNumber frmNumber = new FrmHouseNumber();
        /// <summary>
        /// ��ʾ�б�
        /// ���༭��   ����
        /// ���༭ʱ��  2010-12-23
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="tableName">����</param>
        private void disPlayInfo(DataTable dt,string tableName)
        {
            try
            {
                if (dt == null)
                {
                    frmNumber.Close();
                    return;
                }

                if (this.frmNumber.Visible == false)
                {
                    frmNumber = new FrmHouseNumber();
                    frmNumber.Show();
                    frmNumber.Visible = true;
                }
                frmNumber.mapControl = mapControl;
                frmNumber.getFromNamePath = getFromNamePath;
                frmNumber.LayerName = LayerName;
                frmNumber.strConn = strConn;
                frmNumber.setfrmInfo(dt, tableName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo-��ʾ�б�");
            }
        }

        /// <summary>
        /// ������¼�Ĳ�ѯ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        private void GetAnjanInfo()
        {
            string strSQL = "", tableName = "";
            try
            {
                string colName = tabName.Substring(0, tabName.Length - 1);
                switch (colName)
                {
                    case "��ذ���": 
                        CLC.ForSDGA.GetFromTable.GetFromName("������Ϣ",getFromNamePath);
                        string AnNumber = row[colName].ToString();
                        if (AnNumber != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.FrmFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + AnNumber + "'";
                        tableName = "������Ϣ";
                        break;
                    case "���ݱ��":
                        CLC.ForSDGA.GetFromTable.GetFromName("�����ݷ���ϵͳ",getFromNamePath);
                        string houseNo = row[colName].ToString();
                        if (houseNo != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.FrmFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + houseNo + "'";
                        tableName = "�����ݷ���ϵͳ";
                        break;
                    case "�永��Ա":
                    case "��ʷ��ס����":
                    case "��ǰ��ס����":
                    case "��ס֤��Ч��������":
                    case "δ����ס֤����": 
                        CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ",getFromNamePath);
                        string cardId = row[colName].ToString();
                        if (cardId != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.FrmFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + cardId + "'";
                        tableName = "�˿�ϵͳ";
                        break;
                    case "��ż������ݺ���":
                    case "�������֤����":
                        CLC.ForSDGA.GetFromTable.GetFromName("�˿�ϵͳ", getFromNamePath);
                        string sfzID = row[colName].ToString();
                        if (sfzID != "")
                            strSQL = "select " + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + sfzID + "'";
                        break;
                }

                if (strSQL != "")
                {
                    myConnection.Open();
                    objset = new DataSet();
                    oracleDat = new System.Data.OracleClient.OracleDataAdapter(strSQL, myConnection);
                    oracleDat.Fill(objset, "table");
                }
                else
                {
                    return;
                }

                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("�����¼δ�浵��", "��ʾ");
                    return;
                }
                else
                {
                    Point pt = new Point();
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);

                    disPlayInfo(objset.Tables[0], pt,tableName);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "GetAnjanInfo-������¼�Ĳ�ѯ");
            }
            finally
            {
                oracleDat.Dispose();
                myConnection.Close();
            }
        }
        private FrmHouseInfo frmglMessage = new FrmHouseInfo();
        /// <summary>
        /// ��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="pt">λ��</param>
        /// <param name="tableName">����</param>
        private void disPlayInfo(DataTable dt, Point pt,string tableName)
        {
            try
            {
                if (this.frmglMessage.Visible == false)
                {
                    this.frmglMessage = new FrmHouseInfo();
                    frmglMessage.Show();
                    frmglMessage.Visible = true;
                }
                frmglMessage.mapControl = mapControl;
                frmglMessage.getFromNamePath = getFromNamePath;
                frmglMessage.LayerName = LayerName;
                frmglMessage.strConn = strConn;
                frmglMessage.setInfo(dt.Rows[0], pt, tableName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo-��ʾ��ϸ��Ϣ");
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
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPolice-FrmInfo-" + sFunc);
        }

        /// <summary>
        /// ��ȡ��ѯ�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="sql">Ҫִ�е�SQL</param>
        /// <returns>����ڴ��</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(conStr[0], conStr[1], conStr[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }
    }
}

