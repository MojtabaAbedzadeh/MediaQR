using MediaQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaQR.Infrastructure.Data;

public class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options)
        : base(options)
    {
    }

    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<QrCode> QrCodes => Set<QrCode>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // MediaFile
        // =========================

        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.ToTable("MediaFiles");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(2000);

            entity.Property(x => x.OriginalFileName)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.StoredFileName)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.StoragePath)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.MediaType)
                .IsRequired();

            entity.Property(x => x.FileSize)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });


        // =========================
        // QrCode
        // =========================

        modelBuilder.Entity<QrCode>(entity =>
        {
            entity.ToTable("QrCodes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Title)
                .HasMaxLength(250);

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.ExpiresAt);

            // MediaFile 1 ---> N QrCode
            entity.HasOne(x => x.MediaFile)
                .WithMany(x => x.QrCodes)
                .HasForeignKey(x => x.MediaFileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.MediaFileId);
        });

        // =========================
        // AnalyticsEvent
        // =========================

        modelBuilder.Entity<AnalyticsEvent>(entity =>
        {
            entity.ToTable("AnalyticsEvents");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventType)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.VisitorId)
                .HasMaxLength(100);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.UserAgent)
                .HasMaxLength(1000);

            entity.Property(x => x.DeviceType)
                .HasMaxLength(50);

            entity.Property(x => x.OperatingSystem)
                .HasMaxLength(100);

            entity.Property(x => x.Browser)
                .HasMaxLength(100);

            entity.Property(x => x.BrowserVersion)
                .HasMaxLength(50);

            entity.Property(x => x.Country)
                .HasMaxLength(100);

            entity.Property(x => x.City)
                .HasMaxLength(100);

            entity.Property(x => x.Referrer)
                .HasMaxLength(2000);

            entity.Property(x => x.DurationSeconds);

            entity.Property(x => x.ProgressPercent);

            entity.HasOne(x => x.QrCode)
                .WithMany(x => x.AnalyticsEvents)
                .HasForeignKey(x => x.QrCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.QrCodeId);

            entity.HasIndex(x => x.EventType);

            entity.HasIndex(x => x.CreatedAt);

            entity.HasIndex(x => new
            {
                x.QrCodeId,
                x.EventType,
                x.CreatedAt
            });

            entity.HasIndex(x => new
            {
                x.QrCodeId,
                x.VisitorId
            });
        });
    }
}