using BairroVivo.Application.Dtos.Relatorios;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class RelatorioBairroMapper
    {
        public static RelatorioBairroResponseDto ToResponseDto(this RelatorioBairroEntity entity)
        {
            return new RelatorioBairroResponseDto
            {
                IdRelatorio = entity.IdRelatorio,
                IdMunicipio = entity.IdMunicipio,
                Municipio = entity.Municipio?.Nome ?? string.Empty,
                Bairro = entity.Bairro,
                PeriodoRef = entity.PeriodoRef,
                TotalOcorrencias = entity.TotalOcorrencias,
                TotalResolvidas = entity.TotalResolvidas,
                ResumoIa = entity.ResumoIa,
                ModeloIa = entity.ModeloIa,
                GeradoEm = entity.GeradoEm
            };
        }
    }
}
