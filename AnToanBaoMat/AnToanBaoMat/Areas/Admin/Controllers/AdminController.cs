using AnToanBaoMat.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public AdminController(JobRecruitmentDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        public IActionResult Index()
        {
            ViewBag.TotalUser = _context.Users.Count();

            ViewBag.TotalJob = _context.Jobs.Count();

            ViewBag.TotalCV = _context.Applications.Count();

            ViewBag.TotalBlocked =
                _context.BlockedIps.Count(x => x.IsActive == true);

            ViewBag.TotalUploadLog = _context.CvuploadLogs.Count();

            return View();
        }
    }
}