using System;
using System.Collections.Generic;

namespace SistemaOrdemServico.DTOs;

public record CriarOsRequest(string NumeroOS, string Descricao);

public record OrdemServicoResponseDto(
    Guid Id,
    string NumeroOS,
    string Descricao,
    string Status,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    Guid? FuncionarioId,
    IEnumerable<HistoricoStatusResponseDto> Historico
);

public record HistoricoStatusResponseDto(
    string StatusAnterior,
    string StatusNovo,
    DateTime DataAlteracao,
    string Observacao
);