using System.Configuration;
using System.Web.Mvc;

namespace listener.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Endpoint = ConfigurationManager.AppSettings["endpoint"];
            return View();
        }
    }
}