using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface IAICategorizationService
{
    Task<AnaliseGastoResponseDto> CategorizarGastoAsync(AnaliseGastoRequestDto dto);
}