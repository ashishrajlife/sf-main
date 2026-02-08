using Microsoft.EntityFrameworkCore;
using stripfaces.Models;

namespace stripfaces.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Video> Videos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(10).HasDefaultValue("user");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Model configuration
            modelBuilder.Entity<Model>(entity =>
            {
                entity.ToTable("models");
                entity.HasKey(e => e.ModelId);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ProfilePic).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Video configuration
            modelBuilder.Entity<Video>(entity =>
            {
                entity.ToTable("videos");
                entity.HasKey(e => e.VideoId);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
                entity.Property(e => e.ThumbnailPath).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.IsApproved).HasDefaultValue(true);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETDATE()");

                // Foreign key relationships
                entity.HasOne(v => v.Model)
                      .WithMany(m => m.Videos)
                      .HasForeignKey(v => v.ModelId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.UploadedBy)
                      .WithMany(u => u.UploadedVideos)
                      .HasForeignKey(v => v.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}