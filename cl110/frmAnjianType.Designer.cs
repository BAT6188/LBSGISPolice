namespace cl110
{
    partial class frmAnjianType
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
            this.listBoxStart = new System.Windows.Forms.ListBox();
            this.listBoxEnd = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtRadius = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxStart
            // 
            this.listBoxStart.FormattingEnabled = true;
            this.listBoxStart.ItemHeight = 12;
            this.listBoxStart.Location = new System.Drawing.Point(6, 33);
            this.listBoxStart.Name = "listBoxStart";
            this.listBoxStart.Size = new System.Drawing.Size(120, 124);
            this.listBoxStart.TabIndex = 0;
            // 
            // listBoxEnd
            // 
            this.listBoxEnd.FormattingEnabled = true;
            this.listBoxEnd.ItemHeight = 12;
            this.listBoxEnd.Location = new System.Drawing.Point(175, 33);
            this.listBoxEnd.Name = "listBoxEnd";
            this.listBoxEnd.Size = new System.Drawing.Size(120, 124);
            this.listBoxEnd.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(287, 36);
            this.label2.TabIndex = 3;
            this.label2.Text = "说明： \"刑事案件\"和\"治安案件\"为默认查询必须显示\r\n\r\n       的案件类型。";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(37, 289);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "保存";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(199, 289);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "取消";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRight
            // 
            this.btnRight.Location = new System.Drawing.Point(132, 61);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(38, 23);
            this.btnRight.TabIndex = 6;
            this.btnRight.Text = ">>";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Location = new System.Drawing.Point(132, 111);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(38, 23);
            this.btnLeft.TabIndex = 7;
            this.btnLeft.Text = "<<";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "所有类型";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(200, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "要查询类型";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "请输入查询半径：";
            // 
            // txtRadius
            // 
            this.txtRadius.Location = new System.Drawing.Point(113, 23);
            this.txtRadius.Name = "txtRadius";
            this.txtRadius.Size = new System.Drawing.Size(156, 21);
            this.txtRadius.TabIndex = 11;
            this.txtRadius.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtRadius_KeyPress);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtRadius);
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(301, 52);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "查询半径设置";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.btnLeft);
            this.groupBox2.Controls.Add(this.listBoxStart);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btnRight);
            this.groupBox2.Controls.Add(this.listBoxEnd);
            this.groupBox2.Location = new System.Drawing.Point(5, 63);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(301, 220);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "抽取数据案件类型设置";
            // 
            // frmAnjianType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 318);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.Name = "frmAnjianType";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmAnjianType_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxStart;
        private System.Windows.Forms.ListBox listBoxEnd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtRadius;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}