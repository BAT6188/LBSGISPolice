namespace clKaKou
{
    partial class frmBayonetDire
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
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.comBanyoneDire = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblBayonet = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(2, 34);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(77, 12);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "请选择方向：";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(81, 59);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(73, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // comBanyoneDire
            // 
            this.comBanyoneDire.FormattingEnabled = true;
            this.comBanyoneDire.Location = new System.Drawing.Point(81, 31);
            this.comBanyoneDire.Name = "comBanyoneDire";
            this.comBanyoneDire.Size = new System.Drawing.Size(167, 20);
            this.comBanyoneDire.TabIndex = 2;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(14, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(65, 12);
            this.lblName.TabIndex = 3;
            this.lblName.Text = "卡口名称：";
            // 
            // lblBayonet
            // 
            this.lblBayonet.AutoSize = true;
            this.lblBayonet.Location = new System.Drawing.Point(79, 9);
            this.lblBayonet.Name = "lblBayonet";
            this.lblBayonet.Size = new System.Drawing.Size(53, 12);
            this.lblBayonet.TabIndex = 4;
            this.lblBayonet.Text = "选择错误";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(176, 59);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(72, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "取消";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // frmBayonetDire
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 94);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblBayonet);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.comBanyoneDire);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMessage);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmBayonetDire";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "方向选择";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.ComboBox comBanyoneDire;
        public System.Windows.Forms.Label lblBayonet;
    }
}