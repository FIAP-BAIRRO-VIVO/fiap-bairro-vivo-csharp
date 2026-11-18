using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class OcorrenciaRequestDto
    {
        [Required(ErrorMessage = "O identificador da categoria é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Categoria inválida.")]
        [JsonPropertyName("categoriaId")]
        public long CategoriaId { get; set; }

        [Required(ErrorMessage = "O identificador do município é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Município inválido.")]
        [JsonPropertyName("municipioId")]
        public long MunicipioId { get; set; }

        [Required(ErrorMessage = "O título da ocorrência é obrigatório.")]
        [MinLength(5, ErrorMessage = "O título deve ter no mínimo 5 caracteres.")]
        [MaxLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição da ocorrência é obrigatória.")]
        [MinLength(10, ErrorMessage = "A descrição deve ter no mínimo 10 caracteres.")]
        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude deve estar entre -90 e 90.")]
        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude deve estar entre -180 e 180.")]
        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        [MaxLength(255)]
        [JsonPropertyName("enderecoReferencia")]
        public string? EnderecoReferencia { get; set; }

        [JsonPropertyName("anonimoPublico")]
        public bool AnonimoPublico { get; set; } = false;
    }
}
