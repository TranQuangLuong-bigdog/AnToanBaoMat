using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class DownloadLog
{
    public int LogId { get; set; }

    public int? ApplicationId { get; set; }

    public int? EmployerId { get; set; }

    public DateTime? DownloadTime { get; set; }

    public string? Ip { get; set; }

    public string? Status { get; set; }
}
