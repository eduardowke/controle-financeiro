namespace ControleFinanceiro.Application.DTOs;

public class ExtratoContaDto
{
    public ContaResponseDto Conta { get; set; } = null!;
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public decimal SaldoProjetado { get; set; }
    public List<TransacaoResponseDto> Compensados { get; set; } = [];
    public List<TransacaoResponseDto> Futuros { get; set; } = [];
}