namespace BairroVivo.Domain.Entities
{
    public class AuditoriaOcorrenciaEntity
    {
        public decimal IdAuditoria { get; set; }

        public decimal IdOcorrencia { get; set; }

        public string Operacao { get; set; } = string.Empty;

        public string? UsuarioBanco { get; set; }

        public string? StatusAnterior { get; set; }

        public string? StatusNovo { get; set; }

        public string? DadosAnteriores { get; set; }

        public string? DadosNovos { get; set; }

        public DateTime DataOperacao { get; set; }
    }
}
