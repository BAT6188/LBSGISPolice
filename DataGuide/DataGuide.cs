using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data;
using CommonLib;
using Rainny.DataAccess;
using System.Drawing;
using System.Reflection;

namespace LBSDataGuide
{
    public class DataGuide : LBSDataGuide.IDataGuide
    {
        public DataGuide()
        {
            _connStr = GetConnStr();
            ConnectionProperty cp = new ConnectionProperty();
            cp.ConnectionString = _connStr;
            cp.DatabaseType = DatabaseType.Oracle;
            _iDataAccess = DAFactory.CreateDataAccess(cp);

        }
        private string _connStr;
        private IDataAccess _iDataAccess;
        private List<string> _listInsert;
        private List<string> _listUpdate;
        private List<string> _listNO;
        private string sqlInsert;      // 存储插入时字段  lili 2010-6-7
         
        /// <summary>
        /// 导入数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dataPath">Excel路径</param>
        /// <param name="tableName">表名</param>
        /// <returns>导入成功记录数</returns>
        public int InData(string dataPath, string tableName)
        {
            int dataCount = 0;
            frmSchedule frmSch = new frmSchedule("导入");
            try
            {
                _listInsert = new List<string>();
                _listUpdate = new List<string>();
                _listNO = new List<string>();
                
                int maxMi_prinx = 0;

                GetFromName gName = new GetFromName(tableName);
                string objID = gName.ObjID;
                string objName = gName.ObjName;

                XlsConn xlsConn = GetXlsConn(dataPath);
                string sheetName = GetSheetName(dataPath);
                if (sheetName == "")
                {
                    sheetName = "Sheet1";
                }
                string selectSql = "select * from [" + sheetName + "$]";

                DataTable dt = xlsConn.DataTable(selectSql);
                if (dt == null)
                {
                    MessageBox.Show("excel中无数据表");
                    return 0;
                }

                _iDataAccess.Open();
                sqlInsert = "";
                // 获取要插入表的所有字段名
                for (int s = 0; s < dt.Columns.Count; s++)
                {
                    if (tableName == "案件信息" || tableName == "人口系统" || tableName == "出租屋房屋系统")
                    {
                        if ((dt.Columns[s].ColumnName.ToUpper() == "MAPID" && tableName != "视频位置") || dt.Columns[s].ColumnName.ToUpper() == "X" || dt.Columns[s].ColumnName.ToUpper() == "Y")
                            continue;
                        else
                        {
                            
                            sqlInsert += dt.Columns[s].ColumnName + ",";
                        }
                    }
                    else
                    {
                        sqlInsert += dt.Columns[s].ColumnName + ",";
                    }
                }
                if (tableName == "案件信息" || tableName == "人口系统" || tableName == "出租屋房屋系统")
                {
                    sqlInsert += "mi_prinx";
                }
                else
                {
                    sqlInsert = sqlInsert.Remove(sqlInsert.Length - 1, 1);
                }

                #region 判断表名

                frmSch.progressBar1.Value = 0;
                frmSch.progressBar1.Maximum = dt.Rows.Count;
                frmSch.Show();
                Application.DoEvents();

                if ((tableName == "案件信息" || tableName == "人口系统" || tableName == "出租屋房屋系统"))
                {
                    #region

                    string sql = "select max(Mi_prinx) from " + tableName;
                    try
                    {
                        maxMi_prinx = Convert.ToInt32(_iDataAccess.ExecuteScalar(sql));
                    }
                    catch
                    { }
                    maxMi_prinx++;
                  
                    int iError = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow dr = dt.Rows[i];
                        string id = Convert.ToString(dr[objID]);
                        string idSql = "select " + objID + "," + objName + " from " + tableName + " where " + objID + "='" + id + "'";
                        IDataReader idr = _iDataAccess.ExecuteDataReader(idSql);
                        if (idr.Read())
                        {
                            #region update
                            string  updateSql = "Update " + tableName + " set ";

                            if (dr["X"].ToString() != "")
                            {
                                updateSql = " update " + tableName + " set  GEOLOC = MDSYS.SDO_GEOMETRY(2001, 8307, MDSYS.SDO_POINT_TYPE(" + dr["X"].ToString() + "," + dr["Y"].ToString() + ", NULL),NULL, NULL),  ";
                            }

                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                string columnsName = dt.Columns[j].ColumnName;

                                if (dr[columnsName].ToString() == "")
                                {
                                    continue;
                                }
                                if (columnsName.IndexOf("日期") > -1 || columnsName.IndexOf("时间") > -1)
                                {
                                    if (columnsName == "出生日期")
                                        updateSql += columnsName + "='" + dr[columnsName].ToString() + "',";
                                    else
                                        updateSql += columnsName + "=to_date('" + dr[columnsName].ToString() + "','yy-mm-dd hh24:mi:ss'),";
                                }

                                else if (columnsName == "照片")
                                {
                                    updateSql += "照片='',";
                                }
                                else if (columnsName.ToUpper() != "X" && columnsName.ToUpper() != "Y")
                                {
                                    if (tableName == "案件信息" && columnsName.ToUpper() == "MAPID") continue; 
                                    updateSql += columnsName + "='" + dr[columnsName].ToString() + "',";
                                }
                            }
                            updateSql = updateSql.Remove(updateSql.LastIndexOf(',')) + " where " + objID + "='" + dr[objID].ToString() + "'";
                            try
                            {
                                WriteLine("更新:" + updateSql);
                                int count = _iDataAccess.ExecuteNonQuery(updateSql);
                                dataCount = dataCount + count;
                                if (count > 0 || updateSql == "")
                                {
                                    _listUpdate.Add(dr[objID].ToString());
                                }
                            }
                            catch (System.Data.OracleClient.OracleException ex)
                            {
                                if (ex.Code == 1407)
                                {
                                    iError++;
                                }
                                _listNO.Add( dr[objID].ToString() + "  " + dr[objName].ToString());
                                WriteLine(ex.ToString());
                            }
                            #endregion
                        }
                        else
                        {
                            // gName.SQLInsert
                            #region insert
                            string insertSql = "INSERT INTO " + tableName + "(" + sqlInsert + ") VALUES (";
                            string insertId = "", insertName = "";
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                string columnsName = dt.Columns[j].ColumnName;

                                if (columnsName.IndexOf("日期") > -1 || columnsName.IndexOf("时间") > -1)
                                {
                                    if (columnsName == "出生日期")
                                        insertSql += "'" + dr[columnsName].ToString().Trim() + "',";
                                    else
                                        insertSql += "to_date('" + dr[columnsName].ToString() + "','yy-mm-dd hh24:mi:ss'),";
                                    continue;
                                }
                                if ((columnsName.ToUpper() == "MAPID" || columnsName.ToUpper() == "X" || columnsName.ToUpper() == "Y"))
                                {
                                    continue;
                                }
                                else if (columnsName == objID)
                                {
                                    insertSql += "'" + dr[columnsName].ToString() + "',";
                                    insertId = dt.Rows[i][objID].ToString().Trim();
                                    insertName = dt.Rows[i][objName].ToString().Trim();
                                }
                                else if (columnsName == "照片")
                                {
                                    insertSql += "'',";
                                }
                                else
                                {
                                    insertSql += "'" + dr[columnsName].ToString().Trim() +"',";
                                }
                            }
                           
                            insertSql += "'" + maxMi_prinx + "')";
                            maxMi_prinx++;
                           

                            try
                            {
                                WriteLine("增加:" + insertSql);
                                int count = _iDataAccess.ExecuteNonQuery(insertSql);
                                dataCount = dataCount + count;
                                if (count > 0)
                                {
                                    _listInsert.Add( insertId );
                                }
                            }
                            catch (System.Data.OracleClient.OracleException ex)
                            {
                                if (ex.Code == 1407)
                                {
                                    iError++;

                                }
                                _listNO.Add( dr[objID].ToString() + "  " + dr[objName].ToString());
                                WriteLine(ex.ToString());
                            }

                            if (dr["X"].ToString() != "")
                            {
                                string updateSql = " update " + tableName + "  set  GEOLOC = MDSYS.SDO_GEOMETRY(2001, 8307, MDSYS.SDO_POINT_TYPE(" + dr["X"].ToString() + "," + dr["Y"].ToString() + ", NULL),NULL, NULL) where " + objID + "='" + dr[objID].ToString() + "'";
                                try
                                {
                                    int count = _iDataAccess.ExecuteNonQuery(updateSql);

                                }
                                catch (System.Data.OracleClient.OracleException ex)
                                {
                                    if (ex.Code == 1407)
                                    {
                                        iError++;

                                    }
                                    // _listNO.Add(dr[objID].ToString());
                                    WriteLine(ex.ToString());
                                }
                            }

                            #endregion
                        } 
                        idr.Close();
                        frmSch.progressBar1.Value += 1;
                        Application.DoEvents();
                    }
                    frmSch.Close();

