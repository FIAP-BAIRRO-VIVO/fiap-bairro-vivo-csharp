using BairroVivo.API.Application.Dtos;

namespace BairroVivo.Application.Dtos.Atualizacoes
{
    public class AtualizacaoResponseDto
    {
        public decimal IdAtualizacao { get; set; }
        public decimal IdOcorrencia { get; set; }
        public decimal IdUsuario { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Comentario { get; set; }
        public string? UrlFotoEvidencia { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime CriadaEm { get; set; }
        public List<LinkDto> Links { get; set; } = new();
    }
}
