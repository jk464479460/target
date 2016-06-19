using System.Web.Http;
using Bll;
using Newtonsoft.Json;
using QueryContract.cs;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class GoodsAddCartController : ApiController
    {
        //加入购物车
        [HttpPost]
        public string AddGoodsToCart([FromBody]QueryAddCart addCart)
        {
            var result=new GoodsHanlder().AddToCart(addCart);
            return JsonConvert.SerializeObject(result);
        }
    }
}