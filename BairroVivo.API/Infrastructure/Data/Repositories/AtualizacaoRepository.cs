using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.Infrastructure.Data.Repositories
{
    public class AtualizacaoRepository : IAtualizacaoRepository
    {
        private readonly ApplicationContext _context;

        public AtualizacaoRepository(ApplicationContext context)
        {
            _context = context;
        }

        public IEnumerable<AtualizacaoOcorrenciaEntity> ObterPorOcorrencia(decimal idOcorrencia) =>
            _context.AtualizacoesOcorrencia
                .Include(a => a.Usuario)
                .Where(a => a.IdOcorrencia == idOcorrencia)
                .OrderByDescending(a => a.CriadaEm)
                .ToList();

        public (IEnumerable<AtualizacaoOcorrenciaEntity> Itens, int Total) ObterPorOcorrenciaPaginado(
            decimal idOcorrencia, int deslocamento, int limite)
        {
            var consulta = _context.AtualizacoesOcorrencia
                .Include(a => a.Usuario)
                .Where(a => a.IdOcorrencia == idOcorrencia)
                .OrderByDescending(a => a.CriadaEm)
                .ThenByDescending(a => a.IdAtualizacao);

            var total = consulta.Count();
            var itens = consulta.Skip(deslocamento).Take(limite).ToList();
            return (itens, total);
        }

        public AtualizacaoOcorrenciaEntity? ObterPorId(decimal idAtualizacao) =>
            _context.AtualizacoesOcorrencia
                .Include(a => a.Usuario)
                .FirstOrDefault(a => a.IdAtualizacao == idAtualizacao);

        public bool JaVotou(decimal idOcorrencia, decimal idUsuario, string tipo) =>
            _context.AtualizacoesOcorrencia
                .Count(a => a.IdOcorrencia == idOcorrencia && a.IdUsuario == idUsuario && a.Tipo == tipo) > 0;

        public AtualizacaoOcorrenciaEntity Registrar(AtualizacaoOcorrenciaEntity atualizacao)
        {
            _context.AtualizacoesOcorrencia.Add(atualizacao);
            _context.SaveChanges();
            return atualizacao;
        }
    }
}
