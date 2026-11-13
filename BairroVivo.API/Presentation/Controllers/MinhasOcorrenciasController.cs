using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Application.Dtos.Ocorrencias;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BairroVivo.API.Presentation.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    public IActionResult Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? ordem = "recentes")
    {
        var id = _acesso.ObterIdUsuario(User);
        if (!id.HasValue)
            return Forbid();

        var resultado = _ocorrencias.ObterPorUsuarioPaginado(id.Value, pagina, tamanho, status, ordem);
        var caminho = "/api/ocorrencias/me";
        var query = $"&status={Uri.EscapeDataString(status ?? "")}&ordem={Uri.EscapeDataString(ordem ?? "recentes")}";

        var itens = resultado.Items.Select(o => new OcorrenciaItemDto
        {
            Ocorrencia = o,
            Links = HateoasLinks.ParaOcorrencia(o.IdOcorrencia)
        }).ToList();

        resultado.Links = HateoasLinks.ParaPagina(caminho, resultado.Pagina, resultado.Tamanho, resultado.Total)
            .Select(link => new LinkDto(link.Href + query, link.Rel, link.Method)).ToList();

        return Ok(new PageResponseDto<OcorrenciaItemDto>
        {
            Deslocamento = resultado.Deslocamento,
            RegistroRetornado = resultado.RegistroRetornado,
            TotalRegistros = resultado.TotalRegistros,
            Items = itens,
            Links = resultado.Links
        });
    }
}
