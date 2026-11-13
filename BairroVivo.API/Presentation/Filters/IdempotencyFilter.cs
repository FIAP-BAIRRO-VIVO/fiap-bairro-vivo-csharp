using System.Text.Json;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.Domain.Entities;
using BairroVivo.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.API.Presentation.Filters;

// Garante idempotencia em operacoes de escrita (POST/PUT/PATCH) utilizando a tabela TB_REQUISICAO_IDEMPOTENTE.
public class IdempotencyFilter : IAsyncActionFilter
{
    public const string HeaderName = "Idempotency-Key";
    private readonly ApplicationContext _db;
    private readonly IOcorrenciaAccessService _acesso;

    public IdempotencyFilter(ApplicationContext db, IOcorrenciaAccessService acesso)
    {
        _db = db;
        _acesso = acesso;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var req = context.HttpContext.Request;
        var metodoEscrita = req.Method == HttpMethods.Post ||
                            req.Method == HttpMethods.Put ||
                            req.Method == HttpMethods.Patch;

        if (!metodoEscrita || !req.Headers.TryGetValue(HeaderName, out var valoresChave))
        {
            await next();
            return;
        }

        var chave = valoresChave.ToString().Trim();
        if (string.IsNullOrWhiteSpace(chave))
        {
            await next();
            return;
        }

        var agora = DateTime.Now;
        var existente = await _db.RequisicoesIdempotentes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ChaveIdempotencia == chave && r.ExpiraEm > agora);

        if (existente != null)
        {
            context.HttpContext.Response.Headers["X-Idempotent-Replay"] = "true";
            context.Result = new ContentResult
            {
                StatusCode = existente.StatusHttpResposta,
                ContentType = "application/json",
                Content = existente.CorpoResposta ?? "{}"
            };
            return;
        }

        var executado = await next();

        if (executado.Exception == null && executado.Result is ObjectResult objectResult)
        {
            var status = objectResult.StatusCode ?? StatusCodes.Status200OK;
            if (status >= 200 && status < 300)
            {
                var corpoJson = JsonSerializer.Serialize(
                    objectResult.Value,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                _db.RequisicoesIdempotentes.Add(new RequisicaoIdempotenteEntity
                {
                    ChaveIdempotencia = chave,
                    IdUsuario = _acesso.ObterIdUsuario(context.HttpContext.User),
                    Endpoint = req.Path.Value ?? "/",
                    MetodoHttp = req.Method,
                    StatusHttpResposta = status,
                    CorpoResposta = corpoJson,
                    CriadaEm = agora,
                    ExpiraEm = agora.AddHours(24)
                });
                await _db.SaveChangesAsync();
            }
        }
    }
}
