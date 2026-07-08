using System;
using System.Collections.Generic;
using AnToanBaoMat.Models;
using Microsoft.EntityFrameworkCore;

namespace AnToanBaoMat.Data;

public partial class JobRecruitmentDbContext : DbContext
{
    public JobRecruitmentDbContext()
    {
    }

    public JobRecruitmentDbContext(DbContextOptions<JobRecruitmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BlockedIp> BlockedIps { get; set; }

    public virtual DbSet<CvuploadLog> CvuploadLogs { get; set; }

    public virtual DbSet<DownloadLog> DownloadLogs { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<SecurityEvent> SecurityEvents { get; set; }

    public virtual DbSet<SecuritySetting> SecuritySettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwCvuploadByIp> VwCvuploadByIps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=JobRecruitmentDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Applicat__C93A4C99D1CC9329");

            entity.Property(e => e.ApplyTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Cvfile)
                .HasMaxLength(255)
                .HasColumnName("CVFile");
            entity.Property(e => e.EncryptedFileName).HasMaxLength(255);
            entity.Property(e => e.ExpireTime).HasColumnType("datetime");
            entity.Property(e => e.FileHash).HasMaxLength(200);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.Nonce).HasMaxLength(100);
            entity.Property(e => e.OriginalFileName).HasMaxLength(255);
            entity.Property(e => e.SessionToken).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Đã gửi");
            entity.Property(e => e.UploaderIp)
                .HasMaxLength(50)
                .HasColumnName("UploaderIP");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Application_Job");

            entity.HasOne(d => d.User).WithMany(p => p.Applications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Application_User");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBD732880E5");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LogTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<BlockedIp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlockedI__3214EC0717CEFB26");

            entity.ToTable("BlockedIPs");

            entity.HasIndex(e => e.Ipaddress, "UQ__BlockedI__F0C25BE0D4A6F77A").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Reason).HasMaxLength(255);
        });

        modelBuilder.Entity<CvuploadLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CVUpload__3214EC07AC463FD0");

            entity.ToTable("CVUploadLogs");

            entity.HasIndex(e => e.Ipaddress, "IX_CV_IP");

            entity.HasIndex(e => e.UploadTime, "IX_CV_TIME");

            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UploadTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.CvuploadLogs)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__CVUploadL__JobId__4AB81AF0");

            entity.HasOne(d => d.User).WithMany(p => p.CvuploadLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CVUploadL__UserI__49C3F6B7");
        });

        modelBuilder.Entity<DownloadLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Download__5E548648E796AE56");

            entity.Property(e => e.DownloadTime).HasColumnType("datetime");
            entity.Property(e => e.Ip)
                .HasMaxLength(50)
                .HasColumnName("IP");
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Jobs__056690C28D477FCE");

            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.JobTitle).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Salary).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<SecurityEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Security__7944C810356B41BD");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EventTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
        });

        modelBuilder.Entity<SecuritySetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Security__3214EC072D590EF5");

            entity.Property(e => e.AllowDoc).HasDefaultValue(true);
            entity.Property(e => e.AllowDocx).HasDefaultValue(true);
            entity.Property(e => e.AllowPdf).HasDefaultValue(true);
            entity.Property(e => e.AutoBlock).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C44EEABAA");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053482F84AE4").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");
            entity.Property(e => e.SessionToken).HasMaxLength(100);
        });

        modelBuilder.Entity<VwCvuploadByIp>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CVUploadByIP");

            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LastUpload).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
