using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SistemaIncidentesSeguridad.EF;

public partial class SistemaGestionDeIncidentesSeguridadContext : DbContext
{
    public SistemaGestionDeIncidentesSeguridadContext()
    {
    }

    public SistemaGestionDeIncidentesSeguridadContext(DbContextOptions<SistemaGestionDeIncidentesSeguridadContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Comentario> Comentarios { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Prioridad> Prioridads { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=SistemaGestionDeIncidentesSeguridad;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC071A1C7772");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comentar__3214EC0734037DD1");

            entity.ToTable("Comentario");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comentari__IdTic__47DBAE45");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comentari__IdUsu__48CFD27E");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Estado__3214EC07AF16A656");

            entity.ToTable("Estado");

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Prioridad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Priorida__3214EC07CD52001D");

            entity.ToTable("Prioridad");

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ticket__3214EC0763CC749E");

            entity.ToTable("Ticket");

            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaResolucion).HasColumnType("datetime");
            entity.Property(e => e.Titulo).HasMaxLength(150);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__IdCatego__4222D4EF");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__IdEstado__4316F928");

            entity.HasOne(d => d.IdPrioridadNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdPrioridad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__IdPriori__440B1D61");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__IdUsuari__412EB0B6");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC077A9AD32F");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.CorreoElectronico, "UQ__Usuario__531402F3E15C9BE7").IsUnique();

            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Contrasenia).HasMaxLength(255);
            entity.Property(e => e.CorreoElectronico).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