                    Console.WriteLine("导入完成时间:" + System.DateTime.Now);

                    if (iError > 0 && dataCount > 0)
                    {
                        MessageBox.Show("有 " + iError.ToString() + " 条记录的非标识字段存在非空值,不能导入或更新,\r\r详情请查看DataGuide.Log!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    string txt = "更新数据：";
                    for (int i = 0; i < _listUpdate.Count; i++)
                    {
                        txt += _listUpdate[i] + ",";
                    }
                    txt += "\r\n导入数据：";
                    for (int i = 0; i < _listInsert.Count; i++)
                    {
                        txt += _listInsert[i] + ",";
                    }
                    txt += "\r\n失败数据：\r\n";
                    for (int i = 0; i < _listNO.Count; i++)
                    {
                        txt += _listNO[i] + "； \r\n";
                    }
                    WriteID(txt);
                    #endregion
                }
                else
                {
                    #region

                    Hashtable htId = new Hashtable();
                    
                    string idSql = "select " + objID + " from " + tableName;
                    IDataReader idr = _iDataAccess.ExecuteDataReader(idSql);
                    while (idr.Read())
                    {
                        try
                        {
                            htId.Add(Convert.ToString(idr.GetValue(0)), Convert.ToString(idr.GetValue(0)));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            // WriteLine(ex.ToString());
                        }

                    }
                    //_iDataAccess.Close();

                    //_iDataAccess.Open();
                    int iError = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        string id = Convert.ToString(dr[objID]);
                        string name = dr[objName].ToString();
                        if (htId.Contains(id))
                        {
                            string updateSql = "";
                            updateSql = "Update " + tableName + " set ";

                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                string columnsName = dt.Columns[j].ColumnName;

                                if (dr[columnsName].ToString() == "")
                                {
                                    continue;
                                }
                                if (columnsName.IndexOf("日期") > -1 || columnsName.IndexOf("时间") > -1)
                                {
                                    if (columnsName == "出生日期")
                                        updateSql += columnsName + "='" + dr[columnsName].ToString() + "',";
                                    else
                                        updateSql += columnsName + "=to_date('" + dr[columnsName].ToString() + "','yy-mm-dd hh24:mi:ss'),";
                                }

                                else if (columnsName == "照片")
                                {
                                    updateSql += "照片='',";
                                }
                                else if (columnsName.ToUpper() != "MAPID")
                                {
                                    updateSql += columnsName + "='" + dr[columnsName].ToString() + "',";
                                }

                            }
                            updateSql = updateSql.Remove(updateSql.LastIndexOf(',')) + " where " + objID + "='" + dr[objID].ToString() + "'";

                            try
                            {
                                WriteLine("更新:" + updateSql);
                                int count = _iDataAccess.ExecuteNonQuery(updateSql);
                                dataCount = dataCount + count;
                                if (count > 0 || updateSql == "")
                                {
                                    _listUpdate.Add( dr[objID].ToString());
                                }

                            }
                            catch (System.Data.OracleClient.OracleException ex)
                            {
                                if (ex.Code == 1407)
                                {
                                    iError++;

                                }
                                _listNO.Add(dr[objID].ToString() + "  " + dr[objName].ToString());
                                WriteLine(ex.ToString());
                            }


                        }
                        else
                        {
                            if (tableName == "视频位置")
                            {
                                continue;
                            }
                            string insertSql = "INSERT INTO " + tableName + "(" + sqlInsert + ") VALUES (";

                            string insertId = "", insertName = "";
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                string columnsName = dt.Columns[j].ColumnName;

                                if (columnsName.IndexOf("日期") > -1 || columnsName.IndexOf("时间") > -1)
                                {
                                    if (columnsName == "出生日期")
                                        insertSql += "'" + dr[columnsName].ToString() + "',";
                                    else
                                        insertSql += "to_date('" + dr[columnsName].ToString() + "','yy-mm-dd hh24:mi:ss'),";
                                    continue;
                                }
                               // if ((tableName != "案件信息" || tableName != "人口系统" || tableName != "出租屋房屋系统") && (columnsName.ToUpper() == "MAPID"))
                               // {
                               //     insertSql += "'" + 1 + "',";
                               // }
                               // else 
                                if (columnsName == objID)
                                {
                                   // if (hasIntCode)
                                    //{
                                    //    if (dt.Rows[i][objID].ToString().Trim() == "")
                                    //    {
                                    //        insertId = getMaxObjID(tableName, objID);
                                    //        insertSql += "'" + getMaxObjID(tableName, objID) + "',";
                                    //    }
                                    //    else
                                    //    {
                                    //        insertId = dt.Rows[i][objID].ToString().Trim();
                                    //        insertSql += "'" + dr[columnsName].ToString() + "',";
                                    //    }
                                    //}
                                   // else
                                   // {
                                        insertSql += "'" + dr[columnsName].ToString() + "',";
                                        insertId = dt.Rows[i][objID].ToString().Trim();
                                        insertName = dt.Rows[i][objName].ToString();
                                   // }
                                }
                                else if (columnsName == "照片")
                                {
                                    insertSql += "'',";
                                }
                                else
                                {
                                    insertSql += "'" + dr[columnsName].ToString() + "',";
                                }
                            }
                            
                            insertSql = insertSql.Remove(insertSql.LastIndexOf(',')) + ")";

                            try
                            {
                                WriteLine("增加:"+insertSql);
                                int count = _iDataAccess.ExecuteNonQuery(insertSql);
                                dataCount = dataCount + count;
                                if (count > 0)
                                {
                                    _listInsert.Add( insertId);

                                }
                            }
                            catch (System.Data.OracleClient.OracleException ex)
                            {
                                if (ex.Code == 1407)
                                {
                                    iError++;

                                }
                                _listNO.Add( dr[objID].ToString() + "  " + dr[objName].ToString());
                                WriteLine(ex.ToString());
                            }
                        }
                        frmSch.progressBar1.Value += 1;
                        Application.DoEvents();
                    }
                    frmSch.Close();

