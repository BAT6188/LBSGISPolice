using System;
using System.Collections.Generic;
using System.Text;

namespace LBSDataGuide
{
	public class GetFromName
	{
        private string tableName, bmpName, objID, objName,xiaquField,sqlFields;
        public bool isCancel;

        // ����
        public string TableName {
            get { return tableName; }
            set { tableName = value; }
        }

        // ��ͼ����ͼƬ
        public string BmpName
        {
            get { return bmpName; }
            set { bmpName = value; }
        }

        // ��ID�ֶ�
        public string ObjID
        {
            get { return objID; }
            set { objID = value; }
        }

        // �������ֶ�
        public string ObjName
        {
            get { return objName; }
            set { objName = value; }
        }

        // ����Ͻ��
        public string XiaquField
        {
            get { return xiaquField; }
            set { xiaquField = value; }
        }
         
        // ��ѯʱ�õ��ֶ�
        public string SQLFields
        {
            get { return sqlFields; }
            set { sqlFields = value; }
        }

        private string sqlInsert="";
        // ����ʱ�õ��ֶ�
        public string SQLInsert
        {
            get { return sqlInsert; }
            set { sqlInsert = value; }
        }

        // ���캯��
        public GetFromName(string name) {
            bmpName = "";
            xiaquField = "";
            sqlFields = "*";
            switch (name)
            {
                case "����":
                case "������Ϣ":
                    objName = "��������";
                    objID = "�������";
                    tableName = "������Ϣ";
                    bmpName = "anjian";
                    xiaquField = "�����ɳ���";
                    sqlFields = "��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ص�_�ֵ�,�����ж�,��������,�����ص���ַ,��������,������Դ,�����ֶ��ص�,�����ɳ�������,�����жӴ���,���������Ҵ���,��ȡID,��ȡ����ʱ��,��������,�����ɳ���,����������,�永��Ա,��ע��,��עʱ��,�����ֶ�һ,�����ֶζ�,�����ֶ���,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    sqlInsert = "��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ص�_�ֵ�,�����ж�,��������,�����ص���ַ,��������,������Դ,�����ֶ��ص�,�����ɳ�������,�����жӴ���,���������Ҵ���,��ȡID,��ȡ����ʱ��,��������,�����ɳ���,����������,�永��Ա,��ע��,��עʱ��,�����ֶ�һ,�����ֶζ�,�����ֶ���,mi_prinx";
                    break;
                case "��������":
                    objName = "��ҵʵ�ʾ�Ӫ����";
                    objID = "���";
                    tableName = "��������";
                    bmpName = "gonggong";
                    xiaquField = "�����ɳ���";
                    break;
                case "�˿�":
                case "�˿�ϵͳ":
                    objName = "����";
                    objID = "���֤����";
                    tableName = "�˿�ϵͳ";
                    bmpName = "ren.bmp";
                    sqlFields = "����,���֤����,�˿�����,����,�Ա�,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,���ݱ��,��ذ���,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,��ȡID,��ȡ����ʱ��,��ע��,��עʱ��,��������,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,�����ֶ�һ,�����ֶζ�,�����ֶ���";
                    sqlInsert = "����,���֤����,�˿�����,����,�Ա�,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,���ݱ��,��ذ���,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,��ȡID,��ȡ����ʱ��,��ע��,��עʱ��,��������,�����ֶ�һ,�����ֶζ�,�����ֶ���,mi_prinx";

                    xiaquField = "�����ɳ���";
                    break;
                case "��ȫ������λ":
                    objName = "��λ����";
                    objID = "���";
                    tableName = "��ȫ������λ";
                    bmpName = "afdw.bmp";
                    xiaquField = "�����ɳ���";
                    break;
                case "����":
                    objName = "��������";
                    objID = "���";
                    tableName = "����";
                    bmpName = "wb.bmp";
                    xiaquField = "�����ɳ���";
                    break;
                case "�����ݷ���":
                case "�����ݷ���ϵͳ":
                    objName = "��������";
                    objID = "���ݱ��";
                    tableName = "�����ݷ���ϵͳ";
                    bmpName = "fw.bmp";
                    xiaquField = "���";
                    sqlFields = "���ݱ��,��������,������ϵ�绰,��ϵ��ַ,��������,��Ȩ֤���,��Ȩ֤��,����Ƭ��,����վ,���,��ǰ��ס����,��ס֤��Ч��������,δ����ס֤����,��ʷ��ס����,ȫ��ַ,��ַ��·��,��ַ����,¥��,�����,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,��ȡID,��ȡ����ʱ��,��ע��,��עʱ��,��������,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,�����ֶ�һ,�����ֶζ�,�����ֶ���";
                    sqlInsert = "���ݱ��,��������,������ϵ�绰,��ϵ��ַ,��������,��Ȩ֤���,��Ȩ֤��,����Ƭ��,����վ,���,��ǰ��ס����,��ס֤��Ч��������,δ����ס֤����,��ʷ��ס����,ȫ��ַ,��ַ��·��,��ַ����,¥��,�����,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,��ȡID,��ȡ����ʱ��,��ע��,��עʱ��,��������,�����ֶ�һ,�����ֶζ�,�����ֶ���,mi_prinx";
                    break;
                case "�ΰ�����":
                case "�ΰ�����ϵͳ":
                    objName = "��������";
                    objID = "���";
                    tableName = "�ΰ�����ϵͳ";
                    bmpName = "zakk.bmp";
                    break;
                case "������ҵ":
                    objName = "��ҵʵ�ʾ�Ӫ����";
                    objID = "���";
                    tableName = "������ҵ";
                    bmpName = "tezhong";
                    xiaquField = "�����ɳ���";
                    break;
                case "��Ƶ":
                case "��Ƶλ��":
                    objName = "�豸����";
                    objID = "�豸���";
                    tableName = "��Ƶλ��";
                    bmpName = "sxt.bmp";
                    xiaquField = "�����ɳ���";
                    break;
                case "����˨":
                    objName = "�����ڲ����";
                    objID = "���";
                    tableName = "����˨";
                    bmpName = "xfs.bmp";
                    xiaquField = "�����ɳ���";
                    break;
                case "��ͨ����":
                case "��ͨ����ϵͳ":
                    objName = "���ڱ��";
                    objID = "���ڱ��";
                    tableName = "��ͨ����ϵͳ";
                    bmpName = "jtkk.bmp";
                    break;
                case "�����ص㵥λ":
                    objName = "������ȫ�ص㵥λ";
                    objID = "���";
                    tableName = "�����ص㵥λ";
                    bmpName = "xfdw.bmp";
                    xiaquField = "�����ɳ���";
                    break;
                case "����":
                case "GPS������λϵͳ":
                    objName = "�ն˳�������";
                    objID = "�ն�ID����";
                    tableName = "GPS������λϵͳ";
                    bmpName = "jCar.bmp";
                    xiaquField = "Ȩ�޵�λ";
                    break;
                case "�����ɳ���":
                    objName = "�ɳ�����";
                    objID = "�ɳ�������";
                    tableName = "�����ɳ���";
                    bmpName = "pcs.bmp";
                    xiaquField = "�ɳ�����";
                    break;
                case "��Ա":
                case "��Ա��λϵͳ":
                    objName = "�������";
                    objID = "�������";
                    tableName = "��Ա��λϵͳ";
                    bmpName = "jc.bmp";
                    xiaquField = "�ɳ�����";
                    break;
                case "�������ж�":
                    objName = "�ж���";
                    objID = "�жӴ���";
                    tableName = "�������ж�";
                    bmpName = "zd.bmp";
                    xiaquField = "�����ɳ���";
                    break;
                case "����������":
                    objName = "��������";
                    objID = "�����Ҵ���";
                    tableName = "����������";
                    bmpName = "jws.bmp";
                    xiaquField = "�����ɳ���";
                    break;
                case "GPS110":
                case "GPS110.������Ϣ110":
                    objName = "��������";
                    objID = "�������";
                    tableName = "GPS110.������Ϣ110";
                    bmpName = "";
                    xiaquField = "�����ɳ���";
                    break;
                case "�û�":
                    objName = "USERNAME";
                    objID = "USERNAME";
                    tableName = "�û�";
                    bmpName = "";
                    xiaquField = "�û���λ";
                    break;
                default:
                    break;
            }
        }
	}
}
