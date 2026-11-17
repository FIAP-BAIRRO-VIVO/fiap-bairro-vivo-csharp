using System.Data;
using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Domain.Entities;
using BairroVivo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.API.Infrastructure.Data.Repositories;

// Integra objetos PL/SQL da disciplina Mastering Relational and Non-Relational Database:
// Package PKG_COMUNIDADE (PR_REGISTRAR_ATUALIZACAO e FN_TOTAL_VOTOS_ATIVOS).
public class IntegracaoOracleRepository : IIntegracaoOracleRepository
{
    private readonly ApplicationContext _context;

    public IntegracaoOracleRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<long> ExecutarRegistrarAtualizacaoAsync(
        long idOcorrencia, long idUsuario, string tipo, string? texto, string flAnonimo)
    {
        if (!_context.Database.IsRelational())
        {
            // Em ambiente de testes InMemory, simula com 100% de fidelidade a procedure PL/SQL PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.FlAtivo == "S");

            if (usuario == null || usuario.Papel != "CIDADAO")
                throw new InvalidOperationException("Somente cidadao ativo pode atualizar.");

            var ocorrencia = await _context.Ocorrencias
                .FirstOrDefaultAsync(o => o.IdOcorrencia == idOcorrencia && o.FlExcluida == "N");

            if (ocorrencia == null)
                throw new KeyNotFoundException("Ocorrencia nao encontrada ou excluida.");

            var ehVoto = (ocorrencia.Status == "ABERTA" && tipo == "RESOLVIDO") ||
                         (ocorrencia.Status == "RESOLVIDA" && tipo == "PERSISTE");

            if (ehVoto)
            {
                var jaVotou = await _context.Votos
                    .AnyAsync(v => v.IdOcorrencia == idOcorrencia &&
                                   v.IdUsuario == idUsuario &&
                                   v.NumCiclo == ocorrencia.NumCiclo);

                if (jaVotou)
                    throw new InvalidOperationException("Usuario ja votou no ciclo atual.");
            }

            var atualizacao = new AtualizacaoOcorrenciaEntity
            {
                IdOcorrencia = idOcorrencia,
                IdUsuario = idUsuario,
                Tipo = tipo,
                Texto = texto,
                FlAnonimo = flAnonimo,
                DtCriacao = DateTime.UtcNow
            };

            _context.Atualizacoes.Add(atualizacao);
            await _context.SaveChangesAsync();

            if (ehVoto)
            {
                var voto = new VotoSituacaoEntity
                {
                    IdOcorrencia = idOcorrencia,
                    IdAtualizacao = atualizacao.IdAtualizacao,
                    IdUsuario = idUsuario,
                    NumCiclo = ocorrencia.NumCiclo,
                    TipoVoto = tipo,
                    DtVoto = DateTime.UtcNow
                };
                _context.Votos.Add(voto);
                await _context.SaveChangesAsync();

                var totalVotosCiclo = await _context.Votos
                    .CountAsync(v => v.IdOcorrencia == idOcorrencia &&
                                     v.NumCiclo == ocorrencia.NumCiclo &&
                                     v.TipoVoto == tipo);

                if (totalVotosCiclo >= 2)
                {
                    var statusAnterior = ocorrencia.Status;
                    var cicloAnterior = ocorrencia.NumCiclo;
                    var novoStatus = ocorrencia.Status == "ABERTA" ? "RESOLVIDA" : "ABERTA";

                    ocorrencia.Status = novoStatus;
                    ocorrencia.NumCiclo += 1;
                    ocorrencia.IdAutorTransicao = idUsuario;
                    ocorrencia.DtAtualizacao = DateTime.UtcNow;

                    _context.HistoricoStatus.Add(new HistoricoStatusOcorrenciaEntity
                    {
                        IdOcorrencia = idOcorrencia,
                        IdUsuario = idUsuario,
                        Operacao = "UPDATE",
                        StatusAnterior = statusAnterior,
                        StatusNovo = novoStatus,
                        CicloAnterior = cicloAnterior,
                        CicloNovo = ocorrencia.NumCiclo,
                        DtEvento = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                }
            }

            return atualizacao.IdAtualizacao;
        }

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "BEGIN PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO(:p_id_ocorrencia, :p_id_usuario, :p_tipo, :p_texto, :p_fl_anonimo, :p_id_atualizacao); END;";

        AdicionarParametro(cmd, "p_id_ocorrencia", idOcorrencia);
        AdicionarParametro(cmd, "p_id_usuario", idUsuario);
        AdicionarParametro(cmd, "p_tipo", tipo);
        AdicionarParametro(cmd, "p_texto", (object?)texto ?? DBNull.Value);
        AdicionarParametro(cmd, "p_fl_anonimo", flAnonimo);

        var paramOut = cmd.CreateParameter();
        paramOut.ParameterName = "p_id_atualizacao";
        paramOut.DbType = DbType.Int64;
        paramOut.Direction = ParameterDirection.Output;
        cmd.Parameters.Add(paramOut);

        await cmd.ExecuteNonQueryAsync();

        return paramOut.Value is not null and not DBNull ? Convert.ToInt64(paramOut.Value) : 0;
    }

    public async Task<int> ContarVotosAtivosAsync(long idOcorrencia, string tipoVoto)
    {
        if (!_context.Database.IsRelational())
        {
            var ocorrencia = await _context.Ocorrencias.FirstOrDefaultAsync(o => o.IdOcorrencia == idOcorrencia);
            if (ocorrencia == null) return 0;

            return await _context.Votos
                .CountAsync(v => v.IdOcorrencia == idOcorrencia &&
                                 v.NumCiclo == ocorrencia.NumCiclo &&
                                 v.TipoVoto == tipoVoto);
        }

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT PKG_COMUNIDADE.FN_TOTAL_VOTOS_ATIVOS(:p_id_ocorrencia, :p_tipo_voto) FROM DUAL";
        AdicionarParametro(cmd, "p_id_ocorrencia", idOcorrencia);
        AdicionarParametro(cmd, "p_tipo_voto", tipoVoto);

        var valor = await cmd.ExecuteScalarAsync();
        return valor is null or DBNull ? 0 : Convert.ToInt32(valor);
    }

    private static void AdicionarParametro(IDbCommand cmd, string nome, object valor)
    {
        var parametro = cmd.CreateParameter();
        parametro.ParameterName = nome;
        parametro.Value = valor;
        cmd.Parameters.Add(parametro);
    }
}
