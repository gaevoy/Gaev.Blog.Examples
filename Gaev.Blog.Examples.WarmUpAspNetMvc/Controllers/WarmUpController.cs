using System;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Gaev.Blog.Examples.Controllers
{
    [AllowAnonymous]
    public class WarmUpController : Controller
    {
        private static bool _isWarm;

        public async Task Index()
        {
            if (_isWarm)
                return;
            await Task.WhenAll(
                WarmUp(html => html.RenderAction("Index", "Home")),
                WarmUp(html => html.RenderAction("About", "Home")),
                WarmUp(html => html.RenderAction("Contact", "Home"))
            );
            _isWarm = true;
        }
        // https://serverfault.com/questions/590865/how-can-i-warm-up-my-asp-net-mvc-webapp-after-an-app-pool-recycle

        private Task WarmUp(Action<HtmlHelper> act)
        {
            return Task.Factory.StartNew(() =>
            {
                var httpContext = NewHttpContext(Request.Url.AbsoluteUri, new WarmUpUser());
                System.Web.HttpContext.Current = httpContext;
                var htmlHelper = CreateHtmlHelper(httpContext);
                act(htmlHelper);
            }, TaskCreationOptions.LongRunning);
        }

        private class WarmUpUser : IPrincipal, IIdentity
        {
            public bool IsInRole(string role) => true;
            public IIdentity Identity => this;
            public string Name { get; } = "Warm-up user";
            public string AuthenticationType { get; } = "";
            public bool IsAuthenticated { get; } = true;
        }

        private static HttpContext NewHttpContext(string requestUrl, IPrincipal currentUser)
        {
            var request = new HttpRequest("", requestUrl, "");
            var response = new HttpResponse(TextWriter.Null);
            return new HttpContext(request, response) {User = currentUser};
        }

        private static HtmlHelper CreateHtmlHelper(HttpContext httpContext)
        {
            var controller = new NullController();
            var requestContext = new RequestContext(
                new HttpContextWrapper(httpContext),
                new RouteData());
            var controllerContext = new ControllerContext(requestContext, controller);
            controller.ControllerContext = controllerContext;
            var viewContext = new ViewContext(
                controllerContext,
                new NullView(),
                new ViewDataDictionary(),
                new TempDataDictionary(),
                TextWriter.Null);
            return new HtmlHelper(viewContext, new ViewPage());
        }

        private class NullController : ControllerBase
        {
            protected override void ExecuteCore() => throw new NotImplementedException();
        }

        private class NullView : IView
        {
            public void Render(ViewContext _, TextWriter __) => throw new NotImplementedException();
        }
    }
}