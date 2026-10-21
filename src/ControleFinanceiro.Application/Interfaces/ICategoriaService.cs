using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface ICategoriaService
{
    Task<CategoriaResponseDto> CriarAsync(CriarCategoriaDto dto);
    Task<IEnumerable<CategoriaResponseDto>> ObterTodasAsync();
}