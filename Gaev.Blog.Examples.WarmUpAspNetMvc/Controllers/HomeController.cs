using System.Web.Mvc;

namespace Gaev.Blog.Examples.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            InitialSlowdown.For("Index");
            return View();
        }

        public ActionResult About()
        {
            InitialSlowdown.For("About");
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            InitialSlowdown.For("Contact");
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}