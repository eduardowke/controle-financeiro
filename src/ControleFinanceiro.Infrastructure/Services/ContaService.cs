using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class ContaService : IContaService
{
    private readonly AppDbContext _context;

    public ContaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ContaResponseDto> CriarAsync(CriarContaDto dto)
    {
        var conta = new Conta
        {
            Nome = dto.Nome,
            SaldoInicial = dto.SaldoInicial
        };

        _context.Contas.Add(conta);
        await _context.SaveChangesAsync();

        return new ContaResponseDto
        {
            Id = conta.Id,
            Nome = conta.Nome,
            SaldoInicial = conta.SaldoInicial
        };
    }

    public async Task<IEnumerable<ContaResponseDto>> ObterTodasAsync()
    {
        return await _context.Contas
            .Select(c => new ContaResponseDto
            {
                Id = c.Id,
                Nome = c.Nome,
                SaldoInicial = c.SaldoInicial
            })
            .ToListAsync();
    }
}