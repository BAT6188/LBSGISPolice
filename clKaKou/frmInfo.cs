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
using System.Net;
using System.Net.Sockets;
using MapInfo.Windows.Controls;

namespace clKaKou
{
    public partial class FrmInfo : Form
    {
        Boolean UseDll = true;

        public string[] StrCon;         // ���ݿ������ַ�������
        public string UserName;         // ��½�û�����
        public string photoserver;      // ͼƬ��������ַ
        public MapControl mapControl;   // ��ͼ�ؼ�
        public string getFromNamePath;  // GetFromNameConfig.ini�ĵ�ַ
        private string mysql;           // �����ַ���
        public string layerName;        // ͼ������

        GDW_GIS_Interface.communication gdwcom = new GDW_GIS_Interface.communication(); //�ߵ����ӿ�

        public FrmInfo()
        {
            InitializeComponent();
        }
       
        public void setInfo(DataRow dRow,Point pt,string[] Constr,string un)
        {
            try
            {
                StrCon = Constr;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(Constr[0], Constr[1], Constr[2]);
                mysql = CLC.DatabaseRelated.OracleDriver.GetConString;
                UserName = un;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("�����ֶ�") < 0)
                    {
                        if (col.Caption.IndexOf("��Ƭ") < 0 && col.Caption !="��������" )
                        {
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                        }
                        else
                        {
                            if ((col.Caption == "��������" && dRow[col].ToString().Length > 0)|| (col.Caption == "��Ƭ1" && dRow[col].ToString().Length > 0) 
                                                                                              || (col.Caption == "��Ƭ2" && dRow[col].ToString().Length > 0)
                                                                                              || (col.Caption == "��Ƭ3" && dRow[col].ToString().Length > 0))
                            {
                                DataGridViewLinkCell dglc1 = new DataGridViewLinkCell();

                                dglc1.Value = "�鿴 " + col.Caption + " ����Ϣ";

                                if (col.Caption == "��������")
                                    dglc1.Value = dRow[col].ToString();

                                dglc1.ToolTipText =dRow[col].ToString();

                                this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);

                                this.dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[1] = dglc1;
                            }                           
                        }
                    }
                }

                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //����λ��

