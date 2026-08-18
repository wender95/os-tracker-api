using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOrdemServico.Data;

namespace SistemaOrdemServico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly AppDbContext _context;

    public RelatoriosController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Exporta a tabela bruta de eventos para cálculo de KPIs no Power BI/Metabase
    /// </summary>
    [HttpGet("eventos-brutos")]
    public async Task<IActionResult> ObterEventosBrutos()
    {
        var fluxos = await _context.FluxosOS
            .Include(f => f.Eventos)
            .AsNoTracking()
            .ToListAsync();

        var eventosBrutos = fluxos.SelectMany(f => f.Eventos.Select(e => new
        {
            f.NumeroOS,
            FluxoId = f.Id,
            f.IdentificadorFluxo,
            EventoId = e.Id,
            Setor = e.Setor.ToString(),
            TipoEvento = e.TipoEvento.ToString(),
            e.UsuarioId,
            e.Timestamp,
            e.MotivoJustificativa
        }))
        .OrderByDescending(x => x.Timestamp)
        .ToList();

        return Ok(eventosBrutos);
    }
}