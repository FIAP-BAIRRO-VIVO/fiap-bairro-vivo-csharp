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
        private readonly AtualizacaoUseCase _atualizacaoUseCase;

        public AtualizacaoUseCaseTest()
        {
            _atualizacaoRepositoryMock = new Mock<IAtualizacaoRepository>();
            _ocorrenciaRepositoryMock = new Mock<IOcorrenciaRepository>();
            _atualizacaoUseCase = new AtualizacaoUseCase(
                _atualizacaoRepositoryMock.Object,
                _ocorrenciaRepositoryMock.Object);
        }

        [Fact]
        [Trait("AtualizacaoUseCase", "Registrar")]
        public void Registrar_VotoDuplicadoMesmoUsuario_LancaInvalidOperationException()
        {
            // Arrange
            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                Status = "ABERTA",
                Urgencia = "MEDIA",
                Confirmacoes = 1
            };

            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(1)).Returns(ocorrencia);
            _ocorrenciaRepositoryMock.Setup(r => r.ExisteUsuario(2)).Returns(true);
            _atualizacaoRepositoryMock.Setup(r => r.JaVotou(1, 2, "CONFIRMACAO")).Returns(true);

            var dto = new AtualizacaoRequestDto
            {
                Tipo = "CONFIRMACAO",
                Comentario = "Ainda está lá!"
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _atualizacaoUseCase.Registrar(1, 2, dto));
        }

        [Fact]
        [Trait("AtualizacaoUseCase", "Registrar")]
        public void Registrar_TerceiraConfirmacao_ValidaComunidadeEElevaUrgencia()
        {
            // Arrange
            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                Status = "ABERTA",
                Urgencia = "MEDIA",
                Confirmacoes = 2,
                ValidadaComunidade = "N"
            };

            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(1)).Returns(ocorrencia);
            _ocorrenciaRepositoryMock.Setup(r => r.ExisteUsuario(3)).Returns(true);
            _atualizacaoRepositoryMock.Setup(r => r.JaVotou(1, 3, "CONFIRMACAO")).Returns(false);
            _atualizacaoRepositoryMock
                .Setup(r => r.Registrar(It.IsAny<AtualizacaoOcorrenciaEntity>()))
                .Returns((AtualizacaoOcorrenciaEntity a) =>
                {
                    a.IdAtualizacao = 10;
                    return a;
                });

            var dto = new AtualizacaoRequestDto
            {
                Tipo = "CONFIRMACAO",
                Comentario = "Terceiro morador confirmando que ainda está lá."
            };

            // Act
            var resultado = _atualizacaoUseCase.Registrar(1, 3, dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(3, ocorrencia.Confirmacoes);
            Assert.Equal("S", ocorrencia.ValidadaComunidade);
            Assert.Equal("ALTA", ocorrencia.Urgencia);
            _ocorrenciaRepositoryMock.Verify(r => r.Atualizar(ocorrencia), Times.Once);
        }
    }
}
