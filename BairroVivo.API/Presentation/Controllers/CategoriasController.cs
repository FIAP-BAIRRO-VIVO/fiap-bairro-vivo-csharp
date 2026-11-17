using BairroVivo.API.Application.Interfaces;
using BairroVivo.Application.Dtos.Categorias;
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
    [Route("api/v1/categorias")]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaUseCase _categoriaUseCase;
        private readonly IOcorrenciaAccessService _acesso;

        public CategoriasController(
            ICategoriaUseCase categoriaUseCase,
            IOcorrenciaAccessService acesso)
        {
            _categoriaUseCase = categoriaUseCase;
            _acesso = acesso;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar categorias ativas de zeladoria urbana",
            Description = "Retorna todas as categorias ativas (FL_ATIVA = 'S') disponíveis para abertura de ocorrências."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Categorias retornadas com sucesso.", typeof(IEnumerable<CategoriaResponseDto>))]
        public IActionResult ObterTodas()
        {
            return Ok(_categoriaUseCase.ObterTodas());
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Buscar categoria por ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Categoria encontrada com sucesso.", typeof(CategoriaResponseDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Categoria não encontrada.")]
        public IActionResult ObterPorId(long id)
        {
            var categoria = _categoriaUseCase.ObterPorId(id);
            if (categoria == null)
                return NotFound(new { mensagem = "Categoria não encontrada." });

            return Ok(categoria);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar nova categoria (ADMIN)")]
        [SwaggerResponse(StatusCodes.Status201Created, "Categoria criada com sucesso.", typeof(CategoriaResponseDto))]
        public IActionResult Cadastrar([FromBody] CategoriaRequestDto dto)
        {
            var userId = _acesso.ObterIdUsuario(User);
            if (!userId.HasValue || _acesso.ObterPerfil(User, userId.Value) != "ADMIN")
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem gerenciar categorias." });

            var criada = _categoriaUseCase.Cadastrar(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criada.IdCategoria }, criada);
        }

        [HttpPut("{id:long}")]
        [SwaggerOperation(Summary = "Atualizar categoria (ADMIN)")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Categoria atualizada com sucesso.")]
        public IActionResult Atualizar(long id, [FromBody] CategoriaRequestDto dto)
        {
            var userId = _acesso.ObterIdUsuario(User);
            if (!userId.HasValue || _acesso.ObterPerfil(User, userId.Value) != "ADMIN")
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem gerenciar categorias." });

            if (!_categoriaUseCase.Atualizar(id, dto))
                return NotFound(new { mensagem = "Categoria não encontrada." });

            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [SwaggerOperation(Summary = "Inativar categoria (ADMIN)")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Categoria inativada com sucesso.")]
        public IActionResult Remover(long id)
        {
            var userId = _acesso.ObterIdUsuario(User);
            if (!userId.HasValue || _acesso.ObterPerfil(User, userId.Value) != "ADMIN")
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem gerenciar categorias." });

            if (!_categoriaUseCase.Remover(id))
                return NotFound(new { mensagem = "Categoria não encontrada." });

            return NoContent();
        }
    }
}
