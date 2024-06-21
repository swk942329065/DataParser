using DataBaseAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Timers;
using CommonTools;
using System.Text.RegularExpressions;
using System.Configuration;
using VSCapture;
using System.IO.Ports;
using log4net;

namespace DataAnalysis
{

    public partial class Form1 : Form
    {
        private static ILog log = LogManager.GetLogger(typeof(Form1));
        MysqlBLL bll = new MysqlBLL();
        private static EventHandler dataEvent;
        Socket socketMonitorListen = null;
        Socket socketMonitorListenT5 = null;
        System.Timers.Timer geB650Timer = null;

        System.Timers.Timer T5Timer = null;
        System.Timers.Timer xqyTimer = null;
        private SerialPort serialPort = null;
        //创建一个和客户端通信的套接字
        static Socket SocketWatch = null;
        //定义一个集合，存储客户端信息
        static Dictionary<string, Socket> ClientConnectionItems = new Dictionary<string, Socket> { };

        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 读取 APP.config 键的值
            string IP = ConfigurationManager.AppSettings["IP"];
            log.Debug("静态资源参数APP'config" + IP);
            log.Info("静态资源参数APP'config" + IP);
            log.Error("静态资源参数APP'config" + IP);
            ShowMsg("静态资源参数APP'config" + IP);
            this.WindowState = FormWindowState.Minimized; // 设置窗口状态为最小化  
            this.ShowInTaskbar = false; // 不在任务栏中显示窗口  
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ShowMsg("迈瑞监护仪服务端！");
            button1.Enabled = false;

            Thread worker = new Thread(() =>
             {
                 mrN();

             });
            worker.Start();


        }

        private void button2_Click(object sender, EventArgs e)
        {
            // ShowMsg("客户端请求启动");
            //  TimerRespirator_ElapsedMR1B();
            button2.Enabled = false;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            // string gePort = ConfigurationManager.AppSettings["gePort"];
            //   ShowMsg("ge监护仪串口启动"+ gePort);
            button3.Enabled = false;
            //  geB650Timer = new System.Timers.Timer(30000); // 1000毫秒 = 1秒  
            //   geB650Timer.Elapsed += geB650Start;
            //   geB650Timer.Start();

            //   geB650Start(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // ShowMsg("新客户端请求启动");

            //  Thread worker = new Thread(() =>
            //   {
            //       khd();

            //   });
            //  worker.Start();



            button4.Enabled = false;
        }


        private void button5_Click(object sender, EventArgs e)
        {
            ShowMsg("T5客户端请求启动");
            button5.Enabled = false;
            T5Timer = new System.Timers.Timer(60000); // 1000毫秒 = 1秒  
            T5Timer.Elapsed += TimerRespirator_ElapsedT5;
            T5Timer.Start();

            Thread worker = new Thread(() =>
            {
                TimerRespirator_ElapsedT5(null, null);

            });
            worker.Start();



        }

