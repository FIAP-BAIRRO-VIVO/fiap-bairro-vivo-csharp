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
            // Arrange
            const decimal ocorrenciaId = 1;

            // Act
            var response = await _client.GetAsync($"/api/ocorrencias/{ocorrenciaId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var ocorrencia = await response.Content.ReadFromJsonAsync<OcorrenciaResponseDto>();

            Assert.NotNull(ocorrencia);
            Assert.Equal(1, ocorrencia.IdOcorrencia);
            Assert.Equal("BV-2026-000001", ocorrencia.Protocolo);
            Assert.Equal("Buraco na Via", ocorrencia.Categoria);
        }

        [Fact]
        [Trait("Integration", "Ocorrencias")]
        public async Task ObterPorId_OcorrenciaInexistente_Retorna404()
        {
            // Arrange
            const decimal ocorrenciaId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/ocorrencias/{ocorrenciaId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("Integration", "Ocorrencias")]
        public async Task AtualizarStatus_StatusValido_Retorna204EPersisteAlteracao()
        {
            // Arrange
            const decimal ocorrenciaId = 1;
            var request = new OcorrenciaStatusRequestDto
            {
                Status = "RESOLVIDA",
                Observacao = "Equipe de zeladoria concluiu o reparo."
            };

            // Act
            var patchResponse = await _client.PatchAsJsonAsync(
                $"/api/ocorrencias/{ocorrenciaId}/status",
                request
            );

            var getResponse = await _client.GetAsync($"/api/ocorrencias/{ocorrenciaId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var ocorrencia = await getResponse.Content.ReadFromJsonAsync<OcorrenciaResponseDto>();

            Assert.NotNull(ocorrencia);
            Assert.Equal("RESOLVIDA", ocorrencia.Status);

            // Restaura o estado para nao afetar outros testes
            await _client.PatchAsJsonAsync(
                $"/api/ocorrencias/{ocorrenciaId}/status",
                new OcorrenciaStatusRequestDto { Status = "ABERTA" }
            );
        }
    }
}
