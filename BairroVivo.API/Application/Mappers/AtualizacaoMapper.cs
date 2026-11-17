using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Application.Mappers;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class AtualizacaoMapper
    {
        public static AtualizacaoOcorrenciaEntity ToEntity(
            this AtualizacaoRequestDto dto,
            long idOcorrencia,
            long idUsuario)
        {
            return new AtualizacaoOcorrenciaEntity
            {
                IdOcorrencia = idOcorrencia,
                IdUsuario = idUsuario,
                Tipo = dto.Tipo,
                Texto = dto.Texto ?? dto.Comentario,
                FlAnonimo = dto.AnonimoPublico ? "S" : "N",
                DtCriacao = DateTime.UtcNow
            };
        }

        public static AtualizacaoResponseDto ToResponseDto(this AtualizacaoOcorrenciaEntity entity)
        {
            var autorPublico = entity.FlAnonimo == "S"
                ? "Cidadao anonimo"
                : (!string.IsNullOrWhiteSpace(entity.Usuario?.Nome) ? entity.Usuario.Nome : "Cidadao");

            return new AtualizacaoResponseDto
            {
                Id = entity.IdAtualizacao.ToString(),
                IdOcorrencia = entity.IdOcorrencia.ToString(),
                Tipo = entity.Tipo,
                Texto = entity.Texto,
                AutorExibicao = autorPublico,
                AnonimoPublico = entity.FlAnonimo == "S",
                ContabilizadaComoVoto = entity.Voto != null,
                StatusAtual = entity.Ocorrencia?.Status ?? string.Empty,
                NumeroCiclo = entity.Ocorrencia?.NumCiclo ?? 1,
                DtCriacao = entity.DtCriacao,
                Midias = entity.Midias?.Select(m => m.ToResponseDto()).ToList() ?? new()
            };
        }
    }
}
