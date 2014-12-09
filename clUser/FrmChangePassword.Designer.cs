namespace clUser
{
    partial class FrmChangePassword
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbUser = new System.Windows.Forms.TextBox();
            this.textPW = new System.Windows.Forms.TextBox();
            this.textPWNew = new System.Windows.Forms.TextBox();
            this.textPWNewAgain = new System.Windows.Forms.TextBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(31, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "用户名：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(31, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "旧密码：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(31, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "新密码：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(31, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "新密码：";
            // 
            // cbUser
            // 
            this.cbUser.Location = new System.Drawing.Point(98, 23);
            this.cbUser.Name = "cbUser";
            this.cbUser.ReadOnly = true;
            this.cbUser.Size = new System.Drawing.Size(100, 21);
            this.cbUser.TabIndex = 2;
            // 
            // textPW
            // 
            this.textPW.Location = new System.Drawing.Point(98, 67);
            this.textPW.Name = "textPW";
            this.textPW.PasswordChar = '*';
            this.textPW.Size = new System.Drawing.Size(100, 21);
            this.textPW.TabIndex = 4;
            // 
            // textPWNew
            // 
            this.textPWNew.Location = new System.Drawing.Point(98, 110);
            this.textPWNew.Name = "textPWNew";
            this.textPWNew.PasswordChar = '*';
            this.textPWNew.Size = new System.Drawing.Size(100, 21);
            this.textPWNew.TabIndex = 6;
            // 
            // textPWNewAgain
            // 
            this.textPWNewAgain.Location = new System.Drawing.Point(98, 154);
            this.textPWNewAgain.Name = "textPWNewAgain";
            this.textPWNewAgain.PasswordChar = '*';
            this.textPWNewAgain.Size = new System.Drawing.Size(100, 21);
            this.textPWNewAgain.TabIndex = 8;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(33, 202);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(60, 25);
            this.buttonOk.TabIndex = 9;
            this.buttonOk.Text = "确 定";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(150, 202);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(62, 25);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "取 消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textPWNewAgain);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textPWNew);
            this.groupBox1.Controls.Add(this.cbUser);
            this.groupBox1.Controls.Add(this.textPW);
            this.groupBox1.Location = new System.Drawing.Point(8, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(231, 193);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // FrmChangePassword
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::clUser.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(248, 239);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChangePassword";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "更改密码";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textPW;
        private System.Windows.Forms.TextBox textPWNew;
        private System.Windows.Forms.TextBox textPWNewAgain;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        public System.Windows.Forms.TextBox cbUser;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}