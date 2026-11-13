using BairroVivo.API.Application.Dtos;

namespace BairroVivo.API.Presentation;

public static class HateoasLinks
{
    public static List<LinkDto> ParaOcorrencia(decimal idOcorrencia) => new()
    {
        new($"/api/ocorrencias/{idOcorrencia}", "self", "GET"),
        new($"/api/ocorrencias/{idOcorrencia}", "update", "PUT"),
        new($"/api/ocorrencias/{idOcorrencia}/status", "status", "PATCH"),
        new($"/api/ocorrencias/{idOcorrencia}", "delete", "DELETE"),
        new($"/api/ocorrencias/{idOcorrencia}/atualizacoes", "atualizacoes", "GET"),
        new($"/api/ocorrencias/{idOcorrencia}/atualizacoes", "confirmar_ou_votar", "POST"),
        new($"/api/ocorrencias/{idOcorrencia}/midias", "adicionar_midia", "POST"),
        new($"/api/ocorrencias/{idOcorrencia}/integracao-oracle/total-votos-ativos", "votos_oracle", "GET")
    };

    public static List<LinkDto> ParaRegistro(
        string recurso, decimal idRegistro, decimal idOcorrencia, bool permiteExclusao = false)
    {
        var links = new List<LinkDto>
        {
            new($"/api/{recurso}/{idRegistro}", "self", "GET"),
            new($"/api/ocorrencias/{idOcorrencia}", "ocorrencia", "GET"),
            new($"/api/ocorrencias/{idOcorrencia}/{recurso}", "collection", "GET")
        };
        if (permiteExclusao)
            links.Add(new($"/api/{recurso}/{idRegistro}", "delete", "DELETE"));
        return links;
    }

    public static List<LinkDto> ParaPagina(string rotaBase, int pagina, int tamanho, int total)
    {
        var ultimaPagina = Math.Max(1, (int)Math.Ceiling(total / (double)Math.Max(1, tamanho)));
        var links = new List<LinkDto>
        {
            new($"{rotaBase}?pagina={pagina}&tamanho={tamanho}", "self", "GET"),
            new($"{rotaBase}?pagina=1&tamanho={tamanho}", "first", "GET"),
            new($"{rotaBase}?pagina={ultimaPagina}&tamanho={tamanho}", "last", "GET")
        };
        if (pagina > 1)
            links.Add(new($"{rotaBase}?pagina={pagina - 1}&tamanho={tamanho}", "prev", "GET"));
        if (pagina < ultimaPagina)
            links.Add(new($"{rotaBase}?pagina={pagina + 1}&tamanho={tamanho}", "next", "GET"));
        return links;
    }
}
