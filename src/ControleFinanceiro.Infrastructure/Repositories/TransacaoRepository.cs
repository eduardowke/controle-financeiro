using ControleFinanceiro.Domain;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class TransacaoRepository(AppDbContext context) : ITransacaoRepository
{
    public async Task<Transacao> CriarAsync(Transacao transacao)
    {
        await context.Transacoes.AddAsync(transacao);
        await context.SaveChangesAsync();
        return transacao;
    }

    public async Task<IEnumerable<Transacao>> ObterTodasAsync()
    {
        return await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .ToListAsync();
    }

    public async Task<Transacao?> ObterPorIdAsync(int id)
    {
        return await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}