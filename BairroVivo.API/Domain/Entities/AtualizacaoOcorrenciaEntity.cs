namespace BairroVivo.Domain.Entities
{
    public class AtualizacaoOcorrenciaEntity
    {
        public long IdAtualizacao { get; set; }

        public long IdOcorrencia { get; set; }

        public long IdUsuario { get; set; }

        public string Tipo { get; set; } = "COMENTARIO"; // COMENTARIO, PERSISTE, RESOLVIDO

        public string? Texto { get; set; }

        public string FlAnonimo { get; set; } = "N";

        public DateTime DtCriacao { get; set; } = DateTime.UtcNow;

        public OcorrenciaEntity Ocorrencia { get; set; } = null!;

        public UsuarioEntity Usuario { get; set; } = null!;

        public ICollection<MidiaOcorrenciaEntity> Midias { get; set; } = new List<MidiaOcorrenciaEntity>();

        public VotoSituacaoEntity? Voto { get; set; }
    }
}
