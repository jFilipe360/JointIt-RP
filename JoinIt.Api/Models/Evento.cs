using JoinIt.Api.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Models
{
    // Representa um evento e as respetivas regras de validação
    public class Evento : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título não pode ultrapassar 100 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength( 1000, ErrorMessage = "A descrição não pode ultrapassar 1000 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Data e hora de início")]
        public DateTime DataHora { get; set; }

        [Display(Name = "Data e hora de fim")]
        public DateTime DataFim { get; set; }

        [Display(Name = "Evento online")]
        public bool IsOnline { get; set; }

        [Display(Name = "Link do evento online")]
        [StringLength(500, ErrorMessage = "O link não pode ultrapassar 500 caracteres.")]
        [Url(ErrorMessage = "Introduz um link válido.")]
        public string? LinkOnline { get; set; }

        [StringLength(150, ErrorMessage = "O local não pode ultrapassar 150 caracteres.")]
        public string? Local { get; set; }

        [StringLength(250, ErrorMessage = "A morada não pode ultrapassar 250 caracteres.")]
        public string? Morada { get; set; }

        [Range(-90, 90, ErrorMessage = "A latitude deve estar entre -90 e 90.")]
        public double? Latitude { get; set; }

        [Range( -180, 180, ErrorMessage = "A longitude deve estar entre -180 e 180.")]
        public double? Longitude { get; set; }

        [Display(Name = "Evento privado")]
        public bool IsPrivado { get; set; }

        [Range(2, 1000, ErrorMessage = "O número máximo de participantes deve estar entre 2 e 1000.")]
        [Display(Name = "Número máximo de participantes")]
        public int NumMaxParticipantes { get; set; }

        public EstadoEvento Estado { get; set; } = EstadoEvento.ParaBreve;

        [Required]
        public string CriadorId { get; set; } = string.Empty;

        // Utilizador que criou o evento
        [ValidateNever]
        public ApplicationUser Criador { get; set; } = null!;

        // Relações associadas ao evento
        public ICollection<EventoCategoria> EventosCategorias { get; set; } = new List<EventoCategoria>();

        public ICollection<Participante> Participantes { get; set; } = new List<Participante>();

        public ICollection<ConviteEvento> Convites { get; set; } = new List<ConviteEvento>();

        public ICollection<MensagemEvento> Mensagens { get; set; } = new List<MensagemEvento>();

        // Valida regras que dependem da combinação de várias propriedades
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataFim <= DataHora)
            {
                yield return new ValidationResult(
                    "A data de fim deve ser posterior à data de início.",
                    new[] { nameof(DataFim) });
            }

            if (IsOnline)
            {
                if (string.IsNullOrWhiteSpace(LinkOnline))
                {
                    yield return new ValidationResult(
                        "O link do evento é obrigatório para eventos online.",
                        new[] { nameof(LinkOnline) });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Local))
                {
                    yield return new ValidationResult(
                        "O local é obrigatório para eventos presenciais.",
                        new[] { nameof(Local) });
                }
            }

            if (!IsOnline)
            {
                bool temLatitude = Latitude.HasValue;
                bool temLongitude = Longitude.HasValue;

                if (temLatitude != temLongitude)
                {
                    yield return new ValidationResult(
                        "A latitude e a longitude devem ser preenchidas em conjunto.",
                        new[]
                        {
                            nameof(Latitude),
                            nameof(Longitude)
                        });
                }
            }
        }
    }
}