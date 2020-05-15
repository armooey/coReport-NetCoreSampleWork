using coReport.Auth;
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
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<short>>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //Creating Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                await RoleManager.CreateAsync(new IdentityRole<short>("Admin"));
            }

            //Creating Employee Role
            roleCheck = await RoleManager.RoleExistsAsync("Employee");
            if (!roleCheck)
            {
                await RoleManager.CreateAsync(new IdentityRole<short>("Employee"));
            }

            //Creating Manager Role
            roleCheck = await RoleManager.RoleExistsAsync("Manager");
            if (!roleCheck)
            {
                await RoleManager.CreateAsync(new IdentityRole<short>("Manager"));
            }

            //Creating Admin User
            var user = await UserManager.FindByNameAsync("admin");
            if (user == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = AppSettingInMemoryDatabase.ADMIN_USERNAME,
                    Email = "admin@email.com",
                    IsActive = true
                };
                string adminPassword = AppSettingInMemoryDatabase.ADMIN_PASSWORD;
                var createPowerUser = await UserManager.CreateAsync(adminUser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
