using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class SecurityEvent
{
    public int EventId { get; set; }

    public int? UserId { get; set; }

    public string? EventType { get; set; }

    public string? Description { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? EventTime { get; set; }
}
