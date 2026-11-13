using System.ComponentModel.DataAnnotations;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class OcorrenciaRequestDto
    {
        [Range(1, double.MaxValue, ErrorMessage = "A categoria é obrigatória.")]
        public decimal IdCategoria { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "O município é obrigatório.")]
        public decimal IdMunicipio { get; set; }

        [Required(ErrorMessage = "O título da ocorrência é obrigatório.")]
        [MaxLength(140)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição da ocorrência é obrigatória.")]
        [MaxLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Urgencia { get; set; } = "MEDIA";

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        [MaxLength(200)]
        public string? Logradouro { get; set; }

        [Required(ErrorMessage = "O bairro é obrigatório.")]
        [MaxLength(100)]
        public string Bairro { get; set; } = string.Empty;

        [MaxLength(9)]
        public string? Cep { get; set; }

        [MaxLength(60)]
        public string? CategoriaSugeridaIa { get; set; }

        public decimal? ConfiancaIa { get; set; }
    }
}
