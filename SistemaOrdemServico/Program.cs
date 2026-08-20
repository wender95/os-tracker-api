var builder = WebApplication.CreateBuilder(args);

// 1. Adicionar Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. Adicionar Autorização Nativa
builder.Services.AddAuthorization();

var app = builder.Build();

// 4. Middlewares na ordem correta
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Permite a leitura dos metadados de autorização
app.UseAuthorization();

app.MapControllers();

app.Run();