namespace AnToanBaoMat.Services
{
    public interface ISecurityService
    {
        bool IsBlocked(string ip);

        bool IsSpam(string ip);

        void BlockIP(string ip, string reason);

        void WriteLog(
            int userId,
            int jobId,
            string fileName,
            string ip,
            string status,
            string message);
    }
}