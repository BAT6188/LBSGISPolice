namespace clZhihui
{
    partial class frmCustomYuAn
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
            this.textYuanName = new System.Windows.Forms.TextBox();
            this.textCarDis = new System.Windows.Forms.TextBox();
            this.textVideoDis = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkCar = new System.Windows.Forms.CheckBox();
            this.checkVideo = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textFire = new System.Windows.Forms.TextBox();
            this.checkFire = new System.Windows.Forms.CheckBox();
            this.checkXF = new System.Windows.Forms.CheckBox();
            this.textXF = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textMJZD = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.checkMJZD = new System.Windows.Forms.CheckBox();
            this.checkPCS = new System.Windows.Forms.CheckBox();
            this.textPCS = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textJWS = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.checkJWS = new System.Windows.Forms.CheckBox();
            this.checkAF = new System.Windows.Forms.CheckBox();
            this.textAF = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textTZHY = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.checkTZHY = new System.Windows.Forms.CheckBox();
            this.checkJY = new System.Windows.Forms.CheckBox();
            this.textJY = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textZAKK = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkZAKK = new System.Windows.Forms.CheckBox();
            this.checkGGCS = new System.Windows.Forms.CheckBox();
            this.textGGCS = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textWB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkWB = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "预案名称：";
            // 
            // textYuanName
            // 
            this.textYuanName.Location = new System.Drawing.Point(87, 13);
            this.textYuanName.Name = "textYuanName";
            this.textYuanName.Size = new System.Drawing.Size(313, 21);
            this.textYuanName.TabIndex = 1;
            // 
            // textCarDis
            // 
            this.textCarDis.Enabled = false;
            this.textCarDis.Location = new System.Drawing.Point(126, 62);
            this.textCarDis.Name = "textCarDis";
            this.textCarDis.Size = new System.Drawing.Size(98, 21);
            this.textCarDis.TabIndex = 3;
            this.textCarDis.Tag = "0";
            this.textCarDis.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // textVideoDis
            // 
            this.textVideoDis.Enabled = false;
            this.textVideoDis.Location = new System.Drawing.Point(126, 96);
            this.textVideoDis.Name = "textVideoDis";
            this.textVideoDis.Size = new System.Drawing.Size(98, 21);
            this.textVideoDis.TabIndex = 5;
            this.textVideoDis.Tag = "1";
            this.textVideoDis.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(230, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "米";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(230, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "米";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(195, 313);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 6;
            this.buttonAdd.Text = "添加";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(316, 313);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // checkCar
            // 
            this.checkCar.AutoSize = true;
            this.checkCar.Location = new System.Drawing.Point(18, 64);
            this.checkCar.Name = "checkCar";
            this.checkCar.Size = new System.Drawing.Size(72, 16);
            this.checkCar.TabIndex = 2;
            this.checkCar.Tag = "0";
            this.checkCar.Text = "警车范围";
            this.checkCar.UseVisualStyleBackColor = true;
            this.checkCar.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // checkVideo
            // 
            this.checkVideo.AutoSize = true;
            this.checkVideo.Location = new System.Drawing.Point(18, 98);
            this.checkVideo.Name = "checkVideo";
            this.checkVideo.Size = new System.Drawing.Size(84, 16);
            this.checkVideo.TabIndex = 4;
            this.checkVideo.Tag = "1";
            this.checkVideo.Text = "摄像头范围";
            this.checkVideo.UseVisualStyleBackColor = true;
            this.checkVideo.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.textFire);
            this.groupBox1.Controls.Add(this.checkFire);
            this.groupBox1.Controls.Add(this.checkXF);
            this.groupBox1.Controls.Add(this.textXF);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.textMJZD);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.checkMJZD);
            this.groupBox1.Controls.Add(this.checkPCS);
            this.groupBox1.Controls.Add(this.textPCS);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.textJWS);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.checkJWS);
            this.groupBox1.Controls.Add(this.checkAF);
            this.groupBox1.Controls.Add(this.textAF);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.textTZHY);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.checkTZHY);
            this.groupBox1.Controls.Add(this.checkJY);
            this.groupBox1.Controls.Add(this.textJY);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textZAKK);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.checkZAKK);
            this.groupBox1.Controls.Add(this.checkGGCS);
            this.groupBox1.Controls.Add(this.textGGCS);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textWB);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.checkWB);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkCar);
            this.groupBox1.Controls.Add(this.textYuanName);
            this.groupBox1.Controls.Add(this.textCarDis);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textVideoDis);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.checkVideo);
            this.groupBox1.Location = new System.Drawing.Point(12, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(569, 297);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(230, 272);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 45;
            this.label12.Text = "米";
            // 
            // textFire
            // 
            this.textFire.Enabled = false;
            this.textFire.Location = new System.Drawing.Point(126, 270);
            this.textFire.Name = "textFire";
            this.textFire.Size = new System.Drawing.Size(98, 21);
            this.textFire.TabIndex = 44;
            this.textFire.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // checkFire
            // 
            this.checkFire.AutoSize = true;
            this.checkFire.Location = new System.Drawing.Point(18, 272);
            this.checkFire.Name = "checkFire";
            this.checkFire.Size = new System.Drawing.Size(84, 16);
            this.checkFire.TabIndex = 43;
            this.checkFire.Tag = "12";
            this.checkFire.Text = "消防栓范围";
            this.checkFire.UseVisualStyleBackColor = true;
            this.checkFire.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // checkXF
            // 
            this.checkXF.AutoSize = true;
            this.checkXF.Location = new System.Drawing.Point(304, 200);
            this.checkXF.Name = "checkXF";
            this.checkXF.Size = new System.Drawing.Size(120, 16);
            this.checkXF.TabIndex = 37;
            this.checkXF.Tag = "10";
            this.checkXF.Text = "消防重点单位范围";
            this.checkXF.UseVisualStyleBackColor = true;
            this.checkXF.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // textXF
            // 
            this.textXF.Enabled = false;
            this.textXF.Location = new System.Drawing.Point(424, 198);
            this.textXF.Name = "textXF";
            this.textXF.Size = new System.Drawing.Size(98, 21);
            this.textXF.TabIndex = 38;
            this.textXF.Tag = "10";
            this.textXF.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(528, 235);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(17, 12);
            this.label13.TabIndex = 42;
            this.label13.Text = "米";
            // 
            // textMJZD
            // 
            this.textMJZD.Enabled = false;
            this.textMJZD.Location = new System.Drawing.Point(424, 232);
            this.textMJZD.Name = "textMJZD";
            this.textMJZD.Size = new System.Drawing.Size(98, 21);
            this.textMJZD.TabIndex = 40;
            this.textMJZD.Tag = "11";
            this.textMJZD.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(528, 201);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(17, 12);
            this.label14.TabIndex = 41;
            this.label14.Text = "米";
            // 
            // checkMJZD
            // 
            this.checkMJZD.AutoSize = true;
            this.checkMJZD.Location = new System.Drawing.Point(304, 234);
            this.checkMJZD.Name = "checkMJZD";
            this.checkMJZD.Size = new System.Drawing.Size(120, 16);
            this.checkMJZD.TabIndex = 39;
            this.checkMJZD.Tag = "11";
            this.checkMJZD.Text = "基层民警中队范围";
            this.checkMJZD.UseVisualStyleBackColor = true;
            this.checkMJZD.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // checkPCS
            // 
            this.checkPCS.AutoSize = true;
            this.checkPCS.Location = new System.Drawing.Point(18, 203);
            this.checkPCS.Name = "checkPCS";
            this.checkPCS.Size = new System.Drawing.Size(108, 16);
            this.checkPCS.TabIndex = 31;
            this.checkPCS.Tag = "4";
            this.checkPCS.Text = "基层派出所范围";
            this.checkPCS.UseVisualStyleBackColor = true;
            this.checkPCS.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // textPCS
            // 
            this.textPCS.Enabled = false;
            this.textPCS.Location = new System.Drawing.Point(126, 201);
            this.textPCS.Name = "textPCS";
            this.textPCS.Size = new System.Drawing.Size(98, 21);
            this.textPCS.TabIndex = 32;
            this.textPCS.Tag = "4";
            this.textPCS.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(230, 238);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(17, 12);
            this.label10.TabIndex = 36;
            this.label10.Text = "米";
            // 
            // textJWS
            // 
            this.textJWS.Enabled = false;
            this.textJWS.Location = new System.Drawing.Point(126, 235);
            this.textJWS.Name = "textJWS";
            this.textJWS.Size = new System.Drawing.Size(98, 21);
            this.textJWS.TabIndex = 34;
            this.textJWS.Tag = "5";
            this.textJWS.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(230, 204);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(17, 12);
            this.label11.TabIndex = 35;
            this.label11.Text = "米";
            // 
            // checkJWS
            // 
            this.checkJWS.AutoSize = true;
            this.checkJWS.Location = new System.Drawing.Point(18, 235);
            this.checkJWS.Name = "checkJWS";
            this.checkJWS.Size = new System.Drawing.Size(108, 16);
            this.checkJWS.TabIndex = 33;
            this.checkJWS.Tag = "5";
            this.checkJWS.Text = "社区警务室范围";
            this.checkJWS.UseVisualStyleBackColor = true;
            this.checkJWS.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // checkAF
            // 
            this.checkAF.AutoSize = true;
            this.checkAF.Location = new System.Drawing.Point(304, 132);
            this.checkAF.Name = "checkAF";
            this.checkAF.Size = new System.Drawing.Size(120, 16);
            this.checkAF.TabIndex = 22;
            this.checkAF.Tag = "8";
            this.checkAF.Text = "安全防护单位范围";
            this.checkAF.UseVisualStyleBackColor = true;
            this.checkAF.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // textAF
            // 
            this.textAF.Enabled = false;
            this.textAF.Location = new System.Drawing.Point(424, 130);
            this.textAF.Name = "textAF";
            this.textAF.Size = new System.Drawing.Size(98, 21);
            this.textAF.TabIndex = 23;
            this.textAF.Tag = "8";
            this.textAF.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(528, 167);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 12);
            this.label8.TabIndex = 27;
            this.label8.Text = "米";
            // 
            // textTZHY
            // 
            this.textTZHY.Enabled = false;
            this.textTZHY.Location = new System.Drawing.Point(424, 164);
            this.textTZHY.Name = "textTZHY";
            this.textTZHY.Size = new System.Drawing.Size(98, 21);
            this.textTZHY.TabIndex = 25;
            this.textTZHY.Tag = "9";
            this.textTZHY.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(528, 133);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(17, 12);
            this.label9.TabIndex = 26;
            this.label9.Text = "米";
            // 
            // checkTZHY
            // 
            this.checkTZHY.AutoSize = true;
            this.checkTZHY.Location = new System.Drawing.Point(304, 166);
            this.checkTZHY.Name = "checkTZHY";
            this.checkTZHY.Size = new System.Drawing.Size(96, 16);
            this.checkTZHY.TabIndex = 24;
            this.checkTZHY.Tag = "9";
            this.checkTZHY.Text = "特种行业范围";
            this.checkTZHY.UseVisualStyleBackColor = true;
            this.checkTZHY.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // checkJY
            // 
            this.checkJY.AutoSize = true;
            this.checkJY.Location = new System.Drawing.Point(18, 132);
            this.checkJY.Name = "checkJY";
            this.checkJY.Size = new System.Drawing.Size(72, 16);
            this.checkJY.TabIndex = 16;
            this.checkJY.Tag = "2";
            this.checkJY.Text = "警员范围";
            this.checkJY.UseVisualStyleBackColor = true;
            this.checkJY.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // textJY
            // 
            this.textJY.Enabled = false;
            this.textJY.Location = new System.Drawing.Point(126, 130);
            this.textJY.Name = "textJY";
            this.textJY.Size = new System.Drawing.Size(98, 21);
            this.textJY.TabIndex = 17;
            this.textJY.Tag = "2";
            this.textJY.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(230, 167);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 21;
            this.label6.Text = "米";
            // 
            // textZAKK
            // 
            this.textZAKK.Enabled = false;
            this.textZAKK.Location = new System.Drawing.Point(126, 164);
            this.textZAKK.Name = "textZAKK";
            this.textZAKK.Size = new System.Drawing.Size(98, 21);
            this.textZAKK.TabIndex = 19;
            this.textZAKK.Tag = "3";
            this.textZAKK.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(230, 133);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 20;
            this.label7.Text = "米";
            // 
            // checkZAKK
            // 
            this.checkZAKK.AutoSize = true;
            this.checkZAKK.Location = new System.Drawing.Point(18, 166);
            this.checkZAKK.Name = "checkZAKK";
            this.checkZAKK.Size = new System.Drawing.Size(96, 16);
            this.checkZAKK.TabIndex = 18;
            this.checkZAKK.Tag = "3";
            this.checkZAKK.Text = "治安卡口范围";
            this.checkZAKK.UseVisualStyleBackColor = true;
            this.checkZAKK.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // checkGGCS
            // 
            this.checkGGCS.AutoSize = true;
            this.checkGGCS.Location = new System.Drawing.Point(304, 64);
            this.checkGGCS.Name = "checkGGCS";
            this.checkGGCS.Size = new System.Drawing.Size(96, 16);
            this.checkGGCS.TabIndex = 10;
            this.checkGGCS.Tag = "6";
            this.checkGGCS.Text = "公共场所范围";
            this.checkGGCS.UseVisualStyleBackColor = true;
            this.checkGGCS.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // textGGCS
            // 
            this.textGGCS.Enabled = false;
            this.textGGCS.Location = new System.Drawing.Point(424, 62);
            this.textGGCS.Name = "textGGCS";
            this.textGGCS.Size = new System.Drawing.Size(98, 21);
            this.textGGCS.TabIndex = 11;
            this.textGGCS.Tag = "6";
            this.textGGCS.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(528, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "米";
            // 
            // textWB
            // 
            this.textWB.Enabled = false;
            this.textWB.Location = new System.Drawing.Point(424, 96);
            this.textWB.Name = "textWB";
            this.textWB.Size = new System.Drawing.Size(98, 21);
            this.textWB.TabIndex = 13;
            this.textWB.Tag = "7";
            this.textWB.Leave += new System.EventHandler(this.textDis_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(528, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "米";
            // 
            // checkWB
            // 
            this.checkWB.AutoSize = true;
            this.checkWB.Location = new System.Drawing.Point(304, 98);
            this.checkWB.Name = "checkWB";
            this.checkWB.Size = new System.Drawing.Size(72, 16);
            this.checkWB.TabIndex = 12;
            this.checkWB.Tag = "7";
            this.checkWB.Text = "网吧范围";
            this.checkWB.UseVisualStyleBackColor = true;
            this.checkWB.CheckedChanged += new System.EventHandler(this.check_CheckedChanged);
            // 
            // frmCustomYuAn
            // 
            this.AcceptButton = this.buttonAdd;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(593, 345);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAdd);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmCustomYuAn";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自定义预案";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textYuanName;
        public System.Windows.Forms.TextBox textCarDis;
        public System.Windows.Forms.TextBox textVideoDis;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonCancel;
        public System.Windows.Forms.CheckBox checkCar;
        public System.Windows.Forms.CheckBox checkVideo;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.CheckBox checkPCS;
        public System.Windows.Forms.TextBox textPCS;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.TextBox textJWS;
        private System.Windows.Forms.Label label11;
        public System.Windows.Forms.CheckBox checkJWS;
        public System.Windows.Forms.CheckBox checkAF;
        public System.Windows.Forms.TextBox textAF;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.TextBox textTZHY;
        private System.Windows.Forms.Label label9;
        public System.Windows.Forms.CheckBox checkTZHY;
        public System.Windows.Forms.CheckBox checkJY;
        public System.Windows.Forms.TextBox textJY;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox textZAKK;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.CheckBox checkZAKK;
        public System.Windows.Forms.CheckBox checkGGCS;
        public System.Windows.Forms.TextBox textGGCS;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textWB;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.CheckBox checkWB;
        public System.Windows.Forms.CheckBox checkXF;
        public System.Windows.Forms.TextBox textXF;
        private System.Windows.Forms.Label label13;
        public System.Windows.Forms.TextBox textMJZD;
        private System.Windows.Forms.Label label14;
        public System.Windows.Forms.CheckBox checkMJZD;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textFire;
        private System.Windows.Forms.CheckBox checkFire;
    }
}