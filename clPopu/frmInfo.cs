using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.IO;
using clInfo;
using System.Text.RegularExpressions;


namespace clPopu
{
    public partial class FrmInfo : Form
    {
        public string strConn;
        public MapInfo.Windows.Controls.MapControl mapControl = null;
        private DataRow row = null;
        public string getFromNamePath;
        private string LayerName;
        private string[] StrCon;

        public FrmInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ����dt��ȡ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="dRow">��¼��</param>
        /// <param name="pt">������ʾλ��</param>
        /// <param name="LayName">ͼ������</param>
        public void setInfo(DataRow dRow,Point pt,string LayName)
        {
            try
            {
                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                row = dRow;
                LayerName = LayName;
                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("�����ֶ�") < 0 && col.Caption != "�����ɳ�������" 
                                                            && col.Caption != "�����жӴ���" 
                                                            && col.Caption != "���������Ҵ���"
                                                            && col.Caption != "��ȡID" 
                                                            && col.Caption != "��ȡ����ʱ��" 
                                                            && col.Caption != "��������" 
                                                            && col.Caption != "��ż������ݺ���" 
                                                            && col.Caption != "סַ����")
                    {
                        if (col.Caption == "�永��Ա" || col.Caption == "��ذ���" 
                                                      || (col.Caption == "���ݱ��" && col.Table.Columns[0].Caption != "���ݱ��") 
                                                      || col.Caption == "��ǰ��ס����" 
                                                      || col.Caption == "��ʷ��ס����" 
                                                      || col.Caption == "��ס֤��Ч��������" 
                                                      || col.Caption == "δ����ס֤����")
                            this.dataGridView1.Rows.Add(col.Caption + ":", "");
                        else
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                    }
                }
                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //����λ��

