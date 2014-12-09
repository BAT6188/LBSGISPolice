using System;
using System.Collections.Generic;
using System.Text;

namespace LBSgisPolice
{
    public class GetFromName
    {
        private string tableName, bmpName, objID, objName, xiaquField, zhongduiField, sqlFields, frmFields;
        public bool isCancel;

        public string TableName
        {
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

        public string FrmFields
        {
            get { return frmFields; }
            set { frmFields = value; }
        }

        public GetFromName(string name)
        {
            bmpName = "";
            xiaquField = "";
            sqlFields = "*";
            FrmFields = "*";
            switch (name)
            {
                case "案件":
                case "案件信息":
                    objName = "案件名称";
                    objID = "案件编号";
                    tableName = "案件信息";
                    bmpName = "anjian";
                    xiaquField = "所属派出所";
                    zhongduiField = "所属中队";
                    sqlFields = "案件名称,案件编号,案发状态,案件类型,案别_案由,简要案情,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属社区,发案地点详址,发案场所,案件来源,作案手段特点,所属派出所代码,所属中队代码,所属警务室代码,涉案人员,抽取ID,抽取更新时间,最后更新人,所属警务室,所属中队,所属派出所,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,备用字段一,备用字段二,备用字段三";
                    frmFields = "案件名称,案件编号,案发状态,案件类型,案别_案由,简要案情,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属社区,发案地点详址,发案场所,案件来源,作案手段特点,涉案人员,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "公共场所":
                    objName = "企业实际经营名称";
                    objID = "编号";
                    tableName = "公共场所";
                    bmpName = "gonggong";
                    xiaquField = "所属派出所";
                    zhongduiField = "所属中队";
                    frmFields = "编号,企业注册名称,企业实际经营名称,主营,兼营,当前等级,联系电话,行业类别,详细地址,法定代表人,实际经营人姓名,保安组织联系电话,保安组织负责人姓名,保安组织负责人联系电话,责任民警姓名,民警联系电话,所属派出所,所属中队,所属警务室,标注人,标注时间,X,Y";
                    break;
                case "人口":
                case "人口系统":
                    objName = "姓名";
                    objID = "身份证号码";
                    tableName = "人口系统";
                    bmpName = "ren.bmp";
                    sqlFields = "姓名,身份证号码,民族,性别,人口性质,出生日期,全住址,住址街路巷,住址门牌,楼层,房间号,户籍地址,婚姻状态,配偶姓名,结婚时间,交通工具,车牌号码,服务处所,政治面貌, 暂住证号,暂住证有效期限,入住日期,注销日期,联系电话,重点人口,所属派出所,房屋编号,相关案件,抽取ID,抽取更新时间,最后更新人,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,备用字段一,备用字段二,备用字段三";
                    xiaquField = "所属派出所";
                    frmFields = "姓名,身份证号码,民族,性别,人口性质,出生日期,全住址,住址街路巷,住址门牌,楼层,房间号,户籍地址,婚姻状态,配偶姓名,结婚时间,交通工具,车牌号码,服务处所,政治面貌, 暂住证号,暂住证有效期限,入住日期,注销日期,联系电话,重点人口,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "安全防护单位":
                    objName = "单位名称";
                    objID = "编号";
                    tableName = "安全防护单位";
                    bmpName = "afdw.bmp";
                    xiaquField = "所属派出所";
                    frmFields = "编号,单位名称,单位性质,单位地址,主管保卫工作负责人,手机号码,所属派出所,所属中队,所属警务室,标注人,标注时间,X,Y";
                    break;
                case "网吧":
                    objName = "网吧名称";
                    objID = "编号";
                    tableName = "网吧";
                    bmpName = "wb.bmp";
                    xiaquField = "所属派出所";
                    frmFields = "编号,网吧名称,地址,联系电话,电脑数目,法人代表,负责人,所属派出所,所属中队,所属警务室,标注人,标注时间,X,Y";
                    break;
                case "出租屋房屋":
                case "出租屋房屋系统":
                    objName = "屋主姓名";
                    objID = "房屋编号";
                    tableName = "出租屋房屋系统";
                    bmpName = "fw.bmp";
                    xiaquField = "镇街";
                    sqlFields = "房屋编号,屋主姓名,屋主联系电话,房屋类型,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,当前居住人数,暂住证有效期内人数,未办暂住证人数,历史居住人数,全地址,地址街路巷,地址门牌,楼层,房间号,抽取ID,抽取更新时间,最后更新人,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y,备用字段一,备用字段二,备用字段三";
                    frmFields = "房屋编号,屋主姓名,屋主联系电话,房屋类型,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,当前居住人数,暂住证有效期内人数,未办暂住证人数,历史居住人数,全地址,地址街路巷,地址门牌,楼层,房间号,所属派出所,所属中队,所属警务室,标注人,标注时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "治安卡口":
                case "治安卡口系统":
                    objName = "卡口名称";
                    objID = "编号";
                    tableName = "治安卡口系统";
                    bmpName = "zakk.bmp";
                    break;
                case "特种行业":
                    objName = "企业实际经营名称";
                    objID = "编号";
                    tableName = "特种行业";
                    bmpName = "tezhong";
                    xiaquField = "所属派出所";
                    frmFields = "编号,企业注册名称,企业实际经营名称,主营,兼营,当前等级,联系电话,行业类别,详细地址,法定代表人,实际经营人姓名,保安组织联系电话,保安组织负责人姓名,保安组织负责人联系电话,责任民警姓名,民警联系电话,所属派出所,所属中队,所属警务室,标注人,标注时间,X,Y";
                    break;
                case "视频":
                case "视频位置":
                    objName = "设备名称";
                    objID = "设备编号";
                    tableName = "视频位置";
                    bmpName = "sxt.bmp";
                    xiaquField = "所属派出所";
                    break;
                case "消防栓":
                    objName = "消防内部编号";
                    objID = "编号";
                    tableName = "消防栓";
                    bmpName = "xfs.bmp";
                    xiaquField = "所属派出所";
                    frmFields = "编号,消防内部编号,位置描述,消防管理单位,所属派出所,所属中队,所属警务室,标注人,标注时间,X,Y";
                    break;
                case "消防重点单位":
                    objName = "消防安全重点单位";
                    objID = "编号";
                    tableName = "消防重点单位";
                    bmpName = "xfdw.bmp";
                    xiaquField = "所属派出所";
                    frmFields = "编号,消防安全重点单位,地址,消防安全责任人,消防安全管理人,联系电话,单位类别,所属派出所,所属中队,所属警务室,标注人,标注时间,X,Y";
                    break;
                case "警车":
                case "GPS警车定位系统":
                    objName = "终端车辆号牌";
                    objID = "终端ID号码";
                    tableName = "GPS警车定位系统";
                    bmpName = "jCar.bmp";
                    xiaquField = "权限单位";
                    break;
                case "基层派出所":
                    objName = "派出所名";
                    objID = "派出所代码";
                    tableName = "基层派出所";
                    bmpName = "pcs.bmp";
                    xiaquField = "派出所名";
                    break;
                case "警员":
                case "GPS警员":
                    objName = "警力编号";
                    objID = "警力编号";
                    tableName = "gps警员";
                    bmpName = "Police.BMP";
                    xiaquField = "派出所名";
                    sqlFields = "警力编号,派出所名,中队名,所属科室,当前任务,设备编号";
                    break;
                case "基层民警中队":
                    objName = "中队名";
                    objID = "中队代码";
                    tableName = "基层民警中队";
                    bmpName = "zd.bmp";
                    xiaquField = "所属派出所";
                    break;
                case "社区警务室":
                    objName = "警务室名";
                    objID = "警务室代码";
                    tableName = "社区警务室";
                    bmpName = "jws.bmp";
                    xiaquField = "所属派出所";
                    break;
                case "派出所辖区":
                    objName = "派出所名称";
                    objID = "派出所代码";
                    tableName = "派出所辖区";
                    bmpName = "";
                    break;
                case "民警中队辖区":
                    objName = "中队名称";
                    objID = "中队代码";
                    tableName = "民警中队辖区";
                    bmpName = "";
                    break;
                case "警务室辖区":
                    objName = "警务室名称";
                    objID = "警务室代码";
                    tableName = "警务室辖区";
                    bmpName = "";
                    break;
                case "信息点":
                    objName = "名称";
                    objID = "id";
                    tableName = "信息点";
                    bmpName = "gonggong";
                    sqlFields = "id,名称,类别,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                default:
                    break;
            }
        }
    }
}
