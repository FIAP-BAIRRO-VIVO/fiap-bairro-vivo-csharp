using System.Security.Claims;

namespace BairroVivo.API.Application.Interfaces;

public interface IOcorrenciaAccessService
{
    decimal? ObterIdUsuario(ClaimsPrincipal usuario);
    string ObterPerfil(ClaimsPrincipal usuario, decimal idUsuario);
    bool UsuarioAtivo(decimal idUsuario);
    bool ExisteOcorrencia(decimal idOcorrencia);
    bool PodeModificarOcorrencia(decimal idUsuario, decimal idOcorrencia, string perfil);
}
