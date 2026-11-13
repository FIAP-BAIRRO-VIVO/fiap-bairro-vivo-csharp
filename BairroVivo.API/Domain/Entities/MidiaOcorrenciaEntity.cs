namespace BairroVivo.Domain.Entities
{
    public class MidiaOcorrenciaEntity
    {
        public decimal IdMidia { get; set; }

        public decimal IdOcorrencia { get; set; }

        public string UrlMidia { get; set; } = string.Empty;

        public string TipoMidia { get; set; } = "FOTO";

        public string Principal { get; set; } = "N";

        public DateTime CriadaEm { get; set; }

        public OcorrenciaEntity Ocorrencia { get; set; } = null!;
    }
}
