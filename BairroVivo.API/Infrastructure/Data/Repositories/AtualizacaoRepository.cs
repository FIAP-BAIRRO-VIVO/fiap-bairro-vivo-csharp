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

        public IEnumerable<AtualizacaoOcorrenciaEntity> ObterPorOcorrencia(long idOcorrencia) =>
            _context.Atualizacoes
                .Include(a => a.Usuario)
                .Include(a => a.Midias)
                .Include(a => a.Voto)
                .Include(a => a.Ocorrencia)
                .Where(a => a.IdOcorrencia == idOcorrencia)
                .OrderByDescending(a => a.DtCriacao)
                .ToList();

        public (IEnumerable<AtualizacaoOcorrenciaEntity> Itens, int Total) ObterPorOcorrenciaPaginado(
            long idOcorrencia, int deslocamento, int limite)
        {
            var consulta = _context.Atualizacoes
                .Include(a => a.Usuario)
                .Include(a => a.Midias)
                .Include(a => a.Voto)
                .Include(a => a.Ocorrencia)
                .Where(a => a.IdOcorrencia == idOcorrencia)
                .OrderByDescending(a => a.DtCriacao)
                .ThenByDescending(a => a.IdAtualizacao);

            var total = consulta.Count();
            var itens = consulta.Skip(deslocamento).Take(limite).ToList();
            return (itens, total);
        }

        public AtualizacaoOcorrenciaEntity? ObterPorId(long idAtualizacao) =>
            _context.Atualizacoes
                .Include(a => a.Usuario)
                .Include(a => a.Midias)
                .Include(a => a.Voto)
                .Include(a => a.Ocorrencia)
                .FirstOrDefault(a => a.IdAtualizacao == idAtualizacao);

        public bool JaVotouNoCiclo(long idOcorrencia, long idUsuario, int numCiclo) =>
            _context.Votos.Any(v => v.IdOcorrencia == idOcorrencia && v.IdUsuario == idUsuario && v.NumCiclo == numCiclo);

        public AtualizacaoOcorrenciaEntity Registrar(AtualizacaoOcorrenciaEntity atualizacao)
        {
            _context.Atualizacoes.Add(atualizacao);
            _context.SaveChanges();
            return atualizacao;
        }

        public void RegistrarVoto(VotoSituacaoEntity voto)
        {
            _context.Votos.Add(voto);
            _context.SaveChanges();
        }

        public int ContarVotosCiclo(long idOcorrencia, int numCiclo, string tipoVoto) =>
            _context.Votos.Count(v => v.IdOcorrencia == idOcorrencia && v.NumCiclo == numCiclo && v.TipoVoto == tipoVoto);

        public MidiaOcorrenciaEntity AdicionarMidia(MidiaOcorrenciaEntity midia)
        {
            _context.Midias.Add(midia);
            _context.SaveChanges();
            return midia;
        }
    }
}
