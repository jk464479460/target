using System;
using System.Collections.Generic;

namespace Model
{
    public class OrderDelivery
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
        public string Name { get; set; }
    }

    public class HeartMessage
    {
        public bool Heart { get; set; }
    }
    public class SaleRegClient
    {
        public int ClientId { get; set; }
        public bool RegClient { get; set; }
    }
}
