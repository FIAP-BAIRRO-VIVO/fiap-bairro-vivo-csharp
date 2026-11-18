using System.Text.Json.Serialization;
using BairroVivo.API.Application.Dtos;
using BairroVivo.Application.Dtos.Categorias;
using BairroVivo.Application.Dtos.Municipios;

namespace BairroVivo.Application.Dtos.Ocorrencias
{
    public class OcorrenciaResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("idOcorrencia")]
        public long IdOcorrencia
        {
            get => long.TryParse(Id, out var val) ? val : 0;
            set => Id = value.ToString();
        }

        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "ABERTA";

        [JsonPropertyName("numeroCiclo")]
        public int NumeroCiclo { get; set; } = 1;

        [JsonPropertyName("categoria")]
        public CategoriaResponseDto Categoria { get; set; } = new();

        [JsonPropertyName("idCategoria")]
        public long IdCategoria => Categoria.IdCategoria;

        [JsonPropertyName("municipio")]
        public MunicipioResponseDto Municipio { get; set; } = new();

        [JsonPropertyName("idMunicipio")]
        public long IdMunicipio => Municipio.IdMunicipio;

        [JsonPropertyName("uf")]
        public string Uf => Municipio.Uf;

        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        [JsonPropertyName("enderecoReferencia")]
        public string? EnderecoReferencia { get; set; }

        [JsonPropertyName("autorExibicao")]
        public string AutorExibicao { get; set; } = string.Empty;

        [JsonPropertyName("anonimoPublico")]
        public bool AnonimoPublico { get; set; } = false;

        [JsonPropertyName("dtCriacao")]
        public DateTime DtCriacao { get; set; }

        [JsonPropertyName("dtAtualizacao")]
        public DateTime DtAtualizacao { get; set; }

        [JsonPropertyName("midias")]
        public List<MidiaOcorrenciaResponseDto> Midias { get; set; } = new();

        [JsonPropertyName("totalVotosCiclo")]
        public int TotalVotosCiclo { get; set; }

        [JsonPropertyName("links")]
        public List<LinkDto> Links { get; set; } = new();
    }
}
