using System.Security.Claims;
using BairroVivo.API.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.Infrastructure.Data.Repositories;

// Consulta os vínculos no Oracle (TB_USUARIO e TB_OCORRENCIA), nunca aceitando ID livre sem validação JWT.
public class OcorrenciaAccessService : IOcorrenciaAccessService
{
    private readonly ApplicationContext _db;

    public OcorrenciaAccessService(ApplicationContext db)
    {
        _db = db;
    }

    public long? ObterIdUsuario(ClaimsPrincipal usuario)
    {
        var claimId = usuario.FindFirst("userId")?.Value
            ?? usuario.FindFirst("idUsuario")?.Value
            ?? usuario.FindFirst("sub")?.Value;

        if (long.TryParse(claimId, out var id) && id > 0)
            return id;

        // Suporta também o padrão Spring Security onde "sub" contém o e-mail do usuário
        var email = usuario.FindFirst("email")?.Value ?? claimId;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();
            return _db.Usuarios.AsNoTracking()
                .Where(u => u.Email.ToLower() == emailNormalizado)
                .Select(u => (long?)u.IdUsuario)
                .FirstOrDefault();
        }

        return null;
    }

    public string ObterPerfil(ClaimsPrincipal usuario, long idUsuario)
    {
        var perfilClaim = usuario.FindFirst("perfil")?.Value
            ?? usuario.FindFirst("roles")?.Value
            ?? usuario.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrWhiteSpace(perfilClaim))
        {
            if (perfilClaim.Contains("ADMIN", StringComparison.OrdinalIgnoreCase))
                return "ADMIN";
            return perfilClaim.Trim().ToUpperInvariant();
        }

        return _db.Usuarios.AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => u.Papel)
            .FirstOrDefault() ?? "CIDADAO";
    }

    public bool UsuarioAtivo(long idUsuario) =>
        _db.Usuarios.AsNoTracking()
            .Count(u => u.IdUsuario == idUsuario && u.FlAtivo == "S") > 0;

    public bool ExisteOcorrencia(long idOcorrencia) =>
        _db.Ocorrencias.AsNoTracking().Count(o => o.IdOcorrencia == idOcorrencia && o.FlExcluida == "N") > 0;

    public bool EhAutorOcorrencia(long idUsuario, long idOcorrencia)
    {
        return _db.Ocorrencias.AsNoTracking()
            .Any(o => o.IdOcorrencia == idOcorrencia && o.IdUsuario == idUsuario && o.FlExcluida == "N");
    }

    public bool EhAutorAtualizacao(long idUsuario, long idAtualizacao)
    {
        return _db.Atualizacoes.AsNoTracking()
            .Any(a => a.IdAtualizacao == idAtualizacao && a.IdUsuario == idUsuario);
    }

    public bool PodeModificarOcorrencia(long idUsuario, long idOcorrencia, string perfil)
    {
        if (!UsuarioAtivo(idUsuario))
            return false;

        // Conforme a especificação do Bairro Vivo v1.2:
        // Apenas o autor original CIDADAO pode editar ou excluir logicamente sua ocorrência.
        // ADMIN não altera ocorrências.
        return EhAutorOcorrencia(idUsuario, idOcorrencia);
    }
}
