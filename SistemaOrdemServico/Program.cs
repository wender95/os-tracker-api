using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");

// Middleware de Autenticação e Controle de Acesso por Perfil
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    var method = context.Request.Method.ToUpper();

    // Rotas públicas
    if (path.Contains("/api/auth") || path.Contains("/api/login") || method == "OPTIONS")
    {
        await next();
        return;
    }

    // Valida autenticação básica
    if (!context.Request.Headers.ContainsKey("Authorization") || 
        string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Acesso não autorizado. Realize o login.");
        return;
    }

    var authHeader = context.Request.Headers["Authorization"].ToString();

    // Restrição de Criação de OS: Bloqueia se um Operador tentar fazer POST em /api/fluxos
    if (path == "/api/fluxos" && method == "POST")
    {
        if (authHeader.Contains("Role=Operador"))
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Apenas Vendas/Comercial ou Administradores podem criar novas OSs.");
            return;
        }
    }

    await next();
});

app.MapControllers();

app.Run();