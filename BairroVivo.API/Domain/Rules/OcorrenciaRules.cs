namespace BairroVivo.Domain.Rules;

// Regras de dominio puras, sem dependencia de banco ou HTTP, facilitando xUnit.
public static class OcorrenciaRules
{
    private static readonly string[] StatusValidos =
    {
        "ABERTA",
        "EM_ANALISE",
        "EM_ANDAMENTO",
        "RESOLVIDA",
        "ARQUIVADA"
    };

    private static readonly string[] UrgenciasValidas =
    {
        "BAIXA",
        "MEDIA",
        "ALTA",
        "CRITICA"
    };

    public static string ValidarTitulo(string? titulo)
    {
        var normalizado = titulo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizado) || normalizado.Length < 5)
            throw new ArgumentException("O título da ocorrência deve conter pelo menos 5 caracteres.");
        if (normalizado.Length > 140)
            throw new ArgumentException("O título da ocorrência deve conter no máximo 140 caracteres.");
        return normalizado;
    }

    public static string ValidarDescricao(string? descricao)
    {
        var normalizado = descricao?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizado) || normalizado.Length < 10)
            throw new ArgumentException("A descrição da ocorrência deve conter pelo menos 10 caracteres.");
        return normalizado;
    }

    public static string ValidarBairro(string? bairro)
    {
        var normalizado = bairro?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizado))
            throw new ArgumentException("O bairro da ocorrência é obrigatório.");
        return normalizado;
    }

    public static string ValidarStatus(string? status)
    {
        var normalizado = status?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!StatusValidos.Contains(normalizado))
            throw new ArgumentException("Status inválido. Utilize ABERTA, EM_ANALISE, EM_ANDAMENTO, RESOLVIDA ou ARQUIVADA.");
        return normalizado;
    }

    public static string ValidarUrgencia(string? urgencia)
    {
        if (string.IsNullOrWhiteSpace(urgencia))
            return "MEDIA";

        var normalizado = urgencia.Trim().ToUpperInvariant();
        if (!UrgenciasValidas.Contains(normalizado))
            throw new ArgumentException("Urgência inválida. Utilize BAIXA, MEDIA, ALTA ou CRITICA.");
        return normalizado;
    }

    public static void ValidarCoordenadas(decimal latitude, decimal longitude)
    {
        if (latitude < -90m || latitude > 90m)
            throw new ArgumentException("Latitude inválida. O valor deve estar entre -90 e 90.");
        if (longitude < -180m || longitude > 180m)
            throw new ArgumentException("Longitude inválida. O valor deve estar entre -180 e 180.");
    }

    public static string GerarProtocolo(DateTime dataReferencia, decimal sequencial)
    {
        return $"BV-{dataReferencia:yyyy}-{sequencial:000000}";
    }
}
