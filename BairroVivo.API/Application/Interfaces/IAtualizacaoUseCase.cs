using BairroVivo.API.Application.Dtos;
using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Application.Dtos.Ocorrencias;

namespace BairroVivo.Application.Interfaces
{
    public interface IAtualizacaoUseCase
    {
        IEnumerable<AtualizacaoResponseDto> ObterPorOcorrencia(long idOcorrencia);

        PageResponseDto<AtualizacaoResponseDto> ObterPorOcorrenciaPaginado(
            long idOcorrencia, int pagina, int tamanho);

        AtualizacaoResponseDto? ObterPorId(long idAtualizacao);

        Task<AtualizacaoResponseDto> RegistrarAsync(long idOcorrencia, long idUsuario, AtualizacaoRequestDto dto);

        MidiaOcorrenciaResponseDto AdicionarMidia(long idOcorrencia, long idAtualizacao, long idUsuario, MidiaOcorrenciaRequestDto dto);
    }
}
