namespace LBSgisPolice
{
    partial class frmEditLog
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
            this.label1 = new System.Windows.Forms.Label();
            this.timeTo = new System.Windows.Forms.DateTimePicker();
            this.timeFrom = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxUser = new System.Windows.Forms.ComboBox();
            this.comboBoxModule = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名:";
            // 
            // timeTo
            // 
            this.timeTo.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timeTo.Location = new System.Drawing.Point(170, 55);
            this.timeTo.Name = "timeTo";
            this.timeTo.ShowUpDown = true;
            this.timeTo.Size = new System.Drawing.Size(86, 21);
            this.timeTo.TabIndex = 18;
            this.timeTo.Value = new System.DateTime(2008, 12, 25, 23, 59, 59, 0);
            // 
            // timeFrom
            // 
            this.timeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timeFrom.Location = new System.Drawing.Point(170, 24);
            this.timeFrom.Name = "timeFrom";
            this.timeFrom.ShowUpDown = true;
            this.timeFrom.Size = new System.Drawing.Size(86, 21);
            this.timeFrom.TabIndex = 16;
            this.timeFrom.Value = new System.DateTime(2008, 12, 25, 0, 0, 0, 0);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "到";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 19;
            this.label2.Text = "从";
            // 
            // dateFrom
            // 
            this.dateFrom.Location = new System.Drawing.Point(33, 24);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(131, 21);
            this.dateFrom.TabIndex = 15;
            this.dateFrom.Value = new System.DateTime(2009, 6, 30, 0, 0, 0, 0);
            // 
            // dateTo
            // 
            this.dateTo.Location = new System.Drawing.Point(33, 55);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(131, 21);
            this.dateTo.TabIndex = 17;
            this.dateTo.Value = new System.DateTime(2009, 6, 30, 0, 0, 0, 0);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBoxUser);
            this.groupBox1.Controls.Add(this.comboBoxModule);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.buttonOK);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(460, 135);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            // 
            // textBoxUser
            // 
            this.textBoxUser.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxUser.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.textBoxUser.FormattingEnabled = true;
            this.textBoxUser.Location = new System.Drawing.Point(59, 14);
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.Size = new System.Drawing.Size(157, 20);
            this.textBoxUser.TabIndex = 26;
            // 
            // comboBoxModule
            // 
            this.comboBoxModule.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxModule.FormattingEnabled = true;
            this.comboBoxModule.Items.AddRange(new object[] {
            "全部",
            "综合查询",
            "案件分析",
            "车辆监控",
            "视频监控",
            "人口系统",
            "房屋系统",
            "直观指挥",
            "数据编辑",
            "登录系统",
            "退出系统"});
            this.comboBoxModule.Location = new System.Drawing.Point(318, 14);
            this.comboBoxModule.Name = "comboBoxModule";
            this.comboBoxModule.Size = new System.Drawing.Size(135, 20);
            this.comboBoxModule.TabIndex = 25;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(253, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 24;
            this.label3.Text = "操作模块:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBox1);
            this.groupBox3.Controls.Add(this.timeFrom);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.timeTo);
            this.groupBox3.Controls.Add(this.dateFrom);
            this.groupBox3.Controls.Add(this.dateTo);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(8, 45);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(269, 84);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(6, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 21;
            this.checkBox1.Text = "选择时间";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.buttonOK.ForeColor = System.Drawing.Color.White;
            this.buttonOK.Location = new System.Drawing.Point(302, 104);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(106, 23);
            this.buttonOK.TabIndex = 22;
            this.buttonOK.Text = "查看操作记录";
            this.buttonOK.UseVisualStyleBackColor = false;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.dataGridView1);
            this.groupBox2.Location = new System.Drawing.Point(6, 142);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(460, 340);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 17);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 25;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(454, 320);
            this.dataGridView1.TabIndex = 0;
            // 
            // frmEditLog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(471, 485);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmEditLog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "操作记录";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker timeTo;
        private System.Windows.Forms.DateTimePicker timeFrom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.DateTimePicker dateTo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxModule;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox textBoxUser;
    }
}