using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.DTOs;

public class TransacaoResponseDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public DateTime Data { get; set; }
    public DateTime? DataCompensacao { get; set; }
    public int Ordem { get; set; }
    public bool IsPago { get; set; }

    public int ContaId { get; set; }
    public string NomeConta { get; set; } = string.Empty;

    public int CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;

    public Guid? TransferenciaGrupoId { get; set; }
    public double? ConfiancaIA { get; set; }

    // Preenchido no extrato individual da conta (saldo acumulado linha a linha)
    public decimal? SaldoAposLancamento { get; set; }
}