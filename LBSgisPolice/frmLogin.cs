using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace LBSgisPolice
{
    public partial class frmLogin : Form
    {
        public static string string�û����� = "";
        public static string stringϽ�� = "˳����";
        
        //����14����������洢��ǰ�û�����Щ�ɳ����в���Ȩ�� (added by fisher in 10-03-04)
        private string pcsZH = "";     //�ۺϲ�ѯ    
        private string pcsAJ = "";     //��������   
        private string pcsCL = "";     //�������
        private string pcsSP = "";     //��Ƶ���
        private string pcsZA = "";     //�ΰ�����
        private string pcsRK = "";     //�˿ڹ���
        private string pcsFW = "";     //���ݹ���
        private string pcsZG = "";     //ֱ��ָ��
        private string pcsGPS = "";    //GPS��Ա
        private string pcs110 = "";    //110�Ӿ�
        private string pcsJSJ = "";    //�������ݱ༭
        private string pcsYSJ = "";    //ҵ�����ݱ༭
        private string pcsSB = "";     //��Ƶ�༭
        private string pcsQX = "";     //Ȩ�޹���
        private string pcsDC = "";     //�ɵ���

        //����14����������洢��ǰ�û�����Щ�ж��в���Ȩ��
        private string zdZH = "";
        private string zdAJ = "";
        private string zdCL = "";
        private string zdSP = "";
        private string zdZA = "";
        private string zdRK = "";
        private string zdFW = "";
        private string zdZG = "";
        private string zdGPS = "";        //GPS��Ա
        private string zd110 = "";        //110�Ӿ�
        private string zdJSJ = "";
        private string zdYSJ = "";
        private string zdSB = "";
        private string zdQX = "";
        private string zdDC = "";

        //public string userType="";
        public string userName = "";
        public static DataTable temDt = null;
        public static DataTable temRegionDt = null;  // �����洢��ѯ����fisher��
        public static DataTable temEditDt = null;    // �����洢�༭ģ���Ȩ�ޣ�Lili��

        public static string region1 = "";       //�����洢���б༭ģ��Ȩ�޵��ɳ�����Lili��
        public static string region2 = "";       //�����洢���б༭ģ��Ȩ�޵��жӣ�Lili��

        public frmLogin()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ��ȡ�����ļ����������ݿ������ַ���
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <returns>���ݿ������ַ���</returns>
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                string userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                string password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);

                string connString = "user id = " + userid + ";data source = " + datasource + ";password =" + password;
                return connString;
            }
            catch( Exception ex) {
                ExToLog(ex, "��ȡ�����ļ�");
                return "";
            }
        }

        /// <summary>
        /// ��¼����¼�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string strConn=getStrConn();
            if (strConn == "")
            {
                MessageBox.Show("��ȡ�����ļ�ʱ��������,���޸������ļ�������!");
                return;
            }

            try
            {
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from �û� where USERNAME='" + textUserName.Text.Trim().ToLower() + "'");                
               
                if (dr.HasRows)
                {
                    dr.Read();
                    if (textPW.Text == dr["PASSWORD"].ToString())
                    {
                        this.DialogResult = DialogResult.OK;
                        userName = textUserName.Text.Trim();
                        frmLogin.string�û����� = this.textUserName.Text.Trim();
                        setTemDt(dr["��ɫ"].ToString());����������������// �������Ȩ��
                        setTemEditDt(userName, dr["��ɫ"].ToString());  // �༭ģ���Ȩ��
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("�������");
                    }
                }
                else
                {
                    MessageBox.Show("���û������ڣ�");
                }
                dr.Close();
            }
            catch(Exception ex) {
                ExToLog(ex,"��½ϵͳ");
                MessageBox.Show("��������,���ݿ�����,�����ļ��е����ݿ�����!" + ex.Message);
            }
        }

        /// <summary>
        /// ȡ����¼���رմ���
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// �����༭ģ��Ȩ���ڴ�� �� ����Ȩ���ַ���
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="uname">�û���</param>
        /// <param name="roles">��ɫ��</param>
        private void setTemEditDt(string uname, string roles)
        {
            try
            {
                temEditDt = new DataTable("temQuanX");
                DataColumn dc = new DataColumn("userNow");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                //��         edit by fisher in 10-02-26
                dc = new DataColumn("�������ݿɱ༭");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("ҵ�����ݿɱ༭");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("��Ƶ�ɱ༭");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("Ͻ��");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("���������ݿ�ɾ����");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("��ҵ�����ݿ�ɾ����");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("�ɵ���");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                dc = new DataColumn("�ɵ���");
                dc.DataType = System.Type.GetType("System.String");
                temEditDt.Columns.Add(dc);

                //��
                DataRow dRow;
                dRow = temEditDt.NewRow();
                dRow[0] = uname;
                dRow[1] = "0";
                dRow[2] = "0";
                dRow[5] = "0";
                dRow[6] = "0";
                dRow[7] = "0"; 
                dRow[8] = "0";
                temEditDt.Rows.Add(dRow);

                //��ȡ��ɫ������dt������
                OracleDataReader dr = null;
                string[] aRole = roles.Split(',');
                for (int i = 0; i < aRole.Length; i++)  //�ж��Ƿ�ɱ༭�Ϳɲ�����Ͻ��
                {
                    dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from ��ɫ where ��ɫ�� = '" + aRole[i] + "'");
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            if (dr["ģ��Ȩ��"].ToString().IndexOf("���������ݿ�ɾ����") > -1)
                            {
                                temEditDt.Rows[0]["���������ݿ�ɾ����"] = "1";
                            }
                            if (dr["ģ��Ȩ��"].ToString().IndexOf("��ҵ�����ݿ�ɾ����") > -1)
                            {
                                temEditDt.Rows[0]["��ҵ�����ݿ�ɾ����"] = "1";
                            }
                            if (dr["ģ��Ȩ��"].ToString().IndexOf("�������ݱ༭") > -1)
                            {
                                temEditDt.Rows[0]["�������ݿɱ༭"] = "1";
                            }
                            if (dr["ģ��Ȩ��"].ToString().IndexOf("ҵ�����ݱ༭") > -1)
                            {
                                temEditDt.Rows[0]["ҵ�����ݿɱ༭"] = "1";
                            }
                            if (dr["ģ��Ȩ��"].ToString().IndexOf("��Ƶ�༭") > -1)
                            {
                                temEditDt.Rows[0]["��Ƶ�ɱ༭"] = "1";
                            } 
                            if (dr["ģ��Ȩ��"].ToString().IndexOf("�ɵ���") > -1)
                            {
                                temEditDt.Rows[0]["�ɵ���"] = "1";
                            }
                            if (dr["ģ��Ȩ��"].ToString().IndexOf("�ɵ���") > -1)
                            {
                                temEditDt.Rows[0]["�ɵ���"] = "1";
                            }
                            temEditDt.Rows[0]["Ͻ��"] += dr["����Ȩ��"].ToString() + ",";     //���������ظ�
                        }
                        dr.Close();
                    }
                }
                temEditDt.Rows[0]["Ͻ��"] = temEditDt.Rows[0]["Ͻ��"].ToString().Remove(temEditDt.Rows[0]["Ͻ��"].ToString().LastIndexOf(','));
                string[] quyu = temEditDt.Rows[0]["Ͻ��"].ToString().Split(',');
                string xiaqu = "";
                for (int i = 0; i < quyu.Length; i++)
                {
                    if (xiaqu.IndexOf(quyu[i]) < 0)   //�������ظ����ַ���
                    {
                        xiaqu += quyu[i] + ",";
                    }
                }
                xiaqu = xiaqu.Remove(xiaqu.LastIndexOf(','));   //�����õ�Ͻ����û���ظ�������ַ���
                temEditDt.Rows[0]["Ͻ��"] = xiaqu;
                string[] quyu1 = xiaqu.Split(',');
                if (xiaqu.IndexOf("˳��������") > -1)
                {
                    region1 = "˳����";
                }
                else
                {
                    for (int i = 0; i < quyu1.Length; i++)
                    {
                        if (quyu1[i].Length == 2)     // �ɳ�����ֻ��������Ϊ�ɳ�����
                        {
                            region1 += quyu1[i] + ",";
                        }
                        else������������������������  // �жӣ��жӲ�����ֻ�������֣�
                        {
                            region2 += quyu1[i] + ",";
                        }
                    }
                    if (region1 != "")
                    {
                        region1 = region1.Remove(region1.LastIndexOf(','));
                    }
                    if (region2 != "")
                    {
                        region2 = region2.Remove(region2.LastIndexOf(','));
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// ����ģ��Ȩ���ڴ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="roles">��ɫ��</param>
        private void setTemDt(string roles)
        {
            //��ȡ��ɫ������,����dt�ı�ṹ
            try
            {
                temDt = new DataTable("role");
                DataColumn dc = new DataColumn("region");
                //ָ���ֶε��������ͣ��ⲽû��Ҳ�������
                dc.DataType = System.Type.GetType("System.String");
                temDt.Columns.Add(dc);

                //��
                string[] func ={"�ۺϲ�ѯ","��������","�������","��Ƶ���","�ΰ�����","�˿ڹ���","���ݹ���","ֱ��ָ��","GPS��Ա","llo�Ӿ�","�������ݱ༭","ҵ�����ݱ༭","��Ƶ�༭","Ȩ�޹���","�ɵ���" };
                for (int i = 0; i < func.Length; i++)
                {
                    //��������������е��������
                    dc = new DataColumn(func[i]);
                    //ָ���ֶε��������ͣ��ⲽû��Ҳ�������
                    dc.DataType = System.Type.GetType("System.Int16");
                    temDt.Columns.Add(dc);
                }

                //��
                DataRow dRow;
                dRow = temDt.NewRow();
                dRow[0] = "˳��������";
                for (int i = 1; i <= func.Length; i++) {
                    dRow[i] = 0;
                }
                temDt.Rows.Add(dRow);
                OracleDataReader dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select �ɳ����� from �����ɳ��� group by �ɳ�����");
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        dRow = temDt.NewRow();
                        dRow["region"] = dr.GetValue(0).ToString().Trim();
                        for (int i = 1; i <= func.Length; i++)
                        {
                            dRow[i] = 0;
                        }
                        temDt.Rows.Add(dRow);
                    }
                    dr.Close();
                }

                OracleDataReader dr1 = CLC.DatabaseRelated.OracleDriver.OracleComReader("select �ж��� from �������ж� group by �ж���");
                if (dr1.HasRows)
                {
                    while (dr1.Read())
                    {
                        DataRow dRow1 = temDt.NewRow();
                        dRow1["region"] = dr1.GetValue(0).ToString().Trim();
                        for (int i = 1; i <= func.Length; i++)
                        {
                            dRow1[i] = 0;
                        }
                        temDt.Rows.Add(dRow1);
                    }
                    dr1.Close();
                }

                //��ȡ��ɫ������,����dt������
                string[] aRole = roles.Split(',');
                for (int i = 0; i < aRole.Length; i++)
                {
                    dr = CLC.DatabaseRelated.OracleDriver.OracleComReader("select * from ��ɫ where ��ɫ�� = '" + aRole[i] + "'");
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            string[] a = dr["����Ȩ��"].ToString().Split(',');
                            for (int j = 0; j < a.Length; j++)
                            {
                                int rIndex = getIndex(a[j]);
                                string[] b = dr["ģ��Ȩ��"].ToString().Split(',');
                                for (int k = 0; k < b.Length; k++)
                                {
                                    //b[k] = b[k].Replace("110", "llo");
                                    if (b[k].IndexOf("��ɾ����") < 0 && b[k].IndexOf("�ɵ���") < 0)      //���ڱ༭ģ���Ժ�Ҫ�ϲ�����ϵͳ�У�����Ľӿ������Ԥ�����Ⱦ��巽����������������fisher in 10-03-01��
                                    {
                                        if (Convert.ToInt16(temDt.Rows[rIndex][b[k]]) == 0)
                                        {
                                            temDt.Rows[rIndex][b[k]] = 1;
                                        }
                                    }
                                }
                            }
                        }
                        dr.Close();
                    }
                    else
                    {
                        dr.Close();
                    }
                }
                //��ȡ��ʱ��temDt�����ݣ�����temRegionDt
                setRegionDt();
            }
            catch(Exception ex)
            {
                ExToLog(ex, "����Ȩ����ʱ��");
            }
        }

        /// <summary>
        /// ��ȡ��ʱ��temDt�����ݣ�����temRegionDt
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        private void setRegionDt()
        {
            try
            {
                initialStr("�ۺϲ�ѯ", ref pcsZH, ref zdZH);
                initialStr("��������", ref pcsAJ, ref zdAJ);
                initialStr("�������", ref pcsCL, ref zdCL);
                initialStr("��Ƶ���", ref pcsSP, ref zdSP);
                initialStr("�ΰ�����", ref pcsZA, ref zdZA);
                initialStr("�˿ڹ���", ref pcsRK, ref zdRK);
                initialStr("���ݹ���", ref pcsFW, ref zdFW);
                initialStr("ֱ��ָ��", ref pcsZG, ref zdZG);
                initialStr("GPS��Ա", ref pcsGPS, ref zdGPS);
                initialStr("llo�Ӿ�", ref pcs110, ref zd110);
                initialStr("�������ݱ༭", ref pcsJSJ, ref zdJSJ);
                initialStr("ҵ�����ݱ༭", ref pcsYSJ, ref zdYSJ);
                initialStr("��Ƶ�༭", ref pcsSB, ref zdSB);
                initialStr("Ȩ�޹���", ref pcsQX, ref zdQX);
                initialStr("�ɵ���", ref pcsDC, ref zdDC);

                temRegionDt = new DataTable("popedom");
                DataColumn dc = new DataColumn("zone");
                dc.DataType = System.Type.GetType("System.String");
                temRegionDt.Columns.Add(dc);

                //��
                string[] func ={ "�ۺϲ�ѯ", "��������", "�������", "��Ƶ���", "�ΰ�����", "�˿ڹ���", "���ݹ���", "ֱ��ָ��", "GPS��Ա","llo�Ӿ�", "�������ݱ༭","ҵ�����ݱ༭","��Ƶ�༭", "Ȩ�޹���" ,"�ɵ���"};
                for (int i = 0; i < func.Length; i++)
                {
                    //��������������е��������
                    dc = new DataColumn(func[i]);
                    //ָ���ֶε��������ͣ��ⲽû��Ҳ�������
                    dc.DataType = System.Type.GetType("System.String");
                    temRegionDt.Columns.Add(dc);
                }

                //��
                DataRow drow = temRegionDt.NewRow();
                drow["zone"] = "�ɳ���";
                temRegionDt.Rows.Add(drow);
                drow = temRegionDt.NewRow();
                drow["zone"] = "�ж�";
                temRegionDt.Rows.Add(drow);

                //����temRegionDt�������
                temRegionDt.Rows[0]["�ۺϲ�ѯ"] = pcsZH;         temRegionDt.Rows[1]["�ۺϲ�ѯ"] = zdZH;
                temRegionDt.Rows[0]["��������"] = pcsAJ;         temRegionDt.Rows[1]["��������"] = zdAJ;
                temRegionDt.Rows[0]["�������"] = pcsCL;         temRegionDt.Rows[1]["�������"] = zdCL;
                temRegionDt.Rows[0]["��Ƶ���"] = pcsSP;         temRegionDt.Rows[1]["��Ƶ���"] = zdSP;
                temRegionDt.Rows[0]["�ΰ�����"] = pcsZA;         temRegionDt.Rows[1]["�ΰ�����"] = zdZA;
                temRegionDt.Rows[0]["�˿ڹ���"] = pcsRK;         temRegionDt.Rows[1]["�˿ڹ���"] = zdRK;
                temRegionDt.Rows[0]["���ݹ���"] = pcsFW;         temRegionDt.Rows[1]["���ݹ���"] = zdFW;
                temRegionDt.Rows[0]["ֱ��ָ��"] = pcsZG;         temRegionDt.Rows[1]["ֱ��ָ��"] = zdZG;
                temRegionDt.Rows[0]["GPS��Ա"] = pcsGPS;         temRegionDt.Rows[1]["GPS��Ա"] = zdGPS;
                temRegionDt.Rows[0]["llo�Ӿ�"] = pcs110;         temRegionDt.Rows[1]["llo�Ӿ�"] = zd110;    
                temRegionDt.Rows[0]["�������ݱ༭"] = pcsJSJ;    temRegionDt.Rows[1]["�������ݱ༭"] = zdJSJ;
                temRegionDt.Rows[0]["ҵ�����ݱ༭"] = pcsYSJ;    temRegionDt.Rows[1]["ҵ�����ݱ༭"] = zdYSJ;
                temRegionDt.Rows[0]["��Ƶ�༭"] = pcsSB;         temRegionDt.Rows[1]["��Ƶ�༭"] = zdSB;
                temRegionDt.Rows[0]["Ȩ�޹���"] = pcsQX;         temRegionDt.Rows[1]["Ȩ�޹���"] = zdQX;
                temRegionDt.Rows[0]["�ɵ���"] = pcsDC;           temRegionDt.Rows[1]["�ɵ���"] = zdDC;
            }
            catch(Exception ex)
            {
                ExToLog(ex,"�����û�Ȩ��");
            }
        }

        //  (fisher)
        /// <summary>
        /// ���º���ּ�ڳ�ʼ���û��ܹ���ѯ��Ͻ��
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="mkStr">ģ������</param>
        /// <param name="pcsStr">�ɳ���Ȩ��</param>
        /// <param name="zdStr">�ж�Ȩ��</param>
        private void initialStr(string mkStr,ref string pcsStr,ref string zdStr)  //��ַ����
        {
            try
            {
                if (temDt.Select(mkStr + " = 1").Length > 0)
                {
                    if (temDt.Rows[0][mkStr].ToString() == "1")
                    {
                        pcsStr = "˳����";
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)    //ȷ��10���ɳ�����ӵ�еĲ�ͬģ���Ȩ��
                        {
                            if (temDt.Rows[i][mkStr].ToString() == "1")
                            {
                                pcsStr += temDt.Rows[i]["region"] + ",";
                            }
                        }

                        if (pcsStr != "") { pcsStr = pcsStr.Remove(pcsStr.LastIndexOf(",")); }
                        for (int j = 11; j < temDt.Rows.Count; j++)    //ȷ��n���ж���ӵ�еĲ�ͬģ���Ȩ��
                        {
                            if (temDt.Rows[j][mkStr].ToString() == "1")
                            {
                                zdStr += temDt.Rows[j]["region"] + ",";
                            }
                        }
                        if (zdStr != "") { zdStr = zdStr.Remove(zdStr.LastIndexOf(",")); }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "��ʼ���û���ѯ��Ͻ��");
            }
        }

        /// <summary>
        /// ��ȡ�ַ���P���û�Ȩ���ڴ�������ڵ��к�
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="p">p</param>
        /// <returns>�к�</returns>
        private int getIndex(string p)
        {
            for (int i = 0; i < temDt.Rows.Count; i++)
            {
                if (temDt.Rows[i]["region"].ToString() == p)
                {
                    return i;
                }
            }
            return -1;
        }        

        /// <summary>
        /// �쳣��־
        /// �������� ����
        /// ������ʱ�� 2011-1-24
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "Main-frmLogin-" + sFunc);
        }
    }
}