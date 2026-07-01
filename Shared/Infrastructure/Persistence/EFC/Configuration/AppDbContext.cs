using SafeFlow.API.Alerts.Domain.Model.Aggregates;
using SafeFlow.API.Alerts.Domain.Model.ValueObjects;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.ValueObjects;
using SafeFlow.API.Iam.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Reporting.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryLine> InventoryLines => Set<InventoryLine>();
    public DbSet<LogisticsCarrier> LogisticsCarriers => Set<LogisticsCarrier>();
    public DbSet<LogisticsDriver> LogisticsDrivers => Set<LogisticsDriver>();
    public DbSet<LogisticsDestination> LogisticsDestinations => Set<LogisticsDestination>();
    public DbSet<LogisticsRoute> LogisticsRoutes => Set<LogisticsRoute>();
    public DbSet<LogisticsDispatch> LogisticsDispatches => Set<LogisticsDispatch>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<TemperatureReading> TemperatureReadings => Set<TemperatureReading>();
    public DbSet<ReportCatalogItem> ReportCatalogItems => Set<ReportCatalogItem>();
    public DbSet<ReportRun> ReportRuns => Set<ReportRun>();
    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d));

        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
            d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : null,
            d => d.HasValue ? DateOnly.FromDateTime(d.Value) : null);

        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.ProductCode)
                .HasConversion(vo => vo.Value, v => new ProductCode(v))
                .HasMaxLength(64)
                .IsRequired();
            entity.HasIndex(p => p.ProductCode).IsUnique();
            entity.Property(p => p.Name).HasMaxLength(512).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(256).IsRequired();
            entity.Property(p => p.Batch).HasMaxLength(64).IsRequired();
            entity.Property(p => p.Status).HasMaxLength(32).IsRequired();
            entity.Property(p => p.ExpiryDate).HasConversion(nullableDateOnlyConverter);
        });

        builder.Entity<InventoryLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).ValueGeneratedOnAdd();
            entity.Property(l => l.LineCode)
                .HasConversion(vo => vo.Value, v => new InventoryLineCode(v))
                .HasMaxLength(64)
                .IsRequired();
            entity.HasIndex(l => l.LineCode).IsUnique();
            entity.Property(l => l.Location).HasMaxLength(512).IsRequired();
            entity.Property(l => l.EntryDate).HasConversion(dateOnlyConverter);
            entity.HasOne(l => l.Product)
                .WithMany(p => p.Lines)
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LogisticsCarrier>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CarrierCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.CarrierCode).IsUnique();
        });

        builder.Entity<LogisticsDriver>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DriverCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.DriverCode).IsUnique();
        });

        builder.Entity<LogisticsDestination>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DestinationCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.DestinationCode).IsUnique();
        });

        builder.Entity<LogisticsRoute>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RouteCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.RouteCode).IsUnique();
        });

        builder.Entity<LogisticsDispatch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DispatchCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.DispatchCode).IsUnique();
            entity.Property(x => x.InventoryLineCode).HasMaxLength(64).IsRequired();
        });

        builder.Entity<Alert>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.AlertCode)
                .HasConversion(vo => vo.Value, v => new AlertCode(v))
                .HasMaxLength(64)
                .IsRequired();
            entity.HasIndex(x => x.AlertCode).IsUnique();
            entity.Property(x => x.AlertType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(16).IsRequired();
            entity.Property(x => x.TitleJson).HasMaxLength(1024).IsRequired();
        });

        builder.Entity<TemperatureReading>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.ReadingCode)
                .HasConversion(vo => vo.Value, v => new TemperatureReadingCode(v))
                .HasMaxLength(64)
                .IsRequired();
            entity.HasIndex(x => x.ReadingCode).IsUnique();
            entity.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
        });

        builder.Entity<ReportCatalogItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CatalogCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.CatalogCode).IsUnique();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
        });

        builder.Entity<ReportRun>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RunCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.RunCode).IsUnique();
            entity.Property(x => x.CatalogCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(16).IsRequired();
        });

        builder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.Username).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        });

        builder.UseSnakeCaseNamingConvention();
    }
}
