using System.Security.Claims;
using BairroVivo.API.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.Infrastructure.Data.Repositories;

// Consulta os vinculos no Oracle (TB_USUARIO e TB_OCORRENCIA), nunca aceitando ID livre sem validacao JWT.
public class OcorrenciaAccessService : IOcorrenciaAccessService
{
    private readonly ApplicationContext _db;

    public OcorrenciaAccessService(ApplicationContext db)
    {
        _db = db;
    }

    public decimal? ObterIdUsuario(ClaimsPrincipal usuario)
    {
        var claimId = usuario.FindFirst("userId")?.Value
            ?? usuario.FindFirst("idUsuario")?.Value
            ?? usuario.FindFirst("sub")?.Value;

        if (decimal.TryParse(claimId, out var id) && id > 0)
            return id;

        // Suporta tambem o padrao Spring Security onde "sub" contem o e-mail do usuario
        var email = usuario.FindFirst("email")?.Value ?? claimId;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();
            return _db.Usuarios.AsNoTracking()
                .Where(u => u.Email.ToLower() == emailNormalizado)
                .Select(u => (decimal?)u.IdUsuario)
                .FirstOrDefault();
        }

        return null;
    }

    public string ObterPerfil(ClaimsPrincipal usuario, decimal idUsuario)
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
            .Select(u => u.Perfil)
            .FirstOrDefault() ?? "CIDADAO";
    }

    public bool UsuarioAtivo(decimal idUsuario) =>
        _db.Usuarios.AsNoTracking()
            .Count(u => u.IdUsuario == idUsuario && (u.Ativo == "S" || u.Ativo == "ATIVO")) > 0;

    public bool ExisteOcorrencia(decimal idOcorrencia) =>
        _db.Ocorrencias.AsNoTracking().Count(o => o.IdOcorrencia == idOcorrencia) > 0;

    public bool PodeModificarOcorrencia(decimal idUsuario, decimal idOcorrencia, string perfil)
    {
        if (!UsuarioAtivo(idUsuario))
            return false;

        if (perfil == "ADMIN")
            return true;

        var ocorrencia = _db.Ocorrencias.AsNoTracking()
            .Where(o => o.IdOcorrencia == idOcorrencia)
            .Select(o => new { o.IdUsuario })
            .FirstOrDefault();

        if (ocorrencia is null)
            return false;

        return ocorrencia.IdUsuario == idUsuario;
    }
}
