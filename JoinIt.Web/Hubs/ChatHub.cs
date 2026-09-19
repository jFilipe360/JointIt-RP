using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using JoinIt.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace JoinIt.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        //Liga cada ConnectionId ao evento e ao utilizador que está ligado a esse evento.
        private static readonly ConcurrentDictionary<string, LigacaoChat> Ligacoes = new();

        private readonly ApplicationDbContext _context;
        private readonly IEstadoEventoService _estadoEventoService;

        public ChatHub(ApplicationDbContext context,IEstadoEventoService estadoEventoService)
        {
            _context = context;
            _estadoEventoService = estadoEventoService;
        }

        //Valida o acesso e adiciona a ligação ao grupo SignalR correspondente ao evento.
        public async Task EntrarNoEvento(int eventoId)
        {
            if (eventoId <= 0)
            {
                throw new HubException("O evento indicado não é válido.");
            }

            string? utilizadorId = Context.UserIdentifier;

            if (string.IsNullOrWhiteSpace(utilizadorId))
            {
                throw new HubException("Não foi possível identificar o utilizador.");
            }

            bool podeAceder = await PodeAcederAoEventoAsync(eventoId, utilizadorId);

            if (!podeAceder)
            {
                throw new HubException("Não tens acesso ao chat deste evento.");
            }

            string? nomeUtilizador = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == utilizadorId)
                .Select(u => u.Nome)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(nomeUtilizador))
            {
                throw new HubException("O utilizador não foi encontrado.");
            }

            //Uma ligação só pode estar associada a um evento de cada vez.
            if (Ligacoes.TryGetValue(Context.ConnectionId, out LigacaoChat? ligacaoAnterior))
            {
                if (ligacaoAnterior.EventoId == eventoId)
                {
                    await EnviarUtilizadoresLigadosAsync(eventoId);
                    return;
                }

                await RemoverLigacaoDoEventoAsync(Context.ConnectionId,ligacaoAnterior);
            }

            bool utilizadorJaEstavaLigado = ExisteOutraLigacao(eventoId, utilizadorId,Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId,ObterNomeGrupo(eventoId));

            Ligacoes[Context.ConnectionId] = new LigacaoChat(eventoId,utilizadorId,nomeUtilizador);

            //Evita avisos repetidos quando o utilziador tem várias ligações ao mesmo evento.
            if (!utilizadorJaEstavaLigado)
            {
                await Clients
                    .OthersInGroup(ObterNomeGrupo(eventoId))
                    .SendAsync("UtilizadorEntrou",new
                        {
                            utilizadorId,
                            nome = nomeUtilizador
                        });
            }

            await EnviarUtilizadoresLigadosAsync(eventoId);
        }

        //Remove a ligação atual do grupo do evento.
        public async Task SairDoEvento(int eventoId)
        {
            if (eventoId <= 0)
            {
                return;
            }

            if (!Ligacoes.TryGetValue(Context.ConnectionId,out LigacaoChat? ligacao))
            {
                return;
            }

            if (ligacao.EventoId != eventoId)
            {
                return;
            }

            await RemoverLigacaoDoEventoAsync(Context.ConnectionId,ligacao);
        }

        //Valida, guarda e envia a mensagem para todos os utilizadores ligados ao evento.
        public async Task EnviarMensagem(int eventoId,string? conteudo)
        {
            if (eventoId <= 0)
            {
                throw new HubException("O evento indicado não é válido.");
            }

            string? utilizadorId = Context.UserIdentifier;

            if (string.IsNullOrWhiteSpace(utilizadorId))
            {
                throw new HubException("Não foi possível identificar o utilizador.");
            }

            string conteudoLimpo = conteudo?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(conteudoLimpo))
            {
                throw new HubException("A mensagem não pode estar vazia.");
            }

            if (conteudoLimpo.Length > 500)
            {
                throw new HubException("A mensagem não pode ultrapassar 500 caracteres.");
            }

            var evento = await _context.Eventos
                .Include(e => e.Participantes)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

            if (evento is null)
            {
                throw new HubException("O evento não foi encontrado.");
            }

            bool podeAceder =
                evento.CriadorId == utilizadorId ||
                evento.Participantes.Any(p =>
                    p.UtilizadorId == utilizadorId &&
                    p.Estado == EstadoPedido.Aceite);

            if (!podeAceder)
            {
                throw new HubException("Não tens acesso ao chat deste evento.");
            }

            //Adiciona o estado do evento antes de decidir se o chat permite ou não enviar mensagens.
            EstadoEvento estadoAtual =
                _estadoEventoService.CalcularEstado(
                    evento.DataHora,
                    evento.DataFim,
                    evento.Estado);

            if (evento.Estado != estadoAtual)
            {
                evento.Estado = estadoAtual;
            }

            //O histórico continua disponível, mas só eventos ativos aceitam mensagens.
            bool chatAtivo = estadoAtual == EstadoEvento.ParaBreve || estadoAtual == EstadoEvento.ADecorrer;

            if (!chatAtivo)
            {
                if (_context.ChangeTracker.HasChanges())
                {
                    await _context.SaveChangesAsync();
                }

                throw new HubException("Este chat encontra-se em modo de leitura.");
            }

            var utilizador = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == utilizadorId)
                .Select(u => new
                {
                    u.Nome,
                    u.FotoPerfil
                })
                .FirstOrDefaultAsync();

            if (utilizador is null)
            {
                throw new HubException("O utilizador não foi encontrado.");
            }

            var mensagem = new MensagemEvento
            {
                EventoId = eventoId,
                UtilizadorId = utilizadorId,
                Conteudo = conteudoLimpo,
                EnviadaEm = DateTime.Now
            };

            _context.MensagensEvento.Add(mensagem);

            await _context.SaveChangesAsync();

            var mensagemCliente = new
            {
                id = mensagem.Id,
                utilizadorId,
                nome = utilizador.Nome,
                fotoPerfil = utilizador.FotoPerfil,
                conteudo = mensagem.Conteudo,
                enviadaEm = mensagem.EnviadaEm.ToString("O")
            };

            await Clients
                .Group(ObterNomeGrupo(eventoId))
                .SendAsync("ReceberMensagem",mensagemCliente);
        }

        //Limpa a presença do utilizador quando a ligação SignalR é encerrada.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Ligacoes.TryRemove(Context.ConnectionId,out LigacaoChat? ligacao))
            {
                bool utilizadorContinuaLigado =
                    ExisteOutraLigacao(
                        ligacao.EventoId,
                        ligacao.UtilizadorId,
                        Context.ConnectionId);

                if (!utilizadorContinuaLigado)
                {
                    await Clients
                        .Group(ObterNomeGrupo(ligacao.EventoId))
                        .SendAsync("UtilizadorSaiu",
                            new
                            {
                                utilizadorId =
                                    ligacao.UtilizadorId,

                                nome =
                                    ligacao.Nome
                            });
                }

                await EnviarUtilizadoresLigadosAsync(ligacao.EventoId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        //Remove a ligação e notifica o grupo se o utilizador ficar totalmente offline.
        private async Task RemoverLigacaoDoEventoAsync(string connectionId,LigacaoChat ligacao)
        {
            await Groups.RemoveFromGroupAsync(connectionId,ObterNomeGrupo(ligacao.EventoId));

            Ligacoes.TryRemove(connectionId,out _);

            bool utilizadorContinuaLigado =
                ExisteOutraLigacao(ligacao.EventoId,ligacao.UtilizadorId,connectionId);

            if (!utilizadorContinuaLigado)
            {
                await Clients
                    .Group(ObterNomeGrupo(ligacao.EventoId))
                    .SendAsync("UtilizadorSaiu",
                        new
                        {
                            utilizadorId =
                                ligacao.UtilizadorId,

                            nome =
                                ligacao.Nome
                        });
            }

            await EnviarUtilizadoresLigadosAsync(ligacao.EventoId);
        }

        //Envia ao grupo a lista de utilizadores únicos atualmente ligados.
        private async Task EnviarUtilizadoresLigadosAsync(int eventoId)
        {
            var utilizadoresLigados = Ligacoes
                .Values
                .Where(l => l.EventoId == eventoId)
                .GroupBy(l => l.UtilizadorId)
                .Select(grupo => new
                {
                    utilizadorId = grupo.Key,

                    nome = grupo
                        .Select(l => l.Nome)
                        .First()
                })
                .OrderBy(u => u.nome)
                .ToList();

            await Clients
                .Group(ObterNomeGrupo(eventoId))
                .SendAsync("AtualizarUtilizadoresLigados",utilizadoresLigados);
        }

        //Verifica se a mesma conta ainda possui outra ligação ao mesmo evento.
        private static bool ExisteOutraLigacao(int eventoId,string utilizadorId,string connectionIdIgnorado)
        {
            return Ligacoes.Any(item =>
                item.Key != connectionIdIgnorado &&
                item.Value.EventoId == eventoId &&
                item.Value.UtilizadorId == utilizadorId);
        }

        //O chat só é acessível ao criador do evento e aos participantes que tenham o pedido aceite.
        private async Task<bool> PodeAcederAoEventoAsync(int eventoId,string utilizadorId)
        {
            return await _context.Eventos
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Id == eventoId &&
                    (
                        e.CriadorId == utilizadorId ||
                        e.Participantes.Any(p =>
                            p.UtilizadorId == utilizadorId &&
                            p.Estado == EstadoPedido.Aceite)
                    ));
        }

        private static string ObterNomeGrupo(int eventoId)
        {
            return $"evento-{eventoId}";
        }

        private sealed record LigacaoChat(int EventoId,string UtilizadorId,string Nome);
    }
}