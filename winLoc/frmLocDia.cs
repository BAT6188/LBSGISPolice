using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Windows.Controls;
using GeoCoding;
using System.IO;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Geometry;
using MapInfo.Mapping;
using Rainny.DataAccess;
using CommonLib;
namespace winLoc
{
    public partial class frmLocDia : Form
    {
        private MapControl mapControl1 = null;
        private Table _pointTable;
        private string _cityName = "顺德";
        private Coding _coding;
        private GeoSplitNormal _geoSplitNormal;
        private GeoSplitMatch _geoSplitMatch = null;
        private string datasource, userid, password; // add by fisher in 09-12-29 
        private GeoCity _geoCity;
        private IDataAccess _iDataAccess;
        MapInfo.Data.Table editLocTable = null;  //当前的编辑图层

        public frmLocDia(MapControl m)
        {
            InitializeComponent();
            mapControl1 = m;
            try
            {
                ///连接数据库   add by fisher in 09-12-29
                string exePath = Application.StartupPath;
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                password = CLC.INIClass.IniReadValuePW("数据库", "密码");

                InitCoding();//jie.zhang 20100517 

                this.GetTable("民警中队辖区");
               
                this.TopMost = true;
                if (mapControl1.Map.Layers["Location_temp"] == null)
                {
                    this.editLocTable = ctempTable("Location_temp"); //创建临时图层
                    ////地图显示
                    FeatureLayer fl = new FeatureLayer(editLocTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);
                }
            }
            catch { }
        }


        private void InitCoding()
        {
            if (MapInfo.Engine.Session.Current.Catalog.GetTable("CodingPoint") == null)
            {
                InsertLayer();
                //CodingDBInit();

                //this.editLocTable = ctempTable("Location_temp"); //创建临时图层
                ////地图显示
                //FeatureLayer fl = new FeatureLayer(editLocTable);
                //mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);
            }
            CodingDBInit();
        }


        /// <summary>
        ///GeoCoding 初始化
        /// </summary>
        public void CodingDBInit()
        {
            try
            {
                _geoCity = LoadCity();
                _geoSplitNormal = new GeoSplitNormal(_geoCity, true);
                GeoSplitMatch.dt = null;
                _geoSplitMatch = new GeoSplitMatch(_geoCity);
                //---------数据不能满足，以下暂时无用-------------------
                // _geoSplitArea = new GeoSplitArea(_geoCity);
                //_geoSplitPinYin = new GeoSplitPinYin(_geoCity);
                //------------------------------------------------------
                Coding.dt = null;
                _coding = new Coding(_geoCity);

                ConnectionProperty cp = new ConnectionProperty();
                cp.ConnectionString = _geoCity.ConnStr;
                cp.DatabaseType = _geoCity.DBType;
                _iDataAccess = DAFactory.CreateDataAccess(cp);
            }
            catch (Exception ex)
            {
                writelog("GeoCoding初始化时发生错误" + ex.Message, "CodingDBInit");
            }
        }

