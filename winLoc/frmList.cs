using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Crownwood.DotNetMagic.Forms;
using GeoCoding;
using System.IO;

using MapInfo.Windows.Controls;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Geometry;
using MapInfo.Mapping;

namespace winLoc
{
    public partial class frmList : DotNetMagicForm
    {
        private MapControl mapControl1 = null;
        private Table _pointTable;
        public GeoPoint _geoPoint;
        public frmList(MapControl mapControl)
        {
            InitializeComponent();
            this.mapControl1 = mapControl;
        }

        public void GoInitData(GeoPoint geoPoint)
        {
            _geoPoint = geoPoint;
            this.lbxMulti.Items.Clear();
            for (int i = 0; i < geoPoint.GeoPoints.Count; i++)
            {
                //if (geoPoint.GeoPoints[i].Address != null && geoPoint.GeoPoints[i].Address.Trim().Length >0)
                lbxMulti.Items.Add(geoPoint.GeoPoints[i].Address);
            }
            this.lbxMulti.SelectedIndex =0;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.OK;
                //this.Hide();
                NewPositionInfo(_geoPoint.GeoPoints[lbxMulti.SelectedIndex]);
                //this.WindowState = FormWindowState.Minimized;
            }
            catch (Exception ex)
            {
                writelog(ex.Message, "buttonOK_Click");
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
                writelog("更新函数出错" + ex.Message, "NewPositionInfo");
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

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="p">错误信息</param>
        /// <param name="sFunc">方法名</param>
        private void writelog(string p, string sFunc)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\ProgramLog.log", true);
            sw.WriteLine(DateTime.Now.ToString() + ": " + "GeoCodeing:在 " + sFunc + "方法中发生错误。");
            sw.WriteLine(p);
            sw.Close();
        }
    }
}