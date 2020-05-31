using coReport.Auth;
using coReport.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace coReport.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<short>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var activityData = serviceProvider.GetRequiredService<IActivityData>();


            //Initialize activities
            activityData.InitializeActivities();

            //Creating Admin Role
            var roleCheck = await roleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                await roleManager.CreateAsync(new IdentityRole<short>("Admin"));
            }

            //Creating Employee Role
            roleCheck = await roleManager.RoleExistsAsync("Employee");
            if (!roleCheck)
            {
                await roleManager.CreateAsync(new IdentityRole<short>("Employee"));
            }

            //Creating Manager Role
            roleCheck = await roleManager.RoleExistsAsync("Manager");
            if (!roleCheck)
            {
                await roleManager.CreateAsync(new IdentityRole<short>("Manager"));
            }

            //Creating Admin User
            var user = await userManager.FindByNameAsync("admin");
            if (user == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = AppSettingInMemoryDatabase.ADMIN_USERNAME,
                    Email = "admin@email.com",
                    IsActive = true
                };
                string adminPassword = AppSettingInMemoryDatabase.ADMIN_PASSWORD;
                var createPowerUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}