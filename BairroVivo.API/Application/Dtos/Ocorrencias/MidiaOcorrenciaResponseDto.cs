using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class MidiaOcorrenciaResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonIgnore]
        public long IdMidia
        {
            get => long.TryParse(Id, out var val) ? val : 0;
            set => Id = value.ToString();
        }

        [JsonPropertyName("idOcorrencia")]
        public string IdOcorrencia { get; set; } = string.Empty;

        [JsonPropertyName("idAtualizacao")]
        public string? IdAtualizacao { get; set; }

        [JsonPropertyName("provedor")]
        public string Provedor { get; set; } = string.Empty;

        [JsonPropertyName("chaveStorage")]
        public string ChaveStorage { get; set; } = string.Empty;

        [JsonPropertyName("urlExibicao")]
        public string? UrlExibicao { get; set; }

        [JsonIgnore]
        public string? UrlMidia => UrlExibicao ?? ChaveStorage;

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("tamanhoBytes")]
        public long TamanhoBytes { get; set; }

        [JsonPropertyName("dtCriacao")]
        public DateTime DtCriacao { get; set; }
    }
}
