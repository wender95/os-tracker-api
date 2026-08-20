using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.IO;

namespace SistemaOrdemServico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly string _caminhoBanco = Path.Combine(Directory.GetCurrentDirectory(), "ordensservico.db");
    private readonly string _connectionString = $"Data Source={_caminhoBanco}";

    public AuthController()
    {
        InicializarBanco();
    }

    private void InicializarBanco()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 1. Garante que a tabela tenha apenas os campos necessários (Usuario, Senha, Setor, Role)
        var createCmd = connection.CreateCommand();
        createCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id TEXT PRIMARY KEY,
                Usuario TEXT UNIQUE COLLATE NOCASE,
                Senha TEXT,
                Setor TEXT,
                Role TEXT
            );";
        createCmd.ExecuteNonQuery();

        // 2. Insere o admin padrão se não existir
        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT OR IGNORE INTO Usuarios (Id, Usuario, Senha, Setor, Role)
            VALUES ('admin-id-padrao', 'admin', '123456', 'Vendas', 'Administrador');";
        insertCmd.ExecuteNonQuery();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Usuario) || string.IsNullOrWhiteSpace(model.Senha))
            return BadRequest("Informe usuário e senha.");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Usuario, Setor, Role FROM Usuarios WHERE LOWER(Usuario) = LOWER(@usuario) AND Senha = @senha;";
        command.Parameters.AddWithValue("@usuario", model.Usuario.Trim());
        command.Parameters.AddWithValue("@senha", model.Senha.Trim());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var usuario = reader.GetString(0);
            var setor = reader.GetString(1);
            var role = reader.GetString(2);

            var token = $"TOKEN_{usuario}_{setor}_{role}";
            return Ok(new LoginResponseDto { Token = token, Usuario = usuario, Setor = setor, Role = role });
        }

        return Unauthorized("Usuário ou senha incorretos.");
    }

    [HttpGet("usuarios")]
    public IActionResult ListarUsuarios()
    {
        var lista = new List<UsuarioResponseDto>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Usuario, Setor, Role FROM Usuarios;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new UsuarioResponseDto
            {
                Id = reader.GetString(0),
                Usuario = reader.GetString(1),
                Setor = reader.GetString(2),
                Role = reader.GetString(3)
            });
        }

        return Ok(lista);
    }

    [HttpPost("usuarios")]
    public IActionResult CadastrarUsuario([FromBody] CadastrarUsuarioDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Usuario))
            return BadRequest("Nome de usuário é obrigatório.");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Usuarios (Id, Usuario, Senha, Setor, Role)
            VALUES (@id, @usuario, @senha, @setor, @role);";

        command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("@usuario", dto.Usuario.Trim());
        command.Parameters.AddWithValue("@senha", string.IsNullOrWhiteSpace(dto.Senha) ? "123456" : dto.Senha);
        command.Parameters.AddWithValue("@setor", string.IsNullOrWhiteSpace(dto.Setor) ? "Vendas" : dto.Setor);
        command.Parameters.AddWithValue("@role", string.IsNullOrWhiteSpace(dto.Role) ? "Operador" : dto.Role);

        try
        {
            command.ExecuteNonQuery();
            return Ok();
        }
        catch
        {
            return BadRequest("Usuário já cadastrado.");
        }
    }
}

public class LoginDto
{
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UsuarioResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class CadastrarUsuarioDto
{
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}