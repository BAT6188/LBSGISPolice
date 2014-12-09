using System;
using System.Collections.Generic;
using System.Text;
using MapInfo.Data;
using System.Data;

namespace clGISPoliceEdit
{
    public class MIConn
    {
        private MIConnection conn;
        private MICommand cmd;

        private void ConnectionDB()
        {
            try
            {
                if (conn != null)
                {
                    conn.Close();
                }
                conn = new MIConnection();
                conn.Open();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }


        public string ExecuteScalar(string Sql)
        {
            try
            {
                ConnectionDB();
                cmd = conn.CreateCommand();
                cmd.CommandText = Sql;
                object oj = cmd.ExecuteScalar();
                return oj.ToString();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;

            }
            finally
            {
                cmd.Clone();
                cmd.Dispose();
                conn.Close();
            }
        }
        public void CreateCommand(string Sql)
        {
            ConnectionDB();
            cmd = conn.CreateCommand();
            cmd.CommandText = Sql;
        }


        public void AddParameters(string parName, object o)
        {
            cmd.Parameters.Add(parName, o);
        }
        public int ExecuteParameters()
        {
            try
            {
                int mess = cmd.ExecuteNonQuery();
                return mess;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return 0;

            }
            finally
            {
                cmd.Clone();
                cmd.Dispose();
                conn.Close();
            }
        }
        public int ExecuteNonQuery(string Sql)
        {
            try
            {
                ConnectionDB();
                cmd = conn.CreateCommand();
                cmd.CommandText = Sql;
                int mess = cmd.ExecuteNonQuery();
                return mess;

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return 0;

            }
            finally
            {
                cmd.Clone();
                cmd.Dispose();
                conn.Close();
            }

        }

        public MIDataReader ExecuteReader(string Sql)
        {
            try
            {
                ConnectionDB();
                cmd = conn.CreateCommand();
                cmd.CommandText = Sql;
                MIDataReader dataReader = cmd.ExecuteReader();
                return dataReader;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }
        public System.Data.DataTable DataTable(string Sql)
        {
            try
            {
                ConnectionDB();
                cmd = conn.CreateCommand();
                cmd.CommandText = Sql;
                MIDataReader miReader = cmd.ExecuteReader();
                DataTable dt = new DataTable("Data");
                for (int i = 0; i < miReader.FieldCount; i++)
                {
                    DataColumn dc = dt.Columns.Add(miReader.GetName(i));
                }
                while (miReader.Read())
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < miReader.FieldCount; i++)
                    {
                        dr[i] = miReader.GetValue(i);
                    }
                    dt.Rows.Add(dr);
                }
                miReader.Close();
                return dt;

            }

            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                cmd.Clone();
                cmd.Dispose();
                conn.Close();
            }

        }

        public void Close()
        {
            try
            {
                if (cmd != null)
                {
                    cmd.Clone();
                    cmd.Dispose();
                }
                if (conn != null)
                {
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }


        }
    }



}


