namespace clZonghe
{
    partial class frmDisplay
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
            this.dataGridDisplay = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridDisplay
            // 
            this.dataGridDisplay.AllowUserToAddRows = false;
            this.dataGridDisplay.AllowUserToDeleteRows = false;
            this.dataGridDisplay.AllowUserToOrderColumns = true;
            this.dataGridDisplay.AllowUserToResizeRows = false;
            this.dataGridDisplay.BackgroundColor = System.Drawing.Color.White;
            this.dataGridDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridDisplay.GridColor = System.Drawing.SystemColors.Window;
            this.dataGridDisplay.Location = new System.Drawing.Point(0, 0);
            this.dataGridDisplay.MultiSelect = false;
            this.dataGridDisplay.Name = "dataGridDisplay";
            this.dataGridDisplay.ReadOnly = true;
            this.dataGridDisplay.RowHeadersVisible = false;
            this.dataGridDisplay.RowTemplate.Height = 23;
            this.dataGridDisplay.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridDisplay.Size = new System.Drawing.Size(1045, 511);
            this.dataGridDisplay.TabIndex = 0;
            // 
            // frmDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 511);
            this.Controls.Add(this.dataGridDisplay);
            this.Name = "frmDisplay";
            this.Text = "数据";
            this.Load += new System.EventHandler(this.frmDisplay_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView dataGridDisplay;


    }
}