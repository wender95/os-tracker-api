using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SistemaOrdemServico.Domain.Enums;
using SistemaOrdemServico.DTOs;

namespace SistemaOrdemServico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FluxosController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ordensservico.db";

    public FluxosController()
    {
        CriarTabelaSeNaoExistir();
    }

    private void CriarTabelaSeNaoExistir()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS FluxosTabela (
                Id TEXT PRIMARY KEY,
                NumeroOS TEXT,
                IdentificadorFluxo TEXT,
                NomeCliente TEXT,
                SetorAtual INTEGER,
                Status INTEGER
            );";
        command.ExecuteNonQuery();
    }

    [HttpGet]
    public ActionResult<List<FluxoResponseDto>> ObterTodos()
    {
        var lista = new List<FluxoResponseDto>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, Status FROM FluxosTabela;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new FluxoResponseDto
            {
                Id = Guid.Parse(reader.GetString(0)),
                NumeroOS = reader.IsDBNull(1) ? "" : reader.GetString(1),
                IdentificadorFluxo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                NomeCliente = reader.IsDBNull(3) ? "" : reader.GetString(3),
                SetorAtual = (SetorEnum)reader.GetInt32(4),
                Status = (StatusFluxo)reader.GetInt32(5)
            });
        }

        return Ok(lista);
    }

    [HttpGet("setor/{setorId}")]
    public ActionResult<List<FluxoResponseDto>> ObterPorSetor(int setorId)
    {
        var lista = new List<FluxoResponseDto>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, Status FROM FluxosTabela WHERE SetorAtual = @setorId;";
        command.Parameters.AddWithValue("@setorId", setorId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new FluxoResponseDto
            {
                Id = Guid.Parse(reader.GetString(0)),
                NumeroOS = reader.IsDBNull(1) ? "" : reader.GetString(1),
                IdentificadorFluxo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                NomeCliente = reader.IsDBNull(3) ? "" : reader.GetString(3),
                SetorAtual = (SetorEnum)reader.GetInt32(4),
                Status = (StatusFluxo)reader.GetInt32(5)
            });
        }

        return Ok(lista);
    }

    [HttpPost]
    public ActionResult<FluxoResponseDto> Criar([FromBody] CriarFluxoDto dto)
    {
        if (dto == null) return BadRequest("Dados inválidos.");

        var id = Guid.NewGuid();
        var numOS = dto.NumeroOS ?? string.Empty;
        var identFluxo = dto.IdentificadorFluxo ?? string.Empty;
        var nomeCliente = dto.NomeCliente ?? string.Empty;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO FluxosTabela (Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, Status)
            VALUES (@id, @numeroOS, @identificadorFluxo, @nomeCliente, @setorAtual, @status);";

        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@numeroOS", numOS);
        command.Parameters.AddWithValue("@identificadorFluxo", identFluxo);
        command.Parameters.AddWithValue("@nomeCliente", nomeCliente);
        command.Parameters.AddWithValue("@setorAtual", (int)dto.SetorInicial);
        command.Parameters.AddWithValue("@status", 0);

        command.ExecuteNonQuery();

        var resposta = new FluxoResponseDto
        {
            Id = id,
            NumeroOS = numOS,
            IdentificadorFluxo = identFluxo,
            NomeCliente = nomeCliente,
            SetorAtual = dto.SetorInicial,
            Status = (StatusFluxo)0
        };

        return Ok(resposta);
    }

    [HttpGet("{id}")]
    public ActionResult<FluxoResponseDto> ObterPorId(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, Status FROM FluxosTabela WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return Ok(new FluxoResponseDto
            {
                Id = Guid.Parse(reader.GetString(0)),
                NumeroOS = reader.IsDBNull(1) ? "" : reader.GetString(1),
                IdentificadorFluxo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                NomeCliente = reader.IsDBNull(3) ? "" : reader.GetString(3),
                SetorAtual = (SetorEnum)reader.GetInt32(4),
                Status = (StatusFluxo)reader.GetInt32(5)
            });
        }

        return NotFound();
    }

    [HttpPost("{id}/receber")]
    public IActionResult Receber(Guid id, [FromBody] ReceberFluxoDto dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE FluxosTabela SET Status = 1 WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id.ToString());

        var linhasAfetadas = command.ExecuteNonQuery();
        if (linhasAfetadas == 0) return NotFound();

        return Ok();
    }

    [HttpPost("{id}/despachar")]
    public IActionResult Despachar(Guid id, [FromBody] DespacharFluxoDto dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE FluxosTabela SET SetorAtual = @setorDestino, Status = 0 WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@setorDestino", (int)dto.SetorDestino);

        var linhasAfetadas = command.ExecuteNonQuery();
        if (linhasAfetadas == 0) return NotFound();

        return Ok();
    }
}