namespace ControleFinanceiro.Application.DTOs;

public class CriarContaDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
}

public class ContaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
}