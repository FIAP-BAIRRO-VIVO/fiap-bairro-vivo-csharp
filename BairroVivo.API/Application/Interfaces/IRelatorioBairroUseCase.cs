using BairroVivo.API.Application.Dtos;
using BairroVivo.Application.Dtos.Relatorios;

namespace BairroVivo.Application.Interfaces
{
    public interface IRelatorioBairroUseCase
    {
        PageResponseDto<RelatorioBairroResponseDto> ObterPaginado(
            int pagina, int tamanho, string? bairro, string? periodoRef);
        RelatorioBairroResponseDto? ObterPorId(decimal idRelatorio);
    }
}
