using BairroVivo.API.Application.Dtos;
using BairroVivo.Application.Dtos.Atualizacoes;

namespace BairroVivo.Application.Interfaces
{
    public interface IAtualizacaoUseCase
    {
        IEnumerable<AtualizacaoResponseDto> ObterPorOcorrencia(decimal idOcorrencia);
        PageResponseDto<AtualizacaoResponseDto> ObterPorOcorrenciaPaginado(
            decimal idOcorrencia, int pagina, int tamanho);
        AtualizacaoResponseDto? ObterPorId(decimal idAtualizacao);
        AtualizacaoResponseDto Registrar(decimal idOcorrencia, decimal idUsuario, AtualizacaoRequestDto dto);
    }
}
