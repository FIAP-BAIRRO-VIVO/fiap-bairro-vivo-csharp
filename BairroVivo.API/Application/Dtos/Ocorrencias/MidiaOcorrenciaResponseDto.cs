namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class MidiaOcorrenciaResponseDto
    {
        public decimal IdMidia { get; set; }
        public decimal IdOcorrencia { get; set; }
        public string UrlMidia { get; set; } = string.Empty;
        public string TipoMidia { get; set; } = string.Empty;
        public string Principal { get; set; } = string.Empty;
        public DateTime CriadaEm { get; set; }
    }
}
