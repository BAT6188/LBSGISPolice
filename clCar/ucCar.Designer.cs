namespace clCar
{
    partial class ucCar
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
            this.timLocation = new System.Windows.Forms.Timer(this.components);
            this.timeCar = new System.Windows.Forms.Timer(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.bindingNavigator1 = new System.Windows.Forms.BindingNavigator(this.components);
            this.PageFirst1 = new System.Windows.Forms.ToolStripButton();
            this.PagePre1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.PageNow1 = new System.Windows.Forms.ToolStripTextBox();
            this.PageNum1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.PageNext1 = new System.Windows.Forms.ToolStripButton();
            this.PageLast1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.RecordCount1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.TextNum1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolgz = new System.Windows.Forms.ToolStripMenuItem();
            this.CaseSearchBox = new SplitWord.SpellSearchBoxEx();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).BeginInit();
            this.bindingNavigator1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timLocation
            // 
            this.timLocation.Interval = 500;
            this.timLocation.Tick += new System.EventHandler(this.timLocation_Tick);
            // 
            // timeCar
            // 
            this.timeCar.Interval = 10000;
            this.timeCar.Tick += new System.EventHandler(this.timeCar_Tick);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.comboBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(272, 32);
            this.panel2.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 12);
            this.label2.TabIndex = 28;
            // 
            // button1
            // 
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(36)))), ((int)(((byte)(123)))));
            this.button1.Image = global::clCar.Properties.Resources.search;
            this.button1.Location = new System.Drawing.Point(207, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(62, 20);
            this.button1.TabIndex = 27;
            this.button1.Text = "查询";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "终端车辆号牌",
            "所属单位"});
            this.comboBox1.Location = new System.Drawing.Point(104, 6);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(100, 20);
            this.comboBox1.TabIndex = 26;
            this.comboBox1.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dataGridView1);
            this.panel3.Controls.Add(this.bindingNavigator1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 56);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(272, 289);
            this.panel3.TabIndex = 26;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 25);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(272, 264);
            this.dataGridView1.TabIndex = 21;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_ColumnHeaderMouseClick);
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dataGridView1_DataBindingComplete);
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
            this.PageNum1,
            this.toolStripSeparator11,
            this.PageNext1,
            this.PageLast1,
            this.toolStripSeparator12,
            this.RecordCount1,
            this.toolStripSeparator13,
            this.toolStripLabel1,
            this.TextNum1,
            this.toolStripLabel2});
            this.bindingNavigator1.Location = new System.Drawing.Point(0, 0);
            this.bindingNavigator1.MoveFirstItem = null;
            this.bindingNavigator1.MoveLastItem = null;
            this.bindingNavigator1.MoveNextItem = null;
            this.bindingNavigator1.MovePreviousItem = null;
            this.bindingNavigator1.Name = "bindingNavigator1";
            this.bindingNavigator1.PositionItem = null;
            this.bindingNavigator1.Size = new System.Drawing.Size(272, 25);
            this.bindingNavigator1.TabIndex = 44;
            this.bindingNavigator1.Text = "bindingNavigator1";
            this.bindingNavigator1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.bindingNavigator1_ItemClicked);
            // 
            // PageFirst1
            // 
            this.PageFirst1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageFirst1.Image = global::clCar.Properties.Resources.control_skip_180;
            this.PageFirst1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PageFirst1.Name = "PageFirst1";
            this.PageFirst1.Size = new System.Drawing.Size(23, 22);
            this.PageFirst1.Text = "转到首页";
            // 
            // PagePre1
            // 
            this.PagePre1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PagePre1.Image = global::clCar.Properties.Resources.control_double_180;
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
            this.PageNow1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PageNow1_KeyPress);
            // 
            // PageNum1
            // 
            this.PageNum1.Name = "PageNum1";
            this.PageNum1.Size = new System.Drawing.Size(35, 22);
            this.PageNum1.Text = "/ {0}";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
            // 
            // PageNext1
            // 
            this.PageNext1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageNext1.Image = global::clCar.Properties.Resources.control_double;
            this.PageNext1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PageNext1.Name = "PageNext1";
            this.PageNext1.Size = new System.Drawing.Size(23, 22);
            this.PageNext1.Text = "下一页";
            // 
            // PageLast1
            // 
            this.PageLast1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PageLast1.Image = global::clCar.Properties.Resources.control_skip;
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
            this.RecordCount1.Size = new System.Drawing.Size(59, 22);
            this.RecordCount1.Text = "共0条记录";
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
            this.TextNum1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextNum1_KeyPress);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(41, 12);
            this.toolStripLabel2.Text = "条记录";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick_1);
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::clCar.Properties.Resources.toolbarBg;
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(272, 24);
            this.panel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(87, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 30;
            this.button2.Text = "开始";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(194, 1);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 29;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
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
            this.label1.Text = "车辆监控";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolgz});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(95, 26);
            // 
            // toolgz
            // 
            this.toolgz.Name = "toolgz";
            this.toolgz.Size = new System.Drawing.Size(94, 22);
            this.toolgz.Text = "跟踪";
            this.toolgz.Click += new System.EventHandler(this.toolgz_Click);
            // 
            // CaseSearchBox
            // 
            this.CaseSearchBox.Location = new System.Drawing.Point(2, 29);
            this.CaseSearchBox.MaxItemCount = 5;
            this.CaseSearchBox.Name = "CaseSearchBox";
            this.CaseSearchBox.Size = new System.Drawing.Size(100, 21);
            this.CaseSearchBox.TabIndex = 29;
            this.CaseSearchBox.TextChanged += new System.EventHandler(this.CaseSearchBox_TextChanged);
            this.CaseSearchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CaseSearchBox_KeyDown);
            this.CaseSearchBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CaseSearchBox_MouseDown);
            // 
            // ucCar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CaseSearchBox);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "ucCar";
            this.Size = new System.Drawing.Size(272, 345);
            this.VisibleChanged += new System.EventHandler(this.ucCar_VisibleChanged);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).EndInit();
            this.bindingNavigator1.ResumeLayout(false);
            this.bindingNavigator1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timLocation;
        public System.Windows.Forms.Timer timeCar;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Panel panel3;
        public  System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.BindingNavigator bindingNavigator1;
        private System.Windows.Forms.ToolStripButton PageFirst1;
        private System.Windows.Forms.ToolStripButton PagePre1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripTextBox PageNow1;
        private System.Windows.Forms.ToolStripLabel PageNum1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripButton PageNext1;
        private System.Windows.Forms.ToolStripButton PageLast1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripLabel RecordCount1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox TextNum1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.BindingSource bindingSource1;
        private SplitWord.SpellSearchBoxEx CaseSearchBox;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolgz;
    }
}