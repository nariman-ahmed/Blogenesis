using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogenesis.Models;
using Microsoft.AspNetCore.Identity;
using Blogenesis.Helpers.Constants;

namespace Blogenesis.Data
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (userManager == null)
            {
                throw new ArgumentNullException(nameof(userManager));
            }

            // Ensure the database is created
            await context.Database.EnsureCreatedAsync();

            // Seed initial data if necessary
            if (!context.Blogs.Any())
            {
                // Get the user we created in SeedUsersAndRolesAsync
                var user = await userManager.FindByEmailAsync("nari@gmail.com");
                
                if (user != null)
                {
                    // Add initial blogs with valid user reference
                    var newBlog = new BlogModel()
                    {
                        Title = "Welcome to Blogenesis",
                        Content = "This is our first blog post! Welcome to Blogenesis, a modern blogging platform where ideas come to life.",
                        DateCreated = DateTime.UtcNow,
                        ReadTimeMinutes = 5,
                        Subject = "Tech",
                        UserId = user.Id // Link the blog to our seeded user
                    };

                    await context.Blogs.AddAsync(newBlog);
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task SeedUsersAndRolesAsync(UserManager<UserModel> userManager,
        RoleManager<IdentityRole> roleManager)
        {
            //initialize roles admin and user
            if (!roleManager.Roles.Any())
            {
                foreach (var role in AppRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }

            //initialize a user
            if (!userManager.Users.Any())
            {
                var newPassword = "newPassword123!";
                var newUser = new UserModel()
                {
                    UserName = "nariman.ahmed",
                    Email = "nari@gmail.com",
                    NormalizedEmail = "NARI@GMAIL.COM",
                    NormalizedUserName = "NARIMAN.AHMED",
                    FullName = "Nariman Ahmed",
                    ProfilePicUrl = "https://shorturl.at/EbnVL",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, newPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, AppRoles.User);
                }
            }
            
        }
    }
}