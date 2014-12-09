namespace clGPSPolice
{
    partial class frmMajorTask
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
            this.lblAddress = new System.Windows.Forms.Label();
            this.lblUnits = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.txtCommander = new System.Windows.Forms.TextBox();
            this.lblCommander = new System.Windows.Forms.Label();
            this.lblNumber = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblRenNum = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.Location = new System.Drawing.Point(3, 13);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(89, 12);
            this.lblAddress.TabIndex = 0;
            this.lblAddress.Text = "重大任务编号：";
            // 
            // lblUnits
            // 
            this.lblUnits.AutoSize = true;
            this.lblUnits.Location = new System.Drawing.Point(3, 47);
            this.lblUnits.Name = "lblUnits";
            this.lblUnits.Size = new System.Drawing.Size(89, 12);
            this.lblUnits.TabIndex = 2;
            this.lblUnits.Text = "重大任务名称：";
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(98, 109);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(280, 134);
            this.txtMessage.TabIndex = 3;
            // 
            // txtCommander
            // 
            this.txtCommander.Location = new System.Drawing.Point(98, 75);
            this.txtCommander.Name = "txtCommander";
            this.txtCommander.Size = new System.Drawing.Size(138, 21);
            this.txtCommander.TabIndex = 2;
            // 
            // lblCommander
            // 
            this.lblCommander.AutoSize = true;
            this.lblCommander.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCommander.Location = new System.Drawing.Point(15, 78);
            this.lblCommander.Name = "lblCommander";
            this.lblCommander.Size = new System.Drawing.Size(77, 12);
            this.lblCommander.TabIndex = 1;
            this.lblCommander.Text = "任务指挥员：";
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblNumber.Location = new System.Drawing.Point(27, 112);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(65, 12);
            this.lblNumber.TabIndex = 0;
            this.lblNumber.Text = "任务详情：";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(11, 156);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(74, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(11, 203);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "取消";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblRenNum
            // 
            this.lblRenNum.AutoSize = true;
            this.lblRenNum.Location = new System.Drawing.Point(98, 13);
            this.lblRenNum.Name = "lblRenNum";
            this.lblRenNum.Size = new System.Drawing.Size(0, 12);
            this.lblRenNum.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(235, 249);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "（任务详情限字500以内）";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(98, 44);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(138, 21);
            this.txtName.TabIndex = 9;
            // 
            // frmMajorTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 268);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblRenNum);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtCommander);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblNumber);
            this.Controls.Add(this.lblCommander);
            this.Controls.Add(this.lblUnits);
            this.Controls.Add(this.lblAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMajorTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "重大任务设置";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmMajorTask_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMajorTask_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.Label lblUnits;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblCommander;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TextBox txtCommander;
        private System.Windows.Forms.Label lblRenNum;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnOK;
    }
}