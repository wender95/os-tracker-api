using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace SistemaOrdemServico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ordensservico.db";

    public AuthController()
    {
        InicializarBanco();
    }

    private void InicializarBanco()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 1. Recria a tabela caso a estrutura antiga não possua a coluna Usuario
        var checkColCmd = connection.CreateCommand();
        checkColCmd.CommandText = "PRAGMA table_info(Usuarios);";
        
        bool temColunaUsuario = false;
        using (var reader = checkColCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var nomeColuna = reader.GetString(1);
                if (nomeColuna.Equals("Usuario", StringComparison.OrdinalIgnoreCase))
                {
                    temColunaUsuario = true;
                    break;
                }
            }
        }

        // Se a tabela existe mas está na versão antiga sem 'Usuario', exclui para recriar
        if (!temColunaUsuario)
        {
            var dropCmd = connection.CreateCommand();
            dropCmd.CommandText = "DROP TABLE IF EXISTS Usuarios;";
            dropCmd.ExecuteNonQuery();
        }

        // 2. Cria a tabela correta
        var createCmd = connection.CreateCommand();
        createCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id TEXT PRIMARY KEY,
                Usuario TEXT UNIQUE COLLATE NOCASE,
                Email TEXT,
                Senha TEXT,
                Setor TEXT,
                Role TEXT
            );";
        createCmd.ExecuteNonQuery();

        // 3. Insere o usuário admin padrão
        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT OR IGNORE INTO Usuarios (Id, Usuario, Email, Senha, Setor, Role)
            VALUES ('admin-id-padrao', 'admin', 'admin@adesipar.com', '123456', 'Criacao', 'Administrador');";
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
        command.CommandText = "SELECT Id, COALESCE(Usuario, Email), Setor, Role FROM Usuarios;";

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
            INSERT INTO Usuarios (Id, Usuario, Email, Senha, Setor, Role)
            VALUES (@id, @usuario, @email, @senha, @setor, @role);";

        command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("@usuario", dto.Usuario.Trim());
        command.Parameters.AddWithValue("@email", dto.Usuario.Trim() + "@adesipar.com");
        command.Parameters.AddWithValue("@senha", string.IsNullOrWhiteSpace(dto.Senha) ? "123456" : dto.Senha);
        command.Parameters.AddWithValue("@setor", string.IsNullOrWhiteSpace(dto.Setor) ? "Criacao" : dto.Setor);
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