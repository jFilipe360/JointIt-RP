using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.DTOs.Auth;

// Dados necessários para autenticar um utilizador na API.
public class LoginDto
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduz um email válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A password é obrigatória.")]
    public string Password { get; set; } = string.Empty;
}