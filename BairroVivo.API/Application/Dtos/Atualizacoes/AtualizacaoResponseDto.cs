using System.Text.Json.Serialization;
using BairroVivo.API.Application.Dtos;
using BairroVivo.Application.Dtos.Ocorrencias;

namespace BairroVivo.Application.Dtos.Atualizacoes
{
    public class AtualizacaoResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("idAtualizacao")]
        public long IdAtualizacao
        {
            get => long.TryParse(Id, out var val) ? val : 0;
            set => Id = value.ToString();
        }

        [JsonPropertyName("atualizacaoId")]
        public string AtualizacaoId => Id;

        [JsonPropertyName("idOcorrencia")]
        public string IdOcorrencia { get; set; } = string.Empty;

        [JsonPropertyName("ocorrenciaId")]
        public string OcorrenciaId => IdOcorrencia;

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("texto")]
        public string? Texto { get; set; }

        [JsonPropertyName("comentario")]
        public string? Comentario => Texto;

        [JsonPropertyName("autorExibicao")]
        public string AutorExibicao { get; set; } = string.Empty;

        [JsonPropertyName("nomeUsuario")]
        public string NomeUsuario => AutorExibicao;

        [JsonPropertyName("anonimoPublico")]
        public bool AnonimoPublico { get; set; } = false;

        [JsonPropertyName("contabilizadaComoVoto")]
        public bool ContabilizadaComoVoto { get; set; }

        [JsonPropertyName("statusAtual")]
        public string StatusAtual { get; set; } = string.Empty;

        [JsonPropertyName("numeroCiclo")]
        public int NumeroCiclo { get; set; }

        [JsonPropertyName("confirmacoesNoCiclo")]
        public int ConfirmacoesNoCiclo { get; set; }

        [JsonPropertyName("dtCriacao")]
        public DateTime DtCriacao { get; set; }

        [JsonPropertyName("criadaEm")]
        public DateTime CriadaEm => DtCriacao;

        [JsonPropertyName("midias")]
        public List<MidiaOcorrenciaResponseDto> Midias { get; set; } = new();

        [JsonPropertyName("links")]
        public List<LinkDto> Links { get; set; } = new();
    }
}
