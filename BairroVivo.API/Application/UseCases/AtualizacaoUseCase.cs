using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;
using BairroVivo.Domain.Rules;

namespace BairroVivo.Application.UseCases
{
    public class AtualizacaoUseCase : IAtualizacaoUseCase
    {
        private readonly IAtualizacaoRepository _atualizacaoRepository;
        private readonly IOcorrenciaRepository _ocorrenciaRepository;

        public AtualizacaoUseCase(
            IAtualizacaoRepository atualizacaoRepository,
            IOcorrenciaRepository ocorrenciaRepository)
        {
            _atualizacaoRepository = atualizacaoRepository;
            _ocorrenciaRepository = ocorrenciaRepository;
        }

        public IEnumerable<AtualizacaoResponseDto> ObterPorOcorrencia(decimal idOcorrencia)
        {
            if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
                throw new KeyNotFoundException("Ocorrência não encontrada.");

            return _atualizacaoRepository.ObterPorOcorrencia(idOcorrencia)
                .Select(a => a.ToResponseDto());
        }

        public PageResponseDto<AtualizacaoResponseDto> ObterPorOcorrenciaPaginado(
            decimal idOcorrencia, int pagina, int tamanho)
        {
            if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
                throw new KeyNotFoundException("Ocorrência não encontrada.");

            pagina = Math.Max(1, pagina);
            tamanho = Math.Clamp(tamanho, 1, 50);
            var deslocamento = (pagina - 1) * tamanho;

            var (itens, total) = _atualizacaoRepository
                .ObterPorOcorrenciaPaginado(idOcorrencia, deslocamento, tamanho);

            return new PageResponseDto<AtualizacaoResponseDto>
            {
                Deslocamento = deslocamento,
                RegistroRetornado = tamanho,
                TotalRegistros = total,
                Items = itens.Select(a => a.ToResponseDto()).ToList()
            };
        }

        public AtualizacaoResponseDto? ObterPorId(decimal idAtualizacao)
        {
            var atualizacao = _atualizacaoRepository.ObterPorId(idAtualizacao);
            return atualizacao?.ToResponseDto();
        }

        public AtualizacaoResponseDto Registrar(decimal idOcorrencia, decimal idUsuario, AtualizacaoRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                throw new KeyNotFoundException("Ocorrência não encontrada.");

            if (!_ocorrenciaRepository.ExisteUsuario(idUsuario))
                throw new KeyNotFoundException("Usuário não encontrado.");

            dto.Tipo = AtualizacaoRules.ValidarTipo(dto.Tipo);

            if ((dto.Tipo == "CONFIRMACAO" || dto.Tipo == "JA_RESOLVIDO") &&
                _atualizacaoRepository.JaVotou(idOcorrencia, idUsuario, dto.Tipo))
            {
                throw new InvalidOperationException(
                    $"O cidadão já registrou '{dto.Tipo}' para esta ocorrência.");
            }

            var entity = dto.ToEntity(idOcorrencia, idUsuario);
            var salva = _atualizacaoRepository.Registrar(entity);

            if (dto.Tipo == "CONFIRMACAO")
            {
                ocorrencia.Confirmacoes += 1;
                var (validada, novaUrgencia) = AtualizacaoRules.AvaliarValidacaoComunitaria(
                    ocorrencia.Confirmacoes, ocorrencia.Urgencia);
                ocorrencia.ValidadaComunidade = validada;
                ocorrencia.Urgencia = novaUrgencia;
                ocorrencia.AtualizadaEm = DateTime.Now;
                _ocorrenciaRepository.Atualizar(ocorrencia);
            }
            else if (dto.Tipo == "JA_RESOLVIDO" && ocorrencia.Status != "RESOLVIDA")
            {
                ocorrencia.Status = "EM_ANALISE";
                ocorrencia.AtualizadaEm = DateTime.Now;
                _ocorrenciaRepository.Atualizar(ocorrencia);
            }

            var completa = _atualizacaoRepository.ObterPorId(salva.IdAtualizacao) ?? salva;
            return completa.ToResponseDto();
        }
    }
}
