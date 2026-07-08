using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Role { get; set; } = null!;

    public string? SessionToken { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CvuploadLog> CvuploadLogs { get; set; } = new List<CvuploadLog>();
}
