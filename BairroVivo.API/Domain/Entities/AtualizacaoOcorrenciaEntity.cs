namespace BairroVivo.Domain.Entities
{
    public class AtualizacaoOcorrenciaEntity
    {
        public decimal IdAtualizacao { get; set; }

        public decimal IdOcorrencia { get; set; }

        public decimal IdUsuario { get; set; }

        public string Tipo { get; set; } = "CONFIRMACAO";

        public string? Comentario { get; set; }

        public string? UrlFotoEvidencia { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public DateTime CriadaEm { get; set; }

        public OcorrenciaEntity Ocorrencia { get; set; } = null!;

        public UsuarioEntity Usuario { get; set; } = null!;
    }
}
