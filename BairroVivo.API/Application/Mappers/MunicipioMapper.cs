using BairroVivo.Application.Dtos.Municipios;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class MunicipioMapper
    {
        public static MunicipioResponseDto ToResponseDto(this MunicipioEntity entity)
        {
            return new MunicipioResponseDto
            {
                Id = entity.IdMunicipio.ToString(),
                CodigoIbge = entity.CodigoIbge,
                Nome = entity.Nome,
                Uf = entity.Uf
            };
        }
    }
}
