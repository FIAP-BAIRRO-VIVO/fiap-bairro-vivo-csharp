using BairroVivo.Application.Dtos.Municipios;
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
    [Route("api/v1/municipios")]
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
            Description = "Retorna os municípios cadastrados na base Oracle v1.2, com filtros opcionais por nome e UF."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Municípios retornados com sucesso.", typeof(IEnumerable<MunicipioResponseDto>))]
        public IActionResult ObterTodos(
            [FromQuery] string? nome = null,
            [FromQuery] string? uf = null)
        {
            return Ok(_municipioUseCase.ObterTodos(nome, uf));
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Buscar município por ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Município encontrado com sucesso.", typeof(MunicipioResponseDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Município não encontrado.")]
        public IActionResult ObterPorId(long id)
        {
            var municipio = _municipioUseCase.ObterPorId(id);
            if (municipio == null)
                return NotFound(new { mensagem = "Município não encontrado." });

            return Ok(municipio);
        }
    }
}
