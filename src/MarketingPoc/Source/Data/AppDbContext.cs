using MarketingPoc.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketingPoc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<TestResult> TestResults => Set<TestResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>().ToTable("Tenants");
        modelBuilder.Entity<Tenant>().HasKey(t => t.Id);
        modelBuilder.Entity<Tenant>().Property(t => t.Name).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Tenant>().Property(t => t.RateLimit).IsRequired();

        modelBuilder.Entity<Campaign>().ToTable("Campaigns");
        modelBuilder.Entity<Campaign>().HasKey(c => c.Id);
        modelBuilder.Entity<Campaign>().Property(c => c.Channel).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Campaign>().Property(c => c.Content).IsRequired();
        modelBuilder.Entity<Campaign>().Property(c => c.Status).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Campaign>().Property(c => c.ScheduledAt).HasColumnType("timestamp with time zone");
        modelBuilder.Entity<Campaign>().HasIndex(c => c.TenantId);
        modelBuilder.Entity<Campaign>().HasOne(c => c.Tenant).WithMany(t => t.Campaigns).HasForeignKey(c => c.TenantId);

        modelBuilder.Entity<TestResult>().ToTable("TestResults");
        modelBuilder.Entity<TestResult>().HasKey(t => t.Id);
        modelBuilder.Entity<TestResult>().Property(t => t.TestType).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<TestResult>().Property(t => t.StartTime).HasColumnType("timestamp with time zone");
        modelBuilder.Entity<TestResult>().Property(t => t.EndTime).HasColumnType("timestamp with time zone");
    }
}