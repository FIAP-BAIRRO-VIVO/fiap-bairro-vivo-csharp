using System.Text.Json.Serialization;

namespace BairroVivo.API.Application.Dtos;

// Resposta paginada com metadados de navegação e HATEOAS
public class PageResponseDto<T>
{
    [JsonIgnore]
    public int Deslocamento { get; set; }
    [JsonIgnore]
    public int RegistroRetornado { get; set; }
    [JsonIgnore]
    public int TotalRegistros { get; set; }

    [JsonPropertyName("pagina")]
    public int Pagina => RegistroRetornado > 0 ? (Deslocamento / RegistroRetornado) + 1 : 1;
    [JsonPropertyName("tamanho")]
    public int Tamanho => RegistroRetornado;
    [JsonPropertyName("total")]
    public int Total => TotalRegistros;
    [JsonPropertyName("dados")]
    public IEnumerable<T> Items { get; set; } = new List<T>();
    [JsonPropertyName("links")]
    public List<LinkDto> Links { get; set; } = new();
}
