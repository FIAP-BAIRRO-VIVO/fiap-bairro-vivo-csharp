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
                IdMunicipio = entity.IdMunicipio,
                CodigoIbge = entity.CodigoIbge,
                Nome = entity.Nome,
                Uf = entity.Uf,
                Ativo = entity.Ativo
            };
        }
    }
}
