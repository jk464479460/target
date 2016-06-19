/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/5/7 10:38:07
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
using System.Threading;
using Help;
using Model;
using SalesDispatcher.Dal;
using SocketCore;

namespace SalesDispatcher.Bll
{
    //订单分发
    public class OrderDispatcher
    {
        private readonly IOperDal _operDal=new OperDal();
        private static DispatcherCore _dispatcherCore;

        public OrderDispatcher()
        {
            var ip = ConfigurationManager.AppSettings["LaneIp"];
            var prot = ConfigurationManager.AppSettings["Port"];
            _dispatcherCore=new DispatcherCore(ip,int.Parse(prot));
        }

        //启动监控
        public void StartDispatcher()
        {
            try
            {
                _dispatcherCore.StartRun();
            }
            catch (Exception ex)
            {
                AppLogger.Error($"{ex.Message} {ex.StackTrace}");
            }
           
        }
        //启动分发
        public void OrderDelivery()
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    //var orderAreaList = _operDal.GetDeliveryAreas();
                    var orderNotSend = _operDal.GeTbOrdersNotSend();
                    //根据area定义的名字，发送到对一个的socket
                    //发送后，将数据库IsSend状态修改为1
                    foreach (var item in orderNotSend.GroupBy(x => x.OrderId))
                    {
                        var orderId = item.Key;
                        var list = new OrderDelivery {Content=new List<OrderContent>()};
                        var last = new OrderDeliveryModel();
                        foreach (var p in orderNotSend.Where(x => x.OrderId.Equals(orderId)))
                        {
                            last = p;
                            list.Address = p.Address;
                            list.CreateTime = p.CreateTime;
                            list.OrderId = orderId;
                            list.PhoneNumber = p.PhoneNumber;
                            list.Content.Add(new OrderContent { BuyCnt = p.BuyCnt, CurPrice = p.CurPrice, StockCode = p.StockCode,Name=p.Name });
                        }
                        list.Address = last.Address;
                        list.CreateTime = last.CreateTime;
                        list.PhoneNumber = last.PhoneNumber;
                        list.OrderId = orderId;
                        list.AreaId = last.AreaId;
                        _dispatcherCore.SendMessage(last.AreaId, list);
                        _operDal.UpdateTbOrdersSend(orderId);
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}", ex);
                }
            }
        }

        //处理积累队列
        public void HandleMq()
        {
            while (true)
            {
                try
                {
                    var mess = _dispatcherCore.GetMessageFromMq();
                    _dispatcherCore.SendMessage(mess.AreaId, mess);
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}");
                }
            }
        }
    }
}
