namespace clGISPoliceEdit
{
    partial class frmClaim
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.checkAll = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comCliam = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.Select = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.编号 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.名称 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Select,
            this.编号,
            this.名称});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(432, 419);
            this.dataGridView1.TabIndex = 0;
            // 
            // checkAll
            // 
            this.checkAll.AutoSize = true;
            this.checkAll.Location = new System.Drawing.Point(12, 425);
            this.checkAll.Name = "checkAll";
            this.checkAll.Size = new System.Drawing.Size(48, 16);
            this.checkAll.TabIndex = 1;
            this.checkAll.Text = "全选";
            this.checkAll.UseVisualStyleBackColor = true;
            this.checkAll.CheckedChanged += new System.EventHandler(this.checkAll_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comCliam);
            this.groupBox1.Location = new System.Drawing.Point(12, 447);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 53);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "认领单位";
            // 
            // comCliam
            // 
            this.comCliam.FormattingEnabled = true;
            this.comCliam.Location = new System.Drawing.Point(29, 20);
            this.comCliam.Name = "comCliam";
            this.comCliam.Size = new System.Drawing.Size(152, 20);
            this.comCliam.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(218, 467);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(323, 467);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Select
            // 
            this.Select.HeaderText = "选择";
            this.Select.Name = "Select";
            this.Select.Width = 40;
            // 
            // 编号
            // 
            this.编号.HeaderText = "编号";
            this.编号.Name = "编号";
            this.编号.Width = 190;
            // 
            // 名称
            // 
            this.名称.HeaderText = "名称";
            this.名称.Name = "名称";
            this.名称.Width = 195;
            // 
            // frmClaim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 504);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkAll);
            this.Controls.Add(this.dataGridView1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmClaim";
            this.Text = "认领数据";
            this.Load += new System.EventHandler(this.frmClaim_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.CheckBox checkAll;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comCliam;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Select;
        private System.Windows.Forms.DataGridViewTextBoxColumn 编号;
        private System.Windows.Forms.DataGridViewTextBoxColumn 名称;
    }
}