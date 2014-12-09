namespace winLoc
{
    partial class frmList
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
            this.lbxMulti = new System.Windows.Forms.ListBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbxMulti
            // 
            this.lbxMulti.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbxMulti.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbxMulti.FormattingEnabled = true;
            this.lbxMulti.ItemHeight = 16;
            this.lbxMulti.Location = new System.Drawing.Point(3, 3);
            this.lbxMulti.Name = "lbxMulti";
            this.lbxMulti.Size = new System.Drawing.Size(299, 228);
            this.lbxMulti.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(42, 239);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(174, 239);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // frmList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 268);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.lbxMulti);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmList";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.ShowInTaskbar = false;
            this.Text = "请选择匹配项";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox lbxMulti;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;

    }
}