using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.API.Presentation;
using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Application.Dtos.Ocorrencias;
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

        [HttpGet("ocorrencias/{ocorrenciaId:long}/atualizacoes")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar atualizações e votos comunitários da ocorrência",
            Description = "Retorna o histórico público de relatos, persistências e resoluções com autor mascarado quando anônimo e links HATEOAS."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Atualizações retornadas com sucesso.", typeof(PageResponseDto<AtualizacaoResponseDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult ObterPorOcorrencia(
            long ocorrenciaId,
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

        [HttpGet("atualizacoes/{id:long}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Consultar atualização comunitária por ID",
            Description = "Retorna os detalhes públicos de uma atualização comunitária específica."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Atualização retornada com sucesso.", typeof(AtualizacaoResponseDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Atualização não encontrada.")]
        public IActionResult ObterPorId(long id)
        {
            var atualizacao = _atualizacaoUseCase.ObterPorId(id);
            if (atualizacao is null)
                return NotFound(new { mensagem = "Atualização não encontrada." });

            if (long.TryParse(atualizacao.IdOcorrencia, out var idOcorrencia))
                atualizacao.Links = HateoasLinks.ParaRegistro("atualizacoes", atualizacao.IdAtualizacao, idOcorrencia);

            return Ok(atualizacao);
        }

        [HttpPost("ocorrencias/{ocorrenciaId:long}/atualizacoes")]
        [SwaggerOperation(
            Summary = "Registrar atualização / votação comunitária (CIDADAO)",
            Description = "Executa a procedure PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO no Oracle com tipos COMENTARIO, PERSISTE ou RESOLVIDO. Regra de 2 votos transiciona o status da ocorrência."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Atualização registrada com sucesso.", typeof(AtualizacaoResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token ausente ou inválido.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Somente cidadão ativo pode votar.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Usuário já votou no ciclo atual da ocorrência.")]
        public async Task<IActionResult> Registrar(
            long ocorrenciaId,
            [FromBody] AtualizacaoRequestDto dto)
        {
            var idUsuario = _acesso.ObterIdUsuario(User);
            if (!idUsuario.HasValue)
                return Unauthorized(new { mensagem = "Token JWT sem identificador de cidadão válido." });

            var perfil = _acesso.ObterPerfil(User, idUsuario.Value);
            if (perfil == "ADMIN")
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Administradores não registram atualizações ou votos comunitários." });

            try
            {
                var criada = await _atualizacaoUseCase.RegistrarAsync(ocorrenciaId, idUsuario.Value, dto);
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

        [HttpPost("ocorrencias/{ocorrenciaId:long}/atualizacoes/{atualizacaoId:long}/midias")]
        [SwaggerOperation(
            Summary = "Anexar foto à atualização comunitária (Autor da atualização)",
            Description = "Registra metadados de foto vinculada a uma atualização (TB_MIDIA). Apenas o autor da atualização pode anexar foto a ela."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Foto anexada com sucesso.", typeof(MidiaOcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados de mídia inválidos.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Apenas o autor da atualização pode anexar fotos a ela.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência ou atualização não encontrada.")]
        public IActionResult AdicionarMidiaAtualizacao(
            long ocorrenciaId,
            long atualizacaoId,
            [FromBody] MidiaOcorrenciaRequestDto dto)
        {
            var idUsuario = _acesso.ObterIdUsuario(User);
            if (!idUsuario.HasValue)
                return Unauthorized(new { mensagem = "Token JWT sem identificador de usuário válido." });

            try
            {
                var midia = _atualizacaoUseCase.AdicionarMidia(ocorrenciaId, atualizacaoId, idUsuario.Value, dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = atualizacaoId }, midia);
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
