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

    public async Task ExecutarRegistrarAtualizacaoAsync(
        decimal idOcorrencia, decimal idUsuario, string tipo, string? comentario, string? urlFoto)
    {
        if (!_context.Database.IsRelational())
        {
            // Em ambiente de testes InMemory, simula o comportamento da procedure PL/SQL
            _context.AtualizacoesOcorrencia.Add(new AtualizacaoOcorrenciaEntity
            {
                IdOcorrencia = idOcorrencia,
                IdUsuario = idUsuario,
                Tipo = tipo,
                Comentario = comentario,
                UrlFotoEvidencia = urlFoto,
                CriadaEm = DateTime.Now
            });
            var ocorrencia = await _context.Ocorrencias.FirstOrDefaultAsync(o => o.IdOcorrencia == idOcorrencia);
            if (ocorrencia != null && tipo == "CONFIRMACAO")
            {
                ocorrencia.Confirmacoes += 1;
                if (ocorrencia.Confirmacoes >= 3)
                    ocorrencia.ValidadaComunidade = "S";
            }
            await _context.SaveChangesAsync();
            return;
        }

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "BEGIN PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO(:p_id_ocorrencia, :p_id_usuario, :p_tipo, :p_comentario, :p_url_foto); END;";

        AdicionarParametro(cmd, "p_id_ocorrencia", idOcorrencia);
        AdicionarParametro(cmd, "p_id_usuario", idUsuario);
        AdicionarParametro(cmd, "p_tipo", tipo);
        AdicionarParametro(cmd, "p_comentario", (object?)comentario ?? DBNull.Value);
        AdicionarParametro(cmd, "p_url_foto", (object?)urlFoto ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> ContarVotosAtivosAsync(decimal idOcorrencia)
    {
        if (!_context.Database.IsRelational())
        {
            return await _context.AtualizacoesOcorrencia
                .CountAsync(a => a.IdOcorrencia == idOcorrencia && a.Tipo == "CONFIRMACAO");
        }

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT PKG_COMUNIDADE.FN_TOTAL_VOTOS_ATIVOS(:p_id_ocorrencia) FROM DUAL";
        AdicionarParametro(cmd, "p_id_ocorrencia", idOcorrencia);

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
