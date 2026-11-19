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
   public async Task<DashboardContasDto> ObterResumoDashboardAsync(DateTime? dataCorte = null)
    {
        var corte = (dataCorte ?? DateTime.Today).Date;
        var inicioMes = new DateTime(corte.Year, corte.Month, 1);

        var contas = await context.Contas
            .Include(c => c.Banco)
            .Where(c => c.Ativa)
            .OrderBy(c => c.Nome)
            .AsNoTracking()
            .ToListAsync();

        var transacoes = await context.Transacoes
            .Include(t => t.Categoria)
            .AsNoTracking()
            .ToListAsync();

        var dashboard = new DashboardContasDto { DataCorte = corte };

        foreach (var conta in contas)
        {
            var transacoesConta = transacoes.Where(t => t.ContaId == conta.Id).ToList();
            var isCartao = conta.TipoConta == TipoConta.CartaoCredito;

            // 1. Compensadas (DataCompensacao != null) -> compõem o Saldo/Fatura Atual
            var compensadas = transacoesConta.Where(t => t.DataCompensacao.HasValue).ToList();

            // 2. Pendentes (DataCompensacao == null) com Data prevista até a DataCorte -> compõem a Projeção
            var pendentesAteCorte = transacoesConta
                .Where(t => !t.DataCompensacao.HasValue && t.Data.Date <= corte)
                .ToList();

            decimal saldoAtual;
            decimal saldoProjetado;

            if (!isCartao)
            {
                // Conta Corrente / Investimento: Receita soma, Despesa subtrai
                var movCompensada = compensadas.Sum(t => t.Tipo == TipoTransacao.Receita ? t.Valor : -t.Valor);
                var movPendente = pendentesAteCorte.Sum(t => t.Tipo == TipoTransacao.Receita ? t.Valor : -t.Valor);

                saldoAtual = conta.SaldoInicial + movCompensada;
                saldoProjetado = saldoAtual + movPendente;
            }
            else
            {
                // Cartão de Crédito: Despesa aumenta a fatura, Receita/Pagamento de Fatura reduz a fatura
                var movCompensada = compensadas.Sum(t => t.Tipo == TipoTransacao.Despesa ? t.Valor : -t.Valor);
                var movPendente = pendentesAteCorte.Sum(t => t.Tipo == TipoTransacao.Despesa ? t.Valor : -t.Valor);

                saldoAtual = conta.SaldoInicial + movCompensada;
                saldoProjetado = saldoAtual + movPendente;
            }

            var itemDto = new ContaSaldoDto
            {
                Id = conta.Id,
                Nome = conta.Nome,
                NumBanco = conta.NumBanco,
                NomeBanco = conta.Banco?.NomeReduzido ?? "Não informado",
                TipoConta = conta.TipoConta,
                TipoContaDescricao = conta.TipoConta switch
                {
                    TipoConta.ContaCorrente => "Conta Corrente",
                    TipoConta.ContaInvestimento => "Conta Investimento",
                    TipoConta.CartaoCredito => "Cartão de Crédito",
                    _ => "Outros"
                },
                Titular = conta.Titular,
                Agencia = conta.Agencia,
                NumConta = conta.NumConta,
                Ativa = conta.Ativa,
                SaldoInicial = conta.SaldoInicial,
                SaldoAtual = saldoAtual,
                SaldoProjetado = saldoProjetado,
                LancamentosPendentesAteCorte = pendentesAteCorte.Count
            };

            if (isCartao)
                dashboard.CartoesCredito.Add(itemDto);
            else
                dashboard.ContasBancarias.Add(itemDto);
        }

        dashboard.LiquidezAtual = dashboard.ContasBancarias.Sum(c => c.SaldoAtual);
        dashboard.LiquidezProjetada = dashboard.ContasBancarias.Sum(c => c.SaldoProjetado);

        dashboard.FaturaCartoesAtual = dashboard.CartoesCredito.Sum(c => c.SaldoAtual);
        dashboard.FaturaCartoesProjetada = dashboard.CartoesCredito.Sum(c => c.SaldoProjetado);

        // Indicadores globais do mês da data de corte (ignorando Categorias Neutras = 3)
        var transacoesNaoNeutras = transacoes
            .Where(t => t.Categoria == null || t.Categoria.TipoCategoria != TipoCategoria.Neutra)
            .ToList();

        dashboard.ReceitasRealizadasPeriodo = transacoesNaoNeutras
            .Where(t => t.DataCompensacao.HasValue
                     && t.DataCompensacao.Value.Date >= inicioMes
                     && t.DataCompensacao.Value.Date <= corte
                     && t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor);

        dashboard.DespesasRealizadasPeriodo = transacoesNaoNeutras
            .Where(t => t.DataCompensacao.HasValue
                     && t.DataCompensacao.Value.Date >= inicioMes
                     && t.DataCompensacao.Value.Date <= corte
                     && t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor);

        dashboard.ReceitasPrevistasPeriodo = transacoesNaoNeutras
            .Where(t => !t.DataCompensacao.HasValue
                     && t.Data.Date <= corte
                     && t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor);

        dashboard.DespesasPrevistasPeriodo = transacoesNaoNeutras
            .Where(t => !t.DataCompensacao.HasValue
                     && t.Data.Date <= corte
                     && t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor);

        return dashboard;
    } 
}