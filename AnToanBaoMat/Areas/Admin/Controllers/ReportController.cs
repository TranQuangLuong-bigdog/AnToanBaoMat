using AnToanBaoMat.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnToanBaoMat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public ReportController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalUpload = _context.Applications.Count();

            ViewBag.TotalUser = _context.Users.Count();

            ViewBag.TotalJob = _context.Jobs.Count();

            ViewBag.TotalBlocked =
                _context.BlockedIps.Count(x => x.IsActive == true);

            // Top 10 IP gửi CV nhiều nhất
            ViewBag.TopIP = _context.Applications
                .GroupBy(x => x.Ipaddress)
                .Select(x => new
                {
                    IP = x.Key,
                    Total = x.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            // Top Job được ứng tuyển nhiều
            ViewBag.TopJob = _context.Applications
                .Include(x => x.Job)
                .GroupBy(x => x.Job.JobTitle)
                .Select(x => new
                {
                    Job = x.Key,
                    Total = x.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            return View();
        }
    }
}