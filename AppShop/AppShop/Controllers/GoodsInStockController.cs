using System.Web.Http;
using Bll;
using Newtonsoft.Json;
using QueryContract.cs;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class GoodsInStockController : ApiController
    {
        //入库
        [HttpPost]
        public string InGoods([FromBody]QueryGoodsStockIn goodsStockIn)
        {
            var result = new GoodsHanlder().GoodsStockIn(goodsStockIn);
            return JsonConvert.SerializeObject(result);
        }
    }
}