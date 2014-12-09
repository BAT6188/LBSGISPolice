using System;
using System.Collections.Generic;
using System.Text;

namespace nsGetFromName
{
	public class GetFromName
	{
        private string tableName, bmpName, objID, objName, xiaquField, zhongduiField, sqlFields, frmFilelds;
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

        public string ZhongduiField                    //edit by fisher in 09-11-25
        {
            get { return zhongduiField; }
            set { zhongduiField = value; }
        }

        public string SQLFields
        {
            get { return sqlFields; }
            set { sqlFields = value; }
        }

        public string FrmFilelds
        {
            get { return frmFilelds; }
            set { frmFilelds = value; }
        }

        public GetFromName(string name) {
            bmpName = "";
            xiaquField = "";
            sqlFields = "*";
            frmFilelds = "*";
            switch (name)
            {
                case "����":
                case "������Ϣ":
                    objName = "��������";
                    objID = "�������";
                    tableName = "������Ϣ";
                    bmpName = "anjian";
                    xiaquField = "�����ɳ���";
                    zhongduiField = "�����ж�";
                    sqlFields = "��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ص�_�ֵ�,��������,�����ص���ַ,��������,������Դ,�����ֶ��ص�,�����ɳ�������,�����жӴ���,���������Ҵ���,�永��Ա,��ȡID,��ȡ����ʱ��,��������,�����ֶ�һ,�����ֶζ�,�����ֶ���,����������,�����ж�,�����ɳ���,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    frmFilelds = "��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ص�_�ֵ�,��������,�����ص���ַ,��������,������Դ,�����ֶ��ص�,�永��Ա,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "������Ϣli":
                    objName = "��������";
                    objID = "�������";
                    tableName = "������Ϣ";
                    bmpName = "anjian";
                    xiaquField = "�����ɳ���";
                    zhongduiField = "�����ж�";
                    sqlFields = "��������,�������,����״̬,��������,����_����,��Ҫ����,ר����ʶ,����ʱ���ֵ,����ʱ����ֵ,�����ص�_����,�����ص�_�ֵ�,��������,�����ص���ַ,��������,������Դ,�����ֶ��ص�,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "��������":
                    objName = "��ҵʵ�ʾ�Ӫ����";
                    objID = "���";
                    tableName = "��������";
                    bmpName = "gonggong";
                    xiaquField = "�����ɳ���";
                    zhongduiField = "�����ж�";
                    frmFilelds = "���,��ҵע������,��ҵʵ�ʾ�Ӫ����,��Ӫ,��Ӫ,��ǰ�ȼ�,��ϵ�绰,��ҵ���,��ϸ��ַ,����������,ʵ�ʾ�Ӫ������,������֯��ϵ�绰,������֯����������,������֯��������ϵ�绰,����������,����ϵ�绰,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,X,Y";
                    break;
                case "�˿�":
                case "�˿�ϵͳ":
                    objName = "����";
                    objID = "���֤����";
                    tableName = "�˿�ϵͳ";
                    bmpName = "ren.bmp";
                    sqlFields = "����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,�����ɳ���,���ݱ��,��ذ���,��ȡID,��ȡ����ʱ��,��������,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,�����ֶ�һ,�����ֶζ�,�����ֶ���";
                    xiaquField = "�����ɳ���";
                    frmFilelds = "����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,���ݱ��,��ذ���,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "�˿�ϵͳli":
                    objName = "����";
                    objID = "���֤����";
                    tableName = "�˿�ϵͳ";
                    bmpName = "ren.bmp";
                    sqlFields = "����,���֤����,����,�Ա�,�˿�����,��������,ȫסַ,סַ��·��,סַ����,¥��,�����,������ַ,����״̬,��ż����,���ʱ��,��ͨ����,���ƺ���,������,������ò, ��ס֤��,��ס֤��Ч����,��ס����,ע������,��ϵ�绰,�ص��˿�,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    xiaquField = "�����ɳ���";
                    break;
                case "��ȫ������λ":
                    objName = "��λ����";
                    objID = "���";
                    tableName = "��ȫ������λ";
                    bmpName = "afdw.bmp";
                    xiaquField = "�����ɳ���";
                    frmFilelds = "���,��λ����,��λ����,��λ��ַ,���ܱ�������������,�ֻ�����,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,X,Y";
                    break;
                case "����":
                    objName = "��������";
                    objID = "���";
                    tableName = "����";
                    bmpName = "wb.bmp";
                    xiaquField = "�����ɳ���";
                    frmFilelds = "���,��������,��ַ,��ϵ�绰,������Ŀ,���˴���,������,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,X,Y";
                    break;
                case "�����ݷ���":
                case "�����ݷ���ϵͳ":
                    objName = "��������";
                    objID = "���ݱ��";
                    tableName = "�����ݷ���ϵͳ";
                    bmpName = "fw.bmp";
                    xiaquField = "���";
                    sqlFields = "���ݱ��,��������,������ϵ�绰,��������,��ϵ��ַ,��Ȩ֤���,��Ȩ֤��,����Ƭ��,����վ,���,��ǰ��ס����,��ס֤��Ч��������,δ����ס֤����,��ʷ��ס����,ȫ��ַ,��ַ��·��,��ַ����,¥��,�����,��ȡID,��ȡ����ʱ��,��������,�����ɳ���,�����ж�,����������,�����ɳ�������,�����жӴ���,���������Ҵ���,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,�����ֶ�һ,�����ֶζ�,�����ֶ���";
                    frmFilelds = "���ݱ��,��������,������ϵ�绰,��������,��ϵ��ַ,��Ȩ֤���,��Ȩ֤��,����Ƭ��,����վ,���,��ǰ��ס����,��ס֤��Ч��������,δ����ס֤����,��ʷ��ס����,ȫ��ַ,��ַ��·��,��ַ����,¥��,�����,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "�����ݷ���ϵͳli":
                    objName = "��������";
                    objID = "���ݱ��";
                    tableName = "�����ݷ���ϵͳ";
                    bmpName = "fw.bmp";
                    xiaquField = "���";
                    sqlFields = "���ݱ��,��������,������ϵ�绰,��������,��ϵ��ַ,��Ȩ֤���,��Ȩ֤��,����Ƭ��,����վ,���,��ǰ��ס����,��ס֤��Ч��������,δ����ס֤����,��ʷ��ס����,ȫ��ַ,��ַ��·��,��ַ����,¥��,�����,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
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
                    frmFilelds = "���,��ҵע������,��ҵʵ�ʾ�Ӫ����,��Ӫ,��Ӫ,��ǰ�ȼ�,��ϵ�绰,��ҵ���,��ϸ��ַ,����������,ʵ�ʾ�Ӫ������,������֯��ϵ�绰,������֯����������,������֯��������ϵ�绰,����������,����ϵ�绰,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,X,Y";
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
                    frmFilelds = "���,�����ڲ����,λ������,��������λ,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,X,Y";
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
                    frmFilelds = "���,������ȫ�ص㵥λ,��ַ,������ȫ������,������ȫ������,��ϵ�绰,��λ���,�����ɳ���,�����ж�,����������,��ע��,��עʱ��,X,Y";
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
                case "GPS��Ա":
                    objName = "�������";
                    objID = "�������";
                    tableName = "gps��Ա";
                    bmpName = "Police.BMP";
                    xiaquField = "�ɳ�����";
                    sqlFields = "�������,�ɳ�����,�ж���,��������,��ǰ����,�豸���,��λ����ʱ��";
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
                case "�ɳ���Ͻ��":
                    objName = "�ɳ�������";
                    objID = "�ɳ�������";
                    tableName = "�ɳ���Ͻ��";
                    bmpName = "";
                    break;
                case "���ж�Ͻ��":
                    objName = "�ж�����";
                    objID = "�жӴ���";
                    tableName = "���ж�Ͻ��";
                    bmpName = "";
                    break;
                case "������Ͻ��":
                    objName = "����������";
                    objID = "�����Ҵ���";
                    tableName = "������Ͻ��";
                    bmpName = "";
                    break;
                case "��Ϣ��":
                    objName = "����";
                    objID = "id";
                    tableName = "��Ϣ��";
                    bmpName = "gonggong";
                    sqlFields = "id,����,���,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                default:
                    break;
            }
        }
	}
}
