using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class BlockedIp
{
    public int Id { get; set; }

    public string? Ipaddress { get; set; }

    public string? Reason { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
