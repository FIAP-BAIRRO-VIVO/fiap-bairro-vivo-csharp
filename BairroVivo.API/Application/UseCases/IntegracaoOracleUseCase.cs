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

    public async Task<long> ExecutarRegistrarAtualizacaoAsync(
        long idOcorrencia, long idUsuario, AtualizacaoRequestDto dto)
    {
        if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
            throw new KeyNotFoundException("Ocorrência não encontrada ou excluída.");

        var tipo = AtualizacaoRules.ValidarTipo(dto.Tipo);
        var flAnonimo = dto.AnonimoPublico ? "S" : "N";
        var texto = dto.Texto ?? dto.Comentario;

        return await _integracaoOracleRepository.ExecutarRegistrarAtualizacaoAsync(
            idOcorrencia, idUsuario, tipo, texto, flAnonimo);
    }

    public async Task<int> ContarVotosAtivosAsync(long idOcorrencia, string tipoVoto)
    {
        if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
            throw new KeyNotFoundException("Ocorrência não encontrada ou excluída.");

        return await _integracaoOracleRepository.ContarVotosAtivosAsync(idOcorrencia, tipoVoto);
    }
}