                #region ������ӣ��໥����
                string[] hoNum = null, anNum = null;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    #region ����ǰ����
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "����:")
                    //{
                    //    //k = i;
                    //    //�������ϼ�����,�ɲ鿴��Ƭ
                    //    DataGridViewLinkCell dglc = new DataGridViewLinkCell();
                    //    dglc.Value = dRow[dRow.Table.Columns[i]];
                    //    dglc.ToolTipText = "�鿴��Ƭ";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc;
                    //    continue;
                    //}
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "���ݱ��:")
                    //{
                    //    hoNum = new string[] { };
                    //    hoNum = row["���ݱ��"].ToString().Split(',');
                    //    //�鿴��������
                    //    DataGridViewLinkCell dglc1 = new DataGridViewLinkCell();
                    //    dglc1.Value = "�鿴������ϸ��Ϣ";
                    //    dglc1.ToolTipText = "�鿴������ϸ��Ϣ";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc1;
                    //    continue;
                    //}
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ذ���:")
                    //{
                    //    anNum = new string[] { };
                    //    anNum = row["��ذ���"].ToString().Split(',');
                    //    DataGridViewLinkCell dglc2 = new DataGridViewLinkCell();
                    //    dglc2.Value = "�鿴��ذ�����Ϣ";
                    //    dglc2.ToolTipText = "�鿴��ذ�����Ϣ";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc2;
                    //    continue;
                    //}
                    //if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "��ż����:")
                    //{
                    //    //����ż�����ϼ�����,�ɲ鿴��ż��Ϣ
                    //    DataGridViewLinkCell dglc = new DataGridViewLinkCell();
                    //    dglc.Value = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                    //    dglc.ToolTipText = "�鿴��ż��Ϣ";
                    //    this.dataGridView1.Rows[i].Cells[1] = dglc;
                    //    continue;
                    //}
                    #endregion

                    string valueRow=this.dataGridView1.Rows[i].Cells[0].Value.ToString();
                    switch (valueRow)
                    {
                        case "����:":
                            setDataGridLinkCell(i, dRow[dRow.Table.Columns[i]].ToString(), "�鿴��Ƭ");
                            break;
                        case "���ݱ��:":
                            hoNum = new string[] { };
                            hoNum = row["���ݱ��"].ToString().Split(',');
                            setDataGridLinkCell(i, "�鿴������ϸ��Ϣ", "�鿴������ϸ��Ϣ");
                            break;
                        case "��ذ���:":
                            anNum = new string[] { };
                            anNum = row["��ذ���"].ToString().Split(',');
                            setDataGridLinkCell(i, "�鿴��ذ�����Ϣ", "�鿴��ذ�����Ϣ");
                            break;
                        case "��ż����:":
                            setDataGridLinkCell(i, this.dataGridView1.Rows[i].Cells[1].Value.ToString(), "�鿴��ż��Ϣ");
                            break;
                    }
                }
                #endregion

                this.Visible = true;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setInfo");
            }
        }

        /// <summary>
        /// ������ӵķ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="s">Ҫ��ӵļ�¼��</param>
        /// <param name="valueName">������ʾֵ</param>
        /// <param name="toolText">������ʾToolText</param>
        private void setDataGridLinkCell(int s, string valueName, string toolText)
        {
            try {
                DataGridViewLinkCell dglc = new DataGridViewLinkCell();
                dglc.Value = valueName;
                dglc.ToolTipText = toolText;
                this.dataGridView1.Rows[s].Cells[1] = dglc;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setDataGridLinkCell");
            }
        }

        /// <summary>
        /// ���ô����С
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void setSize()
        {
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
        /// ���ô���λ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="iWidth">������</param>
        /// <param name="iHeight">����߶�</param>
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

        private FrmHouseNumber frmhn = new FrmHouseNumber();

        /// <summary>
        /// ����б�����ʵ�ֹ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            try
            {
                System.Data.OracleClient.OracleCommand oracleCmd = null;
                System.Data.OracleClient.OracleDataReader dataReader = null;
                System.Data.OracleClient.OracleDataAdapter objoda = null;
                System.Data.DataSet objset = new System.Data.DataSet();
                string[] strHouse = null, Anjan = null;

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
                    System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=czrk_cx;" +
                                                                                                                      "Password=czrk_cx;" + 
                                                                                                                      "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)" + 
                                                                                                                                               "(HOST=10.47.227.15)(PORT=1521)))" +
                                                                                                                                               "(CONNECT_DATA=(SID=ora81)))");
                    try
                    {
                        FrmImage fimage = new FrmImage();
                        string sqlstr = ""; 
                        if(dataGridView1.Rows[r].Cells[1].Value.ToString()=="��ס�˿�")
                            sqlstr = "select TX from czrk_cx.v_gis_czrk_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";
                        else
                            sqlstr = "select TX from czrk_cx.v_gis_ldry_tx where ZJHM='" + this.dataGridView1.Rows[k].Cells[1].Value.ToString() + "'";

                        oraconn.Open();
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
                                bmp.Save(Application.StartupPath + "\\popu.jpg");
                                fimage.pictureBox1.ImageLocation = Application.StartupPath + "\\popu.jpg";
                                bmp.Dispose();//�ͷ�bmp�ļ���Դ
                                fimage.pictureBox1.Invalidate();
                                fs.Close();
                                fimage.TopMost = true;
                                fimage.Visible = false;
                                fimage.ShowDialog();
                                fimage.Dispose();
                                File.Delete(Application.StartupPath + "\\popu.jpg");
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("��Ƭδ�浵!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExToLog(ex, "dataGridView1_CellContentClick-��ʾ��Ƭ");
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
                else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "���ݱ��:")
                {
                    string RenHouse = "�鿴������ϸ��Ϣ";

                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() == RenHouse)
                    {
                        try
                        {
                            if (frmhn.dataGridView1.Rows.Count != 0)
                            {
                                frmhn.dataGridView1.Rows.Clear();
                            }
                            myConnection.Open();
                            strHouse = row["���ݱ��"].ToString().Split(',');
                            string strHouseNumber = row["���ݱ��"].ToString();
                            strHouseNumber = strHouseNumber.Replace(",", "','");
                            string sql = "select ���ݱ��,�������� from �����ݷ���ϵͳ where ���ݱ�� in('" + strHouseNumber + "')";
                            objoda = new OracleDataAdapter(sql, myConnection);
                            objoda.Fill(objset, "table");
                            if (objset.Tables[0].Rows.Count == 0)
                            {
                                MessageBox.Show("����δ�浵��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }

                            if (strHouse.Length > 1)
                                disPlayNumberInfo(objset.Tables[0], "�����ݷ���");

                        }
                        catch (Exception ex)
                        {
                            ExToLog(ex, "dataGridView1_CellContentClick--��ʾ������¼");
                        }
                        finally
                        {
                            objoda.Dispose();
                            myConnection.Close();
                            try
                            {
                                // �ж��Ƿ�ֻ��һ�䷿��
                                if (strHouse.Length == 1 && strHouse[0] != "")
                                    onlyFirst(strHouse[0], "�����ݷ���");
                            }
                            catch (Exception ex) { ExToLog(ex, "dataGridView1_CellContentClick--��ʾ������¼"); }
                        }
                    }
                }
                else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ذ���:")
                {
                    string RenHouse = "�鿴��ذ�����Ϣ";

                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() == RenHouse)
                    {
                        try
                        {
                            if (frmhn.dataGridView1.Rows.Count != 0)
                            {
                                frmhn.dataGridView1.Rows.Clear();
                            }
                            myConnection.Open();
                            Anjan = row["��ذ���"].ToString().Split(',');
                            string AnjanNumber = row["��ذ���"].ToString();
                            AnjanNumber = AnjanNumber.Replace(",", "','");
                            string sql = "select �������,�������� from ������Ϣ where ������� in('" + AnjanNumber + "')";
                            objoda = new OracleDataAdapter(sql, myConnection);
                            objoda.Fill(objset, "table");
                            if (objset.Tables[0].Rows.Count == 0)
                            {
                                MessageBox.Show("����δ�浵��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            }

                            if (Anjan.Length > 1)
                                disPlayNumberInfo(objset.Tables[0], "������Ϣ");

                        }
                        catch (Exception ex)
                        {
                            ExToLog(ex, "dataGridView1_CellContentClick--��ʾ������¼");
                        }
                        finally
                        {
                            objoda.Dispose();
                            myConnection.Close();
                            try
                            {
                                // �ж��Ƿ�ֻ��һ�䷿��
                                if (Anjan.Length == 1 && Anjan[0] != "")
                                    onlyFirst(Anjan[0], "������Ϣ");
                            }
                            catch (Exception ex) { ExToLog(ex, "dataGridView1_CellContentClick--��ʾһ����¼"); }
                        }
                    }
                }
                else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��ż����:")
                {
                    onlyFirst(row["��ż������ݺ���"].ToString(), "�˿�ϵͳ");
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "dataGridView1_CellContentClick");
            }
        }


        /// <summary>
        /// ��ֻ��һ�䷿��ʱֱ����ʾ������Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="strHouse">���ݱ��</param>
        /// <param name="tabName">����</param>
        private void onlyFirst(string strHouse,string tabName)
        {
            System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(strConn);
            System.Data.OracleClient.OracleDataAdapter objoda = null;
            System.Data.DataSet objset = new System.Data.DataSet();
            try
            {
                myConnection.Open();
                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                string sqlFields = CLC.ForSDGA.GetFromTable.SQLFields;
                string strSQL = "select " + sqlFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " t where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + strHouse + "'";
                objoda = new OracleDataAdapter(strSQL, myConnection);
                objoda.Fill(objset, "table");

                if (objset.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("��ȷ�ϱ���Ƿ���ȷ��", "ϵͳ��ʾ", MessageBoxButtons.OK);
                    return;
                }

                DataRow row = objset.Tables[0].Rows[0];

                System.Drawing.Point pt = new System.Drawing.Point();
                if (objset.Tables[0].Rows[0]["X"] != null && objset.Tables[0].Rows[0]["Y"] != null && objset.Tables[0].Rows[0]["X"].ToString() != "" && objset.Tables[0].Rows[0]["Y"].ToString() != "")
                {
                    pt.X = Convert.ToInt32(objset.Tables[0].Rows[0]["X"]);
                    pt.Y = Convert.ToInt32(objset.Tables[0].Rows[0]["Y"]);
                }

                disPlayInfo(objset.Tables[0], pt,tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "onlyFirst");
            }
            finally
            {
                objoda.Dispose();
                myConnection.Close();
            }
        }

        //private FrmHouseNumber frmNumeber = new FrmHouseNumber();
        //private void disPlayNumberInfo(DataTable dt, string tabName)
        //{
        //    try
        //    {
        //        if (this.frmNumeber.Visible == false)
        //        {
        //            this.frmNumeber = new FrmHouseNumber();
        //            frmNumeber.Show();
        //            frmNumeber.Visible = true;
        //        }
        //        frmNumeber.mapControl = mapControl;
        //        frmNumeber.LayerName = LayerName;
        //        frmNumeber.getFromNamePath = getFromNamePath;
        //        frmNumeber.strConn = strConn;
        //        frmNumeber.setfrmInfo(dt, tabName);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExToLog(ex, "disPlayNumberInfo");
        //    }
        //}

        /// <summary>
        /// ��ʾ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="tabName">����</param>
        private void disPlayNumberInfo(DataTable dt, string tabName)
        {
            try
            {
                frmNumber fmNum = new frmNumber();
                if (fmNum.Visible == false)
                {
                    fmNum = new frmNumber();
                    fmNum.Show();
                    fmNum.Visible = true;
                }
                fmNum.mapControl = mapControl;
                fmNum.LayerName = LayerName;
                fmNum.getFromNamePath = getFromNamePath;
                fmNum.StrCon = StrCon;
                fmNum.setInfo(dt, tabName);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayNumberInfo");
            }
        }

        //private FrmHouseInfo frmhouse = new FrmHouseInfo();
        //private void disPlayInfo(DataTable dt, System.Drawing.Point pt,string tabName)
        //{
        //    try
        //    {
        //        if (this.frmhouse.Visible == false)
        //        {
        //            this.frmhouse = new FrmHouseInfo();
        //            frmhouse.SetDesktopLocation(-30, -30);
        //            frmhouse.Show();
        //            frmhouse.Visible = false;
        //        }
        //        frmhouse.mapControl = mapControl;
        //        frmhouse.LayerName = LayerName;
        //        frmhouse.getFromNamePath = getFromNamePath;
        //        frmhouse.strConn = strConn;
        //        frmhouse.setInfo(dt.Rows[0], pt, tabName);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExToLog(ex, "disPlayInfo");
        //    }
        //}

        /// <summary>
        /// ��ʾ��ϸ��Ϣ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="dt">����Դ</param>
        /// <param name="pt">��ʾλ��</param>
        /// <param name="tabName">����</param>
        private void disPlayInfo(DataTable dt, System.Drawing.Point pt, string tabName)
        {
            try
            {
                frmInfo fmIn = new frmInfo();
                if (fmIn.Visible == false)
                {
                    fmIn = new frmInfo();
                    fmIn.SetDesktopLocation(-30, -30);
                    fmIn.Show();
                    fmIn.Visible = false;
                }
                fmIn.mapControl = mapControl;
                fmIn.LayerName = LayerName;
                fmIn.getFromNamePath = getFromNamePath;
                fmIn.StrCon = StrCon;
                fmIn.setInfo(dt.Rows[0], pt);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "disPlayInfo");
            }
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-26
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFuns">������</param>
        private void ExToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clPopu-FrmInfo-" + sFuns);
        }
    }
}

