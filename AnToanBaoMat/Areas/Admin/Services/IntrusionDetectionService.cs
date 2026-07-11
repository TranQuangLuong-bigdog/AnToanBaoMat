using AnToanBaoMat.Data;
using AnToanBaoMat.Models;

namespace AnToanBaoMat.Services
{
    public class IntrusionDetectionService
    {
        private readonly JobRecruitmentDbContext _context;

        public IntrusionDetectionService(JobRecruitmentDbContext context)
        {
            _context = context;
        }

        public void Detect(
            int? userId,
            string ip,
            string attackType,
            string severity,
            string description)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                UserId = userId,
                Ipaddress = ip,
                EventType = attackType,
                Severity = severity,
                Description = description,
                Status = "Blocked",
                Device = "Web Browser",
                EventTime = DateTime.Now
            });

            _context.SaveChanges();
        }
    }
}