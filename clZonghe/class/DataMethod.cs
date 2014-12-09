using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using DY.Utility;

namespace clZonghe
{
    public class DataMethod
	{
        #region 生成excel并输出到客户端   在格式化为double类型的列上自动增加公式
        /// <summary>
        /// 生成excel并输出到客户端   在格式化为double类型的列上自动增加公式
        /// </summary>
        /// <param name="companyName">公司名称</param>
        /// <param name="fileSubject">文件主题</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="title">表格标题</param>
        /// <param name="dt">数据源DataTable</param>
        /// <param name="formatColumns">需格式化的列</param>
        /// <param name="sumRowNm">合计列名称，为空时，不合计   （在格式化为double类型的列上自动增加公式）</param>
        public static void ToExcle(string sheetName, string title, DataTable dt, string formatColumns, string sumRowNm)
        {
            ExcelMethods em = new ExcelMethods();
            em.GenerateExcel("Yjm", "Yjm", sheetName, title, dt, formatColumns, sumRowNm);
        }
        #endregion


        #region 生成sql语句
        /// <summary>
        /// 生成sql语句
        /// </summary>
        /// <param name="statType">统计方式，传入（派出所或所属中队）</param>
        /// <param name="type">类型（按月MM，按周WW）</param>
        /// <param name="typeName">输入月或周，按类型</param>
        /// <param name="groupName">用于分组名称</param>
        /// <param name="fieldName">字段名（一般为抽取更新时间）</param>
        /// <param name="tableName">表名</param>
        /// <param name="innerJoin">连接</param>
        /// <param name="where">where</param>
        /// <returns></returns>
        public static string generateSql(string statType, string type, string typeName, string groupName, string fieldName, string tableName, string innerJoin, string where, string sumType,string strConn)
        {
            string condition = "";
            //如统计最近1月或最近1周，则使用count，否则按条件生成条件
            if (sumType == "最近1月" || sumType == "最近1周")
            {
                condition = "count(1) as " + sumType + " ";
            }
            else
            {
                condition = generateSelect(type, typeName, fieldName, tableName, where,strConn);
            }
            if (condition != "")
            {
                string group = (groupName != "" && condition != "") ? " nvl(" + groupName + ",'合计') " + statType + "," : "";
                //生成group
                if (sumType == "最近1月" || sumType == "最近1周")
                {
                    groupName = groupName != "" ? "  group by rollup(" + groupName + ")" : " ";
                }
                else
                {
                    groupName = groupName != "" ? "  group by rollup(" + groupName + ")" : " group by to_char(" + fieldName + ",'" + type + "')";
                }
                //生成最终的Sql语句
                string strSql = "select " + group + condition + " from " + tableName + " " + innerJoin + " " + where + groupName;
                return strSql;
            }
            return "";
        }
        #endregion


        #region 生成行列转换的field部分
        /// <summary>
        /// 生成行列转换的field部分
        /// </summary>
        /// <param name="type">类型（按月MM，按周WW）</param>
        /// <param name="typeName">输入月或周，按类型</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="tableName">表名</param>
        /// <param name="where">条件</param>
        /// <param name="strConn">数据库连接接字符串</param>
        /// <returns></returns>
        public static string generateSelect(string type, string typeName, string fieldName, string tableName, string where,string strConn)
        {
            string text = "";
            OracelData oda = new OracelData(strConn);
            string strSql = "select distinct to_char(" + fieldName + ",'" + type + "') week from " + tableName + " " + where + "  order by week";
            //between to_date('" + begin + "','yyyy-MM-dd') and to_date('" + end + "','yyyy-MM-dd')
            DataTable dt = oda.SelectDataBase(strSql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string value = dt.Rows[i][0].ToString();
                    text += "sum(decode(to_char(" + fieldName + ",'" + type + "'),'" + value + "',1)) as \"" + value + typeName + "\",";
                }
                text = text.Length > 0 ? text.Substring(0, text.Length - 1) : text;
                return text;
            }
            return "";
        }
        #endregion
	}
}
