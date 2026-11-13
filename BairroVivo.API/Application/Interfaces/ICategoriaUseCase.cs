using BairroVivo.Application.Dtos.Categorias;

namespace BairroVivo.Application.Interfaces
{
    public interface ICategoriaUseCase
    {
        IEnumerable<CategoriaResponseDto> ObterTodas();
        CategoriaResponseDto? ObterPorId(decimal idCategoria);
        CategoriaResponseDto Cadastrar(CategoriaRequestDto dto);
        bool Atualizar(decimal idCategoria, CategoriaRequestDto dto);
        bool Remover(decimal idCategoria);
    }
}
