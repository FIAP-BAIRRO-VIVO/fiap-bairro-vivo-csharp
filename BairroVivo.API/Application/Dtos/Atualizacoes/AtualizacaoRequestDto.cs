using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Atualizacoes
{
    public class AtualizacaoRequestDto
    {
        [Required(ErrorMessage = "O tipo de atualização é obrigatório.")]
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "COMENTARIO";

        [JsonPropertyName("texto")]
        public string? Texto { get; set; }

        [JsonPropertyName("comentario")]
        public string? Comentario
        {
            get => Texto;
            set => Texto ??= value;
        }

        [JsonPropertyName("anonimoPublico")]
        public bool AnonimoPublico { get; set; } = false;

        [JsonPropertyName("urlFotoEvidencia")]
        public string? UrlFotoEvidencia { get; set; }
    }
}
