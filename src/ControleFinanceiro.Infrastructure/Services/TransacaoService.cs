using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class TransacaoService(AppDbContext context) : ITransacaoService
{
    public async Task<TransacaoResponseDto> CriarTransacaoAsync(CriarTransacaoDto dto)
    {
        var transacao = new Transacao
        {
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            Tipo = dto.Tipo,
            Data = dto.Data == default ? DateTime.UtcNow : dto.Data,
            IsPago = dto.IsPago,
            ContaId = dto.ContaId,
            CategoriaId = dto.CategoriaId,
            ConfiancaIA = dto.ConfiancaIA
        };

        context.Transacoes.Add(transacao);
        await context.SaveChangesAsync();

        return new TransacaoResponseDto
        {
            Id = transacao.Id,
            Descricao = transacao.Descricao,
            Valor = transacao.Valor,
            Tipo = transacao.Tipo,
            Data = transacao.Data,
            IsPago = transacao.IsPago,
            ContaId = transacao.ContaId,
            CategoriaId = transacao.CategoriaId,
            ConfiancaIA = transacao.ConfiancaIA
        };
    }

    public async Task<TransacaoResponseDto?> ObterPorIdAsync(int id)
    {
        var transacao = await context.Transacoes.FirstOrDefaultAsync(t => t.Id == id);
        if (transacao == null) return null;

        return new TransacaoResponseDto
        {
            Id = transacao.Id,
            Descricao = transacao.Descricao,
            Valor = transacao.Valor,
            Tipo = transacao.Tipo,
            Data = transacao.Data,
            IsPago = transacao.IsPago,
            ContaId = transacao.ContaId,
            CategoriaId = transacao.CategoriaId,
            ConfiancaIA = transacao.ConfiancaIA
        };
    }

    public async Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync()
    {
        return await context.Transacoes
            .Select(t => new TransacaoResponseDto
            {
                Id = t.Id,
                Descricao = t.Descricao,
                Valor = t.Valor,
                Tipo = t.Tipo,
                Data = t.Data,
                IsPago = t.IsPago,
                ContaId = t.ContaId,
                CategoriaId = t.CategoriaId,
                ConfiancaIA = t.ConfiancaIA
            })
            .ToListAsync();
    }
}