using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.DTOs;

public class CriarTransacaoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public bool IsPago { get; set; }
    public int ContaId { get; set; }
    public int CategoriaId { get; set; }
    public double? ConfiancaIA { get; set; }
}