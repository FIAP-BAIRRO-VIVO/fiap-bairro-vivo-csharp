namespace BairroVivo.Domain.Entities
{
    public class CategoriaEntity
    {
        public long IdCategoria { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public string? Icone { get; set; }

        public string FlAtiva { get; set; } = "S";

        public ICollection<OcorrenciaEntity> Ocorrencias { get; set; } = new List<OcorrenciaEntity>();
    }
}
