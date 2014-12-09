namespace clUser
{
    partial class FrmRole
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
            this.typeName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.ListQuyu = new System.Windows.Forms.CheckedListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.cbListModule = new System.Windows.Forms.CheckedListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Rolelist = new System.Windows.Forms.ListBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLook = new System.Windows.Forms.Button();
            this.btnDel = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // typeName
            // 
            this.typeName.Location = new System.Drawing.Point(291, 14);
            this.typeName.Name = "typeName";
            this.typeName.Size = new System.Drawing.Size(167, 21);
            this.typeName.TabIndex = 7;
            this.typeName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.typeName_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.Location = new System.Drawing.Point(231, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "角色名：";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Controls.Add(this.ListQuyu);
            this.groupBox2.Location = new System.Drawing.Point(226, 52);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(244, 263);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "区域权限设置";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "顺德区公安",
            "派出所",
            "民警中队"});
            this.comboBox1.Location = new System.Drawing.Point(8, 18);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(230, 20);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // ListQuyu
            // 
            this.ListQuyu.CheckOnClick = true;
            this.ListQuyu.FormattingEnabled = true;
            this.ListQuyu.Items.AddRange(new object[] {
            "大良",
            "德胜",
            "容桂",
            "勒流",
            "伦教",
            "北滘",
            "陈村",
            "龙江",
            "乐从",
            "杏坛",
            "均安"});
            this.ListQuyu.Location = new System.Drawing.Point(8, 41);
            this.ListQuyu.Name = "ListQuyu";
            this.ListQuyu.Size = new System.Drawing.Size(230, 212);
            this.ListQuyu.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Transparent;
            this.groupBox3.Controls.Add(this.checkBox1);
            this.groupBox3.Controls.Add(this.cbListModule);
            this.groupBox3.Location = new System.Drawing.Point(488, 52);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(178, 263);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "模块权限设置";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(14, 19);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(48, 16);
            this.checkBox1.TabIndex = 14;
            this.checkBox1.Text = "全选";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // cbListModule
            // 
            this.cbListModule.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.cbListModule.CheckOnClick = true;
            this.cbListModule.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbListModule.FormattingEnabled = true;
            this.cbListModule.Items.AddRange(new object[] {
            "综合查询",
            "案件分析",
            "车辆监控",
            "视频监控",
            "治安卡口",
            "人口管理",
            "房屋管理",
            "直观指挥",
            "GPS警员",
            "110接警",
            "视频编辑",
            "基础数据编辑",
            "└基础数据可删、改",
            "业务数据编辑",
            "└业务数据可删、改",
            "权限管理",
            "可导出",
            "可导入"});
            this.cbListModule.Location = new System.Drawing.Point(14, 41);
            this.cbListModule.Name = "cbListModule";
            this.cbListModule.Size = new System.Drawing.Size(160, 212);
            this.cbListModule.TabIndex = 12;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(227, 321);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(83, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "添 加";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(533, 321);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(83, 24);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "关 闭";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.Rolelist);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(192, 333);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "角色列表";
            // 
            // Rolelist
            // 
            this.Rolelist.FormattingEnabled = true;
            this.Rolelist.ItemHeight = 12;
            this.Rolelist.Location = new System.Drawing.Point(11, 21);
            this.Rolelist.Name = "Rolelist";
            this.Rolelist.Size = new System.Drawing.Size(168, 292);
            this.Rolelist.TabIndex = 0;
            this.Rolelist.SelectedIndexChanged += new System.EventHandler(this.Rolelist_SelectedIndexChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(333, 321);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(82, 24);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "更 新";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLook
            // 
            this.btnLook.Location = new System.Drawing.Point(491, 13);
            this.btnLook.Name = "btnLook";
            this.btnLook.Size = new System.Drawing.Size(81, 22);
            this.btnLook.TabIndex = 10;
            this.btnLook.Text = "查 看";
            this.btnLook.UseVisualStyleBackColor = true;
            this.btnLook.Click += new System.EventHandler(this.btnLook_Click);
            // 
            // btnDel
            // 
            this.btnDel.ForeColor = System.Drawing.Color.Red;
            this.btnDel.Location = new System.Drawing.Point(437, 321);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(80, 24);
            this.btnDel.TabIndex = 11;
            this.btnDel.Text = "删 除";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // FrmRole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::clUser.Properties.Resources.bg;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(678, 357);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnLook);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.typeName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Name = "FrmRole";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "角色管理";
            this.Load += new System.EventHandler(this.FrmRole_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox typeName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckedListBox ListQuyu;
        private System.Windows.Forms.CheckedListBox cbListModule;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox Rolelist;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLook;
        private System.Windows.Forms.Button btnDel;

    }
}