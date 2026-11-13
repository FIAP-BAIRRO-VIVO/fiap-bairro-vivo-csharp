using System.ComponentModel.DataAnnotations;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class OcorrenciaStatusRequestDto
    {
        [Required(ErrorMessage = "O novo status é obrigatório.")]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Observacao { get; set; }
    }
}
