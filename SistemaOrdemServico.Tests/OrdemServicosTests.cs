using System;
using FluentAssertions;
using SistemaOrdemServico.Domain.Entities;
using SistemaOrdemServico.Domain.Enums;
using Xunit;

namespace SistemaOrdemServico.Tests;

public class OrdemServicoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveIniciarComStatusAberta()
    {
        // Arrange & Act
        var os = new OrdemServico("OS-001", "Instalação de ramal");

        // Assert (utilizando FluentAssertions)
        os.Status.Should().Be(StatusOrdemServico.Aberta);
        os.NumeroOS.Should().Be("OS-001");
        os.DataFechamento.Should().BeNull();
    }

    [Fact]
    public void Concluir_SemFuncionarioAtribuido_DeveLancarExcecao()
    {
        // Arrange
        var os = new OrdemServico("OS-002", "Conserto de impressora");

        // Act
        Action act = () => os.Concluir("Serviço finalizado sem problemas");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("OS não pode ser concluída sem um funcionário atribuído.");
    }

    [Fact]
    public void Concluir_ComFuncionarioAtribuido_DeveMudarStatusParaConcluidai()
    {
        // Arrange
        var os = new OrdemServico("OS-003", "Troca de HD");
        var funcionarioId = Guid.NewGuid();

        // Act
        os.AtribuirFuncionario(funcionarioId);
        os.Concluir("Troca efetuada com sucesso.");

        // Assert
        os.Status.Should().Be(StatusOrdemServico.Concluida);
        os.DataFechamento.Should().NotBeNull();
        os.FuncionarioId.Should().Be(funcionarioId);
    }
}