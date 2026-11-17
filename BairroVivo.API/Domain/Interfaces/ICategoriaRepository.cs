using BairroVivo.Domain.Entities;

namespace BairroVivo.API.Domain.Interfaces
{
    public interface ICategoriaRepository
    {
        IEnumerable<CategoriaEntity> ObterTodas();
        CategoriaEntity? ObterPorId(long idCategoria);
        CategoriaEntity Cadastrar(CategoriaEntity categoria);
        void Atualizar(CategoriaEntity categoria);
        void Remover(CategoriaEntity categoria);
    }
}
