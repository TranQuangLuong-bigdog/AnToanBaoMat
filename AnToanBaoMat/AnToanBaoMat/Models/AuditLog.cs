using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public string? Description { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? LogTime { get; set; }

    public string? Status { get; set; }
}
