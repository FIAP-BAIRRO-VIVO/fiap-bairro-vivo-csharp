using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Application.UseCases;
using BairroVivo.Domain.Entities;
using Moq;
using Xunit;

namespace BairroVivo.Tests.App
{
    public class AtualizacaoUseCaseTest
    {
        private readonly Mock<IAtualizacaoRepository> _atualizacaoRepositoryMock;
        private readonly Mock<IOcorrenciaRepository> _ocorrenciaRepositoryMock;
        private readonly Mock<IIntegracaoOracleRepository> _integracaoOracleMock;
        private readonly AtualizacaoUseCase _atualizacaoUseCase;

        public AtualizacaoUseCaseTest()
        {
            _atualizacaoRepositoryMock = new Mock<IAtualizacaoRepository>();
            _ocorrenciaRepositoryMock = new Mock<IOcorrenciaRepository>();
            _integracaoOracleMock = new Mock<IIntegracaoOracleRepository>();
            _atualizacaoUseCase = new AtualizacaoUseCase(
                _atualizacaoRepositoryMock.Object,
                _ocorrenciaRepositoryMock.Object,
                _integracaoOracleMock.Object);
        }

        [Fact]
        [Trait("AtualizacaoUseCase", "RegistrarAsync")]
        public async Task RegistrarAsync_OcorrenciaInexistente_LancaKeyNotFoundException()
        {
            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(999)).Returns((OcorrenciaEntity?)null);

            var dto = new AtualizacaoRequestDto { Tipo = "RESOLVIDO", Texto = "Resolvido" };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _atualizacaoUseCase.RegistrarAsync(999, 1, dto));
        }

        [Fact]
        [Trait("AtualizacaoUseCase", "RegistrarAsync")]
        public async Task RegistrarAsync_UsuarioInativo_LancaKeyNotFoundException()
        {
            var ocorrencia = new OcorrenciaEntity { IdOcorrencia = 1, Status = "ABERTA", NumCiclo = 1, FlExcluida = "N" };
            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(1)).Returns(ocorrencia);
            _ocorrenciaRepositoryMock.Setup(r => r.ExisteUsuario(999)).Returns(false);

            var dto = new AtualizacaoRequestDto { Tipo = "COMENTARIO", Texto = "Comentário" };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _atualizacaoUseCase.RegistrarAsync(1, 999, dto));
        }

        [Fact]
        [Trait("AtualizacaoUseCase", "RegistrarAsync")]
        public async Task RegistrarAsync_VotoResolvidoValido_InvocaIntegracaoOracleERetornaConfirmacoes()
        {
            // Arrange
            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                Status = "ABERTA",
                NumCiclo = 1,
                FlExcluida = "N"
            };

            var atualizacaoSalva = new AtualizacaoOcorrenciaEntity
            {
                IdAtualizacao = 10,
                IdOcorrencia = 1,
                IdUsuario = 2,
                Tipo = "RESOLVIDO",
                Texto = "Problema resolvido pela prefeitura.",
                FlAnonimo = "N",
                DtCriacao = DateTime.UtcNow,
                Ocorrencia = ocorrencia
            };

            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(1)).Returns(ocorrencia);
            _ocorrenciaRepositoryMock.Setup(r => r.ExisteUsuario(2)).Returns(true);
            _integracaoOracleMock
                .Setup(i => i.ExecutarRegistrarAtualizacaoAsync(1, 2, "RESOLVIDO", "Problema resolvido pela prefeitura.", "N"))
                .ReturnsAsync(10);
            _integracaoOracleMock
                .Setup(i => i.ContarVotosAtivosAsync(1, "RESOLVIDO"))
                .ReturnsAsync(1);
            _atualizacaoRepositoryMock
                .Setup(r => r.ObterPorId(10))
                .Returns(atualizacaoSalva);

            var dto = new AtualizacaoRequestDto
            {
                Tipo = "RESOLVIDO",
                Texto = "Problema resolvido pela prefeitura."
            };

            // Act
            var resultado = await _atualizacaoUseCase.RegistrarAsync(1, 2, dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("10", resultado.Id);
            Assert.True(resultado.ContabilizadaComoVoto);
            Assert.Equal(1, resultado.ConfirmacoesNoCiclo);
            _integracaoOracleMock.Verify(
                i => i.ExecutarRegistrarAtualizacaoAsync(1, 2, "RESOLVIDO", "Problema resolvido pela prefeitura.", "N"),
                Times.Once);
        }
    }
}
