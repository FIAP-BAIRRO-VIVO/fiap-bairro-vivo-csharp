using System.ComponentModel.DataAnnotations;

namespace BairroVivo.Application.Dtos.Atualizacoes
{
    public class AtualizacaoRequestDto
    {
        [Required(ErrorMessage = "O tipo de atualização é obrigatório.")]
        [MaxLength(25)]
        public string Tipo { get; set; } = "CONFIRMACAO";

        [MaxLength(1000)]
        public string? Comentario { get; set; }

        [MaxLength(500)]
        public string? UrlFotoEvidencia { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
