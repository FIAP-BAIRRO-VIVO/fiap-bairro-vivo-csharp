using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Municipios;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;

namespace BairroVivo.Application.UseCases
{
    public class MunicipioUseCase : IMunicipioUseCase
    {
        private readonly IMunicipioRepository _municipioRepository;

        public MunicipioUseCase(IMunicipioRepository municipioRepository)
        {
            _municipioRepository = municipioRepository;
        }

        public IEnumerable<MunicipioResponseDto> ObterTodos() =>
            _municipioRepository.ObterTodos().Select(m => m.ToResponseDto());

        public MunicipioResponseDto? ObterPorId(decimal idMunicipio) =>
            _municipioRepository.ObterPorId(idMunicipio)?.ToResponseDto();
    }
}
