using JoinIt.Web.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class Evento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(
            100,
            ErrorMessage = "O título não pode ultrapassar 100 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(
            1000,
            ErrorMessage = "A descrição não pode ultrapassar 1000 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data e hora são obrigatórias.")]
        [Display(Name = "Data e hora")]
        public DateTime DataHora { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Display(Name = "Evento privado")]
        public bool IsPrivado { get; set; }

        [Range(
            2,
            1000,
            ErrorMessage = "O número máximo de participantes deve estar entre 2 e 1000.")]
        [Display(Name = "Número máximo de participantes")]
        public int NumMaxParticipantes { get; set; }

        public EstadoEvento Estado { get; set; }
            = EstadoEvento.ParaBreve;

        [Required]
        public string CriadorId { get; set; } = string.Empty;

        [ValidateNever]
        public ApplicationUser Criador { get; set; } = null!;

        public ICollection<EventoCategoria> EventosCategorias { get; set; } = new List<EventoCategoria>();

        public ICollection<Participante> Participantes { get; set; } = new List<Participante>();

        public ICollection<ConviteEvento> Convites { get; set; } = new List<ConviteEvento>();
    }
}