using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IAtualizacaoRepository
    {
        IEnumerable<AtualizacaoOcorrenciaEntity> ObterPorOcorrencia(decimal idOcorrencia);
        (IEnumerable<AtualizacaoOcorrenciaEntity> Itens, int Total) ObterPorOcorrenciaPaginado(
            decimal idOcorrencia, int deslocamento, int limite);
        AtualizacaoOcorrenciaEntity? ObterPorId(decimal idAtualizacao);
        bool JaVotou(decimal idOcorrencia, decimal idUsuario, string tipo);
        AtualizacaoOcorrenciaEntity Registrar(AtualizacaoOcorrenciaEntity atualizacao);
    }
}
