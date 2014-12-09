namespace cl110
{
    partial class uc110
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(uc110));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvip = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvNoip = new System.Windows.Forms.DataGridView();
            this.timeflash = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolVip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolNoVip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBusVip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBusNoVip = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvip)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNoip)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(293, 22);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "110接处警";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabControl1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 22);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(293, 525);
            this.panel2.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.ContextMenuStrip = this.contextMenuStrip2;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(293, 525);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvip);
            this.tabPage1.Location = new System.Drawing.Point(4, 21);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(285, 500);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "本机警情";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvip
            // 
            this.dgvip.AllowUserToAddRows = false;
            this.dgvip.AllowUserToDeleteRows = false;
            this.dgvip.AllowUserToResizeRows = false;
            this.dgvip.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dgvip.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvip.ContextMenuStrip = this.contextMenuStrip1;
            this.dgvip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvip.GridColor = System.Drawing.SystemColors.Info;
            this.dgvip.Location = new System.Drawing.Point(3, 3);
            this.dgvip.MultiSelect = false;
            this.dgvip.Name = "dgvip";
            this.dgvip.ReadOnly = true;
            this.dgvip.RowHeadersVisible = false;
            this.dgvip.RowTemplate.Height = 23;
            this.dgvip.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvip.Size = new System.Drawing.Size(279, 494);
            this.dgvip.TabIndex = 5;
            this.dgvip.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvip_CellDoubleClick);
            this.dgvip.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvip_CellClick);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvNoip);
            this.tabPage2.Location = new System.Drawing.Point(4, 21);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(285, 500);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "非本机警情";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvNoip
            // 
            this.dgvNoip.AllowUserToAddRows = false;
            this.dgvNoip.AllowUserToDeleteRows = false;
            this.dgvNoip.AllowUserToResizeRows = false;
            this.dgvNoip.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dgvNoip.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvNoip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNoip.GridColor = System.Drawing.SystemColors.Info;
            this.dgvNoip.Location = new System.Drawing.Point(3, 3);
            this.dgvNoip.MultiSelect = false;
            this.dgvNoip.Name = "dgvNoip";
            this.dgvNoip.ReadOnly = true;
            this.dgvNoip.RowHeadersVisible = false;
            this.dgvNoip.RowTemplate.Height = 23;
            this.dgvNoip.Size = new System.Drawing.Size(279, 494);
            this.dgvNoip.TabIndex = 0;
            this.dgvNoip.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvNoip_CellDoubleClick);
            this.dgvNoip.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvNoip_CellClick);
            // 
            // timeflash
            // 
            this.timeflash.Interval = 1000;
            this.timeflash.Tick += new System.EventHandler(this.timeflash_Tick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolVip,
            this.toolBusVip});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(119, 48);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolNoVip,
            this.toolBusNoVip});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(153, 70);
            // 
            // ToolVip
            // 
            this.ToolVip.Name = "ToolVip";
            this.ToolVip.Size = new System.Drawing.Size(118, 22);
            this.ToolVip.Text = "校正数据";
            // 
            // toolNoVip
            // 
            this.toolNoVip.Name = "toolNoVip";
            this.toolNoVip.Size = new System.Drawing.Size(152, 22);
            this.toolNoVip.Text = "校正数据";
            // 
            // toolBusVip
            // 
            this.toolBusVip.Name = "toolBusVip";
            this.toolBusVip.Size = new System.Drawing.Size(118, 22);
            this.toolBusVip.Text = "关联业务";
            // 
            // toolBusNoVip
            // 
            this.toolBusNoVip.Name = "toolBusNoVip";
            this.toolBusNoVip.Size = new System.Drawing.Size(152, 22);
            this.toolBusNoVip.Text = "关联业务";
            // 
            // uc110
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "uc110";
            this.Size = new System.Drawing.Size(293, 547);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvip)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvNoip)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgvNoip;
        public System.Windows.Forms.DataGridView dgvip;
        private System.Windows.Forms.Timer timeflash;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ToolVip;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolNoVip;
        private System.Windows.Forms.ToolStripMenuItem toolBusNoVip;
        private System.Windows.Forms.ToolStripMenuItem toolBusVip;

    }
}
