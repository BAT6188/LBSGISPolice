using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace LBSgisPolice110
{
    public partial class FrmGeoCoding :Form 
    {
        public double disRadius = 0; 
      
        public FrmGeoCoding()
        {
            InitializeComponent();
        }


        //发送数据
        private void btnOK_Click(object sender, EventArgs e)
        {

            this.disRadius = Convert.ToDouble(this.txbAddress.Text.Trim());
            if (this.txbAddress.Text == String.Empty)
            {
                MessageBox.Show("请输入范围查询半径", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Boolean isnum = System.Text.RegularExpressions.Regex.IsMatch(this.txbAddress.Text.Trim(), @"^\d+(\d*)?$");
            if (isnum)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("输入的参数非整型，请重新输入", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


           //GetCon();

           //Boolean sf = SendMessage(this.txbAddress.Text);
           //if (sf == true)
           //{
           //    MessageBox.Show("发送成功");
           //}
           //else
           //{
           //    MessageBox.Show("发送失败");
           //}


            //try
            //{

            //    //建立客户端套接字
            //    TcpClient tcpclnt = new TcpClient();
            //    //Console.WriteLine("连接...");

            //    //连接服务器
            //    tcpclnt.Connect("127.0.0.1", 8081);

            //    //得到客户端的流
            //    Stream stm = tcpclnt.GetStream();

            //    //Console.WriteLine("已连接");
            //    //Console.Write("你说:");
            //    string str = this.txbAddress.Text;//输入说话内容,如义乌网站制作

            //    //发送字符串
            //    ASCIIEncoding ascen = new ASCIIEncoding();
            //    byte[] ba = ascen.GetBytes(str);
            //    stm.Write(ba, 0, ba.Length);


            //    //接收从服务器返回的信息
            //    byte[] bb = new byte[100];
            //    int k = stm.Read(bb, 0, 100);
            //    string chat = "";
            //    for (int i = 0; i < k; i++)
            //    {
            //        chat = chat + Convert.ToString(bb[i]);
            //     }
            //     this.listBox1.Items.Add(chat);
            //    tcpclnt.Close();

            //}
            //catch 
            //{
            //    //Console.WriteLine("错误..." + e.StackTrace);
            //}

            //try
            //{
                //Byte[] sendByte = new Byte[64];
                //string sendStr = this.txbAddress.Text;

                //string temp = StringToHex(sendStr);                
                
                //int a = int.Parse("2", System.Globalization.NumberStyles.AllowHexSpecifier);

                ////sendByte = Encoding.ASCII.GetBytes(sendStr.ToCharArray());
                ////sendByte = Encoding.ASCII.GetBytes(temp);
                //for (int i = 0; i < temp.Length; i++)
                //{
                //    sendByte[i] = Convert.ToByte( temp.Substring(i, 1));
                //}

                //socket.Send(sendByte, sendByte.Length, 0);

            //}
            //catch { } 
        }

        private void FrmGeoCoding_Load(object sender, EventArgs e)
        {
            this.txbAddress.Text = this.disRadius.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        //public string StringToHex(string str)
        //{
        //    //str = str.Trim();
        //    byte[] ByteFoo = System.Text.Encoding.Default.GetBytes(str);
        //    string TempStr = "";
        //    foreach (byte b in ByteFoo)
        //    {
        //        TempStr += "0X"+b.ToString("X"); //X表示十六进制显示
        //    }
        //    return TempStr;
        //}


        //private void button1_Click(object sender, EventArgs e)
        //{
        //    //建立监听连接
        //    ConSocket();
            
        //    //建立发送连接
           
        //}

        ////建立监控
        //public void ConSocket()
        //{
        //    Control.CheckForIllegalCrossThreadCalls = false;

        //    //thThreadRead = new Thread(new ThreadStart(Listening));
        //    //thThreadRead.Start();
        //    //this.lblStatus.Text = "网络:建立监听--" + this.Listenip + ":" + this.Listenport.ToString();

        //    HostIP = IPAddress.Parse("127.0.0.1");
        //    try
        //    {
        //        point = new IPEndPoint(HostIP, Int32.Parse("8081"));
        //        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Connect(point);
        //        Thread thread = new Thread(new ThreadStart(Proccess));
        //        thread.Start();
        //        MessageBox.Show("已连接服务器");
        //    }
        //    catch (Exception ey)
        //    {
        //        MessageBox.Show("服务器没有开启\r\n" + ey.Message);
        //    } 
        //}

        //private void Listening()
        //{
        //    //IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());
        //    try
        //    {
        //        //监听
        //        IPAddress ip;
        //        ip = IPAddress.Parse("127.0.0.1");
        //        _tcpListener = new TcpListener(ip,8081);
        //        _tcpListener.Start();
        //    }
        //    catch
        //    {
        //        this.lblStatus.Text = "服务:无法建立通讯,请查看" + this.Listenport + "端口是否被使用";
        //        return;
        //    }

        //    try
        //    {
        //        _tcpClient = _tcpListener.AcceptTcpClient();
        //        string receiveMessage = null;
        //        NetworkStream stream = _tcpClient.GetStream();
        //        StreamReader srRead = new StreamReader(stream);

        //        while (true)
        //        {
        //            receiveMessage = srRead.ReadLine();
        //            if (receiveMessage.Length != 0)
        //            {
        //                this.listBox1.Items.Add(receiveMessage);
        //            }                 
        //        }
        //    }
        //    catch
        //    {
        //       // ShowDoInfo("接收数据时发生错误！");
        //    }
        //}


        //// 发送信息
        //private StreamWriter swWriter;
        //private NetworkStream SendStream;
        //private TcpClient SendtcpClient;

        //private void GetCon()
        //{
        //    //判断给定的IP地址的合法性
        //    IPAddress ipRemote;
        //    try
        //    {
        //        ipRemote = IPAddress.Parse("127.0.0.1");
        //    }
        //    catch
        //    {
        //        MessageBox.Show("发送IP地址不合法！", "MapInfo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }


        //    //判断IP地址对应主机是否在线
        //    IPHostEntry ipHost;
        //    try
        //    {
        //        ipHost = Dns.GetHostEntry(this.Sendip);
        //    }
        //    catch
        //    {
        //        MessageBox.Show("远程主机不在线！", "MapInfo！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }


        //    string sHostName = ipHost.HostName;

        //    sHostName = this.Sendip;

        //    int sHostport = this.Sendport;
        //    try
        //    {
        //        SendtcpClient = new TcpClient(sHostName, sHostport);//对远程主机的8000端口提出TCP连接申请
        //        SendStream = SendtcpClient.GetStream();//通过申请，并获取传送数据的网络基础数据流　　
        //        swWriter = new StreamWriter(SendStream);//使用获取的网络基础数据流来初始化StreamWriter实例               
        //    }
        //    catch
        //    {
        //        MessageBox.Show("无法和远程主机建立连接！", "MapInfo！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }
        //}


        //private bool SendMessage(string id)
        //{
        //    ////判断给定的IP地址的合法性
        //    //IPAddress ipRemote;
        //    //try
        //    //{
        //    //    ipRemote = IPAddress.Parse("127.0.0.1");
        //    //}
        //    //catch
        //    //{
        //    //    MessageBox.Show("发送IP地址不合法！", "MapInfo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    return false ;
        //    //}


        //    ////判断IP地址对应主机是否在线
        //    //IPHostEntry ipHost;
        //    //try
        //    //{
        //    //    ipHost = Dns.GetHostEntry(this.Sendip);
        //    //}
        //    //catch
        //    //{
        //    //    MessageBox.Show("远程主机不在线！", "MapInfo！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    return;
        //    //}


        //    //string sHostName = ipHost.HostName;

        //    //sHostName = this.Sendip;

        //    //int sHostport = this.Sendport;
        //    //try
        //    //{
        //    //    SendtcpClient = new TcpClient(sHostName, sHostport);//对远程主机的8000端口提出TCP连接申请
        //    //    SendStream = SendtcpClient.GetStream();//通过申请，并获取传送数据的网络基础数据流　　
        //    //    swWriter = new StreamWriter(SendStream);//使用获取的网络基础数据流来初始化StreamWriter实例               
        //    //}
        //    //catch
        //    //{
        //    //    MessageBox.Show("无法和远程主机建立连接！", "MapInfo！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    return;
        //    //}



        //    //try
        //    //{
        //    //    if (swWriter != null)
        //    //    {
        //    //        swWriter.WriteLine(id);
        //    //        swWriter.Flush();
        //    //        return true;
        //    //    }
        //    //    else
        //    //    {                  
        //    //        return false;
        //    //    }
        //    //}
        //    //catch
        //    //{                
        //    return false;
        //    //}
        //}

        //private void FrmGeoCoding_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    try
        //    {
        //        //this.thThreadRead.Abort();
        //        //if (_tcpListener != null)
        //        //    _tcpListener.Stop();

        //        //if (SendtcpClient != null)
        //        //    SendtcpClient.Close();
        //        //Application.ExitThread();
        //        //this.Dispose();
        //    }
        //    catch
        //    { }
        //}


        //#region//声名变量
        //IPAddress HostIP = IPAddress.Parse("127.0.0.1");
        //IPEndPoint point;
        //Socket socket;
        //bool flag = true;
        //#endregion 

        //private void Proccess()
        //{
        //    if (socket.Connected)
        //    {
        //        while (flag)
        //        {
        //            byte[] receiveByte = new byte[200];
        //            socket.Receive(receiveByte, receiveByte.Length, 0);
        //            string strInfo = Encoding.Unicode.GetString(receiveByte);
        //            this.listBox1.Items.Add(strInfo);

        //        }
        //    }
        //} 
    }
}