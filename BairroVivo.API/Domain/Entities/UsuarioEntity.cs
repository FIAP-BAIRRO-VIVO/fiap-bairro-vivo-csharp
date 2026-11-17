namespace BairroVivo.Domain.Entities
{
    public class UsuarioEntity
    {
        public long IdUsuario { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SenhaHash { get; set; } = string.Empty;

        public string Papel { get; set; } = "CIDADAO";

        public string FlAtivo { get; set; } = "S";

        public DateTime DtCriacao { get; set; } = DateTime.UtcNow;

        public DateTime DtAtualizacao { get; set; } = DateTime.UtcNow;

        public ICollection<OcorrenciaEntity> Ocorrencias { get; set; } = new List<OcorrenciaEntity>();

        public ICollection<AtualizacaoOcorrenciaEntity> Atualizacoes { get; set; } = new List<AtualizacaoOcorrenciaEntity>();
    }
}
