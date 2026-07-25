using JoinIt.Web.Enums;

namespace JoinIt.Web.Services
{
    public interface IEstadoEventoService
    {
        EstadoEvento CalcularEstado(
            DateTime dataInicio,
            DateTime dataFim,
            EstadoEvento estadoAtual);

        Task AtualizarEstadosAsync();
    }
}