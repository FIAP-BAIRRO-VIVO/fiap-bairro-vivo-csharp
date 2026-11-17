namespace BairroVivo.API.Domain.Interfaces;

public interface IIntegracaoOracleRepository
{
    Task<long> ExecutarRegistrarAtualizacaoAsync(
        long idOcorrencia, long idUsuario, string tipo, string? texto, string flAnonimo);

    Task<int> ContarVotosAtivosAsync(long idOcorrencia, string tipoVoto);
}
