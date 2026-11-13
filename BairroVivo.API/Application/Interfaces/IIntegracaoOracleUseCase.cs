using BairroVivo.Application.Dtos.Atualizacoes;

namespace BairroVivo.API.Application.Interfaces;

public interface IIntegracaoOracleUseCase
{
    Task ExecutarRegistrarAtualizacaoAsync(decimal idOcorrencia, decimal idUsuario, AtualizacaoRequestDto dto);
    Task<int> ContarVotosAtivosAsync(decimal idOcorrencia);
}
