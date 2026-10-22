using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface ITransacaoService
{
    Task<TransacaoResponseDto> CriarTransacaoAsync(CriarTransacaoDto dto);
    Task<TransacaoResponseDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync();
}