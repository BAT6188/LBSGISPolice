using System;
using System.Runtime.InteropServices;
using System.Text;

using System.Collections;
using System.ComponentModel;

using System.Diagnostics;

namespace clZonghe
{
    /// <summary>
    /// ClsFileManagement 的摘要说明。
    /// </summary>
    public class ClsFileManagement
    {
        //		public ClsFileManagement()
        //		{
        //			//
        //			// TODO: 在此处添加构造函数逻辑
        //			//
        //		}
        public static void FileUpdate()
        {
            string Section = "文件";
            string ConString;
            ConString = OracelData.getStrConn(); 
           
            System.Data.OracleClient.OracleConnection OracleConnect = new System.Data.OracleClient.OracleConnection(ConString);
            OracleConnect.Open();
            System.Data.OracleClient.OracleCommand OracleCom = new System.Data.OracleClient.OracleCommand("Select ID,文件名称,本机位置,文件版本 From 安全防护单位文件存储表", OracleConnect);
            System.Data.OracleClient.OracleDataAdapter ResultAdapter = new System.Data.OracleClient.OracleDataAdapter();
            ResultAdapter.SelectCommand = OracleCom;
            System.Data.DataTable ResultDT = new System.Data.DataTable();
            ResultAdapter.Fill(ResultDT);
            OracleConnect.Close();
            for (int i = 0; i < ResultDT.Rows.Count; i++)
            {
                string Key;
                double Ver;
                Key = ResultDT.Rows[i]["文件名称"].ToString();
                Ver = double.Parse(ResultDT.Rows[i]["文件版本"].ToString());
                if (INIClass.IniReadValue(Section, Key) == "" || double.Parse(INIClass.IniReadValue(Section, Key).ToString()) < Ver)
                {
                    System.IO.File.Copy("W:\\" + Key, ResultDT.Rows[i]["本机位置"].ToString() + "\\" + Key, true);
                    INIClass.IniWriteValue(Section, Key, Ver.ToString());
                }
            }
        }

        public static void NewFileAdd(string FileName,string FilePath, string DWMC)
        {
            string Section = "文件";
            INIClass.IniWriteValue(Section, FileName, "1.0");
            System.IO.File.Copy(FilePath + "\\" + FileName, "W:\\" + FileName, true);
            string ConString;
            ConString = OracelData.getStrConn(); 
            System.Data.OracleClient.OracleConnection OracleConnect = new System.Data.OracleClient.OracleConnection(ConString);
            OracleConnect.Open();
            System.Data.OracleClient.OracleCommand OracleCom = new System.Data.OracleClient.OracleCommand("Select Count(*) From 安全防护单位文件存储表", OracleConnect);
            int i = int.Parse(OracleCom.ExecuteOracleScalar().ToString());
            OracleCom.CommandText = "INSERT INTO 安全防护单位文件存储表 (ID,文件名称,单位名称,本机位置,文件版本) VALUES ( " + i + " + 1,'" + FileName + "','" + DWMC + "','" + FilePath + "','1.0')";
            OracleCom.ExecuteNonQuery();
            OracleConnect.Close();
        }

        public static void FileDel(string FileName)
        {
            string Section = "文件";
            INIClass.IniWriteValue(Section, FileName, "");
            System.IO.File.Delete("W:\\" + FileName);
            string ConString;
            ConString = OracelData.getStrConn(); 
            System.Data.OracleClient.OracleConnection OracleConnect = new System.Data.OracleClient.OracleConnection(ConString);
            OracleConnect.Open();
            System.Data.OracleClient.OracleCommand OracleCom = new System.Data.OracleClient.OracleCommand("DELETE From  安全防护单位文件存储表  WHERE 文件名称 = '" + FileName + "'", OracleConnect);
            OracleCom.ExecuteNonQuery();
            OracleConnect.Close();
        }

        public static void FileUpdata(string FileName, string FilePath)
        {
            string ConString;
            
            string Section = "文件";
            ConString =OracelData.getStrConn(); 
            System.Data.OracleClient.OracleConnection OracleConnect = new System.Data.OracleClient.OracleConnection(ConString);
            OracleConnect.Open();
            System.Data.OracleClient.OracleCommand OracleCom = new System.Data.OracleClient.OracleCommand("SELECT 文件版本 FROM 文件存储表 WHERE 文件名称 = '" + FileName + "'", OracleConnect);
            double i = double.Parse(OracleCom.ExecuteOracleScalar().ToString());
            i = i + 0.1;
            System.IO.File.Copy(FilePath + "\\" + FileName, "W:\\" + FileName, true);
            INIClass.IniWriteValue(Section, FileName, i.ToString());
            OracleCom.CommandText = "UPDATE  安全防护单位文件存储表  SET 文件版本 = '" + i.ToString() + "',本机位置 = '" + FilePath + "' WHERE 文件名称 = '" + FileName + "'";
            OracleCom.ExecuteNonQuery();
            OracleConnect.Close();
        }

        public static bool FileExist(string FileName)
        {
            string ConString;
            ConString =OracelData.getStrConn(); 
            System.Data.OracleClient.OracleConnection OracleConnect = new System.Data.OracleClient.OracleConnection(ConString);
            OracleConnect.Open();
            System.Data.OracleClient.OracleCommand OracleCom = new System.Data.OracleClient.OracleCommand("SELECT COUNT(*) FROM  安全防护单位文件存储表  WHERE  文件名称 = '" + FileName + "'", OracleConnect);
            int i = int.Parse(OracleCom.ExecuteOracleScalar().ToString());
            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class INIClass
    {
        private static string IniPath = "Config\\File.ini";
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        /// 构造方法
        /// 文件路径  
        ///  
        /// 写入INI文件  
        ///  
        /// 项目名称(如 [TypeName] )  [Page]
        /// 键  
        /// 值  
        public static void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, IniPath);
        }
        ///  
        /// 读出INI文件    
        /// 项目名称(如 [TypeName] )  
        /// 键  
        public static string IniReadValue(string Section, string Key)
        {
            StringBuilder Temp = new StringBuilder(500);
            int i = GetPrivateProfileString(Section, Key, "", Temp, 500, IniPath);
            return Temp.ToString();
        }
        ///  
        /// 验证文件是否存在  
        ///  
        /// 布尔值  
        public bool ExistINIFile()
        {
            return System.IO.File.Exists(IniPath);
        }
    }

}
