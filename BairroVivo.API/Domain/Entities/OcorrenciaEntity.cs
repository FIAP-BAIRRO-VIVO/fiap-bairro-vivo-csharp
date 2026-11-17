namespace BairroVivo.Domain.Entities
{
    public class OcorrenciaEntity
    {
        public long IdOcorrencia { get; set; }

        public long IdUsuario { get; set; }

        public long IdCategoria { get; set; }

        public long IdMunicipio { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public string? EnderecoReferencia { get; set; }

        public string FlAnonimo { get; set; } = "N";

        public string Status { get; set; } = "ABERTA";

        public int NumCiclo { get; set; } = 1;

        public string FlExcluida { get; set; } = "N";

        public long? IdAutorTransicao { get; set; }

        public DateTime DtCriacao { get; set; } = DateTime.UtcNow;

        public DateTime DtAtualizacao { get; set; } = DateTime.UtcNow;

        // Propriedades de navegação
        public UsuarioEntity Usuario { get; set; } = null!;

        public CategoriaEntity Categoria { get; set; } = null!;

        public MunicipioEntity Municipio { get; set; } = null!;

        public UsuarioEntity? AutorTransicao { get; set; }

        public ICollection<MidiaOcorrenciaEntity> Midias { get; set; } = new List<MidiaOcorrenciaEntity>();

        public ICollection<AtualizacaoOcorrenciaEntity> Atualizacoes { get; set; } = new List<AtualizacaoOcorrenciaEntity>();

        public ICollection<VotoSituacaoEntity> Votos { get; set; } = new List<VotoSituacaoEntity>();

        public ICollection<HistoricoStatusOcorrenciaEntity> HistoricoStatus { get; set; } = new List<HistoricoStatusOcorrenciaEntity>();
    }
}
