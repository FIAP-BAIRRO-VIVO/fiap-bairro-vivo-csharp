using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Presentation;
using BairroVivo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BairroVivo.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("api/relatorios-bairro")]
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioBairroUseCase _relatorioUseCase;

        public RelatoriosController(IRelatorioBairroUseCase relatorioUseCase)
        {
            _relatorioUseCase = relatorioUseCase;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar relatórios executivos de bairro gerados por IA",
            Description = "Retorna os relatórios periódicos de bairro com paginação e links HATEOAS."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Relatórios retornados com sucesso.")]
        public IActionResult ObterPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanho = 10,
            [FromQuery] string? bairro = null,
            [FromQuery] string? periodoRef = null)
        {
            var resultado = _relatorioUseCase.ObterPaginado(pagina, tamanho, bairro, periodoRef);
            var itens = resultado.Items.ToList();
            foreach (var item in itens)
            {
                item.Links = new List<LinkDto>
                {
                    new($"/api/relatorios-bairro/{item.IdRelatorio}", "self", "GET"),
                    new("/api/relatorios-bairro", "collection", "GET")
                };
            }

            var caminho = "/api/relatorios-bairro";
            var query = $"&bairro={Uri.EscapeDataString(bairro ?? "")}&periodoRef={Uri.EscapeDataString(periodoRef ?? "")}";
            resultado.Items = itens;
            resultado.Links = HateoasLinks.ParaPagina(caminho, resultado.Pagina, resultado.Tamanho, resultado.Total)
                .Select(link => new LinkDto(link.Href + query, link.Rel, link.Method)).ToList();

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Buscar relatório de bairro por ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Relatório encontrado com sucesso.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Relatório não encontrado.")]
        public IActionResult ObterPorId(decimal id)
        {
            var relatorio = _relatorioUseCase.ObterPorId(id);
            if (relatorio == null)
                return NotFound(new { mensagem = "Relatório de bairro não encontrado." });

            relatorio.Links = new List<LinkDto>
            {
                new($"/api/relatorios-bairro/{relatorio.IdRelatorio}", "self", "GET"),
                new("/api/relatorios-bairro", "collection", "GET")
            };

            return Ok(relatorio);
        }
    }
}
