using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using coReport.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using coReport.Auth;
using coReport.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
namespace coReport
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole<short>>(options =>
                 {
                     options.SignIn.RequireConfirmedAccount = false;
                     options.Password.RequiredLength = 6;
                     options.Password.RequireNonAlphanumeric = false;
                     options.Password.RequireUppercase = true;
                     options.User.RequireUniqueEmail = true;
                     options.Lockout.AllowedForNewUsers = false;
                 })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();
            //setting default urls
            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, option => 
            {
                option.LoginPath = "/Account/Login";
                option.AccessDeniedPath = "/Home/AccessDenied";
            });
            services.AddControllersWithViews();
            services.AddTransient<IReportData, ReportData>();
            services.AddTransient<IManagerReportData, ManagerReportData>();
            services.AddTransient<IManagerData, ManagerData>();
            services.AddTransient<IMessageService, MessageService>();
            services.AddTransient<IProjectData, ProjectData>();
            services.AddTransient<IActivityData, ActivityData>();
            services.AddScoped<ILogService, LogService>();
            services.AddRazorPages();
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = 737280000;
                x.MultipartBodyLengthLimit = 737280000;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            app.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == 404)
                {
                    context.HttpContext.Response.Redirect("/Home/NotFound");
                }
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
            AppSettingInMemoryDatabase.Initialize(Configuration);
            DbInitializer.Initialize(services).Wait();
        }
    }
}
