using System;
using System.Collections.Generic;
using Conversaciones.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conversaciones.API.Data;

public partial class GruposDbContext : DbContext
{
    public GruposDbContext()
    {
    }

    public GruposDbContext(DbContextOptions<GruposDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Grupo> Grupos { get; set; }

    public virtual DbSet<MiembrosGrupo> MiembrosGrupos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("grupos");

            entity.Property(e => e.Id).HasColumnName("id");
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

        modelBuilder.Entity<MiembrosGrupo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("miembros_grupo");

            entity.HasIndex(e => new { e.GrupoId, e.UsuarioId }, "uq_grupo_usuario").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GrupoId).HasColumnName("grupo_id");
            entity.Property(e => e.Rol)
                .HasDefaultValueSql("'miembro'")
                .HasColumnType("enum('miembro','admin')")
                .HasColumnName("rol");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Grupo).WithMany(p => p.MiembrosGrupos)
                .HasForeignKey(d => d.GrupoId)
                .HasConstraintName("fk_miembro_grupo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
