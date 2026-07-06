using AnToanBaoMat.Data;
using AnToanBaoMat.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    public class BlockedIpController : Controller
    {
        private readonly JobRecruitmentDbContext _context;

        public BlockedIpController(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        //==============================
        // Danh sách IP
        //==============================

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var list = _context.BlockedIps
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(list);
        }


        //==============================
        // GET: Thêm IP
        //==============================

        public IActionResult Create()
        {
            return View();
        }

        //==============================
        // POST: Thêm IP
        //==============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BlockedIp model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool exists = _context.BlockedIps
                .Any(x => x.Ipaddress == model.Ipaddress);

            if (exists)
            {
                ViewBag.Error = "IP đã tồn tại.";
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            model.IsActive = true;

            _context.BlockedIps.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Đã thêm IP thành công.";

            return RedirectToAction(nameof(Index));
        }
        //==============================
        // GET: Edit
        //==============================

        public IActionResult Edit(int id)
        {
            var ip = _context.BlockedIps.Find(id);

            if (ip == null)
            {
                return NotFound();
            }

            return View(ip);
        }
        //==============================
        // POST: Edit
        //==============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BlockedIp model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var data = _context.BlockedIps.Find(model.Id);

            if (data == null)
                return NotFound();

            data.Ipaddress = model.Ipaddress;
            data.Reason = model.Reason;
            data.IsActive = model.IsActive;

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thành công.";

            return RedirectToAction(nameof(Index));
        }
        //==============================
        // Delete
        //==============================

        public IActionResult Delete(int id)
        {
            var ip = _context.BlockedIps.Find(id);

            if (ip == null)
                return NotFound();

            _context.BlockedIps.Remove(ip);

            _context.SaveChanges();

            TempData["Success"] = "Đã xóa.";

            return RedirectToAction(nameof(Index));
        }
        //==============================
        // Lock / Unlock
        //==============================

        public IActionResult Toggle(int id)
        {
            var ip = _context.BlockedIps.Find(id);

            if (ip == null)
                return NotFound();

            ip.IsActive = !ip.IsActive;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}