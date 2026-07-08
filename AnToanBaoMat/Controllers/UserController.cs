using AnToanBaoMat.Data;
using AnToanBaoMat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnToanBaoMat.Controllers
{
    public class UserController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public UserController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            return View(_context.Users.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);

            _context.SaveChanges();

            TempData["Success"] = "Thêm người dùng thành công.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            _context.Users.Update(user);

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thành công.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
                _context.Users.Remove(user);

                _context.SaveChanges();
            }

            TempData["Success"] = "Đã xóa.";

            return RedirectToAction(nameof(Index));
        }
    }
}