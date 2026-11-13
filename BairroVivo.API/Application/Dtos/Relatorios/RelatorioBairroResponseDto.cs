using BairroVivo.API.Application.Dtos;

namespace BairroVivo.Application.Dtos.Relatorios
{
    public class RelatorioBairroResponseDto
    {
        public decimal IdRelatorio { get; set; }
        public decimal IdMunicipio { get; set; }
        public string Municipio { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string PeriodoRef { get; set; } = string.Empty;
        public int TotalOcorrencias { get; set; }
        public int TotalResolvidas { get; set; }
        public string ResumoIa { get; set; } = string.Empty;
        public string? ModeloIa { get; set; }
        public DateTime GeradoEm { get; set; }
        public List<LinkDto> Links { get; set; } = new();
    }
}
