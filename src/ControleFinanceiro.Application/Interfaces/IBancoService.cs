using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface IBancoService
{
    Task<IEnumerable<BancoResponseDto>> ObterTodosAsync(string? filtro = null);
    Task<BancoResponseDto?> ObterPorNumeroAsync(string numBanco);
}