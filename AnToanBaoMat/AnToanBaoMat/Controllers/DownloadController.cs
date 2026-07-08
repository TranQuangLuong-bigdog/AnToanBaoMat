using AnToanBaoMat.Data;
using AnToanBaoMat.Models;
using AnToanBaoMat.Services;
using Microsoft.AspNetCore.Mvc;                                                                         
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


namespace AnToanBaoMat.Controllers
{
    [RoleAuthorize("Employer")]
    public class DownloadController : Controller
    {
        private readonly AuditService _audit;
        private readonly JobRecruitmentDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly EncryptionService _encrypt;
        private readonly ISecurityService _security;
        private readonly DigitalSignatureService _signature;
        private readonly IntrusionDetectionService _ids;
        private readonly OtpService _otp;
        private readonly MailService _mail;
        public DownloadController(
            JobRecruitmentDbContext context,
            IWebHostEnvironment environment,
            EncryptionService encrypt,
            ISecurityService security,
            AuditService audit,
            DigitalSignatureService signature,
            IntrusionDetectionService ids,
            OtpService otp,
            MailService mail)
        {
            _context = context;
            _environment = environment;
            _encrypt = encrypt;
            _security = security;
            _audit = audit;
            _signature = signature;
            _ids = ids;
            _otp = otp;
            _mail = mail;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Employer")
            {
                return RedirectToAction("Login", "Account");
            }

            var list = _context.Applications
                .Include(x => x.User)
                .Include(x => x.Job)
                .OrderByDescending(x => x.ApplyTime)
                .ToList();

            return View(list);
        }
        public IActionResult SendOTP(int id)
        {
            int userId =
                HttpContext.Session.GetInt32("UserId") ?? 0;

            var user =
                _context.Users.Find(userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            string otp =
                _otp.GenerateOTP();

            _context.DownloadOtps.Add(new DownloadOtp
            {
                UserId = userId,
                ApplicationId = id,

                Otpcode = otp,

                Otptype = "EMAIL",

                CreatedTime = DateTime.Now,

                ExpireTime = DateTime.Now.AddMinutes(5),

                IsUsed = false
            });
            _mail.SendEmail(
                user.Email!,
                "CV SAFE - Mã OTP xác thực",
                $@"
                <h2>CV SAFE</h2>

                <p>Xin chào {user.FullName},</p>

                <p>Bạn vừa yêu cầu tải CV.</p>

                <h1 style='color:red'>{otp}</h1>

                <p>Mã OTP có hiệu lực trong <b>5 phút</b>.</p>

                <p>Nếu không phải bạn thực hiện yêu cầu này, vui lòng đổi mật khẩu ngay.</p>
            ");

            _context.SaveChanges();


            return RedirectToAction("VerifyOTP", new { id = id });
        }
        [HttpGet]
        public IActionResult VerifyOTP(int id)
        {
            ViewBag.ApplicationId = id;

            return View();
        }
        [HttpPost]
        public IActionResult VerifyOTP(int applicationId, string otp)
        {
            int userId =
                HttpContext.Session.GetInt32("UserId") ?? 0;

            var code = _context.DownloadOtps
                .Where(x => x.UserId == userId &&
                            x.ApplicationId == applicationId &&
                            x.IsUsed == false)
                .OrderByDescending(x => x.CreatedTime)
                .FirstOrDefault();

            if (code == null)
            {
                TempData["Error"] = "Không có OTP.";
                ViewBag.ApplicationId = applicationId;
                return View();
            }

            if (code.ExpireTime < DateTime.Now)
            {
                TempData["Error"] = "OTP đã hết hạn.";
                ViewBag.ApplicationId = applicationId;
                return View();
            }

            if (code.Otpcode != otp)
            {
                TempData["Error"] = "OTP không đúng.";
                ViewBag.ApplicationId = applicationId;
                return View();
            }

            code.IsUsed = true;

            _context.SaveChanges();

            return RedirectToAction("Decrypt", new { id = applicationId });
        }
        public IActionResult Download(int id)
        {
            //=========================
            // Kiểm tra đăng nhập
            //=========================

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var application = _context.Applications
                .FirstOrDefault(x => x.ApplicationId == id);

            if (application == null)
            {
                return NotFound("Không tìm thấy CV.");
            }
            //=========================
            // Kiểm tra hết hạn
            //=========================

            if (application.ExpireTime == null)
            {
                return Content("CV chưa có thời hạn.");
            }

            if (application.ExpireTime <= DateTime.Now)
            {
                _audit.Write(
                    HttpContext.Session.GetInt32("UserId"),
                    "DOWNLOAD",
                    "CV hết hạn",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                    "FAILED");
                return Content("Liên kết tải CV đã hết hạn.");
            }

            //=========================
            // File mã hóa
            //=========================

            string encryptedFile = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "cv",
                application.Cvfile);

            if (!System.IO.File.Exists(encryptedFile))
            {
                return NotFound("Không tìm thấy file.");
            }

            //=========================
            // File tạm
            //=========================

            string extension =
                Path.GetExtension(application.OriginalFileName);

            string tempFile = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid() + extension);

            //=========================
            // Giải mã
            //=========================

            _encrypt.DecryptFile(
                encryptedFile,
                tempFile);

            //=========================
            // Kiểm tra SHA256
            //=========================

            string hashString;

            using (var stream = System.IO.File.OpenRead(tempFile))
            {
                using var sha = SHA256.Create();

                hashString = Convert.ToHexString(
                    sha.ComputeHash(stream));
            }

            if (hashString != application.FileHash)
            {
                System.IO.File.Delete(tempFile);
                _audit.Write(
                    HttpContext.Session.GetInt32("UserId"),
                    "DOWNLOAD",
                    "Hash không hợp lệ",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                    "FAILED");
                return Content("CV đã bị chỉnh sửa hoặc hỏng.");

            }

            //=========================
            // Ghi Download Log
            //=========================

            _context.DownloadLogs.Add(new DownloadLog
            {
                ApplicationId = application.ApplicationId,
                EmployerId = HttpContext.Session.GetInt32("UserId") ?? 0,
                DownloadTime = DateTime.Now,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Status = "Success"
            });
            
            _context.SaveChanges();

            //=========================
            // Đọc file
            //=========================

            byte[] fileBytes = System.IO.File.ReadAllBytes(tempFile);

            bool verify = _signature.Verify(
                fileBytes,
                application.Signature!,
                application.PublicKey!);

            if (!verify)
            {
                System.IO.File.Delete(tempFile);

                return Content("Digital Signature không hợp lệ.");
            }

            application.IsVerified = true;

            _context.SaveChanges();

            //=========================
            // Xóa file tạm
            //=========================

            System.IO.File.Delete(tempFile);

            //=========================
            // Download
            //=========================
            _audit.Write(
                HttpContext.Session.GetInt32("UserId"),
                "DOWNLOAD",
                "Download CV",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "SUCCESS");

            return File(
                fileBytes,
                GetContentType(application.OriginalFileName),
                application.OriginalFileName);
        }
        public IActionResult AES(int id)
        {
            var application = _context.Applications.Find(id);

            if (application == null)
                return NotFound();

            ViewBag.Algorithm = "AES-256 CBC";
            ViewBag.Status = "Decrypt Success";

            return View(application);
        }
        public IActionResult VerifyHash(int id)
        {
            var application = _context.Applications.Find(id);

            if (application == null)
                return NotFound();

            ViewBag.Hash = application.FileHash;
            ViewBag.Result = "✔ Hash hợp lệ";

            return View(application);
        }
        public IActionResult VerifySignature(int id)
        {
            var application = _context.Applications.Find(id);

            if (application == null)
                return NotFound();

            ViewBag.PublicKey = application.PublicKey;
            ViewBag.Signature = application.Signature;
            ViewBag.Result = "✔ Digital Signature hợp lệ";

            return View(application);
        }
        public IActionResult SecurityInfo(int id)
        {
            var app = _context.Applications
                .FirstOrDefault(x => x.ApplicationId == id);

            if (app == null)
                return Content("Không tìm thấy.");

            return PartialView("_SecurityInfo", app);
        }
        public IActionResult Decrypt(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employer")
            {
                return RedirectToAction("Login", "Account");
            }

            var application = _context.Applications
                .FirstOrDefault(x => x.ApplicationId == id);

            if (application == null)
            {
                return NotFound();
            }

            // Kiểm tra hết hạn
            if (application.ExpireTime <= DateTime.Now)
            {
                _ids.Detect(
                    HttpContext.Session.GetInt32("UserId"),
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                    "Expired CV",
                    "MEDIUM",
                    "Download CV hết hạn.");

                return Content("CV đã hết hạn.");
            }
            if (application == null)
            {
                return Content("Không tìm thấy Application.");
            }

            if (string.IsNullOrEmpty(application.Cvfile))
            {
                return Content("Cvfile đang NULL trong database.");
            }
            Console.WriteLine("WebRoot = " + _environment.WebRootPath);
            Console.WriteLine("CvFile = " + application.Cvfile);

            string webRoot = _environment.WebRootPath;

            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");
            }

