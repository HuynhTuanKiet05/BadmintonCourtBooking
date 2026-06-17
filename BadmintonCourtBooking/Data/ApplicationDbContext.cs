using BadmintonCourtBooking.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUserEntity, IdentityRole, string>(options)
{
    public DbSet<VenueEntity> Venues => Set<VenueEntity>();
    public DbSet<CourtEntity> Courts => Set<CourtEntity>();
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUserEntity>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.PlayArea).HasMaxLength(120);
            entity.Property(x => x.AvatarPath).HasMaxLength(260);
            entity.Property(x => x.PhoneNumber).HasMaxLength(30);
            entity.HasIndex(x => x.PhoneNumber);
        });

        modelBuilder.Entity<VenueEntity>(entity =>
        {
            entity.ToTable("Venues");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.District).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(240).IsRequired();
            entity.Property(x => x.OpenHours).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ContactName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ContactPhone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Highlight).HasMaxLength(200);
            entity.Property(x => x.ImagePath).HasMaxLength(260);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasMany(x => x.Courts)
                .WithOne(x => x.Venue)
                .HasForeignKey(x => x.VenueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourtEntity>(entity =>
        {
            entity.ToTable("Courts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(200);
            entity.HasIndex(x => new { x.VenueId, x.Name }).IsUnique();
            entity.HasMany(x => x.Bookings)
                .WithOne(x => x.Court)
                .HasForeignKey(x => x.CourtId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingEntity>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.CustomerPhone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.CancelReason).HasMaxLength(250);
            entity.HasIndex(x => new { x.CourtId, x.StartAt }).IsUnique();
            entity.HasOne(x => x.PlayerUser)
                .WithMany(x => x.PlayerBookings)
                .HasForeignKey(x => x.PlayerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Content).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
