using System;
using System.Collections.Generic;
using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.DTOs;

public record CriarFluxoDto(
    string NumeroOS,
    string IdentificadorFluxo,
    SetorEnum SetorInicial,
    Guid UsuarioVendedorId
);

public record DespacharFluxoDto(
    SetorEnum SetorDestino,
    Guid UsuarioId
);

public record ReceberFluxoDto(
    Guid UsuarioOperadorId
);

public record CancelarFluxoDto(
    string Motivo,
    Guid UsuarioId
);

public record EventoMovimentacaoResponseDto(
    Guid Id,
    SetorEnum Setor,
    TipoEvento TipoEvento,
    Guid UsuarioId,
    DateTime Timestamp,
    string? MotivoJustificativa
);

public record FluxoResponseDto(
    Guid Id,
    string NumeroOS,
    string IdentificadorFluxo,
    SetorEnum SetorAtual,
    SetorEnum? SetorAnterior,
    StatusFluxo Status,
    DateTime DataCriacao,
    DateTime? DataEncerramento,
    IEnumerable<EventoMovimentacaoResponseDto> Eventos
);