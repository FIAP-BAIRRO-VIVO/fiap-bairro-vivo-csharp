namespace BairroVivo.Domain.Entities
{
    public class RequisicaoIdempotenteEntity
    {
        public decimal IdRequisicao { get; set; }

        public string ChaveIdempotencia { get; set; } = string.Empty;

        public decimal? IdUsuario { get; set; }

        public string Endpoint { get; set; } = string.Empty;

        public string MetodoHttp { get; set; } = string.Empty;

        public int StatusHttpResposta { get; set; }

        public string? CorpoResposta { get; set; }

        public DateTime CriadaEm { get; set; }

        public DateTime ExpiraEm { get; set; }
    }
}
