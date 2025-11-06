using System;
using System.Collections.Generic;
using Conversaciones.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conversaciones.API.Data;

public partial class ConversacionesDbContext : DbContext
{
    public ConversacionesDbContext()
    {
    }

    public ConversacionesDbContext(DbContextOptions<ConversacionesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversacion> Conversaciones { get; set; }

    public virtual DbSet<MiembrosConversacion> MiembrosConversacion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("conversaciones");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");
            entity.Property(e => e.AvatarUrl)
                .HasColumnType("text")
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreadorId)
                .HasMaxLength(255)
                .HasColumnName("creador_id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<MiembrosConversacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("miembros_conversacion");

            entity.HasIndex(e => new { e.ConversacionId, e.UsuarioId }, "uq_conversacion_usuario").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConversacionId).HasColumnName("conversacion_id");
            entity.Property(e => e.Rol)
                .HasDefaultValueSql("'miembro'")
                .HasColumnType("enum('miembro','admin')")
                .HasColumnName("rol");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Conversacion).WithMany(p => p.MiembrosConversacion)
                .HasForeignKey(d => d.ConversacionId)
                .HasConstraintName("fk_miembro_conversacion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
