using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaOrdemServico.Controllers;
using SistemaOrdemServico.Domain.Entities;
using SistemaOrdemServico.Domain.Enums;
using SistemaOrdemServico.Domain.Interfaces;
using SistemaOrdemServico.DTOs;
using Xunit;

namespace SistemaOrdemServico.Tests;

public class FluxosControllerTests
{
    private readonly Mock<IFluxoRepository> _mockRepo;
    private readonly FluxosController _controller;

    public FluxosControllerTests()
    {
        _mockRepo = new Mock<IFluxoRepository>();
        _controller = new FluxosController(_mockRepo.Object);
    }

    [Fact]
    public async Task Criar_DeveRetornarCreated_QuandoDtoEhValido()
    {
        // Arrange
        var dto = new CriarFluxoDto("OS-1001", "Fluxo A - Lona", SetorEnum.Impressao, Guid.NewGuid());

        // Act
        var result = await _controller.Criar(dto);

        // Assert
        var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<FluxoResponseDto>(actionResult.Value);
        Assert.Equal("OS-1001", response.NumeroOS);
        Assert.Equal(SetorEnum.Impressao, response.SetorAtual);
        _mockRepo.Verify(r => r.AdicionarAsync(It.IsAny<FluxoOS>()), Times.Once);
    }

    [Fact]
    public async Task Despachar_DeveRetornarBadRequest_QuandoTransicaoForInvalida()
    {
        // Arrange
        var fluxoId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var fluxo = new FluxoOS("OS-1002", "Fluxo B", SetorEnum.Preparacao, vendedorId);

        _mockRepo.Setup(r => r.ObterPorIdAsync(fluxoId)).ReturnsAsync(fluxo);

        // Tentativa inválida: Despachar de Preparação direto para Acabamento (Matriz exige Frota)
        var dtoDespacho = new DespacharFluxoDto(SetorEnum.Acabamento, Guid.NewGuid());

        // Act
        var result = await _controller.Despachar(fluxoId, dtoDespacho);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Transição de setor não permitida", badRequestResult.Value?.ToString());
    }
}
