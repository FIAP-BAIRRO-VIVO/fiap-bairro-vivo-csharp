namespace BairroVivo.Application.Dtos.Municipios
{
    public class MunicipioResponseDto
    {
        public decimal IdMunicipio { get; set; }
        public string CodigoIbge { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Ativo { get; set; } = string.Empty;
    }
}
