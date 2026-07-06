using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}