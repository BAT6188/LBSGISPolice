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


        //��������
        private void btnOK_Click(object sender, EventArgs e)
        {

            this.disRadius = Convert.ToDouble(this.txbAddress.Text.Trim());
            if (this.txbAddress.Text == String.Empty)
            {
                MessageBox.Show("�����뷶Χ��ѯ�뾶", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Boolean isnum = System.Text.RegularExpressions.Regex.IsMatch(this.txbAddress.Text.Trim(), @"^\d+(\d*)?$");
            if (isnum)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("����Ĳ��������ͣ�����������", "ϵͳ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


           //GetCon();

           //Boolean sf = SendMessage(this.txbAddress.Text);
           //if (sf == true)
           //{
           //    MessageBox.Show("���ͳɹ�");
           //}
           //else
           //{
           //    MessageBox.Show("����ʧ��");
           //}


            //try
            //{

            //    //�����ͻ����׽���
            //    TcpClient tcpclnt = new TcpClient();
            //    //Console.WriteLine("����...");

            //    //���ӷ�����
            //    tcpclnt.Connect("127.0.0.1", 8081);

            //    //�õ��ͻ��˵���
            //    Stream stm = tcpclnt.GetStream();

            //    //Console.WriteLine("������");
            //    //Console.Write("��˵:");
            //    string str = this.txbAddress.Text;//����˵������,��������վ����

            //    //�����ַ���
            //    ASCIIEncoding ascen = new ASCIIEncoding();
            //    byte[] ba = ascen.GetBytes(str);
            //    stm.Write(ba, 0, ba.Length);


            //    //���մӷ��������ص���Ϣ
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
            //    //Console.WriteLine("����..." + e.StackTrace);
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
        //        TempStr += "0X"+b.ToString("X"); //X��ʾʮ��������ʾ
        //    }
        //    return TempStr;
        //}


        //private void button1_Click(object sender, EventArgs e)
        //{
        //    //������������
        //    ConSocket();
            
        //    //������������
           
        //}

        ////�������
        //public void ConSocket()
        //{
        //    Control.CheckForIllegalCrossThreadCalls = false;

        //    //thThreadRead = new Thread(new ThreadStart(Listening));
        //    //thThreadRead.Start();
        //    //this.lblStatus.Text = "����:��������--" + this.Listenip + ":" + this.Listenport.ToString();

        //    HostIP = IPAddress.Parse("127.0.0.1");
        //    try
        //    {
        //        point = new IPEndPoint(HostIP, Int32.Parse("8081"));
        //        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Connect(point);
        //        Thread thread = new Thread(new ThreadStart(Proccess));
        //        thread.Start();
        //        MessageBox.Show("�����ӷ�����");
        //    }
        //    catch (Exception ey)
        //    {
        //        MessageBox.Show("������û�п���\r\n" + ey.Message);
        //    } 
        //}

        //private void Listening()
        //{
        //    //IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());
        //    try
        //    {
        //        //����
        //        IPAddress ip;
        //        ip = IPAddress.Parse("127.0.0.1");
        //        _tcpListener = new TcpListener(ip,8081);
        //        _tcpListener.Start();
        //    }
        //    catch
        //    {
        //        this.lblStatus.Text = "����:�޷�����ͨѶ,��鿴" + this.Listenport + "�˿��Ƿ�ʹ��";
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
        //       // ShowDoInfo("��������ʱ��������");
        //    }
        //}


        //// ������Ϣ
        //private StreamWriter swWriter;
        //private NetworkStream SendStream;
        //private TcpClient SendtcpClient;

        //private void GetCon()
        //{
        //    //�жϸ�����IP��ַ�ĺϷ���
        //    IPAddress ipRemote;
        //    try
        //    {
        //        ipRemote = IPAddress.Parse("127.0.0.1");
        //    }
        //    catch
        //    {
        //        MessageBox.Show("����IP��ַ���Ϸ���", "MapInfo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }


        //    //�ж�IP��ַ��Ӧ�����Ƿ�����
        //    IPHostEntry ipHost;
        //    try
        //    {
        //        ipHost = Dns.GetHostEntry(this.Sendip);
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Զ�����������ߣ�", "MapInfo��", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }


        //    string sHostName = ipHost.HostName;

        //    sHostName = this.Sendip;

        //    int sHostport = this.Sendport;
        //    try
        //    {
        //        SendtcpClient = new TcpClient(sHostName, sHostport);//��Զ��������8000�˿����TCP��������
        //        SendStream = SendtcpClient.GetStream();//ͨ�����룬����ȡ�������ݵ������������������
        //        swWriter = new StreamWriter(SendStream);//ʹ�û�ȡ�������������������ʼ��StreamWriterʵ��               
        //    }
        //    catch
        //    {
        //        MessageBox.Show("�޷���Զ�������������ӣ�", "MapInfo��", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }
        //}


        //private bool SendMessage(string id)
        //{
        //    ////�жϸ�����IP��ַ�ĺϷ���
        //    //IPAddress ipRemote;
        //    //try
        //    //{
        //    //    ipRemote = IPAddress.Parse("127.0.0.1");
        //    //}
        //    //catch
        //    //{
        //    //    MessageBox.Show("����IP��ַ���Ϸ���", "MapInfo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    return false ;
        //    //}


        //    ////�ж�IP��ַ��Ӧ�����Ƿ�����
        //    //IPHostEntry ipHost;
        //    //try
        //    //{
        //    //    ipHost = Dns.GetHostEntry(this.Sendip);
        //    //}
        //    //catch
        //    //{
        //    //    MessageBox.Show("Զ�����������ߣ�", "MapInfo��", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    return;
        //    //}


        //    //string sHostName = ipHost.HostName;

        //    //sHostName = this.Sendip;

        //    //int sHostport = this.Sendport;
        //    //try
        //    //{
        //    //    SendtcpClient = new TcpClient(sHostName, sHostport);//��Զ��������8000�˿����TCP��������
        //    //    SendStream = SendtcpClient.GetStream();//ͨ�����룬����ȡ�������ݵ������������������
        //    //    swWriter = new StreamWriter(SendStream);//ʹ�û�ȡ�������������������ʼ��StreamWriterʵ��               
        //    //}
        //    //catch
        //    //{
        //    //    MessageBox.Show("�޷���Զ�������������ӣ�", "MapInfo��", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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


        //#region//��������
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