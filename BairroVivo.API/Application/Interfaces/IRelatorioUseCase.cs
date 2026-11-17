using BairroVivo.Application.Dtos.Relatorios;

namespace BairroVivo.Application.Interfaces
{
    public interface IRelatorioUseCase
    {
        IEnumerable<RelatorioOcorrenciaResponseDto> ObterRelatorioOcorrencias(
            long? idMunicipio = null, long? idCategoria = null, string? status = null);
    }
}
