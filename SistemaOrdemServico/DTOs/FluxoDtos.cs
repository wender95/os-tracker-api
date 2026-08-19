using System;
using SistemaOrdemServico.Domain.Enums; // Ajuste para o namespace correto do seu SetorEnum/StatusFluxo se necessário

namespace SistemaOrdemServico.DTOs;

public class CriarFluxoDto
{
    public string NumeroOS { get; set; } = string.Empty;
    public string IdentificadorFluxo { get; set; } = string.Empty;
    public string? NomeCliente { get; set; }
    public SetorEnum SetorInicial { get; set; }
    public Guid CriadoPorId { get; set; }

    public CriarFluxoDto() { }

    public CriarFluxoDto(string numeroOS, string identificadorFluxo, SetorEnum setorInicial, Guid criadoPorId, string? nomeCliente = null)
    {
        NumeroOS = numeroOS;
        IdentificadorFluxo = identificadorFluxo;
        SetorInicial = setorInicial;
        CriadoPorId = criadoPorId;
        NomeCliente = nomeCliente;
    }
}

public class ReceberFluxoDto
{
    public Guid UsuarioId { get; set; }

    public ReceberFluxoDto() { }

    public ReceberFluxoDto(Guid usuarioId)
    {
        UsuarioId = usuarioId;
    }
}

public class DespacharFluxoDto
{
    public SetorEnum SetorDestino { get; set; }
    public Guid UsuarioId { get; set; }

    public DespacharFluxoDto() { }

    public DespacharFluxoDto(SetorEnum setorDestino, Guid usuarioId)
    {
        SetorDestino = setorDestino;
        UsuarioId = usuarioId;
    }
}

public class CancelarFluxoDto
{
    public Guid UsuarioId { get; set; }
    public string Motivo { get; set; } = string.Empty;

    public CancelarFluxoDto() { }

    public CancelarFluxoDto(Guid usuarioId, string motivo)
    {
        UsuarioId = usuarioId;
        Motivo = motivo;
    }
}

public class FluxoResponseDto
{
    public Guid Id { get; set; }
    public string NumeroOS { get; set; } = string.Empty;
    public string IdentificadorFluxo { get; set; } = string.Empty;
    public string? NomeCliente { get; set; }
    public SetorEnum SetorAtual { get; set; }
    public StatusFluxo Status { get; set; }
}