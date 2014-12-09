using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using System.Windows.Forms;
using System.IO;
using System.Web;
using System.Data;
using System.Xml;

namespace cl3Color
{

    class clsCal3Color
    {
        private double a1, a2, a3, a4, a0;
        private double BFBa1, BFBa2, BFBa3, BFBa4, BFBa0;
        private int  xazs, dqqc, dmtc, lrdq, fcqd, ybqd, qj,sh, zp, zazs,db,djdo,tq,xd,zs;
        private double  xazs2, dqqc2, dmtc2, lrdq2, fcqd2, ybqd2, qj2, sh2, zp2, zazs2, db2, djdo2, tq2, xd2, zs2;
        private DateTime timeFrom;
        private DateTime timeTo;
        private bool jz, pcs;
        private int period;
        public bool isFudong = false;

        public clsCal3Color(DateTime timeFrom, DateTime timeTo,bool isJZ,int pd,bool isPCS)
        {
            this.timeFrom = timeFrom;
            this.timeTo = timeTo;
            this.jz = isJZ;
            this.pcs = isPCS;
            this.period = pd;
        }

        public double A1
        {
            get { return a1; }
            set { a1 = value; }
        }

        public double A2
        {
            get { return a2; }
            set { a2 = value; }
        }

        public double A3
        {
            get { return a3; }
            set { a3 = value; }
        }

        public double A4
        {
            get { return a4; }
            set { a4 = value; }
        }

        public double A0
        {
            get { return a0; }
            set { a0 = value; }
        }

        public double BFBA1
        {
            get { return BFBa1; }
            set { BFBa1 = value; }
        }

        public double BFBA2
        {
            get { return BFBa2; }
            set { BFBa2 = value; }
        }

        public double BFBA3
        {
            get { return BFBa3; }
            set { BFBa3 = value; }
        }

        public double BFBA4
        {
            get { return BFBa4; }
            set { BFBa4 = value; }
        }

        public double BFBA0
        {
            get { return BFBa0; }
            set { BFBa0 = value; }
        }

        public int XAZS
        {
            get { return xazs; }
            set { xazs = value; }
        }

        public int DQQC
        {
            get { return dqqc; }
            set { dqqc = value; }
        }

        public int DMTC
        {
            get { return dmtc; }
            set { dmtc = value; }
        }

        public int LRDQ
        {
            get { return lrdq; }
            set { lrdq = value; }
        }

        public int FCQD
        {
            get { return fcqd; }
            set { fcqd = value; }
        }

        public int YBQD
        {
            get { return ybqd; }
            set { ybqd = value; }
        }

        public int QJ
        {
            get { return qj; }
            set { qj = value; }
        }

        public int SH
        {
            get { return sh; }
            set { sh = value; }
        }

        public int ZP
        {
            get { return zp; }
            set { zp = value; }
        }

        public int ZAZS
        {
            get { return zazs; }
            set { zazs = value; }
        }

        public int DB
        {
            get { return db; }
            set { db = value; }
        }
        public int DJDO
        {
            get { return djdo; }
            set { djdo = value; }
        }
                public int TQ
        {
            get { return tq; }
            set { tq = value; }
        }
                public int XD
        {
            get { return xd; }
            set { xd = value; }
        }
                public int ZS
        {
            get { return zs; }
            set { zs = value; }
        }

        public double XAZS2
        {
            get { return xazs2; }
            set { xazs2 = value; }
        }

        public double DQQC2
        {
            get { return dqqc2; }
            set { dqqc2 = value; }
        }

        public double DMTC2
        {
            get { return dmtc2; }
            set { dmtc2 = value; }
        }

        public double LRDQ2
        {
            get { return lrdq2; }
            set { lrdq2 = value; }
        }

        public double FCQD2
        {
            get { return fcqd2; }
            set { fcqd2 = value; }
        }

        public double YBQD2
        {
            get { return ybqd2; }
            set { ybqd2 = value; }
        }

        public double QJ2
        {
            get { return qj2; }
            set { qj2 = value; }
        }

        public double SH2
        {
            get { return sh2; }
            set { sh2 = value; }
        }

        public double ZP2
        {
            get { return zp2; }
            set { zp2 = value; }
        }

        public double ZAZS2
        {
            get { return zazs2; }
            set { zazs2 = value; }
        }

        public double DB2
        {
            get { return db2; }
            set { db2 = value; }
        }
        public double DJDO2
        {
            get { return djdo2; }
            set { djdo2 = value; }
        }
        public double TQ2
        {
            get { return tq2; }
            set { tq2 = value; }
        }
        public double XD2
        {
            get { return xd2; }
            set { xd2 = value; }
        }
        public double ZS2
        {
            get { return zs2; }
            set { zs2 = value; }
        }

