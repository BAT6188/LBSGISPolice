namespace clAnjian
{
    partial class frmStatic
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
            this.radioTongbi = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboYear2 = new System.Windows.Forms.ComboBox();
            this.radioCustom = new System.Windows.Forms.RadioButton();
            this.dateTimeEnd = new System.Windows.Forms.DateTimePicker();
            this.dateTimeBegin = new System.Windows.Forms.DateTimePicker();
            this.comboMonth = new System.Windows.Forms.ComboBox();
            this.comboYear = new System.Windows.Forms.ComboBox();
            this.radioMonth = new System.Windows.Forms.RadioButton();
            this.radioYear = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioZA = new System.Windows.Forms.RadioButton();
            this.radioXS = new System.Windows.Forms.RadioButton();
            this.radioAll = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radio110 = new System.Windows.Forms.RadioButton();
            this.radioJingzong = new System.Windows.Forms.RadioButton();
            this.comboBoxHighLevel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboRegion = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonReCreate = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioNumCase = new System.Windows.Forms.RadioButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioTongbi
            // 
            this.radioTongbi.AutoSize = true;
            this.radioTongbi.Location = new System.Drawing.Point(207, 17);
            this.radioTongbi.Name = "radioTongbi";
            this.radioTongbi.Size = new System.Drawing.Size(71, 16);
            this.radioTongbi.TabIndex = 13;
            this.radioTongbi.Text = "同比环比";
            this.radioTongbi.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.comboYear2);
            this.groupBox2.Controls.Add(this.radioCustom);
            this.groupBox2.Controls.Add(this.dateTimeEnd);
            this.groupBox2.Controls.Add(this.dateTimeBegin);
            this.groupBox2.Controls.Add(this.comboMonth);
            this.groupBox2.Controls.Add(this.comboYear);
            this.groupBox2.Controls.Add(this.radioMonth);
            this.groupBox2.Controls.Add(this.radioYear);
            this.groupBox2.Location = new System.Drawing.Point(5, 202);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(217, 106);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "选择统计时间";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(166, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "月";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(100, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "年";
            // 
            // comboYear2
            // 
            this.comboYear2.FormattingEnabled = true;
            this.comboYear2.Location = new System.Drawing.Point(42, 73);
            this.comboYear2.Name = "comboYear2";
            this.comboYear2.Size = new System.Drawing.Size(57, 20);
            this.comboYear2.TabIndex = 6;
            // 
            // radioCustom
            // 
            this.radioCustom.AutoSize = true;
            this.radioCustom.Location = new System.Drawing.Point(29, 108);
            this.radioCustom.Name = "radioCustom";
            this.radioCustom.Size = new System.Drawing.Size(95, 16);
            this.radioCustom.TabIndex = 8;
            this.radioCustom.Text = "自定义时间段";
            this.radioCustom.UseVisualStyleBackColor = true;
            this.radioCustom.Visible = false;
            // 
            // dateTimeEnd
            // 
            this.dateTimeEnd.Location = new System.Drawing.Point(74, 160);
            this.dateTimeEnd.Name = "dateTimeEnd";
            this.dateTimeEnd.Size = new System.Drawing.Size(109, 21);
            this.dateTimeEnd.TabIndex = 10;
            this.dateTimeEnd.Visible = false;
            // 
            // dateTimeBegin
            // 
            this.dateTimeBegin.Location = new System.Drawing.Point(74, 131);
            this.dateTimeBegin.Name = "dateTimeBegin";
            this.dateTimeBegin.Size = new System.Drawing.Size(109, 21);
            this.dateTimeBegin.TabIndex = 9;
            this.dateTimeBegin.Value = new System.DateTime(2008, 11, 7, 0, 0, 0, 0);
            this.dateTimeBegin.Visible = false;
            // 
            // comboMonth
            // 
            this.comboMonth.FormattingEnabled = true;
            this.comboMonth.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.comboMonth.Location = new System.Drawing.Point(123, 73);
            this.comboMonth.Name = "comboMonth";
            this.comboMonth.Size = new System.Drawing.Size(41, 20);
            this.comboMonth.TabIndex = 7;
            this.comboMonth.Text = "1";
            // 
            // comboYear
            // 
            this.comboYear.FormattingEnabled = true;
            this.comboYear.Location = new System.Drawing.Point(126, 22);
            this.comboYear.Name = "comboYear";
            this.comboYear.Size = new System.Drawing.Size(57, 20);
            this.comboYear.TabIndex = 4;
            // 
            // radioMonth
            // 
            this.radioMonth.AutoSize = true;
            this.radioMonth.Checked = true;
            this.radioMonth.Location = new System.Drawing.Point(29, 50);
            this.radioMonth.Name = "radioMonth";
            this.radioMonth.Size = new System.Drawing.Size(95, 16);
            this.radioMonth.TabIndex = 5;
            this.radioMonth.TabStop = true;
            this.radioMonth.Text = "以月份为基准";
            this.radioMonth.UseVisualStyleBackColor = true;
            // 
            // radioYear
            // 
            this.radioYear.AutoSize = true;
            this.radioYear.Location = new System.Drawing.Point(29, 24);
            this.radioYear.Name = "radioYear";
            this.radioYear.Size = new System.Drawing.Size(95, 16);
            this.radioYear.TabIndex = 3;
            this.radioYear.Text = "以年份为基准";
            this.radioYear.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox4);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Controls.Add(this.comboBoxHighLevel);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.comboRegion);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.buttonReCreate);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(3, 5);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.panel2.Size = new System.Drawing.Size(227, 593);
            this.panel2.TabIndex = 16;
            this.panel2.TabStop = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.radioZA);
            this.groupBox4.Controls.Add(this.radioXS);
            this.groupBox4.Controls.Add(this.radioAll);
            this.groupBox4.Location = new System.Drawing.Point(5, 140);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(217, 44);
            this.groupBox4.TabIndex = 21;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "案件类型";
            // 
            // radioZA
            // 
            this.radioZA.AutoSize = true;
            this.radioZA.Location = new System.Drawing.Point(157, 18);
            this.radioZA.Name = "radioZA";
            this.radioZA.Size = new System.Drawing.Size(47, 16);
            this.radioZA.TabIndex = 4;
            this.radioZA.TabStop = true;
            this.radioZA.Text = "行政";
            this.radioZA.UseVisualStyleBackColor = true;
            // 
            // radioXS
            // 
            this.radioXS.AutoSize = true;
            this.radioXS.Location = new System.Drawing.Point(85, 18);
            this.radioXS.Name = "radioXS";
            this.radioXS.Size = new System.Drawing.Size(47, 16);
            this.radioXS.TabIndex = 3;
            this.radioXS.TabStop = true;
            this.radioXS.Text = "刑事";
            this.radioXS.UseVisualStyleBackColor = true;
            // 
            // radioAll
            // 
            this.radioAll.AutoSize = true;
            this.radioAll.Checked = true;
            this.radioAll.Location = new System.Drawing.Point(17, 18);
            this.radioAll.Name = "radioAll";
            this.radioAll.Size = new System.Drawing.Size(47, 16);
            this.radioAll.TabIndex = 2;
            this.radioAll.TabStop = true;
            this.radioAll.Text = "全部";
            this.radioAll.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radio110);
            this.groupBox1.Controls.Add(this.radioJingzong);
            this.groupBox1.Location = new System.Drawing.Point(5, 81);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(217, 44);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "案件来源";
            // 
            // radio110
            // 
            this.radio110.AutoSize = true;
            this.radio110.Location = new System.Drawing.Point(157, 18);
            this.radio110.Name = "radio110";
            this.radio110.Size = new System.Drawing.Size(41, 16);
            this.radio110.TabIndex = 3;
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
            this.radioJingzong.TabIndex = 2;
            this.radioJingzong.TabStop = true;
            this.radioJingzong.Text = "警综";
            this.radioJingzong.UseVisualStyleBackColor = true;
            // 
            // comboBoxHighLevel
            // 
            this.comboBoxHighLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxHighLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHighLevel.Enabled = false;
            this.comboBoxHighLevel.FormattingEnabled = true;
            this.comboBoxHighLevel.Location = new System.Drawing.Point(50, 48);
            this.comboBoxHighLevel.Name = "comboBoxHighLevel";
            this.comboBoxHighLevel.Size = new System.Drawing.Size(171, 20);
            this.comboBoxHighLevel.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "所属区";
            // 
            // comboRegion
            // 
            this.comboRegion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRegion.FormattingEnabled = true;
            this.comboRegion.Items.AddRange(new object[] {
            "派出所",
            "中队",
            "警务室"});
            this.comboRegion.Location = new System.Drawing.Point(50, 15);
            this.comboRegion.Name = "comboRegion";
            this.comboRegion.Size = new System.Drawing.Size(171, 20);
            this.comboRegion.TabIndex = 1;
            this.comboRegion.SelectedIndexChanged += new System.EventHandler(this.comboRegion_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "区　域";
            // 
            // buttonReCreate
            // 
            this.buttonReCreate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.buttonReCreate.ForeColor = System.Drawing.Color.White;
            this.buttonReCreate.Location = new System.Drawing.Point(144, 319);
            this.buttonReCreate.Name = "buttonReCreate";
            this.buttonReCreate.Size = new System.Drawing.Size(75, 23);
            this.buttonReCreate.TabIndex = 11;
            this.buttonReCreate.Text = "重新生成";
            this.buttonReCreate.UseVisualStyleBackColor = false;
            this.buttonReCreate.Click += new System.EventHandler(this.buttonReCreate_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioNumCase);
            this.groupBox3.Controls.Add(this.radioTongbi);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(230, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(821, 38);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "统计方法";
            // 
            // radioNumCase
            // 
            this.radioNumCase.AutoSize = true;
            this.radioNumCase.Checked = true;
            this.radioNumCase.Location = new System.Drawing.Point(116, 17);
            this.radioNumCase.Name = "radioNumCase";
            this.radioNumCase.Size = new System.Drawing.Size(59, 16);
            this.radioNumCase.TabIndex = 12;
            this.radioNumCase.TabStop = true;
            this.radioNumCase.Text = "案件量";
            this.radioNumCase.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(230, 43);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(821, 555);
            this.panel3.TabIndex = 19;
            // 
            // frmStatic
            // 
            this.AcceptButton = this.buttonReCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1054, 601);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmStatic";
            this.Padding = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "分区统计";
            this.Load += new System.EventHandler(this.frmStatic_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioTongbi;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox comboMonth;
        private System.Windows.Forms.ComboBox comboYear;
        private System.Windows.Forms.DateTimePicker dateTimeEnd;
        private System.Windows.Forms.DateTimePicker dateTimeBegin;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioMonth;
        private System.Windows.Forms.RadioButton radioYear;
        private System.Windows.Forms.RadioButton radioCustom;
        private System.Windows.Forms.Button buttonReCreate;
        public System.Windows.Forms.ComboBox comboRegion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioNumCase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboYear2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxHighLevel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radio110;
        private System.Windows.Forms.RadioButton radioJingzong;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton radioZA;
        private System.Windows.Forms.RadioButton radioXS;
        private System.Windows.Forms.RadioButton radioAll;
    }
}