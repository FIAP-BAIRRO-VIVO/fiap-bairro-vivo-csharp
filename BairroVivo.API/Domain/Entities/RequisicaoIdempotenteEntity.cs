namespace BairroVivo.Domain.Entities
{
    public class RequisicaoIdempotenteEntity
    {
        public long IdRequisicao { get; set; }

        public long IdUsuario { get; set; }

        public string Chave { get; set; } = string.Empty;

        public string Rota { get; set; } = string.Empty;

        public string MetodoHttp { get; set; } = string.Empty;

        public string HashCorpo { get; set; } = string.Empty;

        public long? IdRecurso { get; set; }

        public DateTime DtCriacao { get; set; } = DateTime.UtcNow;

        public UsuarioEntity Usuario { get; set; } = null!;
    }
}
