namespace clVideo
{
    partial class frmrecord
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmrecord));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.button29 = new System.Windows.Forms.Button();
            this.axBabyOnline1 = new AxBABYONLINELib.AxBabyOnline();
            this.tabControl1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axBabyOnline1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(485, 533);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.dataGridView1);
            this.tabPage4.Controls.Add(this.button4);
            this.tabPage4.Controls.Add(this.button3);
            this.tabPage4.Controls.Add(this.button2);
            this.tabPage4.Controls.Add(this.button1);
            this.tabPage4.Controls.Add(this.btnStop);
            this.tabPage4.Controls.Add(this.button5);
            this.tabPage4.Controls.Add(this.btnPlay);
            this.tabPage4.Controls.Add(this.btnDown);
            this.tabPage4.Controls.Add(this.listBox1);
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.label7);
            this.tabPage4.Controls.Add(this.dateTimePicker2);
            this.tabPage4.Controls.Add(this.dateTimePicker1);
            this.tabPage4.Controls.Add(this.button29);
            this.tabPage4.Location = new System.Drawing.Point(4, 21);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(477, 508);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "视频录像";
            this.tabPage4.UseVisualStyleBackColor = true;
            this.tabPage4.Click += new System.EventHandler(this.tabPage4_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 91);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(474, 414);
            this.dataGridView1.TabIndex = 15;
            this.dataGridView1.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dataGridView1_DataBindingComplete);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(327, 62);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(39, 23);
            this.button4.TabIndex = 14;
            this.button4.Text = ">>";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(283, 62);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(38, 23);
            this.button3.TabIndex = 13;
            this.button3.Text = "正常";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(240, 62);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(37, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "<<";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(372, 62);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "正常速度播放";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(385, 91);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(45, 23);
            this.btnStop.TabIndex = 10;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Visible = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(149, 62);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(48, 23);
            this.button5.TabIndex = 9;
            this.button5.Text = "暂停";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(99, 62);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(48, 23);
            this.btnPlay.TabIndex = 9;
            this.btnPlay.Text = "播放";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(54, 62);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(44, 23);
            this.btnDown.TabIndex = 8;
            this.btnDown.Text = "下载";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(131, 109);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(146, 76);
            this.listBox1.TabIndex = 7;
            this.listBox1.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(238, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 6;
            this.label8.Text = "终止时间";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 5;
            this.label7.Text = "起始时间";
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker2.Location = new System.Drawing.Point(240, 26);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker2.TabIndex = 4;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(10, 26);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker1.TabIndex = 3;
            // 
            // button29
            // 
            this.button29.Location = new System.Drawing.Point(3, 62);
            this.button29.Name = "button29";
            this.button29.Size = new System.Drawing.Size(48, 23);
            this.button29.TabIndex = 1;
            this.button29.Text = "查询";
            this.button29.UseVisualStyleBackColor = true;
            this.button29.Click += new System.EventHandler(this.button29_Click);
            // 
            // axBabyOnline1
            // 
            this.axBabyOnline1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axBabyOnline1.Enabled = true;
            this.axBabyOnline1.Location = new System.Drawing.Point(485, 0);
            this.axBabyOnline1.Name = "axBabyOnline1";
            this.axBabyOnline1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axBabyOnline1.OcxState")));
            this.axBabyOnline1.Size = new System.Drawing.Size(687, 533);
            this.axBabyOnline1.TabIndex = 2;
            // 
            // frmrecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1172, 533);
            this.Controls.Add(this.axBabyOnline1);
            this.Controls.Add(this.tabControl1);
            this.Name = "frmrecord";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "视频录像";
            this.Load += new System.EventHandler(this.frmrecord_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmrecord_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axBabyOnline1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button button29;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private AxBABYONLINELib.AxBabyOnline axBabyOnline1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button5;
    }
}