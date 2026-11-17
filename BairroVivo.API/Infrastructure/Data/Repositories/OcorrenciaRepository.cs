using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.Infrastructure.Data.Repositories
{
    public class OcorrenciaRepository : IOcorrenciaRepository
    {
        private readonly ApplicationContext _context;

        public OcorrenciaRepository(ApplicationContext context)
        {
            _context = context;
        }

        public IEnumerable<OcorrenciaEntity> ObterTodas() =>
            _context.Ocorrencias
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Municipio)
                .Include(o => o.Midias)
                .Include(o => o.Votos)
                .Where(o => o.FlExcluida == "N")
                .OrderByDescending(o => o.DtCriacao)
                .ToList();

        public (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPaginado(
            int deslocamento, int limite,
            decimal? minLat, decimal? maxLat, decimal? minLon, decimal? maxLon,
            long? idCategoria, long? idMunicipio, string? status, string? ordem)
        {
            var consulta = _context.Ocorrencias
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Municipio)
                .Include(o => o.Midias)
                .Include(o => o.Votos)
                .Where(o => o.FlExcluida == "N")
                .AsQueryable();

            if (minLat.HasValue)
                consulta = consulta.Where(o => o.Latitude >= minLat.Value);

            if (maxLat.HasValue)
                consulta = consulta.Where(o => o.Latitude <= maxLat.Value);

            if (minLon.HasValue)
                consulta = consulta.Where(o => o.Longitude >= minLon.Value);

            if (maxLon.HasValue)
                consulta = consulta.Where(o => o.Longitude <= maxLon.Value);

            if (idCategoria.HasValue && idCategoria.Value > 0)
                consulta = consulta.Where(o => o.IdCategoria == idCategoria.Value);

            if (idMunicipio.HasValue && idMunicipio.Value > 0)
                consulta = consulta.Where(o => o.IdMunicipio == idMunicipio.Value);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var filtroStatus = status.Trim().ToUpperInvariant();
                consulta = consulta.Where(o => o.Status.ToUpper() == filtroStatus);
            }

            consulta = (ordem ?? "recentes").ToLowerInvariant() switch
            {
                "titulo" => consulta.OrderBy(o => o.Titulo).ThenBy(o => o.IdOcorrencia),
                "antigas" => consulta.OrderBy(o => o.DtCriacao).ThenBy(o => o.IdOcorrencia),
                _ => consulta.OrderByDescending(o => o.DtCriacao).ThenByDescending(o => o.IdOcorrencia)
            };

            var total = consulta.Count();
            var itens = consulta.Skip(deslocamento).Take(limite).ToList();
            return (itens, total);
        }

        public (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPorUsuarioPaginado(
            long idUsuario, int deslocamento, int limite, string? status, string? ordem)
        {
            var consulta = _context.Ocorrencias
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Municipio)
                .Include(o => o.Midias)
                .Include(o => o.Votos)
                .Where(o => o.IdUsuario == idUsuario && o.FlExcluida == "N");

            if (!string.IsNullOrWhiteSpace(status))
            {
                var filtroStatus = status.Trim().ToUpperInvariant();
                consulta = consulta.Where(o => o.Status.ToUpper() == filtroStatus);
            }

            consulta = (ordem ?? "recentes").ToLowerInvariant() switch
            {
                "titulo" => consulta.OrderBy(o => o.Titulo).ThenBy(o => o.IdOcorrencia),
                "antigas" => consulta.OrderBy(o => o.DtCriacao).ThenBy(o => o.IdOcorrencia),
                _ => consulta.OrderByDescending(o => o.DtCriacao).ThenByDescending(o => o.IdOcorrencia)
            };

            var total = consulta.Count();
            var itens = consulta.Skip(deslocamento).Take(limite).ToList();
            return (itens, total);
        }

        public OcorrenciaEntity? ObterPorId(long idOcorrencia)
        {
            return _context.Ocorrencias
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Municipio)
                .Include(o => o.Midias)
                .Include(o => o.Votos)
                .FirstOrDefault(o => o.IdOcorrencia == idOcorrencia && o.FlExcluida == "N");
        }

        public OcorrenciaEntity Cadastrar(OcorrenciaEntity ocorrencia)
        {
            _context.Ocorrencias.Add(ocorrencia);
            _context.SaveChanges();
            return ocorrencia;
        }

        public void Atualizar(OcorrenciaEntity ocorrencia)
        {
            _context.Ocorrencias.Update(ocorrencia);
            _context.SaveChanges();
        }

        public void Remover(OcorrenciaEntity ocorrencia)
        {
            // Exclusão lógica obrigatória conforme Oracle Schema v1.2
            ocorrencia.FlExcluida = "S";
            ocorrencia.DtAtualizacao = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void RegistrarHistoricoStatus(HistoricoStatusOcorrenciaEntity historico)
        {
            _context.HistoricoStatus.Add(historico);
            _context.SaveChanges();
        }

        public IEnumerable<HistoricoStatusOcorrenciaEntity> ObterHistoricoStatus(long idOcorrencia)
        {
            return _context.HistoricoStatus
                .Where(h => h.IdOcorrencia == idOcorrencia)
                .OrderBy(h => h.DtEvento)
                .ToList();
        }

        public MidiaOcorrenciaEntity AdicionarMidia(MidiaOcorrenciaEntity midia)
        {
            _context.Midias.Add(midia);
            _context.SaveChanges();
            return midia;
        }

        public bool ExisteUsuario(long idUsuario)
        {
            return _context.Usuarios.Any(u => u.IdUsuario == idUsuario && u.FlAtivo == "S");
        }

        public bool ExisteCategoria(long idCategoria)
        {
            return _context.Categorias.Any(c => c.IdCategoria == idCategoria && c.FlAtiva == "S");
        }

        public CategoriaEntity? ObterCategoria(long idCategoria)
        {
            return _context.Categorias.FirstOrDefault(c => c.IdCategoria == idCategoria && c.FlAtiva == "S");
        }

        public bool ExisteMunicipio(long idMunicipio)
        {
            return _context.Municipios.Any(m => m.IdMunicipio == idMunicipio);
        }
    }
}
