using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using System.Windows.Forms;
using System.IO;
using System.Web;
using System.Data;
using System.Xml;
namespace clZhihui
{
	public class OracelData
	{
        private string strSQL;
		//与SQL Server的连接字符串设置

        public static string connectionString = "";
		//与数据库的连接
		private System.Data.OracleClient.OracleConnection  myConnection;
		
		private System.Data.OracleClient.OracleCommandBuilder  oracleCmdBld;
		private DataSet ds = new DataSet();
		private System.Data.OracleClient.OracleDataAdapter  da;

		public OracelData(string s)
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
            connectionString =s;
		}

		/////////////////////////////////  操作脱机数据库(创建了该类的实例时直接用)  /////////////////////////////////////////////////////
		
		//根据输入的SQL语句检索数据库数据
		public DataSet SelectDataBase(string tempStrSQL,string tempTableName)//返回的是DataSet
		{ 
			try
			{
				this.strSQL = tempStrSQL;
				this.myConnection = new System.Data.OracleClient.OracleConnection(connectionString);
				this.da = new System.Data.OracleClient.OracleDataAdapter(this.strSQL,this.myConnection);
				this.ds.Clear();
				this.da.Fill(ds,tempTableName);
				this.myConnection.Close();
				return ds;//返回填充了数据的DataSet，其中数据表以tempTableName给出的字符串命名
			}
			catch(System.Data.OracleClient.OracleException  ex)
			{
                System.Console.WriteLine(ex.Message);
                return null;
			}
		}

		//数据库数据更新(传DataSet和DataTable的对象)
		public DataSet UpdateDataBase(DataSet changedDataSet,string tableName)//用数据库中的数据更新DataSet和DataTable
		{
            try
            {
                this.myConnection = new System.Data.OracleClient.OracleConnection(connectionString);
                this.da = new System.Data.OracleClient.OracleDataAdapter(this.strSQL, this.myConnection);
                this.oracleCmdBld = new System.Data.OracleClient.OracleCommandBuilder(da);
                this.da.Update(changedDataSet, tableName);
                myConnection.Close();
                return changedDataSet;//返回更新了的数据库表
            }
            catch (System.Data.OracleClient.OracleException ex)
            {
                string ss = ex.Message;
                return null;
            }


		}

		/////////////////////////////////  直接操作数据库(未创建该类的实例时直接用)  /////////////////////////////////////////////////////

		//检索数据库数据(传字符串,直接操作数据库)
		public DataTable SelectDataBase(string tempStrSQL)//返回的是一个DataTable
		{
            try
            {
                this.myConnection = new System.Data.OracleClient.OracleConnection(connectionString);
                DataSet tempDataSet = new DataSet();
                this.da = new System.Data.OracleClient.OracleDataAdapter(tempStrSQL, this.myConnection);
                this.da.Fill(tempDataSet);
                myConnection.Close();
                return tempDataSet.Tables[0];
            }
            catch (System.Data.OracleClient.OracleException ex)
            {
                string ss = ex.Message;
               // MessageBox.Show("查询语句不正确,请检查");
                return null;
            }
		}

		//数据库数据更新(传字符串，直接操作数据库)
		public int UpdateDataBase(string tempStrSQL)//对数据库进行更新
		{
            try
            {   
                this.myConnection = new System.Data.OracleClient.OracleConnection(connectionString);
                //使用Command之前一定要先打开连接,后关闭连接,而DataAdapter则会自动打开关闭连接
                myConnection.Open();
                System.Data.OracleClient.OracleCommand tempOracleCommand = new System.Data.OracleClient.OracleCommand(tempStrSQL, this.myConnection);
                int intNumber = tempOracleCommand.ExecuteNonQuery();//返回数据库中影响的行数
                myConnection.Close();
                return intNumber;
            }
            catch(System.Data.OracleClient.OracleException ex)
            {
                string ss = ex.Message;
                return 0;

            }
		}  
    }
}




