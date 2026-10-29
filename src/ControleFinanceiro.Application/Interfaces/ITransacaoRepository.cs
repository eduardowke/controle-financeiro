using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.Interfaces; // ou ControleFinanceiro.Domain.Interfaces

public interface ITransacaoRepository
{
    Task<IEnumerable<Transacao>> ObterTodasAsync();
    Task<Transacao?> ObterPorIdAsync(int id);
    Task<Transacao> CriarAsync(Transacao transacao);
    
    // Método necessário para a RAG (Retrieval)
    Task<IEnumerable<Transacao>> ObterPorPeriodoEContaAsync(
        int? contaId, 
        DateTime? dataInicio, 
        DateTime? dataFim, 
        CancellationToken cancellationToken = default);
}