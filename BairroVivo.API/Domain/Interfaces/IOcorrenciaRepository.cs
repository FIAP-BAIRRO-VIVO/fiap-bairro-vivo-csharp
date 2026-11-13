using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IOcorrenciaRepository
    {
        IEnumerable<OcorrenciaEntity> ObterTodas();
        (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPaginado(
            int deslocamento, int limite, string? status, string? urgencia,
            string? bairro, decimal? idCategoria, decimal? idMunicipio, string? ordem);
        (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPorUsuarioPaginado(
            decimal idUsuario, int deslocamento, int limite, string? status, string? ordem);
        OcorrenciaEntity? ObterPorId(decimal idOcorrencia);
        OcorrenciaEntity Cadastrar(OcorrenciaEntity ocorrencia);
        void Atualizar(OcorrenciaEntity ocorrencia);
        void Remover(OcorrenciaEntity ocorrencia);
        void RegistrarHistoricoStatus(HistoricoStatusOcorrenciaEntity historico);
        MidiaOcorrenciaEntity AdicionarMidia(MidiaOcorrenciaEntity midia);
        bool ExisteUsuario(decimal idUsuario);
        bool ExisteCategoria(decimal idCategoria);
        CategoriaEntity? ObterCategoria(decimal idCategoria);
        bool ExisteMunicipio(decimal idMunicipio);
        decimal ObterProximoSequencialProtocolo();
    }
}
