using System;

namespace SistemaOrdemServico.DTOs;

public record MovimentacaoBrutaDto(
    Guid OrdemServicoId,
    string NumeroOS,
    string Descricao,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    Guid? FuncionarioId,
    string StatusAnterior,
    string StatusNovo,
    DateTime DataMovimentacao,
    string Observacao
);