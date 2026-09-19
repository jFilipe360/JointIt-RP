using System.Security.Claims;
using System.Text;
using JoinIt.Api.Data;
using JoinIt.Api.Models;
using JoinIt.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configura o Identity para utilizar ApplicationUser e o Entity Framework
builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Serviços da aplicação
builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<IEstadoEventoService, EstadoEventoService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

// Configura a autenticação da API através de tokens JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não está configurada.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key deve ter pelo menos 32 bytes.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer não está configurado.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience não está configurado.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,

            NameClaimType = ClaimTypes.Name,

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// OpenAPI e Scalar ficam disponíveis apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// A autenticação deve ocorrer antes da autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Adiciona o esquema Bearer à documentação OpenAPI.
internal sealed class BearerSecuritySchemeTransformer(
    Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider)
    : Microsoft.AspNetCore.OpenApi.IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        Microsoft.OpenApi.OpenApiDocument document,
        Microsoft.AspNetCore.OpenApi.OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (schemes.Any(s => s.Name == "Bearer"))
        {
            document.Components ??= new Microsoft.OpenApi.OpenApiComponents();

            document.Components.SecuritySchemes =
                new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
                    {
                        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = Microsoft.OpenApi.ParameterLocation.Header,
                        BearerFormat = "JWT"
                    }
                };
        }
    }
}