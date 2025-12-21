using Domain.Entities;
using Domain;
using Domain.Enums;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class UserSeedData
    {
        public static async Task InitializeAsync(FDADbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "IT", Code = SystemCategory.IT },
                    new Category { Name = "Sales", Code = SystemCategory.Sales },
                    new Category { Name = "HR", Code = SystemCategory.HR },
                    new Category { Name = "Marketing", Code = SystemCategory.Marketing }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = FDAConst.ADMIN_ROLE, Code = SystemRole.Admin },
                    new Role { Name = FDAConst.STUDENT_ROLE, Code = SystemRole.Student }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                var passHasher = new PasswordHasher<User>();
                var adminRoleId = context.Roles.First(r => r.Code == SystemRole.Admin).Id;

                var admin = new User
                {
                    Name = "ghadah Admin",
                    Email = "Gadmin@fda.com",
                    PhoneNumber = "00962793500205",
                    RoleId = adminRoleId,
                };
                admin.Password = passHasher.HashPassword(admin, "Admin@123");
                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
