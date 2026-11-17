using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Services;

public class ContaService(AppDbContext context) : IContaService
{
    public async Task<ContaResponseDto> CriarAsync(CriarContaDto dto)
    {
        ValidarDadosConta(dto.Titular, dto.NumBanco, dto.TipoConta);

        var numBancoFormatado = dto.NumBanco.Trim().PadLeft(3, '0');
        var banco = await context.Bancos.FirstOrDefaultAsync(b => b.NumBanco == numBancoFormatado)
            ?? throw new ArgumentException($"Banco '{numBancoFormatado}' não encontrado.");

        var titularLimpo = dto.Titular.Trim();
        var primeiroNome = titularLimpo.Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
        var nomeConta = string.IsNullOrWhiteSpace(dto.Nome)
            ? $"{banco.NomeReduzido} - {primeiroNome}"
            : dto.Nome.Trim();

        var isCartao = dto.TipoConta == TipoConta.CartaoCredito;

        var conta = new Conta
        {
            Nome = nomeConta,
            NumBanco = banco.NumBanco,
            TipoConta = dto.TipoConta,
            Titular = titularLimpo,
            Agencia = isCartao ? null : dto.Agencia?.Trim(),
            NumConta = isCartao ? null : dto.NumConta?.Trim(),
            SaldoInicial = dto.SaldoInicial,
            Ativa = true
        };

        context.Contas.Add(conta);
        await context.SaveChangesAsync();

        conta.Banco = banco;
        return MapearParaDto(conta);
    }

    public async Task<IEnumerable<ContaResponseDto>> ObterTodasAsync(bool incluirInativas = false)
    {
        var query = context.Contas
            .Include(c => c.Banco)
            .AsNoTracking()
            .AsQueryable();

        if (!incluirInativas)
            query = query.Where(c => c.Ativa);

        var contas = await query.OrderBy(c => c.Nome).ToListAsync();
        return contas.Select(MapearParaDto);
    }

    public async Task<ContaResponseDto?> ObterPorIdAsync(int id)
    {
        var conta = await context.Contas
            .Include(c => c.Banco)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return conta is null ? null : MapearParaDto(conta);
    }

    public async Task<ContaResponseDto?> AtualizarAsync(int id, AtualizarContaDto dto)
    {
        var conta = await context.Contas.FirstOrDefaultAsync(c => c.Id == id);
        if (conta is null) return null;

        ValidarDadosConta(dto.Titular, dto.NumBanco, dto.TipoConta);

        var numBancoFormatado = dto.NumBanco.Trim().PadLeft(3, '0');
        var banco = await context.Bancos.FirstOrDefaultAsync(b => b.NumBanco == numBancoFormatado)
            ?? throw new ArgumentException($"Banco '{numBancoFormatado}' não encontrado.");

        var isCartao = dto.TipoConta == TipoConta.CartaoCredito;

        conta.Nome = string.IsNullOrWhiteSpace(dto.Nome) ? conta.Nome : dto.Nome.Trim();
        conta.NumBanco = banco.NumBanco;
        conta.TipoConta = dto.TipoConta;
        conta.Titular = dto.Titular.Trim();
        conta.Agencia = isCartao ? null : dto.Agencia?.Trim();
        conta.NumConta = isCartao ? null : dto.NumConta?.Trim();
        conta.SaldoInicial = dto.SaldoInicial;
        conta.Ativa = dto.Ativa;

        await context.SaveChangesAsync();

        conta.Banco = banco;
        return MapearParaDto(conta);
    }

    public async Task<bool> AlterarStatusAsync(int id, bool ativa)
    {
        var conta = await context.Contas.FirstOrDefaultAsync(c => c.Id == id);
        if (conta is null) return false;

        conta.Ativa = ativa;
        await context.SaveChangesAsync();
        return true;
    }

    private static void ValidarDadosConta(string titular, string numBanco, TipoConta tipoConta)
    {
        if (string.IsNullOrWhiteSpace(titular) || titular.Trim().Length < 3)
            throw new ArgumentException("O nome do titular é obrigatório e deve ter no mínimo 3 caracteres.");

        if (string.IsNullOrWhiteSpace(numBanco))
            throw new ArgumentException("O número do banco é obrigatório.");

        if (!Enum.IsDefined(typeof(TipoConta), tipoConta))
            throw new ArgumentException("Tipo de conta inválido.");
    }

    private static ContaResponseDto MapearParaDto(Conta c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        NumBanco = c.NumBanco,
        NomeBanco = c.Banco?.NomeReduzido ?? "Não informado",
        LogoBancoUrl = c.Banco?.LogoUrl,
        TipoConta = c.TipoConta,
        TipoContaDescricao = c.TipoConta switch
        {
            TipoConta.ContaCorrente => "Conta Corrente",
            TipoConta.ContaInvestimento => "Conta Investimento",
            TipoConta.CartaoCredito => "Cartão de Crédito",
            _ => "Outros"
        },
        Titular = c.Titular,
        Agencia = c.Agencia,
        NumConta = c.NumConta,
        SaldoInicial = c.SaldoInicial,
        Ativa = c.Ativa
    };
}