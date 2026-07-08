using AnToanBaoMat.Data;
using AnToanBaoMat.Services;
using Microsoft.EntityFrameworkCore;

namespace AnToanBaoMat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext
            builder.Services.AddDbContext<JobRecruitmentDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<ISecurityService, SecurityService>();
            builder.Services.AddScoped<IntrusionDetectionService>();
            builder.Services.AddSingleton<EncryptionService>();
            builder.Services.AddScoped<OtpService>();
            // MVC
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<MailService>();
            builder.Services.AddScoped<AuditService>();
            builder.Services.AddScoped<DigitalSignatureService>();
            builder.Services.AddScoped<RSAService>();
            // Session
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Security Service

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}