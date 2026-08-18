using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaOrdemServico.Data;
using SistemaOrdemServico.Domain.Entities;
using SistemaOrdemServico.Domain.Enums;
using SistemaOrdemServico.Domain.Interfaces;

namespace SistemaOrdemServico.Repositories;

public class FluxoRepository : IFluxoRepository
{
    private readonly AppDbContext _context;

    public FluxoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(FluxoOS fluxo)
    {
        await _context.FluxosOS.AddAsync(fluxo);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(FluxoOS fluxo)
    {
        _context.FluxosOS.Update(fluxo);
        await _context.SaveChangesAsync();
    }

    public async Task<FluxoOS?> ObterPorIdAsync(Guid id)
    {
        return await _context.FluxosOS
            .Include(f => f.Eventos)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<FluxoOS>> ObterTodosAsync()
    {
        return await _context.FluxosOS
            .Include(f => f.Eventos)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<FluxoOS>> ObterFilaDoSetorAsync(SetorEnum setor)
    {
        return await _context.FluxosOS
            .Include(f => f.Eventos)
            .Where(f => f.SetorAtual == setor && f.Status != StatusFluxo.Encerrado && f.Status != StatusFluxo.Cancelado)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<FluxoOS>> ObterPorNumeroOSAsync(string numeroOS)
    {
        return await _context.FluxosOS
            .Include(f => f.Eventos)
            .Where(f => f.NumeroOS == numeroOS)
            .AsNoTracking()
            .ToListAsync();
    }
}