namespace LBSgisPoliceEdit
{
    partial class frmNoLocation
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
            this.dataEditFrontMonth = new System.Windows.Forms.DataGridView();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.butRight = new System.Windows.Forms.Button();
            this.radClaim = new System.Windows.Forms.RadioButton();
            this.radAssigned = new System.Windows.Forms.RadioButton();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.butLeft = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataEditFrontMonth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataEditFrontMonth
            // 
            this.dataEditFrontMonth.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataEditFrontMonth.Location = new System.Drawing.Point(0, 0);
            this.dataEditFrontMonth.Name = "dataEditFrontMonth";
            this.dataEditFrontMonth.RowTemplate.Height = 23;
            this.dataEditFrontMonth.Size = new System.Drawing.Size(341, 457);
            this.dataEditFrontMonth.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(452, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(397, 457);
            this.dataGridView1.TabIndex = 1;
            // 
            // butRight
            // 
            this.butRight.Location = new System.Drawing.Point(371, 109);
            this.butRight.Name = "butRight";
            this.butRight.Size = new System.Drawing.Size(47, 23);
            this.butRight.TabIndex = 2;
            this.butRight.Text = ">>";
            this.butRight.UseVisualStyleBackColor = true;
            // 
            // radClaim
            // 
            this.radClaim.AutoSize = true;
            this.radClaim.Location = new System.Drawing.Point(371, 23);
            this.radClaim.Name = "radClaim";
            this.radClaim.Size = new System.Drawing.Size(47, 16);
            this.radClaim.TabIndex = 3;
            this.radClaim.TabStop = true;
            this.radClaim.Text = "认领";
            this.radClaim.UseVisualStyleBackColor = true;
            // 
            // radAssigned
            // 
            this.radAssigned.AutoSize = true;
            this.radAssigned.Location = new System.Drawing.Point(371, 54);
            this.radAssigned.Name = "radAssigned";
            this.radAssigned.Size = new System.Drawing.Size(47, 16);
            this.radAssigned.TabIndex = 4;
            this.radAssigned.TabStop = true;
            this.radAssigned.Text = "指派";
            this.radAssigned.UseVisualStyleBackColor = true;
            // 
            // btnRight
            // 
            this.btnRight.Location = new System.Drawing.Point(371, 165);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(47, 23);
            this.btnRight.TabIndex = 5;
            this.btnRight.Text = ">";
            this.btnRight.UseVisualStyleBackColor = true;
            // 
            // btnLeft
            // 
            this.btnLeft.Location = new System.Drawing.Point(371, 217);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(47, 23);
            this.btnLeft.TabIndex = 6;
            this.btnLeft.Text = "<";
            this.btnLeft.UseVisualStyleBackColor = true;
            // 
            // butLeft
            // 
            this.butLeft.Location = new System.Drawing.Point(371, 270);
            this.butLeft.Name = "butLeft";
            this.butLeft.Size = new System.Drawing.Size(47, 23);
            this.butLeft.TabIndex = 7;
            this.butLeft.Text = "<<";
            this.butLeft.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(360, 390);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // frmNoLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 457);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.butLeft);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.btnRight);
            this.Controls.Add(this.radAssigned);
            this.Controls.Add(this.radClaim);
            this.Controls.Add(this.butRight);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.dataEditFrontMonth);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "frmNoLocation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "无定位数据处理";
            ((System.ComponentModel.ISupportInitialize)(this.dataEditFrontMonth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.DataGridView dataEditFrontMonth;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button butRight;
        private System.Windows.Forms.RadioButton radClaim;
        private System.Windows.Forms.RadioButton radAssigned;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button butLeft;
        private System.Windows.Forms.Button button1;

    }
}