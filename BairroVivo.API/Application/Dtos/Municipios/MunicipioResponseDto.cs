using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Municipios
{
    public class MunicipioResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("idMunicipio")]
        public long IdMunicipio
        {
            get => long.TryParse(Id, out var val) ? val : 0;
            set => Id = value.ToString();
        }

        [JsonPropertyName("codigoIbge")]
        public int? CodigoIbge { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("uf")]
        public string Uf { get; set; } = string.Empty;
    }
}
