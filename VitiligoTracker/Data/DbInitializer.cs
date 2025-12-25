using Microsoft.AspNetCore.Identity;
using VitiligoTracker.Models;

namespace VitiligoTracker.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
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
                // 初始化部位字典
                if (!context.BodyPartDicts.Any())
                {
                    var parts = new[]
                    {
                        "头顶","前额","眉间","太阳穴","颞部","枕部","头后","发际","发际下",
                        "额头","眉毛","眼睑","眼角","眼周","鼻梁","鼻翼","鼻尖","鼻孔",
                        "面颊","颧骨","口唇","上唇","下唇","口角","下颌","颏部","颈前","颈侧","颈后",
                        "耳廓","耳垂","耳后","耳前",
                        "咽喉","锁骨","肩峰","肩胛","腋窝","胸前","乳房","乳晕","乳头","胸廓","肋部",
                        "腹部","脐周","脐下","腰部","胁肋","背部","脊柱","骶尾部",
                        "臂","上臂","肘窝","前臂","腕部","手背","手掌","手指","指甲",
                        "髋部","臀部","腹股沟","会阴","股内侧","股外侧","大腿","小腿","腘窝","踝部","足背","足底","足趾","趾甲"
                    };
                    foreach (var p in parts)
                    {
                        context.BodyPartDicts.Add(new Models.BodyPartDict { Name = p });
                    }
                    context.SaveChanges();
                }
        }
    }
}
