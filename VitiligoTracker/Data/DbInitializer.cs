using Microsoft.AspNetCore.Identity;
using VitiligoTracker.Models;

namespace VitiligoTracker.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Ensure Admin User from Configuration
            var adminConfig = configuration.GetSection("SeedData:AdminUser");
            var adminName = adminConfig["UserName"];
            var adminPass = adminConfig["Password"];
            var adminPhone = adminConfig["PhoneNumber"];
            var adminEmail = adminConfig["Email"];

            if (!string.IsNullOrEmpty(adminName) && !string.IsNullOrEmpty(adminPass))
            {
                var adminUser = await userManager.FindByNameAsync(adminName);
                if (adminUser == null)
                {
                    var newAdmin = new IdentityUser
                    {
                        UserName = adminName,
                        Email = adminEmail,
                        PhoneNumber = adminPhone,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(newAdmin, adminPass);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdmin, "Admin");
                    }
                }
                else
                {
                    // Update phone number if needed
                    if (!string.IsNullOrEmpty(adminPhone) && adminUser.PhoneNumber != adminPhone)
                    {
                        adminUser.PhoneNumber = adminPhone;
                        await userManager.UpdateAsync(adminUser);
                    }
                }
            }
        }
    }
}
