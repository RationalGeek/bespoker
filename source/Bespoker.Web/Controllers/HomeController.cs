using Bespoker.Web.Models;
using System.Web.Mvc;

namespace Bespoker.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PokerSession(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpNotFoundResult();

            var model = new PokerSessionViewModel { Name = id };
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}