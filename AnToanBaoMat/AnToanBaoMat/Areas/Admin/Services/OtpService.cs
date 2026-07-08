using System.Security.Cryptography;

namespace AnToanBaoMat.Services;

public class OtpService
{
    public string GenerateOTP()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 999999)
            .ToString();
    }
}