        private void button6_Click(object sender, EventArgs e)
        {
            //  ShowMsg("血气仪客户端请求启动");
            //  xqyTimer = new System.Timers.Timer(15000);
            //  xqyTimer.Elapsed += TimerRespirator_ElapsedXL;
            //   xqyTimer.Enabled = true;
            button6.Enabled = false;
            // TimerRespirator_ElapsedXL(null, null);
        }
        private void TimerRespirator_ElapsedT5(object sender, ElapsedEventArgs e)
        {
            string ipList = ConfigurationManager.AppSettings["T5Ip"];
            string[] ips = ipList.Split('+');

            foreach (string host in ips)
            {
                ShowMsg("T5客户端请求启动" + host);
                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint endPoint = new IPEndPoint(ip, 4601);
                //ShowMsg("与" + endPoint + "连接中……");
                socketMonitorListenT5 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {  //sockClient.ReceiveTimeout= 30000;
                    var ar = socketMonitorListenT5.BeginConnect(endPoint, null, null);
                    ar.AsyncWaitHandle.WaitOne(200);

                    if (socketMonitorListenT5.Connected)
                    {

                        //dict.Add(sockClient.RemoteEndPoint.ToString(), sockClient);
                        ShowMsg("T5客户端请求启动" + "连接成功！");
                        Thread threadClient = new Thread(RecMsgT5);
                        threadClient.IsBackground = true;
                        threadClient.Start(socketMonitorListenT5);            //dictThread.Add(sockClient.RemoteEndPoint.ToString(), threadClient);
                    }
                    else
                    {
                        //ShowMsg("与" + endPoint + "连接失败！！！");
                    }
                }
                catch (SocketException ex)
                {
                    // 处理SocketException异常  
                    ShowMsg("SocketException: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // 处理其他异常  
                    ShowMsg("Exception: " + ex.Message);
                }
            }

        }

        private void RecMsgT5(object sokConnectionparn)
        {
            Socket sockClient = sokConnectionparn as Socket;

            try
            {
                string msg2 = "0b4d53487c5e7e5c267c4d696e647261797c504453546573747c7c7c7c7c5152595e5230327c313230337c507c322e332e310d5152447c32303136303932313139333033377c527c497c4d52517279327c7c7c7c7c5245530d5152467c4d4f4e7c7c7c7c3026305e315e315e315e0d1c0d";
                sockClient.Send(strToToHexByte("7c527c")); // 发送消息；
                Thread.Sleep(1000);
                sockClient.Send(strToToHexByte(msg2)); // 发送消息

            }
            catch (SocketException ex)
            {
                sockClient.Dispose();
                sockClient.Close();
                // 处理套接字异常  
                log.Info("发送数据时发生套接字异常: " + ex.Message);
            }
            catch (Exception ex)
            {
                sockClient.Dispose();
                sockClient.Close();
                // 处理其他可能的异常  
                log.Info("发送数据时发生异常: " + ex.Message);
            }
            string data = "";
            while (true)
            {
                // 发送消息；
                // 定义一个2M的缓存区；
                byte[] arrMsgRec = new byte[1024 * 1024 * 2];
                Thread.Sleep(1500);
                // 将接受到的数据存入到输入  arrMsgRec中；
                int length = -1;
                try
                {
                    length = sockClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度；
                }
                catch (SocketException se)
                {
                    //ShowMsg("异常：" + se.Message);
                    //Console.Write("连接关闭1" + se.Message);

                    // 从 通信套接字 集合中删除被中断连接的通信套接字；
                    //dict.Remove(sockClient.RemoteEndPoint.ToString());
                    // 从通信线程集合中删除被中断连接的通信线程对象；
                    //dictThread.Remove(sockClient.RemoteEndPoint.ToString());
                    break;
                }
                catch (Exception e)
                {
                    //ShowMsg("异常：" + e.Message);

                    // 从 通信套接字 集合中删除被中断连接的通信套接字；
                    //dict.Remove(sockClient.RemoteEndPoint.ToString());
                    // 从通信线程集合中删除被中断连接的通信线程对象；
                    //dictThread.Remove(sockClient.RemoteEndPoint.ToString());
                    break;
                }

                string strMsg = Encoding.UTF8.GetString(arrMsgRec, 0, length);
                data = strMsg;
                if (length > 0)
                {
                    HL7ToXmlConverter hl = new HL7ToXmlConverter();
                    try
                    {
                        //data = data.Substring(data.IndexOf("MSH|^~\\&|||||||ORU^R01|204|P|2.3.1|"), data.Length - (data.IndexOf("MSH|^~\\&|||||||ORU^R01|204|P|2.3.1|")));
                        data = data.Replace("\v", "").Replace("\r", "").Replace("&", "||");
                        //HLT协议
                        List<string> H7List = hl.formatGetHl7T5(data);
                        foreach (string h7 in H7List)
                        {
                            //转xml
                            XmlDocument document = hl.ConvertToXmlObject(h7.Replace("\u000B", "").Replace("\u000A", "").Replace("\u000D", ""));
                            XmlElement root = document.DocumentElement;
                            XmlNodeList listNodes = root.SelectNodes("/HL7Message/OBX");
                            foreach (XmlNode Element in listNodes)
                            {

                                string code = Element.SelectSingleNode("OBX.3").InnerText;
                                string val = Element.SelectSingleNode("OBX.5").InnerText;
                                Dictionary<string, string> result = formatConversion.equipmentCodeToDataCodeT5(code.Split('^')[0]);
                                if (result.Count > 0)
                                {
                                    ShowMsg("----" + result["code"] + "------" + result["codeName"] + "----" + val);
                                    SysDeviceData m = new SysDeviceData();
                                    m.dataCode = result["code"];
                                    m.deviceIp = ((IPEndPoint)sockClient.RemoteEndPoint).Address.ToString();
                                    m.dataName = result["codeName"];
                                    m.deviceType = "1";
                                    m.createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                                    m.dataValue = val;
                                    m.remark = "";
                                    if (val.Contains("-"))
                                    {
                                    }
                                    else
                                    {
                                        bll.InsertOne(m);
                                    }

                                }
                                else
                                {
                                    log.Info("客户端数据--" + code.Split('^')[0] + "------" + "----" + val);
                                }



                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Info("解析数据出现异常: " + ex.Message);
                        break;
                    }

                    Thread.Sleep(1000);
                    sockClient.Dispose();
                    sockClient.Close();
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                    sockClient.Dispose();
                    sockClient.Close();
                    break;
                }
            }



        }
        private void geB650Start(object sender, ElapsedEventArgs e)
        {
            DSerialPort getInstance = DSerialPort.getInstance;

            if (!getInstance.IsOpen)
            {
                try
                {
                    Console.WriteLine("打开串口");
                    // 调用方法  
                    geB650();
                    // 连接成功后，可以在这里执行其他操作  
                }
                catch (Exception ex)
                {
                    Console.WriteLine("22222");
                }
            }
            else
            {
                Console.WriteLine("串口已经打开");
            }
        }


        private void khd()
        {
            string serverIP = ConfigurationManager.AppSettings["khdserverIP"];
            string khdserverPort = ConfigurationManager.AppSettings["khdserverPort"];
            string host = ConfigurationManager.AppSettings["khdIp"];
            int serverPort = int.Parse(khdserverPort); // 服务器端口号  
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 连接到服务端  
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIP), serverPort));
                ShowMsg("连接成功");

                // 接收数据循环  
                while (true)
                {
                    byte[] data = new byte[1024];

                    int bytesReceived = clientSocket.Receive(data);
                    string dataTotal = Encoding.UTF8.GetString(data, 0, bytesReceived);
                    if (dataTotal.Length > 0)
                    {
                        ShowMsg("接收到服务器消息: " + dataTotal);
                        HL7ToXmlConverter htxc = new HL7ToXmlConverter();
                        List<string> hl7List = htxc.formatGetHl7(dataTotal);
                        foreach (string hlt in hl7List)
                        {
                            string sHL7asXml = htxc.ConvertToXml(hlt);
                            XmlDocument xmlObject1 = htxc.ConvertToXmlObject(hlt);
                            string pId = "";
                            try
                            {
                                //床号
                                pId = xmlObject1.DocumentElement.SelectSingleNode("PV1/PV1.3").InnerText.Split('^')[2];
                            }
                            catch
                            {
                                continue;
                            }

                            XmlElement root = xmlObject1.DocumentElement;
                            XmlNodeList listNodes = root.SelectNodes("/HL7Message/OBX");
                            foreach (XmlNode item in listNodes)
                            {
                                XmlNode codeStr = item.SelectSingleNode("OBX.3");//68060^MDC_ATTR_PT_HEIGHT^MDC
                                try
                                {
                                    string a = codeStr.InnerText.Split('^')[0];
                                }
                                catch
                                {
                                    continue;
                                }
                                //某项目的code
                                string code = codeStr.InnerText.Split('^')[0];//参数code
                                                                              //某项目的value
                                XmlNode valueStr = item.SelectSingleNode("OBX.5");//169.0
                                if (valueStr == null)
                                {
                                    continue;
                                }
                                string value = valueStr.InnerText.Trim();//参数值
                                log.Info("客户端数据" + code + "------" + "----" + value);
                                if (!"".Equals(value) && !"0".Equals(value) && !"".Equals(pId))
                                {
                                    Dictionary<string, string> result = formatConversion.equipmentCodeToDataCode(code);

                                    log.Info("客户端数据" + code + "------" + "----" + value);
                                    SysDeviceData m = new SysDeviceData();

                                    if (result.Count == 0)
                                    {
                                        m.dataCode = code;
                                        m.dataName = value;
                                    }
                                    else
                                    {
                                        m.dataCode = result["code"];
                                        m.dataName = result["codeName"];
                                    }

                                    m.deviceIp = host;

                                    m.deviceType = "1";
                                    m.createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                                    m.dataValue = value;
                                    m.remark = "";
                                    bll.InsertOne(m);
                                }
                            }
                        }



                    }
                    else
                    {
                        ShowMsg("服务端断开: ");
                        // 服务端关闭连接，可以尝试重新连接或显示错误消息  

                        Thread.Sleep(5000); // 等待5秒钟后重试连接  
                        khd(); // 递归调用重新连接方法  
                    }

                }
            }
            catch (SocketException ex)
            {
                ShowMsg("连接失败: " + ex.Message);
                // 服务端关闭连接，可以尝试重新连接或显示错误消息  
                // 这里使用重新连接的示例逻辑  
                Thread.Sleep(5000); // 等待5秒钟后重试连接  
                khd(); // 递归调用重新连接方法  
            }

        }



        private void TimerRespirator_ElapsedMR1B()
        {
            IPAddress ip = IPAddress.Parse("172.16.6.157");//中央站地址
            IPEndPoint endPoint = new IPEndPoint(ip, 8080);//中央站端口号
                                                           //ShowMsg("与" + endPoint + "连接中……");
            socketMonitorListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建socket连接
                                                                                                              //sockClient.ReceiveTimeout= 30000;
            var ar = socketMonitorListen.BeginConnect(endPoint, null, null);//开始连接
            ar.AsyncWaitHandle.WaitOne(500);//设置连接间隔

            if (socketMonitorListen.Connected)
            {
                //dict.Add(sockClient.RemoteEndPoint.ToString(), sockClient);
                ShowMsg(endPoint + "连接成功！");
                Thread threadClient = new Thread(RecMsg);
                threadClient.IsBackground = true;
                threadClient.Start(socketMonitorListen); //dictThread.Add(sockClient.RemoteEndPoint.ToString(), threadClient);
            }
            else
            {
                ShowMsg("与" + endPoint + "连接失败！！！");
            }
            //}
        }
        void RecMsg(object sokConnectionparn)
        {
            string host = ConfigurationManager.AppSettings["khdIp"];
            Socket sockClient = sokConnectionparn as Socket;
            string endPoint = sockClient.RemoteEndPoint.ToString();
            sockClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, true);
            sockClient.ReceiveTimeout = 60000;
            string dataTotal = "";

            MysqlBLL bll = new MysqlBLL();
            while (true)
            {
                // 定义一个2M的缓存区；
                byte[] arrMsgRec = new byte[1024 * 1024 * 2];
                // 将接受到的数据存入到输入  arrMsgRec中；
                int length = -1;
                try
                {
                    length = sockClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度；
                }
                catch (Exception e)
                {
                    sockClient.Close();
                    sockClient.Dispose();
                    //ShowMsg("endPoint + 异常：" + e.Message);
                    ShowMsg("连接关闭：" + e.Message);
                    // 从 通信套接字 集合中删除被中断连接的通信套接字；
                    //dict.Remove(sockClient.RemoteEndPoint.ToString());
                    // 从通信线程集合中删除被中断连接的通信线程对象；
                    //dictThread.Remove(sockClient.RemoteEndPoint.ToString());
                    break;
                }
                ShowMsg("length：" + length);
                if (length == 0)
                {
                    sockClient.Close();
                    sockClient.Dispose();
                    break;
                }
                else
                {
                    string strMsg = Encoding.UTF8.GetString(arrMsgRec, 0, length);
                    // dataTotal += strMsg;

                    dataTotal = strMsg;
                }
            }
            Console.WriteLine(dataTotal);
            log.Info("客户端数据" + dataTotal);
            if (!sockClient.Connected && dataTotal.Length > 0)
            {
                HL7ToXmlConverter htxc = new HL7ToXmlConverter();
                List<string> hl7List = htxc.formatGetHl7(dataTotal);
                foreach (string hlt in hl7List)
                {
                    string sHL7asXml = htxc.ConvertToXml(hlt);
                    XmlDocument xmlObject1 = htxc.ConvertToXmlObject(hlt);
                    string pId = "";
                    try
                    {
                        //床号
                        pId = xmlObject1.DocumentElement.SelectSingleNode("PV1/PV1.3").InnerText.Split('^')[2];
                    }
                    catch
                    {
                        continue;
                    }

                    XmlElement root = xmlObject1.DocumentElement;
                    XmlNodeList listNodes = root.SelectNodes("/HL7Message/OBX");
                    foreach (XmlNode item in listNodes)
                    {
                        XmlNode codeStr = item.SelectSingleNode("OBX.3");//68060^MDC_ATTR_PT_HEIGHT^MDC
                        try
                        {
                            string a = codeStr.InnerText.Split('^')[0];
                        }
                        catch
                        {
                            continue;
                        }
                        //某项目的code
                        string code = codeStr.InnerText.Split('^')[0];//参数code
                        //某项目的value
                        XmlNode valueStr = item.SelectSingleNode("OBX.5");//169.0
                        if (valueStr == null)
                        {
                            continue;
                        }
                        string value = valueStr.InnerText.Trim();//参数值
                        if (!"".Equals(value) && !"0".Equals(value) && !"".Equals(pId))
                        {
                            Dictionary<string, string> result = formatConversion.equipmentCodeToDataCode(code);
                            if (result.Count == 0)
                            {
                                continue;
                            }
                            ShowMsg(pId + "----" + result["code"] + "------" + result["codeName"] + "----" + value);
                            log.Info("客户端数据" + code + "------" + "----" + value);
                            SysDeviceData m = new SysDeviceData();

                            m.dataCode = result["code"];
                            m.deviceIp = host;
                            m.dataName = result["codeName"];
                            m.deviceType = "1";
                            m.createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                            m.dataValue = value;
                            m.remark = "";
                            bll.InsertOne(m);
                        }
                    }
                }
            }
        }
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            return returnBytes;
        }

        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="str"></param>
        void ShowMsg(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }


        //迈瑞监护仪N17 系列（服务端
        private void mrN()
        {
            MysqlBLL bll = new MysqlBLL();
            List<SysDevice> selectDeviceList = bll.selectDeviceList();
            //端口号（用来监听的）
            int port = 3506;
            string host = ConfigurationManager.AppSettings["kmRoomIp"];

            IPAddress ip = IPAddress.Parse(host);

            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);

            Thread worker = new Thread(() =>
            {
                //监护仪未启动需要判断
                while (true)
                {
                    if (ipadrlist.Contains(ip))
                    {
                        //提示套接字监听异常   
                        ShowMsg("监护仪启动成功" + ip + "端口" + port);
                        break;
                    }
                    else
                    {
                        //提示套接字监听异常   
                        ShowMsg("监护仪未启动" + ip);
                        Thread.Sleep(10000);
                        log.Error("监护仪未启动-10秒后重新连接");
                        //重新获取
                        ipadrlist = Dns.GetHostAddresses(name);
                    }
                };


                //将IP地址和端口号绑定到网络节点point上 
                IPEndPoint ipe = new IPEndPoint(ip, port);
                //定义一个套接字用于监听客户端发来的消息，包含三个参数（IP4寻址协议，流式连接，Tcp协议） 
                SocketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //监听绑定的网络节点 
                SocketWatch.Bind(ipe);
                //将套接字的监听队列长度限制为20 
                SocketWatch.Listen(20);
                //负责监听客户端的线程:创建一个监听线程 
                Thread threadwatch = new Thread(WatchConnecting);
                //将窗体线程设置为与后台同步，随着主线程结束而结束 
                threadwatch.IsBackground = true;
                //启动线程   
                threadwatch.Start();

            });
            worker.Start();

        }
        //监听客户端发来的请求 
        void WatchConnecting()
        {
            Socket connection = null;
            //持续不断监听客户端发来的请求   
            while (true)
            {
                try
                {
                    connection = SocketWatch.Accept();
                }
                catch (Exception ex)
                {
                    //提示套接字监听异常   
                    Console.WriteLine(ex.Message);
                    break;
                }
                //客户端网络结点号 
                string remoteEndPoint = connection.RemoteEndPoint.ToString();
                //添加客户端信息 
                ClientConnectionItems.Add(remoteEndPoint, connection);
                //显示与客户端连接情况
                log.Info("\r\n[客户端\"" + remoteEndPoint + "\"建立连接成功！ 客户端数量：" + ClientConnectionItems.Count + "]");

                //获取客户端的IP和端口号 
                IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;

                //让客户显示"连接成功的"的信息 
                string sendmsg = "[" + "本地IP：" + clientIP + " 本地端口：" + clientPort.ToString() + " 连接服务端成功！]";
                byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                connection.Send(arrSendMsg);

                //创建一个通信线程   
                Thread thread = new Thread(recvMaiRuiData);
                //设置为后台线程，随着主线程退出而退出 
                thread.IsBackground = true;
                //启动线程   
                thread.Start(connection);
            }
        }
        /// <summary>
        /// 接收客户端发来的信息，客户端套接字对象
        /// </summary>
        /// <param name="socketclientpara"></param>  
        void recvMaiRuiData(object socketclientpara)
        {
            MysqlBLL bll = new MysqlBLL();
            Socket sockClient = socketclientpara as Socket;
            string dataTotal = "";
            while (true)
            {
                //创建一个内存缓冲区，其大小为1024*1024字节 即1M   
                byte[] arrServerRecMsg = new byte[1024 * 1024 * 2];
                //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度  
                int length = 0;
                try
                {
                    length = sockClient.Receive(arrServerRecMsg); // 接收数据，并返回数据的长度；
                    string strMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                    dataTotal = strMsg;
                    //发送客户端数据
                    // ShowMsg("客户端" + sockClient.RemoteEndPoint);
                    //ShowMsg(strMsg);
                    if (strMsg.Length == 0)
                    {
                        log.Error("客户端" + sockClient.RemoteEndPoint + "已经中断连接！");
                        ClientConnectionItems.Remove(sockClient.RemoteEndPoint.ToString());
                        //提示套接字监听异常 
                        Console.WriteLine("\r\n[客户端\"" + sockClient.RemoteEndPoint + "\"已经中断连接！ 客户端数量：" + ClientConnectionItems.Count + "]");
                        //关闭之前accept出来的和客户端进行通信的套接字 
                        sockClient.Close();
                        break;
                    }
                    if (length > 0)
                    {
                        Console.WriteLine(dataTotal);
                        HL7ToXmlConverter htxc = new HL7ToXmlConverter();
                        List<string> hl7List = htxc.formatGetHl7(dataTotal);
                        foreach (string hlt in hl7List)
                        {
                            string sHL7asXml = htxc.ConvertToXml(hlt);
                            XmlDocument xmlObject1 = htxc.ConvertToXmlObject(hlt);

                            XmlElement root = xmlObject1.DocumentElement;
                            XmlNodeList listNodes = root.SelectNodes("/HL7Message/OBX");
                            foreach (XmlNode item in listNodes)
                            {




                                XmlNode codeStr = item.SelectSingleNode("OBX.3");//68060^MDC_ATTR_PT_HEIGHT^MDC
                                string code = codeStr.InnerText.Split('^')[0];//参数code
                                if ("147842 ^ MDC_ECG_HEART_RATE ^ MDC" == codeStr.InnerText.Split('^')[1])
                                {
                                    continue;
                                }
                                String codeStrA = item.SelectSingleNode("OBX.4").InnerText;//1.7.4.147842
                                if (codeStrA == "1.2.1.150344" || codeStrA == "1.2.2.150344")
                                {
                                    code = codeStrA;
                                }
                                Dictionary<string, string> result = formatConversion.equipmentCodeToDataCode(code);

                                XmlNode valueStr = item.SelectSingleNode("OBX.5");//169.0
                                string value = valueStr.InnerText.Trim();//参数值
                                SysDeviceData m = new SysDeviceData();
                                log.Info("数据" + code + "------" + "----" + value);

                                m.deviceIp = ((IPEndPoint)sockClient.RemoteEndPoint).Address.ToString();
                                if (result.Count == 0)
                                {
                                    m.dataCode = code;
                                    m.dataName = value;
                                }
                                else
                                {
                                    m.dataCode = result["code"];
                                    m.dataName = result["codeName"];
                                }
                                m.deviceType = "1";
                                m.createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                                try
                                {
                                    if (result["codeName"].Equals("T1") || result["codeName"].Equals("T2"))
                                    {

                                        if (double.Parse(value) > 70)
                                        {
                                            double res = (double)(Math.Round((decimal)((double.Parse(value) - 32) / 1.8), 2));
                                            m.dataValue = string.Format("{0:f1}", res);
                                        }
                                        else
                                        {
                                            m.dataValue = value;
                                        }

                                    }
                                    else
                                    {
                                        m.dataValue = value;
                                    }
                                }
                                catch
                                {
                                    m.dataValue = value;
                                }

                                m.remark = "";
                                bll.InsertOne(m);


                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    ClientConnectionItems.Remove(sockClient.RemoteEndPoint.ToString());
                    //提示套接字监听异常 
                    Console.WriteLine("\r\n[客户端\"" + sockClient.RemoteEndPoint + "\"已经中断连接！ 客户端数量：" + ClientConnectionItems.Count + "]");
                    //关闭之前accept出来的和客户端进行通信的套接字 
                    sockClient.Close();
                    break;
                }
            }
        }




        private void TimerRespirator_ElapsedXL(object sender, ElapsedEventArgs e)
        {
            string com = ConfigurationManager.AppSettings["xqycom"];
            serialPort = new SerialPort();
            serialPort.PortName = com;//华东医院COM9
            serialPort.DataBits = 8; //每个字节的标准数据位长度
            serialPort.StopBits = StopBits.One; //设置每个字节的标准停止位数
            serialPort.Parity = Parity.None; //设置奇偶校验检查协议
            serialPort.ReadTimeout = 13000; //单位毫秒
            serialPort.WriteTimeout = 13000; //单位毫秒
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.BaudRate = Int32.Parse("9600"); //串行波特率
            serialPort.DataReceived += new SerialDataReceivedEventHandler(CommDataReceivedDEG); //设置数据接收事件（监听）
            try
            {
                ShowMsg("与血气机连接中...." + com);
                serialPort.Open(); //打开串口
                xqyTimer.Stop();

            }
            catch
            {
                xqyTimer.Start();
            }
        }

        public void CommDataReceivedDEG(Object sender, SerialDataReceivedEventArgs e)
        {
            ShowMsg("与血气机连接中11111....");
            MysqlBLL md = new MysqlBLL();
            string dataTotal = "";
            string con1 = "06";

            while (true)
            {


                Thread.Sleep(1000);
                try
                {

                    serialPort.DiscardInBuffer();
                }
                catch
                {
                }
                try
                {
                    serialPort.Write(strToToHexByte(con1), 0, strToToHexByte(con1).Length);

                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    // break;
                }

                //Comm.BytesToRead中为要读入的字节长度
                int length = serialPort.BytesToRead;
                Byte[] readBuffer = new Byte[length];
                serialPort.Read(readBuffer, 0, length); //将数据读入缓存
                                                        //，自定义处理过程
                                                        //   string strMs处理readBuffer中的数据g = Encoding.UTF8.GetString(readBuffer, 0, length); //获取出入库产品编号



                string strMsg = Encoding.UTF8.GetString(readBuffer, 0, length).Replace(" ", "");


                //Console.WriteLine(strMsg);
                if (strMsg.Length > 3)
                {
                    ShowMsg(strMsg + "~~~~");
                    int index = strMsg.Length;
                    dataTotal += strMsg.Substring(2, index - 7);
                }
                else
                {
                    dataTotal += strMsg;

                }
                //   if (length > 1 && (dataTotal.Contains("R|22") || dataTotal.Contains("R|21") || dataTotal.Contains("R|20") || dataTotal.Contains("R|19") || dataTotal.Contains("R|18")))
                if (length > 1 && (dataTotal.Contains("R|29")))
                {
                    ShowMsg(dataTotal);
                    dataTotal = dataTotal.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "");
                    dataTotal = dataTotal.Replace("P|", "\n").Replace("O|", "\n").Replace("R|", "\n");
                    string[] str = dataTotal.Split('\n');
                    string reportTime = "";
                    long pid = 0;
                    string hospitalNumber = "";
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (i == 0)
                        {
                            string[] repot = str[i].Split('|');
                            //  reportTime = repot[13];
                            //  reportTime = reportTime.Substring(0, 4) + "-" + reportTime.Substring(4, 2) + "-" + reportTime.Substring(6, 2) + " " + reportTime.Substring(8, 2) + ":" + reportTime.Substring(10, 2) + ":" + reportTime.Substring(12, 2);
                        }
                        if (i == 1)
                        {
                            try
                            {
                                string hosNo = str[i].Split('|')[2];

                                hospitalNumber = hosNo;
                            }
                            catch (Exception)
                            {
                                ShowMsg("住院号解析失败：" + str[i]);
                            }

                        }



                        if (i == 2)
                        {
                            try
                            {
                                reportTime = str[i].Split('|')[6];
                                reportTime = reportTime.Substring(0, 4) + "-" + reportTime.Substring(4, 2) + "-" + reportTime.Substring(6, 2) + " " + reportTime.Substring(8, 2) + ":" + reportTime.Substring(10, 2) + ":" + reportTime.Substring(12, 2);
                                ShowMsg("时间---" + reportTime);
                            }
                            catch (Exception)
                            {
                                ShowMsg("时间解析失败：" + str[i]);
                            }

                        }




                        try
                        {
                            if (i > 2)
                            {
                                string vars = str[i].Split('|')[1].Replace("^^^", "");
                                string value = str[i].Split('|')[2];
                                string units = "";
                                try
                                {
                                    units = str[i].Split('|')[3];
                                }
                                catch (Exception)
                                {
                                    units = "";
                                }
                                ShowMsg("插入成" + vars);
                                if (vars != "Temp" && vars != "A-aDO2")
                                {
                                    ShowMsg("插入成功---------" + vars + "---------" + value.Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "") + "住院号" + hospitalNumber);
                                    SysDeviceData m = new SysDeviceData();
                                    m.dataCode = vars;
                                    m.deviceIp = hospitalNumber;
                                    m.dataName = vars;
                                    m.deviceType = "3";
                                    m.createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                                    if (reportTime.Length > 0)
                                    {
                                        m.createTime = reportTime;
                                    }
                                    else
                                    {
                                        m.createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                                    }
                                    m.dataValue = value.Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");
                                    m.remark = units.Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");
                                    bll.InsertOne(m);




                                    dataTotal = "";
                                }
                            }

                        }
                        catch (Exception e1)

                        {
                            log.Error("ErrorErrorErrorErrorErrorErrorErrorError " + e1.Message);
                        }






                    }
                }
            }
        }


        //麻醉机


        private void geB650()
        {


            List<SysDevice> selectDeviceList = bll.selectDeviceList();
            DSerialPort getInstance = DSerialPort.getInstance;

            log.Info("333333");
            try
            {
                getInstance.Open();
                log.Info("44444");
                if (getInstance.OSIsUnix())
                {
                    dataEvent = (EventHandler)Delegate.Combine(dataEvent, (EventHandler)delegate (object sender, EventArgs e)
                    {
                        ReadData(sender);
                    });
                }
                if (!getInstance.OSIsUnix())
                {
                    getInstance.DataReceived += p_DataReceived;
                    getInstance.ErrorReceived += SerialPortErrorReceived;
                }

                //数据间隔
                string text2 = "25";
                short num = 5;


                if (text2 != "")
                {
                    num = Convert.ToInt16(text2);
                }
                if (num < 5)
                {
                    num = 5;
                }

                int num2 = 1;


                short num3 = 1;


                getInstance.RequestTransfer(1, num, 9);
                getInstance.RequestTransfer(1, num, 8);
                getInstance.RequestTransfer(1, num, 7);
                byte[] array = new byte[8];
                CreateWaveformSet(num3, array);
                if (num3 != 0)
                {

                    getInstance.RequestMultipleWaveTransfer(array, 0, 9);
                    getInstance.RequestMultipleWaveTransfer(array, 0, 8);
                    getInstance.RequestMultipleWaveTransfer(array, 0, 7);
                }


                if (getInstance.OSIsUnix())
                {
                    do
                    {
                        if (getInstance.BytesToRead != 0)
                        {
                            dataEvent(getInstance, new EventArgs());
                        }
                    }
                    while (true);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening/writing to serial port :: " + ex.Message, "Error!");
                log.Error("Error opening/writing to serial port :: " + ex.Message);
            }
            finally
            {
                getInstance.StopTransfer();
                getInstance.StopwaveTransfer();
                getInstance.Close();
            }
        }

        private static void p_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ReadData(sender);
        }


        private static void SerialPortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;

            // 处理串口错误
            log.Error("发生串口----错误：" + e.EventType);

            // 尝试重新连接串口
            try
            {
                serialPort.Close();

                log.Info("已重新连接串口-----定时器就重新开始了");
            }
            catch (Exception ex)
            {
                log.Error("重新连接串口失败：" + ex.Message);
            }
        }




        public static void ReadData(object sender)
        {
            try
            {
                (sender as DSerialPort).ReadBuffer();
            }
            catch (TimeoutException)
            {
            }
        }

        public static void WaitForSeconds(int nsec)
        {
            DateTime t = DateTime.Now.AddSeconds(nsec);
            DateTime now;
            do
            {
                now = DateTime.Now;
            }
            while (t > now);
        }

        public static void CreateWaveformSet(int nWaveSetType, byte[] WaveTrtype)
        {
            switch (nWaveSetType)
            {
                case 0:
                    break;
                case 1:
                    WaveTrtype[0] = 1;
                    WaveTrtype[1] = 4;
                    WaveTrtype[2] = 5;
                    WaveTrtype[3] = 8;
                    WaveTrtype[4] = byte.MaxValue;
                    break;
                case 2:
                    WaveTrtype[0] = 1;
                    WaveTrtype[1] = 4;
                    WaveTrtype[2] = 8;
                    WaveTrtype[3] = 9;
                    WaveTrtype[4] = 15;
                    WaveTrtype[5] = byte.MaxValue;
                    break;
                case 3:
                    WaveTrtype[0] = 1;
                    WaveTrtype[1] = 8;
                    WaveTrtype[2] = 9;
                    WaveTrtype[3] = 15;
                    WaveTrtype[4] = 13;
                    WaveTrtype[5] = 23;
                    WaveTrtype[6] = 14;
                    WaveTrtype[7] = byte.MaxValue;
                    break;
                case 4:
                    WaveTrtype[0] = 1;
                    WaveTrtype[1] = 2;
                    WaveTrtype[2] = byte.MaxValue;
                    break;
                case 5:
                    WaveTrtype[0] = 18;
                    WaveTrtype[1] = 19;
                    WaveTrtype[2] = 20;
                    WaveTrtype[3] = 21;
                    WaveTrtype[4] = byte.MaxValue;
                    break;
                case 6:
                    WaveTrtype[0] = 1;
                    WaveTrtype[1] = 2;
                    WaveTrtype[2] = 3;
                    WaveTrtype[3] = byte.MaxValue;
                    break;
                default:
                    WaveTrtype[0] = 1;
                    WaveTrtype[1] = 4;
                    WaveTrtype[2] = 5;
                    WaveTrtype[3] = 8;
                    WaveTrtype[4] = byte.MaxValue;
                    break;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();

            WindowState = FormWindowState.Normal;

            Activate();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("您确定要退出程序吗？", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)

            {

                Environment.Exit(0);

            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)

            {

                e.Cancel = true;

                Hide();

            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)

            {

                Hide();

            }
        }

    }

}
