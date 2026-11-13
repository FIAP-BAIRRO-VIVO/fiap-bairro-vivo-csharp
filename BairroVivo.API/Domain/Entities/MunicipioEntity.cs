namespace BairroVivo.Domain.Entities
{
    public class MunicipioEntity
    {
        public decimal IdMunicipio { get; set; }

        public string CodigoIbge { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public string Uf { get; set; } = string.Empty;

        public string Ativo { get; set; } = "S";

        public ICollection<UsuarioEntity> Usuarios { get; set; } = new List<UsuarioEntity>();

        public ICollection<OcorrenciaEntity> Ocorrencias { get; set; } = new List<OcorrenciaEntity>();

        public ICollection<RelatorioBairroEntity> Relatorios { get; set; } = new List<RelatorioBairroEntity>();
    }
}
