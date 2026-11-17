using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Atualizacoes;
using BairroVivo.Application.Dtos.Ocorrencias;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;
using BairroVivo.Domain.Entities;
using BairroVivo.Domain.Rules;

namespace BairroVivo.Application.UseCases
{
    public class AtualizacaoUseCase : IAtualizacaoUseCase
    {
        private readonly IAtualizacaoRepository _atualizacaoRepository;
        private readonly IOcorrenciaRepository _ocorrenciaRepository;
        private readonly IIntegracaoOracleRepository _integracaoOracle;

        public AtualizacaoUseCase(
            IAtualizacaoRepository atualizacaoRepository,
            IOcorrenciaRepository ocorrenciaRepository,
            IIntegracaoOracleRepository integracaoOracle)
        {
            _atualizacaoRepository = atualizacaoRepository;
            _ocorrenciaRepository = ocorrenciaRepository;
            _integracaoOracle = integracaoOracle;
        }

        public IEnumerable<AtualizacaoResponseDto> ObterPorOcorrencia(long idOcorrencia)
        {
            if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
                throw new KeyNotFoundException("Ocorrência não encontrada ou excluída.");

            return _atualizacaoRepository.ObterPorOcorrencia(idOcorrencia)
                .Select(a => a.ToResponseDto());
        }

        public PageResponseDto<AtualizacaoResponseDto> ObterPorOcorrenciaPaginado(
            long idOcorrencia, int pagina, int tamanho)
        {
            if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
                throw new KeyNotFoundException("Ocorrência não encontrada ou excluída.");

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

        public AtualizacaoResponseDto? ObterPorId(long idAtualizacao)
        {
            var atualizacao = _atualizacaoRepository.ObterPorId(idAtualizacao);
            return atualizacao?.ToResponseDto();
        }

        public async Task<AtualizacaoResponseDto> RegistrarAsync(long idOcorrencia, long idUsuario, AtualizacaoRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                throw new KeyNotFoundException("Ocorrência não encontrada ou excluída.");

            if (!_ocorrenciaRepository.ExisteUsuario(idUsuario))
                throw new KeyNotFoundException("Usuário não encontrado ou inativo.");

            var tipo = AtualizacaoRules.ValidarTipo(dto.Tipo);
            var flAnonimo = dto.AnonimoPublico ? "S" : "N";
            var texto = dto.Texto ?? dto.Comentario;

            // Executa a procedure oficial PKG_COMUNIDADE.PR_REGISTRAR_ATUALIZACAO no Oracle
            // (ou simulação fiel em memória durante os testes unitários)
            var idCriado = await _integracaoOracle.ExecutarRegistrarAtualizacaoAsync(
                idOcorrencia, idUsuario, tipo, texto, flAnonimo);

            // Consulta o estado real e atualizado da ocorrência e da atualização após a procedure
            var ocorrenciaAtualizada = _ocorrenciaRepository.ObterPorId(idOcorrencia) ?? ocorrencia;
            var atualizacaoCriada = _atualizacaoRepository.ObterPorId(idCriado);

            var confirmacoesNoCiclo = 0;
            if (tipo == "RESOLVIDO" || tipo == "PERSISTE")
            {
                confirmacoesNoCiclo = await _integracaoOracle.ContarVotosAtivosAsync(idOcorrencia, tipo);
            }

            var foiVoto = atualizacaoCriada?.Voto != null ||
                          AtualizacaoRules.ContaComoVoto(ocorrencia.Status, tipo);

            var dtoResposta = atualizacaoCriada != null
                ? atualizacaoCriada.ToResponseDto()
                : new AtualizacaoResponseDto
                {
                    Id = idCriado.ToString(),
                    IdOcorrencia = idOcorrencia.ToString(),
                    Tipo = tipo,
                    Texto = texto,
                    AutorExibicao = dto.AnonimoPublico ? "Cidadao anonimo" : "Cidadao",
                    AnonimoPublico = dto.AnonimoPublico,
                    DtCriacao = DateTime.UtcNow
                };

            dtoResposta.ContabilizadaComoVoto = foiVoto;
            dtoResposta.StatusAtual = ocorrenciaAtualizada.Status;
            dtoResposta.NumeroCiclo = ocorrenciaAtualizada.NumCiclo;
            dtoResposta.ConfirmacoesNoCiclo = confirmacoesNoCiclo;

            return dtoResposta;
        }

        public MidiaOcorrenciaResponseDto AdicionarMidia(long idOcorrencia, long idAtualizacao, long idUsuario, MidiaOcorrenciaRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                throw new KeyNotFoundException("Ocorrência não encontrada.");

            var atualizacao = _atualizacaoRepository.ObterPorId(idAtualizacao);
            if (atualizacao == null || atualizacao.IdOcorrencia != idOcorrencia)
                throw new KeyNotFoundException("Atualização não encontrada para esta ocorrência.");

            if (atualizacao.IdUsuario != idUsuario)
                throw new UnauthorizedAccessException("Somente o autor da atualização pode anexar foto a ela.");

            var chave = !string.IsNullOrWhiteSpace(dto.ChaveStorage) ? dto.ChaveStorage : dto.UrlExibicao;
            if (string.IsNullOrWhiteSpace(chave))
                throw new ArgumentException("A chave de storage ou URL da mídia é obrigatória.");

            var mime = (dto.MimeType ?? "image/jpeg").Trim().ToLowerInvariant();
            if (mime != "image/jpeg" && mime != "image/png" && mime != "image/webp")
                throw new ArgumentException("Formato de mídia não suportado. Utilize image/jpeg, image/png ou image/webp.");

            var salva = _atualizacaoRepository.AdicionarMidia(new MidiaOcorrenciaEntity
            {
                IdOcorrencia = idOcorrencia,
                IdAtualizacao = idAtualizacao,
                IdUsuario = idUsuario,
                Provedor = !string.IsNullOrWhiteSpace(dto.Provedor) ? dto.Provedor.Trim() : "LOCAL",
                ChaveStorage = chave.Trim(),
                UrlExibicao = dto.UrlExibicao?.Trim(),
                MimeType = mime,
                TamanhoBytes = dto.TamanhoBytes > 0 ? dto.TamanhoBytes : 1024,
                DtCriacao = DateTime.UtcNow
            });

            return salva.ToResponseDto();
        }
    }
}
