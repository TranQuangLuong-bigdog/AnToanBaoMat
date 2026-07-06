using System;
using System.Collections.Generic;

namespace AnToanBaoMat.Models;

public partial class Job
{
    public int JobId { get; set; }

    public string JobTitle { get; set; } = null!;

    public string? CompanyName { get; set; }

    public string? Location { get; set; }

    public decimal? Salary { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CvuploadLog> CvuploadLogs { get; set; } = new List<CvuploadLog>();
}
