using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class DownloadOtp
{
    public int Otpid { get; set; }

    public int UserId { get; set; }

    public int ApplicationId { get; set; }

    public string? Otpcode { get; set; }

    public string? Otptype { get; set; }

    public DateTime? CreatedTime { get; set; }

    public DateTime? ExpireTime { get; set; }

    public bool? IsUsed { get; set; }
}
