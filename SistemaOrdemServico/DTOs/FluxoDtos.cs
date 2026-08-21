namespace SistemaOrdemServico.DTOs;

using SistemaOrdemServico.Domain.Enums;

public record CriarFluxoDto(
    string NumeroOS,
    string IdentificadorFluxo,
    string NomeCliente,
    SetorEnum SetorInicial
);

public record ReceberFluxoDto(Guid UsuarioId);

public record DespacharFluxoDto(
    SetorEnum SetorDestino,
    Guid UsuarioId
);

public class FluxoResponseDto
{
    public Guid Id { get; set; }
    public string NumeroOS { get; set; } = string.Empty;
    public string IdentificadorFluxo { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
    public SetorEnum SetorAtual { get; set; }
    public SetorEnum? SetorAnterior { get; set; }
    public StatusFluxo Status { get; set; }
}

public class EditarFluxoDto
{
    public string NumeroOS { get; set; } = string.Empty;
    public string IdentificadorFluxo { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
}