namespace LBSgisPolice
{
    partial class frmLayer
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
            this.tvLayer = new MapInfo.Windows.Controls.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvLayer
            // 
            this.tvLayer.CheckBoxes = true;
            this.tvLayer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvLayer.Location = new System.Drawing.Point(5, 5);
            this.tvLayer.Name = "tvLayer";
            this.tvLayer.ShowNodeToolTips = true;
            this.tvLayer.Size = new System.Drawing.Size(221, 281);
            this.tvLayer.TabIndex = 1;
            this.tvLayer.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvLayer_AfterCheck);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(226, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(73, 281);
            this.panel1.TabIndex = 2;
            // 
            // buttonCancel
            // 
            this.buttonCancel.ForeColor = System.Drawing.Color.Navy;
            this.buttonCancel.Location = new System.Drawing.Point(7, 55);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(58, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "取 消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.ForeColor = System.Drawing.Color.Navy;
            this.buttonOK.Location = new System.Drawing.Point(7, 12);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(58, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "确 定";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // frmLayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImage = global::LBSgisPolice.Properties.Resources.infobg;
            this.ClientSize = new System.Drawing.Size(304, 291);
            this.Controls.Add(this.tvLayer);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmLayer";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "图层控制";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MapInfo.Windows.Controls.TreeView tvLayer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}