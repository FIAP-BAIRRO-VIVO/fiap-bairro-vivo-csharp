using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BairroVivo.API.Infrastructure.Health;

// Checa a disponibilidade HTTP do servico externo de CEP/Geolocalizacao (ViaCEP).
public class ViaCepHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ViaCepHealthCheck(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var url = _configuration["ViaCep:Url"];
        if (string.IsNullOrWhiteSpace(url))
            return HealthCheckResult.Degraded("Integracao ViaCEP nao configurada.");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return HealthCheckResult.Unhealthy("URL ViaCEP invalida.");
        }

        try
        {
            var cliente = _httpClientFactory.CreateClient();
            cliente.Timeout = TimeSpan.FromSeconds(5);
            using var resposta = await cliente.GetAsync(
                new Uri(uri.GetLeftPart(UriPartial.Authority)), cancellationToken);

            return (int)resposta.StatusCode < 500
                ? HealthCheckResult.Healthy("Servico ViaCEP acessivel por HTTP.")
                : HealthCheckResult.Unhealthy("Servico ViaCEP retornou erro 5xx.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Servico ViaCEP indisponivel no momento.", ex);
        }
    }
}
