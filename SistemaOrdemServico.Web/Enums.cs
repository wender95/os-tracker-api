namespace SistemaOrdemServico.Domain.Enums;

public enum SetorEnum
{
    Vendas,
    Criacao,
    Impressao,
    Recorte,
    Preparacao,
    Acabamento,
    Frota,
    Prateleira,
    Patio,
    Financeiro
}

public enum StatusFluxo
{
    AguardandoRecebimento,
    EmExecucao,
    Concluido,
    Cancelado
}