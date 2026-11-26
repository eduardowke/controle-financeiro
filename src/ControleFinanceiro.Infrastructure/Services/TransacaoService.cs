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
        if (string.IsNullOrWhiteSpace(dto.Descricao))
            throw new ArgumentException("A descrição da transação é obrigatória.");

        if (dto.Valor <= 0)
            throw new ArgumentException("O valor da transação deve ser maior que zero.");

        var conta = await context.Contas.FirstOrDefaultAsync(c => c.Id == dto.ContaId)
            ?? throw new ArgumentException("Conta não encontrada.");

        var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == dto.CategoriaId)
            ?? throw new ArgumentException("Categoria não encontrada.");

        var dataPrevista = dto.Data == default ? DateTime.Today : dto.Data.Date;

        // Sincroniza DataCompensacao e IsPago
        DateTime? dataCompensacao = dto.DataCompensacao?.Date;
        if (!dataCompensacao.HasValue && dto.IsPago)
            dataCompensacao = dataPrevista;

        var isPago = dataCompensacao.HasValue;

        // Se compensado, calcula a próxima ordem do dia naquela conta automaticamente
        int ordem = dto.Ordem ?? 0;
        if (dataCompensacao.HasValue && ordem == 0)
        {
            var maxOrdemDia = await context.Transacoes
                .Where(t => t.ContaId == dto.ContaId && t.DataCompensacao == dataCompensacao.Value)
                .Select(t => (int?)t.Ordem)
                .MaxAsync() ?? 0;

            ordem = maxOrdemDia + 1;
        }

        var transacao = new Transacao
        {
            Descricao = dto.Descricao.Trim(),
            Valor = dto.Valor,
            Tipo = dto.Tipo,
            Data = dataPrevista,
            DataCompensacao = dataCompensacao,
            Ordem = ordem,
            IsPago = isPago,
            ContaId = dto.ContaId,
            CategoriaId = dto.CategoriaId,
            ConfiancaIA = dto.ConfiancaIA
        };

        context.Transacoes.Add(transacao);
        await context.SaveChangesAsync();

        transacao.Conta = conta;
        transacao.Categoria = categoria;

        return MapearParaDto(transacao);
    }

    public async Task<TransacaoResponseDto?> ObterPorIdAsync(int id)
    {
        var transacao = await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return transacao is null ? null : MapearParaDto(transacao);
    }

    public async Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync()
    {
        var lista = await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .OrderByDescending(t => t.DataCompensacao ?? t.Data)
            .ThenByDescending(t => t.Id)
            .AsNoTracking()
            .ToListAsync();

        return lista.Select(t => MapearParaDto(t));
    }

    public async Task<ExtratoContaDto?> ObterExtratoPorContaAsync(int contaId)
    {
        var conta = await context.Contas
            .Include(c => c.Banco)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contaId);

        if (conta is null) return null;

        var transacoes = await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .Where(t => t.ContaId == contaId)
            .AsNoTracking()
            .ToListAsync();

        var isCartao = conta.TipoConta == TipoConta.CartaoCredito;

        // 1. Itens Compensados: ordenados por DataCompensacao, Ordem no extrato e Id
        var compensadosOrdenados = transacoes
            .Where(t => t.DataCompensacao.HasValue)
            .OrderBy(t => t.DataCompensacao!.Value.Date)
            .ThenBy(t => t.Ordem)
            .ThenBy(t => t.Id)
            .ToList();

        decimal saldoAcumulado = conta.SaldoInicial;
        var compensadosDto = new List<TransacaoResponseDto>();

        foreach (var t in compensadosOrdenados)
        {
            if (!isCartao)
                saldoAcumulado += t.Tipo == TipoTransacao.Receita ? t.Valor : -t.Valor;
            else
                saldoAcumulado += t.Tipo == TipoTransacao.Despesa ? t.Valor : -t.Valor;

            var dto = MapearParaDto(t);
            dto.SaldoAposLancamento = saldoAcumulado;
            compensadosDto.Add(dto);
        }

        // 2. Itens Futuros/Pendentes: ordenados por Data Prevista
        var futurosOrdenados = transacoes
            .Where(t => !t.DataCompensacao.HasValue)
            .OrderBy(t => t.Data.Date)
            .ThenBy(t => t.Id)
            .ToList();

        decimal saldoProjetado = saldoAcumulado;
        var futurosDto = new List<TransacaoResponseDto>();

        foreach (var t in futurosOrdenados)
        {
            if (!isCartao)
                saldoProjetado += t.Tipo == TipoTransacao.Receita ? t.Valor : -t.Valor;
            else
                saldoProjetado += t.Tipo == TipoTransacao.Despesa ? t.Valor : -t.Valor;

            var dto = MapearParaDto(t);
            dto.SaldoAposLancamento = saldoProjetado;
            futurosDto.Add(dto);
        }

        return new ExtratoContaDto
        {
            Conta = new ContaResponseDto
            {
                Id = conta.Id,
                Nome = conta.Nome,
                NumBanco = conta.NumBanco,
                NomeBanco = conta.Banco?.NomeReduzido ?? "Não informado",
                LogoBancoUrl = conta.Banco?.LogoUrl,
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
                SaldoInicial = conta.SaldoInicial,
                Ativa = conta.Ativa
            },
            SaldoInicial = conta.SaldoInicial,
            SaldoAtual = saldoAcumulado,
            SaldoProjetado = saldoProjetado,
            Compensados = compensadosDto,
            Futuros = futurosDto
        };
    }

    public async Task<TransacaoResponseDto?> AtualizarTransacaoAsync(int id, AtualizarTransacaoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao))
            throw new ArgumentException("A descrição da transação é obrigatória.");

        if (dto.Valor <= 0)
            throw new ArgumentException("O valor da transação deve ser maior que zero.");

        var transacao = await context.Transacoes
            .Include(t => t.Conta)
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transacao is null) return null;

        var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == dto.CategoriaId)
            ?? throw new ArgumentException("Categoria não encontrada.");

        var dataPrevista = dto.Data == default ? transacao.Data.Date : dto.Data.Date;
        DateTime? dataCompensacao = dto.DataCompensacao?.Date;
        if (!dataCompensacao.HasValue && dto.IsPago)
            dataCompensacao = dataPrevista;

        // Se mudou de não-compensado para compensado (ou mudou o dia de compensação), calcula a ordem no dia
        if (dataCompensacao.HasValue && transacao.DataCompensacao?.Date != dataCompensacao.Value)
        {
            var maxOrdemDia = await context.Transacoes
                .Where(t => t.ContaId == transacao.ContaId
                            && t.Id != transacao.Id
                            && t.DataCompensacao == dataCompensacao.Value)
                .Select(t => (int?)t.Ordem)
                .MaxAsync() ?? 0;

            transacao.Ordem = maxOrdemDia + 1;
        }
        else if (!dataCompensacao.HasValue)
        {
            transacao.Ordem = 0;
        }

        transacao.Descricao = dto.Descricao.Trim();
        transacao.Valor = dto.Valor;
        transacao.Tipo = dto.Tipo;
        transacao.Data = dataPrevista;
        transacao.DataCompensacao = dataCompensacao;
        transacao.IsPago = dataCompensacao.HasValue;
        transacao.CategoriaId = dto.CategoriaId;
        transacao.Categoria = categoria;

        await context.SaveChangesAsync();
        return MapearParaDto(transacao);
    }

    public async Task<bool> ExcluirTransacaoAsync(int id)
    {
        var transacao = await context.Transacoes.FirstOrDefaultAsync(t => t.Id == id);
        if (transacao is null) return false;

        context.Transacoes.Remove(transacao);
        await context.SaveChangesAsync();
        return true;
    }

    private static TransacaoResponseDto MapearParaDto(Transacao t) => new()
    {
        Id = t.Id,
        Descricao = t.Descricao,
        Valor = t.Valor,
        Tipo = t.Tipo,
        Data = t.Data,
        DataCompensacao = t.DataCompensacao,
        Ordem = t.Ordem,
        IsPago = t.IsPago,
        ContaId = t.ContaId,
        NomeConta = t.Conta?.Nome ?? string.Empty,
        CategoriaId = t.CategoriaId,
        NomeCategoria = t.Categoria?.Nome ?? string.Empty,
        TransferenciaGrupoId = t.TransferenciaGrupoId,
        ConfiancaIA = t.ConfiancaIA
    };
}