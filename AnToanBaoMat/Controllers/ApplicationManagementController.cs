using AnToanBaoMat.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnToanBaoMat.Controllers
{
    public class ApplicationManagementController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public ApplicationManagementController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        //=========================
        // Danh sách CV
        //=========================

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var list = _context.Applications
                .Include(x => x.User)
                .Include(x => x.Job)
                .OrderByDescending(x => x.ApplyTime)
                .ToList();

            return View(list);
        }

        //=========================
        // Chi tiết
        //=========================

        public IActionResult Details(int id)
        {
            var application = _context.Applications
                .Include(x => x.User)
                .Include(x => x.Job)
                .FirstOrDefault(x => x.ApplicationId == id);

            if (application == null)
                return NotFound();

            return View(application);
        }

        //=========================
        // Xóa
        //=========================

        public IActionResult Delete(int id)
        {
            var application = _context.Applications
                .FirstOrDefault(x => x.ApplicationId == id);

            if (application == null)
                return NotFound();

            return View(application);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            var application = _context.Applications
                .FirstOrDefault(x => x.ApplicationId == id);

            if (application != null)
            {
                _context.Applications.Remove(application);

                _context.SaveChanges();
            }

            TempData["Success"] = "Đã xóa CV.";

            return RedirectToAction(nameof(Index));
        }
    }
}