                int ki=0;
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].ToString().IndexOf("����״̬") > 0)
                    {
                        ki = i;
                    }
                }

                if (dRow.Table.Columns[0].Caption == "�������" && this.dataGridView1.Rows[ki].Cells[1].Value.ToString() != "�Ѵ���")
                {
                    this.panel3.Visible = true;
                }
                else
                {
                    this.panel3.Visible = false;
                }

                this.TopMost = true;

                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
            catch(Exception ex) {
                writeToLog(ex, "clKaKou-frmInfo-01-setInfo");
            }
        }

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
                cMessageHeight += 100;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //���ô�С
            }
            catch(Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-02-setSize");
            }
        }


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
                writeToLog(ex, "clKaKou-frmInfo-03-setLocation");
            }
        }


        /// <summary>
        /// �鿴ͼƬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��Ƭ1:") || (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��Ƭ2:") || (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��Ƭ3:"))
            {
                string photoip = string.Empty;

                if (this.dataGridView1.Rows[0].Cells[0].Value.ToString() == "�������:")
                {
                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString().IndexOf("��Ƭ") > -1)
                    {
                        photoip = this.dataGridView1.Rows[e.RowIndex].Cells[1].ToolTipText;
                    }
                }
                else if (dataGridView1.Rows[0].Cells[0].Value.ToString() == "ͨ�г������:")
                {
                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString().IndexOf("��Ƭ") > -1)
                    {
                        photoip = photoserver + this.dataGridView1.Rows[e.RowIndex].Cells[1].ToolTipText;
                    }
                }

                //Ϊ�˷�ֹ��ǰ�������д��ڲ������ַ�����ftp��������ַ����http��������ַ���ִ����ڴ˽��д����ַ�Replace��
                if (photoip.IndexOf("\\") > 0)
                {
                    photoip = photoip.Replace("\\", "/");
                }
               // writeToLog("photoip: " + photoip); 

                try
                {
                    FrmImage fimage = new FrmImage();
                    fimage.pictureBox1.Image = Image.FromFile(Application.StartupPath + "\\wait.gif");
                   // writelog("��ͼƬǰ��ͼƬ��������ַ photoip: " + photoip);
                    fimage.pictureBox1.Image = Image.FromStream(System.Net.WebRequest.Create(photoip).GetResponse().GetResponseStream());
                    fimage.TopMost = true;
                    fimage.username = this.UserName;
                    fimage.SQLCON = this.StrCon;
                    fimage.ShowDialog();

                    WriteEditLog("�鿴��Ƭ " + photoip, "�鿴��Ƭ", "��ϸ��Ϣ");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("��ͼƬʱ��������" + photoip, "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    writeToLog(ex, "clKaKou-frmInfo-04-�鿴ͼƬ��"+photoip);
                    return;
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "��������:")
            {
                System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=sdgis_cx;Password=!giscx123;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.47.227.29)(PORT=1521)))(CONNECT_DATA=(SID=sdgazyk)))");
                //System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection(mysql);
                if (oraconn.State == ConnectionState.Open)
                    oraconn.Close();
                try
                {

                   //                                                                                         ��ס����ϸ��ַ	��ס��������	����Ȩ	���εǼ�����	�����������	������Ч��ֹ	ǿ�Ʊ�����ֹ	��֤����	������	��������	����ʻ֤����	���Ǽ�֤������	����֤����	����ֹ����	������ƴ���	������ʻ֤����	��_����֤�����	�Ǽ�֤����	�ƵǼ�֤������	�������	����Ͻ��	������״̬	�Զ���״̬	��Ѻ��־	������	������Դ	ע����ˮ��	�������ͺ�	ȼ������	����	����	ת����ʽ	����	��������	��������	�����ڲ�����	�����ڲ����	�����ڲ��߶�	�ְ嵯��Ƭ��	����	���	ǰ�־�	���־�	��̥���	��̥��	������	��������	�˶�������	�˶��ؿ�����	׼ǣ������	��ʻ��ǰ���ؿ�����	��ʻ�Һ����ؿ�����	����������	��������	��÷�ʽ	����ƾ֤1	ƾ֤���1	����ƾ֤2	ƾ֤���2	���۵�λ	���ۼ۸�	��������	����ƾ֤	��ƾ֤���	�ϸ�֤���	��˰֤��	��˰֤�����																											
                   //XH as ���������,HPZL as ��������,	HPHM,���ƺ���,	CLPP1 as ����Ʒ��,	CLXH as �����ͺ�,	CLPP2 as Ӣ��Ʒ��,	GCJK as ����_����,	ZZG as �����,,	ZZCMC as ���쳧���� ,	CLSBDH as ����ʶ����� ,	FDJH as ��������,
                   // CLLX ��������,	CSYS ������ɫ,	SYXZ ʹ������,	SFZMHM ���֤������,	SFZMMC���֤����������,	SYR������������,	ZSXZQHס����������,	ZSXXDZ�Ǽ�ס����ϸ��ַ,	YZBM1ס����������,	LXDH��ϵ�绰,	ZZZ��ס��ס֤��,
                   //ZZXZQH��ס��������,	ZZXXDZ	YZBM2	SYQ	CCDJRQ	DJRQ	YXQZ	QZBFQZ	FZJG	GLBM	FPRQ	FZRQ	FDJRQ	FHGZRQ	BXZZRQ	BPCS	BZCS	BDJCS	DJZSBH	ZDJZSHS	DABH	XZQH	ZT	ZDYZT	DYBJ	JBR	CLLY	LSH	FDJXH	RLZL	PL	GL	ZXXS	CWKC	CWKK	CWKG	HXNBCD	HXNBKD	HXNBGD	GBTHPS	ZS	ZJ	QLJ	HLJ	LTGG	LTS	ZZL	ZBZL	HDZZL	HDZK	ZQYZL	QPZK	HPZK	HBDBQK	CCRQ	HDFS	LLPZ1	PZBH1	LLPZ2	PZBH2	XSDW	XSJG	XSRQ	JKPZ	JKPZHM	HGZBH	NSZM	NSZMBH	GXRQ	XGZL	QMBH	HMBH	BZ	JYW	WFCS	LJJF	SGCS	LJJJSS	CLYT	YTSX	DZYX	XSZBH	SJHM	JYHGBZBH	CYRY	DPHGZBH	SQDM	YXH	TYBIP	AZDM	ZDRUSJ	BJRUSJ	CSIP	CSBS	RMJYW

                    string fields = "XH as ���������,HPZL as ��������,	HPHM as ���ƺ���,	CLPP1 as ����Ʒ��,	CLXH as �����ͺ�,	CLPP2 as Ӣ��Ʒ��,	GCJK as ����_����,	ZZG as �����,	ZZCMC as ���쳧���� ,	CLSBDH as ����ʶ����� ,	FDJH as ��������," +
                    "CLLX as ��������,	CSYS as  ������ɫ,	SYXZ as ʹ������,	SFZMHM as ���֤������,	SFZMMC as ���֤����������,	SYR as ������������,	ZSXZQH as ס����������,	ZSXXDZ as �Ǽ�ס����ϸ��ַ,	YZBM1 as ס����������,	LXDH as ��ϵ�绰,	ZZZ as ��ס��ס֤�� ";
                    oraconn.Open();
                    OracleCommand cmd = new OracleCommand("Select "+ fields +" from gis_vehicle where HPHM='" + this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() + "'", oraconn);
                    OracleDataAdapter Adp = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    Adp.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows.Count >= 2)
                        {
                            frmCarNum frmnum = new frmCarNum(this.mapControl,this.StrCon,this.getFromNamePath,this.layerName,this.UserName);
                            frmnum.setfrmInfo(dt);
                            frmnum.Visible = false;
                            frmnum.ShowDialog();
                            return;
                        }
                        frmCarInfo frmcar = new frmCarInfo();
                        frmcar.setInfo(dt.Rows[0], this.StrCon, this.UserName);
                        frmcar.mapControl = this.mapControl;
                        frmcar.mysql = this.mysql;
                        frmcar.layerName = this.layerName;
                        frmcar.getFromNamePath = this.getFromNamePath;
                        frmcar.TopMost = true;
                        frmcar.Visible = false;
                        frmcar.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("��س�����δ�Ǽǣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                   
                }
                catch(Exception ex)
                {
                    writeToLog(ex, "clKaKou-frmInfo-dataGridView1_CellContentClick-�鿴����");
                }
                finally
                {
                    if (oraconn.State == ConnectionState.Open)
                        oraconn.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       /// <summary>
       /// �������
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {          
            try
            {
                string exedtl = string.Empty;

                frmExcut frmexec = new frmExcut();
                frmexec.ShowDialog(this);
                if (frmexec.DialogResult == DialogResult.OK)
                {
                    if (frmexec.ExDetail.Length > 50)
                    {
                        MessageBox.Show("���������������50�ַ�,ϵͳ�Զ�ȡǰ50���ַ���Ϊ������Ϣ���б���", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        exedtl = frmexec.ExDetail.Substring(0, 50);
                    }
                    else
                    {
                        exedtl = frmexec.ExDetail;
                    }
                }
                else
                {
                    MessageBox.Show("û��д�봦���������,ϵͳ�Զ�ȡ��ֵ��Ϊ������Ϣ���б���", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    exedtl = frmexec.ExDetail;
                }
               
                string sqlstr = "update V_ALARM set ����״̬ = '�Ѵ���',������='" + this.UserName + "',����ʱ��= to_date('" + System.DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),�������='" + exedtl + "' where ������� ='" + this.dataGridView1.Rows[0].Cells[1].Value.ToString() + "'";
                RunCommand(sqlstr);
                ////����dll
                if (UseDll == true)
                {  //xxxxxx 3
                    Boolean exc = gdwcom.AlarmWite(this.dataGridView1.Rows[0].Cells[1].Value.ToString(), this.UserName, System.DateTime.Now); 

                    if (exc == false)
                    {
                       // MessageBox.Show("���ΰ����ڷ�������д�봦������ʱ��������", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                WriteEditLog(sqlstr, "������", "V_ALARM");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-05-�������");
            }           
        }

        /// <summary>
        /// ��ѯSQL
        /// </summary>
        /// <param name="sql">��ѯ���</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// ִ��SQL
        /// </summary>
        /// <param name="sql">SQL���</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
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
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into ������¼ values('" + UserName + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'�ΰ�����','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-06-��¼������¼");
            }
        }
    }
}

