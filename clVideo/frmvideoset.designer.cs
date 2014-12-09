namespace clVideo
{
    partial class frmvideoset
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.txtip = new System.Windows.Forms.TextBox();
            this.txtport = new System.Windows.Forms.TextBox();
            this.txtfolder = new System.Windows.Forms.TextBox();
            this.txtusername = new System.Windows.Forms.TextBox();
            this.txtpswd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtClient = new System.Windows.Forms.TextBox();
            this.btnClient = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(53, 224);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "保存";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(166, 224);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(76, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtip
            // 
            this.txtip.Location = new System.Drawing.Point(105, 11);
            this.txtip.Name = "txtip";
            this.txtip.Size = new System.Drawing.Size(100, 21);
            this.txtip.TabIndex = 3;
            this.txtip.Text = "173.13.2.10";
            // 
            // txtport
            // 
            this.txtport.Location = new System.Drawing.Point(105, 43);
            this.txtport.Name = "txtport";
            this.txtport.Size = new System.Drawing.Size(100, 21);
            this.txtport.TabIndex = 4;
            this.txtport.Text = "8081";
            // 
            // txtfolder
            // 
            this.txtfolder.Location = new System.Drawing.Point(105, 74);
            this.txtfolder.Name = "txtfolder";
            this.txtfolder.Size = new System.Drawing.Size(100, 21);
            this.txtfolder.TabIndex = 5;
            this.txtfolder.Text = "viss";
            // 
            // txtusername
            // 
            this.txtusername.Location = new System.Drawing.Point(105, 104);
            this.txtusername.Name = "txtusername";
            this.txtusername.Size = new System.Drawing.Size(100, 21);
            this.txtusername.TabIndex = 6;
            // 
            // txtpswd
            // 
            this.txtpswd.Location = new System.Drawing.Point(105, 129);
            this.txtpswd.Name = "txtpswd";
            this.txtpswd.PasswordChar = '*';
            this.txtpswd.Size = new System.Drawing.Size(100, 21);
            this.txtpswd.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "端口";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "文件夹";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "用户名";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "密码";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 173);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "视频客户端";
            // 
            // txtClient
            // 
            this.txtClient.Location = new System.Drawing.Point(105, 163);
            this.txtClient.Name = "txtClient";
            this.txtClient.Size = new System.Drawing.Size(138, 21);
            this.txtClient.TabIndex = 14;
            // 
            // btnClient
            // 
            this.btnClient.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnClient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClient.Location = new System.Drawing.Point(237, 163);
            this.btnClient.Name = "btnClient";
            this.btnClient.Size = new System.Drawing.Size(39, 23);
            this.btnClient.TabIndex = 15;
            this.btnClient.Text = "...";
            this.btnClient.UseVisualStyleBackColor = false;
            this.btnClient.Click += new System.EventHandler(this.btnClient_Click);
            // 
            // frmvideoset
            // 
            this.ClientSize = new System.Drawing.Size(288, 259);
            this.Controls.Add(this.btnClient);
            this.Controls.Add(this.txtClient);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.txtfolder);
            this.Controls.Add(this.txtip);
            this.Controls.Add(this.txtport);
            this.Controls.Add(this.txtpswd);
            this.Controls.Add(this.txtusername);
            this.Name = "frmvideoset";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.TextBox txtip;
        public System.Windows.Forms.TextBox txtport;
        public System.Windows.Forms.TextBox txtfolder;
        public System.Windows.Forms.TextBox txtusername;
        public System.Windows.Forms.TextBox txtpswd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox txtClient;
        private System.Windows.Forms.Button btnClient;
    }
}