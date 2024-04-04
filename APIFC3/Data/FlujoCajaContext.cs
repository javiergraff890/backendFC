using System;
using System.Collections.Generic;
using APIFC3.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace APIFC3.Data;

public partial class FlujoCajaContext : DbContext
{
    public FlujoCajaContext()
    {
    }

    public FlujoCajaContext(DbContextOptions<FlujoCajaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Caja> Cajas { get; set; }

    public virtual DbSet<Movimiento> Movimientos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
    //    IConfiguration configuration = builder.Build();
    //    optionsBuilder.UseSqlServer(configuration.GetConnectionString("flujoCajaString"));
    //}
       

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Caja__3213E83FAA21B2B9");

            entity.ToTable("Caja");

            entity.HasIndex(e => new { e.Nombre, e.UserId }, "uq_yourtablename").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Saldo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("saldo");

            entity.HasOne(d => d.User).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Caja__UserId__1BC821DD");
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Movimien__3213E83F81820802");

            entity.ToTable("Movimiento");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Concepto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("concepto");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdCaja).HasColumnName("idCaja");
            entity.Property(e => e.Valor)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valor");

            entity.HasOne(d => d.IdCajaNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdCaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__idCaj__2BFE89A6");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Usuario__1788CC4C6418DB47");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.UserName, "UQ__Usuario__C9F2845615276AB1").IsUnique();

            entity.Property(e => e.Password)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
