using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.DTOs;

public class DashboardContasDto
{
    public DateTime DataCorte { get; set; }

    // Totais Consolidados de Liquidez (Conta Corrente + Investimento)
    public decimal LiquidezAtual { get; set; }
    public decimal LiquidezProjetada { get; set; }

    // Totais Consolidados de Cartões de Crédito (Separados da Liquidez)
    public decimal FaturaCartoesAtual { get; set; }
    public decimal FaturaCartoesProjetada { get; set; }

    // Indicadores Globais de Resultado (Ignora Categorias Neutras)
    public decimal ReceitasRealizadasPeriodo { get; set; }
    public decimal DespesasRealizadasPeriodo { get; set; }
    public decimal ReceitasPrevistasPeriodo { get; set; }
    public decimal DespesasPrevistasPeriodo { get; set; }

    public List<ContaSaldoDto> ContasBancarias { get; set; } = [];
    public List<ContaSaldoDto> CartoesCredito { get; set; } = [];
}

public class ContaSaldoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NumBanco { get; set; } = string.Empty;
    public string NomeBanco { get; set; } = string.Empty;
    public TipoConta TipoConta { get; set; }
    public string TipoContaDescricao { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;
    public string? Agencia { get; set; }
    public string? NumConta { get; set; }
    public bool Ativa { get; set; }

    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public decimal SaldoProjetado { get; set; }
    public int LancamentosPendentesAteCorte { get; set; }
}