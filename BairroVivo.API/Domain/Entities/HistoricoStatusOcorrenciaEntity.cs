namespace BairroVivo.Domain.Entities
{
    public class HistoricoStatusOcorrenciaEntity
    {
        public decimal IdHistorico { get; set; }

        public decimal IdOcorrencia { get; set; }

        public decimal? IdUsuarioResponsavel { get; set; }

        public string? StatusAnterior { get; set; }

        public string StatusNovo { get; set; } = string.Empty;

        public string? Observacao { get; set; }

        public DateTime AlteradoEm { get; set; }

        public OcorrenciaEntity Ocorrencia { get; set; } = null!;

        public UsuarioEntity? UsuarioResponsavel { get; set; }
    }
}
