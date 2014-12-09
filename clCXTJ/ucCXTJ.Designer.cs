namespace clCXTJ
{
    partial class ucCXTJ
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

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timeFrom = new System.Windows.Forms.DateTimePicker();
            this.timeTo = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radio110 = new System.Windows.Forms.RadioButton();
            this.radioJingzong = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxZD = new System.Windows.Forms.ComboBox();
            this.radioZD = new System.Windows.Forms.RadioButton();
            this.radioPCS = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioMonth = new System.Windows.Forms.RadioButton();
            this.radioWeek = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // dateFrom
            // 
            this.dateFrom.Location = new System.Drawing.Point(25, 20);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(129, 21);
            this.dateFrom.TabIndex = 11;
            this.dateFrom.Value = new System.DateTime(2008, 4, 15, 0, 0, 0, 0);
            // 
            // dateTo
            // 
            this.dateTo.Location = new System.Drawing.Point(25, 46);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(129, 21);
            this.dateTo.TabIndex = 13;
            this.dateTo.Value = new System.DateTime(2008, 7, 14, 0, 0, 0, 0);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "从";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 17;
            this.label4.Text = "到";
            // 
            // timeFrom
            // 
            this.timeFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.timeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timeFrom.Location = new System.Drawing.Point(160, 20);
            this.timeFrom.Name = "timeFrom";
            this.timeFrom.ShowUpDown = true;
            this.timeFrom.Size = new System.Drawing.Size(90, 21);
            this.timeFrom.TabIndex = 12;
            this.timeFrom.Value = new System.DateTime(2008, 12, 25, 0, 0, 0, 0);
            // 
            // timeTo
            // 
            this.timeTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.timeTo.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timeTo.Location = new System.Drawing.Point(160, 46);
            this.timeTo.Name = "timeTo";
            this.timeTo.ShowUpDown = true;
            this.timeTo.Size = new System.Drawing.Size(90, 21);
            this.timeTo.TabIndex = 14;
            this.timeTo.Value = new System.DateTime(2008, 12, 25, 23, 59, 59, 0);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(176, 272);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(82, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "生成";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.Desktop;
            this.label5.Location = new System.Drawing.Point(7, 310);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(250, 35);
            this.label5.TabIndex = 12;
            this.label5.Text = "提示:使用工具栏上的选择工具,双击地图上区域查看相应的成效趋势分析";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.radio110);
            this.groupBox3.Controls.Add(this.radioJingzong);
            this.groupBox3.Location = new System.Drawing.Point(8, 124);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(264, 47);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "案件来源";
            // 
            // radio110
            // 
            this.radio110.AutoSize = true;
            this.radio110.Location = new System.Drawing.Point(113, 21);
            this.radio110.Name = "radio110";
            this.radio110.Size = new System.Drawing.Size(41, 16);
            this.radio110.TabIndex = 9;
            this.radio110.TabStop = true;
            this.radio110.Text = "110";
            this.radio110.UseVisualStyleBackColor = true;
            // 
            // radioJingzong
            // 
            this.radioJingzong.AutoSize = true;
            this.radioJingzong.Checked = true;
            this.radioJingzong.Location = new System.Drawing.Point(20, 21);
            this.radioJingzong.Name = "radioJingzong";
            this.radioJingzong.Size = new System.Drawing.Size(47, 16);
            this.radioJingzong.TabIndex = 8;
            this.radioJingzong.TabStop = true;
            this.radioJingzong.Text = "警综";
            this.radioJingzong.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxZD);
            this.groupBox2.Controls.Add(this.radioZD);
            this.groupBox2.Controls.Add(this.radioPCS);
            this.groupBox2.Location = new System.Drawing.Point(8, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(264, 47);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "统计范围";
            // 
            // comboBoxZD
            // 
            this.comboBoxZD.Enabled = false;
            this.comboBoxZD.FormattingEnabled = true;
            this.comboBoxZD.Location = new System.Drawing.Point(161, 20);
            this.comboBoxZD.Name = "comboBoxZD";
            this.comboBoxZD.Size = new System.Drawing.Size(89, 20);
            this.comboBoxZD.TabIndex = 3;
            // 
            // radioZD
            // 
            this.radioZD.AutoSize = true;
            this.radioZD.Location = new System.Drawing.Point(113, 21);
            this.radioZD.Name = "radioZD";
            this.radioZD.Size = new System.Drawing.Size(47, 16);
            this.radioZD.TabIndex = 2;
            this.radioZD.Text = "中队";
            this.radioZD.UseVisualStyleBackColor = true;
            this.radioZD.CheckedChanged += new System.EventHandler(this.radioZD_CheckedChanged);
            // 
            // radioPCS
            // 
            this.radioPCS.AutoSize = true;
            this.radioPCS.Checked = true;
            this.radioPCS.Location = new System.Drawing.Point(20, 21);
            this.radioPCS.Name = "radioPCS";
            this.radioPCS.Size = new System.Drawing.Size(59, 16);
            this.radioPCS.TabIndex = 1;
            this.radioPCS.TabStop = true;
            this.radioPCS.Text = "派出所";
            this.radioPCS.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.radioMonth);
            this.groupBox4.Controls.Add(this.radioWeek);
            this.groupBox4.Location = new System.Drawing.Point(8, 66);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(264, 47);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "统计周期";
            // 
            // radioMonth
            // 
            this.radioMonth.AutoSize = true;
            this.radioMonth.Location = new System.Drawing.Point(113, 21);
            this.radioMonth.Name = "radioMonth";
            this.radioMonth.Size = new System.Drawing.Size(35, 16);
            this.radioMonth.TabIndex = 6;
            this.radioMonth.Text = "月";
            this.radioMonth.UseVisualStyleBackColor = true;
            // 
            // radioWeek
            // 
            this.radioWeek.AutoSize = true;
            this.radioWeek.Checked = true;
            this.radioWeek.Location = new System.Drawing.Point(18, 21);
            this.radioWeek.Name = "radioWeek";
            this.radioWeek.Size = new System.Drawing.Size(35, 16);
            this.radioWeek.TabIndex = 5;
            this.radioWeek.TabStop = true;
            this.radioWeek.Text = "周";
            this.radioWeek.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.dateFrom);
            this.groupBox5.Controls.Add(this.timeTo);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Controls.Add(this.dateTo);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.timeFrom);
            this.groupBox5.Location = new System.Drawing.Point(8, 182);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(264, 78);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "统计时间";
            // 
            // ucCXTJ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button1);
            this.Name = "ucCXTJ";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Size = new System.Drawing.Size(280, 577);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.DateTimePicker dateTo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker timeFrom;
        private System.Windows.Forms.DateTimePicker timeTo;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radio110;
        private System.Windows.Forms.RadioButton radioJingzong;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.ComboBox comboBoxZD;
        private System.Windows.Forms.RadioButton radioZD;
        private System.Windows.Forms.RadioButton radioPCS;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton radioMonth;
        private System.Windows.Forms.RadioButton radioWeek;
        private System.Windows.Forms.GroupBox groupBox5;
    }
}
