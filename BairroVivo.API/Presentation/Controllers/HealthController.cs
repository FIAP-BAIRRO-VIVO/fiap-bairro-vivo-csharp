using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BairroVivo.API.Presentation.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthChecks;

    public HealthController(HealthCheckService healthChecks)
    {
        _healthChecks = healthChecks;
    }

    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "Healthy" });

    [HttpGet("db")]
    public Task<IActionResult> Database(CancellationToken ct) =>
        Executar(r => r.Tags.Contains("db"), ct);

    [HttpGet("ready")]
    public Task<IActionResult> Ready(CancellationToken ct) =>
        Executar(r => r.Tags.Contains("ready"), ct);

    private async Task<IActionResult> Executar(Func<HealthCheckRegistration, bool> filtro, CancellationToken ct)
    {
        var relatorio = await _healthChecks.CheckHealthAsync(filtro, ct);
        var codigo = relatorio.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return StatusCode(codigo, new
        {
            status = relatorio.Status.ToString(),
            checks = relatorio.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
    }
}
