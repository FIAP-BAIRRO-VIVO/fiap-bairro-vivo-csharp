using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class HistoricoStatusResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonIgnore]
        public long IdHistorico
        {
            get => long.TryParse(Id, out var val) ? val : 0;
            set => Id = value.ToString();
        }

        [JsonPropertyName("idOcorrencia")]
        public string IdOcorrencia { get; set; } = string.Empty;

        [JsonPropertyName("operacao")]
        public string Operacao { get; set; } = string.Empty;

        [JsonPropertyName("statusAnterior")]
        public string? StatusAnterior { get; set; }

        [JsonPropertyName("statusNovo")]
        public string StatusNovo { get; set; } = string.Empty;

        [JsonPropertyName("cicloAnterior")]
        public int? CicloAnterior { get; set; }

        [JsonPropertyName("cicloNovo")]
        public int CicloNovo { get; set; }

        [JsonPropertyName("dtEvento")]
        public DateTime DtEvento { get; set; }
    }
}
