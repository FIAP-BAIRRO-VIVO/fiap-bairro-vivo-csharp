using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.Infrastructure.Data.Repositories
{
    public class RelatorioBairroRepository : IRelatorioBairroRepository
    {
        private readonly ApplicationContext _context;

        public RelatorioBairroRepository(ApplicationContext context)
        {
            _context = context;
        }

        public (IEnumerable<RelatorioBairroEntity> Itens, int Total) ObterPaginado(
            int deslocamento, int limite, string? bairro, string? periodoRef)
        {
            var consulta = _context.RelatoriosBairro
                .Include(r => r.Municipio)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(bairro))
            {
                var filtroBairro = bairro.Trim().ToUpperInvariant();
                consulta = consulta.Where(r => r.Bairro.ToUpper().Contains(filtroBairro));
            }

            if (!string.IsNullOrWhiteSpace(periodoRef))
            {
                var filtroPeriodo = periodoRef.Trim().ToUpperInvariant();
                consulta = consulta.Where(r => r.PeriodoRef.ToUpper() == filtroPeriodo);
            }

            consulta = consulta.OrderByDescending(r => r.GeradoEm).ThenByDescending(r => r.IdRelatorio);

            var total = consulta.Count();
            var itens = consulta.Skip(deslocamento).Take(limite).ToList();
            return (itens, total);
        }

        public RelatorioBairroEntity? ObterPorId(decimal idRelatorio) =>
            _context.RelatoriosBairro
                .Include(r => r.Municipio)
                .FirstOrDefault(r => r.IdRelatorio == idRelatorio);
    }
}
