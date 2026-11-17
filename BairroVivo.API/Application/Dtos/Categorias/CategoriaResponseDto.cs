using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Categorias
{
    public class CategoriaResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("idCategoria")]
        public long IdCategoria
        {
            get => long.TryParse(Id, out var val) ? val : 0;
            set => Id = value.ToString();
        }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string? Descricao { get; set; }

        [JsonPropertyName("icone")]
        public string? Icone { get; set; }

        [JsonPropertyName("ativa")]
        public bool Ativa { get; set; } = true;
    }
}
