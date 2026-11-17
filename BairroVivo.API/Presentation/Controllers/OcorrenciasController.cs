using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Application.Dtos.Ocorrencias;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.API.Presentation;
using BairroVivo.Application.Dtos.Ocorrencias;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;
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
    [Route("api/v1")]
    [Route("api")]
    public class OcorrenciasController : ControllerBase
    {
        private readonly IOcorrenciaUseCase _ocorrenciaUseCase;
        private readonly IOcorrenciaAccessService _acesso;

        public OcorrenciasController(
            IOcorrenciaUseCase ocorrenciaUseCase,
            IOcorrenciaAccessService acesso)
        {
            _ocorrenciaUseCase = ocorrenciaUseCase;
            _acesso = acesso;
        }

        [HttpGet("ocorrencias")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar ocorrências urbanas públicas / mapa",
            Description = "Retorna ocorrências ativas com autor público mascarado, filtros por bounding box geográfico (minLat, maxLat, minLon, maxLon), categoria, município, status e links HATEOAS."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lista paginada de ocorrências retornada com sucesso.", typeof(PageResponseDto<OcorrenciaItemDto>))]
        public IActionResult ObterPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanho = 10,
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] decimal? minLat = null,
            [FromQuery] decimal? maxLat = null,
            [FromQuery] decimal? minLon = null,
            [FromQuery] decimal? maxLon = null,
            [FromQuery] long? categoriaId = null,
            [FromQuery] long? idCategoria = null,
            [FromQuery] long? municipioId = null,
            [FromQuery] long? idMunicipio = null,
            [FromQuery] string? status = null,
            [FromQuery] string? ordem = "recentes")
        {
            var p = page ?? pagina;
            var s = pageSize ?? tamanho;
            var cat = categoriaId ?? idCategoria;
            var mun = municipioId ?? idMunicipio;

            var resultado = _ocorrenciaUseCase.ObterPaginado(
                p, s, minLat, maxLat, minLon, maxLon, cat, mun, status, ordem);

            var caminho = "/api/v1/ocorrencias";
            var query = $"&status={Uri.EscapeDataString(status ?? "")}&ordem={Uri.EscapeDataString(ordem ?? "recentes")}";

            var itens = resultado.Items.Select(o => new OcorrenciaItemDto
            {
                Ocorrencia = o,
                Links = HateoasLinks.ParaOcorrencia(o.IdOcorrencia)
            }).ToList();

            var linksPagina = HateoasLinks
                .ParaPagina(caminho, resultado.Pagina, resultado.Tamanho, resultado.Total)
                .Select(link => new LinkDto(link.Href + query, link.Rel, link.Method))
                .ToList();

            return Ok(new PageResponseDto<OcorrenciaItemDto>
            {
                Deslocamento = resultado.Deslocamento,
                RegistroRetornado = resultado.RegistroRetornado,
                TotalRegistros = resultado.TotalRegistros,
                Items = itens,
                Links = linksPagina
            });
        }

        [HttpGet("ocorrencias/{id:long}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Buscar ocorrência pública por ID",
            Description = "Retorna os dados públicos da ocorrência sanitizada (sem dados privados, com autor mascarado quando anônimo e votos do ciclo atual)."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Ocorrência encontrada.", typeof(OcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult ObterPorId(long id)
        {
            var ocorrencia = _ocorrenciaUseCase.ObterPorId(id);
            if (ocorrencia == null)
                return NotFound(new { mensagem = "Ocorrência não encontrada." });

            return Ok(ocorrencia.ParaRespostaDetalhada());
        }

        [HttpGet("ocorrencias/{id:long}/historico")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Consultar histórico de transições de status da ocorrência",
            Description = "Retorna a trilha de auditoria e avanço de ciclos da ocorrência sem expor IDs privados."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Histórico retornado com sucesso.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult ObterHistorico(long id)
        {
            try
            {
                var historico = _ocorrenciaUseCase.ObterHistorico(id);
                return Ok(historico);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpPost("ocorrencias")]
        [SwaggerOperation(
            Summary = "Cadastrar nova ocorrência urbana (CIDADAO)",
            Description = "Cria uma nova ocorrência com status ABERTA, ciclo 1 e autor extraído do JWT. Aceita Idempotency-Key no cabeçalho."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Ocorrência criada com sucesso.", typeof(OcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token ausente ou inválido.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Apenas cidadão ativo pode cadastrar.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Conflito de idempotência.")]
        public IActionResult Cadastrar([FromBody] OcorrenciaRequestDto dto)
        {
            var idUsuario = _acesso.ObterIdUsuario(User);
            if (!idUsuario.HasValue)
                return Unauthorized(new { mensagem = "Token JWT sem identificador de cidadão válido." });

            var perfil = _acesso.ObterPerfil(User, idUsuario.Value);
            if (perfil == "ADMIN")
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Administradores não abrem ocorrências no sistema." });

            try
            {
                var criada = _ocorrenciaUseCase.Cadastrar(idUsuario.Value, dto);
                return CreatedAtAction(
                    nameof(ObterPorId),
                    new { id = criada.IdOcorrencia },
                    criada.ParaRespostaDetalhada()
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("ocorrencias/{id:long}")]
        [SwaggerOperation(
            Summary = "Atualizar ocorrência (Autor CIDADAO)",
            Description = "Permite ao autor original atualizar título, descrição, coordenadas e endereço de referência. Nunca permite alterar status, ciclo ou autor."
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Ocorrência atualizada com sucesso.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Sem permissão (somente o autor original pode editar).")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult Atualizar(long id, [FromBody] OcorrenciaRequestDto dto)
        {
            var idUsuario = _acesso.ObterIdUsuario(User);
            if (!idUsuario.HasValue)
                return Unauthorized(new { mensagem = "Token JWT inválido." });

            try
            {
                var atualizado = _ocorrenciaUseCase.Atualizar(id, idUsuario.Value, dto);
                if (!atualizado)
                    return NotFound(new { mensagem = "Ocorrência não encontrada." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("ocorrencias/{id:long}")]
        [SwaggerOperation(
            Summary = "Excluir logicamente ocorrência (Autor CIDADAO)",
            Description = "Marca a ocorrência como excluída (FL_EXCLUIDA = 'S'). Apenas o autor original pode executar a exclusão."
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Ocorrência excluída com sucesso.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Sem permissão (somente o autor original pode excluir).")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult Remover(long id)
        {
            var idUsuario = _acesso.ObterIdUsuario(User);
            if (!idUsuario.HasValue)
                return Unauthorized(new { mensagem = "Token JWT inválido." });

            try
            {
                var removido = _ocorrenciaUseCase.Remover(id, idUsuario.Value);
                if (!removido)
                    return NotFound(new { mensagem = "Ocorrência não encontrada." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message });
            }
        }

        [HttpPost("ocorrencias/{id:long}/midias")]
        [SwaggerOperation(
            Summary = "Anexar foto à ocorrência (Autor CIDADAO)",
            Description = "Registra metadados de foto diretamente na ocorrência (TB_MIDIA). Apenas o autor original pode anexar foto diretamente à ocorrência."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Foto anexada com sucesso.", typeof(MidiaOcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados de mídia inválidos.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Apenas o autor original pode anexar fotos à ocorrência.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult AdicionarMidia(long id, [FromBody] MidiaOcorrenciaRequestDto dto)
        {
            var idUsuario = _acesso.ObterIdUsuario(User);
            if (!idUsuario.HasValue)
                return Unauthorized(new { mensagem = "Token JWT inválido." });

            try
            {
                var midia = _ocorrenciaUseCase.AdicionarMidia(id, idUsuario.Value, dto);
                return CreatedAtAction(nameof(ObterPorId), new { id }, midia);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}
