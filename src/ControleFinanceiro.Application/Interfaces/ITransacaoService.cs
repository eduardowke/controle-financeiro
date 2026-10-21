using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface ITransacaoService
{
    Task<TransacaoResponseDto> CriarAsync(CriarTransacaoDto dto);
    Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync();
}