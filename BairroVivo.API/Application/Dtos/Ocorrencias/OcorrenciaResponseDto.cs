namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class OcorrenciaResponseDto
    {
        public decimal IdOcorrencia { get; set; }
        public string Protocolo { get; set; } = string.Empty;
        public decimal IdUsuario { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public decimal IdCategoria { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal IdMunicipio { get; set; }
        public string Municipio { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Urgencia { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Logradouro { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string? Cep { get; set; }
        public int Confirmacoes { get; set; }
        public string ValidadaComunidade { get; set; } = string.Empty;
        public string? CategoriaSugeridaIa { get; set; }
        public decimal? ConfiancaIa { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? AtualizadaEm { get; set; }
        public DateTime? ResolvidaEm { get; set; }
        public DateTime? SlaLimite { get; set; }
        public List<MidiaOcorrenciaResponseDto> Midias { get; set; } = new();
    }
}
