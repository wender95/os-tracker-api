using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SistemaOrdemServico.Domain.Entities;
using SistemaOrdemServico.Domain.Enums;
using SistemaOrdemServico.Domain.Interfaces;
using SistemaOrdemServico.Domain.Services;
using SistemaOrdemServico.DTOs;

namespace SistemaOrdemServico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FluxosController : ControllerBase
{
    private readonly IFluxoRepository _repository;

    public FluxosController(IFluxoRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// RF01 - Cria um novo fluxo operacional para uma OS (Vendedor)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FluxoResponseDto>> Criar([FromBody] CriarFluxoDto dto)
    {
        var fluxo = new FluxoOS(dto.NumeroOS, dto.IdentificadorFluxo, dto.SetorInicial, dto.UsuarioVendedorId);
        await _repository.AdicionarAsync(fluxo);

        return CreatedAtAction(nameof(ObterPorId), new { id = fluxo.Id }, MapearParaDto(fluxo));
    }

    /// <summary>
    /// RF02 - Exibe a fila de trabalho do setor logado
    /// </summary>
    [HttpGet("fila-setor/{setor}")]
    public async Task<ActionResult> ObterFilaDoSetor(SetorEnum setor)
    {
        var fluxos = await _repository.ObterFilaDoSetorAsync(setor);
        return Ok(fluxos.Select(MapearParaDto));
    }

    /// <summary>
    /// RF02 - Operador clica em "Receber" na fila do seu setor
    /// </summary>
    [HttpPost("{id}/receber")]
    public async Task<IActionResult> Receber(Guid id, [FromBody] ReceberFluxoDto dto)
    {
        var fluxo = await _repository.ObterPorIdAsync(id);
        if (fluxo == null) return NotFound("Fluxo de OS não encontrado.");

        fluxo.Receber(dto.UsuarioOperadorId);
        await _repository.AtualizarAsync(fluxo);

        return Ok(MapearParaDto(fluxo));
    }

    /// <summary>
    /// RF03 - Operador despacha o fluxo para o próximo setor validando a Matriz de Transição
    /// </summary>
    [HttpPost("{id}/despachar")]
    public async Task<IActionResult> Despachar(Guid id, [FromBody] DespacharFluxoDto dto)
    {
        var fluxo = await _repository.ObterPorIdAsync(id);
        if (fluxo == null) return NotFound("Fluxo de OS não encontrado.");

        // Validação da Matriz de Transição rígida + Regra de Exceção (Retorno)
        bool transicaoValida = MatrizTransicaoService.TransicaoEhValida(fluxo.SetorAtual, dto.SetorDestino, fluxo.SetorAnterior);
        if (!transicaoValida)
        {
            return BadRequest($"Transição de setor não permitida: de {fluxo.SetorAtual} para {dto.SetorDestino}.");
        }

        fluxo.Despachar(dto.SetorDestino, dto.UsuarioId);
        await _repository.AtualizarAsync(fluxo);

        return Ok(MapearParaDto(fluxo));
    }

    /// <summary>
    /// RF06 - Cancelamento do fluxo com justificativa obrigatória
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarFluxoDto dto)
    {
        var fluxo = await _repository.ObterPorIdAsync(id);
        if (fluxo == null) return NotFound("Fluxo de OS não encontrado.");

        fluxo.Cancelar(dto.Motivo, dto.UsuarioId);
        await _repository.AtualizarAsync(fluxo);

        return Ok(MapearParaDto(fluxo));
    }

    /// <summary>
    /// RF05 - Consulta um fluxo por ID com linha do tempo completa
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FluxoResponseDto>> ObterPorId(Guid id)
    {
        var fluxo = await _repository.ObterPorIdAsync(id);
        if (fluxo == null) return NotFound("Fluxo de OS não encontrado.");

        return Ok(MapearParaDto(fluxo));
    }

    /// <summary>
    /// RF01 - Consulta todos os fluxos de uma OS (Split de fluxos)
    /// </summary>
    [HttpGet("os/{numeroOS}")]
    public async Task<ActionResult> ObterPorNumeroOS(string numeroOS)
    {
        var fluxos = await _repository.ObterPorNumeroOSAsync(numeroOS);
        return Ok(fluxos.Select(MapearParaDto));
    }

    private static FluxoResponseDto MapearParaDto(FluxoOS fluxo)
    {
        return new FluxoResponseDto(
            fluxo.Id,
            fluxo.NumeroOS,
            fluxo.IdentificadorFluxo,
            fluxo.SetorAtual,
            fluxo.SetorAnterior,
            fluxo.Status,
            fluxo.DataCriacao,
            fluxo.DataEncerramento,
            fluxo.Eventos.Select(e => new EventoMovimentacaoResponseDto(
                e.Id,
                e.Setor,
                e.TipoEvento,
                e.UsuarioId,
                e.Timestamp,
                e.MotivoJustificativa
            ))
        );
    }
}