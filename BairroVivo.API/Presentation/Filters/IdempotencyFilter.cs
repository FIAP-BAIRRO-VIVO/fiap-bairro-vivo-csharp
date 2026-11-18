using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.Application.Dtos.Ocorrencias;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;
using BairroVivo.Domain.Entities;
using BairroVivo.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.API.Presentation.Filters;

// Garante idempotência em operações de escrita (POST/PUT) utilizando TB_REQUISICAO_IDEMPOTENTE (Oracle v1.2)
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
                            req.Method == HttpMethods.Put;

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

        var idUsuario = _acesso.ObterIdUsuario(context.HttpContext.User);
        if (!idUsuario.HasValue)
        {
            // Sem usuário autenticado, deixa o pipeline de autenticação/autorização agir
            await next();
            return;
        }

        var rota = req.Path.Value?.ToLowerInvariant() ?? "/";

        // Lê e calcula o hash SHA-256 do corpo da requisição (64 caracteres hex)
        req.EnableBuffering();
        string corpoString = string.Empty;
        if (req.Body.CanSeek)
        {
            req.Body.Position = 0;
            using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
            corpoString = await reader.ReadToEndAsync();
            req.Body.Position = 0;
        }

        if (string.IsNullOrWhiteSpace(corpoString) && context.ActionArguments.Count > 0)
        {
            var primeiroArgumento = context.ActionArguments.Values.FirstOrDefault();
            if (primeiroArgumento != null)
                corpoString = JsonSerializer.Serialize(primeiroArgumento);
        }

        var hashCorpo = CalcularSha256(corpoString);

        // Verifica registro existente em TB_REQUISICAO_IDEMPOTENTE
        var existente = await _db.RequisicoesIdempotentes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdUsuario == idUsuario.Value && r.Chave == chave && r.Rota == rota);

        if (existente != null)
        {
            // Se a chave já existe com método ou hash de corpo divergente: 409 Conflict
            if (existente.MetodoHttp != req.Method || existente.HashCorpo != hashCorpo)
            {
                context.Result = new ConflictObjectResult(new
                {
                    codigo = "IDEMPOTENCIA_CONFLITO",
                    mensagem = "Esta chave de idempotência já foi utilizada com dados ou método divergentes."
                });
                return;
            }

            // Replay da operação anterior
            context.HttpContext.Response.Headers["X-Idempotent-Replay"] = "true";

            if (existente.IdRecurso.HasValue)
            {
                var ocorrencia = await _db.Ocorrencias
                    .Include(o => o.Categoria)
                    .Include(o => o.Municipio)
                    .Include(o => o.Midias)
                    .Include(o => o.Votos)
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.IdOcorrencia == existente.IdRecurso.Value);

                if (ocorrencia != null)
                {
                    var dto = ocorrencia.ToResponseDto();

                    if (req.Method == HttpMethods.Post)
                        context.Result = new CreatedResult(rota + "/" + dto.Id, dto);
                    else
                        context.Result = new OkObjectResult(dto);

                    return;
                }
            }

            var statusReplay = req.Method == HttpMethods.Post ? StatusCodes.Status201Created : StatusCodes.Status200OK;
            context.Result = new ObjectResult(new
            {
                mensagem = "Operação previamente processada com sucesso.",
                idRecurso = existente.IdRecurso?.ToString()
            })
            {
                StatusCode = statusReplay
            };
            return;
        }

        var executado = await next();

        if (executado.Exception == null && executado.Result is ObjectResult objectResult)
        {
            var status = objectResult.StatusCode ?? StatusCodes.Status200OK;
            if (status >= 200 && status < 300)
            {
                long? idRecurso = null;
                if (objectResult.Value is OcorrenciaResponseDto ocorrenciaCriada)
                {
                    if (long.TryParse(ocorrenciaCriada.Id, out var parsed))
                        idRecurso = parsed;
                }
                else if (objectResult.Value is not null)
                {
                    var prop = objectResult.Value.GetType().GetProperty("Id")
                               ?? objectResult.Value.GetType().GetProperty("IdOcorrencia")
                               ?? objectResult.Value.GetType().GetProperty("IdAtualizacao");
                    if (prop != null)
                    {
                        var val = prop.GetValue(objectResult.Value);
                        if (val is long lVal) idRecurso = lVal;
                        else if (val is string sVal && long.TryParse(sVal, out var parsedVal)) idRecurso = parsedVal;
                    }
                }

                try
                {
                    _db.RequisicoesIdempotentes.Add(new RequisicaoIdempotenteEntity
                    {
                        IdUsuario = idUsuario.Value,
                        Chave = chave,
                        Rota = rota,
                        MetodoHttp = req.Method,
                        HashCorpo = hashCorpo,
                        IdRecurso = idRecurso,
                        DtCriacao = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Concorrência: outra requisição com a mesma chave gravou primeiro
                }
            }
        }
    }

    private static string CalcularSha256(string texto)
    {
        var bytes = Encoding.UTF8.GetBytes(texto);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
