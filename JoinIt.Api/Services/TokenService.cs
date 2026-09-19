using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JoinIt.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace JoinIt.Api.Services;

// Cria e assina os tokens JWT utilizados na autenticação da API
public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Gera um token JWT com os dados essenciais do utilizador
    public (string Token, DateTime ExpiraEm) CriarToken(ApplicationUser user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não está configurada.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer não está configurado.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience não está configurado.");

        var expirationMinutes = _configuration.GetValue<int?>("Jwt:ExpirationMinutes");

        if (expirationMinutes is null || expirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Jwt:ExpirationMinutes não está configurado corretamente.");
        }

        var expiraEm = DateTime.UtcNow.AddMinutes(expirationMinutes.Value);

        // Identifica o utilizador autenticado através das claims do token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        // Assina o token com a chave secreta configurada na aplicação
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return (tokenString, expiraEm);
    }
}