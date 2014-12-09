namespace clUser
{
    partial class FrmManager
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.buttonSave = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbRole = new System.Windows.Forms.CheckedListBox();
            this.buttonQXGL = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tbZhiWu = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRealName = new System.Windows.Forms.Button();
            this.cbPhone = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cbDanwei = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.cbPassword1 = new System.Windows.Forms.TextBox();
            this.cbUser = new System.Windows.Forms.TextBox();
            this.cbPassword2 = new System.Windows.Forms.TextBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.btnInData = new System.Windows.Forms.Button();
            this.btnOutData = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSave
            // 
            this.buttonSave.ForeColor = System.Drawing.Color.Navy;
            this.buttonSave.Location = new System.Drawing.Point(365, 356);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(74, 25);
            this.buttonSave.TabIndex = 12;
            this.buttonSave.Text = "更 改";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbRole);
            this.groupBox1.ForeColor = System.Drawing.Color.Navy;
            this.groupBox1.Location = new System.Drawing.Point(580, 15);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(187, 325);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "角色信息";
            // 
            // cbRole
            // 
            this.cbRole.CheckOnClick = true;
            this.cbRole.FormattingEnabled = true;
            this.cbRole.HorizontalScrollbar = true;
            this.cbRole.Items.AddRange(new object[] {
            "管理员",
            "区公安",
            "派出所",
            "角色1",
            "角色2"});
            this.cbRole.Location = new System.Drawing.Point(7, 37);
            this.cbRole.Name = "cbRole";
            this.cbRole.Size = new System.Drawing.Size(174, 276);
            this.cbRole.TabIndex = 11;
            // 
            // buttonQXGL
            // 
            this.buttonQXGL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.buttonQXGL.Location = new System.Drawing.Point(253, 28);
            this.buttonQXGL.Name = "buttonQXGL";
            this.buttonQXGL.Size = new System.Drawing.Size(69, 21);
            this.buttonQXGL.TabIndex = 4;
            this.buttonQXGL.Text = "查 询";
            this.buttonQXGL.UseVisualStyleBackColor = true;
            this.buttonQXGL.Click += new System.EventHandler(this.buttonQXGL_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView1.Location = new System.Drawing.Point(7, 20);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            this.dataGridView1.RowHeadersWidth = 25;
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(197, 347);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            // 
            // buttonDelete
            // 
            this.buttonDelete.ForeColor = System.Drawing.Color.Red;
            this.buttonDelete.Location = new System.Drawing.Point(480, 356);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(71, 25);
            this.buttonDelete.TabIndex = 13;
            this.buttonDelete.Text = "删 除";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.dataGridView1);
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(12, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(212, 374);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "用户列表";
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Transparent;
            this.groupBox3.Controls.Add(this.tbZhiWu);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.btnRealName);
            this.groupBox3.Controls.Add(this.cbPhone);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.buttonQXGL);
            this.groupBox3.Controls.Add(this.cbDanwei);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.cbName);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.checkBox2);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.cbPassword1);
            this.groupBox3.Controls.Add(this.cbUser);
            this.groupBox3.Controls.Add(this.cbPassword2);
            this.groupBox3.ForeColor = System.Drawing.Color.Navy;
            this.groupBox3.Location = new System.Drawing.Point(230, 15);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(342, 325);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "基本信息";
            // 
            // tbZhiWu
            // 
            this.tbZhiWu.Location = new System.Drawing.Point(91, 287);
            this.tbZhiWu.Name = "tbZhiWu";
            this.tbZhiWu.Size = new System.Drawing.Size(133, 21);
            this.tbZhiWu.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(19, 290);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 21;
            this.label1.Text = "职　　务：";
            // 
            // btnRealName
            // 
            this.btnRealName.Location = new System.Drawing.Point(253, 179);
            this.btnRealName.Name = "btnRealName";
            this.btnRealName.Size = new System.Drawing.Size(68, 21);
            this.btnRealName.TabIndex = 20;
            this.btnRealName.Text = "查 询";
            this.btnRealName.UseVisualStyleBackColor = true;
            this.btnRealName.Click += new System.EventHandler(this.btnRealName_Click);
            // 
            // cbPhone
            // 
            this.cbPhone.Location = new System.Drawing.Point(91, 253);
            this.cbPhone.Name = "cbPhone";
            this.cbPhone.Size = new System.Drawing.Size(133, 21);
            this.cbPhone.TabIndex = 19;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ForeColor = System.Drawing.Color.White;
            this.label13.Location = new System.Drawing.Point(18, 256);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(65, 12);
            this.label13.TabIndex = 18;
            this.label13.Text = "联系电话：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.ForeColor = System.Drawing.Color.Red;
            this.label9.Location = new System.Drawing.Point(232, 143);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 14);
            this.label9.TabIndex = 17;
            this.label9.Text = "*";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(232, 105);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 14);
            this.label8.TabIndex = 16;
            this.label8.Text = "*";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.ForeColor = System.Drawing.Color.Red;
            this.label7.Location = new System.Drawing.Point(232, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(14, 14);
            this.label7.TabIndex = 15;
            this.label7.Text = "*";
            // 
            // cbDanwei
            // 
            this.cbDanwei.Location = new System.Drawing.Point(91, 215);
            this.cbDanwei.Name = "cbDanwei";
            this.cbDanwei.Size = new System.Drawing.Size(133, 21);
            this.cbDanwei.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(18, 218);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "用户单位：";
            // 
            // cbName
            // 
            this.cbName.Location = new System.Drawing.Point(91, 178);
            this.cbName.Name = "cbName";
            this.cbName.Size = new System.Drawing.Size(133, 21);
            this.cbName.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(18, 183);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "真实姓名：";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.ForeColor = System.Drawing.Color.White;
            this.checkBox2.Location = new System.Drawing.Point(17, 73);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(156, 16);
            this.checkBox2.TabIndex = 3;
            this.checkBox2.Text = "使用默认密码：12345678";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(19, 29);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 1;
            this.label10.Text = "用 户 名：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(18, 144);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 6;
            this.label11.Text = "密码确认：";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ForeColor = System.Drawing.Color.White;
            this.label12.Location = new System.Drawing.Point(18, 106);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 12);
            this.label12.TabIndex = 4;
            this.label12.Text = "密    码：";
            // 
            // cbPassword1
            // 
            this.cbPassword1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbPassword1.Location = new System.Drawing.Point(91, 102);
            this.cbPassword1.Name = "cbPassword1";
            this.cbPassword1.PasswordChar = '*';
            this.cbPassword1.Size = new System.Drawing.Size(133, 21);
            this.cbPassword1.TabIndex = 5;
            // 
            // cbUser
            // 
            this.cbUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbUser.Location = new System.Drawing.Point(92, 26);
            this.cbUser.Name = "cbUser";
            this.cbUser.Size = new System.Drawing.Size(132, 21);
            this.cbUser.TabIndex = 2;
            this.cbUser.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbUser_KeyPress);
            // 
            // cbPassword2
            // 
            this.cbPassword2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbPassword2.Location = new System.Drawing.Point(91, 140);
            this.cbPassword2.Name = "cbPassword2";
            this.cbPassword2.PasswordChar = '*';
            this.cbPassword2.Size = new System.Drawing.Size(133, 21);
            this.cbPassword2.TabIndex = 7;
            // 
            // buttonOk
            // 
            this.buttonOk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.buttonOk.Location = new System.Drawing.Point(251, 356);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(70, 25);
            this.buttonOk.TabIndex = 16;
            this.buttonOk.Text = "添 加";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // btnInData
            // 
            this.btnInData.Location = new System.Drawing.Point(580, 357);
            this.btnInData.Name = "btnInData";
            this.btnInData.Size = new System.Drawing.Size(75, 23);
            this.btnInData.TabIndex = 17;
            this.btnInData.Text = "导入用户";
            this.btnInData.UseVisualStyleBackColor = true;
            this.btnInData.Click += new System.EventHandler(this.btnInData_Click);
            // 
            // btnOutData
            // 
            this.btnOutData.Location = new System.Drawing.Point(686, 357);
            this.btnOutData.Name = "btnOutData";
            this.btnOutData.Size = new System.Drawing.Size(75, 23);
            this.btnOutData.TabIndex = 18;
            this.btnOutData.Text = "导出用户";
            this.btnOutData.UseVisualStyleBackColor = true;
            this.btnOutData.Click += new System.EventHandler(this.btnOutData_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 12;
            this.label2.Text = "角色：";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(58, 18);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(48, 16);
            this.checkBox1.TabIndex = 13;
            this.checkBox1.Text = "全选";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // FrmManager
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::clUser.Properties.Resources.bg;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(775, 404);
            this.Controls.Add(this.btnOutData);
            this.Controls.Add(this.btnInData);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmManager";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "用户管理";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonQXGL;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckedListBox cbRole;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox cbDanwei;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox cbName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox cbPassword1;
        private System.Windows.Forms.TextBox cbUser;
        private System.Windows.Forms.TextBox cbPassword2;
        private System.Windows.Forms.TextBox cbPhone;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnRealName;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TextBox tbZhiWu;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnInData;
        private System.Windows.Forms.Button btnOutData;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label2;

    }
}