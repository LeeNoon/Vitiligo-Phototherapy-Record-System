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
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

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
                        logger.LogInformation("Admin user created successfully");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
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
                        "头顶","前额","眉间","左太阳穴","右太阳穴","左颞部","右颞部","枕部","头后","发际","发际下",
                        "额头","左眉毛","右眉毛","左眼睑","右眼睑","左眼角","右眼角","左眼周","右眼周","鼻梁","左鼻翼","右鼻翼","鼻尖","左鼻孔","右鼻孔",
                        "左面颊","右面颊","左颧骨","右颧骨","口唇","上唇","下唇","左口角","右口角","下颌","颏部","颈前","左颈侧","右颈侧","颈后",
                        "左耳廓","右耳廓","左耳垂","右耳垂","左耳后","右耳后","左耳前","右耳前",
                        "咽喉","左锁骨","右锁骨","左肩峰","右肩峰","左肩胛","右肩胛","左腋窝","右腋窝","胸前","左乳房","右乳房","左乳晕","右乳晕","左乳头","右乳头","胸廓","左肋部","右肋部",
                        "腹部","脐周","脐下","腰部","左胁肋","右胁肋","背部","脊柱","骶尾部",
                        "左臂","右臂","左上臂","右上臂","左肘窝","右肘窝","左前臂","右前臂","左腕部","右腕部","左手背","右手背","左手掌","右手掌","左手指","右手指","左指甲","右指甲",
                        "左髋部","右髋部","左臀部","右臀部","左腹股沟","右腹股沟","会阴","左股内侧","右股内侧","左股外侧","右股外侧","左大腿","右大腿","左小腿","右小腿","左腘窝","右腘窝","左踝部","右踝部","左足背","右足背","左足底","右足底","左足趾","右足趾","左趾甲","右趾甲"
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
