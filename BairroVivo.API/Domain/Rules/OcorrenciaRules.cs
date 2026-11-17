namespace BairroVivo.Domain.Rules;

// Regras de domínio puras para Ocorrência no Oracle v1.2
public static class OcorrenciaRules
{
    private static readonly string[] StatusValidos =
    {
        "ABERTA",
        "RESOLVIDA"
    };

    public static string ValidarTitulo(string? titulo)
    {
        var normalizado = titulo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizado) || normalizado.Length < 5)
            throw new ArgumentException("O título da ocorrência deve conter pelo menos 5 caracteres.");
        if (normalizado.Length > 150)
            throw new ArgumentException("O título da ocorrência deve conter no máximo 150 caracteres.");
        return normalizado;
    }

    public static string ValidarDescricao(string? descricao)
    {
        var normalizado = descricao?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizado) || normalizado.Length < 10)
            throw new ArgumentException("A descrição da ocorrência deve conter pelo menos 10 caracteres.");
        return normalizado;
    }

    public static string? ValidarEnderecoReferencia(string? enderecoReferencia)
    {
        if (string.IsNullOrWhiteSpace(enderecoReferencia))
            return null;

        var normalizado = enderecoReferencia.Trim();
        if (normalizado.Length > 255)
            throw new ArgumentException("O endereço de referência deve conter no máximo 255 caracteres.");
        return normalizado;
    }

    public static string ValidarStatus(string? status)
    {
        var normalizado = status?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!StatusValidos.Contains(normalizado))
            throw new ArgumentException("Status inválido. Utilize ABERTA ou RESOLVIDA.");
        return normalizado;
    }

    public static string ValidarAnonimo(bool? anonimo)
    {
        return anonimo == true ? "S" : "N";
    }

    public static string ValidarAnonimo(string? flAnonimo)
    {
        var normalizado = flAnonimo?.Trim().ToUpperInvariant() ?? "N";
        if (normalizado != "S" && normalizado != "N")
            throw new ArgumentException("Flag de anonimato inválida. Utilize 'S' ou 'N'.");
        return normalizado;
    }

    public static void ValidarCoordenadas(decimal latitude, decimal longitude)
    {
        if (latitude < -90m || latitude > 90m)
            throw new ArgumentException("Latitude inválida. O valor deve estar entre -90 e 90.");
        if (longitude < -180m || longitude > 180m)
            throw new ArgumentException("Longitude inválida. O valor deve estar entre -180 e 180.");
    }
}
