using System;
using System.Collections.Generic;
using APIFC3.Data.Models;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=flujo_caja;Trusted_connection=true; TrustServerCertificate=true");

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
            entity.HasKey(e => e.Id).HasName("PK__Movimien__3213E83F65AA1B84");

            entity.ToTable("Movimiento");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Concepto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("concepto");
            entity.Property(e => e.IdCaja).HasColumnName("idCaja");
            entity.Property(e => e.Valor)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valor");

            entity.HasOne(d => d.IdCajaNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdCaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__idCaj__1EA48E88");
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
