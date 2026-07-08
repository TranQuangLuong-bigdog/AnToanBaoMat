using AnToanBaoMat.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnToanBaoMat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public DashboardController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalUser = _context.Users.Count();

            ViewBag.TotalJob = _context.Jobs.Count();

            ViewBag.TotalApplication = _context.Applications.Count();

            ViewBag.TotalBlocked =
                _context.BlockedIps.Count(x => x.IsActive == true);

            ViewBag.TodayUpload = _context.Applications.Count(x =>
                x.ApplyTime.HasValue &&
                x.ApplyTime.Value.Date == DateTime.Today);

            ViewBag.TodayBlocked = _context.BlockedIps.Count(x =>
                x.CreatedAt.HasValue &&
                x.CreatedAt.Value.Date == DateTime.Today);

            ViewBag.SuccessUpload = _context.CvuploadLogs
                .Count(x => x.Status == "Thành công");

            ViewBag.FailUpload = _context.CvuploadLogs
                .Count(x => x.Status == "Thất bại");

            return View();
        }
    }
}