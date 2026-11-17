using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class MidiaOcorrenciaRequestDto
    {
        [JsonPropertyName("provedor")]
        public string Provedor { get; set; } = "LOCAL";

        [JsonPropertyName("chaveStorage")]
        public string ChaveStorage { get; set; } = string.Empty;

        [JsonPropertyName("urlExibicao")]
        public string? UrlExibicao { get; set; }

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = "image/jpeg";

        [JsonPropertyName("tamanhoBytes")]
        public long TamanhoBytes { get; set; } = 1024;

        // Propriedades retrocompatíveis
        [JsonPropertyName("urlMidia")]
        public string? UrlMidia
        {
            get => UrlExibicao ?? ChaveStorage;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    UrlExibicao = value;
                    if (string.IsNullOrWhiteSpace(ChaveStorage))
                        ChaveStorage = value;
                }
            }
        }

        [JsonPropertyName("tipoMidia")]
        public string? TipoMidia { get; set; } = "FOTO";

        [JsonPropertyName("principal")]
        public string? Principal { get; set; } = "N";
    }
}
