namespace clVideo
{
    partial class FrmSheZhi
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
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.sheZhilbl = new System.Windows.Forms.Label();
            this.proSheZhi = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "---请选择报表类型---",
            "公共安全视频监控建设总体情况表",
            "治安卡口视频监控系统明细表",
            "重点区域、场所系统联网建设情况表"});
            this.comboBox3.Location = new System.Drawing.Point(102, 12);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(215, 20);
            this.comboBox3.TabIndex = 2;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(102, 42);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "生成报表";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(234, 42);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "取消";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "生成报表类型：";
            // 
            // sheZhilbl
            // 
            this.sheZhilbl.AutoSize = true;
            this.sheZhilbl.Location = new System.Drawing.Point(31, 79);
            this.sheZhilbl.Name = "sheZhilbl";
            this.sheZhilbl.Size = new System.Drawing.Size(65, 12);
            this.sheZhilbl.TabIndex = 7;
            this.sheZhilbl.Text = "生成进度：";
            this.sheZhilbl.Visible = false;
            // 
            // proSheZhi
            // 
            this.proSheZhi.Location = new System.Drawing.Point(102, 76);
            this.proSheZhi.Name = "proSheZhi";
            this.proSheZhi.Size = new System.Drawing.Size(215, 19);
            this.proSheZhi.TabIndex = 8;
            this.proSheZhi.Visible = false;
            // 
            // FrmSheZhi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 103);
            this.Controls.Add(this.proSheZhi);
            this.Controls.Add(this.sheZhilbl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.comboBox3);
            this.MaximizeBox = false;
            this.Name = "FrmSheZhi";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "报表设置";
            this.Load += new System.EventHandler(this.FrmSheZhi_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label sheZhilbl;
        private System.Windows.Forms.ProgressBar proSheZhi;
    }
}