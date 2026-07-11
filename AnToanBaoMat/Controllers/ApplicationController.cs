using AnToanBaoMat.Data;
using AnToanBaoMat.Models;
using AnToanBaoMat.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace AnToanBaoMat.Controllers
{
    [RoleAuthorize("Candidate")]

    public class ApplicationController : Controller
    {
        private readonly AuditService _audit;
        private readonly JobRecruitmentDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ISecurityService _security;
        private readonly EncryptionService _encrypt;
        private readonly RSAService _rsa;
        private readonly DigitalSignatureService _signature;
        private readonly IntrusionDetectionService _ids;
        public ApplicationController(
            JobRecruitmentDbContext context,
            IWebHostEnvironment environment,
            ISecurityService security,
            EncryptionService encrypt,
            AuditService audit,
            RSAService rsa,
            DigitalSignatureService signature,
            IntrusionDetectionService ids)
        {
            _context = context;
            _environment = environment;
            _security = security;
            _encrypt = encrypt;
            _rsa = rsa;
            _audit = audit;
            _signature = signature;
            _ids = ids;
        }
        //=============================
        // Lịch sử gửi CV
        //=============================

        public IActionResult History()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var list = _context.Applications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.ApplyTime)
                .ToList();

            return View(list);
        }

        //=============================
        // Danh sách CV
        //=============================

        public IActionResult Index()
        {
            var list = _context.Applications
                               .OrderByDescending(x => x.ApplyTime)
                               .ToList();

            return RedirectToAction("Upload");
        }

        //=============================
        // Upload GET
        //=============================

        [HttpGet]
        public IActionResult Upload()
        {
            if (HttpContext.Session.GetString("Role") != "Candidate")
            {
                return Content("Chỉ ứng viên mới được gửi CV.");
            }
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            

            ViewBag.JobId = new SelectList(
                _context.Jobs,
                "JobId",
                "JobTitle");

            ViewBag.AES = "Chưa thực hiện";

            ViewBag.SHA256 = "Chưa tạo";

            ViewBag.RSA = "Chưa tạo";

            ViewBag.Signature = "Chưa ký";
            ViewBag.CurrentIP =
                HttpContext.Connection.RemoteIpAddress?.ToString();
            

            return View();
        }

        //=============================
        // Upload POST
        //=============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            Application model,
            IFormFile cvFile)
        {


            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.JobId = new SelectList(
                _context.Jobs,
                "JobId",
                "JobTitle");

            if (cvFile == null)
            {
                ViewBag.Error = "Vui lòng chọn CV.";

                return View(model);
            }
            
            string ip =
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            model.Ipaddress = ip;

            model.ApplyTime = DateTime.Now;

            model.UserId =
                HttpContext.Session.GetInt32("UserId")!.Value;
            //========================================
            // TẠO NONCE
            //========================================

            string nonce = Guid.NewGuid().ToString("N");
            //========================================
            // KIỂM TRA REPLAY ATTACK
            //========================================

            bool replay = _context.Applications
                .Any(x => x.Nonce == nonce);

            if (replay)
            {
                _ids.Detect(
                    model.UserId,
                    ip,
                    "Replay Attack",
                    "HIGH",
                    "Nonce bị sử dụng nhiều lần.");

                ViewBag.Error =
                    "Replay Attack được phát hiện.";

                return View(model);
            }
            string? sessionToken =
                HttpContext.Session.GetString("SessionToken");

            if (string.IsNullOrEmpty(sessionToken))
            {
                ViewBag.Error = "Phiên đăng nhập không hợp lệ.";

                return View(model);
            }
            bool valid = _context.Users.Any(x =>
                x.UserId == model.UserId &&
                x.SessionToken == sessionToken);

            if (!valid)
            {
                ViewBag.Error = "Session Token không hợp lệ.";

                return View(model);
            }
            bool existed =
                _context.Applications
                .Any(x => x.Nonce == nonce);

            if (existed)
            {
                ViewBag.Error = "Replay Attack detected.";

                return View(model);
            }
            HttpContext.Session.SetString(
                "SessionToken",
                sessionToken);
            
            //========================================
            // KIỂM TRA IP BỊ CHẶN
            //========================================

            if (_security.IsBlocked(ip))
            {
                ViewBag.Error = "Địa chỉ IP của bạn đã bị chặn.";

                return View(model);
            }

            //========================================
            // KIỂM TRA GIỚI HẠN GỬI CV
            // Tối đa 5 lần trong 10 phút
            //========================================

            if (_security.IsSpam(ip))
            {
                _security.BlockIP(
                    ip,
                    "Spam upload CV quá nhiều lần");

                ViewBag.Error = "Bạn đã gửi quá nhiều CV. IP đã bị tạm khóa.";

                _ids.Detect(
                    model.UserId,
                    ip,
                    "Spam Upload",
                    "MEDIUM",
                    "Upload quá nhiều trong thời gian ngắn.");
                return View(model);
            }

            //========================================
            // KIỂM TRA KÍCH THƯỚC FILE
            //========================================

            long maxSize = 5 * 1024 * 1024;

            if (cvFile.Length > maxSize)
            {
                ViewBag.Error = "CV không được vượt quá 5MB.";

                return View(model);
            }

            //========================================
            // KIỂM TRA PHẦN MỞ RỘNG
            //========================================

            string extension = Path.GetExtension(cvFile.FileName).ToLower();

            string[] allowExtension =
            {
                ".pdf",
                ".doc",
                ".docx"
            };

            if (!allowExtension.Contains(extension))
            {
                ViewBag.Error = "Chỉ hỗ trợ file PDF, DOC hoặc DOCX.";

                return View(model);
            }

            //========================================
            // KIỂM TRA MIME TYPE
            //========================================

            string[] allowMime =
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };

            if (!allowMime.Contains(cvFile.ContentType))
            {
                ViewBag.Error = "Định dạng file không hợp lệ.";

                return View(model);
            }

            //========================================
            // TẠO TÊN FILE MỚI
            //========================================

            string newFileName = Guid.NewGuid().ToString() + extension;

            //========================================
            // TẠO THƯ MỤC uploads/cv
            //========================================

            string folder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "cv");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //========================================
            // LƯU FILE
            //========================================

            string filePath = Path.Combine(folder, newFileName);

            //========================================
            // LƯU FILE
            //========================================

            _context.SaveChanges();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(stream);
            }

            //========================================
            // TÍNH SHA256
            //========================================

            string hashString;

            using (var stream = System.IO.File.OpenRead(filePath))
            {
                using var sha = SHA256.Create();

                byte[] hash = sha.ComputeHash(stream);

                hashString = Convert.ToHexString(hash);
            }
            //=============================
            // MÃ HÓA FILE
            //=============================

            string encryptedFile = Path.Combine(
                folder,
                Guid.NewGuid().ToString() + ".enc");

            var key = _rsa.GenerateKey();

            model.PublicKey = key.PublicKey;

            model.PrivateKey = key.PrivateKey;

            // Ký file gốc trước khi xóa
            byte[] originalBytes =
                System.IO.File.ReadAllBytes(filePath);

            model.Signature =
                _signature.Sign(
                    originalBytes,
                    model.PrivateKey);
            _encrypt.EncryptFile(filePath, encryptedFile);
            _audit.Write(
                HttpContext.Session.GetInt32("UserId"),
                "AES",
                "AES Encrypt File",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "SUCCESS");
            _audit.Write(
                model.UserId,
                "RSA",
                "Generate RSA Key",
                ip,
                "SUCCESS");
            _audit.Write(
                model.UserId,
                "SIGNATURE",
                "Digital Signature",
                ip,
                "SUCCESS");
            // Xóa file gốc
            System.IO.File.Delete(filePath);
            Console.WriteLine(model.Cvfile);    
            
            // Chỉ lưu file mã hóa
            model.Cvfile = Path.GetFileName(encryptedFile);
            model.OriginalFileName = cvFile.FileName;
            //========================================
            // CẬP NHẬT MODEL
            //========================================

            model.Status = "Đã gửi";
            // Link tải có hiệu lực 7 ngày
            model.ExpireTime = DateTime.Now.AddDays(7);
            model.FileHash = hashString;
            model.Nonce = nonce;

            // Các thuộc tính mới trong bảng Applications
            model.UploaderIp = ip;

            model.SessionToken = sessionToken;

            model.Nonce = nonce;

            model.FileHash = hashString;

            model.ExpireTime = DateTime.Now.AddDays(7);

            //========================================
            // BẮT ĐẦU TRANSACTION
            //========================================

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //----------------------------------------
                // LƯU APPLICATION
                //----------------------------------------
                Console.WriteLine(model.OriginalFileName);

                // Đã mã hóa bằng AES
                model.IsEncrypted = true;

                model.IsEncrypted = true;
                model.IsSigned = true;
                model.IsVerified = false;

                _context.Applications.Add(model);

                await _context.SaveChangesAsync();

                model.ExpireTime = DateTime.Now.AddDays(7);

                model.Nonce = Guid.NewGuid().ToString("N");

                model.SessionToken =
                    HttpContext.Session.GetString("SessionToken");

                model.UploaderIp =
                    HttpContext.Connection.RemoteIpAddress?.ToString();

                await _context.SaveChangesAsync();

                //----------------------------------------
                // GHI NHẬT KÝ UPLOAD
                //----------------------------------------

                _security.WriteLog(
                    model.UserId,
                    model.JobId,
                    newFileName,
                    ip,
                    "Thành công",
                    "Upload CV thành công.");
                //----------------------------------------
                // COMMIT
                //----------------------------------------

                await transaction.CommitAsync();

                TempData["Success"] = "Gửi CV thành công.";
                TempData["AES"] = "AES-256 CBC";

                TempData["SHA"] = hashString;

                TempData["RSA"] = "2048-bit";

                TempData["SIGN"] = "Generated";

                TempData["Original"] = cvFile.FileName;

                TempData["Encrypted"] = model.Cvfile;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //----------------------------------------
                // ROLLBACK
                //----------------------------------------

                await transaction.RollbackAsync();

                //----------------------------------------
                // GHI LOG THẤT BẠI
                //----------------------------------------

                _security.WriteLog(
                    model.UserId,
                    model.JobId,
                    newFileName,
                    ip,
                    "Thất bại",
                    ex.Message);

                ViewBag.Error = "Có lỗi xảy ra khi gửi CV.";
                _audit.Write(
                    model.UserId,
                    "UPLOAD",
                    "Upload CV",
                    ip,
                    "SUCCESS");
                _audit.Write(
                        model.UserId,
                        "UPLOAD",
                        "Upload thất bại",
                        ip,
                        "FAILED");

                return View(model);
            }
        }
        public IActionResult Encrypt(int id)
        {
            var application = _context.Applications
                .FirstOrDefault(x => x.ApplicationId == id);

            if (application == null)
            {
                return NotFound();
            }

            if (application.IsEncrypted == true)
            {
                TempData["Error"] = "File đã được mã hóa.";

                return RedirectToAction(nameof(Index));
            }

            string folder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "cv");

            string inputFile = Path.Combine(
                folder,
                application.Cvfile);

            if (!System.IO.File.Exists(inputFile))
            {
                TempData["Error"] = "Không tìm thấy file.";

                return RedirectToAction(nameof(Index));
            }

            string encryptedFile = inputFile + ".enc";

            _encrypt.EncryptFile(
                inputFile,
                encryptedFile);

            System.IO.File.Delete(inputFile);

            application.Cvfile =
                Path.GetFileName(encryptedFile);

            application.IsVerified = true;

            _context.SaveChanges();

            TempData["Success"] = "AES Encrypt thành công.";

            return RedirectToAction(nameof(Index));
        }

    }
}   