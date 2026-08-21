using JoinIt.Api.Data;
using JoinIt.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Api.Services;

public class EstadoEventoService : IEstadoEventoService
{
    private readonly ApplicationDbContext _context;

    public EstadoEventoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public EstadoEvento CalcularEstado(
        DateTime dataInicio,
        DateTime dataFim,
        EstadoEvento estadoAtual)
    {
        // Um evento cancelado permanece sempre cancelado.
        if (estadoAtual == EstadoEvento.Cancelado)
        {
            return EstadoEvento.Cancelado;
        }

        var agora = DateTime.Now;

        if (agora >= dataFim)
        {
            return EstadoEvento.Terminado;
        }

        if (agora >= dataInicio)
        {
            return EstadoEvento.ADecorrer;
        }

        return EstadoEvento.ParaBreve;
    }

    public async Task AtualizarEstadosAsync()
    {
        var eventos = await _context.Eventos
            .Where(e => e.Estado != EstadoEvento.Cancelado)
            .ToListAsync();

        var existemAlteracoes = false;

        foreach (var evento in eventos)
        {
            var novoEstado = CalcularEstado(
                evento.DataHora,
                evento.DataFim,
                evento.Estado);

            if (evento.Estado != novoEstado)
            {
                evento.Estado = novoEstado;
                existemAlteracoes = true;
            }
        }

        if (existemAlteracoes)
        {
            await _context.SaveChangesAsync();
        }
    }
}