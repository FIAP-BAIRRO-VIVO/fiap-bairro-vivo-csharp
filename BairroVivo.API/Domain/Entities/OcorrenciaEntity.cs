namespace BairroVivo.Domain.Entities
{
    public class OcorrenciaEntity
    {
        public decimal IdOcorrencia { get; set; }

        public string Protocolo { get; set; } = string.Empty;

        public decimal IdUsuario { get; set; }

        public decimal IdCategoria { get; set; }

        public decimal IdMunicipio { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Status { get; set; } = "ABERTA";

        public string Urgencia { get; set; } = "MEDIA";

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public string? Logradouro { get; set; }

        public string Bairro { get; set; } = string.Empty;

        public string? Cep { get; set; }

        public int Confirmacoes { get; set; }

        public string ValidadaComunidade { get; set; } = "N";

        public string? CategoriaSugeridaIa { get; set; }

        public decimal? ConfiancaIa { get; set; }

        public DateTime CriadaEm { get; set; }

        public DateTime? AtualizadaEm { get; set; }

        public DateTime? ResolvidaEm { get; set; }

        public DateTime? SlaLimite { get; set; }

        public UsuarioEntity Usuario { get; set; } = null!;

        public CategoriaEntity Categoria { get; set; } = null!;

        public MunicipioEntity Municipio { get; set; } = null!;

        public ICollection<MidiaOcorrenciaEntity> Midias { get; set; } = new List<MidiaOcorrenciaEntity>();

        public ICollection<AtualizacaoOcorrenciaEntity> Atualizacoes { get; set; } = new List<AtualizacaoOcorrenciaEntity>();

        public ICollection<HistoricoStatusOcorrenciaEntity> HistoricoStatus { get; set; } = new List<HistoricoStatusOcorrenciaEntity>();
    }
}
