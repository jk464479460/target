using System.Web.Http;
using Bll;
using Newtonsoft.Json;
using QueryContract.cs;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class PayController : ApiController
    {
        [HttpPost]
        public string InitPayInfo([FromBody]QueryPayGood query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new CartgoBll().GetPayGoodsList(query);
            return JsonConvert.SerializeObject(json);
        }
    }
    [AllowAnonymous]
    public class PhoneController : ApiController
    {
        [HttpPost]
        public string SendCheckCode([FromBody]QueryCheckCode query)
        {
            var json = new ValidateClient().SendCheckCode(query.PhoneNumber);
            return JsonConvert.SerializeObject(json);
        }
    }
}