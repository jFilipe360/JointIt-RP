using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            // Garante que as migrations pendentes são aplicadas.
            await context.Database.MigrateAsync();

            const string adminRole = "Admin";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            string? adminEmail = configuration["AdminUser:Email"];

            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                ApplicationUser? adminUser =
                    await userManager.FindByEmailAsync(adminEmail);

                if (adminUser is not null &&
                    !await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    await userManager.AddToRoleAsync(
                        adminUser,
                        adminRole);
                }
            }

            string[] nomesCategorias =
            {
                "Desporto",
                "Música",
                "Tecnologia",
                "Educação",
                "Cultura",
                "Jogos",
                "Natureza",
                "Gastronomia",
                "Voluntariado",
                "Outros"
            };

            foreach (var nome in nomesCategorias)
            {
                bool categoriaExiste = await context.Categorias
                    .AnyAsync(c => c.Nome == nome);

                if (!categoriaExiste)
                {
                    context.Categorias.Add(new Categoria
                    {
                        Nome = nome
                    });
                }
            }

            // Garante que o criador também é participante dos eventos antigos.
            var eventosSemCriador = await context.Eventos
                .Where(e => !e.Participantes.Any(
                    p => p.UtilizadorId == e.CriadorId))
                .Select(e => new
                {
                    e.Id,
                    e.CriadorId
                })
                .ToListAsync();

            foreach (var evento in eventosSemCriador)
            {
                context.Participantes.Add(new Participante
                {
                    EventoId = evento.Id,
                    UtilizadorId = evento.CriadorId,
                    Estado = EstadoPedido.Aceite,
                    DataPedido = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
        }
    }
}