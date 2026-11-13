namespace BairroVivo.Domain.Rules;

// Regras de dominio da votacao e atualizacao comunitaria (Ainda esta la / Ja foi resolvido).
public static class AtualizacaoRules
{
    private static readonly string[] TiposValidos =
    {
        "CONFIRMACAO",
        "JA_RESOLVIDO",
        "COMENTARIO",
        "RESPOSTA_PREFEITURA"
    };

    public static string ValidarTipo(string? tipo)
    {
        var normalizado = tipo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TiposValidos.Contains(normalizado))
            throw new ArgumentException("Tipo de atualização inválido. Utilize CONFIRMACAO, JA_RESOLVIDO, COMENTARIO ou RESPOSTA_PREFEITURA.");
        return normalizado;
    }

    public static (string ValidadaComunidade, string UrgenciaAtualizada) AvaliarValidacaoComunitaria(
        int confirmacoesAtuais,
        string urgenciaAtual)
    {
        if (confirmacoesAtuais >= 3)
        {
            var novaUrgencia = urgenciaAtual == "BAIXA" || urgenciaAtual == "MEDIA"
                ? "ALTA"
                : urgenciaAtual;
            return ("S", novaUrgencia);
        }

        return ("N", urgenciaAtual);
    }
}
