using BairroVivo.API.Application.Dtos;
using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Ocorrencias;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;
using BairroVivo.Domain.Entities;
using BairroVivo.Domain.Rules;

namespace BairroVivo.Application.UseCases
{
    public class OcorrenciaUseCase : IOcorrenciaUseCase
    {
        private readonly IOcorrenciaRepository _ocorrenciaRepository;

        public OcorrenciaUseCase(IOcorrenciaRepository ocorrenciaRepository)
        {
            _ocorrenciaRepository = ocorrenciaRepository;
        }

        public IEnumerable<OcorrenciaResponseDto> ObterTodas()
        {
            var ocorrencias = _ocorrenciaRepository.ObterTodas();
            return ocorrencias.Select(o => o.ToResponseDto());
        }

        public PageResponseDto<OcorrenciaResponseDto> ObterPaginado(
            int pagina, int tamanho,
            decimal? minLat, decimal? maxLat, decimal? minLon, decimal? maxLon,
            long? idCategoria, long? idMunicipio, string? status, string? ordem)
        {
            pagina = Math.Max(1, pagina);
            tamanho = Math.Clamp(tamanho, 1, 50);
            var deslocamento = (pagina - 1) * tamanho;

            if (!string.IsNullOrWhiteSpace(status))
                status = OcorrenciaRules.ValidarStatus(status);

            if (minLat.HasValue && maxLat.HasValue)
            {
                if (minLat.Value > maxLat.Value)
                    throw new ArgumentException("minLat não pode ser maior que maxLat.");
            }

            if (minLon.HasValue && maxLon.HasValue)
            {
                if (minLon.Value > maxLon.Value)
                    throw new ArgumentException("minLon não pode ser maior que maxLon.");
            }

            var (itens, total) = _ocorrenciaRepository
                .ObterPaginado(deslocamento, tamanho, minLat, maxLat, minLon, maxLon, idCategoria, idMunicipio, status, ordem);

            return new PageResponseDto<OcorrenciaResponseDto>
            {
                Deslocamento = deslocamento,
                RegistroRetornado = tamanho,
                TotalRegistros = total,
                Items = itens.Select(o => o.ToResponseDto()).ToList()
            };
        }

        public PageResponseDto<OcorrenciaResponseDto> ObterPorUsuarioPaginado(
            long idUsuario, int pagina, int tamanho, string? status, string? ordem)
        {
            if (!_ocorrenciaRepository.ExisteUsuario(idUsuario))
                throw new KeyNotFoundException("Usuário não encontrado ou inativo.");

            pagina = Math.Max(1, pagina);
            tamanho = Math.Clamp(tamanho, 1, 50);
            var deslocamento = (pagina - 1) * tamanho;

            if (!string.IsNullOrWhiteSpace(status))
                status = OcorrenciaRules.ValidarStatus(status);

            var (itens, total) = _ocorrenciaRepository
                .ObterPorUsuarioPaginado(idUsuario, deslocamento, tamanho, status, ordem);

            return new PageResponseDto<OcorrenciaResponseDto>
            {
                Deslocamento = deslocamento,
                RegistroRetornado = tamanho,
                TotalRegistros = total,
                Items = itens.Select(o => o.ToResponseDto()).ToList()
            };
        }

        public OcorrenciaResponseDto? ObterPorId(long idOcorrencia)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            return ocorrencia?.ToResponseDto();
        }

        public OcorrenciaResponseDto Cadastrar(long idUsuario, OcorrenciaRequestDto dto)
        {
            if (!_ocorrenciaRepository.ExisteUsuario(idUsuario))
                throw new KeyNotFoundException("Usuário não encontrado ou inativo.");

            if (!_ocorrenciaRepository.ExisteCategoria(dto.CategoriaId))
                throw new ArgumentException("Categoria inválida ou inativa.");

            if (!_ocorrenciaRepository.ExisteMunicipio(dto.MunicipioId))
                throw new ArgumentException("Município inválido ou inexistente.");

            dto.Titulo = OcorrenciaRules.ValidarTitulo(dto.Titulo);
            dto.Descricao = OcorrenciaRules.ValidarDescricao(dto.Descricao);
            dto.EnderecoReferencia = OcorrenciaRules.ValidarEnderecoReferencia(dto.EnderecoReferencia);
            OcorrenciaRules.ValidarCoordenadas(dto.Latitude, dto.Longitude);

            var entity = dto.ToEntity(idUsuario);
            var salva = _ocorrenciaRepository.Cadastrar(entity);

            _ocorrenciaRepository.RegistrarHistoricoStatus(new HistoricoStatusOcorrenciaEntity
            {
                IdOcorrencia = salva.IdOcorrencia,
                IdUsuario = idUsuario,
                Operacao = "INSERT",
                StatusAnterior = null,
                StatusNovo = "ABERTA",
                CicloAnterior = null,
                CicloNovo = 1,
                DtEvento = DateTime.UtcNow
            });

            var completa = _ocorrenciaRepository.ObterPorId(salva.IdOcorrencia);
            return completa!.ToResponseDto();
        }

