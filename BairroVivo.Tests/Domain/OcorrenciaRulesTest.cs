using BairroVivo.Domain.Rules;

namespace BairroVivo.Tests.Domain;

public class OcorrenciaRulesTest
{
    [Theory]
    [InlineData("aberta", "ABERTA")]
    [InlineData(" resolvida ", "RESOLVIDA")]
    public void ValidarStatus_ValorPermitido_Normaliza(string entrada, string esperado)
    {
        // Act
        var resultado = OcorrenciaRules.ValidarStatus(entrada);
        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("EM_ANALISE")]
    [InlineData("EM_ANDAMENTO")]
    [InlineData("ARQUIVADA")]
    [InlineData("CANCELADA")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidarStatus_ValorNaoPermitidoNoV12_LancaArgumentException(string? entrada)
    {
        // Act
        var erro = Record.Exception(() => OcorrenciaRules.ValidarStatus(entrada));
        // Assert
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void ValidarCoordenadas_LatitudeForaDoIntervalo_LancaArgumentException()
    {
        const decimal latitude = -95.0m;
        const decimal longitude = -46.6333m;
        var erro = Record.Exception(() => OcorrenciaRules.ValidarCoordenadas(latitude, longitude));
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void ValidarCoordenadas_LongitudeForaDoIntervalo_LancaArgumentException()
    {
        const decimal latitude = -23.55m;
        const decimal longitude = 185.0m;
        var erro = Record.Exception(() => OcorrenciaRules.ValidarCoordenadas(latitude, longitude));
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void ValidarCoordenadas_CoordenadasValidas_NaoLancaExcecao()
    {
        const decimal latitude = -23.5614m;
        const decimal longitude = -46.6558m;
        var erro = Record.Exception(() => OcorrenciaRules.ValidarCoordenadas(latitude, longitude));
        Assert.Null(erro);
    }

    [Fact]
    public void ValidarTitulo_MuitoCurto_LancaArgumentException()
    {
        const string titulo = "Rua";
        var erro = Record.Exception(() => OcorrenciaRules.ValidarTitulo(titulo));
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void ValidarTitulo_Valido_RemoveEspacos()
    {
        const string titulo = "  Buraco na Avenida Paulista  ";
        var resultado = OcorrenciaRules.ValidarTitulo(titulo);
        Assert.Equal("Buraco na Avenida Paulista", resultado);
    }

    [Fact]
    public void ValidarDescricao_MinimoDezCaracteres()
    {
        const string descCurta = "Curto";
        var erro = Record.Exception(() => OcorrenciaRules.ValidarDescricao(descCurta));
        Assert.IsType<ArgumentException>(erro);

        const string descValida = "Descrição detalhada do problema na via.";
        var resultado = OcorrenciaRules.ValidarDescricao(descValida);
        Assert.Equal(descValida, resultado);
    }

    [Fact]
    public void ValidarEnderecoReferencia_ValidaLimiteMaximo()
    {
        var textoLongo = new string('A', 256);
        var erro = Record.Exception(() => OcorrenciaRules.ValidarEnderecoReferencia(textoLongo));
        Assert.IsType<ArgumentException>(erro);

        var textoValido = "Próximo à estação do metrô";
        var resultado = OcorrenciaRules.ValidarEnderecoReferencia(textoValido);
        Assert.Equal(textoValido, resultado);
    }

    [Theory]
    [InlineData("COMENTARIO", "COMENTARIO")]
    [InlineData("PERSISTE", "PERSISTE")]
    [InlineData("RESOLVIDO", "RESOLVIDO")]
    public void AtualizacaoRules_TiposValidosV12_Normaliza(string entrada, string esperado)
    {
        var resultado = AtualizacaoRules.ValidarTipo(entrada);
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("CONFIRMACAO")]
    [InlineData("JA_RESOLVIDO")]
    [InlineData("OUTRO")]
    public void AtualizacaoRules_TiposObsoletos_LancaArgumentException(string entrada)
    {
        var erro = Record.Exception(() => AtualizacaoRules.ValidarTipo(entrada));
        Assert.IsType<ArgumentException>(erro);
    }

    [Fact]
    public void AtualizacaoRules_RegraDoisVotos_TransicionaStatusEElevaCiclo()
    {
        // 1 voto em ABERTA com RESOLVIDO não transiciona ainda
        var (transiciona1, status1) = AtualizacaoRules.AvaliarTransicaoCiclo("ABERTA", "RESOLVIDO", 1);
        Assert.False(transiciona1);
        Assert.Null(status1);

        // 2 votos em ABERTA com RESOLVIDO transiciona para RESOLVIDA
        var (transiciona2, status2) = AtualizacaoRules.AvaliarTransicaoCiclo("ABERTA", "RESOLVIDO", 2);
        Assert.True(transiciona2);
        Assert.Equal("RESOLVIDA", status2);

        // 2 votos em RESOLVIDA com PERSISTE transiciona de volta para ABERTA
        var (transiciona3, status3) = AtualizacaoRules.AvaliarTransicaoCiclo("RESOLVIDA", "PERSISTE", 2);
        Assert.True(transiciona3);
        Assert.Equal("ABERTA", status3);
    }
}
