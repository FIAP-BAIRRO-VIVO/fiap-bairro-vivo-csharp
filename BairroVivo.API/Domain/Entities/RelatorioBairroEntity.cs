namespace BairroVivo.Domain.Entities
{
    public class RelatorioBairroEntity
    {
        public decimal IdRelatorio { get; set; }

        public decimal IdMunicipio { get; set; }

        public string Bairro { get; set; } = string.Empty;

        public string PeriodoRef { get; set; } = string.Empty;

        public int TotalOcorrencias { get; set; }

        public int TotalResolvidas { get; set; }

        public string ResumoIa { get; set; } = string.Empty;

        public string? ModeloIa { get; set; }

        public DateTime GeradoEm { get; set; }

        public MunicipioEntity Municipio { get; set; } = null!;
    }
}
