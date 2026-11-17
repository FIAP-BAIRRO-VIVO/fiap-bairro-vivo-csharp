using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IOcorrenciaRepository
    {
        IEnumerable<OcorrenciaEntity> ObterTodas();

        (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPaginado(
            int deslocamento, int limite,
            decimal? minLat, decimal? maxLat, decimal? minLon, decimal? maxLon,
            long? idCategoria, long? idMunicipio, string? status, string? ordem);

        (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPorUsuarioPaginado(
            long idUsuario, int deslocamento, int limite, string? status, string? ordem);

        OcorrenciaEntity? ObterPorId(long idOcorrencia);

        OcorrenciaEntity Cadastrar(OcorrenciaEntity ocorrencia);

        void Atualizar(OcorrenciaEntity ocorrencia);

        void Remover(OcorrenciaEntity ocorrencia);

        void RegistrarHistoricoStatus(HistoricoStatusOcorrenciaEntity historico);

        IEnumerable<HistoricoStatusOcorrenciaEntity> ObterHistoricoStatus(long idOcorrencia);

        MidiaOcorrenciaEntity AdicionarMidia(MidiaOcorrenciaEntity midia);

        bool ExisteUsuario(long idUsuario);

        bool ExisteCategoria(long idCategoria);

        CategoriaEntity? ObterCategoria(long idCategoria);

        bool ExisteMunicipio(long idMunicipio);
    }
}
