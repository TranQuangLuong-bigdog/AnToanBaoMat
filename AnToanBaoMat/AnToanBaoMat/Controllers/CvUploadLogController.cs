using AnToanBaoMat.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnToanBaoMat.Controllers
{
    public class CvUploadLogController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public CvUploadLogController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        //=========================
        // Danh sách Log
        //=========================

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var logs = _context.CvuploadLogs
                .Include(x => x.User)
                .Include(x => x.Job)
                .OrderByDescending(x => x.UploadTime)
                .ToList();

            return View(logs);
        }

        //=========================
        // Chi tiết
        //=========================

        public IActionResult Detail(int id)
        {
            var log = _context.CvuploadLogs
                .Include(x => x.User)
                .Include(x => x.Job)
                .FirstOrDefault(x => x.Id == id);

            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

    }
}