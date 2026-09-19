using JoinIt.Web.Data;
using JoinIt.Web.Hubs;
using JoinIt.Web.Models;
using JoinIt.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configura o Identity com suporte a roles e persistência através do EF Core
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Redireciona acessos sem permissão para a página personalizada de erro 403
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Erro/403";
});

builder.Services.AddRazorPages();

// Serviços da aplicação

builder.Services.AddScoped<
    INotificacaoService,
    NotificacaoService>();

builder.Services.AddScoped<
    IEstadoEventoService,
    EstadoEventoService>();

// Configura o serviço de email utilizado pelo ASP.NET Core Identity
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<
    Microsoft.AspNetCore.Identity.UI.Services.IEmailSender,
    EmailSender>();

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // Em produção, trata exceções e força o uso de HTTPS
    app.UseHsts();
}

app.UseHttpsRedirection();

// Apresenta a página personalizada para respostas HTTP como 403 e 404
app.UseStatusCodePagesWithReExecute("/Erro/{0}");

app.UseRouting();

// Bloqueia funcionalidades do Identity que não são utilizadas pela aplicação
app.Use(async (context, next) =>
{
    string path = context.Request.Path.Value ?? string.Empty;

    string[] paginasIdentityBloqueadas =
    {
        "/Identity/Account/Manage/TwoFactorAuthentication",
        "/Identity/Account/Manage/EnableAuthenticator",
        "/Identity/Account/Manage/Disable2fa",
        "/Identity/Account/Manage/GenerateRecoveryCodes",
        "/Identity/Account/Manage/ResetAuthenticator",
        "/Identity/Account/Manage/PersonalData",
        "/Identity/Account/Manage/Email",
        "/Identity/Account/Manage",
        "/Identity/Account/ResendEmailConfirmation"
    };

    if (paginasIdentityBloqueadas.Any(
        pagina => path.Equals(
            pagina,
            StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.StatusCode =
            StatusCodes.Status404NotFound;

        return;
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Endpoints SignalR do chat e das notificações em tempo real
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificacoesHub>("/hubs/notificacoes");

// Inicializa roles e o utilizador administrador da aplicação
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    await DbInitializer.InitializeAsync(
        context,
        roleManager,
        userManager,
        app.Configuration);
}

app.Run();
