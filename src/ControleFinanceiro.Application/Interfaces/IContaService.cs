using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface IContaService
{
    Task<ContaResponseDto> CriarAsync(CriarContaDto dto);
    Task<IEnumerable<ContaResponseDto>> ObterTodasAsync(bool incluirInativas = false);
    Task<ContaResponseDto?> ObterPorIdAsync(int id);
    Task<ContaResponseDto?> AtualizarAsync(int id, AtualizarContaDto dto);
    Task<bool> AlterarStatusAsync(int id, bool ativa);
    Task<DashboardContasDto> ObterResumoDashboardAsync(DateTime? dataCorte = null);
}
