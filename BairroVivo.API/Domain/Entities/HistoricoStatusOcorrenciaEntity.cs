namespace BairroVivo.Domain.Entities
{
    public class HistoricoStatusOcorrenciaEntity
    {
        public long IdHistorico { get; set; }

        public long IdOcorrencia { get; set; }

        public long? IdUsuario { get; set; }

        public string Operacao { get; set; } = "INSERT"; // INSERT, UPDATE

        public string? StatusAnterior { get; set; }

        public string StatusNovo { get; set; } = "ABERTA";

        public int? CicloAnterior { get; set; }

        public int CicloNovo { get; set; } = 1;

        public DateTime DtEvento { get; set; } = DateTime.UtcNow;

        public OcorrenciaEntity Ocorrencia { get; set; } = null!;

        public UsuarioEntity? Usuario { get; set; }
    }
}
