using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface ITransacaoService
{
    Task<TransacaoResponseDto> CriarTransacaoAsync(CriarTransacaoDto dto);
    Task<TransacaoResponseDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync();
    Task<ExtratoContaDto?> ObterExtratoPorContaAsync(int contaId);
    Task<TransacaoResponseDto?> AtualizarTransacaoAsync(int id, AtualizarTransacaoDto dto);
    Task<bool> ExcluirTransacaoAsync(int id);
}