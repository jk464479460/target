using System.Web.Http;
using Bll;
using Newtonsoft.Json;
using QueryContract.cs;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class CartGoShowController : ApiController
    {
        [HttpPost]
        public string InitShowCart([FromBody]QueryUserCartGo query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new CartgoBll().ShowUserCartgo(query);
            return JsonConvert.SerializeObject(json);
        }
    }
    [AllowAnonymous]
    public class RmCartgoController : ApiController
    {
        [HttpPost]
        public string RmCartgo([FromBody]QueryRmCartgo query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new CartgoBll().RmUserCartgo(query);
            return JsonConvert.SerializeObject(json);
        }
    }
    [AllowAnonymous]
    public class CartgoAddBuyCntController : ApiController
    {
        [HttpPost]
        public string AddBuyCnt([FromBody]QueryAddBuyCnt query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new CartgoBll().AddBuyCnt(query);
            return JsonConvert.SerializeObject(json);
        }
    }
    [AllowAnonymous]
    public class SubmitOrderController : ApiController
    {
        [HttpPost]
        public string Submit([FromBody]QueryCreateOrder query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new OrderBll().SubimtOrder(query);
            return JsonConvert.SerializeObject(json);
        }
    }
}