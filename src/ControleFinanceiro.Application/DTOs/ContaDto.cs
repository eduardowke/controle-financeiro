using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.DTOs;

public class CriarContaDto
{
    public string? Nome { get; set; } // Se vazio, o sistema gera "{NomeReduzido} - {PrimeiroNome}"
    public string NumBanco { get; set; } = string.Empty;
    public TipoConta TipoConta { get; set; }
    public string Titular { get; set; } = string.Empty;
    public string? Agencia { get; set; }
    public string? NumConta { get; set; }
    public decimal SaldoInicial { get; set; }
}

public class AtualizarContaDto
{
    public string Nome { get; set; } = string.Empty;
    public string NumBanco { get; set; } = string.Empty;
    public TipoConta TipoConta { get; set; }
    public string Titular { get; set; } = string.Empty;
    public string? Agencia { get; set; }
    public string? NumConta { get; set; }
    public decimal SaldoInicial { get; set; }
    public bool Ativa { get; set; } = true;
}

public class ContaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NumBanco { get; set; } = string.Empty;
    public string NomeBanco { get; set; } = string.Empty;
    public string? LogoBancoUrl { get; set; }
    public TipoConta TipoConta { get; set; }
    public string TipoContaDescricao { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;
    public string? Agencia { get; set; }
    public string? NumConta { get; set; }
    public decimal SaldoInicial { get; set; }
    public bool Ativa { get; set; }
}