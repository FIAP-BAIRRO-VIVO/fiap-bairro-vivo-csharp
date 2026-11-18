using BairroVivo.API.Application.Interfaces;
using BairroVivo.Application.Dtos.Relatorios;
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
    [Route("api/admin/relatorios")]
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioUseCase _relatorioUseCase;
        private readonly IOcorrenciaAccessService _acesso;

        public RelatoriosController(
            IRelatorioUseCase relatorioUseCase,
            IOcorrenciaAccessService acesso)
        {
            _relatorioUseCase = relatorioUseCase;
            _acesso = acesso;
        }

        [HttpGet("ocorrencias")]
        [SwaggerOperation(
            Summary = "Relatório agregado de ocorrências (ADMIN)",
            Description = "Retorna contadores agregados por município, categoria e status baseados na VW_RELATORIO_OCORRENCIAS da base Oracle v1.2."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Relatório retornado com sucesso.", typeof(IEnumerable<RelatorioOcorrenciaResponseDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Acesso restrito ao perfil ADMIN.")]
        public IActionResult ObterRelatorioOcorrencias(
            [FromQuery] long? idMunicipio = null,
            [FromQuery] long? idCategoria = null,
            [FromQuery] string? status = null)
        {
            var userId = _acesso.ObterIdUsuario(User);
            if (!userId.HasValue || _acesso.ObterPerfil(User, userId.Value) != "ADMIN")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensagem = "Apenas administradores podem consultar relatórios executivos."
                });
            }

            var dados = _relatorioUseCase.ObterRelatorioOcorrencias(idMunicipio, idCategoria, status);
            return Ok(dados);
        }
    }
}
