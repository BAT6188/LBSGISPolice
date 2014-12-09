using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.OracleClient;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using MapInfo.Tools;
using MapInfo.Geometry;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Styles;
using MapInfo.Engine;
using MapInfo.Mapping.Thematics;
using MapInfo.Windows.Dialogs;
using MapInfo.Windows.Controls;
using LBSDataGuide;

namespace clGISPoliceEdit
{
    public partial class ucGISPoliceEdit : UserControl
    {
        private string mysqlstr = "";               // 数据库边接字符串
        string exePath;                             // 程序存放路径
        private bool isOracleSpatialTab = false;    // 实为是否是OracleSpatial表
        private string datasource, userid, password;// 数据库连接的数据源、用户名、密码
        private string[] StrCon;                    // 数据库连接的数据源、用户名、密码
        private AreaStyle aStyle;                   // 设置面绘制要素的默认样式
        private Style featStyle;                    // 设置点绘制要素的默认样式
        private string fileName = "";               // 照片路径
        private string getFromNamePath = "";        // 存取访问GetFromNameConfig.ini文件的路径　lili
        public string userName = "";                // 存取当前用户的用户名  lili
        public string ZoomFile = "";                // 存取地图缩放比例尺值 lili
        private MapControl mapControl1;             // 地图控件
        private ToolStrip toolStrip1;               // tool按钮

        private DataTable _exportDT = null;         // 将查询出的数据导出
        private DataTable Ga_exportDT = null;       // 将公安编辑查询出的数据导出
        private DataTable temEditDt = null;         // 存储编辑模块权限　lili
        private Table zhongduiST = null;            // 用来在添加对象时判断添加区域是否正确

        private string[] listPaichusuo = null;      // 派出所名称数组
        string strRegion = "";                      // 用来存储权限所在的派出所
        string strRegion1 = "";                     // 用来存储权限所在的中队

        public ToolStripProgressBar toolEditPro;    // 用于查询的进度条　lili 2010-8-10
        public ToolStripLabel toolEditProLbl;       // 用于显示进度文本　
        public ToolStripSeparator toolEditProSep;   // 分隔符

        public System.Windows.Forms.Panel panPc;    // 用于鼠标伸到地图显示中队警务室的面板
        public System.Windows.Forms.Label label5;   // 用于显示中队警务室名称

        /// <summary>
        /// 构造函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="mapCon">地图控件</param>
        /// <param name="region1">派出所权限</param>
        /// <param name="region2">中队权限</param>
        /// <param name="dt">编辑模块权限</param>
        /// <param name="toolStr">tool按钮</param>
        public ucGISPoliceEdit(MapControl mapCon, string region1, string region2, DataTable dt,ToolStrip toolStr)
        {
            try
            {
                InitializeComponent();

                this.mapControl1 = mapCon;
                this.toolStrip1 = toolStr;
                strRegion = region1;
                strRegion1 = region2;
                temEditDt = dt;

                //连接数据库
                exePath = Application.StartupPath;
                getFromNamePath = exePath + "\\GetFromNameConfig.ini";
                CLC.INIClass.IniPathSet(exePath.Remove(exePath.LastIndexOf("\\")) + "\\config.ini");
                datasource = CLC.INIClass.IniReadValue("数据库", "数据源");
                userid = CLC.INIClass.IniReadValue("数据库", "用户名");
                password = CLC.INIClass.IniReadValuePW("数据库", "密码");
                mysqlstr = "data source=" + datasource + ";user id=" + userid + ";password=" + password;
                StrCon = new string[] { datasource, userid, password };
                listPaichusuo = getPaichusuo();
                comboTables.Text = comboTables.Items[0].ToString();
                setPrivilege();//设置权限

                //添加基层民警中队图层，用以判断添加点的所属区域  （fisher in 09-11-30）
                addMJZD();

                this.mapControl1.Tools.FeatureChanging += new FeatureChangingEventHandler(Tools_FeatureChanging);      // 地图点改变时调用的函数
                this.mapControl1.Tools.FeatureChanged += new MapInfo.Tools.FeatureChangedEventHandler(Feature_Changed);// 地图编辑时调用的函数
                this.mapControl1.Tools.FeatureSelected += new FeatureSelectedEventHandler(Feature_Selected);           // 地图选择时调用的函数

                this.mapControl1.Tools.FeatureAdded += new FeatureAddedEventHandler(Tools_FeatureAdded);               // 地图添加时调用的函数
                this.mapControl1.Tools.Used += new ToolUsedEventHandler(Tools_Used);                                   
                this.mapControl1.Tools.NodeChanged += new NodeChangedEventHandler(Tools_NodeChanged);
                this.mapControl1.MouseClick += new MouseEventHandler(mapControl1_MouseClick);
                this.mapControl1.MouseMove += new MouseEventHandler(mapControl1_MouseMove);
                this.mapControl1.KeyDown += new KeyEventHandler(mapControl1_KeyDown);
                this.dataGridView1.DataError += new DataGridViewDataErrorEventHandler(dataGridView1_DataError);

                //添加自定义定位工具
                MapInfo.Tools.MapTool ptMapTool = new MapInfo.Tools.CustomPointMapTool(false, this.mapControl1.Tools.FeatureViewer, 
                                                                                              this.mapControl1.Handle.ToInt32(), 
                                                                                              this.mapControl1.Tools, 
                                                                                              this.mapControl1.Tools.MouseToolProperties, 
                                                                                              this.mapControl1.Tools.MapToolProperties);
                ptMapTool.UseDefaultCursor = false;
                ptMapTool.Cursor = Cursors.Cross;
                this.mapControl1.Tools.Add("Location", ptMapTool);

                //初始化默认表的编辑字段
                this.mapToolBar1.Buttons["mapToolAddpolygon"].Visible = false;

                //初始化点样式，线样式
                SimpleLineStyle simLineStyle = new SimpleLineStyle(new LineWidth(2, MapInfo.Styles.LineWidthUnit.Point), 2, System.Drawing.Color.Red);
                SimpleInterior simInterior = new SimpleInterior(1);
                this.aStyle = new AreaStyle(simLineStyle, simInterior);
                this.featStyle = setFeatStyle(comboTables.Text);

                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                this.mapToolBar1.MapControl = mapCon;  // 将toolBar按钮指明操作地图
                this.mapToolBar2.MapControl = mapCon;  // 将toolBar按钮指明操作地图
                this.toolStrip1.ItemClicked += new ToolStripItemClickedEventHandler(toolStrip1_ItemClicked);

                //string info = "权限范围为: ";
                //if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1" && temEditDt.Rows[0]["业务数据可编辑"].ToString() == "1" && temEditDt.Rows[0]["视频可编辑"].ToString() == "1")
                //{
                //    info += "全部编辑权限";
                //}
                //else
                //{
                //    info += "只有部分数据编辑权限";
                //}
                //toolStatusInfo.Text = info;
                //toolStatusUser.Text = "用户: " + temEditDt.Rows[0]["userNow"].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "构造函数");
            }
        }

        /// <summary>
        /// 添加民警中队辖区视图
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void addMJZD()
        {
            MIConnection miConnection = new MIConnection();
            try
            {
                miConnection.Open();
                if (miConnection.Catalog.GetTable("中队辖区") == null)
                {
                    TableInfoServer ti = new TableInfoServer("中队辖区");
                    string str1 = mysqlstr.Replace("data source", "SRVR").Replace("user id", "UID").Replace("password", "PWD");
                    ti.ConnectString = str1;
                    string strSQL = "Select * From 民警中队辖区";
                    ti.Query = strSQL;
                    ti.Toolkit = ServerToolkit.Oci;
                    ti.CacheSettings.CacheType = CacheOption.Off;
                    zhongduiST = miConnection.Catalog.OpenTable(ti);
                    //添加图层到layers，设置图层不可见
                    int t = mapControl1.Map.Layers.Add(new FeatureLayer(zhongduiST));
                    mapControl1.Map.Layers[t].Enabled = false;
                    //MapInfo.Mapping.LayerHelper.SetSelectable(
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "addMJZD");
                MessageBox.Show("打开民警中队辖区错误:" + ex.Message);
            }
            finally
            {
                if (miConnection.State == ConnectionState.Open)
                    miConnection.Close();
            }
        }

        /// <summary>
        /// 改变图元时确发
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        void Tools_FeatureChanging(object sender, FeatureChangingEventArgs e)
        {
            try
            {
                if (!this.Visible) return; // 不是当前模块返回

                if (e.FeatureChangeMode == FeatureChangeMode.Delete)
                {
                    if (tabControl1.SelectedTab == tabPage2)
                    {  //视频位置不允许删除   edit by fisher in 09-12-10
                        e.Cancel = true;
                        return;
                    }
                    if (MessageBox.Show("确认删除?", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_FeatureChanging");
            }
        }

        /// <summary>
        /// 对面进行节点编辑时
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Tools_NodeChanged(object sender, NodeChangedEventArgs e)
        {
            try
            {
                if (!this.Visible) return; // 不是当前模块返回

                //找到移动后的要素
                SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + selPrinx);
                si.QueryDefinition.Columns = null;
                Feature f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);

                //找到oracleSpatial表中对应的要素
                si = MapInfo.Data.SearchInfoFactory.SearchWhere("MI_PRINX=" + selPrinx);
                si.QueryDefinition.Columns = null;
                Feature newFeat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(lTable.Alias, si);

                //新建对象,为原要素的属性+移动后的geometry
                Feature addFeat = newFeat;
                addFeat.Geometry = f.Geometry;

                //先删除原来的要素,再添加新位置的要素
                lTable.DeleteFeature(newFeat);
                lTable.InsertFeature(addFeat);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_NodeChanged");
            }
        }

        /// <summary>
        /// 基础数据设置权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void setPrivilege() 
        {
            try
            {
                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() != "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text], false);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setPrivilege");
            }
        }

        private bool isUse = false;
        /// <summary>
        /// 地图点击
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        void Tools_Used(object sender, ToolUsedEventArgs e)
        {
            if (!this.Visible) return; // 不是当前模块返回

            if (e.ToolStatus == ToolStatus.Start)
            {
                isUse = true;
            }
            if (e.ToolStatus == ToolStatus.End)
            {
                isUse = false;
            }
            try
            {
                if (e.ToolName == "Location")
                {
                    //added by fisher(09-09-03)
                    if (this.tabControl1.SelectedTab == tabPage1)  //基础业务数据
                    {
                        if (dataGridViewList.CurrentCell == null)
                        {
                            MessageBox.Show("请选择需要定位的点!", "提示");
                            btnLoc3.Enabled = false;
                            return;
                        }

                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                this.tabControl2.SelectedTab = tabPageInfo;
                                Feature f;   //当前即将被添加的对象
                                Feature Jc_pref = null;   //即将被删除的对象
                                if (this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value.ToString() != "" && this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value.ToString() != "")
                                {
                                    if (Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value) != 0)
                                    {
                                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value));
                                        si.QueryDefinition.Columns = null;
                                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                                        if (f != null)
                                        {
                                            Jc_pref = f;
                                            this.editTable.DeleteFeature(f);
                                        }
                                        //先删除
                                    }
                                }

                                //添加点
                                CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                                string sName = CLC.ForSDGA.GetFromTable.ObjName;
                                string sobjID = CLC.ForSDGA.GetFromTable.ObjID;   //默认的主键

                                f = new Feature(this.editTable.TableInfo.Columns);
                                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(e.MapCoordinate.x, e.MapCoordinate.y));

                                if (dataGridViewList.Rows[browIndex].Cells["mapid"].Value == null || dataGridViewList.Rows[browIndex].Cells["mapid"].Value.ToString() == "")
                                {
                                    OracleConnection conn = new OracleConnection(mysqlstr);
                                    try
                                    {
                                        conn.Open();
                                        //以下代码由fisher于09-09-22日更新，在重新定位一个没有XY坐标信息的记录时，如果该记录的mapid为空，
                                        //则首先更新其mapid，以免后续的更新出现问题
                                        if (isOracleSpatialTab == false)
                                        {
                                            OracleCommand cmd = new OracleCommand("select max(mapid) from " + comboTables.Text.ToString(), conn);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            int objId3 = 0;
                                            if (dr.HasRows)
                                            {
                                                dr.Read();
                                                objId3 = Convert.ToInt32(dr.GetValue(0)) + 1;
                                                selMapID = Convert.ToString(objId3);   //给全局变量赋值，便于数据的更新
                                            }
                                            f["mapid"] = objId3.ToString();
                                            dataGridViewList.Rows[browIndex].Cells["mapid"].Value = objId3;
                                            dr.Close();
                                            cmd.Dispose();
                                            cmd = new OracleCommand("update " + comboTables.Text + " t set t.mapid = " + objId3 + " where " + sobjID + " = " + dataGridViewList.Rows[browIndex].Cells[sobjID].Value.ToString(), conn);
                                            cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        writeToLog(ex, "Tools_Used-添加点");
                                    }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open)
                                            conn.Close();
                                    }
                                }
                                else
                                {
                                    f["mapid"] = this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value.ToString();
                                }
                                f["name"] = this.dataGridViewList.Rows[browIndex].Cells[sName].Value.ToString();
                                f.Style = featStyle;

                                //查找到新添加点的范围，判断权限   fisher in 09-12-31
                                SearchInfo Jc_si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                                Jc_si.QueryDefinition.Columns = null;
                                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", Jc_si);

