using BairroVivo.API.Application.Dtos;
using BairroVivo.Application.Dtos.Ocorrencias;

namespace BairroVivo.Application.Interfaces
{
    public interface IOcorrenciaUseCase
    {
        IEnumerable<OcorrenciaResponseDto> ObterTodas();
        PageResponseDto<OcorrenciaResponseDto> ObterPaginado(
            int pagina, int tamanho, string? status, string? urgencia,
            string? bairro, decimal? idCategoria, decimal? idMunicipio, string? ordem);
        PageResponseDto<OcorrenciaResponseDto> ObterPorUsuarioPaginado(
            decimal idUsuario, int pagina, int tamanho, string? status, string? ordem);
        OcorrenciaResponseDto? ObterPorId(decimal idOcorrencia);
        OcorrenciaResponseDto Cadastrar(decimal idUsuario, OcorrenciaRequestDto dto);
        bool Atualizar(decimal idOcorrencia, OcorrenciaRequestDto dto);
        bool AtualizarStatus(decimal idOcorrencia, decimal? idUsuarioResponsavel, OcorrenciaStatusRequestDto dto);
        bool Remover(decimal idOcorrencia);
        MidiaOcorrenciaResponseDto AdicionarMidia(decimal idOcorrencia, MidiaOcorrenciaRequestDto dto);
    }
}
