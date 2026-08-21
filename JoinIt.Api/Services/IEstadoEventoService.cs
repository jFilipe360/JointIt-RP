using JoinIt.Api.Enums;

namespace JoinIt.Api.Services;

public interface IEstadoEventoService
{
    EstadoEvento CalcularEstado(
        DateTime dataInicio,
        DateTime dataFim,
        EstadoEvento estadoAtual);

    Task AtualizarEstadosAsync();
}