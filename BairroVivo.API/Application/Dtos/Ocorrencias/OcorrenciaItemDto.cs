using BairroVivo.Application.Dtos.Ocorrencias;

namespace BairroVivo.API.Application.Dtos.Ocorrencias;

public class OcorrenciaItemDto
{
    public OcorrenciaResponseDto Ocorrencia { get; set; } = new();
    public List<LinkDto> Links { get; set; } = new();
}
