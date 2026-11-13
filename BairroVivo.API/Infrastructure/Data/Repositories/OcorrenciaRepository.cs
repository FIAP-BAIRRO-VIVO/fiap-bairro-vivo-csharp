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
                .OrderByDescending(o => o.CriadaEm)
                .ToList();

        // Paginacao com Offset/Limit conforme Aula 07 e padrao Amandaba.
        public (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPaginado(
            int deslocamento, int limite, string? status, string? urgencia,
            string? bairro, decimal? idCategoria, decimal? idMunicipio, string? ordem)
        {
            var consulta = _context.Ocorrencias
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Municipio)
                .Include(o => o.Midias)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var filtroStatus = status.Trim().ToUpperInvariant();
                consulta = consulta.Where(o => o.Status.ToUpper() == filtroStatus);
            }

            if (!string.IsNullOrWhiteSpace(urgencia))
            {
                var filtroUrgencia = urgencia.Trim().ToUpperInvariant();
                consulta = consulta.Where(o => o.Urgencia.ToUpper() == filtroUrgencia);
            }

            if (!string.IsNullOrWhiteSpace(bairro))
            {
                var filtroBairro = bairro.Trim().ToUpperInvariant();
                consulta = consulta.Where(o => o.Bairro.ToUpper().Contains(filtroBairro));
            }

            if (idCategoria.HasValue && idCategoria.Value > 0)
            {
                consulta = consulta.Where(o => o.IdCategoria == idCategoria.Value);
            }

            if (idMunicipio.HasValue && idMunicipio.Value > 0)
            {
                consulta = consulta.Where(o => o.IdMunicipio == idMunicipio.Value);
            }

            consulta = (ordem ?? "recentes").ToLowerInvariant() switch
            {
                "confirmacoes" => consulta.OrderByDescending(o => o.Confirmacoes).ThenByDescending(o => o.IdOcorrencia),
                "titulo" => consulta.OrderBy(o => o.Titulo).ThenBy(o => o.IdOcorrencia),
                "antigas" => consulta.OrderBy(o => o.CriadaEm).ThenBy(o => o.IdOcorrencia),
                _ => consulta.OrderByDescending(o => o.CriadaEm).ThenByDescending(o => o.IdOcorrencia)
            };

            var total = consulta.Count();
            var itens = consulta.Skip(deslocamento).Take(limite).ToList();
            return (itens, total);
        }

        public (IEnumerable<OcorrenciaEntity> Itens, int Total) ObterPorUsuarioPaginado(
            decimal idUsuario, int deslocamento, int limite, string? status, string? ordem)
        {
            var consulta = _context.Ocorrencias
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Municipio)
                .Include(o => o.Midias)
                .Where(o => o.IdUsuario == idUsuario);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var filtroStatus = status.Trim().ToUpperInvariant();
                consulta = consulta.Where(o => o.Status.ToUpper() == filtroStatus);
            }

            consulta = (ordem ?? "recentes").ToLowerInvariant() switch
            {
                "confirmacoes" => consulta.OrderByDescending(o => o.Confirmacoes).ThenByDescending(o => o.IdOcorrencia),
                "titulo" => consulta.OrderBy(o => o.Titulo).ThenBy(o => o.IdOcorrencia),
                _ => consulta.OrderByDescending(o => o.CriadaEm).ThenByDescending(o => o.IdOcorrencia)
            };

            var total = consulta.Count();
            var itens = consulta.Skip(deslocamento).Take(limite).ToList();
            return (itens, total);
        }

        public OcorrenciaEntity? ObterPorId(decimal idOcorrencia)
        {
            return _context.Ocorrencias
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Municipio)
                .Include(o => o.Midias)
                .FirstOrDefault(o => o.IdOcorrencia == idOcorrencia);
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
            _context.Ocorrencias.Remove(ocorrencia);
            _context.SaveChanges();
        }

        public void RegistrarHistoricoStatus(HistoricoStatusOcorrenciaEntity historico)
        {
            _context.HistoricoStatusOcorrencias.Add(historico);
            _context.SaveChanges();
        }

        public MidiaOcorrenciaEntity AdicionarMidia(MidiaOcorrenciaEntity midia)
        {
            _context.MidiasOcorrencia.Add(midia);
            _context.SaveChanges();
            return midia;
        }

        public bool ExisteUsuario(decimal idUsuario)
        {
            return _context.Usuarios.Count(u => u.IdUsuario == idUsuario) > 0;
        }

        public bool ExisteCategoria(decimal idCategoria)
        {
            return _context.Categorias.Count(c => c.IdCategoria == idCategoria) > 0;
        }

        public CategoriaEntity? ObterCategoria(decimal idCategoria)
        {
            return _context.Categorias.FirstOrDefault(c => c.IdCategoria == idCategoria);
        }

        public bool ExisteMunicipio(decimal idMunicipio)
        {
            return _context.Municipios.Count(m => m.IdMunicipio == idMunicipio) > 0;
        }

        public decimal ObterProximoSequencialProtocolo()
        {
            var maxId = _context.Ocorrencias.Select(o => (decimal?)o.IdOcorrencia).Max() ?? 0m;
            return maxId + 1m;
        }
    }
}
