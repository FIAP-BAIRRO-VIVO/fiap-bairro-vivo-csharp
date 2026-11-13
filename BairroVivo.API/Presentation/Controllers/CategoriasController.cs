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
            Summary = "Listar categorias de zeladoria urbana",
            Description = "Retorna todas as categorias ativas disponíveis para abertura de ocorrências."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Categorias retornadas com sucesso.")]
        public IActionResult ObterTodas()
        {
            return Ok(_categoriaUseCase.ObterTodas());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Buscar categoria por ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Categoria encontrada com sucesso.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Categoria não encontrada.")]
        public IActionResult ObterPorId(decimal id)
        {
            var categoria = _categoriaUseCase.ObterPorId(id);
            if (categoria == null)
                return NotFound(new { mensagem = "Categoria não encontrada." });

            return Ok(categoria);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar nova categoria (ADMIN)")]
        [SwaggerResponse(StatusCodes.Status201Created, "Categoria criada com sucesso.")]
        public IActionResult Cadastrar([FromBody] CategoriaRequestDto dto)
        {
            var userId = _acesso.ObterIdUsuario(User);
            if (!userId.HasValue || _acesso.ObterPerfil(User, userId.Value) != "ADMIN")
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem gerenciar categorias." });

            var criada = _categoriaUseCase.Cadastrar(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criada.IdCategoria }, criada);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar categoria (ADMIN)")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Categoria atualizada com sucesso.")]
        public IActionResult Atualizar(decimal id, [FromBody] CategoriaRequestDto dto)
        {
            var userId = _acesso.ObterIdUsuario(User);
            if (!userId.HasValue || _acesso.ObterPerfil(User, userId.Value) != "ADMIN")
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem gerenciar categorias." });

            if (!_categoriaUseCase.Atualizar(id, dto))
                return NotFound(new { mensagem = "Categoria não encontrada." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Inativar categoria (ADMIN)")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Categoria inativada com sucesso.")]
        public IActionResult Remover(decimal id)
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