                                //为空时,表明画到了区域外,不添加
                                if (ft == null)
                                {
                                    this.editTable.InsertFeature(Jc_pref);
                                    this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = Jc_pref.Geometry.Centroid.x;
                                    this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = Jc_pref.Geometry.Centroid.y;
                                    MessageBox.Show("不能将对象移动到地图外!", "友情提示");
                                }
                                else
                                {
                                    bool quyuCZ = false;  //用于判断添加的点是不是在用于允许的权限范围内
                                    if (strRegion != "顺德区")
                                    {
                                        //跟街镇面相交,看看所在街镇面是不是用户范围
                                        if (Array.IndexOf(strRegion.Split(','), ft["name"].ToString()) > -1)
                                        {
                                            quyuCZ = true;
                                        }

                                        if (quyuCZ == false)   //不能在权限允许的范围外添加数据
                                        {
                                            this.editTable.InsertFeature(Jc_pref);
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = Jc_pref.Geometry.Centroid.x;
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = Jc_pref.Geometry.Centroid.y;
                                            MessageBox.Show("不能将对象移动到您权限范围以外的区域!", "友情提示");
                                        }
                                        else
                                        {
                                            this.editTable.InsertFeature(f);
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                            Jc_pref = f;
                                        }
                                    }
                                    else  //顺德区权限
                                    {
                                        this.editTable.InsertFeature(f);
                                        this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                        this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                        Jc_pref = f;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else if (this.tabControl1.SelectedTab == tabPage2)  //视频定位
                    {
                        if (dataGridViewVideo.CurrentCell == null)
                        {
                            MessageBox.Show("请选择定位的视频!", "提示");
                            return;
                        }
                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                Feature f;   //当前即将被添加的对象
                                Feature f_pre = null;   //前一个对象,即即将被删除的对象 fisher 09-12-31
                                int iRow = dataGridViewVideo.CurrentCell.RowIndex;
                                //updated by fisher in 09-09-21
                                if (dataGridViewVideo.Rows[iRow].Cells["x"].Value.ToString() != "" && dataGridViewVideo.Rows[iRow].Cells["y"].Value.ToString() != "")
                                {
                                    if (Convert.ToDouble(dataGridViewVideo.Rows[iRow].Cells["x"].Value) != 0 && Convert.ToDouble(dataGridViewVideo.Rows[iRow].Cells["y"].Value) != 0)
                                    {
                                        //先删除
                                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(dataGridViewVideo.Rows[iRow].Cells["mapid"].Value));
                                        si.QueryDefinition.Columns = null;
                                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                                        if (f != null)
                                        {
                                            f_pre = f;
                                            this.editTable.DeleteFeature(f);
                                        }
                                    }
                                }

                                //添加点
                                f = new Feature(this.editTable.TableInfo.Columns);
                                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(e.MapCoordinate.x, e.MapCoordinate.y));
                                f["name"] = dataGridViewVideo.Rows[iRow].Cells["设备名称"].Value.ToString();

                                if (dataGridViewVideo.Rows[iRow].Cells["mapid"].Value == null || dataGridViewVideo.Rows[iRow].Cells["mapid"].Value.ToString() == "")
                                {
                                    OracleConnection conn = new OracleConnection(mysqlstr);
                                    try
                                    {
                                        conn.Open();
                                        OracleCommand cmd = new OracleCommand("select max(mapid) from 视频位置", conn);
                                        OracleDataReader dr = cmd.ExecuteReader();
                                        int videoId = 0;
                                        if (dr.HasRows)
                                        {
                                            dr.Read();
                                            videoId = Convert.ToInt32(dr.GetValue(0)) + 1;
                                        }
                                        f["mapid"] = videoId.ToString();
                                        dataGridViewVideo.Rows[iRow].Cells["mapid"].Value = videoId;
                                        dr.Close();
                                        cmd.Dispose();
                                    }
                                    catch (Exception ex)
                                    { writeToLog(ex, "Tools_Used-视频添加点"); }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open)
                                            conn.Close();
                                    }
                                }
                                else
                                {
                                    f["mapid"] = dataGridViewVideo.Rows[iRow].Cells["mapid"].Value.ToString();
                                }
                                f.Style = new MapInfo.Styles.BitmapPointStyle("sxt.bmp");

                                //查找到新添加点的范围，判断权限   fisher in 09-12-31
                                SearchInfo newsi = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                                newsi.QueryDefinition.Columns = null;
                                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", newsi);

                                //为空时,表明画到了区域外,不添加
                                if (ft == null)
                                {
                                    this.editTable.InsertFeature(f_pre);
                                    dataGridViewVideo.Rows[iRow].Cells["x"].Value = f_pre.Geometry.Centroid.x;
                                    dataGridViewVideo.Rows[iRow].Cells["y"].Value = f_pre.Geometry.Centroid.y;
                                    MessageBox.Show("不能将对象移动到地图外!", "友情提示");
                                }
                                else
                                {
                                    bool quyuCZ = false;  //用于判断添加的点是不是在用于允许的权限范围内
                                    if (strRegion != "顺德区")
                                    {
                                        //跟街镇面相交,看看所在街镇面是不是用户范围
                                        if (Array.IndexOf(strRegion.Split(','), ft["name"].ToString()) > -1)
                                        {
                                            quyuCZ = true;
                                        }

                                        if (quyuCZ == false)   //不能在权限允许的范围外添加数据
                                        {
                                            this.editTable.InsertFeature(f_pre);
                                            dataGridViewVideo.Rows[iRow].Cells["x"].Value = f_pre.Geometry.Centroid.x;
                                            dataGridViewVideo.Rows[iRow].Cells["y"].Value = f_pre.Geometry.Centroid.y;
                                            MessageBox.Show("不能将对象移动到您权限范围以外的区域!", "友情提示");
                                        }
                                        else
                                        {
                                            this.editTable.InsertFeature(f);
                                            dataGridViewVideo.Rows[iRow].Cells["x"].Value = e.MapCoordinate.x;
                                            dataGridViewVideo.Rows[iRow].Cells["y"].Value = e.MapCoordinate.y;
                                            f_pre = f;
                                        }
                                    }
                                    else  //顺德区权限
                                    {
                                        this.editTable.InsertFeature(f);
                                        dataGridViewVideo.Rows[iRow].Cells["x"].Value = e.MapCoordinate.x;
                                        dataGridViewVideo.Rows[iRow].Cells["y"].Value = e.MapCoordinate.y;
                                        f_pre = f;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    //added by fisher (09-09-03)
                    else if (this.tabControl1.SelectedTab == tabPage3)   //公安业务数据
                    {
                        if (dataGridViewGaList.CurrentCell == null)
                        {
                            MessageBox.Show("请选择需要定位的点!", "提示");
                            return;
                        }

                        switch (e.ToolStatus)
                        {
                            case MapInfo.Tools.ToolStatus.Start:
                                this.tabControl3.SelectedTab = tabGaInfo;

                                // by fisher on 09-09-23
                                //如果是MapInfo的表，在表中没有XY信息时，先删除该条记录，然后添加，并将相应的XY赋值为0

                                if (this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() == "" && this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() == "")
                                {
                                    if (isOracleSpatialTab)
                                    {

                                    }
                                }

                                Feature f;     //当前即将被添加的对象
                                Feature J_pref = null;  //当前即将被删除的对象      add by fisher in 09-12-31

                                if (this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() != "" && this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() != "")
                                {
                                    if (Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value) != 0)
                                    {
                                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value));
                                        si.QueryDefinition.Columns = null;
                                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                                        if (f != null)
                                        {
                                            J_pref = f;
                                            this.editTable.DeleteFeature(f);
                                        }
                                    }
                                }

                                //添加点
                                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                                string sName = CLC.ForSDGA.GetFromTable.ObjName;
                                string sobjID = CLC.ForSDGA.GetFromTable.ObjID;

                                f = new Feature(this.editTable.TableInfo.Columns);
                                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(e.MapCoordinate.x, e.MapCoordinate.y));

                                if (dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value == null || dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value.ToString() == "")
                                {
                                    OracleConnection conn = new OracleConnection(mysqlstr);
                                    try
                                    {
                                        conn.Open();
                                        //以下代码由fisher于09-09-22日更新，在重新定位一个没有XY坐标信息的记录时，如果该记录的mapid为空，
                                        //则首先更新其mapid，以免后续的更新出现问题
                                        if (isOracleSpatialTab)
                                        {
                                            OracleCommand cmd = new OracleCommand("select max(MI_PRINX) from " + comboTable.Text.ToString(), conn);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            int objId1 = 0;
                                            if (dr.HasRows)
                                            {
                                                dr.Read();
                                                objId1 = Convert.ToInt32(dr.GetValue(0)) + 1;
                                                selPrinx = objId1;   //给全局变量赋值，便于数据的更新
                                            }
                                            f["mapid"] = objId1.ToString();
                                            dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value = objId1;
                                            dr.Close();
                                            cmd.Dispose();
                                            cmd = new OracleCommand("update " + comboTable.Text + " t set t.MI_PRINX = " + objId1 + " where " + sobjID + " = " + dataGridViewGaList.Rows[rowIndex].Cells[sobjID].Value.ToString(), conn);
                                            cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                        }
                                        else
                                        {
                                            OracleCommand cmd = new OracleCommand("select max(mapid) from " + comboTable.Text.ToString(), conn);
                                            OracleDataReader dr = cmd.ExecuteReader();
                                            int objId2 = 0;
                                            if (dr.HasRows)
                                            {
                                                dr.Read();
                                                objId2 = Convert.ToInt32(dr.GetValue(0)) + 1;
                                                selMapID = Convert.ToString(objId2);   //给全局变量赋值，便于数据的更新
                                            }
                                            f["mapid"] = objId2.ToString();
                                            dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value = objId2;
                                            dr.Close();
                                            cmd.Dispose();
                                            cmd = new OracleCommand("update " + comboTable.Text + " t set t.mapid = " + objId2 + " where " + sobjID + " = " + dataGridViewGaList.Rows[rowIndex].Cells[sobjID].Value.ToString(), conn);
                                            cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    { writeToLog(ex, "Tools_Used-业务数据添加点"); }
                                    finally
                                    {
                                        if (conn.State == ConnectionState.Open)
                                            conn.Close();
                                    }
                                }
                                else
                                {
                                    f["mapid"] = dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value.ToString();
                                    selPrinx = Convert.ToInt32(f["mapid"]);
                                }

                                f["name"] = this.dataGridViewGaList.Rows[rowIndex].Cells[sName].Value.ToString();
                                f.Style = featStyle;

                                //查找到新添加点的范围，判断权限   fisher in 09-12-31
                                SearchInfo J_si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                                J_si.QueryDefinition.Columns = null;
                                Feature ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", J_si);

                                //为空时,表明画到了区域外,不添加
                                if (ft == null)
                                {
                                    this.editTable.InsertFeature(J_pref);
                                    this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = J_pref.Geometry.Centroid.x;
                                    this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = J_pref.Geometry.Centroid.y;
                                    MessageBox.Show("不能将对象移动到地图外!", "友情提示");
                                }
                                else
                                {
                                    bool quyuCZ = false;  //用于判断添加的点是不是在用于允许的权限范围内
                                    if (strRegion != "顺德区")
                                    {
                                        //跟街镇面相交,看看所在街镇面是不是用户范围
                                        if (Array.IndexOf(strRegion.Split(','), ft["name"].ToString()) > -1)
                                        {
                                            quyuCZ = true;
                                        }

                                        if (quyuCZ == false)   //不能在权限允许的范围外添加数据
                                        {
                                            this.editTable.InsertFeature(J_pref);
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = J_pref.Geometry.Centroid.x;
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = J_pref.Geometry.Centroid.y;
                                            MessageBox.Show("不能将对象移动到您权限范围以外的区域!", "友情提示");
                                        }
                                        else
                                        {
                                            this.editTable.InsertFeature(f);
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                            J_pref = f;
                                        }
                                    }
                                    else  //顺德区权限
                                    {
                                        this.editTable.InsertFeature(f);
                                        this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = e.MapCoordinate.x;
                                        this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = e.MapCoordinate.y;
                                        J_pref = f;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_Used");
            }
        }

        private double dx = 0, dy = 0;
        private Feature ft = null;       // 刚添加的地图对象或者被选中的对象
        private Feature feature = null;  // oracleSpatial表中对应于地图层上的对象

        /// <summary>
        /// 地图上添加对象
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Tools_FeatureAdded(object sender, FeatureAddedEventArgs e)
        {
            try
            {
                if (!this.Visible) return; // 不是当前模块返回

                if (tabControl1.SelectedTab == tabPage3)
                {
                    string tabName = comboTable.Text.Trim();
                    if (isOracleSpatialTab)
                    {
                        tabName += "_tem";
                    }
                    setTabInsertable(tabName, false); //找到要插入的图层

                    double gx = 0, gy = 0;  //added by fisher(09-09-02)
                    int w = 0;     // 存取最后新人的行位置 lili
                    if (e.Feature.Type == GeometryType.Point)
                    {
                        try
                        {
                            //找到刚添加的对象
                            SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(e.Feature, ContainsType.Geometry);
                            si.QueryDefinition.Columns = null;
                            ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tabName, si);

                            //找到添加对象所在乡镇
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft1 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", si);

                            //找到添加对象所在中队辖区  (以下代码由fisher添加  09-12-1)
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("中队辖区", si);
                            string zdmc = "";  //中队名称

                            //以下代码返回新添加点所在的中队名称
                            if (ft2 != null)
                            {
                                string zddm = ft2["中队代码"].ToString(); //中队代码
                                OracleConnection conn = new OracleConnection(mysqlstr);
                                try
                                {
                                    conn.Open();
                                    OracleCommand cmd = new OracleCommand("select 中队名 from 基层民警中队 where 中队代码 = " + zddm, conn);
                                    OracleDataReader dr = cmd.ExecuteReader();
                                    if (dr.HasRows)
                                    {
                                        dr.Read();
                                        zdmc = dr.GetValue(0).ToString();
                                    }
                                    dr.Close();
                                    cmd.Dispose();
                                    conn.Close();
                                }
                                catch (Exception ex)
                                {
                                    writeToLog(ex, "Tools_FeatureAdded");
                                    conn.Close();
                                }
                            }

                            //为空时,表明画到了区域外,不添加
                            if (ft1 == null)
                            {
                                editTable.DeleteFeature(ft);
                                setTabInsertable(tabName, true);
                                return;
                            }
                            else
                            {
                                bool quyuCZ = false;  //用于判断添加的点是不是在用户允许的权限范围内
                                if (strRegion != "" || strRegion1 != "")
                                {
                                    if (strRegion != "顺德区")
                                    {
                                        if (strRegion != "")
                                        {
                                            //跟街镇面相交,看看所在街镇面是不是用户范围
                                            if (Array.IndexOf(strRegion.Split(','), ft1["name"].ToString()) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (strRegion1 != "")
                                        {
                                            if (Array.IndexOf(strRegion1.Split(','), zdmc) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (quyuCZ == false)   //不能在权限允许的范围外添加数据
                                        {
                                            editTable.DeleteFeature(ft);
                                            setTabInsertable(tabName, true);
                                            return;
                                        }
                                    }
                                }
                            }

                            if (isOracleSpatialTab)
                            {
                                ft["MapID"] = -1;
                                gx = e.Feature.Centroid.x;
                                gy = e.Feature.Centroid.y;
                            }
                            else
                            {
                                dx = e.Feature.Centroid.x;
                                dy = e.Feature.Centroid.y;

                                gx = e.Feature.Centroid.x;
                                gy = e.Feature.Centroid.y;
                            }
                            ft.Style = featStyle;
                            ft.Update();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Tools_FeatureAdded");
                            MessageBox.Show(ex.Message, "GaTools_FeatureAdded000");
                        }
                    }
                    try
                    {
                        for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                        {
                            switch (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString())
                            {
                                case "X":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = gx;
                                    break;
                                case "Y":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = gy;
                                    break;
                                case "最后更新人":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = userName;
                                    w = i;
                                    break;
                                case "标注人":
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = userName;
                                    break;
                                default:
                                    dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                                    fileName = "";
                                    break;
                            }
                        }
                        if (comboTable.Text == "安全防护单位")
                        {
                            dataGridViewGaInfo.Rows.Remove(dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1]);
                        }
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "Tools_FeatureAdded");
                        MessageBox.Show(ex.Message, "Tools_FeatureAdded(添加坐标不成功)");
                    }
                    tabControl3.SelectedTab = tabGaInfo;
                    btnGaSave.Text = "保存";
                    btnGaCancel.Text = "取消添加";
                    dataGridViewGaInfo.Visible = true;
                    btnGaSave.Enabled = false;
                    btnGaCancel.Enabled = true;
                    dataGridViewGaInfo.Columns[1].ReadOnly = false;
                    dataGridViewGaInfo.Rows[w].Cells[1].ReadOnly = true;   // 最后更新人不可以修改  lili 2010-9-27
                }
                else
                {
                    string tabName = comboTables.Text.Trim();
                    if (isOracleSpatialTab)
                    {
                        tabName += "_tem";
                    }
                    setTabInsertable(tabName, false);

                    if (e.Feature.Type == GeometryType.Point)
                    {
                        try
                        {
                            //找到刚添加的对象
                            SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(e.Feature, ContainsType.Geometry);
                            si.QueryDefinition.Columns = null;
                            ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tabName, si);

                            //找到添加对象所在乡镇
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft1 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", si);

                            //找到添加对象所在中队辖区  (以下代码由fisher添加  09-12-1)
                            si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(e.Feature, IntersectType.Geometry);
                            si.QueryDefinition.Columns = null;
                            Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("中队辖区", si);
                            string zdmc = "";  //中队名称
                            if (ft2 != null)
                            {
                                string zddm = ft2["中队代码"].ToString();
                                OracleConnection conn = new OracleConnection(mysqlstr);
                                try
                                {
                                    conn.Open();
                                    OracleCommand cmd = new OracleCommand("select 中队名 from 基层民警中队 where 中队代码 = '" + zddm + "'", conn);
                                    OracleDataReader dr = cmd.ExecuteReader();
                                    if (dr.HasRows)
                                    {
                                        dr.Read();
                                        zdmc = dr.GetValue(0).ToString();
                                    }
                                    dr.Close();
                                    cmd.Dispose();
                                    conn.Close();
                                }
                                catch (Exception ex)
                                {
                                    writeToLog(ex, "Tools_FeatureAdded");
                                    conn.Close();
                                }
                            }

                            //为空时,表明画到了区域外,不添加
                            if (ft1 == null)
                            {
                                editTable.DeleteFeature(ft);
                                setTabInsertable(tabName, true);
                                return;
                            }
                            else
                            {
                                bool quyuCZ = false;  //用于判断添加的点是不是在用于允许的权限范围内
                                if (strRegion != "" || strRegion1 != "")
                                {
                                    if (strRegion != "顺德区")
                                    {
                                        if (strRegion != "")
                                        {
                                            //跟街镇面相交,看看所在街镇面是不是用户范围
                                            if (Array.IndexOf(strRegion.Split(','), ft1["name"].ToString()) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (strRegion1 != "")
                                        {
                                            if (Array.IndexOf(strRegion1.Split(','), zdmc) > -1)
                                            {
                                                quyuCZ = true;
                                            }
                                        }
                                        if (quyuCZ == false)
                                        {
                                            editTable.DeleteFeature(ft);
                                            setTabInsertable(tabName, true);
                                            return;
                                        }
                                    }
                                }
                            }

                            if (isOracleSpatialTab)
                            {
                                ft["MapID"] = -1;
                            }
                            else
                            {
                                dx = e.Feature.Centroid.x;
                                dy = e.Feature.Centroid.y;
                            }
                            ft.Style = featStyle;
                            ft.Update();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Tools_FeatureAdded");
                            MessageBox.Show(ex.Message, "Tools_FeatureAdded feng");
                        }
                    }

                    if (e.Feature.Type == GeometryType.MultiPolygon)
                    {
                        try
                        {
                            SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWithinGeometry(e.Feature, ContainsType.Geometry);
                            si.QueryDefinition.Columns = null;
                            ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tabName, si);

                            ft["MapID"] = -1;
                            ft.Style = aStyle;
                            ft.Update();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Tools_FeatureAdded");
                        }
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        //updated by fisher(09-09-02)
                        if (comboTables.Text == "治安卡口系统" || comboTables.Text == "基层派出所" || comboTables.Text == "基层民警中队" || comboTables.Text == "社区警务室")
                        {
                            if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                            {
                                dataGridView1.Rows[i].Cells[1].Value = dx;
                            }
                            else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                            {
                                dataGridView1.Rows[i].Cells[1].Value = dy;
                            }
                            else
                            {
                                dataGridView1.Rows[i].Cells[1].Value = "";
                                fileName = "";
                            }
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[1].Value = "";
                            fileName = "";
                        }
                    }

                    tabControl2.SelectedTab = tabPageInfo;
                    buttonSave.Text = "保存";
                    buttonCancel.Text = "取消添加";
                    dataGridView1.Visible = true;
                    buttonSave.Enabled = false;
                    buttonCancel.Enabled = true;
                    dataGridView1.Columns[1].ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Tools_FeatureAdded");
            }
        }

        /// <summary>
        /// 创建标注,在创建之前要删除以前的标注图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="table">数据源</param>
        private void labeLayer(Table table)
        {
            try
            {
                LabelLayer labelLayer = null;
                for (int i = 0; i < mapControl1.Map.Layers.Count; i++)
                {
                    if (mapControl1.Map.Layers[i] is LabelLayer)
                    {
                        labelLayer = mapControl1.Map.Layers[i] as LabelLayer;
                        break;
                    }
                }
                LabelSource source = new LabelSource(table);

                source.DefaultLabelProperties.Caption = "Name";
                if (table.Alias == "信息点")
                {
                    source.DefaultLabelProperties.Caption = "名称";
                    source.VisibleRange = new VisibleRange(0, 5, DistanceUnit.Kilometer);
                }
                source.DefaultLabelProperties.Layout.Offset = 4;

                source.DefaultLabelProperties.Style.Font.TextEffect = TextEffect.Halo;

                labelLayer.Sources.Append(source);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "labeLayer");
            }
        }

        /// <summary>
        /// 初始化基础数据编辑的编辑列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="tabName">表名</param>
        private void InitialEditFields(string tabName)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();
                OracleCommand cmd = new OracleCommand("SELECT COLUMN_NAME, NULLABLE,DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME= '" + tabName + "'", Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                cmd.Dispose();
                Conn.Close();

                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                string BobjID = CLC.ForSDGA.GetFromTable.ObjID;    //  by fisher on 09-09-03
                dataGridView1.Rows.Clear();
                int k = 0;
                int j = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string fieldName = dt.Rows[i][0].ToString();
                    if (fieldName.ToUpper() == "MAPID" || fieldName.ToUpper() == "X" 
                                                       || fieldName.ToUpper() == "Y"
                                                       || fieldName.IndexOf("备用字段") > -1 
                                                       || fieldName.ToUpper() == "MI_STYLE" 
                                                       || fieldName.ToUpper() == "MI_PRINX" 
                                                       || fieldName.ToUpper() == "GEOLOC")
                    {
                        k++;
                        continue;
                    }
                    dataGridView1.Rows.Add(1);//在dataGridView1中添加一行

                    dataGridView1.Rows[i - k].Cells[0].Value = fieldName;
                    if (fieldName == CLC.ForSDGA.GetFromTable.XiaQuField && tabName != "基层派出所")
                    {
                        dataGridView1.Rows[i - k].Cells[1] = dgvComboBoxJieZhen();
                    }
                    else if (fieldName == "所属中队")
                    {
                        dataGridView1.Rows[i - k].Cells[1] = dgvComboBoxZhongdu();
                    }
                    else if (fieldName == "所属警务室")
                    {
                        dataGridView1.Rows[i - k].Cells[1] = dgvComboBoxJinWuShi();
                    }
                    else if (fieldName == "重点人口")
                    {
                        dataGridView1.Rows[i - k].Cells[1] = dgvComboBoxShiFou();
                    }
                    else
                    {
                        dataGridView1.Rows[i - k].Cells[1].Value = "";
                    }

                    if (dt.Rows[i][1].ToString().ToUpper() == "N" || fieldName == BobjID)
                    {
                        dataGridView1.Rows[i - k].Cells[2].Value = "必填";
                    }

                    dataGridView1.Rows[i - k].Cells[3].Value = dt.Rows[i][2].ToString().ToUpper();
                    j++;
                }

                if (comboTables.Text == "人口系统")
                {
                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[j].Cells[0].Value = "照片";
                    dataGridView1.Rows[j].Cells[1].Value = "";
                    dataGridView1.Rows[j].Cells[2].Value = "";
                    dataGridView1.Rows[j].Cells[3].Value = "VARCHAR2";
                }
                // added by fisher (09-09-02)
                if (comboTables.Text == "治安卡口系统" || comboTables.Text == "基层派出所" || comboTables.Text == "基层民警中队" || comboTables.Text == "社区警务室")
                {
                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[j].Cells[0].Value = "X";
                    dataGridView1.Rows[j].Cells[1].Value = "";
                    dataGridView1.Rows[j].Cells[2].Value = "";
                    dataGridView1.Rows[j].Cells[3].Value = "FLOAT";

                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[j + 1].Cells[0].Value = "Y";
                    dataGridView1.Rows[j + 1].Cells[1].Value = "";
                    dataGridView1.Rows[j + 1].Cells[2].Value = "";
                    dataGridView1.Rows[j + 1].Cells[3].Value = "FLOAT";
                }
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                writeToLog(ex, "InitialEditFields");
            }

            setDataGridViewColumnWidth(dataGridView1);
        }

        /// <summary>
        /// 获得当前用户所属的派出所权限
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>派出所权限数组</returns>
        private string[] getPaichusuo()    //edit by fisher in 09-12-01
        {
            try
            {
                string a = "";
                string pcsArr = strRegion;
                string pcsArr1 = strRegion1;
                if (pcsArr != "顺德区" && pcsArr != "")
                {
                    if (Array.IndexOf(pcsArr.Split(','), "大良") > -1)
                    {
                        pcsArr = pcsArr.Replace("大良", "大良,德胜");
                    }
                    if (pcsArr1 != "")
                    {
                        pcsArr += ",其他派出所";
                    }
                    return pcsArr.Split(',');
                }
                else if (pcsArr == "" && pcsArr1 != "")
                {
                    pcsArr = "其他派出所";
                    return pcsArr.Split(',');
                }
                else
                {
                    OracleConnection Conn = new OracleConnection(mysqlstr);
                    try
                    {
                        Conn.Open();
                        OracleCommand cmd = Conn.CreateCommand();

                        cmd.CommandText = "select 派出所名 from 基层派出所";

                        OracleDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            a += dr.GetValue(0).ToString().Trim() + ",";
                        }
                        dr.Close();
                        cmd.Dispose();
                        Conn.Close();
                        a = a.Remove(a.LastIndexOf(','));
                        return a.Split(',');
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "getPaichusuo");
                        if (Conn.State == ConnectionState.Open)
                        {
                            Conn.Close();
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getPaichusuo");
                return null;
            }
            //add by fisher in 09-12-01
        }

        /// <summary>
        /// 将派出所权限放入下拉列表中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>返回嵌入数据控件中的下拉列表</returns>
        private DataGridViewComboBoxCell dgvComboBoxJieZhen()
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                for (int i = 0; i < listPaichusuo.Length; i++)
                {
                    dgvComBox.Items.Add(listPaichusuo[i]);
                }
                dgvComBox.Value = dgvComBox.Items[0].ToString();
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxJieZhen");
                return null;
            }
        }

        /// <summary>
        /// 将警务室权限放入下拉列表中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>返回嵌入数据控件中的下拉列表</returns>
        private DataGridViewComboBoxCell dgvComboBoxJinWuShi()
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                string sqlstr = "select 警务室名 from 社区警务室";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                OracleDataReader orader = CLC.DatabaseRelated.OracleDriver.OracleComReader(sqlstr);
                while (orader.Read())
                {
                    dgvComBox.Items.Add(orader[0]);
                }
                dgvComBox.Value = dgvComBox.Items[0].ToString();
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxJinWuShi");
                return null;
            }
        }

        /// <summary>
        /// 将中队权限放入下拉列表中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>返回嵌入数据控件中的下拉列表</returns>
        private DataGridViewComboBoxCell dgvComboBoxZhongdu()
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                string sqlstr = "select 中队名 from 基层民警中队";
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                OracleDataReader orader = CLC.DatabaseRelated.OracleDriver.OracleComReader(sqlstr);

                while (orader.Read())
                {
                    dgvComBox.Items.Add(orader[0]);
                }
                dgvComBox.Value = dgvComBox.Items[0].ToString();
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxZhongdu");
                return null;
            }
        }

        /// <summary>
        /// 生成带有“是、否”值的下拉列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>返回嵌入数据控件中的下拉列表</returns>
        private DataGridViewComboBoxCell dgvComboBoxShiFou()
        {
            try
            {
                DataGridViewComboBoxCell dgvComBox = new DataGridViewComboBoxCell();
                dgvComBox.Items.Add("是");
                dgvComBox.Items.Add("否");
                dgvComBox.Value = dgvComBox.Items[0];
                return dgvComBox;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dgvComboBoxJieZhen");
                return null;
            }

        }


        #region 查看影像 (由于编辑模块与主程序合并此功能已重复)
        /// <summary>
        /// 当地图视野变化时，根据比例尺指定地图级别
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>返回级别数</returns>
        //private int setToolslevel()
        //{
            //int iLevel = 0;
            //try
            //{
            //    if (mapControl1.Map.Scale >= 200000)
            //    {
            //        iLevel = 1;
            //    }
            //    else if (mapControl1.Map.Scale < 200000 && mapControl1.Map.Scale >= 100000)
            //    {
            //        iLevel = 2;
            //    }
            //    else if (mapControl1.Map.Scale < 100000 && mapControl1.Map.Scale >= 50000)
            //    {
            //        iLevel = 3;
            //    }
            //    else if (mapControl1.Map.Scale < 50000 && mapControl1.Map.Scale >= 20000)
            //    {
            //        iLevel = 4;
            //    }
            //    else if (mapControl1.Map.Scale < 20000 && mapControl1.Map.Scale >= 10000)
            //    {
            //        iLevel = 5;
            //    }
            //    else if (mapControl1.Map.Scale < 10000 && mapControl1.Map.Scale >= 5000)
            //    {
            //        iLevel = 6;
            //    }
            //    else
            //    {
            //        iLevel = 7;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    writeToLog(ex, "setToolslevel");
            //}
            //return iLevel;
        //}

        /// <summary>
        /// 关闭某一级别的影像图
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="iLevel">影像级别</param>
        private void closeOtherLevelImg(int iLevel)
        {
            //try
            //{
            //    GroupLayer gLayer = mapControl1.Map.Layers["影像"] as GroupLayer;
            //    if (gLayer == null) return;

            //    int iCount = gLayer.Count;
            //    for (int i = 0; i < iCount; i++)
            //    {
            //        IMapLayer layer = gLayer[0];
            //        string alies = layer.Alias;
            //        if (Convert.ToInt16(alies.Substring(1, 1)) != iLevel)
            //        {
            //            MapInfo.Engine.Session.Current.Catalog.CloseTable(alies);
            //        }
            //    }
            //    mapControl1.Refresh();
            //}
            //catch (Exception ex)
            //{
            //    writeToLog(ex, "closeOtherLevelImg");
            //}
        }

        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="iLevel">地图级别</param>
        private void CalRowColAndDisImg(int iLevel)
        {
            //double dScale = 0;
            //int minRow = 0, minCol = 0;
            //int maxRow = 0, maxCol = 0;
            //switch (iLevel)
            //{
            //    case 1:
            //        dScale = 200000;
            //        minRow = 1;
            //        minCol = 1;
            //        maxRow = 2;
            //        maxCol = 2;
            //        break;
            //    case 2:
            //        dScale = 100000;
            //        minRow = 1;
            //        minCol = 1;
            //        maxRow = 4;
            //        maxCol = 4;
            //        break;
            //    case 3:
            //        dScale = 50000;
            //        minRow = 2;
            //        minCol = 2;
            //        maxRow = 7;
            //        maxCol = 8;
            //        break;
            //    case 4:
            //        dScale = 20000;
            //        minRow = 3;
            //        minCol = 3;
            //        maxRow = 17;
            //        maxCol = 19;
            //        break;
            //    case 5:
            //        dScale = 10000;
            //        minRow = 6;
            //        minCol = 6;
            //        maxRow = 34;
            //        maxCol = 37;
            //        break;
            //    case 6:
            //        dScale = 5000;
            //        minRow = 12;
            //        minCol = 11;
            //        maxRow = 67;
            //        maxCol = 74;
            //        break;
            //    case 7:
            //        dScale = 2000;
            //        minRow = 29;
            //        minCol = 27;
            //        maxRow = 166;
            //        maxCol = 184;
            //        break;
            //}
            //double gridDis = 6.0914200613 * Math.Pow(10, -7) * dScale * 2;  //各级的网格长度
            //int beginRow = 0, endRow = 0;
            //int beginCol = 0, endCol = 0;
            ////计算行列号
            ////起始行号
            //int dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y2) / gridDis);
            //if (dRow > maxRow) return;    //如果此起始行号比本级图片最大行号还大，说明此范围无图

            //if (dRow < minRow)
            //{
            //    beginRow = minRow;
            //}
            //else
            //{
            //    beginRow = dRow;
            //}

            ////终止行号
            //dRow = Convert.ToInt32((23.08 - mapControl1.Map.Bounds.y1) / gridDis) + 1;
            //if (dRow < minRow) return; //如果此终止行号比本级图片最小行号还小，说明此范围无图
            //if (dRow > maxRow)
            //{
            //    endRow = maxRow;
            //}
            //else
            //{
            //    endRow = dRow;
            //}

            //int dCol = Convert.ToInt32((mapControl1.Map.Bounds.x1 - 112.94) / gridDis);
            //if (dCol > maxCol) return; //如果此起始列号比本级图片最大列号还大，说明此范围无图
            //if (dCol < minCol)
            //{
            //    beginCol = minCol;
            //}
            //else
            //{
            //    beginCol = dCol;
            //}

            //dCol = Convert.ToInt32((mapControl1.Map.Bounds.x2 - 112.94) / gridDis) + 1;  //计算终止列号
            //if (dCol < minCol) return; //如果此终止列号比本级图片最小列号还大，说明此范围无图
            //if (dCol > maxCol)
            //{
            //    endCol = maxCol;
            //}
            //else
            //{
            //    endCol = dCol;
            //}

            //DisImg(iLevel, beginRow, endRow, beginCol, endCol);
        }

        private void DisImg(int iLevel, int beginRow, int endRow, int beginCol, int endCol)
        {
            //string tabName = "";
            //for (int i = beginRow; i <= endRow; i++)
            //{
            //    for (int j = beginCol; j <= endCol; j++)
            //    {
            //        tabName = iLevel.ToString() + "_" + i.ToString() + "_" + j.ToString();
            //        openTable(iLevel, tabName);
            //    }
            //}
            // mapControl1.Refresh();
        }

        private void openTable(int iLevel, string tableName)
        {
            //再判断文件夹中村不存在，存在就打开
            //try
            //{
            //    GroupLayer groupLayer = mapControl1.Map.Layers["影像"] as GroupLayer;//先判断有没有加载
            //    if (groupLayer["_" + tableName] == null)
            //    {
            //        string imgPath = exePath.Remove(exePath.LastIndexOf("\\")) + "\\Data\\ImgData\\" + iLevel.ToString() + "\\" + tableName + ".tab";
            //        if (File.Exists(imgPath))
            //        {
            //            Table tab = MapInfo.Engine.Session.Current.Catalog.OpenTable(imgPath);

            //            MapInfo.Mapping.FeatureLayer fl = new MapInfo.Mapping.FeatureLayer(tab, "_" + tableName);

            //            groupLayer.Add(fl);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    writeToLog(ex, "openTable");
            //}
        }

        #endregion

        private string policeNo = "";
        /// <summary>
        /// tool按钮项
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Name)
                {
                    case "toolEditPolice":    // 警员联动编辑
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        switch (toolStrip1.Items[19].Text)
                        {
                            case "警员联动编辑":
                                frmGpsPolice frmgps = new frmGpsPolice();
                                if (frmgps.ShowDialog() == DialogResult.OK)
                                {
                                    policeNo = frmgps.textBox1.Text;
                                    GetPolice(policeNo);
                                }
                                else
                                {
                                    ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = false;
                                    toolStrip1.Items[19].Text = "警员联动编辑";
                                    toolStrip1.Items[20].Visible = false;
                                    this.timerGPS.Stop();
                                    DelGPSLayer();
                                    this.dataGridViewGaList.ContextMenuStrip = null;
                                }
                                break;
                            case "关闭联动编辑":
                                ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = false;
                                toolStrip1.Items[19].Text = "警员联动编辑";
                                toolStrip1.Items[20].Visible = false;
                                this.timerGPS.Stop();
                                DelGPSLayer();
                                this.dataGridViewGaList.ContextMenuStrip = null;
                                break;
                        }
                        break;
                    case "toolRefresh":     // 刷新
                        this.UncheckedTool();
                        ((System.Windows.Forms.ToolStripButton)e.ClickedItem).Checked = true;
                        DelGPSLayer();
                        GetPolice(policeNo);
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "toolStrip1_ItemClicked");
            }
        }

        /// <summary>
        /// 关闭图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        public void ClearLayer()
        {
            try
            {
                if (this.editTable != null)
                {
                    string sAlias = this.editTable.Alias;
                    if (mapControl1.Map.Layers[sAlias] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    }
                }
            }
            catch { MessageBox.Show("清除图层出错！", "提示"); }
        }

        /// <summary>
        /// 获警员进行联动编辑
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="policeNo">警力编号</param>
        private void GetPolice(string policeNo)
        {
            try
            {
                string sql = string.Empty;
                string _strUseRegion = this.strRegion;
                string _strZDRegion = this.strRegion1;
                string regionStr = "";   // 存放权限条件
                if (_strUseRegion == string.Empty && _strZDRegion == string.Empty)
                {
                    isShowPro(false);
                    MessageBox.Show(@"没有设置区域权限", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    // 权限设置（派出所及中队权限）
                    if (_strUseRegion != "顺德区")   // edit by fisher in 09-12-08
                    {
                        if (_strUseRegion != "")
                        {
                            if (Array.IndexOf(_strUseRegion.Split(','), "大良") > -1)
                            {
                                _strUseRegion = _strUseRegion.Replace("大良", "大良,德胜");
                            }
                            regionStr += " 派出所名 in ('" + _strUseRegion.Replace(",", "','") + "')";

                            if (_strZDRegion != "")
                            {
                                if (regionStr.IndexOf("and") > -1)
                                {
                                    regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                    regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                }
                                else
                                {
                                    regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                }
                            }
                        }
                        else if (_strUseRegion == "")
                        {
                            if (_strZDRegion != "")
                            {
                                if (regionStr.IndexOf("and") > -1)
                                {
                                    regionStr = regionStr.Remove(regionStr.LastIndexOf(")"));
                                    regionStr += " or 中队名 in ('" + _strZDRegion.Replace(",", "','") + "'))";
                                }
                                else
                                {
                                    regionStr += " 中队名 in ('" + _strZDRegion.Replace(",", "','") + "')";
                                }
                            }
                            else
                            {
                                MessageBox.Show("您没有查询权限!");
                                return;
                            }
                        }
                    }
                }
                if (regionStr == "")
                    sql = "Select 警力编号 as Name,警力编号 as 表_ID,'GPS警员' as 表名,X,Y from GPS警员 where 警力编号='" + policeNo + "'";
                else
                    sql = "Select 警力编号 as Name,警力编号 as 表_ID,'GPS警员' as 表名,X,Y from GPS警员 where 警力编号='" + policeNo + "' and " + regionStr;

                // 查询出结果在地图显示（2010-10-24）
                DataTable table = new DataTable();
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

                if (table.Rows.Count <= 0)
                {
                    MessageBox.Show("您查询的警员不存在！", "提示");
                    frmGpsPolice frmgps = new frmGpsPolice();
                    if (frmgps.ShowDialog() == DialogResult.OK)
                    {
                        policeNo = frmgps.textBox1.Text;
                        GetPolice(policeNo);
                    }
                    else
                    {
                        ((System.Windows.Forms.ToolStripButton)toolStrip1.Items[19]).Checked = false;
                        toolStrip1.Items[19].Text = "警员联动编辑";
                        toolStrip1.Items[20].Visible = false;
                        this.timerGPS.Stop();
                        DelGPSLayer();
                    }
                    return;
                }
                toolStrip1.Items[19].Text = "关闭联动编辑";
                toolStrip1.Items[20].Visible = true;
                this.timerGPS.Interval = Convert.ToInt32(getGPSTime());
                this.timerGPS.Start();
                CreateGPSLayer(table);
                this.dataGridViewGaList.ContextMenuStrip = this.contextMenuStrip1;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GetPolice");
            }
        }

        /// <summary>
        /// 创建“GPSLayer”图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dt">数据源</param>
        private void CreateGPSLayer(DataTable dt)
        {
            try
            {
                DelGPSLayer();

                MapInfo.Data.TableInfoAdoNet ti = new MapInfo.Data.TableInfoAdoNet("GPSLayer", dt);
                MapInfo.Data.SpatialSchemaXY xy = new SpatialSchemaXY();
                xy.XColumn = "X";
                xy.YColumn = "Y";
                xy.NullPoint = "0.0,0.0";
                xy.StyleType = MapInfo.Data.StyleType.None;
                xy.CoordSys = MapInfo.Engine.Session.Current.CoordSysFactory.CreateLongLat(MapInfo.Geometry.DatumID.WGS84);
                ti.SpatialSchema = xy;

                MapInfo.Data.Table temTable = MapInfo.Engine.Session.Current.Catalog.OpenTable(ti);

                FeatureLayer temlayer = new FeatureLayer(temTable, "GPSLayer");

                this.mapControl1.Map.Layers.Add(temlayer);

                //改变图层的图元样式
                CompositeStyle cs = new CompositeStyle(new BitmapPointStyle("Police.bmp", BitmapStyles.None, System.Drawing.Color.Red, 20));
                FeatureOverrideStyleModifier fsm = new MapInfo.Mapping.FeatureOverrideStyleModifier(null, cs);
                temlayer.Modifiers.Clear();

                // ProtectMap();

                temlayer.Modifiers.Append(fsm);

                //添加标注
                const string activeMapLabel = "GPSLabel";
                Table activeMapTable = MapInfo.Engine.Session.Current.Catalog.GetTable("GPSLayer");
                LabelLayer lblayer = new LabelLayer(activeMapLabel, activeMapLabel);

                LabelSource lbsource = new LabelSource(activeMapTable);
                lbsource.DefaultLabelProperties.Style.Font.Name = "楷书";
                lbsource.DefaultLabelProperties.Style.Font.Size = 10;
                lbsource.DefaultLabelProperties.Layout.Alignment = MapInfo.Text.Alignment.BottomCenter;

                lbsource.DefaultLabelProperties.Layout.Offset = 2;
                lbsource.DefaultLabelProperties.Style.Font.ForeColor = System.Drawing.Color.DarkBlue;
                lbsource.DefaultLabelProperties.Style.Font.BackColor = System.Drawing.Color.Transparent;
                lbsource.DefaultLabelProperties.Caption = "Name";
                lblayer.Sources.Append(lbsource);

                //ProtectMap();

                mapControl1.Map.Layers.Add(lblayer);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CreateGPSLayer");
            }
        }

        /// <summary>
        /// 移除“GPSLayer”图层
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void DelGPSLayer()
        {
            try
            {
                if (mapControl1.Map.Layers["GPSLayer"] != null)
                    MapInfo.Engine.Session.Current.Catalog.CloseTable("GPSLayer");
                if (mapControl1.Map.Layers["GPSLabel"] != null)
                    mapControl1.Map.Layers.Remove("GPSLabel");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "DelLayer");
            }
        }

        //点击工具栏上的工具时，对工具按钮进行设置，
        //选中的背景颜色设为白色，其他透明，以便明确当前的选择项
        //由于按钮分组，iFrom表示组的首Index，iEnd表示末Index
        /// <summary>
        /// 将ToolStripButton设置为未选中的状态
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void UncheckedTool()
        {
            try
            {
                for (int i = 0; i < this.toolStrip1.Items.Count; i++)
                {
                    System.Windows.Forms.ToolStripButton tempToolStripButton = (System.Windows.Forms.ToolStripButton)this.toolStrip1.Items[i];
                    if (tempToolStripButton != null)
                    {
                        if (tempToolStripButton.Checked == true)
                        {
                            tempToolStripButton.Checked = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "UncheckedTool");
            }
        }

        /// <summary>
        /// 当鼠标移到某一个中队或警务室时显示该中队或警务室的名字
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void mapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!this.Visible) return; // 不是当前模块返回

                if (tabControl1.SelectedTab == tabPage1 && this.comboTables.Text == "民警中队辖区")
                {
                    // 当鼠标移到某一个警务室时显示该警务室的名字
                    System.Drawing.Point point = e.Location;
                    MapInfo.Geometry.DPoint dpt;
                    mapControl1.Map.DisplayTransform.FromDisplay(point, out dpt);

                    Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 30);
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dpt, mapControl1.Map.GetDisplayCoordSys(), d);
                    si.SearchResultProcessor = null;
                    si.QueryDefinition.Columns = null;

                    Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("中队辖区", si);

                    if (ft2 != null)
                    {
                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                        string sqlStr = "select 中队名 from 基层民警中队 where 中队代码 = '" + ft2["中队代码"].ToString() + "'";
                        DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        this.label5.Text = table.Rows[0][0].ToString();
                        point.X = point.X + 20;
                        this.panPc.Location = point;
                        this.panPc.Visible = true;
                    }
                    else
                    {
                        this.panPc.Visible = false;
                    }
                }
                if (tabControl1.SelectedTab == tabPage1 && this.comboTables.Text == "警务室辖区")
                {
                    // 当鼠标移到某一个警务室时显示该警务室的名字
                    System.Drawing.Point point = e.Location;
                    MapInfo.Geometry.DPoint dpt;
                    mapControl1.Map.DisplayTransform.FromDisplay(point, out dpt);

                    Distance d = MapInfo.Mapping.SearchInfoFactory.ScreenToMapDistance(mapControl1.Map, 30);
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchNearest(dpt, mapControl1.Map.GetDisplayCoordSys(), d);
                    si.SearchResultProcessor = null;
                    si.QueryDefinition.Columns = null;

                    Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("警务室辖区", si);

                    if (ft2 != null)
                    {
                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                        string sqlStr = "select 警务室名 from 社区警务室 where 警务室代码 = '" + ft2["警务室代码"].ToString() + "'";
                        DataTable table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sqlStr);
                        this.label5.Text = table.Rows[0][0].ToString();
                        point.X = point.X + 20;
                        this.panPc.Location = point;
                        this.panPc.Visible = true;
                    }
                    else
                    {
                        this.panPc.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapControl1_MouseMove");
            }
        }

        private string valueText = "";
        MapInfo.Data.Table editTable = null;
        MapInfo.Data.Table lTable = null;
        /// <summary>
        /// 基础数据编辑的下拉列表切换事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void comboTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (this.valueText == this.comboTables.Text)
                {
                    this.mapControl1.Focus();
                    return;
                }

                dataGridViewList.DataSource = null;
                //dataGridViewList.Visible = false;
                dataGridView1.Rows.Clear();
                dataGridView1.Visible = false;

                this.bpageCount = 0;
                this.bpageCurrent = 0;
                this.toolStripRecord.Text = "0条";
                this.tsTextBoxPageNow.Text = "0";//设置当前页
                this.tStripLabelPageCount.Text = "/ {0}";//设置总页数
                this.toolStripPageSize.Text = bpageSize.ToString();

                this.valueText = this.comboTables.Text;

                CLC.ForSDGA.GetFromTable.GetFromName(valueText, getFromNamePath);
                label3.Text = CLC.ForSDGA.GetFromTable.ObjName;
                try  //先关闭之前的表和标注表
                {
                    if (this.editTable != null)
                    {
                        string sAlias = this.editTable.Alias;
                        if (mapControl1.Map.Layers[sAlias] != null)
                        {
                            MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                        }
                    }
                }
                catch (Exception ex) { writeToLog(ex, "comboTables_SelectedIndexChanged-关闭之前的表和标注表"); }

                this.btnLoc1.Enabled = false;
                this.mapControl1.Tools.LeftButtonTool = "Pan";

                textKeyWord.Text = "";
                switch (comboTables.Text)
                {
                    case "视频位置":
                    case "公共场所":
                    case "安全防护单位":
                    case "网吧":
                    case "特种行业":
                    case "消防栓":
                    case "消防重点单位":
                    case "治安卡口系统":
                    case "基层派出所":
                    case "基层民警中队":
                    case "社区警务室":
                        mapToolBar1.Buttons[0].Visible = true;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = true;
                        mapToolBar1.Buttons[6].Visible = false;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPolygon")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = false;
                        this.buttonSearch.Enabled = true;
                        this.textKeyWord.Enabled = true;
                        this.cbMohu.Enabled = true;
                        this.textKeyWord.Focus();
                        featStyle = setFeatStyle(comboTables.Text);
                        this.GetTable(this.comboTables.Text);
                        break;
                    case "派出所辖区":
                    case "警务室辖区":
                    case "民警中队辖区":
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = true;
                        mapToolBar1.Buttons[3].Visible = true;
                        mapToolBar1.Buttons[3].Pushed = false;
                        mapToolBar1.Buttons[4].Visible = true;
                        mapToolBar1.Buttons[4].Enabled = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = true;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPoint")
                        {
                            this.mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.buttonSearch.Enabled = false;
                        this.textKeyWord.Enabled = false;
                        this.cbMohu.Enabled = false;
                        this.mapControl1.Focus();
                        this.isOracleSpatialTab = true;
                        this.GetTable(this.comboTables.Text);
                        break;
                    case "人口系统":
                    case "出租屋房屋系统":
                    case "案件信息":
                        mapToolBar1.Buttons[0].Visible = true;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = true;
                        mapToolBar1.Buttons[6].Visible = false;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPolygon")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = true;
                        this.buttonSearch.Enabled = true;
                        this.textKeyWord.Enabled = true;
                        this.cbMohu.Enabled = true;
                        this.textKeyWord.Focus();
                        featStyle = setFeatStyle(comboTables.Text);
                        this.GetTable(this.comboTables.Text);
                        break;
                    case "派出所每日警员表":
                        if (listPaichusuo[0] == "" || (listPaichusuo.Length == 1 && listPaichusuo[0] == "其他派出所"))
                        {
                            MessageBox.Show("您没有该操作权限");
                            break;
                        }
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = false;

                        frmPoliceCount fPoliceCount = new frmPoliceCount(listPaichusuo, comboTables.Text, this.temEditDt);
                        fPoliceCount.ShowDialog();
                        break;
                    case "中队每日警员表":
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = false;

                        frmPoliceCount fPoliceCount1 = new frmPoliceCount(listPaichusuo, comboTables.Text, this.temEditDt);
                        fPoliceCount1.ShowDialog();
                        break;
                    case "案件类别维护":
                        mapToolBar1.Buttons[0].Visible = false;
                        mapToolBar1.Buttons[1].Visible = false;
                        mapToolBar1.Buttons[3].Visible = false;
                        mapToolBar1.Buttons[4].Visible = false;
                        mapToolBar1.Buttons[5].Visible = false;
                        mapToolBar1.Buttons[6].Visible = false;
                        frmCaseType fCaseType = new frmCaseType(mysqlstr);
                        fCaseType.ShowDialog();
                        break;
                }
                //fisher(09-09-01)
                switch (comboTables.Text)
                {
                    case "视频位置":
                    case "治安卡口系统":
                    case "基层派出所":
                    case "基层民警中队":
                    case "社区警务室":
                        this.btnLocatedYes.Enabled = true;
                        this.btnLocatedNo.Enabled = true;
                        break;
                    default:
                        this.btnLocatedYes.Enabled = false;
                        this.btnLocatedNo.Enabled = false;
                        break;
                }
                if (comboTables.Text == "视频位置")
                {
                    mapToolBar1.Buttons[0].Visible = false;
                    mapToolBar1.Buttons[5].Visible = false;
                }

                _exportDT = null;

                buttonSave.Text = "保存";
                buttonCancel.Text = "取消";
                buttonSave.Enabled = false;
                buttonCancel.Enabled = false;

                SetButtonStyle(this.valueText);//设置编辑按钮
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex) { writeToLog(ex, "comboTables_SelectedIndexChanged"); }
        }

        /// <summary>
        /// 设置点样式
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="p">表名</param>
        /// <returns>点样式</returns>
        private Style setFeatStyle(string p)
        {
            try
            {
                GetFromName getName = new GetFromName(p);
                BasePointStyle ppStyle;
                string bmpName = getName.BmpName;

                if (bmpName == "anjian")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(34, Color.Red, 9);
                    return ppStyle;
                }
                else if (bmpName == "gonggong")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(33, Color.Yellow, 9);
                    return ppStyle;
                }
                else if (bmpName == "tezhong")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(36, Color.Cyan, 10);
                    return ppStyle;
                }
                else if (bmpName == "")
                {
                    ppStyle = new MapInfo.Styles.SimpleVectorPointStyle(45, Color.Blue, 15);
                    return ppStyle;
                }
                else
                {
                    MapInfo.Styles.BitmapPointStyle bitmappointstyle = new MapInfo.Styles.BitmapPointStyle(bmpName);
                    bitmappointstyle.NativeSize = true;
                    return bitmappointstyle;
                }
            }
            catch (Exception ex) { writeToLog(ex, "setFeatStyle"); return null; }
        }

        /// <summary>
        /// 从数据库中得到表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="tableName">表名</param>
        private void GetTable(string tableName)
        {
            if (isOracleSpatialTab)
            {
                MIConnection miConnection = new MIConnection();
                try
                {
                    if (this.lTable != null)
                    {
                        this.lTable.Close();
                    }
                    miConnection.Open();

                    //之前有空间表打不开, 所以用了多次打开.
                    try
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select  *  From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;

                        this.lTable = miConnection.Catalog.OpenTable(ti);
                    }
                    catch
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select * From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;
                        this.lTable = miConnection.Catalog.OpenTable(ti);
                    }

                    MapInfo.Data.TableInfoNative ListTableInfo = new MapInfo.Data.TableInfoNative(tableName + "_tem");
                    ListTableInfo.Temporary = true;
                    MapInfo.Geometry.CoordSys LCoordsys;
                    MapInfo.Data.GeometryColumn GC = (MapInfo.Data.GeometryColumn)(this.lTable.TableInfo.Columns["obj"]);
                    LCoordsys = GC.CoordSys;
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateFeatureGeometryColumn(LCoordsys));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateIntColumn("MapID"));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("keyID", 100));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("Name", 200));
                    ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStyleColumn());

                    this.editTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ListTableInfo);

                    if (tableName == "派出所辖区" || tableName == "民警中队辖区" || tableName == "警务室辖区")
                    {
                        CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                        MICommand command = miConnection.CreateCommand();
                        command.CommandText = "insert into " + tableName + "_tem Select obj,MI_PRINX as MapID," + CLC.ForSDGA.GetFromTable.ObjID + " as keyID," + CLC.ForSDGA.GetFromTable.ObjName + " as Name,MI_STYLE From " + lTable.Alias + " t";
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    miConnection.Close();

                    //地图显示
                    FeatureLayer fl = new FeatureLayer(editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(editTable);//标注图层

                    if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                    {
                        MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    }
                    MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    mapControl1.Map.Center = mapControl1.Map.Center;
                }
                catch (Exception ex)
                {
                    if (miConnection.State == ConnectionState.Open)
                    {
                        miConnection.Close();
                    }
                    writeToLog(ex, "GetTable-OracleSpatialTab");
                }
            }
            else
            {
                try
                {
                    if (this.editTable != null)
                    {
                        this.editTable.Close();
                    }
                    this.editTable = createTable(tableName);
                    FeatureLayer fl = new FeatureLayer(this.editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(this.editTable);//标注图层

                    if (this.editTable != null)
                    {
                        if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                        }
                        MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                        //setPrivilege();
                        mapControl1.Map.Center = mapControl1.Map.Center;
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "GetTable");
                }
            }

            this.InitialEditFields(this.comboTables.Text);
        }

        /// <summary>
        /// 根据表名创建表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>MapInfo表</returns>
        private MapInfo.Data.Table createTable(string tableName)
        {
            try
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
                    column.Alias = "keyID";
                    column.DataType = MIDbType.String;
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
            catch (Exception ex)
            {
                writeToLog(ex, "createTable");
                return null;
            }
        }

        /// <summary>
        /// 判断主键是否重复
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <returns>布尔值(true-是 false-否)</returns>
        private bool isZhujian()
        {
            OracleConnection Conn = new OracleConnection(mysqlstr); //插入数据库
            Conn.Open();
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
            string Pdstr = "select " + CLC.ForSDGA.GetFromTable.ObjID + " from " + comboTables.Text;
            OracleCommand orcmd = new OracleCommand(Pdstr, Conn);
            OracleDataAdapter PdAdapter = new OracleDataAdapter(orcmd);
            DataSet Pdds = new DataSet();
            PdAdapter.Fill(Pdds);
            DataTable Pddt = Pdds.Tables[0];
            try
            {
                int ix = 0;
                for (int j = 0; j < dataGridView1.Rows.Count; j++)
                {
                    if (dataGridView1.Rows[j].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = j;
                    }
                }
                if (Pddt.Rows.Count < 1)
                {
                    orcmd.Dispose();
                    Conn.Close();
                    return false;
                }
                else
                {
                    for (int i = 0; i < Pddt.Rows.Count; i++)
                    {
                        if (Pddt.Rows[i][0].ToString() == dataGridView1.Rows[ix].Cells[1].Value.ToString())
                        {
                            orcmd.Dispose();
                            Conn.Close();
                            return true;
                        }
                    }
                }
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "isZhujian()");
                MessageBox.Show(ex.Message, "isZhujian()");
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
        }

        int mapId = 0;
        /// <summary>
        /// 基础数据保存按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            //先判断主键字段,如果输入的值已存在,给出提示
            OracleConnection Conn = new OracleConnection(mysqlstr); //插入数据库

            this.Cursor = Cursors.WaitCursor;

            this.dataGridView1.CurrentCell = null;//在进行保存的时候让DataGridView1失去焦点
            this.mapControl1.Focus();

            bool IsorKey = false;

            if (isOracleSpatialTab)
            {
                OracleDataReader dr = null;
                try
                {
                    Conn.Open();
                    OracleCommand cmd;
                    if (buttonSave.Text == "保存")
                    {
                        IsorKey = isZhujian();
                        if (IsorKey)
                        {
                            MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            IsorKey = false;
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        else
                        {
                            cmd = new OracleCommand("select max(MI_PRINX) from " + comboTables.Text, Conn);
                            dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                dr.Read();
                                if (dr.GetValue(0) != null)
                                {
                                    selPrinx = Convert.ToInt32(dr.GetValue(0)) + 1;
                                }
                            }
                            dr.Close();
                            cmd.Dispose();

                            feature = new Feature(lTable.TableInfo.Columns);
                            feature.Geometry = ft.Geometry;
                            feature.Style = ft.Style;

                            string strValue = "";
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                strValue = "";
                                if (dataGridView1.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                }
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "照片")
                                {
                                    continue;
                                }
                                if (dataGridView1.Rows[i].Cells[2].Value != null && dataGridView1.Rows[i].Cells[2].Value.ToString() == "必填")
                                {
                                    if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridView1.Rows[i].Cells[0].Value.ToString() + " 不能为空.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        return;
                                    }
                                }
                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    feature[dataGridView1.Rows[i].Cells[0].Value.ToString()] = Convert.ToDateTime(strValue);
                                }
                                else
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    feature[dataGridView1.Rows[i].Cells[0].Value.ToString()] = strValue;
                                }
                            }
                            feature["MI_PRINX"] = selPrinx;

                            for (int i = 0; i < feature.Columns.Count; i++)
                            {
                                if (feature.Columns[i].Alias.ToUpper() == "MAPID")
                                {
                                    feature["MapID"] = selPrinx;
                                    break;
                                }
                            }
                            lTable.InsertFeature(feature);

                            //由于feature只记录时间值的日期部分,所以再次更新date型值
                            string command = "update " + comboTables.Text + " set ";
                            strValue = "";
                            bool isUpdate = false;
                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridView1.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                }

                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE" && strValue != "")
                                {
                                    DateTime dTime = Convert.ToDateTime(strValue);
                                    if (dTime.TimeOfDay.ToString() != "00:00:00")
                                    {
                                        command += dataGridView1.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                        isUpdate = true;
                                    }
                                }

                            }
                            if (isUpdate)
                            {
                                command = command.Remove(command.Length - 1);
                                command += " where MI_PRINX=" + selPrinx;
                                cmd = new OracleCommand(command, Conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                Conn.Close();
                            }

                            // 新添加的数据不允许马上就执行更新，因为会出错，只能切换到dataGridViewGaList选择某条记录后才能更新
                            // updated by fisher in 09-10-23
                            this.dataGridView1.Columns[1].ReadOnly = true;
                            addToList(feature.Geometry.Centroid.x, feature.Geometry.Centroid.y);
                        }
                    }
                    else
                    {//更新
                        string command = "update " + comboTables.Text + " set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                        {
                            strValue = "";
                            if (dataGridView1.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            }
                            if (dataGridView1.Rows[i].Cells[0].Value.ToString() != "照片")
                            {
                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    command += dataGridView1.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X = '0',";
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X='" + strValue + "',";
                                    }
                                }
                                else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y = '0',";
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y='" + strValue + "',";
                                    }
                                }
                                else
                                {
                                    command += dataGridView1.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                                }
                            }
                        }
                        command = command.Remove(command.Length - 1);

                        //update by fisher in 1019
                        GetFromName OrgetName = new GetFromName(comboTables.Text);
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                        command += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewList.Rows[browIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                        cmd = new OracleCommand(command, Conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        Conn.Close();

                        updateListValue();
                    }
                    //更新地图要素的属性
                    updateMapValue();

                }
                catch (OracleException ex)
                {
                    if (ex.Code == 1)//主键字段输入了非唯一值
                    {
                        MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "buttonSave_Click-OracleSpatialTab");
                    }
                    this.Cursor = Cursors.Default;
                    if (Conn.State == ConnectionState.Open)
                        Conn.Close();
                    return;
                }

                if (comboTables.Text.Trim() == "人口系统")
                {
                    if (hasUpdate)
                    {
                        Conn = new OracleConnection(mysqlstr); //插入数据库
                        OracleCommand cmd;
                        Conn.Open();
                        string strValue = "";

                        try
                        {
                            string strExe = "";
                            strValue = "";
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "身份证号码")
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                    break;
                                }
                            }
                            if (strValue != "")
                            {
                                if (buttonSave.Text == "保存")
                                {
                                    strExe = "insert into 人口照片(身份证号码,照片) values('" + strValue + "' ,:imgData)";
                                }
                                else
                                {
                                    //先查询,看看人口照片中有没有对应的对象
                                    cmd = new OracleCommand("select * from 人口照片 where 身份证号码='" + strValue + "'", Conn);
                                    OracleDataReader oDr = cmd.ExecuteReader();
                                    if (oDr.HasRows)
                                    {
                                        strExe = "update 人口照片 set 照片=:imgData where 身份证号码='" + strValue + "'";
                                    }
                                    else
                                    {
                                        strExe = "insert into 人口照片(身份证号码,照片) values('" + strValue + "' ,:imgData)";
                                    }
                                    oDr.Close();
                                }
                                cmd = new OracleCommand(strExe, Conn);

                                if (fileName == "")
                                {
                                    fileName = Application.StartupPath + "\\默认.bmp";
                                }

                                FileStream fs = File.OpenRead(fileName);
                                byte[] imgData = new byte[fs.Length];
                                long n = fs.Read(imgData, 0, imgData.Length);
                                cmd.Parameters.Add(":ImgData", OracleType.LongRaw).Value = imgData;
                                fs.Close();

                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "buttonSave_Click-OracleSpatialTab");
                            if (Conn.State == ConnectionState.Open)
                                Conn.Close();
                        }
                    }
                    Conn.Close();
                }
            }

            else  //非oracleSpatial表
            {
                try
                {
                    Conn.Open();
                    OracleCommand cmd;
                    string strExe = "";
                    if (buttonSave.Text == "保存")
                    {
                        IsorKey = isZhujian();
                        if (IsorKey)
                        {
                            MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            IsorKey = false;
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        else
                        {
                            strExe = "insert into " + comboTables.Text + "(";

                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                if (dataGridView1.Rows[i].Cells[2].Value != null && dataGridView1.Rows[i].Cells[2].Value.ToString() == "必填")
                                {
                                    if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridView1.Rows[i].Cells[0].Value.ToString() + " 不能为空.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        return;
                                    }
                                }
                                if (i == 0)
                                    strExe += dataGridView1.Rows[i].Cells[0].Value.ToString();
                                else
                                    strExe += "," + dataGridView1.Rows[i].Cells[0].Value.ToString();
                            }

                            strExe += ") values(";

                            string strValue = "";
                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridView1.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                }
                                else
                                {
                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        strValue = "0";
                                    }
                                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        strValue = "0";
                                    }
                                }
                                if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    strExe += "to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else
                                {
                                    strExe += "'" + strValue + "',";
                                }
                            }

                            strExe = strExe.Remove(strExe.Length - 1);
                            strExe += ")";

                            selMapID = mapId.ToString();

                            // 新添加的数据不允许马上就执行更新，因为会出错，只能切换到dataGridViewGaList选择某条记录后才能更新
                            // updated by fisher in 09-10-23
                            this.dataGridView1.Columns[1].ReadOnly = true;
                            addToList(dx, dy);
                        }
                    }
                    else
                    {   //更新
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                        strExe = "update " + comboTables.Text + " set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                        {
                            //update by siumo 09-01-08, 当为null时,toString出错
                            strValue = "";
                            if (dataGridView1.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            }
                            else
                            {
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                    strValue = "0";
                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                    strValue = "0";
                            }
                            if (dataGridView1.Rows[i].Cells[2].Value != null && dataGridView1.Rows[i].Cells[2].Value.ToString() == "必填")
                            {
                                if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                                {
                                    MessageBox.Show(dataGridView1.Rows[i].Cells[0].Value.ToString() + " 不能为空.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Cursor = Cursors.Default;
                                    return;
                                }
                            }
                            if (this.dataGridView1.Rows[i].Cells[3].Value.ToString() == "DATE")
                            {
                                strExe += dataGridView1.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                            }
                            else
                            {
                                strExe += dataGridView1.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                            }
                        }
                        strExe = strExe.Remove(strExe.Length - 1);
                        strExe += "where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewList.Rows[browIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";

                        updateListValue();
                    }

                    cmd = new OracleCommand(strExe, Conn);
                    cmd.ExecuteNonQuery();

                    Conn.Close();
                }
                catch (OracleException ex)
                {
                    if (Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                    if (ex.Code == 1)//主键字段输入了非唯一值
                    {
                        MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "buttonSave_Click");
                    }
                    this.Cursor = Cursors.Default;
                    return;
                }

                //更新地图要素的属性
                updateMapValue();

            }
            string editMothed = "添加";
            if (buttonSave.Text == "更新")
            {
                MessageBox.Show("更新成功!");
                editMothed = "修改属性";
            }
            else
            {
                MessageBox.Show("添加数据成功!");
                buttonSave.Text = "更新";
                buttonCancel.Text = "取消";
            }

            //记录编辑log
            WriteEditLog(comboTables.Text.Trim(), selMapID, editMothed);
            this.mapControl1.Tools.LeftButtonTool = "Pan";
            this.UncheckedTool();
            this.dataGridView1.CurrentCell = null;
            buttonSave.Enabled = false;
            buttonCancel.Enabled = false;
            try
            {
                string tabName = comboTables.Text.Trim();
                if (isOracleSpatialTab)
                {
                    tabName += "_tem";
                }
                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex) { writeToLog(ex, "buttonSave_Click"); }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 基础数据编辑模块 更新列表中的值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void updateListValue()
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);

                //得到keyID的值
                string sId = "";
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        sId = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        break;
                    }
                }

                //查找keyID在列表中的列
                int ix = 0;
                for (int i = 0; i < dataGridViewList.Columns.Count; i++)
                {
                    if (dataGridViewList.Columns[i].Name == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = i;
                        break;
                    }
                }

                //查找更新要素在列表中的行,并更新这一行的值.
                for (int i = 0; i < dataGridViewList.Rows.Count; i++)
                {
                    if (dataGridViewList.Rows[i].Cells[ix].Value.ToString() == sId)
                    {
                        for (int j = 0; j < dataGridView1.Rows.Count; j++)
                        {
                            if (dataGridView1.Rows[j].Cells[0].Value.ToString() != "照片")
                            {
                                dataGridViewList.Rows[i].Cells[dataGridView1.Rows[j].Cells[0].Value.ToString()].Value = dataGridView1.Rows[j].Cells[1].Value;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "updateListValue");
            }
        }

        /// <summary>
        /// 业务数据编辑模块 更新列表中的值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void GaupdateListValue()
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);

                //得到keyID的值
                string sId = "";
                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                {
                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        sId = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                        break;
                    }
                }

                //查找keyID在列表中的列
                int ix = 0;
                for (int i = 0; i < dataGridViewGaList.Columns.Count; i++)
                {
                    if (dataGridViewGaList.Columns[i].Name == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = i;
                        break;
                    }
                }

                //查找更新要素在列表中的行,并更新这一行的值.
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (dataGridViewGaList.Rows[i].Cells[ix].Value.ToString() == sId)
                    {
                        for (int j = 0; j < dataGridViewGaInfo.Rows.Count; j++)
                        {
                            if (dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString() != "照片")
                            {
                                dataGridViewGaList.Rows[i].Cells[dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString()].Value = dataGridViewGaInfo.Rows[j].Cells[1].Value;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaupdateListValue");
            }
        }

        /// <summary>
        /// 基础数据编辑模块  向列表中添加记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        private void addToList(double x, double y)
        {
            try
            {
                if (dataGridViewList.Rows.Count == 0)
                {
                    dataGridViewList.Columns.Clear();
                    string stag = "";
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        stag = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        if (stag != "照片")
                        {
                            dataGridViewList.Columns.Add(stag, stag);
                        }
                    }
                }

                DataTable dt = new DataTable();
                if (dataGridViewList.DataSource == null)
                {
                    for (int i = 0; i < dataGridViewList.Columns.Count; i++)
                    {
                        dt.Columns.Add(dataGridViewList.Columns[i].Name);
                    }
                }
                else
                {
                    dt = (DataTable)dataGridViewList.DataSource;
                }
                DataRow dr = dt.NewRow();

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString() != "照片")
                    {
                        if (dt.Columns[dataGridView1.Rows[i].Cells[0].Value.ToString()].DataType.ToString() == "System.Decimal" && dataGridView1.Rows[i].Cells[1].Value.ToString().Trim() == "")
                        { continue; }
                        else
                        {
                            dr[dataGridView1.Rows[i].Cells[0].Value.ToString()] = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        }
                    }
                }
                dr["x"] = x.ToString("#.######");
                dr["y"] = y.ToString("#.######");
                dt.Rows.Add(dr);
                dataGridViewList.DataSource = null;
                dataGridViewList.Columns.Clear();
                dataGridViewList.DataSource = dt;

                setDataGridBG();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "addToList");
            }
        } 

        /// <summary>
        /// 业务数据编辑模块 向列表中添加记录
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        private void GaaddToList(double x, double y)
        {
            try
            {
                if (dataGridViewGaList.Rows.Count == 0)
                {
                    dataGridViewGaList.Columns.Clear();
                    string stag = "";
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                    {
                        stag = dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                        if (stag != "照片")
                        {
                            dataGridViewGaList.Columns.Add(stag, stag);
                        }
                    }
                }

                DataTable dt = new DataTable();
                if (dataGridViewGaList.DataSource == null)
                {
                    for (int i = 0; i < dataGridViewGaList.Columns.Count; i++)
                    {
                        dt.Columns.Add(dataGridViewGaList.Columns[i].Name);
                    }
                }
                else
                {
                    dt = (DataTable)dataGridViewGaList.DataSource;
                }
                DataRow dr = dt.NewRow();

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                {
                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() != "照片")
                    {
                        if (dt.Columns[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()].DataType.ToString() == "System.Decimal" && dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString().Trim() == "")
                        { continue; }
                        else
                        {
                            dr[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                        }
                    }
                }
                dr["x"] = x.ToString("#.######");
                dr["y"] = y.ToString("#.######");
                dt.Rows.Add(dr);
                dataGridViewGaList.DataSource = null;
                dataGridViewGaList.Columns.Clear();
                dataGridViewGaList.DataSource = dt;

                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaaddToList");
            }
        }

        /// <summary>
        /// 基础数据编辑模块 根据更改的值更新地图值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void updateMapValue()
        {
            try
            {
                if (ft != null)
                {
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text.Trim(), getFromNamePath);
                    string sname = "";
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjName)
                        {
                            sname = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    string sKey = "";
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                        {
                            sKey = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    ft["MapID"] = selPrinx;
                    ft["keyID"] = sKey;
                    ft["Name"] = sname;
                    ft.Update();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "updateMapValue");
            }
        }

        /// <summary>
        /// 业务数据编辑模块 根据更改的值更新地图值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void GaupdateMapValue()
        {
            try
            {
                if (ft != null)
                {
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text.Trim(), getFromNamePath);
                    string sname = "";
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                    {
                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjName)
                        {
                            sname = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    string sKey = "";
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                    {
                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                        {
                            sKey = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            break;
                        }
                    }

                    ft["MapID"] = selPrinx;
                    ft["keyID"] = sKey;
                    ft["Name"] = sname;
                    ft.Update();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaupdateMapValue");
            }
        }

        /// <summary>
        /// 操作记录日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sTabName">表名</param>
        /// <param name="editID">MapID/编号</param>
        /// <param name="editMothed">操作方式</param>
        private void WriteEditLog(string sTabName, string editID, string editMothed)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();
                OracleCommand cmd;
                string strExe = "insert into 操作记录 values('" + temEditDt.Rows[0]["userNow"].ToString() + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'编辑模块','表：" + sTabName + " 记录MapID/编号：" + editID + "','" + editMothed + "')";
                cmd = new OracleCommand(strExe, Conn);
                cmd.ExecuteNonQuery();

                Conn.Close();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "WriteEditLog");
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }

        /// <summary>
        /// 基础数据编辑 取消按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonCancel.Text == "取消添加")
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++) //清空datagridview
                    {
                        dataGridView1.Rows[i].Cells[1].Value = "";
                    }
                    dataGridView1.Visible = false;
                    buttonSave.Enabled = false;
                    buttonCancel.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();
                    deleteFeature();
                }
                else if (buttonCancel.Text == "取消")
                {
                    try
                    {
                        for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                        {
                            if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "照片")
                                continue;
                            this.dataGridView1.Rows[i].Cells[1].Value = this.dataGridViewList.Rows[browIndex].Cells[dataGridView1.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        MessageBox.Show(ex.Message, "buttonCancel_Click(取消)");
                    }

                    //地图上的点位置复原
                    Feature f;

                    //added by fisher in 09-09-23

                    if (this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value.ToString() == "" || this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value.ToString() == ""
                     || this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value.ToString() == "0" || this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value.ToString() == "0")
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("MapID=" + Convert.ToInt32(this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        this.buttonSave.Enabled = false;
                        this.buttonCancel.Enabled = false;
                        this.mapControl1.Tools.LeftButtonTool = "Pan";
                        this.UncheckedTool();
                        btnLoc1.Enabled = false;
                        return;
                        //删除后，跳出！ 
                    }

                    if (Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value) != 0)
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("MapID=" + Convert.ToInt32(this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        //先删除
                    }

                    //添加点
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                    string sName = CLC.ForSDGA.GetFromTable.ObjName;

                    f = new Feature(this.editTable.TableInfo.Columns);
                    f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(Convert.ToDouble(this.dataGridViewList.Rows[browIndex].Cells["X"].Value), Convert.ToDouble(this.dataGridViewList.Rows[browIndex].Cells["Y"].Value)));
                    f["mapid"] = this.dataGridViewList.Rows[browIndex].Cells["mapid"].Value.ToString();
                    f["name"] = this.dataGridViewList.Rows[browIndex].Cells[sName].Value.ToString();
                    f.Style = featStyle;
                    this.editTable.InsertFeature(f);
                    this.dataGridView1.CurrentCell = null;
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonCancel_Click");
            }
        }

        /// <summary>
        /// 基础数据编辑 删除图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void deleteFeature()
        {
            try
            {
                editTable.DeleteFeature(ft);
                ft = null;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "deleteFeature-删除图元");
            }

            //设置可添加
            string tabName = comboTables.Text.Trim();
            if (isOracleSpatialTab)
            {
                tabName += "_tem";
            }
            try
            {
                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "deleteFeature-设置可添加");
            }
        }

        /// <summary>
        /// 业务数据编辑 删除图元
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void GadeleteFeature()
        {
            try
            {
                editTable.DeleteFeature(ft);
                ft = null;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GadeleteFeature-删除图元");
            }

            //设置可添加
            string tabName = comboTable.Text.Trim();
            if (isOracleSpatialTab)
            {
                tabName += "_tem";
            }
            try
            {
                if (temEditDt.Rows[0]["业务数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GadeleteFeature-设置可添加");
            }
        }

        private MIConn dbMI = new MIConn();
        /// <summary>
        /// 图元的删除或移动
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Feature_Changed(object sender, MapInfo.Tools.FeatureChangedEventArgs e)
        {
            if (!this.Visible) return; // 不是当前模块返回

            if ((this.buttonCancel.Enabled == true && this.buttonCancel.Text == "取消添加") || (this.btnGaCancel.Enabled == true && this.btnGaCancel.Text == "取消添加"))
            {
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                string sql = "";
                if (e.FeatureChangeMode == FeatureChangeMode.Delete)  //删除要素
                {
                    if (this.tabControl1.SelectedTab == tabPage1 && temEditDt.Rows[0]["└基础数据可删、改"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("您没有删除权限！");
                        return;
                    }
                    if (this.tabControl1.SelectedTab == tabPage3 && temEditDt.Rows[0]["└业务数据可删、改"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("您没有删除权限！");
                        return;
                    }

                    //找到选中对象所在乡镇
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(ft.Geometry, IntersectType.Geometry);
                    si.QueryDefinition.Columns = null;
                    Feature ft1 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", si);

                    //为空时,表明对象在区域外,可删除
                    if (ft1 != null)
                    {
                        if (strRegion != "顺德区")
                        {
                            //跟街镇面相交,看看所在街镇面是不是用户范围,如果不是不能删除.
                            if (Array.IndexOf(strRegion.Split(','), ft1["name"].ToString()) == -1)
                            {
                                editTable.InsertFeature(ft);
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("不能删除权限范围外的对象.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                    }

                    if (isOracleSpatialTab == false)
                    {
                        sql = "delete from " + this.editTable.Alias + " where   to_char(MapID) ='" + selMapID + "'";

                        Conn.Open();
                        OracleCommand cmd = new OracleCommand(sql, Conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        Conn.Close();
                        if (tabControl1.SelectedTab == tabPage1)
                        {
                            if (temEditDt.Rows[0]["└基础数据可删、改"].ToString() == "1")
                            {
                                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                            }
                            MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                        }
                        if (tabControl1.SelectedTab == tabPage3)
                        {
                            if (temEditDt.Rows[0]["└业务数据可删、改"].ToString() == "1")
                            {
                                MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);
                            }
                            MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);
                        }
                    }
                    else
                    {
                        if (tabControl1.SelectedTab == tabPage3 && comboTable.Text.Trim() == "人口系统")
                        {
                            try
                            {
                                int iID = 0;
                                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                                {
                                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString().Trim() == "身份证号码")
                                    {
                                        iID = i;
                                        break;
                                    }
                                }
                                sql = "delete from 人口照片 where 身份证号码='" + dataGridViewGaInfo.Rows[iID].Cells[1].Value.ToString() + "'";
                                if (Conn.State == ConnectionState.Closed)
                                {
                                    Conn.Open();
                                }
                                OracleCommand cmd = new OracleCommand(sql, Conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                Conn.Close();

                                this.dataGridViewGaInfo.Visible = false;
                                this.btnGaSave.Enabled = false;
                                this.btnGaSave.Text = "保存";
                                this.btnGaCancel.Enabled = false;
                            }
                            catch (Exception ex)
                            {
                                writeToLog(ex, "Feature_Changed");
                                if (Conn.State == ConnectionState.Open)
                                {
                                    Conn.Close();
                                }
                            }
                        }

                        MIConnection miConn = new MIConnection();
                        try
                        {
                            miConn.Open();
                            MICommand miCmd = miConn.CreateCommand();
                            miCmd.CommandText = "delete from " + lTable.Alias + " where MI_PRINX=" + selPrinx;
                            miCmd.ExecuteNonQuery();
                            miCmd.Dispose();
                            miConn.Close();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Changed");
                            if (miConn.State == ConnectionState.Open)
                                miConn.Close();
                        }
                    }
                    if (tabControl1.SelectedTab == tabPage1)
                    {
                        dataGridView1.Visible = false;
                        buttonSave.Enabled = false;
                        buttonSave.Text = "保存";
                        buttonCancel.Enabled = false;

                        reMoveListValue(ft["keyID"].ToString());
                        setDataGridBG();

                        WriteEditLog(comboTables.Text.Trim(), selMapID, "删除");
                    }
                    else if (tabControl1.SelectedTab == tabPage3)
                    {
                        this.dataGridViewGaInfo.Visible = false;
                        this.btnGaSave.Enabled = false;
                        this.btnGaSave.Text = "保存";
                        this.btnGaCancel.Enabled = false;

                        GareMoveListValue(ft["keyID"].ToString());
                        GasetDataGridBG();

                        WriteEditLog(comboTable.Text.Trim(), selMapID, "删除");
                    }
                }
                else if (e.FeatureChangeMode == FeatureChangeMode.Move)  // 移动要素
                {
                    if (this.tabControl1.SelectedTab == tabPage1 && temEditDt.Rows[0]["基础数据可编辑"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("您没有移动权限！");
                        return;
                    }
                    if (this.tabControl1.SelectedTab == tabPage3 && temEditDt.Rows[0]["业务数据可编辑"].ToString() != "1")
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("您没有移动权限！");
                        return;
                    }
                    //add by fisher in 09-12-10
                    if (tabControl1.SelectedTab == tabPage2)
                    {
                        OracleConnection conn = new OracleConnection(mysqlstr);
                        try
                        {
                            conn.Open();
                            //找到移动后的要素
                            SearchInfo sio = MapInfo.Data.SearchInfoFactory.SearchWhere("keyID = '" + selMapID + "'");
                            sio.QueryDefinition.Columns = null;
                            Feature ff = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, sio);

                            //查找移动后的要素所在的乡镇
                            sio = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(ff.Geometry, IntersectType.Geometry);
                            sio.QueryDefinition.Columns = null;
                            Feature vft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", sio);

                            if (vft == null)  //不能将对象移动到区域范围外
                            {
                                editTable.DeleteFeature(ff);
                                editTable.InsertFeature(ft);
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("不能将对象移动到权限范围外.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            else
                            {
                                bool quyuCZ = false;  // 用于判断移动的点是不是在用于允许的权限范围内
                                if (strRegion != "顺德区")
                                {
                                    //跟街镇面相交,看看所在街镇面是不是用户范围
                                    if (Array.IndexOf(this.strRegion.Split(','), vft["name"].ToString()) > -1)
                                    {
                                        quyuCZ = true;
                                    }
                                    if (quyuCZ == false)
                                    {
                                        editTable.DeleteFeature(ff);
                                        editTable.InsertFeature(ft);
                                        this.Cursor = Cursors.Default;
                                        MessageBox.Show("不能将对象移动到权限范围外", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return;
                                    }
                                }
                            }

                            string vmoveStr = "update 视频位置 set X = " + ff.Geometry.Centroid.x + ",Y = " + ff.Geometry.Centroid.y + " where 设备编号 = '" + selMapID + "'";
                            int s = 0;
                            // 找到你选择的点所在dataGridViewVideo中的哪一行
                            for (int i = 0; i < this.dataGridViewVideo.Rows.Count; i++)
                            {
                                if (this.dataGridViewVideo.Rows[i].Cells["设备编号"].Value.ToString() == selMapID)
                                {
                                    s = i;
                                    break;
                                }
                            }
                            // 获得未拖动之前的坐标
                            double xSize = Convert.ToDouble(this.dataGridViewVideo.Rows[s].Cells["X"].Value);
                            double ySize = Convert.ToDouble(this.dataGridViewVideo.Rows[s].Cells["Y"].Value);
                            // 比较是否有拖动
                            if (xSize == ff.Geometry.Centroid.x && ySize == ff.Geometry.Centroid.y)
                            {
                                return;
                            }
                            // 如果有则改变dataGridViewVideo中的值
                            this.dataGridViewVideo.Rows[s].Cells["X"].Value = ff.Geometry.Centroid.x;
                            this.dataGridViewVideo.Rows[s].Cells["Y"].Value = ff.Geometry.Centroid.y;

                            // 然后更新数据库中有值
                            OracleCommand cmd = conn.CreateCommand();
                            cmd.CommandText = vmoveStr;
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            conn.Close();
                            this.Cursor = Cursors.Default;
                            MessageBox.Show("坐标移动成功,已保存!", "提示");
                            return;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "移动要素");
                            writeToLog(ex, "Feature_Changed");
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }

                    //找到移动后的要素
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("MapID=" + selPrinx);
                    si.QueryDefinition.Columns = null;
                    Feature f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                    Feature fuwei = ft;   //定义一个用来复位的feature,记忆最先移动的位置  add by fisher in 09-12-30
                    ft = f;

                    //找到添加对象所在乡镇
                    si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                    si.QueryDefinition.Columns = null;
                    Feature ft2 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("街镇面", si);

                    //找到移动对象所在中队辖区  (以下代码由fisher添加  09-12-1)
                    si = MapInfo.Data.SearchInfoFactory.SearchIntersectsGeometry(f.Geometry, IntersectType.Geometry);
                    si.QueryDefinition.Columns = null;
                    Feature ft3 = MapInfo.Engine.Session.Current.Catalog.SearchForFeature("中队辖区", si);
                    string zdmc = "";  //中队名称

                    if (ft3 != null)
                    {
                        string zddm = ft3["中队代码"].ToString();
                        OracleConnection conn = new OracleConnection(mysqlstr);
                        try
                        {
                            conn.Open();
                            OracleCommand cmd = new OracleCommand("select 中队名 from 基层民警中队 where 中队代码 = " + zddm, conn);
                            OracleDataReader dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                dr.Read();
                                zdmc = dr.GetValue(0).ToString();
                            }
                            dr.Close();
                            cmd.Dispose();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Changed");
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }

                    //为空时,表明对象在区域外,不能移动
                    if (ft2 == null)
                    {
                        editTable.DeleteFeature(f);
                        editTable.InsertFeature(fuwei);
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("不能将对象移动到权限范围外.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        bool quyuCZ = false;  //用于判断移动的点是不是在用于允许的权限范围内
                        if (strRegion != "" || strRegion1 != "")
                        {
                            if (strRegion != "顺德区")
                            {
                                if (strRegion != "")
                                {
                                    //跟街镇面相交,看看所在街镇面是不是用户范围
                                    if (Array.IndexOf(strRegion.Split(','), ft2["name"].ToString()) > -1)
                                    {
                                        quyuCZ = true;
                                    }
                                }
                                if (strRegion1 != "")
                                {
                                    if (Array.IndexOf(strRegion1.Split(','), zdmc) > -1)
                                    {
                                        quyuCZ = true;
                                    }
                                }
                                if (quyuCZ == false)
                                {
                                    editTable.DeleteFeature(f);
                                    editTable.InsertFeature(fuwei);
                                    this.Cursor = Cursors.Default;
                                    MessageBox.Show("不能将对象移动到权限范围外.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                        }
                    }

                    if (isOracleSpatialTab)
                    {
                        //找到oracleSpatial表中对应的要素
                        si = MapInfo.Data.SearchInfoFactory.SearchWhere("MI_PRINX=" + selPrinx);
                        si.QueryDefinition.Columns = null;
                        Feature newFeat = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(lTable.Alias, si);

                        //新建对象,为原要素的属性+移动后的geometry
                        Feature addFeat = newFeat;
                        addFeat.Geometry = f.Geometry;

                        //以下代码由fisher添加 （09-09-04）
                        if (this.tabControl1.SelectedTab == tabPage1)
                        {
                            double xSize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value);
                            if (xSize == addFeat.Geometry.GeometricCentroid.x && ySize == addFeat.Geometry.GeometricCentroid.y)
                            {
                                return;
                            }
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = addFeat.Geometry.GeometricCentroid.x;
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = addFeat.Geometry.GeometricCentroid.y;
                        }
                        if (this.tabControl1.SelectedTab == tabPage3)
                        {
                            double xSize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value);
                            if (xSize == addFeat.Geometry.GeometricCentroid.x && ySize == addFeat.Geometry.GeometricCentroid.y)
                            {
                                return;
                            }
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = addFeat.Geometry.GeometricCentroid.x;
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = addFeat.Geometry.GeometricCentroid.y;
                        }

                        //先删除原来的要素,再添加新位置的要素
                        lTable.DeleteFeature(newFeat);
                        lTable.InsertFeature(addFeat);

                    }
                    else
                    {
                        //更新数据库中对应要素的坐标  
                        //以下代码由fisher添加 （09-09-04）
                        if (this.tabControl1.SelectedTab == tabPage1)
                        {
                            double xSize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value);
                            if (xSize == f.Geometry.Centroid.x && ySize == f.Geometry.Centroid.y)
                            {
                                return;
                            }
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[1].Value = f.Geometry.Centroid.x;
                            this.dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = f.Geometry.Centroid.y;
                        }
                        if (this.tabControl1.SelectedTab == tabPage3)
                        {
                            double xSize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value);
                            double ySize = Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value);
                            if (xSize == f.Geometry.Centroid.x && ySize == f.Geometry.Centroid.y)
                            {
                                return;
                            }
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value = f.Geometry.Centroid.x;
                            this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value = f.Geometry.Centroid.y;
                        }
                    }

                    //以下代码由fisher添加，移动时直接更新（09-12-31）
                    if (tabControl1.SelectedTab == tabPage3)   //公安业务数据
                    {
                        MoveUpData(dataGridViewGaList, dataGridViewGaInfo, comboTable.Text, rowIndex);
                    }
                    else if (tabControl1.SelectedTab == tabPage1) //基础数据编辑
                    {
                        MoveUpData(dataGridViewList, dataGridView1, comboTables.Text, browIndex);
                    }
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;
                    WriteEditLog(comboTables.Text.Trim(), selMapID, "移动");
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                writeToLog(ex, "Feature_Changed");
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 用于在移动后直接更新移动要素
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dGVList">显示数据列表</param>
        /// <param name="dGVInfo">编辑数据列表</param>
        /// <param name="tableName">表名</param>
        /// <param name="Rowid">行号</param>
        private void MoveUpData(DataGridView dGVList, DataGridView dGVInfo, string tableName, int Rowid)
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            OracleCommand cmd = null;
            try
            {
                conn.Open();
                string command = "update " + tableName + " t set ";
                string strValue = "";
                for (int i = 0; i < dGVInfo.RowCount; i++)
                {
                    strValue = "";
                    if (dGVInfo.Rows[i].Cells[0].Value.ToString() == "X")
                    {
                        strValue = dGVInfo.Rows[i].Cells[1].Value.ToString();
                        if (isOracleSpatialTab)
                        {
                            command += "t.GEOLOC.SDO_POINT.X = '" + strValue + "',";
                        }
                        else
                        {
                            command += "t." + dGVInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                        }
                    }
                    else if (dGVInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                    {
                        strValue = dGVInfo.Rows[i].Cells[1].Value.ToString();
                        if (isOracleSpatialTab)
                        {
                            command += "t.GEOLOC.SDO_POINT.Y = '" + strValue + "',";
                        }
                        else
                        {
                            command += "t." + dGVInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                        }
                    }
                }
                command = command.Remove(command.Length - 1);
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                command += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + dGVList.Rows[Rowid].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                cmd = new OracleCommand(command, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
                MessageBox.Show("坐标移动成功,已保存!", "提示");
            }
            catch (Exception ex)
            {
                WriteEditLog(comboTables.Text.Trim(), selMapID, "移动要素更新");
                MessageBox.Show(ex.Message, "移动要素更新");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 基础数据编辑 移除列表中的值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="keyID"></param>
        private void reMoveListValue(string keyID)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                for (int i = 0; i < dataGridViewList.Rows.Count; i++)
                {
                    if (dataGridViewList.Rows[i].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() == keyID)
                    {
                        dataGridViewList.Rows.RemoveAt(i);
                        return;
                    }
                }
                setDataGridBG();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "reMoveListValue");
            }
        }

        /// <summary>
        /// 业务数据编辑 移除列表中的值
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="keyID"></param>
        private void GareMoveListValue(string keyID)
        {
            try
            {
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (dataGridViewGaList.Rows[i].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() == keyID)
                    {
                        dataGridViewGaList.Rows.RemoveAt(i);
                        return;
                    }
                }
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GareMoveListValue");
            }
        }

        string selMapID = "";
        int selPrinx = -1;
        /// <summary>
        /// 选择图元事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void Feature_Selected(object sender, FeatureSelectedEventArgs e)
        {
            if (!this.Visible) return; // 不是当前模块返回

            if (tabControl1.SelectedTab == tabPage2)  // add by fisher in 09-12-10
            {
                try
                {
                    IResultSetFeatureCollection fc = e.Selection[this.editTable];
                    if (fc == null || fc.Count == 0)
                    {
                        selMapID = "";
                        return;
                    }
                    ft = fc[0];
                    selMapID = ft["keyID"].ToString();
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                }
            }

            if (tabControl1.SelectedTab == tabPage3)   //公安业务编辑模块  by feng 09-08-21
            {
                if (this.btnGaCancel.Enabled == true && this.btnGaCancel.Text == "取消添加")
                {
                    MessageBox.Show("请完成上一条记录的属性添加和保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Session.Current.Selections.DefaultSelection.Clear();
                    return;
                }

                try
                {
                    IResultSetFeatureCollection fc = e.Selection[this.editTable];
                    if (fc == null || fc.Count == 0)
                    {
                        GaNodesEdit.Enabled = false;
                        GaAddNode.Enabled = false;
                        dataGridViewGaInfo.Visible = false;
                        btnGaSave.Enabled = false;
                        selPrinx = -1;
                        selMapID = "";
                        return;
                    }
                    ft = feature = fc[0];
                    selPrinx = Convert.ToInt32(feature["MapID"]);
                    selMapID = feature["keyID"].ToString();
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                    MessageBox.Show(ex.Message, "Feature_Selected000");
                }

                try
                {
                    this.dataGridViewGaInfo.Visible = true;
                    this.btnGaSave.Text = "更新";
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Text = "取消";
                    this.btnGaCancel.Enabled = false;

                    if (isOracleSpatialTab)
                    {
                        MIConnection miConnection = new MIConnection();
                        miConnection.Open();
                        Table changeTable = null;
                        CLC.ForSDGA.GetFromTable.GetFromName(this.comboTable.Text.Trim(), getFromNamePath);
                        string tableName = CLC.ForSDGA.GetFromTable.TableName;
                        if (tableName == null || tableName == "")
                        {
                            tableName = this.comboTable.Text.Trim();
                        }
                        TableInfoServer ti = new TableInfoServer("change" + tableName);  //这里设置ti的别名
                        ti.ConnectString = "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password;
                        ti.Query = "Select * From  " + tableName + "  where MI_PRINX = " + selPrinx + " ";
                        ti.Toolkit = ServerToolkit.Oci;
                        ti.CacheSettings.CacheType = CacheOption.Off;

                        changeTable = miConnection.Catalog.OpenTable(ti);
                        miConnection.Close();

                        MICommand miCommand = miConnection.CreateCommand();
                        MIDataReader miReader = null;

                        try
                        {
                            miConnection.Open();
                            miCommand.CommandText = "select * from  " + changeTable.Alias + " ";
                            miReader = miCommand.ExecuteReader();
                            int i = 0;
                            int iFieldsCount = this.dataGridViewGaInfo.Rows.Count;

                            if (miReader.Read())
                            {
                                for (i = 0; i < iFieldsCount; i++)
                                {
                                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "照片")
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                                        fileName = "";  // 照片路径 
                                    }
                                    else if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = e.MapCoordinate.x;
                                    }
                                    else if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = e.MapCoordinate.y;
                                    }
                                    else
                                    {
                                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = miReader.GetValue(this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()).ToString();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                        finally
                        {
                            if (miReader != null)
                            {
                                miReader.Dispose();
                            }
                            if (miCommand != null)
                            {
                                miCommand.Dispose();
                            }
                            if (miConnection != null)
                            {
                                miConnection.Dispose();
                            }
                        }
                        changeTable.Close();
                    }
                    else
                    {
                        try
                        {
                            CLC.ForSDGA.GetFromTable.GetFromName(this.comboTable.Text, getFromNamePath);
                            string tableName = CLC.ForSDGA.GetFromTable.TableName;
                            OracelData linkData = new OracelData(mysqlstr);
                            DataTable dt = linkData.SelectDataBase("select * from  " + tableName + "  where 编号 = '" + selMapID + "'");
                            for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                            {
                                this.dataGridViewGaInfo.Rows[i].Cells[1].Value = dt.Rows[0][this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()];
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                    }

                    //add by siumo 208-12-31:如果是只有添加功能的用户,选择要素后只能查看信息,不能修改,设置datagridview不可编辑
                    if (temEditDt.Rows[0]["业务数据可编辑"].ToString() != "1")
                    {
                        dataGridViewGaInfo.Columns[1].ReadOnly = true;
                    }
                    else
                    {
                        dataGridViewGaInfo.Columns[1].ReadOnly = false;
                    }

                    tabControl3.SelectedTab = tabGaInfo;

                    //以下代码由fisher添加，旨在通过当前选中的feature去寻找dataGridViewGaList中与之相对应的行  (09-09-04)
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    string fobjId = CLC.ForSDGA.GetFromTable.ObjID;
                    if (isOracleSpatialTab)
                    {
                        for (int i = 0; i < this.dataGridViewGaList.Rows.Count; i++)
                        {
                            if (this.dataGridViewGaList.Rows[i].Cells["MapID"].Value.ToString() == selMapID)
                            {
                                rowIndex = i;
                                this.dataGridViewGaList.CurrentCell = this.dataGridViewGaList.Rows[rowIndex].Cells[fobjId];
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.dataGridViewGaList.Rows.Count; i++)
                        {
                            if (this.dataGridViewGaList.Rows[i].Cells["编号"].Value.ToString() == selMapID)
                            {
                                rowIndex = i;
                                this.dataGridViewGaList.CurrentCell = this.dataGridViewGaList.Rows[rowIndex].Cells[fobjId];
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                }

                this.btnGaCancel.Enabled = false;
                this.btnGaSave.Enabled = false;
            }
            //................................................

            if (tabControl1.SelectedTab == tabPage1)
            {
                if (this.buttonCancel.Enabled == true && this.buttonCancel.Text == "取消添加")
                {
                    MessageBox.Show("请完成上一条记录的属性添加和保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Session.Current.Selections.DefaultSelection.Clear();
                    return;
                }

                try
                {
                    IResultSetFeatureCollection fc = e.Selection[this.editTable];
                    if (fc == null || fc.Count == 0)
                    {
                        toolBarNodesEdit.Enabled = false;
                        toolBarAddNode.Enabled = false;
                        dataGridView1.Visible = false;
                        buttonSave.Enabled = false;
                        selPrinx = -1;
                        selMapID = "";
                        return;
                    }
                    toolBarNodesEdit.Enabled = true;
                    ft = feature = fc[0];

                    selPrinx = Convert.ToInt32(feature["MapID"]);
                    selMapID = feature["keyID"].ToString();
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                }

                try
                {
                    this.dataGridView1.Visible = true;
                    this.buttonSave.Text = "更新";
                    this.buttonCancel.Text = "取消";
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;

                    if (isOracleSpatialTab)
                    {
                        MIConnection miConnection = new MIConnection();
                        miConnection.Open();
                        Table changeTable = null;
                        CLC.ForSDGA.GetFromTable.GetFromName(this.comboTables.Text.Trim(), getFromNamePath);
                        string tableName = CLC.ForSDGA.GetFromTable.TableName;
                        if (tableName == null || tableName == "")
                        {
                            tableName = this.comboTables.Text.Trim();
                        }
                        TableInfoServer ti = new TableInfoServer("change" + tableName);
                        ti.ConnectString = "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password;
                        ti.Query = "Select * From  " + tableName + "  where MI_PRINX = " + selPrinx + " ";
                        ti.Toolkit = ServerToolkit.Oci;
                        ti.CacheSettings.CacheType = CacheOption.Off;

                        changeTable = miConnection.Catalog.OpenTable(ti);
                        miConnection.Close();

                        MICommand miCommand = miConnection.CreateCommand();
                        MIDataReader miReader = null;

                        try
                        {
                            miConnection.Open();
                            miCommand.CommandText = "select * from  " + changeTable.Alias + " ";
                            miReader = miCommand.ExecuteReader();
                            int i = 0;
                            int iFieldsCount = this.dataGridView1.Rows.Count;

                            if (miReader.Read())
                            {
                                for (i = 0; i < iFieldsCount; i++)
                                {
                                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        this.dataGridView1.Rows[i].Cells[1].Value = e.MapCoordinate.x;
                                    }
                                    else if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        this.dataGridView1.Rows[i].Cells[1].Value = e.MapCoordinate.y;
                                    }
                                    else
                                    {
                                        this.dataGridView1.Rows[i].Cells[1].Value = miReader.GetValue(this.dataGridView1.Rows[i].Cells[0].Value.ToString()).ToString();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                        finally
                        {
                            if (miReader != null)
                            {
                                miReader.Dispose();
                            }
                            if (miCommand != null)
                            {
                                miCommand.Dispose();
                            }
                            if (miConnection != null)
                            {
                                miConnection.Dispose();
                            }
                        }
                        changeTable.Close();
                    }
                    else
                    {
                        try
                        {
                            CLC.ForSDGA.GetFromTable.GetFromName(this.comboTables.Text, getFromNamePath);
                            string tableName = CLC.ForSDGA.GetFromTable.TableName;
                            OracelData linkData = new OracelData(mysqlstr);
                            DataTable dt = linkData.SelectDataBase("select * from  " + tableName + "  where 编号 = '" + selMapID + "'");
                            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                            {
                                this.dataGridView1.Rows[i].Cells[1].Value = dt.Rows[0][this.dataGridView1.Rows[i].Cells[0].Value.ToString()];
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "Feature_Selected");
                        }
                    }

                    //add by siumo 208-12-31:如果是只有添加功能的用户,选择要素后只能查看信息,不能修改,设置datagridview不可编辑
                    if (temEditDt.Rows[0]["基础数据可编辑"].ToString() != "1")
                    {
                        dataGridView1.Columns[1].ReadOnly = true;
                    }
                    else
                    {
                        dataGridView1.Columns[1].ReadOnly = false;
                    }

                    tabControl2.SelectedTab = tabPageInfo;
                    //以下代码由fisher添加，旨在通过当前选中的feature去寻找dataGridViewList中与之相对应的行  (09-09-04)
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                    string bobjID = CLC.ForSDGA.GetFromTable.ObjID;
                    if (isOracleSpatialTab)
                    {
                        for (int i = 0; i < this.dataGridViewList.Rows.Count; i++)
                        {
                            if (this.dataGridViewList.Rows[i].Cells["MapID"].Value.ToString() == selMapID)
                            {
                                browIndex = i;
                                this.dataGridViewList.CurrentCell = this.dataGridViewList.Rows[browIndex].Cells[bobjID];
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.dataGridViewList.Rows.Count; i++)
                        {
                            if (this.dataGridViewList.Rows[i].Cells["编号"].Value.ToString() == selMapID)
                            {
                                browIndex = i;
                                this.dataGridViewList.CurrentCell = this.dataGridViewList.Rows[browIndex].Cells[bobjID];
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "Feature_Selected");
                }
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;
            }
        }

        /// <summary>
        /// 添加语句，用来判断输入的格式正不正确
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string type = "";//得到类型
            try
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "身份证号码")
                {
                    if (dataGridView1.Rows[e.RowIndex].Cells[1].Value != null)
                    {
                        CheckIdentity(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                    }
                }
                else
                {
                    if (this.dataGridView1.CurrentCell == null || this.dataGridView1.CurrentCell.Value == null)
                    {
                        return;
                    }

                    string value = this.dataGridView1.CurrentCell.Value.ToString().Trim();
                    type = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value.ToString();
                    switch (type)
                    {
                        case "NUMBER":
                        case "INTEGER":
                        case "LONG":
                            this.checkNumber(value);
                            break;
                        case "FLOAT":
                        case "DOUBLE":
                            this.checkFloat(value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridView1_CellEndEdit");
            }
        }

        /// <summary>
        /// 基础数据编辑  检查在编辑列表中输入的身份证号是否符合要求
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="strID">身份证号码</param>
        private void CheckIdentity(string strID)
        {
            try
            {
                if (strID.Length != 15 && strID.Length != 18)
                {
                    System.Windows.Forms.MessageBox.Show("请确认您输入的身份证号码为15位或18位!");
                    this.dataGridView1.CurrentCell.Value = string.Empty;
                    return;
                }
                string str = strID.Substring(0, strID.Length - 1);
                if (!System.Text.RegularExpressions.Regex.IsMatch(strID.Trim(), @"^\d+(\d*)?$"))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))
                    {
                        System.Windows.Forms.MessageBox.Show("请确认您输入的身份证号码格式正确!");
                        this.dataGridView1.CurrentCell.Value = string.Empty;
                    }
                    if (strID.Substring(strID.Length - 1, 1).ToUpper() != "X")
                    {
                        System.Windows.Forms.MessageBox.Show("请确认您输入的身份证号码格式正确!");
                        this.dataGridView1.CurrentCell.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "CheckIdentity");
            }
        }

        /// <summary>
        /// 业务数据编辑 检查编辑列表中输入的身份证号是否符合要求
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="strID">身份证号码</param>
        private void GaCheckIdentity(string strID)
        {
            try
            {
                if (strID.Length != 15 && strID.Length != 18)
                {
                    System.Windows.Forms.MessageBox.Show("请确认您输入的身份证号码为15位或18位!");
                    this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                    return;
                }
                string str = strID.Substring(0, strID.Length - 1);
                if (!System.Text.RegularExpressions.Regex.IsMatch(strID.Trim(), @"^\d+(\d*)?$"))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))
                    {
                        System.Windows.Forms.MessageBox.Show("请确认您输入的身份证号码格式正确!");
                        this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                    }
                    if (strID.Substring(strID.Length - 1, 1).ToUpper() != "X")
                    {
                        System.Windows.Forms.MessageBox.Show("请确认您输入的身份证号码格式正确!");
                        this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaCheckIdentity");
            }
        }

        private bool hasUpdate = false;
        /// <summary>
        /// 
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // add by fisher in 09-12-29
            this.buttonSave.Enabled = true;
            this.buttonCancel.Enabled = true;

            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "照片")
            {
                System.Data.OracleClient.OracleConnection myConnection = new System.Data.OracleClient.OracleConnection(mysqlstr);
                System.Data.OracleClient.OracleCommand oracleCmd = null;
                System.Data.OracleClient.OracleDataReader dataReader = null;

                FrmImage fimage = new FrmImage();
                if (buttonCancel.Enabled)
                {
                    if (fimage.ShowDialog(this) == DialogResult.OK)
                    {
                        fileName = fimage.openFileDialog.FileName;
                        dataGridView1.Rows[e.RowIndex].Cells[1].Value = fileName;
                        hasUpdate = fimage.hasUpdate;
                    }
                }
                else
                {
                    try
                    {
                        myConnection.Open();
                        int iID = 0;
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            if (this.dataGridView1.Rows[i].Cells[0].Value.ToString().Trim() == "身份证号码")
                            {
                                iID = i;
                                break;
                            }
                        }
                        string sqlstr = "select 照片 from  人口照片 where 身份证号码='" + this.dataGridView1.Rows[iID].Cells[1].Value.ToString() + "'  ";
                        oracleCmd = new OracleCommand(sqlstr, myConnection);
                        dataReader = oracleCmd.ExecuteReader();

                        try
                        {
                            if (dataReader.Read())
                            {
                                byte[] bytes = new byte[2000000];
                                long reallyLong = dataReader.GetBytes(0, 0, bytes, 0, 2000000);
                                Stream fs = new MemoryStream(bytes);
                                fimage.pictureBox1.Image = Image.FromStream(fs);
                                fimage.pictureBox1.Invalidate();
                                fs.Close();
                            }
                            dataReader.Close();
                        }
                        catch (Exception ex) { writeToLog(ex, "dataGridView1_CellBeginEdit"); }
                        if (fimage.ShowDialog(this) == DialogResult.OK)
                        {
                            fileName = fimage.openFileDialog.FileName;
                            dataGridView1.Rows[e.RowIndex].Cells[1].Value = fileName;
                            hasUpdate = fimage.hasUpdate;
                        }
                    }

                    catch (Exception ex)
                    {
                        writeToLog(ex, "dataGridView1_CellBeginEdit");
                    }
                    finally
                    {
                        if (oracleCmd != null)
                            oracleCmd.Dispose();
                        if (myConnection.State == ConnectionState.Open)
                            myConnection.Close();
                    }
                }
                try
                {
                    dataGridView1.CurrentCell = null;
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "dataGridView1_CellBeginEdit");
                }
            }
            else
            {
                string type = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value.ToString();

                try
                {
                    if (type == "DATE")
                    {
                        FrmMonthCalendar fCalendar = new FrmMonthCalendar();
                        if (fCalendar.ShowDialog() == DialogResult.OK)
                        {
                            dataGridView1.CurrentCell.Value = fCalendar.dateString;
                        }
                        dataGridView1.CurrentCell = null;
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "dataGridView1_CellBeginEdit");
                }
            }
        }

        /// <summary>
        /// 基础数据编辑 判断输入的是不是数字(不能带小数)
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        private void checkNumber(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$")) // 正则表达式判断输入的是不是数字
                {

                    MessageBox.Show("输入数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridView1.CurrentCell.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "checkNumber");
            }
        }

        /// <summary>
        /// 业务数据编辑 判断输入的是不是数字(不能带小数)
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        private void GacheckNumber(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\d*)?$"))//判断输入的是不是数字
                {

                    MessageBox.Show("请输入数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GacheckNumber");
            }
        }

        /// <summary>
        /// 基础数据编辑 判断输入的是不是数字(可能带小数)
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        private void checkFloat(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\.\d*)?$"))//判断输入的是不是flaot
                {
                    MessageBox.Show("输入数字，可以带小数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridView1.CurrentCell.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "checkFloat");
            }
        }

        /// <summary>
        /// 业务数据编辑 判断输入的是不是数字(可能带小数)
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        private void GacheckFloat(string str)
        {
            try
            {
                str = str.Trim();
                if (str == "")
                {
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Trim(), @"^\d+(\.\d*)?$"))//判断输入的是不是flaot
                {
                    MessageBox.Show("输入数字，可以带小数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dataGridViewGaInfo.CurrentCell.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GacheckFloat");
            }
        }

        /// <summary>
        /// 基础数据编辑 数据表下拉
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void comboTables_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.Visible && buttonCancel.Enabled)
            {
                MessageBox.Show("请完成上一条记录的属性添加和保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 基础数据编辑 查找按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            searchByKey();
        }

        //以下变量用于编辑的分页显示
        int bpageSize = 100;     // 每页显示行数
        int bnMax = 0;           // 总记录数
        int bpageCount = 0;      // 页数＝总记录数/每页显示行数
        int bpageCurrent = 0;    // 当前页号
        int bnCurrent = 0;       // 当前记录行

        string bPageSQL = "";    // 用于获得分页数据
        string blctStr = "";     // 编辑模块下 定位用字符串

        /// <summary>
        /// 基础数据编辑 数据查询
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void searchByKey()
        {
            isShowPro(true);
            dataGridViewList.Columns.Clear();
            if (textKeyWord.Text.Trim() == "")
            {
                isShowPro(false);
                MessageBox.Show("请输入关键词！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);

            string strSQL = "";
            if (isOracleSpatialTab)
            {
                strSQL = "select MI_PRINX as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text + " t";
                bPageSQL = "select MI_PRINX as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text + " t";
                blctStr = "select MI_PRINX as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text + " t";
            }
            else
            {
                strSQL = "select rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text;
                bPageSQL = "select  rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text;
                blctStr = "select  rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + comboTables.Text;
            }

            if (cbMohu.Checked)
            {
                strSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " like '%" + textKeyWord.Text.Trim() + "%'";
                bPageSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " like '%" + textKeyWord.Text.Trim() + "%'";
                blctStr += " where " + CLC.ForSDGA.GetFromTable.ObjName + " like '%" + textKeyWord.Text.Trim() + "%'";
            }
            else
            {
                strSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " ='" + textKeyWord.Text.Trim() + "'";
                bPageSQL += " where " + CLC.ForSDGA.GetFromTable.ObjName + " ='" + textKeyWord.Text.Trim() + "'";
                blctStr += " where " + CLC.ForSDGA.GetFromTable.ObjName + " ='" + textKeyWord.Text.Trim() + "'";
            }

            //alter by siumo 2009-03-10

            if (strRegion != "顺德区" && strRegion != "")
            {
                if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                {
                    strRegion = strRegion.Replace("大良", "大良,德胜");
                }

                strSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                bPageSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                blctStr += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
            }
            if (strRegion1 != "" && comboTables.Text == "社区警务室")
            {
                strSQL += " and " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
                bPageSQL += " and " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
                blctStr += " and " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "')";
            }
            this.toolEditPro.Value = 1;
            Application.DoEvents();
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(strSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                dAdapter.Dispose();
                conn.Close();
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.toolStripRecord.Text = "0条";
                    this.tsTextBoxPageNow.Text = "0";//设置当前页
                    this.tStripLabelPageCount.Text = "/ {0}";//设置总页数
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    return;
                }

                //以下代码用来设置翻页
                try
                {
                    bnMax = dt.Rows.Count;
                    this.toolStripRecord.Text = bnMax.ToString() + "条";
                    bpageSize = 100;      //设置页面行数
                    bpageCount = (bnMax / bpageSize);//计算出总页数
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//设置总页数
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    if (bnMax != 0)
                    {
                        bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//设置当前页
                    }
                    bnCurrent = 0;       //当前记录数从0开始
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "searchByKey()获取分页参数");
                    MessageBox.Show(ex.Message, "searchByKey() 获取分页参数");
                }


                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //获取当前页数据
                dataGridViewList.DataSource = datatable;

                if (dataGridViewList.Columns["MAPID"] != null)
                {
                    dataGridViewList.Columns["MAPID"].Visible = false;
                }

                setDataGridBG();
                this.toolEditPro.Value = 2;

                this.insertQueryIntoTable(dt);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "searchByKey");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            this.toolEditPro.Value = 3;
            Application.DoEvents();
            isShowPro(false);
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// 基础数据编辑 设置列表行的背景,间隔设色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void setDataGridBG()
        {
            try
            {
                for (int i = 0; i < dataGridViewList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setDataGridBG");
            }
        }

        /// <summary>
        /// 业务数据编辑 设置列表行的背景,间隔设色
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void GasetDataGridBG()
        {
            try
            {
                if (dataGridViewGaList.Rows.Count == 0) return;
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (comboTable.Text.Trim() == "安全防护单位")
                    {
                        // 给安全防护单位的文件加上链接
                        DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                        dgvlc.Value = "点击编辑";
                        dgvlc.ToolTipText = "查看安全防护单位文件";
                        dataGridViewGaList.Rows[i].Cells["安全防护单位文件"] = dgvlc;
                    }
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GasetDataGridBG");
            }
        }

        /// <summary>
        /// 将查询到的结果添加到表中
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dataTable">数据源</param>
        private void insertQueryIntoTable(DataTable dataTable)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("没有相关对象，请重设查询关键词！");
                    (this.editTable as IFeatureCollection).Clear();
                    this.editTable.Pack(PackType.All);
                    return;
                }
                (this.editTable as IFeatureCollection).Clear();
                this.editTable.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "insertQueryIntoTable");
                (this.editTable as IFeatureCollection).Clear();
                this.editTable.Pack(PackType.All);
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                double dX = 0, dY = 0;
                try
                {
                    dX = Convert.ToDouble(dataTable.Rows[i]["X"]);
                    dY = Convert.ToDouble(dataTable.Rows[i]["Y"]);

                    if (dX > 0 && dY > 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(this.editTable)).CoordSys, dX, dY);
                        string tabName = comboTables.Text;
                        if (tabControl1.SelectedTab == tabPage2)
                        {
                            tabName = "视频";
                        }
                        CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                        string sName = CLC.ForSDGA.GetFromTable.ObjName;
                        string strID = CLC.ForSDGA.GetFromTable.ObjID;

                        Feature pFeat = new Feature(editTable.TableInfo.Columns);

                        pFeat.Geometry = pt;
                        pFeat.Style = featStyle;
                        if (dataTable.Columns.Contains("MapID"))
                        {
                            pFeat["MapID"] = dataTable.Rows[i]["MapID"].ToString();
                        }

                        pFeat["keyID"] = dataTable.Rows[i][strID].ToString();
                        pFeat["Name"] = dataTable.Rows[i][sName].ToString();
                        editTable.InsertFeature(pFeat);
                    }
                }
                catch
                {//如果数据有问题,跳过此记录
                    continue;
                }
            }
        }

        /// <summary>
        /// 基础数据编辑 mapToolBar控件点击事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void mapToolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button == mapToolAddPoint || e.Button == mapToolAddpolygon)
                {
                    this.UncheckedTool();
                }
                else if (e.Button == toolBarNodesEdit)
                {
                    if (e.Button.Pushed)
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.Nodes;
                        mapControl1.Tools.LeftButtonTool = "Select";
                        this.UncheckedTool();
                        //toolSel.Checked = true;
                        mapToolBar1.Buttons[4].Enabled = true;
                    }
                    else
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.Objects;
                        mapToolBar1.Buttons[4].Pushed = false;
                        mapToolBar1.Buttons[4].Enabled = false;
                    }
                }
                if (e.Button == toolBarAddNode)
                {
                    if (e.Button.Pushed)
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.AddNode;
                    }
                    else
                    {
                        mapControl1.Tools.SelectMapToolProperties.EditMode = EditMode.Nodes;
                    }
                }
                if (e.Button == toolBarPointStyle)
                {
                    SymbolStyleDlg pStyleDlg = new SymbolStyleDlg();
                    if (pStyleDlg.ShowDialog() == DialogResult.OK)
                    {
                        featStyle = pStyleDlg.SymbolStyle;
                    }
                }
                if (e.Button == toolBarPolygonStyle)
                {
                    AreaStyleDlg aStyleDlg = new AreaStyleDlg();
                    if (aStyleDlg.ShowDialog() == DialogResult.OK)
                    {
                        aStyle = aStyleDlg.AreaStyle;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar1_ButtonClick");
            }
        }

        /// <summary>
        /// 关闭时，如果有对象未保存，提示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void frmMap_FormClosing(object sender, FormClosingEventArgs e) 
        {
            try
            {
                if (buttonCancel.Visible && buttonCancel.Enabled)
                {
                    if (MessageBox.Show("有对象未保存，是否仍要关闭!", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        this.deleteFeature();
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar1_ButtonClick");
            }
        }

        /// <summary>
        /// 按键控制地图
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void mapControl1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!this.Visible) return; // 不是当前模块返回

                DPoint dp;
                switch (e.KeyCode.ToString())
                {
                    case "Up":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y + mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Down":
                        dp = new DPoint(mapControl1.Map.Center.x, mapControl1.Map.Center.y - mapControl1.Map.Bounds.Height() / 4);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Left":
                        dp = new DPoint(mapControl1.Map.Center.x - mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Right":
                        dp = new DPoint(mapControl1.Map.Center.x + mapControl1.Map.Bounds.Width() / 4, mapControl1.Map.Center.y);
                        this.mapControl1.Map.SetView(dp, mapControl1.Map.GetDisplayCoordSys(), mapControl1.Map.Zoom);
                        break;
                    case "Oemplus":
                    case "Add":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale / 2f;
                        break;
                    case "OemMinus":
                    case "Subtract":
                        this.mapControl1.Map.Scale = mapControl1.Map.Scale * 2f;
                        break;
                    case "Delete":
                        if (buttonCancel.Enabled)
                        {
                            e.Handled = false;
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], false);
                            MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], false);
                        }
                        break;
                    default:
                        System.Console.WriteLine(e.KeyCode.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapControl1_KeyDown");
            }
        }

        /// <summary>
        /// 设置图层是否可编辑
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="flag">布尔值(true-可编辑 false-不可编辑)</param>
        private void setTabInsertable(string tableName, bool flag)
        {
            try
            {
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tableName], flag);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setTabInsertable");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFuns">函数名</param>
        private void writeToLog(Exception ex, string sFuns)
        {
            CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-frmMap-" + sFuns);
        }

        /// <summary>
        /// 基础数据编辑 编辑列表改变大小事件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            try
            {
                DataGridView dataGridView = (DataGridView)sender;
                if (dataGridView.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView);
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 135;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridView1_Resize");
            }
        }

        /// <summary>
        /// 如果记录总高度大于容器高度,会出现滚动条,名称列的宽度要自定
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dataGridView">要设置的列表控件</param>
        private void setDataGridViewColumnWidth(DataGridView dataGridView)
        {
            try
            {
                if (dataGridView.Rows[0].Height * dataGridView.Rows.Count + 40 > dataGridView.Height)
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 150;
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 135;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setDataGridViewColumnWidth");
            }
        }

        /// <summary>
        /// 数据出错 返回
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            return;
        }

        private Feature flashFt;     // 全局图元
        private Style defaultStyle;  // 默认样式
        int k = 0;                   // 用于闪烁次数的变量
        int browIndex = 0;           // 用于获取当前选中的行号
        /// <summary>
        /// 点击单元格，查找对应的要素，变换要素的样式，实现闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void dataGridViewList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //点击一个记录，进行地图定位
                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                {
                    btnLoc1.Enabled = true;  //准备编辑点的坐标
                }
                if (e.RowIndex > -1)
                {
                    //this.toolLoc.Enabled = true;
                    if (dataGridViewList["X", e.RowIndex].Value == null || dataGridViewList["Y", e.RowIndex].Value == null 
                                                                        || dataGridViewList["X", e.RowIndex].Value.ToString() == "" 
                                                                        || dataGridViewList["Y", e.RowIndex].Value.ToString() == "")
                    {
                        System.Windows.Forms.MessageBox.Show("此对象未定位.");
                        buttonSave.Enabled = false;
                        buttonSave.Text = "更新";
                        buttonCancel.Enabled = false;
                        buttonCancel.Text = "取消";

                        //填充datagridview1
                        browIndex = e.RowIndex;
                        fillInfoDatagridView(this.dataGridViewList.Rows[e.RowIndex]);
                        return;
                    }
                    double x = Convert.ToDouble(dataGridViewList["X", e.RowIndex].Value);
                    double y = Convert.ToDouble(dataGridViewList["Y", e.RowIndex].Value);
                    if (x == 0 || y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("此对象未定位.");
                        buttonSave.Enabled = false;
                        buttonSave.Text = "更新";
                        buttonCancel.Enabled = false;
                        buttonCancel.Text = "取消";

                        //填充datagridview1
                        browIndex = e.RowIndex;
                        fillInfoDatagridView(this.dataGridViewList.Rows[e.RowIndex]);
                        return;
                    }

                    // edit by fisher in 09-12-24   设置地图视野
                    MapInfo.Geometry.DPoint dP = new MapInfo.Geometry.DPoint(x, y);
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                    MapInfo.Geometry.FeatureGeometry fg = new MapInfo.Geometry.Point(cSys, dP);

                    mapControl1.Map.SetView(dP, cSys, getScale());
                    this.mapControl1.Map.Center = dP;
                    mapControl1.Refresh();

                    //待完善,从地图中查找要素,闪烁

                    FeatureLayer tempLayer = mapControl1.Map.Layers[editTable.Alias] as MapInfo.Mapping.FeatureLayer;
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
                    string sID = dataGridViewList[CLC.ForSDGA.GetFromTable.ObjID, e.RowIndex].Value.ToString();
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tempLayer.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("keyID='" + sID + "'"));
                    if (ft != null)
                    {
                        if (ft["MapID"] != null && ft["MapID"].ToString() != "")
                        {
                            selPrinx = Convert.ToInt32(ft["MapID"]);
                        }
                        selMapID = sID;
                        ////闪烁要素
                        flashFt = ft;
                        defaultStyle = ft.Style;
                        k = 0;
                        timer1.Start();
                    }
                    fillInfoDatagridView(this.dataGridViewList.Rows[e.RowIndex]);
                    buttonSave.Enabled = false;
                    buttonSave.Text = "更新";
                    buttonCancel.Enabled = false;
                    buttonCancel.Text = "取消";
                }
                this.dataGridViewList.Invalidate();
                browIndex = e.RowIndex;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("此对象未定位.");
            }
            setDataGridBG();
        }

        /// <summary>
        /// 基础数据编辑 填充编辑列表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="dataGridViewRow">数据行</param>
        private void fillInfoDatagridView(DataGridViewRow dataGridViewRow)
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "照片")
                        continue;
                    this.dataGridView1.Rows[i].Cells[1].Value = dataGridViewRow.Cells[this.dataGridView1.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "fillInfoDatagridView");
            }
            if (temEditDt.Rows[0]["基础数据可编辑"].ToString() != "1")
            {
                this.dataGridView1.Visible = true;
                this.dataGridView1.ReadOnly = true;
                this.btnLoc1.Enabled = false;
                buttonSave.Enabled = false;
            }
            else
            {
                this.dataGridView1.Visible = true;
                this.dataGridView1.Columns[1].ReadOnly = false;
                this.buttonSave.Text = "更新";
            }
            // 所属派出所代码及所属中队代码的值会自动生成，所以不用修
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                switch (comboTables.Text)
                {
                    case "基层民警中队":
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "所属派出所代码")
                        {
                            dataGridView1.Rows[i].Cells[1].ReadOnly = true;
                        }
                        break;
                    case "社区警务室":
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "所属派出所代码" || dataGridView1.Rows[i].Cells[0].Value.ToString() == "所属中队代码")
                        {
                            dataGridView1.Rows[i].Cells[1].ReadOnly = true;
                        }
                        break;
                }
            }
        }

        private Color col = Color.Blue;
        /// <summary>
        /// 图元闪烁
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (col == Color.Red)
                {
                    col = Color.Blue;
                }
                else
                {
                    col = Color.Red;
                }
                BasePointStyle pStyle = new SimpleVectorPointStyle(70, col, 12);
                flashFt.Style = pStyle;
                flashFt.Update();
                k++;
                if (k == 7)
                {
                    flashFt.Style = defaultStyle;
                    flashFt.Update();
                    timer1.Stop();
                }
            }
            catch
            {
                timer1.Stop();
            }
        }

        #region 数据导入导出(Creat by Rainny)

        /// <summary>
        /// 设置按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="tableName">表名</param>
        private void SetButtonStyle(string tableName)
        {
            try
            {
                if (tableName == "派出所辖区" || tableName == "警务室辖区" || tableName == "民警中队辖区" || tableName == "派出所每日警员表" || tableName == "中队每日警员表" || tableName == "案件类别维护")
                {
                    btnDataIn.Enabled = false;
                    btnDataOut.Enabled = false;

                    buttonSearch.Enabled = false;
                    textKeyWord.Enabled = false;
                    cbMohu.Enabled = false;
                }
                else if (tableName == "视频位置")
                {
                    btnDataIn.Enabled = false;
                    btnDataOut.Enabled = false;

                    buttonSearch.Enabled = true;
                    textKeyWord.Enabled = true;
                    cbMohu.Enabled = true;
                }
                else
                {
                    btnDataIn.Enabled = true;
                    btnDataOut.Enabled = true;

                    buttonSearch.Enabled = true;
                    textKeyWord.Enabled = true;
                    cbMohu.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "SetButtonStyle");
            }
        }

        /// <summary>
        /// 导入数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btnDataIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["可导入"].ToString() != "1")
                {
                    MessageBox.Show("您没有导入权限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "请选择将导入的EXCEL文件路径";
                ofd.Filter = "Excel文档(*.xls)|*.xls";
                ofd.FileName = this.comboTables.Text;

                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
                {
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;
                    int dataCount = dg.InData(ofd.FileName, this.comboTables.Text);
                    this.Cursor = Cursors.Default;
                    if (dataCount != 0)
                    {
                        if (MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新，\n\t您是否要查看导出报告！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            string FilePath = Application.StartupPath + @"\Export.txt";

                            if (File.Exists(FilePath))
                            {
                                System.Diagnostics.Process.Start(FilePath);         // 然后删除该文件
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnDataIn_Click");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 导出数据
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void btnDataOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["可导出"].ToString() != "1")
                {
                    MessageBox.Show("您没有导出权限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_exportDT == null)
                {
                    if (MessageBox.Show("现在未查询任何数据,将导出数据库所有该表数据;\r会耗费较长时间,继续请点击确认,重新查询请点取消", "导出确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Title = "请选择将导出的EXCEL文件存放路径";
                        sfd.Filter = "Excel文档(*.xls)|*.xls";
                        sfd.FileName = this.comboTables.Text;
                        if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                        {
                            if (sfd.FileName.LastIndexOf(".xls") <= 0)
                            {
                                sfd.FileName = sfd.FileName + ".xls";
                            }
                            string fileName = sfd.FileName;
                            if (System.IO.File.Exists(fileName))
                            {
                                System.IO.File.Delete(fileName);
                            }
                            DataGuide dg = new DataGuide();
                            this.Cursor = Cursors.WaitCursor;
                            if (dg.OutData(fileName, this.comboTables.Text))
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("导出Excel完成!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("导出Excel失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                        }
                    }
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "请选择将导出的EXCEL文件存放路径";
                    sfd.Filter = "Excel文档(*.xls)|*.xls";
                    sfd.FileName = this.comboTables.Text;
                    if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                    {
                        if (sfd.FileName.LastIndexOf(".xls") <= 0)
                        {
                            sfd.FileName = sfd.FileName + ".xls";
                        }
                        string fileName = sfd.FileName;
                        if (System.IO.File.Exists(fileName))
                        {
                            System.IO.File.Delete(fileName);
                        }
                        DataGuide dg = new DataGuide();
                        this.Cursor = Cursors.WaitCursor;

                        if (dg.OutData(fileName, _exportDT, this.comboTables.Text))
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("导出Excel完成!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("导出Excel失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        _exportDT = null;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnDataOut_Click");
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        /// <summary>
        /// 基础数据编辑 tab切换
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl2.SelectedTab == tabPageList)
                {
                    if (this.buttonCancel.Enabled == true && this.buttonCancel.Text == "取消添加")
                    {
                        MessageBox.Show("请完成上一条记录的属性添加和保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabControl2.SelectedTab = tabPageInfo;
                    }
                }
                else if (comboTables.Text == "视频位置")
                {
                    tabControl2.SelectedTab = tabPageList;
                }
                else
                {
                    if (this.dataGridView1.Visible == false)
                    {
                        this.buttonSave.Enabled = false;
                        this.buttonCancel.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "tabControl2_SelectedIndexChanged");
                this.Cursor = Cursors.Default;
            }
        }

        private void checkListAll_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkListAll.Checked)
            //{
            //    textBox1.Enabled = false;
            //    checkISMohu.Enabled = false;
            //    try
            //    {
            //        string sql = "";
            //        if (dataGridViewVideo.DataSource == null || dataGridViewVideo.Visible == false || videolocSql == "" || this.dataGridExp.Rows.Count == 0)
            //        {
            //            sql = "select 设备名称,X,Y,所属派出所,日常管理人,设备编号,mapid from 视频位置 where x is null or x=0 or y is null or y=0";
            //        }
            //        else
            //        {
            //            if (videolocSql.IndexOf("where") > -1)
            //            {
            //                sql = videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
            //            }
            //            else
            //            {
            //                sql = videolocSql + " where (X is null or Y is null or X = 0 or Y = 0)";
            //            }
            //        }
            //        OracelData linkData = new OracelData(mysqlstr);
            //        ds = linkData.SelectDataBase(sql, "temVideo");
            //        DataTable dt = ds.Tables[0];
            //        dataGridViewVideo.DataSource = dt;
            //        dataGridViewVideo.Columns[5].Visible = false;      // 设备编号不显示
            //        dataGridViewVideo.Columns[6].Visible = false;      // mapid不显示
            //    }
            //    catch (Exception ex) { writeToLog(ex, "checkListAll_CheckedChanged"); }
            //}
            //else
            //{
            //    textBox1.Enabled = true;
            //    checkISMohu.Enabled = true;
            //}
        }

        /// <summary>
        /// 视频编辑 更新位置
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void buttonSaveWZ_Click(object sender, EventArgs e)
        {
            if (dataGridViewVideo.Rows.Count < 1) return;
            OracelData linkData = new OracelData(mysqlstr);
            string sql = "";
            try
            {
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    //updated by fisher in 09-09-21
                    if (dataGridViewVideo.Rows[i].Cells["x"].Value == null || dataGridViewVideo.Rows[i].Cells["y"].Value == null || dataGridViewVideo.Rows[i].Cells["x"].Value.ToString() == "" || dataGridViewVideo.Rows[i].Cells["y"].Value.ToString() == "")
                    {
                        continue;
                    }
                    else
                    {
                        if (dataGridViewVideo.Rows[i].Cells["mapid"].Value != null && dataGridViewVideo.Rows[i].Cells["mapid"].Value.ToString() != "")
                        {
                            sql = "update 视频位置 set x=" + Convert.ToDouble(dataGridViewVideo.Rows[i].Cells["x"].Value) +
                            ",y=" + Convert.ToDouble(dataGridViewVideo.Rows[i].Cells["y"].Value) +
                            ",设备名称 = '" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["设备名称"].Value) +
                            "',所属派出所='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["所属派出所"].Value) +
                            "',日常管理人='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["日常管理人"].Value) +
                            "',mapid='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["mapid"].Value) +
                            "' where 设备编号='" + Convert.ToString(dataGridViewVideo.Rows[i].Cells["设备编号"].Value) + "'";
                            linkData.UpdateDataBase(sql);
                        }
                        continue;
                    }
                }
                insertQueryIntoTable((DataTable)dataGridViewVideo.DataSource);
                MessageBox.Show("保存成功!", "提示");
                buttonSaveWZ.Enabled = false;
                butLoc2.Enabled = false;
                btnCancel2.Enabled = false;
                this.mapControl1.Tools.LeftButtonTool = "Pan";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonSaveWZ_Click");
                MessageBox.Show(ex.Message, "buttonSaveWZ_Click()");
            }
        }

        /// <summary>
        /// 视频编辑 异常提示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void dataGridViewVideo_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
            {
                MessageBox.Show("坐标必须为浮点型!", "提示");
            }
        }

        //以下变量用于公安视频编辑的分页显示 lili 2010-11-11 光棍节万岁
        int perPageSize = 100;       // 每页显示行数
        int vMax = 0;                // 总记录数
        int pageVideoCount = 0;      // 页数＝总记录数/每页显示行数
        int pageVideoCurrent = 0;    // 当前页号
        int vCurrent = 0;            // 当前记录行

        int _startNo = 0, _endNo = 0;// 存储查询结果的开始、结尾数
        string videolocSql = "";     // 用于列出未定位视频点的查询语
        string videoSQL = "";
        /// <summary>
        /// 视频编辑 查询按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)       //update by fisher in 09-12-22
        {
            try
            {
                if (temEditDt.Rows[0]["视频可编辑"].ToString() == "0")
                {
                    MessageBox.Show("您没有查询权限!", "提示");
                    return;
                }
                isShowPro(true);
                string countSql = "";
                if (this.dataGridExp.Rows.Count == 0)
                {
                    if (strRegion == "顺德区")
                    {
                        videoSQL = "";
                        videolocSql = "";
                        countSql = "select count(*) from 视频位置";
                    }
                    else
                    {
                        videoSQL = " 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        videolocSql = " 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        countSql = "select count(*) from 视频位置 where 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else
                {
                    if (Video_getSqlString() == "")
                    {
                        isShowPro(false);
                        MessageBox.Show("查询语句有错误,请重设!");
                        return;
                    }
                    if (strRegion == "顺德区")
                    {
                        videoSQL = " " + Video_getSqlString();
                        videolocSql = " " + Video_getSqlString();
                        countSql = "select count(*) from 视频位置 where " + Video_getSqlString();
                    }
                    else
                    {
                        videoSQL = " " + Video_getSqlString() + " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        videolocSql = " " + Video_getSqlString() + " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "') ";
                        countSql = "select count(*) from 视频位置 where " + Video_getSqlString() + " and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                this.getMaxCount(countSql);
                InitDataSet(this.tsCount);
                _startNo = 0;
                _endNo = perPageSize;
                DataTable dt = getLoadData(_startNo, _endNo, videoSQL);
                dataGridViewVideo.DataSource = dt;
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;

                dataGridViewVideo.Columns[5].Visible = false;      // 设备编号不显示
                dataGridViewVideo.Columns[6].Visible = false;      // mapid不显示
                insertQueryIntoTable(dt);
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "buttonOK_Click");
            }
        }

        /// <summary>
        /// 查询结果  
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="starNo">开始记录数</param>
        /// <param name="endNo">结束记录数</param>
        /// <param name="sql">所设条件</param>
        /// <returns>查询结果</returns>
        private DataTable getLoadData(int starNo, int endNo, string sql)
        {
            try
            {
                string completeSQL = "";
                DataTable objset = new DataTable();
                OracelData linkData = new OracelData(mysqlstr);
                if (sql == "")
                {
                    completeSQL = "select 设备名称,X,Y,所属派出所,日常管理人,设备编号,mapid from (select rownum as rn1,a.* from 视频位置 a where rownum<=" + endNo + ") t where rn1 >=" + _startNo;
                }
                else
                {
                    completeSQL = "select 设备名称,X,Y,所属派出所,日常管理人,设备编号,mapid from (select rownum as rn1,a.* from 视频位置 a where rownum<=" + endNo + " and " + sql + ") t where rn1 >=" + _startNo;
                }
                objset = linkData.SelectDataBase(completeSQL);
                return objset;

            }
            catch (Exception ex)
            {
                writeToLog(ex, "getLoadData");
                return null;
            }
        }

        /// <summary>
        /// 获取所设条件记录总数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="sql">sql语句</param>
        private void getMaxCount(string sql)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();

                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                vMax = Convert.ToInt32(Cmd.ExecuteScalar().ToString());
                Cmd.Dispose();
                Conn.Close();
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                writeToLog(ex, "getMaxCount");
                vMax = 0;
            }
        }

        /// <summary>
        /// 初始化分页控件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-2-10
        /// </summary>
        /// <param name="tsLabel"></param>
        public void InitDataSet(ToolStripLabel tsLabel)
        {
            try
            {
                perPageSize = Convert.ToInt32(this.tstbPre.Text);      //设置页面行数                
                tsLabel.Text = vMax.ToString() + "条";//在导航栏上显示总记录数
                pageVideoCount = (vMax / perPageSize);//计算出总页数
                if ((vMax % perPageSize) > 0) pageVideoCount++;
                if (vMax != 0)
                {
                    pageVideoCurrent = 1;
                }
                else { pageVideoCurrent = 0; }
                this.bnCount.Text = "/" + pageVideoCount.ToString();
                this.tstNow.Text = pageVideoCurrent.ToString();
                vCurrent = 0;       //当前记录数从0开始
            }
            catch (Exception ex)
            {
                writeToLog(ex, "InitDataSet");
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                LayerControlDlg lcDlg = new LayerControlDlg();
                lcDlg.Map = mapControl1.Map;
                lcDlg.LayerControl.Tools = mapControl1.Tools;
                lcDlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "toolStripButton1_Click");
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string layName = "";
            if (tabControl1.SelectedTab == tabPage1)
            {
                //feng  09-08-20
                //toolLoc.Enabled = false;
                comboTables.Text = "基层派出所";
                comboTables_SelectedIndexChanged(null, null);
            }
            if (tabControl1.SelectedTab == tabPage2)
            {
                Initialvideo();
                V_setfield();  //初始化查询字段
                //toolLoc.Enabled = true;
                //checkListAll.Checked = false;
                layName = "视频位置";
                Cursor.Current = Cursors.Default;
                mapControl1.Tools.LeftButtonTool = "Pan";
                this.valueText = "视频位置";
                //feng
                try  //先关闭之前的表和标注表
                {
                    string sAlias = this.editTable.Alias;
                    if (mapControl1.Map.Layers[sAlias] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    }
                    //（以下代码由feng  09-08-20 修改完成）
                    this.editTable = createTable(layName);
                    FeatureLayer fl = new FeatureLayer(this.editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(this.editTable);//标注图层
                    this.featStyle = setFeatStyle(layName);

                    if (this.editTable != null)
                    {
                        if (temEditDt.Rows[0]["视频可编辑"].ToString() == "1")   //设置视频位置的图层是否可以编辑
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layName], true);
                        }
                        else
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[layName], false);
                        }
                        MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[layName], true);
                        mapControl1.Map.Center = mapControl1.Map.Center;
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "tabControl1_SelectedIndexChanged");
                    MessageBox.Show(ex.Message, "tabControl1_SelectedIndexChanged");
                }
                finally
                {
                    //add by fisher in 09-12-15
                    if (temEditDt.Rows[0]["视频可编辑"].ToString() != "1")  //没有编辑权限
                    {
                        this.dataGridViewVideo.ReadOnly = true;
                        this.butLoc2.Enabled = false;
                        this.buttonSaveWZ.Enabled = false;
                        this.btnCancel2.Enabled = false;
                    }
                }
            }
            if (tabControl1.SelectedTab == tabPage3) //公安业务编辑
            {
                settabGa();//设置公安业务编辑的初值  feng
                this.dataGridViewValue.Rows.Clear();
                //toolLoc.Enabled = false;
                comboTable.Text = "案件信息";
                comboTable_SelectedIndexChanged(null, null);
            }

        }

        //公安业务编辑初始化
        private void settabGa()
        {
            try
            {
                this.comboTable.SelectedIndex = 0;
                this.comboOrAnd.SelectedIndex = 0;
                this.comboField.SelectedIndex = 0;
                this.comboYunsuanfu.SelectedIndex = 0;
                //初始时“案件信息”，应该将isOracleSpatialTab的属性设置为true
                isOracleSpatialTab = true;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "settabGa");
            }
        }

        private void Initialvideo()  //初始化视频表
        {
            try
            {
                this.FieldStr.Text = "设备编号";
                this.ValueStr.Text = "";
                this.dataGridExp.Rows.Clear();
                this.dataGridViewVideo.DataSource = null;
                this.butLoc2.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Initialvideo");
            }
        }

        private void dataGridViewList_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                buttonSave.Enabled = true;
                buttonCancel.Enabled = true;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                if (dataGridView1.Rows.Count > 2)
                {
                    string cellName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string celStr = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string sqlStr = "", telNo = "";

                    switch (cellName)
                    {
                        case "所属派出所":
                            sqlStr = "select 派出所代码 from 基层派出所 where 派出所名='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "所属派出所代码", dataGridView1);
                            break;
                        case "所属中队":
                            sqlStr = "select 中队代码 from 基层民警中队 where 中队名='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "所属中队代码", dataGridView1);
                            break;
                        case "所属警务室":
                            sqlStr = "select 警务室代码 from 社区警务室 where 警务室名='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "所属警务室代码", dataGridView1);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridView1_CellValueChanged");
            }
        }

        private void dataGridViewVideo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            buttonSaveWZ.Enabled = true;
            btnCancel2.Enabled = true;
        }

        //以下代码由Fisher.feng添加  20090814
        private void comboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (this.valueText == this.comboTable.Text)
                {
                    this.mapControl1.Focus();
                    return;
                }
                try  //先关闭之前的表和标注表
                {
                    string sAlias = this.editTable.Alias;
                    if (mapControl1.Map.Layers[sAlias] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "comboTable_SelectedIndexChanged");
                    MessageBox.Show(ex.Message, "comboTable_SelectedIndexChanged关闭之前的表");
                }

                this.pageCount = 0;
                this.pageCurrent = 0;
                this.RecordCount.Text = "0条";
                this.PageNow.Text = "0";//设置当前页
                this.lblPageCount.Text = "/ {0}";//设置总页数
                this.toolStripTextBox1.Text = pageSize.ToString();

                dataGridViewGaList.DataSource = null;
                this.dataGridViewGaInfo.Rows.Clear();
                this.dataGridViewGaInfo.Visible = false;
                btnGaSave.Text = "保存";
                btnGaCancel.Text = "取消";
                this.valueText = this.comboTable.Text;
                this.btnLoc3.Enabled = false;

                //通过名称获取表名
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text.Trim(), getFromNamePath);
                setFields(CLC.ForSDGA.GetFromTable.TableName);//.设置各查询字段
                switch (comboTable.Text)
                {
                    case "公共场所":
                    case "安全防护单位":
                    case "网吧":
                    case "特种行业":
                    case "消防栓":
                    case "消防重点单位":
                    case "治安卡口系统":
                        mapToolBar2.Buttons[0].Visible = true;
                        mapToolBar2.Buttons[0].Enabled = true;
                        mapToolBar2.Buttons[2].Visible = true;
                        mapToolBar2.Buttons[2].Enabled = true;
                        mapToolBar2.Buttons[5].Visible = true;
                        mapToolBar2.Buttons[5].Enabled = true;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPoint")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = false;

                        featStyle = setFeatStyle(comboTable.Text);
                        this.GaGetTable(this.comboTable.Text);
                        break;

                    case "人口系统":
                    case "出租屋房屋系统":
                    case "案件信息":
                        mapToolBar2.Buttons[0].Visible = true;
                        mapToolBar2.Buttons[0].Enabled = true;
                        mapToolBar2.Buttons[2].Visible = true;
                        mapToolBar2.Buttons[2].Enabled = true;
                        mapToolBar2.Buttons[5].Visible = true;
                        mapToolBar2.Buttons[5].Enabled = true;
                        if (this.mapControl1.Tools.LeftButtonTool == "AddPoint")
                        {
                            mapControl1.Tools.LeftButtonTool = "Pan";
                        }
                        this.isOracleSpatialTab = true;

                        featStyle = setFeatStyle(comboTable.Text);
                        this.GaGetTable(this.comboTable.Text);
                        break;
                }
                Ga_exportDT = null;

            }
            catch (Exception ex)
            {
                writeToLog(ex, "comboTable_SelectedIndexChanged");
            }
            Cursor.Current = Cursors.Default;
            btnGaSave.Enabled = false;
            btnGaCancel.Enabled = false;
            btnLoc3.Enabled = false;
        }

        //设置comboField里边的字段值
        string arrType = "";
        private void setFields(string tableName)
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                conn.Open();
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME= '" + tableName + "'";
                OracleCommand cmd = new OracleCommand(sExp, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                dAdapter.Dispose();
                cmd.Dispose();
                DataTable dt = ds.Tables[0];
                conn.Close();

                comboField.Items.Clear();
                arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string aCol = dt.Rows[i][0].ToString();
                    string atype = dt.Rows[i][1].ToString();

                    if (aCol != "" && aCol != "mapid" && aCol.IndexOf("备用字段") < 0 && aCol != "X" && aCol != "Y" && aCol != "GEOLOC" && aCol != "MI_STYLE" && aCol != "MI_PRINX" && aCol.IndexOf("代码") < 0)
                    {
                        comboField.Items.Add(aCol);
                        arrType += atype + ",";
                    }
                }

                comboField.Text = comboField.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setFields");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        //设置视频定位中的FieldStr字段
        string V_arrType = "";
        private void V_setfield()
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                conn.Open();
                string sExp = "SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '视频位置'";
                OracleCommand cmd = new OracleCommand(sExp, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();

                FieldStr.Items.Clear();
                V_arrType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string vCol = dt.Rows[i][0].ToString();
                    string vType = dt.Rows[i][1].ToString();
                    if (vCol != "" && vCol != "mapID" && vCol != "X" && vCol != "Y")
                    {
                        FieldStr.Items.Add(vCol);
                        V_arrType += vType + ",";
                    }
                }
                FieldStr.Text = "设备编号";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "V_setfield");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        //当comboField的选项发生变换时，要控制运算符符合逻辑性
        private void comboField_SelectedIndexChanged(object sender, EventArgs e)
        {
            setYunsuanfuValue(comboField.SelectedIndex);
        }

        private void setYunsuanfuValue(int p)
        {
            try
            {
                string[] arr = arrType.Split(',');
                string type = arr[p].ToUpper();
                if (type == "DATE")
                {
                    dateTimePicker1.Visible = true;
                    dateTimePicker1.Text = DateTime.Now.ToString();
                    textValue.Visible = false;
                }
                else
                {
                    dateTimePicker1.Visible = false;
                    textValue.Visible = true;
                }
                comboYunsuanfu.Items.Clear();

                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        comboYunsuanfu.Items.Add("等于");
                        comboYunsuanfu.Items.Add("不等于");
                        comboYunsuanfu.Items.Add("大于");
                        comboYunsuanfu.Items.Add("大于等于");
                        comboYunsuanfu.Items.Add("小于");
                        comboYunsuanfu.Items.Add("小于等于");
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        comboYunsuanfu.Items.Add("等于");
                        comboYunsuanfu.Items.Add("不等于");
                        comboYunsuanfu.Items.Add("包含");
                        break;
                }
                comboYunsuanfu.Text = "等于";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setYunsuanfuValue");
            }
        }

        private void V_setYSF(int V_index)  //视频定位设置运算符
        {
            try
            {
                string[] arr = V_arrType.Split(',');
                string type = arr[V_index].ToUpper();
                MathStr.Items.Clear();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                    case "DATE":
                        MathStr.Items.Add("等于");
                        MathStr.Items.Add("不等于");
                        MathStr.Items.Add("大于");
                        MathStr.Items.Add("大于等于");
                        MathStr.Items.Add("小于");
                        MathStr.Items.Add("小于等于");
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        MathStr.Items.Add("等于");
                        MathStr.Items.Add("不等于");
                        MathStr.Items.Add("包含");
                        break;
                }
                MathStr.Text = "等于";
                connStr.Text = connStr.Items[0].ToString();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "V_setYSF");
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboTable.Text.Trim() == "")
                {
                    MessageBox.Show("请选择表");
                    return;
                }

                if (textValue.Visible && textValue.Text.Trim() == "")
                {
                    MessageBox.Show("查询值不能为空！");
                    return;
                }

                if (this.textValue.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("输入的字符串中不能包含单引号!");
                    return;
                }

                string strExp = "";
                int p = comboField.SelectedIndex;
                string[] arr = arrType.Split(',');
                string type = arr[p].ToUpper();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   " + textValue.Text.Trim();
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   " + textValue.Text.Trim();
                        }
                        this.dataGridViewValue.Rows.Add(new object[] { strExp, "数字" });
                        break;
                    case "DATE":
                        string tValue = this.dateTimePicker1.Value.ToString();
                        if (tValue == "")
                        {
                            MessageBox.Show("查询值不能为空！");
                            return;
                        }

                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "时间" });
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + tValue + "'";
                            this.dataGridViewValue.Rows.Add(new object[] { strExp, "时间" });
                        }
                        break;
                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridViewValue.Rows.Count == 0)
                        {
                            strExp = this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "包含")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        else
                        {
                            strExp = this.comboOrAnd.Text + "  " + this.comboField.Text + "   " + this.comboYunsuanfu.Text + "   '" + textValue.Text.Trim() + "'";
                            if (this.comboYunsuanfu.Text.Trim() == "包含")
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridViewValue.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        break;
                }
                this.comboTable.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonAdd_Click");
            }
        }

        // 移除一个表达式
        private void buttonRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridViewValue.Rows.Count != 0)
                {
                    if (this.dataGridViewValue.CurrentRow.Index != 0)
                    {
                        this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                    }
                    else
                    {
                        if (this.dataGridViewValue.Rows.Count > 1)
                        {
                            this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                            string text = this.dataGridViewValue.Rows[0].Cells["Value"].Value.ToString().Replace("并且", "");

                            text = text.Replace("或者", "").Trim();
                            this.dataGridViewValue.Rows[0].Cells["Value"].Value = text;
                        }
                        else
                        {
                            this.dataGridViewValue.Rows.Remove(this.dataGridViewValue.CurrentRow);
                            this.comboTable.Enabled = true;
                        }

                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonRemove_Click");
            }
        }

        //清空条件表达式
        private void buttonClear_Click(object sender, EventArgs e)
        {
            _mySQL = "";
            _first = Convert.ToInt32(this.toolStripTextBox1.Text);
            _end = 1;
            try
            {
                this.dataGridViewGaList.Columns.Clear();
                this.dataGridViewGaInfo.Visible = false;
                this.textValue.Text = "";
                this.comboTable.Enabled = true;
                dataGridViewValue.Rows.Clear();
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;
                //this.toolLoc.Enabled = false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttonClear_Click");
            }
        }

        //将dataGridViewValue里边的条目转换为字符串
        private string getSqlString()//转换字符串
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";

                for (int i = 0; i < this.dataGridViewValue.Rows.Count; i++)
                {
                    string type = this.dataGridViewValue.Rows[i].Cells["Type"].Value.ToString();
                    string str = this.dataGridViewValue.Rows[i].Cells["Value"].Value.ToString();
                    if (type == "包含")
                    {
                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {

                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                str += " '%" + strArray[1] + "%'";
                            }
                        }
                        array.Add(str);
                    }
                    else if (type == "时间")
                    {

                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {

                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                str += "to_date('" + strArray[1] + "', 'YYYY-MM-DD HH24:MI:SS')";
                            }
                        }
                        array.Add(str);
                    }
                    else
                    {
                        array.Add(str);
                    }
                }

                for (int j = 0; j < array.Count; j++)
                {
                    if (j == 0)
                    {
                        getsql = array[j].ToString();
                    }
                    else
                    {
                        getsql += "   " + array[j].ToString();
                    }
                }

                getsql = getsql.Replace("并且", "and");
                getsql = getsql.Replace("或者", "or");
                getsql = getsql.Replace("包含", "like");
                getsql = getsql.Replace("大于等于", ">=");
                getsql = getsql.Replace("小于等于", "<=");
                getsql = getsql.Replace("大于", ">");
                getsql = getsql.Replace("小于", "<");
                getsql = getsql.Replace("不等于", "!=");
                getsql = getsql.Replace("等于", "=");

                return getsql;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getSqlString");
                return "";
            }
        }

        //将dataGridExp里边的条目转换为字符串
        private string Video_getSqlString()//转换字符串
        {
            try
            {
                ArrayList array = new ArrayList();
                string getsql = "";
                for (int i = 0; i < this.dataGridExp.Rows.Count; i++)
                {
                    string type = this.dataGridExp.Rows[i].Cells["video_Type"].Value.ToString();
                    string str = this.dataGridExp.Rows[i].Cells["video_Value"].Value.ToString();
                    if (type == "包含")
                    {
                        string[] strArray = new string[3];
                        strArray = str.Split('\'');
                        str = "";
                        for (int j = 0; j < strArray.Length; j++)
                        {

                            if (j == 0)
                            {
                                str = strArray[0];
                            }
                            if (j == 1)
                            {
                                str += " '%" + strArray[1] + "%'";
                            }
                        }
                        array.Add(str);
                    }
                    else
                    {
                        array.Add(str);
                    }
                }

                for (int j = 0; j < array.Count; j++)
                {
                    if (j == 0)
                    {
                        getsql = array[j].ToString();
                    }
                    else
                    {
                        getsql += "   " + array[j].ToString();
                    }
                }

                getsql = getsql.Replace("并且", "and");
                getsql = getsql.Replace("或者", "or");
                getsql = getsql.Replace("包含", "like");
                getsql = getsql.Replace("大于等于", ">=");
                getsql = getsql.Replace("小于等于", "<=");
                getsql = getsql.Replace("大于", ">");
                getsql = getsql.Replace("小于", "<");
                getsql = getsql.Replace("不等于", "!=");
                getsql = getsql.Replace("等于", "=");

                return getsql;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "Video_getSqlString");
                return "";
            }
        }

        //string GJsql = "";
        //以下变量用于公安业务编辑的分页显示
        int pageSize = 100;     //每页显示行数
        int nMax = 0;         //总记录数
        int pageCount = 0;    //页数＝总记录数/每页显示行数
        int pageCurrent = 0;   //当前页号
        int nCurrent = 0;      //当前记录行

        //----------- 以下变量用于分页 add by lili 2010-8-26 ----------------         
        int _first = 0;         // 结束行数
        int _end = 1;           // 开始行数
        string tabName = "";    // 表名
        string _mySQL = "", _PageSQL = "";  //用户查询权限
        //-------------------------------------------------------------------

        string PageSQL = "";   //用于获得分页数据
        string mySQL = "";

        private void buttonMultiOk_Click(object sender, EventArgs e)
        {
            try
            {
                isShowPro(true);
                this.Cursor = Cursors.WaitCursor;
                dataGridViewGaList.Columns.Clear();

                //通过名称获取表名，对象名
                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                tabName = CLC.ForSDGA.GetFromTable.TableName;
                _mySQL = "";
                _first = Convert.ToInt32(this.toolStripTextBox1.Text);
                _end = 1;
                // 如果没有设条件就查询全部结果
                if (this.dataGridViewValue.Rows.Count == 0)
                {
                    mySQL = "select count(*) from " + tabName;
                }
                else
                {
                    if (isOracleSpatialTab)
                    {
                        mySQL = "select count(*) from " + tabName + " t" + " where " + this.getSqlString();
                    }
                    else
                    {
                        mySQL = "select count(*) from " + tabName + " where " + this.getSqlString();
                    }
                }

                if (strRegion != "顺德区" && strRegion != "")   //edit by fisher in 09-12-01
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }

                    if (this.dataGridViewValue.Rows.Count == 0)
                    {
                        _mySQL += " " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                    }
                    else
                    {
                        _mySQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                    }

                }
                if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                {
                    if (this.dataGridViewValue.Rows.Count == 0)
                    {
                        _mySQL += " (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        if (_mySQL.IndexOf("and") > -1)
                        {
                            _mySQL = _mySQL.Remove(_mySQL.LastIndexOf(")"));
                            _mySQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                        }
                        else
                        {
                            _mySQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                        }
                    }
                }
                OracleConnection conn = new OracleConnection(mysqlstr);
                try
                {
                    // 如果没有设条件就查询全部结果
                    if (this.dataGridViewValue.Rows.Count == 0)
                    {
                        mySQL = mySQL.IndexOf("where") > -1 ? mySQL : _mySQL == string.Empty ? mySQL : mySQL + " where " + _mySQL;
                    }
                    else
                    {
                        mySQL += _mySQL;
                    }
                    conn.Open();
                    OracleCommand cmd = new OracleCommand(mySQL, conn);
                    OracleDataAdapter orada = new OracleDataAdapter(cmd);
                    DataTable _ds = new DataTable();
                    orada.Fill(_ds);
                    cmd.Dispose();
                    orada.Dispose();
                    conn.Close();
                    if (Convert.ToInt32(_ds.Rows[0][0]) < 1)
                    {
                        isShowPro(false);
                        MessageBox.Show("所设条件无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Cursor = Cursors.Default;
                        this.RecordCount.Text = "0条";
                        this.PageNow.Text = "0";//设置当前页
                        this.lblPageCount.Text = "/ {0}";//设置总页数
                        this.toolStripTextBox1.Text = pageSize.ToString();
                        return;
                    }
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();
                    //以下代码用来设置翻页
                    try
                    {
                        nMax = Convert.ToInt32(_ds.Rows[0][0]);
                        this.RecordCount.Text = nMax.ToString() + "条";
                        pageSize = Convert.ToInt32(this.toolStripTextBox1.Text);      //设置页面行数
                        pageCount = (nMax / pageSize);//计算出总页数
                        if ((nMax % pageSize) > 0) pageCount++;
                        this.lblPageCount.Text = "/" + pageCount.ToString();//设置总页数
                        this.toolStripTextBox1.Text = pageSize.ToString();
                        if (nMax != 0)
                        {
                            pageCurrent = 1;
                            this.PageNow.Text = pageCurrent.ToString();//设置当前页
                        }
                        nCurrent = 0;       //当前记录数从0开始
                    }
                    catch (Exception ex)
                    {
                        writeToLog(ex, "buttonMultiOk_Click 获取分页参数");
                        MessageBox.Show(ex.Message, "buttonMultiOk_Click 获取分页参数");
                    }

                    if (this.dataGridViewValue.Rows.Count == 0)          // 判断是否用户设有条件
                        _mySQL = _mySQL == string.Empty ? "" : " and " + _mySQL;

                    DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //获取当前页数据
                    dataGridViewGaList.DataSource = datatable;
                    this.toolEditPro.Value = 2;
                    if (dataGridViewGaList.Columns["mapid"] != null)
                    {
                        dataGridViewGaList.Columns["mapid"].Visible = false;
                    }
                    //设置gridview的间隔颜色
                    GasetDataGridBG();

                    this.insertGaQueryIntoTable(datatable);
                    this.dataGridViewGaInfo.Visible = false;

                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//清空datagridview
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                    }

                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;

                    this.toolEditPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "buttonMultiOk_Click设置");
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "buttonMultiOk_Click");
            }
        }

        /// <summary>
        /// 查询分页数据 lili 2010-8-26
        /// </summary>
        /// <param name="wheresql">权限条件</param>
        /// <param name="_first">开始行数</param>
        /// <param name="_end">结束行数</param>
        /// <param name="tabName">表名</param>
        /// <returns>结果集</returns>
        public DataTable LoadData(string wheresql, int _first, int _end, string tabName)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                string sql = "";
                string getSqlStr = getSqlString() == "" ? "" : "and " + getSqlString();    // 判断是否为空
                string _wheresql = wheresql == "" ? "" : wheresql;                         // 判断是否为空
                if (isOracleSpatialTab)
                {
                    sql = "select MI_PRINX as mapid," + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from " + tabName + " a where rownum<=" + _first + " " + getSqlStr + _wheresql + ") t where rn1 >=" + _end;
                }
                else
                {
                    if (tabName == "安全防护单位")
                        sql = "select rownum as mapid,编号,单位名称,单位性质,单位地址,主管保卫工作负责人,手机号码,所属派出所,所属中队,所属警务室,所属派出所代码,所属中队代码,所属警务室代码,'点击编辑' as 安全防护单位文件,抽取ID,抽取更新时间,标注人,标注时间,最后更新人,X,Y,备用字段一,备用字段二,备用字段三 from (select rownum as rn1,a.* from " + tabName + " a where rownum<=" + _first + getSqlStr + _wheresql + ") t where rn1 >=" + _end;
                    else
                        sql = "select rownum as mapid," + CLC.ForSDGA.GetFromTable.SQLFields + " from (select rownum as rn1,a.* from " + tabName + " a where rownum<=" + _first + getSqlStr + _wheresql + ") t where rn1 >=" + _end;
                }

                int nStartPos = 0;   //当前页面开始记录行
                nStartPos = nCurrent;

                DataTable dtInfo;

                dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                Adp.Fill(dtInfo);
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                return dtInfo;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LoadData()");
                MessageBox.Show(ex.Message, "错误LoadData()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
        }

        public DataTable LoadData(string sql)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                int nStartPos = 0;   //当前页面开始记录行
                nStartPos = nCurrent;

                DataTable dtInfo;

                dtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                DataTable[] dataTables = new DataTable[1];
                for (int i = 0; i < dataTables.Length; i++)
                {
                    dataTables[i] = new DataTable();
                }
                Adp.Fill(nStartPos, pageSize, dataTables);//这个地方不知道是从数据库中查到前100行返回，还是所有的数据据都查询到返回，再从中获取前100行。

                dtInfo = dataTables[0];
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                return dtInfo;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LoadData()");
                MessageBox.Show(ex.Message, "错误LoadData()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
        }

        public DataTable BJLoadData(string sql)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                int nStartPos = 0;   //当前页面开始记录行
                nStartPos = bnCurrent;

                DataTable bjdtInfo;

                bjdtInfo = new DataTable();
                Conn.Open();
                OracleCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = sql;
                OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
                DataTable[] dataTables = new DataTable[1];
                for (int i = 0; i < dataTables.Length; i++)
                {
                    dataTables[i] = new DataTable();
                }
                Adp.Fill(nStartPos, bpageSize, dataTables);//这个地方不知道是从数据库中查到前100行返回，还是所有的数据据都查询到返回，再从中获取前100行。

                bjdtInfo = dataTables[0];
                Adp.Dispose();
                Cmd.Dispose();
                Conn.Close();

                return bjdtInfo;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "BJLoadData()");
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
        }

        private void insertGaQueryIntoTable(DataTable dataTable)//将查询到的结果添加到表中
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("没有相关对象，请重设查询关键词！");
                    return;
                }

                (this.editTable as IFeatureCollection).Clear();
                this.editTable.Pack(PackType.All);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "insertGaQueryIntoTable");
                MessageBox.Show(ex.Message, "insertGaQueryIntoTable111");
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                double dX = 0, dY = 0;
                try
                {
                    dX = Convert.ToDouble(dataTable.Rows[i]["X"]);
                    dY = Convert.ToDouble(dataTable.Rows[i]["Y"]);

                    if (dX > 0 && dY > 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(this.editTable)).CoordSys, dX, dY);
                        string tabName = comboTable.Text;
                        if (tabControl1.SelectedTab == tabPage2)
                        {
                            tabName = "视频";
                        }
                        CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                        string sName = CLC.ForSDGA.GetFromTable.ObjName;
                        string strID = CLC.ForSDGA.GetFromTable.ObjID;

                        Feature pFeat = new Feature(editTable.TableInfo.Columns);

                        pFeat.Geometry = pt;
                        pFeat.Style = setFeatStyle(comboTable.Text.Trim());
                        if (dataTable.Columns.Contains("mapid"))
                        {
                            pFeat["mapid"] = dataTable.Rows[i]["mapid"].ToString();
                        }
                        pFeat["keyID"] = dataTable.Rows[i][strID].ToString();
                        pFeat["Name"] = dataTable.Rows[i][sName].ToString();
                        editTable.InsertFeature(pFeat);
                    }
                }
                catch (Exception ex)
                {//如果数据有问题,跳过此记录
                    writeToLog(ex, "insertGaQueryIntoTable");
                    continue;
                }
            }
        }


        /// <summary>
        /// 公安业务编辑导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttDataOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["可导出"].ToString() != "1")
                {
                    MessageBox.Show("您没有导出权限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Ga_exportDT == null)
                {
                    if (MessageBox.Show("现在未查询任何数据,将导出数据库所有该表数据;\r会耗费较长时间,继续请点击确认,重新查询请点取消", "导出确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Title = "请选择将导出的EXCEL文件存放路径";
                        sfd.Filter = "Excel文档(*.xls)|*.xls";
                        sfd.FileName = this.comboTable.Text;
                        if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                        {
                            if (sfd.FileName.LastIndexOf(".xls") <= 0)
                            {
                                sfd.FileName = sfd.FileName + ".xls";
                            }
                            string fileName = sfd.FileName;
                            if (System.IO.File.Exists(fileName))
                            {
                                System.IO.File.Delete(fileName);
                            }
                            DataGuide dg = new DataGuide();
                            this.Cursor = Cursors.WaitCursor;
                            if (dg.OutData(fileName, this.comboTable.Text.Trim()))
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("导出Excel完成!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                this.Cursor = Cursors.Default;
                                MessageBox.Show("导出Excel失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "请选择将导出的EXCEL文件存放路径";
                    sfd.Filter = "Excel文档(*.xls)|*.xls";
                    sfd.FileName = this.comboTable.Text;
                    if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                    {
                        if (sfd.FileName.LastIndexOf(".xls") <= 0)
                        {
                            sfd.FileName = sfd.FileName + ".xls";
                        }
                        string fileName = sfd.FileName;
                        if (System.IO.File.Exists(fileName))
                        {
                            System.IO.File.Delete(fileName);
                        }
                        DataGuide dg = new DataGuide();
                        this.Cursor = Cursors.WaitCursor;
                        if (dg.OutData(fileName, Ga_exportDT, this.comboTable.Text))
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("导出Excel完成!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            this.Cursor = Cursors.Default;

                            MessageBox.Show("导出Excel失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        Ga_exportDT = null;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttDataOut_Click");
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 公安业务编辑导入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttDataIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["可导入"].ToString() != "1")
                {
                    MessageBox.Show("您没有导入权限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "请选择将导入的EXCEL文件路径";
                ofd.Filter = "Excel文档(*.xls)|*.xls";
                ofd.FileName = this.comboTable.Text;

                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileName != "")
                {
                    DataGuide dg = new DataGuide();
                    this.Cursor = Cursors.WaitCursor;
                    int dataCount = dg.InData(ofd.FileName, this.comboTable.Text);
                    this.Cursor = Cursors.Default;
                    if (dataCount != 0)
                    {
                        if (MessageBox.Show("成功完成 " + dataCount.ToString() + " 条数据导入与更新，\n您是否要查看导出报告！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            string FilePath = Application.StartupPath + @"\Export.txt";

                            if (File.Exists(FilePath))
                            {
                                System.Diagnostics.Process.Start(FilePath);             // 然后删除该文件
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("导入数据失败，请检查该Excel是否有数据，\r \r或者该Excel是否正在使用,以及数据格式是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "buttDataIn_Click");
                this.Cursor = Cursors.Default;
            }
        }

        //公安业务编辑中，点击单元格，查找对应的要素，变换要素的样式，实现闪烁。
        FrmZLMessage frmZL;
        int rowIndex = 0;   //定义变量获取当前操作的行用以填充dataGridViewGaInfo的值  fisher（09-09-02）
        private void dataGridViewGaList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //点击一个记录，进行地图定位
                if (e.RowIndex > -1)
                {
                    //点击更多，显示详细信息，否则地图定位
                    if (e.ColumnIndex == dataGridViewGaList.Columns.Count - 11 && comboTable.Text.Trim() == "安全防护单位")
                    {
                        if (dataGridViewGaList.Rows[e.RowIndex].Cells[dataGridViewGaList.Columns.Count - 11].Value.ToString() != "点击编辑") return;

                        if (this.frmZL != null)
                        {
                            if (this.frmZL.Visible == true)
                            {
                                this.frmZL.Close();
                            }
                        }

                        if (dataGridViewGaList.Rows[dataGridViewGaList.CurrentRow.Index].Cells["单位名称"].Value.ToString() == "")
                        {
                            MessageBox.Show("单位名称不能为空！", "提示");
                            return;
                        }
                        this.frmZL = new FrmZLMessage(dataGridViewGaList.Rows[dataGridViewGaList.CurrentRow.Index].Cells["单位名称"].Value.ToString(), mysqlstr, this.temEditDt);

                        //设置信息框在右下角
                        System.Drawing.Point p = this.PointToScreen(mapControl1.Parent.Location);
                        this.frmZL.SetDesktopLocation(mapControl1.Width - frmZL.Width + p.X, mapControl1.Height - frmZL.Height + p.Y + 25);
                        this.frmZL.Show();
                    }

                    if (dataGridViewGaList["X", e.RowIndex].Value == null || dataGridViewGaList["Y", e.RowIndex].Value == null || dataGridViewGaList["X", e.RowIndex].Value.ToString() == "" || dataGridViewGaList["Y", e.RowIndex].Value.ToString() == "")
                    {
                        System.Windows.Forms.MessageBox.Show("此对象未定位.");
                        btnGaSave.Enabled = false;
                        btnGaSave.Text = "更新";
                        btnGaCancel.Enabled = false;
                        btnGaCancel.Text = "取消";
                        if (temEditDt.Rows[0]["业务数据可编辑"].ToString() == "1")
                        {
                            btnLoc3.Enabled = true;  //准备编辑点的坐标
                        }

                        //填充信息DatagridView
                        rowIndex = e.RowIndex;
                        GafillInfoDatagridView(this.dataGridViewGaList.Rows[e.RowIndex]);

                        //add by fisher on 09-09-24
                        //以下代码是在用户录入数据是，如果没有给MapInfo的表添加XY信息，则将其赋值为0，并更新数据库，为后续数据更新打下基础
                        OracleConnection conn = new OracleConnection(mysqlstr);
                        try
                        {
                            if (isOracleSpatialTab)
                            {
                                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                                string GAobjId = CLC.ForSDGA.GetFromTable.ObjID;
                                string KeyID = this.dataGridViewGaList[GAobjId, e.RowIndex].Value.ToString();
                                conn.Open();
                                string updtstr = "update " + comboTable.Text + " t set t.GEOLOC = MDSYS.SDO_GEOMETRY(2001, 8307, MDSYS.SDO_POINT_TYPE('0','0', NULL),NULL, NULL) where " + GAobjId + "= '" + KeyID + "'";
                                OracleCommand cmd = new OracleCommand(updtstr, conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                conn.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "dataGridViewGaList_CellClick(更新数据库)");
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                        return;
                    }
                    btnLoc3.Enabled = true;  //准备编辑点的坐标

                    double x = Convert.ToDouble(dataGridViewGaList["X", e.RowIndex].Value);
                    double y = Convert.ToDouble(dataGridViewGaList["Y", e.RowIndex].Value);
                    if (x == 0 || y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("此对象未定位.");
                        btnGaSave.Enabled = false;
                        btnGaSave.Text = "更新";
                        btnGaCancel.Enabled = false;
                        btnGaCancel.Text = "取消";

                        if (temEditDt.Rows[0]["业务数据可编辑"].ToString() == "1")
                        {
                            btnLoc3.Enabled = true;  //准备编辑点的坐标
                        }

                        //填充信息DatagridView
                        rowIndex = e.RowIndex;
                        GafillInfoDatagridView(this.dataGridViewGaList.Rows[e.RowIndex]);
                        return;
                    }

                    // 以下代码用来将当前地图的视野缩放至该对象所在的派出所
                    //add by fisher in 09-12-24
                    MapInfo.Geometry.DPoint dP = new MapInfo.Geometry.DPoint(x, y);
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                    mapControl1.Map.SetView(dP, cSys, getScale());
                    this.mapControl1.Map.Center = dP;

                    //待完善,从地图中查找要素,闪烁

                    FeatureLayer tempLayer = mapControl1.Map.Layers[editTable.Alias] as MapInfo.Mapping.FeatureLayer;
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    string sID = dataGridViewGaList[CLC.ForSDGA.GetFromTable.ObjID, e.RowIndex].Value.ToString();
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tempLayer.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("keyID='" + sID + "'"));
                    if (ft != null)
                    {
                        if (ft["mapid"] != null && ft["mapid"].ToString() != "")
                        {
                            selPrinx = Convert.ToInt32(ft["mapid"]);
                        }
                        selMapID = sID;  //??????????????????????
                        ////闪烁要素
                        flashFt = ft;
                        defaultStyle = ft.Style;
                        k = 0;
                        timer1.Start();
                    }
                    //填充信息DatagridView
                    rowIndex = e.RowIndex;
                    GafillInfoDatagridView(this.dataGridViewGaList.Rows[e.RowIndex]);
                    btnGaSave.Enabled = false;
                    btnGaSave.Text = "更新";
                    btnGaCancel.Enabled = false;
                    btnGaCancel.Text = "取消";
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaList_CellClick");
                System.Windows.Forms.MessageBox.Show("此对象未定位.");
            }
            //设置gridview的间隔颜色
            for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
            {
                if (i % 2 == 1)
                {
                    dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                }
                else
                {
                    dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                }
            }
        }

        private void GafillInfoDatagridView(DataGridViewRow dataGridViewRow)
        {
            try
            {
                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                {
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "照片")
                        continue;
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "最后更新人")
                        this.dataGridViewGaInfo.Rows[i].Cells[1].ReadOnly = true;
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = dataGridViewRow.Cells[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GafillInfoDatagridView");
                MessageBox.Show(ex.Message, "GafillInfoDatagridView");
            }
            if (temEditDt.Rows[0]["业务数据可编辑"].ToString() != "1")  //不可编辑
            {
                this.dataGridViewGaInfo.Visible = true;
                this.dataGridViewGaInfo.ReadOnly = true;
                btnGaSave.Enabled = false;
                btnLoc3.Enabled = false;
            }
            else
            {
                string colName = "";
                this.dataGridViewGaInfo.Visible = true;
                this.dataGridViewGaInfo.Columns[1].ReadOnly = false;
                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                {
                    colName = this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                    if (colName == "最后更新人" || colName == "所属派出所代码" || colName == "所属中队代码" || colName == "所属警务室代码")
                        this.dataGridViewGaInfo.Rows[i].Cells[1].ReadOnly = true;
                }
                this.btnGaSave.Text = "更新";
                this.btnGaCancel.Text = "取消";
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;
            }
        }

        private void GaGetTable(string tableName)//从数据库中得到表
        {
            if (isOracleSpatialTab)
            {
                MIConnection miConnection = new MIConnection();
                try
                {
                    if (this.lTable != null)
                    {
                        this.lTable.Close();
                    }
                    miConnection.Open();

                    //之前有空间表打不开, 所以用了多次打开.
                    try
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select  *  From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;

                        if (miConnection.Catalog.GetTable(tableName) == null)
                            this.lTable = miConnection.Catalog.OpenTable(ti);
                        else
                            this.lTable = miConnection.Catalog.GetTable(tableName);

                    }
                    catch
                    {
                        TableInfoServer ti = new TableInfoServer(tableName, "SRVR=" + datasource + ";UID=" + userid + ";PWD=" + password, "Select * From " + tableName, ServerToolkit.Oci);
                        ti.CacheSettings.CacheType = CacheOption.Off;
                        if (miConnection.Catalog.GetTable(tableName) == null)
                            this.lTable = miConnection.Catalog.OpenTable(ti);
                        else
                            this.lTable = miConnection.Catalog.GetTable(tableName);
                    }

                    if (MapInfo.Engine.Session.Current.Catalog.GetTable(tableName + "_tem") == null)
                    {
                        MapInfo.Data.TableInfoNative ListTableInfo = new MapInfo.Data.TableInfoNative(tableName + "_tem");
                        ListTableInfo.Temporary = true;
                        MapInfo.Geometry.CoordSys LCoordsys;
                        MapInfo.Data.GeometryColumn GC = (MapInfo.Data.GeometryColumn)(this.lTable.TableInfo.Columns["obj"]);
                        LCoordsys = GC.CoordSys;
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateFeatureGeometryColumn(LCoordsys));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateIntColumn("mapid"));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("keyID", 100));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStringColumn("Name", 200));
                        ListTableInfo.Columns.Add(MapInfo.Data.ColumnFactory.CreateStyleColumn());
                        this.editTable = MapInfo.Engine.Session.Current.Catalog.CreateTable(ListTableInfo);
                    }
                    else
                        this.editTable = MapInfo.Engine.Session.Current.Catalog.GetTable(tableName + "_tem");

                    miConnection.Close();

                    //地图显示
                    FeatureLayer fl = new FeatureLayer(editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(editTable);//标注图层

                    if (temEditDt.Rows[0]["业务数据可编辑"].ToString() == "1")
                    {
                        MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    }
                    MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tableName + "_tem"], true);
                    mapControl1.Map.Center = mapControl1.Map.Center;
                }
                catch (Exception ex)
                {
                    if (miConnection.State == ConnectionState.Open)
                    {
                        miConnection.Close();
                    }
                    MessageBox.Show(ex.Message, "GaGetTable()");
                    writeToLog(ex, "GaGetTable");
                }
            }
            else
            {
                try
                {
                    if (this.editTable != null)
                    {
                        this.editTable.Close();
                    }
                    this.editTable = createTable(tableName);
                    FeatureLayer fl = new FeatureLayer(this.editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(this.editTable);//标注图层

                    if (this.editTable != null)
                    {
                        if (temEditDt.Rows[0]["业务数据可编辑"].ToString() == "1")
                        {
                            MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTable.Text.Trim()], true);
                        }
                        MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTable.Text], true);
                        mapControl1.Map.Center = mapControl1.Map.Center;
                    }
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "GaGetTable");
                    MessageBox.Show(ex.Message, "GaGetTable111");
                }
            }

            this.GaInitialEditFields(this.comboTable.Text);
        }

        private void GaInitialEditFields(string tabName)
        {
            OracleConnection Conn = new OracleConnection(mysqlstr);
            try
            {
                Conn.Open();
                OracleCommand cmd = new OracleCommand("SELECT COLUMN_NAME, NULLABLE,DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME= '" + tabName + "'", Conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                cmd.Dispose();
                Conn.Close();

                CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                string GaobjID = CLC.ForSDGA.GetFromTable.ObjID;   //  by fisher on 09-09-23
                dataGridViewGaInfo.Rows.Clear();
                int k = 0;
                int j = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string fieldName = dt.Rows[i][0].ToString();
                    if (fieldName.ToUpper() == "mapid" || fieldName.IndexOf("备用字段") > -1 || fieldName.ToUpper() == "MI_STYLE" || fieldName.ToUpper() == "MI_PRINX" || fieldName.ToUpper() == "GEOLOC" || fieldName.ToUpper() == "X" || fieldName.ToUpper() == "Y")
                    {
                        k++;
                        continue;
                    }
                    dataGridViewGaInfo.Rows.Add(1);//添加一行

                    dataGridViewGaInfo.Rows[i - k].Cells[0].Value = fieldName;
                    if (fieldName == CLC.ForSDGA.GetFromTable.XiaQuField)
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxJieZhen();
                    }
                    else if (fieldName == "所属中队")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxZhongdu();
                    }
                    else if (fieldName == "所属警务室")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxJinWuShi();
                    }
                    else if (fieldName == "重点人口")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1] = dgvComboBoxShiFou();
                    }
                    else
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[1].Value = "";
                    }

                    if (dt.Rows[i][1].ToString().ToUpper() == "N" || fieldName == GaobjID || fieldName == "标注人")
                    {
                        dataGridViewGaInfo.Rows[i - k].Cells[2].Value = "必填";
                    }

                    dataGridViewGaInfo.Rows[i - k].Cells[3].Value = dt.Rows[i][2].ToString().ToUpper();
                    j++;
                }
                //以下由fisher 修改（09-09-01）
                if (comboTable.Text == "人口系统")
                {
                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j].Cells[0].Value = "照片";
                    dataGridViewGaInfo.Rows[j].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[3].Value = "VARCHAR2";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 1].Cells[0].Value = "X";
                    dataGridViewGaInfo.Rows[j + 1].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[3].Value = "FLOAT";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 2].Cells[0].Value = "Y";
                    dataGridViewGaInfo.Rows[j + 2].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 2].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 2].Cells[3].Value = "FLOAT";
                }
                else if (comboTable.Text == "安全防护单位")
                {
                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j].Cells[0].Value = "X";
                    dataGridViewGaInfo.Rows[j].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[3].Value = "FLOAT";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 1].Cells[0].Value = "Y";
                    dataGridViewGaInfo.Rows[j + 1].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[3].Value = "FLOAT";
                    dataGridViewGaInfo.Rows.Add(1);

                    dataGridViewGaInfo.Rows[j + 2].Cells[0].Value = "安全防护单位文件";
                    DataGridViewLinkCell dgvlc = new DataGridViewLinkCell();
                    dgvlc.Value = "点击编辑";
                    dgvlc.ToolTipText = "点击编辑安全防护单位文件";
                    dataGridViewGaInfo.Rows[j + 2].Cells[1] = dgvlc;
                    dataGridViewGaInfo.Rows[j + 2].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 2].Cells[3].Value = "VARCHAR2";


                }
                else
                {
                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j].Cells[0].Value = "X";
                    dataGridViewGaInfo.Rows[j].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j].Cells[3].Value = "FLOAT";

                    dataGridViewGaInfo.Rows.Add(1);
                    dataGridViewGaInfo.Rows[j + 1].Cells[0].Value = "Y";
                    dataGridViewGaInfo.Rows[j + 1].Cells[1].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[2].Value = "";
                    dataGridViewGaInfo.Rows[j + 1].Cells[3].Value = "FLOAT";
                }
            }
            catch (Exception ex)
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
                writeToLog(ex, "GaInitialEditFields");
                MessageBox.Show(ex.Message, "GaInitialEditFields");
            }

            setDataGridViewColumnWidth(dataGridViewGaInfo);
        }

        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl3.SelectedTab == tabGaList)
                {
                    if (this.btnGaSave.Text == "保存" && this.btnGaCancel.Enabled == true)
                    {
                        MessageBox.Show("请完成上一条记录的属性添加和保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabControl3.SelectedTab = tabGaInfo;
                    }
                }
                else if (tabControl3.SelectedTab == tabGaInfo)
                {
                    if (this.dataGridViewGaInfo.Visible == false)
                    {
                        this.btnGaSave.Enabled = false;
                        this.btnGaCancel.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaInitialEditFields");
            }
        }

        private void dataGridViewGaInfo_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            return;
        }

        //dataGridViewGaInfo开始编辑
        private bool GahasUpdate = false; //判断照片数据是否更新

        private void dataGridViewGaInfo_CellEndEdit(object sender, DataGridViewCellEventArgs e) //添加语句，用来判断输入的格式正不正确
        {
            string type = "";//得到类型
            try
            {
                if (dataGridViewGaInfo.Rows[e.RowIndex].Cells[0].Value.ToString() == "身份证号码")
                {
                    if (dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value != null)
                    {
                        GaCheckIdentity(dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value.ToString());
                    }
                }
                else
                {
                    if (this.dataGridViewGaInfo.CurrentCell == null || this.dataGridViewGaInfo.CurrentCell.Value == null)
                    {
                        return;
                    }

                    string value = this.dataGridViewGaInfo.CurrentCell.Value.ToString().Trim();
                    type = dataGridViewGaInfo.Rows[dataGridViewGaInfo.CurrentCell.RowIndex].Cells[3].Value.ToString();
                    switch (type)
                    {
                        case "INTEGER":
                        case "LONG":
                            this.GacheckNumber(value);
                            break;
                        case "NUMBER":
                        case "FLOAT":
                        case "DOUBLE":
                            this.checkFloat(value);
                            break;
                    }
                }
                setSystemValue(null, userName, "最后更新人", dataGridViewGaInfo);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaInfo_CellEndEdit");
            }
        }

        /// <summary>
        /// 根据某字段自动生成值   lili 2010-9-26
        /// </summary>
        /// <param name="sql">查找到值的sql</param>
        /// <param name="telNo">要生成的值</param>
        /// <param name="valueName">对于哪列生成</param>
        private void setSystemValue(string sql, string telNo, string valueName, System.Windows.Forms.DataGridView data)
        {
            try
            {
                DataTable table = null;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                if (sql != null && sql != "")
                {
                    table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
                    telNo = table.Rows[0][0].ToString();
                }
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    if (data.Rows[i].Cells[0].Value.ToString() == valueName)
                    {
                        data.Rows[i].Cells[1].Value = telNo;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setSystemValue");
            }
        }

        private void dataGridViewGaInfo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                btnGaSave.Enabled = true;
                btnGaCancel.Enabled = true;

                if (dataGridViewGaInfo.Rows.Count > 2)
                {
                    string cellName = dataGridViewGaInfo.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string celStr = dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string sqlStr = "", telNo = "";

                    switch (cellName)
                    {
                        case "所属派出所":
                            sqlStr = "select 派出所代码 from 基层派出所 where 派出所名='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "所属派出所代码", dataGridViewGaInfo);
                            break;
                        case "所属中队":
                            sqlStr = "select 中队代码 from 基层民警中队 where 中队名='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "所属中队代码", dataGridViewGaInfo);
                            break;
                        case "所属警务室":
                            sqlStr = "select 警务室代码 from 社区警务室 where 警务室名='" + celStr + "'";
                            setSystemValue(sqlStr, telNo, "所属警务室代码", dataGridViewGaInfo);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaInfo_CellValueChanged");
            }
        }

        private void dataGridViewGaInfo_Resize(object sender, EventArgs e)
        {
            try
            {
                DataGridView dataGridView = (DataGridView)sender;
                if (dataGridView.Rows.Count > 0)
                {
                    setDataGridViewColumnWidth(dataGridView);
                }
                else
                {
                    dataGridView.Columns[1].Width = dataGridView.Width - 135;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaInfo_Resize");
            }
        }

        //判断主键是否重复，代码由fisher添加，09-08-28
        private bool GaisZhujian()
        {
            OracleConnection Conn = new OracleConnection(mysqlstr); //插入数据库
            Conn.Open();
            CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
            string Pdstr = "select " + CLC.ForSDGA.GetFromTable.ObjID + " from " + comboTable.Text;
            OracleCommand orcmd = new OracleCommand(Pdstr, Conn);
            OracleDataAdapter PdAdapter = new OracleDataAdapter(orcmd);
            DataSet Pdds = new DataSet();
            PdAdapter.Fill(Pdds);
            DataTable Pddt = Pdds.Tables[0];
            try
            {
                int ix = 0;
                for (int j = 0; j < dataGridViewGaInfo.Rows.Count; j++)
                {
                    if (dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString() == CLC.ForSDGA.GetFromTable.ObjID)
                    {
                        ix = j;
                        break;
                    }
                }
                if (Pddt.Rows.Count < 1)
                {
                    orcmd.Dispose();
                    Conn.Close();
                    return false;
                }
                else
                {
                    for (int i = 0; i < Pddt.Rows.Count; i++)
                    {
                        if (Pddt.Rows[i][0].ToString() == dataGridViewGaInfo.Rows[ix].Cells[1].Value.ToString())
                        {
                            orcmd.Dispose();
                            Conn.Close();
                            return true;
                        }
                    }
                }
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "GaisZhujian()");
                MessageBox.Show(ex.Message, "GaisZhujian()");
                orcmd.Dispose();
                Conn.Close();
                return false;
            }
        }

        //保存修改记录  （feng 2009-08-19）
        private void btnGaSave_Click(object sender, EventArgs e)
        {
            //先判断主键字段,如果输入的值已存在,给出提示
            OracleConnection Conn = new OracleConnection(mysqlstr); //插入数据库
            this.Cursor = Cursors.WaitCursor;
            this.dataGridViewGaInfo.CurrentCell = null;//在进行保存的时候让dataGridViewGaInfo失去焦点
            this.mapControl1.Focus();

            bool GaisorKey = false; //判断是否主键重复

            if (isOracleSpatialTab)
            {
                OracleDataReader dr = null;
                try
                {

                    Conn.Open();
                    OracleCommand cmd;
                    if (btnGaSave.Text == "保存")
                    {
                        GaisorKey = GaisZhujian();
                        if (GaisorKey)
                        {
                            MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            GaisorKey = false;
                            this.Cursor = Cursors.Default;
                            btnLoc3.Enabled = false;
                            return;
                        }
                        else
                        {
                            cmd = new OracleCommand("select max(MI_PRINX) from " + comboTable.Text, Conn);
                            dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                dr.Read();
                                if (dr.GetValue(0) != null)
                                {
                                    selPrinx = Convert.ToInt32(dr.GetValue(0)) + 1;
                                }
                            }
                            dr.Close();
                            cmd.Dispose();

                            feature = new Feature(lTable.TableInfo.Columns);
                            feature.Geometry = ft.Geometry;
                            feature.Style = ft.Style;

                            string strValue = "";
                            for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                            {
                                strValue = "";
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                }
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "照片")
                                {
                                    continue;
                                }
                                if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "必填")
                                {
                                    if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " 不能为空.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        return;
                                    }
                                }
                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    feature[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = Convert.ToDateTime(strValue);
                                }
                                else
                                {
                                    if (strValue == "")
                                    {
                                        continue;
                                    }
                                    //以下代码由fisher添加   （09-09-01）
                                    if (isOracleSpatialTab)
                                    {
                                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                        {
                                            continue;
                                        }
                                        if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            feature[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = strValue;
                                        }
                                    }
                                    else
                                    {
                                        feature[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()] = strValue;
                                    }
                                }
                            }
                            feature["MI_PRINX"] = selPrinx;

                            for (int i = 0; i < feature.Columns.Count; i++)
                            {
                                if (feature.Columns[i].Alias.ToUpper() == "mapid")
                                {
                                    feature["mapid"] = selPrinx;
                                    break;
                                }
                            }
                            lTable.InsertFeature(feature);


                            //由于feature只记录时间值的日期部分,所以再次更新date型值
                            string command = "update " + comboTable.Text + " set ";
                            strValue = "";
                            bool isUpdate = false;
                            for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                }

                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE" && strValue != "")
                                {
                                    DateTime dTime = Convert.ToDateTime(strValue);
                                    if (dTime.TimeOfDay.ToString() != "00:00:00")
                                    {
                                        command += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                        isUpdate = true;
                                    }
                                }
                            }
                            if (isUpdate)
                            {
                                command = command.Remove(command.Length - 1);
                                command += " where MI_PRINX=" + selPrinx;
                                cmd = new OracleCommand(command, Conn);
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                Conn.Close();
                            }

                            GaaddToList(feature.Geometry.Centroid.x, feature.Geometry.Centroid.y);

                            // 新添加的数据不允许马上就执行更新，因为会出错，只能切换到dataGridViewGaList选择某条记录后才能更新
                            // updated by fisher in 09-10-23
                            this.dataGridViewGaInfo.Columns[1].ReadOnly = true;

                            btnLoc3.Enabled = false;
                        }
                    }
                    else
                    {//更新
                        string command = "update " + comboTable.Text + " t set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                        {
                            strValue = "";
                            if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            }

                            if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "必填")
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                {
                                    MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " 不能为空.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Cursor = Cursors.Default;
                                    return;
                                }
                            }

                            if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() != "照片")
                            {
                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    if (strValue == "")     //update by fisher in 09-12-24
                                    { continue; }
                                    command += "t." + dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X = '0',";
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.X='" + strValue + "',";
                                    }
                                }
                                else if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                {
                                    if (strValue == "")
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y = '0',";
                                    }
                                    else
                                    {
                                        command += "t.GEOLOC.SDO_POINT.Y ='" + strValue + "',";
                                    }
                                }
                                else
                                {
                                    command += "t." + dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                                }
                            }
                        }
                        command = command.Remove(command.Length - 1);
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                        command += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                        cmd = new OracleCommand(command, Conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        Conn.Close();

                        //设置鼠标状态 fisher
                        this.mapControl1.Tools.LeftButtonTool = "Pan";
                        this.UncheckedTool();
                        btnLoc3.Enabled = false;
                        GaupdateListValue();
                    }
                    //更新地图要素的属性
                    GaupdateMapValue();

                }
                catch (OracleException ex)
                {
                    if (ex.Code == 1)//主键字段输入了非唯一值
                    {
                        MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "btnGaSave_Click");
                    }
                    this.Cursor = Cursors.Default;
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示");
                    writeToLog(ex, "btnGaSave_Click");
                    this.Cursor = Cursors.Default;
                    return;
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                        Conn.Close();
                }
                if (comboTable.Text.Trim() == "人口系统")
                {
                    if (GahasUpdate)//判断是否已经更新了数据
                    {
                        Conn = new OracleConnection(mysqlstr); //插入数据库
                        OracleCommand cmd;
                        Conn.Open();
                        string strValue = "";

                        try
                        {
                            string strExe = "";
                            strValue = "";
                            for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "身份证号码")
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                    break;
                                }
                            }
                            if (strValue != "")
                            {
                                if (btnGaSave.Text == "保存")
                                {
                                    strExe = "insert into 人口照片(身份证号码,照片) values('" + strValue + "' ,:imgData)";
                                }
                                else
                                {
                                    //先查询,看看人口照片中有没有对应的对象
                                    cmd = new OracleCommand("select * from 人口照片 where 身份证号码='" + strValue + "'", Conn);
                                    OracleDataReader oDr = cmd.ExecuteReader();
                                    if (oDr.HasRows)
                                    {
                                        strExe = "update 人口照片 set 照片=:imgData where 身份证号码='" + strValue + "'";
                                    }
                                    else
                                    {
                                        strExe = "insert into 人口照片(身份证号码,照片) values('" + strValue + "' ,:imgData)";
                                    }
                                    oDr.Close();
                                }
                                cmd = new OracleCommand(strExe, Conn);

                                if (fileName == "")
                                {
                                    fileName = Application.StartupPath + "\\默认.bmp";
                                }

                                FileStream fs = File.OpenRead(fileName);
                                byte[] imgData = new byte[fs.Length];
                                long n = fs.Read(imgData, 0, imgData.Length);
                                cmd.Parameters.Add(":ImgData", OracleType.LongRaw).Value = imgData;
                                fs.Close();

                                cmd.ExecuteNonQuery();
                                btnLoc3.Enabled = false;
                                cmd.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            writeToLog(ex, "btnGaSave_Click");
                            MessageBox.Show(ex.Message, "btnGaSave_Click,110");
                        }
                        finally
                        {
                            if (Conn.State == ConnectionState.Open)
                                Conn.Close();
                        }
                    }
                }
            }

            else  //非oracleSpatial表
            {
                try
                {
                    Conn.Open();
                    OracleCommand cmd;
                    string strExe = "";
                    if (btnGaSave.Text == "保存")
                    {
                        GaisorKey = GaisZhujian();
                        if (GaisorKey)
                        {
                            MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            GaisorKey = false;
                            this.Cursor = Cursors.Default;
                            return;
                        }
                        else
                        {
                            strExe = "insert into " + comboTable.Text + "(";

                            for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "安全防护单位文件")
                                    continue;
                                if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "必填")
                                {
                                    if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                    {
                                        MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " 不能为空.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Cursor = Cursors.Default;
                                        return;
                                    }
                                }
                                if (i == 0)
                                    strExe += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                                else
                                    strExe += "," + dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString();
                            }

                            strExe += ") values(";

                            string strValue = "";
                            for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                            {
                                strValue = "";
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                                {
                                    strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                                }
                                else
                                {
                                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                    {
                                        strValue = "0";
                                    }
                                    if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                    {
                                        strValue = "0";
                                    }
                                }
                                if (strValue == "点击编辑")
                                    continue;
                                if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE")
                                {
                                    strExe += "to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                                }
                                else
                                {
                                    strExe += "'" + strValue + "',";
                                }
                            }

                            //strExe += dx + "," + dy + ")";
                            strExe = strExe.Remove(strExe.Length - 1);
                            strExe += ")";

                            selMapID = mapId.ToString();

                            // 新添加的数据不允许马上就执行更新，因为会出错，只能切换到dataGridViewGaList选择某条记录后才能更新
                            // updated by fisher in 09-10-23
                            this.dataGridViewGaInfo.Columns[1].ReadOnly = true;

                            GaaddToList(dx, dy);
                        }
                    }
                    else
                    {   //更新
                        CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                        strExe = "update " + comboTable.Text + " set ";
                        string strValue = "";
                        for (int i = 0; i < dataGridViewGaInfo.RowCount; i++)
                        {
                            //update by siumo 09-01-08, 当为null时,toString出错
                            strValue = "";
                            if (dataGridViewGaInfo.Rows[i].Cells[1].Value != null)
                            {
                                strValue = dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString();
                            }
                            else
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                                    strValue = "0";
                                if (dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                                    strValue = "0";
                            }
                            if (strValue == "点击编辑")
                                continue;
                            if (dataGridViewGaInfo.Rows[i].Cells[2].Value != null && dataGridViewGaInfo.Rows[i].Cells[2].Value.ToString() == "必填")
                            {
                                if (dataGridViewGaInfo.Rows[i].Cells[1].Value == null || dataGridViewGaInfo.Rows[i].Cells[1].Value.ToString() == "")
                                {
                                    MessageBox.Show(dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + " 不能为空.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Cursor = Cursors.Default;
                                    return;
                                }
                            }
                            if (this.dataGridViewGaInfo.Rows[i].Cells[3].Value.ToString() == "DATE" && strValue != "")
                            {
                                strExe += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "=to_date('" + strValue + "','yy-mm-dd hh24:mi:ss'),";
                            }
                            else
                            {
                                strExe += dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() + "='" + strValue + "',";
                            }
                        }
                        strExe = strExe.Remove(strExe.Length - 1);
                        strExe += " where " + CLC.ForSDGA.GetFromTable.ObjID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[CLC.ForSDGA.GetFromTable.ObjID].Value.ToString() + "'";
                        btnLoc3.Enabled = false;

                    }
                    cmd = new OracleCommand(strExe, Conn);
                    cmd.ExecuteNonQuery();
                    GaupdateListValue();
                    cmd.Dispose();
                    Conn.Close();
                }
                catch (OracleException ex)
                {
                    if (ex.Code == 1)//主键字段输入了非唯一值
                    {
                        MessageBox.Show("数据库中已存在此编号的数据,请修改;\r\r如果要更新此编号数据,请先进行查询!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        writeToLog(ex, "btnGaSave_Click");
                    }
                    this.Cursor = Cursors.Default;
                    btnLoc3.Enabled = false;
                    return;
                }
                finally
                {
                    if (Conn.State == ConnectionState.Open)
                        Conn.Close();
                }

                //更新地图要素的属性
                GaupdateMapValue();

            }
            string editMothed = "添加";
            if (btnGaSave.Text == "更新")
            {
                editMothed = "修改属性";
                MessageBox.Show("更新成功!");
            }
            else
            {
                MessageBox.Show("添加数据成功!");
                btnGaSave.Text = "更新";
                btnGaCancel.Text = "取消";
            }

            //记录编辑log
            WriteEditLog(comboTable.Text.Trim(), selMapID, editMothed);
            this.dataGridViewGaInfo.CurrentCell = null;
            btnGaSave.Enabled = false;
            btnGaCancel.Enabled = false;
            this.mapControl1.Tools.LeftButtonTool = "Pan";
            this.UncheckedTool();
            try
            {
                string tabName = comboTable.Text.Trim();
                if (isOracleSpatialTab)
                {
                    tabName += "_tem";
                }
                if (temEditDt.Rows[0]["业务数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[tabName], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[tabName], true);
            }
            catch (Exception ex) { writeToLog(ex, "btnGaSave_Click"); }
            this.Cursor = Cursors.Default;
            this.dataGridViewGaList.ContextMenuStrip = null;
            btnLoc3.Enabled = false;
        }

        private void btnGaCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnGaCancel.Text == "取消添加")
                {
                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++) //清空datagridview
                    {
                        dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                    }
                    dataGridViewGaInfo.Visible = false;
                    btnGaSave.Enabled = false;
                    btnGaCancel.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();

                    GadeleteFeature();
                    this.mapControl1.Focus();
                }
                else if (btnGaCancel.Text == "取消")
                {
                    try
                    {
                        for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                        {
                            if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "照片")
                                continue;
                            this.dataGridViewGaInfo.Rows[i].Cells[1].Value = this.dataGridViewGaList.Rows[rowIndex].Cells[dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString()].Value.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        MessageBox.Show(ex.Message, "btnGaCancel_Click(取消)");
                        btnLoc3.Enabled = false;
                    }

                    //地图上的点位置复原
                    Feature f;

                    //added by fisher in 09-09-22

                    if (this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() == "" || this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() == "" || this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value.ToString() == "0" || this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value.ToString() == "0")
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        btnGaSave.Enabled = false;
                        btnGaCancel.Enabled = false;
                        this.mapControl1.Tools.LeftButtonTool = "Pan";
                        this.UncheckedTool();
                        btnLoc3.Enabled = false;
                        return;
                        //删除后，跳出！ 
                    }

                    if (Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 2].Cells[1].Value) != 0 && Convert.ToDouble(this.dataGridViewGaInfo.Rows[dataGridViewGaInfo.Rows.Count - 1].Cells[1].Value) != 0)
                    {
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                        //先删除
                    }

                    //添加点
                    CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                    string sName = CLC.ForSDGA.GetFromTable.ObjName;

                    f = new Feature(this.editTable.TableInfo.Columns);
                    f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(Convert.ToDouble(this.dataGridViewGaList.Rows[rowIndex].Cells["X"].Value), Convert.ToDouble(this.dataGridViewGaList.Rows[rowIndex].Cells["Y"].Value)));
                    f["mapid"] = this.dataGridViewGaList.Rows[rowIndex].Cells["mapid"].Value.ToString();
                    f["name"] = this.dataGridViewGaList.Rows[rowIndex].Cells[sName].Value.ToString();
                    f.Style = featStyle;
                    this.editTable.InsertFeature(f);

                    this.dataGridViewGaInfo.CurrentCell = null;
                    this.btnLoc3.Enabled = false;
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    this.UncheckedTool();
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnGaCancel_Click");
                MessageBox.Show(ex.Message, "btnGaCancel_Click");
            }
        }

        private void mapToolBar2_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button == GaAddPoint)
                {
                    this.UncheckedTool();
                }
                if (e.Button == GaPointStyle)
                {
                    SymbolStyleDlg pStyleDlg = new SymbolStyleDlg();
                    if (pStyleDlg.ShowDialog() == DialogResult.OK)
                    {
                        featStyle = pStyleDlg.SymbolStyle;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapToolBar2_ButtonClick");
                MessageBox.Show(ex.Message, "mapToolBar2_ButtonClick");
            }
        }

        private void comboTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridViewGaInfo.Visible && btnGaCancel.Enabled)
            {
                MessageBox.Show("请完成上一条记录的属性添加和保存!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void bindingNavigatorGa_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                tabName = CLC.ForSDGA.GetFromTable.TableName;
                int countShu = Convert.ToInt32(this.toolStripTextBox1.Text);
                if (e.ClickedItem.Text == "上一页")
                {
                    if (pageCurrent <= 1)
                    {
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        if (_first == nMax)
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _first = nMax - (nMax % countShu);
                            _end = nMax - (nMax % countShu) + 1 - countShu;
                        }
                        else
                        {
                            pageCurrent--;
                            nCurrent = pageSize * (pageCurrent - 1);
                            _first -= countShu;
                            _end -= countShu;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "下一页")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent++;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _first += countShu;
                        _end += countShu;
                    }
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (pageCurrent <= 1)
                    {
                        MessageBox.Show("已经是首页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = 1;
                        nCurrent = 0;
                        _first = countShu;
                        _end = 1;
                    }
                }
                else if (e.ClickedItem.Text == "转到尾页")
                {
                    if (pageCurrent > pageCount - 1)
                    {
                        MessageBox.Show("已经是尾页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageCurrent = pageCount;
                        nCurrent = pageSize * (pageCurrent - 1);
                        _first = nMax;
                        _end = nMax - (nMax % countShu) + 1;
                    }
                }
                else
                {
                    return;
                }

                isShowPro(true);
                this.PageNow.Text = pageCurrent.ToString();//设置当前页

                //以下代码用于填充datagridview（feng）
                this.Cursor = Cursors.WaitCursor;
                dataGridViewGaList.Columns.Clear();
                DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //获取当前页数据; lili 2010-8-26
                dataGridViewGaList.DataSource = datatable;
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                //dataGridViewList.Visible = true;
                //设置gridview的间隔颜色
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;

                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                }
                this.Cursor = Cursors.Default;
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "bindingNavigatorGa_ItemClicked()");
                MessageBox.Show(ex.Message, "bindingNavigatorGa_ItemClicked()");
                this.Cursor = Cursors.Default;
            }
        }

        private void PageNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                try
                {
                    if (Convert.ToInt32(this.PageNow.Text) < 1 || Convert.ToInt32(this.PageNow.Text) > pageCount)
                    {
                        MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.PageNow.Text = pageCurrent.ToString();
                        return;
                    }
                    else
                    {
                        isShowPro(true);
                        this.pageCurrent = Convert.ToInt32(this.PageNow.Text);
                        nCurrent = pageSize * (pageCurrent - 1);
                        _end = ((pageCurrent - 1) * pageSize) + 1;
                        _first = _end + pageSize - 1;

                        //以下代码用于填充datagridview（feng）
                        this.Cursor = Cursors.WaitCursor;
                        dataGridViewGaList.Columns.Clear();
                        DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //获取当前页数据
                        dataGridViewGaList.DataSource = datatable;
                        this.toolEditPro.Value = 1;
                        Application.DoEvents();

                        if (dataGridViewGaList.Columns["mapid"] != null)
                        {
                            dataGridViewGaList.Columns["mapid"].Visible = false;
                        }
                        //设置gridview的间隔颜色
                        for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                        {
                            if (i % 2 == 1)
                            {
                                dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                            }
                            else
                            {
                                dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                            }
                        }
                        this.toolEditPro.Value = 2;
                        this.insertGaQueryIntoTable(datatable);
                        this.dataGridViewGaInfo.Visible = false;
                        this.btnGaSave.Enabled = false;
                        this.btnGaCancel.Enabled = false;

                        for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//清空datagridview
                        {
                            this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                        }
                        this.Cursor = Cursors.Default;
                        this.toolEditPro.Value = 3;
                        Application.DoEvents();
                        isShowPro(false);
                    }
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "PageNow_KeyPress()");
                }
            }
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r" && this.dataGridViewGaList.DataSource != null)
            {
                try
                {
                    isShowPro(true);
                    this.Cursor = Cursors.WaitCursor;
                    this.pageSize = Convert.ToInt32(this.toolStripTextBox1.Text);
                    this.pageCount = (nMax / pageSize);//计算出总页数
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//设置总页数
                    if (nMax != 0)
                    {
                        this.pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//设置当前页
                    }
                    this.nCurrent = 0;       //当前记录数从0开始
                    _first = pageSize;
                    _end = 1;

                    //以下代码用于填充datagridview（feng）
                    this.Cursor = Cursors.WaitCursor;
                    dataGridViewGaList.Columns.Clear();
                    DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //获取当前页数据
                    dataGridViewGaList.DataSource = datatable;
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();

                    if (dataGridViewGaList.Columns["mapid"] != null)
                    {
                        dataGridViewGaList.Columns["mapid"].Visible = false;
                    }
                    //dataGridViewList.Visible = true;
                    //设置gridview的间隔颜色
                    for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                        else
                        {
                            dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                        }
                    }
                    this.toolEditPro.Value = 2;

                    this.insertGaQueryIntoTable(datatable);
                    this.dataGridViewGaInfo.Visible = false;
                    this.btnGaSave.Enabled = false;
                    this.btnGaCancel.Enabled = false;

                    for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//清空datagridview
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                    }
                    this.Cursor = Cursors.Default;
                    this.toolEditPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        private void tsTextBoxPageNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                try
                {
                    if (Convert.ToInt32(this.tsTextBoxPageNow.Text) < 1 || Convert.ToInt32(this.tsTextBoxPageNow.Text) > bpageCount)
                    {
                        MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();
                        return;
                    }
                    else
                    {
                        isShowPro(true);
                        this.Cursor = Cursors.WaitCursor;
                        this.bpageCurrent = Convert.ToInt32(this.tsTextBoxPageNow.Text);
                        this.bnCurrent = bpageSize * (bpageCurrent - 1);       //当前记录数从0开始

                        //以下代码用于填充datagridview（feng）
                        this.Cursor = Cursors.WaitCursor;
                        dataGridViewList.Columns.Clear();

                        DataTable datatable = _exportDT = BJLoadData(bPageSQL); //获取当前页数据
                        dataGridViewList.DataSource = datatable;
                        this.toolEditPro.Value = 1;
                        Application.DoEvents();

                        if (dataGridViewList.Columns["mapid"] != null)
                        {
                            dataGridViewList.Columns["mapid"].Visible = false;
                        }
                        //设置gridview的间隔颜色
                        setDataGridBG();
                        this.toolEditPro.Value = 2;

                        this.insertQueryIntoTable(datatable);
                        this.dataGridView1.Visible = false;
                        this.buttonSave.Enabled = false;
                        this.buttonCancel.Enabled = false;

                        for (int i = 0; i < dataGridView1.Rows.Count; i++)//清空datagridview
                        {
                            this.dataGridView1.Rows[i].Cells[1].Value = "";
                        }
                        this.Cursor = Cursors.Default;
                        this.toolEditPro.Value = 3;
                        Application.DoEvents();
                    }
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        private void bindingNavigatorEdit_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Text == "上一页")
                {
                    if (bpageCurrent <= 1)
                    {
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent--;
                        bnCurrent = bpageSize * (bpageCurrent - 1);
                    }
                }
                else if (e.ClickedItem.Text == "下一页")
                {
                    if (bpageCurrent > bpageCount - 1)
                    {
                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent++;
                        bnCurrent = bpageSize * (bpageCurrent - 1);
                    }
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (bpageCurrent <= 1)
                    {
                        MessageBox.Show("已经是首页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent = 1;
                        bnCurrent = 0;
                    }
                }
                else if (e.ClickedItem.Text == "转到末页")
                {
                    if (bpageCurrent > bpageCount - 1)
                    {
                        MessageBox.Show("已经是尾页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        bpageCurrent = bpageCount;
                        bnCurrent = bpageSize * (bpageCurrent - 1);
                    }
                }
                else
                {
                    return;
                }

                isShowPro(true);
                this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//设置当前页

                //以下代码用于填充datagridview（feng）
                this.Cursor = Cursors.WaitCursor;
                dataGridViewList.Columns.Clear();

                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //获取当前页数据
                dataGridViewList.DataSource = datatable;
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                if (dataGridViewList.Columns["mapid"] != null)
                {
                    dataGridViewList.Columns["mapid"].Visible = false;
                }
                //设置gridview的间隔颜色
                setDataGridBG();
                this.toolEditPro.Value = 2;

                this.insertQueryIntoTable(datatable);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                this.Cursor = Cursors.Default;
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "bindingNavigatorGa_ItemClicked()");
                MessageBox.Show(ex.Message, "bindingNavigatorGa_ItemClicked()");
                this.Cursor = Cursors.Default;
            }
        }

        private void toolStripPageSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r" && this.dataGridViewList.DataSource != null)
            {
                try
                {
                    isShowPro(true);
                    this.Cursor = Cursors.WaitCursor;
                    this.bpageSize = Convert.ToInt32(this.toolStripPageSize.Text);
                    this.bpageCount = (bnMax / bpageSize);//计算出总页数
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//设置总页数
                    if (bnMax != 0)
                    {
                        this.bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//设置当前页
                    }
                    this.bnCurrent = 0;       //当前记录数从0开始


                    //以下代码用于填充datagridview（feng）
                    this.Cursor = Cursors.WaitCursor;
                    dataGridViewList.Columns.Clear();

                    DataTable datatable = _exportDT = BJLoadData(bPageSQL); //获取当前页数据
                    dataGridViewList.DataSource = datatable;
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();

                    if (dataGridViewList.Columns["mapid"] != null)
                    {
                        dataGridViewList.Columns["mapid"].Visible = false;
                    }
                    //设置gridview的间隔颜色
                    setDataGridBG();
                    this.toolEditPro.Value = 2;

                    this.insertQueryIntoTable(datatable);
                    this.dataGridView1.Visible = false;
                    this.buttonSave.Enabled = false;
                    this.buttonCancel.Enabled = false;

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)//清空datagridview
                    {
                        this.dataGridView1.Rows[i].Cells[1].Value = "";
                    }
                    this.Cursor = Cursors.Default;
                    this.toolEditPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        private void toolStripPageSize_Click(object sender, EventArgs e)
        {

        }

        //已定位代码(fisher09-08-31)
        private void btnGaLocatedYes_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
            tabName = CLC.ForSDGA.GetFromTable.TableName;
            _mySQL = "";
            _PageSQL = "";
            _first = Convert.ToInt32(this.toolStripTextBox1.Text);
            _end = 1;
            string getSqlStr = getSqlString();
            if (dataGridViewGaList.DataSource == null || dataGridViewGaList.Visible == false || getSqlStr == "")
            {
                if (isOracleSpatialTab)
                {
                    PageSQL = "select count(*) from " + tabName + " t" + " where (t.geoloc.SDO_POINT.X is not null and t.geoloc.SDO_POINT.Y is not null and t.geoloc.SDO_POINT.X != '0' and t.geoloc.SDO_POINT.Y != '0') or 备用字段一 is not null ";
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is not null and a.geoloc.SDO_POINT.Y is not null and a.geoloc.SDO_POINT.X != '0' and a.geoloc.SDO_POINT.Y != '0')  or 备用字段一 is not null) ";

                }
                else
                {
                    PageSQL = "select count(*) from " + tabName + " where (X is not null and Y is not null and X != '0' and Y != '0') or 备用字段一 is not null";
                    _mySQL = " and ((a.X is not null and a.Y is not null and a.X != '0' and a.Y != '0')  or 备用字段一 is not null) ";
                }

                if (strRegion != "顺德区" && strRegion != "")  //edit by fisher in 09-12-01
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                {
                    if (_PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }
            else
            {
                if (isOracleSpatialTab)
                {
                    PageSQL = "select count(*) from " + tabName + " t" + " where ((t.geoloc.SDO_POINT.X is not null and t.geoloc.SDO_POINT.Y is not null and t.geoloc.SDO_POINT.X != '0' and t.geoloc.SDO_POINT.Y != '0') or 备用字段一 is not null) and " + getSqlStr;
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is not null and a.geoloc.SDO_POINT.Y is not null and a.geoloc.SDO_POINT.X != '0' and a.geoloc.SDO_POINT.Y != '0') or 备用字段一 is not null)";
                }
                else
                {
                    PageSQL = "select count(*) from " + tabName + " where ((X is not null and Y is not null and X != '0' and Y != '0') or 备用字段一 is not null) and " + getSqlStr;
                    _mySQL = " and ((a.X is not null and a.Y is not null and a.X != '0' and a.Y != '0') or 备用字段一 is not null) ";
                }

                if (strRegion != "顺德区" && strRegion != "")  //edit by fisher in 09-12-01
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                {
                    if (_PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }

            this.Cursor = Cursors.WaitCursor;
            try
            {
                conn.Open();
                PageSQL += _PageSQL;
                OracleCommand cmd = new OracleCommand(PageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                dAdapter.Fill(dt);
                cmd.Dispose();
                conn.Close();
                if (Convert.ToInt32(dt.Rows[0][0]) < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.RecordCount.Text = "0条";
                    this.PageNow.Text = "0";//设置当前页
                    this.lblPageCount.Text = "/ {0}";//设置总页数
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    return;
                }

                //以下代码用来设置翻页
                try
                {
                    nMax = Convert.ToInt32(dt.Rows[0][0]);
                    this.RecordCount.Text = nMax.ToString() + "条";
                    pageSize = 100;      //设置页面行数
                    pageCount = (nMax / pageSize);//计算出总页数
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//设置总页数
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    if (nMax != 0)
                    {
                        pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//设置当前页
                    }
                    nCurrent = 0;       //当前记录数从0开始
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnGaLocatedYes_Click 获取分页参数");
                    MessageBox.Show(ex.Message, "btnGaLocatedYes_Click 获取分页参数");
                }
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //获取当前页数据
                dataGridViewGaList.DataSource = datatable;

                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                //设置gridview的间隔颜色
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;
                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                }
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex, "btnGaLocatedYes_Click");
            }

            this.Cursor = Cursors.Default;
        }

        //未定位代码(fisher09-08-31)
        private void btnGaLocatedNo_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
            string tabName = CLC.ForSDGA.GetFromTable.TableName;
            string frmStr = CLC.ForSDGA.GetFromTable.FrmFields; // 用于显示认领、指派功能的数据列
            string frmSql = "";                                 // 用于显示认领、指派功能的SQL

            _mySQL = "";
            _PageSQL = "";
            _first = Convert.ToInt32(this.toolStripTextBox1.Text);
            _end = 1;
            string getSqlStr = getSqlString();

            if (dataGridViewGaList.DataSource == null || dataGridViewGaList.Visible == false || getSqlStr == "")
            {
                if (isOracleSpatialTab)
                {
                    PageSQL = "select count(*) from " + tabName + " t" + " where (t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') and 备用字段一 is null";
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is null or a.geoloc.SDO_POINT.Y is null or a.geoloc.SDO_POINT.X = '0' or a.geoloc.SDO_POINT.Y = '0') and 备用字段一 is null)";
                }
                else
                {
                    PageSQL = "select count(*)  from " + tabName + " where (X is null or Y is null or X = '0' or Y = '0') and 备用字段一 is null";
                    _mySQL = " and ((a.X is null or a.Y is null or a.X = '0' or a.Y = '0') and 备用字段一 is null)";
                }

                if (strRegion == "" && strRegion1 == "")
                {
                    isShowPro(false);
                    MessageBox.Show("你没有查询权限！");
                    return;
                }
                if (strRegion != "顺德区" && strRegion != "")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                {
                    if (PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }
            else
            {
                if (isOracleSpatialTab)
                {
                    PageSQL = "select count(*) from " + tabName + " t" + " where ((t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') and 备用字段一 is null) and " + getSqlStr;
                    _mySQL = " and ((a.geoloc.SDO_POINT.X is null or a.geoloc.SDO_POINT.Y is null or a.geoloc.SDO_POINT.X = '0' or a.geoloc.SDO_POINT.Y = '0')  and 备用字段一 is null) ";
                }
                else
                {
                    PageSQL = "select count(*)  from " + tabName + " where ((X is null or Y is null or X = '0' or Y = '0') and 备用字段一 is null) and " + getSqlStr;
                    _mySQL = " and ((a.X is null or a.Y is null or a.X = '0' or a.Y = '0') and 备用字段一 is null )";
                }

                if (strRegion != "顺德区" && strRegion != "")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                }
                if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                {
                    if (PageSQL.IndexOf("and") > -1)
                    {
                        _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                        _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }
                _mySQL += _PageSQL;
            }

            this.Cursor = Cursors.WaitCursor;
            try
            {
                conn.Open();
                PageSQL += _PageSQL;
                OracleCommand cmd = new OracleCommand(PageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                dAdapter.Fill(dt);
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();
                if (Convert.ToInt32(dt.Rows[0][0]) < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.pageCount = 0;
                    this.pageCurrent = 0;
                    this.RecordCount.Text = "0条";
                    this.PageNow.Text = "0";//设置当前页
                    this.lblPageCount.Text = "/ {0}";//设置总页数
                    this.toolStripTextBox1.Text = pageSize.ToString();

                    this.dataGridViewGaList.Columns.Clear();
                    this.dataGridViewGaInfo.Visible = false;

                    return;
                }

                //以下代码用来设置翻页
                try
                {
                    nMax = Convert.ToInt32(dt.Rows[0][0]);
                    this.RecordCount.Text = nMax.ToString() + "条";
                    pageSize = 100;      //设置页面行数
                    pageCount = (nMax / pageSize);//计算出总页数
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//设置总页数
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    if (nMax != 0)
                    {
                        pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//设置当前页
                    }
                    nCurrent = 0;       //当前记录数从0开始
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnGaLocatedNo_Click 获取分页参数");
                    MessageBox.Show(ex.Message, "btnGaLocatedNo_Click 获取分页参数");
                }
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, tabName); //获取当前页数据
                dataGridViewGaList.DataSource = datatable;

                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                //设置gridview的间隔颜色
                for (int i = 0; i < dataGridViewGaList.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewGaList.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;
                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                }
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex, "btnGaLocatedNo_Click");
                MessageBox.Show(ex.Message, "btnGaLocatedNo_Click 设置");
            }

            this.Cursor = Cursors.Default;
        }

        private void btnLocationNoClick(object sender, EventArgs e)
        {
            try
            {
                //OracleConnection conn = new OracleConnection(mysqlstr);
                //CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                //string tabName = CLC.ForSDGA.GetFromTable.TableName;
                //_mySQL = "";
                //_PageSQL = "";
                //_first = Convert.ToInt32(this.toolStripTextBox1.Text);
                //_end = 1;
                //string getSqlStr = getSqlString();

                //if (dataGridViewGaList.DataSource == null || dataGridViewGaList.Visible == false || getSqlStr == "")
                //{
                //    if (isOracleSpatialTab)
                //    {
                //        PageSQL = "select count(*) from " + tabName + " t" + " where (t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') and 备用字段一 is null";
                //        _mySQL = " and ((a.geoloc.SDO_POINT.X is null or a.geoloc.SDO_POINT.Y is null or a.geoloc.SDO_POINT.X = '0' or a.geoloc.SDO_POINT.Y = '0') and 备用字段一 is null)";
                //    }
                //    else
                //    {
                //        PageSQL = "select count(*)  from " + tabName + " where (X is null or Y is null or X = '0' or Y = '0') and 备用字段一 is null";
                //        _mySQL = " and ((a.X is null or a.Y is null or a.X = '0' or a.Y = '0') and 备用字段一 is null)";
                //    }

                //    if (strRegion == "" && strRegion1 == "")
                //    {
                //        isShowPro(false);
                //        MessageBox.Show("你没有查询权限！");
                //        return;
                //    }
                //    if (strRegion != "顺德区" && strRegion != "")
                //    {
                //        if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                //        {
                //            strRegion = strRegion.Replace("大良", "大良,德胜");
                //        }
                //        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                //    }
                //    if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                //    {
                //        if (PageSQL.IndexOf("and") > -1)
                //        {
                //            _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                //            _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                //        }
                //        else
                //        {
                //            _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                //        }
                //    }
                //    _mySQL += _PageSQL;
                //}
                //else
                //{
                //    if (isOracleSpatialTab)
                //    {
                //        PageSQL = "select count(*) from " + tabName + " t" + " where ((t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') and 备用字段一 is null) and " + getSqlStr;
                //        _mySQL = " and ((a.geoloc.SDO_POINT.X is null or a.geoloc.SDO_POINT.Y is null or a.geoloc.SDO_POINT.X = '0' or a.geoloc.SDO_POINT.Y = '0')  and 备用字段一 is null) ";
                //    }
                //    else
                //    {
                //        PageSQL = "select count(*)  from " + tabName + " where ((X is null or Y is null or X = '0' or Y = '0') and 备用字段一 is null) and " + getSqlStr;
                //        _mySQL = " and ((a.X is null or a.Y is null or a.X = '0' or a.Y = '0') and 备用字段一 is null )";
                //    }

                //    if (strRegion != "顺德区" && strRegion != "")
                //    {
                //        if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                //        {
                //            strRegion = strRegion.Replace("大良", "大良,德胜");
                //        }
                //        _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";
                //    }
                //    if (strRegion1 != "" && (tabName == "案件信息" || tabName == "公共场所"))
                //    {
                //        if (PageSQL.IndexOf("and") > -1)
                //        {
                //            _PageSQL = _PageSQL.Remove(_PageSQL.LastIndexOf(")"));
                //            _PageSQL += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                //        }
                //        else
                //        {
                //            _PageSQL += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                //        }
                //    }
                //    _mySQL += _PageSQL;

                //}

               // frmNoLocation noLocation = new frmNoLocation();
                //noLocation.Show();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnLocationNoClick");
            }
        }

        private void btnLocatedYes_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
            Cursor.Current = Cursors.WaitCursor;

            if (dataGridViewList.DataSource == null || dataGridViewList.Visible == false || blctStr == "" || textKeyWord.Text == "")  //updated by fisher in 09-10-10
            {
                bPageSQL = "select rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " where (X is not null and Y is not null and X != '0' and Y != '0')";

                if (strRegion != "顺德区")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    bPageSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                }
            }
            else
            {
                bPageSQL = blctStr + " and (X is not null and Y is not null and X != '0' and Y != '0')";
            }
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(bPageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.toolStripRecord.Text = "0条";
                    this.tsTextBoxPageNow.Text = "0";//设置当前页
                    this.tStripLabelPageCount.Text = "/ {0}";//设置总页数
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    return;
                }

                //以下代码用来设置翻页
                try
                {
                    bnMax = dt.Rows.Count;
                    this.toolStripRecord.Text = bnMax.ToString() + "条";
                    bpageSize = 100;      //设置页面行数
                    bpageCount = (bnMax / bpageSize);//计算出总页数
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//设置总页数
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    if (bnMax != 0)
                    {
                        bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//设置当前页
                    }
                    bnCurrent = 0;       //当前记录数从0开始
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnLocatedYes_Click()获取分页参数");
                    MessageBox.Show(ex.Message, "btnLocatedYes_Click() 获取分页参数");
                }

                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //获取当前页数据
                dataGridViewList.DataSource = datatable;

                if (dataGridViewList.Columns["mapid"] != null)
                {
                    dataGridViewList.Columns["mapid"].Visible = false;
                }

                setDataGridBG();

                this.toolEditPro.Value = 2;

                this.insertQueryIntoTable(dt);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);

                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex, "btnLocatedYes_Click");
            }

            Cursor.Current = Cursors.Default;
        }

        private void btnLocatedNo_Click(object sender, EventArgs e)
        {
            isShowPro(true);
            OracleConnection conn = new OracleConnection(mysqlstr);
            CLC.ForSDGA.GetFromTable.GetFromName(comboTables.Text, getFromNamePath);
            Cursor.Current = Cursors.WaitCursor;

            if (dataGridViewList.DataSource == null || dataGridViewList.Visible == false || blctStr == "" || textKeyWord.Text == "") //updated by fisher in 09-10-10
            {
                bPageSQL = "select rownum as MapID," + CLC.ForSDGA.GetFromTable.SQLFields + " from " + CLC.ForSDGA.GetFromTable.TableName + " where (X is null or Y is null or X = '0' or Y = '0')";

                if (strRegion != "顺德区")
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    bPageSQL += " and " + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "')";
                }
            }
            else
            {
                bPageSQL = blctStr + " and (X is null or Y is null or X = '0' or Y = '0')";
            }
            try
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand(bPageSQL, conn);
                OracleDataAdapter dAdapter = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                dAdapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                cmd.Dispose();
                dAdapter.Dispose();
                conn.Close();
                if (dt.Rows.Count < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.toolStripRecord.Text = "0条";
                    this.tsTextBoxPageNow.Text = "0";//设置当前页
                    this.tStripLabelPageCount.Text = "/ {0}";//设置总页数
                    this.toolStripPageSize.Text = bpageSize.ToString();

                    this.bpageCount = 0;
                    this.bpageCurrent = 0;
                    this.dataGridViewList.Columns.Clear();
                    this.dataGridView1.Visible = false;

                    return;
                }

                //以下代码用来设置翻页
                try
                {
                    bnMax = dt.Rows.Count;
                    this.toolStripRecord.Text = bnMax.ToString() + "条";
                    bpageSize = 100;      //设置页面行数
                    bpageCount = (bnMax / bpageSize);//计算出总页数
                    if ((bnMax % bpageSize) > 0) bpageCount++;
                    this.tStripLabelPageCount.Text = "/" + bpageCount.ToString();//设置总页数
                    this.toolStripPageSize.Text = bpageSize.ToString();
                    if (bnMax != 0)
                    {
                        bpageCurrent = 1;
                        this.tsTextBoxPageNow.Text = bpageCurrent.ToString();//设置当前页
                    }
                    bnCurrent = 0;       //当前记录数从0开始
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "btnLocatedNo_Click()获取分页参数");
                    MessageBox.Show(ex.Message, "btnLocatedNo_Click() 获取分页参数");
                }

                this.toolEditPro.Value = 1;
                Application.DoEvents();

                DataTable datatable = _exportDT = BJLoadData(bPageSQL); //获取当前页数据
                dataGridViewList.DataSource = datatable;

                if (dataGridViewList.Columns["mapid"] != null)
                {
                    dataGridViewList.Columns["mapid"].Visible = false;
                }

                setDataGridBG();
                this.toolEditPro.Value = 2;

                this.insertQueryIntoTable(dt);
                this.dataGridView1.Visible = false;
                this.buttonSave.Enabled = false;
                this.buttonCancel.Enabled = false;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridView1.Rows[i].Cells[1].Value = "";
                }

                if (temEditDt.Rows[0]["基础数据可编辑"].ToString() == "1")
                {
                    MapInfo.Mapping.LayerHelper.SetEditable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);
                }
                MapInfo.Mapping.LayerHelper.SetInsertable(mapControl1.Map.Layers[comboTables.Text.Trim()], true);

                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                writeToLog(ex, "btnLocatedNo_Click");
            }
            Cursor.Current = Cursors.Default;
        }

        private void butLoc2_Click(object sender, EventArgs e)
        {
            this.mapControl1.Tools.LeftButtonTool = "Location";
        }

        private void btnLoc1_Click(object sender, EventArgs e)
        {
            this.mapControl1.Tools.LeftButtonTool = "Location";
        }

        private void btnLoc3_Click(object sender, EventArgs e)
        {
            this.mapControl1.Tools.LeftButtonTool = "Location";
        }

        string videoX = "", videoY = "";  //定义两个变量获取当前点击行的XY值，以便后续取消成功！ fisher(09-09-27)
        int videoIndex = 0;
        private void dataGridViewVideo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex > -1)
                {
                    videoIndex = e.RowIndex;
                    videoX = dataGridViewVideo["X", e.RowIndex].Value.ToString();
                    videoY = dataGridViewVideo["Y", e.RowIndex].Value.ToString();

                    if (dataGridViewVideo["X", e.RowIndex].Value == null || dataGridViewVideo["Y", e.RowIndex].Value == null
                        || dataGridViewVideo["X", e.RowIndex].Value.ToString() == "" || dataGridViewVideo["Y", e.RowIndex].Value.ToString() == ""
                        || Convert.ToInt32(dataGridViewVideo["X", e.RowIndex].Value) == 0 || Convert.ToInt32(dataGridViewVideo["Y", e.RowIndex].Value) == 0)
                    {
                        if (temEditDt.Rows[0]["视频可编辑"].ToString() == "1")  // 有编辑权限（09-12-15）
                        {
                            this.butLoc2.Enabled = true;
                        }
                        return;
                    }
                    double x = Convert.ToDouble(dataGridViewVideo["X", e.RowIndex].Value);
                    double y = Convert.ToDouble(dataGridViewVideo["Y", e.RowIndex].Value);
                    if (x == 0 || y == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("此对象未定位.");
                        if (temEditDt.Rows[0]["视频可编辑"].ToString() == "1")
                        {
                            this.butLoc2.Enabled = true;
                        }
                        return;
                    }

                    //edit by fisher in 09-12-24  定位地图视野
                    MapInfo.Geometry.DPoint dP = new MapInfo.Geometry.DPoint(x, y);
                    MapInfo.Geometry.CoordSys cSys = mapControl1.Map.GetDisplayCoordSys();
                    mapControl1.Map.SetView(dP, cSys, getScale());
                    this.mapControl1.Map.Center = dP;

                    //待完善,从地图中查找要素,闪烁
                    FeatureLayer tempLayer = mapControl1.Map.Layers[editTable.Alias] as MapInfo.Mapping.FeatureLayer;
                    CLC.ForSDGA.GetFromTable.GetFromName("视频位置 ", getFromNamePath);
                    string sID = dataGridViewVideo[CLC.ForSDGA.GetFromTable.ObjID, e.RowIndex].Value.ToString();
                    ft = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(tempLayer.Alias, MapInfo.Data.SearchInfoFactory.SearchWhere("keyID='" + sID + "'"));
                    if (ft != null)
                    {
                        if (ft["mapid"] != null && ft["mapid"].ToString() != "")
                        {
                            selPrinx = Convert.ToInt32(ft["mapid"]);
                        }
                        selMapID = sID;  //??????????????????????
                        ////闪烁要素
                        flashFt = ft;
                        defaultStyle = ft.Style;
                        k = 0;
                        timer1.Start();
                    }

                    if (temEditDt.Rows[0]["视频可编辑"].ToString() == "1")
                    {
                        this.butLoc2.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewVideo_CellClick");
            }
        }

        private void frmMap_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel2_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoX == "" || videoY == "")
                {
                    this.dataGridViewVideo["X", videoIndex].Value = 0.0;
                    this.dataGridViewVideo["Y", videoIndex].Value = 0.0;
                }
                else
                {
                    this.dataGridViewVideo["X", videoIndex].Value = Convert.ToDouble(videoX);
                    this.dataGridViewVideo["Y", videoIndex].Value = Convert.ToDouble(videoY);
                }
                this.btnCancel2.Enabled = false;
                this.buttonSaveWZ.Enabled = false;
                this.butLoc2.Enabled = false;

                Feature f;
                if (videoX == "" || videoY == "" || videoX == "0" || videoY == "0")
                {
                    //删除后退出
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(dataGridViewVideo.Rows[videoIndex].Cells["mapid"].Value));
                    si.QueryDefinition.Columns = null;
                    f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                    if (f != null)
                    {
                        this.editTable.DeleteFeature(f);
                    }
                    this.mapControl1.Tools.LeftButtonTool = "Pan";
                    return;
                }
                //updated by fisher in 09-09-21
                else if (dataGridViewVideo.Rows[videoIndex].Cells["x"].Value.ToString() != "" && dataGridViewVideo.Rows[videoIndex].Cells["y"].Value.ToString() != "")
                {
                    if (Convert.ToDouble(dataGridViewVideo.Rows[videoIndex].Cells["x"].Value) != 0 && Convert.ToDouble(dataGridViewVideo.Rows[videoIndex].Cells["y"].Value) != 0)
                    {
                        //先删除
                        SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(dataGridViewVideo.Rows[videoIndex].Cells["mapid"].Value));
                        si.QueryDefinition.Columns = null;
                        f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                        if (f != null)
                        {
                            this.editTable.DeleteFeature(f);
                        }
                    }
                }

                //添加点
                f = new Feature(this.editTable.TableInfo.Columns);
                f.Geometry = new MapInfo.Geometry.Point(mapControl1.Map.GetDisplayCoordSys(), new DPoint(Convert.ToDouble(videoX), Convert.ToDouble(videoY)));
                f["name"] = dataGridViewVideo.Rows[videoIndex].Cells["设备名称"].Value.ToString();
                f["mapid"] = dataGridViewVideo.Rows[videoIndex].Cells["mapid"].Value.ToString();
                f.Style = new MapInfo.Styles.BitmapPointStyle("sxt.bmp");
                this.editTable.InsertFeature(f);
                this.mapControl1.Tools.LeftButtonTool = "Pan";
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnCancel2_Click");
            }
        }

        private void FieldStr_SelectedIndexChanged(object sender, EventArgs e)
        {
            V_setYSF(FieldStr.SelectedIndex);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValueStr.Text == "")
                {
                    MessageBox.Show("查询值不能为空！");
                    return;
                }
                if (ValueStr.Text.IndexOf("\'") > -1)
                {
                    MessageBox.Show("输入的字符串中不能包含单引号!");
                    return;
                }
                string strExp = "";
                int p = FieldStr.SelectedIndex;
                string[] arr = V_arrType.Split(',');
                string type = arr[p].ToUpper();
                switch (type)
                {
                    case "NUMBER":
                    case "INTEGER":
                    case "LONG":
                    case "FLOAT":
                    case "DOUBLE":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   " + ValueStr.Text.Trim();
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   " + ValueStr.Text.Trim();
                        }
                        this.dataGridExp.Rows.Add(new object[] { strExp, "数字" });
                        break;

                    case "VARCHAR2":
                    case "NVARCHAR2":
                        if (this.dataGridExp.Rows.Count == 0)
                        {
                            strExp = this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + ValueStr.Text.Trim() + "'";
                            if (this.MathStr.Text.Trim() == "包含")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        else
                        {
                            strExp = this.connStr.Text + "  " + this.FieldStr.Text + "   " + this.MathStr.Text + "   '" + ValueStr.Text.Trim() + "'";
                            if (this.MathStr.Text.Trim() == "包含")
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "包含" });
                            }
                            else
                            {
                                this.dataGridExp.Rows.Add(new object[] { strExp, "字符串" });
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnAdd_Click");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dataGridExp.Rows.Count != 0)
                {
                    if (this.dataGridExp.CurrentRow.Index != 0)
                    {
                        this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                    }
                    else
                    {
                        if (this.dataGridExp.Rows.Count > 1)
                        {
                            this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                            string text = this.dataGridExp.Rows[0].Cells["video_Value"].Value.ToString().Replace("并且", "");

                            text = text.Replace("或者", "").Trim();
                            this.dataGridExp.Rows[0].Cells["video_Value"].Value = text;
                        }
                        else
                        {
                            this.dataGridExp.Rows.Remove(this.dataGridExp.CurrentRow);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnDelete_Click");
            }
        }

        private void mapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (!this.Visible) return; // 不是当前模块返回

                //System.Drawing.Point sPoint = new System.Drawing.Point(e.X, e.Y);
                //DPoint dPoint;
                //mapControl1.Map.DisplayTransform.FromDisplay(sPoint, out dPoint);
                //string nPoint = "X = " + dPoint.x.ToString("#.#####") + ", Y = " + dPoint.y.ToString("#.#####");
                //switch (e.Button.ToString())
                //{
                //    case "Right":
                //        labelXY.Visible = true;
                //        labelXY.Text = nPoint;
                //        labelXY.Location = new System.Drawing.Point(e.X, e.Y);
                //        break;
                //    case "Left":
                //        labelXY.Visible = false;
                //        break;
                //    default:
                //        break;
                //}
            }
            catch (Exception ex)
            {
                writeToLog(ex, "mapControl1_MouseClick");
            }
        }

        //查询已定位视频
        private void btnVlocYes_Click(object sender, EventArgs e)
        {
            try
            {
                if (temEditDt.Rows[0]["视频可编辑"].ToString() == "0")
                {
                    MessageBox.Show("您没有查询权限!", "提示");
                    return;
                }
                isShowPro(true);
                string sql = "", countSQL = "";
                if (dataGridViewVideo.DataSource == null || dataGridViewVideo.Visible == false || videolocSql == "" || this.dataGridExp.Rows.Count == 0)
                {
                    if (strRegion == "顺德区")
                    {
                        sql = "select 设备名称,X,Y,所属派出所,日常管理人,设备编号,mapid from 视频位置 where (x is not null and x!=0 and y is not null and y!=0)";
                        countSQL = "select count(*) from 视频位置 where (x is not null and x!=0 and y is not null and y!=0)";
                        sql = "(x is not null and x!=0 and y is not null and y!=0)";
                    }
                    else
                    {
                        sql = "select 设备名称,X,Y,所属派出所,日常管理人,设备编号,mapid from 视频位置 where (x is not null and x!=0 and y is not null and y!=0) and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        countSQL = "select count(*) from 视频位置 where (x is not null and x!=0 and y is not null and y!=0) and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        sql = "(x is not null and x!=0 and y is not null and y!=0) and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else
                {
                    if (videolocSql.IndexOf("where") > -1)
                    {
                        sql = videolocSql + " and (X is not null and Y is not null and X != 0 and Y != 0)";
                        countSQL = "select count(*) from 视频位置 where " + videolocSql + " and (X is not null and Y is not null and X != 0 and Y != 0)";
                        sql = videolocSql + " and (X is not null and Y is not null and X != 0 and Y != 0)";
                    }
                    else
                    {
                        sql = videolocSql + " where (X is not null and Y is not null and X! = 0 and Y != 0)";
                        countSQL = "select count(*) from 视频位置  where " + videolocSql + " and (X is not null and Y is not null and X! = 0 and Y != 0)";
                        sql = videolocSql + " and (X is not null and Y is not null and X! = 0 and Y != 0)";
                    }
                }
                this.getMaxCount(countSQL);
                InitDataSet(this.tsCount);
                _startNo = 0;
                _endNo = perPageSize;

                this.toolEditPro.Value = 1;
                Application.DoEvents();
                DataTable dt = getLoadData(_startNo, _endNo, sql);
                dataGridViewVideo.DataSource = dt;
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                dataGridViewVideo.Columns[5].Visible = false;
                dataGridViewVideo.Columns[6].Visible = false;      // mapid不显示
                this.toolEditPro.Value = 2;
                insertQueryIntoTable(dt);
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnVlocYes_Click");
                isShowPro(false);
            }
        }

        //查询未定位视频
        private void btnVlocNo_Click(object sender, EventArgs e)
        {
            try
            {
                isShowPro(true);
                if (temEditDt.Rows[0]["视频可编辑"].ToString() == "0")
                {
                    isShowPro(false);
                    MessageBox.Show("您没有查询权限!", "提示");
                    return;
                }

                (this.editTable as IFeatureCollection).Clear();  //清除地图上的所有点
                this.editTable.Pack(PackType.All);
                string sql = "", countSQL = "";

                if (dataGridViewVideo.DataSource == null || dataGridViewVideo.Visible == false || videolocSql == "" || this.dataGridExp.Rows.Count == 0)
                {
                    if (strRegion == "顺德区")
                    {
                        sql = "select 设备名称,X,Y,所属派出所,日常管理人,设备编号,mapid from 视频位置 where (x is null or x=0 or y is null or y=0)";
                        countSQL = "select count(*) from 视频位置 where (x is null or x=0 or y is null or y=0)";
                        sql = " (x is null or x=0 or y is null or y=0)";
                    }
                    else
                    {
                        sql = "select 设备名称,X,Y,所属派出所,日常管理人,设备编号,mapid from 视频位置 where (x is null or x=0 or y is null or y=0) and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        countSQL = "select count(*) from 视频位置 where (x is null or x=0 or y is null or y=0) and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                        sql = " (x is null or x=0 or y is null or y=0) and 所属派出所 in ('" + strRegion.Replace(",", "','") + "')";
                    }
                }
                else
                {
                    if (videolocSql.IndexOf("where") > -1)
                    {
                        sql = videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                        countSQL = "select count(*) from 视频位置 where " + videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                    }
                    else
                    {
                        sql = videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                        countSQL = "select count(*) from 视频位置 where " + videolocSql + " and (X is null or Y is null or X = 0 or Y = 0)";
                    }
                }

                this.getMaxCount(countSQL);
                if (vMax < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("所设条件无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.pageVideoCount = 0;
                    this.pageVideoCurrent = 0;
                    this.RecordCount.Text = "0条";
                    this.tstNow.Text = "0"; //设置当前页
                    this.bnCount.Text = "/ {0}";//设置总页数
                    this.tstbPre.Text = this.perPageSize.ToString();

                    this.dataGridViewVideo.Columns.Clear();
                    return;
                }
                InitDataSet(this.tsCount);
                _startNo = 0;
                _endNo = perPageSize;

                this.toolEditPro.Value = 1;
                Application.DoEvents();
                DataTable dt = this.getLoadData(_startNo, _endNo, sql);
                this.toolEditPro.Value = 2;
                dataGridViewVideo.DataSource = dt;
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                dataGridViewVideo.Columns[5].Visible = false;
                dataGridViewVideo.Columns[6].Visible = false;      // mapid不显示
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnVlocNo_Click");
                isShowPro(false);
            }
        }

        //////////////////------以下是自动匹配代码(add by lili in 2010-5-28)-----////////////////

        /// <summary>
        /// 自动补全方法(add by LiLi in 2010-5-21)
        /// </summary>
        /// <param name="keyword">文本框中输入的值</param>
        /// <param name="colword">列名</param>
        /// <param name="tableName">表名</param>
        /// <param name="listBox1">显示自动补全值的控件</param>
        /// <returns>配配结果</returns>
        private DataTable getListBox(string keyword, string colword, string tableName)
        {
            try
            {
                DataTable dt = null;
                string strExp = "";
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\GetFromNameConfig.ini");
                string tabName = CLC.ForSDGA.GetFromTable.TableName;
                string match = CLC.INIClass.IniReadValue(tableName, "MatchField");

                if (keyword != "" && colword != "")
                {
                    if (match.IndexOf(colword) >= 0)
                    {
                        #region 生成SQL语句
                        switch (colword)
                        {
                            case "案件类型":
                                strExp = "select 类型名称 from 案件类型 t where 类型名称  like '" + keyword + "%' union select 类型名称 from 案件类型 t where 类型名称 like '%" + keyword + "%' and 类型名称 not like '" + keyword + "%' and 类型名称 not like '%" + keyword + "' union select 类型名称 from 案件类型 t where 类型名称  like '%" + keyword + "'";
                                break;
                            case "案别_案由":
                                strExp = "select 名称2 from 警综案别 t where 名称2  like '" + keyword + "%' union select 名称2 from 警综案别 t where 名称2 like '%" + keyword + "%' and 名称2 not like '" + keyword + "%' and 名称2 not like '%" + keyword + "' union select 名称2 from 警综案别 t where 名称2  like '%" + keyword + "'";
                                break;
                            case "人口性质":
                                strExp = "select 类型名称 from 人口性质 t where 类型名称  like '" + keyword + "%' union select 类型名称 from 人口性质 t where 类型名称 like '%" + keyword + "%' and 类型名称 not like '" + keyword + "%' and 类型名称 not like '%" + keyword + "' union select 类型名称 from 人口性质 t where 类型名称  like '%" + keyword + "'";
                                break;
                            case "性别":
                                strExp = "select 名称 from 性别 t where 名称  like '" + keyword + "%' union select 名称 from 性别 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 性别 t where 名称  like '%" + keyword + "'";
                                break;
                            case "民族":
                                strExp = "select 名称 from 民族 t where 名称  like '" + keyword + "%' union select 名称 from 民族 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 民族 t where 名称  like '%" + keyword + "'";
                                break;
                            case "婚姻状态":
                                strExp = "select 名称 from 婚姻状况 t where 名称  like '" + keyword + "%' union select 名称 from 婚姻状况 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 婚姻状况 t where 名称  like '%" + keyword + "'";
                                break;
                            case "政治面貌":
                                strExp = "select 名称 from 政治面貌 t where 名称  like '" + keyword + "%' union select 名称 from 政治面貌 t where 名称 like '%" + keyword + "%' and 名称 not like '" + keyword + "%' and 名称 not like '%" + keyword + "' union select 名称 from 政治面貌 t where 名称  like '%" + keyword + "'";
                                break;
                            case "房屋类型":
                                strExp = "select 类型名称 from 房屋类型 t where 类型名称  like '" + keyword + "%' union select 类型名称 from 房屋类型 t where 类型名称 like '%" + keyword + "%' and 类型名称 not like '" + keyword + "%' and 类型名称 not like '%" + keyword + "' union select 类型名称 from 房屋类型 t where 类型名称  like '%" + keyword + "'";
                                break;
                            case "所属派出所":
                                strExp = "select 派出所名 from 基层派出所 t where 派出所名  like '" + keyword + "%' union select 派出所名 from 基层派出所 t where 派出所名 like '%" + keyword + "%' and 派出所名 not like '" + keyword + "%' and 派出所名 not like '%" + keyword + "' union select 派出所名 from 基层派出所 t where 派出所名  like '%" + keyword + "'";
                                break;
                            case "所属中队":
                                strExp = "select 中队名 from 基层民警中队 t where 中队名  like '" + keyword + "%' union select 中队名 from 基层民警中队 t where 中队名 like '%" + keyword + "%' and 中队名 not like '" + keyword + "%' and 中队名 not like '%" + keyword + "' union select 中队名 from 基层民警中队 t where 中队名  like '%" + keyword + "'";
                                break;
                            case "所属警务室":
                                strExp = "select 警务室名 from 社区警务室 t where 警务室名  like '" + keyword + "%' union select 警务室名 from 社区警务室 t where 警务室名 like '%" + keyword + "%' and 警务室名 not like '" + keyword + "%' and 警务室名 not like '%" + keyword + "' union select 警务室名 from 社区警务室 t where 警务室名  like '%" + keyword + "'";
                                break;
                        }
                        #endregion

                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                    }
                    else
                    {
                        dt = null;
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getListBox");
                return null;
            }
        }

        /// <summary>
        /// 列为固定值时自动添加(add by LiLi in 2010-5-21)
        /// </summary>
        /// <param name="colName">列名</param>
        /// <param name="tableName">表名</param>
        /// <param name="listBox">显示自动补全值的控件</param>
        /// <returns>配配结果</returns>
        private DataTable MatchShu(string colName, string tableName)
        {
            try
            {
                DataTable dt = null;
                string strExp = "";
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                string tabName = CLC.ForSDGA.GetFromTable.TableName;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\GetFromNameConfig.ini");
                string match = CLC.INIClass.IniReadValue(tableName, "MatchField");
                if (colName != "" && tabName != "")
                {
                    if (match.IndexOf(colName) >= 0)
                    {
                        #region 生成SQL语句
                        switch (colName)
                        {
                            case "案件类型":
                                strExp = "select 类型名称 from 案件类型 t Group by 类型名称";
                                break;
                            case "案别_案由":
                                strExp = "select 名称2 from 警综案别 t Group by 名称2";
                                break;
                            case "人口性质":
                                strExp = "select 性质名称 from 人口性质 t Group by 性质名称";
                                break;
                            case "性别":
                                strExp = "select 名称 from 性别 t Group by 名称";
                                break;
                            case "民族":
                                strExp = "select 名称 from 民族 t Group by 名称";
                                break;
                            case "婚姻状态":
                                strExp = "select 名称 from 婚姻状况 t Group by 名称";
                                break;
                            case "政治面貌":
                                strExp = "select 名称 from 政治面貌 t Group by 名称";
                                break;
                            case "房屋类型":
                                strExp = "select 类型名称 from 房屋类型 t Group by 类型名称";
                                break;
                            case "所属派出所":
                                strExp = "select 派出所名 from 基层派出所 t Group by 派出所名";
                                break;
                            case "所属中队":
                                strExp = "select 中队名 from 基层民警中队 t Group by 中队名";
                                break;
                            case "所属警务室":
                                strExp = "select 警务室名 from 社区警务室 t Group by 警务室名";
                                break;
                        }
                        #endregion

                        CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                        dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(strExp);
                    }
                    else
                        dt = null;
                }
                return dt;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "MatchShu");
                return null;
            }
        }

        // 基础数据编辑匹配
        private void textKeyWord_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = null;
                switch (comboTables.Text)
                {
                    case "基层派出所":
                        dt = getListBox(this.textKeyWord.Text.Trim(), "所属派出所", this.comboTables.Text);
                        break;
                    case "基层民警中队":
                        dt = getListBox(this.textKeyWord.Text.Trim(), "所属中队", this.comboTables.Text);
                        break;
                    case "社区警务室":
                        dt = getListBox(this.textKeyWord.Text.Trim(), "所属警务室", this.comboTables.Text);
                        break;
                }
                if (dt != null)
                    textKeyWord.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textKeyWord_TextChanged_1");
            }
        }

        // 基础数据编辑匹配
        private void textKeyWord_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = null;
                switch (comboTables.Text)
                {
                    case "基层派出所":
                        dt = MatchShu("所属派出所", this.comboTables.Text);
                        break;
                    case "基层民警中队":
                        dt = MatchShu("所属中队", this.comboTables.Text);
                        break;
                    case "社区警务室":
                        dt = MatchShu("所属警务室", this.comboTables.Text);
                        break;
                }
                if (dt != null)
                    textKeyWord.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textKeyWord_Click_1");
            }
        }

        private void textKeyWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                searchByKey();
            }
        }

        // 视频编辑匹配
        private void ValueStr_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.ValueStr.Text.Trim(), "视频位置", this.FieldStr.Text);
                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ValueStr_TextChanged");
            }
        }

        // 视频编辑匹配
        private void ValueStr_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.FieldStr.Text, "视频位置");
                if (dt != null)
                    ValueStr.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ValueStr_Click");
            }
        }

        // 业务数据表匹配
        private void textValue_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = getListBox(this.textValue.Text.Trim(), this.comboField.Text, this.comboTable.Text);
                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textValue_TextChanged_1");
            }
        }

        // 业务数据表匹配
        private void textValue_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = MatchShu(this.comboField.Text, this.comboTable.Text);
                if (dt != null)
                    textValue.GetSpellBoxSource(dt);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "textValue_Click_1");
            }
        }

        /// <summary>
        /// 隐藏或显示进度条
        /// </summary>
        /// <param name="falg">布尔值</param>
        private void isShowPro(bool falg)
        {
            try
            {
                //this.toolEditPro.Value = 0;
                //this.toolEditPro.Maximum = 3;
                //this.toolEditPro.Visible = falg;
                //this.toolEditProLbl.Visible = falg;
                //this.toolEditProSep.Visible = falg;
                //Application.DoEvents();
            }
            catch (Exception ex)
            {
                writeToLog(ex, "isShowPro");
            }
        }

        // 单击单元格
        private void dataGridViewGaInfo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int rowIn = dataGridViewGaInfo.Rows.Count - 1;
                if (e.RowIndex == rowIn && comboTable.Text == "安全防护单位")
                {
                    if (dataGridViewGaInfo.Rows[e.RowIndex].Cells[1].Value.ToString() != "点击编辑") return;

                    if (this.frmZL != null)
                    {
                        if (this.frmZL.Visible == true)
                        {
                            this.frmZL.Close();
                        }
                    }
                    string DWMC = "";
                    for (int j = 0; j < this.dataGridViewGaInfo.Rows.Count; j++)
                    {
                        if (dataGridViewGaInfo.Rows[j].Cells[0].Value.ToString() == "单位名称")
                        {
                            DWMC = dataGridViewGaInfo.Rows[j].Cells[1].Value.ToString();
                        }
                    }
                    if (DWMC == "")
                    {
                        MessageBox.Show("名称不能为空！", "提示");
                        return;
                    }
                    this.frmZL = new FrmZLMessage(DWMC, mysqlstr, this.temEditDt);

                    //设置信息框在右下角
                    System.Drawing.Point p = this.PointToScreen(mapControl1.Parent.Location);
                    this.frmZL.SetDesktopLocation(mapControl1.Width - frmZL.Width + p.X, mapControl1.Height - frmZL.Height + p.Y + 25);
                    this.frmZL.Show();
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex, "dataGridViewGaInfo_CellClick");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                LinkLabel link = (LinkLabel)sender;

                switch (link.Text)
                {
                    case "隐藏条件栏":
                        if (tabControl1.SelectedTab == tabPage1)
                        {
                            this.textKeyWord.Visible = false;
                            groupBox3.Visible = false;
                        }
                        if (tabControl1.SelectedTab == tabPage2)
                        {
                            this.ValueStr.Visible = false;
                            this.groupBox1.Visible = false;
                        }
                        if (tabControl1.SelectedTab == tabPage3)
                        {
                            this.textValue.Visible = false;
                            groupBox2.Visible = false;
                        }
                        link.Text = "显示条件栏";
                        break;
                    case "显示条件栏":
                        if (tabControl1.SelectedTab == tabPage1)
                        {
                            this.textKeyWord.Visible = true;
                            groupBox3.Visible = true;
                        }
                        if (tabControl1.SelectedTab == tabPage2)
                        {
                            this.ValueStr.Visible = true;
                            this.groupBox1.Visible = true;
                        }
                        if (tabControl1.SelectedTab == tabPage3)
                        {
                            this.textValue.Visible = true;
                            groupBox2.Visible = true;
                        }
                        link.Text = "隐藏条件栏";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LinklblHides_LinkClicked");
            }
        }

        // 获取缩放比例
        private double getScale()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(ZoomFile);
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("比例尺", "缩放比例"));
                return dou;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "LBSgisPoliceEdit-getScale");
                return 0;
            }
        }

        private void timerGPS_Tick(object sender, EventArgs e)
        {
            try
            {
                DelGPSLayer();
                GetPolice(policeNo);
            }
            catch (Exception ex)
            {
                this.timerGPS.Stop();
                writeToLog(ex, "timerGPS_Tick");
            }
        }

        // GPS警员关联所选项坐标并切换到编辑样式
        private void udToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                setEditXY(policeNo);
                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;
                this.tabControl3.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "timerGPS_Tick");
            }
        }

        /// <summary>
        /// 根据警员的编号将警员的坐标赋值给所选的业务数据
        /// </summary>
        /// <param name="policeNo">警员的编号</param>
        private void setEditXY(string policeNo)
        {
            try
            {
                if (dataGridViewGaList.CurrentRow == null)
                {
                    MessageBox.Show("列表内无数据，无法实现关联操作！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                string sql = "Select X,Y from GPS警员 where 警力编号='" + policeNo + "'";
                DataTable table = new DataTable();
                CLC.DatabaseRelated.OracleDriver.CreateConstring(datasource, userid, password);
                table = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);

                // 更新信息列表的坐标
                for (int i = 0; i < this.dataGridViewGaInfo.Rows.Count; i++)
                {
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "X")
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = table.Rows[0]["X"].ToString();
                        continue;
                    }
                    if (this.dataGridViewGaInfo.Rows[i].Cells[0].Value.ToString() == "Y")
                    {
                        this.dataGridViewGaInfo.Rows[i].Cells[1].Value = table.Rows[0]["Y"].ToString();
                        continue;
                    }
                }

                double dX = 0, dY = 0;
                try
                {
                    dX = Convert.ToDouble(table.Rows[0]["X"]);
                    dY = Convert.ToDouble(table.Rows[0]["Y"]);

                    string tabName = comboTable.Text;
                    if (tabControl1.SelectedTab == tabPage2)
                    {
                        tabName = "视频";
                    }
                    string upstr = "";   // 更新的SQL
                    CLC.ForSDGA.GetFromTable.GetFromName(tabName, getFromNamePath);
                    string sName = CLC.ForSDGA.GetFromTable.ObjName;
                    string strID = CLC.ForSDGA.GetFromTable.ObjID;
                    string tableName = CLC.ForSDGA.GetFromTable.TableName;
                    if (tableName == "人口系统" || tableName == "案件信息" || tableName == "出租屋房屋系统")
                    {
                        upstr = "update " + tableName + " t set t.geoloc.SDO_POINT.X=" + dX + ",t.geoloc.SDO_POINT.Y=" + dY + " where t." + strID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[strID].Value.ToString() + "'";
                    }
                    else
                    {
                        upstr = "update " + CLC.ForSDGA.GetFromTable.TableName + " t set t.X=" + dX + ",t.Y=" + dY + " where t." + strID + "='" + this.dataGridViewGaList.Rows[rowIndex].Cells[strID].Value.ToString() + "'";
                    }
                    CLC.DatabaseRelated.OracleDriver.OracleComRun(upstr);

                    // 先删除之前的点
                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("mapid=" + Convert.ToInt32(this.dataGridViewGaList.Rows[rowIndex].Cells["MAPID"].Value));
                    si.QueryDefinition.Columns = null;
                    Feature f = MapInfo.Engine.Session.Current.Catalog.SearchForFeature(editTable.Alias, si);
                    if (f != null)
                    {
                        this.editTable.DeleteFeature(f);
                    }

                    // 再添加更新点
                    if (dX > 0 && dY > 0)
                    {
                        FeatureGeometry pt = new MapInfo.Geometry.Point((new FeatureLayer(this.editTable)).CoordSys, dX, dY);

                        Feature pFeat = new Feature(editTable.TableInfo.Columns);

                        pFeat.Geometry = pt;
                        pFeat.Style = featStyle;
                        pFeat["MapID"] = this.dataGridViewGaList.Rows[rowIndex].Cells["MAPID"].Value.ToString();
                        pFeat["keyID"] = this.dataGridViewGaList.Rows[rowIndex].Cells[strID].Value.ToString();
                        pFeat["Name"] = this.dataGridViewGaList.Rows[rowIndex].Cells[sName].Value.ToString();
                        editTable.InsertFeature(pFeat);
                    }

                    // 更新查询列表里面的坐标
                    this.dataGridViewGaList.Rows[rowIndex].Cells["X"].Value = dX;
                    this.dataGridViewGaList.Rows[rowIndex].Cells["Y"].Value = dY;
                    MessageBox.Show("坐标关联成功！", "提示");
                }
                catch
                {
                    MessageBox.Show("坐标关联失败！", "提示");
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "setEditXY");
            }
        }

        // 获取GPS警员刷新时间
        private double getGPSTime()
        {
            try
            {
                double dou = 0;
                CLC.INIClass.IniPathSet(Application.StartupPath + "\\ConfigBJXX.ini");
                dou = Convert.ToDouble(CLC.INIClass.IniReadValue("GPS警员", "更新频率"));
                return dou * 1000;      // 将毫秒转换成秒
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getGPSTime");
                return 0;
            }
        }

        private void bindingNavigatorVideo_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                int countShu = Convert.ToInt32(this.tstbPre.Text);
                if (e.ClickedItem.Text == "上一页")
                {
                    if (pageVideoCurrent <= 1)
                    {
                        MessageBox.Show("已经是第一页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        if (_endNo == vMax)
                        {
                            pageVideoCurrent--;
                            vCurrent = perPageSize * (pageVideoCurrent - 1);
                            _startNo = vMax - (vMax % countShu) + 1 - countShu;
                            _endNo = vMax - (vMax % countShu);
                        }
                        else
                        {
                            pageVideoCurrent--;
                            vCurrent = pageSize * (pageVideoCurrent - 1);
                            _startNo -= countShu;
                            _endNo -= countShu;
                        }
                    }
                }
                else if (e.ClickedItem.Text == "下一页")
                {
                    if (pageVideoCurrent > pageVideoCount - 1)
                    {
                        MessageBox.Show("已经是最后一页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageVideoCurrent++;
                        vCurrent = perPageSize * (pageVideoCurrent - 1);
                        _startNo += countShu;
                        _endNo += countShu;
                    }
                }
                else if (e.ClickedItem.Text == "转到首页")
                {
                    if (pageVideoCurrent <= 1)
                    {
                        MessageBox.Show("已经是首页，请点击“下一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageVideoCurrent = 1;
                        vCurrent = 0;
                        _endNo = countShu;
                        _startNo = 1;
                    }
                }
                else if (e.ClickedItem.Text == "转到尾页")
                {
                    if (pageVideoCurrent > pageVideoCount - 1)
                    {
                        MessageBox.Show("已经是尾页，请点击“上一页”查看！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        return;
                    }
                    else
                    {
                        pageVideoCurrent = pageVideoCount;
                        vCurrent = perPageSize * (pageVideoCurrent - 1);
                        _endNo = vMax;
                        _startNo = vMax - (vMax % countShu) + 1;
                    }
                }
                else
                {
                    return;
                }

                isShowPro(true);
                this.tstNow.Text = pageVideoCurrent.ToString();//设置当前页

                //以下代码用于填充datagridview（feng）
                this.Cursor = Cursors.WaitCursor;
                dataGridViewVideo.Columns.Clear();
                //DataTable datatable = Ga_exportDT = LoadData(PageSQL); //获取当前页数据
                DataTable datatable = getLoadData(_startNo, _endNo, videoSQL); //获取当前页数据; lili 2010-8-26
                dataGridViewVideo.DataSource = datatable;
                this.toolEditPro.Value = 1;
                Application.DoEvents();

                //设置gridview的间隔颜色
                for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                    }
                    else
                    {
                        dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
                this.toolEditPro.Value = 2;

                this.insertQueryIntoTable(datatable);
                this.Cursor = Cursors.Default;
                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "bindingNavigatorVideo_ItemClicked()");
                MessageBox.Show(ex.Message, "bindingNavigatorVideo_ItemClicked()");
                this.Cursor = Cursors.Default;
            }
        }

        private void tstNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                try
                {
                    if (Convert.ToInt32(this.tstNow.Text) < 1 || Convert.ToInt32(this.tstNow.Text) > pageVideoCount)
                    {
                        MessageBox.Show("页码超出范围，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        this.tstNow.Text = pageVideoCurrent.ToString();
                        return;
                    }
                    else
                    {
                        isShowPro(true);
                        this.pageVideoCurrent = Convert.ToInt32(this.tstNow.Text);
                        vCurrent = perPageSize * (pageVideoCurrent - 1);
                        _startNo = ((pageVideoCurrent - 1) * perPageSize) + 1;
                        _endNo = _startNo + perPageSize - 1;

                        //以下代码用于填充datagridview（feng）
                        this.Cursor = Cursors.WaitCursor;
                        dataGridViewVideo.Columns.Clear();
                        DataTable datatable = getLoadData(_startNo, _endNo, videoSQL); //获取当前页数据
                        dataGridViewVideo.DataSource = datatable;
                        this.toolEditPro.Value = 1;
                        Application.DoEvents();

                        if (dataGridViewVideo.Columns["mapid"] != null)
                        {
                            dataGridViewVideo.Columns["mapid"].Visible = false;
                        }
                        //设置gridview的间隔颜色
                        for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                        {
                            if (i % 2 == 1)
                            {
                                dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                            }
                            else
                            {
                                dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                            }
                        }
                        this.toolEditPro.Value = 2;

                        this.insertQueryIntoTable(datatable);
                        this.Cursor = Cursors.Default;
                        this.toolEditPro.Value = 3;
                        Application.DoEvents();
                        isShowPro(false);
                    }
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "tstNow_KeyPress()");
                }
            }
        }

        private void tstbPre_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r" && this.dataGridViewVideo.DataSource != null)
            {
                try
                {
                    isShowPro(true);
                    this.Cursor = Cursors.WaitCursor;
                    this.perPageSize = Convert.ToInt32(this.tstbPre.Text);
                    this.pageVideoCount = (vMax / perPageSize);//计算出总页数
                    if ((vMax % perPageSize) > 0) pageVideoCount++;
                    this.bnCount.Text = "/" + pageVideoCount.ToString();//设置总页数
                    if (vMax != 0)
                    {
                        this.pageVideoCurrent = 1;
                        this.tstNow.Text = pageVideoCurrent.ToString();//设置当前页
                    }
                    this.vCurrent = 0;       //当前记录数从0开始
                    _endNo = perPageSize;
                    _startNo = 1;

                    //以下代码用于填充datagridview（feng）
                    this.Cursor = Cursors.WaitCursor;
                    dataGridViewVideo.Columns.Clear();
                    DataTable datatable = getLoadData(_startNo, _endNo, videoSQL); //获取当前页数据
                    dataGridViewVideo.DataSource = datatable;
                    this.toolEditPro.Value = 1;
                    Application.DoEvents();

                    if (dataGridViewVideo.Columns["mapid"] != null)
                    {
                        dataGridViewVideo.Columns["mapid"].Visible = false;
                    }
                    //设置gridview的间隔颜色
                    for (int i = 0; i < dataGridViewVideo.Rows.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(211, 222, 233);
                        }
                        else
                        {
                            dataGridViewVideo.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                        }
                    }
                    this.toolEditPro.Value = 2;

                    this.insertQueryIntoTable(datatable);
                    this.Cursor = Cursors.Default;
                    this.toolEditPro.Value = 3;
                    Application.DoEvents();
                    isShowPro(false);
                }
                catch (Exception ex)
                {
                    isShowPro(false);
                    writeToLog(ex, "toolStripTextBox1_KeyPress()");
                }
            }
        }

        // 此方法用于业务数据查看最近一个月的数据     edit by lili in 2010-12-17
        private void btnMonth_Click(object sender, EventArgs e)
        {
            OracleConnection conn = new OracleConnection(mysqlstr);
            try
            {
                this.Cursor = Cursors.WaitCursor;
                isShowPro(true);
                string powerLimit = "";
                DateTime frontTime = DateTime.Now;
                DateTime time = frontTime.AddDays(-30);
                _first = Convert.ToInt32(this.toolStripTextBox1.Text);
                _end = 1;

                CLC.ForSDGA.GetFromTable.GetFromName(comboTable.Text, getFromNamePath);
                string biaoName = CLC.ForSDGA.GetFromTable.TableName;

                string sqlStr = "";
                if (biaoName == "案件信息")
                {
                    sqlStr = "select count(*) from " + biaoName + " where 发案时间初值 >= to_date('" + time + "', 'YYYY-MM-DD HH24:MI:SS') " +
                             "and 发案时间初值 <= to_date('" + frontTime + "', 'YYYY-MM-DD HH24:MI:SS')";
                }
                else
                {
                    sqlStr = "select count(*) from " + biaoName + " where 抽取更新时间 >= to_date('" + time + "', 'YYYY-MM-DD HH24:MI:SS') " +
                             "and 抽取更新时间 <= to_date('" + frontTime + "', 'YYYY-MM-DD HH24:MI:SS')";
                }


                if (strRegion != "顺德区" && strRegion != "")     // 权限处理
                {
                    if (Array.IndexOf(strRegion.Split(','), "大良") > -1)
                    {
                        strRegion = strRegion.Replace("大良", "大良,德胜");
                    }
                    powerLimit += " and (" + CLC.ForSDGA.GetFromTable.XiaQuField + " in ('" + strRegion.Replace(",", "','") + "'))";

                }
                if (strRegion1 != "" && (biaoName == "案件信息" || biaoName == "公共场所"))
                {
                    if (powerLimit.IndexOf("and") > -1)
                    {
                        powerLimit = powerLimit.Remove(powerLimit.LastIndexOf(")"));
                        powerLimit += " or " + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                    else
                    {
                        powerLimit += " and (" + CLC.ForSDGA.GetFromTable.ZhongDuiField + " in ('" + strRegion1.Replace(",", "','") + "'))";
                    }
                }

                this.toolEditPro.Value = 1;
                Application.DoEvents();
                sqlStr = powerLimit == string.Empty ? sqlStr : sqlStr + " and " + powerLimit;

                conn.Open();
                OracleCommand cmd = new OracleCommand(sqlStr, conn);
                OracleDataAdapter orada = new OracleDataAdapter(cmd);
                DataTable _ds = new DataTable();
                orada.Fill(_ds);
                cmd.Dispose();
                orada.Dispose();
                conn.Close();
                if (Convert.ToInt32(_ds.Rows[0][0]) < 1)
                {
                    isShowPro(false);
                    MessageBox.Show("最近一个月无记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Cursor = Cursors.Default;
                    this.RecordCount.Text = "0条";
                    this.PageNow.Text = "0";//设置当前页
                    this.lblPageCount.Text = "/ {0}";//设置总页数
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    return;
                }

                //以下代码用来设置翻页
                try
                {
                    nMax = Convert.ToInt32(_ds.Rows[0][0]);
                    this.RecordCount.Text = nMax.ToString() + "条";
                    pageSize = Convert.ToInt32(this.toolStripTextBox1.Text);      //设置页面行数
                    pageCount = (nMax / pageSize);//计算出总页数
                    if ((nMax % pageSize) > 0) pageCount++;
                    this.lblPageCount.Text = "/" + pageCount.ToString();//设置总页数
                    this.toolStripTextBox1.Text = pageSize.ToString();
                    if (nMax != 0)
                    {
                        pageCurrent = 1;
                        this.PageNow.Text = pageCurrent.ToString();//设置当前页
                    }
                    nCurrent = 0;       //当前记录数从0开始
                }
                catch (Exception ex)
                {
                    writeToLog(ex, "btnMonth_Click 获取分页参数");
                    MessageBox.Show(ex.Message, "btnMonth_Click 获取分页参数");
                }

                string _whereSql = "";
                if (biaoName == "案件信息")
                    _whereSql = " and " + sqlStr.Substring(sqlStr.IndexOf("发案时间初值"));
                else
                    _whereSql = " and " + sqlStr.Substring(sqlStr.IndexOf("抽取更新时间"));

                _mySQL = _whereSql;
                this.toolEditPro.Value = 2;
                Application.DoEvents();

                DataTable datatable = Ga_exportDT = LoadData(_mySQL, _first, _end, biaoName); //获取当前页数据
                this.dataGridViewGaList.DataSource = datatable;

                if (dataGridViewGaList.Columns["mapid"] != null)
                {
                    dataGridViewGaList.Columns["mapid"].Visible = false;
                }
                GasetDataGridBG();

                //设置gridview的间隔颜色
                this.insertGaQueryIntoTable(datatable);
                this.dataGridViewGaInfo.Visible = false;

                for (int i = 0; i < dataGridViewGaInfo.Rows.Count; i++)//清空datagridview
                {
                    this.dataGridViewGaInfo.Rows[i].Cells[1].Value = "";
                }

                this.btnGaSave.Enabled = false;
                this.btnGaCancel.Enabled = false;

                this.toolEditPro.Value = 3;
                Application.DoEvents();
                isShowPro(false);
            }
            catch (Exception ex)
            {
                isShowPro(false);
                writeToLog(ex, "LBSgisPoliceEdit--btnMonth_Click");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void ucGISPoliceEdit_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible)  // 开启模块时新建图层
                {
                    this.toolStrip1.Items[19].Visible = true;
                    this.tabControl1.SelectedIndex = 0;
                    string sAlias = "基层派出所";
                    this.editTable = createTable(sAlias);
                    FeatureLayer fl = new FeatureLayer(this.editTable);
                    mapControl1.Map.Layers.Insert(0, (IMapLayer)fl);

                    this.labeLayer(this.editTable); //标注图层
                }
                else   // 关闭模块时关掉在使用图层
                {
                    this.toolStrip1.Items[19].Visible = false;
                    string sAlias = this.editTable.Alias;
                    if (mapControl1.Map.Layers[sAlias] != null)
                    {
                        MapInfo.Engine.Session.Current.Catalog.CloseTable(sAlias);
                    }
                    tabControl1_SelectedIndexChanged(null, null);
                    DelGPSLayer();  // 删除警员关连坐标图层
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "ucGISPoliceEdit_VisibleChanged");
            }
        }

        /// <summary>
        /// 认领按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-1
        /// </summary>
        private void btnClaim_Click(object sender, EventArgs e)
        {
            try
            {
                string claimSql = getSqlStr(comboTable.Text, claAssig.assigned);
                frmClaim frm = new frmClaim(claimSql, strRegion, strRegion1, StrCon);
                if (frm.ShowDialog() == DialogResult.OK)
                {
 
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnClaim_Click");
            }
        }

        /// <summary>
        /// 指派按钮
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-1
        /// </summary>
        private void btnAssigned_Click(object sender, EventArgs e)
        {
            try
            {
                string assigSql = getSqlStr(comboTable.Text, claAssig.assigned);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnAssigned_Click");
            }
        }

        /// <summary>
        /// 枚举值 claim表示认领，assigned表示指派
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        private enum claAssig
        {
            claim,
            assigned
        }

        /// <summary>
        /// 根据表名获取查询SQL
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="clAs">枚举值 claim表示认领，assigned表示指派</param>
        /// <returns>查询SQL</returns>
        private string getSqlStr(string tableName,claAssig clAs)
        {
            try
            {
                string sqlStr = "";
                CLC.ForSDGA.GetFromTable.GetFromName(tableName, getFromNamePath);
                string selStr = CLC.ForSDGA.GetFromTable.ObjID + "," + CLC.ForSDGA.GetFromTable.ObjName;
                string table = CLC.ForSDGA.GetFromTable.TableName;

                if (strRegion == "顺德区")
                {
                    if (isOracleSpatialTab)
                    {
                        sqlStr = "select " + selStr + " from " + table + " t where (t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') " +
                                                                                   "and 备用字段一 is null";
                    }
                    else
                    {
                        sqlStr = "select " + selStr + " from " + table + " where (X is null or Y is null or X = '0' or Y = '0') and 备用字段一 is null";
                    }
                    return sqlStr;
                }
                else
                {
                    if (isOracleSpatialTab)
                    {
                        sqlStr = "select " + selStr + " from " + table + " t where (t.geoloc.SDO_POINT.X is null or t.geoloc.SDO_POINT.Y is null or t.geoloc.SDO_POINT.X = '0' or t.geoloc.SDO_POINT.Y = '0') and 备用字段一 is null";

                    }
                    else
                    {
                        sqlStr = "select " + selStr + " from " + table + " where (X is null or Y is null or X = '0' or Y = '0') and 备用字段一 is null";
                    }
                }

                string pcsStr = getNumber(clAs);
                sqlStr += pcsStr;

                return sqlStr;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "getSqlStr");
                return null;
            }
        }

        /// <summary>
        /// 获取派出所编号
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-3-2
        /// </summary>
        /// <returns>派出所中队条件</returns>
        private string getNumber(claAssig clAs)
        {
            try
            {
                string whSql="";
                string[] strQ = null;
                string sql = "";

                if (strRegion == "顺德区")                        // 全区
                    if (clAs.ToString() == "assigned") // 指派
                        whSql = " and 所属派出所代码 is null and 所属中队代码 is null";

                if (strRegion != "顺德区" && strRegion != "")     // 派出所
                {
                    sql = "select distinct(派出所代码) from 基层派出所 where 派出所名 in ('" + strRegion.Replace(",", "','") + "')";
                    DataTable pcsTb = GetSelTable(sql);

                    if (clAs.ToString() == "claim")  // 认领
                        whSql = " and 所属派出所代码 is null and 所属中队代码 is null";
                    else                             // 指派
                    {
                        for (int j = 0; j < pcsTb.Rows.Count; j++)
                        {
                            whSql += " and 所属派出所代码='" + pcsTb.Rows[j][0].ToString() + "'";
                        }
                        whSql += " and 所属中队代码 is null";
                    }

                    return whSql;
                }

                if (strRegion1 != "")                             // 中队
                {
                    string pSql = "select distinct(所属派出所) from 基层民警中队 where 中队名 in ('" + strRegion1.Replace(",", "','") + "')";
                    DataTable sb = GetSelTable(pSql);
                    string pStr = "";
                    for (int i = 0; i < sb.Rows.Count; i++)
                    {
                        if (pStr == "")
                            pStr = sb.Rows[i][0].ToString();
                        else
                            pStr += "," + sb.Rows[i][0].ToString();
                    }
                    sql = "select distinct(派出所代码) from 基层派出所 where 派出所名 in ('" + pStr.Replace(",", "','") + "')";
                    DataTable tb = GetSelTable(sql);

                    if (clAs.ToString() == "assigned") // 指派
                    {
                        for (int j = 0; j < tb.Rows.Count; j++)
                        {
                            whSql += " and 所属派出所代码='" + tb.Rows[j][0].ToString() + "'";
                        }
                    }
                    whSql = " and 所属中队代码 is null";
                }
                return whSql;
            }
            catch (Exception ex)
            {
                writeToLog(ex, "btnAssigned_Click");
                return null;
            }
        }

        /// <summary>
        /// 获取查询结果表
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>结果集</returns>
        private DataTable GetSelTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }
    }
}
