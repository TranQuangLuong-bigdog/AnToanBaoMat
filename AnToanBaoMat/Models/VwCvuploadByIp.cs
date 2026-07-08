using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class VwCvuploadByIp
{
    public string? Ipaddress { get; set; }

    public int? TotalUpload { get; set; }

    public DateTime? LastUpload { get; set; }
}
