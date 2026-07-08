using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}