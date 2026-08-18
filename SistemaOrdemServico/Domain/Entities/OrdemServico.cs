using System;
using System.Collections.Generic;
using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.Domain.Entities;

public class FluxoOS
{
    public Guid Id { get; private set; }
    public string NumeroOS { get; private set; } = string.Empty;
    public string IdentificadorFluxo { get; private set; } = string.Empty; // Ex: "Fluxo A - Lona", "Fluxo B - Frota"
    public SetorEnum SetorAtual { get; private set; }
    public SetorEnum? SetorAnterior { get; private set; }
    public StatusFluxo Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataEncerramento { get; private set; }

    private readonly List<EventoMovimentacao> _eventos = new();
    public IReadOnlyCollection<EventoMovimentacao> Eventos => _eventos.AsReadOnly();

    public FluxoOS(string numeroOS, string identificadorFluxo, SetorEnum setorInicial, Guid usuarioVendedorId)
    {
        Id = Guid.NewGuid();
        NumeroOS = numeroOS;
        IdentificadorFluxo = identificadorFluxo;
        SetorAtual = setorInicial;
        SetorAnterior = SetorEnum.Vendas;
        Status = StatusFluxo.AguardandoRecebimento;
        DataCriacao = DateTime.UtcNow;

        _eventos.Add(new EventoMovimentacao(Id, setorInicial, TipoEvento.Criado, usuarioVendedorId));
    }

    private FluxoOS() { } // EF Core

    public void Receber(Guid usuarioOperadorId)
    {
        if (Status != StatusFluxo.AguardandoRecebimento)
            throw new InvalidOperationException("Fluxo já se encontra em atendimento ou foi finalizado.");

        Status = StatusFluxo.EmEmAtendimento;
        _eventos.Add(new EventoMovimentacao(Id, SetorAtual, TipoEvento.Recebido, usuarioOperadorId));
    }

    public void Despachar(SetorEnum setorDestino, Guid usuarioId)
    {
        if (Status == StatusFluxo.Cancelado || Status == StatusFluxo.Encerrado)
            throw new InvalidOperationException("Não é possível despachar um fluxo encerrado ou cancelado.");

        SetorAnterior = SetorAtual;
        SetorAtual = setorDestino;

        if (setorDestino == SetorEnum.Financeiro)
        {
            Status = StatusFluxo.Encerrado;
            DataEncerramento = DateTime.UtcNow;
        }
        else
        {
            Status = StatusFluxo.AguardandoRecebimento;
        }

        _eventos.Add(new EventoMovimentacao(Id, setorDestino, TipoEvento.Despachado, usuarioId));
    }

    public void Cancelar(string motivo, Guid usuarioId)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("A justificativa de cancelamento é obrigatória.");

        Status = StatusFluxo.Cancelado;
        DataEncerramento = DateTime.UtcNow;
        _eventos.Add(new EventoMovimentacao(Id, SetorAtual, TipoEvento.Cancelado, usuarioId, motivo));
    }
}