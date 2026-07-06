using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int JobId { get; set; }

    public int UserId { get; set; }

    public string? Cvfile { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? ApplyTime { get; set; }

    public string? Status { get; set; }

    public string? UploaderIp { get; set; }

    public string? SessionToken { get; set; }

    public string? Nonce { get; set; }

    public string? FileHash { get; set; }

    public DateTime? ExpireTime { get; set; }

    public string? EncryptedFileName { get; set; }

    public string? OriginalFileName { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
