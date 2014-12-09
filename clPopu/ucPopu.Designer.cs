namespace clPopu
{
    partial class ucPopu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucPopu));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel3 = new System.Windows.Forms.Panel();
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
            this.新建NToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.打开OToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.保存SToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.打印PToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.剪切UToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.复制CToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.粘贴PToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.帮助LToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnEnalData = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.btnAdd = new System.Windows.Forms.Button();
            this.dataGridExp = new System.Windows.Forms.DataGridView();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FieldStr = new System.Windows.Forms.ComboBox();
            this.MathStr = new System.Windows.Forms.ComboBox();
            this.connStr = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.ValueStr = new SplitWord.SpellSearchBoxEx();
            this.panel3.SuspendLayout();
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
            // panel3
            // 
            this.panel3.Controls.Add(this.dataGV);
            this.panel3.Controls.Add(this.bindingNavigator1);
            this.panel3.Controls.Add(this.groupBox1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 19);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(5);
            this.panel3.Size = new System.Drawing.Size(285, 467);
            this.panel3.TabIndex = 39;
            // 
            // dataGV
            // 
            this.dataGV.AllowUserToAddRows = false;
            this.dataGV.AllowUserToDeleteRows = false;
            this.dataGV.AllowUserToOrderColumns = true;
            this.dataGV.AllowUserToResizeRows = false;
            this.dataGV.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGV.ColumnHeadersHeight = 24;
            this.dataGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGV.Location = new System.Drawing.Point(5, 231);
            this.dataGV.MultiSelect = false;
            this.dataGV.Name = "dataGV";
            this.dataGV.ReadOnly = true;
            this.dataGV.RowHeadersVisible = false;
            this.dataGV.RowHeadersWidth = 20;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Wheat;
            this.dataGV.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGV.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dataGV.RowTemplate.Height = 23;
            this.dataGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGV.Size = new System.Drawing.Size(275, 231);
            this.dataGV.TabIndex = 40;
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
            this.toolStripLabel2,
            this.新建NToolStripButton,
            this.打开OToolStripButton,
            this.保存SToolStripButton,
            this.打印PToolStripButton,
            this.toolStripSeparator,
            this.剪切UToolStripButton,
            this.复制CToolStripButton,
            this.粘贴PToolStripButton,
            this.toolStripSeparator1,
            this.帮助LToolStripButton});
            this.bindingNavigator1.Location = new System.Drawing.Point(5, 206);
            this.bindingNavigator1.MoveFirstItem = null;
            this.bindingNavigator1.MoveLastItem = null;
            this.bindingNavigator1.MoveNextItem = null;
            this.bindingNavigator1.MovePreviousItem = null;
            this.bindingNavigator1.Name = "bindingNavigator1";
            this.bindingNavigator1.PositionItem = null;
            this.bindingNavigator1.Size = new System.Drawing.Size(275, 25);
            this.bindingNavigator1.TabIndex = 39;
            this.bindingNavigator1.Text = "bindingNavigator5";
            this.bindingNavigator1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.bindingNavigator1_ItemClicked);
            // 
            // PageFirst1
            // 
            this.PageFirst1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageFirst1.Image = global::clPopu.Properties.Resources.control_skip_180;
            this.PageFirst1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PageFirst1.Name = "PageFirst1";
            this.PageFirst1.Size = new System.Drawing.Size(23, 22);
            this.PageFirst1.Text = "转到首页";
            // 
            // PagePre1
            // 
            this.PagePre1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PagePre1.Image = global::clPopu.Properties.Resources.control_double_180;
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
            this.PageNext1.Image = global::clPopu.Properties.Resources.control_double;
            this.PageNext1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PageNext1.Name = "PageNext1";
            this.PageNext1.Size = new System.Drawing.Size(23, 22);
            this.PageNext1.Text = "下一页";
            this.PageNext1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // PageLast1
            // 
            this.PageLast1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageLast1.Image = global::clPopu.Properties.Resources.control_skip;
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
            // 新建NToolStripButton
            // 
            this.新建NToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.新建NToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("新建NToolStripButton.Image")));
            this.新建NToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.新建NToolStripButton.Name = "新建NToolStripButton";
            this.新建NToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.新建NToolStripButton.Text = "新建(&N)";
            // 
            // 打开OToolStripButton
            // 
            this.打开OToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.打开OToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("打开OToolStripButton.Image")));
            this.打开OToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.打开OToolStripButton.Name = "打开OToolStripButton";
            this.打开OToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.打开OToolStripButton.Text = "打开(&O)";
            // 
            // 保存SToolStripButton
            // 
            this.保存SToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.保存SToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("保存SToolStripButton.Image")));
            this.保存SToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.保存SToolStripButton.Name = "保存SToolStripButton";
            this.保存SToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.保存SToolStripButton.Text = "保存(&S)";
            // 
            // 打印PToolStripButton
            // 
            this.打印PToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.打印PToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("打印PToolStripButton.Image")));
            this.打印PToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.打印PToolStripButton.Name = "打印PToolStripButton";
            this.打印PToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.打印PToolStripButton.Text = "打印(&P)";
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // 剪切UToolStripButton
            // 
            this.剪切UToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.剪切UToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("剪切UToolStripButton.Image")));
            this.剪切UToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.剪切UToolStripButton.Name = "剪切UToolStripButton";
            this.剪切UToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.剪切UToolStripButton.Text = "剪切(&U)";
            // 
            // 复制CToolStripButton
            // 
            this.复制CToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.复制CToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("复制CToolStripButton.Image")));
            this.复制CToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.复制CToolStripButton.Name = "复制CToolStripButton";
            this.复制CToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.复制CToolStripButton.Text = "复制(&C)";
            // 
            // 粘贴PToolStripButton
            // 
            this.粘贴PToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.粘贴PToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("粘贴PToolStripButton.Image")));
            this.粘贴PToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.粘贴PToolStripButton.Name = "粘贴PToolStripButton";
            this.粘贴PToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.粘贴PToolStripButton.Text = "粘贴(&P)";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // 帮助LToolStripButton
            // 
            this.帮助LToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.帮助LToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("帮助LToolStripButton.Image")));
            this.帮助LToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.帮助LToolStripButton.Name = "帮助LToolStripButton";
            this.帮助LToolStripButton.Size = new System.Drawing.Size(23, 20);
            this.帮助LToolStripButton.Text = "帮助(&L)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnEnalData);
            this.groupBox1.Controls.Add(this.buttonClear);
            this.groupBox1.Controls.Add(this.buttonSearch);
            this.groupBox1.Controls.Add(this.ValueStr);
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.dateTimePicker1);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.dataGridExp);
            this.groupBox1.Controls.Add(this.FieldStr);
            this.groupBox1.Controls.Add(this.MathStr);
            this.groupBox1.Controls.Add(this.connStr);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(275, 201);
            this.groupBox1.TabIndex = 38;
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
            this.btnEnalData.TabIndex = 22;
            this.btnEnalData.Text = "放大查看数据";
            this.btnEnalData.UseVisualStyleBackColor = true;
            this.btnEnalData.Click += new System.EventHandler(this.btnEnalData_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(36)))), ((int)(((byte)(123)))));
            this.buttonClear.Image = global::clPopu.Properties.Resources.remove;
            this.buttonClear.Location = new System.Drawing.Point(158, 169);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 21);
            this.buttonClear.TabIndex = 20;
            this.buttonClear.Text = "重 置";
            this.buttonClear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonSearch
            // 
            this.buttonSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(36)))), ((int)(((byte)(123)))));
            this.buttonSearch.Image = global::clPopu.Properties.Resources.search;
            this.buttonSearch.Location = new System.Drawing.Point(46, 169);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(77, 21);
            this.buttonSearch.TabIndex = 10;
            this.buttonSearch.Text = "查 找";
            this.buttonSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(6, 129);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(62, 23);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "<< 移除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(75, 46);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(193, 21);
            this.dateTimePicker1.TabIndex = 8;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(6, 100);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(62, 23);
            this.btnAdd.TabIndex = 6;
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
            this.dataGridExp.BackgroundColor = System.Drawing.Color.White;
            this.dataGridExp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridExp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridExp.ColumnHeadersVisible = false;
            this.dataGridExp.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Value,
            this.Type});
            this.dataGridExp.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridExp.Location = new System.Drawing.Point(75, 93);
            this.dataGridExp.MultiSelect = false;
            this.dataGridExp.Name = "dataGridExp";
            this.dataGridExp.ReadOnly = true;
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
            // FieldStr
            // 
            this.FieldStr.FormattingEnabled = true;
            this.FieldStr.Location = new System.Drawing.Point(75, 20);
            this.FieldStr.Name = "FieldStr";
            this.FieldStr.Size = new System.Drawing.Size(116, 20);
            this.FieldStr.TabIndex = 0;
            this.FieldStr.SelectedIndexChanged += new System.EventHandler(this.FieldStr_SelectedIndexChanged);
            // 
            // MathStr
            // 
            this.MathStr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.MathStr.FormattingEnabled = true;
            this.MathStr.Items.AddRange(new object[] {
            "等于",
            "不等于",
            "大于",
            "大于等于",
            "小于",
            "小于等于",
            "包含"});
            this.MathStr.Location = new System.Drawing.Point(197, 20);
            this.MathStr.Name = "MathStr";
            this.MathStr.Size = new System.Drawing.Size(71, 20);
            this.MathStr.TabIndex = 1;
            // 
            // connStr
            // 
            this.connStr.FormattingEnabled = true;
            this.connStr.Items.AddRange(new object[] {
            "并且",
            "或者"});
            this.connStr.Location = new System.Drawing.Point(97, 70);
            this.connStr.Name = "connStr";
            this.connStr.Size = new System.Drawing.Size(172, 20);
            this.connStr.TabIndex = 3;
            this.connStr.Text = "并且";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "条件连接方式：";
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::clPopu.Properties.Resources.toolbarBg;
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
            this.linkLabel1.Location = new System.Drawing.Point(220, 4);
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
            this.label1.Text = "人口管理";
            // 
            // ValueStr
            // 
            this.ValueStr.Location = new System.Drawing.Point(75, 46);
            this.ValueStr.MaxItemCount = 5;
            this.ValueStr.Name = "ValueStr";
            this.ValueStr.Size = new System.Drawing.Size(193, 21);
            this.ValueStr.TabIndex = 21;
            this.ValueStr.TextChanged += new System.EventHandler(this.ValueStr_TextChanged_1);
            this.ValueStr.Click += new System.EventHandler(this.ValueStr_Click_1);
            this.ValueStr.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ValueStr_MouseDown);
            // 
            // ucPopu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "ucPopu";
            this.Size = new System.Drawing.Size(285, 486);
            this.VisibleChanged += new System.EventHandler(this.ucPopu_VisibleChanged);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
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
        private System.Windows.Forms.Panel panel3;
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
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonSearch;
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
        private System.Windows.Forms.ToolStripButton 新建NToolStripButton;
        private System.Windows.Forms.ToolStripButton 打开OToolStripButton;
        private System.Windows.Forms.ToolStripButton 保存SToolStripButton;
        private System.Windows.Forms.ToolStripButton 打印PToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton 剪切UToolStripButton;
        private System.Windows.Forms.ToolStripButton 复制CToolStripButton;
        private System.Windows.Forms.ToolStripButton 粘贴PToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton 帮助LToolStripButton;
        private SplitWord.SpellSearchBoxEx ValueStr;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnEnalData;
    }
}