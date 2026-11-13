using BairroVivo.Application.Dtos.Ocorrencias;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class OcorrenciaMapper
    {
        public static OcorrenciaEntity ToEntity(
            this OcorrenciaRequestDto dto,
            decimal idUsuario,
            string protocolo,
            int slaDiasPadrao)
        {
            var agora = DateTime.Now;
            return new OcorrenciaEntity
            {
                Protocolo = protocolo,
                IdUsuario = idUsuario,
                IdCategoria = dto.IdCategoria,
                IdMunicipio = dto.IdMunicipio,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Status = "ABERTA",
                Urgencia = dto.Urgencia ?? "MEDIA",
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Logradouro = dto.Logradouro,
                Bairro = dto.Bairro,
                Cep = dto.Cep,
                Confirmacoes = 0,
                ValidadaComunidade = "N",
                CategoriaSugeridaIa = dto.CategoriaSugeridaIa,
                ConfiancaIa = dto.ConfiancaIa,
                CriadaEm = agora,
                SlaLimite = agora.AddDays(slaDiasPadrao > 0 ? slaDiasPadrao : 15)
            };
        }

        public static void UpdateEntity(this OcorrenciaRequestDto dto, OcorrenciaEntity entity)
        {
            entity.IdCategoria = dto.IdCategoria;
            entity.IdMunicipio = dto.IdMunicipio;
            entity.Titulo = dto.Titulo;
            entity.Descricao = dto.Descricao;
            entity.Urgencia = dto.Urgencia ?? entity.Urgencia;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.Logradouro = dto.Logradouro;
            entity.Bairro = dto.Bairro;
            entity.Cep = dto.Cep;
            entity.CategoriaSugeridaIa = dto.CategoriaSugeridaIa;
            entity.ConfiancaIa = dto.ConfiancaIa;
            entity.AtualizadaEm = DateTime.Now;
        }

        public static OcorrenciaResponseDto ToResponseDto(this OcorrenciaEntity entity)
        {
            return new OcorrenciaResponseDto
            {
                IdOcorrencia = entity.IdOcorrencia,
                Protocolo = entity.Protocolo,
                IdUsuario = entity.IdUsuario,
                NomeUsuario = entity.Usuario?.Nome ?? string.Empty,
                IdCategoria = entity.IdCategoria,
                Categoria = entity.Categoria?.Nome ?? string.Empty,
                IdMunicipio = entity.IdMunicipio,
                Municipio = entity.Municipio?.Nome ?? string.Empty,
                Uf = entity.Municipio?.Uf ?? string.Empty,
                Titulo = entity.Titulo,
                Descricao = entity.Descricao,
                Status = entity.Status,
                Urgencia = entity.Urgencia,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Logradouro = entity.Logradouro,
                Bairro = entity.Bairro,
                Cep = entity.Cep,
                Confirmacoes = entity.Confirmacoes,
                ValidadaComunidade = entity.ValidadaComunidade,
                CategoriaSugeridaIa = entity.CategoriaSugeridaIa,
                ConfiancaIa = entity.ConfiancaIa,
                CriadaEm = entity.CriadaEm,
                AtualizadaEm = entity.AtualizadaEm,
                ResolvidaEm = entity.ResolvidaEm,
                SlaLimite = entity.SlaLimite,
                Midias = entity.Midias?.Select(m => m.ToResponseDto()).ToList() ?? new List<MidiaOcorrenciaResponseDto>()
            };
        }

        public static MidiaOcorrenciaResponseDto ToResponseDto(this MidiaOcorrenciaEntity entity)
        {
            return new MidiaOcorrenciaResponseDto
            {
                IdMidia = entity.IdMidia,
                IdOcorrencia = entity.IdOcorrencia,
                UrlMidia = entity.UrlMidia,
                TipoMidia = entity.TipoMidia,
                Principal = entity.Principal,
                CriadaEm = entity.CriadaEm
            };
        }
    }
}
