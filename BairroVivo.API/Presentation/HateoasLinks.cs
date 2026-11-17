using BairroVivo.API.Application.Dtos;

namespace BairroVivo.API.Presentation;

public static class HateoasLinks
{
    public static List<LinkDto> ParaOcorrencia(long idOcorrencia) => new()
    {
        new($"/api/v1/ocorrencias/{idOcorrencia}", "self", "GET"),
        new($"/api/v1/ocorrencias/{idOcorrencia}", "update", "PUT"),
        new($"/api/v1/ocorrencias/{idOcorrencia}", "delete", "DELETE"),
        new($"/api/v1/ocorrencias/{idOcorrencia}/atualizacoes", "atualizacoes", "GET"),
        new($"/api/v1/ocorrencias/{idOcorrencia}/atualizacoes", "atualizar_ou_votar", "POST"),
        new($"/api/v1/ocorrencias/{idOcorrencia}/historico", "historico", "GET"),
        new($"/api/v1/ocorrencias/{idOcorrencia}/midias", "adicionar_midia", "POST")
    };

    public static List<LinkDto> ParaRegistro(
        string recurso, long idRegistro, long idOcorrencia, bool permiteExclusao = false)
    {
        var links = new List<LinkDto>
        {
            new($"/api/v1/{recurso}/{idRegistro}", "self", "GET"),
            new($"/api/v1/ocorrencias/{idOcorrencia}", "ocorrencia", "GET"),
            new($"/api/v1/ocorrencias/{idOcorrencia}/{recurso}", "collection", "GET")
        };
        if (permiteExclusao)
            links.Add(new($"/api/v1/{recurso}/{idRegistro}", "delete", "DELETE"));
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
