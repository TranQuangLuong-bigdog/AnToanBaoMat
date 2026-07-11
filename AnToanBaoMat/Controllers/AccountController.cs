using AnToanBaoMat.Data;
using AnToanBaoMat.Models;
using AnToanBaoMat.Services;
using Microsoft.AspNetCore.Mvc;
namespace AnToanBaoMat.Controllers
{
    public class AccountController : Controller
    {
        private readonly IntrusionDetectionService _ids;
        private readonly MailService _mail;
        private readonly JobRecruitmentDbContext _context;
        private readonly AuditService _audit;
        public AccountController(
            JobRecruitmentDbContext context,
            AuditService audit,
            IntrusionDetectionService ids,
            MailService mail)
        {
            _context = context;
            _audit = audit;
            _ids = ids;
            _mail = mail;
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
            string ip =
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            string device =
                Request.Headers["User-Agent"].ToString();
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Email == email &&
                    x.PasswordHash == password);
            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View();
            }
            bool suspicious = false;
            _context.SaveChanges();
            if (!string.IsNullOrEmpty(user.LastIp))
            {
                if (user.LastIp != ip)
                {
                    suspicious = true;
                }
            }
            if (!string.IsNullOrEmpty(user.LastDevice))
            {
                if (user.LastDevice != device)
                {
                    suspicious = true;
                }
            }
            if (suspicious)
            {
                _ids.Detect(
                    user.UserId,
                    ip,
                    "Account Hijacking",
                    "HIGH",
                    "Đăng nhập từ thiết bị mới"
                );

                _mail.SendEmail(
                    user.Email!,
                    "CV SAFE - Cảnh báo đăng nhập",

            $@"
            <h2>⚠ Cảnh báo bảo mật</h2>

            <p>Hệ thống phát hiện tài khoản của bạn đăng nhập từ thiết bị mới.</p>

            <b>IP:</b> {ip}<br>

            <b>Thiết bị:</b> {device}<br>

            <b>Thời gian:</b> {DateTime.Now}
            ");
            }
            //=========================
            // Cập nhật thông tin đăng nhập
            //=========================
            string token = Guid.NewGuid().ToString();
            user.SessionToken = token;
            // Cập nhật đăng nhập cuối
            user.LastIp = ip;
            user.LastDevice = device;
            user.LastLogin = DateTime.Now;
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
            if (user == null)
            {
                _audit.Write(
                    null,
                    "LOGIN",
                    "Sai tài khoản hoặc mật khẩu",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                    "FAILED");

                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";

                return View();
            }
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