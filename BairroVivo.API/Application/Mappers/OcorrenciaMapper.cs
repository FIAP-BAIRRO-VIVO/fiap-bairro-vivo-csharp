using BairroVivo.API.Presentation;
using BairroVivo.Application.Dtos.Categorias;
using BairroVivo.Application.Dtos.Municipios;
using BairroVivo.Application.Dtos.Ocorrencias;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class OcorrenciaMapper
    {
        public static OcorrenciaEntity ToEntity(
            this OcorrenciaRequestDto dto,
            long idUsuario)
        {
            var agora = DateTime.UtcNow;
            return new OcorrenciaEntity
            {
                IdUsuario = idUsuario,
                IdCategoria = dto.CategoriaId,
                IdMunicipio = dto.MunicipioId,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                EnderecoReferencia = dto.EnderecoReferencia,
                FlAnonimo = dto.AnonimoPublico ? "S" : "N",
                Status = "ABERTA",
                NumCiclo = 1,
                FlExcluida = "N",
                DtCriacao = agora,
                DtAtualizacao = agora
            };
        }

        public static void UpdateEntity(this OcorrenciaRequestDto dto, OcorrenciaEntity entity)
        {
            entity.IdCategoria = dto.CategoriaId;
            entity.IdMunicipio = dto.MunicipioId;
            entity.Titulo = dto.Titulo;
            entity.Descricao = dto.Descricao;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.EnderecoReferencia = dto.EnderecoReferencia;
            entity.FlAnonimo = dto.AnonimoPublico ? "S" : "N";
            entity.DtAtualizacao = DateTime.UtcNow;
        }

        public static OcorrenciaResponseDto ToResponseDto(this OcorrenciaEntity entity)
        {
            var autorPublico = entity.FlAnonimo == "S"
                ? "Cidadao anonimo"
                : (!string.IsNullOrWhiteSpace(entity.Usuario?.Nome) ? entity.Usuario.Nome : "Cidadao");

            var totalVotosCiclo = entity.Votos != null
                ? entity.Votos.Count(v => v.NumCiclo == entity.NumCiclo)
                : 0;

            return new OcorrenciaResponseDto
            {
                Id = entity.IdOcorrencia.ToString(),
                Titulo = entity.Titulo,
                Descricao = entity.Descricao,
                Status = entity.Status,
                NumeroCiclo = entity.NumCiclo,
                Categoria = entity.Categoria != null
                    ? entity.Categoria.ToResponseDto()
                    : new CategoriaResponseDto { Id = entity.IdCategoria.ToString() },
                Municipio = entity.Municipio != null
                    ? entity.Municipio.ToResponseDto()
                    : new MunicipioResponseDto { Id = entity.IdMunicipio.ToString() },
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                EnderecoReferencia = entity.EnderecoReferencia,
                AutorExibicao = autorPublico,
                AnonimoPublico = entity.FlAnonimo == "S",
                DtCriacao = entity.DtCriacao,
                DtAtualizacao = entity.DtAtualizacao,
                TotalVotosCiclo = totalVotosCiclo,
                Midias = entity.Midias?.Select(m => m.ToResponseDto()).ToList() ?? new List<MidiaOcorrenciaResponseDto>()
            };
        }

        public static MidiaOcorrenciaResponseDto ToResponseDto(this MidiaOcorrenciaEntity entity)
        {
            return new MidiaOcorrenciaResponseDto
            {
                Id = entity.IdMidia.ToString(),
                IdOcorrencia = entity.IdOcorrencia.ToString(),
                IdAtualizacao = entity.IdAtualizacao?.ToString(),
                Provedor = entity.Provedor,
                ChaveStorage = entity.ChaveStorage,
                UrlExibicao = entity.UrlExibicao,
                MimeType = entity.MimeType,
                TamanhoBytes = entity.TamanhoBytes,
                DtCriacao = entity.DtCriacao
            };
        }

        public static object ParaRespostaDetalhada(this OcorrenciaResponseDto o)
        {
            return new
            {
                o.Id,
                o.IdOcorrencia,
                o.Protocolo,
                o.Titulo,
                o.Descricao,
                o.Status,
                o.NumeroCiclo,
                o.Categoria,
                o.IdCategoria,
                o.Municipio,
                o.IdMunicipio,
                o.Uf,
                o.Latitude,
                o.Longitude,
                o.EnderecoReferencia,
                o.Bairro,
                o.AutorExibicao,
                o.NomeUsuario,
                o.AnonimoPublico,
                o.DtCriacao,
                o.CriadaEm,
                o.DtAtualizacao,
                o.AtualizadaEm,
                o.TotalVotosCiclo,
                o.Confirmacoes,
                o.ValidadaComunidade,
                o.Midias,
                Links = HateoasLinks.ParaOcorrencia(o.IdOcorrencia)
            };
        }
    }
}
