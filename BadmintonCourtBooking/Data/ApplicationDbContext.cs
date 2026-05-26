using BadmintonCourtBooking.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AppUserEntity> Users => Set<AppUserEntity>();
    public DbSet<VenueEntity> Venues => Set<VenueEntity>();
    public DbSet<CourtEntity> Courts => Set<CourtEntity>();
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    public DbSet<UserSnapshotEntity> UserSnapshots => Set<UserSnapshotEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.NormalizedPhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
            entity.Property(x => x.PlayArea).HasMaxLength(120);
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
            entity.HasIndex(x => x.NormalizedPhoneNumber).IsUnique();
        });

        modelBuilder.Entity<VenueEntity>(entity =>
        {
            entity.ToTable("Venues");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.District).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(240).IsRequired();
            entity.Property(x => x.OpenHours).HasMaxLength(40).IsRequired();
            entity.Property(x => x.OwnerName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.OwnerPhone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Highlight).HasMaxLength(200);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasMany(x => x.Courts)
                .WithOne(x => x.Venue)
                .HasForeignKey(x => x.VenueId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.OwnerUser)
                .WithMany(x => x.OwnedVenues)
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
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

        modelBuilder.Entity<UserSnapshotEntity>(entity =>
        {
            entity.ToTable("UserSnapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.RoleLabel).HasMaxLength(40).IsRequired();
        });
    }
}
