using System.Web.Http;
using Bll;
using Newtonsoft.Json;
using QueryContract.cs;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class GoodsOperController : ApiController
    {
        //搜索
        [HttpPost]
        public string GoodsSearch([FromBody]QuerySearchGoods goodsStockIn)
        {
            var result = new GoodsHanlder().SearchGoods(goodsStockIn);
            return JsonConvert.SerializeObject(result);
        }
    }
}