        public bool Atualizar(long idOcorrencia, long idUsuario, OcorrenciaRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                return false;

            if (ocorrencia.IdUsuario != idUsuario)
                throw new UnauthorizedAccessException("Somente o autor da ocorrência pode editá-la.");

            if (!_ocorrenciaRepository.ExisteCategoria(dto.CategoriaId))
                throw new ArgumentException("Categoria inválida ou inativa.");

            if (!_ocorrenciaRepository.ExisteMunicipio(dto.MunicipioId))
                throw new ArgumentException("Município inválido ou inexistente.");

            dto.Titulo = OcorrenciaRules.ValidarTitulo(dto.Titulo);
            dto.Descricao = OcorrenciaRules.ValidarDescricao(dto.Descricao);
            dto.EnderecoReferencia = OcorrenciaRules.ValidarEnderecoReferencia(dto.EnderecoReferencia);
            OcorrenciaRules.ValidarCoordenadas(dto.Latitude, dto.Longitude);

            dto.UpdateEntity(ocorrencia);
            _ocorrenciaRepository.Atualizar(ocorrencia);
            return true;
        }

        public bool Remover(long idOcorrencia, long idUsuario)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                return false;

            if (ocorrencia.IdUsuario != idUsuario)
                throw new UnauthorizedAccessException("Somente o autor da ocorrência pode excluí-la.");

            _ocorrenciaRepository.Remover(ocorrencia);
            return true;
        }

        public IEnumerable<HistoricoStatusResponseDto> ObterHistorico(long idOcorrencia)
        {
            if (_ocorrenciaRepository.ObterPorId(idOcorrencia) == null)
                throw new KeyNotFoundException("Ocorrência não encontrada.");

            var lista = _ocorrenciaRepository.ObterHistoricoStatus(idOcorrencia);
            return lista.Select(h => new HistoricoStatusResponseDto
            {
                Id = h.IdHistorico.ToString(),
                IdOcorrencia = h.IdOcorrencia.ToString(),
                Operacao = h.Operacao,
                StatusAnterior = h.StatusAnterior,
                StatusNovo = h.StatusNovo,
                CicloAnterior = h.CicloAnterior,
                CicloNovo = h.CicloNovo,
                DtEvento = h.DtEvento
            });
        }

        public MidiaOcorrenciaResponseDto AdicionarMidia(long idOcorrencia, long idUsuario, MidiaOcorrenciaRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                throw new KeyNotFoundException("Ocorrência não encontrada.");

            if (ocorrencia.IdUsuario != idUsuario)
                throw new UnauthorizedAccessException("Somente o autor original da ocorrência pode anexar fotos diretamente a ela.");

            var chave = !string.IsNullOrWhiteSpace(dto.ChaveStorage) ? dto.ChaveStorage : dto.UrlExibicao;
            if (string.IsNullOrWhiteSpace(chave))
                throw new ArgumentException("A chave de storage ou URL da mídia é obrigatória.");

            var mime = (dto.MimeType ?? "image/jpeg").Trim().ToLowerInvariant();
            if (mime != "image/jpeg" && mime != "image/png" && mime != "image/webp")
                throw new ArgumentException("Formato de mídia não suportado. Utilize image/jpeg, image/png ou image/webp.");

            var salva = _ocorrenciaRepository.AdicionarMidia(new MidiaOcorrenciaEntity
            {
                IdOcorrencia = idOcorrencia,
                IdAtualizacao = null,
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
