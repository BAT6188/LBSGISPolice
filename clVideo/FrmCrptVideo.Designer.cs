namespace clVideo
{
    partial class FrmCrptVideo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCrptVideo));
            this.crystalReportViewer1 = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.CrptVideo1 = new clVideo.CrptVideo();
            this.crystalReport11 = new clVideo.CrystalReport1();
            this.cachedCrptNet1 = new clVideo.CachedCrptNet();
            this.crystalReport21 = new clVideo.CrystalReport2();
            this.SuspendLayout();
            // 
            // crystalReportViewer1
            // 
            this.crystalReportViewer1.ActiveViewIndex = -1;
            this.crystalReportViewer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crystalReportViewer1.Location = new System.Drawing.Point(0, 0);
            this.crystalReportViewer1.Name = "crystalReportViewer1";
            this.crystalReportViewer1.SelectionFormula = "";
            this.crystalReportViewer1.ShowExportButton = false;
            this.crystalReportViewer1.Size = new System.Drawing.Size(876, 421);
            this.crystalReportViewer1.TabIndex = 0;
            this.crystalReportViewer1.ViewTimeSelectionFormula = "";
            // 
            // FrmCrptVideo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(876, 421);
            this.Controls.Add(this.crystalReportViewer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmCrptVideo";
            this.Text = "报表";
            this.ResumeLayout(false);

        }

        #endregion

        public CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer1;
        public CrptVideo CrptVideo1;
        public CrystalReport1 crystalReport11;
        public CachedCrptNet cachedCrptNet1;
        public CrystalReport2 crystalReport21;
    }
}