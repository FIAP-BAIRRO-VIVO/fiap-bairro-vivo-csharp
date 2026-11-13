namespace BairroVivo.API.Domain.Interfaces;

public interface IIntegracaoOracleRepository
{
    Task ExecutarRegistrarAtualizacaoAsync(
        decimal idOcorrencia, decimal idUsuario, string tipo, string? comentario, string? urlFoto);
    Task<int> ContarVotosAtivosAsync(decimal idOcorrencia);
}
