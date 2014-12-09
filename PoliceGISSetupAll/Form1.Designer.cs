namespace SDPoliceGISSetup
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lExit = new System.Windows.Forms.Label();
            this.lSetupGIS = new System.Windows.Forms.Label();
            this.linkSetupMapxtreme = new System.Windows.Forms.Label();
            this.lSetupOracleService = new System.Windows.Forms.Label();
            this.lSetupOracleClient = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioVideoNet = new System.Windows.Forms.RadioButton();
            this.radioPoliceNet = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.lExit);
            this.groupBox1.Controls.Add(this.lSetupGIS);
            this.groupBox1.Controls.Add(this.linkSetupMapxtreme);
            this.groupBox1.Controls.Add(this.lSetupOracleService);
            this.groupBox1.Controls.Add(this.lSetupOracleClient);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.groupBox1.Location = new System.Drawing.Point(2, 115);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(484, 228);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            // 
            // lExit
            // 
            this.lExit.AutoSize = true;
            this.lExit.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lExit.ForeColor = System.Drawing.Color.White;
            this.lExit.Location = new System.Drawing.Point(10, 193);
            this.lExit.Name = "lExit";
            this.lExit.Size = new System.Drawing.Size(60, 16);
            this.lExit.TabIndex = 18;
            this.lExit.Text = "4 退出";
            this.lExit.MouseLeave += new System.EventHandler(this.link_MouseLeave);
            this.lExit.Click += new System.EventHandler(this.lExit_Click);
            this.lExit.MouseEnter += new System.EventHandler(this.link_MouseEnter);
            this.lExit.MouseHover += new System.EventHandler(this.link_MouseHover);
            // 
            // lSetupGIS
            // 
            this.lSetupGIS.AutoSize = true;
            this.lSetupGIS.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lSetupGIS.ForeColor = System.Drawing.Color.White;
            this.lSetupGIS.Location = new System.Drawing.Point(10, 152);
            this.lSetupGIS.Name = "lSetupGIS";
            this.lSetupGIS.Size = new System.Drawing.Size(308, 16);
            this.lSetupGIS.TabIndex = 17;
            this.lSetupGIS.Text = "3 安装社会治安视频监控平台GIS客户端";
            this.lSetupGIS.MouseLeave += new System.EventHandler(this.link_MouseLeave);
            this.lSetupGIS.Click += new System.EventHandler(this.lSetupGIS_Click);
            this.lSetupGIS.MouseEnter += new System.EventHandler(this.link_MouseEnter);
            this.lSetupGIS.MouseHover += new System.EventHandler(this.link_MouseHover);
            // 
            // linkSetupMapxtreme
            // 
            this.linkSetupMapxtreme.AutoSize = true;
            this.linkSetupMapxtreme.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkSetupMapxtreme.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkSetupMapxtreme.ForeColor = System.Drawing.Color.White;
            this.linkSetupMapxtreme.Location = new System.Drawing.Point(10, 112);
            this.linkSetupMapxtreme.Name = "linkSetupMapxtreme";
            this.linkSetupMapxtreme.Size = new System.Drawing.Size(213, 16);
            this.linkSetupMapxtreme.TabIndex = 16;
            this.linkSetupMapxtreme.Text = "2 安装Mapxtreme2005 6.7";
            this.linkSetupMapxtreme.MouseLeave += new System.EventHandler(this.link_MouseLeave);
            this.linkSetupMapxtreme.Click += new System.EventHandler(this.linkSetupMapxtreme_Click);
            this.linkSetupMapxtreme.MouseEnter += new System.EventHandler(this.link_MouseEnter);
            this.linkSetupMapxtreme.MouseHover += new System.EventHandler(this.link_MouseHover);
            // 
            // lSetupOracleService
            // 
            this.lSetupOracleService.AutoSize = true;
            this.lSetupOracleService.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lSetupOracleService.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lSetupOracleService.ForeColor = System.Drawing.Color.White;
            this.lSetupOracleService.Location = new System.Drawing.Point(53, 74);
            this.lSetupOracleService.Name = "lSetupOracleService";
            this.lSetupOracleService.Size = new System.Drawing.Size(105, 14);
            this.lSetupOracleService.TabIndex = 14;
            this.lSetupOracleService.Text = "配置Oracle服务";
            this.lSetupOracleService.MouseLeave += new System.EventHandler(this.link_MouseLeave);
            this.lSetupOracleService.Click += new System.EventHandler(this.lSetupOracleService_Click);
            this.lSetupOracleService.MouseEnter += new System.EventHandler(this.link_MouseEnter);
            this.lSetupOracleService.MouseHover += new System.EventHandler(this.link_MouseHover);
            // 
            // lSetupOracleClient
            // 
            this.lSetupOracleClient.AutoSize = true;
            this.lSetupOracleClient.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lSetupOracleClient.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lSetupOracleClient.ForeColor = System.Drawing.Color.White;
            this.lSetupOracleClient.Location = new System.Drawing.Point(53, 47);
            this.lSetupOracleClient.Name = "lSetupOracleClient";
            this.lSetupOracleClient.Size = new System.Drawing.Size(147, 14);
            this.lSetupOracleClient.TabIndex = 13;
            this.lSetupOracleClient.Text = "安装Oracle10.0客户端";
            this.toolTip1.SetToolTip(this.lSetupOracleClient, "请在安装过程中选择安装＇运行时＇");
            this.lSetupOracleClient.MouseLeave += new System.EventHandler(this.link_MouseLeave);
            this.lSetupOracleClient.Click += new System.EventHandler(this.lSetupOracleClient_Click);
            this.lSetupOracleClient.MouseEnter += new System.EventHandler(this.link_MouseEnter);
            this.lSetupOracleClient.MouseHover += new System.EventHandler(this.link_MouseHover);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Cursor = System.Windows.Forms.Cursors.Default;
            this.label5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label5.Location = new System.Drawing.Point(10, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(238, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "1 安装设置Oracle10g Client";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.BackgroundImage = global::SDPoliceGISSetup.Properties.Resources.infoTitle;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(488, 80);
            this.panel1.TabIndex = 9;
            // 
            // radioVideoNet
            // 
            this.radioVideoNet.AutoSize = true;
            this.radioVideoNet.BackColor = System.Drawing.Color.Transparent;
            this.radioVideoNet.Checked = true;
            this.radioVideoNet.Cursor = System.Windows.Forms.Cursors.Default;
            this.radioVideoNet.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioVideoNet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.radioVideoNet.Location = new System.Drawing.Point(68, 89);
            this.radioVideoNet.Name = "radioVideoNet";
            this.radioVideoNet.Size = new System.Drawing.Size(74, 20);
            this.radioVideoNet.TabIndex = 11;
            this.radioVideoNet.TabStop = true;
            this.radioVideoNet.Text = "视频网";
            this.radioVideoNet.UseVisualStyleBackColor = false;
            // 
            // radioPoliceNet
            // 
            this.radioPoliceNet.AutoSize = true;
            this.radioPoliceNet.BackColor = System.Drawing.Color.Transparent;
            this.radioPoliceNet.Cursor = System.Windows.Forms.Cursors.Default;
            this.radioPoliceNet.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioPoliceNet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.radioPoliceNet.Location = new System.Drawing.Point(265, 89);
            this.radioPoliceNet.Name = "radioPoliceNet";
            this.radioPoliceNet.Size = new System.Drawing.Size(74, 20);
            this.radioPoliceNet.TabIndex = 12;
            this.radioPoliceNet.Text = "公安网";
            this.radioPoliceNet.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::SDPoliceGISSetup.Properties.Resources.infobg;
            this.ClientSize = new System.Drawing.Size(488, 352);
            this.ControlBox = false;
            this.Controls.Add(this.radioPoliceNet);
            this.Controls.Add(this.radioVideoNet);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "欢迎使用：顺德警用地理信息系统安装";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radioVideoNet;
        private System.Windows.Forms.RadioButton radioPoliceNet;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lSetupOracleClient;
        private System.Windows.Forms.Label lSetupOracleService;
        private System.Windows.Forms.Label linkSetupMapxtreme;
        private System.Windows.Forms.Label lSetupGIS;
        private System.Windows.Forms.Label lExit;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

