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
    [HttpPost("api/ocorrencias/{ocorrenciaId:long}/integracao-oracle/registrar-atualizacao")]
    public async Task<IActionResult> ExecutarProcedureAtualizacao(
        long ocorrenciaId,
        [FromBody] AtualizacaoRequestDto dto)
    {
        var idUsuario = _acesso.ObterIdUsuario(User);
        if (!idUsuario.HasValue)
            return Unauthorized(new { mensagem = "JWT sem userId válido." });

        var idAtualizacao = await _integracao.ExecutarRegistrarAtualizacaoAsync(ocorrenciaId, idUsuario.Value, dto);
        return Ok(new
        {
            mensagem = "PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO executada com sucesso no Oracle.",
            ocorrenciaId = ocorrenciaId.ToString(),
            atualizacaoId = idAtualizacao.ToString(),
            idUsuario = idUsuario.Value.ToString(),
            tipo = dto.Tipo
        });
    }

    // Invoca a function PL/SQL PKG_COMUNIDADE.FN_TOTAL_VOTOS_ATIVOS no lado Oracle.
    [HttpGet("api/ocorrencias/{ocorrenciaId:long}/integracao-oracle/total-votos-ativos")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterTotalVotosAtivos(
        long ocorrenciaId,
        [FromQuery] string tipoVoto = "RESOLVIDO")
    {
        var totalVotos = await _integracao.ContarVotosAtivosAsync(ocorrenciaId, tipoVoto);
        return Ok(new
        {
            ocorrenciaId = ocorrenciaId.ToString(),
            tipoVoto,
            totalVotosAtivos = totalVotos
        });
    }
}
