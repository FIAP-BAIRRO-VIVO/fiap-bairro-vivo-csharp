using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface IMunicipioRepository
    {
        IEnumerable<MunicipioEntity> ObterTodos();
        MunicipioEntity? ObterPorId(decimal idMunicipio);
    }
}
