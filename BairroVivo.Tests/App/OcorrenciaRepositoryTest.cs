using BairroVivo.Domain.Entities;
using BairroVivo.Infrastructure.Data;
using BairroVivo.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BairroVivo.Tests.App
{
    public class OcorrenciaRepositoryTest
    {
        private ApplicationContext CriarContexto()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationContext(options);
        }

        [Fact]
        [Trait("OcorrenciaRepository", "ObterPorId")]
        public void ObterPorId_OcorrenciaExistente_RetornaOcorrencia()
        {
            // Arrange
            using var context = CriarContexto();

            var municipio = new MunicipioEntity { IdMunicipio = 1, CodigoIbge = 3550308, Nome = "São Paulo", Uf = "SP" };
            var usuario = new UsuarioEntity { IdUsuario = 1, Nome = "Maria Silva", Email = "maria@bairrovivo.com", SenhaHash = "hash", Papel = "CIDADAO", FlAtivo = "S", DtCriacao = DateTime.UtcNow };
            var categoria = new CategoriaEntity { IdCategoria = 1, Nome = "Buraco na Via", FlAtiva = "S" };
            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Buraco fundo na rua",
                Descricao = "Buraco grande causando risco aos motoristas.",
                EnderecoReferencia = "Próximo à praça principal",
                Status = "ABERTA",
                NumCiclo = 1,
                FlExcluida = "N",
                Latitude = -23.5614m,
                Longitude = -46.6558m,
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow,
                Usuario = usuario,
                Categoria = categoria,
                Municipio = municipio
            };

            context.Municipios.Add(municipio);
            context.Usuarios.Add(usuario);
            context.Categorias.Add(categoria);
            context.Ocorrencias.Add(ocorrencia);
            context.SaveChanges();

            var repository = new OcorrenciaRepository(context);

            // Act
            var resultado = repository.ObterPorId(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.IdOcorrencia);
            Assert.NotNull(resultado.Categoria);
            Assert.Equal("Buraco na Via", resultado.Categoria.Nome);
        }

        [Fact]
        [Trait("OcorrenciaRepository", "ObterPorId")]
        public void ObterPorId_OcorrenciaInexistente_RetornaNull()
        {
            using var context = CriarContexto();
            var repository = new OcorrenciaRepository(context);

            var resultado = repository.ObterPorId(999);

            Assert.Null(resultado);
        }

        [Fact]
        [Trait("OcorrenciaRepository", "Cadastrar")]
        public void Cadastrar_OcorrenciaValida_SalvaOcorrencia()
        {
            using var context = CriarContexto();
            var repository = new OcorrenciaRepository(context);

            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Poste apagado",
                Descricao = "Poste sem iluminação há duas semanas.",
                EnderecoReferencia = "Rua dos Pinheiros",
                Status = "ABERTA",
                NumCiclo = 1,
                FlExcluida = "N",
                Latitude = -23.56m,
                Longitude = -46.69m,
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow
            };

            var resultado = repository.Cadastrar(ocorrencia);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.IdOcorrencia);
            var salva = context.Ocorrencias.FirstOrDefault(o => o.IdOcorrencia == 1);
            Assert.NotNull(salva);
            Assert.Equal("Poste apagado", salva.Titulo);
            Assert.Equal("ABERTA", salva.Status);
        }

        [Fact]
        [Trait("OcorrenciaRepository", "Remover")]
        public void Remover_ExecutaExclusaoLogica_SetaFlExcluidaComoS()
        {
            using var context = CriarContexto();
            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Calçada quebrada",
                Descricao = "Calçada danificada na esquina.",
                EnderecoReferencia = "Moema",
                Status = "ABERTA",
                NumCiclo = 1,
                FlExcluida = "N",
                Latitude = -23.60m,
                Longitude = -46.66m,
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow
            };

            context.Ocorrencias.Add(ocorrencia);
            context.SaveChanges();

            var repository = new OcorrenciaRepository(context);

            // Act: exclusão lógica
            repository.Remover(ocorrencia);

            // Assert: no banco a ocorrência permanece com FL_EXCLUIDA = 'S', mas ObterPorId ignora
            var noBanco = context.Ocorrencias.First(o => o.IdOcorrencia == 1);
            Assert.Equal("S", noBanco.FlExcluida);
            Assert.Null(repository.ObterPorId(1));
        }
    }
}
