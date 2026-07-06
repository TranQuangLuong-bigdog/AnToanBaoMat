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

        public DownloadController(
            JobRecruitmentDbContext context,
            IWebHostEnvironment environment,
            EncryptionService encrypt,
            ISecurityService security,
            AuditService audit)
        {
            _context = context;
            _environment = environment;
            _encrypt = encrypt;
            _security = security;
            _audit = audit;
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

            // Xóa file tạm
            System.IO.File.Delete(tempFile);

            // Ghi Download Log
            _context.DownloadLogs.Add(new DownloadLog
            {
                ApplicationId = application.ApplicationId,
                EmployerId = HttpContext.Session.GetInt32("UserId") ?? 0,
                DownloadTime = DateTime.Now,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Status = "Decrypt + Download"
            });

            _context.SaveChanges();

            Console.WriteLine(application.Cvfile);
            // Ghi Audit
            _audit.Write(
                HttpContext.Session.GetInt32("UserId"),
                "DECRYPT",
                "Giải mã và tải CV",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "SUCCESS");

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