using System;
using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.DTOs;

public class CriarFluxoDto
{
    public string NumeroOS { get; set; } = string.Empty;
    public string IdentificadorFluxo { get; set; } = string.Empty;
    public SetorEnum SetorInicial { get; set; }
    public Guid CriadoPorId { get; set; }

    public CriarFluxoDto() { }

    public CriarFluxoDto(string numeroOS, string identificadorFluxo, SetorEnum setorInicial, Guid criadoPorId)
    {
        NumeroOS = numeroOS;
        IdentificadorFluxo = identificadorFluxo;
        SetorInicial = setorInicial;
        CriadoPorId = criadoPorId;
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

public class FluxoResponseDto
{
    public Guid Id { get; set; }
    public string NumeroOS { get; set; } = string.Empty;
    public string IdentificadorFluxo { get; set; } = string.Empty;
    public SetorEnum SetorAtual { get; set; }
    public StatusFluxo Status { get; set; }
}
