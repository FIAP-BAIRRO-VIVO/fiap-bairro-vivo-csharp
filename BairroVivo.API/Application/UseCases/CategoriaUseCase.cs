using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Application.Dtos.Categorias;
using BairroVivo.Application.Interfaces;
using BairroVivo.Application.Mappers;

namespace BairroVivo.Application.UseCases
{
    public class CategoriaUseCase : ICategoriaUseCase
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaUseCase(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public IEnumerable<CategoriaResponseDto> ObterTodas() =>
            _categoriaRepository.ObterTodas().Select(c => c.ToResponseDto());

        public CategoriaResponseDto? ObterPorId(decimal idCategoria) =>
            _categoriaRepository.ObterPorId(idCategoria)?.ToResponseDto();

        public CategoriaResponseDto Cadastrar(CategoriaRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new ArgumentException("O nome da categoria é obrigatório.");

            var salva = _categoriaRepository.Cadastrar(dto.ToEntity());
            return salva.ToResponseDto();
        }

        public bool Atualizar(decimal idCategoria, CategoriaRequestDto dto)
        {
            var categoria = _categoriaRepository.ObterPorId(idCategoria);
            if (categoria == null)
                return false;

            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new ArgumentException("O nome da categoria é obrigatório.");

            dto.UpdateEntity(categoria);
            _categoriaRepository.Atualizar(categoria);
            return true;
        }

        public bool Remover(decimal idCategoria)
        {
            var categoria = _categoriaRepository.ObterPorId(idCategoria);
            if (categoria == null)
                return false;

            _categoriaRepository.Remover(categoria);
            return true;
        }
    }
}
