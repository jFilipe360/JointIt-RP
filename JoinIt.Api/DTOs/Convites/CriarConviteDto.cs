using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.DTOs.Convites;

// Dados necessários para criar um convite para um evento
public class CriarConviteDto
{
    [Required(ErrorMessage = "O utilizador destinatário é obrigatório.")]
    public string RecetorId { get; set; } = string.Empty;
}