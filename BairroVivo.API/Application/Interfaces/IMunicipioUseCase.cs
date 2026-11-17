using BairroVivo.Application.Dtos.Municipios;

namespace BairroVivo.Application.Interfaces
{
    public interface IMunicipioUseCase
    {
        IEnumerable<MunicipioResponseDto> ObterTodos(string? nome = null, string? uf = null);
        MunicipioResponseDto? ObterPorId(long idMunicipio);
    }
}
