using System;
using System.Web.Http;
using Bll;
using Dal;
using Newtonsoft.Json;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class CookieController : ApiController
    {
        private readonly IRedisOper _redisOper = new RedisOper(new ReadCfg().GetRedisCfg());
        [HttpPost]
        public string ApplyGruid()
        {
            var guid = Guid.NewGuid().ToString();
            guid = guid.Replace("-", "");
            _redisOper.Set(guid,$"1999_{DateTime.Now}");//1999代表session
           
            var res = new EncryDecry().Md5Encrypt(guid);
            return JsonConvert.SerializeObject(res);
        }
    }

  
}