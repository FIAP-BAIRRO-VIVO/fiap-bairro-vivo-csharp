namespace BairroVivo.Domain.Entities
{
    public class MidiaOcorrenciaEntity
    {
        public long IdMidia { get; set; }

        public long IdOcorrencia { get; set; }

        public long? IdAtualizacao { get; set; }

        public long IdUsuario { get; set; }

        public string Provedor { get; set; } = "LOCAL";

        public string ChaveStorage { get; set; } = string.Empty;

        public string? UrlExibicao { get; set; }

        public string MimeType { get; set; } = "image/jpeg";

        public long TamanhoBytes { get; set; }

        public DateTime DtCriacao { get; set; } = DateTime.UtcNow;

        public OcorrenciaEntity Ocorrencia { get; set; } = null!;

        public AtualizacaoOcorrenciaEntity? Atualizacao { get; set; }

        public UsuarioEntity Usuario { get; set; } = null!;
    }
}
