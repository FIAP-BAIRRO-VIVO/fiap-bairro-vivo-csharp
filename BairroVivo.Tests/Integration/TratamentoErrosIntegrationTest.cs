using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BairroVivo.Application.Interfaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace BairroVivo.Tests.Integration;

[Collection("Integration")]
public class TratamentoErrosIntegrationTest
{
    private readonly BairroVivoApiFactory _factory;

    public TratamentoErrosIntegrationTest(BairroVivoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ObterCategorias_FalhaInesperada_Retorna500SemDetalhesInternos()
    {
        // Arrange
        var useCase = new Mock<ICategoriaUseCase>();
        useCase.Setup(servico => servico.ObterTodas())
            .Throws(new Exception("Detalhe interno de infraestrutura Oracle"));

        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICategoriaUseCase>();
                services.AddScoped<ICategoriaUseCase>(_ => useCase.Object);
            }));
        var client = factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/categorias");
        var corpo = await resposta.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Contains("Erro interno da API.", corpo);
        Assert.DoesNotContain("Detalhe interno de infraestrutura Oracle", corpo);
    }

    [Fact]
    public async Task CadastrarOcorrencia_CategoriaInvalida_Retorna400()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", BairroVivoApiFactory.CriarToken(1, "CIDADAO"));
        var dados = new
        {
            idCategoria = 0,
            idMunicipio = 1,
            titulo = "Buraco na rua",
            descricao = "Descrição válida com mais de dez caracteres.",
            latitude = -23.55m,
            longitude = -46.63m,
            bairro = "Centro"
        };

        // Act
        var resposta = await client.PostAsJsonAsync("/api/ocorrencias", dados);
        var corpo = await resposta.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Dados inválidos.", corpo);
    }

    [Fact]
    public async Task CadastrarOcorrencia_ComMesmaIdempotencyKey_EvitaDuplicidadeERetornaReplay()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", BairroVivoApiFactory.CriarToken(1, "CIDADAO"));

        var chaveIdempotencia = $"idem-test-{Guid.NewGuid()}";
        var payload = new
        {
            idCategoria = 1,
            idMunicipio = 1,
            titulo = "Entulho descartado na calçada",
            descricao = "Grande quantidade de entulho bloqueando a passagem de pedestres.",
            urgencia = "MEDIA",
            latitude = -23.5620m,
            longitude = -46.6560m,
            bairro = "Bela Vista"
        };

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/ocorrencias")
        {
            Content = JsonContent.Create(payload)
        };
        req1.Headers.Add("Idempotency-Key", chaveIdempotencia);

        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/ocorrencias")
        {
            Content = JsonContent.Create(payload)
        };
        req2.Headers.Add("Idempotency-Key", chaveIdempotencia);

        // Act
        var resp1 = await client.SendAsync(req1);
        var corpo1 = await resp1.Content.ReadAsStringAsync();

        var resp2 = await client.SendAsync(req2);
        var corpo2 = await resp2.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);
        Assert.True(resp2.Headers.Contains("X-Idempotent-Replay"));
        Assert.Equal(corpo1, corpo2);
    }
}
