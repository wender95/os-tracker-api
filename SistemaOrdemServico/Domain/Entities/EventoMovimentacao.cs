using System;
using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.Domain.Entities;

public class EventoMovimentacao
{
    public Guid Id { get; private set; }
    public Guid FluxoId { get; private set; }
    public SetorEnum Setor { get; private set; }
    public TipoEvento TipoEvento { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? MotivoJustificativa { get; private set; }

    public EventoMovimentacao(Guid fluxoId, SetorEnum setor, TipoEvento tipoEvento, Guid usuarioId, string? motivoJustificativa = null)
    {
        Id = Guid.NewGuid();
        FluxoId = fluxoId;
        Setor = setor;
        TipoEvento = tipoEvento;
        UsuarioId = usuarioId;
        Timestamp = DateTime.UtcNow;
        MotivoJustificativa = motivoJustificativa;
    }

    private EventoMovimentacao() { } // EF Core
}