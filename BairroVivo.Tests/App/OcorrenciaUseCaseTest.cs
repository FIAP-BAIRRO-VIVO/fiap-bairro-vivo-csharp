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
            long idOcorrencia = 999;
            _ocorrenciaRepositoryMock
                .Setup(r => r.ObterPorId(idOcorrencia))
                .Returns((OcorrenciaEntity?)null);

            var resultado = _ocorrenciaUseCase.ObterPorId(idOcorrencia);

            Assert.Null(resultado);
            _ocorrenciaRepositoryMock.Verify(r => r.ObterPorId(idOcorrencia), Times.Once);
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "Cadastrar")]
        public void Cadastrar_UsuarioInexistente_LancaKeyNotFoundException()
        {
            long idUsuario = 999;
            var dto = new OcorrenciaRequestDto();

            _ocorrenciaRepositoryMock
                .Setup(r => r.ExisteUsuario(idUsuario))
                .Returns(false);

            var exception = Assert.Throws<KeyNotFoundException>(
                () => _ocorrenciaUseCase.Cadastrar(idUsuario, dto)
            );

            Assert.Equal("Usuário não encontrado ou inativo.", exception.Message);
            _ocorrenciaRepositoryMock.Verify(r => r.Cadastrar(It.IsAny<OcorrenciaEntity>()), Times.Never);
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "Cadastrar")]
        public void Cadastrar_DadosValidos_RetornaOcorrenciaAbertaNoCiclo1()
        {
            long idUsuario = 1;
            var dto = new OcorrenciaRequestDto
            {
                CategoriaId = 1,
                MunicipioId = 1,
                Titulo = "Vazamento de água na rua",
                Descricao = "Vazamento intenso na calçada há mais de 24 horas.",
                Latitude = -23.5614m,
                Longitude = -46.6558m,
                EnderecoReferencia = "Avenida Paulista, 1500"
            };

            var categoria = new CategoriaEntity
            {
                IdCategoria = 1,
                Nome = "Vazamento de Água",
                FlAtiva = "S"
            };

            _ocorrenciaRepositoryMock.Setup(r => r.ExisteUsuario(idUsuario)).Returns(true);
            _ocorrenciaRepositoryMock.Setup(r => r.ExisteCategoria(1)).Returns(true);
            _ocorrenciaRepositoryMock.Setup(r => r.ExisteMunicipio(1)).Returns(true);
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
                .Returns((long _) => new OcorrenciaEntity
                {
                    IdOcorrencia = 12,
                    IdUsuario = idUsuario,
                    IdCategoria = 1,
                    IdMunicipio = 1,
                    Titulo = dto.Titulo,
                    Descricao = dto.Descricao,
                    EnderecoReferencia = dto.EnderecoReferencia,
                    Status = "ABERTA",
                    NumCiclo = 1,
                    FlExcluida = "N",
                    Categoria = categoria
                });

            var resultado = _ocorrenciaUseCase.Cadastrar(idUsuario, dto);

            Assert.NotNull(resultado);
            Assert.Equal("12", resultado.Id);
            Assert.Equal("ABERTA", resultado.Status);
            Assert.Equal(1, resultado.NumeroCiclo);
            Assert.Equal("Vazamento de Água", resultado.Categoria.Nome);
            _ocorrenciaRepositoryMock.Verify(
                r => r.RegistrarHistoricoStatus(It.IsAny<HistoricoStatusOcorrenciaEntity>()),
                Times.Once);
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "Atualizar")]
        public void Atualizar_NaoAutor_LancaUnauthorizedAccessException()
        {
            long idOcorrencia = 1;
            var ocorrencia = new OcorrenciaEntity { IdOcorrencia = 1, IdUsuario = 10 };
            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(idOcorrencia)).Returns(ocorrencia);

            var dto = new OcorrenciaRequestDto
            {
                CategoriaId = 1,
                MunicipioId = 1,
                Titulo = "Título válido para atualização",
                Descricao = "Descrição válida para atualização"
            };

            // Act & Assert: usuário 20 tentando atualizar ocorrência do usuário 10
            Assert.Throws<UnauthorizedAccessException>(
                () => _ocorrenciaUseCase.Atualizar(idOcorrencia, 20, dto)
            );
        }

        [Fact]
        [Trait("OcorrenciaUseCase", "Remover")]
        public void Remover_Autor_ExecutaExclusaoLogica()
        {
            long idOcorrencia = 1;
            long idUsuario = 10;
            var ocorrencia = new OcorrenciaEntity { IdOcorrencia = idOcorrencia, IdUsuario = idUsuario };
            _ocorrenciaRepositoryMock.Setup(r => r.ObterPorId(idOcorrencia)).Returns(ocorrencia);

            var removido = _ocorrenciaUseCase.Remover(idOcorrencia, idUsuario);

            Assert.True(removido);
            _ocorrenciaRepositoryMock.Verify(r => r.Remover(ocorrencia), Times.Once);
        }
    }
}
