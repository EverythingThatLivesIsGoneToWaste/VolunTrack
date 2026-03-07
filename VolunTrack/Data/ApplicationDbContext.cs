using Microsoft.EntityFrameworkCore;
using VolunTrack.Models;

namespace VolunTrack.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<UserCategory> UserCategories => Set<UserCategory>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventPhoto> EventPhotos => Set<EventPhoto>();
        public DbSet<UserLeaderAssignment> UserLeaderAssignments => Set<UserLeaderAssignment>();
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<Participation> Participations => Set<Participation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Login).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Login).IsUnique();

                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);

                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Phone);

                entity.Property(e => e.Email).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);

                entity.Property(e => e.Role).IsRequired().HasConversion<string>();

                entity.Property(e => e.IsActive).IsRequired();
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.CreatedAtUtc).IsRequired();
            });

            modelBuilder.Entity<User>()
                .HasMany(e => e.Categories)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Participations)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(e => e.LeaderAssignments)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(e => e.CreatedEvents)
                .WithOne(e => e.CreatedByUser)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UploadedAttachments)
                .WithOne(e => e.UploadedByUser)
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UploadedEventPhotos)
                .WithOne(e => e.UploadedByUser)
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Category entity configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(80);
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);

                entity.Property(e => e.ColorRgb).IsRequired();

                entity.Property(e => e.IsActive).IsRequired();
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<Category>()
                .HasMany(e => e.UserCategories)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.EventCategories)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // UserCategory entity configuration
            modelBuilder.Entity<UserCategory>(entity => {
                entity.HasKey(e => e.Id);

                entity.HasIndex(uc => new { uc.UserId, uc.CategoryId }).IsUnique();
            });

            // Attachment entity configuration
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);

                entity.Property(e => e.EntityType).IsRequired().HasConversion<string>();

                entity.Property(e => e.EntityId).IsRequired();

                entity.HasIndex(a => new { a.EntityType, a.EntityId });

                entity.Property(e => e.FileName).IsRequired().HasMaxLength(150);

                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(200);

                entity.Property(e => e.UploadedAtUtc).IsRequired();
            });

            // Event entity configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(80);

                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);

                entity.Property(e => e.Place).IsRequired().HasMaxLength(200);

                entity.Property(e => e.Status).IsRequired().HasConversion<string>();

                entity.Property(e => e.StartDateTime).IsRequired();

                entity.Property(e => e.EndDateTime).IsRequired();

                entity.Property(e => e.EstimatedParticipantsCount).IsRequired();

                entity.Property(e => e.SkillsRequired).IsRequired().HasMaxLength(150);

                entity.Property(e => e.CreatedAtUtc).IsRequired();
            });

            modelBuilder.Entity<Event>()
                .HasMany(e => e.EventPhotos)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Participations)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.LeaderAssignments)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.EventCategories)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // EventPhotos entity configuration
            modelBuilder.Entity<EventPhoto>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(80);

                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(200);

                entity.Property(e => e.PhotoType).IsRequired().HasConversion<string>();

                entity.Property(e => e.UploadedAtUtc).IsRequired();

                entity.HasIndex(e => e.EventId);
            });

            // UserLeaderAssignment entity configuration
            modelBuilder.Entity<UserLeaderAssignment>(entity => {
                entity.HasKey(e => e.Id);

                entity.HasIndex(ula => new { ula.EventId, ula.UserId }).IsUnique();
            });

            // EventCategory entity configuration
            modelBuilder.Entity<EventCategory>(entity => {
                entity.HasKey(e => e.Id);

                entity.HasIndex(ec => new { ec.EventId, ec.CategoryId }).IsUnique();
            });

            // Participation entity configuration
            modelBuilder.Entity<Participation>(entity => {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Notes).HasMaxLength(200);

                entity.Property(e => e.Status).IsRequired().HasConversion<string>();

                entity.Property(e => e.TotalHours).IsRequired();

                entity.Property(e => e.IsManualCheckOut).IsRequired();

                entity.Property(e => e.IsConfirmedByCoordinator).IsRequired();

                entity.Property(e => e.IsConfirmedByLeader).IsRequired();

                entity.Property(e => e.HoursModerated).IsRequired();

                entity.Property(e => e.CreatedAtUtc).IsRequired();
            });
        }
    }
}
