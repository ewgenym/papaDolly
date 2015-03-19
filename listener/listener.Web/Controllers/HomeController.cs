using System.Web.Mvc;
using NetMQ;

namespace listener.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}