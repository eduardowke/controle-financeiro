using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface IContaService
{
    Task<ContaResponseDto> CriarAsync(CriarContaDto dto);
    Task<IEnumerable<ContaResponseDto>> ObterTodasAsync();
}