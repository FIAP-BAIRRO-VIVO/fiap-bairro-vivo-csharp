using BairroVivo.Application.Dtos.Municipios;

namespace BairroVivo.Application.Interfaces
{
    public interface IMunicipioUseCase
    {
        IEnumerable<MunicipioResponseDto> ObterTodos();
        MunicipioResponseDto? ObterPorId(decimal idMunicipio);
    }
}
