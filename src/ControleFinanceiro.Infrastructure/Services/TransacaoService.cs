using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class TransacaoService : ITransacaoService
{
    private readonly AppDbContext _context;

    public TransacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TransacaoResponseDto> CriarAsync(CriarTransacaoDto dto)
    {
        var transacao = new Transacao
        {
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            Tipo = dto.Tipo,
            Data = DateTime.UtcNow,
            ContaId = dto.ContaId,
            CategoriaId = dto.CategoriaId
        };

        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        return new TransacaoResponseDto
        {
            Id = transacao.Id,
            Descricao = transacao.Descricao,
            Valor = transacao.Valor,
            Tipo = transacao.Tipo,
            Data = transacao.Data,
            ContaId = transacao.ContaId,
            CategoriaId = transacao.CategoriaId
        };
    }

    public async Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync()
    {
        return await _context.Transacoes
            .Select(t => new TransacaoResponseDto
            {
                Id = t.Id,
                Descricao = t.Descricao,
                Valor = t.Valor,
                Tipo = t.Tipo,
                Data = t.Data,
                ContaId = t.ContaId,
                CategoriaId = t.CategoriaId
            })
            .ToListAsync();
    }
}