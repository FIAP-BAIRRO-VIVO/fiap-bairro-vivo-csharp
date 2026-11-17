using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Relatorios
{
    public class RelatorioOcorrenciaResponseDto
    {
        [JsonPropertyName("idMunicipio")]
        public string IdMunicipio { get; set; } = string.Empty;

        [JsonPropertyName("nomeMunicipio")]
        public string NomeMunicipio { get; set; } = string.Empty;

        [JsonPropertyName("uf")]
        public string Uf { get; set; } = string.Empty;

        [JsonPropertyName("idCategoria")]
        public string IdCategoria { get; set; } = string.Empty;

        [JsonPropertyName("nomeCategoria")]
        public string NomeCategoria { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("totalOcorrencias")]
        public int TotalOcorrencias { get; set; }
    }
}
