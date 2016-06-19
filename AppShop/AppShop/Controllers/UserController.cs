using System.Web.Http;
using Bll;
using Newtonsoft.Json;
using QueryContract.cs;

namespace AppShop.Controllers
{
    [AllowAnonymous]
    public class UserController : ApiController
    {
        [HttpPost]
        public string Login([FromBody] QueryUserLogin query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new UserLogin().Login(query);
          
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class RegController : ApiController
    {
        [HttpPost]
        public string SendRegCode([FromBody] QueryCheckCode query)
        {
            var json = new ValidateClient().RegMessage(query.PhoneNumber);
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class RegAddController : ApiController
    {
        [HttpPost]
        public string Add([FromBody] QueryUserReg query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new UserLogin().Add(query);
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class UserSidController : ApiController
    {
        [HttpPost]
        public string IsReg([FromBody] QueryUserExists query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json=new UserLogin().IsReg(query);
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class UserAddressController : ApiController
    {
        [HttpPost]
        public string SaveAddress([FromBody] QueryAdress query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new UserLogin().SaveAddress(query);
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class UserModifyPassController : ApiController
    {
        [HttpPost]
        public string ModifyPass([FromBody]QueryUserModify query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new UserLogin().ModifyPass(query);
            return JsonConvert.SerializeObject(json);
        }
    }
    [AllowAnonymous]
    public class UserPostInfoController : ApiController
    {
        [HttpPost]
        public string AddPostInfo([FromBody]QueryAddPostInfo query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new UserPostInfo().AddPostInfo(query);
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class GetPostInfoUController : ApiController
    {
        [HttpPost]
        public string GetPostInfo()
        {
            var json = new UserPostInfo().GetPostInfo();
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class AddtPostViewController : ApiController
    {
        [HttpPost]
        public string AddClick([FromBody]QueryAddClick query)
        {
            var json=new UserPostInfo().AddClick(query);
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class RecommendUserController : ApiController
    {
        [HttpPost]
        public string RecommendUser([FromBody]QueryRecommendUser query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new UserRecommend().AddRecommend(query);
            return JsonConvert.SerializeObject(json);
        }
    }

    [AllowAnonymous]
    public class GetRecommendController : ApiController
    {
        [HttpPost]
        public string GetRecommendUser([FromBody]QueryGetRecommend query)
        {
            query.Ssid = query.Ssid.Substring(1, query.Ssid.Length - 2);
            var json = new UserRecommend().GetRecommendInfo(query);
            return JsonConvert.SerializeObject(json);
        }
    }
}