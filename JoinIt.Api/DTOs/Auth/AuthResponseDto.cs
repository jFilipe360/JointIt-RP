namespace JoinIt.Api.DTOs.Auth;

// Dados devolvidos após um registo ou login bem-sucedido
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEm { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}