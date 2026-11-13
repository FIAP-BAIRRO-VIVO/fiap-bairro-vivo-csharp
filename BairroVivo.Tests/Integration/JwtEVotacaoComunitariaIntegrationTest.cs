using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BairroVivo.Tests.Integration;

[Collection("Integration")]
public class JwtEVotacaoComunitariaIntegrationTest
{
    private readonly BairroVivoApiFactory _factory;

    public JwtEVotacaoComunitariaIntegrationTest(BairroVivoApiFactory factory)
    {
        _factory = factory;
    }

    private HttpClient ClienteAutenticado(decimal userId, string perfil)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", BairroVivoApiFactory.CriarToken(userId, perfil));
        return client;
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_Retorna401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/ocorrencias/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task ModificarOcorrenciaDeOutroCidadao_SemSerAdmin_Retorna403()
    {
        // Arrange: cidadao 2 tenta alterar status da ocorrencia 1 (pertencente ao cidadao 1)
        var client = ClienteAutenticado(2, "CIDADAO");

        // Act
        var resposta = await client.PatchAsJsonAsync(
            "/api/ocorrencias/1/status",
            new { status = "ARQUIVADA" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task ModificarOcorrenciaDeOutroCidadao_ComoAdmin_Retorna204()
    {
        // Arrange: usuario 3 possui perfil ADMIN
        var client = ClienteAutenticado(3, "ADMIN");

        // Act
        var resposta = await client.PatchAsJsonAsync(
            "/api/ocorrencias/1/status",
            new { status = "EM_ANDAMENTO", observacao = "Vistoria agendada pela prefeitura." });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        // Restaura
        await client.PatchAsJsonAsync("/api/ocorrencias/1/status", new { status = "ABERTA" });
    }

    [Fact]
    public async Task VotacaoComunitaria_ConfirmacaoEVotoDuplicado_ValidaComunidadeERetorna409NoSegundoVoto()
    {
        // Arrange: cidadao 2 confirma ("Ainda esta la!") na ocorrencia 1 (que ja possui 2 confirmacoes)
        var client = ClienteAutenticado(2, "CIDADAO");
        var voto = new
        {
            tipo = "CONFIRMACAO",
            comentario = "Passei hoje cedo e o buraco continua aberto!"
        };

        // Act 1: primeiro voto
        var primeiroVoto = await client.PostAsJsonAsync("/api/ocorrencias/1/atualizacoes", voto);

        // Assert 1: 201 Created e ocorrencia 1 passa a ter VALIDADA_COMUNIDADE = S
        Assert.Equal(HttpStatusCode.Created, primeiroVoto.StatusCode);

        var consultaOcorrencia = await client.GetAsync("/api/ocorrencias/1");
        using var doc = JsonDocument.Parse(await consultaOcorrencia.Content.ReadAsStringAsync());
        Assert.Equal("S", doc.RootElement.GetProperty("validadaComunidade").GetString());

        // Act 2: tentativa de voto duplicado pelo mesmo cidadao 2
        var segundoVoto = await client.PostAsJsonAsync("/api/ocorrencias/1/atualizacoes", voto);

        // Assert 2: 409 Conflict
        Assert.Equal(HttpStatusCode.Conflict, segundoVoto.StatusCode);
    }
}
