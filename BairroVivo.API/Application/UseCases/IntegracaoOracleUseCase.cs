using BairroVivo.API.Application.Interfaces;
using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Domain.Rules;

namespace BairroVivo.API.Application.UseCases;

public class IntegracaoOracleUseCase : IIntegracaoOracleUseCase
{
    private readonly IIntegracaoOracleRepository _integracaoOracleRepository;
    private readonly IOcorrenciaRepository _ocorrenciaRepository;

    public IntegracaoOracleUseCase(
        IIntegracaoOracleRepository integracaoOracleRepository,
        IOcorrenciaRepository ocorrenciaRepository)
    {
        _integracaoOracleRepository = integracaoOracleRepository;
        _ocorrenciaRepository = ocorrenciaRepository;
    }

    public async Task ExecutarRegistrarAtualizacaoAsync(
        decimal idOcorrencia, decimal idUsuario, AtualizacaoRequestDto dto)
    {
        if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
            throw new KeyNotFoundException("Ocorrência não encontrada.");

        var tipo = AtualizacaoRules.ValidarTipo(dto.Tipo);
        await _integracaoOracleRepository.ExecutarRegistrarAtualizacaoAsync(
            idOcorrencia, idUsuario, tipo, dto.Comentario, dto.UrlFotoEvidencia);
    }

    public async Task<int> ContarVotosAtivosAsync(decimal idOcorrencia)
    {
        if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
            throw new KeyNotFoundException("Ocorrência não encontrada.");

        return await _integracaoOracleRepository.ContarVotosAtivosAsync(idOcorrencia);
    }
}
