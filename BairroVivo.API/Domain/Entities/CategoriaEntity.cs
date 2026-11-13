namespace BairroVivo.Domain.Entities
{
    public class CategoriaEntity
    {
        public decimal IdCategoria { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public string? Icone { get; set; }

        public string? CorHex { get; set; }

        public int SlaDiasPadrao { get; set; } = 15;

        public string Ativo { get; set; } = "S";

        public ICollection<OcorrenciaEntity> Ocorrencias { get; set; } = new List<OcorrenciaEntity>();
    }
}
