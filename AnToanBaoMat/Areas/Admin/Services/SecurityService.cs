using AnToanBaoMat.Data;
using AnToanBaoMat.Models;

namespace AnToanBaoMat.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly JobRecruitmentDbContext _context;

        public SecurityService(JobRecruitmentDbContext context)
        {
            _context = context;
        }
        public void WriteAudit(
            int? userId,
            string action,
            string description,
            string ip,
            string status)
        {
            AuditLog log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Description = description,
                Ipaddress = ip,
                Status = status,
                LogTime = DateTime.Now
            };

            _context.AuditLogs.Add(log);

            _context.SaveChanges();
        }
        public void WriteSecurityEvent(
            int? userId,
            string eventType,
            string description,
            string ip)
        {
            var log = new SecurityEvent
            {
                UserId = userId,
                EventType = eventType,
                Description = description,
                Ipaddress = ip,
                EventTime = DateTime.Now    
            };

            _context.SecurityEvents.Add(log);

            _context.SaveChanges();
        }
        public bool IsBlocked(string ip)
        {
            return _context.BlockedIps.Any(x =>
                x.Ipaddress == ip &&
                x.IsActive == true);
        }

        public bool IsSpam(string ip)
        {
            DateTime time = DateTime.Now.AddMinutes(-10);

            int count = _context.Applications.Count(x =>
                x.Ipaddress == ip &&
                x.ApplyTime >= time);

            return count >= 5;
        }

        public void BlockIP(string ip, string reason)
        {
            if (_context.BlockedIps.Any(x => x.Ipaddress == ip))
                return;

            _context.BlockedIps.Add(new BlockedIp
            {
                Ipaddress = ip,
                Reason = reason,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();
        }

        public void WriteLog(
            int userId,
            int jobId,
            string fileName,
            string ip,
            string status,
            string message)
        {
            _context.CvuploadLogs.Add(new CvuploadLog
            {
                UserId = userId,
                JobId = jobId,
                FileName = fileName,
                Ipaddress = ip,
                UploadTime = DateTime.Now,
                Status = status,
                Message = message
            });

            _context.SaveChanges();
        }
    }
}