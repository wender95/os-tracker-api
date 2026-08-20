using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Security.Claims;
using SistemaOrdemServico.Domain.Enums;
using SistemaOrdemServico.DTOs;

namespace SistemaOrdemServico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FluxosController : ControllerBase
{
    private static readonly string _caminhoBanco = Path.Combine(Directory.GetCurrentDirectory(), "ordensservico.db");
    private readonly string _connectionString = $"Data Source={_caminhoBanco}";

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
                SetorAnterior INTEGER NULL,
                Status INTEGER
            );";
        command.ExecuteNonQuery();

        try
        {
            var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE FluxosTabela ADD COLUMN SetorAnterior INTEGER NULL;";
            alterCmd.ExecuteNonQuery();
        }
        catch
        {
            // Coluna já existe
        }
    }

    private bool ValidarPermissaoSetor(SetorEnum setorRequerido)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Administrador") return true;

        var setorUsuarioClaim = User.FindFirst("Setor")?.Value;
        if (Enum.TryParse<SetorEnum>(setorUsuarioClaim, out var setorUsuario))
        {
            return setorUsuario == setorRequerido;
        }

        return true;
    }

    [HttpGet]
    public ActionResult<List<FluxoResponseDto>> ObterTodos()
    {
        var lista = new List<FluxoResponseDto>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, SetorAnterior, Status FROM FluxosTabela;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(MapearFluxo(reader));
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
        command.CommandText = "SELECT Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, SetorAnterior, Status FROM FluxosTabela WHERE SetorAtual = @setorId AND Status != 2;";
        command.Parameters.AddWithValue("@setorId", setorId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(MapearFluxo(reader));
        }

        return Ok(lista);
    }

    [HttpGet("concluidas")]
    public ActionResult<List<FluxoResponseDto>> ObterConcluidas()
    {
        var lista = new List<FluxoResponseDto>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, SetorAnterior, Status FROM FluxosTabela WHERE Status = 2 ORDER BY rowid DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(MapearFluxo(reader));
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
            INSERT INTO FluxosTabela (Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, SetorAnterior, Status)
            VALUES (@id, @numeroOS, @identificadorFluxo, @nomeCliente, @setorAtual, NULL, 0);";

        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@numeroOS", numOS);
        command.Parameters.AddWithValue("@identificadorFluxo", identFluxo);
        command.Parameters.AddWithValue("@nomeCliente", nomeCliente);
        command.Parameters.AddWithValue("@setorAtual", (int)dto.SetorInicial);

        command.ExecuteNonQuery();

        return Ok(new FluxoResponseDto
        {
            Id = id,
            NumeroOS = numOS,
            IdentificadorFluxo = identFluxo,
            NomeCliente = nomeCliente,
            SetorAtual = dto.SetorInicial,
            SetorAnterior = null,
            Status = (StatusFluxo)0
        });
    }

    [HttpPost("{id}/receber")]
    public IActionResult Receber(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE FluxosTabela SET Status = 1 WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id.ToString());

        var linhasAfetadas = command.ExecuteNonQuery();
        if (linhasAfetadas == 0) return NotFound("OS não encontrada.");

        return Ok();
    }

    [HttpPost("{id}/despachar")]
    public IActionResult Despachar(Guid id, [FromBody] DespacharFluxoDto dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT SetorAtual, SetorAnterior FROM FluxosTabela WHERE Id = @id;";
        selectCmd.Parameters.AddWithValue("@id", id.ToString());

        SetorEnum setorAtual;
        SetorEnum? setorAnterior = null;

        using (var reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read()) return NotFound("OS não encontrada.");

            setorAtual = (SetorEnum)reader.GetInt32(0);
            if (!reader.IsDBNull(1))
            {
                setorAnterior = (SetorEnum)reader.GetInt32(1);
            }
        }

        if (!ValidarTransicaoSetor(setorAtual, dto.SetorDestino, setorAnterior))
        {
            return BadRequest($"O setor '{setorAtual}' não possui permissão para despachar para '{dto.SetorDestino}'.");
        }

        var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE FluxosTabela 
            SET SetorAnterior = SetorAtual, 
                SetorAtual = @setorDestino, 
                Status = 0 
            WHERE Id = @id;";

        updateCmd.Parameters.AddWithValue("@id", id.ToString());
        updateCmd.Parameters.AddWithValue("@setorDestino", (int)dto.SetorDestino);

        updateCmd.ExecuteNonQuery();
        return Ok();
    }

    [HttpPost("{id}/concluir")]
    public IActionResult Concluir(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE FluxosTabela SET Status = 2 WHERE Id = @id AND SetorAtual = @setorFinanceiro;";
        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@setorFinanceiro", (int)SetorEnum.Financeiro);

        var linhasAfetadas = command.ExecuteNonQuery();
        if (linhasAfetadas == 0) 
            return BadRequest("Não foi possível concluir. Apenas OSs no setor Financeiro podem ser finalizadas.");

        return Ok();
    }

    private bool ValidarTransicaoSetor(SetorEnum origem, SetorEnum destino, SetorEnum? setorAnterior)
    {
        if (origem == SetorEnum.Financeiro) return false;

        if (setorAnterior.HasValue && destino == setorAnterior.Value)
        {
            return true;
        }

        return origem switch
        {
            SetorEnum.Vendas => destino is SetorEnum.Criacao or SetorEnum.Recorte or SetorEnum.Impressao or SetorEnum.Preparacao or SetorEnum.Frota or SetorEnum.Acabamento,
            SetorEnum.Criacao => destino is SetorEnum.Recorte or SetorEnum.Impressao or SetorEnum.Preparacao or SetorEnum.Frota or SetorEnum.Acabamento,
            SetorEnum.Impressao => destino is SetorEnum.Recorte or SetorEnum.Preparacao or SetorEnum.Frota or SetorEnum.Acabamento,
            SetorEnum.Recorte => destino is SetorEnum.Preparacao or SetorEnum.Frota or SetorEnum.Acabamento,
            SetorEnum.Preparacao => destino == SetorEnum.Frota,
            SetorEnum.Acabamento => destino == SetorEnum.Prateleira,
            SetorEnum.Frota => destino == SetorEnum.Patio,
            SetorEnum.Prateleira => destino == SetorEnum.Financeiro,
            SetorEnum.Patio => destino == SetorEnum.Financeiro,
            _ => false
        };
    }

    private static FluxoResponseDto MapearFluxo(SqliteDataReader reader)
    {
        return new FluxoResponseDto
        {
            Id = Guid.Parse(reader.GetString(0)),
            NumeroOS = reader.IsDBNull(1) ? "" : reader.GetString(1),
            IdentificadorFluxo = reader.IsDBNull(2) ? "" : reader.GetString(2),
            NomeCliente = reader.IsDBNull(3) ? "" : reader.GetString(3),
            SetorAtual = (SetorEnum)reader.GetInt32(4),
            SetorAnterior = reader.IsDBNull(5) ? null : (SetorEnum)reader.GetInt32(5),
            Status = (StatusFluxo)reader.GetInt32(6)
        };
    }
}