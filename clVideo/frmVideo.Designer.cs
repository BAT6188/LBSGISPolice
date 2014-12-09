namespace clVideo
{
    partial class frmVideo
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtID = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtPol = new System.Windows.Forms.TextBox();
            this.txtMan = new System.Windows.Forms.TextBox();
            this.txtX = new System.Windows.Forms.TextBox();
            this.txtY = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.lblcount = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.gvVideo = new System.Windows.Forms.DataGridView();
            this.timLocation = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvVideo)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::clVideo.Properties.Resources.toolbarBg;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(289, 25);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label1.Location = new System.Drawing.Point(5, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "视频监控";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 366);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "设备编号";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 417);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "设备名称";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 468);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "所属派出所";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 517);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "日常管理人";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(47, 572);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = " 经 度";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 631);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = " 纬 度";
            // 
            // txtID
            // 
            this.txtID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtID.Location = new System.Drawing.Point(116, 364);
            this.txtID.Name = "txtID";
            this.txtID.ReadOnly = true;
            this.txtID.Size = new System.Drawing.Size(100, 20);
            this.txtID.TabIndex = 8;
            // 
            // txtName
            // 
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtName.Location = new System.Drawing.Point(116, 415);
            this.txtName.Name = "txtName";
            this.txtName.ReadOnly = true;
            this.txtName.Size = new System.Drawing.Size(100, 20);
            this.txtName.TabIndex = 9;
            // 
            // txtPol
            // 
            this.txtPol.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPol.Location = new System.Drawing.Point(116, 466);
            this.txtPol.Name = "txtPol";
            this.txtPol.ReadOnly = true;
            this.txtPol.Size = new System.Drawing.Size(100, 20);
            this.txtPol.TabIndex = 10;
            // 
            // txtMan
            // 
            this.txtMan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMan.Location = new System.Drawing.Point(115, 515);
            this.txtMan.Name = "txtMan";
            this.txtMan.ReadOnly = true;
            this.txtMan.Size = new System.Drawing.Size(100, 20);
            this.txtMan.TabIndex = 11;
            // 
            // txtX
            // 
            this.txtX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtX.Location = new System.Drawing.Point(115, 570);
            this.txtX.Name = "txtX";
            this.txtX.ReadOnly = true;
            this.txtX.Size = new System.Drawing.Size(100, 20);
            this.txtX.TabIndex = 12;
            // 
            // txtY
            // 
            this.txtY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtY.Location = new System.Drawing.Point(115, 628);
            this.txtY.Name = "txtY";
            this.txtY.ReadOnly = true;
            this.txtY.Size = new System.Drawing.Size(100, 20);
            this.txtY.TabIndex = 13;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.comboBox1);
            this.panel2.Controls.Add(this.lblcount);
            this.panel2.Controls.Add(this.btnSearch);
            this.panel2.Controls.Add(this.textBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 25);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(289, 76);
            this.panel2.TabIndex = 17;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "设备名称",
            "设备编号",
            "所属派出所",
            "日常管理人"});
            this.comboBox1.Location = new System.Drawing.Point(115, 7);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(89, 21);
            this.comboBox1.TabIndex = 20;
            this.comboBox1.Text = "设备名称";
            // 
            // lblcount
            // 
            this.lblcount.AutoSize = true;
            this.lblcount.Location = new System.Drawing.Point(5, 49);
            this.lblcount.Name = "lblcount";
            this.lblcount.Size = new System.Drawing.Size(35, 13);
            this.lblcount.TabIndex = 19;
            this.lblcount.Text = "label8";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(211, 7);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 25);
            this.btnSearch.TabIndex = 18;
            this.btnSearch.Text = "查询";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(7, 7);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(93, 20);
            this.textBox1.TabIndex = 17;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.gvVideo);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 101);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(289, 598);
            this.panel3.TabIndex = 18;
            // 
            // gvVideo
            // 
            this.gvVideo.AllowUserToAddRows = false;
            this.gvVideo.AllowUserToDeleteRows = false;
            this.gvVideo.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gvVideo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvVideo.Location = new System.Drawing.Point(0, 0);
            this.gvVideo.Name = "gvVideo";
            this.gvVideo.ReadOnly = true;
            this.gvVideo.RowHeadersVisible = false;
            this.gvVideo.RowTemplate.Height = 23;
            this.gvVideo.Size = new System.Drawing.Size(289, 598);
            this.gvVideo.TabIndex = 15;
            this.gvVideo.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.gvVideo_DataBindingComplete);
            this.gvVideo.DoubleClick += new System.EventHandler(this.gvVideo_DoubleClick);
            this.gvVideo.SelectionChanged += new System.EventHandler(this.gvVideo_SelectionChanged);
            // 
            // timLocation
            // 
            this.timLocation.Interval = 500;
            // 
            // frmVideo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(289, 699);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.txtY);
            this.Controls.Add(this.txtX);
            this.Controls.Add(this.txtMan);
            this.Controls.Add(this.txtPol);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtID);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmVideo";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "视频监控";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmVideo_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvVideo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtPol;
        private System.Windows.Forms.TextBox txtMan;
        private System.Windows.Forms.TextBox txtX;
        private System.Windows.Forms.TextBox txtY;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataGridView gvVideo;
        private System.Windows.Forms.Label lblcount;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Timer timLocation;
        //private AxBABYONLINELib.AxBabyOnline vd1;
    }
}