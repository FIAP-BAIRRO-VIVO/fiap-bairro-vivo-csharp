namespace BairroVivo.Domain.Entities
{
    public class VotoSituacaoEntity
    {
        public long IdVoto { get; set; }

        public long IdOcorrencia { get; set; }

        public long IdAtualizacao { get; set; }

        public long IdUsuario { get; set; }

        public int NumCiclo { get; set; }

        public string TipoVoto { get; set; } = "RESOLVIDO"; // PERSISTE, RESOLVIDO

        public DateTime DtVoto { get; set; } = DateTime.UtcNow;

        public OcorrenciaEntity Ocorrencia { get; set; } = null!;

        public AtualizacaoOcorrenciaEntity Atualizacao { get; set; } = null!;

        public UsuarioEntity Usuario { get; set; } = null!;
    }
}
