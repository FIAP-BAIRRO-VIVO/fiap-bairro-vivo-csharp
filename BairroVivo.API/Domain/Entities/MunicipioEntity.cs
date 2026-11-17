namespace BairroVivo.Domain.Entities
{
    public class MunicipioEntity
    {
        public long IdMunicipio { get; set; }

        public int? CodigoIbge { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Uf { get; set; } = string.Empty;

        public ICollection<OcorrenciaEntity> Ocorrencias { get; set; } = new List<OcorrenciaEntity>();
    }
}
