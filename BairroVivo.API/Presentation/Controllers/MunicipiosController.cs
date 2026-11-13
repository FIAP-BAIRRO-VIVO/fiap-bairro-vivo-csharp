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
    [Route("api/municipios")]
    public class MunicipiosController : ControllerBase
    {
        private readonly IMunicipioUseCase _municipioUseCase;

        public MunicipiosController(IMunicipioUseCase municipioUseCase)
        {
            _municipioUseCase = municipioUseCase;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar municípios atendidos",
            Description = "Retorna todos os municípios ativos cadastrados na plataforma Bairro Vivo."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Municípios retornados com sucesso.")]
        public IActionResult ObterTodos()
        {
            return Ok(_municipioUseCase.ObterTodos());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Buscar município por ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Município encontrado com sucesso.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Município não encontrado.")]
        public IActionResult ObterPorId(decimal id)
        {
            var municipio = _municipioUseCase.ObterPorId(id);
            if (municipio == null)
                return NotFound(new { mensagem = "Município não encontrado." });

            return Ok(municipio);
        }
    }
}
