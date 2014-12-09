namespace clAnjian
{
    partial class frmDotDensity
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
            this.buttonThemeColor = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.textNumPerDot = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.comboRegionLevel = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxHighLevel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonThemeColor
            // 
            this.buttonThemeColor.BackColor = System.Drawing.Color.Red;
            this.buttonThemeColor.ForeColor = System.Drawing.Color.Cyan;
            this.buttonThemeColor.Location = new System.Drawing.Point(136, 114);
            this.buttonThemeColor.Name = "buttonThemeColor";
            this.buttonThemeColor.Size = new System.Drawing.Size(24, 24);
            this.buttonThemeColor.TabIndex = 3;
            this.buttonThemeColor.UseVisualStyleBackColor = false;
            this.buttonThemeColor.Click += new System.EventHandler(this.buttonThemeColor_Click);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(7, 120);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(89, 12);
            this.label18.TabIndex = 9;
            this.label18.Text = "点密度表示颜色";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(155, 87);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(17, 12);
            this.label17.TabIndex = 8;
            this.label17.Text = "件";
            // 
            // textNumPerDot
            // 
            this.textNumPerDot.Location = new System.Drawing.Point(97, 84);
            this.textNumPerDot.Name = "textNumPerDot";
            this.textNumPerDot.Size = new System.Drawing.Size(52, 21);
            this.textNumPerDot.TabIndex = 23;
            this.textNumPerDot.Text = "2";
            this.textNumPerDot.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textNumPerDot_MouseDown);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(58, 168);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(57, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "生  成";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "每单位案件数";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(133, 168);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(57, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "取  消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // comboRegionLevel
            // 
            this.comboRegionLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRegionLevel.FormattingEnabled = true;
            this.comboRegionLevel.Items.AddRange(new object[] {
            "派出所",
            "中队",
            "警务室"});
            this.comboRegionLevel.Location = new System.Drawing.Point(97, 14);
            this.comboRegionLevel.Name = "comboRegionLevel";
            this.comboRegionLevel.Size = new System.Drawing.Size(138, 20);
            this.comboRegionLevel.TabIndex = 1;
            this.comboRegionLevel.SelectedIndexChanged += new System.EventHandler(this.comboRegionLevel_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "区域分析等级";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxHighLevel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.comboRegionLevel);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textNumPerDot);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.buttonThemeColor);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(241, 149);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            // 
            // comboBoxHighLevel
            // 
            this.comboBoxHighLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxHighLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHighLevel.Enabled = false;
            this.comboBoxHighLevel.FormattingEnabled = true;
            this.comboBoxHighLevel.Location = new System.Drawing.Point(97, 49);
            this.comboBoxHighLevel.Name = "comboBoxHighLevel";
            this.comboBoxHighLevel.Size = new System.Drawing.Size(138, 20);
            this.comboBoxHighLevel.TabIndex = 24;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 25;
            this.label1.Text = "所属上级区";
            // 
            // frmDotDensity
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(247, 203);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmDotDensity";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "点密度选项";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonThemeColor;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textNumPerDot;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ColorDialog colorDialog1;
        public System.Windows.Forms.ComboBox comboRegionLevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.ComboBox comboBoxHighLevel;
        private System.Windows.Forms.Label label1;
    }
}