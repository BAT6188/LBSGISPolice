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

namespace clKaKou
{
    public partial class frmZLMessage : Form
    {
        private string DWMC;

        private  string  strConn ="";

        public frmZLMessage()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();

			//
			// TODO: 在 InitializeComponent 调用后添加任何构造函数代码
			//
		}

        /// <summary>
        /// 构造函数
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="DWMC">单位名称</param>
        /// <param name="s">连接字符串</param>
        public frmZLMessage(string DWMC, string s)
        {
            try
            {
                //
                // Windows 窗体设计器支持所必需的
                //
                InitializeComponent();
                this.DWMC = DWMC;
                string root = Application.StartupPath;

                CLC.INIClass.IniPathSet(root.Remove(root.LastIndexOf("\\")) + "\\config.ini");
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

                ClsFileManagement.ClsFileManagement.SetConstring(root.Remove(root.LastIndexOf("\\")) + @"\config.ini", serverIP);
                strConn = s;
                //
                // TODO: 在 InitializeComponent 调用后添加任何构造函数代码
                //
            }
            catch (Exception ex)
            {
                writeZongheLog(ex, "FrmZLMessage-02-构造函数");
            }
        }

        /// <summary>
        /// 关闭窗体
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 检测服务器
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="webResourceAddress">服务器地址</param>
        /// <returns>布尔值(true-显示 false-隐藏)</returns>
        private bool IsWebResourceAvailable(string webResourceAddress)
        {
            TcpClient tcpClient = null;
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(webResourceAddress), 80);
                tcpClient = new TcpClient();
                tcpClient.Connect(ipep);

                return true;
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
        /// 查询单位名称所属文件并显示
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void createitem()
        {
            OracleConnection conn = new System.Data.OracleClient.OracleConnection(strConn);
            try
            {
                ListViewItem lvi;
                ListViewItem.ListViewSubItem lvsi;
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
                writeZongheLog(ex, "createitem");
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                MessageBox.Show("无相关详情!");
            }
        }

        /// <summary>
        /// 双击实现查看文件
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
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
                writeZongheLog(ex, "listView1_DoubleClick");
                MessageBox.Show("打开文件错误!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root">文件地址</param>
        /// <param name="filename">文件名</param>
        /// <param name="p"></param>
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
                writeZongheLog(e, "file_AddnewFile");
            }
        }

        /// <summary>
        /// 异常输出
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void writeZongheLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "clKaKou-frmZLMessage" + sFunc);
        }

        /// <summary>
        /// 窗体加载初始化
        /// 最后编辑人   李立
        /// 最后编辑时间  2011-1-28
        /// </summary>
        private void frmZLMessage_Load(object sender, EventArgs e)
        {
            this.createitem();
        }
    }
}