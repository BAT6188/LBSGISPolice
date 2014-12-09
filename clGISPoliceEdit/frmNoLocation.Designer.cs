namespace clGISPoliceEdit
{
    partial class frmNoLocation
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
            this.btnAssigned = new System.Windows.Forms.Button();
            this.btnColse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lalCla = new System.Windows.Forms.Label();
            this.btnClaim = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lalAss = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAssigned
            // 
            this.btnAssigned.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAssigned.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAssigned.ForeColor = System.Drawing.Color.IndianRed;
            this.btnAssigned.Location = new System.Drawing.Point(146, 146);
            this.btnAssigned.Name = "btnAssigned";
            this.btnAssigned.Size = new System.Drawing.Size(115, 23);
            this.btnAssigned.TabIndex = 8;
            this.btnAssigned.Text = "处理指派数据";
            this.btnAssigned.UseVisualStyleBackColor = true;
            this.btnAssigned.Click += new System.EventHandler(this.btnAssigned_Click);
            // 
            // btnColse
            // 
            this.btnColse.Location = new System.Drawing.Point(186, 6);
            this.btnColse.Name = "btnColse";
            this.btnColse.Size = new System.Drawing.Size(75, 23);
            this.btnColse.TabIndex = 9;
            this.btnColse.Text = "取消";
            this.btnColse.UseVisualStyleBackColor = true;
            this.btnColse.Click += new System.EventHandler(this.btnColse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(167, 20);
            this.label1.TabIndex = 10;
            this.label1.Text = "共有 条认领数据";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(12, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(176, 21);
            this.label2.TabIndex = 11;
            this.label2.Text = "共有 条指派数据";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lalCla);
            this.groupBox1.Controls.Add(this.btnClaim);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(283, 179);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "认领数据";
            // 
            // lalCla
            // 
            this.lalCla.AutoSize = true;
            this.lalCla.Location = new System.Drawing.Point(9, 76);
            this.lalCla.Name = "lalCla";
            this.lalCla.Size = new System.Drawing.Size(305, 12);
            this.lalCla.TabIndex = 15;
            this.lalCla.Text = "认领数据：指此条数据知道其派出所但不知道中队的数据";
            // 
            // btnClaim
            // 
            this.btnClaim.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClaim.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClaim.ForeColor = System.Drawing.Color.IndianRed;
            this.btnClaim.Location = new System.Drawing.Point(146, 142);
            this.btnClaim.Name = "btnClaim";
            this.btnClaim.Size = new System.Drawing.Size(115, 23);
            this.btnClaim.TabIndex = 14;
            this.btnClaim.Text = "处理认领数据";
            this.btnClaim.UseVisualStyleBackColor = true;
            this.btnClaim.Click += new System.EventHandler(this.btnClaim_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lalAss);
            this.groupBox2.Controls.Add(this.btnAssigned);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 179);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(283, 175);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "指派数据";
            // 
            // lalAss
            // 
            this.lalAss.AutoSize = true;
            this.lalAss.Location = new System.Drawing.Point(9, 68);
            this.lalAss.Name = "lalAss";
            this.lalAss.Size = new System.Drawing.Size(281, 12);
            this.lalAss.TabIndex = 12;
            this.lalAss.Text = "指派数据：指此条数据不知道其派出所及中队的数据";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnColse);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 354);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(283, 34);
            this.panel1.TabIndex = 14;
            // 
            // frmNoLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 388);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNoLocation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "无定位数据处理";
            this.Load += new System.EventHandler(this.frmNoLocation_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAssigned;
        private System.Windows.Forms.Button btnColse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnClaim;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lalCla;
        private System.Windows.Forms.Label lalAss;
        private System.Windows.Forms.Panel panel1;

    }
}