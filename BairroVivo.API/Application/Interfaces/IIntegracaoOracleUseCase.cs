using BairroVivo.Application.Dtos.Atualizacoes;

namespace BairroVivo.API.Application.Interfaces;

public interface IIntegracaoOracleUseCase
{
    Task<long> ExecutarRegistrarAtualizacaoAsync(long idOcorrencia, long idUsuario, AtualizacaoRequestDto dto);
    Task<int> ContarVotosAtivosAsync(long idOcorrencia, string tipoVoto);
}