        private GeoCity LoadCity()
        {
            try
            {
                string appPath = Application.StartupPath;
                string cinifile = appPath.Remove(appPath.LastIndexOf(@"\")) + "\\config.ini";
                CLC.INIClass.IniPathSet(cinifile);
                GeoCity geoCity = new GeoCity();
                geoCity.TBMatch = "GeoMap";

                geoCity.CityNameCN = "顺德";
                geoCity.CityNameEN = "SunDe";
                geoCity.DBServer = "";
                geoCity.DBName = CLC.INIClass.IniReadValue("数据库", "数据源");
                geoCity.DBUser = CLC.INIClass.IniReadValue("数据库", "用户名");
                geoCity.DBPassword = CLC.INIClass.IniReadValuePW("数据库", "密码");
                geoCity.DBType = DatabaseType.Oracle;

                return geoCity;
            }
            catch (Exception ex)
            {
                writelog(ex.ToString(), "LoadCity");
                return null;
            }
        }

        /// <summary>
        ///插入定位图层
        /// </summary>
        private void InsertLayer()
        {
            try
            {
                if (mapControl1.Map.Layers["CodingPoint"] != null)
                {
                    mapControl1.Map.Layers.Remove("CodingPoint");
                }

                if (mapControl1.Map.Layers["geoCodeLabel"] != null)
                {
                    mapControl1.Map.Layers.Remove("geoCodeLabel");
                }

                TableInfoMemTable ti = new TableInfoMemTable("CodingPoint");
                ti.Columns.Add(ColumnFactory.CreateIndexedIntColumn("ID"));
                ti.Columns.Add(ColumnFactory.CreateStringColumn("address", 30));

                ti.Columns.Add(ColumnFactory.CreateStyleColumn());
                CoordSys Robinson = mapControl1.Map.GetDisplayCoordSys();
                ti.Columns.Add(ColumnFactory.CreateFeatureGeometryColumn(Robinson));

                _pointTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                FeatureLayer layer = new FeatureLayer(_pointTable, "CodingPoint");
                mapControl1.Map.Layers.Insert(0, layer);

                FeatureOverrideStyleModifier featureOverrideStyleModifier = new FeatureOverrideStyleModifier("", new CompositeStyle(new SimpleVectorPointStyle(35, Color.Red, 20)));
                layer.Modifiers.Append(featureOverrideStyleModifier);

                LabelLayer labelLayer = new LabelLayer("geoCodeLabel", "geoCodeLabel");
                mapControl1.Map.Layers.Insert(0, labelLayer);

                LabelSource source = new LabelSource(_pointTable);

                source.DefaultLabelProperties.Caption = "address";
                source.DefaultLabelProperties.Style.Font.Shadow = true;

                labelLayer.Sources.Append(source);
            }
            catch (Exception ex)
            {
                writelog("插入定位图层时发生错误:" + ex.Message, "InsertLayer");
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string address = textBoxAddress.Text;

            // 本机定位
            GeoPoint geoPoint = new GeoPoint(address, _cityName);
            CodingDBInit();

            if (GeoCoding(ref geoPoint))
            {
                if (geoPoint.GeoPoints.Count >= 1)
                {
                    //InitData(geoPoint);
                    frmList fList = new frmList(this.mapControl1);
                    fList.GoInitData(geoPoint);
                    fList.Show();
                }
                else
                {
                    NewPositionInfo(geoPoint);
                }
                //this.Dispose();
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                //MessageBox.Show("定位失败");
                MessageBox.Show("定位点不在范围内或不存在此点！");
            }
        }

        /// <summary>
        /// 更新list列表
        /// </summary>
        /// <param name="geoPoint"></param>
        //public void InitData(GeoPoint geoPoint)
        //{
        //    try
        //    {
        //        frmList fList = new frmList();
        //        fList.GoInitData(geoPoint);
        //        DialogResult dResult = fList.ShowDialog();
        //        if (dResult == DialogResult.OK)
        //        {                    
        //            NewPositionInfo(geoPoint.GeoPoints[fList.lbxMulti.SelectedIndex]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        writelog("更新list列表时发生错误" + ex.Message,"InitData");
        //    }
        //}

        private void writelog(string p,string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine(DateTime.Now.ToString() + ": " + "GeoCodeing:在 " + sFunc + "方法中发生错误。");
            sw.WriteLine(p);
            sw.Close();
        }

        /// <summary>
        /// 定位主函数
        /// </summary>
        /// <param name="geoPoint">要定位的点</param>
        /// <returns></returns>
        private bool GeoCoding(ref GeoPoint geoPoint)
        {
            try
            {                
                geoPoint.Road = geoPoint.Address;
                geoPoint.Roads.Add(geoPoint.Address);
                geoPoint = _coding.GoCoding(geoPoint);

                if (geoPoint.X != 0)
                {
                    return true;
                }
                else
                {
                    geoPoint.Roads.Clear();

                    geoPoint = _coding.GoCoding(geoPoint, true);
                    if (geoPoint.CodingSuccess)
                    {
                        return true;
                    }

                    geoPoint = _geoSplitNormal.Geo(geoPoint);
                    geoPoint = _coding.GoCoding(geoPoint);

                    if (geoPoint.CodingSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                writelog("定位主函数发生错误" + ex.Message,"GeoCode");
                return false;
            }
        }

        /// <summary>
        /// 更新函数
        /// </summary>
        private void NewPositionInfo(GeoPoint geoPoint)
        {
            try
            {
                UpdateInfo(geoPoint);

                MapInfo.Geometry.DPoint dpt;

                dpt.x = geoPoint.X;
                dpt.y = geoPoint.Y;

                //设置定位点所在的中队辖区为当前视野范围
                try
                {
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                    MapInfo.Geometry.FeatureGeometry fg = new MapInfo.Geometry.Point(cSys, dpt);
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(fg, IntersectType.Geometry);
                    si.QueryDefinition.Columns = null;
                    Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("民警中队辖区", si);
                    if (ft != null)
                    {
                        mapControl1.Map.SetView(ft);
                    }
                    else
                    {
                        mapControl1.Map.SetView(dpt, cSys, getScale());
                        this.mapControl1.Map.Center = dpt;
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                writelog("更新函数出错" + ex.Message,"NewPositionInfo");
            }
        }

        /// <summary>
        /// 添加点并闪烁
        /// </summary>
        private void UpdateInfo(GeoPoint geoPoint)
        {
            //-------以下是否清除上次定位的点--------
            try
            {
                _pointTable = MapInfo.Engine.Session.Current.Catalog["CodingPoint"];

                //(_pointTable as IFeatureCollection).Clear();   //清除图层已有图元  jie.zhang 2010.5.17
                //_pointTable.Pack(PackType.All);

                Feature newFeature = new Feature(_pointTable.TableInfo.Columns);
                SimpleVectorPointStyle svStyle = new SimpleVectorPointStyle(37, Color.Red, 22);
                MapInfo.Geometry.Point p = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), geoPoint.X, geoPoint.Y);

                newFeature.Geometry = p;
                newFeature["address"] = geoPoint.Address;
                newFeature.Style = svStyle;
                _pointTable.InsertFeature(newFeature);

                mapControl1.Map.Center = p.Data;
                //mapControl1.Map.Zoom = new Distance(3, DistanceUnit.Kilometer);
            }
            catch (Exception ex)
            {
                writelog("更新地图时发生错误" + ex.Message, "UpdateInfo");
            }
        }

        private void btnXYloc_Click(object sender, EventArgs e)
        {
            try
            {
                double x = Convert.ToDouble(textX.Text);
                double y = Convert.ToDouble(textY.Text);
                MapInfo.Geometry.DPoint dp = new MapInfo.Geometry.DPoint(x, y);
                MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                mapControl1.Map.SetView(dp, cSys, getScale());
                mapControl1.Map.Center = dp;
                
                Style ftstyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                FeatureGeometry pt = new MapInfo.Geometry.Point(cSys, x, y);
                Feature pfeat = new Feature(pt, ftstyle);

                FeatureLayer fl = mapControl1.Map.Layers["Location_temp"] as FeatureLayer;
                //FeatureLayer fl = mapControl1.Map.Layers["街镇面"] as FeatureLayer;
                this.editLocTable = fl.Table;
                this.editLocTable.InsertFeature(pfeat);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "btnXYloc_Click()");
            }
        }

        // 获取缩放比例
        private double getScale()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("比例尺", "缩放比例"));
                return dou;
            }
            catch (Exception ex)
            {
                CLC.BugRelated.ExceptionWrite(ex, "winLoc-getScale");
                return 0;
            }
        }

        //创建临时图层
        private MapInfo.Data.Table ctempTable(string tableName)
        {
            Table table;
            if (MapInfo.Engine.Session.Current.Catalog.GetTable(tableName) == null)
            {
                TableInfoMemTable ti = new TableInfoMemTable(tableName);
                ti.Temporary = true;
                Column column;
                column = new GeometryColumn(mapControl1.Map.GetDisplayCoordSys());
                column.Alias = "MI_Geometry";
                column.DataType = MIDbType.FeatureGeometry;
                ti.Columns.Add(column);
                column = new Column();
                column.Alias = "MI_Style";
                column.DataType = MIDbType.Style;
                ti.Columns.Add(column);
                column = new Column();
                column.Alias = "MapID";
                column.DataType = MIDbType.Int;
                ti.Columns.Add(column);
                column = new Column();
                column.Alias = "Name";
                column.DataType = MIDbType.String;
                ti.Columns.Add(column);
                try
                {
                    table = MapInfo.Engine.Session.Current.Catalog.CreateTable(ti);
                }
                catch
                {
                    table = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);
                }
            }
            else
            {
                table = MapInfo.Engine.Session.Current.Catalog.GetTable(tableName);
            }
            return table; 
        }

        private void frmLocDia_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MapInfo.Engine.Session.Current.Catalog.GetTable("民警中队辖区") != null)
            {
                MapInfo.Engine.Session.Current.Catalog.CloseTable("民警中队辖区");
            }
            //if (this.editLocTable != null)
            //{
            //    MapInfo.Engine.Session.Current.Catalog.CloseTable("Location_temp"); 
            //}
        }

        //以下代码用于将民警中队辖区添加到线程中，以便定位后将地址放大到所属中队范围  add by fisher in 09-12-29
        MapInfo.Data.Table zTable = null;
        private void GetTable(string tableName)
        {
            MIConnection miConnection = new MIConnection();
            if (MapInfo.Engine.Session.Current.Catalog.GetTable("民警中队辖区") == null)
            {
                miConnection.Open();

                //之前有空间表打不开, 所以用了多次打开.
                try
                {
                    TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select  *  From " + tableName, ServerToolkit.Oci);
                    ti.CacheSettings.CacheType = CacheOption.Off;
                    this.zTable = miConnection.Catalog.OpenTable(ti);
                    miConnection.Close();
                }
                catch
                { }
            }
        }
    }
}