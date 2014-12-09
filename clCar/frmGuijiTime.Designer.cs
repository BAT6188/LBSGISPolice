namespace clCar
{
    partial class frmGuijiTime
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
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.btncar1 = new System.Windows.Forms.Button();
            this.btncar4 = new System.Windows.Forms.Button();
            this.dateTimePicker3 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker4 = new System.Windows.Forms.DateTimePicker();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btncar2 = new System.Windows.Forms.Button();
            this.btncar3 = new System.Windows.Forms.Button();
            this.txtplyspe = new System.Windows.Forms.TextBox();
            this.splbl = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtspe = new System.Windows.Forms.TextBox();
            this.txtver = new System.Windows.Forms.TextBox();
            this.txtdate = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "警车编号：";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(83, 9);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(119, 20);
            this.comboBox1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(214, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "起始时间：";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(287, 5);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(96, 21);
            this.dateTimePicker1.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(214, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "结束时间：";
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker2.Location = new System.Drawing.Point(287, 41);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(96, 21);
            this.dateTimePicker2.TabIndex = 5;
            // 
            // btncar1
            // 
            this.btncar1.Location = new System.Drawing.Point(34, 95);
            this.btncar1.Name = "btncar1";
            this.btncar1.Size = new System.Drawing.Size(75, 21);
            this.btncar1.TabIndex = 6;
            this.btncar1.Text = "播放";
            this.btncar1.UseVisualStyleBackColor = true;
            this.btncar1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btncar4
            // 
            this.btncar4.Enabled = false;
            this.btncar4.Location = new System.Drawing.Point(358, 97);
            this.btncar4.Name = "btncar4";
            this.btncar4.Size = new System.Drawing.Size(75, 21);
            this.btncar4.TabIndex = 7;
            this.btncar4.Text = "停止";
            this.btncar4.UseVisualStyleBackColor = true;
            this.btncar4.Click += new System.EventHandler(this.btncar4_Click);
            // 
            // dateTimePicker3
            // 
            this.dateTimePicker3.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePicker3.Location = new System.Drawing.Point(389, 5);
            this.dateTimePicker3.Name = "dateTimePicker3";
            this.dateTimePicker3.ShowUpDown = true;
            this.dateTimePicker3.Size = new System.Drawing.Size(84, 21);
            this.dateTimePicker3.TabIndex = 8;
            // 
            // dateTimePicker4
            // 
            this.dateTimePicker4.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePicker4.Location = new System.Drawing.Point(389, 41);
            this.dateTimePicker4.Name = "dateTimePicker4";
            this.dateTimePicker4.ShowUpDown = true;
            this.dateTimePicker4.Size = new System.Drawing.Size(84, 21);
            this.dateTimePicker4.TabIndex = 9;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btncar2
            // 
            this.btncar2.Enabled = false;
            this.btncar2.Location = new System.Drawing.Point(139, 95);
            this.btncar2.Name = "btncar2";
            this.btncar2.Size = new System.Drawing.Size(75, 23);
            this.btncar2.TabIndex = 10;
            this.btncar2.Text = "暂停";
            this.btncar2.UseVisualStyleBackColor = true;
            this.btncar2.Click += new System.EventHandler(this.btncar2_Click);
            // 
            // btncar3
            // 
            this.btncar3.Enabled = false;
            this.btncar3.Location = new System.Drawing.Point(249, 95);
            this.btncar3.Name = "btncar3";
            this.btncar3.Size = new System.Drawing.Size(75, 23);
            this.btncar3.TabIndex = 11;
            this.btncar3.Text = "继续";
            this.btncar3.UseVisualStyleBackColor = true;
            this.btncar3.Click += new System.EventHandler(this.btncar3_Click);
            // 
            // txtplyspe
            // 
            this.txtplyspe.Location = new System.Drawing.Point(83, 42);
            this.txtplyspe.Name = "txtplyspe";
            this.txtplyspe.Size = new System.Drawing.Size(47, 21);
            this.txtplyspe.TabIndex = 12;
            this.txtplyspe.Text = "1";
            // 
            // splbl
            // 
            this.splbl.AutoSize = true;
            this.splbl.Location = new System.Drawing.Point(12, 50);
            this.splbl.Name = "splbl";
            this.splbl.Size = new System.Drawing.Size(53, 12);
            this.splbl.TabIndex = 13;
            this.splbl.Text = "更新频率";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(149, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "单位:秒";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 15;
            this.label5.Text = "时速";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(149, 168);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 16;
            this.label6.Text = "方向";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(306, 168);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 17;
            this.label7.Text = "时间";
            // 
            // txtspe
            // 
            this.txtspe.Location = new System.Drawing.Point(38, 165);
            this.txtspe.Name = "txtspe";
            this.txtspe.Size = new System.Drawing.Size(71, 21);
            this.txtspe.TabIndex = 18;
            // 
            // txtver
            // 
            this.txtver.Location = new System.Drawing.Point(191, 165);
            this.txtver.Name = "txtver";
            this.txtver.Size = new System.Drawing.Size(80, 21);
            this.txtver.TabIndex = 19;
            // 
            // txtdate
            // 
            this.txtdate.Location = new System.Drawing.Point(341, 165);
            this.txtdate.Name = "txtdate";
            this.txtdate.Size = new System.Drawing.Size(121, 21);
            this.txtdate.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 197);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 12);
            this.label8.TabIndex = 21;
            // 
            // frmGuijiTime
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 216);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtdate);
            this.Controls.Add(this.txtver);
            this.Controls.Add(this.txtspe);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.splbl);
            this.Controls.Add(this.txtplyspe);
            this.Controls.Add(this.btncar3);
            this.Controls.Add(this.btncar2);
            this.Controls.Add(this.dateTimePicker4);
            this.Controls.Add(this.dateTimePicker3);
            this.Controls.Add(this.btncar4);
            this.Controls.Add(this.btncar1);
            this.Controls.Add(this.dateTimePicker2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmGuijiTime";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置轨迹时间段";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmGuijiTime_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmGuijiTime_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Button btncar1;
        private System.Windows.Forms.Button btncar4;
        private System.Windows.Forms.DateTimePicker dateTimePicker3;
        private System.Windows.Forms.DateTimePicker dateTimePicker4;
        public System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btncar2;
        private System.Windows.Forms.Button btncar3;
        private System.Windows.Forms.TextBox txtplyspe;
        private System.Windows.Forms.Label splbl;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtspe;
        private System.Windows.Forms.TextBox txtver;
        private System.Windows.Forms.TextBox txtdate;
        public  System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label8;
    }
}