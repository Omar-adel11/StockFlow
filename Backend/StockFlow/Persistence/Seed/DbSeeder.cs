using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Seed
{
    public static class DbSeeder
    {
        // BusinessOwner/Manager/Staff are created through signup or an
        // invitation, never seeded directly - only the roles themselves
        // and the one SaasAdmin account need to exist before anyone logs in.
        private static readonly string[] RolesToSeed = Roles.All;

        private const string AdminEmail = "o.adel1029@gmail.com";
        private const string AdminInitialPassword = "ChangeMe!2026"; // change on first login

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            foreach (var roleName in RolesToSeed)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            var existingAdmin = await userManager.FindByEmailAsync(AdminEmail);
            if (existingAdmin is not null)
            {
                return; // already seeded on a previous run - nothing else to do
            }

            var admin = new User
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                Name = "admin",
                EmailConfirmed = true,
                // Deliberately null: a SaasAdmin belongs to no Business -
                // this is exactly the flag that lets them bypass the
                // per-business query filter once that's wired up.
                BusinessId = null
            };

            var result = await userManager.CreateAsync(admin, AdminInitialPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.SaasAdmin);
            }
            else
            {
                // Surfacing this via exception rather than swallowing it -
                // a failed seed (e.g. password policy rejects the default)
                // should stop startup loudly, not fail silently and leave
                // you with no way to log in at all.
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed SaasAdmin user: {errors}");
            }
        }
    }
}