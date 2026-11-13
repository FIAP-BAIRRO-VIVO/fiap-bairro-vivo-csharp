using BairroVivo.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BairroVivo.API.Presentation.Filters;

// Filtro MVC seguindo o padrao de PetAccessFilter: valida usuario ativo no Oracle e permissoes da ocorrencia.
public class OcorrenciaAccessFilter : IAsyncActionFilter
{
    private readonly IOcorrenciaAccessService _acesso;

    public OcorrenciaAccessFilter(IOcorrenciaAccessService acesso)
    {
        _acesso = acesso;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Rotas publicas nao exigem validacao de usuario no banco.
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await next();
            return;
        }

        var userId = _acesso.ObterIdUsuario(context.HttpContext.User);
        if (!userId.HasValue)
        {
            context.Result = new UnauthorizedObjectResult(new { mensagem = "JWT sem userId ou sub valido." });
            return;
        }

        if (!_acesso.UsuarioAtivo(userId.Value))
        {
            context.Result = new ObjectResult(new { mensagem = "Usuario inativo ou inexistente." }) { StatusCode = 403 };
            return;
        }

        var req = context.HttpContext.Request;
        var rota = req.RouteValues;
        var perfil = _acesso.ObterPerfil(context.HttpContext.User, userId.Value);

        if (rota.TryGetValue("idUsuario", out var usuarioObj) &&
            decimal.TryParse(usuarioObj?.ToString(), out var rotaIdUsuario))
        {
            if (perfil != "ADMIN" && rotaIdUsuario != userId.Value)
            {
                context.Result = new ObjectResult(new { mensagem = "Cidadão não autorizado para operar em nome de outro usuário." }) { StatusCode = 403 };
                return;
            }
        }

        decimal idOcorrencia = 0;
        var rotaDeOcorrencia = (rota.TryGetValue("ocorrenciaId", out var ocObj) &&
                                decimal.TryParse(ocObj?.ToString(), out idOcorrencia)) ||
                               (req.Path.Value?.StartsWith("/api/ocorrencias/", StringComparison.OrdinalIgnoreCase) == true &&
                                rota.TryGetValue("id", out var idObj) &&
                                decimal.TryParse(idObj?.ToString(), out idOcorrencia));

        if (rotaDeOcorrencia)
        {
            if (!_acesso.ExisteOcorrencia(idOcorrencia))
            {
                context.Result = new NotFoundObjectResult(new { mensagem = "Ocorrência não encontrada." });
                return;
            }

            var metodoModificacaoDireta =
                req.Method == HttpMethods.Put ||
                req.Method == HttpMethods.Patch ||
                req.Method == HttpMethods.Delete;

            if (metodoModificacaoDireta && !_acesso.PodeModificarOcorrencia(userId.Value, idOcorrencia, perfil))
            {
                context.Result = new ObjectResult(new { mensagem = "Sem permissão para alterar ou remover esta ocorrência." }) { StatusCode = 403 };
                return;
            }
        }

        await next();
    }
}
