using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaOrdemServico.Domain.Entities;
using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.Domain.Interfaces;

public interface IFluxoRepository
{
    Task AdicionarAsync(FluxoOS fluxo);
    Task AtualizarAsync(FluxoOS fluxo);
    Task<FluxoOS?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<FluxoOS>> ObterTodosAsync();
    Task<IEnumerable<FluxoOS>> ObterFilaDoSetorAsync(SetorEnum setor);
    Task<IEnumerable<FluxoOS>> ObterPorNumeroOSAsync(string numeroOS);
}