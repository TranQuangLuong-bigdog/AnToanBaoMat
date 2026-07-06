using AnToanBaoMat.Data;
using AnToanBaoMat.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    public class JobController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public JobController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        //=========================
        // Danh sách Job
        //=========================

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            return View(
                _context.Jobs
                .OrderByDescending(x => x.CreatedAt)
                .ToList());
        }

        //=========================
        // Thêm
        //=========================

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Job job)
        {
            if (!ModelState.IsValid)
                return View(job);

            job.CreatedAt = DateTime.Now;

            _context.Jobs.Add(job);

            _context.SaveChanges();

            TempData["Success"] = "Đã thêm Job.";

            return RedirectToAction(nameof(Index));
        }

        //=========================
        // Sửa
        //=========================

        public IActionResult Edit(int id)
        {
            var job = _context.Jobs.Find(id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        [HttpPost]
        public IActionResult Edit(Job job)
        {
            if (!ModelState.IsValid)
                return View(job);

            _context.Jobs.Update(job);

            _context.SaveChanges();

            TempData["Success"] = "Đã cập nhật.";

            return RedirectToAction(nameof(Index));
        }

        //=========================
        // Xóa
        //=========================

        public IActionResult Delete(int id)
        {
            var job = _context.Jobs.Find(id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            var job = _context.Jobs.Find(id);

            if (job != null)
            {
                _context.Jobs.Remove(job);

                _context.SaveChanges();
            }

            TempData["Success"] = "Đã xóa Job.";

            return RedirectToAction(nameof(Index));
        }
    }
}