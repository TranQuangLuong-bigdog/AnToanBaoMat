using AnToanBaoMat.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public AuditLogController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var list = _context.AuditLogs
                .OrderByDescending(x => x.LogTime)
                .ToList();

            return View(list);
        }
    }
}