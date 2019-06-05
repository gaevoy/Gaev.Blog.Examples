using System.Web.Mvc;

namespace Gaev.Blog.Examples.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => View("Problem1");
        public ActionResult Problem1() => View();
        public ActionResult Problem2() => View();
        public ActionResult Fix1() => View();
        public ActionResult Fix2() => View();
    }
}