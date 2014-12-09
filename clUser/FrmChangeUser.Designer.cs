namespace clUser
{
    partial class FrmChangeUser
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
            this.textPW = new System.Windows.Forms.TextBox();
            this.textUserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textPW
            // 
            this.textPW.Location = new System.Drawing.Point(76, 67);
            this.textPW.Name = "textPW";
            this.textPW.PasswordChar = '*';
            this.textPW.Size = new System.Drawing.Size(109, 21);
            this.textPW.TabIndex = 4;
            // 
            // textUserName
            // 
            this.textUserName.Location = new System.Drawing.Point(75, 24);
            this.textUserName.Name = "textUserName";
            this.textUserName.Size = new System.Drawing.Size(110, 21);
            this.textUserName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(23, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "密 码：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(22, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "用 户：";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(34, 126);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(60, 25);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "确 定";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(152, 126);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(60, 25);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "取 消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textUserName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textPW);
            this.groupBox1.Location = new System.Drawing.Point(18, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(209, 109);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // FrmChangeUser
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::clUser.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(248, 163);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChangeUser";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "切换用户";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textPW;
        private System.Windows.Forms.TextBox textUserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}