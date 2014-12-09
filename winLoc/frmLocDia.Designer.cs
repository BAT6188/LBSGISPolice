namespace winLoc
{
    partial class frmLocDia
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
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnXYloc = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textY = new System.Windows.Forms.TextBox();
            this.textX = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "输入地址:";
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxAddress.Location = new System.Drawing.Point(76, 17);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(231, 26);
            this.textBoxAddress.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(328, 19);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "定位";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnXYloc);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textY);
            this.groupBox1.Controls.Add(this.textX);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(3, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 75);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // btnXYloc
            // 
            this.btnXYloc.Location = new System.Drawing.Point(272, 29);
            this.btnXYloc.Name = "btnXYloc";
            this.btnXYloc.Size = new System.Drawing.Size(74, 22);
            this.btnXYloc.TabIndex = 14;
            this.btnXYloc.Text = "坐标定位";
            this.btnXYloc.UseVisualStyleBackColor = true;
            this.btnXYloc.Click += new System.EventHandler(this.btnXYloc_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(111, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "Y:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(112, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "X:";
            // 
            // textY
            // 
            this.textY.Location = new System.Drawing.Point(139, 48);
            this.textY.Name = "textY";
            this.textY.Size = new System.Drawing.Size(109, 21);
            this.textY.TabIndex = 11;
            // 
            // textX
            // 
            this.textX.Location = new System.Drawing.Point(139, 17);
            this.textX.Name = "textX";
            this.textX.Size = new System.Drawing.Size(109, 21);
            this.textX.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "输入坐标:";
            // 
            // frmLocDia
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(415, 123);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxAddress);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "frmLocDia";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "定位";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLocDia_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxAddress;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnXYloc;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textY;
        private System.Windows.Forms.TextBox textX;
        private System.Windows.Forms.Label label2;
    }
}

