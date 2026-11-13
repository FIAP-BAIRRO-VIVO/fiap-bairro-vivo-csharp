using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Relatorios;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;

namespace BairroVivo.Application.UseCases
{
    public class RelatorioBairroUseCase : IRelatorioBairroUseCase
    {
        private readonly IRelatorioBairroRepository _relatorioRepository;

        public RelatorioBairroUseCase(IRelatorioBairroRepository relatorioRepository)
        {
            _relatorioRepository = relatorioRepository;
        }

        public PageResponseDto<RelatorioBairroResponseDto> ObterPaginado(
            int pagina, int tamanho, string? bairro, string? periodoRef)
        {
            pagina = Math.Max(1, pagina);
            tamanho = Math.Clamp(tamanho, 1, 50);
            var deslocamento = (pagina - 1) * tamanho;

            var (itens, total) = _relatorioRepository
                .ObterPaginado(deslocamento, tamanho, bairro, periodoRef);

            return new PageResponseDto<RelatorioBairroResponseDto>
            {
                Deslocamento = deslocamento,
                RegistroRetornado = tamanho,
                TotalRegistros = total,
                Items = itens.Select(r => r.ToResponseDto()).ToList()
            };
        }

        public RelatorioBairroResponseDto? ObterPorId(decimal idRelatorio) =>
            _relatorioRepository.ObterPorId(idRelatorio)?.ToResponseDto();
    }
}
