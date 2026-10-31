using Microsoft.EntityFrameworkCore;
using RescatApp26.Models.Local;

namespace RescatApp26.Data;

public class LocalDbContext : DbContext
{
    public static string DatabasePath { get; } = Path.Combine(AppContext.BaseDirectory, "rescatapp_local.db");

    public DbSet<ColaItem> Cola => Set<ColaItem>();
    public DbSet<CacheItem> Cache => Set<CacheItem>();

    public static void EnsureReady()
    {
        using var db = new LocalDbContext();
        db.Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColaItem>(e =>
        {
            e.ToTable("ColaSincronizacion");
            e.HasKey(x => x.Id);
            e.Property(x => x.Entidad).HasMaxLength(50);
            e.Property(x => x.Operacion).HasMaxLength(20);
            e.Property(x => x.Estado).HasMaxLength(20);
            e.HasIndex(x => new { x.Estado, x.Id });
        });

        modelBuilder.Entity<CacheItem>(e =>
        {
            e.ToTable("CacheLocal");
            e.HasKey(x => x.Clave);
        });
    }
}