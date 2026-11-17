using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class BancoService(AppDbContext context) : IBancoService
{
    public async Task<IEnumerable<BancoResponseDto>> ObterTodosAsync(string? filtro = null)
    {
        var query = context.Bancos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(b =>
                b.NumBanco.Contains(termo) ||
                b.NomeReduzido.Contains(termo) ||
                b.RazaoSocial.Contains(termo));
        }

        return await query
            .OrderBy(b => b.NomeReduzido)
            .Select(b => new BancoResponseDto
            {
                NumBanco = b.NumBanco,
                NomeReduzido = b.NomeReduzido,
                RazaoSocial = b.RazaoSocial,
                LogoUrl = b.LogoUrl
            })
            .ToListAsync();
    }

    public async Task<BancoResponseDto?> ObterPorNumeroAsync(string numBanco)
    {
        var banco = await context.Bancos
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.NumBanco == numBanco.PadLeft(3, '0'));

        if (banco is null) return null;

        return new BancoResponseDto
        {
            NumBanco = banco.NumBanco,
            NomeReduzido = banco.NomeReduzido,
            RazaoSocial = banco.RazaoSocial,
            LogoUrl = banco.LogoUrl
        };
    }
}