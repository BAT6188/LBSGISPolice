namespace clAnjian
{
    partial class frmRegionOption
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
            this.CmbMethod = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownClass = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.BtnColL = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.BtnColH = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.comboRegionLevel = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.radioClass = new System.Windows.Forms.RadioButton();
            this.radioBar = new System.Windows.Forms.RadioButton();
            this.radioPie = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxHighLevel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownClass)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CmbMethod
            // 
            this.CmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbMethod.FormattingEnabled = true;
            this.CmbMethod.Items.AddRange(new object[] {
            "数量相等",
            "范围相等",
            "自然间隔",
            "标准偏差",
            "分位数"});
            this.CmbMethod.Location = new System.Drawing.Point(78, 119);
            this.CmbMethod.Name = "CmbMethod";
            this.CmbMethod.Size = new System.Drawing.Size(137, 20);
            this.CmbMethod.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 122);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "分级方法";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownClass
            // 
            this.numericUpDownClass.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDownClass.Location = new System.Drawing.Point(78, 150);
            this.numericUpDownClass.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownClass.Name = "numericUpDownClass";
            this.numericUpDownClass.Size = new System.Drawing.Size(54, 23);
            this.numericUpDownClass.TabIndex = 8;
            this.numericUpDownClass.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 154);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "范围等级数";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BtnColL
            // 
            this.BtnColL.BackColor = System.Drawing.Color.Yellow;
            this.BtnColL.Location = new System.Drawing.Point(191, 83);
            this.BtnColL.Name = "BtnColL";
            this.BtnColL.Size = new System.Drawing.Size(24, 24);
            this.BtnColL.TabIndex = 6;
            this.BtnColL.UseVisualStyleBackColor = false;
            this.BtnColL.Click += new System.EventHandler(this.BtnColL_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(130, 89);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 13;
            this.label8.Text = "低值颜色";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BtnColH
            // 
            this.BtnColH.BackColor = System.Drawing.Color.Red;
            this.BtnColH.ForeColor = System.Drawing.Color.Cyan;
            this.BtnColH.Location = new System.Drawing.Point(72, 83);
            this.BtnColH.Name = "BtnColH";
            this.BtnColH.Size = new System.Drawing.Size(24, 24);
            this.BtnColH.TabIndex = 5;
            this.BtnColH.UseVisualStyleBackColor = false;
            this.BtnColH.Click += new System.EventHandler(this.BtnColH_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 89);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 12;
            this.label9.Text = "高值颜色";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(137, 233);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 24);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(35, 233);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(67, 24);
            this.buttonOk.TabIndex = 9;
            this.buttonOk.Text = "生成";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // comboRegionLevel
            // 
            this.comboRegionLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRegionLevel.FormattingEnabled = true;
            this.comboRegionLevel.Items.AddRange(new object[] {
            "派出所",
            "中队",
            "警务室"});
            this.comboRegionLevel.Location = new System.Drawing.Point(77, 15);
            this.comboRegionLevel.Name = "comboRegionLevel";
            this.comboRegionLevel.Size = new System.Drawing.Size(137, 20);
            this.comboRegionLevel.TabIndex = 4;
            this.comboRegionLevel.SelectedIndexChanged += new System.EventHandler(this.comboRegionLevel_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "分析区域";
            // 
            // radioClass
            // 
            this.radioClass.AutoSize = true;
            this.radioClass.Checked = true;
            this.radioClass.Location = new System.Drawing.Point(23, 8);
            this.radioClass.Name = "radioClass";
            this.radioClass.Size = new System.Drawing.Size(47, 16);
            this.radioClass.TabIndex = 1;
            this.radioClass.TabStop = true;
            this.radioClass.Text = "分级";
            this.radioClass.UseVisualStyleBackColor = true;
            // 
            // radioBar
            // 
            this.radioBar.AutoSize = true;
            this.radioBar.Location = new System.Drawing.Point(85, 8);
            this.radioBar.Name = "radioBar";
            this.radioBar.Size = new System.Drawing.Size(59, 16);
            this.radioBar.TabIndex = 2;
            this.radioBar.TabStop = true;
            this.radioBar.Text = "柱状图";
            this.radioBar.UseVisualStyleBackColor = true;
            // 
            // radioPie
            // 
            this.radioPie.AutoSize = true;
            this.radioPie.Location = new System.Drawing.Point(156, 8);
            this.radioPie.Name = "radioPie";
            this.radioPie.Size = new System.Drawing.Size(59, 16);
            this.radioPie.TabIndex = 3;
            this.radioPie.TabStop = true;
            this.radioPie.Text = "饼状图";
            this.radioPie.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxHighLevel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.comboRegionLevel);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.BtnColH);
            this.groupBox1.Controls.Add(this.CmbMethod);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.BtnColL);
            this.groupBox1.Controls.Add(this.numericUpDownClass);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(6, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 187);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            // 
            // comboBoxHighLevel
            // 
            this.comboBoxHighLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxHighLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHighLevel.Enabled = false;
            this.comboBoxHighLevel.FormattingEnabled = true;
            this.comboBoxHighLevel.Location = new System.Drawing.Point(77, 49);
            this.comboBoxHighLevel.Name = "comboBoxHighLevel";
            this.comboBoxHighLevel.Size = new System.Drawing.Size(137, 20);
            this.comboBoxHighLevel.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "所属上级区";
            // 
            // frmRegionOption
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(243, 270);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.radioPie);
            this.Controls.Add(this.radioBar);
            this.Controls.Add(this.radioClass);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmRegionOption";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "选项";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownClass)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox CmbMethod;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.NumericUpDown numericUpDownClass;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.Button BtnColL;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.Button BtnColH;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        public System.Windows.Forms.ComboBox comboRegionLevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColorDialog colorDialog1;
        public System.Windows.Forms.RadioButton radioClass;
        public System.Windows.Forms.RadioButton radioBar;
        public System.Windows.Forms.RadioButton radioPie;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.ComboBox comboBoxHighLevel;
        private System.Windows.Forms.Label label1;
    }
}