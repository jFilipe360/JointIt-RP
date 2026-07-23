using JoinIt.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Evento> Eventos { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<EventoCategoria> EventosCategorias { get; set; }

        public DbSet<Participante> Participantes { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventoCategoria>()
                .HasKey(ec => new
                {
                    ec.EventoId,
                    ec.CategoriaId
                });

            builder.Entity<EventoCategoria>()
                .HasOne(ec => ec.Evento)
                .WithMany(e => e.EventosCategorias)
                .HasForeignKey(ec => ec.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventoCategoria>()
                .HasOne(ec => ec.Categoria)
                .WithMany(c => c.EventosCategorias)
                .HasForeignKey(ec => ec.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Evento>()
                .HasOne(e => e.Criador)
                .WithMany(u => u.EventosCriados)
                .HasForeignKey(e => e.CriadorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Categoria>()
                .HasIndex(c => c.Nome)
                .IsUnique();

            // Um utilizador só pode participar uma vez em cada evento.
            builder.Entity<Participante>()
                .HasKey(p => new
                {
                    p.EventoId,
                    p.UtilizadorId
                });

            builder.Entity<Participante>()
                .HasOne(p => p.Evento)
                .WithMany(e => e.Participantes)
                .HasForeignKey(p => p.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Participante>()
                .HasOne(p => p.Utilizador)
                .WithMany(u => u.Participacoes)
                .HasForeignKey(p => p.UtilizadorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}