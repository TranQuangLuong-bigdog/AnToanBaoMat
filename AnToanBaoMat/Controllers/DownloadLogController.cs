using AnToanBaoMat.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    public class DownloadLogController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public DownloadLogController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var list = _context.DownloadLogs
                .OrderByDescending(x => x.DownloadTime)
                .ToList();

            return View(list);
        }
    }
}