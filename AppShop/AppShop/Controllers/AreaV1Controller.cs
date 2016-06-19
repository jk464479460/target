using System.Web.Http;
using Bll;
using Newtonsoft.Json;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class AreaV1Controller : ApiController
    {
        [HttpPost]
        public string GetArea()
        {
            var json = new AreaBll().GetAllArea();
            return JsonConvert.SerializeObject(json);
        }
    }
}
