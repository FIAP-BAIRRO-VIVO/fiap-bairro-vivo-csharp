using System.Security.Claims;

namespace BairroVivo.API.Application.Interfaces;

public interface IOcorrenciaAccessService
{
    long? ObterIdUsuario(ClaimsPrincipal usuario);
    string ObterPerfil(ClaimsPrincipal usuario, long idUsuario);
    bool UsuarioAtivo(long idUsuario);
    bool ExisteOcorrencia(long idOcorrencia);
    bool PodeModificarOcorrencia(long idUsuario, long idOcorrencia, string perfil);
    bool EhAutorOcorrencia(long idUsuario, long idOcorrencia);
    bool EhAutorAtualizacao(long idUsuario, long idAtualizacao);
}
