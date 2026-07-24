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

        public DbSet<Amizade> Amizades { get; set; }

        public DbSet<ConviteEvento> ConvitesEvento { get; set; }

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

            builder.Entity<Amizade>()
                .HasOne(a => a.Emissor)
                .WithMany(u => u.PedidosAmizadeEnviados)
                .HasForeignKey(a => a.EmissorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Amizade>()
                .HasOne(a => a.Recetor)
                .WithMany(u => u.PedidosAmizadeRecebidos)
                .HasForeignKey(a => a.RecetorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Impede pedidos repetidos na mesma direção.
            builder.Entity<Amizade>()
                .HasIndex(a => new
                {
                    a.EmissorId,
                    a.RecetorId
                })
                .IsUnique();

            builder.Entity<ConviteEvento>()
                .HasOne(c => c.Evento)
                .WithMany(e => e.Convites)
                .HasForeignKey(c => c.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ConviteEvento>()
                .HasOne(c => c.Emissor)
                .WithMany(u => u.ConvitesEnviados)
                .HasForeignKey(c => c.EmissorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ConviteEvento>()
                .HasOne(c => c.Recetor)
                .WithMany(u => u.ConvitesRecebidos)
                .HasForeignKey(c => c.RecetorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Um utilizador só pode receber um convite por evento.
            builder.Entity<ConviteEvento>()
                .HasIndex(c => new
                {
                    c.EventoId,
                    c.RecetorId
                })
                .IsUnique();
        }
    }
}