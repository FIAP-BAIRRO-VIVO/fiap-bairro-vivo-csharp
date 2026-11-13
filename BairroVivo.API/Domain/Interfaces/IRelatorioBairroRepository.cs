using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IRelatorioBairroRepository
    {
        (IEnumerable<RelatorioBairroEntity> Itens, int Total) ObterPaginado(
            int deslocamento, int limite, string? bairro, string? periodoRef);
        RelatorioBairroEntity? ObterPorId(decimal idRelatorio);
    }
}
