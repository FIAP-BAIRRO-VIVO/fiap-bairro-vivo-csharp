using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.Infrastructure.Data.Repositories
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly ApplicationContext _context;

        public RelatorioRepository(ApplicationContext context)
        {
            _context = context;
        }

        public IEnumerable<RelatorioOcorrenciaEntity> ObterRelatorioOcorrencias(
            long? idMunicipio = null, long? idCategoria = null, string? status = null)
        {
            // Em provedor InMemory ou Relacional, podemos agregar diretamente de TB_OCORRENCIA com FL_EXCLUIDA = 'N'
            // que espelha exatamente a definição da view VW_RELATORIO_OCORRENCIAS:
            // SELECT ID_MUNICIPIO, ID_CATEGORIA, STATUS, COUNT(*) AS TOTAL_OCORRENCIAS
            // FROM TB_OCORRENCIA WHERE FL_EXCLUIDA = 'N' GROUP BY ID_MUNICIPIO, ID_CATEGORIA, STATUS;
            var consulta = _context.Ocorrencias
                .Include(o => o.Municipio)
                .Include(o => o.Categoria)
                .Where(o => o.FlExcluida == "N")
                .AsQueryable();

            if (idMunicipio.HasValue && idMunicipio.Value > 0)
                consulta = consulta.Where(o => o.IdMunicipio == idMunicipio.Value);

            if (idCategoria.HasValue && idCategoria.Value > 0)
                consulta = consulta.Where(o => o.IdCategoria == idCategoria.Value);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var filtroStatus = status.Trim().ToUpperInvariant();
                consulta = consulta.Where(o => o.Status.ToUpper() == filtroStatus);
            }

            return consulta
                .GroupBy(o => new { o.IdMunicipio, o.IdCategoria, o.Status })
                .Select(g => new RelatorioOcorrenciaEntity
                {
                    IdMunicipio = g.Key.IdMunicipio,
                    IdCategoria = g.Key.IdCategoria,
                    Status = g.Key.Status,
                    TotalOcorrencias = g.Count(),
                    Municipio = g.First().Municipio,
                    Categoria = g.First().Categoria
                })
                .ToList();
        }
    }
}
