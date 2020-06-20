using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LambdaForums.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LambdaForums
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            try
            {
                var scope = host.Services.CreateScope();

                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userMgr = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
                var roleMgr = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<IdentityRole>>();

                ctx.Database.EnsureCreated();

                var adminRole = new IdentityRole("Admin");
                if (!ctx.Roles.Any())
                {
                    roleMgr.CreateAsync(adminRole).GetAwaiter().GetResult();
                }

                if (!ctx.Users.Any(u => u.UserName == "ForumAdmin2"))
                {
                    //create a admin
                    var adminUser = new ApplicationUser
                    {
                        UserName = "ForumAdmin2",
                        Email = "admin2@example.com"
                    };
                    var result = userMgr.CreateAsync(adminUser, "password").GetAwaiter().GetResult();

                    //add role to user
                    userMgr.AddToRoleAsync(adminUser, adminRole.Name).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((builderContext, config) => 
            {
                IHostEnvironment env = builderContext.HostingEnvironment;
                config.AddJsonFile("storageSettings.json", optional: false, reloadOnChange: true);
            })


                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
