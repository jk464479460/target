using System.Web.Http;
using Owin;

namespace OwinExample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            #region WebApi
            var httpConfig = new HttpConfiguration();
            httpConfig.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            //强制设定目前的WebApi返回格式为json
            httpConfig.Formatters.Remove(httpConfig.Formatters.XmlFormatter);
            //加载WebApi中间件
            app.UseWebApi(httpConfig);
            #endregion
        }
    }
}