            string encryptedFile = Path.Combine(
                webRoot,
                "uploads",
                "cv",
                application.Cvfile);

            if (!application.Cvfile.EndsWith(".enc"))
            {
                return Content("Đây không phải file đã mã hóa.");
            }

            if (!System.IO.File.Exists(encryptedFile))
            {
                return Content("Không tìm thấy file.");
            }

            // File tạm
            string extension =
                Path.GetExtension(application.OriginalFileName);

            string tempFile = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + extension);

            // Giải mã
            _encrypt.DecryptFile(encryptedFile, tempFile);

            // Kiểm tra SHA256
            using (var fs = System.IO.File.OpenRead(tempFile))
            using (var sha = SHA256.Create())
            {
                string hash = Convert.ToHexString(sha.ComputeHash(fs));

                if (hash != application.FileHash)
                {
                    System.IO.File.Delete(tempFile);

                    return Content("CV đã bị chỉnh sửa.");
                }
            }

            // Đọc file
            byte[] bytes = System.IO.File.ReadAllBytes(tempFile);
            bool verify = false;

            try
            {
                verify = _signature.Verify(
                    bytes,
                    application.Signature!,
                    application.PublicKey!);
            }
            catch
            {
                verify = false;
            }

            // Lưu trạng thái Verify
            application.IsVerified = verify;