                    Console.WriteLine("导入完成时间:" + System.DateTime.Now);

                    if (iError > 0 && dataCount > 0)
                    {
                        MessageBox.Show("有 " + iError.ToString() + " 条记录的非标识字段存在非空值,不能导入或更新,\r\r详情请查看DataGuide.Log!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    string txt = "更新数据：";
                    for (int i = 0; i < _listUpdate.Count; i++)
                    {
                        txt += _listUpdate[i] + ",";
                    }
                    txt += "\r\n导入数据：";
                    for (int i = 0; i < _listInsert.Count; i++)
                    {
                        txt += _listInsert[i] + ",";
                    }
                    txt += "\r\n失败数据：\r\n";
                    for (int i = 0; i < _listNO.Count; i++)
                    {
                        txt += _listNO[i] + "； \r\n";
                    }
                    WriteID(txt);
                    #endregion
                }

                _iDataAccess.Close();
                #endregion
                return _listUpdate.Count + _listInsert.Count;
            }
            catch (Exception ex)
            {
                frmSch.Close();
                WriteLine(ex.Message);
                return _listUpdate.Count + _listInsert.Count;
            }
        }

        /// <summary>
        /// 找到最大的ID号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="objID">表ID</param>
        /// <returns>最大ID号</returns>
        private string getMaxObjID(string tableName, string objID)
        {
            try
            {
                //_iDataAccess.Open();
                string strExp = "select max(" + objID + ") from " + tableName;
                int newMapid = 1;
                IDataReader idr = _iDataAccess.ExecuteDataReader(strExp);
                if (idr.Read())
                {
                    newMapid = Convert.ToInt32(idr.GetValue(0)) + 1;
                }
                //_iDataAccess.Close();
                return newMapid.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }
        /// <summary>
        /// 导出数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <param name="dataPath">Excel 路径</param>
        /// <param name="tableName">表名</param>
        /// <returns>导出是否成功（布尔值）</returns>
        public bool OutData(string dataPath, string tableName)
        {
            try
            {
                GetFromName getName = new GetFromName(tableName);

                string strSQL = "";
                if (tableName == "案件信息" || tableName == "人口系统" || tableName == "出租屋房屋系统")
                {
                    strSQL = "select MI_PRINX as MapID," + getName.SQLFields + " from " + tableName + " t";
                }
                else
                {
                    strSQL = "select * from " + tableName;
                }

                //string sql = "select * from " + tableName;
                _iDataAccess.Open();
                DataTable dt = _iDataAccess.ExecuteDatatable(strSQL);
                return ExportDataTable(dataPath, dt,getName.ObjID);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 导出数据 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25         
        /// </summary>
        /// <param name="dataPath">导出路径</param>
        /// <param name="dt">导出数据源</param>
        /// <returns>导出是否成功（布尔值）</returns>
        public bool OutData(string dataPath, DataTable dt,string tableName)
        {
            try
            {
                GetFromName getFrom = new GetFromName(tableName);
                return ExportDataTable(dataPath, dt, getFrom.ObjID);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 导出数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        /// <param name="dataPath">导出路径</param>
        /// <param name="dt">导出数据源</param>
        /// <returns>导出是否成功（布尔值）</returns>
        public bool OutData(string dataPath, DataTable dt)
        {
            try
            {
                return ExportDataTable(dataPath, dt);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Excel连接字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        /// <param name="xlsPath">Excel路径</param>
        /// <returns>连接对象</returns>
        private XlsConn GetXlsConn(string xlsPath)
        {
            try
            {
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + xlsPath + ";" + "Extended Properties='Excel 8.0;HDR=YES;IMEX=1';";
                // string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Extended Properties='Excel 8.0;IHDR=Yes;IMEX=1;'" + "data source=" + xlsPath;
                return new XlsConn(connStr);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 取得数据库连接字符串
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25
        /// </summary>
        /// <returns>数据库连接字符串</returns>
        private string GetConnStr()
        {
            try
            {
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                string datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                string userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                string password = CLC.INIClass.IniReadValuePW("数据库", "密码");
                return "user id = " + userid + ";data source =" + datasource + ";password =" + password;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return null;
            }
        }


        /// <summary>   
        /// 将指定Excel文件中读取第一张工作表的名称 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>   
        /// <param name="filePath">要读取的Excel文件路径</param>   
        /// <returns>工作表的名称</returns>   
        private string GetSheetName(string filePath)
        {
            try
            {
                string sheetName = "";

                System.IO.FileStream tmpStream = File.OpenRead(filePath);
                byte[] fileByte = new byte[tmpStream.Length];
                tmpStream.Read(fileByte, 0, fileByte.Length);
                tmpStream.Close();

                byte[] tmpByte = new byte[]{Convert.ToByte(11),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),   
    Convert.ToByte(11),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),   
  //   Convert.ToByte(11),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),   
  //   Convert.ToByte(11),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),Convert.ToByte(0),   
    Convert.ToByte(30),Convert.ToByte(16),Convert.ToByte(0),Convert.ToByte(0)};

                int index = GetSheetIndex(fileByte, tmpByte);
                if (index > -1)
                {
                    //index+=32+12;   
                    index += 16 + 12;
                    System.Collections.ArrayList sheetNameList = new System.Collections.ArrayList();

                    for (int i = index; i < fileByte.Length - 1; i++)
                    {
                        byte temp = fileByte[i];
                        if (temp != Convert.ToByte(0))
                            sheetNameList.Add(temp);
                        else
                            break;
                    }
                    byte[] sheetNameByte = new byte[sheetNameList.Count];
                    for (int i = 0; i < sheetNameList.Count; i++)
                        sheetNameByte[i] = Convert.ToByte(sheetNameList[i]);

                    sheetName = System.Text.Encoding.Default.GetString(sheetNameByte);
                }
                return sheetName;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return null;
            }
        }
        /// <summary>   
        /// 只供方法GetSheetName()使用 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25    
        /// </summary>   
        /// <returns></returns>   
        private int GetSheetIndex(byte[] FindTarget, byte[] FindItem)
        {
            try
            {
                int index = -1;

                int FindItemLength = FindItem.Length;
                if (FindItemLength < 1) return -1;
                int FindTargetLength = FindTarget.Length;
                if ((FindTargetLength - 1) < FindItemLength) return -1;

                for (int i = FindTargetLength - FindItemLength - 1; i > -1; i--)
                {
                    System.Collections.ArrayList tmpList = new System.Collections.ArrayList();
                    int find = 0;
                    for (int j = 0; j < FindItemLength; j++)
                    {
                        if (FindTarget[i + j] == FindItem[j]) find += 1;
                    }
                    if (find == FindItemLength)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return 0;
            }
        }


        /// <summary>
        /// 导出Excel 工作表的名称 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        /// <param name="fileName">保存路径</param>
        /// <param name="dt">导出的数据</param>
        /// <param name="tableName">表名</param>
        /// <returns>导出是否成功（布尔值）</returns>
        private bool ExportDataTable(string fileName, DataTable dt, string UniqueCol)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                Excel.ApplicationClass xlApp = new Excel.ApplicationClass();

                if (xlApp == null)
                {

                    MessageBox.Show("无法创建Excel对象，可能您的机器未安装Excel！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                frmSchedule frmSch = new frmSchedule("导出");
                frmSch.progressBar1.Value = 0;
                frmSch.progressBar1.Maximum = dt.Rows.Count;
                frmSch.Show();
                Application.DoEvents();

                Excel.Workbooks workbooks = xlApp.Workbooks;
                Excel.Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                Excel.Range rng3 = null;
                Application.DoEvents();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (i == 0)
                        {
                            worksheet.Cells[1, j + 1] = dt.Columns[j].ColumnName;
                            if (UniqueCol != "")
                            {
                                rng3 = worksheet.get_Range(GetZi(j + 1) + (i + 1), Missing.Value);   // 获得单元格

                                // 以下列名的单元格背景颜色改变，方便公安导入数据
                                if (dt.Columns[j].ColumnName.ToUpper() == "X" || dt.Columns[j].ColumnName.ToUpper() == "Y" 
                                                                              || dt.Columns[j].ColumnName == "标注人" 
                                                                              || dt.Columns[j].ColumnName == "标注时间" 
                                                                              || dt.Columns[j].ColumnName == UniqueCol)
                                {
                                    rng3.Interior.Color = ColorTranslator.ToWin32(Color.Red);  // 改变单元格的背景颜色 
                                }
                            }
                        }
                        worksheet.Cells[i + 2, j + 1] = "'" + dt.Rows[i][j].ToString();
                    }
                    frmSch.progressBar1.Value += 1;
                    Application.DoEvents();
                }
                frmSch.Close();

                workbook.Saved = true;
                workbook.SaveCopyAs(fileName);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                worksheet = null;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;
                workbooks.Close();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbooks);
                workbooks = null;
                xlApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                xlApp = null;
                return true;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="fileName">保存路径</param>
        /// <param name="dt">导出的数据</param>
        /// <returns>导出是否成功</returns>
        private bool ExportDataTable(string fileName, DataTable dt)
        {
            try
            {
                // 显示进度条
                frmSchedule frmSch = new frmSchedule("导出");
                frmSch.progressBar1.Value = 0;
                frmSch.progressBar1.Maximum = dt.Rows.Count;
                frmSch.Show();
                Application.DoEvents();

                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                Excel.ApplicationClass xlApp = new Excel.ApplicationClass();

                if (xlApp == null)
                {

                    MessageBox.Show("无法创建Excel对象，可能您的机器未安装Excel！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                Excel.Workbooks workbooks = xlApp.Workbooks;
                Excel.Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                Application.DoEvents();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (i == 0)
                        {
                            worksheet.Cells[1, j + 1] = dt.Columns[j].ColumnName;
                        }

                        worksheet.Cells[i + 2, j + 1] = "'" + dt.Rows[i][j].ToString();
                    }
                    frmSch.progressBar1.Value += 1;
                    Application.DoEvents();
                }
                frmSch.Close();

                workbook.Saved = true;
                workbook.SaveCopyAs(fileName);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                worksheet = null;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;
                workbooks.Close();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbooks);
                workbooks = null;
                xlApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                xlApp = null;
                return true;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 根据传入的数字找到对应的EXCEL列名
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        /// <param name="j">数字</param>
        /// <returns>Excel的列字母</returns>
        private string GetZi(int j)
        {
            string s = "";
            try
            {
                #region 根据数字找到Excel列
                switch (j) 
                {
                    case 1:
                        s = "A";
                        break;
                    case 2:
                        s = "B";
                        break;
                    case 3:
                        s = "C";
                        break;
                    case 4:
                        s = "D";
                        break;
                    case 5:
                        s = "E";
                        break;
                    case 6:
                        s = "F";
                        break;
                    case 7:
                        s = "G";
                        break;
                    case 8:
                        s = "H";
                        break;
                    case 9:
                        s = "I";
                        break;
                    case 10:
                        s = "J";
                        break;
                    case 11:
                        s = "K";
                        break;
                    case 12:
                        s = "L";
                        break;
                    case 13:
                        s = "M";
                        break;
                    case 14:
                        s = "N";
                        break;
                    case 15:
                        s = "O";
                        break;
                    case 16:
                        s = "P";
                        break;
                    case 17:
                        s = "Q";
                        break;
                    case 18:
                        s = "R";
                        break;
                    case 19:
                        s = "S";
                        break;
                    case 20:
                        s = "T";
                        break;
                    case 21:
                        s = "U";
                        break;
                    case 22:
                        s = "V";
                        break;
                    case 23:
                        s = "W";
                        break;
                    case 24:
                        s = "X";
                        break;
                    case 25:
                        s = "Y";
                        break;
                    case 26:
                        s = "Z";
                        break;
                    case 27:
                        s = "AA";
                        break;
                    case 28:
                        s = "AB";
                        break;
                    case 29:
                        s = "AC";
                        break;
                    case 30:
                        s = "AD";
                        break;
                    case 31:
                        s = "AE";
                        break;
                    case 32:
                        s = "AF";
                        break;
                    case 33:
                        s = "AG";
                        break;
                    case 34:
                        s = "AH";
                        break;
                    case 35:
                        s = "AI";
                        break;
                    case 36:
                        s = "AJ";
                        break;
                    case 37:
                        s = "AK";
                        break;
                    case 38:
                        s = "AL";
                        break;
                    case 39:
                        s = "AM";
                        break;
                    case 40:
                        s = "AN";
                        break;
                    case 41:
                        s = "AO";
                        break;
                    case 42:
                        s = "AP";
                        break;
                    case 43:
                        s = "AQ";
                        break;
                    case 44:
                        s = "AR";
                        break;
                    case 45:
                        s = "AS";
                        break;
                    default:
                        s = "BS";
                        break;
                }
                #endregion
                return s;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 导入日志报告
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        /// <param name="txt">导出的详细信息</param>
        private void WriteID(string txt)
        {
            try
            {
                string FilePath = Application.StartupPath + @"\Export.txt";
                string NewFilePath = Application.StartupPath + @"\Export_OLD.txt";

                if (File.Exists(FilePath))
                {
                    if (File.Exists(NewFilePath))
                        File.Delete(NewFilePath);
                    
                    File.Move(FilePath, NewFilePath);  // 先给旧文件改名
                    
                    //File.Delete(FilePath);             // 然后删除该文件
                }

                // 从新创建文件来存储新一次导入日志
                FileStream fs = new FileStream(FilePath, FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fs);
                streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                streamWriter.WriteLine(txt);
                streamWriter.Flush();
                fs.Close();
            }
            catch(Exception ex)
            {
                WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 错误输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-25  
        /// </summary>
        /// <param name="format">日志内容</param>
        private void WriteLine(string format)
        {
            FileStream fs = new FileStream(Application.StartupPath + @"\DataGuide" + System.DateTime.Now.ToShortDateString() + ".Log", FileMode.Append);
            StreamWriter streamWriter = new StreamWriter(fs);
            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            streamWriter.WriteLine(DateTime.Now.ToString() + ":" + format);
            streamWriter.Flush();
            fs.Close();
        }
    }
}
