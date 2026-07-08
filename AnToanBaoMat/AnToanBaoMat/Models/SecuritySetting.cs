using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class SecuritySetting
{
    public int Id { get; set; }

    public int MaxUpload { get; set; }

    public int TimeLimit { get; set; }

    public int MaxFileSize { get; set; }

    public bool? AllowPdf { get; set; }

    public bool? AllowDoc { get; set; }

    public bool? AllowDocx { get; set; }

    public bool? AutoBlock { get; set; }

    public DateTime? CreatedAt { get; set; }
}
