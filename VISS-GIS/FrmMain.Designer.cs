namespace LBSgisPolice110
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.toolStripToolBar = new System.Windows.Forms.ToolStrip();
            this.tsbGeoCoding = new System.Windows.Forms.ToolStripButton();
            this.tsbOpenTable = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.tsbZoomIn = new System.Windows.Forms.ToolStripButton();
            this.tsbZoomOut = new System.Windows.Forms.ToolStripButton();
            this.tsbPan = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.tsbMapLayer = new System.Windows.Forms.ToolStripButton();
            this.toolradius = new System.Windows.Forms.ToolStripButton();
            this.toolcamera = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.选择ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.周边ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolInsert = new System.Windows.Forms.ToolStripMenuItem();
            this.tooledit = new System.Windows.Forms.ToolStripMenuItem();
            this.tooldel = new System.Windows.Forms.ToolStripMenuItem();
            this.VideoClient = new System.Windows.Forms.ToolStripButton();
            this.tsbVideo = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolstatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelDo = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelView = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelCoord = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusGeoCenter = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusMapCenter = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolstausContent = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolNumber = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelMap = new System.Windows.Forms.Panel();
            this.panTip = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblVideo = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblPol = new System.Windows.Forms.Label();
            this.lblMan = new System.Windows.Forms.Label();
            this.mapControl1 = new MapInfo.Windows.Controls.MapControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rtoolopen = new System.Windows.Forms.ToolStripMenuItem();
            this.查看单个ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看周边ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rtoolAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.rtooledit = new System.Windows.Forms.ToolStripMenuItem();
            this.rtooldel = new System.Windows.Forms.ToolStripMenuItem();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbl = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.toolimage = new System.Windows.Forms.ToolStripButton();
            this.toolStripToolBar.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panelMap.SuspendLayout();
            this.panTip.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripToolBar
            // 
            this.toolStripToolBar.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.toolStripToolBar.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripToolBar.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.toolStripToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbGeoCoding,
            this.tsbOpenTable,
            this.toolStripButton2,
            this.tsbZoomIn,
            this.tsbZoomOut,
            this.tsbPan,
            this.toolStripButton1,
            this.toolimage,
            this.tsbMapLayer,
            this.toolradius,
            this.toolcamera,
            this.VideoClient,
            this.tsbVideo});
            this.toolStripToolBar.Location = new System.Drawing.Point(0, 0);
            this.toolStripToolBar.Name = "toolStripToolBar";
            this.toolStripToolBar.Size = new System.Drawing.Size(1010, 25);
            this.toolStripToolBar.Stretch = true;
            this.toolStripToolBar.TabIndex = 7;
            this.toolStripToolBar.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStripToolBar_ItemClicked);
            // 
            // tsbGeoCoding
            // 
            this.tsbGeoCoding.Image = global::MapInfo.Properties.Resources.fullextent;
            this.tsbGeoCoding.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbGeoCoding.Name = "tsbGeoCoding";
            this.tsbGeoCoding.Size = new System.Drawing.Size(75, 22);
            this.tsbGeoCoding.Tag = "GeoCoding";
            this.tsbGeoCoding.Text = "配置管理";
            // 
            // tsbOpenTable
            // 
            this.tsbOpenTable.Image = global::MapInfo.Properties.Resources.open;
            this.tsbOpenTable.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenTable.Name = "tsbOpenTable";
            this.tsbOpenTable.Size = new System.Drawing.Size(51, 22);
            this.tsbOpenTable.Tag = "Open";
            this.tsbOpenTable.Text = "打开";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.Image = global::MapInfo.Properties.Resources.selByPoint;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(51, 22);
            this.toolStripButton2.Tag = "select";
            this.toolStripButton2.Text = "选择";
            // 
            // tsbZoomIn
            // 
            this.tsbZoomIn.Image = global::MapInfo.Properties.Resources.zoom_in;
            this.tsbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbZoomIn.Name = "tsbZoomIn";
            this.tsbZoomIn.Size = new System.Drawing.Size(51, 22);
            this.tsbZoomIn.Tag = "ZoomIn";
            this.tsbZoomIn.Text = "放大";
            // 
            // tsbZoomOut
            // 
            this.tsbZoomOut.Image = global::MapInfo.Properties.Resources.zoom_out;
            this.tsbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbZoomOut.Name = "tsbZoomOut";
            this.tsbZoomOut.Size = new System.Drawing.Size(51, 22);
            this.tsbZoomOut.Tag = "ZoomOut";
            this.tsbZoomOut.Text = "缩小";
            // 
            // tsbPan
            // 
            this.tsbPan.Image = global::MapInfo.Properties.Resources.pan;
            this.tsbPan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPan.Name = "tsbPan";
            this.tsbPan.Size = new System.Drawing.Size(51, 22);
            this.tsbPan.Tag = "Pan";
            this.tsbPan.Text = "平移";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Image = global::MapInfo.Properties.Resources.Full;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(51, 22);
            this.toolStripButton1.Tag = "FullMap";
            this.toolStripButton1.Text = "全图";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // tsbMapLayer
            // 
            this.tsbMapLayer.Image = global::MapInfo.Properties.Resources.lcmap1;
            this.tsbMapLayer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMapLayer.Name = "tsbMapLayer";
            this.tsbMapLayer.Size = new System.Drawing.Size(51, 22);
            this.tsbMapLayer.Tag = "Layer";
            this.tsbMapLayer.Text = "图层";
            // 
            // toolradius
            // 
            this.toolradius.Image = global::MapInfo.Properties.Resources.selByCircle;
            this.toolradius.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolradius.Name = "toolradius";
            this.toolradius.Size = new System.Drawing.Size(75, 22);
            this.toolradius.Tag = "toolradius";
            this.toolradius.Text = "修改半径";
            // 
            // toolcamera
            // 
            this.toolcamera.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolOpen,
            this.toolInsert,
            this.tooledit,
            this.tooldel});
            this.toolcamera.Image = global::MapInfo.Properties.Resources.sxt;
            this.toolcamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolcamera.Name = "toolcamera";
            this.toolcamera.Size = new System.Drawing.Size(72, 22);
            this.toolcamera.Text = "摄像机";
            this.toolcamera.Visible = false;
            // 
            // toolOpen
            // 
            this.toolOpen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.选择ToolStripMenuItem,
            this.周边ToolStripMenuItem});
            this.toolOpen.Name = "toolOpen";
            this.toolOpen.Size = new System.Drawing.Size(152, 22);
            this.toolOpen.Text = "打开";
            this.toolOpen.Click += new System.EventHandler(this.toolOpen_Click);
            // 
            // 选择ToolStripMenuItem
            // 
            this.选择ToolStripMenuItem.Name = "选择ToolStripMenuItem";
            this.选择ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.选择ToolStripMenuItem.Text = "选择";
            this.选择ToolStripMenuItem.Click += new System.EventHandler(this.选择ToolStripMenuItem_Click);
            // 
            // 周边ToolStripMenuItem
            // 
            this.周边ToolStripMenuItem.Name = "周边ToolStripMenuItem";
            this.周边ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.周边ToolStripMenuItem.Text = "周边";
            this.周边ToolStripMenuItem.Click += new System.EventHandler(this.周边ToolStripMenuItem_Click);
            // 
            // toolInsert
            // 
            this.toolInsert.Name = "toolInsert";
            this.toolInsert.Size = new System.Drawing.Size(152, 22);
            this.toolInsert.Text = "插入";
            this.toolInsert.Click += new System.EventHandler(this.toolInsert_Click);
            // 
            // tooledit
            // 
            this.tooledit.Name = "tooledit";
            this.tooledit.Size = new System.Drawing.Size(152, 22);
            this.tooledit.Text = "修改";
            this.tooledit.Click += new System.EventHandler(this.tooledit_Click);
            // 
            // tooldel
            // 
            this.tooldel.Name = "tooldel";
            this.tooldel.Size = new System.Drawing.Size(152, 22);
            this.tooldel.Text = "删除";
            this.tooldel.Click += new System.EventHandler(this.tooldel_Click);
            // 
            // VideoClient
            // 
            this.VideoClient.Image = ((System.Drawing.Image)(resources.GetObject("VideoClient.Image")));
            this.VideoClient.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.VideoClient.Name = "VideoClient";
            this.VideoClient.Size = new System.Drawing.Size(87, 22);
            this.VideoClient.Text = "监控客户端";
            this.VideoClient.Click += new System.EventHandler(this.VideoClient_Click);
            // 
            // tsbVideo
            // 
            this.tsbVideo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbVideo.Name = "tsbVideo";
            this.tsbVideo.Size = new System.Drawing.Size(35, 22);
            this.tsbVideo.Tag = "tsbVideo";
            this.tsbVideo.Text = "退出";
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.panelMap);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1010, 448);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1010, 495);
            this.toolStripContainer1.TabIndex = 8;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStripToolBar);
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolstatus,
            this.toolStripStatusLabelDo,
            this.toolStripStatusLabelView,
            this.toolStripStatusLabelCoord,
            this.toolStripStatusGeoCenter,
            this.toolStripStatusMapCenter,
            this.toolstausContent,
            this.toolNumber});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip.Size = new System.Drawing.Size(1010, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolstatus
            // 
            this.toolstatus.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolstatus.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolstatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolstatus.Image = global::MapInfo.Properties.Resources.server;
            this.toolstatus.Name = "toolstatus";
            this.toolstatus.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.toolstatus.Size = new System.Drawing.Size(41, 17);
            this.toolstatus.Text = "IP";
            // 
            // toolStripStatusLabelDo
            // 
            this.toolStripStatusLabelDo.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabelDo.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolStripStatusLabelDo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelDo.Image = global::MapInfo.Properties.Resources.staType;
            this.toolStripStatusLabelDo.Name = "toolStripStatusLabelDo";
            this.toolStripStatusLabelDo.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.toolStripStatusLabelDo.Size = new System.Drawing.Size(55, 17);
            this.toolStripStatusLabelDo.Text = "状态";
            // 
            // toolStripStatusLabelView
            // 
            this.toolStripStatusLabelView.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabelView.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolStripStatusLabelView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelView.Image = global::MapInfo.Properties.Resources.dingwei;
            this.toolStripStatusLabelView.Name = "toolStripStatusLabelView";
            this.toolStripStatusLabelView.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.toolStripStatusLabelView.Size = new System.Drawing.Size(55, 17);
            this.toolStripStatusLabelView.Text = "缩放";
            this.toolStripStatusLabelView.ToolTipText = "地图缩放";
            // 
            // toolStripStatusLabelCoord
            // 
            this.toolStripStatusLabelCoord.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolStripStatusLabelCoord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelCoord.Image = global::MapInfo.Properties.Resources.coord;
            this.toolStripStatusLabelCoord.Name = "toolStripStatusLabelCoord";
            this.toolStripStatusLabelCoord.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.toolStripStatusLabelCoord.Size = new System.Drawing.Size(51, 17);
            this.toolStripStatusLabelCoord.Text = "坐标";
            // 
            // toolStripStatusGeoCenter
            // 
            this.toolStripStatusGeoCenter.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolStripStatusGeoCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusGeoCenter.Image = global::MapInfo.Properties.Resources.jiansuo;
            this.toolStripStatusGeoCenter.Name = "toolStripStatusGeoCenter";
            this.toolStripStatusGeoCenter.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.toolStripStatusGeoCenter.Size = new System.Drawing.Size(198, 17);
            this.toolStripStatusGeoCenter.Spring = true;
            this.toolStripStatusGeoCenter.Text = "报警区域";
            this.toolStripStatusGeoCenter.Visible = false;
            // 
            // toolStripStatusMapCenter
            // 
            this.toolStripStatusMapCenter.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolStripStatusMapCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusMapCenter.Image = global::MapInfo.Properties.Resources.Full;
            this.toolStripStatusMapCenter.Name = "toolStripStatusMapCenter";
            this.toolStripStatusMapCenter.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.toolStripStatusMapCenter.Size = new System.Drawing.Size(264, 17);
            this.toolStripStatusMapCenter.Spring = true;
            this.toolStripStatusMapCenter.Text = "地图中心";
            this.toolStripStatusMapCenter.Visible = false;
            // 
            // toolstausContent
            // 
            this.toolstausContent.Name = "toolstausContent";
            this.toolstausContent.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.toolstausContent.Size = new System.Drawing.Size(396, 17);
            this.toolstausContent.Spring = true;
            this.toolstausContent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolNumber
            // 
            this.toolNumber.Name = "toolNumber";
            this.toolNumber.Size = new System.Drawing.Size(396, 17);
            this.toolNumber.Spring = true;
            this.toolNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelMap
            // 
            this.panelMap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelMap.Controls.Add(this.panTip);
            this.panelMap.Controls.Add(this.mapControl1);
            this.panelMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMap.Location = new System.Drawing.Point(0, 0);
            this.panelMap.Name = "panelMap";
            this.panelMap.Size = new System.Drawing.Size(1010, 448);
            this.panelMap.TabIndex = 5;
            // 
            // panTip
            // 
            this.panTip.BackColor = System.Drawing.Color.Ivory;
            this.panTip.Controls.Add(this.label1);
            this.panTip.Controls.Add(this.label2);
            this.panTip.Controls.Add(this.label3);
            this.panTip.Controls.Add(this.label4);
            this.panTip.Controls.Add(this.lblVideo);
            this.panTip.Controls.Add(this.lblName);
            this.panTip.Controls.Add(this.lblPol);
            this.panTip.Controls.Add(this.lblMan);
            this.panTip.Location = new System.Drawing.Point(335, 160);
            this.panTip.Name = "panTip";
            this.panTip.Size = new System.Drawing.Size(178, 90);
            this.panTip.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "设 备 编 号:";
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "设 备 名 称:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "所属派出所 :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "日常管理人员:";
            // 
            // lblVideo
            // 
            this.lblVideo.AutoSize = true;
            this.lblVideo.Location = new System.Drawing.Point(87, 11);
            this.lblVideo.Name = "lblVideo";
            this.lblVideo.Size = new System.Drawing.Size(41, 12);
            this.lblVideo.TabIndex = 12;
            this.lblVideo.Text = "label5";
            this.lblVideo.Visible = false;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(86, 11);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(41, 12);
            this.lblName.TabIndex = 13;
            this.lblName.Text = "label6";
            // 
            // lblPol
            // 
            this.lblPol.AutoSize = true;
            this.lblPol.Location = new System.Drawing.Point(86, 39);
            this.lblPol.Name = "lblPol";
            this.lblPol.Size = new System.Drawing.Size(41, 12);
            this.lblPol.TabIndex = 14;
            this.lblPol.Text = "label7";
            // 
            // lblMan
            // 
            this.lblMan.AutoSize = true;
            this.lblMan.Location = new System.Drawing.Point(86, 66);
            this.lblMan.Name = "lblMan";
            this.lblMan.Size = new System.Drawing.Size(41, 12);
            this.lblMan.TabIndex = 15;
            this.lblMan.Text = "label8";
            // 
            // mapControl1
            // 
            this.mapControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.mapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapControl1.IgnoreLostFocusEvent = false;
            this.mapControl1.Location = new System.Drawing.Point(0, 0);
            this.mapControl1.Name = "mapControl1";
            this.mapControl1.Size = new System.Drawing.Size(1006, 444);
            this.mapControl1.TabIndex = 0;
            this.mapControl1.Text = "mapControl1";
            this.mapControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mapControl1_MouseMove);
            this.mapControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mapControl1_MouseUp);
            this.mapControl1.Tools.LeftButtonTool = null;
            this.mapControl1.Tools.MiddleButtonTool = null;
            this.mapControl1.Tools.RightButtonTool = null;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rtoolopen,
            this.rtoolAdd,
            this.rtooledit,
            this.rtooldel});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(135, 92);
            // 
            // rtoolopen
            // 
            this.rtoolopen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.查看单个ToolStripMenuItem,
            this.查看周边ToolStripMenuItem});
            this.rtoolopen.Name = "rtoolopen";
            this.rtoolopen.Size = new System.Drawing.Size(134, 22);
            this.rtoolopen.Text = "打开摄像机";
            // 
            // 查看单个ToolStripMenuItem
            // 
            this.查看单个ToolStripMenuItem.Name = "查看单个ToolStripMenuItem";
            this.查看单个ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.查看单个ToolStripMenuItem.Text = "查看单个";
            this.查看单个ToolStripMenuItem.Click += new System.EventHandler(this.查看单个ToolStripMenuItem_Click);
            // 
            // 查看周边ToolStripMenuItem
            // 
            this.查看周边ToolStripMenuItem.Name = "查看周边ToolStripMenuItem";
            this.查看周边ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.查看周边ToolStripMenuItem.Text = "查看周边";
            this.查看周边ToolStripMenuItem.Click += new System.EventHandler(this.查看周边ToolStripMenuItem_Click);
            // 
            // rtoolAdd
            // 
            this.rtoolAdd.Name = "rtoolAdd";
            this.rtoolAdd.Size = new System.Drawing.Size(134, 22);
            this.rtoolAdd.Text = "添加摄像机";
            this.rtoolAdd.Click += new System.EventHandler(this.rtoolAdd_Click);
            // 
            // rtooledit
            // 
            this.rtooledit.Name = "rtooledit";
            this.rtooledit.Size = new System.Drawing.Size(134, 22);
            this.rtooledit.Text = "修改摄像机";
            this.rtooledit.Click += new System.EventHandler(this.rtooledit_Click);
            // 
            // rtooldel
            // 
            this.rtooldel.Name = "rtooldel";
            this.rtooldel.Size = new System.Drawing.Size(134, 22);
            this.rtooldel.Text = "删除摄像机";
            this.rtooldel.Click += new System.EventHandler(this.rtooldel_Click);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 23);
            this.label5.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(0, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 23);
            this.label6.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 23);
            this.label7.TabIndex = 0;
            // 
            // lbl
            // 
            this.lbl.Location = new System.Drawing.Point(0, 0);
            this.lbl.Name = "lbl";
            this.lbl.Size = new System.Drawing.Size(100, 23);
            this.lbl.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(0, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 23);
            this.label9.TabIndex = 0;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(0, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(100, 23);
            this.label10.TabIndex = 0;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(0, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 23);
            this.label11.TabIndex = 0;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(0, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(100, 23);
            this.label12.TabIndex = 0;
            // 
            // toolimage
            // 
            this.toolimage.Image = ((System.Drawing.Image)(resources.GetObject("toolimage.Image")));
            this.toolimage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolimage.Name = "toolimage";
            this.toolimage.Size = new System.Drawing.Size(51, 22);
            this.toolimage.Tag = "toolimage";
            this.toolimage.Text = "影像";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 495);
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "FrmMain";
            this.Text = "Viss_GIS";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMain_FormClosed);
            this.Resize += new System.EventHandler(this.FrmMain_Resize);
            this.MinimumSizeChanged += new System.EventHandler(this.FrmMain_MinimumSizeChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.toolStripToolBar.ResumeLayout(false);
            this.toolStripToolBar.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelMap.ResumeLayout(false);
            this.panTip.ResumeLayout(false);
            this.panTip.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripToolBar;
        private System.Windows.Forms.ToolStripButton tsbOpenTable;
        private System.Windows.Forms.ToolStripButton tsbMapLayer;
        private System.Windows.Forms.ToolStripButton tsbZoomIn;
        private System.Windows.Forms.ToolStripButton tsbZoomOut;
        private System.Windows.Forms.ToolStripButton tsbPan;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelView;
        private System.Windows.Forms.ToolStripStatusLabel toolstatus;
        private System.Windows.Forms.Panel panelMap;
        private MapInfo.Windows.Controls.MapControl mapControl1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCoord;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelDo;
        private System.Windows.Forms.ToolStripButton tsbGeoCoding;
        private System.Windows.Forms.ToolStripButton tsbVideo;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusGeoCenter;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusMapCenter;
        public System.Windows.Forms.Label lblMan;
        public System.Windows.Forms.Label lblPol;
        public System.Windows.Forms.Label lblName;
        public System.Windows.Forms.Label lblVideo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panTip;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label lbl;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ToolStripStatusLabel toolstausContent;
        private System.Windows.Forms.ToolStripStatusLabel toolNumber;
        private System.Windows.Forms.ToolStripDropDownButton toolcamera;
        private System.Windows.Forms.ToolStripMenuItem toolInsert;
        private System.Windows.Forms.ToolStripMenuItem tooledit;
        private System.Windows.Forms.ToolStripMenuItem tooldel;
        private System.Windows.Forms.ToolStripMenuItem toolOpen;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rtoolAdd;
        private System.Windows.Forms.ToolStripMenuItem rtooledit;
        private System.Windows.Forms.ToolStripMenuItem rtooldel;
        private System.Windows.Forms.ToolStripMenuItem rtoolopen;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem 选择ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 周边ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看单个ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看周边ToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton VideoClient;
        private System.Windows.Forms.ToolStripButton toolradius;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolimage;     
    }
}

