using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IMunicipioRepository
    {
        IEnumerable<MunicipioEntity> ObterTodos(string? nome = null, string? uf = null);
        MunicipioEntity? ObterPorId(long idMunicipio);
    }
}
