using System;
using System.Collections.Generic;
using Mensajes.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mensajes.API.Data;

public partial class MensajesDbContext : DbContext
{
    public MensajesDbContext()
    {
    }

    public MensajesDbContext(DbContextOptions<MensajesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcusesLectura> AcusesLecturas { get; set; }

    public virtual DbSet<Mensaje> Mensajes { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcusesLectura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("acuses_lectura");

            entity.HasIndex(e => new { e.MensajeId, e.UsuarioId }, "uq_mensaje_usuario").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LeidoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("leido_en");
            entity.Property(e => e.MensajeId).HasColumnName("mensaje_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Mensaje).WithMany(p => p.AcusesLecturas)
                .HasForeignKey(d => d.MensajeId)
                .HasConstraintName("acuses_lectura_ibfk_1");
        });

        modelBuilder.Entity<Mensaje>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mensajes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AutorId)
                .HasMaxLength(255)
                .HasColumnName("autor_id");
            entity.Property(e => e.Contenido)
                .HasColumnType("text")
                .HasColumnName("contenido");
            entity.Property(e => e.ConversacionId)
                .HasMaxLength(255)
                .HasColumnName("conversacion_id");
            entity.Property(e => e.ConversacionTipo)
                .HasColumnType("enum('grupo','directo')")
                .HasColumnName("conversacion_tipo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_creacion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
