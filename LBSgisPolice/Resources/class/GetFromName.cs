using System;
using System.Collections.Generic;
using System.Text;

namespace LBSgisPolice
{
	public class GetFromName
	{
        private string tableName, bmpName, objID, objName,xiaquField,sqlFields;
        public bool isCancel;

        public string TableName {
            get { return tableName; }
            set { tableName = value; }
        }

        public string BmpName
        {
            get { return bmpName; }
            set { bmpName = value; }
        }

        public string ObjID
        {
            get { return objID; }
            set { objID = value; }
        }

        public string ObjName
        {
            get { return objName; }
            set { objName = value; }
        }

        public string XiaquField
        {
            get { return xiaquField; }
            set { xiaquField = value; }
        }

        public string SQLFields
        {
            get { return sqlFields; }
            set { sqlFields = value; }
        }

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
                    xiaquField = "�����ص�_�ֵ�";
                    sqlFields = "��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ص�_�ֵ�,��������,������������,��������,�����ص���ַ,��������,������Դ,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "��������":
                    objName = "��ҵʵ�ʾ�Ӫ����";
                    objID = "���";
                    tableName = "��������";
                    bmpName = "gonggong";
                    xiaquField = "Ͻ���ɳ���";
                    break;
                case "�˿�":
                case "�˿�ϵͳ":
                    objName = "����";
                    objID = "���֤����";
                    tableName = "�˿�ϵͳ";
                    bmpName = "ren.bmp";
                    sqlFields = "����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,�����ɳ���,����ʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    xiaquField = "�����ɳ���";
                    break;
                case "��ȫ������λ":
                    objName = "��λ����";
                    objID = "���";
                    tableName = "��ȫ������λ";
                    bmpName = "afdw.bmp";
                    xiaquField = "Ͻ���ɳ���";
                    break;
                case "����":
                    objName = "��������";
                    objID = "���ɱ��";
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
                    sqlFields = "���ݱ��,��������,������ϵ�绰,��ϵ��ַ,��Ȩ֤���,��Ȩ֤��,����Ƭ��,����վ,���,��ǰ��ס����,��ס֤��Ч��������,δ����ס֤����,��ʷ��ס����,ȫ��ַ,��ַ��·��,��ַ����,¥��,�����,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "�ΰ�����":
                case "�ΰ�����ϵͳ":
                    objName = "��������";
                    objID = "���ڱ��";
                    tableName = "�ΰ�����ϵͳ";
                    bmpName = "zakk.bmp";
                    break;
                case "������ҵ":
                    objName = "��ҵʵ�ʾ�Ӫ����";
                    objID = "���";
                    tableName = "������ҵ";
                    bmpName = "tezhong";
                    xiaquField = "Ͻ���ɳ���";
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
                    objID = "���";
                    tableName = "�����ɳ���";
                    bmpName = "pcs.bmp";
                    xiaquField = "���ڽ�����";
                    break;
                case "��Ա":
                case "��Ա��λϵͳ":
                    objName = "����";
                    objID = "����";
                    tableName = "��Ա��λϵͳ";
                    bmpName = "jc.bmp";
                    xiaquField = "������λ";
                    break;
                case "�������ж�":
                    objName = "�ж���";
                    objID = "���";
                    tableName = "�������ж�";
                    bmpName = "zd.bmp";
                    xiaquField = "���ڽ�����";
                    break;
                case "����������":
                    objName = "��������";
                    objID = "���";
                    tableName = "����������";
                    bmpName = "jws.bmp";
                    xiaquField = "���ڽ�����";
                    break;
                default:
                    break;
            }
        }
	}
}
