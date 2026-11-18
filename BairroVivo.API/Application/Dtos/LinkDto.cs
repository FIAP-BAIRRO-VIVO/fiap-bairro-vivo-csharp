namespace BairroVivo.API.Application.Dtos;

// Representação de link HATEOAS padronizado (RFC 8288 / RESTful API)
public class LinkDto
{
    public string Href { get; set; } = string.Empty;
    public string Rel { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";

    public LinkDto(string href, string rel, string method = "GET")
    { Href = href; Rel = rel; Method = method; }
}
