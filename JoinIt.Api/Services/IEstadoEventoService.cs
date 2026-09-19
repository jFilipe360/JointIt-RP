using JoinIt.Api.Enums;

namespace JoinIt.Api.Services;

// Define as operações responsáveis pelo cálculo e atualização do estado dos eventos
public interface IEstadoEventoService
{
    EstadoEvento CalcularEstado(DateTime dataInicio, DateTime dataFim, EstadoEvento estadoAtual);

    Task AtualizarEstadosAsync();
}