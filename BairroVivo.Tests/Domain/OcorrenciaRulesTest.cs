using BairroVivo.Domain.Rules;

namespace BairroVivo.Tests.Domain;

public class OcorrenciaRulesTest
{
    [Theory]
    [InlineData("aberta", "ABERTA")]
    [InlineData(" resolvida ", "RESOLVIDA")]
    [InlineData("em_andamento", "EM_ANDAMENTO")]
    public void ValidarStatus_ValorPermitido_Normaliza(string entrada, string esperado)
    {
        // Arrange
        var status = entrada;
        // Act
        var resultado = OcorrenciaRules.ValidarStatus(status);
        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("CANCELADA")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidarStatus_ValorInvalido_LancaArgumentException(string? entrada)
    {
        // Arrange
        var status = entrada;
        // Act
        var erro = Record.Exception(() => OcorrenciaRules.ValidarStatus(status));
        // Assert
        Assert.IsType<ArgumentException>(erro);
    }

    [Theory]
    [InlineData("alta", "ALTA")]
    [InlineData(" CRITICA ", "CRITICA")]
    [InlineData(null, "MEDIA")]
    public void ValidarUrgencia_ValorPermitido_Normaliza(string? entrada, string esperado)
    {
        // Arrange
        var urgencia = entrada;
        // Act
        var resultado = OcorrenciaRules.ValidarUrgencia(urgencia);
        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void ValidarUrgencia_ValorInvalido_LancaArgumentException()
    {
        // Arrange
        const string urgencia = "URGENTISSIMA";
        // Act
        var erro = Record.Exception(() => OcorrenciaRules.ValidarUrgencia(urgencia));
        // Assert
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void ValidarCoordenadas_LatitudeForaDoIntervalo_LancaArgumentException()
    {
        // Arrange
        const decimal latitude = -95.0m;
        const decimal longitude = -46.6333m;
        // Act
        var erro = Record.Exception(() => OcorrenciaRules.ValidarCoordenadas(latitude, longitude));
        // Assert
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void ValidarCoordenadas_CoordenadasValidas_NaoLancaExcecao()
    {
        // Arrange
        const decimal latitude = -23.5614m;
        const decimal longitude = -46.6558m;
        // Act
        var erro = Record.Exception(() => OcorrenciaRules.ValidarCoordenadas(latitude, longitude));
        // Assert
        Assert.Null(erro);
    }

    [Fact]
    public void ValidarTitulo_MuitoCurto_LancaArgumentException()
    {
        // Arrange
        const string titulo = "Rua";
        // Act
        var erro = Record.Exception(() => OcorrenciaRules.ValidarTitulo(titulo));
        // Assert
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void ValidarTitulo_Valido_RemoveEspacos()
    {
        // Arrange
        const string titulo = "  Buraco na Avenida Paulista  ";
        // Act
        var resultado = OcorrenciaRules.ValidarTitulo(titulo);
        // Assert
        Assert.Equal("Buraco na Avenida Paulista", resultado);
    }

    [Fact]
    public void AvaliarValidacaoComunitaria_TresConfirmacoes_ValidaEElevaUrgencia()
    {
        // Arrange & Act
        var (validada, novaUrgencia) = AtualizacaoRules.AvaliarValidacaoComunitaria(3, "MEDIA");
        // Assert
        Assert.Equal("S", validada);
        Assert.Equal("ALTA", novaUrgencia);
    }
}
