using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace LBSgisPolice110
{
    public partial class frmInput : Form
    {
        public frmInput()
        {
            InitializeComponent();
        }

        public string _DataSource;  //数据库名称
        public string _DataUser;    //数据库用户名
        public string _DataPass;    //数据库密码

        public string _Mainport;    // 管理客户端 端口
        public string _Videoport;   // 监控客户端 端口

        public string _MapPath; // 地图名称

        public string _show = "false";  //地图是否显示
        public string _VideoPath = ""; // 视频客户端启动位置 

        public string _Dist; // 半径

        public string _Region;// 区域



        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                 if(this.txtMainPort.Text == String.Empty)  
                   {
                       MessageBox.Show("请输入管理客户端端口", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                       return;
                   }

                   if (this.txtVideoPort.Text == String.Empty)
                   {
                       MessageBox.Show("请输入监控客户端端口", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                       return;
                   }

                    if (this.txtDataSource.Text == String.Empty)
                    {
                       MessageBox.Show("请输入数据库名称", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                       return;
                    }

                    if (this.txtUser.Text == String.Empty)
                    {
                       MessageBox.Show("请输入数据库用户名称", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                       return;
                    }

                    if (this.txtPass.Text == String.Empty)
                    {
                        MessageBox.Show("请输入数据库用户密码", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (this.txtDist.Text == String.Empty)
                    {
                        MessageBox.Show("请输入范围查询半径", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (this.txtMap.Text == String.Empty)
                    {
                        MessageBox.Show("请选择地图文件", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (this.txtVideo.Text == String.Empty)
                    {
                        MessageBox.Show("请选择监控程序路径", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //if (this.txtzone.Text == String.Empty)
                    //{
                    //    MessageBox.Show("请输入辖区名称", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                    if (this.txtVideo.Text.IndexOf(".exe") == -1)
                    {
                        MessageBox.Show("监控客户端应该为应用程序", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (this.txtMap.Text.IndexOf("mws") == -1)
                    {
                        MessageBox.Show("请选择正确格式的地图文件", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    this._DataSource = this.txtDataSource.Text.Trim();
                    this._DataUser = this.txtUser.Text.Trim();

                    this._DataPass = StringToHex(this.txtPass.Text.Trim());

                    this._Mainport = this.txtMainPort.Text.Trim();
                    this._Videoport = this.txtVideoPort.Text.Trim();

                    //this._MapPath = this.txtMap.Text.Trim();

                    this._show = "true";

                    this._VideoPath = this.txtVideo.Text.Trim();
                    this._Dist = this.txtDist.Text.Trim();

                    if (this.txtzone.Text == string.Empty || this.txtzone.Text == "")
                    {
                        this._Region = " ";
                    }
                    else
                    {
                        this._Region = this.txtzone.Text.Trim();
                    }

                    CreatXML();

                    this.DialogResult = DialogResult.OK;
            }
            catch(Exception exception)
            {

                MessageBox.Show("保存配置文件失败，详情请查看日志文件", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                writelog(exception.Message);
                this.DialogResult = DialogResult.Cancel;
            }            
        }




        /// <summary>
        /// 字符转换为十六进制
        /// </summary>
        /// <param name="HexCode"></param>
        /// <returns></returns>
        private string StringToHex(String StringCode)
        {
            string strReturn = "";// 存储转换后的编码
            foreach (short shortx in StringCode.ToCharArray())
            {
                strReturn += shortx.ToString("X4");
            }
            return strReturn;
        }


        public void setparaments(string Datasource,string Datauser,string Datapass,string mainport,string videoport,string mappath,string videopath,string reg,double dist)
        {
            this._DataSource = Datasource;
            this._DataUser = Datauser;
            this._DataPass = Datapass;

            this._Mainport = mainport;
            this._Videoport = videoport;
            this._Dist = dist.ToString();


            this._MapPath = mappath;
            this._VideoPath = videopath;
            this._Region = reg;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

          private void txtMap_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog MyDlg = new OpenFileDialog();

            MyDlg.Filter = "地图文件(*.mws)|*.mws";

            MyDlg.InitialDirectory = Application.StartupPath + "\\Map\\";

            MyDlg.FileName = MyDlg.InitialDirectory + "Map.mws";

            MyDlg.Title = "选择地图";

            MyDlg.AddExtension = true;

            MyDlg.DefaultExt = "mws";

            if (MyDlg.ShowDialog() == DialogResult.OK)
            {
                this.txtMap.Text = MyDlg.FileName;

                this._MapPath = this.txtMap.Text.Substring(MyDlg.InitialDirectory.Length, this.txtMap.Text.Length - MyDlg.InitialDirectory.Length );

            }
        }

        private void txtVideo_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog MyDlg = new OpenFileDialog();

            MyDlg.InitialDirectory = Application.StartupPath;

            MyDlg.Filter = "应用程序(*.exe)|*.exe";

            MyDlg.Title = "选择监控客户端";

            MyDlg.AddExtension = true;

            MyDlg.DefaultExt = "exe";

            if (MyDlg.ShowDialog() == DialogResult.OK)
            {
                this.txtVideo.Text = MyDlg.FileName;
            }
        }


        private void CreatXML()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(Application.StartupPath + @"\Config.XML");
                
                XmlNodeList nodeList = xmlDoc.SelectSingleNode("Con").ChildNodes;
                
                foreach (XmlNode mynode in nodeList)
                {
                    if (mynode.Name == "Listenport")
                    {
                        mynode.InnerText = this._Mainport;
                    }
                    if (mynode.Name == "ListenportVideo")
                    {
                        mynode.InnerText = this._Videoport;
                    }

                    if (mynode.Name == "Map")
                    {
                        mynode.InnerText = this._MapPath;
                    }

                    if (mynode.Name == "user")
                    {
                        mynode.InnerText = this._DataUser;
                    }

                    if (mynode.Name == "ds")
                    {
                        mynode.InnerText = this._DataSource;
                    }

                    if (mynode.Name == "password")
                    {
                        mynode.InnerText = this._DataPass;
                    }

                    if (mynode.Name == "dist")
                    {
                        mynode.InnerText = this._Dist;
                    }

                    if (mynode.Name == "showMap")
                    {
                        mynode.InnerText = "true";
                    }

                    if (mynode.Name == "VideoExePath")
                    {
                        mynode.InnerText = this._VideoPath;
                    }

                    if (mynode.Name == "zone")
                    {
                        mynode.InnerText = this._Region;
                    }

                }
                xmlDoc.Save(Application.StartupPath + @"\Config.XML");//保存。
            }
            catch (System.Exception e)
            {
                MessageBox.Show("创建配置文件失败", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                writelog("创建文件失败" + e.Message);
                return;
            } 
        }

        private void frmInput_Load(object sender, EventArgs e)
        {
            this.txtDataSource.Text = this._DataSource;
            this.txtDist.Text = this._Dist;
            this.txtMainPort.Text = this._Mainport;
            this.txtMap.Text  = this._MapPath;
            this.txtPass.Text = this._DataPass;
            this.txtUser.Text = this._DataUser;
            this.txtVideo.Text = this._VideoPath;
            this.txtVideoPort.Text = this._Videoport;
            this.txtzone.Text = this._Region;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog MyDlg = new OpenFileDialog();

            MyDlg.Filter = "地图文件(*.mws)|*.mws";

            MyDlg.InitialDirectory = Application.StartupPath + "\\Map\\";

            MyDlg.FileName = MyDlg.InitialDirectory + "Map.mws";

            MyDlg.Title = "选择地图";

            MyDlg.AddExtension = true;

            MyDlg.DefaultExt = "mws";

            if (MyDlg.ShowDialog() == DialogResult.OK)
            {
                this.txtMap.Text = MyDlg.FileName;

                this._MapPath = this.txtMap.Text;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog MyDlg = new OpenFileDialog();

            MyDlg.InitialDirectory = Application.StartupPath;

            MyDlg.Filter = "应用程序(*.exe)|*.exe";

            MyDlg.Title = "选择监控客户端";

            MyDlg.AddExtension = true;

            MyDlg.DefaultExt = "exe";

            if (MyDlg.ShowDialog() == DialogResult.OK)
            {                
                    this.txtVideo.Text = MyDlg.FileName;                   
            }
        }

        public void writelog(string msg)
        {
            try
            {
                string filepath = Application.StartupPath + "\\rec.log";
                msg = DateTime.Now.ToString() + ":" + msg;

                StreamWriter sw = File.AppendText(filepath);
                sw.WriteLine(msg);
                sw.Flush();
                sw.Close();
            }
            catch
            {

            }

        }
   
    }

        
}