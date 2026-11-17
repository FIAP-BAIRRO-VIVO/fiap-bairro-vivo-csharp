namespace BairroVivo.Domain.Rules;

// Regras de domínio da votação e atualização comunitária conforme PKG_COMUNIDADE no Oracle v1.2
public static class AtualizacaoRules
{
    private static readonly string[] TiposValidos =
    {
        "COMENTARIO",
        "PERSISTE",
        "RESOLVIDO"
    };

    public static string ValidarTipo(string? tipo)
    {
        var normalizado = tipo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TiposValidos.Contains(normalizado))
            throw new ArgumentException("Tipo de atualização inválido. Utilize COMENTARIO, PERSISTE ou RESOLVIDO.");
        return normalizado;
    }

    public static bool ContaComoVoto(string statusAtual, string tipoAtualizacao)
    {
        if (statusAtual == "ABERTA" && tipoAtualizacao == "RESOLVIDO")
            return true;
        if (statusAtual == "RESOLVIDA" && tipoAtualizacao == "PERSISTE")
            return true;
        return false;
    }

    public static (bool Transiciona, string? NovoStatus) AvaliarTransicaoCiclo(
        string statusAtual,
        string tipoAtualizacao,
        int totalVotosNoCicloAposEste)
    {
        if (!ContaComoVoto(statusAtual, tipoAtualizacao))
            return (false, null);

        // Regra dos 2 votos da Global Solution / Oracle v1.2:
        // 2 votos válidos de usuários distintos encerram ou reabrem a ocorrência
        if (totalVotosNoCicloAposEste >= 2)
        {
            var novoStatus = statusAtual == "ABERTA" ? "RESOLVIDA" : "ABERTA";
            return (true, novoStatus);
        }

        return (false, null);
    }
}
