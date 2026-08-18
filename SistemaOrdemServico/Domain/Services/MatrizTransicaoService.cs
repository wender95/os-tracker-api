using System.Collections.Generic;
using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.Domain.Services;

public static class MatrizTransicaoService
{
    private static readonly Dictionary<SetorEnum, List<SetorEnum>> DestinosPermitidos = new()
    {
        { SetorEnum.Vendas, new() { SetorEnum.Criacao, SetorEnum.Recorte, SetorEnum.Impressao, SetorEnum.Preparacao, SetorEnum.Frota, SetorEnum.Acabamento } },
        { SetorEnum.Criacao, new() { SetorEnum.Recorte, SetorEnum.Impressao, SetorEnum.Preparacao, SetorEnum.Frota, SetorEnum.Acabamento } },
        { SetorEnum.Impressao, new() { SetorEnum.Recorte, SetorEnum.Preparacao, SetorEnum.Frota, SetorEnum.Acabamento } },
        { SetorEnum.Recorte, new() { SetorEnum.Preparacao, SetorEnum.Frota, SetorEnum.Acabamento } },
        { SetorEnum.Preparacao, new() { SetorEnum.Frota } },
        { SetorEnum.Acabamento, new() { SetorEnum.Prateleira } },
        { SetorEnum.Frota, new() { SetorEnum.Patio } },
        { SetorEnum.Prateleira, new() { SetorEnum.Financeiro } },
        { SetorEnum.Patio, new() { SetorEnum.Financeiro } }
    };

    public static bool TransicaoEhValida(SetorEnum setorOrigem, SetorEnum setorDestino, SetorEnum? setorAnterior)
    {
        // 1. Regra de Exceção: Retorno para o setor de origem imediata
        if (setorAnterior.HasValue && setorDestino == setorAnterior.Value)
            return true;

        // 2. Regra da Matriz Normal
        if (DestinosPermitidos.TryGetValue(setorOrigem, out var destinos))
        {
            return destinos.Contains(setorDestino);
        }

        return false;
    }
}