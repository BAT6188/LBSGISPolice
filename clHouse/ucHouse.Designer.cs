namespace clHouse
{
    partial class ucHouse
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.dataGV = new System.Windows.Forms.DataGridView();
            this.bindingNavigator1 = new System.Windows.Forms.BindingNavigator(this.components);
            this.PageFirst1 = new System.Windows.Forms.ToolStripButton();
            this.PagePre1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.PageNow1 = new System.Windows.Forms.ToolStripTextBox();
            this.PageCount1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.PageNext1 = new System.Windows.Forms.ToolStripButton();
            this.PageLast1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.RecordCount1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.TextNum1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnEnalData = new System.Windows.Forms.Button();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.ValueStr = new SplitWord.SpellSearchBoxEx();
            this.buttonClear = new System.Windows.Forms.Button();
            this.FieldStr = new System.Windows.Forms.ComboBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.btnAdd = new System.Windows.Forms.Button();
            this.MathStr = new System.Windows.Forms.ComboBox();
            this.dataGridExp = new System.Windows.Forms.DataGridView();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.connStr = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).BeginInit();
            this.bindingNavigator1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridExp)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 400;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.dataGV);
            this.panel2.Controls.Add(this.bindingNavigator1);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 19);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(5);
            this.panel2.Size = new System.Drawing.Size(285, 470);
            this.panel2.TabIndex = 23;
            // 
            // dataGV
            // 
            this.dataGV.AllowUserToAddRows = false;
            this.dataGV.AllowUserToDeleteRows = false;
            this.dataGV.AllowUserToOrderColumns = true;
            this.dataGV.AllowUserToResizeRows = false;
            this.dataGV.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGV.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGV.ColumnHeadersHeight = 24;
            this.dataGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGV.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGV.Location = new System.Drawing.Point(5, 231);
            this.dataGV.MultiSelect = false;
            this.dataGV.Name = "dataGV";
            this.dataGV.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGV.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGV.RowHeadersVisible = false;
            this.dataGV.RowHeadersWidth = 20;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Wheat;
            this.dataGV.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGV.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dataGV.RowTemplate.Height = 23;
            this.dataGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGV.Size = new System.Drawing.Size(275, 234);
            this.dataGV.TabIndex = 41;
            this.dataGV.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGV_CellDoubleClick);
            this.dataGV.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGV_CellClick);
            // 
            // bindingNavigator1
            // 
            this.bindingNavigator1.AddNewItem = null;
            this.bindingNavigator1.BackColor = System.Drawing.SystemColors.Control;
            this.bindingNavigator1.CountItem = null;
            this.bindingNavigator1.DeleteItem = null;
            this.bindingNavigator1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.bindingNavigator1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PageFirst1,
            this.PagePre1,
            this.toolStripSeparator10,
            this.PageNow1,
            this.PageCount1,
            this.toolStripSeparator11,
            this.PageNext1,
            this.PageLast1,
            this.toolStripSeparator12,
            this.RecordCount1,
            this.toolStripSeparator13,
            this.toolStripLabel1,
            this.TextNum1,
            this.toolStripLabel2});
            this.bindingNavigator1.Location = new System.Drawing.Point(5, 206);
            this.bindingNavigator1.MoveFirstItem = null;
            this.bindingNavigator1.MoveLastItem = null;
            this.bindingNavigator1.MoveNextItem = null;
            this.bindingNavigator1.MovePreviousItem = null;
            this.bindingNavigator1.Name = "bindingNavigator1";
            this.bindingNavigator1.PositionItem = null;
            this.bindingNavigator1.Size = new System.Drawing.Size(275, 25);
            this.bindingNavigator1.TabIndex = 40;
            this.bindingNavigator1.Text = "bindingNavigator5";
            this.bindingNavigator1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.bindingNavigator1_ItemClicked);
            // 
            // PageFirst1
            // 
            this.PageFirst1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageFirst1.Image = global::clHouse.Properties.Resources.control_skip_180;
            this.PageFirst1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PageFirst1.Name = "PageFirst1";
            this.PageFirst1.Size = new System.Drawing.Size(23, 22);
            this.PageFirst1.Text = "转到首页";
            // 
            // PagePre1
            // 
            this.PagePre1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PagePre1.Image = global::clHouse.Properties.Resources.control_double_180;
            this.PagePre1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PagePre1.Name = "PagePre1";
            this.PagePre1.Size = new System.Drawing.Size(23, 22);
            this.PagePre1.Text = "上一页";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 25);
            // 
            // PageNow1
            // 
            this.PageNow1.AutoSize = false;
            this.PageNow1.Name = "PageNow1";
            this.PageNow1.Size = new System.Drawing.Size(35, 25);
            this.PageNow1.Text = "0";
            this.PageNow1.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PageNow1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ValueStr_MouseDown);
            this.PageNow1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PageNow1_KeyPress);
            // 
            // PageCount1
            // 
            this.PageCount1.Name = "PageCount1";
            this.PageCount1.Size = new System.Drawing.Size(35, 22);
            this.PageCount1.Text = "/ {0}";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
            // 
            // PageNext1
            // 
            this.PageNext1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageNext1.Image = global::clHouse.Properties.Resources.control_double;
            this.PageNext1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PageNext1.Name = "PageNext1";
            this.PageNext1.Size = new System.Drawing.Size(23, 22);
            this.PageNext1.Text = "下一页";
            // 
            // PageLast1
            // 
            this.PageLast1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageLast1.Image = global::clHouse.Properties.Resources.control_skip;
            this.PageLast1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PageLast1.Name = "PageLast1";
            this.PageLast1.Size = new System.Drawing.Size(23, 22);
            this.PageLast1.Text = "转到尾页";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 25);
            // 
            // RecordCount1
            // 
            this.RecordCount1.Name = "RecordCount1";
            this.RecordCount1.Size = new System.Drawing.Size(23, 22);
            this.RecordCount1.Text = "0条";
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(53, 12);
            this.toolStripLabel1.Text = "每页显示";
            // 
            // TextNum1
            // 
            this.TextNum1.AutoSize = false;
            this.TextNum1.Name = "TextNum1";
            this.TextNum1.Size = new System.Drawing.Size(50, 21);
            this.TextNum1.Text = "100";
            this.TextNum1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ValueStr_MouseDown);
            this.TextNum1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextNum1_KeyPress);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(41, 12);
            this.toolStripLabel2.Text = "条记录";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnEnalData);
            this.groupBox1.Controls.Add(this.buttonSearch);
            this.groupBox1.Controls.Add(this.ValueStr);
            this.groupBox1.Controls.Add(this.buttonClear);
            this.groupBox1.Controls.Add(this.FieldStr);
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.dateTimePicker1);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.MathStr);
            this.groupBox1.Controls.Add(this.dataGridExp);
            this.groupBox1.Controls.Add(this.connStr);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(275, 201);
            this.groupBox1.TabIndex = 36;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "查询表达式";
            // 
            // btnEnalData
            // 
            this.btnEnalData.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEnalData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnalData.ForeColor = System.Drawing.Color.IndianRed;
            this.btnEnalData.Location = new System.Drawing.Point(8, 20);
            this.btnEnalData.Name = "btnEnalData";
            this.btnEnalData.Size = new System.Drawing.Size(60, 47);
            this.btnEnalData.TabIndex = 23;
            this.btnEnalData.Text = "放大查看数据";
            this.btnEnalData.UseVisualStyleBackColor = true;
            this.btnEnalData.Click += new System.EventHandler(this.btnEnalData_Click);
            // 
            // buttonSearch
            // 
            this.buttonSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(36)))), ((int)(((byte)(123)))));
            this.buttonSearch.Image = global::clHouse.Properties.Resources.search;
            this.buttonSearch.Location = new System.Drawing.Point(78, 173);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(71, 21);
            this.buttonSearch.TabIndex = 21;
            this.buttonSearch.Text = "查 找";
            this.buttonSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // ValueStr
            // 
            this.ValueStr.Location = new System.Drawing.Point(78, 46);
            this.ValueStr.MaxItemCount = 5;
            this.ValueStr.Name = "ValueStr";
            this.ValueStr.Size = new System.Drawing.Size(191, 21);
            this.ValueStr.TabIndex = 22;
            this.ValueStr.TextChanged += new System.EventHandler(this.ValueStr_TextChanged_1);
            this.ValueStr.Click += new System.EventHandler(this.ValueStr_Click_1);
            this.ValueStr.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ValueStr_MouseDown);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(36)))), ((int)(((byte)(123)))));
            this.buttonClear.Image = global::clHouse.Properties.Resources.remove;
            this.buttonClear.Location = new System.Drawing.Point(174, 173);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 21);
            this.buttonClear.TabIndex = 20;
            this.buttonClear.Text = "重 置";
            this.buttonClear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // FieldStr
            // 
            this.FieldStr.FormattingEnabled = true;
            this.FieldStr.Location = new System.Drawing.Point(78, 20);
            this.FieldStr.Name = "FieldStr";
            this.FieldStr.Size = new System.Drawing.Size(115, 20);
            this.FieldStr.TabIndex = 0;
            this.FieldStr.SelectedIndexChanged += new System.EventHandler(this.FieldStr_SelectedIndexChanged);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(6, 135);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(62, 23);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "<< 移除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(78, 46);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(191, 21);
            this.dateTimePicker1.TabIndex = 8;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(6, 106);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(62, 23);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "添加 >>";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // MathStr
            // 
            this.MathStr.FormattingEnabled = true;
            this.MathStr.Items.AddRange(new object[] {
            "等于",
            "不等于",
            "大于",
            "大于等于",
            "小于",
            "小于等于",
            "包含"});
            this.MathStr.Location = new System.Drawing.Point(199, 20);
            this.MathStr.Name = "MathStr";
            this.MathStr.Size = new System.Drawing.Size(70, 20);
            this.MathStr.TabIndex = 1;
            // 
            // dataGridExp
            // 
            this.dataGridExp.AllowDrop = true;
            this.dataGridExp.AllowUserToAddRows = false;
            this.dataGridExp.AllowUserToDeleteRows = false;
            this.dataGridExp.AllowUserToResizeColumns = false;
            this.dataGridExp.AllowUserToResizeRows = false;
            this.dataGridExp.BackgroundColor = System.Drawing.Color.White;
            this.dataGridExp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridExp.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridExp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridExp.ColumnHeadersVisible = false;
            this.dataGridExp.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Value,
            this.Type});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridExp.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridExp.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridExp.Location = new System.Drawing.Point(78, 99);
            this.dataGridExp.MultiSelect = false;
            this.dataGridExp.Name = "dataGridExp";
            this.dataGridExp.ReadOnly = true;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridExp.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridExp.RowHeadersVisible = false;
            this.dataGridExp.RowTemplate.Height = 23;
            this.dataGridExp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridExp.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridExp.Size = new System.Drawing.Size(193, 68);
            this.dataGridExp.TabIndex = 5;
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
            // connStr
            // 
            this.connStr.FormattingEnabled = true;
            this.connStr.Items.AddRange(new object[] {
            "并且",
            "或者"});
            this.connStr.Location = new System.Drawing.Point(100, 73);
            this.connStr.Name = "connStr";
            this.connStr.Size = new System.Drawing.Size(172, 20);
            this.connStr.TabIndex = 3;
            this.connStr.Text = "并且";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "条件连接方式：";
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::clHouse.Properties.Resources.toolbarBg;
            this.panel1.Controls.Add(this.linkLabel1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(285, 19);
            this.panel1.TabIndex = 8;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel1.Location = new System.Drawing.Point(216, 4);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(65, 12);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "隐藏条件栏";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label1.Location = new System.Drawing.Point(5, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "房屋管理";
            // 
            // ucHouse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "ucHouse";
            this.Size = new System.Drawing.Size(285, 489);
            this.VisibleChanged += new System.EventHandler(this.ucHouse_VisibleChanged);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).EndInit();
            this.bindingNavigator1.ResumeLayout(false);
            this.bindingNavigator1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridExp)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.DataGridView dataGV;
        private System.Windows.Forms.BindingNavigator bindingNavigator1;
        private System.Windows.Forms.ToolStripButton PageFirst1;
        private System.Windows.Forms.ToolStripButton PagePre1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripTextBox PageNow1;
        private System.Windows.Forms.ToolStripLabel PageCount1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripButton PageNext1;
        private System.Windows.Forms.ToolStripButton PageLast1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripLabel RecordCount1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox TextNum1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.DataGridView dataGridExp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox connStr;
        private System.Windows.Forms.ComboBox MathStr;
        private System.Windows.Forms.ComboBox FieldStr;
        private SplitWord.SpellSearchBoxEx ValueStr;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button btnEnalData;
    }
}