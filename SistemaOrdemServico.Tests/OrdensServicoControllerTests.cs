using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaOrdemServico.Controllers;
using SistemaOrdemServico.Domain.Entities;
using SistemaOrdemServico.Domain.Interfaces;
using SistemaOrdemServico.DTOs; // <--- Importação do DTO
using Xunit;

namespace SistemaOrdemServico.Tests;

public class OrdensServicoControllerTests
{
    private readonly Mock<IOrdemServicoRepository> _repositoryMock;
    private readonly OrdensServicoController _controller;

    public OrdensServicoControllerTests()
    {
        _repositoryMock = new Mock<IOrdemServicoRepository>();
        _controller = new OrdensServicoController(_repositoryMock.Object);
    }

    [Fact]
    public async Task ObterPorId_QuandoOsExiste_DeveRetornarStatus200OK()
    {
        // Arrange
        var osFake = new OrdemServico("OS-55", "Ajuste de configuração");
        
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(osFake.Id))
            .ReturnsAsync(osFake);

        // Act
        var result = await _controller.ObterPorId(osFake.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        
        // MUDANÇA AQUI: Agora validamos se o retorno é o OrdemServicoResponseDto
        var osRetornada = okResult.Value.Should().BeOfType<OrdemServicoResponseDto>().Subject;
        osRetornada.NumeroOS.Should().Be("OS-55");
    }

    [Fact]
    public async Task ObterPorId_QuandoOsNaoExiste_DeveRetornarStatus404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(idInexistente))
            .ReturnsAsync((OrdemServico?)null);

        // Act
        var result = await _controller.ObterPorId(idInexistente);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}