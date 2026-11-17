using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IRelatorioRepository
    {
        IEnumerable<RelatorioOcorrenciaEntity> ObterRelatorioOcorrencias(
            long? idMunicipio = null, long? idCategoria = null, string? status = null);
    }
}
