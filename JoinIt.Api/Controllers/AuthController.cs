using System.Security.Claims;
using JoinIt.Api.DTOs.Auth;
using JoinIt.Api.Models;
using JoinIt.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JoinIt.Api.Controllers;

// Gere o registo, autenticação e identificação dos utilizadores da API
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    // Cria uma conta através do ASP.NET Core Identity e devolve um token JWT
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existente = await _userManager.FindByEmailAsync(dto.Email.Trim());

        if (existente != null)
        {
            return Conflict(new
            {
                message = "Já existe um utilizador com este email."
            });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email.Trim(),
            Nome = dto.Nome.Trim()
        };

        // O Identity valida e guarda a password de forma segura
        var resultado = await _userManager.CreateAsync(user, dto.Password);

        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                errors = resultado.Errors.Select(e => e.Description)
            });
        }

        // Após o registo, autentica imediatamente o utilizador através de JWT
        var (token, expiraEm) = _tokenService.CriarToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiraEm = expiraEm,
            UserId = user.Id,
            Nome = user.Nome ?? string.Empty,
            Email = user.Email ?? string.Empty
        });
    }

    // Valida as credenciais através do Identity e devolve um token JWT
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized(new
            {
                message = "Email ou password inválidos."
            });
        }

        var (token, expiraEm) = _tokenService.CriarToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiraEm = expiraEm,
            UserId = user.Id,
            Nome = user.Nome ?? string.Empty,
            Email = user.Email ?? string.Empty
        });
    }

    // Devolve os dados do utilizador identificado pelo token JWT atual
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            nome = user.Nome,
            email = user.Email
        });
    }
}