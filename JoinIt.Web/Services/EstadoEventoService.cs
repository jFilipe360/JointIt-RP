using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Services
{
    public class EstadoEventoService : IEstadoEventoService
    {
        private readonly ApplicationDbContext _context;

        public EstadoEventoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Determina o estado do evento com base nas datas e no estado atual
        public EstadoEvento CalcularEstado(DateTime dataInicio, DateTime dataFim, EstadoEvento estadoAtual)
        {
            // Um evento cancelado permanece sempre cancelado.
            if (estadoAtual == EstadoEvento.Cancelado)
            {
                return EstadoEvento.Cancelado;
            }

            DateTime agora = DateTime.Now;

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

        // Atualiza na base de dados os eventos cujo estado já mudou
        public async Task AtualizarEstadosAsync()
        {
            var eventos = await _context.Eventos
                .Where(e =>
                    e.Estado != EstadoEvento.Cancelado)
                .ToListAsync();

            // Evita guardar na base de dados quando nenhum estado foi alterado
            bool existemAlteracoes = false;

            foreach (var evento in eventos)
            {
                EstadoEvento novoEstado = CalcularEstado(
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
}