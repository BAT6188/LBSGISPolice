using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.Data.OracleClient;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace clZonghe
{
	/// <summary>
	/// FrmZLMessage ��ժҪ˵����
	/// </summary>
	public class FrmZLMessage : System.Windows.Forms.Form
	{
        private string DWMC;

        private  string  strConn ="";
        private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Button CButton;
        private Button AddButton;
        private Button SubtractButton;
        private Panel panel2;
		private System.ComponentModel.IContainer components;
        private System.Data.DataTable dtEdit = null;  // �洢�޸�Ȩ�޵ı�lili��

		public FrmZLMessage()
		{
			//
			// Windows ���������֧���������
			//
			InitializeComponent();

			//
			// TODO: �� InitializeComponent ���ú�����κι��캯������
			//
		}
        
        /// <summary>
        /// ���캯��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="DWMC">��λ����</param>
        /// <param name="s">�����ַ���</param>
        /// <param name="temEditDt">�༭Ȩ��</param>
		public FrmZLMessage(string  DWMC,string s,System.Data.DataTable temEditDt)
		{
            try
            {
                //
                // Windows ���������֧���������
                //
                InitializeComponent();
                this.DWMC = DWMC;
                string root = Application.StartupPath;
                dtEdit = temEditDt;

                CLC.INIClass.IniPathSet(root.Remove(root.LastIndexOf("\\")) + "\\config.ini");
                string fileIP1 = CLC.INIClass.IniReadValue("�ļ�������", "IP1");
                bool b1 = IsWebResourceAvailable(fileIP1);
                string fileIP2 = CLC.INIClass.IniReadValue("�ļ�������", "IP2");
                bool b2 = IsWebResourceAvailable(fileIP2);

                string serverIP = "http://" + fileIP1 + "/LBSCHINA";
                if (b1 == false && b2 == false)
                {
                    DialogResult dResult = MessageBox.Show("��̨�ļ�����������������,�����ܶ��ļ����в���,�����������ϵ����Ա!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    if (b1 == false)
                    {
                        serverIP = "http://" + fileIP2 + "/LBSCHINA";
                    }
                }

                ClsFileManagement.ClsFileManagement.SetConstring(root.Remove(root.LastIndexOf("\\")) + @"\config.ini", serverIP);
                strConn = s;
                //
                // TODO: �� InitializeComponent ���ú�����κι��캯������
                //
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "FrmZLMessage-01-���캯��");
            }
		}
        
        /// <summary>
        /// ���캯��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="DWMC">��λ����</param>
        /// <param name="s">�����ַ���</param>
        public FrmZLMessage(string DWMC, string s)
        {
            try
            {
                //
                // Windows ���������֧���������
                //
                InitializeComponent();
                this.DWMC = DWMC;
                string root = Application.StartupPath;

                CLC.INIClass.IniPathSet(root.Remove(root.LastIndexOf("\\")) + "\\config.ini");
                string fileIP1 = CLC.INIClass.IniReadValue("�ļ�������", "IP1");
                bool b1 = IsWebResourceAvailable(fileIP1);
                string fileIP2 = CLC.INIClass.IniReadValue("�ļ�������", "IP2");
                bool b2 = IsWebResourceAvailable(fileIP2);

                string serverIP = "http://" + fileIP1 + "/LBSCHINA";
                if (b1 == false && b2 == false)
                {
                    DialogResult dResult = MessageBox.Show("��̨�ļ�����������������,�����ܶ��ļ����в���,�����������ϵ����Ա!", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    if (b1 == false)
                    {
                        serverIP = "http://" + fileIP2 + "/LBSCHINA";
                    }
                }

                ClsFileManagement.ClsFileManagement.SetConstring(root.Remove(root.LastIndexOf("\\")) + @"\config.ini", serverIP);
                strConn = s;
                //
                // TODO: �� InitializeComponent ���ú�����κι��캯������
                //
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "FrmZLMessage-02-���캯��");
            }
        }

        /// <summary>
        /// ����ļ��������Ƿ�����ͨ
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="webResourceAddress">IP��ַ</param>
        /// <returns>����ֵ��true-��ͨ false-δ��ͨ��</returns>
        private bool IsWebResourceAvailable(string webResourceAddress)
        {
            TcpClient tcpClient = null;
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(webResourceAddress), 80);
                tcpClient = new TcpClient();
                tcpClient.Connect(ipep);

                //tcpClient.GetStream();

                return true;
                //HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(webResourceAddress);
                //req.Method = "HEAD";
                //req.Timeout = 15000;
                //HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                //return (res.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "IsWebResourceAvailable");
                return false;
            }
            finally
            {
                tcpClient.Close();
            }
        }

		/// <summary>
        /// ������������ʹ�õ���Դ��
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows ������������ɵĴ���
		/// <summary>
		/// �����֧������ķ��� - ��Ҫʹ�ô���༭���޸Ĵ˷��������ݡ�
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmZLMessage));
            this.panel1 = new System.Windows.Forms.Panel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.CButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.SubtractButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listView1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(326, 265);
            this.panel1.TabIndex = 0;
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(326, 265);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            this.imageList1.Images.SetKeyName(3, "");
            this.imageList1.Images.SetKeyName(4, "");
            this.imageList1.Images.SetKeyName(5, "");
            this.imageList1.Images.SetKeyName(6, "");
            // 
            // CButton
            // 
            this.CButton.Location = new System.Drawing.Point(236, 3);
            this.CButton.Name = "CButton";
            this.CButton.Size = new System.Drawing.Size(62, 21);
            this.CButton.TabIndex = 3;
            this.CButton.Text = "�� ��";
            this.CButton.Click += new System.EventHandler(this.CButton_Click);
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(27, 3);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(63, 21);
            this.AddButton.TabIndex = 1;
            this.AddButton.Text = "�� ��";
            this.AddButton.Visible = false;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // SubtractButton
            // 
            this.SubtractButton.Location = new System.Drawing.Point(132, 3);
            this.SubtractButton.Name = "SubtractButton";
            this.SubtractButton.Size = new System.Drawing.Size(62, 21);
            this.SubtractButton.TabIndex = 2;
            this.SubtractButton.Text = "ɾ ��";
            this.SubtractButton.Visible = false;
            this.SubtractButton.Click += new System.EventHandler(this.SubtractButton_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.SubtractButton);
            this.panel2.Controls.Add(this.AddButton);
            this.panel2.Controls.Add(this.CButton);
            this.panel2.Location = new System.Drawing.Point(0, 239);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(331, 32);
            this.panel2.TabIndex = 4;
            // 
            // FrmZLMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(326, 265);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmZLMessage";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "������Ϣ";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FrmZLMessage_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

        /// <summary>
        /// �������
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
		private void FrmZLMessage_Load(object sender, System.EventArgs e)
		{
            this.createitem();
		}

        /// <summary>
        /// ���ļ�ȫ��������ʾ����
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
		private void createitem()
        {
            OracleConnection conn = new System.Data.OracleClient.OracleConnection(strConn);
			try
			{   
				ListViewItem lvi;
				ListViewItem.ListViewSubItem lvsi;
                conn.Open();
                string sql = "select �ļ����� from ��ȫ������λ�ļ�  where ��λ����='" + this.DWMC + "' and �ļ��汾=1";                                                                           
				OracleCommand cmd = new OracleCommand(sql, conn);
				OracleDataReader miReader = cmd.ExecuteReader();
				
				string fi="";
                
				listView1.Clear();//ע����������ǰ�listview�������ѡ������������ɾ��

				listView1.BeginUpdate();

				while(miReader.Read())//���ļ���Ϣ��ӵ�listview��ѡ����
				{
					fi=miReader.GetString(0).Trim();
					lvi=new ListViewItem();
					lvi.Text=fi;
					lvi.Tag=fi;
					string str=fi.ToString();
					
					int n=str.IndexOf(@".");
                    str = str.Substring(n + 1, str.Length - n - 1);

                    if(str.ToUpper() == "JPG" || str.ToUpper() == "JPEG" || str.ToUpper() == "BMP" || str.ToUpper() == "GIF")
                    {
                        lvi.ImageIndex = 0;
                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.Length.ToString();
                        lvi.SubItems.Add(lvsi);

                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.ToString();

                        lvi.SubItems.Add(lvsi);

                        this.listView1.Items.Add(lvi);
                    }

					else if(str.ToUpper()=="PPT")
					{
						lvi.ImageIndex=1;
						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.Length.ToString();
						lvi.SubItems.Add(lvsi);

						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.ToString();

						lvi.SubItems.Add(lvsi);

						this.listView1.Items.Add(lvi);
					}
					else if(str.ToUpper()=="TXT")
					{
						lvi.ImageIndex=2;
						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.Length.ToString();
						lvi.SubItems.Add(lvsi);

						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.ToString();

						lvi.SubItems.Add(lvsi);

						this.listView1.Items.Add(lvi);
					}
					else if(str.ToUpper()=="DOC")
					{
						lvi.ImageIndex=3;
						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.Length.ToString();
						lvi.SubItems.Add(lvsi);

						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.ToString();

						lvi.SubItems.Add(lvsi);

						this.listView1.Items.Add(lvi);
					}
					else if(str.ToUpper()=="XLS"||str.ToUpper()=="XLSX")
					{
						lvi.ImageIndex=4;
						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.Length.ToString();
						lvi.SubItems.Add(lvsi);

						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.ToString();

						lvi.SubItems.Add(lvsi);

						this.listView1.Items.Add(lvi);
					}
					else if(str.ToUpper()=="PDF")
					{
						lvi.ImageIndex=5;
						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.Length.ToString();
						lvi.SubItems.Add(lvsi);

						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.ToString();

						lvi.SubItems.Add(lvsi);

						this.listView1.Items.Add(lvi);
					}
					else 
					{
						lvi.ImageIndex=6;
						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.Length.ToString();
						lvi.SubItems.Add(lvsi);

						lvsi=new System.Windows.Forms.ListViewItem.ListViewSubItem();
						lvsi.Text=fi.ToString();
						
						lvi.SubItems.Add(lvsi);
                        this.listView1.Items.Add(lvi);
					}
			    }
				this.listView1.EndUpdate();
				miReader.Dispose();
				miReader.Close();
				conn.Close();
				conn.Dispose();
			}
			catch(Exception ex)
			{
                writeZongheLog(ex, "createitem");
                if (conn.State == ConnectionState.Open)
                    conn.Close();
				MessageBox.Show("���������!");
			}
		}

        /// <summary>
        /// ˫���б����д��ļ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
		private void listView1_DoubleClick(object sender, System.EventArgs e)
		{
			try
			{
				ProcessStartInfo pInfo = new ProcessStartInfo();
			
				foreach (ListViewItem listitem in this.listView1.SelectedItems)
                {
                    string root = Application.StartupPath;
                    pInfo.FileName = root.Remove(root.LastIndexOf("\\")) + @"\Data\InfoData\"+listitem.Text;

					Process p = Process.Start(pInfo);
				}
			}
			catch(Exception ex)
			{
                writeZongheLog(ex, "listView1_DoubleClick");
				MessageBox.Show("���ļ�����!");
			}
		}

        /// <summary>
        /// �رմ���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
		private void CButton_Click(object sender, System.EventArgs e)
		{
            try
            {
                this.Close();
            }
            catch(Exception ex)
            {
                writeZongheLog(ex, "CButton_Click");
            }
		}

        /// <summary>
        /// ������ļ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
		private void AddButton_Click(object sender, System.EventArgs e)
		{
            if (dtEdit.Rows[0]["ҵ�����ݿɱ༭"].ToString().Trim() == "0")
            {
                MessageBox.Show("��û�����Ȩ�ޣ�","��ʾ");
                return;
            }
            OracleConnection conn = new System.Data.OracleClient.OracleConnection(strConn);
			bool sameNameBool=false;
            string root = Application.StartupPath;
            root = root.Remove(root.LastIndexOf("\\")) + @"\Data\InfoData";
			try
			{
				if(this.openFileDialog1.ShowDialog(this)== System.Windows.Forms.DialogResult.OK )
				{					
					string str=this.openFileDialog1.FileName;
				
					int n=str.LastIndexOf(@"\");
					string filename=str.Substring(n+1,str.Length-n-1);
					string path=root+"\\"+filename;
					
					System.Environment.CurrentDirectory=Application.StartupPath;

                    conn.Open();
                    string sql = "select �ļ����� from ��ȫ������λ�ļ� where �ļ��汾=1";                                                                           
					OracleCommand cmd = new OracleCommand(sql, conn);
					OracleDataReader miReader = cmd.ExecuteReader();
					while(miReader.Read())
					{
						if(miReader.GetString(0).Trim()==filename)
						{
                         sameNameBool=true;
						}
						if(sameNameBool==true)
						{
							MessageBox.Show("ϵͳ���Ѵ���һ����ͬ�ļ������ļ���������ڵ��ļ�������������ӣ�");
							return;
						}
					}
                    if (ClsFileManagement.ClsFileManagement.FileExist(filename))
					{
						Cursor.Current=Cursors.WaitCursor;
						if (MessageBox.Show("�Ƿ�����ļ�","Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)==System.Windows.Forms.DialogResult.OK)
						{			   
							System.IO.FileInfo file = new System.IO.FileInfo(path);
							string newFile=root+"\\"+str.Substring(n,str.Length-n);
                            System.IO.File.Copy(this.openFileDialog1.FileName, newFile, true);

                            ClsFileManagement.ClsFileManagement.FileUpdata(filename, root, this.DWMC);
						}
						 Cursor.Current=Cursors.Default;
					}
					else 
					{
					  Cursor.Current=Cursors.WaitCursor;
					  string newFile=root+str.Substring(n,str.Length-n);
                      System.IO.File.Copy(this.openFileDialog1.FileName, newFile, true);
                      ClsFileManagement.ClsFileManagement.NewFileAdd( root, filename,this.DWMC);
                      //file_AddnewFile(root, filename, this.DWMC);
					  Cursor.Current=Cursors.Default;
					}
                    miReader.Close();
                    cmd.Dispose();
                    conn.Close();
                    this.createitem();
				}
			}
			catch(Exception ex)
			{
                writeZongheLog(ex, "AddButton_Click");
                if (conn.State == ConnectionState.Open)
                    conn.Close();
				MessageBox.Show("�����ӵ��ļ���������");
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filename">�ļ���</param>
        /// <param name="p"></param>
        private void file_AddnewFile(string root, string filename, string p)
        {
            try
            {
                CLC.INIClass.IniWriteValue("�ļ�", filename, "1");
                System.Net.WebClient WC = new System.Net.WebClient();
                WC.UploadFile("http://"+CLC.INIClass.IniReadValue("�ļ�������", "IP1")+"/lbschina/Data/InfoData/" + filename, "PUT", root + "\\" + filename);
                string SQLString;
                SQLString = "SELECT COUNT(*) FROM ��ȫ������λ�ļ�";
                int I = CLC.DatabaseRelated.OracleDriver.OracleComScalar(SQLString);
                SQLString = "DELETE ��ȫ������λ�ļ� WHERE �ļ����� = '" + filename + "'";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(SQLString);
                SQLString = "INSERT INTO ��ȫ������λ�ļ�(ID,�ļ�����,��λ����,�ļ��汾) VALUES ( " + I + " + 1,'" + filename + "','" + DWMC + "','1')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(SQLString);
            }
            catch (Exception e)
            {
                writeZongheLog(e, "file_AddnewFile");
            }
        }

        /// <summary>
        /// ɾ���ļ�
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
		private void SubtractButton_Click(object sender, System.EventArgs e)
        {
            if (dtEdit.Rows[0]["��ҵ�����ݿ�ɾ����"].ToString().Trim() == "0")
            {
                MessageBox.Show("��û��ɾ��Ȩ�ޣ�", "��ʾ");
                return;
            }
            try
            {
                if (MessageBox.Show("ȷ��Ҫɾ����?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    string root = Application.StartupPath;
                    root = root.Remove(root.LastIndexOf("\\")) + @"\Data\InfoData\";
                    string path;
                    foreach (ListViewItem listitem in this.listView1.SelectedItems)
                    {
                        ClsFileManagement.ClsFileManagement.FileDel(listitem.Text);
                        path = root + listitem.Text;
                        System.IO.File.SetAttributes(path, System.IO.FileAttributes.Normal);
                        System.IO.FileInfo file = new System.IO.FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                    this.createitem();
                }
            }
            catch(Exception ex)
            {
                writeZongheLog(ex, "SubtractButton_Click");
            }
            Cursor.Current = Cursors.Default;
		}

        /// <summary>
        /// �쳣��־���
        /// ���༭��   ����
        /// ���༭ʱ��  2011-1-25
        /// </summary>
        /// <param name="ex">�쳣Դ</param>
        /// <param name="sFunc">������</param>
        private void writeZongheLog(Exception ex,string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clZonghe-FrmZLMessage" + sFunc);
        }
////////////////////////////////////////////////////////////////////////////////////////////////////
	}
}
