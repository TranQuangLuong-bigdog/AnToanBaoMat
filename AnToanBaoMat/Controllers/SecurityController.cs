using AnToanBaoMat.Data;
using AnToanBaoMat.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    public class SecurityController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public SecurityController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.TotalUpload = _context.Applications.Count();

            ViewBag.TotalBlocked =
                _context.BlockedIps.Count(x => x.IsActive == true);

            ViewBag.TotalLog =
                _context.CvuploadLogs.Count();

            ViewBag.TodayUpload =
                _context.Applications.Count(x =>
                    x.ApplyTime.Value.Date == DateTime.Today);

            ViewBag.SuccessUpload =
                _context.CvuploadLogs.Count(x =>
                    x.Status == "Thành công");

            ViewBag.FailUpload =
                _context.CvuploadLogs.Count(x =>
                    x.Status == "Thất bại");

            return View();
        }
        
        public IActionResult Attack()
        {
            var topIP = _context.Applications

                .GroupBy(x => x.Ipaddress)

                .Select(x => new
                {
                    IP = x.Key,

                    Total = x.Count()
                })

                .OrderByDescending(x => x.Total)

                .Take(10)

                .ToList();

            ViewBag.TopIP = topIP;

            return View();
        }
        public IActionResult Events()
        {
            var list = _context.SecurityEvents
                .OrderByDescending(x => x.EventTime)
                .ToList();

            return View(list);
        }
    }
}