        /// <summary>
        /// ��ȡ�����ļ����������ݿ������ַ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <returns>�����ַ���</returns>
        private string getStrConn()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("���ݿ�", "����Դ");
                string userid = CLC.INIClass.IniReadValue("���ݿ�", "�û���");
                string password = CLC.INIClass.IniReadValuePW("���ݿ�", "����");

                string connString = "user id = " + userid + ";data source = " + datasource + ";password =" + password;
                return connString;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "getStrConn");
                return null;
            }
        }

        /// <summary>
        /// ����ֵ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="str"></param>
        public void setParameter(string str)
        {
            try
            {
                TimeSpan ts =timeTo - timeFrom ;
                TimeSpan ts1 = new TimeSpan(ts.Days, 0, 0, 0);
                TimeSpan ts2 = new TimeSpan(ts.Days * 2, 0, 0, 0);
                TimeSpan ts3 = new TimeSpan(ts.Days * 3, 0, 0, 0);
                TimeSpan ts4 = new TimeSpan(ts.Days * 4, 0, 0, 0);

                if (isFudong)
                {
                    this.a1 = this.Cal3Color(timeFrom.Subtract(ts1), timeTo.Subtract(ts1), str);
                    this.a2 = this.Cal3Color(timeFrom.Subtract(ts2), timeTo.Subtract(ts2), str);
                    this.a3 = this.Cal3Color(timeFrom.Subtract(ts3), timeTo.Subtract(ts3), str);
                }
                this.a0 = this.Cal3Color(timeFrom, timeTo, str);
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setParameter");
            }
        }

        /// <summary>
        /// �����ɳ������ж�ͳ��ĳһʱ��ε����з�����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="timeFrom">��ʼʱ��</param>
        /// <param name="timeTo">����ʱ��</param>
        /// <param name="str">�ɳ������жӴ���</param>
        /// <returns>������</returns>
        private double Cal3Color(DateTime timeFrom, DateTime timeTo, string str)
        {
            try
            {
                string pcsSQL = "";
                if (str != "")
                {
                    if (pcs)
                    {
                        pcsSQL = " and �����ɳ�������='" + str + "'";
                    }
                    else {
                        pcsSQL = " and �����жӴ���='" + str + "'";
                    }
                    
                }
                double dCycleValue, dCycleValue2;
                int dayCount;
                OracelData oracleData = new OracelData(this.getStrConn());
                TimeSpan ts = timeTo - timeFrom;
                TimeSpan ts2 = new TimeSpan(ts.Days * 3, 0, 0, 0);
                dayCount = ts.Days * 3;
                DateTime dt1 = timeFrom.Subtract(ts2);      //��������ǰ��ʱ��

                string condStr = "where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and ��������='����' " + pcsSQL;
                
                //string strExp = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and ��������='����' " + pcsSQL;
                string strExp = addOption("count(*)", condStr);
                xazs = oracleData.getCount(strExp);

                condStr = "where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and ��������='����' " + pcsSQL;

                //strExp = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and ��������='����' " + pcsSQL;
                strExp = addOption("count(*)", condStr);
                double xa2 = oracleData.getCount(strExp);

                dCycleValue = getCycleValue(pcsSQL, "��������='����'");

                condStr = "where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and (��������='�ΰ�' or �������� = '����') " + pcsSQL;   //edit by fisher in 10-01-07
                
                //strExp = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and (��������='�ΰ�' or �������� = '����') " + pcsSQL;   //edit by fisher in 10-01-07
                strExp = addOption("count(*)", condStr);
                zazs = oracleData.getCount(strExp);

                condStr = "where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and (��������='�ΰ�' or �������� = '����') " + pcsSQL;      //��ѯ�ΰ�����ʱ������������
                
                //strExp = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss')  and (��������='�ΰ�' or �������� = '����') " + pcsSQL;      //��ѯ�ΰ�����ʱ������������
                strExp = addOption("count(*)", condStr);
                double za2 = oracleData.getCount(strExp);

                dCycleValue2 = getCycleValue(pcsSQL, "(��������='�ΰ�' or �������� = '����')");

                if (period == 2)
                {
                    xazs2 = xa2 / 3.0 * 0.5 + dCycleValue * 0.5;
                    zazs2 = za2 / 3.0 * 0.5 + dCycleValue2 * 0.5;
                }
                else
                {
                    xazs2 = xa2 / 3.0 * 0.7 + dCycleValue * 0.3;
                    zazs2 = za2 / 3.0 * 0.7 + dCycleValue2 * 0.3;
                }
                return (xazs-xazs2) / xazs2 * 0.7 + (zazs-zazs2) / zazs2 * 0.3;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "Cal3Color");
                return 0;
            }
        }

        /// <summary>
        /// ����ĳ���ɳ���ͳ�ƽ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="str">�ɳ�������</param>
        private void setData(string str)
        {
            try
            {
                if (str != "") {
                    str = " and �����ɳ�������='" + str + "'";
                }
                double dCycleValue = 0.0, dCycleValue2 = 0.0;

                OracelData oracleData = new OracelData(this.getStrConn());
                //xazs2 = 0; dqqc2 = 0; dmtc2 = 0; fcqd2 = 0; lrdq2 = 0; ybqd2 = 0; qj2 = 0; sh2 = 0; zp2 = 0;
                //zazs2 = 0; db2 = 0; djdo2 = 0; tq2 = 0; xd2 = 0;zs2 = 0;
                //this.xazs = 0; this.zazs = 0; this.dqqc =0; this.dmtc = 0; this.fcqd =0;
                //this.lrdq = 0; this.ybqd =0; this.qj = 0; this.sh=0; this.zp = 0;
                //this.zazs=0; this.db=0; this.djdo=0; this.tq=0; this.xd=0; this.zs = 0;
                TimeSpan ts = timeTo - timeFrom;
                TimeSpan ts2 = new TimeSpan(ts.Days, 0, 0, 0);
                DateTime dt1 = timeFrom.Subtract(ts2);//��������ǰ��ʱ

                string condStr = "where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')" + str + " and ��������='����'";
                
                //string strExp = "select ����_���� from ������Ϣ where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')" + str + " and ��������='����'";
                string strExp = addOption("����_����", condStr);
                DataTable dt = oracleData.SelectDataBase(strExp);
                dCycleValue = getCycleValue(str, "��������='����'");

                //���ܸ������ֵ
                this.dqqc = dt.Select("����_���� like '%��������%'").Length;
                this.dmtc = dt.Select("����_���� like '%����Ħ�г�%'").Length;
                this.fcqd = dt.Select("����_���� like '%�ɳ�����%'").Length;
                this.lrdq = dt.Select("����_���� like '%���ҵ���%' or ����_���� like '%���Ա��չ�%'").Length;
                this.ybqd = dt.Select("����_���� like '%����%' and ����_���� <> '�ɳ����᰸'").Length;
                this.qj = dt.Select("����_���� like '%����%'").Length;
                this.sh = dt.Select("����_���� like '%�˺�%'").Length;
                this.zp = dt.Select("����_���� like '%թƭ%'").Length;

                condStr = "where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')" + str + " and (��������='�ΰ�' or �������� = '����')";
               
                //strExp = "select ����_���� from ������Ϣ where ����ʱ���ֵ>=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeTo.ToString() + "','yyyy-mm-dd hh24:mi:ss')" + str + " and (��������='�ΰ�' or �������� = '����')";
                strExp = addOption("����_����", condStr);
                DataTable dt4 = oracleData.SelectDataBase(strExp);
                dCycleValue2 = getCycleValue(str, "(��������='�ΰ�' or �������� = '����')");

                this.db = dt4.Select("����_���� like '%�Ĳ�%'").Length;
                this.djdo = dt4.Select("����_���� like '%��ܶ�Ź%'").Length;
                this.tq = dt4.Select("����_���� like '%͵��%' or ����_���� like '%�������г�%'").Length;
                this.xd = dt4.Select("����_���� like '%����%'").Length;
                this.zs = dt4.Select("����_���� like '%����%' or ����_���� like '%������ٲ���%'").Length;

                condStr = "where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') " + str + " and ��������='����'";
                
                //strExp = "select ����_���� from ������Ϣ where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') " + str + " and ��������='����'";
                strExp = addOption("����_����", condStr);
                DataTable dt2 = oracleData.SelectDataBase(strExp);
                dCycleValue = getCycleValue(str, "��������='����'");

                dqqc2 = dt2.Select("����_���� like '%��������%'").Length;
                dmtc2 = dt2.Select("����_���� like '%����Ħ�г�%'").Length;
                fcqd2 = dt2.Select("����_���� like '%�ɳ�����%'").Length;
                lrdq2 = dt2.Select("����_���� like '%���ҵ���%' or ����_���� like '%���Ա��չ�%'").Length;
                ybqd2 = dt2.Select("����_���� like '%����%' and ����_���� <> '�ɳ����᰸'").Length;
                qj2 = dt2.Select("����_���� like '%����%'").Length;
                sh2 = dt2.Select("����_���� like '%�˺�%'").Length;
                zp2 = dt2.Select("����_���� like '%թƭ%'").Length;

                condStr = "where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') " + str + " and (��������='�ΰ�' or �������� = '����')";
                
                strExp = "select ����_���� from ������Ϣ where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') " + str + " and (��������='�ΰ�' or �������� = '����')";
                strExp = addOption("����_����", condStr);
                DataTable dt3 = oracleData.SelectDataBase(strExp);
                dCycleValue2 = getCycleValue(str, "(��������='�ΰ�' or �������� = '����')");

                db2 = dt3.Select("����_���� like '%�Ĳ�%'").Length;
                djdo2 = dt3.Select("����_���� like '%��ܶ�Ź%'").Length;
                tq2 = dt3.Select("����_���� like '%͵��%' or ����_���� like '%�������г�%'").Length;
                xd2 = dt3.Select("����_���� like '%����%'").Length;
                zs2 = dt3.Select("����_���� like '%����%' or ����_���� like '%������ٲ���%'").Length;

                //strExp1 = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ��������='����'"+str;
                //strExp1 = addOption(strExp1);
                //xazs2 = oracleData.getCount(strExp1);//�̰�����

                //strExp2 = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + dt1.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<=to_date('" + timeFrom.ToString() + "','yyyy-mm-dd hh24:mi:ss') and ��������='�ΰ�'"+str;
                //strExp2 = addOption(strExp2);
                //zazs2 = oracleData.getCount(strExp2);//�ΰ�����

                //���������ֵ
                if (period == 2)
                {
                    dqqc2 = (dqqc2 / 3.0) * 0.5 + dCycleValue * 0.5;
                    dmtc2 = (dmtc2 / 3.0) * 0.5 + dCycleValue * 0.5;
                    fcqd2 = (fcqd2 / 3.0) * 0.5 + dCycleValue * 0.5;
                    lrdq2 = (lrdq2 / 3.0) * 0.5 + dCycleValue * 0.5;
                    ybqd2 = (ybqd2 / 3.0) * 0.5 + dCycleValue * 0.5;
                    qj2 = (qj2 / 3.0) * 0.5 + dCycleValue * 0.5;
                    sh2 = (sh2 / 3.0) * 0.5 + dCycleValue * 0.5;
                    zp2 = (zp2 / 3.0) * 0.5 + dCycleValue * 0.5;

                    db2 = (db2 / 3.0) * 0.5 + dCycleValue2 * 0.5;
                    djdo2 = (djdo2 / 3.0) * 0.5 + dCycleValue2 * 0.5;
                    tq2 = (tq2 / 3.0) * 0.5 + dCycleValue2 * 0.5;
                    xd2 = (xd2 / 3.0) * 0.5 + dCycleValue2 * 0.5;
                    zs2 = (zs2 / 3.0) * 0.5 + dCycleValue2 * 0.5;
                }
                else
                {
                    //xazs2 = (xazs2 / 3) * 0.7 + dCycleValue * 0.3;
                    //zazs2 = (zazs2 / 3) * 0.7 + dCycleValue * 0.3;
                    dqqc2 = (dqqc2 / 3.0) * 0.7 + dCycleValue * 0.3;
                    dmtc2 = (dmtc2 / 3.0) * 0.7 + dCycleValue * 0.3;
                    fcqd2 = (fcqd2 / 3.0) * 0.7 + dCycleValue * 0.3;
                    lrdq2 = (lrdq2 / 3.0) * 0.7 + dCycleValue * 0.3;
                    ybqd2 = (ybqd2 / 3.0) * 0.7 + dCycleValue * 0.3;
                    qj2 = (qj2 / 3.0) * 0.7 + dCycleValue * 0.3;
                    sh2 = (sh2 / 3.0) * 0.7 + dCycleValue * 0.3;
                    zp2 = (zp2 / 3.0) * 0.7 + dCycleValue * 0.3;

                    db2 = (db2 / 3.0) * 0.7 + dCycleValue2 * 0.3;
                    djdo2 = (djdo2 / 3.0) * 0.7 + dCycleValue2 * 0.3;
                    tq2 = (tq2 / 3.0) * 0.7 + dCycleValue2 * 0.3;
                    xd2 = (xd2 / 3.0) * 0.7 + dCycleValue2 * 0.3;
                    zs2 = (zs2 / 3.0) * 0.7 + dCycleValue2 * 0.3;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "setData");
            }
        }

        private string addOption(string strExp)
        {
            try
            {
                if (jz)
                {
                    strExp += " and ������Դ='����'";
                }
                else
                {
                    strExp += " and ������Դ='110'";
                }
                return strExp;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "addOption");
                return null;
            }
        }

        /// <summary>
        /// �����û�ѡ�񡮾��ۡ���110�������SQL
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="colName">Ҫ��ѯ����</param>
        /// <param name="_whereStr">��ѯ����</param>
        /// <returns>SQL���</returns>
        private string addOption(string colName, string _whereStr)
        {
            try
            {
                string strExp = "";
                if (jz)
                {
                    strExp = "select " + colName + " from ������Ϣ " + _whereStr;
                }
                else
                {
                    strExp = "select " + colName + " from gps110.������Ϣ110 " + _whereStr;
                }
                return strExp;
            }
            catch (Exception ex)
            {
                ExToLog(ex, "addOption");
                return null;
            }
        }

        /// <summary>
        /// �������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="str"></param>
        public void getData(string str)
        {
            this.setParameter(str);
            this.setData(str);
        }

        /// <summary>
        /// ��ȡ����Ԥ��ֵ[����Ԥ��ֵ��(2008��ȫ��Ӿ���/365) * ��������(7��14)]
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="str">�ɳ������ж�����</param>
        /// <param name="Antype">������������</param>
        /// <returns>����Ԥ��ֵ</returns>
        private double getCycleValue(string str,string Antype)
        {
            int iYear = timeFrom.Year;
            int lastYear = iYear - 1;

            int iMonth = timeFrom.Month;
            int iDay = timeFrom.Day;
            int iEMonth = timeTo.Month;
            int iEDay = timeTo.Day;
            OracleConnection Conn = new OracleConnection(getStrConn());
            try
            {
                Conn.Open();
                string strExp = "";
                string condStr = "";
                OracleCommand cmd = Conn.CreateCommand();
                if (period == 2)
                {
                    condStr = "where ����ʱ���ֵ>=to_date('" + lastYear.ToString() + "-" + iMonth.ToString() + "-" + iDay.ToString() + " 00:00:00','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + iYear.ToString() + "-" + iMonth.ToString() + "-" + iDay.ToString() + " 00:00:00','yyyy-mm-dd hh24:mi:ss') " + str + " and " + Antype;
                   
                    //strExp = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + lastYear.ToString() + "-" + iMonth.ToString() + "-" + iDay.ToString() + " 00:00:00','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + iYear.ToString() + "-" + iMonth.ToString() + "-" + iDay.ToString() + " 00:00:00','yyyy-mm-dd hh24:mi:ss') " + str + " and " + Antype;
                    strExp = addOption("count(*)", condStr);
                    cmd.CommandText = strExp;
                }
                else
                {
                    condStr = "where ����ʱ���ֵ>=to_date('" + lastYear.ToString() + "-01-01 00:00:00','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + iYear.ToString() + "-01-01 00:00:00','yyyy-mm-dd hh24:mi:ss') " + str + " and " + Antype;
                    
                    //strExp = "select count(*) from ������Ϣ where ����ʱ���ֵ>=to_date('" + lastYear.ToString() + "-01-01 00:00:00','yyyy-mm-dd hh24:mi:ss') and ����ʱ���ֵ<to_date('" + iYear.ToString() + "-01-01 00:00:00','yyyy-mm-dd hh24:mi:ss') " + str + " and " + Antype;
                    strExp = addOption("count(*)", condStr);
                    cmd.CommandText = strExp;
                }
                OracleDataReader dr = cmd.ExecuteReader();
                dr.Read();
                Int32 sumLastYear = Convert.ToInt32(dr[0]);

                double dCycleValue = 0;
                if (period == 0)
                {
                    dCycleValue = (sumLastYear / 365.0) * 7.0;
                }
                else if (period == 1)
                {
                    dCycleValue = (sumLastYear / 365.0) * 14.0;
                }
                else
                {
                    dCycleValue = sumLastYear;
                }
                return dCycleValue;
            }
            catch(Exception ex)
            {
                ExToLog(ex, "getCycleValue");
                return 0;
            }
        }

        /// <summary>
        /// �쳣��־
        /// ���༭��   ����
        /// ���༭ʱ��  2011-2-23
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "cl3Color-clsCal3Color-" + sFunc);
        }
    }
 }

