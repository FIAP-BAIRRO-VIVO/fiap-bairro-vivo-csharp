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
            int pagina, int tamanho, string? status, string? urgencia,
            string? bairro, decimal? idCategoria, decimal? idMunicipio, string? ordem)
        {
            pagina = Math.Max(1, pagina);
            tamanho = Math.Clamp(tamanho, 1, 50);
            var deslocamento = (pagina - 1) * tamanho;
            var (itens, total) = _ocorrenciaRepository
                .ObterPaginado(deslocamento, tamanho, status, urgencia, bairro, idCategoria, idMunicipio, ordem);

            return new PageResponseDto<OcorrenciaResponseDto>
            {
                Deslocamento = deslocamento,
                RegistroRetornado = tamanho,
                TotalRegistros = total,
                Items = itens.Select(o => o.ToResponseDto()).ToList()
            };
        }

        public PageResponseDto<OcorrenciaResponseDto> ObterPorUsuarioPaginado(
            decimal idUsuario, int pagina, int tamanho, string? status, string? ordem)
        {
            if (!_ocorrenciaRepository.ExisteUsuario(idUsuario))
                throw new KeyNotFoundException("Usuário não encontrado.");

            pagina = Math.Max(1, pagina);
            tamanho = Math.Clamp(tamanho, 1, 50);
            var deslocamento = (pagina - 1) * tamanho;
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

        public OcorrenciaResponseDto? ObterPorId(decimal idOcorrencia)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                return null;

            return ocorrencia.ToResponseDto();
        }

        public OcorrenciaResponseDto Cadastrar(decimal idUsuario, OcorrenciaRequestDto dto)
        {
            if (!_ocorrenciaRepository.ExisteUsuario(idUsuario))
                throw new KeyNotFoundException("Usuário não encontrado.");

            var categoria = _ocorrenciaRepository.ObterCategoria(dto.IdCategoria);
            if (categoria == null)
                throw new ArgumentException("Categoria inválida ou inexistente.");

            if (!_ocorrenciaRepository.ExisteMunicipio(dto.IdMunicipio))
                throw new ArgumentException("Município inválido ou inexistente.");

            dto.Titulo = OcorrenciaRules.ValidarTitulo(dto.Titulo);
            dto.Descricao = OcorrenciaRules.ValidarDescricao(dto.Descricao);
            dto.Bairro = OcorrenciaRules.ValidarBairro(dto.Bairro);
            dto.Urgencia = OcorrenciaRules.ValidarUrgencia(dto.Urgencia);
            OcorrenciaRules.ValidarCoordenadas(dto.Latitude, dto.Longitude);

            var sequencial = _ocorrenciaRepository.ObterProximoSequencialProtocolo();
            var protocolo = OcorrenciaRules.GerarProtocolo(DateTime.Now, sequencial);

            var entity = dto.ToEntity(idUsuario, protocolo, categoria.SlaDiasPadrao);
            var salva = _ocorrenciaRepository.Cadastrar(entity);

            _ocorrenciaRepository.RegistrarHistoricoStatus(new HistoricoStatusOcorrenciaEntity
            {
                IdOcorrencia = salva.IdOcorrencia,
                IdUsuarioResponsavel = idUsuario,
                StatusAnterior = null,
                StatusNovo = "ABERTA",
                Observacao = "Abertura inicial da ocorrência urbana.",
                AlteradoEm = DateTime.Now
            });

            var completa = _ocorrenciaRepository.ObterPorId(salva.IdOcorrencia);
            return completa!.ToResponseDto();
        }

        public bool Atualizar(decimal idOcorrencia, OcorrenciaRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                return false;

            if (!_ocorrenciaRepository.ExisteCategoria(dto.IdCategoria))
                throw new ArgumentException("Categoria inválida ou inexistente.");

            if (!_ocorrenciaRepository.ExisteMunicipio(dto.IdMunicipio))
                throw new ArgumentException("Município inválido ou inexistente.");

            dto.Titulo = OcorrenciaRules.ValidarTitulo(dto.Titulo);
            dto.Descricao = OcorrenciaRules.ValidarDescricao(dto.Descricao);
            dto.Bairro = OcorrenciaRules.ValidarBairro(dto.Bairro);
            dto.Urgencia = OcorrenciaRules.ValidarUrgencia(dto.Urgencia);
            OcorrenciaRules.ValidarCoordenadas(dto.Latitude, dto.Longitude);

            dto.UpdateEntity(ocorrencia);
            _ocorrenciaRepository.Atualizar(ocorrencia);
            return true;
        }

        public bool AtualizarStatus(decimal idOcorrencia, decimal? idUsuarioResponsavel, OcorrenciaStatusRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                return false;

            var novoStatus = OcorrenciaRules.ValidarStatus(dto.Status);
            var statusAnterior = ocorrencia.Status;

            ocorrencia.Status = novoStatus;
            ocorrencia.AtualizadaEm = DateTime.Now;
            if (novoStatus == "RESOLVIDA")
                ocorrencia.ResolvidaEm = DateTime.Now;

            _ocorrenciaRepository.Atualizar(ocorrencia);
            _ocorrenciaRepository.RegistrarHistoricoStatus(new HistoricoStatusOcorrenciaEntity
            {
                IdOcorrencia = ocorrencia.IdOcorrencia,
                IdUsuarioResponsavel = idUsuarioResponsavel,
                StatusAnterior = statusAnterior,
                StatusNovo = novoStatus,
                Observacao = dto.Observacao?.Trim(),
                AlteradoEm = DateTime.Now
            });

            return true;
        }

        public bool Remover(decimal idOcorrencia)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                return false;

            _ocorrenciaRepository.Remover(ocorrencia);
            return true;
        }

        public MidiaOcorrenciaResponseDto AdicionarMidia(decimal idOcorrencia, MidiaOcorrenciaRequestDto dto)
        {
            var ocorrencia = _ocorrenciaRepository.ObterPorId(idOcorrencia);
            if (ocorrencia == null)
                throw new KeyNotFoundException("Ocorrência não encontrada.");

            if (string.IsNullOrWhiteSpace(dto.UrlMidia))
                throw new ArgumentException("A URL da mídia é obrigatória.");

            var tipo = (dto.TipoMidia ?? "FOTO").Trim().ToUpperInvariant();
            if (tipo != "FOTO" && tipo != "VIDEO")
                throw new ArgumentException("Tipo de mídia inválido. Utilize FOTO ou VIDEO.");

            var principal = (dto.Principal ?? "N").Trim().ToUpperInvariant() == "S" ? "S" : "N";

            var salva = _ocorrenciaRepository.AdicionarMidia(new MidiaOcorrenciaEntity
            {
                IdOcorrencia = idOcorrencia,
                UrlMidia = dto.UrlMidia.Trim(),
                TipoMidia = tipo,
                Principal = principal,
                CriadaEm = DateTime.Now
            });

            return salva.ToResponseDto();
        }
    }
}
