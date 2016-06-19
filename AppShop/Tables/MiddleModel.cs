/****************************************************************
*   作者：wxy
*   CLR版本：4.0.30319.42000
*   创建时间：2016/4/20 12:17:19
*   2016
*   描述说明：
*
*   修改历史：
*
*
*****************************************************************/

using System;
using System.Collections.Generic;

namespace Tables
{
    [Serializable]
    public class GoodsDisplayInfo
    {
        /// <summary>
        /// 商品编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 商品名字
        /// </summary>
        public string GoodsName { get; set; }
        /// <summary>
        /// 库存数量
        /// </summary>
        public int StockNum { get; set; }
        /// <summary>
        /// 零售价
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 售出数量
        /// </summary>
        public int SaleCount { get; set; }
    }

    public class UserCartGoInfo
    {
        public string Ssid { get; set; }
        public string StockCode { get; set; }
        public string GoodsName { get; set; }
        public int BuyCnt { get; set; }
        public decimal CurPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UserCartGoAll
    {
        public IList<UserCartGoInfo> AllGoodsInCartgo { get; set; } 
        public decimal TotalPayment { get; set; }
    }

    public class PayGoodsList
    {
        /// <summary>
        /// 商品名字
        /// </summary>
        public string GoodsName { get; set; }
        /// <summary>
        /// 商品价格
        /// </summary>
        public string GoodsPrice { get; set; }

        public string GoodsCode { get; set; }
    }

    public class AreaCode
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AdeverModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string User { get; set; }
        public DateTime Time { get; set; }
        public int Count { get; set; }
        public string Content { get; set; }
    }

    public class RecommendUser
    {
        public string User { get; set; }
    }
}
