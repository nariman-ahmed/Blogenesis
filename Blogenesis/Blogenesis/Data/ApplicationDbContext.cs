using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Blogenesis.Models;

namespace Blogenesis.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BlogModel> Blogs { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<LikeModel> Likes { get; set; }
        public new DbSet<UserModel> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Blog relationships
            modelBuilder.Entity<BlogModel>()
                .HasMany(b => b.Comments)
                .WithOne(c => c.Blog)
                .HasForeignKey(c => c.BlogId)
                .OnDelete(DeleteBehavior.Cascade);  // When blog is deleted, delete its comments

            modelBuilder.Entity<BlogModel>()
                .HasMany(b => b.Likes)
                .WithOne(l => l.Blog)
                .HasForeignKey(l => l.BlogId)
                .OnDelete(DeleteBehavior.Cascade);  // When blog is deleted, delete its likes

            // User relationships
            modelBuilder.Entity<UserModel>()
                .HasMany(u => u.Blogs)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent accidental deletion of blogs when user is deleted

            modelBuilder.Entity<UserModel>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent accidental deletion of comments when user is deleted

            modelBuilder.Entity<UserModel>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // When user is deleted, remove their likes

            // Add indexes for better query performance
            modelBuilder.Entity<BlogModel>()
                .HasIndex(b => b.DateCreated);

            modelBuilder.Entity<BlogModel>()
                .HasIndex(b => b.Subject);  // For filtering by subject

            modelBuilder.Entity<CommentModel>()
                .HasIndex(c => c.DateCreated);  // For ordering comments by date

            modelBuilder.Entity<LikeModel>()
                .HasIndex(l => new { l.BlogId, l.UserId });  // For checking if user liked a blog
        }
    }
}
