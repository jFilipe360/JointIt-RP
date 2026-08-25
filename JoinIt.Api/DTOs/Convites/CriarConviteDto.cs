using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.DTOs.Convites;

public class CriarConviteDto
{
    [Required]
    public string RecetorId { get; set; } = string.Empty;
}