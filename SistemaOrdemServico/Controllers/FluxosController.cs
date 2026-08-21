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
        
        // Tabela Principal de Fluxos
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS FluxosTabela (
                Id TEXT PRIMARY KEY,
                NumeroOS TEXT,
                IdentificadorFluxo TEXT,
                NomeCliente TEXT,
                SetorAtual INTEGER,
                SetorAnterior INTEGER NULL,
                Status INTEGER,
                DataCriacao DATETIME,
                DataConclusao DATETIME NULL,
                UsuarioCriacao TEXT
            );";
        command.ExecuteNonQuery();

        // Tabela de Logs/Histórico para Alimentação de BI
        var cmdHistorico = connection.CreateCommand();
        cmdHistorico.CommandText = @"
            CREATE TABLE IF NOT EXISTS HistoricoMovimentacoes (
                Id TEXT PRIMARY KEY,
                FluxoId TEXT,
                NumeroOS TEXT,
                SetorOrigem INTEGER NULL,
                SetorDestino INTEGER,
                Acao TEXT, -- 'CRIADA', 'RECEBIDA', 'DESPACHADA', 'CONCLUIDA', 'EDITADA', 'CANCELADA'
                Usuario TEXT,
                DataHora DATETIME
            );";
        cmdHistorico.ExecuteNonQuery();

        // Migration simples para adicionar colunas se o banco já existia
        TentarAdicionarColuna(connection, "FluxosTabela", "SetorAnterior INTEGER NULL");
        TentarAdicionarColuna(connection, "FluxosTabela", "DataCriacao DATETIME");
        TentarAdicionarColuna(connection, "FluxosTabela", "DataConclusao DATETIME NULL");
        TentarAdicionarColuna(connection, "FluxosTabela", "UsuarioCriacao TEXT");
    }

    private void TentarAdicionarColuna(SqliteConnection connection, string tabela, string colunaDefinicao)
    {
        try
        {
            var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = $"ALTER TABLE {tabela} ADD COLUMN {colunaDefinicao};";
            alterCmd.ExecuteNonQuery();
        }
        catch
        {
            // Coluna já existe no SQLite
        }
    }

    private string ObterNomeUsuarioLogado()
    {
        var nome = User.FindFirst(ClaimTypes.Name)?.Value 
                ?? User.FindFirst(ClaimTypes.GivenName)?.Value 
                ?? User.FindFirst("UniqueName")?.Value;

        if (string.IsNullOrEmpty(nome))
        {
            var setor = User.FindFirst("Setor")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            nome = !string.IsNullOrEmpty(setor) ? $"Usuário ({setor})" : (role ?? "Sistema");
        }

        return nome;
    }

    private void RegistrarHistorico(SqliteConnection connection, Guid fluxoId, string numeroOS, SetorEnum? setorOrigem, SetorEnum setorDestino, string acao, string usuario)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO HistoricoMovimentacoes (Id, FluxoId, NumeroOS, SetorOrigem, SetorDestino, Acao, Usuario, DataHora)
            VALUES (@id, @fluxoId, @numeroOS, @setorOrigem, @setorDestino, @acao, @usuario, @dataHora);";

        cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("@fluxoId", fluxoId.ToString());
        cmd.Parameters.AddWithValue("@numeroOS", numeroOS);
        cmd.Parameters.AddWithValue("@setorOrigem", setorOrigem.HasValue ? (object)(int)setorOrigem.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@setorDestino", (int)setorDestino);
        cmd.Parameters.AddWithValue("@acao", acao);
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@dataHora", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        cmd.ExecuteNonQuery();
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
        var usuarioLogado = ObterNomeUsuarioLogado();
        var dataAgora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO FluxosTabela (Id, NumeroOS, IdentificadorFluxo, NomeCliente, SetorAtual, SetorAnterior, Status, DataCriacao, UsuarioCriacao)
            VALUES (@id, @numeroOS, @identificadorFluxo, @nomeCliente, @setorAtual, NULL, 0, @dataCriacao, @usuarioCriacao);";

        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@numeroOS", numOS);
        command.Parameters.AddWithValue("@identificadorFluxo", identFluxo);
        command.Parameters.AddWithValue("@nomeCliente", nomeCliente);
        command.Parameters.AddWithValue("@setorAtual", (int)dto.SetorInicial);
        command.Parameters.AddWithValue("@dataCriacao", dataAgora);
        command.Parameters.AddWithValue("@usuarioCriacao", usuarioLogado);

        command.ExecuteNonQuery();

        // Grava no Histórico para BI
        RegistrarHistorico(connection, id, numOS, null, dto.SetorInicial, "CRIADA", usuarioLogado);

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

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT NumeroOS, SetorAtual FROM FluxosTabela WHERE Id = @id;";
        selectCmd.Parameters.AddWithValue("@id", id.ToString());

        string numeroOS = "";
        SetorEnum setorAtual;

        using (var reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read()) return NotFound("OS não encontrada.");
            numeroOS = reader.GetString(0);
            setorAtual = (SetorEnum)reader.GetInt32(1);
        }

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE FluxosTabela SET Status = 1 WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id.ToString());
        command.ExecuteNonQuery();

        var usuarioLogado = ObterNomeUsuarioLogado();
        // Grava no Histórico para BI
        RegistrarHistorico(connection, id, numeroOS, setorAtual, setorAtual, "RECEBIDA", usuarioLogado);

        return Ok();
    }

    [HttpPost("{id}/despachar")]
    public IActionResult Despachar(Guid id, [FromBody] DespacharFluxoDto dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT NumeroOS, SetorAtual, SetorAnterior FROM FluxosTabela WHERE Id = @id;";
        selectCmd.Parameters.AddWithValue("@id", id.ToString());

        string numeroOS = "";
        SetorEnum setorAtual;
        SetorEnum? setorAnterior = null;

        using (var reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read()) return NotFound("OS não encontrada.");

            numeroOS = reader.GetString(0);
            setorAtual = (SetorEnum)reader.GetInt32(1);
            if (!reader.IsDBNull(2))
            {
                setorAnterior = (SetorEnum)reader.GetInt32(2);
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

        var usuarioLogado = ObterNomeUsuarioLogado();
        // Grava no Histórico para BI
        RegistrarHistorico(connection, id, numeroOS, setorAtual, dto.SetorDestino, "DESPACHADA", usuarioLogado);

        return Ok();
    }

    [HttpPost("{id}/concluir")]
    public IActionResult Concluir(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT NumeroOS, SetorAtual FROM FluxosTabela WHERE Id = @id;";
        selectCmd.Parameters.AddWithValue("@id", id.ToString());

        string numeroOS = "";
        SetorEnum setorAtual = SetorEnum.Financeiro;

        using (var reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read()) return NotFound("OS não encontrada.");
            numeroOS = reader.GetString(0);
            setorAtual = (SetorEnum)reader.GetInt32(1);
        }

        var dataConclusao = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE FluxosTabela SET Status = 2, DataConclusao = @dataConclusao WHERE Id = @id AND SetorAtual = @setorFinanceiro;";
        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@setorFinanceiro", (int)SetorEnum.Financeiro);
        command.Parameters.AddWithValue("@dataConclusao", dataConclusao);

        var linhasAfetadas = command.ExecuteNonQuery();
        if (linhasAfetadas == 0) 
            return BadRequest("Não foi possível concluir. Apenas OSs no setor Financeiro podem ser finalizadas.");

        var usuarioLogado = ObterNomeUsuarioLogado();
        // Grava no Histórico para BI
        RegistrarHistorico(connection, id, numeroOS, setorAtual, setorAtual, "CONCLUIDA", usuarioLogado);

        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult Editar(Guid id, [FromBody] EditarFluxoDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var setorUsuarioClaim = User.FindFirst("Setor")?.Value;

        if (role != "Administrador" && setorUsuarioClaim != "Comercial" && setorUsuarioClaim != "Vendas")
        {
            return StatusCode(403, "Acesso negado: Apenas Administrador e Comercial podem editar a OS.");
        }

        if (dto == null || string.IsNullOrWhiteSpace(dto.NumeroOS))
        {
            return BadRequest("Dados inválidos para edição.");
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Status, SetorAtual FROM FluxosTabela WHERE Id = @id;";
        selectCmd.Parameters.AddWithValue("@id", id.ToString());

        SetorEnum setorAtual;
        using (var reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read()) return NotFound("OS não encontrada.");
            if (reader.GetInt32(0) == 2)
            {
                return BadRequest("Ordens de Serviço concluídas não podem ser alteradas.");
            }
            setorAtual = (SetorEnum)reader.GetInt32(1);
        }

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE FluxosTabela 
            SET NumeroOS = @numeroOS, 
                IdentificadorFluxo = @identificadorFluxo, 
                NomeCliente = @nomeCliente 
            WHERE Id = @id;";

        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@numeroOS", dto.NumeroOS.Trim());
        command.Parameters.AddWithValue("@identificadorFluxo", dto.IdentificadorFluxo?.Trim() ?? "");
        command.Parameters.AddWithValue("@nomeCliente", dto.NomeCliente?.Trim() ?? "");

        command.ExecuteNonQuery();

        var usuarioLogado = ObterNomeUsuarioLogado();
        // Grava no Histórico para BI
        RegistrarHistorico(connection, id, dto.NumeroOS, setorAtual, setorAtual, "EDITADA", usuarioLogado);

        return Ok();
    }

    [HttpDelete("{id}/cancelar")]
    public IActionResult Cancelar(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT NumeroOS, Status, SetorAtual FROM FluxosTabela WHERE Id = @id;";
        selectCmd.Parameters.AddWithValue("@id", id.ToString());

        string numeroOS = "";
        SetorEnum setorAtual;

        using (var reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read()) return NotFound("OS não encontrada.");
            if (reader.GetInt32(1) == 2)
            {
                return BadRequest("Ordens de Serviço concluídas não podem ser canceladas.");
            }
            numeroOS = reader.GetString(0);
            setorAtual = (SetorEnum)reader.GetInt32(2);
        }

        var usuarioLogado = ObterNomeUsuarioLogado();
        // Grava no Histórico para BI antes de excluir
        RegistrarHistorico(connection, id, numeroOS, setorAtual, setorAtual, "CANCELADA", usuarioLogado);

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM FluxosTabela WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id.ToString());

        command.ExecuteNonQuery();
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