using System;
using SistemaOrdemServico.Domain.Enums;

namespace SistemaOrdemServico.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public TipoPerfil Perfil { get; private set; }
    public SetorEnum? Setor { get; private set; }

    public Usuario(string nome, string email, TipoPerfil perfil, SetorEnum? setor = null)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        Perfil = perfil;
        Setor = setor;
    }

    private Usuario() { } // EF Core
}