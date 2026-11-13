using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Application.Dtos.Ocorrencias;
using BairroVivo.API.Application.Interfaces;
using BairroVivo.API.Presentation;
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
            Summary = "Listar ocorrências urbanas paginadas",
            Description = "Retorna as ocorrências com filtros por status, urgência, bairro, categoria, município, paginação e links HATEOAS."
        )]
        [SwaggerResponse(
            StatusCodes.Status200OK,
            "Lista paginada de ocorrências retornada com sucesso.",
            typeof(PageResponseDto<OcorrenciaItemDto>)
        )]
        public IActionResult ObterPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanho = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? urgencia = null,
            [FromQuery] string? bairro = null,
            [FromQuery] decimal? idCategoria = null,
            [FromQuery] decimal? idMunicipio = null,
            [FromQuery] string? ordem = "recentes")
        {
            var resultado = _ocorrenciaUseCase.ObterPaginado(
                pagina, tamanho, status, urgencia, bairro, idCategoria, idMunicipio, ordem);

            var caminho = "/api/ocorrencias";
            var query = $"&status={Uri.EscapeDataString(status ?? "")}" +
                        $"&urgencia={Uri.EscapeDataString(urgencia ?? "")}" +
                        $"&bairro={Uri.EscapeDataString(bairro ?? "")}" +
                        $"&ordem={Uri.EscapeDataString(ordem ?? "recentes")}";

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

        [HttpGet("usuarios/{idUsuario}/ocorrencias")]
        [SwaggerOperation(
            Summary = "Listar ocorrências por cidadão",
            Description = "Retorna as ocorrências registradas por um cidadão com paginação, filtros e links HATEOAS."
        )]
        [SwaggerResponse(
            StatusCodes.Status200OK,
            "Lista de ocorrências do cidadão retornada com sucesso.",
            typeof(PageResponseDto<OcorrenciaItemDto>)
        )]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Usuário não encontrado.")]
        public IActionResult ObterPorUsuario(
            decimal idUsuario,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanho = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? ordem = "recentes")
        {
            try
            {
                var resultado = _ocorrenciaUseCase
                    .ObterPorUsuarioPaginado(idUsuario, pagina, tamanho, status, ordem);
                var caminho = $"/api/usuarios/{idUsuario}/ocorrencias";
                var query = $"&status={Uri.EscapeDataString(status ?? "")}" +
                            $"&ordem={Uri.EscapeDataString(ordem ?? "recentes")}";

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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("ocorrencias/{id}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Buscar ocorrência por ID",
            Description = "Retorna os dados completos de uma ocorrência urbana específica e seus links HATEOAS."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Ocorrência encontrada com sucesso.", typeof(OcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult ObterPorId(decimal id)
        {
            var ocorrencia = _ocorrenciaUseCase.ObterPorId(id);

            if (ocorrencia == null)
                return NotFound(new { mensagem = "Ocorrência não encontrada." });

            return Ok(new
            {
                ocorrencia.IdOcorrencia,
                ocorrencia.Protocolo,
                ocorrencia.IdUsuario,
                ocorrencia.NomeUsuario,
                ocorrencia.IdCategoria,
                ocorrencia.Categoria,
                ocorrencia.IdMunicipio,
                ocorrencia.Municipio,
                ocorrencia.Uf,
                ocorrencia.Titulo,
                ocorrencia.Descricao,
                ocorrencia.Status,
                ocorrencia.Urgencia,
                ocorrencia.Latitude,
                ocorrencia.Longitude,
                ocorrencia.Logradouro,
                ocorrencia.Bairro,
                ocorrencia.Cep,
                ocorrencia.Confirmacoes,
                ocorrencia.ValidadaComunidade,
                ocorrencia.CategoriaSugeridaIa,
                ocorrencia.ConfiancaIa,
                ocorrencia.CriadaEm,
                ocorrencia.AtualizadaEm,
                ocorrencia.ResolvidaEm,
                ocorrencia.SlaLimite,
                ocorrencia.Midias,
                Links = HateoasLinks.ParaOcorrencia(ocorrencia.IdOcorrencia)
            });
        }

        [HttpPost("ocorrencias")]
        [SwaggerOperation(
            Summary = "Cadastrar nova ocorrência urbana",
            Description = "Registra uma ocorrência associada ao cidadão autenticado pelo token JWT."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Ocorrência cadastrada com sucesso.", typeof(OcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        public IActionResult CadastrarPropria([FromBody] OcorrenciaRequestDto dto)
        {
            var idUsuario = _acesso.ObterIdUsuario(User);
            if (!idUsuario.HasValue)
                return Unauthorized(new { mensagem = "JWT sem userId válido." });

            return ExecutarCadastro(idUsuario.Value, dto);
        }

        [HttpPost("usuarios/{idUsuario}/ocorrencias")]
        [SwaggerOperation(
            Summary = "Cadastrar ocorrência para um cidadão",
            Description = "Cria uma nova ocorrência urbana vinculada ao cidadão informado."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Ocorrência cadastrada com sucesso.", typeof(OcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Usuário não encontrado.")]
        public IActionResult Cadastrar(decimal idUsuario, [FromBody] OcorrenciaRequestDto dto)
        {
            return ExecutarCadastro(idUsuario, dto);
        }

        private IActionResult ExecutarCadastro(decimal idUsuario, OcorrenciaRequestDto dto)
        {
            try
            {
                var ocorrencia = _ocorrenciaUseCase.Cadastrar(idUsuario, dto);
                return CreatedAtAction(
                    nameof(ObterPorId),
                    new { id = ocorrencia.IdOcorrencia },
                    new
                    {
                        ocorrencia.IdOcorrencia,
                        ocorrencia.Protocolo,
                        ocorrencia.IdUsuario,
                        ocorrencia.NomeUsuario,
                        ocorrencia.IdCategoria,
                        ocorrencia.Categoria,
                        ocorrencia.IdMunicipio,
                        ocorrencia.Municipio,
                        ocorrencia.Uf,
                        ocorrencia.Titulo,
                        ocorrencia.Descricao,
                        ocorrencia.Status,
                        ocorrencia.Urgencia,
                        ocorrencia.Latitude,
                        ocorrencia.Longitude,
                        ocorrencia.Logradouro,
                        ocorrencia.Bairro,
                        ocorrencia.Cep,
                        ocorrencia.Confirmacoes,
                        ocorrencia.ValidadaComunidade,
                        ocorrencia.CategoriaSugeridaIa,
                        ocorrencia.ConfiancaIa,
                        ocorrencia.CriadaEm,
                        ocorrencia.SlaLimite,
                        ocorrencia.Midias,
                        Links = HateoasLinks.ParaOcorrencia(ocorrencia.IdOcorrencia)
                    }
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

        [HttpPut("ocorrencias/{id}")]
        [SwaggerOperation(
            Summary = "Atualizar dados da ocorrência",
            Description = "Atualiza as informações cadastrais de uma ocorrência existente."
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Ocorrência atualizada com sucesso.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult Atualizar(decimal id, [FromBody] OcorrenciaRequestDto dto)
        {
            try
            {
                var atualizado = _ocorrenciaUseCase.Atualizar(id, dto);
                if (!atualizado)
                    return NotFound(new { mensagem = "Ocorrência não encontrada." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPatch("ocorrencias/{id}/status")]
        [SwaggerOperation(
            Summary = "Atualizar status da ocorrência",
            Description = "Altera o status da ocorrência (ABERTA, EM_ANALISE, EM_ANDAMENTO, RESOLVIDA ou ARQUIVADA) e registra histórico."
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Status atualizado com sucesso.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Status inválido.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult AtualizarStatus(decimal id, [FromBody] OcorrenciaStatusRequestDto dto)
        {
            try
            {
                var idUsuario = _acesso.ObterIdUsuario(User);
                var atualizado = _ocorrenciaUseCase.AtualizarStatus(id, idUsuario, dto);
                if (!atualizado)
                    return NotFound(new { mensagem = "Ocorrência não encontrada." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("ocorrencias/{id}")]
        [SwaggerOperation(
            Summary = "Remover ocorrência",
            Description = "Exclui uma ocorrência existente."
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Ocorrência removida com sucesso.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult Remover(decimal id)
        {
            var removido = _ocorrenciaUseCase.Remover(id);
            if (!removido)
                return NotFound(new { mensagem = "Ocorrência não encontrada." });

            return NoContent();
        }

        [HttpPost("ocorrencias/{id}/midias")]
        [SwaggerOperation(
            Summary = "Anexar foto ou vídeo à ocorrência",
            Description = "Adiciona uma mídia (FOTO ou VIDEO) em TB_MIDIA_OCORRENCIA."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Mídia anexada com sucesso.", typeof(MidiaOcorrenciaResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ocorrência não encontrada.")]
        public IActionResult AdicionarMidia(decimal id, [FromBody] MidiaOcorrenciaRequestDto dto)
        {
            try
            {
                var midia = _ocorrenciaUseCase.AdicionarMidia(id, dto);
                return CreatedAtAction(nameof(ObterPorId), new { id }, midia);
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
    }
}
