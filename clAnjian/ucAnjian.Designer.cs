namespace clAnjian
{
    partial class ucAnjian
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dataGridViewJixi = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolDotDensity = new System.Windows.Forms.ToolStripButton();
            this.toolRegion = new System.Windows.Forms.ToolStripButton();
            this.toolStatics = new System.Windows.Forms.ToolStripDropDownButton();
            this.staTongHuanbi = new System.Windows.Forms.ToolStripMenuItem();
            this.staByType = new System.Windows.Forms.ToolStripMenuItem();
            this.staByDate = new System.Windows.Forms.ToolStripMenuItem();
            this.staByTime = new System.Windows.Forms.ToolStripMenuItem();
            this.tool3D = new System.Windows.Forms.ToolStripButton();
            this.tool4D = new System.Windows.Forms.ToolStripButton();
            this.panel7 = new System.Windows.Forms.Panel();
            this.LinklblHides = new System.Windows.Forms.LinkLabel();
            this.labelCount1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textValue = new SplitWord.SpellSearchBoxEx();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.dataGridExp = new System.Windows.Forms.DataGridView();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.connStr = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radio110 = new System.Windows.Forms.RadioButton();
            this.radioJingzong = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkTime = new System.Windows.Forms.CheckBox();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.timeTo = new System.Windows.Forms.DateTimePicker();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.timeFrom = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.comboField = new System.Windows.Forms.ComboBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.comboTiaojian = new System.Windows.Forms.ComboBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.checkOption = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJixi)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.panel7.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridExp)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // colorDialog1
            // 
            this.colorDialog1.FullOpen = true;
            this.colorDialog1.ShowHelp = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 400;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::clAnjian.Properties.Resources.toolbarBg;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(280, 19);
            this.panel1.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label3.Location = new System.Drawing.Point(5, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "案件分析";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ItemSize = new System.Drawing.Size(80, 20);
            this.tabControl1.Location = new System.Drawing.Point(0, 19);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 3);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(280, 504);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 19;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.Controls.Add(this.panel2);
            this.tabPage1.Controls.Add(this.toolStrip1);
            this.tabPage1.Controls.Add(this.panel7);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tabPage1.Size = new System.Drawing.Size(272, 476);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "查询分析";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dataGridViewJixi);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 312);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(269, 124);
            this.panel2.TabIndex = 21;
            // 
            // dataGridViewJixi
            // 
            this.dataGridViewJixi.AllowUserToAddRows = false;
            this.dataGridViewJixi.AllowUserToDeleteRows = false;
            this.dataGridViewJixi.AllowUserToOrderColumns = true;
            this.dataGridViewJixi.AllowUserToResizeRows = false;
            this.dataGridViewJixi.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridViewJixi.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewJixi.ColumnHeadersHeight = 24;
            this.dataGridViewJixi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewJixi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewJixi.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewJixi.Name = "dataGridViewJixi";
            this.dataGridViewJixi.ReadOnly = true;
            this.dataGridViewJixi.RowHeadersVisible = false;
            this.dataGridViewJixi.RowHeadersWidth = 20;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Wheat;
            this.dataGridViewJixi.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewJixi.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dataGridViewJixi.RowTemplate.Height = 23;
            this.dataGridViewJixi.Size = new System.Drawing.Size(269, 124);
            this.dataGridViewJixi.TabIndex = 22;
            this.dataGridViewJixi.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewJixi_CellDoubleClick);
            this.dataGridViewJixi.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewJixi_CellClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolDotDensity,
            this.toolRegion,
            this.toolStatics,
            this.tool3D,
            this.tool4D});
            this.toolStrip1.Location = new System.Drawing.Point(3, 436);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(269, 37);
            this.toolStrip1.TabIndex = 20;
            this.toolStrip1.Text = "功能项";
            // 
            // toolDotDensity
            // 
            this.toolDotDensity.ForeColor = System.Drawing.SystemColors.Desktop;
            this.toolDotDensity.Image = global::clAnjian.Properties.Resources.dotDengsity;
            this.toolDotDensity.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolDotDensity.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDotDensity.Name = "toolDotDensity";
            this.toolDotDensity.Size = new System.Drawing.Size(33, 34);
            this.toolDotDensity.Text = "密度";
            this.toolDotDensity.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolDotDensity.Click += new System.EventHandler(this.toolDotDensity_Click);
            // 
            // toolRegion
            // 
            this.toolRegion.ForeColor = System.Drawing.SystemColors.Desktop;
            this.toolRegion.Image = global::clAnjian.Properties.Resources.regionTheme;
            this.toolRegion.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolRegion.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRegion.Name = "toolRegion";
            this.toolRegion.Size = new System.Drawing.Size(33, 34);
            this.toolRegion.Text = "范围";
            this.toolRegion.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolRegion.Click += new System.EventHandler(this.toolRegion_Click);
            // 
            // toolStatics
            // 
            this.toolStatics.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.staTongHuanbi,
            this.staByType,
            this.staByDate,
            this.staByTime});
            this.toolStatics.ForeColor = System.Drawing.SystemColors.Desktop;
            this.toolStatics.Image = global::clAnjian.Properties.Resources.tongjitu;
            this.toolStatics.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStatics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStatics.Name = "toolStatics";
            this.toolStatics.Size = new System.Drawing.Size(42, 34);
            this.toolStatics.Text = "统计";
            this.toolStatics.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStatics.ToolTipText = "对结果进行统计";
            this.toolStatics.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStatics_DropDownItemClicked);
            // 
            // staTongHuanbi
            // 
            this.staTongHuanbi.Image = global::clAnjian.Properties.Resources.tongjitu;
            this.staTongHuanbi.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.staTongHuanbi.Name = "staTongHuanbi";
            this.staTongHuanbi.Size = new System.Drawing.Size(156, 24);
            this.staTongHuanbi.Text = "分区统计......";
            // 
            // staByType
            // 
            this.staByType.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.staByType.Image = global::clAnjian.Properties.Resources.staType;
            this.staByType.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.staByType.Name = "staByType";
            this.staByType.Size = new System.Drawing.Size(156, 24);
            this.staByType.Text = "按案件类别";
            // 
            // staByDate
            // 
            this.staByDate.Image = global::clAnjian.Properties.Resources.staDate;
            this.staByDate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.staByDate.Name = "staByDate";
            this.staByDate.Size = new System.Drawing.Size(156, 24);
            this.staByDate.Text = "按发案日期";
            // 
            // staByTime
            // 
            this.staByTime.BackColor = System.Drawing.SystemColors.Control;
            this.staByTime.Image = global::clAnjian.Properties.Resources.staTime;
            this.staByTime.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.staByTime.Name = "staByTime";
            this.staByTime.Size = new System.Drawing.Size(156, 24);
            this.staByTime.Text = "按发案时间";
            // 
            // tool3D
            // 
            this.tool3D.ForeColor = System.Drawing.SystemColors.Desktop;
            this.tool3D.Image = global::clAnjian.Properties.Resources._3d;
            this.tool3D.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tool3D.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tool3D.Name = "tool3D";
            this.tool3D.Size = new System.Drawing.Size(33, 34);
            this.tool3D.Text = "三维";
            this.tool3D.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tool3D.Click += new System.EventHandler(this.tool3D_Click);
            // 
            // tool4D
            // 
            this.tool4D.ForeColor = System.Drawing.SystemColors.Desktop;
            this.tool4D.Image = global::clAnjian.Properties.Resources._4d;
            this.tool4D.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tool4D.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tool4D.Name = "tool4D";
            this.tool4D.Size = new System.Drawing.Size(33, 34);
            this.tool4D.Text = "四维";
            this.tool4D.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tool4D.Click += new System.EventHandler(this.tool4D_Click);
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.SystemColors.Control;
            this.panel7.Controls.Add(this.LinklblHides);
            this.panel7.Controls.Add(this.labelCount1);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panel7.Location = new System.Drawing.Point(3, 290);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(269, 22);
            this.panel7.TabIndex = 15;
            // 
            // LinklblHides
            // 
            this.LinklblHides.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LinklblHides.AutoSize = true;
            this.LinklblHides.BackColor = System.Drawing.Color.Transparent;
            this.LinklblHides.Location = new System.Drawing.Point(200, 6);
            this.LinklblHides.Name = "LinklblHides";
            this.LinklblHides.Size = new System.Drawing.Size(65, 12);
            this.LinklblHides.TabIndex = 1;
            this.LinklblHides.TabStop = true;
            this.LinklblHides.Text = "隐藏条件栏";
            this.LinklblHides.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinklblHides_LinkClicked);
            // 
            // labelCount1
            // 
            this.labelCount1.AutoSize = true;
            this.labelCount1.ForeColor = System.Drawing.SystemColors.Desktop;
            this.labelCount1.Location = new System.Drawing.Point(4, 6);
            this.labelCount1.Name = "labelCount1";
            this.labelCount1.Size = new System.Drawing.Size(0, 12);
            this.labelCount1.TabIndex = 0;
            this.labelCount1.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.textValue);
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.dataGridExp);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.connStr);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.comboField);
            this.groupBox1.Controls.Add(this.buttonSearch);
            this.groupBox1.Controls.Add(this.comboTiaojian);
            this.groupBox1.Controls.Add(this.buttonClear);
            this.groupBox1.Controls.Add(this.checkOption);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(269, 287);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // textValue
            // 
            this.textValue.Location = new System.Drawing.Point(180, 15);
            this.textValue.MaxItemCount = 5;
            this.textValue.Name = "textValue";
            this.textValue.Size = new System.Drawing.Size(93, 21);
            this.textValue.TabIndex = 21;
            this.textValue.TextChanged += new System.EventHandler(this.textValue_TextChanged_1);
            this.textValue.Click += new System.EventHandler(this.textValue_Click_1);
            this.textValue.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textValue_MouseDown);
            this.textValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textValue_KeyPress);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(2, 101);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(62, 23);
            this.btnDelete.TabIndex = 20;
            this.btnDelete.Text = "<< 移除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(2, 72);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(62, 23);
            this.btnAdd.TabIndex = 19;
            this.btnAdd.Text = "添加 >>";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // dataGridExp
            // 
            this.dataGridExp.AllowDrop = true;
            this.dataGridExp.AllowUserToAddRows = false;
            this.dataGridExp.AllowUserToDeleteRows = false;
            this.dataGridExp.AllowUserToResizeColumns = false;
            this.dataGridExp.AllowUserToResizeRows = false;
            this.dataGridExp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridExp.BackgroundColor = System.Drawing.Color.White;
            this.dataGridExp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridExp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridExp.ColumnHeadersVisible = false;
            this.dataGridExp.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Value,
            this.Type});
            this.dataGridExp.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridExp.Location = new System.Drawing.Point(74, 65);
            this.dataGridExp.MultiSelect = false;
            this.dataGridExp.Name = "dataGridExp";
            this.dataGridExp.ReadOnly = true;
            this.dataGridExp.RowHeadersVisible = false;
            this.dataGridExp.RowTemplate.Height = 23;
            this.dataGridExp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridExp.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridExp.Size = new System.Drawing.Size(190, 68);
            this.dataGridExp.TabIndex = 18;
            // 
            // Value
            // 
            this.Value.HeaderText = "vValue";
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            this.Value.Width = 200;
            // 
            // Type
            // 
            this.Type.HeaderText = "vType";
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.Visible = false;
            this.Type.Width = 200;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "条件连接方式：";
            // 
            // connStr
            // 
            this.connStr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.connStr.FormattingEnabled = true;
            this.connStr.Items.AddRange(new object[] {
            "并且",
            "或者"});
            this.connStr.Location = new System.Drawing.Point(95, 39);
            this.connStr.Name = "connStr";
            this.connStr.Size = new System.Drawing.Size(169, 20);
            this.connStr.TabIndex = 16;
            this.connStr.Text = "并且";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.radio110);
            this.groupBox3.Controls.Add(this.radioJingzong);
            this.groupBox3.Location = new System.Drawing.Point(4, 130);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(262, 44);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "案件来源";
            // 
            // radio110
            // 
            this.radio110.AutoSize = true;
            this.radio110.Location = new System.Drawing.Point(157, 18);
            this.radio110.Name = "radio110";
            this.radio110.Size = new System.Drawing.Size(41, 16);
            this.radio110.TabIndex = 7;
            this.radio110.TabStop = true;
            this.radio110.Text = "110";
            this.radio110.UseVisualStyleBackColor = true;
            // 
            // radioJingzong
            // 
            this.radioJingzong.AutoSize = true;
            this.radioJingzong.Checked = true;
            this.radioJingzong.Location = new System.Drawing.Point(38, 18);
            this.radioJingzong.Name = "radioJingzong";
            this.radioJingzong.Size = new System.Drawing.Size(47, 16);
            this.radioJingzong.TabIndex = 6;
            this.radioJingzong.TabStop = true;
            this.radioJingzong.Text = "警综";
            this.radioJingzong.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkTime);
            this.groupBox2.Controls.Add(this.dateFrom);
            this.groupBox2.Controls.Add(this.timeTo);
            this.groupBox2.Controls.Add(this.dateTo);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.timeFrom);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(6, 181);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(257, 72);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "         ";
            // 
            // checkTime
            // 
            this.checkTime.AutoSize = true;
            this.checkTime.Checked = true;
            this.checkTime.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkTime.ForeColor = System.Drawing.SystemColors.Desktop;
            this.checkTime.Location = new System.Drawing.Point(0, 1);
            this.checkTime.Name = "checkTime";
            this.checkTime.Size = new System.Drawing.Size(72, 16);
            this.checkTime.TabIndex = 8;
            this.checkTime.Text = "发案时间";
            this.checkTime.UseVisualStyleBackColor = true;
            // 
            // dateFrom
            // 
            this.dateFrom.Location = new System.Drawing.Point(34, 20);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(129, 21);
            this.dateFrom.TabIndex = 9;
            this.dateFrom.Value = new System.DateTime(2009, 1, 1, 0, 0, 0, 0);
            // 
            // timeTo
            // 
            this.timeTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.timeTo.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timeTo.Location = new System.Drawing.Point(169, 44);
            this.timeTo.Name = "timeTo";
            this.timeTo.ShowUpDown = true;
            this.timeTo.Size = new System.Drawing.Size(77, 21);
            this.timeTo.TabIndex = 12;
            this.timeTo.Value = new System.DateTime(2008, 12, 25, 23, 59, 59, 0);
            // 
            // dateTo
            // 
            this.dateTo.Location = new System.Drawing.Point(34, 44);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(129, 21);
            this.dateTo.TabIndex = 11;
            this.dateTo.Value = new System.DateTime(2009, 3, 23, 0, 0, 0, 0);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "从";
            // 
            // timeFrom
            // 
            this.timeFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.timeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timeFrom.Location = new System.Drawing.Point(169, 20);
            this.timeFrom.Name = "timeFrom";
            this.timeFrom.ShowUpDown = true;
            this.timeFrom.Size = new System.Drawing.Size(77, 21);
            this.timeFrom.TabIndex = 10;
            this.timeFrom.Value = new System.DateTime(2008, 12, 25, 0, 0, 0, 0);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 47);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "到";
            // 
            // comboField
            // 
            this.comboField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboField.FormattingEnabled = true;
            this.comboField.Location = new System.Drawing.Point(20, 15);
            this.comboField.Name = "comboField";
            this.comboField.Size = new System.Drawing.Size(98, 20);
            this.comboField.TabIndex = 2;
            this.comboField.SelectedIndexChanged += new System.EventHandler(this.comboField_SelectedIndexChanged);
            // 
            // buttonSearch
            // 
            this.buttonSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(36)))), ((int)(((byte)(123)))));
            this.buttonSearch.Image = global::clAnjian.Properties.Resources.search;
            this.buttonSearch.Location = new System.Drawing.Point(29, 261);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(69, 22);
            this.buttonSearch.TabIndex = 13;
            this.buttonSearch.Text = "查  询";
            this.buttonSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // comboTiaojian
            // 
            this.comboTiaojian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTiaojian.FormattingEnabled = true;
            this.comboTiaojian.Items.AddRange(new object[] {
            "等于",
            "不等于",
            "大于",
            "大于等于",
            "小于",
            "小于等于",
            "包含"});
            this.comboTiaojian.Location = new System.Drawing.Point(121, 15);
            this.comboTiaojian.Name = "comboTiaojian";
            this.comboTiaojian.Size = new System.Drawing.Size(57, 20);
            this.comboTiaojian.TabIndex = 3;
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(36)))), ((int)(((byte)(123)))));
            this.buttonClear.Image = global::clAnjian.Properties.Resources.remove;
            this.buttonClear.Location = new System.Drawing.Point(150, 261);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(69, 22);
            this.buttonClear.TabIndex = 14;
            this.buttonClear.Text = "重  置";
            this.buttonClear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // checkOption
            // 
            this.checkOption.AutoSize = true;
            this.checkOption.Checked = true;
            this.checkOption.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkOption.Location = new System.Drawing.Point(4, 18);
            this.checkOption.Name = "checkOption";
            this.checkOption.Size = new System.Drawing.Size(15, 14);
            this.checkOption.TabIndex = 1;
            this.checkOption.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.AutoScroll = true;
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(272, 476);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "成效统计";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(272, 476);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "三色预警";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ucAnjian
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "ucAnjian";
            this.Size = new System.Drawing.Size(280, 523);
            this.VisibleChanged += new System.EventHandler(this.ucAnjian_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJixi)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridExp)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.DateTimePicker timeTo;
        private System.Windows.Forms.DateTimePicker timeFrom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkTime;
        private System.Windows.Forms.CheckBox checkOption;
        private System.Windows.Forms.DateTimePicker dateTo;
        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.ComboBox comboTiaojian;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.ComboBox comboField;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radio110;
        private System.Windows.Forms.RadioButton radioJingzong;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.DataGridView dataGridExp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox connStr;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label labelCount1;
        public System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolDotDensity;
        private System.Windows.Forms.ToolStripButton toolRegion;
        private System.Windows.Forms.ToolStripDropDownButton toolStatics;
        private System.Windows.Forms.ToolStripMenuItem staTongHuanbi;
        private System.Windows.Forms.ToolStripMenuItem staByType;
        private System.Windows.Forms.ToolStripMenuItem staByDate;
        private System.Windows.Forms.ToolStripMenuItem staByTime;
        private System.Windows.Forms.ToolStripButton tool3D;
        private System.Windows.Forms.ToolStripButton tool4D;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.DataGridView dataGridViewJixi;
        private SplitWord.SpellSearchBoxEx textValue;
        private System.Windows.Forms.LinkLabel LinklblHides;
    }
}