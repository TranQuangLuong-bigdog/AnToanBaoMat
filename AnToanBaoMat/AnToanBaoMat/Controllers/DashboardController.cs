using AnToanBaoMat.Data;
using AnToanBaoMat.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    [RoleAuthorize("Admin")]    
    public class DashboardController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public DashboardController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            if(HttpContext.Session.GetInt32("UserId") == null)
{
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ExpiredCV =
                _context.Applications
                .Count(x => x.ExpireTime < DateTime.Now);
            ViewBag.User =
                HttpContext.Session.GetString("UserName");
            ViewBag.TotalEncrypt =
                _context.Applications
                .Count(x => x.Cvfile.EndsWith(".enc"));
            ViewBag.TotalUser =
                _context.Users.Count();

            ViewBag.TotalJob =
                _context.Jobs.Count();

            ViewBag.TotalApplication =
                _context.Applications.Count();

            ViewBag.TotalBlocked =
                _context.BlockedIps.Count(x => x.IsActive == true);

            ViewBag.TotalDownload = 
                _context.DownloadLogs.Count();

            ViewBag.TotalBlockedIP =
                _context.BlockedIps.Count();

            ViewBag.TodayUpload =
                _context.Applications.Count(x =>
                    x.ApplyTime != null &&
                    x.ApplyTime.Value.Date == DateTime.Today);
            ViewBag.LastCV =
                _context.Applications
                .OrderByDescending(x => x.ApplyTime)
                .Take(5)
                .ToList();

            ViewBag.ThisWeek =
                _context.Applications.Count(x =>
                    x.ApplyTime != null &&
                    x.ApplyTime >= DateTime.Today.AddDays(-7));

            ViewBag.ThisMonth =
                _context.Applications.Count(x =>
                    x.ApplyTime != null &&
                    x.ApplyTime.Value.Month == DateTime.Now.Month &&
                    x.ApplyTime.Value.Year == DateTime.Now.Year);

            ViewBag.TotalCV =
                _context.Applications.Count();
            ViewBag.FailedUpload =
                _context.CvuploadLogs.Count(x =>
                    x.Status == "Thất bại");
            ViewBag.RecentUpload =
                _context.CvuploadLogs
                .OrderByDescending(x => x.UploadTime)
                .Take(5)
                .ToList();
            ViewBag.SafeIP =
                _context.BlockedIps.Count(x => x.IsActive == false);

            ViewBag.DangerIP =
                _context.BlockedIps.Count(x => x.IsActive == true);

            ViewBag.TotalLog =
                _context.CvuploadLogs.Count();
            ViewBag.TotalExpired = _context.Applications
                .Count(x => x.ExpireTime < DateTime.Now);

            ViewBag.TotalAudit =
            _context.AuditLogs.Count();

            ViewBag.TotalEncrypt =
            _context.Applications
            .Count(x => x.FileHash != null);

            ViewBag.RecentAudit =
            _context.AuditLogs
            .OrderByDescending(x => x.LogTime)
            .Take(5)
            .ToList();

            ViewBag.RecentDownload =
            _context.DownloadLogs
            .OrderByDescending(x => x.DownloadTime)
            .Take(5)
            .ToList();
            return View();
        }
    }
}