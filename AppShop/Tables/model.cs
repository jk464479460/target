using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tables
{
    public class Tb_Goods
    {
        //自增ID
        [Key]
        public Int64 Id { get; set; }
        //条码
        public string Code { get; set; }
        //名称
        public string Name { get; set; }
        //最后进货时间
        public DateTime LastUpDateTime { get; set; }
        //入库数量
        public int Numbers { get; set; }
        //商品单价
        public decimal InnerPrice { get; set; }

        //售出数量
        public int SaleCount { get; set; }
        //是否下架
        public Int16 IsShow { get; set; }
    }

    public class Tb_GoodsInfo
    {
        [Key]
        public Int64 GoodsId { get; set; }
        public string SalePrice { get; set; }
        //折扣默认为1
        public decimal Discount { get; set; }
        //折扣最后时间
        public string DeadLine { get; set; }

        public int StockType { get; set; }
    }

    public class Tb_CartGo
    {

        public string UserId { get; set; }

        public string StockCode { get; set; }
        public int BuyCnt { get; set; }
        public decimal CurPrice { get; set; }
        public int TempUser { get; set; }
        public DateTime CreateTime { get; set; }

        [DefaultValue(0)]
        public int IsPay { get; set; }
        [DefaultValue(null)]
        public string OrderId { get; set; }
        [Key]
        public Int64 Id { get; set; }
    }

    public class Tb_UserInfo
    {
        [Key]
        public Int64 UserId { get; set; }
        [DefaultValue(null)]
        public string Address { get; set; }
        public string Phone1 { get; set; }
        [DefaultValue(0)]
        public Int16 AreaId { get; set; }
    }

    public class Tb_Order
    {
        public string OrderId { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreateTime { get; set; }
        public string Address { get; set; }
        public string UserId { get; set; }
        [Key]
        public Int64 Id { get; set; }

        public string OperUser { get; set; }
        public DateTime OperTime { get; set; }
        public Int16 IsSend { get; set; }
        public int AreaId { get; set; }
    }

    public class Tb_Order_Old
    {
        public string OrderId { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreateTime { get; set; }
        public string Address { get; set; }
        public string UserId { get; set; }
        [Key]
        public Int64 Id { get; set; }

        public string OperUser { get; set; }
        public DateTime OperTime { get; set; }
        public Int16 IsSend { get; set; }
        public int AreaId { get; set; }
    }

    public class Tb_User
    {
        [Key]
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public Int16 HumanType { get; set; }
        public Int16 IsPermit { get; set; }
        public string Pwd { get; set; }
    }

    public class Tb_SaleArea
    {
        [Key]
        public int Id { get; set; }
        public string Area { get; set; }
    }

    public class Tb_PostInfo
    {
        [Key]
        public Int64 Id { get; set; }
        public string Title { get; set; }
        public string PostText { get; set; }
        [DefaultValue(1)]
        public int ViewCount { get; set; }
        public DateTime CreateTime { get; set; }
        public string UserId { get; set; }
        [DefaultValue(1)]
        public Int16 IsShow { get; set; }
    }

    public class Tb_Recommend
    {
        [Key]
        public Int64 Id {get;set;}
        public string RecommendUser { get; set; }
        public string User { get; set; }
        public DateTime CDT{ get; set; }
}
    public class Test
    {
        public int T { get; set; }
    }
}
