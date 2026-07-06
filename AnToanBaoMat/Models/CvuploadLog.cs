using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class CvuploadLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? JobId { get; set; }

    public string? FileName { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? UploadTime { get; set; }

    public string? Status { get; set; }

    public string? Message { get; set; }

    public virtual Job? Job { get; set; }

    public virtual User? User { get; set; }
}
