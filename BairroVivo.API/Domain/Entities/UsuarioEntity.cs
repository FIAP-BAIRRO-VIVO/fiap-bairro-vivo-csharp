namespace BairroVivo.Domain.Entities
{
    public class UsuarioEntity
    {
        public decimal IdUsuario { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SenhaHash { get; set; } = string.Empty;

        public string? CpfHash { get; set; }

        public string? Telefone { get; set; }

        public string? Bairro { get; set; }

        public decimal? IdMunicipio { get; set; }

        public string Perfil { get; set; } = "CIDADAO";

        public int Pontos { get; set; }

        public string Ativo { get; set; } = "S";

        public DateTime CriadoEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }

        public MunicipioEntity? Municipio { get; set; }

        public ICollection<OcorrenciaEntity> Ocorrencias { get; set; } = new List<OcorrenciaEntity>();

        public ICollection<AtualizacaoOcorrenciaEntity> Atualizacoes { get; set; } = new List<AtualizacaoOcorrenciaEntity>();
    }
}
