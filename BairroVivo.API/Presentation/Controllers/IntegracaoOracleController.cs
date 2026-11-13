using BairroVivo.API.Application.Interfaces;
using BairroVivo.Application.Dtos.Atualizacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BairroVivo.API.Presentation.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class IntegracaoOracleController : ControllerBase
{
    private readonly IIntegracaoOracleUseCase _integracao;
    private readonly IOcorrenciaAccessService _acesso;

    public IntegracaoOracleController(
        IIntegracaoOracleUseCase integracao,
        IOcorrenciaAccessService acesso)
    {
        _integracao = integracao;
        _acesso = acesso;
    }

    // Executa a procedure PL/SQL PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO no lado Oracle.
    [HttpPost("api/ocorrencias/{ocorrenciaId}/integracao-oracle/registrar-atualizacao")]
    public async Task<IActionResult> ExecutarProcedureAtualizacao(
        decimal ocorrenciaId,
        [FromBody] AtualizacaoRequestDto dto)
    {
        var idUsuario = _acesso.ObterIdUsuario(User);
        if (!idUsuario.HasValue)
            return Unauthorized(new { mensagem = "JWT sem userId valido." });

        await _integracao.ExecutarRegistrarAtualizacaoAsync(ocorrenciaId, idUsuario.Value, dto);
        return Ok(new
        {
            mensagem = "PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO executada com sucesso no Oracle.",
            ocorrenciaId,
            idUsuario = idUsuario.Value,
            tipo = dto.Tipo
        });
    }

    // Invoca a function PL/SQL PKG_COMUNIDADE.FN_TOTAL_VOTOS_ATIVOS no lado Oracle.
    [HttpGet("api/ocorrencias/{ocorrenciaId}/integracao-oracle/total-votos-ativos")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterTotalVotosAtivos(decimal ocorrenciaId)
    {
        var totalVotos = await _integracao.ContarVotosAtivosAsync(ocorrenciaId);
        return Ok(new { ocorrenciaId, totalVotosAtivos = totalVotos });
    }
}
