using System.ComponentModel.DataAnnotations;

namespace BairroVivo.Application.Dtos.Categorias
{
    public class CategoriaRequestDto
    {
        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [MaxLength(60)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descricao { get; set; }

        [MaxLength(40)]
        public string? Icone { get; set; }

        [MaxLength(7)]
        public string? CorHex { get; set; }

        [Range(1, 365)]
        public int SlaDiasPadrao { get; set; } = 15;
    }
}
