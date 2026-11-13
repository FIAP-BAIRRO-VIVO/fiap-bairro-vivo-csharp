using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Ocorrencias;
using BairroVivo.Application.UseCases;
using BairroVivo.Domain.Entities;
using Moq;
using Xunit;

namespace BairroVivo.Tests.App
{
    public class OcorrenciaUseCaseTest
    {
        private readonly Mock<IOcorrenciaRepository> _ocorrenciaRepositoryMock;
        private readonly OcorrenciaUseCase _ocorrenciaUseCase;

        public OcorrenciaUseCaseTest()
        {
            _ocorrenciaRepositoryMock = new Mock<IOcorrenciaRepository>();
            _ocorrenciaUseCase = new OcorrenciaUseCase(_ocorrenciaRepositoryMock.Object);
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "ObterPorId")]
        public void ObterPorId_OcorrenciaInexistente_RetornaNull()
        {
            // Arrange
            decimal idOcorrencia = 999;
            _ocorrenciaRepositoryMock
                .Setup(r => r.ObterPorId(idOcorrencia))
                .Returns((OcorrenciaEntity?)null);

            // Act
            var resultado = _ocorrenciaUseCase.ObterPorId(idOcorrencia);

            // Assert
            Assert.Null(resultado);
            _ocorrenciaRepositoryMock.Verify(r => r.ObterPorId(idOcorrencia), Times.Once);
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "Cadastrar")]
        public void Cadastrar_UsuarioInexistente_LancaKeyNotFoundException()
        {
            // Arrange
            decimal idUsuario = 999;
            var dto = new OcorrenciaRequestDto();

            _ocorrenciaRepositoryMock
                .Setup(r => r.ExisteUsuario(idUsuario))
                .Returns(false);

            // Act
            var exception = Assert.Throws<KeyNotFoundException>(
                () => _ocorrenciaUseCase.Cadastrar(idUsuario, dto)
            );

            // Assert
            Assert.Equal("Usuário não encontrado.", exception.Message);
            _ocorrenciaRepositoryMock.Verify(r => r.Cadastrar(It.IsAny<OcorrenciaEntity>()), Times.Never);
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "Cadastrar")]
        public void Cadastrar_DadosValidos_GeraProtocoloERetornaOcorrencia()
        {
            // Arrange
            decimal idUsuario = 1;
            var dto = new OcorrenciaRequestDto
            {
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Vazamento de água na rua",
                Descricao = "Vazamento intenso na calçada há mais de 24 horas.",
                Urgencia = "ALTA",
                Latitude = -23.5614m,
                Longitude = -46.6558m,
                Bairro = "Bela Vista"
            };

            var categoria = new CategoriaEntity
            {
                IdCategoria = 1,
                Nome = "Vazamento de Água",
                SlaDiasPadrao = 5,
                Ativo = "S"
            };

            _ocorrenciaRepositoryMock.Setup(r => r.ExisteUsuario(idUsuario)).Returns(true);
            _ocorrenciaRepositoryMock.Setup(r => r.ObterCategoria(1)).Returns(categoria);
            _ocorrenciaRepositoryMock.Setup(r => r.ExisteMunicipio(1)).Returns(true);
            _ocorrenciaRepositoryMock.Setup(r => r.ObterProximoSequencialProtocolo()).Returns(12);
            _ocorrenciaRepositoryMock
                .Setup(r => r.Cadastrar(It.IsAny<OcorrenciaEntity>()))
                .Returns((OcorrenciaEntity oc) =>
                {
                    oc.IdOcorrencia = 12;
                    oc.Categoria = categoria;
                    return oc;
                });
            _ocorrenciaRepositoryMock
                .Setup(r => r.ObterPorId(12))
                .Returns((decimal _) => new OcorrenciaEntity
                {
                    IdOcorrencia = 12,
                    Protocolo = $"BV-{DateTime.Now:yyyy}-000012",
                    IdUsuario = idUsuario,
                    IdCategoria = 1,
                    IdMunicipio = 1,
                    Titulo = dto.Titulo,
                    Descricao = dto.Descricao,
                    Bairro = dto.Bairro,
                    Status = "ABERTA",
                    Urgencia = "ALTA",
                    Categoria = categoria
                });

            // Act
            var resultado = _ocorrenciaUseCase.Cadastrar(idUsuario, dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(12, resultado.IdOcorrencia);
            Assert.StartsWith("BV-", resultado.Protocolo);
            Assert.Equal("ABERTA", resultado.Status);
            Assert.Equal("Vazamento de Água", resultado.Categoria);
            _ocorrenciaRepositoryMock.Verify(
                r => r.RegistrarHistoricoStatus(It.IsAny<HistoricoStatusOcorrenciaEntity>()),
                Times.Once);
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "AtualizarStatus")]
        public void AtualizarStatus_StatusInvalido_LancaArgumentException()
        {
            // Arrange
            decimal idOcorrencia = 1;
            var ocorrencia = new OcorrenciaEntity { IdOcorrencia = 1, Status = "ABERTA" };
            var dto = new OcorrenciaStatusRequestDto { Status = "INVALIDO" };

            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(idOcorrencia)).Returns(ocorrencia);

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => _ocorrenciaUseCase.AtualizarStatus(idOcorrencia, 1, dto)
            );
            _ocorrenciaRepositoryMock.Verify(r => r.Atualizar(It.IsAny<OcorrenciaEntity>()), Times.Never);
        }
    }
}