            _context.SaveChanges();

            // Không chặn download
            if (!verify)
            {
                _ids.Detect(
                    HttpContext.Session.GetInt32("UserId"),
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                    "Digital Signature",
                    "HIGH",
                    "Verify thất bại khi Download.");

                TempData["Warning"] =
                    "Digital Signature không hợp lệ.";
            }
            // Xóa file tạm
            System.IO.File.Delete(tempFile);

            // Ghi Download Log
            _context.DownloadLogs.Add(new DownloadLog
            {
                ApplicationId = application.ApplicationId,
                EmployerId = HttpContext.Session.GetInt32("UserId") ?? 0,
                DownloadTime = DateTime.Now,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Status = verify
                    ? "Verified + Download"
                    : "Download (Verify Failed)"
            });

            _context.SaveChanges();

            Console.WriteLine(application.Cvfile);
            // Ghi Audit    
            _audit.Write(
                HttpContext.Session.GetInt32("UserId"),
                "DECRYPT",
                verify
                    ? "Giải mã + Verify thành công"
                    : "Giải mã nhưng Verify thất bại",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                verify ? "SUCCESS" : "WARNING");

            // Tên file tải về
            string downloadName = application.OriginalFileName;

            if (string.IsNullOrWhiteSpace(downloadName))
            {
                downloadName = "CV" + Path.GetExtension(application.Cvfile);
            }
            Console.WriteLine(application.Cvfile);

            Console.WriteLine(application.OriginalFileName);

            Console.WriteLine(downloadName);

            return File(
                bytes,
                GetContentType(application.OriginalFileName),
                application.OriginalFileName);
        }
        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";

                case ".doc":
                    return "application/msword";

                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                default:
                    return "application/octet-stream";
            }
        }
    }

}