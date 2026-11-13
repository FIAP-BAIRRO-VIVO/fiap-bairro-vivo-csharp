using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.API.Presentation;
using BairroVivo.Application.Dtos.Atualizacoes;
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
    [Route("api")]
    public class AtualizacoesController : ControllerBase
    {
        private readonly IAtualizacaoUseCase _atualizacaoUseCase;
        private readonly IOcorrenciaAccessService _acesso;

        public AtualizacoesController(
            IAtualizacaoUseCase atualizacaoUseCase,
            IOcorrenciaAccessService acesso)
        {
            _atualizacaoUseCase = atualizacaoUseCase;
            _acesso = acesso;
        }

        [HttpGet("ocorrencias/{ocorrenciaId}/atualizacoes")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar atualizações e votos comunitários da ocorrência",
            Description = "Retorna as confirmações ('Ainda está lá'), sinalizações ('Já foi resolvido') e comentários de uma ocorrência com paginação e HATEOAS."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Atualizações retornadas com sucesso.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult ObterPorOcorrencia(
            decimal ocorrenciaId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanho = 10)
        {
            try
            {
                var resultado = _atualizacaoUseCase.ObterPorOcorrenciaPaginado(ocorrenciaId, pagina, tamanho);
                var itens = resultado.Items.ToList();
                foreach (var item in itens)
                {
                    item.Links = HateoasLinks.ParaRegistro("atualizacoes", item.IdAtualizacao, ocorrenciaId);
                }

                var caminho = $"/api/ocorrencias/{ocorrenciaId}/atualizacoes";
                resultado.Items = itens;
                resultado.Links = HateoasLinks.ParaPagina(caminho, resultado.Pagina, resultado.Tamanho, resultado.Total);

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("atualizacoes/{id}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Consultar atualização comunitária por ID",
            Description = "Retorna os detalhes de uma atualização comunitária específica."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Atualização retornada com sucesso.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Atualização não encontrada.")]
        public IActionResult ObterPorId(decimal id)
        {
            var atualizacao = _atualizacaoUseCase.ObterPorId(id);
            if (atualizacao is null)
                return NotFound(new { mensagem = "Atualização não encontrada." });

            atualizacao.Links = HateoasLinks.ParaRegistro("atualizacoes", atualizacao.IdAtualizacao, atualizacao.IdOcorrencia);
            return Ok(atualizacao);
        }

        [HttpPost("ocorrencias/{ocorrenciaId}/atualizacoes")]
        [SwaggerOperation(
            Summary = "Registrar confirmação comunitária ou comentário",
            Description = "Registra CONFIRMACAO ('Ainda está lá!'), JA_RESOLVIDO ('Já foi resolvido!'), COMENTARIO ou RESPOSTA_PREFEITURA. Ao atingir 3 confirmações, valida automaticamente a ocorrência na comunidade."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Atualização registrada com sucesso.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Cidadão já votou nesta modalidade para a ocorrência.")]
        public IActionResult Registrar(
            decimal ocorrenciaId,
            [FromBody] AtualizacaoRequestDto dto)
        {
            try
            {
                var idUsuario = _acesso.ObterIdUsuario(User);
                if (!idUsuario.HasValue)
                    return Unauthorized(new { mensagem = "JWT sem userId válido." });

                var criada = _atualizacaoUseCase.Registrar(ocorrenciaId, idUsuario.Value, dto);
                criada.Links = HateoasLinks.ParaRegistro("atualizacoes", criada.IdAtualizacao, ocorrenciaId);

                return CreatedAtAction(
                    nameof(ObterPorId),
                    new { id = criada.IdAtualizacao },
                    criada
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
        }
    }
}
