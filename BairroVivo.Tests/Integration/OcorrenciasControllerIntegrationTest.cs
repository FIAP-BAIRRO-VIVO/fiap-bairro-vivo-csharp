using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BairroVivo.Application.Dtos.Ocorrencias;
using Xunit;

namespace BairroVivo.Tests.Integration
{
    [Collection("Integration")]
    public class OcorrenciasControllerIntegrationTest
    {
        private readonly HttpClient _client;

        public OcorrenciasControllerIntegrationTest(BairroVivoApiFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer", BairroVivoApiFactory.CriarToken(1, "CIDADAO"));
        }

        [Fact]
        [Trait("Integration", "Ocorrencias")]
        public async Task ObterPorId_OcorrenciaExistente_Retorna200()
        {
            // Act
            var response = await _client.GetAsync("/api/ocorrencias/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var ocorrencia = await response.Content.ReadFromJsonAsync<OcorrenciaResponseDto>();

            Assert.NotNull(ocorrencia);
            Assert.Equal("1", ocorrencia.Id);
            Assert.Equal("Buraco na Via", ocorrencia.Categoria.Nome);
            Assert.Equal("ABERTA", ocorrencia.Status);
        }

        [Fact]
        [Trait("Integration", "Ocorrencias")]
        public async Task ObterPorId_OcorrenciaInexistente_Retorna404()
        {
            var response = await _client.GetAsync("/api/ocorrencias/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("Integration", "Ocorrencias")]
        public async Task Atualizar_AutorCidadao_Retorna204EAtualizaCamposPermitidos()
        {
            // Arrange
            var request = new OcorrenciaRequestDto
            {
                CategoriaId = 1,
                MunicipioId = 1,
                Titulo = "Buraco muito fundo na Avenida Paulista",
                Descricao = "Cratera ampliada causando risco grave a pedestres e ciclistas.",
                Latitude = -23.5614m,
                Longitude = -46.6558m,
                EnderecoReferencia = "Avenida Paulista, altura do MASP"
            };

            // Act: Autor (idUsuario 1) atualiza sua ocorrência 1
            var putResponse = await _client.PutAsJsonAsync("/api/ocorrencias/1", request);
            var getResponse = await _client.GetAsync("/api/ocorrencias/1");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var ocorrencia = await getResponse.Content.ReadFromJsonAsync<OcorrenciaResponseDto>();

            Assert.NotNull(ocorrencia);
            Assert.Equal("Buraco muito fundo na Avenida Paulista", ocorrencia.Titulo);
            Assert.Equal("ABERTA", ocorrencia.Status); // Status nunca é alterado via PUT
        }

        [Fact]
        [Trait("Integration", "Ocorrencias")]
        public async Task ListarComFiltroBoundingBox_RetornaApenasAreaVisivel()
        {
            // Arrange: filtro que abrange São Paulo (-23.56, -46.65)
            var url = "/api/ocorrencias?minLat=-24&maxLat=-23&minLon=-47&maxLon=-46";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Buraco fundo na Avenida Paulista", body);
        }
    }
}
