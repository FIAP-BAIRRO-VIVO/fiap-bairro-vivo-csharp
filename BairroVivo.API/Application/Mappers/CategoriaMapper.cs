using BairroVivo.Application.Dtos.Categorias;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Application.Mappers
{
    public static class CategoriaMapper
    {
        public static CategoriaEntity ToEntity(this CategoriaRequestDto dto)
        {
            return new CategoriaEntity
            {
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao?.Trim(),
                Icone = dto.Icone?.Trim(),
                FlAtiva = "S"
            };
        }

        public static void UpdateEntity(this CategoriaRequestDto dto, CategoriaEntity entity)
        {
            entity.Nome = dto.Nome.Trim();
            entity.Descricao = dto.Descricao?.Trim();
            entity.Icone = dto.Icone?.Trim();
        }

        public static CategoriaResponseDto ToResponseDto(this CategoriaEntity entity)
        {
            return new CategoriaResponseDto
            {
                Id = entity.IdCategoria.ToString(),
                Nome = entity.Nome,
                Descricao = entity.Descricao,
                Icone = entity.Icone,
                Ativa = entity.FlAtiva == "S"
            };
        }
    }
}
