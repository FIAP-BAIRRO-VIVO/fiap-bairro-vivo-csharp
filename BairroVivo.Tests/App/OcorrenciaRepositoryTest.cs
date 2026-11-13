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

            var municipio = new MunicipioEntity { IdMunicipio = 1, CodigoIbge = "3550308", Nome = "São Paulo", Uf = "SP", Ativo = "S" };
            var usuario = new UsuarioEntity { IdUsuario = 1, Nome = "Maria Silva", Email = "maria@bairrovivo.com", SenhaHash = "hash", Perfil = "CIDADAO", Ativo = "S", CriadoEm = DateTime.Now };
            var categoria = new CategoriaEntity { IdCategoria = 1, Nome = "Buraco na Via", SlaDiasPadrao = 10, Ativo = "S" };
            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                Protocolo = "BV-2026-000001",
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Buraco fundo na rua",
                Descricao = "Buraco grande causando risco aos motoristas.",
                Bairro = "Bela Vista",
                Status = "ABERTA",
                Urgencia = "ALTA",
                Latitude = -23.5614m,
                Longitude = -46.6558m,
                CriadaEm = DateTime.Now,
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
            Assert.Equal("BV-2026-000001", resultado.Protocolo);
            Assert.NotNull(resultado.Categoria);
            Assert.Equal("Buraco na Via", resultado.Categoria.Nome);
        }

        [Fact]
        [Trait("OcorrenciaRepository", "ObterPorId")]
        public void ObterPorId_OcorrenciaInexistente_RetornaNull()
        {
            // Arrange
            using var context = CriarContexto();
            var repository = new OcorrenciaRepository(context);

            // Act
            var resultado = repository.ObterPorId(999);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        [Trait("OcorrenciaRepository", "Cadastrar")]
        public void Cadastrar_OcorrenciaValida_SalvaOcorrencia()
        {
            // Arrange
            using var context = CriarContexto();
            var repository = new OcorrenciaRepository(context);

            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                Protocolo = "BV-2026-000001",
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Poste apagado",
                Descricao = "Poste sem iluminação há duas semanas.",
                Bairro = "Pinheiros",
                Status = "ABERTA",
                Urgencia = "MEDIA",
                Latitude = -23.56m,
                Longitude = -46.69m,
                CriadaEm = DateTime.Now
            };

            // Act
            var resultado = repository.Cadastrar(ocorrencia);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.IdOcorrencia);
            var salva = context.Ocorrencias.FirstOrDefault(o => o.IdOcorrencia == 1);
            Assert.NotNull(salva);
            Assert.Equal("Poste apagado", salva.Titulo);
            Assert.Equal("ABERTA", salva.Status);
        }

        [Fact]
        [Trait("OcorrenciaRepository", "Atualizar")]
        public void Atualizar_OcorrenciaExistente_SalvaAlteracoes()
        {
            // Arrange
            using var context = CriarContexto();
            var ocorrencia = new OcorrenciaEntity
            {
                IdOcorrencia = 1,
                Protocolo = "BV-2026-000001",
                IdUsuario = 1,
                IdCategoria = 1,
                IdMunicipio = 1,
                Titulo = "Calçada quebrada",
                Descricao = "Calçada danificada na esquina.",
                Bairro = "Moema",
                Status = "ABERTA",
                Urgencia = "MEDIA",
                CriadaEm = DateTime.Now
            };

            context.Ocorrencias.Add(ocorrencia);
            context.SaveChanges();

            var repository = new OcorrenciaRepository(context);
            ocorrencia.Status = "RESOLVIDA";

            // Act
            repository.Atualizar(ocorrencia);

            // Assert
            var atualizada = context.Ocorrencias.First(o => o.IdOcorrencia == 1);
            Assert.Equal("RESOLVIDA", atualizada.Status);
        }
    }
}
