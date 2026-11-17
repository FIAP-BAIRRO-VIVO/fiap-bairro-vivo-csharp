using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BairroVivo.Tests.Integration;

[Collection("Integration")]
public class HateoasPaginacaoHealthTest
{
    private readonly BairroVivoApiFactory _factory;

    public HateoasPaginacaoHealthTest(BairroVivoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListarMinhasOcorrencias_ComPaginacao_RetornaMetadadosELinksHateoas()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", BairroVivoApiFactory.CriarToken(1, "CIDADAO"));

        // Act
        var resposta = await client.GetAsync("/api/v1/ocorrencias/minhas?pagina=1&tamanho=5&ordem=recentes");
        var json = await resposta.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(1, doc.RootElement.GetProperty("pagina").GetInt32());
        Assert.Equal(5, doc.RootElement.GetProperty("tamanho").GetInt32());
        Assert.True(doc.RootElement.GetProperty("total").GetInt32() >= 1);
        Assert.NotEmpty(doc.RootElement.GetProperty("links").EnumerateArray());
        var primeiroItem = doc.RootElement.GetProperty("dados").EnumerateArray().First();
        Assert.NotEmpty(primeiroItem.GetProperty("links").EnumerateArray());
    }

    [Fact]
    public async Task ListarOcorrenciasPublicas_ComFiltroMunicipioEBoundingBox_Retorna200ComLinksHateoas()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/v1/ocorrencias?pagina=1&tamanho=10&idMunicipio=1&minLat=-24&maxLat=-23&minLon=-47&maxLon=-46");
        var json = await resposta.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("total").GetInt32() >= 1);
        Assert.NotEmpty(doc.RootElement.GetProperty("links").EnumerateArray());
    }

    [Fact]
    public async Task HealthLive_SemToken_Retorna200Healthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task ApiHealthLive_SemToken_Retorna200Healthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}
