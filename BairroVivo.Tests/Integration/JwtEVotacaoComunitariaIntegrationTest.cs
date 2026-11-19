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

    [Fact]
    public async Task TokenAssinadoComChavePrivadaRealDoJava_ValidaComSucesso()
    {
        var client = _factory.CreateClient();

        var chavePrivadaPem = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC5SCpGFlAtIZbE
3QDDRsIpqF8CNnoLQFp6T07c0lhcjGIiY3Ek4zgTBuvPgv5SOtZ0t4XzVdNwUxRW
E6YljpZ+tqeUY35Ow8mj+l7Stq9Gq/x31AT/G5eVUj8Jb2KSAolx7vrBf/XeAVgv
YnR80TltPCoQ202hbDN+jwHKqH1TdIYtcSppj2FcX0/iHffNJRljhs6+1dTiqffl
IGG5L21Yrln6ldciR/snU0Akzn0zt8v/jnJYREug8GA+gUrndkZ5eA6rJN4tuDbu
261V3+2GdfP0DR8MxaTgq/Op0QR6Rfj4ALbbG+5m3BbkjpW2sbBR94NHJADMWW1e
kkQxsca3AgMBAAECggEABaoGnNDmz5JkzQzijpW1rtdWXDd1IK3tdirBgu+tQtsJ
zvpSRanql25RPC1JgBMLZlML9cXsalUn3H/KRwCmaiZEVj16aYcDvY8UYgWyswrj
83fYVUIvLqHuimfM0CDiuchwAwVZ61kILJ6qZ/xvcH7z3EMQCvzHUDmn+N2Qw0HR
1VGobp2EN68twgBNGkW7VrgSplI7xvGK5R0fDoMFtBXsrYrZ+aq7qI70dApKUN/J
UHcr9BkNzThdrnZLG/ryeYHynsni46/r/nj3zzcc01KQIRx5JGzuN7tqX6MbI9eQ
a2tnPoEmhjb+SxDnujisAwG9S2H4aoTJy8ThaABEdQKBgQDLVOy4xBuE0pigDdY5
XhKw//mKH4XIjvXa3c/Gt5rS0/GP1/T27SI26Wjjx+BnxcvfUDvk1TKr3kICFbwG
7KTwodptHtZgnrQs/C+li8qhuwWVjgwhGoD2oN7wxkRt7ZA5R9sqlzc0ODw6CAHo
BonEhdp5EiNBEB01rkEDn+0bUwKBgQDpRlegUEvTL1LdbCKMNVaP8SOtefP5Apau
e2XLappDGwzOzadDKwbNmJ9KaTq0PAMHKpCI4KoCn9c0dlWEFqpu5Gc+8DoWXq0x
D55CKOc6O9UgZ5aFmAyBL/CFBbth+e7Ee1Vxcaaf0EJkriQvC6gdamVzWfXZjCpJ
47mPKsIejQKBgHGIFLlB/p+WO1Q7BZ94kHU2KC5gVYHbmRlA+s/ZGuCXKO0JRyVM
2OrSPxfYNcubnK8vDJJEHIvsiOzDF/7ByYL2zJBbRJYkZzQrB6umt1jApWiRLOKa
+uE6h+Dmw7ShjgL9xTktv8wvjilU6UOVyvzpREiPwRGGHtknV/PfKzQzAoGAfCcz
+97AfGVzdOVYfcblizaMBoghkSYPjE2gn+Lct1puNqK9osIDnOjd3nyjAcnu0qGU
y0THNx5bK1DPsr1deCeXy6ePQtig1Lu7PvsGFbhCdjyrn+6MuTUKjcG/OSYMLgZ2
MyqJxATTUUJdf6x0jTNTS1FVx7MRzsGkwM3BAwECgYEAmfLDdSIMetvPAB3L1paG
reNAO9tcNn16yEAShT1mRbglgxwoOxQ0pYCg23CgptDLjSOQC76Wk9awhmMxVLiv
y/OJH43cFYnHDYCFr35oEQwSAkDR03H+ogqafvL1AsK1zPzzHObrTvoMdhKGRuB1
/XGP9wa1b577YRpM4Vt9SeM=
-----END PRIVATE KEY-----";

        using var rsaPrivada = System.Security.Cryptography.RSA.Create();
        rsaPrivada.ImportFromPem(chavePrivadaPem.Trim());

        var credenciais = new SigningCredentials(
            new RsaSecurityKey(rsaPrivada),
            SecurityAlgorithms.RsaSha256);

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

