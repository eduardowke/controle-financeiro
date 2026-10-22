using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.Interfaces;

public interface ITransacaoRepository
{
    Task<Transacao> CriarAsync(Transacao transacao);
    Task<IEnumerable<Transacao>> ObterTodasAsync();
    Task<Transacao?> ObterPorIdAsync(int id);
}