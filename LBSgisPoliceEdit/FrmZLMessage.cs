using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.OracleClient;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace LBSgisPoliceEdit
{
    public partial class FrmZLMessage : Form
    {
        public FrmZLMessage()
        {
            InitializeComponent();
        }
        private string DWMC;

        private string strConn = "";
        private System.Data.DataTable dtEdit = null;  // 存储修改权限的表（lili）

        public FrmZLMessage(string DWMC, string s, System.Data.DataTable temEditDt)
        {
            try
            {
                //
                // Windows 窗体设计器支持所必需的
                //
                InitializeComponent();
                this.DWMC = DWMC;
                string root = Application.StartupPath;
                dtEdit = temEditDt;

                CLC.INIClass.IniPathSet(root.Remove(root.LastIndexOf("\\")) + "\\config.ini");
                // CLC.INIClass.IniPathSet(root + "\\config.ini");
                string fileIP1 = CLC.INIClass.IniReadValue("文件服务器", "IP1");
                bool b1 = IsWebResourceAvailable(fileIP1);
                string fileIP2 = CLC.INIClass.IniReadValue("文件服务器", "IP2");
                bool b2 = IsWebResourceAvailable(fileIP2);

                string serverIP = "http://" + fileIP1 + "/LBSCHINA";
                if (b1 == false && b2 == false)
                {
                    DialogResult dResult = MessageBox.Show("两台文件服务器均不能连接,将不能对文件进行操作,请检测网络或联系管理员!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    if (b1 == false)
                    {
                        serverIP = "http://" + fileIP2 + "/LBSCHINA";
                    }
                }
                string sfilePath = root.Remove(root.LastIndexOf("\\")) + @"\config.ini";
                //string sfilePath = root + @"\config.ini";
                ClsFileManagement.ClsFileManagement.SetConstring(sfilePath, serverIP);
                strConn = s;
                //
                // TODO: 在 InitializeComponent 调用后添加任何构造函数代码
                //
            }
            catch (Exception ex)
            {
                writeToLog(ex, "构造函数");
            }
        }

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
                writeToLog(ex, "IsWebResourceAvailable");
                return false;
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private void createitem()
        {
            try
            {
                ListViewItem lvi;
                ListViewItem.ListViewSubItem lvsi;
                OracleConnection conn = new System.Data.OracleClient.OracleConnection(strConn);
                conn.Open();
                string sql = "select 文件名称 from 安全防护单位文件  where 单位名称='" + this.DWMC + "' and 文件版本=1";
                OracleCommand cmd = new OracleCommand(sql, conn);
                OracleDataReader miReader = cmd.ExecuteReader();

                string fi = "";

                listView1.Clear();//注意这个函数是把listview里的所有选项与所列名都删除

                listView1.BeginUpdate();

                while (miReader.Read())//把文件信息添加到listview的选项中
                {
                    fi = miReader.GetString(0).Trim();
                    lvi = new ListViewItem();
                    lvi.Text = fi;
                    lvi.Tag = fi;
                    string str = fi.ToString();

                    int n = str.IndexOf(@".");
                    str = str.Substring(n + 1, str.Length - n - 1);

                    if (str.ToUpper() == "JPG" || str.ToUpper() == "JPEG" || str.ToUpper() == "BMP" || str.ToUpper() == "GIF")
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

                    else if (str.ToUpper() == "PPT")
                    {
                        lvi.ImageIndex = 1;
                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.Length.ToString();
                        lvi.SubItems.Add(lvsi);

                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.ToString();

                        lvi.SubItems.Add(lvsi);

                        this.listView1.Items.Add(lvi);
                    }
                    else if (str.ToUpper() == "TXT")
                    {
                        lvi.ImageIndex = 2;
                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.Length.ToString();
                        lvi.SubItems.Add(lvsi);

                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.ToString();

                        lvi.SubItems.Add(lvsi);

                        this.listView1.Items.Add(lvi);
                    }
                    else if (str.ToUpper() == "DOC")
                    {
                        lvi.ImageIndex = 3;
                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.Length.ToString();
                        lvi.SubItems.Add(lvsi);

                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.ToString();

                        lvi.SubItems.Add(lvsi);

                        this.listView1.Items.Add(lvi);
                    }
                    else if (str.ToUpper() == "XLS" || str.ToUpper() == "XLSX")
                    {
                        lvi.ImageIndex = 4;
                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.Length.ToString();
                        lvi.SubItems.Add(lvsi);

                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.ToString();

                        lvi.SubItems.Add(lvsi);

                        this.listView1.Items.Add(lvi);
                    }
                    else if (str.ToUpper() == "PDF")
                    {
                        lvi.ImageIndex = 5;
                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.Length.ToString();
                        lvi.SubItems.Add(lvsi);

                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.ToString();

                        lvi.SubItems.Add(lvsi);

                        this.listView1.Items.Add(lvi);
                    }
                    else
                    {
                        lvi.ImageIndex = 6;
                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.Length.ToString();
                        lvi.SubItems.Add(lvsi);

                        lvsi = new System.Windows.Forms.ListViewItem.ListViewSubItem();
                        lvsi.Text = fi.ToString();

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
            catch (Exception ex)
            {
                writeToLog(ex, "createitem");
                MessageBox.Show("无相关详情!");
            }
        }

        private void writeToLog(Exception ex,string sFuns)
        {
            try
            {
                //string exePath = Application.StartupPath;
                //StreamWriter sw = new StreamWriter(exePath + "\\log.log", true);
                //sw.WriteLine(DateTime.Now.ToString());
                //sw.WriteLine(ex.StackTrace);
                //sw.WriteLine(ex.Message);
                //sw.Close();
                CLC.BugRelated.ExceptionWrite(ex, "LBSgisPoliceEdit-FrmZLMessage-"+sFuns);
            }
            catch { }
        }

        private void FrmZLMessage_Load(object sender, EventArgs e)
        {
            this.createitem();
        }

        // 添加按钮
        private void AddButton_Click(object sender, EventArgs e)
        {
            if (dtEdit.Rows[0]["业务数据可编辑"].ToString().Trim() == "0")
            {
                MessageBox.Show("您没有添加权限！", "提示");
                return;
            }
            bool sameNameBool = false;
            string root = Application.StartupPath;
            root = root.Remove(root.LastIndexOf("\\")) + @"\Data\InfoData";
            try
            {
                if (this.openFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    string str = this.openFileDialog1.FileName;

                    int n = str.LastIndexOf(@"\");
                    string filename = str.Substring(n + 1, str.Length - n - 1);
                    string path = root + "\\" + filename;

                    System.Environment.CurrentDirectory = Application.StartupPath;

                    OracleConnection conn = new System.Data.OracleClient.OracleConnection(strConn);
                    conn.Open();
                    string sql = "select 文件名称 from 安全防护单位文件 where 文件版本=1";
                    OracleCommand cmd = new OracleCommand(sql, conn);
                    OracleDataReader miReader = cmd.ExecuteReader();
                    while (miReader.Read())
                    {
                        if (miReader.GetString(0).Trim() == filename)
                        {
                            sameNameBool = true;
                        }
                        if (sameNameBool == true)
                        {
                            MessageBox.Show("系统中已存在一个相同文件名的文件，请对现在的文件重命名后再添加！");
                            return;
                        }
                    }
                    if (ClsFileManagement.ClsFileManagement.FileExist(filename))
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        if (MessageBox.Show("是否更新文件", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.OK)
                        {
                            System.IO.FileInfo file = new System.IO.FileInfo(path);
                            string newFile = root + "\\" + str.Substring(n, str.Length - n);
                            System.IO.File.Copy(this.openFileDialog1.FileName, newFile, true);

                            ClsFileManagement.ClsFileManagement.FileUpdata(filename, root, this.DWMC);
                        }
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        if (!Directory.Exists(root))
                        {
                            Directory.CreateDirectory(root);  
                        }
                        string newFile = root + str.Substring(n, str.Length - n);
                        System.IO.File.Copy(this.openFileDialog1.FileName, newFile, true);
                        ClsFileManagement.ClsFileManagement.NewFileAdd(root, filename, this.DWMC);
                        //file_AddnewFile(root, filename, this.DWMC);
                        Cursor.Current = Cursors.Default;
                    }
                    this.createitem();
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "AddButton_Click");
                MessageBox.Show("请连接到文件服务器！");
            }
        }

        // 删除按钮
        private void SubtractButton_Click(object sender, EventArgs e)
        {
            if (dtEdit.Rows[0]["└业务数据可删、改"].ToString().Trim() == "0")
            {
                MessageBox.Show("您没有删除权限！", "提示");
                return;
            }
            try
            {
                if (MessageBox.Show("确定要删除吗?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
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
            catch (Exception ex)
            {
                writeToLog(ex, "SubtractButton_Click");
            }
            Cursor.Current = Cursors.Default;
        }

        // 关闭按钮
        private void CButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch
            {

            }
        }

        private void file_AddnewFile(string root, string filename, string p)
        {
            try
            {
                CLC.INIClass.IniWriteValue("文件", filename, "1");
                System.Net.WebClient WC = new System.Net.WebClient();
                WC.UploadFile("http://" + CLC.INIClass.IniReadValue("文件服务器", "IP1") + "/lbschina/Data/InfoData/" + filename, "PUT", root + "\\" + filename);
                string SQLString;
                SQLString = "SELECT COUNT(*) FROM 安全防护单位文件";
                int I = CLC.DatabaseRelated.OracleDriver.OracleComScalar(SQLString);
                SQLString = "DELETE 安全防护单位文件 WHERE 文件名称 = '" + filename + "'";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(SQLString);
                SQLString = "INSERT INTO 安全防护单位文件(ID,文件名称,单位名称,文件版本) VALUES ( " + I + " + 1,'" + filename + "','" + DWMC + "','1')";
                CLC.DatabaseRelated.OracleDriver.OracleComRun(SQLString);
            }
            catch (Exception e)
            {
                writeToLog(e, "file_AddnewFile");
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                ProcessStartInfo pInfo = new ProcessStartInfo();

                foreach (ListViewItem listitem in this.listView1.SelectedItems)
                {
                    string root = Application.StartupPath;
                    pInfo.FileName = root.Remove(root.LastIndexOf("\\")) + @"\Data\InfoData\" + listitem.Text;

                    Process p = Process.Start(pInfo);
                }
            }
            catch (Exception ex)
            {
                writeToLog(ex, "listView1_DoubleClick");
                MessageBox.Show("打开文件错误!");
            }
        }
    }
}