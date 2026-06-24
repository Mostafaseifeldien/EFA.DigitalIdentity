using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFA.Domain.Identity;
using EFA.Domain.Matches;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFA.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            [
                "Admin",
            "MembershipOfficer",
            "AssignmentsOfficer",
            "CommunicationsOfficer",
            "SecurityOfficer",
            "ReportsViewer",
            "Member",
            "RefereeCommittee"
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            var adminEmail = "admin@efa.local";

            var admin =
                await userManager.FindByEmailAsync(adminEmail);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(
                    admin,
                    "Admin@123");

                await userManager.AddToRoleAsync(
                    admin,
                    "Admin");
            }

            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedMatchReferenceDataAsync(dbContext);
        }

        private static async Task SeedMatchReferenceDataAsync(ApplicationDbContext dbContext)
        {
            string[] tournaments =
            [
                "الدوري المصري الممتاز",
                "الدوري الممتاز",
                "كأس مصر"
            ];

            foreach (var name in tournaments)
            {
                if (!await dbContext.Tournaments.AnyAsync(x => x.Name == name))
                {
                    dbContext.Tournaments.Add(new Tournament
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            string[] stadiums =
            [
                "استاد القاهرة الدولي",
                "استاد برج العرب"
            ];

            foreach (var name in stadiums)
            {
                if (!await dbContext.Stadiums.AnyAsync(x => x.Name == name))
                {
                    dbContext.Stadiums.Add(new Stadium
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            string[] teams =
            [
                "الأهلي",
                "الزمالك",
                "بيراميدز",
                "الإسماعيلي",
                "المصري"
            ];

            foreach (var name in teams)
            {
                if (!await dbContext.Teams.AnyAsync(x => x.Name == name))
                {
                    dbContext.Teams.Add(new Team
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
