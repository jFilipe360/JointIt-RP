using System.Security.Claims;
using JoinIt.Api.DTOs.Auth;
using JoinIt.Api.Models;
using JoinIt.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JoinIt.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existente = await _userManager.FindByEmailAsync(dto.Email);

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
            Email = dto.Email,
            Nome = dto.Nome
        };

        var resultado = await _userManager.CreateAsync(user, dto.Password);

        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                errors = resultado.Errors.Select(e => e.Description)
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null ||
            !await _userManager.CheckPasswordAsync(user, dto.Password))
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