using AnToanBaoMat.Data;
using AnToanBaoMat.Models;

namespace AnToanBaoMat.Services
{
    public class AuditService
    {
        private readonly JobRecruitmentDbContext _context;

        public AuditService(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public void Write(
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
    }
}