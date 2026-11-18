using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

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
        var resposta = await client.GetAsync("/api/ocorrencias/minhas");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task ModificarOcorrenciaDeOutroCidadao_SemSerAutor_Retorna403()
    {
        // Cidadão 2 tenta editar a ocorrência 1 (pertencente ao cidadão 1)
        var client = ClienteAutenticado(2, "CIDADAO");

        var resposta = await client.PutAsJsonAsync(
            "/api/ocorrencias/1",
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
            "/api/ocorrencias",
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
        var resposta = await client.GetAsync("/api/admin/relatorios/ocorrencias");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task AnonimatoPublico_OcorrenciaComFlAnonimoS_MascaraAutorComoCidadaoAnonimo()
    {
        var client = _factory.CreateClient();
        var resposta = await client.GetAsync("/api/ocorrencias/2");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        Assert.Equal("Cidadao anonimo", doc.RootElement.GetProperty("autorExibicao").GetString());
    }

    [Fact]
    public async Task VotacaoComunitaria_DoisVotosResolvido_EncerraOcorrenciaEElevaCiclo()
    {
        // Criar uma nova ocorrência com o Cidadão 1 para testar o ciclo completo de votos
        var autor = ClienteAutenticado(1, "CIDADAO");
        var novaOcResp = await autor.PostAsJsonAsync("/api/ocorrencias", new
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
        var voto1 = await cidadao1.PostAsJsonAsync($"/api/ocorrencias/{idNova}/atualizacoes", new
        {
            tipo = "RESOLVIDO",
            texto = "A equipe da CET acabou de arrumar o semáforo."
        });
        Assert.Equal(HttpStatusCode.Created, voto1.StatusCode);

        // Tentativa de voto duplicado no mesmo ciclo pelo Cidadão 1 -> 409 Conflict
        var votoDuplicado = await cidadao1.PostAsJsonAsync($"/api/ocorrencias/{idNova}/atualizacoes", new
        {
            tipo = "RESOLVIDO",
            texto = "Confirmando novamente que o semáforo foi arrumado."
        });
        Assert.Equal(HttpStatusCode.Conflict, votoDuplicado.StatusCode);

        // Voto 2: Cidadão 2 vota RESOLVIDO -> atinge os 2 votos necessários e transiciona para RESOLVIDA no ciclo 2
        var cidadao2 = ClienteAutenticado(2, "CIDADAO");
        var voto2 = await cidadao2.PostAsJsonAsync($"/api/ocorrencias/{idNova}/atualizacoes", new
        {
            tipo = "RESOLVIDO",
            texto = "Também passei por lá e o trânsito está fluindo normalmente."
        });
        Assert.Equal(HttpStatusCode.Created, voto2.StatusCode);

        // Verifica estado atualizado da ocorrência
        var consulta = await _factory.CreateClient().GetAsync($"/api/ocorrencias/{idNova}");
        using var consultaDoc = JsonDocument.Parse(await consulta.Content.ReadAsStringAsync());

        Assert.Equal("RESOLVIDA", consultaDoc.RootElement.GetProperty("status").GetString());
        Assert.Equal(2, consultaDoc.RootElement.GetProperty("numeroCiclo").GetInt32());
    }

    [Fact]
    public async Task GerarTokenDev_EmiteTokenRS256ComContratoJava_EValidaEmEndpointProtegido()
    {
        var client = _factory.CreateClient();

        // 1. Gera token RS256 pelo endpoint de autenticação
        var responseToken = await client.PostAsJsonAsync("/api/auth/token", new
        {
            idUsuario = 1,
            email = "maria@bairrovivo.com",
            perfil = "CIDADAO"
        });

        Assert.Equal(HttpStatusCode.OK, responseToken.StatusCode);
        using var docToken = JsonDocument.Parse(await responseToken.Content.ReadAsStringAsync());
        var tokenString = docToken.RootElement.GetProperty("token").GetString()!;

        // 2. Inspeciona cabeçalho e claims do token
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        Assert.Equal("RS256", jwt.Header.Alg);
        Assert.Equal("bairro-vivo-java", jwt.Issuer);
        Assert.Contains("bairro-vivo-apis", jwt.Audiences);
        Assert.Equal("1", jwt.Claims.First(c => c.Type == "sub").Value);
        Assert.Equal("CIDADAO", jwt.Claims.First(c => c.Type == "roles").Value);

        // 3. Usa o token RS256 para chamar endpoint protegido
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/ocorrencias/minhas");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

        var responseProtegido = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, responseProtegido.StatusCode);
    }

    [Fact]
    public async Task TokenExclusivoJavaComSubERoles_IdentificaUsuarioEPerfilCorretamente()
    {
        var client = _factory.CreateClient();

        // Emite token contendo estritamente as claims sub e roles definidas no contrato Java
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(BairroVivoApiFactory.ChaveTeste)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("sub", "1"),
            new("roles", "CIDADAO")
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "bairro-vivo-java",
            audience: "bairro-vivo-apis",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credenciais);

        var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/ocorrencias/minhas");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

