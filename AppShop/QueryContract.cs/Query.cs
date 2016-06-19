namespace QueryContract.cs
{
    public class QueryGoodsStockIn
    {
        /// <summary>
        /// 条码数据
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName { get; set; }
        /// <summary>
        /// 入库的数量
        /// </summary>
        public int StockInCnt { get; set; }
        /// <summary>
        /// 商品单价
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 商品售价
        /// </summary>
        public string SalePrice { get; set; }

        public string Discount { get; set; }
    }

    public class QuerySearchGoods
    {
        /// <summary>
        /// 货物名字
        /// </summary>
        public string GoodsName { get; set; }
        /// <summary>
        /// 页面索引
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 单次显示数量
        /// </summary>
        public int PageNum { get; set; }

        public int StockType { get; set; }
    }

    public class QueryAddCart
    {
        //编码
        public string Code { get; set; }
        //数量
        public int Count { get; set; }

        //客户标识
        public string Uid { get; set; }
    }
    /// <summary>
    /// 查询用户名下的购物车
    /// </summary>
    public class QueryUserCartGo
    {
        public string Ssid { get; set; }

    }
    public class QueryRmCartgo
    {
        public string Ssid { get; set; }
        public string Code { get; set; }
    }
    public class QueryAddBuyCnt
    {
        public string Ssid { get; set; }
        public string Code { get; set; }
        public string Cnt { get; set; }
    }
    /// <summary>
    /// 客户端编号
    /// </summary>
    public class QueryPayGood
    {
        public string Ssid { get; set; }
    }
    public class QueryCheckCode
    {
        public string PhoneNumber { get; set; }
    }

    public class QueryCreateOrder
    {
        public string Ssid { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string CheckCode { get; set; }
        public string AreaId { set; get; }
    }

    public class QueryUserLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Ssid { get; set; }
    }

    public class QueryUserReg
    {
        public string Ssid { get; set; }
        public string UName { get; set; }
        public string Paw { get; set; }
        public string Phone { get; set; }
        public string CheckCode { get; set; }
    }

    public class QueryUserModify
    {
        public string Ssid { get; set; }
        public string OldPass { get; set; }
        public string Paw { get; set; }
        public string Phone { get; set; }
    }

    public class QueryAdress
    {
        public string Ssid { get; set; }
        public string Address { get; set; }
    }

    public class QueryUserExists
    {
        public string Ssid { get; set; }
    }

    public class QueryAddPostInfo
    {
        public string Title { get; set; }
        public string PostText { get; set; } 
        public string Ssid { get; set; }
    }

    public class QueryAddClick
    {
        public string Id { get; set; }
    }

    public class QueryRecommendUser
    {
        public string Ssid { get; set; }
        public string User { get; set; }
        public string Phone { get; set; }
    }

    public class QueryGetRecommend
    {
        public string Ssid { get; set; }
       
    }
}
