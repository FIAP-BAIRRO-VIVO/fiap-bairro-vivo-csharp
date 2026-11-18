using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Application.Dtos.Ocorrencias;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BairroVivo.API.Presentation.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Route("api/ocorrencias/minhas")]
[Route("api/ocorrencias/me")]
public class MinhasOcorrenciasController : ControllerBase
{
    private readonly IOcorrenciaAccessService _acesso;
    private readonly IOcorrenciaUseCase _ocorrencias;

    public MinhasOcorrenciasController(IOcorrenciaAccessService acesso, IOcorrenciaUseCase ocorrencias)
    {
        _acesso = acesso;
        _ocorrencias = ocorrencias;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar minhas ocorrências (CIDADAO)",
        Description = "Retorna todas as ocorrências do cidadão logado no JWT (inclusive anônimas), com paginação e links HATEOAS."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Ocorrências do cidadão retornadas com sucesso.", typeof(PageResponseDto<OcorrenciaItemDto>))]
    public IActionResult Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? ordem = "recentes")
    {
        var id = _acesso.ObterIdUsuario(User);
        if (!id.HasValue)
            return Unauthorized(new { mensagem = "Token JWT sem identificador de cidadão válido." });

        var resultado = _ocorrencias.ObterPorUsuarioPaginado(id.Value, pagina, tamanho, status, ordem);
        var caminho = "/api/ocorrencias/minhas";
        var query = $"&status={Uri.EscapeDataString(status ?? "")}&ordem={Uri.EscapeDataString(ordem ?? "recentes")}";

        var itens = resultado.Items.Select(o => new OcorrenciaItemDto
        {
            Ocorrencia = o,
            Links = HateoasLinks.ParaOcorrencia(o.IdOcorrencia)
        }).ToList();

        var links = HateoasLinks.ParaPagina(caminho, resultado.Pagina, resultado.Tamanho, resultado.Total)
            .Select(link => new LinkDto(link.Href + query, link.Rel, link.Method)).ToList();

        return Ok(new PageResponseDto<OcorrenciaItemDto>
        {
            Deslocamento = resultado.Deslocamento,
            RegistroRetornado = resultado.RegistroRetornado,
            TotalRegistros = resultado.TotalRegistros,
            Items = itens,
            Links = links
        });
    }
}
