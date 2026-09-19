using JoinIt.Web.Enums;

namespace JoinIt.Web.Services
{
    // Define as operações usadas para calcular e atualizar o estado dos eventos
    public interface IEstadoEventoService
    {
        EstadoEvento CalcularEstado(DateTime dataInicio, DateTime dataFim, EstadoEvento estadoAtual);

        Task AtualizarEstadosAsync();
    }
}