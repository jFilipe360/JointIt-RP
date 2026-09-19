using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.DTOs.Auth;

// Dados necessários para registar um novo utilizador na API
public class RegisterDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode ultrapassar 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduz um email válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A password é obrigatória.")]
    [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
}