using BairroVivo.API.Infrastructure.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace BairroVivo.Tests.App;

public class HealthChecksTest
{
    [Fact]
    public async Task ViaCepHealthCheck_SemUrlConfigurada_RetornaDegraded()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ViaCep:Url"] = "" }).Build();
        var health = new ViaCepHealthCheck(config, new Mock<IHttpClientFactory>().Object);

        // Act
        var resultado = await health.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Degraded, resultado.Status);
    }

    [Fact]
    public async Task ViaCepHealthCheck_UrlInvalida_RetornaUnhealthy()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ViaCep:Url"] = "url-invalida" }).Build();
        var health = new ViaCepHealthCheck(config, new Mock<IHttpClientFactory>().Object);

        // Act
        var resultado = await health.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, resultado.Status);
    }
}
