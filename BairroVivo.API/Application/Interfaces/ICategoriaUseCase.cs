using BairroVivo.Application.Dtos.Categorias;

namespace BairroVivo.Application.Interfaces
{
    public interface ICategoriaUseCase
    {
        IEnumerable<CategoriaResponseDto> ObterTodas();
        CategoriaResponseDto? ObterPorId(long idCategoria);
        CategoriaResponseDto Cadastrar(CategoriaRequestDto dto);
        bool Atualizar(long idCategoria, CategoriaRequestDto dto);
        bool Remover(long idCategoria);
    }
}
