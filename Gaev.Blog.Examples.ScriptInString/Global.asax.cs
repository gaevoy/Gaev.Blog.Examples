using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gaev.Blog.Examples
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            RouteTable.Routes.MapRoute(
                name: "Default",
                url: "{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
            );
        }
    }
}