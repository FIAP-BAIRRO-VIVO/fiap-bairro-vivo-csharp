namespace BairroVivo.Domain.Entities
{
    public class RelatorioOcorrenciaEntity
    {
        public long IdMunicipio { get; set; }

        public long IdCategoria { get; set; }

        public string Status { get; set; } = string.Empty;

        public int TotalOcorrencias { get; set; }

        public MunicipioEntity? Municipio { get; set; }

        public CategoriaEntity? Categoria { get; set; }
    }
}
