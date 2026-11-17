using BairroVivo.Application.Dtos.Relatorios;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class RelatorioOcorrenciaMapper
    {
        public static RelatorioOcorrenciaResponseDto ToResponseDto(this RelatorioOcorrenciaEntity entity)
        {
            return new RelatorioOcorrenciaResponseDto
            {
                IdMunicipio = entity.IdMunicipio.ToString(),
                NomeMunicipio = entity.Municipio?.Nome ?? string.Empty,
                Uf = entity.Municipio?.Uf ?? string.Empty,
                IdCategoria = entity.IdCategoria.ToString(),
                NomeCategoria = entity.Categoria?.Nome ?? string.Empty,
                Status = entity.Status,
                TotalOcorrencias = entity.TotalOcorrencias
            };
        }
    }
}
