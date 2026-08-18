using Microsoft.EntityFrameworkCore;
using SistemaOrdemServico.Data;
using SistemaOrdemServico.Domain.Interfaces;
using SistemaOrdemServico.Middlewares;
using SistemaOrdemServico.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositório atualizado
builder.Services.AddScoped<IFluxoRepository, FluxoRepository>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();