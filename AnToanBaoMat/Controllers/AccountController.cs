using AnToanBaoMat.Data;
using AnToanBaoMat.Models;
using AnToanBaoMat.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnToanBaoMat.Controllers
{
    public class AccountController : Controller
    {
        private readonly JobRecruitmentDbContext _context;
        private readonly AuditService _audit;

        public AccountController(
            JobRecruitmentDbContext context,
            AuditService audit)
        {
            _context = context;
            _audit = audit;
        }
        

        //=========================
        // Đăng nhập
        //=========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Email == email &&
                    x.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View();
            }

            //=========================
            // Tạo Session Token
            //=========================

            string token = Guid.NewGuid().ToString();

            user.SessionToken = token;

            _context.SaveChanges();

            //=========================
            // Lưu Session
            //=========================

            HttpContext.Session.SetInt32("UserId", user.UserId);

            HttpContext.Session.SetString("UserName", user.FullName);

            HttpContext.Session.SetString("SessionToken", token);
            HttpContext.Session.SetString(
                "Role",
                user.Role ?? "Candidate");

            // Nếu có Role trong bảng Users
            if (!string.IsNullOrEmpty(user.Role))
            {
                HttpContext.Session.SetString("Role", user.Role);
            }
            _audit.Write(
                null,
                "LOGIN",
                "Sai tài khoản hoặc mật khẩu",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "FAILED");
            _audit.Write(
                user.UserId,
                "LOGIN",
                "Đăng nhập thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "SUCCESS");

            return RedirectToAction("Index", "Home");
        }

        //=========================
        // Đăng ký
        //=========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Users.Add(model);

            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công.";

            return RedirectToAction("Login");
        }

        //=========================
        // Đăng xuất
        //=========================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            _audit.Write(
                null,
                "LOGIN",
                "Sai tài khoản hoặc mật khẩu",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "FAILED");

            return RedirectToAction("Login");
        }
    }
}