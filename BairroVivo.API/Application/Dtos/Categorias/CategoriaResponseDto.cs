namespace BairroVivo.Application.Dtos.Categorias
{
    public class CategoriaResponseDto
    {
        public decimal IdCategoria { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Icone { get; set; }
        public string? CorHex { get; set; }
        public int SlaDiasPadrao { get; set; }
        public string Ativo { get; set; } = string.Empty;
    }
}
