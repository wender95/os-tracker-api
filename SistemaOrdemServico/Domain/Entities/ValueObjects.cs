using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.Domain.Entities;

public record Funcionario(Guid Id, string Nome, string Cargo);

public record HistoricoStatus(
    StatusOrdemServico StatusAnterior, 
    StatusOrdemServico StatusNovo, 
    DateTime DataAlteracao, 
    string Observacao
);