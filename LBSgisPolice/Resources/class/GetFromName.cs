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
                case "案件":
                case "案件信息":
                    objName = "案件名称";
                    objID = "案件编号";
                    tableName = "案件信息";
                    bmpName = "anjian";
                    xiaquField = "发案地点_街道";
                    sqlFields = "案件名称,案件编号,案发状态,案件类型,案别_案由,简要案情,专案标识,发案时间初值,发案时间终值,发案地点_区县,发案地点_街道,所属警区,发案地政区划,所属社区,发案地点详址,发案场所,案件来源,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "公共场所":
                    objName = "企业实际经营名称";
                    objID = "编号";
                    tableName = "公共场所";
                    bmpName = "gonggong";
                    xiaquField = "辖区派出所";
                    break;
                case "人口":
                case "人口系统":
                    objName = "姓名";
                    objID = "身份证号码";
                    tableName = "人口系统";
                    bmpName = "ren.bmp";
                    sqlFields = "姓名,身份证号码,民族,性别,人口性质,出生日期,全住址,住址街路巷,住址门牌,楼层,房间号,户籍地址,婚姻状态,配偶姓名,结婚时间,交通工具,车牌号码,服务处所,政治面貌, 暂住证号,暂住证有效期限,入住日期,注销日期,联系电话,重点人口,所属派出所,更新时间,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    xiaquField = "所属派出所";
                    break;
                case "安全防护单位":
                    objName = "单位名称";
                    objID = "编号";
                    tableName = "安全防护单位";
                    bmpName = "afdw.bmp";
                    xiaquField = "辖区派出所";
                    break;
                case "网吧":
                    objName = "网吧名称";
                    objID = "网吧编号";
                    tableName = "网吧";
                    bmpName = "wb.bmp";
                    xiaquField = "所属派出所";
                    break;
                case "出租屋房屋":
                case "出租屋房屋系统":
                    objName = "屋主姓名";
                    objID = "房屋编号";
                    tableName = "出租屋房屋系统";
                    bmpName = "fw.bmp";
                    xiaquField = "镇街";
                    sqlFields = "房屋编号,屋主姓名,屋主联系电话,联系地址,产权证类别,产权证号,所属片区,服务站,镇街,当前居住人数,暂住证有效期内人数,未办暂住证人数,历史居住人数,全地址,地址街路巷,地址门牌,楼层,房间号,t.geoloc.SDO_POINT.X as X,t.geoloc.SDO_POINT.Y as Y";
                    break;
                case "治安卡口":
                case "治安卡口系统":
                    objName = "卡口名称";
                    objID = "卡口编号";
                    tableName = "治安卡口系统";
                    bmpName = "zakk.bmp";
                    break;
                case "特种行业":
                    objName = "企业实际经营名称";
                    objID = "编号";
                    tableName = "特种行业";
                    bmpName = "tezhong";
                    xiaquField = "辖区派出所";
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
                    break;
                case "交通卡口":
                case "交通卡口系统":
                    objName = "卡口编号";
                    objID = "卡口编号";
                    tableName = "交通卡口系统";
                    bmpName = "jtkk.bmp";
                    break;
                case "消防重点单位":
                    objName = "消防安全重点单位";
                    objID = "编号";
                    tableName = "消防重点单位";
                    bmpName = "xfdw.bmp";
                    xiaquField = "所属派出所";
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
                    objID = "编号";
                    tableName = "基层派出所";
                    bmpName = "pcs.bmp";
                    xiaquField = "所在街镇乡";
                    break;
                case "警员":
                case "警员定位系统":
                    objName = "警号";
                    objID = "警号";
                    tableName = "警员定位系统";
                    bmpName = "jc.bmp";
                    xiaquField = "所属单位";
                    break;
                case "基层民警中队":
                    objName = "中队名";
                    objID = "编号";
                    tableName = "基层民警中队";
                    bmpName = "zd.bmp";
                    xiaquField = "所在街镇乡";
                    break;
                case "社区警务室":
                    objName = "警务室名";
                    objID = "编号";
                    tableName = "社区警务室";
                    bmpName = "jws.bmp";
                    xiaquField = "所在街镇乡";
                    break;
                default:
                    break;
            }
        }
	}
}
