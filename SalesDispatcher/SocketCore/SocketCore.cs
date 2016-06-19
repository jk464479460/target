/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/7 11:08:02
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Help;
using Model;
using MsCore;
using Newtonsoft.Json;

namespace SocketCore
{
    public class DispatcherCore
    {
        private string LoacalIp;
        private int Port;
        private static Socket _socket;
        private const int BufferSize = 1024;
        private object _lock=new object();
        private const int DataLengthDefine = 5;
        private IDictionary<int,Socket> _salesClientDict=new Dictionary<int, Socket>(); 
        private static MsMq _msMq=new MsMq();

        public DispatcherCore(string ip,int port)
        {
            LoacalIp = ip;
            Port = port;
        }
        public void StartRun()
        {
            var ip = IPAddress.Parse(LoacalIp);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(ip, Port));
            _socket.Listen(50);
            Console.WriteLine("开始监听服务... ...");
            var thread = new Thread(ListenClientConnectWcs);
            thread.Start();
        }

        private string CreateSendJson(string json)
        {
            var str = json;
            var lenStr = str.Length.ToString();
            lenStr = string.Join("", lenStr.Reverse());
            for (var i = 0; i <= 6 - lenStr.Length; i++)
                lenStr += "0";
            lenStr = string.Join("", lenStr.Reverse());
            return lenStr;
        }
        /// <summary>
        /// 发送新订单
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="delivery"></param>
        public void SendMessage(int clientId, OrderDelivery delivery)
        {
            if (SendHeart(clientId))
            {
                Socket yes = null;
                lock (_lock)
                {
                    if (_salesClientDict.ContainsKey(clientId))
                        yes = _salesClientDict[clientId];
                }
                var str = JsonConvert.SerializeObject(delivery);
                try
                {
                    var head=CreateSendJson(str);
                    yes?.Send(Encoding.UTF8.GetBytes(head));
                    yes?.Send(Encoding.UTF8.GetBytes(str));
                }
                catch (Exception ex)
                {
                    _msMq.SendMessageToMq<OrderDelivery>(delivery.OrderId,delivery);//发送 msq
                }
            }
            else
            {
                //发送 msq
                _msMq.SendMessageToMq<OrderDelivery>(delivery.OrderId, delivery);//发送 msq
            }
        }
        /// <summary>
        /// 读取队列消息
        /// </summary>
        /// <returns></returns>
        public OrderDelivery GetMessageFromMq()
        {
            return _msMq.GetAllMessage<OrderDelivery>();
        } 
        //发送一次心跳
        private bool SendHeart(int clientId)
        {
            Socket  yes = null;
            lock (_lock)
            {
                if(_salesClientDict.ContainsKey(clientId))
                    yes = _salesClientDict[clientId];
            }
            if (yes==null)
            {
                return false;
            }
            else
            {
                var client = yes;
                var heart = new {Heart = true};
              
                var str = JsonConvert.SerializeObject(heart);
                try
                {
                    var head = CreateSendJson(str);
                    client.Send(Encoding.UTF8.GetBytes(head));
                    client.Send(Encoding.UTF8.GetBytes(str));
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                    return false;
                }
                return true;
            }
        }

        private void ListenClientConnectWcs()
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    var clientSocket = _socket.Accept();
                    Console.WriteLine($"新建连接 {clientSocket.RemoteEndPoint}");
                    var receiveThread = new Thread(ReceiveMessageWcs); 
                    receiveThread.Start(clientSocket);
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                }

            }
        }
        private void HandleRecev(Socket myClientSocket,ref StringBuilder sb,ref int? curDataLength)
        {
            if (curDataLength == null)
            {
                if (sb.Length < DataLengthDefine)
                    return;
            }
            if (curDataLength == null && sb.Length >= DataLengthDefine)
            {
                var dataBitStr = sb.ToString().Substring(0, DataLengthDefine);
                curDataLength = int.Parse(dataBitStr);
                sb = sb.Remove(0, DataLengthDefine);
                return;
            }

            if (curDataLength != null && sb.Length >= curDataLength.Value)
            {
                var dataBitStr = sb.ToString().Substring(0, curDataLength.Value);
                var jsonObj = JsonConvert.DeserializeObject<IDictionary<string,object>>(dataBitStr);
                ActionFunc(myClientSocket, jsonObj);
                sb = sb.Remove(0, curDataLength.Value);
                curDataLength = null;
                //sb.Clear();
            }
        }
        private void ReceiveMessageWcs(object clientSocket)
        {
            int? curDataLength = null;
            var sb=new StringBuilder();

            while (true)
            {
                try
                {
                    var myClientSocket = (Socket)clientSocket;
                    HandleRecev(myClientSocket, ref sb, ref curDataLength);
                    if (sb.Length > 0) continue;
                    var result = new byte[BufferSize];
                    try
                    {
                        var dataCount = myClientSocket.Receive(result);
                        Debug.Assert(dataCount < BufferSize, "接收数据大于缓冲区设置 " + dataCount + " " + Encoding.UTF8.GetString(result));
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error($"ReceiveMessageWcs {ex.Message}", new Exception(ex.Message));
                        return;
                    }
                    var receiveStr = Encoding.UTF8.GetString(result);
                    receiveStr = receiveStr.Replace("\0", string.Empty);
                    sb.Append(receiveStr);
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}", ex);
                }

            }
        }

      
        private  void ActionFunc(Socket socket, IDictionary<string,object> jsonObj)
        {
            try
            {
                if( jsonObj.ContainsKey("Heart"))
                {

                }
                else
                {
                    var obj =jsonObj["ClientId"].ToString();
                    RegisterClient(int.Parse(obj), socket);
                }
            }
            catch (Exception ex)
            {
                
            }
           
            //客户端使用这段代码

        }
        //远端注册至本地
        private void RegisterClient(int mark, Socket socket)
        {
            AddClient(socket,mark);
            Console.WriteLine($"注册客户端 {mark} {socket.RemoteEndPoint}");
        }

        private void AddClient(Socket socket,int mark)
        {
            lock (_lock)
            {
                _salesClientDict.Add(mark,socket);
            }
        }
    }

   /* public class OrderDelivery
    {
        public string OrderId { get; set; }
        public DateTime CreateTime { set; get; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int AreaId { get; set; }
        public List<OrderContent> Content { get; set; } 
    }

    public class OrderContent
    {
        public string StockCode { get; set; }
        public int BuyCnt { get; set; }
        public decimal CurPrice { get; set; }
    }*/
}
