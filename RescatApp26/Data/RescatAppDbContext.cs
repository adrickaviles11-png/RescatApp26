using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RescatApp26.Models.Entities;

namespace RescatApp26.Data;

public class RescatAppDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<Adoptante> Adoptantes => Set<Adoptante>();
    public DbSet<Adopcion> Adopciones => Set<Adopcion>();
    public DbSet<Veterinaria> Veterinarias => Set<Veterinaria>();
    public DbSet<Cita> Citas => Set<Cita>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        optionsBuilder.UseSqlServer(configuration.GetConnectionString("RescatAppConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreUsuario).HasMaxLength(50);
            e.Property(x => x.ContrasenaHash).HasMaxLength(200);
        });

        modelBuilder.Entity<Mascota>(e =>
        {
            // HasTrigger avisa a EF que la tabla tiene triggers (el nombre es solo informativo).
            e.ToTable("Mascotas", t => t.HasTrigger("TR_Mascotas_Bitacora"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100);
            e.Property(x => x.Especie).HasMaxLength(50);
            e.Property(x => x.Raza).HasMaxLength(50);
            e.Property(x => x.Descripcion).HasMaxLength(500);
            e.Property(x => x.FechaIngreso).HasColumnType("datetime");
        });

        modelBuilder.Entity<Adoptante>(e =>
        {
            e.ToTable("Adoptantes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombres).HasMaxLength(100);
            e.Property(x => x.Apellidos).HasMaxLength(100);
            e.Property(x => x.DocumentoIdentidad).HasMaxLength(30);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.Property(x => x.Correo).HasMaxLength(100);
            e.Property(x => x.Direccion).HasMaxLength(200);
            e.Property(x => x.PaisOrigen).HasMaxLength(50);
            e.Property(x => x.FechaRegistro).HasColumnType("datetime");
        });

        modelBuilder.Entity<Adopcion>(e =>
        {
            e.ToTable("Adopciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.CodigoAdopcion)
            .HasDefaultValueSql("NEXT VALUE FOR dbo.SeqCodigoAdopcion")
            .ValueGeneratedOnAdd();
            e.Property(x => x.Observaciones).HasMaxLength(500);
            e.Property(x => x.FechaAdopcion).HasColumnType("datetime");
            e.Property(x => x.CostoAdopcion).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Mascota).WithMany(m => m.Adopciones).HasForeignKey(x => x.MascotaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Adoptante).WithMany(a => a.Adopciones).HasForeignKey(x => x.AdoptanteId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Veterinaria>(e =>
        {
            e.ToTable("Veterinarias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.Property(x => x.Direccion).HasMaxLength(200);
        });

        modelBuilder.Entity<Cita>(e =>
        {
            e.ToTable("Citas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Motivo).HasMaxLength(200);
            e.Property(x => x.Observaciones).HasMaxLength(500);
            e.Property(x => x.FechaHora).HasColumnType("datetime");
            e.Property(x => x.FechaRegistro).HasColumnType("datetime");
            e.HasOne(x => x.Mascota).WithMany().HasForeignKey(x => x.MascotaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Veterinaria).WithMany().HasForeignKey(x => x.VeterinariaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Adoptante).WithMany().HasForeignKey(x => x.AdoptanteId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}