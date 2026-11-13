using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class AtualizacaoMapper
    {
        public static AtualizacaoOcorrenciaEntity ToEntity(
            this AtualizacaoRequestDto dto,
            decimal idOcorrencia,
            decimal idUsuario)
        {
            return new AtualizacaoOcorrenciaEntity
            {
                IdOcorrencia = idOcorrencia,
                IdUsuario = idUsuario,
                Tipo = dto.Tipo,
                Comentario = dto.Comentario?.Trim(),
                UrlFotoEvidencia = dto.UrlFotoEvidencia?.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CriadaEm = DateTime.Now
            };
        }

        public static AtualizacaoResponseDto ToResponseDto(this AtualizacaoOcorrenciaEntity entity)
        {
            return new AtualizacaoResponseDto
            {
                IdAtualizacao = entity.IdAtualizacao,
                IdOcorrencia = entity.IdOcorrencia,
                IdUsuario = entity.IdUsuario,
                NomeUsuario = entity.Usuario?.Nome ?? string.Empty,
                Tipo = entity.Tipo,
                Comentario = entity.Comentario,
                UrlFotoEvidencia = entity.UrlFotoEvidencia,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                CriadaEm = entity.CriadaEm
            };
        }
    }
}
