using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Relatorios;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;

namespace BairroVivo.Application.UseCases
{
    public class RelatorioUseCase : IRelatorioUseCase
    {
        private readonly IRelatorioRepository _relatorioRepository;

        public RelatorioUseCase(IRelatorioRepository relatorioRepository)
        {
            _relatorioRepository = relatorioRepository;
        }

        public IEnumerable<RelatorioOcorrenciaResponseDto> ObterRelatorioOcorrencias(
            long? idMunicipio = null, long? idCategoria = null, string? status = null)
        {
            var relatorios = _relatorioRepository.ObterRelatorioOcorrencias(idMunicipio, idCategoria, status);
            return relatorios.Select(r => r.ToResponseDto());
        }
    }
}
