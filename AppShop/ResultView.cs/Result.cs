using System.Collections.Generic;
using Tables;

namespace ResultView.cs
{
    public class MyException
    {
        public bool Success { get; set; }
        public string Exmsg { get; set; }
    }
    /// <summary>
    /// 商品入库返回结果
    /// </summary>
    public class ResultStockIn
    {
        public MyException Exception { get; set; }
    }

    /// <summary>
    /// 商品查询
    /// </summary>
    public class ResultGoodsSearch
    {
        public MyException Exception { get; set; }
        public IList<GoodsDisplayInfo> GoodsList { get; set; }

        public int PageIndex { get; set; }
        public int PageTotal { get; set; }
    }

    public class ResultAddToCart
    {
        public MyException Exception { get; set; }
    }
    /// <summary>
    /// 用户的购物车
    /// </summary>
    public class ResultUserCartgo
    {
        public MyException Exception { get; set; }
        public UserCartGoAll CartGoAll { get; set; }
    }

    public class ResultRmUserCartgo
    {
        public MyException Exception { get; set; }
    }
    /// <summary>
    /// 返回结算
    /// </summary>
    public class ResultPayGoods
    {
        public MyException Exception { get; set; }
        
        public IList<PayGoodsList> GoodsList { get; set; }
        /// <summary>
        /// 应付价格
        /// </summary>
        public string PayFee { get; set; }
        /// <summary>
        /// 注册用户拉取电话信息
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 注册用户拉取配送地址信息
        /// </summary>
        public string Address { get; set; }
    }
    /// <summary>
    /// 提交订单
    /// </summary>
    public class ResultSubmitOrder
    {
        public MyException Exception { get; set; }
        public string OrderId { get; set; }
    }

    public class ResultLogin
    {
        public MyException Exception { get; set; }
    }
    public class ResultReg
    {
        public MyException Exception { get; set; }
    }

    public class ResultArea
    {
        public MyException Exception { get; set; }
        public IList<AreaCode> Areas { get; set; } 
    }

    public class ResultModifyAddress
    {
        public MyException Exception { get; set; }
        public string Address { get; set; }
    }

    public class ResultAddPostInfo
    {
        public MyException Exception { get; set; }
    }

    public class ResultAdverInfo
    {
        public MyException Exception { get; set; }
        public IList<AdeverModel> InfoList { get; set; }  
    }
    public class ResultRecommend
    {
        public MyException Exception { get; set; }
        public IList<RecommendUser> RecommendList { get; set; }
    }

}
