/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/7 17:09:21
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Help;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinClient
{
    public class Client
    {
        private readonly string _serverIp;
        private readonly int _port;
        private readonly int _index;
        private static Socket _socket;
        private const int BufferSize = 1024;
        private const int DataLengthDefine = 5;

        public Client()
        {
            _serverIp = ConfigurationManager.AppSettings["LaneIp"];
            _port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            _index = int.Parse(ConfigurationManager.AppSettings["Index"]);
        }

        //保持连接状态
        public void AwayConnect()
        {
            while (true)
            {
                if (!SendHeart())
                {
                    Connect();
                    Send();
                }
                Thread.Sleep(10000);
            }

        }

        private void Connect()
        {
            var ip = IPAddress.Parse(_serverIp);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(new IPEndPoint(ip, _port)); //配置服务器IP与端口
            }
            catch
            {
                AppLogger.Error("连接失败");
            }
        }

        private void Send()
        {
            try
            {
                if (!SendHeart()) return;
                var sendObj = new SaleRegClient
                {
                    RegClient = true,
                    ClientId = _index
                };
                var str = JsonConvert.SerializeObject(sendObj);
                var lenStr = str.Length.ToString();
                lenStr = string.Join("", lenStr.Reverse());
                for (var i = 0; i <= 6 - lenStr.Length; i++)
                    lenStr += "0";

                lenStr = string.Join("", lenStr.Reverse());
                _socket.Send(Encoding.UTF8.GetBytes(lenStr));
                //Thread.Sleep(2000);
                _socket.Send(Encoding.UTF8.GetBytes(str));
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }
        }

        private bool SendHeart()
        {
            var heart = new HeartMessage { Heart = true };
            var str = JsonConvert.SerializeObject(heart);
            try
            {
                var lenStr = str.Length.ToString();
                lenStr = string.Join("", lenStr.Reverse());
                for (var i = 0; i <= 6 - lenStr.Length; i++)
                    lenStr += "0";
                lenStr = string.Join("", lenStr.Reverse());
                _socket.Send(Encoding.UTF8.GetBytes(lenStr));
                //Thread.Sleep(2000);
                _socket.Send(Encoding.UTF8.GetBytes(str));
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                return false;
            }
            return true;
        }

        private void HandleRecev(Socket myClientSocket, ref StringBuilder sb, ref int? curDataLength)
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
                //var jsonObj = (dynamic)JsonConvert.DeserializeObject(dataBitStr);
                ActionFunc(myClientSocket, dataBitStr);
                sb = sb.Remove(0, curDataLength.Value);
                curDataLength = null;
                //sb.Clear();
            }
        }
        public void ReceiveMessageWcs()
        {
            int? curDataLength = null;
            var sb = new StringBuilder();

            while (true)
            {
                try
                {
                    var myClientSocket = (Socket)_socket;
                    HandleRecev(myClientSocket, ref sb, ref curDataLength);
                    if (sb.Length > 0) continue;
                    var result = new byte[BufferSize];
                    try
                    {
                        var dataCount = myClientSocket.Receive(result);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error($"ReceiveMessageWcs {ex.Message}", new Exception(ex.Message));
                        continue;
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
        private void ActionFunc(Socket socket, string json)
        {
            //客户端使用这段代码
            try
            {
                var obj = (OrderDelivery)JsonConvert.DeserializeObject<OrderDelivery>(json);
                if (obj.Content == null) return;
                UpdateUi.Post(obj);
            }
            catch (Exception ex)
            {

            }

        }
    }



   

}
