using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IAtualizacaoRepository
    {
        IEnumerable<AtualizacaoOcorrenciaEntity> ObterPorOcorrencia(long idOcorrencia);

        (IEnumerable<AtualizacaoOcorrenciaEntity> Itens, int Total) ObterPorOcorrenciaPaginado(
            long idOcorrencia, int deslocamento, int limite);

        AtualizacaoOcorrenciaEntity? ObterPorId(long idAtualizacao);

        bool JaVotouNoCiclo(long idOcorrencia, long idUsuario, int numCiclo);

        AtualizacaoOcorrenciaEntity Registrar(AtualizacaoOcorrenciaEntity atualizacao);

        void RegistrarVoto(VotoSituacaoEntity voto);

        int ContarVotosCiclo(long idOcorrencia, int numCiclo, string tipoVoto);

        MidiaOcorrenciaEntity AdicionarMidia(MidiaOcorrenciaEntity midia);
    }
}
