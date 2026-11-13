using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Infrastructure.Data.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationContext _context;

        public CategoriaRepository(ApplicationContext context)
        {
            _context = context;
        }

        public IEnumerable<CategoriaEntity> ObterTodas() =>
            _context.Categorias
                .Where(c => c.Ativo == "S")
                .OrderBy(c => c.Nome)
                .ToList();

        public CategoriaEntity? ObterPorId(decimal idCategoria) =>
            _context.Categorias.FirstOrDefault(c => c.IdCategoria == idCategoria);

        public CategoriaEntity Cadastrar(CategoriaEntity categoria)
        {
            _context.Categorias.Add(categoria);
            _context.SaveChanges();
            return categoria;
        }

        public void Atualizar(CategoriaEntity categoria)
        {
            _context.Categorias.Update(categoria);
            _context.SaveChanges();
        }

        public void Remover(CategoriaEntity categoria)
        {
            categoria.Ativo = "N";
            _context.Categorias.Update(categoria);
            _context.SaveChanges();
        }
    }
}
