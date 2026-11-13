using System.ComponentModel.DataAnnotations;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class MidiaOcorrenciaRequestDto
    {
        [Required(ErrorMessage = "A URL da mídia é obrigatória.")]
        [MaxLength(500)]
        public string UrlMidia { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? TipoMidia { get; set; } = "FOTO";

        [MaxLength(1)]
        public string? Principal { get; set; } = "N";
    }
}
