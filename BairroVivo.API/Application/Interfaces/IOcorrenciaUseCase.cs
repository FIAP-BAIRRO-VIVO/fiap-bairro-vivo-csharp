using BairroVivo.API.Application.Dtos;
using BairroVivo.Application.Dtos.Ocorrencias;

namespace BairroVivo.Application.Interfaces
{
    public interface IOcorrenciaUseCase
    {
        IEnumerable<OcorrenciaResponseDto> ObterTodas();

        PageResponseDto<OcorrenciaResponseDto> ObterPaginado(
            int pagina, int tamanho,
            decimal? minLat, decimal? maxLat, decimal? minLon, decimal? maxLon,
            long? idCategoria, long? idMunicipio, string? status, string? ordem);

        PageResponseDto<OcorrenciaResponseDto> ObterPorUsuarioPaginado(
            long idUsuario, int pagina, int tamanho, string? status, string? ordem);

        OcorrenciaResponseDto? ObterPorId(long idOcorrencia);

        OcorrenciaResponseDto Cadastrar(long idUsuario, OcorrenciaRequestDto dto);

        bool Atualizar(long idOcorrencia, long idUsuario, OcorrenciaRequestDto dto);

        bool Remover(long idOcorrencia, long idUsuario);

        IEnumerable<HistoricoStatusResponseDto> ObterHistorico(long idOcorrencia);

        MidiaOcorrenciaResponseDto AdicionarMidia(long idOcorrencia, long idUsuario, MidiaOcorrenciaRequestDto dto);
    }
}
