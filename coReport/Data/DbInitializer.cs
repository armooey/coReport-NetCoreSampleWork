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
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration Configuration)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<short>>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //Creating Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("ادمین");
            if (!roleCheck)
            {
                await RoleManager.CreateAsync(new IdentityRole<short>("ادمین"));
            }

            //Creating Employee Role
            roleCheck = await RoleManager.RoleExistsAsync("کارمند");
            if (!roleCheck)
            {
                await RoleManager.CreateAsync(new IdentityRole<short>("کارمند"));
            }

            //Creating Manager Role
            roleCheck = await RoleManager.RoleExistsAsync("مدیر");
            if (!roleCheck)
            {
                await RoleManager.CreateAsync(new IdentityRole<short>("مدیر"));
            }

            //Creating Admin User
            var user = await UserManager.FindByNameAsync("admin");
            if (user == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = Configuration.GetSection("AdminAuthentication").GetValue<String>("Username"),
                    Email = "admin@email.com",
                    IsActive = true
                };
                string adminPassword = Configuration.GetSection("AdminAuthentication").GetValue<String>("Password");

                var createPowerUser = await UserManager.CreateAsync(adminUser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(adminUser, "ادمین");

                }
            }
        }
    }
}
