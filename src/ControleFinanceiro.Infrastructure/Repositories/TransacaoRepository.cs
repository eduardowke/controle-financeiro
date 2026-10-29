using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class TransacaoRepository(AppDbContext context) : ITransacaoRepository
{
    public async Task<IEnumerable<Transacao>> ObterTodasAsync()
    {
        return await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Transacao?> ObterPorIdAsync(int id)
    {
        return await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transacao> CriarAsync(Transacao transacao)
    {
        await context.Transacoes.AddAsync(transacao);
        await context.SaveChangesAsync();
        return transacao;
    }

    public async Task<IEnumerable<Transacao>> ObterPorPeriodoEContaAsync(
        int? contaId, 
        DateTime? dataInicio, 
        DateTime? dataFim, 
        CancellationToken cancellationToken = default)
    {
        var query = context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .AsNoTracking()
            .AsQueryable();

        if (contaId.HasValue && contaId.Value > 0)
            query = query.Where(t => t.ContaId == contaId.Value);

        if (dataInicio.HasValue)
            query = query.Where(t => t.Data >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(t => t.Data <= dataFim.Value);

        return await query
            .OrderByDescending(t => t.Data)
            .ToListAsync(cancellationToken);
    }
}