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

    private HttpClient ClienteAutenticado(long userId, string perfil)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", BairroVivoApiFactory.CriarToken(userId, perfil));
        return client;
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_Retorna401()
    {
        var client = _factory.CreateClient();
        var resposta = await client.GetAsync("/api/v1/ocorrencias/minhas");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task ModificarOcorrenciaDeOutroCidadao_SemSerAutor_Retorna403()
    {
        // Cidadão 2 tenta editar a ocorrência 1 (pertencente ao cidadão 1)
        var client = ClienteAutenticado(2, "CIDADAO");

        var resposta = await client.PutAsJsonAsync(
            "/api/v1/ocorrencias/1",
            new
            {
                categoriaId = 1,
                municipioId = 1,
                titulo = "Tentativa indevida de edição",
                descricao = "Descrição detalhada de teste para rejeição 403."
            });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task AdminNaoPodeAbrirOcorrencias_Retorna403()
    {
        // Administrador (usuário 3) tenta abrir ocorrência: proibido pelas regras de domínio
        var client = ClienteAutenticado(3, "ADMIN");

        var resposta = await client.PostAsJsonAsync(
            "/api/v1/ocorrencias",
            new
            {
                categoriaId = 1,
                municipioId = 1,
                titulo = "Abertura por administrador",
                descricao = "Tentativa de abertura de ocorrência por perfil administrativo."
            });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task AdminConsultaRelatorios_Retorna200()
    {
        var client = ClienteAutenticado(3, "ADMIN");
        var resposta = await client.GetAsync("/api/v1/admin/relatorios/ocorrencias");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task AnonimatoPublico_OcorrenciaComFlAnonimoS_MascaraAutorComoCidadaoAnonimo()
    {
        var client = _factory.CreateClient();
        var resposta = await client.GetAsync("/api/v1/ocorrencias/2");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        Assert.Equal("Cidadao anonimo", doc.RootElement.GetProperty("autorExibicao").GetString());
    }

    [Fact]
    public async Task VotacaoComunitaria_DoisVotosResolvido_EncerraOcorrenciaEElevaCiclo()
    {
        // Criar uma nova ocorrência com o Cidadão 1 para testar o ciclo completo de votos
        var autor = ClienteAutenticado(1, "CIDADAO");
        var novaOcResp = await autor.PostAsJsonAsync("/api/v1/ocorrencias", new
        {
            categoriaId = 1,
            municipioId = 1,
            titulo = "Semáforo piscando em amarelo",
            descricao = "Cruzamento perigoso com semáforo em pane há horas.",
            latitude = -23.5614m,
            longitude = -46.6558m,
            enderecoReferencia = "Esquina com Alameda Santos"
        });

        Assert.Equal(HttpStatusCode.Created, novaOcResp.StatusCode);
        using var ocDoc = JsonDocument.Parse(await novaOcResp.Content.ReadAsStringAsync());
        var idNova = ocDoc.RootElement.GetProperty("id").GetString()!;

        // Voto 1: Cidadão 1 vota RESOLVIDO
        var cidadao1 = ClienteAutenticado(1, "CIDADAO");
        var voto1 = await cidadao1.PostAsJsonAsync($"/api/v1/ocorrencias/{idNova}/atualizacoes", new
        {
            tipo = "RESOLVIDO",
            texto = "A equipe da CET acabou de arrumar o semáforo."
        });
        Assert.Equal(HttpStatusCode.Created, voto1.StatusCode);

        // Tentativa de voto duplicado no mesmo ciclo pelo Cidadão 1 -> 409 Conflict
        var votoDuplicado = await cidadao1.PostAsJsonAsync($"/api/v1/ocorrencias/{idNova}/atualizacoes", new
        {
            tipo = "RESOLVIDO",
            texto = "Confirmando novamente que o semáforo foi arrumado."
        });
        Assert.Equal(HttpStatusCode.Conflict, votoDuplicado.StatusCode);

        // Voto 2: Cidadão 2 vota RESOLVIDO -> atinge os 2 votos necessários e transiciona para RESOLVIDA no ciclo 2
        var cidadao2 = ClienteAutenticado(2, "CIDADAO");
        var voto2 = await cidadao2.PostAsJsonAsync($"/api/v1/ocorrencias/{idNova}/atualizacoes", new
        {
            tipo = "RESOLVIDO",
            texto = "Também passei por lá e o trânsito está fluindo normalmente."
        });
        Assert.Equal(HttpStatusCode.Created, voto2.StatusCode);

        // Verifica estado atualizado da ocorrência
        var consulta = await _factory.CreateClient().GetAsync($"/api/v1/ocorrencias/{idNova}");
        using var consultaDoc = JsonDocument.Parse(await consulta.Content.ReadAsStringAsync());

        Assert.Equal("RESOLVIDA", consultaDoc.RootElement.GetProperty("status").GetString());
        Assert.Equal(2, consultaDoc.RootElement.GetProperty("numeroCiclo").GetInt32());
    }
}
